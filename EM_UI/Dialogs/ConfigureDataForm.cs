using EM_Common;
using EM_Common_Win;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ConfigureDataForm : Form
    {
        internal enum ChangeStates { unchanged, changed, added, removed };
        static Color _privateForeColor = Color.Red;
        static Color _privateBackColor = Color.LightBlue;
        Point _mousePositionWhenMenuOpened;
        DataGridView _gridWhereMenuOpened;

        internal class RowTag
        {
            internal string ID = string.Empty;
            internal string Name = string.Empty;
            internal string Comment = string.Empty;
            internal string YearCollection = string.Empty;
            internal string YearInc = string.Empty;
            internal string Currency = DefPar.Value.EURO;
            internal string FilePath = string.Empty;
            internal string DecimalSign = ".";
            internal string Private = DefPar.Value.NO;
            internal string UseCommonDefault = DefPar.Value.NO;
            internal bool IsCommonDefaultNull = false;
            internal string ReadXVariables = DefPar.Value.NO;
            internal string ListStringOutVar = string.Empty;
            internal string IndirectTaxTableYear = string.Empty;
            internal ChangeStates ChangeState = ChangeStates.unchanged;
            internal Dictionary<string, CellTag> CellTags = new Dictionary<string,CellTag>();
        }

        internal class CellTag
        {
            internal string SystemID = string.Empty;
            internal string UseDefault = string.Empty;
            internal string UseCommonDefault = DefPar.Value.NO;
            internal string Uprate = string.Empty;
            internal string BestMatch = DefPar.Value.NO;
            internal ChangeStates ChangeState = ChangeStates.unchanged;
        }

        internal List<RowTag> _dataBaseInfo = new List<RowTag>();
        internal Dictionary<string, string> _systemInfo = new Dictionary<string, string>();

        string _countryShortName = null;
        RowTag _displayedDataRow = null;

        static string _comboEntryBestMatch = DefPar.Value.DATA_SYS_BEST;
        static string _comboEntryNA = DefPar.Value.NA;
        static string _comboEntryApplicable = DefPar.Value.DATA_SYS_X;

        void ConfigureDataForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            dgvSystemDataCombinations.EnableHeadersVisualStyles = false; // without this changing header-colours (for private systems and databases) would have no effect
            dgvHHOT.EnableHeadersVisualStyles = false;
            UpdateContentTable();

            labIndirectTaxTableYear.Visible = txtIndirectTaxTableYear.Visible = EnvironmentInfo.ShowComponent(EnvironmentInfo.Display.IndirectTaxes_debug);
        }

        void UpdateContentTable(string dataToSelect = "")
        {
            dgvSystemDataCombinations.Rows.Clear();
            dgvSystemDataCombinations.Columns.Clear();
            dgvHHOT.Rows.Clear();
            dgvHHOT.Columns.Clear();

            DataGridViewRow rowToSelect = null;

            //add systems to system-dataset-combination table
            foreach (string systemID in _systemInfo.Keys)
            {
                AddSystemToDGV(dgvSystemDataCombinations, systemID);
                AddSystemToDGV(dgvHHOT, systemID);
            }
        
            //add datasets and whether they are applicable on systems to system-dataset-combination table
            List<string> dbNamesSorted = (from di in _dataBaseInfo select di.Name + di.ID).ToList(); //add id to be sure to avoid missing any dataset, because of duplicate name
            dbNamesSorted.Sort();
            foreach (string dbNameSorted in dbNamesSorted)
            {
                RowTag dataBaseInfo = (from di in _dataBaseInfo where di.Name + di.ID == dbNameSorted select di).First();
                if (dataBaseInfo.ChangeState == ChangeStates.removed)
                    continue;

                DataGridView dgv = dataBaseInfo.Name.ToLower().Contains("hhot") ? dgvHHOT : dgvSystemDataCombinations;
                int rowIndex = dgv.Rows.Add();
                DataGridViewRow dgvRow = dgv.Rows[rowIndex];
                dgvRow.Tag = dataBaseInfo;
                dgvRow.HeaderCell.Value = dataBaseInfo.Name;
                if (dataBaseInfo.ID == dataToSelect)
                    rowToSelect = dgvRow;
                if (dataBaseInfo.Private == DefPar.Value.YES)
                {
                    dgvRow.HeaderCell.Style.ForeColor = _privateForeColor;
                    dgvRow.HeaderCell.Style.SelectionForeColor = _privateForeColor;
                    dgvRow.HeaderCell.Style.SelectionBackColor = _privateBackColor; // just to make the red fore colour more visible
                }

                for (int columnIndex = 0; columnIndex < dgv.Columns.Count; ++columnIndex)
                {
                    dgvRow.Cells[columnIndex].Value = _comboEntryNA;
                    string systemID = dgv.Columns[columnIndex].Tag.ToString();
                    if (dataBaseInfo.CellTags.Keys.Contains(systemID))
                    {
                        CellTag cellTag = dataBaseInfo.CellTags[systemID];
                        if (cellTag.BestMatch == DefPar.Value.YES)
                            dgvRow.Cells[columnIndex].Value = _comboEntryBestMatch;
                        else
                            dgvRow.Cells[columnIndex].Value = _comboEntryApplicable;
                        dgvRow.Cells[columnIndex].Tag = cellTag;
                    }
                }
            }

            //display attributes of first or newly added dataset
            if (dataToSelect == string.Empty && GetSelectedDGV().Rows.Count > 0) rowToSelect = GetSelectedDGV().Rows[0];
            if (rowToSelect != null)
            {
                tabControl.SelectedTab = rowToSelect.DataGridView.Name == dgvHHOT.Name ? tabHHOT : tabGeneral;
                rowToSelect.Selected = true;
            }
        }

        private DataGridView GetSelectedDGV()
        {
            return tabControl.SelectedTab.Name == tabHHOT.Name ? dgvHHOT : dgvSystemDataCombinations;
        }

        private void AddSystemToDGV(DataGridView dgv, string systemID)
        {
            DataGridViewComboBoxColumn cmbApplicable = new DataGridViewComboBoxColumn();
            cmbApplicable.HeaderText = _systemInfo[systemID];
            cmbApplicable.Items.AddRange(_comboEntryApplicable, _comboEntryNA, _comboEntryBestMatch);

            int columnIndex = dgv.Columns.Add(cmbApplicable);
            dgv.Columns[columnIndex].Tag = systemID;
            dgv.Columns[columnIndex].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            if (CountryAdministrator.GetCountryConfigFacade(_countryShortName).GetSystemRowByID(systemID).Private == DefPar.Value.YES)
                dgv.Columns[columnIndex].HeaderCell.Style.ForeColor = _privateForeColor;
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        void btnPath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the folder to path of database file (only if not default path!).";
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtPath.Text = folderBrowserDialog.SelectedPath;
        }

        void PrivateAttributeChanged(object sender, EventArgs e)
        {
            DataAttributeChanged(sender, e);
            if (_displayedDataRow == null || GetSelectedDGV().SelectedRows.Count == 0) return; 
            bool priv = _displayedDataRow.Private == DefPar.Value.YES;
            GetSelectedDGV().SelectedRows[0].HeaderCell.Style.ForeColor = priv ? _privateForeColor : SystemColors.WindowText;
            GetSelectedDGV().SelectedRows[0].HeaderCell.Style.SelectionForeColor = priv ? _privateForeColor : SystemColors.HighlightText;
            GetSelectedDGV().SelectedRows[0].HeaderCell.Style.SelectionBackColor = priv ? _privateBackColor : SystemColors.Highlight;
        }

        void DataAttributeChanged(object sender, EventArgs e)
        {
            if (_displayedDataRow == null)
                return;

            if (txtPath.Text != _displayedDataRow.FilePath)
                _displayedDataRow.FilePath = txtPath.Text;
            if (txtYearCollection.Text != _displayedDataRow.YearCollection)
                _displayedDataRow.YearCollection = txtYearCollection.Text;
            if (txtYearIncome.Text != _displayedDataRow.YearInc)
                _displayedDataRow.YearInc = txtYearIncome.Text;
            if (txtListStringOutVar.Text != _displayedDataRow.ListStringOutVar)
                _displayedDataRow.ListStringOutVar = txtListStringOutVar.Text;
            if (txtIndirectTaxTableYear.Text != _displayedDataRow.IndirectTaxTableYear)
                _displayedDataRow.IndirectTaxTableYear = txtIndirectTaxTableYear.Text;
            if (cboCurrency.Text != _displayedDataRow.Currency)
                _displayedDataRow.Currency = cboCurrency.Text;
            if (cboDecimalSign.Text != _displayedDataRow.DecimalSign)
                _displayedDataRow.DecimalSign = cboDecimalSign.Text;
            _displayedDataRow.Private = chkPrivate.Checked ? DefPar.Value.YES : DefPar.Value.NO;
            _displayedDataRow.UseCommonDefault = chkUseCommonDefault.Checked ? DefPar.Value.YES : DefPar.Value.NO;
            _displayedDataRow.ReadXVariables = chkReadXVariables.Checked ? DefPar.Value.YES : DefPar.Value.NO;
            if (_displayedDataRow.ChangeState != ChangeStates.added)
                _displayedDataRow.ChangeState = ChangeStates.changed;
        }

        void btnDeleteDataBase_Click(object sender, EventArgs e)
        {
            if (_displayedDataRow == null)
                return;

            if (_displayedDataRow.ChangeState == ChangeStates.added)
                _dataBaseInfo.Remove(_displayedDataRow);
            else
                _displayedDataRow.ChangeState = ChangeStates.removed;
            _displayedDataRow = null;

            UpdateContentTable();
        }

        void btnAddDataBase_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "txt files (*.txt)|*.txt";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = EM_AppContext.FolderInput;

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            string dbName = openFileDialog.SafeFileName;
            string dbPath = openFileDialog.FileName.Substring(0, openFileDialog.FileName.Length - dbName.Length);
            if (dbPath.ToLower() == EM_AppContext.FolderInput.ToLower())
                dbPath = string.Empty;

            RowTag dataBaseInfo = new RowTag();
            dataBaseInfo.Name = dbName.EndsWith(".txt") ? dbName.Substring(0, dbName.Length-4) : dbName;
            dataBaseInfo.FilePath = dbPath;
            dataBaseInfo.ID = Guid.NewGuid().ToString(); //just a preliminary ID
            dataBaseInfo.ChangeState = ChangeStates.added;
            _dataBaseInfo.Add(dataBaseInfo);

            _displayedDataRow = null;
            UpdateContentTable(dataBaseInfo.ID);
        }

        internal ConfigureDataForm(string countryShortName)
        {
            InitializeComponent();

            _countryShortName = countryShortName;
            labelCountry.Text = _countryShortName;
            _displayedDataRow = null;

            for (int index = 0; index < GetCurrencies().Count; ++index)
                cboCurrency.Items.Add(GetCurrencies().ElementAt(index));
            for (int index = 0; index < GetDecimalSigns().Count; ++index)
                cboDecimalSign.Items.Add(GetDecimalSigns().ElementAt(index));
        }

        List<string> GetDecimalSigns() { return new List<string> { ".", "," }; }
        List<string> GetCurrencies() { return new List<string> { DefPar.Value.EURO, DefPar.Value.NATIONAL }; }

        void dgv_SelectionChanged(object sender, EventArgs e = null)
        {
            DataGridView dgv = sender as DataGridView;

            if (dgv.SelectedRows.Count != 1)
                return;

            _displayedDataRow = null; //to avoid updating while attributes are written

            RowTag dataBaseRow = dgv.SelectedRows[0].Tag as RowTag;
  
            //fill controls with database attributes
            txtPath.Text = (dataBaseRow == null) ? string.Empty : dataBaseRow.FilePath;
            txtYearCollection.Text = (dataBaseRow == null) ? string.Empty : dataBaseRow.YearCollection;
            txtYearIncome.Text = (dataBaseRow == null) ? string.Empty : dataBaseRow.YearInc;
            txtListStringOutVar.Text = (dataBaseRow == null) ? string.Empty : dataBaseRow.ListStringOutVar;
            txtIndirectTaxTableYear.Text = (dataBaseRow == null) ? string.Empty : dataBaseRow.IndirectTaxTableYear;
            string currency = (dataBaseRow == null) ? string.Empty : dataBaseRow.Currency.ToLower();
            cboCurrency.SelectedIndex = 0;
            for (int i = 0; i < GetCurrencies().Count; ++i)
                if (GetCurrencies().ElementAt(i).ToLower() == currency)
                {
                    cboCurrency.SelectedIndex = i;
                    break;
                }
            string decimalSign = (dataBaseRow == null) ? string.Empty : dataBaseRow.DecimalSign;
            cboDecimalSign.SelectedIndex = 0;
            for (int index = 0; index < GetDecimalSigns().Count; ++index)
                if (GetDecimalSigns().ElementAt(index).ToLower() == decimalSign)
                {
                    cboDecimalSign.SelectedIndex = index;
                    break;
                }
            chkPrivate.Checked = (dataBaseRow != null && dataBaseRow.Private == DefPar.Value.YES);

            if (dataBaseRow != null && dataBaseRow.IsCommonDefaultNull)
                OptionalWarningsManager.ShowOutdatedWarning("The parameter 'UseCommonDefault' does not exist for dataset '" + dataBaseRow.Name +
                    "'." + Environment.NewLine + "This is probably due to using an outdated TXT-to-XLM-converter." + Environment.NewLine + "Please make sure that the parameter is set properly.");

            chkUseCommonDefault.Checked = (dataBaseRow != null && !dataBaseRow.IsCommonDefaultNull && dataBaseRow.UseCommonDefault == DefPar.Value.YES);

            chkReadXVariables.Checked = (dataBaseRow != null && dataBaseRow.ReadXVariables == DefPar.Value.YES);

            //this is to make btnRenameDatabase_Click work (see comment in function)
            dgv.SelectedRows[0].HeaderCell.Value = dataBaseRow.Name;

            _displayedDataRow = dataBaseRow;
        }

        void dgv_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_displayedDataRow == null)
                return;

            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
            CellTag dbSystemCombination = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag as CellTag;
            string cellValue = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

            //the data-system combination was not available before
            if (dbSystemCombination == null) //combo was set from n/a (data not applicable for the system) to 'best' or 'x'
            {                                //null means that there was never another setting (i.e. the combination is actually new, not just changed back)
                dbSystemCombination = new CellTag();
                dbSystemCombination.SystemID = dgv.Columns[e.ColumnIndex].Tag.ToString();
                dbSystemCombination.ChangeState = ChangeStates.added;
                dbSystemCombination.BestMatch = (cellValue == _comboEntryBestMatch) ? DefPar.Value.YES : DefPar.Value.NO;
                RowTag dataRow = dgv.Rows[e.RowIndex].Tag as RowTag;
                dataRow.CellTags.Add(dbSystemCombination.SystemID, dbSystemCombination);
                dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag = dbSystemCombination;
            }
            //the data-system combination was available before
            else
            {
                if (cellValue == _comboEntryNA) //combo was set from 'best' or 'x' to n/a (data not applicable for the system)
                {
                    if (dbSystemCombination.ChangeState != ChangeStates.added)
                        dbSystemCombination.ChangeState = ChangeStates.removed;
                    else //combination was not really available, but preliminary created in this session of the dialog
                        _displayedDataRow.CellTags.Remove(dbSystemCombination.SystemID);
                }
                else //combo was set to 'x' or 'best'
                {
                    dbSystemCombination.BestMatch = (cellValue == _comboEntryBestMatch) ? DefPar.Value.YES : dbSystemCombination.BestMatch = DefPar.Value.NO;
                    if (dbSystemCombination.ChangeState != ChangeStates.added)
                        dbSystemCombination.ChangeState = ChangeStates.changed; //works for x->best, best->x, no matter if changed or unchanged before
                }                                                               //as well as for n/a->best n/a->x if removed before (i.e. user changed her mind)
            }
        }

        void btnRenameDatabase_Click(object sender, EventArgs e)
        {
            if (_displayedDataRow == null)
                return;

            string newName = _displayedDataRow.Name;
            if (UserInput.Get("New name of dataset", out newName, newName) == DialogResult.Cancel)
                return;

            _displayedDataRow.Name = newName;
            if (_displayedDataRow.ChangeState != ChangeStates.added)
                _displayedDataRow.ChangeState = ChangeStates.changed;

            //UpdateContentTable for what ever reason fails when trying to set the HeaderCell.Value, this works(?)
            dgv_SelectionChanged(GetSelectedDGV());
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                DataGridView dgv = GetSelectedDGV();
                if (dgv.Rows.Count > 0) dgv.Rows[0].Selected = true;
                dgv_SelectionChanged(dgv);
            }
            catch { }
        }

        void ctmMultiSelect_Opening(object sender, CancelEventArgs e)
        {
            _gridWhereMenuOpened = tabControl.SelectedIndex == 0 ? dgvSystemDataCombinations : dgvHHOT;
            if (_gridWhereMenuOpened == null) return;
            _mousePositionWhenMenuOpened = MousePosition;
            KeyValuePair<int, int> info = GetHitInfo();
            mniAllSystemsNa.Visible = info.Key > -1;
            mniAllSystemsX.Visible = info.Key > -1;
            toolStripSeparator1.Visible = info.Key > -1 && info.Value > -1;
            mniAllDatasetsNa.Visible = info.Value > -1;
            mniAllDatasetsX.Visible = info.Value > -1;
            toolStripSeparator2.Visible = info.Key > -1 | info.Value > -1;
            mniCopyFrom.Visible = info.Key > -1 | info.Value > -1;
        }
        int GetHitRow() { return GetHitInfo().Key; }
        int GetHitColumn() { return GetHitInfo().Value; }
        KeyValuePair<int, int> GetHitInfo()
        {
            Point hit = _gridWhereMenuOpened.PointToClient(_mousePositionWhenMenuOpened);
            DataGridView.HitTestInfo hitInfo = _gridWhereMenuOpened.HitTest(hit.X, hit.Y);
            return new KeyValuePair<int, int>(hitInfo.RowIndex, hitInfo.ColumnIndex);
        }

        void mniAllSystemsX_Click(object sender, EventArgs e) { SetAllSystemsTo(DefPar.Value.DATA_SYS_X); }
        void mniAllSystemsNa_Click(object sender, EventArgs e) { SetAllSystemsTo(DefPar.Value.NA); }
        void SetAllSystemsTo(string value)
        {
            if (_gridWhereMenuOpened == null) return;
            try
            {
                int datasetRow = GetHitRow();
                if (datasetRow == -1)
                {
                    UserInfoHandler.ShowInfo("Please open the menu via a position within the respective dataset's row.");
                    return;
                }

                DataGridViewRow row = _gridWhereMenuOpened.Rows[datasetRow];
                foreach (DataGridViewColumn column in _gridWhereMenuOpened.Columns)
                {
                    if (row.Cells[column.Index].ReadOnly == false)
                        row.Cells[column.Index].Value = value;
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void mniAllDatasetsX_Click(object sender, EventArgs e) { SetAllDatasetsTo(DefPar.Value.DATA_SYS_X); }
        void mniAllDatasetsNa_Click(object sender, EventArgs e) { SetAllDatasetsTo(DefPar.Value.NA); }
        void SetAllDatasetsTo(string value)
        {
            if (_gridWhereMenuOpened == null) return;
            try
            {
                int systemColumn = GetHitColumn();

                if (systemColumn == -1)
                {
                    UserInfoHandler.ShowInfo("Please open the menu via a position within the respective system's column.");
                    return;
                }

                foreach (DataGridViewRow row in _gridWhereMenuOpened.Rows)
                {
                    if (row.Cells[systemColumn].ReadOnly == false)
                        row.Cells[systemColumn].Value = value;
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void mniCopyFrom_DropDownOpening(object sender, EventArgs e)
        {
            int selCol = GetHitColumn(), selRow = GetHitRow();
            mniCopyFrom.DropDownItems.Clear();
           
            if (selCol >= 0)
            {
                foreach (DataGridViewColumn col in _gridWhereMenuOpened.Columns)
                {
                    if (col.Index == selCol) continue;
                    ToolStripMenuItem subItem = new ToolStripMenuItem() { Text = col.HeaderText };
                    subItem.Click += (s, ev) =>
                    {
                        foreach (DataGridViewRow row in _gridWhereMenuOpened.Rows)
                            row.Cells[selCol].Value = row.Cells[col.Index].Value;
                    };
                    mniCopyFrom.DropDownItems.Add(subItem);
                }
            }
            
            if (mniCopyFrom.DropDownItems.Count > 0 && selRow >= 0) mniCopyFrom.DropDownItems.Add(new ToolStripSeparator());
            
            if (selRow >= 0)
            {
                foreach (DataGridViewRow row in _gridWhereMenuOpened.Rows)
                {
                    if (row.Index == selRow) continue;
                    ToolStripMenuItem subItem = new ToolStripMenuItem() { Text = row.HeaderCell.Value.ToString() };
                    subItem.Click += (s, ev) =>
                    {
                        foreach (DataGridViewColumn col in _gridWhereMenuOpened.Columns)
                            _gridWhereMenuOpened.Rows[selRow].Cells[col.Index].Value = row.Cells[col.Index].Value;
                    };
                    mniCopyFrom.DropDownItems.Add(subItem);
                }
            }
        }
    }
}
