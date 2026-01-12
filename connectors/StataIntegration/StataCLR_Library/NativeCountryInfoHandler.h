#pragma once
#include <string>
#include <map>
#ifdef TEST
#define NAT_API __declspec(dllexport)

#else
#define NAT_API __declspec(dllimport)

#endif


class  NAT_API  NativeCountryInfoHandler {
public:
     NativeCountryInfoHandler(const std::string& path, const std::string& country);
     ~NativeCountryInfoHandler();

    std::map<std::string, std::string> GetSystemInfo(const std::string& nameSystem);
    
private:
    // Forward declaration of the implementation class
    class Impl;
    std::unique_ptr<Impl> pImpl; // Pointer to the implementation
};


