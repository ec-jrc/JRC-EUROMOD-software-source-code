#pragma once
#include "ModelInfoHandler.h"
#include <gcroot.h>
#include <msclr/marshal_cppstd.h> // for marshaling
#include <string>
#include <vector>
#include "utilities.h"
using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace msclr::interop;
using namespace EM_XmlHandler;
using namespace EM_Executable;
using namespace EM_Common;
using namespace EM_Transformer;


class NativeModelInfoHandler::mImpl {
public: 
	mImpl(const std::string& path) {

			EMPath^ emPath = gcnew EMPath(marshal_as<String^>(path), false, false);
			List<String^>^ errors = gcnew List<String^>();
			if (!File::Exists(emPath->GetExtensionsFilePath(false)) && Directory::Exists(marshal_as<String^>(path)))
				EM3Global::Transform(emPath->GetFolderEuromodFiles(), errors, true);
			infoHandler_ = gcnew ModelInfoHandler(marshal_as<String^>(path));
		
		
		
	}
	mImpl() {

	}
	static bool TranslateToEM3(std::string pathEM, std::string country, std::vector<std::string> addOns) {
		List<String^>^ addOnsCs = gcnew List<String^>();
		for (std::string addOn : addOns) {
			addOnsCs->Add(marshal_as<String^>(addOn));
		}
		return Control::TranslateToEM3(marshal_as<String^>(pathEM), marshal_as<String^>(country), addOnsCs);
	}
	std::vector<std::string> get_countries() {
		std::vector<std::string> countries;
		for each (String^ ctry in infoHandler_->countries) {
			countries.push_back(marshal_as<std::string>(ctry));
		}
		return countries;
	}
	bool IsValidModel() {
		return infoHandler_->IsValidModel();
	}
	ModelInfoHandler^ get_info_handler() {
		return infoHandler_;
	}

	std::string GetExtSWitchInfo(std::string country, std::string sys_name, std::string dataset_name, std::string ext_name, std::map<std::string, std::string>& info) {
		Dictionary<String^, String^>^ infoCs;
		String^ message = infoHandler_->GetExtensionSwitchInfo(marshal_as<String^>(country), marshal_as<String^>(sys_name), marshal_as<String^>(dataset_name), marshal_as<String^>(ext_name), infoCs);
		getMarshalledInfo(infoCs,info);
		return marshal_as<std::string>(message);

	}

private:
	gcroot<ModelInfoHandler^> infoHandler_;
};