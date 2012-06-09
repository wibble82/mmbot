/* Linksprite */

#include <SoftwareSerial.h>

byte incomingbyte;
SoftwareSerial mySerial(2,3);                     //Configure pin 4 and 5 as soft serial port
int a=0x0000,j=0,k=0,count=0;                    //Read Starting address       
uint8_t MH,ML;
boolean EndFlag=0;
                               
void SendResetCmd();
void SendTakePhotoCmd();
void SendReadDataCmd();
void StopTakePhotoCmd();

void setup()
{ 
  Serial.begin(19200);
  mySerial.begin(38400);
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
  //send reset command
  SendResetCmd();
  delay(4000);                              
 
  //send take photo command
  Serial.println("Taking photo");
  SendTakePhotoCmd();

  //drain any bytes out of the serial buffer that will have come back from the previous 2 commands
  while(mySerial.available()>0)
  {
    incomingbyte=mySerial.read();
    printbyte(incomingbyte);
  }   
  Serial.println();
  
  //begin reading the photo
  Serial.println("Reading data");
  while(!EndFlag)
  {  
    //send command to read data
    SendReadDataCmd();
    
    //delay until correct amount of data is here
    while(mySerial.available() < 37);
    
    //read whole response (5 byte response+32 byte data) into 'all' buffer
    int pos=0;
    int count=0;
    byte all[64];
    while(mySerial.available()>0)
    {
      //get response
      all[pos]=mySerial.read();
      
      //if within the actual data range
      if((pos>5)&&(pos<37)&&(!EndFlag))
      {
        //check for EOF marker (0xFF followed by 0xD9)
        if((all[pos-1]==0xFF)&&(all[pos]==0xD9))      //Check if the picture is over
          EndFlag=1;                           
        //increment bytes read count
        count++;
      }
      
      //move forwards 1 byte
      pos++;
    }
    
    //check we actually got data
    if(pos > 0)
    {
      //print out the response bytes (should always be 0x76 0x00 0x32 0x00 0x00)
      for(j=0; j < 5; j++)
      {
         printbyte(all[j]);
      }
      Serial.println();
      
      //print out the data bytes
      for(j=0;j<count;j++)
      {   
         printbyte(all[j+5]);
      }                                       //Send jpeg picture over the serial port
      Serial.println();
    }
    else
    {
      //if didn't get anything, print a warning
      Serial.println("No data!");
    }
  }      
  while(1);
}

//Send Reset command
void SendResetCmd()
{
      mySerial.write((uint8_t)0x56);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x26);
      mySerial.write((uint8_t)0x00);
}

//Send take picture command
void SendTakePhotoCmd()
{
      mySerial.write((uint8_t)0x56);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x36);
      mySerial.write((uint8_t)0x01);
      mySerial.write((uint8_t)0x00);  
}

//Read data
void SendReadDataCmd()
{
      MH=a/0x100;
      ML=a%0x100;
      mySerial.write((uint8_t)0x56);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x32);
      mySerial.write((uint8_t)0x0c);
      mySerial.write((uint8_t)0x00); 
      mySerial.write((uint8_t)0x0a);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)MH);
      mySerial.write((uint8_t)ML);   
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x20);
      mySerial.write((uint8_t)0x00);  
      mySerial.write((uint8_t)0x0a);
      a+=0x20;                            //address increases 32£¬set according to buffer size
}

void StopTakePhotoCmd()
{
      mySerial.write((uint8_t)0x56);
      mySerial.write((uint8_t)0x00);
      mySerial.write((uint8_t)0x36);
      mySerial.write((uint8_t)0x01);
      mySerial.write((uint8_t)0x03);        
}









