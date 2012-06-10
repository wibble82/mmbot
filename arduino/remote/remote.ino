#include <Servo.h>
#include <SoftwareSerial.h>
#include <SpeakJet.h>
#include <Wire.h>

#define speechRxPin 7  //digital
#define speechTxPin 4  //digital
#define speechSpeakingPin 8
#define bluToothTxPin 12         //digital
#define bluToothRxPin 11         //digital
#define ledPin 13            //digital
#define motionPin 2          //digital
#define motorAPin 5
#define motorBPin 6
#define servoAPin 3
#define servoBPin 9
#define ultraSoundPin 10


#define compassAddress 0x60 //defines address of compass


// Create a new software serial port object called "speakJet"
CSpeakJet speakjet = CSpeakJet(speechRxPin,speechTxPin,speechSpeakingPin);

Servo sensorservo;
Servo sensorservo2;
SoftwareSerial bluTooth(bluToothRxPin,bluToothTxPin);

long microsecondsToCentimeters(long microseconds)
{
  // The speed of sound is 340 m/s or 29 microseconds per centimeter.
  // The ping travels out and back, so to find the distance of the
  // object we take half of the distance travelled.
  return microseconds / 29 / 2;
}

/**
 */
void setup()  
{
  Wire.begin(); //conects I2C
  pinMode(ledPin, OUTPUT);  
  pinMode(bluToothRxPin, INPUT);  
  pinMode(bluToothTxPin, OUTPUT);  
  pinMode(motionPin, INPUT);  
  pinMode(ultraSoundPin, INPUT);  
  pinMode(motorAPin, OUTPUT);
  pinMode(motorBPin, OUTPUT);
  pinMode(servoAPin, OUTPUT);
  pinMode(servoBPin, OUTPUT);
  Serial.begin(9600);
  
  Serial.println("Hello");
  
  // Configure software serial port pins for SpeakJet
  pinMode(speechRxPin, INPUT);
  pinMode(speechTxPin, OUTPUT);
  pinMode(speechSpeakingPin, INPUT);
  
  speakjet.Begin();
  speakjet.SetVolume(100);
  speakjet.SetSpeed(114);
  speakjet.SetPitch(100);
  speakjet.SetBend(5);
  speakjet.Play(SPEECH_HELLO,true);
  
  bluTooth.begin(115200);
  
  sensorservo.attach(servoAPin);
  sensorservo2.attach(servoBPin);
    sensorservo.write(90);
    sensorservo2.write(80);       

   while(readMotion())
     delay(100);
     
  delay(1000);
}

int readUltraSound()
{
  pinMode(ultraSoundPin, OUTPUT);
  digitalWrite(ultraSoundPin, HIGH);
  delayMicroseconds(5);
  digitalWrite(ultraSoundPin, LOW);
  pinMode(ultraSoundPin, INPUT);
  
  return microsecondsToCentimeters(pulseIn(ultraSoundPin, HIGH));  
}

int readCompass()
{
  byte highByte;
  byte lowByte;
  
   Serial.println("Begin compass reading");

   Wire.beginTransmission(compassAddress);      //starts communication with cmps03
   Wire.write(2);                         //Sends the register we wish to read
   Wire.endTransmission();

   //Serial.println("Requesting");
   Wire.requestFrom(compassAddress, 2);        //requests high byte
   
   //Serial.println("Waiting");
   while(Wire.available() < 2);         //while there is a byte to receive
   
   //Serial.println("Reading");   
   highByte = Wire.read();           //reads the byte as an integer
   lowByte = Wire.read();
   
   return ((highByte<<8)+lowByte)/10; 
}

int readMotion()
{
  return digitalRead(motionPin);
}

enum ECommands
{
  COMMAND_KEEPALIVE,
  COMMAND_PING,
  COMMAND_MOTION,
  COMMAND_COMPASS,
  COMMAND_SAY,
  COMMAND_SET_SENSOR_POSITION,
  COMMAND_SET_MOTOR_SPEEDS,
  COMMAND_HORIZONTAL_SENSOR_SWEEP,
  TOTAL_COMMANDS
};

void WaitForBytes(int count)
{
  while(Serial.available() < count) {}
}

int ReadWord()
{
  WaitForBytes(2);
  return Serial.read() | (Serial.read() << 8);
}

void WriteWord(int data)
{
    byte highbyte = data >> 8;
    byte lowbyte = data & 0xff;
    Serial.write(lowbyte);
    Serial.write(highbyte);
}

uint8_t speechbuffer[1024];

/**
 */
void loop()
{     
  if(bluTooth.available() > 0)
  {
    Serial.write(bluTooth.read());
  }
  
  if(Serial.available() > 0)
  {
    int command = Serial.read();
    switch(command)
    {
      case COMMAND_KEEPALIVE:
        Serial.println("Hello");
        break;
      case COMMAND_PING:
      {  
        int data = readUltraSound();
        WriteWord(data);
        break;
      }
      case COMMAND_MOTION:
      {  
        int data = readMotion();
        WriteWord(data);
        break;
      }
      case COMMAND_COMPASS:
      {  
        int data = readCompass();
        WriteWord(data);
        break;
      }
      case COMMAND_SAY:
      {
        int count = ReadWord();
        Serial.print("Say bytes: ");
        Serial.print(count);
        Serial.print(":");
        WaitForBytes(count);
        for(int i = 0; i < count; i++)
        {
          uint8_t data = Serial.read();
          speakjet.AddByte(data);
          Serial.print(data);
          Serial.print(",");
        }
        speakjet.Wait();
        Serial.println("");
        break;
      }
      case COMMAND_SET_SENSOR_POSITION:
      {
        sensorservo.write(ReadWord());
        sensorservo2.write(ReadWord());
        break;
      }
      case COMMAND_SET_MOTOR_SPEEDS:
      {
        digitalWrite(motorAPin, ReadWord() > 0 ? HIGH : LOW);
        digitalWrite(motorBPin, ReadWord() > 0 ? HIGH : LOW);
        break;
      }
      case COMMAND_HORIZONTAL_SENSOR_SWEEP:
      {
        int resolution = ReadWord();
        sensorservo.write(0);
        delay(2000);
        for(int x = 0; x <= 180; x+=resolution)
        {
          sensorservo.write(x);
          delay(50);
          WriteWord(readUltraSound());          
        }
        break;
      }
      
      default:
        break;
    }
  }
}
