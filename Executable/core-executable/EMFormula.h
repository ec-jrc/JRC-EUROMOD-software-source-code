#include <string>
#include <vector>

#define MIN_LEVEL 1
#define MAX_LEVEL 3
#define POW_LEVEL 1
#define POINT_LEVEL 2
#define LINE_LEVEL 3
#define NO_LEVEL -999
#define NUMBER '#'
//^=power (e.g. 2^3=8); {=min (e.g. 2{3=2); }=max (e.g. 2}3=3); \=modulo (e.g. 8%5=3); a=amount
//[: replaces !( and means 0 if term in brackets is zero, one otherwise (i.e. if negative or positive)
//]: replaces @( and means absolute value of term in brackets
#define VALID_OP "()^{}*/%-+a[]0\\"

#define NUMTYPE_UNDEF	0
#define NUMTYPE_INT	1
#define NUMTYPE_UNSIGNEDINT	2

class COp
{
public:
	COp() { m_ParLevel=-1; m_ParMatch=-1; }
	void Set(int lev, char op, double val=-1.0, bool Not=0, bool Abs=0)
		{ m_OpLevel=lev; m_Op=op; m_Value=val; m_Empty=0; m_Not=Not; m_Abs=Abs; }
	void Print();

	int m_OpLevel;
	int m_ParLevel;
	int m_ParMatch;
	bool m_Not;
	bool m_Abs;
	char m_Op;
	double m_Value;
	bool m_Empty;
};

class CEMFormula
{
public:
	bool ParseFormula(std::string &strFormula);
	bool CalcFormula(double &result, std::vector<double> &amts);
	std::string m_ErrMessage;

private:
	bool IsValidOp(char c);
	bool CheckPara();
	bool CheckValidFormula();
	bool CalcLevel(int level, int start, int end, int &resi);
	void SetParaLevels();

	std::string m_Formula;
	std::vector<COp> m_Ops;
	std::vector<COp> m_tmpOps;
	int m_MaxParLevel;
	char m_cs[500];
};
