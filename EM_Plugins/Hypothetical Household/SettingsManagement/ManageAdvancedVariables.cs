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
    public partial class ManageAdvancedVariables : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        string errorMessage = "";
        internal bool changed = false;

        public ManageAdvancedVariables(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            this.repositoryItemLookUpEdit1.DataSource = Plugin.settingsData.Cur_Categories;
            this.repositoryItemCheckedComboBoxEditCountries.DataSource = Plugin.settingsData.Cur_Countries;
            colVariableType.ColumnEdit = HypotheticalHousehold.Program.getVariableTypeCombo();
            colPrivate.Visible = !_plugin.isPublicVersion();
        }

        private void ManageAdvancedVariables_Shown(object sender, EventArgs e)
        {
            gridControl1.DataSource = Plugin.settingsData.Cur_AdvancedVariables;
            gridView1.ShowFilterPopupListBox += gridView1_ShowFilterPopupListBox;
        }

        void gridView1_ShowFilterPopupListBox(object sender, FilterPopupListBoxEventArgs e)
        {
            if (e.Column.FieldName == "DefaultValue")
            {
                Dictionary<string, List<object>> values = new Dictionary<string, List<object>>();
                DevExpress.XtraEditors.Controls.ComboBoxItemCollection col = new DevExpress.XtraEditors.Controls.ComboBoxItemCollection(new DevExpress.XtraEditors.Repository.RepositoryItemComboBox());

                foreach (FilterItem it in e.ComboBox.Items)
                {
                    if (!it.Text.StartsWith("("))
                    {
                        string t = it.Text;
                        if (t.StartsWith("Value")) t = NumericEditor.GetDisplayText(t, true, Plugin);

                        if (!values.ContainsKey(t)) values.Add(t, new List<object>());
                        values[t].Add("[DefaultValue] = '"+it.Value+"'");
                    }
                }
                foreach (string t in values.Keys)
                {
                    DevExpress.XtraGrid.Columns.ColumnFilterInfo cfi = new DevExpress.XtraGrid.Columns.ColumnFilterInfo(String.Join(" OR ", values[t]), t);
                    col.Add(new FilterItem(t, cfi));
                }
                e.ComboBox.Items.Clear();
                e.ComboBox.Items.Add(new FilterItem("(All)", new DevExpress.XtraGrid.Columns.ColumnFilterInfo()));
                e.ComboBox.Items.AddRange(col);

            }
        }

        private bool checkValuesOK()
        {
            bool allOK = true;
            errorMessage = "";
            if (Plugin.settingsData.HasChanges())
            {
                // there is only one table to check, still make sure it has changes!
                if (Plugin.settingsData.Cur_AdvancedVariables.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_AdvancedVariables.GetChanges().Rows)
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
                            if (row.Field<string>("VariableType") == null || row.Field<string>("VariableType") == "")
                            {
                                errorMessage += "You need to fill the VariableType field for '" + row.Field<string>("VariableName") + "'.\n";
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
                                else if (row.Field<string>("DefaultValue") != null && row.Field<string>("DefaultValue") != "")
                                {
                                    bool exists = false;
                                    foreach (string v in row.Field<string>("ValueRange").Split('#'))
                                    {
                                        if (v.Trim() == row.Field<string>("DefaultValue"))
                                        {
                                            exists = true;
                                            break;
                                        }
                                    }
                                    if (!exists)
                                    {
                                        errorMessage += "The Default Value for Combo '" + row.Field<string>("VariableName") + "' must match one of the values in Value Range.\n";
                                        allOK = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return allOK;
        }

        private void ManageAdvancedVariables_FormClosing(object sender, FormClosingEventArgs e)
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

        private void gridView1_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow row = null;
            if (e.RowHandle >= 0) 
            {
                row = gridView1.GetDataRow(e.RowHandle);
            }
            else
            {
                DataRowView r = (view.GetFocusedRow() as DataRowView);
                if (r != null && r.Row != null) row = r.Row;
            }
            if (row == null || row.RowState == DataRowState.Deleted || row.Field<string>("VariableType") == null) return;
            
            if (e.Column.FieldName == "DefaultValue")
            {
                switch (row.Field<string>("VariableType"))
                {
                    case Program.EDITOR_TYPE_NUMERIC:
                        e.RepositoryItem = new RepositoryItemPopupContainerEditNumericRange(Plugin);
                        break;
                    default:
                        // if there was a Numeric value before, remove as it just looks weird
                        if (e.CellValue != null && e.CellValue.ToString().Contains("|RefTable_"))
                        {
                            row.SetField<string>("DefaultValue", "");
                        }
                        break;
                }
            }
        }

        private void gridView1_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow row = null;
            DataRowView r = (view.GetFocusedRow() as DataRowView);
            if (r != null && r.Row != null) row = r.Row;
            if (row == null || row.RowState == DataRowState.Deleted || row.Field<string>("VariableType") == null) return;
            
            // make sure the default value for Connection variables is always 0! 
            if (row.Field<string>("VariableType")  == Program.EDITOR_TYPE_CONNECTION)
                row.SetField<string>("DefaultValue", "0");

            if (view.FocusedColumn.FieldName == "ValueRange" || view.FocusedColumn.FieldName == "TextRange")
            {
                if (row.Field<string>("VariableType") != Program.EDITOR_TYPE_COMBO) e.Cancel = true;
            }
            else if (view.FocusedColumn.FieldName == "DefaultValue")
            {
                if (row.Field<string>("VariableType") == Program.EDITOR_TYPE_CONNECTION) e.Cancel = true;
            }
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0) return;
            DataRow row = gridView1.GetDataRow(e.RowHandle);
            if (row == null || row.RowState == DataRowState.Deleted || row.Field<string>("VariableType") == null) return;

            if (e.Column.FieldName == "ValueRange" || e.Column.FieldName == "TextRange")
            {
                if (row.Field<string>("VariableType") != Program.EDITOR_TYPE_COMBO)
                    e.Appearance.BackColor = Color.LightGray;
                else
                    e.Appearance.BackColor = Color.White;
            }
            else if (e.Column.FieldName == "DefaultValue")
            {
                if (row.Field<string>("VariableType") == Program.EDITOR_TYPE_CONNECTION)
                    e.Appearance.BackColor = Color.LightGray;
                else
                    e.Appearance.BackColor = Color.White;
            }
        }

        private void gridView1_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete", DeleteRow));
        }
    }
}
