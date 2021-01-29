using System;
using System.Linq;

namespace EM_ExecutableCaller
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                // call from the user interface's RunTool:
                // sytax: EM_ExecutableCaller UI_RUNTOOL_CALL pathToEMConfig
                // example: EM_ExecutableCaller UI_RUNTOOL_CALL c:\Euromod\EuromodFiles_H0.38 BG\XMLParam\Temp\EMConfig0f2e0b89-3200-4cf6-a651-080b27b3434b.xml
                if (args.Length > 0 && args[0] == "UI_RUNTOOL_CALL") return UIRunToolCall.Go(RemoveFirstArg());

                // start EM3 executable from command-line or e.g. Stata
                // 4 possibilities (<...> to be filled by users):
                // (1) EM_ExecutableCaller -conf <configFilePath>
                // (2) EM_ExecutableCaller -emPath <EuromodFilesPath> -sys <sysName> -data <dataName> [-outPath <outputPath>] [-dataPath <dataPath>]
                // (3) EM_ExecutableCaller <configFilePath>
                // (4) EM_ExecutableCaller <EuromodFilesPath> <sysName> <dataName> [<outPath>] [<dataPath>]
                // note: unnamed-option-versions (3) and (4) aim to replicated the behaviour of the EM2 executable
                //       named-option-versions (2) and maybe (1) are planned to be equipped with further (named) options in future
                return EuromodCall.Go(args);
            }
            catch (Exception exception) { Console.WriteLine(exception.Message); return 0; }

            string[] RemoveFirstArg() { return args.Where(w => w != args[0]).ToArray(); }
        }
    }
}
