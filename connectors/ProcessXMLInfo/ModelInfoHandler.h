#pragma once
#include "NativeCountryInfoHandler.h"
#include <vector>
#ifdef PROCESSXMLINFO_EXPORT
#define DLL_API_XML _declspec(dllexport)
#else
#define DLL_API_XML _declspec(dllimport)
#endif
/// <summary>
/// This class wraps around the Csharp ModelInfo Handler 
/// </summary>
class DLL_API_XML NativeModelInfoHandler {
public:
	std::map<std::string, std::unique_ptr<native_country_info>> countriesDict;
	NativeModelInfoHandler();
	NativeModelInfoHandler(const std::string path);
	~NativeModelInfoHandler();
	std::string path;
	bool is_valid_model();
	void add_countries();
	bool load_country(std::string country);
	bool get_country(std::string country, native_country_info*& countryInfo);
	std::string get_ext_switch_info(std::string country, std::string sys_name, std::string dataset_name, std::string ext_name, std::map<std::string, std::string>& info);
	std::vector<std::string> get_countries();
	// Copy constructor
	NativeModelInfoHandler(const NativeModelInfoHandler& other);

	// Move constructor
	NativeModelInfoHandler(NativeModelInfoHandler&& other) noexcept;
	static bool translate_to_EM3(std::string pathEM, std::string country, std::vector<std::string> addOns);

	// Copy assignment operator
	NativeModelInfoHandler& operator=(const NativeModelInfoHandler& other);

	// Move assignment operator
	NativeModelInfoHandler& operator=(NativeModelInfoHandler&& other) noexcept;

private:
	class mImpl;
	std::unique_ptr<mImpl> mImpl_;
};