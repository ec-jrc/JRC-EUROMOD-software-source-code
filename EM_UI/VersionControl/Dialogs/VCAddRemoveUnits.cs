using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCAddRemoveUnits : Form
    {
        string nextVersion = string.Empty;
        internal VCAddRemoveUnits(List<VCAdministrator.UnitExistence> units, bool displayVersion, string nextAutoVersion = "")
        {
            nextVersion = nextAutoVersion;
            
            InitializeComponent();

            if (displayVersion)
            {
                textVersion.Text = nextVersion;
            }
            else
            {
                textVersion.Visible = false;
                chkVersion.Visible = false;
            }
            

            colUnit.Width = colType.Width = listUnits.Width / 2 - 3;

            foreach (VCAdministrator.UnitExistence unit in units)
            {
                ListViewItem item = listUnits.Items.Add(unit.unitName);
                item.SubItems.Add(VCContentControl.UnitTypeToString(unit.unitType));
                item.Tag = unit;
            }
        }

        void VCAddRemoveUnits_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {


            List<VCAdministrator.UnitExistence> selectedUnits = GetSelected();
            if (selectedUnits.Count() == 0)
            {
                UserInfoHandler.ShowInfo("Please, select at least one unit.");
                return;
            }
            else
            {
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
            
        }

        internal List<VCAdministrator.UnitExistence> GetSelected()
        {
            List<VCAdministrator.UnitExistence> selected = new List<VCAdministrator.UnitExistence>();
            foreach (ListViewItem item in listUnits.CheckedItems) selected.Add(item.Tag as VCAdministrator.UnitExistence);
            return selected;
        }

        internal string getNextVersion()
        {
            return textVersion.Text;
        }

        private void btnSel_Click(object sender, EventArgs e){btnSelAll_Click(true);}

        private void btnUnsel_Click(object sender, EventArgs e){ btnSelAll_Click(false); }

        void btnSelAll_Click(bool sel) { foreach (ListViewItem item in listUnits.Items) item.Checked = sel; }

        private void chkVersion_CheckedChanged(object sender, EventArgs e)
        {
            if (chkVersion.Checked)
            {
                textVersion.Enabled = true;
            }
            else
            {
                textVersion.Enabled = false;
                textVersion.Text = nextVersion;
            }
        }
    }
}
