#include <SoftwareSerial.h>

SoftwareSerial MotorSerial(8,7);

//PIN Definitions
#define PIN_ENCODER_LEFT_1 2
#define PIN_ENCODER_LEFT_2 3
#define PIN_ENCODER_RIGHT_1 20
#define PIN_ENCODER_RIGHT_2 21
#define PIN_SWITCH_MOTORS 10
#define PIN_SWITCH_SPEEDCONTROL 11

byte current_left_encoder = 0;
byte current_right_encoder = 0;
int QEM [16] = {0,-1,1,2,1,0,2,-1,-1,2,0,1,2,1,-1,0};
long left_encoder_pos = 0;
long left_encoder_time = 0;
long right_encoder_pos = 0;
long right_encoder_time = 0;
byte readLeftEncoder()
{
   byte d0 = digitalRead(PIN_ENCODER_LEFT_1);
   byte d1 = digitalRead(PIN_ENCODER_LEFT_2);
   return d0 | (d1 << 1);
}
byte readRightEncoder()
{
   byte d0 = digitalRead(PIN_ENCODER_RIGHT_1);
   byte d1 = digitalRead(PIN_ENCODER_RIGHT_2);
   return d0 | (d1 << 1);
}

void updateLeftEncoder()
{
  left_encoder_time = micros();
  byte new_encoder = readLeftEncoder();
  int dir = QEM[current_left_encoder*4+new_encoder];
  left_encoder_pos -= dir;
  current_left_encoder = new_encoder;
 // Serial.println("Left");
}
void updateRightEncoder()
{
  right_encoder_time = micros();
  byte new_encoder = readRightEncoder();
  int dir = QEM[current_right_encoder*4+new_encoder];
  right_encoder_pos += dir;
  current_right_encoder = new_encoder;
  //Serial.println(right_encoder_time/1000);
}

int motor_right_speed = 196;

void setup()  
{
  MotorSerial.begin(9600);
  //MotorSerial.write((byte)50);
  //MotorSerial.write((byte)200);
  MotorSerial.write(20);
  MotorSerial.write(motor_right_speed);
  
  pinMode(PIN_ENCODER_LEFT_1,INPUT);
  pinMode(PIN_ENCODER_LEFT_2,INPUT);
  pinMode(PIN_ENCODER_RIGHT_1,INPUT);
  pinMode(PIN_ENCODER_RIGHT_2,INPUT);
  pinMode(PIN_SWITCH_MOTORS,INPUT);
  pinMode(PIN_SWITCH_SPEEDCONTROL,INPUT);

  attachInterrupt(0, updateLeftEncoder, CHANGE);
  attachInterrupt(1, updateLeftEncoder, CHANGE);
  attachInterrupt(2, updateRightEncoder, CHANGE);
  attachInterrupt(3, updateRightEncoder, CHANGE);

  Serial.begin(9600);
  Serial.println("Hello"); 
}

int ctr = 0;
void loop()
{
  /*for(int i = 0; i < 10; i++)
  {
     digitalWrite(PIN_ENCODER_RIGHT_1,(i&1 )?HIGH :LOW);
     digitalWrite(PIN_ENCODER_RIGHT_2,(i&1 )?HIGH :LOW);
     delay(20);
   }*/
   
   if(digitalRead(PIN_SWITCH_MOTORS) == LOW)
   {
     Serial.println("Motors off");
      MotorSerial.write((byte)0); 
   }
   else if(digitalRead(PIN_SWITCH_SPEEDCONTROL) == LOW)
   {
     Serial.println("Motors on, speed control off");
     MotorSerial.write(1+19);
     MotorSerial.write(128+19);
   }
   else
   {
     Serial.println("Motors on, speed control on");
      if(left_encoder_pos < right_encoder_pos)
      {
        motor_right_speed = min(255,motor_right_speed+1);
        MotorSerial.write(20);
        MotorSerial.write(motor_right_speed);    
        ctr=0;
        left_encoder_pos = 0;
        right_encoder_pos = 0;
      }
      else if(left_encoder_pos > right_encoder_pos)
      {
        motor_right_speed = max(128,motor_right_speed-1);
        MotorSerial.write(20);
        MotorSerial.write(motor_right_speed);         
        ctr=0;
        left_encoder_pos = 0;
        right_encoder_pos = 0;
      }
   }
  delay(50);
  
  Serial.print(motor_right_speed);
  Serial.print("\t");
  Serial.print(left_encoder_pos);
  Serial.print("\t");
  Serial.print(right_encoder_pos);
  Serial.println("");
  
  //gradually take motors from full stop to full reverse
  /*for(int i = 64; i >= 1; i--)
  {
    MotorSerial.write(i);
    MotorSerial.write(i+128);
    delay(100);
  }
  
  //take motors from full reverse, back to stop and then to full forwards
  for(int i = 1; i <= 127; i++)
  {
    MotorSerial.write(i);
    MotorSerial.write(i+128);
    delay(100);
  }
  
  //take motors back down to full stop
  for(int i = 127; i >= 64; i--)
  {
    MotorSerial.write(i);
    MotorSerial.write(i+128);
    delay(100);
  }*/
}
