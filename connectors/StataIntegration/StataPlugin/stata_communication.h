#pragma once
#include <string>
#include "stplugin.h"
#include <sstream>
using namespace std;
ST_retcode printMessage(std::string message);
void printErrorMessage(std::string message);
void setErrorMessage();
void clearErrorMessage();
