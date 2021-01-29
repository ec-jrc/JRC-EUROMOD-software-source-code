using EM_Common_Win;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace VariablesChecker
{
    public partial class VariablesCheckerForm : Form
    {
        public VariablesCheckerForm()
        {
            InitializeComponent();
        }

        private void DoCheck(Func<List<string>, string> checkAction, string outFile)
        {
            Cursor = Cursors.WaitCursor;
            List<string> print = new List<string>();
            string message = checkAction(print);
            if (print.Count > 0)
            {
                Print(print, outFile);
                message += Environment.NewLine + Environment.NewLine + "Find check-results in " + Environment.NewLine + GetOutPath(outFile);
            }
            Cursor = Cursors.Default;
            MessageBox.Show(message);
        }

        private void Print(List<string> print, string outFile)
        {
            if (!Directory.Exists(GetOutPath())) Directory.CreateDirectory(GetOutPath());
            using (StreamWriter sw = new StreamWriter(GetOutPath(outFile)))
                foreach (string p in print) sw.WriteLine(p);
        }

        private string GetOutPath(string outFile = null)
        {
            string resDir = "ResultsVariablesCheck";
            return outFile == null ? Path.Combine(UISessionInfo.GetOutputFolder(), resDir)
                                   : Path.Combine(UISessionInfo.GetOutputFolder(), resDir, outFile + ".txt");
        }

        private void btnVarDuplicates_Click(object sender, EventArgs e) { DoCheck(Check.VarDuplicates, "VarDuplicates"); }

        private void btnVarAcroTwins_Click(object sender, EventArgs e) { DoCheck(Check.AcroTwins, "AcroTwins"); }

        private void btnVarNaming_Click(object sender, EventArgs e) { DoCheck(Check.VarNaming, "VarNaming"); }

        private void btnUnusedAcros_Click(object sender, EventArgs e) { DoCheck(Check.UnusedAcros, "UnusedAcros"); }

        private void btnDescFormatting_Click(object sender, EventArgs e) { DoCheck(Check.DescFormatting, "DescriptionFormatting"); }

        private void btnAcroDuplicates_Click(object sender, EventArgs e) { DoCheck(Check.AcroDuplicates, "AcroDuplicates"); }

        // note: the progress-bar and cancel-button are just a bit of playing and not at all perfect
        // (e.g. allow for clicking everywhere, not just the cancel-button) while parallel process is running ...)

        private void btnVarUsage_Click(object sender, EventArgs e)
        {
            progressBar.Visible = true;
            btnCancelParallel.Visible = true;
            backgroundWorker.RunWorkerAsync();
        }

        private void btnCancelParallel_Click(object sender, EventArgs e) { backgroundWorker.CancelAsync(); }

        private void backgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            List<string> print = new List<string>();
            string message = Check.VarUsage(print, backgroundWorker), outFile = "VarUsage";
            if (print.Count > 0)
            {
                Print(print, outFile);
                message += Environment.NewLine + Environment.NewLine + "Find check-results in " + Environment.NewLine + GetOutPath(outFile);
            }
            MessageBox.Show(message);
        }

        private void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar.Value = Math.Min(Math.Max(progressBar.Minimum, e.ProgressPercentage), progressBar.Maximum);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            progressBar.Visible = false;
            btnCancelParallel.Visible = false;
        }

        private void btnVarDataCheck_Click(object sender, EventArgs e)
        {
            string varPath = GetOutPath("VarUsage");
            string inputFilePath = UISessionInfo.GetInputFolder();
            string newVarPath = GetOutPath("UpdatedVarUsage");

            if (!File.Exists(varPath))
            {
                MessageBox.Show("File '"+varPath+"' not found. Please run 'Variable Usage' first.");
                return;
            }
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Please select the folder with the input files to compare";
            fbd.SelectedPath = inputFilePath;
            if (fbd.ShowDialog(this) == System.Windows.Forms.DialogResult.Cancel) return;
            inputFilePath = fbd.SelectedPath;

            // read the varusage file
            List<List<string>> varInfo = new List<List<string>>();
            foreach (string line in File.ReadLines(varPath))
                varInfo.Add(new List<string>(line.Split('\t')));
            
            // read the haders of all data
            Dictionary<string, List<string>> allInputInfo = new Dictionary<string, List<string>>();
            foreach (string inputFile in Directory.EnumerateFiles(inputFilePath))
                allInputInfo.Add(Path.GetFileNameWithoutExtension(inputFile), File.ReadLines(inputFile).First().Split('\t').ToList());

            // check usage of vars in input files
            List<string> inputInfo = new List<string>() { "Exists in Data" };
            for (int i = 1; i < varInfo.Count; i++)
            {
                string varName = varInfo[i][0];
                string containingDatasets = string.Empty;
                foreach (KeyValuePair<string,List<string>> data in allInputInfo)
                    if (data.Value.Contains(varName, StringComparer.Create(System.Globalization.CultureInfo.InvariantCulture, true))) 
                        containingDatasets += " " + data.Key;
                if (containingDatasets != string.Empty) containingDatasets = containingDatasets.Substring(1);
                inputInfo.Add(containingDatasets);
            }

            // write the new file
            List<string> print = new List<string>();
            for (int i = 0; i < varInfo.Count; i++)
                print.Add(String.Join("\t", new string[] { varInfo[i][0], varInfo[i][1], varInfo[i][2], inputInfo[i], varInfo[i][3], varInfo[i][4] }));
            File.WriteAllLines(newVarPath, print);
            MessageBox.Show("Updated VarUsage completed: '" + newVarPath + "'");
        }
    }
}
