
#pragma once
#ifdef CLR_LIBRARY
#define clrLibrary_API __declspec(dllexport)

#else
#define clrLibrary_API __declspec(dllimport)

#endif

using namespace System::Collections::Generic;
using namespace EM_XmlHandler;
using namespace System;
using namespace std;
#include<vector>;
#include<string>;
// This class serves as a bridge between csharp and c++ in order to retrieve information from EUROMOD model that is encoded in the XML
namespace Euromod {
	private ref class EuromodXmlLinker {
		Dictionary<String^, ExeXmlReader::ReadCountryOptions>^ mapping;
	public:
		EuromodXmlLinker();
		bool IsLinkable(String^ nameField);
		bool Link(String^ nameField, ExeXmlReader::ReadCountryOptions% key);

	};
	public ref class clrLibrary_API EuromodInfoHandler {
	private:
		EuromodXmlLinker ^ linker = gcnew EuromodXmlLinker();
		Dictionary<ExeXmlReader::ReadCountryOptions, Dictionary<String^, Dictionary<String^, String^>^>^>^ xmlInfo;
		List<Tuple<String^, String^, String^>^>^ sysDataCombinations;
		List<Tuple<String^, String^>^>^ bestMatchSysDataCombinations;
		bool getClassInfo(ExeXmlReader::ReadCountryOptions option, Dictionary<String^, Dictionary<String^, String^>^>^% classInfo);
		bool getPieceOfInfo(ExeXmlReader::ReadCountryOptions option, string name, string nameIdentifier ,Dictionary<String^, String^>^% pieceOfInfo);
		String^ convertInformation(Dictionary<String^, String^>^ pieceOfInfo, String^ header);


	public:
		EuromodInfoHandler();
		string getSystemInfo(string systemName);
		bool getDataInfo(string datasetName, string& test );
		string getSystemDataInfo(string systemName, string datasetName);
		EuromodInfoHandler(string pathEM, string country);
	};

}