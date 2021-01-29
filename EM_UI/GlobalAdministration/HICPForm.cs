using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.GlobalAdministration
{
    internal partial class HICPForm : Form
    {
        private const string colID = "ID";
        private const string colCountry = "Country";
        private const string colComment = "Comment";
        internal List<Tuple<string, int, double, string>> updatedHICPs = null;

        internal HICPForm(List<HICPConfig.HICPRow> hicps)
        {
            InitializeComponent();

            try
            {
                if (!dgvHICP.InitDataSource()) { ShowGridLastError(); return; }

                if (!dgvHICP.AddColumn(false, colCountry, typeof(string))) { ShowGridLastError(); return; }

                List<string> countries = (from c in CountryAdministrator.GetCountries() select c._shortName.ToUpper()).ToList();
                List<int> years = (from y in hicps select y.Year).Distinct().ToList(); years.Sort();
                foreach (int year in years) if (!AddYear(year)) return;

                if (!dgvHICP.AddColumn(false, colComment, typeof(string))) { ShowGridLastError(); return; }
                dgvHICP.pasteableColumns.Add(colComment);
                if (!dgvHICP.AddColumn(false, colID, typeof(Int16), -1, true)) { ShowGridLastError(); return; }

                Dictionary<string, DataRow> countryRowIndex = new Dictionary<string, DataRow>();
                foreach (string country in countries)
                {
                    DataRow dataRow; if (!AddCountryRow(out dataRow, years.Count, country)) return;
                    countryRowIndex.Add(country, dataRow);
                }

                foreach (HICPConfig.HICPRow hicp in hicps)
                {
                    string country = hicp.Country.ToUpper();
                    if (!countryRowIndex.ContainsKey(country)) // a country that does not belong to the countries of the loaded version (and is not yet added to the table)
                    {
                        DataRow dataRow; if (!AddCountryRow(out dataRow, years.Count, hicp.Country, hicp.Comment)) return;
                        countryRowIndex.Add(country, dataRow);
                        cmbCountryToDelete.Items.Add(country); // only allow deleting rows for countries which are not part of the loaded version (i.e. are likely to be just fake or error)
                    }
                    dgvHICP.SetCellValue(countryRowIndex[country], hicp.Year.ToString(), hicp.Value);
                    dgvHICP.SetCellValue(countryRowIndex[country], colComment, hicp.Comment);
                }
                FormatGridViewColumns();

                cmbCountryToDelete.Visible = cmbCountryToDelete.Items.Count > 0; // only show the option to delete a country-row in the unlikely case that there are "unknown" countries
                btnDeleteCountryRow.Visible = cmbCountryToDelete.Items.Count > 0;

                if (!dgvHICP.StartUndoManager()) ShowGridLastError();
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private void ShowGridLastError() { if (dgvHICP.lastError != string.Empty) UserInfoHandler.ShowError(dgvHICP.lastError); }

        private void FormatGridViewColumns(string yearColName = "")
        {
            if (yearColName == string.Empty)
            {
                DataGridViewColumn col = dgvHICP.Columns[colID];
                col.Visible = false;

                col = dgvHICP.Columns[colCountry];
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.ReadOnly = true;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                
                col = dgvHICP.Columns[colComment];
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleLeft;
                col.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;

            }
            else
            {
                DataGridViewColumn col = dgvHICP.Columns[yearColName];
                col.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                col.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        private bool AddCountryRow(out DataRow dataRow, int cntYears, string country, string comment = "")
        {
            if (!dgvHICP.AddRow(false, out dataRow)) { ShowGridLastError(); return false; }
            if (!dgvHICP.SetCellValue(dataRow, colCountry, country)) { ShowGridLastError(); return false; }
            if (!dgvHICP.SetCellValue(dataRow, colComment, comment)) { ShowGridLastError(); return false; }
            return true;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (!dgvHICP.verifyNumericValues(out string problems))
            {
                UserInfoHandler.ShowError("There are cells with invalid values!" + Environment.NewLine + problems);
                return;
            }

            updatedHICPs = new List<Tuple<string, int, double, string>>();
            DataTable dtHICP = dgvHICP.GetDataTable();
            foreach (DataRow r in dtHICP.Rows)
            {
                foreach (DataColumn c in dtHICP.Columns)
                {
                    if (c.Caption == colCountry || c.Caption == colComment || c.Caption == colID || r.IsNull(c.Caption)) continue;
                    try
                    {
                        updatedHICPs.Add(new Tuple<string, int, double, string>(
                            r.Field<string>(colCountry),    // country
                            Convert.ToInt32(c.Caption),     // year
                            double.Parse(r[c.Caption].ToString()),     // value
                            r.Field<string>(colComment)));  // comment
                    }
                    catch (Exception exception) { UserInfoHandler.ShowException(exception, $"{r.Field<string>(colCountry)} / {c.Caption}", false); return; }
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnAddYear_Click(object sender, EventArgs e) { AddYear(); }
        private bool AddYear(int year = -1)
        {
            try
            {
                bool showWarning = false;
                if (year == -1)
                {
                    showWarning = true;
                    year = Convert.ToInt32(updwnYearToAdd.Value);
                    if (GetExistingYears().Contains(year)) { UserInfoHandler.ShowError(year + " already exits."); return false; }
                }
                int preCount = GetExistingYears().Where(y => y < year).ToList().Count();

                if (!dgvHICP.AddColumn(showWarning, year.ToString(), typeof(double), preCount + 1)) { ShowGridLastError(); return false; }
                dgvHICP.pasteableColumns.Add(year.ToString());
                updwnYearToAdd.Value = year + 1;
                cmbYearToDelete.Items.Add(year);

                FormatGridViewColumns(year.ToString());
                return true;
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); return false; }
        }

        private void btnDeleteYear_Click(object sender, EventArgs e)
        {
            if (cmbYearToDelete.Text == string.Empty) { UserInfoHandler.ShowError("Please select a year."); return; }
            try
            {
                if (!dgvHICP.RemoveColumn(true, cmbYearToDelete.Text)) ShowGridLastError();
                else cmbYearToDelete.Items.Remove(cmbYearToDelete.SelectedItem);
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private List<int> GetExistingYears()
        {
            List<int> existingYears = new List<int>();
            foreach (int year in cmbYearToDelete.Items) existingYears.Add(year);
            existingYears.Sort();
            return existingYears;
        }

        private void btnDeleteCountryRow_Click(object sender, EventArgs e)
        {
            if (cmbCountryToDelete.Text == string.Empty) { UserInfoHandler.ShowError("Please select a country."); return; }
            try
            {
                if (!dgvHICP.RemoveRow(true, colCountry, cmbCountryToDelete.Text)) ShowGridLastError();
                else cmbCountryToDelete.Items.Remove(cmbCountryToDelete.SelectedItem);
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        private void HICPForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult == System.Windows.Forms.DialogResult.Cancel && dgvHICP.HasChanges() &&
                UserInfoHandler.GetInfo("Are you sure you want to close without saving?", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.No)
                e.Cancel = true;
        }
    }
}
