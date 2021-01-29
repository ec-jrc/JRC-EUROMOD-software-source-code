using EM_Common;
using EM_Common_Win;
using EM_Executable;
using EM_Transformer;
using EM_XmlHandler;
using System.Collections.Generic;
using System.IO;

namespace HypotheticalHousehold
{
    internal class ExeCaller
    {
        internal void RunTransfomer(List<string> countries, out List<string> errors)
        {
            errors = new List<string>();
            foreach (string country in countries) EM3Country.Transform(UISessionInfo.GetEuromodFilesFolder(), country, out errors);
            EM3Global.Transform(UISessionInfo.GetEuromodFilesFolder(), out List<string> gErrors); errors.AddRange(gErrors);
            EM3Variables.Transform(UISessionInfo.GetEuromodFilesFolder(), out List<string> vErrors); errors.AddRange(vErrors);
        }
        
        internal bool RunExe(string country, string year, string dataFullPath, bool outputInEuro, out List<string> errors)
        {
            Dictionary<string, string> config = ExeRunConfig.GetBasicConfig(
                UISessionInfo.GetEuromodFilesFolder(), // EurmodFiles-folder
                country, $"{country}_{year}", // country and system
                Path.GetDirectoryName(dataFullPath), Path.GetFileName(dataFullPath), // data-folder and data-file
                UISessionInfo.GetOutputFolder()); // output-folder

            if (outputInEuro) config.Add(TAGS.CONFIG_FORCE_OUTPUT_EURO, DefPar.Value.YES);

            List<string> err = new List<string>();
            bool success = new Control().Run(config, null, e => { err.Add((e.isWarning ? "Warning: " : "Error: ") + e.message); });
            errors = err;

            return success;
        }
    }
}
