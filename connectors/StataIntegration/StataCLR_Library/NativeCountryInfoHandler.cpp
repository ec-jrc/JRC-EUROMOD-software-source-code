#include <msclr/marshal_cppstd.h> // for marshaling

// Include the C++/CLI header (generated from your C++/CLI project)
#include "NativeCountryInfoHandler.h"
#include "NativeCountryInfoHandler.impl.h"
// NativeCountryInfoHandler constructor
 NativeCountryInfoHandler::NativeCountryInfoHandler(const std::string& path, const std::string& country) : pImpl(std::make_unique<Impl>(path, country)) {
}

// NativeCountryInfoHandler destructor
NativeCountryInfoHandler::~NativeCountryInfoHandler() = default;

// NativeCountryInfoHandler methods
std::map<std::string, std::string> NativeCountryInfoHandler::GetSystemInfo(const std::string& nameSystem) {
    return pImpl->GetSystemInfo(nameSystem);
}