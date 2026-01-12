#include "pch.h"

gcroot<DataHandler^> managedDataHandler = gcnew DataHandler();
bool ContainsExactlyOne(String^ str, char character) {
	// Use the String::Count method to count occurrences of '=' in the string
	int equalSignCount = 0;

	for each (char c in str) {
		if (c == character) {
			equalSignCount++;
		}
	}

	// Check if the count is exactly one
	return equalSignCount == 1;
}

bool getXmlInfo(string pathEM, string country, string& systemIteratorStr,string& dataIteratorStr, string& isBestMatchIteratorStr, string&bestSystemIteratorStr, string& bestDataIteratorStr) {
	
	auto communicator = gcnew Communicator();
	Dictionary<String^, String^>^ settings = gcnew Dictionary<String^, String^>();
	settings[TAGS::CONFIG_PATH_EUROMODFILES] = marshal_as<String^>(pathEM);
	String^ countryCS = marshal_as<String^>(country);
	settings[TAGS::CONFIG_COUNTRY] = countryCS;
	
	if (!Control::TranslateToEM3(settings, communicator)) return false;
	EMPath^ emPath = gcnew EMPath(marshal_as<String^>(pathEM), false,false);
	
	String^ pathCountryXml = emPath->GetCountryFilePath(countryCS, false);
	if (!File::Exists(pathCountryXml)) return false;
	Dictionary<ReadCountryOptions, Dictionary<String^,Dictionary<String^, String^>^>^>^ xmlInfo = gcnew Dictionary<ReadCountryOptions, Dictionary<String^, Dictionary<String^, String^>^>^>();
	List<Tuple<String^, String^, String^>^>^ sysDataCombinations = gcnew List<Tuple<String^, String^, String^>^>();
	List<Tuple<String^, String^>^>^ bestMatchSysDataCombinations = gcnew List<Tuple<String^, String^>^>();
	if (!ExeXmlReader().GetXmlInfoProcessed(pathCountryXml, xmlInfo,
		sysDataCombinations,
		bestMatchSysDataCombinations, ReadCountryOptions(0)))
		return false;
	vector<tuple<string, string, string>> sysDataCombinationsCpp;
	int ctr = 0;

	for each (auto tup in sysDataCombinations) {
		systemIteratorStr += ctr > 0 ? " " + marshal_as<string>(tup->Item1) : marshal_as<string>(tup->Item1);
		dataIteratorStr += ctr > 0 ? " " + marshal_as<string>(tup->Item2) : marshal_as<string>(tup->Item2);
		isBestMatchIteratorStr += ctr > 0 ? " " + marshal_as<string>(tup->Item3) : marshal_as<string>(tup->Item3);
		ctr++;
	}
	ctr = 0;
	for each (auto tup in bestMatchSysDataCombinations) {
		bestSystemIteratorStr += ctr > 0 ? " " + marshal_as<string>(tup->Item1) : marshal_as<string>(tup->Item1);
		bestDataIteratorStr += ctr > 0 ? " " + marshal_as<string>(tup->Item2) : marshal_as<string>(tup->Item2);
		ctr++;
	}
	return true;
}


int runEM(string system, string pathEM, string dataSetId, string& error_message,string pathData, string country,
	double input_arr[],int length, vector<string> variables,map<string,double*, CaseInsensitiveCompare>& outputDictCpp, map<string,vector<string>, CaseInsensitiveCompare>& outputVarDictCpp, map<string, int, CaseInsensitiveCompare>& EMoutputObsDict, vector<EuromodError>& errorsCpp,
	Tuple<List<String^>^, List<String^>^, List<String^>^, List<String^>^>^ newOutput, List<Tuple<String^, List<String^>^>^>^ outputQueries, bool suppressOutput,
	Dictionary<Tuple<String^, String^>^, String^>^ constantsToOverwrite ,Dictionary<String^,String^>^ extraSettingsDict, bool useLogger,native_country_info* countryInfo,bool keepInMemory) {
	String^ pathEMcs = marshal_as<String^>(pathEM);
	String^ systemCs = marshal_as<String^>(system);
	String^ dataSetIdCs = marshal_as<String^>(dataSetId);
	/*
	
	*/
	// Marhsall the variable list to a csharp equivalent
	List<String^>^ variablesCS = gcnew List<String^>(variables.size());
	for(string var_name : variables) {
		variablesCS->Add(marshal_as<String^>(var_name));
	}

	cli::array< double >^ input_arr_cs = gcnew cli::array<double>(length);
	IntPtr input_arr_ptr = (IntPtr)input_arr;
	Marshal::Copy(input_arr_ptr, input_arr_cs, 0, length);
	
	
	Dictionary<String^, String^>^  settings = gcnew Dictionary<String^, String^>();
	vector<string> syssplit = split(system, '_');
	if ((syssplit.size() > 1) && country == "") {
		country = syssplit[0];
	}
	String^ countryCs = marshal_as<String^>(country);
	if (pathData == "") {
		pathData = pathEM + "\\Input";
	}
	
	String^ pathDataCs = marshal_as<String^>(pathData);

	/*pathDataCs = "R:\\B2\\04 - EUROMOD JRC\\01 - Repository\\03 - Datasets\\All data\\All countries\\";
	pathEMcs = "R:\\B2\\01 - Households\\01 - European Semester\\09 - 2022 - 2023\\Country requests\\PL\\2 - Working Area\\1 - Model\\EUROMOD_MASTER_VERSION_I4.168_new\\";
	pathOutputCs = "c:\\Users\\serruha";
	dataSetIdCs = "PL_2020_b2";
	countryCs = "PL";*/
	settings->Add("PATH_EUROMODFILES", pathEMcs);
	settings->Add("PATH_DATA", pathDataCs);
	settings->Add("PATH_OUTPUT", "");
	settings->Add("COUNTRY", countryCs);
	settings->Add("ID_DATASET", dataSetIdCs);
	settings->Add("ID_SYSTEM", systemCs);
	if (extraSettingsDict != nullptr) 
	for each (auto item in extraSettingsDict) {
		settings[item.Key] = item.Value;
	}
	// We get the countryinfohandler object and pass it to the run function such that the xmlinfo can be used in run function
	void* ptr = countryInfo->GetInfoHandlerPtr();
	System::IntPtr handlerPtr(ptr);
	EM_XmlHandler::CountryInfoHandler^ countryInfoHandlerManaged = static_cast<EM_XmlHandler::CountryInfoHandler^>(System::Runtime::InteropServices::Marshal::GetObjectForIUnknown(handlerPtr));
	countryInfoHandlerManaged->CheckIfRecent();
	Dictionary<String^, String^>^ systemInfo = countryInfoHandlerManaged->GetSystemInfo(systemCs);
	if (systemInfo->Count == 0) { // check if SystemInfo exists
		errorsCpp.push_back(EuromodError(false, "Invalid System name passed"));
		return -1;
	}
	Control^ control = gcnew Control();
	control->SetPreviousData(managedDataHandler->getHeader(), managedDataHandler->getData());
	Dictionary<String^,cli::array<double, 2>^>^% outputDict = gcnew Dictionary<String^, cli::array<double, 2>^>();
	Dictionary<String^,List<String^>^>^% outputVarDict = gcnew Dictionary<String^, List<String^>^>;
	List<Communicator::ErrorInfo^>^% errors = gcnew List<Communicator::ErrorInfo^>();
	bool success = control->RunFromArrayStata(settings, input_arr_cs, variablesCS, outputDict, outputVarDict, errors, countryInfoHandlerManaged, nullptr, nullptr, suppressOutput, newOutput, constantsToOverwrite, outputQueries, useLogger,nullptr);
	for each (Communicator::ErrorInfo ^ error in errors) {
		String^ message = error->message;
		errorsCpp.push_back(EuromodError(error->isWarning, marshal_as<std::string>(message)));
	}
	
	if (!success) return -1;
	if (keepInMemory) {
		managedDataHandler->clearData();
		managedDataHandler->addData(outputDict, outputVarDict);
	}
	
	//control->RunFromArray(settings, input_arr_cs, variablesCS, output, output_vars, nullptr, nullptr, nullptr, nullptr, false, nullptr);

	double temp = 0;
	int counter = 0;
	for each (auto kvp in outputDict) {
		//This is copying the data from a List of Csharp 2D-Arrays in a cplusplus vector of 1D-arrays. 
		auto outputArr = kvp.Value;
		int rows = outputArr->GetLength(0);
		int cols = outputArr->GetLength(1);
		int length = rows * cols;
		// Converts the data from a csharp format to a Cpp 1D array
		cli::array<double>^ output1D = gcnew cli::array<double>(length);
		Buffer::BlockCopy(outputArr, 0, output1D, 0, length * sizeof(double));
		pin_ptr<double> p = &output1D[0]; // Pin the managed array in memory
		double* temp_arr = p;
		//convert key to cpp format
		string keyCpp = marshal_as<string>(kvp.Key);
		outputDictCpp[keyCpp] = temp_arr;
		EMoutputObsDict[keyCpp] = rows;
		// Here we copy the variable names to vector of vector of strings
		List<String^>^ varnames = outputVarDict[kvp.Key];
		vector<string> varnames_cpp;

		
		for each (String^ el in varnames) {
			varnames_cpp.push_back(marshal_as<string>(el));
		}

		outputVarDictCpp[keyCpp] = varnames_cpp;
		counter++;
	}
	



	return 0;
	
}



int runEMfromStata(string system, string pathEM, string dataSetId, string& error_message, string pathData, string country,
	double input_arr[], int length, vector<string> variables, map<string, double*, CaseInsensitiveCompare>& outputDictCpp, map<string, vector<string>, CaseInsensitiveCompare>& outputVarDictCpp, map<string, int, CaseInsensitiveCompare>& EMoutputObsDict, vector<EuromodError>& errors,
	string varsOutputRequested, string ILOutputRequested, string outputQueriesStr ,bool suppressOutput, string constantsToOverwriteStr, string extraSettings, bool useLogger, string extensionString, 
	string addonString, native_country_info* countryInfoPtr , string pathOutput, bool keepInMemory, string breakAfterFunId) {
	Dictionary<Tuple<String^, String^>^, String^>^ constantsToOverwriteDict;
	if (constantsToOverwriteStr != "") {
		constantsToOverwriteDict = gcnew Dictionary<Tuple<String^, String^>^, String^>();
		if (processCommands(constantsToOverwriteStr, constantsToOverwriteDict) != 0) {
			EuromodError error = EuromodError(false, "Error in processing constants. \"" + constantsToOverwriteStr + "\" does not satisfy syntax requirements");
			errors.push_back(error);
			return -1;
		}
	}
	Dictionary<String^, String^>^ extraSettingsDict;
	if(!extraSettings.empty() || !extensionString.empty() || !addonString.empty() || !pathOutput.empty()) extraSettingsDict = gcnew Dictionary<String^, String^>();
	if (!extraSettings.empty()) {
		if (createDictionary(extraSettings, extraSettingsDict) != 0) {
			EuromodError error = EuromodError(false, "Error in processing extra settings. \"" + extraSettings + "\" does not satisfy syntax requirements");
			errors.push_back(error);
			return -1;
		}
	}
	if (!extensionString.empty()) {
		List<String^>^ listOfExtensions = gcnew List<String^>();
		if (parseList(extensionString, listOfExtensions) != 0) {
			EuromodError error = EuromodError(false, "Error in processing extensions. \"" + extensionString + "\" does not satisfy syntax requirements");
			errors.push_back(error);
			return -1;
		}
		for (int i = 0; i < listOfExtensions->Count; i++) {
			if (!ContainsExactlyOne(listOfExtensions[i],'='))
			{
				EuromodError error = EuromodError(false, "Error in processing extensions. \"" + extensionString + "\" does not satisfy syntax requirements");
				errors.push_back(error);
				return -1;

			}
			extraSettingsDict[TAGS::EXTENSION_SWITCH + "_CONNECTOR" + i.ToString()] = listOfExtensions[i];
		}
	}
	if (!addonString.empty()) {
		List<String^>^ listOfAddons = gcnew List<String^>();
		if (parseList(addonString, listOfAddons) != 0) {
			EuromodError error = EuromodError(false, "Error in processing add-ons. \"" + addonString + "\" does not satisfy syntax requirements");
			errors.push_back(error);
			return -1;
		}
		for (int i = 0; i < listOfAddons->Count; i++) {
			if (!ContainsExactlyOne(listOfAddons[i], '|'))
			{
				EuromodError error = EuromodError(false, "Error in processing addons. \"" + addonString + "\" does not satisfy syntax requirements");
				errors.push_back(error);
				return -1;

			}
			extraSettingsDict[TAGS::CONFIG_ADDON + "_CONNECTOR" + i.ToString()] = listOfAddons[i];
		}
		
	}

	if (!pathOutput.empty()) {
		extraSettingsDict[TAGS::CONFIG_PATH_OUTPUT] = marshal_as<String^>(pathOutput);
	}

	List<Tuple<String^, List<String^>^>^>^ outputQueries;
	if (!outputQueriesStr.empty())
	{
		outputQueries = gcnew List<Tuple<String^, List<String^>^>^>();
		if (createListOfTupleOfStr_And_ListOfStr(outputQueriesStr, outputQueries) != 0) {
			EuromodError error = EuromodError(false, "Error in processing extraoutput_info. \"" + outputQueriesStr + "\" does not satisfy syntax requirements");
			errors.push_back(error);
			return -1;
		} 
	}
	Tuple< List<String^>^, List<String^>^,List<String^>^, List<String^>^>^ newOutput = nullptr;
	if (varsOutputRequested != "" || ILOutputRequested != "")
	{
		List<String^>^ varRequestedList = gcnew List<String^>();
		List<String^>^ ILrequestedList = gcnew List<String^>();
		List<String^>^ varGroups = gcnew List<String^>(); //To implement still for stata connector
		List<String^>^ ILGroups = gcnew List<String^>(); //To implement still for stata connector
		if (varsOutputRequested != "") {
			varRequestedList = stringCppToList(varsOutputRequested);
		}
		if (ILOutputRequested != "") {
			ILrequestedList = stringCppToList(ILOutputRequested);
		}

		newOutput = gcnew Tuple<List<String^>^, List<String^>^, List<String^>^, List<String^>^>(varRequestedList, ILrequestedList,varGroups,ILGroups);
	}

	return runEM(system, pathEM, dataSetId, error_message, pathData, country, input_arr, length, variables, outputDictCpp, outputVarDictCpp, EMoutputObsDict, errors,
		newOutput, outputQueries, suppressOutput, constantsToOverwriteDict,extraSettingsDict,useLogger,countryInfoPtr,keepInMemory);


}