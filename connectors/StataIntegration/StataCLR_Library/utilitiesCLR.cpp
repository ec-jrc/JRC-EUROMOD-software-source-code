#include "pch.h"
#include "utilitiesCLR.h"


List<String^>^ stringCppToList(string str, const char* sep) {
	// first convert character array to type of csharp
	cli::array<wchar_t>^ sep_cs = marshal_as<String^>(string(sep))->ToCharArray();
	// split using csharp string function
	return (gcnew List<String^>(marshal_as<String^>(str)->Split(sep_cs)));
}