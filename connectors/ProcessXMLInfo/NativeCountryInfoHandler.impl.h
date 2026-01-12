#pragma once
#include "NativeCountryInfoHandler.h"
#include <gcroot.h>
#include <msclr/marshal_cppstd.h> // for marshaling
#include <string>
#include "utilities.h" // contains getMarshalledInfo
using namespace msclr::interop;
using namespace System;
using namespace System::Collections::Generic;
using namespace EM_XmlHandler;
using namespace EM_Executable;
using namespace EM_Transformer; // This is needed for EM3Translation
using namespace EM_Common;






class native_country_info::impl {
public:
    impl(const std::string& path, const std::string& country) {
        String^ countryCS = marshal_as<String^>(country);
        auto communicator = gcnew Communicator();
        Dictionary<String^, String^>^ settings = gcnew Dictionary<String^, String^>();
        settings[TAGS::CONFIG_PATH_EUROMODFILES] = marshal_as<String^>(path);
        settings[TAGS::CONFIG_COUNTRY] = countryCS;
        if (!Control::TranslateToEM3(settings, communicator)) {
            throw new std::exception("EM3Translation for Country failed");
        }
        
        infoHandler_ = gcnew CountryInfoHandler(marshal_as<String^>(path), marshal_as<String^>(country));
    }
    impl() {

    }
    /// <summary>
    /// Constructs dictionary containing system Info idenfitied by name of the system
    /// </summary>
    /// <param name="nameSystem"></param>
    /// <returns>map<string,string> with as key the name of characteristics, name the values</returns>
    std::map<std::string, std::string> GetSystemInfo(const std::string& nameSystem) {
        Dictionary<String^, String^>^ info = infoHandler_->GetSystemInfo(marshal_as<String^>(nameSystem));
        std::map<std::string, std::string> output = getMarshalledInfo(info);
        return output;
    }

    bool static isValidCountryModel(const std::string& path, const std::string& country) {
        EMPath^ emPath = gcnew EMPath(marshal_as<String^>(path),true,false);
        return emPath->isValidCountryModel(marshal_as<String^>(country));
    }

    void check_if_recent() {
        infoHandler_->CheckIfRecent();
    }
    std::vector<std::string > get_datasets() {
        std::vector<std::string> output;
        for each (String ^ sys in infoHandler_->GetDatasets()) {
            output.push_back(marshal_as<std::string>(sys));
        }
        return output;
    }
    std::vector<std::string > get_systems() {
        std::vector<std::string> output;
        for each (String ^ sys in infoHandler_->GetSystems()) {
            output.push_back(marshal_as<std::string>(sys));
        }
        return output;
    }
    /// <summary>
    /// Constructs dictionary containing system Info idenfitied by name of the system.
    /// Modifies outString to be a string containing the output with some standard formatting.
    /// </summary>
    /// <param name="nameSystem"></param>
    /// <returns>map<string,string> with as key the name of characteristics, name the values</returns>
    std::map<std::string, std::string> GetSystemInfo(const std::string& nameSystem, std::string& outString) {
        Dictionary<String^, String^>^ info = infoHandler_->GetSystemInfo(marshal_as<String^>(nameSystem));
        std::map<std::string, std::string> output = getMarshalledInfo(info);
        outString = marshal_as<std::string>(infoHandler_->GetInfoInString(info, 0));
        return output;
    }

    std::map<std::string, std::string> GetDatasetInfo(const std::string& nameDataset) {
        Dictionary<String^, String^>^ info = infoHandler_->GetDataSetInfo(marshal_as<String^>(nameDataset));
        std::map<std::string, std::string> output = getMarshalledInfo(info);
        return output;
    }

    std::map<std::string, std::string> GetSysParInfo(const std::string& nameSystem, const std::string& idPar) {
        Dictionary<String^, String^>^ info = infoHandler_->GetSysParInfo(marshal_as<String^>(nameSystem), marshal_as<String^>(idPar));
        std::map<std::string, std::string> output;
        for each (auto kvp in info) {
            output[marshal_as<std::string>(kvp.Key)] = marshal_as<std::string>(XmlHelpers::RemoveCData(kvp.Value));
        }
        return output;
    }

    bool SetSysParValue(const std::string& nameSystem, const std::string& idPar, const std::string& value) {
        return infoHandler_->SetSysParValue(marshal_as<String^>(nameSystem), marshal_as<String^>(idPar), marshal_as<String^>(value));
    }

    /// <summary>
    /// Get system info + best datasets available
    /// </summary>
    /// <param name="nameSystem"></param>
    /// <returns></returns>
    std::map<std::string, std::string> GetSystemExpandedInfo(const std::string& nameSystem) {
        Dictionary<String^, String^>^ info = infoHandler_->GetSystemExpandedInfo(marshal_as<String^>(nameSystem));
        std::map<std::string, std::string> output = getMarshalledInfo(info);
        return output;
    }
    std::map<std::string, std::string> GetSystemExpandedInfo(const std::string& nameSystem, std::string& outString) {
        Dictionary<String^, String^>^ info = infoHandler_->GetSystemExpandedInfo(marshal_as<String^>(nameSystem));
        std::map<std::string, std::string> output = getMarshalledInfo(info);
        outString = marshal_as<std::string>(infoHandler_->GetInfoInString(info, 0));
        return output;
    }
    CountryInfoHandler^ GetInfoHandler() {
        return infoHandler_;
    }
private:
    gcroot<CountryInfoHandler^> infoHandler_;
};