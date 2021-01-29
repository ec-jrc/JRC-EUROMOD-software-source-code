using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        public class GeneralInfo
        {
            internal const string LOGTAG_SOFTWAREVERSION = "Software-Version";
            internal const string LOGTAG_PROJECT = "Project";
            internal const string LOGTAG_OUTPUTPATH = "Output-Path";

            public string softwareVersion = DefGeneral.UI_VERSION;
            public string project = DefPar.Value.NA;
            public Duration duration = new Duration();
            public string outputPath = DefPar.Value.NA;
        }

        public class RunInfo
        {
            internal const string LOGTAG_RUNID = "Run-Id";
            internal const string LOGTAG_STATUS = "Status";
            internal const string LOGTAG_SYSTEM = "System";
            internal const string LOGTAG_DATABASE = "Database";
            internal const string LOGTAG_OUTPUTFILES = "Outputfile(s)";
            internal const string LOGTAG_CURRENCY = "Currency";
            internal const string LOGTAG_EXRATE = "Exchangerate";
            internal const string LOGTAG_NONDEFAULT_OUTPUTPATH = "Non-Default Output-Path";
            internal const string LOGTAG_OUTPUTFILE = "Output-File";
            internal const string LOGTAG_OUTPUTHASH = "Output-Hash";

            public string runId = null;
            public string systemName = DefPar.Value.NA;
            public string databaseName = DefPar.Value.NA;
            public Duration duration = new Duration();
            public string nonDefaultOutputPath = "-";
            internal List<string> outputFiles = new List<string>();
            internal List<string> outputHashes = new List<string>();
            public string currency = DefPar.Value.NA;
            public string exchangeRate = DefPar.Value.NA;
            public List<string> addOnSystemNames = new List<string>();
            public Dictionary<string, string> extensionSwitches = new Dictionary<string, string>(); // key: extension-short-name, value: on/off
            public List<Communicator.ErrorInfo> errorInfo = new List<Communicator.ErrorInfo>();
            public FINISH_STATUS finishStatus = FINISH_STATUS.unknown;

            public enum FINISH_STATUS { finished, aborted, unknown }

            public bool HasErrorsOrWarnings() { return errorInfo != null && errorInfo.Any(); }
            public bool HasErrors() { return errorInfo != null && (from ei in errorInfo where !ei.isWarning select ei).Any(); }
            public bool HasWarnings() { return HasErrorsOrWarnings() && !HasErrors(); }

            public void ExtractOutputFiles(Dictionary<string, string> detailedProgressInfo)
            {
                if (detailedProgressInfo != null)
                {
                    var outputFilesInfo = from di in detailedProgressInfo
                                          where di.Key.ToUpper().StartsWith(Communicator.EXEPROG_DETINFO_OUTPUT_FILE)
                                          select di.Value;
                    if (outputFilesInfo.Any()) outputFiles.AddRange(outputFilesInfo.ToList());
                }
            }

            public void ExtractAddonSystemNames(Dictionary<string, string> emConfig, string tagAddOn)
            {
                foreach (string ao in from e in emConfig
                                      where e.Key.StartsWith(tagAddOn)
                                      select e.Value) // $"{addOn}|{addOnSys}"
                {
                    string[] split = ao.Split('|');
                    if (split.Length == 2) addOnSystemNames.Add(split[1]);
                }
            }

            public void ExtractExtensionSwitches(Dictionary<string, string> em2Config, string em2TagPolicySwitch)
            {
                List<KeyValuePair<string, string>> em2ConfigLi = new List<KeyValuePair<string, string>>();
                foreach (var ec in em2Config) em2ConfigLi.Add(new KeyValuePair<string, string>(ec.Key, ec.Value));
                ExtractExtensionSwitches(em2ConfigLi, em2TagPolicySwitch);
            }
            public void ExtractExtensionSwitches(List<KeyValuePair<string, string>> em2Config, string em2TagPolicySwitch)
            {
                foreach (string extensionSwitch in from es in em2Config
                                                   where es.Key.StartsWith(em2TagPolicySwitch)
                                                   select es.Value)
                {
                    string[] split = extensionSwitch.Split('='); // example: BTA_??=c5ec590d-9c51-4b88-b482-0a5cb514a880=off
                    if (split.Length == 3) extensionSwitches.Add(split[0], split[2]); // key: BTA, value: on/off
                }
            }
        }

        public class PetInfo
        {
            public const string LOGTAG_ALPHA_CPI = "Alpha CPI";
            public const string LOGTAG_ALPHA_MII = "Alpha MII";
            public const string LOGTAG_STARTED_AT = "Started at";
            public const string LOGTAG_PROJECT = "Project";
            public const string LOGTAG_DECOMPOSITION_TYPE = "Decomposition type";
            public const string LOGTAG_YEAR1 = "Year1";
            public const string LOGTAG_YEAR2 = "Year2";
            public const string LOGTAG_ALPHAS = "Alpha(s)";
            public const string LOGTAG_DECOMPOSING_ON = "Decomposing on";
            public const string LOGTAG_OUTPUT = "Output";
            public const string LOGTAG_SELECTED_COUNTRY = "Selected Country";

            public List<KeyValuePair<string, string>> logEntries = new List<KeyValuePair<string, string>>(); // key: heading, value: info

            internal List<string> systemsWithWarnings = new List<string>();
            internal List<string> systemsWithErrors = new List<string>();
            internal List<string> systemIndependentErrors = new List<string>();

            public void AddSystemWithErrors(string sysName) { systemsWithErrors.AddUnique(sysName, true); }
            public void AddSystemWithWarnings(string sysName) { systemsWithWarnings.AddUnique(sysName, true); }
            public void AddSystemIndependentError(string error) { systemIndependentErrors.Add(error); }

            public bool HasWarnings() { return systemsWithWarnings.Any(); }
            public bool HasErrors() { return systemsWithErrors.Any() || systemIndependentErrors.Any(); }

            public bool EM2_GetErrorInfo(out string info)
            {
                info = string.Empty;
                foreach (string err in systemIndependentErrors) info += err + Environment.NewLine;
                foreach (string sys in systemsWithErrors)
                    info += $"System '{sys}' had {(systemsWithWarnings.Contains(sys, StringComparer.OrdinalIgnoreCase) ? "errors and warnings" : "errors")}!" + Environment.NewLine;
                foreach (string sys in systemsWithWarnings)
                    if (!systemsWithErrors.Contains(sys, StringComparer.OrdinalIgnoreCase)) info += $"System '{sys}' had warnings!" + Environment.NewLine;
                return info != string.Empty;
            }
        }

        public class Duration
        {
            internal const string LOGTAG_START = "Start";
            internal const string LOGTAG_END = "End";
            internal const string LOGTAG_DURATION = "Duration";

            private DateTime startTime_dt = defaultStartTime; private string startTime_s = DefPar.Value.NA;
            private DateTime endTime_dt = defaultEndTime; private string endTime_s = DefPar.Value.NA;
            private string duration = DefPar.Value.NA;

            internal Duration() { }
            public Duration(DateTime startTime, DateTime endTime) { startTime_dt = startTime; endTime_dt = endTime; }
            public Duration(string startTime, string endTime, string _duration) { startTime_s = startTime; endTime_s = endTime; duration = _duration; }

            public string GetEndTime_s() { return GetStringTime(endTime_dt, endTime_s, defaultEndTime); }
            public string GetStartTime_s() { return GetStringTime(startTime_dt, startTime_s, defaultStartTime); }
            public DateTime GetEndTime_dt() { return GetDateTime(endTime_dt, endTime_s, defaultEndTime); }
            public DateTime GetStartTime_dt() { return GetDateTime(startTime_dt, startTime_s, defaultStartTime); }

            public string GetDuration()
            {
                if (duration != DefPar.Value.NA) return duration;
                if (startTime_dt != defaultStartTime && endTime_dt != defaultEndTime) return FormatDuration(endTime_dt - startTime_dt);
                if (DateTime.TryParse(startTime_s, out DateTime s) && DateTime.TryParse(endTime_s, out DateTime e)) return FormatDuration(e - s);
                return DefPar.Value.NA;
            }

            private static DateTime GetDateTime(DateTime dt, string s, DateTime defaultValue)
            {
                if (dt != defaultValue) return dt;
                if (DateTime.TryParse(s, out DateTime pdt)) return pdt;
                return defaultValue;
            }
            public static string GetStringTime(DateTime dt, string s, DateTime defaultValue)
            {
                if (s != DefPar.Value.NA) return s;
                if (dt != defaultValue) return FormatTime(dt);
                return DefPar.Value.NA;
            }

            private static string FormatTime(DateTime dt) { return dt == defaultStartTime || dt == defaultEndTime ? DefPar.Value.NA :
                                                            $"{dt.ToLongDateString()} {dt.ToLongTimeString()}"; }
            private string FormatDuration(TimeSpan ts) { return ts.TotalSeconds.ToString() + "s"; }
            internal static DateTime defaultStartTime = DateTime.MinValue, defaultEndTime = DateTime.MaxValue; 
        }
    }
}
