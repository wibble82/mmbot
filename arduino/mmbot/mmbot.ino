#include <SoftwareSerial.h>
#include <Camera.h>
#include <Servo.h>

#define PICTURE_STREAM_SIZE 1024

class CCameraStream
{
public:
    CCamera* Cam; 
   
    int CameraStage;
    byte PictureBuffer[PICTURE_STREAM_SIZE+32];
    unsigned long PictureSize;
    unsigned long PictureAmountRead;
    unsigned long PictureReadAddress;
    unsigned long PictureSendAddress;
    unsigned long PictureStreamUsed;
    unsigned long PictureStreamWrite;
    unsigned long PictureStreamRead;
    
    CCameraStream()
    {
      CameraStage = 0;
      PictureSize = 0;
      PictureAmountRead = 0;
      PictureReadAddress = 0;
      PictureSendAddress = 0;
      PictureStreamUsed = 0;
      PictureStreamWrite = 0;
      PictureStreamRead = 0;
    }
    
    void UpdateCamera()
    {
      switch(CameraStage)
      {
        case 0:
          //idle - i.e. nothing requested
          break;
        case 1:
          //remote has requested a picture be taken, so tell camera to begin taking picture
          Serial.println("Requesting camera take picture");
          Cam->StartTakingPicture();
          CameraStage++;
          //fall through
        case 2:
          //wait for camera response
          if(!Cam->Update())
            break;
          Serial.println("Camera picture taken");
          CameraStage++;
          //fall through
        case 3:
          //request the file size
          Serial.println("Requesting camera file size");
          Cam->GetFileSize(&PictureSize);
          CameraStage++;
          //fall through
        case 4:
          //wait for camera response
          if(!Cam->Update())
            break;
          Serial.print("Camera file size received: ");
          Serial.println(PictureSize);
          CameraStage++;
          //fall through
        case 5:
          //this is the main content bit
          //first, we bail out if we haven't yet sent the remote the data that is in the picture buffer
          if(PictureStreamUsed >= PICTURE_STREAM_SIZE)
            break;
          //next, we check if we have reached the end of the photo
          if(PictureReadAddress >= PictureSize)
          {
            //reached the end, so jump to stage 7 (where we stop taking the picture)
            CameraStage=7;
            break;
          }  
          //need more data, so begin getting content
          //Serial.print("Requesting 32 bytes to camera stream at ");
          //Serial.print(PictureStreamWrite);
          //Serial.println("");
          Cam->GetContent(PictureBuffer+PictureStreamWrite,&PictureAmountRead,32,PictureReadAddress);
          CameraStage++;
           //fall through
        case 6:
          //wait for camera response
          if(!Cam->Update())
            break;
          //got response, so increment the read address and loop back to stage 5
          /*if( (PictureAmountRead%32) != 0)
          {
            Serial.print("Received ");
            Serial.print(PictureAmountRead);
            Serial.print(" bytes to camera stream at ");
            Serial.print(PictureStreamWrite);
          }*/
          PictureStreamWrite = (PictureStreamWrite + PictureAmountRead) % PICTURE_STREAM_SIZE;
          PictureStreamUsed += PictureAmountRead;
          PictureReadAddress += PictureAmountRead;
          /*if( (PictureAmountRead%32) != 0)
          {
            Serial.print(" total read ");
            Serial.print(PictureReadAddress);
            Serial.print(" of ");
            Serial.print(PictureSize);
            Serial.println("");
          }*/
          CameraStage=5;
          break;
        case 7:
          //all done, so tell camera to stop taking picture
          Cam->StopTakingPicture();
          CameraStage++;
          //fall through
        case 8:
          //wait for camera
          if(!Cam->Update())
            break;    
          CameraStage++;
          //fall through
        case 9:
          //finally, wait for remote to drain the camera stream
          if(PictureSendAddress < PictureSize)
            break;
          //reset camera state
          CameraStage=0;
          PictureSize = 0;
          PictureAmountRead = 0;
          PictureReadAddress = 0;
          PictureSendAddress = 0;
          PictureStreamWrite = 0;
          PictureStreamRead = 0;
          PictureStreamUsed = 0;
          //fall through
        default:
          //anything that hits here just goes back to stage 0
          CameraStage = 0;
          break;  
      }
    }  
};

//PIN Definitions
#define PIN_MOTOR_LEFT 22
#define PIN_MOTOR_RIGHT 23
#define SENSOR_SERVO_PIN_EYE 4
#define SENSOR_SERVO_PIN_X 2
#define SENSOR_SERVO_PIN_Y 3
#define PIN_ULTRASOUND 10
#define PIN_MOTIONSENSOR 9

//Servos to control head movement
Servo SensorServoX;
Servo SensorServoY;
Servo EyeServo;
CCamera Camera1;
CCamera Camera2;
CCameraStream CS1;
CCameraStream CS2;

//current targets for servos
int ServoCurrentX = 90;
int ServoCurrentY = 90;
int EyeCurrentY = 90;
int ServoTargetX = 90;
int ServoTargetY = 90;
int EyeTarget = 90;

//time remaining that left/right motors should be active
int LeftMotorActiveTimer = 0;
int RightMotorActiveTimer = 0;

//serial source (so I can switch between usb/bluetooth)
HardwareSerial* Comms;

///////////////////////////////////////////////////////////////////
//converts ultrasound timer to centimeters
///////////////////////////////////////////////////////////////////
long microsecondsToCentimeters(long microseconds)
{
  return microseconds / 29 / 2;
}

///////////////////////////////////////////////////////////////////
//reads ultrasound distance sensor
///////////////////////////////////////////////////////////////////
int readUltraSound()
{
  pinMode(PIN_ULTRASOUND, OUTPUT);
  digitalWrite(PIN_ULTRASOUND, HIGH);
  delayMicroseconds(5);
  digitalWrite(PIN_ULTRASOUND, LOW);
  pinMode(PIN_ULTRASOUND, INPUT); 
  return microsecondsToCentimeters(pulseIn(PIN_ULTRASOUND, HIGH));  
}

///////////////////////////////////////////////////////////////////
//reads motion sensor
///////////////////////////////////////////////////////////////////
int readMotion()
{
  return digitalRead(PIN_MOTIONSENSOR);
}

///////////////////////////////////////////////////////////////////
//init
///////////////////////////////////////////////////////////////////
void setup()  
{
  //init pins
  pinMode(PIN_MOTOR_LEFT,OUTPUT);
  pinMode(PIN_MOTOR_RIGHT,OUTPUT);
  pinMode(PIN_ULTRASOUND,INPUT);
  pinMode(PIN_MOTIONSENSOR,INPUT);
  
  //set default pin values
  digitalWrite(PIN_MOTOR_LEFT,LOW);
  digitalWrite(PIN_MOTOR_RIGHT,LOW);
  
  //init blue tooth serial port
  Serial1.begin(115200);
  
  //init serial port for debugging and print message  
  Serial.begin(115200);
  
  //init camera at 38400
  Camera1.Begin(&Serial2,38400);
  Camera2.Begin(&Serial3,38400);
  
  //set serial communications to be via main serial connection
  Comms = &Serial1;
  Serial.println("Hello");
  
  //setup the sensor servos
  SensorServoX.attach(SENSOR_SERVO_PIN_X);
  SensorServoY.attach(SENSOR_SERVO_PIN_Y);
  EyeServo.attach(SENSOR_SERVO_PIN_EYE);
  SensorServoX.write(ServoTargetX);
  SensorServoY.write(ServoTargetY);
  EyeServo.write(EyeTarget);
  
  //send set image dimensions command
  Serial.println("Setting  camera size");
  Camera1.SetDimensions(IMG_DIMS_320x240);
  Camera2.SetDimensions(IMG_DIMS_320x240);
  Camera1.Wait(0);
  Camera2.Wait(0);
  Serial.println("OK");

  //send reset command
  Serial.println("Resetting camera");
  Camera1.Reset();
  Camera2.Reset();
  Camera1.Wait(0);
  Camera2.Wait(0);
  delay(4000);                              
  Serial.println("OK");  
   
  //send set compression ratio command
  Serial.print("Setting camera compression to ");
  Serial.println(255);
  Camera1.SetCompressionRatio(255);
  Camera2.SetCompressionRatio(255);
  Camera1.Wait(0);
  Camera2.Wait(0);
  Serial.println("OK");      
  
  //set baud rate
  /*Serial.println("Setting baud rate");
  Camera.SetBaudRate(CAM_BAUD_115200);
  Camera.Wait(0);
  Serial2.end();
  Camera.Begin(&Serial2,115200);
  Serial.println("OK");  */
  CS1.Cam = &Camera1;
  CS2.Cam = &Camera2;
}

//commands that can be received (advanced mode)
enum ECommands
{
  COMMAND_KEEPALIVE,
  COMMAND_PING,
  COMMAND_MOTION,
  COMMAND_COMPASS,
  COMMAND_SAY,
  COMMAND_SET_SENSOR_POSITION,
  COMMAND_FORWARDS,
  COMMAND_LEFT,
  COMMAND_RIGHT,
  COMMAND_HORIZONTAL_SENSOR_SWEEP,
  COMMAND_START_PHOTO_LEFT,
  COMMAND_PHOTO_SIZE_LEFT,
  COMMAND_PHOTO_DATA_LEFT,
  COMMAND_START_PHOTO_RIGHT,
  COMMAND_PHOTO_SIZE_RIGHT,
  COMMAND_PHOTO_DATA_RIGHT,
  TOTAL_COMMANDS,
  COMMAND_MODE = 'T'
};

//states robot can be in (advanced mode)
enum EState
{
  STATE_IDLE,
  TOTAL_STATES
};

int GMode = 0; //current mode (simple=0, advanced=1)
ECommands GCurrentCommand = TOTAL_COMMANDS; //command currently being read
EState GCurrentState = STATE_IDLE; //current state

///////////////////////////////////////////////////////////////////
//read 2 bytes from blue tooth and encode as little endian integer
///////////////////////////////////////////////////////////////////
int ReadWord()
{
  return Comms->read() | (Comms->read() << 8);
}

///////////////////////////////////////////////////////////////////
//read 1 bytes from blue tooth and encode as little endian integer
///////////////////////////////////////////////////////////////////
int ReadByte()
{
  return Comms->read();
}

///////////////////////////////////////////////////////////////////
//write 2 bytes to blue tooth as little endian integer
///////////////////////////////////////////////////////////////////
void WriteWord(int data)
{
    byte highbyte = data >> 8;
    byte lowbyte = data & 0xff;
    Comms->write(lowbyte);
    Comms->write(highbyte);
}

///////////////////////////////////////////////////////////////////
//updates the servos and motors
///////////////////////////////////////////////////////////////////
void UpdateMotorTargets()
{
  SensorServoX.write(ServoTargetX);
  SensorServoY.write(ServoTargetY);
  EyeServo.write(EyeTarget);
  
  if(LeftMotorActiveTimer > 0)
  {
    digitalWrite(PIN_MOTOR_LEFT,HIGH);
    LeftMotorActiveTimer--;
  }
  else
  {
    digitalWrite(PIN_MOTOR_LEFT,LOW);
  }
  
  if(RightMotorActiveTimer > 0)
  {
    digitalWrite(PIN_MOTOR_RIGHT,HIGH);
    RightMotorActiveTimer--;
  }
  else
  {
    digitalWrite(PIN_MOTOR_RIGHT,LOW);
  }  

}


///////////////////////////////////////////////////////////////////
//reads commands from pc
///////////////////////////////////////////////////////////////////
int FrameCounter = 0;
void ProcessCommands()
{
  //if not currently reading a command, check if there is one available to be read
  if(GCurrentCommand == TOTAL_COMMANDS)
  {
    if(Comms->available() >= 1)
    {
      //read the new command
      GCurrentCommand = (ECommands)ReadByte();
    }
  }
  
  //will now execute the command, assuming there's enough data available
  switch(GCurrentCommand)
  {
    case COMMAND_KEEPALIVE:
      {
        //reply with keep alive response
        WriteWord(1);
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_FORWARDS:
      {
        //move forwards and return 1
        LeftMotorActiveTimer = 10;
        RightMotorActiveTimer = 10;        
        WriteWord(1);
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_LEFT:
      {
        //turn left and return 1
        LeftMotorActiveTimer = 10;
        RightMotorActiveTimer = 0;
        WriteWord(1);
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_RIGHT:
      {
        //turn right and return 1
        LeftMotorActiveTimer = 0;
        RightMotorActiveTimer = 10;
        WriteWord(1);
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
    
    case COMMAND_PING:
      {
        //read ultra sound and return value
        WriteWord(readUltraSound());
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_MOTION:
      {
        //read motion sensor and return 0 or 1
        WriteWord(readMotion());
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_SET_SENSOR_POSITION:
      {
        //sets sensor positions - requires 2 integers so doesn't execute until 4 bytes are available
        if(Comms->available() >= 6)
        {
          ServoTargetX = ReadWord();
          ServoTargetY = ReadWord();
          EyeTarget = ReadWord();
          Serial.print("Set Sensor positions: ");
          Serial.print(ServoTargetX); Serial.print(" ");
          Serial.print(ServoTargetY); Serial.print(" ");
          Serial.println(EyeTarget);
          GCurrentCommand = TOTAL_COMMANDS;
        }
        break;
      }
      
    case COMMAND_MODE:
      {
        //switches back to simple mode
        GMode = 0;
        GCurrentCommand = TOTAL_COMMANDS;
        Comms->println("SM");    
        break;    
      }
      
    case COMMAND_START_PHOTO_LEFT:
      {
        if(CS1.CameraStage == 0)
        {
          CS1.CameraStage = 1;
          WriteWord(1);
        }
        else
        {
          WriteWord(0);
        }
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_PHOTO_SIZE_LEFT:
      {
        if(CS1.CameraStage >= 5)
        {
          WriteWord(CS1.PictureSize);
        }
        else
        {
          WriteWord(0);
        }
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;

    case COMMAND_PHOTO_DATA_LEFT:
      {
        //check if there's any data available
        if(CS1.PictureStreamUsed > 0)
        {
          //got data, so work out how much to send, up to a maximum of (currently) 32 bytes
          int bytes_to_send = min(CS1.PictureStreamUsed,256);
          
          //clamp the amount to avoid overrunning the end of the stream buffer
          if( (CS1.PictureStreamRead+bytes_to_send) > PICTURE_STREAM_SIZE )
          {
            bytes_to_send = PICTURE_STREAM_SIZE-CS1.PictureStreamRead;
          }
          
          //clamp the amount to discard extra bytes at the end of the image
          if( (CS1.PictureSendAddress+bytes_to_send) > CS1.PictureSize )
          {
            bytes_to_send = CS1.PictureSize - CS1.PictureSendAddress;
          }
           
          //write out number of bytes to send, followed by the actual data
          WriteWord(bytes_to_send);
          Comms->write(CS1.PictureBuffer+CS1.PictureStreamRead,bytes_to_send);
          Serial.print("L[");
          Serial.print(CS1.PictureSendAddress);
          Serial.print("] sz=");
          Serial.print(bytes_to_send);
          Serial.print(", strm=");
          Serial.print(CS1.PictureStreamUsed);
          Serial.println("");
          //update stream position
          CS1.PictureStreamRead = (CS1.PictureStreamRead+bytes_to_send)%PICTURE_STREAM_SIZE;
          CS1.PictureStreamUsed -= bytes_to_send;
          CS1.PictureSendAddress += bytes_to_send;
        }
        else
        {
          //no data, so write '0' (to indicate 0 bytes)
          WriteWord(0);
        }
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_START_PHOTO_RIGHT:
      {
        if(CS2.CameraStage == 0)
        {
          CS2.CameraStage = 1;
          WriteWord(1);
        }
        else
        {
          WriteWord(0);
        }
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;
      
    case COMMAND_PHOTO_SIZE_RIGHT:
      {
        if(CS2.CameraStage >= 5)
        {
          WriteWord(CS2.PictureSize);
        }
        else
        {
          WriteWord(0);
        }
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;

    case COMMAND_PHOTO_DATA_RIGHT:
      {
        if(CS2.PictureStreamUsed > 0)
        {
          int bytes_to_send = min(CS2.PictureStreamUsed,256);
          if( (CS2.PictureStreamRead+bytes_to_send) > PICTURE_STREAM_SIZE )
          {
            bytes_to_send = PICTURE_STREAM_SIZE-CS2.PictureStreamRead;
          }
           
          //clamp the amount to discard extra bytes at the end of the image
          if( (CS2.PictureSendAddress+bytes_to_send) > CS2.PictureSize )
          {
            bytes_to_send = CS2.PictureSize - CS2.PictureSendAddress;
          }
           
          WriteWord(bytes_to_send);
          Comms->write(CS2.PictureBuffer+CS2.PictureStreamRead,bytes_to_send);
          Serial.print("R[");
          Serial.print(CS2.PictureSendAddress);
          Serial.print("] sz=");
          Serial.print(bytes_to_send);
          Serial.print(", strm=");
          Serial.print(CS2.PictureStreamUsed);
          Serial.println("");
          CS2.PictureStreamRead = (CS2.PictureStreamRead+bytes_to_send)%PICTURE_STREAM_SIZE;
          CS2.PictureStreamUsed -= bytes_to_send;
          CS2.PictureSendAddress += bytes_to_send;
        }
        else
        {
          WriteWord(0);
        }
        GCurrentCommand = TOTAL_COMMANDS;
      }
      break;

      //none of these do anything yet
    case COMMAND_COMPASS:
    case COMMAND_SAY:
    case COMMAND_HORIZONTAL_SENSOR_SWEEP:
    default:
      GCurrentCommand = TOTAL_COMMANDS;
      break;
  }   
}

///////////////////////////////////////////////////////////////////
//Main loop in advanced mode
///////////////////////////////////////////////////////////////////
void LoopAdvancedMode()
{
  //read incoming commands
  ProcessCommands();
  
  //do state specific logic
  switch(GCurrentState)
  {
    default:
      break;
  }

  //update the motors
  UpdateMotorTargets();
  
  //update the camera
  CS1.UpdateCamera();
  CS2.UpdateCamera();
}

///////////////////////////////////////////////////////////////////
//Main loop in simple (text) mode
///////////////////////////////////////////////////////////////////
void LoopSimpleMode()
{
  while(Comms->available())
  {
    int val = Comms->read();

    if(val == '8')
    {
      LeftMotorActiveTimer = 10;
      RightMotorActiveTimer = 10;
    }
    else if(val == '4')
    {
      LeftMotorActiveTimer = 10;
      RightMotorActiveTimer = 0;
    }
    else if(val == '6')
    {
      LeftMotorActiveTimer = 0;
      RightMotorActiveTimer = 10;
    }
    else if(val == 'a')
    {
      ServoTargetX--;
      Comms->println(ServoTargetX);
    }
    else if(val == 'd')
    {
      ServoTargetX++;
      Comms->println(ServoTargetX);
    }
    else if(val == 'w')
    {
      ServoTargetY--;
      Comms->println(ServoTargetY);
    }
    else if(val == 's')
    {
      ServoTargetY++;
      Comms->println(ServoTargetY);
    }
    else if(val == 'q')
    {
      EyeTarget--;
      Comms->println(EyeTarget);
    }
    else if(val == 'e')
    {
      EyeTarget++;
      Comms->println(EyeTarget);
    }
    else if(val == ' ')
    {
      Comms->println(readUltraSound());
    }
    else if(val == 'm')
    {
      Comms->println(readMotion());
    }
    else if(val == 'W' || val == 'S')
    {
      Comms->println(ServoTargetX);
    }
    else if(val == 'A' || val == 'D')
    {
      Comms->println(ServoTargetY);
    }
    else if(val == 'T')
    {
      GMode = 1;
      Comms->println("AM");
    }   
          
    if(GMode == 0)
      Comms->write(val);
  }
  
  UpdateMotorTargets();
  
  delay(10);  
}

///////////////////////////////////////////////////////////////////
//Main loop - just calls simple or advanced version
///////////////////////////////////////////////////////////////////
void loop()
{
  if(GMode == 0)
  {
    LoopSimpleMode();
  }
  else
  {
    LoopAdvancedMode();
  }
}


