// RtspStreamerLib.cpp : Defines the exported functions for the DLL application.

#include "stdafx.h"
#include "RtspStreamerLib.h"

RTSPSTREAMERLIB_API void* CreateRtspStreamerLib(void)
{
	return new CRtspStreamerLib();
}

RTSPSTREAMERLIB_API void DestroyRtspStreamerLib(void* pLib)
{
	if (pLib != NULL)
	{
		CRtspStreamerLib *lib = (CRtspStreamerLib*)pLib;
		delete lib;
	}
}

RTSPSTREAMERLIB_API int StartStreamLib(void* pLib, char* streamName, char* mediaPath, int portNumber)
{
	if (pLib != NULL)
	{
		CRtspStreamerLib *lib = (CRtspStreamerLib*)pLib;
		if (lib->_IsStreaming)
		{
			lib->RestartStreaming();
			return RET_SUCCESS;
		}
		else
		{
			return lib->StartStream(streamName, mediaPath, portNumber);
		}
	}
	return RET_FAILED;
}

RTSPSTREAMERLIB_API void StopStreamLib(void* pLib)
{
	if (pLib != NULL)
	{
		CRtspStreamerLib *lib = (CRtspStreamerLib*)pLib;
		lib->StopStream();
	}
}

RTSPSTREAMERLIB_API BOOL GetStreamStatusLib(void* pLib)
{
	if (pLib != NULL)
	{
		CRtspStreamerLib *lib = (CRtspStreamerLib*)pLib;
		return lib->_IsStreaming;
	}
	return false;
}

RTSPSTREAMERLIB_API int GetStreamRateLib(void* pLib)
{
	if (pLib != NULL)
	{
		CRtspStreamerLib *lib = (CRtspStreamerLib*)pLib;
		return lib->GetStreamRate();
	}
	return RET_FAILED;
}

RTSPSTREAMERLIB_API char* GetVlcVersionLib(void* pLib)
{
	if (pLib != NULL)
	{
		CRtspStreamerLib *lib = (CRtspStreamerLib*)pLib;
		return lib->GetVlcVersion();
	}
	return NULL;
}

// ---------------------------------------------------------------------

// This is the constructor of a class that has been exported.
// see RtspStreamerLib.h for the class definition

const char * const vlc_args[] = {
	"-I",
	"dummy",					// Don't use any interface
	"--ignore-config",			// Don't use VLC's config
	"--avcodec-hw=any",
	"--avcodec-hw=dxva2",

	"--network-caching=100",
	"--postproc-q=0",
	// Loop for version 2.2.6.
	"--loop",
	// Loop for version 3 and higher.
	//"--input-repeat=65535",		// ¬ыставл€ем репит дл€ потока.
	//"--plugin-path=/plugins",
	//"--no-ffmpeg-hurry-up",
	//"--ffmpeg-hw",
};

const char * const vlc_opts[] = {
	":sout-all",
	":sout-keep",
	":sout-no-audio"
};

CRtspStreamerLib::CRtspStreamerLib()
{
	return;
}

CRtspStreamerLib::~CRtspStreamerLib()
{
	CloseInstance();
}

void CRtspStreamerLib::CloseInstance()
{
	if (_IsStreaming)
	{
		StopStream();
	}
}

int CRtspStreamerLib::StartStream(char* streamName, char* mediaPath, int portNumber)
{
	if (mediaPath != NULL)
	{
		//DWORD threadId = 0;
		_StreamName = streamName;
		_MediaPath = mediaPath;
		_Port = portNumber;
		InitiateStreaming();
		return RET_SUCCESS;
	}
	else return RET_FAILED;
}

void CRtspStreamerLib::StopStream()
{
	if (_Event_Mgr != NULL)
	{
		libvlc_event_detach(_Event_Mgr, libvlc_VlmMediaInstanceStatusPlaying, CallbackEvent, this);
		libvlc_event_detach(_Event_Mgr, libvlc_VlmMediaInstanceStopped, CallbackEvent, this);
		libvlc_event_detach(_Event_Mgr, libvlc_VlmMediaInstanceStatusEnd, CallbackEvent, this);
		libvlc_event_detach(_Event_Mgr, libvlc_VlmMediaInstanceStatusPause, CallbackEvent, this);
		libvlc_event_detach(_Event_Mgr, libvlc_VlmMediaInstanceStatusError, CallbackEvent, this);
	}
	libvlc_vlm_stop_media(_VlcInstance, _StreamName.c_str());
	libvlc_vlm_del_media(_VlcInstance, _StreamName.c_str());
	libvlc_vlm_release(_VlcInstance);
	_VlcInstance = NULL;
	_IsStreaming = FALSE;
}

void CRtspStreamerLib::InitiateStreaming()
{
	_VlcInstance = libvlc_new((sizeof(vlc_args) / sizeof(*vlc_args)), vlc_args);

	_Event_Mgr = libvlc_vlm_get_event_manager(_VlcInstance);
	if (_Event_Mgr != NULL)
	{
		libvlc_event_attach(_Event_Mgr, libvlc_VlmMediaInstanceStatusPlaying, CallbackEvent, this);
		libvlc_event_attach(_Event_Mgr, libvlc_VlmMediaInstanceStopped, CallbackEvent, this);
		libvlc_event_attach(_Event_Mgr, libvlc_VlmMediaInstanceStatusEnd, CallbackEvent, this);
		libvlc_event_attach(_Event_Mgr, libvlc_VlmMediaInstanceStatusPause, CallbackEvent, this);
		libvlc_event_attach(_Event_Mgr, libvlc_VlmMediaInstanceStatusError, CallbackEvent, this);
	}

	string sout = "#rtp{sdp=rtsp://:" + std::to_string(_Port) + "/}";

	libvlc_vlm_add_broadcast(_VlcInstance, _StreamName.c_str(), ("file:///" + _MediaPath).c_str(),
		sout.c_str(), (sizeof(vlc_opts) / sizeof(*vlc_opts)), vlc_opts, true, true);

	libvlc_vlm_play_media(_VlcInstance, _StreamName.c_str());
}

void CRtspStreamerLib::RestartStreaming()
{
	if (_VlcInstance != NULL)
	{
		libvlc_vlm_stop_media(_VlcInstance, _StreamName.c_str());
		libvlc_vlm_play_media(_VlcInstance, _StreamName.c_str());
	}
}

void CRtspStreamerLib::CallbackEvent(const libvlc_event_t* event, void* ptr)
{
	CRtspStreamerLib* self = reinterpret_cast<CRtspStreamerLib*>(ptr);
	switch (event->type) 
	{
		case libvlc_VlmMediaInstanceStatusPlaying:
		{
			self->_IsStreaming = TRUE;
			break;
		}

		case libvlc_VlmMediaInstanceStopped:
		case libvlc_VlmMediaInstanceStatusPause:
		case libvlc_VlmMediaInstanceStatusEnd:
		{
			self->_IsStreaming = FALSE;
			break;
		}

		case libvlc_VlmMediaInstanceStatusError:
		{
			//self->RestartStreaming();
			break;
		}

		default:
			break;
	}

	self->_status = event->type;
	//if (event->type == libvlc_VlmMediaInstanceStatusPlaying)
	//{
	//	//thread th(&CRtspStreamerLib::RestartStreaming, self);
	//	//th.detach();
	//}
}
int CRtspStreamerLib::GetStreamRate()
{
	if (_VlcInstance != NULL)
	{
		return _status;
		// ”прощаем работу, отказываемс€ от насили€ над потоком в пользу событий.
		//return libvlc_vlm_get_media_instance_rate(_VlcInstance, _StreamName.c_str(), 0);
	}
	return RET_FAILED;
}

char* CRtspStreamerLib::GetVlcVersion()
{
	return (char *)libvlc_get_version();
}
	
