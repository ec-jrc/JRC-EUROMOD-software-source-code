#pragma once
#include "pch.h"
#include <string>
#include <vector>
#include <map>
using namespace System;
using namespace EM_XmlHandler;
using namespace std;
enum class ReadCountryOptionsNative {
    COUNTRY = 1 << 0,
    SYS = 1 << 1,
    POL = 1 << 2,
    SYS_POL = 1 << 3,
    REFPOL = 1 << 4,
    FUN = 1 << 5,
    SYS_FUN = 1 << 6,
    PAR = 1 << 7,
    SYS_PAR = 1 << 8,
    UPIND = 1 << 9,
    UPIND_YEAR = 1 << 10,
    EXSTAT = 1 << 11,
    EXSTAT_YEAR = 1 << 12,
    DATA = 1 << 13,
    SYS_DATA = 1 << 14,
    LOCAL_EXTENSION = 1 << 15,
    EXTENSION_POL = 1 << 16,
    EXTENSION_FUN = 1 << 17,
    EXTENSION_PAR = 1 << 18,
    EXTENSION_SWITCH = 1 << 19,
    INDTAX = 1 << 20,
    INDTAX_YEAR = 1 << 21,
};

public ref class CountryInfoHandlerWrapper
{
private:
    CountryInfoHandler^ ctryHandler;
public:
    CountryInfoHandlerWrapper(string path, string country)
    {
        ctryHandler = gcnew CountryInfoHandler(marshal_as<String^>(path), marshal_as<String^>(country));
    }
    map<string, string> GetSystemInfo(string nameSystem) {

        Dictionary<String^, String^>^ info = ctryHandler->GetSystemInfo(marshal_as<String^>(nameSystem));
        map<string, string> output;
        for each (auto kvp in info) {
            output[marshal_as<string>(kvp.Key)] = marshal_as<string>(kvp.Value);
        }
        return output;
    }
    map<string, string> GetSystemInfo(string nameSystem, string& outputstring) {
        Dictionary<String^, String^>^ info = ctryHandler->GetSystemInfo(marshal_as<String^>(nameSystem));
        map<string, string> output;
        for each (auto kvp in info) {
            output[marshal_as<string>(kvp.Key)] = marshal_as<string>(kvp.Value);
        }
        outputstring = marshal_as<string>(ctryHandler->GetInfoInString(info, 0));
        return output;
    }
};

