#include <fstream>
#include <algorithm>
#include "EMParam.h"
#include "EMControl.h"
#include "EMSystem.h"
#include "EMUtilities.h"

/********************************************************************************************
 functions class CEMParam and derived classes
********************************************************************************************/
CEMParam::CEMParam()
{
	m_Compulsory = 0;
	m_Module = NULL;
}

// default destructor is assumed in windows, but required in Linux
CEMParam::~CEMParam()
{
}

bool CEMParam::Prepare()
{
	if (m_Compulsory && m_strValue=="n/a") return CEMError::CritErr("Compulsory parameter set to n/a.", this);
	return 1;
}

bool CEMParam::Assess(double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, double DefVal)
{
	TU; HHVar; PersIndex; DefVal; //just to avoid warning about unreferenced formal parameter
	val=m_numValue; return 1;
}

bool CEMSwitchParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	return CEMUtilities::CheckOnOff(m_numValue, this);
}

bool CEMVarParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	if(!m_Control->GetVarIndex(m_strValue, m_VarIndex, CREATE_PARAM))
		return CEMError::CritErr("Unknown variable '"+m_strValue+"'.", this);
	return 1;
}

bool CEMVarParam::Assess(double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, double DefVal)
{
	val = TU->GetVarVal(m_VarIndex, HHVar, PersIndex);
	if(val==EMVOID)
	{
		char handling[500];
		EM_SPRINTF(handling, "%.2f is used as default.", DefVal);
		if(!CEMError::NonCritErr("Use of not yet calculated variable '"+m_strValue+"'.", this, handling)) return 0;
		val = 0;
	}
	return 1;
}

bool CEMValParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	bool err;
	if(!CEMUtilities::ExtractPeriod(m_strValue, m_Period, 1)) return CEMError::CritErr("Parameter is not a valid number.", this);
	bool pc=(m_strValue.substr(m_strValue.length()-1,1)=="%");
	if(pc) m_strValue=m_strValue.substr(0, m_strValue.length()-1);
	m_numValue = CEMUtilities::StrToDouble(m_strValue, &err);
	
	if (err)
	{
		//to allow for constants to be used with value parameters (constants are initialised in CheckParam already)
		int vi;
		if(!m_Control->GetVarIndex(m_strValue, vi) || !m_Control->m_VarIndexV[vi].m_IsGlobal)
			return CEMError::CritErr("Parameter is not a valid number.", this); //neither an amount nor a constant

		CEMTaxunit *staticTU = m_System->GetStaticTU();
		PersonVarContainer_t *staticHHVar = &(staticTU->m_HH->m_Persons);
		m_numValue = staticTU->GetVarVal(vi, staticHHVar, 0);
		if (m_numValue == EMVOID)
			if (!CEMError::NonCritErr("Usage of not initialised constant '" + m_Control->m_VarIndexV[vi].m_Name + ".", this))
				return 0;
		return 1;
	}

	if(pc) m_numValue/=100;
	return CEMUtilities::ConvertMonthly(m_numValue, m_Period.at(0), this);
}

bool CEMYesNoParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	return CEMUtilities::CheckYesNo(m_numValue, this);
}

bool CEMILParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	if(!m_System->IsILDefined(m_strValue)) return CEMError::CritErr("Incomelist '"+m_strValue+"' not defined '.", this);
	return 1;
}

bool CEMILParam::Assess(double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, double DefVal)
{
	std::string VoidVar="";
	val = TU->GetILVal(m_System->m_ILs[m_strValue], HHVar, PersIndex, &VoidVar);
	if(!VoidVar.empty())
	{
		char handling[500];
		if(DefVal) EM_SPRINTF(handling, "%.2f is used as default value for the incomelist.", DefVal);
		else EM_SPRINTF(handling, "Zero is used as default value for the not defined variables.");
		if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+m_strValue+"'.", this, handling)) return 0;
	}
	return 1;
}

bool CEMVarILParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	switch(m_viType)
	{
	case VITYPE_UNDEF:
	case VITYPE_VAR:
		if(m_Control->GetVarIndex(m_strValue, m_VarIndex, CREATE_PARAM)) { m_viType=VITYPE_VAR; return 1; }
		if(m_viType!=VITYPE_UNDEF) return CEMError::CritErr("Unknown variable '"+m_strValue+"'.", this);
	case VITYPE_IL:
		if(m_System->IsILDefined(m_strValue)) { m_viType=VITYPE_IL; return 1; }
		return CEMError::CritErr("Variable or incomelist '"+m_strValue+"' not defined.", this);
	}
	return 1;
}

bool CEMVarILParam::Assess(double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, std::string *VoidEntry)
{
	std::string err, VoidVar="", Handling="";
	switch(m_viType)
	{
	case VITYPE_VAR:
		val = TU->GetVarVal(m_VarIndex, HHVar, PersIndex);
		if(val!=EMVOID) return 1;
		if(VoidEntry) { (*VoidEntry)+="'"+m_strValue+"' "; return 0; }
		err = "Use of not yet calculated variable '";
		Handling = "Zero is used as default";
		break;
	case VITYPE_IL:
		if(!VoidEntry) VoidEntry=&VoidVar;
		val = TU->GetILVal(m_System->m_ILs[m_strValue], HHVar, PersIndex, VoidEntry);
		if(VoidEntry->empty()) return 1;
		if(VoidEntry!=&VoidVar) return 1; //returning 1 though there is an 'error': don't prevent adding up whatever is available (error is catched by not empty VoidEntry)
		err = "Use of not yet calculated variable '"+VoidVar+"' in incomelist '";
		Handling = "Incomelist is set to zero.";
		break;
	}
	val = 0;
	return CEMError::NonCritErr(err+m_strValue+"'.", this, Handling);
}

bool CEMTUParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;
	m_TUDefMod = m_System->GetTUDefModule(m_strValue);
	if(!m_TUDefMod) return CEMError::CritErr("Unknown assessment unit '"+m_strValue+"'.", this);
	return 1;
}

/********************************************************************************************
 functions class CEMFormulaParam and helper classes
********************************************************************************************/
bool CEMFormOp::Prepare(CEMModule *Module, std::string strValue, int FootnoteNo, char Period)
{
	m_strValue = strValue;
	m_FootnoteNo = FootnoteNo;
	std::string DefType="";
	m_LowLimPar=m_UpLimPar=NULL;
	m_AltTU=NULL;
	m_IsStatic=0;
	m_IsGlobal=0;
	m_numValue=EMVOID;

	if(m_FootnoteNo==-1) m_sFootnote="#?_";
	else
	{
		char sFN[50];
		EM_SPRINTF(sFN, "#%d_", m_FootnoteNo);
		m_sFootnote=sFN;
		if(Module->m_ParamAdmin.IsParamDefined(m_sFootnote+"type")) DefType=Module->m_ParamAdmin.GetGeneralParam(sFN)->m_strValue;
		if(Module->m_ParamAdmin.IsParamDefined(m_sFootnote+"lowlim")) m_LowLimPar=Module->m_ParamAdmin.GetFormulaParam(m_sFootnote+"lowlim");
		if(Module->m_ParamAdmin.IsParamDefined(m_sFootnote+"uplim")) m_UpLimPar=Module->m_ParamAdmin.GetFormulaParam(m_sFootnote+"uplim");
		if(Module->m_ParamAdmin.IsParamDefined(m_sFootnote+"level")) m_AltTU=Module->m_ParamAdmin.GetTUParam(m_sFootnote+"level");
	}
	m_OthMember=0;
	if(m_strValue.find("head:")!=std::string::npos) { m_strValue=CEMUtilities::Replace(m_strValue, "head:",""); m_OthMember=1; }
	if(m_strValue.find("partner:")!=std::string::npos) { m_strValue=CEMUtilities::Replace(m_strValue, "partner:",""); m_OthMember=2; }
	if(!DefType.compare("var"))
	{
		if(!Module->m_Control->GetVarIndex(m_strValue, m_VarIndex, CREATE_PARAM)) return CEMError::CritErr("Variable '"+m_strValue+"' does not exist.", Module);
		m_Type=OPTYPE_VAR;
		m_IsGlobal=Module->m_Control->m_VarIndexV[m_VarIndex].m_IsGlobal;
		return 1;
	}
	if(!DefType.compare("il"))
	{
		if(!Module->m_System->IsILDefined(m_strValue)) return CEMError::CritErr("Incomelist '"+m_strValue+"' is not defined.", Module);
		m_Type=OPTYPE_IL;
		return 1;
	}
	if(!DefType.compare("query")) 
	{
		m_IsStatic=m_Function.m_IsStatic;
		if(!m_Function.IsFuncDefined(m_strValue)) return CEMError::CritErr("Query '"+m_strValue+"' does not exist.", Module);
		if(!m_Function.Prepare(Module, m_strValue, m_FootnoteNo)) return 0;
		m_IsGlobal = m_Function.m_IsGlobal;
		return 1;
	}

	bool err;
	double nV=CEMUtilities::StrToDouble(m_strValue, &err);
	if(!err)
	{
		m_numValue=nV;
		CEMUtilities::ConvertMonthly(m_numValue, Period, m_FormulaParam);
		m_Type=OPTYPE_AMOUNT;
		m_IsGlobal=m_IsStatic=1;
		return 1;
	}
	if(!strValue.compare("amount"))
	{
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+"amount", PTYPE_VAL, Module, "0")) return 0;
		CEMValParam *amtParam = Module->m_ParamAdmin.GetValParam(m_sFootnote+"amount");
		if (amtParam->m_strValue == "n/a")
			return CEMError::CritErr("Parameter '"+m_sFootnote+"amount"+"' set to n/a though used.", Module);
		m_numValue = amtParam->m_numValue;
		m_Type=OPTYPE_AMOUNT;
		m_IsGlobal=m_IsStatic=1;
		return 1;
	}
	if(!strValue.compare("rand"))
	{
		m_Type=OPTYPE_RAND;
		m_IsGlobal=m_IsStatic=0;
		return 1;
	}
	if(Module->m_Control->GetVarIndex(m_strValue, m_VarIndex, CREATE_PARAM))
	{
		m_Type=OPTYPE_VAR;
		m_IsGlobal=Module->m_Control->m_VarIndexV[m_VarIndex].m_IsGlobal;
		return 1;
	}
	if(Module->m_System->IsILDefined(m_strValue)) { m_Type=OPTYPE_IL; return 1; }
	if(m_Function.IsFuncDefined(m_strValue))
	{
		m_Type=OPTYPE_FUNC;
		if(!m_Function.Prepare(Module, m_strValue, m_FootnoteNo)) return 0;
		m_IsGlobal = m_Function.m_IsGlobal;
		m_IsStatic = m_Function.m_IsStatic;
		if (m_IsStatic) m_numValue = m_Function.m_numValue;
		return 1;
	}
	return CEMError::CritErr("Unrecognised operand '"+m_strValue+"'. (Note: if the operand is supposed to be a variable, consider declaring in the variables file.)", Module);
}

bool CEMFormOp::Assess(CEMModule *Module, CEMTaxunit *TU, PersonVarContainer_t *HHVar)
{
	if(m_IsStatic) return 1;

	CEMTaxunit *ContTU = NULL;
	int bkupHead = -1;
	if(m_AltTU && m_AltTU->m_strValue!="n/a")
	{
		if(!TU->m_HH->AreTUsBuilt(m_AltTU->m_strValue))
			if(!m_AltTU->m_TUDefMod->BuildTUs(TU->m_HH, HHVar)) return 0;
		if(m_FormulaParam->m_Name=="CEMCondParam using CEMFormulaParam")
		{//in principle only taxunits containing the main taxunit make sense and are allowed
		 //however for condition parameters the main taxunit is not relevant, as the assessment unit is in principle always the individual
			ContTU=TU->m_HH->GetContainingTU(TU->m_HeadIndex, m_AltTU->m_strValue);
			if(!ContTU) return CEMError::CodeErr("Programming error: using '"+m_AltTU->m_strValue+"' as alternative level went wrong.");
			bkupHead=ContTU->m_HeadIndex;			//condition parameter assess conditions for each person in turn, with this person set to head
			ContTU->m_HeadIndex=TU->m_HeadIndex;	//this needs to be reflected with an alternative TU
		}
		else
		{
			ContTU=TU->m_HH->GetContainingTU(TU, m_AltTU->m_strValue);
			if(!ContTU)
			{
				if(!CEMError::NonCritErr("Assessment unit cannot be used as alternative level. (Only assessment units containing the function's main assessment unit are allowed (e.g. maintu=family/alttu=hh is ok as family is part of hh; maintu=hh/alttu=family not ok as hh is not part of family.)", Module, "#x_level", m_AltTU->m_strValue, "Alternative level is ignored.")) return 0;
				ContTU=TU;
			}
		}
		TU=ContTU;
	}

	int pIndex=-1; std::string VoidVar="";
	switch(m_Type)
	{
	case OPTYPE_AMOUNT:
		break;
	case OPTYPE_RAND:
		m_numValue = double(rand())/double(RAND_MAX);
		break;
	case OPTYPE_VAR:
		if(m_OthMember)
		{
			for(PersonRefContainer_t::iterator it=TU->m_PersonRefs.begin(); it!=TU->m_PersonRefs.end(); ++it)
			{
				if(m_OthMember==1 && it->m_IsHead) { pIndex=it->m_Index; break; }
				if(m_OthMember==2 && it->m_IsPartner) { pIndex=it->m_Index; break; }
			}
			if(m_OthMember==1 && pIndex==-1) return CEMError::CritErr("Trying to assess variable '"+m_strValue+"' for head of assessment unit ('head:'), though head is not yet determined.", Module);
		}
		m_numValue = TU->GetVarVal(m_VarIndex, HHVar, pIndex);
		if(m_numValue==EMVOID)
		{
			if(!CEMError::NonCritErr("Use of not yet calculated variable '"+m_strValue+"'.", Module, "", "", "Zero is used as default.")) return 0;
			m_numValue = 0;
		}
		break;
	case OPTYPE_IL:
		m_numValue = TU->GetILVal(Module->m_System->m_ILs[m_strValue], HHVar, -1, &VoidVar);
		if(!VoidVar.empty())
			{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+m_strValue+"'.", Module, "", "", "Zero is used as default value for the not defined variables.")) return 0; }
		break;
	case OPTYPE_FUNC:
		if(!m_Function.Assess(Module, TU, HHVar)) return 0;
		m_numValue = m_Function.m_numValue;
		break;
	}
	if(ContTU && bkupHead!=-1) ContTU->m_HeadIndex=bkupHead;

	return Module->ApplyLimParam(m_numValue, m_LowLimPar, m_UpLimPar);
}

bool CEMFunction::TakeParam(std::string ParName, std::string ParVal, int &ParType, int &Compulsory)
{
	//parameters for isntomchild
	int len=int(ParName.length());
	if(!ParName.substr(0,1).compare("#") &&
		!ParName.substr(std::max(len-2,0)).compare("_n")) ParType=PTYPE_VAL;
	else if(!ParName.substr(0,1).compare("#") &&
		!ParName.substr(std::max(len-2,0)).compare("_m")) ParType=PTYPE_VAL;
	//parameters for hasmaxvalintu (old name: isrichestintu) and hasminvalintu 
	else if(!ParName.substr(0,1).compare("#") &&
		(!ParName.substr(std::max(len-10,0)).compare("_means_var") 
		|| !ParName.substr(std::max(len-9,0)).compare("_means_il")
		|| !ParName.substr(std::max(len-6,0)).compare("_means")
		|| !ParName.substr(std::max(len-4,0)).compare("_val")))
		ParType=PTYPE_VARIL;
	else if(!ParName.substr(0,1).compare("#") &&
		!ParName.substr(std::max(len-7,0)).compare("_unique")) ParType=PTYPE_YESNO;
	else if(!ParName.substr(0,1).compare("#") &&
		!ParName.substr(std::max(len-12,0)).compare("_adults_only")) ParType=PTYPE_YESNO;
	//parameters for npersinunit/ndepchildrenintu/nadultsintu/nchofcouple/ndepchofcouple/nchofperson/ndepchofperson
	else if(!ParName.substr(0,1).compare("#") &&
		!ParName.substr(std::max(len-7,0)).compare("_agemin")) ParType=PTYPE_VAL;
	else if(!ParName.substr(0,1).compare("#") &&
		!ParName.substr(std::max(len-7,0)).compare("_agemax")) ParType=PTYPE_VAL;
	//parameter for getpartnerincome/getcoupleincome/getparentsincome/getmotherincome/getfatherincome
	else if(!ParName.substr(0,1).compare("#") &&
		(!ParName.substr(std::max(len-11,0)).compare("_income_var") 
		|| !ParName.substr(std::max(len-10,0)).compare("_income_il")
		|| !ParName.substr(std::max(len-7,0)).compare("_income")))
		ParType=PTYPE_VARIL;
	//parameter for getpartnerinfo/getmotherinfo/getfatherinfo
	else if(!ParName.substr(0,1).compare("#") &&
		(!ParName.substr(std::max(len-9,0)).compare("_info_var") 
		|| !ParName.substr(std::max(len-8,0)).compare("_info_il") //phase out, should not be a monetary information
		|| !ParName.substr(std::max(len-5,0)).compare("_info")))
		ParType=PTYPE_VARIL;
	//parameter for isuseddatabase
	else if(!ParName.substr(0,1).compare("#") && !ParName.substr(std::max(len-13,0)).compare("_databasename"))
		ParType=PTYPE_NAME;
	Compulsory = 0; //even if a function parameter is compulsory (e.g. IsNtoMChild #_m, #_n) the respective parameter may not be used in this system (e.g. #5_m used 2007 but set to n/a in 2006)
					//setting Compulsory = 1 results in an error message if the parameter is set to n/a (existence is checked below anyway)
	return (ParType!=-1);
}

bool CEMFunction::IsFuncDefined(std::string Name)
{
	if(!Name.compare("ismarried")) { m_IdentNo=0; return 1; }
	else if(!Name.compare("iscohabiting")) { m_IdentNo=1; return 1; }
	else if(!Name.compare("iswithpartner")) { m_IdentNo=2; return 1; }
	else if(!Name.compare("isloneparent") || !Name.compare("islonepar") || !Name.compare("issingleparent") || !Name.compare("issinglepar")) { m_IdentNo=3; return 1; }
	else if(!Name.compare("isloneparentofdepchild") || !Name.compare("issingleparentofdepchild")) { m_IdentNo=4; return 1; }
	else if(!Name.compare("isparent")) { m_IdentNo=5; return 1; }
	else if(!Name.compare("isparentofdepchild")) { m_IdentNo=6; return 1; }
	else if(!Name.compare("isdependentparent") || !Name.compare("isdepparent") || !Name.compare("isdeppar")) { m_IdentNo=7; return 1; }
	else if(!Name.compare("isntomchild")) { m_IdentNo=8; return 1; }
	else if(!Name.compare("nchildrenofperson") || !Name.compare("nchofperson")) { m_IdentNo=-9; return 1; } //query is phased out to be replaced by nch..ofcouple (because of misleading name)
	else if(!Name.compare("nchildrenofcouple") || !Name.compare("nchofcouple")) { m_IdentNo=9; return 1; }
	else if(!Name.compare("ndepchildrenofperson") || !Name.compare("ndepchofperson")) { m_IdentNo=-10; return 1; } //query is phased out to be replaced by ndepch..ofcouple (because of misleading name)
	else if(!Name.compare("ndepchildrenofcouple") || !Name.compare("ndepchofcouple")) { m_IdentNo=10; return 1; }
	else if(!Name.compare("isdependentchild") || !Name.compare("isdepchild")) { m_IdentNo=11; return 1; }
	else if(!Name.compare("isheadoftu") || !Name.compare("ishead")) { m_IdentNo=12; return 1; }
	else if(!Name.compare("ispartnerofheadoftu") || !Name.compare("ispartner")) { m_IdentNo=13; return 1; }
	else if(!Name.compare("isineducation")) { m_IdentNo=14; return 1; }
	else if(!Name.compare("isdisabled")) { m_IdentNo=15; return 1; }
	else if(!Name.compare("iscivilservant")) { m_IdentNo=19; return 1; }
	else if(!Name.compare("isbluecoll")) { m_IdentNo=20; return 1; }
	else if(!Name.compare("hasmaxvalintu") || !Name.compare("isrichestintu")/*old name*/) { m_IdentNo=21; return 1; }
	else if(!Name.compare("ndepchildrenintu") || !Name.compare("ndepchintu") ||
			!Name.compare("ndepchildrenintaxunit") || !Name.compare("ndepchintaxunit")) { m_IdentNo=22; return 1; }
	else if(!Name.compare("npersonsintu") || !Name.compare("npersonsintaxunit") || !Name.compare("npersintu") ||
			!Name.compare("nperstaxunit") || !Name.compare("npersinunit")) { m_IdentNo=23; return 1; }
	else if(!Name.compare("isownchild")) { m_IdentNo=24; return 1; }
	else if(!Name.compare("isowndependentchild") || !Name.compare("isowndepchild")) { m_IdentNo=25; return 1; }
	else if(!Name.compare("isdependentrelative") || !Name.compare("isdeprelative")  || !Name.compare("isdeprel")) { m_IdentNo=26; return 1; }
	else if(!Name.compare("isloosedependentchild") || !Name.compare("isloosedepchild")) { m_IdentNo=27; return 1; }
	else if(!Name.compare("nloosedepchildrenintu") || !Name.compare("nloosedepchintu") ||
			!Name.compare("nloosedepchildrenintaxunit") || !Name.compare("nloosedepchintaxunit")) { m_IdentNo=28; return 1; }
	else if(!Name.compare("nadultsintu") || !Name.compare("nadultsintaxunit")) { m_IdentNo=29; return 1; }
	else if(!Name.compare("hasminvalintu")) { m_IdentNo=30; return 1; }
	else if(!Name.compare("getpartnerincome")) { m_IdentNo=31; return 1; }
	else if(!Name.compare("getcoupleincome")) { m_IdentNo=32; return 1; }
	else if(!Name.compare("getparentsincome")) { m_IdentNo=33; return 1; } 
	else if(!Name.compare("getparentincome")) { m_IdentNo=-33; return 1; } //phase out
	else if(!Name.compare("getmotherincome")) { m_IdentNo=34; return 1; }
	else if(!Name.compare("getfatherincome")) { m_IdentNo=35; return 1; }
	else if(!Name.compare("getpartnerinfo")) { m_IdentNo=36; return 1; }
	else if(!Name.compare("getmotherinfo")) { m_IdentNo=37; return 1; }
	else if(!Name.compare("getfatherinfo")) { m_IdentNo=38; return 1; }
	else if(!Name.compare("ndepparentsintu")) { m_IdentNo=39; return 1; }
	else if(!Name.compare("ndeprelativesintu")) { m_IdentNo=40; return 1; }
	else if(!Name.compare("ndepparentsandrelativesintu")) { m_IdentNo=41; return 1; }
	else if(!Name.compare("getsystemyear")) { m_IdentNo=42; return 1; }
	else if(!Name.compare("isoutputcurrencyeuro")) { m_IdentNo=43; return 1; }
	else if(!Name.compare("isparamcurrencyeuro")) { m_IdentNo=44; return 1; }
	else if(!Name.compare("getexchangerate")) { m_IdentNo=45; return 1; }
	else if(!Name.compare("isuseddatabase")) { m_IdentNo=46; return 1; }
	else if(!Name.compare("getdataincomeyear")) { m_IdentNo=47; return 1; }
	else return 0;
}

bool CEMFunction::Prepare(CEMModule *Module, std::string Name, int FootnoteNo)
{
	static int FreeFotenote = 1000;
	m_Name=Name;
	m_FootnoteNo=FootnoteNo;
	if(m_FootnoteNo==-1) m_FootnoteNo=FreeFotenote++;
	char sFN[50];
	EM_SPRINTF(sFN, "#%d_", m_FootnoteNo);
	m_sFootnote=sFN;
	m_IsGlobal=m_IsStatic=0;
	m_numValue=EMVOID;
	std::string hp;
	double n,m;

	switch(m_IdentNo)
	{
	case 8: //isntomchild
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+"n", PTYPE_VAL, Module, "0")) return 0;
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+"m", PTYPE_VAL, Module, "0")) return 0;
		n = Module->m_ParamAdmin.GetValParam(m_sFootnote+"n")->m_numValue;
		m = Module->m_ParamAdmin.GetValParam(m_sFootnote+"m")->m_numValue;
		if (n>m && !CEMError::CritErr("#_N is greater than #_M", Module)) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetValParam(m_sFootnote+"n"));
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetValParam(m_sFootnote+"m"));
		break;
	case 21: //hasmaxvalintu (old name: isrichestintu)
	case 30: //hasminvalintu
		hp="val"; if(Module->m_ParamAdmin.IsParamDefined(m_sFootnote+"means")) hp="means";
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+hp, PTYPE_VARIL, Module, "0")) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetVarILParam(m_sFootnote+hp));
		if(!Module->m_ParamAdmin.CheckOptParamExists(m_sFootnote+"unique", PTYPE_YESNO, Module, "0")) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetYesNoParam(m_sFootnote+"unique"));
		if(!Module->m_ParamAdmin.CheckOptParamExists(m_sFootnote+"adults_only", PTYPE_YESNO, Module, "0")) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetYesNoParam(m_sFootnote+"adults_only"));
		break;
	case 22: //ndepchildrenintu
	case 23: //npersinunit
	case 29: //nadultsintu
	case -9: //nchofperson
	case -10: //ndepchofperson
	case 9: //nchofcouple
	case 10: //ndepchofcouple
		if(!Module->m_ParamAdmin.CheckOptParamExists(m_sFootnote+"agemin", PTYPE_VAL, Module, NEGINFINITE)) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetValParam(m_sFootnote+"agemin"));
		if(!Module->m_ParamAdmin.CheckOptParamExists(m_sFootnote+"agemax", PTYPE_VAL, Module, POSINFINITE)) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetValParam(m_sFootnote+"agemax"));
		break;
	case 31: //getpartnerincome
	case 32: //getcoupleincome
	case 33: //getparentsincome
	case -33: //getparentincome (phase out)
	case 34: //getmotherincome
	case 35: //getfatherincome
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+"income", PTYPE_VARIL, Module, "0")) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetVarILParam(m_sFootnote+"income"));
		break;
	case 36: //getpartnerinfo
	case 37: //getmotherinfo
	case 38: //getfatherinfo
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+"info", PTYPE_VARIL, Module, "0")) return 0;
		m_Args.insert(m_Args.end(), Module->m_ParamAdmin.GetVarILParam(m_sFootnote+"info"));
		break;
	case 42: //getsystemyear
		m_IsGlobal = 1;
		m_IsStatic = 1;
		//assess already here as static
		hp = Module->m_System->m_Year != "" ? Module->m_System->m_Year : CEMUtilities::ExtractSystemYear(Module->m_System->m_Name);
		m_numValue = (hp.empty()) ? -1 : CEMUtilities::StrToDouble(hp);
		break;
	case 46: //isuseddatabase
		m_IsGlobal = 1;
		m_IsStatic = 1;
		if(!Module->m_ParamAdmin.CheckCompParamExists(m_sFootnote+"databasename", PTYPE_NAME, Module, "")) return 0;
		//assess already here as static
		hp = Module->m_ParamAdmin.GetGeneralParam(m_sFootnote+"databasename")->m_strValue;
		if (hp != "n/a")
			hp = CEMUtilities::AppendExtension(hp);
		m_numValue = CEMUtilities::DoesValueMatchPattern(hp, Module->m_Control->m_DataSet);
		break;
	case 47: //getdataincomeyear
		m_IsGlobal = 1;
		m_IsStatic = 1;
		//assess already here as static
		m_numValue = Module->m_Control->m_DataIncomeYear;
		break;
	}
	return 1;
}

bool CEMFunction::Assess(CEMModule *Module, CEMTaxunit *TU, PersonVarContainer_t *HHVar)
{
	m_numValue=0;
	PersonRefContainer_t::iterator itp;
	PersonVarContainer_t::iterator ith;
	unsigned int i;
	std::string VoidVar="";
	//find head as all individual conditions are assessed for the head of the taxunit
	//note that this "head" is not necessarily really the head of the taxunit, as func_elig iteratively sets each tu-member to head
	CEMPerson *tuHead=NULL;
	for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		if(itp->m_Index==TU->m_HeadIndex) { tuHead=&(*itp); break; }
	double hp1=0, hp2=0, hp3=0; std::string shp1="";
	switch(m_IdentNo)
	{
	case 0: //ismarried
		m_numValue=TU->GetVarVal(Module->m_Control->m_ivMaritStat, HHVar, TU->m_HeadIndex)==2;
		break;
	case 1: //iscohabiting
		if(TU->GetVarVal(Module->m_Control->m_ivMaritStat, HHVar, TU->m_HeadIndex)==2) break;
		if(TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex)<=0) break;
		m_numValue=1;
		break;
	case 2: //iswithpartner
		if(TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex)<=0) break;
		m_numValue=1;
		break;
	case 3: //isloneparent
		if(TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex)>0) break;
		hp1=TU->GetVarVal(Module->m_Control->m_ivPID, HHVar, TU->m_HeadIndex);
		for(ith=HHVar->begin(); ith!=HHVar->end(); ith++)
			if(hp1==ith->at(Module->m_Control->m_ivMotherID)
				|| hp1==ith->at(Module->m_Control->m_ivFatherID)) { m_numValue=1; break; }
		break;
	case 4: //isloneparentofdepchild
		if(tuHead->m_IsLonePar==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though lone parent condition (LoneParentCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being a lone parent is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsLonePar;
		break;
	case 5: //isparent
		hp1=TU->GetVarVal(Module->m_Control->m_ivPID, HHVar, TU->m_HeadIndex);
		hp2=TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex);
		if(hp2==0) hp2=-1;
		for(ith=HHVar->begin(); ith!=HHVar->end(); ith++)
			if(hp1==ith->at(Module->m_Control->m_ivMotherID)
				|| hp2==ith->at(Module->m_Control->m_ivMotherID)
				|| hp1==ith->at(Module->m_Control->m_ivFatherID)
				|| hp2==ith->at(Module->m_Control->m_ivFatherID))
				{ m_numValue=1; break; }
		break;
	case 6: //isparentofdepchild
		hp1=TU->GetVarVal(Module->m_Control->m_ivPID, HHVar, TU->m_HeadIndex);
		hp2=TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex);
		if(hp2==0) hp2=-1;
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{
			if(!itp->m_IsDepChild || itp->m_Index==TU->m_HeadIndex) continue; //second condition: can't be one's own parent
			if(hp1==(*HHVar)[itp->m_Index][Module->m_Control->m_ivMotherID]
				|| hp2==(*HHVar)[itp->m_Index][Module->m_Control->m_ivMotherID]
				|| hp1==(*HHVar)[itp->m_Index][Module->m_Control->m_ivFatherID]
				|| hp2==(*HHVar)[itp->m_Index][Module->m_Control->m_ivFatherID])
				{ m_numValue=1; break; }
			//assign loose children to head/partner of taxunit
			if(itp->m_IsLooseDepChild!=1) continue;
			if(tuHead->m_IsHead || tuHead->m_IsPartner) { m_numValue=1; break; }
		}
		break;
	case 7: //isdependentparent
		if(tuHead->m_IsDepPar==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though dependent parent condition (DepParentCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being a dependent parent is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsDepPar;
		break;
	case 8: //isntomchild
		if(!tuHead->m_IsDepChild) break;
		hp1=TU->GetVarVal(Module->m_Control->m_ivAge, HHVar, TU->m_HeadIndex);
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{
			if(!itp->m_IsDepChild) continue;
			double Age=TU->GetVarVal(Module->m_Control->m_ivAge, HHVar, itp->m_Index);
			if(hp1<Age) ++hp2;  //older
			if(hp1==Age && itp->m_Index<tuHead->m_Index) ++hp2; //equal age
		}
		m_numValue=((hp2+1>=((CEMValParam*)(m_Args[0]))->m_numValue) && (hp2+1<=((CEMValParam*)(m_Args[1]))->m_numValue));
		break;
	case -9: //nchildrenofperson: query is phased out to be replaced by nch..ofcouple (because of misleading name)
	case 9: //nchildrenofcouple
		hp1=TU->GetVarVal(Module->m_Control->m_ivPID, HHVar, TU->m_HeadIndex);
		hp2=TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex);
		if(hp2==0) hp2=-1;
		for(ith=HHVar->begin(); ith!=HHVar->end(); ith++)
			if((hp1==ith->at(Module->m_Control->m_ivMotherID)
				|| hp2==ith->at(Module->m_Control->m_ivMotherID)
				|| hp1==ith->at(Module->m_Control->m_ivFatherID)
				|| hp2==ith->at(Module->m_Control->m_ivFatherID))
				&& ith->at(Module->m_Control->m_ivAge)>=m_Args[0]->m_numValue
				&& ith->at(Module->m_Control->m_ivAge)<=m_Args[1]->m_numValue)
				++m_numValue;
		if(m_IdentNo==-9) { if(!CEMError::NonCritErr("Query 'nChildrenOfPerson' has phase-out status. Please consider using 'nChildrenOfCouple' instead.", Module)) return 0; }
		break;
	case -10: //ndepchildrenofperson: query is phased out to be replaced by ndepch..ofcouple (because of misleading name)
	case 10: //ndepchildrenofcouple
		hp1=TU->GetVarVal(Module->m_Control->m_ivPID, HHVar, TU->m_HeadIndex);
		hp2=TU->GetVarVal(Module->m_Control->m_ivPartnerID, HHVar, TU->m_HeadIndex);
		if(hp2==0) hp2=-1;
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{
			if(!itp->m_IsDepChild) continue;
			if((hp1==(*HHVar)[itp->m_Index][Module->m_Control->m_ivMotherID]
				|| hp2==(*HHVar)[itp->m_Index][Module->m_Control->m_ivMotherID]
				|| hp1==(*HHVar)[itp->m_Index][Module->m_Control->m_ivFatherID]
				|| hp2==(*HHVar)[itp->m_Index][Module->m_Control->m_ivFatherID]
				//assign loose children to head/partner of taxunit
				//however loose children cannot be their own parents (that means a.o. that, if they are alone in a taxunit, they have no parent)
				|| (itp->m_IsLooseDepChild && itp->m_Index!=TU->m_HeadIndex && (tuHead->m_IsHead || tuHead->m_IsPartner)))
				&& (*HHVar)[itp->m_Index][Module->m_Control->m_ivAge]>=m_Args[0]->m_numValue
				&& (*HHVar)[itp->m_Index][Module->m_Control->m_ivAge]<=m_Args[1]->m_numValue)
				++m_numValue;
		}
		if(m_IdentNo==-10) { if(!CEMError::NonCritErr("Query 'nDepChildrenOfPerson' has phase-out status. Please consider using 'nDepChildrenOfCouple' instead.", Module)) return 0; }
		break;
	case 11: //isdependentchild
		if(tuHead->m_IsDepChild==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though dependent child condition (DepChildCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being a dependent child is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsDepChild;
		break;
	case 12: //isheadoftu
		if(tuHead->m_IsHead==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though head of assessment unit is not yet determined.", Module, "", "", "Not being head is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsHead;
		break;
	case 13: //ispartnerofheadoftu
		if(tuHead->m_IsPartner==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though partner condition (PartnerCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being partner is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsPartner;
		break;
	case 14: //isineducation
		hp1=TU->GetVarVal(Module->m_Control->m_ivEducStat, HHVar, TU->m_HeadIndex);
		hp2=TU->GetVarVal(Module->m_Control->m_ivLabSupStat, HHVar, TU->m_HeadIndex);
		m_numValue=((hp1>0) && (hp2==6));
		break;
	case 15: //isdisabled
		if(Module->m_Control->m_ivDisab!=-1) hp1=TU->GetVarVal(Module->m_Control->m_ivDisab, HHVar, TU->m_HeadIndex);
		m_numValue=(hp1>0);
		break;
	case 19: //iscivilservant
		if(Module->m_Control->m_ivCivilServ!=-1) hp1=TU->GetVarVal(Module->m_Control->m_ivCivilServ, HHVar, TU->m_HeadIndex);
		m_numValue=(hp1>0);
		break;
	case 20: //isbluecoll
		hp1=TU->GetVarVal(Module->m_Control->m_ivOcc, HHVar, TU->m_HeadIndex);
		m_numValue=(((hp1>=6) && (hp1<=9)) || (hp1==0));
		break;
	case 21: //hasmaxvalintu (old name: isrichestintu)
	case 30: //hasminvalintu
		hp1=((CEMYesNoParam*)m_Args[2])->m_numValue; //adults_only
		if(hp1) 
		{//avoid that there is no richest/poorest, because there are only children in the taxunit
			for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
				if(!itp->m_IsDepChild) break;
			if(itp==TU->m_PersonRefs.end()) hp1=0;
		}
		if(tuHead->m_IsDepChild && hp1) break;
		if(!((CEMVarILParam*)m_Args[0])->Assess(hp2, TU, HHVar, TU->m_HeadIndex, &VoidVar)) return 0; //get means of head
		if(!VoidVar.empty())
			{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+((CEMVarILParam *)m_Args[0])->m_strValue+"'.", Module, "", "", "Zero is used as default value for the not defined variables.")) return 0; }
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{//look if any person is richer/poorer than tuHead
			if(itp->m_IsDepChild && hp1) continue; //if the other one is a child and children can't be richest/poorest, the other one can't be the richest/poorest
			if(!((CEMVarILParam*)m_Args[0])->Assess(hp3, TU, HHVar, itp->m_Index, &VoidVar)) return 0; //get means of the other one
			if((m_IdentNo==21 && hp3>hp2) || (m_IdentNo==30 && hp3<hp2)) break; //the other one is richer/poorer -> tuhead is not richest/poorest
			if((m_IdentNo==21 && hp3<hp2) || (m_IdentNo==30 && hp3>hp2)) continue; //the other one is less rich/poor -> continue checking if there is a richer/poorer person
			if(!((CEMYesNoParam*)m_Args[1])->m_numValue) continue; //equally rich/poor is allowed -> continue checking if there is a richer/poorer person
			if(itp->m_Index<tuHead->m_Index) break; //equally rich/poor is not allowed and the other one has a lower pid -> tuhead is not richest/poorest
		}
		m_numValue=(itp==TU->m_PersonRefs.end()); //set isrichest/ispoorest true if no person found that rules tuhead out
		break;
	case 22: //ndepchildrenintu
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{
			if(!itp->m_IsDepChild) continue;
			double Age=TU->GetVarVal(Module->m_Control->m_ivAge, HHVar, itp->m_Index);
			if(Age>=m_Args[0]->m_numValue && Age<=m_Args[1]->m_numValue) ++m_numValue;
		}
		break;
	case 23: //npersinunit
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{
			double Age=TU->GetVarVal(Module->m_Control->m_ivAge, HHVar, itp->m_Index);
			if(Age>=m_Args[0]->m_numValue && Age<=m_Args[1]->m_numValue) ++m_numValue;
		}
		break;
	case 24: //isownchild
		if(tuHead->m_IsOwnChild==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though own child condition (OwnChildCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being an own child is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsOwnChild;
		break;
	case 25: //isowndepchild
		if(tuHead->m_IsOwnDepChild==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though own dependent child condition (OwnDepChildCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being an own dependent child is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsOwnDepChild;
		break;
	case 26: //isdeprelative
		if(tuHead->m_IsDepRel==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though dependent relative condition (DepRelativeCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being a dependent relative is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsDepRel;
		break;
	case 27: //isloosedepchild
		if(tuHead->m_IsLooseDepChild==-1)
		{
			if(!CEMError::NonCritErr("Use of query '"+m_Name+"' though loose dependent child condition (LooseDepChildCond) of assessment unit specification is not yet evaluated.", Module, "", "", "Not being a loose dependent child is used as default.")) return 0;
			m_numValue=0;
		}
		m_numValue=tuHead->m_IsLooseDepChild;
		break;
	case 28: //nloosedepchildrenintu
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
			m_numValue+=itp->m_IsLooseDepChild;
		break;
	case 29: //nadultsintu
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
		{
			if(itp->m_IsDepChild) continue;
			double Age=TU->GetVarVal(Module->m_Control->m_ivAge, HHVar, itp->m_Index);
			if(Age>=m_Args[0]->m_numValue && Age<=m_Args[1]->m_numValue) ++m_numValue;
		}
		break;
	case 31: //getpartnerincome
	case 36: //getpartnerinfo
		hp1=(*HHVar)[TU->m_HeadIndex][Module->m_Control->m_ivPartnerID];
		for(i=0; i<HHVar->size(); ++i)
		{
			if(hp1!=(*HHVar)[i][Module->m_Control->m_ivPID]) continue;
			if(!((CEMVarILParam *)m_Args[0])->Assess(m_numValue, TU, HHVar, i, &VoidVar)) return 0;
			if(!VoidVar.empty())
				{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+((CEMVarILParam *)m_Args[0])->m_strValue+"'.", Module, "", "", "Zero is used as default value for the not defined variables.")) return 0; }
			break;
		}
		hp1=0; if(((CEMVarILParam *)m_Args[0])->m_viType==VITYPE_VAR && !Module->m_Control->m_VarIndexV[((CEMVarILParam *)m_Args[0])->m_VarIndex].m_Monetary) hp1=1;
		if(m_IdentNo==31 && hp1) { if(!CEMError::NonCritErr("Query 'GetPartnerIncome' is supposed to be used with monetary variables or incomelists. Please consider using 'GetPartnerInfo' instead.", Module)) return 0; }
		if(m_IdentNo==36 && !hp1) { if(!CEMError::NonCritErr("Query 'GetPartnerInfo' is supposed to be used with non-monetary variables. Please consider using 'GetPartnerIncome' instead.", Module)) return 0; }
		break;
	case 32: //getcoupleincome
		if(!((CEMVarILParam *)m_Args[0])->Assess(m_numValue, TU, HHVar, TU->m_HeadIndex, &VoidVar)) return 0;
		if(!VoidVar.empty())
			{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+((CEMVarILParam *)m_Args[0])->m_strValue+"'.", Module, "", "", "Zero is used as default value for the not defined variables.")) return 0; }
		hp1=(*HHVar)[TU->m_HeadIndex][Module->m_Control->m_ivPartnerID];
		for(i=0; i<HHVar->size(); ++i)
		{
			if(hp1!=(*HHVar)[i][Module->m_Control->m_ivPID]) continue;
			if(!((CEMVarILParam *)m_Args[0])->Assess(hp2, TU, HHVar, i, &VoidVar)) return 0;
			m_numValue+=hp2;
			break;
		}
		break;
	case 33: //getparentsincome
	case -33: //getparentincome (phase out)
		hp1=(*HHVar)[TU->m_HeadIndex][Module->m_Control->m_ivMotherID];
		hp2=(*HHVar)[TU->m_HeadIndex][Module->m_Control->m_ivFatherID];
		for(i=0; i<HHVar->size(); ++i)
		{
			if(hp1!=(*HHVar)[i][Module->m_Control->m_ivPID]
				&& hp2!=(*HHVar)[i][Module->m_Control->m_ivPID]) continue;
			if(!((CEMVarILParam *)m_Args[0])->Assess(hp3, TU, HHVar, i, &VoidVar)) return 0;
			if(!VoidVar.empty())
				{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+((CEMVarILParam *)m_Args[0])->m_strValue+"'.", Module, "", "", "Zero is used as default value for the not defined variables.")) return 0; }
			m_numValue+=hp3;
			if(hp1==hp2) break;
		}
		if(m_IdentNo==-33) { if(!CEMError::NonCritErr("Query 'GetParentIncome' has phase-out status. Please consider using 'GetParentsIncome' instead (note the plural).", Module)) return 0; }
		break;
	case 34: //getmotherincome
	case 35: //getfatherincome
	case 37: //getmotherinfo
	case 38: //getfatherinfo
		if(m_IdentNo==34 || m_IdentNo==37) hp1=(*HHVar)[TU->m_HeadIndex][Module->m_Control->m_ivMotherID];
		else hp1=(*HHVar)[TU->m_HeadIndex][Module->m_Control->m_ivFatherID];
		for(i=0; i<HHVar->size(); ++i)
		{
			if(hp1!=(*HHVar)[i][Module->m_Control->m_ivPID]) continue;
			if(!((CEMVarILParam *)m_Args[0])->Assess(m_numValue, TU, HHVar, i, &VoidVar)) return 0;
			if(!VoidVar.empty())
				{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+((CEMVarILParam *)m_Args[0])->m_strValue+"'.", Module, "", "", "Zero is used as default value for the not defined variables.")) return 0; }
			break;
		}
		hp1=0; if(((CEMVarILParam *)m_Args[0])->m_viType==VITYPE_VAR && !Module->m_Control->m_VarIndexV[((CEMVarILParam *)m_Args[0])->m_VarIndex].m_Monetary) hp1=1;
		shp1="GetMother"; if(m_IdentNo==35 || m_IdentNo==38) shp1="GetFather";
		if((m_IdentNo==34 || m_IdentNo==35) && hp1) { if(!CEMError::NonCritErr("Query '"+shp1+"Income' is supposed to be used with monetary variables or incomelists. Please consider using '"+shp1+"Info' instead.", Module)) return 0; }
		if((m_IdentNo==37 || m_IdentNo==38) && !hp1) { if(!CEMError::NonCritErr("Query '"+shp1+"Info' is supposed to be used with non-monetary variables. Please consider using '"+shp1+"Income' instead.", Module)) return 0; }
		break;
	case 39: //ndepparentsintu
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
			m_numValue += itp->m_IsDepPar;
		break;
	case 40: //ndeprelativesintu
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
			m_numValue += itp->m_IsDepRel;
		break;
	case 41: //ndepparentsandrelativesintu
		for(itp=TU->m_PersonRefs.begin(); itp!=TU->m_PersonRefs.end(); itp++)
			if (itp->m_IsDepRel || itp->m_IsDepPar)
				++m_numValue;
		break;
	case 42: //getsystemyear
		//nothing to do as static value is already assessed in Prepare
		break;
	case 43: //isoutputcurrencyeuro
		m_numValue = Module->m_System->m_OutputCurrencyEuro;
		break;
	case 44: //isparamcurrencyeuro
		m_numValue = Module->m_System->m_ParamCurrencyEuro;
		break;
	case 45: //getexchangerate
		m_numValue = Module->m_System->m_ExchRate;
		break;
	case 46: //isuseddatabase
		//nothing to do as static value is already assessed in Prepare
		break;
	case 47: //getdataincomeyear
		//nothing to do as static value is already assessed in Prepare
		break;
	}
	return 1;
}

bool CEMFormulaParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;

	m_numValue=EMVOID;
	m_strValue="("+m_strValue+")";

	std::string fm=m_strValue;
	m_Operands.clear();

	//replace possible uses of base_amount, base_var, base_il (for bencalc)
	size_t fa=fm.find("base_amount");
	size_t fv=fm.find("base_var");
	size_t fi=fm.find("base_il");
	size_t fb=fm.find("$base");
	if(fa!=std::string::npos || fv!=std::string::npos || fi!=std::string::npos || fb!=std::string::npos)
	{
		CEMFormulaParam *pB = NULL;
		if(m_Module->m_ParamAdmin.IsParamDefined("base")) pB=m_Module->m_ParamAdmin.GetFormulaParam("base");
		if(pB && pB->m_strValue!="n/a")
		{
			fm = CEMUtilities::Replace(fm, "base_amount", "("+pB->m_strValue+")");
			fm = CEMUtilities::Replace(fm, "base_var", "("+pB->m_strValue+")");
			fm = CEMUtilities::Replace(fm, "base_il", "("+pB->m_strValue+")");
			fm = CEMUtilities::Replace(fm, "$base", "("+pB->m_strValue+")");
		}
		else
		{
			if(!CEMError::CritErr("Use of 'base_x' in formula without respective definition of parameter.", this)) return 0;
			fm = CEMUtilities::Replace(fm, "base_amount", "0");
			fm = CEMUtilities::Replace(fm, "base_var", "0");
			fm = CEMUtilities::Replace(fm, "base_il", "0");
			fm = CEMUtilities::Replace(fm, "$base", "0");
		}
	}
	//remove spaces
	fm=CEMUtilities::Replace(fm, " ","");
	//replace x% by x/100
	fm=CEMUtilities::ReplacePercent(fm);
	//use [ to describe not( with one sign
	fm=CEMUtilities::Replace(fm, "!(", "[");
	//// use { } ] as the operand for min/max/abs (i.e. use an arbitrary one-character-operand)
	fm=CEMUtilities::ReplaceAbsMaxMin(fm);

	std::string Formula="";
	for(;;)
	{
		std::string fPart;
		size_t fd=fm.find_first_of("([])^*\\§/-+{}");
		if(fd!=std::string::npos) fPart=fm.substr(0,fd);
		else fPart=fm;
		if(!fPart.empty())
		{
			Formula += "a";
			CEMFormOp Op;
			Op.m_FormulaParam = this;
			size_t fn=fPart.find('#');
			
			if(fn==std::string::npos)  { if (!Op.Prepare(m_Module, fPart)) return 0; }
			else
			{
				bool err=0;
				int fnNo = CEMUtilities::StrToInt(fPart.substr(fn+1),&err);
				if(err)
				{
					std::string Period;
					std::string Val=fPart;
					if(!CEMUtilities::ExtractPeriod(Val, Period)) { return CEMError::CritErr("Invalid footnote: '"+fPart+"'.", this); }
					if(!Op.Prepare(m_Module, Val, -1, Period.at(0))) return 0;
				}
				else { if(!Op.Prepare(m_Module, fPart.substr(0,fn), fnNo)) return 0; }
			}
			m_Operands.insert(m_Operands.end(), Op);
		}
		if (fd==std::string::npos) break;
		Formula+=fm[fd];
		fm=fm.substr(fd+1);
	}
	if(m_Calculator) delete m_Calculator; m_Calculator=new CEMFormula;
	if(!m_Calculator->ParseFormula(Formula)) return CEMError::CritErr("Invalid formula: "+m_Calculator->m_ErrMessage, this);

	//if all operands are "static", i.e. amounts or static functions
	//there is no need to assess the formula for each TU, which would take too much time
	m_IsStatic=0;
	bool IsStatic=1;
	std::vector<CEMFormOp>::iterator it;
	for(it=m_Operands.begin(); it!=m_Operands.end(); it++)
		if(!it->m_IsStatic) { IsStatic=0; break; }

	m_IsGlobal=1;//m_IsGlobal=1 means formula contains only static operands (amounts and static functions) and global variables or functions using global variables only
	if(!IsStatic)
	{//if formula is not static it still can be global
		for(it=m_Operands.begin(); it!=m_Operands.end(); it++)
			if(!it->m_IsGlobal) { m_IsGlobal=0; break; }
		return 1;
	}
	//if formula is static (and therewith automatically global) assess its value immediately
	if(!Assess(m_numValue)) return 0;
	m_IsStatic=1;
	return 1;
}

bool CEMFormulaParam::Assess(double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, double DefVal)
{
	PersIndex; DefVal; //just to avoid warning about unreferenced formal parameter
	if(m_IsStatic) { val=m_numValue; return 1; }
	std::vector<double> amts;

	for(std::vector<CEMFormOp>::iterator ito=m_Operands.begin(); ito!=m_Operands.end(); ito++)
	{
		if(!ito->Assess(m_Module, TU, HHVar)) return 0;
		amts.insert(amts.end(), ito->m_numValue);
	}
	if(!m_Calculator->CalcFormula(val, amts))
	{
		if(!CEMError::NonCritErr("Invalid formula: "+m_Calculator->m_ErrMessage, this, "Function result is set to zero.")) return 0;
		val=0;
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCondParam and helper classes
********************************************************************************************/
void CEMCondition::Set(CEMModule *Module, std::string RowCond, std::string &sFootnote, size_t StartPos, size_t EndPos)
{
	m_Module=Module;
	m_Control=m_Module->m_Control;
	m_System=m_Module->m_System;
	m_RowCond=RowCond;
	m_sFootnote=sFootnote;
	m_StartPos=StartPos;
	m_EndPos=EndPos;
}

bool CEMCondition::Cook()
{
	m_RowCond = CEMUtilities::ReplaceAbsMaxMin(m_RowCond); //this function is actually called CEMFormulaParam::Prepare again, but <abs>() <min>() and <max>() would prevent finding the real comparison oparator (because of the <>)
	size_t pos = m_RowCond.find_first_of("<>=");
	if(pos==std::string::npos) m_CompOp=COMPOP_NOC;
	else
	{
		char c=m_RowCond.at(pos);
		switch(c)
		{
		case '<':
			if(pos+1<m_RowCond.length() && m_RowCond.at(pos+1)=='=') m_CompOp=COMPOP_LE;
			else m_CompOp=COMPOP_L;
			break;
		case '>':
			if(pos+1<m_RowCond.length() && m_RowCond.at(pos+1)=='=') m_CompOp=COMPOP_GE;
			else m_CompOp=COMPOP_G;
			break;
		case '=':
			if(pos>0 && m_RowCond.at(pos-1)=='!') m_CompOp=COMPOP_NEQ;
			else m_CompOp=COMPOP_EQ;
			break;
		}
	}
	if(!m_sFootnote.empty())
	{//put the footnotes after the operands instead of after the } (in order to make the ArithOp-Module able to interpret them)
		if(m_CompOp!=COMPOP_NOC && m_CompOp!=COMPOP_NEQ) m_RowCond.insert(pos, m_sFootnote);
		if(m_CompOp==COMPOP_NEQ) m_RowCond.insert(pos-1, m_sFootnote);
		m_RowCond=CEMUtilities::Replace(m_RowCond, "+", m_sFootnote+"+");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "-", m_sFootnote+"-");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "*", m_sFootnote+"*");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "/", m_sFootnote+"/");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "\\", m_sFootnote+"\\");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "^", m_sFootnote+"^");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "<min>", m_sFootnote+"<min>");
		m_RowCond=CEMUtilities::Replace(m_RowCond, "<max>", m_sFootnote+"<max>");
		m_RowCond=CEMUtilities::Replace(m_RowCond, ")", m_sFootnote+")");
		m_RowCond=CEMUtilities::Replace(m_RowCond, ")"+m_sFootnote+")", "))");
		m_RowCond+=m_sFootnote;
	}
	pos = m_RowCond.find_first_of("<>=");
	switch(m_CompOp)
	{
	case COMPOP_NOC:
		pos=0;
		m_LeftSide="";
		m_RightSide=m_RowCond;
		break;
	case COMPOP_EQ:
	case COMPOP_G:
	case COMPOP_L:
		m_LeftSide=m_RowCond.substr(0,pos);
		m_RightSide=m_RowCond.substr(pos+1);
		break;
	case COMPOP_NEQ:
		m_LeftSide=m_RowCond.substr(0,pos-1);
		m_RightSide=m_RowCond.substr(pos+1);
		break;
	case COMPOP_GE:
	case COMPOP_LE:
		m_LeftSide=m_RowCond.substr(0,pos);
		m_RightSide=m_RowCond.substr(pos+2);
		break;
	}
	if(m_RightSide.find_first_of("<>=") != std::string::npos)
	{
		m_CookedCond="(0-0)";
		return CEMError::CritErr("Invalid condition: more than one comparison operator found in sequence '"+m_RowCond+ "'.", m_Module);
	}

	m_LeftSide="("+m_LeftSide+")"; m_RightSide="("+m_RightSide+")";
	switch(m_CompOp)
	{
	case COMPOP_NOC:
		m_CookedCond="(!(!("+m_RowCond+")))"; //the two ! are just for transformation in 0/1
		break;
	case COMPOP_EQ:
		m_CookedCond="(!("+m_LeftSide+"-"+m_RightSide+"))"; //!(a-b)
		break;
	case COMPOP_NEQ:
		m_CookedCond="(!(!("+m_LeftSide+"-"+m_RightSide+")))"; //!(!(a-b))
		break;
	case COMPOP_G:
		m_CookedCond="(!(!(("+m_LeftSide+"-"+m_RightSide+")<max>0)))"; //!(!(max(a-b,0)))
		break;
	case COMPOP_GE:
		m_CookedCond="(!(("+m_RightSide+"-"+m_LeftSide+")<max>0))"; //!(max(b-a,0))
		break;
	case COMPOP_L:
		m_CookedCond="(!(!(("+m_RightSide+"-"+m_LeftSide+")<max>0)))"; //!(!(max(b-a,0)))
		break;
	case COMPOP_LE:
		m_CookedCond="(!(("+m_LeftSide+"-"+m_RightSide+")<max>0))"; //!(max(a-b,0))
		break;
	}
	return 1;
}

bool CEMCondParam::Prepare()
{
	if(!CEMParam::Prepare()) return 0;
	if(m_strValue=="n/a") return 1;

	std::vector<CEMCondition> Conds;
	Conds.clear();
	std::string EligCond = m_strValue;
	EligCond=CEMUtilities::Replace(EligCond, " ", "");
	EligCond=CEMUtilities::Replace(EligCond, "{default}", "{1}"); //temporarily
	size_t spos=0, epos=0;
	for(;;)
	{
		spos=EligCond.find('{',epos);
		if(spos==std::string::npos) break;
		epos=EligCond.find('}',spos);
		if(epos==std::string::npos) return CEMError::CritErr("Invalid condition: mismatch between '{' and '}'.", this);
		std::string token = EligCond.substr(spos+1, epos-spos-1);
		if(token.find('{')!=std::string::npos) return CEMError::CritErr("Invalid condition: '{' within '{...}' - subconditions are not permitted.", this);
		std::string sFootnote;
		if(epos+1<EligCond.length() && EligCond.at(epos+1)=='#')
		{
			if(epos+2==EligCond.length()) return CEMError::CritErr("Invalid condition: footnote without a number after sequence '"+token+ "'.", this);
			size_t fne=EligCond.find_first_not_of("0123456789",epos+2);
			if(fne==std::string::npos) fne=EligCond.length();
			if(!CEMUtilities::StrToInt(EligCond.substr(epos+2, fne-(epos+2)))) return CEMError::CritErr("Invalid condition: invalid footnote after sequence '"+token+ "'.", this);
			sFootnote = EligCond.substr(epos+1, fne-(epos+1));
			epos = fne-1;
		}
		CEMCondition Cond;
		Cond.Set(m_Module, token, sFootnote, spos, epos);
		Conds.insert(Conds.end(), Cond);
	}
	if(!Conds.size()) return CEMError::CritErr("Invalid condition: no conditons found - check if conditions are put within '{...}'.", this);
	std::string CookedEligCond = EligCond.substr(0, Conds[0].m_StartPos);
	for(std::vector<CEMCondition>::iterator it=Conds.begin(); it!=Conds.end(); ++it)
	{
		if(!it->Cook()) return 0;
		CookedEligCond += "(";
		CookedEligCond += it->m_CookedCond;
		CookedEligCond += ")";
		if(it+1==Conds.end())
		{
			CookedEligCond += EligCond.substr(it->m_EndPos+1, EligCond.length()-it->m_EndPos);
			break;
		}
		CookedEligCond += EligCond.substr(it->m_EndPos+1, (it+1)->m_StartPos-it->m_EndPos-1);
	}
	CookedEligCond = CEMUtilities::Replace(CookedEligCond, "&", "*");
	CookedEligCond = CEMUtilities::Replace(CookedEligCond, "|", "+");

	if(m_Formula) { delete m_Formula; m_Formula=NULL; }
	m_Formula = new CEMFormulaParam;
	m_Formula->m_strValue = CookedEligCond;
	m_Formula->m_Module = m_Module;
	m_Formula->m_System = m_System;
	m_Formula->m_Control = m_Control;
	m_Formula->m_Name = "CEMCondParam using CEMFormulaParam";
	if(!m_Formula->Prepare()) return 0;
	m_IsGlobal=m_Formula->m_IsGlobal;
	return 1;
}

bool CEMCondParam::Assess(int OutVarIndex, double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, int ResVarIndex)
{
	int bkup_HeadIndex = TU->m_HeadIndex;
	val=0;
	for(PersonRefContainer_t::iterator it=TU->m_PersonRefs.begin(); it!=TU->m_PersonRefs.end(); it++)
	{	//for each person in taxunit call m_Formula->Assess with TU set to Elig-TU, in order to get tu-level information fot the tu
		//but with this person being head, in order to get personal level information for this person
		if(PersIndex!=-1 && PersIndex!=it->m_Index) continue;
		TU->m_HeadIndex = it->m_Index;
		double elig;
		if(!m_Formula->Assess(elig, TU, HHVar)) return 0;
		elig=(elig!=0);
		if(OutVarIndex!=-1) TU->OverwriteVarVal(elig, OutVarIndex, HHVar, it->m_Index);
		if(ResVarIndex!=-1) TU->OverwriteVarVal(elig, ResVarIndex, HHVar, it->m_Index);
		val+=elig;
	}
	TU->m_HeadIndex = bkup_HeadIndex;
	return 1;
}

bool CEMCondParam::Assess(double &val, CEMTaxunit *TU, PersonVarContainer_t *HHVar, int PersIndex, double DefVal)
{
	DefVal; //just to avoid warning about unreferenced formal parameter
	return Assess(-1, val, TU, HHVar, PersIndex);
}

/********************************************************************************************
 functions class CEMParamAdmin
********************************************************************************************/
bool CEMParamAdmin::InsertParam(int Type, CEMModule *Module, std::string Name, std::string Val, std::string ParId, int Compulsory, int Single, std::string Group, int Line)
{
	CEMParam *EMParam=NULL;
	switch(Type)
	{
	case PTYPE_NAME:
	case PTYPE_CATEG:
		EMParam = new CEMParam;
		break;
	case PTYPE_SWITCH:
		EMParam = new CEMSwitchParam;
		break;
	case PTYPE_VAR:
		EMParam = new CEMVarParam;
		break;
	case PTYPE_VAL:
		EMParam = new CEMValParam;
		break;
	case PTYPE_IL:
		EMParam = new CEMILParam;
		break;
	case PTYPE_YESNO:
		EMParam = new CEMYesNoParam;
		break;
	case PTYPE_VARIL:
		EMParam = new CEMVarILParam;
		break;
	case PTYPE_TU:
		EMParam = new CEMTUParam;
		break;
	case PTYPE_FORMULA:
		EMParam = new CEMFormulaParam;
		break;
	case PTYPE_COND:
		EMParam = new CEMCondParam;
		break;
	}

	EMParam->m_Type = Type;
	EMParam->m_Module = Module;
	EMParam->m_System = Module->m_System;
	EMParam->m_Control = Module->m_Control;
	EMParam->m_Name = Name;
	EMParam->m_strValue = CEMUtilities::ClearEnds(Val);
	EMParam->m_Compulsory = Compulsory;
	EMParam->m_UnusedFootnotePar = (ParId!="op" && Name.at(0)=='#' && Val!="n/a");
	EMParam->m_Period = "m";
	EMParam->m_Single = Single;
	EMParam->m_Group = Group;
	EMParam->m_Order = Line;
	EMParam->m_Id=CEMUtilities::GenId(EMParam->m_Module->m_PolName, ParId, Line);
	if(ParId!="op") EMParam->m_System->AddToParList(EMParam, ParId.empty());
	if(Single && GetParam(Name, Group, 0)!=NULL)
	{
		char err[500] = "", grperr[100] = "";
		if (!Group.empty()) EM_SPRINTF(grperr, " Check if group '%s' is already in use.", Group.c_str());
		EM_SPRINTF(err, "Multiple definition of parameter.%s", grperr);
		return CEMError::CritErr(err, EMParam);
	}

	if(Type==PTYPE_SWITCH) { if(!EMParam->Prepare()) return 0; } //switch parameter must be evaluated immediately, because other parameters do not need to be evaluated (in CheckParam) if module is switched off

	if(EMParam->m_Name.substr(0,2)=="#_")
	{//new user interface: footnote number is stored in group instead of parameter name
		EMParam->m_Name = "#" + EMParam->m_Group + EMParam->m_Name.substr(1);
		EMParam->m_Group = "";
	}

	m_Params.insert(m_Params.end(), EMParam);
	return 1;
}

bool CEMParamAdmin::Prepare()
{
	size_t i=0;
	for(; i<m_Params.size(); ++i)
	{
		if(m_Params[i]->m_Name.substr(0,1) == "#") //first prepare footnote parameters as formulas and conditions assume that #amount and query parameters are prepared
			{ if(!m_Params[i]->Prepare()) return 0; }
	}
	//note: CEMFunction::Prepare adds parameters to m_Params, but as it adds them at the end (after current m_Params.size) that's ok (but it wouldn't if iterator-version of loop was used)
	for(i=0; i<m_Params.size(); ++i)
	{
		if(m_Params[i]->m_Name.substr(0,1) != "#") //than prepare other parameters
			{ if(!m_Params[i]->Prepare()) return 0; }
	}
	return 1;
}

bool CEMParamAdmin::PrepareSkip(std::vector<std::string> SkipParNames)
{
	for(Param_t::iterator itp=m_Params.begin(); itp!=m_Params.end(); ++itp)
	{
		bool skip = false;
		for(std::vector<std::string>::iterator its=SkipParNames.begin(); its!=SkipParNames.end(); ++its)
			{ if((*itp)->m_Name==(*its)) { skip = true; break; } }
		if (!skip)
			{ if(!(*itp)->Prepare()) return 0; }
	}
	return 1;
}

bool CEMParamAdmin::Prepare(std::string ParName)
{
	return GetGeneralParam(ParName)->Prepare();
}

void CEMParamAdmin::FreeMemory()
{
	for(Param_t::iterator itp=m_Params.begin(); itp!=m_Params.end(); ++itp)
		delete (*itp);
}

bool CEMParamAdmin::IsParamDefined(std::string ParName, bool AcceptNA, std::string Group)
{
	CEMParam *p = GetParam(ParName, Group, 0);
	if(p==NULL) return 0;

	p->m_UnusedFootnotePar=0;
	if(AcceptNA || (p->m_strValue!="n/a")) return 1; return 0;
}

bool CEMParamAdmin::CheckCompParamExists(std::string ParName, int Type, CEMModule *Module, std::string DefVal, std::string Group)
{
	CEMParam *p = GetParam(ParName, Group, 0);
	if(p!=NULL)
	{
		p->m_UnusedFootnotePar=0;
		return 1;
	}
	if(!CheckOptParamExists(ParName, Type, Module, DefVal)) return 0;
	return CEMError::CritErr("Compulsory parameter not defined.", Module, ParName);
}

bool CEMParamAdmin::CheckOptParamExists(std::string ParName,
	int Type, CEMModule *Module, std::string DefVal, bool DoNotPrepare, int Single, std::string Group)
{
	CEMParam *p = GetParam(ParName, Group, 0);
	if(p!=NULL)
	{
		if(p->m_strValue!="n/a") { p->m_UnusedFootnotePar=0; return 1; }
		p->m_strValue=DefVal;
	}
	else { if (!InsertParam(Type, Module, ParName, DefVal, "op", 0, Single, Group, 0)) return 0; }
	if(DoNotPrepare) return 1;
	p = GetGeneralParam(ParName, Group);
	if(!p->Prepare()) return CEMError::CodeErr("Programming error: definition of optional parameter went wrong.");
	return 1;
}

int CEMParamAdmin::CheckSequence(std::string ParName, CEMModule *Module, std::string ParNamePart2, std::string ParNamePart2Alias, std::string Location, std::string SysName, std::string PolName, bool AcceptOmission)
{
	std::vector<int> sequ;
	sequ.clear();
	ParName=CEMUtilities::LCase(ParName);
	ParNamePart2=CEMUtilities::LCase(ParNamePart2);
	ParNamePart2Alias=CEMUtilities::LCase(ParNamePart2Alias);
	int maxnum=0;
	for(Param_t::iterator itp=m_Params.begin(); itp!=m_Params.end(); ++itp)
	{
		std::string pn=CEMUtilities::LCase((*itp)->m_Name);
		size_t x=pn.find(ParName);
		if(x!=0 || pn.length()<=ParName.length()) continue;
		size_t y=pn.length();
		if(ParNamePart2!="")
		{
			y=pn.rfind(ParNamePart2);
			if(y==std::string::npos)
			{
				if(ParNamePart2Alias=="") continue;
				y=pn.rfind(ParNamePart2Alias);
				if(y==std::string::npos) continue;
			}
		}
		std::string snum=pn.substr(ParName.length(), y-ParName.length());
		bool err;
		int nnum= CEMUtilities::StrToInt(snum, &err);
		if(err) continue;
		sequ.insert(sequ.end(), nnum);
		if(nnum>maxnum) maxnum=nnum;
	}
	if(AcceptOmission) return maxnum;
	if((int)(sequ.size())==maxnum)
	{
		for(int i=1; i<=maxnum; ++i)
			for(std::vector<int>::iterator it=sequ.begin(); it!=sequ.end(); it++)
				if(*it==i) { sequ.erase(it); break; }
	}
	if(!sequ.empty())
	{
		CEMError::CritErr("Inconistent numbering of parameter '"+ParName+"i"+ParNamePart2+"'.", Module);
		return -1;
	}
	return maxnum;
}

bool CEMParamAdmin::CheckFootnoteParUsage()
{
	for(Param_t::iterator itp=m_Params.begin(); itp!=m_Params.end(); ++itp)
		if((*itp)->m_UnusedFootnotePar) { (*itp)->m_UnusedFootnotePar=0; return CEMError::NonCritErr("Unused footnote parameter.", *itp); }
	return 1;
}

void CEMParamAdmin::List(std::string sep, std::ofstream *ofs)
{
	for(Param_t::iterator itp=m_Params.begin(); itp!=m_Params.end(); ++itp)
	{
		if(ofs)	(*ofs) << (*itp)->m_Name << ": " << (*itp)->m_strValue << sep;
		else std::cout << (*itp)->m_Name << ": " << (*itp)->m_strValue << sep;
	}
}

void CEMParamAdmin::RemoveNA()
{
	int nna=0;
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_strValue=="n/a") ++nna;
	for(int i=0; i<nna; ++i)
	{
		for(Param_t::iterator itd=m_Params.begin(); itd!=m_Params.end(); ++itd)
			if((*itd)->m_strValue=="n/a")
			{
				delete (*itd);
				m_Params.erase(itd);
				break; //don't know how deleting one item changes the map's structure, therefore break the loop and start from the beginning
			}
	}
}

CEMParam *CEMParamAdmin::GetParam(std::string ParName, std::string Group, bool issueCodeErr)
{
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_Name == ParName && (*it)->m_Group == Group)
			return (*it);

	if(issueCodeErr)
	{
		if(Group.empty()) CEMError::CodeErr("Parameter '" + ParName + "' not found!");
		else CEMError::CodeErr("Parameter '" + ParName + "' (group '"+Group+"') not found!");
	}
	return NULL;
}

void CEMParamAdmin::GetNotSingleParams(std::string ParName, Param_t &Params, bool InclNA, bool clear)
{
	if(clear) Params.clear();
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_Name == ParName && (InclNA || (*it)->m_strValue != "n/a"))
			Params.insert(Params.end(), *it);
}

void CEMParamAdmin::GetNotSingleParams(std::string ParName, std::vector<CEMVarParam*> &Params, bool InclNA, bool clear)
{
	if(clear) Params.clear();
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_Name == ParName && (InclNA || (*it)->m_strValue != "n/a"))
			Params.insert(Params.end(), (CEMVarParam*)(*it));
}

void CEMParamAdmin::GetNotSingleParams(std::string ParName, std::vector<CEMILParam*> &Params, bool InclNA, bool clear)
{
	if(clear) Params.clear();
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_Name == ParName && (InclNA || (*it)->m_strValue != "n/a"))
			Params.insert(Params.end(), (CEMILParam*)(*it));
}

void CEMParamAdmin::GetNotSingleParams(std::string ParName, std::vector<CEMFormulaParam*> &Params, bool InclNA, bool clear)
{
	if(clear) Params.clear();
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_Name == ParName && (InclNA || (*it)->m_strValue != "n/a"))
			Params.insert(Params.end(), (CEMFormulaParam*)(*it));
}

void CEMParamAdmin::GetNotSingleParams(std::string ParName, std::vector<CEMCondParam*> &Params, bool InclNA, bool clear)
{
	if(clear) Params.clear();
	for(Param_t::iterator it=m_Params.begin(); it!=m_Params.end(); ++it)
		if((*it)->m_Name == ParName && (InclNA || (*it)->m_strValue != "n/a"))
			Params.insert(Params.end(), (CEMCondParam*)(*it));
}

bool CEMParamAdmin::GetGroupParam(std::map<int,Param_t> &ParamGroups, std::string GroupDef, bool InclNA, bool clear, bool reportEmptyGroup)
{	//gather all group parameters of a function and sort them in a map:
	//map->first = group / map->second = all parameters with this group
	//'GroupDef' defines if a specific group is required, e.g. all parameters starting with 'comp_'
	if (clear)
		ParamGroups.clear();
	
	std::string foundGroups; foundGroups.clear();
	for (Param_t::iterator it = m_Params.begin(); it != m_Params.end(); ++it)
	{
		if (!InclNA && (*it)->m_strValue == "n/a")
			continue;
		if ((*it)->m_Name.length() < GroupDef.length())
			continue;	
		if ((*it)->m_Name.substr(0, GroupDef.length()) != GroupDef)
			continue; //no parameter of the specific group defined by 'GroupDef'

		if ((*it)->m_Group.empty())
		{
			if (reportEmptyGroup)
				if (!CEMError::NonCritErr("No group defined.", *it, "Parameter ist ignored.")) return 0;
			continue; //no group parameter or no group defined
		}

		int iGroup = CEMUtilities::StrToInt((*it)->m_Group);
		if (foundGroups.find((*it)->m_Group) == std::string::npos)
		{	//group does not yet exist in map: generate
			Param_t parVec; parVec.clear();
			ParamGroups.insert(std::pair<int,Param_t>(iGroup, parVec));
		}
		//insert parameter into respective group
		ParamGroups[iGroup].push_back(*it);
	}
	return true;
}
