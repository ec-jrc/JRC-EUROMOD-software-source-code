using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    /// <summary> administrates the content of the configuration file for running the executable </summary>
    public class ExeRunConfig
    {
        public EMPath pathHandler = null;
        public string pathProject = string.Empty;
        public string pathData = string.Empty;
        public string pathOutput = string.Empty;
        public string pathGlobal = string.Empty;
        public string countryShortName = string.Empty;
        public string dataID = string.Empty; // in fact id or name
        public string sysID = string.Empty;  // ditto
        public bool forceOutputEuro = false;
        public string dateExchangeRate = TAGS.EXRATE_DEFAULT;
        public Dictionary<string, string> addOns = new Dictionary<string, string>(); // key: add-on (e.g. MTR), value: add-on-system (name, e.g. AT_MTR, or id)
        public Dictionary<string, bool> extensionSwitches = new Dictionary<string, bool>(); // key: extension-id, value: on=true, off=false
        public bool forceSequentialRun = false;
        public bool forceSequentialOutput = false;
        public bool forceAutoSequentialRun = false;
        public int maxRunTimeErrors = 5; // maximum errors issued, if a run-time-error concerns many or all TUs (e.g. upper limit < lower limit)
        public bool ignorePrivate = false;
        public double? firstHH = null, lastHH = null;
        public double nHHOnly = double.MaxValue;
        public string outFileDate = string.Empty;
        public string stdOutputFilenameSuffix = string.Empty;
        public string pathParModifications = string.Empty;
        public string stringParModifications = string.Empty;
        public bool returnOutputInMemory = false;
        public string dataPassword = string.Empty;
        public bool warnAboutUselessGroups = false;

        public Dictionary<string, string> origSettings = new Dictionary<string, string>();

        /// <summary> takes and "prepares" setting for the executable, i.e. checks for compulsory respectively unknown settings </summary>
        public void TakeSettings(Dictionary<string, string> settings, Communicator communicator)
        {
            origSettings = settings; bool useFinalEM3Paths = false;
            foreach (var setting in settings) // take what we've got
            {
                if (setting.Key.StartsWith(TAGS.CONFIG_EXTENSION_SWITCH))
                { 
                    string[] es = setting.Value.Split('=');
                    if (es.Length == 2) extensionSwitches.Add(es[0], es[1] == DefPar.Value.ON);
                }
                else if (setting.Key.StartsWith(TAGS.CONFIG_ADDON))
                {
                    if (SplitAddOnEntry(setting.Value, out string addOnName, out string addOnSys))
                        addOns.Add(addOnName, addOnSys);
                }
                else switch (setting.Key)
                {
                    case TAGS.CONFIG_PATH_EUROMODFILES: pathProject = setting.Value; break;
                    case TAGS.CONFIG_PATH_DATA: pathData = setting.Value; break;
                    case TAGS.CONFIG_PATH_OUTPUT: pathOutput = setting.Value; break;
                    case TAGS.CONFIG_PATH_GLOBAL: pathGlobal = setting.Value; break; 
                    case TAGS.CONFIG_COUNTRY: countryShortName = setting.Value; break;
                    case TAGS.CONFIG_ID_DATA: dataID = setting.Value; break;
                    case TAGS.CONFIG_ID_SYSTEM: sysID = setting.Value; break;
                    case TAGS.CONFIG_DATE_EXCHANGE_RATE: dateExchangeRate = setting.Value; break;
                    case TAGS.CONFIG_FORCE_SEQUENTIAL_RUN: forceSequentialRun = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_FORCE_SEQUENTIAL_OUTPUT: forceSequentialOutput = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_FORCE_AUTO_SEQUENTIAL_RUN: forceAutoSequentialRun = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_MAX_RUNTIME_ERRORS: if (int.TryParse(setting.Value, out int mre)) maxRunTimeErrors = mre; break;
                    case TAGS.CONFIG_IGNORE_PRIVATE: ignorePrivate = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_FIRST_HH: if (double.TryParse(setting.Value, out double f) && f >= 0) firstHH = f; break;
                    case TAGS.CONFIG_LAST_HH: if (double.TryParse(setting.Value, out double l) && l >= 0) lastHH = l; break;
                    case TAGS.CONFIG_FIRST_N_HH_ONLY: if (double.TryParse(setting.Value, out double n)) nHHOnly = n; break;
                    case TAGS.CONFIG_FORCE_OUTPUT_EURO: forceOutputEuro = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_DATE_OUTFILE: if (setting.Value != "-") outFileDate = setting.Value; break;
                    case TAGS.STDOUTPUT_FILENAME_SUFFIX: stdOutputFilenameSuffix = setting.Value; break;
                    case TAGS.CONFIG_PATH_PAR_MODIFICATIONS: pathParModifications = setting.Value; break;
                    case TAGS.CONFIG_STRING_PAR_MODIFICATIONS: stringParModifications = setting.Value; break;
                    case TAGS.CONFIG_USE_FINAL_EM3_PATHS: useFinalEM3Paths = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_RETURN_OUTPUT_IN_MEMORY: returnOutputInMemory = EM_Helpers.GetBool(setting.Value) == true; break;
                    case TAGS.CONFIG_DATA_PASSWORD: dataPassword = setting.Value; break;
                    case TAGS.CONFIG_WARN_ABOUT_USELESS_GROUPS: warnAboutUselessGroups = EM_Helpers.GetBool(setting.Value) == true; break;
                    default: communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"Unknown configuration setting: {setting.Key} = {setting.Value}" }); break;
                }
            }

            pathHandler = new EMPath(pathProject, pathGlobal, useFinalEM3Paths);

            string missing = string.Empty;
            foreach (string setting in new List<string>() { TAGS.CONFIG_PATH_EUROMODFILES, TAGS.CONFIG_PATH_DATA,
                TAGS.CONFIG_PATH_OUTPUT, TAGS.CONFIG_COUNTRY, TAGS.CONFIG_ID_DATA, TAGS.CONFIG_ID_SYSTEM })
                if (!settings.ContainsKey(setting)) missing += setting + " ";
            if (missing != string.Empty) throw new Exception($"Compulsory configuration setting(s) missing: {missing}");
        }

        public static bool SplitAddOnEntry(string setting, out string addOnName, out string addOnSys)
        {
            addOnName = string.Empty; addOnSys = string.Empty;
            string[] ao = setting.Split('|');
            if (ao.Length != 2) return false;
            addOnName = ao[0]; addOnSys = ao[1];
            return true;
        }

        public static Dictionary<string, string> GetBasicConfig(string euromodFilesFolder, string country, string sysNameOrId,
                                                                string dataFolder, string dataNameOrId, string outputFolder)
        {
            return new Dictionary<string, string>()
            {
                { TAGS.CONFIG_PATH_EUROMODFILES, euromodFilesFolder },
                { TAGS.CONFIG_COUNTRY, country },
                { TAGS.CONFIG_ID_SYSTEM, sysNameOrId },
                { TAGS.CONFIG_PATH_DATA, dataFolder },
                { TAGS.CONFIG_ID_DATA, dataNameOrId },
                { TAGS.CONFIG_PATH_OUTPUT, outputFolder }
            };
        }
    }
}
