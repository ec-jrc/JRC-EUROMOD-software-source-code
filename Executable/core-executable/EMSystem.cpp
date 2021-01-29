#include <locale.h>
#include <algorithm>
#include "EMSystem.h"
#include "EMUtilities.h"
#include "EMControl.h"
#include "EMModule.h"

#ifdef FLEMOSI
#include <fstream> 
#include <iostream> 
#endif

/********************************************************************************************
 functions class CEMSystem
********************************************************************************************/
CEMSystem::CEMSystem(CEMControl *Control)
{
	m_Control=Control;
	m_OutputCurrencyEuro=-1;
	m_ParamCurrencyEuro=-1;
	m_ExchRate=0.0;
	m_Uprate="";
	m_ActiveUprateFunctions.clear();
	m_SpineOn = 0;
	m_StaticTU = NULL;
	m_DummyIndividualTU = NULL;
	m_HeadDefInc = "n/a";
	m_Private = "";
	m_viNULElig = -1;
	m_ActMod = NULL;
	m_AOControlName = "";
	m_AOSystemName = "";
	m_AOSystem = NULL;
	m_Modules.clear();
	m_Pols.clear();
	m_Year = "";
}

void CEMSystem::CleanUp()
{
	for(ModuleContainer_t::iterator itm=m_Modules.begin(); itm<m_Modules.end(); ++itm)
	{
		(*itm)->CleanUp();
		delete (*itm);
	}
	if(m_StaticTU) delete m_StaticTU;
	if(m_DummyIndividualTU) delete m_DummyIndividualTU;
}

bool CEMSystem::TakeControlParam(std::string ParName, std::string ParVal, int Line)
{
	bool err;
	if(!ParName.compare("currency_output"))
		{ if(!CEMUtilities::CheckCurrency(m_OutputCurrencyEuro, ParVal, m_Control->m_ControlName, Line, "currency_output", m_Name)) return 0; }
	else if(!ParName.compare("currency_param"))
		{ if(!CEMUtilities::CheckCurrency(m_ParamCurrencyEuro, ParVal, m_Control->m_ControlName, Line, "currency_param", m_Name)) return 0; }						
	else if(!ParName.compare("exch_rate_euro_to_nat"))
	{
		m_ExchRate = CEMUtilities::StrToDouble(ParVal, &err);
		if(err) return CEMError::CritErr("Parameter is not a valid number.", m_Control->m_ControlName+", line "+CEMUtilities::IntToStr(Line), m_Name, "", "", ParName, ParVal);
		if(!m_ExchRate) return CEMError::CritErr("Exchange rate cannot be set to zero.", m_Control->m_ControlName+", line "+CEMUtilities::IntToStr(Line), m_Name, "", "", ParName, ParVal);
	}
	else if(!ParName.compare("uprate"))
	{
		m_Uprate=ParVal;
		if(m_Uprate=="n/a" || m_Uprate=="no") m_Uprate.clear();
	}
	else if(!ParName.compare("headdefinc")) m_HeadDefInc = ParVal;
	else if(!ParName.compare("private")) m_Private = ParVal; //only for migration purposes
	return 1;
}

bool CEMSystem::TakeSpineParam(std::string ParName, std::string ParVal, std::string ParId, int Line)
{
	//create artificial taxunit for miscellaneous use
	if(!m_StaticTU)
	{
		m_StaticTU = new CEMTaxunit(m_Control, &(m_Households[0]));
		m_StaticTU->m_HeadIndex=0;
		CEMPerson p;
		p.m_Index=0;
		p.m_IsHead=1;
		m_StaticTU->m_PersonRefs.insert(m_StaticTU->m_PersonRefs.end(), p);
	}
	//create dummy taxunit, which is "indvidualised" in GetDummyIndividualTU
	if(!m_DummyIndividualTU)
		m_DummyIndividualTU = new CEMTaxunit(m_Control, &(m_Households[0]));

	if(!CEMUtilities::LCase(ParName).compare("func_spine")) //LCase: for migration need case sensitivity, normally anyway lowercase
	{
		if(!ParVal.compare("off")) { m_SpineOn=0; return 1; }
		if(!ParVal.compare("on"))
		{
			if(m_Pols.size()==0) { m_SpineOn=1; return 1; }
			return CEMError::CritErr("Only one active 'spine' is allowed.", m_Control->m_SpineName+", line "+CEMUtilities::IntToStr(Line), m_Name, "", "", ParName, ParVal);
		}
		return CEMError::CritErr("'Spine' must be set to on or off.", m_Control->m_SpineName+", line "+CEMUtilities::IntToStr(Line), m_Name, "", "", ParName, ParVal);
	}
	if(!m_SpineOn) return 1;

	double sw; if(!CEMUtilities::CheckOnOff(sw, NULL, ParVal, m_Control->m_SpineName, m_Name, ParName)) return 0;
	CEMPolicy pol; pol.m_Name=ParName; pol.m_Switch=(int)(sw); pol.m_Order=Line;
	pol.m_Id=CEMUtilities::GenId(m_Control->m_SpineName, ParId, Line);
	m_Pols.insert(m_Pols.end(), pol);
	return 1;
}

bool CEMSystem::TakePolParam(unsigned int pnum, std::string ParName, std::string ParVal, std::string ParId, int Line, std::string ParGroup, int *AddOnInsInd)
{
	std::string lcParName = CEMUtilities::LCase(ParName); //for migration need case sensitivity, normally is anyway lower case
	ParVal=CEMUtilities::Replace(ParVal, "=sys=", m_Name);
	if(!m_Pols[pnum].m_Switch) return 1; //don't read parametes if policy is switched off
	if(!lcParName.substr(0,5).compare("func_"))
	{
		if(!lcParName.compare(COMOD1))
			m_ActMod = new CEMCM_ArithOp();
		else if(!lcParName.compare(COMOD2))
			m_ActMod = new CEMCM_Elig();
		else if(!lcParName.compare(COMOD3))
			m_ActMod = new CEMCM_BenCalc();
		else if(!lcParName.compare(COMOD4))
			m_ActMod = new CEMCM_SchedCalc();
		else if(!lcParName.compare(COMOD5))
			m_ActMod = new CEMCM_MiniMaxi();
		else if(!lcParName.compare(COMOD6))
			m_ActMod = new CEMCM_Min();
		else if(!lcParName.compare(COMOD7))
			m_ActMod = new CEMCM_Max();
		else if(!lcParName.compare(COMOD8))
			m_ActMod = new CEMCM_Allocate_F210(); //!!! PHASE-OUT !!!
		else if(!lcParName.compare(COMOD25))
			m_ActMod = new CEMCM_Allocate();
		else if(!lcParName.compare(COMOD9) || !lcParName.compare(COMOD24))
			m_ActMod = new CEMCM_DefVar();
		else if(!lcParName.compare(COMOD10))
			m_ActMod = new CEMCM_DefIL();
		else if(!lcParName.compare(COMOD11))
			m_ActMod = new CEMCM_DefTU();
		else if(!lcParName.compare(COMOD12))
			m_ActMod = new CEMCM_Uprate();
		else if(!lcParName.compare(COMOD13))
			m_ActMod = new CEMCM_DefOutput();
		else if(!lcParName.compare(COMOD14))
			m_ActMod = new CEMCM_UpdateTU();
		else if(!lcParName.compare(COMOD15))
			m_ActMod = new CEMCM_ChangeParam();
		else if(!lcParName.compare(COMOD16))
			m_ActMod = new CEMCM_Loop();
		else if(!lcParName.compare(COMOD17))
			m_ActMod = new CEMCM_UnitLoop();
		else if(!lcParName.compare(COMOD18))
			m_ActMod = new CEMCM_Store();
		else if(!lcParName.compare(COMOD19))
			m_ActMod = new CEMCM_Restore();
		else if(!lcParName.compare(COMOD20))
			m_ActMod = new CEMCM_DropKeepUnit(0);
		else if(!lcParName.compare(COMOD21))
			m_ActMod = new CEMCM_DropKeepUnit(1);
		else if(!lcParName.compare(COMOD22))
			m_ActMod = new CEMCM_ILVarOp();
		else if(!lcParName.compare(COMOD23))
			m_ActMod = new CEMCM_Totals();
		else if(!lcParName.compare(COMOD26))
			m_ActMod = new CEMCM_RandSeed();
		else if(!lcParName.compare(COMOD27))
			m_ActMod = new CEMCM_SetDefault();
#ifdef _WIN32
		else if(!lcParName.compare(COMOD28))
			m_ActMod = new CEMCM_CallProgramme();
#endif
		else if(!lcParName.compare(COMOD29))
			m_ActMod = new CEMCM_DefInput();
		else
		{
			if(!m_Pols[pnum].m_Switch) m_ActMod = new CEMModule; //happens during migration (m_Control->m_Mig==1): switched off policy with unknown function (e.g. mtr)
			else return CEMError::CritErr("Unknown function name.", m_Name+", line "+CEMUtilities::IntToStr(Line), m_Name, m_Pols[pnum].m_Name, lcParName);
		}
		m_ActMod->Init(ParName, this, m_Pols[pnum].m_Name, ParId, Line);
		m_ActMod->m_PolOrder = m_Pols[pnum].m_Order; //needs refactoring
		if(lcParName.compare(COMOD16) && lcParName.compare(COMOD17))
		{
			if(!AddOnInsInd || (*AddOnInsInd)==-1) m_Modules.insert(m_Modules.end(), m_ActMod);
			else { m_Modules.insert(m_Modules.begin()+(*AddOnInsInd), m_ActMod); ++(*AddOnInsInd); }
		}
		else m_Loops.insert(m_Loops.end(), (CEMCM_Loop *)m_ActMod);

		double sw;
		if(!CEMUtilities::CheckOnOff(sw, NULL, ParVal, m_Control->m_SpineName, m_Name, m_ActMod->m_PolName, m_ActMod->m_Name)) return 0;
		if(m_Pols[pnum].m_Switch==2 && sw==1) ParVal="2"; //set module switch from on to toggle if policy switch is set to toggle
		return m_ActMod->TakeParam("switch", ParVal, ParId, Line, m_ParamValColumn);
	}
	if(!m_ActMod) return CEMError::CritErr("Misplaced parameter (allocation to function not possible).", m_Name+", line "+CEMUtilities::IntToStr(Line), m_Name, m_Pols[pnum].m_Name, "", ParName, ParVal);
	if(!m_ActMod->TakeParam(ParName, ParVal, ParId, Line, m_ParamValColumn, ParGroup)) return 0;
	return 1;
}

bool CEMSystem::CheckParam()
{
	if(m_OutputCurrencyEuro==-1)
	{
		if(!CEMError::NonCritErr("Curreny for output not defined (Euro is used as default).", m_Control->m_ControlName, m_Name, "", "", "currency_output")) return 0;
		m_OutputCurrencyEuro=1;
	}
	if(m_ParamCurrencyEuro==-1)
	{
		if(!CEMError::NonCritErr("Curreny of parameters not defined (Euro is used as default).", m_Control->m_ControlName, m_Name, "", "", "currency_param")) return 0;
		m_ParamCurrencyEuro=1;
	}
	if(m_ExchRate==0.0)
	{
		if(!CEMError::NonCritErr("Exchange rate from euro to national curreny not defined (1 is used as default).", m_Control->m_ControlName, m_Name, "", "", "exch_rate_euro_to_nat")) return 0;
		m_ExchRate=1;
	}
	if(m_HeadDefInc=="n/a")
	{
		if(!CEMError::NonCritErr("Income for definition of head of unit is not defined ('ils_origy' is used as default).", m_Control->m_ControlName, m_Name, "", "", "headdefinc")) return 0;
		m_HeadDefInc="ils_origy";
	}
	if(!LoopFitIn()) return 0; //put loops in appropriate position in spine
	//bool uprfound=m_Uprate.empty();
	//stuff that needs to be done before "normal" parameter checking
	ModuleContainer_t::iterator itm;
	for(itm=m_Modules.begin(); itm<m_Modules.end(); ++itm)
	{
		//readtime parameter changes take place before parameters checking, i.e. the already changed parameters are checked
		if((*itm)->m_Name==COMOD15) { if(!((CEMCM_ChangeParam*)(*itm))->ReadtimeChangeParam()) return 0; }
		//check if func_uprate indicated by parameter uprate of func_datset exists
		/*if((*itm)->m_Name==COMOD12)
		{
			if(!(*itm)->m_ParamAdmin.GetSwitchParam()->m_numValue) continue;
			if(!(*itm)->m_ParamAdmin.IsParamDefined("name")) continue;
			if((*itm)->m_ParamAdmin.GetGeneralParam("name")->m_strValue==m_Uprate) uprfound=1;
		}*/
	}
	//if(!uprfound) { if(!CEMError::CritErr("No (active) func_uprate named '"+m_Uprate+"' exists.", m_Control->m_ControlName, m_Name, "", "", "uprate")) return 0; }	
	//"normal" parameter checking
	for(itm=m_Modules.begin(); itm<m_Modules.end(); ++itm) { if(!(*itm)->CheckParam()) return 0; }
	return 1;
}

bool CEMSystem::IsILDefined(std::string ILName)
{
	if(m_ILs.find(ILName)!=m_ILs.end()) return 1;

	//care about loop-incomelists applied by func_store (if used e.g. in func_defoutput before they are generated by func_store)
	//assume that a not defined incomelist ending with a digit is a loop-il (e.g. ils_dispy_mtr1)
	//in this case add an empty il-definition for the moment, to be filled by func_store later
	ILContainer_t::iterator it;
	for(it=m_ILs.begin(); it!=m_ILs.end(); ++it)
		if(it->first+"_"==ILName.substr(0,it->first.length()+1)) break;
	if(it==m_ILs.end()) return 0;

	IL_t ILDef; ILDef.clear();
	m_ILs.insert(std::pair<std::string,IL_t>(ILName, ILDef));
	return 1;
}

bool CEMSystem::AddIL(std::string ILName, IL_t &ILDef, CEMModule *Module)
{
	if(m_ILs.find(ILName)!=m_ILs.end()) return CEMError::CritErr("Incomelist '"+ILName+"' already defined.", CEMUtilities::IdToLocation(Module->m_Id), m_Name, Module->m_PolName, Module->m_Name);
	m_ILs.insert(std::pair<std::string,IL_t>(ILName, ILDef));
	return 1;
}

bool CEMSystem::GetILContent(std::string ILName, std::vector<int> &VarInd, CEMParam *Par, std::vector<double> *Fac, double CurFac)
{//get "expanded" incomelist, i.e. the indexes of all actually included variables (even if they are indicated as ils)
	if(!IsILDefined(ILName)) return CEMError::CritErr("Undefined incomelist '"+ILName+"'.", CEMUtilities::IdToLocation(Par->m_Id), Par->m_System->m_Name, Par->m_Module->m_PolName, Par->m_Module->m_Name, Par->m_Name, Par->m_strValue);
	IL_t ILDef = m_ILs.find(ILName)->second;
	for(IL_t::iterator it=ILDef.begin(); it!=ILDef.end(); ++it)
	{
		CEMVarILParam *p = it->m_Entry;
		if(p->m_viType==VITYPE_VAR)
		{
			if(std::find(VarInd.begin(), VarInd.end(), p->m_VarIndex)==VarInd.end())
			{
				VarInd.insert(VarInd.end(), p->m_VarIndex);
				if(Fac) Fac->insert(Fac->end(), it->m_Factor*CurFac);
			}
			continue;
		}
		if(p->m_viType==VITYPE_IL)
		{
			if(!IsILDefined(p->m_strValue)) return CEMError::CritErr("Undefined incomelist '"+ILName+"'.", CEMUtilities::IdToLocation(Par->m_Id), Par->m_System->m_Name, Par->m_Module->m_PolName, Par->m_Module->m_Name, Par->m_Name, Par->m_strValue);
			if(!GetILContent(p->m_strValue, VarInd, Par, Fac, it->m_Factor*CurFac)) return 0;
		}
	}
	return 1;
}

CEMCM_DefTU *CEMSystem::GetTUDefModule(std::string TUName)
{
	for(ModuleContainer_t::iterator itm=m_Modules.begin(); itm!=m_Modules.end(); itm++)
	{
		if((*itm)->m_Name==COMOD11)
		{
			CEMCM_DefTU *Mod=(CEMCM_DefTU *)(*itm);
			if(Mod->m_ParamAdmin.CheckCompParamExists("name", PTYPE_NAME, Mod, "n/a") &&
				!Mod->m_ParamAdmin.GetGeneralParam("name")->m_strValue.compare(TUName))
				return Mod;
		}
	}
	return NULL;
}

bool CEMSystem::AddToParList(CEMParam *param, bool SysMadeId)
{
	if(!SysMadeId && m_ParList.find(param->m_Id)!=m_ParList.end()) //don't check if id was assigned automatically (i.e. polname_#row) to avoid difficulties with 'artificial' parameters
	{
		//a not very clean solution to avoid warnings if a policy is listed more than once in the spine
		int n=0; for(size_t i=0; i<m_Pols.size(); ++i) if(m_Pols[i].m_Name==param->m_Module->m_PolName) ++n;
		//second condition (!m_Control->m_InputTXT) for intermediate solution to enable mtr-calculations with xml-input
		if(n==1 && m_Control->m_InputTXT) return CEMError::NonCritErr("Multiple use of identifier '"+param->m_Id+"'.", param);
	}
	m_ParList.insert(std::pair<std::string, CEMParam*>(param->m_Id, param));
	return 1;
}

bool CEMSystem::GetGlobalVar(int vInd, double &vVal)
{
	if((int)(m_Households[0].m_Persons[0].size())<=vInd) return CEMError::CodeErr("Reading of global variable failed.");
	vVal = m_Households[0].m_Persons[0][vInd];
	return 1;
}

bool CEMSystem::SetGlobalVar(int vInd, double vVal)
{
	if((int)(m_Households[0].m_Persons[0].size())<=vInd) return CEMError::CodeErr("Changing of global variable failed.");
	for(HHContainer_t::iterator ith=m_Households.begin(); ith!=m_Households.end(); ith++)
		for(size_t p=0; p<ith->m_Persons.size(); ++p)
			ith->m_Persons[p][vInd]=vVal;
	return 1;
}

bool CEMSystem::Run()
{
#ifdef FLEMOSI
	writeToMonitorFile( starting, 0, 0 );
#endif

	clock_t sTime = clock(); size_t At = m_Control->m_TimeRec.size();
	//convert data to currency of parameters if necessary
	double cfact=1;
	if(m_Control->m_DataCurrencyEuro!=m_ParamCurrencyEuro)
	{
		cfact = m_ExchRate;
		if(m_ParamCurrencyEuro) cfact = 1/cfact;
		for(Vardesc_t::iterator itv=m_Control->m_VarIndex.begin(); itv!=m_Control->m_VarIndex.end(); itv++)
		{
			if(!itv->second.m_Monetary || !itv->second.m_GenType==VARTYPE_DATA) continue;
			for(HHContainer_t::iterator ith=m_Households.begin(); ith!=m_Households.end(); ith++)
				for(size_t p=0; p<ith->m_Persons.size(); ++p)
					ith->m_Persons[p][itv->second.m_Index]*=cfact;
		}
	}

#ifdef FLEMOSI
	writeToMonitorFile( currency, 0, 0 );
	int module_no = 0;
#endif
	//run over all functions
	std::string wkName = m_Name; if(m_AOSystem) wkName = m_AOSystemName + " based on " + m_Name;
	for(ModuleContainer_t::iterator itm=m_Modules.begin(); itm!=m_Modules.end(); itm++)	
	{
		clock_t msTime=clock();

		#ifdef FLEMOSI
			module_no++;
		#endif
		if((*itm)->m_ParamAdmin.GetSwitchParam()->m_numValue!=1) continue; //don't run switched off modules	
		std::cout << "working on " << wkName << " - " << (*itm)->m_PolName << " - " << (*itm)->m_Name << " (" << CEMUtilities::IntToStr((*itm)->m_PolOrder) << "." << CEMUtilities::IntToStr((*itm)->m_Order)+")" << " ..." << std::flush;

		if((*itm)->m_Name!=COMOD13 && (*itm)->m_Name!=COMOD15 && (*itm)->m_Name!=COMOD16 && (*itm)->m_Name!=COMOD17 &&
			(*itm)->m_Name!=COMOD20 && (*itm)->m_Name!=COMOD21 && (*itm)->m_Name!=COMOD23 && (*itm)->m_Name!=COMOD26 && (*itm)->m_Name!=COMOD28) //functions which do not run over all households or have their own hh-loop (see else)
		{
			//run function over all households
			#ifdef FLEMOSI
			int hhref = 0;
			#endif
			for(HHContainer_t::iterator ith=m_Households.begin(); ith!=m_Households.end(); ith++)
			{
				#ifdef FLEMOSI
					hhref++;
			        if( hhref % 100  == 0 ){ writeToMonitorFile( households, hhref, module_no ); }
				#endif

				CEMTUParam *tup = (*itm)->m_ParamAdmin.GetTUParam(); //assess modules taxunit type
				if(tup->m_strValue!="n/a")
				{	//if taxunits of this type do not yet exist they must be built
					if(!ith->AreTUsBuilt(tup->m_strValue)) { if(!tup->m_TUDefMod->BuildTUs(&(*ith), &(ith->m_Persons))) return 0; }
					//run all taxunits of the module's taxunit type through module
					TUContainer_t TUs = ith->GetTUsOfType(tup->m_strValue);
					for(TUContainer_t::iterator itt=TUs.begin(); itt!=TUs.end(); itt++)
					{
						bool goahead=1; //for func_unitloop: skip modules if there is no 'eligible' unit left
						if(m_viNULElig!=-1)
						{
							double loopcount, nElig = itt->GetVarVal(m_viNULElig, &(ith->m_Persons));
							GetGlobalVar(m_viULoopCount, loopcount);
							goahead = (loopcount<=nElig);
						}
						if(!goahead) continue;
						if(!(*itm)->Run(&(*itt), &(ith->m_Persons))) return 0;	// if it cannot run, exit
					}
				}
				else //functions without a taxunit
				{
					if((*itm)->m_Name==COMOD9 || (*itm)->m_Name==COMOD24) { if(!((CEMCM_DefVar*)(*itm))->Run(&(ith->m_Persons))) return 0; }
					else if((*itm)->m_Name==COMOD12) { if(!((CEMCM_Uprate*)(*itm))->Run(&(*ith))) return 0; }
					else if((*itm)->m_Name==COMOD27) { if(!((CEMCM_SetDefault*)(*itm))->Run(&(*ith))) return 0; }
					else if((*itm)->m_Name==COMOD14) { if(!((CEMCM_UpdateTU*)(*itm))->Run(&(ith->m_Taxunits))) return 0; }
					else if((*itm)->m_Name==COMOD18 || (*itm)->m_Name==COMOD19) { if(!(*itm)->Run(NULL, &(ith->m_Persons))) return 0; }
					else if((*itm)->m_Name==COMOD29) { if(!((CEMCM_DefInput*)(*itm))->Run(&(ith->m_Persons), ith+1==m_Households.end())) return 0; }
				}
			}//end hh-loop
		}
		else //functions which do not run over all households or have their own loop
		{
			#ifdef FLEMOSI
			writeToMonitorFile( non_households, 0, module_no );
			#endif

			if((*itm)->m_Name==COMOD13) { if(!((CEMCM_DefOutput*)(*itm))->Run()) return 0; } //does its own loop over households
			else if((*itm)->m_Name==COMOD15) { if(!((CEMCM_ChangeParam*)(*itm))->RuntimeChangeParam()) return 0; } //no loop over households
			else if((*itm)->m_Name==COMOD20 || (*itm)->m_Name==COMOD21) { if(!((CEMCM_DropKeepUnit*)(*itm))->Run()) return 0; } //do their own loop over households
			else if((*itm)->m_Name==COMOD23) { if(!((CEMCM_Totals*)(*itm))->Run()) return 0; } //does its own loop over households
			else if((*itm)->m_Name==COMOD16 || (*itm)->m_Name==COMOD17)
			{
				std::string JumpTo=""; if(!((CEMCM_Loop*)(*itm))->Run(JumpTo)) return 0;
				if(!JumpTo.empty())
				{
					size_t m; for(m=0; m<m_Modules.size(); ++m) if(JumpTo==m_Modules[m]->m_Id) break;
					itm=m_Modules.begin()+m-1; continue;
				}
			}
			else if((*itm)->m_Name==COMOD26) { if(!((CEMCM_RandSeed*)(*itm))->Run()) return 0; } //no loop over households
#ifdef _WIN32
			else if((*itm)->m_Name==COMOD28) { if(!((CEMCM_CallProgramme*)(*itm))->Run()) return 0; } //no loop over households
#endif
		}
		if(m_Control->m_LogRunTime) {
			m_Control->AddTimeRec("run function", (*itm)->m_PolName+" "+(*itm)->m_Name+" ("+CEMUtilities::IntToStr((*itm)->m_PolOrder) + "." + CEMUtilities::IntToStr((*itm)->m_Order)+")", clock()-msTime);
			std::cout << clock() - msTime;
		}
		std::cout << "\n" << std::flush;
	}//end function-loop

	if (m_ActiveUprateFunctions.size() == 0) { if (!CEMError::NonCritErr("No applicable uprate function found.", "", m_Name, "", "", "")) return 0; }
	if (m_ActiveUprateFunctions.size() > 1)
	{
		std::string funcRows = "";
		for (std::vector<CEMCM_Uprate*>::iterator it = m_ActiveUprateFunctions.begin(); it != m_ActiveUprateFunctions.end(); it++)
			funcRows += CEMUtilities::IntToStr((*it)->m_PolOrder) + "." + CEMUtilities::IntToStr((*it)->m_Order) + "   ";		
		if (!CEMError::NonCritErr("More than one applicable uprate function found at rows  " + funcRows, "", m_Name, "", "", "")) return 0;
	}

	std::cout << "finished working on " << wkName << "\n" << std::flush;
	#ifdef FLEMOSI
		writeToMonitorFile( eu_run_complete, 0, module_no );
	#endif

	m_Control->AddTimeRec("RUN SYSTEM", m_Name, clock()-sTime, At);
	return 1;
}

bool CEMSystem::LoopFitIn()
{
	for(LoopContainer_t::iterator itl=m_Loops.begin(); itl!=m_Loops.end(); itl++)
	{
		if(!(*itl)->m_ParamAdmin.CheckCompParamExists("switch", PTYPE_SWITCH, *itl, "off")) return 0;
		if((*itl)->m_ParamAdmin.GetSwitchParam()->m_numValue!=1) continue; //don't do anything if loop is switched off

		std::string Start, Stop; bool IsStartFunc, StartAfter, IsStopFunc, StopBefore;
		if(!(*itl)->FitIn(Start, IsStartFunc, StartAfter, Stop, IsStopFunc, StopBefore)) return 0;

		int iStart=-1, iStop=-1;
		for(size_t m=0; m<m_Modules.size(); m++)
		{
			if((IsStartFunc && m_Modules[m]->m_Id==Start) || (!IsStartFunc && (iStart==-1 || StartAfter) && m_Modules[m]->m_PolName==Start)) iStart=(int)(m);
			if((IsStopFunc && m_Modules[m]->m_Id==Stop) || (!IsStopFunc && (iStop==-1 || !StopBefore) && m_Modules[m]->m_PolName==Stop)) iStop=(int)(m)+1;
		}
		if(iStart==-1) { if (!CEMError::CritErr("Unknown loop-start policy/function '"+Start+"'.", *itl)) return 0; continue; }
		if(iStop==-1) { if (!CEMError::CritErr("Unknown loop-stop policy/function '"+Stop+"'.", *itl)) return 0; continue; }
		if(StartAfter && iStart==(int)(m_Modules.size())-1) { if (!CEMError::CritErr("Loop cannot start after the last policy/function in the spine ('"+Start+"').", *itl)) return 0; continue; }
		if(StopBefore && iStop==0) { if (!CEMError::CritErr("Loop cannot stop before the first policy/function in the spine ('"+Stop+"').", *itl)) return 0; continue; }

		CEMCM_Loop *Twin; if((*itl)->m_Name==COMOD16) Twin=new CEMCM_Loop(); else Twin=new CEMCM_UnitLoop();
		Twin->Init((*itl)->m_Name, this, (*itl)->m_PolName); Twin->TakeParam("switch", "on", "", 0, 0);
		(*itl)->m_IsLoopStart=1; (*itl)->m_Twin=Twin; Twin->m_IsLoopStart=0; Twin->m_Twin=(*itl);
		if(StartAfter) ++iStart; m_Modules.insert(m_Modules.begin()+iStart, *itl);
		if(!StopBefore) ++iStop; m_Modules.insert(m_Modules.begin()+iStop, Twin);
	}
	return 1;
}

bool CEMSystem::IsVariableUsed(size_t VarIndex, double CheckForValue)
{
	for(HHContainer_t::iterator ith=m_Households.begin(); ith!=m_Households.end(); ith++)
		for(size_t p=0; p<ith->m_Persons.size(); ++p)
			if(ith->m_Persons[p][VarIndex]!=CheckForValue)
				return 1;
	return 0;
}

CEMTaxunit *CEMSystem::GetDummyIndividualTU(int indexPerson)
{
	m_DummyIndividualTU->m_HeadIndex=indexPerson;
	CEMPerson pe;
	pe.m_Index=indexPerson;
	pe.m_IsHead=1;
	m_DummyIndividualTU->m_PersonRefs.clear();
	m_DummyIndividualTU->m_PersonRefs.insert(m_DummyIndividualTU->m_PersonRefs.end(), pe);
	return m_DummyIndividualTU;
}

#ifdef FLEMOSI

void CEMSystem::writeToMonitorFile( Steps step, int hhcount, int module ){
        // using namespace std;
        std::ofstream myfile;
        std::string file_name;
        file_name = m_Control->m_OutputPath + "monitor.txt";
        myfile.open( file_name.c_str(), std::ios::trunc | std::ios::in );
        myfile << "Step HHCount Module\n" << step << " " << hhcount << " " << module << "\n";
        myfile.close();
}

#endif