#include "ModelInfoHandler.h"
#include "ModelInfoHandler.impl.h"
#include <filesystem>
#include <string>
namespace fs = std::filesystem;
/// <summary>
/// Implementation of the method that loads a country
/// </summary>
/// <param name="country"></param>
/// <returns></returns>
bool NativeModelInfoHandler::load_country(std::string country) {
	if (countriesDict.find(country) == countriesDict.end()) {
		if (!native_country_info::isValidCountryModel(this->path, country)) return false;
		auto countryInfoPtr = std::make_unique<native_country_info>(this->path, country);
		countriesDict[country] = std::move(countryInfoPtr);
	}
	return true;
}

std::vector<std::string> NativeModelInfoHandler::get_countries() {
	return mImpl_->get_countries();
}

NativeModelInfoHandler::NativeModelInfoHandler()
	: mImpl_(nullptr) {
}

bool NativeModelInfoHandler::get_country(std::string country,native_country_info*& countryInfoPtr) {
	auto it = countriesDict.find(country);
	if (it != countriesDict.end()) {
		countryInfoPtr = it->second.get();
		return true;
	}
	else {
		if (!native_country_info::isValidCountryModel(this->path,country)) return false;
		else {
			auto countryInfo = std::make_unique<native_country_info>(path, country);
			countryInfoPtr = countryInfo.get();
			countriesDict[country] = std::move(countryInfo);
			return true;
		}
	}
}

/// <summary>
/// Constructor in CPP. Implementation Happens in ModelInfoHandler.Impl.h using pImpl idiom
/// </summary>
/// <param name="_path"></param>
NativeModelInfoHandler::NativeModelInfoHandler(std::string _path) : path(_path) ,mImpl_(std::make_unique<mImpl>(path)) {
	
}

NativeModelInfoHandler::~NativeModelInfoHandler() = default;

bool NativeModelInfoHandler::is_valid_model() {
	return mImpl_->IsValidModel();
}

bool NativeModelInfoHandler::translate_to_EM3(std::string pathEM, std::string country, std::vector<std::string> addOns) {
	return mImpl::TranslateToEM3(pathEM, country, addOns);
}

void NativeModelInfoHandler::add_countries() {

	fs::path countriesPath = fs::path(path) / "XMLParam" / "Countries";
	if (!fs::exists(countriesPath)) throw std::exception("Path does not exist");

	for (const auto& entry : fs::directory_iterator(countriesPath)) {
		std::string path_country = entry.path().filename().string();
		if (path_country.size() == 2) {
			this->load_country(path_country);
		}
	}
}

NativeModelInfoHandler::NativeModelInfoHandler(const NativeModelInfoHandler& other) : mImpl_(new mImpl(*other.mImpl_)) {};
NativeModelInfoHandler::NativeModelInfoHandler(NativeModelInfoHandler&& other) noexcept
	: mImpl_(std::move(other.mImpl_)) {
	// No need to set other.mImpl_ to nullptr; std::move already does that.
}

NativeModelInfoHandler& NativeModelInfoHandler::operator=(const NativeModelInfoHandler& other) {
	if (this != &other) {
		*mImpl_ = *other.mImpl_;
	}
	return *this;
}


NativeModelInfoHandler& NativeModelInfoHandler::operator=(NativeModelInfoHandler&& other) noexcept {
	if (this != &other) {
		mImpl_ = std::move(other.mImpl_);
	}
	return *this;
}

std::string NativeModelInfoHandler::get_ext_switch_info(std::string country, std::string sys_name, std::string dataset_name, std::string ext_name, std::map<std::string, std::string>& info) {
	return mImpl_->GetExtSWitchInfo(country, sys_name,  dataset_name, ext_name,  info);
}