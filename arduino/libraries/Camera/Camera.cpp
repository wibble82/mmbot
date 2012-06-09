#include "Camera.h"

CCamera::CCamera()
{
	HwSrl = NULL;
	SwSrl = NULL;
	State = CAMSTATE_IDLE;
	IntBuffer = NULL;
	DataBuffer = NULL;
	ContentBytesRequested = 0;
}

bool CCamera::Begin(HardwareSerial* serial, unsigned long baud)
{
	if(!SwSrl && !HwSrl)
	{
		SwSrl = NULL;
		HwSrl = serial;
		HwSrl->begin(baud);
		State = CAMSTATE_IDLE;
	}
	return true;
}

bool CCamera::Begin(SoftwareSerial* serial, unsigned long baud)
{
	if(!SwSrl && !HwSrl)
	{
		HwSrl = NULL;
		SwSrl = serial;
		SwSrl->begin(baud);
		State = CAMSTATE_IDLE;
	}
	return true;
}

bool CCamera::Update()
{
	switch(State)
	{
	case CAMSTATE_RESETTING:
		{
			if(GetAvailable() < 63) //63 bytes is cos camera prints a load of gumf after a reset!
				return false;
		}
		break;
	case CAMSTATE_STARTINGPIC:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	case CAMSTATE_FILESIZE:
		{
			if(GetAvailable() < 9)
				return false;
			for(int i = 0; i < 7; i++)
				Read();
			*IntBuffer = ((unsigned long)(Read()) << 8) + (unsigned long)(Read());
		}
		break;
	case CAMSTATE_CONTENT:
		{
			if(GetAvailable() < (ContentBytesRequested+10))
				return false;
			for(int i = 0; i < 5; i++)
				Read();
			DataBuffer[0] = Read();
			int eof = -1;
			for(int i = 1; i < ContentBytesRequested; i++)
			{
				DataBuffer[i] = Read();
				if(DataBuffer[i-1] == 0xff && DataBuffer[i] == 0xd9)
					eof = i;
			}
			*IntBuffer = eof == -1 ? ContentBytesRequested : eof;
			for(int i = 0; i < 5; i++)
				Read();
		}
		break;
	case CAMSTATE_STOPPINGPIC:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	case CAMSTATE_SETDIMS:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	case CAMSTATE_SETCOMPRATIO:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	case CAMSTATE_ENTERPOWER:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	case CAMSTATE_LEAVEPOWER:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	case CAMSTATE_SETBAUD:
		{
			if(GetAvailable() < 5)
				return false;
		}
		break;
	default:
		break;
	}

	if(GetAvailable()>0)
	{
		Serial.print("Unprocessed bytes: ");
		Serial.print(GetAvailable());
		Serial.print(": ");
		while(GetAvailable() > 0)
		{
			Serial.print("0x");
			uint8_t data = Read();
			if(data < 0x10)
				Serial.print("0");
			Serial.print(data,HEX);
			Serial.print(" ");
		}
		Serial.println("");
	}


	State = CAMSTATE_IDLE;
	IntBuffer = NULL;
	DataBuffer = NULL;
	ContentBytesRequested = 0;
	return true;
}

bool CCamera::IsDone()
{
	return State == CAMSTATE_IDLE;
}

ECameraState CCamera::GetState()
{
	return State;
}

bool CCamera::Wait(unsigned long timeout_ms)
{
	unsigned long pmstart = micros();
	unsigned long t = millis();
	while(!IsDone())
	{
		Update();
		if(timeout_ms != 0 && (millis() - t) > timeout_ms)
			return false;
	}
	WaitPerfMon += (pmstart-micros());
	return true;
}

bool CCamera::Reset()
{
	if(State != CAMSTATE_IDLE)
		return false;
	Write(0x56);
	Write(0x00);
	Write(0x26);
	Write(0x00);
	State = CAMSTATE_RESETTING;
	return true;
}

bool CCamera::StartTakingPicture()
{
	if(State != CAMSTATE_IDLE)
		return false;
	Write(0x56);
	Write(0x00);
	Write(0x36);
	Write(0x01);
	Write(0x00);  
	State = CAMSTATE_STARTINGPIC;
	return true;
}

bool CCamera::GetFileSize(unsigned long* size_buffer)
{
	if(State != CAMSTATE_IDLE)
		return false;
	Write(0x56);
	Write(0x00);
	Write(0x34);
	Write(0x01);
	Write(0x00);  
	IntBuffer = size_buffer;
	State = CAMSTATE_FILESIZE;
	return true;
}

bool CCamera::GetContent(uint8_t* buffer, unsigned long* amount_read_buffer, unsigned long bytes, unsigned long address)
{
	unsigned pmstart = micros();

	if(State != CAMSTATE_IDLE)
		return false;

	uint8_t MH=uint8_t(address/0x100);
	uint8_t ML=uint8_t(address%0x100);
	uint8_t SH=uint8_t(bytes/0x100);
	uint8_t SL=uint8_t(bytes%0x100);

	//did this slightly more optimal way cos it gets called a lot!
	uint8_t buff[] = {0x56,0x00,0x32,0x0c,0x00,0x0a,0x00,0x00,MH,ML,0x00,0x00,SH,SL,0x00,0x0a};
	if(HwSrl)
	{
		HwSrl->write(buff,sizeof(buff));
	}
	else if(SwSrl)
	{
		SwSrl->write(buff,sizeof(buff));
	}

	/*Write(0x56);
	Write(0x00);
	Write(0x32);
	Write(0x0c);
	Write(0x00); 
	Write(0x0a);
	Write(0x00);
	Write(0x00);
	Write(MH);
	Write(ML);   
	Write(0x00);
	Write(0x00);
	Write(SH);
	Write(SL);
	Write(0x00);  
	Write(0x0a);*/

	ContentBytesRequested = bytes;
	IntBuffer = amount_read_buffer;
	DataBuffer = buffer;

	State = CAMSTATE_CONTENT;
	SendPerfMon += micros()-pmstart;
	return true;
}

bool CCamera::StopTakingPicture()
{
	if(State != CAMSTATE_IDLE)
		return false;

	Write(0x56);
	Write(0x00);
	Write(0x36);
	Write(0x01);
	Write(0x03);        

	State = CAMSTATE_STOPPINGPIC;
	return true;
}

bool CCamera::SetDimensions(ECameraImageDimensions dims)
{
	if(State != CAMSTATE_IDLE)
		return false;

	Write(0x56);
	Write(0x00);
	Write(0x31);
	Write(0x05);
	Write(0x04);
	Write(0x01);
	Write(0x00);
	Write(0x19);

	switch(dims)
	{
	case IMG_DIMS_160x120: Write(0x22); break;
	case IMG_DIMS_320x240: Write(0x11); break;
	case IMG_DIMS_640x480: Write(0x00); break;
	default: break;
	};

	State = CAMSTATE_SETDIMS;
	return true;
}

bool CCamera::SetCompressionRatio(unsigned long ratio_0_to_255)
{
	if(State != CAMSTATE_IDLE)
		return false;
	
	Write(0x56);
	Write(0x00);
	Write(0x31);
	Write(0x05);
	Write(0x01);
	Write(0x01);
	Write(0x12);
	Write(0x04);
	Write(uint8_t(ratio_0_to_255));

	State = CAMSTATE_SETCOMPRATIO;
	return true;
}

bool CCamera::EnterPowerSaving()
{
	if(State != CAMSTATE_IDLE)
		return false;

	Write(0x56);
	Write(0x00);
	Write(0x3E);
	Write(0x03);
	Write(0x00);
	Write(0x01);
	Write(0x01);

	State = CAMSTATE_ENTERPOWER;
	return true;
}

bool CCamera::LeavePowerSaving()
{
	if(State != CAMSTATE_IDLE)
		return false;

	Write(0x56);
	Write(0x00);
	Write(0x3E);
	Write(0x03);
	Write(0x00);
	Write(0x01);
	Write(0x00);

	State = CAMSTATE_LEAVEPOWER;
	return true;
}

bool CCamera::SetBaudRate(ECameraBaudRate rate)
{
	if(State != CAMSTATE_IDLE)
		return false;

	Write(0x56);
	Write(0x00);
	Write(0x24);
	Write(0x03);
	Write(0x01);

	switch(rate)
	{
	case CAM_BAUD_9600: Write(0xAE); Write(0xC8); break;
	case CAM_BAUD_19200: Write(0x56); Write(0xE4); break;
	case CAM_BAUD_38400: Write(0x2A); Write(0xF2); break;
	case CAM_BAUD_57600: Write(0x1C); Write(0x4C); break;
	case CAM_BAUD_115200: Write(0x0D); Write(0xA6); break;
	default: break;
	}

	State = CAMSTATE_SETBAUD;
	return true;
}

unsigned long CCamera::GetAvailable()
{
	if(HwSrl)
		return HwSrl->available();
	else if(SwSrl)
		return SwSrl->available();
	else
		return 0;
}

uint8_t CCamera::Read()
{
	unsigned pmstart = micros();
	uint8_t res = 0;
	if(HwSrl)
		res = HwSrl->read();
	else if(SwSrl)
		res = SwSrl->read();
	RecvPerfMon += micros()-pmstart;
	return res;
}

void CCamera::Write(uint8_t val)
{
	unsigned pmstart = micros();
	if(HwSrl)
		HwSrl->write(val);
	else if(SwSrl)
		SwSrl->write(val);
	SendPerfMon += micros()-pmstart;
}
