using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        public void TxtWritePetLog(string outputPath)
        {
            string logFilePath = Path.Combine(outputPath, GetLogFileNameWithTimeStamp(true));
            try
            {
                if (petInfo == null) throw new Exception("Pet-info not available"); // this is a programming-error

                if (generalInfo.outputPath == DefPar.Value.NA) generalInfo.outputPath = outputPath;
                GeneralInfoToFile(out string txtGeneralInfo);
                PetLogToFile(out string txtPetLog);
                RunLogToFile(out string txtRunLog);

                string txtErrLog = string.Empty;
                if (PetErrLogToFile(out string pel)) txtErrLog += pel + Environment.NewLine + SEPARATOR_LINE;
                if (ErrLogToFile(out string rel)) txtErrLog += rel;
                if (txtErrLog != string.Empty) txtErrLog = SEPARATOR_LINE + LOGTAG_HEADER_ERROR_LIST + Environment.NewLine + txtErrLog;

                File.WriteAllText(logFilePath,
                    LOGTAG_HEADER_PET_LOG + Environment.NewLine + txtGeneralInfo + Environment.NewLine + SEPARATOR_LINE +
                    LOGTAG_HEADER_PET_SETTINGS + Environment.NewLine + txtPetLog + Environment.NewLine + SEPARATOR_LINE +
                    LOGTAG_HEADER_RUN_LIST + Environment.NewLine + txtRunLog + Environment.NewLine +
                    txtErrLog);
            }
            catch (Exception exception) { throw new Exception($"Error writing {logFilePath}:{Environment.NewLine}{exception.Message}"); }

            void PetLogToFile(out string txtPetLog)
            {
                txtPetLog = string.Empty;
                foreach (var pi in petInfo.logEntries)
                    txtPetLog += $"{pi.Key}{(string.IsNullOrEmpty(pi.Value) ? string.Empty : $"{INFO_SEPARATOR}{pi.Value}")}" + Environment.NewLine;
                txtPetLog = txtPetLog.TrimEnd();
            }

            bool PetErrLogToFile(out string txtPetErrLog)
            {
                txtPetErrLog = string.Empty;
                foreach (string error in petInfo.systemIndependentErrors)
                    txtPetErrLog += ERRORINFO_ERROR + error + Environment.NewLine;
                txtPetErrLog = txtPetErrLog.TrimEnd();
                return petInfo.systemIndependentErrors.Any();
            }
        }

        private void TxtReadPetLog(string logFilePath)
        {
            try
            {
                List<string> fileContent = File.ReadAllLines(logFilePath).ToList(); Clear();
                GeneralInfoFromFile(GetLogSection(fileContent, LOGTAG_HEADER_PET_LOG, true, LOGTAG_HEADER_PET_SETTINGS, true));
                PetLogFromFile(GetLogSection(fileContent, LOGTAG_HEADER_PET_SETTINGS, true, LOGTAG_HEADER_RUN_LIST, true));
                RunLogFromFile(GetLogSection(fileContent, LOGTAG_HEADER_RUN_LIST, true, LOGTAG_HEADER_ERROR_LIST));
                List<string> errLog = GetLogSection(fileContent, LOGTAG_HEADER_ERROR_LIST);
                PetErrLogFromFile(errLog); ErrLogFromFile(errLog);
            }
            catch (Exception exception) { throw new Exception($"Error interpreting {logFilePath}:{Environment.NewLine}{exception.Message}"); }

            void PetLogFromFile(List<string> petLogSection)
            {
                petInfo = new PetInfo();
                foreach (string line in petLogSection)
                    if (SplitInfo(line, out string infoHeader, out string infoValue))
                        petInfo.logEntries.Add(new KeyValuePair<string, string>(infoHeader, infoValue));
            }

            void PetErrLogFromFile(List<string> petErrLogSection)
            {
                foreach (string line in petErrLogSection)
                {
                    if (line.StartsWith(RunInfo.LOGTAG_RUNID)) break;
                    if (line.StartsWith(ERRORINFO_ERROR)) petInfo.AddSystemIndependentError(line.Substring(ERRORINFO_ERROR.Length));
                }
            }
        }
    }
}
