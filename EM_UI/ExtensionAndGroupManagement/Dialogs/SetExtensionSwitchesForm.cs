using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal partial class SetExtensionSwitchesForm : Form
    {
        DataConfig _dataConfig = null;
        List<ExtensionSwitchDefaultValue> _extensionDefaultSwitches = null;
        string _countryShortName = null;
        string _selectedPolicyID = string.Empty;
        Point _mousePositionWhenMenuOpened;
        static Color _privateColor = Color.Red;

        void SetPolicySwitchesForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            dgvSwitches.EnableHeadersVisualStyles = false; // without this changing header-colours (for private systems and databases) would have no effect
        }

        internal SetExtensionSwitchesForm(string countryShortName)
        {
            InitializeComponent();

            try
            {
                _countryShortName = countryShortName;
                labelCountry.Text = _countryShortName;
                _dataConfig = CountryAdministrator.GetDataConfigFacade(_countryShortName).GetDataConfig();

                _extensionDefaultSwitches = new List<ExtensionSwitchDefaultValue>();
                foreach (GlobLocExtensionRow e in ExtensionAndGroupManager.GetExtensions(_countryShortName))
                {
                    ListViewItem item = lvSwitchablePolicies.Items.Add(e.Name); item.SubItems.Add(e.ShortName); item.Tag = e.ID;

                    //store initial default values of the policy switches in this structure,
                    //which is necessary as the dialog changes between switchable policies and the changed values need to be buffered
                    //(actual database changes are performed after closing the dialog with OK)
                    foreach (DataConfig.DataBaseRow dataBaseRow in from db in _dataConfig.DataBase select db)
                        foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in from ds in _dataConfig.DBSystemConfig where ds.DataBaseID == dataBaseRow.ID select ds)
                            _extensionDefaultSwitches.Add(new ExtensionSwitchDefaultValue(e.ID, dbSystemConfigRow,
                                ExtensionAndGroupManager.GetExtensionDefaultSwitch(dbSystemConfigRow, e.ID))); //returns n/a if switch does not (yet) exist
                }

                InitSwitchTable(); //draw columns (systems) and rows (datasets) of the table

                if (lvSwitchablePolicies.Items.Count > 0)
                    lvSwitchablePolicies.Items[0].Checked = true; //select the first switchable policy

                UpdateSwitchTable(); //fill the table with the switch values of this policy
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        internal List<ExtensionSwitchDefaultValue> GetExtensionDefaultSwitches() { return _extensionDefaultSwitches; }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                StoreDisplayedSwitches(); //before leaving the dialog, overtake potential changes in the currently displayed switch-table into the buffer-table
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        DataGridViewComboBoxColumn CreateSystemColumn(string columnName, string headerText)
        {
            DataGridViewComboBoxColumn column = new DataGridViewComboBoxColumn();
            {
                column.Name = columnName;
                column.HeaderText = headerText;
                column.Items.Add(DefPar.Value.ON);
                column.Items.Add(DefPar.Value.OFF);
                column.Items.Add(DefPar.Value.NA);
            }
            dgvSwitches.Columns.Add(column);
            return column;
        }

        void InitSwitchTable()
        {
            dgvSwitches.Rows.Clear();
            dgvSwitches.Columns.Clear();

            //add systems as columns
            foreach (CountryConfig.SystemRow systemRow in CountryAdministrator.GetCountryConfigFacade(_countryShortName).GetSystemRows())
            {
                DataGridViewComboBoxColumn dgvColumn = CreateSystemColumn("col" + systemRow.Name, systemRow.Name); //create the combo-boxes with values on, off and n/a
                if (systemRow.Private == DefPar.Value.YES) dgvColumn.HeaderCell.Style.ForeColor = _privateColor;
                dgvColumn.Tag = systemRow;
            }

            //add datasets as rows
            foreach (DataConfig.DataBaseRow dataBaseRow in from db in _dataConfig.DataBase select db)
            {
                int rowIndex = dgvSwitches.Rows.Add();
                DataGridViewRow dgvRow = dgvSwitches.Rows[rowIndex];
                dgvRow.HeaderCell.Value = dataBaseRow.Name;
                dgvRow.Tag = dataBaseRow.ID;
                if (dataBaseRow.Private == DefPar.Value.YES)
                {
                    dgvRow.HeaderCell.Style.ForeColor = _privateColor;
                    dgvRow.HeaderCell.Style.SelectionForeColor = _privateColor;
                }
            }
        }
        
        void UpdateSwitchTable(bool reportInvalid = true)
        {
            _selectedPolicyID = string.Empty;

            if (lvSwitchablePolicies.CheckedItems.Count == 0)
                return; //leave table empty if no switchable policy is available

            //find out which switchable policy is selected
            _selectedPolicyID = (lvSwitchablePolicies.CheckedItems[0].Tag != null) ? lvSwitchablePolicies.CheckedItems[0].Tag.ToString() : string.Empty;
            txtSelectedSwitchPolicy.Text = lvSwitchablePolicies.CheckedItems[0].Text;

            string invalidDefaultSwiches = string.Empty;
            foreach (DataGridViewRow dgvRow in dgvSwitches.Rows)
            {
                for (int columnIndex = 0; columnIndex < dgvSwitches.Columns.Count; ++columnIndex)
                {
                    //run over available dataset-system combinations
                    dgvRow.Cells[columnIndex].Value = string.Empty; //if dataset is not applicable for the system put an emtpy string, which is overwritten, if a db-system-combination is found
                    dgvRow.Cells[columnIndex].ReadOnly = true;
                    foreach (DataConfig.DBSystemConfigRow dbSystemConfigRow in from ds in _dataConfig.DBSystemConfig where ds.DataBaseID == dgvRow.Tag as string select ds)
                    {
                        //if dataset is applicable for the system, get (if exists) the policy-switch value for this switchable policy (i.e. the selected switchable policy), system and dataset
                        if (dbSystemConfigRow.SystemID == (dgvSwitches.Columns[columnIndex].Tag as CountryConfig.SystemRow).ID)
                        {
                            string defaultSwitch = GetExtensionDefaultSwitch(dbSystemConfigRow, _selectedPolicyID);
                            if (defaultSwitch != DefPar.Value.ON && defaultSwitch != DefPar.Value.OFF && defaultSwitch != DefPar.Value.NA)
                                invalidDefaultSwiches += Environment.NewLine + $"Invalid Switch: '{defaultSwitch}' for {txtSelectedSwitchPolicy.Text}/{dbSystemConfigRow.DataBaseRow.Name}/{dbSystemConfigRow.SystemName}";
                            else dgvRow.Cells[columnIndex].Value = defaultSwitch;                
                            dgvRow.Cells[columnIndex].ReadOnly = false;
                            dgvRow.Cells[columnIndex].Tag = dbSystemConfigRow;
                            break;
                        }
                    }
                }
            }
            if (reportInvalid && !string.IsNullOrEmpty(invalidDefaultSwiches)) UserInfoHandler.ShowError(
                $"{EMPath.GetEM2DataConfigFileName(_countryShortName)} contains invalid default switches!!!" + Environment.NewLine +
                    (invalidDefaultSwiches.Length > 1500 ? invalidDefaultSwiches.Substring(0, 1500) + "..." : invalidDefaultSwiches));
        }

        string GetExtensionDefaultSwitch(DataConfig.DBSystemConfigRow dbSystemConfigRow, string switchablePolicyID)
        {
            foreach (ExtensionSwitchDefaultValue policySwitchDefaultValue in _extensionDefaultSwitches)
                if (policySwitchDefaultValue._dbSystemConfigRow == dbSystemConfigRow && policySwitchDefaultValue._extensionID == switchablePolicyID)
                    return policySwitchDefaultValue._defaultValue;
            return DefPar.Value.NA; // switch is not (yet) existing
        }

        void SetExtensionDefaultSwitch(DataConfig.DBSystemConfigRow dbSystemConfigRow, string switchablePolicyID, string defaultValue)
        {
            foreach (ExtensionSwitchDefaultValue policySwitchDefaultValue in _extensionDefaultSwitches)
                if (policySwitchDefaultValue._dbSystemConfigRow == dbSystemConfigRow && policySwitchDefaultValue._extensionID == switchablePolicyID)
                {
                    policySwitchDefaultValue._defaultValue = defaultValue;
                    return;
                }
        }

        void lvSwitchablePolicies_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (lvSwitchablePolicies.CheckedItems.Count > 0)
                lvSwitchablePolicies.CheckedItems[0].Checked = false; //to provide for single-selection
        }

        void lvSwitchablePolicies_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            try
            {
                StoreDisplayedSwitches(); //overtake potential changes in the currently displayed switch-table into the dataconfig-table
                UpdateSwitchTable(e.Item.Checked); //set switches-tabel to the values of the now selected switchable policy

                if (e.Item.Checked)
                    e.Item.Selected = true; //without doing this an item can be checked but not selected (blue background color)
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void StoreDisplayedSwitches()
        {
            if (_selectedPolicyID != string.Empty)
            {
                foreach (DataGridViewRow row in dgvSwitches.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        if (cell.Tag == null)
                            continue; //dataset not applicable for system
                        DataConfig.DBSystemConfigRow dbSystemConfigRow = cell.Tag as DataConfig.DBSystemConfigRow;
                        if (dbSystemConfigRow == null)
                            continue; //should not happen
                        SetExtensionDefaultSwitch(dbSystemConfigRow, _selectedPolicyID, cell.Value.ToString());
                    }
                }
            }
        }

        void ctmMultiSelect_Opening(object sender, CancelEventArgs e) 
        { 
            _mousePositionWhenMenuOpened = MousePosition;
            KeyValuePair<int, int> info = GetHitInfo();
            mniAllSystemsNa.Visible = info.Key > -1;
            mniAllSystemsOff.Visible = info.Key > -1;
            mniAllSystemsOn.Visible = info.Key > -1;
            toolStripSeparator1.Visible = info.Key > -1;
            mniAllDatasetsNa.Visible = info.Value > -1;
            mniAllDatasetsOff.Visible = info.Value > -1;
            mniAllDatasetsOn.Visible = info.Value > -1;
            toolStripSeparator2.Visible = info.Value > -1;
        }
        int GetHitRow() { return GetHitInfo().Key; }
        int GetHitColumn() { return GetHitInfo().Value; }
        KeyValuePair<int, int> GetHitInfo()
        {
            Point hit = dgvSwitches.PointToClient(_mousePositionWhenMenuOpened);
            DataGridView.HitTestInfo hitInfo = dgvSwitches.HitTest(hit.X, hit.Y);
            return new KeyValuePair<int, int>(hitInfo.RowIndex, hitInfo.ColumnIndex);
        }

        void mniAllSystemsOn_Click(object sender, EventArgs e) { SetAllSystemsTo(DefPar.Value.ON); }
        void mniAllSystemsOff_Click(object sender, EventArgs e) { SetAllSystemsTo(DefPar.Value.OFF); }
        void mniAllSystemsNa_Click(object sender, EventArgs e) { SetAllSystemsTo(DefPar.Value.NA); }
        void SetAllSystemsTo(string value)
        {
            try
            {
                int datasetRow = GetHitRow();
                if (datasetRow == -1)
                {
                    UserInfoHandler.ShowInfo("Please open the menu via a position within the respective dataset's row.");
                    return;
                }

                DataGridViewRow row = dgvSwitches.Rows[datasetRow];
                foreach (DataGridViewColumn column in dgvSwitches.Columns)
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

        void mniAllDatasetsOn_Click(object sender, EventArgs e) { SetAllDatasetsTo(DefPar.Value.ON); }
        void mniAllDatasetsOff_Click(object sender, EventArgs e) { SetAllDatasetsTo(DefPar.Value.OFF); }
        void mniAllDatasetsNa_Click(object sender, EventArgs e) { SetAllDatasetsTo(DefPar.Value.NA); }
        void SetAllDatasetsTo(string value)
        {
            try
            {
                int systemColumn = GetHitColumn();

                if (systemColumn == -1)
                {
                    UserInfoHandler.ShowInfo("Please open the menu via a position within the respective system's column.");
                    return;
                }

                foreach (DataGridViewRow row in dgvSwitches.Rows)
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

        void mniAllOn_Click(object sender, EventArgs e) { SetAllTo(DefPar.Value.ON); }
        void mniAllOff_Click(object sender, EventArgs e) { SetAllTo(DefPar.Value.OFF); }
        void mniAllNa_Click(object sender, EventArgs e) { SetAllTo(DefPar.Value.NA); }
        void SetAllTo(string value)
        {
            try
            {
                foreach (DataGridViewRow row in dgvSwitches.Rows)
                {
                    foreach (DataGridViewColumn column in dgvSwitches.Columns)
                    {
                        if (row.Cells[column.Index].ReadOnly == false)
                            row.Cells[column.Index].Value = value;
                    }
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void lvSwitchablePolicies_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lvSwitchablePolicies.SelectedIndices.Count > 0)
                lvSwitchablePolicies.Items[lvSwitchablePolicies.SelectedIndices[0]].Checked = true; //without doing this an item can be selected (blue background color) but not checked
        }
    }
}
