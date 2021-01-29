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
    public partial class ManageDerivedVariables : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        string errorMessage = "";
        internal bool changed = false;

        public ManageDerivedVariables(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            this.repositoryItemLookUpEdit1.DataSource = Plugin.settingsData.Cur_Categories;
            this.repositoryItemCheckedComboBoxEdit1.DataSource = Plugin.settingsData.Cur_Countries;
            gridView1.MasterRowExpanding += gridView1_MasterRowExpanding;
            colPrivate.Visible = !_plugin.isPublicVersion();
        }

        void gridView1_MasterRowExpanding(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowCanExpandEventArgs e)
        {
            e.Allow = true;
        }

        private void ManageDerivedVariables_Shown(object sender, EventArgs e)
        {
            gridControl1.DataSource = Plugin.settingsData.Cur_DerivedVariables;
        }

        private bool checkValuesOK()
        {
            bool allOK = true;
            errorMessage = "";
            if (Plugin.settingsData.HasChanges())
            {
                // first check changes in master table
                if (Plugin.settingsData.Cur_DerivedVariables.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_DerivedVariables.GetChanges().Rows)
                    {
                        if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                        {
                            if (row.Field<string>("Category") == null || row.Field<string>("Category").Trim() == "")
                            {
                                errorMessage += "You need to fill the Category field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("Description") == null || row.Field<string>("Description").Trim() == "")
                            {
                                errorMessage += "You need to fill the Description field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("Countries") == null || row.Field<string>("Countries") == "")
                            {
                                errorMessage += "You need to fill the Country field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("DefaultValue") == null)
                            {
                                errorMessage += "You need to fill the DefaultValue field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                        }
                    }
                }
                // then check changed in the detail table
                if (Plugin.settingsData.Cur_DerivedVariablesDetail.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_DerivedVariablesDetail.GetChanges().Rows)
                    {
                        if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                        {
                            if (row.Field<string>("Condition") == null || row.Field<string>("Condition").Trim() == "")
                            {
                                errorMessage += "You need to fill the Condition field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("DerivedValue") == null || row.Field<string>("DerivedValue").Trim() == "")
                            {
                                errorMessage += "You need to fill the Derived Value field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                        }
                    }
                }
            }
            return allOK;
        }

        private void ManageDerivedVariables_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnClose.Focus();
            if (!checkValuesOK())
            {
                DialogResult res = MessageBox.Show(this, "You have errors: \n\n" + errorMessage + "\nDo you wish to correct them or revert your changes?\n\nYes => continue editting to correct the errors\nNo => lose your latest changes", "Errors found", MessageBoxButtons.YesNo);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    Plugin.settingsData.RejectChanges();
                }
            }
            if (Plugin.settingsData.HasChanges())
            {
                DialogResult res = MessageBox.Show(this, "Do you want to save your changes?", "Save", MessageBoxButtons.YesNoCancel);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
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
            if (!checkValuesOK())
            {
                DialogResult res = MessageBox.Show(this, "You have errors: \n\n" + errorMessage + "\nPlease correct them before you save your changes.", "Errors found", MessageBoxButtons.OK);
            }
            else
            {
                Plugin.saveSettings();
                changed = true;
                MessageBox.Show("Settings saved.");
            }
        }

        private void gridView_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete", DeleteRow));
        }
    }
}
