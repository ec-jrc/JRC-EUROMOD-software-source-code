using EM_BackEnd;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace EM_Statistics
{
    public class EM_StatisticsBackEndResponder : EM_BackEndResponder
    {
        public string urlBack = null;

        public const string LOAD_STATISTICS_HTML = "LoadStatisticsHtml";
        public const string LOAD_STATISTICS = "LoadStatistics";
        public const string SAVE_AS_EXCEL = "SaveAsExcel";

        private class ResultsPackage
        {
            internal string packageKey = null;
            internal DisplayResults displayResults = null;
            internal List<StringBuilder> microData = null;
            internal string error = null;
            //internal int FilePackageIndex = -1; // used in ReplaceRunTimeCaptions
        }

        private BackEnd backEnd = null;
        private Template template = null;
        private List<FilePackageContent> filePackages = null;
        private List<Template.TemplateInfo.UserVariable> userInput = null;
        private List<ResultsPackage> resultsPackages = new List<ResultsPackage>();
        internal Dictionary<string, EM_TemplateCalculator> calculators = new Dictionary<string, EM_TemplateCalculator>();

        public EM_StatisticsBackEndResponder(BackEnd _backEnd, Template _template, List<FilePackageContent> _filePackages,
                                             List<Template.TemplateInfo.UserVariable> _userInput = null,
                                             string responderKey = null) : base(responderKey)
        {
            backEnd = _backEnd; template = _template; filePackages = _filePackages; userInput = _userInput;
            if (userInput != null) template.info.TakeUserInput(userInput);
            PrepareAndStartCalculatingResults();
            
            // Do NOT add the responses until you are ready to respond! These should be at the end of the constructor... ;)
            if (!responses.ContainsKey(LOAD_STATISTICS_HTML)) responses.Add(LOAD_STATISTICS_HTML, BuildResponse_LoadStatisticsHtml);
            if (!responses.ContainsKey(LOAD_STATISTICS)) responses.Add(LOAD_STATISTICS, BuildResponse_LoadStatistics);
            if (!responses.ContainsKey(SAVE_AS_EXCEL)) responses.Add(SAVE_AS_EXCEL, BuildResponse_SaveAsExcel);
        }

        private void PrepareAndStartCalculatingResults()
        {
            foreach (FilePackageContent filePackage in filePackages)    // tested multi-threading this, but there was no noticeable gain!
            {
                List<string> filenames = new List<string>();
                // Prepare the filenames and microdata
                List<StringBuilder> microData = new List<StringBuilder>();

                if (template.info.templateType != HardDefinitions.TemplateType.Multi)
                {
                    filenames.Add(filePackage.PathBase); 
                    microData.Add(null);
                }

                for (int f = 0; f < Math.Max(filePackage.PathsAlt.Count, filePackage.MicroDataAlt.Count); ++f)
                {
                    filenames.Add(f < filePackage.PathsAlt.Count ? filePackage.PathsAlt[f] : string.Empty);
                    microData.Add(f < filePackage.MicroDataAlt.Count ? filePackage.MicroDataAlt[f] : null);
                }
                if (filePackage.MicroDataAlt.Count == 0) microData = null;

                // Prepare the resultsPackage
                ResultsPackage resultsPackage = new ResultsPackage();
                EM_TemplateCalculator calculator = new EM_TemplateCalculator(template, filePackage.Key);
                bool prepared = calculator.Prepare(filenames, out ErrorCollector errorCollector);
                calculators.Add(filePackage.Key, calculator);
                resultsPackage.displayResults = calculator.GetDisplayResults();
                if (!prepared) resultsPackage.displayResults.calculated = true; // if preperation went wrong, there will not be any results comming later!
                resultsPackage.packageKey = filePackage.Key;
                resultsPackage.error = errorCollector.GetErrorMessage();
                resultsPackage.microData = microData;
                resultsPackages.Add(resultsPackage);
            }

            new System.Threading.Thread(() =>
            {
                Parallel.ForEach<FilePackageContent>(filePackages, filePackage =>
                {
                    CalcPackage(filePackage);
                });
            }).Start();
        }

        private void CalcPackage(FilePackageContent filePackage)
        {
            // The package must ALWAYS exist at this point! 
            EM_TemplateCalculator calculator = calculators[filePackage.Key];
            ResultsPackage resultsPackage = resultsPackages.Find(x => x.packageKey == filePackage.Key);
            try
            {
                bool success = calculator.CalculateStatistics(resultsPackage.microData);
                lock (resultsPackages)
                {
                    resultsPackage.displayResults = calculator.GetDisplayResults();
                    resultsPackage.error = calculator.GetErrorMessage(); // this may also just be warnings
                    if (!success) resultsPackage.displayResults.calculated = true;  // if did not succeed, declare this done
                    // cleanup
                    calculators.Remove(filePackage.Key);
                    resultsPackage.microData = null;
                    filePackage = null;
                }
            }
            catch (Exception exception)
            {
                lock (resultsPackages)
                {
                    resultsPackage.error = exception.Message;
                }
            }
        }
        private void BuildResponse_LoadStatistics(HttpContext context, EM_BackEndResponder beResponder)
        {
            try
            {
                EM_StatisticsBackEndResponder me = beResponder as EM_StatisticsBackEndResponder;
                context.Response.ContentType = "text/json";

                if (resultsPackages == null || resultsPackages.Count == 0)
                { BackEnd.WriteResponseError(context, "No files for analysing defined!"); return; }

                bool hasForm = context.Request.HasFormContentType && context.Request.Form.Count > 0;
                string packageKey = hasForm && context.Request.Form.ContainsKey("packageKey") ? context.Request.Form["packageKey"].ToString() : resultsPackages[0].packageKey;

                ResultsPackage resultsPackage = (from rp in resultsPackages.ToList() where rp.packageKey == packageKey select rp).FirstOrDefault(); // .ToList() here is extremely important for thread-safety

                // package does not yet exist!
                if (resultsPackage == null) 
                {
                    BackEnd.WriteResponseError(context, "No Results?!? This should never happen!"); return;
                    //                    FilePackageContent filePackage = (from fp in filePackages where fp.Key == packageKey select fp).FirstOrDefault();
                    //                    if (filePackage == null) { BackEnd.WriteResponseError(context, $"Unknown package-key: '{packageKey}'!"); return; }
                    //                    resultsPackage = CalcPackage(filePackage); resultsPackages.Add(resultsPackage);
                }
                // statistics not yet calculated
                if (!resultsPackage.displayResults.calculated)
                {
                    DisplayResults results = new DisplayResults() { info = resultsPackage.displayResults.info };
                    LoadInfo loadInfo = new LoadInfo() { packageKey = packageKey, completed = false, results = results };
                    context.Response.WriteAsync(JsonConvert.SerializeObject(loadInfo));
                    return;
                }
                    
                if (resultsPackage.displayResults != null)
                {
                    LoadInfo loadInfo = new LoadInfo() { packageKey = packageKey, completed = true };
                    string json = "";
                    lock (resultsPackages)
                    {
                        loadInfo.results = resultsPackage.displayResults;
                        loadInfo.warnings = resultsPackage.error;
                        json = JsonConvert.SerializeObject(loadInfo);
                    }
                    context.Response.WriteAsync(json);
                }
                else BackEnd.WriteResponseError(context, resultsPackage.error);
            }
            catch (Exception exception) { BackEnd.WriteResponseError(context, exception.Message); }
        }

        private void BuildResponse_LoadStatisticsHtml(HttpContext context, EM_BackEndResponder beResponder)
        {
            try
            {
                context.Response.ContentType = "text/html";
                bool showButtons = !template.info.HideMainSelectorForSingleFilePackage || filePackages.Count > 1;
                string path = context.Request.Path.ToUriComponent();
                path = path.Substring(0, path.LastIndexOf("/"));
                string html = Resources.statistics_html
                    .Replace(BackEndResourceProvider.MESSAGEBOX_HTML, BackEndResourceProvider.Get_MessageBox_html())
                    .Replace(BackEndResourceProvider.STRINGINPUTBOX_HTML, BackEndResourceProvider.Get_StringInputBox_html())
                    .Replace("PLACEHOLDER_RESPONDER_KEY", beResponder.responderKey)
                    .Replace("PLACEHOLDER_SHOW_BUTTONS", showButtons ? "initial" : "none")
                    .Replace("PLACEHOLDER_SHOW_BACK", urlBack != null ? "inline-block" : "none")
                    .Replace("PLACEHOLDER_URL_BACK", urlBack != null ? path + urlBack : string.Empty);
                

                string contents = string.Empty;
                foreach (ResultsPackage resultsPackage in resultsPackages)
                {
                    contents += Resources.Content_html.Replace("PLACEHOLDER_PACKAGE_KEY", resultsPackage.packageKey);
                }
                html = html.Replace("PLACEHOLDER_CONTENTS", contents); 

                if (showButtons)
                {
                    string buttons = string.Empty;
                    foreach (ResultsPackage resultsPackage in resultsPackages)
                        buttons += Resources.Button_html
                            .Replace("PLACEHOLDER_PACKAGE_KEY", resultsPackage.packageKey)
                            .Replace("PLACEHOLDER_LINK", $"ShowStatsContent('{resultsPackage.packageKey}');")
                            .Replace("PLACEHOLDER_TEXT", resultsPackages.Find(x => x.packageKey == resultsPackage.packageKey).displayResults.info.button);
                    html = html.Replace("PLACEHOLDER_BUTTONS", buttons);
                }

                context.Response.WriteAsync(html);
            }
            catch (Exception exception) { BackEnd.WriteResponseError(context, exception.Message); }
        }

        private void BuildResponse_SaveAsExcel(HttpContext context, EM_BackEndResponder beResponder)
        {
            try
            {
                EM_StatisticsBackEndResponder me = beResponder as EM_StatisticsBackEndResponder;

                if (resultsPackages == null || resultsPackages.Count == 0)
                { BackEnd.WriteResponseError(context, "No files for analysing defined!"); return; }

                List<ResultsPackage> allResults = new List<ResultsPackage>();

                if (context.Request.Query.ContainsKey("packageKey"))
                {
                    string packageKey = context.Request.Query["packageKey"].First().ToString();
                    ResultsPackage resultsPackage = (from rp in resultsPackages.ToList() where rp.packageKey == packageKey select rp).FirstOrDefault(); // .ToList() here is extremely important for thread-safety
                    if (resultsPackage == null)
                    {
                        BackEnd.WriteResponseError(context, "Package not found!"); 
                        return;
                    }
                    allResults.Add(resultsPackage);
                }
                else
                {
                    allResults.AddRange(resultsPackages.ToList());
                }

                if (allResults.Count < 1)
                {
                    BackEnd.WriteResponseError(context, "No Results found!"); 
                    return;
                }

                bool allReady = true;
                foreach (ResultsPackage resultsPackage in allResults)
                    if (resultsPackage.displayResults == null || !resultsPackage.displayResults.calculated)
                        allReady = false;

                if (allReady)
                {
                    MemoryStream excelStream;
                    string errMsg = string.Empty;
                    foreach (ResultsPackage resultsPackage in allResults)
                        errMsg += (errMsg == string.Empty ? "" : Environment.NewLine) + resultsPackage.error;

                    context.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    if (allResults.Count == 1)
                    {
                        context.Response.Headers.Append("Content-Disposition", $"attachment; filename={allResults[0].displayResults.info.button}.xlsx");
                        ExportHandling.ExportSinglePackage(allResults[0].displayResults, out errMsg, out excelStream);
                    }
                    else
                    {
                        context.Response.Headers.Append("Content-Disposition", $"attachment; filename={template?.info?.name ?? "EM_Statistics_export"}.xlsx");
                        List<DisplayResults> allDisplayResults = new List<DisplayResults>();
                        foreach (ResultsPackage resultsPackage in allResults)
                            allDisplayResults.Add(resultsPackage.displayResults);
                        ExportHandling.ExportMultiPackages(allDisplayResults, out errMsg, out excelStream);
                    }

                    context.Response.Body.WriteAsync(excelStream.ToArray(), 0, (int)excelStream.Length);
                    return;
                }
                else
                {
                    System.Threading.Thread.Sleep(500);
                    BuildResponse_SaveAsExcel(context, beResponder);
                    return;
                }

            }
            catch (Exception exception) { BackEnd.WriteResponseError(context, exception.Message); }
        }

        public override bool BuildResponse_FromResource(HttpContext context, string r)
        {
            switch (r)
            {
                case "LoadStatistics.js":
                    context.Response.ContentType = "text/javascript";
                    context.Response.WriteAsync(Resources.LoadStatistics_js); return true;
            }
            return false;
        }

        public class LoadInfo
        {
            public DisplayResults results;
            public string warnings = string.Empty;
            public string packageKey = string.Empty;
            public bool completed = false;
        }
        public class ExcelInfo
        {
            public byte[] excelContent = null;
            public string packageKey = string.Empty;
            public string filename = string.Empty;
            public string warnings = string.Empty;
            public bool completed = false;
        }
    }
}
