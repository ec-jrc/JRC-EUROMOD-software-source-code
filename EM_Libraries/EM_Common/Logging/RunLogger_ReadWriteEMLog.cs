using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        public void TxtWriteEMLog(string outputPath)
        {
            string logFilePath = Path.Combine(outputPath, GetLogFileNameWithTimeStamp());
            try
            {
                if (generalInfo.outputPath == DefPar.Value.NA) generalInfo.outputPath = outputPath;
                GeneralInfoToFile(out string txtGeneralInfo);
                RunLogToFile(out string txtRunLog);
                string txtErrLog = !ErrLogToFile(out string el) ? string.Empty :
                    SEPARATOR_LINE + LOGTAG_HEADER_ERROR_LIST + Environment.NewLine + el;

                File.WriteAllText(logFilePath,
                    LOGTAG_HEADER_EM_LOG + Environment.NewLine + txtGeneralInfo + Environment.NewLine + SEPARATOR_LINE +
                    LOGTAG_HEADER_RUN_LIST + Environment.NewLine + txtRunLog + Environment.NewLine +
                    txtErrLog);
            }
            catch (Exception exception) { throw new Exception($"Error writing {logFilePath}:{Environment.NewLine}{exception.Message}"); }
        }

        private void TxtReadEMLog(string logFilePath)
        {
            try
            {
                List<string> fileContent = File.ReadAllLines(logFilePath).ToList(); Clear();
                GeneralInfoFromFile(GetLogSection(fileContent, LOGTAG_HEADER_EM_LOG, true, LOGTAG_HEADER_RUN_LIST, true));
                RunLogFromFile(GetLogSection(fileContent, LOGTAG_HEADER_RUN_LIST, true, LOGTAG_HEADER_ERROR_LIST));
                ErrLogFromFile(GetLogSection(fileContent, LOGTAG_HEADER_ERROR_LIST));
            }
            catch (Exception exception) { throw new Exception($"Error interpreting {logFilePath}:{Environment.NewLine}{exception.Message}"); }
        }
    }
}
