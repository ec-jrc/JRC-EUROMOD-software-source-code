#include "pch.h"
#include <string>
#include <vector>
#include <map>
#include <tuple>

extern "C" EUROMOD_API int runEMfromCpp_C(
    const char* system, const char* pathEM, const char* dataSetId,
    char* error_message, const char* pathData, const char* country,
    double* input_arr, int length, const char** variables, int variables_length,
    std::map<std::string, double*, CaseInsensitiveCompare>& outputDictCpp,
    std::map<std::string, std::vector<std::string>, CaseInsensitiveCompare>& outputVarDictCpp,
    std::map<std::string, int, CaseInsensitiveCompare>& EMoutputObsDict,
    std::vector<EuromodError>& errors,
    bool suppressOutput,
    std::map<std::tuple<std::string, std::string>, std::string>& constantsToOverwrite,
    std::map<std::string, std::string>& extraSettings, bool useLogger,
    std::map<std::string, bool>& extensions, const std::tuple<std::string, std::string>* addons, int addons_length,
    native_country_info* countryInfoPtr, const char* pathOutput, bool keepInMemory,
    const char* breakAfterFunId) {
    try {
        std::vector<std::string> variables_vec(variables, variables + variables_length);
        std::vector<std::tuple<std::string, std::string>> addons_vec(addons, addons + addons_length);
        std::string error_message_cpp = std::string(error_message);

        // Wrap the actual call
        try {
            return runEMfromCpp(system, pathEM, dataSetId, error_message_cpp, pathData, country, input_arr, length, variables_vec,
                outputDictCpp, outputVarDictCpp, EMoutputObsDict, errors, "",
                "", "", suppressOutput, constantsToOverwrite, extraSettings,
                useLogger, extensions, addons_vec, countryInfoPtr, pathOutput, keepInMemory, breakAfterFunId);
        }
        catch (System::Exception^ e) {
            // Capture managed exception details
            String^ fullError = String::Format(
                "Managed Exception: {0}\nStack Trace:\n{1}\nInner Exception: {2}",
                e->Message,
                e->StackTrace,
                e->InnerException != nullptr ? e->InnerException->ToString() : "None"
            );

            // Convert to native string and copy to error_message
            pin_ptr<const wchar_t> nativeError = PtrToStringChars(fullError);
            std::wstring ws(nativeError);
            std::string errorStr(ws.begin(), ws.end());

            if (error_message) {
                strncpy(error_message, errorStr.c_str(), 4095);
                error_message[4095] = '\0';
            }

            return -99; // Special error code for managed exceptions
        }
    }
    catch (const std::exception& e) {
        // Native C++ exception
        if (error_message) {
            snprintf(error_message, 4096, "C++ Exception: %s", e.what());
        }
        return -98;
    }
    catch (...) {
        // Unknown exception
        if (error_message) {
            strcpy(error_message, "Unknown exception occurred");
        }
        return -97;
    }
}