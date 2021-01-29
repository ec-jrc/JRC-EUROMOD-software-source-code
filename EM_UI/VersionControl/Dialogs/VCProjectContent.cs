using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCProjectContent : Form
    {
        internal VCProjectContent(VCNewProject.ProjectContent projectContent)
        {
            InitializeComponent();
            for (int i = 0; i < projectContent.projectUnits.Count; ++i)
            {
                ListViewItem item = listUnits.Items.Add($"{projectContent.projectUnits[i].Name} ({VCContentControl.UnitTypeToString(projectContent.projectUnits[i].UnitType)})");
                item.Checked = i < projectContent.selections.Count ? projectContent.selections[i] : false;
                item.Tag = projectContent.projectUnits[i];
            }
            foreach (string year in projectContent.selectedYears) listYears.Items.Add(year);
            checkAllYears.Checked = listYears.Items.Count == 0;
        }

        void VCProjectContent_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!checkAllYears.Checked && listYears.Items.Count == 0) { UserInfoHandler.ShowError("Please select one or more years."); return; }
            DialogResult = System.Windows.Forms.DialogResult.OK;
            Close();
        }

        internal void GetChoices(out List<bool> units, out List<string> years)
        {
            units = new List<bool>(); years = new List<string>();
            foreach (ListViewItem item in listUnits.Items) units.Add(item.Checked);
            if (!checkAllYears.Checked) foreach (string year in listYears.Items) years.Add(year);
        }

        void btnSel_Click(bool sel) { foreach (ListViewItem item in listUnits.Items) item.Checked = sel; }
        void btnSel_Click(object sender, EventArgs e) { btnSel_Click(true); }
        void btnUnsel_Click(object sender, EventArgs e) { btnSel_Click(false); }

        private void listYears_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Delete || checkAllYears.Checked || listYears.SelectedIndices == null || listYears.SelectedIndices.Count == 0) return;
            for (int i = listYears.SelectedIndices.Count - 1; i >= 0; --i) listYears.Items.RemoveAt(listYears.SelectedIndices[i]);
        }

        private void checkAllYears_CheckedChanged(object sender, EventArgs e)
        {
            listYears.Items.Clear();
            if (checkAllYears.Checked) listYears.Items.Add("All Years");
            btnAddYear.Enabled = numYear.Enabled = !checkAllYears.Checked;
        }

        private void btnAddYear_Click(object sender, EventArgs e)
        {
            if (!listYears.Items.Contains(numYear.Value.ToString()))
            {
                listYears.Items.Add(numYear.Value.ToString());
                numYear.Value++;
            }
        }
    }
}
