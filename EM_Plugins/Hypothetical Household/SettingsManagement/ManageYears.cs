using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HypotheticalHousehold.SettingsManagement
{
    public partial class ManageYears : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        internal bool changed = false;

        public ManageYears(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            gridControl1.DataSource = Plugin.settingsData.Cur_Years;
            gridView1.ValidatingEditor += gridView1_ValidatingEditor;
        }

        void gridView1_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            if (e.Value.ToString() == "")
            {
                gridView1.DeleteSelectedRows();
            }
        }

        private void ManageYears_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnClose.Focus();
            if (Plugin.settingsData.HasChanges())
            {
                DialogResult res = MessageBox.Show(this, "Do you want to save your changes?", "Save", MessageBoxButtons.YesNoCancel);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    Plugin.automaticlyUpdateReferenceTables();  // if the Years changed then you probably also need to update all the Reference Tables!
                    Plugin.saveSettings();
                    changed = true;
                }
                else if (res == System.Windows.Forms.DialogResult.Cancel)
                    e.Cancel = true;
                else if (res == System.Windows.Forms.DialogResult.No)
                    Plugin.settingsData.RejectChanges();
            }
        }

        private void gridControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteRow(null, null);
            }
        }

        private void DeleteRow(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Do you want to delete this record?", "Delete", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                (gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView).DeleteSelectedRows();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Plugin.automaticlyUpdateReferenceTables();  // if the Countries changed then you probably also need to update all the Reference Tables!
            Plugin.saveSettings();
            changed = true;
            MessageBox.Show("Settings saved.");
        }

        private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete", DeleteRow));
        }
    }
}
