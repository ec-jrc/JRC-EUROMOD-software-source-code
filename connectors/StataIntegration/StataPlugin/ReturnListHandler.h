#pragma once
// This class will contain ReturnList Macro's that need to be generated
#include<map>
#include<string>
class  ReturnListHandler {
public:
	void add_local(std::string key, std::string value);
	void set_macros();
	void clear();
private:
	std::map<std::string, std::string> locals;
};

extern ReturnListHandler returnListHandler;
	
