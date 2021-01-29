using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraGrid.Views.Grid;

namespace HypotheticalHousehold.SettingsManagement
{
    public partial class ManageBasicVariables : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        string errorMessage = "";
        internal bool changed = false;

        public ManageBasicVariables(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            this.repositoryItemLookUpEdit1.DataSource = Plugin.settingsData.Cur_Categories;
            this.repositoryItemCheckedComboBoxEdit1.DataSource = Plugin.settingsData.Cur_Countries;
            colVariableType.ColumnEdit = HypotheticalHousehold.Program.getVariableTypeCombo();
            gridView1.MasterRowExpanding += gridView1_MasterRowExpanding;
            colPrivate.Visible = !_plugin.isPublicVersion();
        }

        void gridView1_MasterRowExpanding(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowCanExpandEventArgs e)
        {
            e.Allow = true;
        }

        private void ManageBasicVariables_Shown(object sender, EventArgs e)
        {
            gridControl1.DataSource = Plugin.settingsData.Cur_BasicVariables;
        }

        private bool checkValuesOK()
        {
            bool allOK = true;
            errorMessage = "";
            if (Plugin.settingsData.HasChanges())
            {
                // first check changes in master table
                if (Plugin.settingsData.Cur_BasicVariables.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_BasicVariables.GetChanges().Rows)
                    {
                        if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                        {
                            if (row.Field<string>("Category") == null || row.Field<string>("Category") == "")
                            {
                                errorMessage += "You need to fill the Category field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("Description") == null || row.Field<string>("Description") == "")
                            {
                                errorMessage += "You need to fill the Description field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("VariableType") == null || row.Field<string>("VariableType") == "")
                            {
                                errorMessage += "You need to fill the VariableType field for '" + row.Field<string>("VariableName") + "'.\n";
                                allOK = false;
                            }
                            if (row.Field<string>("VariableType") == Program.EDITOR_TYPE_COMBO)
                            {
                                if (row.Field<string>("ValueRange") == null || row.Field<string>("TextRange") == null
                                    || row.Field<string>("ValueRange") == "" || row.Field<string>("TextRange") == "")
                                {
                                    errorMessage += "You need to fill both the ValueRange & TextRange for Combo '" + row.Field<string>("VariableName") + "'.\n";
                                    allOK = false;
                                }
                                else if (row.Field<string>("ValueRange").Split('#').Length != row.Field<string>("TextRange").Split('#').Length)
                                {
                                    errorMessage += "ValueRange and TextRange must have the same number of elements for '" + row.Field<string>("VariableName") + "'.\n";
                                    allOK = false;
                                }
                            }
                        }
                    }
                }
                // then check changed in the detail table
                if (Plugin.settingsData.Cur_BasicCountrySpecificDetail.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_BasicCountrySpecificDetail.GetChanges().Rows)
                    {
                        if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                        {
                            if (row.Field<string>("Countries") == null || row.Field<string>("Countries") == "")
                            {
                                errorMessage += "You need to fill the Countries field for '" + row.Field<string>("VariableName") + "->" + row.Field<string>("CountrySpecificVariableName") + "'.\n";
                                allOK = false;
                            }
                        }
                    }
                }
            }
            return allOK;
        }

        private void ManageBasicVariables_FormClosing(object sender, FormClosingEventArgs e)
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

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow row = null;
            DataRowView r = (view.GetFocusedRow() as DataRowView);
            if (r != null && r.Row != null) row = r.Row;
            if (row == null || row.RowState == DataRowState.Deleted || row.Field<string>("VariableType") == null) return;

            if (view.FocusedColumn.FieldName == "ValueRange" || view.FocusedColumn.FieldName == "TextRange")
                if (row.Field<string>("VariableType") != Program.EDITOR_TYPE_COMBO) e.Cancel = true;
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.Column.FieldName == "ValueRange" || e.Column.FieldName == "TextRange")
            {
                DataRow row = null;
                if (e.RowHandle >= 0)
                {
                    row = gridView1.GetDataRow(e.RowHandle);
                }
                if (row == null || row.Field<string>("VariableType") == null) return;

                if (row.Field<string>("VariableType") != Program.EDITOR_TYPE_COMBO)
                    e.Appearance.BackColor = Color.LightGray;
                else
                    e.Appearance.BackColor = Color.White;
            }
        }

        private void gridView_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete", DeleteRow));
        }

    }
}
