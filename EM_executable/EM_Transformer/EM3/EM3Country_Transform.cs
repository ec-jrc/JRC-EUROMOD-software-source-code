using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_Transformer
{   
    public partial class EM3Country
    {
        /// <summary>
        /// reads a country's EM2 country- and dataconfig-XML-files and transfers them to EM3 style
        /// creates the country folder, if it does not exist, and overwrites any existing country file
        /// note: the intended usage is "single country transformation"
        ///       the EM3All class is responsible for complete EM-content transformation (using the EM3Country.Write function)
        /// </summary>
        /// <param name="emPath"> EuromodFiles folder (containing EM2-files in XMLParam and (will contain) EM3-files in EM3Translation\XMLParam) </param>
        /// <param name="country"> short-name of country </param>
        /// <param name="errors"> critical and non-critical erros during the transformation-process, empty structure for no errors </param>
        public static bool Transform(string emPath, string country, out List<string> errors)
        {
            errors = new List<string>(); EMPath pathHandler = new EMPath(emPath);
            string em2CountryFile = string.Empty, em2DataFile = string.Empty;
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(pathHandler.GetCountryFolderPath(country));

                // read EM2-files
                em2CountryFile = pathHandler.GetCountryFilePath(country: country, em2: true);
                em2DataFile = pathHandler.GetEM2DataConfigFilePath(country);

                bool up2D1 = TransformerCommon.IsFileUpToDate(em2CountryFile, pathHandler.GetCountryFolderPath(country), out string hash1);
                bool up2D2 = TransformerCommon.IsFileUpToDate(em2DataFile, pathHandler.GetCountryFolderPath(country), out string hash2);
                if (up2D1 && up2D2) return true; // do not combine in one if, to make sure that both hash-files (for country and dataconfig) are generated

                EM2Country.Content ctryContent = EM2Country.Read(em2CountryFile, out List<string> cErrors);
                EM2Data.Content dataContent = EM2Data.Read(em2DataFile, out List<string> dErrors);
                // need the global file with policy-switches for proper transformation of local policy switches
                List<List<MultiProp>> extensions = EM2Global.ReadSwitchPol(pathHandler.GetFolderConfig(em2: true), out List<string> gErrors);
                errors.AddRange(cErrors); errors.AddRange(dErrors); errors.AddRange(gErrors);
                if (ctryContent == null || dataContent == null || extensions == null) return false;

                // write EM3-file (includes EM2->EM3 adaptations, via EM23Adapt class)
                string em3CountryFile = pathHandler.GetCountryFilePath(country);
                bool success = Write(ctryContent, dataContent, extensions, em3CountryFile, out List<string> wErrors);
                errors.AddRange(wErrors);

                if (success && errors.Count == 0)
                {
                    TransformerCommon.WriteUpToDate(em2CountryFile, pathHandler.GetCountryFolderPath(country), hash1);
                    TransformerCommon.WriteUpToDate(em2DataFile, pathHandler.GetCountryFolderPath(country), hash2);
                }
                    
                return success;
            }
            catch (Exception exception)
            {
                errors.Add($"{country}: {exception.Message}");
                return false;
            }
        }

        public static bool IsUpToDate(string emPath, string country)
        {
            EMPath pathHandler = new EMPath(emPath);
            return TransformerCommon.IsFileUpToDate(pathHandler.GetCountryFilePath(country, true), pathHandler.GetCountryFolderPath(country), out string hash1) &&
                   TransformerCommon.IsFileUpToDate(pathHandler.GetEM2DataConfigFilePath(country), pathHandler.GetCountryFolderPath(country), out string hash2);
        }
    }
}
