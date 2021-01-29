#include <fstream>
#include <time.h>
#include <locale.h>
#include <string.h>
#include <iostream>			// assumed in windows, but required in Linux
#include "EMDefinitions.h"
#include "EMControl.h"
#include "EMUtilities.h"
using namespace std;		// std namespace is assumed in windows, but required in Linux

/********************************************************************************************
 functions class CEMControl
********************************************************************************************/
int CEMControl::m_InputTXT=0;

CEMControl::CEMControl()
{
	m_ExeVersion = "f6.11"; //not used anymore since UI and executable form one software bundle
	m_EMVersion="";
	m_UIVersion="";
	m_ParamPath="";
	m_AOParamPath="";
	m_ConfigPath="";
	m_EMContentPath="";
	m_OutputPath="";
	m_DataPath="";
	m_DataSet="";
	m_HeaderDate="";
	m_OutfileDate="";
	m_ControlName="";
	m_DataConfig="";
	m_SpineName="";
	m_Country="";
	m_DataCurrencyEuro=-1;
	m_UseDefault="";
	m_UseCommonDefault=0;
	m_DataIncomeYear=0;
	m_ErrLogFile="";
	m_LogWarnings=0;
	m_LogRunTime=0;
	m_IsPublicVersion=0;
	m_LastRun=0;
	m_InputTXT=0;
	m_StartHHID = -1.0;
	m_LastHHID = -1.0;
	m_SimInfoAvailable = 0;
	m_IgnorePrivate = 0;
	m_FirstNHHOnly = LONG_MAX;
	m_CommandLineConfig = 0;
	m_ExchangeRateDate = "";
	m_ReadXVariables=0;
	CEMUtilities::InitDecSigns();
}

CEMControl::~CEMControl()
{
	for(SystemContainer_t::iterator itS=m_Systems.begin(); itS!=m_Systems.end(); itS++)
		itS->CleanUp();
	for(std::vector<CEMTable*>::iterator itT=m_InputTables.begin(); itT!=m_InputTables.end(); itT++)
		if(*itT!=NULL) delete *itT;
	m_InputTables.clear();
}

bool CEMControl::GoAhead(std::string ConfigName)
{
	m_StartTime = time(NULL);
	clock_t sTime = clock();

	m_RunTime=0;
	m_ConfigName = ConfigName;
	int len=int(ConfigName.length()); std::string lcn=CEMUtilities::LCase(ConfigName);
	m_InputTXT=lcn.substr((int)std::fmax(len-4,0)).compare(".xml");
	if (m_CommandLineConfig) m_InputTXT = 0;
	std::cout << "reading data ...\n" << std::flush;
	bool succ = ReadInput();

	if(succ)
	{
		//error handling, see EMUtilities.cpp, function CEMError::Err for an explanation
		if(CEMError::m_CritErrCnt>0 || (CEMError::m_NonCritErrCnt>0 && !m_LogWarnings))
			{ CEMError::ErrNotif("Errors are listed above and in '" + m_ErrLogFile + "'.", "EUROMOD FOUND ONE OR MORE ERRORS AND DID NOT START CALCULATIONS."); return 0; }
		m_RunTime=1;
		succ = Run();
	}
	AddTimeRec("TOTAL", "", clock()-sTime, 0);
	if(m_LogRunTime) LogRunTime();

	if(CEMError::m_NonCritErrCnt+CEMError::m_CritErrCnt>0)
	{
		if(succ) CEMError::ErrSum("EUROMOD FINISHED ITS CALCULATIONS BUT ISSUED ONE OR MORE WARNINGS.");
		else CEMError::ErrSum("EUROMOD ENCOUNTERED A CRITICAL ERROR AND COULD NOT FINISH ITS CALCULATIONS.");
	}

	if(m_LastRun)
	{
		std::ifstream iftest;
		iftest.open(m_ErrLogFile.c_str());
		if(!iftest.is_open()) return 1;
		if(m_InputTXT) //message not reasonable for new UI
			CEMError::ErrNotif("Errors are listed above and in '" + m_ErrLogFile + "'.", "ONE OR MORE RUNS ENCOUNTERED PROBLEMS - PLEASE CONSIDER THE ERROR LOG.");
		iftest.close();
	}
	return succ;
}

bool CEMControl::ReadInput()
{
	//read configuration and general settings (including variables description) as well as input data
	clock_t sTime = clock();
	if(m_InputTXT) { if(!TXT_ReadInput_Config()) return 0; } else { if(!XML_ReadInput_Config()) return 0; }
	if(!ReadData()) return 0;
	clock_t eTime = clock();
	AddTimeRec("READ DATA", "", eTime-sTime);

	//read and check (other) parameters and accomplish some other necessary tasks
	sTime = eTime;

	if(m_UseCommonDefault) { if(!CreateDefaultVariables()) return 0; }
	if(m_InputTXT) { if(!TXT_ReadInput_Param()) return 0; } else { if(!XML_ReadInput_Param()) return 0; }
	
	if (!CheckParam()) return 0;
	if (!ReadGlobalConfig()) return 0; // will overwrite any definition of $HICP in uprating-factors (most likely values are anyway equal, but global-table should dominate)
									   // similar procedure for euro-exchange-rate (including option to define the data - see m_ExchangeRateDate)
	if(!AssessGenVarInd()) return 0;
	AddTimeRec("READ PARAM", "", clock()-sTime);
	return 1;
}

bool CEMControl::TXT_ReadInput_Config()
{
	if(!TXT_ReadConfigParam()) return 0;
	if(!TXT_ReadControlParam()) return 0;
	if(!TXT_ReadAOControlParam()) return 0;
	if(!TXT_ReadVardesc()) return 0;
	return 1;
}

bool CEMControl::TXT_ReadInput_Param()
{
	if(!TXT_ReadSpineParam()) return 0;
	if(!TXT_ReadPolicyParam()) return 0;
	if(!TXT_ReadAOParam()) return 0;
	return 1;
}

bool CEMControl::XML_ReadInput_Config()
{
	if (!m_CommandLineConfig) if (!XML_ReadConfigParam()) return 0;
	std::string file = m_ParamPath + m_DataConfig;
	if(!DataConfigXML.LoadFile(file)) return CEMError::CodeErr("Failed to read input file '" + file + "' (err1).", "Check if the XML-code is correct.");

	if(!XML_ReadControlParam()) return 0;
	if(!TXT_ReadAOControlParam()) return 0; //keep txt-version to enable intermediate solution for mtr-calculations
	if(!XML_ReadVardesc()) return 0;

	return 1;
}

bool CEMControl::ReadUpratingIndices(CEMXML *xml)
{
	for (size_t i = 0; i < xml->m_Ch.size(); ++i)
	{
		if (CEMUtilities::LCase(xml->m_Ch[i]->m_Name) != "upratingindex") continue;

		UpratingIndex uInd;
		std::string reference = xml->m_Ch[i]->GetAtrVal("reference");
		std::vector<std::string> yearValues = CEMUtilities::TokeniseLine(xml->m_Ch[i]->GetAtrVal("yearvalues"), "", '°');
		for (size_t y = 0; y < yearValues.size(); ++y)
			uInd.m_YearValues.insert(std::pair<string, double>(yearValues[y].substr(0, 4), CEMUtilities::StrToDouble(yearValues[y].substr(5))));
		if (!GetVarIndex(reference, uInd.m_VarIndex, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 1, 0))
			return CEMError::CodeErr("Programming error: adding uprating index '" + reference + "' to variable list failed.");
		m_UpratingIndices.push_back(uInd);
	}
	return 1;
}

void CEMControl::SetUpratingIndices(CEMSystem *sys)
{
	std::string ySys = sys->m_Year != "" ? sys->m_Year : CEMUtilities::ExtractSystemYear(sys->m_Name, 1);
	std::string yData = CEMUtilities::DoubleToStr(m_DataIncomeYear).substr(0,4);
	for (size_t h = 0; h < sys->m_Households.size(); ++h) //loop over households (within system)
	{
		for (size_t i = 0; i < m_UpratingIndices.size(); ++i)
		{
			double iSys = m_UpratingIndices[i].m_YearValues.find(ySys) != m_UpratingIndices[i].m_YearValues.end() ? iSys = m_UpratingIndices[i].m_YearValues[ySys] : -1;
			double iData = m_UpratingIndices[i].m_YearValues.find(yData) != m_UpratingIndices[i].m_YearValues.end() ? iData = m_UpratingIndices[i].m_YearValues[yData] : -1;
			double val = iSys == -1 || iData <= 0 ? NUMNEGINFINITE : iSys / iData;
			for (size_t p = 0; p < sys->m_Households[h].m_Persons.size(); ++p) //loop over persons within household
				sys->m_Households[h].m_Persons[p][m_UpratingIndices[i].m_VarIndex] = val;
		}
	}
}

bool CEMControl::XML_ReadInput_Param()
{
	std::string file = m_ParamPath + this->m_ControlName;
	CEMXML xml; std::string Val; bool err;
	if(!xml.LoadFile(file)) return CEMError::CodeErr("Failed to read input file '" + file + "' (err2).", "Check if the XML-code is correct.");

	ReadUpratingIndices(xml.GetNode("countryconfig"));

	for(size_t i=0, j; i<m_Systems.size(); ++i)
	{
		//S Y S T E M S
		CEMXML *pSys = NULL;
		if (!m_CommandLineConfig)
		{
			pSys = xml.GetNode("System", m_Systems[i].m_Id);
			if (!pSys) m_Systems[i].m_Name = m_Systems[i].m_Id; // this may happen if EM_Config contains system-name instead of system-id (i.e. a manually adapted EM_Config)
		}
		if (!pSys)
		{
			XMLAtr ident; ident.Name = "name"; ident.Val = m_Systems[i].m_Name;
			pSys = xml.GetNode("System", &ident);
			if (!pSys) return CEMError::CodeErr("Failed to find node 'System' with 'Name/Id'='" + m_Systems[i].m_Name + "' in input file '" + file + "'.",
												m_CommandLineConfig ? "Check command line argument." : "Check if the XML-code is correct.");
			m_Systems[i].m_Id = pSys->GetAtrVal("id");
		}
		m_Systems[i].m_Name=pSys->GetAtrVal("name");
		m_Systems[i].m_Order=CEMUtilities::StrToInt(pSys->GetAtrVal("order"));
		if(!CEMUtilities::CheckCurrency(m_Systems[i].m_OutputCurrencyEuro, pSys->GetAtrVal("currencyoutput"), m_ControlName, 0, "CurrencyOutput", m_Systems[i].m_Name)) return 0;
		if(!CEMUtilities::CheckCurrency(m_Systems[i].m_ParamCurrencyEuro, pSys->GetAtrVal("currencyparam"), m_ControlName, 0, "CurrencyParam", m_Systems[i].m_Name)) return 0;
		Val=pSys->GetAtrVal("exchangerateeuro");
		m_Systems[i].m_ExchRate = CEMUtilities::StrToDouble(Val, &err);
		if(err) return CEMError::CritErr("Parameter is not a valid number.", m_ControlName, m_Systems[i].m_Name, "", "", "ExchangeRateEuro", Val);
		//if(!m_Systems[i].m_ExchRate) return CEMError::CritErr("Exchange rate cannot be set to zero.", m_ControlName, m_Systems[i].m_Name, "", "", "ExchangeRateEuro", Val);
		if (!m_Systems[i].m_ExchRate) m_Systems[i].m_ExchRate = 1; // Juli 2018: exchange rate is moved from system to global exchange-rate-file
		m_Systems[i].m_HeadDefInc=pSys->GetAtrVal("headdefinc");
		m_Systems[i].m_Year = pSys->GetAtrVal("year");

		SetUpratingIndices(&(m_Systems[i]));

		std::map<int, CEMXML*> PolOrder; PolOrder.clear();
		for(j=0; j<pSys->m_Ch.size(); ++j)
		{
			if(CEMUtilities::LCase(pSys->m_Ch[j]->m_Name) != "policy")
				continue; //ignore other system-children (e.g. conditional formatting)
			Val=pSys->m_Ch[j]->GetAtrVal("order");
			int order = CEMUtilities::StrToInt(Val, &err);
			if(Val.empty() || err) return CEMError::CodeErr("Failed to read input file '" + file + "' (err3).", "Check if the XML-code is correct.");
			PolOrder.insert(std::pair<int,CEMXML*>(order, pSys->m_Ch[j]));
		}

		//P O L I C I E S
		int PolicyOrder = 0;
		for(std::map<int, CEMXML*>::iterator itp=PolOrder.begin(); itp!=PolOrder.end(); ++itp) 
		{
			CEMXML *pPol = itp->second;
			std::string PolSwitch = pPol->GetAtrVal("switch"); if (PolSwitch == "n/a" || PolSwitch == "switch") PolSwitch = "off";
			if (m_IgnorePrivate && pPol->GetAtrVal("private") == "yes") PolSwitch = "off";
			std::string PolID = pPol->GetAtrVal("id");
			std::string RefPol = pPol->GetAtrVal("referencepolid");
			if(!RefPol.empty()) { pPol=XML_ReadInput_GetReferencePolicy(RefPol, PolOrder); if(!pPol) return 0; }
			std::string PolName = pPol->GetAtrVal("name");
			//int PolicyOrder = CEMUtilities::StrToInt(pPol->GetAtrVal("order"));
			if (CEMUtilities::LCase(PolName).substr(0,18) != "defupratingfactors")
				++PolicyOrder; //count policy order instead of using the UI's order, thus the order corresponds with the row in the UI (which is not guaranteed with the UI's order)

			//adapt switch is this is a switchable policy and the switch was changed via the configration file
			PolSwitch = AdaptSwitchIfSwitchablePolicy(m_Systems[i].m_Id, PolName, PolSwitch);

			if(!m_Systems[i].TakeSpineParam(PolName, PolSwitch, PolID, PolicyOrder)) return 0;

			//F U N C T I O N S
			std::map<int, CEMXML*> FuncOrder; FuncOrder.clear();
			for(j=0; j<pPol->m_Ch.size(); ++j)
			{
				if(CEMUtilities::LCase(pPol->m_Ch[j]->m_Name) != "function")
					continue; //ignore other policy-children (if there are any)
				Val=pPol->m_Ch[j]->GetAtrVal("order");
				int order = CEMUtilities::StrToInt(Val, &err);
				if(Val.empty() || err) return CEMError::CodeErr("Failed to read input file '" + file + "' (err4).", "Check if the XML-code is correct.");
				FuncOrder.insert(std::pair<int,CEMXML*>(order, pPol->m_Ch[j]));
			}
			int FunctionOrder = 0;
			for(std::map<int, CEMXML*>::iterator itf=FuncOrder.begin(); itf!=FuncOrder.end(); ++itf) 
			{
				CEMXML *pFunc = itf->second;
				std::string FuncName = pFunc->GetAtrVal("name");
				if(CEMUtilities::LCase(FuncName).substr(0,5)!="func_") FuncName = "func_"+FuncName;
				std::string FuncSwitch = pFunc->GetAtrVal("switch"); if (FuncSwitch == "n/a") FuncSwitch = "off";
				if (m_IgnorePrivate && pFunc->GetAtrVal("private") == "yes") FuncSwitch = "off";
				std::string FuncID = pFunc->GetAtrVal("id");
				//int FunctionOrder = CEMUtilities::StrToInt(pFunc->GetAtrVal("order"));
				++FunctionOrder; //see comment policy order above
				if(!m_Systems[i].TakePolParam(int(m_Systems[i].m_Pols.size())-1, FuncName, FuncSwitch, FuncID, FunctionOrder)) return 0;

				//P A R A M E T E R S
				std::map<int, CEMXML*> ParOrder; ParOrder.clear();
				for(j=0; j<pFunc->m_Ch.size(); ++j)
				{
					if(CEMUtilities::LCase(pFunc->m_Ch[j]->m_Name) != "parameter")
						continue; //ignore other function-children (if there are any)
					Val=pFunc->m_Ch[j]->GetAtrVal("order");
					int order = CEMUtilities::StrToInt(Val, &err);
					if(Val.empty() || err) return CEMError::CodeErr("Failed to read input file '" + file + "' (err5).", "Check if the XML-code is correct.");
					ParOrder.insert(std::pair<int,CEMXML*>(order, pFunc->m_Ch[j]));
				}
				int ParameterOrder = 0;
				for(std::map<int, CEMXML*>::iterator itp=ParOrder.begin(); itp!=ParOrder.end(); ++itp) 
				{
					CEMXML *pPar = itp->second;
					std::string ParName = pPar->GetAtrVal("name");
					std::string ParVal = pPar->GetAtrVal("value");
					std::string ParID = pPar->GetAtrVal("id");
					std::string ParGroup = pPar->GetAtrVal("group");
					if (m_IgnorePrivate && pPar->GetAtrVal("private") == "yes") ParVal = "n/a";
					//int ParameterOrder = CEMUtilities::StrToInt(pPar->GetAtrVal("order"));
					++ParameterOrder; //see comment policy order above
					if (ParVal.empty() && PolSwitch == "on" && FuncSwitch == "on")
					{
						CEMError::NonCritErr("Empty value found", ParID, m_Systems[i].m_Name, PolName, FuncName, ParName);
						continue;
					}
					if(!m_Systems[i].TakePolParam(int(m_Systems[i].m_Pols.size())-1, ParName, ParVal, ParID, ParameterOrder, ParGroup)) return 0;
				}
			}
		}

		CEMXML *pDBSys;
		XMLAtr ident1; ident1.Name="SystemID"; ident1.Val=m_Systems[i].m_Id;
		XMLAtr ident2; ident2.Name="DataBaseID"; ident2.Val=m_DataSetId;
		pDBSys=DataConfigXML.GetNode("DBSystemConfig", &ident1, &ident2);
		if(!pDBSys) return CEMError::CodeErr("Failed to find node 'DBSystemConfig' with 'SystemID'='"+m_Systems[i].m_Id+"' and 'DataBaseID'='"+m_DataSetId+"' in input file '" + file + "'.", "Check if the XML-code is correct.");
		Val=pDBSys->GetAtrVal("uprate"); if(Val!="no" && Val!="n/a") m_Systems[i].m_Uprate = Val;
		Val=pDBSys->GetAtrVal("usedefault"); if(m_UseDefault.empty() && Val!="no" && Val!="n/a") m_UseDefault=Val; //actually not system specific, therefore take from first selected system
		//Val=pDBSys->GetAtrVal("usecommondefault"); if(Val=="yes") m_UseCommonDefault=1; //to late to read it here, therefore written to (and read from) config
																					      //Feb 2012: UseCommonDefault transferred to database level (can be deleted on db-sys-level once not used anymore)
	}

	if(!TXT_ReadAOParam()) return 0; //keep txt-version to enable intermediate solution for mtr-calculations

	return 1;
}

CEMXML *CEMControl::XML_ReadInput_GetReferencePolicy(std::string RefPol, std::map<int, CEMXML*>PolOrder)
{
	for(std::map<int, CEMXML*>::iterator itp=PolOrder.begin(); itp!=PolOrder.end(); ++itp) 
	{
		CEMXML *pPol = itp->second;
		if(pPol->GetAtrVal("id")==RefPol) return pPol;
	}
	CEMError::CodeErr("Failed to find reference policy id "+RefPol+" in input file '" + m_ParamPath + this->m_ControlName + "'.", "Check if the XML-code is correct.");
	return NULL;
}

bool CEMControl::XML_ReadVardesc()
{
	std::string file = m_ConfigPath + "VarConfig.xml";
	CEMXML xml, *pXml;
	if(!xml.LoadFile(file)) return CEMError::CodeErr("Failed to read input file '" + file + "' (err6).", "Check if the XML-code is correct.");
	pXml=xml.GetNode("VarConfig");
	if(!pXml) return CEMError::CodeErr("Failed to find node 'VarConfig' in input file '" + file + "'.", "Check if the XML-code is correct.");
	for(size_t i=0; i<pXml->m_Ch.size(); ++i)
	{
		CEMVar v;
		v.m_Name=pXml->m_Ch[i]->GetAtrVal("Name");
		if(CEMUtilities::StrToInt(pXml->m_Ch[i]->GetAtrVal("Monetary"))) v.m_Monetary=1; else v.m_Monetary=0;
/*		std::string def=pXml->m_Ch[i]->GetAtrVal("Default");
		if(def.empty())
		{
			v.m_DefVal = CEMUtilities::StrToDouble(pXml->m_Ch[i]->GetAtrVal("DefaultValue"), &err);
			if(err && !CEMError::CritErr("Default value of variable '"+v.m_Name+"' is not a valid number.", "file 'VarConfig.xlm'", "", "", "", "", pXml->m_Ch[i]->GetAtrVal("DefaultValue"))) return 0;
			v.m_DefVar=pXml->m_Ch[i]->GetAtrVal("DefaultVariable");
		}
		else
		{
			v.m_DefVal = CEMUtilities::StrToDouble(def, &err);
			if(err) { v.m_DefVal=EMVOID; v.m_DefVar = def; }
			else v.m_DefVar = "n/a";
		}*/
		v.m_SumTU = v.m_Monetary;
		m_Vardesc.insert(std::pair<std::string,CEMVar>(v.m_Name, v));
	}
	m_SimInfoAvailable=1;
	return 1;
}

bool CEMControl::TXT_ReadVardesc()
{
	std::string line;
	std::vector<std::string> tokline;
	std::string VardescPath = m_ParamPath + "variables.txt";
	std::ifstream ifs(VardescPath.c_str());
	int lnum = 0;
	if(ifs.is_open())
	{
		bool HeaderRead=0;
		unsigned int NameCol=999, MonCol=999, /*DefVarCol=999, DefValCol=999,*/ DefaultCol=999, MaxCol=0;
		m_SimInfoAvailable=0;
		while(!ifs.eof())
		{
			std::getline(ifs,line);
			++lnum;
			line = CEMUtilities::Trim(line);
			tokline = CEMUtilities::TokeniseLine(line, m_Country);
			if(!HeaderRead)
			{//find header line by searching for label sys_var_name
				unsigned int i;
				for(i=0; i<tokline.size(); ++i)
					if(!tokline[i].compare(LAB_VARNAME)) break;
				if(i>=tokline.size()) continue;
				for(i=0; i<tokline.size(); ++i)
				{
					if(!tokline[i].compare(LAB_VARNAME)) NameCol=i;
					else if (!tokline[i].compare(LAB_MON)) MonCol=i;
					//else if (!tokline[i].compare(LAB_DEFVAR)) DefVarCol=i;
					//else if (!tokline[i].compare(LAB_DEFVAL)) DefValCol=i;
					//else if (!tokline[i].compare(LAB_DEFAULT)) DefaultCol=i;
					else if (!tokline[i].compare(LAB_MODGEN)) m_SimInfoAvailable=1;
				}
				char formerr[500] = "";
				if(NameCol==999) EM_SPRINTF(formerr, "Label '%s' not found in sheet 'variables' of file 'variables.xls'.\n", LAB_VARNAME);
				if(MonCol==999) EM_SPRINTF(formerr, "%sLabel '%s' not found in sheet 'variables' of file 'variables.xls'.\n", formerr, LAB_MON);
				//if(DefaultCol==999 && DefVarCol==999) EM_SPRINTF(formerr, "%sLabel '%s' (new) resp. '%s' (old) not found in sheet 'variables' of file 'variables.xls'.\n", formerr, LAB_DEFAULT, LAB_DEFVAR);
				//if(DefaultCol==999 && DefValCol==999) EM_SPRINTF(formerr, "%sLabel '%s' (new) resp. '%s' (old) not found in sheet 'variables' of file 'variables.xls'.", formerr, LAB_DEFAULT, LAB_DEFVAL);
				if(strlen(formerr)>0) return CEMError::ErrNotif(formerr);
				if(MaxCol<NameCol) MaxCol=NameCol;
				if(MaxCol<MonCol) MaxCol=MonCol;
				//if(DefVarCol!=999 && MaxCol<DefVarCol) MaxCol=DefVarCol;
				//if(DefValCol!=999 && MaxCol<DefValCol) MaxCol=DefValCol;
				if(DefaultCol!=999 && MaxCol<DefaultCol) MaxCol=DefaultCol;
				HeaderRead=1;
				continue;
			}
			if(!tokline[NameCol].compare(LAB_ENDVAR)) break;

			CEMVar v;
			v.m_Name = tokline[NameCol];
			if(CEMUtilities::StrToInt(tokline[MonCol].c_str())) v.m_Monetary=1;
/*			bool err;
			if(DefaultCol==999)
			{
				v.m_DefVal = CEMUtilities::StrToDouble(tokline[DefValCol], &err);
				if(err && !CEMError::CritErr("Default value of variable '"+v.m_Name+"' is not a valid number.", "file 'variables.xls', sheet 'variables'", "", "", "", "", tokline[DefValCol])) return 0;
				v.m_DefVar = tokline[DefVarCol];
			}
			else
			{
				v.m_DefVal = CEMUtilities::StrToDouble(tokline[DefaultCol], &err);
				if(err) { v.m_DefVal=EMVOID; v.m_DefVar = tokline[DefaultCol]; }
				else v.m_DefVar = "n/a";
			}*/
			v.m_SumTU = v.m_Monetary;
			m_Vardesc.insert(std::pair<std::string,CEMVar>(v.m_Name, v));
		}
		ifs.close();
		if(!m_Vardesc.size()) return CEMError::CodeErr("No variables found in variable description file.", "Check format of parameter file 'variables.xls'.");
	}
	else return CEMError::CodeErr("Variable description file '" + VardescPath + "' could not be opened.", "Try to re-run the EUROMOD run tool.");
	return 1;
}

bool CEMControl::ReadData()
{
	std::string line;
	std::vector<std::string> tokline;
	std::string Data = m_DataPath + m_DataSet;
	std::ifstream InputData(Data.c_str());
	int lnum = 0;
	char err[500];

	long nHHRead=0;

	if(InputData.is_open())
	{
		std::vector<int> ModVars;
		unsigned int HHIDCol=999;
		size_t LineLength=0;
		double prevHHID=-1;
		while(!InputData.eof())
		{
			std::getline(InputData,line);
			++lnum;
			line = CEMUtilities::Trim(line);
			if(line.empty()) continue;
			tokline = CEMUtilities::TokeniseLine(line, m_Country);
			if(lnum==1)
			{//analyse header row
				unsigned int vind=0;
				for(unsigned int i=0; i<tokline.size(); ++i)
				{
					std::map<std::string,CEMVar>::iterator it = m_Vardesc.find(tokline[i]);
					if (it == m_Vardesc.end())
					{
						if (!m_ReadXVariables) continue; // usually ignore variables not existing in variables file
						// new since September 2017: data-configuration can define to read all expenditure variables
						if (tokline[i].length() <= 0 ||	// expenditure variable must start with x, q or p ...
							(tokline[i][0] != 'x' && tokline[i][0] != 'q' && tokline[i][0] != 'p')) continue;
						bool isNum = 1;					// ... and contain only numbers after that
						for (int x = 1; x < (int)tokline[i].length(); ++x) if (tokline[i][x] < '0' || tokline[i][x] > '9') { isNum = 0; break; }
						if (!isNum) continue;
						// simplest approach: pretend the variable is defined in the variables file
						CEMVar v; v.m_Name = tokline[i];
						v.m_Monetary = v.m_Name[0] != 'q';
						v.m_SumTU = v.m_Monetary;
						m_Vardesc.insert(std::pair<std::string, CEMVar>(v.m_Name, v));
						it = m_Vardesc.find(v.m_Name);
					}
					if(!tokline[i].compare(VARNAME_HHID)) HHIDCol=i;
					CEMVar v = it->second;
					v.m_Index=vind; v.m_GenType=VARTYPE_DATA;
					EM_SPRINTF(err, "Multiple occurrence of variable '%s' in data.", tokline[i].c_str());
					if(m_VarIndex.find(tokline[i])!=m_VarIndex.end()) { CEMError::CritErr(err, m_DataSet); return 0; }
					m_VarIndex.insert(std::pair<std::string,CEMVar>(tokline[i],v));
					m_VarIndexV.insert(m_VarIndexV.end(),v);
					ModVars.insert(ModVars.end(), i);
					++vind;
				}
				EM_SPRINTF(err, "Variable '%s' not found in dataset '%s'.", VARNAME_HHID, m_DataSet.c_str());
				if(HHIDCol==999) { CEMError::CritErr(err, m_DataSet); return 0;}
				LineLength=tokline.size();
				continue;
			}
			if(tokline.size()!=LineLength)
			{
				std::string ShortLine=CEMUtilities::Trim(line,1);
				if(ShortLine.empty()) continue; //probably empty line at the end of dataset
				for(size_t v=0; v<tokline.size(); ++v) ShortLine+=tokline[v]+"\t";
				if(!CEMError::NonCritErr("Data-row too short: "+ShortLine, m_DataSet, "", "", "", "", "", "Row is ignored.")) return 0;
				continue;
			}
			bool readerr;
			double actHHID=CEMUtilities::StrToDouble(tokline[HHIDCol], &readerr, 0);
			if(readerr) return CEMError::ErrNotif("Error reading data: HHID '"+tokline[HHIDCol]+"' is not a valid number.");
			if(m_StartHHID>0 && actHHID<m_StartHHID) continue; if(m_LastHHID>0 && actHHID>m_LastHHID) break;

			if(prevHHID!=actHHID)
			{
				if (nHHRead >= m_FirstNHHOnly) break;

				if(prevHHID>actHHID)
				{
					CEMError::CritErr("Data is not sorted by household ID!", m_DataSet);
					return 0;
				}
				CEMHousehold hh;
				for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++)
					it->m_Households.insert(it->m_Households.end(), hh);
				prevHHID = actHHID;

				++nHHRead;
			}
			PersonVar_t pers;
			for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++)
			{
				it->m_Households.back().m_Persons.insert(it->m_Households.back().m_Persons.end(), pers);
				for(unsigned int i=0; i<ModVars.size(); ++i)
				{
					bool readerr;
					std::string sval = CEMUtilities::Trim(tokline[ModVars[i]]);
					if(sval.empty()) return CEMError::ErrNotif("Error reading data: variable '"+m_VarIndexV[i].m_Name+"' contains missing values.");
					double dval;
					dval = CEMUtilities::StrToDouble(sval, &readerr, 0);
					if(readerr) return CEMError::ErrNotif("Error reading data: variable value '"+sval+"' is not a valid number.");
					it->m_Households.back().m_Persons.back().insert(it->m_Households.back().m_Persons.back().end(), dval);
				}
			}
		}
		InputData.close();
		if(m_Systems[0].m_Households.size()==0)
		{
			CEMError::CritErr("No households could be read (maybe you should check the 'func_dataset' parameters 'start_hhid' and 'last_hhid').", m_DataSet);
			return 0;
		}
	}
	else { CEMError::CritErr("Input data file '" + m_DataSet + "' could not be opened.", m_DataPath); return 0; }
	
	//initialise internally used variables (for assessing internal formulas or conditions, e.g. in func_allocate, func_dropkeepunit, etc.)
	if(!GetVarIndex(VARNAME_INTERNAL1, m_ivInternal1, CREATE_INTERNAL, VARTYPE_NONDESC, 1)) return CEMError::CodeErr("Programming error: system variable 'internal1' could not be created.");
	if(!GetVarIndex(VARNAME_INTERNAL2, m_ivInternal2, CREATE_INTERNAL, VARTYPE_NONDESC, 1)) return CEMError::CodeErr("Programming error: system variable 'internal2' could not be created.");

	if(!GetVarIndex("sel_s", m_ivElig, CREATE_INTERNAL, VARTYPE_NONDESC, 0)) return CEMError::CodeErr("Programming error: system variable 'sel_s' could not be created.");

	//initialise variable for harmonised consumer price index (which is latter either filled via global table (new approach) or via uprating-factors policy (old approach) or ignored)
	int iHicp; if(!GetVarIndex("$hicp", iHicp, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 1, 0)) return CEMError::CodeErr("Programming error: adding constant '$HICP' to variable list failed.");

	return 1;
}

bool CEMControl::CreateDefaultVariables()
{
	//remark: if this option is used with large datasets and many systems the memory capacities may be exhausted
	//but it is assumed that the option serves test pruposes only

	for (Vardesc_t::iterator itv=m_Vardesc.begin(); itv != m_Vardesc.end(); itv++)
	{//run through all variables in variable description file
		CEMVar v = itv->second;
		if (m_VarIndex.find(v.m_Name) != m_VarIndex.end())
			continue; //nothing to do with variables already found in data

		size_t i=v.m_Name.find("_s");
		if(i!=std::string::npos && v.m_Name.length()<=i+2)
			continue; //last two characters are _s: a simulated variable, do not create a default variable
		
		//add all variables to variable description
		v.m_Index = (int)m_VarIndex.size();
		v.m_GenType = VARTYPE_DEFAULT;
		m_VarIndex.insert(std::pair<std::string,CEMVar>(v.m_Name, v));
		m_VarIndexV.insert(m_VarIndexV.end(), v);
	}

	//add variables to personal variables
	for(size_t i=m_Systems[0].m_Households[0].m_Persons[0].size(); i<m_VarIndexV.size(); ++i) //loop over variables that need to be added
	{
		for(SystemContainer_t::iterator its=m_Systems.begin(); its!=m_Systems.end(); its++) //loop over systems
			for(HHContainer_t::iterator ith=its->m_Households.begin(); ith!=its->m_Households.end(); ith++) //loop over households (within system)
				for(PersonVarContainer_t::iterator itp=ith->m_Persons.begin(); itp!=ith->m_Persons.end(); itp++) //loop over persons within household
					itp->insert(itp->end(), 0);
	}
	return 1;
}

//function not used anymore
bool CEMControl::GetDefaultValues()
{
	for(Vardesc_t::iterator itv=m_Vardesc.begin(); itv!=m_Vardesc.end(); itv++)
	{//run through all variables in variable description file
		CEMVar v = itv->second;
		if(m_VarIndex.find(v.m_Name)!=m_VarIndex.end()) continue; //nothing to do with variables already found in data
		
		//until 14.5.2011: add all variables having either a default value or a default variable to variable description
		//if(v.m_DefVal==EMVOID && (v.m_DefVar=="" || v.m_DefVar=="n/a" || v.m_DefVar=="n\a")) continue;
		//now add all variables to variable description
		v.m_Index=(int)m_VarIndex.size(); v.m_GenType=VARTYPE_DEFAULT;
		m_VarIndex.insert(std::pair<std::string,CEMVar>(v.m_Name,v));
		m_VarIndexV.insert(m_VarIndexV.end(),v);
	}

	//add variables using default values to personal variables
	for(size_t i=m_Systems[0].m_Households[0].m_Persons[0].size(); i<m_VarIndexV.size(); ++i) //loop over variables that need to be added
	{
		/*
		double defvarval=EMVOID;
		int vind=-1;
		if(m_VarIndexV[i].m_DefVar!="" && m_VarIndexV[i].m_DefVar!="n/a" && m_VarIndexV[i].m_DefVar!="n\a") //variables with default variable
		{
			std::string err = "Default variable '" + m_VarIndexV[i].m_DefVar;
			err += "' for variable '" + m_VarIndexV[i].m_Name + "' not found.";
			if(!GetVarIndex(m_VarIndexV[i].m_DefVar, vind) || (size_t)vind >= m_Systems[0].m_Households[0].m_Persons[0].size())
			{
				if(!CEMError::CritErr(err, "file 'variables.xls', sheet 'variables'")) return 0;
				continue;
			}
		}
		else //variables with default value
			defvarval=m_VarIndexV[i].m_DefVal;
		*/

		for(SystemContainer_t::iterator its=m_Systems.begin(); its!=m_Systems.end(); its++) //loop over systems
			for(HHContainer_t::iterator ith=its->m_Households.begin(); ith!=its->m_Households.end(); ith++) //loop over households (within system)
				for(PersonVarContainer_t::iterator itp=ith->m_Persons.begin(); itp!=ith->m_Persons.end(); itp++) //loop over persons within household
				{
					if(m_UseCommonDefault)
					{
						/*if(vind!=-1) //variables with default variable
							itp->insert(itp->end(), itp->at(vind));
						else //variables with default value
							itp->insert(itp->end(), defvarval);
						*/
						itp->insert(itp->end(), 0);
					}
					else itp->insert(itp->end(), EMVOID);
				}
	}
	return 1;
}

bool CEMControl::TXT_ReadConfigParam()
{
	std::string line;
	std::vector<std::string> tokline;
	std::ifstream ifs(m_ConfigName.c_str());
	m_AddOns.clear();
	int lnum = 0;
	if(!ifs.is_open()) return CEMError::CodeErr("Configuration file '" + m_ConfigName + "' could not be opened.", "Try to re-run the EUROMOD run tool.");
	while(!ifs.eof())
	{
		std::getline(ifs,line);
		++lnum;
		line = CEMUtilities::Trim(line);
		if(line.empty() || line[0]=='*') continue;
		tokline = CEMUtilities::TokeniseLine(line, m_Country);
		if(!tokline[0].compare("errlog_file") && tokline.size()>1)
			m_ErrLogFile = CEMUtilities::RemoveQuotes(tokline[1]);
		else if(!tokline[0].compare("log_warnings") && tokline.size()>1)
			m_LogWarnings=CEMUtilities::CheckYesNo(tokline[1]);
		else if(!tokline[0].compare("emversion") && tokline.size()>1)
			m_EMVersion = tokline[1];
		else if(!tokline[0].compare("uiversion") && tokline.size()>1)
			m_UIVersion = tokline[1];
		else if(!tokline[0].compare("parampath") && tokline.size()>1)
			m_ParamPath = m_AOParamPath = CEMUtilities::CheckPathName(tokline[1]);
		else if(!tokline[0].compare("outputpath") && tokline.size()>1)
			m_OutputPath = CEMUtilities::CheckPathName(tokline[1]);
		else if(!tokline[0].compare("datapath") && tokline.size()>1)
			m_DataPath = CEMUtilities::CheckPathName(tokline[1]);
		else if(!tokline[0].compare("datasetname") && tokline.size()>1)
			m_DataSet = CEMUtilities::AppendExtension(tokline[1]);
		else if(!tokline[0].compare("header_date") && tokline.size()>1)
			m_HeaderDate = tokline[1];
		else if(!tokline[0].compare("outfile_date") && tokline.size()>1)
			m_OutfileDate = tokline[1];
		else if(!tokline[0].compare("log_runtime") && tokline.size()>1)
			m_LogRunTime=CEMUtilities::CheckYesNo(tokline[1]);
		else if(!tokline[0].compare("last_run") && tokline.size()>1)
			m_LastRun=CEMUtilities::CheckYesNo(tokline[1]);
		else if(!tokline[0].compare("decsign_param") && tokline.size()>1)
			CEMUtilities::CheckDecSign(tokline[1], m_ConfigName, 1);
		else if(!tokline[0].compare("control") && tokline.size()>1)
			m_ControlName = tokline[1];
		else if(!tokline[0].compare("spine") && tokline.size()>1)
			m_SpineName = tokline[1];
		else if(!tokline[0].compare("system") && tokline.size()>1)
		{
			CEMSystem EMSystem(this);
			EMSystem.m_Name = tokline[1];
			if(tokline.size()>2) EMSystem.m_AOControlName=tokline[2];
			if(tokline.size()>3) EMSystem.m_AOSystemName=tokline[3];
			m_Systems.insert(m_Systems.end(), EMSystem);
		}
		else if(!tokline[0].compare("addon_control") && tokline.size()>1)
		{
			struct AddOn AO; AO.m_ControlName=tokline[1];
			m_AddOns.insert(m_AddOns.end(), AO);
		}
	}
	ifs.close();
	return CheckConfigParam();
}

bool CEMControl::XML_ReadConfigParam()
{
	CEMXML xml, *pXml;
	if(!xml.LoadFile(m_ConfigName)) return CEMError::CodeErr("Failed to read input file '" + m_ConfigName + "' (err7).", "Check if the XML-code is correct.");
	pXml=xml.GetNode("EMConfig");
	if(!pXml) return CEMError::CodeErr("Failed to find node 'EMConfig' in input file '" + m_ConfigName + "'.", "Check if the XML-code is correct.");
	for(size_t i=0; i<pXml->m_Atr.size(); ++i)
	{
		bool err;
		std::string Name = CEMUtilities::LCase(pXml->m_Atr[i].Name);
		std::string Val = CEMUtilities::LCase(pXml->m_Atr[i].Val);
		if(Name=="errlog_file") m_ErrLogFile = CEMUtilities::RemoveQuotes(Val);
		else if(Name=="log_warnings") m_LogWarnings=CEMUtilities::CheckYesNo(Val);
		else if(Name=="emversion") m_EMVersion = Val;
		else if(Name=="uiversion") m_UIVersion = Val;
		else if(Name=="parampath")m_ParamPath = CEMUtilities::CheckPathName(pXml->m_Atr[i].Val);	// for paths, do NOT use the lower-case values
		else if(Name=="configpath") m_ConfigPath = CEMUtilities::CheckPathName(pXml->m_Atr[i].Val);	// for paths, do NOT use the lower-case values
		else if(Name=="emcontentpath") m_EMContentPath = CEMUtilities::CheckPathName(pXml->m_Atr[i].Val);	// for paths, do NOT use the lower-case values
		else if(Name=="outputpath") m_OutputPath = CEMUtilities::CheckPathName(pXml->m_Atr[i].Val);	// for paths, do NOT use the lower-case values
		else if(Name=="datapath") m_DataPath = CEMUtilities::CheckPathName(pXml->m_Atr[i].Val);	// for paths, do NOT use the lower-case values
		else if(Name=="dataset_id") m_DataSetId = Val;
		else if(Name=="header_date") m_HeaderDate = Val;
		else if(Name=="outfile_date") m_OutfileDate = Val;
		else if(Name=="log_runtime") m_LogRunTime=CEMUtilities::CheckYesNo(Val);
		else if(Name=="ispublicversion") m_IsPublicVersion=CEMUtilities::CheckYesNo(Val);
		else if(Name=="last_run") m_LastRun=CEMUtilities::CheckYesNo(Val);
		else if(Name=="decsign_param") CEMUtilities::CheckDecSign(Val, m_ConfigName, 1);
		else if(Name=="starthh")
		{
			m_StartHHID = CEMUtilities::StrToDouble(Val, &err);
			if(err) return CEMError::CodeErr("StartHH is not a valid number.", "Check in file '"+m_ConfigName+"'.");
		}
		else if(Name=="lasthh")
		{
			m_LastHHID = CEMUtilities::StrToDouble(Val, &err);
			if(err) return CEMError::CodeErr("LastHH is not a valid number.", "Check in file '"+m_ConfigName+"'.");
		}
		else if(Name=="country_file") m_ControlName = Val;
		else if(Name=="dataconfig_file") m_DataConfig = Val;
		else if(Name=="system_id")
		{
			CEMSystem EMSystem(this);
			EMSystem.m_Id = Val;
			EMSystem.m_SpineOn = 1; //not necessary with new interface
			m_Systems.insert(m_Systems.end(), EMSystem);
		}
		//intermediate solution to enable mtr-calculations with xml-input
		else if(Name=="mtr_parampath")
		{
			m_AOParamPath = CEMUtilities::CheckPathName(Val);
			struct AddOn AO; AO.m_ControlName="ao_control_mtr";
			m_AddOns.insert(m_AddOns.end(), AO);
		}
		else if(Name=="mtr_system_id")
		{
			size_t sep = Val.find("#");
			CEMSystem EMSystem(this);
			EMSystem.m_Id = Val.substr(0,sep);
			EMSystem.m_SpineOn = 1;
			EMSystem.m_AOControlName="ao_control_mtr";
			EMSystem.m_AOSystemName=Val.substr(sep+1);
			m_Systems.insert(m_Systems.end(), EMSystem);
		}
		else if(Name=="usecommondefault") m_UseCommonDefault =CEMUtilities::CheckYesNo(Val); //Feb 2012: UseCommonDefault transferred to database level (can be deleted on db-sys-level once not used anymore)
		else if(Name=="policy_switch") //store policy switch, to use with respective system in XML_ReadInput_Param
		{
			std::vector<std::string> PolicySwitchElements = CEMUtilities::TokeniseLine(Val, "", '=');
			if (PolicySwitchElements.size() == 3) //policy switch must have 3 elements: 1) pattern, e.g. yem_??, 2) system-id, 3) value (on or off)
				m_PolicySwitches.push_back(PolicySwitchElements);
		}
		else if (Name=="ignore_private") m_IgnorePrivate = CEMUtilities::CheckYesNo(Val); //July 2016: new option for running only public components
		else if (Name == "first_n_hh_only") m_FirstNHHOnly = CEMUtilities::StrToInt(Val);
		else if (Name == "exchange_rate_date") m_ExchangeRateDate = Val;
		else return CEMError::CodeErr("Unknown attribute '"+Name+"'='"+Val+"' found in file '"+m_ConfigName+"'.", "Check if the XML-code is correct.");
	}

	return CheckConfigParam();
}

bool CEMControl::CheckConfigParam()
{
	if(m_ErrLogFile=="")
		return CEMError::CodeErr("Name of error logfile not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	m_ErrLogFile = CEMUtilities::AppendExtension(m_ErrLogFile);
	if(m_LogWarnings<0)
		return CEMError::CodeErr("Configuration parameter 'LOG_WARNINGS' must be set to 'yes' or '1' / 'no' or '0'.", "Try to re-run the EUROMOD run tool.");
	if(m_EMVersion=="")
		return CEMError::CodeErr("EUROMOD Version not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(m_ParamPath=="")
		return CEMError::CodeErr("Path to parameters in txt-format not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(!m_InputTXT && m_ConfigPath=="")
		return CEMError::CodeErr("Path to VarConfig.xml not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(m_OutputPath=="")
		return CEMError::CodeErr("Output path not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(m_DataPath=="")
		return CEMError::CodeErr("Data path not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if((m_InputTXT && m_DataSet=="") || (!m_InputTXT && this->m_DataSetId==""))
		return CEMError::CodeErr("Identifier of data set not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(m_LogRunTime<0)
		return CEMError::CodeErr("Configuration parameter 'LOG_RUNTIME' must be set to 'yes' or '1' / 'no' or '0'.", "Try to re-run the EUROMOD run tool.");
	if(m_LastRun<0)
		return CEMError::CodeErr("Configuration parameter 'LAST_RUN' must be set to 'yes' or '1' / 'no' or '0'.", "Try to re-run the EUROMOD run tool.");
	if(m_ControlName=="")
		return CEMError::CodeErr("Name of file with control parameters not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(m_InputTXT && m_SpineName=="")
		return CEMError::CodeErr("Name of file with spine parameters not defined in configuration file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	if(m_OutfileDate=="-") m_OutfileDate="";
	if(m_Systems.size()==0)
		return CEMError::CodeErr("No systems defined in file '" + m_ConfigName + "'.", "Try to re-run the EUROMOD run tool.");
	CEMError::m_Control=this;
	return 1;
}

bool CEMControl::TXT_ReadControlParam()
{
	std::string line;
	std::vector<std::string> tokline;
	std::string ControlPath = m_ParamPath + m_ControlName;
	std::ifstream ifs(ControlPath.c_str());
	if(ifs.is_open())
	{
		//analyse first line to find columns of sytems to run (between delimiters 'first_system' and 'end_system'
		std::getline(ifs,line);
		tokline = CEMUtilities::TokeniseLine(line, m_Country);
		//first find labels ...
		m_ParamNameCol = 999; m_FirstSystemCol = 999; m_EndSystemCol = m_ParamIdCol = 999;
		unsigned int i;
		for(i=0; i<tokline.size(); ++i)
		{
			if(m_EndSystemCol==999 && m_FirstSystemCol==999 && m_ParamIdCol==999 && m_ParamNameCol==999 && !tokline[i].compare(LAB_PARNAME)) m_ParamNameCol=i;
			if(m_EndSystemCol==999 && m_FirstSystemCol==999 && m_ParamIdCol==999 && m_ParamNameCol!=999 && !tokline[i].compare(LAB_PARID)) m_ParamIdCol=i;
			if(m_EndSystemCol==999 && m_FirstSystemCol==999 && m_ParamIdCol!=999 && m_ParamNameCol!=999 && !tokline[i].compare(LAB_FIRSTSYS)) m_FirstSystemCol=i;
			if(m_EndSystemCol==999 && m_FirstSystemCol!=999 && m_ParamIdCol!=999 && m_ParamNameCol!=999 && !tokline[i].compare(LAB_ENDSYS)) m_EndSystemCol=i;
			if(m_EndSystemCol!=999 && m_FirstSystemCol!=999 && m_ParamIdCol!=999 && m_ParamNameCol!=999) break;
		}
		char serr[500]="";
		if(m_ParamNameCol==999) EM_SPRINTF(serr, "Label '%s' not found or misplaced in '%s'.\n", LAB_PARNAME, m_ControlName.c_str());
		if(m_ParamIdCol==999) EM_SPRINTF(serr, "Label '%s' not found or misplaced in '%s'.\n", LAB_PARID, m_ControlName.c_str());
		if(m_FirstSystemCol==999) EM_SPRINTF(serr, "%sLabel '%s' not found or misplaced in '%s'.\n", serr, LAB_FIRSTSYS, m_ControlName.c_str());
		if(m_EndSystemCol==999) EM_SPRINTF(serr, "%sLabel '%s' not found or misplaced in '%s'.\n", serr, LAB_ENDSYS, m_ControlName.c_str());
		if(strlen(serr)>0) return CEMError::ErrNotif(serr);
		//... then find system columns
		SystemContainer_t::iterator it;
		for(it=m_Systems.begin(); it!=m_Systems.end(); it++)
		{
			for(i=m_FirstSystemCol+1; i<m_EndSystemCol; ++i)
				if(!tokline[i].compare(it->m_Name))
				{
					it->m_ParamValColumn=i;
					break;
				}
				if(i>=m_EndSystemCol) return CEMError::CodeErr("System '" + it->m_Name + "' not found in file '"+m_ControlName+"'.", "Try to re-run the EUROMOD run tool.");
		}

		//read the other lines (from label first_par to end_par) and store parameter or hand over to respective systems
		unsigned int lnum=1, headline=1, genset=1, dsset=0;
		bool err=0;
		while(!ifs.eof())
		{
			std::getline(ifs,line);
			++lnum;
			tokline = CEMUtilities::TokeniseLine(line, m_Country);
			if(tokline.size()-1<m_ParamNameCol)
			{
				if(line=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", m_ControlName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
				else { if (CEMError::CritErr("Line too short.", m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", "", line)) return 0; continue; }
				continue;
			}
			if(headline && !tokline[m_ParamNameCol].compare(LAB_FIRSTPAR)) { headline=0; continue; }
			if(headline) continue;
			if(!tokline[m_ParamNameCol].compare(LAB_ENDPAR)) break;
			std::string trimline = CEMUtilities::Trim(line, 1);
			if(trimline=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", m_ControlName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
			if(tokline.size()<m_EndSystemCol)
			{
				if (CEMError::CritErr("Line too short.", m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", "", line)) return 0;
				continue;
			}
			
			//general settings
			if(!tokline[m_ParamNameCol].compare("func_dataset")) genset=0; //general settings section is terminated after first datset description
			if(genset)
			{	//parameters which are equal for all systems (store in control structure)
				if(!tokline[m_ParamNameCol].compare("country"))
					m_Country = tokline[m_FirstSystemCol+1];
				//parameters which may be different for each system (hand over to respective system)
				else 
					for(it=m_Systems.begin(); it!=m_Systems.end(); it++)
						if(!it->TakeControlParam(tokline[m_ParamNameCol], tokline[it->m_ParamValColumn], lnum)) return 0;
				continue;
			}

			//dataset settings
			if(!dsset && !tokline[m_ParamNameCol].compare("func_dataset"))
			{//first find section where the dataset is described
				std::string tt = CEMUtilities::AppendExtension(tokline[m_Systems[0].m_ParamValColumn]);
				if(!tokline[m_Systems[0].m_ParamValColumn].compare(m_DataSet) || !tt.compare(m_DataSet)) { dsset=1; continue; }
			}
			if(!dsset) continue;
			if(dsset && !tokline[m_ParamNameCol].compare("func_dataset")) break;

			//parameters which are equal for all systems (store in control structure)
			if(!tokline[m_ParamNameCol].compare("datapath"))
			{//datapath indicated in run file may be overwritten in control
				if(tokline[m_Systems[0].m_ParamValColumn].compare("n/a"))
					m_DataPath = CEMUtilities::CheckPathName(tokline[m_Systems[0].m_ParamValColumn]);
			}
			else if(!tokline[m_ParamNameCol].compare("decsign_data"))
			{
				if(tokline[m_Systems[0].m_ParamValColumn].compare("n/a"))
				{ if(!CEMUtilities::CheckDecSign(tokline[m_Systems[0].m_ParamValColumn], m_ControlName, 0)) return 0; }
			}
			else if(!tokline[m_ParamNameCol].compare("currency_db"))
				{ if(!CEMUtilities::CheckCurrency(m_DataCurrencyEuro, tokline[m_Systems[0].m_ParamValColumn].c_str(), m_ControlName, lnum, "currency_db")) return 0; }
			else if(!tokline[m_ParamNameCol].compare("use_default"))
			{
				for(unsigned int udi=0; udi<m_Systems.size(); ++udi)
				{//use default is in principle not system specific, but may be specified not in the first system (as only a later system needs the defaults)
					m_UseDefault=CEMUtilities::LCase(tokline[m_Systems[udi].m_ParamValColumn]);
					if(m_UseDefault!="n/a" && m_UseDefault!="no")
						break;
				}
				if(m_UseDefault=="n/a" || m_UseDefault=="no") m_UseDefault.clear();
			}
			else if(!tokline[m_ParamNameCol].compare("use_commondefault"))
			{
				std::string ParVal=tokline[m_Systems[0].m_ParamValColumn];
				if(ParVal!="n/a")
					{ if(!CEMUtilities::CheckYesNo(m_UseCommonDefault, CEMUtilities::LCase(tokline[m_Systems[0].m_ParamValColumn]), m_ControlName, lnum, "use_commondefault")) return 0; }
			}
			else if(!tokline[m_ParamNameCol].compare("start_hhid"))
			{
				std::string ParVal=tokline[m_Systems[0].m_ParamValColumn];
				if(ParVal.compare("n/a"))
				{
					m_StartHHID = CEMUtilities::StrToDouble(ParVal, &err);
					if(err) { if(CEMError::CritErr("Parameter is not a valid number.", m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "start_hhid")) return 0; }
				}
			}
			else if(!tokline[m_ParamNameCol].compare("last_hhid"))
			{
				std::string ParVal=tokline[m_Systems[0].m_ParamValColumn];
				if(ParVal.compare("n/a"))
				{
					m_LastHHID = CEMUtilities::StrToDouble(ParVal, &err);
					if(err) { if(CEMError::CritErr("Parameter is not a valid number.", m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "last_hhid")) return 0; }
				}
			}
			else
				for(it=m_Systems.begin(); it!=m_Systems.end(); it++)
					if(!it->TakeControlParam(tokline[m_ParamNameCol], tokline[it->m_ParamValColumn], lnum)) return 0;
		}
		if(!dsset) return CEMError::CodeErr("Settings for dataset '" + m_DataSet + "' not found in sheet '"+m_ControlName+"'.", "Try to re-run the EUROMOD run tool.");
	}
	else return CEMError::CodeErr("Control file '" + ControlPath + "' could not be opened.", "Try to re-run the EUROMOD run tool.");
	return 1;
}

bool CEMControl::XML_ReadControlParam()
{
	std::string file = m_ParamPath + m_ControlName;
	CEMXML xml, *pXml; std::string Name, Val; size_t i;
	if(!xml.LoadFile(file)) return CEMError::CodeErr("Failed to read input file '" + file + "' (err8).", "Check if the XML-code is correct.");
	pXml=xml.GetNode("Country");
	if(!pXml) return CEMError::CodeErr("Failed to find node 'Country' in input file '" + file + "'.", "Check if the XML-code is correct.");
	for(i=0; i<pXml->m_Atr.size(); ++i)
	{
		Name = CEMUtilities::LCase(pXml->m_Atr[i].Name);
		Val = CEMUtilities::LCase(pXml->m_Atr[i].Val);
		if(Val.empty() || Val=="n/a") continue;
		if(Name=="shortname") m_Country = Val;
	}

	pXml = NULL;
	if (!m_CommandLineConfig)
	{
		pXml = DataConfigXML.GetNode("Database", m_DataSetId);
		if (!pXml) m_DataSet = CEMUtilities::AppendExtension(m_DataSetId); // this may happen if EM_Config contains data-name instead of data-id (i.e. a manually adapted EM_Config)
	}
	if (!pXml) // if started via command-line-arguments or searching for id failed (see above)
	{
		XMLAtr ident; ident.Name = "name"; ident.Val = m_DataSet; // try with .txt (was appended in TakeConfigParam)
		pXml = DataConfigXML.GetNode("Database", &ident);
		if (!pXml)
		{
			ident.Name = "name"; ident.Val = m_DataSet.substr(0, m_DataSet.length() - 4); // try without .txt
			pXml = DataConfigXML.GetNode("Database", &ident);
		}
		if (!pXml) return CEMError::CodeErr("Failed to find node 'Database' with 'Name/Id'='" + m_DataSet + "' in input file '" + m_ParamPath + m_DataConfig + "'.",
											m_CommandLineConfig ? "Check command line argument." : "Check if the XML-code is correct.");
		m_DataSetId = pXml->GetAtrVal("id");
		// note: dataset must be "registered" - otherwise one would need to request important settings via the command line, most prominent yearinc (for uprating)
	}

	bool DataDecsignRead = 0;
	for(i=0; i<pXml->m_Atr.size(); ++i)
	{
		Name = CEMUtilities::LCase(pXml->m_Atr[i].Name);
		Val = CEMUtilities::LCase(pXml->m_Atr[i].Val);
		if(Val.empty() || Val=="n/a") continue;
		if(Name=="name") m_DataSet=CEMUtilities::AppendExtension(Val);
		else if(Name=="filepath") m_DataPath = CEMUtilities::CheckPathName(Val);
		else if(Name=="decimalsign") { DataDecsignRead = 1; if(!CEMUtilities::CheckDecSign(Val, m_ControlName, 0)) return 0; }
		else if(Name=="currency") { if(!CEMUtilities::CheckCurrency(m_DataCurrencyEuro, Val.c_str(), m_ControlName, 0, "currency_db")) return 0; }
		else if(Name=="usecommondefault") { m_UseCommonDefault=CEMUtilities::CheckYesNo(Val); }
		else if(Name=="yearinc") { m_DataIncomeYear=CEMUtilities::StrToDouble(Val); }
		else if(Name=="readxvariables") { m_ReadXVariables=CEMUtilities::CheckYesNo(Val); }
	}
	if (!DataDecsignRead) { if(!CEMUtilities::CheckDecSign(".", m_ControlName, 0)) return 0; } //set point as default for data if parameter decimalsign was not found
	return 1;
}

bool CEMControl::TXT_ReadSpineParam()
{
	std::string line;
	std::vector<std::string> tokline;
	std::string SpinePath = m_ParamPath + m_SpineName;
	std::ifstream ifs(SpinePath.c_str());
	if(ifs.is_open())
	{
		unsigned int lnum=0, headline=1;
		while(!ifs.eof())
		{
			std::getline(ifs,line);
			++lnum;
			tokline = CEMUtilities::TokeniseLine(line, m_Country, '\t'); //for migration need case sensitivity

			if(lnum==1) { if(!CheckParSheetFormat(tokline, m_SpineName)) return 1; }
			if(tokline.size()-1<m_ParamNameCol)
			{
				if(line=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", m_SpineName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
				else { if(CEMError::CritErr("Line too short.", m_SpineName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", "", line)) return 0; continue; }
			}
			if(headline && !tokline[m_ParamNameCol].compare(LAB_FIRSTPAR)) { headline=0; continue; }
			if(headline) continue;
			if(!tokline[m_ParamNameCol].compare(LAB_ENDPAR)) break;
			std::string trimline = CEMUtilities::Trim(line, 1);
			if(trimline=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", m_SpineName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
			for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++)
			{
				if(tokline.size()-1<it->m_ParamValColumn)
				{
					if(CEMError::CritErr("Line too short.", m_SpineName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", "", line)) return 0;
					continue;
				}
				std::string id=tokline[m_ParamIdCol];
				std::string PolSwitch = tokline[it->m_ParamValColumn];
				if (PolSwitch == "n/a") PolSwitch = "off"; //mainly necessary if migrated back from XML, where switches are frequently set to n/a
				if(!it->TakeSpineParam(tokline[m_ParamNameCol], PolSwitch, id, lnum)) return 0;
			}
		}
	}
	else return CEMError::CodeErr("Spine file '" + SpinePath + "' could not be opened.", "Try to re-run the EUROMOD run tool.");
	return 1;
}

bool CEMControl::TXT_ReadPolicyParam()
{
	for(unsigned int p=0; p<m_Systems[0].m_Pols.size(); ++p)
	{
		//first check if no need to read policy, as it's switched off in all systems
		SystemContainer_t::iterator it;
		for(it=m_Systems.begin(); it!=m_Systems.end(); it++) if(it->m_Pols[p].m_Switch) break;
		if(it==m_Systems.end()) continue;

		std::string line;
		std::vector<std::string> tokline;
		std::string PolName = CEMUtilities::AppendExtension(m_Systems[0].m_Pols[p].m_Name);
		std::string PolPath = m_ParamPath + PolName;
		std::ifstream ifs(PolPath.c_str());
		if(ifs.is_open())
		{
			unsigned int lnum=0, headline=1;
			while(!ifs.eof())
			{
				std::getline(ifs,line);
				++lnum;
				tokline = CEMUtilities::TokeniseLine(line, m_Country, '\t'); //for migration need case sensitivity
				if(lnum==1) { if(!CheckParSheetFormat(tokline, PolName)) break; }
				if(tokline.size()-1<m_ParamNameCol)
				{
					if(line=="") { if(!CEMError::CritErr("Emtpy line.", PolName+"Line "+CEMUtilities::IntToStr(lnum)+".", "", PolName)) return 0; break; }
					else { if(!CEMError::CritErr("Line too short.", PolName+" line "+CEMUtilities::IntToStr(lnum)+".", "", PolName, "", "", line)) return 0; continue; }
				}
				if(headline && !tokline[m_ParamNameCol].compare(LAB_FIRSTPAR)) { headline=0; continue; }
				if(headline) continue;
				if(!tokline[m_ParamNameCol].compare(LAB_ENDPAR)) break;
				std::string trimline = CEMUtilities::Trim(line, 1);
				if(trimline=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", PolName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
				for(it=m_Systems.begin(); it!=m_Systems.end(); it++)
				{
					std::string val="";
					if(tokline.size()>it->m_ParamValColumn) val=tokline[it->m_ParamValColumn];
					std::string id;
					id=tokline[m_ParamIdCol];
					if(CEMUtilities::LCase(tokline[m_ParamNameCol]).substr(0,5) == "func_" && val == "n/a") val = "off"; //mainly necessary if migrated back from XML, where switches are frequently set to n/a
					if(!it->TakePolParam(p, tokline[m_ParamNameCol], val, id, lnum)) return 0;
				}
			}
		}
		else return CEMError::CritErr("Policy file not found at '"+m_ParamPath+"'.", "", "", PolName);
	}
	return 1;
}

bool CEMControl::CheckParam()
{
	if(m_Country=="")
		if(!CEMError::CritErr("Country acronym not defined.", m_ControlName, "", "", "", "country")) return 0;
	if(m_DataCurrencyEuro==-1)
	{
		if(!CEMError::NonCritErr("Curreny of data not defined (Euro is used as default).", m_ControlName, "", "", "", "currency_db")) return 0;
		m_DataCurrencyEuro=1;
	}
	if(!m_UseDefault.empty())
	{		
		bool found=0;
		for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++) //use default is in principle not system specific, but may be specified not in the first system (as only a later system needs the defaults)
			for(ModuleContainer_t::iterator itm=it->m_Modules.begin(); itm!=it->m_Modules.end(); itm++)
			{
				if((*itm)->m_Name==COMOD27)
				{
					if(!(*itm)->m_ParamAdmin.GetSwitchParam()->m_numValue) continue;
					if(!(*itm)->m_ParamAdmin.IsParamDefined("name")) continue;
					if((*itm)->m_ParamAdmin.GetGeneralParam("name")->m_strValue==m_UseDefault) { found=1; break; }				
				}
			}
		if(!found) { if(!CEMError::CritErr("No (active) func_SetDefault named '"+m_UseDefault+"' exists.", m_ControlName, "", "", "", "use_default")) return 0; }
	}
	// Check system param
	for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++)
		if(!it->CheckParam()) return 0;
	return 1;
}

bool CEMControl::Run()
{
	for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++)
		{ if(!it->Run()) return 0; }
	return 1;
}

bool CEMControl::CheckParSheetFormat(std::vector<std::string> tokline, std::string sheet, unsigned int FirstSystemCol, unsigned int EndSystemCol, unsigned int ParamNameCol, unsigned int ParamIdCol, unsigned int SysColumn, std::string SysName)
{
	if(!FirstSystemCol) FirstSystemCol=m_FirstSystemCol;
	if(!EndSystemCol) EndSystemCol=m_EndSystemCol;
	if(!ParamNameCol) ParamNameCol=m_ParamNameCol;
	if(!ParamIdCol) ParamIdCol=m_ParamIdCol;
	if(tokline.size()<=EndSystemCol) { CEMError::CritErr("First line too short", sheet); return 0; };
	char err[500]="";
	if(tokline[FirstSystemCol].compare(LAB_FIRSTSYS)) EM_SPRINTF(err, "Label '%s' not found or misplaced.\n", LAB_FIRSTSYS);
	if(tokline[EndSystemCol].compare(LAB_ENDSYS)) EM_SPRINTF(err, "%sLabel '%s' not found or misplaced.\n", err, LAB_ENDSYS);
	if(tokline[ParamIdCol].compare(LAB_PARID)) EM_SPRINTF(err, "%sLabel '%s' not found or misplaced.\n", err, LAB_PARID);
	if(tokline[ParamNameCol].compare(LAB_PARNAME)) EM_SPRINTF(err, "%sLabel '%s' not found or misplaced.\n", err, LAB_PARNAME);
	if(!SysColumn) //LCase: for migration need case sensitivity, normally anyway lowercase
	{
		for(SystemContainer_t::iterator it=m_Systems.begin(); it!=m_Systems.end(); it++)
			if(CEMUtilities::LCase(tokline[it->m_ParamValColumn]).compare(CEMUtilities::LCase(it->m_Name))) EM_SPRINTF(err, "%sColumn for system '%s' not found or misplaced.", err, it->m_Name.c_str());
	}
	else
		if(CEMUtilities::LCase(tokline[SysColumn]).compare(CEMUtilities::LCase(SysName))) EM_SPRINTF(err, "%sColumn for system '%s' not found or misplaced.", err, SysName.c_str());
	if(strlen(err)<=0) return 1;
	CEMError::CritErr(err, sheet);
	return 0;
}

bool CEMControl::GetVarIndex(std::string VarName, int &VarIndex, int CreateType, int VarType, int Monetary, int IsGlobal, int SumTU)
{
	Vardesc_t::iterator it; CEMVar v; size_t i; std::string vn=VarName;
	
	//first find out whether variable exists
	it = m_VarIndex.find(VarName);
	if(it!=m_VarIndex.end()) { VarIndex=it->second.m_Index; return 1; } //does exist

	if(CreateType==CREATE_NOT) return 0; //does not exist and should not be created

	switch(VarType)
	{
	//standard variable described in variables.xls (e.g. yem)
	case VARTYPE_DESC:
		if(!m_SimInfoAvailable && VarName.at(VarName.length()-2)=='_' && VarName.at(VarName.length()-1)=='s')
			vn=VarName.substr(0, VarName.length()-2);
		it = m_Vardesc.find(vn); //search variable name in variable description file (variables.xls)
		if(it!=m_Vardesc.end()) { v = it->second; v.m_GenType=VARTYPE_DESC; break; }
		else { if(CreateType!=CREATE_PARAM) return 0; }
	//variable derived from a standard variable described in variables.xls (i.e. variables generated by func_store, e.g. yem_bkup, yem_loop, yem_loop1, etc.)
	case VARTYPE_DERDESC:
		i=VarName.find("_s");
		if(i!=std::string::npos && VarName.length()<=i+2) return 0; //last two characters are _s: not a func_store-variable, but a not existing simulated variable
		i=VarName.rfind("_");
		if(i==std::string::npos) return 0;
		it = m_VarIndex.find(VarName.substr(0, i));
		if(it==m_VarIndex.end()) return 0;
		v = it->second;
		v.m_GenType=VARTYPE_DERDESC;
		break;	
	//variable not described in variables.xls (variables generated by func_loop, func_unitloop, func_totals, func_defvar, func_defconst)
	case VARTYPE_NONDESC:
		v.m_GenType=VARTYPE_NONDESC;
		break;
	}
	v.m_Name=VarName;

	if(Monetary!=-1) v.m_Monetary=(Monetary==1);
	if(IsGlobal!=-1) v.m_IsGlobal=(IsGlobal==1);
	if(SumTU!=-1) v.m_SumTU=(SumTU==1); else v.m_SumTU=v.m_Monetary;
	
	//insert new variable in storing arrays (m_VarIndex: sorted by name, m_VarIndexV: sorted by index)
	v.m_Index = (unsigned int)(m_VarIndex.size()); VarIndex = v.m_Index;
	m_VarIndex.insert(std::pair<std::string,CEMVar>(VarName,v));
	m_VarIndexV.insert(m_VarIndexV.end(),v);

	//add variable to each person's variable container and set it to VOID
	double InitVal=EMVOID;
	for(SystemContainer_t::iterator its=m_Systems.begin(); its!=m_Systems.end(); its++)
		for(HHContainer_t::iterator ith=its->m_Households.begin(); ith!=its->m_Households.end(); ith++)
			for(PersonVarContainer_t::iterator itp=ith->m_Persons.begin(); itp!=ith->m_Persons.end(); itp++)
			{
				try //on revision of the programme it should be considered if such try-catch blocks should be used more frequently
				{
					itp->insert(itp->end(), InitVal);
				}
				catch (std::bad_alloc& ba)
				{
					std::string what = ba.what();
					return CEMError::CodeErr("Generating variable failed with error '" + what + "'.", "The programme's space for variable may be exceeded. Consider using less systems, less loop-iterations or a declined dataset.");
				}
			}
	return 1;
}

bool CEMControl::AssessGenVarInd()
{
	char err[500];
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_PID);
	if(!GetVarIndex(VARNAME_PID, m_ivPID)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_HHID);
	if(!GetVarIndex(VARNAME_HHID, m_ivHHID)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_AGE);
	if(!GetVarIndex(VARNAME_AGE, m_ivAge)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_MARITAL_STATUS);
	if(!GetVarIndex(VARNAME_MARITAL_STATUS, m_ivMaritStat)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_LABSUP_STATUS);
	if(!GetVarIndex(VARNAME_LABSUP_STATUS, m_ivLabSupStat)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_EDUC_STATUS);
	if(!GetVarIndex(VARNAME_EDUC_STATUS, m_ivEducStat)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_OCC_STATUS);
	if(!GetVarIndex(VARNAME_OCC_STATUS, m_ivOcc)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_DISAB);
	if(!GetVarIndex(VARNAME_DISAB, m_ivDisab))
	{
		if(!GetVarIndex("ddilv", m_ivDisab)) { m_ivDisab=-1; if(!CEMError::NonCritErr(err, "", "", "", "", "", "", "Query 'IsDisabled' is ignored.")) return 0; }
		else { if(!CEMError::NonCritErr(err, "", "", "", "", "", "", "Variable 'ddilv' is used instead.")) return 0; }
	}
	if(!GetVarIndex(VARNAME_CIVIL_SERV, m_ivCivilServ)) m_ivCivilServ=-1; //variable is not compulsory
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_PARTNERID);
	if(!GetVarIndex(VARNAME_PARTNERID, m_ivPartnerID)) return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
	EM_SPRINTF(err, "Variable '%s' not found.", VARNAME_PARENTID);
	m_ivParentID=m_ivMotherID=m_ivFatherID=-1;
	GetVarIndex(VARNAME_PARENTID, m_ivParentID);
	if(!GetVarIndex(VARNAME_MOTHERID, m_ivMotherID))
	{
		if(m_ivParentID>=0) m_ivMotherID=m_ivParentID;
		else
		{
			EM_SPRINTF(err, "Neither variable '%s' nor '%s' found.", VARNAME_PARENTID, VARNAME_MOTHERID);
			return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
		}
	}
	if(!GetVarIndex(VARNAME_FATHERID, m_ivFatherID))
	{
		if(m_ivParentID>=0) m_ivFatherID=m_ivParentID;
		else
		{
			EM_SPRINTF(err, "Neither variable '%s' nor '%s' found.", VARNAME_PARENTID, VARNAME_FATHERID);
			return CEMError::CodeErr(err, "Check format of parameter file 'variables.xls'.");
		}
	}
	if(m_ivParentID==-1)
	{//acutally not necessary, because internally ParentID is not used
		if(m_ivMotherID>0) m_ivParentID=m_ivMotherID;
		else m_ivParentID=m_ivFatherID;
	}
	return 1;
}

void CEMControl::AddTimeRec(std::string Action,	std::string Actor, int Duration, size_t At)
{
	CTimeRec tr;
	tr.Action=Action;
	tr.Actor=Actor;
	tr.Duration=Duration;
	if(At==9999) m_TimeRec.insert(m_TimeRec.end(), tr);
	else m_TimeRec.insert(m_TimeRec.begin()+At, tr);
}

void CEMControl::LogRunTime()
{
	std::ofstream hFile;
	std::string fName = m_OutputPath + m_HeaderDate+ "_EMHeader.txt";
	hFile.open(fName.c_str(), std::ios_base::out | std::ios_base::app);
	if(!hFile.is_open()) return;
	hFile.seekp(std::ios_base::end);
	hFile << "====================================================================================" << std::endl;
	hFile << "TIME RECORD FOR DATASET '" + m_DataSet + "'" << std::endl;
	hFile << "ACTION\tACTOR\tDURATION IN SEC\n";
	int sysCnt = 0, modCnt = 0;
	for(size_t i=0; i<m_TimeRec.size(); ++i)
	{
		if(m_TimeRec[i].Actor=="") m_TimeRec[i].Actor="-";
		std::string ac=m_TimeRec[i].Action;
		if(ac=="RUN SYSTEM") { ++sysCnt; modCnt=0; }
		else if(ac=="run function") ++modCnt;
		hFile << m_TimeRec[i].Action << "\t"  << m_TimeRec[i].Actor << "\t" << 
				(m_TimeRec[i].Duration)*(1.0/CLOCKS_PER_SEC) << std::endl;
	}
	hFile << "====================================================================================" << std::endl << std::endl;
	hFile.close();
}

bool CEMControl::TXT_ReadAOControlParam()
{
	if(m_AddOns.empty()) return 1;
	for(size_t i=0; i<m_AddOns.size(); ++i)
	{
		std::string line;
		std::vector<std::string> tokline;
		std::string ControlPath = m_AOParamPath + CEMUtilities::AppendExtension(m_AddOns[i].m_ControlName);
		std::ifstream ifs(ControlPath.c_str());
		if(!ifs.is_open()) return CEMError::CodeErr("Add-on-control file '" + ControlPath + "' could not be opened.", "Try to re-run the EUROMOD run tool.");
		//analyse first line to find columns of sytems (between delimiters 'first_system' and 'end_system'
		std::getline(ifs,line);
		tokline = CEMUtilities::TokeniseLine(line, m_Country);
		//first find labels ...
		m_AddOns[i].m_ParamNameCol = m_AddOns[i].m_FirstSystemCol = m_AddOns[i].m_EndSystemCol = m_AddOns[i].m_ParamIdCol = 999;
		for(unsigned int j=0; j<tokline.size(); ++j)
		{
			if(m_AddOns[i].m_EndSystemCol==999 && m_AddOns[i].m_FirstSystemCol==999 && m_AddOns[i].m_ParamIdCol==999 && m_AddOns[i].m_ParamNameCol==999 && !tokline[j].compare(LAB_PARNAME)) m_AddOns[i].m_ParamNameCol=j;
			if(m_AddOns[i].m_EndSystemCol==999 && m_AddOns[i].m_FirstSystemCol==999 && m_AddOns[i].m_ParamIdCol==999 && m_AddOns[i].m_ParamNameCol!=999 && !tokline[j].compare(LAB_PARID)) m_AddOns[i].m_ParamIdCol=j;
			if(m_AddOns[i].m_EndSystemCol==999 && m_AddOns[i].m_FirstSystemCol==999 && m_AddOns[i].m_ParamIdCol!=999 && m_AddOns[i].m_ParamNameCol!=999 && !tokline[j].compare(LAB_FIRSTSYS)) m_AddOns[i].m_FirstSystemCol=j;
			if(m_AddOns[i].m_EndSystemCol==999 && m_AddOns[i].m_FirstSystemCol!=999 && m_AddOns[i].m_ParamIdCol!=999 && m_AddOns[i].m_ParamNameCol!=999 && !tokline[j].compare(LAB_ENDSYS)) m_AddOns[i].m_EndSystemCol=j;
			if(m_AddOns[i].m_EndSystemCol!=999 && m_AddOns[i].m_FirstSystemCol!=999 && m_AddOns[i].m_ParamIdCol!=999 && m_AddOns[i].m_ParamNameCol!=999) break;
		}
		char serr[500]="";
		if(m_AddOns[i].m_ParamNameCol==999) EM_SPRINTF(serr, "Label '%s' not found or misplaced in '%s'.\n", LAB_PARNAME, m_AddOns[i].m_ControlName.c_str());
		if(m_AddOns[i].m_ParamIdCol==999) EM_SPRINTF(serr, "Label '%s' not found or misplaced in '%s'.\n", LAB_PARID, m_AddOns[i].m_ControlName.c_str());
		if(m_AddOns[i].m_FirstSystemCol==999) EM_SPRINTF(serr, "%sLabel '%s' not found or misplaced in '%s'.\n", serr, LAB_FIRSTSYS, m_AddOns[i].m_ControlName.c_str());
		if(m_AddOns[i].m_EndSystemCol==999) EM_SPRINTF(serr, "%sLabel '%s' not found or misplaced in '%s'.\n", serr, LAB_ENDSYS, m_AddOns[i].m_ControlName.c_str());
if(strlen(serr)>0) return CEMError::ErrNotif(serr);

		//... then read system names ...
		for(unsigned int j=m_AddOns[i].m_FirstSystemCol+1; j<m_AddOns[i].m_EndSystemCol; ++j)
		{
			AddOnSys AOS; AOS.m_AddOn = &(m_AddOns[i]); AOS.m_Name=tokline[j]; AOS.m_Col=j;
			m_AddOns[i].m_Systems.insert(m_AddOns[i].m_Systems.end(), AOS);
		}

		//... and find out whether they belong to any of the systems to run
		SystemContainer_t::iterator it;
		for(it=m_Systems.begin(); it!=m_Systems.end(); it++)
		{
			if(it->m_AOControlName!=m_AddOns[i].m_ControlName) continue;
			for(size_t j=0; j<m_AddOns[i].m_Systems.size(); ++j)
				if(m_AddOns[i].m_Systems[j].m_Name==it->m_AOSystemName)
				{
					it->m_AOSystem = &(m_AddOns[i].m_Systems[j]);
					break;
				}
		}

		//read the other lines (from label first_par to end_par)
		unsigned int lnum=1, headline=1; std::string funcType=""; char on[500]="000000000000000";
		while(!ifs.eof())
		{
			std::getline(ifs,line);
			++lnum;
			tokline = CEMUtilities::TokeniseLine(line, m_Country);
			if(tokline.size()-1<m_AddOns[i].m_ParamNameCol)
			{
				if(line=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
				else { if (CEMError::CritErr("Line too short.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", "", line)) return 0; continue; }
				continue;
			}
			if(headline && !tokline[m_AddOns[i].m_ParamNameCol].compare(LAB_FIRSTPAR)) { headline=0; continue; }
			if(headline) continue;
			std::string trimline = CEMUtilities::Trim(line, 1);
			if(trimline=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
			if(tokline.size()<m_AddOns[i].m_EndSystemCol && tokline[m_AddOns[i].m_ParamNameCol]!=LAB_ENDPAR)
			{
				if (CEMError::CritErr("Line too short.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", "", line)) return 0;
				continue;
			}
			if(!tokline[m_AddOns[i].m_ParamNameCol].substr(0,11).compare("func_addon_") || !tokline[m_AddOns[i].m_ParamNameCol].compare(LAB_ENDPAR))
			{
				if(!funcType.empty())
				{
					for(size_t j=0; j<m_AddOns[i].m_Systems.size(); ++j)
					{
						if(!on[j]) continue;
						if(funcType=="func_addon_pol")
						{
							struct AddOnEntry &rAOE = m_AddOns[i].m_Systems[j].m_Pols[m_AddOns[i].m_Systems[j].m_Pols.size()-1];
							if(rAOE.m_Ident.empty()) { if(!CEMError::CritErr("Compulsory parameter not indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_pol", "pol_name")) return 0; break; }
							if(rAOE.m_Pos.empty()) { if(!CEMError::CritErr("Compulsory parameter not indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_pol", "insert_before_pol/insert_after_pol")) return 0; break; }
						}
						else if(funcType=="func_addon_func")
						{
							struct AddOnEntry &rAOE = m_AddOns[i].m_Systems[j].m_Funcs[m_AddOns[i].m_Systems[j].m_Funcs.size()-1];
							if(rAOE.m_Ident.empty()) { if(!CEMError::CritErr("Compulsory parameter not indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_func", "id_func")) return 0; break; }
							if(rAOE.m_Pos.empty()) { if(!CEMError::CritErr("Compulsory parameter not indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_func", "insert_before_func/insert_before_func")) return 0; break; }
							if(rAOE.m_FuncDefPol.empty()) { if(!CEMError::CritErr("Compulsory parameter not indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_func", "def_pol")) return 0; break; }
						}
						else if(funcType=="func_addon_par")
						{
							struct AddOnEntry &rAOE = m_AddOns[i].m_Systems[j].m_Pars[m_AddOns[i].m_Systems[j].m_Pars.size()-1];
							if(rAOE.m_Ident.empty()) { if(!CEMError::CritErr("Compulsory parameter not indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_par", "insert_func")) return 0; break; }
							if(rAOE.m_ParNames.empty()) { if(!CEMError::CritErr("No add-on parameters indicated.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_par", "pari_name")) return 0; break; }
							if(rAOE.m_ParNames.size()!=rAOE.m_ParVals.size()) { if(!CEMError::CritErr("Number of parameters names does not correspond to number of parameter values.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(rAOE.m_Row), "", "", "func_addon_par", "pari_name/pari_value")) return 0; break; }
						}
					}
				}
				if(!tokline[m_AddOns[i].m_ParamNameCol].compare(LAB_ENDPAR)) break;
				funcType=tokline[m_AddOns[i].m_ParamNameCol];
				if(funcType!="func_addon_applic" && funcType!="func_addon_pol" && funcType!="func_addon_func" && funcType!="func_addon_par") { if(!CEMError::CritErr("Unknown function or function not allowed in add-on-control.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", funcType)) return 0; break; }
#ifdef _WIN32
				strcpy_s(on, "000000000000000");
#else	// linux does not support _s functions
				strcpy(on, "000000000000000");
#endif
				for(size_t j=0; j<m_AddOns[i].m_Systems.size(); ++j)
				{
					on[j]=(tokline[m_AddOns[i].m_FirstSystemCol+1+j]=="on"); if(!on[j]) continue;
					struct AddOnEntry AOE; AOE.m_Row=lnum;
					if(funcType=="func_addon_pol") m_AddOns[i].m_Systems[j].m_Pols.insert(m_AddOns[i].m_Systems[j].m_Pols.end(), AOE);
					if(funcType=="func_addon_func") m_AddOns[i].m_Systems[j].m_Funcs.insert(m_AddOns[i].m_Systems[j].m_Funcs.end(), AOE);
					if(funcType=="func_addon_par") m_AddOns[i].m_Systems[j].m_Pars.insert(m_AddOns[i].m_Systems[j].m_Pars.end(), AOE);
				}
			}
			else
			{
				if(funcType=="func_addon_applic") continue;
				std::string ParName = tokline[m_AddOns[i].m_ParamNameCol];
				if(funcType.empty()) { CEMError::CritErr("Misplaced parameter (allocation to function not possible).", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", ParName); return 0; }
				for(size_t j=0; j<m_AddOns[i].m_Systems.size(); ++j)
				{
					if(!on[j]) continue;
					std::string ParVal = tokline[m_AddOns[i].m_FirstSystemCol+1+j];
					if(funcType=="func_addon_pol")
					{
						struct AddOnEntry &rAOE = m_AddOns[i].m_Systems[j].m_Pols[m_AddOns[i].m_Systems[j].m_Pols.size()-1];
						if(ParName=="pol_name") rAOE.m_Ident=ParVal;
						else if (ParName=="insert_before_pol") { rAOE.m_Pos=ParVal; rAOE.m_Before=1; }
						else if (ParName=="insert_after_pol") { rAOE.m_Pos=ParVal; rAOE.m_Before=0; }
						else { if(!CEMError::CritErr("Unknown parameter.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", ParName, ParVal)) return 0; break; }
					}
					else if(funcType=="func_addon_func")
					{
						struct AddOnEntry &rAOE = m_AddOns[i].m_Systems[j].m_Funcs[m_AddOns[i].m_Systems[j].m_Funcs.size()-1];
						if(ParName=="func_id" || ParName=="id_func") rAOE.m_Ident=ParVal;
						else if(ParName=="def_pol") rAOE.m_FuncDefPol=ParVal;
						else if (ParName=="insert_before_func") { rAOE.m_Pos=ParVal; rAOE.m_Before=1; }
						else if (ParName=="insert_after_func") { rAOE.m_Pos=ParVal; rAOE.m_Before=0; }
						else { if(!CEMError::CritErr("Unknown parameter.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", ParName, ParVal)) return 0; break; }
					}
					else if(funcType=="func_addon_par")
					{
						struct AddOnEntry &rAOE = m_AddOns[i].m_Systems[j].m_Pars[m_AddOns[i].m_Systems[j].m_Pars.size()-1];
						int len=int(ParName.length());
						if(ParName=="insert_func") rAOE.m_Ident=ParVal;
						else if (ParName.substr(0,3)=="par" && ParName.substr((int)std::fmax(len-5,0))=="_name")
						{
							rAOE.m_ParNames.insert(rAOE.m_ParNames.end(), ParVal);
							std::string ParId = tokline[m_AddOns[i].m_ParamIdCol];
							rAOE.m_ParIds.insert(rAOE.m_ParIds.end(), CEMUtilities::GenId(m_AddOns[i].m_ControlName, ParId, lnum));
						}
						else if (ParName.substr(0,3)=="par" && ParName.substr((int)std::fmax(len-6,0))=="_value") rAOE.m_ParVals.insert(rAOE.m_ParVals.end(), ParVal);
						else { if(!CEMError::CritErr("Unknown parameter.", m_AddOns[i].m_ControlName+", line "+CEMUtilities::IntToStr(lnum), "", "", "", ParName, ParVal)) return 0; break; }
					}
				}
			}
		}
	}
	//check for usage of policy in add-ons with the same name as standard policies
	for(size_t x=0, y, z; x<m_AddOns.size(); ++x)
	{
		for(y=0; y<m_AddOns[x].m_Systems.size(); ++y)
		{
			for(size_t a=0, b; a<m_Systems.size(); ++a)
			{
				if(!m_Systems[a].m_AOSystem || m_Systems[a].m_AOSystem->m_AddOn->m_ControlName!=m_AddOns[x].m_ControlName  || m_Systems[a].m_AOSystem->m_Name!=m_AddOns[x].m_Systems[y].m_Name) continue;
				for(z=0; z<m_AddOns[x].m_Systems[y].m_Pols.size(); ++z)
				{
					std::string PolName = m_AddOns[x].m_Systems[y].m_Pols[z].m_Ident;
					for(b=0; b<m_Systems[a].m_Pols.size(); ++b) if(PolName==m_Systems[a].m_Pols[b].m_Name) { if(CEMError::CritErr("Multiple usage of policy name '"+PolName+"'.", m_AddOns[x].m_ControlName)) return 0; }
				}
				for(z=0; z<m_AddOns[x].m_Systems[y].m_Funcs.size(); ++z)
				{
					std::string PolName = m_AddOns[x].m_Systems[y].m_Funcs[z].m_FuncDefPol;
					for(b=0; b<m_Systems[a].m_Pols.size(); ++b) if(PolName==m_Systems[a].m_Pols[b].m_Name) { if(CEMError::CritErr("Multiple usage of policy name '"+PolName+"'.", m_AddOns[x].m_ControlName)) return 0; }
				}
			}
		}
	}
	return 1;
}

bool CEMControl::TXT_ReadAOParam()
{
	if(m_AddOns.empty()) return 1;
	for(size_t x=0, y, z; x<m_AddOns.size(); ++x)
	{
		//read add-on policies and fit in spine
		for(y=0; y<m_AddOns[x].m_Systems.size(); ++y)
		{
			for(z=0; z<m_AddOns[x].m_Systems[y].m_Pols.size(); ++z)
			{
				std::string PolName = m_AddOns[x].m_Systems[y].m_Pols[z].m_Ident;
				std::string InsPolName = m_AddOns[x].m_Systems[y].m_Pols[z].m_Pos;
				bool Before = m_AddOns[x].m_Systems[y].m_Pols[z].m_Before;
				for(size_t a=0, b, c; a<m_Systems.size(); ++a)
				{
					if(!m_Systems[a].m_AOSystem || m_Systems[a].m_AOSystem->m_AddOn->m_ControlName!=m_AddOns[x].m_ControlName  || m_Systems[a].m_AOSystem->m_Name!=m_AddOns[x].m_Systems[y].m_Name) continue;
					for(b=0; b<m_Systems[a].m_Pols.size(); ++b) if(InsPolName==m_Systems[a].m_Pols[b].m_Name) break;
					if(b>=m_Systems[a].m_Pols.size()) { if(!CEMError::CritErr("Unknown policy.", m_AddOns[x].m_ControlName+", line "+CEMUtilities::IntToStr(m_AddOns[x].m_Systems[y].m_Pols[z].m_Row), m_AddOns[x].m_Systems[y].m_Name, "", "", "insert_before_pol/insert_after_pol", InsPolName)) return 0; break; }
					CEMPolicy pol; pol.m_Name=PolName; pol.m_Switch=1;
					pol.m_Id=CEMUtilities::GenId(m_AddOns[x].m_ControlName, "", m_AddOns[x].m_Systems[y].m_Pols[z].m_Row);
					if(!Before) ++b; m_Systems[a].m_Pols.insert(m_Systems[a].m_Pols.begin()+b, pol);
					bool found=0;
					for(c=0; c<m_Systems[a].m_Modules.size(); ++c)
					{
						if(m_Systems[a].m_Modules[c]->m_PolName!=InsPolName) { if(!found) continue; break; }
						found=1; if(Before) break;
					}
					if(!found) { if(!CEMError::CritErr("Unknown policy (maybe switched off).", m_AddOns[x].m_ControlName+", line "+CEMUtilities::IntToStr(m_AddOns[x].m_Systems[y].m_Pols[z].m_Row), m_AddOns[x].m_Systems[y].m_Name, "", "", "insert_before_pol/insert_after_pol", InsPolName)) return 0; break; }
					if(!TXT_ReadAOPolicyParam(PolName, &(m_AddOns[x].m_Systems[y]), &(m_Systems[a]), (unsigned int)(b), (int)(c))) return 0;
				}
			}
		}
		//read add-on functions and fit in spine
		for(y=0; y<m_AddOns[x].m_Systems.size(); ++y)
		{	//gather the policies, where the functions are defined
			std::vector<std::string> DefPols; std::string IsIn=""; DefPols.clear();
			for(z=0; z<m_AddOns[x].m_Systems[y].m_Funcs.size(); ++z)
			{
				std::string DefPol = m_AddOns[x].m_Systems[y].m_Funcs[z].m_FuncDefPol;
				if(IsIn.find(";"+DefPol+";")!=std::string::npos) continue;
				DefPols.insert(DefPols.end(), DefPol); IsIn+=";"+DefPol+";";
			}
			//read policies where functions are defined
			for(z=0; z<DefPols.size(); ++z)
			{
				for(size_t a=0; a<m_Systems.size(); ++a)
				{
					if(!m_Systems[a].m_AOSystem || m_Systems[a].m_AOSystem->m_AddOn->m_ControlName!=m_AddOns[x].m_ControlName  || m_Systems[a].m_AOSystem->m_Name!=m_AddOns[x].m_Systems[y].m_Name) continue;
					CEMPolicy pol; pol.m_Name=DefPols[z]; pol.m_Switch=2; //toggle!!!
					pol.m_Id=CEMUtilities::GenId(m_AddOns[x].m_ControlName, "", 0);
					m_Systems[a].m_Pols.insert(m_Systems[a].m_Pols.end(), pol);
					if(!TXT_ReadAOPolicyParam(DefPols[z], &(m_AddOns[x].m_Systems[y]), &(m_Systems[a]), (unsigned int)(m_Systems[a].m_Pols.size()-1))) return 0;
				}
			}
			//move functions from switched-off (toggle) definition policies to where they belong
			for(z=0; z<m_AddOns[x].m_Systems[y].m_Funcs.size(); ++z)
			{
				std::string DefPol = m_AddOns[x].m_Systems[y].m_Funcs[z].m_FuncDefPol;
				std::string FuncId = m_AddOns[x].m_Systems[y].m_Funcs[z].m_Ident;
				std::string InsFuncId = m_AddOns[x].m_Systems[y].m_Funcs[z].m_Pos;
				bool Before = m_AddOns[x].m_Systems[y].m_Funcs[z].m_Before;
				for(size_t a=0, b, c; a<m_Systems.size(); ++a)
				{
					if(!m_Systems[a].m_AOSystem || m_Systems[a].m_AOSystem->m_AddOn->m_ControlName!=m_AddOns[x].m_ControlName  || m_Systems[a].m_AOSystem->m_Name!=m_AddOns[x].m_Systems[y].m_Name) continue;
					for(b=0; b<m_Systems[a].m_Modules.size(); ++b) if(InsFuncId==m_Systems[a].m_Modules[b]->m_Id) break;
					if(b>=m_Systems[a].m_Modules.size()) { if(!CEMError::CritErr("Unknown function identifier.", m_AddOns[x].m_ControlName+", line "+CEMUtilities::IntToStr(m_AddOns[x].m_Systems[y].m_Funcs[z].m_Row), m_AddOns[x].m_Systems[y].m_Name, "", "", "insert_before_func/insert_after_func", InsFuncId)) return 0; break; }
					if(!Before) ++b;
					for(c=0; c<m_Systems[a].m_Modules.size(); ++c) if(FuncId==m_Systems[a].m_Modules[c]->m_Id) break;
					if(c>=m_Systems[a].m_Modules.size()) { if(!CEMError::CritErr("Unknown function identifier.", m_AddOns[x].m_ControlName+", line "+CEMUtilities::IntToStr(m_AddOns[x].m_Systems[y].m_Funcs[z].m_Row), m_AddOns[x].m_Systems[y].m_Name, "", "", "id_func", FuncId)) return 0; break; }
					m_Systems[a].m_Modules.insert(m_Systems[a].m_Modules.begin()+b, m_Systems[a].m_Modules[c]);
					if(m_Systems[a].m_Modules[b]->m_ParamAdmin.GetSwitchParam()->m_numValue==2)
					{
						m_Systems[a].m_Modules[b]->m_ParamAdmin.GetSwitchParam()->m_strValue="on";
						m_Systems[a].m_Modules[b]->m_ParamAdmin.GetSwitchParam()->m_numValue=1;
					}
					if(c>b) ++c; //very likely
					m_Systems[a].m_Modules.erase(m_Systems[a].m_Modules.begin()+c);
				}
			}
		}
		//read add-on parameters and fit in respective functions
		for(y=0; y<m_AddOns[x].m_Systems.size(); ++y)
		{	
			for(z=0; z<m_AddOns[x].m_Systems[y].m_Pars.size(); ++z)
			{
				std::string FuncId = m_AddOns[x].m_Systems[y].m_Pars[z].m_Ident;
				for(size_t a=0, b, p; a<m_Systems.size(); ++a)
				{
					if(!m_Systems[a].m_AOSystem || m_Systems[a].m_AOSystem->m_AddOn->m_ControlName!=m_AddOns[x].m_ControlName  || m_Systems[a].m_AOSystem->m_Name!=m_AddOns[x].m_Systems[y].m_Name) continue;
					for(b=0; b<m_Systems[a].m_Modules.size(); ++b) if(FuncId==m_Systems[a].m_Modules[b]->m_Id) break;
				
					//intermediate solution to enable mtr-calculations with xml-input:
					//default-output-function is the only referenced function in mtr-calculations
					if(b>=m_Systems[a].m_Modules.size() && !m_InputTXT)
						for(b=0; b<m_Systems[a].m_Modules.size(); ++b)
							if(m_Systems[a].m_Modules[b]->m_PolName.find("_std")!=std::string::npos &&
								m_Systems[a].m_Modules[b]->m_PolName.find("_std_hh")==std::string::npos &&
								m_Systems[a].m_Modules[b]->m_Name.find("defoutput")!=std::string::npos)
								break;
					if(b>=m_Systems[a].m_Modules.size()) { if(!CEMError::CritErr("Unknown function identifier.", m_AddOns[x].m_ControlName+", line "+CEMUtilities::IntToStr(m_AddOns[x].m_Systems[y].m_Pars[z].m_Row), m_AddOns[x].m_Systems[y].m_Name, "", "", "insert_func", FuncId)) return 0; break; }
					
					std::map<std::string, int>PlaceHolder; PlaceHolder.clear();
					for(p=0; p<m_AddOns[x].m_Systems[y].m_Pars[z].m_ParNames.size(); ++p)
					{
						std::string ParName=m_AddOns[x].m_Systems[y].m_Pars[z].m_ParNames[p];
						CEMUtilities::ReplaceXY(ParName, m_Systems[a].m_Modules[b], PlaceHolder);
						std::string ParVal=m_AddOns[x].m_Systems[y].m_Pars[z].m_ParVals[p];
						std::string ParId=m_AddOns[x].m_Systems[y].m_Pars[z].m_ParIds[p];
						int Line=m_AddOns[x].m_Systems[y].m_Pars[z].m_Row;
						int Column=m_AddOns[x].m_Systems[y].m_Col;
						if(!m_Systems[a].m_Modules[b]->TakeParam(ParName, ParVal, ParId, Line, Column)) return 0;
					}
				}
			}
		}
	}
	return 1;
}

bool CEMControl::TXT_ReadAOPolicyParam(std::string PolName, AddOnSys *AOSys, CEMSystem *Sys, unsigned int pnum, int fInd)
{
	std::string line;
	std::vector<std::string> tokline;
	std::string PolPath = CEMUtilities::AppendExtension(m_AOParamPath + PolName);

	std::ifstream ifs(PolPath.c_str());
	if(ifs.is_open())
	{
		unsigned int lnum=0, headline=1;
		while(!ifs.eof())
		{
			std::getline(ifs,line);
			++lnum;
			tokline = CEMUtilities::TokeniseLine(line, m_Country);
			if(lnum==1) { if(!CheckParSheetFormat(tokline, PolName, AOSys->m_AddOn->m_FirstSystemCol, AOSys->m_AddOn->m_EndSystemCol, AOSys->m_AddOn->m_ParamNameCol, AOSys->m_AddOn->m_ParamIdCol, AOSys->m_Col, AOSys->m_Name)) break; }
			if(tokline.size()-1<m_ParamNameCol)
			{
				if(line=="") { if(!CEMError::CritErr("Emtpy line.", PolName+"Line "+CEMUtilities::IntToStr(lnum)+".", "", PolName)) return 0; break; }
				else if(m_InputTXT) { if(!CEMError::CritErr("Line too short.", PolName+" line "+CEMUtilities::IntToStr(lnum)+".", "", PolName, "", "", line)) return 0; continue; }
			}
			if(headline && !tokline[AOSys->m_AddOn->m_ParamNameCol].compare(LAB_FIRSTPAR)) { headline=0; continue; }
			if(headline) continue;
			if(!tokline[AOSys->m_AddOn->m_ParamNameCol].compare(LAB_ENDPAR)) break;
			std::string trimline = CEMUtilities::Trim(line, 1);
			if(trimline=="") { if (CEMError::CritErr("Empty line. Check whether label 'sys_end_par' is missing, misspelled or misplaced.", PolName+", line "+CEMUtilities::IntToStr(lnum))) return 0; break; }
			std::string val="";
			if(tokline.size()>AOSys->m_Col) val=tokline[AOSys->m_Col];
			if(!Sys->TakePolParam(pnum, tokline[AOSys->m_AddOn->m_ParamNameCol], val, tokline[AOSys->m_AddOn->m_ParamIdCol], lnum, "", &fInd)) return 0;
		}
	}
	else return CEMError::CritErr("Policy file not found at '"+m_AOParamPath+"'.", "", "", PolName);
	return 1;
}

CEMTable *CEMControl::GetInputTable(std::string path, std::string file, bool useIfExists, std::string systemName, int viRowMergeVar, int ignoreNRows,
		int ignoreNCols, std::string decSep, std::string repByEMPath, double defaultValue, bool doRanges, bool lookUpMode)
{
	//compose the full path- and filename of the input file
#ifdef _WIN32
	if (!m_EMContentPath.empty()) //path to EM file structure is now passed by UI ...
		path=CEMUtilities::Replace(path, repByEMPath, m_EMContentPath);
	else //... thus assessing storage place of executable is not necessary anymore
		path=CEMUtilities::Replace(path, repByEMPath, GetEMPath()); //EMPath is assessed via the location of the executable, using a function only available under Windows
	path=CEMUtilities::Replace(path, "/", "\\"); path=CEMUtilities::Replace(path, "\\\\", "\\"); //cleanup
#endif
	std::string fullFileName = CEMUtilities::CheckPathName(path) + file;

	//use existing (kept in memory) table if the file was already read in and the respective option (for keeping in memory) set
	if(useIfExists)
		for(std::vector<CEMTable*>::iterator itT=m_InputTables.begin(); itT!=m_InputTables.end(); itT++)
			if((*itT)!=NULL && //table-pointer is set to NULL if table is released at run-time (see ReleaseInputTable)
				(*itT)->m_ID.length()>fullFileName.length() && //table-ID is 'FullFileName#SystemName' (see below)
				(*itT)->m_ID.substr(0, fullFileName.length()+1)==fullFileName+"#")
				return *itT;
	
	//table does not yet exist, i.e. input file must be read and analysed
	CEMTable *table = NULL;

	//assess if the input file uses a different decimal sign than the local system
	bool needConvertDecSep=0;
	setlocale (LC_NUMERIC,"");
	struct lconv *lc=localeconv();
	std::string locDecSep=""; locDecSep+=*(lc->decimal_point);
	if(locDecSep!=decSep)
		needConvertDecSep=1;

	//open and read the input file
	std::ifstream fileStream(fullFileName.c_str());
	if(!fileStream.is_open())
	{
		CEMError::CritErr("Input data file could not be opened.", fullFileName);
		return NULL;
	}

	std::string line = ""; int lnum = 0; bool result = 0;
	while(!fileStream.eof())
	{
		std::getline(fileStream,line);
		++lnum;
		if(lnum <= ignoreNRows)
			continue; //ignore header rows if necessary (i.e. respective option set)
		
		line = CEMUtilities::Trim(line);
		if(line.empty())
			continue; //ignore empty lines

		if(needConvertDecSep) //replace decimal sign if necessary
			line=CEMUtilities::Replace(line, decSep, locDecSep);

		//analyse header row
		if(lnum==ignoreNRows+1)
		{
			table = new CEMTable();
			//table gets an ID consisting of full file name (including path) and system name, separated by # (for reassessment if appropriate, see above)
			//the system name is necessary, as different systems may use the same file (name) but re-read should be enforced
			//reasons for re-read: the content of the file has changed since the first read; memory should be freed between the reads; (anything else?)
			table->Init(fullFileName+"#"+systemName, defaultValue, doRanges, ignoreNCols, this);

			if(lookUpMode)
				result = table->GenerateColIndex(line); //in lookup mode we need a column- and a row-index (row-index is generated with AddRow)
			else
				result = table->TakeHeaderRow(line, viRowMergeVar); //assess which variables are to be filled by analysing the header row of the input file
		}
		else //data row (i.e. not header row)
			result = table->AddRow(line, lnum);
		
		if(!result)
		{
			CEMError::CritErr(table->GetError(), fullFileName);
			delete table; return NULL;
		}
	}
	
	fileStream.close();

	m_InputTables.push_back(table); //store pointer to table to allow for reuse
	return table;
}

void CEMControl::ReleaseInputTable(CEMTable *table)
{
	for(std::vector<CEMTable*>::iterator itT=m_InputTables.begin(); itT!=m_InputTables.end(); itT++)
	{
		if(*itT!=NULL && table->m_ID==(*itT)->m_ID)
		{
			delete *itT;
			*itT = NULL; //do not delete vector entry (just because it seems easier), but take account of NULL-pointers when analysing the vector
			break;
		}
	}
	//remark: tables which are not released at run-time are released in the destructor (see above)
}

std::string CEMControl::AdaptSwitchIfSwitchablePolicy(std::string SystemID, std::string PolicyName, std::string CurrentSwitch)
{
	for (std::vector<std::vector<std::string> >::iterator it = m_PolicySwitches.begin(); it != m_PolicySwitches.end(); ++it)	// please retain the space in "> >" to avoid compiler errors in Linux
	{
		std::vector<std::string> PolicySwitchElements = *it; //policy switch with 3 elements: 1) pattern, e.g. yem_??, 2) system-id, 3) value (on or off)
		if (PolicySwitchElements[1] != SystemID)
			continue;

		if (CEMUtilities::DoesValueMatchPattern(PolicySwitchElements[0], PolicyName))
			return PolicySwitchElements[2]; //if policy corresponds to the pattern (e.g. yem_es corresponds to yem_??) return the redefined switch-value
	}
	return CurrentSwitch; //switch not redefined
}

std::string CEMControl::GetSystemPolicySwitchesForHeader(std::string SystemID)
{
	std::string SystemPolicySwitches = "";
	for (std::vector<std::vector<std::string> >::iterator it = m_PolicySwitches.begin(); it != m_PolicySwitches.end(); ++it) // please retain the space in "> >" to avoid compiler errors in Linux
	{
		std::vector<std::string> PolicySwitchElements = *it; //policy switch with 3 elements: 1) pattern, e.g. yem_??, 2) system-id, 3) value (on or off)
		if (PolicySwitchElements[1] != SystemID)
			continue;
		if (!SystemPolicySwitches.empty())
			SystemPolicySwitches += ";";
		SystemPolicySwitches += PolicySwitchElements[0] + "=" + PolicySwitchElements[2];
	}
	return SystemPolicySwitches.empty() ? "none" : SystemPolicySwitches;
}

void CEMControl::TakeConfigParam(std::string projectPath, std::string systemName, std::string dataName, std::string dataPath, std::string outputPath)
{
	m_CommandLineConfig = 1;
	m_EMContentPath = CEMUtilities::CheckPathName(projectPath);
	size_t us = systemName.find("_");
	std::string country = us > 0 ?  systemName.substr(0, us) : "??";
	m_ParamPath = m_EMContentPath + "XMLParam\\Countries\\" + country + "\\";
	if (outputPath.empty()) outputPath = m_EMContentPath + "Output\\";
	m_OutputPath = CEMUtilities::CheckPathName(outputPath);
	time_t t1 = time(0); struct tm t2; localtime_s(&t2, &t1);
	char now[100]; EM_SPRINTF(now, "%4d%2d%2d%2d%2d", t2.tm_year + 1900, t2.tm_mon + 1, t2.tm_mday, t2.tm_hour, t2.tm_min);
	m_HeaderDate = CEMUtilities::Replace(now, " ", "0");;
	m_ErrLogFile = m_OutputPath + m_HeaderDate + "_errorlog.txt";
	m_LogWarnings = 1;
	m_EMVersion = "unknown";
	m_UIVersion = "unknown";
	m_ConfigPath = m_EMContentPath + "XMLParam\\Config\\";
	m_DataSet = CEMUtilities::AppendExtension(dataName);
	if (dataPath.empty()) dataPath = m_EMContentPath + "Input\\";
	m_DataPath = CEMUtilities::CheckPathName(dataPath);
	m_LastRun = 1;
	m_ControlName = country + ".xml";
	m_DataConfig = country + "_DataConfig.xml";
	CEMSystem EMSystem(this); EMSystem.m_Name = systemName; EMSystem.m_SpineOn = 1; m_Systems.insert(m_Systems.end(), EMSystem);

	CEMError::m_Control = this;
	CEMUtilities::CheckDecSign(".", m_ConfigName, 1);
}

bool CEMControl::ReadGlobalConfig()
{
	ReadHICPConfig();
	ReadExchangeRatesConfig();
	return 1; // do not if reading these files fails (hoping things may still work without them ...)
}

bool CEMControl::ReadHICPConfig()
{
	int iHicp; if (!GetVarIndex("$hicp", iHicp)) return 1; // note: $HICP is generated in ReadData function (to be available in CheckParam for sure), thus the if is just "formal" 

	std::string file = m_ConfigPath + "HICPConfig.xml";
	CEMXML xml, *pXmlHICP;
	if (!xml.LoadFile(file)) return 1; // do nothing, because most likely HICPConfig does not yet exist

	pXmlHICP = xml.GetNode("HICPConfig");
	if (!pXmlHICP) return CEMError::CodeErr("Failed to find node 'HICPConfig' in input file '" + file + "'.", "Check if the XML-code is correct.");

	for (SystemContainer_t::iterator itSystem = m_Systems.begin(); itSystem != m_Systems.end(); itSystem++)
	{
		if (itSystem->m_Households.size() <= 0 || itSystem->m_Households[0].m_Persons.size() <= 0) continue; // just formal check
		double countryHICP = itSystem->m_Households[0].m_Persons[0][iHicp]; // for perhaps not overwritting (see below)

		std::string systemYearStr = itSystem->m_Year != "" ? itSystem->m_Year : CEMUtilities::ExtractSystemYear(itSystem->m_Name, 1);
		double indexData = -1, indexSystem = -1;
		if (!systemYearStr.empty())
		{
			double systemYear = CEMUtilities::StrToDouble(systemYearStr);
			for (size_t i = 0; i < pXmlHICP->m_Ch.size(); ++i)
			{
				if (CEMUtilities::LCase(pXmlHICP->m_Ch[i]->m_Name) != "hicp") continue; // ignore other children
				if (CEMUtilities::LCase(pXmlHICP->m_Ch[i]->GetAtrVal("country")) != CEMUtilities::LCase(m_Country)) continue; // not the current country
				double year = CEMUtilities::StrToDouble(pXmlHICP->m_Ch[i]->GetAtrVal("year"));
				double val = CEMUtilities::StrToDouble(pXmlHICP->m_Ch[i]->GetAtrVal("value"));
				if (m_DataIncomeYear == year) indexData = val; // index for data income year found
				if (systemYear == year) indexSystem = val;	   // index for system year found
			}
		}
		double globalHICP = indexData > 0 && indexSystem >= 0 ? indexSystem / indexData : -1;
		if (abs(globalHICP - countryHICP) < 0.000001) continue; // no need for overwritting if equal (actually a very likely case)

		// if HICP cannot be calculated with values in global table: try to use HICP defined in country uprating-factors-policy, if not existent set 1
		if (globalHICP == -1) { if (countryHICP != EMVOID) continue; else globalHICP = 1; }

		for (size_t h = 0; h < itSystem->m_Households.size(); ++h) //loop over households (within system)
		for (size_t p = 0; p < itSystem->m_Households[h].m_Persons.size(); ++p) //loop over persons within household
			itSystem->m_Households[h].m_Persons[p][iHicp] = globalHICP;
	}
	return 1;
}

bool CEMControl::ReadExchangeRatesConfig()
{
	std::string file = m_ConfigPath + "ExchangeRatesConfig.xml";
	CEMXML xml, *pXmlEXR;
	if (!xml.LoadFile(file)) return 1; // do nothing, because most likely ExchangeRatesConfig does not yet exist

	pXmlEXR = xml.GetNode("ExchangeRatesConfig");
	if (!pXmlEXR) return CEMError::CodeErr("Failed to find node 'ExchangeRatesConfig' in input file '" + file + "'.", "Check if the XML-code is correct.");

	for (SystemContainer_t::iterator itSystem = m_Systems.begin(); itSystem != m_Systems.end(); itSystem++)
	{
		if (!itSystem->m_ParamCurrencyEuro && !m_ExchangeRateDate.empty()) itSystem->m_OutputCurrencyEuro = 1;
		for (size_t e = 0; e < pXmlEXR->m_Ch.size(); ++e) // check if the global exchange rates table contains a rate for the system
		{
			if (CEMUtilities::LCase(pXmlEXR->m_Ch[e]->m_Name) != "exchangerates") continue; // ignore other children
			if (CEMUtilities::LCase(pXmlEXR->m_Ch[e]->GetAtrVal("country")) != CEMUtilities::LCase(m_Country)) continue; // not the current country
			std::string validFor = CEMUtilities::LCase(pXmlEXR->m_Ch[e]->GetAtrVal("validfor"));
			size_t f = validFor.find(CEMUtilities::LCase(itSystem->m_Name)); if (f == std::string::npos) continue;
			if (!((f == 0 || validFor[f-1] == ' ') &&
				(f + itSystem->m_Name.length() == validFor.length() || validFor[f + itSystem->m_Name.length()] == ','))) continue;
			double rate = -1;
			std::string relevantRate = CEMUtilities::LCase(pXmlEXR->m_Ch[e]->GetAtrVal("default"));
			if (!m_ExchangeRateDate.empty() && m_ExchangeRateDate != "default") relevantRate = CEMUtilities::LCase(m_ExchangeRateDate);
			if (relevantRate == "june 30") rate = CEMUtilities::StrToDouble(pXmlEXR->m_Ch[e]->GetAtrVal("june30"));
			else if (relevantRate == "year average") rate = CEMUtilities::StrToDouble(pXmlEXR->m_Ch[e]->GetAtrVal("yearaverage"));
			else if (relevantRate == "first semester") rate = CEMUtilities::StrToDouble(pXmlEXR->m_Ch[e]->GetAtrVal("firstsemester"));
			else if (relevantRate == "second semester") rate = CEMUtilities::StrToDouble(pXmlEXR->m_Ch[e]->GetAtrVal("secondsemester"));
			if (rate != -1) itSystem->m_ExchRate = rate;
		}
	}
	return 1;
}