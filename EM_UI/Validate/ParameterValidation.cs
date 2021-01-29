using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.Validate
{
    class ParameterValidation
    {
        const string _additionallyAllowedChar_TypeString = "$.&+-*? ";
        const string _additionallyAllowedChar_TypeFormula = " $!%/\\()=*+#-.,<>^[]";
        const string _additionallyAllowedChar_TypeCondition = _additionallyAllowedChar_TypeFormula + "&|{}";

        static bool ValidateAmount(string value, SystemTreeListTag systemColumnTag, ref string errorText)
        {
            //double min = double.MinValue;
            //double max = double.MaxValue;
            //bool isInteger = false;
            //bool allowsPeriod = true;
            //EM_AppContext.Instance.GetFunctionConfigFacade().GetAmountParameterInfo(configParameterRow, ref isInteger, ref min, ref max, ref allowsPeriod);

            //if (allowsPeriod)
            //    value = RemoveTailPeriodSymbol(value); //remove #m, #w, etc. at the end

            //if (isInteger)
            //{
            //    if (!EM_Helpers.IsNonNegInteger(value))
            //    {
            //        errorText = value + " is not a valid integer";
            //        return false;
            //    }
            //}
            //else
            //{
            //    if (!EM_Helpers.IsNumeric(value))
            //    {
            //        errorText = value + " is not a valid number";
            //        return false;
            //    }
            //}

            //bool errorOccured = true; //issuing an error message is not wanted here (error happens if value is empty)
            //double valueAsNumber = ConvertToDouble(value, ref errorOccured);
            //if (errorOccured)
            //{
            //    errorText = value + " is not a valid number";
            //    return false;
            //}

            //if (valueAsNumber < min)
            //{
            //    errorText = value + " is lower than minimum (" + min.ToString() + ")";
            //    return false;
            //}
            //if (valueAsNumber > max)
            //{
            //    errorText = value + " exceeds maximum (" + min.ToString() + ")";
            //    return false;
            //}

            return true; // validation is since ever switched off, as it was not reliable enough, with removing FuncConfig we also drop the (actually never properly filled) info
        }

        static bool ValidateString(string value, SystemTreeListTag systemColumnTag, ref string errorText)
        {
            if (EM_Helpers.ContainsIllegalChar(value, ref errorText, _additionallyAllowedChar_TypeString))
                return false;
            return true;
        }

        static bool ValidateFormula(string value, SystemTreeListTag systemColumnTag, ref string errorText)
        {
            if (EM_Helpers.ContainsIllegalChar(value, ref errorText, _additionallyAllowedChar_TypeFormula))
                return false;
            Dictionary<char, char> parenthesesPairsToCheck = new Dictionary<char, char>();
            parenthesesPairsToCheck.Add('(', ')');
            if (!DoParenthesesMatch(value, ref errorText, parenthesesPairsToCheck))
                return false;
            return true;
        }

        static bool ValidateCondition(string value, SystemTreeListTag systemColumnTag, ref string errorText)
        {
            if (EM_Helpers.ContainsIllegalChar(value, ref errorText, _additionallyAllowedChar_TypeCondition))
                return false;
            Dictionary<char, char> parenthesesPairsToCheck = new Dictionary<char, char>();
            parenthesesPairsToCheck.Add('(', ')');
            parenthesesPairsToCheck.Add('{', '}');
            if (!DoParenthesesMatch(value, ref errorText, parenthesesPairsToCheck))
                return false;
            return true;
        }

        static bool ValidateVariable(string value, SystemTreeListTag systemColumnTag, ref string errorText)
        {
            //if ((from variableGeneratedByDefVar in systemColumnTag.vars
            //     where variableGeneratedByDefVar.Value.ToLower() == value.ToLower()
            //     select variableGeneratedByDefVar).ToList().Count > 0)
            //    return true;
            //if ((from variableGeneratedByDefConst in systemColumnTag.consts
            //     where variableGeneratedByDefConst.Value.ToLower() == value.ToLower()
            //     select variableGeneratedByDefConst).ToList().Count > 0)
            //    return true;
            //if ((from standardVariable in EM_AppContext.Instance.VarConfigFacade.getVariableNames()
            //     where standardVariable.ToLower() == value.ToLower()
            //     select standardVariable).ToList().Count > 0)
            //    return true;

            //errorText = "not a valid variable name";
            //return false;

            return true; //currently not reasonable, as not all possible variables are considered (e.g. those generated by Store)
        }

        static bool ValidateVariableIncomelist(string value, SystemTreeListTag systemColumnTag, ref string errorText)
        {
            if ((from ILGeneratedByDefIL in systemColumnTag.GetParameterRowsILs()
                 where ILGeneratedByDefIL.Value.ToLower() == value.ToLower()
                 select ILGeneratedByDefIL).ToList().Count > 0)
                return true;

            if (ValidateVariable(value, systemColumnTag, ref errorText))
                return true;

            errorText = "neither a valid variable nor incomelist name";
            return false;
        }

        internal static bool ValidateEditorInput(string value, TreeListNode node, TreeListColumn column, ref string errorText)
        {
            if (value == DefPar.Value.NA)
                return true;

            CountryConfig.ParameterRow countryParameterRow = (node.Tag as ParameterTreeListTag).GetDefaultParameterRow();
            SystemTreeListTag systemColumnTag = column.Tag as SystemTreeListTag;

            switch (DefinitionAdmin.GetParDefinition(countryParameterRow.FunctionRow.Name, countryParameterRow.Name).valueType)
            {
                case DefPar.PAR_TYPE.FORMULA: return ValidateFormula(value, systemColumnTag, ref errorText);
                case DefPar.PAR_TYPE.CONDITION: return ValidateCondition(value, systemColumnTag, ref errorText);
                case DefPar.PAR_TYPE.TEXT: return ValidateString(value, systemColumnTag, ref errorText);
                case DefPar.PAR_TYPE.NUMBER: return ValidateAmount(value, systemColumnTag, ref errorText);
                case DefPar.PAR_TYPE.VAR: return ValidateVariable(value, systemColumnTag, ref errorText);
                case DefPar.PAR_TYPE.VARorIL: return ValidateVariableIncomelist(value, systemColumnTag, ref errorText);
            }
            return true;
        }

        internal static bool ValidateGroupInput(string value, string parameterName, ref string errorText)
        {
            if (value == string.Empty)
                return true;

            if (parameterName != "-" && parameterName != "+")
            {   //in princple only integer values are allowed ...
                if (!EM_Helpers.IsNonNegInteger(value))
                {
                    errorText = "value is not a non-negative integer";
                    return false;
                }
            }
            else
            {   //... however for (phase out !!!) + and - parameter of DefIL group column may contain a decimal number
                if (!EM_Helpers.IsNumeric(value))
                {
                    errorText = "value is not a non-negative number";
                    return false;
                }
            }

            bool errorOccured = true;  //issuing an error message is not wanted here (error happens if value is empty)
            double valueAsNumber = ConvertToDouble(value, ref errorOccured);
            if (errorOccured)
            {
                errorText = value + " is not a valid number";
                return false;
            }

            if (valueAsNumber < 0)
            {
                errorText = "value is not non-negative";
                return false;
            }

            return true;
        }

        //returns the footnote parameters which need to be added when a formula was changed
        //e.g. formula contains 'Amount#4711' (and did not contain that before the change): footnote parameter #Amount with group 4711 must be added
        internal static Dictionary<KeyValuePair<string, string>, DefinitionAdmin.Par> GetFootnoteParametersToAdd(TreeListNode node, ref string formulaText)
        {
            Dictionary<KeyValuePair<string, string>, DefinitionAdmin.Par> footnoteParametersToAdd =
                        new Dictionary<KeyValuePair<string, string>, //name and group of new footnote parameter
                                             DefinitionAdmin.Par>(); //definition (from config file) of new parameter

            int nextFootnote = TreeListManager.GetNextAvailableFootnoteCounter(node.ParentNode);
            string functionName = node.ParentNode.GetDisplayText(TreeListBuilder._policyColumnName);

            //search for Amount#xi (i.e Amount#x1, Amount#x2, etc.)
            for (int indexLabelAmount = formulaText.ToLower().IndexOf(DefPar.Value.AMOUNT.ToLower() + "#x"), indexNonDigit;
                     indexLabelAmount >= 0;
                     indexLabelAmount = formulaText.ToLower().IndexOf(DefPar.Value.AMOUNT.ToLower() + "#x"))
            {
                for (indexNonDigit = indexLabelAmount + DefPar.Value.AMOUNT.Length + 2;
                     indexNonDigit < formulaText.Length && EM_Helpers.IsDigit(formulaText.ElementAt(indexNonDigit));
                     ++indexNonDigit)
                    ; //search first non digit after Amount#x

                string parameterName = "#_" + DefPar.Value.AMOUNT; //#_Amount
                DefinitionAdmin.Par parDef = DefinitionAdmin.GetParDefinition(functionName, parameterName);
                KeyValuePair<string, string> parameterNameAndGroup = new KeyValuePair<string, string>(parameterName, nextFootnote.ToString());
                footnoteParametersToAdd.Add(parameterNameAndGroup, parDef); //put parameter into list of footnote parameters which need to be generated
                string toReplace = SubstringFromTo(formulaText, indexLabelAmount, indexNonDigit-1); //Amount#xi (e.g. Amount#x1)
                string replaceBy = DefPar.Value.AMOUNT + "#" + nextFootnote.ToString(); //Amount#f (e.g. Amount#x3)
                formulaText = formulaText.Replace(toReplace, replaceBy); //replace all occurrences (there might be more than one)
                ++nextFootnote;
            }

            //search for #xi[_yyy] (i.e. #x1[_Level], #x2[_Level], #x1[_UpLim], etc.)
            for (int indexPlaceholder = formulaText.IndexOf("[_");
                     indexPlaceholder >= 0;
                     indexPlaceholder = formulaText.IndexOf("[_"))
            {
                int iStart = formulaText.Substring(0, indexPlaceholder).LastIndexOf("#x");
                int iEnd = formulaText.Substring(indexPlaceholder).IndexOf("]") + indexPlaceholder;
                if (iStart >= 0 && iEnd >= 0 &&
                    EM_Helpers.IsNonNegInteger(SubstringFromTo(formulaText, iStart + 2, indexPlaceholder - 1)))
                {
                    string parameterName = "#" + SubstringFromTo(formulaText, indexPlaceholder + 1, iEnd - 1); //#_yyy (e.g. #_Level)
                    DefinitionAdmin.Fun funDef = DefinitionAdmin.GetFunDefinition(functionName, false); // check if function allows for this footnote
                    if (funDef != null && (from p in funDef.GetParList() where p.Value.isFootnote && p.Key.ToLower() == parameterName.ToLower() select p).Count() > 0)
                    {
                        DefinitionAdmin.Par parDef = DefinitionAdmin.GetParDefinition(functionName, parameterName);
                        KeyValuePair<string, string> parameterNameAndGroup = new KeyValuePair<string, string>(parameterName, nextFootnote.ToString());
                        footnoteParametersToAdd.Add(parameterNameAndGroup, parDef); //put parameter into list of footnote parameters which need to be generated
                        string toReplace = SubstringFromTo(formulaText, iStart, iEnd); //#xi_yyy (e.g. #x1[_Level])
                        string replaceBy = "#" + nextFootnote.ToString(); //#f_yyy (e.g. #4)
                        formulaText = formulaText.Replace(toReplace, replaceBy); //replace all occurrences (there might be more than one)
                        ++nextFootnote;
                    }
                    else
                        break; //should not happen (though can), but to avoid infinite loop
                }
                else
                    break; //should not happen (though can), but to avoid infinite loop
            }

            //search for query#x (e.g. isNtoMchild#x)
            for (int indexPlaceholder = formulaText.IndexOf("#x");
                     indexPlaceholder >= 0;
                     indexPlaceholder = formulaText.IndexOf("#x"))
            {
                string theQuery = string.Empty;
                foreach (string queryName in DefinitionAdmin.GetQueryNamesAndDesc().Keys)
                {
                    string potQueryName = SubstringFromTo(formulaText, indexPlaceholder - (queryName.Length - 2), indexPlaceholder + 1).ToLower();
                    if (potQueryName == queryName.ToLower())
                    {
                        theQuery = queryName;
                        break;
                    }
                }
                if (theQuery == string.Empty)
                    formulaText = formulaText.Substring(0, indexPlaceholder) + "#°" +
                                    formulaText.Substring(indexPlaceholder + 2); //no query found, replace #x preliminary by #° (change back below)
                else //add all parameters of the query
                {
                    Dictionary<string, DefinitionAdmin.Par> queryParameters = new Dictionary<string, DefinitionAdmin.Par>();
                    DefinitionAdmin.GetQueryDefinition(theQuery, out DefinitionAdmin.Query queryDef, out string dummy, false);
                    if (queryDef != null) queryParameters = queryDef.par;
                    List<string> alreadyCoveredByAlias = new List<string>(); // avoid adding e.g. #_income as well as #_info
                    foreach (var q in queryParameters)
                    {
                        string queryParName = q.Key; DefinitionAdmin.Par queryParDef = q.Value;
                        if (alreadyCoveredByAlias.Contains(queryParName)) continue;
                        footnoteParametersToAdd.Add(new KeyValuePair<string, string>(queryParName, nextFootnote.ToString()), queryParDef); //put parameter into list of footnote parameters which need to be generated
                        alreadyCoveredByAlias.AddRange(queryParDef.substitutes);
                    }
                    formulaText = formulaText.Substring(0, indexPlaceholder) + "#" + nextFootnote.ToString() +
                                  formulaText.Substring(indexPlaceholder + 2); //replace #x by #f (e.g. #x by #5)
                    ++nextFootnote;
                }
            }
            formulaText = formulaText.Replace("#°", "#x");

            return footnoteParametersToAdd;
        }

        private static string SubstringFromTo(string value, int indexFirstChar, int indexLastChar)
        {
            indexFirstChar = Math.Max(indexFirstChar, 0);
            value = value.Substring(indexFirstChar);
            int len = Math.Max(indexLastChar - indexFirstChar + 1, 0);
            len = Math.Min(len, value.Length);
            return value.Substring(0, len);
        }

        private static bool DoParenthesesMatch(string toTest, ref string errorText, Dictionary<char, char> parenthesesPairsToCheck)
        {
            //check if opened Parentheses of a type matches closed
            Dictionary<char, char> parenthesesPairsToCheck_reversed = new Dictionary<char, char>();
            foreach (char openParenthesis in parenthesesPairsToCheck.Keys)
            {
                int countOpen = (from c in toTest where c == openParenthesis select c).ToList().Count;
                int countClose = (from c in toTest where c == parenthesesPairsToCheck[openParenthesis] select c).ToList().Count;
                if (countOpen != countClose)
                {
                    errorText = "Parentheses do not match: " + countOpen.ToString() + " '" + openParenthesis + "' but " + countClose.ToString() + " '" + parenthesesPairsToCheck[openParenthesis] + "'";
                    return false;
                }
                parenthesesPairsToCheck_reversed.Add(parenthesesPairsToCheck[openParenthesis], openParenthesis); //reverse key and value for check below
            }

            //check for sequences like { a + ( b - c } ) 
            string currentlyOpenParentheses = string.Empty;
            foreach (char c in toTest)
            {
                if (parenthesesPairsToCheck_reversed.ContainsValue(c))
                    currentlyOpenParentheses += c;
                else if (parenthesesPairsToCheck_reversed.ContainsKey(c))
                {
                    if (currentlyOpenParentheses.Last() != parenthesesPairsToCheck_reversed[c])
                    {
                        errorText = "Trying to close '" + currentlyOpenParentheses.Last() + "' with '" + c + "'";
                        return false;
                    }
                    currentlyOpenParentheses = currentlyOpenParentheses.Substring(0, currentlyOpenParentheses.Length - 1);
                }
            }
            return true;
        }

        private static string RemoveTailPeriodSymbol(string value)
        {
            value = value.TrimEnd();
            foreach (string period in DefPeriod.GetPeriods().Keys)
            {
                if (value.ToLower().EndsWith(period))
                {
                    value = value.Substring(0, value.Length - period.Length);
                    break;
                }
            }
            return value;
        }

        private static double ConvertToDouble(string toConvert, ref bool reportErrorSilently)
        { // this somewhat more complicated call replicates the original behaviour
            if (!EM_Helpers.TryConvertToDouble(toConvert, out double result))
            {
                if (reportErrorSilently) reportErrorSilently = true; // just inform the caller that an error occured
                else Tools.UserInfoHandler.ShowError(string.Format("{0} is not a valid number.", toConvert)); // issue the error message
            }
            else reportErrorSilently = false;
            return result;
        }
    }
}
