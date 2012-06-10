#include <SoftwareSerial.h>
#include <Camera.h>


/* Linksprite */

byte incomingbyte;
int a=0x0000,j=0,k=0,count=0;                    //Read Starting address       
uint8_t MH,ML;
boolean EndFlag=0;

CCamera Camera;
                               
void SendResetCmd();
void SendTakePhotoCmd();
void SendReadDataCmd();
void StopTakePhotoCmd();

void setup()
{ 
  Serial.begin(19200);
  Serial1.begin(115200);
  Camera.Begin(&Serial2);
  
  //send set image dimensions command
  Serial.println("Setting size");
  Camera.SetDimensions(IMG_DIMS_320x240);
  Camera.Wait(0);
  Serial.println("OK");

  //send reset command
  Serial.println("Resetting");
  Camera.Reset();
  Camera.Wait(0);
  delay(4000);                              
  Serial.println("OK");  

  //send set baud rate command
  //Serial.println("Setting baud rate");    
  //Camera.SetBaudRate(CAM_BAUD_115200);
  //Camera.Wait();
  
  //re-connect serial
  //Serial2.end();
  //Serial2.begin(115200);
   
  //send set compression ratio command
  Serial.print("Setting compression to ");
  Serial.println(128);
  Camera.SetCompressionRatio(128);
  Camera.Wait(0);
  Serial.println("OK");  
}

void printbyte(uint8_t b)
{
  Serial.print("0x");
  if(b < 0x10)
    Serial.print("0");
  Serial.print(b,HEX);
  Serial.print(" ");
}

void loop() 
{
  //send take photo command
  Serial.println("Starting take picture");
  Camera.StartTakingPicture();
  Camera.Wait();
  Serial.println("OK");
  
  //send get file size command
  Serial.println("Getting size");
  unsigned long fsize;
  Camera.GetFileSize(&fsize);
  Camera.Wait();
  Serial.println(fsize);
  
  //begin reading the photo
  uint8_t buffer[32];
  unsigned long address = 0;
  unsigned long amount_read;
  Serial.println("{");
  unsigned long start = millis();
  Camera.ResetPerfMons();
  while(address < fsize)
  {  
    //read data
    Camera.GetContent(buffer,&amount_read,32,address);
    Camera.Wait();
    
    //print data
    for(int i = 0; i < amount_read; i++)
    {
      printbyte(buffer[i]);
      Serial.print(",");
    }
    Serial.println("");
    delay(100);
    
    address += amount_read;
  }      
  unsigned long taken = millis()-start;
  Serial.println("}");
  Serial.print("Read time: ");
  Serial.println(taken);
  Serial.println(Camera.GetSendPerfMon()/1000);
  Serial.println(Camera.GetRecvPerfMon()/1000);
  Serial.println(Camera.GetWaitPerfMon()/1000);
  
  Serial.println("Stopping take picture");
  Camera.StopTakingPicture();
  Camera.Wait();
  Serial.println("OK");
  
  delay(100);
}

//Send Reset command
void SendResetCmd()
{
      Serial2.write((uint8_t)0x56);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x26);
      Serial2.write((uint8_t)0x00);
      
}

//Send take picture command
void SendTakePhotoCmd()
{
      Serial2.write((uint8_t)0x56);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x36);
      Serial2.write((uint8_t)0x01);
      Serial2.write((uint8_t)0x00);  
}

//Read data
void SendReadDataCmd()
{
      MH=a/0x100;
      ML=a%0x100;
      Serial2.write((uint8_t)0x56);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x32);
      Serial2.write((uint8_t)0x0c);
      Serial2.write((uint8_t)0x00); 
      Serial2.write((uint8_t)0x0a);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)MH);
      Serial2.write((uint8_t)ML);   
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x20);
      Serial2.write((uint8_t)0x00);  
      Serial2.write((uint8_t)0x0a);
      a+=0x20;                            //address increases 32£¬set according to buffer size
}

void StopTakePhotoCmd()
{
      Serial2.write((uint8_t)0x56);
      Serial2.write((uint8_t)0x00);
      Serial2.write((uint8_t)0x36);
      Serial2.write((uint8_t)0x01);
      Serial2.write((uint8_t)0x03);        
}









