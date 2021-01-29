using EM_Common;
using EM_UI.Dialogs;
using EM_UI.Tools;
using EM_UI.VariablesAdministration.VariablesManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    internal partial class RVHHLevelVarForm : Form
    {
        private string dataPath, namePattern;
        private List<string> dataToCheck;
        private const string SEPARATOR = "; ";

        internal RVHHLevelVarForm(string _dataPath, string _namePattern, List<string> _customData)
        {
            InitializeComponent();
            dataPath = _dataPath; namePattern = _namePattern; dataToCheck = _customData;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            try
            {
                string path = Path.Combine(EM_AppContext.FolderOutput, "ReleaseValidation_HHVarInfo.txt");
                using (StreamWriter sw = new StreamWriter(path))
                {
                    foreach (DataGridViewRow row in dgvVar.Rows)
                    {
                        string line = $"{row.Cells[0].Value.ToString()}\t";
                        string info = row.Cells[1].Value.ToString();
                        if (!info.Contains(SEPARATOR)) line += info;
                        else foreach (string i in info.Split(SEPARATOR.First())) line += $"{i.Trim()}\t";
                        sw.WriteLine(line.Trim());
                    }
                }
                UserInfoHandler.ShowSuccess($"Saved to {path}");
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private void RVHHLevelVarForm_Shown(object sender, EventArgs e)
        {
            try
            {
                // get all HH-level variables from the variables file
                List<string> hhVars = (from v in EM_AppContext.Instance.GetVarConfigFacade().GetVarConfig().Variable
                                       where VariablesManager.IsHHLevel(v) && v.Name.ToLower() != DefVarName.IDHH.ToLower()
                                       select v.Name.ToLower()).ToList();
                // gather the files to check: those fulfilling the pattern and the custom ones
                if (Directory.Exists(dataPath)) foreach (string f in Directory.GetFiles(dataPath, namePattern)) dataToCheck.Add(Path.Combine(dataPath, f));

                // do the checking, showing a progress bar
                // the background-worker gets 2 parameters:
                // - the list of HH-level variables and
                // - the list of files to check
                ProgressIndicator progressIndicator = new ProgressIndicator(CheckData_BackgroundEventHandler, "Checking Data ...", 
                                                                            new Tuple<List<string>, List<string>>(hhVars, dataToCheck));
                if (progressIndicator.ShowDialog() != DialogResult.OK) { Close(); return; } // user pressed Cancel

                // interpret the results
                // the background-worker returns 2 results:
                // - the list of variables, each variable with a list of the file(s) that caused problems, and an empty list if there are no problems
                // - a (hopefully empty) list of critical errors, i.e. one for each file that could not be analysed
                Dictionary<string, List<string>> hhVarResults = (progressIndicator.Result as Tuple<Dictionary<string, List<string>>, List<string>>).Item1;
                List<string> problemData = (progressIndicator.Result as Tuple<Dictionary<string, List<string>>, List<string>>).Item2;

                if (problemData.Count > 0)
                {
                    UserInfoHandler.ShowError("The following data could not be checked:" + Environment.NewLine + string.Join(Environment.NewLine, problemData));
                    if (problemData.Count == dataToCheck.Count) { Close(); return; } // checking failed for all data-files
                }

                // show the results in the grid
                foreach (var result in hhVarResults)
                {
                    string problems = result.Value.Count == 0 ? "No problems found for the checked files" : string.Join(SEPARATOR, result.Value);
                    dgvVar.Rows.Add(result.Key, problems);
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); Close(); }
        }

        private static void CheckData_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            List<string> hhVars = (e.Argument as Tuple<List<string>, List<string>>).Item1;
            List<string> dataToCheck = (e.Argument as Tuple<List<string>, List<string>>).Item2;

            Dictionary<string, List<string>> hhVarResults = new Dictionary<string, List<string>>();
            foreach (string hhVar in hhVars) hhVarResults.Add(hhVar, new List<string>());
            List<string> problemData = new List<string>();

            for (int i = 0; i < dataToCheck.Count; ++i)
            {
                try
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } // user pressed Cancel
                    var dataLines = File.ReadLines(dataToCheck[i]);

                    if (dataLines.Count() <= 1) { AddErr("Empty file"); continue; }
                    List<string> headerLine = dataLines.First().ToLower().Split('\t').ToList();
                    if (!headerLine.Contains(DefVarName.IDHH)) { AddErr($"{DefVarName.IDHH} not found"); continue; }
                    int indexIdHH = headerLine.IndexOf(DefVarName.IDHH);

                    Dictionary<int, string> curVals = new Dictionary<int, string>();
                    foreach (string hhVar in hhVars) if (headerLine.Contains(hhVar)) curVals.Add(headerLine.IndexOf(hhVar), string.Empty);
                    List<int> indexHHVars = curVals.Keys.ToList();

                    string curIdHH = null;
                    const string varFoundFaulty = "---";
                    foreach (string dl in dataLines.Skip(1))
                    {
                        string[] dataLine = dl.Split('\t');
                        if (dataLine.Count() < curVals.Count + 1) { AddErr($"Too short line found"); continue; }
                        foreach (int indexHHVar in indexHHVars)
                        {
                            if (curVals[indexHHVar] == varFoundFaulty) continue; // variable is already found to be faulty, see below

                            if (dataLine[indexIdHH] != curIdHH) // first person in HH: assess the values of the HH-level-variables ...
                            {                               
                                curVals[indexHHVar] = dataLine[indexHHVar];
                            }
                            else // ... all other persons in HH: compare whether values are equal
                            {
                                if (curVals[indexHHVar] != dataLine[indexHHVar])
                                {
                                    string varName = headerLine[indexHHVar];
                                    hhVarResults[varName].Add(Path.GetFileName(dataToCheck[i]));
                                    curVals[indexHHVar] = varFoundFaulty; // do not check any further HHs, as there is no detailed error report (we just state that the data isn't ok for this variable)
                                }
                            }
                        }
                        curIdHH = dataLine[indexIdHH];
                    }

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (dataToCheck.Count * 1.0) * 100.0));
                }
                catch (Exception exception) { AddErr(exception.Message); }

                void AddErr(string err)
                {
                    string error = $"{dataToCheck[i]}: {err}";
                    if (!problemData.Contains(error)) problemData.Add(error); // the check for adding the same error is actually only necessary for too short lines (see above)
                }
            }

            e.Result = new Tuple<Dictionary<string, List<string>>, List<string>>(hhVarResults, problemData);
        }
    }
}
