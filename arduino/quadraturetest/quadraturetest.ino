
//PIN Definitions
#define PIN_IRLEFT 2
#define PIN_IRRIGHT 3
#define PIN_OUTLEFT 4
#define PIN_OUTRIGHT 5
#define PIN_OUTXOR 6

byte current_encoder = 0;
int QEM [16] = {0,-1,1,2,1,0,2,-1,-1,2,0,1,2,1,-1,0};
long encoder_pos = 0;
long encoder_time = 0;
byte readEncoder()
{
   byte left = digitalRead(PIN_IRLEFT);
   byte right = digitalRead(PIN_IRRIGHT);
   return left | (right << 1);
}

void updateEncoder()
{
  encoder_time = micros();
  byte new_encoder = readEncoder();
  int dir = QEM[current_encoder*4+new_encoder];
  encoder_pos += dir;
  current_encoder = new_encoder;
}


///////////////////////////////////////////////////////////////////
//init
///////////////////////////////////////////////////////////////////
long last_encoder_pos = 0;
long last_time = 0;
long last_encoder_time = 0;
void setup()  
{
  //init pins
  pinMode(PIN_IRLEFT,INPUT);
  pinMode(PIN_IRRIGHT,INPUT);
  pinMode(PIN_OUTLEFT,OUTPUT);
  pinMode(PIN_OUTRIGHT,OUTPUT);
  pinMode(PIN_OUTXOR,OUTPUT);
    
  //init serial port for debugging and print message  
  Serial.begin(115200);
  Serial.println("Hello");
  
  last_time = micros();
  current_encoder = readEncoder();
  attachInterrupt(0, updateEncoder, CHANGE);
  attachInterrupt(1, updateEncoder, CHANGE);
}

long current_rpm_nocomp = 0;
long current_rpm_timecomp = 0;
long current_rpm_evencomp = 0;
long current_rpm_allcomp = 0;

long calcrpm(long last_time, long new_time, long last_encoder_pos, long new_encoder_pos)
{
    long pos_delta        = new_encoder_pos - last_encoder_pos;
    long time_delta       = new_time-last_time;
    return ((pos_delta * 60000000) / time_delta)/32;
}
 
void loop()
{    
  //record current time, the current encoder position, and the time the last encoder reading occured
  long new_time         = micros();
  long new_encoder_pos  = encoder_pos;
  long new_encoder_time = encoder_time;
  
  //calculate rpm with no compensation
  current_rpm_nocomp    = calcrpm(last_time, new_time, last_encoder_pos, new_encoder_pos);
  
  //calculate rpm using with time compensation (i.e. we use the last encoder time rather than current time in calculations)
  current_rpm_timecomp  = calcrpm(last_encoder_time, new_encoder_time, last_encoder_pos, new_encoder_pos);
  
  //calculate rpm, only updating if it's an even numbered reading
  current_rpm_evencomp  = (new_encoder_pos & 1) ? current_rpm_evencomp : calcrpm(last_time, new_time, last_encoder_pos, new_encoder_pos);
  
  //calculate rpm if even numbered reading, using time compensation
  current_rpm_allcomp   = (new_encoder_pos & 1) ? current_rpm_allcomp : calcrpm(last_encoder_time, new_encoder_time, last_encoder_pos, new_encoder_pos);
  
  //record last readings to use in next calculations
  last_time             = new_time;
  last_encoder_pos      = new_encoder_pos;
  last_encoder_time     = new_encoder_time;
  
  Serial.print(new_time);
  Serial.print("\t");
  Serial.print(new_encoder_pos);
  Serial.print("\t");
  Serial.print(current_rpm_nocomp);
  Serial.print("\t");
  Serial.print(current_rpm_timecomp);
  Serial.print("\t");
  Serial.print(current_rpm_evencomp);
  Serial.print("\t");
  Serial.print(current_rpm_allcomp);
  Serial.print("\n");
  
  delay(50);
}
