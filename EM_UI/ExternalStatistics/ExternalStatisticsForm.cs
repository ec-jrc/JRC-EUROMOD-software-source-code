using EM_Common;
using EM_Common_Win;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using EM_Statistics.ExternalStatistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Xml;
using static EM_UI.ExternalStatistics.ExternalStatisticsComponent;

namespace EM_UI.ExternalStatistics
{
    public partial class ExternalStatisticsForm : Form
    {
        internal EM_UI_MainForm _mainForm = null;
        internal CountryConfigFacade _countryConfigFacade = null;


        //Aggregates - Amounts tab
        DataSet dgvDataSetExStatAggAmounts = new DataSet("dgvDataSetExStatAggAmounts");
        DataTable dgvDataTableExStatAggAmounts = new DataTable("dgvDataTableExStatAggAmounts");

        //Aggregates - Beneficiaries/Taxpayers tab
        DataSet dgvDataSetExStatAggBenTax = new DataSet("dgvDataSetExStatAggBenTax");
        DataTable dgvDataTableExStatAggBenTax = new DataTable("dgvDataTableExStatAggBenTax");

        //Aggregates - Level
        DataSet dgvDataSetExStatAggLevel = new DataSet("dgvDataSetExStatAggLevel");
        DataTable dgvDataTableExStatAggLevel = new DataTable("dgvDataTableExStatAggLevel");

        //Distributional - inequality
        DataSet dgvDataSetExStatDistIneq = new DataSet("dgvDataSetExStatDistIneq");
        DataTable dgvDataTableExStatDistIneq = new DataTable("dgvDataTableExStatDistIneq");

        //Distributional - poverty
        DataSet dgvDataSetExStatDistPov = new DataSet("dgvDataSetExStatDistPov");
        DataTable dgvDataTableExStatDistPov = new DataTable("dgvDataTableExStatDistPov");

        bool keepUndoData = false;
        bool scrolling = false;
        bool columnsChanged = false;
        ADOUndoManager undoManager = new ADOUndoManager();
        const string _colYear = "colYear";
        const string _other = "Other";
        public const string AGGREGATES = "Aggregates";
        public const string DISTRIBUTIONAL = "Distributional";
        public const string DISTRIBUTIONAL_INEQUALITY = "Distributional - Inequality";
        public const string DISTRIBUTIONAL_POVERTY = "Distributional - Poverty";
        int colIdIndex = 0;
        int colNameIndex = 0;
        int colDescriptionIndex = 0;
        int colCommentsIndex = 0;
        int colSourceIndex = 0;
        int colIncomeListIndex = 0;
        int colDestinationIndex = 0;
        List<string> years = new List<string>();

        internal ExternalStatisticsForm(EM_UI_MainForm mainForm)
        {
            mainForm.Cursor = Cursors.WaitCursor;
            _mainForm = mainForm;
            _countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(mainForm.GetCountryShortName());
            InitializeComponent();

            //Before loading the tables, we need to obtain the information to populate them
            ExternalStatistic externalStatistics = ExternalStatisticUtil.LoadExternatStatisticsComponentsAndValues(mainForm.GetCountryShortName());
            Dictionary<string, ExternalStatisticAggregate> storedExternalStatisticsAggregates = externalStatistics.Aggregate;
            years = externalStatistics.Years;

            //dgvExStatAggAmountsData.CellLeave += new DataGridViewCellEventHandler(dgvExStatAmountsData_CellLeave);


            //Now we display it in the table Amounts
            dgvDataSetExStatAggAmounts.Tables.Add(dgvDataTableExStatAggAmounts);
            dgvExStatAggAmountsData.DataSource = dgvDataSetExStatAggAmounts;
            dgvExStatAggAmountsData.DataMember = "dgvDataTableExStatAggAmounts";
            dgvExStatAggAmountsData.CellEndEdit += new DataGridViewCellEventHandler(dgv_CellEndEdit);

            //And also in the table Beneficiaries/Taxpayers
            dgvDataSetExStatAggBenTax.Tables.Add(dgvDataTableExStatAggBenTax);
            dgvExStatAggBenTaxData.DataSource = dgvDataSetExStatAggBenTax;
            dgvExStatAggBenTaxData.DataMember = "dgvDataTableExStatAggBenTax";
            dgvExStatAggBenTaxData.AllowUserToAddRows = false;
            dgvExStatAggBenTaxData.CellEndEdit += new DataGridViewCellEventHandler(dgv_CellEndEdit);

            //And also in the table Levels
            dgvDataSetExStatAggLevel.Tables.Add(dgvDataTableExStatAggLevel);
            dgvExStatAggLevelData.DataSource = dgvDataSetExStatAggLevel;
            dgvExStatAggLevelData.DataMember = "dgvDataTableExStatAggLevel";
            dgvExStatAggLevelData.AllowUserToAddRows = false;
            dgvExStatAggLevelData.CellEndEdit += new DataGridViewCellEventHandler(dgv_CellEndEdit);

            //Distributional inequality
            dgvDataSetExStatDistIneq.Tables.Add(dgvDataTableExStatDistIneq);
            dgvExStatDistIneqData.DataSource = dgvDataSetExStatDistIneq;
            dgvExStatDistIneqData.DataMember = "dgvDataTableExStatDistIneq";
            dgvExStatDistIneqData.CellEndEdit += new DataGridViewCellEventHandler(dgv_CellEndEdit);

            //Distributional poverty
            dgvDataSetExStatDistPov.Tables.Add(dgvDataTableExStatDistPov);
            dgvExStatDistPovData.DataSource = dgvDataSetExStatDistPov;
            dgvExStatDistPovData.DataMember = "dgvDataTableExStatDistPov";
            dgvExStatDistPovData.CellEndEdit += new DataGridViewCellEventHandler(dgv_CellEndEdit);

            PopulateTablesWithExternalStatistics(externalStatistics, years);

            // make sure that you  all data is loaded before running this! 
            CustomizeCells(dgvExStatAggAmountsData);
            CustomizeCells(dgvExStatAggBenTaxData);
            CustomizeCells(dgvExStatAggLevelData);
            CustomizeCells(dgvExStatDistIneqData);
            CustomizeCells(dgvExStatDistPovData);

            mainForm.Cursor = Cursors.Default;
        }

        // This function must be run only when the data has already been loaded!
        private void CustomizeCells(DataGridView dgv)
        {
            // fill row headers with numbers 
            dgv.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            dgv.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(dgv_RowPostPaint);
            // handle the copy/paste/undo/redo/delete shortcuts
            dgv.KeyDown += new System.Windows.Forms.KeyEventHandler(dgv_KeyDown);
            // Draw the readonly cells grey and lock them 
            dgv.CellBeginEdit += new DataGridViewCellCancelEventHandler(dgv_CellTryEnter);
            dgv.CellPainting += new DataGridViewCellPaintingEventHandler(dgv_CellPainting);
            // hide the id column
            dgv.Columns[0].Visible = false;
            // block sorting
            foreach (DataGridViewColumn column in dgv.Columns) column.SortMode = DataGridViewColumnSortMode.NotSortable;
            // add right-click functionalities - ONLY for Levels at the moment
            if (dgv == dgvExStatAggLevelData) dgv.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgv_MouseClick);
        }

        private void dgv_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            var rowIdx = (e.RowIndex + 1).ToString();

            var centerFormat = new StringFormat()
            {
                // right alignment might actually make more sense for numbers
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            var headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top, dgv.RowHeadersWidth, e.RowBounds.Height);
            e.Graphics.DrawString(rowIdx, this.Font, SystemBrushes.ControlText, headerBounds, centerFormat);
        }

        // Decides if a cell is readonly or not. 
        private bool IsCellReadOnly(DataGridView dgv, int rowIndex, int colIndex, bool checkNameDesc = true)
        {
            if (rowIndex < 0 || rowIndex >= dgv.RowCount - (dgv.AllowUserToAddRows ? 1 : 0) || colIndex < 0) return false; // not a row or new row
            if (dgv.Rows[rowIndex].Cells[colIndex].ReadOnly) return true; // a readonly cell by definition (e.g. 
            if (colIndex == colIdIndex) return true;  // ID column is always readonly
            if (dgv == dgvExStatAggAmountsData && (colIndex == colNameIndex || colIndex == colDescriptionIndex))    // for the aggregates table only, Name & Description columns might be readonly, but might not
            {
                if (dgv.Rows[rowIndex].Cells[colIncomeListIndex] == null || dgv.Rows[rowIndex].Cells[colIncomeListIndex].Value == null) return false;   // this is a new row
                if (string.IsNullOrEmpty(dgv.Rows[rowIndex].Cells[colIncomeListIndex].Value.ToString()) || dgv.Rows[rowIndex].Cells[colIncomeListIndex].Value.ToString().Equals(_other)) return false;  // this is a new row or "Other" incomelist
                return true;    // this is a standard incomelist, so readonly
            }
            return false;   // this is not a readonly column
        }

        private void dgv_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;
            e.CellStyle.BackColor = IsCellReadOnly(dgv, e.RowIndex, e.ColumnIndex) ? Color.LightGray : Color.White;
        }

        //The name, income list and description cannot be edited
        private void dgv_CellTryEnter(object sender, DataGridViewCellCancelEventArgs e)
        {
            if ((sender as DataGridView) == null) return;
            if (IsCellReadOnly(sender as DataGridView, e.RowIndex, e.ColumnIndex)) e.Cancel = true;
        }

        // every time you edit something on the Aggregates table you need to check if the beneficiaries and Level tables need updating
        private void copyFixedColumns(int rowIndex, int colIndex, bool acceptChanges = true)
        {
            if (rowIndex < 0 || colIdIndex < 0) return;
            string value = dgvExStatAggAmountsData.Rows[rowIndex].Cells[colIndex].Value.ToString();
            DataRow rowBen = (rowIndex < dgvDataTableExStatAggBenTax.Rows.Count) ? dgvDataTableExStatAggBenTax.Rows[rowIndex] : null;
            // if a new row was added, add it to this table too
            if (rowBen == null)
            {
                rowBen = dgvDataTableExStatAggBenTax.Rows.Add();
                dgvExStatAggBenTaxData.Rows[rowIndex].Cells[colBenIncomeList.Name].Value = _other;
            }
            // if a fixed cell was changed, change it in this table too
            if (colIndex == colNameIndex || colIndex == colDescriptionIndex || colIndex == colCommentsIndex || colIndex == colSourceIndex || colIndex == colDestinationIndex)
                rowBen.SetField(colIndex, value);
            if (acceptChanges && dgvDataSetExStatAggBenTax.HasChanges()) dgvDataSetExStatAggBenTax.AcceptChanges();

            DataRow rowLevel = (rowIndex < dgvDataTableExStatAggLevel.Rows.Count) ? dgvDataTableExStatAggLevel.Rows[rowIndex] : null;
            // if a new row was added, add it to this table too
            if (rowLevel == null)
            {
                rowLevel = dgvDataTableExStatAggLevel.Rows.Add();
                dgvExStatAggLevelData.Rows[rowIndex].Cells[colLevelIncomeList.Name].Value = _other;
            }
            // if a fixed cell was changed, change it in this table too
            if (colIndex == colNameIndex || colIndex == colDescriptionIndex || colIndex == colCommentsIndex || colIndex == colSourceIndex || colIndex == colDestinationIndex)
                rowLevel.SetField(colIndex, value);
            if (acceptChanges && dgvDataSetExStatAggLevel.HasChanges()) dgvDataSetExStatAggLevel.AcceptChanges();
        }

        void ClearCells(DataGridView dgv)
        {
            if (dgv.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                foreach (DataGridViewCell cell in dgv.SelectedCells)
                {
                    if (cell.OwningRow.DataBoundItem != null)
                    {
                        if (!IsCellReadOnly(dgv, cell.RowIndex, cell.ColumnIndex))
                        {
                            cell.Value = "";
                            (cell.OwningRow.DataBoundItem as DataRowView).Row.EndEdit();
                        }
                    }
                }
            }
        }

        void CopyToClipboard(DataGridView dgv)
        {
            if (dgv.GetCellCount(DataGridViewElementStates.Selected) > 0)
            {
                try
                {
                    Clipboard.SetDataObject(dgv.GetClipboardContent());
                }
                catch (Exception exception)
                {
                    UserInfoHandler.ShowException(exception);
                }
            }
        }

        List<DataGridViewColumn> GetYearColumnsAsDisplayed(DataGridView dgv)
        {
            SortedList<int, DataGridViewColumn> sortedColumns = new SortedList<int, DataGridViewColumn>();
            foreach (DataGridViewColumn yearColumn in dgv.Columns)
                if (yearColumn.Name.StartsWith(_colYear))
                    sortedColumns.Add(yearColumn.DisplayIndex, yearColumn);
            List<DataGridViewColumn> yearColumnsAsDisplayed = new List<DataGridViewColumn>();
            foreach (DataGridViewColumn yearColumn in sortedColumns.Values)
                yearColumnsAsDisplayed.Add(yearColumn);
            return yearColumnsAsDisplayed;
        }

        int GetYearColumnIndexByDisplayIndex(DataGridView dgv, int displayIndex)
        {
            foreach (DataGridViewColumn column in dgv.Columns)
                if (column.DisplayIndex == displayIndex)
                    return column.Index;
            return -1;
        }

        private void storeUndoAction(DataGridView dgv)
        {
            if (keepUndoData)
            {
                if ((dgv.DataSource as DataSet) == null) return;
                keepUndoData = false;
                if ((dgv.DataSource as DataSet).HasChanges())
                {
                    Point cell = dgv.CurrentCellAddress;
                    if (cell.X == -1) cell = new Point(dgv.SelectedCells[0].ColumnIndex, dgv.SelectedCells[0].RowIndex);
                    undoManager.Commit();
                    dgv.CurrentCell = dgv[Math.Min(cell.X, dgv.ColumnCount - 1), Math.Min(cell.Y, dgv.RowCount - 1)];
                }
                keepUndoData = true;
            }
        }

        private void applyUndo(DataGridView dgv)
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                Point cell = dgv.CurrentCellAddress;
                if (cell.X == -1) cell = new Point(dgv.SelectedCells[0].ColumnIndex, dgv.SelectedCells[0].RowIndex);
                undoManager.Undo();
                dgv.CurrentCell = dgv[Math.Min(cell.X, dgv.ColumnCount - 1), Math.Min(cell.Y, dgv.RowCount - 1)];
                keepUndoData = true;
            }
        }

        private void applyRedo(DataGridView dgv)
        {
            if (keepUndoData)
            {
                keepUndoData = false;
                Point cell = GetCurrentCell(dgv);
                undoManager.Redo();
                SetCurrentCell(dgv, cell);
                keepUndoData = true;
            }
        }

        Point GetCurrentCell(DataGridView dgv)
        {
            Point cell = dgv.CurrentCellAddress;
            if (cell.X == -1) cell = new Point(dgv.SelectedCells[0].ColumnIndex, dgv.SelectedCells[0].RowIndex);
            return cell;
        }

        void SetCurrentCell(DataGridView dgv, Point cell)
        {
            dgv.CurrentCell = dgv[Math.Min(cell.X, dgv.ColumnCount - 1), Math.Min(cell.Y, dgv.RowCount - 1)];
        }

        void GetFromClipboard(DataGridView dgv)
        {
            try
            {
                if ((dgv.DataSource as DataSet) == null || (dgv.DataSource as DataSet).Tables.Count == 0) return;
                DataTable dt = (dgv.DataSource as DataSet).Tables[0];
                string clipboardText = Clipboard.GetText();
                string[] clipboardLines = clipboardText.Split('\n');
                int indexRow = dgv.CurrentCell.RowIndex;
                bool addedExtraLine = (indexRow == dt.Rows.Count);
                int currentCellColumnIndex = dgv.Columns[dgv.CurrentCell.ColumnIndex].DisplayIndex;
                foreach (string line in clipboardLines)
                {
                    if (line.Length > 0)
                    {
                        DataRow row;
                        if (indexRow == dt.Rows.Count)
                        {
                            if (!dgv.AllowUserToAddRows) break;
                            row = dt.NewRow();
                            dt.Rows.Add(row);
                        }
                        else
                            row = dt.Rows[indexRow];
                        List<DataGridViewColumn> cols = GetYearColumnsAsDisplayed(dgv);
                        string[] clipboardCells = line.Split('\t').Select(p => p.Trim()).ToArray(); // trim cells to avoid unwanted new lines and spaces at the end
                        for (int indexColumn = 0; indexColumn < clipboardCells.GetLength(0); ++indexColumn)
                        {
                            if (currentCellColumnIndex + indexColumn < dgv.ColumnCount)
                            { 
                                // if not a readonly cell
                                if (!IsCellReadOnly(dgv, indexRow, currentCellColumnIndex + indexColumn))
                                {
                                    // copy the value
                                    int colIndex = GetYearColumnIndexByDisplayIndex(dgv, currentCellColumnIndex + indexColumn);

                                    // if this is a Year column, that is not in the Level table, replace comma with dot (fix decimal separator issue)
                                    if (dgv != dgvExStatAggLevelData && dgv.Columns[colIndex].Name.StartsWith(_colYear))
                                        row[colIndex] = EM_Helpers.SetCleanInvariantNumberFormat(clipboardCells[indexColumn]);
                                    else    // else copy value as is
                                        row[colIndex] = clipboardCells[indexColumn];

                                    // if this is the aggregate table, check if we need to copy to other tables
                                    if (dgv == dgvExStatAggAmountsData) copyFixedColumns(indexRow, currentCellColumnIndex + indexColumn, false);
                                }
                            }
                            else
                                break;
                        }
                        indexRow++;
                    }
                    else
                        break;
                }
                if (addedExtraLine) dgv.Rows.RemoveAt(dgv.Rows.Count - 2);    // get rid of the extra line added by the grid
                if (dgv == dgvExStatAggAmountsData) // if this is the aggregate table, check if we need to accept changes in the other tables
                {
                    if (dgvDataSetExStatAggBenTax.HasChanges()) dgvDataSetExStatAggBenTax.AcceptChanges();
                    if (dgvDataSetExStatAggLevel.HasChanges()) dgvDataSetExStatAggLevel.AcceptChanges();
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }

        }

        private void dgv_KeyDown(object sender, KeyEventArgs keyEventArgs)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;
            if (keyEventArgs.Control && (keyEventArgs.KeyCode == Keys.C || keyEventArgs.KeyCode == Keys.Insert))
            {
                CopyToClipboard(dgv);
            }
            else if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.V) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Insert))
            {
                keepUndoData = false;
                GetFromClipboard(dgv);
                keepUndoData = true;
                storeUndoAction(dgv);
            }
            else if ((keyEventArgs.Control && keyEventArgs.KeyCode == Keys.X) || (keyEventArgs.Shift && keyEventArgs.KeyCode == Keys.Delete))
            {
                keepUndoData = false;
                CopyToClipboard(dgv);
                ClearCells(dgv);
                keepUndoData = true;
                storeUndoAction(dgv);
            }
            else if (keyEventArgs.KeyCode == Keys.Delete)
            {
                keepUndoData = false;
                ClearCells(dgv);
                keepUndoData = true;
                storeUndoAction(dgv);
            }
            else if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Z)
            {
                applyUndo(dgv);
            }
            else if (keyEventArgs.Control && keyEventArgs.KeyCode == Keys.Y)
            {
                applyRedo(dgv);
            }
        }

        private void deleteSelectedRows(DataGridView dgv)
        {
            string note = (dgv == dgvExStatAggAmountsData || dgv == dgvExStatAggBenTaxData || dgv == dgvExStatAggLevelData) ? "Note: These rows will be removed from all three tables: Amounts, Beneficiaries & Level\n\n" : "";
            if (dgv.SelectedRows.Count < 1 || MessageBox.Show("Are you sure you want to remove the selected row(s)?\n\n" + note + "Note: you will not be able to undo this action or any action before this.", "Remove Rows", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;
            if ((dgv.DataSource as DataSet) == null || (dgv.DataSource as DataSet).Tables.Count == 0) return;
            DataTable dt = (dgv.DataSource as DataSet).Tables[0];

            keepUndoData = false;
            foreach (DataGridViewRow vrow in dgv.SelectedRows)
            {
                if (vrow.IsNewRow) continue;    // ignore delete for the empty new row
                
                if (!string.IsNullOrEmpty(note))    // if it is one of the first three tables, delete in all three, based on id
                {
                    string id = vrow.Cells[0].Value.ToString();
                    dgvExStatAggAmountsData.Rows.Remove(dgvExStatAggAmountsData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString().Equals(id)).First());
                    dgvExStatAggBenTaxData.Rows.Remove(dgvExStatAggBenTaxData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString().Equals(id)).First());
                    dgvExStatAggLevelData.Rows.Remove(dgvExStatAggLevelData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[0].Value.ToString().Equals(id)).First());
                }
                else    // else delete this row
                {
                    dt.Rows.Remove((vrow.DataBoundItem as DataRowView).Row);
                }
            }
            keepUndoData = true;
            if (!string.IsNullOrEmpty(note))    // if it is one of the first three tables, save all three
            {
                dgvDataSetExStatAggAmounts.AcceptChanges();
                dgvDataSetExStatAggBenTax.AcceptChanges();
                dgvDataSetExStatAggLevel.AcceptChanges();
            }
            else
            {
                (dgv.DataSource as DataSet).AcceptChanges();    // else save just this
            }
                
            undoManager.Reset();
            columnsChanged = true;
        }

        private void toolStripMenuItemDelete_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsm = sender as ToolStripMenuItem;
            if (tsm == null) return;
            ContextMenuStrip cms = tsm.GetCurrentParent() as ContextMenuStrip;
            if (cms == null) return;
            DataGridView dgv = cms.SourceControl as DataGridView;
            if (dgv == null) return;
            deleteSelectedRows(dgv);
        }

        private void dgv_MouseClick(object sender, MouseEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;
            if (e.Button == MouseButtons.Right)
            {
                int currentMouseOverRow = dgv.HitTest(e.X, e.Y).RowIndex;
                int currentMouseOverColumn = dgv.HitTest(e.X, e.Y).ColumnIndex;
                if (currentMouseOverRow >= 0)
                {
                    bool keepSelection = false;
                    foreach (DataGridViewRow vrow in dgv.SelectedRows)
                    {
                        if (vrow.Index == currentMouseOverRow) keepSelection = true;
                    }
                    if (!keepSelection)
                    {
                        dgv.ClearSelection();
                        dgv.Rows[currentMouseOverRow].Selected = true;
                    }
                    contextMenuStripRows.Show(dgv, e.X, e.Y);
                }
            }

        }


        void PopulateTablesWithExternalStatistics(ExternalStatistic externalStatistics, List<string> years)
        {
            keepUndoData = false;

            Dictionary<string, ExternalStatisticAggregate> storedExternalStatisticsAggregates = externalStatistics.Aggregate;
            //Aggregates - Amount table
            dgvDataTableExStatAggAmounts.PrimaryKey = new DataColumn[] { dgvDataTableExStatAggAmounts.Columns.Add("ID", typeof(Int16)) };
            dgvDataTableExStatAggAmounts.PrimaryKey[0].AutoIncrement = true;
            DataColumn colIncomeList = new DataColumn(colAmIncomeList.Name) { DefaultValue = _other };
            dgvDataTableExStatAggAmounts.Columns.Add(colIncomeList);
            DataColumn colAm = dgvDataTableExStatAggAmounts.Columns.Add(colAmName.Name);
            DataColumn colDescription = dgvDataTableExStatAggAmounts.Columns.Add(colAmDescription.Name);

            // Aggregates - Number of beneficiaries/taxpayers
            dgvDataTableExStatAggBenTax.PrimaryKey = new DataColumn[] { dgvDataTableExStatAggBenTax.Columns.Add(colBenID.Name, typeof(Int16)) };
            dgvDataTableExStatAggBenTax.PrimaryKey[0].AutoIncrement = true;
            dgvDataTableExStatAggBenTax.Columns.Add(colBenIncomeList.Name);
            dgvDataTableExStatAggBenTax.Columns.Add(colBenName.Name);
            dgvDataTableExStatAggBenTax.Columns.Add(colBenDescription.Name);

            // Aggregates - Level
            dgvDataTableExStatAggLevel.PrimaryKey = new DataColumn[] { dgvDataTableExStatAggLevel.Columns.Add(colLevelID.Name, typeof(Int16)) };
            dgvDataTableExStatAggLevel.PrimaryKey[0].AutoIncrement = true;
            dgvDataTableExStatAggLevel.Columns.Add(colLevelIncomeList.Name);
            dgvDataTableExStatAggLevel.Columns.Add(colLevelName.Name);
            dgvDataTableExStatAggLevel.Columns.Add(colLevelDescription.Name);

            // Distributional - Inequality
            dgvDataTableExStatDistIneq.PrimaryKey = new DataColumn[] { dgvDataTableExStatDistIneq.Columns.Add(colIneqID.Name, typeof(Int16)) };
            dgvDataTableExStatDistIneq.PrimaryKey[0].AutoIncrement = true;
            dgvDataTableExStatDistIneq.Columns.Add(colIneqName.Name);
            dgvDataTableExStatDistIneq.Columns.Add(colIneqDesc.Name);

            // Distributional - Poverty
            dgvDataTableExStatDistPov.PrimaryKey = new DataColumn[] { dgvDataTableExStatDistPov.Columns.Add(colPovID.Name, typeof(Int16)) };
            dgvDataTableExStatDistPov.PrimaryKey[0].AutoIncrement = true;
            dgvDataTableExStatDistPov.Columns.Add(colPovName.Name);
            dgvDataTableExStatDistPov.Columns.Add(colPovDesc.Name);

            //First, we add the columns for the years
            foreach (string year in years.OrderBy(x => x))
            {
                AddYearColumn(year);
            }

            DataColumn colComments = dgvDataTableExStatAggAmounts.Columns.Add(colAmComments.Name);
            dgvDataTableExStatAggBenTax.Columns.Add(colBenComments.Name);
            dgvDataTableExStatAggLevel.Columns.Add(colLevelComments.Name);
            dgvDataTableExStatDistPov.Columns.Add(colPovComments.Name);
            dgvDataTableExStatDistIneq.Columns.Add(colIneqComments.Name);

            DataColumn colSource = dgvDataTableExStatAggAmounts.Columns.Add(colAmSource.Name);
            dgvDataTableExStatAggBenTax.Columns.Add(colBenSource.Name);
            dgvDataTableExStatAggLevel.Columns.Add(colLevelSource.Name);
            dgvDataTableExStatDistPov.Columns.Add(colPovSource.Name);
            dgvDataTableExStatDistIneq.Columns.Add(colIneqSource.Name);

            DataColumn colDestination = dgvDataTableExStatAggAmounts.Columns.Add(colAmDestination.Name);
            colDestination.SetOrdinal(colAmDestination.DisplayIndex);
            int di = colAmDestination.DisplayIndex;
            //dgvDataTableExStatAggAmounts.Columns.Remove(colAmDestination.Name);
            dgvExStatAggAmountsData.Columns.Remove(colAmDestination.Name);
            DataGridViewComboBoxColumn cmbColDestination = new DataGridViewComboBoxColumn();
            cmbColDestination.DataPropertyName = colAmDestination.Name;
            cmbColDestination.HeaderText = "Destination";
            cmbColDestination.Name = colDestination.ColumnName;
            cmbColDestination.Width = 125;
            cmbColDestination.FlatStyle = FlatStyle.Flat;
            cmbColDestination.Items.AddRange(InDepthDefinitions.DestinationTables.ToArray());

            cmbColDestination.DisplayIndex = di;
            dgvExStatAggAmountsData.Columns.Add(cmbColDestination);

            dgvDataTableExStatAggBenTax.Columns.Add(colBenDestination.Name);
            dgvDataTableExStatAggLevel.Columns.Add(colLevelDestination.Name);

            dgvExStatAggAmountsData.EditingControlShowing += DgvExStatAggAmountsData_EditingControlShowing;

            //AGGREGATES
            //We sort the dictionarty by incomelist and variable name
            Dictionary<string, ExternalStatisticAggregate> sortedStoredExternalStatisticsAggregates = new Dictionary<string, ExternalStatisticAggregate>(storedExternalStatisticsAggregates);

            foreach (KeyValuePair<string, ExternalStatisticAggregate> var in sortedStoredExternalStatisticsAggregates) // add one row for each index
            {
                string dest = string.IsNullOrEmpty(var.Value.Destination) ?
                        (InDepthDefinitions.IncomelistDefaultDestination.ContainsKey(var.Value.IncomeList) ? InDepthDefinitions.IncomelistDefaultDestination[var.Value.IncomeList] : InDepthDefinitions.DESTINATION_NONE) :
                        var.Value.Destination;

                //Aggregates - Amounts
                DataRow rowAm = dgvDataTableExStatAggAmounts.Rows.Add();
                rowAm.SetField(colAmIncomeList.Name, var.Value.IncomeList);
                rowAm.SetField(colAmName.Name, var.Value.Name);
                rowAm.SetField(colAmDescription.Name, var.Value.Description);
                rowAm.SetField(colAmComments.Name, var.Value.Comment);
                rowAm.SetField(colAmSource.Name, var.Value.Source);
                rowAm.SetField(colAmDestination.Name, dest);

                //Aggregates - Beneficiaries / Taxpayers
                DataRow rowBen = dgvDataTableExStatAggBenTax.Rows.Add();
                rowBen.SetField(colBenIncomeList.Name, var.Value.IncomeList);
                rowBen.SetField(colBenName.Name, var.Value.Name);
                rowBen.SetField(colBenDescription.Name, var.Value.Description);
                rowBen.SetField(colBenComments.Name, var.Value.Comment);
                rowBen.SetField(colBenSource.Name, var.Value.Source);
                rowBen.SetField(colBenDestination.Name, dest);

                //Aggregates - Level
                DataRow rowLevel = dgvDataTableExStatAggLevel.Rows.Add();
                rowLevel.SetField(colLevelIncomeList.Name, var.Value.IncomeList);
                rowLevel.SetField(colLevelName.Name, var.Value.Name);
                rowLevel.SetField(colLevelDescription.Name, var.Value.Description);
                rowLevel.SetField(colLevelComments.Name, var.Value.Comment);
                rowLevel.SetField(colLevelSource.Name, var.Value.Source);
                rowLevel.SetField(colLevelDestination.Name, dest);


                //Tables are populated with the values
                Dictionary<string, ExternalStatisticAggregateValues> yearValues = var.Value.YearValues;
                if (yearValues != null)
                {
                    foreach (KeyValuePair<string, ExternalStatisticAggregateValues> yearValue in yearValues)
                    {
                        string year = yearValue.Key;
                        string amount = yearValue.Value.Amount;
                        rowAm.SetField(_colYear + year, amount);

                        string ben = yearValue.Value.Beneficiares;
                        rowBen.SetField(_colYear + year, ben);

                        string level = yearValue.Value.Level;
                        rowLevel.SetField(_colYear + year, string.IsNullOrEmpty(level) ? "Individual" : level);
                    }
                }
            }

            //DISTRIBUTIONAL
            Dictionary<string, ExternalStatisticDistributional> externalStatDistr = externalStatistics.Distributional;
            foreach (ExternalStatisticDistributional var in externalStatDistr.Values) // add one row for each index
            {
                DataRow row = null;
                if (var.TableName.Equals(DISTRIBUTIONAL_INEQUALITY))
                {
                    row = dgvDataTableExStatDistIneq.Rows.Add();
                    row.SetField(colIneqName.Name, var.Name);
                    row.SetField(colIneqDesc.Name, var.Description);
                    row.SetField(colIneqComments.Name, var.Comment);
                    row.SetField(colIneqSource.Name, var.Source);
                }
                else if (var.TableName.Equals(DISTRIBUTIONAL_POVERTY))
                {
                    row = dgvDataTableExStatDistPov.Rows.Add();
                    row.SetField(colPovName.Name, var.Name);
                    row.SetField(colPovDesc.Name, var.Description);
                    row.SetField(colPovComments.Name, var.Comment);
                    row.SetField(colPovSource.Name, var.Source);
                }
                else continue;

                //Tables are populated with the values
                Dictionary<string, string> yearValues = var.YearValues;
                foreach (KeyValuePair<string, string> yearValue in yearValues)
                {
                    string year = yearValue.Key;
                    string amount = yearValue.Value;
                    row.SetField(_colYear + year, amount);
                }
            }
            /**/

            dgvDataSetExStatAggAmounts.AcceptChanges();
            dgvDataSetExStatAggBenTax.AcceptChanges();
            dgvDataSetExStatAggLevel.AcceptChanges();
            dgvDataSetExStatDistIneq.AcceptChanges();
            dgvDataSetExStatDistPov.AcceptChanges();
            undoManager.AddDataSet(dgvDataSetExStatAggAmounts);
            undoManager.AddDataSet(dgvDataSetExStatAggBenTax);
            undoManager.AddDataSet(dgvDataSetExStatAggLevel);
            undoManager.AddDataSet(dgvDataSetExStatDistIneq);
            undoManager.AddDataSet(dgvDataSetExStatDistPov);

            colNameIndex = colAm.Ordinal;
            colDescriptionIndex = colDescription.Ordinal;
            colCommentsIndex = colComments.Ordinal;
            colSourceIndex = colSource.Ordinal;
            colIncomeListIndex = colIncomeList.Ordinal;
            colDestinationIndex = di;

            keepUndoData = true;
        }


        /**
         * This function controls which options are displayed in the Destination combobox for each ils/var
         */
        private void DgvExStatAggAmountsData_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            if (e.Control is DataGridViewComboBoxEditingControl)
            {
                DataGridView dgv = sender as DataGridView;
                string il = dgv.Rows[dgv.CurrentCell.RowIndex].Cells[colAmIncomeList.Name].Value.ToString();
                // variables and ils NOT in ils_extstat_other, should have the default destination or "none"
                if (!il.Equals(InDepthDefinitions.ILS_EXTSTAT_OTHER))
                {
                    DataGridViewComboBoxEditingControl cmb = e.Control as DataGridViewComboBoxEditingControl;
                    cmb.Items.Clear();
                    cmb.Items.Add(InDepthDefinitions.IncomelistDefaultDestination[il]);
                    cmb.Items.Add(InDepthDefinitions.DESTINATION_NONE);
                }
                // the ils_extstat_other itself, should only have "none"
                else if (string.IsNullOrEmpty(dgv.Rows[dgv.CurrentCell.RowIndex].Cells[colAmName.Name].Value.ToString()))
                {
                    DataGridViewComboBoxEditingControl cmb = e.Control as DataGridViewComboBoxEditingControl;
                    cmb.Items.Clear();
                    cmb.Items.Add(InDepthDefinitions.DESTINATION_NONE);
                }
            }
        }

        void AddYearColumn(string caption, bool manual = false)
        {
            // Starting position for years is hard-coded for each table, so we have them all here for transparency
            const int StartPosAggBenLev = 4;
            const int StartPosIneq = 3;
            const int StartPosPov = 3;

            //Aggregates - Amounts
            DataColumn colAm = dgvDataTableExStatAggAmounts.Columns.Add(_colYear + caption);
            //add before the comments-column

            List<string> allyears = GetExistingYears();
            allyears.Add(caption);
            int yearIndex = allyears.OrderBy(x => x).ToList().IndexOf(caption);
            
            colAm.SetOrdinal(StartPosAggBenLev + yearIndex);
            DataGridViewColumn headerColumn = dgvExStatAggAmountsData.Columns[_colYear + caption];
            headerColumn.FillWeight = 80F;
            headerColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            headerColumn.HeaderText = caption;
            headerColumn.Name = _colYear + caption;
            headerColumn.DataPropertyName = headerColumn.Name;
            headerColumn.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumn.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumn.DisplayIndex = colAm.Ordinal;

            //Aggregates - Tax Beneficiaries - Taxpayers
            DataColumn colBen = dgvDataTableExStatAggBenTax.Columns.Add(_colYear + caption);
            //add before the comments-column
            colBen.SetOrdinal(StartPosAggBenLev + yearIndex);
            DataGridViewColumn headerColumnBen = dgvExStatAggBenTaxData.Columns[_colYear + caption];
            headerColumnBen.FillWeight = 80F;
            headerColumnBen.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            headerColumnBen.HeaderText = caption;
            headerColumnBen.Name = _colYear + caption;
            headerColumnBen.DataPropertyName = headerColumn.Name;
            headerColumnBen.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumnBen.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumnBen.DisplayIndex = colBen.Ordinal;

            //Aggregates - Level
            DataColumn colLevel = dgvDataTableExStatAggLevel.Columns.Add(_colYear + caption);
            colLevel.SetOrdinal(StartPosAggBenLev + yearIndex);
            dgvExStatAggLevelData.Columns.Remove(_colYear + caption);
            DataGridViewComboBoxColumn headerColumnLevel = new DataGridViewComboBoxColumn();
            headerColumnLevel.DataPropertyName = _colYear + caption;
            headerColumnLevel.HeaderText = caption;
            headerColumnLevel.Name = _colYear + caption;
            headerColumnLevel.Width = 125;
            headerColumnLevel.FlatStyle = FlatStyle.Flat;
            List<string> colLevelOptions = new List<string> { "Individual", "Household" };
            headerColumnLevel.DataSource = colLevelOptions;
            headerColumnLevel.DisplayIndex = colLevel.Ordinal;
            dgvExStatAggLevelData.Columns.Add(headerColumnLevel);
            dgvExStatAggLevelData.DataError += new DataGridViewDataErrorEventHandler(DgvExStatAggLevelData_DataError);


            //Distributional - Inequality
            DataColumn colIneq = dgvDataTableExStatDistIneq.Columns.Add(_colYear + caption);
            //add before the comments-column
            colIneq.SetOrdinal(StartPosIneq + yearIndex);
            DataGridViewColumn headerColumnIneq = dgvExStatDistIneqData.Columns[_colYear + caption];
            headerColumnIneq.FillWeight = 80F;
            headerColumnIneq.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            headerColumnIneq.HeaderText = caption;
            headerColumnIneq.Name = _colYear + caption;
            headerColumnIneq.DataPropertyName = headerColumn.Name;
            headerColumnIneq.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumnIneq.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumnIneq.DisplayIndex = colIneq.Ordinal;


            //Distributional - Poverty
            DataColumn colPov = dgvDataTableExStatDistPov.Columns.Add(_colYear + caption);
            //add before the comments-column
            colPov.SetOrdinal(StartPosPov + yearIndex);
            DataGridViewColumn headerColumnPov = dgvExStatDistPovData.Columns[_colYear + caption];
            headerColumnPov.FillWeight = 80F;
            headerColumnPov.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            headerColumnPov.HeaderText = caption;
            headerColumnPov.Name = _colYear + caption;
            headerColumnPov.DataPropertyName = headerColumn.Name;
            headerColumnPov.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumnPov.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
            headerColumnPov.DisplayIndex = colPov.Ordinal;

            foreach (DataRow row in dgvDataTableExStatAggAmounts.Rows) row[headerColumn.Name] = ""; // this is to make sure the added column counts as a DataSet change...
            foreach (DataRow row in dgvDataTableExStatAggBenTax.Rows) row[headerColumnBen.Name] = "";
            foreach (DataRow row in dgvDataTableExStatDistIneq.Rows) row[headerColumnIneq.Name] = "";
            foreach (DataRow row in dgvDataTableExStatDistPov.Rows) row[headerColumnPov.Name] = "";

            //also add year to the combo-box that allows for selecting a year to delete
            cmbYearToDelete.Items.Insert(yearIndex, caption);

            int nextYear = EM_Helpers.SaveConvertToInt(caption) + 1;
            if (updwnYearToAdd.Value < nextYear)
                updwnYearToAdd.Value = nextYear;
            dgvExStatAggAmountsData.Refresh();
            dgvExStatAggBenTaxData.Refresh();
            dgvExStatAggLevelData.Refresh();
            dgvExStatDistIneqData.Refresh();
            dgvExStatDistPovData.Refresh();

            //The index of the column for comments may have changed, it needs to be updated
            colCommentsIndex = dgvDataTableExStatAggAmounts.Columns.IndexOf(colAmComments.Name);
            colSourceIndex = dgvDataTableExStatAggAmounts.Columns.IndexOf(colAmSource.Name);
            colDestinationIndex = dgvDataTableExStatAggAmounts.Columns.IndexOf(colAmDestination.Name);
            //updateColumnsColor(true, previousColCommentsIndex);
        }

        // cancels out errors for invalid values that would otherwise crash the Level table when pasting wrong values
        private void DgvExStatAggLevelData_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            _mainForm.PerformAction(new ExternalStatisticsAction(this), false);
            Close();
        }

        private void btnAddYear_Click(object sender, EventArgs e)
        {
            string year = updwnYearToAdd.Value.ToString();
            if (GetExistingYears().Contains(year))
            {
                UserInfoHandler.ShowError(year + " already exits.");
                return;
            }
            if (MessageBox.Show("Are you sure you want to add a new column?\n\nNote: you will not be able to undo this action or any action before this.", "Add Year Column", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;
            AddYearColumn(year);
            dgvDataSetExStatAggAmounts.AcceptChanges();
            undoManager.Reset();
            columnsChanged = true;
            MessageBox.Show("Please save & close, and reopen the dialogue to see the updated incomelists.");
        }

        internal List<string> GetExistingYears()
        {
            List<string> existingYears = new List<string>();
            foreach (string year in cmbYearToDelete.Items)
                existingYears.Add(year);
            return existingYears;
        }

        private void btnDeleteYear_Click(object sender, EventArgs e)
        {
            if (cmbYearToDelete.Text == string.Empty)
            {
                UserInfoHandler.ShowError("Please select a year.");
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this column?\n\nNote: you will not be able to undo this action or any action before this.", "Delete Year Column", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No) return;

            dgvDataTableExStatAggAmounts.Columns.Remove(_colYear + cmbYearToDelete.Text);
            dgvDataTableExStatAggBenTax.Columns.Remove(_colYear + cmbYearToDelete.Text);
            dgvDataTableExStatAggLevel.Columns.Remove(_colYear + cmbYearToDelete.Text);
            dgvDataTableExStatDistIneq.Columns.Remove(_colYear + cmbYearToDelete.Text);
            dgvDataTableExStatDistPov.Columns.Remove(_colYear + cmbYearToDelete.Text);

            cmbYearToDelete.Items.RemoveAt(cmbYearToDelete.SelectedIndex);

            dgvDataSetExStatAggAmounts.AcceptChanges();
            dgvDataSetExStatAggBenTax.AcceptChanges();
            dgvDataTableExStatAggLevel.AcceptChanges();
            dgvDataTableExStatDistIneq.AcceptChanges();
            dgvDataTableExStatDistPov.AcceptChanges();
            dgvExStatAggAmountsData.Refresh();
            dgvExStatAggBenTaxData.Refresh();
            dgvExStatAggLevelData.Refresh();
            dgvExStatDistIneqData.Refresh();
            dgvExStatDistPovData.Refresh();
            undoManager.Reset();

            //The index of the column for comments may have changed, it needs to be updated
            colCommentsIndex = dgvDataTableExStatAggAmounts.Columns.IndexOf(colAmComments.Name);
            colSourceIndex = dgvDataTableExStatAggAmounts.Columns.IndexOf(colAmSource.Name);
            colDestinationIndex = dgvDataTableExStatAggAmounts.Columns.IndexOf(colAmDestination.Name);

            columnsChanged = true;
            MessageBox.Show("Please save & close, and reopen the dialogue to see the updated incomelists.");
        }


        public List<ExternalStatsTuple> GetExternalStatisticsInfo() { 

            //IncomeList (Category), Referece (name), Comment, Source, YearValues, TableName
            List<ExternalStatsTuple> externalStatisticsTupleList = new List<ExternalStatsTuple>();

            List<string> existingYears = GetExistingYears();

            //Aggregates 
            foreach (DataGridViewRow extStatisticRow in dgvExStatAggAmountsData.Rows)
            {
                if ((extStatisticRow.Cells[colAmName.Name].Value == null || extStatisticRow.Cells[colAmName.Name].Value.ToString() == string.Empty)
                   && (extStatisticRow.Cells[colAmIncomeList.Name].Value == null || extStatisticRow.Cells[colAmIncomeList.Name].Value.ToString() == string.Empty))
                        continue; //probably an empty row
                
                string incomeList = extStatisticRow.Cells[colAmIncomeList.Name].Value == null ? string.Empty : extStatisticRow.Cells[colAmIncomeList.Name].Value.ToString();
                string name = extStatisticRow.Cells[colAmName.Name].Value == null ? string.Empty : extStatisticRow.Cells[colAmName.Name].Value.ToString();
                string description = extStatisticRow.Cells[colAmDescription.Name].Value == null ? string.Empty : extStatisticRow.Cells[colAmDescription.Name].Value.ToString();
                string comment = extStatisticRow.Cells[colAmComments.Name].Value == null ? string.Empty : extStatisticRow.Cells[colAmComments.Name].Value.ToString();
                string source = extStatisticRow.Cells[colAmSource.Name].Value == null ? string.Empty : extStatisticRow.Cells[colAmSource.Name].Value.ToString();
                string destination = extStatisticRow.Cells[colAmDestination.Name].Value == null ? string.Empty : extStatisticRow.Cells[colAmDestination.Name].Value.ToString();
                string yearValues = string.Empty;

                DataRow rowBen = dgvDataTableExStatAggBenTax.Rows.Find(extStatisticRow.Index);
                DataRow rowLevel = dgvDataTableExStatAggLevel.Rows.Find(extStatisticRow.Index);

                foreach (string year in existingYears)
                {
                    string amounts = extStatisticRow.Cells[_colYear + year].Value == null ? string.Empty : extStatisticRow.Cells[_colYear + year].Value.ToString();
                    string beneficiaries = rowBen.Field<string>(_colYear + year) == null ? string.Empty : rowBen.Field<string>(_colYear + year);
                    string level = rowLevel.Field<string>(_colYear + year) == null ? string.Empty : rowLevel.Field<string>(_colYear + year);

                    if (!String.IsNullOrEmpty(yearValues)) yearValues += InDepthDefinitions.SEPARATOR;

                    yearValues += year + InDepthDefinitions.SEPARATOR_INNER + amounts + InDepthDefinitions.SEPARATOR_INNER + beneficiaries + InDepthDefinitions.SEPARATOR_INNER + level;
                }

                ExternalStatsTuple singleElement = new ExternalStatsTuple(incomeList, name, description, yearValues, comment, source, AGGREGATES, destination);
                externalStatisticsTupleList.Add(singleElement);
            }

            //Distributional - Inequality
            foreach (DataGridViewRow extStatisticRow in dgvExStatDistIneqData.Rows)
            {
                if (extStatisticRow.Cells[colIneqName.Name].Value == null || extStatisticRow.Cells[colIneqName.Name].Value.ToString() == string.Empty)
                    continue; //probably an empty row


                string category = String.Empty;
                string name = extStatisticRow.Cells[colIneqName.Name].Value == null ? string.Empty : extStatisticRow.Cells[colIneqName.Name].Value.ToString();
                string description = extStatisticRow.Cells[colIneqDesc.Name].Value == null ? string.Empty : extStatisticRow.Cells[colIneqDesc.Name].Value.ToString();
                string comment = extStatisticRow.Cells[colIneqComments.Name].Value == null ? string.Empty : extStatisticRow.Cells[colIneqComments.Name].Value.ToString();
                string source = extStatisticRow.Cells[colIneqSource.Name].Value == null ? string.Empty : extStatisticRow.Cells[colIneqSource.Name].Value.ToString();
                string yearValues = string.Empty;

                foreach (string year in existingYears)
                {
                    string value = extStatisticRow.Cells[_colYear + year].Value == null ? string.Empty : extStatisticRow.Cells[_colYear + year].Value.ToString();

                    if (!String.IsNullOrEmpty(yearValues)) yearValues += InDepthDefinitions.SEPARATOR;

                    yearValues += year + InDepthDefinitions.SEPARATOR_INNER + value;
                }

                ExternalStatsTuple singleElement = new ExternalStatsTuple(category, name, description, yearValues, comment, source, DISTRIBUTIONAL_INEQUALITY, InDepthDefinitions.DESTINATION_INEQUALITY);
                externalStatisticsTupleList.Add(singleElement);

            }

            //Distributional - Poverty
            foreach (DataGridViewRow extStatisticRow in dgvExStatDistPovData.Rows)
            {
                if (extStatisticRow.Cells[colPovName.Name].Value == null || extStatisticRow.Cells[colPovName.Name].Value.ToString() == string.Empty)
                    continue; //probably an empty row


                string category = string.Empty;
                string name = extStatisticRow.Cells[colPovName.Name].Value == null ? string.Empty : extStatisticRow.Cells[colPovName.Name].Value.ToString();
                string description = extStatisticRow.Cells[colPovDesc.Name].Value == null ? string.Empty : extStatisticRow.Cells[colPovDesc.Name].Value.ToString();
                string comment = extStatisticRow.Cells[colPovComments.Name].Value == null ? string.Empty : extStatisticRow.Cells[colPovComments.Name].Value.ToString();
                string source = extStatisticRow.Cells[colPovSource.Name].Value == null ? string.Empty : extStatisticRow.Cells[colPovSource.Name].Value.ToString();
                string yearValues = string.Empty;

                foreach (string year in existingYears)
                {
                    string value = extStatisticRow.Cells[_colYear + year].Value == null ? string.Empty : extStatisticRow.Cells[_colYear + year].Value.ToString();

                    if (!String.IsNullOrEmpty(yearValues)) yearValues += InDepthDefinitions.SEPARATOR;

                    yearValues += year + InDepthDefinitions.SEPARATOR_INNER + value;
                }

                ExternalStatsTuple singleElement = new ExternalStatsTuple(category, name, description, yearValues, comment, source, DISTRIBUTIONAL_INEQUALITY, InDepthDefinitions.DESTINATION_POVERTY);
                externalStatisticsTupleList.Add(singleElement);
            }

            return externalStatisticsTupleList;
        }

        // This function synchronises the vertical scroll position in the three first tables, by aligning the first visible row)
        private void dgv_Scroll(object sender, ScrollEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (scrolling || dgv == null) return;
            scrolling = true;
            dgvExStatAggAmountsData.FirstDisplayedScrollingRowIndex = dgv.FirstDisplayedScrollingRowIndex;
            dgvExStatAggBenTaxData.FirstDisplayedScrollingRowIndex = dgv.FirstDisplayedScrollingRowIndex;
            dgvExStatAggLevelData.FirstDisplayedScrollingRowIndex = dgv.FirstDisplayedScrollingRowIndex;
            scrolling = false;
        }

        private void tabExternalStatistics_Selected(object sender, TabControlEventArgs e)
        {
            // microsoft bug: if the DataGridView was not yet rendered and you changed the dataset (because you changed the Aggregate table), you need to hide the ID column again... 
            dgvExStatAggAmountsData.Columns[0].Visible = false;
            dgvExStatAggBenTaxData.Columns[0].Visible = false;
            dgvExStatAggLevelData.Columns[0].Visible = false;
        }

        private void toolStripMenuItemSetAllToIndividual_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsm = sender as ToolStripMenuItem;
            if (tsm == null) return;
            ContextMenuStrip cms = tsm.GetCurrentParent() as ContextMenuStrip;
            if (cms == null) return;
            DataGridView dgv = cms.SourceControl as DataGridView;
            if (dgv == null) return;
            if (dgv == dgvExStatAggLevelData)
                setLevelRows(dgv, true);
        }

        private void toolStripMenuItemSetAllToHousehold_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem tsm = sender as ToolStripMenuItem;
            if (tsm == null) return;
            ContextMenuStrip cms = tsm.GetCurrentParent() as ContextMenuStrip;
            if (cms == null) return;
            DataGridView dgv = cms.SourceControl as DataGridView;
            if (dgv == null) return;
            if (dgv == dgvExStatAggLevelData)
                setLevelRows(dgv, false);
        }

        private void setLevelRows(DataGridView dgv, bool ind)
        {
            if ((dgv.DataSource as DataSet) == null || (dgv.DataSource as DataSet).Tables.Count == 0) return;
            DataTable dt = (dgv.DataSource as DataSet).Tables[0];

            keepUndoData = false;
            foreach (DataGridViewRow vrow in dgv.SelectedRows)
            {
                if (vrow.IsNewRow) continue;    // ignore delete for the empty new row

                for (int i = 4; i < vrow.Cells.Count - 2; i++)
                    vrow.Cells[i].Value = ind ? "Individual" : "Household";
            }
            keepUndoData = true;
            storeUndoAction(dgv);
        }

        private void contextMenuStripRows_Opening(object sender, CancelEventArgs e)
        {
            ContextMenuStrip cms = sender as ContextMenuStrip;
            if (cms == null) { e.Cancel = true; return; }
            DataGridView dgv = cms.SourceControl as DataGridView;
            if (dgv == null) { e.Cancel = true; return; }
            if (dgv != dgvExStatAggAmountsData && dgv != dgvExStatAggLevelData) { e.Cancel = true; return; }
            toolStripMenuItemDelete.Visible = false; // dgv == dgvExStatAggAmountsData;     Do not show at all the Delete Row context menu for now, as it seems to be buggy...
            toolStripMenuItemSetAllToHousehold.Visible = dgv == dgvExStatAggLevelData;
            toolStripMenuItemSetAllToIndividual.Visible = dgv == dgvExStatAggLevelData;
        }

        private void DgvExStatAggAmountsData_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;
            if (dgv == dgvExStatAggAmountsData) copyFixedColumns(e.RowIndex, e.ColumnIndex);
            storeUndoAction(dgv);
        }

        private void dgv_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (dgv == null) return;
            if (dgv == dgvExStatAggAmountsData) copyFixedColumns(e.RowIndex, e.ColumnIndex);
            (dgv.CurrentCell.OwningRow.DataBoundItem as DataRowView).Row.EndEdit(); // make sure the edit is committed to the DataSet 
            storeUndoAction(dgv);
        }

        private void ExternalStatisticsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.Cancel)
                if (dgvDataSetExStatAggAmounts.HasChanges() || dgvDataSetExStatAggBenTax.HasChanges() || dgvDataSetExStatAggLevel.HasChanges() || dgvDataSetExStatDistIneq.HasChanges() || dgvDataSetExStatDistPov.HasChanges() || undoManager.HasChanges() || columnsChanged)
                    if (UserInfoHandler.GetInfo("Are you sure you want to close without saving?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                        e.Cancel = true;
        }
    }


}
