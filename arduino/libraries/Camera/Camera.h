#ifndef _CAMERA_H_
#define _CAMERA_H_

#include <Arduino.h>
#include <SoftwareSerial.h>

enum ECameraImageDimensions
{
	IMG_DIMS_640x480,
	IMG_DIMS_320x240,
	IMG_DIMS_160x120
};

enum ECameraBaudRate
{
	CAM_BAUD_9600,
	CAM_BAUD_19200,
	CAM_BAUD_38400,
	CAM_BAUD_57600,
	CAM_BAUD_115200
};

enum ECameraState
{
	CAMSTATE_IDLE,
	CAMSTATE_RESETTING,
	CAMSTATE_STARTINGPIC,
	CAMSTATE_FILESIZE,
	CAMSTATE_CONTENT,
	CAMSTATE_STOPPINGPIC,
	CAMSTATE_SETDIMS,
	CAMSTATE_SETCOMPRATIO,
	CAMSTATE_ENTERPOWER,
	CAMSTATE_LEAVEPOWER,
	CAMSTATE_SETBAUD
};

class CCamera
{
public:

	CCamera();
	bool Begin(HardwareSerial* serial, unsigned long baud=38400);
	bool Begin(SoftwareSerial* serial, unsigned long baud=38400);
	bool Update(); //returns true if idle after update
	bool IsDone();
	ECameraState GetState();
	bool Wait(unsigned long timeout_ms=0); //pass 0 for 'infinite'
	bool Reset();
	bool StartTakingPicture();
	bool GetFileSize(unsigned long* size_buffer);
	bool GetContent(uint8_t* buffer, unsigned long* amount_read_buffer, unsigned long bytes, unsigned long address);
	bool StopTakingPicture();
	bool SetDimensions(ECameraImageDimensions dims);
	bool SetCompressionRatio(unsigned long ratio_0_to_255);
	bool EnterPowerSaving();
	bool LeavePowerSaving();
	bool SetBaudRate(ECameraBaudRate rate);
		
	void ResetPerfMons() { SendPerfMon = RecvPerfMon = WaitPerfMon = 0; }
	unsigned long GetSendPerfMon() { return SendPerfMon; }
	unsigned long GetRecvPerfMon() { return RecvPerfMon; }
	unsigned long GetWaitPerfMon() { return WaitPerfMon; }
private:
	HardwareSerial* HwSrl;
	SoftwareSerial* SwSrl;
	ECameraState State;
	uint8_t* DataBuffer;
	unsigned long* IntBuffer;
	unsigned long ContentBytesRequested;
	unsigned long SendPerfMon;
	unsigned long RecvPerfMon;
	unsigned long WaitPerfMon;

	unsigned long GetAvailable();
	uint8_t Read();
	void Write(uint8_t);
};

#endif
