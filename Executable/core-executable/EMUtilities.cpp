#include "EMUtilities.h"
#include <iostream>
#include <fstream>
#include <stdlib.h>
#include <time.h>
#include <string.h>
#include "EMControl.h"
#include "EMParam.h"
#include "tinyxml.h"

/********************************************************************************************
 functions class CEMError
********************************************************************************************/
std::ofstream CEMError::m_ErrLog;
bool CEMError::m_ErrLogOpened=0;
CEMControl *CEMError::m_Control=NULL;
int CEMError::m_CritErrCnt=0;
int CEMError::m_NonCritErrCnt=0;
char CEMUtilities::m_DecSignData='.';
char CEMUtilities::m_DecSignParam='.';
bool CEMUtilities::m_NeedConvert_ParamDecSign=1;
bool CEMUtilities::m_NeedConvert_DataDecSign=1;

bool CEMError::CodeErr(std::string Err, std::string Solution)
{
	std::cerr << std::endl << "____________________________________________________________________________________" << std::endl << std::endl;
	std::cerr << "EUROMOD ENCOUNTERED A CRITCAL ERROR AND WILL BE TERMINATED." << std::endl;
	std::cerr << "Please note, that this seems to be a technical error, which may need resolving by the EUROMOD team." << std::endl << std::endl;
	std::cerr << "Error: " << Err << std::endl;
	std::cerr << "Trouble shooting: " << Solution << std::endl << std::endl;
	std::cerr << "Please, do not miss to report the error to the EUROMOD team if you do not think you have caused it yourself.";
	std::cerr << std::endl << "____________________________________________________________________________________" << std::endl << std::endl;
	std::cerr << "Press any key to continue." << std::endl << std::flush;

#ifndef FLEMOSI
	if (m_Control->m_InputTXT) //new user interface does not use console
	{
		char c;
		std::cin >> c;
	}
#endif

	return 0;
}

bool CEMError::ErrNotif(std::string Err, std::string Header, int ForceStop)
{
	std::cerr << std::endl << "____________________________________________________________________________________" << std::endl << std::endl;
	if(Header=="") std::cerr <<  "EUROMOD ENCOUNTERED A CRITICAL ERROR AND WILL BE TERMINATED." << std::endl;
	else std::cerr << Header << std::endl;
	std::cerr << Err << std::endl;
	std::cerr << "____________________________________________________________________________________" << std::endl << std::endl << std::flush;
	if(!ForceStop) return 0;
	std::cerr << "Press any key to continue." << std::endl << std::flush;
	if (m_Control->m_InputTXT) //new user interface does not use console
	{
		char c;
		std::cin >> c;
	}
	return 0;
}

bool CEMError::OpenErrLog()
{
	if(m_ErrLogOpened) return 1;
	m_ErrLog.open(m_Control->m_ErrLogFile.c_str(), std::ios_base::out | std::ios_base::app);
	if(!m_ErrLog.is_open()) return CodeErr("Error logfile '" + m_Control->m_ErrLogFile + "' could not be opened.", "Try to re-run the EUROMOD run tool.");
	m_ErrLog.seekp(std::ios_base::end);
	m_ErrLog << "=====================================================================================================================" << std::endl;
	m_ErrLog << "EUROMOD ERROR LOG FOR " << m_Control->m_Country << std::endl;
	m_ErrLog << "Dataset: " << m_Control->m_DataSet << "; EM-Version: " << m_Control->m_EMVersion << "; Executable-Version: "
		<< m_Control->m_UIVersion.c_str() //formerly: m_ExeVersion.c_str(), own version number not used anymore since UI and executable form one software bundle
		<< "; User-Interface-Version: " << m_Control->m_UIVersion << std::endl;
	m_ErrLog << "=====================================================================================================================" << std::endl;
	m_ErrLogOpened=1;
	return 1;
}

void CEMError::CloseErrLog()
{
	if(m_ErrLog.is_open())
	{
		m_ErrLog << std::endl << std::endl; //for possible further runs, which append to this log
		m_ErrLog.close();
	}
}

bool CEMError::CritErr(std::string Err, CEMParam *Par)
{
	std::string Location="";
	std::string SysName="";
	std::string PolName="";
	std::string ModName="";
	std::string ParName="";
	std::string ParValue="";
	if(Par)
	{
		if(CEMControl::m_InputTXT) //needs refactoring
		{
			if(Par->m_System) SysName=Par->m_System->m_Name;
			if(Par->m_Module) { ModName=Par->m_Module->m_Name; PolName=Par->m_Module->m_PolName; }
			if(Par->m_Id=="") ModName+= " (starting at " + CEMUtilities::IdToLocation(Par->m_Module->m_Id, 1) + ")";
			ParName=Par->m_Name;
			if(Par->m_Id!="") ParName+=" (" + CEMUtilities::IdToLocation(Par->m_Id, 1) + ")";
			ParValue=Par->m_strValue;
		}
		else
		{
			if(Par->m_System) SysName = Par->m_System->m_Name;
			std::string parRow = "";
			if(Par->m_Module)
			{
				std::string polRow = "; row " + CEMUtilities::IntToStr(Par->m_Module->m_PolOrder);
				std::string modRow = polRow + "." + CEMUtilities::IntToStr(Par->m_Module->m_Order);
				parRow = (Par->m_Order <= 0) ? "" : modRow + "." + CEMUtilities::IntToStr(Par->m_Order);
				PolName = Par->m_Module->m_PolName + polRow;
				ModName = Par->m_Module->m_Name + modRow;
			}
			ParName = Par->m_Name + parRow;
			ParValue = Par->m_strValue;
			Location = (Par->m_Id == "op") ? "" : Par->m_Id;
		}
	}
	return CritErr(Err, Location, SysName, PolName, ModName, ParName, ParValue);
}

bool CEMError::CritErr(std::string Err, CEMModule *Mod, std::string ParName, std::string ParVal)
{
	std::string Location="";
	std::string SysName="";
	std::string PolName="";
	std::string ModName="";
	if(Mod)
	{
		if(CEMControl::m_InputTXT) //needs refactoring
		{
			if(Mod->m_System) SysName=Mod->m_System->m_Name;
			PolName=Mod->m_PolName;
			ModName=Mod->m_Name + " (starting at " + CEMUtilities::IdToLocation(Mod->m_Id, 1) + ")";
		}
		else
		{
			if(Mod->m_System) SysName = Mod->m_System->m_Name;
			std::string polRow = "; row " + CEMUtilities::IntToStr(Mod->m_PolOrder);
			std::string modRow = polRow + "." + CEMUtilities::IntToStr(Mod->m_Order);
			PolName = Mod->m_PolName + polRow;
			ModName = Mod->m_Name + modRow;
			Location = Mod->m_Id;
		}
	}
	return CritErr(Err, Location, SysName, PolName, ModName, ParName, ParVal);
}

bool CEMError::CritErr(std::string Err, std::string Location,
					std::string SysName, std::string PolName, std::string ModName,
					std::string ParName, std::string ParVal)
{
	++m_CritErrCnt;
	return CEMError::Err(1, Err, Location, SysName, PolName, ModName, ParName, ParVal);
}

bool CEMError::NonCritErr(std::string Err, CEMParam *Par, std::string Handling)
{
	std::string Location="";
	std::string SysName="";
	std::string PolName="";
	std::string ModName="";
	std::string ParName="";
	std::string ParValue="";
	if(Par)
	{
		if(CEMControl::m_InputTXT) //needs refactoring
		{
			if(Par->m_System) SysName=Par->m_System->m_Name;
			if(Par->m_Module) { ModName=Par->m_Module->m_Name; PolName=Par->m_Module->m_PolName; }
			ParName=Par->m_Name;
			if(Par->m_Id!="") ParName+=" (" + CEMUtilities::IdToLocation(Par->m_Id,1) + ")";
			ParValue=Par->m_strValue;
		}
		else
		{
			if(Par->m_System) SysName = Par->m_System->m_Name;
			std::string parRow = "";
			if (Par->m_Module)
			{
				std::string polRow = "; row " + CEMUtilities::IntToStr(Par->m_Module->m_PolOrder);
				std::string modRow = polRow + "." + CEMUtilities::IntToStr(Par->m_Module->m_Order);
				parRow = (Par->m_Order <= 0) ? "" : modRow + "." + CEMUtilities::IntToStr(Par->m_Order);
				PolName = Par->m_Module->m_PolName + polRow;
				ModName = Par->m_Module->m_Name + modRow;
			}
			ParName = Par->m_Name + parRow;
			ParValue = Par->m_strValue;
			Location = (Par->m_Id == "op") ? "" : Par->m_Id;
		}
	}
	return NonCritErr(Err, Location, SysName, PolName, ModName, ParName, ParValue, Handling);
}

bool CEMError::NonCritErr(std::string Err, CEMModule *Mod, std::string ParName, std::string ParVal, std::string Handling)
{
	std::string Location="";
	std::string SysName="";
	std::string PolName="";
	std::string ModName="";
	if(Mod)
	{
		if(CEMControl::m_InputTXT) //needs refactoring
		{
			if(Mod->m_System) SysName=Mod->m_System->m_Name;
			PolName=Mod->m_PolName;
			ModName=Mod->m_Name + " (starting at " + CEMUtilities::IdToLocation(Mod->m_Id,1) + ")";
		}
		else
		{
			if(Mod->m_System) SysName = Mod->m_System->m_Name;
			std::string polRow = "; row " + CEMUtilities::IntToStr(Mod->m_PolOrder);
			std::string modRow = polRow + "." + CEMUtilities::IntToStr(Mod->m_Order);
			PolName = Mod->m_PolName + polRow;
			ModName = Mod->m_Name + modRow;
			Location = Mod->m_Id;
		}
	}
	return NonCritErr(Err, Location, SysName, PolName, ModName, ParName, ParVal, Handling);
}

bool CEMError::NonCritErr(std::string Err, std::string Location,
					std::string SysName, std::string PolName, std::string ModName,
					std::string ParName, std::string ParVal, std::string Handling)
{
	static std::map<std::string, int> ErrMem;
	static std::map<std::string, int> SimErrMem;
	if(m_Control->m_RunTime && m_Control->m_LogWarnings) //to avoid repeating a run-time error (warning) for each unit
	{	//usually warning/error is equal for each unit, therefore store to not repeat it
		std::string ThisErr=Err+Location+SysName+PolName+ModName+ParName+ParVal+Handling;
		if(ErrMem.find(ThisErr)!=ErrMem.end()) return 1;
		ErrMem.insert(std::pair<std::string,int>(ThisErr, 0));

		//in some cases warning/error refers to unit, example: "Invalid band for unit with head-id 101.00: lower limit (1030.00) is higher than upper limit (587.00)"
		//this may concern only a few units, but it may also concern a lot of units
		//thus restrict the number of such messages to an upper limit, to avoid crashes if the error-log is displayed in th UI (and to avoid long, confusing error-logs)
		ThisErr=Location+SysName+PolName+ModName+ParName+ParVal+Handling;
		std::map<std::string, int>::iterator fd = SimErrMem.find(ThisErr);
		if(fd!=SimErrMem.end())
		{
			if (fd->second >= 3)
				return 1;
			fd->second++;
		}
		else
			SimErrMem.insert(std::pair<std::string,int>(ThisErr, 1));

	}
	++m_NonCritErrCnt;
	return CEMError::Err(0, Err, Location, SysName, PolName, ModName, ParName, ParVal, Handling);
}

bool CEMError::Err(bool Critical, std::string Err, std::string Location,
						 std::string SysName, std::string PolName, std::string ModName,
						 std::string ParName, std::string ParVal, std::string Handling)
{
	if(!OpenErrLog()) return 0;

	std::cerr << std::endl << "____________________________________________________________________________________" << std::endl << std::endl;
	if(Critical) std::cerr << "Error:     " << Err << std::endl; else std::cerr << "Warning:    " << Err << std::endl;
	if(m_Control->m_LogWarnings && Handling!="") std::cerr << "Handling:   " << Handling << std::endl;
	if(SysName!="") std::cerr << "System:     " << SysName << std::endl;
	if(PolName!="") std::cerr << "Policy:     " << PolName << std::endl;
	if(ModName!="") std::cerr << "Function:   " << ModName << std::endl;
	if(ParName!="") std::cerr << "Parameter:  " << ParName << std::endl;
	if(ParVal!="") std::cerr <<  "Value:      " << ParVal << std::endl;
	if(Location!="")
	{
		if(CEMControl::m_InputTXT) std::cerr << "Location:   ";
		else std::cerr << "Identifier: ";
		std::cerr << Location << std::endl;
	}
	std::cerr << "____________________________________________________________________________________" << std::endl << std::flush;

	if(Critical) m_ErrLog << "Error:     " << Err << std::endl; else m_ErrLog << "Warning:    " << Err << std::endl;
	if(m_Control->m_LogWarnings && Handling!="") m_ErrLog << "Handling:   " << Handling << std::endl;
	if(SysName!="") m_ErrLog << "System:     " << SysName << std::endl;
	if(PolName!="") m_ErrLog << "Policy:     " << PolName << std::endl;
	if(ModName!="") m_ErrLog << "Function:   " << ModName << std::endl;
	if(ParName!="") m_ErrLog << "Parameter:  " << ParName << std::endl;
	if(ParVal!="") m_ErrLog <<  "Value:      " << ParVal << std::endl;
	if(Location!="")
	{
		if(CEMControl::m_InputTXT) m_ErrLog << "Location:   ";
		else m_ErrLog << "Identifier: ";
		m_ErrLog << Location << std::endl;
	}
	m_ErrLog << "____________________________________________________________________________________" << std::endl;

	if(!m_Control->m_RunTime || (!Critical && m_Control->m_LogWarnings)) return 1;
	return 0;

	//condition above reflects the following error handling policy:
	//while program reads parameters: errors are logged, program is eventually stopped after param reading
	//runtime: program is always stopped on critcal erros and eventually on non-critical
	//'eventually' refers to the option 'log non-critcal errors'
	//
	// the following table summarises to possible settings:
	//         |            param-reading               |              runtime
	//         | don't log warnings |  log warnings     | don't log warnings | log warnings
	//---------|--------------------|-------------------|--------------------|----------------
	//critical |stop after par-read |stop after par-read|  stop immediately  |stop immediately  
	//         |     return 1       |     return 1      |      return 0      |    return 0
	//---------|--------------------|-------------------|--------------------|----------------
	//non-     |stop after par-read |      no stop      |  stop immediately  |    no stop
	//critical |     return 1       |      return 1     |      return 0      |    return 1
	//---------|--------------------|-------------------|--------------------|----------------
	//
	//'very' critical errors (i.e. usually programming errors) are handeled by the 
	//functions CodeErr or ErrNotif and always cause an immediate program stop (even at param-read-time)
}
void CEMError::ErrSum(std::string Sum)
{
	std::cerr << std::endl << "____________________________________________________________________________________" << std::endl << std::endl;
	std::cerr << Sum << std::endl;
	std::cerr << "____________________________________________________________________________________" << std::endl << std::flush;
	m_ErrLog << Sum << std::endl;
	m_ErrLog << "____________________________________________________________________________________" << std::endl;
}

/********************************************************************************************
 functions class CEMUtilities
********************************************************************************************/
std::vector<std::string> CEMUtilities::TokeniseLine(std::string line, std::string acro, char delim, bool lcase)
{
	std::vector<std::string> columns;
	line = Replace(line, "=cc=", acro);
	if(line[line.length()-1]!=delim) line.push_back(delim);
	for(size_t d=line.find(delim); d!=std::string::npos; d=line.find(delim))
	{
		std::string tok = "";
		if(lcase) tok = Trim(LCase(line.substr(0, d)));
		else tok = Trim(line.substr(0, d));
		columns.insert(columns.end(), tok);
		line.erase(0,d+1);
	}
	return columns;
}

std::string CEMUtilities::LCase(std::string str)
{
	std::string lstr="";
	for(unsigned int i=0; i<str.length(); ++i)
	{
		if(str[i]>='A' && str[i]<='Z')
			lstr.push_back(str[i]-'A'+'a');
		else
			lstr.push_back(str[i]);
	}
	return lstr;
}

std::string CEMUtilities::Trim(std::string str, bool EraseTab)
{
	std::string lstr=str;
	while(lstr.length() && lstr[0]==' ')
		lstr.erase(0,1);
	while(lstr.length() && lstr[lstr.length()-1]==' ')
		lstr.erase(lstr.length()-1,1);
	if(!EraseTab) return lstr;
	while(lstr.length() && lstr[0]=='\t')
		lstr.erase(0,1);
	while(lstr.length() && lstr[lstr.length()-1]=='\t')
		lstr.erase(lstr.length()-1,1);
	return lstr;
}

std::string CEMUtilities::ClearEnds(std::string str)
{
	std::string lstr=str;
	std::string WhiteSpace = " \n\t\r";
	while(lstr.length() && WhiteSpace.find(lstr[0]) != std::string::npos)
		lstr.erase(0,1);
	while(lstr.length() && WhiteSpace.find(lstr[lstr.length()-1]) != std::string::npos)
		lstr.erase(lstr.length()-1,1);
	return lstr;
}

std::string CEMUtilities::Replace(std::string str, std::string what, std::string by)
{
	if(what==by) return str;
	for(size_t fd=0-by.length();;)
	{
		fd=str.find(what,fd+by.length());
		if(fd==std::string::npos) break;
		str.replace(fd,what.length(),by);
	}
	return str;
}

std::string CEMUtilities::ReplacePercent(std::string str)
{
	for(size_t proe=str.find("%"); proe!=std::string::npos; proe=str.find("%"))
	{
		std::string nm="0123456789"; nm+=m_DecSignParam;
		size_t pros=str.substr(0,proe).find_last_not_of(nm);
		int prosi = (pros==std::string::npos) ? -1 : (int)pros;
		str.replace(prosi+1, proe-prosi, "("+str.substr(prosi+1, proe-prosi-1)+"/100)");
	}
	return str;
}

std::string CEMUtilities::RemoveQuotes(std::string s)
{
	if(s.length() && s[0]=='"') s.erase(0,1);
	if(s.length() && s[s.length()-1]=='"') s.erase(s.length()-1,1);
	return s;
}

std::string CEMUtilities::CheckPathName(std::string path)
{
	path=RemoveQuotes(path);
	const char PATH_DELIM='/';
	if(path.length() && path[path.length()-1]!=PATH_DELIM && path[path.length()-1]!='\\') path.push_back(PATH_DELIM);
	return path;
}

std::string CEMUtilities::DoubleToStr(double num, bool Param)
{
	char cs[500];
	EM_SPRINTF(cs, "%.2f", num);
	std::string sd="";
	sd.push_back(Param ? m_DecSignParam : m_DecSignData);
	std::string s=Replace(cs, ".", sd);
	return s;
}

std::string CEMUtilities::IntToStr(int num)
{
	char cs[500];
	EM_SPRINTF(cs, "%d", num);
	return cs;
}

void CEMUtilities::InitDecSigns()
{
	setlocale (LC_NUMERIC,"");
	struct lconv *lc=localeconv();
	m_DecSignData=*(lc->decimal_point);
	m_DecSignParam=*(lc->decimal_point);
}

bool CEMUtilities::CheckDecSign(std::string DecSign, std::string FileName, bool Param)
{
	DecSign=RemoveQuotes(DecSign);
	char dc = ' ';
	if(DecSign.length()==1) dc = DecSign.at(0);
	if(dc!='.' && dc!=',')
		return CEMError::CritErr("Parameter must be set to . or , (or n/a).", FileName);
	
	if(Param)
	{
		m_NeedConvert_ParamDecSign = (dc!=m_DecSignParam);
		m_DecSignParam = dc;
	}
	else
	{
		m_NeedConvert_DataDecSign = (dc!=m_DecSignData);
		m_DecSignData = dc;
	}
	return 1;	
}

std::string CEMUtilities::ConvertLocPeriod(std::string s, bool Param)
{
	if((Param && !m_NeedConvert_ParamDecSign) || (!Param && !m_NeedConvert_DataDecSign)) return s;
	
	setlocale (LC_NUMERIC,"");
	struct lconv *lc=localeconv();
	char locDecSign=*(lc->decimal_point);
	char DecSign = Param ? m_DecSignParam : m_DecSignData;
	for(int i=int(s.length())-1; i>=0; i--)
		if(s.at(i)==DecSign)
		{						//only replace the very last ./,
			s.at(i)=locDecSign;	//this way one has the chance to dedect 1,500.5#m
			break;				//though not 1,500#m
		}
	return s;
}

double CEMUtilities::StrToDouble(std::string s, bool *err, bool Param)
{
	s=ConvertLocPeriod(s, Param);
	char *rem;
	double d = strtod(s.c_str(), &rem);
	if(err) *err=strcmp(rem, "")!=0;
	return d;
}

int CEMUtilities::StrToInt(std::string s, bool *err)
{
	char *rem;
	int i = strtol(s.c_str(), &rem, 10);
	if(err)
		*err=strcmp(rem, "")!=0;
	return i;
}

bool CEMUtilities::CheckCurrency(int &euro, std::string ParVal, std::string Sheet, int Line, std::string ParName, std::string System)
{
	if(!ParVal.compare("euro")) { euro=1; return 1; }
	if(!ParVal.compare("national") || !ParVal.compare("nat")) { euro=0; return 1; }
	return CEMError::CritErr("Parameter value must be set to 'euro' or 'national'.", 
					Sheet+", line "+IntToStr(Line), System, "", "", ParName, ParVal);
}

bool CEMUtilities::CheckYesNo(double &Yes, std::string ParVal, std::string Sheet, int Line, std::string ParName)
{
	Yes=CheckYesNo(ParVal);
	if(Yes!=-1) return 1;
	return CEMError::CritErr("Parameter value must be set to 'yes' or '1' / 'no' or '0'.", 
					"'"+Sheet+"', line "+IntToStr(Line), "", "", "", ParName, ParVal);
}

bool CEMUtilities::CheckYesNo(double &Yes, CEMParam *Par)
{
	Yes=CheckYesNo(Par->m_strValue);
	if(Yes!=-1) return 1;
	return CEMError::CritErr("Parameter value must be set to 'yes' or '1' / 'no' or '0'.", Par);
}

int CEMUtilities::CheckYesNo(std::string str)
{
	if(!str.compare("yes") || !str.compare("1")) return 1;
	if(!str.compare("no") || !str.compare("0")) return 0;
	return -1;
}

bool CEMUtilities::CheckOnOff(double &OnOff, CEMParam *Par, std::string ParVal, std::string Location, std::string SysName, std::string PolName, std::string ModName)
{
	if(Par) ParVal=Par->m_strValue;
	if(!ParVal.compare("on") || !ParVal.compare("1")) { OnOff=1; return 1; }
	if(!ParVal.compare("off") || !ParVal.compare("0")) { OnOff=0; return 1; }
	if(!ParVal.compare("toggle") || !ParVal.compare("2")) { OnOff=2; return 1; }
	if(Par) return CEMError::CritErr("Parameter value must be set to 'off' or '0' / 'on' or '1' / 'toggle' or '2'.", Par);
	return CEMError::CritErr("Parameter value must be set to 'off' or '0' / 'on' or '1' / 'toggle' or '2'.", Location, SysName, PolName, ModName);
}

double CEMUtilities::Round(double Amount, double RdBase, int AlwaysUpDown)
{
	double distance = (Amount-((int)(Amount/RdBase))*RdBase)/RdBase;
	if (distance <= 0.0000001 || distance >= 0.9999999) return Amount; //this is necessary because of Amounts like 6.3000000000001 or 6.2999999999999, where I really have no idea where they come from ???
	if((distance < .4999999 || AlwaysUpDown == -1) && AlwaysUpDown != 1) //alwaysUpDown: -1 always down, 1: always up, 0: normal rounding 
		Amount = ((int)(Amount/RdBase))*RdBase;			//round to next lower full base
	else
		Amount = (((int)(Amount/RdBase))+1)*RdBase;		//round to next higher full base
	return Amount;
}

bool CEMUtilities::ConvertMonthly(double &Val, char Period, CEMParam *Par)
{
	switch(Period)
	{
	case 'm': return 1; //month (m) monthly rate (mr)
	case 'c': return 1; //capital (just to indicate monetary amount)
	case 'y': Val *= 0.08333333333333; return 1; //year (y) yearly rate (yr), 1/12
	case 'q': Val *= 0.3333333333333; return 1; //quarter (q) or quarterly rate (qr), 1/3
	case 'w': Val *= 4.34; return 1; //week (w) or weekly rate (wr), round(365/12/7)
	case 'd': Val *= 30.5; return 1; //day (d) or daily rate (dr)
	case 'l': Val *= 21.73; return 1; //labour day (365/12/7*5)
	case 's': Val *= 26.07; return 1; //labour day in a six days week (365/12/7*6)
	default: return CEMError::CritErr("Invalid period.\nValid periods are:\nm (month) mr (monthly rate),\ny (annual) yr (annualy rate),\nq (quarter) qr (quarterly rate),\nw (week) wr (weekly rate),\nd (day) dr (dayly rate),\nl (labour day)", Par);
	}
}

bool CEMUtilities::ExtractPeriod(std::string &Val, std::string &Period, bool NoRemainder)
{
	Val=Trim(Val);
	if(m_NeedConvert_ParamDecSign)
	{//this is because 123.5#m would not be converted to 123,5#m by Excel if the local decimal sign is , (and the other way round)
	 //it's not a very good solution, because it ignores 1,500.5#m
		std::string parDC = ""; parDC+=m_DecSignParam;
		std::string locDC = (m_DecSignParam == '.') ? "," : ".";
		Val = Replace(Val, parDC, locDC);
	}
	std::string oVal=Val;			
	size_t hash = Val.find('#');
	if(Period.empty()) Period="m";
	if(hash==std::string::npos) return 1;
	if(Val.length()<=hash+1 || Val.substr(hash+1,1).find_first_of("mcyqwdls")==std::string::npos) return 0;
	Period=Val.at(hash+1);
	Val=Val.substr(0,hash);
	if(!NoRemainder) return 1;
	//do not allow for any text after the period (e.g. 100#monthly)
	if(oVal.length()==hash+2) return 1;
	if(oVal.length()>hash+3) { Val=oVal; return 0;}
	//allow for mr, yr, etc.
	if(oVal.substr(hash+2)!="r") { Val=oVal; return 0;}
	return 1;
}

std::string CEMUtilities::IdToLocation(std::string Id, bool LineOnly)
{
	std::string Loc="";
	size_t sep = Id.find("_#");
	if(sep==std::string::npos) return Loc;
	if(LineOnly) Loc="line "+Id.substr(sep+2);
	else Loc="sheet '"+Id.substr(0,sep)+"', line "+Id.substr(sep+2);
	return Loc;
}

std::string CEMUtilities::GenId(std::string loc, std::string id, int line)
{
	if(!CEMControl::m_InputTXT) return id;
	char format[500];
	size_t t=loc.find(".txt"); if(t!=std::string::npos) loc=loc.substr(0,t);
	if(id.empty()) EM_SPRINTF(format, "%s_#%d", loc.c_str(), line);
	else EM_SPRINTF(format, "%s_#%s", loc.c_str(), id.c_str());
	return format;
}

void CEMUtilities::ReplaceXY(std::string &ParName, CEMModule *Mod, std::map<std::string, int> &PlaceHolder)
{
	size_t x = ParName.find("["); if(x==std::string::npos) return;

	size_t z = ParName.substr(x).find("]"); if(z==std::string::npos) return; //shouldn't happen
	std::string ph = ParName.substr(x,z+1), phid = Mod->m_Id+ph;
	std::map<std::string, int>::iterator it = PlaceHolder.find(phid); int a;
	if(it==PlaceHolder.end())
	{
		std::string cs1 = ParName.substr(0,x), cs2="";
		if(ParName.length()>x+z+1) cs2 = ParName.substr(x+z+1);
		if(Mod->m_Name==COMOD9) cs1 = CEMUtilities::Replace(cs1, "const", "var");
		a = Mod->m_ParamAdmin.CheckSequence(cs1, Mod, cs2) + 1; if(a<=0) return;
		PlaceHolder.insert(std::pair<std::string,int>(phid, a));
	}
	else a = it->second;
	ParName = Replace(ParName, ph, IntToStr(a));

	x = ParName.find("["); if(x==std::string::npos) return;
	z = ParName.substr(x).find("]"); if(z==std::string::npos) return; //shouldn't happen
	ph = ParName.substr(x,z+1); phid = Mod->m_Id+ph;
	it = PlaceHolder.find(phid);
	if(it==PlaceHolder.end())
	{
		std::string cs1 = ParName.substr(0,x);
		a = Mod->m_ParamAdmin.CheckSequence(cs1, Mod) + 1; if(a<=0) return;
		PlaceHolder.insert(std::pair<std::string,int>(phid, a));
	}
	else a = it->second;
	ParName = Replace(ParName, ph, IntToStr(a));
}

CEMXML::~CEMXML()
{
	for(size_t i=0; i<m_Ch.size(); ++i) delete m_Ch.at(i);
}

bool CEMXML::LoadFile(std::string FileName)
{
	TiXmlDocument root(FileName.c_str());
	if(!root.LoadFile()) return 0;
	return Load(&root);
}

bool CEMXML::Load(TiXmlNode *Node)
{
	if(!Node) return 1;

	m_Name=Node->Value();
	for(TiXmlNode *pCh=Node->FirstChild(); pCh; pCh=pCh->NextSibling())
	{
		if(pCh->Type()!=TiXmlNode::TINYXML_ELEMENT) continue;
        TiXmlNode *pGCh=pCh->FirstChild();
		if(!pGCh || pGCh->Type() == TiXmlNode::TINYXML_TEXT) //end-node
		{
			XMLAtr Atr; Atr.Name=pCh->Value(); Atr.Val=pGCh ? pGCh->ToText()->Value() : "";
			if(Atr.Val=="n\a") Atr.Val="n/a";
			m_Atr.insert(m_Atr.end(), Atr);
			continue;
		}
		CEMXML *xmlCh = new CEMXML;
		m_Ch.insert(m_Ch.end(), xmlCh);
		if(!xmlCh->Load(pCh)) return 0;
	}
	return 1;
}

CEMXML *CEMXML::GetNode(std::string Name)
{
	return GetNode(Name, "", NULL, NULL);
}

CEMXML *CEMXML::GetNode(std::string Name, std::string Id)
{
	return GetNode(Name, Id, NULL, NULL);
}

CEMXML *CEMXML::GetNode(std::string Name, XMLAtr *Ident1, XMLAtr *Ident2)
{
	return GetNode(Name, "", Ident1, Ident2);
}

CEMXML *CEMXML::GetNode(std::string Name, std::string Id, XMLAtr *Ident1, XMLAtr *Ident2)
{
	Name=CEMUtilities::LCase(Name); Id=CEMUtilities::LCase(Id);
	for(size_t i=0, j; i<m_Ch.size(); ++i)
	{
		if(CEMUtilities::LCase(m_Ch[i]->m_Name) == Name)
		{
			bool id0=Id.empty(), id1=(Ident1==NULL), id2=(Ident2==NULL);
			if(!Id.empty())
				for(j=0; j<m_Ch[i]->m_Atr.size(); ++j)
					if(CEMUtilities::LCase(m_Ch[i]->m_Atr[j].Name) == "id" &&
						CEMUtilities::LCase(m_Ch[i]->m_Atr[j].Val)==Id)
						id0=1;
			if(Ident1)
				for(j=0; j<m_Ch[i]->m_Atr.size(); ++j)
					if(CEMUtilities::LCase(m_Ch[i]->m_Atr[j].Name) == CEMUtilities::LCase(Ident1->Name) &&
						CEMUtilities::LCase(m_Ch[i]->m_Atr[j].Val)==CEMUtilities::LCase(Ident1->Val))
						id1=1;
			if(Ident2)
				for(j=0; j<m_Ch[i]->m_Atr.size(); ++j)
					if(CEMUtilities::LCase(m_Ch[i]->m_Atr[j].Name) == CEMUtilities::LCase(Ident2->Name) &&
						CEMUtilities::LCase(m_Ch[i]->m_Atr[j].Val)==CEMUtilities::LCase(Ident2->Val))
						id2=1;
			if(id0 && id1 && id2) return m_Ch[i];
		}
		CEMXML *Node = m_Ch[i]->GetNode(Name, Id, Ident1, Ident2);
		if(Node) return Node;
	}
	return NULL;
}

std::string CEMXML::GetAtrVal(std::string Name)
{
	Name=CEMUtilities::LCase(Name);
	for(size_t i=0; i<m_Atr.size(); ++i)
		if(CEMUtilities::LCase(m_Atr[i].Name) == Name) return CEMUtilities::LCase(m_Atr[i].Val);
	return "";
}

//outcommented (5th Nov 2012) as it didn't work properly, exchanged by function below
//bool CEMUtilities::DoesValueMatchPattern(std::string pattern, std::string value, bool matchCase)
//{
//    std::vector<std::string> patternTokens;
//    std::vector<std::string> valueTokens;
//    bool isNewToken;
//
//    if (!matchCase)
//    {
//        pattern = LCase(pattern);
//        value = LCase(value);
//    }
//
//    patternTokens.clear();
//    valueTokens.clear();
//
//	if (pattern.empty() || value.empty())
//        return 0;
//
//	if (pattern.find("*?") != std::string::npos || pattern.find("?*") != std::string::npos)
//        return 0; //invalid pattern
//
//    //split pattern into tokens of 3 types: *-tokens, ?-tokens, other tokens, e.g. be_????_*hihi -> t1=be_, t2=????, t3=_, t4=*, t5=hihi
//    patternTokens.push_back(pattern.substr(0, 1));
//    for (int i = 1; i < (int)(pattern.length()); ++i)
//    {
//		std::string c = pattern.substr(i, 1);
//        isNewToken = 0;
//		std::string lc = patternTokens[patternTokens.size() - 1].substr(0, 1);
//        if (lc == "*" || lc == "?")
//        {
//            if (c != lc)
//                isNewToken = 1;
//        }
//        else
//        {
//            if (c == "?" || c == "*")
//                isNewToken = 1;
//        }
//        if (isNewToken)
//            patternTokens.push_back(c);
//        else
//            patternTokens[patternTokens.size() - 1] += c;
//    }
//
//    //first check if necessary tokens (i.e. 'other tokens') match; at the same time split value into tokens
//    for (int j = 0; j < (int)(patternTokens.size()); ++j)
//    {
//        if (patternTokens[j].substr(0, 1) != "*" && patternTokens[j].substr(0, 1) != "?")
//        {
//            int pos = (int)(value.find(patternTokens[j]));
//            if (j == 0 && pos != 0)
//                return 0; //necessary token at the very beginning is missing
//            if (pos < 0)
//                return 0; //necessary token is missing
//            if (pos > 0) //gather characters which are not covered by necessary tokens -> assume they are wildcard tokens
//                valueTokens.push_back(value.substr(0, pos));
//            value = value.substr(pos + patternTokens[j].length()); //pos>=0
//        }
//    }
//    if (!value.empty())
//        valueTokens.push_back(value);
//
//	//now check if wildcard tokens match
//    for (int t = 0, w = 0; t < (int)(valueTokens.size()); ++t)
//    {
//        for (; w < (int)(patternTokens.size()); ++w)
//        {
//            if (patternTokens[w].substr(0, 1) == "*" || patternTokens[w].substr(0, 1) == "?")
//                break;
//        }
//
//        if (w >= (int)(patternTokens.size()))
//            return 0;
//        if (patternTokens[w].substr(0, 1) == "?" && valueTokens[t].length() != patternTokens[w].length())
//            return 0; //e.g. Wildcard=ba??na and Term=baXna or baXXXna
//        ++w;
//    }
//    return 1;
//}

bool CEMUtilities::DoesValueMatchPattern(std::string pattern, std::string value, bool matchCase)
{//adaptation of code copied from www.sysexpand.com/?path=net/sysexpand/text/strmatch/source/wildmatch
	if (!matchCase)
	{
		pattern = CEMUtilities::LCase(pattern);
		value = CEMUtilities::LCase(value);
	}
	char singleWildcard = '?';
	char multipleWildcard = '*';

    int* valuePosStack = new int[(value.length() + 1) * (pattern.length() + 1)];	// Stack containing value positions that should be tested for further matching
    int* patternPosStack = new int[(value.length() + 1) * (pattern.length() + 1)];	// Stack containing pattern positions that should be tested for further matching
    int stackPos = -1;																// Points to last occupied entry in stack; -1 indicates that stack is empty
	bool** pointTested = new bool*[value.length() + 1];						      // Each 1 value indicates that value position vs. pattern position has been tested
	for (size_t i = 0; i < value.length() + 1; ++i)
	{
		pointTested[i] = new bool[pattern.length() + 1];
		for (size_t j = 0; j < pattern.length() + 1; ++j)
		pointTested[i][j] = 0;
	}

    size_t valuePos = 0;   // Position in value matched up to the first multiple wildcard in pattern
    size_t patternPos = 0; // Position in pattern matched up to the first multiple wildcard in pattern

    // Match beginning of the string until first multiple wildcard in pattern
    while (valuePos < value.length() && patternPos < pattern.length() &&
            pattern[patternPos] != multipleWildcard &&
            (value[valuePos] == pattern[patternPos] || pattern[patternPos] == singleWildcard))
    {
        valuePos++;
        patternPos++;
    }

    // Push this position to stack if it points to end of pattern or to a general wildcard character
    if (patternPos == pattern.length() || pattern[patternPos] == multipleWildcard)
    {
        pointTested[valuePos][patternPos] = 1;
        valuePosStack[++stackPos] = valuePos;
        patternPosStack[stackPos] = patternPos;
    }

    bool matched = 0;

    // Repeat matching until either string is matched against the pattern or no more parts remain on stack to test
    while (stackPos >= 0 && !matched)
    {
        valuePos = valuePosStack[stackPos];         // Pop value and pattern positions from stack
        patternPos = patternPosStack[stackPos--];   // Matching will succeed if rest of the value string matches rest of the pattern

        if (valuePos == value.length() && patternPos == pattern.length())
            matched = 1;     // Reached end of both pattern and value string, hence matching is successful
        else if (patternPos == pattern.length() - 1)
            matched = 1;     // Current pattern character is multiple wildcard and it will match all the remaining characters in the value string
        else
        {
            // First character in next pattern block is guaranteed to be multiple wildcard
            // So skip it and search for all matches in value string until next multiple wildcard character is reached in pattern
            for (size_t curValueStart = valuePos; curValueStart < value.length(); curValueStart++)
            {
                size_t curValuePos = curValueStart;
                size_t curPatternPos = patternPos + 1;

                while (curValuePos < value.length() && curPatternPos < pattern.length() &&
                        pattern[curPatternPos] != multipleWildcard &&
                        (value[curValuePos] == pattern[curPatternPos] || pattern[curPatternPos] == singleWildcard))
                {
                    curValuePos++;
                    curPatternPos++;
                }

                // If we have reached next multiple wildcard character in pattern without breaking the matching sequence,
                // then we have another candidate for full match.
                // This candidate should be pushed to stack for further processing.
                // At the same time, pair (value position, pattern position) will be marked as tested,
                // so that it will not be pushed to stack later again.
                if (((curPatternPos == pattern.length() && curValuePos == value.length()) ||
                        (curPatternPos < pattern.length() && pattern[curPatternPos] == multipleWildcard)) &&
                    !pointTested[curValuePos][curPatternPos])
                {
                    pointTested[curValuePos][curPatternPos] = 1;
                    valuePosStack[++stackPos] = curValuePos;
                    patternPosStack[stackPos] = curPatternPos;
                }

            }
        }

    }

	delete valuePosStack;
    delete patternPosStack;
	for (size_t i=0; i < value.length() + 1; ++i)
		delete pointTested[i];
    delete pointTested;

    return matched;
}

std::string CEMUtilities::RemoveExtension(std::string fileName)
{
	if (fileName.length() > 4 && fileName.substr(fileName.length() - 4, 1) == ".")
		return fileName.substr(0, fileName.length() - 4);
	return fileName;
}

std::string CEMUtilities::AppendExtension(std::string fileName, std::string extension, bool ReplaceExistingExtension)
{
	extension = LCase(extension);
	if (extension.length() && extension[0] != '.')
		extension = '.' + extension; //e.g. txt -> .txt

	std::string existingExtension = "";
	if (fileName.length() > extension.length())
		existingExtension = LCase(fileName.substr(fileName.length() - extension.length(), extension.length()));

	if (existingExtension == extension)
		return fileName; //extension already appended

	if (ReplaceExistingExtension)
		return fileName + extension;

	//if possible other extension (e.g. .csv) is not to be replaced check for existing .
	if (existingExtension.length() && existingExtension[0] == '.')
		return fileName;

	return fileName + extension;
}

std::string CEMUtilities::ReplaceAbsMaxMin(std::string formula)
{
	// use { as the operand for min (i.e. use an arbitrary one-character-operand)
	formula=CEMUtilities::Replace(formula, "<min>","{");
	// and } as the operand for max
	formula=CEMUtilities::Replace(formula, "<max>","}");
	//use ] to describe abs( with one sign
	formula=CEMUtilities::Replace(formula, "<abs>(", "]");
	return formula;
}

std::string CEMUtilities::ExtractSystemYear(std::string origSystemName, bool noWarning)
{
	size_t index1 = 999;
	//try to find 4 subsequent digits, supposing this is the system year		
	for (std::string systemName = origSystemName + "x";;)
	{
		index1 = systemName.find_first_of("0123456789");
		if (index1 == std::string::npos)
			break;
		size_t index2 = systemName.substr(index1).find_first_not_of("0123456789");
		if (index2 == 4.0)
			break;
		index2 += index1;
		for (; index1 < index2; ++index1)
			systemName[index1] = 'x';
		index1 = 999;
	}
	if (index1 < 999)
		return origSystemName.substr((size_t)(index1), 4);
	std::string warning = "Query 'GetSystemYear' could not extract the year from the system name '"; warning += origSystemName; warning += "'.";
	if (!noWarning) CEMError::NonCritErr(warning, "");
	return "";
}
