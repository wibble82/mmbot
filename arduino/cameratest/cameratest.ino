#include <JPEGCamera.h>
#include <SoftwareSerial.h>

char response[512];
unsigned int count=0;
int size=0;
int address=0;
int eof=0;
JPEGCamera camera;

void setup()
{
     //Setup the camera and serial port
    Serial.begin(9600);
    Serial.println("Hello");
    
    camera.begin();
    camera.reset(response);
    delay(3000);
    
    //wait for pc
    while(!Serial.available());
    while(Serial.available()) Serial.read();
  
    //take a picture
    Serial.println("Taking picture");
    camera.takePicture(response);
    delay(2000);
    Serial.println("");

    //Get + print the size of the picture
    Serial.println("Size");
    count = camera.getSize(response, &size);
    Serial.println(size);
    Serial.println("");
       
    //Get + print data
    Serial.println("Data");
    while(address < size)
    {               
	//pull the data out that we requested earlier
        count=camera.readData(response,address);
        Serial.flush();
        for(int i = 0; i < count; i++)
        {
	  Serial.write(uint8_t(response[i]));
	}
        address+=count;
        delay(25);
    }
    
}

void loop()
{
}
