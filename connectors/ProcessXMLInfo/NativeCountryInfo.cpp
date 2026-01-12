#include "NativeCountryInfoHandler.h"
#include "NativeCountryInfoHandler.impl.h"
#include <string>
//#include <map>
native_country_info::native_country_info(const std::string & path, const std::string & country) 
    :impl_(std::make_unique<impl>(path,country)) 
{
}

native_country_info::native_country_info()
    : impl_(nullptr) { 
}

native_country_info::~native_country_info() = default;

void* native_country_info::GetInfoHandlerPtr() {
    if (impl_) {
        System::IntPtr ptr = System::Runtime::InteropServices::Marshal::GetIUnknownForObject(impl_->GetInfoHandler());
        return ptr.ToPointer();
    }
    return nullptr;
}

std::vector<std::string > native_country_info::get_systems() {
    return impl_->get_systems();
}

std::vector<std::string > native_country_info::get_datasets() {
    return impl_->get_datasets();
}

bool  native_country_info::isValidCountryModel(const std::string& path, const std::string& country) {
    return impl::isValidCountryModel(path,country);
}

std::map<std::string, std::string> native_country_info::GetSystemInfo(const std::string& nameSystem) {
    return impl_->GetSystemInfo(nameSystem);
}

void native_country_info::check_if_recent() {
    impl_->check_if_recent();
}

std::map<std::string, std::string> native_country_info::GetSystemExpandedInfo(const std::string& nameSystem) {
    return impl_->GetSystemExpandedInfo(nameSystem);
}

std::map<std::string, std::string> native_country_info::GetSystemExpandedInfo(const std::string& nameSystem, std::string& outString) {
    return impl_->GetSystemExpandedInfo(nameSystem, outString);
}

std::map<std::string, std::string> native_country_info::GetSystemInfo(const std::string& nameSystem, std::string& outString) {
    return impl_->GetSystemInfo(nameSystem,outString);
}
native_country_info::native_country_info(const native_country_info& other) : impl_(new impl(*other.impl_)) {};
native_country_info::native_country_info(native_country_info&& other) noexcept
    : impl_(std::move(other.impl_)) {
    // No need to set other.impl_ to nullptr; std::move already does that.
}

native_country_info& native_country_info::operator=(const native_country_info& other) {
    if (this != &other) {
        *impl_ = *other.impl_;
    }
    return *this;
}


native_country_info& native_country_info::operator=(native_country_info&& other) noexcept{
    if (this != &other) {
        impl_ = std::move(other.impl_);
    }
    return *this;
}

std::map<std::string, std::string> native_country_info::GetSysParInfo(const std::string& nameSystem, const std::string& idPar) {
    return impl_->GetSysParInfo(nameSystem, idPar);
}

bool native_country_info::SetSysParValue(const std::string& nameSystem, const std::string& idPar, const std::string& value) {
    return impl_->SetSysParValue(nameSystem, idPar,value);
}

std::map<std::string, std::string> native_country_info::GetDatasetInfo(const std::string& nameDataset) {
    return impl_->GetDatasetInfo(nameDataset);
}