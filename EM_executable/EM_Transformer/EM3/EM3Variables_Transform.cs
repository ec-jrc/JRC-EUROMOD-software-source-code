using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_Transformer
{
    public partial class EM3Variables
    {
        /// <summary>
        /// reads the EM2 variables file and transfers it to EM3 style
        /// creates the config folder, if it does not exist, and overwrites any existing variables file
        /// note: the intended usage is "single variables transformation"
        ///       the EM3All class is responsible for complete EM-content transformation (using the EM3Variables.Write functions)
        /// </summary>
        /// <param name="emPath"> EuromodFiles folder (containing EM2-files in XMLParam and (will contain) EM3-files in EM3Translation\XMLParam) </param>
        /// <param name="errors"> critical and non-critical erros during the transformation-process, empty structure for no errors </param>
        public static bool Transform(string emPath, out List<string> errors)
        {
            errors = new List<string>(); EMPath pathHandler = new EMPath(emPath);
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(pathHandler.GetFolderConfig());

                if (TransformerCommon.IsFileUpToDate(pathHandler.GetVarFilePath(em2: true), di.FullName, out string hashCode)) return true;

                // read EM2-file
                EM2Variables.Content content = EM2Variables.Read(pathHandler.GetFolderConfig(em2: true), out List<string> rErrors);
                if (content == null) { errors = rErrors; return false; }

                // transfer to EM3-structure
                bool success = Write(content, emPath, out List<string> wErrors);
                errors.AddRange(wErrors);

                if (success && errors.Count == 0) TransformerCommon.WriteUpToDate(pathHandler.GetVarFilePath(em2: true), di.FullName, hashCode);
                return success;
            }
            catch (Exception exception)
            {
                errors.Add($"Variables: {exception.Message}");
                return false;
            }
        }
    }
}
