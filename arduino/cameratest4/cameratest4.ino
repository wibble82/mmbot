/* Linksprite */

byte incomingbyte;
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
  Serial2.begin(38400);
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
  while(Serial2.available()>0)
  {
    incomingbyte=Serial2.read();
    printbyte(incomingbyte);
  }   
  Serial.println();
 
  //send take photo command
  Serial.println("Taking photo");
  SendTakePhotoCmd();
  delay(1000);

  //drain any bytes out of the serial buffer that will have come back from the previous 2 commands
  while(Serial2.available()>0)
  {
    incomingbyte=Serial2.read();
    printbyte(incomingbyte);
  }   
  Serial.println();
  
  //begin reading the photo
  Serial.println("Reading data");
  Serial.println("{");
  while(!EndFlag)
  {  
    //send command to read data
    SendReadDataCmd();
        
    //delay until correct amount of data is here
    unsigned long timems = millis();
    while(Serial2.available() < 42)
    {
      if((millis()-timems) > 1000)
      {
        Serial.print("Been waiting for data for ages now, only got ");
        Serial.print(Serial2.available());
        Serial.print(" current address 0x");
        Serial.print(a,HEX);
        Serial.println("");
        while(Serial2.available())
          printbyte(Serial2.read());
        Serial.println("");
        timems = millis();
      }
    }
    
    //read whole response (5 byte response+32 byte data) into 'all' buffer
    int pos=0;
    int count=0;
    byte all[64];
    while(Serial2.available()>0)
    {
      //get response
      all[pos]=Serial2.read();
      
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
      if(pos != 42 || all[0] != 0x76 || all[1] != 0x00 || all[2] != 0x32 || all[3] != 0x00 || all[4] != 0x00)
      {
        Serial.print("Erroneous data received at 0x");
        Serial.println(a,HEX);
        Serial.print(pos);
        Serial.println(" bytes read");
        for(j=0;j<5;j++)
        {   
           printbyte(all[j]);
        }                                       //Send jpeg picture over the serial port
        Serial.println();
        for(j=5;j<pos;j++)
        {   
           printbyte(all[j]);
        }                                       //Send jpeg picture over the serial port
        Serial.println();
      }
      else
      {
        for(j=5;j<37;j++)
        {   
           printbyte(all[j]);
           Serial.print(",");
        }        //Send jpeg picture over the serial port       
        Serial.println("");
      }
    }
    else
    {
      //if didn't get anything, print a warning
      Serial.println("No data!");
    }
  }      
  Serial.println("}");
  Serial.println("Done");
  while(1);
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









