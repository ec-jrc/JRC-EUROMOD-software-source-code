#pragma once
#include "NativeCountryInfoHandler.h"
#include <gcroot.h>
#include <msclr/marshal_cppstd.h> // for marshaling
#include <string>
using namespace msclr::interop;
using namespace System;
using namespace System::Collections::Generic;
using namespace std;
using namespace EM_XmlHandler;
// Include the C++/CLI header (generated from your C++/CLI project)
class NativeCountryInfoHandler::Impl {
public:
    // Managed wrapper object, created using C++/CLI
    

    Impl(const std::string& path, const std::string& country) {
        ctryHandler = gcnew CountryInfoHandler(marshal_as<String^>(path), marshal_as<String^>(country));
    }

    std::map<std::string, std::string> GetSystemInfo(const std::string& nameSystem) {
        Dictionary<String^, String^>^ info = ctryHandler->GetSystemInfo(marshal_as<String^>(nameSystem));
        map<string, string> output;
        for each (auto kvp in info) {
            output[marshal_as<string>(kvp.Key)] = marshal_as<string>(kvp.Value);
        }
        return output;
    }
private:
    gcroot<CountryInfoHandler^> ctryHandler;


};