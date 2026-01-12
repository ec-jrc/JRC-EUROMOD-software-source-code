#include "ReturnListHandler.h"
#include <sstream>
#include "stplugin.h"
#include "stata_communication.h"
#include <cctype>

char RETURN_LIST_LOCALS[] = "_return_list_locals_connector";
std::string prefix_macro = "_rlm";
/// <summary>
/// Adds a local 
/// </summary>
/// <param name="key">Name of the local</param>
/// <param name="value">Value of the local</param>
void ReturnListHandler::add_local(std::string key, std::string value) {
	locals[key] = value;	
}
/// <summary>
/// Clears all locals
/// </summary>
void ReturnListHandler::clear() {
	locals.clear();
}

std::string replaceSubstring(const std::string& strToModify, const std::string& from, const std::string& to) {
	if (from.empty())
		return strToModify;
	std::string copyStr = strToModify;
	size_t startPos = 0;
	while ((startPos = copyStr.find(from, startPos)) != std::string::npos) {
		copyStr.replace(startPos, from.length(), to);
		// In case 'to' contains 'from', like replacing 'x' with 'yx'
		startPos += to.length();
	}
	return copyStr;
}
std::string escape_str(const std::string& str) {
	return replaceSubstring(str, "$", "\\$");
}
/// <summary>
/// Function that sets a macro containing all the return list macro's to be set.
/// </summary>
void ReturnListHandler::set_macros() {
	std::ostringstream oss;
	bool first = true;
	//we create a unique local for every local to be set in return list
	//we create a local containing the name of the locals to be stored
	for (const auto& kvp : locals) {
		if (!first) oss << " ";
		oss << kvp.first;
		first = false;
		std::string temp_name = prefix_macro + kvp.first;
		char* name_macro = (char*)temp_name.c_str();
		//saving every local to be stored in the return list
		SF_macro_save(name_macro, (char*)escape_str(kvp.second).c_str());
	}
	std::string tempvalue = oss.str();
	char* list_locals = (char*)tempvalue.c_str();
	//saving the local containing the locals
	SF_macro_save(RETURN_LIST_LOCALS, list_locals);
	clear();
}