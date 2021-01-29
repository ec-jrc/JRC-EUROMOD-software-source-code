using EM_Common;
using EM_Transformer;
using System;
using System.Collections.Generic;
using System.IO;

namespace EM_ExecutableCaller
{
    internal partial class Transform
    {
        
        internal static int GoPet(string[] args)
        {
            try
            {
                if (args.Length < 2) return WrongArgumentError();

                string emPath = EM_Helpers.RemoveQuotes(args[0].Trim());
                string country = args[1];
                bool? transformGlobal = EM_Helpers.GetBool(args[2]);

                // other than for the UI-call, the country-files are saved in the temp-folder, thus we cannot call the standard EM3Country.Transform, etc.
                // as a solution (which is considered to be temporary and therefore should touch other code as less as possible)
                // we do "by hand read-write" (see below) and create an EM3Translation-folder in the temp-folder from where we start the PET-runs

                // here we have the information: EM2 country-file (in temp-folder) and global-files (in config-folder)
                EMPath em2PathHandler = new EMPath(emPath);
                // here we write the EM3 files in a structure the EM3-exe can process, i.e. we create EM3Translation-folder in temp-folder
                EMPath transPathHandler = new EMPath(em2PathHandler.GetFolderTemp());

                List<string> errors = new List<string>();
                bool success = TransformCountry() && (transformGlobal == false || (TransformGlobals() && TransformVariables()));
                foreach (string error in errors) Console.Error.WriteLine(error);
                return success ? 0 : 1;

                bool TransformCountry()
                {
                    // read EM2-files
                    string ccPath = Path.Combine(em2PathHandler.GetFolderTemp(), EMPath.GetCountryFileName(country));
                    string dcPath = Path.Combine(em2PathHandler.GetFolderTemp(), EMPath.GetEM2DataConfigFileName(country));
                    EM2Country.Content ctryContent = EM2Country.Read(ccPath, out List<string> _errors); errors.AddRange(_errors);
                    EM2Data.Content dataContent = EM2Data.Read(dcPath, out _errors); errors.AddRange(_errors);
                    List<List<MultiProp>> extensions = EM2Global.ReadSwitchPol(em2PathHandler.GetFolderConfig(em2: true), out _errors); errors.AddRange(_errors);
                    if (ctryContent == null || dataContent == null || extensions == null) return false;

                    // write EM3-file
                    DirectoryInfo di = Directory.CreateDirectory(transPathHandler.GetCountryFolderPath(country));
                    string em3CountryFile = transPathHandler.GetCountryFilePath(country);
                    bool ok = EM3Country.Write(ctryContent, dataContent, extensions, em3CountryFile, out _errors); errors.AddRange(_errors);
                    return ok;
                }

                bool TransformGlobals()
                {
                    // read EM2-files
                    List<List<MultiProp>> exRates = EM2Global.ReadExRates(em2PathHandler.GetFolderConfig(em2: true), out List<string> _errors); errors.AddRange(_errors);
                    List<List<MultiProp>> hicp = EM2Global.ReadHICP(em2PathHandler.GetFolderConfig(em2: true), out _errors); errors.AddRange(_errors);
                    List<List<MultiProp>> switches = EM2Global.ReadSwitchPol(em2PathHandler.GetFolderConfig(em2: true), out _errors); errors.AddRange(_errors);

                    // transfer to EM3-structure
                    bool ok = true;
                    try
                    {
                        DirectoryInfo di = Directory.CreateDirectory(transPathHandler.GetFolderConfig());
                        if (!EM3Global.WriteExRates(exRates, transPathHandler.GetFolderEuromodFiles(), out _errors)) ok = false; errors.AddRange(_errors);
                        if (!EM3Global.WriteHICP(hicp, transPathHandler.GetFolderEuromodFiles(), out _errors)) ok = false; errors.AddRange(_errors);
                        if (!EM3Global.WriteExtensions(switches, transPathHandler.GetFolderEuromodFiles(), out _errors)) ok = false; errors.AddRange(_errors);
                    }
                    catch { } // this is a primitive way for provide thread-security, as more than one PETs may be started
                    return ok;
                }

                bool TransformVariables()
                {
                    // read EM2-file
                    EM2Variables.Content content = EM2Variables.Read(em2PathHandler.GetFolderConfig(em2: true), out List<string> _errors); errors.AddRange(_errors);
                    if (content == null) return false;

                    // transfer to EM3-structure
                    try
                    {
                        DirectoryInfo di = Directory.CreateDirectory(transPathHandler.GetFolderConfig());
                        bool ok = EM3Variables.Write(content, transPathHandler.GetFolderEuromodFiles(), out _errors); errors.AddRange(_errors);
                        return ok;
                    }
                    catch { return true; } // see above
                }
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine("EUROMOD3 TRANSFORM: " + exception.Message);
                return 1;
            }

            int WrongArgumentError()
            {
                string actArgs = string.Empty; foreach (string arg in args) actArgs += arg + " ";
                Console.Error.WriteLine("EUROMOD3 TRANSFORM: invalid use. Correct: EM_ExecutableCaller UI_TRANSFORM_PET emPath 0/1 cc [ao1[|ao2|...|aoN]]" +
                    Environment.NewLine + $"Acutal use: EM_ExecutableCaller TRANSFORM {actArgs}");
                return 1;
            }
        }
    }
}
