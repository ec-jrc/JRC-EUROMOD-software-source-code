using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Data;

namespace HypotheticalHousehold
{
    public partial class ImportVariablesForm : Form
    {
        private Program plugin;
        private string importProjectPath;

        private VariableDataSet masterVarSettings = null;
        private VariableDataSet importVarSettings = null;

        private List<string> acceptedCountries = new List<string>();
        private List<string> notAcceptedCountries = new List<string>();

        private const string GRID_TEXT_NEW = "NEW";
        private const string GRID_TEXT_DIFFERENT = "DIFFERENT ...";
        private const string COL_VARIABLE_NAME = "VariableName";
        private const string COL_CATEGORY = "Category";

        internal class Difference
        {
            internal string description, masterVal, importVal;
        }

        public ImportVariablesForm()
        {
            InitializeComponent();
        }

        internal bool Init(Program plugin, string importProjectPath)
        {
            try
            {
                this.plugin = plugin;
                this.importProjectPath = importProjectPath;

                // read the VariableSettings of the import project into importVarSettings
                // and, for convenience, call plugin.settingsData masterVarSettings (i.e. masterVarSettings = plugin.settingsData)
                GetData(plugin, importProjectPath);

                // check if import project has countries which do not exist in master project
                if (!CompareCountries()) return false;
                if (notAcceptedCountries.Count > 0)         // remove all references for countries which exist in import project
                {                                           // but the user has decided to not import
                    SettingsManagement.ManageCountries.RemoveCountryReferences(importVarSettings, notAcceptedCountries);
                    importVarSettings.AcceptChanges();
                }

                // compare each variable type and write results to the respective grid ...
                // ... basic variables
                CompareTable(masterVarSettings.Cur_BasicVariables, importVarSettings.Cur_BasicVariables, gridBasic);
                // ... advanced variables
                CompareTable(masterVarSettings.Cur_AdvancedVariables, importVarSettings.Cur_AdvancedVariables, gridAdvanced);
                // ... advanced country specific variables
                CompareTable(masterVarSettings.Cur_AdvancedCountrySpecificVariables, importVarSettings.Cur_AdvancedCountrySpecificVariables, gridAdvCSpec);
                // ... derived variables
                CompareTable(masterVarSettings.Cur_DerivedVariables, importVarSettings.Cur_DerivedVariables, gridDerived);

                // activate the first tab where the user can see a change
                bool diffFound = false;
                if (gridBasic.Rows.Count > 0) { tabControl.SelectedTab = tabBasic; diffFound = true; }
                else if (gridAdvanced.Rows.Count > 0) { tabControl.SelectedTab = tabAdvanced; diffFound = true; }
                else if (gridAdvCSpec.Rows.Count > 0) { tabControl.SelectedTab = tabAdCSpec; diffFound = true; }
                else if (gridDerived.Rows.Count > 0) { tabControl.SelectedTab = tabDerived; diffFound = true; }
                
                if (!diffFound)
                    MessageBox.Show(string.Format("Variables Settings of {0} and {1} are equal.",
                                    Path.GetFileName(plugin.currentProjectPath), Path.GetFileName(importProjectPath)) +
                                    Environment.NewLine + "Nothing to import.");
                return diffFound;
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Failed to assess variables-differences of HHOT projects {0} and {1}:",
                    Path.GetFileName(plugin.currentProjectPath), Path.GetFileName(importProjectPath)) +
                    Environment.NewLine + exception.Message);
                return false;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                // first, if necessary, add the countries which exist in import project but not in master project
                foreach (string country in acceptedCountries)
                {
                    string caseSensCountry = (from i in importVarSettings.Cur_Countries // for the unlikely case that country is not
                        where i.Country.ToUpper().Trim() == country.ToUpper().Trim()    // upper-case, take the original name to not get 
                        select i.Country).First();                                      // broken links for adv-country-spec. variables
                    masterVarSettings.Cur_Countries.AddCur_CountriesRow(caseSensCountry);
                }

                // then, for each variable-type, execute the changes the user decided for
                PerformChangesVarType(gridBasic, masterVarSettings.Cur_BasicVariables, masterVarSettings.Cur_BasicCountrySpecificDetail);
                PerformChangesVarType(gridAdvanced, masterVarSettings.Cur_AdvancedVariables);
                PerformChangesVarType(gridAdvCSpec , masterVarSettings.Cur_AdvancedCountrySpecificVariables, masterVarSettings.Cur_AdvancedCountrySpecificDetail);
                PerformChangesVarType(gridDerived, masterVarSettings.Cur_DerivedVariables, masterVarSettings.Cur_DerivedVariablesDetail);

                plugin.saveSettings();
                MessageBox.Show("Import succeeded - new settings saved.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                masterVarSettings.RejectChanges();
                MessageBox.Show(string.Format("Failed to import variables from HHOT project {0}:",
                    Path.GetFileName(importProjectPath)) + Environment.NewLine + exception.Message);
                DialogResult = DialogResult.Cancel;
            }
            Close();
        }

        private void PerformChangesVarType(DataGridView grid, DataTable masterMainTable, DataTable masterSubTable = null)
        {
            string getColName = colGetBasic.Name.Replace("Basic", grid.Name.Replace("grid", "")); // colGetBasic -> e.g. colGetAdvanced
            string statusColName = colStatusBasic.Name.Replace("Basic", grid.Name.Replace("grid", ""));

            foreach (DataGridViewRow dgvRow in grid.Rows)
            {
                if (Convert.ToBoolean(dgvRow.Cells[getColName].Value) == false) continue; // user does not want to overtake the difference
                if (dgvRow.Cells[statusColName].Value.ToString() == GRID_TEXT_NEW)
                {
                    AddVariable(masterMainTable, masterSubTable, dgvRow.Tag as DataRow);
                }
                else // if different, simply ...
                {
                    Tuple<DataRow, DataRow> impAndMasterRows = dgvRow.Tag as Tuple<DataRow, DataRow>;
                    RemoveVariable(masterSubTable, impAndMasterRows.Item1); // ... delete the master-variable ...
                    AddVariable(masterMainTable, masterSubTable, impAndMasterRows.Item2); // ... to then add the import-variable
                }
            }
        }

        private void RemoveVariable(DataTable masterSubTable, DataRow masterMainRow)
        {
            if (masterSubTable != null) // first delete the variable from the sub-table (e.g. Cur_BasicCountrySpecificDetail) ...
            {
                List<DataRow> delRows = new List<DataRow>();
                foreach (DataRow masterSubRow in masterMainRow.GetChildRows(masterMainRow.Table.ChildRelations[0]))
                    delRows.Add(masterSubRow);
                foreach (DataRow delRow in delRows) delRow.Delete();
            }
            masterMainRow.Delete(); // ... then from the main-table (e.g. Cur_BasicVariables)
        }

        private void AddVariable(DataTable masterMainTable, DataTable masterSubTable, DataRow importRow)
        {
            // if the imported variable has a category that does not exist in the master project, the category must be added
            string categ = importRow.Field<string>(COL_CATEGORY);
            if ((from m in masterVarSettings.Cur_Categories where m.Category == categ select m).Count() == 0)
                masterVarSettings.Cur_Categories.AddCur_CategoriesRow(categ);

            // first add the main-row of the variable (e.g. Cur_BasicVariables) ...
            masterMainTable.Rows.Add(importRow.ItemArray);
            if (masterSubTable == null) return;
            // ... then add the sub-rows of the variable (e.g. Cur_BasicCountrySpecificDetail)
            // the tables each only have one sub-table (e.g. Cur_BasicVariables + Cur_BasicCountrySpecificDetail)
            // thus one can use ChildRelations[0]
            foreach (DataRow importSubRow in importRow.GetChildRows(importRow.Table.ChildRelations[0]))
                masterSubTable.Rows.Add(importSubRow.ItemArray);
        }

        // basic variable: get differences in country specific names
        List<Difference> GetSubDiffBasic(string varName)
        {
            var masterRows = from m in masterVarSettings.Cur_BasicCountrySpecificDetail
                             where m.VariableName.ToLower().Trim() == varName select m;
            var importRows = from i in importVarSettings.Cur_BasicCountrySpecificDetail
                             where i.VariableName.ToLower().Trim() == varName select i;
            
            List<Difference> diffs = new List<Difference>();
            foreach (VariableDataSet.Cur_BasicCountrySpecificDetailRow masterRow in masterRows)
            {
                var imp = from i in importRows
                          where i.CountrySpecificVariableName.ToLower().Trim() == masterRow.CountrySpecificVariableName.ToLower().Trim()
                          select i;
                // specific name only exists in master project
                if (imp.Count() == 0)
                {
                    diffs.Add(new Difference() { description = "Specific Name: " + masterRow.CountrySpecificVariableName, 
                                                 masterVal = masterRow.Countries, importVal = "-" });
                    continue;
                }
                // if specific name exists in both projects, check if it concerns the same countries
                VariableDataSet.Cur_BasicCountrySpecificDetailRow importRow = imp.First();
                string masterCountries = masterRow.Countries;
                string importCountries = importRow.Countries;
                if (importCountries.ToLower().Trim() != masterCountries.ToLower().Trim())
                    diffs.Add(new Difference() { description = "Specific Name: " + masterRow.CountrySpecificVariableName, 
                                                 masterVal = masterRow.Countries, importVal = importRow.Countries });
            }
            // finally check for specific names which exist only in import project
            foreach (VariableDataSet.Cur_BasicCountrySpecificDetailRow importRow in importRows)
            {
                if ((from m in masterRows
                     where m.CountrySpecificVariableName.ToLower().Trim() == importRow.CountrySpecificVariableName.ToLower().Trim()
                     select m).Count() != 0) continue; // exists in both projects, being equal was checked above

                diffs.Add(new Difference() { description = "Specific Name: " + importRow.CountrySpecificVariableName,
                                             masterVal = "-", importVal = importRow.Countries });
            }
            return diffs;
        }

        // advanced country specific variable: check if country details are equal
        List<Difference> GetSubDiffAdvCSpec(string varName)
        {
            var masterRows = from m in masterVarSettings.Cur_AdvancedCountrySpecificDetail
                             where m.VariableName.ToLower().Trim() == varName
                             select m;
            var importRows = from i in importVarSettings.Cur_AdvancedCountrySpecificDetail
                             where i.VariableName.ToLower().Trim() == varName
                             select i;

            List<Difference> diffs = new List<Difference>();
            foreach (VariableDataSet.Cur_AdvancedCountrySpecificDetailRow masterRow in masterRows)
            {
                var imp = from i in importRows
                          where i.Country.ToLower().Trim() == masterRow.Country.ToLower().Trim()
                          select i;
                // country details only exist in master project
                if (imp.Count() == 0)
                {
                    diffs.Add(new Difference() { description = masterRow.Country, masterVal = "Has Specification", importVal = "-" });
                    continue;
                }
                // if country details exist in both projects, check if they are equal
                VariableDataSet.Cur_AdvancedCountrySpecificDetailRow importRow = imp.First();
                foreach (DataColumn col in masterRow.Table.Columns)
                {
                    string masterVal = GetFieldStringValue(masterRow, col.ColumnName);
                    string importVal = GetFieldStringValue(importRow, col.ColumnName);
                    if (masterVal.ToLower().Trim() == importVal.ToLower().Trim()) continue;
                    diffs.Add(new Difference() { description = masterRow.Country + ": " + col.ColumnName,
                                                 masterVal = masterVal, importVal = importVal });
                }
            }
            // finally check for country details which exist only in import project
            foreach (VariableDataSet.Cur_AdvancedCountrySpecificDetailRow importRow in importRows)
            {
                if ((from m in masterRows
                     where m.Country.ToLower().Trim() == importRow.Country.ToLower().Trim()
                     select m).Count() != 0) continue; // exists in both projects, being equal was checked above

                diffs.Add(new Difference() { description = importRow.Country,
                                             masterVal = "-", importVal = "Has Specification" });
            }
            return diffs;
        }

        // derived variable: check if conditions are equal
        List<Difference> GetSubDiffDerived(string varName)
        {
            var masterRows = from m in masterVarSettings.Cur_DerivedVariablesDetail
                             where m.VariableName.ToLower().Trim() == varName
                             select m;
            var importRows = from i in importVarSettings.Cur_DerivedVariablesDetail
                             where i.VariableName.ToLower().Trim() == varName
                             select i;

            bool equal = masterRows.Count() == importRows.Count();
            if (equal)
            {
                foreach (VariableDataSet.Cur_DerivedVariablesDetailRow masterRow in masterRows)
                {
                    var imp = from i in importRows
                              where i.Condition.ToLower().Trim() == masterRow.Condition.ToLower().Trim() &&
                              i.DerivedValue.ToLower().Trim() == masterRow.DerivedValue.ToLower().Trim()
                              select i;
                    if (imp.Count() == 0) { equal = false; break; }
                }
            }
            
            List<Difference> diffs = new List<Difference>();
            if (!equal) // if different, just show the whole list of conditions for each project (this seems most informative)
            {
                string masterCond = string.Empty, importCond = string.Empty;
                foreach (VariableDataSet.Cur_DerivedVariablesDetailRow m in masterRows)
                    masterCond += m.Condition + " => " + m.DerivedValue + Environment.NewLine;
                foreach (VariableDataSet.Cur_DerivedVariablesDetailRow i in importRows)
                    importCond += i.Condition + " => " + i.DerivedValue + Environment.NewLine;
                diffs.Add(new Difference() { description = "Conditions", masterVal = masterCond.Trim(), importVal = importCond.Trim() });
            }
            return diffs;
        }

        private void CompareTable(DataTable masterTable, DataTable importTable, DataGridView grid)
        {
            foreach (DataRow importRow in importTable.Rows)
            {
                string varName = GetFieldStringValue(importRow, COL_VARIABLE_NAME);
                DataRow masterRow = null;
                foreach (DataRow mr in masterTable.Rows)
                    if (GetFieldStringValue(mr, COL_VARIABLE_NAME).ToLower().Trim() == varName.ToLower().Trim()) { masterRow = mr; break; }

                // variable exists only in import project - add to grid as NEW
                if (masterRow == null)
                {
                    int r = grid.Rows.Add(varName, true, GRID_TEXT_NEW);
                    grid.Rows[r].Tag = importRow;
                }
                // variable exists in both projects, check for differences, and possibly add to grid as DIFFERENT
                else
                {
                    List<Difference> diffs = GetVarDiff(masterRow, importRow, grid);
                    if (diffs.Count > 0)
                    {
                        int r = grid.Rows.Add(varName, true, GRID_TEXT_DIFFERENT);
                        grid.Rows[r].Tag = new Tuple<DataRow, DataRow>(masterRow, importRow);
                    }
                }
            }
        }

        private List<Difference> GetMainDiff(DataRow masterRow, DataRow importRow)
        {
            List<Difference> diffs = new List<Difference>();
            foreach (DataColumn col in masterRow.Table.Columns)
            {
                string masterVal = GetFieldStringValue(masterRow, col.ColumnName);
                string importVal = GetFieldStringValue(importRow, col.ColumnName);

                if (masterVal.ToLower().Trim() != importVal.ToLower().Trim())
                    diffs.Add(new Difference() { description = col.ColumnName, masterVal = masterVal, importVal = importVal });
            }
            return diffs;
        }

        internal static string GetFieldStringValue(DataRow row, string colName)
        {
            object o = row.Field<object>(colName);
            return o == null ? string.Empty : o.ToString();
        }

        private void GetData(Program plugin, string importProjectPath)
        {
            importVarSettings = new VariableDataSet();
            importVarSettings.ReadXml(plugin.getSettingsFile(importProjectPath));
            plugin.fixVariablesDecimalSeparator(importVarSettings); importVarSettings.AcceptChanges();
            masterVarSettings = plugin.settingsData;
        }

        private bool CompareCountries()
        {
            List<string> importOnlyCountries = new List<string>();
            foreach (VariableDataSet.Cur_CountriesRow iC in importVarSettings.Cur_Countries)
            {
                if ((from mC in masterVarSettings.Cur_Countries
                     where mC.Country.ToUpper().Trim() == iC.Country.ToUpper().Trim()
                     select mC).Count() == 0)
                    importOnlyCountries.Add(iC.Country.ToUpper().Trim());
            }
            if (importOnlyCountries.Count == 0) return true; // no countries which exist only in import project

            ImportCountriesForm form = new ImportCountriesForm(importOnlyCountries);
            if (form.ShowDialog() == DialogResult.Cancel) return false;
            form.GetResult(out acceptedCountries, out notAcceptedCountries);
            return true;
        }

        private void grid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            DataGridView grid = sender as DataGridView;
            if (grid == null || e.RowIndex < 0 || e.ColumnIndex < 0) return;
            if (grid.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString() != GRID_TEXT_DIFFERENT) return;

            Tuple<DataRow, DataRow> tag = grid.Rows[e.RowIndex].Tag as Tuple<DataRow, DataRow>;
            DataRow masterRow = tag.Item1, importRow = tag.Item2;
            string varName = GetFieldStringValue(tag.Item1, COL_VARIABLE_NAME);

            List<Difference> diff = GetVarDiff(masterRow, importRow, grid);
            
            (new DiffVariablesForm(varName, Path.GetFileName(plugin.currentProjectPath), Path.GetFileName(importProjectPath), diff, plugin))
                .ShowDialog();
        }

        private List<Difference> GetVarDiff(DataRow masterRow, DataRow importRow, DataGridView grid)
        {
            // first get the differneces of the main table, e.g. Cur_BasicVariables, i.e. Category, Description, ValueRange, ...
            List<Difference> diff = GetMainDiff(masterRow, importRow);

            // then get the differneces of a (possible, specific) sub-table, e.g. Cur_BasicCountrySpecificDetail
            string varName = GetFieldStringValue(masterRow, COL_VARIABLE_NAME).ToLower().Trim();
            if (grid.Name == gridBasic.Name) diff.AddRange(GetSubDiffBasic(varName));
            else if (grid.Name == gridAdvCSpec.Name) diff.AddRange(GetSubDiffAdvCSpec(varName));
            else if (grid.Name == gridDerived.Name) diff.AddRange(GetSubDiffDerived(varName));

            return diff;
        }
        
        private void btnGetAll_Click(object sender, EventArgs e) { btnGet_Click(true); }
        private void btnGetNone_Click(object sender, EventArgs e) { btnGet_Click(false); }
        private void btnGet_Click(bool get)
        {
            if (tabControl.SelectedTab == null) return;
            DataGridView grid = null; string colGet = null;
            if (tabControl.SelectedTab.Name == tabBasic.Name) { grid = gridBasic; colGet = colGetBasic.Name; }
            else if (tabControl.SelectedTab.Name == tabAdvanced.Name) { grid = gridAdvanced; colGet = colGetAdvanced.Name; }
            else if (tabControl.SelectedTab.Name == tabAdCSpec.Name) { grid = gridAdvCSpec; colGet = colGetAdvCSpec.Name; }
            else if (tabControl.SelectedTab.Name == tabDerived.Name) { grid = gridDerived; colGet = colGetDerived.Name; }

            foreach (DataGridViewRow row in grid.Rows) row.Cells[colGet].Value = get;
        }
    }
}
