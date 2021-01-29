using DevExpress.XtraTreeList.Columns;
using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.GlobalAdministration;
using EM_UI.NodeOperations;
using EM_UI.Tools;
using EM_UI.UndoManager;
using EM_UI.Validate;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ImportExport
{
    internal class ImportExportAdministrator
    {
        static System.Drawing.Color _colorForMarkingDifferences = System.Drawing.Color.YellowGreen;

        static internal void ImportAddOn(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            ImportAddOnForm importAddOnForm = new ImportAddOnForm((from system in countryConfigFacade.GetSystemRows() select system.Name).ToList());
            if (importAddOnForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            string backUpFolder = RestoreManager.StoreCountry(mainForm);
            if (backUpFolder == string.Empty)
                return;

            mainForm.Cursor = Cursors.WaitCursor;
            try
            {
                //first generate the add-on system
                AddOnGenerator addOnGenerator = new AddOnGenerator();
                CountryConfig.SystemRow addOnSystemRow = addOnGenerator.PerformAddOnGeneration(importAddOnForm._addOnSystemName, //new system name (same as add-on system name)
                            countryConfigFacade.GetCountryShortName(), importAddOnForm._addOnShortName, importAddOnForm._baseSystemName, importAddOnForm._addOnSystemName);
                if (addOnSystemRow == null)
                {
                    Tools.UserInfoHandler.ShowError(addOnGenerator.GetErrorMessages());
                    mainForm.Cursor = Cursors.Default;
                    return;
                }

                //then import the just generated add-on system into the country
                string newName = addOnSystemRow.Name; //check if the system must be renamed as a system with the same name already exists
                if (!GetNewNameForExistingSystem(countryConfigFacade, ref newName))
                    return; //user pressed cancel when aksed for new name
                Dictionary<string, string> dummy = null; //this parameter is only necessary if more than one system is imported, thus not relevant here
                CountryConfig.SystemRow newSystemRow = ImportSystem(countryConfigFacade, addOnSystemRow, newName, ref dummy);
                if (newSystemRow == null)
                    new System.ArgumentException(string.Empty); //should actually not be necessary to check (an error in ImportSystem should anyway land in the catch-brach)

                //finally copy dataset configuration of base system (i.e. all datasets available for the base system are available for the importet add-on system)
                dataConfigFacade.CopyDBSystemConfigRows(countryConfigFacade.GetSystemRowByName(importAddOnForm._baseSystemName), newSystemRow);

                RestoreManager.SaveDirectXMLAction(mainForm);
                mainForm.UpdateTree();

                RestoreManager.ReportSuccessAndInfo("Importing add-on", backUpFolder);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception, "Failed to import add-on.", false);
                RestoreManager.RestoreCountry(mainForm, backUpFolder);
            }
            finally
            {
                mainForm.Cursor = Cursors.Default;
            }
        }

        static internal void ExportAddOn(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            ExportAddOnForm exportAddOnForm = new ExportAddOnForm((from system in countryConfigFacade.GetSystemRows() select system.Name).ToList());
            if (exportAddOnForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            string backUpFolder = RestoreManager.StoreCountry(mainForm);
            if (backUpFolder == string.Empty)
                return;

            try
            {
                ExportAddOn(countryConfigFacade, exportAddOnForm);

                //remove exported add-on system from the country's xml-files and delete everything that got n/a in all systems
                if (exportAddOnForm.radExportAndDelete.Checked == true)
                {
                    mainForm.Cursor = Cursors.WaitCursor;
                    RemoveSystems(countryConfigFacade, new List<string>() { exportAddOnForm._addOnSystemName });
                    RemoveDataConfigurations(dataConfigFacade, new List<string>() { exportAddOnForm._addOnSystemName });
                    RestoreManager.SaveDirectXMLAction(mainForm);
                    mainForm.UpdateTree();
                }

                RestoreManager.ReportSuccessAndInfo("Exporting system '" + exportAddOnForm._addOnSystemName +
                                                    "' to add-on '" + exportAddOnForm.txtShortName.Text + "' (see Add-Ons gallery)", backUpFolder);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception, "Failed to export add-on.", false);
                RestoreManager.RestoreCountry(mainForm, backUpFolder);
            }
            finally
            {
                mainForm.Cursor = Cursors.Default;
            }
        }

        static void ExportAddOn(CountryConfigFacade countryConfigFacade, ExportAddOnForm exportAddOnForm)
        {
            CountryConfig.SystemRow derivedSystem = countryConfigFacade.GetSystemRowByName(exportAddOnForm._addOnSystemName);
            CountryConfig.SystemRow baseSystem = countryConfigFacade.GetSystemRowByName(exportAddOnForm._baseSystemName);

            //create the add-on and equip it with the always necessary elements (policy for definitions, policy for implementations, etc.)
            Country addOn;
            CountryConfig.PolicyRow addOnDefPolicy; //policy for definitions: to be filled with functions AddOnApplic, AddOn_Pol, AddOn_Func and AddOn_Par
            CountryConfig.PolicyRow addOnImpPolicy; //policy for implementations: to be filled with policies which are not defined in the bases system and ...
            CountryConfig.FunctionRow changeParamFunction; //... special function for changing swiches of policies/functions and values of parameters which differ in the two systems
            CreateAndPrepareAddOn(exportAddOnForm, baseSystem, out addOn, out addOnDefPolicy, out addOnImpPolicy, out changeParamFunction);

            //check the differences of the 'derived system' (i.e. the one to export) and the base system and reflect them in the add-on
            CountryConfigFacade addOnCountryConfigFacade = addOn.GetCountryConfigFacade(); //some preparations for the comparison (see below for the use)
            CountryConfig countryConfig = countryConfigFacade.GetCountryConfig();
            List<CountryConfig.PolicyRow> derivedPolicies = (from pR in countryConfig.Policy where pR.SystemID == derivedSystem.ID select pR).OrderBy(pR => long.Parse(pR.Order)).ToList();
            List<CountryConfig.PolicyRow> basePolicies = (from pR in countryConfig.Policy where pR.SystemID == baseSystem.ID select pR).OrderBy(pR => long.Parse(pR.Order)).ToList();
            List<string> basePolicySwitches = (from pol in basePolicies select pol.Switch).ToList();
            List<string> derivedPolicySwitches = (from pol in derivedPolicies select pol.Switch).ToList();

            int changeParamGroupIndex = 1;
            //check differences in policies:
            for (int iPolicy = 0; iPolicy < derivedPolicies.Count; ++iPolicy) //this assumes that base and derived system (must) have completely the same structure in terms of the order of polcies, functions and parameters
            {
                CountryConfig.PolicyRow derivedPolicy = derivedPolicies.ElementAt(iPolicy);
                CountryConfig.PolicyRow basePolicy = basePolicies.ElementAt(iPolicy);

                //policy switch is different ...
                if (derivedPolicy.Switch.ToLower() != basePolicy.Switch.ToLower())
                {
                    if (basePolicy.Switch == DefPar.Value.NA) //... policy only relevant for the derived system: create an add-on-policy
                    {
                        //copy the policy to the add-ons implementation policy
                        CountryConfig.PolicyRow addOnPolicy = CountryConfigFacade.CopyPolicyRow(derivedPolicy, derivedPolicy.Name,
                                                                                    addOnDefPolicy.SystemRow.GetPolicyRows().Last(),
                                                                                    false, //copyBeforeNeighbour (i.e. copy after last policy row)
                                                                                    false, //switchNA (is corrected below)
                                                                                    true); //keepIDs
                        addOnPolicy.Switch = derivedPolicy.Switch;

                        //add a respective AddOn_Pol function to the add-ons definition policy
                        CountryConfig.FunctionRow addPolFunction = CountryConfigFacade.AddFunctionRowAtTail(DefFun.AddOn_Pol, addOnDefPolicy,
                                                        DefPar.Value.ON);
                        CountryConfig.ParameterRow addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addPolFunction,
                                    DefPar.AddOn_Pol.Pol_Name, DefinitionAdmin.GetParDefinition(DefFun.AddOn_Pol, DefPar.AddOn_Pol.Pol_Name));
                        addOnParameter.Value = derivedPolicy.Name;
                        //search for 'neighbour policy', i.e. the policy after or before which the add-on policy should be added in the spine
                        int index = iPolicy;
                        bool insertAfter;
                        bool isBase;
                        if (!SearchForAddOnNeighbour(basePolicySwitches, derivedPolicySwitches, ref index, out insertAfter, out isBase)) //failing is hopefully rather unlikely, therefore just let the mechanism crash after a warning
                            UserInfoHandler.ShowError("Policy '" + derivedPolicy.Name + "' cannot be linked to any policy in the base system.");
                        string beforeAfter = insertAfter ? DefPar.AddOn_Pol.Insert_After_Pol : DefPar.AddOn_Pol.Insert_Before_Pol;
                        addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addPolFunction, beforeAfter, 
                                    DefinitionAdmin.GetParDefinition(DefFun.AddOn_Pol, beforeAfter));
                        CountryConfig.PolicyRow neighbourPolicy = isBase ? basePolicies.ElementAt(index) : derivedPolicies.ElementAt(index);
                        addOnParameter.Value = neighbourPolicy.Name;

                        continue; //no further action (i.e. checking of policy content) necessary
                    }
                    else //... switch is different
                    {
                        CountryConfig.ParameterRow addOnParameter = CountryConfigFacade.AddParameterRowAtTail(changeParamFunction,
                            DefPar.ChangeParam.Param_Id, DefinitionAdmin.GetParDefinition(DefFun.ChangeParam, DefPar.ChangeParam.Param_Id));
                        addOnParameter.Value = basePolicy.ID;
                        addOnParameter.Group = changeParamGroupIndex.ToString();
                        addOnParameter.Comment = "policy '" + basePolicy.Name + "'";
                        addOnParameter = CountryConfigFacade.AddParameterRowAtTail(changeParamFunction,
                            DefPar.ChangeParam.Param_NewVal, DefinitionAdmin.GetParDefinition(DefFun.ChangeParam, DefPar.ChangeParam.Param_NewVal));
                        addOnParameter.Value = derivedPolicy.Switch;
                        addOnParameter.Group = changeParamGroupIndex.ToString();
                        ++changeParamGroupIndex;

                        if (derivedPolicy.Switch == DefPar.Value.NA)
                            continue; //not necessary to check the content of the policy
                    }
                }

                if (basePolicy.Switch == DefPar.Value.OFF) //in practise it is not possible to change the switch of switched-off policies, but leave any such mistake to be reported by the executable and corrected by the developer
                    continue; //any attempt to change the content of policies switched off in the base would lead to errors that might be hard to understand, as the policy is not read and therefore unknown

                //check differences in functions
                List<CountryConfig.FunctionRow> derivedFunctions = (from fR in countryConfig.Function where fR.PolicyID == derivedPolicy.ID select fR).OrderBy(fR => long.Parse(fR.Order)).ToList();
                List<CountryConfig.FunctionRow> baseFunctions = (from fR in countryConfig.Function where fR.PolicyID == basePolicy.ID select fR).OrderBy(fR => long.Parse(fR.Order)).ToList();
                for (int iFunction = 0; iFunction < derivedFunctions.Count; ++iFunction)
                {
                    CountryConfig.FunctionRow derivedFunction = derivedFunctions.ElementAt(iFunction);
                    CountryConfig.FunctionRow baseFunction = baseFunctions.ElementAt(iFunction);
                    List<string> baseFunctionSwitches = (from func in baseFunctions select func.Switch).ToList();
                    List<string> derivedFunctionSwitches = (from func in derivedFunctions select func.Switch).ToList();

                    //function switch is different ...
                    if (derivedFunction.Switch.ToLower() != baseFunction.Switch.ToLower())
                    {
                        if (baseFunction.Switch == DefPar.Value.NA) //... function only relevant for the derived system: create an add-on-function
                        {
                            //copy the function to the add-ons implementation policy
                            CountryConfig.FunctionRow addOnFunction = CountryConfigFacade.CopyFunctionRow(derivedFunction,
                                                                                        addOnImpPolicy.GetFunctionRows().Last(),
                                                                                        false, //copyBeforeNeighbour (i.e. copy after last policy row)
                                                                                        false, //switchNA (is corrected below)
                                                                                        true); //keepIDs
                            addOnFunction.Switch = derivedFunction.Switch;

                            //add a respective AddOn_Func function to the add-ons definition policy
                            CountryConfig.FunctionRow addFuncFunction = CountryConfigFacade.AddFunctionRowAtTail(DefFun.AddOn_Func, addOnDefPolicy,
                                                            DefPar.Value.ON);
                            CountryConfig.ParameterRow addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addFuncFunction,
                                DefPar.AddOn_Func.Id_Func, DefinitionAdmin.GetParDefinition(DefFun.AddOn_Func, DefPar.AddOn_Func.Id_Func));
                            addOnParameter.Value = addOnFunction.ID;
                            addOnParameter.Comment = "function '" + addOnFunction.Name + "' in policy '" + addOnFunction.PolicyRow.Name + "'";
                            //search for 'neighbour function', i.e. the function after or before which the add-on function should be added in the spine
                            int index = iFunction;
                            bool insertAfter;
                            bool isBase;
                            if (!SearchForAddOnNeighbour(baseFunctionSwitches, derivedFunctionSwitches, ref index, out insertAfter, out isBase)) //failing is hopefully rather unlikely, therefore just let the mechanism crash after a warning
                                Tools.UserInfoHandler.ShowError("Function '" + derivedFunction.Name + "' in policy '" + derivedFunction.PolicyRow.Name + "' cannot be linked to any function in the base system.");
                            string beforeAfter = insertAfter ? DefPar.AddOn_Func.Insert_After_Func : DefPar.AddOn_Func.Insert_Before_Func;
                            addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addFuncFunction,
                                beforeAfter, DefinitionAdmin.GetParDefinition(DefFun.AddOn_Func, beforeAfter));
                            CountryConfig.FunctionRow neighbourFunction = isBase ? baseFunctions.ElementAt(index) : derivedFunctions.ElementAt(index);
                            addOnParameter.Value = neighbourFunction.ID;
                            addOnParameter.Comment = "function '" + neighbourFunction.Name + "' in policy '" +
                                                        (isBase ? neighbourFunction.PolicyRow.Name : addOnImpPolicy.Name) + "'";

                            continue; //no further action (i.e. checking of function content) necessary
                        }
                        else //... switch is different
                        {
                            CountryConfig.ParameterRow addOnParameter = CountryConfigFacade.AddParameterRowAtTail(changeParamFunction,
                                DefPar.ChangeParam.Param_Id, DefinitionAdmin.GetParDefinition(DefFun.ChangeParam, DefPar.ChangeParam.Param_Id));
                            addOnParameter.Value = baseFunction.ID;
                            addOnParameter.Group = changeParamGroupIndex.ToString();
                            addOnParameter.Comment = "function '" + baseFunction.Name + "' in policy '" + baseFunction.PolicyRow.Name + "'";
                            addOnParameter = CountryConfigFacade.AddParameterRowAtTail(changeParamFunction,
                                DefPar.ChangeParam.Param_NewVal, DefinitionAdmin.GetParDefinition(DefFun.ChangeParam, DefPar.ChangeParam.Param_NewVal));
                            addOnParameter.Value = derivedFunction.Switch;
                            addOnParameter.Group = changeParamGroupIndex.ToString();
                            ++changeParamGroupIndex;

                            if (derivedFunction.Switch == DefPar.Value.NA)
                                continue; //not necessary to check content of function
                        }
                    }

                    if (baseFunction.Switch == DefPar.Value.OFF)
                        continue; //see note in corresponding code for policy

                    //check differences in parameters
                    List<CountryConfig.ParameterRow> derivedParameters = (from pR in countryConfig.Parameter where pR.FunctionID == derivedFunction.ID select pR).OrderBy(pR => long.Parse(pR.Order)).ToList();
                    List<CountryConfig.ParameterRow> baseParameters = (from pR in countryConfig.Parameter where pR.FunctionID == baseFunction.ID select pR).OrderBy(pR => long.Parse(pR.Order)).ToList();
                    CountryConfig.FunctionRow addParFunction = null;
                    for (int iParameter = 0; iParameter < derivedParameters.Count; ++iParameter)
                    {
                        CountryConfig.ParameterRow derivedParameter = derivedParameters.ElementAt(iParameter);
                        CountryConfig.ParameterRow baseParameter = baseParameters.ElementAt(iParameter);

                        //parameter value is equal: no action necessary
                        if (derivedParameter.Value.ToLower() == baseParameter.Value.ToLower()) //name and group cannot be different, assuming equal structure of the two systems
                            continue;

                        //parameter only relevant for the derived system: create an add-on-parameter
                        CountryConfig.ParameterRow addOnParameter;
                        if (baseParameter.Value == DefPar.Value.NA)
                        {
                            //add a respective AddOn_Par function to the add-ons definition policy
                            if (addParFunction == null)
                            {
                                addParFunction = CountryConfigFacade.AddFunctionRowAtTail(DefFun.AddOn_Par, addOnDefPolicy,
                                                                                          DefPar.Value.ON);
                                addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addParFunction,
                                    DefPar.AddOn_Par.Insert_Func, DefinitionAdmin.GetParDefinition(DefFun.AddOn_Par, DefPar.AddOn_Par.Insert_Func));
                                addOnParameter.Value = baseFunction.ID;
                                addOnParameter.Comment = "function '" + baseFunction.Name + "' in policy '" + baseFunction.PolicyRow.Name + "'";
                            }
                            addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addParFunction, DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.PLACEHOLDER),
                                DefinitionAdmin.GetParDefinition(DefFun.AddOn_Par, DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.PLACEHOLDER)));
                            addOnParameter.Name = derivedParameter.Name;
                            addOnParameter.Value = derivedParameter.Value;
                            continue;
                        }

                        //parameter value is different
                        addOnParameter = CountryConfigFacade.AddParameterRowAtTail(changeParamFunction, DefPar.ChangeParam.Param_Id,
                                DefinitionAdmin.GetParDefinition(DefFun.ChangeParam, DefPar.ChangeParam.Param_Id));
                        addOnParameter.Value = baseParameter.ID;
                        addOnParameter.Group = changeParamGroupIndex.ToString();
                        addOnParameter.Comment = "parameter '" + baseParameter.Name + "' in function '" + baseParameter.FunctionRow.Name + "' in policy '" + baseParameter.FunctionRow.PolicyRow.Name + "'";
                        addOnParameter = CountryConfigFacade.AddParameterRowAtTail(changeParamFunction, DefPar.ChangeParam.Param_NewVal,
                            DefinitionAdmin.GetParDefinition(DefFun.ChangeParam, DefPar.ChangeParam.Param_NewVal));
                        addOnParameter.Value = derivedParameter.Value;
                        addOnParameter.Group = changeParamGroupIndex.ToString();
                        ++changeParamGroupIndex;
                    }
                }
            }

            //replace IDs by symbolic IDs, if the option is checked, otherwise redirect IDs which point to the derived system to the base system (e.g. in loop functions)
            AdaptIDs(countryConfigFacade, addOnCountryConfigFacade, baseSystem.ID, derivedSystem.ID, exportAddOnForm.chkUseSymbolicIDs.Checked, exportAddOnForm.chkUseCC.Checked);

            //replace country shortname by =cc=, if option is checked
            if (exportAddOnForm.chkUseCC.Checked)
                ReplaceCountryShortName(addOnCountryConfigFacade, (from pR in countryConfigFacade.GetCountryConfig().Policy select pR.Name.ToLower()).ToList(), countryConfigFacade.GetCountryShortName());

            //save changes
            addOn.WriteXML();
        }

        static void ReplaceCountryShortName(CountryConfigFacade addOnCountryConfigFacade, List<string> allPolicyNames, string countryShortName)
        {
            //replace country symbol in taxunit parameters, e.g. individual_de -> individual_=cc=
            //and parameters which use policy names, e.g. std_output_de -> std_output_=cc=
            //other parameters are not likely to use a country symbol and one risks wrong replacements, like tax_deduction -> tax_=cc=duction
            //moreover do not replace country symbol in policies, which are copied to the add-on, i.e. are in fact add-on specific (it's not very reasonable to use the country symbol for add-on specific policies, but still quite likely)
            List<string> addOnPolicyNames = (from pR in addOnCountryConfigFacade.GetCountryConfig().Policy select pR.Name.ToLower()).ToList();
            foreach (CountryConfig.ParameterRow parameterRow in (from pR in addOnCountryConfigFacade.GetCountryConfig().Parameter select pR))
            {
                if (DefinitionAdmin.GetParDefinition(parameterRow.FunctionRow.Name, parameterRow.Name).valueType == DefPar.PAR_TYPE.TU ||
                    (allPolicyNames.Contains(parameterRow.Value.ToLower()) && !addOnPolicyNames.Contains(parameterRow.Value.ToLower())))
                    if (parameterRow.Value.ToLower().EndsWith(countryShortName.ToLower()))
                        parameterRow.Value = parameterRow.Value.Substring(0, parameterRow.Value.Length - countryShortName.Length) + DefPar.Value.PLACEHOLDER_CC;
            }
        }

        static void AdaptIDs(CountryConfigFacade countryConfigFacade, CountryConfigFacade addOnCountryConfigFacade,
                                string baseSystemID, string derivedSystemID, bool replaceBySymbolicIDs, bool replaceCountryShortName)
        {
            int guidLength = Guid.NewGuid().ToString().Length;
            foreach (CountryConfig.ParameterRow parameterRow in (from pR in addOnCountryConfigFacade.GetCountryConfig().Parameter select pR))
            {
                if (parameterRow.Value.Length != guidLength) //try to identify GUIDs by their length
                    continue;
                if (CountryConfigFacade.GetRowByID(addOnCountryConfigFacade.GetCountryConfig(), parameterRow.Value) != null)
                    continue; //the parameter refers to an id within the add-on, thus do not change

                System.Data.DataRow row = CountryConfigFacade.GetRowByID(countryConfigFacade.GetCountryConfig(), parameterRow.Value);
                if (row == null)
                    continue; //keep unknown identifier to be identified by the executable

                if (replaceBySymbolicIDs)
                {
                    string replaceCountryShortNameBy = replaceCountryShortName ? countryConfigFacade.GetCountryShortName() : string.Empty;
                    parameterRow.Value = ImportExportHelper.GetSymbolicID(row, replaceCountryShortNameBy); //replace ID by symbolic ID, if the option is checked
                }
                else if (CountryConfigFacade.GetSystemRow(row).ID == derivedSystemID) //otherwise redirect ID pointing to the derived system to the base system (e.g. in loop functions)
                    parameterRow.Value = CountryConfigFacade.GetRowID(CountryConfigFacade.GetTwinRow(row, baseSystemID));
            }
        }

        static bool SearchForAddOnNeighbour(List<string> baseSwitches, List<string> derivedSwitches, ref int startAndResultIndex, out bool insertAfter, out bool isBase)
        {
            insertAfter = true;
            isBase = true;
            //search for an applicable predecessor
            for (int index = startAndResultIndex - 1; index >= 0; --index)
            {
                if (baseSwitches.ElementAt(index) == DefPar.Value.NA &&
                    derivedSwitches.ElementAt(index) != DefPar.Value.NA && derivedSwitches.ElementAt(index) != DefPar.Value.OFF)
                {//the predecessor is also an add-on function (i.e. relevant only for the derived system (not n/a nor off) and irrelevant for the base system (n/a)) ...
                    isBase = false;
                    startAndResultIndex = index;
                    return true;
                }
                if (baseSwitches.ElementAt(index) != DefPar.Value.NA && baseSwitches.ElementAt(index) != DefPar.Value.OFF)
                {//... else, the predecessor is a base function (i.e. neither off nor n/a in the derived system)
                    startAndResultIndex = index;
                    return true;
                }
            }
            //if no applicable predecessor found (no element with switch set to on or toggle), search for an applicable successor
            insertAfter = false;
            for (int index = startAndResultIndex + 1; index < baseSwitches.Count; ++index)
            {//only search in the base system as any successor in the derived system is not yet linked
                if (baseSwitches.ElementAt(index) != DefPar.Value.NA && baseSwitches.ElementAt(index) != DefPar.Value.OFF)
                {
                    startAndResultIndex = index;
                    return true;
                }
            }

            startAndResultIndex = -1; //only happens, if there is no element in the base which is switched-on or toggle (this is unlikely (in practise impossible) for policies and does not make much sense for functions)
            return false;
        }

        static void CreateAndPrepareAddOn(ExportAddOnForm exportAddOnForm, CountryConfig.SystemRow baseSystem,
                                          out Country addOn, out CountryConfig.PolicyRow addOnDefPolicy,
                                          out CountryConfig.PolicyRow addOnImpPolicy, out CountryConfig.FunctionRow changeParamFunction)
        {
            //create an empty add-on with an empty system and get the ConfigFacades for further action
            addOn = CountryAdministrator.AddCountryOrAddOn(exportAddOnForm.txtShortName.Text, exportAddOnForm.txtLongName.Text, exportAddOnForm.txtSymbol.Text, true);
            CountryConfigFacade addOnCountryConfigFacade = addOn.GetCountryConfigFacade();

            addOnDefPolicy = (from p in addOnCountryConfigFacade.GetCountryConfig().Policy
                              where p.Name.ToLower().StartsWith(ExeXml.AddOn.POL_AO_CONTROL.ToLower())
                              select p).FirstOrDefault(); // this policy was created by AddCountryOrAddOn, thus must exist
            CountryConfig.ParameterRow addOnApplicParameter = (from p in addOnCountryConfigFacade.GetCountryConfig().Parameter
                              where p.Name.ToLower() == DefPar.AddOn_Applic.Sys.ToLower() && p.FunctionRow.Name.ToLower() == DefFun.AddOn_Applic.ToLower()
                              select p).FirstOrDefault(); // this parameter was created by AddCountryOrAddOn, thus must exist

            addOnApplicParameter.Value = baseSystem.Name; //... which defines that the base system is applicable with the add-on

            //create the policy for implementations: to be filled with (single) functions which are not defined in the bases system
            addOnImpPolicy = CountryConfigFacade.AddPolicyRow("Implementation_" + exportAddOnForm.txtShortName.Text,
                                                        string.Empty, addOnDefPolicy, false, DefPar.Value.ON);

            //create a special function for changing swiches of policies/functions and values of parameters which differ in the two systems 
            changeParamFunction = CountryConfigFacade.AddFunctionRowAtTail(DefFun.ChangeParam,
                                                        addOnImpPolicy, DefPar.Value.ON); //store it in the implementation policy
            CountryConfig.FunctionRow addFuncFunction = CountryConfigFacade.AddFunctionRowAtTail(DefFun.AddOn_Func, addOnDefPolicy,
                                                        DefPar.Value.ON); //create a respective AddOn_Func in the definition policy
            CountryConfig.ParameterRow addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addFuncFunction,
                DefPar.AddOn_Func.Id_Func, DefinitionAdmin.GetParDefinition(DefFun.AddOn_Func, DefPar.AddOn_Func.Id_Func));
            addOnParameter.Value = changeParamFunction.ID;
            addOnParameter.Comment = "function '" + changeParamFunction.Name + "' in policy '" + changeParamFunction.PolicyRow.Name + "'";
            addOnParameter = CountryConfigFacade.AddParameterRowAtTail(addFuncFunction, DefPar.AddOn_Func.Insert_Before_Func,
                DefinitionAdmin.GetParDefinition(DefFun.AddOn_Func, DefPar.AddOn_Func.Insert_Before_Func));
            //search for in the base system for a function for linking the ChangeParam function to, as it is not important which function, take the first not switched off
            CountryConfig.FunctionRow insertBeforeFunction = null;
            foreach (CountryConfig.PolicyRow policyRow in baseSystem.GetPolicyRows())
            {
                if (policyRow.Switch == DefPar.Value.ON)
                {
                    foreach (CountryConfig.FunctionRow functionRow in policyRow.GetFunctionRows())
                        if (functionRow.Switch == DefPar.Value.ON)
                        {
                            insertBeforeFunction = functionRow;
                            break;
                        }
                    if (insertBeforeFunction != null)
                        break;
                }
            }
            addOnParameter.Value = insertBeforeFunction.ID;
            addOnParameter.Comment = "function '" + insertBeforeFunction.Name + "' in policy '" + insertBeforeFunction.PolicyRow.Name + "'";
        }

        static internal void ExportSystems(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            ExportSystemsForm exportSystemsForm = new ExportSystemsForm((from system in countryConfigFacade.GetSystemRows() select system.Name).ToList());
            if (exportSystemsForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            string backUpFolder = RestoreManager.StoreCountry(mainForm);
            if (backUpFolder == string.Empty)
                return;

            try
            {
                //first simply save the country's xml-files at the indicated export path (to adapt both, country's xml-files and the exported xml-files, later)
                string exportPath = exportSystemsForm._selectedPath + mainForm.GetCountryShortName();
                if (Directory.Exists(exportPath))
                {
                    if (Tools.UserInfoHandler.GetInfo("Folder '" + exportPath + "' already exists. Should its content be overwritten?", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                        return;
                }
                else
                    Directory.CreateDirectory(exportPath);

                mainForm.Cursor = Cursors.WaitCursor;
                CountryAdministrator.WriteXML(mainForm.GetCountryShortName(), exportSystemsForm._selectedPath + mainForm.GetCountryShortName());

                //remove not exported systems from the exported xml-files and delete everything that got n/a in all systems
                Country exportCountry = new Country();
                exportCountry._shortName = mainForm.GetCountryShortName();
                exportCountry._isAddOn = CountryAdministrator.IsAddOn(mainForm.GetCountryShortName());

                CountryConfigFacade exportCountryConfigFacade = exportCountry.GetCountryConfigFacade(true, exportPath); //read the exported xml-files
                DataConfigFacade exportDataConfigFacade = exportCountry.GetDataConfigFacade(true, exportPath);

                List<string> notSelectedSystems = new List<string>();
                foreach (CountryConfig.SystemRow systemRow in countryConfigFacade.GetSystemRows()) //reverse system selection, as the not selected must be removed from the exported files
                {
                    if (!exportSystemsForm._selectedSystems.Contains(systemRow.Name))
                        notSelectedSystems.Add(systemRow.Name);
                }

                RemoveSystems(exportCountryConfigFacade, notSelectedSystems);
                RemoveDataConfigurations(exportDataConfigFacade, notSelectedSystems);
                exportCountry.WriteXML(exportPath); //save the changes

                //remove exported systems from the country's xml-files and delete everything that got n/a in all systems
                if (exportSystemsForm._deleteExportedSystems)
                {
                    RemoveSystems(countryConfigFacade, exportSystemsForm._selectedSystems);
                    RemoveDataConfigurations(dataConfigFacade, exportSystemsForm._selectedSystems);

                    // remove systems from global exchange-rates file
                    // note that the exchange-rate-file is not back-uped, thus this change is 'permanent'
                    ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
                    if (excf != null) excf.RemoveSystems(mainForm.GetCountryShortName(), exportSystemsForm._selectedSystems);

                    RestoreManager.SaveDirectXMLAction(mainForm);
                    mainForm.UpdateTree();
                }

                RestoreManager.ReportSuccessAndInfo("Exporting systems", backUpFolder);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Failed to export systems.", false);
                RestoreManager.RestoreCountry(mainForm, backUpFolder);
            }
            finally
            {
                mainForm.Cursor = Cursors.Default;
            }
        }

        static internal void CleanUpCountryFromNA(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            if (MessageBox.Show("You are about to remove all policies/functions/parameters that are set as 'n/a' in all systems.\n\n This action cannot be undone! Are you sure you want to continue?", "Clean Up Systems", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            string backUpFolder = RestoreManager.StoreCountry(mainForm);
            if (backUpFolder == string.Empty)
                return;
            try
            {
                RemoveSystems(countryConfigFacade, new List<string>());
                RemoveDataConfigurations(dataConfigFacade, new List<string>());
                RestoreManager.SaveDirectXMLAction(mainForm);
                mainForm.UpdateTree();
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception, "Failed to clean-up systems.", false);
                RestoreManager.RestoreCountry(mainForm, backUpFolder);
            }
            RestoreManager.ReportSuccessAndInfo("System Clean-up", backUpFolder);
        }

        static void RemoveDataConfigurations(DataConfigFacade dataConfigFacade, List<string> systemNames)
        {
            if (dataConfigFacade == null)
                return; //add-ons do not have a data-configuration file
            List<DataConfig.DBSystemConfigRow> configsToDelete = new List<DataConfig.DBSystemConfigRow>();
            foreach (string systemName in systemNames)
                configsToDelete.AddRange(dataConfigFacade.GetDBSystemConfigRowsBySystem(systemName));
            foreach (DataConfig.DBSystemConfigRow configToDelete in configsToDelete)
                configToDelete.Delete();
        }

        static void RemoveSystems(CountryConfigFacade countryConfigFacade, List<string> systemNames)
        {
            //first assess which systems are to be removed and which are to be kept
            List<CountryConfig.SystemRow> systemsToDelete = new List<CountryConfig.SystemRow>();
            List<CountryConfig.SystemRow> systemsToKeep = new List<CountryConfig.SystemRow>();
            foreach (CountryConfig.SystemRow systemRow in countryConfigFacade.GetSystemRows())
            {
                if (systemNames.Contains(systemRow.Name))
                    systemsToDelete.Add(systemRow);
                else
                    systemsToKeep.Add(systemRow);
            }

            //then get all policies, functions and parameters which are set to n/a in all of the systems, which are kept
            List<System.Data.DataRow> rowsToDelete = new List<System.Data.DataRow>();
            if (systemsToKeep.Count != 0)
            {
                foreach (CountryConfig.PolicyRow policyRow in systemsToKeep.First().GetPolicyRows())
                {
                    if (policyRow.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name)) //hidden policy for definition of uprating factors
                        continue; //the parameters of the definition-function contain their info in the parameter-name and have n/a-values for all systems, thus take care that they are not deleted here

                    if (policyRow.Switch == DefPar.Value.NA && //no need to check if policy is n/a in all systems if it isn't in the first: would be far too slow
                        countryConfigFacade.IsNAInAllSystems(policyRow, systemsToKeep))
                        rowsToDelete.AddRange(countryConfigFacade.GetTwinRows(policyRow, systemsToKeep));
                    else
                    {
                        foreach (CountryConfig.FunctionRow functionRow in policyRow.GetFunctionRows())
                        {
                            if (functionRow.Switch == DefPar.Value.NA && //see above concerning speed
                                countryConfigFacade.IsNAInAllSystems(functionRow, systemsToKeep))
                                rowsToDelete.AddRange(countryConfigFacade.GetTwinRows(functionRow, systemsToKeep));
                            else
                            {
                                foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                                {
                                    if (parameterRow.Value == DefPar.Value.NA && //see above concerning speed
                                        countryConfigFacade.IsNAInAllSystems(parameterRow, systemsToKeep))
                                        rowsToDelete.AddRange(countryConfigFacade.GetTwinRows(parameterRow, systemsToKeep));
                                }
                            }
                        }
                    }
                }
            }

            //finally delete n/a-components and systems
            foreach (System.Data.DataRow rowToDelete in rowsToDelete)
                rowToDelete.Delete();
            foreach (CountryConfig.SystemRow systemRow in systemsToDelete)
                CountryConfigFacade.DeleteSystemRow(systemRow);
        }

        static internal void ImportSystems(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade, DataConfigFacade dataConfigFacade)
        {
            ImportSystemsForm importSystemsForm = new ImportSystemsForm(CountryAdministrator.IsAddOn(mainForm.GetCountryShortName()));
            if (importSystemsForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            string backUpFolder = RestoreManager.StoreCountry(mainForm);
            if (backUpFolder == string.Empty)
                return;

            mainForm.Cursor = Cursors.WaitCursor;
            try
            {
                CountryConfigFacade importCountryConfigFacade = importSystemsForm._importCountry.GetCountryConfigFacade();
                List<CountryConfig.SystemRow> systemsToImport = new List<CountryConfig.SystemRow>();
                List<string> newSystemNames = new List<string>();
                List<CountryConfig.SystemRow> importedSystems = new List<CountryConfig.SystemRow>();

                //gather the systems to import and check if any system must be renamed as a system with the same name already exists
                foreach (string selectedSystem in importSystemsForm._selectedSystems)
                {
                    CountryConfig.SystemRow systemRowToImport = importCountryConfigFacade.GetSystemRowByName(selectedSystem);
                    systemsToImport.Add(systemRowToImport);
                    string newName = systemRowToImport.Name;
                    if (!GetNewNameForExistingSystem(countryConfigFacade, ref newName))
                        return; //user pressed cancel when aksed for new name
                    newSystemNames.Add(newName);
                }
                
                //import the system, either by trying an as-good-as-possible policy-function-parameter-match by content (names, values) ...
                if (!importSystemsForm._machtByID)
                {
                    //first add the one system and do all the adaptations that are necessary (i.e. symmetrising the import and existing systems) ...
                    Dictionary<string, string> elementsOldAndNewOrder = (systemsToImport.Count == 1) ? null : new Dictionary<string, string>();

                    CountryConfig.SystemRow firstImportedSystem = ImportSystem(countryConfigFacade, systemsToImport.ElementAt(0), newSystemNames.ElementAt(0),
                        ref elementsOldAndNewOrder, //this is used to import the rest of the systems (see below)
                        true); //if the system belongs to another country, the user is asked whether to change the ending of policies (e.g. xxx_sl -> xxx_uk) - ask just once

                    if (firstImportedSystem == null)
                        throw new System.ArgumentException(string.Empty); //to hand the error to the error handler
                    importedSystems.Add(firstImportedSystem); //store to add dataset-connection below
                    
                    //... then add the rest of the systems by using the order-information gained during importing the first system
                    for (int index = 1; index < systemsToImport.Count; ++index)
                    {
                        importedSystems.Add(ImportFurtherSystem(systemsToImport.ElementAt(index), newSystemNames.ElementAt(index),
                                                                firstImportedSystem, elementsOldAndNewOrder));
                    }
                }
                //... or by using unique identifiers (which is only possible if there is at least one system with the same ID in both countries)
                else
                {
                    ImportByIDAssistant importByIDAssistant = new ImportByIDAssistant(countryConfigFacade, importCountryConfigFacade);
                    if (!importByIDAssistant.ImportSystems(systemsToImport, newSystemNames))
                        throw new System.ArgumentException(string.Empty); //to hand the error to the error handler
                    importedSystems = importByIDAssistant.GetImportedSystems(); //store to add dataset-connection below

                    //overtake the system-settings (currencies, etc.)
                    //note: this was neglected in the original implementation and inserted later - trying to not jeopardise any functionality ...
                    CopySystemSettings(systemsToImport, importedSystems);

                    //take care about remaining differences (most importantly in policy/function/parameter-order and parameter-name and -group) to inform user where to find respective info
                    mainForm._importByIDDiscrepancies = importByIDAssistant.AssessRemainingDiscrepancies();
                }

                //import the datasets settings used by the system
                for (int index = 0; index < importedSystems.Count; ++index)
                {
                    if (!ImportDataConfiguration(importedSystems.ElementAt(index),
                                        systemsToImport.ElementAt(index).Name, //old system name
                                        dataConfigFacade, importSystemsForm._importCountry.GetDataConfigFacade()))
                        throw new System.ArgumentException(string.Empty); //to hand the error to the error handler
                }

                // a rather awful way to take extensions into account
                foreach (var impSys in importedSystems) AdaptExtensions(countryConfigFacade, impSys);

                // copy exchange-rates from the global-file in the import model to the global file in the current model
                // note that the exchange-rate-file is not back-uped, thus this change is 'permanent'
                CopyExchangeRates(systemsToImport, newSystemNames, mainForm.GetCountryShortName(), importSystemsForm.GetImportFolder());

                RestoreManager.SaveDirectXMLAction(mainForm);
                mainForm.UpdateTree();

                RestoreManager.ReportSuccessAndInfo("Importing systems", backUpFolder);

                if (importSystemsForm._machtByID && mainForm._importByIDDiscrepancies != null)
                    UserInfoHandler.ShowInfo("Please note that there are not-system-specific differences between the imported system(s) in the existing systems, " +
                        "which were not taken into account by the import and may change model output or cause errors for the imported systems." + Environment.NewLine + Environment.NewLine +
                        "Please find information concerning those differences by moving the mouse over the red and green info markers in the group column.");
            }
            catch (Exception exception)
            {
                if (exception.Message != string.Empty)
                    UserInfoHandler.ShowException(exception, "Failed to import systems.", false);
                RestoreManager.RestoreCountry(mainForm, backUpFolder);
            }
            finally
            {
                mainForm.Cursor = Cursors.Default;
            }
        }

        private static void CopyExchangeRates(List<CountryConfig.SystemRow> srcSysRows, List<string> destSysNames, string destCtryName, string srcCtryPath)
        {
            try
            {
                if (!srcSysRows.Any()) return;

                // open the source-models and the current models exchangerate-file-configs, for the former the path has to be derived from the import country's path
                ExchangeRatesConfigFacade destExrCf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false); if (destExrCf == null) return;
                ExchangeRatesConfigFacade srcExrCf = CountryAdministrator.GetExternalExchangeRatesConfigFacade(srcCtryPath); if (srcExrCf == null) return;

                bool changed = false;
                for (int i = 0; i < srcSysRows.Count; ++i) // loop over the imported systems (import-sys-list and equaly sized new-name-list)
                {
                    CountryConfig.SystemRow srcSysRow = srcSysRows[i]; string destSysName = destSysNames[i];
                    ExchangeRatesConfig.ExchangeRatesRow srcExr = (from er in srcExrCf.GetExchangeRates(srcSysRow.CountryRow.ShortName)
                                                                   where ExchangeRate.ValidForToList(er.ValidFor).Contains(srcSysRow.Name.ToLower())
                                                                   select er).FirstOrDefault();
                    if (srcExr == null) continue; changed = true;                   
                    if ((from e in destExrCf.GetExchangeRates(destCtryName)
                         where ExchangeRate.ValidForToList(e.ValidFor).Contains(destSysName.ToLower())
                         select e).Any()) continue; // the current model may already contain an exchange-rate for the new system (due to messing up)
                    destExrCf.GetExchangeRatesConfig().ExchangeRates.AddExchangeRatesRow(destCtryName, srcExr.June30, srcExr.YearAverage,
                        srcExr.FirstSemester, srcExr.SecondSemester, srcExr.Default, ExchangeRate.AddToValidFor(string.Empty, destSysName));
                    destExrCf.GetExchangeRatesConfig().AcceptChanges(); // this is necessary if the source-model is equal to the current model (systems are just imported from another country) and more than one system is imported
                }
                if (changed) destExrCf.WriteXML();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "A problems occured while copying exchange-rates. Make sure exchange-rates are correctly set in the global file", false);
            }
        }

        static void CopySystemSettings(List<CountryConfig.SystemRow> srcSystems, List<CountryConfig.SystemRow> destSystems)
        {
            for (int i = 0; i < srcSystems.Count; ++i)
            {
                if (destSystems.Count <= i) continue; // should not happen
                destSystems[i].CurrencyParam = srcSystems[i].CurrencyParam;
                destSystems[i].CurrencyOutput = srcSystems[i].CurrencyOutput;
                destSystems[i].ExchangeRateEuro = srcSystems[i].ExchangeRateEuro;
                destSystems[i].HeadDefInc = srcSystems[i].HeadDefInc;
                destSystems[i].Private = srcSystems[i].Private;
                destSystems[i].Year = srcSystems[i].Year;
            }
        }

        static CountryConfig.SystemRow ImportFurtherSystem(CountryConfig.SystemRow systemToImport, string systemToImportName,
                                                           CountryConfig.SystemRow firstImportedSystem, Dictionary<string, string> elementsOldAndNewOrder)
        {
            //first make a copy of the first imported systems, thus n/a's for not existing elements, order, etc. should be already ok
            CountryConfig.SystemRow importedSystem = CountryConfigFacade.CopySystemRow(systemToImportName, firstImportedSystem);

            //then correct the values to the values of the system that actually should be imported here
            foreach (CountryConfig.PolicyRow policyToImport in systemToImport.GetPolicyRows())
            {
                string order = elementsOldAndNewOrder[policyToImport.Order]; //get the order in the imported system (i.e. the copy of the first imported system)
                CountryConfig.PolicyRow importedPolicy = (from pol in importedSystem.GetPolicyRows()
                                                          where pol.Order == order
                                                          select pol).First(); //get the policy with the found order
                importedPolicy.Switch = policyToImport.Switch; //replace the value (i.e. switch)

                foreach (CountryConfig.FunctionRow functionToImport in policyToImport.GetFunctionRows())
                {
                    order = elementsOldAndNewOrder[policyToImport.Order + ";" + functionToImport.Order];
                    CountryConfig.FunctionRow importedFunction = (from func in importedPolicy.GetFunctionRows()
                                                                  where func.Order == order
                                                                  select func).First();
                    importedFunction.Switch = functionToImport.Switch;

                    foreach (CountryConfig.ParameterRow parameterToImport in functionToImport.GetParameterRows())
                    {
                        order = elementsOldAndNewOrder[policyToImport.Order + ";" + functionToImport.Order + ";" + parameterToImport.Order];
                        CountryConfig.ParameterRow importedParameter = (from par in importedFunction.GetParameterRows()
                                                                        where par.Order == order
                                                                        select par).First();
                        //CopySystemRow cares for correcting identifiers (e.g. for parameter's like function Loop's First_Func or ChangeParam's Param_ID)
                        //make sure that this is not set back here
                        if (!CountryConfigFacade.IsIdentifier(importedSystem, importedParameter.Value))
                            importedParameter.Value = parameterToImport.Value;
                    }
                }
            }

            return importedSystem;
        }

        static CountryConfig.SystemRow ImportSystem(CountryConfigFacade existingCountryConfigFacade,
                                                    CountryConfig.SystemRow originalImportSystemRow, string newSystemName,
                                                    ref Dictionary<string, string> elementsOldAndNewOrder,
                                                    bool changeCountryAcronym = false)
        {
            //do not use original CountryConfigFacade, as imported system will be changed, but generate a copy of the import-system in an for this purpose generated CountryConfigFacade
            CountryConfigFacade importCountryConfigFacade = new CountryConfigFacade(originalImportSystemRow.CountryRow.ShortName);
            //when copying the import-system keep all ids except the system id identically: otherwise the id-references (e.g. in ChangeParam, etc. would not work)
            CountryConfig.SystemRow importSystemRow = CountryConfigFacade.CopySystemRowToAnotherCountry(originalImportSystemRow, originalImportSystemRow.Name,
                                                                                    importCountryConfigFacade.GetCountryConfig(), true);
            importSystemRow.Name = newSystemName; //it may be necessary to rename the system if a system with this name already exists

            if (existingCountryConfigFacade.GetSystemRows().Count == 0 ||
                existingCountryConfigFacade.GetCountryConfig().Policy.Count == 0) //not foreseen to import system into an "empty country", i.e. no system or policies exist yet
            {
                Tools.UserInfoHandler.ShowError("System cannot be imported into an empty country. Please, consider using 'Save Country As' instead.");
                return null;
            }

            //initialise a reference-list for "easy" importing of all other systems but the first (they can use order-numbers of the first imported system instead of repeating the complex matching-process)
            Dictionary<string, string> elementsIDAndOldOrder = (elementsOldAndNewOrder == null) ? null //only necessary if more than one system is imported
                                                               : GenerateElementsIDAndOldOrderDictionary(importSystemRow);

            //"symmetrise" import system with existing systems, i.e. there must be the same order of policies, functions and parameters (i.e. the same spine), for this purpose
            //(a) find "matching policies", i.e. policies which exist in the import system as well as in the existing systems
            //(b) with these findings symmetrise policies, i.e. policies not present in the import system but in the existing systems must be added to the import system and switched off
            //(c) the same procedure must be applied vice versa for policies not present in the existing systems but in the import system
            //(d) the content of the matching polcies found in (a) must be symmetrised as well, i.e. do steps (a) to (d) for the functions contained in these polices,
            //    where (d) means doing steps (a) to (c) for parameters of matching functions

            //symmetrise policies
            List<CountryConfig.PolicyRow> existingPolicyRows = existingCountryConfigFacade.GetPolicyRowsOrderedAndDistinct(); //get all policies in spine-order
            List<CountryConfig.PolicyRow> importPolicyRows = importCountryConfigFacade.GetPolicyRowsOrderedAndDistinct();

            //if the systems is imported into another country, ask user whether the polcies' country acronym should be changed (e.g. Uprate_be -> Uprate_nl)
            string importCountryShortName = importCountryConfigFacade.GetCountryShortName().ToLower();
            string existingCountryShortName = existingCountryConfigFacade.GetCountryShortName().ToLower();
            if (importCountryShortName != existingCountryShortName)
            {
                if (changeCountryAcronym && Tools.UserInfoHandler.GetInfo("Should policies of import system(s) be renamed from 'xxx_" + importCountryShortName +
                                                    "' to 'xxx_" + existingCountryShortName + "'?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    foreach (CountryConfig.PolicyRow policyRow in importPolicyRows)
                        if (policyRow.Name.ToLower().EndsWith(importCountryShortName))
                        {
                            policyRow.Name = policyRow.Name.Substring(0, policyRow.Name.Length - importCountryShortName.Length);
                            policyRow.Name += existingCountryShortName;
                        }
                }
            }

            List<string> matchingPolicyIDs = FindMatchingPolicyRows(existingCountryConfigFacade, importCountryConfigFacade,
                                                existingPolicyRows, importPolicyRows); //step (a): retrieves a list "polX-id; polR-id, empty, polS-id, empty, ..." which are ids of existing policies which match with import policies (see function below for more details)
            List<string> existingPolicyIDs = (from policy in existingPolicyRows select policy.ID).ToList<string>(); //list of existing policy-ids in spine-order
            List<string> importPolicyIDs = (from policy in importPolicyRows select policy.ID).ToList<string>(); //list of import policy-ids in spine-order

            List<string> importPolicyNames = (from policy in importPolicyRows select policy.Name.ToLower()).ToList<string>(); //list of import policy-names, used for avoiding equally named polcies after symmetrising
            List<string> existingPolicyNames = (from policy in existingPolicyRows select policy.Name.ToLower()).ToList<string>(); //list of existing policy-names, used for the same reason

            SymmetriseRows(existingCountryConfigFacade, importCountryConfigFacade, existingPolicyIDs, importPolicyIDs, matchingPolicyIDs); //steps (b) and (c)

            for (int iPolicy = 0; iPolicy < matchingPolicyIDs.Count; ++iPolicy) //step (d)
            {
                string importPolicyID = importPolicyIDs.ElementAt(iPolicy);
                string matchingPolicyID = matchingPolicyIDs.ElementAt(iPolicy);

                if (matchingPolicyID == string.Empty)
                    continue; //no matching policy found, thus symmetrising task already accomplished via steps (b) and (c)

                //symmetrise functions (not separately commented, as similar to symmetrising policies)
                List<CountryConfig.FunctionRow> existingFunctionRows = existingCountryConfigFacade.GetFunctionRowsOfPolicyOrdered(matchingPolicyID);
                List<CountryConfig.FunctionRow> importFunctionRows = importCountryConfigFacade.GetFunctionRowsOfPolicyOrdered(importPolicyID);

                List<CountryConfig.FunctionRow> matchingFunctions = FindMatchingFunctionRows(existingFunctionRows, importFunctionRows);
                List<string> matchingFunctionIDs = new List<string>();
                foreach (CountryConfig.FunctionRow matchingFunction in matchingFunctions)
                    matchingFunctionIDs.Add(matchingFunction == null ? string.Empty : matchingFunction.ID);
                List<string> existingFunctionIDs = (from function in existingFunctionRows select function.ID).ToList<string>();
                List<string> importFunctionIDs = (from function in importFunctionRows select function.ID).ToList<string>();

                SymmetriseRows(existingCountryConfigFacade, importCountryConfigFacade, existingFunctionIDs, importFunctionIDs, matchingFunctionIDs,
                                importPolicyID, matchingPolicyID);

                for (int iFunction = 0; iFunction < matchingFunctions.Count; ++iFunction)
                {
                    CountryConfig.FunctionRow importFunction = importFunctionRows.ElementAt(iFunction);
                    CountryConfig.FunctionRow matchingFunction = matchingFunctions.ElementAt(iFunction);
                    if (matchingFunction == null)
                        continue;

                    //symmetrising parameters is easier than symmetrising policies and functions, as they are only ordered for visual purposes (see below)
                    MatchParameterRows(existingCountryConfigFacade, importCountryConfigFacade, matchingFunction, importFunction);
                }
            }

            //though import and existing systems are now symmetric, their order parameters probably still differ (see below)
            if (!AdjustOrder(existingCountryConfigFacade, importCountryConfigFacade))
                return null;

            //generate a (the final) reference-list for "easy" importing (as started above), important: do this before IDs change due to CopySystem below
            if (elementsOldAndNewOrder != null)
                GenerateElementsOldAndNewOrderDictionary(importSystemRow, elementsIDAndOldOrder, ref elementsOldAndNewOrder);

            //rename second occurrences of policy names, which result from different policy orders (from e.g. yem_sl to yem1_sl), as some routines expect unique policy names
            AvoidEquallyNamedPolicies(existingCountryConfigFacade, importCountryConfigFacade);

            //finally copy the now symmetric import system into the country
            return CountryConfigFacade.CopySystemRowToAnotherCountry(importSystemRow, importSystemRow.Name, existingCountryConfigFacade.GetCountryConfig());
        }

        static void AdaptExtensions(CountryConfigFacade ccf, CountryConfig.SystemRow systemRow)
        {
            try // this is rather awful, but I have no better id to symmetrise the extension-info for a new system
            {
                CountryConfig countryConfig = ccf.GetCountryConfig();
                Dictionary<string, List<CountryConfig.Extension_PolicyRow>> ePolOrders = new Dictionary<string, List<CountryConfig.Extension_PolicyRow>>();
                Dictionary<string, List<CountryConfig.Extension_FunctionRow>> eFunOrders = new Dictionary<string, List<CountryConfig.Extension_FunctionRow>>();
                Dictionary<string, List<CountryConfig.Extension_ParameterRow>> eParOrders = new Dictionary<string, List<CountryConfig.Extension_ParameterRow>>();
                foreach (var e in countryConfig.Extension_Policy)
                {
                    string order = (from p in countryConfig.Policy where e.PolicyID == p.ID select p.Order).First();
                    if (!ePolOrders.ContainsKey(order)) ePolOrders.Add(order, new List<CountryConfig.Extension_PolicyRow>() { e });
                    else if ((from c in ePolOrders[order] where c.ExtensionID == e.ExtensionID select c).Count() == 0) ePolOrders[order].Add(e);
                }
                foreach (var e in countryConfig.Extension_Function)
                {
                    string order = (from f in countryConfig.Function where e.FunctionID == f.ID select f.PolicyRow.Order + "|" + f.Order).First();
                    if (!eFunOrders.ContainsKey(order)) eFunOrders.Add(order, new List<CountryConfig.Extension_FunctionRow>() { e });
                    else if ((from c in eFunOrders[order] where c.ExtensionID == e.ExtensionID select c).Count() == 0) eFunOrders[order].Add(e);
                }

                foreach (var e in countryConfig.Extension_Parameter)
                {
                    string order = (from p in countryConfig.Parameter where e.ParameterID == p.ID select p.FunctionRow.PolicyRow.Order + "|" + p.FunctionRow.Order + "|" + p.Order).First();
                    if (!eParOrders.ContainsKey(order)) eParOrders.Add(order, new List<CountryConfig.Extension_ParameterRow>() { e });
                    else if ((from c in eParOrders[order] where c.ExtensionID == e.ExtensionID select c).Count() == 0) eParOrders[order].Add(e);
                }

                foreach (CountryConfig.PolicyRow polRow in systemRow.GetPolicyRows())
                {
                    // take care of old-style extension-handling, where switch is set to 'switch' in xml, but the respective extension is missing in the import country
                    if (polRow.Switch == DefPar.Value.SWITCH) polRow.Switch = DefPar.Value.TOGGLE;

                    if (ePolOrders.ContainsKey(polRow.Order))
                        foreach (var ep in ePolOrders[polRow.Order])
                            countryConfig.Extension_Policy.AddExtension_PolicyRow(ep.ExtensionID, polRow, ep.BaseOff);
                    foreach (CountryConfig.FunctionRow funRow in polRow.GetFunctionRows())
                    {
                        if (funRow.Switch == DefPar.Value.SWITCH) funRow.Switch = DefPar.Value.TOGGLE; // see comment above

                        string order = polRow.Order + "|" + funRow.Order;
                        if (eFunOrders.ContainsKey(order))
                            foreach (var ef in eFunOrders[order])
                                countryConfig.Extension_Function.AddExtension_FunctionRow(ef.ExtensionID, funRow, ef.BaseOff);
                        foreach (CountryConfig.ParameterRow parRow in funRow.GetParameterRows())
                        {
                            order = polRow.Order + "|" + funRow.Order + "|" + parRow.Order;
                            if (eParOrders.ContainsKey(order))
                                foreach (var ep in eParOrders[order])
                                    countryConfig.Extension_Parameter.AddExtension_ParameterRow(ep.ExtensionID, parRow, ep.BaseOff);
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "A problem occured in updating extensions. Please check and correct manually where necessary.", false);
            }
        }

        static Dictionary<string, string> GenerateElementsIDAndOldOrderDictionary(CountryConfig.SystemRow importSystemRow)
        {   //generate a reference-list to be used in the GenerateElementsOldAndNewOrderDictionary:
            //key = element ID, value = order in the form of pol-order;func-order;par-order (for parameters) to build a full identifier
            Dictionary<string, string> elementsIDAndOldOrder = new Dictionary<string, string>();
            foreach (CountryConfig.PolicyRow policyRow in importSystemRow.GetPolicyRows())
            {
                elementsIDAndOldOrder.Add(policyRow.ID, policyRow.Order);
                foreach (CountryConfig.FunctionRow functionRow in policyRow.GetFunctionRows())
                {
                    elementsIDAndOldOrder.Add(functionRow.ID, policyRow.Order + ";" + functionRow.Order);
                    foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                        elementsIDAndOldOrder.Add(parameterRow.ID, policyRow.Order + ";" + functionRow.Order + ";" + parameterRow.Order);
                }
            }

            return elementsIDAndOldOrder;
        }

        static void GenerateElementsOldAndNewOrderDictionary(CountryConfig.SystemRow importSystemRow, Dictionary<string, string> elementsIDAndOldOrder, ref Dictionary<string, string> elementsOldAndNewOrder)
        {   //generate a reference-list to be used in ImportSystems to import all other systems but the first (by using reference-list as created in GenerateElementsIDAndOldOrderDictionary)
            //key = element's absolute (and unique) order in extern system (i.e. pol-order;func-order;par-order): uniqueness necessary for identification in dictionary
            //value = element's relative order (e.g. just par-order) in just imported system: do not need more than relative order (see ImportFurtherSystem)
            //thus elements in import and imported system can be uniquly matched (by order) and values can be easily transferred (see ImportSystems)
            elementsOldAndNewOrder.Clear();
            foreach (CountryConfig.PolicyRow policyRow in importSystemRow.GetPolicyRows())
            {
                if (!elementsIDAndOldOrder.ContainsKey(policyRow.ID))
                    continue; //policy does not exist in extern system
                elementsOldAndNewOrder.Add(elementsIDAndOldOrder[policyRow.ID], policyRow.Order);
                foreach (CountryConfig.FunctionRow functionRow in policyRow.GetFunctionRows())
                {
                    if (!elementsIDAndOldOrder.ContainsKey(functionRow.ID))
                        continue;
                    elementsOldAndNewOrder.Add(elementsIDAndOldOrder[functionRow.ID], functionRow.Order);
                    foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                    {
                        if (!elementsIDAndOldOrder.ContainsKey(parameterRow.ID))
                            continue;
                        elementsOldAndNewOrder.Add(elementsIDAndOldOrder[parameterRow.ID], parameterRow.Order);
                    }
                }
            }
        }

        static bool GetNewNameForExistingSystem(CountryConfigFacade existingCountryConfigFacade, ref string newSystemName)
        {
            string dummy = string.Empty;
            return !SystemValidation.IsExistingSystemName(newSystemName, null, existingCountryConfigFacade) ||
                    SystemValidation.GetSystemNameAndYear(false, ref newSystemName, ref dummy, null, existingCountryConfigFacade);
        }

        static bool AdjustOrder(CountryConfigFacade existingCountryConfigFacade, CountryConfigFacade importCountryConfigFacade)
        {
            const string _error = "Importing failed due to technical reasons."; //use function also for checking (errors should only be of technical nature)

            //policies, functions and parameters are know in the same order, but the concrete order-string may differ
            //e.g. existing policies show orders 1, 2, 3, 5, 8, while import policies show orders 1, 2, 4, 6, 9
            List<CountryConfig.PolicyRow> existingPolicyRows = existingCountryConfigFacade.GetPolicyRowsOrderedAndDistinct(); //get all policies in spine-order
            List<CountryConfig.PolicyRow> importPolicyRows = importCountryConfigFacade.GetPolicyRowsOrderedAndDistinct();

            if (existingPolicyRows.Count != importPolicyRows.Count)
            {
                Tools.UserInfoHandler.ShowError(_error);
                return false;
            }

            for (int iPolicy = 0; iPolicy < importPolicyRows.Count; ++iPolicy)
            {
                if (importPolicyRows.ElementAt(iPolicy).Name.ToLower() != existingPolicyRows.ElementAt(iPolicy).Name.ToLower())
                {
                    Tools.UserInfoHandler.ShowError(_error);
                    return false;
                }
                TakeCareOfReferencePolicies(existingCountryConfigFacade, importCountryConfigFacade,
                                            existingPolicyRows.ElementAt(iPolicy), importPolicyRows.ElementAt(iPolicy)); //see function for explanation

                importPolicyRows.ElementAt(iPolicy).Order = existingPolicyRows.ElementAt(iPolicy).Order;

                List<CountryConfig.FunctionRow> existingFunctionRows = existingCountryConfigFacade.GetFunctionRowsOfPolicyOrdered(existingPolicyRows.ElementAt(iPolicy).ID);
                List<CountryConfig.FunctionRow> importFunctionRows = importCountryConfigFacade.GetFunctionRowsOfPolicyOrdered(importPolicyRows.ElementAt(iPolicy).ID);

                if (existingFunctionRows.Count != importFunctionRows.Count)
                {
                    Tools.UserInfoHandler.ShowError(_error);
                    return false;
                }

                for (int iFunction = 0; iFunction < importFunctionRows.Count; ++iFunction)
                {
                    if (importFunctionRows.ElementAt(iFunction).Name.ToLower() != existingFunctionRows.ElementAt(iFunction).Name.ToLower())
                    {
                        Tools.UserInfoHandler.ShowError(_error);
                        return false;
                    }

                    importFunctionRows.ElementAt(iFunction).Order = existingFunctionRows.ElementAt(iFunction).Order;

                    List<CountryConfig.ParameterRow> existingParameterRows = existingCountryConfigFacade.GetParameterRowsOfFunctionOrdered(existingFunctionRows.ElementAt(iFunction));
                    List<CountryConfig.ParameterRow> importParameterRows = importCountryConfigFacade.GetParameterRowsOfFunctionOrdered(importFunctionRows.ElementAt(iFunction));

                    if (existingParameterRows.Count != importParameterRows.Count)
                    {
                        Tools.UserInfoHandler.ShowError(_error);
                        return false;
                    }

                    for (int iParameter = 0; iParameter < importParameterRows.Count; ++iParameter)
                    {
                        if (importParameterRows.ElementAt(iParameter).Name.ToLower() != existingParameterRows.ElementAt(iParameter).Name.ToLower())
                        {
                            Tools.UserInfoHandler.ShowError(_error);
                            return false;
                        }

                        //no adjusting of order necessary, as already done in MatchParameterRows (i.e. just controlling for correct matching is done here)
                    }
                }
            }

            return true;
        }

        static void TakeCareOfReferencePolicies(CountryConfigFacade existingCountryConfigFacade, CountryConfigFacade importCountryConfigFacade,
                                                CountryConfig.PolicyRow existingPolicy, CountryConfig.PolicyRow importPolicy)
        {
            //care needs to be taken only if the import policy is a reference:
            //if the existing policy is a reference, the import system contains an "empty policy" which is switched to n/a, which is ok
            //however if the import policy is a reference (though the existing policies are "empty switched to n/a policies" as well before this function is called)
            //the existing policies need to refer to the corresponding policy, as otherwise the policy column shows no name (reference policies have no own name and the anyway wrong reference wasn't copied)
            if ((importPolicy.ReferencePolID == null || importPolicy.ReferencePolID == string.Empty) ||
                (existingPolicy.ReferencePolID != null && existingPolicy.ReferencePolID != string.Empty))
                return;

            foreach (CountryConfig.SystemRow systemRow in existingCountryConfigFacade.GetSystemRows())
            {
                CountryConfig.PolicyRow referencingPolicy = CountryConfigFacade.GetTwinRow(existingPolicy, systemRow.ID) as CountryConfig.PolicyRow;
                CountryConfig.PolicyRow referencePolicy = importCountryConfigFacade.GetPolicyRowByID(importPolicy.ReferencePolID);
                if (referencingPolicy != null && referencePolicy != null) //should not be null
                    referencingPolicy.ReferencePolID = existingCountryConfigFacade.GetPolicyRowByNameOrderAndSystemID(referencePolicy.Name, referencePolicy.Order, systemRow.ID).ID;
            }
        }

        static void AvoidEquallyNamedPolicies(CountryConfigFacade existingCountryConfigFacade, CountryConfigFacade importCountryConfigFacade)
        {
            //loop over all policies (unimportant which system, therefore take the import system) to check if there are equally named polices
            //(assuming there could be only twins (i.e. no triples or worse) unless there was a double naming already before the import)
            List<CountryConfig.PolicyRow> importPolicies = importCountryConfigFacade.GetSystemRows().First().GetPolicyRows().ToList();
            for (int i = 0; i < importPolicies.Count; ++i)
            {
                if (importPolicies.ElementAt(i).ReferencePolID != null && importPolicies.ElementAt(i).ReferencePolID != string.Empty)
                    continue; //just to be sure, name of a reference should actually be empty

                string policyName = importPolicies.ElementAt(i).Name.ToLower();

                for (int j = i + 1; j < importPolicies.Count; ++j)
                {
                    if (importPolicies.ElementAt(j).ReferencePolID != null && importPolicies.ElementAt(j).ReferencePolID != string.Empty)
                        continue; //just to be sure, name of a reference should actually be empty

                    if (policyName == importPolicies.ElementAt(j).Name.ToLower()) //equally named policy found
                    {
                        //find a name for the duplicate: insert a counter before the country short name (e.g. yem1_sl) or at the end (mtr_init1)
                        List<string> usedNames = (from pR in importPolicies select pR.Name.ToLower()).ToList();
                        int counterPosition = policyName.LastIndexOf('_');
                        string newName = string.Empty;
                        for (int k = 1; ; ++k)
                        {
                            newName = counterPosition < 0 ? policyName + k.ToString() : policyName.Substring(0, counterPosition) + k.ToString() + policyName.Substring(counterPosition);
                            if (!usedNames.Contains(newName.ToLower()))
                                break;
                        }

                        //decide which of the two policies is the duplicate: select the one that came into being by import, i.e. all switches in existing systems are n/a
                        CountryConfig.PolicyRow duplicatePolicy = importPolicies.ElementAt(i).Switch.ToLower() == DefPar.Value.NA ?
                            importPolicies.ElementAt(j) : importPolicies.ElementAt(i); //checking which of the two policies is set to n/a in the import system should be a sufficient to assess which came into being by import
                        foreach (CountryConfig.SystemRow systemRow in existingCountryConfigFacade.GetSystemRows())
                        {
                            CountryConfig.PolicyRow twinOfDuplicate = existingCountryConfigFacade.GetPolicyRowByNameOrderAndSystemID(duplicatePolicy.Name, duplicatePolicy.Order, systemRow.ID);
                            twinOfDuplicate.Name = newName; //rename the policy in the existing systems
                        }
                        duplicatePolicy.Name = newName; //rename the policy in the import system

                        break;
                    }
                }
            }
        }

        static void SymmetriseRows(CountryConfigFacade existingCountryConfigFacade, CountryConfigFacade importCountryConfigFacade,
                                    List<string> existingIDs, List<string> importIDs, List<string> matchingIDs,
                                    string importParentID = "", string matchingParentID = "")
        {
            //example for illustration of algorithm:
            //existingIDs =  A, B, C, D, E, F, G
            //importIDs =    A, C, X, D, E, Y, F, Z
            //matchingIDs =  A, C, -, D, E, -, F, -   //i.e. lists the matches with existing IDs (importIDs.Count=matchingIDs.Count)
            //
            //eIndex->A; mIndes->A
            //loop 1: A == A :  eIndex->B; mIndex->C
            //loop 2: B != C :  generate B in import system; eIndex->C      //import system is altered to     A, B, C, X, D, E, Y, F, Z
            //loop 3: C == C :  eIndex->D; mIndex->-
            //loop 4: D != - :  generate X in existing systems; iIndex->D   //existing sytems are altered to  A, B, C, X, D, E, F, G
            //loop 5: D == D :  eIndex->E; mIndex->E
            //etc.
            int eIndex = 0;
            for (int mIndex = 0; mIndex < matchingIDs.Count || eIndex < existingIDs.Count; ) //importIDs and matchingIDs are of same size
            {
                //only exists in import system -> must be generated in existing systems
                if (mIndex < matchingIDs.Count && matchingIDs.ElementAt(mIndex) == string.Empty)
                {
                    List<System.Data.DataRow> neighbourRows;
                    List<System.Data.DataRow> parentRows;
                    GetSystemSpecificReferences(existingCountryConfigFacade, existingIDs, matchingParentID, eIndex, out neighbourRows, out parentRows); //see below

                    for (int sIndex = 0; sIndex < neighbourRows.Count; ++sIndex) //loop over systems, as element must be copied into each system
                    {
                        CountryConfigFacade.CopyPolicyOrFunction(CountryConfigFacade.GetRowByID(importCountryConfigFacade.GetCountryConfig(), importIDs.ElementAt(mIndex)), //row to copy
                            neighbourRows.ElementAt(sIndex), //neighbour where to copy ...
                            eIndex < existingIDs.Count ? true : false, //... before or after: in general copy before next element to match, but if there isn't any left, copy after last element
                            parentRows.ElementAt(sIndex), //parent row is only needed, if the policy/function in the import system contains functions/parameters, while the policy/function in the existing systems is empty, or vice-versa (instead of a fitting in, the functions/parameters must be simply added then)
                            true); //set to n/a
                    }
                    ++mIndex;
                }

                //exists in both systems: in principle nothing to do, just care for equal spelling referring to upper- and lower-case 
                else if (mIndex < matchingIDs.Count && eIndex < existingIDs.Count && matchingIDs.ElementAt(mIndex) == existingIDs.ElementAt(eIndex))
                {
                    System.Data.DataRow existingRow = CountryConfigFacade.GetRowByID(existingCountryConfigFacade.GetCountryConfig(), existingIDs.ElementAt(eIndex));
                    System.Data.DataRow importRow = CountryConfigFacade.GetRowByID(importCountryConfigFacade.GetCountryConfig(), importIDs.ElementAt(mIndex));
                    if (CountryConfigFacade.GetRowName(existingRow) != CountryConfigFacade.GetRowName(importRow))
                        CountryConfigFacade.SetRowName(importRow, CountryConfigFacade.GetRowName(existingRow));
                    ++mIndex; ++eIndex;
                }

                //only exists in existing systems -> must be generated in import system
                else if (eIndex < existingIDs.Count) //just ensures that we are not beyond the end, the relevant condition is matchingIDs[mIndex] != existingIDs[eIndex] (coverde by the else)
                {
                    System.Data.DataRow neighbourRow = null;
                    System.Data.DataRow parentRow = CountryConfigFacade.GetRowByID(importCountryConfigFacade.GetCountryConfig(), importParentID);
                    bool insertBefore = true;
                    if (importIDs.Count > 0)
                    {
                        if (mIndex < importIDs.Count) //copy before next element to match ...
                            neighbourRow = CountryConfigFacade.GetRowByID(importCountryConfigFacade.GetCountryConfig(), importIDs.ElementAt(mIndex));
                        else //... but if there isn't any left, copy after last element
                        {
                            neighbourRow = CountryConfigFacade.GetLastSibling(CountryConfigFacade.GetRowByID(importCountryConfigFacade.GetCountryConfig(), importIDs.ElementAt(0)));
                            insertBefore = false;
                        }
                    }
                    CountryConfigFacade.CopyPolicyOrFunction( //no loop over systems, as there is only one system (the one to be copied in)
                        CountryConfigFacade.GetRowByID(existingCountryConfigFacade.GetCountryConfig(), existingIDs.ElementAt(eIndex)), //row to copy
                        neighbourRow, insertBefore, //see above
                        parentRow, //parent-row is only needed, if the policy/function in the import system contains functions/parameters, while the policy/function in the existing systems is empty, or vice-versa (instead of a fitting in, the functions/parameters must be simply added then)
                        true); //set to n/a
                    ++eIndex;
                }
            }
        }

        static void GetSystemSpecificReferences(CountryConfigFacade existingCountryConfigFacade, List<string> existingIDs, string parentID, int eIndex,
                                                out List<System.Data.DataRow> specificNeighbourRows, out List<System.Data.DataRow> specificParentRows)
        {   //for matching purposes an arbitrary element was used in the sense that it was taken from an arbitrary system (assuming symmetry)
            //for copying the corresponding elements in the respective systems are required
            System.Data.DataRow arbitraryNeighbourRow = null;
            System.Data.DataRow arbitraryParentRow = null;
            if (existingIDs.Count != 0)
            {
                if (eIndex < existingIDs.Count)
                    arbitraryNeighbourRow = CountryConfigFacade.GetRowByID(existingCountryConfigFacade.GetCountryConfig(), existingIDs.ElementAt(eIndex));
                else
                    arbitraryNeighbourRow = CountryConfigFacade.GetLastSibling(CountryConfigFacade.GetRowByID(existingCountryConfigFacade.GetCountryConfig(), existingIDs.ElementAt(0)));
            }
            else
                arbitraryParentRow = CountryConfigFacade.GetRowByID(existingCountryConfigFacade.GetCountryConfig(), parentID);

            specificNeighbourRows = new List<System.Data.DataRow>();
            specificParentRows = new List<System.Data.DataRow>();
            foreach (CountryConfig.SystemRow systemRow in existingCountryConfigFacade.GetSystemRows())
            {
                specificNeighbourRows.Add(arbitraryNeighbourRow == null ? null : CountryConfigFacade.GetTwinRow(arbitraryNeighbourRow, systemRow.ID));
                specificParentRows.Add(arbitraryParentRow == null ? null : CountryConfigFacade.GetTwinRow(arbitraryParentRow, systemRow.ID));
            }
        }

        static List<string> FindMatchingPolicyRows(CountryConfigFacade existingCountryConfigFacade, CountryConfigFacade importCountryConfigFacade,
                                                   List<CountryConfig.PolicyRow> existingPoliciesOrdered, List<CountryConfig.PolicyRow> importPoliciesOrdered)
        {
            //not an overwhelmingly sophisticated algorithm, could be improved if required
            List<string> matchingIDs = new List<string>();

            int eIndex = -1;
            for (int iIndex = 0; iIndex < importPoliciesOrdered.Count; ++iIndex)
            {
                CountryConfig.PolicyRow importPolicy = importPoliciesOrdered.ElementAt(iIndex);
                CountryConfig.PolicyRow importReferencePolicy = null; //also need to take account of reference policies
                if (importPolicy.ReferencePolID != null && importPolicy.ReferencePolID != string.Empty)
                    importReferencePolicy = importCountryConfigFacade.GetPolicyRowByID(importPolicy.ReferencePolID);

                string matchingID = string.Empty;
                int bkupIndex = eIndex;
                for (eIndex++; eIndex < existingPoliciesOrdered.Count; ++eIndex) //search for a policy with the same name after the last found match
                {                                                                //to ensure that order - in both spines - is not destroyed
                    CountryConfig.PolicyRow existingPolicy = existingPoliciesOrdered.ElementAt(eIndex);
                    CountryConfig.PolicyRow existingReferencePolicy = null;
                    if (existingPolicy.ReferencePolID != null && existingPolicy.ReferencePolID != string.Empty)
                        existingReferencePolicy = existingCountryConfigFacade.GetPolicyRowByID(existingPolicy.ReferencePolID);
                    if (importReferencePolicy == null && existingReferencePolicy == null) //the usual case, i.e. no reference policies in either system
                    {
                        if (importPolicy.Name.ToLower() == existingPolicy.Name.ToLower())
                        {
                            matchingID = existingPolicy.ID;
                            break;
                        }
                    }
                    else //if referenece policy exists in both systems there is still no problem (if not, special care needs to be taken (see above))
                    {
                        if (importReferencePolicy != null && existingReferencePolicy != null &&
                            importReferencePolicy.Name.ToLower() == existingReferencePolicy.Name.ToLower())
                        {
                            matchingID = existingPolicy.ID;
                            break;
                        }
                    }
                }
                if (matchingID == string.Empty)
                    eIndex = bkupIndex;
                matchingIDs.Add(matchingID);
            }

            return matchingIDs;
        }

        static List<CountryConfig.FunctionRow> FindMatchingFunctionRows(List<CountryConfig.FunctionRow> existingFunctionsOrdered, List<CountryConfig.FunctionRow> importFunctionsOrdered)
        {
            //it is not easy to reintegrate the functions of an "outsourced" system as there is no reliable match-criterion (like the name of policies)
            //therefore check "all" potential constellations and select the "best match", which is the one where most functions of the import system can be assigned to functions of the existing systems
            //the "all" is in quotation marks, as for large policies exploring all constellations is too much effort (see comments in function SymmetriseLists)

            //initialiset the arrays which will contain the indices of the matching functions (to be filled by SymmetriseLists)
            List<int> iPotentialMatch = new List<int>();
            List<int> iBestMatch = new List<int>();
            foreach (CountryConfig.FunctionRow function in importFunctionsOrdered)
            {
                iPotentialMatch.Add(-1);
                iBestMatch.Add(-1);
            }
            int counter = 0; //see below for need of counter

            //in general just match via equal name (i.e. ArithOp <-> ArithOp, BenCalc <-> BenCalc, etc.)
            //except for policies ILDef and TUDef, where functions DefIL and DefTU provide the parameter name for identfication
            List<string> existingFunctionsNames = null;
            List<string> importFunctionsNames = null;
            if (existingFunctionsOrdered.Count > 0 &&
               (existingFunctionsOrdered.ElementAt(0).PolicyRow.Name.ToLower().StartsWith(DefPol.SPECIAL_POL_ILDEF.ToLower()) ||
               existingFunctionsOrdered.ElementAt(0).PolicyRow.Name.ToLower().StartsWith(DefPol.SPECIAL_POL_ILSDEF.ToLower()) ||
               existingFunctionsOrdered.ElementAt(0).PolicyRow.Name.ToLower().StartsWith(DefPol.SPECIAL_POL_TUDEF.ToLower())))
            {
                existingFunctionsNames = GetILTUNames(existingFunctionsOrdered);
                importFunctionsNames = GetILTUNames(importFunctionsOrdered);
            }
            else
            {
                existingFunctionsNames = (from function in existingFunctionsOrdered select function.Name.ToLower()).ToList<string>();
                importFunctionsNames = (from function in importFunctionsOrdered select function.Name.ToLower()).ToList<string>();
            }
            SymmetriseLists(existingFunctionsNames, importFunctionsNames, 0, 0, ref iPotentialMatch, ref iBestMatch, ref counter);

            //SymmetriseLists delivers indices in existingFunctionsOrdered-list: they need to be translated to the IDs of the concerned functions
            List<CountryConfig.FunctionRow> bestMatch = new List<CountryConfig.FunctionRow>();
            for (int index = 0; index < iBestMatch.Count; ++index)
            {
                if (iBestMatch.ElementAt(index) == -1)
                    bestMatch.Add(null);
                else
                    bestMatch.Add(existingFunctionsOrdered.ElementAt(iBestMatch.ElementAt(index)));
            }
            return bestMatch;
        }

        static List<string> GetILTUNames(List<CountryConfig.FunctionRow> functionRows)
        {
            List<string> iltuNames = new List<string>();
            foreach (CountryConfig.FunctionRow functionRow in functionRows)
            {
                string iltuName = functionRow.Name.ToLower(); //use function's name (DefIL, DefTU or can even be another function) as default if parameter 'name' can't be found
                foreach (CountryConfig.ParameterRow parameterRow in functionRow.GetParameterRows())
                {
                    if (parameterRow.Name.ToLower() == DefPar.DefIl.Name.ToLower())
                    {
                        iltuName = ";" + parameterRow.Value.ToLower() + ";";
                        break;
                    }
                }
                iltuNames.Add(iltuName);
            }
            return iltuNames;
        }

        static void MatchParameterRows(CountryConfigFacade existingCountryConfigFacade, CountryConfigFacade importCountryConfigFacade,
                                       CountryConfig.FunctionRow existingFunctionRow, CountryConfig.FunctionRow importFunctionRow)
        {
            List<CountryConfig.ParameterRow> existingParameterRows = existingCountryConfigFacade.GetParameterRowsOfFunctionOrdered(existingFunctionRow);
            List<CountryConfig.ParameterRow> importParameterRows = importCountryConfigFacade.GetParameterRowsOfFunctionOrdered(importFunctionRow);
            List<CountryConfig.ParameterRow> matchingParameterRows = new List<CountryConfig.ParameterRow>();

            //first try to find an exact match for each parameter of the import system in the existing systems, i.e. name, group and value are equal
            List<string> matchingParameterIDs = new List<string>();

            foreach (CountryConfig.ParameterRow importParameter in importParameterRows)
            {
                List<CountryConfig.ParameterRow> matches = (from existingParameter in existingParameterRows
                                                            where existingParameter.Name.ToLower() == importParameter.Name.ToLower() &&
                                                                  existingParameter.Group.ToLower() == importParameter.Group.ToLower() &&
                                                                  existingParameter.Value.ToLower() == importParameter.Value.ToLower() && //i.e. exact match
                                                                  !matchingParameterIDs.Contains(existingParameter.ID) //parameter of existing system cannot be matched to more than on parameter of import system
                                                            select existingParameter).ToList();
                if (matches.Count() > 0)
                {
                    matchingParameterRows.Add(matches.First());
                    matchingParameterIDs.Add(matches.First().ID); //to prevent double match (see above)
                }
                else
                    matchingParameterRows.Add(null);
            }

            //then relax the condition and try to find an suitable match for each parameter where no exact match was found, i.e. name and group are equal
            int index;
            for (index = 0; index < importParameterRows.Count(); ++index)
            {
                if (matchingParameterRows.ElementAt(index) != null)
                    continue; //already found an exact match
                CountryConfig.ParameterRow importParameter = importParameterRows.ElementAt(index);
                List<CountryConfig.ParameterRow> matches = (from existingParameter in existingParameterRows //relaxed condition: no checking for value
                                                            where existingParameter.Name.ToLower() == importParameter.Name.ToLower() &&
                                                                  existingParameter.Group.ToLower() == importParameter.Group.ToLower() &&
                                                                  !matchingParameterIDs.Contains(existingParameter.ID)
                                                            select existingParameter).ToList();
                if (matches.Count() > 0)
                {
                    matchingParameterRows[index] = matches.First();
                    matchingParameterIDs.Add(matches.First().ID);
                }
            }

            //in existing systems generate all parameters which exist in the import system, but not in the existing systems (i.e. no match was found)
            for (index = 0; index < importParameterRows.Count(); ++index)
            {
                if (matchingParameterRows.ElementAt(index) != null)
                    continue;
                CountryConfig.ParameterRow importParameter = importParameterRows.ElementAt(index);
                foreach (CountryConfig.SystemRow systemRow in existingCountryConfigFacade.GetSystemRows()) //parameter must be generated in all existing systems
                {
                    CountryConfig.FunctionRow arbitraryFunctionRow = existingFunctionRow; //this is just 'one of' the parameter's parent functions, i.e. with arbitrary system
                    CountryConfig.FunctionRow specificFunctionRow = existingCountryConfigFacade.GetFunctionRowByPolicyNameAndOrder(systemRow.ID,
                                                                    arbitraryFunctionRow.PolicyRow.Name, arbitraryFunctionRow.Order); //this is the real parent function, i.e. specific system
                    string order = CountryConfigFacade.GetMaxOrderPlus1AsString(specificFunctionRow); //assess next available order, as copy-procedure will use the order of original parameter
                    CountryConfig.ParameterRow parameterRow = CountryConfigFacade.CopyParameterRow(importParameter, specificFunctionRow, true); //setToNA=true
                    parameterRow.Order = order; //i.e. add at tail
                    matchingParameterRows[index] = parameterRow; //add to the matching list, as needed later to adjust the order of parameters in the import system
                }
            }

            //for symmetry also all parameters, which exist in the existing system only, must be generated in the import system
            foreach (CountryConfig.ParameterRow existingParameter in existingParameterRows)
            {
                if (matchingParameterIDs.Contains(existingParameter.ID))
                    continue;
                //note that order is taken from existingParameter, which should be ok
                CountryConfigFacade.CopyParameterRow(existingParameter, importFunctionRow, true); //setToNA=true
            }

            //finally take care that the order parameters of the import and the existing systems correspond
            for (index = 0; index < importParameterRows.Count; ++index)
            {
                importParameterRows.ElementAt(index).Order = matchingParameterRows.ElementAt(index).Order;
                importParameterRows.ElementAt(index).Name = matchingParameterRows.ElementAt(index).Name; //just to ensure that spelling with regard to upper-lower-case is equal
            }
        }

        static void SymmetriseLists(List<string> list1, List<string> list2, int indexList1, int indexList2, ref List<int> potentialMatch, ref List<int> bestMatch, ref int counter)
        {   //more easy to understand if comments are read in order A - B - C
            //C) stop searching for better matches once all list elements are assigned, though there could be an equally good match, but without a criterion to prefer the one or the other
            if (counter > 100000)
                return; //not very good solution to avoid seemingly infinite loops, as the number of possible constellations gets huge with large policies (however the score usually doesn't get better after testing so many constellations)
            double bestMatchScore = bestMatch.Count(match => match != -1);
            if (bestMatchScore == list1.Count || bestMatchScore == list2.Count)
                return;

            //B) potential match of the two lists found (i.e. no more possibilities to match further elements): check whether this potential is better than the best match so far
            if (list1.Count <= indexList1 || list2.Count <= indexList2)
            {
                int potentialMatchScore = potentialMatch.Count(match => match != -1);
                if (bestMatchScore < potentialMatchScore)
                {
                    bestMatch.Clear();
                    bestMatch.AddRange(potentialMatch);
                }
                ++counter;
                return;
            }

            //A) generate (recursively) potential matchings of the two lists:
            //first find out which and how many elements of list1 would match with the "current" element of list2, where current is the element of list2 where the list-index (indexList1) points to
            List<int> potentialEquivalentsOfElement = new List<int>();
            for (int index = indexList1;
                     index < indexList1 + 3 //don't search further than 3 elements down as such a match is rather unlikely and extends the number of constellations (also see break at counter > 100000 above)
                     && index < list1.Count; ++index)
            {
                if (list1[index] == list2[indexList2])
                    potentialEquivalentsOfElement.Add(index);
            }
            //then loop over these possible machting elements: in each loop recursively call the matching-function to find out all matchings of the two lists, where these two elements are equivalents
            for (int index = 0; index < potentialEquivalentsOfElement.Count; ++index)
            {
                potentialMatch[indexList2] = potentialEquivalentsOfElement[index];
                SymmetriseLists(list1, list2, potentialEquivalentsOfElement[index] + 1, indexList2 + 1, ref potentialMatch, ref bestMatch, ref counter);
            }
            potentialMatch[indexList2] = -1; //the no-match case
            SymmetriseLists(list1, list2, indexList1, indexList2 + 1, ref potentialMatch, ref bestMatch, ref counter);
        }

        static bool ImportDataConfiguration(CountryConfig.SystemRow newSystemRow, string systemOldName,
                                            DataConfigFacade existingDataConfigFacade, DataConfigFacade importDataConfigFacade)
        {
            if (importDataConfigFacade == null)
                return true; //no database-configuration available (e.g. for add-ons)

            //loop over all database-configurations that are available for the import system in the import database-configuration-file (i.e. the cc_DataConfig.xml-file)
            foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in importDataConfigFacade.GetDBSystemConfigRowsBySystem(systemOldName))
            {
                //first check whether the database is also present in the existing database-configuration-file
                DataConfig.DataBaseRow dataBaseRow = existingDataConfigFacade.GetDataBaseRowByDataFile(dbSystemConfigRow.DataBaseRow.FilePath, dbSystemConfigRow.DataBaseRow.Name);
                if (dataBaseRow == null) //if not generate it by copying the respective settings from the import-file
                    dataBaseRow = DataConfigFacade.CopyDataBaseRowFromAnotherCountry(existingDataConfigFacade.GetDataConfig(), dbSystemConfigRow.DataBaseRow);
                if (dataBaseRow == null)
                    continue; //should not happen

                //then assign the system to the database
                DataConfigFacade.OvertakeDBSystemConfigRowFromAnotherCountry(existingDataConfigFacade.GetDataConfig(),
                                                                             dbSystemConfigRow, dataBaseRow, newSystemRow);
            }

            return true;
        }

        static internal Country GetImportAddOn_OldStyle(out string selectedPath, bool hasInsertedPath = false, string insertedPath = "")
        {
            string addOnPath, addOnShortName, fileName;
            if (!GetImportAddOnPath_OldStyle(out addOnPath, out addOnShortName, out fileName, hasInsertedPath, insertedPath))
            {
                selectedPath = "";
                return null;
            }

            selectedPath = fileName;

            Country country = new Country(addOnShortName, true);
            country.GetCountryConfigFacade(true, addOnPath);
            return country;
        }

        internal static bool GetImportAddOnPath_OldStyle(out string addOnPath, out string addOnShortName, out string fileName, bool hasInsertedPath = false, string insertedPath = "")
        {
            addOnPath = addOnShortName = string.Empty;

            if (!hasInsertedPath)
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Title = "Please select the import add-on ...";
                openFileDialog.Filter = "XML Files (*.xml)|*.xml";
                openFileDialog.CheckPathExists = true;
                openFileDialog.CheckFileExists = true;
                openFileDialog.AddExtension = true;
                openFileDialog.Multiselect = false;
                openFileDialog.InitialDirectory = EM_AppContext.FolderEuromodFiles;


                if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                {
                    fileName = string.Empty;
                    return false;
                }
                    

                fileName = openFileDialog.FileName;
            }
            else
            {
                fileName = insertedPath;
            }
            
            FileInfo fileInfo = new FileInfo(fileName);

            string errorMessage;
            addOnShortName = fileInfo.Name.ToLower().EndsWith(".xml") ? fileInfo.Name.Substring(0, fileInfo.Name.Length - 4) : fileInfo.Name;

            if (!Country.DoesFolderContainValidXMLFiles(addOnShortName, out errorMessage, fileInfo.DirectoryName, true))
            {
                Tools.UserInfoHandler.ShowError("Failed to load add-on XML-files from '" + fileInfo.DirectoryName + "'.");
                return false;
            }
            addOnPath = fileInfo.DirectoryName;
            return true;
        }

        static internal Country GetImportCountry(out string selectedPath, bool getAddOn, bool hasInsertedPath = false, string insertedPath = "")
        {
            string importPath, countryShortName;
            if (!GetImportCountryPath(getAddOn, out importPath, out countryShortName, hasInsertedPath, insertedPath))
            {
                selectedPath = "";
                return null;
            }

            selectedPath = importPath;

            Country country = new Country(countryShortName, false);
            country._isAddOn = getAddOn;
            country.GetCountryConfigFacade(true, importPath);
            if (!getAddOn)
                country.GetDataConfigFacade(true, importPath);
            return country;
        }

        internal static bool GetImportCountryPath(bool getAddOn, out string importPath, out string countryShortName, bool hasInsertedPath = false, string insertedPath = "")
        {
            importPath = countryShortName = string.Empty;

            if (!hasInsertedPath)
            {
                FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
                folderBrowserDialog.SelectedPath = EM_AppContext.FolderEuromodFiles;
                folderBrowserDialog.Description = "Please select the import folder";
                if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                    return false;

                //separate the country folder from the rest of the path, i.e. split '...\exportedToSomewhere\IT' into '...\exportedToSomewhere\' and 'IT'
                importPath = folderBrowserDialog.SelectedPath;
            }
            else
            {
                importPath = insertedPath;
            }

            if (importPath.EndsWith("\\") || importPath.EndsWith("/"))
                importPath = importPath.Substring(0, importPath.Length - 1);
            int indexCountryFolder = importPath.LastIndexOfAny(new char[] { '\\', '/' });

            if (indexCountryFolder < 0)
            {
                Tools.UserInfoHandler.ShowError("Failed to load country XML-files from '" + importPath + "'.");
                return false; //unlikely, therefore dispense with error message
            }

            string errorMessage;
            countryShortName = importPath.Substring(indexCountryFolder + 1);
            //name of folder is supposed to be name of country (this is accomplished like this by export and allows to load systems of other countries)
            if (!Country.DoesFolderContainValidXMLFiles(countryShortName, out errorMessage, importPath, getAddOn))
            {
                Tools.UserInfoHandler.ShowError("Failed to load country XML-files from '" + importPath + "'.");
                return false;
            }
            return true;
        }

        static internal void CompareVersions(EM_UI_MainForm mainForm, CountryConfigFacade inCountryConfigFacade)
        {

            bool isAddOn = CountryAdministrator.IsAddOn(mainForm.GetCountryShortName());
            CompareVersionsForm compareVersionsForm = new CompareVersionsForm(isAddOn);
            if (compareVersionsForm.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return;

            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            string backUpFolder = RestoreManager.StoreCountry(mainForm);
            if (backUpFolder == string.Empty)
                return;

            //allow user to select the (version of the) country/add-on for comparison
            Country outCountry = compareVersionsForm._importCountry;

            if (outCountry == null)
                return;

            try
            {
                string userInfo = string.Empty;
                mainForm.Cursor = Cursors.WaitCursor;

                //initially import all systems of the comparison country
                ImportByIDAssistant importByIDAssistant = new ImportByIDAssistant(inCountryConfigFacade, outCountry.GetCountryConfigFacade());
                if (!importByIDAssistant.ImportSystems())
                    throw new System.ArgumentException(importByIDAssistant._importError); //to hand the error to the error handler

                //take care about remaining differences (most importantly in policy/function/parameter-order and parameter-name and -group) to inform user where to find respective info
                mainForm._importByIDDiscrepancies = importByIDAssistant.AssessRemainingDiscrepancies();

                //update treeview, because the following comparisons take place in the "visual display" and not in the data files
                mainForm.UpdateTree();

                //now compare each of the imported systems with its twin (if there is any)
                Dictionary<string, CountryConfig.SystemRow> equalSystems = new Dictionary<string, CountryConfig.SystemRow>(); //storage structure for equal systems
                List<CountryConfig.SystemRow> twinLessSystems = new List<CountryConfig.SystemRow>(); //storage structure for "new" systems, i.e. systems which only exist in out-country

                List<CountryConfig.SystemRow> importedSystems; List<CountryConfig.SystemRow> twinSystems;
                importByIDAssistant.GetImportedSystemsAndPotentialTwins(out importedSystems, out twinSystems);
                for (int index = 0; index < importedSystems.Count; ++index)
                {
                    CountryConfig.SystemRow importedSystem = importedSystems.ElementAt(index);
                    CountryConfig.SystemRow twinSystem = twinSystems.ElementAt(index);

                    if (twinSystem == null)
                    {
                        twinLessSystems.Add(importedSystem);
                        continue;
                    }

                    //get the columns of the twin-systems
                    TreeListColumn twinColumn = mainForm.GetTreeListBuilder().GetSystemColumnByID(twinSystem.ID);
                    TreeListColumn importedColumn = mainForm.GetTreeListBuilder().GetSystemColumnByID(importedSystem.ID);

                    //assess if there are differences: this has two purposes:
                    //- check whether the systems are equal and therefore the comparison system can be removed
                    //- expand the policies/functions with differences, to make them visible
                    IsNodeValueDifferent isNodeValueDifferent = new IsNodeValueDifferent(importedColumn, twinColumn, mainForm._importByIDDiscrepancies);
                    TreatSpecificNodes treatSpecificNodes = new TreatSpecificNodes(isNodeValueDifferent, null, true);
                    mainForm.treeList.NodesIterator.DoOperation(treatSpecificNodes);

                    if (treatSpecificNodes.GetSpecificNodes().Count - isNodeValueDifferent._countImportByIDDiscrepancies == 0)
                        equalSystems.Add(twinSystem.Name, importedSystem); //no difference, system can be removed (below), unless "other" differences are detected (e.g. in order)
                    else //mark the differences by implementing a conditional format (derived system in comparison to base system)
                    {
                        CountryConfig.ConditionalFormatRow conditionalFormatRow = inCountryConfigFacade.AddConditionalFormatRow(
                                            ConditionalFormattingHelper.GetDisplayTextFromColor(_colorForMarkingDifferences),
                                            ConditionalFormattingHelper.GetDisplayTextFromColor(System.Drawing.Color.Empty),
                                            string.Empty, twinSystem.Name);
                        inCountryConfigFacade.AddConditionalFormat_SystemsRow(conditionalFormatRow, importedSystem);
                    }
                }

                //compose an info for the user with respect to equal/new systems
                if (equalSystems.Count > 0)
                {
                    userInfo += "The following systems are equal in the comparison version (and were not imported):" + Environment.NewLine;
                    foreach (string equalSystemName in equalSystems.Keys)
                        userInfo += "   - " + equalSystemName + Environment.NewLine;
                    userInfo += Environment.NewLine;
                }
                if (twinLessSystems.Count > 0)
                {
                    userInfo += "The following systems only exist in the comparison version (and were imported for viewing):" + Environment.NewLine;
                    foreach (CountryConfig.SystemRow twinLessSystem in twinLessSystems)
                        userInfo += "   - " + twinLessSystem.Name + Environment.NewLine;
                    userInfo += Environment.NewLine;
                }
                if (importedSystems.Count - twinLessSystems.Count > equalSystems.Count)
                {
                    userInfo += "Different systems were imported for comparison and positioned alongside their 'twins'. See differences marked in "
                                + _colorForMarkingDifferences.ToString() + "." + Environment.NewLine + Environment.NewLine;
                }
                else if (mainForm._importByIDDiscrepancies == null)
                    userInfo += "No other differences found.";

                if (mainForm._importByIDDiscrepancies != null)
                    userInfo += "For differences which cannot be reflected in the spine move the mouse over the red and green info markers in the group column.";

                //remove the imported systems, which are equal to their twin-system
                for (int index = equalSystems.Count - 1; index >= 0; --index)
                    equalSystems.Values.ElementAt(index).Delete();

                RestoreManager.SaveDirectXMLAction(mainForm);
                mainForm.UpdateTree();
                RestoreManager.ReportSuccessAndInfo("Comparing versions", backUpFolder);
                UserInfoHandler.ShowInfo(userInfo);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception, "Failed to compare versions.", false);
                RestoreManager.RestoreCountry(mainForm, backUpFolder);
            }
            finally
            {
                mainForm.Cursor = Cursors.Default;
            }
        }

        static internal ImportByIDDiscrepancies LoadImportByIDDiscrepancies(EM_UI_MainForm mainForm)
        {
            ImportByIDDiscrepancies importByIDDiscrepancies = new ImportByIDDiscrepancies();
            if (!importByIDDiscrepancies.LoadFromFile())
                return null;

            mainForm.UpdateTree();
            return importByIDDiscrepancies;
        }
    }
}
