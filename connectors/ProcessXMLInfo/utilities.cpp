#include "utilities.h"

std::map<std::string, std::string> getMarshalledInfo(Dictionary<String^, String^>^ info) {
    std::map<std::string, std::string> output;
    for each (auto kvp in info) {
        output[marshal_as<std::string>(kvp.Key)] = marshal_as<std::string>(XmlHelpers::RemoveCData(kvp.Value)->Replace("$", "\$"));
    }
    return output;
}

void getMarshalledInfo(Dictionary<String^, String^>^ info, std::map<std::string,std::string>& infocpp) {
    for each (auto kvp in info) {
        infocpp[marshal_as<std::string>(kvp.Key)] = marshal_as<std::string>(XmlHelpers::RemoveCData(kvp.Value)->Replace("$", "\$"));
    }
}