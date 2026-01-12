#pragma once
#include "version.h"
#include <gcroot.h>
#include <msclr/marshal_cppstd.h> // for marshaling
#include <string>
using namespace msclr::interop;
using namespace System;
using namespace System::Collections::Generic;
using namespace EM_Common;

std::string get_em_software_version() {
	return marshal_as<std::string>(DefGeneral::UI_VERSION);
}