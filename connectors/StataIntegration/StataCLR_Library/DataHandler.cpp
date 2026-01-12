#include "pch.h"


void DataHandler::addData(Dictionary<String^, cli::array<double, 2>^>^% data, Dictionary<String^, List<String^>^>^% dataHeader) {
	for each (auto kvp in data) {
		returnedData[kvp.Key] = kvp.Value;
	}
	for each (auto kvp in dataHeader) {
		returnedVars[kvp.Key] = kvp.Value;
	}

}
void DataHandler::clearData() {
	returnedData->Clear();
	returnedVars->Clear();
}

Dictionary<String^, cli::array<double, 2>^>^ DataHandler::getData() {
	return returnedData;
}
Dictionary<String^, List<String^>^>^ DataHandler::getHeader() {
	return returnedVars;

}


	
