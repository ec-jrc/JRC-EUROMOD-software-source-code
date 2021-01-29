#ifndef EMUTILITIES
#define EMUTILITIES

//#define FLEMOSI

#include <string>
#include <vector>
#include <map>
using namespace std;	// std namespace is assumed in windows, but required in Linux

class CEMControl;
class CEMParam;
class CEMModule;

class CEMError {
public:
	static CEMControl *m_Control;
	static int m_CritErrCnt;
	static int m_NonCritErrCnt;
	
	static bool OpenErrLog();
	static void CloseErrLog();
	static bool CodeErr(std::string Err, std::string Solution="no suggestions recorded");
	static bool ErrNotif(std::string Err, std::string Header="", int ForceStop=1);
	static bool CritErr(std::string Err, std::string Location,
					std::string SysName="", std::string PolName="", std::string ModName="",
					std::string ParName="", std::string ParVal="");
	static bool CritErr(std::string Err, CEMParam *Par);
	static bool CritErr(std::string Err, CEMModule *Mod, std::string ParName="", std::string ParVal="");
	static bool NonCritErr(std::string Err, std::string Location,
					std::string SysName="", std::string PolName="", std::string ModName="",
					std::string ParName="", std::string ParVal="", std::string Handling="");
	static bool NonCritErr(std::string Err, CEMParam *Par, std::string Handling="");
	static bool NonCritErr(std::string Err, CEMModule *Mod, std::string ParName="", std::string ParVal="", std::string Handling="");
	static void ErrSum(std::string Sum);

private:
	static std::ofstream m_ErrLog;
	static bool m_ErrLogOpened;
	static bool Err(bool Critical, std::string Err, std::string Location, std::string SysName,
			std::string PolName, std::string ModName, std::string ParName, std::string ParVal, std::string Handling="");
};

class CEMUtilities {
public:
	static std::vector<std::string> TokeniseLine(std::string line, std::string acro, char delim='\t', bool lcase=true);
	static std::string LCase(std::string str);
	static std::string Trim(std::string str, bool EraseTab=0);
	static std::string ClearEnds(std::string str); //a probably better version of the Trim above (just being reluctant to change a function that's used in core-code)
	static std::string Replace(std::string str, std::string what, std::string by);
	static std::string ReplacePercent(std::string str);
	static std::string DoubleToStr(double num, bool Param=1);
	static std::string IntToStr(int num);
	static double StrToDouble(std::string s, bool *err=NULL, bool Param=1);
	static int StrToInt(std::string s, bool *err=NULL);
	static std::string RemoveQuotes(std::string s);
	static std::string CheckPathName(std::string path);
	static bool CheckCurrency(int &euro, std::string ParVal, std::string Sheet, int Line, std::string ParName, std::string System="");
	static bool CheckYesNo(double &Yes, std::string ParVal, std::string Sheet, int Line, std::string ParName);
	static bool CheckYesNo(double &Yes, CEMParam *Par);
	static int CheckYesNo(std::string str);
	static bool CheckOnOff(double &OnOff, CEMParam *Par, std::string ParVal="", std::string Location="", std::string SysName="", std::string PolName="", std::string ModName="");
	static double Round(double Amount, double RdBase, int AlwaysUpDown=0);
	static bool ConvertMonthly(double &Val, char Period, CEMParam *Par);
	static bool ExtractPeriod(std::string &Val, std::string &Period, bool NoRemainder=0);
	static std::string IdToLocation(std::string Id, bool LineOnly=0);
	static std::string ConvertLocPeriod(std::string s, bool Param);
	static std::string GenId(std::string loc, std::string id, int line);
	static void ReplaceXY(std::string &ParName, CEMModule *Mod, std::map<std::string, int> &PlaceHolder);
	static bool DoesValueMatchPattern(std::string pattern, std::string value, bool matchCase=0);
	static std::string AppendExtension(std::string fileName, std::string extension="txt", bool ReplaceExistingExtension=1);
	static std::string RemoveExtension(std::string fileName);
	static std::string ReplaceAbsMaxMin(std::string formula);
	static std::string ExtractSystemYear(std::string systemName, bool noWarning = 0);

	//decimal sign (. or ,) handling and conversion
	static char m_DecSignData; //decimal sign of data
	static char m_DecSignParam; //decimal sign of parameters
	static bool m_NeedConvert_ParamDecSign;
	static bool m_NeedConvert_DataDecSign;
	static void InitDecSigns();
	static bool CheckDecSign(std::string DecSign, std::string FileName, bool Param);
};

struct XMLAtr {
public:
	std::string Name, Val;
	XMLAtr() { Name=Val=""; }
};
class TiXmlNode;

class CEMXML {
private:
	bool Load(TiXmlNode *Node);
	CEMXML *GetNode(std::string Name, std::string Id, XMLAtr *Ident1, XMLAtr *Ident2);

public:
	std::string m_Name;
	std::vector<CEMXML*> m_Ch;
	std::vector<XMLAtr> m_Atr;

	CEMXML() { m_Name=""; m_Ch.clear(); m_Atr.clear(); }
	~CEMXML();
	bool LoadFile(std::string FileName);
	CEMXML *GetNode(std::string Name);
	CEMXML *GetNode(std::string Name, std::string Id);
	CEMXML *GetNode(std::string Name, XMLAtr *Ident1, XMLAtr *Ident2=NULL);
	std::string GetAtrVal(std::string Name);
};

#ifdef _WIN32
bool CallProgramme(std::string ProgName, std::string Param, double Wait=0, std::string ProgPath="");
std::string GetExePath(); //outdated (see EMMain.cpp)
std::string GetEMPath(); //outdated (see EMMain.cpp)
#endif

#endif
