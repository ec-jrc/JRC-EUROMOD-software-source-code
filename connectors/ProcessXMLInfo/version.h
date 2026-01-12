#pragma once
#ifdef PROCESSXMLINFO_EXPORT
	#define DLL_API_VERSION _declspec(dllexport)
#else
	#define DLL_API_VERSION _declspec(dllimport)
#endif

#include <string>

std::string DLL_API_VERSION get_em_software_version();