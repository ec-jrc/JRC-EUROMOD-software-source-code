using EM_Common_Win;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    internal partial class StatisticsWizardForm : Form
    {
        private Program plugin;
        private InputForm inputForm;
        private List<string> countries, years;
        private BackgroundWorker backgroundWorker = null;
        private DataGenerator dataGenerator;
        private DataGenerator.AllFileGenerationDetails generationDetails;
        private string tempPathData = Path.Combine(UISessionInfo.GetOutputFolder(), "HHoTData");
        private List<string> generatedOutputFiles = null;
        private bool saveSettings = true;

        private class BackgroundWorkerResult
        {
            internal List<string> generatedOutputFiles = new List<string>();
            internal Dictionary<string, List<string>> errors = new Dictionary<string, List<string>>(); // key: country+year, value: errors
        }

        internal StatisticsWizardForm(Program _plugin, InputForm _inputForm)
        {
            InitializeComponent();

            saveSettings = false;
            plugin = _plugin; inputForm = _inputForm;
            countriesCheckedComboBoxEdit.Properties.DataSource = plugin.settingsData.Cur_Countries;
            yearsCheckedComboBoxEdit.Properties.DataSource = plugin.settingsData.Cur_Years;
            countriesCheckedComboBoxEdit.EditValue = inputForm.countriesCheckedComboBoxEdit.EditValue;
            yearsCheckedComboBoxEdit.EditValue = inputForm.yearsCheckedComboBoxEdit.EditValue;

            foreach (var hhType in GetInfoHHTypes(true))
            {
                listHHType_MultiSelect.Items.Add(hhType.Value);
                listHHType_SingleSelect.Items.Add(hhType.Value);
            }

            saveSettings = true;
        }

        private void LoadSettings()
        {
            txtRangeFrom.Text = plugin.userSettings.settings.wizardSettings.rangeSettings.rangeValueFrom;
            txtRangeTo.Text = plugin.userSettings.settings.wizardSettings.rangeSettings.rangeValueTo;
            txtRangeStep.Text = plugin.userSettings.settings.wizardSettings.rangeSettings.rangeValueStep;
            txtFixedValue.Text = plugin.userSettings.settings.wizardSettings.rangeSettings.fixedValue;

            switch (plugin.userSettings.settings.wizardSettings.rangeSettings.rangeType)
            {
                case Program.HHoT_INCOME_RANGE.RANGE_HOURS_INCOME:
                    radioRangeHoursFixedWage.Checked = true; break;
                case Program.HHoT_INCOME_RANGE.RANGE_INCOME_HOURS:
                    radioRangeIncomeFixedWage.Checked = true; break;
                case Program.HHoT_INCOME_RANGE.RANGE_INCOME_WAGE:
                    radioRangeIncomeFixedHours.Checked = true; break;
                case Program.HHoT_INCOME_RANGE.RANGE_WAGE_INCOME:
                    radioRangeWageFixedHours.Checked = true; break;
            }

            switch (plugin.userSettings.settings.wizardSettings.templateType)
            {
                case Program.HHoT_TEMPLATE_TYPE.BUDGET_CONSTRAINTS:
                    radioBudget.Checked = true;
                    for (int i = 0; i < listHHType_SingleSelect.Items.Count; ++i)
                        if (plugin.userSettings.settings.wizardSettings.householdTypes.Contains(listHHType_SingleSelect.Items[i]))
                            { listHHType_SingleSelect.SetItemChecked(i, true); break; } // there ought to be only one
                    if (listHHType_SingleSelect.CheckedItems.Count == 0 && listHHType_SingleSelect.Items.Count > 0) listHHType_SingleSelect.SetItemChecked(0, true);
                    break;
                case Program.HHoT_TEMPLATE_TYPE.BREAKDOWN_COUNTRY_YEAR:
                    radioCountryYear.Checked = true;
                    for (int i = 0; i < listHHType_MultiSelect.Items.Count; ++i)
                        if (plugin.userSettings.settings.wizardSettings.householdTypes.Contains(listHHType_MultiSelect.Items[i]))
                            listHHType_MultiSelect.SetItemChecked(i, true);
                    break;
                case Program.HHoT_TEMPLATE_TYPE.BREAKDOWN_HH_TYPES:
                    radioTypes.Checked = true; break;
            }
            statisticRadioChanged(null, null); // en/disable controls, etc.

            if (comboPerson.Items.Contains(plugin.userSettings.settings.wizardSettings.householdMember))
                comboPerson.Text = plugin.userSettings.settings.wizardSettings.householdMember;
            if (comboPerson.SelectedIndex < 0 & comboPerson.Items.Count > 0) comboPerson.SelectedIndex = 0;

            chkOutputInEuro.Checked = plugin.userSettings.settings.wizardSettings.outputInEuro;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            string msg = string.Empty;
            if (!radioTypes.Checked && GetSelectedHHTypes().Count == 0) msg = "Please select a Household Type" + Environment.NewLine;

            if (radioBudget.Checked)
            {
                if (comboPerson.SelectedIndex < 0) msg += "Please select a Person in HH" + Environment.NewLine;

                double from = double.MinValue, to = double.MinValue, step = double.MinValue, fix = double.MinValue; 
                if (!double.TryParse(txtRangeFrom.Text, out from)) msg += $"'{labFrom.Text}' is not a valid number" + Environment.NewLine;
                if (!double.TryParse(txtRangeTo.Text, out to)) msg += $"'to' is not a valid number" + Environment.NewLine;
                if (!double.TryParse(txtRangeStep.Text, out step)) msg += $"'Step' is not a valid number" + Environment.NewLine;
                if (!double.TryParse(txtFixedValue.Text, out fix)) msg += $"'{labFixedVal.Text}' is not a valid number" + Environment.NewLine;

                if (msg == string.Empty)
                {
                    if (from < 0) msg += $"'{labFrom.Text}' must not be negative" + Environment.NewLine;
                    if (to < 0) msg += $"'to' must not be negative" + Environment.NewLine;
                    if (step <= 0) msg += $"'step' must be positive" + Environment.NewLine;
                    if (fix <= 0) msg += $"'{labFixedVal.Text}' must be positive" + Environment.NewLine;

                    if (msg == string.Empty)
                    {
                        int z = 0, maxRange = 3000, minRange = 10;
                        for (double d = from; d <= to; d += step) { ++z; if (z > maxRange) break; }
                        if (z == 0) msg += $"'{labFrom.Text}/to/Step' does not produce any number" + Environment.NewLine;
                        if (z < minRange) msg += $"'{labFrom.Text}/to/Step' produces less than {minRange} numbers" + Environment.NewLine;
                        if (z > maxRange) msg += $"'{labFrom.Text}/to/Step' produces more than {maxRange} numbers" + Environment.NewLine;
                    }
                }
            }

            countries = (from cv in inputForm.countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues() select cv as string).ToList();
            years = (from cv in inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues() select cv as string).ToList();

            if (countries.Count < 1) msg += "You need to select at least one Country.\n";
            if (years.Count < 1) msg += "You need to select at least one Year.\n";

            if (msg == string.Empty) GenerateStatistics(); 
            else MessageBox.Show(msg, "Errors found");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (backgroundWorker != null && backgroundWorker.IsBusy) backgroundWorker.CancelAsync();
            else Close();
        }

        private WorktimeRangeAdmin PrepareWorktimeRangeAdmin()
        {
            if (!radioBudget.Checked) // for charts per scenario and per HH-type just avoid ranges
                return new WorktimeRangeAdmin(WorktimeRangeAdmin.RangeType.None, true, years, radioCountryYear.Checked ? GetSelectedHHTypes() : null);

            WorktimeRangeAdmin.RangeType t = WorktimeRangeAdmin.RangeType.None;
            if (radioRangeHoursFixedWage.Checked) t = WorktimeRangeAdmin.RangeType.RangeHours_FixWage;
            else if (radioRangeWageFixedHours.Checked) t = WorktimeRangeAdmin.RangeType.RangeWage_FixHours;
            else if (radioRangeIncomeFixedHours.Checked) t = WorktimeRangeAdmin.RangeType.RangeInc_FixHours;
            else if (radioRangeIncomeFixedWage.Checked) t = WorktimeRangeAdmin.RangeType.RangeInc_FixWage;
            WorktimeRangeAdmin worktimeRangeAdmin = new WorktimeRangeAdmin(t, true, years, GetSelectedHHTypes());

            foreach (var hhType in GetInfoHHTypes()) // find the HH-type ...
            {
                int hhTypeId = hhType.Key; string hhTypeName = hhType.Value;
                foreach (string pName in GetHHTypePersonNames(hhTypeId)) // ... and person ...
                {
                    if (hhTypeName == GetSelectedHHTypes().First() && pName == comboPerson.Text)
                    {
                        worktimeRangeAdmin.AddPersonWorktimeRange(hhTypeName, pName, new WorktimeRange() // ... for whom to range income
                        {
                            fixValue = double.Parse(txtFixedValue.Text),
                            rangeFrom = double.Parse(txtRangeFrom.Text),
                            rangeTo = double.Parse(txtRangeTo.Text),
                            rangeStep = double.Parse(txtRangeStep.Text)
                        });
                    }
                }
            }
            return worktimeRangeAdmin;
        }

        private void GenerateStatistics()
        {
            try
            {
                dataGenerator = new DataGenerator(inputForm, plugin); WorktimeRangeAdmin worktimeRangeAdmin = PrepareWorktimeRangeAdmin();
                if (!dataGenerator.getAllFileDetails(out generationDetails, out List<string> errors, worktimeRangeAdmin))
                    throw new Exception(String.Join("\n", errors));

                if (worktimeRangeAdmin.HasReducedRanges(out List<string> rangeAvertInfo) &&
                    MessageBox.Show("The following range-variables were reduced to their average value:" + Environment.NewLine +
                    String.Join(Environment.NewLine, rangeAvertInfo), string.Empty, MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;

                if (!Directory.Exists(tempPathData)) Directory.CreateDirectory(tempPathData);

                btnStart.Enabled = false; Refresh();
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.WorkerSupportsCancellation = true;
                backgroundWorker.WorkerReportsProgress = true;
                backgroundWorker.DoWork += backgroundWorker_DoWork;
                backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
                backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
                backgroundWorker.RunWorkerAsync();
            }
            catch (Exception e)
            {
                MessageBox.Show("Statistics generation aborted with errors:" + Environment.NewLine + e.Message);
            }
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            labProgress.Text = e.UserState.ToString();
            labProgress.Refresh();
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorkerResult result = new BackgroundWorkerResult();
            try
            {
                BackgroundWorker bw = sender as BackgroundWorker;

                bw.ReportProgress(0, $"Translating to EM3 format ...");
                new ExeCaller().RunTransfomer(countries, out List<string> errors); AddErrors(errors, result);
                if (bw.CancellationPending) { e.Cancel = true; return; }

                foreach (string country in countries)
                {
                    foreach (string year in years)
                    {
                        if (!GenerateHHotData(country, year, out string dataFullPath, result)) continue;
                        if (bw.CancellationPending) break;
                        GenerateOutput(country, year, dataFullPath, result);
                        if (bw.CancellationPending) break;
                    }
                    if (bw.CancellationPending) break;
                }
                if (bw.CancellationPending) e.Cancel = true;
            }
            catch (Exception exception)
            {
                AddErrors(new List<string>() { exception.Message }, result);
            }
            e.Result = result;
        }

        private void GenerateOutput(string country, string year, string dataFullPath, BackgroundWorkerResult result)
        {
            backgroundWorker.ReportProgress(0, $"Generating output for {country} {year} ...");

            bool success = new ExeCaller().RunExe(country, year, dataFullPath, chkOutputInEuro.Checked, out List<string> errors);
            AddErrors(errors, result, country, year);

            string outputFile = Path.Combine(UISessionInfo.GetOutputFolder(), $"{country}_{year}_std.txt");
            if (success && File.Exists(outputFile)) result.generatedOutputFiles.Add(outputFile);
        }

        private bool GenerateHHotData(string country, string year, out string dataFullPath, BackgroundWorkerResult bwResult)
        {
            backgroundWorker.ReportProgress(0, $"Generating data for {country} {year} ...");
            dataFullPath = null; List<string> err = new List<string>();

            string dataName = dataGenerator.generateCountryYearData(tempPathData, country, year, generationDetails.fileDetails[country], err);
            AddErrors(err, bwResult, country, year);

            if (!string.IsNullOrEmpty(dataName) && File.Exists(Path.Combine(tempPathData, dataName)))
                dataFullPath = Path.Combine(tempPathData, dataName);

            return dataFullPath != null;
        }

        private void AddErrors(List<string> errors, BackgroundWorkerResult bwResult, string country = null, string year = null)
        {
            if (errors == null || errors.Count == 0) return;
            string key = $"{country ?? "General"} {year ?? string.Empty}".Trim();
            if (!bwResult.errors.ContainsKey(key)) bwResult.errors.Add(key, new List<string>());
            bwResult.errors[key].AddRange(errors);
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            CleanUp();
            if (!e.Cancelled)
            {
                BackgroundWorkerResult result = e.Result as BackgroundWorkerResult;
                ReportErrors(result.generatedOutputFiles.Count > 0, result.errors);
                if (result.generatedOutputFiles.Count == 0) return;
                generatedOutputFiles = result.generatedOutputFiles;
                Close();
            }
            else MessageBox.Show("Generation of Statistics was cancelled.");
        }

        internal void StartPresenter()
        {
            if (generatedOutputFiles == null || generatedOutputFiles.Count == 0) return;

            try
            {
                Dictionary<int, string> hhTypes = GetInfoHHTypes();
                if (radioTypes.Checked) SimpleStatistics.BreakDownHHTypes(hhTypes, generatedOutputFiles);
                else
                {
                    // listHHType contains only the names of the hh-types, thus we need to get the corresponding ids
                    Dictionary<int, string> selHHTypes = new Dictionary<int, string>();
                    foreach (var hht in hhTypes) if (GetSelectedHHTypes().Contains(hht.Value)) selHHTypes.Add(hht.Key, hht.Value);

                    if (radioBudget.Checked) SimpleStatistics.BudgetConstraints(selHHTypes, generatedOutputFiles);
                    if (radioCountryYear.Checked) SimpleStatistics.BreakDownCountryYear(selHHTypes, generatedOutputFiles);
                }
            }
            catch (Exception e) { MessageBox.Show($"Problems with starting Statistics Prenter:{Environment.NewLine + e.Message}"); }
        }

        private void ReportErrors(bool anySuccess, Dictionary<string, List<string>> errors) // could be handled differently, e.g. written to log-file
        {
            if (anySuccess && (errors == null || errors.Count == 0)) return;

            string msg = !anySuccess ? "Failed to generate Statistics!" + Environment.NewLine : string.Empty;
            msg += "The following errors were found:" + Environment.NewLine;
            foreach (var e in errors)
            {
                msg += e.Key.ToUpper() +  Environment.NewLine;
                foreach (string err in e.Value) msg += "   " + err + Environment.NewLine;
            }               
            MessageBox.Show(msg);
        }

        private void CleanUp()
        {
            try
            {
                backgroundWorker = null;
                labProgress.Text = string.Empty;
                btnStart.Enabled = true;
                if (Directory.Exists(tempPathData))
                {
                    foreach (string f in Directory.GetFiles(tempPathData)) File.Delete(f);
                    Directory.Delete(tempPathData);
                }
            }
            catch { }
        }

        private void StatisticsWizardForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backgroundWorker != null && backgroundWorker.IsBusy) backgroundWorker.CancelAsync();
        }

        List<string> GetHHTypePersonNames(int hhTypeId)
        {
            return (from DataRow r in plugin.householdData.Tables[plugin.HOUSEHOLD_STRUCTURE_TABLE].Rows
                    where r.Field<int>("ParentID") == hhTypeId
                    select r.Field<string>("HouseholdName")).ToList();
        }

        private void budgetConstraintsRangeType_CheckedChanged(object sender, EventArgs e)
        {
            labFrom.Text = $"{(radioRangeHoursFixedWage.Checked ? "Hours" : (radioRangeWageFixedHours.Checked ? "Wage" : "Income"))} from";
            labFixedVal.Text = radioRangeIncomeFixedHours.Checked | radioRangeWageFixedHours.Checked ? "Hours" : "Wage";
            plugin.userSettings.settings.wizardSettings.rangeSettings.rangeType = radioRangeHoursFixedWage.Checked ? Program.HHoT_INCOME_RANGE.RANGE_HOURS_INCOME :
                                                                                  radioRangeWageFixedHours.Checked ? Program.HHoT_INCOME_RANGE.RANGE_WAGE_INCOME :
                                                                                  radioRangeIncomeFixedHours.Checked ? Program.HHoT_INCOME_RANGE.RANGE_INCOME_WAGE :
                                                                                  Program.HHoT_INCOME_RANGE.RANGE_INCOME_HOURS;
        }

        private void txtRangeFrom_TextChanged(object sender, EventArgs e)
        {
            plugin.userSettings.settings.wizardSettings.rangeSettings.rangeValueFrom = txtRangeFrom.Text;
        }

        private void txtRangeTo_TextChanged(object sender, EventArgs e)
        {
            plugin.userSettings.settings.wizardSettings.rangeSettings.rangeValueTo = txtRangeTo.Text;
        }

        private void txtRangeStep_TextChanged(object sender, EventArgs e)
        {
            plugin.userSettings.settings.wizardSettings.rangeSettings.rangeValueStep = txtRangeStep.Text;
        }

        private void txtFixedValue_TextChanged(object sender, EventArgs e)
        {
            plugin.userSettings.settings.wizardSettings.rangeSettings.fixedValue = txtFixedValue.Text;
        }

        private void comboPerson_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (saveSettings) plugin.userSettings.settings.wizardSettings.householdMember = comboPerson.Text;
        }

        private void chkOutputInEuro_CheckedChanged(object sender, EventArgs e)
        {
            plugin.userSettings.settings.wizardSettings.outputInEuro = chkOutputInEuro.Checked;
        }

        private void countriesCheckedComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            string selection = countriesCheckedComboBoxEdit.EditValue.ToString();
            labelSelectedCountries.Text = (selection == "") ? "No countries selected!" : "Selected countries:\n" + selection;
            if (saveSettings) plugin.userSettings.settings.selectedCounries = selection;
            inputForm.countriesCheckedComboBoxEdit.SetEditValue(countriesCheckedComboBoxEdit.EditValue);
            inputForm.countriesCheckedComboBoxEdit_EditValueChanged(null, null);
        }

        private void yearsCheckedComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            string selection = yearsCheckedComboBoxEdit.EditValue.ToString();
            labelSelectedYears.Text = (selection == "") ? "No years selected!" : "Selected years:\n" + selection;
            if (saveSettings) plugin.userSettings.settings.selectedYears = selection;
            inputForm.yearsCheckedComboBoxEdit.SetEditValue(yearsCheckedComboBoxEdit.EditValue);
            inputForm.yearsCheckedComboBoxEdit_EditValueChanged(null, null);
        }

        private void StatisticsWizardForm_Shown(object sender, EventArgs e)
        {
            saveSettings = false;
            LoadSettings();
            saveSettings = true;
        }

        private void statisticRadioChanged(object sender, System.EventArgs e)
        {
            groupBoxBudget.Enabled = radioBudget.Checked;

            // (a) for 'Break Down Country/Years' several hh-types can be selected, thus use a "normal" multi-select CheckedListBox
            // (b) for 'Budget Constraints' only one hh-types can be selected (otherwise it would be difficult to define the "ranging" person), thus use a "manipulated" single-select CheckedListBox (see listHHType_SingleSelect_ItemCheck)
            // (c) for 'Break Down HH Types' the user does not need to select hh-types, thus disable either box (the user still can un-check hh-types in the main window, if required)
            listHHType_SingleSelect.Visible = radioBudget.Checked;
            listHHType_MultiSelect.Visible = !radioBudget.Checked;
            listHHType_MultiSelect.Enabled = radioCountryYear.Checked;

            if (radioBudget.Checked) FillComboPersons(); // make sure the content of comboPerson is correct

            if (saveSettings) plugin.userSettings.settings.wizardSettings.templateType =
                                    radioBudget.Checked ? Program.HHoT_TEMPLATE_TYPE.BUDGET_CONSTRAINTS :
                                    radioTypes.Checked ? Program.HHoT_TEMPLATE_TYPE.BREAKDOWN_HH_TYPES :
                                    Program.HHoT_TEMPLATE_TYPE.BREAKDOWN_COUNTRY_YEAR;
        }


        private int lastCheckedItem = -1;
        private void listHHType_SingleSelect_ItemCheck(object sender, ItemCheckEventArgs e) // make list single-select
        {
            if (e.CurrentValue != CheckState.Checked)
                if (lastCheckedItem != -1)
                    listHHType_SingleSelect.SetItemChecked(lastCheckedItem, false);
            lastCheckedItem = e.Index;
        }

        private void listHHType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!saveSettings) return;
            plugin.userSettings.settings.wizardSettings.householdTypes.Clear();
            foreach (var item in (sender as CheckedListBox).CheckedItems) plugin.userSettings.settings.wizardSettings.householdTypes.Add(item.ToString());
            if (radioBudget.Checked) FillComboPersons();
        }

        private List<string> GetSelectedHHTypes()
        {
            List<string> selectedHHTypes = new List<string>();
            if (radioBudget.Checked) foreach (var ci in listHHType_SingleSelect.CheckedItems) selectedHHTypes.Add(ci.ToString()); // that should be one
            if (radioCountryYear.Checked) foreach (var ci in listHHType_MultiSelect.CheckedItems) selectedHHTypes.Add(ci.ToString());
            return selectedHHTypes;
        }

        void FillComboPersons()
        {
            comboPerson.Items.Clear();

            if (listHHType_SingleSelect.CheckedItems.Count == 0) return;
            int hhTypeId = (from t in GetInfoHHTypes()
                            where t.Value == listHHType_SingleSelect.CheckedItems[0].ToString()
                            select t.Key).First();
            foreach (string p in GetHHTypePersonNames(hhTypeId)) comboPerson.Items.Add(p);
            if (comboPerson.Items.Count > 0) comboPerson.SelectedIndex = 0;
        }

        private Dictionary<int, string> GetInfoHHTypes(bool checkedOnly = false)
        {
            Dictionary<int, string> hhTypes = new Dictionary<int, string>();
            foreach (DataRow row in from DataRow r in plugin.householdData.Tables[plugin.HOUSEHOLD_STRUCTURE_TABLE].Rows
                                    where r.Field<int>("ParentID") < 0 && (!checkedOnly || r.Field<bool>("isChecked"))
                                    select r)
                hhTypes.Add(row.Field<int>("dataId"), row.Field<string>("HouseholdName"));
            return hhTypes;
        }
    }
}
