#ifndef EMTAXUNIT
#define EMTAXUNIT

#include <string>
#include <vector>
#include <map>

//forward declaration
class CEMHousehold;
class CEMCM_DefTU;
class CEMControl;
class CEMVarILParam;
class CEMSystem;
class CEMVarParam;

class CEMILEntry {
public:
	CEMVarILParam *m_Entry;
	double m_Factor;
};

typedef std::vector<CEMILEntry> IL_t;
typedef std::map<std::string, IL_t> ILContainer_t;

class CEMVar {
public:
	//constructor
	CEMVar();

	//functions
	CEMVar& operator= (const CEMVar& v);

	//variables
	std::string m_Name;
	int m_Index; //index in variable vector of persons
	bool m_Monetary;
//	double m_DefVal;
//	std::string m_DefVar;
	bool m_SumTU; //usually set to !m_Monetary; set to 'no' for constant (same for all hh-members) and global (same for all persons) variables, to avoid that GetVarVal sums over TU (takes from head instead)
	bool m_IsGlobal; //yes: variable has the same value for each person, i.e. does not need to be reassessed for each person at run-time
	int m_GenType; //type of variable with respect to "origin", i.e. data-generated, non-data-generated but described in variables.xls, non-data-generated and not described in variables.xls, ... (see emcontrol.h VARTYPE_XXX for types)
};

class CEMPerson {
public:
	CEMPerson() { m_IsHead=m_IsPartner=m_IsDepChild=m_IsOwnChild=m_IsOwnDepChild=m_IsLooseDepChild=m_IsDepPar=m_IsDepRel=m_IsLonePar=-1; }
	int m_Index;
	int m_IsHead;
	int m_IsPartner;
	int m_IsDepChild;
	int m_IsOwnChild;
	int m_IsOwnDepChild;
	int m_IsLooseDepChild;
	int m_IsDepPar;
	int m_IsDepRel;
	int m_IsLonePar;
};

//type definition
typedef std::vector<double> PersonVar_t; //data for one person
typedef std::vector<PersonVar_t> PersonVarContainer_t; //data for a group of persons (used in hh)
typedef std::vector<CEMPerson> PersonRefContainer_t; //persons belonging to taxunit (indexes in hh persons vector)

class CEMTaxunit {
public:
	//constructor
	CEMTaxunit(CEMControl *Control, CEMHousehold *HH);

	//variables
	PersonRefContainer_t m_PersonRefs; //persons belonging to taxunit (indexes in hh persons vector)
	int m_HeadIndex; //index of head of taxunit within m_PersonRefs
	int m_ChildCnt;
	int m_AdultCnt;
	CEMHousehold *m_HH;
	CEMControl *m_Control;

	//functions
	double GetVarVal(int VarIndex, PersonVarContainer_t *HHVar, int PersIndex=-1);
	double GetILVal(IL_t &ILDef, PersonVarContainer_t *HHVar, int PersIndex=-1, std::string *VoidEntry=NULL);
	bool OverwriteVarVal(double amount, int VarIndex, PersonVarContainer_t *HHVar, int PersIndex=-1);
	bool AddToVarVal(double amount, CEMVarParam *OutVarPar, PersonVarContainer_t *HHVar, int PersIndex=-1);
};

//type definition
typedef std::vector<CEMTaxunit> TUContainer_t;
typedef std::map<std::string, TUContainer_t> TUTypeContainer_t;

class CEMHousehold {
public:
	//variables
	PersonVarContainer_t m_Persons; //structure storing data for all persons belonging to the HH (input and output)
	TUTypeContainer_t m_Taxunits; //structure storing taxunits for all taxunit types (generated at runtime from respective module)
	int m_HeadIndex; //index of head of household in vector m_Persons;

	//functions
	bool AreTUsBuilt(std::string TUName);
	TUContainer_t GetTUsOfType(std::string TUName);
	bool GetContainedTUs(CEMTaxunit *TU, std::string TUName, TUContainer_t *TC);
	CEMTaxunit *GetContainingTU(CEMTaxunit *TU, std::string TUName);
	CEMTaxunit *GetContainingTU(int PersIndex, std::string TUName);

private:
	//variables

	//functions
};

#endif
