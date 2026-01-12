#include "xmlinfo.h"
#include "stata_communication.h"
#include "../StataCLR_Library/clrLibrary.h"
#include <filesystem>
#include <map>
#include "../../ProcessXMLInfo/ModelInfoHandler.h"
#include <sstream>
#include "ReturnListHandler.h"
#include <cctype>

char COUNTRIES_LOCAL_MACRO[]= "_em_connector_countries";

std::map<std::string, std::unique_ptr<NativeModelInfoHandler>> model_handlers;

bool get_model_handler(const std::string& path_model, NativeModelInfoHandler*& refModelInfoHandler) {

    auto it = model_handlers.find(path_model);
    if (it == model_handlers.end()) {
        // Create a new instance of NativeModelInfoHandler using std::make_unique
        auto modelInfoHandlerPtr = std::make_unique<NativeModelInfoHandler>(path_model);
        if (!modelInfoHandlerPtr->is_valid_model()) {
            return false;
        }
        // Get a raw pointer to assign to refModelInfoHandler before moving the unique_ptr to the map
        refModelInfoHandler = modelInfoHandlerPtr.get();
        // Move the unique_ptr to the map to take ownership
        
        model_handlers[path_model] = std::move(modelInfoHandlerPtr);
    }
    else {
        // Assign the existing pointer to refModelInfoHandler
        refModelInfoHandler = it->second.get();
    }
    return true;
}

ST_retcode reload_model(int argc, char* argv[]) {
    if (argc < 2) {
        printErrorMessage("Insufficient number of arguments provided. Aborting! \n");
        return (ST_retcode)-1;
    }
    std::string path_model = std::string(argv[1]);
    auto it = model_handlers.find(path_model);
    if (it == model_handlers.end()) {
        // Create a new instance of NativeModelInfoHandler using std::make_unique
        printErrorMessage("Cannot find model.");
        return (ST_retcode)-1;
    }
    else {
        model_handlers.erase(it);
        auto modelInfoHandlerPtr = std::make_unique<NativeModelInfoHandler>(path_model);
        if (!modelInfoHandlerPtr->is_valid_model()) {
            return false;
        }
        // Get a raw pointer to assign to refModelInfoHandler before moving the unique_ptr to the map
        
        // Move the unique_ptr to the map to take ownership

        model_handlers[path_model] = std::move(modelInfoHandlerPtr);
        return (ST_retcode)0;
    }

}



/// <summary>
/// Funciton that sets the best match and all possible combination macro's
/// </summary>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
ST_retcode setIterators(int argc, char* argv[]) {
    if (argc < 3) {
        printErrorMessage("Insufficient number of arguments provided. Aborting! \n");
        return -1;
    }
    std::string pathEM = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::string dataIteratorStr;
    std::string systemIteratorStr;
    std::string isBestMatchIteratorStr;
    std::string bestSystemIteratorStr;
    std::string bestDataIteratorStr;
    if (!std::filesystem::exists(pathEM)) {
        printErrorMessage(pathEM + "does not exist!\n");
        return -1;
    }
    // getXmlInfo function  is defined in StataCLR_Library
    if (!getXmlInfo(pathEM, country, systemIteratorStr, dataIteratorStr, isBestMatchIteratorStr, bestSystemIteratorStr, bestDataIteratorStr)) {
        printErrorMessage("Could not read country Xml and get systemdata combinations. Aborting.");
        return ST_retcode(-1);
    }
    else {
        // Save all the macro's
        SF_macro_save((char*)"EM_all_systems", (char*)systemIteratorStr.c_str());
        returnListHandler.add_local(std::string("ctry_allSystems"), systemIteratorStr);
        SF_macro_save((char*)"EM_all_datasets", (char*)dataIteratorStr.c_str());
        returnListHandler.add_local(std::string("ctry_allDatasets"), dataIteratorStr);
        SF_macro_save((char*)"EM_isBestMatch", (char*)isBestMatchIteratorStr.c_str());
        returnListHandler.add_local(std::string("ctry_isBestMatch"), isBestMatchIteratorStr);
        SF_macro_save((char*)"EM_bestmatch_systems", (char*)bestSystemIteratorStr.c_str());
        returnListHandler.add_local(std::string("ctry_bestMatchSystems"), bestSystemIteratorStr);
        SF_macro_save((char*)"EM_bestmatch_datasets", (char*)bestDataIteratorStr.c_str());
        returnListHandler.add_local(std::string("ctry_bestMatchDatasets"), bestDataIteratorStr);
        // Give information to the user on how to use the iterators
    }
    return ST_retcode(0);
}

/// <summary>
/// This stores information related to the model. 
/// The concertning information get stored via returnListHandler
/// </summary>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
ST_retcode get_model_info_command(int argc, char* argv[]) {
    std::string path_model = std::string(argv[1]);
    std::ostringstream oss;
    //check if the model handler exissts already
    auto it = model_handlers.find(path_model);
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model. Aborting");
        return (ST_retcode)-1;
    }
    bool i = false;
    // Add the countries of the model to a string
    for (std::string country : modelInfoHandlerPtr->get_countries()) {
        if (i) {
            oss << " ";
        }
        oss << country;
        i = true;
    }
    //store information in return list
    returnListHandler.add_local(std::string("mod_countries"), oss.str());
    return (ST_retcode)0;
}

/// <summary>
/// This stores information related on the Country level.
/// </summary>
/// <param name="argc">number of arguments passed from stata command</param>
/// <param name="argv">number of </param>
/// <returns></returns>
ST_retcode get_country_info_command(int argc, char* argv[]) {
    if (argc < 3) {
        printErrorMessage("Insufficient arguments provided to get_country_info_command");
        return ST_retcode(-1);
    }
    std::string path_model = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::ostringstream oss;

    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model. Aborting");
        return -1;
    }

    bool i = false;
    native_country_info* nciPtr;
    if (!modelInfoHandlerPtr->get_country(country, nciPtr)) {
        printErrorMessage("No valid model for model on path " + path_model + " with country \"" + country + "\".");
        return -1;
    }

    for (const std::string& sys : nciPtr->get_systems()){ //add systems macro
        if (i) {
            oss << " ";
        }
        oss << sys;
        i = true;
    }
    returnListHandler.add_local(std::string("ctry_systems"), oss.str());
    std::ostringstream publicdatasets;
    oss.str("");
    oss.clear();
    bool had_public = false;
    for (const std::string& dataset : nciPtr->get_datasets()) { //add dataset macro
        if (i) {
            oss << " ";
        }
        auto data_info = nciPtr->GetDatasetInfo(dataset);
        bool private_dataset = false;
        auto it = data_info.find("private");
        if (it != data_info.end()){
            private_dataset = it->second == "yes";
        }
        
        if (!private_dataset) {
            if (had_public == true) {
                publicdatasets << " ";
            }
            publicdatasets << dataset;
            had_public = true;
        }
        oss << dataset;
        i = true;
    }
    returnListHandler.add_local(std::string("ctry_datasets"), oss.str());
    returnListHandler.add_local(std::string("ctry_publicdatasets"), publicdatasets.str());
    return (ST_retcode)0;
}

std::string format_str(const std::string& str) {
    std::string copy = str;
    copy[0] = tolower(static_cast<unsigned char>(copy[0]));
    return copy;
}


void setReturnListForInfo(std::map<string,string>& info,const std::string& pref) {
    for (auto kvp : info) {
        std::string value = kvp.second.empty() ? "none" : kvp.second;
        returnListHandler.add_local(pref + format_str(kvp.first), kvp.second);
    }
}


ST_retcode get_dataset_info_command(int argc, char* argv[]) {
    if (argc < 4) {
        printErrorMessage("Insufficient arguments provided to get_dataset_info_command");
        return ST_retcode(-1);
    }
    std::string path_model = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::string dataset = std::string(argv[3]);

    std::ostringstream oss;
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model. Aborting");
        return -1;
    }
    native_country_info* nciPtr;
    if (!modelInfoHandlerPtr->get_country(country, nciPtr)) {
        printErrorMessage("No valid model for model on path " + path_model + " with country \"" + country + "\".");
        return -1;
    }
    auto dataInfo = nciPtr->GetDatasetInfo(dataset);
    setReturnListForInfo(dataInfo, "data_");
    return (ST_retcode)0;
}

ST_retcode get_system_info_command(int argc, char* argv[]) {
    if (argc < 4) {
        printErrorMessage("Insufficient arguments provided to get_country_info_command");
        return ST_retcode(-1);
    }
    std::string path_model = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::string system = std::string(argv[3]);
    std::ostringstream oss;
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model. Aborting");
        return -1;
    }
    native_country_info* nciPtr;
    if (!modelInfoHandlerPtr->get_country(country, nciPtr)) {
        printErrorMessage("No valid model for model on path " + path_model + " with country \"" + country + "\".");
        return -1;
    }
    auto sysinfo = nciPtr->GetSystemExpandedInfo(system);
    if (sysinfo.find("Name") == sysinfo.end()) {
        printErrorMessage("No info for system " + system + " could be retrieved.");
        return -1;
    }
    setReturnListForInfo(sysinfo,"sys_");
    return (ST_retcode)0;
}

ST_retcode get_parameter_info_command(int argc, char* argv[]) {
    if (argc < 5) {
        printErrorMessage("Insufficient arguments provided to get_parameter_info_command");
        return ST_retcode(-1);
    }
    std::string path_model = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::string system = std::string(argv[3]);
    std::string paramId = std::string(argv[4]);
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model. Aborting");
        return -1;
    }
    native_country_info* nciPtr;
    if (!modelInfoHandlerPtr->get_country(country, nciPtr)) {
        printErrorMessage("No valid model for model on path " + path_model + " with country \"" + country + "\".");
        return -1;
    }
    auto sysParInfo = nciPtr->GetSysParInfo(system, paramId);
    setReturnListForInfo(sysParInfo, "par_");
    return ST_retcode(0);
}
ST_retcode get_ext_switch(int argc, char* argv[]) {
    if (argc < 6) {
        printErrorMessage("Insufficient arguments provided to get_ext_switch");
        return ST_retcode(-1);
    }
    std::string path_model = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::string system_name = std::string(argv[3]);
    std::string dataset_name = std::string(argv[4]);
    std::string ext_name = std::string(argv[5]);
    std::map<std::string, std::string> info;
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model.");
        return (ST_retcode)-1;
    }
    string outmessage = modelInfoHandlerPtr->get_ext_switch_info(country, system_name, dataset_name, ext_name, info);
    if (outmessage != "") {
        printErrorMessage(outmessage);
        return (ST_retcode)-1;
    }
    string value;
    if (info.size() == 0) {
        value = "n/a";
    }
    else {
        value = info["Value"];
    }
    returnListHandler.add_local("switchvalue" , value);
    return (ST_retcode)0;
}


ST_retcode set_syspar_value_command(int argc, char* argv[]) {
    if (argc < 6) {
        printErrorMessage("Insufficient arguments provided to get_parameter_info_command");
        return ST_retcode(-1);
    }
    std::string path_model = std::string(argv[1]);
    std::string country = std::string(argv[2]);
    std::string system = std::string(argv[3]);
    std::string paramId = std::string(argv[4]);
    std::string paramValue = std::string(argv[5]);
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(path_model, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model.");
        return -1;
    }
    native_country_info* nciPtr;
    if (!modelInfoHandlerPtr->get_country(country, nciPtr)) {
        printErrorMessage("No valid model for model on path " + path_model + " with country \"" + country + "\".");
        return -1;
    }
    if ( nciPtr->SetSysParValue(system, paramId, paramValue))
        return ST_retcode(0);
    else
        return ST_retcode(-1);

}