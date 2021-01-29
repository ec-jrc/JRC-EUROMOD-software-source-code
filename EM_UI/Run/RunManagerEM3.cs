using EM_Common;
using EM_Transformer;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ImportExport;
using EM_UI.Tools;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Run
{
    internal partial class RunManager
    {
        internal bool _runEM2 = false;
        private string _outputPath = string.Empty;

        private List<string> countriesToTransform, addOnsToTransform;

        // this does not only add the addOn-info, but also collects the country- and addOn-file that need to be transformed to EM3 format
        bool EM3_AddAddOnInfo(string outputPath, Dictionary<ConfigKey, Dictionary<string, string>> contentEMConfigs, RunMainForm runMainForm)
        {
            addOnsToTransform = new List<string>(); countriesToTransform = new List<string>(); // for collecting country- and addon-files which need to be transformed to EM3

            foreach (DataGridViewRow systemRow in runMainForm.dgvRun.Rows) // loop over systems
            {
                // assess whether any add-on is selected for run (theoretically more than one add-on can be selected for a system)
                List<AddOnSystemInfo> selectedAddOns = new List<AddOnSystemInfo>();
                foreach (DataGridViewColumn column in runMainForm.dgvRun.Columns)
                    if (column.Name.StartsWith(RunMainForm._colAddOnPrefix) && EM_Helpers.SaveConvertToBoolean(systemRow.Cells[column.Name].Value))
                    {
                        if (!AddOnInfoHelper.GetSupportedSystemInfo(systemRow.Cells[runMainForm.colSystem.Name].Value.ToString(),
                            column.Tag as List<AddOnSystemInfo>, out List<AddOnSystemInfo> supportedAOSystemInfo)) return false;
                        selectedAddOns.AddRange(supportedAOSystemInfo);
                    }

                // we need to know whether the system is selected, first to collect the country for transformation (see remark above)
                // and, if it is selected in addition to any add-on(s), we need to copy the configuration file (instead of just adapting it)
                bool isSystemSelected = EM_Helpers.SaveConvertToBoolean(systemRow.Cells[runMainForm.colRun.Name].Value);
                string countryShortName = systemRow.Cells[runMainForm.colCountry.Name].Value.ToString().ToLower();
                if (isSystemSelected || selectedAddOns.Any())
                {
                    string countryToTransform = countryShortName;
                    if (!countriesToTransform.Contains(countryToTransform)) countriesToTransform.Add(countryToTransform);
                }

                if (!selectedAddOns.Any()) continue;

                // to find the config (prepared in GenerateEMConfigs) we need the dataset
                DataConfig.DBSystemConfigRow dbSystemConfigRow = runMainForm.GetSelectedDBSystemCombination(systemRow); if (dbSystemConfigRow == null) continue; // should not happen
                ConfigKey key = new ConfigKey(countryShortName, dbSystemConfigRow.DataBaseID, dbSystemConfigRow.SystemID);
                if (!contentEMConfigs.ContainsKey(key)) continue; // should not happen
                Dictionary<string, string> contentEMConfig = contentEMConfigs[key];

                // if the system itself is selected too, we need to make a copy of the config (i.e. run the plain system and run the add-on)
                if (isSystemSelected)
                {
                    contentEMConfig = new Dictionary<string, string>();
                    foreach (var entry in contentEMConfigs[key]) contentEMConfig.Add(entry.Key, entry.Value);
                    ConfigKey someKey = new ConfigKey(Guid.NewGuid().ToString(), string.Empty, string.Empty); // the key is irrelevant
                    contentEMConfigs.Add(someKey, contentEMConfig);
                }

                int cnt = 0;
                foreach (AddOnSystemInfo addOnSystemInfo in selectedAddOns) // loop over the add-ons which are selected for run (usually one)
                {
                    contentEMConfig[RunMainForm._labelRunFormInfoText] += addOnSystemInfo._addOnSystemName + " "; // add to text that is displayed in the window showing run status
                    // add: $"ADDON{cnt}", $"{addOn}|{addOnSys}"
                    contentEMConfig.Add(TAGS.EM2CONFIG_ADDON + (cnt++).ToString(), addOnSystemInfo._addOnShortName + "|" + addOnSystemInfo._addOnSystemName);

                    string addOnToTransform = addOnSystemInfo._addOnShortName.ToLower();
                    if (!addOnsToTransform.Contains(addOnToTransform))
                    {
                        EM_AppContext.Instance.WriteXml(addOnSystemInfo._addOnShortName, false, false); // make sure the EM2 version is saved
                        addOnsToTransform.Add(addOnToTransform);
                    }
                }
            }
            return true;
        }

        private static object transformLock = new object();

        private bool EM3_Transform()
        {
            try
            {
                lock (transformLock)
                {
                    string transformerErrors = string.Empty;

                    foreach (string country in countriesToTransform)
                    {
                        EM3Country.Transform(EM_AppContext.FolderEuromodFiles, country, out List<string> errors);
                        foreach (string error in errors) transformerErrors += error + Environment.NewLine;
                    }
                    foreach (string addOn in addOnsToTransform)
                    {
                        EM3Country.TransformAddOn(EM_AppContext.FolderEuromodFiles, addOn, out List<string> errors);
                        foreach (string error in errors) transformerErrors += error + Environment.NewLine;
                    }

                    EM3Global.Transform(EM_AppContext.FolderEuromodFiles, out List<string> gErrors);
                    foreach (string error in gErrors) transformerErrors += error + Environment.NewLine;

                    EM3Variables.Transform(EM_AppContext.FolderEuromodFiles, out List<string> vErrors);
                    foreach (string error in vErrors) transformerErrors += error + Environment.NewLine;

                    if (transformerErrors == string.Empty) return true;
                    return UserInfoHandler.GetInfo("Errors on transforming to EM3 structure:" + Environment.NewLine + transformerErrors + Environment.NewLine +
                                                   "Do you want to continue?", MessageBoxButtons.YesNo) == DialogResult.Yes;
                }
            }
            catch (Exception exception)
            {
                return UserInfoHandler.GetInfo("Errors on transforming to EM3 structure:" + Environment.NewLine + exception.Message +
                                                Environment.NewLine + Environment.NewLine + "Do you want to continue?",
                                                MessageBoxButtons.YesNo) == DialogResult.Yes;
            }
        }

        private void EM3_WriteLog()
        {
            List<RunLogger.RunInfo> runInfoList = new List<RunLogger.RunInfo>();
            foreach (Run run in _runs)
            {
                // some info needs to be looked up in country-files
                string country = run._contentConfig[TAGS.EM2CONFIG_COUNTRY_FILE];
                if (country.ToLower().EndsWith(".xml")) country = country.Substring(0, country.Length - 4);
                CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
                DataConfigFacade dcf = CountryAdministrator.GetDataConfigFacade(country);
                string sysID = (from e in run._contentConfig where e.Key.StartsWith(TAGS.EM2CONFIG_SYSTEM_ID) select e.Value).FirstOrDefault(); // SYSTEM_ID is followed by a GUID
                string dataID = run._contentConfig[TAGS.EM2CONFIG_DATASET_ID];

                if (!EM_AppContext.Instance._runExeViaLib && run._process != null)
                {
                    run.em3RunInfo.duration = new RunLogger.Duration(run._process.StartTime, run._process.ExitTime);
                    run.em3RunInfo.finishStatus = run._processStatus == Run._processStatus_Finished ? RunLogger.RunInfo.FINISH_STATUS.finished : RunLogger.RunInfo.FINISH_STATUS.aborted;
                }
                run.em3RunInfo.systemName = ccf.GetSystemRowByID(sysID).Name;
                run.em3RunInfo.ExtractAddonSystemNames(run._contentConfig, TAGS.EM2CONFIG_ADDON);
                run.em3RunInfo.databaseName = dcf.GetDataBaseRow(dataID).Name;
                run.em3RunInfo.currency = run._contentConfig.ContainsKey(TAGS.EM2CONFIG_EXCHANGE_RATE_DATE) ? DefPar.Value.EURO
                         : ccf.GetSystemRowByID(sysID).CurrencyOutput; // config contains entry EXCHANGE_RATE_DATE only if "All Output in €" is checked
                run.em3RunInfo.exchangeRate = GetExchangeRate(run, country, ccf.GetSystemRowByID(sysID).Name);
                run.em3RunInfo.ExtractExtensionSwitches(run._contentConfig, TAGS.EM2CONFIG_POLICY_SWITCH);
                string runOutputPath = run._contentConfig.GetOrEmpty(TAGS.EM2CONFIG_OUTPUTPATH);
                if (!string.IsNullOrEmpty(runOutputPath) && !EMPath.IsSamePath(runOutputPath, _outputPath)) run.em3RunInfo.nonDefaultOutputPath = runOutputPath;
                runInfoList.Add(run.em3RunInfo);
            }

            new RunLogger(EM_AppContext.Instance.GetProjectName(), runInfoList).TxtWriteEMLog(_outputPath);

            string GetExchangeRate(Run run, string country, string sysName)
            {
                ExchangeRatesConfigFacade excf = EM_AppContext.Instance.GetExchangeRatesConfigFacade(false);
                if (excf == null) return Dialogs.ConfigureSystemsForm.DEFAULT_EXCHANGE_RATE;
                foreach (ExchangeRatesConfig.ExchangeRatesRow exRate in excf.GetExchangeRates(country))
                {
                    if (!exRate.ValidFor.ToLower().Trim().EndsWith(sysName.ToLower()) && !exRate.ValidFor.ToLower().Contains(sysName.ToLower() + ",")) continue;
                    string date = run._contentConfig.ContainsKey(TAGS.EM2CONFIG_EXCHANGE_RATE_DATE) &&
                                  run._contentConfig[TAGS.EM2CONFIG_EXCHANGE_RATE_DATE].ToLower() != "default"
                        ? run._contentConfig[TAGS.EM2CONFIG_EXCHANGE_RATE_DATE].ToLower() : exRate.Default.ToLower();
                    double value = 1.0;
                    if (date.StartsWith("june")) value = exRate.June30;
                    else if (date.StartsWith("year")) value = exRate.YearAverage;
                    else if (date.StartsWith("first")) value = exRate.FirstSemester;
                    else if (date.StartsWith("second")) value = exRate.SecondSemester;
                    return value.ToString();
                }
                return Dialogs.ConfigureSystemsForm.DEFAULT_EXCHANGE_RATE;
            }
        }

        private static bool GetExtensionInfo(Dictionary<string, string> contentConfig, out string info) // for text in run-tool
        {
            info = string.Empty; // extract exentsion(aka policy-switch)-info from configuration-file
            foreach (string entry in from e in contentConfig where e.Key.StartsWith(TAGS.EM2CONFIG_POLICY_SWITCH) select e.Value)
            {
                string[] split = entry.Split('='); if (split.Length != 3) continue; // example: BTA_??=c5ec590d-9c51-4b88-b482-0a5cb514a880=off
                info += split[0] + "=" + split[2] + ";";
            }
            info.TrimEnd(new char[] { ';' });
            return info != string.Empty;
        }

        // if required, rename output-files to indicate if switches are set to non-default values
        bool EM3_AddNonDefaultSwitches_To_OutputFileName(Dictionary<ConfigKey, Dictionary<string, string>> contentEMConfigs, RunMainForm runMainForm)
        {
            try
            {
                // check if automatic renaming of the output-filenames is switched on
                if (!EM_UI.EM_AppContext.Instance.GetUserSettingsAdministrator().GetPolicySwitchAutoRenameValue()) return true;

                foreach (DataGridViewRow systemRow in runMainForm.dgvRun.Rows) // loop over systems
                {
                    // assess whether any run-checkboxes (standard run, add-ons) are checked for the system
                    if (!EM_Helpers.SaveConvertToBoolean(systemRow.Cells[runMainForm.colRun.Name].Value)) continue;

                    // assess which dataset is selected for run
                    DataConfig.DBSystemConfigRow dbSystemConfigRow = runMainForm.GetSelectedDBSystemCombination(systemRow);
                    if (dbSystemConfigRow == null) continue; // should not happen

                    // find the EMConfig (prepared in GenerateEMConfigs) where the output needs to be renamed
                    Dictionary<string, string> contentEMConfig = null;
                    ConfigKey key = new ConfigKey(systemRow.Cells[runMainForm.colCountry.Name].Value.ToString(), dbSystemConfigRow.DataBaseID, dbSystemConfigRow.SystemID);
                    if (contentEMConfigs.ContainsKey(key)) contentEMConfig = contentEMConfigs[key];
                    if (contentEMConfig == null) continue; // should not happen

                    // instruct the executable to apply this suffix to standard output (i.e. concerns only DefOutputs in policies output_std and output_std_hh):
                    // specificly, change e.g. mt_2014_std to mt_2014_UAAon and/or mt_2014_std_hh to mt_2014_UAAon_hh
                    string renameString = GenerateSwitchRenamingSuffix(runMainForm, systemRow, dbSystemConfigRow);
                    if (renameString != string.Empty) contentEMConfig.Add(TAGS.STDOUTPUT_FILENAME_SUFFIX, renameString);
                }
            }
            catch { }
            return true;
        }
    }
}
