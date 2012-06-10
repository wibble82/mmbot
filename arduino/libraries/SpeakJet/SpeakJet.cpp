#include "SpeakJet.h"
#include "Arduino.h"
#include "SoftwareSerial.h"

#define PAUSE90 6

CSpeakJet::CSpeakJet(uint8_t receive_pin, uint8_t transmit_pin, uint8_t is_speaking_pin)
  : mSerial(receive_pin, transmit_pin)
{
  pinMode(receive_pin, INPUT);
  pinMode(transmit_pin, OUTPUT);
  pinMode(is_speaking_pin, INPUT);
  
  mSpeakingPin = is_speaking_pin;
  
}

void CSpeakJet::Begin()
{
  mSerial.begin(9600); 
}

void CSpeakJet::SetVolume(int lVol)
{
  mSerial.write(20);     // Enter volume set mode
  mSerial.write(lVol);   // Set volume to 96 (out of 127)
}

void CSpeakJet::SetPitch(int lPitch)
{
  mSerial.write(22);
  mSerial.write(lPitch);
}

void CSpeakJet::SetSpeed(int lSpeed)
{
  mSerial.write(21);   // Enter speed set mode
  mSerial.write(lSpeed);  // Set speed to 114 (out of 127)
}

void CSpeakJet::SetBend(int lBend)
{
  mSerial.write(23);
  mSerial.write(lBend);
}

void CSpeakJet::Play(uint8_t* data, int len, bool wait)
{
  int i;
  for (i=0; i<len; i++)
  {
    mSerial.write(data[i]);
  }  
  if(wait)
  {
    mSerial.write(uint8_t(255));

    delay(50);
    while(digitalRead(mSpeakingPin) == HIGH)
      delay(1);
  }
}

void CSpeakJet::AddByte(uint8_t data)
{
	mSerial.write(data);
}

void CSpeakJet::Wait()
{
	mSerial.write(uint8_t(255));

	delay(50);
	while(digitalRead(mSpeakingPin) == HIGH)
		delay(1);
}

uint8_t SPEECHMSG_HELLO		[]		= {183, 7, 159, 146, 164 };
uint8_t SPEECHMSG_YOUR		[]		= {128, 153 };
uint8_t SPEECHMSG_ENGINE	[]		= {130, 141, 7, 165, 8, 129, 141 };
uint8_t SPEECHMSG_IS		[]		= {8, 129, 167 };
uint8_t SPEECHMSG_BOUNCY	[]		= {8, 171, 7, 163, 141, 187, 128 };
uint8_t SPEECHMSG_I		[]		= {157};
uint8_t SPEECHMSG_CAN		[]		= {194, 8, 132, 141};
uint8_t SPEECHMSG_SEE		[]		= {187, 187, 128, 128};
uint8_t SPEECHMSG_YOU		[]		= {8, 160};
uint8_t SPEECHMSG_DEGREES	[]		= {174, 131, 178, 148, 128, 157};
uint8_t SPEECHMSG_ANGLE	        []		= {154, 143, 145};
uint8_t SPEECHMSG_DISTANCE	[]		= {174, 7, 129, 187, 191, 133, 141, 8, 187};
uint8_t SPEECHMSG_0		[]		= {167, 7, 128, 7, 149, 164};
uint8_t SPEECHMSG_1		[]		= {147, 14, 136, 8, 141};
uint8_t SPEECHMSG_2		[]		= {8, 191, 162};
uint8_t SPEECHMSG_3		[]		= {8, 190, 148, 8, 128};
uint8_t SPEECHMSG_4		[]		= {186, 7, 137, 153};
uint8_t SPEECHMSG_5		[]		= {186, 157, 166};
uint8_t SPEECHMSG_6		[]		= {8, 187, 129, 14, 194, 7, 187};
uint8_t SPEECHMSG_7		[]		= {8, 187, 7, 131, 166, 131, 141};
uint8_t SPEECHMSG_8		[]		= {154, 4, 191};
uint8_t SPEECHMSG_9		[]		= {141, 14, 157, 141};

void CSpeakJet::Play(int id, bool wait)
{
  switch(id)
  {
    case SPEECH_HELLO		: Play(SPEECHMSG_HELLO		, sizeof(SPEECHMSG_HELLO	), wait); break;
    case SPEECH_YOUR		: Play(SPEECHMSG_YOUR		, sizeof(SPEECHMSG_YOUR		), wait); break;
    case SPEECH_ENGINE		: Play(SPEECHMSG_ENGINE		, sizeof(SPEECHMSG_ENGINE	), wait); break;
    case SPEECH_IS		: Play(SPEECHMSG_IS		, sizeof(SPEECHMSG_IS		), wait); break;
    case SPEECH_BOUNCY		: Play(SPEECHMSG_BOUNCY		, sizeof(SPEECHMSG_BOUNCY	), wait); break;
    case SPEECH_I		: Play(SPEECHMSG_I		, sizeof(SPEECHMSG_I		), wait); break;
    case SPEECH_CAN		: Play(SPEECHMSG_CAN		, sizeof(SPEECHMSG_CAN		), wait); break;
    case SPEECH_SEE		: Play(SPEECHMSG_SEE		, sizeof(SPEECHMSG_SEE		), wait); break;
    case SPEECH_YOU		: Play(SPEECHMSG_YOU		, sizeof(SPEECHMSG_YOU		), wait); break;
    case SPEECH_DEGREES		: Play(SPEECHMSG_DEGREES	, sizeof(SPEECHMSG_DEGREES	), wait); break;
    case SPEECH_ANGLE		: Play(SPEECHMSG_ANGLE	        , sizeof(SPEECHMSG_ANGLE	), wait); break;
    case SPEECH_DISTANCE	: Play(SPEECHMSG_DISTANCE	, sizeof(SPEECHMSG_DISTANCE	), wait); break;
    case SPEECH_0		: Play(SPEECHMSG_0		, sizeof(SPEECHMSG_0		), wait); break;
    case SPEECH_1		: Play(SPEECHMSG_1		, sizeof(SPEECHMSG_1		), wait); break;
    case SPEECH_2		: Play(SPEECHMSG_2		, sizeof(SPEECHMSG_2		), wait); break;
    case SPEECH_3		: Play(SPEECHMSG_3		, sizeof(SPEECHMSG_3		), wait); break;
    case SPEECH_4		: Play(SPEECHMSG_4		, sizeof(SPEECHMSG_4		), wait); break;
    case SPEECH_5		: Play(SPEECHMSG_5		, sizeof(SPEECHMSG_5		), wait); break;
    case SPEECH_6		: Play(SPEECHMSG_6		, sizeof(SPEECHMSG_6		), wait); break;
    case SPEECH_7		: Play(SPEECHMSG_7		, sizeof(SPEECHMSG_7		), wait); break;
    case SPEECH_8		: Play(SPEECHMSG_8		, sizeof(SPEECHMSG_8		), wait); break;
    case SPEECH_9		: Play(SPEECHMSG_9		, sizeof(SPEECHMSG_9		), wait); break;
    default: break;
  }  
}

void CSpeakJet::PlayNumber(int number, bool wait)
{
  int orignumber = number;
  if(orignumber >= 1000)
    Play(int(SPEECH_0+number/1000),false);
  number = number % 1000;
  if(orignumber >= 100)
    Play(int(SPEECH_0+number/100),false);
  number = number % 100;
  if(orignumber >= 10)
    Play(int(SPEECH_0+number/10),false);
  number = number % 10;
  
  Play(int(SPEECH_0+number/1),false);
  
  if(wait)
  {
    delay(50);
    while(digitalRead(mSpeakingPin) == HIGH)
      delay(1);
  }  
}

