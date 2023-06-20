using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.DataSets;
using EM_UI.Validate;
using EM_UI.NodeOperations;
using EM_UI.Tools;
using EM_Common;

namespace EM_UI.Dialogs
{
    internal partial class ConditionalFormattingForm : Form
    {
        CountryConfigFacade _countryConfigFacade = null;

        const string _separator = ", ";
        const string _exampleCondition = "{TEXT} OR {*TEXT*} OR {??TEXT}";

        internal static Color _defaultColorSystemDifferences = Color.FromArgb(255, 192, 0); //default color for showing difference between base and derived system, e.g. 2005 versus 2006, or 2005 versus 2005 reform (it's the color used in the Excel interface)

        DataGridView _clearColorGridView = null;
        int _clearColorRowIndex = -1;
        int _clearColorColumnIndex = -1;

        internal List<KeyValuePair<string, string>> _systemsToExpand { get; set; }

        void ConditionalFormattingForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            //store information of grid for format settings of conditional formatting
            for (int iRow = 0; iRow < dgvConditionalFormatting.RowCount; ++iRow)
            {
                CountryConfig.ConditionalFormatRow conditionalFormatRow = null;
                if (dgvConditionalFormatting.Rows[iRow].Tag != null)
                    conditionalFormatRow = dgvConditionalFormatting.Rows[iRow].Tag as CountryConfig.ConditionalFormatRow;

                //delete condtional formats
                if (dgvConditionalFormatting.Rows[iRow].Visible == false)
                {
                    _countryConfigFacade.DeleteConditionalFormatRow(conditionalFormatRow);
                    continue;
                }

                // first delete any existing line, then build the new one.
                if (conditionalFormatRow != null) conditionalFormatRow.Delete();
                conditionalFormatRow = _countryConfigFacade.AddConditionalFormatRow(
                    dgvConditionalFormatting.Rows[iRow].Cells[colBackColorConditionalFormatting.Name].Value.ToString(),
                    dgvConditionalFormatting.Rows[iRow].Cells[colForeColorConditionalFormatting.Name].Value.ToString(),
                    dgvConditionalFormatting.Rows[iRow].Cells[colCondition.Name].Value.ToString(),
                    string.Empty);
                
                //change conditional formats' systems to apply on
                object cellValue = dgvConditionalFormatting.Rows[dgvConditionalFormatting.Rows[iRow].Index].Cells[colSystemsToApply.Name].Value;
                if (cellValue != null)
                {
                    foreach (string systemName in cellValue.ToString().Split(_separator.First()))
                    {
                        CountryConfig.SystemRow systemRow = _countryConfigFacade.GetSystemRowByName(systemName.Trim());
                        if (systemRow != null)
                            _countryConfigFacade.AddConditionalFormat_SystemsRow(conditionalFormatRow, systemRow);
                    }
                }
            }

            //store information of grid for format settings of base-derived-system differences
            _systemsToExpand = new List<KeyValuePair<string, string>>();
            for (int iRow = 0; iRow < dgvBaseSystemFormatting.RowCount; ++iRow)
            {
                CountryConfig.SystemRow systemRow = _countryConfigFacade.GetSystemRowByName(dgvBaseSystemFormatting.Rows[iRow].Cells[colSystem.Name].Value.ToString());
                if (systemRow == null)
                    continue;

                CountryConfig.ConditionalFormatRow conditionalFormatRow = null;
                if (dgvBaseSystemFormatting.Rows[iRow].Tag != null)
                    conditionalFormatRow = dgvBaseSystemFormatting.Rows[iRow].Tag as CountryConfig.ConditionalFormatRow;

                string baseSystemName = string.Empty;
                object cellValue = dgvBaseSystemFormatting.Rows[iRow].Cells[colBaseSystem.Name].Value;
                if (cellValue != null)
                    baseSystemName = cellValue.ToString();

                //expanding of differences between system and its base (if ticked by the user) is handled by the TreeListManager after the CF-action (see TreeListManager.HandleConditionalFormatting)
                //here only gather respective systems and their base
                if (EM_Helpers.SaveConvertToBoolean(dgvBaseSystemFormatting.Rows[iRow].Cells[colExpandDifferences.Name].Value) == true)
                {
                    if (baseSystemName == string.Empty)
                        continue;
                    _systemsToExpand.Add(new KeyValuePair<string, string>(systemRow.Name, baseSystemName));
                }

                if (conditionalFormatRow != null)   // if there was a base system
                {
                    if (conditionalFormatRow.BaseSystemName == baseSystemName) //just format changed, but same base system
                    {
                        conditionalFormatRow.BackColor = dgvBaseSystemFormatting.Rows[iRow].Cells[colBackColorBaseSystemFormatting.Name].Value.ToString();
                        conditionalFormatRow.ForeColor = dgvBaseSystemFormatting.Rows[iRow].Cells[colForeColorBaseSystemFormatting.Name].Value.ToString();
                        continue;
                    }
                    conditionalFormatRow.Delete(); // else delete this and make a new one if required
                }

                if (baseSystemName == string.Empty) continue;

                conditionalFormatRow = _countryConfigFacade.AddConditionalFormatRow(
                                        dgvBaseSystemFormatting.Rows[iRow].Cells[colBackColorBaseSystemFormatting.Name].Value.ToString(),
                                        dgvBaseSystemFormatting.Rows[iRow].Cells[colForeColorBaseSystemFormatting.Name].Value.ToString(),
                                        string.Empty, baseSystemName);

                _countryConfigFacade.AddConditionalFormat_SystemsRow(conditionalFormatRow, systemRow);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void btnAddCondForm_Click(object sender, EventArgs e)
        {
            string allSystemNames = string.Empty; //as default suggust to apply on all systems
            foreach (CountryConfig.SystemRow systemRow in _countryConfigFacade.GetSystemRows())
                allSystemNames += systemRow.Name + _separator;
            if (allSystemNames != string.Empty)
                allSystemNames = allSystemNames.Substring(0, allSystemNames.Length - 1);

            int index = dgvConditionalFormatting.Rows.Add(_exampleCondition, allSystemNames,
                        ConditionalFormattingHelper._noSpecialColor, ConditionalFormattingHelper._noSpecialColor);
            dgvConditionalFormatting.Rows[index].Tag = null;
            dgvConditionalFormatting.Rows[index].Selected = true;
        }

        void btnDeleteCondForm_Click(object sender, EventArgs e)
        {
            int index = GetSelectedRow(dgvConditionalFormatting);
            if (index < 0)
                return;
            if (dgvConditionalFormatting.Rows[index].Tag == null)
                dgvConditionalFormatting.Rows.RemoveAt(index);
            else
                dgvConditionalFormatting.Rows[index].Visible = false;
        }

        void btnSetDefaultSysDif_Click(object sender, EventArgs e)
        {
            for (int index = 0; index < dgvBaseSystemFormatting.Rows.Count; ++index)
            {
                dgvBaseSystemFormatting.Rows[index].Cells[colBackColorBaseSystemFormatting.Name].Value =
                    ConditionalFormattingHelper.GetDisplayTextFromColor(_defaultColorSystemDifferences);
                dgvBaseSystemFormatting.Rows[index].Cells[colForeColorBaseSystemFormatting.Name].Value = ConditionalFormattingHelper._noSpecialColor;
            }
        }

        int GetSelectedRow(DataGridView dataGridView)
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                EM_UI.Tools.UserInfoHandler.ShowError("Please select a row.");
                return -1;
            }
            return dataGridView.SelectedRows[0].Index;
        }

        internal ConditionalFormattingForm(string countryShortName, CountryConfigFacade countryConfigFacade)
        {
            InitializeComponent();

            _countryConfigFacade = countryConfigFacade;
            lblCountry.Text = countryShortName;

            //put all systems in a list for easier access
            List<string> allSystemNames = new List<string>();
            foreach (CountryConfig.SystemRow systemRow in countryConfigFacade.GetSystemRows())
                allSystemNames.Add(systemRow.Name);

            //grid for format settings of base-derived-system differences:
            //add a row for each system (to be completed by base-system and format settings below, if there are any)
            //fill combo for selecting base system (with all available systems)
            foreach (string systemName in allSystemNames)
            {
                int index = dgvBaseSystemFormatting.Rows.Add(systemName, string.Empty,
                            ConditionalFormattingHelper._noSpecialColor, ConditionalFormattingHelper._noSpecialColor);
                dgvBaseSystemFormatting.Rows[index].Cells[colBackColorBaseSystemFormatting.Name].Value =
                    ConditionalFormattingHelper.GetDisplayTextFromColor(_defaultColorSystemDifferences);
                dgvBaseSystemFormatting.Rows[index].Tag = null;
            }

            foreach (CountryConfig.ConditionalFormatRow conditionalFormatRow in countryConfigFacade.GetConditionalFormatRows())
            {
                //fill grid for format settings of conditional formatting
                if (conditionalFormatRow.Condition != null && conditionalFormatRow.Condition != string.Empty)
                {
                    string applySystemNames = string.Empty;
                    foreach (CountryConfig.ConditionalFormat_SystemsRow conditionalFormat_SystemsRow in conditionalFormatRow.GetConditionalFormat_SystemsRows())
                        applySystemNames += conditionalFormat_SystemsRow.SystemName + _separator;
                    if (applySystemNames != string.Empty)
                        applySystemNames = applySystemNames.Substring(0, applySystemNames.Length - 1);
                    int index = dgvConditionalFormatting.Rows.Add(conditionalFormatRow.Condition, applySystemNames,
                        conditionalFormatRow.BackColor == string.Empty ? ConditionalFormattingHelper._noSpecialColor : conditionalFormatRow.BackColor,
                        conditionalFormatRow.ForeColor == string.Empty ? ConditionalFormattingHelper._noSpecialColor : conditionalFormatRow.ForeColor);
                    dgvConditionalFormatting.Rows[index].Tag = conditionalFormatRow;
                }

                //complete grid for format settings of base-derived-system differences
                if (conditionalFormatRow.BaseSystemName != null && conditionalFormatRow.BaseSystemName != string.Empty)
                {
                    if (countryConfigFacade.GetSystemRowByName(conditionalFormatRow.BaseSystemName) == null)
                        continue; //base system may have been deleted meanwhile
                    if (conditionalFormatRow.GetConditionalFormat_SystemsRows().Count() == 0)
                        continue; //should not happen, there ought to be either no or exactely one base-derived-differences setting (and if there is a setting there must be a respective system-format-link)
                    CountryConfig.ConditionalFormat_SystemsRow conditionalFormat_SystemsRow = conditionalFormatRow.GetConditionalFormat_SystemsRows().ElementAt(0);
                    int index = allSystemNames.IndexOf(conditionalFormat_SystemsRow.SystemName);
                    if (index < 0)
                        continue; //should not happen (would mean incomplete delete of a system or a similar error)
                    dgvBaseSystemFormatting.Rows[index].Cells[colBaseSystem.Name].Value = conditionalFormatRow.BaseSystemName;
                    dgvBaseSystemFormatting.Rows[index].Cells[colBackColorBaseSystemFormatting.Name].Value =
                        conditionalFormatRow.BackColor == string.Empty ? ConditionalFormattingHelper._noSpecialColor : conditionalFormatRow.BackColor;
                    dgvBaseSystemFormatting.Rows[index].Cells[colForeColorBaseSystemFormatting.Name].Value =
                        conditionalFormatRow.ForeColor == string.Empty ? ConditionalFormattingHelper._noSpecialColor : conditionalFormatRow.ForeColor;
                    dgvBaseSystemFormatting.Rows[index].Tag = conditionalFormatRow;
                }
            }
        }

        void dgvConditionalFormatting_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)    
                return;

            if (e.ColumnIndex == colBackColorConditionalFormatting.Index || e.ColumnIndex == colForeColorConditionalFormatting.Index)
                SetBackOrTextColor(dgvConditionalFormatting, e.RowIndex, e.ColumnIndex);

            if (e.ColumnIndex == colSystemsToApply.Index)
                AddDelSystem(e.RowIndex);
        }

        void SetBackOrTextColor(DataGridView gridView, int rowIndex, int columnIndex)
        {
            dlgColor.Color = ConditionalFormattingHelper.GetColorFromDisplayText((gridView.Rows[rowIndex].Cells[columnIndex]).Value.ToString());
            if (dlgColor.ShowDialog() == DialogResult.Cancel)
                return;
            (gridView.Rows[rowIndex].Cells[columnIndex]).Value = ConditionalFormattingHelper.GetDisplayTextFromColor(dlgColor.Color);
        }

        void AddDelSystem(int rowIndex)
        {
            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(lblCountry.Text, null);
            List<string> checkedSystemNames = new List<string>();
            foreach (string systemName in dgvConditionalFormatting.Rows[rowIndex].Cells[colSystemsToApply.Name].Value.ToString().Split(_separator.First()))
                checkedSystemNames.Add(systemName.Trim());
            selectSystemsForm.CheckSystems(checkedSystemNames);
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;

            string applySystemNames = string.Empty;
            foreach (CountryConfig.SystemRow systemRow in selectSystemsForm.GetSelectedSystemRows())
                applySystemNames += systemRow.Name + _separator;
            if (applySystemNames != string.Empty)
                applySystemNames = applySystemNames.Substring(0, applySystemNames.Length - 1);
            dgvConditionalFormatting.Rows[rowIndex].Cells[colSystemsToApply.Name].Value = applySystemNames;
        }

        void dgvBaseSystemFormatting_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.ColumnIndex == colBackColorBaseSystemFormatting.Index || e.ColumnIndex == colForeColorBaseSystemFormatting.Index)
                SetBackOrTextColor(dgvBaseSystemFormatting, e.RowIndex, e.ColumnIndex);

            if (e.ColumnIndex != colBaseSystem.Index)
                return;
            
            List<string> noShowSystemsIDs = new List<string>();
            noShowSystemsIDs.Add(_countryConfigFacade.GetSystemRowByName(dgvBaseSystemFormatting.Rows[e.RowIndex].Cells[colSystem.Name].Value.ToString()).ID);
            List<string> checkedSystemNames = new List<string>();
            checkedSystemNames.Add(dgvBaseSystemFormatting.Rows[e.RowIndex].Cells[colBaseSystem.Name].Value.ToString());

            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(lblCountry.Text, noShowSystemsIDs);
            selectSystemsForm.CheckSystems(checkedSystemNames);
            selectSystemsForm.SetSingleSelectionMode(false);
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;

            dgvBaseSystemFormatting.Rows[e.RowIndex].Cells[colBaseSystem.Name].Value = string.Empty;
            if (selectSystemsForm.GetSelectedSystemRows().Count > 0)
                dgvBaseSystemFormatting.Rows[e.RowIndex].Cells[colBaseSystem.Name].Value = selectSystemsForm.GetSelectedSystemRows().First().Name;
        }

        void dgvConditionalFormatting_MouseClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PrepareClearColorMenu(dgvConditionalFormatting, e);
        }

        void dgvBaseSystemFormatting_MouseClick(object sender, MouseEventArgs e)
        {
            PrepareClearColorMenu(dgvBaseSystemFormatting, e);
        }

        void PrepareClearColorMenu(DataGridView gridView, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;

            int rowIndex = gridView.HitTest(e.X, e.Y).RowIndex;
            int columnIndex = gridView.HitTest(e.X, e.Y).ColumnIndex;

            if (rowIndex < 0 || columnIndex < 0)
                return;

            if (columnIndex != colBackColorBaseSystemFormatting.Index && columnIndex != colForeColorBaseSystemFormatting.Index &&
                columnIndex != colBackColorConditionalFormatting.Index && columnIndex != colForeColorConditionalFormatting.Index)
                return;

            _clearColorGridView = gridView;
            _clearColorRowIndex = rowIndex;
            _clearColorColumnIndex = columnIndex;
            
            menuClearColor.Show(gridView, new Point(e.X, e.Y));
        }

        void menuClearColor_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (_clearColorGridView == null)
                return;
            (_clearColorGridView.Rows[_clearColorRowIndex].Cells[_clearColorColumnIndex]).Value = ConditionalFormattingHelper._noSpecialColor;

            _clearColorGridView = null;
            _clearColorRowIndex = -1;
            _clearColorColumnIndex = -1;
        }

        private void dgvBaseSystemFormatting_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
