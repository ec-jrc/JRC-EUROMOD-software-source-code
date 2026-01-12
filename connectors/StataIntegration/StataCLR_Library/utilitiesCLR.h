#pragma once

#include <string>
#include <vector>
#include <sstream>
#include <msclr/marshal_cppstd.h>
#include <msclr/auto_gcroot.h>
using namespace std;
using namespace msclr::interop;
using namespace System;
using namespace System::Collections::Generic;

List<String^>^ stringCppToList(string str, const char* sep = "");