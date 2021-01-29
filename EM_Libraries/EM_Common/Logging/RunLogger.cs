using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        private static string SEPARATOR_LINE = "-----------------------------------------------------------------------------------------------------------------------------------" + Environment.NewLine;
        private const string HEADER_LINE_MARKER = "=====";
        private const string INFO_SEPARATOR = ": ";
        private const string ERRORINFO_WARNING = "WARNING: ";
        private const string ERRORINFO_ERROR = "ERROR: ";

        private static string LOGTAG_HEADER_EM_LOG = $"{HEADER_LINE_MARKER} {DefGeneral.BRAND_TITLE} LOG {HEADER_LINE_MARKER}";
        private static string LOGTAG_HEADER_RUN_LIST = $"{HEADER_LINE_MARKER} RUN-LOG {HEADER_LINE_MARKER}";
        private static string LOGTAG_HEADER_ERROR_LIST = $"{HEADER_LINE_MARKER} ERROR-LOG {HEADER_LINE_MARKER}";
        private static string LOGTAG_HEADER_PET_LOG = $"{HEADER_LINE_MARKER} {DefGeneral.BRAND_TITLE} POLICY EFFECTS LOG {HEADER_LINE_MARKER}";
        private static string LOGTAG_HEADER_PET_SETTINGS = $"{HEADER_LINE_MARKER} POLICY EFFECT SETTINGS {HEADER_LINE_MARKER}";

        private static string EMLOG_FILENAME { get { return $"{DefGeneral.BRAND_TITLE}_Log.txt"; } }
        private static string PETLOG_FILENAME { get { return $"{DefGeneral.BRAND_TITLE}_PolicyEffects_Log.txt"; } }
        private string GetLogFileNameWithTimeStamp(bool petLog = false) { return $"{timeStamp}_" + (petLog ? PETLOG_FILENAME : EMLOG_FILENAME); }

        private string timeStamp { get { if (_timeStamp == null) _timeStamp = string.Format("{0:yyyyMMddHHmm}", DateTime.Now); return _timeStamp; } }
        private string _timeStamp = null;

        private GeneralInfo generalInfo = new GeneralInfo();
        private List<RunInfo> runInfoList = null;
        private PetInfo petInfo = null;

        public RunLogger(string _project, List<RunInfo> _runInfoList, PetInfo _petInfo = null)
        {
            generalInfo = new GeneralInfo() { project = _project };
            runInfoList = _runInfoList;
            petInfo = _petInfo;
            
            if (runInfoList != null)
            {
                DateTime startTime = DateTime.MaxValue, endTime = DateTime.MinValue;
                foreach (RunInfo runInfo in runInfoList)
                {
                    runInfo.runId = Guid.NewGuid().ToString();
                    if (runInfo.duration.GetEndTime_dt() != Duration.defaultEndTime && runInfo.duration.GetEndTime_dt() > endTime) endTime = runInfo.duration.GetEndTime_dt();
                    if (runInfo.duration.GetStartTime_dt() != Duration.defaultStartTime && runInfo.duration.GetStartTime_dt() < startTime) startTime = runInfo.duration.GetStartTime_dt();
                }
                generalInfo.duration = new Duration(startTime, endTime);
            }
        }
        public RunLogger(string logFilePath, bool petLog = false)
        {
            if (petLog) TxtReadPetLog(logFilePath); else TxtReadEMLog(logFilePath);
        }

        private void Clear() { generalInfo = new GeneralInfo(); runInfoList = null; petInfo = null; }

        private List<string> GetLogSection(List<string> fileContent,
                                   string headerMarker, bool headerMarkerCompulsory = false,
                                   string endMarker = null, bool endMarkerCompulsory = false)
        {
            var hm = from c in fileContent where c == headerMarker select c;
            if (headerMarkerCompulsory && !hm.Any()) throw new Exception($"Marker '{headerMarker}' not found!");
            if (!hm.Any()) return new List<string>();
            int indexHeaderLine = fileContent.IndexOf(hm.First());

            var em = from c in fileContent where c == endMarker select c;
            if (endMarkerCompulsory && !em.Any()) throw new Exception($"Marker '{endMarker}' not found!");
            if (!em.Any()) return (from line in fileContent where fileContent.IndexOf(line) > indexHeaderLine select line).ToList();
            int indexEndLine = fileContent.IndexOf(em.First());

            return (from line in fileContent
                    where fileContent.IndexOf(line) > indexHeaderLine && fileContent.IndexOf(line) < indexEndLine
                    select line).ToList();
        }

        private bool SplitInfo(string line, out string infoHeader, out string infoValue)
        {
            infoHeader = infoValue = string.Empty;
            int iSplit = line.IndexOf(INFO_SEPARATOR);
            if (iSplit < 0 || line.Length < iSplit + INFO_SEPARATOR.Length + 1) return false;
            infoHeader = line.Substring(0, iSplit);
            infoValue = line.Substring(iSplit + INFO_SEPARATOR.Length);
            return true;
        }
    }
}
