#ifndef EMMODULE
#define EMMODULE

#include <string>
#include <vector>
#include "EMParam.h"
#include "EMTaxUnit.h"

//forward declaration
class CEMControl;
class CEMSystem;
class CEMCM_Elig;
class CEMTable;

//**********************************************************************
// class CEMModule
//**********************************************************************
class CEMModule {
public:
	//variables
	std::string m_Id; //unique identifier (within system)
	std::string m_Name; //name of module
	int m_Order; //order in the policy (only available in xml)
	CEMParamAdmin m_ParamAdmin; //module's parameters
	CEMControl *m_Control; //references to "parent" structures
	CEMSystem *m_System;
	std::string m_PolName;
	int m_PolOrder;
	CEMTaxunit *m_ActTU;
	PersonVarContainer_t *m_ActHHVar;
	double m_Result;

	//functions
	virtual void Init(std::string Name, CEMSystem *System, std::string PolName, std::string Id="", int Line=0);
	virtual ~CEMModule();	// default destructor is assumed in windows, but required in Linux
	virtual bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1); //process module parameter
	virtual bool CheckParam(); //check if all necessary parameters are indicated
	bool CheckWhoMustBeElig(std::string wmbe, double &numValue);
	virtual bool Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar); //performs module calculations
	int EvalRunCond(bool ignoreLoopCount = 0);
	bool ApplyLimParam(double &Result, CEMFormulaParam *pLowLim, CEMFormulaParam *pUpLim);
	void CleanUp(); //destroy elements created with new
	virtual bool DoModCalc() { return 1; }
	bool GetTUElig(int &Elig, int EligType=-1, int EligVarIndex=-1);
	virtual bool ApplyStdLimits();
	virtual bool SetOutputVars();
};

//**********************************************************************
// class CEMCM_ArithOp
//**********************************************************************
class CEMCM_ArithOp : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc();
};

//**********************************************************************
// class CEMCM_Elig
//**********************************************************************
class CEMCM_Elig : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc();
	bool ApplyStdLimits() { return 1; }
	bool SetOutputVars();
};

//**********************************************************************
// class CEMCM_BenCalc
//**********************************************************************
class CEMBenComp {
public:
	CEMBenComp() { m_Cond=NULL; m_Formula=m_LowLim=m_UpLim=NULL; }
	CEMCondParam *m_Cond;
	CEMFormulaParam *m_Formula;
	bool m_perElig;
	CEMFormulaParam *m_LowLim;
	CEMFormulaParam *m_UpLim;
};

class CEMCM_BenCalc : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc();

	std::vector<CEMBenComp> m_Comps;
};

//**********************************************************************
// class CEMCM_SchedCalc
//**********************************************************************
class CEMBand {
public:
	CEMBand() { m_LowLim=m_UpLim=m_Rate=m_Amount=NULL; m_Order=-1; m_Group=""; }
	CEMFormulaParam *m_LowLim;
	CEMFormulaParam *m_UpLim;
	CEMFormulaParam *m_Rate;
	CEMFormulaParam *m_Amount;
	int m_Order;
	std::string m_Group;
};

class CEMCM_SchedCalc : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc();

	std::vector<CEMBand> m_Bands;
};

//**********************************************************************
// class CEMCM_MiniMaxi
//**********************************************************************
class CEMCM_MiniMaxi : public CEMModule {
public:
	//functions
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc(bool Min);

	std::vector<CEMFormulaParam*> m_Vals;
};

class CEMCM_Min : public CEMCM_MiniMaxi {
protected:
	bool DoModCalc();
};

class CEMCM_Max : public CEMCM_MiniMaxi {
protected:
	bool DoModCalc();
};

//**********************************************************************
// class CEMCM_Allocate_F210 ( !!! PHASE-OUT !!! )
//**********************************************************************
class CEMCM_Allocate_F210 : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc();
	bool ApplyStdLimits() { return 1; }
	bool SetOutputVars() { return 1; }

	CEMCondParam *m_ShareBetween;
};

//**********************************************************************
// class CEMCM_Allocate
//**********************************************************************
class CEMCM_Allocate : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool DoModCalc();
	bool ApplyStdLimits() { return 1; }
	bool SetOutputVars() { return 1; }

	CEMCondParam *m_ShareBetween;
};

//**********************************************************************
// class CEMCM_DefVar
//**********************************************************************
class CEMCM_DefVar : public CEMModule {
public:
	//functions
	CEMCM_DefVar() { m_nV = 0; m_nG = 0; }
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(PersonVarContainer_t *HHVar);

	std::vector<int> m_VarInd;
	std::vector<CEMFormulaParam *> m_VarVals;
	int m_nV;
	int m_nG;
};

//**********************************************************************
// class CEMCM_DefIL
//**********************************************************************
class CEMCM_DefIL : public CEMModule {
public:
	void Init(std::string Name, CEMSystem *System, std::string PolName, std::string Id="", int Line=0);
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar) { TU; HHVar; /*just to avoid warning about unreferenced formal parameter*/ return 1; }

	int m_nEntries;
	static std::map<std::string, int> m_ilNames;
};

//**********************************************************************
// class CEMCM_DefTU
//**********************************************************************
class CEMCM_DefTU : public CEMModule {
public:
	void Init(std::string Name, CEMSystem *System, std::string PolName, std::string Id="", int Line=0);
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar) { TU; HHVar; /*just to avoid warning about unreferenced formal parameter*/ return 1; }
	bool BuildTUs(CEMHousehold *hh, PersonVarContainer_t *HHVar);
	bool AssessHeadOfTU(CEMTaxunit *TU);
	bool AssessTUStatus(CEMTaxunit *TU, CEMCondParam *Cond, int Type);

	int m_Type;
	std::string m_TUName;
	bool m_IsPartnerMem;
	bool m_AreDepChMem;
	bool m_AreOwnChMem;
	bool m_AreOwnDepChMem;
	bool m_AreLooseDepChMem;
	bool m_AreDepParMem;
	bool m_AreDepRelMem;
	CEMCondParam *m_ExtHeadCond;
	CEMCondParam *m_PartnerCond;
	CEMCondParam *m_DepChCond;
	CEMCondParam *m_OwnChCond;
	CEMCondParam *m_OwnDepChCond;
	CEMCondParam *m_LooseDepChCond;
	CEMCondParam *m_DepParCond;
	CEMCondParam *m_DepRelCond;
	CEMCondParam *m_LoneParCond;
};

//**********************************************************************
// class CEMCM_UpdateTU
//**********************************************************************
class CEMCM_UpdateTU : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(TUTypeContainer_t *Taxunits);
	std::string m_TUName;
};

//**********************************************************************
// class CEMCM_DefOutput
//**********************************************************************
class CEMCM_DefOutput : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run();
	void GenHeader(int Mode, std::string &Header, std::string OutfileName);

	std::vector<int> m_Vars;
	std::vector<std::string> m_ILs;
	std::vector<CEMTUParam*> m_UnitInfoTU;
	std::vector<bool> m_PrintHeadID;
	std::vector<bool> m_PrintIsPartner;
	std::vector<bool> m_PrintIsDepChild;
	std::vector<bool> m_PrintIsOwnChild;
	std::vector<bool> m_PrintIsOwnDepChild;
	std::vector<bool> m_PrintIsLooseDepChild;
	std::vector<bool> m_PrintIsDepParent;
	std::vector<bool> m_PrintIsDepRelative;
	std::vector<bool> m_PrintIsLoneParent;
};

//**********************************************************************
// class CEMCM_Uprate
//**********************************************************************
class CEMUprateVar {
public:
	int m_VarIndex;
	CEMCondParam *m_Cond;
	double m_Factor;
};

class CEMUprateAgg {
public:
	int m_VarIndex;
	std::vector<int> m_PartIndex;
	double m_Tolerance;
};

class CEMCM_Uprate : public CEMModule {
public:
	CEMCM_Uprate() { m_nV = 0; m_addedToActiveList = 0; }
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMHousehold *HH);
	bool DoesReferToDataset();
	bool GetFactorValueOrConstIndex(CEMParam *pFactor, double &valFactor, bool issueError = 1);

	double m_DefFactor;
	int m_nV;
	std::vector<CEMUprateVar> m_UprateVars;
	std::vector<CEMUprateAgg> m_UprateAggs;
	bool m_addedToActiveList;
};

//**********************************************************************
// class CEMCM_ChangeParam
//**********************************************************************
class CEMCM_ChangeParam : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam() { return 1; }
	bool ReadtimeChangeParam();
	bool RuntimeChangeParam();

	std::vector<CEMParam *> m_ParToCh;
	std::vector<std::string> m_CondVals;
};

//**********************************************************************
// class CEMCM_Loop and class CEMCM_UnitLoop
//**********************************************************************
class CEMCM_Loop;
class CEMCM_Loop : public CEMModule {
public:
	CEMCM_Loop() { m_OutOfLoop=1; }
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam() { return 1; }
	virtual bool FitIn(std::string &Start, bool &IsStartFunc, bool &StartAfter, std::string &Stop, bool &IsStopFunc, bool &StopBefore);
	bool Run(std::string &JumpTo);
	virtual bool LoopStartAction();
	virtual bool LoopEndAction(std::string &JumpTo);

	CEMCM_Loop *m_Twin;
	bool m_IsLoopStart;
	bool m_OutOfLoop;
	std::string m_LoopId;
	double m_nIt;
	int m_viLoopCount;
};

class CEMCM_UnitLoop : public CEMCM_Loop {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool FitIn(std::string &Start, bool &IsStartFunc, bool &StartAfter, std::string &Stop, bool &IsStopFunc, bool &StopBefore);
	bool LoopStartAction();
	bool LoopEndAction(std::string &JumpTo);

	int m_viCurElig;
	int m_viNULElig;
	int m_viIsULElig;
	int m_viIsEligInIter;
};

//**********************************************************************
// class CEMCM_Store
//**********************************************************************
class CEMCM_Store : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar);
	bool StoreIL(std::string OrILName, std::string CpILName);

	std::vector<int> m_viOr; //indexes of variables to store (e.g. index of yem)
	std::vector<int> m_viCop; //indexes of storing variables (e.g. index of yem_pfix, yem_ploop)
	std::vector<std::string> m_vNames; //names of variables to store (e.g. yem)
	std::vector<std::string> m_vLev; //only relevant for postloop with unit loop
	std::vector<CEMILParam*> m_ilPar; //names of incomelists to store (e.g. ils_dispy)
	std::vector<std::string> m_ilLev; //only relevant for postloop with unit loop
	bool m_ilVarGen;
};

//**********************************************************************
// class CEMCM_Restore
//**********************************************************************
class CEMCM_Restore : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar);

	CEMCM_Store *m_Store;
};

//**********************************************************************
// class CEMCM_DropKeepUnit
//**********************************************************************
class CEMCM_DropKeepUnit : public CEMModule {
public:
	CEMCM_DropKeepUnit(bool Keep) { m_Keep=Keep; }
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run();

	bool m_Keep;
};

//**********************************************************************
// class CEMCM_ILVarOp
//**********************************************************************
class CEMCM_ILVarOp : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMTaxunit *TU, PersonVarContainer_t *HHVar);
};

//**********************************************************************
// class CEMCM_Totals
//**********************************************************************
class CEMCM_Totals : public CEMModule {
public:
	CEMCM_Totals() { m_nIlVar=0; m_viSum.clear(); m_viMean.clear(); m_viCount.clear(); m_viPosCount.clear(); m_viNegCount.clear(); m_viMedian.clear(); m_viMin.clear(); m_viMax.clear(); m_viDec.clear(); m_viQuint.clear(); }
	~CEMCM_Totals();
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run();

	unsigned int m_nIlVar;
	std::vector<int> m_viSum, m_viMean, m_viCount, m_viPosCount, m_viNegCount, m_viMedian, m_viMin, m_viMax;
	std::vector<int*> m_viDec; std::vector<int*> m_viQuint;

protected:
	bool GetTotVarIndex(std::string &ParName, std::string ParVal);
};

class CWeightedVal {
public:
	double m_Val;
	double m_Weight;
};


//**********************************************************************
// class CEMCM_RandSeed
//**********************************************************************
class CEMCM_RandSeed : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run();
};

//**********************************************************************
// class CEMCM_SetDefault
//**********************************************************************
class CEMCM_SetDefault : public CEMModule {
public:
	CEMCM_SetDefault() { m_nV=0; }
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(CEMHousehold *HH);
	int m_nV;
};

#ifdef _WIN32
//**********************************************************************
// class CEMCM_CallProgramme
//**********************************************************************
class CEMCM_CallProgramme : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run();
};
#endif

class CEMCM_DefInput : public CEMModule {
public:
	bool TakeParam(std::string ParName, std::string ParVal, std::string ParId, int Line, int Column, std::string Group="", int ParType=-1, int Compulsory=0, int Single=1);
	bool CheckParam();
	bool Run(PersonVarContainer_t *HHVar, bool IsLastHH);
	bool GetTable();

	CEMTable *m_table;
	int m_viColMergeVar;
	int m_viRowMergeVar;
	int m_viInputVar;
};

#endif
