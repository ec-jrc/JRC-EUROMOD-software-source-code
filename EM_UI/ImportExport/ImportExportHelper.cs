using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ImportExport
{
    internal class ImportExportHelper
    {
        const string _symbolicID_OrderSeparator = "_#";
        const string _symbolicID_ParameterSeparator = ".";

        static internal string GetSymbolicID(System.Data.DataRow row, string replaceCountryShortName = "")
        {
            if (CountryConfigFacade.IsPolicyRow(row))
                return GetSymbolicID(row as CountryConfig.PolicyRow, replaceCountryShortName);
            if (CountryConfigFacade.IsFunctionRow(row))
                return GetSymbolicID(row as CountryConfig.FunctionRow, replaceCountryShortName);
            return GetSymbolicID(row as CountryConfig.ParameterRow, replaceCountryShortName);
        }

        static internal string GetSymbolicID(CountryConfig.PolicyRow policyRow, string replaceCountryShortName = "")
        {
            string symbolicID = policyRow.Name;
            if (replaceCountryShortName != string.Empty && symbolicID.ToLower().EndsWith(replaceCountryShortName.ToLower()))
                symbolicID = symbolicID.Substring(0, symbolicID.Length - replaceCountryShortName.Length) + DefPar.Value.PLACEHOLDER_CC;
            return symbolicID;
        }

        static internal string GetSymbolicID(CountryConfig.FunctionRow functionRow, string replaceCountryShortName = "")
        {
            //for functions DefIL and DefTU use a more practical and less insecure identifier than display order, i.e. the name
            if (functionRow.Name.ToLower() == DefFun.DefIl.ToLower() || functionRow.Name.ToLower() == DefFun.DefTu.ToLower())
            {
                foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                {
                    if (parameterRow.Name.ToLower() == DefPar.DefIl.Name.ToLower())
                    {
                        string symbolicID = parameterRow.Value; //replace country symbol in taxunit name if required (e.g. tu_individual_sl -> tu_individua_=cc=)
                        if (replaceCountryShortName != string.Empty && functionRow.Name.ToLower() == DefFun.DefTu.ToLower() && symbolicID.ToLower().EndsWith(replaceCountryShortName.ToLower()))
                            symbolicID = symbolicID.Substring(0, symbolicID.Length - replaceCountryShortName.Length) + DefPar.Value.PLACEHOLDER_CC;
                        return GetSymbolicID(functionRow.PolicyRow, replaceCountryShortName) + _symbolicID_OrderSeparator + symbolicID; //e.g. ILDef_sl_#ils_dispy for the function defining ils_dispy in policy ILDef_sl
                    }
                } //should not happen, but if no name-parameter found still use order
            }

            //for all other functions use display order
            return GetSymbolicID(functionRow.PolicyRow, replaceCountryShortName) + _symbolicID_OrderSeparator + OrderToDisplayOrder(functionRow); //e.g. ILDef_sl_#10 for the 10th function in policy ILDef_sl
        }

        static internal string GetSymbolicID(CountryConfig.ParameterRow parameterRow, string replaceCountryShortName = "")
        {   //e.g. ILDef_sl_#10.3 for the 3rd parameter in the 10th function in policy ILDef_sl
            return GetSymbolicID(parameterRow.FunctionRow) + _symbolicID_ParameterSeparator + OrderToDisplayOrder(parameterRow);
        }

        static internal string GetIDBySymbolicID(string symbolicID, CountryConfigFacade countryConfigFacade, string systemID)
        {
            ReplaceCountrySymbol(symbolicID, countryConfigFacade.GetCountryShortName());

            if (symbolicID.Contains(_symbolicID_ParameterSeparator))
            {
                CountryConfig.ParameterRow parameterRow = GetParameterRowBySymbolicID(symbolicID, countryConfigFacade, systemID);
                if (parameterRow != null)
                    return parameterRow.ID;
            }
            else if (symbolicID.Contains(_symbolicID_OrderSeparator))
            {
                CountryConfig.FunctionRow functionRow = GetFunctionRowBySymbolicID(symbolicID, countryConfigFacade, systemID);
                if (functionRow != null)
                    return functionRow.ID;
            }
            else
            {
                CountryConfig.PolicyRow policyRow = countryConfigFacade.GetPolicyRowByFullName(systemID, symbolicID);
                if (policyRow != null)
                    return policyRow.ID;
            }
            
            return string.Empty;
        }

        static System.Data.DataRow GetDataRowBySymbolicID(string symbolicID, CountryConfigFacade countryConfigFacade, string systemID, bool function)
        {
            string policyName = string.Empty;
            string functionDisplayOrder = string.Empty;
            string parameterDisplayOrder = string.Empty;

            CountryConfig.FunctionRow functionRow = null;
            CountryConfig.ParameterRow parameterRow = null;

            if (GetPolicyAndOrderFromSymbolicID(symbolicID, countryConfigFacade.GetCountryShortName(), ref policyName, ref functionDisplayOrder, ref parameterDisplayOrder))
            {
                if (EM_Helpers.IsNonNegInteger(functionDisplayOrder))
                    functionRow = GetFunctionRowByPolicyNameAndDisplayOrder(countryConfigFacade, systemID, policyName, functionDisplayOrder);
                else //take account of functions DefIL and DefTU, which use the (value of the) name parameter as identifier instead of the order
                    functionRow = GetFunctionRowByPolicyNameAndValueOfSpecialParameter(countryConfigFacade, systemID, policyName,
                                                                                       DefPar.DefIl.Name, functionDisplayOrder);
            }

            if (function)
                return functionRow;

            if (functionRow != null)
                parameterRow = GetParameterRowByDisplayOrder(countryConfigFacade, functionRow.ID, parameterDisplayOrder);
            return parameterRow;
        }

        static internal CountryConfig.FunctionRow GetFunctionRowBySymbolicID(string symbolicID, CountryConfigFacade countryConfigFacade, string systemID)
        {
            return GetDataRowBySymbolicID(symbolicID, countryConfigFacade, systemID, true) as CountryConfig.FunctionRow;
        }

        static internal CountryConfig.ParameterRow GetParameterRowBySymbolicID(string symbolicID, CountryConfigFacade countryConfigFacade, string systemID)
        {
            return GetDataRowBySymbolicID(symbolicID, countryConfigFacade, systemID, false) as CountryConfig.ParameterRow;
        }

        static bool GetPolicyAndOrderFromSymbolicID(string symbolicID, string countryShortName, 
                                                    ref string policyName, ref string functionOrder, ref string parameterOrder)
        {
            symbolicID = ReplaceCountrySymbol(symbolicID, countryShortName);

            int separator = symbolicID.LastIndexOf(_symbolicID_OrderSeparator); //find _# in e.g. output_std_sl_#3 (pointing to a function) or _yse_sl#1.3 (pointing to a parameter)
            if (separator == -1 || symbolicID.EndsWith(_symbolicID_OrderSeparator))
                return false; //no symbolic id

            policyName = symbolicID.Substring(0, separator);
            functionOrder = symbolicID.Substring(separator + 2);

            separator = functionOrder.LastIndexOf(_symbolicID_ParameterSeparator);
            if (separator == -1)
                parameterOrder = string.Empty; //symbolic id of a function (no .)
            else
            {
                if (functionOrder.EndsWith(_symbolicID_ParameterSeparator) || separator == 0)
                    return false; //no symbolic id
                parameterOrder = functionOrder.Substring(separator + 1);
                functionOrder = functionOrder.Substring(0, separator);
            }

            return true;
        }

        //intermediate solution for error-output, needs to be integrated into a user interface produced error-log once available (currently error-log is generated by executable)
        static internal string GenerateErrorMessage(CountryConfig.FunctionRow addOnFunction, string error)
        {
            string errorMessage =
                "Error:      " + error + Environment.NewLine +
                "System:     " + addOnFunction.PolicyRow.SystemRow.Name + " (Order: " + addOnFunction.PolicyRow.SystemRow.Order + ")" + Environment.NewLine +
                "Policy:     " + addOnFunction.PolicyRow.Name + " (Order: " + addOnFunction.PolicyRow.Order + ")" + Environment.NewLine +
                "Function:   " + addOnFunction.Name + " (Order: " + addOnFunction.Order + ")" + Environment.NewLine +
                "Identifier: " + addOnFunction.ID + Environment.NewLine + Environment.NewLine;
            return errorMessage;
        }

        static internal void WriteErrorLogFile(string errlogPath, string errors)
        {
            string dateTimePrefix = string.Format("{0:yyyyMMddHHmm}", DateTime.Now);
            string fileName = EMPath.AddSlash(errlogPath) + dateTimePrefix + EM_XmlHandler.TAGS.EM2CONFIG_errLogAddOnFileName;

            string heading = "=====================================================================================================================" + Environment.NewLine;
            heading += DefGeneral.BRAND_TITLE + " ADD-ON ERROR LOG" + Environment.NewLine;
            heading += "=====================================================================================================================" + Environment.NewLine;

            System.IO.TextWriter textWriter = new System.IO.StreamWriter(fileName);
            textWriter.Write(heading + errors);
            textWriter.Close();

            Tools.UserInfoHandler.ShowError(errors);
        }

        static internal void ReplaceCountrySymbol(CountryConfig.PolicyRow policyRow, string countryShortName)
        {
            policyRow.Name = ReplaceCountrySymbol(policyRow.Name, countryShortName);
            foreach (CountryConfig.FunctionRow functionRow in policyRow.GetFunctionRows())
                ReplaceCountrySymbol(functionRow, countryShortName);
        }

        static internal void ReplaceCountrySymbol(CountryConfig.FunctionRow functionRow, string countryShortName)
        {
            foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                ReplaceCountrySymbol(parameterRow, countryShortName);
        }

        static internal void ReplaceCountrySymbol(CountryConfig.ParameterRow parameterRow, string countryShortName)
        {
            parameterRow.Value = ReplaceCountrySymbol(parameterRow.Value, countryShortName);
        }

        static internal string ReplaceCountrySymbol(string component, string countryShortName)
        {
            return component.Replace(DefPar.Value.PLACEHOLDER_CC, countryShortName);
        }

        static string OrderToDisplayOrder(CountryConfig.FunctionRow functionRow)
        {
            List <CountryConfig.FunctionRow> siblingFunctions = functionRow.PolicyRow.GetFunctionRows().OrderBy(f => long.Parse(f.Order)).ToList();
            return (siblingFunctions.IndexOf(functionRow) + 1).ToString();
        }

        static string OrderToDisplayOrder(CountryConfig.ParameterRow parameterRow)
        {
            List<CountryConfig.ParameterRow> siblingParameters = parameterRow.FunctionRow.GetParameterRows().OrderBy(p => long.Parse(p.Order)).ToList();
            return (siblingParameters.IndexOf(parameterRow) + 1).ToString();
        }


        static CountryConfig.ParameterRow GetParameterRowByDisplayOrder(CountryConfigFacade countryConfigFacade, string functionID, string displayOrder)
        {
            try
            {
                CountryConfig.FunctionRow functionRow = countryConfigFacade.GetFunctionRowByID(functionID);
                int displayIndex = Convert.ToInt32(displayOrder) - 1;

                CountryConfig.ParameterRow par = null;
                if (functionRow != null && displayIndex >= 0)
                {
                    List<CountryConfig.ParameterRow> siblingParameters = functionRow.GetParameterRows().OrderBy(p => long.Parse(p.Order)).ToList();
                    if (displayIndex < siblingParameters.Count) par = siblingParameters.ElementAt(displayIndex);
                }
                return par;
            }
            catch { return null; }
        }

        static CountryConfig.FunctionRow GetFunctionRowByPolicyNameAndDisplayOrder(CountryConfigFacade countryConfigFacade,
                                                                    string systemID, string policyName, string displayOrder)
        {
            try
            {
                CountryConfig.PolicyRow policyRow = countryConfigFacade.GetPolicyRowByFullName(systemID, policyName);
                int displayIndex = Convert.ToInt32(displayOrder) - 1;

                CountryConfig.FunctionRow func = null;
                if (policyRow != null && displayIndex >= 0)
                {
                    List<CountryConfig.FunctionRow> siblingFunctions = policyRow.GetFunctionRows().OrderBy(f => long.Parse(f.Order)).ToList();
                    if (displayIndex < siblingFunctions.Count) func = siblingFunctions.ElementAt(displayIndex);
                }
                return func;
            }
            catch { return null; }
        }

        static CountryConfig.FunctionRow GetFunctionRowByPolicyNameAndValueOfSpecialParameter(CountryConfigFacade countryConfigFacade,
                                    string systemID, string policyName, string specialParameterName, string specialParameterValue)
        {
            try
            {
                List<CountryConfig.ParameterRow> parameterRows = (from parameter in countryConfigFacade.GetCountryConfig().Parameter
                                                                  where parameter.FunctionRow.PolicyRow.SystemID == systemID
                                                                      && parameter.FunctionRow.PolicyRow.Name.ToLower() == policyName.ToLower()
                                                                      && parameter.Name.ToLower() == specialParameterName.ToLower()
                                                                      && parameter.Value.ToLower() == specialParameterValue.ToLower()
                                                                  select parameter).ToList();
                if (parameterRows.Count != 1)
                    return null;
                return parameterRows.First().FunctionRow;
            }
            catch { return null; }
        }
    }
}
