using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EM_UI.IndirectTaxes
{
    internal partial class IndirectTaxesForm : Form
    {
        private EM_UI_MainForm mainForm = null;
        internal CountryConfigFacade _countryConfigFacade = null;
        private const string colYear = "colYear";
        internal const char separator = '°';
        internal const char separatorInner = '|';
        private DataSet dataSet = new DataSet();
        private DataTable dataTable = new DataTable("IttDataTable");
        private ADOUndoManager undoManager = new ADOUndoManager();
        private bool keepUndoData = false;
        private bool columnsChanged = false;

        internal IndirectTaxesForm(EM_UI_MainForm _mainForm)
        {
            InitializeComponent();

            try
            {
                mainForm = _mainForm;
                _countryConfigFacade = EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(mainForm.GetCountryShortName());

                dataSet.Tables.Add(dataTable);
                table.DataSource = dataSet;
                table.DataMember = "IttDataTable";

                LoadTableContent();

                // only after all the data is loaded, add the row numbers and the refresh listeners
                table.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
                table.RowsAdded += new DataGridViewRowsAddedEventHandler(table_RefreshRowNumbers);
                table.RowsRemoved += new DataGridViewRowsRemovedEventHandler(table_RefreshRowNumbers);

                table_RefreshRowNumbers(null, null);
                colName.Frozen = true;
                //colComment.Frozen = true; // this can only happen if it is not the last row!
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        void LoadTableContent()
        {
            CountryConfig countryConfig = _countryConfigFacade.GetCountryConfig();
            keepUndoData = false;

            dataTable.PrimaryKey = new DataColumn[] { dataTable.Columns.Add("ID", typeof(Int16)) };
            dataTable.PrimaryKey[0].AutoIncrement = true;
            dataTable.Columns.Add(colName.Name);
            List<string> years = new List<string>();
            foreach (CountryConfig.IndirectTaxRow itr in countryConfig.IndirectTax)
                foreach (string year in DecomposeYearValues(itr.YearValues).Keys)
                    if (!years.Contains(year)) years.Add(year);
            foreach (string year in years) AddYearColumn(year);
            dataTable.Columns.Add(colComment.Name);

            foreach (CountryConfig.IndirectTaxRow itr in countryConfig.IndirectTax)
            {
                DataRow row = dataTable.Rows.Add();
                row.SetField(colName.Name, itr.Reference);
                row.SetField(colComment.Name, itr.Comment);
                foreach (var yv in DecomposeYearValues(itr.YearValues)) row.SetField(colYear + yv.Key.ToString(), yv.Value);
            }
            dataSet.AcceptChanges();
            undoManager.AddDataSet(dataSet);

            keepUndoData = true;
        }

        private Dictionary<string, string> DecomposeYearValues(string yearValuesString)
        {
            Dictionary<string, string> yearValues = new Dictionary<string, string>();
            foreach (string yv in yearValuesString.Split(separator))
            {
                if (string.IsNullOrEmpty(yv) || yv.Split(separatorInner).Count() != 2) continue;
                yearValues.Add(yv.Split(separatorInner)[0], yv.Split(separatorInner)[1].
                    Replace(NumberFormatInfo.CurrentInfo.NumberDecimalSeparator, EM_Helpers.uiDecimalSeparator)); // make sure you always show "." as decimal separator
            }
            return yearValues;
        }

        void AddYearColumn(string caption)
        {
            DataColumn col = dataTable.Columns.Add(colYear + caption);
            //add before the comments-column
            col.SetOrdinal(dataTable.Columns[colComment.Name] != null ? dataTable.Columns[colComment.Name].Ordinal : dataTable.Columns.Count - 1);
            DataGridViewColumn headerColumn = table.Columns[colYear + caption];
            headerColumn.FillWeight = 80F;
            headerColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            headerColumn.HeaderText = caption;
            headerColumn.Name = colYear + caption;
            headerColumn.DataPropertyName = headerColumn.Name;
            headerColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumn.DisplayIndex = col.Ordinal;

            foreach (DataRow row in dataTable.Rows) row[headerColumn.Name] = ""; // this is to make sure the added column counts as a DataSet change...

            //also add year to the combo-box that allows for selecting a year to delete
            cmbYearToDelete.Items.Add(caption);

            int nextYear = EM_Helpers.SaveConvertToInt(caption) + 1;
            if (updwnYearToAdd.Value < nextYear)
                updwnYearToAdd.Value = nextYear;
            table.Refresh();
        }

        internal List<string> GetExistingYears()
        {
            List<string> existingYears = new List<string>();
            foreach (string year in cmbYearToDelete.Items) existingYears.Add(year);
            return existingYears;
        }

        void IndirectTaxesForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!verifyNumericValues()) UserInfoHandler.ShowError("Cell(s) with invalid value found!");
            else if (!haveEmptyCells() || UserInfoHandler.GetInfo("Empty cell(s) found; they will be treated as n/a when running the model." + Environment.NewLine +
                                                                  "Do you want to correct?", MessageBoxButtons.YesNo) == DialogResult.No)
            {
                mainForm.PerformAction(new IndirectTaxesAction(this), false);
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        bool haveEmptyCells()
        {
            foreach (DataGridViewRow row in table.Rows)
            {
                if (row.IsNewRow) break;
                for (int c = 2; c < table.ColumnCount - 1; c++)
                {
                    DataGridViewCell cell = row.Cells[c];
                    if (cell.Value == null || cell.Value.ToString() == "") return true;
                }
            }
            return false;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void btnAddYear_Click(object sender, EventArgs e)
        {
            keepUndoData = false;
            string year = updwnYearToAdd.Value.ToString();
            if (GetExistingYears().Contains(year)) { UserInfoHandler.ShowError(year + " already exits."); return; }
            if (UserInfoHandler.GetInfo("Are you sure you want to add a new column?\n\nNote: you will not be able to undo this action or any action before this.",
                MessageBoxButtons.YesNo) == DialogResult.No) return;
            AddYearColumn(year); keepUndoData = true; dataSet.AcceptChanges(); undoManager.Reset(); columnsChanged = true;
        }

        void btnDeleteYear_Click(object sender, EventArgs e)
        {
            if (cmbYearToDelete.Text == string.Empty) { UserInfoHandler.ShowError("Please select a year.");return; }
            if (UserInfoHandler.GetInfo("Are you sure you want to delete this column?\n\nNote: you will not be able to undo this action or any action before this.",
                MessageBoxButtons.YesNo) == DialogResult.No) return;
            for (int index = table.Columns.Count - 1; index >= 0; --index)
            {
                if (table.Columns[index].HeaderText == cmbYearToDelete.Text)
                {
                    dataTable.Columns.Remove(table.Columns[index].Name);
                    //table.Columns.RemoveAt(index);
                    break;
                }
            }
            foreach (DataRow row in dataTable.Rows) row[0] = row[0]; // this is to make sure the deleted column counts as a DataSet change...

            cmbYearToDelete.Items.RemoveAt(cmbYearToDelete.SelectedIndex);
            dataSet.AcceptChanges(); undoManager.Reset(); columnsChanged = true;
        }

        List<DataGridViewColumn> GetYearColumnsAsDisplayed()
        {
            SortedList<int, DataGridViewColumn> sortedColumns = new SortedList<int, DataGridViewColumn>();
            foreach (DataGridViewColumn yearColumn in table.Columns)
                if (yearColumn.Name.StartsWith(colYear))
                    sortedColumns.Add(yearColumn.DisplayIndex, yearColumn);
            List<DataGridViewColumn> yearColumnsAsDisplayed = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn yearColumn in sortedColumns.Values)
                yearColumnsAsDisplayed.Add(yearColumn);
            return yearColumnsAsDisplayed;
        }

        int GetYearColumnIndexByDisplayIndex(int displayIndex)
        {
            foreach (DataGridViewColumn column in table.Columns)
                if (column.DisplayIndex == displayIndex) return column.Index;
            return -1;
        }

        void ClearCells()
        {
            if (table.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                foreach (DataGridViewCell cell in table.SelectedCells)
                {
                    if (cell.OwningRow.DataBoundItem != null)
                    {
                        if (cell.ColumnIndex == colID.Index) continue; // do not delete the id-column's content which is not allowed to be empty (happens on selecting row via the header cell)
                        cell.Value = "";
                        (cell.OwningRow.DataBoundItem as DataRowView).Row.EndEdit();
                    }
                }
            }
        }

        void CopyToClipboard()
        {
            if (table.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try { Clipboard.SetDataObject(table.GetClipboardContent()); }
                catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            }
        }

        void GetFromClipboard()
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                string[] clipboardLines = clipboardText.Split('\n');
                int indexRow = table.CurrentCell.RowIndex;
                bool addedExtraLine = (indexRow == dataTable.Rows.Count);
                int currentCellColumnIndex = table.Columns[table.CurrentCell.ColumnIndex].DisplayIndex;
                foreach (string line in clipboardLines)
                {
                    if (line.Length > 0)
                    {
                        DataRow row;
                        if (indexRow == dataTable.Rows.Count)
                        {
                            row = dataTable.NewRow();
                            dataTable.Rows.Add(row);
                        }
                        else
                            row = dataTable.Rows[indexRow];

                        string[] clipboardCells = line.Split('\t').Select(p => p.Trim()).ToArray(); // trim cells to avoid unwanted new lines and spaces at the end
                        for (int indexColumn = 0; indexColumn < clipboardCells.GetLength(0); ++indexColumn)
                        {
                            if (currentCellColumnIndex + indexColumn < table.ColumnCount)
                            {
                                // if one of the default columns
                                if (currentCellColumnIndex + indexColumn == table.Columns[colName.Name].DisplayIndex ||
                                    currentCellColumnIndex + indexColumn == table.Columns[colComment.Name].DisplayIndex)
                                    row[GetYearColumnIndexByDisplayIndex(currentCellColumnIndex + indexColumn)] = clipboardCells[indexColumn];
                                else    // if one of the system columns
                                    row[GetYearColumnIndexByDisplayIndex(currentCellColumnIndex + indexColumn)] = EM_Helpers.SetCleanInvariantNumberFormat(clipboardCells[indexColumn]);    // fix decimal separator
                            }
                            else
                                break;
                        }
                        (table[0, indexRow].OwningRow.DataBoundItem as DataRowView).Row.EndEdit();
                        indexRow++;
                    }
                    else break;
                }
                if (addedExtraLine) table.Rows.RemoveAt(table.Rows.Count - 2);    // get rid of the extra line added by the grid
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return; }
        }

        private void table_KeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Control && (keyEventArgs.KeyCode == Keys.C || keyEventArgs.KeyCode == Keys.Insert))
            {
                CopyToClipboard();
            }
            else if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.V) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Insert))
            {
                keepUndoData = false;
                GetFromClipboard();
                keepUndoData = true;
                storeUndoAction();
            }
            else if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.X) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Delete))
            {
                keepUndoData = false;
                CopyToClipboard();
                ClearCells();
                keepUndoData = true;
                storeUndoAction();
            }
            else if (keyEventArgs.KeyCode == Keys.Delete)
            {
                keepUndoData = false;
                ClearCells();
                keepUndoData = true;
                storeUndoAction();
            }
            else if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Z)
            {
                applyUndo();
            }
            else if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Y)
            {
                applyRedo();
            }
        }

        private void deleteSelectedRows()
        {
            if (table.SelectedRows.Count < 1 || UserInfoHandler.GetInfo("Are you sure you want to remove the selected row(s)?\n\nNote: you will not be able to undo this action or any action before this.",
                MessageBoxButtons.YesNo) == DialogResult.No) return;
            keepUndoData = false;
            foreach (DataGridViewRow vrow in table.SelectedRows)
            {
                if (!vrow.IsNewRow) dataTable.Rows.Remove((vrow.DataBoundItem as DataRowView).Row);   // ignore delete for the empty new row
            }
            keepUndoData = true; dataSet.AcceptChanges(); undoManager.Reset(); columnsChanged = true;
        }

        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e) { deleteSelectedRows(); }

        private void table_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = table.HitTest(e.X, e.Y).RowIndex;
                if (currentMouseOverRow >= 0)
                {
                    bool keepSelection = false;
                    foreach (DataGridViewRow vrow in table.SelectedRows)
                    {
                        if (vrow.Index == currentMouseOverRow) keepSelection = true;
                    }
                    if (!keepSelection)
                    {
                        table.ClearSelection();
                        table.Rows[currentMouseOverRow].Selected = true;
                    }
                    contextMenuStripRows.Show(table, e.X, e.Y);
                }
            }

        }

        private void table_RefreshRowNumbers(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in table.Rows) row.HeaderCell.Value = (row.Index + 1).ToString();
        }

        private bool verifyNumericValues()
        {
            Regex rgx = new Regex("[^0-9.%]");
            foreach (DataGridViewRow row in table.Rows)
            {
                // if you reached the new  line, you are good!
                if (row.IsNewRow) return true;
                DataGridViewCell cell = row.Cells[1];
                // if the reference is null or empty, focus and exit with false
                if (cell.Value == null || cell.Value.ToString() == "")
                {
                    table.ClearSelection();
                    cell.Selected = true;
                    table.CurrentCell = cell;
                    table.BeginEdit(true);
                    return false;
                }
                // check the value cells
                for (int c = 0; c < table.ColumnCount; c++)
                {
                    if (!table.Columns[c].Name.StartsWith(colYear)) continue;
                    cell = row.Cells[c];
                    bool allOK = true;
                    if (cell.Value == null || cell.Value.ToString() == "" || cell.Value.ToString() == DefPar.Value.NA) // if n/a or empty do nothing
                    {
                        //sallOK = false;
                    }
                    else
                    {
                        string sval = cell.Value.ToString();
                        if (rgx.IsMatch(sval)) allOK = false; // if it contains any unwanted characters
                        else if (sval.Count(x => x == '.') > 1) allOK = false; // if it has more than one dot
                        else if (sval.IndexOf('%') > 0 && sval.IndexOf('%') != sval.Length - 1) allOK = false; // if % is not at the end
                    }

                    if (allOK == false) // if something was wrong, focus and exit with false
                    {
                        table.ClearSelection();
                        cell.Selected = true;
                        table.CurrentCell = cell;
                        table.BeginEdit(true);
                        return false;
                    }
                }
            }
            return true;
        }

        void IndirectTaxesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == DialogResult.Cancel)
                if (dataSet.GetChanges() != null || undoManager.HasChanges() || columnsChanged)
                    if (UserInfoHandler.GetInfo("Are you sure you want to close without saving?", MessageBoxButtons.YesNo) == DialogResult.No)
                        e.Cancel = true;
        }

        private void storeUndoAction()
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                if (dataSet.HasChanges())
                {
                    Point cell = table.CurrentCellAddress;
                    if (cell.X == -1) cell = new Point(table.SelectedCells[0].ColumnIndex, table.SelectedCells[0].RowIndex);
                    undoManager.Commit();
                    table.CurrentCell = table[Math.Min(cell.X, table.ColumnCount - 1), Math.Min(cell.Y, table.RowCount - 1)];
                }
                keepUndoData = true;
            }
        }

        private void applyUndo()
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                Point cell = table.CurrentCellAddress;
                if (cell.X == -1) cell = new Point(table.SelectedCells[0].ColumnIndex, table.SelectedCells[0].RowIndex);
                undoManager.Undo();
                table.CurrentCell = table[Math.Min(cell.X, table.ColumnCount - 1), Math.Min(cell.Y, table.RowCount - 1)];
                keepUndoData = true;
            }
        }

        private void applyRedo()
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                Point cell = GetCurrentCell();
                undoManager.Redo();
                SetCurrentCell(cell);
                keepUndoData = true;
            }
        }

        Point GetCurrentCell()
        {
            Point cell = table.CurrentCellAddress;
            if (cell.X == -1) cell = new Point(table.SelectedCells[0].ColumnIndex, table.SelectedCells[0].RowIndex);
            return cell;
        }

        void SetCurrentCell(Point cell)
        {
            table.CurrentCell = table[Math.Min(cell.X, table.ColumnCount - 1), Math.Min(cell.Y, table.RowCount - 1)];
        }

        void table_CellEndEdit(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (keepUndoData)
            {
                (table.CurrentRow.DataBoundItem as DataRowView).Row.EndEdit();
                storeUndoAction();
            }
        }

        internal void SaveTable()
        {
            List<Tuple<string, string, string>> indTaxes = new List<Tuple<string, string, string>>();
            foreach (DataGridViewRow row in table.Rows)
            {
                if (row.Cells[colName.Name].Value == null || string.IsNullOrEmpty(row.Cells[colName.Name].Value.ToString())) continue; //probably an empty row

                string name = row.Cells[colName.Name].Value == null ? string.Empty : row.Cells[colName.Name].Value.ToString();
                string comment = row.Cells[colComment.Name].Value == null ? string.Empty : row.Cells[colComment.Name].Value.ToString();
                string yearValues = string.Empty;
                foreach (DataGridViewColumn yearColumn in GetYearColumnsAsDisplayed())
                {
                    if (row.Cells[yearColumn.Index].Value != null)
                        yearValues += yearColumn.HeaderText + separatorInner + row.Cells[yearColumn.Index].Value.ToString();
                    yearValues += separator;
                }
                indTaxes.Add(new Tuple<string, string, string>(name, comment, yearValues.TrimEnd(new char[] { separator })));
            }

            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(mainForm.GetCountryShortName()).GetCountryConfig();
            foreach (var indTax in indTaxes)
            {
                List<CountryConfig.IndirectTaxRow> its = (from it in countryConfig.IndirectTax where it.Reference.ToLower() == indTax.Item1.ToLower() select it).ToList();
                if (its.Count == 0) countryConfig.IndirectTax.AddIndirectTaxRow(Guid.NewGuid().ToString(), indTax.Item1, indTax.Item2, indTax.Item3);
                else { its[0].Reference = indTax.Item1; its[0].Comment = indTax.Item2; its[0].YearValues = indTax.Item3; }
            }
            List<string> itNames = (from i in indTaxes select i.Item1.ToLower()).ToList();
            for (int i = countryConfig.IndirectTax.Count - 1; i >= 0; --i)
                if (!itNames.Contains(countryConfig.IndirectTax[i].Reference.ToLower())) countryConfig.IndirectTax.ElementAt(i).Delete();
        }
    }
}
