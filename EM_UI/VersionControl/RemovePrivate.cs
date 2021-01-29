using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;

namespace EM_UI.VersionControl
{
    internal partial class RemovePrivate
    {
        static string _privateComponents = string.Empty;

        static internal void RemovePrivateComponents()
        {
            ProgressIndicator progressIndicator = new ProgressIndicator(RemovePrivate_BackgroundEventHandler, //the handler passed to the progress indicator will do the work (see below)
                                                                        "Removing Private Components");
            if (progressIndicator.ShowDialog() == System.Windows.Forms.DialogResult.OK) //regular termination, i.e user did not cancel the procedure
                UserInfoHandler.ShowSuccess("Private components removed.");
        }

        static void RemovePrivate_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel

            //then adapt the copy
            try
            {
                List<Country> countries = CountryAdministrator.GetCountries();

                //remove private systems, policies and datasets of each country
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                    Country country = countries[i];
                    CountryConfigFacade countryConfigFacade = country.GetCountryConfigFacade();
                    DataConfigFacade dataConfigFacade = country.GetDataConfigFacade();

                    //assess which systems, policies and datasets are private
                    List<CountryConfig.SystemRow> privateSystems = new List<CountryConfig.SystemRow>(); //systems
                    List<CountryConfig.PolicyRow> privatePolicies = new List<CountryConfig.PolicyRow>(); //policies
                    List<CountryConfig.FunctionRow> privateFunctions = new List<CountryConfig.FunctionRow>(); //functions
                    List<CountryConfig.ParameterRow> privateParameters = new List<CountryConfig.ParameterRow>(); //parameters
                    List<string> privateSystemIDs = new List<string>(); //necessary for afterwards identifying database-connections of private systems
                    foreach (CountryConfig.SystemRow system in countryConfigFacade.GetSystemRows())
                    {
                        if (system.Private.ToLower() == DefPar.Value.YES.ToLower())
                        {
                            privateSystems.Add(system);
                            privateSystemIDs.Add(system.ID);
                        }
                        else
                        {
                            foreach (CountryConfig.PolicyRow policy in system.GetPolicyRows())
                            {
                                if (policy.Private == DefPar.Value.YES)
                                    privatePolicies.Add(policy);
                                else
                                {
                                    if (policy.PrivateComment != null && policy.PrivateComment != string.Empty)
                                        policy.PrivateComment = string.Empty; //remove private policy-comment if there is any
                                    foreach (CountryConfig.FunctionRow function in policy.GetFunctionRows())
                                    {
                                        if (function.Private == DefPar.Value.YES)
                                            privateFunctions.Add(function);
                                        else
                                        {
                                            if (function.PrivateComment != null && function.PrivateComment != string.Empty)
                                                function.PrivateComment = string.Empty; //remove private function-comment if there is any
                                            foreach (CountryConfig.ParameterRow parameter in function.GetParameterRows())
                                            {
                                                if (parameter.Private == DefPar.Value.YES)
                                                    privateParameters.Add(parameter);
                                                else if (parameter.PrivateComment != null && parameter.PrivateComment != string.Empty)
                                                    parameter.PrivateComment = string.Empty; //remove private parameter-comment if there is any
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    List<DataConfig.DataBaseRow> privateDataSets = new List<DataConfig.DataBaseRow>(); //datasets
                    List<DataConfig.DBSystemConfigRow> privateDBSystemConfigs = new List<DataConfig.DBSystemConfigRow>(); //database-connections of private systems
                    foreach (DataConfig.DataBaseRow dataSet in dataConfigFacade.GetDataBaseRows())
                    {
                        if (dataSet.Private.ToLower() == DefPar.Value.YES.ToLower())
                            privateDataSets.Add(dataSet);
                        else
                        {
                            foreach (DataConfig.DBSystemConfigRow dbSystemConfig in dataConfigFacade.GetDBSystemConfigRows(dataSet.ID))
                            {
                                if (privateSystemIDs.Contains(dbSystemConfig.SystemID))
                                    privateDBSystemConfigs.Add(dbSystemConfig);
                            }
                        }
                    }

                    //remove user-set node colors
                    countryConfigFacade.RemoveAllNodeColors();

                    //restore or install default base-system-colouring
                    countryConfigFacade.setAutomaticConditionalFormatting(true);

                    //remove private systems
                    if (countryConfigFacade.GetCountryRow().Private == DefPar.Value.YES || //if country is private or
                        privateSystems.Count == countryConfigFacade.GetSystemRows().Count) //there are no systems left, delete country
                    {
                        Directory.Delete(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + country._shortName, true);
                        country.SetCountryConfigFacade(null);
                        country.SetDataConfigFacade(null);
                        continue;
                    }
                    else //otherwise delete private systems
                    {
                        foreach (CountryConfig.SystemRow privateSystem in privateSystems)
                            privateSystem.Delete();
                    }

                    //remove private parameters
                    foreach (CountryConfig.ParameterRow privateParameter in privateParameters)
                        privateParameter.Delete();

                    //remove private functions
                    foreach (CountryConfig.FunctionRow privateFunction in privateFunctions)
                        privateFunction.Delete();

                    //remove private policies
                    foreach (CountryConfig.PolicyRow privatePolicy in privatePolicies)
                        privatePolicy.Delete();

                    //remove private datasets
                    foreach (DataConfig.DataBaseRow privateDataSet in privateDataSets)
                        privateDataSet.Delete();

                    //remove database-connections of private systems
                    foreach (DataConfig.DBSystemConfigRow privateDBSystemConfig in privateDBSystemConfigs)
                        privateDBSystemConfig.Delete();

                    country.WriteXML();
                    country.SetCountryConfigFacade(null);
                    country.SetDataConfigFacade(null);

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0));
                }

                //remove private add-ons
                foreach (Country addOn in CountryAdministrator.GetAddOns())
                {
                    bool oldStyle = CountryAdministrator.ConsiderOldAddOnFileStructure(true);
                    CountryConfigFacade addOnConfigFacade = addOn.GetCountryConfigFacade();
                    if (addOnConfigFacade.GetCountryRow().Private != DefPar.Value.YES)
                        continue;
                    if (oldStyle)
                        File.Delete(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + addOn._shortName + ".xml");
                    else
                        Directory.Delete(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + addOn._shortName, true);
                    addOn.SetCountryConfigFacade(null);
                }

                // remove the "other" column from the variables file
                VarConfigFacade vcf = EM_AppContext.Instance.GetVarConfigFacade();
                foreach (VarConfig.CountryLabelRow r in from l in vcf._varConfig.CountryLabel where l.Country.ToLower() == "other" select l)
                    r.Delete();
                vcf.Commit(); vcf.WriteXML();

                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                e.Cancel = true; //stop the process and allow progress indicator to set dialog result to Cancel
            }
        }

        static internal void CheckPrivateComponents() //assess which datasets, systems and policies are private, using the progress indicator as this may take a while
        {
            //the handler passed to the progress indicator will do the work (see below)
            ProgressIndicator progressIndicator = new ProgressIndicator(CheckPrivateComponents_BackgroundEventHandler, "Assessing private components");
            if (progressIndicator.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return; //user cancelled the procedure - nothing further to do

            try //show and store results
            {
                string resultPath = EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles) + "PrivateComponents.txt"; //store result in temporary folder
                using (StreamWriter writer = new StreamWriter(resultPath))
                    writer.Write(_privateComponents);
                System.Diagnostics.Process.Start(resultPath); //open resulting textfile with default text-editor
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        static void CheckPrivateComponents_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        { //assess which datasets, systems and policies are private (as a background process of the progress indicator)
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel

            try
            {
                List<Country> countries = CountryAdministrator.GetCountries();

                _privateComponents = string.Empty;
                for (int i = 0; i < countries.Count; i++)
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                    Country country = countries[i];
                    CountryConfigFacade countryConfigFacade = country.GetCountryConfigFacade();
                    DataConfigFacade dataConfigFacade = country.GetDataConfigFacade();

                    //assess which systems are private
                    List<CountryConfig.SystemRow> privateSystems = new List<CountryConfig.SystemRow>();
                    foreach (CountryConfig.SystemRow system in countryConfigFacade.GetSystemRows())
                    {
                        if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                        if (system.Private.ToLower() == DefPar.Value.YES.ToLower())
                            privateSystems.Add(system);
                    }

                    //assess which policies are private
                    List<CountryConfig.PolicyRow> privatePolicies = new List<CountryConfig.PolicyRow>();
                    List<CountryConfig.FunctionRow> privateFunctions = new List<CountryConfig.FunctionRow>();
                    List<CountryConfig.ParameterRow> privateParameters = new List<CountryConfig.ParameterRow>();
                    if (countryConfigFacade.GetSystemRows().Count > privateSystems.Count)
                    {//only necessary to do for first system, as policies can only be marked private for all systems, but check if not all systems are private
                        foreach (CountryConfig.PolicyRow policy in countryConfigFacade.GetSystemRows()[0].GetPolicyRows())
                        {
                            if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                            if (policy.Private == DefPar.Value.YES)
                                privatePolicies.Add(policy);
                            else
                            {
                                foreach (CountryConfig.FunctionRow function in policy.GetFunctionRows())
                                {
                                    if (function.Private == DefPar.Value.YES)
                                        privateFunctions.Add(function);
                                    else
                                    {
                                        foreach (CountryConfig.ParameterRow parameter in function.GetParameterRows())
                                            if (parameter.Private == DefPar.Value.YES)
                                                privateParameters.Add(parameter);
                                    }
                                }
                            }
                        }
                    }

                    //assess which datasets are private
                    List<DataConfig.DataBaseRow> privateDataSets = new List<DataConfig.DataBaseRow>();
                    foreach (DataConfig.DataBaseRow dataSet in dataConfigFacade.GetDataBaseRows())
                    {
                        if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: see above

                        if (dataSet.Private.ToLower() == DefPar.Value.YES.ToLower())
                            privateDataSets.Add(dataSet);
                    }

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0)); //increase progress (for each country)

                    if (privateDataSets.Count == 0 && privateSystems.Count == 0 && privatePolicies.Count == 0)
                        continue;

                    //fill _privateComponents into string which is to be output in btnCheckPrivate_Click
                    _privateComponents += country._shortName.ToUpper() + Environment.NewLine;

                    if (countryConfigFacade.GetCountryRow().Private == DefPar.Value.YES)
                        _privateComponents += "\tPRIVATE COUNTRY" + Environment.NewLine;

                    if (privateDataSets.Count != 0)
                    {
                        _privateComponents += "\tPRIVATE DATASETS" + Environment.NewLine;
                        foreach (DataConfig.DataBaseRow dataSet in privateDataSets)
                            _privateComponents += "\t\t" + dataSet.Name + Environment.NewLine;
                    }
                    if (privateSystems.Count != 0)
                    {
                        _privateComponents += "\tPRIVATE SYSTEMS" + Environment.NewLine;
                        foreach (CountryConfig.SystemRow system in privateSystems)
                            _privateComponents += "\t\t" + system.Name + Environment.NewLine;
                    }
                    if (privatePolicies.Count != 0)
                    {
                        _privateComponents += "\tPRIVATE POLICIES" + Environment.NewLine;
                        foreach (CountryConfig.PolicyRow policy in privatePolicies)
                            _privateComponents += "\t\t" + policy.Name + Environment.NewLine;
                    }
                    if (privateFunctions.Count != 0)
                    {
                        _privateComponents += "\tPRIVATE FUNCTIONS" + Environment.NewLine;
                        foreach (CountryConfig.FunctionRow function in privateFunctions)
                            _privateComponents += "\t\t" + function.Name + " in policy " + function.PolicyRow.Name + " (order " + function.Order + ")" + Environment.NewLine;
                    }
                    if (privateParameters.Count != 0)
                    {
                        _privateComponents += "\tPRIVATE PARAMETERS" + Environment.NewLine;
                        foreach (CountryConfig.ParameterRow parameter in privateParameters)
                            _privateComponents += "\t\t" + parameter.Name + " in function " + parameter.FunctionRow.Name + " in policy " + parameter.FunctionRow.PolicyRow.Name +
                                                  " (function-order " + parameter.FunctionRow.Order + ", parameter-order " + parameter.Order + ")" + Environment.NewLine;
                    }
                    _privateComponents += "_________________________________________________________" + Environment.NewLine;
                }

                foreach (Country addOn in CountryAdministrator.GetAddOns())
                {
                    CountryConfigFacade addOnConfigFacade = addOn.GetCountryConfigFacade();
                    if (addOnConfigFacade.GetCountryRow().Private != DefPar.Value.YES)
                        continue;
                    _privateComponents += addOn._shortName.ToUpper() + Environment.NewLine;
                    _privateComponents += "\tPRIVATE ADD-ON" + Environment.NewLine;
                    _privateComponents += "_________________________________________________________" + Environment.NewLine;
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                e.Cancel = true; //stop the process and allow progress indicator to set dialog result to Cancel
            }
        }
    }
}