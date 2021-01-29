using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EM_UI.GlobalAdministration
{
    internal partial class ExRatesSyncForm : Form
    {
        internal List<ExchangeRate> ratesSync = new List<ExchangeRate>();
        private List<ExchangeRate> ratesCountry = new List<ExchangeRate>();
        private List<ExchangeRate> ratesGlobal = new List<ExchangeRate>();

        internal ExRatesSyncForm(List<ExchangeRate> ratesCountry, List<ExchangeRatesConfig.ExchangeRatesRow> ratesGlobal)
        {
            InitializeComponent();
            foreach (ExchangeRatesConfig.ExchangeRatesRow r in ratesGlobal) this.ratesGlobal.Add(new ExchangeRate(r));
            this.ratesCountry = ratesCountry;
        }

        internal bool HasDifferences() // find out whether there are differences, if not the dialog does not need to be shown
        {                              // at the same time prepare dialog (for the case that there are differences)
            // remove all 'questionable systems' (systems for which exchange-rate differs) from ratesGlobal
            // and put them into the data-grid (for further treatment by the user)
            // also add exchange-rates from ratesCountry which do not (yet) exist in ratesGlobal to the data-grid
            foreach (ExchangeRate rateCountry in ratesCountry)
            {
                foreach (string system in rateCountry.ValidForToList())
                {
                    ExchangeRate matchingGlobal = null;
                    foreach (ExchangeRate rateGlobal in ratesGlobal)
                    {
                        if (rateCountry.Country.ToLower() != rateGlobal.Country.ToLower()) continue;
                        if (!rateGlobal.ValidForToList().Contains(system)) continue;
                        matchingGlobal = rateGlobal; break;
                    }
                    if (matchingGlobal != null && matchingGlobal.DefaultRate() == rateCountry.DefaultRate()) continue; // exchange-rate exists and is equal
                    if (matchingGlobal == null) // exchange-rate does not (yet) exist in global table
                    {
                        if (rateCountry.DefaultRate() == 1) continue; // euro-country
                        int iRow = dgvDiff.Rows.Add(rateCountry.Country, system, rateCountry.DefaultRate(), "n/a",
                                                    true, false, ExchangeRate.JUNE30, "Rate does not (yet) exist in global table.");
                        DataGridViewCheckBoxCell check = dgvDiff.Rows[iRow].Cells[colTakeGlobal.Index] as DataGridViewCheckBoxCell;
                        check.ReadOnly = true; check.Style.BackColor = Color.LightGray;
                    }
                    else // exchange-rate exists, but is different
                    {
                        string hint = string.Empty;
                        if (matchingGlobal.June30 == rateCountry.DefaultRate()) hint = ExchangeRate.JUNE30;
                        else if (matchingGlobal.YearAverage == rateCountry.DefaultRate()) hint = ExchangeRate.YEARAVERAGE;
                        else if (matchingGlobal.FirstSemester == rateCountry.DefaultRate()) hint = ExchangeRate.FIRSTSEMESTER;
                        else if (matchingGlobal.SecondSemester == rateCountry.DefaultRate()) hint = ExchangeRate.SECONDSEMESTER;
                        if (hint != string.Empty) hint = string.Format("Country exchange rate matches with global table '{0}': consider changing 'Rate Type'.", hint);

                        int iRow = dgvDiff.Rows.Add(matchingGlobal.Country, system, rateCountry.DefaultRate(), matchingGlobal.DefaultRate(),
                                                    false, true, matchingGlobal.Default, hint);
                        dgvDiff.Rows[iRow].Tag = matchingGlobal;
                        matchingGlobal.RemoveFromValidFor(system); // remove the system (to add on OK in the way user choses)
                    }
                }
            }
            return dgvDiff.Rows.Count > 0;
        }

        private void dgvDiff_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                if (dgvDiff.CurrentCell.ColumnIndex != colDefault.Index || !(e.Control is ComboBox)) return;
                ComboBox comboBox = e.Control as ComboBox;
                comboBox.SelectedIndexChanged += comboBox_SelectedIndexChanged;
            }
            catch { }
        }

        void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvDiff.CurrentRow.Tag == null) return; // no global row available (i.e. exchange rate only exists in country file)
                ExchangeRate row = dgvDiff.CurrentRow.Tag as ExchangeRate;
                double rate = -1;
                switch ((sender as DataGridViewComboBoxEditingControl).EditingControlFormattedValue.ToString())
                {
                    case ExchangeRate.JUNE30: rate = row.June30; break;
                    case ExchangeRate.YEARAVERAGE: rate = row.YearAverage; break;
                    case ExchangeRate.FIRSTSEMESTER: rate = row.FirstSemester; break;
                    case ExchangeRate.SECONDSEMESTER: rate = row.SecondSemester; break;
                }

                dgvDiff.CurrentRow.Cells[colRateGlobal.Index].Value = (rate == -1) ? "n/a" : rate.ToString();

                // adapt possibility to 'Take Global', i.e. is disabled if not global value exists for this rate
                DataGridViewCheckBoxCell chkTakeGlobal = dgvDiff.CurrentRow.Cells[colTakeGlobal.Index] as DataGridViewCheckBoxCell;
                chkTakeGlobal.ReadOnly = rate == -1; chkTakeGlobal.Style.BackColor = (rate == -1) ? Color.LightGray : Color.White;
                if (rate == -1)
                {
                    DataGridViewCheckBoxCell chkTakeCountry = dgvDiff.CurrentRow.Cells[colTakeCountry.Index] as DataGridViewCheckBoxCell;
                    chkTakeGlobal.Value = false; chkTakeCountry.Value = true;
                }
            }
            catch { }
        }

        private void dgvDiff_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;
                int clickedCol, otherCol;
                if (e.ColumnIndex == colTakeCountry.Index) { clickedCol = colTakeCountry.Index; otherCol = colTakeGlobal.Index; }
                else if (e.ColumnIndex == colTakeGlobal.Index) { clickedCol = colTakeGlobal.Index; otherCol = colTakeCountry.Index; }
                else return;

                if (!dgvDiff.Rows[e.RowIndex].Cells[clickedCol].ReadOnly && dgvDiff.Rows[e.RowIndex].Cells[clickedCol].Value.ToString() == false.ToString())
                    dgvDiff.Rows[e.RowIndex].Cells[otherCol].Value = false;
            }
            catch { }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                // first overtake all current rows of the global table and check if they need adaptation due to grid-view-entries ...
                List<int> done = new List<int>();
                Dictionary<string, List<Tuple<string, string>>> changesCountryFiles = new Dictionary<string, List<Tuple<string, string>>>();
                foreach (ExchangeRate rateGlobal in ratesGlobal)
                {
                    string country = rateGlobal.Country.ToLower();
                    foreach (DataGridViewRow rowSync in dgvDiff.Rows)
                    {
                        if (done.Contains(rowSync.Index) ||
                            country != rowSync.Cells[colCountry.Index].Value.ToString().ToLower() ||
                            rateGlobal.Default != rowSync.Cells[colDefault.Index].Value.ToString()) continue;

                        bool takeCountry;
                        if (rowSync.Cells[colTakeCountry.Index].Value.ToString() == true.ToString()) takeCountry = true;
                        else if (rowSync.Cells[colTakeGlobal.Index].Value.ToString() == true.ToString()) takeCountry = false;
                        else continue;

                        string rate = rowSync.Cells[takeCountry ? colRateCountry.Index : colRateGlobal.Index].Value.ToString();
                        if (rateGlobal.DefaultRate().ToString() != rate) continue;

                        string system = rowSync.Cells[colSystem.Index].Value.ToString();
                        rateGlobal.AddToValidFor(system);
                        done.Add(rowSync.Index);

                        // memorise which systems need to be changed in the country files (to do so below)
                        if (!takeCountry &&
                            rate.ToString() != rowSync.Cells[colRateCountry.Index].Value.ToString()) // it is still possible that the country file does not need to be changed if the period (June 30, ...) was changed
                        {
                            if (!changesCountryFiles.ContainsKey(country)) changesCountryFiles.Add(country, new List<Tuple<string, string>>());
                            changesCountryFiles[country].Add(new Tuple<string, string>(system, rate));
                        }
                    }
                    ratesSync.Add(rateGlobal);
                }

                // ... then add new rows to global table, for those grid-view-entries which do not fit into existing global table rows ...
                foreach (DataGridViewRow rowSync in dgvDiff.Rows)
                {
                    if (done.Contains(rowSync.Index)) continue;
                    
                    bool takeCountry;
                    if (rowSync.Cells[colTakeCountry.Index].Value.ToString() == true.ToString()) takeCountry = true;
                    else if (rowSync.Cells[colTakeGlobal.Index].Value.ToString() == true.ToString()) takeCountry = false;
                    else continue;

                    double june30 = -1, yearAverage = -1, firstSemester = -1, secondSemester = -1;
                    if (rowSync.Tag != null)
                    {
                        ExchangeRate gr = rowSync.Tag as ExchangeRate;
                        june30 = gr.June30; yearAverage = gr.YearAverage; firstSemester = gr.FirstSemester; secondSemester = gr.SecondSemester;
                    }
                    string def = rowSync.Cells[colDefault.Index].Value.ToString();
                    double rate = double.Parse(rowSync.Cells[takeCountry ? colRateCountry.Index : colRateGlobal.Index].Value.ToString());
                    ratesSync.Add(new ExchangeRate() { Country = rowSync.Cells[colCountry.Index].Value.ToString().ToUpper(),
                                                       Default = def,
                                                       ValidFor = rowSync.Cells[colSystem.Index].Value.ToString().ToLower(),
                                                       June30 = def == ExchangeRate.JUNE30 ? rate : june30,
                                                       YearAverage = def == ExchangeRate.YEARAVERAGE ? rate : yearAverage,
                                                       FirstSemester = def == ExchangeRate.FIRSTSEMESTER ? rate : firstSemester,
                                                       SecondSemester = def == ExchangeRate.SECONDSEMESTER ? rate : secondSemester 
                                                     });
                }

                // ... finally remove global tabel rows which got useless (i.e. are not valid for any system anymore)
                for (int del = ratesSync.Count - 1; del >= 0; --del) if (ratesSync[del].ValidFor.Trim() == string.Empty) ratesSync.RemoveAt(del);

                // finally, finally change country files where necessary (take global chosen) 
                UpdateCountryFiles(changesCountryFiles, this);

                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
        }

        internal static void UpdateCountryFiles(Dictionary<string, List<Tuple<string, string>>> changesCountryFiles, Form showWaitCursorForm = null)
        {
            try
            {
                if (showWaitCursorForm == null) showWaitCursorForm = EM_AppContext.Instance.GetActiveCountryMainForm();
                showWaitCursorForm.Cursor = Cursors.WaitCursor;
                bool showOpenWarning = true;
                foreach (var ccChange in changesCountryFiles)
                {
                    string country = ccChange.Key;
                    EM_UI_MainForm ccMainForm = EM_AppContext.Instance.GetCountryMainForm(country);
                    if (ccMainForm != null)
                    {
                        if (showOpenWarning)
                        {
                            switch (UserInfoHandler.GetInfo(country.ToUpper() + " is open." + Environment.NewLine +
                                    "Updating exchange rates requires a reset of the undo-functionality." + Environment.NewLine +
                                    "That means you will not be able to undo any of your changes so far." + Environment.NewLine + Environment.NewLine +
                                    "Do you still want to update exchange rates of " + country.ToUpper() + "?" + Environment.NewLine + Environment.NewLine +
                                    "(Press Cancel to not be requested with respect to further open countries.)", MessageBoxButtons.YesNoCancel))
                            {
                                case DialogResult.No: continue;
                                case DialogResult.Cancel: showOpenWarning = false; break;
                            }
                        }
                        ccMainForm.GetUndoManager().Commit(); ccMainForm.GetUndoManager().Reset();
                    }
                    CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
                    foreach (var change in ccChange.Value) ccf.GetSystemRowByName(change.Item1).ExchangeRateEuro = change.Item2;

                    if (ccMainForm != null) { ccMainForm.GetUndoManager().Commit(); ccMainForm.GetUndoManager().Reset(); ccMainForm.WriteXml(); }
                    else CountryAdministrator.WriteXML(country);
                }
            }
            catch (Exception)
            {
            }
            finally { showWaitCursorForm.Cursor = Cursors.Default; }
        }
    }
}
