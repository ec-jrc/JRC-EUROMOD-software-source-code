using System;
using System.Collections.Generic;

namespace EM_Common
{
    public partial class RunLogger
    {
        private void GeneralInfoToFile(out string txtGeneraInfo)
        {
            txtGeneraInfo = $"{GeneralInfo.LOGTAG_SOFTWAREVERSION}{INFO_SEPARATOR}{generalInfo.softwareVersion}" + Environment.NewLine
                          + $"{GeneralInfo.LOGTAG_PROJECT}{INFO_SEPARATOR}{generalInfo.project}" + Environment.NewLine
                          + $"{Duration.LOGTAG_START}{INFO_SEPARATOR}{generalInfo.duration.GetStartTime_s()}" + Environment.NewLine
                          + $"{Duration.LOGTAG_END}{INFO_SEPARATOR}{generalInfo.duration.GetEndTime_s()}" + Environment.NewLine
                          + $"{Duration.LOGTAG_DURATION}{INFO_SEPARATOR}{generalInfo.duration.GetDuration()}" + Environment.NewLine
                          + $"{GeneralInfo.LOGTAG_OUTPUTPATH}{INFO_SEPARATOR}{generalInfo.outputPath}";
        }

        private void GeneralInfoFromFile(List<string> fileContent)
        {
            string startTime = DefPar.Value.NA, endTime = DefPar.Value.NA, duration = DefPar.Value.NA;
            foreach (string line in fileContent)
            {
                SplitInfo(line, out string infoHeader, out string infoValue);
                switch (infoHeader)
                {
                    case GeneralInfo.LOGTAG_SOFTWAREVERSION: generalInfo.softwareVersion = infoValue; break;
                    case GeneralInfo.LOGTAG_PROJECT: generalInfo.project = infoValue; break;
                    case Duration.LOGTAG_START: startTime = infoValue; break;
                    case Duration.LOGTAG_END: endTime = infoValue; break;
                    case Duration.LOGTAG_DURATION: duration = infoValue; break;
                    case GeneralInfo.LOGTAG_OUTPUTPATH: generalInfo.outputPath = infoValue; break;
                }
            }
            generalInfo.duration = new Duration(startTime, endTime, duration);
        }
    }
}
