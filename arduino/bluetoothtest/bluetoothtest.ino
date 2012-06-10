
void setup()
{
  Serial.begin(9600);
  Serial1.begin(115200);
  
  Serial.println("Hello");  
}

void loop()
{
  if(Serial1.available() > 0)
  {
    byte val = Serial1.read();
    Serial1.write(val+1);
    Serial.write(val);
  }
}
