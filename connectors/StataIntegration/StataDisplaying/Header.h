#pragma once
#include <string>
#include "stplugin.h"
#ifdef STATADISPLAYING_EXPORTS
	#define DLL_API_DISP __declspec(dllexport)
#else
	#define DLL_API_DISP __declspec(dllimport)
#endif
ST_retcode DLL_API_DISP printMessage(std::string message);
void DLL_API_DISP printErrorMessage(std::string message);

