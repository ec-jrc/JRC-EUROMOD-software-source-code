#ifdef _WIN32
#include <windows.h>
#include <shellapi.h>
#endif

#include <iostream>
#include <algorithm>
#include "EMControl.h"
#include "EMUtilities.h"
#include <fstream>
using namespace std;	// std namespace is assumed in windows, but required in Linux

int main(int argc, char *argv[12])
{
	CEMControl EMControl;
	if (argc == 2 || argc == 4 || argc == 5 || argc == 6)
	{
		if (argc != 2) EMControl.TakeConfigParam(argv[1], // project path
												 argv[2], // system name
											     argv[3], // data name
												 argc >= 5 ? argv[4] : "",	// data path (default: project path subfolder Input)
												 argc == 6 ? argv[5] : ""); // output path (default: project path subfolder Output)
		bool ret = EMControl.GoAhead(argv[1]);
		CEMError::CloseErrLog();
		return ret;
	}
	else
	{
		std::string error = "Programme was called with the wrong number of arguments.\n";
		error += "Option 1: EUROMOD ConfigFilePath\n";
		error += "     e.g. \"C:\\Program Files (x86)\\EUROMOD\\Executable\\EUROMOD\" \"C:\\Euromod\\EuromodFiles_G3.0+\\XMLParam\\Temp\\EMConfigb82cbf33-b7de-4255-8a6a-3fd395f1e5eb.xml\"\n\n";
		error += "Option 2: EUROMOD ProjectPath SystemName DataName [DataPath] [OutputPath]\n";
		error += "     e.g. \"C:\\Program Files (x86)\\EUROMOD\\Executable\\EUROMOD\" \"C:\\Euromod\\EuromodFiles_G3.0+\" \"BE_2014\" \"BE_2015_a1\"\n";
		return CEMError::CodeErr(error, "If you are using the User Interface, try to re-run the Run Tool.");
	}
}

#ifdef _WIN32 //using the system independent function system() instead has the (main) disadvantage that it waits until the started programme terminates (e.g. Excel is closed again)
//for some reason including windows.h and shellapi.h in other cpp-file of this project leads to a lot of error messages
bool CallProgramme(std::string ProgName, std::string Param, double Wait, std::string ProgPath)
{
	SHELLEXECUTEINFO ShellInfo;
	memset(&ShellInfo, 0, sizeof(ShellInfo));
	ShellInfo.cbSize = sizeof(ShellInfo);
	ShellInfo.hwnd = NULL;
	
#ifdef UNICODE		// use temp wchar_t arrays to pass the values to ShellInfo in Unicode
	wchar_t wtext1[MAX_PATH];
	wchar_t wtext2[MAX_PATH];
	wchar_t wtext3[MAX_PATH];
	wchar_t wtext4[MAX_PATH];
	size_t* tmp = 0;
	mbstowcs_s(tmp, wtext1, "open", sizeof("open")+1);
	ShellInfo.lpVerb = wtext1;
	mbstowcs_s(tmp, wtext2, ProgName.c_str(), sizeof(ProgName.c_str())+1);
	ShellInfo.lpFile = wtext2;
	mbstowcs_s(tmp, wtext3, ProgPath.c_str(), sizeof(ProgPath.c_str())+1);
	ShellInfo.lpDirectory = wtext3;
	mbstowcs_s(tmp, wtext4, Param.c_str(), sizeof(Param.c_str())+1);
	ShellInfo.lpParameters = wtext4;
#else				// else directly pass the values to ShellInfo in ASCII
	ShellInfo.lpVerb = "open";
	ShellInfo.lpFile = ProgName.c_str();
	ShellInfo.lpDirectory = ProgPath.c_str();
	ShellInfo.lpParameters = Param.c_str();
#endif

	ShellInfo.nShow = SW_NORMAL;
	ShellInfo.fMask = SEE_MASK_NOCLOSEPROCESS; //necessary if you want to wait for process termination

	int res = ShellExecuteEx(&ShellInfo);
	if(!res) return 0;
	if(Wait) WaitForSingleObject(ShellInfo.hProcess, INFINITE); //wait forever for process to finish
	CloseHandle(ShellInfo.hProcess);
	return 1;
}

std::string GetExePath() //only used for GetEMPath, therefore below comment applies as well
{
	char cExePath[MAX_PATH]="";
#ifdef UNICODE		// use a temp wchar_t array to get the GetModuleFileNameW value (Unicode)
	wchar_t wtext[MAX_PATH];
	size_t* tmp = 0;
	mbstowcs_s(tmp, wtext, cExePath, sizeof(cExePath)+1);
	GetModuleFileNameW(0, wtext, MAX_PATH-1);
#else				// else directly get the GetModuleFileNameA value (ASCII)
	GetModuleFileNameA(0, cExePath, MAX_PATH-1);
#endif
	std::string ExePath=cExePath;
	ExePath=ExePath.substr(0, ExePath.find_last_of('\\'));
    return ExePath;
}

std::string GetEMPath() //should not be used anymore with introduction of config-parameter EMCONTENTPATH (= path to EM file structure as passed by the UI)
{
	std::string EMPath=GetExePath();
	EMPath=EMPath.substr(0, EMPath.find_last_of('\\')+1);
	return EMPath;
}

#endif
