#include <Wire.h>

#define ADDRESS 0x60 //defines address of compass

#define accelXPin 8
#define accelYPin 9
#define ledPin 13

void setup(){
  Wire.begin(); //conects I2C
  Serial.begin(9600);
  pinMode(accelXPin,INPUT);
  pinMode(accelYPin,INPUT);
  pinMode(ledPin,OUTPUT);
}

void loop(){
  byte highByte;
  byte lowByte;
  
   //Serial.println("Begin transmission");

   Wire.beginTransmission(ADDRESS);      //starts communication with cmps03
   Wire.write(2);                         //Sends the register we wish to read
   Wire.endTransmission();

   //Serial.println("Requesting");
   Wire.requestFrom(ADDRESS, 2);        //requests high byte
   
   //Serial.println("Waiting");
   while(Wire.available() < 2);         //while there is a byte to receive
   
   //Serial.println("Reading");   
   highByte = Wire.read();           //reads the byte as an integer
   lowByte = Wire.read();
   
   int bearing = ((highByte<<8)+lowByte)/10; 
   //Serial.println(bearing);
   analogWrite(ledPin,map(bearing,0,360,0,255));
   
   
   Serial.print(pulseIn(accelXPin,HIGH));
   Serial.print(",");
   Serial.print(pulseIn(accelYPin,HIGH));
   Serial.println("");
  
   delay(300);
}
