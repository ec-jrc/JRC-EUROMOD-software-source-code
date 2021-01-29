using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        private bool ErrLogToFile(out string txtErrLog)
        {
            if (runInfoList == null) throw new Exception("Run-info not available"); // this is a programming-error

            txtErrLog = string.Empty;
            var runsWithError = from ri in runInfoList where ri.errorInfo.Count > 0 select ri;
            if (!runsWithError.Any()) return false;

            foreach (RunInfo runInfo in runsWithError)
            {
                if (runInfo != runsWithError.First()) txtErrLog += SEPARATOR_LINE;
                txtErrLog += $"{RunInfo.LOGTAG_RUNID}{INFO_SEPARATOR}{runInfo.runId}" + Environment.NewLine;
                foreach (Communicator.ErrorInfo errorInfo in runInfo.errorInfo)
                    txtErrLog += (errorInfo.isWarning ? ERRORINFO_WARNING : ERRORINFO_ERROR) + errorInfo.message + Environment.NewLine;
            }
            txtErrLog = txtErrLog.TrimEnd();  return true;
        }

        private void ErrLogFromFile(List<string> fileContent)
        {
            if (fileContent.Count == 0) return;

            // analyse sections between 'Run-Id: abc...' and Run-Id: def...' (i.e. the warnings and errors of a specific run)
            List<int> iRunStarts = (from l in fileContent where l.StartsWith(RunInfo.LOGTAG_RUNID) select fileContent.IndexOf(l)).ToList();
            foreach (int iRunStart in iRunStarts)
            {
                string runId = fileContent[iRunStart].Substring(RunInfo.LOGTAG_RUNID.Length + INFO_SEPARATOR.Length);
                var rl = from ri in runInfoList where ri.runId == runId select ri; if (!rl.Any()) continue;
                RunInfo runInfo = rl.First();

                int iRunStartNext = iRunStart == iRunStarts.Last() ? fileContent.Count : iRunStarts[iRunStarts.IndexOf(iRunStart)+1];
                for (int i = iRunStart + 1, j; i < iRunStartNext;)
                {
                    bool isE = fileContent[i].StartsWith(ERRORINFO_ERROR), isW = fileContent[i].StartsWith(ERRORINFO_WARNING);
                    if (!isE && !isW) { ++i; continue; }
                    string m = fileContent[i].Substring(isE ? ERRORINFO_ERROR.Length : ERRORINFO_WARNING.Length);
                    for (j = i + 1; j < iRunStartNext && !fileContent[j].StartsWith(ERRORINFO_ERROR) && !fileContent[j].StartsWith(ERRORINFO_WARNING); ++j)
                        m += Environment.NewLine + fileContent[j]; // error has more than 1 line
                    runInfo.errorInfo.Add(new Communicator.ErrorInfo() { isWarning = isW, message = m });
                    i = j;
                }
            }
        }
    }
}
