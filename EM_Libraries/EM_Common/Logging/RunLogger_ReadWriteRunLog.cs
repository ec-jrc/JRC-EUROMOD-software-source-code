using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace EM_Common
{
    public partial class RunLogger
    {
        private void RunLogToFile(out string txtRunLog)
        {
            if (runInfoList == null) throw new Exception("Run-info not available"); // this is a programming-error

            txtRunLog = string.Join("\t", new List<string>() { RunInfo.LOGTAG_RUNID, RunInfo.LOGTAG_STATUS,
                                                               RunInfo.LOGTAG_SYSTEM, RunInfo.LOGTAG_DATABASE,
                                                               Duration.LOGTAG_START, Duration.LOGTAG_END, Duration.LOGTAG_DURATION,
                                                               RunInfo.LOGTAG_CURRENCY, RunInfo.LOGTAG_EXRATE });

            List<string> extensionHeaders = (from ri in runInfoList select ri.extensionSwitches.Keys)
                                            .Aggregate(new List<string>(), (x, y) => x.Concat(y).ToList())
                                            .Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            if (extensionHeaders.Any()) txtRunLog += "\t" + string.Join("\t", extensionHeaders);

            txtRunLog += "\t" + RunInfo.LOGTAG_NONDEFAULT_OUTPUTPATH;
            for (int o = 0; o < (from r in runInfoList select r.outputFiles.Count()).Max(); ++o)
                txtRunLog += $"\t{RunInfo.LOGTAG_OUTPUTFILE}\t{RunInfo.LOGTAG_OUTPUTHASH}";
            txtRunLog += Environment.NewLine;

            foreach (RunInfo runInfo in runInfoList)
            {
                string addOnSystemNames = runInfo.addOnSystemNames.Any() ? "+" + string.Join("+", runInfo.addOnSystemNames) : string.Empty;
                List<string> extensionValues = new List<string>();
                foreach (string eh in extensionHeaders)
                {
                    if (!runInfo.extensionSwitches.CaseInsensitiveContainsKey(eh)) extensionValues.Add("-1");
                    else if (runInfo.extensionSwitches.CaseInsensitiveGet(eh).Value == DefPar.Value.ON) extensionValues.Add("1");
                    else if (runInfo.extensionSwitches.CaseInsensitiveGet(eh).Value == DefPar.Value.OFF) extensionValues.Add("0");
                    else extensionValues.Add(runInfo.extensionSwitches.CaseInsensitiveGet(eh).Value);
                }
                txtRunLog += runInfo.runId
                           + "\t" + runInfo.finishStatus
                           + "\t" + runInfo.systemName + addOnSystemNames
                           + "\t" + runInfo.databaseName
                           + "\t" + runInfo.duration.GetStartTime_s()
                           + "\t" + runInfo.duration.GetEndTime_s()
                           + "\t" + runInfo.duration.GetDuration()
                           + "\t" + runInfo.currency
                           + "\t" + runInfo.exchangeRate;

                if (extensionValues.Any()) txtRunLog += "\t" + string.Join("\t", extensionValues);

                txtRunLog += "\t" + runInfo.nonDefaultOutputPath;
                foreach (string outputFile in runInfo.outputFiles)
                {
                    string hash = DefPar.Value.NA;
                    int iOut = runInfo.outputFiles.IndexOf(outputFile);
                    if (runInfo.outputHashes.Count > iOut) hash = runInfo.outputHashes[iOut];
                    else
                    {
                        try
                        {
                            hash = EM_Helpers.GetFileMD5Hash(Path.Combine(
                                runInfo.nonDefaultOutputPath == "-" ? generalInfo.outputPath : runInfo.nonDefaultOutputPath,
                                $"{outputFile}{(outputFile.EndsWith(".txt") ? string.Empty : ".txt")}"));
                        }
                        catch { }
                    }
                    txtRunLog += $"\t{outputFile}\t{hash}";
                }
                txtRunLog += Environment.NewLine;
            }
            txtRunLog = txtRunLog.TrimEnd();
        }

        private void RunLogFromFile(List<string> fileContent)
        {
            if (fileContent.Count < 2) throw new Exception($"Insufficient information in section '{LOGTAG_HEADER_RUN_LIST}'!");
            
            runInfoList = new List<RunInfo>();
            List<string> headerLine = fileContent.First().Split('\t').ToList();

            foreach (string l in fileContent.Skip(1))
            {
                List<string> line = l.Split('\t').ToList();
                RunInfo runInfo = new RunInfo()
                {
                    runId = GetProperty(RunInfo.LOGTAG_RUNID, line),
                    databaseName = GetProperty(RunInfo.LOGTAG_DATABASE, line),
                    nonDefaultOutputPath = GetProperty(RunInfo.LOGTAG_NONDEFAULT_OUTPUTPATH, line),
                    currency = GetProperty(RunInfo.LOGTAG_CURRENCY, line),
                    exchangeRate = GetProperty(RunInfo.LOGTAG_EXRATE, line),
                    duration = new Duration(GetProperty(Duration.LOGTAG_START, line), GetProperty(Duration.LOGTAG_END, line), GetProperty(Duration.LOGTAG_DURATION, line)),
                    finishStatus = Enum.TryParse(GetProperty(RunInfo.LOGTAG_STATUS, line), out RunInfo.FINISH_STATUS f) ? f : RunInfo.FINISH_STATUS.unknown
                };

                string[] sysAo = GetProperty(RunInfo.LOGTAG_SYSTEM, line).Split('+');
                if (sysAo.Any()) runInfo.systemName = sysAo.First();
                foreach (string ao in sysAo.Skip(1)) runInfo.addOnSystemNames.Add(ao);

                int iLastSimpleProp = headerLine.IndexOf(RunInfo.LOGTAG_EXRATE);
                if (iLastSimpleProp < 0) throw new Exception($"Marker '{RunInfo.LOGTAG_EXRATE}' not found!");
                int iOutputInfo = headerLine.IndexOf(RunInfo.LOGTAG_NONDEFAULT_OUTPUTPATH);
                if (iOutputInfo < 0) throw new Exception($"Marker '{RunInfo.LOGTAG_NONDEFAULT_OUTPUTPATH}' not found!");

                for (int iExSwitch = iLastSimpleProp + 1; iExSwitch < iOutputInfo; ++iExSwitch)
                    if (iExSwitch < line.Count && line[iExSwitch] != "-1")
                        runInfo.extensionSwitches.Add(headerLine[iExSwitch],
                                                      line[iExSwitch] == "1" ? DefPar.Value.ON : (line[iExSwitch] == "0" ? DefPar.Value.OFF : line[iExSwitch]));
                for (int iOutput = iOutputInfo + 1; iOutput < headerLine.Count - 1; iOutput += 2)
                {
                    if (iOutput + 1 >= line.Count) break;
                    runInfo.outputFiles.Add(line[iOutput]); runInfo.outputHashes.Add(line[iOutput + 1]);
                }

                runInfoList.Add(runInfo);
            }

            string GetProperty(string propName, List<string> line)
            {
                if (headerLine.Contains(propName) && line.Count > headerLine.IndexOf(propName)) return line.ElementAt(headerLine.IndexOf(propName));
                return DefPar.Value.NA;
            }
        }
    }
}
