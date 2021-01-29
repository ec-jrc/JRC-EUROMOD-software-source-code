using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.GlobalAdministration
{
    internal partial class ExchangeRatesForm : Form
    {
        internal List<ExchangeRate> updatedExRates = null;
        internal Dictionary<string, List<Tuple<string, string>>> requiredCountryUpdates = new Dictionary<string, List<Tuple<string, string>>>();
        private List<ExchangeRate> initalExRates = new List<ExchangeRate>();

        internal ExchangeRatesForm(List<ExchangeRatesConfig.ExchangeRatesRow> exRates)
        {
            InitializeComponent();

            try
            {
                if (!dgvRates.InitDataSource()) { ShowGridLastError(); return; }
                dgvRates.allowAddRows = false; dgvRates.showDelRowWarning = true;
                dgvRates.pasteableColumns = new List<string>()
                {
                    this.colJune30.Name,
                    this.colYearAverage.Name,
                    this.colFirstSemester.Name,
                    this.colSecondSemester.Name
                };

                if (!dgvRates.AddColumn(false, colCountry.Name, typeof(string))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colJune30.Name, typeof(double))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colYearAverage.Name, typeof(double))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colFirstSemester.Name, typeof(double))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colSecondSemester.Name, typeof(double))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colDefault.Name, typeof(string))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colValidFor.Name, typeof(string))) { ShowGridLastError(); return; }
                if (!dgvRates.AddColumn(false, colID.Name, typeof(Int16), -1, true)) { ShowGridLastError(); return; }

                foreach (ExchangeRatesConfig.ExchangeRatesRow exRate in exRates)
                {
                    DataRow dataRow;
                    if (!dgvRates.AddRow(false, out dataRow)) { ShowGridLastError(); return; }
                    if (!dgvRates.SetCellValue(dataRow, colCountry.Name, exRate.Country)) { ShowGridLastError(); return; }
                    if (exRate.June30 > 0) { if (!dgvRates.SetCellValue(dataRow, colJune30.Name, exRate.June30)) { ShowGridLastError(); return; } }
                    if (exRate.YearAverage > 0) { if (!dgvRates.SetCellValue(dataRow, colYearAverage.Name, exRate.YearAverage)) { ShowGridLastError(); return; } }
                    if (exRate.FirstSemester > 0) { if (!dgvRates.SetCellValue(dataRow, colFirstSemester.Name, exRate.FirstSemester)) { ShowGridLastError(); return; } }
                    if (exRate.SecondSemester > 0) { if (!dgvRates.SetCellValue(dataRow, colSecondSemester.Name, exRate.SecondSemester)) { ShowGridLastError(); return; } }
                    if (!dgvRates.SetCellValue(dataRow, colDefault.Name, exRate.Default)) { ShowGridLastError(); return; }
                    if (!dgvRates.SetCellValue(dataRow, colValidFor.Name, exRate.ValidFor)) { ShowGridLastError(); return; }

                    initalExRates.Add(new ExchangeRate(exRate)); // remember for checking if any country files need updating once user stores the changes
                }

                List<string> countries = (from c in CountryAdministrator.GetCountries() select c._shortName.ToUpper()).ToList();
                foreach (string country in countries) cmbCountryToAdd.Items.Add(country);

                if (!dgvRates.StartUndoManager()) ShowGridLastError();
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private void ShowGridLastError() { if (dgvRates.lastError != string.Empty) UserInfoHandler.ShowError(dgvRates.lastError); }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (!dgvRates.verifyNumericValues(out string problems))
                {
                    UserInfoHandler.ShowError("There are cells with invalid values!" + Environment.NewLine + problems);
                    return;
                }
                updatedExRates = new List<ExchangeRate>();
                DataTable dtExRates = dgvRates.GetDataTable();
                foreach (DataRow row in dtExRates.Rows)
                {
                    if (row[colValidFor.Name] == DBNull.Value || row.Field<string>(colValidFor.Name).ToString() == string.Empty) continue;
                    string defaultCol = string.Empty;
                    switch (row.Field<string>(colDefault.Name))
                    {
                        case ExchangeRate.JUNE30: defaultCol = colJune30.Name; break;
                        case ExchangeRate.YEARAVERAGE: defaultCol = colYearAverage.Name; break;
                        case ExchangeRate.FIRSTSEMESTER: defaultCol = colFirstSemester.Name; break;
                        case ExchangeRate.SECONDSEMESTER: defaultCol = colSecondSemester.Name; break;
                    }
                    if (row[defaultCol] == DBNull.Value)
                    {
                        UserInfoHandler.ShowInfo(string.Format("{0} ({1}): Default ({2}) must not be empty.",
                            row.Field<string>(colCountry.Name), row.Field<string>(colValidFor.Name), defaultCol)); return;
                    }
                    updatedExRates.Add(new ExchangeRate() { 
                        Country = row.Field<string>(colCountry.Name),
                        June30 = row[colJune30.Name] == DBNull.Value ? -1 : double.Parse(row[colJune30.Name].ToString()),
                        YearAverage = row[colYearAverage.Name] == DBNull.Value ? -1 : double.Parse(row[colYearAverage.Name].ToString()),
                        FirstSemester = row[colFirstSemester.Name] == DBNull.Value ? -1 : double.Parse(row[colFirstSemester.Name].ToString()),
                        SecondSemester = row[colSecondSemester.Name] == DBNull.Value ? -1 : double.Parse(row[colSecondSemester.Name].ToString()),
                        Default = row.Field<string>(colDefault.Name),
                        ValidFor = row.Field<string>(colValidFor.Name) });
                }

                AssessChanges(); // check whether any country files need to be updated
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return; }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AssessChanges() // gather all systems with new or changed rates
        {                            // ignore deleted - they keep the rate stored in country file (what would a system without exchange rate mean? it's more likely that the system does not exist anymore)
            foreach (ExchangeRate updated in updatedExRates)
            {
                string country = updated.Country.ToLower();
                foreach (string updatedSys in updated.ValidForToList())
                {
                    bool requiresUpdate = true;
                    foreach (ExchangeRate initial in initalExRates)
                    {
                        if (initial.Country.ToLower() != country) continue;
                        foreach (string initialSys in initial.ValidForToList())
                        {
                            if (initialSys.ToLower() != updatedSys.ToLower()) continue;
                            if (initial.DefaultRate() == updated.DefaultRate()) { requiresUpdate = false; break; }
                        }
                    }
                    if (requiresUpdate)
                    {
                        if (!requiredCountryUpdates.ContainsKey(country)) requiredCountryUpdates.Add(country, new List<Tuple<string, string>>());
                        requiredCountryUpdates[country].Add(new Tuple<string, string>(updatedSys, updated.DefaultRate().ToString()));
                    }
                }
            }
        }

        private void ExchangeRatesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.Cancel && dgvRates.HasChanges() &&
                UserInfoHandler.GetInfo("Are you sure you want to close without saving?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                e.Cancel = true;
        }

        private void btnAddRow_Click(object sender, EventArgs e)
        {
            if (cmbCountryToAdd.Text == string.Empty) { UserInfoHandler.ShowError("Please select a country from the combo-box."); return; }

            bool countryFound = false; DataGridViewRow anker = null;
            foreach (DataGridViewRow row in dgvRates.Rows) // add the row under existing rows of this country, if there are any
            {
                if (!countryFound) { if (row.Cells[colCountry.Name].Value.ToString().ToLower() == cmbCountryToAdd.Text.ToLower()) countryFound = true; }
                else if (row.Cells[colCountry.Name].Value.ToString().ToLower() != cmbCountryToAdd.Text.ToLower()) { anker = row; break; }
            }
            DataRow dataRow;
            bool ok = anker == null ? dgvRates.AddRow(true, out dataRow) : dgvRates.AddRow(true, out dataRow, anker, true);
            if (!ok) { ShowGridLastError(); return; }
            if (!dgvRates.SetCellValue(dataRow, colCountry.Name, cmbCountryToAdd.Text)) { ShowGridLastError(); return; }
            if (!dgvRates.SetCellValue(dataRow, colDefault.Name, ExchangeRate.JUNE30)) { ShowGridLastError(); return; }
        }

        private void dgvRates_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                if (e.ColumnIndex < 0 || e.RowIndex < 0 || dgvRates.Columns[e.ColumnIndex].Name != colValidFor.Name) return; // one needs to request the Name, the index of colValidFor does not work (seems to be 0)

                DataGridViewRow clickedRow = dgvRates.Rows[e.RowIndex]; string country = clickedRow.Cells[colCountry.Index].Value.ToString().ToLower();
                // gether the systems for which rates exist
                List<string> sysDel = new List<string>(), sysMove = new List<string>();
                foreach (DataGridViewRow row in dgvRates.Rows)
                {
                    if (row.Cells[colCountry.Name].Value.ToString().ToLower() != country ||
                        row.Cells[colValidFor.Name].Value == null || row.Cells[colValidFor.Name].Value.ToString() == string.Empty) continue;
                    List<string> sys = ExchangeRate.ValidForToList(row.Cells[colValidFor.Name].Value.ToString());
                    if (row.Index == e.RowIndex) sysDel = sys.ToList(); else sysMove.AddRange(sys);
                }
                // gether the systems for which no rates exist
                List<string> sysAdd = new List<string>();
                CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
                foreach (CountryConfig.SystemRow sys in ccf.GetSystemRows())
                    if (!sysDel.Contains(sys.Name.ToLower()) && !sysMove.Contains(sys.Name.ToLower())) sysAdd.Add(sys.Name.ToLower());

                // construct menu with systems to delete (from the clicked row), systems to move (from other rows of the country), systems to add (not yet existent in any row)
                ContextMenuStrip menu = new ContextMenuStrip(); menu.ShowImageMargin = false; menu.ShowCheckMargin = false;
                if (sysAdd.Count > 0)
                {
                    ToolStripMenuItem menuItemAdd = new ToolStripMenuItem("Add ...");
                    foreach (string sys in sysAdd) menuItemAdd.DropDownItems.Add(sys, null, menuItemAddSys_Click).Tag = clickedRow;
                    menu.Items.Add(menuItemAdd);
                }
                if (sysDel.Count > 0)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem("Delete ...");
                    foreach (string sys in sysDel) menuItem.DropDownItems.Add(sys, null, menuItemDelSys_Click).Tag = clickedRow;
                    menu.Items.Add(menuItem);
                }
                if (sysMove.Count > 0)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem("Move ...");
                    foreach (string sys in sysMove) menuItem.DropDownItems.Add(sys, null, menuItemMoveSys_Click).Tag = clickedRow;
                    menu.Items.Add(menuItem);
                }
                menu.Show(MousePosition);
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            finally { Cursor = Cursors.Default; }
        }

        private bool menuItem_GetActionInfo(object sender, out string sysSelected, out DataGridViewRow rowClicked)
        {
            sysSelected = null; rowClicked = null;
            try
            {
                if (sender == null || !(sender is ToolStripMenuItem)) return false;
                ToolStripItem menuItem = sender as ToolStripItem;
                sysSelected = menuItem.ToString();
                if (menuItem.Tag == null || !(menuItem.Tag is DataGridViewRow)) return false;
                rowClicked = menuItem.Tag as DataGridViewRow;
                return rowClicked != null && rowClicked.Index >= 0;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        private void menuItemAddSys_Click(object sender, EventArgs e = null)
        {
            string system; DataGridViewRow clickedRow; if (!menuItem_GetActionInfo(sender, out system, out clickedRow)) return;
            try
            {
                string systems = clickedRow.Cells[colValidFor.Name].Value == null ? string.Empty : clickedRow.Cells[colValidFor.Name].Value.ToString();
                if (!dgvRates.SetCellValue(clickedRow, colValidFor.Name, ExchangeRate.AddToValidFor(systems, system))) ShowGridLastError();
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private void menuItemDelSys_Click(object sender, EventArgs e = null)
        {
            string system; DataGridViewRow clickedRow; if (!menuItem_GetActionInfo(sender, out system, out clickedRow)) return;
            try
            { // just try to remove the system from any row of this country, thus move can be realised as del+add
                string country = clickedRow.Cells[colCountry.Name].Value.ToString().ToLower();
                foreach (DataGridViewRow row in dgvRates.Rows)
                {
                    if (row.Cells[colCountry.Name].Value.ToString().ToLower() != country || row.Cells[colValidFor.Name].Value == null) continue;
                    bool calledFromMove = e == null; // if called from Move suppress storing undo action, to store it together with the following Add
                    if (!dgvRates.SetCellValue(row, colValidFor.Name,
                                               ExchangeRate.RemoveFromValidFor(row.Cells[colValidFor.Name].Value.ToString(), system),
                                               !calledFromMove)) ShowGridLastError();
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private void menuItemMoveSys_Click(object sender, EventArgs e) { menuItemDelSys_Click(sender); menuItemAddSys_Click(sender); }

        private void btnDeleteRow_Click(object sender, EventArgs e)
        {
            if (dgvRates.SelectedRows == null || dgvRates.SelectedRows.Count == 0) UserInfoHandler.ShowInfo("Please select the (whole) rows you want to remove.");
            else dgvRates.RemoveSelectedRows();
        }
    }
}
