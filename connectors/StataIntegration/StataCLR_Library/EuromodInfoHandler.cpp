#include "EurmodInfoHandler.h";
#include <msclr/marshal_cppstd.h>
using namespace EM_Common;
using namespace System::Runtime::InteropServices;
using namespace msclr::interop;
using namespace System::Text;

namespace Euromod {

	EuromodXmlLinker::EurmodXmlLinker() {
		mapping = gcnew Dictionary<String^, ExeXmlReader::ReadCountryOptions>{
			{"CountryId", ExeXmlReader::ReadCountryOptions::COUNTRY},

				// ... Add more static mappings as needed
		};
	}
	bool EuromodXmlLinker::IsLinkable(String^ name) {
		return mapping->ContainsKey(name);
	}
	bool EuromodXmlLinker::Link(String^ name, ExeXmlReader::ReadCountryOptions% linkedKey) {
		return mapping->TryGetValue(name, linkedKey);
	}


	/// <summary>
	/// Constructor for EuromodInfoHandler. Reads in the info from the xml File;
	/// </summary>
	/// <param name="pathEM">path to EUROMOD model</param>
	/// <param name="country">Country Parth</param>
	EuromodInfoHandler::EuromodInfoHandler(string pathEM, string country) {
		EMPath^ emPath = gcnew EMPath(marshal_as<String^>(pathEM), false);
		String^ countryCS = marshal_as<String^>(country);
		String^ pathCountryXml = emPath->GetCountryFilePath(countryCS, false);
		Dictionary<ExeXmlReader::ReadCountryOptions, Dictionary<String^, Dictionary<String^, String^>^>^>^ xmlInfo = gcnew Dictionary<ExeXmlReader::ReadCountryOptions, Dictionary<String^, Dictionary<String^, String^>^>^>();
		List<Tuple<String^, String^, String^>^>^ sysDataCombinations = gcnew List<Tuple<String^, String^, String^>^>();
		List<Tuple<String^, String^>^>^ bestMatchSysDataCombinations = gcnew List<Tuple<String^, String^>^>();
		ExeXmlReader().GetXmlInfoProcessed(pathCountryXml, xmlInfo,
			sysDataCombinations,
			bestMatchSysDataCombinations, ExeXmlReader::ReadCountryOptions(0));
		this->xmlInfo = xmlInfo;
		this->sysDataCombinations = sysDataCombinations;
		this->bestMatchSysDataCombinations = bestMatchSysDataCombinations;
	}
	bool EuromodInfoHandler::getClassInfo(ExeXmlReader::ReadCountryOptions option, Dictionary<String^, Dictionary<String^, String^>^>^% classInfo) {
		return this->xmlInfo->TryGetValue(option, classInfo);
	}
	/// <summary>
	/// This function return as specific piece of Info based on the name of the Identifier and a specific Value
	/// </summary>
	/// <param name="option">option as specified by Enum</param>
	/// <param name="name">Value of the value looked for</param>
	/// <param name="nameIdentifier"></param>
	/// <param name="pieceOfInfo">Dict that is being passed by reference</param>
	/// <returns></returns>
	bool EuromodInfoHandler::getPieceOfInfo(ExeXmlReader::ReadCountryOptions option, string name,string nameIdentifier, Dictionary<String^, String^>^% pieceOfInfo) {
		Dictionary<String^, Dictionary<String^, String^>^>^ classInfo;
		if (!getClassInfo(option, classInfo)) return false;
		for each (auto kvp in classInfo) {
			if (String::Compare(kvp.Value[marshal_as<String^>(nameIdentifier)], marshal_as<String^>(name), StringComparison::OrdinalIgnoreCase)) {
				pieceOfInfo =  kvp.Value;
				return true;
			}
		}
		pieceOfInfo = gcnew Dictionary<String^, String^>();
		return false;
	}

	String^ EuromodInfoHandler::convertInformation(Dictionary<String^, String^>^ pieceOfInfo, String^ header) {
		StringBuilder^ infoStr;
		infoStr->AppendLine(header);
		for each (auto kvp in pieceOfInfo) {
			infoStr->AppendLine(kvp.Key + ": " + kvp.Value);
		}
		return infoStr->ToString();
	}

	bool EuromodInfoHandler::getDataInfo(string datasetName,string&out) {
		
		return true;
	}

}