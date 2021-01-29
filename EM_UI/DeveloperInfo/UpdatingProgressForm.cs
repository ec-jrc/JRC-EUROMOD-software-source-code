using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo
{
    internal partial class UpdatingProgressForm : Form
    {
        const string _private = "private";
        const string _available = "available";
        const string _notAvailable = "-";
        const string _colHeadingNonStandard = "other";
        const string _colDataYearPrefix = "colDataYear";

        void UpdatingProgressForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        internal UpdatingProgressForm()
        {
            InitializeComponent();

            txtFolder.Text = EM_AppContext.FolderOutput;

            foreach (Country country in CountryAdministrator.GetCountries())
                lstCountries.Items.Add(country._shortName);
        }

        void chkExportCombinations_CheckedChanged(object sender, EventArgs e) { txtCombinations.Enabled = !txtCombinations.Enabled; }
        void chkExportDatasetProgress_CheckedChanged(object sender, EventArgs e) { txtDatasetProgress.Enabled = !txtDatasetProgress.Enabled; }
        void chkExportSystemProgress_CheckedChanged(object sender, EventArgs e) { txtSystemProgress.Enabled = !txtSystemProgress.Enabled; }

        void btnAll_Click(object sender, EventArgs e) { CheckCountries(CheckState.Checked); }
        void btnNo_Click(object sender, EventArgs e) { CheckCountries(CheckState.Unchecked); }
        void CheckCountries(CheckState checkState)
        {
            for (int i = 0; i < lstCountries.Items.Count; ++i )
                lstCountries.SetItemCheckState(i, checkState);
        }
        
        void btnSelectExportFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the Export folder";
            folderBrowserDialog.SelectedPath = txtFolder.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtFolder.Text = folderBrowserDialog.SelectedPath;
        }

        void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                List<string> selectedCountries = new List<string>();
                foreach (object checkedItem in lstCountries.CheckedItems)
                    selectedCountries.Add(checkedItem as string);

                //outsource the time-consuming reading of country files to a backgroundworker showhing progress
                ProgressIndicator progressIndicator = new ProgressIndicator(Generate_BackgroundEventHandler, "Generating required info ...", selectedCountries);
                if (progressIndicator.ShowDialog() == DialogResult.Cancel)
                    return;

                //fill the tables (rather done here than in the backgroundworker is the latter causes problems with access to the tables (which belong to another process)
                Cursor = Cursors.WaitCursor;

                List<object> configFacades = progressIndicator.Result as List<object>;
                List<CountryConfigFacade> countryConfigFacades = configFacades.ElementAt(0) as List<CountryConfigFacade>;
                List<DataConfigFacade> dataConfigFacades = configFacades.ElementAt(1) as List<DataConfigFacade>;

                if (chkGenerateSystemProgress.Checked) //fill system progress table
                    FillSystemTable(countryConfigFacades);
                if (chkGenerateDatasetProgress.Checked) //fill dataset progress table
                    FillDatasetTable(dataConfigFacades, selectedCountries);
                if (chkGenerateCombinations.Checked) //fill combination table
                    FillCombinationsTable(dataConfigFacades);

                UserInfoHandler.ShowSuccess("Tables successfully generated.");
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        void Generate_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker backgroundWorker = sender as BackgroundWorker;

                //gather the country-configs of the selected countries, to be used by the tabel filling functions
                List<CountryConfigFacade> countryConfigFacades = new List<CountryConfigFacade>();
                List<DataConfigFacade> dataConfigFacades = new List<DataConfigFacade>();
                int actionCounter = 0;
                
                List<string> selectedCountries = e.Argument as List<string>;
                foreach (string countryShortName in selectedCountries)
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button

                    countryConfigFacades.Add(CountryAdministrator.GetCountryConfigFacade(countryShortName));
                    dataConfigFacades.Add(CountryAdministrator.GetDataConfigFacade(countryShortName));
                    backgroundWorker.ReportProgress(Convert.ToInt32((actionCounter++ + 1) / (selectedCountries.Count * 1.0) * 100.0));
                }

                //rather difficult to put two-fold information in the result-object, but List<object> works
                List<object> configFacades = new List<object>();
                configFacades.Add(countryConfigFacades);
                configFacades.Add(dataConfigFacades);
                e.Result = configFacades;
            }
            catch (Exception exception)
            {
                e.Cancel = true;
                UserInfoHandler.ShowException(exception);
            }
        }

        void FillSystemTable(List<CountryConfigFacade> countryConfigFacades)
        {
            dgvSystems.Rows.Clear();
            dgvSystems.Columns.Clear();

            //key: system year, value-dictionary: key: country, value: country's system of this year is private or not
            Dictionary<string, Dictionary<string, string>> systemsCountrySettings = new Dictionary<string, Dictionary<string, string>>();

            dgvSystems.Columns.Add(string.Empty, string.Empty); //rows can only be added if at least one column exists (header is changed below)
            
            //loop over countries
            foreach (CountryConfigFacade countryConfigFacade in countryConfigFacades)
            {
                string countryShortName = countryConfigFacade.GetCountryShortName().ToUpper();

                //loop over systems of the country
                foreach (CountryConfig.SystemRow system in countryConfigFacade.GetSystemRows())
                {
                    //if the system is a "standard system", i.e. called cc_yyyy, extract the year of the name
                    string systemYear = string.Empty;
                    if (system.Name.Length == 7 &&
                        system.Name.Substring(0, 3).ToUpper() == countryShortName + "_" &&
                        EM_Helpers.IsNonNegInteger(system.Name.Substring(3)))
                        systemYear = system.Name.Substring(3);

                    if (systemYear != string.Empty) //standard system (e.g. EE_2012)
                    {
                        //add an entry for the system year, if not already existent
                        if (!systemsCountrySettings.ContainsKey(systemYear))
                            systemsCountrySettings.Add(systemYear, new Dictionary<string, string>());

                        //add the country and whether the system is private or not
                        Dictionary<string, string> countrySettings = systemsCountrySettings[systemYear];
                        if (countrySettings.ContainsKey(countryShortName))
                            continue; //if fact not possible (country cannot have two systems with the same name)
                        countrySettings.Add(countryShortName, system.Private == DefPar.Value.YES ? _private : _available);
                    }
                    else //non-standard system 
                    {
                        //add an entry "other" for non-standard systems, if not already existent
                        if (!systemsCountrySettings.ContainsKey(_colHeadingNonStandard))
                            systemsCountrySettings.Add(_colHeadingNonStandard, new Dictionary<string, string>());
                        
                        //add the country and the non-standard-system and indicate if private
                        Dictionary<string, string> countrySettings = systemsCountrySettings[_colHeadingNonStandard];
                        string entry = system.Name + ((system.Private == DefPar.Value.YES) ? " (" + _private + ")" : string.Empty);
                        if (countrySettings.ContainsKey(countryShortName))
                            countrySettings[countryShortName] += "; " + entry;
                        else
                            countrySettings.Add(countryShortName, entry);
                    } 
                }

                //generate a row for each country
                int rowIndex = dgvSystems.Rows.Add();
                dgvSystems.Rows[rowIndex].HeaderCell.Value = countryShortName;
            }

            //sort the dictionary (to get 2001, 2002, 2003, etc. instead of 2001, 2003, 2002, etc.)
            List<KeyValuePair<string, Dictionary<string, string>>> sortedSystemsCountrySettings = systemsCountrySettings.ToList();
            sortedSystemsCountrySettings.Sort((firstPair, nextPair) => { return firstPair.Key.CompareTo(nextPair.Key); });

            //generate the columns ...
            bool first = true;
            foreach (KeyValuePair<string, Dictionary<string, string>> systemCountrySettings in sortedSystemsCountrySettings)
            {
                Dictionary<string, string> countrySettings = systemCountrySettings.Value;

                int columnIndex = 0;
                if (!first)
                {
                    columnIndex = dgvSystems.Columns.Add(string.Empty, string.Empty);
                    dgvSystems.Columns[columnIndex].SortMode = DataGridViewColumnSortMode.NotSortable;
                }
                else
                    first = false;
                dgvSystems.Columns[columnIndex].HeaderCell.Value = systemCountrySettings.Key;

                //... and fill the table
                foreach (DataGridViewRow row in dgvSystems.Rows)
                {
                    string countryShortName = row.HeaderCell.Value.ToString();
                    if (countrySettings.ContainsKey(countryShortName))
                        row.Cells[columnIndex].Value = countrySettings[countryShortName];
                    else
                        row.Cells[columnIndex].Value = _notAvailable;
                }
            }
        }

        void FillDatasetTable(List<DataConfigFacade> dataConfigFacades, List<string> countryShortNames)
        {
            dgvDatasets.Rows.Clear();
            for (int i = dgvDatasets.Columns.Count - 1; i >= 0; --i)
                if (dgvDatasets.Columns[i].Name.StartsWith(_colDataYearPrefix))
                    dgvDatasets.Columns.RemoveAt(i); //delete all but the fix columns (those displaying data-properties like collection year, private, ...)

            List<string> dataYears = new List<string>();

            //loop over countries
            for (int index = 0; index < dataConfigFacades.Count; ++index)
            {
                DataConfigFacade dataConfigFacade = dataConfigFacades.ElementAt(index);

                //loop over dataset of the country
                bool firstDatasetOfCountry = true;
                foreach (DataConfig.DataBaseRow dataset in dataConfigFacade.GetDataBaseRows())
                {
                    //if the dataset is a "standard dataset", i.e. called cc_yyyy_xx, extract the year of the name
                    string datasetName = dataset.Name.ToLower().EndsWith(".txt") ? dataset.Name.Substring(0, dataset.Name.Length - 4) : dataset.Name;
                    string dataYear = string.Empty;
                    if (datasetName.Length >= 8 &&
                        datasetName.Substring(2, 1) == "_" && datasetName.Substring(7, 1) == "_" &&
                        EM_Helpers.IsNonNegInteger(datasetName.Substring(3, 4)))
                        dataYear = datasetName.Substring(3, 4);
                    else if (datasetName.ToLower().Contains("hypo")) //take respect of hypo-data ...
                        dataYear = "hypo";
                    else if (datasetName.ToLower().Contains("training")) //... and training data (which also exist for most countries)
                        dataYear = "training";
                    else
                        dataYear = _colHeadingNonStandard; //probably only SL_demo_vi

                    //add an entry for the dataset year, if not already existent
                    if (!dataYears.Contains(dataYear))
                        dataYears.Add(dataYear);

                    //generate a row for each dataset and equip the row's tag with information (data-year and data-properties) for the cell-filling below
                    int rowIndex = dgvDatasets.Rows.Add();
                    dgvDatasets.Rows[rowIndex].Tag = new KeyValuePair<string, DataConfig.DataBaseRow>(dataYear, dataset);

                    //put name of country in row header if first dataset for this country
                    if (firstDatasetOfCountry && index < countryShortNames.Count)
                    {
                        dgvDatasets.Rows[rowIndex].HeaderCell.Value = countryShortNames.ElementAt(index);
                        firstDatasetOfCountry = false;
                    }
                }
            }

            //sort the list of years (to get 2001, 2002, 2003, etc. instead of 2001, 2003, 2002, etc.) and generate the columns
            dataYears.Sort();
            dataYears.Reverse();
            foreach (string dataYear in dataYears)
            {
                DataGridViewTextBoxColumn column = new DataGridViewTextBoxColumn();
                column.Name = _colDataYearPrefix + dataYear;
                column.HeaderCell.Value = dataYear;
                dgvDatasets.Columns.Insert(0, column); //insert before the fix columns (collection year, private, etc.)
            }

            //fill the table
            foreach (DataGridViewRow row in dgvDatasets.Rows)
            {
                KeyValuePair<string, DataConfig.DataBaseRow> rowTag = (KeyValuePair<string, DataConfig.DataBaseRow>)(row.Tag);
                DataConfig.DataBaseRow dataset = rowTag.Value;
                string dataYear = rowTag.Key;
                row.Cells[colYearCollection.Name].Value = dataset.YearCollection;
                row.Cells[colYearIncome.Name].Value = dataset.YearInc;
                row.Cells[colPrivate.Name].Value = dataset.Private;
                row.Cells[_colDataYearPrefix + dataYear].Value = dataset.Name;
            }
        }

        void FillCombinationsTable(List<DataConfigFacade> dataConfigFacades)
        {
            dgvCombinations.Rows.Clear();
            dgvCombinations.Columns.Clear();

            foreach (DataConfigFacade dataConfigFacade in dataConfigFacades)
            {
                Dictionary<string, int> systemNames = new Dictionary<string, int>();
                List<KeyValuePair<string, List<string>>> datasetAvailablities = new List<KeyValuePair<string, List<string>>>();

                //first gather all systems (for the header row)
                foreach (DataConfig.DataBaseRow dataset in dataConfigFacade.GetDataBaseRows())
                {
                    foreach (DataConfig.DBSystemConfigRow dataSystemCombination in dataset.GetDBSystemConfigRows())
                    {
                        if (!systemNames.ContainsKey(dataSystemCombination.SystemName))
                            systemNames.Add(dataSystemCombination.SystemName, systemNames.Count);
                    }
                }

                //then run over datasets and assess for which systems they are available
                foreach (DataConfig.DataBaseRow dataset in dataConfigFacade.GetDataBaseRows())
                {
                    List<string> availability = new List<string>();
                    for (int index = 0; index < systemNames.Count; ++index)
                        availability.Add(string.Empty);
                    foreach (DataConfig.DBSystemConfigRow dataSystemCombination in dataset.GetDBSystemConfigRows())
                    {
                        int index = systemNames[dataSystemCombination.SystemName];
                        availability[index] = dataSystemCombination.BestMatch == DefPar.Value.YES ? DefPar.Value.DATA_SYS_BEST : DefPar.Value.DATA_SYS_X;
                    }
                    datasetAvailablities.Add(new KeyValuePair<string, List<string>>(dataset.Name, availability));
                }

                //finally add country's dataset-system-combinations to the table
                //first add header-row with system-names
                int columnIndex = 0;
                if (dgvCombinations.Columns.Count == 0)
                    dgvCombinations.Columns.Add(string.Empty, string.Empty); //cannot add a row to the GridView if there is not at least one column
                int rowIndex = dgvCombinations.Rows.Add();
                foreach (string systemName in systemNames.Keys)
                {
                    if (dgvCombinations.Columns.Count <= columnIndex)
                        dgvCombinations.Columns.Add(string.Empty, string.Empty);
                    dgvCombinations.Rows[rowIndex].Cells[columnIndex].Value = systemName;
                    ++columnIndex;
                }
                //then add dataset rows and fill cells with availability
                foreach (KeyValuePair<string, List<string>> datasetAvailablity in datasetAvailablities)
                {
                    rowIndex = dgvCombinations.Rows.Add();
                    dgvCombinations.Rows[rowIndex].HeaderCell.Value = datasetAvailablity.Key;
                    for (columnIndex = 0; columnIndex < datasetAvailablity.Value.Count; ++columnIndex)
                        dgvCombinations.Rows[rowIndex].Cells[columnIndex].Value = datasetAvailablity.Value.ElementAt(columnIndex);
                }
            }
        }

        void btnExport_Click(object sender, EventArgs e)
        {
            if (chkExportSystemProgress.Checked)
                if (!DeveloperInfoTools.ExportDataGridView(dgvSystems, txtFolder.Text, txtSystemProgress.Text))
                    return;

            if (chkExportDatasetProgress.Checked)
                if (!DeveloperInfoTools.ExportDataGridView(dgvDatasets, txtFolder.Text, txtDatasetProgress.Text))
                    return;
            
            if (chkExportCombinations.Checked)
                if (!DeveloperInfoTools.ExportDataGridView(dgvCombinations, txtFolder.Text, txtCombinations.Text))
                    return;

            UserInfoHandler.ShowInfo("Exporting files accomplished.");
        }
    }
}
