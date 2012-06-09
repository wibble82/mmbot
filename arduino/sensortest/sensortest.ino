
//PIN Definitions
#define PIN_MOTOR_LEFT 22
#define PIN_MOTOR_RIGHT 23
#define SENSOR_SERVO_PIN_EYE 4
#define SENSOR_SERVO_PIN_X 2
#define SENSOR_SERVO_PIN_Y 3
#define PIN_ULTRASOUND 5
#define PIN_MOTIONSENSOR 6
#define PIN_IRLEFT 1
#define PIN_IRRIGHT 2


///////////////////////////////////////////////////////////////////
//converts ultrasound timer to centimeters
///////////////////////////////////////////////////////////////////
long microsecondsToCentimeters(long microseconds)
{
  return microseconds / 29 / 2;
}

///////////////////////////////////////////////////////////////////
//reads ultrasound distance sensor
///////////////////////////////////////////////////////////////////
int readUltraSound()
{
  pinMode(PIN_ULTRASOUND, OUTPUT);
  digitalWrite(PIN_ULTRASOUND, HIGH);
  delayMicroseconds(5);
  digitalWrite(PIN_ULTRASOUND, LOW);
  pinMode(PIN_ULTRASOUND, INPUT); 
  return microsecondsToCentimeters(pulseIn(PIN_ULTRASOUND, HIGH));  
}

///////////////////////////////////////////////////////////////////
//reads motion sensor
///////////////////////////////////////////////////////////////////
int readMotion()
{
  return digitalRead(PIN_MOTIONSENSOR);
}

///////////////////////////////////////////////////////////////////
//reads infra red left
///////////////////////////////////////////////////////////////////
int readIRLeft()
{
  return analogRead(PIN_IRLEFT);
}

///////////////////////////////////////////////////////////////////
//reads infra red left
///////////////////////////////////////////////////////////////////
int readIRRight()
{
  return analogRead(PIN_IRRIGHT);
}

///////////////////////////////////////////////////////////////////
//init
///////////////////////////////////////////////////////////////////
void setup()  
{
  //init pins
  pinMode(PIN_MOTOR_LEFT,OUTPUT);
  pinMode(PIN_MOTOR_RIGHT,OUTPUT);
  pinMode(PIN_ULTRASOUND,INPUT);
  pinMode(PIN_MOTIONSENSOR,INPUT);
  pinMode(PIN_IRLEFT,INPUT);
  pinMode(PIN_IRRIGHT,INPUT);
  
  //set default pin values
  digitalWrite(PIN_MOTOR_LEFT,LOW);
  digitalWrite(PIN_MOTOR_RIGHT,LOW);
  
  //init serial port for debugging and print message  
  Serial.begin(115200);
  Serial.println("Hello");

}

void loop()
{
  Serial.print("PING: "); Serial.println(readUltraSound());
  Serial.print("MOTN: "); Serial.println(readMotion());
  Serial.print("IRLE: "); Serial.println(readIRLeft());
  Serial.print("IRRI: "); Serial.println(readIRRight());
  delay(250);
}
