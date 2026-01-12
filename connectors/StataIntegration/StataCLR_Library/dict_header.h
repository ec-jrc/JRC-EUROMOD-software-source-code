#include "CommandProcessing.h"
#pragma once
#if defined(CLR_LIBRARY)
#define DICT_API __declspec(dllexport)
#else
#define DICT_API __declspec(dllimport)
#endif

DICT_API int createDictionary(const std::string& s, Dictionary<String^, String^>^ dict);