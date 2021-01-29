using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EM_Transformer
{
    public partial class EM3All
    {
        /// <summary> note: this is a private function, exclusively called by EM3All.Transform, just for reasons of clearer arrangement </summary>
        private static bool Write(EM2All.Content content, string emPath, out List<string> errors,
                                  Action<string> report = null, CancellationTokenSource cancelSrc = null)
        {
            List<string> _errors = new List<string>();

            object errWriteLock = new object();
            ParallelOptions parallelOptions = new ParallelOptions();
            if (cancelSrc != null) parallelOptions.CancellationToken = cancelSrc.Token;

            try
            {
                EMPath pathHandler = new EMPath(emPath);

                // WRITE COUNTRIES IN PARALLEL 
                Parallel.ForEach(content.countries, parallelOptions, country =>
                {
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                    string countryName = country.country.general.properties[EM2TAGS.SHORTNAME];
                    DirectoryInfo di = Directory.CreateDirectory(pathHandler.GetCountryFolderPath(countryName));
                    bool success = EM3Country.Write(country.country, country.data, content.switchPol,
                                                    pathHandler.GetCountryFilePath(countryName), out List<string> cErrors);
                    if (cErrors.Count > 0) lock (errWriteLock) { _errors.AddRange(cErrors); }
                    ReportSuccess(countryName, success, cErrors.Count);

                    if (success && cErrors.Count == 0) // produce up2Date-files
                    {
                        TransformerCommon.WriteUpToDate(pathHandler.GetCountryFilePath(countryName, true), pathHandler.GetCountryFolderPath(countryName));
                        TransformerCommon.WriteUpToDate(pathHandler.GetEM2DataConfigFilePath(countryName), pathHandler.GetCountryFolderPath(countryName));
                    }
                });
                // the rest is written sequentially (parallel would be unnecessary overhead)

                // WRITE ADD-ONS
                foreach (var addOn in content.addOns)
                {
                    string addOnName = addOn.general.properties[EM2TAGS.SHORTNAME];
                    DirectoryInfo di = Directory.CreateDirectory(pathHandler.GetAddOnFolderPath(addOnName));
                    bool success = EM3Country.Write(addOn, null, content.switchPol,
                                                    pathHandler.GetAddOnFilePath(addOnName), out List<string> aoErrors);
                    if (aoErrors.Count > 0) _errors.AddRange(aoErrors);
                    ReportSuccess(addOnName, success, aoErrors.Count);

                    // produce up2Date-file
                    if (success && aoErrors.Count == 0) TransformerCommon.WriteUpToDate(pathHandler.GetAddOnFilePath(addOnName, true), pathHandler.GetAddOnFolderPath(addOnName));
                }

                // WRITE GLOBAL FILES
                if (!Directory.Exists(pathHandler.GetFolderConfig())) Directory.CreateDirectory(pathHandler.GetFolderConfig());
                bool erSuccess = EM3Global.WriteExRates(content.exRates, emPath, out List<string> erErrors);
                ReportSuccess("Exchangerates", erSuccess, erErrors.Count); _errors.AddRange(erErrors);
                bool hicSuccess = EM3Global.WriteHICP(content.hicp, emPath, out List<string> hicErrors);
                ReportSuccess("HICP", hicSuccess, hicErrors.Count); _errors.AddRange(hicErrors);
                bool gsSuccess = EM3Global.WriteExtensions(content.switchPol, emPath, out List<string> gsErrors);
                ReportSuccess("Extensions", gsSuccess, gsErrors.Count); _errors.AddRange(gsErrors);

                // WRITE VARIABLES
                bool varSuccess = EM3Variables.Write(content.varConfig, emPath, out List<string> vErrors);
                ReportSuccess("Variables", varSuccess, vErrors.Count); _errors.AddRange(vErrors);
                if (varSuccess && vErrors.Count == 0) TransformerCommon.WriteUpToDate(pathHandler.GetVarFilePath(true), pathHandler.GetFolderConfig()); // produce up2Date-file

                return true;
            }
            catch (OperationCanceledException) { report($"Writing {emPath} cancelled!"); return true; }
            catch (Exception exception)
            {
                lock (errWriteLock) { _errors.Add(exception.Message); }
                return false;
            }
            finally { errors = _errors; }

            void ReportSuccess(string what, bool success, int cntErrors)
            {
                if (success) report($"Finished writing {what} with " + $"{(cntErrors == 0 ? "success" : $"{cntErrors} errors") }");
                else report($"Failed writing {what}");
            }
        }
    }
}
