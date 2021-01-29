#include <stdio.h>
#include <algorithm>
#include <fstream>
#include <time.h>
#include "EMModule.h"
#include "EMControl.h"
#include "EMUtilities.h"
#include "EMSystem.h"
#include "EMTaxUnit.h"
#include "EMDefinitions.h"
#include "EMTable.h"
#ifdef _WIN32
#include "direct.h"
#endif

/********************************************************************************************
 functions class CEMModule
********************************************************************************************/
void CEMModule::Init(std::string Name, CEMSystem *System, std::string PolName, std::string Id, int Line)
{
	m_Name = Name;
	m_System = System;
	m_Control = m_System->m_Control;
	m_PolName = PolName;
	m_ActHHVar = NULL;
	m_ActTU = NULL;
	m_Id=CEMUtilities::GenId(m_PolName, Id, Line);
	m_Order = Line;
}

// default destructor is assumed in windows, but required in Linux
CEMModule::~CEMModule() {
}

bool CEMModule::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	Column; //just to avoid warning about unreferenced formal parameter
	if(ParType==-1)
	{
		int len=int(ParName.length());
		if(!ParName.compare("switch")) { ParType=PTYPE_SWITCH; Compulsory=1; }
		else if(!ParName.compare("tax_unit")) { ParType=PTYPE_TU; Compulsory=1; }
		else if(!ParName.compare("output_var")) ParType=PTYPE_VAR;
		else if(!ParName.compare("output_add_var")) ParType=PTYPE_VAR;
		else if(!ParName.compare("result_var")) ParType=PTYPE_VAR;
		else if(!ParName.compare("who_must_be_elig")) ParType=PTYPE_CATEG;
		else if(!ParName.compare("elig_var")) ParType=PTYPE_VAR;
		else if(!ParName.compare("run_cond")) ParType=PTYPE_COND;
		else if(!ParName.substr(0,9).compare("threshold")) ParType=PTYPE_FORMULA;
		else if(!ParName.substr(0,6).compare("lowlim")) ParType=PTYPE_FORMULA;
		else if(!ParName.substr(0,5).compare("uplim")) ParType=PTYPE_FORMULA;
		else if(!ParName.compare("limpriority")) ParType=PTYPE_CATEG;
		else if(!ParName.compare("round_to")) ParType=PTYPE_VAL;
		else if(!ParName.compare("round_up")) ParType=PTYPE_VAL;
		else if(!ParName.compare("round_down")) ParType=PTYPE_VAL;
		else if(!ParName.substr(0,1).compare("#") &&
			(!ParName.substr(std::max(len-7,0)).compare("_lowlim") || !ParName.substr(std::max(len-14,0)).compare("_lowlim_amount") || !ParName.substr(std::max(len-11,0)).compare("_lowlim_var") || !ParName.substr(std::max(len-10,0)).compare("_lowlim_il")))
			ParType=PTYPE_FORMULA;
		else if(!ParName.substr(0,1).compare("#") &&
			(!ParName.substr(std::max(len-6,0)).compare("_uplim") || !ParName.substr(std::max(len-13,0)).compare("_uplim_amount") || !ParName.substr(std::max(len-10,0)).compare("_uplim_var") || !ParName.substr(std::max(len-9,0)).compare("_uplim_il")))
			ParType=PTYPE_FORMULA;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-7,0)).compare("_amount")) ParType=PTYPE_VAL;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-5,0)).compare("_type")) ParType=PTYPE_CATEG;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-6,0)).compare("_level")) ParType=PTYPE_TU;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-11,0)).compare("_level_left")) ParType=PTYPE_TU;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-12,0)).compare("_level_right")) ParType=PTYPE_TU;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-12,0)).compare("_limpriority")) ParType=PTYPE_CATEG;
		else if(!ParName.substr(0,1).compare("#") &&
			!ParName.substr(std::max(len-13,0)).compare("_databasename")) ParType=PTYPE_NAME; //parameter of query IsUsedDatabase, which can be used in run_cond
		else { if(!CEMFunction::TakeParam(ParName, ParVal, ParType, Compulsory)) return CEMError::CritErr("Unknown parameter.", this, ParName, ParVal); }
	}
	if(ParType==PTYPE_FORMULA || ParType==PTYPE_VARIL)
	{
		size_t us=ParName.find_last_of("_");
		if(!ParName.substr(us+1).compare("amount") || !ParName.substr(us+1).compare("var") || 
			!ParName.substr(us+1).compare("il")) ParName = ParName.substr(0,us);
	}
	if(!m_ParamAdmin.InsertParam(ParType, this, ParName, ParVal, ParId, Compulsory, Single, Group, Line)) return 0;
	return 1;
}

bool CEMModule::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	if(!m_ParamAdmin.Prepare()) return 0;
	
	//check if compulsory parameters are defined
	if(!m_ParamAdmin.CheckCompParamExists("tax_unit", PTYPE_TU, this, "tu_individual_"+m_Control->m_Country)) return 0;

	//add not defined optional parameters (with default values)
	if(!m_ParamAdmin.IsParamDefined("output_var") && !m_ParamAdmin.IsParamDefined("output_add_var"))
		{ if(!CEMError::CritErr("Neither parameter 'output_var' nor parameter 'output_add_var' defined.", this)) return 0; }
	else if(m_ParamAdmin.IsParamDefined("output_var") && m_ParamAdmin.IsParamDefined("output_add_var"))
		{ if(!CEMError::CritErr("Double definition of output variable.", this, "'output_var'/'output_add_var'")) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("who_must_be_elig", PTYPE_CATEG, this, "nobody")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("result_var", PTYPE_VAR, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("elig_var", PTYPE_VAR, this, "sel_s")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("threshold", PTYPE_FORMULA, this, NEGINFINITE_BR)) return 0;

	//check non standard parameters
	CEMParam *pwmbe=m_ParamAdmin.GetGeneralParam("who_must_be_elig");
	return CheckWhoMustBeElig(pwmbe->m_strValue, pwmbe->m_numValue);
}

bool CEMModule::CheckWhoMustBeElig(std::string wmbe, double &numValue)
{
	if(!wmbe.compare("nobody")) numValue = ELIGTYPE_NOBODY;
	else if(!wmbe.compare("n/a")) numValue = ELIGTYPE_NOBODY;
	else if(!wmbe.compare("0")) numValue = ELIGTYPE_NOBODY;
	else if(!wmbe.compare("one")) numValue = ELIGTYPE_ONE;
	else if(!wmbe.compare("one_member")) numValue = ELIGTYPE_ONE;
	else if (!wmbe.compare("one_adult")) numValue = ELIGTYPE_ONEADULT;
	else if (!wmbe.compare("oneadult")) numValue = ELIGTYPE_ONEADULT;
	else if (!wmbe.compare("all")) numValue = ELIGTYPE_ALL;
	else if (!wmbe.compare("all_members")) numValue = ELIGTYPE_ALL;
	else if (!wmbe.compare("tu")) numValue = ELIGTYPE_ALL;
	else if (!wmbe.compare("taxunit")) numValue = ELIGTYPE_ALL;
	else if (!wmbe.compare("all_adults")) numValue = ELIGTYPE_ALLADULTS;
	else if (!wmbe.compare("alladults")) numValue = ELIGTYPE_ALLADULTS;
	else if (!wmbe.compare("all_adult")) numValue = ELIGTYPE_ALLADULTS;
	else if (!wmbe.compare("alladult")) numValue = ELIGTYPE_ALLADULTS;
	else return CEMError::CritErr("Parameter value must be set to 'one_member'/'one_adult'/'all_members'/'all_adults'/'nobody'.", m_ParamAdmin.GetGeneralParam("who_must_be_elig"));
	return 1;
}

bool CEMModule::GetTUElig(int &Elig, int EligType, int EligVarIndex)
{
	if(EligType==-1) EligType = int(m_ParamAdmin.GetGeneralParam("who_must_be_elig")->m_numValue);
	Elig = 1;
	if(EligType==ELIGTYPE_NOBODY) return 1;
	int nElig=0, nAdult=0, nEligAdult=0;
	for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
	{
		if(EligVarIndex==-1) EligVarIndex=m_ParamAdmin.GetVarParam("elig_var")->m_VarIndex;
		double pElig = m_ActTU->GetVarVal(EligVarIndex, m_ActHHVar, it->m_Index);
		if(pElig==EMVOID)
		{
			std::string err = "Use of parameter 'who_must_be_elig' with not yet calculated eligibility variable (variable is defined by parameter 'elig_var', default 'sel_s').";
			if(m_Control->m_LogWarnings) { if(!CEMError::NonCritErr(err, this, "", "", "Parameter is ignored (respectively set to 'nobody').")) return 0; }
			else { if(!CEMError::NonCritErr(err, this)) return 0; }
			pElig=1;
		}
		nElig+=(int)pElig;
		if(!it->m_IsDepChild) { ++nAdult; nEligAdult+=(int)pElig; }
	}
	Elig=((EligType==ELIGTYPE_ONE && nElig>0) ||
		(EligType==ELIGTYPE_ONEADULT && nEligAdult>0) ||
		(EligType==ELIGTYPE_ALL && (int)(m_ActTU->m_PersonRefs.size())==nElig) ||
		(EligType==ELIGTYPE_ALLADULTS && nAdult==nEligAdult));
	return 1;
}

bool CEMModule::ApplyStdLimits()
{
	//apply upper and lower limit 
	CEMFormulaParam *pLowLim=NULL, *pUpLim=NULL;
	if(m_ParamAdmin.IsParamDefined("lowlim")) pLowLim=m_ParamAdmin.GetFormulaParam("lowlim");
	if(m_ParamAdmin.IsParamDefined("uplim")) pUpLim=m_ParamAdmin.GetFormulaParam("uplim");
	if(!ApplyLimParam(m_Result, pLowLim, pUpLim)) return 0;

	//apply threshold
	CEMFormulaParam *pThresh=m_ParamAdmin.GetFormulaParam("threshold");
	double Thresh;
	if(!pThresh->Assess(Thresh, m_ActTU, m_ActHHVar, -1, NUMNEGINFINITE)) return 0;
	if(m_Result >= Thresh) return 1;

	if(pUpLim && Thresh > pUpLim->m_numValue) return CEMError::NonCritErr("Threshold is higher than upper limit.", this, pThresh->m_Name+"/uplim", pThresh->m_strValue+"/"+pUpLim->m_strValue, "Threshold is ignored.");
	if(pLowLim && Thresh < pLowLim->m_numValue) return CEMError::NonCritErr("Threshold is lower than lower limit.", this, pThresh->m_Name+"/uplim", pThresh->m_strValue+"/"+pLowLim->m_strValue, "Threshold is ignored.");

	m_Result = (pLowLim && pLowLim->m_numValue!=NUMNEGINFINITE) ? pLowLim->m_numValue : 0.0;
	return 1;
}

bool CEMModule::ApplyLimParam(double &Result, CEMFormulaParam *pLowLim, CEMFormulaParam *pUpLim)
{
	double LowLim=NUMNEGINFINITE;
	if(pLowLim) { if(!pLowLim->Assess(LowLim, m_ActTU, m_ActHHVar, -1, NUMNEGINFINITE)) return 0; }
	double UpLim=NUMPOSINFINITE;
	if(pUpLim) { if(!pUpLim->Assess(UpLim, m_ActTU, m_ActHHVar, -1, NUMPOSINFINITE)) return 0; }
	if(Result>=LowLim && Result<=UpLim) return 1;

	if(LowLim>UpLim)
	{
		std::string ParName="";
		size_t f=0;
		if(pLowLim) f=pLowLim->m_Name.find("lowlim");
		if(f>0) ParName=pLowLim->m_Name.substr(0, f);
        ParName+="limpriority";
		CEMParam *pPri=NULL;
		if(m_ParamAdmin.IsParamDefined(ParName)) pPri=m_ParamAdmin.GetGeneralParam(ParName);
		if(!pPri)
		{
			std::string parNames = (pLowLim ? pLowLim->m_Name : "") + " " + (pUpLim ? pUpLim->m_Name : "");
			char parVals[500]; EM_SPRINTF(parVals, "%f > %f", LowLim, UpLim);
			if(!CEMError::NonCritErr("Lower limit is higher than upper limit.", this, parNames, parVals, "Upper limit overwrites lower limit.")) return 0;
			LowLim=UpLim;
		}
		else
		{
			if(pPri->m_strValue=="lower") UpLim=LowLim;
			else
			{
				if(pPri->m_strValue!="upper") { if(!CEMError::NonCritErr("Lower limit is higher than upper limit and parameter 'limpriority' set to an unknown value (allowed values: 'upper'/'lower').", pPri, "Upper limit overwrites lower limit.")) return 0; }
				LowLim=UpLim;
			}
		}
	}
	Result = std::max(Result, LowLim);
	Result = std::min(Result, UpLim);
	return 1;
}

bool CEMModule::SetOutputVars()
{
	CEMVarParam *pResVar = m_ParamAdmin.GetVarParam("result_var");
	CEMVarParam *pOutVar; 
	double AddToOutVar=0;
	if(m_ParamAdmin.IsParamDefined("output_var")) pOutVar=m_ParamAdmin.GetVarParam("output_var");
	else { pOutVar=m_ParamAdmin.GetVarParam("output_add_var"); AddToOutVar=1; }
	if(pResVar->m_strValue!="n/a") m_ActTU->OverwriteVarVal(m_Result, pResVar->m_VarIndex, m_ActHHVar);
	if(pOutVar->m_strValue=="n/a") return 1;
	if(AddToOutVar) m_ActTU->AddToVarVal(m_Result, pOutVar, m_ActHHVar);
	else m_ActTU->OverwriteVarVal(m_Result, pOutVar->m_VarIndex, m_ActHHVar);
	return 1;
}

int CEMModule::EvalRunCond(bool ignoreLoopCount)
{
	CEMTaxunit *BkUpTU = m_ActTU;
	PersonVarContainer_t *BkUpHH = m_ActHHVar;
	if(!m_ParamAdmin.IsParamDefined("run_cond")) return 1;
	CEMCondParam *pc = m_ParamAdmin.GetCondParam("run_cond");
	if (ignoreLoopCount && pc->m_strValue.find("loopcount")!=std::string::npos) return 1; //not very elegant solution for avoiding a warning for function ReadtimeChangeParam (see below)
	if(!pc->m_Formula) { if(!pc->Prepare()) return 0; }
	if(!pc->m_IsGlobal) { if(!CEMError::NonCritErr("Condition is supposed to be global but contains personal/household related operands.", pc)) return 0; }
	m_ActTU=m_System->GetStaticTU(); m_ActHHVar=&(m_ActTU->m_HH->m_Persons);
	double run;
	if(!pc->Assess(run, m_ActTU, m_ActHHVar)) return 0;
	m_ActTU=BkUpTU; m_ActHHVar=BkUpHH;
	if(!run) return -1;
	return 1;
}

bool CEMModule::Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar)
{
	m_ActTU = TU;
	m_ActHHVar = HHVar;
	m_Result = 0;

	int run = EvalRunCond();
	if(!run) return 0; /*error*/ if(run==-1) return 1; /* run condition not fulfilled */

	int Elig;
	if(!GetTUElig(Elig)) return 0;
	if(Elig)
	{
		if(!DoModCalc()) return 0;
		if(!ApplyStdLimits()) return 0;
		if(m_ParamAdmin.IsParamDefined("round_to")) m_Result=CEMUtilities::Round(m_Result, m_ParamAdmin.GetValParam("round_to")->m_numValue);
		if(m_ParamAdmin.IsParamDefined("round_up")) m_Result=CEMUtilities::Round(m_Result, m_ParamAdmin.GetValParam("round_up")->m_numValue, 1);
		if(m_ParamAdmin.IsParamDefined("round_down")) m_Result=CEMUtilities::Round(m_Result, m_ParamAdmin.GetValParam("round_down")->m_numValue, -1);
	}
	return SetOutputVars();
}

void CEMModule::CleanUp()
{
	m_ParamAdmin.FreeMemory();
}

/********************************************************************************************
 functions class CEMCM_ArithOp
********************************************************************************************/
bool CEMCM_ArithOp::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("formula")) { ParType=PTYPE_FORMULA; Compulsory=1; }
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_ArithOp::CheckParam()
{
	//check if common parameters are defined
	if(!CEMModule::CheckParam()) return 0;
	if(!m_ParamAdmin.GetSwitchParam()->m_numValue) return 1; //don't check if module is switched off
	//check if compulsory parameters are defined
	if(!m_ParamAdmin.CheckCompParamExists("formula", PTYPE_FORMULA, this, "(1)")) return 0;
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_ArithOp::DoModCalc()
{
	return m_ParamAdmin.GetFormulaParam("formula")->Assess(m_Result, m_ActTU, m_ActHHVar);
}

/********************************************************************************************
 functions class CEMCM_Elig
********************************************************************************************/
bool CEMCM_Elig::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("elig_cond")) { ParType=PTYPE_COND; Compulsory=1; }
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Elig::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	if(!m_ParamAdmin.CheckOptParamExists("output_var", PTYPE_VAR, this, "sel_s")) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("elig_cond", PTYPE_COND, this, "{0}")) return 0;
	if(!CEMModule::CheckParam()) return 0;
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_Elig::DoModCalc()
{
	int ResVarIndex = -1;
	double val;
	if(m_ParamAdmin.IsParamDefined("result_var")) ResVarIndex=m_ParamAdmin.GetVarParam("result_var")->m_VarIndex;
	return m_ParamAdmin.GetCondParam("elig_cond")->Assess(m_ParamAdmin.GetVarParam("output_var")->m_VarIndex, val, m_ActTU, m_ActHHVar, -1, ResVarIndex);
}

bool CEMCM_Elig::SetOutputVars()
{
	if(!m_ParamAdmin.IsParamDefined("who_must_be_elig"))
		return 1; //the following code is only relevant if the Elig function is used with the parameter who_must_be_elig

	//Elig sets output- and result-variable in DoModCalc and usually just does nothing on SetOutputVar (overwrites the base function's actions by return 1)
	//however DoModCalc operates only on eligible units, thus if the parameter who_must_be_elig is used, output- and result-variable of non-eligible units just keep their value (VOID or a value set before)
	//to achieve standard behaviour set output- and result-variable of non-eligible units here (there may be more elegant solutions, but this doesn't jeopardise the usual behaviour of Elig)
	//also see ETM - Bug #48 in redmine
	int Elig;
	if(!GetTUElig(Elig)) return 0;
	if(Elig) return 1; //for eligible units DoModCalc did the work
		
	int OutVarIndex = m_ParamAdmin.GetVarParam("output_var")->m_VarIndex;
	int ResVarIndex = m_ParamAdmin.IsParamDefined("result_var") ? m_ParamAdmin.GetVarParam("result_var")->m_VarIndex : -1;
	for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
	{
		m_ActTU->OverwriteVarVal(0, OutVarIndex, m_ActHHVar, it->m_Index);
		if (ResVarIndex!=-1) m_ActTU->OverwriteVarVal(0, ResVarIndex, m_ActHHVar, it->m_Index);
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_BenCalc
********************************************************************************************/
bool CEMCM_BenCalc::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int len=int(ParName.length());
	if(!ParName.substr(0,4).compare("base")) ParType=PTYPE_FORMULA;
	else if(!ParName.substr(0,13).compare("withdraw_base")) ParType=PTYPE_FORMULA;
	else if(!ParName.compare("withdraw_rate")) ParType=PTYPE_FORMULA;
	else if(!ParName.compare("withdraw_start")) ParType=PTYPE_FORMULA;
	else if(!ParName.compare("withdraw_end")) ParType=PTYPE_FORMULA;
	else if(!ParName.substr(0,12).compare("replace_base_var")) ParType=PTYPE_FORMULA;
	else if(!ParName.compare("replace_rate")) ParType=PTYPE_FORMULA;
	else if(!ParName.compare("ncomp")) return 1; //not used anymore
	else if(!ParName.substr(0,4).compare("comp"))
	{
		size_t i=ParName.find("_");
		if(i!=std::string::npos)
		{
			if(Group.empty()) Group=ParName.substr(4,i-4);
			Single=0;
			if(ParName.substr(std::max(len-5,0))=="_cond") { ParName = "comp_cond"; ParType=PTYPE_COND; }
			else if(ParName.substr(std::max(len-8,0))=="_perelig") { ParName = "comp_perelig"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-6,0))=="_pertu") { ParName = "comp_pertu"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-7,0))=="_lowlim" || ParName.substr(std::max(len-14,0))=="_lowlim_amount" || ParName.substr(std::max(len-10,0))=="_lowlim_il" || ParName.substr(std::max(len-11,0))=="_lowlim_var") { ParName = "comp_lowlim"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-6,0))=="_uplim" || ParName.substr(std::max(len-13,0))=="_uplim_amount" || ParName.substr(std::max(len-9,0))=="_uplim_il" || ParName.substr(std::max(len-10,0))=="_uplim_var") { ParName = "comp_uplim"; ParType=PTYPE_FORMULA; }
		}
	}
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_BenCalc::CheckParam()
{
	if(!CEMModule::CheckParam()) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off

	//add not defined optional parameters (with default values)
	if(!m_ParamAdmin.CheckOptParamExists("base", PTYPE_FORMULA, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("withdraw_base", PTYPE_FORMULA, this, "0")) return 0;
	if(m_ParamAdmin.IsParamDefined("withdraw_rate") && m_ParamAdmin.IsParamDefined("withdraw_end"))
	{ if(!CEMError::CritErr("Double definition of withdrawal advance.", this, "withdraw_rate/withdraw_end", m_ParamAdmin.GetValParam("withdraw_rate")->m_strValue+"/"+m_ParamAdmin.GetValParam("withdraw_end")->m_strValue)) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("withdraw_rate", PTYPE_FORMULA, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("withdraw_start", PTYPE_FORMULA, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("withdraw_end", PTYPE_FORMULA, this, POSINFINITE)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("replace_base", PTYPE_FORMULA, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("replace_rate", PTYPE_FORMULA, this, "0")) return 0;

	std::map<int,Param_t> compParam;
	m_ParamAdmin.GetGroupParam(compParam, "comp_", 0, 1, 1);
	for (std::map<int,Param_t>::iterator itM = compParam.begin(); itM != compParam.end(); ++itM)
	{
		CEMBenComp Comp; Comp.m_Cond = NULL; Comp.m_Formula = Comp.m_LowLim = Comp.m_UpLim = NULL;
		CEMParam *DoubleDef = NULL;
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == "comp_cond")
			{
				if(Comp.m_Cond == NULL)
					Comp.m_Cond = (CEMCondParam *)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "comp_perelig")
			{
				Comp.m_perElig = 1;
				if (Comp.m_Formula == NULL)
					Comp.m_Formula = (CEMFormulaParam *)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "comp_pertu")
			{
				Comp.m_perElig = 0;
				if (Comp.m_Formula == NULL)
					Comp.m_Formula = (CEMFormulaParam *)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if((*itP)->m_Name == "comp_lowlim")
			{
				if (Comp.m_LowLim == NULL)
					Comp.m_LowLim = (CEMFormulaParam *)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "comp_uplim")
			{
				if (Comp.m_UpLim == NULL)
					Comp.m_UpLim = (CEMFormulaParam *)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else
			{
				if (!CEMError::NonCritErr("Unknown parameter for component group " + (*itP)->m_Group + ".",
					(*itP), "Parameter is ignored."))
					return 0;
			}
		}

		if(DoubleDef)
		{
			if (!CEMError::NonCritErr("Multiple definition for component group " + DoubleDef->m_Group + ".",
				DoubleDef, "Component " + DoubleDef->m_Group + " is ignored."))
				return 0;
			continue;
		}

		if (!Comp.m_Formula)
		{
			if (!CEMError::NonCritErr("Missing definition for amount ('Comp_perElig'/'Comp_perTU') of component group "
				+ CEMUtilities::IntToStr(itM->first) + ".", this, "", "", "Component " + CEMUtilities::IntToStr(itM->first) + " is ignored."))
				return 0;
			continue;
		}

		if (!Comp.m_Cond)
		{
			if (!CEMError::NonCritErr("Missing definition for condition ('Comp_Cond') of component group "
				+ CEMUtilities::IntToStr(itM->first) + ".", this, "", "", "Component " + CEMUtilities::IntToStr(itM->first) + " is ignored."))
				return 0;
			continue;
		}

		m_Comps.push_back(Comp);
	}

	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_BenCalc::DoModCalc()
{
	m_Result = 0.0;
	for(size_t i=0; i<m_Comps.size(); ++i)
	{
		double Elig;
		if(!m_Comps[i].m_Cond->Assess(Elig, m_ActTU, m_ActHHVar)) return 0;
		if(Elig)
		{
			double Comp=0;
			if(!m_Comps[i].m_Formula->Assess(Comp, m_ActTU, m_ActHHVar)) return 0;
			if(!ApplyLimParam(Comp, m_Comps[i].m_LowLim, m_Comps[i].m_UpLim)) return 0;
			if(m_Comps[i].m_perElig) m_Result += Comp*Elig;
			else m_Result += Comp;
		}
	}

	//THIS IS NOT DESCRIBED IN THE DOCUMENTATION, BECAUSE I'M NOT SURE WHAT IT IS FOR !!!
	double RpBase=0, RpRate=0;
	if(!m_ParamAdmin.GetFormulaParam("replace_base")->Assess(RpBase, m_ActTU, m_ActHHVar)) return 0;
	if(!m_ParamAdmin.GetFormulaParam("replace_rate")->Assess(RpRate, m_ActTU, m_ActHHVar)) return 0;
	m_Result += RpBase * RpRate;

	if(!m_ParamAdmin.IsParamDefined("withdraw_base")) return 1;
	double WdBase=0, WdStart=0, WdEnd=0, WdRate=0;
	if(!m_ParamAdmin.GetFormulaParam("withdraw_base")->Assess(WdBase, m_ActTU, m_ActHHVar)) return 0;
	if(!m_ParamAdmin.GetFormulaParam("withdraw_rate")->Assess(WdRate, m_ActTU, m_ActHHVar)) return 0;
	if(!m_ParamAdmin.GetFormulaParam("withdraw_start")->Assess(WdStart, m_ActTU, m_ActHHVar)) return 0;
	if(!m_ParamAdmin.GetFormulaParam("withdraw_end")->Assess(WdEnd, m_ActTU, m_ActHHVar)) return 0;
	if(WdStart>=WdEnd)
	{
		if(WdStart==WdEnd && WdEnd==0) return CEMError::CritErr("'withdraw_start' needs to be smaller than 'withdraw_end' (hint: use n/a instead of zero to indicate that a parameter should not be used).", this, "withdraw_start/withdraw_end", CEMUtilities::DoubleToStr(WdStart)+"/"+CEMUtilities::DoubleToStr(WdEnd));
		return CEMError::CritErr("'withdraw_start' needs to be smaller than 'withdraw_end'.", this, "withdraw_start/withdraw_end", CEMUtilities::DoubleToStr(WdStart)+"/"+CEMUtilities::DoubleToStr(WdEnd));
	}
	if(WdBase<=WdStart) return 1;
	if(WdBase>=WdEnd) { m_Result=0; return 1; }
	if(!WdRate) WdRate = m_Result/(WdEnd-WdStart);
	m_Result -= (WdBase-WdStart)*WdRate;
	if(m_Result<0) m_Result=0;
	return 1;
}

/********************************************************************************************
 functions class CEMCM_SchedCalc
********************************************************************************************/
bool CEMCM_SchedCalc::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int len=int(ParName.length());
	if(!ParName.substr(0,4).compare("base")) { Compulsory=1; ParType=PTYPE_FORMULA; }
	else if(!ParName.compare("nbands")) return 1; //not used anymore
	else if(!ParName.substr(0,4).compare("band"))
	{
		size_t i=ParName.find("_");
		if(i!=std::string::npos)
		{
			if(Group.empty()) Group=ParName.substr(4,i-4);
			Single=0;
			if(ParName.substr(std::max(len-7,0))=="_lowlim") { ParName = "band_lowlim"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-6,0))=="_uplim") { ParName = "band_uplim"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-5,0))=="_rate") { ParName = "band_rate"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-7,0))=="_amount") { ParName = "band_amt"; ParType=PTYPE_FORMULA; }
			else if(ParName.substr(std::max(len-6,0))=="_order") { ParName = "band_order"; ParType=PTYPE_VAL; }
		}
	}
	else if(!ParName.compare("do_average_rates")) { ParName="simple_prog"; ParType=PTYPE_YESNO; }
	else if(!ParName.compare("simple_prog")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("round_base")) ParType=PTYPE_VAL;
	if(!ParName.substr(0,13).compare("basethreshold")) ParType=PTYPE_FORMULA;
	if(!ParName.substr(0,8).compare("quotient")) ParType=PTYPE_FORMULA;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_SchedCalc::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	m_ParamAdmin.RemoveNA(); //remove parameters set to n/a

	//check if common parameters are defined
	if(!CEMModule::CheckParam()) return 0;

	//check if compulsory parameters are defined
	if(!m_ParamAdmin.CheckCompParamExists("base", PTYPE_FORMULA, this, "0")) return 0;
	
	//add not defined optional parameters (with default values)
	if(!m_ParamAdmin.CheckOptParamExists("simple_prog", PTYPE_YESNO, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("round_base", PTYPE_VAL, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("basethreshold", PTYPE_FORMULA, this, NEGINFINITE_BR)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("quotient", PTYPE_FORMULA, this, "1")) return 0;

	//loop over bands
	m_Bands.empty();
	std::map<int,Param_t> bandParam;
	m_ParamAdmin.GetGroupParam(bandParam, "band_");
	for (std::map<int,Param_t>::iterator itM = bandParam.begin(); itM != bandParam.end(); ++itM)
	{		
		CEMParam *DoubleDef = NULL;
		CEMBand Band; Band.m_Rate = Band.m_Amount = Band.m_LowLim = Band.m_UpLim = NULL; Band.m_Order = -1;
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == "band_rate")
			{
				if (!Band.m_Amount && !Band.m_Rate)
					Band.m_Rate = (CEMFormulaParam*)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "band_amt")
			{
				if (!Band.m_Amount && !Band.m_Rate)
					Band.m_Amount = (CEMFormulaParam*)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "band_lowlim")
			{
				if (!Band.m_LowLim)
					Band.m_LowLim = (CEMFormulaParam*)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "band_uplim")
			{
				if (!Band.m_UpLim)
					Band.m_UpLim = (CEMFormulaParam*)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "band_order")
			{
				if (Band.m_Order == -1)
					Band.m_Order = int(((CEMValParam*)(*itP))->m_numValue);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else
			{
				if (!CEMError::CritErr("Unknown parameter for band " + (*itP)->m_Group + ".", (*itP)))
					return 0;
			}
		}

		if (DoubleDef)
		{
			if (!CEMError::CritErr("Multiple definition for band " + DoubleDef->m_Group + ".", DoubleDef))
				return 0;
			continue;
		}

		Band.m_Group = CEMUtilities::IntToStr(itM->first);
		Band.m_Order = itM->first;

		if (!Band.m_Amount && !Band.m_Rate)
		{
			if (!CEMError::CritErr("Missing rate/amount ('Band_Rate'/'Band_Amount') for band " + Band.m_Group + ".", this))
				return 0;
			continue;
		}

		m_Bands.push_back(Band);
	}
	
	//check band limits
	for(size_t b=0; b<m_Bands.size(); ++b)
	{
		if(b==0)
		{//lower limit of first band is optional with default value 0
			if(!m_Bands[b].m_LowLim)
			{
				if(!m_ParamAdmin.CheckOptParamExists("band_lowlim", PTYPE_FORMULA, this, "0", 0, 0, m_Bands[b].m_Group)) return 0;
				m_Bands[b].m_LowLim = m_ParamAdmin.GetFormulaParam("band_lowlim", m_Bands[b].m_Group);
			}
		}
		else
		{
			if(!m_Bands[b].m_LowLim) m_Bands[b].m_LowLim = m_Bands[b-1].m_UpLim;
		}
		if(b==m_Bands.size()-1)
		{//upper limit of last band is optional with default value infinite
			if(!m_Bands[b].m_UpLim)
			{
				if(!m_ParamAdmin.CheckOptParamExists("band_uplim", PTYPE_FORMULA, this, POSINFINITE, 0, 0, m_Bands[b].m_Group)) return 0;
				m_Bands[b].m_UpLim = m_ParamAdmin.GetFormulaParam("band_uplim", m_Bands[b].m_Group);
			}
		}
		if(m_Bands[b].m_UpLim && (m_Bands.size()>b+1 && m_Bands[b+1].m_LowLim))
		{
			if(CEMError::CritErr("Double definition of upper limit as 'Band_UpLim "+m_Bands[b].m_Group+"' and 'Band_LowLim "+m_Bands[b+1].m_Group+"'.", this)) return 0;
		}
		if(!m_Bands[b].m_UpLim && (m_Bands.size()>b+1 && !m_Bands[b+1].m_LowLim))
		{
			if(!CEMError::CritErr("Band "+m_Bands[b].m_Group+": insufficient definition of upper limit. Use parameters 'band_uplim' or 'band_lowlim'(+1).", this)) return 0;
		}
		if(!m_Bands[b].m_UpLim && m_Bands.size()>b+1) m_Bands[b].m_UpLim = m_Bands[b+1].m_LowLim;
	}
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_SchedCalc::DoModCalc()
{
	//assess value of base amount
	double Base = 0;
	if(!m_ParamAdmin.GetFormulaParam("base")->Assess(Base, m_ActTU, m_ActHHVar)) return 0;
	//assess value of possible threshold for base
	double Thresh = NUMNEGINFINITE;
	if(!m_ParamAdmin.GetFormulaParam("basethreshold")->Assess(Thresh, m_ActTU, m_ActHHVar, -1, NUMNEGINFINITE)) return 0;
	//assess value of possible quotient
	double Quotient = 1;
	if(!m_ParamAdmin.GetFormulaParam("quotient")->Assess(Quotient, m_ActTU, m_ActHHVar, -1, 1)) return 0;;
	//apply quotient on base (if defined)
	if (!Quotient)
	{
		if(!CEMError::NonCritErr("Illegal setting of quotient to zero (would cause a division by zero).", m_ParamAdmin.GetFormulaParam("quotient"), "Quotient is set to 1.")) return 0;
		Quotient=1;
	}	
	Base /= Quotient;
	//round base (if defined)
	CEMValParam *pRd = m_ParamAdmin.GetValParam("round_base");
	if(pRd->m_strValue!="n/a") Base=CEMUtilities::Round(Base, pRd->m_numValue);
	//apply base threshold (if defined)
	if(Base<Thresh) Base=Thresh;
	// loop over all tax bands
	for(std::vector<CEMBand>::iterator it=m_Bands.begin(); it!=m_Bands.end(); it++)
	{
		double lowlim, uplim, rate_amt;
		if(!it->m_LowLim->Assess(lowlim, m_ActTU, m_ActHHVar)) return 0;;
		if(!it->m_UpLim->Assess(uplim, m_ActTU, m_ActHHVar)) return 0;
		if(lowlim>uplim)
		{
			std::string spid = CEMUtilities::DoubleToStr(m_ActTU->GetVarVal(m_Control->m_ivPID, m_ActHHVar, m_ActTU->m_HeadIndex));
			std::string slow = CEMUtilities::DoubleToStr(lowlim);
			std::string sup = CEMUtilities::DoubleToStr(uplim);
			if(!CEMError::NonCritErr("Invalid band for unit with head-id "+spid+": lower limit ("+slow+") is higher than upper limit ("+sup+").", this, "", "", "Result is set to zero.")) return 0; continue;
		}
		if(it->m_Rate) { if(!it->m_Rate->Assess(rate_amt, m_ActTU, m_ActHHVar)) return 0; }
		else { if(!it->m_Amount->Assess(rate_amt, m_ActTU, m_ActHHVar)) return 0; }
		if(Base<lowlim) break;
		if(it->m_Rate)
		{
			if(m_ParamAdmin.GetYesNoParam("simple_prog")->m_numValue) m_Result=Base*rate_amt;
			else m_Result+=(std::min(uplim, Base)-lowlim)*rate_amt;
		}
		else
		{
			if(m_ParamAdmin.GetYesNoParam("simple_prog")->m_numValue) m_Result=rate_amt;
			else m_Result+=rate_amt;
		}
	}
	//apply quotient on result (if defined)
	m_Result*=Quotient;
	return 1;
}

/********************************************************************************************
 functions class CEMCM_MiniMaxi, CEMCM_Min, CEMCM_Max
********************************************************************************************/
bool CEMCM_MiniMaxi::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("nvalues")) return 1; //not used anymore
	if(!ParName.substr(0,3).compare("val")) { ParType=PTYPE_FORMULA; Single=0; ParName="val"; }
	if(!ParName.compare("positive_only")) ParType=PTYPE_YESNO;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_MiniMaxi::CheckParam()
{
	//check if common parameters are defined
	if(!CEMModule::CheckParam()) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	// loop over values
	m_ParamAdmin.GetNotSingleParams("val", m_Vals);
	if(!m_ParamAdmin.CheckOptParamExists("positive_only", PTYPE_YESNO, this, "0")) return 0;
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_MiniMaxi::DoModCalc(bool Min)
{
	m_Result = Min ? NUMPOSINFINITE : NUMNEGINFINITE;
	double positive_only;
	if(!m_ParamAdmin.GetYesNoParam("positive_only")->Assess(positive_only)) return 0;
	for(size_t i=0; i<m_Vals.size(); ++i)
	{
		double Val = Min ? NUMPOSINFINITE : NUMNEGINFINITE;
		if(!m_Vals[i]->Assess(Val, m_ActTU, m_ActHHVar, -1, Val)) return 0;
		if (!Min && Val > m_Result) m_Result = Val;
		if (Min && Val < m_Result && (!positive_only || Val>0)) m_Result = Val;
	}
	if (positive_only && m_Result==NUMPOSINFINITE) m_Result=0; //to make sure that min(0,0,0) does not lead to 999999999.99
	return 1;
}

bool CEMCM_Min::DoModCalc()
{
	return CEMCM_MiniMaxi::DoModCalc(1);
}

bool CEMCM_Max::DoModCalc()
{
	return CEMCM_MiniMaxi::DoModCalc(0);
}

/********************************************************************************************
 functions class CEMCM_Allocate
********************************************************************************************/
bool CEMCM_Allocate::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.substr(0,10).compare("share_prop")) ParType=PTYPE_VARIL;
	else if(!ParName.compare("share_equ_ifzero")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("share_between")) ParType=PTYPE_COND;
	else if(!ParName.compare("share_all_ifnoelig")) ParType=PTYPE_YESNO;
	else if(!ParName.substr(0,5).compare("share")) { ParType=PTYPE_FORMULA; Compulsory=1; }
	else if(!ParName.compare("ignore_neg_prop")) ParType=PTYPE_YESNO;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Allocate::CheckParam()
{
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	//check if common parameters are defined
	if(!CEMModule::CheckParam()) return 0;
	//check if compulsory parameter share is defined
	if(!m_ParamAdmin.CheckCompParamExists("share", PTYPE_FORMULA, this, "0")) return 0;
	//check optional parameters
	if(m_ParamAdmin.IsParamDefined("share_between")) m_ShareBetween=m_ParamAdmin.GetCondParam("share_between");
	else m_ShareBetween=NULL;
	if(!m_ParamAdmin.CheckOptParamExists("share_all_ifnoelig", PTYPE_YESNO, this, "1")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("share_equ_ifzero", PTYPE_YESNO, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("ignore_neg_prop", PTYPE_YESNO, this, "0")) return 0;
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_Allocate::DoModCalc()
{
	//assess value of amount to share
	double ShareVal=0, EligCnt=m_ActTU->m_AdultCnt+m_ActTU->m_ChildCnt ;
	if(!m_ParamAdmin.GetFormulaParam("share")->Assess(ShareVal, m_ActTU, m_ActHHVar)) return 0;

	//assess other parameters
	double ShareAllIfNoElig, ShareEqIfZero, IngnoreNegProp;
	if(!m_ParamAdmin.GetYesNoParam("share_all_ifnoelig")->Assess(ShareAllIfNoElig)) return 0;
	if(!m_ParamAdmin.GetYesNoParam("share_equ_ifzero")->Assess(ShareEqIfZero)) return 0;
	bool ShareProp = m_ParamAdmin.IsParamDefined("share_prop");
	if(!m_ParamAdmin.GetYesNoParam("ignore_neg_prop")->Assess(IngnoreNegProp)) return 0;
	
	//evaluate condition on persons to share between
	if(m_ShareBetween)
	{
		if(!m_ShareBetween->Assess(m_Control->m_ivInternal1, EligCnt, m_ActTU, m_ActHHVar)) return 0;
		//handle the case of no eligible persons
		if(EligCnt==0)
		{
			if(ShareAllIfNoElig)
			{
				for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
				{
					m_ActTU->OverwriteVarVal(1, m_Control->m_ivInternal1, m_ActHHVar, it->m_Index);
					++EligCnt;
				}
			}
			else ShareProp=0;
		}
	}
	//handle proportional sharing
	double SumProp=0;
	if(ShareProp)
	{
		for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
		{
			if(!m_ShareBetween || m_ActTU->GetVarVal(m_Control->m_ivInternal1, m_ActHHVar, it->m_Index))
			{//add income proportions are based on for all eligible unit members
				double Prop = 0;
				std::string VoidVar = "";
				if(!m_ParamAdmin.GetVarILParam("share_prop")->Assess(Prop, m_ActTU, m_ActHHVar, it->m_Index, &VoidVar)) return 0;
				if(!VoidVar.empty())
					{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+m_ParamAdmin.GetVarILParam("share_prop")->m_strValue+"'.", m_ParamAdmin.GetVarILParam("share_prop"), "Zero is used as default value for the not defined variables.")) return 0; }
				if(!IngnoreNegProp || Prop>0) SumProp+=Prop;
			}		
		}
		//handle the case if proportion var/il is zero for all "eligible" persons, but only if there is something to share
		if(SumProp==0)
		{
			if(ShareVal==0 || ShareEqIfZero) ShareProp=0;
			else
			{
				std::string spid = CEMUtilities::DoubleToStr(m_ActTU->GetVarVal(m_Control->m_ivPID, m_ActHHVar, m_ActTU->m_HeadIndex));
				if(!CEMError::NonCritErr("Unit with head-id "+spid+": no unit member receiving a share has income as defined by parameter 'share_prop_x'.", m_ParamAdmin.GetVarILParam("share_prop"), "Equal sharing is applied.")) return 0;
				ShareProp=0;
			}
		}
	}
	
	//assign shares to persons
	double sumResult=0;
	for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
	{
		m_Result=0;
		if(!m_ShareBetween || m_ActTU->GetVarVal(m_Control->m_ivInternal1, m_ActHHVar, it->m_Index))
		{
			if(ShareProp)
			{
				double prop=0; std::string VoidVar="";
				if(!m_ParamAdmin.GetVarILParam("share_prop")->Assess(prop, m_ActTU, m_ActHHVar, it->m_Index, &VoidVar)) return 0;
				if(IngnoreNegProp && prop<0) prop=0;
				m_Result = (ShareVal/SumProp)*prop;
			}
			else { if(!EligCnt) m_Result=0; else m_Result = ShareVal / EligCnt; }
		}
		if(m_ParamAdmin.GetVarParam("result_var")->m_strValue!="n/a") m_ActTU->OverwriteVarVal(m_Result, m_ParamAdmin.GetVarParam("result_var")->m_VarIndex, m_ActHHVar, it->m_Index);
		if(m_ParamAdmin.IsParamDefined("output_add_var")) m_ActTU->AddToVarVal(m_Result, m_ParamAdmin.GetVarParam("output_add_var"), m_ActHHVar, it->m_Index);
		else m_ActTU->OverwriteVarVal(m_Result, m_ParamAdmin.GetVarParam("output_var")->m_VarIndex, m_ActHHVar, it->m_Index);
		sumResult+=m_Result;
	}
	//check if value of original benefit/tax corresponds to value of shared benefit/tax
	if((EligCnt==0 && !ShareAllIfNoElig) || IngnoreNegProp) return 1; //unless output variable is set to zero because nobody is eligible for sharing or negative proportional sharing is outruled
	if((sumResult-ShareVal)>1 ||(sumResult-ShareVal)<(-1))
	{
		char err[500];
		double hh = m_ActTU->GetVarVal(m_Control->m_ivHHID, m_ActHHVar, m_ActTU->m_HeadIndex);
		EM_SPRINTF(err, "Household %f: sum of shares (%.2f) does not correspond with original value of amount to share (%.2f).", hh, sumResult, ShareVal);
		if(!CEMError::NonCritErr(err, this)) return 0;
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_Allocate_F210 (!!! PHASE-OUT !!!)
********************************************************************************************/
bool CEMCM_Allocate_F210::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.substr(0,10).compare("share_prop")) ParType=PTYPE_VARIL;
	else if(!ParName.compare("share_equ_ifzero")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("adults_only")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("share_between")) ParType=PTYPE_COND;
	else if(!ParName.compare("share_all_ifnoelig")) ParType=PTYPE_YESNO;
	else if(!ParName.substr(0,5).compare("share")) { ParType=PTYPE_FORMULA; Compulsory=1; }
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Allocate_F210::CheckParam()
{
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	//check optional parameter output_var, to eventually produce one set to share_var (necessary for the common parameter check, which expects an output variable)
	if(!m_ParamAdmin.IsParamDefined("output_var") && !m_ParamAdmin.IsParamDefined("output_add_var") && m_ParamAdmin.IsParamDefined("share"))
	{
		int ind; //share needs to refer to a variable (not e.g. to a formula) to serve as output variable as well
		if(m_Control->GetVarIndex(m_ParamAdmin.GetFormulaParam("share")->m_strValue, ind))
			{ if(!m_ParamAdmin.CheckOptParamExists("output_var", PTYPE_VAR, this, m_ParamAdmin.GetFormulaParam("share")->m_strValue)) return 0; }
	}
	//check if common parameters are defined
	if(!CEMModule::CheckParam()) return 0;
	//check if compulsory parameter share is defined
	if(!m_ParamAdmin.CheckCompParamExists("share", PTYPE_FORMULA, this, "0")) return 0;
	//check optional parameters
	if(!m_ParamAdmin.CheckOptParamExists("adults_only", PTYPE_YESNO, this, "1")) return 0;
	if(m_ParamAdmin.IsParamDefined("share_between")) m_ShareBetween=m_ParamAdmin.GetCondParam("share_between");
	else m_ShareBetween=NULL;
	if(!m_ParamAdmin.CheckOptParamExists("share_all_ifnoelig", PTYPE_YESNO, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("share_equ_ifzero", PTYPE_YESNO, this, "0")) return 0;

	if(!m_ParamAdmin.CheckFootnoteParUsage()) return 0;
	return CEMError::NonCritErr("func_allocate_F210 has phase-out status. Please consider using func_allocate instead.", this);
}

bool CEMCM_Allocate_F210::DoModCalc()
{
	//assess value of amount to share
	double ShareVal=0, AdultsOnly=1, EligCnt=m_ActTU->m_AdultCnt;
	if(!m_ParamAdmin.GetFormulaParam("share")->Assess(ShareVal, m_ActTU, m_ActHHVar)) return 0;

	//assess wether sharing takes place only amongst adults
	if(!m_ParamAdmin.GetYesNoParam("adults_only")->Assess(AdultsOnly)) return 0;
	if(!AdultsOnly || !EligCnt) { EligCnt+=m_ActTU->m_ChildCnt; AdultsOnly=0; }

	//assess other parameters
	double ShareAllIfNoElig, ShareEqIfZero;
	if(!m_ParamAdmin.GetYesNoParam("share_all_ifnoelig")->Assess(ShareAllIfNoElig)) return 0;
	if(!m_ParamAdmin.GetYesNoParam("share_equ_ifzero")->Assess(ShareEqIfZero)) return 0;
	
	//evaluate condtion on persons to share between
	if(m_ShareBetween)
	{
		if(AdultsOnly)
		{
			if(m_ShareBetween->m_strValue.find(")&!{isdepchild}")==std::string::npos)
			{//do prepare only once (not necessary to do it for each unit and takes very long)
				m_ShareBetween->m_strValue="("+m_ShareBetween->m_strValue+")&!{isdepchild}";
				if(!m_ShareBetween->Prepare()) return 0;
			}
		}
		if(!m_ShareBetween->Assess(m_Control->m_ivInternal1, EligCnt, m_ActTU, m_ActHHVar)) return 0;
		//handle the case of no eligible persons
		if(EligCnt==0)
		{
			if (!ShareAllIfNoElig)
			{
				double hh = m_ActTU->GetVarVal(m_Control->m_ivHHID, m_ActHHVar, m_ActTU->m_HeadIndex);
				if(!CEMError::NonCritErr("Household "+CEMUtilities::DoubleToStr(hh)+": no member of the assessment unit is 'eligible' to receive a share.", m_ParamAdmin.GetGeneralParam("share_between"), "Amount is equally shared among (adults, if adults_only=yes) members of assessment unit.")) return 0;
			}
			//set all (adult) taxunit members "eligible"
			EligCnt=0;
			for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
				if(!AdultsOnly || !it->m_IsDepChild)
				{
					m_ActTU->OverwriteVarVal(1, m_Control->m_ivInternal1, m_ActHHVar, it->m_Index);
					++EligCnt;
				}
			// this can happen if there are only children in the taxunit - set head "eligible" in this case
			if(EligCnt==0)
			{
				m_ActTU->OverwriteVarVal(1, m_Control->m_ivInternal1, m_ActHHVar, m_ActTU->m_HeadIndex);
				EligCnt=1;
			}
		}
	}
	//handle proportional sharing
	double SumProp=0;
	bool ShareProp = m_ParamAdmin.IsParamDefined("share_prop");
	if(ShareProp)
	{
		for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
		{
			if((!m_ShareBetween || m_ActTU->GetVarVal(m_Control->m_ivInternal1, m_ActHHVar, it->m_Index)) &&
				(!AdultsOnly || !it->m_IsDepChild))
			{//add income proportions are based on for all eligible unit members
				double Prop = 0; std::string VoidVar = "";
				if(!m_ParamAdmin.GetVarILParam("share_prop")->Assess(Prop, m_ActTU, m_ActHHVar, it->m_Index, &VoidVar)) return 0;
				if(!VoidVar.empty())
					{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+m_ParamAdmin.GetVarILParam("share_prop")->m_strValue+"'.", m_ParamAdmin.GetVarILParam("share_prop"), "Zero is used as default value for the not defined variables.")) return 0; }
				SumProp+=Prop;
			}		
		}
		//handle the case if proportion var/il is zero for all "eligible" persons, but only if there is something to share
		if(SumProp==0)
		{
			if(ShareVal==0 || ShareEqIfZero) ShareProp=0;
			else
			{
				if(!CEMError::NonCritErr("No unit member receiving a share has income as defined by parameter 'share_prop_x'.", "Equal sharing is applied.")) return 0;
				ShareProp=0;
			}
		}
	}
	
	//assign shares to persons
	double sumResult=0;
	for(PersonRefContainer_t::iterator it=m_ActTU->m_PersonRefs.begin(); it!=m_ActTU->m_PersonRefs.end(); it++)
	{
		m_Result=0;
		if((!m_ShareBetween || m_ActTU->GetVarVal(m_Control->m_ivInternal1, m_ActHHVar, it->m_Index)) &&
			(!AdultsOnly || !it->m_IsDepChild))
		{
			if(ShareProp)
			{
				double prop=0; std::string VoidVar="";
				if(!m_ParamAdmin.GetVarILParam("share_prop")->Assess(prop, m_ActTU, m_ActHHVar, it->m_Index, &VoidVar)) return 0;
				m_Result = (ShareVal/SumProp)*prop;
			}
			else m_Result = ShareVal / EligCnt;
		}
		if(m_ParamAdmin.GetVarParam("result_var")->m_strValue!="n/a") m_ActTU->OverwriteVarVal(m_Result, m_ParamAdmin.GetVarParam("result_var")->m_VarIndex, m_ActHHVar, it->m_Index);
		if(m_ParamAdmin.IsParamDefined("output_add_var")) m_ActTU->AddToVarVal(m_Result, m_ParamAdmin.GetVarParam("output_add_var"), m_ActHHVar, it->m_Index);
		else m_ActTU->OverwriteVarVal(m_Result, m_ParamAdmin.GetVarParam("output_var")->m_VarIndex, m_ActHHVar, it->m_Index);
		sumResult+=m_Result;
	}
	if((sumResult-ShareVal)>1 ||(sumResult-ShareVal)<(-1))
	{
		char err[500];
		double hh = m_ActTU->GetVarVal(m_Control->m_ivHHID, m_ActHHVar, m_ActTU->m_HeadIndex);
		EM_SPRINTF(err, "Household %f: sum of shares (%.2f) does not correspond with original value of amount to share (%.2f).", hh, sumResult, ShareVal);
		if(!CEMError::NonCritErr(err, this)) return 0;
	}
	return 1;	
}

/********************************************************************************************
 functions class CEMCM_DefVar
********************************************************************************************/
bool CEMCM_DefVar::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int len = int(ParName.length());
	std::string vc = (m_Name == COMOD24) ? "var" : "const";
	if (ParName.length() > vc.length() && ParName.substr(0, vc.length()) == vc && ParName.substr(std::max(len-7,0)) == "_amount")
		ParName = CEMUtilities::Replace(ParName, "amount", "amt"); //otherwise parameter _amount is cut in CEMModule::TakeParam (because of old 3-type-parameters)
	len = int(ParName.length());

	if (ParName == "n" + vc)
		return 1; //nvar, nconst not used anymore

	//first try if common parameter ...
	if(ParName == "switch" || ParName == "run_cond" || ParName.substr(0,1) == "#") //permitted are switch, run_cond and footnotes for run_cond
	{
		CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
		//... if that worked can only be assessed by checking, via GetParam, if the parameter was registered (TakeParam only generates an error but returns 1, to allow for detecting further errors)
		std::string footNoteParameter = ""; //footnote parameters of run_cond (e.g. #_amount or query parameters like #_DataBaseName) are transformed ...
		if (ParName.length() > 2 && ParName.substr(0,2) == "#_") //... from #_amount (Group 4711) to #4711_amount in InsertParam ...
			footNoteParameter = "#" + Group + "_" + ParName.substr(2); //... have to this here too, to find the parameter
		if(m_ParamAdmin.GetParam(ParName, Group, 0) || m_ParamAdmin.GetParam(footNoteParameter, "", 0)) return 1;
	}
	//... all other parameters are supposed to be defintions of variables/constants

	//do not allow for # even if it is not used as first character (which already generated an error in the TakeParam above, but with the not telling error message "Unknown parameter")
	//as formulas would still interpret it as footnotes and not as part of the variable/constant name
	if(ParName.find("#") != std::string::npos) //new style: policy column contains variable/constant name (see below for old style)
		return CEMError::CritErr("Character # cannot be used for variable/constant name as it is reserved for footnotes.", this, ParName, ParVal);

	bool oldStyle = 0;
	if(ParName.substr(0, vc.length()) == vc)
	{
		size_t i = ParName.find("_");
		if(i != std::string::npos)
		{
			if (Group.empty())
				Group = ParName.substr(vc.length(), i-vc.length());
			if (Group.empty()) //happens with new UI, where there is only Var_Name, without monetary- or value-definition
				Group = CEMUtilities::IntToStr(1000 + m_nG++);
			Single=0;
			if (ParName.substr(std::max(len-5,0)) == "_name")
			{
				if(ParVal.find("#") != std::string::npos) //old style: system column contains variable/constant name (see above for new style)
					return CEMError::CritErr("Character # cannot be used for variable/constant name as it is reserved for footnotes.", this, ParName, ParVal);
				ParName = vc + "_name";
				ParType=PTYPE_NAME;
				oldStyle = 1;
			}
			else if (ParName.substr(std::max(len-9,0)) == "_monetary")
			{
				ParName = vc + "_monetary";
				ParType=PTYPE_YESNO;
				oldStyle = 1;
			}
			else if (ParName.substr(std::max(len-8,0)) == "_dataset")
			{
				ParName = vc + "_dataset";
				ParType=PTYPE_NAME;
				oldStyle = 1;
			}
			else if (ParName.substr(std::max(len-11,0)) == "_systemyear")
			{
				ParName = vc + "_systemyear";
				ParType=PTYPE_NAME;
				oldStyle = 1;
			}
			else if (ParName.substr(std::max(len-4,0))=="_amt") //actually only available for const, but doesn't harm
			{
				ParName = vc + "_amt";
				ParType=PTYPE_FORMULA;
				oldStyle = 1;
			}
		}
	}

	if (!oldStyle)
	{//assume that ParName contains the name of the variable/constant and ParVal the amount
		//first insert parameter for constant/variable name ...
		if (ParVal == "n/a")
			return 1;
		if (Group.empty())
			Group = CEMUtilities::IntToStr(1000 + m_nV);
		if (!CEMModule::TakeParam(vc + "_name", ParName, ParId, Line, Column, Group, PTYPE_NAME, 0))
			return 0;
		//... then parameter for constant/variable amount
		ParName = vc + "_amt";
		ParType = PTYPE_FORMULA;
		ParId = "op"; //to avoid double id
		++m_nV;
	}

	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_DefVar::CheckParam()
{
	if (m_ParamAdmin.GetSwitchParam()->m_numValue == 0)
		return 1; //don't check if module is switched off

	if (!m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a")) return 0; //create this for the module not necessary parameter

	//run_cond needs to be checked already here (and not as usual only in Run) because otherwise constants/variables (which should not exist) are generated and possibly even initialised (if static)
	if (m_ParamAdmin.IsParamDefined("run_cond"))
	{
		m_ParamAdmin.Prepare("run_cond");
		//only check if condition is fixed (e.g. IsUsedDatabase), otherwise e.g. totals or variables generated in loops could not be used in run_cond (just do not initialise if there is a non-static run cond - see below)
		if (m_ParamAdmin.GetCondParam("run_cond")->m_Formula->m_IsStatic)
			{ int run = EvalRunCond(); if(!run) return 0; if (run == -1) return 1; }
	}

	if (!m_ParamAdmin.Prepare())
		return 0;
	
	bool isConst = (m_Name == COMOD9);
	std::string vc = isConst ? "const" : "var";
	std::string vcLong = isConst ? "constant" : "variable";

	m_VarInd.clear(); m_VarVals.clear();

	//run over all variable/constant definitions
	std::map<int,Param_t> vcParam;
	m_ParamAdmin.GetGroupParam(vcParam, vc);
	for (std::map<int,Param_t>::iterator itM = vcParam.begin(); itM != vcParam.end(); ++itM)
	{
		CEMParam *DoubleDef = NULL;
		std::string vcName = "";
		int isMonetary = -1;
		std::string vcDataset = "";
		std::string vcSystemYear = "";
		CEMFormulaParam *vcFormula = NULL;
		//(1) gather the possible parameters for a variable/constant: name, amount, monetary, dataset, systemyear
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == vc + "_name")
			{
				if (vcName.empty())
					vcName = (*itP)->m_strValue;
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == vc + "_amt")
			{
				if ((*itP)->m_strValue == "n/a")
					continue;
				if (!vcFormula)
					vcFormula = (CEMFormulaParam*)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == vc + "_monetary")
			{
				if (isMonetary == -1)
					isMonetary = (int)((*itP)->m_numValue);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == vc + "_dataset")
			{
				if ((*itP)->m_strValue == "n/a")
					continue;
				if (vcDataset.empty())
					vcDataset = (*itP)->m_strValue;
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == vc + "_systemyear")
			{
				if ((*itP)->m_strValue == "n/a")
					continue;
				if (vcSystemYear.empty())
					vcSystemYear = (*itP)->m_strValue;
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else
			{
				if (!CEMError::NonCritErr("Unknown parameter for " + vcLong + " " + CEMUtilities::IntToStr(itM->first) + ".",
					(*itP), "Parameter is ignored."))
					return 0;
				continue;
			}
		}

		//(2) check for double or missing definitions
		if (DoubleDef)
		{
			if (!CEMError::NonCritErr("Multiple definition for " + vcLong + " " + CEMUtilities::IntToStr(itM->first) + ".",
				DoubleDef, "Definition of " + vcLong + " is ignored."))
				return 0;
			continue;
		}
		if (vcName.empty())
		{
			if (!CEMError::NonCritErr("Missing definition of name for " + vcLong + " " + CEMUtilities::IntToStr(itM->first) + ".",
				this, "Definition of " + vcLong + " is ignored."))
				return 0;
			continue;
		}
		if (vcFormula && (isConst && !vcFormula->m_IsGlobal)) //2nd condition: variables can be initialised with amounts on person basis (see below)
		{
			if (!CEMError::NonCritErr("Trying to initialise " + vcLong + " " + CEMUtilities::IntToStr(itM->first) + " with a non-constant amount.",
				this, "Initialisation is ignored."))
				return 0;
			vcFormula = NULL;
		}
		if (isMonetary == -1)
			isMonetary = 1;

		//(3) create variable/constant
		//unless the variable/constant is only generate on the condition of matching dataset or system year (i.e. is an uprating factor, which depends on dataset / system year)
		if ((!vcDataset.empty() && !CEMUtilities::DoesValueMatchPattern(vcDataset, CEMUtilities::RemoveExtension(m_Control->m_DataSet))
								&& !CEMUtilities::DoesValueMatchPattern(vcDataset, CEMUtilities::AppendExtension(m_Control->m_DataSet))) ||
			(!vcSystemYear.empty() && (m_System->m_Year != "" ? m_System->m_Year : CEMUtilities::ExtractSystemYear(m_System->m_Name)) != vcSystemYear))
			continue; 

		int vcIndex;
		if (m_Control->GetVarIndex(vcName, vcIndex) && m_Control->m_VarIndexV[vcIndex].m_GenType != VARTYPE_NONDESC)
		{//2nd condition: don't complain if existing variable seems to be also defined by func_DefVar as this happens when the constant/variable exists in several systems (quite likely); disadvantage: double definition via func_DefVar is not recognised
			if (!CEMError::CritErr("Variable/constant named '" + vcName + "' already exists.", this))
				return 0; 
			continue;
		}
		if (isConst)
		{
			if (!m_Control->GetVarIndex(vcName, vcIndex, CREATE_INTERNAL, VARTYPE_NONDESC, isMonetary, 1, 0))
				return CEMError::CodeErr("Programming error: adding constant '" + vcName + "' to variable list failed.");
		}
		else
		{
			if (!m_Control->GetVarIndex(vcName, vcIndex, CREATE_INTERNAL, VARTYPE_NONDESC, isMonetary, 0, isMonetary))
				return CEMError::CodeErr("Programming error: adding variable '"+ vcName + "' to variable list failed.");
		}

		//(4) put variables/constant into list to be used by Run for filling them with values
		if (vcFormula) //variables may have no initial value and constants can be set to n/a
		{
			m_VarInd.push_back(vcIndex);
			m_VarVals.push_back(vcFormula);
		}
	}

	//initalise constants/variables if their value is static already here, thus they can be assessed by other functions at readtime (used for allowing constants for value parameters)
	//however only initialise if there is no run_cond or the run_cond is fixed and fulfilled (the latter checked above), otherwise a constant could exist and even be initialised though the run_cond says "do not define"
	if (!m_ParamAdmin.IsParamDefined("run_cond") || m_ParamAdmin.GetCondParam("run_cond")->m_Formula->m_IsStatic)
		for (size_t i = 0; i < m_VarInd.size(); ++i)
		{
			m_ActTU = m_System->GetStaticTU();
			m_ActHHVar = &(m_ActTU->m_HH->m_Persons);
			double val;
			if (!m_VarVals[i]->m_IsStatic)
				continue;
			if (!m_VarVals[i]->Assess(val, m_ActTU, m_ActHHVar))
				return 0;

			//find out whether a variable/constant is a rate (percentage) in order to set it non-monetary and therewith avoid possible currency-converting
			//the approach is somewhat questionable but justified by the rare occurance and minimal impact (currency coverting only effects output - thus it's only for not wrongly convert outputted rates)
			int last = (int)(m_VarVals[i]->m_strValue.length()) - 1;
			if (last > 3 && m_VarVals[i]->m_strValue[last-3] == '#' && m_VarVals[i]->m_strValue[last-1] == 'r') //does the string end with #?r, taking into account that the 'formula' is put into brackets, examples: (0.12#mr) or (0.03#cr)
				m_Control->m_VarIndexV[m_VarInd[i]].m_Monetary = 0;

			for(size_t h = 0; h < m_System->m_Households.size(); ++h) //loop over households (within system)
			{
				for(size_t p = 0; p < m_System->m_Households[h].m_Persons.size(); ++p) //loop over persons within household
					m_System->m_Households[h].m_Persons[p][m_VarInd[i]] = val;
			}
		}

	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_DefVar::Run(PersonVarContainer_t *HHVar)
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	for (size_t i = 0; i < m_VarInd.size(); ++i)
	{
		if (m_Control->m_VarIndexV[m_VarInd[i]].m_Name == "$hicp") // this prevents overwriting the $HICP of the global table with a possibly different (old) value defined in the uprating-factors-policy
			continue; // note: the constant is actually filled in CheckParam to be available for uprating (thus no problem if there is no global-table-$HICP)

		for (size_t p = 0; p < HHVar->size(); ++p)
		{
			CEMTaxunit *tu = m_System->GetDummyIndividualTU((int)p);

			double val = 0.0;
			if(!m_VarVals[i]->Assess(val, tu, &(*HHVar)))
			{
				char err[500];
				double hh = tu->GetVarVal(m_Control->m_ivHHID, &(*HHVar), tu->m_HeadIndex);
				EM_SPRINTF(err, "Household %f: unable to asses parameter.", hh);
				if(!CEMError::CritErr(err, m_VarVals[i]))
					return 0;
			}
			(*HHVar)[p][m_VarInd[i]]=val;
		}
	}
	return 1;
}


/********************************************************************************************
 functions class CEMCM_DefIL
********************************************************************************************/
std::map<std::string, int> CEMCM_DefIL::m_ilNames;

void CEMCM_DefIL::Init(std::string Name, CEMSystem *System, std::string PolName, std::string Id, int Line)
{
	CEMModule::Init(Name, System, PolName, Id, Line);
	m_nEntries=0;
}

bool CEMCM_DefIL::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	char pn[100];
	//old notation: +/- (and optionally factor) in the parameter-name-column, variable name in the system-columns
	if (ParName.substr(0,1) == "+" || ParName.substr(0,1) == "-")
	{
		Compulsory = ParVal != "n/a";
		++m_nEntries;

		//in new interface factor is stored in group (for the time being undo this)
		if (!Group.empty())
		{
			ParName += Group;
			Group.clear();
		}
		//first insert parameter for entry (variable, incomelist, amount) ...
		EM_SPRINTF(pn, "entry%d", m_nEntries);
		if (!CEMModule::TakeParam(pn, ParVal, ParId, Line, Column, Group, PTYPE_VARIL, Compulsory))
			return 0;
		//... then insert parameter for factor
		ParName = CEMUtilities::Replace(ParName, " ", ""); //remove spaces
		if (ParName == "+" || ParName == "-")
			ParVal = ParName + "1"; //replace +/- by +1/-1
		else ParVal = ParName;
		EM_SPRINTF(pn, "factor%d", m_nEntries);
		ParName = pn;
		ParId = "op"; //to avoid double id
		ParType = PTYPE_VAL;
	}
	else if (ParName == "name")
	{
		ParType=PTYPE_NAME;
		Compulsory=1;
		m_ilNames.insert(std::pair<std::string, int>(ParVal, 0)); //store incomelist names just to be able to identify them as such below
	}
	else if (ParName == "warn_if_nonmonetary") ParType=PTYPE_YESNO;
	else if(!ParName.compare("run_cond")) return CEMError::NonCritErr("Parameter 'run_cond' is not allowed with function 'func_defil'.", this, ParName, ParVal, "Run condition is ignored.");
	
	//new notation: variable name in the parameter-name-column, factors in the system-columns
	else if (ParName != "switch")
	{
		Compulsory = ParVal != "n/a";
		++m_nEntries;

		//first insert parameter for factor ...
		ParVal = CEMUtilities::Replace(ParVal, " ", ""); //remove spaces
		if (ParVal == "+" || ParVal == "-")
			ParVal = ParVal + "1"; //replace +/- by +1/-1
		EM_SPRINTF(pn, "factor%d", m_nEntries);
		if (!CEMModule::TakeParam(pn, ParVal, ParId, Line, Column, Group, PTYPE_VAL, Compulsory))
			return 0;
		//... then insert parameter for entry (variable, incomelist, amount)
		ParVal = ParName;
		EM_SPRINTF(pn, "entry%d", m_nEntries);
		ParName = pn;
		ParId = "op"; //to avoid double id
		ParType = PTYPE_VARIL;
	}
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_DefIL::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	if(!m_ParamAdmin.CheckCompParamExists("name", PTYPE_NAME, this, "n/a")) return 0;
	std::string ILName = m_ParamAdmin.GetGeneralParam("name")->m_strValue;
	if (ILName.length() < 4 || (ILName.substr(0,4) != "ils_" && ILName.substr(0,3) != "il_"))
		{ if(!CEMError::NonCritErr("Incomelist names are supposed to start with 'il_' or 'ils_', which is not fulfilled by '"+ILName+"'.", m_ParamAdmin.GetGeneralParam("name"))) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("warn_if_nonmonetary", PTYPE_YESNO, this, "yes")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a")) return 0; //create this for the module not necessary parameter

	IL_t IL;
	char pn[100];
	m_ParamAdmin.GetYesNoParam("warn_if_nonmonetary")->Prepare();
	for(int i=1; i<=m_nEntries ;++i)
	{
		EM_SPRINTF(pn, "entry%d", i);
		CEMVarILParam *pEntry = m_ParamAdmin.GetVarILParam(pn);
		EM_SPRINTF(pn, "factor%d", i);
		CEMValParam *pFactor = m_ParamAdmin.GetValParam(pn);
		if (pEntry->m_strValue=="n/a" || pFactor->m_strValue=="n/a") continue;
		pEntry->Prepare();
		pFactor->Prepare();
		CEMILEntry entry;
		entry.m_Entry = pEntry;
		entry.m_Factor = pFactor->m_numValue;
		if(entry.m_Entry->m_viType == VITYPE_VAR)
			if(!m_Control->m_VarIndexV[entry.m_Entry->m_VarIndex].m_Monetary && m_ParamAdmin.GetYesNoParam("warn_if_nonmonetary")->m_numValue)
				{ if(!CEMError::NonCritErr("Non-monetary variable '"+m_Control->m_VarIndexV[entry.m_Entry->m_VarIndex].m_Name+"' is included in incomelist '"+ILName+"'.", this)) return 0; }
		IL.insert(IL.end(), entry);
	}
	return m_System->AddIL(ILName, IL, this);
}

/********************************************************************************************
 functions class CEMCM_DefTU
********************************************************************************************/
void CEMCM_DefTU::Init(std::string Name, CEMSystem *System, std::string PolName, std::string Id, int Line)
{
	CEMModule::Init(Name, System, PolName, Id, Line);
	m_IsPartnerMem=0;
	m_AreOwnChMem=0;
	m_AreOwnDepChMem=0;
	m_AreLooseDepChMem=0;
	m_AreDepParMem=0;
	m_AreDepRelMem=0;
}

bool CEMCM_DefTU::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("name")) { ParType=PTYPE_NAME; Compulsory=1; }
	else if(!ParName.compare("type")) { ParType=PTYPE_CATEG; Compulsory=1; }
	else if(!ParName.compare("members")) ParType=PTYPE_CATEG;
	else if(!ParName.compare("extheadcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("partnercond")) ParType=PTYPE_COND;
	else if(!ParName.compare("depchildcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("ownchildcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("owndepchildcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("loosedepchildcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("depparentcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("deprelativecond")) ParType=PTYPE_COND;
	else if(!ParName.compare("loneparentcond")) ParType=PTYPE_COND;
	else if(!ParName.compare("stopifnoheadfound")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("headdef_var")) { ParName="headdefinc"; ParType=PTYPE_VARIL; }
	else if(!ParName.compare("headdef_il")) { ParName="headdefinc"; ParType=PTYPE_VARIL; }
	else if(!ParName.compare("headdefinc")) ParType=PTYPE_VARIL;
	else if(!ParName.compare("nochildifhead")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("nochildifpartner")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("assigndepchofdependents")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("assignpartnerofdependents")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("run_cond")) return CEMError::NonCritErr("Parameter 'run_cond' is not allowed with function 'func_deftu'.", this, ParName, ParVal, "Run condition is ignored.");
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_DefTU::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off

	if(!m_ParamAdmin.CheckOptParamExists("extheadcond", PTYPE_COND, this, DEFAULT_EXTHEADCOND, 1)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("partnercond", PTYPE_COND, this, DEFAULT_PARTNERCOND, 1)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("depchildcond", PTYPE_COND, this, DEFAULT_NOCHILDCOND, 1)) return 0;
	int mid;
	if(!m_Control->GetVarIndex(VARNAME_MOTHERID, mid)) { if(!m_ParamAdmin.CheckOptParamExists("ownchildcond", PTYPE_COND, this, DEFAULT_OWNCHILDCOND1, 1)) return 0; }
	else { if(!m_ParamAdmin.CheckOptParamExists("ownchildcond", PTYPE_COND, this, DEFAULT_OWNCHILDCOND2, 1)) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("owndepchildcond", PTYPE_COND, this, DEFAULT_OWNDEPCHILDCOND, 1)) return 0;
	if(!m_Control->GetVarIndex(VARNAME_MOTHERID, mid)) { if(!m_ParamAdmin.CheckOptParamExists("loosedepchildcond", PTYPE_COND, this, DEFAULT_LOOSEDEPCHILDCOND1, 1)) return 0; }
	else { if(!m_ParamAdmin.CheckOptParamExists("loosedepchildcond", PTYPE_COND, this, DEFAULT_LOOSEDEPCHILDCOND2, 1)) return 0; }
	if(!m_Control->GetVarIndex(VARNAME_MOTHERID, mid)) { if(!m_ParamAdmin.CheckOptParamExists("depparentcond", PTYPE_COND, this, DEFAULT_PARENTCOND1, 1)) return 0; }
	else { if(!m_ParamAdmin.CheckOptParamExists("depparentcond", PTYPE_COND, this, DEFAULT_PARENTCOND2, 1)) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("deprelativecond", PTYPE_COND, this, DEFAULT_RELATIVECOND, 1)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("loneparentcond", PTYPE_COND, this, DEFAULT_LONEPARENTCOND, 1)) return 0;
	
	m_ExtHeadCond = m_ParamAdmin.GetCondParam("extheadcond");
	m_ExtHeadCond->m_strValue = CEMUtilities::Replace(m_ExtHeadCond->m_strValue, " ","");
	m_ExtHeadCond->m_strValue = CEMUtilities::Replace(m_ExtHeadCond->m_strValue, "{default}",DEFAULT_EXTHEADCOND);
	m_PartnerCond = m_ParamAdmin.GetCondParam("partnercond");
	m_PartnerCond->m_strValue = CEMUtilities::Replace(m_PartnerCond->m_strValue, " ","");
	m_PartnerCond->m_strValue = CEMUtilities::Replace(m_PartnerCond->m_strValue, "{default}",DEFAULT_PARTNERCOND);
	m_DepChCond = m_ParamAdmin.GetCondParam("depchildcond");
	m_DepChCond->m_strValue = CEMUtilities::Replace(m_DepChCond->m_strValue, " ","");
	m_DepChCond->m_strValue = CEMUtilities::Replace(m_DepChCond->m_strValue, "{default}",DEFAULT_DEPCHILDCOND);
	m_OwnChCond = m_ParamAdmin.GetCondParam("ownchildcond");
	m_OwnChCond->m_strValue = CEMUtilities::Replace(m_OwnChCond->m_strValue, " ","");
	if(!m_Control->GetVarIndex(VARNAME_MOTHERID, mid)) m_OwnChCond->m_strValue = CEMUtilities::Replace(m_OwnChCond->m_strValue, "{default}",DEFAULT_OWNCHILDCOND1);
	else  m_OwnChCond->m_strValue = CEMUtilities::Replace(m_OwnChCond->m_strValue, "{default}",DEFAULT_OWNCHILDCOND2);
	m_OwnDepChCond = m_ParamAdmin.GetCondParam("owndepchildcond");
	m_OwnDepChCond->m_strValue = CEMUtilities::Replace(m_OwnDepChCond->m_strValue, " ","");
	m_OwnDepChCond->m_strValue = CEMUtilities::Replace(m_OwnDepChCond->m_strValue, "{default}",DEFAULT_OWNDEPCHILDCOND);
	m_LooseDepChCond = m_ParamAdmin.GetCondParam("loosedepchildcond");
	m_LooseDepChCond->m_strValue = CEMUtilities::Replace(m_LooseDepChCond->m_strValue, " ","");
	if(!m_Control->GetVarIndex(VARNAME_MOTHERID, mid)) m_LooseDepChCond->m_strValue = CEMUtilities::Replace(m_LooseDepChCond->m_strValue, "{default}",DEFAULT_LOOSEDEPCHILDCOND1);
	else m_LooseDepChCond->m_strValue = CEMUtilities::Replace(m_LooseDepChCond->m_strValue, "{default}",DEFAULT_LOOSEDEPCHILDCOND2);
	m_DepParCond = m_ParamAdmin.GetCondParam("depparentcond");
	m_DepParCond->m_strValue = CEMUtilities::Replace(m_DepParCond->m_strValue, " ","");
	if(!m_Control->GetVarIndex(VARNAME_MOTHERID, mid)) m_DepParCond->m_strValue = CEMUtilities::Replace(m_DepParCond->m_strValue, "{default}",DEFAULT_PARENTCOND1);
	else m_DepParCond->m_strValue = CEMUtilities::Replace(m_DepParCond->m_strValue, "{default}",DEFAULT_PARENTCOND2);
	m_DepRelCond = m_ParamAdmin.GetCondParam("deprelativecond");
	m_DepRelCond->m_strValue = CEMUtilities::Replace(m_DepRelCond->m_strValue, " ","");
	m_DepRelCond->m_strValue = CEMUtilities::Replace(m_DepRelCond->m_strValue, "{default}",DEFAULT_RELATIVECOND);
	m_LoneParCond = m_ParamAdmin.GetCondParam("loneparentcond");
	m_LoneParCond->m_strValue = CEMUtilities::Replace(m_LoneParCond->m_strValue, " ","");
	m_LoneParCond->m_strValue = CEMUtilities::Replace(m_LoneParCond->m_strValue, "{default}",DEFAULT_LONEPARENTCOND);
	
	if(!m_ParamAdmin.Prepare()) return 0;
	
	if(!m_ParamAdmin.CheckCompParamExists("name", PTYPE_NAME, this, "n/a")) return 0;
	m_TUName = m_ParamAdmin.GetGeneralParam("name")->m_strValue;
	if(!m_ParamAdmin.CheckCompParamExists("type", PTYPE_CATEG, this, "ind")) return 0;
	CEMParam *pType = m_ParamAdmin.GetGeneralParam("type");
	std::string Type = pType->m_strValue;
	if(!Type.compare("hh")) m_Type=TUTYPE_HH;
	else if(!Type.compare("ind")) m_Type=TUTYPE_IND;
	else if (!Type.compare("subgroup")) m_Type=TUTYPE_SUBGROUP;
	else { if(!CEMError::CritErr("Parameter value must be set to 'HH', 'IND' or 'SUBGROUP'.", pType)) return 0; }
	if(m_Type==TUTYPE_SUBGROUP)
	{
		if(!m_ParamAdmin.CheckCompParamExists("members", PTYPE_CATEG, this, "")) return 0;
		CEMParam *pmem = m_ParamAdmin.GetGeneralParam("members");
		std::string mem = pmem->m_strValue;
		mem=CEMUtilities::Replace(mem, " ","");
		if(mem.find("head")!=std::string::npos) mem=CEMUtilities::Replace(mem, "head","");
		if(mem.find("partner")!=std::string::npos) { m_IsPartnerMem=1; mem=CEMUtilities::Replace(mem, "partner",""); }
		if(mem.find("owndepchild")!=std::string::npos) { m_AreOwnDepChMem=1; mem=CEMUtilities::Replace(mem, "owndepchild",""); }
		if(mem.find("loosedepchild")!=std::string::npos) { m_AreLooseDepChMem=1; mem=CEMUtilities::Replace(mem, "loosedepchild",""); }
		if(mem.find("ownchild")!=std::string::npos) { m_AreOwnChMem=1; mem=CEMUtilities::Replace(mem, "ownchild",""); }
		if(mem.find("depparent")!=std::string::npos) { m_AreDepParMem=1; mem=CEMUtilities::Replace(mem, "depparent",""); }
		if(mem.find("deprelative")!=std::string::npos) { m_AreDepRelMem=1; mem=CEMUtilities::Replace(mem, "deprelative",""); }
		mem=CEMUtilities::Replace(mem, "&","");
		if(!mem.empty()) { if(!CEMError::CritErr("Not interpretable text: '"+mem+"'.", pmem)) return 0; }
	}
	if(!m_ParamAdmin.IsParamDefined("headdefinc"))
		if(!m_ParamAdmin.CheckOptParamExists("headdefinc", PTYPE_VARIL, this, m_System->m_HeadDefInc)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("stopifnoheadfound", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("nochildifhead", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("nochildifpartner", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("assigndepchofdependents", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("assignpartnerofdependents", PTYPE_YESNO, this, "no")) return 0;

	if(!m_ParamAdmin.CheckFootnoteParUsage()) return 0;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_DefTU::BuildTUs(CEMHousehold *hh, PersonVarContainer_t *HHVar)
{
	m_ActHHVar = HHVar;
	CEMTaxunit TU(m_Control, hh);
	CEMPerson p;
	TUContainer_t TUC;
	TUC.clear();

	unsigned int i;
	switch(m_Type)
	{
	case TUTYPE_IND:
		//each persons of the hh forms an own taxunit
		for(i=0; i<HHVar->size(); i++)
		{
			CEMTaxunit TU(m_Control, hh);
			p.m_Index=i;
			p.m_IsHead=1;
			p.m_IsPartner=p.m_IsOwnChild=p.m_IsOwnDepChild=p.m_IsLooseDepChild=p.m_IsDepPar=p.m_IsDepRel=0;
			TU.m_PersonRefs.insert(TU.m_PersonRefs.end(), p);
			TU.m_HeadIndex=i;
			if(!AssessTUStatus(&TU, m_DepChCond, 1)) return 0; //assess child status
			if(!AssessTUStatus(&TU, m_LoneParCond, 7)) return 0; //assess lone parent status
			TUC.insert(TUC.end(), TU);
		}
		break;

	case TUTYPE_HH:
		//all persons of the hh belong to the same taxunit
		for(i=0; i<HHVar->size(); i++)
		{
			p.m_Index=i;
			TU.m_PersonRefs.insert(TU.m_PersonRefs.end(), p);
		}
		if(!AssessTUStatus(&TU, m_DepChCond, 1)) return 0; //assess child status
		if(!AssessTUStatus(&TU, m_LooseDepChCond, 8)) return 0; //assess loose dependent children
		if(!AssessHeadOfTU(&TU)) return 0; //assess head of household
		if(!AssessTUStatus(&TU, m_PartnerCond, 2)) return 0; //assess partner of head
		if(!AssessTUStatus(&TU, m_OwnChCond, 3)) return 0; //assess own children of head and partner
		if(!AssessTUStatus(&TU, m_OwnDepChCond, 4)) return 0; //assess own dependent children of head and partner
		if(!AssessTUStatus(&TU, m_DepParCond, 5)) return 0; //assess dependent parents of head and partner
		if(!AssessTUStatus(&TU, m_DepRelCond, 6)) return 0; //assess dependent relatives of head and partner
		if(!AssessTUStatus(&TU, m_LoneParCond, 7)) return 0; //assess lone parent status
		TUC.insert(TUC.end(), TU);
		break;

	case TUTYPE_SUBGROUP:
		//persons are allocated to taxunits with respect to conditions of tu-definition
		for(i=0; i<HHVar->size(); i++)
		{//initially put all household members into the same taxunit
			p.m_Index=i;
			TU.m_PersonRefs.insert(TU.m_PersonRefs.end(), p);
		}
		if(!AssessTUStatus(&TU, m_DepChCond, 1)) return 0; //assess dependent child status of all household members
		if(!AssessTUStatus(&TU, m_LooseDepChCond, 8)) return 0; //assess loose dependent children status of all household members
		while(!TU.m_PersonRefs.empty())
		{
			//in the first iteration assess head of household,
			//in all further iterations assess head of "rest",
			//i.e. household members who have not yet been assigned to a taxunit
			if(!AssessHeadOfTU(&TU)) return 0;
			if(!AssessTUStatus(&TU, m_PartnerCond, 2)) return 0; //assess partner of head
			if(!AssessTUStatus(&TU, m_OwnChCond, 3)) return 0; //assess own children of head and partner
			if(!AssessTUStatus(&TU, m_OwnDepChCond, 4)) return 0; //assess own dependent children of head and partner
			if(!AssessTUStatus(&TU, m_DepParCond, 5)) return 0; //assess dependent parents of head and partner
			if(!AssessTUStatus(&TU, m_DepRelCond, 6)) return 0; //assess dependent relatives of head and partner

			//copy head and dependents of head of current taxunit into a sub-taxunit and erase them from main taxunit
			CEMTaxunit SubTU(m_Control, hh);
			SubTU.m_HeadIndex=TU.m_HeadIndex;
			SubTU.m_PersonRefs.clear();
			for(i=0; i<TU.m_PersonRefs.size(); ++i)
			{
				if(TU.m_PersonRefs.at(i).m_IsHead || (m_IsPartnerMem && TU.m_PersonRefs.at(i).m_IsPartner) ||
					(m_AreOwnDepChMem && TU.m_PersonRefs.at(i).m_IsOwnDepChild) || (m_AreLooseDepChMem && TU.m_PersonRefs.at(i).m_IsLooseDepChild) ||
					(m_AreOwnChMem && TU.m_PersonRefs.at(i).m_IsOwnChild) ||
					(m_AreDepParMem && TU.m_PersonRefs.at(i).m_IsDepPar) || (m_AreDepRelMem && TU.m_PersonRefs.at(i).m_IsDepRel))
				{
					SubTU.m_PersonRefs.insert(SubTU.m_PersonRefs.end(), TU.m_PersonRefs.at(i));
					TU.m_PersonRefs.erase(TU.m_PersonRefs.begin()+i);
					--i;
				}
			}
			//a second round over all not yet assessed members is necessary to assign "dependents of dependents"
			//i.e. if persons have "no" relation to head or parent but are children or partner of another tu member
			//(this is to avoid separating children/partners from their dependent parents/partners)
			if(m_ParamAdmin.GetYesNoParam("assigndepchofdependents")->m_numValue || m_ParamAdmin.GetYesNoParam("assignpartnerofdependents")->m_numValue)
			{
				for(i=0; i<TU.m_PersonRefs.size(); ++i)
				{
					double PartnerID=TU.GetVarVal(m_Control->m_ivPartnerID, m_ActHHVar, TU.m_PersonRefs.at(i).m_Index);
					double MotherID=TU.GetVarVal(m_Control->m_ivMotherID, m_ActHHVar, TU.m_PersonRefs.at(i).m_Index);
					double FatherID=TU.GetVarVal(m_Control->m_ivFatherID, m_ActHHVar, TU.m_PersonRefs.at(i).m_Index);
					for(unsigned int j=0; j<SubTU.m_PersonRefs.size(); ++j)
					{
						if(SubTU.m_PersonRefs.at(j).m_IsHead || SubTU.m_PersonRefs.at(j).m_IsPartner) continue;
						double memPID=SubTU.GetVarVal(m_Control->m_ivPID, m_ActHHVar, SubTU.m_PersonRefs.at(j).m_Index);	
						if((m_ParamAdmin.GetYesNoParam("assignpartnerofdependents")->m_numValue && memPID==PartnerID) ||
							(m_ParamAdmin.GetYesNoParam("assigndepchofdependents")->m_numValue && TU.m_PersonRefs.at(i).m_IsDepChild && (memPID==MotherID || memPID==FatherID)))
						{
							SubTU.m_PersonRefs.insert(SubTU.m_PersonRefs.end(), TU.m_PersonRefs.at(i));
							TU.m_PersonRefs.erase(TU.m_PersonRefs.begin()+i);
							--i;
							break;
						}
					}
				}
			}
			if(!AssessTUStatus(&SubTU, m_LoneParCond, 7)) return 0; //assess lone parent status
			TUC.insert(TUC.end(), SubTU);
		}
		break;
	}
	for(TUContainer_t::iterator ittuc=TUC.begin(); ittuc!=TUC.end(); ++ittuc)
	{
		(*ittuc).m_AdultCnt=(*ittuc).m_ChildCnt=0;
		for(PersonRefContainer_t::iterator itpr=(*ittuc).m_PersonRefs.begin(); itpr!=(*ittuc).m_PersonRefs.end(); ++itpr)
		{
			if(m_ParamAdmin.GetYesNoParam("nochildifhead")->m_numValue && (*itpr).m_IsHead && (*itpr).m_IsDepChild) (*itpr).m_IsDepChild=0;
			if(m_ParamAdmin.GetYesNoParam("nochildifpartner")->m_numValue && (*itpr).m_IsPartner && (*itpr).m_IsDepChild) (*itpr).m_IsDepChild=0;
			if((*itpr).m_IsDepChild) ((*ittuc).m_ChildCnt)++;
			else ((*ittuc).m_AdultCnt)++;
		}
	}
	hh->m_Taxunits.insert(std::pair<std::string, TUContainer_t>(m_TUName, TUC));
	return 1;
}

bool CEMCM_DefTU::AssessHeadOfTU(CEMTaxunit *TU)
{
	double maxy=0;
	std::vector<int> potHeads, fulfilHC;
	potHeads.clear();
	fulfilHC.clear();
	int nfulfilHC=0;
	unsigned int i;
	for(i=0; i<TU->m_PersonRefs.size(); i++)
	{//assess if taxunit members fulfil extended head condition
		TU->m_HeadIndex=TU->m_PersonRefs.at(i).m_Index;
		double ff=0;
		if(!m_ExtHeadCond->Assess(ff, TU, m_ActHHVar, TU->m_HeadIndex)) return 0;
		nfulfilHC+=(int)ff;
		fulfilHC.insert(fulfilHC.end(), (int)ff);
	}
	//if extended head condition rules out all potential heads, either stop or drop condition
	if(!nfulfilHC && m_ParamAdmin.GetYesNoParam("stopifnoheadfound")->m_numValue)
	{
		double hh = TU->GetVarVal(m_Control->m_ivHHID, m_ActHHVar);
		if(!CEMError::CritErr("Household "+CEMUtilities::DoubleToStr(hh)+": extended head conidtion rules out all assessment unit members for being head.", m_ParamAdmin.GetGeneralParam("extheadcond"))) return 0;
	}
	TU->m_HeadIndex=-1;
	for(i=0; i<TU->m_PersonRefs.size(); i++)
	{
		//persons cannot be head if they do not fulfil extended head condtion, unless nobody fulfils the condition
		if(nfulfilHC && !fulfilHC.at(i)) continue;
		//check if person is "the richest"
		double y=0; std::string VoidVar;
		if(!m_ParamAdmin.GetVarILParam("headdefinc")->Assess(y, TU, m_ActHHVar, TU->m_PersonRefs.at(i).m_Index, &VoidVar)) return 0;
		if(!VoidVar.empty()) //change 10.11.2012: before the parameter VoidVar was not used, which leaded to setting the whole incomelist to zero, instead of only the void values (see Bug #48 in redmine)
			{ if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+m_ParamAdmin.GetVarILParam("headdefinc")->m_strValue+"'.", m_ParamAdmin.GetVarILParam("headdefinc"), "Zero is used as default value for the not defined variables.")) return 0; }
		if(potHeads.empty()) { maxy=y; potHeads.insert(potHeads.end(), i); continue; }	
		if(y<maxy) continue; //there is a richer person
		if(y>maxy)
		{//person is the richest so far, i.e. a potential head
			potHeads.clear(); //all potential heads found so far are less rich than this person, i.e. no potential heads any longer
			potHeads.insert(potHeads.end(), i);
			maxy=y;
			continue;
		}
		//there is at least one equally rich person - check if person is oldest of potential heads
		double age=TU->GetVarVal(m_Control->m_ivAge, m_ActHHVar, TU->m_PersonRefs.at(i).m_Index);
		double ageoph=TU->GetVarVal(m_Control->m_ivAge, m_ActHHVar, TU->m_PersonRefs.at(potHeads.at(0)).m_Index); //the other potential heads must be equally old, therefore enough to compare with the first
		if(age<ageoph) continue; //the other potential head(s) are older
		if(age>ageoph) potHeads.clear(); //all potential heads found so far are younger than this person, i.e. no potential heads any longer
		potHeads.insert(potHeads.end(), i);
	}
	int hi=potHeads.at(0); //if there is more than one potential head, take the first, i.e. that with the smallest PID
	for(i=0; i<TU->m_PersonRefs.size(); i++) TU->m_PersonRefs.at(i).m_IsHead=((int)(i)==hi);
	TU->m_HeadIndex=TU->m_PersonRefs.at(hi).m_Index;
	return 1;
}

bool CEMCM_DefTU::AssessTUStatus(CEMTaxunit *TU, CEMCondParam *Cond, int Type)
{
	int origHeadIndex = TU->m_HeadIndex;
	bool PartnerFound=0;
	for(unsigned int i=0; i<TU->m_PersonRefs.size(); i++)
	{
		double dHasStatus; int HasStatus;
		if(!Cond->Assess(dHasStatus, TU, m_ActHHVar, TU->m_PersonRefs.at(i).m_Index)) return 0;
		HasStatus=int(dHasStatus);
		switch(Type)
		{
		case 1:
			TU->m_PersonRefs.at(i).m_IsDepChild=HasStatus;
			break;
		case 2:
			TU->m_PersonRefs.at(i).m_IsPartner=HasStatus;
			if(!HasStatus) break;
			if(!PartnerFound) PartnerFound=1;
			else
			{
				double hh = TU->GetVarVal(m_Control->m_ivHHID, m_ActHHVar, TU->m_HeadIndex);
				if(!CEMError::NonCritErr("Household "+CEMUtilities::DoubleToStr(hh)+": more than one possible partner found for assessment unit '"+m_TUName+"'.", m_ParamAdmin.GetGeneralParam("partnercond"), "All 'partners' but the first are ignored.")) return 0;
				TU->m_PersonRefs.at(i).m_IsPartner=0;
			}
			break;
		case 3:
			TU->m_PersonRefs.at(i).m_IsOwnChild=HasStatus;
			break;
		case 4:
			TU->m_PersonRefs.at(i).m_IsOwnDepChild=HasStatus;
			break;
		case 5:
			TU->m_PersonRefs.at(i).m_IsDepPar=HasStatus;
			break;
		case 6:
			TU->m_PersonRefs.at(i).m_IsDepRel=HasStatus;
			break;
		case 7:
			TU->m_PersonRefs.at(i).m_IsLonePar=HasStatus;
			break;
		case 8:
			TU->m_PersonRefs.at(i).m_IsLooseDepChild=HasStatus;
			break;
		}
	}
	TU->m_HeadIndex = origHeadIndex;
	return 1;
}


/********************************************************************************************
 functions class CEMCM_UpdateTU
********************************************************************************************/
bool CEMCM_UpdateTU::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("name")) ParType=PTYPE_NAME;
	else if(!ParName.compare("update_all")) ParType=PTYPE_YESNO;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_UpdateTU::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	
	if(!m_ParamAdmin.IsParamDefined("name") && !m_ParamAdmin.IsParamDefined("update_all"))
		{ if(!CEMError::CritErr("Neither parameter 'name' nor parameter 'update_all' defined.", this)) return 0; }
	m_TUName.clear();
	if(m_ParamAdmin.IsParamDefined("name")) m_TUName = m_ParamAdmin.GetGeneralParam("name")->m_strValue;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_UpdateTU::Run(TUTypeContainer_t *Taxunits)
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;
	//delete taxunit(s) from households taxunits - that means that it will be rebuilt when it is applied next time
	if(!m_TUName.empty())
	{
		TUTypeContainer_t::iterator itt = Taxunits->find(m_TUName);
		if(itt==Taxunits->end()) return CEMError::NonCritErr("Trying to update not yet defined assessment unit.", this, "", "", "Function 'UpdateTU' for assessment unit '"+m_TUName+"' is ignored.");
		Taxunits->erase(itt);
	}
	else Taxunits->clear();
	return 1;
}

/********************************************************************************************
 functions class CEMCM_DefOutput
********************************************************************************************/
bool CEMCM_DefOutput::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int len=int(ParName.length());
	if(!ParName.compare("file")) { ParType=PTYPE_NAME; Compulsory=1; }
	else if(ParName=="ndefils" || ParName=="nils" || ParName=="nvars" || ParName=="nsysvars") return 1; //not used anymore
	else if(ParName=="ndecimals") ParType=PTYPE_VAL;
	else if(ParName.substr(0,8)=="vargroup") { ParType=PTYPE_NAME; ParName="vargroup"; Single=0; }
	else if(ParName.substr(0,7)=="ilgroup") { ParType=PTYPE_NAME; ParName="ilgroup"; Single=0; }
	else if(ParName.substr(0,3)=="var") { ParType=PTYPE_VAR; ParName="var"; Single=0; }
	else if(ParName.substr(0,2)=="il") { ParType=PTYPE_IL; ParName="il"; Single=0; }
	else if(ParName.substr(0,5)=="defil") { ParType=PTYPE_IL; ParName="defil"; Single=0; }
	else if(ParName.substr(0,8)=="unitinfo")
	{
		size_t i=ParName.find("_");
		if(i!=std::string::npos)
		{
			if(Group.empty()) Group=ParName.substr(8,i-8);
			Single=0;
			if(ParName.substr(std::max(len-3,0))=="_tu") { ParName = "unitinfo_tu"; ParType=PTYPE_TU; }
			else if(ParName.find("_id")!=std::string::npos) { ParName = "unitinfo_id"; ParType=PTYPE_CATEG; }
		}
	}
	else if(ParName=="suppress_void_message") ParType=PTYPE_YESNO;
	else if(ParName=="replace_void_by") ParType=PTYPE_VAL;
	else if(ParName=="append") ParType=PTYPE_YESNO;
	else if(ParName=="multiplymonetaryby") ParType=PTYPE_FORMULA;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_DefOutput::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	if(!m_ParamAdmin.CheckCompParamExists("tax_unit", PTYPE_TU, this, "tu_individual_"+m_Control->m_Country)) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("file", PTYPE_NAME, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("ndecimals", PTYPE_VAL, this, "2")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("suppress_void_message", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("replace_void_by", PTYPE_VAL, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("append", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("who_must_be_elig", PTYPE_CATEG, this, "nobody")) return 0;
	if(!CheckWhoMustBeElig(m_ParamAdmin.GetGeneralParam("who_must_be_elig")->m_strValue, m_ParamAdmin.GetGeneralParam("who_must_be_elig")->m_numValue)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("elig_var", PTYPE_VAR, this, "sel_s")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("multiplymonetaryby", PTYPE_FORMULA, this, "1")) return 0;
	if(!m_ParamAdmin.Prepare()) return 0;
	
	//init variables
	m_Vars.empty();
	m_ILs.empty();

	//outcommented - see below
	//check if standard output (for checking below if non-standard variables are included)
	//bool isStd = (m_ParamAdmin.GetGeneralParam("file")->m_strValue.find("_std")!=std::string::npos);

	//loop over var-parameters
	std::vector<CEMVarParam*> varParams;
	m_ParamAdmin.GetNotSingleParams("var", varParams);
	for(std::vector<CEMVarParam*>::iterator itv=varParams.begin(); itv!=varParams.end(); ++itv)
	{
		if((*itv)->m_strValue=="n/a") continue;
		if(std::find(m_Vars.begin(), m_Vars.end(), (*itv)->m_VarIndex)==m_Vars.end())
		{
			//outcommented as Euromod team decided to not want this warning anymore (redmine issue 155)
			//if(isStd && (m_Control->m_VarIndexV[(*itv)->m_VarIndex].m_GenType==VARTYPE_DERDESC || m_Control->m_VarIndexV[(*itv)->m_VarIndex].m_GenType==VARTYPE_NONDESC))
			//	{ if(!CEMError::NonCritErr("Use of non-standard variable '"+m_Control->m_VarIndexV[(*itv)->m_VarIndex].m_Name+"' in standard output.", this, "", "", "Consider to use a standard variable instead.")) return 0; }
			m_Vars.insert(m_Vars.end(), (*itv)->m_VarIndex);
		}
  	}

	//vargroup-parameters
	std::vector<CEMParam*> vargroupParams;
	m_ParamAdmin.GetNotSingleParams("vargroup", vargroupParams);
	for(size_t vi=0; vi<m_Control->m_VarIndexV.size(); ++vi) //loop over all variables ...
	{
		//outcommented as the Essex team wants to output all variables by vargroup, including those defined by functions DefVar, DefConst, etc.
		//if(!(m_Control->m_VarIndexV[vi].m_GenType==VARTYPE_DEFAULT || m_Control->m_VarIndexV[vi].m_GenType==VARTYPE_DATA || m_Control->m_VarIndexV[vi].m_GenType==VARTYPE_DESC
		//	|| m_Control->m_VarIndexV[vi].m_Name=="sel_s" || m_Control->m_VarIndexV[vi].m_Name.substr(0,5)=="temp_"))
		//	continue; //only consider 'normal' variables plus sel_s and temp_*
		if (m_Control->m_VarIndexV[vi].m_Name == VARNAME_INTERNAL1 || m_Control->m_VarIndexV[vi].m_Name == VARNAME_INTERNAL2)
			continue;

		//... and check if they correspond to any vargroup-description
		for(std::vector<CEMParam*>::iterator itvg=vargroupParams.begin(); itvg!=vargroupParams.end(); ++itvg)
		{
			if((*itvg)->m_strValue=="n/a") continue;
			if(CEMUtilities::DoesValueMatchPattern((*itvg)->m_strValue, m_Control->m_VarIndexV[vi].m_Name))
			{
				if(m_Control->m_UseCommonDefault && m_Control->m_VarIndexV[vi].m_GenType == VARTYPE_DEFAULT && !m_System->IsVariableUsed(vi))
					continue; //use_commondefault loads all variables defined in the variable description file
							  //thus this avoids printing all variables and prints those with values different from zero only
							  //(not completely correct, as this happens at read-time and even dataset variables can be changed (to something different from zero) at run-time
				if(std::find(m_Vars.begin(), m_Vars.end(), vi)==m_Vars.end())
					m_Vars.insert(m_Vars.end(), (int)vi * (-1)); //multiply by -1 to mark as defined by wildcard to avoid a warning if the variable is void
				break;											 //happens e.g. if used in one system but not in another, if they are run together
			}
		}
  	}

	//loop over il-parameters
	std::vector<CEMILParam*> ilParams;
	m_ParamAdmin.GetNotSingleParams("il", ilParams);
	std::vector<CEMILParam*>::iterator iti;
	for(iti=ilParams.begin(); iti!=ilParams.end(); ++iti)
	{
		if((*iti)->m_strValue=="n/a") continue;
		if(std::find(m_ILs.begin(), m_ILs.end(), (*iti)->m_strValue)==m_ILs.end())
			m_ILs.insert(m_ILs.end(), (*iti)->m_strValue);
  	}

	//ilgroup-parameters
	std::vector<CEMParam*> ilgroupParams;
	m_ParamAdmin.GetNotSingleParams("ilgroup", ilgroupParams);
	for(ILContainer_t::iterator itil=m_System->m_ILs.begin(); itil!=m_System->m_ILs.end(); ++itil) //loop over all incomelists ...
	{
		std::string ilName = itil->first;
		//... and check if they correspond to any ilgroup-description
		for(std::vector<CEMParam*>::iterator itig=ilgroupParams.begin(); itig!=ilgroupParams.end(); ++itig)
		{
			if((*itig)->m_strValue=="n/a") continue;
			if(CEMUtilities::DoesValueMatchPattern((*itig)->m_strValue, ilName))
			{
				if(std::find(m_ILs.begin(), m_ILs.end(), ilName)==m_ILs.end())
					m_ILs.insert(m_ILs.end(), ilName);
				break;
			}
		}
  	}

	//loop over defil-parameters
	m_ParamAdmin.GetNotSingleParams("defil", ilParams);
	for(iti=ilParams.begin(); iti!=ilParams.end(); ++iti)
	{
		if((*iti)->m_strValue=="n/a") continue;
		std::vector<int> VarInd;
		VarInd.clear();
		m_System->GetILContent((*iti)->m_strValue, VarInd, *iti);
		if(VarInd.size()==0) { if(!CEMError::NonCritErr("Incomelist '"+(*iti)->m_strValue+"' does not (yet) contain any variables. (Hint: Incomelists generated at run-time cannot be used as definition incomelists.)", this, "", "", "Parameter '"+(*iti)->m_Name+"' is ignored.")) return 0; }
		for(std::vector<int>::iterator itv=VarInd.begin(); itv!=VarInd.end(); ++itv)
		{
			if(std::find(m_Vars.begin(), m_Vars.end(), *itv)==m_Vars.end()
				&& std::find(m_Vars.begin(), m_Vars.end(), *itv * (-1)) == m_Vars.end()) // 22.8.2017: to avoid that using VarGroup and DefIL produces double-variables (see "multiply by -1 to mark as defined by wildcard ..." comment above )
				m_Vars.insert(m_Vars.end(), *itv);
		}
  	}

	//loop over unit-id-parameters
	if(m_ParamAdmin.GetTUParam()->m_TUDefMod->m_Type==TUTYPE_IND)
	{
		m_UnitInfoTU.clear();m_PrintHeadID.clear();m_PrintIsPartner.clear();m_PrintIsDepChild.clear();
		m_PrintIsOwnChild.clear();m_PrintIsOwnDepChild.clear();m_PrintIsLooseDepChild.clear();
		m_PrintIsDepParent.clear();m_PrintIsDepRelative.clear();m_PrintIsLoneParent.clear();
			
		std::map<int,Param_t> UnitInfoParam;
		m_ParamAdmin.GetGroupParam(UnitInfoParam, "unitinfo_");
		for (std::map<int,Param_t>::iterator itM = UnitInfoParam.begin(); itM != UnitInfoParam.end(); ++itM)
		{		
			bool PrintHeadID=0,PrintIsPartner=0,PrintIsDepChild=0,PrintIsOwnChild=0,PrintIsOwnDepChild=0,PrintIsLooseDepChild=0,PrintIsDepParent=0,PrintIsDepRelative=0,PrintIsLoneParent=0;
			CEMTUParam *uiTU = NULL;
			for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
			{
				if ((*itP)->m_Name == "unitinfo_id")
				{
					if ((*itP)->m_strValue == "headid") PrintHeadID = 1;
					else if ((*itP)->m_strValue == "ispartner")
						PrintIsPartner = 1;
					else if ((*itP)->m_strValue == "isdependentchild" || (*itP)->m_strValue=="isdepchild")
						PrintIsDepChild = 1;
					else if ((*itP)->m_strValue == "isownchild")
						PrintIsOwnChild = 1;
					else if ((*itP)->m_strValue == "isowndependentchild" || (*itP)->m_strValue=="isowndepchild")
						PrintIsOwnDepChild = 1;
					else if ((*itP)->m_strValue == "isloosedependentchild" || (*itP)->m_strValue=="isloosedepchild")
						PrintIsLooseDepChild = 1;
					else if ((*itP)->m_strValue == "isdependentparent" || (*itP)->m_strValue=="isdepparent" || (*itP)->m_strValue=="isdeppar")
						PrintIsDepParent = 1;
					else if ((*itP)->m_strValue == "isdependentrelative" || (*itP)->m_strValue=="isdeprelative" || (*itP)->m_strValue=="isdeprel")
						PrintIsDepRelative = 1;
					else if ((*itP)->m_strValue == "isloneparent" || (*itP)->m_strValue=="islonepar" || (*itP)->m_strValue=="singleparent" || (*itP)->m_strValue=="singlepar")
						PrintIsLoneParent = 1;
					else
					{
						if (!CEMError::CritErr("Unknown unit information variable.", (*itP)))
							return 0;
					}
				}
				else if ((*itP)->m_Name == "unitinfo_tu")
					uiTU = (CEMTUParam*)(*itP);
				else
				{
					if (!CEMError::NonCritErr("Unknown parameter for unitinfo group " + CEMUtilities::IntToStr(itM->first) + ".",
						(*itP), "Parameter is ignored."))
						return 0;
				}
			}

			if (!uiTU)
			{
				if (!CEMError::NonCritErr("Missing definition for assessment unit ('UnitInfo_TU') of group "
					+ CEMUtilities::IntToStr(itM->first) + ".", this, "Group " + CEMUtilities::IntToStr(itM->first) + " is ignored."))
					return 0;
				continue;
			}

			m_UnitInfoTU.push_back(uiTU);
			m_PrintHeadID.push_back(PrintHeadID);
			m_PrintIsPartner.push_back(PrintIsPartner);
			m_PrintIsDepChild.push_back(PrintIsDepChild);
			m_PrintIsOwnChild.push_back(PrintIsOwnChild);
			m_PrintIsOwnDepChild.push_back(PrintIsOwnDepChild);
			m_PrintIsLooseDepChild.push_back(PrintIsLooseDepChild);
			m_PrintIsDepParent.push_back(PrintIsDepParent);
			m_PrintIsDepRelative.push_back(PrintIsDepRelative);
			m_PrintIsLoneParent.push_back(PrintIsLoneParent);
		}

	}
	return 1;
}

void CEMCM_DefOutput::GenHeader(int Mode, std::string &Header, std::string OutfileName)
{
	char startBuf[100], curBuf[100];
	time_t curTime = time(NULL);
#ifdef _WIN32
	struct tm timeinfo;
	localtime_s(&timeinfo, &(m_Control->m_StartTime));
	strftime(startBuf, 100, "%d %b %Y; %H:%M:%S", &timeinfo);
	localtime_s(&timeinfo, &curTime);
	strftime(curBuf, 100, "%d %b %Y; %H:%M:%S", &timeinfo);
#else	// linux does not support _s functions
	struct tm * timeinfo;
	timeinfo = localtime(&(m_Control->m_StartTime));
	strftime(startBuf, 100, "%d %b %Y; %H:%M:%S", timeinfo);
	timeinfo = localtime(&curTime);
	strftime(curBuf, 100, "%d %b %Y; %H:%M:%S", timeinfo);
#endif
	std::string EurNat;
	if(m_System->m_OutputCurrencyEuro) EurNat="euro"; else EurNat="national";
	char sheader[5000];
	switch(Mode)
	{
	case 1:
		EM_SPRINTF(sheader, "System\t%s\nDatabase\t%s\nEUROMOD-Version\t%s\nExecutable-Version\t%s\nUser-Interface-Version\t%s\nPrinted\t%s\nOutputfile\t%s\nCurrency\t%s\nExchangerate\t%f\nPolicy Switches\t%s\n",
			m_System->m_Name.c_str(),
			m_Control->m_DataSet.c_str(),
			m_Control->m_EMVersion.c_str(),
			m_Control->m_UIVersion.c_str(), //formerly: m_ExeVersion.c_str(), own version number not used anymore since UI and executable form one software bundle
			m_Control->m_UIVersion.c_str(),
			curBuf,
			OutfileName.c_str(),
			EurNat.c_str(),
			m_System->m_ExchRate,
			m_Control->GetSystemPolicySwitchesForHeader(m_System->m_Id).c_str());
		break;
	case 2:
		EM_SPRINTF(sheader, "%s", "System\tDatabase\tEUROMOD-Version\tExecutable-Version\tUser-Interface-Version\tStart\tEnd\tOutputfile\tCurrency\tExchangerate\tPolicy Switches\n");
		break;
	case 3:
		EM_SPRINTF(sheader, "%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%s\t%f\t%s\n",
			m_System->m_Name.c_str(),
			m_Control->m_DataSet.c_str(),
			m_Control->m_EMVersion.c_str(),
			m_Control->m_UIVersion.c_str(), //formerly: m_ExeVersion.c_str(), own version number not used anymore since UI and executable form one software bundle
			m_Control->m_UIVersion.c_str(),
			startBuf, curBuf,
			OutfileName.c_str(),
			EurNat.c_str(),
			m_System->m_ExchRate,
			m_Control->GetSystemPolicySwitchesForHeader(m_System->m_Id).c_str());
	}
	Header = sheader;
}

bool CEMCM_DefOutput::Run()
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	//currency converting
	double CurConvFact=1;
	if(m_System->m_OutputCurrencyEuro!=m_System->m_ParamCurrencyEuro)
	{
		CurConvFact = m_System->m_ExchRate;
		if(m_System->m_OutputCurrencyEuro) CurConvFact = 1/CurConvFact;
	}

	std::string file = m_ParamAdmin.GetGeneralParam("file")->m_strValue;
	if(file.length()>4 && !file.substr(file.length()-4).compare(".txt")) file=file.substr(0,file.length()-4);
	if(m_Control->m_OutfileDate!="") file += "_";
	file += m_Control->m_OutfileDate + ".txt";
	file = m_Control->m_OutputPath+file;
	FILE *pFile; bool append=0;
#ifdef _WIN32
	if(!m_ParamAdmin.GetYesNoParam("append")->m_numValue)
		fopen_s(&pFile, file.c_str(),"w");
	else
	{
		fopen_s(&pFile, file.c_str(),"r"); if(pFile) { append=1; fclose(pFile); }
		fopen_s(&pFile, file.c_str(),"a");
	}
#else	// linux does not support _s functions
	if(!m_ParamAdmin.GetYesNoParam("append")->m_numValue)
		pFile = fopen(file.c_str(),"w");
	else
	{
		pFile = fopen(file.c_str(),"r"); if(pFile) { append=1; fclose(pFile); }
		pFile = fopen(file.c_str(),"a");
	}
#endif
	if(!pFile) return CEMError::CritErr("Output data file '"+file+"' could not be opened.", m_ParamAdmin.GetGeneralParam("file"));

	char prec[10];
	EM_SPRINTF(prec, "%d", (int)(m_ParamAdmin.GetValParam("ndecimals")->m_numValue));
	std::string fmt = "%.";
	fmt += prec;
	fmt += "f\t";

	//print header if not in separate file
	if(m_Control->m_HeaderDate=="")
	{
		std::string Header;
		GenHeader(1, Header, file);
		fprintf(pFile, "%s", Header.c_str());
	}
	//print headline
	std::string headline="";
	std::vector<int>::iterator itv;
	for(itv=m_Vars.begin(); itv!=m_Vars.end(); itv++)
		headline += m_Control->m_VarIndexV[((*itv)<0?(*itv)*(-1):(*itv))].m_Name + "\t";
	std::vector<std::string>::iterator iti;
	for(iti=m_ILs.begin(); iti!=m_ILs.end(); iti++) headline += (*iti) + "\t";
	for(size_t ui=0; ui<m_UnitInfoTU.size(); ++ui)
	{
		std::string tun = m_UnitInfoTU[ui]->m_strValue + "_";
		if(m_PrintHeadID[ui]) headline += tun+"headid\t";
		if(m_PrintIsPartner[ui]) headline += tun+"ispartner\t";
		if(m_PrintIsDepChild[ui]) headline += tun+"isdepchild\t";
		if(m_PrintIsOwnChild[ui]) headline += tun+"isownchild\t";
		if(m_PrintIsOwnDepChild[ui]) headline += tun+"isowndepchild\t";
		if(m_PrintIsLooseDepChild[ui]) headline += tun+"isloosedepchild\t";
		if(m_PrintIsDepParent[ui]) headline += tun+"isdepparent\t";
		if(m_PrintIsDepRelative[ui]) headline += tun+"isdeprelative\t";
		if(m_PrintIsLoneParent[ui]) headline += tun+"isloneparent\t";
	}
	if(!append) fprintf(pFile, "%s\n", headline.c_str());
	double SupVoidMess=m_ParamAdmin.GetYesNoParam("suppress_void_message")->m_numValue;
	double RepVoidBy=m_ParamAdmin.GetValParam("replace_void_by")->m_numValue;
	
	//loop over households
	double multiplyMonetaryBy = EMVOID;
	for(HHContainer_t::iterator ith=m_System->m_Households.begin(); ith!=m_System->m_Households.end(); ith++)
	{
		//loop over taxunits
		CEMTUParam *tup=m_ParamAdmin.GetTUParam();
		if(!ith->AreTUsBuilt(tup->m_strValue)) tup->m_TUDefMod->BuildTUs(&(*ith), &(ith->m_Persons));
		TUContainer_t TUs = ith->GetTUsOfType(tup->m_strValue);
		for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
		{
			int Elig;
			m_ActTU = &(*itt);
			m_ActHHVar = &(ith->m_Persons);
			if(!GetTUElig(Elig)) return 0;
			if(!Elig) continue;

			if(multiplyMonetaryBy == EMVOID) //only do once, but output-function specific exchange rate cannot be assessed without TU and HH, as it is a formula
			{
				m_ParamAdmin.GetFormulaParam("multiplymonetaryby")->Assess(multiplyMonetaryBy, m_ActTU, m_ActHHVar);
				CurConvFact *= multiplyMonetaryBy;
			}

			//print variables
			for(itv=m_Vars.begin(); itv!=m_Vars.end(); itv++)
			{
				bool definedByWildcard = 0;
				int varIndex = *itv;
				if(varIndex < 0)
				{
					definedByWildcard = 1;
					varIndex *= -1;
				}
				if(m_Control->m_VarIndexV[varIndex].m_Monetary)
				{
					double mv=itt->GetVarVal(varIndex, &(ith->m_Persons));
					if(mv==EMVOID)
					{
						if(!(SupVoidMess || definedByWildcard)) { if(!CEMError::NonCritErr("Use of not yet calculated variable '"+m_Control->m_VarIndexV[varIndex].m_Name+"'.", this, "", "", CEMUtilities::DoubleToStr(RepVoidBy)+" is used as default.")) return 0; }
						fprintf(pFile, fmt.c_str(), RepVoidBy);
					}
					else fprintf(pFile, fmt.c_str(), CurConvFact*mv);
				}
				else
				{
					double nmv=itt->GetVarVal(varIndex, &(ith->m_Persons));
					if(nmv==EMVOID)
					{
						if(!(SupVoidMess || definedByWildcard)) { if(!CEMError::NonCritErr("Use of not yet calculated variable '"+m_Control->m_VarIndexV[varIndex].m_Name+"'.", this, "", "", CEMUtilities::DoubleToStr(RepVoidBy)+" is used as default.")) return 0; }
						nmv = RepVoidBy;
					}
					if(nmv-int(nmv)==0.0) fprintf(pFile, "%d\t", int(nmv));
					else fprintf(pFile, fmt.c_str(), nmv);
				}
			}
			//print incomelists
			for(iti=m_ILs.begin(); iti!=m_ILs.end(); iti++)
			{
				std::string VoidVar="";
				double il = itt->GetILVal(m_System->m_ILs[(*iti)], &(ith->m_Persons), -1, &VoidVar);
				if(!VoidVar.empty())
				{
					if(!SupVoidMess)
					{
						std::string handling = "Zero is used as default value for the not defined variables.";
						if(RepVoidBy) handling = CEMUtilities::DoubleToStr(RepVoidBy)+" is used as default value for the incomelist.";
						if(!CEMError::NonCritErr("Use of not yet calculated variable(s) "+VoidVar+"in incomelist '"+*iti+"'.", this, "", "", handling)) return 0;
					}
					if(RepVoidBy) il = RepVoidBy / CurConvFact; //divide by currency converter as il is multiplied by it below, to avoid VOID being currency converted
				}
				fprintf(pFile, fmt.c_str(), CurConvFact*il);
			}
			//print system variables
			for(size_t ui=0; ui<m_UnitInfoTU.size(); ++ui)
			{
				CEMTUParam *uiTU = m_UnitInfoTU[ui];
				CEMTaxunit *svTU;
				if(!uiTU) svTU=&(*itt);
				else
				{
					if(!ith->AreTUsBuilt(uiTU->m_strValue))
					{
						if(!CEMError::NonCritErr("'"+uiTU->m_strValue+"' is a not used assessment unit. Therefore it is not possible to print unit information variables.", uiTU, "Unit information variables of main assessment unit will be printed instead.")) return 0;
						svTU=&(*itt);
					}
					else
					{
						svTU=ith->GetContainingTU(&(*itt), uiTU->m_strValue);
						if(!svTU)
						{
							if(!CEMError::NonCritErr("'"+uiTU->m_strValue+"' cannot be used as assessment unit for unit information variables. Only assessment units containing the function's main assessment unit are allowed (e.g. maintu=family/sysvartu=hh is ok as family is part of hh; maintu=hh/sysvartu=family not ok as hh is not part of family).", m_ParamAdmin.GetTUParam("sysvar_tu"), "Main assessment unit is used as assessment unit for system variables.")) return 0;
							svTU=&(*itt);
						}
					}
				}
				CEMPerson pp;
				for(PersonRefContainer_t::iterator itp=svTU->m_PersonRefs.begin(); itp!=svTU->m_PersonRefs.end(); ++itp)
					if(itp->m_Index==itt->m_PersonRefs[0].m_Index) { pp=*itp; break; } //actually it should not be m_PersonRefs[0] but the head of the main taxunit, but to apply this meaningfully the main tu should anyway be individual
				if(m_PrintHeadID[ui]) fprintf(pFile, "%.0f\t", svTU->GetVarVal(m_Control->m_ivPID, &(ith->m_Persons), svTU->m_HeadIndex));
				if(m_PrintIsPartner[ui]) fprintf(pFile, "%d\t", pp.m_IsPartner);
				if(m_PrintIsDepChild[ui]) fprintf(pFile, "%d\t", pp.m_IsDepChild);
				if(m_PrintIsOwnChild[ui]) fprintf(pFile, "%d\t", pp.m_IsOwnChild);
				if(m_PrintIsOwnDepChild[ui]) fprintf(pFile, "%d\t", pp.m_IsOwnDepChild);
				if(m_PrintIsLooseDepChild[ui]) fprintf(pFile, "%d\t", pp.m_IsLooseDepChild);
				if(m_PrintIsDepParent[ui]) fprintf(pFile, "%d\t", pp.m_IsDepPar);
				if(m_PrintIsDepRelative[ui]) fprintf(pFile, "%d\t", pp.m_IsDepRel);
				if(m_PrintIsLoneParent[ui]) fprintf(pFile, "%d\t", pp.m_IsLonePar);
			}
			fputc('\n',pFile);
		}
	}
	fclose(pFile);
	//print header in separate file
	if(m_Control->m_HeaderDate!="")
	{
		std::string Header;
		std::string hfile = m_Control->m_OutputPath + m_Control->m_HeaderDate+ "_EMHeader.txt";
		FILE *hFile;
#ifdef _WIN32
		fopen_s(&hFile, hfile.c_str(),"r+"); //try to open header file for reading and writting
#else	// linux does not support _s functions
		hFile = fopen(hfile.c_str(),"r+"); //try to open header file for reading and writting
#endif
		if(!hFile)
		{//create and open for writing if header file does not exist
#ifdef _WIN32
			fopen_s(&hFile, hfile.c_str(),"w");
#else	// linux does not support _s functions
			hFile = fopen(hfile.c_str(),"w");
#endif
			if(!hFile) return CEMError::NonCritErr("Header file '"+hfile+"' could not be opened.", this, "", "", "No header is printed.");
			GenHeader(2, Header, file); //print headings
			fprintf(hFile, "%s", Header.c_str()); 
		}
		else fseek(hFile, 0, SEEK_END); //seek to end if header file exists
		GenHeader(3, Header, file); //print info
		fprintf(hFile, "%s", Header.c_str()); 
		fclose(hFile);
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_Uprate
********************************************************************************************/
bool CEMCM_Uprate::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int vi=-1;
	int len=int(ParName.length());
	
	if (ParName.substr(0,3) == "var" && //to allow for varx_factor and varx_condition
		(ParName.substr(std::max(len-7, 0)) == "_factor" || //instead of varx_factor1 and varx_condition1
		ParName.substr(std::max(len-10, 0)) == "_condition"))
	{
		ParName+="1";
		++len;
	}

	if (ParName == "name")
		ParType = PTYPE_NAME;
	else if (ParName == "dataset")
	{
		if (ParVal != "n/a")
			ParVal = CEMUtilities::AppendExtension(ParVal);
		ParType = PTYPE_NAME;
		Single = 0;
	}
	//new style (xml): factor is assigned as with non-conditional variables
	//(variable<->factor: parameter-name = yem, parameter-value = factor (e.g. 1.01), group = X;
	// variable/factor<->condition: factor_condition = {condition}, group = X)
	else if (ParName == "factor_condition")
	{
		ParName = "var_condition";
		ParType = PTYPE_COND;
	}
	//old style (txt): Var1_Name, Var1_Factor1, Var1_Condition1, Var1_Factor2, Var1_Condition2; Var2_Name, etc.
	else if (ParName.substr(0,3) == "var")
	{
		size_t i = ParName.find("_");
		if (i != std::string::npos)
		{
			Single=0;
			int XGroup = CEMUtilities::StrToInt(ParName.substr(3, i-3)); //assess the X of VarX_Name, VarX_FactorY, VarX_ConditionY
			int YGroup = 0;
			std::string sYGroup = "";
			std::string nums = "0123456789";
			for (size_t j = ParName.length()-1; j > i; --j) //assess the Y of VarX_FactorY, VarX_ConditionY
			{
				if (nums.find(ParName.at(j)) == std::string::npos)
					break;
				sYGroup = ParName.substr(j,1) + sYGroup;
			}
			YGroup = CEMUtilities::StrToInt(sYGroup);
			Group = CEMUtilities::IntToStr(XGroup * 1000 + YGroup); //temporarly write both groups into group (YGroup is empty for VarX_Name)
			if (ParName.rfind("_factor") != std::string::npos)
			{
				ParName = "var_factor";
				ParType = PTYPE_NAME;
			}
			else if (ParName.rfind("_condition") != std::string::npos)
			{
				if (ParVal == "n/a")
					return 1;

				//first insert parameter var_name with respective group, as otherwise VarX_ConditionY1, as VarX_ConditionY2, etc. share one VarX (with currently (wrong) name (orig_var_name) and group (X*1000)) ...
				if (m_ParamAdmin.IsParamDefined("orig_var_name", CEMUtilities::IntToStr(XGroup * 1000)))
				{
					CEMVarParam *varPar = m_ParamAdmin.GetVarParam("orig_var_name", CEMUtilities::IntToStr(XGroup * 1000));
					if (!CEMModule::TakeParam("var_name", varPar->m_strValue, "op", Line, Column, Group, PTYPE_VAR, 0))
						return 0;
				}
				//... then insert var_condition
				ParName = "var_condition";
				ParType=PTYPE_COND;
			}
			else if (ParName.substr(std::max(len-5,0)) == "_name")
			{
				ParName = "orig_var_name";
				ParType = PTYPE_VAR;
			}
		}
	}
	else if (ParName == "warnifnofactor")
		ParType=PTYPE_YESNO;
	else if (ParName == "def_factor")
		ParType=PTYPE_NAME; //allow for values and constants
	else if (ParName.substr(0,6) == "factor")
	{
		size_t i=ParName.find("_");
		if(i != std::string::npos)
		{
			if (Group.empty())
				Group = ParName.substr(6, i-6);
			Single=0;
			if (ParName.substr(std::max(len-5,0)) == "_name")
			{
				ParName = "factor_name";
				ParType = PTYPE_NAME;
			}
			else if (ParName.substr(std::max(len-6,0)) == "_value")
			{
				ParName = "factor_value";
				ParType=PTYPE_NAME; //allow for values and constants
			}
		}
	}
	else if(ParName.substr(0,6) == "aggvar")
	{
		size_t i = ParName.find("_");
		if (i != std::string::npos)
		{
			if (Group.empty())
				Group = ParName.substr(6, i-6);
			Single=0;
			if (ParName.substr(std::max(len-5,0)) == "_name")
			{
				ParName = "aggvar_name";
				ParType = PTYPE_VAR;
			}
			else if (ParName.substr(std::max(len-10,0)) == "_tolerance")
			{
				ParName = "aggvar_tolerance";
				ParType = PTYPE_VAL;
			}
			else if (ParName.rfind("_part") != std::string::npos)
			{
				ParName = "aggvar_part";
				ParType=PTYPE_VAR;
			}
		}
	}
	else if (m_Control->GetVarIndex(ParName, vi) || m_Control->m_Vardesc.find(ParName) != m_Control->m_Vardesc.end())
	{//check if parameter is the name of a variable
		if (ParVal == "n/a")
			return 1;
		std::string strCondition = "";
		if (Group.empty())
			Group = CEMUtilities::IntToStr(100000 + m_nV);
		else //do not just use the group, as it could be there by accident (the parameter-administrator automatically puts a number because there could be a respective factor_condition)
		{
			if (m_ParamAdmin.IsParamDefined("var_condition", Group)) //search for possible factor_condition (to not loose it due to changing of group below)
				strCondition = m_ParamAdmin.GetParam("var_condition", Group)->m_strValue; //this way the condition can also be used on several variables
			Group = CEMUtilities::IntToStr(CEMUtilities::StrToInt(Group) * 100000 + m_nV); //this gives a unique number, even if the group is accidentially used twice
		}
		//first insert parameter for variable name ...
		if (!CEMModule::TakeParam("var_name", ParName, ParId, Line, Column, Group, PTYPE_VAR, 0))
			return 0;

		//... second (possibly) insert parameter for the uprating-condition ...
		if (strCondition != "")
			if (!CEMModule::TakeParam("var_condition", strCondition, "op", Line, Column, Group, PTYPE_COND, 0))
				return 0;

		//... finally adapt parameter for factor value
		ParName = "var_factor";
		ParType = PTYPE_NAME;
		ParId = "op"; //to avoid double id
		++m_nV;
	}

	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Uprate::GetFactorValueOrConstIndex(CEMParam *pFactor, double &valFactor, bool issueError)
{
	bool err = 1;
	valFactor = CEMUtilities::StrToDouble(pFactor->m_strValue, &err);
	if (err)
	{
		int constIndex = 0;
		if (m_Control->GetVarIndex(pFactor->m_strValue, constIndex))
			valFactor = NUMPOSINFINITE + constIndex; //set the factor temporarily to infinite plus the index to assess the value of the constant in the run function
		else
		{
			if (issueError)
			{
				valFactor = 1;
				return CEMError::CritErr("Factor is not a valid number.", pFactor);
			}
			return 0;
		}
	}
	return 1;
}

bool CEMCM_Uprate::CheckParam()
{
	if (!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off"))
		return 0;
	if (m_ParamAdmin.GetSwitchParam()->m_numValue==0)
		return 1; //don't check if module is switched off
	if (!m_ParamAdmin.Prepare())
		return 0;
	if (!m_ParamAdmin.CheckOptParamExists("warnifnofactor", PTYPE_YESNO, this, "1"))
		return 0;
	if (!m_ParamAdmin.CheckOptParamExists("def_factor", PTYPE_NAME, this, "1"))
		return 0;

	if (!GetFactorValueOrConstIndex(m_ParamAdmin.GetGeneralParam("def_factor"), m_DefFactor))
		return 0;

	//deactivate this warning for the time being, as parameter is still widely used
	//if (m_ParamAdmin.IsParamDefined("name"))
	//	{ if(!CEMError::NonCritErr("parameter 'name' has phase-out status. Please consider using parameter 'dataset' instead.", m_ParamAdmin.GetGeneralParam("name"))) return 0; }

	Param_t dsPars; m_ParamAdmin.GetNotSingleParams("dataset", dsPars);
	if (!m_ParamAdmin.IsParamDefined("name") && dsPars.size()==0)
		{ if(!CEMError::NonCritErr("Neither parameter 'name' nor parameter 'dataset' defined.", this)) return 0; }

	//assess defined uprating factors
	std::vector<std::string> FacNames; FacNames.clear();
	std::vector<double> FacVals; FacVals.clear();
	std::map<int,Param_t> facParam;
	m_ParamAdmin.GetGroupParam(facParam, "factor_");
	for (std::map<int,Param_t>::iterator itM = facParam.begin(); itM != facParam.end(); ++itM)
	{
		CEMParam *DoubleDef = NULL; std::string fName = ""; double fValue = EMVOID;
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == "factor_name")
			{
				if (fName.empty())
					fName = (*itP)->m_strValue;
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "factor_value")
			{
				if (fValue == EMVOID)
				{
					if(!GetFactorValueOrConstIndex(*itP, fValue))
						return 0;
				}
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else
				if (!CEMError::CritErr("Unknown parameter for factor " + CEMUtilities::IntToStr(itM->first) + ".", (*itP)))
					return 0;
		}

		if(DoubleDef)
		{
			if (!CEMError::CritErr("Multiple definition for factor " + DoubleDef->m_Group + ".", DoubleDef))
				return 0;
			continue;
		}

		if (fName.empty())
		{
			if (!CEMError::CritErr("Missing factor name for group " + CEMUtilities::IntToStr(itM->first) + ".", this))
				return 0;
			continue;
		}

		if (fValue == EMVOID)
		{
			if (!CEMError::CritErr("Missing factor value for group " + CEMUtilities::IntToStr(itM->first) + ".", this))
				return 0;
			continue;
		}

		FacNames.push_back(fName);
		FacVals.push_back(fValue);
	}

	//assess defined variables to uprate
	std::map<int,Param_t> uprVarParam;
	m_ParamAdmin.GetGroupParam(uprVarParam, "var_");
	for (std::map<int,Param_t>::iterator itM = uprVarParam.begin(); itM != uprVarParam.end(); ++itM)
	{
		CEMUprateVar UprVar; UprVar.m_Cond = NULL; UprVar.m_Factor = EMVOID; UprVar.m_VarIndex = -1;
		CEMParam *DoubleDef = NULL;								 
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == "var_name")
			{
				if (UprVar.m_VarIndex == -1)
					UprVar.m_VarIndex = ((CEMVarParam*)(*itP))->m_VarIndex;
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name=="var_condition")
			{
				if (!UprVar.m_Cond)
					UprVar.m_Cond = (CEMCondParam*)(*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "var_factor")
			{
				if (UprVar.m_Factor != EMVOID)
				{
					DoubleDef = (*itP);
					break;
				}

				if (!GetFactorValueOrConstIndex(*itP, UprVar.m_Factor, 0))
				{//if factor is no number or constant: still possible it is a named factor 
					size_t f;
					for(f = 0; f < FacNames.size(); ++f)
						if (FacNames.at(f) == (*itP)->m_strValue)
							break;
					if (f < FacNames.size())
						UprVar.m_Factor = FacVals.at(f);
					else
					{
						if (!CEMError::CritErr("Unknown factor '" + (*itP)->m_strValue + "'.", (*itP)))
							return 0;
					}
				}
			}
			else
				if (!CEMError::CritErr("Unknown parameter.", (*itP)))
					return 0;
		}

		if (DoubleDef)
		{
			if (!CEMError::CritErr("Multiple definition for uprating group " + DoubleDef->m_Group + ".", DoubleDef))
				return 0;
			continue;
		}
		if (UprVar.m_VarIndex == -1)
		{
			if (UprVar.m_Factor != EMVOID) //otherwise it's most likely just a "loose" contion which was already taken into account above (see comment: "//... second (possibly) insert parameter for the uprating-condition")
				if (!CEMError::CritErr("No variable defined for uprate group " + CEMUtilities::IntToStr(itM->first) + ".", this))
					return 0;
			continue;
		}
		if (UprVar.m_Factor == EMVOID)
		{
			if (UprVar.m_VarIndex != -1) //otherwise it's most likely just a "loose" contion which was already taken into account above (see comment: "//... second (possibly) insert parameter for the uprating-condition")
				if (!CEMError::CritErr("No factor defined for uprate group " + CEMUtilities::IntToStr(itM->first) + ".", this))
					return 0;
			continue;
		}

		m_UprateVars.push_back(UprVar);
	}

	//assess aggregate variables
	std::map<int,Param_t> aggParam;
	m_ParamAdmin.GetGroupParam(aggParam, "aggvar_");
	for (std::map<int,Param_t>::iterator itM = aggParam.begin(); itM != aggParam.end(); ++itM)
	{
		CEMParam *DoubleDef = NULL;
		CEMUprateAgg UA; UA.m_PartIndex.clear(); UA.m_Tolerance = 0.1; UA.m_VarIndex = -1;
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == "aggvar_name")
			{
				if (UA.m_VarIndex == -1)
					UA.m_VarIndex = ((CEMVarParam*)(*itP))->m_VarIndex;
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "aggvar_part")
				UA.m_PartIndex.push_back(((CEMVarParam*)(*itP))->m_VarIndex);
			else if ((*itP)->m_Name == "aggvar_tolerance")
				UA.m_Tolerance = ((CEMValParam*)(*itP))->m_numValue;
			else
			{
				if (!CEMError::CritErr("Unknown parameter for aggregate " + CEMUtilities::IntToStr(itM->first) + ".", (*itP)))
					return 0;
			}
		}

		if(DoubleDef)
		{
			if (!CEMError::CritErr("Multiple definition for aggregate " + DoubleDef->m_Group + ".", DoubleDef))
				return 0;
			continue;
		}

		if (UA.m_VarIndex == -1)
		{
			if (!CEMError::CritErr("Aggregate variable " + CEMUtilities::IntToStr(itM->first) + " has parts but no name.", this))
				return 0;
			continue;
		}
		else if (!UA.m_PartIndex.size())
		{
			if (!CEMError::CritErr("Aggregate variable " + CEMUtilities::IntToStr(itM->first) + " has no parts.", this))
				return 0;
		}
		else
			m_UprateAggs.push_back(UA);
	}

	//set default factor as uprating factor for all not explicitly indicated monetary variables
	double warnIfNoFactor = m_ParamAdmin.GetYesNoParam("warnifnofactor")->m_numValue;
	if(!DoesReferToDataset())
		warnIfNoFactor = 0.0;
		
	std::string noFactorVar = "";
	for(Vardesc_t::iterator it=m_Control->m_VarIndex.begin(); it!=m_Control->m_VarIndex.end(); ++it)
	{
		if(!it->second.m_Monetary || !it->second.m_GenType==VARTYPE_DATA)
			continue;
		std::vector<CEMUprateVar>::iterator itv;
		for(itv=m_UprateVars.begin(); itv!=m_UprateVars.end(); ++itv)
			if(it->second.m_Index==itv->m_VarIndex)
				break;
		if(itv!=m_UprateVars.end())
			continue;
		std::vector<CEMUprateAgg>::iterator ita;
		for(ita=m_UprateAggs.begin(); ita!=m_UprateAggs.end(); ++ita)
			if(it->second.m_Index==ita->m_VarIndex)
				break;
		if(ita!=m_UprateAggs.end())
			continue;

		if(warnIfNoFactor)
		{
			if (!noFactorVar.empty()) noFactorVar += ", ";
			noFactorVar += it->second.m_Name;
		}
		
		if(m_DefFactor != 1)
		{	
			CEMUprateVar UV;
			UV.m_VarIndex=it->second.m_Index;
			UV.m_Cond=NULL;
			UV.m_Factor=m_DefFactor;
			m_UprateVars.insert(m_UprateVars.end(), UV);
		}
	}
	if(!noFactorVar.empty())
		if(!CEMError::NonCritErr("No uprating factor defined for variable(s): "+noFactorVar, this, "", "", "Default factor is applied.")) return 0;

	//create this for the module not necessary parameter
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a");
}

bool CEMCM_Uprate::DoesReferToDataset()
{
	//old style: func_DataSet parameter 'uprate' defines a name, which corresponds to parameter 'name' of func_Uprate
	if (m_ParamAdmin.IsParamDefined("name"))
	{
		if (m_ParamAdmin.GetGeneralParam("name")->m_strValue != m_System->m_Uprate)
			return 0; //don't do anything if module belongs to another dataset
	}
	else
	{
		//new style: parameters 'dataset' defines to which datasets this func_Uprate refers
		Param_t dsPars;
		m_ParamAdmin.GetNotSingleParams("dataset", dsPars);
		Param_t::iterator itds;
		for (itds = dsPars.begin(); itds != dsPars.end(); ++itds)
			if (CEMUtilities::DoesValueMatchPattern((*itds)->m_strValue, m_Control->m_DataSet))
				break;
		if (itds == dsPars.end())
			return 0; //the function does not apply to the current dataset
	}
	return 1;
}

bool CEMCM_Uprate::Run(CEMHousehold *HH)
{
	if(!DoesReferToDataset())
		return 1;

	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	if (!m_addedToActiveList) { m_addedToActiveList = 1; m_System->m_ActiveUprateFunctions.push_back(this); }

	for(int p=0; p<(int)(HH->m_Persons.size()); ++p)
	{
		//create an artifical TU which is needed for some of the operations
		CEMTaxunit TU(m_Control, HH);
		CEMPerson pers;
		pers.m_Index=p; pers.m_IsDepChild=0;pers.m_IsDepPar=0;
		pers.m_IsHead=1; pers.m_IsLonePar=0; pers.m_IsPartner=0;
		TU.m_PersonRefs.insert(TU.m_PersonRefs.end(), pers);
		TU.m_HeadIndex=p; TU.m_ChildCnt=0; TU.m_AdultCnt=1;

		//before any uprating check whether aggregates are actually sum of their parts
		std::vector<CEMUprateAgg>::iterator ita;
		for(ita=m_UprateAggs.begin(); ita!=m_UprateAggs.end(); ++ita)
		{
			double sum=0;
			double agg = TU.GetVarVal(ita->m_VarIndex, &(HH->m_Persons), p);
			for(std::vector<int>::iterator itp=ita->m_PartIndex.begin(); itp!=ita->m_PartIndex.end(); ++itp)
			{
				double part = TU.GetVarVal((*itp), &(HH->m_Persons), p);
				if(part==EMVOID) { if(!CEMError::NonCritErr("Component of aggregate variable does not exist: "+m_Control->m_VarIndexV.at((*itp)).m_Name, this, m_Control->m_VarIndexV.at(ita->m_VarIndex).m_Name, "", "Note that an undefined value will be added instead (EMVOID=1E-13)!")) return 0; sum=EMVOID; break; }
				sum+=part;
			}
			if(sum!=EMVOID && (sum-agg>ita->m_Tolerance || sum-agg<ita->m_Tolerance*(-1)))
			{
				char err[500];
				double pid = TU.GetVarVal(m_Control->m_ivPID, &(HH->m_Persons), p);
				EM_SPRINTF(err, "IdPerson %.0f: Aggregate variable differs from sum of its parts: aggregate: %.2f, sum: %.2f", pid, agg, sum);
				if(!CEMError::NonCritErr(err, this, m_Control->m_VarIndexV.at(ita->m_VarIndex).m_Name)) return 0;
			}
		}

		//uprate variables
		for(std::vector<CEMUprateVar>::iterator itv=m_UprateVars.begin(); itv!=m_UprateVars.end(); ++itv)
		{
			if((HH->m_Persons)[p][itv->m_VarIndex]==EMVOID) if(!CEMError::NonCritErr("An undefined variable is uprated (EMVOID*factor?!)", this, m_Control->m_VarIndexV.at(itv->m_VarIndex).m_Name)) return 0;
			
			if(itv->m_Factor > NUMPOSINFINITE) //factor is indicated via a constant and the value still needs to be assessed
				itv->m_Factor = (HH->m_Persons)[p][(int)(itv->m_Factor-NUMPOSINFINITE)]; //get value of constant from first person, as it needs to be equal for everybody
			
			if (itv->m_Factor < 0) 
				if (itv->m_Factor == NUMNEGINFINITE)
				{
					if (!CEMError::NonCritErr("Variable is uprated with a negative factor due to the existance of an uprating index equal to 0.", this, m_Control->m_VarIndexV.at(itv->m_VarIndex).m_Name)) return 0;
				}
				else
					if (!CEMError::NonCritErr("Variable is uprated with a negative factor.", this, m_Control->m_VarIndexV.at(itv->m_VarIndex).m_Name)) return 0;
			if (itv->m_Factor == EMVOID) if (!CEMError::NonCritErr("Variable is uprated with an undefined factor.", this, m_Control->m_VarIndexV.at(itv->m_VarIndex).m_Name)) return 0;
			if(!itv->m_Cond)
				(HH->m_Persons)[p][itv->m_VarIndex] *= itv->m_Factor;
			else
			{
				double doup;
				if(!itv->m_Cond->Assess(doup, &TU, &(HH->m_Persons), p)) return 0;
				if(doup) (HH->m_Persons)[p][itv->m_VarIndex] *= itv->m_Factor;
			}
		}

		//sum up aggregates
		for(ita=m_UprateAggs.begin(); ita!=m_UprateAggs.end(); ++ita)
		{
			(HH->m_Persons)[p][ita->m_VarIndex]=0;
			for(std::vector<int>::iterator itp=ita->m_PartIndex.begin(); itp!=ita->m_PartIndex.end(); ++itp)
				(HH->m_Persons)[p][ita->m_VarIndex]+=TU.GetVarVal((*itp), &(HH->m_Persons), p);
		}
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_ChangeParam
********************************************************************************************/
bool CEMCM_ChangeParam::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int len=int(ParName.length());
	if (ParName.substr(0,5) == "param")
	{
		size_t i = ParName.find("_");
		if (i != std::string::npos)
		{
			if (Group.empty())
				Group = ParName.substr(5, i-5);
			Single=0;
			if (ParName.substr(std::max(len-3, 0)) == "_id")
			{
				ParName = "param_id";
				ParType=PTYPE_NAME;
			}
			else if (ParName.substr(std::max(len-7, 0)) == "_newval")
			{
				ParName = "param_newval";
				ParType=PTYPE_NAME;
			}
			else if (ParName.substr(std::max(len-8, 0)) == "_condval")
			{
				ParName = "param_condval";
				ParType=PTYPE_NAME;
			}
		}
	}
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_ChangeParam::ReadtimeChangeParam()
{
	if (!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off"))
		return 0;
	if (m_ParamAdmin.GetSwitchParam()->m_numValue==0)
		return 1; //don't check if module is switched off

	int run = EvalRunCond(1); if (!run) return 0; if (run==-1) return 1; //remark: calling EvalRunCond with ignoreLoopCount set to 1 means avoiding a warning:
																		 //loopcount is not yet available (at readtime) but will be evaluated in RuntimeChangeParam
	std::map<int,Param_t> cpParam;
	m_ParamAdmin.GetGroupParam(cpParam, "param_", 1); //note that the last parameter means that n/a parameters are allowed for, because it must be possible to set parameters to n/a
	for (std::map<int,Param_t>::iterator itM = cpParam.begin(); itM != cpParam.end(); ++itM)
	{
		CEMParam *Id = NULL, *newVal = NULL, *DoubleDef = NULL;
		bool isConditional = 0;
		for (Param_t::iterator itP = itM->second.begin(); itP != itM->second.end(); ++itP)
		{
			if ((*itP)->m_Name == "param_id")
			{
				if (!Id)
					Id = (*itP);
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
			else if ((*itP)->m_Name == "param_newval" || (*itP)->m_Name == "param_condval")
			{
				if (!newVal)
				{
					newVal = (*itP);
					isConditional = (*itP)->m_Name == "param_condval";
				}
				else
				{
					DoubleDef = (*itP);
					break;
				}
			}
		}
		if (DoubleDef)
		{
			if (!CEMError::NonCritErr("Multiple definition for change parameter group " + DoubleDef->m_Group + ".",
				DoubleDef, "Change parameter group " + DoubleDef->m_Group + " is ignored."))
				return 0;
			continue;
		}
		if (!Id)
		{
			if (!CEMError::CritErr("No parameter-id defined for change parameter group " + CEMUtilities::IntToStr(itM->first) + ".", this))
				return 0;
			continue;
		}
		if (!newVal)
		{
			if (!CEMError::CritErr("No change value defined for change parameter group " + CEMUtilities::IntToStr(itM->first) + ".", this))
				return 0;
			continue;
		}

		if (m_System->m_ParList.find(Id->m_strValue) != m_System->m_ParList.end())
		{
			CEMParam *ParToCh = m_System->m_ParList.find(Id->m_strValue)->second;
			if (!isConditional)
			{//parameter changes via 'newval' can be done directly
				if (!ParToCh->m_strValue.compare("off") && !newVal->m_strValue.compare("on"))
				{
					if(!CEMError::CritErr("func_ChangeParam cannot switch on policies/functions, which are switched off. (Hint: consider setting the switch to 'toggle'.)", this))
						return 0;
				}
				else
					for (std::multimap<std::string, CEMParam*>::iterator itMM = m_System->m_ParList.find(Id->m_strValue); itMM != m_System->m_ParList.end(); ++itMM)
					{//loop over parameters with the respective ID: in fact parameters are only not unique, if a policy is used as a reference (i.e. comes twice or more in the spine)
						if (itMM->first != Id->m_strValue)
							break; //break the loop once the ID is different (multimaps are sorted by key)
						itMM->second->m_strValue = newVal->m_strValue;
					}
			}
			else //parameter changes via 'condval' take place at run-time
			{//not allowed parameter changes:
				int len=int(ParToCh->m_Name.length());
				if (ParToCh->m_Module && (ParToCh->m_Module->m_Name == COMOD24 || //func_defvar
					ParToCh->m_Module->m_Name == COMOD24 || //func_defvar
					ParToCh->m_Module->m_Name == COMOD10 || //func_defil
					ParToCh->m_Module->m_Name == COMOD11 || //func_deftu
					ParToCh->m_Module->m_Name == COMOD12 || //func_uprate
					ParToCh->m_Module->m_Name == COMOD18 || //func_store
					ParToCh->m_Module->m_Name == COMOD19 || //func_restore
					ParToCh->m_Module->m_Name == COMOD23 || //func_totals
					ParToCh->m_Module->m_Name == COMOD27 || //func_setdefault
					(ParToCh->m_Module->m_Name == COMOD9 && ParToCh->m_Name.substr(std::max(len-7,0)) == "_amount") || //func_defconst
					(ParToCh->m_Module->m_Name == COMOD13 && //func_defoutput
						ParToCh->m_Name != "file" && ParToCh->m_Name != "append" && ParToCh->m_Name != "ndecimals" && ParToCh->m_Name != "multiplymonetaryby"))) 
				{
					if (!CEMError::NonCritErr("Parameters of function '" + ParToCh->m_Module->m_Name + 
						"' cannot be changed at runtime.", this, "paramX_condval", "", "Parameter change is ignored."))
						return 0;
				}
				else if (ParToCh->m_Module && ParToCh->m_Module->m_Name == COMOD8 && ParToCh->m_Name == "share" && //func_allocate_F210
					ParToCh->m_Module->m_ParamAdmin.IsParamDefined("output_var") &&
					ParToCh->m_strValue == ParToCh->m_Module->m_ParamAdmin.GetGeneralParam("output_var")->m_strValue)
				{
					if (!CEMError::NonCritErr("Parameter 'share' of function 'func_allocate_F210' cannot be changed at runtime if there it is also the output variable of the function.",
						this, "paramX_condval", "", "Parameter change is ignored."))
						return 0;
				}
				else
				{
					if (ParToCh->m_strValue == "off" && newVal->m_strValue == "on")
					{
						if (!CEMError::CritErr("func_changeparam cannot switch on policies/functions, which are switched off. (Hint: consider setting the switch to 'toggle'.)", this))
							return 0;
					}
					else
					{
						for (std::multimap<std::string, CEMParam*>::iterator itMM = m_System->m_ParList.find(Id->m_strValue); itMM != m_System->m_ParList.end(); ++itMM)
						{//loop over parameters with the respective ID: in fact parameters are only not unique, if a policy is used as a reference (i.e. comes twice or more in the spine)
							if (itMM->first != Id->m_strValue)
								break; //break the loop once the ID is different (multimaps are sorted by key)
							m_ParToCh.push_back(itMM->second);
							m_CondVals.push_back(newVal->m_strValue);
						}
						//if footnote is changed the respective formula or condition must be (artifically) changed too, to make the change happen
						if(ParToCh->m_Name.at(0) == '#')
						{
							size_t i = ParToCh->m_Name.substr(1).find_first_not_of("0123456789");
							std::string fn = ParToCh->m_Name.substr(0, i+1);
							for (std::multimap<std::string, CEMParam*>::iterator itp = m_System->m_ParList.begin(); itp != m_System->m_ParList.end(); ++itp)
							{
								if(itp->second->m_strValue.find(fn) != std::string::npos)
								{
									m_ParToCh.push_back(itp->second);
									m_CondVals.push_back(itp->second->m_strValue);
								}
							}
						}
					}
				}
			}
		}
		else
		{	//if not found under "normal" parameters, could still be a policy switch
			size_t p;
			for (p = 0; p<m_System->m_Pols.size(); ++p)
				if (m_System->m_Pols[p].m_Id == Id->m_strValue)
					break;
			if (p < m_System->m_Pols.size())
			{
				if (!m_System->m_Pols[p].m_Switch)
				{
					if (!CEMError::CritErr("func_changeparam cannot change the switch-parameter of switched off policies. (Hint: consider setting the switch to 'toggle'.)", this))
						return 0;
				}
				else
				{//change the switch of all modules which belong to this policy
					for (ModuleContainer_t::iterator itm = m_System->m_Modules.begin(); itm != m_System->m_Modules.end(); itm++)
					{
						if ((*itm)->m_PolName != m_System->m_Pols[p].m_Name)
							continue;
						CEMSwitchParam *sw = (*itm)->m_ParamAdmin.GetSwitchParam();
						if (!sw->m_numValue)
							continue;
						if (!isConditional)
						{
							sw->m_strValue = newVal->m_strValue;
							sw->Prepare();
						}
						else
						{
							m_ParToCh.push_back(sw);
							m_CondVals.push_back(newVal->m_strValue);
						}
					}
				}
			}
			else
			{
				//intermediate solution to enable mtr-calculations with xml-input:
				//file-name of default-output-function is the only referenced parameter-id in mtr-calculations
				//bool isMTRFilePar = 0;
				//if(!m_Control->m_InputTXT && newVal->m_Module->m_PolName.substr(0,3)=="mtr")
				//{
				//	for(std::map<std::string, CEMParam*>::iterator itP=m_System->m_ParList.begin(); itP!=m_System->m_ParList.end(); itP++)
				//	{
				//		CEMParam *potParToCh = (*itP).second;
				//		if(potParToCh->m_Module->m_PolName.find("_std")!=std::string::npos &&
				//				potParToCh->m_Module->m_PolName.find("_std_hh")==std::string::npos &&
				//				potParToCh->m_Module->m_Name.find("defoutput")!=std::string::npos &&
				//				potParToCh->m_Name=="file")
				//		{
				//			potParToCh->m_strValue = newVal->m_strValue;
				//			isMTRFilePar = 1;
				//			break;
				//		}		
				//	}
				//}

				//if(!isMTRFilePar)
				//{
					if (Id->m_strValue != "n/a" && !CEMError::CritErr("Unknown identifier '" + Id->m_strValue + "' (group " + CEMUtilities::IntToStr(itM->first) + ").", this))
						return 0; //n/a is allowed for above, but if id is set to n/a this would lead to this error message
				//}
			}
		}
	}
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_ChangeParam::RuntimeChangeParam()
{//this function is called only once (i.e. not for each hh)
	if (m_ParToCh.size() == 0)
		return 1;
	int run = EvalRunCond();
	if (!run)
		return 0;
	if (run==-1)
		return 1;
	for(size_t i = 0; i < m_ParToCh.size(); ++i)
	{
		m_ParToCh[i]->m_strValue = m_CondVals[i];
		if(!m_ParToCh[i]->Prepare())
			return 0;
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_Loop and CEMCM_UnitLoop
********************************************************************************************/
bool CEMCM_Loop::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("loop_id")) { ParType=PTYPE_NAME; Compulsory=1; }
	else if(!ParName.compare("first_pol")) ParType=PTYPE_NAME;
	else if(!ParName.compare("first_func")) ParType=PTYPE_NAME;
	else if(!ParName.compare("start_after_pol")) ParType=PTYPE_NAME;
	else if(!ParName.compare("start_after_func")) ParType=PTYPE_NAME;
	else if(!ParName.compare("last_pol")) ParType=PTYPE_NAME;
	else if(!ParName.compare("last_func")) ParType=PTYPE_NAME;
	else if(!ParName.compare("stop_before_pol")) ParType=PTYPE_NAME;
	else if(!ParName.compare("stop_before_func")) ParType=PTYPE_NAME;
	else if(m_Name==COMOD16 && !ParName.compare("num_iterations")) ParType=PTYPE_VAL;
	else if(m_Name==COMOD16 && !ParName.compare("breakcond")) ParType=PTYPE_COND;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Loop::FitIn(std::string &Start, bool &IsStartFunc, bool &StartAfter, std::string &Stop, bool &IsStopFunc, bool &StopBefore)
{
	std::vector<std::string> SkipParNames; SkipParNames.push_back("elig_unit_cond"); SkipParNames.push_back("breakcond");
	if(!m_ParamAdmin.PrepareSkip(SkipParNames)) return 0;

	if(!m_ParamAdmin.CheckCompParamExists("loop_id", PTYPE_NAME, this, "n/a")) return 0;
	m_LoopId = m_ParamAdmin.GetGeneralParam("loop_id")->m_strValue;

	std::string vn = "loopcount_"+m_LoopId; //generate loop counter
	if(!m_Control->GetVarIndex(vn, m_viLoopCount, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 1, 0)) { if(!CEMError::CritErr("Failed to create loop-counter '"+vn+"'.", this)) return 0; }

	bool op1 = m_ParamAdmin.IsParamDefined("first_pol");
	bool op2 = m_ParamAdmin.IsParamDefined("first_func");
	bool op3 = m_ParamAdmin.IsParamDefined("start_after_pol");
	bool op4 = m_ParamAdmin.IsParamDefined("start_after_func");
	if(!op1 && !op2 && !op3 && !op4) { if(!CEMError::CritErr("Loop start undefined. Use one of the parameters 'first_pol'/'first_func'/'start_after_pol'/'start_after_func'.", this)) return 0; }
	if(op1+op2+op3+op4>1) { if(!CEMError::CritErr("Multiple definition of loop start. Use only one of the parameters 'first_pol'/'first_func'/'start_after_pol'/'start_after_func'.", this)) return 0; }
	if(op1) { Start=m_ParamAdmin.GetGeneralParam("first_pol")->m_strValue; StartAfter=0; IsStartFunc=0; }
	if(op2) { Start=m_ParamAdmin.GetGeneralParam("first_func")->m_strValue; StartAfter=0; IsStartFunc=1; }
	if(op3) { Start=m_ParamAdmin.GetGeneralParam("start_after_pol")->m_strValue; StartAfter=1; IsStartFunc=0; }
	if(op4) { Start=m_ParamAdmin.GetGeneralParam("start_after_func")->m_strValue; StartAfter=1; IsStartFunc=1; }

	op1 = m_ParamAdmin.IsParamDefined("last_pol");
	op2 = m_ParamAdmin.IsParamDefined("last_func");
	op3 = m_ParamAdmin.IsParamDefined("stop_before_pol");
	op4 = m_ParamAdmin.IsParamDefined("stop_before_func");
	if(!op1 && !op2 && !op3 && !op4) { if(!CEMError::CritErr("Loop end undefined. Use one of the parameters 'last_pol'/'last_func'/'stop_before_pol'/'stop_before_func'.", this)) return 0; }
	if(op1+op2+op3+op4>1) { if(!CEMError::CritErr("Multiple definition of loop end. Use only one of the parameters 'last_pol'/'last_func'/'stop_before_pol'/'stop_before_func'.", this)) return 0; }
	if(op1) { Stop=m_ParamAdmin.GetGeneralParam("last_pol")->m_strValue; StopBefore=0; IsStopFunc=0; }
	if(op2) { Stop=m_ParamAdmin.GetGeneralParam("last_func")->m_strValue; StopBefore=0; IsStopFunc=1; }
	if(op3) { Stop=m_ParamAdmin.GetGeneralParam("stop_before_pol")->m_strValue; StopBefore=1; IsStopFunc=0; }
	if(op4) { Stop=m_ParamAdmin.GetGeneralParam("stop_before_func")->m_strValue; StopBefore=1; IsStopFunc=1; }

	if(m_Name==COMOD16) //standard loop
	{
		m_nIt=NUMPOSINFINITE;
		op1 = m_ParamAdmin.IsParamDefined("num_iterations");
		op2 = m_ParamAdmin.IsParamDefined("breakcond");
		if(!op1 && !op2) { if(!CEMError::CritErr("Number of loop-iterations undefined. Use either parameter 'num_iterations' or 'breakcond'.", this)) return 0; }
		if(op1+op2>1) { if(!CEMError::CritErr("Multiple definition of loop-iterations. Use only one of the parameters 'num_iterations' and 'breakcond'.", this)) return 0; }
		if(op1) m_nIt=m_ParamAdmin.GetValParam("num_iterations")->m_numValue;
	}

	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_Loop::Run(std::string &JumpTo)
{
	if(!m_IsLoopStart) return m_Twin->LoopEndAction(JumpTo);
	return LoopStartAction();
}

bool CEMCM_Loop::LoopStartAction()
{
	if(!m_OutOfLoop) return 1;
	m_OutOfLoop=m_Twin->m_OutOfLoop=0;
	return m_System->SetGlobalVar(m_viLoopCount, 1);
}

bool CEMCM_Loop::LoopEndAction(std::string &JumpTo)
{
	JumpTo="";
	int run = EvalRunCond(); if(!run) return 0; /*error*/ if(run==-1) return 1; //no run-cond or run-cond is false

	double Break, LoopCount;
	if(!m_System->GetGlobalVar(m_viLoopCount, LoopCount)) return 0;
	if(m_nIt<NUMPOSINFINITE) Break = (LoopCount>=m_nIt);
	else
	{
		m_ActTU = m_System->GetStaticTU();
		m_ActHHVar=&(m_ActTU->m_HH->m_Persons);

		CEMCondParam *bc=m_ParamAdmin.GetCondParam("breakcond");
		if(!bc->m_Formula) { if(!bc->Prepare()) return 0; }
		if(!bc->m_IsGlobal)  { if(!CEMError::NonCritErr("Loop condition is supposed to be global but contains personal/household related operands.", bc, "Conditions are evaluated for the first person of the first household.")) return 0; }
		if(!bc->Assess(Break, m_ActTU, m_ActHHVar)) return 0;
	}
	if(!Break)
	{
		if(!m_System->SetGlobalVar(m_viLoopCount, LoopCount+1)) return 0;
		JumpTo=m_Id;
	}
	else m_OutOfLoop=m_Twin->m_OutOfLoop=1;
	return 1;
}

bool CEMCM_UnitLoop::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("elig_unit")) { ParType=PTYPE_TU; Compulsory=1; }
	else if(!ParName.compare("elig_unit_cond")) ParType=PTYPE_COND;
	else if(!ParName.compare("elig_unit_cond_who")) ParType=PTYPE_CATEG;
	else if(!ParName.compare("run_once_if_no_elig")) ParType=PTYPE_YESNO;
	return CEMCM_Loop::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_UnitLoop::FitIn(std::string &Start, bool &IsStartFunc, bool &StartAfter, std::string &Stop, bool &IsStopFunc, bool &StopBefore)
{
	if(!CEMCM_Loop::FitIn(Start, IsStartFunc, StartAfter, Stop, IsStopFunc, StopBefore)) return 0;

	if(!m_ParamAdmin.CheckCompParamExists("elig_unit", PTYPE_TU, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("elig_unit_cond", PTYPE_COND, this, "{1}")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("elig_unit_cond_who", PTYPE_CATEG, this, "all")) return 0;
	CEMParam *pucw=m_ParamAdmin.GetGeneralParam("elig_unit_cond_who");
	if(!CheckWhoMustBeElig(pucw->m_strValue, pucw->m_numValue)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("run_once_if_no_elig", PTYPE_YESNO, this, "no")) return 0;

	std::string vn = "iscurelig_"+m_LoopId; //generate variable for currently eligible unit
	if(!m_Control->GetVarIndex(vn, m_viCurElig, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 0, 0)) { if(!CEMError::CritErr("Failed to create variable '"+vn+"'.", this)) return 0; }
	vn = "nulelig_"+m_LoopId; //generate variable for number of eligible units
	if(!m_Control->GetVarIndex(vn, m_viNULElig, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 0, 0)) { if(!CEMError::CritErr("Failed to create variable '"+vn+"'.", this)) return 0; }
	vn = "isulelig_"+m_LoopId; //generate variable for whether a unit is eligible or not
	if(!m_Control->GetVarIndex(vn, m_viIsULElig, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 0, 0)) { if(!CEMError::CritErr("Failed to create variable '"+vn+"'.", this)) return 0; }
	vn = "iseliginiter_"+m_LoopId; //generate variable storing in which iteration a unit is the "currently processed"
	if(!m_Control->GetVarIndex(vn, m_viIsEligInIter, CREATE_INTERNAL, VARTYPE_NONDESC, 0, 0, 0)) { if(!CEMError::CritErr("Failed to create variable '"+vn+"'.", this)) return 0; }

	m_nIt=0;
	return 1;
}

bool CEMCM_UnitLoop::LoopEndAction(std::string &JumpTo)
{
	if(!CEMCM_Loop::LoopEndAction(JumpTo)) return 0;
	if(m_OutOfLoop) m_System->m_viNULElig=-1;
	return 1;
}

bool CEMCM_UnitLoop::LoopStartAction()
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;
	if(!CEMCM_Loop::LoopStartAction()) return 0;
	double nIt; if(!m_System->GetGlobalVar(m_viLoopCount, nIt)) return 0;
	//loop over households
	for(HHContainer_t::iterator ith=m_System->m_Households.begin(); ith!=m_System->m_Households.end(); ith++)
	{
		m_ActHHVar=&(ith->m_Persons);
		//loop over taxunits
		CEMTUParam *tup=m_ParamAdmin.GetTUParam("elig_unit");
		if(!ith->AreTUsBuilt(tup->m_strValue)) tup->m_TUDefMod->BuildTUs(&(*ith), &(ith->m_Persons));
		TUContainer_t TUs = ith->GetTUsOfType(tup->m_strValue);
		if(nIt==1)
		{//find out number of "eligible" small units and set first "current eligible"	
			int nElig=0;
			for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
			{
				m_ActTU=&(*itt);
				CEMCondParam *suc=m_ParamAdmin.GetCondParam("elig_unit_cond");
				int Elig, EligType=(int)(m_ParamAdmin.GetGeneralParam("elig_unit_cond_who")->m_numValue); double dummy;
				if(!suc->m_Formula) { if(!suc->Prepare()) return 0; }
				if(!suc->Assess(m_Control->m_ivInternal2, dummy, m_ActTU, m_ActHHVar)) return 0;
				if(!GetTUElig(Elig, EligType, m_Control->m_ivInternal2)) return 0;
				m_System->m_viNULElig=m_viNULElig; //used in CEMSystem::Run to skip iterations if there is no "eligible" unit left to be processed
				m_System->m_viULoopCount=m_viLoopCount;
				for(PersonRefContainer_t::iterator itp=itt->m_PersonRefs.begin(); itp!=itt->m_PersonRefs.end(); itp++)
				{
					itt->OverwriteVarVal(Elig, m_viIsULElig, m_ActHHVar, itp->m_Index); //mark whether unit is eligible or not
					if(Elig && !nElig)
					{//set first (i.e. !nElig) "current eligible" unit
						itt->OverwriteVarVal(Elig, m_viCurElig, m_ActHHVar, itp->m_Index);
						itt->OverwriteVarVal(1, m_viIsEligInIter, m_ActHHVar, itp->m_Index);
					}
					else itt->OverwriteVarVal(0, m_viCurElig, m_ActHHVar, itp->m_Index);
				}
				nElig+=Elig;
			}
			if(nElig==0) { nElig+=(int)(m_ParamAdmin.GetYesNoParam("run_once_if_no_elig")->m_numValue); }
			for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
			{//set number of small units within household (obviousely to the same value for everyone)
				m_ActTU=&(*itt);
				double prevnElig=m_ActTU->GetVarVal(m_viNULElig, m_ActHHVar);
				if(prevnElig!=EMVOID && prevnElig!=nElig) { if(!CEMError::NonCritErr("'elig_unit_cond' does not deliver the same result in each iteration. Check if you really want to use components that change during runtime.", this)) return 0; }
				for(PersonRefContainer_t::iterator itp=m_ActTU->m_PersonRefs.begin(); itp!=m_ActTU->m_PersonRefs.end(); itp++)
					m_ActTU->OverwriteVarVal(nElig, m_viNULElig, m_ActHHVar, itp->m_Index);
			}
			if(nElig>m_nIt) m_nIt=nElig;
		}
		else
		{//set next "current eligible"
			bool found=0;
			for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
			{
				if(!found) //first find current eligible unit ...
				{
					if(itt->GetVarVal(m_viCurElig, m_ActHHVar))
					{
						for(PersonRefContainer_t::iterator itp=itt->m_PersonRefs.begin(); itp!=itt->m_PersonRefs.end(); itp++)
							itt->OverwriteVarVal(0, m_viCurElig, m_ActHHVar, itp->m_Index);
						found=1;
					}
				}
				else //then find next eligible unit and change the "current status"
				{
					if(itt->GetVarVal(m_viIsULElig, m_ActHHVar))
					{
						for(PersonRefContainer_t::iterator itp=itt->m_PersonRefs.begin(); itp!=itt->m_PersonRefs.end(); itp++)
						{
							itt->OverwriteVarVal(1, m_viCurElig, m_ActHHVar);
							itt->OverwriteVarVal(nIt, m_viIsEligInIter, m_ActHHVar);
						}
						break;
					}
				}
			}
		}
	}
	return m_ParamAdmin.CheckFootnoteParUsage();
}

/********************************************************************************************
 functions class CEMCM_Store
********************************************************************************************/
bool CEMCM_Store::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	int len=int(ParName.length());
	if (ParName == "postfix")
		ParType = PTYPE_NAME;
	else if (ParName == "postloop")
		ParType = PTYPE_NAME;
	else if (ParName == "level")
		ParType=PTYPE_TU;
	else if (ParName.substr(0,3) == "var")
	{
		if (Group.empty())
		{
			for (size_t c = 3; c < ParName.length() && ParName.substr(c,1) != "_"; ++c)
				Group += ParName.substr(c,1);
			Single = 0;
		}
		if (ParName.substr(std::max(len-6, 0)) == "_level")
		{
			ParName = "var_level";
			ParType = PTYPE_TU;
		}
		else
		{
			ParName = "var";
			ParType = PTYPE_VAR;
		}
	}
	else if (ParName.substr(0,2) == "il")
	{
		if (Group.empty())
		{
			for (size_t c = 2; c < ParName.length() && ParName.substr(c,1) != "_"; ++c)
				Group += ParName.substr(c,1);
			Single = 0;
		}
		if (ParName.substr(std::max(len-6, 0)) == "_level")
		{
			ParName = "il_level";
			ParType = PTYPE_TU;
		}
		else
		{
			ParName = "il";
			ParType = PTYPE_IL;
		}
	}

	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Store::CheckParam()
{
	if (!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off"))
		return 0;
	if (m_ParamAdmin.GetSwitchParam()->m_numValue == 0)
		return 1;
	if (!m_ParamAdmin.Prepare())
		return 0;
	
	std::string post = "", taxunit = "n/a"; bool isUnitLoop = 0;
	if (!m_ParamAdmin.IsParamDefined("postfix") && !m_ParamAdmin.IsParamDefined("postloop"))
	{
		if (!CEMError::CritErr("Neiter parameter 'postfix' nor 'postloop' defined.", this))
			return 0;
	}
	else if (m_ParamAdmin.IsParamDefined("postfix") && m_ParamAdmin.IsParamDefined("postloop"))
	{
		if (!CEMError::CritErr("Redundant definition: use either parameter 'postfix' or 'postloop'.", this))
			return 0;
	}
	else if (m_ParamAdmin.IsParamDefined("postfix"))
		post = m_ParamAdmin.GetGeneralParam("postfix")->m_strValue;
	else
	{
		post = m_ParamAdmin.GetGeneralParam("postloop")->m_strValue;
		for (LoopContainer_t::iterator itl = m_System->m_Loops.begin(); itl != m_System->m_Loops.end(); itl++)
		{
			if ((*itl)->m_Name != COMOD17)
				continue;
			CEMCM_UnitLoop *UL = (CEMCM_UnitLoop*)(*itl);
			if (UL->m_ParamAdmin.GetGeneralParam("loop_id")->m_strValue != post)
				continue;
			//poostloop with func_unitloop: function gets a taxunit (the 'small' unit) to be processed with the functions with a taxunit in CEMSystem::Run
			//(allows special treatment of variables in Run)
			taxunit = UL->m_ParamAdmin.GetTUParam("elig_unit")->m_strValue;
			isUnitLoop = 1;
			break;
		}
	}
	if (!isUnitLoop)
	{
		if (m_ParamAdmin.IsParamDefined("level"))
		{
			if (!CEMError::CritErr("Parameter 'level' can only be used with 'postloop' referring to func_unitloop.", this))
				return 0;
		}
	}
	
	std::vector<std::string> varNames; varNames.clear();
	std::vector<int> varInd; varInd.clear();
	m_viOr.clear(); m_vNames.clear(); m_viCop.clear(); m_ilPar.clear(); m_vLev.clear(); m_ilLev.clear();
	
	std::vector<CEMVarParam*> varParams;
	m_ParamAdmin.GetNotSingleParams("var", varParams);
	for (std::vector<CEMVarParam*>::iterator itv = varParams.begin(); itv != varParams.end(); ++itv)
	{
		varNames.push_back((*itv)->m_strValue);
		varInd.push_back((*itv)->m_VarIndex);
		if (isUnitLoop) 
		{
			if (m_ParamAdmin.IsParamDefined("var_level", (*itv)->m_Group))
				m_vLev.push_back(m_ParamAdmin.GetTUParam("var_level", (*itv)->m_Group)->m_strValue);
			else
				m_vLev.push_back("n/a");
		}
		else if (m_ParamAdmin.IsParamDefined("var_level", (*itv)->m_Group))
		{
			if (!CEMError::CritErr("Parameter 'var_level' can only be used with 'postloop' referring to func_unitloop.", this))
				return 0;
		}
	}

	std::vector<CEMILParam*> ilParams;
	m_ParamAdmin.GetNotSingleParams("il", ilParams);
	for (std::vector<CEMILParam*>::iterator iti = ilParams.begin(); iti != ilParams.end(); ++iti)
	{
		m_ilPar.push_back((*iti));
		if (isUnitLoop)
		{//only for unit loops: generate storage variable for incomelists: e.g. il1=ils_dispy -> generate ils_dispy_post
			int vi;
			if (!m_Control->GetVarIndex((*iti)->m_strValue + "_" + post, vi))
			{
				if (!m_Control->GetVarIndex((*iti)->m_strValue + "_" + post, vi, CREATE_INTERNAL, VARTYPE_NONDESC, 1, 0, 0))
				{
					if (!CEMError::CritErr("Failed to create variable '" + (*iti)->m_strValue + "_" + post + "'.", this))
						return 0;
				}
			}
			if (m_ParamAdmin.IsParamDefined("il_level", (*iti)->m_Group))
				m_ilLev.push_back(m_ParamAdmin.GetTUParam("il_level", (*iti)->m_Group)->m_strValue);
			else
				m_ilLev.push_back("n/a");
		}
		else if (m_ParamAdmin.IsParamDefined("il_level", (*iti)->m_Group))
		{
			if (!CEMError::CritErr("Parameter 'il_level' can only be used with 'postloop' referring to func_unitloop.", this))
				return 0;
		}
	}

	//generate storage variables: e.g. var1=yem, postfix/postloop=post -> generate yem_post
	//   postfix: Run will simply fill the variable, i.e. yem_post=yem
	//   postloop with normal loop: Run will fill the variable with the most recent value, i.e. yem_post=yem(of current iteration)
	//            yem_postX will be generated and filled in Run, i.e. generate yem_post1, yem_postX=yem(of X.iteration)
	//   postloop with unit loop: Run will fill the variable (more precisely constant) with the value of the currently processed unit (see Run)
	//            yem_postX will be treated as described for normal loop
	for (size_t j = 0; j < varNames.size(); ++j)
	{
		int vi=-1, sumtu=-1;
		if (isUnitLoop)
		{
			if (m_Control->m_VarIndexV[varInd[j]].m_Monetary)
				sumtu=0;
		}
		if (!m_Control->GetVarIndex(varNames[j] + "_" + post, vi))
		{
			if (!m_Control->GetVarIndex(varNames[j] + "_" + post, vi, CREATE_INTERNAL, VARTYPE_DERDESC, -1, -1, sumtu))
				return CEMError::CritErr("Failed to create variable '" + varNames[j] + "_" + post + "'.", this);
		}
		m_viOr.push_back(varInd[j]);
		m_viCop.push_back(vi);
		m_vNames.push_back(varNames[j]);
	}
	m_ilVarGen = 0;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, taxunit);
}

bool CEMCM_Store::Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar)
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	bool uloop=(TU!=NULL); bool pfix=0; std::string post="";
	if(m_ParamAdmin.IsParamDefined("postloop")) post=m_ParamAdmin.GetGeneralParam("postloop")->m_strValue;
	else { post=m_ParamAdmin.GetGeneralParam("postfix")->m_strValue; pfix=1; }
	
	//if postloop find out in which iteration we are
	double nIt=0;
	if(!pfix)
	{
		int vilpcnt;
		if(!m_Control->GetVarIndex("loopcount_"+post, vilpcnt)) { if(!CEMError::NonCritErr("Loop '"+post+"' does not exist or is currently not active.", this, "", "", "'postloop' is used as 'postfix'.")) return 0; }
		else { if(!m_System->GetGlobalVar(vilpcnt, nIt)) return 0; }
	}

	//generate remaining storage variables (i.e. those not already generated in CheckParam) ...
	//... variables contained in incomelists (e.g. postfix=post, il1=il_wky(=yem+yse) -> generate yem_post, yse_post)
	if(!m_ilVarGen)
	{
		for(size_t i=0; i<m_ilPar.size(); ++i)
		{
			std::vector<int>vis; int vi;
			if(!m_System->GetILContent(m_ilPar[i]->m_strValue, vis, m_ilPar[i])) return 0;
			for(std::vector<int>::iterator it=vis.begin(); it!=vis.end(); it++)
			{
				std::string vName=m_Control->m_VarIndexV[*it].m_Name+"_"+post;
				if(!m_Control->GetVarIndex(vName, vi))
					{ if(!m_Control->GetVarIndex(vName, vi, CREATE_INTERNAL, VARTYPE_DERDESC)) { if(!CEMError::CritErr("Failed to create variable '"+vName+"'.", this, "", "")) return 0; continue; } }
				m_viOr.insert(m_viOr.end(), (*it));
				m_viCop.insert(m_viCop.end(), vi);
			}
			//generate incomelist-definition (e.g. postfix=post, il1=il_wky(=yem+yse) -> generate il_wky_post=yem_post+yse_post)
			if(!StoreIL(m_ilPar[i]->m_strValue, "_"+post)) { if(!CEMError::NonCritErr("Failed to copy incomelist '"+m_ilPar[i]->m_strValue+"'.", this, "", "", "'"+m_ilPar[i]->m_strValue+post+"' is set to zero.")) return 0; }
		}
		m_ilVarGen=1; //variables only need to be generated once (i.e. not for each hh and not for each loop)
	}
	//... loop variables, i.e. postloop=post, var1=tin_s, il1=il_wky=(yem+yse), loopcount_post=2 
	//    -> generate variables tin_s_post2, yem_post2, yse_post2 and incomelist-defintion il_wky_post2
	std::vector<int> viCopX; viCopX.clear();
	if(!pfix)
	{//(is done for each hh (resp.taxunit), which is only necessary to fill viCopX, could be reconsidered if there are performance problems)
		for(size_t i=0; i<m_viCop.size(); ++i)
		{
			std::string vName=m_Control->m_VarIndexV[m_viCop[i]].m_Name+CEMUtilities::IntToStr(int(nIt));
			int vi=0;
			if(!m_Control->GetVarIndex(vName, vi))
				{ if(!m_Control->GetVarIndex(vName, vi, CREATE_INTERNAL, VARTYPE_DERDESC)) { if(!CEMError::CritErr("Failed to create variable '"+vName+"'.", this)) return 0; } }
			viCopX.insert(viCopX.end(), vi);
		}
		for(size_t j=0; j<m_ilPar.size(); ++j)
			{ if(!StoreIL(m_ilPar[j]->m_strValue+"_"+post, CEMUtilities::IntToStr(int(nIt)))) { if(!CEMError::NonCritErr("Failed to copy incomelist '"+m_ilPar[j]->m_strValue+"_"+post+"'.", this, "", "", "'"+m_ilPar[j]->m_strValue+"_"+post+CEMUtilities::IntToStr(int(nIt))+"' is set to zero.")) return 0; } }
	}

	//copy variables into storage variables
	for(size_t p=0; p<HHVar->size(); ++p)
	{
		if(!uloop) for(size_t i=0; i<m_viCop.size(); ++i) (*HHVar)[p][m_viCop[i]]=(*HHVar)[p][m_viOr[i]];
		for(size_t i=0; i<viCopX.size(); ++i) (*HHVar)[p][viCopX[i]]=(*HHVar)[p][m_viOr[i]];
	}

	//special handling for postloop referring to a unit loop: e.g. postloop=unit, var1=yem, il1=il_wky=(yem+yse)
	//constant yem_unit and constant il_wky_unit (both generated in CheckParam) are filled in each iteration with the value of the 'currently processed unit'
	if(!uloop) return 1;
	//assess whether taxunit is the 'currently processed unit'
	int vi;
	if(!m_Control->GetVarIndex("iscurelig_"+post, vi)) { if(!CEMError::NonCritErr("Not possible to assess variable 'IsCurElig_"+post+"'.", this)) return 0; }
	if(!TU->GetVarVal(vi, HHVar)) return 1; //not 'currently processed unit'
	//assess at which 'level' variables and incomelists are to be stored
	CEMTaxunit *RefTU=TU;
	if(m_ParamAdmin.IsParamDefined("level")) //all vars and ils are to be stored at another assessment level then 'elig_unit'
	{
		std::string altlev=m_ParamAdmin.GetTUParam("level")->m_strValue;
		if(!TU->m_HH->AreTUsBuilt(altlev)) { if(!CEMError::NonCritErr("Assessment unit '"+altlev+"' was not used before. func_store cannot change its level to a not yet used assessment unit.", this, "", "", "Parameter 'level' is ignored.")) return 0; }
		RefTU=TU->m_HH->GetContainingTU(TU->m_HeadIndex, altlev);
		if(!RefTU) { if(!CEMError::NonCritErr("Assessing assessment unit '"+altlev+"' failed.", this, "", "", "Alternative assessment unit is ignored.")) return 0; } // should not happen
	}
	//store variables
	size_t i;
	for(i=0; i<m_vNames.size(); ++i)
	{
		CEMTaxunit *bkupRefTU=RefTU;
		if(m_vLev[i]!="n/a")
		{//variable is to be stored at another assessment level then the other variables and incomelists
			if(!TU->m_HH->AreTUsBuilt(m_vLev[i])) { if(!CEMError::NonCritErr("Assessment unit '"+m_vLev[i]+"' was not used before. func_store cannot change its level to a not yet used assessment unit.", this, "", "", "Alternative assessment unit is ignored.")) return 0; }
			RefTU=TU->m_HH->GetContainingTU(TU->m_HeadIndex, m_vLev[i]);
			if(!RefTU) { if(!CEMError::NonCritErr("Assessing assessment unit '"+m_vLev[i]+"' failed.", this, "", "", "Alternative assessment unit is ignored.")) return 0; } // should not happen
		}
		int viOr=-1, viCop=-1;
		if(!m_Control->GetVarIndex(m_vNames[i], viOr)) { if(!CEMError::NonCritErr("Not possible to assess variable '"+m_vNames[i]+"'.", this, "", "", "'"+m_vNames[i]+"_"+post+"' cannot be filled correctly.")) return 0; } //should not happen
		if(!m_Control->GetVarIndex(m_vNames[i]+"_"+post, viCop))  { if(!CEMError::NonCritErr("Not possible to assess variable '"+m_vNames[i]+"_"+post+"'.", this, "", "", "'"+m_vNames[i]+"' cannot be stored correctly.")) return 0; } //should not happen
		TU->OverwriteVarVal(RefTU->GetVarVal(viOr, HHVar), viCop, HHVar);
		RefTU=bkupRefTU;
	}
	//store incomelists
	for(i=0; i<m_ilPar.size(); ++i)
	{
		CEMTaxunit *bkupRefTU=RefTU;
		if(m_ilLev[i]!="n/a")
		{//incomelist is to be stored at another assessment level then the other variables and incomelists
			if(!TU->m_HH->AreTUsBuilt(m_ilLev[i])) { if(!CEMError::NonCritErr("Assessment unit '"+m_ilLev[i]+"' was not used before. func_store cannot change its level to a not yet used assessment unit.", this, "", "", "Alternative assessment unit is ignored.")) return 0; }
			RefTU=TU->m_HH->GetContainingTU(TU->m_HeadIndex, m_ilLev[i]);
			if(!RefTU) { if(!CEMError::NonCritErr("Assessing assessment unit '"+m_ilLev[i]+"' failed.", this, "", "", "Alternative assessment unit is ignored.")) return 0; } // should not happen
		}
		int viCop=-1;
		if(!m_Control->GetVarIndex(m_ilPar[i]->m_strValue+"_"+post, viCop))  { if(!CEMError::NonCritErr("Not possible to assess variable '"+m_ilPar[i]->m_strValue+"_"+post+"'.", this, "", "", "'"+m_ilPar[i]->m_strValue+"' cannot be stored correctly.")) return 0; } //should not happen
		TU->OverwriteVarVal(RefTU->GetILVal(m_System->m_ILs[m_ilPar[i]->m_strValue], HHVar), viCop, HHVar);
		RefTU=bkupRefTU;
	}
	return 1;	
}

bool CEMCM_Store::StoreIL(std::string ILName, std::string post)
{
	ILContainer_t::iterator ito=m_System->m_ILs.find(ILName+post);
	if(ito!=m_System->m_ILs.end())
		{ if(!ito->second.empty()) return 1; m_System->m_ILs.erase(ito); }
	ito=m_System->m_ILs.find(ILName);
	if(ito==m_System->m_ILs.end()) return 0;
	
	IL_t ILCp; ILCp.clear(); std::vector<int> vis; vis.clear(); std::vector<double> facs; facs.clear();
	if(!m_System->GetILContent(ILName, vis, NULL, &facs)) return 0;
	for(size_t i=0; i<vis.size(); ++i)
	{
		CEMILEntry entry;
		entry.m_Factor=facs[i];
		std::string pn=ILName+post+"_entry"+CEMUtilities::IntToStr(int(i+1));
		if(!m_ParamAdmin.CheckOptParamExists(pn, PTYPE_VARIL, this, m_Control->m_VarIndexV[vis[i]].m_Name+post)) return 0;
		entry.m_Entry=m_ParamAdmin.GetVarILParam(pn);
		ILCp.insert(ILCp.end(), entry);
	}
	return m_System->AddIL(ILName+post, ILCp, this);
}

/********************************************************************************************
 functions class CEMCM_Restore
********************************************************************************************/
bool CEMCM_Restore::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("postfix")) ParType=PTYPE_NAME;
	else if(!ParName.compare("postloop")) ParType=PTYPE_NAME;
	else if(!ParName.compare("iteration")) ParType=PTYPE_VAL;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Restore::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;
	std::string pnpost="", post="";
	if(!m_ParamAdmin.IsParamDefined("postfix") && !m_ParamAdmin.IsParamDefined("postloop"))
		{ if(!CEMError::CritErr("Neiter of parameter 'postfix' nor parameter 'postloop' defined.", this)) return 0; }
	else if(m_ParamAdmin.IsParamDefined("postfix") && m_ParamAdmin.IsParamDefined("postloop"))
		{ if(!CEMError::CritErr("Redundant definition: use either parameter 'postfix' or 'postloop'.", this)) return 0; }
	else if(m_ParamAdmin.IsParamDefined("postfix")) { pnpost="postfix"; post=m_ParamAdmin.GetGeneralParam("postfix")->m_strValue; }
	else if(m_ParamAdmin.IsParamDefined("postloop")) { pnpost="postloop"; post=m_ParamAdmin.GetGeneralParam("postloop")->m_strValue; }
	if(m_ParamAdmin.IsParamDefined("iteration") && !m_ParamAdmin.IsParamDefined("postloop"))
		{ if(!CEMError::CritErr("Parameter 'iteration' makes only sense in combination with parameter 'postloop'.", this)) return 0; }
	m_Store=NULL;
	if(!post.empty())
	{//search for corresponding func_store
			for(ModuleContainer_t::iterator itm=m_System->m_Modules.begin(); itm!=m_System->m_Modules.end(); itm++)
			{
				if((*itm)->m_Name!=COMOD18) continue;
				CEMCM_Store *stm=(CEMCM_Store *)(*itm);
				if(!stm->m_ParamAdmin.IsParamDefined(pnpost)) continue;
				if(stm->m_ParamAdmin.GetGeneralParam(pnpost)->m_strValue!=post) continue;
				m_Store=stm; break;
			}
		if(!m_Store) { if(!CEMError::CritErr("No func_store with parameter '"+pnpost+"' set to '"+post+"' found.", this)) return 0; }
		if(m_Store && m_Store->m_Name==COMOD17) { if(!CEMError::CritErr("Loop with 'loop_id' '"+post+"' is a func_unitloop. func_restore cannot be used with unit loops.", this)) return 0; }
	}
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a");
}

bool CEMCM_Restore::Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar)
{
	TU; //just to avoid warning about unreferenced formal parameter
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;
	double ita=0;
	if(m_ParamAdmin.IsParamDefined("postloop") && m_ParamAdmin.IsParamDefined("iteration"))
		ita=m_ParamAdmin.GetValParam("iteration")->m_numValue;
	for(size_t p=0; p<HHVar->size(); ++p)
	{
		if(m_Store)
			for(size_t i=0; i<m_Store->m_viOr.size(); ++i)
			{
				if(!ita) { (*HHVar)[p][m_Store->m_viOr[i]]=(*HHVar)[p][m_Store->m_viCop[i]]; continue; }
				int vi=-1;
				std::string vn=m_Control->m_VarIndexV[m_Store->m_viOr[i]].m_Name;
				std::string vnl=vn+"_"+m_ParamAdmin.GetGeneralParam("postloop")->m_strValue+CEMUtilities::IntToStr(int(ita));
				if(!m_Control->GetVarIndex(vnl, vi)) { if(!CEMError::NonCritErr("Variable '"+vnl+"' not found.", this, "", "", "Variables '"+vn+"' will not be restored.")) return 0; continue;}
				(*HHVar)[p][m_Store->m_viOr[i]]=(*HHVar)[p][vi];
			}
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_DropKeepUnit
********************************************************************************************/
bool CEMCM_DropKeepUnit::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("drop_cond") && m_Keep==0) { ParType=PTYPE_COND; Compulsory=1; }
	else if(!ParName.compare("keep_cond") && m_Keep==1) { ParType=PTYPE_COND; Compulsory=1; }
	else if(!ParName.compare("drop_cond_who") && !m_Keep) ParType=PTYPE_CATEG;
	else if(!ParName.compare("keep_cond_who") && m_Keep) ParType=PTYPE_CATEG;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_DropKeepUnit::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;

	if(!m_Keep && !m_ParamAdmin.CheckCompParamExists("drop_cond", PTYPE_COND, this, "{0}")) return 0;
	if(m_Keep && !m_ParamAdmin.CheckCompParamExists("keep_cond", PTYPE_COND, this, "{0}")) return 0;
	std::string who = m_Keep ? "keep_cond_who" : "drop_cond_who";
	if(!m_ParamAdmin.CheckOptParamExists(who, PTYPE_CATEG, this, "one")) return 0;
	CEMParam *pwho=m_ParamAdmin.GetGeneralParam(who);
	if(!CheckWhoMustBeElig(pwho->m_strValue, pwho->m_numValue)) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("tax_unit", PTYPE_TU, this, "individual"+m_Control->m_Country)) return 0;
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool CEMCM_DropKeepUnit::Run()
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	std::map<int,int> delhh; delhh.empty(); int i=0;
	for(HHContainer_t::iterator ith=m_System->m_Households.begin(); ith!=m_System->m_Households.end(); ith++)
	{
		CEMTUParam *tup=m_ParamAdmin.GetTUParam();
		if(!ith->AreTUsBuilt(tup->m_strValue)) { if(!tup->m_TUDefMod->BuildTUs(&(*ith), &(ith->m_Persons))) return 0; }
		TUContainer_t TUs = ith->GetTUsOfType(tup->m_strValue);
		std::map<int,int> delpers; delpers.empty();
		for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
		{
			m_ActTU = &(*itt);
			m_ActHHVar = &(ith->m_Persons);
			std::string cond = m_Keep ? "keep_cond" : "drop_cond"; std::string who = cond+"_who";
			int fulfilled; double dummy;
			CEMCondParam *pcond=m_ParamAdmin.GetCondParam(cond);
			CEMParam *pwho=m_ParamAdmin.GetGeneralParam(who);
			if(!pcond->Assess(m_Control->m_ivInternal2, dummy, m_ActTU, m_ActHHVar)) return 0;
			if(!GetTUElig(fulfilled, (int)(pwho->m_numValue), m_Control->m_ivInternal2)) return 0;
			if((m_Keep && fulfilled) || (!m_Keep && !fulfilled)) continue;
			for(PersonRefContainer_t::iterator itp=m_ActTU->m_PersonRefs.begin(); itp!=m_ActTU->m_PersonRefs.end(); itp++)
				delpers[itp->m_Index*(-1)]=itp->m_Index; //remember persons for later removal
		}
		if(delpers.size()<ith->m_Persons.size())
		{//if one or more household members are kept remove persons from households which are not kept ...
			for(std::map<int,int>::iterator it=delpers.begin(); it!=delpers.end(); it++) ith->m_Persons.erase(ith->m_Persons.begin()+it->second);
			ith->m_Taxunits.clear(); //all taxunits need to be newly generated
		}
		else delhh[i*(-1)]=i; //... else remember household for later removal
		++i;
	}
	//remove households where no person is kept
	for(std::map<int,int>::iterator it=delhh.begin(); it!=delhh.end(); it++) m_System->m_Households.erase(m_System->m_Households.begin()+it->second);
	return 1;
}

/********************************************************************************************
 functions class CEMCM_ILVarOp
********************************************************************************************/
bool CEMCM_ILVarOp::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("operator_il")) { ParType=PTYPE_IL; Compulsory=1; }
	else if(!ParName.compare("operand")) ParType=PTYPE_FORMULA;
	else if(!ParName.compare("operand_il")) ParType=PTYPE_IL;
	else if(!ParName.compare("operand_factors")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("operation")) ParType=PTYPE_CATEG;
	else if(!ParName.compare("sel_var")) ParType=PTYPE_CATEG;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_ILVarOp::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;

	if(!m_ParamAdmin.CheckCompParamExists("operator_il", PTYPE_IL, this, "n/a")) return 0;
	double factorsAreOperands = 0;
	if(m_ParamAdmin.IsParamDefined("operand_factors"))
		factorsAreOperands = m_ParamAdmin.GetYesNoParam("operand_factors")->m_numValue;
	if(!m_ParamAdmin.CheckOptParamExists("operation", PTYPE_CATEG, this, "mul")) return 0;
	CEMParam *par=m_ParamAdmin.GetGeneralParam("operation");
	if(par->m_strValue!="mul" && par->m_strValue!="add" && par->m_strValue!="negtozero") { if(!CEMError::CritErr("Parameter must be set to either 'mul' or 'add' or 'negtozero'.", par)) return 0; }
	if(!m_ParamAdmin.IsParamDefined("operand") && !m_ParamAdmin.IsParamDefined("operand_il") && par->m_strValue!="negtozero" && factorsAreOperands == 0.0) { if(!CEMError::CritErr("Neither parameter 'operand' nor parameter 'operand_il' nor parameter 'operand_factors' defined.", this)) return 0; }
	if(m_ParamAdmin.IsParamDefined("operand") + m_ParamAdmin.IsParamDefined("operand_il") + factorsAreOperands > 1) { if(!CEMError::CritErr("Only one of the parameters 'operand', 'operand_il' and 'operand_factors' can be applied.", this)) return 0; }
	//the parameter operand_il is not described in EMM_Functions.doc as it does not really make sense unless one can define a "mulitplicator-incomelist", e.g. il_earns=yem,yse, il_mul=2,3; result: yem=yem*2, yse=yse*3
	//at some time it was possible to define a "mulitplicator-incomelist" (i.e. an incomelist with numerical entries), but this was skipped when 3-type-parameters were abandoned
	if(m_ParamAdmin.IsParamDefined("operand_il") && m_ParamAdmin.IsParamDefined("sel_var")) { if(!CEMError::CritErr("Parameter 'sel_var' cannot be used together with parameter 'operand_il'.", this)) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("sel_var", PTYPE_CATEG, this, "all")) return 0;
	par=m_ParamAdmin.GetGeneralParam("sel_var");
	if(par->m_strValue!="all" && par->m_strValue!="max" && par->m_strValue!="min" && par->m_strValue!="minpos") { if(!CEMError::CritErr("Parameter must be set to either 'all', 'max', 'min' or 'minpos'.", par)) return 0; }

	//check standard elig-parameters as CEMModule::CheckParam is not called
	if(!m_ParamAdmin.CheckOptParamExists("elig_var", PTYPE_VAR, this, "sel_s")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("who_must_be_elig", PTYPE_CATEG, this, "nobody")) return 0;
	par=m_ParamAdmin.GetGeneralParam("who_must_be_elig");
	if(!CheckWhoMustBeElig(par->m_strValue, par->m_numValue)) return 0; //in fact some of the generally allowed values do not make sense (e.g. all_adults)

	if(!m_ParamAdmin.CheckFootnoteParUsage()) return 0;

	//parameter TAX_UNIT must not be indicated as function always operates on individual level ...
	if(m_ParamAdmin.IsParamDefined("tax_unit")) { if(!CEMError::CritErr("Function does not take parameter 'TAX_UNIT' as it always operates on individual level.", this)) return 0; }
	//... but internally TAX_UNIT is set to tu_individual_cc
	std::string tuind="tu_individual_"+m_Control->m_Country;
	if(!m_System->GetTUDefModule(tuind)) tuind="individual_"+m_Control->m_Country;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, tuind);
}

bool CEMCM_ILVarOp::Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar)
{
	m_ActTU = TU;
	m_ActHHVar = HHVar;

	int run = EvalRunCond(); if(!run) return 0; /*error*/ if(run==-1) return 1; /* run condition not fulfilled */
	int Elig; if(!GetTUElig(Elig)) return 0; if(!Elig) return 1;

	std::vector<int> viOperator;
	CEMILParam *pOperator = m_ParamAdmin.GetILParam("operator_il");
	if(!m_System->GetILContent(pOperator->m_strValue, viOperator, pOperator)) return 0;
	std::string operation = m_ParamAdmin.GetGeneralParam("operation")->m_strValue;
	if(operation=="negtozero")
	{
		for(size_t i=0; i<viOperator.size(); ++i)
		{
			double vv = m_ActTU->GetVarVal(viOperator[i], m_ActHHVar);
			if(vv<0) m_ActTU->OverwriteVarVal(0, viOperator[i], m_ActHHVar);
		}
		return 1;
	}
	if(m_ParamAdmin.IsParamDefined("operand"))
	{
		std::string selvar = m_ParamAdmin.GetGeneralParam("sel_var")->m_strValue;
		double operand;
		if(!m_ParamAdmin.GetFormulaParam("operand")->Assess(operand, m_ActTU, m_ActHHVar)) return 0;
		double mm = (selvar=="max") ? NUMNEGINFINITE : NUMPOSINFINITE; int vi=-1;
		for(size_t i=0; i<viOperator.size(); ++i)
		{
			double vv = m_ActTU->GetVarVal(viOperator[i], m_ActHHVar);
			if(selvar=="all")
			{
				if(operation=="mul") m_ActTU->OverwriteVarVal(vv*operand, viOperator[i], m_ActHHVar);
				else m_ActTU->OverwriteVarVal(vv+operand, viOperator[i], m_ActHHVar);
			}
			else if(selvar=="min") { if(vv<mm) { mm=vv; vi=viOperator[i]; } }
			else if(selvar=="minpos") { if(vv>0 && vv<mm) { mm=vv; vi=viOperator[i]; } }
			else { if(vv>mm) { mm=vv; vi=viOperator[i]; } }
		}
		if(selvar!="all" && !(selvar=="minpos" && vi<0))
		{
			if(vi<0) return CEMError::CritErr("Determining highest/lowest variable of incomelist '"+pOperator->m_strValue+"' failed.", this); //should not happen
			if(operation=="mul") m_ActTU->OverwriteVarVal(mm*operand, vi, m_ActHHVar);
			else m_ActTU->OverwriteVarVal(mm+operand, vi, m_ActHHVar);
		}
	}
	else //operand_il is defined or the factors of the il are the operands
	{
		double factorsAreOperands = 0;
		if(m_ParamAdmin.IsParamDefined("operand_factors"))
			factorsAreOperands = m_ParamAdmin.GetYesNoParam("operand_factors")->m_numValue;

		std::vector<int> viOperand;
		std::vector<double> facOperand;
		CEMILParam *pOperand = factorsAreOperands ? pOperator : m_ParamAdmin.GetILParam("operand_il");
		if(!m_System->GetILContent(pOperand->m_strValue, viOperand, pOperand, &facOperand)) return 0;
		if(viOperator.size()!=viOperand.size())
		{
			char err[500];
			EM_SPRINTF(err, "Operator incomelist '%s' contains %d variable(s) while operand incomelist '%s' contains %d variable(s). Operation cannot be performed.", pOperator->m_strValue.c_str(), viOperator.size(), pOperand->m_strValue.c_str(), viOperand.size());
			return CEMError::CritErr(err, this);
		}
		std::vector<double>valOperator; std::vector<double>valOperand; valOperator.clear(); valOperand.clear();
		for(size_t i=0; i<viOperator.size(); ++i)
		{
			valOperator.insert(valOperator.end(), m_ActTU->GetVarVal(viOperator[i], m_ActHHVar));
			if(factorsAreOperands)
				valOperand.insert(valOperand.end(), facOperand[i]);
			else
				valOperand.insert(valOperand.end(), m_ActTU->GetVarVal(viOperand[i], m_ActHHVar));
		}
		for(size_t i=0; i<valOperator.size(); ++i)
		{
			if(operation=="mul") m_ActTU->OverwriteVarVal(valOperator[i]*valOperand[i], viOperator[i], m_ActHHVar);
			else m_ActTU->OverwriteVarVal(valOperator[i]+valOperand[i], viOperator[i], m_ActHHVar);
		}
	}
	return 1;
}

/********************************************************************************************
 functions class CEMCM_Totals
********************************************************************************************/
bool CEMCM_Totals::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.substr(0,6).compare("agg_il")) { if(ParVal=="n/a") return 1; ParName="agg_il"; if (!GetTotVarIndex(ParName, ParVal)) return 0; ParType=PTYPE_IL; }
	else if(!ParName.substr(0,7).compare("agg_var")) { if(ParVal=="n/a") return 1; ParName="agg_var"; if (!GetTotVarIndex(ParName, ParVal)) return 0; ParType=PTYPE_VAR; }
	else if(!ParName.compare("varname_sum")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_mean")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_count")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_poscount")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_negcount")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_median")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_decile")) ParType=PTYPE_NAME;
	else if(!ParName.compare("varname_quintile")) ParType=PTYPE_NAME;
	else if(!ParName.compare("incl_cond")) ParType=PTYPE_COND;
	else if(!ParName.compare("incl_cond_who")) ParType=PTYPE_CATEG;
	else if(!ParName.compare("use_weights")) ParType=PTYPE_YESNO;
	else if(!ParName.compare("weight_var")) ParType=PTYPE_VAR;
	if(!ParName.substr(0,8).compare("varname_")) { if(ParVal=="n/a") return 1; GetTotVarIndex(ParName, ParVal); ParType=PTYPE_NAME; }
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_Totals::GetTotVarIndex(std::string &ParName, std::string ParVal)
{
	if(!ParName.compare("agg_il") || !ParName.compare("agg_var"))
	{
		int vi, i; 
		char sn[50]; 
		EM_SPRINTF(sn, "%d", m_nIlVar); 
		ParName+=sn; ++m_nIlVar;
		for(i=0; i<8; ++i)
		{
			std::vector<int> *vec=NULL; std::string aggdef;
			switch(i)
			{
				case 0: vec=&m_viSum; aggdef="varname_sum"; break;
				case 1: vec=&m_viMean; aggdef="varname_mean"; break;
				case 2: vec=&m_viCount; aggdef="varname_count"; break;
				case 3: vec=&m_viPosCount; aggdef="varname_poscount"; break;
				case 4: vec=&m_viNegCount; aggdef="varname_negcount"; break;
				case 5: vec=&m_viMedian; aggdef="varname_median"; break;
				case 6: vec=&m_viMin; aggdef="varname_min"; break;
				case 7: vec=&m_viMax; aggdef="varname_max"; break;
			}
			if(m_ParamAdmin.IsParamDefined(aggdef))
			{
				std::string aggname = m_ParamAdmin.GetGeneralParam(aggdef)->m_strValue;
				if(!m_Control->GetVarIndex(aggname+"_"+ParVal, vi, CREATE_INTERNAL, VARTYPE_NONDESC, 1, 1, 0)) { if(!CEMError::CritErr("Failed to create aggregate variable '"+aggname+"_"+ParVal+"'.", this)) return 0; }
				vec->insert(vec->end(), vi);
			}
		}
		for(i=0; i<2; ++i)
		{
			std::vector<int*> *vec; std::string aggdef; int qd;
			if(i) { vec=&m_viDec; qd=9; aggdef="varname_decile"; } else { vec=&m_viQuint; qd=4; aggdef="varname_quintile"; }
			if(m_ParamAdmin.IsParamDefined(aggdef))
			{
				int *vvi=new int[qd]; std::string aggname = m_ParamAdmin.GetGeneralParam(aggdef)->m_strValue;
				for(int j=0; j<qd; ++j)
				{
					char sn[5]; 
					EM_SPRINTF(sn, "%d_", j+1);
					if(!m_Control->GetVarIndex(aggname+sn+ParVal, vi, CREATE_INTERNAL, VARTYPE_NONDESC, 1, 1, 0)) { if(!CEMError::CritErr("Failed to create aggregate variable '"+aggname+sn+ParVal+"'.", this)) return 0; }
					vvi[j]=vi;
				}
				vec->insert(vec->end(), vvi);
			}
		}
	}
	int len=int(ParName.length());
	if(!ParName.substr(0,8).compare("varname_"))
	{
		for(unsigned int i=0; i<m_nIlVar; ++i)
		{
			std::string varil; 
			char sni[50]; 
			EM_SPRINTF(sni, "agg_il%d", i); 
			char snv[50]; 
			EM_SPRINTF(snv, "agg_var%d", i); 
			int vi;
			if(m_ParamAdmin.IsParamDefined(sni)) varil = m_ParamAdmin.GetGeneralParam(sni)->m_strValue;
			else varil = m_ParamAdmin.GetGeneralParam(snv)->m_strValue;
			if(ParName.substr(std::max(len-6,0)).compare("decile") && ParName.substr(std::max(len-8,0)).compare("quintile"))
			{
				std::vector<int> *vec=NULL;
				if(!ParName.substr(std::max(len-3,0)).compare("sum")) vec=&m_viSum;
				if(!ParName.substr(std::max(len-4,0)).compare("mean")) vec=&m_viMean;
				if(!ParName.substr(std::max(len-5,0)).compare("count")) vec=&m_viCount;
				if(!ParName.substr(std::max(len-8,0)).compare("poscount")) vec=&m_viPosCount;
				if(!ParName.substr(std::max(len-8,0)).compare("negcount")) vec=&m_viNegCount;
				if(!ParName.substr(std::max(len-6,0)).compare("median")) vec=&m_viMedian;
				if(!ParName.substr(std::max(len-3,0)).compare("min")) vec=&m_viMin;
				if(!ParName.substr(std::max(len-3,0)).compare("max")) vec=&m_viMax;
				if(!m_Control->GetVarIndex(ParVal+"_"+varil, vi, CREATE_INTERNAL, VARTYPE_NONDESC, 1, 1, 0)) { if(!CEMError::CritErr("Failed to create aggregate variable '"+ParVal+"_"+varil+"'.", this)) return 0; }
				vec->insert(vec->end(), vi);
			}
			else
			{
				std::vector<int*> *vec; int qd;
				if(!ParName.substr(std::max(len-6,0)).compare("decile")) { vec=&m_viDec; qd=9; } else { vec=&m_viQuint; qd=4; }
				int *vvi=new int[qd];
				for(int j=0; j<qd; ++j)
				{
					char sn[5]; 
					EM_SPRINTF(sn, "%d_", j+1);
					if(!m_Control->GetVarIndex(ParVal+sn+varil, vi, CREATE_INTERNAL, VARTYPE_NONDESC, 1, 1, 0)) { if(!CEMError::CritErr("Failed to create aggregate variable '"+ParVal+sn+varil+"'.", this)) return 0; }
					vvi[j]=vi;
				}
				vec->insert(m_viDec.end(), vvi);
			}
		}
	}
	return 1;
}

CEMCM_Totals::~CEMCM_Totals()
{
	for(size_t d=0; d<m_viDec.size(); ++d) delete m_viDec[d];
	for(size_t q=0; q<m_viQuint.size(); ++q) delete m_viQuint[q];
}

bool CEMCM_Totals::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;

	if(!m_nIlVar) { if(!CEMError::CritErr("No variable or incomelist defined. Use parameters 'agg_ilX' or 'agg_varX'.", this)) return 0; }
	if(!m_ParamAdmin.IsParamDefined("varname_sum") && !m_ParamAdmin.IsParamDefined("varname_mean") &&
		 !m_ParamAdmin.IsParamDefined("varname_median") && !m_ParamAdmin.IsParamDefined("varname_decile") && !m_ParamAdmin.IsParamDefined("varname_quintile") &&
		 !m_ParamAdmin.IsParamDefined("varname_count") && !m_ParamAdmin.IsParamDefined("varname_poscount") && !m_ParamAdmin.IsParamDefined("varname_negcount") &&
		 !m_ParamAdmin.IsParamDefined("varname_max") && !m_ParamAdmin.IsParamDefined("varname_min"))
		{ if(!CEMError::CritErr("No variable for any aggregate defined. Use one of the 'varname_xxx' parameters (e.g. 'varname_sum'", this)) return 0; }
	if(!m_ParamAdmin.CheckOptParamExists("incl_cond", PTYPE_COND, this, "{1}")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("incl_cond_who", PTYPE_CATEG, this, "all")) return 0;
	CEMParam *par=m_ParamAdmin.GetGeneralParam("incl_cond_who");
	if(!CheckWhoMustBeElig(par->m_strValue, par->m_numValue)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("use_weights", PTYPE_YESNO, this, "yes")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("weight_var", PTYPE_VAR, this, "dwt")) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("tax_unit", PTYPE_TU, this, "tu_individual_"+m_Control->m_Country)) return 0;
	return m_ParamAdmin.CheckFootnoteParUsage();
}

bool WgtValCmp (CWeightedVal i, CWeightedVal j) { return (i.m_Val<j.m_Val); }

bool CEMCM_Totals::Run()
{
	int run = EvalRunCond(); if(!run) return 0; /*error*/ if(run==-1) return 1; /* run condition not fulfilled */
	double wgt=1;
	double *sum=new double[m_nIlVar], *count=new double[m_nIlVar], *poscount=new double[m_nIlVar], *negcount=new double[m_nIlVar], *min=new double[m_nIlVar], *max=new double[m_nIlVar];
	std::vector<CWeightedVal> *wgtVals = new std::vector<CWeightedVal>[m_nIlVar];
	for(unsigned int i=0; i<m_nIlVar; ++i)
	{
		sum[i]=0; count[i]=0; poscount[i]=0; negcount[i]=0; min[i]=NUMPOSINFINITE; max[i]=NUMNEGINFINITE;
		wgtVals[i].clear();
	}
	double usewgt=m_ParamAdmin.GetYesNoParam("use_weights")->m_numValue;

	for(HHContainer_t::iterator ith=m_System->m_Households.begin(); ith!=m_System->m_Households.end(); ith++)
	{
		CEMTUParam *tup=m_ParamAdmin.GetTUParam();
		if(!ith->AreTUsBuilt(tup->m_strValue)) { if(!tup->m_TUDefMod->BuildTUs(&(*ith), &(ith->m_Persons))) return 0; }
		TUContainer_t TUs = ith->GetTUsOfType(tup->m_strValue);
		for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
		{
			m_ActTU = &(*itt);
			m_ActHHVar = &(ith->m_Persons);
			int incl; double dummy;
			CEMCondParam *pcond=m_ParamAdmin.GetCondParam("incl_cond");
			CEMParam *pwho=m_ParamAdmin.GetGeneralParam("incl_cond_who");
			if(!pcond->Assess(m_Control->m_ivInternal2, dummy, m_ActTU, m_ActHHVar)) return 0;
			if(!GetTUElig(incl, (int)(pwho->m_numValue), m_Control->m_ivInternal2)) return 0;
			if(!incl) continue;
			if(usewgt) m_ParamAdmin.GetVarParam("weight_var")->Assess(wgt, m_ActTU, m_ActHHVar);
			for(unsigned int i=0; i<m_nIlVar; ++i)
			{
				double val; 
				char sni[50]; 
				EM_SPRINTF(sni, "agg_il%d", i); 
				char snv[50]; 
				EM_SPRINTF(snv, "agg_var%d", i);
				if(m_ParamAdmin.IsParamDefined(snv)) m_ParamAdmin.GetVarParam(snv)->Assess(val, m_ActTU, m_ActHHVar);
				else m_ParamAdmin.GetILParam(sni)->Assess(val, m_ActTU, m_ActHHVar);
				sum[i]+=(val*wgt); count[i]+=wgt; if(val>0) poscount[i]+=wgt; if(val<0) negcount[i]+=wgt;
				min[i]=std::min(min[i],val); max[i]=std::max(max[i],val);
				if(!m_viMedian.empty() || !m_viDec.empty() || !m_viQuint.empty())
				{
					CWeightedVal wgtVal; wgtVal.m_Val=val; wgtVal.m_Weight=wgt;
					wgtVals[i].insert(wgtVals[i].end(), wgtVal);
				}
			}
		}
	}
	for(size_t i=0; i<m_nIlVar; ++i)
	{
		if(m_viSum.size()>i) { if(!m_System->SetGlobalVar(m_viSum[i], sum[i])) return 0; }
		if(m_viCount.size()>i) { if(!m_System->SetGlobalVar(m_viCount[i], poscount[i]+negcount[i])) return 0; }
		if(m_viPosCount.size()>i) { if(!m_System->SetGlobalVar(m_viPosCount[i], poscount[i])) return 0; }
		if(m_viNegCount.size()>i) { if(!m_System->SetGlobalVar(m_viNegCount[i], negcount[i])) return 0; }
		if(m_viMin.size()>i) { if(!m_System->SetGlobalVar(m_viMin[i], min[i])) return 0; }
		if(m_viMax.size()>i) { if(!m_System->SetGlobalVar(m_viMax[i], max[i])) return 0; }
		if(m_viMean.size()>i)
		{
			if(!count) { if(!m_System->SetGlobalVar(m_viMean[i], EMVOID)) return 0; }
			else { if(!m_System->SetGlobalVar(m_viMean[i], sum[i]/count[i])) return 0; }
		}
		if(m_viMedian.size()>i)
		{
			if(!count[i]) { if(!m_System->SetGlobalVar(m_viMedian[i], EMVOID)) return 0; }
			else
			{
				std::sort(wgtVals[i].begin(), wgtVals[i].end(), WgtValCmp);
				double sw=0; size_t j;
				for(j=0; j<wgtVals[i].size(); ++j) { sw+=wgtVals[i][j].m_Weight; if(sw>=count[i]/2) break; }
				if(sw>count[i]/2 || j+1>=wgtVals[i].size()) { if(!m_System->SetGlobalVar(m_viMedian[i], wgtVals[i][j].m_Val)) return 0; }
				else  { if(!m_System->SetGlobalVar(m_viMedian[i], (wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2)) return 0; }
			}
		}
		if(m_viDec.size()>i)
		{
			if(!count[i]) { for(int d=0;d<9;++d) { if(!m_System->SetGlobalVar(m_viDec[i][d], EMVOID)) return 0; } }
			else
			{
				std::sort(wgtVals[i].begin(), wgtVals[i].end(), WgtValCmp);
				double sw=0, decPt[]={EMVOID,EMVOID,EMVOID,EMVOID,EMVOID,EMVOID,EMVOID,EMVOID,EMVOID};
				for(size_t j=0; j<wgtVals[i].size(); ++j)
				{
					sw+=wgtVals[i][j].m_Weight;
					if(decPt[0]==EMVOID && (sw>(count[i]/10)*1 || j+1>=wgtVals[i].size())) decPt[0]=wgtVals[i][j].m_Val;
					if(decPt[0]==EMVOID && (sw==(count[i]/10)*1 && j+1<wgtVals[i].size())) decPt[0]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[1]==EMVOID && (sw>(count[i]/10)*2 || j+1>=wgtVals[i].size())) decPt[1]=wgtVals[i][j].m_Val;
					if(decPt[1]==EMVOID && (sw==(count[i]/10)*2 && j+1<wgtVals[i].size())) decPt[1]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[2]==EMVOID && (sw>(count[i]/10)*3 || j+1>=wgtVals[i].size())) decPt[2]=wgtVals[i][j].m_Val;
					if(decPt[2]==EMVOID && (sw==(count[i]/10)*3 && j+1<wgtVals[i].size())) decPt[2]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[3]==EMVOID && (sw>(count[i]/10)*4 || j+1>=wgtVals[i].size())) decPt[3]=wgtVals[i][j].m_Val;
					if(decPt[3]==EMVOID && (sw==(count[i]/10)*4 && j+1<wgtVals[i].size())) decPt[3]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[4]==EMVOID && (sw>(count[i]/10)*5 || j+1>=wgtVals[i].size())) decPt[4]=wgtVals[i][j].m_Val;
					if(decPt[4]==EMVOID && (sw==(count[i]/10)*5 && j+1<wgtVals[i].size())) decPt[4]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[5]==EMVOID && (sw>(count[i]/10)*6 || j+1>=wgtVals[i].size())) decPt[5]=wgtVals[i][j].m_Val;
					if(decPt[5]==EMVOID && (sw==(count[i]/10)*6 && j+1<wgtVals[i].size())) decPt[5]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[6]==EMVOID && (sw>(count[i]/10)*7 || j+1>=wgtVals[i].size())) decPt[6]=wgtVals[i][j].m_Val;
					if(decPt[6]==EMVOID && (sw==(count[i]/10)*7 && j+1<wgtVals[i].size())) decPt[6]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[7]==EMVOID && (sw>(count[i]/10)*8 || j+1>=wgtVals[i].size())) decPt[7]=wgtVals[i][j].m_Val;
					if(decPt[7]==EMVOID && (sw==(count[i]/10)*8 && j+1<wgtVals[i].size())) decPt[7]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(decPt[8]==EMVOID && (sw>(count[i]/10)*9 || j+1>=wgtVals[i].size())) decPt[8]=wgtVals[i][j].m_Val;
					if(decPt[8]==EMVOID && (sw==(count[i]/10)*9 && j+1<wgtVals[i].size())) decPt[8]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
				}
				for(int d=0;d<9;++d) { if(!m_System->SetGlobalVar(m_viDec[i][d], decPt[d])) return 0; }
			}
		}
		if(m_viQuint.size()>i)
		{
			if(!count[i]) { for(int q=0;q<4;++q) { if(!m_System->SetGlobalVar(m_viQuint[i][q], EMVOID)) return 0; } }
			else
			{
				std::sort(wgtVals[i].begin(), wgtVals[i].end(), WgtValCmp);
				double sw=0, quPt[]={EMVOID,EMVOID,EMVOID,EMVOID};
				for(size_t j=0; j<wgtVals[i].size(); ++j)
				{
					sw+=wgtVals[i][j].m_Weight;
					if(quPt[0]==EMVOID && (sw>(count[i]/5)*1 || j+1>=wgtVals[i].size())) quPt[0]=wgtVals[i][j].m_Val;
					if(quPt[0]==EMVOID && (sw==(count[i]/5)*1 && j+1<wgtVals[i].size())) quPt[0]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(quPt[1]==EMVOID && (sw>(count[i]/5)*2 || j+1>=wgtVals[i].size())) quPt[1]=wgtVals[i][j].m_Val;
					if(quPt[1]==EMVOID && (sw==(count[i]/5)*2 && j+1<wgtVals[i].size())) quPt[1]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(quPt[2]==EMVOID && (sw>(count[i]/5)*3 || j+1>=wgtVals[i].size())) quPt[2]=wgtVals[i][j].m_Val;
					if(quPt[2]==EMVOID && (sw==(count[i]/5)*3 && j+1<wgtVals[i].size())) quPt[2]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
					if(quPt[3]==EMVOID && (sw>(count[i]/5)*4 || j+1>=wgtVals[i].size())) quPt[3]=wgtVals[i][j].m_Val;
					if(quPt[3]==EMVOID && (sw==(count[i]/5)*4 && j+1<wgtVals[i].size())) quPt[3]=(wgtVals[i][j].m_Val+wgtVals[i][j+1].m_Val)/2;
				}
				for(int q=0;q<4;++q) { if(!m_System->SetGlobalVar(m_viQuint[i][q], quPt[q])) return 0; }
			}
		}
	}
	delete sum; delete count; delete poscount; delete negcount; delete min; delete max;
	return 1;
}

/********************************************************************************************
 functions class CEMCM_RandSeed
********************************************************************************************/
bool CEMCM_RandSeed::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(!ParName.compare("seed")) ParType=PTYPE_VAL;
	else if(!ParName.compare("run_cond")) return CEMError::NonCritErr("Parameter 'run_cond' is not allowed with function 'func_randseed'.", this, ParName, ParVal, "Run condition is ignored.");
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_RandSeed::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("seed", PTYPE_VAL, this, "1")) return 0;
	return 1;
}

bool CEMCM_RandSeed::Run()
{
	srand(int(m_ParamAdmin.GetValParam("seed")->m_numValue));
	return 1;
}

/********************************************************************************************
 functions class CEMCM_SetDefault
********************************************************************************************/
bool CEMCM_SetDefault::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if (ParName == "name")
		ParType = PTYPE_NAME;
	else if (ParName == "dataset")
	{
		if (ParVal != "n/a")
			ParVal = CEMUtilities::AppendExtension(ParVal);
		ParType = PTYPE_NAME;
		Single = 0;
	}
	else if (ParName != "switch" && ParName != "run_cond")
	{
		if (ParVal == "n/a") return true; // changed August 2017: so far the variable was generated (as void) irrespectively of any switches (off / n/a / used dataset)
		char u[100]; ++m_nV;
		//first insert parameter for variable name ...
		EM_SPRINTF(u, "var%d_name", m_nV);
		if (!CEMModule::TakeParam(u, ParName, "op", Line, Column, Group, PTYPE_VAR, 0))
			return 0;
		//... then parameter for default value
		EM_SPRINTF(u, "var%d_defval", m_nV);
		ParName = u;
		ParType = PTYPE_FORMULA;
	}
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_SetDefault::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1; //don't check if module is switched off
	//deactivate this warning for the time being, as parameter is still widely used
	//if (m_ParamAdmin.IsParamDefined("name"))
	//	{ if(!CEMError::NonCritErr("Parameter 'name' has phase-out status. Please consider using parameter 'dataset' instead.", m_ParamAdmin.GetGeneralParam("name"))) return 0; }
	Param_t dsPars; m_ParamAdmin.GetNotSingleParams("dataset", dsPars);
	if (!m_ParamAdmin.IsParamDefined("name") && dsPars.size()==0)
		{ if(!CEMError::NonCritErr("Neither parameter 'name' nor parameter 'dataset' defined.", this)) return 0; }
	if(!m_ParamAdmin.Prepare()) return 0;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_SetDefault::Run(CEMHousehold *HH)
{
	//old style: func_DataSet parameter 'use_default' defines a name, which corresponds to parameter 'name' of func_SetDefault
	if (m_ParamAdmin.IsParamDefined("name"))
	{
		if (m_ParamAdmin.GetGeneralParam("name")->m_strValue != m_Control->m_UseDefault)
			return 1; //don't do anything if module belongs to another dataset
	}
	else
	{
		//new style: parameters 'dataset' defines to which datasets this func_SetDefault refers
		Param_t dsPars;
		m_ParamAdmin.GetNotSingleParams("dataset", dsPars);
		Param_t::iterator itds;
		for (itds = dsPars.begin(); itds != dsPars.end(); ++itds)
			if (CEMUtilities::DoesValueMatchPattern((*itds)->m_strValue, m_Control->m_DataSet))
				break;
		if (itds == dsPars.end())
			return 1; //the function does not apply to the current dataset
	}

	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	char sVN[50], sVD[50];
	for(int i=1;;++i)
	{
		EM_SPRINTF(sVN, "var%d_name", i);
		EM_SPRINTF(sVD, "var%d_defval", i);
		if(!m_ParamAdmin.IsParamDefined(sVN)) break;
		if(m_ParamAdmin.GetGeneralParam(sVD)->m_strValue=="n/a") continue;
		int vi=m_ParamAdmin.GetVarParam(sVN)->m_VarIndex;
		//there is no need to generate the variables as they are already generated (by CEMControl::GetVarIndex)
		//but need to take care that variables which exist in the dataset are not overwritten
		if(m_Control->m_VarIndexV[vi].m_GenType!=VARTYPE_DEFAULT && m_Control->m_VarIndexV[vi].m_GenType!=VARTYPE_DESC
			&& m_Control->m_VarIndexV[vi].m_GenType!=VARTYPE_DERDESC) continue; //VARTYPE_DEFAULT: overwrites possible default in variables.xls, VARTYPE_DESC/VARTYPE_DERDESC: variables existent in variables.xls but with no default value (n/a, EMVOID)
		for(int p=0; p<(int)(HH->m_Persons.size()); ++p)
		{
			CEMTaxunit *tu = m_System->GetDummyIndividualTU(p);
			double def = 0;
			if(!m_ParamAdmin.GetFormulaParam(sVD)->Assess(def, tu, &(HH->m_Persons)))
			{
				char err[500];
				double hh = tu->GetVarVal(m_Control->m_ivHHID, &(HH->m_Persons), tu->m_HeadIndex);
				EM_SPRINTF(err, "Household %f: unable to asses parameter.", hh);
				if(!CEMError::CritErr(err, m_ParamAdmin.GetFormulaParam(sVD)))
					return 0;
			}
			(HH->m_Persons)[p][vi]=def;
		}
	}
	return 1;
}

#ifdef _WIN32
/********************************************************************************************
 functions class CEMCM_CallProgramme
********************************************************************************************/
bool CEMCM_CallProgramme::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(ParName=="programme") { ParType=PTYPE_NAME; Compulsory=1; }
	else if(ParName=="path") ParType=PTYPE_NAME;
	else if(ParName.length() >= 8 && ParName.substr(0,8)=="argument") { ParName="argument"; ParType=PTYPE_NAME; Single=0; }
	else if(ParName=="repbyempath") ParType=PTYPE_NAME;
	else if(ParName=="wait") ParType=PTYPE_YESNO;
	else if(ParName=="unifyslash") ParType=PTYPE_YESNO;
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_CallProgramme::CheckParam()
{
	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("programme", PTYPE_NAME, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("path", PTYPE_NAME, this, "")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("repbyempath", PTYPE_NAME, this, "..\\")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("wait", PTYPE_YESNO, this, "no")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("unifyslash", PTYPE_YESNO, this, "yes")) return 0;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_CallProgramme::Run()
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;
	
	std::string Name=m_ParamAdmin.GetGeneralParam("programme")->m_strValue;
	std::string Path=m_ParamAdmin.GetGeneralParam("path")->m_strValue;
	if(Path == "n/a") Path.clear();
	std::string RepByEMPath=m_ParamAdmin.GetGeneralParam("repbyempath")->m_strValue;
	double Wait = m_ParamAdmin.GetYesNoParam("wait")->m_numValue;
	double UnifySlash = m_ParamAdmin.GetYesNoParam("unifyslash")->m_numValue;

	std::string Args="";
	std::vector<CEMParam*> argParams;
	m_ParamAdmin.GetNotSingleParams("argument", argParams);
	for(std::vector<CEMParam*>::iterator ita=argParams.begin(); ita!=argParams.end(); ++ita)
	{
		std::string Arg=CEMUtilities::Trim((*ita)->m_strValue);
		if(Arg=="n/a" || Arg.empty()) continue;
		Args+=Arg+" ";
	}
	Args=CEMUtilities::Trim(Args);

	std::string EMPath="";
	if (!m_Control->m_EMContentPath.empty()) //path to EM file structure is now passed by UI ...
		EMPath = m_Control->m_EMContentPath;
	else //... thus assessing storage place of executable is not necessary anymore
		EMPath = GetEMPath(); //EMPath is assessed via the location of the executable, using a function only available under Windows
	Path=CEMUtilities::Replace(Path, RepByEMPath, EMPath);
	Args=CEMUtilities::Replace(Args, RepByEMPath, EMPath);
	if (UnifySlash)
	{
		Path=CEMUtilities::Replace(Path, "/", "\\");
		Args=CEMUtilities::Replace(Args, "/", "\\");
	}
	Path=CEMUtilities::Replace(Path, "\\\\", "\\");
	Args=CEMUtilities::Replace(Args, "\\\\", "\\");

	if(!CallProgramme(Name, Args, Wait, Path)) return CEMError::NonCritErr("The execution of programme '"+Name+"' failed.", this);
	return 1;
}
#endif

/********************************************************************************************
 functions class CEMCM_DefInput
********************************************************************************************/
bool CEMCM_DefInput::TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group, int ParType, int Compulsory, int Single)
{
	if(ParName=="path") { ParType=PTYPE_NAME; Compulsory=1; }
	else if(ParName=="file") { ParType=PTYPE_NAME; Compulsory=1; }
	else if(ParName=="rowmergevar") { ParType=PTYPE_VAR; Compulsory=1; }
	else if(ParName=="colmergevar") ParType=PTYPE_VAR;
	else if(ParName=="inputvar") ParType=PTYPE_VAR;
	else if(ParName=="defaultifnomatch") ParType=PTYPE_VAL;
	else if(ParName=="ignorenrows") ParType=PTYPE_VAL;
	else if(ParName=="ignorencols") ParType=PTYPE_VAL;
	else if(ParName=="decsepcomma") ParType=PTYPE_YESNO;
	else if(ParName=="doranges") ParType=PTYPE_YESNO;
	else if(ParName=="multisystemuse") ParType=PTYPE_YESNO;
#ifdef _WIN32
	else if(ParName=="repbyempath") ParType=PTYPE_NAME;
#endif
	return CEMModule::TakeParam(ParName, ParVal, ParId, Line, Column, Group, ParType, Compulsory, Single);
}

bool CEMCM_DefInput::CheckParam()
{
	m_table=NULL;

	if(!m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, this, "off")) return 0;
	if(m_ParamAdmin.GetSwitchParam()->m_numValue==0) return 1;
	if(!m_ParamAdmin.Prepare()) return 0;

	if(!m_ParamAdmin.CheckCompParamExists("path", PTYPE_NAME, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("file", PTYPE_NAME, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckCompParamExists("rowmergevar", PTYPE_VAR, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("colmergevar", PTYPE_VAR, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("inputvar", PTYPE_VAR, this, "n/a")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("defaultifnomatch", PTYPE_VAL, this, STREMVOID)) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("ignorenrows", PTYPE_VAL, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("ignorencols", PTYPE_VAL, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("decsepcomma", PTYPE_YESNO, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("doranges", PTYPE_YESNO, this, "0")) return 0;
	if(!m_ParamAdmin.CheckOptParamExists("multisystemuse", PTYPE_YESNO, this, "1")) return 0;
#ifdef _WIN32
	if(!m_ParamAdmin.CheckOptParamExists("repbyempath", PTYPE_NAME, this, "..\\")) return 0;
#else
	if(!m_ParamAdmin.CheckOptParamExists("repbyempath", PTYPE_NAME, this, "../")) return 0;
#endif
	if(m_ParamAdmin.IsParamDefined("colmergevar") && !m_ParamAdmin.IsParamDefined("inputvar"))
		if(!CEMError::CritErr("Compulsory parameter not defined", this, "InputVar")) return 0;
	return m_ParamAdmin.CheckOptParamExists("tax_unit", PTYPE_TU, this, "n/a"); //create this for the module not necessary parameter
}

bool CEMCM_DefInput::GetTable()
{
	m_viRowMergeVar = m_ParamAdmin.GetVarParam("rowmergevar")->m_VarIndex;
	m_viColMergeVar = m_viInputVar = -1;
	if(m_ParamAdmin.IsParamDefined("colmergevar"))
	{
		m_viColMergeVar = m_ParamAdmin.GetVarParam("colmergevar")->m_VarIndex;
		m_viInputVar = m_ParamAdmin.GetVarParam("inputvar")->m_VarIndex;
	}

	std::string fileName = m_ParamAdmin.GetGeneralParam("file")->m_strValue;
	fileName = CEMUtilities::AppendExtension(fileName, "txt", 0); //if there is no extension, add .txt

	m_table = m_Control->GetInputTable(m_ParamAdmin.GetGeneralParam("path")->m_strValue,
				fileName,
				m_ParamAdmin.GetValParam("multisystemuse")->m_numValue==1,
				m_System->m_Name, //necessary for generating ID
				m_viRowMergeVar,
				(int)(m_ParamAdmin.GetValParam("ignorenrows")->m_numValue),
				(int)(m_ParamAdmin.GetValParam("ignorencols")->m_numValue),
				m_ParamAdmin.GetYesNoParam("decsepcomma")->m_numValue ? "," : ".",
				m_ParamAdmin.GetGeneralParam("repbyempath")->m_strValue,
				m_ParamAdmin.GetValParam("defaultifnomatch")->m_numValue,
				m_ParamAdmin.GetYesNoParam("doranges")->m_numValue==1,
				m_viColMergeVar!=-1); //lookup mode or not
	return m_table!=NULL;
}

bool CEMCM_DefInput::Run(PersonVarContainer_t *HHVar, bool IsLastHH)
{
	int run = EvalRunCond(); if(!run) return 0; if(run==-1) return 1;

	if(!m_table)
		if(!GetTable()) return 0; //errors are issued in the respective functions

	for (size_t p = 0; p < HHVar->size(); ++p)
	{
		//lookup mode: value one variable (m_viInputVar) is assessed by a column- and row-merge-value (m_viColMergeVar, m_viRowMergeVar)
		if(m_ParamAdmin.IsParamDefined("colmergevar"))
		{
			 if(!m_table->LookUp((*HHVar)[p][m_viRowMergeVar], (*HHVar)[p][m_viColMergeVar], (*HHVar)[p][m_viInputVar]))
			 {
				char err[500];
				EM_SPRINTF(err, "No match found for %s=%.2f / %s=%.2f.",
					m_Control->m_VarIndexV[m_viRowMergeVar].m_Name.c_str(), (*HHVar)[p][m_viRowMergeVar],
								m_Control->m_VarIndexV[m_viColMergeVar].m_Name.c_str(), (*HHVar)[p][m_viColMergeVar]);
				if((*HHVar)[p][m_viRowMergeVar] == EMVOID)
					 EM_SPRINTF(err, "%s Note: %s is void.", err, m_Control->m_VarIndexV.at(m_viRowMergeVar).m_Name.c_str());
				if((*HHVar)[p][m_viColMergeVar] == EMVOID)
					 EM_SPRINTF(err, "%s Note: %s is void.", err, m_Control->m_VarIndexV.at(m_viColMergeVar).m_Name.c_str());
				if(!CEMError::CritErr(err, this))
					return 0;
			 }
		}
		//'normal' mode: value of one or more variables (<m_table->m_viInputVars) is assessed by a row-merge-value (m_viRowMergeVar)
		else
		{
			std::vector<double> rowValues;
			if(!m_table->GetRow((*HHVar)[p][m_viRowMergeVar], rowValues))
			{
				char err[500];
				EM_SPRINTF(err, "No match found for %s=%.2f.", m_Control->m_VarIndexV[m_viRowMergeVar].m_Name.c_str(), (*HHVar)[p][m_viRowMergeVar]);
				if ((*HHVar)[p][m_viRowMergeVar] == EMVOID)
					EM_SPRINTF(err, "%s Note: %s is void.", m_Control->m_VarIndexV.at(m_viRowMergeVar).m_Name.c_str());
				if(!CEMError::CritErr(err, this))
					return 0;
			}
			for(size_t iVar=0; iVar<m_table->m_viInputVars.size(); iVar++)
				if(m_table->m_viInputVars[iVar] != m_viRowMergeVar)
					(*HHVar)[p][m_table->m_viInputVars[iVar]] = rowValues.at(iVar);
		}
	}

	//table can be released if the last household is reached, but only if there is no potential other use of its content by another system
	//MultiSystemUse is set to yes as a default, but can be switched off, if the content of the file changes between the usage of the systems or if memeory should be freed
	if(IsLastHH && !m_ParamAdmin.GetYesNoParam("multisystemuse")->m_numValue)
		m_Control->ReleaseInputTable(m_table);

	return 1;
}
