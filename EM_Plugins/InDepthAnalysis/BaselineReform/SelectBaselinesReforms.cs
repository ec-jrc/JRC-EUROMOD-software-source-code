using EM_Common;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace InDepthAnalysis
{
    internal partial class SelectBaselinesReforms : Form
    {
        readonly List<string> requiredVariables = new List<string>();
        readonly List<int> selectedIndices = new List<int>(); // store the order of selection for reforms
        internal List<FilePackageContent> filePackages = null;

        public SelectBaselinesReforms(Settings currentSettings,List<string> _requiredVariables)
        {
            InitializeComponent();
            InDepthAnalysis.SetShowHelp(this, helpProvider);

            requiredVariables = _requiredVariables;
            txtBaselinesPath.Text = currentSettings.pathBaselineFiles;
            txtReformsPath.Text = currentSettings.pathReformFiles;

            ShowCurrentSelection(currentSettings.baselineReformPackages);
        }

        private void ShowCurrentSelection(List<BaselineReformPackage> baselineReformPackages)
        {
            try
            {
                if (baselineReformPackages == null || string.IsNullOrEmpty(txtBaselinesPath?.Text) || string.IsNullOrEmpty(txtReformsPath?.Text)) return;
                foreach (BaselineReformPackage brp in baselineReformPackages)
                {
                    SetOutputFileSelected(listBaselines, brp.baseline.filePath);
                    foreach (var reform in brp.reforms) SetOutputFileSelected(listReforms, reform.filePath);
                }

                void SetOutputFileSelected(ListBox listBox, string outputFile)
                {
                    foreach (string file in listBox.Items)
                        if (EMPath.IsSamePath(Path.Combine(txtBaselinesPath.Text, file), outputFile))
                            { listBox.SetSelected(listBox.Items.IndexOf(file), true); break; }
                }
            }
            catch { }
        }

        private void btnSelectBaselinesPath_Click(object sender, EventArgs e) { SelectPath(false); }
        private void btnSelectReformsPath_Click(object sender, EventArgs e) { SelectPath(true); }
        private void SelectPath(bool reforms)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.SelectedPath = reforms ? txtReformsPath.Text : txtBaselinesPath.Text;
            folderBrowserDialog.Description = $"Please select the folder containing the {(reforms ? "reform file(s)" : "baseline file(s)")}.";
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                if (reforms) txtReformsPath.Text = folderBrowserDialog.SelectedPath; else txtBaselinesPath.Text = folderBrowserDialog.SelectedPath;
        }

        private void txtBaselinesPath_TextChanged(object sender, EventArgs e) { FillFileList(false); }
        private void txtReformsPath_TextChanged(object sender, EventArgs e) { FillFileList(true); }

        private void FillFileList(bool reforms)
        {
            ListBox box = reforms ? listReforms : listBaselines;
            string filePattern = "*.txt";
            string path = reforms ? txtReformsPath.Text : txtBaselinesPath.Text;

            box.Items.Clear();
            if (Directory.Exists(path))
                foreach (string filePath in Directory.GetFiles(path, filePattern))
                    if (IsValidInput(filePath)) box.Items.Add((new FileInfo(filePath)).Name);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (listBaselines.SelectedItems.Count == 0) { MessageBox.Show("Please select at least one baseline file."); return; }

            filePackages = new List<FilePackageContent>();
            List<string> errors = new List<string>(); Dictionary<string, string> blPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string baseline in listBaselines.SelectedItems)
            {
                FilePackageContent fpc = new FilePackageContent() { PathBase = Path.Combine(txtBaselinesPath.Text, baseline) };
                if (!GetCountryPrefix(baseline, out string blPrefix)) continue;
                if (blPrefixes.ContainsKey(blPrefix))
                {
                    errors.Add($"Baselines {baseline} and {blPrefixes[blPrefix]} have the same prefix ({blPrefix}). Reforms cannot be assiged.");
                    continue;
                }
                else
                {
                    blPrefixes.Add(blPrefix, baseline);
                    foreach (string reform in selectedIndices.Select(i => listReforms.Items[i]))  // get alt selections in order
                        if (reform.StartsWith(blPrefix, true, null)) fpc.PathsAlt.Add(Path.Combine(txtReformsPath.Text, reform));
                    if (!fpc.PathsAlt.Any())
                    {
                        errors.Add($"Baseline {baseline}: no compatible reforms (starting with {blPrefix}) found.");
                        continue;
                    }
                }
                filePackages.Add(fpc);
            }
            foreach (string reform in listReforms.SelectedItems)
            {
                if (!GetCountryPrefix(reform, out string rPrefix)) continue;
                if (!blPrefixes.ContainsKey(rPrefix)) errors.Add($"Reform {reform}: no compatible baseline (starting with { rPrefix}) found.");
            }

            if (errors.Any()) { MessageBox.Show(string.Join(Environment.NewLine, errors)); return; }
            Cursor = Cursors.WaitCursor; // the dialog may not disappear immediatley and thus give the user the impression of a frozen programme, if no wait cursor is shown
            DialogResult = DialogResult.OK; Close();

            bool GetCountryPrefix(string fileName, out string prefix)
            {
                prefix = null;
                if (fileName.Contains("_")) prefix = fileName.Substring(0, fileName.IndexOf("_") + 1);
                else errors.Add($"{fileName} does not contain any underscore. Thus assessing country respectively baseline-reforms-assignement is not possible.");
                return prefix != null;
            }
        }

        private void lblMissingFiles_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("This template requires tab-delimited text files that contain at least the following columns: " + Environment.NewLine + string.Join(Environment.NewLine, requiredVariables));
        }

        private void listReforms_SizeChanged(object sender, EventArgs e)
        {
            lblMultiSelect.Top = listReforms.Top + listReforms.Height;
            lblMultiSelect.Left = listReforms.Left + listReforms.Width - lblMultiSelect.Width;
            lblOrderRetained.Top = lblMultiSelect.Top + lblMultiSelect.Height;
            lblOrderRetained.Left = lblMultiSelect.Left + lblMultiSelect.Width - lblMultiSelect.Width;
        }

        private void listReforms_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listReforms.SelectedIndex > -1)
            {
                selectedIndices.AddRange(listReforms.SelectedIndices.OfType<int>()
                                                 .Except(selectedIndices));
                List<int> removeItems = new List<int>();
                foreach (int x in selectedIndices) if (!listReforms.SelectedIndices.Contains(x)) removeItems.Add(x);
                foreach (int x in removeItems) selectedIndices.Remove(x);
            }
            listReforms.Refresh();    // to make sure that the DrawItem is called after the index chagned
        }

        private void listReforms_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();
            if (listReforms.SelectedIndices.Contains(e.Index) && selectedIndices.Contains(e.Index))
                e.Graphics.DrawString("{" + selectedIndices.IndexOf(e.Index) + "} " + listReforms.Items[e.Index], e.Font, new System.Drawing.SolidBrush(e.ForeColor), e.Bounds);
            else
                e.Graphics.DrawString(listReforms.Items[e.Index].ToString(), e.Font, new System.Drawing.SolidBrush(e.ForeColor), e.Bounds);
            e.DrawFocusRectangle();
        }

        private bool IsValidInput(string filePath)
        {
            string[] headers;
            try
            {
                string firstLine = File.ReadLines(filePath).FirstOrDefault();
                if (firstLine == null || firstLine.Length < 1) return false;
                headers = firstLine.ToLower().Split('\t');
            }
            catch (IOException) { return false; } // if the file is locked (e.g. open in Excel) ignore it
            catch (Exception ex) { MessageBox.Show(filePath + " has returned the unexpected error: '" + ex.Message + "'"); return false; }

            foreach (string rf in requiredVariables) if (!headers.Contains(rf.ToLower())) return false;
            return true;
        }
    }
}
