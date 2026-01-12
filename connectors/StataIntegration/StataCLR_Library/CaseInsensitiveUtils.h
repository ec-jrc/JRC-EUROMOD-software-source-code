// CaseInsensitiveUtils.h

#pragma once

#include <string>
#include <cstddef>  // For std::size_t

#ifdef CLR_LIBRARY
#define clrLibrary_API __declspec(dllexport)

#else
#define clrLibrary_API __declspec(dllimport)

#endif




// Custom hash function for case-insensitive hashing
struct CaseInsensitiveHash {
    std::size_t operator()(const std::string& s) const;
};

// Custom equality function for case-insensitive comparison
struct CaseInsensitiveEqual {
    bool operator()(const std::string& a, const std::string& b) const;
};


 struct clrLibrary_API CaseInsensitiveCompare {
    bool operator()(const std::string& a, const std::string& b) const;
};