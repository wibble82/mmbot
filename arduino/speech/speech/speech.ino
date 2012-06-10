#include "SpeakJet.h"
#include "SoftwareSerial.h"
#include <Wire.h>

#define rxPin 7
#define txPin 5
#define speakingPin 6
#define switchPin 12
#define ledPin 13
#define motionPin 11

#define compassAddress 0x60 //defines address of compass

#define ultraSoundPin 10

// Create a new software serial port object called "speakJet"
CSpeakJet speakjet = CSpeakJet(rxPin,txPin,speakingPin);

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
  pinMode(switchPin, INPUT);  
  pinMode(motionPin, INPUT);  
  Serial.begin(9600);
  
  // Configure software serial port pins for SpeakJet
  pinMode(rxPin, INPUT);
  pinMode(txPin, OUTPUT);
  pinMode(speakingPin, INPUT);
  
  speakjet.Begin();
  speakjet.SetVolume(96);
  speakjet.SetSpeed(114);
  speakjet.SetPitch(100);
  speakjet.SetBend(5);

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

boolean waitmotion = true;
int nxt = SPEECH_HELLO;
int number = 0;

/**
 */
void loop()
{ 
  //Serial.println(cm);
  
  if(digitalRead(switchPin) == HIGH)
  {
    int dist = readUltraSound();
    int bearing = readCompass();
        
    digitalWrite(ledPin, HIGH);
    
    speakjet.SetVolume(127);
    speakjet.SetSpeed(100);
    speakjet.SetBend(3);
    speakjet.SetPitch(110);
  
    speakjet.Play(SPEECH_DISTANCE, false);
    speakjet.Play(SPEECH_IS, false);
    speakjet.PlayNumber(dist,true);
    delay(1000);
    
    speakjet.Play(SPEECH_ANGLE, false);
    speakjet.Play(SPEECH_IS, false);
    speakjet.PlayNumber(bearing,true);
    delay(1000);
    
    digitalWrite(ledPin, LOW);
    number++;
  }
  
  if(waitmotion)
  {
    if(digitalRead(motionPin) == LOW)
      return;
    waitmotion = false;
  }
  else
  {
    if(digitalRead(motionPin) == HIGH)
      return;
     waitmotion = true;
     return;
  }
 
  speakjet.SetVolume(96);
  speakjet.SetSpeed(114);
  speakjet.SetBend(100);
  
  int pitch = map(constrain(readUltraSound(), 1, 100), 1, 100, 255, 150);
  speakjet.SetPitch(pitch);
  
  speakjet.Play(SPEECH_I, false);  
  speakjet.Play(SPEECH_CAN, false);  
  speakjet.Play(SPEECH_SEE, false);  
 speakjet.Play(SPEECH_YOU, true);  

}
