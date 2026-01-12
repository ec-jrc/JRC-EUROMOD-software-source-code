
#include <string>
#include <vector>
#include <sstream>
using namespace std;
#ifdef CLR_LIBRARY
	#define clrLibrary_API __declspec(dllexport)
#else
	#define clrLibrary_API __declspec(dllimport)
#endif

clrLibrary_API vector<string> split(string input_str, const char seperator);
clrLibrary_API vector<int> split_integerstring(string input_str);
clrLibrary_API std::string ltrim(const std::string& s);
clrLibrary_API std::string rtrim(const std::string& s);


