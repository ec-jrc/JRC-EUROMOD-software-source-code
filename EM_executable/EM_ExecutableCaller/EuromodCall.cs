using EM_Common;
using EM_Transformer;
using EM_XmlHandler;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EM_ExecutableCaller
{
    internal class EuromodCall
    {
        private const string ARG_CONFIG = "-config";
        private const string ARG_EMPATH = "-emPath";
        private const string ARG_SYS = "-sys";
        private const string ARG_DATA = "-data";
        private const string ARG_OUTPATH = "-outPath";
        private const string ARG_DATAPATH = "-dataPath";
        private const string ARG_GLOBALPATH = "-globalPath";
        private const string ARG_NOTRANSLATE = "-noTranslate";
        private const string ARG_SEQUENTIAL_RUN = "-forceSequentialRun";
        private const string ARG_SEQUENTIAL_OUTPUT = "-forceSequentialOutput";
        private const string ARG_OUTPUT_IN_EURO = "-forceOutputInEuro";
        private const string ARG_ADDON = "-addOn";
        private const string ARG_EXTENSION_SWITCH = "-extSwitch";
        private const string ARG_PATH_PAR_MODIFICATIONS = "-pathParModifications";
        private const string ARG_DATA_PASSWORD = "-dataPassword";

        internal static int Go(string[] args)
        {
            try
            {
                //args = Test.TPlay();

                if (args.Length == 0) return WrongUseError();

                // get arguments into a dictionary
                List<string> _args = (from a in args select a.Trim()).ToList();
                Dictionary<string, string> arguments = args[0].Trim().StartsWith("-") ? GetArgsEM3Style(_args) : GetArgsEM2Style(_args);
                if (arguments == null) return WrongUseError();

                // prepare logging
                RunLogger.RunInfo runInfo = new RunLogger.RunInfo();

                // translate or create config
                Dictionary<string, string> em3Config = null; List<KeyValuePair<string, string>> em2Config = null;
                if (arguments.ContainsKey(ARG_CONFIG))
                {
                    if (!TransformEMConfig.Transform(arguments[ARG_CONFIG], out em2Config, out em3Config,
                                                     err => { WriteErr($"EM3 CONFIG-TRANSFORMER", err); })) return 1;
                    arguments.Remove(ARG_CONFIG);
                }
                else
                {
                    if (!arguments.ContainsKey(ARG_EMPATH) || !arguments.ContainsKey(ARG_SYS) || !arguments.ContainsKey(ARG_DATA))
                        return WrongUseError("Compulsory argument missing.");
                    CreateConfig(arguments[ARG_EMPATH], arguments[ARG_SYS], arguments[ARG_DATA], out em3Config); // create a minimal-config
                    arguments.Remove(ARG_EMPATH); arguments.Remove(ARG_SYS); arguments.ContainsKey(ARG_DATA);
                }

                // handle optional parameters
                int addOnCnt = 0, extCnt = 0;
                foreach (var optArg in arguments)
                {
                    if (optArg.Key.ToLower() == ARG_OUTPATH.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_PATH_OUTPUT, optArg.Value);
                    else if (optArg.Key.ToLower() == ARG_DATAPATH.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_PATH_DATA, optArg.Value);
                    else if (optArg.Key.ToLower() == ARG_GLOBALPATH.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_PATH_GLOBAL, optArg.Value);
                    else if (optArg.Key.ToLower() == ARG_SEQUENTIAL_RUN.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_FORCE_SEQUENTIAL_RUN, DefPar.Value.YES);
                    else if (optArg.Key.ToLower() == ARG_SEQUENTIAL_OUTPUT.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_FORCE_SEQUENTIAL_OUTPUT, DefPar.Value.YES);
                    else if (optArg.Key.ToLower() == ARG_OUTPUT_IN_EURO.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_FORCE_OUTPUT_EURO, DefPar.Value.YES);
                    else if (optArg.Key.ToLower().StartsWith(ARG_ADDON.ToLower()))
                    {
                        if (optArg.Value.Split("|").Count() != 2) return WrongUseError($"Invalid {ARG_ADDON} argument '{optArg.Value}' (correct: add-on-name|add-on-system, e.g. MTR|MTR_EL)");
                        em3Config.Add($"{TAGS.CONFIG_ADDON}{addOnCnt++}", optArg.Value);
                    }
                    else if (optArg.Key.ToLower().StartsWith(ARG_EXTENSION_SWITCH.ToLower()))
                    {
                        if (optArg.Value.Split("=").Count() != 2) return WrongUseError($"Invalid {ARG_EXTENSION_SWITCH} argument '{optArg.Value}' (correct: extension-name=switch, e.g. BTA_??=off)");
                        em3Config.Add($"{TAGS.EXTENSION_SWITCH}{extCnt++}", optArg.Value); // still extension-name needs to be replaced by id, see below
                    }
                    else if (optArg.Key.ToLower() == ARG_PATH_PAR_MODIFICATIONS.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_PATH_PAR_MODIFICATIONS, optArg.Value);
                    else if (optArg.Key.ToLower() == ARG_DATA_PASSWORD.ToLower())
                        em3Config.AddOrReplace(TAGS.CONFIG_DATA_PASSWORD, optArg.Value);
                    // more to come ...
                }

                if (!arguments.ContainsKey(ARG_NOTRANSLATE))
                {
                    // translate country and global files (but do not stop on error - maybe still works ...)
                    if (!em3Config.ContainsKey(TAGS.CONFIG_COUNTRY)) return WrongUseError($"Configuration file does not contain tag {TAGS.CONFIG_COUNTRY}");
                    if (!em3Config.ContainsKey(TAGS.CONFIG_PATH_EUROMODFILES)) return WrongUseError($"Configuration file does not contain tag {TAGS.CONFIG_PATH_EUROMODFILES}");
                    EM3Country.Transform(em3Config[TAGS.CONFIG_PATH_EUROMODFILES], em3Config[TAGS.CONFIG_COUNTRY], out List<string> errors);
                    foreach (string err in errors) WriteErr("EM3 COUNTRY-TRANSFORMER", err);
                    EM3Global.Transform(em3Config[TAGS.CONFIG_PATH_EUROMODFILES], out errors);
                    foreach (string err in errors) WriteErr("EM3 GLOBAL-FILE-TRANSFORMER", err);
                    EM3Variables.Transform(em3Config[TAGS.CONFIG_PATH_EUROMODFILES], out errors);
                    foreach (string err in errors) WriteErr("EM3 VARIABLES-TRANSFORMER", err);

                    // translate possible add-ons
                    foreach (var entry in em3Config)
                    {
                        if (!entry.Key.StartsWith(TAGS.CONFIG_ADDON)) continue;
                        EM3Country.TransformAddOn(em3Config[TAGS.CONFIG_PATH_EUROMODFILES], entry.Value.Split('|').First(), out errors);
                        foreach (string err in errors) WriteErr("EM3 ADD-ON-TRANSFORMER", err);
                    }
                }

                // replace extension-name by id in config, if necessary 
                if (!HandleExtensionSwitches(em3Config, out Dictionary<string, string> modifiedExtensionSwitches)) return 1;

                // finally run
                DateTime startTime = DateTime.Now;
                int returnCode = new EM_Executable.Control().Run(em3Config,
                    prog =>
                    {
                        Console.WriteLine(prog.message);
                        runInfo.ExtractOutputFiles(prog.detailedInfo);
                        return true;
                    },
                    err => { WriteError(err); }) ? 0 : 1;

                WriteHeader(runInfo, returnCode, startTime, DateTime.Now, em2Config, em3Config, modifiedExtensionSwitches);

                if (returnCode == 1) Console.WriteLine($"{DefGeneral.BRAND_TITLE} run aborted with errors!");
                return returnCode;

                void WriteErr(string guiltyProgramme, string err)
                {
                    WriteError(new Communicator.ErrorInfo() { message = $"{guiltyProgramme}: {err}", isWarning = true });
                }
                void WriteError(Communicator.ErrorInfo err)
                {
                    Console.Error.WriteLine((err.isWarning ? "warning: " : "error: ") + err.message);
                    runInfo.errorInfo.Add(err);
                }
            }
            catch (Exception exception) { return WrongUseError(exception.Message); }
        }

        private static bool HandleExtensionSwitches(Dictionary<string, string> config, out Dictionary<string, string> modifiedExtensionSwitches)
        {
            modifiedExtensionSwitches = new Dictionary<string, string>();
            Dictionary<string, string> toReplace = new Dictionary<string, string>();
            foreach (var entry in config)
            {
                if (!entry.Key.StartsWith(TAGS.EXTENSION_SWITCH)) continue;
                string[] splitVal = entry.Value.Split("="); if (splitVal.Count() < 2) continue;
                string extName = splitVal[0], extVal = splitVal[1];
                modifiedExtensionSwitches.Add(extName, extVal);
                if (!EM_Helpers.IsGuid(extName)) toReplace.Add(entry.Key, extName.ToLower());
            }
            if (toReplace.Count == 0) return true;

            EMPath pathHandler = new EMPath(config[TAGS.CONFIG_PATH_EUROMODFILES]);
            Communicator dummyCommunicator = new Communicator();
            var globExt = ExeXmlReader.ReadExtensions(pathHandler.GetExtensionsFilePath(), dummyCommunicator);

            // first search in global file ...
            List<string> done = new List<string>();
            foreach (var tr in toReplace)
            {
                string extName = tr.Value, configKey = tr.Key;
                // the name of the extension must be replaced by the id: 
                foreach (var e in globExt)
                {
                    string extId = e.Key, shortName = e.Value.Item1.ToLower(), longName = e.Value.Item2.ToLower();
                    if (extName == shortName || extName == longName) { Replace(configKey, extId); break; }
                }
            }
            // ... if any extension-name was not found in global file, search in country file
            if (done.Count == toReplace.Count) return true;
            ExeXml.Country country = ExeXmlReader.ReadCountry(pathHandler.GetCountryFilePath(config[TAGS.CONFIG_COUNTRY]),
                config[TAGS.CONFIG_ID_SYSTEM], config[TAGS.CONFIG_ID_DATA], false, dummyCommunicator);
            foreach (var tr in toReplace)
            {
                string extName = tr.Value, configKey = tr.Key;
                if (done.Contains(configKey)) continue;
                foreach (var e in from ce in country.extensions where ce.Value.localShortName != null select ce)
                {
                    string extId = e.Key, shortName = e.Value.localShortName.ToLower(),
                        longName = e.Value.localLongName; if (longName != null) longName = longName.ToLower();
                    if (extName == shortName || extName == longName) { Replace(configKey, extId); break; }
                }
            }

            if (done.Count == toReplace.Count) return true;
            
            string notDone = string.Empty; foreach (var tr in toReplace) if (!done.Contains(tr.Key)) notDone += tr.Value + " ";
            Console.WriteLine($"Unknown extension(s): {notDone}");
            return false;

            void Replace(string configKey, string extId) { config[configKey] = $"{extId}={config[configKey].Split("=")[1]}"; done.Add(configKey); }
        }

        private static Dictionary<string, string> GetArgsEM2Style(List<string> args)
        {
            if (args.Count != 1 && !(args.Count >= 3 && args.Count <= 5)) return null;
            Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (args.Count == 1) arguments.Add(ARG_CONFIG, args[0]);
            else
            {
                arguments.Add(ARG_EMPATH, args[0]); arguments.Add(ARG_SYS, args[1]); arguments.Add(ARG_DATA, args[2]);
                if (args.Count >= 4) arguments.Add(ARG_DATAPATH, args[3]); if (args.Count >= 5) arguments.Add(ARG_OUTPATH, args[4]);
            }
            return arguments;
        }

        private static Dictionary<string, string> GetArgsEM3Style(List<string> args)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            args.Reverse(); string argValue = null;
            foreach (string arg in args)
            {
                if (arg.StartsWith("-"))
                {
                    if (!arguments.ContainsKey(arg)) arguments.Add(arg, argValue);
                    else arguments.Add(arg + "_" + Guid.NewGuid().ToString(), argValue);
                    argValue = null;
                }
                else { if (argValue != null) return null; argValue = arg; }
            }
            return arguments;
        }

        private static void CreateConfig(string emPath, string sysName, string dataName, out Dictionary<string, string> config)
        {
            config = new Dictionary<string, string>()
            {
                { TAGS.CONFIG_PATH_EUROMODFILES, emPath },
                { TAGS.CONFIG_COUNTRY, sysName.IndexOf('_') < 0 ? "country??" : sysName.Substring(0, sysName.IndexOf('_')) },
                { TAGS.CONFIG_PATH_DATA, new EMPath(emPath).GetFolderInput() },
                { TAGS.CONFIG_PATH_OUTPUT, new EMPath(emPath).GetFolderOutput() },
                { TAGS.CONFIG_ID_SYSTEM, sysName },
                { TAGS.CONFIG_ID_DATA, dataName }
            };
        }

        private static void WriteHeader(RunLogger.RunInfo runInfo, int returnCode, DateTime startTime, DateTime endTime,
                                        List<KeyValuePair<string, string>> em2Config, Dictionary<string, string> em3Config,
                                        Dictionary<string, string> modifiedExtensionSwitches)
        {
            try
            {
                runInfo.duration = new RunLogger.Duration(startTime, endTime);
                runInfo.finishStatus = returnCode == 0 ? RunLogger.RunInfo.FINISH_STATUS.finished : RunLogger.RunInfo.FINISH_STATUS.aborted;
                runInfo.systemName = em3Config[TAGS.CONFIG_ID_SYSTEM]; // if a config-file is used
                runInfo.databaseName = em3Config[TAGS.CONFIG_ID_DATA]; // these may be the ids instead of the names
                runInfo.ExtractAddonSystemNames(em3Config, TAGS.CONFIG_ADDON);
                if (em2Config != null) runInfo.ExtractExtensionSwitches(em2Config, TAGS.EM2CONFIG_POLICY_SWITCH); // user-interface generated config: full extension-switch-info available
                else if (modifiedExtensionSwitches.Count == 0)
                    runInfo.extensionSwitches.Add("All Extension Switches", "default-settings");
                else
                {
                    runInfo.extensionSwitches.AddRange(modifiedExtensionSwitches);
                    runInfo.extensionSwitches.Add("Any other Extension Switches", "default-settings");
                }
                new RunLogger(DefPar.Value.NA, new List<RunLogger.RunInfo>() { runInfo }).TxtWriteEMLog(em3Config[TAGS.CONFIG_PATH_OUTPUT]);
            }
            catch (Exception exception) { Console.WriteLine($"Failed writing log-file: '{exception.Message}'"); }
        }

        private static int WrongUseError(string specificError = null)
        {
            string error = "EM_ExecutableCaller WRONG USAGE!" + Environment.NewLine;
            if (specificError != null) error += specificError + Environment.NewLine; error += Environment.NewLine;
            error += $"Compulsory arguments:" + Environment.NewLine;
            error += $"(1): {ARG_CONFIG}: path to {DefGeneral.BRAND_TITLE} configuration file (e.g. {ARG_CONFIG} \"c:\\Euromod\\EuromodFiles_H0.38\\XMLParam\\Temp\\EMConfigb2d43dc7-92a8-48f8-ac17-42e183069f17.xml\"" + Environment.NewLine;
            error += $"(2): {ARG_EMPATH}: path to {DefGeneral.BRAND_TITLE} project-folder (e.g. {ARG_EMPATH} \"c:\\Euromod\\EuromodFiles_H0.38\"" + Environment.NewLine;
            error += $"     {ARG_SYS}: name of system (e.g. {ARG_SYS} MT_2018)" + Environment.NewLine;
            error += $"     {ARG_DATA}: name of dataset (e.g. {ARG_DATA} MT_2017_a1)" + Environment.NewLine;
            error += $"Optional arguments:" + Environment.NewLine;
            error += $"{ARG_DATAPATH}: path of dataset (e.g. {ARG_DATAPATH} \"c:\\SomeFolder\\EMData\")" + Environment.NewLine;
            error += $"{ARG_OUTPATH}: path where output is written to (e.g. {ARG_OUTPATH} \"c:\\SomeFolder\\EMOutput\")" + Environment.NewLine;
            error += $"{ARG_GLOBALPATH}: path of the global files (e.g. {ARG_GLOBALPATH} \"c:\\SomeFolder\\EuromodFiles_H0.38\\xmlparam\\Config\")" + Environment.NewLine;
            error += $"{ARG_ADDON}: add-on-name|add-on-system (e.g. {ARG_ADDON} MTR|MTR_EL), multiple use possible" + Environment.NewLine;
            error += $"{ARG_EXTENSION_SWITCH}: extension-name=switch (e.g. {ARG_EXTENSION_SWITCH} BTA_??=off), multiple use possible" + Environment.NewLine;
            error += $"{ARG_SEQUENTIAL_RUN}: force sequential run (use a single core to run)" + Environment.NewLine;
            error += $"{ARG_SEQUENTIAL_OUTPUT}: force sequential calculation of the output (slower, but uses less memory)" + Environment.NewLine;
            error += $"{ARG_DATA_PASSWORD}: enables using encrypted data (e.g. {ARG_DATA_PASSWORD} my$tr0ngPass)" + Environment.NewLine;
            // more options to come ...

            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
