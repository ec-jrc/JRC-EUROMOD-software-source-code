#ifndef EMSYSTEMS
#define EMSYSTEMS

#include <string>
#include <vector>
#include <map>
#include "EMTaxUnit.h"
#include "EMModule.h"
#include "EMUtilities.h"

//forward declarations
class CEMControl;
class CEMCM_DefTU;
class CEMCM_Loop;
class CEMModule;

class CEMPolicy {
public:
	std::string m_Name;
	int m_Switch;
	std::string m_Id;
	int m_Order; //order in the spine (only available in xml)
};

//type definitions
typedef std::vector<CEMHousehold> HHContainer_t;
typedef std::vector<CEMPolicy> PolVec_t;
typedef std::vector<CEMModule*> ModuleContainer_t;
typedef std::vector<CEMCM_Loop *> LoopContainer_t;

class CEMSystem {
public:
	//constructor
	CEMSystem(CEMControl *Control);

	//variables ...
	//... storing some general information
	std::string m_Name; //name of system (e.g. UK_2008)
	std::string m_Id; //unique identifier
	int m_Order; //order of column in tree (only available in xml)
	int m_OutputCurrencyEuro; //currency used for output; true if euro; false if national
	int m_ParamCurrencyEuro; //currency used for parameters; true if euro; false if national
	double m_ExchRate; //exchange rate euro to national currency
	std::string m_HeadDefInc; //default for income to define head of taxunit
	std::string m_Uprate; //apply uprating factors defined by respective func_setdefault
	std::vector<CEMCM_Uprate*> m_ActiveUprateFunctions; //for warning the user if no or more than one uprating function is defined for the system-dataset combination
	std::string m_Private; //only used for migration purposes
	std::map<std::string,std::string> m_PolicySwitches; //list of policy switch variables, where the default switch value was changed in the run dialog
	std::string m_Year;

	//... storing main structures
	CEMControl *m_Control; //reference to control
	HHContainer_t m_Households; //structure storing input and output data
	PolVec_t m_Pols; //structure storing policy names and switches
	ModuleContainer_t m_Modules; //structure storing module parameters in running order (i.e. in the order as how they are stored in spine)
	ILContainer_t m_ILs; //structure storing incomelist definitions
	LoopContainer_t m_Loops; //structure storing loops
	std::multimap<std::string, CEMParam*> m_ParList; //structure which allows finding parameters by their identifier

	//... for miscellaneous use
	CEMModule *m_ActMod;
	unsigned int m_ParamValColumn; //column in parameter file, where value of system's parameters are stored
	bool m_SpineOn;
	CEMTaxunit *m_StaticTU;
	CEMTaxunit *m_DummyIndividualTU;
	int m_viNULElig;
	int m_viULoopCount;
	std::string m_AOControlName;
	std::string m_AOSystemName;
	struct AddOnSys *m_AOSystem;

	//functions
	bool TakeControlParam(std::string ParName, std::string ParVal, int Line); //store parameters from control parameter sheet
	bool TakeSpineParam(std::string ParName, std::string ParVal, std::string ParId, int Line); //take parameters from spine parameter sheet and create policies
	bool TakePolParam(unsigned int pnum, std::string ParName, std::string ParVal, std::string ParId, int Line, std::string ParGroup="", int *AddOnInsInd=NULL); //take parameters from policy parameter sheet and hand over to respective policy
	bool CheckParam(); //check if all necessary parameters are indicated
	bool Run(); //kicks off system calculations
	void CleanUp(); //destroy elements created with new
	bool AddIL(std::string ILName, IL_t &ILDef, CEMModule *Module); //add incomelist definition
	bool IsILDefined(std::string ILName); //check if incomelist is defined
	bool GetILContent(std::string ILName, std::vector<int> &VarInd, CEMParam *Par, std::vector<double> *Fac=NULL, double CurFac=1); //get "expanded" incomelist, i.e. the indexes of all actually included variables (even if they are indicated as ils)
	CEMCM_DefTU *GetTUDefModule(std::string TUName);
	bool AddToParList(CEMParam *param, bool SysMadeId); //add parameter to structure which allows finding parameters by their identifier
	bool LoopFitIn(); //put loops in appropriate position in spine
	bool GetGlobalVar(int vInd, double &vVal);
	bool SetGlobalVar(int vInd, double vVal);
	CEMTaxunit *GetStaticTU() { return m_StaticTU; }
	CEMTaxunit *GetDummyIndividualTU(int indexPerson);
	bool CheckParamGeneratedVarIL();
	bool IsVariableUsed(size_t VarIndex, double CheckForValue=0);

#ifdef FLEMOSI
	enum Steps{ not_started, starting, currency, households, non_households, eu_output, eu_run_complete };
	void writeToMonitorFile( Steps step, int hhcount, int module );
#endif

};

#endif
