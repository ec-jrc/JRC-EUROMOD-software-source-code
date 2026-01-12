// pch.h: This is a precompiled header file.
// Files listed below are compiled only once, improving build performance for future builds.
// This also affects IntelliSense performance, including code completion and many code browsing features.
// However, files listed here are ALL re-compiled if any one of them is updated between builds.
// Do not add files here that you will be updating frequently as this negates the performance advantage.

#ifndef PCH_H
#define PCH_H
#include "utilities.h"
#include "utilitiesCLR.h"
#include "stdio.h"
#include <iostream>
#include "clrLibrary.h"
#include <msclr/auto_gcroot.h>
#include <msclr/marshal_cppstd.h>
#include <vcclr.h>
#include <string>
#include <vector>
#include <sstream>
#include <sys/stat.h>
#include "CommandProcessing.h"
#include <map>
#include "DataHandler.h"
#include "../../ProcessXMLInfo/NativeCountryInfoHandler.h"
#include "../../ProcessXMLInfo/ModelInfoHandler.h"
#include "CaseInsensitiveUtils.h"
//#using <EM_Executable.dll>
//#using <EM_Common.dll>
//#using <EM_XmlHandler.dll>


using namespace System;
using namespace System::IO;
using namespace System::Linq;
using namespace System::Runtime::InteropServices;
using namespace EM_Executable;
using namespace System::Collections::Generic;
using namespace EM_Common;
using namespace msclr::interop;
using namespace System::Text;
using namespace EM_XmlHandler;
using namespace EM_Common;
using namespace EM_Executable;
// add headers that you want to pre-compile here

#endif //PCH_H
