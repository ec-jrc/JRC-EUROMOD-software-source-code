using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        private static bool SearchLog(string outputFilePath, out RunInfo runInfo, out RunLogger runLogger)
        {
            try
            {
                runInfo = null; runLogger = null;
                string outputFileHash = EM_Helpers.GetFileMD5Hash(outputFilePath);
                DateTime outputFileSaveTime = File.GetLastWriteTime(outputFilePath);

                List<FileInfo> logFiles = (new DirectoryInfo(Path.GetDirectoryName(outputFilePath)).GetFiles($"*{EMLOG_FILENAME}")).ToList();
                logFiles.AddRange(new DirectoryInfo(Path.GetDirectoryName(outputFilePath)).GetFiles($"*{PETLOG_FILENAME}"));

                foreach (FileInfo fiLog in
                    from log in logFiles
                    where File.GetLastWriteTime(log.FullName) >= outputFileSaveTime && // log is always the last to write, thus includes only run-outputs with earlier save-times
                          File.GetLastWriteTime(log.FullName) < outputFileSaveTime + new TimeSpan(24, 0, 0) // do not include "ancient" logs (more than a day after run-output finish-time)
                    select log)
                {
                    RunLogger rl = new RunLogger(fiLog.FullName, fiLog.Name.Contains(PETLOG_FILENAME));
                    foreach (RunInfo ri in rl.runInfoList)
                    {
                        foreach (string hash in ri.outputHashes)
                        {
                            if (hash == outputFileHash) // if the hash is equal, it was a run with the same settings ...
                            {
                                runLogger = rl;
                                runInfo = ri;
                                if (Math.Abs((outputFileSaveTime - ri.duration.GetEndTime_dt()).TotalSeconds) <= 2)
                                    return true; // ... but the save-time should verify that it was really this run
                                break;
                            }
                        }
                    }
                }
                return runInfo != null;
            }
            catch (Exception exception) { throw new Exception($"Error finding log-file for {outputFilePath}:{Environment.NewLine}{exception.Message}"); }
        }

        public static bool GetRunInfo(string outputFilePath, out RunInfo runInfo, out GeneralInfo generalInfo, out PetInfo petInfo)
        {
            generalInfo = null; petInfo = null;
            if (!SearchLog(outputFilePath, out runInfo, out RunLogger runLogger)) return false;
            generalInfo = runLogger.generalInfo; petInfo = runLogger.petInfo; return true;
        }

        public static bool GetRunInfo(string outputFilePath, out RunInfo runInfo, out GeneralInfo generalInfo)
        {
            generalInfo = null;
            if (!SearchLog(outputFilePath, out runInfo, out RunLogger runLogger)) return false;
            generalInfo = runLogger.generalInfo; return true;
        }
    }
}
