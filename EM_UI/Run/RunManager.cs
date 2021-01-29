using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.GlobalAdministration;
using EM_UI.ImportExport;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace EM_UI.Run
{
    internal partial class RunManager
    {
        List<Run> _runs = null;
        RunInfoForm _runInfoForm = null;
        int _finishedCounter = 0;
        int _indexNextRun = 0;

        Run _runShowingLog = null;
        bool _showRunLog = true;

        bool _doNotPoolSystemsDatasets = false; // if this option is selected, the 'Run Log'-button is renamed into 'Open' after finishing and allows to open the output folder

        const string _labErrorLog = "ERROR-LOG ";
        const string _labRunLog = "RUN-LOG ";
        const string _placeHolderSystemID = "REPLACESYSTEMID";

        void UpdateRunInfo(Run specificRun = null)
        {
            try
            {
                if (_runInfoForm == null || _runInfoForm.IsDisposed)
                    return;

                foreach (Run run in _runs)
                {
                    if (specificRun != null && run._rowInRunForm != specificRun._rowInRunForm)
                        continue;

                    _runInfoForm.SetStatus(run._rowInRunForm, run._processStatus, run._processStatusAdditionalInfo);

                    bool isActive = run._processStatus == Run._processStatus_Running;
                    _runInfoForm.SetButtonStatus(RunInfoForm._colShowErrorLog, run._rowInRunForm, run.HasErrorLog());

                    if (!_doNotPoolSystemsDatasets) _runInfoForm.SetButtonStatus(RunInfoForm._colShowRunLog, run._rowInRunForm, isActive && run.HasRunLog());
                    else DoNotPoolHandling_Prepare(run);

                    _runInfoForm.SetButtonStatus(RunInfoForm._colStop, run._rowInRunForm, isActive);
                }
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        Run GetRunByRowInRunInfo(int rowInRunInfo)
        {
            foreach (Run run in _runs)
            {
                if (run._rowInRunForm == rowInRunInfo)
                    return run;
            }
            return null;
        }

        string ComposeLog(List<string> log)
        {
            try
            {
                string sLog = string.Empty;
                foreach (string entry in log)
                    sLog += entry + Environment.NewLine;
                return sLog;
            }
            catch (Exception exception)
            {
                //do nothing: this catches the message that the 'collection has changed', but obviously this does not harm
                UserInfoHandler.RecordIgnoredException("RunManager.ComposeLog", exception);
                return string.Empty;
            }
        }

        internal RunManager()
        {
            _runs = new List<Run>();
            _runInfoForm = new RunInfoForm(this);
        }

        internal void AddRun(string configurationFile, string runFormInfoText, string configID, Dictionary<string, string> contentConfig)
        {
            Run run = new Run(this, configurationFile);
            run._runFormInfoText = runFormInfoText;
            run._rowInRunForm = _runInfoForm.AddInfoRow(runFormInfoText);
            run._configId = configID;
            run._contentConfig = contentConfig;
            _runs.Add(run);
        }

        internal bool PerformRuns()
        {
            int maxRuns = EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().Get().ParallelRunsAuto?Environment.ProcessorCount:EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().Get().ParallelRuns;
            while (_indexNextRun < _runs.Count && _indexNextRun < maxRuns)
            {
                Run run = _runs.ElementAt(_indexNextRun++);
                if (!run.StartRun())
                    return false;
            }

            UpdateRunInfo();
            _runInfoForm.Show();

            return true;
        }

        internal void HandleRunExited(Run sendingRun)
        {
            if (_indexNextRun < _runs.Count)
            {
                Run run = _runs.ElementAt(_indexNextRun++);
                if (!run.StartRun())
                    return;
            }

            UpdateRunInfo();

            ++_finishedCounter;
            if (_finishedCounter >= _runs.Count)
            {
                _runInfoForm.Text += " and finished " + DateTime.Now.ToString();

                if (!_runEM2) EM3_WriteLog();
            }
        }

        internal bool Close(bool sendedByRunForm = false)
        {
            if (_finishedCounter < _runs.Count)
            {
                if (Tools.UserInfoHandler.GetInfo("Closing will abort active model runs.", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return false;

                _indexNextRun = _finishedCounter = _runs.Count; //to avoid any further starting of a run in HandleRunExited

                for (int index = 0; index < _runs.Count; ++index)
                {
                    Run run = _runs.ElementAt(index);
                    run.StopRun();
                }
            }

            if (!sendedByRunForm && _runInfoForm != null && !_runInfoForm.IsDisposed)
                _runInfoForm.Close();

            EM_AppContext.Instance.DeRegisterRunManager(this);
            return true;
        }

        internal void HandleInfoForm_ButtonClick(object sender, EventArgs e)
        {
            try
            {
                Button senderButton = sender as Button;
                int column = (senderButton.Tag as Dictionary<int, int>).ElementAt(0).Key;
                int row = (senderButton.Tag as Dictionary<int, int>).ElementAt(0).Value;

                Run run = GetRunByRowInRunInfo(row);

                switch (column)
                {
                    case RunInfoForm._colShowErrorLog:
                        _runInfoForm.SetLog(_labErrorLog + run._runFormInfoText, ComposeLog(run.ErrorLog));
                        _runShowingLog = run; //remember which run and which log is currently displayed in the info-window (to be used in HandleLogChanged)
                        _showRunLog = false;
                        break;

                    case RunInfoForm._colShowRunLog:
                        if (senderButton.Text == BUTTON_TEXT_OPEN) DoNotPoolHandling_Open(run);
                        else
                        {
                            _runInfoForm.SetLog(_labRunLog + run._runFormInfoText, ComposeLog(run.RunLog));
                            _runShowingLog = run; //see above
                            _showRunLog = true;
                        }
                        break;

                    case RunInfoForm._colStop:
                        run.StopRun();
                        break;
                }

                UpdateRunInfo();
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        internal void HandleLogChanged(Run run)
        {
            try
            {
                if (run.RunLog.Count == 1 || run.ErrorLog.Count == 1)
                    UpdateRunInfo(run); //either the run-log or the error-log of the run got its first entry - enable respective show-button

                if (run.Equals(_runShowingLog)) //update run- or error-log if the reporting run is the one currently shown in the info-window
                {
                    List<string> log = _showRunLog ? run.RunLog : run.ErrorLog;
                    _runInfoForm.SetLog(run._runFormInfoText, ComposeLog(log));
                }
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        void DeleteTemporaryFiles()
        {
            if (Directory.Exists(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles)))
            {
                foreach (string fileName in Directory.GetFiles(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles)))
                {
                    try
                    {
                        FileInfo fileInfo = new FileInfo(fileName);
                        if (fileInfo.CreationTime < DateTime.Now - TimeSpan.FromDays(1)) //do not delete files younger than one day to avoid deleting still required backups or configs
                            File.Delete(fileName);
                    }
                    catch (Exception exception)
                    {
                        //do nothing
                        UserInfoHandler.RecordIgnoredException("RunManager.DeleteTemporaryFiles", exception);
                    }
                }
            }
        }

        private class ConfigKey
        {
            private string ccShortName, dataId, sysId;
            internal ConfigKey(string _ccShortName, string _dataId, string _sysId) { ccShortName = _ccShortName; dataId = _dataId; sysId = _sysId; }

            internal class Comparer : IEqualityComparer<ConfigKey>
            {
                public bool Equals(ConfigKey configKey1, ConfigKey configKey2)
                {
                    return configKey1.ccShortName.ToLower() == configKey2.ccShortName.ToLower() &&
                           configKey1.dataId.ToLower() == configKey2.dataId.ToLower() &&
                           configKey1.sysId.ToLower() == configKey2.sysId.ToLower();
                }
                public int GetHashCode(ConfigKey configKey) { return base.GetHashCode(); }
            }
        }

        internal bool KickOffRuns(string outputPath, string emVersion, RunMainForm runMainForm, bool runEM2)
        {
            _runEM2 = runEM2; _outputPath = outputPath; _doNotPoolSystemsDatasets = runMainForm._doNotPoolSystemsDatasets;
            DeleteTemporaryFiles(); //delete temporary files (EMConfig_XXX.xml) of last run if there are any

            try
            {
                //all EMConfig_XXX.xml's are first put into this list: they cannot be directly written, as systems may be added (to be run with the same dataset)
                Dictionary<ConfigKey, Dictionary<string, string>> contentEMConfigs = new Dictionary<ConfigKey, Dictionary<string, string>>(new ConfigKey.Comparer());

                //for each dataset (selected via a "normal" or an add-on system) generate one EMConfig
                if (!GenerateEMConfigs(outputPath, emVersion, contentEMConfigs, runMainForm))
                    return false;

                if (!(_runEM2 ? RenameOutputFiles(contentEMConfigs, runMainForm)
                              : EM3_AddNonDefaultSwitches_To_OutputFileName(contentEMConfigs, runMainForm)))
                    return false;

                //add the add-on systems to the just generated EMConfigs
                bool activeAddOns = false;
                if (_runEM2) { if (!AddAddOnsToEMConfigs(outputPath, contentEMConfigs, runMainForm, out activeAddOns)) return false; }
                else { if (!EM3_AddAddOnInfo(outputPath, contentEMConfigs, runMainForm)) return false; }

                //now actually write the EMConfigXXX.xml files and hand them over to the run-manager
                int isLastCounter = 0;
                foreach (Dictionary<string, string> contentEMConfig in CloneConfigsForAllExtensionOption(contentEMConfigs.Values.ToList()))
                {
                    string configID = Guid.NewGuid().ToString();
                    string configurationFileNameAndPath = EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles) + EM_XmlHandler.TAGS.EM2CONFIG_labelEMConfig + configID + ".xml";
                    string runFormInfoText = string.Empty;
                    using (XmlTextWriter configurationFileWriter = new XmlTextWriter(configurationFileNameAndPath, null))
                    { 
                        configurationFileWriter.Formatting = Formatting.Indented;
                        configurationFileWriter.WriteStartDocument(true);
                        configurationFileWriter.WriteStartElement(EM_XmlHandler.TAGS.EM2CONFIG_labelEMConfig);
                        
                        foreach (string key in contentEMConfig.Keys)
                        {
                            if (key == RunMainForm._labelRunFormInfoText)
                                runFormInfoText += contentEMConfig[key] + ")";
                            else if (key.StartsWith(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID)) //remove Guid, see above
                                configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID, contentEMConfig[key]);
                            else if (key.StartsWith(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH)) //remove Guid, see above
                                configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH, contentEMConfig[key]);
                            else
                                configurationFileWriter.WriteElementString(key, contentEMConfig[key]);
                        }

                        ++isLastCounter;
                        if (isLastCounter == contentEMConfigs.Values.Count)
                            configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_LAST_RUN, DefPar.Value.YES);
                        else
                            configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_LAST_RUN, DefPar.Value.NO);
                        configurationFileWriter.WriteEndElement();
                        configurationFileWriter.WriteEndDocument();
                    }
                    if (GetExtensionInfo(contentEMConfig, out string extInfo)) runFormInfoText += "; " + extInfo;
                    AddRun(configurationFileNameAndPath, runFormInfoText, configID, contentEMConfig);
                }

                if (!_runEM2 && !EM3_Transform()) return false; // translate country/ies, addOn(s) and global files to EM3-format

                return PerformRuns();
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
                return false;
            }
        }

        // if the 'all' option is selected for any extensions, generate respective clones of the original configuration file, to run both options (off and on)
        List<Dictionary<string, string>> CloneConfigsForAllExtensionOption(List<Dictionary<string, string>> origContentEMConfigs)
        {
            try
            {
                List<Dictionary<string, string>> adaptedContentEMConfigs = new List<Dictionary<string, string>>();
                foreach (Dictionary<string, string> contentEMConfig in origContentEMConfigs)
                {
                    List<Dictionary<string, string>> clones = new List<Dictionary<string, string>>();
                    MakeClones(contentEMConfig, ref clones);
                    adaptedContentEMConfigs.AddRange(clones);
                }
                return adaptedContentEMConfigs;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, $"Failed to run all extension options.", false);
                return new List<Dictionary<string, string>>();
            }

            void MakeClones(Dictionary<string, string> origContentEMConfig, ref List<Dictionary<string, string>> clones, bool overwriteSuffix = true)
            {
                Dictionary<string, string> onClone = null, offClone = null;
                foreach (var entry in origContentEMConfig)
                {
                    if (entry.Key.StartsWith(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH) && entry.Value.EndsWith(DefPar.Value.EXTENSION_ALL))
                    {
                        if (overwriteSuffix && origContentEMConfig.ContainsKey(EM_XmlHandler.TAGS.STDOUTPUT_FILENAME_SUFFIX))
                            origContentEMConfig.Remove(EM_XmlHandler.TAGS.STDOUTPUT_FILENAME_SUFFIX); // remove a possible existing suffix, due to auto-rename=on
                        onClone = MakeCopy(origContentEMConfig, entry, DefPar.Value.ON);
                        offClone = MakeCopy(origContentEMConfig, entry, DefPar.Value.OFF);
                        break;
                    }
                }
                if (onClone == null && offClone == null) clones.Add(origContentEMConfig);
                else { MakeClones(onClone, ref clones, false); MakeClones(offClone, ref clones, false); }

                Dictionary<string, string> MakeCopy(Dictionary<string, string> orig, KeyValuePair<string, string> extEntry, string offOn)
                {
                    Dictionary<string, string> clone = new Dictionary<string, string>(); bool suffixAdded = false;
                    foreach (var entry in orig)
                    {
                        if (entry.Key == extEntry.Key && entry.Value == extEntry.Value)
                            clone.Add(entry.Key, entry.Value.Substring(0, entry.Value.Length - DefPar.Value.EXTENSION_ALL.Length) + offOn);
                        else if (entry.Key == EM_XmlHandler.TAGS.STDOUTPUT_FILENAME_SUFFIX)
                            { clone.Add(entry.Key, entry.Value + GetSuffix()); suffixAdded = true; }
                        else clone.Add(entry.Key, entry.Value);
                    }
                    if (!suffixAdded) clone.Add(EM_XmlHandler.TAGS.STDOUTPUT_FILENAME_SUFFIX, GetSuffix());
                    return clone;

                    string GetSuffix()
                    {
                        string extShortName = extEntry.Value.Substring(0, extEntry.Value.IndexOf('=')), pathSaveShortName = string.Empty;
                        for (int i = 0; i < extShortName.Length; ++i) if (!Path.GetInvalidFileNameChars().Contains(extShortName[i])) pathSaveShortName += extShortName[i];
                        return $"_{pathSaveShortName}{offOn}";
                    }
                }
            }
        }

        bool RenameOutputFiles(Dictionary<ConfigKey, Dictionary<string, string>> contentEMConfigs, RunMainForm runMainForm)
        {
            try
            {
                Dictionary<string, Country> temporaryCountries = new Dictionary<string, Country>(); //a copy of the country is generated in the temp-folder for each country running an add-on-system
                foreach (DataGridViewRow systemRow in runMainForm.dgvRun.Rows) //loop over systems
                {
                    //assess whether any run-checkboxes (standard run, add-ons) are checked for the system
                    bool systemSelected = EM_Helpers.SaveConvertToBoolean(systemRow.Cells[runMainForm.colRun.Name].Value);
                    string countryShortName = systemRow.Cells[runMainForm.colCountry.Name].Value.ToString();

                    if (systemSelected == false)
                        continue; //system not selected for run

                    //assess which dataset is selected for run
                    DataConfig.DBSystemConfigRow dbSystemConfigRow = runMainForm.GetSelectedDBSystemCombination(systemRow);
                    if (dbSystemConfigRow == null)
                        continue; //should not happen

                    //find the EMConfig (prepared in GenerateEMConfigs) where the output needs to be renamed
                    Dictionary<string, string> contentEMConfig = null;
                    ConfigKey key = new ConfigKey(countryShortName, dbSystemConfigRow.DataBaseID, dbSystemConfigRow.SystemID);
                    if (contentEMConfigs.ContainsKey(key))
                        contentEMConfig = contentEMConfigs[key];
                    if (contentEMConfig == null)
                        continue; //should not happen

                    //make a copy of the country, to be later stored in the temp-folder, if it not already exists
                    if (!temporaryCountries.ContainsKey(countryShortName))
                        temporaryCountries.Add(countryShortName, CountryAdministrator.GetCopyOfCountry(countryShortName));

                    // check if you need to automatically rename the output file name
                    if (EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetPolicySwitchAutoRenameValue())
                    {
                        //use copy of country to automatically rename the output file name and run it from the temp-folder, thus not destroying the "real" country files
                        Country copiedCountry = temporaryCountries[countryShortName];

                        string renameString = GenerateSwitchRenamingSuffix(runMainForm, systemRow, dbSystemConfigRow);
                        if (renameString != string.Empty)
                        {
                            CountryConfig.SystemRow sr = copiedCountry.GetCountryConfigFacade().GetSystemRowByID(dbSystemConfigRow.SystemID);
                            addChangesSwitchesInfoToOutputFilename(sr, renameString);
                            contentEMConfig[EM_XmlHandler.TAGS.EM2CONFIG_PARAMPATH] = EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles); //change path from country folder to temp-folder
                        }
                    }
                }
                // then write all changed country files in the temp folder
                foreach (Country c in temporaryCountries.Values) c.WriteXML(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles));
            }
            catch
            {
            }
            return true;
        }

        bool AddAddOnsToEMConfigs(string outputPath, Dictionary<ConfigKey, Dictionary<string, string>> contentEMConfigs, RunMainForm runMainForm, out bool activeAddOns)
        {
            activeAddOns = false; string hiddenExtensionsWarning = string.Empty;
            Dictionary<string, Country> temporaryCountries = new Dictionary<string, Country>(); //a copy of the country is generated in the temp-folder for each country running an add-on-system
            string polSwitchWarningInfos = string.Empty; //just storage-place for gathering infos about a warning (see end of the function)
            //but there ought to be only one copy, even if there are several add-on-systems - therefore use this list
            foreach (DataGridViewRow systemRow in runMainForm.dgvRun.Rows) //loop over systems
            {
                //assess whether any add-on is selected for run
                List<AddOnSystemInfo> selectedAddOns = new List<AddOnSystemInfo>();
                foreach (DataGridViewColumn column in runMainForm.dgvRun.Columns)
                {
                    if (!column.Name.StartsWith(RunMainForm._colAddOnPrefix))
                        continue;
                    if (EM_Helpers.SaveConvertToBoolean(systemRow.Cells[column.Name].Value))
                    {
                        if (!AddOnInfoHelper.GetSupportedSystemInfo(systemRow.Cells[runMainForm.colSystem.Name].Value.ToString(),
                            column.Tag as List<AddOnSystemInfo>, out List<AddOnSystemInfo> supportedAOSystemInfo)) return false;
                        selectedAddOns.AddRange(supportedAOSystemInfo); //put the info for any selected add-on in a list, to be used below
                    }
                }
                if (selectedAddOns.Count == 0)
                    continue; //no add-on selected for run

                activeAddOns = true;

                //assess which dataset is selected for run
                DataConfig.DBSystemConfigRow dbSystemConfigRow = runMainForm.GetSelectedDBSystemCombination(systemRow);
                if (dbSystemConfigRow == null)
                    continue; //should not happen

                //for each (available) switchalbe policy a POLICY_SWITCH-entry needs to be added
                //here these entries are only gathered, they are put to the config-file below, where the add-on-systems are added (and _placeHolderSystemID is replaced by the actual ID of the add-on-system)
                List<string> policySwitchEntries = GenerateConfigEntriesPolicySwitches(runMainForm, systemRow, _placeHolderSystemID, dbSystemConfigRow, _runEM2, out string hiddenWarning);
                hiddenExtensionsWarning += hiddenWarning;

                //find the EMConfig (prepared in GenerateEMConfigs) where the add-on-system(s) need to be added
                string countryShortName = systemRow.Cells[runMainForm.colCountry.Name].Value.ToString();
                Dictionary<string, string> contentEMConfig = null;
                ConfigKey key = new ConfigKey(countryShortName, dbSystemConfigRow.DataBaseID, dbSystemConfigRow.SystemID);
                if (contentEMConfigs.ContainsKey(key))
                    contentEMConfig = contentEMConfigs[key];
                if (contentEMConfig == null)
                    continue; //should not happen

                //make a copy of the country, to be later stored in the temp-folder, if it not already exists
                if (!temporaryCountries.ContainsKey(countryShortName))
                    temporaryCountries.Add(countryShortName, CountryAdministrator.GetCopyOfCountry(countryShortName));

                AddOnGenerator addOnGenerator = new AddOnGenerator();

                foreach (AddOnSystemInfo addOnSystemInfo in selectedAddOns) //loop over the add-ons which are selected for run (gathered above)
                {
                    CountryConfig.SystemRow addOnSystemRow = addOnGenerator.PerformAddOnGeneration(dbSystemConfigRow.SystemName, countryShortName,
                                                    addOnSystemInfo._addOnShortName, dbSystemConfigRow.SystemName, addOnSystemInfo._addOnSystemName);
                    if (addOnSystemRow == null)
                    {
                        ImportExportHelper.WriteErrorLogFile(outputPath, addOnGenerator.GetErrorMessages());
                        return false;
                    }

                    //use copy of country to add the add-on and run it from the temp-folder, thus not destroying the "real" country files
                    Country copiedCountry = temporaryCountries[countryShortName];

                    //add just created add-on system to copied country
                    CountryConfig.SystemRow newSystemRow = CountryConfigFacade.CopySystemRowToAnotherCountry(addOnSystemRow, addOnSystemRow.Name,
                                                                                        copiedCountry.GetCountryConfigFacade().GetCountryConfig());
                    //add system-dataset-connections for the add-on system (same as for base system)
                    CountryConfig.SystemRow baseSystemRow = CountryAdministrator.GetCountryConfigFacade(countryShortName).GetSystemRowByName(newSystemRow.Name); //need to use the system-row in the real country, as copied country now contains two systems with this name (add-on-system has same name as base-system, to allow for correct replacing of =sys=)
                    copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(baseSystemRow, newSystemRow);

                    //this function is mainly to avoid confusion about switchable-policies which seem to be 'on' though they are permanently switched 'off' in the add-on
                    //the function cares for 'off' entries in EMConfig and header-file and issues a warning if the switch is viewed and thus visible (and appears to be 'on')
                    CheckForPermanentlySwitchedOffPolicies(ref newSystemRow, ref policySwitchEntries, ref polSwitchWarningInfos, addOnSystemInfo._addOnShortName, copiedCountry, runMainForm);

                    copiedCountry.WriteXML(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles));

                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID + Guid.NewGuid().ToString(), newSystemRow.ID); //see GenerateEMConfigs wrt Guid
                    contentEMConfig[RunMainForm._labelRunFormInfoText] += addOnSystemInfo._addOnSystemName + " "; //add to text that is displayed in the window showing run status
                    //redefined switches (as gathered above) need to be entered for each add-on-system
                    foreach (string policySwitchEntry in policySwitchEntries)
                        contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH + Guid.NewGuid().ToString(),
                            policySwitchEntry.Replace(_placeHolderSystemID, newSystemRow.ID)); //replace the placeholder temporarily used above with the actual id of the add-on-system
                    contentEMConfig[EM_XmlHandler.TAGS.EM2CONFIG_PARAMPATH] = EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles); //change path from country folder to temp-folder
                }
            }

            if (hiddenExtensionsWarning != string.Empty)
            {
                if (UserInfoHandler.GetInfo("Note that for not displayed extensions default settings are used!" + Environment.NewLine + Environment.NewLine + hiddenExtensionsWarning,
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel) return false;
            }

            if (polSwitchWarningInfos != string.Empty) //issue an optional warning if function CheckForPermanentlySwitchedOffPolicies (see above) gives reason
                OptionalWarningsManager.Show(OptionalWarningsManager._policySwitchWarning, false, string.Empty, polSwitchWarningInfos);

            return true;
        }

        static void CheckForPermanentlySwitchedOffPolicies(ref CountryConfig.SystemRow addOnSystemRow, ref List<string> policySwitchEntries,
                                                           ref string warningInfos, string addOnName, Country country, RunMainForm runMainForm)
        {
            try
            {
                List<CountryConfig.ParameterRow> switchedOffRows = new List<CountryConfig.ParameterRow>();

                //search for any 'ChangeParam'-function, which may contain the permanent off-switching of a switchable policy
                //(note that off-switching is the only direction, on-switching is only possible if policies are set to toggle, which is not possible with switch-policies)
                foreach (CountryConfig.PolicyRow polRow in addOnSystemRow.GetPolicyRows())
                {
                    if (polRow.Switch != DefPar.Value.ON) continue;
                    foreach (CountryConfig.FunctionRow funcRow in polRow.GetFunctionRows())
                    {
                        if (funcRow.Switch != DefPar.Value.ON) continue;
                        if (funcRow.Name.ToLower() == DefFun.ChangeParam.ToLower())
                        {
                            //within the 'ChangeParam'-function gather the 'off's ...
                            List<string> potentialGroups = new List<string>();
                            foreach (CountryConfig.ParameterRow parRow in funcRow.GetParameterRows())
                                if (parRow.Value == DefPar.Value.OFF &&
                                    (parRow.Name.ToLower() == DefPar.ChangeParam.Param_NewVal.ToLower() ||
                                     parRow.Name.ToLower() == DefPar.ChangeParam.Param_CondVal.ToLower()))
                                    potentialGroups.Add(parRow.Group);
                            //... to then assess the respective ids, i.e. what is switched off
                            foreach (CountryConfig.ParameterRow parRow in funcRow.GetParameterRows())
                                if (potentialGroups.Contains(parRow.Group) && parRow.Name.ToLower() == DefPar.ChangeParam.Param_Id.ToLower())
                                    switchedOffRows.Add(parRow);
                        }
                    }
                }

                //now we have parameter-rows (usually one) of 'something' that is switched off in the add-on, which need to be analysed:
                List<string> concernedSwitchPols = new List<string>();
                foreach (CountryConfig.ParameterRow switchedOffRow in switchedOffRows)
                {
                    CountryConfig.PolicyRow polRow = country.GetCountryConfigFacade().GetPolicyRowByID(switchedOffRow.Value /* that's the id of what is switched off */);
                    if (polRow == null) continue; //doesn't seem to be a policy

                    for (int i = 0; i < policySwitchEntries.Count; ++i )
                    {   //policySwitchEntries contain strings like 'BTA_??=REPLACESYSTEMID=on', i.e. get the first part, which is the policy-name
                        string policySwitchEntry = policySwitchEntries.ElementAt(i);
                        int index = policySwitchEntry.IndexOf('='); if (index < 0) continue;
                        string psePolName = policySwitchEntry.Substring(0, index);
                        if (!EM_Helpers.DoesValueMatchPattern(psePolName, polRow.Name)) continue; //e.g. does 'BTA_??' match 'BTA_uk'(yes->relevant) or does 'TCA_??' match 'BTA_uk'(no->irrelevant)

                        //deactivate the switching-off in the add-on and accomplish the switching in the 'normal way', i.e. via the EMConfig
                        if (policySwitchEntry.EndsWith("=on")) policySwitchEntries[i] = policySwitchEntry.Substring(0, policySwitchEntry.Length - 2) + "off";
                        switchedOffRow.Value = DefPar.Value.NA; //this deactivates the switching-off in the add-on

                        // Note that (the perhaps more harmless) deleting of the EMConfig-entry is not possible because things work as follows:
                        // - To the executable 'switch' in first instance means 'off'.
                        // - Then it gets instructions from the EMConfig whether this is actually 'on' or 'off'; no instruction: still 'off'.
                        // - Then an add-on might come and say it should be 'off' (by means of a respective ChangeParam function).
                        //   (a) This is fine if it was 'on' due to the EMConfig-setting
                        //       -> exe does the off-switching.
                        //   (b) This is not fine if it was 'off' due to the EMConfig-setting (or because there isn't any EMConfig-setting)
                        //       -> exe complains that it can't change switches of 'off'-policies (though they change would be from off to off).
                        // Thus removing the Config-entry would land in (b).
                        // Moreover, the treatment above reinstalls "usual handling", thus things are more or less corrected.

                        concernedSwitchPols.Add(psePolName); //for possibly inform user (see below)
                    }
                }

                //check if the respective switch-columns are visible in run-mainform, in order to issue an info (i.e. optional warning) if so
                //here just prepare the info, to be pooled above in order to avoid x warnings for e.g. starting several MTR-calculations
                string info = string.Empty;
                foreach (string switchPol in concernedSwitchPols) if (runMainForm.IsSwitchColumnDisplayed(switchPol)) info += switchPol + " ";
                if (info != string.Empty)
                    warningInfos += string.Format("Country '{0}', Add-on-system: '{1}': Switchalbe Policies: {2}",
                        country._shortName.ToUpper(), addOnName.ToUpper(), info) + Environment.NewLine;
            }
            catch { } //do not jeopardise anything else because of this rather unimportant function
        }

        bool GenerateEMConfigs(string outputPath, string emVersion, Dictionary<ConfigKey, Dictionary<string, string>> contentEMConfigs, RunMainForm runMainForm)
        {
            string exRateError = string.Empty; string hiddenExtensionsWarning = string.Empty;
            foreach (DataGridViewRow systemRow in runMainForm.dgvRun.Rows) //run over systems
            {
                //assess whether any run-checkboxes (standard run, add-ons) are checked for the system
                bool systemSelected = EM_Helpers.SaveConvertToBoolean(systemRow.Cells[runMainForm.colRun.Name].Value);
                bool addOnSelected = false;
                foreach (DataGridViewColumn column in runMainForm.dgvRun.Columns)
                    if (column.Name.StartsWith(RunMainForm._colAddOnPrefix) && EM_Helpers.SaveConvertToBoolean(systemRow.Cells[column.Name].Value))
                        addOnSelected = true;

                if (systemSelected == false && addOnSelected == false)
                    continue; //system not selected for run

                //assess which dataset is selected for run
                DataConfig.DBSystemConfigRow dbSystemConfigRow = runMainForm.GetSelectedDBSystemCombination(systemRow);
                if (dbSystemConfigRow == null)
                    continue; //should not happen

                DataConfig.DataBaseRow dataBaseRow = dbSystemConfigRow.DataBaseRow;

                //save country settings if there are unsaved changes
                string countryShortName = systemRow.Cells[runMainForm.colCountry.Name].Value.ToString();
                if (EM_AppContext.Instance.WriteXml(countryShortName, true, true) == DialogResult.Cancel)
                    return false;
                runMainForm.Cursor = Cursors.WaitCursor; // the writing sets the cursor back to normal

                Dictionary<string, string> contentEMConfig = null; //prepare a list with entries for the EMConfig_XXX.xml file (to be actually written once all systems are collected)
                string outputPathIfDoNotPoolSystemsDatasets = string.Empty;
                if (runMainForm._doNotPoolSystemsDatasets) //add a folder for each dataset, if system-dataset-combinations are displayed separately
                {
                    outputPathIfDoNotPoolSystemsDatasets = EMPath.AddSlash(outputPath) + dataBaseRow.Name;
                    if (!Directory.Exists(outputPathIfDoNotPoolSystemsDatasets))
                        Directory.CreateDirectory(outputPathIfDoNotPoolSystemsDatasets);
                    outputPathIfDoNotPoolSystemsDatasets = EMPath.AddSlash(outputPathIfDoNotPoolSystemsDatasets);
                }

                contentEMConfig = new Dictionary<string, string>();
                contentEMConfigs.Add(new ConfigKey(countryShortName, dataBaseRow.ID, dbSystemConfigRow.SystemID), contentEMConfig); //an own EMConfig is needed for the system if it is to be run separately

                //fill EMConfig-entry-list
                string dateTimePrefix = string.Format("{0:yyyyMMddHHmm}", DateTime.Now);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_ERRLOG_FILE,
                    (runMainForm._doNotPoolSystemsDatasets ? outputPathIfDoNotPoolSystemsDatasets : outputPath)
                    + dateTimePrefix + EM_XmlHandler.TAGS.EM2CONFIG_errLogFileName);
                if (EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(RunMainForm._doNotStopOnNonCriticalErrors))
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LOG_WARNINGS, DefPar.Value.YES);
                else
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LOG_WARNINGS, DefPar.Value.NO);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_EMVERSION, emVersion);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_UIVERSION, DefGeneral.UI_VERSION);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_PARAMPATH, EMPath.AddSlash(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + countryShortName));
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_CONFIGPATH, EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles));
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_OUTPUTPATH, runMainForm._doNotPoolSystemsDatasets ? outputPathIfDoNotPoolSystemsDatasets : outputPath);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DATAPATH, EM_AppContext.FolderInput);
                string executablePath = EnvironmentInfo.GetEM2ExecutableFile();
                if (IsFileYoungerThan(executablePath, new DateTime(2013, 6, 1))) //to avoid problems with an executable that does not allow for this new tag (should not be necessary, as UI and Exe should be boundled)
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_EMCONTENTPATH, EM_AppContext.FolderEuromodFiles);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_HEADER_DATE, dateTimePrefix);
                if (!EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(RunMainForm._addDateToOuputFilename))
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_OUTFILE_DATE, "-");
                else
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_OUTFILE_DATE, dateTimePrefix);
                if (!EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(RunMainForm._logRuntimeInDetail))
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LOG_RUNTIME, DefPar.Value.NO);
                else
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LOG_RUNTIME, DefPar.Value.YES);
                if (EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(RunMainForm._warnAboutUselessGroups))
                    contentEMConfig.Add(EM_XmlHandler.TAGS.CONFIG_WARN_ABOUT_USELESS_GROUPS, DefPar.Value.YES);
                if (EM_AppContext.Instance.IsPublicVersion())
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_ISPUBLICVERSION, DefPar.Value.YES);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DECSIGN_PARAM, EM_Helpers.uiDecimalSeparator);
                if (runMainForm.runPublicOnly())
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_IGNORE_PRIVATE, DefPar.Value.YES);
                string numberOfHHToRun; runMainForm.GetNumberOfHHToRun(out numberOfHHToRun);
                if (numberOfHHToRun != string.Empty) contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_FIRST_N_HH_ONLY, numberOfHHToRun);
                string startHH = EM_XmlHandler.TAGS.EM2CONFIG_defaultHHID;
                string lastHH = EM_XmlHandler.TAGS.EM2CONFIG_defaultHHID;
                if (EM_AppContext.Instance.GetUserSettingsAdministrator().GetRunFormSettings().Contains(RunMainForm._showSelectedHHOptions))
                {
                    startHH = systemRow.Cells[runMainForm.colFirstHH.Name].Value == null ? string.Empty : systemRow.Cells[runMainForm.colFirstHH.Name].Value.ToString();
                    if (startHH == string.Empty)
                        startHH = EM_XmlHandler.TAGS.EM2CONFIG_defaultHHID;
                    else if (!EM_Helpers.IsNonNegInteger(startHH))
                    {
                        Tools.UserInfoHandler.ShowError(dbSystemConfigRow.SystemName + ": " + runMainForm.colFirstHH.HeaderText + " is not a valid number.");
                        return false;
                    }
                    lastHH = systemRow.Cells[runMainForm.colLastHH.Name].Value == null ? string.Empty : systemRow.Cells[runMainForm.colLastHH.Name].Value.ToString();
                    if (lastHH == string.Empty)
                        lastHH = EM_XmlHandler.TAGS.EM2CONFIG_defaultHHID;
                    else if (!EM_Helpers.IsNonNegInteger(lastHH))
                    {
                        Tools.UserInfoHandler.ShowError(dbSystemConfigRow.SystemName + ": " + runMainForm.colLastHH.HeaderText + " is not a valid number.");
                        return false;
                    }
                }
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_STARTHH, startHH);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LASTHH, lastHH);
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_COUNTRY_FILE, CountryAdministrator.GetCountryFileName(countryShortName));
                if (EM_Helpers.SaveConvertToBoolean(runMainForm.chkOutputEuro.EditValue))
                {
                    string exRateDate = runMainForm.cmbExRateDate.EditValue.ToString();
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_EXCHANGE_RATE_DATE, exRateDate);
                    CheckExRateAvailability(ref exRateError, exRateDate, countryShortName, dbSystemConfigRow.SystemID);
                }
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DATACONFIG_FILE, CountryAdministrator.GetDataFileName(countryShortName));
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DATASET_ID, dbSystemConfigRow.DataBaseID);
                contentEMConfig.Add(RunMainForm._labelRunFormInfoText, dataBaseRow.Name + " ( "); //text that is displayed in the window showing run status: put into list temporarily

                if (systemSelected || !_runEM2) //ordinary system selected (add-on-systems are added in function AddAddOnsToEMConfigs)
                {
                    contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID + Guid.NewGuid().ToString(), dbSystemConfigRow.SystemID); //Guid is removed again below, just needed because dictionary cannot have two entries with the same key
                    contentEMConfig[RunMainForm._labelRunFormInfoText] += dbSystemConfigRow.SystemName + " "; //add to text that is displayed in the window showing run status

                    //for each (available) switchable policy add a POLICY_SWITCH-entry
                    List<string> policySwitchEntries = GenerateConfigEntriesPolicySwitches(runMainForm, systemRow, dbSystemConfigRow.SystemID, dbSystemConfigRow, _runEM2, out string hiddenWarning);
                    hiddenExtensionsWarning += hiddenWarning;
                    foreach (string policySwitchEntry in policySwitchEntries)
                        contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH + Guid.NewGuid().ToString(), policySwitchEntry);
                }
            }

            if (hiddenExtensionsWarning != string.Empty)
            {
                if (UserInfoHandler.GetInfo("Note that for not displayed extensions default settings are used!" + Environment.NewLine + Environment.NewLine + hiddenExtensionsWarning,
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel) return false;
            }               

            if (!contentEMConfigs.Any())
            {
                UserInfoHandler.ShowError("Please select configurations to run.");
                return false;
            }

            if (exRateError != string.Empty)
                return UserInfoHandler.GetInfo("Exchange Rate Info is insufficient:" + Environment.NewLine + exRateError + Environment.NewLine +
                                                     "Default settings are used instead.", MessageBoxButtons.OKCancel) == DialogResult.OK;
            return true;
        }

        void CheckExRateAvailability(ref string exRateError, string exRateDate, string countryShortName, string systemID)
        {
            if (exRateDate == "Default") return; // nothing to check, works even if global exchange rate table does not exist

            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(countryShortName);
            CountryConfig.SystemRow systemRow = ccf.GetSystemRowByID(systemID);
            if (systemRow.CurrencyParam == DefPar.Value.EURO) return;

            ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
            if (excf == null) { if (exRateError == string.Empty) exRateError = "Global Exchange Rate Table does not yet exist." + Environment.NewLine; return; }
            ExchangeRatesConfig.ExchangeRatesRow exRate = null;
            foreach (ExchangeRatesConfig.ExchangeRatesRow er in excf.GetExchangeRates(countryShortName))
                if (ExchangeRate.ValidForToList(er.ValidFor).Contains(systemRow.Name.ToLower())) { exRate = er; break; }
            if (exRate == null) { exRateError += "Global Exchange Rate Table does not contain a rate for system '" + systemRow.Name + "'" + Environment.NewLine; return; }
            bool ok = true;
            switch (exRateDate)
            {
                case ExchangeRate.JUNE30: if (exRate.June30 == -1) ok = false; break;
                case ExchangeRate.YEARAVERAGE: if (exRate.YearAverage == -1) ok = false; break;
                case ExchangeRate.FIRSTSEMESTER: if (exRate.FirstSemester == -1) ok = false; break;
                case ExchangeRate.SECONDSEMESTER: if (exRate.SecondSemester == -1) ok = false; break;
            }
            if (!ok) exRateError += "Global Exchange Rate Table does not contain a '" + exRateDate + "' rate for system '" + systemRow.Name + "'" + Environment.NewLine;
        }

        static void addChangesSwitchesInfoToOutputFilename(CountryConfig.SystemRow sr, string suffix)
        {
            try
            {
                CountryConfig.ParameterRow row = sr.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_" + sr.CountryRow.ShortName).ToLower())
                    .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput")
                    .GetParameterRows().First(p => p.Name.ToLower() == "file");
                if (row.Value.EndsWith("_std")) row.Value = row.Value.Substring(0, row.Value.Length-4);
                row.Value += suffix;

                row = sr.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_hh_" + sr.CountryRow.ShortName).ToLower())
                    .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput")
                    .GetParameterRows().First(p => p.Name.ToLower() == "file");
                if (row.Value.EndsWith("_std_hh")) row.Value = row.Value.Substring(0, row.Value.Length-7) + "_hh";
                row.Value += suffix;
            }
            catch
            { }
        }

        static string GenerateSwitchRenamingSuffix(RunMainForm runMainForm, DataGridViewRow systemRow, DataConfig.DBSystemConfigRow dbSystemConfigRow)
        {
            string suffix = string.Empty;

            foreach (Tuple<string, string, string> e in runMainForm.GetAllExtensions())
            {
                string extID = e.Item1, extLongName = e.Item2, extShortName = e.Item3;

                //first check whether the check-box-column for the switchable policy is displayed ...
                string policySwitchValue = string.Empty;
                foreach (DataGridViewColumn column in runMainForm.dgvRun.Columns)
                {
                    if (column.Name.StartsWith(RunMainForm._colPolicySwitchPrefix) &&
                        column.Name.Substring(RunMainForm._colPolicySwitchPrefix.Length) == extID)
                        //... if yes use the value defined (or not redefined, but visible) by the user
                        policySwitchValue = systemRow.Cells[column.Name].Tag.ToString(); //(the cae of n/a value is handeled below)
                }

                // if column not displayed, or switch value is empty, or value is the default, move on to the next switch...
                if (policySwitchValue == string.Empty || policySwitchValue.EndsWith(RunMainForm._policySwitchDefaultValueIndicator) || 
                    policySwitchValue == ExtensionAndGroupManager.GetExtensionDefaultSwitch(dbSystemConfigRow, extID)) continue;

                // else add switch description to the suffix
                string switchName = extShortName.Replace("?", "").Replace("*", "").TrimEnd(new char[] { '_' });
                suffix += "_" + switchName + policySwitchValue.Replace("/", ""); 
            }
            return suffix;
        }

        static List<string> GenerateConfigEntriesPolicySwitches(RunMainForm runMainForm, DataGridViewRow systemRow, string systemID, DataConfig.DBSystemConfigRow dbSystemConfigRow, bool runEM2, out string hiddenWarning)
        {
            List<string> policySwitchEntries = new List<string>();

            //generate for each (available) extension a POLICY_SWITCH-entry
            hiddenWarning = string.Empty;
            foreach (Tuple<string, string, string> e in runMainForm.GetAllExtensions())
            {
                string extID = e.Item1, extLongName = e.Item2, extShortName = e.Item3;

                //first check whether the check-box-column for the switchable policy is displayed ...
                string policySwitchValue = string.Empty; bool runAll = false;
                foreach (DataGridViewColumn column in runMainForm.dgvRun.Columns)
                    if (column.Name.StartsWith(RunMainForm._colPolicySwitchPrefix) &&
                        column.Name.Substring(RunMainForm._colPolicySwitchPrefix.Length) == extID)
                    { //... if yes use the value defined (or not redefined, but visible) by the user
                        policySwitchValue = systemRow.Cells[column.Name].Tag.ToString(); //(the case of n/a value is handeled below)
                        runAll = !runEM2 && systemRow.Cells[column.Name].Value.ToString() == DefPar.Value.EXTENSION_ALL;// assess if the 'all' option is selected (this is not stored in the tag, but only recognisable in the cell-value)
                    }

                //if the check-box-column for the switchable policy is not displayed ...
                if (policySwitchValue == string.Empty)
                {
                    //... get the value set in the dialog in the country settings (ignore user-settings)
                    policySwitchValue = ExtensionAndGroupManager.GetExtensionDefaultSwitch(dbSystemConfigRow, extID);

                    var hidden = runMainForm.GetPolicySwitchValueAndDisplayValue(dbSystemConfigRow, extID);
                    if (!string.IsNullOrEmpty(hidden.Value) && hidden.Value != DefPar.Value.NA && !hidden.Value.Contains("default"))
                        hiddenWarning += $"Not displayed extension '{extLongName}' is set to {hidden.Key}, while used default is {policySwitchValue}." + Environment.NewLine;
                }
                //generate the POLICY_SWITCH-entry
                //taking into account that there is no need to generate it if the switch is set to n/a (i.e. the switchable policy is not switchable for this db-system-combination or does not even exist)
                if (policySwitchValue != string.Empty && policySwitchValue != DefPar.Value.NA)
                {
                    if (policySwitchValue.EndsWith(RunMainForm._policySwitchDefaultValueIndicator))
                        policySwitchValue = policySwitchValue.Substring(0, policySwitchValue.Length - RunMainForm._policySwitchDefaultValueIndicator.Length);
                    string policySwitchEntry = //the executable needs three pieces of information to overwrite the default value of the policy switch:
                        extShortName //which switchable policy (e.g. BTA_??)
                        + "=" + (runEM2 ? systemID : extID) //for EM2: which system, for EM3 (no need for system) the extension-id
                        + "=" + (runAll ? DefPar.Value.EXTENSION_ALL : policySwitchValue); //and the respective value: usually: on or off; special case: all, if the all-option is selected (to be replaced in 'KickOffRuns')
                    policySwitchEntries.Add(policySwitchEntry);
                }
            }
            if (hiddenWarning != string.Empty) hiddenWarning = $"{dbSystemConfigRow.SystemName}/{dbSystemConfigRow.DataBaseRow.Name}:" + Environment.NewLine + hiddenWarning + Environment.NewLine;

            //foreach (DataGridViewColumn column in runMainForm.dgvRun.Columns)
            //{
            //    if (!column.Name.StartsWith(RunMainForm._colPolicySwitchPraefix))
            //        continue;
            //    string policySwitchID = column.Name.Substring(RunMainForm._colPolicySwitchPraefix.Length);
            //    string policySwitchValue = systemRow.Cells[column.Name].Tag.ToString();
            //    if (policySwitchValue != DefPar.Value.NA)
            //        //executable needs three pieces of information to overwrite the default value of the policy switch:
            //        //which switchable policy (e.g. yem_??), which system (ID) and the redefined value (on or off)
            //        policySwitchEntries.Add(EM_AppContext.Instance.GetVarConfigFacade().GetSwitchablePolicy(policySwitchID).NamePattern + "=" + systemID + "=" + policySwitchValue);
            //}

            return policySwitchEntries;
        }

        bool IsFileYoungerThan(string filePath, DateTime date)
        {
            try
            {
                if (File.GetLastWriteTime(filePath) > date)
                    return true;
            }
            catch (Exception exception)
            {
                //do nothing
                UserInfoHandler.RecordIgnoredException("RunManager.IsFileYoungerThan", exception);
            }
            return false;
        }

        // new feature (Aug 18): if 'Do not pool system's datasets' option is checked, allow for opening the respective output folder after run has finished
        const string BUTTON_TEXT_OPEN = "Open";
        private void DoNotPoolHandling_Prepare(Run run)
        {
            bool isActive = run._processStatus == Run._processStatus_Running;
            if (!isActive && run.HasRunLog()) // if run is finished rename 'Run Log' button to 'Open' ...
                _runInfoForm.SetButtonStatus(RunInfoForm._colShowRunLog, run._rowInRunForm, true, BUTTON_TEXT_OPEN);
            else // ... otherwise, usual behaviour (copied from above)
                _runInfoForm.SetButtonStatus(RunInfoForm._colShowRunLog, run._rowInRunForm, isActive && run.HasRunLog());
        }
        // show an open-file-dialog that allows opening the file produced by this run, by opening the repective folder,
        // which is the folder for the used data-set (with the option 'Do not pool system's datasets' files are orgnised in folders per dataset)
        private void DoNotPoolHandling_Open(Run run)
        {   
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = run._contentConfig[EM_XmlHandler.TAGS.EM2CONFIG_OUTPUTPATH];
            openFileDialog.ShowDialog();
        }
    }
}
