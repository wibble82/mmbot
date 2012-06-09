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

int imgdims = 1;
int compratio = 0;

void setup()
{ 
  Serial.begin(19200);
  Camera.Begin(&Serial2);
  
  //send set image dimensions command
  Serial.println("Setting size");
  Camera.SetDimensions((ECameraImageDimensions)imgdims);
  Camera.Wait(0);
  Serial.println("OK");

  //send reset command
  Serial.println("Resetting");
  Camera.Reset();
  Camera.Wait(0);
  delay(4000);                              
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
  //send set compression ratio command
  Serial.print("Setting compression to ");
  Serial.println((compratio/2)*255);
  Camera.SetCompressionRatio((compratio/2)*255);
  Camera.Wait(0);
  Serial.println("OK");

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
  /*
  //begin reading the photo
  uint8_t buffer[32];
  unsigned long address = 0;
  unsigned long amount_read;
  while(address < fsize)
  {  
    //read data
    Serial.print("Reading address ");
    Serial.println(address);
    Camera.GetContent(buffer,&amount_read,32,address);
    Camera.Wait();
    
    //print data
    Serial.print(amount_read);
    Serial.print(": ");
    for(int i = 0; i < amount_read; i++)
      printbyte(buffer[i]);
    Serial.println("");
    
    address += amount_read;
  }      
  */
  Serial.println("Stopping take picture");
  Camera.StopTakingPicture();
  Camera.Wait();
  Serial.println("OK");
  
  //imgdims = (imgdims+1)%3;
  compratio = (compratio+1)%4;
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









