using namespace std;
#include <iostream>
#include <sstream>
#include <string>
#include <vector>
#include "../StataCLR_Library/utilities.h"
#include "stplugin.h"
#include "../../ProcessXMLInfo/version.h"
#include "version.h"
char STATA_GLOBAL_VERSION[] = "EUROMOD_CONNECTOR_VERSION";
char EUROMOD_COMMAND_VERSION[] = "EUROMOD_COMMAND_VERSION";
char EUROMOD_SOFTWARE_VERSION[] = "EUROMOD_SOFTWARE_VERSION";
const string MINIMUM_STATA_COMMAND_VERSION = "1.1.0";
bool get_version(string version, int& major, int& minor, int& patch) {
	vector<int> splitted_version = split_integerstring(version);
	if (splitted_version.size() != 3) {
		return false;
	}
	major = splitted_version[0];
	minor = splitted_version[1];
	patch = splitted_version[2];
	return true;
}

bool version_geq(string version1, string version2) {
	int major1, major2, minor1, minor2, patch1, patch2;
	get_version(version1, major1, minor1, patch1);
	get_version(version2, major2, minor2, patch2);
	if ((major1 > major2) || (major1 == major2 && minor1 > minor2) || (major1 == major2 && minor1 == minor2 && patch1 >= patch2))  return true;
	else return false;
}

bool is_right_version() {
	char version[30];
	if (SF_macro_use(STATA_GLOBAL_VERSION, version, 30) != 0) {
		return true;
	}
	else if (strcmp(version,"") == 0) {
		return true;
	}
	return version_geq(get_em_software_version(),string(version));
}

bool is_right_stata_command_version() {
	char stata_command_version[30];
	if (SF_macro_use(EUROMOD_COMMAND_VERSION, stata_command_version, 30) != 0) {
		return false; //if version not defined then the stata commands are too old to be used.
	}
	else if (strcmp(stata_command_version, "") == 0) {
		return false; //if version not defined then the stata commands are too old to be used.
	}

	return version_geq(stata_command_version, MINIMUM_STATA_COMMAND_VERSION);
}

ST_retcode set_euromod_version() {
	string em_version_str = get_em_software_version();
	if (SF_macro_save(EUROMOD_SOFTWARE_VERSION, &em_version_str[0]) == 0) {
		return 0;
	}
	else {
		return -10;
	}
}
	