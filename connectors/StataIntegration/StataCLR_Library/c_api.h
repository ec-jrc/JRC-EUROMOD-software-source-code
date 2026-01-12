#pragma once
#include "pch.h"
#include <string>
#include <vector>
#include <map>
#include <tuple>

#ifdef CLR_LIBRARY
  #define EUROMOD_API __declspec(dllexport)
#else
  #define EUROMOD_API  __declspec(dllimport)
#endif


extern "C" EUROMOD_API int runEMfromCpp_C(const char* system, const char* pathEM, const char* dataSetId, char* error_message, const char* pathData, const char* country,
    double input_arr[], int length, const char** variables, int variables_length,
    std::map<std::string, double*, CaseInsensitiveCompare>&outputDictCpp,
    std::map<std::string, std::vector<std::string>, CaseInsensitiveCompare>&outputVarDictCpp,
    std::map<std::string, int, CaseInsensitiveCompare>&EMoutputObsDict,
    std::vector<EuromodError>&errors,
    bool suppressOutput,
    std::map<std::tuple<std::string, std::string>, std::string>&constantsToOverwrite,
    std::map<std::string, std::string>&extraSettings, bool useLogger,
    std::map<std::string, bool>&extensions, const std::tuple<std::string, std::string>*addons, int addons_length,
    native_country_info * countryInfoPtr, const char* pathOutput, bool keepInMemory,
    const char* breakAfterFunId);

// Opaque helpers for output maps constructed on the C++ side.
// They return/accept void* so Rust doesn't need to see C++ types.

// Existing helpers with new raw getters
extern "C" EUROMOD_API void* em_out_map_new();
extern "C" EUROMOD_API void  em_out_map_free(void* m);
extern "C" EUROMOD_API void  em_out_map_put(void* m, const char* key, double* buf);
extern "C" EUROMOD_API void* em_out_map_get_raw(void* m);  // NEW

extern "C" EUROMOD_API void* em_varvec_map_new();
extern "C" EUROMOD_API void  em_varvec_map_free(void* m);
extern "C" EUROMOD_API void  em_varvec_map_put_empty(void* m, const char* key);
extern "C" EUROMOD_API void* em_varvec_map_get_raw(void* m);  // NEW
extern "C" EUROMOD_API int   em_varvec_map_vec_len(void* m, const char* key);
extern "C" EUROMOD_API const char* em_varvec_map_vec_get(void* m, const char* key, int index);

// NEW: extraSettings (map<string,string>)
extern "C" EUROMOD_API void* em_strstr_map_new();
extern "C" EUROMOD_API void  em_strstr_map_free(void* m);
extern "C" EUROMOD_API void  em_strstr_map_put(void* m, const char* key, const char* value);

// NEW: constantsToOverwrite (map<tuple<string,string>,string>)
extern "C" EUROMOD_API void* em_constants_map_new();
extern "C" EUROMOD_API void  em_constants_map_free(void* m);
extern "C" EUROMOD_API void  em_constants_map_put(void* m, const char* key1, const char* key2, const char* value);

// NEW: errors vector (std::vector<EuromodError>&)
extern "C" EUROMOD_API void* em_errors_vec_new();
extern "C" EUROMOD_API void  em_errors_vec_free(void* v);
extern "C" EUROMOD_API int   em_errors_vec_count(void* v);
extern "C" EUROMOD_API int   em_errors_vec_get_is_warning(void* v, int index);
extern "C" EUROMOD_API const char* em_errors_vec_get_message(void* v, int index);
// NEW: Get the actual vector pointer to pass to runEMfromCpp_C
extern "C" EUROMOD_API void* em_errors_vec_get_raw_vector(void* v);

// NEW: helpers to iterate over populated outputDict
extern "C" EUROMOD_API int   em_out_map_key_count(void* m);
extern "C" EUROMOD_API const char* em_out_map_get_key(void* m, int index);
extern "C" EUROMOD_API double* em_out_map_get_buffer(void* m, const char* key);

// NEW: helpers to iterate over populated outputVarDict  
extern "C" EUROMOD_API int   em_varvec_map_key_count(void* m);
extern "C" EUROMOD_API const char* em_varvec_map_get_key(void* m, int index);

// NEW: helpers to iterate over populated EMoutputObsDict
extern "C" EUROMOD_API int   em_obs_map_get_len(void* m, const char* key);

// NEW: extensions (likely map<string, string> or similar)
extern "C" EUROMOD_API void* em_extensions_map_new();
extern "C" EUROMOD_API void  em_extensions_map_free(void* m);
extern "C" EUROMOD_API void  em_extensions_map_put(void* m, const char* key, const char* value);

// NEW: addons (array of tuple<string, string>)
extern "C" EUROMOD_API void* em_addons_array_new();
extern "C" EUROMOD_API void  em_addons_array_free(void* a);
extern "C" EUROMOD_API void  em_addons_array_push(void* a, const char* first, const char* second);
extern "C" EUROMOD_API int   em_addons_array_size(void* a);
extern "C" EUROMOD_API void* em_addons_array_data(void* a);  // Get raw pointer to pass to C++

// NEW: country info helpers
extern "C" EUROMOD_API void* em_country_info_new();
extern "C" EUROMOD_API void  em_country_info_free(void* info);
extern "C" EUROMOD_API native_country_info* em_country_info_get_ptr(void* info);

