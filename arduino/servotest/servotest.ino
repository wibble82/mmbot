// Sweep
// by BARRAGAN <http://barraganstudio.com> 
// This example code is in the public domain.


#include <Servo.h> 
 
Servo neckhorzservo;  // create servo object to control a servo 
Servo neckvertservo;  // create servo object to control a servo 
Servo eyeservo;  // create servo object to control a servo 
                // a maximum of eight servo objects can be created 
#define PIN_MOTOR_LEFT 22
#define PIN_MOTOR_RIGHT 23
 
int pos = 0;    // variable to store the servo position 
 
unsigned long startms = 0;
void setup() 
{ 
  neckhorzservo.attach(2);  // attaches the servo on pin 9 to the servo object 
  neckvertservo.attach(3);  // attaches the servo on pin 9 to the servo object 
  eyeservo.attach(4);  // attaches the servo on pin 9 to the servo object 
  pinMode(PIN_MOTOR_LEFT,OUTPUT);
  pinMode(PIN_MOTOR_RIGHT,OUTPUT);
  digitalWrite(PIN_MOTOR_LEFT,HIGH);
  digitalWrite(PIN_MOTOR_RIGHT,HIGH);
  startms = millis();
} 
 
int eyetarget = 90;
int neckhorztarget = 90;
int neckverttarget = 90;
int eyecurrent = 90;
int neckhorzcurrent = 90;
int neckvertcurrent = 90;

bool isdone() { return eyetarget == eyecurrent && neckhorztarget == neckhorzcurrent && neckverttarget == neckvertcurrent; }

bool update()
{
  /*if(eyetarget > eyecurrent)
    eyecurrent++;
  if(eyetarget < eyecurrent)
    eyecurrent--;
  if(neckhorztarget > neckhorzcurrent)
    neckhorzcurrent++;
  if(neckhorztarget < neckhorzcurrent)
    neckhorzcurrent--;
  if(neckverttarget > neckvertcurrent)
    neckvertcurrent++;
  if(neckverttarget < neckvertcurrent)
    neckvertcurrent--;
    
  eyeservo.write(eyecurrent);
  neckhorzservo.write(neckhorzcurrent);
  neckvertservo.write(neckvertcurrent);*/
 
  delay(15); 
  
  if( (millis()-startms) > 1000 )
  {
    digitalWrite(PIN_MOTOR_LEFT,LOW);
    digitalWrite(PIN_MOTOR_RIGHT,LOW);
  }
  
  
  return isdone();
}

void loop() 
{ 
  //eyetarget = random(55,125);
  //neckhorztarget = random(50,130);
  //neckverttarget = random(50,130);
  //while(!update());
  
} 
