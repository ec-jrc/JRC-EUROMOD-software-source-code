#include "pch.h"
#include "utilities.h"

vector<string> split(string input_str, const char seperator) {
	string segment;
	vector<string> seglist;
	std::stringstream streamStr(input_str);
	while (std::getline(streamStr, segment, seperator)) {
		printf((segment + "\n").c_str());
		seglist.push_back(segment);
	}
	return seglist;
}

vector<int> split_integerstring(string input_str) {
	string segment;
	vector<int> seglist;
	std::stringstream streamStr(input_str);
	while (std::getline(streamStr, segment, '.')) {
		int number = stoi(segment);
		seglist.push_back(number);
	}
	return seglist;
}

std::string rtrim(const std::string& s) {
	auto notspace = std::find_if(s.rbegin(), s.rend(), [](char ch) { return !std::isspace(ch); });
	return std::string(s.begin(), notspace.base());
}

std::string ltrim(const std::string& s) {
	auto notspace = std::find_if(s.begin(), s.end(), [](char ch) { return !std::isspace(ch); });
	return std::string(notspace, s.end());
}
