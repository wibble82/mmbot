/*
JPEG Camera Example Sketch
The sketch will take a picture on the JPEG Serial Camera and store the jpeg to an SD card
on an SD Shield
Written by Ryan Owens
SparkFun Electronics

Hardware Notes:
This sketch assumes the arduino has the microSD shield from SparkFun attached.
The camera Rx/Tx should be attached to pins 2 and 3.
IMPORTANT: The JPEG camera requires a TTL level shifter between the camera output
and the arduino. Bypassing this may damage the Arduino pins.
*/

//This example requires the MemoryCard, SdFat, JPEGCamera and NewSoftSerial libraries
#include <SoftwareSerial.h>
#include <MemoryCard.h>
#include <SdFat.h>
#include <JPEGCamera.h>

//Create an instance of the camera
JPEGCamera camera;

  char response[512];
void setup()
{
    //Setup the camera, serial port and memory card
    Serial.begin(115200);
    Serial.println("Hello");
    
    Serial.println("Begin camera");
    camera.begin();
    

    //camera.setImageSize(2,response);
    //delay(500);
    
    //camera.setImageQuality(10,response);
    //delay(500);
    
    //Reset the camera
    Serial.println("Resetting camera");
    camera.reset(response);
    delay(3000);
    while(Serial2.available())
    {
      Serial.write(Serial2.read());
    }
    
        /*Serial2.flush();
    
	Serial2.write(uint8_t(0x56));
	Serial2.write(uint8_t(0x00));
	Serial2.write(uint8_t(0x24));
	Serial2.write(uint8_t(0x03));
	Serial2.write(uint8_t(0x01));
	Serial2.write(uint8_t(0x0D));
	Serial2.write(uint8_t(0xA6));

       for(int i = 0; i < 5; i++)
       {
         while(!Serial2.available());
         Serial.println(int(Serial2.read()),HEX);
       }
         
	Serial2.end();
	Serial2.begin(115200);*/

     delay(3000);    
}

void loop()
{
    Serial.println("Begin");
    
    unsigned int count=0;
    int size=0;
    int address=0;
    int eof=0;
  
    //while(Serial.available() == 0)
    //  delay(10);
      
    Serial.println("Received start command - draining serial");  
      
    while(Serial.available() != 0)
      Serial.read();
    
    //Take a picture
    Serial.println("Taking picture");
    camera.takePicture(response);
    delay(2000);
    Serial.println("");
    
    //Get + print the size of the picture
    Serial.println("Size");
    count = camera.getSize(response, &size);
    Serial.println(size);
    Serial.println("");
    
    unsigned long timereading = 0;
    unsigned long timewriting = 0;
    unsigned long timeprocessing = 0;
    
    Serial.println("Data");
    
    //request the first read
    unsigned long readstart = micros();
    camera.beginReadData(address);
    timereading += (micros()-readstart);
    
    //keep on reading
    while(address < size)
    {               
       //pull the data out that we requested earlier
        Serial.println("Ending read data");
        readstart = micros();
        count=camera.endReadData(response);
        timereading += (micros()-readstart);
                
        //increment the current address by the number of bytes we read
        address+=count;
        
        //request the next read if not at eof
        Serial.println("Starting read data");
        readstart = micros();
        if(address < size)
          camera.beginReadData(address);
        timereading += (micros()-readstart);    
        
        //send the next write while the read is being done
        Serial.println("Printing data");
        unsigned long writestart = micros();
        //Serial.write((uint8_t*)response,count);
        timewriting += (micros()-writestart);
        
    }    
    
    camera.stopPictures(response);
    Serial.println("Done");
    Serial.print("Time reading ");
    Serial.println(timereading);
    Serial.print("Time writing ");
    Serial.println(timewriting);
    Serial.print("Time processing ");
    Serial.println(timeprocessing);
    
    
    delay(500);
}
