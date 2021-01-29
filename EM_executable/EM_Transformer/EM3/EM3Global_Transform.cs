using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_Transformer
{
    public static partial class EM3Global
    {
        /// <summary>
        /// reads EM2 global files (for ex-rates, hicp, glo-switches) and transfers them to EM3 style
        /// creates the config folder, if it does not exist, and overwrites any existing global file
        /// note: the intended usage is "single transformation of global files"
        ///       the EM3All class is responsible for complete EM-content transformation (using the EM3Global.WriteXXX functions)
        /// </summary>
        /// <param name="emPath"> EuromodFiles folder (containing EM2-files in XMLParam and (will contain) EM3-files in EM3Translation\XMLParam) </param>
        /// <param name="errors"> critical and non-critical erros during the transformation-process, empty structure for no errors </param>
        /// <param name="mustExist"> if false, the non-existence of any global file does not produce an error </param>
        public static bool Transform(string emPath, out List<string> errors, bool mustExist = false)
        {
            errors = new List<string>(); EMPath pathHandler = new EMPath(emPath);
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(pathHandler.GetFolderConfig());

                // read EM2-files
                List<List<MultiProp>> exRates = EM2Global.ReadExRates(pathHandler.GetFolderConfig(em2: true), out List<string> _errors, mustExist); errors.AddRange(_errors);
                List<List<MultiProp>> hicp = EM2Global.ReadHICP(pathHandler.GetFolderConfig(em2: true), out _errors, mustExist); errors.AddRange(_errors);
                List<List<MultiProp>> switches = EM2Global.ReadSwitchPol(pathHandler.GetFolderConfig(em2: true), out _errors, mustExist); errors.AddRange(_errors);

                // transfer to EM3-structure
                bool success = WriteExRates(exRates, emPath, out _errors); errors.AddRange(_errors);
                success &= WriteHICP(hicp, emPath, out _errors); errors.AddRange(_errors);
                success &= WriteExtensions(switches, emPath, out _errors); errors.AddRange(_errors);

                return success;
            }
            catch (Exception exception)
            {
                errors.Add($"Global files: {exception.Message}");
                return false;
            }
        }
    }
}
