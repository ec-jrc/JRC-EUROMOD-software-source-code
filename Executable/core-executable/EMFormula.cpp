#include "EMFormula.h"
#include "EMUtilities.h"
#include "EMDefinitions.h"
#include <ctype.h>
#include <math.h>
#include <stdio.h>
#include <iostream>

bool CEMFormula::ParseFormula(std::string &strFormula)
{
	m_Ops.empty();
	m_tmpOps.empty();
	m_ErrMessage.erase();
	m_Formula = strFormula;
	m_Formula=CEMUtilities::Replace(m_Formula, "(-", "(0-");
	m_Formula=CEMUtilities::Replace(m_Formula, "[", "[0+");
	m_Formula=CEMUtilities::Replace(m_Formula, "]", "]0+");
	m_Formula=CEMUtilities::Replace(m_Formula, "[0+-", "[0-");
	m_Formula=CEMUtilities::Replace(m_Formula, "]0+-", "]0-");
	m_Formula = "(" + m_Formula + ")";
	if(!m_Formula.length()) { m_ErrMessage = "formula is empty."; return 0; }

	for(size_t i=0; i<m_Formula.length(); ++i)
	{
		char c = m_Formula.at(i);
		if (!IsValidOp (c)) return 0;
		
		COp Op;
		switch (c)
		{
			case '^': //power
			case '{': //min
			case '}': //max
				Op.Set(POW_LEVEL, c);
				break;
			case '/':
			case '*':
			case '\\': //modulo (changed from % to §, as % is needed for i.e. var*x%, modulo is no longer mentioned in the description, but keep the ability this way)
				Op.Set(POINT_LEVEL, c);
				break;
			case '+':
			case '-':
				Op.Set(LINE_LEVEL, c);
				break;
			case '(':
			case ')':
				Op.Set(NO_LEVEL, c);
				break;
			case '[':
				Op.Set(NO_LEVEL, c, 0, 1);
				break;
			case ']':
				Op.Set(NO_LEVEL, c, 0, 0, 1);
				break;
			case 'a':
				Op.Set(NO_LEVEL, NUMBER);
				break;
			case '0':
				Op.Set(NO_LEVEL, NUMBER, 0);
				break;
		}
		m_Ops.insert(m_Ops.end(), Op);
	}
	if(!CheckPara()) return 0;
	SetParaLevels();
	return CheckValidFormula();
}

bool CEMFormula::CalcFormula (double &result, std::vector<double> &amts)
{
	m_tmpOps.clear();
	std::vector<COp>::iterator it;
	for(it=m_Ops.begin(); it!=m_Ops.end(); it++)
		m_tmpOps.insert(m_tmpOps.end(), *it);
	size_t i=0;
	for(it=m_tmpOps.begin(); it!=m_tmpOps.end(); it++)
	{
		if(it->m_Op!=NUMBER || it->m_Value!=(-1.0)) continue;
		if(i>=amts.size()) { m_ErrMessage = "error in formula-program (too few amounts)."; return 0; }
		it->m_Value=amts[i];
		++i;
	}
	for (int pl=0; pl<=m_MaxParLevel; ++pl)
	{
		for (size_t i=0; i<m_tmpOps.size()-1; ++i)
		{
			if (m_tmpOps[i].m_ParLevel != pl) continue;
			int resi=0;
			for (int ol=MIN_LEVEL; ol<=MAX_LEVEL; ++ol)
				if (!CalcLevel(ol, (int)(i)+1, m_tmpOps[i].m_ParMatch-1, resi)) return 0;
			m_tmpOps[i].m_Empty = 1;
			m_tmpOps[m_tmpOps[i].m_ParMatch].m_Empty = 1;
			if(m_tmpOps[i].m_Not) m_tmpOps[resi].m_Value=(m_tmpOps[resi].m_Value==0);
			else if(m_tmpOps[i].m_Abs && m_tmpOps[resi].m_Value<0) m_tmpOps[resi].m_Value*=(-1.0);
			i = m_tmpOps[i].m_ParMatch + 1;
		}
	}
	int ires = -1;
	for (i=0; i<m_tmpOps.size(); ++i)
	{
		if (!m_tmpOps[i].m_Empty)
		{
			if (ires != -1)
			{
				m_ErrMessage = "error in formula-program (no unique solution found).";
				return 0;
			}
			ires = (int)(i);
		}
	}
	if (ires == -1)
	{
		m_ErrMessage = "error in formula-program (no solution found).";
		return 0;
	}
	result = m_tmpOps[ires].m_Value;
	return 1;
}

bool CEMFormula::IsValidOp (char c)
{
	std::string vo = VALID_OP;
	if(vo.find(c)==std::string::npos)
	{
		EM_SPRINTF(m_cs, "unrecognised character '%c'.", c);
		m_ErrMessage=m_cs;
		return 0;
	}
	return 1;
}

bool CEMFormula::CheckPara()
{
	int openP=0;
	for(std::vector<COp>::iterator it=m_Ops.begin(); it!=m_Ops.end(); it++)
	{
		if (it->m_Op=='(' || it->m_Op=='[' || it->m_Op==']') ++openP;
		if (it->m_Op == ')')
		{
			if (!openP) { m_ErrMessage = "parentheses do not match."; return 0; }
			--openP;
		}
	}
	if (!openP) return 1;
	m_ErrMessage = "parentheses do not match!";
	return 0;
}

void CEMFormula::SetParaLevels ()
{
	m_MaxParLevel = 0;
	for (size_t i=0; i<m_Ops.size(); ++i)
	{
		if(m_Ops[i].m_Op != '(' && m_Ops[i].m_Op != '[' && m_Ops[i].m_Op != ']') continue;
		int level = 0, mlevel = 0;
		size_t j;
		for (j=i+1; j<m_Ops.size(); ++j)
		{
			if(m_Ops[j].m_Op == ')') { if (mlevel == level) break; ++mlevel; }
			if(m_Ops[j].m_Op == '(' || m_Ops[j].m_Op == '[' || m_Ops[j].m_Op == ']') ++level;
		}
		m_Ops[i].m_ParMatch = (int)(j);
		m_Ops[i].m_ParLevel = level;
		if (level > m_MaxParLevel) m_MaxParLevel = level;
	}
}

bool CEMFormula::CheckValidFormula ()
{
	if(m_Ops.empty()) { m_ErrMessage = "error in formula-program!"; return 0; }

	char op = m_Ops[0].m_Op, fop;
	if (op != '(' && op != '[' && op != ']' && op != NUMBER)
	{
		EM_SPRINTF(m_cs, "not permitted start %c!", op);
		m_ErrMessage=m_cs;
		return 0;
	}

	for(std::vector<COp>::iterator it=m_Ops.begin(), nit=it+1; nit!=m_Ops.end(); it++, nit++)
	{
		op = it->m_Op;
		fop = nit->m_Op;
		if (op == ')' || op == NUMBER)
		{
			if (fop == '(' || fop == '[' || fop == ']' || fop == NUMBER)
			{
				EM_SPRINTF(m_cs, "not permitted sequence '%c%c' (#=number).", op, fop);
				m_ErrMessage = m_cs;
				return 0;
			}
		}
		else // (+-*%/
		{
			if (fop != '(' && fop != '[' && fop != ']' && fop != NUMBER)
			{
				EM_SPRINTF(m_cs, "not permitted sequence %c%c", op, fop);
				m_ErrMessage = m_cs;
				return 0;
			}
		}
	}

	op = m_Ops.back().m_Op;
	if (op != ')' && op != NUMBER)
	{
		EM_SPRINTF(m_cs, "not permitted end %c!", op);
		m_ErrMessage = m_cs;
		return 0;
	}
	return 1;
}

bool CEMFormula::CalcLevel(int level, int start, int end, int &resi)
{
	for (int i=start; i<=end; ++i)
	{
		if (m_tmpOps[i].m_Empty || m_tmpOps[i].m_OpLevel != level) continue;
		int h;
		for (h=i-1; h; --h)
			if (!m_tmpOps[h].m_Empty) break;
		if (m_tmpOps[h].m_Op != NUMBER) continue;
		size_t j;
		for (j=i+1; j<m_Formula.length(); ++j)
			if (!m_tmpOps[j].m_Empty) break;
		if (m_tmpOps[j].m_Op != NUMBER) continue;

		switch (m_tmpOps[i].m_Op)
		{
			case '^':
				m_tmpOps[i].m_Value = pow(m_tmpOps[h].m_Value, m_tmpOps[j].m_Value);
				break;
			case '{':
				m_tmpOps[i].m_Value = std::fmin(m_tmpOps[h].m_Value, m_tmpOps[j].m_Value);
				break;
			case '}':
				m_tmpOps[i].m_Value = std::fmax(m_tmpOps[h].m_Value, m_tmpOps[j].m_Value);
				break;
			case '*':
				m_tmpOps[i].m_Value = m_tmpOps[h].m_Value * m_tmpOps[j].m_Value;
				break;
			case '/':
			case '\\':
				if (m_tmpOps[j].m_Value == 0)
				{
					m_ErrMessage = "division by zero!";
					return 0;
				}
				if (m_tmpOps[i].m_Op == '/')
					m_tmpOps[i].m_Value = m_tmpOps[h].m_Value / m_tmpOps[j].m_Value;
				else
					m_tmpOps[i].m_Value = int(m_tmpOps[h].m_Value) % int(m_tmpOps[j].m_Value);
				break;
			case '+':
				m_tmpOps[i].m_Value = m_tmpOps[h].m_Value + m_tmpOps[j].m_Value;
				break;
			case '-':
				m_tmpOps[i].m_Value = m_tmpOps[h].m_Value - m_tmpOps[j].m_Value;
				break;
		}
		m_tmpOps[i].m_Op = NUMBER;
		m_tmpOps[i].m_OpLevel = NO_LEVEL;
		m_tmpOps[h].m_Empty = 1;
		m_tmpOps[j].m_Empty = 1;
		resi=i;
	}
	return 1;
}

//debuging function
void COp::Print()
{
	std::cout << "level: " << m_OpLevel
		<< "; parlevel: " << m_ParLevel
		<< "; parmatch: " << m_ParMatch
		<< "; not: " << m_Not
		<< "; abs: " << m_Abs
		<< "; op: " << m_Op
		<< "; empty: " << m_Empty
		<< "; value: " << m_Value << std::endl;
}
