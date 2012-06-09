#ifndef SPEAKJET_H
#define SPEAKHET_H

#include "Arduino.h"
#include "SoftwareSerial.h"

enum ESpeech
{
 SPEECH_HELLO,
 SPEECH_YOUR,
 SPEECH_ENGINE,
 SPEECH_IS,
 SPEECH_BOUNCY,
 SPEECH_I,
 SPEECH_CAN,
 SPEECH_SEE,
 SPEECH_YOU,
 SPEECH_DEGREES,
 SPEECH_ANGLE,
 SPEECH_DISTANCE,
 SPEECH_0,
 SPEECH_1,
 SPEECH_2,
 SPEECH_3,
 SPEECH_4,
 SPEECH_5,
 SPEECH_6,
 SPEECH_7,
 SPEECH_8,
 SPEECH_9,
 TOTAL_SPEECH 
};

class CSpeakJet
{
public:

  CSpeakJet(uint8_t receive_pin, uint8_t transmit_pin, uint8_t is_speaking_pin);
  
  void Begin();
  void SetVolume(int lVol);
  void SetPitch(int lPitch);
  void SetSpeed(int lSpeed);
  void SetBend(int lBend);
  void Play(uint8_t* data, int len, bool wait);
  void Play(int id, bool wait);
  void PlayNumber(int number, bool wait);
  void AddByte(uint8_t data);
  void Wait();
  
private:
  SoftwareSerial mSerial;
  uint8_t mSpeakingPin;
};

#endif
