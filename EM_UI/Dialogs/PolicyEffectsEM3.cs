using EM_Common;
using EM_Transformer;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ImportExport;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class PolicyEffects : Form
    {
        private RunLogger.PetInfo em3_petInfo = new RunLogger.PetInfo();
        private List<RunLogger.RunInfo> em3_runInfoList = new List<RunLogger.RunInfo>();
        private bool em3_transformGlobals = true;
        private static object transformLock = new object();

        private void EM3_Transform(string country, string addOn)
        {
            try
            {
                EMPath emPath = new EMPath(EM_AppContext.FolderEuromodFiles);
                lock (transformLock)
                {
                    bool success = TransformCountry() && TransformAddOn() &&
                                   (em3_transformGlobals == false || (TransformGlobals() && TransformVariables()));
                }
                bool TransformCountry()
                {
                    bool ok = EM3Country.Transform(emPath.GetFolderEuromodFiles(), country, out List<string> transErrors);
                    AddErrorRange(transErrors); return ok;
                }
                bool TransformGlobals()
                {
                    bool ok = EM3Global.Transform(emPath.GetFolderEuromodFiles(), out List<string> transErrors);
                    AddErrorRange(transErrors); return ok;
                }
                bool TransformVariables()
                {
                    bool ok = EM3Variables.Transform(emPath.GetFolderEuromodFiles(), out List<string> transErrors);
                    AddErrorRange(transErrors); return ok;
                }
                bool TransformAddOn()
                {
                    if (string.IsNullOrEmpty(addOn)) return true;
                    bool ok = EM3Country.TransformAddOn(emPath.GetFolderEuromodFiles(), addOn, out List<string> transErrors);
                    AddErrorRange(transErrors); return ok;
                }
            }
            catch (Exception exception) { em3_petInfo.AddSystemIndependentError(exception.Message); }

            void AddErrorRange(List<string> errors) { foreach (string error in errors) em3_petInfo.AddSystemIndependentError(error); }
        }

        private bool EM3_Run(SystemBackgroundWorker sbw)
        {
            sbw.em3_RunInfo = new RunLogger.RunInfo(); DateTime startTime = DateTime.Now;

            bool success = new EM_Executable.Control().Run(sbw.config,
                progressInfo =>
                {
                    if (sbw.em3_Cancel) return false;
                    sbw.em3_RunInfo.ExtractOutputFiles(progressInfo.detailedInfo);
                    return true;
                },
                errorInfo => { if (!IsParModError(errorInfo.message)) sbw.em3_RunInfo.errorInfo.Add(errorInfo); });

            sbw.em3_RunInfo.duration = new RunLogger.Duration(startTime, DateTime.Now);
            sbw.em3_RunInfo.finishStatus = success ? RunLogger.RunInfo.FINISH_STATUS.finished : RunLogger.RunInfo.FINISH_STATUS.aborted;
            sbw.em3_RunInfo.ExtractAddonSystemNames(sbw.config, TAGS.CONFIG_ADDON);
            sbw.em3_RunInfo.extensionSwitches.Add("All Extension Switches", "default-settings"); // todo: change once modifying extension-switches is possible
            return success;

            // this error may occur by trying to modify a parameter that is not available due to extension-settings
            bool IsParModError(string err) { return err.Contains("not found (modification is ignored)"); }
        }

        private Dictionary<string, string> EM3_CreateConfig(string countryShortName, string outputPath,
                                                            DataConfig.DataBaseRow dbr, CountryConfig.SystemRow sr,
                                                            string parModifications = null)
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            config.Add(TAGS.CONFIG_PATH_OUTPUT, outputPath);
            config.Add(TAGS.CONFIG_PATH_DATA, EM_AppContext.FolderInput);
            config.Add(TAGS.CONFIG_PATH_EUROMODFILES, EM_AppContext.FolderEuromodFiles);
            config.Add(TAGS.CONFIG_COUNTRY, CountryAdministrator.GetCountryFileName(countryShortName).ToLower().Replace(".xml", ""));
            config.Add(TAGS.CONFIG_ID_DATA, dbr.ID);
            config.Add(TAGS.CONFIG_ID_SYSTEM, sr.ID);
            if (textRunFirstNHH.Text != null && !string.IsNullOrEmpty(textRunFirstNHH.Text) && int.TryParse(textRunFirstNHH.Text, out _))
                config.Add(TAGS.CONFIG_FIRST_N_HH_ONLY, textRunFirstNHH.Text);
            if (parModifications != null) config.Add(TAGS.CONFIG_STRING_PAR_MODIFICATIONS, parModifications);

            string addOn = GetCheckedAddon();
            if (!string.IsNullOrEmpty(addOn))
            {
                string addOnSys = null;
                foreach (AddOnSystemInfo aos in AddOnInfoHelper.GetAddOnSystemInfo(addOn))
                    foreach (string sysPattern in aos._supportedSystems)
                        if (EM_Helpers.DoesValueMatchPattern(sysPattern, sr.Name))
                            { addOnSys = aos._addOnSystemName; break; }
                if (addOnSys == null) throw new Exception($"No matching Addon found for {sr.Name}");
                config.Add(TAGS.CONFIG_ADDON, $"{addOn}|{addOnSys}");
            }
            return config;
        }

        private void EM3_AddToRunInfoList(SystemBackgroundWorker sbw)
        {
            sbw.em3_RunInfo.systemName = sbw.systemName;
            sbw.em3_RunInfo.databaseName = sbw.databaseName;
            em3_runInfoList.Add(sbw.em3_RunInfo);
        }
    }
}
