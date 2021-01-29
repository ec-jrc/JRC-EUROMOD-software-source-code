#include <iostream>
#include "EMTaxUnit.h"
#include "EMModule.h"
#include "EMDefinitions.h"
#include "EMSystem.h"
#include "EMControl.h"
#include "EMUtilities.h"

/********************************************************************************************
 functions class CEMHousehold 
********************************************************************************************/
bool CEMHousehold::AreTUsBuilt(std::string TUName)
{
	return m_Taxunits.find(TUName)!=m_Taxunits.end();
}

TUContainer_t CEMHousehold::GetTUsOfType(std::string TUName)
{
	TUTypeContainer_t::iterator it=m_Taxunits.find(TUName);
	return it->second;
}

bool CEMHousehold::GetContainedTUs(CEMTaxunit *TU, std::string TUName, TUContainer_t *TC)
{	//find the taxunits which are contained by TU
	//example:
	//tutype of TU: inner family
	//part tu (indicated in TUName): couple
	//persons in hh: mother(m), father(f), child(c), grandmother(gm), grandfather(gf), uncle(u)
	//tus for inner familiy: F1=m+f+c, F2=gm+gf, F3=u
	//tus for couple: C1=m+f, C2=c, C3=gm+gf, C4=u
	//part tus: F1->C1+C2, F2->C3, F3->C4
	//(this could lead to an error if inner family contains married children and a child
	//is married to some person in the hh not belonging to inner family)

	TC->empty();
	//get all tus of the part type
	TUTypeContainer_t::iterator it=m_Taxunits.find(TUName);
	
	//loop over those tus
	for(TUContainer_t::iterator itt=it->second.begin(); itt!=it->second.end(); itt++)
	{	
		int isPart=-1;
		//loop over persons of the (current) part taxunit
		for(PersonRefContainer_t::iterator itpp=itt->m_PersonRefs.begin(); itpp!=itt->m_PersonRefs.end(); itpp++)
		{	
			int found=0;
			//loop over persons of the container taxunit (TU)
			for(PersonRefContainer_t::iterator itpc=TU->m_PersonRefs.begin(); itpc!=TU->m_PersonRefs.end(); itpc++)
				{ if(itpp->m_Index==itpc->m_Index) { found=1; break; } }
			if(isPart==-1) isPart=found;
			else { if(found!=isPart) return 0; } //part tu must either be totally contained by the containing tu or not at all
		}
		if(isPart) TC->insert(TC->end(), *itt);
	}
	return 1;
}

CEMTaxunit *CEMHousehold::GetContainingTU(CEMTaxunit *TU, std::string TUName)
{	//find the taxunit which contains TU
	//example:
	//tutype of TU: couple
	//container tu (indicated in TUName): inner family
	//persons in hh: mother(m), father(f), child(c), grandmother(gm), grandfather(gf), uncle(u)
	//tus for couple: C1=m+f, C2=c, C3=gm+gf, C4=u
	//tus for inner familiy: F1=m+f+c, F2=gm+gf, F3=u
	//containing tus: C1->F1, C2->F1, C3->F2, C4->F3
	//(this could lead to an error if inner family contains married children and a child
	//is married to some person in the hh not belonging to inner family)
	
	//get all tus of the container type (i.e. inner family)
	TUTypeContainer_t::iterator it=m_Taxunits.find(TUName);
	CEMTaxunit *contTU=NULL;
	//loop over the tus of the container type (i.e. over F1, F2, F3)
	for(TUContainer_t::iterator itt=it->second.begin(); itt!=it->second.end(); itt++)
	{	//find tu where head of TU is part of (there needs to be exactly one)
		contTU=&(*itt);
		PersonRefContainer_t::iterator itp;
		for(itp=contTU->m_PersonRefs.begin(); itp!=contTU->m_PersonRefs.end(); itp++)
			if(itp->m_Index==TU->m_HeadIndex) break;
		if(itp!=contTU->m_PersonRefs.end()) break;
	}
	//now check if all other members of TU are included
	for(PersonRefContainer_t::iterator itpt=TU->m_PersonRefs.begin(); itpt!=TU->m_PersonRefs.end(); itpt++)
	{
		PersonRefContainer_t::iterator itpc;
		for(itpc=contTU->m_PersonRefs.begin(); itpc!=contTU->m_PersonRefs.end(); itpc++)
			if(itpc->m_Index==itpt->m_Index) break;
		if(itpc==contTU->m_PersonRefs.end()) return NULL; //person not found -> no container TU
	}
	return contTU;
}

CEMTaxunit *CEMHousehold::GetContainingTU(int PersIndex, std::string TUName)
{	//find the taxunit which contains a person
	
	//get all tus of the container type
	TUTypeContainer_t::iterator it=m_Taxunits.find(TUName);
	CEMTaxunit *contTU=NULL;
	//loop over the tus of the container type
	for(TUContainer_t::iterator itt=it->second.begin(); itt!=it->second.end(); itt++)
	{	//find tu where person is part of (there needs to be exactly one)
		contTU=&(*itt);
		for(PersonRefContainer_t::iterator itp=contTU->m_PersonRefs.begin(); itp!=contTU->m_PersonRefs.end(); itp++)
			if(itp->m_Index==PersIndex) return contTU;
	}
	return NULL;
}

/********************************************************************************************
 functions class CEMTaxunit
********************************************************************************************/
CEMTaxunit::CEMTaxunit(CEMControl *Control, CEMHousehold *HH)
{
	m_Control=Control; 
	m_HH=HH;
}

double CEMTaxunit::GetVarVal(int VarIndex, PersonVarContainer_t *HHVar, int PersIndex)
{
	if(PersIndex!=-1) return (*HHVar)[PersIndex][VarIndex];
	if(!m_Control->m_VarIndexV[VarIndex].m_SumTU) return (*HHVar)[m_HeadIndex][VarIndex];
	double TUSum=0;
	for(PersonRefContainer_t::iterator it=m_PersonRefs.begin(); it!=m_PersonRefs.end(); it++)
	{
		double Val=(*HHVar)[it->m_Index][VarIndex];
		if(Val==EMVOID) return Val;
		TUSum+=Val;
	}
	return TUSum;
}

bool CEMTaxunit::OverwriteVarVal(double amount, int VarIndex, PersonVarContainer_t *HHVar, int PersIndex)
{
	if(PersIndex!=-1) { (*HHVar)[PersIndex][VarIndex]=amount; return 1; }
	for(PersonRefContainer_t::iterator it=m_PersonRefs.begin(); it!=m_PersonRefs.end(); it++)
		(*HHVar)[it->m_Index][VarIndex] = it->m_Index==m_HeadIndex ? amount : 0.0;
	return 1;
}

bool CEMTaxunit::AddToVarVal(double amount, CEMVarParam *OutVarPar, PersonVarContainer_t *HHVar, int PersIndex)
{
	if(PersIndex!=-1)
	{
		if((*HHVar)[PersIndex][OutVarPar->m_VarIndex]==EMVOID)
		{
			if(!CEMError::NonCritErr("Use of not initialised variable.", OutVarPar)) return 0;
			(*HHVar)[PersIndex][OutVarPar->m_VarIndex] = amount;
		}
		else
			(*HHVar)[PersIndex][OutVarPar->m_VarIndex] += amount;
		return 1;
	}
	for(PersonRefContainer_t::iterator it=m_PersonRefs.begin(); it!=m_PersonRefs.end(); it++)
		if((*HHVar)[it->m_Index][OutVarPar->m_VarIndex]==EMVOID)
		{
			if(!CEMError::NonCritErr("Use of not initialised variable.", OutVarPar)) return 0;
			(*HHVar)[it->m_Index][OutVarPar->m_VarIndex] = 0.0;
		}
	(*HHVar)[m_HeadIndex][OutVarPar->m_VarIndex]+=amount;
	return 1;
}

double CEMTaxunit::GetILVal(IL_t &ILDef, PersonVarContainer_t *HHVar, int PersIndex, std::string *VoidEntry)
{
	double Sum=0, EVal;
	// if(ILDef.empty()) return EMVOID;
	if (ILDef.empty()) return 0; // September 2017: emtpy ILs are not "illegal", moreover returning void is problematic if the IL is used in another IL
	for(IL_t::iterator it=ILDef.begin(); it!=ILDef.end(); it++)
		{ if(it->m_Entry->Assess(EVal, this, HHVar, PersIndex, VoidEntry)) Sum+=EVal*it->m_Factor; }
	return Sum;
}

/********************************************************************************************
 functions class CEMVar
********************************************************************************************/
CEMVar::CEMVar()
{
	m_Name="";
	m_Monetary=0;
//	m_DefVal=EMVOID;
//	m_DefVar="";
	m_Index=-1;
	m_IsGlobal=0;
	m_SumTU=1;
	m_GenType=0;
}

CEMVar& CEMVar::operator= (const CEMVar& v)
{
	m_Name=v.m_Name;
	m_Monetary=v.m_Monetary;
//	m_DefVal=v.m_DefVal;
//	m_DefVar=v.m_DefVar;
	m_Index=v.m_Index;
	m_IsGlobal=v.m_IsGlobal;
	m_SumTU=v.m_SumTU;
	m_GenType=v.m_GenType;
	return *this;
}