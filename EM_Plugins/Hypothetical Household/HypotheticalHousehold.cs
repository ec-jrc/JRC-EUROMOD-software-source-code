using EM_Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    /*
     * The plugin consists of two main parts: the Program part and the Metadata part. 
     * The Metadata holds basic plugin info such as its name and path.
     * The Program part is the one responsible for all the actual work the plugin does. 
     */
    public class Program : PiInterface
    {
        internal bool isDeveloper = false;
        public const string EDITOR_TYPE_NUMERIC = "Numeric";
        public const string EDITOR_TYPE_COMBO = "Categorical";
        public const string EDITOR_TYPE_CONNECTION = "Connection";

        public const string OLD_EDITOR_TYPE_NUMERIC = "EditorNumeric";
        public const string OLD_EDITOR_TYPE_COMBO = "EditorCombo";
        public const string OLD_EDITOR_TYPE_CONNECTION = "EditorConnection";

        private const string FILENAME_HOUSEHOLD_DATA = "HouseholdData.xml";
        private const string FILENAME_SETTINGS_DATA_OLD = "settings.xml";
        private const string FILENAME_SETTINGS_DATA = "VariableSettings.xml";
        private const string FILENAME_TABLES_DATA = "ReferenceTables.xml";
        internal string HOUSEHOLD_STRUCTURE_TABLE = "HouseholdStructure";

        public enum HHoT_TEMPLATE_TYPE { BUDGET_CONSTRAINTS, BREAKDOWN_HH_TYPES, BREAKDOWN_COUNTRY_YEAR }
        public enum HHoT_INCOME_RANGE { RANGE_HOURS_INCOME, RANGE_WAGE_INCOME, RANGE_INCOME_HOURS, RANGE_INCOME_WAGE }

        private readonly List<string> OLD_EDITORTYPE_TABLES = new List<string>() { "Cur_BasicVariables", "Cur_AdvancedVariables", "Cur_AdvancedCountrySpecificVariables", "Def_BasicVariables", "Def_AdvancedVariables", "Def_AdvancedCountrySpecificVariables" };
        
        internal Form mainForm;               // holds a link to the EUROMOD UI Form (used to center the input/output forms)
        private InputForm inputForm = null;   // holds the Input Form
//        private OutputForm outputForm;      // holds the Output Form
//        internal Dictionary<string, object> EMSettings;     // holds the settings that were passed by the EUROMOD UI 
        internal VariableDataSet settingsData;     // holds the plugin settings
        internal ReferenceTablesDataSet referenceTablesData;     // holds the plugin settings
        internal DataSet householdData;     // holds the household data -- check generateHouseholdStructureData
        readonly Random randomGenerator = new Random();
        //internal string currentlyOpenFile = ""; // not necessary anymore with introduction of projects
        
        internal string currentProjectPath = string.Empty;
        internal UserSettings userSettings = null;

        // This function is responsible for running the plugin
        public override void Run(Dictionary<string, object> _settings)
        {
            try
            {
                mainForm = EM_Common_Win.UISessionInfo.GetActiveMainForm();
                if (inputForm != null) // we may experiment if it is possible to have to instances open,
                {                      // but for the time being just activate and show the single instance
                    if (inputForm.WindowState == FormWindowState.Minimized) inputForm.WindowState = FormWindowState.Normal;
                    inputForm.Activate();
                    return;
                }

                isDeveloper = EnvironmentInfo.ShowComponent("HHoT_developer");

                userSettings = new UserSettings();
                string projectPath = userSettings.GetLastOpenProjectPath();
                if (string.IsNullOrEmpty(projectPath) && !HandleFirstCall(out projectPath)) return;
                while(true)
                {
                    string error;
                    if (IsHHotProjectPath(projectPath, out error)) break;
                    MessageBox.Show("Folder " + projectPath + " does not contain a valid HHOT project." + Environment.NewLine + error);
                    if (!HandleFirstCall(out projectPath)) return;
                }
                currentProjectPath = projectPath;

                mainForm.Cursor = Cursors.WaitCursor;
                readSettings();                             // get the plugin settings
                readHouseholdData();                        // get the Household data
                inputForm = new InputForm(this) { Text = GetTitleText() };
                inputForm.Show(mainForm);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to run Hypothetical Household Tool." + Environment.NewLine + exception.Message);
            }
            finally { mainForm.Cursor = Cursors.Default; }
        }

        private bool HandleFirstCall(out string projectPath)
        {
            projectPath = null;
            FirstRunForm firstRunForm = new FirstRunForm();
            if (firstRunForm.ShowDialog() == DialogResult.Cancel) return false;

            bool genExample; firstRunForm.GetInfo(out projectPath, out genExample);
            if (genExample) return CreateNewProject(out projectPath);
            return true;
        }

        internal bool SelectPathOpenProject(out string projectPath)
        {
            projectPath = null;
            try
            {
                SelectFolderForm openProjectForm = new SelectFolderForm();
                if (openProjectForm.ShowDialog() == DialogResult.Cancel) return false;
                projectPath = openProjectForm.GetProjectPath();
               
            }
            catch (Exception exception)
            {
                MessageBox.Show("Opening project failed." + Environment.NewLine + exception.Message);
                return false;
            }
            return true;
        }

        internal bool CreateNewProject(out string projectPath)
        {
            projectPath = null;
            try
            {
                PrepareProjectForm prepareProjectForm = new PrepareProjectForm("Generate HHOT Example Project ...");
                if (prepareProjectForm.ShowDialog() == DialogResult.Cancel) return false;
                projectPath = prepareProjectForm.GetProjectPath();
                File.WriteAllText(getSettingsFile(projectPath), Properties.Resources.VariableSettings);
                File.WriteAllText(getHouseholdFile(projectPath), Properties.Resources.HouseholdData);
                File.WriteAllText(getReferenceTablesFile(projectPath), Properties.Resources.ReferenceTables);
            }
            catch (Exception exception)
            {
                MessageBox.Show("Generateing example project failed." + Environment.NewLine + exception.Message);
                return false;
            }
            userSettings = new UserSettings();
            return true;
        }

        public void Close()
        {
            userSettings.settings.lastOpenProject = currentProjectPath;
            userSettings.Write();
            inputForm = null;
        }

        internal string getHouseholdFile(string path = null)
        {
            return Path.Combine(path == null ? currentProjectPath : path, FILENAME_HOUSEHOLD_DATA);
        }

        internal string getSettingsFile(string path = null)
        {
            return Path.Combine(path == null ? currentProjectPath : path, FILENAME_SETTINGS_DATA);
        }

        internal string getReferenceTablesFile(string path = null)
        {
            return Path.Combine(path == null ? currentProjectPath : path, FILENAME_TABLES_DATA);
        }

        private static string getSettingsFileOld()
        {
            return Path.Combine(getHHOTfolder(), FILENAME_SETTINGS_DATA_OLD);
        }

        internal static string getHHOTfolder()
        {
            return Path.Combine(EnvironmentInfo.GetPluginFolder(), "HypotheticalHousehold");
        }

        internal void readSettings()
        {
            if (settingsData != null)
            {
                settingsData.Clear();
                //referenceTablesData.Clear();                      // Clear leaves "traces" back: if e.g. years were changed and then another
                referenceTablesData = new ReferenceTablesDataSet(); // (unchanged) project was opened, reference tables were adapted
            }                                                       // new seems to work, but I have to admit that I do not understand what happens ...
            else
            {
                settingsData = new VariableDataSet();
                referenceTablesData = new ReferenceTablesDataSet();
            }

            if (File.Exists(getSettingsFile())) // New settings file found (with separate reference tables) - read settings and reference tables
            {
                using (DataSet oldSet = new DataSet())
                {
                    oldSet.ReadXml(getSettingsFile());
                    if (oldSet.Tables.Contains("Cur_BasicVariables") && oldSet.Tables["Cur_BasicVariables"].Columns.Contains("EditorType")) // change old "EditorType" format to "VariableType" format
                    {
                        foreach (DataTable tbl in oldSet.Tables)
                        {
                            bool fixEditorType = OLD_EDITORTYPE_TABLES.Contains(tbl.TableName);
                            CopyTableContent(settingsData.Tables[tbl.TableName], tbl, false, fixEditorType);
                        }
                    }
                    else if (oldSet.Tables.Contains("Cur_DerivedVariables") && oldSet.Tables.Contains("Cur_DerivedVariablesDetail") && !oldSet.Tables["Cur_DerivedVariables"].Columns.Contains("Id"))
                    {
                        // first copy tables
                        foreach (DataTable tbl in oldSet.Tables)
                        {
                            CopyTableContent(settingsData.Tables[tbl.TableName], tbl);
                        }
                        // then match the derived variable ids
                        foreach (DataRow row in settingsData.Tables["Cur_DerivedVariablesDetail"].Rows)
                            foreach (DataRow prow in settingsData.Tables["Cur_DerivedVariables"].Rows)
                                if (prow["VariableName"].ToString().Equals(row["VariableName"]))
                                {
                                    row["VarId"] = prow["Id"];
                                    break;
                                }
                                
                    }
                    else    // latest file, just read
                    {
                        settingsData.ReadXml(getSettingsFile());
                    }
                }
                if (File.Exists(getReferenceTablesFile())) referenceTablesData.ReadXml(getReferenceTablesFile());
                else MessageBox.Show("Reference Tables file not found.");
                fixSettingsDecimalSeparator();
                settingsData.AcceptChanges();
                referenceTablesData.AcceptChanges();
            }
            else if (File.Exists(getSettingsFileOld())) // Old settings file found - create new settings and reference tables files 
            {
                using (DataSet oldSet = new DataSet())
                {
                    oldSet.ReadXml(getSettingsFileOld());

                    foreach (DataTable tbl in oldSet.Tables)
                    {
                        if (settingsData.Tables.IndexOf("Cur_" + tbl.TableName) > -1)   // if the table name matches a settings table, copy in settings file and create defaults
                        {
                            bool fixEditorType = OLD_EDITORTYPE_TABLES.Contains(tbl.TableName);
                            CopyTableContent(settingsData.Tables["Cur_" + tbl.TableName], tbl, false, fixEditorType);
                            CopyTableContent(settingsData.Tables["Def_" + tbl.TableName], tbl, false, fixEditorType);
                        }
                        else if (referenceTablesData.Tables.IndexOf(tbl.TableName) > -1)  // else if it matches the table of reference tables, copy it to the reference tables file
                        {
                            CopyTableContent(referenceTablesData.Tables[tbl.TableName], tbl);
                        }
                        else    // else copy over remaining unknown tables in the reference tables file, assuming that they are reference tables
                        {
                            DataTable ct = new DataTable(tbl.TableName);
                            referenceTablesData.Tables.Add(ct);
                            CopyTableContent(referenceTablesData.Tables[tbl.TableName], tbl, true);
                        }
                    }
                }


                fixSettingsDecimalSeparator();
                saveReferenceTables();  // save reference tables into the new tables file
                saveSettings();         // save settings into the new settings file
            }
            else
            {
                MessageBox.Show("Preparing for first run...");
                try
                {
                    // first make sure the HHOT folder exists
                    if (!Directory.Exists(getHHOTfolder()))
                        Directory.CreateDirectory(getHHOTfolder());
                    // then extract the default files
                    File.WriteAllText(getSettingsFile(), Properties.Resources.VariableSettings);
                    File.WriteAllText(getHouseholdFile(), Properties.Resources.HouseholdData);
                    File.WriteAllText(getReferenceTablesFile(), Properties.Resources.ReferenceTables);
                    // finally, read the newly created settings
                    readSettings();
                }
                catch (Exception e)
                {
                    MessageBox.Show("Unable to generate default settings!" + Environment.NewLine + e.Message + Environment.NewLine + e.InnerException.Message);
                }
            }
        }

        private static void CopyTableContent(DataTable newTbl, DataTable oldTbl, bool createTable = false, bool fixEditorType = false)
        {
            if (fixEditorType && oldTbl.Columns.Contains("EditorType"))
            {
                oldTbl.Columns["EditorType"].ColumnName = "VariableType";
            }
            if (createTable)    // create a new table
            {
                foreach (DataColumn c in oldTbl.Columns)
                    newTbl.Columns.Add(c.ColumnName, c.DataType);
            }
            else    // clear an existing table and add missing columns
            {
                newTbl.Clear();
                foreach (DataColumn c in oldTbl.Columns)
                    if (!newTbl.Columns.Contains(c.ColumnName))
                        newTbl.Columns.Add(c.ColumnName, c.DataType);
            }

            foreach (DataRow r in oldTbl.Rows)  // copy all rows of the old Table into the new Table (assuming they share structure)
            {
                DataRow nr = newTbl.NewRow();
                if (fixEditorType)
                {
                    foreach (DataColumn c in oldTbl.Columns)
                    {
                        if (c.ColumnName == "VariableType")
                        {
                            nr[c.ColumnName] = (r[c.ColumnName].ToString() == OLD_EDITOR_TYPE_NUMERIC ? EDITOR_TYPE_NUMERIC : (r[c.ColumnName].ToString() == OLD_EDITOR_TYPE_COMBO ? EDITOR_TYPE_COMBO : (r[c.ColumnName].ToString() == OLD_EDITOR_TYPE_CONNECTION ? EDITOR_TYPE_CONNECTION : r[c.ColumnName])));
                        }
                        else
                            nr[c.ColumnName] = r[c.ColumnName];
                    }
                }
                else
                    foreach (DataColumn c in oldTbl.Columns)
                        nr[c.ColumnName] = r[c.ColumnName];
                newTbl.Rows.Add(nr);
            }
        }

        internal static DevExpress.XtraEditors.Repository.RepositoryItemComboBox getVariableTypeCombo()
        {
            DevExpress.XtraEditors.Repository.RepositoryItemComboBox combo = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox() { TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor };
            combo.Items.Add(EDITOR_TYPE_NUMERIC);
            combo.Items.Add(EDITOR_TYPE_COMBO);
            combo.Items.Add(EDITOR_TYPE_CONNECTION);
            return combo;
        }

        internal void readHouseholdDataFrom(string filename)
        {
            if (householdData != null)
            {
                householdData.Tables.Clear();
                if (householdData.ExtendedProperties != null) householdData.ExtendedProperties.Clear();
            }
            else
            {
                householdData = new DataSet();
            }

            householdData.ReadXml(filename);

            AdaptHouseholdDataToSettings();
        }

        internal void AdaptHouseholdDataToSettings(DataSet hhData = null)
        {
            if (hhData == null) hhData = householdData;
            addMissingVariables(hhData);
            removeExtraVariables(hhData);
            if (CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator != GetDecimalSeparator(hhData))
            {
                if (hhData.Tables.Count > 50)
                    MessageBox.Show("Household data's decimal separator needs to be adapted to system's separator, this may take some time ...");
                fixHouseholdDecimalSeparator(hhData);
                SetDecimalSeparator(hhData);
            }
            fixPrivateKey(hhData);
            hhData.AcceptChanges();
        }

        internal void fixPrivateKey(DataSet hhData = null)
        {
            foreach (DataTable tbl in (hhData == null ? householdData : hhData).Tables)
            {
                if (tbl.TableName != HOUSEHOLD_STRUCTURE_TABLE)
                {
                    tbl.PrimaryKey = new DataColumn[] { tbl.Columns["dataId"] };
                }
            }
        }

        internal void readHouseholdData()
        {
            if (File.Exists(getHouseholdFile()))
            {
                // If there is a settings file, read the settings from there
                readHouseholdDataFrom(getHouseholdFile());
            }
            else
            {
                householdData = new DataSet() { DataSetName = "HouseholdSettings" };
                // If the settings file does not exist, create the default values
                householdData.Tables.Add(generateHouseholdStructureData(householdData));
                addNewHousehold("Example Household");
            }

//            return data;
        }

        internal bool isRestrictedTableName(string tn)
        {
            return tn == HOUSEHOLD_STRUCTURE_TABLE;
        }

        private void fixSettingsDecimalSeparator()
        {
            fixVariablesDecimalSeparator(settingsData);
            fixRefTablesDecimalSeparator(referenceTablesData);
        }
        internal void fixVariablesDecimalSeparator(VariableDataSet sd)
        {
            string curSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string badSep = curSep == "." ? "," : ".";

            foreach (VariableDataSet.Cur_AdvancedVariablesRow r in sd.Cur_AdvancedVariables.Rows)
                r.DefaultValue = r.DefaultValue.Replace(badSep, curSep);
            foreach (VariableDataSet.Cur_AdvancedCountrySpecificDetailRow r in sd.Cur_AdvancedCountrySpecificDetail.Rows)
                r.DefaultValue = r.DefaultValue.Replace(badSep, curSep);
            foreach (VariableDataSet.Cur_DerivedVariablesRow r in sd.Cur_DerivedVariables.Rows)
                r.DefaultValue = r.DefaultValue.Replace(badSep, curSep);
            foreach (VariableDataSet.Cur_DerivedVariablesDetailRow r in sd.Cur_DerivedVariablesDetail.Rows)
                r.DerivedValue = r.DerivedValue.Replace(badSep, curSep);
        }
        internal void fixRefTablesDecimalSeparator(ReferenceTablesDataSet rt)
        {
            string curSep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            string badSep = curSep == "." ? "," : ".";

            foreach (DataRow rtRow in rt.ReferenceTables.Rows)
            {
                DataTable reftbl = rt.Tables[rtRow.Field<string>("TableName")];
                foreach (DataRow r in reftbl.Rows)
                    foreach (DataColumn c in reftbl.Columns)
                    {
                        string d = r[c].ToString().Replace(badSep, curSep);
                        r[c] = d == "" ? "0" : d;
                    }
            }
        }

        internal void fixHouseholdDecimalSeparator(DataSet hhData = null)
        {
            string curSep = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            string badSep = curSep == "." ? "," : ".";

            foreach (DataTable datatbl in (hhData == null ? householdData : hhData).Tables)
            {
                if (!isRestrictedTableName(datatbl.TableName))
                {
                    foreach (DataColumn c in datatbl.Columns)
                    {
                        foreach (DataRow r in datatbl.Rows)
                        {
                            if (r[c] != null && r[c].ToString().StartsWith("Value|"))
                            {
                                r[c] = r[c].ToString().Replace(badSep, curSep);
                            }
                        }
                    }
                }
            }
        }

        internal bool automaticlyUpdateReferenceTables()
        {
            // This function will automatically update all the Reference Tables to make them match the current list of Coutries/Years
            // ATTENTION: This will automatically ammend AND SAVE all Reference Tables
            string msg;
            if (!adaptYearsAndCountriesOfReferenceTables(out msg)) return false;
            saveReferenceTables();
            MessageBox.Show(msg);
            return true;
        }

        internal bool adaptYearsAndCountriesOfReferenceTables(out string message, ReferenceTablesDataSet refTabData = null)
        {
            if (refTabData == null) refTabData = referenceTablesData;

            List<string> addedCountries = new List<string>();
            List<string> addedYears = new List<string>();
            List<string> removedCountries = new List<string>();
            List<string> removedYears = new List<string>();

            List<DataRow> addedCountryRows = new List<DataRow>();
            List<DataColumn> addedYearCols = new List<DataColumn>();

            // First read the current list of Countries & Years
            List<string> countries = new List<string>();
            foreach (DataRow r in settingsData.Cur_Countries.Rows) if (r.RowState != DataRowState.Deleted) countries.Add(r.Field<string>("Country"));
            List<string> years = new List<string>();
            years.Add("Country");
            foreach (DataRow r in settingsData.Cur_Years.Rows) if (r.RowState != DataRowState.Deleted) years.Add(r.Field<string>("Year"));

            // Then open each table and update it as required
            bool wasUpdated = false;
            foreach (DataRow rtRow in refTabData.ReferenceTables.Rows)
            {
                DataTable reftbl = refTabData.Tables[rtRow.Field<string>("TableName")];

                // Delete and add rows (countries)
                List<DataRow> delRows = new List<DataRow>();
                foreach (DataRow r in reftbl.Rows)
                {
                    if (!countries.Contains(r.Field<string>("Country")))
                    {
                        delRows.Add(r);
                        removedCountries.Add(r.Field<string>("Country"));
                        wasUpdated = true;
                    }
                }
                foreach (DataRow r in delRows) reftbl.Rows.Remove(r);
                foreach (string country in countries)
                {
                    if (reftbl.Select("Country = '" + country + "'").Length == 0)
                    {
                        DataRow newrow = reftbl.NewRow();
                        newrow["Country"] = country;    // add the new country 
                        //foreach (string year in years) if (year != "Country") newrow[year] = 0; // and set values to 0, moved downwards, see below
                        reftbl.Rows.Add(newrow);
                        addedCountries.Add(country);
                        addedCountryRows.Add(newrow);
                        wasUpdated = true;
                    }
                }

                // Delete and add columns (years)
                List<DataColumn> delCols = new List<DataColumn>();
                foreach (DataColumn c in reftbl.Columns)
                {
                    if (!years.Contains(c.ColumnName))
                    {
                        delCols.Add(c);
                        removedYears.Add(c.ColumnName);
                        wasUpdated = true;
                    }
                }
                foreach (DataColumn c in delCols) reftbl.Columns.Remove(c);
                foreach (string year in years)
                {
                    if (!reftbl.Columns.Contains(year))
                    {
                        DataColumn newcol = reftbl.Columns.Add(year, typeof(double));
                        //foreach (DataRow r in reftbl.Rows) r[year] = 0; // moved downwards, see below
                        addedYears.Add(year);
                        addedYearCols.Add(newcol);
                        wasUpdated = true;
                    }
                }

                // in an imported reference table countries and years may both require adaptation, thus do the zero-setting at the very end
                foreach (DataRow addedCountryRow in addedCountryRows)
                    foreach (string year in years) if (year != "Country") addedCountryRow[year] = 0;
                foreach (DataColumn addedYearCol in addedYearCols)
                    foreach (DataRow r in reftbl.Rows) r[addedYearCol.ColumnName] = 0;
            }

            message = "All reference tables have been automatically updated:";
            if (addedCountries.Count > 0) message += Environment.NewLine + " - Countries added (values set to zero): " + string.Join(", ", addedCountries.ToArray());
            if (addedYears.Count > 0) message += Environment.NewLine + " - Years added (values set to zero): " + string.Join(", ", addedYears.ToArray());
            if (removedCountries.Count > 0) message += Environment.NewLine + " - Countries removed: " + string.Join(", ", removedCountries.ToArray());
            if (removedYears.Count > 0) message += Environment.NewLine + " - Years removed: " + string.Join(", ", removedYears.ToArray());

            return wasUpdated;
        }

        internal void addMissingVariables(DataSet hhData = null)
        {
            // Try to add any missing fields in households as appropriate
            foreach (DataTable datatbl in (hhData == null ? householdData : hhData).Tables)
            {
                if (!isRestrictedTableName(datatbl.TableName))
                {
                    // Note that Derived variables don't need to be stored in the HH file, as they are always calculated on output generation
                    foreach (DataTable tbl in new DataTable[] { settingsData.Cur_BasicVariables, settingsData.Cur_AdvancedVariables, settingsData.Cur_AdvancedCountrySpecificVariables })
                    {
                        foreach (DataRow fld in tbl.Rows)
                        {
                            if (tbl == settingsData.Cur_BasicVariables)
                            {
                                if (!datatbl.Columns.Contains(fld.Field<string>("VariableName")))
                                    datatbl.Columns.Add(fld.Field<string>("VariableName"));
                            }
                            else if (tbl == settingsData.Cur_AdvancedVariables)
                            {
                                if (!datatbl.Columns.Contains(fld.Field<string>("VariableName")))
                                {
                                    datatbl.Columns.Add(fld.Field<string>("VariableName"));
                                    foreach (DataRow r in datatbl.Rows)
                                        r.SetField(fld.Field<string>("VariableName"), fld.Field<string>("DefaultValue"));
                                }
                            }
                            else if (tbl == settingsData.Cur_AdvancedCountrySpecificVariables)
                            {
                                foreach (DataRow fld1 in settingsData.Cur_AdvancedCountrySpecificDetail.Select("VariableName = '" + fld.Field<string>("VariableName") + "'"))
                                {
                                    string n = fld.Field<string>("VariableName") + "_" + fld1.Field<string>("Country");
                                    if (!datatbl.Columns.Contains(n))
                                    {
                                        datatbl.Columns.Add(n);
                                        foreach (DataRow r in datatbl.Rows)
                                            r.SetField(n, fld1.Field<string>("DefaultValue"));
                                    }
                                }
                            }
                            else
                            {
                                // this should never happen! 
                                MessageBox.Show("Unknown Variable type: " + fld.Field<string>("VariableName"));
                            }
                        }
                    }
                }
            }
        }

        internal void removeExtraVariables(DataSet hhData = null)
        {
            List<string> reservedNames = new List<string>() { "dataId", "PersonName", "checked", "orderId" };
            
            // Try to remove any excess fields in households as appropriate:
            // first use any HH-DataTable for checking and then apply the removal to all HH-DataTables,
            // because checking takes quite long and must have the same result for all HH-DataTables
            DataTable datatbl = null;
            foreach (DataTable dt in (hhData == null ? householdData : hhData).Tables)
                if (!isRestrictedTableName(dt.TableName)) { datatbl = dt; break; }
            if (datatbl == null) return; // must be empty project

            List<string> removeCols = new List<string>();
            // for each variable in each household table
            foreach (DataColumn col in datatbl.Columns)
            {
                string varName = col.ColumnName;
                bool found = false;
                // first check if this is a reserved field
                if (reservedNames.Contains(varName)) found = true;

                // then first try to match this with a Basic, Advanced or AdvancedCoutnrySpecific variable
                if (!found)
                {
                    foreach (DataTable tbl in new DataTable[] { settingsData.Cur_BasicVariables, settingsData.Cur_AdvancedVariables, settingsData.Cur_AdvancedCountrySpecificVariables })
                    {
                        if (tbl.Select("VariableName = '" + varName + "'").Length > 0)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                // then try to match it with an AdvancedCountrySpecificDetail variable
                if (!found)
                {
                    foreach (DataRow cr in settingsData.Cur_Countries)
                    {
                        string c = cr.Field<string>("Country");
                        if (varName.EndsWith(c))
                        {
                            string vName = varName.Substring(0, varName.Length - (c.Length + 1));
                            if (settingsData.Cur_AdvancedCountrySpecificDetail.Select("VariableName = '" + vName + "' AND Country='" + c + "'").Length > 0)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }

                // if variable was not matched, add to the remove list
                if (!found)
                    removeCols.Add(varName);
            }

            // finally remove all tagged columns
            foreach (DataTable dt in (hhData == null ? householdData : hhData).Tables)
            {
                if (!isRestrictedTableName(dt.TableName))
                {
                    foreach (string c in removeCols)
                        if (dt.Columns.Contains(c))
                            dt.Columns.Remove(c);
                }
            }
        }

        internal bool addNewHousehold(string name)
        {
            if (name == "")
            {
                MessageBox.Show("You have to give a name for the new Household!");
            }
            else if (isRestrictedTableName(name))
            {
                MessageBox.Show("This name is not allowed!");
            }
            else if (householdData.Tables.Contains(name))
            {
                MessageBox.Show("This Household name already exists!");
            }
            else 
            {
                // Everything is OK, so really add the new Household
                DataTable grid = createNewHousehold(name);
                householdData.Tables.Add(grid);
                // and update the structure tree
                DataTable householdStructureTable = householdData.Tables[HOUSEHOLD_STRUCTURE_TABLE];
                int tid = getNewRandomId(householdStructureTable, "ID");
                householdStructureTable.Rows.Add(new object[] { tid, -1, true, true, name, householdStructureTable.Select("ParentID=-1").Count(), tid });  // add the new Household to the tree
                addDefaultPerson(grid, "Example Member");      // then add a default person
                return true;
            }
            return false;
        }

        internal bool duplicateHousehold(string oldName, string newName)
        {
            if (newName == "")
            {
                MessageBox.Show("You have to give a name for the new Household!");
            }
            else if (isRestrictedTableName(newName))
            {
                MessageBox.Show("This name is not allowed!");
            }
            else if (householdData.Tables[newName] != null)
            {
                MessageBox.Show("This Household name already exists!");
            }
            else
            {
                // Everything is OK, so really duplicate the new Household in the DataSet
                copyHousehold(oldName, newName);
                return true;
            }
            return false;
        }

        internal void copyHousehold(string oldName, string newName, DataSet importData = null)
        {
            if (importData == null) importData = householdData;

            DataTable grid = importData.Tables[oldName].Copy();
            grid.TableName = newName;
            // But then you need to change all the IDs!!! To do this, first create a list of all members and calculate their new ID (note that here there is an improbable chance that you create the same ID twice!! we chose to ignore this for now)
            Dictionary<int, int> IDs = new Dictionary<int, int>();
            foreach (DataRow r in grid.Rows)
            {
                int oldId = r.Field<int>("dataId");
                IDs.Add(oldId, getNewRandomId(householdData.Tables[HOUSEHOLD_STRUCTURE_TABLE], "dataId"));
            }
            // Then go through the household members and change the IDs
            foreach (DataRow r in grid.Rows)
            {
                foreach (DataColumn c in grid.Columns)
                {
                    if (c.ColumnName == "dataId")
                    {
                        r[c] = IDs[r.Field<int>(c)];    // replace the old ID with the new one
                    }
                    else
                    {
                        int val;
                        if (int.TryParse(r[c].ToString(), out val)) // if the column can be translated to a numeric value (note that ALL editable fields are stored as strings for editting purposes)
                        {
                            if (IDs.ContainsKey(val))
                                r[c] = IDs[val].ToString();    // replace the old ID with the new one
                        }
                    }
                }
            }
            householdData.Tables.Add(grid);

            // and also duplicate the branch in the structure tree
            DataTable householdStructureTable = householdData.Tables[HOUSEHOLD_STRUCTURE_TABLE];
            DataTable importStructureTable = importData.Tables[HOUSEHOLD_STRUCTURE_TABLE];
            DataRow row = importStructureTable.Select("HouseholdName='" + oldName + "' AND ParentID=-1")[0];
            int tid = getNewRandomId(householdStructureTable, "ID");
            householdStructureTable.Rows.Add(new object[] { tid, -1, row.Field<bool>("isChecked"), row.Field<bool>("isExpanded"), newName, householdStructureTable.Select("ParentID=-1").Count(), tid });  // add the new Household to the tree

            DataRow[] rows = importStructureTable.Select("ParentID='" + row.Field<int>("ID") + "'");
            foreach (DataRow r in rows)  // then add all its inhabitants
            {
                int nid = getNewRandomId(householdStructureTable, "ID");
                householdStructureTable.Rows.Add(new object[] { nid, tid, r.Field<bool>("isChecked"), r.Field<bool>("isExpanded"), r.Field<string>("HouseholdName"), householdStructureTable.Select("ParentID=" + tid).Count(), IDs[r.Field<int>("dataId")] });
            }
        }
        
        internal bool duplicatePerson(DataTable grid, string oldName, string newName)
        {
            if (newName == "")
            {
                MessageBox.Show("Please give a valid name for the new Person.");
            }
            else if (isRestrictedTableName(newName))
            {
                MessageBox.Show("This name is not allowed!");
            }
            else if (grid.Select("PersonName='" + oldName + "'").Length == 0)
            {
                MessageBox.Show("The Person you are trying to duplicate does not exist!");
            }
            else if (grid.Select("PersonName='" + newName + "'").Length > 0)
            {
                MessageBox.Show("This name already exists!");
            }
            else
            {
                DataRow oldRow = grid.Select("PersonName='" + oldName + "'")[0];
                int dataId = getNewRandomId(grid, "dataId");
                DataRow newrow = grid.NewRow();
                newrow.SetField("dataId", dataId);
                newrow.SetField("PersonName", newName);
                newrow.SetField("orderId", grid.Rows.Count);
                foreach (DataColumn col in grid.Columns)
                    if (col.ColumnName != "dataId" && col.ColumnName != "PersonName" && col.ColumnName != "orderId")
                        newrow.SetField(col.ColumnName, oldRow[col.ColumnName]);
                grid.Rows.Add(newrow);

                DataTable householdStructureTable = householdData.Tables[HOUSEHOLD_STRUCTURE_TABLE];
                int tid = householdStructureTable.Select("HouseholdName='" + grid.TableName + "'")[0].Field<int>("ID");
                int nid = getNewRandomId(householdStructureTable, "ID");
                householdStructureTable.Rows.Add(new object[] { nid, tid, true, false, newName, householdStructureTable.Select("ParentID=" + tid).Count(), dataId });
                return true;
            }
            return false;
        }

        internal bool resetHouseholdAdvancedVariables(DataTable grid, bool resetAll = false)
        {
            bool allOK = true;
            foreach (DataRow row in grid.Rows)
                allOK = allOK && resetPersonAdvancedVariables(grid, row.Field<int>("dataId"), resetAll, false);
            return allOK;
        }

        internal bool resetPersonAdvancedVariables(DataTable grid, int dataId, bool resetAll = false, bool copyHouseholdValues = true)
        {
            DataRow[] rows = grid.Select("dataId='" + dataId + "'");
            if (rows.Length != 1) return false;
            DataRow row = rows[0];
            using (NumericEditor ne = new NumericEditor(this))
            {
                ne.setSingleValue("0");
                foreach (DataTable tbl in new DataTable[] { settingsData.Cur_BasicVariables, settingsData.Cur_AdvancedCountrySpecificVariables, settingsData.Cur_AdvancedVariables })
                {
                    foreach (DataRow r in tbl.Rows)
                    {
                        if (!row.Table.Columns.Contains(r.Field<string>("VariableName"))) row.Table.Columns.Add(r.Field<string>("VariableName"));
                        if (resetAll && tbl == settingsData.Cur_BasicVariables)
                        {
                            if (copyHouseholdValues && r.Field<bool>("IsHouseholdVar") && grid.Rows.Count > 1)
                            {
                                DataRow copyRow = grid.Select("dataId<>'" + dataId + "'")[0];
                                row.SetField(r.Field<string>("VariableName"), copyRow.Field<string>(r.Field<string>("VariableName")));
                            }
                            else
                            {
                                if (r.Field<string>("VariableType") == EDITOR_TYPE_COMBO || r.Field<string>("VariableType") == EDITOR_TYPE_CONNECTION)
                                    row.SetField(r.Field<string>("VariableName"), "");
                                else if (r.Field<string>("VariableType") == EDITOR_TYPE_NUMERIC)
                                    row.SetField(r.Field<string>("VariableName"), ne.EditValue);
                            }
                        }
                        else if (tbl == settingsData.Cur_AdvancedCountrySpecificVariables)
                        {
                            foreach (DataRow fld in settingsData.Cur_AdvancedCountrySpecificDetail.Select("VariableName = '" + r.Field<string>("VariableName") + "'"))
                            {
                                if (!row.Table.Columns.Contains(r.Field<string>("VariableName") + "_" + fld.Field<string>("Country"))) row.Table.Columns.Add(r.Field<string>("VariableName") + "_" + fld.Field<string>("Country"));
                                if (copyHouseholdValues && r.Field<bool>("IsHouseholdVar") && grid.Rows.Count > 1)
                                {
                                    DataRow copyRow = grid.Select("dataId<>'" + dataId + "'")[0];
                                    row.SetField(r.Field<string>("VariableName") + "_" + fld.Field<string>("Country"), copyRow.Field<string>(r.Field<string>("VariableName") + "_" + fld.Field<string>("Country")));
                                }
                                else
                                {
                                    row.SetField(r.Field<string>("VariableName") + "_" + fld.Field<string>("Country"), fld.Field<string>("DefaultValue"));
                                }
                            }
                        }
                        else if (tbl == settingsData.Cur_AdvancedVariables)
                        {
                            if (copyHouseholdValues && r.Field<bool>("IsHouseholdVar") && grid.Rows.Count > 1)
                            {
                                DataRow copyRow = grid.Select("dataId<>'" + dataId + "'")[0];
                                row.SetField(r.Field<string>("VariableName"), copyRow.Field<string>(r.Field<string>("VariableName")));
                            }
                            else
                            {
                                row.SetField(r.Field<string>("VariableName"), r.Field<string>("DefaultValue"));
                            }
                        }
                    }
                }
            }

            return true;
        }

        internal bool addDefaultPerson(DataTable grid, string name)
        {
            if (name == "")
            {
                MessageBox.Show("You have to give a name for the new Person!");
            }
            else if (grid.Select("PersonName='"+name+"'").Count() > 0)
            {
                MessageBox.Show("This Person name already exists in the Household!");
            }
            else
            {
                int dataId = getNewRandomId(grid, "dataId");
                DataRow newrow = grid.NewRow();
                newrow.SetField("dataId", dataId);
                newrow.SetField("PersonName", name);
                newrow.SetField("checked", true);
                newrow.SetField("orderId", grid.Rows.Count);

                grid.Rows.Add(newrow);

                resetPersonAdvancedVariables(grid, dataId, true);

                DataTable householdStructureTable = householdData.Tables[HOUSEHOLD_STRUCTURE_TABLE];
                int tid = householdStructureTable.Select("HouseholdName='" + grid.TableName + "'")[0].Field<int>("ID");
                int nid = getNewRandomId(householdStructureTable, "ID");
                householdStructureTable.Rows.Add(new object[] { nid, tid, true, false, name, householdStructureTable.Select("ParentID=" + tid).Count(), dataId });
                return true;
            }
            return false;
        }

        internal DataTable createNewHousehold(string name)
        {
            // If table exists, return existing table
            if (householdData != null && householdData.Tables[name] != null) return householdData.Tables[name];
            // Else create the table with a single person
            DataTable gridData = new DataTable(name);

            gridData.PrimaryKey = new DataColumn[] { gridData.Columns.Add("dataId", typeof(int)) };
            gridData.Columns.Add("PersonName", typeof(string));
            gridData.Columns.Add("checked", typeof(bool));
            gridData.Columns.Add("orderId", typeof(int));

            foreach (DataTable tbl in new DataTable[] { settingsData.Cur_BasicVariables, settingsData.Cur_AdvancedCountrySpecificVariables, settingsData.Cur_AdvancedVariables })
            {
                foreach (DataRow r in tbl.Rows)
                {
                    if (r.Field<string>("VariableType") == EDITOR_TYPE_COMBO || r.Field<string>("VariableType") == EDITOR_TYPE_CONNECTION || r.Field<string>("VariableType") == EDITOR_TYPE_NUMERIC)
                    {
                        if (!gridData.Columns.Contains(r.Field<string>("VariableName")))
                            gridData.Columns.Add(r.Field<string>("VariableName"), typeof(string));
                    }
                }
            }
            return gridData;
        }

        private DataTable generateHouseholdStructureData(DataSet _dataSet)
        {
            DataTable structureTable = new DataTable();
            structureTable.TableName = HOUSEHOLD_STRUCTURE_TABLE;
            structureTable.PrimaryKey = new DataColumn[] { structureTable.Columns.Add("ID", typeof(int)) };
            structureTable.Columns.Add("ParentID", typeof(int));
            structureTable.Columns.Add("isChecked", typeof(bool));
            structureTable.Columns.Add("isExpanded", typeof(bool));
            structureTable.Columns.Add("HouseholdName", typeof(string));
            structureTable.Columns.Add("orderId", typeof(int));
            structureTable.Columns.Add("dataId", typeof(int));
            int i = 0;
            foreach (DataTable t in _dataSet.Tables)
            {
                int tid = getNewRandomId(structureTable, "ID");
                structureTable.Rows.Add(new object[] { tid, -1, true, false, t.TableName, i++, tid });
                foreach (DataRow r in t.Rows)
                {
                    int nid = getNewRandomId(structureTable, "ID");
                    structureTable.Rows.Add(new object[] { nid, tid, true, false, r.Field<string>("PersonName"), r.Field<int>("orderId"), r.Field<int>("dataId") });
                }
            }
            return structureTable;
        }

        private int getNewRandomId(DataTable tbl, string field)
        {
            int id;
            do {
                id = randomGenerator.Next();
            }
            while (id < 10000000 && tbl != null && tbl.Select(field + "='" + id + "'").Count() > 0);  // the id < 10000000 is to ensure that all our IDs have at least 8 digits for safety reasons (we do some find/replace that could affect other values when duplicating households)
            return id;
        }

        internal void saveReferenceTables()
        {
            referenceTablesData.AcceptChanges();
            referenceTablesData.WriteXml(getReferenceTablesFile(), XmlWriteMode.WriteSchema);
        }

        internal void saveSettings()
        {
            settingsData.AcceptChanges();
            settingsData.WriteXml(getSettingsFile(), XmlWriteMode.WriteSchema);
        }

        internal bool IsHHotProjectPath(string path, out string error)
        {
            error = string.Empty;
            if (!File.Exists(getSettingsFile(path))) error = "File not found: " + getSettingsFile(path) + Environment.NewLine;
            if (!File.Exists(getReferenceTablesFile(path))) error += "File not found: " + getReferenceTablesFile(path) + Environment.NewLine;
            if (!File.Exists(getHouseholdFile(path))) error += "File not found: " + getHouseholdFile(path) + Environment.NewLine;
            error.TrimEnd();
            return error == string.Empty;
        }


        internal void openProject(string path)
        {
            try
            {
                string error;
                if (!IsHHotProjectPath(path, out error)) { MessageBox.Show("Failed to open project:" + Environment.NewLine + error); return; }

                currentProjectPath = path;
                readSettings();
                inputForm.readHouseholds(getHouseholdFile(path));
                inputForm.Text = GetTitleText();
                inputForm.ShowHideSaveAsPuplicMenu();
            }
            catch (Exception exception) { MessageBox.Show("Failed to open project:" + Environment.NewLine + exception.Message); }
        }

        private string GetTitleText()
        {
            return "Hypothetical Household Tool - " + currentProjectPath;
        }

        internal void saveProjectAs(string path, bool saveAsPublic)
        {
            try
            {
                // handle settings
                if (!saveAsPublic) // note: changes in settings do not need to be saved, as all Manage-dialogs enforce saving ...
                    File.Copy(getSettingsFile(), getSettingsFile(path)); // ... thus just copy the file ...
                else // ... however, if a public version is saved, settings need to be changed (i.e. private variables deleted) and saved
                    writePublicSettings(path);
                File.Copy(getReferenceTablesFile(), getReferenceTablesFile(path));

                // handle household data: save the current state at the new place, thus all unsaved changes (only) go there
                householdData.WriteXml(getHouseholdFile(path), XmlWriteMode.WriteSchema);

                // update
                currentProjectPath = path;
                inputForm.Text = GetTitleText();
                inputForm.ShowHideSaveAsPuplicMenu();
            }
            catch (Exception exception) { MessageBox.Show("Failed to save project:" + Environment.NewLine + exception.Message); }
        }

        private void writePublicSettings(string path)
        {
            const string PRIVATE = "Private";
            foreach (DataTable tbl in new DataTable[] { settingsData.Cur_BasicVariables, settingsData.Cur_AdvancedVariables, settingsData.Cur_AdvancedCountrySpecificVariables })
            {
                for (int r = tbl.Rows.Count - 1; r >= 0; --r)
                    if (tbl.Rows[r][PRIVATE] != System.DBNull.Value && tbl.Rows[r].Field<bool>(PRIVATE))
                        tbl.Rows.RemoveAt(r);
            }
            if (settingsData.General.Rows.Count == 0) settingsData.General.AddGeneralRow(true);
            else settingsData.General.Rows[0].SetField<bool>(PRIVATE, true);
            settingsData.AcceptChanges();
            settingsData.WriteXml(getSettingsFile(path), XmlWriteMode.WriteSchema); // save settings at the path for the public project
        }

        internal bool isPublicVersion()
        {
            return settingsData.General.Rows.Count == 0 ? false : settingsData.General.Rows[0].Field<bool>("PublicVersion");
        }

        internal void saveDefaultSettings()
        {
            if (MessageBox.Show("Are you sure you want to overwrite the Default Settings?" + Environment.NewLine + Environment.NewLine + "This action cannot be undone.", "Save Defaults", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // copy the contents of "current" tables into "default" tables
                foreach (DataTable tbl in settingsData.Tables)
                    if (tbl.TableName.StartsWith("Cur_"))
                        CopyTableContent(settingsData.Tables["Def_" + tbl.TableName.Substring(4)], tbl);
                
                saveSettings();
                MessageBox.Show("Your new defaults have been saved in the file:" + Environment.NewLine + getSettingsFile());
            }
        }

        internal void restoreDefaultSettings()
        {
            if (MessageBox.Show("Are you sure you want to restore the Default Settings?" + Environment.NewLine + Environment.NewLine + "This action cannot be undone.", "Restore Defaults", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // copy the contents of "current" tables into "default" tables
                foreach (DataTable tbl in settingsData.Tables)
                    if (tbl.TableName.StartsWith("Def_"))
                        CopyTableContent(settingsData.Tables["Cur_" + tbl.TableName.Substring(4)], tbl);

                saveSettings();
                MessageBox.Show("The default settings have been restored.");
            }
        }

        internal void saveHouseholdData()
        {
            householdData.AcceptChanges();
            householdData.WriteXml(getHouseholdFile(), XmlWriteMode.WriteSchema);
        }

        internal void saveHouseholdDataAs(string filename)
        {
            householdData.WriteXml(filename, XmlWriteMode.WriteSchema);
            //currentlyOpenFile = filename;
        }

        public override string GetTitle()
        {
            // This is the text you will see in the Toolbar
            return "Hypothetical Household";
        }

        public override string GetDescription()
        {
            // This is the plugin description (will show in a tooltip when you hover the mouse over the toolbar button)
			return "This Plug-in generates Household Data.";
        }

        public override string GetFullFileName()
        {
            // This is the full path and will be used to extract the image for the toolbar button
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public override bool IsWebApplicable()
        {
            return false;
        }

        // store the used decimal sign in the extended properties of the HouseholdData, this way unnecessary (and slow) converting can be avoided
        private const string DATA_PROP_DEC_SEP = "DecimalSeparator";
        private string GetDecimalSeparator(DataSet dataSet) // this may also be used for import-data, thus have dataSet as parameter
        {
            return dataSet.ExtendedProperties != null && dataSet.ExtendedProperties.Contains(DATA_PROP_DEC_SEP)
                ? dataSet.ExtendedProperties[DATA_PROP_DEC_SEP].ToString() : null;
        }
        private void SetDecimalSeparator(DataSet hhData)
        {
            if (hhData == null) hhData = householdData;
            string separator = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            if (hhData.ExtendedProperties.ContainsKey(DATA_PROP_DEC_SEP))
                hhData.ExtendedProperties[DATA_PROP_DEC_SEP] = separator;
            else hhData.ExtendedProperties.Add(DATA_PROP_DEC_SEP, separator);
        }
    }

    #region Helper classes

    internal class Family
    {
        public int type { get; set; }
        public List<int> members { get; set; }
        public Family(int _type, int[] _members)
        {
            type = _type;
            members = new List<int>(_members);
        }
    }

    internal class GenderOptions
    {
        public int[] code = { 0, 1 };
        public string[] desc = { "Female", "Male" };
    }

    #endregion

}
