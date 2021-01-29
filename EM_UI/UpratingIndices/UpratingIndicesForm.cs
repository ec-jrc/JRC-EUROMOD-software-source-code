using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace EM_UI.UpratingIndices
{
    internal partial class UpratingIndicesForm : Form
    {
        internal EM_UI_MainForm _mainForm = null;
        internal CountryConfigFacade _countryConfigFacade = null;
        internal DataConfigFacade _dataConfigFacade = null;
        internal const char _separator = '°';
        internal const char _separatorYear = '|';
        const string _colYear = "colYear";
        internal const string _policyUprateFactors_Name = "DefUpratingFactors";
        internal const string _factorValueInvalid = "-99999999999";  // the value if a factor is missing or na
        DataSet dgvDataSet = new DataSet();
        DataTable dgvDataTable = new DataTable("dgvDataTable");
        ADOUndoManager undoManager = new ADOUndoManager();
        bool keepUndoData = false;
        bool columnsChanged = false;

        internal UpratingIndicesForm(EM_UI_MainForm mainForm)
        {
            InitializeComponent();

            try
            {
                _mainForm = mainForm;
                _countryConfigFacade = EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName());
                _dataConfigFacade = EM_UI.CountryAdministration.CountryAdministrator.GetDataConfigFacade(_mainForm.GetCountryShortName());

                dgvDataSet.Tables.Add(dgvDataTable);
                dgvIndices.DataSource = dgvDataSet;
                dgvIndices.DataMember = "dgvDataTable";

                //load the information for the 'Raw Indices' tab
                LoadIndices();

                //load the information for the 'Factors per Data and System' tab ...
                //... datasets (for selection by the user)
                foreach (DataConfig.DataBaseRow dataSet in _dataConfigFacade.GetDataBaseRows())
                    cmbDatasets.Items.Add(dataSet.Name);
                //... and systems (for the table)
                foreach (CountryConfig.SystemRow system in _countryConfigFacade.GetSystemRowsOrdered())
                {
                    bool showSystem = false;
                    foreach (DevExpress.XtraTreeList.Columns.TreeListColumn c in mainForm.treeList.VisibleColumns)
                        showSystem = showSystem || (c.Name.ToLower() == system.Name.ToLower());
                    if (showSystem)
                    {
                        DataGridViewColumn headerColumn = colIndexName.Clone() as DataGridViewColumn; //clone the factorname-column to overtake the settings
                        headerColumn.Name = system.ID; //to be able to identify each system column, when the table is filled
                        headerColumn.HeaderText = system.Name;
                        int index = dgvFactors.Columns.Add(headerColumn);
                        dgvFactors.Columns[index].Tag = system;
                    }
                }

                // only after all the data is loaded, add the row numbers and the refresh listeners
                dgvIndices.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
                dgvIndices.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(dgvIndices_RefreshRowNumbers);
                dgvIndices.RowsRemoved += new System.Windows.Forms.DataGridViewRowsRemovedEventHandler(dgvIndices_RefreshRowNumbers);

                dgvIndices_RefreshRowNumbers(null, null);
                colIndexDescription.Frozen = true;
                colReference.Frozen = true;
//                colComment.Frozen = true;     // this can only happen if it is not the last row!
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void LoadIndices()
        {
            keepUndoData = false;

            dgvDataTable.PrimaryKey = new DataColumn[] { dgvDataTable.Columns.Add("ID", typeof(Int16)) };
            dgvDataTable.PrimaryKey[0].AutoIncrement = true;
            dgvDataTable.Columns.Add(colIndexDescription.Name);
            dgvDataTable.Columns.Add(colReference.Name);
            foreach (string year in _countryConfigFacade.GetAllUpratingIndexYears()) AddYearColumn(year);
            dgvDataTable.Columns.Add(colComment.Name);

            GetHICPFromGlobalTable();

            foreach (CountryConfig.UpratingIndexRow index in _countryConfigFacade.GetUpratingIndices()) // add one row for each index
            {
                if (hicpFromGlobalTable && index.Reference.ToUpper() == "$HICP") continue; // ignore country's own definition of HICP as the one from the global table was already inserted above
                DataRow row = dgvDataTable.Rows.Add();
                row.SetField(colIndexDescription.Name, index.Description);
                row.SetField(colReference.Name, index.Reference);
                row.SetField(colComment.Name, index.Comment);
                foreach (var yv in _countryConfigFacade.GetUpratingIndexYearValues(index)) row.SetField(_colYear + yv.Key.ToString(), EM_Helpers.ConvertToString(yv.Value));   // make sure you always show "." as decimal separator
            }
            dgvDataSet.AcceptChanges();
            undoManager.AddDataSet(dgvDataSet);

            keepUndoData = true;
        }

        private bool hicpFromGlobalTable = true;
        private void GetHICPFromGlobalTable()
        {
            hicpFromGlobalTable = false;
            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false); if (hcf == null) return;
            List<string> hicpRow = new List<string>() { "Harmonised Index of Consumer Prices", "$HICP" }; string comment = string.Empty;
            foreach (DataColumn col in dgvDataTable.Columns)
            {
                if (!col.Caption.StartsWith(_colYear)) continue;
                HICPConfig.HICPRow globalHICP = hcf.GetHICP(EM_AppContext.Instance.GetActiveCountryMainForm().GetCountryShortName(), col.Caption.Substring(_colYear.Length));
                string hicpVal = "100";
                if (globalHICP != null) { hicpVal = EM_Helpers.ConvertToString(globalHICP.Value); comment = globalHICP.Comment; hicpFromGlobalTable = true; }  // make sure you always show "." as decimal separator
                hicpRow.Add(hicpVal);
            }
            hicpRow.Add(comment); hicpRow.Insert(0, null);
            if (hicpFromGlobalTable) dgvDataTable.Rows.Add(hicpRow.ToArray());
        }

        private void StoreHICPInGlobalTable()
        {
            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false); if (hcf == null) return; // do nothing if HICPConfig does not exist yet

            foreach (DataRow row in dgvDataTable.Rows)
            {
                if (row.Field<string>(colReference.Name) != "$HICP") continue;
                List<Tuple<string, int, double, string>> hicps = new List<Tuple<string, int, double, string>>();
                string comment = row.Field<string>(colComment.Name);
                string country = EM_AppContext.Instance.GetActiveCountryMainForm().GetCountryShortName();
                foreach (DataColumn col in dgvDataTable.Columns)
                {
                    if (!col.Caption.StartsWith(_colYear)) continue;
                    string sYear = col.Caption.Substring(_colYear.Length); int year; if (!int.TryParse(sYear, out year)) continue;
                    string sVal = row.Field<string>(col.Caption); double dVal; if (!double.TryParse(sVal, out dVal)) continue;
                    hicps.Add(new Tuple<string, int, double, string>(country, year, dVal, comment));
                }
                if (hcf.SetHICPs(hicps)) hcf.WriteXML();
            }
        }

        void AddYearColumn(string caption, bool manual = false)
        {
            DataColumn col = dgvDataTable.Columns.Add(_colYear + caption);
            //add before the comments-column
            col.SetOrdinal(dgvDataTable.Columns[colComment.Name] != null ? dgvDataTable.Columns[colComment.Name].Ordinal : dgvDataTable.Columns.Count - 1);
            DataGridViewColumn headerColumn = dgvIndices.Columns[_colYear + caption];
            headerColumn.FillWeight = 80F;
            headerColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            headerColumn.HeaderText = caption;
            headerColumn.Name = _colYear + caption;
            headerColumn.DataPropertyName = headerColumn.Name;
            headerColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumn.DisplayIndex = col.Ordinal;

            foreach (DataRow row in dgvDataTable.Rows) row[headerColumn.Name] = ""; // this is to make sure the added column counts as a DataSet change...

            //also add year to the combo-box that allows for selecting a year to delete
            cmbYearToDelete.Items.Add(caption);

            int nextYear = EM_Helpers.SaveConvertToInt(caption) + 1;
            if (updwnYearToAdd.Value < nextYear)
                updwnYearToAdd.Value = nextYear;
            dgvIndices.Refresh();
        }

        internal List<string> GetExistingYears()
        {
            List<string> existingYears = new List<string>();
            foreach (string year in cmbYearToDelete.Items)
                existingYears.Add(year);
            return existingYears;
        }

        void UpratingIndicesForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (verifyNumericValues())
            {
                _mainForm.PerformAction(new UpratingIndicesAction(this), false);
                if (haveEmptyCells()) MessageBox.Show("Warning: one or more factor series have empty cells; a value of " + _factorValueInvalid + " will be used for these empty cells when running the model.");

                StoreHICPInGlobalTable();

                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                MessageBox.Show("There are cells with invalid values!");
            }
        }

        bool haveEmptyCells()
        {
            foreach (DataGridViewRow row in dgvIndices.Rows)
            {
                if (row.IsNewRow) break;
                for (int c = 3; c < dgvIndices.ColumnCount - 1; c++)
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
            if (GetExistingYears().Contains(year))
            {
                UserInfoHandler.ShowError(year + " already exits.");
                return;
            }
            if (MessageBox.Show("Are you sure you want to add a new column?\n\nNote: you will not be able to undo this action or any action before this.", "Add Year Column", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;
            AddYearColumn(year);
            keepUndoData = true;
            dgvDataSet.AcceptChanges();
            undoManager.Reset();
            columnsChanged = true;
        }

        void btnDeleteYear_Click(object sender, EventArgs e)
        {
            if (cmbYearToDelete.Text == string.Empty)
            {
                UserInfoHandler.ShowError("Please select a year.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this column?\n\nNote: you will not be able to undo this action or any action before this.", "Delete Year Column", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;

            for (int index = dgvIndices.Columns.Count - 1; index >= 0; --index)
            {
                if (dgvIndices.Columns[index].HeaderText == cmbYearToDelete.Text)
                {
                    dgvDataTable.Columns.Remove(dgvIndices.Columns[index].Name);
//                    dgvIndices.Columns.RemoveAt(index);
                    break;
                }
            }

            foreach (DataRow row in dgvDataTable.Rows) row[0] = row[0]; // this is to make sure the deleted column counts as a DataSet change...

            cmbYearToDelete.Items.RemoveAt(cmbYearToDelete.SelectedIndex);

            dgvDataSet.AcceptChanges();
            undoManager.Reset();
            columnsChanged = true;
        }

        void cmbDatasets_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDatasets.SelectedIndex == -1)
                return; //no dataset selected

            dgvFactors.Rows.Clear();

            //look up which dataset was selected to assess the data's income year, and put it into text-edit
            foreach (DataConfig.DataBaseRow dataset in _dataConfigFacade.GetDataBaseRows())
            {
                if (cmbDatasets.Text == dataset.Name)
                {
                    txtIncomeYear.Text = dataset.YearInc;
                    break;
                }
            }

            //check if the 'Raw Indices' table contains the data-year
            bool noIncomeYear = false;
            if (!GetExistingYears().Contains(txtIncomeYear.Text))
            {
                //UserInfoHandler.ShowError("Year " + txtIncomeYear.Text + " is not recorded in Raw Indices table. Factors cannot be calculated.");
                UserInfoHandler.ShowInfo("Year " + txtIncomeYear.Text + " is not recorded in Raw Indices table. Factors are set to " + _factorValueInvalid + ".");
                noIncomeYear = true;
                //return;
            }

            //fill the 'Factors ...' table for the selected dataset
            foreach (string indexName in GetIndicesNames()) //loop over all indices
            {
                //add one row to the 'Factors ...' table for each index
                DataGridViewRow factorRow = dgvFactors.Rows[dgvFactors.Rows.Add()];

                //put the name of the index (e.g. cpi) in the first column
                factorRow.Cells[colIndexName.Name].Value = indexName;

                foreach (DataGridViewColumn systemColumn in dgvFactors.Columns) //loop over the country's systems
                {
                    if (systemColumn == colIndexName)
                        continue; //name-column is already filled
                    if (noIncomeYear)
                        factorRow.Cells[systemColumn.Name].Value = _factorValueInvalid;
                    else
                    {
                        CountryConfig.SystemRow systemRow = systemColumn.Tag as CountryConfig.SystemRow;
                        string systemYear = systemRow.Year != null && systemRow.Year != string.Empty ? systemRow.Year
                                            : EM_Helpers.ExtractSystemYear((systemColumn.Tag as CountryConfig.SystemRow).Name);
                        // in the display, round the number to 4 digits - also make sure that you display dot regardless of the windows decimal separator. CalculateFactor takes care of all this...
                        factorRow.Cells[systemColumn.Name].Value = CalculateFactor(indexName, txtIncomeYear.Text, systemYear);
                    }
                }
            }
        }

        internal List<string> GetIndicesNames()
        {
            List<string> indicesNames = new List<string>();
            for (int i = 0; i < dgvIndices.Rows.Count; ++i) //loop over all indices
            {
                DataGridViewRow indexRow = dgvIndices.Rows[i]; //assess the name of the index (e.g. cpi)
                if (indexRow.Cells[colReference.Name].Value == null)
                    continue; //if there is no name for the index it's a useless (probably empty) row
                indicesNames.Add(indexRow.Cells[colReference.Name].Value.ToString());
            }
            return indicesNames;
        }

        internal string CalculateFactor(string indexName, string dataYear, string systemYear)
        {
            //search row of the relevant index
            int iRow = -1;
            for (int r = 0; r < dgvIndices.Rows.Count; ++r)
                if (dgvIndices.Rows[r].Cells[colReference.Name].Value != null &&
                    dgvIndices.Rows[r].Cells[colReference.Name].Value.ToString() == indexName)
                {
                    iRow = r;
                    break;  // if row found, exit loop
                }
            //search for column of the relevant years (data-year and system-year)
            int iColData = -1;
            int iColSystem = -1;
            for (int c = 0; c < dgvIndices.Columns.Count; ++c)
            {
                if (systemYear != string.Empty && dgvIndices.Columns[c].HeaderCell.Value.ToString() == systemYear)
                    iColSystem = c;
                if (dataYear != string.Empty && dgvIndices.Columns[c].HeaderCell.Value.ToString() == dataYear)
                    iColData = c;
            }

            //check if possible to calculate factor: may be impossible, because ...
            if (iRow == -1 || //... index-name not found in 'Raw Indeces' table (should not happen in fact)
                iColData == -1 || iColSystem == -1 || //... data-year or system-year not available in 'Raw Indeces' table
                dgvIndices.Rows[iRow].Cells[iColData].Value == null || //... index-value for data-year is not valid
                dgvIndices.Rows[iRow].Cells[iColData].Value.ToString() == string.Empty ||
                !EM_Helpers.IsNumeric(dgvIndices.Rows[iRow].Cells[iColData].Value.ToString()) ||
                dgvIndices.Rows[iRow].Cells[iColSystem].Value == null || //... index-value for system-year is not valid
                dgvIndices.Rows[iRow].Cells[iColSystem].Value.ToString() == string.Empty ||
                !EM_Helpers.IsNumeric(dgvIndices.Rows[iRow].Cells[iColSystem].Value.ToString()))
                return _factorValueInvalid;

            // Check the windows decimal separator and format the string accordingly before doing any conversions or operations
            string nval = dgvIndices.Rows[iRow].Cells[iColSystem].Value.ToString();
            string dval = dgvIndices.Rows[iRow].Cells[iColData].Value.ToString();
            
            double numerator = EM_Helpers.SaveConvertToDouble(nval);
            double denominator = EM_Helpers.SaveConvertToDouble(dval);

            return (denominator == 0) ? _factorValueInvalid : EM_Helpers.ConvertToString(Math.Round(numerator / denominator, 4));
        }

        void btnUpdate_Click(object sender, EventArgs e)
        {
            cmbDatasets_SelectedIndexChanged(null, null);
        }

        List<DataGridViewColumn> GetYearColumnsAsDisplayed()
        {   
            SortedList<int, DataGridViewColumn> sortedColumns = new SortedList<int,DataGridViewColumn>();
            foreach (DataGridViewColumn yearColumn in dgvIndices.Columns)
                if (yearColumn.Name.StartsWith(_colYear))
                    sortedColumns.Add(yearColumn.DisplayIndex, yearColumn);
            List<DataGridViewColumn> yearColumnsAsDisplayed = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn yearColumn in sortedColumns.Values)
                yearColumnsAsDisplayed.Add(yearColumn);
            return yearColumnsAsDisplayed;
        }

        int GetYearColumnIndexByDisplayIndex(int displayIndex)
        {
            foreach (DataGridViewColumn column in dgvIndices.Columns)
                if (column.DisplayIndex == displayIndex)
                    return column.Index;
            return -1;
        }

        internal string GetYearsInfo()
        {
            string yearsInfo = string.Empty;
            foreach (DataGridViewColumn yearColumn in GetYearColumnsAsDisplayed())
                    yearsInfo += yearColumn.HeaderText + _separator;
            return (yearsInfo != string.Empty) ? yearsInfo.Substring(0, yearsInfo.Length-1) : yearsInfo; //remove last °
        }

        internal List<Tuple<string, string, string, string>> GetRawIndicesInfo()
        {
            List<Tuple<string, string, string, string>> rawIndicesInfo = new List<Tuple<string, string, string, string>>();
            foreach (DataGridViewRow indexRow in dgvIndices.Rows)
            {
                if (indexRow.Cells[colReference.Name].Value == null || indexRow.Cells[colReference.Name].Value.ToString() == string.Empty)
                    continue; //probably an empty row
                
                string description = indexRow.Cells[colIndexDescription.Name].Value == null ? string.Empty : indexRow.Cells[colIndexDescription.Name].Value.ToString();
                string reference = indexRow.Cells[colReference.Name].Value.ToString();
                string comment = indexRow.Cells[colComment.Name].Value == null ? string.Empty : indexRow.Cells[colComment.Name].Value.ToString();
                string yearValues = string.Empty;
                foreach (DataGridViewColumn yearColumn in GetYearColumnsAsDisplayed())
                {
                    if (indexRow.Cells[yearColumn.Index].Value != null)
                        yearValues +=  yearColumn.HeaderText + _separatorYear + indexRow.Cells[yearColumn.Index].Value.ToString();
                    yearValues += _separator;
                }
                rawIndicesInfo.Add(new Tuple<string, string, string, string>(description, reference, comment,
                                                                             yearValues.TrimEnd(new char[] { UpratingIndicesForm._separator })));
            }
            return rawIndicesInfo;
        }

        void ClearCells()
        {
            if (dgvIndices.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                foreach (DataGridViewCell cell in dgvIndices.SelectedCells)
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
            if (dgvIndices.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    Clipboard.SetDataObject(dgvIndices.GetClipboardContent());
                }
                catch (Exception exception)
                {
                    UserInfoHandler.ShowException(exception);
                }
            }
        }

        void GetFromClipboard()
        {
            try
            {
                string clipboardText = Clipboard.GetText();
                string[] clipboardLines = clipboardText.Split('\n');
                int indexRow = dgvIndices.CurrentCell.RowIndex;
                bool addedExtraLine = (indexRow == dgvDataTable.Rows.Count);
                int currentCellColumnIndex = dgvIndices.Columns[dgvIndices.CurrentCell.ColumnIndex].DisplayIndex;
                foreach (string line in clipboardLines)
                {
                    if (line.Length > 0)
                    {
                        DataRow row;
                        if (indexRow == dgvDataTable.Rows.Count)
                        {
                            row = dgvDataTable.NewRow();
                            dgvDataTable.Rows.Add(row);
                        }
                        else
                            row = dgvDataTable.Rows[indexRow];

                        if (row.Field<string>(colReference.Name) != "$HICP") // do not copy anything into the read-only $HICP-row
                        {
                            string[] clipboardCells = line.Split('\t').Select(p => p.Trim()).ToArray(); // trim cells to avoid unwanted new lines and spaces at the end
                            for (int indexColumn = 0; indexColumn < clipboardCells.GetLength(0); ++indexColumn)
                            {
                                if (currentCellColumnIndex + indexColumn < dgvIndices.ColumnCount)
                                {
                                    // if one of the default columns
                                    if (currentCellColumnIndex + indexColumn == dgvIndices.Columns[colIndexDescription.Name].DisplayIndex ||
                                            currentCellColumnIndex + indexColumn == dgvIndices.Columns[colReference.Name].DisplayIndex ||
                                            currentCellColumnIndex + indexColumn == dgvIndices.Columns[colComment.Name].DisplayIndex)
                                        row[GetYearColumnIndexByDisplayIndex(currentCellColumnIndex + indexColumn)] = clipboardCells[indexColumn];
                                    else    // if one of the system columns
                                        row[GetYearColumnIndexByDisplayIndex(currentCellColumnIndex + indexColumn)] = clipboardCells[indexColumn].Replace(',', '.');    // fix decimal separator
                                }
                                else
                                    break;
                            }
                        }
                        (dgvIndices[0, indexRow].OwningRow.DataBoundItem as DataRowView).Row.EndEdit();
                        indexRow++;
                    }
                    else
                        break;
                }
                if (addedExtraLine) dgvIndices.Rows.RemoveAt(dgvIndices.Rows.Count - 2);    // get rid of the extra line added by the grid
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return;
            }
        }

        private void dgvIndices_KeyDown(object sender, KeyEventArgs keyEventArgs)
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
            if (dgvIndices.SelectedRows.Count < 1 || MessageBox.Show("Are you sure you want to remove the selected row(s)?\n\nNote: you will not be able to undo this action or any action before this.", "Remove Rows", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;

            keepUndoData = false;
            foreach (DataGridViewRow vrow in dgvIndices.SelectedRows)
            {
                if (!vrow.IsNewRow) dgvDataTable.Rows.Remove((vrow.DataBoundItem as DataRowView).Row);   // ignore delete for the empty new row
            }
            keepUndoData = true;
            dgvDataSet.AcceptChanges();
            undoManager.Reset();
            columnsChanged = true;
        }

        private void deleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteSelectedRows();
        }

        private void dgvIndices_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dgvIndices.HitTest(e.X, e.Y).RowIndex;
                int currentMouseOverColumn = dgvIndices.HitTest(e.X, e.Y).ColumnIndex;
                if (currentMouseOverRow >= 0)
                {
                    bool keepSelection = false;
                    foreach (DataGridViewRow vrow in dgvIndices.SelectedRows)
                    {
                        if (vrow.Index == currentMouseOverRow) keepSelection = true;
                    }
                    if (!keepSelection)
                    {
                        dgvIndices.ClearSelection();
                        dgvIndices.Rows[currentMouseOverRow].Selected = true;
                    }
                    contextMenuStripRows.Show(dgvIndices, e.X, e.Y);
                }
            }

        }

        private void dgvIndices_RefreshRowNumbers(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvIndices.Rows)
            {
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }
        }

        private bool verifyNumericValues()
        {
            Regex rgx = new Regex("[^0-9.]");

            // first make sure that all editors are closed
            dgvFactors.EndEdit();
            dgvFactors.Refresh();

            // then check all cells
            foreach (DataGridViewRow row in dgvIndices.Rows)
            {
                // if you reached the new  line, you are good!
                if (row.IsNewRow) return true;
                DataGridViewCell cell = row.Cells[1];
                // if the reference is null or empty, focus and exit with false
                if (cell.Value == null || cell.Value.ToString() == "")
                {
                    dgvIndices.ClearSelection();
                    cell.Selected = true;
                    dgvIndices.CurrentCell = cell;
                    dgvIndices.BeginEdit(true);
                    return false;
                }
                // check the value cells
                for (int c=0; c<dgvIndices.ColumnCount; c++)
                {
                    if (!dgvIndices.Columns[c].Name.StartsWith(_colYear)) continue;
                    cell = row.Cells[c];
                    bool allOK = true;
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
                    }

                    if (allOK == false) // if something was wrong, focus and exit with false
                    {
                        dgvIndices.ClearSelection();
                        cell.Selected = true;
                        dgvIndices.CurrentCell = cell;
                        dgvIndices.BeginEdit(true);
                        return false;
                    }
                }
            }
            return true;
        }

        void btnCheckUsage_Click(object sender, EventArgs e)
        {
            List<string> indices = new List<string>();
            foreach (DataGridViewRow row in dgvIndices.Rows)
                if (row.Index < dgvIndices.Rows.Count - 1)
                    indices.Add(string.Format("{0}{1}{2}{3}{4}", //i.e. "description°reference°comment" (e.g. "Earnings index°$f_yem°forecast from OBR")
                        row.Cells[colIndexDescription.Name].Value, UpratingIndicesUsageForm.separator,
                        row.Cells[colReference.Name].Value, UpratingIndicesUsageForm.separator,
                        row.Cells[colComment.Name].Value));

            UpratingIndicesUsageForm upratingIndicesUsageForm = new UpratingIndicesUsageForm(indices, _countryConfigFacade);
            upratingIndicesUsageForm.ShowDialog();
        }

        void UpratingIndicesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.Cancel)
                if (dgvDataSet.GetChanges() != null || undoManager.HasChanges() || columnsChanged)
                    if (UserInfoHandler.GetInfo("Are you sure you want to close without saving?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                        e.Cancel = true;
        }

        private void storeUndoAction()
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                if (dgvDataSet.HasChanges())
                {
                    Point cell = dgvIndices.CurrentCellAddress;
                    if (cell.X == -1) cell = new Point(dgvIndices.SelectedCells[0].ColumnIndex, dgvIndices.SelectedCells[0].RowIndex);
                    undoManager.Commit();
                    dgvIndices.CurrentCell = dgvIndices[Math.Min(cell.X, dgvIndices.ColumnCount-1), Math.Min(cell.Y, dgvIndices.RowCount-1)];
                }
                keepUndoData = true;
            }
        }

        private void applyUndo()
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                Point cell = dgvIndices.CurrentCellAddress;
                if (cell.X == -1) cell = new Point(dgvIndices.SelectedCells[0].ColumnIndex, dgvIndices.SelectedCells[0].RowIndex);
                undoManager.Undo();
                dgvIndices.CurrentCell = dgvIndices[Math.Min(cell.X, dgvIndices.ColumnCount - 1), Math.Min(cell.Y, dgvIndices.RowCount - 1)];
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
            Point cell = dgvIndices.CurrentCellAddress;
            if (cell.X == -1) cell = new Point(dgvIndices.SelectedCells[0].ColumnIndex, dgvIndices.SelectedCells[0].RowIndex);
            return cell;
        }

        void SetCurrentCell(Point cell)
        {
            dgvIndices.CurrentCell = dgvIndices[Math.Min(cell.X, dgvIndices.ColumnCount - 1), Math.Min(cell.Y, dgvIndices.RowCount - 1)];
        }

        void dgvIndices_CellEndEdit(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            if (keepUndoData)
            {
                (dgvIndices.CurrentRow.DataBoundItem as DataRowView).Row.EndEdit();
                storeUndoAction();
            }
        }

        // make HICP-row read-only, i.e. HICP can only be edited in the HICP-global-table
        private void dgvIndices_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                if (!hicpFromGlobalTable || e.RowIndex > 0) return;
                dgvIndices.Rows[0].DefaultCellStyle.BackColor = Color.Snow;
                dgvIndices.Rows[0].DefaultCellStyle.ForeColor = Color.DarkGray;
                dgvIndices.Rows[0].ReadOnly = true;
            }
            catch { }
        }

        private void tabIndices_SelectedIndexChanged(object sender, EventArgs e)
        {
            cmbDatasets_SelectedIndexChanged(null, null);
        }
    }
}
