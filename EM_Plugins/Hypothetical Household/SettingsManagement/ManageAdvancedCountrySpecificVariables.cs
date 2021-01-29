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
    public partial class ManageAdvancedCountrySpecificVariables : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        string errorMessage = "";
        internal bool changed = false;

        public ManageAdvancedCountrySpecificVariables(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            this.repositoryItemLookUpEdit1.DataSource = Plugin.settingsData.Cur_Categories;
            this.repositoryItemLookUpEdit2.DataSource = Plugin.settingsData.Cur_Countries;
            colVariableType.ColumnEdit = HypotheticalHousehold.Program.getVariableTypeCombo();
            gridView1.MasterRowExpanding += gridView1_MasterRowExpanding;
            colPrivate.Visible = !_plugin.isPublicVersion();
        }

        void gridView1_MasterRowExpanding(object sender, DevExpress.XtraGrid.Views.Grid.MasterRowCanExpandEventArgs e)
        {
            e.Allow = true;
        }

        private void ManageAdvancedCountrySpecificVariables_Shown(object sender, EventArgs e)
        {
            gridControl1.DataSource = Plugin.settingsData.Cur_AdvancedCountrySpecificVariables;
            gridView2.ShowFilterPopupListBox += gridView2_ShowFilterPopupListBox;
        }

        void gridView2_ShowFilterPopupListBox(object sender, FilterPopupListBoxEventArgs e)
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
                        values[t].Add("[DefaultValue] = '" + it.Value + "'");
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
                // first check changes in master table
                if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.GetChanges().Rows)
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
                        }
                    }
                }
                // then check changed in the detail table
                if (Plugin.settingsData.Cur_AdvancedCountrySpecificDetail.GetChanges() != null)
                {
                    foreach (DataRow row in Plugin.settingsData.Cur_AdvancedCountrySpecificDetail.GetChanges().Rows)
                    {
                        if (row.RowState == DataRowState.Modified || row.RowState == DataRowState.Added)
                        {
                            DataRow[] rs = Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.Select("VariableName='" + row.Field<string>("VariableName") + "'");
                            if (rs.Length == 0)
                            {
                                // parrent row must have been deleted, so we need to delete all child rows too!! 
                                row.Delete();
                                continue;
                            }
                            if (rs.Length > 1)
                            {
                                errorMessage += "Internal error - duplicate or missing variable.\n";
                                allOK = false;
                                break;
                            }
                            if (rs[0].Field<string>("VariableType") == Program.EDITOR_TYPE_COMBO)
                            {
                                if (row.Field<string>("ValueRange") == null || row.Field<string>("TextRange") == null
                                    || row.Field<string>("ValueRange") == "" || row.Field<string>("TextRange") == "")
                                {
                                    errorMessage += "You need to fill both the ValueRange & TextRange for Combo '" + row.Field<string>("VariableName") + "_" + row.Field<string>("Country") + "'.\n";
                                    allOK = false;
                                }
                                else if (row.Field<string>("ValueRange").Split('#').Length != row.Field<string>("TextRange").Split('#').Length)
                                {
                                    errorMessage += "ValueRange and TextRange must have the same number of elements for '" + row.Field<string>("VariableName") + "_" + row.Field<string>("Country") + "'.\n";
                                    allOK = false;
                                }
                                else if (row.Field<string>("DefaultValue") == null || row.Field<string>("DefaultValue") == "")
                                {
                                    errorMessage += "You need to fill a Default Value for Combo '" + row.Field<string>("VariableName") + "_" + row.Field<string>("Country") + "'.\n";
                                    allOK = false;
                                }
                                else
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
                            else if (rs[0].Field<string>("VariableType") == Program.EDITOR_TYPE_NUMERIC)
                            {
                                if (row.Field<string>("DefaultValue") == null || row.Field<string>("DefaultValue") == "")
                                {
                                    errorMessage += "You need to fill a Default Value for '" + row.Field<string>("VariableName") + "_" + row.Field<string>("Country") + "'.\n";
                                    allOK = false;
                                }
                            }
                        }
                    }
                }
            }
            return allOK;
        }

        private void ManageAdvancedCountrySpecificVariables_FormClosing(object sender, FormClosingEventArgs e)
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
                GridView gv = gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
                if (gv == gridView1)
                {
                    foreach (int i in gv.GetSelectedRows())
                    {
                        DataRowView gr = gv.GetRow(i) as DataRowView;
                        DataRow[] drs = Plugin.settingsData.Cur_AdvancedCountrySpecificDetail.Select("VariableName = '"+gr.Row.Field<string>("VariableName")+"'");
                        foreach(DataRow dr in drs) dr.Delete();
                    }
                }
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

        private void gridView2_ShowingEditor(object sender, CancelEventArgs e)
        {
            GridView view = sender as GridView;
            DataRowView r = (view.GetFocusedRow() as DataRowView);
            DataRow row = r==null?null:r.Row;

            if (row == null || row.RowState == DataRowState.Deleted || Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType == null) return;

            // make sure the default value for Connection variables is always 0! 
            if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType == Program.EDITOR_TYPE_CONNECTION)
                row.SetField<string>("DefaultValue", "0");

            if (view.FocusedColumn.FieldName == "ValueRange" || view.FocusedColumn.FieldName == "TextRange")
            {
                if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType != Program.EDITOR_TYPE_COMBO) e.Cancel = true;
            }
            else if (view.FocusedColumn.FieldName == "DefaultValue")
            {
                if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType == Program.EDITOR_TYPE_CONNECTION) e.Cancel = true;
            }
        }

        private void gridView2_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            if (e.RowHandle < 0) return;
            GridView view = sender as GridView;
            DataRow row = view.GetDataRow(e.RowHandle);
            if (row == null || row.RowState == DataRowState.Deleted || Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType == null) return;
            
            if (e.Column.FieldName == "ValueRange" || e.Column.FieldName == "TextRange")
            {
                if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType != Program.EDITOR_TYPE_COMBO)
                    e.Appearance.BackColor = Color.LightGray;
                else
                    e.Appearance.BackColor = Color.White;
            }
            else if (e.Column.FieldName == "DefaultValue")
            {
                if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType == Program.EDITOR_TYPE_CONNECTION)
                    e.Appearance.BackColor = Color.LightGray;
                else
                    e.Appearance.BackColor = Color.White;
            }
        }

        private void gridView2_CustomRowCellEdit(object sender, CustomRowCellEditEventArgs e)
        {
            GridView view = sender as GridView;
            DataRow row = null;

            if (e.RowHandle >= 0)
            {
                row = view.GetDataRow(e.RowHandle);
            }
            else
            {
                DataRowView r = (view.GetFocusedRow() as DataRowView);
                if (r != null && r.Row != null) row = r.Row;
            }
            if (row == null || row.RowState == DataRowState.Deleted) return;

            if (e.Column.FieldName == "DefaultValue")
            {
                switch (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.VariableName == row.Field<string>("VariableName")).VariableType)
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

        private void gridView_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete", DeleteRow));
        }

    }
}
