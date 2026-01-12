// CaseInsensitiveUtils.cpp

#include "CaseInsensitiveUtils.h"
#include <algorithm> // For std::tolower
#include <functional> // For std::hash

std::size_t CaseInsensitiveHash::operator()(const std::string& s) const {
    std::size_t h = 0;
    for (char c : s) {
        h ^= std::hash<char>()(std::tolower(static_cast<unsigned char>(c))) + 0x9e3779b9 + (h << 6) + (h >> 2);
    }
    return h;
}

bool CaseInsensitiveEqual::operator()(const std::string& a, const std::string& b) const {
    return std::equal(a.begin(), a.end(), b.begin(), b.end(),
        [](char c1, char c2) { return std::tolower(static_cast<unsigned char>(c1)) == std::tolower(static_cast<unsigned char>(c2)); });
}

bool CaseInsensitiveCompare::operator()(const std::string& a, const std::string& b) const {
    return std::lexicographical_compare(
        a.begin(), a.end(),
        b.begin(), b.end(),
        [](unsigned char c1, unsigned char c2) {
            return std::tolower(c1) < std::tolower(c2);
        }
    );
}