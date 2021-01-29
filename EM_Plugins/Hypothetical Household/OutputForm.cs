using EM_Common_Win;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class OutputForm : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        InputForm inputForm;
        BackgroundWorker bw;
        DataGenerator dataGenerator;
        DataGenerator.AllFileGenerationDetails generationDetails;
        List<string> errorVars;

        public OutputForm(Program _plugin, InputForm _inputForm)
        {
            Plugin = _plugin;
            inputForm = _inputForm;
            InitializeComponent();
        }

        private void OutputForm_Shown(object sender, EventArgs e)
        {
            dataGenerator = new DataGenerator(inputForm, Plugin);
            if (dataGenerator.getAllFileDetails(out generationDetails, out List<string> errorVars))  // if there was no error
            {
                // create the generation message and ask permission to continue
                string msg = "You are about to generate ";
                if (generationDetails.totalFiles == 1) msg += "a single file.\n\nThis";
                else
                {
                    msg += "a total of " + generationDetails.totalFiles + " files for ";
                    msg += (generationDetails.totalCountries == 1 ? "one country" : generationDetails.totalCountries + " countries") + " and ";
                    msg += (generationDetails.totalYears == 1 ? "one year" : generationDetails.totalYears + " years") + ".\n\nThese";
                }
                msg += " will contain the total amount of ";
                msg += (generationDetails.totalPeople == 1 ? "one person" : generationDetails.totalPeople + " people") + " in ";
                msg += (generationDetails.totalHouseholds == 1 ? "one household" : generationDetails.totalHouseholds + " households") + ".\n\n";
                msg += "Are you sure you want to continue?";

                textBoxOutput.Text = UISessionInfo.GetInputFolder() ?? "";

                labelMainMessage.Text = msg;
            }
            else
            {
                Close();
                MessageBox.Show("Aborting generation. The following errors were found:\n" + String.Join("\n", errorVars));
            }
        }

        private void buttonGenerate_Click(object sender, EventArgs e)
        {
            if (textBoxOutput.Text == null || textBoxOutput.Text == "" || !Directory.Exists(textBoxOutput.Text))
            {
                MessageBox.Show("Please select a valid Output folder");
                return;
            }

            panel1.Hide();
            panel2.Location = new Point(0, 0);
            panel2.Show();
            buttonGenerate.Hide();
            progressBar1.Minimum = 0;
            progressBar1.Maximum = (int)generationDetails.totalPeople;
            updateProgress();
            Refresh();

            bw = new BackgroundWorker();
            bw.WorkerSupportsCancellation = true;
            bw.WorkerReportsProgress = true;
            bw.DoWork += bw_DoWork;
            bw.RunWorkerCompleted += bw_RunWorkerCompleted;
            bw.ProgressChanged += bw_ProgressChanged;
            bw.RunWorkerAsync();
        }

        void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            updateProgress();
        }

        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            // generate the file(s)
            dataGenerator.generateAllData(textBoxOutput.Text, generationDetails, out errorVars, sender as BackgroundWorker);
            if ((sender as BackgroundWorker).CancellationPending) e.Cancel = true;
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Cancelled!");
            }
            else if (dataGenerator.countPeople < generationDetails.totalPeople)
            {
                MessageBox.Show("Finished with errors. Some households were not generated!");
            }
            else
            {
                MessageBox.Show("Finished!");
                if (errorVars.Count > 0)
                {
                    string msg = "The following Derived variable" + (errorVars.Count > 1 ? "s" : "") + " caused errors and got default values:";
                    if (errorVars.Count > 20)
                        msg += "(total errors: " + errorVars.Count + ", only the first 20 are displayed here)";
                    msg += Environment.NewLine + Environment.NewLine;
                    for (int i = 0; i < Math.Min(20, errorVars.Count); i++)
                        msg += errorVars[i] + Environment.NewLine;
                    msg += Environment.NewLine + "These errors can seriously affect the generation speed!.";
                    MessageBox.Show(msg);
                }
            }
            Close();
        }

        private void updateProgress()
        {
            updateFiles();
            updateHouseholds();
            updatePeople();
        }

        private void updateFiles()
        {
            labelFiles.Text = "Files: " + dataGenerator.countFiles + " out of " + generationDetails.totalFiles;
            labelFiles.Refresh();
        }

        private void updateHouseholds()
        {
            labelHouseholds.Text = "Households: " + dataGenerator.countHouseholds + " out of " + generationDetails.totalHouseholds;
            labelHouseholds.Refresh();
        }

        private void updatePeople()
        {
            labelPeople.Text = "People: " + dataGenerator.countPeople + " out of " + generationDetails.totalPeople;
            labelPeople.Refresh();
            progressBar1.Value = (int)dataGenerator.countPeople;
            progressBar1.Refresh();
        }

        private void buttonChangePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = textBoxOutput.Text;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxOutput.Text = fbd.SelectedPath;
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (bw != null && bw.IsBusy) bw.CancelAsync();
            Close();
        }

//        private AllFileGenerationDetails getAllFileDetails()
//        {
//            AllFileGenerationDetails allDetails = new AllFileGenerationDetails();

//            allDetails.totalCountries = inputForm.countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count;
//            allDetails.totalYears = inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count;
//            allDetails.totalFiles = allDetails.totalCountries * allDetails.totalYears;
//            allDetails.fileDetails = new Dictionary<string, FileGenerationDetails>();

//            foreach (string country in inputForm.countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//            {
//                allDetails.fileDetails.Add(country, new FileGenerationDetails());

//                allDetails.fileDetails[country].allVars = new Dictionary<string, string>();
//                allDetails.fileDetails[country].numericVars = new List<string>();
//                allDetails.fileDetails[country].connectionVars = new List<string>();
//                allDetails.fileDetails[country].derivedVars = new Dictionary<string, DerivedInfo>();

//                // First add all the Basic Variables
//                foreach (VariableDataSet.Cur_BasicVariablesRow var in Plugin.settingsData.Cur_BasicVariables.Rows)
//                {
//                    DataRow[] varCS = Plugin.settingsData.Cur_BasicCountrySpecificDetail.Select("VariableName='" + var.VariableName + "' AND Countries Like '*" + country + "*'");
//                    if (varCS != null && varCS.Length > 0)  // if there is a country-specific sub-variable add this
//                    {
//                        foreach (DataRow r in varCS)
//                        {
//                            string csvn = r.Field<string>("CountrySpecificVariableName");
//                            if (!allDetails.fileDetails[country].allVars.ContainsKey(csvn))
//                                allDetails.fileDetails[country].allVars.Add(csvn, var.VariableName);
//                            else
//                            {

//                                addErrorMessage("Duplicate Variable '" + csvn + "' found in " + country + ".");
//                            }
//                        }
//                    }
//                    else    // else add the generic variable
//                    {
//                        if (!allDetails.fileDetails[country].allVars.ContainsKey(var.VariableName))
//                            allDetails.fileDetails[country].allVars.Add(var.VariableName, var.VariableName);
//                        else
//                        {
//                            addErrorMessage("Duplicate Variable '" + var.VariableName + "' found in " + country + ".");
//                        }
//                    }
//                    if (var.VariableType == Program.EDITOR_TYPE_NUMERIC) allDetails.fileDetails[country].numericVars.Add(var.VariableName);
//                    else if (var.VariableType == Program.EDITOR_TYPE_CONNECTION) allDetails.fileDetails[country].connectionVars.Add(var.VariableName);
//                }

//                // Then add all the Advanced Country Specific Variables
//                foreach (VariableDataSet.Cur_AdvancedCountrySpecificDetailRow var in Plugin.settingsData.Cur_AdvancedCountrySpecificDetail.Select("Country = '" + country + "'"))
//                {
//                    if (!allDetails.fileDetails[country].allVars.ContainsKey(var.VariableName))
//                        allDetails.fileDetails[country].allVars.Add(var.VariableName, var.VariableName + "_" + country);
//                    else
//                    {
//                        addErrorMessage("Duplicate Variable '" + var.VariableName + "' found in " + country + ".");
//                    }
//                    if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.FindByVariableName(var.VariableName).VariableType == Program.EDITOR_TYPE_NUMERIC) allDetails.fileDetails[country].numericVars.Add(var.VariableName + "_" + country);
//                    else if (Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.FindByVariableName(var.VariableName).VariableType == Program.EDITOR_TYPE_CONNECTION) allDetails.fileDetails[country].connectionVars.Add(var.VariableName + "_" + country);
//                }

//                // Then add all the Advanced Variables
//                foreach (VariableDataSet.Cur_AdvancedVariablesRow var in Plugin.settingsData.Cur_AdvancedVariables.Select("Countries Like '*" + country + "*'"))
//                {
//                    if (!allDetails.fileDetails[country].allVars.ContainsKey(var.VariableName))
//                        allDetails.fileDetails[country].allVars.Add(var.VariableName, var.VariableName);
//                    else
//                    {
//                        addErrorMessage("Duplicate Variable '" + var.VariableName + "' found in " + country + ".");
//                    }
//                    if (var.VariableType == Program.EDITOR_TYPE_NUMERIC) allDetails.fileDetails[country].numericVars.Add(var.VariableName);
//                    else if (var.VariableType == Program.EDITOR_TYPE_CONNECTION) allDetails.fileDetails[country].connectionVars.Add(var.VariableName);
//                }

//                // Finally add all the Derived Variables
//                foreach (VariableDataSet.Cur_DerivedVariablesRow var in Plugin.settingsData.Cur_DerivedVariables.Select("Countries Like '*" + country + "*'"))
//                {
//                    if (!allDetails.fileDetails[country].allVars.ContainsKey(var.VariableName))
//                        allDetails.fileDetails[country].allVars.Add(var.VariableName, var.VariableName);
//                    else
//                    {
//                        addErrorMessage("Duplicate Variable '" + var.VariableName + "' found in " + country + ".");
//                    }
//                    DerivedInfo di = new DerivedInfo();
//                    di.varName = var.VariableName;
//                    di.defaultValue = var.DefaultValue;
//                    di.conditionalValues = new Dictionary<string,string>();
//                    foreach (VariableDataSet.Cur_DerivedVariablesDetailRow dvar in Plugin.settingsData.Cur_DerivedVariablesDetail.Select("VariableName = '"+var.VariableName+"'"))
//                    {
//                        di.conditionalValues.Add (dvar.Condition, dvar.DerivedValue);
//                    }
//                    allDetails.fileDetails[country].derivedVars.Add(var.VariableName, di);

//                }

//                allDetails.fileDetails[country].numericVarValues = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, double>>>>();
//                allDetails.fileDetails[country].rangeVarValues = new Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<double>>>>>();
                
//                allDetails.fileDetails[country].totalHouseholds = 0;
//                allDetails.fileDetails[country].totalPeople = 0;


//                // we will use the editors to parse the values
//                NumericEditor numericEditor = new NumericEditor(Plugin);

//                foreach (TreeListNode fam in inputForm.treeHouseholds.Nodes)
//                {
//                    if (fam.Checked)
//                    {
//                        long famCounter = 1;
//                        string famName = fam.GetValue("HouseholdName").ToString();
//                        allDetails.fileDetails[country].numericVarValues.Add(famName, new Dictionary<string, Dictionary<string, Dictionary<string, double>>>());
//                        allDetails.fileDetails[country].rangeVarValues.Add(famName, new Dictionary<string, Dictionary<string, Dictionary<string, List<double>>>>());
//                        DataTable tbl = Plugin.householdData.Tables[famName];
//                        long pInFam = 0;
//                        foreach (TreeListNode p in fam.Nodes)
//                        {
//                            if (p.Checked)
//                            {
//                                pInFam++;
//                                string pName = p.GetValue("HouseholdName").ToString();
//                                allDetails.fileDetails[country].numericVarValues[famName].Add(pName, new Dictionary<string, Dictionary<string, double>>());
//                                allDetails.fileDetails[country].rangeVarValues[famName].Add(pName, new Dictionary<string, Dictionary<string, List<double>>>());
//                                DataRow row = tbl.Rows.Find(p.GetValue("dataId"));

//                                // parse all the ranged Numeric values
//                                foreach (string rv in allDetails.fileDetails[country].numericVars)
//                                {
//                                    if (row[rv] != null)
//                                    {
//                                        numericEditor.EditValue = row.Field<string>(rv);

//                                        if (numericEditor.isRangedValue())
//                                        {
//                                            if (numericEditor.getStartingValue() < numericEditor.getEndingValue() && numericEditor.getStepValue()>0)
//                                            {
//                                                if (allDetails.fileDetails[country].rangeVarValues[famName][pName].ContainsKey(rv))
//                                                    continue; // this duplication of a variable name is already recognised by getAllFileDetails, i.e. the Generate will fail
//                                                allDetails.fileDetails[country].rangeVarValues[famName][pName].Add(rv, new Dictionary<string, List<double>>());
//                                                double max = numericEditor.getEndingValue();
//                                                double step = numericEditor.getStepValue();
//                                                if (numericEditor.isReference())
//                                                {
//                                                    string refTableName = numericEditor.getReferenceTable();
//                                                    DataTable refTable = Plugin.referenceTablesData.Tables[refTableName];
//                                                    if (refTable == null)
//                                                    {
//                                                        addErrorMessage("Reference Table for variable " + rv + " not found!");
//                                                        break;
//                                                    }
//                                                    DataRow refRow = refTable.Select("Country = '" + country + "'")[0];

//                                                    foreach (string year in inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//                                                    {
//                                                        double val = numericEditor.getStartingValue();
//                                                        double rf = refRow[year] == DBNull.Value ? 0 : refRow.Field<double>(year);
//                                                        allDetails.fileDetails[country].rangeVarValues[famName][pName][rv].Add(year, new List<double>());
//                                                        double realValue;
//                                                        while (val < max)
//                                                        {
//                                                            realValue = val * rf / 100;
//                                                            allDetails.fileDetails[country].rangeVarValues[famName][pName][rv][year].Add(realValue);
//                                                            val += step;
//                                                        }
//                                                        realValue = max * rf / 100;
//                                                        allDetails.fileDetails[country].rangeVarValues[famName][pName][rv][year].Add(realValue);
//                                                    }
//                                                }
//                                                else
//                                                {
//                                                    foreach (string year in inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//                                                    {
//                                                        double val = numericEditor.getStartingValue();
//                                                        allDetails.fileDetails[country].rangeVarValues[famName][pName][rv].Add(year, new List<double>());
//                                                        while (val < max)
//                                                        {
//                                                            allDetails.fileDetails[country].rangeVarValues[famName][pName][rv][year].Add(val);
//                                                            val += step;
//                                                        }
//                                                        allDetails.fileDetails[country].rangeVarValues[famName][pName][rv][year].Add(max);
//                                                    }
//                                                }
//                                                famCounter *= allDetails.fileDetails[country].rangeVarValues[famName][pName][rv][inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues()[0].ToString()].Count;
//                                            }
//                                            else
//                                            {
//                                                addErrorMessage("Wrong range value in variable '" + rv + "' for member '" + pName + "' in '" + famName + "'.");
//                                            }
//                                        }
//                                        else    // treat as non-range Numeric value
//                                        {
//                                            if (allDetails.fileDetails[country].numericVarValues[famName][pName].ContainsKey(rv))
//                                                continue; // this duplication of a variable name is already recognised by getAllFileDetails, i.e. the Generate will fail
//                                            allDetails.fileDetails[country].numericVarValues[famName][pName].Add(rv, new Dictionary<string, double>());
//                                            if (numericEditor.isReference())
//                                            {
//                                                string refTableName = numericEditor.getReferenceTable();
//                                                DataTable refTable = Plugin.referenceTablesData.Tables[refTableName];
//                                                if (refTable == null)
//                                                {
//                                                    addErrorMessage("Reference Table for variable " + rv + " not found!");
//                                                    break;
//                                                }
//                                                DataRow refRow = refTable.Select("Country = '" + country + "'")[0];

//                                                foreach (string year in inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//                                                {
//                                                    double rf = refRow[year] == DBNull.Value ? 0 : double.Parse(refRow[year].ToString());
//                                                    double realValue = numericEditor.getStartingValue() * rf / 100;
//                                                    allDetails.fileDetails[country].numericVarValues[famName][pName][rv].Add(year, realValue);
//                                                }
//                                            }
//                                            else
//                                            {
//                                                foreach (string year in inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//                                                {
//                                                    allDetails.fileDetails[country].numericVarValues[famName][pName][rv].Add(year, numericEditor.getStartingValue());
//                                                }
//                                            }
//                                        }
//                                    }
//                                }
//                            }
//                        }
//                        allDetails.fileDetails[country].totalHouseholds += famCounter;
//                        allDetails.fileDetails[country].totalPeople += famCounter * pInFam;
//                    }
//                }

//                allDetails.totalHouseholds += allDetails.fileDetails[country].totalHouseholds * allDetails.totalYears;
//                allDetails.totalPeople += allDetails.fileDetails[country].totalPeople * allDetails.totalYears;
//            }
//            return allDetails;
//        }

//        private void generateAllData(string outputFolder, AllFileGenerationDetails generationDetails)
//        {
//            foreach (string country in inputForm.countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//            {
//                foreach (string year in inputForm.yearsCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
//                {
//                    generateCountryYearData(outputFolder, country, year, generationDetails.fileDetails[country]);
//                    countFiles++;
//                    if (bw.CancellationPending) break;
//                    bw.ReportProgress(0);
//                }
//                if (bw.CancellationPending) break;
//            }
//        }

//        private void generateCountryYearData(string outputFolder, string country, string year, FileGenerationDetails fileDetails)
//        {
//            string filename = country.ToUpper() + "_" + year + "_hhot.txt";

//            StreamWriter sw;

//            try
//            {
//                sw = new StreamWriter(Path.Combine(outputFolder, filename), false, DefGeneral.DEFAULT_ENCODING);
//            }
//            catch (Exception)
//            {
//                string msg = "Cannot access \"" + Path.Combine(outputFolder, filename) + "\"! Please make sure ";
//                if (File.Exists(Path.Combine(outputFolder, filename))) msg += "it is not write-protected, or locked by another application.";
//                else msg += "you have permission to create this file.";
//                MessageBox.Show(msg);
//                return;
//            }

//            try
//            {
////                Stopwatch watch = Stopwatch.StartNew();
////                long watch_timer1 = 0;
////                long watch_timer2 = 0;
////                long watch_timer3 = 0;

//                List<string> line = new List<string>();
//                line.Add("idhhtype");   // household type id
//                line.Add("idhh");       // household id
//                line.Add("idperson");   // person id
//                line.Add("dwt");        // weight
//                line.Add("dhr");        // household responsible
//                foreach (string v in fileDetails.allVars.Keys) line.Add(v);
//                sw.WriteLine(String.Join("\t", line));
//                long famId = 0;

//                Regex matchText = new Regex(@"([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
//                Regex matchTextStar = new Regex(@"(?<start>[a-z]+)\*", RegexOptions.IgnoreCase | RegexOptions.Compiled);
//                Regex matchTextStarText = new Regex(@"(?<start>[a-z]+)\*(?<end>[a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);


//                List<string> incomeVars = new List<string>(new string[] { "yem", "yse", "poa" });
//                foreach (TreeListNode fam in inputForm.treeHouseholds.Nodes)
//                {
//                    if (fam.Checked)
//                    {
//                        string famTypeId = fam.GetValue("dataId").ToString();
//                        string famName = fam.GetValue("HouseholdName").ToString();
//                        Dictionary<string, int> curValue = new Dictionary<string, int>();
//                        foreach (string p in fileDetails.rangeVarValues[famName].Keys)
//                            foreach (string v in fileDetails.rangeVarValues[famName][p].Keys)
//                                curValue.Add(famName + "_" + p + "_" + v, 0);
//                        bool keepLooping = true;
//                        DataTable tbl = Plugin.householdData.Tables[famName];
//                        while (keepLooping && !bw.CancellationPending)
//                        {
//                            famId++;
//                            long perId = 0;
//                            Dictionary<string, string> pids = new Dictionary<string, string>();
//                            // get the household members and find the household responsible
//                            foreach (TreeListNode p in fam.Nodes)
//                            {
//                                if (p.Checked)
//                                {
//                                    perId++;
//                                    pids.Add(p.GetValue("dataId").ToString(), famId.ToString() + perId.ToString("00"));
//                                }
//                            }
//                            perId = 0;

//                            TreeListNode hr = null;
//                            double maxIncome = 0;
//                            foreach (TreeListNode p in fam.Nodes)
//                            {
//                                if (p.Checked)
//                                {
//                                    string pName = p.GetValue("HouseholdName").ToString();
//                                    Dictionary<string, Dictionary<string, double>> pNumeric = fileDetails.numericVarValues[famName][pName];
//                                    Dictionary<string, Dictionary<string, List<double>>> pRanged = fileDetails.rangeVarValues[famName][pName];
//                                    DataRow row = tbl.Rows.Find(p.GetValue("dataId"));
//                                    if (hr == null)
//                                    {
//                                        hr = p;
//                                        double total = 0;
//                                        foreach (string v in incomeVars)
//                                        {
//                                            if (pRanged.ContainsKey(v))
//                                                total += pRanged[v][year][curValue[famName + "_" + pName + "_" + v]];
//                                            else if (pNumeric.ContainsKey(v))
//                                                total += pNumeric[v][year];
//                                            else
//                                                total += row[v] == DBNull.Value ? 0 : double.Parse(row[v].ToString());
//                                        }
//                                        maxIncome = total;
//                                    }
//                                    else
//                                    {
//                                        double total = 0;
//                                        foreach (string v in incomeVars)
//                                        {
//                                            if (pRanged.ContainsKey(v))
//                                                total += pRanged[v][year][curValue[famName + "_" + pName + "_" + v]];
//                                            else if (pNumeric.ContainsKey(v))
//                                                total += pNumeric[v][year];
//                                            else
//                                                total += row[v] == DBNull.Value ? 0 : double.Parse(row[v].ToString());
//                                        }
//                                        if (total > maxIncome)
//                                        {
//                                            hr = p;
//                                            maxIncome = total;
//                                        }
//                                        else if (total == maxIncome)
//                                        {
//                                            string pname2 = hr.GetValue("HouseholdName").ToString();
//                                            double age1 = 0, age2 = 0;
//                                            if (pRanged.ContainsKey("dag")) age1 = pRanged["dag"][year][curValue[famName + "_" + pName + "_dag"]];
//                                            else if (pNumeric.ContainsKey("dag")) age1 = pNumeric["dag"][year];
//                                            if (fileDetails.rangeVarValues[famName][pname2].ContainsKey("dag")) age2 = fileDetails.rangeVarValues[famName][pname2]["dag"][year][curValue[famName + "_" + pname2 + "_dag"]];
//                                            else if (fileDetails.numericVarValues[famName][pname2].ContainsKey("dag")) age2 = fileDetails.numericVarValues[famName][pname2]["dag"][year];

//                                            if (age1 > age2) hr = p;
//                                        }
//                                    }
//                                }
//                            }

//                            foreach (TreeListNode p in fam.Nodes)
//                            {
//                                if (p.Checked)
//                                {
//                                    string pName = p.GetValue("HouseholdName").ToString();
//                                    Dictionary<string, Dictionary<string, List<double>>> pRanged = fileDetails.rangeVarValues[famName][pName];
//                                    Dictionary<string, Dictionary<string, double>> pNumeric = fileDetails.numericVarValues[famName][pName];

//                                    perId++;
//                                    line.Clear();
//                                    line.Add(famTypeId);
//                                    line.Add(famId.ToString());
//                                    line.Add(pids[p.GetValue("dataId").ToString()]);
//                                    line.Add("1");  // add "dwt"
//                                    line.Add(p == hr ? "1" : "0");  // add "dhr"
//                                    DataRow row = tbl.Rows.Find(p.GetValue("dataId"));
//                                    foreach (string var in fileDetails.allVars.Values)
//                                    {
//                                        string v = var;
//                                        if (!row.Table.Columns.Contains(v) && v.EndsWith("_" + country))
//                                            v = v.Substring(0, v.Length - ("_" + country).Length);
//                                        if (pRanged.ContainsKey(v))     // if it is a ranged variable
//                                        {
//                                            line.Add(pRanged[v][year][curValue[famName + "_" + pName + "_" + v]].ToString());
//                                        }
//                                        else if (pNumeric.ContainsKey(v))   // if it is a numeric variable
//                                        {
//                                            line.Add(pNumeric[v][year].ToString());
//                                        }
//                                        else if (fileDetails.connectionVars.Contains(v))    // if it is a connection variable
//                                        {
//                                            line.Add(row[v] == DBNull.Value || row[v].ToString() == "" || !pids.ContainsKey(row[v].ToString()) ? "0" : pids[row[v].ToString()]);
//                                        }
//                                        else if (fileDetails.derivedVars.ContainsKey(v))    // if it is a derived variable
//                                        {
//                                            // find the correct expression to calculate
//                                            string expression = "";
//                                            if (fileDetails.derivedVars[v].conditionalValues.Count > 0)
//                                            {
//                                                foreach (string cond in fileDetails.derivedVars[v].conditionalValues.Keys)
//                                                {
//                                                    // check if condition is true
//                                                    int pos = cond.IndexOfAny(ComparisonSymbols);
//                                                    if (pos < 0)
//                                                    {
//                                                        addErrorMessage("Invalid condition in derived variable condition! Variable '" + v + "' -> condition '" + cond + "' was invalid.");
//                                                        break;
//                                                    }
//                                                    string varName = cond.Substring(0, pos).Trim();
//                                                    if (!fileDetails.allVars.ContainsKey(varName))
//                                                    {
//                                                        addErrorMessage("Invalid variable in derived variable condition! Variable '" + v + "' -> condition variable '" + varName + "' was not found.");
//                                                        break;
//                                                    }

//                                                    varName = fileDetails.allVars[varName];
//                                                    int endPos = pos + 1;
//                                                    while (ComparisonSymbols.Contains(cond[endPos])) endPos++;

//                                                    string comparer = cond.Substring(pos, endPos - pos);

//                                                    double compValue;
//                                                    if (!double.TryParse(cond.Substring(endPos + 1).Trim(), out compValue))
//                                                    {
//                                                        addErrorMessage("Invalid condition in derived variable condition! Variable '" + v + "' -> condition '" + cond + "' was invalid.");
//                                                        break;
//                                                    }

//                                                    double varValue;
//                                                    if (pRanged.ContainsKey(varName))
//                                                    {
//                                                        varValue = pRanged[varName][year][curValue[famName + "_" + pName + "_" + varName]];
//                                                    }
//                                                    else if (pNumeric.ContainsKey(varName))
//                                                    {
//                                                        varValue = pNumeric[varName][year];
//                                                    }
//                                                    else
//                                                    {
//                                                        varValue = row[varName] == DBNull.Value ? 0 : double.Parse(row[varName].ToString());
//                                                    }

//                                                    switch (comparer)
//                                                    {
//                                                        case ">": if (varValue > compValue) expression = fileDetails.derivedVars[v].conditionalValues[cond]; break;
//                                                        case "=": if (varValue == compValue) expression = fileDetails.derivedVars[v].conditionalValues[cond]; break;
//                                                        case "<": if (varValue < compValue) expression = fileDetails.derivedVars[v].conditionalValues[cond]; break;
//                                                        case "<=": if (varValue <= compValue) expression = fileDetails.derivedVars[v].conditionalValues[cond]; break;
//                                                        case ">=": if (varValue >= compValue) expression = fileDetails.derivedVars[v].conditionalValues[cond]; break;
//                                                        case "<>": if (varValue != compValue) expression = fileDetails.derivedVars[v].conditionalValues[cond]; break;
//                                                        default:
//                                                            addErrorMessage("Invalid comparer in derived variable condition! Variable '" + v + "' -> comparer '" + comparer + "' was invalid.");
//                                                            continue;
//                                                    }
//                                                    if (expression != "") break;
//                                                }
//                                            }
//                                            if (expression == "")   // if no condition matched, go to default value
//                                            {
//                                                expression = fileDetails.derivedVars[v].defaultValue;
//                                            }

//                                            // Match any variables in the expression and replace with their values
//                                            if (matchText.Match(expression).Success)   // if there are varName, search & replace accordingly
//                                            {
//                                                MatchCollection allMatches = matchTextStarText.Matches(expression);
//                                                foreach (Match match in allMatches)
//                                                {
//                                                    string realValue;
//                                                    double totalValue = 0;
//                                                    foreach (string s in fileDetails.allVars.Keys)
//                                                    {
//                                                        if (s.StartsWith(match.Groups["start"].Value) && s.EndsWith(match.Groups["end"].Value))
//                                                        {
//                                                            string baseVar = fileDetails.allVars[s];
//                                                            if (pRanged.ContainsKey(baseVar))
//                                                                totalValue += pRanged[baseVar][year][curValue[famName + "_" + pName + "_" + baseVar]];
//                                                            else if (pNumeric.ContainsKey(baseVar))
//                                                                totalValue += pNumeric[baseVar][year];
//                                                        }
//                                                    }

//                                                    foreach (DataColumn col in row.Table.Columns)
//                                                    {
//                                                        if (!(pRanged.Keys.Contains(col.ColumnName) || pNumeric.Keys.Contains(col.ColumnName))
//                                                            && fileDetails.allVars.ContainsKey(col.ColumnName)
//                                                            && col.ColumnName.StartsWith(match.Groups["start"].Value) && col.ColumnName.EndsWith(match.Groups["end"].Value))
//                                                        {
//                                                            totalValue += row[col.ColumnName] == DBNull.Value ? 0 : double.Parse(row[col.ColumnName].ToString());
//                                                        }
//                                                    }
//                                                    realValue = totalValue.ToString();
//                                                    expression = expression.Replace(match.Value, realValue);
//                                                }
//                                                allMatches = matchTextStar.Matches(expression);
//                                                foreach (Match match in allMatches)
//                                                {
//                                                    string realValue;
//                                                    double totalValue = 0;
//                                                    foreach (string s in fileDetails.allVars.Keys)  // add ALL matches
//                                                    {
//                                                        if (s.StartsWith(match.Groups["start"].Value))
//                                                        {
//                                                            string baseVar = fileDetails.allVars[s];
//                                                            if (pRanged.ContainsKey(baseVar))
//                                                            {
//                                                                totalValue += pRanged[baseVar][year][curValue[famName + "_" + pName + "_" + baseVar]];
//                                                            }
//                                                            else if (pNumeric.ContainsKey(baseVar))
//                                                            {
//                                                                totalValue += pNumeric[baseVar][year];
//                                                            }
//                                                        }
//                                                    }
//                                                    foreach (DataColumn col in row.Table.Columns)
//                                                    {
//                                                        if (!(pRanged.Keys.Contains(col.ColumnName) || pNumeric.Keys.Contains(col.ColumnName))
//                                                            && fileDetails.allVars.ContainsKey(col.ColumnName)
//                                                            && col.ColumnName.StartsWith(match.Groups["start"].Value))
//                                                        {
//                                                            totalValue += row[col.ColumnName] == DBNull.Value ? 0 : double.Parse(row[col.ColumnName].ToString());
//                                                        }
//                                                    }
//                                                    realValue = totalValue.ToString();
//                                                    expression = expression.Replace(match.Value, realValue);
//                                                }
//                                                allMatches = matchText.Matches(expression);
//                                                foreach (Match match in allMatches)
//                                                {
//                                                    string realValue;
//                                                    if (fileDetails.allVars.ContainsKey(match.Value))
//                                                    {
//                                                        string baseVar = fileDetails.allVars[match.Value];
//                                                        if (pRanged.ContainsKey(match.Value))
//                                                        {
//                                                            realValue = pRanged[match.Value][year][curValue[famName + "_" + pName + "_" + match.Value]].ToString();
//                                                        }
//                                                        else if (pRanged.ContainsKey(baseVar))
//                                                        {
//                                                            realValue = pRanged[baseVar][year][curValue[famName + "_" + pName + "_" + baseVar]].ToString();
//                                                        }
//                                                        else if (pNumeric.ContainsKey(match.Value))
//                                                        {
//                                                            realValue = pNumeric[match.Value][year].ToString();
//                                                        }
//                                                        else if (pNumeric.ContainsKey(baseVar))
//                                                        {
//                                                            realValue = pNumeric[baseVar][year].ToString();
//                                                        }
//                                                        else if (row.Table.Columns.Contains(match.Value))
//                                                        {
//                                                            realValue = row[match.Value] == DBNull.Value ? "0" : row[match.Value].ToString();
//                                                        }
//                                                        else
//                                                        {
//                                                            realValue = row[baseVar] == DBNull.Value ? "0" : row[baseVar].ToString();
//                                                        }
//                                                    }
//                                                    else realValue = "0";
//                                                    expression = expression.Replace(match.Value, realValue);
//                                                }
//                                                /**/
//                                            }

//                                            // calculate the total value of the expression
//                                            try
//                                            {
//                                                var result = new DataTable().Compute(expression.Replace(",", "."), null);
//                                                line.Add(result.ToString());
//                                            }
//                                            catch
//                                            {
//                                                addErrorMessage(v);
//                                                line.Add("0");
//                                            }
//                                        }
//                                        else    // else it should be a categorical variable
//                                        {
//                                            if (row.Table.Columns.Contains(v))
//                                            {
//                                                line.Add(row[v].ToString());
//                                            }
//                                            else    // this is an error!
//                                            {
//                                                MessageBox.Show("Something went wrong! Variable '"+v+"' -> '"+var+"' was not found.");
//                                                line.Add("0");
//                                            }
//                                        }
//                                    }
//                                    sw.WriteLine(String.Join("\t", line).Replace(",", "."));    // note that output should always be with "." as this is expected by EUROMOD
//                                    countPeople++;
//                                }
//                            }

//                            // Choose next loop element
//                            int vDepth = 0;
//                            int pDepth = 0;
//                            bool tryAgain = true;
//                            while (tryAgain && pDepth < fileDetails.rangeVarValues[famName].Count)
//                            {
//                                string pn = fileDetails.rangeVarValues[famName].ElementAt(pDepth).Key;
//                                if (fileDetails.rangeVarValues[famName][pn].Count == 0)
//                                {
//                                    pDepth++;
//                                }
//                                else
//                                {

//                                    string vn = fileDetails.rangeVarValues[famName][pn].ElementAt(vDepth).Key;
//                                    curValue[famName + "_" + pn + "_" + vn]++;
//                                    if (curValue[famName + "_" + pn + "_" + vn] >= fileDetails.rangeVarValues[famName][pn][vn][year].Count)
//                                    {
//                                        curValue[famName + "_" + pn + "_" + vn] = 0;
//                                        vDepth++;
//                                        if (vDepth >= fileDetails.rangeVarValues[famName][pn].Count)
//                                        {
//                                            vDepth = 0;
//                                            pDepth++;
//                                        }
//                                    }
//                                    else tryAgain = false;
//                                }
//                            }
//                            if (pDepth >= fileDetails.rangeVarValues[famName].Count) keepLooping = false;

//                            countHouseholds++;
//                            if (countHouseholds % 8 == 0) bw.ReportProgress(0);
//                        }
//                    }
//                }
//            }
//            finally
//            {
//                sw.Close();
//            }
//        }

//        class AllFileGenerationDetails
//        {
//            internal long totalHouseholds;
//            internal long totalPeople;
//            internal long totalCountries;
//            internal long totalYears;
//            internal long totalFiles;
//            internal Dictionary<string, FileGenerationDetails> fileDetails;
//        }

//        class FileGenerationDetails
//        {
//            internal long totalHouseholds;
//            internal long totalPeople;
//            internal Dictionary<string, string> allVars;
//            internal List<string> connectionVars;
//            internal List<string> numericVars;
//            internal Dictionary<string, DerivedInfo> derivedVars;
//            // dict<family, dict<person, dict<varName, dict<year, value>>>>
//            internal Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, double>>>> numericVarValues;
//            // dict<family, dict<person, dict<varName, dict<year, List<value>>>>>
//            internal Dictionary<string, Dictionary<string, Dictionary<string, Dictionary<string, List<double>>>>> rangeVarValues;
//            // dict<family, dict<person, dict<varName, dict<year, value>>>>
//        }

//        class DerivedInfo
//        {
//            internal string varName;
//            internal string defaultValue;
//            internal Dictionary<string, string> conditionalValues;
//        }

    }
}
