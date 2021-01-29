using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EM_Transformer
{
    /// <summary> class for reading all EM2 XML-files of a version (countries, variables file, global files, add-ons) </summary>
    public partial class EM2All
    {
         /// <summary> reads all components of an EM2 version </summary>
        /// <param name="emPath"> path to EUROMOD version (e.g. C:\euromod\EuromodContent\EuromodFiles_H0.13\) </param>
        /// <param name="errors"> list of errors, only critical errors lead to failure-return (null), i.e. there may be errors on "success"</param>
        /// <param name="progressAction">
        /// optional function for progress reporting, in the form 'void funName(string message)'
        /// is called for each item (country, add-on, global file) and sets message to e.g. 'Finished reading ...', 'Failed reading ...'
        /// </param>
        /// <param name="cancelSrc"> optional cancellation item </param>
        /// <returns> Content structure upon success, null upon failure </returns>
        public static Content Read(string emPath, out List<string> errors,
                                   Action<string> progressAction = null, CancellationTokenSource cancelSrc = null)
        {
            Content content = new Content();
            List<string> _errors = new List<string>();

            object writeLock = new object();
            ParallelOptions parallelOptions = new ParallelOptions();
            if (cancelSrc != null) parallelOptions.CancellationToken = cancelSrc.Token;

            try
            {
                EMPath pathHandler = new EMPath(emPath);

                // READ COUNTRIES IN PARALLEL 
                Parallel.ForEach(new DirectoryInfo(pathHandler.GetFolderCountries(em2: true)).GetDirectories(), parallelOptions, folder =>
                {
                    parallelOptions.CancellationToken.ThrowIfCancellationRequested();

                    string ctryFileName = pathHandler.GetCountryFilePath(country: folder.Name, em2: true);
                    string dataFileName = pathHandler.GetEM2DataConfigFilePath(country: folder.Name);

                    EM2Country.Content c = EM2Country.Read(ctryFileName, out List<string> cErrors);
                    EM2Data.Content d = EM2Data.Read(dataFileName, out List<string> dErrors);

                    bool success = c != null && d != null;
                    lock (writeLock)
                    {
                        if (success) content.countries.Add(new CountryContent() { country = c, data = d });
                        _errors.AddRange(cErrors); _errors.AddRange(dErrors);
                    }
                    ReportSuccess(folder.Name, success, cErrors.Count + dErrors.Count);
                });
                // the rest is read sequentially (parallel would be unnecessary overhead)

                // READ ADD-ONS
                foreach(DirectoryInfo folder in new DirectoryInfo(pathHandler.GetFolderAddOns(em2: true)).GetDirectories())
                {
                    if ((new List<string>() { "HypoData", "SumStat", "XYZ" }).Contains(folder.Name))
                        continue; // that's just old stuff, I think hardcoding isn't a faux pas
                    string aoFileName = pathHandler.GetAddOnFilePath(addOn: folder.Name, em2: true);
                    EM2Country.Content ao = EM2Country.Read(aoFileName, out List<string> aoErrors);
                    if (ao != null) content.addOns.Add(ao);
                    _errors.AddRange(aoErrors); ReportSuccess(folder.Name, ao != null, aoErrors.Count);
                };

                // READ GLOBAL FILES
                string configPath = pathHandler.GetFolderConfig(em2: true);
                content.exRates = EM2Global.ReadExRates(configPath, out List<string> exErrors); _errors.AddRange(exErrors);
                ReportSuccess("Exchange rates", exErrors.Count == 0, exErrors.Count);
                content.hicp = EM2Global.ReadHICP(configPath, out List<string> hicpErrors); _errors.AddRange(hicpErrors);
                ReportSuccess("HICP", hicpErrors.Count == 0, hicpErrors.Count);
                content.switchPol = EM2Global.ReadSwitchPol(configPath, out List<string> spErrors); _errors.AddRange(spErrors);
                ReportSuccess("Policy switches", spErrors.Count == 0, spErrors.Count);

                // READ VARIABLES
                content.varConfig = EM2Variables.Read(configPath, out List<string> vErrors); _errors.AddRange(vErrors);
                ReportSuccess(EM2Variables.FILE_VARCONFIG, content.varConfig != null, vErrors.Count);

                return content;
            }
            catch (OperationCanceledException) { progressAction($"Reading {emPath} cancelled!"); return content; }
            catch (Exception exception)
            {
                lock (writeLock) { _errors.Add(exception.Message); } return null;
            }
            finally { errors = _errors; }

            void ReportSuccess(string what, bool success, int cntErrors)
            {
                if (success) progressAction($"Finished reading {what} with " + $"{(cntErrors == 0 ? "success" : $"{cntErrors} errors") }");
                else progressAction($"Failed reading {what}");
            }
        }
    }
}
