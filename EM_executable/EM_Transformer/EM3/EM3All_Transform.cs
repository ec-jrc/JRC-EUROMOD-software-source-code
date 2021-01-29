using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace EM_Transformer
{
    public partial class EM3All
    {
        /// <summary> transfers all components of an EM2 content into EM3 structure </summary>
        /// <param name="emPath"> EuromodFiles folder (containing EM2-files in XMLParam and (will contain) EM3-files in EM3Translation\XMLParam) </param>
        /// <param name="errors"> list of errors, only critical errors lead to returning false, i.e. there may be errors on "success" </param>
        /// <param name="checkEmptyPath"> if true and e3Path is not an empty folder the function returns false and puts an error into the list </param>
        /// <param name="report"> optional function for progress reporting, with the form 'void fName(string successfullyWrittenFile)' </param>
        /// <param name="cancelSrc"> optional cancellation item </param>
        public static bool Transform(string emPath, out List<string> errors, bool checkEmptyPath = false,
                                     Action<string> report = null, CancellationTokenSource cancelSrc = null)
        {
            errors = new List<string>();
            try
            {
                string em3TranslationFolder = new EMPath(emPath).GetFolderEM3Translation();
                if (!Directory.Exists(em3TranslationFolder)) Directory.CreateDirectory(em3TranslationFolder);
                else if (checkEmptyPath)
                {
                    DirectoryInfo di = new DirectoryInfo(em3TranslationFolder);
                    if (di.GetDirectories().Length > 0 || di.GetFiles().Length > 0)
                        throw new Exception($"Destination path is not empty: {em3TranslationFolder}");
                }

                // read EM2 files: includes STRUCTURE changes
                // i.e. essentially from n pol/fun/par for n systems in EM2 to (initially) 1 pol/fun/par for all systems in EM3
                List<string> wErrors = new List<string>();
                EM2All.Content emContent = EM2All.Read(emPath, out errors, report, cancelSrc);

                // write EM3 files: includes CONTENT changes (performed by the EM23Adapt class)
                // e.g. dropping outdated properties, other treatment of private, global switches, ...
                bool success = emContent == null ? false : Write(emContent, emPath, out wErrors, report, cancelSrc);
                errors.AddRange(wErrors);
                return success;
            }
            catch (Exception exception) { errors.Add(exception.Message); return false; }
        }
    }
}
