#pragma once
#include <msclr/marshal_cppstd.h> // for marshaling
#include <string>
#include <vector>
#include <map>
using namespace EM_XmlHandler;
using namespace System;
using namespace System::Collections::Generic;
using namespace msclr::interop;
std::map<std::string, std::string> getMarshalledInfo(Dictionary<String^, String^>^ info);
void getMarshalledInfo(Dictionary<String^, String^>^ info, std::map<std::string, std::string>& infocpp);