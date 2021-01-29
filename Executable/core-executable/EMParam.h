#ifndef EMPARAM
#define EMPARAM

#include <string>
#include <vector>
#include <map>
#include "EMTaxUnit.h"
#include "EMDefinitions.h"
#include "EMFormula.h"
#include <iostream>

//forward declaration
class CEMControl;
class CEMSystem;
class CEMModule;
class CEMCM_DefTU;
class CEMFormulaParam;

//**********************************************************************
// class CEMParam and derived classes
//**********************************************************************
class CEMParam {
public:
	//constructor
	CEMParam();
	~CEMParam();	// default destructor is assumed in windows, but required in Linux

	//variables
	std::string m_Id; //unique identifier (within system)
	std::string m_Name; //name of parameter
	int m_Order; //order in the function (only available in xml)
	int m_Type; //type of parameter (value, formula, variable, ...)
	std::string m_strValue; //value of parameter in string format
	std::string m_Period; //period of parameter (annual, monthly, ...)
	double m_numValue; //for general use, if there is a numeric translation for strValue
	int m_Compulsory; //true if parameter is compulsory
	bool m_UnusedFootnotePar; //true if footnote parameter and not applied with any operator
	int m_Single; //0 for unique, 1 for non-unique parameter names (e.g. var of DefOutput)
	std::string m_Group; //group sign for parameters which belong together (e.g. comp-parameters of BenCalc)
	CEMControl *m_Control;
	CEMSystem *m_System;
	CEMModule *m_Module;

	//functions
	virtual bool Prepare();
	bool Assess(double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, double DefVal=0);
};

//type definitions
typedef std::vector<CEMParam*> Param_t;

class CEMSwitchParam : public CEMParam {
public:
	bool Prepare();
};

class CEMVarParam : public CEMParam {
public:
	bool Prepare();
	bool Assess(double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, double DefVal=0);
	int m_VarIndex;
};

class CEMValParam : public CEMParam {
public:
	bool Prepare();
};

class CEMYesNoParam : public CEMParam {
public:
	bool Prepare();
};

class CEMILParam : public CEMParam {
public:
	bool Prepare();
	bool Assess(double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, double DefVal=0);
};

class CEMVarILParam : public CEMParam {
public:
	CEMVarILParam () { m_viType=VITYPE_UNDEF; }
	bool Prepare();
	bool Assess(double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, std::string *VoidEntry=NULL);

	int m_viType; //0:undefined, 1:var, 2:il
	int m_VarIndex;
};

class CEMTUParam : public CEMParam {
public:
	//variables
	CEMCM_DefTU *m_TUDefMod;
	
	//functions
	bool Prepare();
};

//**********************************************************************
// class CEMFormulaParam and helper classes
//**********************************************************************
#define OPTYPE_AMOUNT 1
#define OPTYPE_VAR 2
#define OPTYPE_IL 3
#define OPTYPE_FUNC 4
#define OPTYPE_RAND 5

class CEMFunction {
public:
	//functions
	bool Prepare(CEMModule *Module, std::string Name, int FootnoteNo=-1);
	bool Assess(CEMModule *Module, CEMTaxunit *TU, PersonVarContainer_t *HHVar);
	bool IsFuncDefined(std::string Name);
	static bool TakeParam(std::string ParName, std::string ParVal, int &ParType, int &Compulsory);

	//variables
	std::string m_Name;
	int m_FootnoteNo;
	std::string m_sFootnote;
	std::vector<CEMParam*> m_Args;
	bool m_IsStatic;
	bool m_IsGlobal;
	int m_IdentNo;
	double m_numValue;
};

class CEMFormOp {
public:
	//functions
	bool Prepare(CEMModule *Module, std::string strValue, int FootnoteNo=-1, char Period='m');
	bool Assess(CEMModule *Module, CEMTaxunit *TU, PersonVarContainer_t *HHVar);

	//variables
	int m_Type;
	std::string m_strValue;
	double m_numValue;
	int m_FootnoteNo;
	std::string m_sFootnote;
	int m_VarIndex;
	int m_OthMember; //used for taxunit defintion: variable or incomelist refers to another member of taxunit
					 //possible values: 1='head:', 2='partner:', 0=not used
	bool m_IsStatic;
	bool m_IsGlobal;
	CEMFunction m_Function;
	CEMFormulaParam *m_LowLimPar;
	CEMFormulaParam *m_UpLimPar;
	CEMTUParam *m_AltTU;
	CEMParam *m_FormulaParam;
};

class CEMFormulaParam : public CEMParam {
public:
	CEMFormulaParam() { m_Calculator=NULL; }
	~CEMFormulaParam() { if(m_Calculator) { delete m_Calculator; m_Calculator=NULL; } }
	std::vector<CEMFormOp> m_Operands;
	CEMFormula *m_Calculator;
	bool m_IsStatic;
	bool m_IsGlobal;

	bool Prepare();
	bool Assess(double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, double DefVal=0);
};

//**********************************************************************
// class CEMCondParam and helper classes
//**********************************************************************
#define COMPOP_NOC 0
#define COMPOP_EQ 1
#define COMPOP_NEQ 2
#define COMPOP_G 3
#define COMPOP_GE 4
#define COMPOP_L 5
#define COMPOP_LE 6

class CEMCondition {
public:
	//variables
	std::string m_RowCond;
	std::string m_CookedCond;
	std::string m_LeftSide;
	std::string m_RightSide;
	int m_CompOp;
	std::string m_sFootnote;

	size_t m_StartPos;
	size_t m_EndPos;

	CEMControl *m_Control;
	CEMSystem *m_System;
	CEMModule *m_Module;

	//functions
	void Set(CEMModule *Module, std::string RowCond, std::string &sFootnote, size_t StartPos, size_t EndPos);
	std::string Get() { return m_CookedCond; }
	bool Cook();
};

class CEMCondParam : public CEMParam {
public:
	CEMCondParam() { m_Formula=NULL; }
	~CEMCondParam() { if(m_Formula) { delete m_Formula; m_Formula=NULL; } }
	bool Prepare();
	bool Assess(double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, double DefVal=0);
	bool Assess(int OutVarIndex, double &val, CEMTaxunit *TU=NULL, PersonVarContainer_t *HHVar=NULL, int PersIndex=-1, int ResVarIndex=-1);

	CEMFormulaParam *m_Formula;
	bool m_IsGlobal;
};

//**********************************************************************
// class CEMParamAdmin
//**********************************************************************
#define PTYPE_NAME 0
#define PTYPE_SWITCH 1
#define PTYPE_VAR 2
#define PTYPE_VAL 3
#define PTYPE_IL 4
#define PTYPE_YESNO 5
#define PTYPE_VARIL 6
#define PTYPE_TU 7
#define PTYPE_FORMULA 8
#define PTYPE_COND 9
#define PTYPE_CATEG 10
#define PTYPE_CONST 11

class CEMParamAdmin {
public:
	//variables
	Param_t m_Params;

	//functions
	bool InsertParam(int Type, CEMModule *Module, std::string Name, std::string Val, std::string ParId, int Compulsory, int Single, std::string Group, int Line);
	bool Prepare();
	bool PrepareSkip(std::vector<std::string> SkipParNames);
	bool Prepare(std::string ParName);
	bool IsParamDefined(std::string ParName, bool AcceptNA=0, std::string Group="");
	bool IsParamDefined(std::string ParName, std::string Group) { return IsParamDefined(ParName, 0, Group); }
	bool CheckCompParamExists(std::string ParName, int Type, CEMModule *Module, std::string DefVal, std::string Group="");
	bool CheckOptParamExists(std::string ParName, int Type, CEMModule *Module, std::string DefVal, bool DoNotPrepare=0, int Single=1, std::string Group="");
	int CheckSequence(std::string ParName, CEMModule *Module, std::string ParNamePart2="", std::string ParNamePart2Alias="", std::string Location="", std::string SysName="", std::string PolName="", bool AcceptOmission=0);
	bool CheckFootnoteParUsage();
	CEMParam *GetParam(std::string ParName, std::string Group, bool issueCodeErr=1);
	CEMParam *GetGeneralParam(std::string ParName, std::string Group="") { return GetParam(ParName, Group); }
	CEMSwitchParam *GetSwitchParam() { return (CEMSwitchParam *)(GetParam("switch", "")); }
	CEMVarParam *GetVarParam(std::string ParName, std::string Group="") { return (CEMVarParam *)(GetParam(ParName, Group)); }
	CEMValParam *GetValParam(std::string ParName, std::string Group="") { return (CEMValParam *)(GetParam(ParName, Group)); }
	CEMILParam *GetILParam(std::string ParName, std::string Group="") { return (CEMILParam *)(GetParam(ParName, Group)); }
	CEMYesNoParam *GetYesNoParam(std::string ParName, std::string Group="") { return (CEMYesNoParam *)(GetParam(ParName, Group)); }
	CEMVarILParam *GetVarILParam(std::string ParName, std::string Group="") { return (CEMVarILParam *)(GetParam(ParName, Group)); }
	CEMTUParam *GetTUParam(std::string ParName="tax_unit", std::string Group="") { return (CEMTUParam *)(GetParam(ParName, Group)); }
	CEMFormulaParam *GetFormulaParam(std::string ParName, std::string Group="") { return (CEMFormulaParam *)(GetParam(ParName, Group)); }
	CEMCondParam *GetCondParam(std::string ParName, std::string Group="") { return (CEMCondParam *)(GetParam(ParName, Group)); }
	bool GetGroupParam(std::map<int,Param_t> &ParamGroups, std::string GroupDef="", bool InclNA=0, bool clear=1, bool reportEmptyGroup=0);
	void GetNotSingleParams(std::string ParName, Param_t &Params, bool InclNA=0, bool clear=1);
	void GetNotSingleParams(std::string ParName, std::vector<CEMVarParam*> &Params, bool InclNA=0, bool clear=1);
	void GetNotSingleParams(std::string ParName, std::vector<CEMILParam*> &Params, bool InclNA=0, bool clear=1);
	void GetNotSingleParams(std::string ParName, std::vector<CEMFormulaParam*> &Params, bool InclNA=0, bool clear=1);
	void GetNotSingleParams(std::string ParName, std::vector<CEMCondParam*> &Params, bool InclNA=0, bool clear=1);
	void FreeMemory();
	void RemoveNA();
	
	//debugging functions
	void List(std::string sep=";", std::ofstream *ofs=NULL);
};

#endif
