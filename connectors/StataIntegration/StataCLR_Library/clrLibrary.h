#pragma once
#include <map>
#include <string>
#include <vector>
#ifdef CLR_LIBRARY
#define clrLibrary_API __declspec(dllexport)

#else
#define clrLibrary_API __declspec(dllimport)

#endif
#include "euromoderror.h"
#include "../../ProcessXMLInfo/NativeCountryInfoHandler.h"
using namespace Euromod;


#ifdef STATA_PLUGIN
	#define STATA_PLUGIN_API __declspec(dllexport)
#else
	#define STATA_PLUGIN_API __declspec(dllimport)
#endif
#include "CaseInsensitiveUtils.h"

clrLibrary_API int runEMfromStata(string system, string pathEM, string dataSetId, string& error_message, string pathData, 
	string country, double input_arr[], int length, vector<string> variables, 
	map<string,double*, CaseInsensitiveCompare>& outputDictCpp, map<string,vector<string>, CaseInsensitiveCompare>& outputVarsDictCpp, map<string, int, CaseInsensitiveCompare>& EMoutputObs, vector<EuromodError>& errors,
	string varsOutputRequested, string ILoutputRequested, string outputQueriesStr, 
	bool suppressOutput, string constantToOverwriteStr, string extraSettings,
	bool useLogger, string extensionString, string addonString, native_country_info* nciPtr, string pathOutput, bool keepInMemory, string breakAfterFunId);

clrLibrary_API bool getXmlInfo(string pathEM, string country, string& systemIteratorStr, string& dataIteratorStr, string& isBestMatchIteratorStr, string& bestSystemIteratorStr, string& bestDataIteratorStr);

clrLibrary_API int runEMfromCpp(string system, string pathEM, string dataSetId, string& error_message, string pathData, string country,
	double input_arr[], int length, vector<string> variables, map<string, double*, CaseInsensitiveCompare>& outputDictCpp, map<string, vector<string>, CaseInsensitiveCompare>& outputVarDictCpp, map<string, int, CaseInsensitiveCompare>& EMoutputObsDict, vector<EuromodError>& errors,
	string varsOutputRequested, string ILOutputRequested, string outputQueriesStr, bool suppressOutput, map<tuple<string, string>, string> constantsToOverwrite, map<string, string> extraSettings, bool useLogger, map<string, bool> extensions,
	vector<tuple<string, string>> addons, native_country_info* countryInfoPtr, string pathOutput, bool keepInMemory, string breakAfterFunId);



