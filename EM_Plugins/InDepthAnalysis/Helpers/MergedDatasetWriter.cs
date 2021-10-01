using EM_Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InDepthAnalysis
{
    internal class MergedDatasetHandler
    {
        List<BackgroundWorker> backgroundWorkers = new List<BackgroundWorker>();

        internal void StopAllActions()
        {
            foreach (BackgroundWorker worker in backgroundWorkers)
            {
                try
                {
                    if (worker.IsBusy) worker.CancelAsync();
                    worker.Dispose();
                }
                catch { }
            }
            backgroundWorkers.Clear(); GC.Collect();
        }

        internal void SaveMergedDataset(Form mainform, string mergePath, List<FilePackageContent> filePackages)
        {
            if (string.IsNullOrEmpty(mergePath)) { MessageBox.Show("Failed to save merged dataset(s). No path indicated."); return; }
            if (!Directory.Exists(mergePath)) { MessageBox.Show("Failed to save merged dataset(s). Path does not exist."); return; }

            if (filePackages.Count > 0)
                mainform.Cursor = Cursors.WaitCursor;

            foreach (FilePackageContent filePackage in filePackages)
            {
                BackgroundWorker worker = new BackgroundWorker() { WorkerSupportsCancellation = true };
                worker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork);
                worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted);
                worker.RunWorkerAsync(filePackage);
                backgroundWorkers.Add(worker);
            }

            void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
            {
                FilePackageContent fpc = e.Argument as FilePackageContent;
                BackgroundWorker worker = sender as BackgroundWorker;

                string mergeFileName = Path.GetFileNameWithoutExtension(fpc.PathBase).Replace("_std", string.Empty).ToLower();
                string cc = null; int us = mergeFileName.IndexOf('_'); if (us > 0) cc = mergeFileName.Substring(0, us + 1);
                foreach (string r in fpc.PathsAlt)
                {
                    string reformFileName = Path.GetFileNameWithoutExtension(r).Replace("_std", string.Empty).ToLower();
                    mergeFileName += "_" + (reformFileName.StartsWith(cc) ? reformFileName.Substring(cc.Length) : reformFileName);
                }
                mergeFileName += ".txt";
                try
                {
                    if (worker.CancellationPending) { e.Cancel = true; return; }

                    string[] bLines = File.ReadAllLines(fpc.PathBase);
                    List<string[]> rxLines = new List<string[]>();
                    foreach (string reformPath in fpc.PathsAlt)
                    {
                        string[] rLines = File.ReadAllLines(reformPath);
                        if (rLines.Count() == bLines.Count()) rxLines.Add(rLines);
                        else { e.Result = $"Failed to save merged dataset {mergeFileName}: baseline- and reform-files do not have the same number of lines."; return; }
                    }

                    if (worker.CancellationPending) { e.Cancel = true; return; }

                    using (StreamWriter streamWriter = new StreamWriter(Path.Combine(mergePath, mergeFileName), false, new UTF8Encoding(false)))
                    {
                        List<List<int>> reformIdColumns = new List<List<int>>(); // generate the header line and find the columns of "id..." variables in reforms
                        string headLine = ScanHeadLine(bLines[0], 0, out _);
                        for (int r = 0; r < rxLines.Count; ++r)
                        {
                            headLine += ScanHeadLine(rxLines[r][0], r + 1, out List<int> rIdColumns);
                            reformIdColumns.Add(rIdColumns);
                        }
                        streamWriter.WriteLine(headLine.TrimEnd());

                        for (int lineNum = 1; lineNum < bLines.Count(); ++lineNum)
                        {
                            string mergedLine = bLines[lineNum].Trim();
                            for (int r = 0; r < rxLines.Count; ++r)
                            {
                                string[] columns = rxLines[r][lineNum].Trim().Split('\t');
                                for (int c = 0; c < columns.Length; ++c)
                                    if (!reformIdColumns[r].Contains(c)) mergedLine += "\t" + columns[c];
                            }
                            streamWriter.WriteLine(mergedLine);
                        }

                        string ScanHeadLine(string line, int r, out List<int> rIdColumns)
                        {
                            string hl = string.Empty; rIdColumns = new List<int>();
                            string prefix = r == 0 ? "b_" : $"r{r}_";
                            string[] columns = line.Trim().Split('\t');
                            for (int c = 0; c < columns.Length; ++c)
                            {
                                if (columns[c].Trim().StartsWith("id"))
                                {
                                    if (r == 0) hl += $"{columns[c]}\t";
                                    else rIdColumns.Add(c);
                                }
                                else hl += $"{prefix}{columns[c]}\t";
                            }
                            return hl;
                        }
                    }
                }
                catch (Exception exception) { e.Result = $"Failed to save merged dataset {mergeFileName}: {exception.Message}"; }
            }

            void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
            {
                try
                {
                    if (e.Cancelled) return;
                    if (!string.IsNullOrEmpty(e.Result?.ToString())) MessageBox.Show(e.Result.ToString());
                    BackgroundWorker worker = sender as BackgroundWorker; if (worker == null) return;
                    backgroundWorkers.Remove(worker); worker.Dispose();
                    if (backgroundWorkers.Count == 0)
                    {
                        MessageBox.Show("Merge complete!");
                        mainform.Cursor = Cursors.Default;
                    }
                }
                catch { }
            }
        }
    }
}
