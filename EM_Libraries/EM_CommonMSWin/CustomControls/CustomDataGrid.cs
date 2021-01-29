using EM_Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EM_Common_Win
{
    public partial class CustomDataGrid : DataGridView
    {
        private ADOUndoManager undoManager = null;
        private DataSet gridDataSet = new DataSet(); // undoManager requires a dataset, otherwise gridDataTable could be used as DataSource (so gridDataSet is just a wrapper)
        private DataTable gridDataTable = new DataTable("CustomDataGridDataTable");
        private bool keepUndoData = true;
        private bool structureChanged = false; // indicates added or removed rows/columns
        private DataGridViewRow menuRowClicked = null;

        public string lastError = string.Empty;
        public bool allowAddRows = true; // if set true, a context menu offers these functionalities
        public bool allowDelRows = true;
        public bool showAddRowWarning = false;
        public bool showDelRowWarning = false;
        public List<string> pasteableColumns = new List<string>();
        private Dictionary<string, Type> columnType = new Dictionary<string, Type>();

        // important note: derived classes should not access DataGridView.DataSource directly to change the content of the grid (add/del coumns/rows, edit fields)
        // functions Add(Remove)Row(Column) should be used instead 
        // it is ok to read the content of DataGridView.DataSource (as DataSet), but saver to use GetDataTable
        
        public CustomDataGrid()
        {
            InitializeComponent();

            KeyDown += new KeyEventHandler(Custom_KeyDown);
            DataError += Custom_DataError;
            CellEndEdit += Custom_CellEndEdit;
            MouseClick += Custom_MouseClick;
            AllowUserToAddRows = AllowUserToDeleteRows = false; // switch off DataGridView's row-adding mechanism (is replaced by context-menu)
        }
       
        public bool InitDataSource() // this needs to be called at the very beginning of the constructor of derived classes (also see StartUndoManager below)
        {
            lastError = string.Empty;
            try
            {
                gridDataSet.Tables.Add(gridDataTable);
                DataSource = gridDataSet;
                DataMember = gridDataTable.TableName;
                return true;
            }
            catch (Exception exception) { lastError = "Initialising data-source failed." + Environment.NewLine + exception.Message; return false; }
        }

        public bool StartUndoManager() // this needs to be called after the first (by program) filling of the grid, i.e. undo starts after initialising the grid
        {
            lastError = string.Empty;
            try
            {
                if (gridDataTable.PrimaryKey.Count() == 0) throw new Exception("DataTable does not have a PrimaryKey.");
                gridDataSet.AcceptChanges(); // accept (by program) changes of initialising the grid
                undoManager = new ADOUndoManager(); undoManager.AddDataSet(gridDataSet);
                return true;
            }
            catch (Exception exception) { undoManager = null; lastError = "Enabling undo-functionality failed." + Environment.NewLine + exception.Message; return false; }
        }

        public DataTable GetDataTable() { return gridDataTable; } // note: this should only be used for reading-purposes (see 'important note' above)

        public bool AddColumn(bool showWarning, string colName, Type type = null, int displayIndex = -1, bool asPrimaryKey = false)
        {
            lastError = string.Empty;
            if (!AcceptWarning(showWarning)) return false;
            try
            {
                PauseUndoBeginn();
                DataColumn dataCol = gridDataTable.Columns.Add(colName);
                columnType.Add(colName, type == null ? typeof(string) : type);
                if (asPrimaryKey) { gridDataTable.PrimaryKey = new DataColumn[] { dataCol }; gridDataTable.PrimaryKey[0].AutoIncrement = true; }
                if (displayIndex != -1) { dataCol.SetOrdinal(displayIndex); Columns[colName].DisplayIndex = dataCol.Ordinal; }
                return true;
            }
            catch (Exception exception) { lastError = "Adding column '" + colName + "' failed." + Environment.NewLine + exception.Message; return false; }
            finally { PauseUndoEnd(true); }
        }

        public bool AddRow(bool showWarning, out DataRow dataRow) { return AddRowP(showWarning, out dataRow, null, true); }
        public bool AddRow(bool showWarning, out DataRow dataRow, DataGridViewRow anker, bool above)
        {
            lastError = string.Empty; dataRow = null;
            if (anker == null || anker.DataBoundItem == null) { lastError = "Adding row failed. Anker row could not be accessed."; return false; } // stupid error message, but hopefully does not happen (in final implementation)
            DataRowView view = anker.DataBoundItem as DataRowView; if (view == null) { lastError = "Adding row failed. Anker row could not be accessed."; return false; }
            return AddRowP(showAddRowWarning, out dataRow, view.Row, above);
        }

        private bool AddRowP(bool showWarning, out DataRow dataRow, DataRow anker, bool above)
        {
            lastError = string.Empty; dataRow = null;
            if (!AcceptWarning(showWarning)) return false;
            try
            {
                PauseUndoBeginn();
                if (anker == null)
                {
                    dataRow = gridDataTable.Rows.Add(new object[gridDataTable.Columns.Count]);
                    if (undoManager != null) try { CurrentCell = this[0, RowCount - 1]; } catch {}
                }
                else
                {
                    dataRow = gridDataTable.NewRow();
                    int at = gridDataTable.Rows.IndexOf(anker) + (above ? 0 : 1);
                    gridDataTable.Rows.InsertAt(dataRow, at);
                    if (undoManager != null) try { CurrentCell = this[0, at]; } catch {}
                }
                return true;
            }
            catch (Exception exception) { lastError = "Adding row failed." + Environment.NewLine + exception.Message; return false; }
            finally { PauseUndoEnd(true); }
        }

        private static bool AcceptWarning(bool showWarning)
        {
            return !showWarning || MessageBox.Show("Are you sure you want to do this?" + Environment.NewLine + Environment.NewLine +
                   "Note: you will not be able to undo this action or any action before this.", "Add Column", MessageBoxButtons.YesNo) == DialogResult.Yes;
        }

        public bool SetCellValue(DataGridViewRow gridRow, string colName, object value, bool storeUndoAction = true)
        {
            lastError = string.Empty;
            if (gridRow == null || gridRow.DataBoundItem == null) { lastError = "Changing value failed. Row could not be accessed."; return false; }
            DataRowView view = gridRow.DataBoundItem as DataRowView; if (view == null) { lastError = "Changing value failed. Row row could not be accessed."; return false; }
            return SetCellValue(view.Row, colName, value, storeUndoAction);
        }
        public bool SetCellValue(DataRow dataRow, string colName, object value, bool storeUndoAction = true)
        {
            lastError = string.Empty;
            try
            {
                dataRow.SetField(colName, value);
                if (storeUndoAction) StoreUndoAction(); // suppressing the storage of an action for undo allows for storing two (or more) actions in one undo
                return true;
            }
            catch (Exception exception) { lastError = string.Format("Setting value of column '{0}' failed.{1}{2}", colName, Environment.NewLine, exception.Message); return false; }
        }

        public bool RemoveColumn(bool showWarning, string colName)
        {
            lastError = string.Empty;
            if (!AcceptWarning(showWarning)) return false;
            try
            {
                PauseUndoBeginn();
                gridDataTable.Columns.Remove(colName);
                return true;
            }
            catch (Exception exception) { lastError = "Deleting column failed." + Environment.NewLine + exception.Message; return false; }
            finally { PauseUndoEnd(true); }
        }

        public bool RemoveRow(bool showWarning, string idColName, object idColValue) // only required for deleting controlled by program (direct user deleting is handled via context-menu)
        {
            foreach (DataRow row in gridDataTable.Rows)
                if (row.Field<string>(idColName).ToString() == idColValue.ToString()) return RemoveRow(showWarning, row);
            lastError = "Deleting row failed." + Environment.NewLine + string.Format("Row '{0} = {1}' not found.", idColName, idColValue);
            return false;
        }
        public bool RemoveRow(bool showWarning, DataGridViewRow row) { return RemoveRow(showWarning, (row.DataBoundItem as DataRowView).Row); }
        private bool RemoveRow(bool showWarning, DataRow row)
        {
            lastError = string.Empty;
            if (!AcceptWarning(showWarning)) return false;
            try
            {
                PauseUndoBeginn();
                gridDataTable.Rows.Remove(row);
                return true;
            }
            catch (Exception exception) { lastError = "Deleting row failed." + Environment.NewLine + exception.Message; return false; }
            finally { PauseUndoEnd(true); }
        }

        public void RemoveSelectedRows() { MenuItemClick_DelSelectedRows(null, null); }

        public bool HasChanges()
        {
            return (undoManager != null && undoManager.HasChanges()) || structureChanged;
        }

        private void Custom_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            lastError = string.Empty;
            try { MessageBox.Show(e.Exception.Message); } // most likely happens because of entering a string into a numeric field
            catch (Exception exception) { HandleUnexpectedError(exception); } // possible improvement: put more effort in a nicer user-info (or catch error in validate-function)
        }

        private void Custom_KeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.V) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Insert))
                { PauseUndoBeginn(); GetFromClipboard(); PauseUndoEnd(); StoreUndoAction(); }
            else if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.X) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Delete))
                { PauseUndoBeginn(); CopyToClipboard(); ClearSelectedCells(); PauseUndoEnd(); StoreUndoAction(); }
            else if (keyEventArgs.KeyCode == Keys.Delete)
                { PauseUndoBeginn(); ClearSelectedCells(); PauseUndoEnd(); StoreUndoAction(); }
            else if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Z)
                ApplyUndo();
            else if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Y)
                ApplyRedo();
        }

        private void GetFromClipboard()
        {
            lastError = string.Empty;
            try
            {
                if (Clipboard.GetText() == string.Empty || CurrentCell == null) return;              
                
                DataGridViewColumn colStart = Columns[CurrentCell.ColumnIndex];
                int rowIndex = CurrentCell.RowIndex;

                foreach (string clipboardLine in Clipboard.GetText().Split('\n'))
                {
                    if (rowIndex < 0 || clipboardLine.Length <= 0) break;
                    DataGridViewColumn col = colStart;
                    string[] clipboardCells = clipboardLine.Split('\t').Select(p => p.Trim()).ToArray(); // trim cells to avoid unwanted new lines and spaces at the end
                    foreach (string clipboardCell in clipboardCells)
                    {
                        if (col == null) break;
                        if (!col.ReadOnly && pasteableColumns.Contains(col.Name))
                        { // use try-catch to stay in the loop, just empty cell if the conversion fails
                            try {
                                if (columnType[col.Name] == typeof(double))
                                {
                                    Rows[rowIndex].Cells[col.Index].Value = Convert.ChangeType(clipboardCell.Replace(',', '.'), columnType[col.Name]).ToString();
                                }
                                else
                                {
                                    Rows[rowIndex].Cells[col.Index].Value = Convert.ChangeType(clipboardCell, columnType[col.Name]).ToString();
                                }
                            }
                            catch { Rows[rowIndex].Cells[col.Index].Value = DBNull.Value; }
                        }
                        col = Columns.GetNextColumn(col, DataGridViewElementStates.Visible, DataGridViewElementStates.None);
                    }
                    rowIndex = Rows.GetNextRow(rowIndex, DataGridViewElementStates.Visible);
                }
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        public bool verifyNumericValues(out string problems)
        {
            Regex rgx = new Regex("[^0-9.]"); problems = string.Empty;

            // first make sure that all editors are closed
            this.EndEdit();
            this.Refresh();

            // then check all cells
            foreach (DataGridViewRow row in Rows)
            {
                // if you reached the new line, you are good!
                if (row.IsNewRow) return true;

                foreach (DataGridViewColumn col in Columns)
                {
                    bool allOK = true;

                    // check only the "double" cells
                    if (columnType[col.Name] == typeof(double))
                    {
                        DataGridViewCell cell = row.Cells[col.Name];

                        if (cell.Value == null || cell.Value.ToString() == "")    // if null or empty do nothing
                        {
                            //sallOK = false;
                        }
                        else
                        {
                            string sval = cell.Value.ToString();
                            if (rgx.IsMatch(sval)) // if it contains any unwanted characters
                            {
                                allOK = false;
                            }
                            else if (sval.Count(x => x == '.') > 1) // if it has more than one dot
                            {
                                allOK = false;
                            }
                            if (allOK == false) // if something was wrong, focus and exit with false
                            {
                                ClearSelection();
                                cell.Selected = true;
                                CurrentCell = cell;
                                BeginEdit(true);
                                problems = $"{sval} ({col.Name}, row {Rows.IndexOf(row)})";
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        void CopyToClipboard()
        {
            try { Clipboard.SetDataObject(GetClipboardContent()); } catch {}
        }

        private void ClearSelectedCells()
        {
            lastError = string.Empty;
            try
            {
                foreach (DataGridViewCell cell in SelectedCells)
                {
                    if (cell.OwningRow.DataBoundItem == null || cell.ReadOnly || !cell.Visible) continue;
                    cell.Value = DBNull.Value;
                    (cell.OwningRow.DataBoundItem as DataRowView).Row.EndEdit();
                }
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private void StoreUndoAction()
        {
            lastError = string.Empty;
            try
            {
                if (undoManager == null || !gridDataSet.HasChanges() || !keepUndoData) return;
                PauseUndoBeginn();
                Point cell = GetCurrentCell();
                undoManager.Commit();
                SetCurrentCell(cell);
                PauseUndoEnd();
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private void ApplyUndo()
        {
            lastError = string.Empty;
            try
            {
                if (undoManager == null || !keepUndoData) return;
                PauseUndoBeginn();
                Point cell = GetCurrentCell();
                undoManager.Undo();
                SetCurrentCell(cell);
                PauseUndoEnd();
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private void ApplyRedo()
        {
            lastError = string.Empty;
            try
            {
                if (undoManager == null || !keepUndoData) return;
                PauseUndoBeginn();
                Point cell = GetCurrentCell();
                undoManager.Redo();
                SetCurrentCell(cell);
                PauseUndoEnd();
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private Point GetCurrentCell()
        {
            Point cell = CurrentCellAddress;
            if (cell.X == -1) cell = new Point(SelectedCells[0].ColumnIndex, SelectedCells[0].RowIndex);
            return cell;
        }

        private void SetCurrentCell(Point cell) { CurrentCell = this[Math.Min(cell.X, ColumnCount - 1), Math.Min(cell.Y, RowCount - 1)]; }

        private void Custom_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            lastError = string.Empty;
            try
            {
                if (undoManager == null || !keepUndoData) return;
                (CurrentRow.DataBoundItem as DataRowView).Row.EndEdit();
                StoreUndoAction();
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private void PauseUndoBeginn() { keepUndoData = false; }
        private void PauseUndoEnd(bool isStructureChange = false)
        {
            try
            {
                keepUndoData = true;
                if (undoManager != null && isStructureChange) { gridDataSet.AcceptChanges(); undoManager.Reset(); structureChanged = true; }
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private void HandleUnexpectedError(Exception exception)
        {
            lastError = exception.Message; // actually no idea what to do
        }

        private void Custom_MouseClick(object sender, MouseEventArgs e)
        {
            if ((allowAddRows == false && allowDelRows == false)) return;
            if (HitTest(e.X, e.Y).Type != DataGridViewHitTestType.RowHeader) return;
            
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.ShowCheckMargin = false; menu.ShowImageMargin = false;
            if (allowAddRows)
            {
                menu.Items.Add("Add row above", null, MenuItemClick_AddRowAbove);
                menu.Items.Add("Add row below", null, MenuItemClick_AddRowBelow);
                if (allowDelRows) menu.Items.Add(new ToolStripSeparator());
            }
            if (allowDelRows)
            {
                bool inSelection = false;
                foreach (DataGridViewRow row in SelectedRows) { inSelection = true; break; }
                if (inSelection) menu.Items.Add("Delete Selected Rows", null, MenuItemClick_DelSelectedRows);
                else menu.Items.Add("Delete Row", null, MenuItemClick_DelRow);
            }
            menuRowClicked = Rows[HitTest(e.X, e.Y).RowIndex];
            menu.Show(this, e.X, e.Y);
        }

        private void MenuItemClick_AddRowAbove(object sender, EventArgs e) { DataRow dummy; AddRow(showAddRowWarning, out dummy, menuRowClicked, true); }
        private void MenuItemClick_AddRowBelow(object sender, EventArgs e)  { DataRow dummy; AddRow(showAddRowWarning, out dummy, menuRowClicked, false); }

        private void MenuItemClick_DelRow(object sender, EventArgs e)
        {
            try { RemoveRow(showDelRowWarning, (menuRowClicked.DataBoundItem as DataRowView).Row); }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }

        private void MenuItemClick_DelSelectedRows(object sender, EventArgs e)
        {
            if (!AcceptWarning(showDelRowWarning)) return;
            try
            {
                PauseUndoBeginn();
                foreach (DataGridViewRow row in SelectedRows) gridDataTable.Rows.Remove((row.DataBoundItem as DataRowView).Row);
                PauseUndoEnd(true);
            }
            catch (Exception exception) { HandleUnexpectedError(exception); }
        }
    }
}
