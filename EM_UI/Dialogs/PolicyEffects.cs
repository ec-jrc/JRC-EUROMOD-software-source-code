using DevExpress.XtraEditors;
using EM_Common;
using EM_Crypt;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ImportExport;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class PolicyEffects : Form
    {
        private const string FactorCPI = "$HICP";
        internal EM_UI_MainForm _mainForm = null;
        DataTable policyDataTable;
        List<SystemBackgroundWorker> workers = new List<SystemBackgroundWorker>();
        private string logText = string.Empty;
        Dictionary<string, Tuple<string,string>> runCountries = new Dictionary<string,Tuple<string,string>>(); // key: country-short-name; value: item1:system1, item2:system2
        int totalSystems = 0;
        Dictionary<string, double> alphaValues = new Dictionary<string,double>();
        
        List<double> alphaFIXValues = new List<double>(); // contains usually one fix alpha, but can contain a range of alphas

        bool showFull = false;
        bool showResults = false;

        internal PolicyEffects(EM_UI_MainForm mainForm)
        {
            showFull = EnvironmentInfo.ShowComponent("PET_advanced");
            InitializeComponent();
            Height = 475;
            Width = 640;
            Text = "Policy Effects tool";

            try
            {
                _mainForm = mainForm;

                // add F1/F5/F6 support to all form controls
                foreach (Control c in this.Controls)
                {
                    c.KeyDown += this.PolicyEffects_KeyDown;
                    if (c.HasChildren) foreach (Control c1 in c.Controls) c1.KeyDown += this.PolicyEffects_KeyDown;
                }
                
                textBoxOutputPath.Text = EMPath.AddSlash(EM_AppContext.FolderOutput);
                comboBox1.ValueMember = "value";
                comboBox1.DisplayMember = "text";
                comboBox2.ValueMember = "value";
                comboBox2.DisplayMember = "text";
                repositoryItemComboBoxData.Enter += repositoryItemComboBoxData_Enter;
                repositoryItemComboBoxSys1.Enter += repositoryItemComboBoxSys1_Enter;
                repositoryItemComboBoxSys2.Enter += repositoryItemComboBoxSys2_Enter;

            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        #region INPUT
        void repositoryItemComboBoxData_Enter(object sender, EventArgs e)
        {
            ComboBoxEdit cb = sender as ComboBoxEdit;
            if (cb == null) return;
            cb.Properties.Items.Clear();
            cb.Properties.Items.Add("n/a");
            DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
            DataRow row = view.GetDataRow(view.FocusedRowHandle);
            foreach (DataConfig.DataBaseRow dataSet in EM_UI.CountryAdministration.CountryAdministrator.GetDataConfigFacade(row["Country"].ToString()).GetDataBaseRows())
            {
                cb.Properties.Items.Add(dataSet.Name);
            }
        }

        void repositoryItemComboBoxSys1_Enter(object sender, EventArgs e) { repositoryItemComboBoxSys_Enter(sender, 1); }
        void repositoryItemComboBoxSys2_Enter(object sender, EventArgs e) { repositoryItemComboBoxSys_Enter(sender, 2); }
        void repositoryItemComboBoxSys_Enter(object sender, int col)
        {
            ComboBoxEdit cb = sender as ComboBoxEdit;
            if (cb == null) return;
            cb.Properties.Items.Clear();
            cb.Properties.Items.Add("n/a");
            DevExpress.XtraGrid.Views.Grid.GridView view = gridControl1.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView;
            DataRow row = view.GetDataRow(view.FocusedRowHandle);

            string year = col == 1 ? comboBox1.Text : comboBox2.Text;
            foreach (CountryConfig.SystemRow sr in EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(row["Country"].ToString()).GetSystemRows())
            {
                if (sr.Year == year) cb.Properties.Items.Add(sr.Name);
            }
        }

        private Dictionary<string, bool> ExpandIncomeList(string il_name, CountryConfig.FunctionRow[] il_funcs, bool addPart = true)
        {
            Dictionary<string, bool> il_members = new Dictionary<string, bool>();
            il_name = il_name.ToLower();
            // first get all direct childs
            foreach (CountryConfig.FunctionRow il in il_funcs)
            {
                if (il.Name.ToLower() != "defil") continue;
                if (il.GetParameterRows().First(p => p.Name.ToLower() == "name").Value.ToLower() == il_name)
                {
                    foreach (CountryConfig.ParameterRow pr in il.GetParameterRows())
                        if (pr.Name.ToLower() != "name" && pr.Value != "n/a") il_members.Add(pr.Name.ToLower(), pr.Value==(addPart?"+":"-"));
                }
            }
            // then recursively add any deeper childs
            foreach (string ilm in il_members.Keys)
            {
                try
                {
                    il_members = il_members.Concat(ExpandIncomeList(ilm, il_funcs, il_members[ilm])).ToDictionary(x => x.Key, x => x.Value);
                }
                catch (ArgumentException)
                {
                    em3_petInfo.AddSystemIndependentError($"error expanding incomlist: trying to add '{il_members.Last().Key}' twice");
                }
            }
            return il_members;
        }

        private Dictionary<string, string> GetRawCPIindices(CountryConfig.SystemRow sr, string countryShortName)
        {
            Dictionary<string, string> rawIndices = new Dictionary<string, string>();

            HICPConfigFacade hcf = EM_AppContext.Instance.GetHICPConfigFacade(false);
            if (hcf != null) // if golbal HICP table exists first try to get the $HICPs from this file and only if not existent (for a year) use the local definition (if existent)
            {
                foreach (HICPConfig.HICPRow globalHICP in hcf.GetHICPs())
                    if (globalHICP.Country.ToLower() == countryShortName.ToLower() && !rawIndices.ContainsKey(globalHICP.Year.ToString()))
                        rawIndices.Add(globalHICP.Year.ToString(), globalHICP.Value.ToString());
            }

            /////////////////////////////////////////////////////////////////////////////////////////////////////
            // This part replaces what I have backed-up in function
            // GetRawCPIindices_OldApproach(sr, countryShortName, rawIndices);
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(countryShortName);
            CountryConfig.UpratingIndexRow localHICP = ccf.GetUpratingIndex(FactorCPI);
            if (localHICP != null)
                foreach (var yv in ccf.GetUpratingIndexYearValues(localHICP))
                    if (!rawIndices.ContainsKey(yv.Key.ToString())) // only add if not "overwritten" by HICPConfig
                        rawIndices.Add(yv.Key.ToString(), yv.Value.ToString());
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            
            return rawIndices;
        }

        private Dictionary<string,string> GetUpratingFactors(CountryConfig.SystemRow sr, DataConfig.DataBaseRow dataset, string countryShortName)
        {
            Dictionary<string, string> upratingFactors = new Dictionary<string, string>();
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            // This part replaces what I have backed-up in function
            // GetUpratingFactors_OldApproach(sr, dataset.Name, countryShortName, upratingFactors);
            /////////////////////////////////////////////////////////////////////////////////////////////////////
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(countryShortName);
            string systemYear = sr.Year != null && sr.Year != string.Empty ? sr.Year : EM_Helpers.ExtractSystemYear(sr.Name);
            string dataYear = dataset.YearInc;

            // first get $HICP
            Dictionary<string, string> HICP = GetRawCPIindices(sr, countryShortName);
            string systemHICP = HICP.ContainsKey(systemYear) ? HICP[systemYear] : null; double systemHICPnum;
            string dataHICP = HICP.ContainsKey(dataYear) ? HICP[dataYear] : null; double dataHICPnum;
            if (systemHICP != null && dataHICP != null &&
                double.TryParse(systemHICP, out systemHICPnum) && double.TryParse(dataHICP, out dataHICPnum) && dataHICPnum > 0)
                upratingFactors.Add(FactorCPI.ToLower(), FixDecSep((systemHICPnum / dataHICPnum).ToString()));
            // then get the other factors
            foreach (CountryConfig.UpratingIndexRow index in ccf.GetUpratingIndices())
            {
                double systemIndex = -1, dataIndex = -1, factor = double.Parse(UpratingIndices.UpratingIndicesForm._factorValueInvalid);
                foreach (var yv in ccf.GetUpratingIndexYearValues(index))
                {
                    if (yv.Key.ToString() == systemYear) systemIndex = yv.Value;
                    if (yv.Key.ToString() == dataYear) dataIndex = yv.Value;
                    if (dataIndex != -1 && systemIndex != -1) { if (dataIndex != 0) factor = systemIndex / dataIndex; break; }
                }
                if (!upratingFactors.ContainsKey(index.Reference.ToLower())) // do not overwrite $HICP
                    upratingFactors.Add(index.Reference.ToLower(), FixDecSep(factor.ToString()));
            }
            /////////////////////////////////////////////////////////////////////////////////////////////////////

            return upratingFactors;
        }

        private static void GetRawCPIindices_OldApproach(CountryConfig.SystemRow sr, string countryShortName, Dictionary<string, string> rawIndice)
        {
            foreach (CountryConfig.FunctionRow fr in sr.GetPolicyRows().First(p => p.Name.ToLower() == ("DefUpratingFactors_" + countryShortName).ToLower()).GetFunctionRows())
            {
                if (fr.Name.ToLower() == "defvar") // look for the raw indices
                {
                    // is it safe to assume that the first parameter will always be the years?
                    string[] years = fr.GetParameterRows().First().Name.Split('°');
                    // if the specific 
                    if (fr.GetParameterRows().Count(p => p.Name.ToLower().Contains("°" + FactorCPI.ToLower() + "°")) > 0)
                    {
                        string[] rawVals = fr.GetParameterRows().First(p => p.Name.ToLower().Contains("°" + FactorCPI.ToLower() + "°")).Name.Split('°');
                        // if there is at least one year, and the parse seems correct, get the raw indices
                        if (years.Length > 0 && rawVals.Length == years.Length + 3)
                        {
                            for (int i = 0; i < years.Length; i++)
                                if (!rawIndice.ContainsKey(years[i])) // only add if not "overwritten" by HICPConfig
                                    rawIndice.Add(years[i], rawVals[i + 2].ToLower());
                        }
                    }
                    break;
                }
            }
        }

        private static void GetUpratingFactors_OldApproach(CountryConfig.SystemRow sr, string datasetName, string countryShortName, Dictionary<string, string> upratingFactors)
        {
            foreach (CountryConfig.FunctionRow fr in sr.GetPolicyRows().First(p => p.Name.ToLower() == ("DefUpratingFactors_" + countryShortName).ToLower()).GetFunctionRows())
            {
                if (fr.Name.ToLower() == "defconst")    // look for the uprating factors
                {
                    if (fr.GetParameterRows().Count(p => p.Name.ToLower() == "const_dataset" && p.Value == datasetName) > 0)
                    {
                        foreach (CountryConfig.ParameterRow pr in fr.GetParameterRows())
                        {
                            if (pr.Name.ToLower() != "const_dataset")
                            {
                                upratingFactors.Add(pr.Name.ToLower(), pr.Value.ToLower());
                            }
                        }
                    }
                }
            }
        }

        private void FixUprating(DecompSystem decompSystem, Dictionary<string, string> upratingFactors, Dictionary<string, string> upratingFactors2, string countryShortName, double alpha, int uprateType, bool treatAsMarket,
                                     out List<string> parModifications)
        {
            parModifications = new List<string>();
            CountryConfig.SystemRow sr = decompSystem.sr;

            // first get the ils_origy & ils_ben components
            CountryConfig.PolicyRow ilsdef = sr.GetPolicyRows().FirstOrDefault(p => p.Name.ToLower() == ("ilsdef_" + countryShortName).ToLower());
            if (ilsdef == null) ilsdef = sr.GetPolicyRows().FirstOrDefault(p => p.Name.ToLower() == ("ildef_" + countryShortName).ToLower());
            if (ilsdef == null) return;

            CountryConfig.FunctionRow[] il_funcs = ilsdef.GetFunctionRows();
            List<string> ils_origy = ExpandIncomeList(DefVarName.ILSORIGY, il_funcs).Keys.ToList();
            List<string> ils_ben = ExpandIncomeList(DefVarName.ILSBEN, il_funcs).Keys.ToList();
            List<string> overrideInclude = ExpandIncomeList("pet_override", il_funcs).Where(x => x.Value).Select(x => x.Key).ToList();
            List<string> overrideExclude = ExpandIncomeList("pet_override", il_funcs).Where(x => !x.Value).Select(x => x.Key).ToList();

            List<string> reservedWords = new List<string> { "dataset", "def_factor", "factor_name", "factor_value", "factor_condition", "aggvar_name", "aggvar_part", "aggvar_tolerance", "warnifnofactor", "run_cond" };

            CountryConfig cc = CountryAdministrator.GetCountryConfigFacade(countryShortName).GetCountryConfig();

            // Then apply them to the appropriate variables of s1
            foreach (CountryConfig.FunctionRow fr in sr.GetPolicyRows().First(p => p.Name.ToLower() == ("Uprate_" + countryShortName).ToLower()).GetFunctionRows())
            {
                if (fr.Name.ToLower() == "uprate")
                {
                    foreach (CountryConfig.ParameterRow pr in fr.GetParameterRows())
                    {
                        string pn = pr.Name.ToLower();

                        if (!reservedWords.Contains(pn))
                        {
                            if (uprateType == 3)    // uprate all
                            {
                                double val;
                                if (upratingFactors.ContainsKey(pr.Value.ToLower()))
                                    parModifications.Add(ComposeParModification_Change(pr, FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors2[pr.Value.ToLower()]) * alpha).ToString())));
                                else if (EM_Helpers.TryConvertToDouble(pr.Value.ToLower(), out val))
                                    parModifications.Add(ComposeParModification_Change(pr, FixDecSep((val * alpha).ToString())));
                            }
                            else
                            {
                                bool marketIncome = overrideInclude.Contains(pn);   // if in the override include list
                                if (!overrideExclude.Contains(pn) && !marketIncome)     // else if not in the override exlcude list
                                {
                                    VarConfig.VariableRow v = EM_AppContext.Instance.GetVarConfigFacade().GetVariableByName(pn);
                                    if (v == null || v.Monetary != "1")
                                        marketIncome = false;
                                    else
                                    {
                                        if (treatAsMarket)
                                            marketIncome = !pn.EndsWith(DefGeneral.POSTFIX_SIMULATED);
                                        else
                                            marketIncome = !ils_ben.Contains(pn) && (pn[0] == 'y' || pn[0] == 'a' || pn[0] == 'x' || ils_origy.Contains(pn));
                                    }
                                }

                                // if this is a market income
                                if (marketIncome)
                                {
                                    if (uprateType == 1)
                                    {
                                        double val;
                                        if (upratingFactors.ContainsKey(pr.Value.ToLower()))
                                            parModifications.Add(ComposeParModification_Change(pr, FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors[pr.Value.ToLower()]) * alpha).ToString())));
                                        else if (EM_Helpers.TryConvertToDouble(pr.Value.ToLower(), out val))
                                            parModifications.Add(ComposeParModification_Change(pr, FixDecSep((val * alpha).ToString())));
                                    }
                                    else if (uprateType == 2)
                                    {
                                        if (upratingFactors.ContainsKey(pr.Value.ToLower()))
                                            parModifications.Add(ComposeParModification_Change(pr, FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors[pr.Value.ToLower()])).ToString())));
                                    }
                                }
                                else    // if it is non-market income
                                {
                                    if (uprateType == 2)
                                    {
                                        double val;
                                        if (upratingFactors2.ContainsKey(pr.Value.ToLower()))
                                            parModifications.Add(ComposeParModification_Change(pr, FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors2[pr.Value.ToLower()]) / alpha).ToString())));
                                        else if (EM_Helpers.TryConvertToDouble(pr.Value.ToLower(), out val))
                                            parModifications.Add(ComposeParModification_Change(pr, FixDecSep((val / alpha).ToString())));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (uprateType > 1)
            {
                string[] monetaryTypes = new string[] { DefPeriod.M, DefPeriod.Y, DefPeriod.Q, DefPeriod.W,
                                                        DefPeriod.D, DefPeriod.L, DefPeriod.S, DefPeriod.C };
                foreach (CountryConfig.FunctionRow fr in sr.GetPolicyRows().First(p => p.Name.ToLower() == ("Uprate_" + countryShortName).ToLower()).GetFunctionRows())
                {
                    foreach (CountryConfig.ParameterRow pr in fr.GetParameterRows())
                    {
                        string val = pr.Value.ToLower().Trim();
                        if (val.Length < 3) continue;
                        string valType = val.Substring(val.Length - 2);

                        if (monetaryTypes.Contains(valType))
                        {
                            val = val.Substring(0, val.Length - 2);
                            if (uprateType == 2)
                                parModifications.Add(ComposeParModification_Change(pr, FixDecSep((EM_Helpers.SaveConvertToDouble(val) / alpha).ToString()) + valType));
                            else if (uprateType == 3)
                                parModifications.Add(ComposeParModification_Change(pr, FixDecSep((EM_Helpers.SaveConvertToDouble(val) * alpha).ToString()) + valType));
                        }
                    }
                }
            }
            try
            {
                // Then, fix the output filenames
                CountryConfig.ParameterRow ofpr = sr.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_" + countryShortName).ToLower())
                    .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput")
                    .GetParameterRows().First(p => p.Name.ToLower() == "file");
                parModifications.Add(ComposeParModification_Change(ofpr, decompSystem.name + "_std"));
                ofpr = sr.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_hh_" + countryShortName).ToLower())
                    .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput")
                    .GetParameterRows().First(p => p.Name.ToLower() == "file");
                parModifications.Add(ComposeParModification_Change(ofpr, decompSystem.name + "_std_hh"));
                // Finally, if required, do the scaling
                if (checkRadioMarket.Checked)
                {
                    CountryConfig.FunctionRow fr = sr.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_" + countryShortName).ToLower())
                            .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput");
                    CountryConfig.FunctionRow fr_hh = sr.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_hh_" + countryShortName).ToLower())
                            .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput");

                    if (fr.GetParameterRows().Count(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower()) == 0)
                    {
                        parModifications.Add(ComposeParModification_Add(fr, DefPar.DefOutput.MultiplyMonetaryBy, FixDecSep((1 / alpha).ToString())));
                    }
                    else
                    {
                        CountryConfig.ParameterRow mpr = fr.GetParameterRows().First(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower());
                        double d;
                        if (!EM_Helpers.TryConvertToDouble(mpr.Value, out d)) d = 1;
                        parModifications.Add(ComposeParModification_Change(mpr, FixDecSep((d / alpha).ToString())));
                    }

                    if (fr_hh.GetParameterRows().Count(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower()) == 0)
                    {
                        parModifications.Add(ComposeParModification_Add(fr_hh, DefPar.DefOutput.MultiplyMonetaryBy, FixDecSep((1 / alpha).ToString())));
                    }
                    else
                    {
                        CountryConfig.ParameterRow mpr_hh = fr_hh.GetParameterRows().First(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower());
                        double d;
                        if (!EM_Helpers.TryConvertToDouble(mpr_hh.Value, out d)) d = 1;
                        parModifications.Add(ComposeParModification_Change(mpr_hh, FixDecSep((d / alpha).ToString())));
                    }
                }
            }
            catch
            {
                throw new Exception("Problem in default output functions.");
            }

            string ComposeParModification_Change(CountryConfig.ParameterRow par, string parVal)
            {
                CountryConfig.ParameterRow par_1stSys = (
                    from p in cc.Parameter
                    where p.FunctionRow.PolicyRow.Order == par.FunctionRow.PolicyRow.Order &&
                          p.FunctionRow.Order == par.FunctionRow.Order &&
                          p.Order == par.Order &&
                          p.FunctionRow.PolicyRow.SystemID == cc.System[0].ID
                    select p).First();

                return "{" + $"'ModificationType': 'ChangeParameter'," +
                             $"'FunctionId':'{par_1stSys.FunctionID}'," +
                             $"'ParameterId':'{par_1stSys.ID}'," +
                             $"'ParameterValue':'{parVal}'" + "}";
            }

            string ComposeParModification_Add(CountryConfig.FunctionRow fun, string parName, string parVal)
            {
                CountryConfig.FunctionRow fun_1stSys = (
                    from f in cc.Function
                    where f.PolicyRow.Order == fun.PolicyRow.Order &&
                          f.Order == fun.Order &&
                          f.PolicyRow.SystemID == cc.System[0].ID
                    select f).First();

                return "{" + $"'ModificationType': 'AddParameter'," +
                             $"'FunctionId':'{fun_1stSys.ID}'," +
                             $"'ParameterName':'{parName}'," +
                             $"'ParameterValue':'{parVal}'," +
                             $"'Group':''" + "}";
            }
        }

        private string FixDecSep(string num) { return num.Replace(',', '.'); } // the executable only accepts .

        private SystemBackgroundWorker RunSystem(string shortCountryName, string systemName, string databaseName, Dictionary<string, string> config)
        {
            try
            {
                SystemBackgroundWorker bw = new SystemBackgroundWorker(shortCountryName, systemName, databaseName, config);
                bw.WorkerSupportsCancellation = true;
                bw.DoWork += bw_DoWork;
                bw.RunWorkerCompleted += bw_RunWorkerCompleted;
                bw.RunWorkerAsync();
                return bw;
            }
            catch (Exception exception)
            {
                MessageBox.Show("Run failed!\n"+exception.Message);
                return null;
            }

        }

        private void updateInfoLabel()
        {
            int c = 0;
            foreach (SystemBackgroundWorker sw in workers)
                if (!sw.IsBusy) c++;
            lblInfo.Text = "Waiting " + (totalSystems - workers.Count) + " systems out of " + totalSystems + Environment.NewLine + Environment.NewLine;
            lblInfo.Text += "Running " + (workers.Count - c) + " systems out of " + totalSystems + Environment.NewLine + Environment.NewLine;
            lblInfo.Text += "Completed " + c + " systems out of " + totalSystems + Environment.NewLine;
            if (em3_petInfo.HasErrors()) lblInfo.Text += Environment.NewLine + "!Errors Found!" + Environment.NewLine;
            if (em3_petInfo.HasWarnings()) lblInfo.Text += Environment.NewLine + "!Warnings Found!" + Environment.NewLine;
            lblInfo.Refresh();
        }

        void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            bool hasAddon = GetCheckedAddon() != "";
            SystemBackgroundWorker sbw = sender as SystemBackgroundWorker;
            if (!chkEM2.Checked) EM3_AddToRunInfoList(sbw);

            if (!bool.Parse(e.Result.ToString()))
            {
                sbw.hadErrors = true;
                em3_petInfo.AddSystemWithErrors(sbw.systemName);
            }
            else
            {
                sbw.hadErrors = false;

                if ((chkEM2.Checked && sbw.em2_hasErrorFile) || (!chkEM2.Checked && sbw.em3_RunInfo.HasWarnings())) // EM2: if run did not crash but has an error log, then it must have warnings
                    em3_petInfo.AddSystemWithWarnings(sbw.systemName);
                AddToLog($"Finished {sbw.systemName} at", $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}", LOGMODE.EM2);
                lock (sbw.systemName)
                {
                    if (sbw.isBaseline)
                    {
                        if (checkBoxAlphaMII.Checked)
                        {
                            // if this was a baseline system && running MII && secondBaseline is also finished, then go for the Decomp systems
                            if (sbw.secondBaseline.finished && !sbw.secondBaseline.calculateDecomp)
                            {
                                sbw.calculateDecomp = true;    // make sure this does not run twice if baselines finish at the same time!
                                if (!sbw.secondBaseline.hadErrors)  // make sure there were no errors in the other baseline
                                    RunDecompCountry(sbw.shortCountryName);
                            }
                        }
                        else if (hasAddon)
                        {
                            // if this was a baseline system && running MII && secondBaseline is also finished, then go for the Decomp systems
                            if (sbw.secondBaseline != null && sbw.secondBaseline.finished && !sbw.secondBaseline.calculateDecomp)
                            {
                                sbw.calculateDecomp = true;    // make sure this does not run twice if baselines finish at the same time!
                                if (!sbw.secondBaseline.hadErrors)  // make sure there were no errors in the other baseline
                                    RunDecompCountry(sbw.shortCountryName);
                            }
                            else if (sbw.secondBaseline == null)
                            {
                                RunDecompCountry(sbw.shortCountryName);
                            }
                        }
                    }
                }
            }
            bool allDone = true;
            foreach (SystemBackgroundWorker w in workers)
            {
                if (w.IsBusy)
                {
                    allDone = false;
                    break;
                }
            }
            updateInfoLabel();
            if (allDone)
            {
                Report();

                if (em3_petInfo.HasErrors())
                {
                    Cursor = Cursors.Default;
                    lblInfo.Visible = false;

                    foreach (SystemBackgroundWorker w in workers)
                        if (w.hadErrors && runCountries.ContainsKey(w.shortCountryName))
                            runCountries.Remove(w.shortCountryName);
                }
                if (runCountries.Count > 0 && showResults) goToOutput();
                else Close();
            }
        }

        private void Report(bool cancelled = false)
        {
            try
            {
                if (chkEM2.Checked)
                {
                    if (em3_petInfo.EM2_GetErrorInfo(out string info)) logText += Environment.NewLine + info;
                    File.WriteAllText(Path.Combine(textBoxOutputPath.Text, $"PolicyEffects_{DateTime.Now.ToString("yyyymmddHHmmss")}.log"), logText);
                }
                else new RunLogger(EM_AppContext.Instance.GetProjectName(), em3_runInfoList, em3_petInfo).TxtWritePetLog(textBoxOutputPath.Text);

                string problems = string.Empty;
                if (em3_petInfo.HasErrors() && em3_petInfo.HasWarnings()) problems = " with errors and warnings";
                else if (em3_petInfo.HasErrors()) problems = " with errors";
                else if (em3_petInfo.HasWarnings()) problems = " with warnings";

                string messageText = $"{(cancelled ? "Cancelled" : "Finished")}{problems} at {DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}";
                if (problems == string.Empty) messageText += Environment.NewLine + $"Completed {totalSystems} systems";
                messageText += Environment.NewLine + "For more details have a look at the log-file";

                if (problems != string.Empty || !showResults) MessageBox.Show(messageText);
            }
            catch { }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            SystemBackgroundWorker sbw = sender as SystemBackgroundWorker;
            e.Result = chkEM2.Checked ? EM2_Run(sbw) : EM3_Run(sbw);
            sbw.finished = true;
        }

        private void btnRunOnly_Click(object sender, EventArgs e)
        {
            showResults = false;
            runPolicyEffects(false);
        }

        private void btnRunPolicyEffects_Click(object sender, EventArgs e)
        {
            showResults = true;
            runPolicyEffects(true);
        }

        private void runPolicyEffects(bool showResults)
        {
            try
            {
                StoreUserSettings();

                em3_petInfo = new RunLogger.PetInfo();
                logText = $"{Text} Run Log" + Environment.NewLine;
                AddToLog(RunLogger.PetInfo.LOGTAG_STARTED_AT, $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}", LOGMODE.EM2);
                AddToLog($"{DefGeneral.BRAND_TITLE} UI v{DefGeneral.UI_VERSION}", null, LOGMODE.EM2);
                AddToLog(RunLogger.PetInfo.LOGTAG_PROJECT, EM_AppContext.Instance.GetProjectName(), LOGMODE.EM2);
                AddToLog(RunLogger.PetInfo.LOGTAG_DECOMPOSITION_TYPE, showFull ? "multiple combinations (3 per dataset)"
                                                                               : "single combination (1 per dataset)");
                if (!PrepareFolder()) return;

                if (!CheckSystems(showResults)) return;

                btnRunPolicyEffects.Visible = false;
                btnRunOnly.Visible = false;

                chkEM2.Visible = false;
                labelRunFirstNHH.Visible = false;
                textRunFirstNHH.Visible = false;

                
                lblInfo.Visible = true;
                lblInfo.Text = "Preparing...";
                lblInfo.Refresh();

                Cursor = Cursors.WaitCursor; //use hour glass as generating add-ons may take some time

                em3_transformGlobals = true; // to tranform globals (hicp, variables, ...) only once

                // first run all baseline systems
                RunBaselineSystems();
                // then if MII is not checked, and if there is no Add-on selected, run Decomp systems (otherwise wait for MII to finish)
                bool hasAddon = GetCheckedAddon() != "";
                if (!(checkBoxAlphaMII.Checked || hasAddon)) RunDecompSystems();
            }
            catch (Exception ex)
            {
                MessageBox.Show("There has been an error: \n" + ex.Message);
                em3_petInfo.AddSystemIndependentError(ex.Message);
                killAllSystems();
            }
        }

        private void RunDecompCountry(string countryShortName)
        {
            if (chkEM2.Checked) { EM2_RunDecompCountry(countryShortName); return; }

            string currentAction = "";
            try
            {
                currentAction = "getting " + countryShortName + " config files";
                DataRow row = policyDataTable.Select("Country='" + countryShortName + "'")[0];
                string sn1 = showFull ? row["System1"].ToString() : countryShortName + "_" + comboBox1.Text;
                string sn2 = showFull ? row["System2"].ToString() : countryShortName + "_" + comboBox2.Text;
                CountryConfig.SystemRow sr1 = CountryAdministrator.GetCountryConfigFacade(countryShortName).GetSystemRowByName(sn1);
                CountryConfig.SystemRow sr2 = CountryAdministrator.GetCountryConfigFacade(countryShortName).GetSystemRowByName(sn2);

                CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(countryShortName);
                DataConfigFacade _dataConfigFacade = CountryAdministrator.GetDataConfigFacade(countryShortName);

                DataConfig.DataBaseRow dbr1 = null;
                DataConfig.DataBaseRow dbr2 = null;
                foreach (DataConfig.DataBaseRow dataSet in _dataConfigFacade.GetDataBaseRows())
                {
                    if (dataSet.Name == row["Data1"].ToString()) dbr1 = dataSet;
                    if (dataSet.Name == row["Data2"].ToString()) dbr2 = dataSet;
                }

                // then create all required system-changes, depending on decomposition and Alpha selection
                currentAction = "getting " + countryShortName + "'s uprate factors";
                Dictionary<string, string> upratingFactors1 = GetUpratingFactors(sr1, dbr1, countryShortName);
                Dictionary<string, string> upratingFactors2 = GetUpratingFactors(sr2, dbr2, countryShortName);
                double alpha = 0;

                List<DecompSystem> allSystems = new List<DecompSystem>();
                bool treatAsMarket = chkTreatAsMarket.Checked;

                currentAction = "creating " + countryShortName + "'s decomposed systems";

                const double ALPHA_CPI = double.MinValue, ALPHA_MII = double.MaxValue; // just any numbers differnt from the alphas in alphaFIX
                List<double> alphas = new List<double>();
                if (checkBoxAlphaCPI.Checked) alphas.Add(ALPHA_CPI);
                if (checkBoxAlphaMII.Checked) alphas.Add(ALPHA_MII);
                if (checkBoxAlphaFIX.Checked) alphas.AddRange(alphaFIXValues); // those where gathered in GetAlphaFIX

                foreach (double a in alphas)
                {
                    string systemNameExt = "";
                    // first find Alpha, log text, system name extention etc.
                    if (a == ALPHA_CPI)
                    {
                        Dictionary<string, string> rawIndices = GetRawCPIindices(sr1, countryShortName);
                        if (!(rawIndices.ContainsKey(comboBox1.Text) || rawIndices.ContainsKey(comboBox2.Text)))
                            throw new Exception("The CPI raw indices ('" + FactorCPI + "') were not found for the selected years!");
                        double hicp1 = EM_Helpers.SaveConvertToDouble(rawIndices[comboBox1.Text]);
                        double hicp2 = EM_Helpers.SaveConvertToDouble(rawIndices[comboBox2.Text]);
                        alpha = hicp2 / hicp1;
                        AddToLog($"{RunLogger.PetInfo.LOGTAG_ALPHA_CPI} {countryShortName}", $"{alpha} ({hicp2}/{hicp1})", LOGMODE.EM3);
                        systemNameExt = "_cpi";
                        alphaValues.Add(countryShortName + "_cpi", alpha);
                    }
                    else if (a == ALPHA_MII)
                    {
                        double mii1 = getAlphaFromBaselineFile(sr1.Name);
                        double mii2 = getAlphaFromBaselineFile(sr2.Name);
                        alpha = mii2 / mii1;
                        AddToLog($"{RunLogger.PetInfo.LOGTAG_ALPHA_MII} {countryShortName}", $"{alpha} ({mii2}/{mii1})", LOGMODE.EM3);
                        systemNameExt = "_mii";
                        alphaValues.Add(countryShortName + "_mii", alpha);
                    }
                    else
                    {
                        alpha = a;
                        systemNameExt = "_a" + GetAlphaFIXId(a);
                        alphaValues.Add(countryShortName + "_fix" + GetAlphaFIXId(a), alpha);
                    }

                    // Then actually create the required system-changes
                    if (checkRadioData1.Checked || checkRadioDataBoth.Checked)
                    {
                        DecompSystem ds1 = new DecompSystem
                        {
                            name = sr2.Name + "_on_" + dbr1.Name + systemNameExt,
                            sr = ccf.GetSystemRowByID(sr2.ID),
                            dbr = dbr1
                        };

                        FixUprating(ds1, upratingFactors1, upratingFactors2, countryShortName, alpha, 1, treatAsMarket, out ds1.parModifications);
                        allSystems.Add(ds1);
                        if (checkRadioMonetary.Checked)
                        {
                            DecompSystem ds2 = new DecompSystem()
                            {
                                name = sr2.Name + "ind_on_" + dbr1.Name + systemNameExt,
                                sr = sr2,
                                dbr = dbr1
                            };
                            FixUprating(ds2, upratingFactors1, upratingFactors2, countryShortName, alpha, 2, treatAsMarket, out ds2.parModifications);
                            allSystems.Add(ds2);
                            DecompSystem ds3 = new DecompSystem()
                            {
                                name = sr1.Name + "ind" + systemNameExt,
                                sr = sr1,
                                dbr = dbr1
                            };
                            FixUprating(ds3, upratingFactors1, upratingFactors2, countryShortName, alpha, 3, treatAsMarket, out ds3.parModifications);
                            allSystems.Add(ds3);
                        }
                    }
                    if (checkRadioData2.Checked || checkRadioDataBoth.Checked)
                    {
                        DecompSystem ds1 = new DecompSystem()
                        {
                            name = sr1.Name + "_on_" + dbr2.Name + systemNameExt,
                            sr = sr1,
                            dbr = dbr2
                        };
                        FixUprating(ds1, upratingFactors2, upratingFactors1, countryShortName, 1 / alpha, 1, treatAsMarket, out ds1.parModifications);
                        allSystems.Add(ds1);
                        if (checkRadioMonetary.Checked)
                        {
                            DecompSystem ds2 = new DecompSystem()
                            {
                                name = sr1.Name + "ind_on_" + dbr2.Name + systemNameExt,
                                sr = sr2,
                                dbr = dbr2
                            };
                            FixUprating(ds2, upratingFactors2, upratingFactors1, countryShortName, 1 / alpha, 2, treatAsMarket, out ds2.parModifications);
                            allSystems.Add(ds2);
                            DecompSystem ds3 = new DecompSystem()
                            {
                                name = sr2.Name + "ind" + systemNameExt,
                                sr = sr2,
                                dbr = dbr1
                            };
                            FixUprating(ds3, upratingFactors2, upratingFactors1, countryShortName, 1 / alpha, 3, treatAsMarket, out ds3.parModifications);
                            allSystems.Add(ds3);
                        }
                    }
                }

                currentAction = "running " + countryShortName + "'s decomposed systems";
                foreach (DecompSystem ds in allSystems)
                {
                    workers.Add(RunSystem(countryShortName, ds.sr.Name, ds.dbr.Name, EM3_CreateConfig(countryShortName, textBoxOutputPath.Text, ds.dbr, ds.sr,
                                          ds.parModifications.Count > 0 ? $"[{string.Join(",", ds.parModifications)}]" : null)));
                    updateInfoLabel();
                }
            }
            catch (Exception ex)
            {
                em3_petInfo.AddSystemIndependentError($"There was a problem with {currentAction}: {ex.Message}");
            }
        }

        private void btnAlphaRange_Click(object sender, EventArgs e)
        {
            PolicyEffectsAlphaRange rangePicker = new PolicyEffectsAlphaRange();
            rangePicker.Location = PointToScreen(new Point(groupBoxAlpha.Left + btnAlphaRange.Left, groupBoxAlpha.Top + btnAlphaRange.Bottom));
            if (rangePicker.ShowDialog(this) == DialogResult.Cancel) return;
            
            double start, end, step;
            rangePicker.GetStartEndStep(out start, out end, out step);
            textBoxAlpha.Text = string.Format("{0};{1};{2}", FixDecSep(start.ToString()), FixDecSep(end.ToString()), FixDecSep(step.ToString()));
        }

        private string GetAlphaFIXId(double a) { return a.ToString().Replace(".", "_"); }

        private bool GetAlphaFIXValues(bool showResults)
        {
            alphaFIXValues.Clear(); if (!checkBoxAlphaFIX.Checked) return true;

            if (string.IsNullOrEmpty(textBoxAlpha.Text)) { UserInfoHandler.ShowError("Alpha must not be empty."); return false; }

            if (!textBoxAlpha.Text.Contains(";")) // check for a single alpha (i.e. a number)
            {
                double alpha;
                if (EM_Helpers.TryConvertToDouble(textBoxAlpha.Text, out alpha) && !textBoxAlpha.Text.Contains(',')) { alphaFIXValues.Add(alpha); return true; }
                else { UserInfoHandler.ShowError("Alpha value must be a valid number."); return false; }
            }

            // check for a range of alphas in the form 'start;end;step' (e.g. 1;3;0.5)
            string[] split = textBoxAlpha.Text.Split(';'); List<double> startEndStep = new List<double>();
            for (int i = 0; i < split.Count(); ++i)
                { double d; if (!EM_Helpers.TryConvertToDouble(split[i], out d) || split[i].Contains(',')) break; startEndStep.Add(d); }
            if (startEndStep.Count != 3)
                { UserInfoHandler.ShowError("Invalid range of alphas (correct syntax: start;end;step, e.g. 1;3;0.5)"); return false; }
            return PolicyEffectsAlphaRange.GetRangeValues(out alphaFIXValues, startEndStep[0], startEndStep[1], startEndStep[2], showResults);
        }

        private string GetCheckedAddon()
        {
            for (int i = 0; i < chkListAddons.Items.Count; i++)
            {
                if (chkListAddons.GetItemChecked(i)) return chkListAddons.Items[i].ToString();
            }
            return string.Empty;
        }

        private bool MergeAddOn(List<AddOnSystemInfo> selectedAddOns, Country country, ref CountryConfig.SystemRow system)
        {
            DecompSystem decSys = new DecompSystem() { sr = system };
            List<Country> countries = EM_UI.CountryAdministration.CountryAdministrator.GetCountries();

            if (MergeAddOn(selectedAddOns, system.Name, decSys, country, true))
            {
                system = decSys.sr;
                return true;
            }
            else return false;
        }

        private bool MergeAddOn(List<AddOnSystemInfo> selectedAddOns, string baseSystemName, DecompSystem decompSystem, Country tempCountry, bool isBaseline = false)
        {
            AddOnGenerator addOnGenerator = new AddOnGenerator();

            foreach (AddOnSystemInfo addOnSystemInfo in selectedAddOns) //loop over the add-ons which are selected for run (gathered above)
            {
                if (!AddOnInfoHelper.IsSystemSupported(baseSystemName, addOnSystemInfo)) continue;
                CountryConfig.SystemRow addOnSystemRow = addOnGenerator.PerformAddOnGeneration(decompSystem.sr.Name, tempCountry._shortName,
                                                addOnSystemInfo._addOnShortName, baseSystemName, addOnSystemInfo._addOnSystemName);
                if (addOnSystemRow == null)
                {
                    //ImportExportHelper.WriteErrorLogFile(outputPath, addOnGenerator.GetErrorMessages());
                    throw new Exception("Addon could not be generated for " + baseSystemName);
                }

                //add just created add-on system to copied country
                CountryConfig.SystemRow newSystemRow = CountryConfigFacade.CopySystemRowToAnotherCountry(addOnSystemRow, addOnSystemRow.Name,
                                                                                    tempCountry.GetCountryConfigFacade().GetCountryConfig());
                //add system-dataset-connections for the add-on system (same as for base system)
                if (isBaseline)
                {
                    //need to use the system-row in the real country, as copied country now contains two systems with this name (add-on-system has same name as base-system, to allow for correct replacing of =sys=)
                    CountryConfig.SystemRow baseSystemRow = CountryAdministrator.GetCountryConfigFacade(tempCountry._shortName).GetSystemRowByName(newSystemRow.Name); 
                    tempCountry.GetDataConfigFacade().CopyDBSystemConfigRows(baseSystemRow, newSystemRow);
                }
                else
                {
                    tempCountry.GetDataConfigFacade().CopyDBSystemConfigRows(decompSystem.sr, newSystemRow);
                }

                decompSystem.sr = newSystemRow;   // use the new system instead

                //tempCountry.WriteXML(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles));

                /*
                contentEMConfig.Add(EM_XmlHandler.TAGS.SYSTEM_ID + Guid.NewGuid().ToString(), newSystemRow.ID); //see GenerateEMConfigs wrt Guid
                contentEMConfig[RunMainForm._labelRunFormInfoText] += addOnSystemInfo._addOnSystemName + " "; //add to text that is displayed in the window showing run status
                //redefined switches (as gathered above) need to be entered for each add-on-system
                foreach (string policySwitchEntry in policySwitchEntries)
                    contentEMConfig.Add(EM_XmlHandler.TAGS.POLICY_SWITCH + Guid.NewGuid().ToString(),
                        policySwitchEntry.Replace(_placeHolderSystemID, newSystemRow.ID)); //replace the placeholder temporarily used above with the actual id of the add-on-system
                contentEMConfig[EM_XmlHandler.TAGS.PARAMPATH] = EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles); //change path from country folder to temp-folder
                */
                return true;    // if one addon was matched, get out!
            }
            throw new Exception("No matching Addon found for " + baseSystemName);
        }

        private double getAlphaFromBaselineFile(string systemName)
        {
            string fileName = Path.Combine(textBoxOutputPath.Text, systemName + "_std.txt");
            if (!File.Exists(fileName))
            {
                throw new Exception("File '" + fileName + "' does not exist!");
            }
            List<string> _data = File.ReadAllLines(fileName).ToList();
            Random rnd = new Random();
            DataTable data = new DataTable();
            string[] header_line = _data[0].Trim().Split('\t');

            // Create the DataTable columns according to the header_line. If there are duplicate column names, add a random number at the end to avoid conflicts.
            foreach (string h in header_line) if (!data.Columns.Contains(h)) data.Columns.Add(h, typeof(double)); else data.Columns.Add(h + "_duplicate_" + rnd.NextDouble().ToString(), typeof(double));
            // The add the actual data in the table
            for (int i = 1; i < _data.Count(); i++)
                data.Rows.Add(_data[i].Trim().Split('\t'));

            double sumOrig = 0;
            double sumDwt = 0;
            foreach (DataRow dr in data.Rows)
            {
                sumOrig += (double)dr[DefVarName.ILSORIGY] * (double)dr["dwt"];
                sumDwt += (double)dr["dwt"];
            }

            return sumOrig/sumDwt;
        }

        private void RunDecompSystems()
        {
            foreach (DataRow row in policyDataTable.Rows)
            {
                if (row.Field<bool>("Check"))
                {
                    string countryShortName = row["Country"].ToString();
                    RunDecompCountry(countryShortName);
                }
            }
        }

        private void RunBaselineSystems()
        {
            if (chkEM2.Checked) { EM2_RunBaselineSystems(); return; }

            foreach (DataRow row in policyDataTable.Rows)
            {
                if (row.Field<bool>("Check"))
                {
                    string countryShortName = row["Country"].ToString();
                    CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(countryShortName);
                    DataConfigFacade _dataConfigFacade = CountryAdministrator.GetDataConfigFacade(countryShortName);
                    DataConfig.DataBaseRow dbr1 = null;
                    DataConfig.DataBaseRow dbr2 = null;
                    foreach (DataConfig.DataBaseRow dataSet in _dataConfigFacade.GetDataBaseRows())
                    {
                        if (dataSet.Name == row["Data1"].ToString()) dbr1 = dataSet;
                        if (dataSet.Name == row["Data2"].ToString()) dbr2 = dataSet;
                    }

                    string sn1 = showFull ? row["System1"].ToString() : countryShortName + "_" + comboBox1.Text;
                    string sn2 = showFull ? row["System2"].ToString() : countryShortName + "_" + comboBox2.Text;
                    CountryConfig.SystemRow sr1 = ccf.GetSystemRowByName(sn1);
                    CountryConfig.SystemRow sr2 = ccf.GetSystemRowByName(sn2);

                    if (sr1 == null) throw new Exception("System '" + sn1 + "' does not exist!");
                    if (sr2 == null) throw new Exception("System '" + sn2 + "' does not exist!");

                    if (!chkEM2.Checked) EM3_Transform(countryShortName, GetCheckedAddon());

                    if (checkBoxAlphaMII.Checked)
                    {
                        SystemBackgroundWorker w1, w2;
                        w1 = RunSystem(countryShortName, sr1.Name, dbr1.Name, EM3_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr1, sr1));
                        w2 = RunSystem(countryShortName, sr2.Name, dbr2.Name, EM3_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr2, sr2));
                        w1.isBaseline = true;
                        w2.isBaseline = true;
                        w1.secondBaseline = w2;
                        w2.secondBaseline = w1;
                        workers.Add(w1);
                        workers.Add(w2);
                        updateInfoLabel();
                    }
                    else
                    {
                        SystemBackgroundWorker w1 = null, w2 = null;
                        if (checkRadioData1.Checked || checkRadioDataBoth.Checked)
                        {
                            w1 = RunSystem(countryShortName, sr1.Name, dbr1.Name, EM3_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr1, sr1));
                            w1.isBaseline = true;
                        }
                        if (checkRadioData2.Checked || checkRadioDataBoth.Checked)
                        {
                            w2 = RunSystem(countryShortName, sr2.Name, dbr2.Name, EM3_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr2, sr2));
                            w2.isBaseline = true;
                        }

                        // todo: is this important, what does it mean???
                        //if (hasAddon && checkRadioDataBoth.Checked)
                        //{
                        //    w1.secondBaseline = w2;
                        //    w2.secondBaseline = w1;
                        //}

                        if (checkRadioData1.Checked || checkRadioDataBoth.Checked) workers.Add(w1);
                        if (checkRadioData2.Checked || checkRadioDataBoth.Checked) workers.Add(w2);
                        updateInfoLabel();
                    }
                }
            }
        }

        private bool PrepareFolder()
        {
            try
            {
                textBoxOutputPath.Text = EMPath.AddSlash(textBoxOutputPath.Text);
                if (!Directory.Exists(textBoxOutputPath.Text)) Directory.CreateDirectory(textBoxOutputPath.Text);
                /*
                string[] filePaths = Directory.GetFiles(textBoxOutputPath.Text);
                foreach (string filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        MessageBox.Show("Could not delete " + filePath + "!\nPlease make sure it is not open by another application.");
                        return false;
                    }
                }
                */
                return true;
            }
            catch
            {
                MessageBox.Show("Could not prepare output folder!\nPlease make sure you have the appropriate access rights.");
                return false;
            }
        }

        private bool CheckSystems(bool showResults)
        {
            if (int.Parse(comboBox1.Text) > int.Parse(comboBox2.Text))
            {
                MessageBox.Show("The starting year must be smaller than or equal to the ending year.");
                return false;
            }
            if (!GetAlphaFIXValues(showResults))
                return false;
            if (!(checkBoxAlphaMII.Checked || checkBoxAlphaFIX.Checked || checkBoxAlphaCPI.Checked))
            {
                MessageBox.Show("You need to check at least one Alpha.");
                return false;
            }
            Refresh();
            // first check if everything is ok
            bool haveOne = false;
            int allCountries = 0;
            workers.Clear();
            runCountries.Clear();
            AddToLog(RunLogger.PetInfo.LOGTAG_YEAR1, comboBox1.Text); AddToLog(RunLogger.PetInfo.LOGTAG_YEAR2, comboBox2.Text);
            string alphaString = "";
            if (checkBoxAlphaFIX.Checked) alphaString += ", Custom = " + textBoxAlpha.Text;
            if (checkBoxAlphaCPI.Checked) alphaString += ", CPI";
            if (checkBoxAlphaMII.Checked) alphaString += ", MII";
            AddToLog(RunLogger.PetInfo.LOGTAG_ALPHAS, alphaString.Substring(2));
            bool d1 = (checkRadioData1.Checked || checkRadioDataBoth.Checked);
            bool d2 = (checkRadioData2.Checked || checkRadioDataBoth.Checked);
            AddToLog(RunLogger.PetInfo.LOGTAG_DECOMPOSING_ON, d1 && d2 ? "Both" : d1 ? "Data1" : "Data2");
            if (showFull) AddToLog(RunLogger.PetInfo.LOGTAG_OUTPUT, checkRadioMarket.Checked ? "Scaled" : "Not scaled");
            List<string> countries = new List<string>();
            foreach (DataRow row in policyDataTable.Rows)
            {
                string cn = row["Country"].ToString();
                string s1 = showFull ? row["System1"].ToString() : cn + "_" + comboBox1.Text;
                string s2 = showFull ? row["System2"].ToString() : cn + "_" + comboBox2.Text;
                if (row.Field<bool>("Check"))
                {
                    if (!countries.Contains(cn)) countries.Add(cn);
                    // check if all required datasets have been selected
                    if ((checkRadioData1.Checked && row.Field<string>("Data1") == "n/a")
                        || (checkRadioData2.Checked && row.Field<string>("Data2") == "n/a")
                        || (checkRadioDataBoth.Checked && (row.Field<string>("Data1") == "n/a" || row.Field<string>("Data2") == "n/a")))
                    {
                        MessageBox.Show("Country '"+cn+"' has an invalid Dataset selected!\nPlease make sure that all selected countries have the proper Datasets defined.");
                        return false;
                    }
                    // check that the country systems exist for the selected years
                    bool hs1 = false, hs2 = false;
                    foreach (CountryConfig.SystemRow sr in EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(cn).GetSystemRows())
                    {
                        if (s1.ToLower() == sr.Name.ToLower()) hs1 = true; 
                        if (s2.ToLower() == sr.Name.ToLower()) hs2 = true; 
                    }
                    if (!hs1)
                    {
                        MessageBox.Show("System '" + s1 + "' does not exist!");
                        return false;
                    }
                    if (!hs2)
                    {
                        MessageBox.Show("System '" + s2 + "' does not exist!");
                        return false;
                    }
                    // check that the selected year/dataset combinations actually exist
                    DataConfigFacade dcf = EM_UI.CountryAdministration.CountryAdministrator.GetDataConfigFacade(cn);
                    if ((checkRadioData1.Checked || checkRadioDataBoth.Checked) && dcf.GetDBSystemConfigRowsBySystem(s2).Count(x => x.DataBaseRow.Name == row.Field<string>("Data1")) == 0)
                    {
                        MessageBox.Show("System '" + s2 + "' cannot run with Dataset '"+ row.Field<string>("Data1") +"'!");
                        return false;
                    }
                    if ((checkRadioData2.Checked || checkRadioDataBoth.Checked) && dcf.GetDBSystemConfigRowsBySystem(s1).Count(x => x.DataBaseRow.Name == row.Field<string>("Data2")) == 0)
                    {
                        MessageBox.Show("System '" + s1 + "' cannot run with Dataset '"+ row.Field<string>("Data2") +"'!");
                        return false;
                    }

                    // all seems ok, so log the country and move on
                    string with = string.Empty;
                    if (showFull)
                    {
                        string sd1 = d1 ? "Data1" : "Data2", sd2 = d2 ? "Data2" : "Data1";
                        with = $"Sys/Data1={row["System1"]}/{row[sd1]} & Sys/Data2={row["System2"]}/{row[sd2]}";
                    }
                    else
                    {
                        if (d1) with += "Data1='" + row["Data1"].ToString() + "'";
                        if (d1 && d2) with += " & ";
                        if (d2) with += "Data2='" + row["Data2"].ToString() + "'";
                    }
                    AddToLog(RunLogger.PetInfo.LOGTAG_SELECTED_COUNTRY, $"{row["Country"]} with {with}");
                    
                    runCountries.Add(row["Country"].ToString(), new Tuple<string, string>(row["System1"].ToString(), row["System2"].ToString()));
                    allCountries++;
                    haveOne = true;
                }
            }

            foreach (string cn in countries)
            {
                if (EM_AppContext.Instance.WriteXml(cn, true, true) == DialogResult.Cancel) // try saving any changes in open countries
                    return false;
            }

            // count the totalSystems systems that need to run
            // first add the baseline systems
            if (checkBoxAlphaMII.Checked || checkRadioDataBoth.Checked)
                totalSystems = runCountries.Count * 2;
            else
                totalSystems = runCountries.Count;
            // then add the decomp systems
            foreach (DevExpress.XtraEditors.CheckEdit chk in new DevExpress.XtraEditors.CheckEdit[] {checkBoxAlphaMII, checkBoxAlphaCPI}) //, checkBoxAlphaFIX})
                if (chk.Checked)
                    totalSystems += runCountries.Count * (checkRadioDataBoth.Checked ? 2 : 1) * (checkRadioMonetary.Checked ? 3 : 1);
            for (int i = 0; i < alphaFIXValues.Count; ++i)
                totalSystems += runCountries.Count * (checkRadioDataBoth.Checked ? 2 : 1) * (checkRadioMonetary.Checked ? 3 : 1);

            AddToLog(string.Empty, string.Empty, LOGMODE.EM2); // corresponds to: logText += Environment.NewLine;
            
            if (!haveOne)
            {
                MessageBox.Show("You must select at least one country.");
                return false;
            }
            return true;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataColumn(1);
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateDataColumn(2);
        }

        private void UpdateDataColumn(int col, List<string> hasStoredValue = null)
        {
            gridControl1.BeginUpdate();
            string year = col==1?comboBox1.Text:comboBox2.Text;
            foreach (DataRow r in policyDataTable.Rows)
            {
                string cn = r["Country"].ToString();
                if (hasStoredValue != null && hasStoredValue.Contains(cn.ToLower())) continue;
                List<string> systems = new List<string>();
                foreach (CountryConfig.SystemRow sr in EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(cn).GetSystemRows())
                    if (sr.Year == year && (showFull || sr.Name.ToLower() == string.Format("{0}_{1}", cn.ToLower(), year))) systems.Add(sr.Name);
                r["System" + col] = systems.Count == 0 ? "n/a" : systems[0];

                List<DataConfig.DBSystemConfigRow> dbscrs = EM_UI.CountryAdministration.CountryAdministrator.GetDataConfigFacade(cn).GetDBSystemConfigRowsBySystem(r["System" + col].ToString());
                if (dbscrs.Count == 0)
                {
                    r["Data" + col] = "n/a";
                }
                else
                {
                    foreach (DataConfig.DBSystemConfigRow dbscr in dbscrs)
                        if (dbscr.BestMatch == "yes")
                        {
                            r["Data" + col] = dbscr.DataBaseRow.Name;
                        }
                }
            }
            
            gridControl1.EndUpdate();
            if (!showFull && col == 1) comboBox2.Text = (int.Parse(comboBox1.Text) + 1).ToString();
        }

        private void PolicyEffects_Shown(object sender, EventArgs e)
        {
            lblInfo.Dock = DockStyle.Fill;
            lblInfo.BringToFront();
            lblInfo.Visible = true;
            lblInfo.Text = "Please wait while loading project data...";
            lblInfo.Refresh();
            try
            {
                List<int> uniqueYears = new List<int>();
                foreach (Country cc in EM_UI.CountryAdministration.CountryAdministrator.GetCountries())
                {
                    string cn = cc._shortName.ToLower();
                    foreach (CountryConfig.SystemRow sr in cc.GetCountryConfigFacade().GetSystemRows())
                    {
                        int y; if (string.IsNullOrEmpty(sr.Year) || !int.TryParse(sr.Year, out y)) continue;
                        if (showFull || sr.Name.ToLower() == string.Format("{0}_{1}", cn, sr.Year)) // Basic: only allow default names, e.g. "AT_2010"
                        {
                            if (!uniqueYears.Contains(y)) uniqueYears.Add(y);
                        }
                    }
                }

                uniqueYears.Sort();

                for (int i = 0; i < uniqueYears.Count; i++)
                {
                    if (showFull)
                    {
                        comboBox1.Items.Add(uniqueYears[i].ToString());
                        comboBox2.Items.Add(uniqueYears[i].ToString());
                    }
                    else if (uniqueYears.Contains(uniqueYears[i] + 1))
                    {
                        comboBox1.Items.Add(uniqueYears[i].ToString());
                        comboBox2.Items.Add((uniqueYears[i]+1).ToString());
                    }
                }

                policyDataTable = new DataTable();
                policyDataTable.Columns.Add("Check", typeof(bool));
                policyDataTable.Columns.Add("Country", typeof(string));
                policyDataTable.Columns.Add("System1", typeof(string));
                policyDataTable.Columns.Add("System2", typeof(string));
                policyDataTable.Columns.Add("Data1", typeof(string));
                policyDataTable.Columns.Add("Data2", typeof(string));

                comboBox1.SelectedIndex = 0;
                comboBox2.SelectedIndex = 0;
                checkBoxAlphaFIX.Checked = true;
                checkBoxAlphaCPI.Checked = true;
                checkBoxAlphaMII.Checked = false;
                if (!showFull)
                {
                    comboBox1.Text = "2013";
                    comboBox2.Text = "2014";
                }
                checkRadioData1.Checked = true;

                Dictionary<string, Tuple<bool,string,string,string,string>> storedPolicyDataTable; RestoreUserSettings(out storedPolicyDataTable);
                List<string> hasStoredValue = new List<string>();
                foreach (Country c in EM_UI.CountryAdministration.CountryAdministrator.GetCountries())
                {
                    if (storedPolicyDataTable.ContainsKey(c._shortName)) // use settings of a previous PET-run within this UI-session
                    {
                        Tuple<bool, string, string, string, string> cStored = storedPolicyDataTable[c._shortName];
                        policyDataTable.Rows.Add(new object[] { cStored.Item1, c._shortName, cStored.Item2, cStored.Item3, cStored.Item4, cStored.Item5 });
                        hasStoredValue.Add(c._shortName.ToLower());
                    }
                    else // first PET-run (no stored settings available)
                        policyDataTable.Rows.Add(new object[] { false, c._shortName, "n/a", "n/a", "n/a", "n/a" });
                }
                gridControl1.DataSource = policyDataTable;

                // load available Addons in CheckListBox
                foreach (Country addOn in CountryAdministrator.GetAddOns())
                    chkListAddons.Items.Add(addOn._shortName, false);

                // show databases for the selected year(s)
                UpdateDataColumn(1, hasStoredValue); UpdateDataColumn(2, hasStoredValue);

                autoSetColumnWidths();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
            lblInfo.Visible = false;
            gridControl1.Focus();
        }

        private void PolicyEffects_FormClosing(object sender, FormClosingEventArgs e)
        {
            int c = 0;
            foreach (SystemBackgroundWorker w in workers) if (w.IsBusy) c++;
            if (c > 0)
            {
                if (MessageBox.Show("There are still some systems running!\n\nAre you sure you want to stop them?", "Cancel Run", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
                {
                    killAllSystems();
                }
                else e.Cancel = true;
            }
        }

        private void killAllSystems()
        {
            foreach (SystemBackgroundWorker w in workers)
            {
                if (!w.finished)
                    try
                    {
                        if (w.process != null) w.process.Kill();
                        if (w.em3_RunInfo != null) w.em3_Cancel = true;
                    }
                    catch (Exception)
                    {
                        // process exited already
                    }
            }

            Report(true);
            Cursor = Cursors.Default;
            lblInfo.Visible = false;
            showResults = false;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonChangePath_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.SelectedPath = Directory.Exists(textBoxOutputPath.Text)?textBoxOutputPath.Text:EMPath.AddSlash(EM_AppContext.FolderOutput);
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textBoxOutputPath.Text = fbd.SelectedPath;
            }
        }

        private void PolicyEffects_Load(object sender, EventArgs e)
        {
            if (showFull)
            {
                PolicyEffectsChooseMode cmf = new PolicyEffectsChooseMode();
                System.Windows.Forms.DialogResult res = cmf.ShowDialog(_mainForm);
                if (res == System.Windows.Forms.DialogResult.Cancel) Close();
                showFull =  res == System.Windows.Forms.DialogResult.Yes;
            }

            // Hide EM2 if encryption is applied
            if (SecureInfo.IsSecure && !string.IsNullOrEmpty(SecureInfo.DataPassword))
                chkEM2.Visible = false;

            // show/hide extra controls
            checkBoxAlphaMII.Visible = showFull;
            textBoxAlpha.Visible = showFull;
            btnAlphaRange.Visible = showFull;
            groupBoxAlpha.Size = new Size(showFull ? 157 : 122, showFull ? 94 : 75);
            groupBoxAlpha.Location = new Point(showFull ? 455 : 490, 52);
            labelAlpha.Location = new Point(showFull ? 462 : 497, 44);
            gridControl1.Width = showFull ? 422 : 457;
            gridControl1.Height = showFull ? 273 : 317;

            lblAddons.Visible = showFull;
            chkListAddons.Visible = showFull;
            chkTreatAsMarket.Visible = showFull;
            labelDecomposition.Visible = showFull;
            groupBoxData.Visible = showFull;
            labelIndex.Visible = showFull;
            groupBoxIndex.Visible = showFull;
            checkBoxAlphaCPI.Location = new Point(9, showFull ? 36 : 24);
            checkBoxAlphaFIX.Location = new Point(9, showFull ? 62 : 49);
            checkBoxAlphaFIX.Text = showFull ? "" : "1";
            checkRadioMarket.Checked = true;
            btnRunPolicyEffects.Visible = true;
            btnRunOnly.Visible = showFull;
            comboBox2.Enabled = showFull;
            panelResults.Visible = false;
            if (!showFull) gridColumnData1.Caption = "Population";
            gridColumnSys1.Visible = gridColumnSys2.Visible = showFull;
            // re-align labels & combos due to DPI issue
            comboBox1.Left = labelYear1.Left + labelYear1.Width;
            labelYear2.Left = comboBox1.Left + comboBox1.Width + 20;
            comboBox2.Left = labelYear2.Left + labelYear2.Width;
            this.Size = new Size(showFull ? 712: 607, showFull ? 534 : 484);
            this.MinimumSize = new Size(showFull ? 642 : 607, showFull ? 534 : 384);
            this.Left = (_mainForm.Width - this.Width) / 2;
            this.Top = (_mainForm.Height - this.Height) / 2;
        }

        class SystemBackgroundWorker : BackgroundWorker
        {
            internal Dictionary<string, string> config;
            internal Process process = null;
            internal RunLogger.RunInfo em3_RunInfo = new RunLogger.RunInfo();
            internal bool em3_Cancel = false;
            internal bool finished = false;
            internal bool calculateDecomp = false;
            internal bool hadErrors;
            internal bool em2_hasErrorFile = false;
            internal string systemName;
            internal string databaseName;
            internal string shortCountryName;
            internal bool isBaseline = false;
            internal SystemBackgroundWorker secondBaseline = null;

            public SystemBackgroundWorker(string _shortCountryName, string _systemName, string _databaseName, Dictionary<string, string> _config)
            {
                config = _config;
                systemName = _systemName;
                databaseName = _databaseName;
                shortCountryName = _shortCountryName;
            }
        }

        class DecompSystem
        {
            internal string name = null;
            internal CountryConfig.SystemRow sr = null;
            internal DataConfig.DataBaseRow dbr = null;
            internal List<string> parModifications = new List<string>();
        }
        #endregion INPUT

        private void goToOutput()
        {
            goAsc();
            Close();
        }

        private void PolicyEffects_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1 || e.KeyCode == Keys.F5 || e.KeyCode == Keys.F6)
            {
                string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath);
                Help.ShowHelp(this, helpPath, "EM_WW_PolicyEffects.htm");
                e.Handled = true;
            }
        }

        private void gridView1_CustomDrawColumnHeader(object sender, DevExpress.XtraGrid.Views.Grid.ColumnHeaderCustomDrawEventArgs e)
        {
            if (e.Column != null && e.Column.FieldName == "Check")
            {
                e.Info.HeaderPosition = DevExpress.Utils.Drawing.HeaderPositionKind.Center;
                int c = (int)policyDataTable.Compute("Count(Check)", "Check=true");
                e.Info.Caption = c == 0 ? "sel: none" : "sel: " + c.ToString();
            }
        }

        private void repositoryItemCheckEditCountry_CheckedChanged(object sender, EventArgs e)
        {
            gridView1.PostEditor();
            policyDataTable.AcceptChanges();
            gridControl1.Refresh();
        }

        private void radioGroupComparison_SelectedIndexChanged()
        {
            gridColumnData1.Visible = (checkRadioData1.Checked || checkRadioDataBoth.Checked);
            gridColumnData2.Visible = (checkRadioData2.Checked || checkRadioDataBoth.Checked);
        }

        private void checkAllCountries_CheckedChanged(object sender, EventArgs e)
        {
            foreach (DataRow row in policyDataTable.Rows)
                row.SetField<bool>("Check", checkAllCountries.Checked);
            gridControl1.Refresh();
        }

        private void checkRadioData1_CheckedChanged(object sender, EventArgs e)
        {
            radioGroupComparison_SelectedIndexChanged();
        }

        private void checkRadioData2_CheckedChanged(object sender, EventArgs e)
        {
            radioGroupComparison_SelectedIndexChanged();
        }

        private void checkRadioDataBoth_CheckedChanged(object sender, EventArgs e)
        {
            radioGroupComparison_SelectedIndexChanged();
        }

        private void chkListAddons_SelectedIndexChanged(object sender, EventArgs e)
        {
            chkListAddons.ClearSelected();
        }

        private void chkListAddons_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            for (int i = 0; i < chkListAddons.Items.Count; i++)
            {
                if (i != e.Index) chkListAddons.SetItemChecked(i, false);
            }
        }

        private void autoSetColumnWidths()
        {
            int colWidth = Math.Max(chkListAddons.Width - 4, 0) / Math.Max(chkListAddons.Items.Count, 1);
            chkListAddons.ColumnWidth = colWidth > 100 ? 100 : colWidth;
        }

        private void chkListAddons_ClientSizeChanged(object sender, EventArgs e)
        {
            autoSetColumnWidths();
        }

        private void StoreUserSettings() { Dictionary<string, Tuple<bool,string,string,string,string>> dummy; ReStoreUserSettings(true, out dummy); }
        private void RestoreUserSettings(out Dictionary<string, Tuple<bool,string,string,string,string>> storedPolicyDataTable) { ReStoreUserSettings(false, out storedPolicyDataTable); }
        private void ReStoreUserSettings(bool store, out Dictionary<string, Tuple<bool,string,string,string,string>> storedPolicyDataTable)
        {
            storedPolicyDataTable = new Dictionary<string, Tuple<bool,string,string,string,string>>(StringComparer.InvariantCultureIgnoreCase);
            try
            {
                string ID = "ID_PolicyEffects_" + (showFull ? "Advanced" : "Basic"); // store Basic and Advanced separately to avoid "invisible"
                                                                                     // settings if Advanced-backup is applied on Basic
                TSDictionary userSettings;
                if (store) userSettings = new TSDictionary(); // either prepare dictionary for storing settings ...
                else if (!EM_AppContext.Instance._sessionUserSettings.TryGetValue(ID, out userSettings)) return; // ... or get settings stored before

                // these are all the "simple" controls containing settings, i.e. all apart from the GridView
                List<Control> textControls = new List<Control>() { comboBox1, comboBox2, textBoxAlpha, textBoxOutputPath, textRunFirstNHH };
                List<CheckEdit> checkEdits = new List<CheckEdit>() { checkBoxAlphaFIX, checkBoxAlphaCPI, checkBoxAlphaMII,
                                                                     checkRadioData1, checkRadioData2, checkRadioDataBoth,
                                                                     checkRadioMarket, checkRadioMonetary, checkRadioMarketUnscaled };
                List<CheckBox> checkBoxes = new List<CheckBox>() { chkTreatAsMarket, chkEM2 };

                // store or restore simple controls
                foreach (Control textControl in textControls)
                {
                    if (store) userSettings.SetItem(textControl.Name, textControl.Text);
                    else textControl.Text = userSettings.GetItem<string>(textControl.Name);
                }
                foreach (CheckEdit checkEdit in checkEdits)
                {
                    if (store) userSettings.SetItem(checkEdit.Name, checkEdit.Checked);
                    else checkEdit.Checked = userSettings.GetItem<bool>(checkEdit.Name);
                }
                foreach (CheckBox checkBox in checkBoxes)
                {
                    if (store) userSettings.SetItem(checkBox.Name, checkBox.Checked);
                    else checkBox.Checked = userSettings.GetItem<bool>(checkBox.Name);
                }

                // store or restore GridView-settings (selected countries and datasets)
                const string policyDataTableID = "PolicyDataTableID";
                if (store)
                {
                    foreach (DataRow row in policyDataTable.Rows)
                        storedPolicyDataTable.Add(row["Country"].ToString(),
                            new Tuple<bool,string,string,string,string>(Convert.ToBoolean(row["Check"]),
                                row["System1"].ToString(), row["System2"].ToString(),
                                row["Data1"].ToString(), row["Data2"].ToString()));
                    userSettings.SetItem(policyDataTableID, storedPolicyDataTable);
                }
                else storedPolicyDataTable = userSettings.GetItem<Dictionary<string, Tuple<bool,string,string,string,string>>>(policyDataTableID);

                if (store)
                {
                    if (EM_AppContext.Instance._sessionUserSettings.ContainsKey(ID)) EM_AppContext.Instance._sessionUserSettings.Remove(ID);
                    EM_AppContext.Instance._sessionUserSettings.Add(ID, userSettings);
                }
            }
            catch { }
        }

        private enum LOGMODE { EM2, EM3, BOTH }
        private void AddToLog(string logHeader, string logInfo, LOGMODE logMode = LOGMODE.BOTH)
        {
            if (logMode != LOGMODE.EM2) em3_petInfo.logEntries.Add(new KeyValuePair<string, string>(logHeader, logInfo));
            if (logMode != LOGMODE.EM3) logText += $"{logHeader}{(string.IsNullOrEmpty(logInfo) ? string.Empty : $": {logInfo}")}" + Environment.NewLine;
        }
    }
}
