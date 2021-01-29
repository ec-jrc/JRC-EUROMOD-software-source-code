using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ImportExport
{
    internal class AddOnGenerator
    {
        CountryConfigFacade _addOnCountryConfigFacade = null;
        CountryConfigFacade _mergeCountryConfigFacade = null;
        CountryConfig.SystemRow _addOnSystemRow = null;
        CountryConfig.SystemRow _mergeSystemRow = null;

        string _errorMessages = string.Empty;

        internal CountryConfig.SystemRow PerformAddOnGeneration(string newSystemName, string countryShortName, string addOnShortName,
                                            string baseSystemName, string addOnSystemName)
        {
            //get the county's and add-on's data 
            CountryConfigFacade baseCountryConfigFacade = CountryAdministrator.GetCountryConfigFacade(countryShortName);
            _addOnCountryConfigFacade = CountryAdministrator.GetCountryConfigFacade(addOnShortName);

            //get the base-system and the add-on-system
            CountryConfig.SystemRow baseSystemRow = baseCountryConfigFacade.GetSystemRowByName(baseSystemName);
            _addOnSystemRow = _addOnCountryConfigFacade.GetSystemRowByName(addOnSystemName);

            //add-on-system will be stored in an CountryConfig generated for the purpose of storing it
            _mergeCountryConfigFacade = new CountryConfigFacade(countryShortName);

            //generate a copy of the base-system, by keeping all ids except the system id identically: otherwise the id-references (e.g. in ChangeParam, AddOn_Par, etc. would not work)
            _mergeSystemRow = CountryConfigFacade.CopySystemRowToAnotherCountry(baseSystemRow, _addOnSystemRow.Name,
                                                                                    _mergeCountryConfigFacade.GetCountryConfig(), true);
            _mergeSystemRow.Name = newSystemName;

            //add policies following instructions in AddOn_Pol-functions
            AddPolicies();

            //add functions following instructions in AddOn_Func-functions
            AddFunctions();

            //add parameters following instructions in AddOn_Par-functions
            AddParameters();

            //replace e.g. yse_uk_#2 respectively yse_uk_#2.4 by the actual id of this function respectively parameter
            ReplaceSymbolicIDsInChangeParam();

            return _errorMessages == string.Empty ? _mergeSystemRow : null;
        }

        void AddPolicies()
        {
            //analyse all AddOn_Pol functions
            foreach (CountryConfig.FunctionRow function_AddOnPolicy in
                        _addOnCountryConfigFacade.GetFunctionRowsBySystemIDAndFunctionName(_addOnSystemRow.ID, DefFun.AddOn_Pol))
            {
                if (function_AddOnPolicy.Switch.ToLower() != DefPar.Value.ON.ToLower())
                    continue;

                //assess which policy is to be merged and whereto by interpreting the AddOnPol-function's parameters
                CountryConfig.ParameterRow parameter_PolicyName = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnPolicy.ID, DefPar.AddOn_Pol.Pol_Name);
                CountryConfig.ParameterRow parameter_InsertAfterPolicy = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnPolicy.ID, DefPar.AddOn_Pol.Insert_After_Pol);
                CountryConfig.ParameterRow parameter_InsertBeforePolicy = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnPolicy.ID, DefPar.AddOn_Pol.Insert_Before_Pol);
                CountryConfig.ParameterRow parameter_AllowDuplicates = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnPolicy.ID, "allow_duplicates");

                if (parameter_PolicyName == null || (parameter_InsertAfterPolicy == null && parameter_InsertBeforePolicy == null))
                {
                    if (parameter_PolicyName == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnPolicy,
                                            "Compulsory parameter '" + DefPar.AddOn_Pol.Pol_Name + "' not defined.");
                    if (parameter_InsertAfterPolicy == null && parameter_InsertBeforePolicy == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnPolicy, "Compulsory parameter '" +
                                            DefPar.AddOn_Pol.Insert_After_Pol +
                                            "' (alternatively '" + DefPar.AddOn_Pol.Insert_Before_Pol + "') not defined.");
                    continue;
                }

                //search for the policy to insert in the add-on-system and for the link-policy (before/after the which the policy is to be inserted) in the just generated (copied) system
                CountryConfig.PolicyRow addOnPolicy_Policy = _addOnCountryConfigFacade.GetPolicyRowByName(_addOnSystemRow.ID, parameter_PolicyName.Value);
                bool before = parameter_InsertBeforePolicy != null ? true : false;
                string referencePolicyName = before ? parameter_InsertBeforePolicy.Value : parameter_InsertAfterPolicy.Value;
                referencePolicyName = ImportExportHelper.ReplaceCountrySymbol(referencePolicyName, _mergeCountryConfigFacade.GetCountryShortName()); //replace =cc= by country-short-name
                CountryConfig.PolicyRow addOnPolicy_ReferencePolicy = _mergeCountryConfigFacade.GetPolicyRowByName(_mergeSystemRow.ID, referencePolicyName);

                if (addOnPolicy_Policy == null || addOnPolicy_ReferencePolicy == null)
                {
                    if (addOnPolicy_Policy == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnPolicy, "Unknown policy '" + parameter_PolicyName.Value + "'.");
                    if (addOnPolicy_ReferencePolicy == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnPolicy, "Unknown policy '" + referencePolicyName + "'.");
                    continue;
                }

                //if everything is ok, insert the policy at the indicated place
                bool allowDuplicates = parameter_AllowDuplicates != null && parameter_AllowDuplicates.Value == DefPar.Value.YES;
                CountryConfig.PolicyRow addOnPolicyRow = CountryConfigFacade.CopyPolicyRow(addOnPolicy_Policy,
                                  addOnPolicy_Policy.Name,
                                  addOnPolicy_ReferencePolicy, before,
                                  addOnPolicy_Policy.Switch == DefPar.Value.NA,
                                  !allowDuplicates); //unless duplicates of policies are not foreseen: keep all ids identically
                                                     //otherwise the id-references (e.g. in ChangeParam, AddOn_Par, etc.) within the add-on would not work

                ImportExportHelper.ReplaceCountrySymbol(addOnPolicyRow, _mergeCountryConfigFacade.GetCountryShortName()); //replace all incidents of =cc= by country symbol
            }
        }

        void AddFunctions()
        {
            //analyse all AddOn_Func functions
            foreach (CountryConfig.FunctionRow function_AddOnFunction in
                        _addOnCountryConfigFacade.GetFunctionRowsBySystemIDAndFunctionName(_addOnSystemRow.ID, DefFun.AddOn_Func))
            {
                if (function_AddOnFunction.Switch.ToLower() != DefPar.Value.ON.ToLower())
                    continue;

                //assess which function is to be merged and whereto by interpreting the AddOnFunc-function's parameters
                CountryConfig.ParameterRow parameter_FunctionID = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnFunction.ID,
                                                                                                DefPar.AddOn_Func.Id_Func);
                CountryConfig.ParameterRow parameter_InsertAfterFunction = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnFunction.ID,
                                                                                                DefPar.AddOn_Func.Insert_After_Func);
                CountryConfig.ParameterRow parameter_InsertBeforeFunction = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnFunction.ID,
                                                                                                DefPar.AddOn_Func.Insert_Before_Func);
                if (parameter_FunctionID == null || (parameter_InsertAfterFunction == null && parameter_InsertBeforeFunction == null))
                {
                    if (parameter_FunctionID == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnFunction, "Compulsory parameter '" +
                                        DefPar.AddOn_Func.Id_Func + "' not defined.");
                     if (parameter_InsertAfterFunction == null && parameter_InsertBeforeFunction == null)
                         _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnFunction, "Compulsory parameter '" +
                                        DefPar.AddOn_Func.Insert_After_Func + "' (alternatively '" + DefPar.AddOn_Func.Insert_Before_Func + "') not defined.");
                    continue;
                }

                //search for the function to insert in the add-on-system and for the link-function (before/after the which the function is to be inserted) in the just generated (copied) system
                CountryConfig.FunctionRow addOnFunction_Function = _addOnCountryConfigFacade.GetFunctionRowByID(parameter_FunctionID.Value);
                if (addOnFunction_Function == null) //take account of 'symbolic' identifier, e.g. ADDON_DOSOMEWHAT_#3
                    addOnFunction_Function = ImportExportHelper.GetFunctionRowBySymbolicID(parameter_FunctionID.Value, _addOnCountryConfigFacade, _addOnSystemRow.ID);
                bool before = parameter_InsertBeforeFunction != null ? true : false;
                string referenceFunctionID = before ? parameter_InsertBeforeFunction.Value : parameter_InsertAfterFunction.Value;
                CountryConfig.FunctionRow addOnFunction_ReferenceFunction = _mergeCountryConfigFacade.GetFunctionRowByID(referenceFunctionID);
                if (addOnFunction_ReferenceFunction == null) //take account of 'symbolic' identifier
                    addOnFunction_ReferenceFunction = ImportExportHelper.GetFunctionRowBySymbolicID(referenceFunctionID, _mergeCountryConfigFacade, _mergeSystemRow.ID);

                if (addOnFunction_Function == null || addOnFunction_ReferenceFunction == null)
                {
                    if (addOnFunction_Function == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnFunction, "Unknown function '" + parameter_FunctionID.Value + "'.");
                    if (addOnFunction_ReferenceFunction == null)
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnFunction, "Unknown function '" + referenceFunctionID + "'.");
                    continue;
                }
    
                //if everything is ok, insert the function at the indicated place
                CountryConfig.FunctionRow addOnFunctionRow = CountryConfigFacade.CopyFunctionRow(addOnFunction_Function,
                                                    addOnFunction_ReferenceFunction,
                                                    before,
                                                    addOnFunction_Function.Switch == DefPar.Value.NA,
                                                    true); //keep all ids identically: otherwise the id-references (e.g. in ChangeParam, AddOn_Par, etc.) within the add-on would not work

                ImportExportHelper.ReplaceCountrySymbol(addOnFunctionRow, _mergeCountryConfigFacade.GetCountryShortName()); //replace all incidents of =cc= by country symbol
           }
        }

        void AddParameters()
        {
            //analyse all AddOn_Par functions
            foreach (CountryConfig.FunctionRow function_AddOnParameters in
                        _addOnCountryConfigFacade.GetFunctionRowsBySystemIDAndFunctionName(_addOnSystemRow.ID, DefFun.AddOn_Par))
            {
                if (function_AddOnParameters.Switch.ToLower() != DefPar.Value.ON.ToLower())
                    continue;

                //assess which parameters are to be merged and into which function by interpreting the AddOnPar-function's parameters
                CountryConfig.ParameterRow parameter_InsertFunctionID = _addOnCountryConfigFacade.GetParameterRowByName(function_AddOnParameters.ID, DefPar.AddOn_Par.Insert_Func);
                if (parameter_InsertFunctionID == null)
                {
                    _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnParameters, "Compulsory parameter '" + DefPar.AddOn_Par.Insert_Func + "' not defined.");
                    continue;
                }

                //search for the function where parameters are to be added in the just generated (copied) system
                CountryConfig.FunctionRow addOnParameters_InsertFunction = _mergeCountryConfigFacade.GetFunctionRowByID(parameter_InsertFunctionID.Value);
                if (addOnParameters_InsertFunction == null) //take account of 'symbolic' identifier, e.g. output_std_sl_#3
                    addOnParameters_InsertFunction = ImportExportHelper.GetFunctionRowBySymbolicID(parameter_InsertFunctionID.Value, _mergeCountryConfigFacade, _mergeSystemRow.ID);
                
                if (addOnParameters_InsertFunction == null)
                {
                    _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnParameters, "Unknown function '" + parameter_InsertFunctionID.Value + "'.");
                    continue;
                }

                //if everything is ok, insert the parameters (all other parameters than insert_func in AddOn_Par) into the function
                foreach (CountryConfig.ParameterRow addOnParameters_ParameterNameAndValue in function_AddOnParameters.GetParameterRows())
                {
                    if (addOnParameters_ParameterNameAndValue.Name.ToLower() == DefPar.AddOn_Par.Insert_Func.ToLower())
                        continue;

                    DefinitionAdmin.Par parDef = DefinitionAdmin.GetParDefinition(addOnParameters_InsertFunction.Name, addOnParameters_ParameterNameAndValue.Name, false);
                    if (parDef == null)
                    {
                        _errorMessages += ImportExportHelper.GenerateErrorMessage(function_AddOnParameters,
                                "Function '" + addOnParameters_InsertFunction.Name + "' does not allow for parameter '" + addOnParameters_ParameterNameAndValue.Name + "'.");
                        continue;
                    }

                    //add parameter, however if a respective parameter with value n/a exists, it can be used
                    //this is even necessary for single parameters (which may only exist once) if an add-on is added more than once (i.e. there is already the n/a parameter of the first adding-on)
                    CountryConfig.ParameterRow parameterRow = null;
                    List<CountryConfig.ParameterRow> parameterList = (from pR in addOnParameters_InsertFunction.GetParameterRows()
                                                                        where pR.Name.ToLower() == addOnParameters_ParameterNameAndValue.Name.ToLower()
                                                                        && pR.Group == addOnParameters_ParameterNameAndValue.Group
                                                                        && pR.Value == DefPar.Value.NA
                                                                        select pR).ToList();
                    if (parameterList.Count != 0)
                        parameterRow = parameterList.First();

                    if (parameterRow == null)
                        parameterRow = CountryConfigFacade.AddParameterRowAtTail(addOnParameters_InsertFunction, addOnParameters_ParameterNameAndValue.Name, parDef);
                    if (parameterRow.Name.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.PLACEHOLDER).ToLower())
                        parameterRow.Name = addOnParameters_ParameterNameAndValue.Name; //for adding parameters e.g. to function DefIL
                    parameterRow.Value = addOnParameters_ParameterNameAndValue.Value;
                    parameterRow.Comment = addOnParameters_ParameterNameAndValue.Comment;

                    ImportExportHelper.ReplaceCountrySymbol(parameterRow, _mergeCountryConfigFacade.GetCountryShortName()); //replace all incidents of =cc= by country symbol
                }
            }
        }

        void ReplaceSymbolicIDsInChangeParam()
        {
            //analyse all ChangeParam functions ...
            foreach (CountryConfig.FunctionRow function_ChangeParameters in
                        _mergeCountryConfigFacade.GetFunctionRowsBySystemIDAndFunctionName(_mergeSystemRow.ID, DefFun.ChangeParam))
            {
                //... for the parameters param_id
                foreach (CountryConfig.ParameterRow parameterRow in function_ChangeParameters.GetParameterRows())
                {
                    if (parameterRow.Name.ToLower() == DefPar.ChangeParam.Param_Id.ToLower())
                    {
                        string ID = ImportExportHelper.GetIDBySymbolicID(parameterRow.Value, _mergeCountryConfigFacade, _mergeSystemRow.ID);
                        if (ID != string.Empty)                           
                            parameterRow.Value = ID;
                    }
                }
            }
        }

        internal string GetErrorMessages()
        {
            return _errorMessages;
        }
    }
}
