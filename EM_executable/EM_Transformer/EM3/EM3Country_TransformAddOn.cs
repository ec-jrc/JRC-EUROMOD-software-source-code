using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_Transformer
{
    public partial class EM3Country
    {
        /// <summary>
        /// reads an AddOn's EM2 country-XML-files and transfers it to EM3 style
        /// creates the AddOn folder, if it does not exist, and overwrites any existing country AddOn
        /// note: the intended usage is "single AddOn transformation"
        ///       the EM3All class is responsible for complete EM-content transformation (using the EM3Country.Write function)
        /// </summary>
        /// <param name="emPath"> EuromodFiles folder (containing EM2-files in XMLParam and (will contain) EM3-files in EM3Translation\XMLParam) </param>
        /// <param name="addOn"> short-name of AddOn </param>
        /// <param name="errors"> critical and non-critical erros during the transformation-process, empty structure for no errors </param>
        public static bool TransformAddOn(string emPath, string addOn, out List<string> errors)
        {
            errors = new List<string>(); EMPath pathHandler = new EMPath(emPath);
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(pathHandler.GetAddOnFolderPath(addOn));

                // read EM2-files
                string em2AddOnFile = pathHandler.GetAddOnFilePath(addOn: addOn, em2: true);

                if (TransformerCommon.IsFileUpToDate(em2AddOnFile, pathHandler.GetAddOnFolderPath(addOn), out string hashCode)) return true;

                EM2Country.Content addOnContent = EM2Country.Read(em2AddOnFile, out errors);
                if (addOnContent == null) return false;

                // write EM3-file (includes EM2->EM3 adaptations, via EM23Adapt class)
                string em3AddOnFile = pathHandler.GetAddOnFilePath(addOn);
                bool success = Write(addOnContent, null, new List<List<MultiProp>>(), em3AddOnFile, out List<string> wErrors);
                errors.AddRange(wErrors);

                if (success && errors.Count == 0) TransformerCommon.WriteUpToDate(em2AddOnFile, pathHandler.GetAddOnFolderPath(addOn), hashCode);
                return success;
            }
            catch (Exception exception)
            {
                errors.Add($"{addOn}: {exception.Message}");
                return false;
            }
        }
    }
}
