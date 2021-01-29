#ifndef EMTABLE
#define EMTABLE

#include <string>
#include <vector>
#include <map>

class CEMControl;

class CEMTable
{
public:
	void Init(std::string ID, double defaultValue, bool doRanges, int ignoreNCols, CEMControl *m_Control);
	bool TakeHeaderRow(std::string headerRow, int viRowMergeVar);
	bool GenerateColIndex(std::string headerRow);
	bool AddRow(std::string strRow, int rowNumber);
	bool LookUp(double rowMergeValue, double colMergeValue, double &lookupValue);
	bool GetRow(double mergeValue, std::vector<double> &row);
	std::string GetError() { return m_errString; }

	std::string m_ID; //path and file name + system name, used as identifier
	std::vector<int> m_viInputVars;

private:
	std::vector<std::vector<double> > m_table;	// please retain the space in "> >" to avoid compiler errors in Linux
	std::map<double, int> m_rowIndex;
	std::map<double, int> m_colIndex;

	CEMControl *m_Control;
	int m_nCols;
	int m_indexRowMergeVar;
	int m_ignoreNCols;
	double m_defaultValue;
	bool m_doRanges;

	std::string m_errString;
	std::vector<std::string> TokeniseLine(std::string strRow);
	bool StrToDouble(std::string strValue, double &numValue);
};

#endif
