#include "pch.h"
#include "c_api.h"

#include <map>
#include <string>
#include <vector>
#include <tuple>
#include <new>
#include "euromoderror.h"
using namespace Euromod;

// Match the exact types used by runEMfromCpp_C
using OutMap       = std::map<std::string, double*, CaseInsensitiveCompare>;
using ObsMap       = std::map<std::string, int, CaseInsensitiveCompare>;
using VarVecMap    = std::map<std::string, std::vector<std::string>, CaseInsensitiveCompare>;
using StrStrMap    = std::map<std::string, std::string>;
using ConstantsMap = std::map<std::tuple<std::string, std::string>, std::string>;
using ErrorsVec = std::vector<EuromodError>;

// Store message strings to keep them alive for c_str() returns
struct ErrorsVecWrapper {
    ErrorsVec vec;
    std::vector<std::string> message_cache;
};

// Wrapper for OutMap with key cache
struct OutMapWrapper {
    OutMap map;
    mutable std::vector<std::string> key_cache;
    mutable bool cache_valid = false;
    
    void invalidate_cache() { cache_valid = false; }
    
    void ensure_cache() const {
        if (!cache_valid) {
            key_cache.clear();
            for (const auto& kv : map) {
                key_cache.push_back(kv.first);
            }
            cache_valid = true;
        }
    }
};

// Wrapper for VarVecMap with key cache
struct VarVecMapWrapper {
    VarVecMap map;
    mutable std::vector<std::string> key_cache;
    mutable bool cache_valid = false;
    
    void invalidate_cache() { cache_valid = false; }
    
    void ensure_cache() const {
        if (!cache_valid) {
            key_cache.clear();
            for (const auto& kv : map) {
                key_cache.push_back(kv.first);
            }
            cache_valid = true;
        }
    }
};

// Add these type aliases at the top with the others
using ExtensionsMap = std::map<std::string, std::string>;  // Adjust based on actual type
using AddonsTuple = std::tuple<std::string, std::string>;
using AddonsArray = std::vector<AddonsTuple>;

// Wrapper for addons to provide both vector and raw array access
struct AddonsWrapper {
    AddonsArray vec;
    
    void* data() {
        return vec.empty() ? nullptr : vec.data();
    }
    
    int size() const {
        return static_cast<int>(vec.size());
    }
};

#pragma managed(push, off)

extern "C" {

// Updated numeric outputs to use wrapper
EUROMOD_API void* em_out_map_new()  { return new (std::nothrow) OutMapWrapper(); }
EUROMOD_API void  em_out_map_free(void* m) { delete static_cast<OutMapWrapper*>(m); }
EUROMOD_API void  em_out_map_put(void* m, const char* key, double* buf) {
    if (!m || !key) return;
    auto wrapper = static_cast<OutMapWrapper*>(m);
    wrapper->map[std::string(key)] = buf;
    wrapper->invalidate_cache();
}

// Need to provide a way to get the raw map pointer for C++ function call
EUROMOD_API void* em_out_map_get_raw(void* m) {
    if (!m) return nullptr;
    return &static_cast<OutMapWrapper*>(m)->map;
}

// Existing obs lengths
EUROMOD_API void* em_obs_map_new()  { return new (std::nothrow) ObsMap(); }
EUROMOD_API void  em_obs_map_free(void* m) { delete static_cast<ObsMap*>(m); }
EUROMOD_API void  em_obs_map_put(void* m, const char* key, int len) {
    if (!m || !key) return;
    (*static_cast<ObsMap*>(m))[std::string(key)] = len;
}

// Updated outputVarDictCpp to use wrapper
EUROMOD_API void* em_varvec_map_new() { return new (std::nothrow) VarVecMapWrapper(); }
EUROMOD_API void  em_varvec_map_free(void* m) { delete static_cast<VarVecMapWrapper*>(m); }

EUROMOD_API void  em_varvec_map_put_empty(void* m, const char* key) {
    if (!m || !key) return;
    auto wrapper = static_cast<VarVecMapWrapper*>(m);
    (void)wrapper->map.try_emplace(std::string(key));
    wrapper->invalidate_cache();
}

// Get raw map pointer for C++ function call
EUROMOD_API void* em_varvec_map_get_raw(void* m) {
    if (!m) return nullptr;
    return &static_cast<VarVecMapWrapper*>(m)->map;
}

EUROMOD_API int em_varvec_map_vec_len(void* m, const char* key) {
    if (!m || !key) return 0;
    auto& map = static_cast<VarVecMapWrapper*>(m)->map;
    auto it = map.find(std::string(key));
    if (it == map.end()) return 0;
    const auto& vec = it->second;
    return static_cast<int>(vec.size());
}

EUROMOD_API const char* em_varvec_map_vec_get(void* m, const char* key, int index) {
    if (!m || !key || index < 0) return nullptr;
    auto& map = static_cast<VarVecMapWrapper*>(m)->map;
    auto it = map.find(std::string(key));
    if (it == map.end()) return nullptr;
    auto& vec = it->second;
    size_t i = static_cast<size_t>(index);
    if (i >= vec.size()) return nullptr;
    return vec[i].c_str();
}

// NEW: extraSettings (map<string,string>)
EUROMOD_API void* em_strstr_map_new() { return new (std::nothrow) StrStrMap(); }
EUROMOD_API void  em_strstr_map_free(void* m) { delete static_cast<StrStrMap*>(m); }
EUROMOD_API void  em_strstr_map_put(void* m, const char* key, const char* value) {
    if (!m || !key || !value) return;
    (*static_cast<StrStrMap*>(m))[std::string(key)] = std::string(value);
}

// NEW: constantsToOverwrite (map<tuple<string,string>,string>)
EUROMOD_API void* em_constants_map_new() { return new (std::nothrow) ConstantsMap(); }
EUROMOD_API void  em_constants_map_free(void* m) { delete static_cast<ConstantsMap*>(m); }
EUROMOD_API void  em_constants_map_put(void* m, const char* key1, const char* key2, const char* value) {
    if (!m || !key1 || !key2 || !value) return;
    (*static_cast<ConstantsMap*>(m))[std::make_tuple(std::string(key1), std::string(key2))] = std::string(value);
}

// NEW: errors vector helpers - using wrapper to cache messages
EUROMOD_API void* em_errors_vec_new() { 
    return new (std::nothrow) ErrorsVecWrapper(); 
}

EUROMOD_API void em_errors_vec_free(void* v) { 
    delete static_cast<ErrorsVecWrapper*>(v); 
}

EUROMOD_API int em_errors_vec_count(void* v) {
    if (!v) return 0;
    auto& wrapper = *static_cast<ErrorsVecWrapper*>(v);
    return static_cast<int>(wrapper.vec.size());
}

// Helper to ensure messages are cached
static void ensure_messages_cached(ErrorsVecWrapper* wrapper) {
    if (wrapper->message_cache.size() < wrapper->vec.size()) {
        wrapper->message_cache.clear();
        wrapper->message_cache.reserve(wrapper->vec.size());
        for (const auto& err : wrapper->vec) {
            wrapper->message_cache.push_back(err.getMessage());
        }
    }
}

EUROMOD_API int em_errors_vec_get_is_warning(void* v, int index) {
    if (!v || index < 0) return -1;
    auto wrapper = static_cast<ErrorsVecWrapper*>(v);
    size_t i = static_cast<size_t>(index);
    if (i >= wrapper->vec.size()) return -1;
    return wrapper->vec[i].getIsWarning() ? 1 : 0;
}

EUROMOD_API const char* em_errors_vec_get_message(void* v, int index) {
    if (!v || index < 0) return nullptr;
    auto wrapper = static_cast<ErrorsVecWrapper*>(v);
    size_t i = static_cast<size_t>(index);
    if (i >= wrapper->vec.size()) return nullptr;
    
    // Cache all messages if not already cached
    ensure_messages_cached(wrapper);
    
    if (i >= wrapper->message_cache.size()) return nullptr;
    return wrapper->message_cache[i].c_str();
}

// We need to get the raw vector pointer to pass to runEMfromCpp_C
EUROMOD_API void* em_errors_vec_get_raw_vector(void* v) {
    if (!v) return nullptr;
    auto wrapper = static_cast<ErrorsVecWrapper*>(v);
    return &wrapper->vec;
}

// Updated helpers to iterate over populated outputDict
EUROMOD_API int em_out_map_key_count(void* m) {
    if (!m) return 0;
    auto wrapper = static_cast<OutMapWrapper*>(m);
    return static_cast<int>(wrapper->map.size());
}

EUROMOD_API const char* em_out_map_get_key(void* m, int index) {
    if (!m || index < 0) return nullptr;
    auto wrapper = static_cast<OutMapWrapper*>(m);
    wrapper->ensure_cache();
    
    if (static_cast<size_t>(index) >= wrapper->key_cache.size()) return nullptr;
    return wrapper->key_cache[index].c_str();
}

EUROMOD_API double* em_out_map_get_buffer(void* m, const char* key) {
    if (!m || !key) return nullptr;
    auto& map = static_cast<OutMapWrapper*>(m)->map;
    auto it = map.find(std::string(key));
    if (it == map.end()) return nullptr;
    return it->second;
}

// Updated helpers to iterate over populated outputVarDict
EUROMOD_API int em_varvec_map_key_count(void* m) {
    if (!m) return 0;
    auto wrapper = static_cast<VarVecMapWrapper*>(m);
    return static_cast<int>(wrapper->map.size());
}

EUROMOD_API const char* em_varvec_map_get_key(void* m, int index) {
    if (!m || index < 0) return nullptr;
    auto wrapper = static_cast<VarVecMapWrapper*>(m);
    wrapper->ensure_cache();
    
    if (static_cast<size_t>(index) >= wrapper->key_cache.size()) return nullptr;
    return wrapper->key_cache[index].c_str();
}

// NEW: helper to get length from EMoutputObsDict
EUROMOD_API int em_obs_map_get_len(void* m, const char* key) {
    if (!m || !key) return 0;
    auto& map = *static_cast<ObsMap*>(m);
    auto it = map.find(std::string(key));
    if (it == map.end()) return 0;
    return it->second;
}

// NEW: extensions map helpers
EUROMOD_API void* em_extensions_map_new() { 
    return new (std::nothrow) ExtensionsMap(); 
}

EUROMOD_API void em_extensions_map_free(void* m) { 
    delete static_cast<ExtensionsMap*>(m); 
}

EUROMOD_API void em_extensions_map_put(void* m, const char* key, const char* value) {
    if (!m || !key || !value) return;
    (*static_cast<ExtensionsMap*>(m))[std::string(key)] = std::string(value);
}

// NEW: addons array helpers
EUROMOD_API void* em_addons_array_new() { 
    return new (std::nothrow) AddonsWrapper(); 
}

EUROMOD_API void em_addons_array_free(void* a) { 
    delete static_cast<AddonsWrapper*>(a); 
}

EUROMOD_API void em_addons_array_push(void* a, const char* first, const char* second) {
    if (!a || !first || !second) return;
    auto wrapper = static_cast<AddonsWrapper*>(a);
    wrapper->vec.emplace_back(std::string(first), std::string(second));
}

EUROMOD_API int em_addons_array_size(void* a) {
    if (!a) return 0;
    return static_cast<AddonsWrapper*>(a)->size();
}

EUROMOD_API void* em_addons_array_data(void* a) {
    if (!a) return nullptr;
    return static_cast<AddonsWrapper*>(a)->data();
}

// NEW: country info helpers
EUROMOD_API void* em_country_info_new() { 
    return new (std::nothrow) native_country_info(); 
}

EUROMOD_API void em_country_info_free(void* info) { 
    delete static_cast<native_country_info*>(info); 
}

EUROMOD_API native_country_info* em_country_info_get_ptr(void* info) {
    if (!info) return nullptr;
    return static_cast<native_country_info*>(info);
}

} // extern "C"

#pragma managed(pop)