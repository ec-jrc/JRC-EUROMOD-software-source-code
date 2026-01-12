#pragma once
#include <string>
#include <map>
#include <vector>
#include <memory>

#ifdef PROCESSXMLINFO_EXPORT
#define DLL_API_XML _declspec(dllexport)
#else
#define DLL_API_XML _declspec(dllimport)
#endif

#ifdef CLR_LIBRARY
#define CIH_ACCESS _declspec(dllimport)
#else
#define CIH_ACCESS _declspec(dllexport)
#endif

class  native_country_info
{
public:
    DLL_API_XML native_country_info();
    DLL_API_XML native_country_info(const std::string& path, const std::string& country);
    DLL_API_XML ~native_country_info();
    DLL_API_XML std::map<std::string, std::string> GetSystemInfo(const std::string& nameSystem);
    DLL_API_XML std::map<std::string, std::string> GetSystemExpandedInfo(const std::string& nameSystem);
    DLL_API_XML std::map<std::string, std::string> GetSystemInfo(const std::string& nameSystem, std::string& outString);
    DLL_API_XML std::map<std::string, std::string> GetDatasetInfo(const std::string& nameDataset);
    DLL_API_XML std::map<std::string, std::string> GetSystemExpandedInfo(const std::string& nameSystem, std::string& outString);
    DLL_API_XML std::map<std::string, std::string> GetSysParInfo(const std::string& nameSystem, const std::string& idPar);
    DLL_API_XML bool SetSysParValue(const std::string& nameSystem, const std::string& idPar, const std::string& value);
    DLL_API_XML static bool isValidCountryModel(const std::string& path, const std::string& country);
    DLL_API_XML void check_if_recent();
    // Copy constructor
    DLL_API_XML native_country_info(const native_country_info& other);

    // Move constructor
    DLL_API_XML native_country_info(native_country_info&& other) noexcept;
    


    // Copy assignment operator
    DLL_API_XML native_country_info& operator=(const native_country_info& other);

    // Move assignment operator
    DLL_API_XML native_country_info& operator=(native_country_info&& other) noexcept;
    std::vector<std::string > DLL_API_XML get_systems();
    std::vector<std::string > DLL_API_XML get_datasets();
    CIH_ACCESS void* GetInfoHandlerPtr();
private:
    class impl;
    std::unique_ptr<impl> impl_; // Pointer to the implementation
};


