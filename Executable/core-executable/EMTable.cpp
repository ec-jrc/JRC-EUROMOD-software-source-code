#include "EMTable.h"
#include "EMUtilities.h"
#include "EMDefinitions.h"
#include "EMControl.h"
#include <iostream>

void CEMTable::Init(std::string ID, double defaultValue, bool doRanges, int ignoreNCols, CEMControl *Control)
{
	m_table.clear();
	m_rowIndex.clear();
	m_colIndex.clear();
	m_viInputVars.clear();
	m_nCols = 0;
	m_indexRowMergeVar = -1;

	m_ID = ID;
	m_defaultValue = defaultValue;
	m_doRanges = doRanges;
	m_ignoreNCols = ignoreNCols;
	m_Control = Control;
}

bool CEMTable::TakeHeaderRow(std::string headerRow, int viRowMergeVar)
{//analyse the header row of the input file to assess which variables are to be filled and which variable serves as the 'merge variable'
	std::string unknownVariables = ""; m_errString.clear();
	
	std::vector<std::string> colValues = TokeniseLine(headerRow);
	for(unsigned int i=0; i<colValues.size(); ++i)
	{
		int vi = 0;
		if(!m_Control->GetVarIndex(colValues[i], vi) &&
			!m_Control->GetVarIndex(colValues[i], vi, CREATE_PARAM)) //creating a variable should rarely be necessary as, if it is used, it is created with parameters 
		{				//(or even is a data variable or created via DefVar), however creating is necessary if its only use is being outputted via vargroup (e.g. sin*)
				unknownVariables += colValues[i] + " ";
		}
		else
		{
			m_viInputVars.push_back(vi);
			if(vi==viRowMergeVar) m_indexRowMergeVar=i; //'merge variable' found - store index in line
		}
	}

	if(!unknownVariables.empty())
		m_errString = "Unknown variable(s) found: " + unknownVariables;
	else if(m_indexRowMergeVar == -1)
		m_errString = "'RowMergeVar' not found.";

	m_nCols = m_viInputVars.size(); //remember line size (i.e. number of variables) to check with the following (data)rows

	return m_errString.empty();
}

bool CEMTable::GenerateColIndex(std::string headerRow)
{
	//assume table like this:
	//   | 1 | 2 | 3 |...
	//-------------------
	// 1 |v11|v12|v13|...
	// 2 |v21|v22|v23|...
	// 3 |v31|v32|v33|...
	//...|...|...|...|...
	//i.e. (a) at least two columns are required
	//     (b) row-merge-values are assumed to be located in first column
	//     (c) column-merge-values are assumed to be located in first row
	std::vector<std::string> colValues = TokeniseLine(headerRow);

	m_nCols = colValues.size();
	if(m_nCols < 2) //see (a)
	{
		m_errString = "Input data requires at least two columns.";
		return 0;
	}

	m_indexRowMergeVar = 0; //see (b)

	//according to (c) analyse header row
	for(int i=1; i<m_nCols; ++i) //skip first column, as this assumed to contain row-merge-var, see (b)
	{
		double mergeValue;
		if(!StrToDouble(colValues.at(i), mergeValue))
			return 0;
		std::pair<std::map<double,int>::iterator,bool> ret = m_colIndex.insert(std::pair<double,int>(mergeValue, i)); 
		if (ret.second==false)
		{
			char err[500]; 
			EM_SPRINTF(err, "Multiple occurence of ColMergeVar %.2f.", mergeValue);
			m_errString = err;
			return 0;
		}
	}
	return 1;
}

std::vector<std::string> CEMTable::TokeniseLine(std::string strRow)
{
	std::vector<std::string> colValues = CEMUtilities::TokeniseLine(strRow, m_Control->m_Country);
	if(m_ignoreNCols)
		colValues.erase(colValues.begin(), colValues.begin()+m_ignoreNCols);
	return colValues;
}

bool CEMTable::AddRow(std::string strRow, int rowNumber)
{
	std::vector<std::string> colValues = TokeniseLine(strRow);
	if((int)(colValues.size()) != m_nCols)
	{
		char err[500]; 
		EM_SPRINTF(err, "Row number %d has %d columns, while table is supposed to have %d.", rowNumber, colValues.size(), m_nCols);
		m_errString = err;
		return 0;
	}

	std::vector<double> row(m_nCols);
	for(int i=0; i<m_nCols; ++i)
	{
		if(!StrToDouble(colValues.at(i), row.at(i)))
			return 0;

		if(m_indexRowMergeVar == i) //add value of variable to the row-index, if 'merge var'
		{
			std::pair<std::map<double,int>::iterator,bool> ret = m_rowIndex.insert(std::pair<double,int>(row.at(i),m_table.size())); 
			if (ret.second==false)
			{
				char err[500]; 
				EM_SPRINTF(err, "Row number %d: multiple occurence of RowMergeVar %.2f.", rowNumber, row.at(i));
				m_errString = err;
				return 0;
			}
		}
	}
	m_table.push_back(row);
	return 1;
}

bool CEMTable::LookUp(double rowMergeValue, double colMergeValue, double &lookupValue)
{
	std::map<double,int>::iterator itCol, itRow; //lookup in column index

	lookupValue = m_defaultValue;

	if(!m_doRanges)
	{
		itCol = m_colIndex.find(colMergeValue); //find exact match
		itRow = m_rowIndex.find(rowMergeValue);
	}
	else
	{
		itCol = m_colIndex.lower_bound(colMergeValue); //find the first which is equal or higher (also see GetRow)
		itRow = m_rowIndex.lower_bound(rowMergeValue);
	}
	
	if(itCol != m_colIndex.end() && itRow != m_rowIndex.end())
	{//found: indices show where to assess in table
		lookupValue = m_table.at(itRow->second).at(itCol->second);
		return 1;
	}

	return lookupValue != EMVOID; //an error is only issued if no default value is indicated (via parameter), otherwise using default values is ok
}

bool CEMTable::GetRow(double mergeValue, std::vector<double> &row)
{
	std::map<double,int>::iterator it; //lookup in index
	if(!m_doRanges)
		it = m_rowIndex.find(mergeValue); //find exact match
	else
		it = m_rowIndex.lower_bound(mergeValue); //find the first which is equal or higher, e.g. index=3;8;11 means -infinite to 3.0; >3.0 to 8.0; >8.0 to 11.0
	if(it != m_rowIndex.end())
	{//found: index shows where to assess in table
		row = m_table.at(it->second);
		return 1;
	}
	else //not found: return a vector filled with default value
		row.assign(m_nCols, m_defaultValue);

	return m_defaultValue != EMVOID; //an error is only issued if no default value is indicated (via parameter), otherwise using vector with default values is ok
}

bool CEMTable::StrToDouble(std::string strValue, double &numValue)
{//use an own StrToDouble instead of the one in CEMUtilities to not interfere with the complicated decimal conversion (not an optimal solution but ...)
	char *rem;
	numValue = strtod(strValue.c_str(), &rem);
	if(strcmp(rem, "")!=0)
	{
		m_errString = strValue + " is not a valid number.";
		return 0;
	}
	return 1;
}
