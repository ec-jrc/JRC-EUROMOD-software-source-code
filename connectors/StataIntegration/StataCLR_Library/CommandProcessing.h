#pragma once
#include <iostream>  
#include <vector>
#include <string>
#include <sstream>
#include <regex>
#include <msclr/marshal_cppstd.h>
#include <msclr/auto_gcroot.h>


using namespace std;
using namespace System::Collections::Generic;
using namespace System;
using namespace msclr::interop;

int processCommands(const std::string& s, Dictionary<Tuple<String^, String^>^, String^>^% constantsToOverwrite);
int createDictionary(const std::string& s, Dictionary<String^, String^>^% dict);
int createListOfTupleOfStr_And_ListOfStr(const std::string& s, List<Tuple<String^, List<String^>^>^>^% out);
int parseList(const std::string& s, List<String^>^% list,bool removeAllSpaces = true);