
void setup() {                
  pinMode(13, OUTPUT);  
  pinMode(5, OUTPUT);  
  pinMode(12, INPUT);  
  pinMode(11, INPUT);  
  pinMode(10, INPUT);  
  Serial.begin(9600);
}

long microsecondsToCentimeters(long microseconds)
{
  // The speed of sound is 340 m/s or 29 microseconds per centimeter.
  // The ping travels out and back, so to find the distance of the
  // object we take half of the distance travelled.
  return microseconds / 29 / 2;
}

void loop() {
  
  digitalWrite(5, LOW);
  delay(1000);
  //digitalWrite(5, HIGH);
  //delay(1000);
  return;
  
  /*if(digitalRead(12) == HIGH)
  {
    digitalWrite(13, HIGH);   // set the LED on
  }
  else
  {
    digitalWrite(13, LOW);    // set the LED off
  }*/
  
  pinMode(10, OUTPUT);
  digitalWrite(10, HIGH);
  delayMicroseconds(5);
  digitalWrite(10, LOW);
  pinMode(10, INPUT);
  
  int cm = microsecondsToCentimeters(pulseIn(10, HIGH));
  
  //Serial.println(cm);
  
  analogWrite(5, map(constrain(cm, 1, 50), 1, 50, 255, 0));
  
  delay(20);
}


