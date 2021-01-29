using DevExpress.XtraEditors;
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
    public partial class ManageReferenceTables : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        private bool _blockVisualUpdates = false;

        public ManageReferenceTables(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            textEdit1.DataBindings.Add(new Binding("EditValue", Plugin.referenceTablesData.ReferenceTables, "Comments"));
            if (Plugin.automaticlyUpdateReferenceTables())
            {
                Plugin.saveReferenceTables();
            }
        }


        private void updateGrid()
        {
            gridControl1.DataSource = listBoxControlTables.SelectedValue != null ? Plugin.referenceTablesData.Tables[listBoxControlTables.SelectedValue.ToString()] : null;
            if (gridControl1.DataSource != null) gridView1.Columns[0].OptionsColumn.AllowEdit = false;
        }

        private void ManageReferenceTables_Load(object sender, EventArgs e)
        {
            listBoxControlTables.DataSource = Plugin.referenceTablesData.ReferenceTables;
            listBoxControlTables.DisplayMember = "TableDescription";
            listBoxControlTables.ValueMember = "TableName";
            gridView1.OptionsBehavior.CopyToClipboardWithColumnHeaders = false;
            updateGrid();
            if (gridView1.Columns.Count > 0) gridView1.Columns[0].SortOrder = DevExpress.Data.ColumnSortOrder.Ascending;
        }

        private void blockVisualUpdates()
        {
            _blockVisualUpdates = true;
//            gridControl1.BeginUpdate();
//            listBoxControlTables.BeginUpdate();
        }

        private void unblockVisualUpdates()
        {
//            gridControl1.EndUpdate();
//            listBoxControlTables.EndUpdate();
            _blockVisualUpdates = false;
        }

        private bool saveChanges()
        {
            if (_blockVisualUpdates) return true;
            blockVisualUpdates();
            if (Plugin.referenceTablesData.HasChanges())
            {
                DialogResult res = MessageBox.Show(this, "Do you want to save your pending changes?", "Save", MessageBoxButtons.YesNoCancel);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    Plugin.saveReferenceTables();
                    updateGrid();
                }
                else if (res == System.Windows.Forms.DialogResult.Cancel)
                {
                    unblockVisualUpdates();
                    return false;
                }
                else if (res == System.Windows.Forms.DialogResult.No)
                {
                    Plugin.referenceTablesData.RejectChanges();
                    updateGrid();
                }
            }
            else
            {
                updateGrid();
            }
            unblockVisualUpdates();
            return true;
        }

        private void listBoxControlTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blockVisualUpdates) return;
            if (gridControl1.DataSource == null) return;
            if (((sender as DevExpress.XtraEditors.ListBoxControl).SelectedValue != null) && (sender as DevExpress.XtraEditors.ListBoxControl).SelectedValue.ToString() == (gridControl1.DataSource as DataTable).TableName) return;
            // try to save changes - if cancelled, then do not change the selected index
            if (!saveChanges())
            {
                blockVisualUpdates();
                (sender as DevExpress.XtraEditors.ListBoxControl).SelectedValue = (gridControl1.DataSource as DataTable).TableName;
                unblockVisualUpdates();
            }
        }

        private void buttonAddTable_Click(object sender, EventArgs e)
        {
            // first try to save any pending changes
            if (!saveChanges()) return;
            // then add a new table
            blockVisualUpdates();
            string name = "";
            DialogResult res = DialogResult.OK;
            bool done = false;
            do
            {
                res = InputBox.Show("New Reference Table", "Please give the name for the new Reference Table:", ref name, true);
                if (res == DialogResult.OK)
                {
                    DataTable tbl;
                    done = addNewReferenceTable(name, out tbl);
                    if (done)
                    {
                        listBoxControlTables.SelectedValue = tbl.TableName;
                        updateGrid();
                    }
                }
            }
            while (!done && res != DialogResult.Cancel);
            _blockVisualUpdates = false;
        }

        private bool addNewReferenceTable(string name, out DataTable newRefTable)
        {
            newRefTable = new DataTable();
            if (name == "")
            {
                MessageBox.Show("You have to give a name for the new Reference Table!");
            }
            else if (Plugin.referenceTablesData.ReferenceTables.Select("TableDescription='" + name + "'").Count() > 0)
            {
                MessageBox.Show("This name already exists!");
            }
            else
            {
                // first find a table name and add it to the ReferenceTables table
                string tname = getUniqueRefTableName(Plugin);
                Plugin.referenceTablesData.ReferenceTables.Rows.Add(new object[] { tname, name });

                // Then create the actual table in the DataSet
                newRefTable.TableName = tname;
                newRefTable.Columns.Add("Country", typeof(string));
                foreach (DataRow y in Plugin.settingsData.Cur_Years.Rows) newRefTable.Columns.Add(y["Year"].ToString(), typeof(double));
                foreach (DataRow c in Plugin.settingsData.Cur_Countries.Rows) 
                {
                    DataRow nr = newRefTable.NewRow();
                    foreach (DataRow y in Plugin.settingsData.Cur_Years.Rows) nr[y["Year"].ToString()] = 0;
                    nr["Country"] = c["Country"].ToString();
                    newRefTable.Rows.Add(nr);
                }
                Plugin.referenceTablesData.Tables.Add(newRefTable);
                //                Plugin.saveReferenceTables();
                return true;
            }
            return false;
        }

        internal static string getUniqueRefTableName(Program plugin)
        {
            Random r = new Random();
            string tname;
            do
            {
                tname = "RefTable_" + r.Next();
            }
            while (plugin.referenceTablesData.Tables.Contains(tname));
            return tname;
        }

        private void ManageReferenceTables_FormClosing(object sender, FormClosingEventArgs e)
        {
            buttonAddTable.Focus();
            if (!saveChanges())
            {
                e.Cancel = true;
            }
        }

        private void buttonRemoveTable_Click(object sender, EventArgs e)
        {
            // first try to save any pending changes
            if (!saveChanges()) return;
            // then remove the selected new table
            if (listBoxControlTables.ItemCount < 2)
            {
                MessageBox.Show("You cannot delete the last Reference Table!");
                return;
            }
            if (MessageBox.Show("Are you sure you want to delete the selected Reference Table?", "Delete Reference Table", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                blockVisualUpdates();
                string tn = listBoxControlTables.SelectedValue.ToString();
                if (Plugin.referenceTablesData.Tables.Contains(tn)) Plugin.referenceTablesData.Tables.Remove(tn);
                DataRow row = Plugin.referenceTablesData.ReferenceTables.Select("TableName='" + tn + "'").First();
                Plugin.referenceTablesData.ReferenceTables.Rows.Remove(row);
//                Plugin.saveReferenceTables();
                listBoxControlTables.Refresh();
                listBoxControlTables.SelectedIndex = 0;
                updateGrid();
                unblockVisualUpdates();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Plugin.saveReferenceTables();
            MessageBox.Show("Reference Tables saved.");
        }

        private void textEdit1_Leave(object sender, EventArgs e)
        {
            textEdit1.DoValidate();
            BindingContext[Plugin.referenceTablesData.ReferenceTables].EndCurrentEdit();
        }

        private void gridControl1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.V)
            {
                GetFromClipboard();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        void GetFromClipboard()
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                string[] clipboardLines = clipboardText.Split('\n');
                int indexRow = gridView1.FocusedRowHandle;
                int currentCellColumnIndex = gridView1.FocusedColumn.VisibleIndex;
                foreach (string line in clipboardLines)
                {
                    if (line.Length > 0)
                    {
                        if (indexRow == gridView1.RowCount) break; // this should stop new rows from being added 
                        string[] clipboardCells = line.Split('\t');
                        for (int indexColumn = 0; indexColumn < clipboardCells.GetLength(0); ++indexColumn)
                        {
                            if (currentCellColumnIndex + indexColumn == 0) continue;
                            if (currentCellColumnIndex + indexColumn < gridView1.Columns.Count)
                            {
                                double x;
                                if (double.TryParse(clipboardCells[indexColumn].Trim(), out x))
                                    gridView1.SetRowCellValue(indexRow, gridView1.Columns[currentCellColumnIndex + indexColumn], x);    // trim to avoid new lines and spaces
                            }
                            else
                                break;
                        }
                        indexRow++;
                    }
                    else
                        break;
                }
            }
            catch
            {
                return;
            }
        }

        private void listBoxControlTables_MouseMove(object sender, MouseEventArgs e)
        {
            ListBoxControl listBoxControl = sender as ListBoxControl;
            int index = listBoxControl.IndexFromPoint(new Point(e.X, e.Y));
            if(index != -1) {
                string item = (listBoxControl.GetItem(index) as DataRowView).Row[1] as string;
                toolTipController1.ShowHint(item, listBoxControl.PointToScreen(new Point(e.X, e.Y)));
            } else {
                toolTipController1.HideHint();
            }
        }

        private void listBoxControlTables_MouseLeave(object sender, EventArgs e)
        {
            toolTipController1.HideHint();
        }
    }

    class ListBoxItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

}
