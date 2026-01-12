using namespace std;
#include "../StataCLR_Library/utilities.h"
#include <sstream>
#include "stplugin.h"
#include "../StataCLR_Library/clrLibrary.h"
#include "../StataCLR_Library/euromoderror.h"
#include <string.h>
#include <string>
#include <algorithm>
#include <map>
#include <iostream>
#include <filesystem>
//#include "../StataDisplaying/Header.h"
#include "../../ProcessXMLInfo/version.h"
#include "../../ProcessXMLInfo/NativeCountryInfoHandler.h"
#include "../../ModelInfoHandler.h";
#include "version.h"
#include "stata_communication.h"
#include "xmlinfo.h"
#include "ReturnListHandler.h"
#include "../StataCLR_Library/CaseInsensitiveUtils.h"


using namespace Euromod;
bool version_is_set = false;
map<string, double*, CaseInsensitiveCompare> EMoutputDict;
map<string, vector<string>, CaseInsensitiveCompare> EMoutputVarsDict;
map<string, int, CaseInsensitiveCompare> EMoutputObs;
vector<double*> EM_output;
vector<vector<string>> EM_output_vars;
int nIncludedObs = 0;
ST_int  first, last, nVars;
const char* IDPERSON = "idperson";
char VARLIST[] = "_varlist_input_EM9870";
// This will handle adding variables to the return list
ReturnListHandler returnListHandler;






ST_retcode setInputArray(ST_double* arr,ST_int nvars, int& n_included_obs,ST_int first, ST_int last) {
    ST_double value;
    ST_retcode rc;
    rc = (ST_retcode)0;
    char msg[80];
    // Loop over observations
    int counter = 0;
    try {
        for (int i = first; i <= last; i++) {
            if (!SF_ifobs(i)) {
                continue;
            }
            for (int j = 1; j <= nvars; j++) {
            
                
                rc = SF_vdata(j, i, &value);
                if (rc > 0) {
                    sprintf_s(msg, "\n Problem accessing Stata data\n");
                    SF_error(msg);
                    return(rc);
                }
                *(arr + (j - 1) * n_included_obs + (counter)) = value;
            }
            counter++;
        }
    }
    catch (...)
    {
        printErrorMessage(to_string(first) + ", " + to_string(last) + ", " + to_string(n_included_obs));
        return -1;
    }
    return(rc);
}

ST_retcode getVarList(vector<string>& vars) {
    char msg[82];
    ST_retcode rc = ST_retcode(0);
    char contents[100000];
    rc =  SF_macro_use(VARLIST, contents, 100000);
    if (rc != 0) {
        strcpy_s(msg, "\n Problem accessing Stata data\n");
        SF_error(msg);
        SF_error(msg);
        return rc;
    }
    string var_str = string(contents);
    
    vars = split(rtrim(ltrim(var_str)), ' ');
    size_t len = vars.size();

    if (len != SF_nvars()) return (ST_retcode)-1;

    return rc;
}
/// <summary>
/// Function that sets the outputVariable macro in stata
/// </summary>
/// <param name="argc">number of arguments</param>
/// <param name="argv">array of character pointers</param>
/// <returns></returns>
int setOutputVarsMacro(int argc, char* argv[]) {
    if (argc != 2) {
        printErrorMessage("Wrong number of parameters passed.\n You passed " + to_string(argc) + " parameter(s).\n");
        return -1;
    }
    string filename = string(argv[1]);
    auto it = EMoutputVarsDict.find(filename);
    if (it == EMoutputVarsDict.end()) {
        printErrorMessage("Invalid name for output has been provided. Aborting loading data into memory.\n");
        return (ST_retcode)-1;
    }
    else {
        stringstream ss;
        int ctr = 0;
        for (const auto& varName : it->second) {
            if (ctr > 0) ss << " ";
            ss << varName;
            ctr++;
        }
        string concatVarNames = ss.str();
        char* stata_mac_var = (char*)concatVarNames.c_str();
        SF_macro_save((char*)"vars_EM", stata_mac_var);
        returnListHandler.add_local("outputVars", concatVarNames);
        SF_macro_save((char*)"_toload", (char*)filename.c_str());
    }
    return 0;
}

string getStrOfDatasets(map<string,double*, CaseInsensitiveCompare> outputDataDict) {
    int ctr = 0;
    if (outputDataDict.size() == 0) return "";
    std::stringstream ss;
    for (const auto& pair : outputDataDict) {
        if (ctr > 0)
            ss << " " << pair.first;
        else
            ss << pair.first;
        ctr++;
    }


    return ss.str();
}



/// <summary>
/// This function transforms the arguments to be passed to the CLR-Library
/// This function manages all the simulation steps:
/// 1) Getting the arguments for simulation
/// 2) Retrieving the data in stata
/// 3) Calling the CLR library
/// 4) Dealing with the output and if there is more than one output, saving the variable names in a local 
/// </summary>
/// <param name="argc">number of parameters passed to the stata plugin</param>
/// <param name="argv">character arrays containing the parameters passed to the plugin</param>
/// <returns></returns>
int simulate(int argc, char* argv[]) {
    //check if there are enough parameters
    ST_retcode rc = (ST_retcode)0;
    if (argc < 16) {
        printErrorMessage("Not enough arguments provided to plugin.\n");
        return (ST_retcode)-1;
    }

    //Set parameters for the plugin
    int c = 1;
    string system = string(argv[c++]);
    string pathEM = string(argv[c++]);
    string dataSetId = string(argv[c++]);
    string pathData = string(argv[c++]);
    string nameOutput = string(argv[c++]);
    string country = string(argv[c++]);
    string varsOutputRequested = string(argv[c++]);
    string ILoutputRequested = string(argv[c++]);
    string outputQueriesStr = string(argv[c++]);
    bool suppressOutput = (strcmp(argv[c++], "suppress_output") == 0);
    string overwriteConstantStr = string(argv[c++]);
    string extraSettingsStr = string(argv[c++]);
    bool useLogger = !(strcmp(argv[c++], "disableLogger") == 0);
    string extensionsStr = string(argv[c++]);
    string addonStr = string(argv[c++]);
    string pathOutput = string(argv[c++]);
    bool keepInMemory = (strcmp(argv[c++], "keep") == 0);
    c++;
    string breakAfterFunId;
    if (c < argc) {
        breakAfterFunId = string(argv[c]);
    }
 
    
    
    //Check if the model we want to use is a valid one
    if (!native_country_info::isValidCountryModel(pathEM, country)) {
        printErrorMessage("No valid model present for path and country provided.");
        return ST_retcode(-1);
    }
   


    first = SF_in1();
    // Put the last observation in sample into last
    last = SF_in2();


    //ALWAYS reset nIncludedObs to zero for new simulation!!!!
    nIncludedObs = 0;
    for (int i = first; i <= last; i++) {
        if (SF_ifobs(i)) {
            nIncludedObs += 1;
        }
    }


    // Put number of variables in varlist passed in to plugin into nVars
    nVars = SF_nvars();
    ST_double* arr = new ST_double[nVars * nIncludedObs];
    rc = setInputArray(arr, nVars, nIncludedObs, first, last);
    if (rc != 0) return rc;

    vector<string> variables;
    if (getVarList(variables) != 0) return -1;


    if (rc != 0) return rc;
    string error_message = "";
    int length = nVars * nIncludedObs;
    // Every simulation start from empty vectors
    EMoutputDict.clear();
    EMoutputVarsDict.clear();
    EMoutputObs.clear();
    vector<EuromodError> EMerrors;
    //run EUROMOD
    native_country_info* countryInfo;
    NativeModelInfoHandler* modelInfoHandlerPtr = nullptr;
    if (!get_model_handler(pathEM, modelInfoHandlerPtr)) {
        printErrorMessage("Path provided is not a valid model. Aborting");
        return -1;
    }
    if (!modelInfoHandlerPtr->get_country(country, countryInfo)) {
        printErrorMessage("Could not load country info for the model on path " + pathEM + "");
        return -1;
    }
    int success = runEMfromStata(system, pathEM, dataSetId, error_message, pathData, country, arr, length, variables, EMoutputDict, EMoutputVarsDict, EMoutputObs, EMerrors, varsOutputRequested, ILoutputRequested, outputQueriesStr, suppressOutput, overwriteConstantStr, extraSettingsStr, useLogger, extensionsStr, addonStr,countryInfo, pathOutput,keepInMemory,breakAfterFunId);
    //Check and report any errors or warnings
    if (success != 0) {
        printErrorMessage("Simulation not successfull for system " + system + " and dataset " + dataSetId + ".Exited with " +  to_string(EMerrors.size()) + " errors / warnings.\n");
    }
    else if (EMerrors.size() > 0) {
        printErrorMessage("EUROMOD simulations successfull for system " + system + " and dataset " + dataSetId + ", but there were " + to_string(EMerrors.size()) + " warnings: \n ");
    }
    //print error messages and also save then into return list via returnListHandler
    ostringstream ss;
    for (EuromodError error : EMerrors) {
        string prefix = error.getIsWarning() ? "Warning: " : "Error: ";
        string errorline = prefix + error.getMessage() + "\n";
        printErrorMessage(errorline);
        ss << errorline << "\\n";
    }
    if (ss.str().empty()) {
        returnListHandler.add_local("errorMessages", "none");
    }
    else {
        returnListHandler.add_local("errorMessages", ss.str());
    }
    
    string nErrors = to_string(EMerrors.size());
    returnListHandler.add_local("nErrors", nErrors);
    SF_macro_save((char*)"EM_n_errors", (char*)nErrors.c_str());
    if (success != 0) {
        return -1;
    }
    
    //save outputdatasets to macro
    string datasetsStr = getStrOfDatasets(EMoutputDict);
    string nOutputsStr = to_string(EMoutputDict.size());
    SF_macro_save((char*)"EM_outputs", (char*)datasetsStr.c_str());
    SF_macro_save((char*)"EM_n_outputs", (char*)nOutputsStr.c_str());
    returnListHandler.add_local("outputFiles", datasetsStr);
    returnListHandler.add_local("nOutputFiles", nOutputsStr);
    if (EMoutputDict.size() > 1 ) {
        if (nameOutput.empty()) {
            printMessage(nameOutput);
            printMessage("There are multiple outputs stored for this simulation and the desired output has not been specified. \n Please specify which one you would like to return. \n");
            printMessage("You can retrieve the output by running: \n");
            for (auto kvp : EMoutputVarsDict) {
                printMessage("euromod_getdata, outputdataset(" + kvp.first + ")\n");
            }
            printMessage("Note that you can specify the desired prefix as an option.\n");
        }
    }
    else if (EMoutputDict.size() == 0)
    {
        printErrorMessage("No output has been returned by the EUROMOD simulation");
        return(-1);
    }
    
 
    delete[] arr;
    return(rc);
}


int setNobs(int argc, char* argv[]) {
    string nameDataset = string(argv[1]);
    if (EMoutputDict.find(nameDataset) == EMoutputDict.end()) {
        printErrorMessage("Dataset name not present in output!");
        //SF_macro_save((char*)"__rc", (char*)"-1");
        return -1;
    }
    int nOutputObs = EMoutputObs[nameDataset];
    SF_macro_save((char*)"_nObs", (char*)to_string(nOutputObs).c_str());
    returnListHandler.add_local("nOutputObs", to_string(nOutputObs));
    return 0;
}

int canConcatenate(int argc, char* argv[]) {
    string nameDataset = string(argv[1]);
    if (EMoutputDict.find(nameDataset) == EMoutputDict.end()) {
        printErrorMessage("Dataset name not present in output!");
        //SF_macro_save((char*)"__rc", (char*)"-1");
        return -1;
    }
    vector<string> EMoutputVars= EMoutputVarsDict[nameDataset];
    int idxIdpersonDonor = 0;
    auto it = find(EMoutputVars.begin(), EMoutputVars.end(), IDPERSON);
    if (it == EMoutputVars.end()) {
        SF_macro_save((char*)"__rc", (char*)"-1");
        return -1;
    }
    else {
        idxIdpersonDonor = distance(EMoutputVars.begin(), it);
    }
    double* EMoutput = EMoutputDict[nameDataset];
    int nOutputObs = EMoutputObs[nameDataset];
    SF_macro_save((char * )"_nObs", (char *)to_string(nOutputObs).c_str());
    ST_double idpersonRecipient = 0;
    int nVars = EMoutputVarsDict[nameDataset].size();
    if (nIncludedObs != nOutputObs) {
        SF_macro_save((char*)"__rc", (char*)"-1");
        return -1;
    }
    else {
        // counter for getting variable out of EM output
        int ctr = 0;
        for (int i = SF_in1(); i <= SF_in2(); i++) {
            if (!SF_ifobs(i)) {
                continue;
            }
            int rc = SF_vdata(1, i, &idpersonRecipient);
            if (idpersonRecipient != EMoutput[ctr * nVars + idxIdpersonDonor]) {
                printErrorMessage("Not a match for idperson-recipient" + to_string(idpersonRecipient) + " and " + to_string(EMoutput[ctr * nVars + idxIdpersonDonor]));
                return -1;
            }
            ctr++;
        }
    }
    return 0;
}





int store_data2(int argc, char* argv[])
{
    string nameDataset = string(argv[1]);
    double* output = new double[0];
    if (EMoutputDict.empty()) {
        printErrorMessage("No data has been stored by the STATA-Connector. Aborting.");
        return (ST_retcode)-1;
    }
    if (EMoutputDict.size() == 1) {
        output = EMoutputDict.begin()->second;
        nameDataset = EMoutputDict.begin()->first;
    }
    else if (EMoutputDict.size() > 1) {
        if (nameDataset.empty()) {
            printErrorMessage("There are multiple outputs stored for this simulation and the desired output has not been specified. \n Please specify which one you would like to return. \n");
            printErrorMessage("You can retrieve the output by running: \n");
            for (auto kvp : EMoutputVarsDict) {
                printErrorMessage("euromod_getdata, outputdataset(" + kvp.first + ")\n");
            }
            printErrorMessage("Note that you can specify the desired prefix as an option.\n");
            return 0;
        }
        else {
            auto it = EMoutputDict.find(nameDataset);
            if (it == EMoutputDict.end()) {
                printErrorMessage("No valid name for the dataset has been provided. \n");
                return (ST_retcode)-1;
            }
            else {
                output = it->second;
            }
        }
    }
    
    first = SF_in1();
    last = SF_in2();
    int rc = 0;
    int nVars = SF_nvars();
    int counter = 0;
    for (int i = first; i <= last; i++) {
        if (!SF_ifobs(i)) {
            continue;
        }
        for (int j = 1; j <= nVars; j++) {
            rc = SF_vstore(j, i, output[(j - 1) + (counter)*nVars]);
        }
        counter++;
    }
    return(rc);
}


/// <summary>
/// This function dispatches the euromod plugin calls to it respective functions. the first argument determines the function that will be called
/// </summary>
/// <param name="argc"></param>
/// <param name="argv"></param>
/// <returns></returns>
STDLL stata_call(int argc, char* argv[])
{
    clearErrorMessage();
    if (!is_right_version()) {
        printErrorMessage("The right version of the plugin is not installed. Please upgrade your EUROMOD software\n");
        return -1;
    }
    /*
    * This will be added with the public release :)
    if (!is_right_stata_command_version()) {
        printErrorMessage("The version of the euromod command is not compatible with your version of the software. Please upgrade the version of the euromod stata commands through ssc.\n");
        return -1;
    }*/ 
    int rc = -2;
    try {
        if (argc) {
            if (strcmp(argv[0], "simulate") == 0) {
                return simulate(argc, argv);
            }
            else if (strcmp(argv[0], "storeData") == 0)
            {
                return store_data2(argc, argv);
            }
            else if (strcmp(argv[0], "setOutputVarsMacro") == 0) {
                return setOutputVarsMacro(argc, argv);
            }
            else if (strcmp(argv[0], "checkConcatenation") == 0) {
                return canConcatenate(argc, argv);
            }
            else if (strcmp(argv[0], "setNobs") == 0) {
                return setNobs(argc, argv);
            }
            else if (strcmp(argv[0], "setIterators") == 0) {
                return setIterators(argc, argv);
            }
            else if (strcmp(argv[0], "xmlInfo") == 0) {
                return get_model_info_command(argc, argv);
            }
            else if (strcmp(argv[0], "xmlInfoCountry") == 0) {
                return get_country_info_command(argc, argv);
            }
            else if (strcmp(argv[0], "setReturnList") == 0) {
                returnListHandler.set_macros();
                return ST_retcode(0);
            }
            else if (strcmp(argv[0], "xmlInfoSystem") == 0) {
                return get_system_info_command(argc, argv);
            }
            else if (strcmp(argv[0], "xmlInfoDataset") == 0) {
                return get_dataset_info_command(argc, argv);
            }
            else if (strcmp(argv[0], "xmlInfoPar") == 0) {
                return get_parameter_info_command(argc, argv);
            }
            else if (strcmp(argv[0], "setXmlInfoPar") == 0) {
                return set_syspar_value_command(argc, argv);
            }
            else if (strcmp(argv[0], "reload")==0) {
                return reload_model(argc, argv);
            }
            else if (strcmp(argv[0], "getExtensionSwitchValue") == 0) {
                return get_ext_switch(argc, argv);
            } 
            else if (strcmp(argv[0], "setVersion") == 0) {
                return set_euromod_version();
            }
            else {
                printErrorMessage("Non-valid argument passed to the plugin.\n");
                return ST_retcode(-1);
            }

        }
    }
    catch (std::exception e){
        printErrorMessage(e.what());
    }
    catch (...) {
        printErrorMessage("Something unexpected got caught.");
    }
    return(rc);
}


