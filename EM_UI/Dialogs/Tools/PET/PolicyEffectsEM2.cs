using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.ImportExport;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;

namespace EM_UI.Dialogs.Tools
{
    internal partial class PolicyEffects : Form
    {
        private bool EM2_Run(SystemBackgroundWorker sbw)
        {
            sbw.process = new Process();
            sbw.process.StartInfo.FileName = EnvironmentInfo.GetEM2ExecutableFile();
            sbw.process.StartInfo.Arguments += EnvironmentInfo.EncloseByQuotes(sbw.config.First().Key);

            sbw.process.StartInfo.CreateNoWindow = true;
            sbw.process.StartInfo.UseShellExecute = false;
            sbw.process.StartInfo.RedirectStandardError = true;
            sbw.process.ErrorDataReceived += (t1, t2) => { if (t2.Data != null && t2.Data != "") { sbw.em2_hasErrorFile = true; } };

            sbw.process.Start();
            sbw.process.BeginErrorReadLine();
            sbw.process.WaitForExit();
            return sbw.process.ExitCode == 1;
        }

        private Dictionary<string, string> EM2_CreateConfig(string countryShortName, string outputPath, DataConfig.DataBaseRow dbr, CountryConfig.SystemRow sr, bool useTempCountry = true)
        {
            Dictionary<string, string> contentEMConfig = new Dictionary<string, string>();

            string emVersion = EM_AppContext.Instance.GetProjectName();
            if (emVersion.Trim() == string.Empty)
            {
                UserInfoHandler.ShowError($"{DefGeneral.BRAND_TITLE} version is not defined. Please define it via the menu 'Configuration'.");
                return null;
            }
            //fill EMConfig-entry-list
            string dateTimePrefix = string.Format("{0:yyyyMMddHHmm}", DateTime.Now);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_ERRLOG_FILE, outputPath + dateTimePrefix + EM_XmlHandler.TAGS.EM2CONFIG_errLogFileName);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LOG_WARNINGS, DefPar.Value.YES);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_EMVERSION, emVersion);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_UIVERSION, DefGeneral.UI_VERSION);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_PARAMPATH, useTempCountry ? EMPath.AddSlash(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles)) : EMPath.AddSlash(Path.Combine(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles), countryShortName)));
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_CONFIGPATH, EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles));
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_OUTPUTPATH, outputPath);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DATAPATH, EM_AppContext.FolderInput);
            string executablePath = EnvironmentInfo.GetEM2ExecutableFile();
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_EMCONTENTPATH, EM_AppContext.FolderEuromodFiles);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_HEADER_DATE, dateTimePrefix);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_OUTFILE_DATE, "-");
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LOG_RUNTIME, DefPar.Value.NO);
            if (EM_AppContext.Instance.IsPublicVersion())
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_ISPUBLICVERSION, DefPar.Value.YES);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DECSIGN_PARAM, EM_Helpers.uiDecimalSeparator);
            string startHH = EM_XmlHandler.TAGS.EM2CONFIG_defaultHHID;
            string lastHH = EM_XmlHandler.TAGS.EM2CONFIG_defaultHHID;
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_STARTHH, startHH);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_LASTHH, lastHH);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_COUNTRY_FILE, CountryAdministrator.GetCountryFileName(countryShortName));
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DATACONFIG_FILE, CountryAdministrator.GetDataFileName(countryShortName));
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_DATASET_ID, dbr.ID);
            contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID, sr.ID);

            //generate for each (available) switchable policy a POLICY_SWITCH-entry
            Dictionary<string, string> extIDAndPattern = new Dictionary<string, string>();
            foreach (GlobLocExtensionRow e in ExtensionAndGroupManager.GetExtensions(countryShortName)) extIDAndPattern.Add(e.ID, e.ShortName);

            List<string> policySwitchEntries = new List<string>();
            foreach (var e in extIDAndPattern)
            {
                string extID = e.Key, extShortName = e.Value;
                string policySwitchValue = string.Empty;
                if (dbr.GetDBSystemConfigRows().Count(x => x.SystemID == sr.ID) > 0)
                    policySwitchValue = ExtensionAndGroupManager.GetExtensionDefaultSwitch(dbr.GetDBSystemConfigRows().First(x => x.SystemID == sr.ID), extID);

                //generate the POLICY_SWITCH-entry
                //taking into account that there is no need to generate it if the switch is set to n/a (i.e. the switchable policy is not switchable for this db-system-combination or does not even exist)
                if (policySwitchValue != string.Empty && policySwitchValue != DefPar.Value.NA)
                {
                    string policySwitchEntry = //the executable needs three pieces of information to overwrite the default value of the policy switch:
                        extShortName //which switchable policy (e.g. BTA_??)
                        + "=" + sr.ID //which system
                        + "=" + policySwitchValue; //and the respective value (on or off)
                    policySwitchEntries.Add(policySwitchEntry);
                }
            }

            //for each (available) switchable policy add a POLICY_SWITCH-entry
            foreach (string policySwitchEntry in policySwitchEntries)
                contentEMConfig.Add(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH + Guid.NewGuid().ToString(), policySwitchEntry);


            //now actually write the EMConfigXXX.xml files and hand them over to the run-manager
            string configurationFileNameAndPath = EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles) + EM_XmlHandler.TAGS.EM2CONFIG_labelEMConfig + Guid.NewGuid().ToString() + ".xml";
            using (XmlTextWriter configurationFileWriter = new XmlTextWriter(configurationFileNameAndPath, null))
            {
                configurationFileWriter.Formatting = System.Xml.Formatting.Indented;
                configurationFileWriter.WriteStartDocument(true);
                configurationFileWriter.WriteStartElement(EM_XmlHandler.TAGS.EM2CONFIG_labelEMConfig);

                string runFormInfoText = string.Empty;
                foreach (string key in contentEMConfig.Keys)
                {
                    if (key.StartsWith(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID)) //remove Guid, see above
                        configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_SYSTEM_ID, contentEMConfig[key]);
                    else if (key.StartsWith(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH)) //remove Guid, see above
                        configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_POLICY_SWITCH, contentEMConfig[key]);
                    else
                        configurationFileWriter.WriteElementString(key, contentEMConfig[key]);
                }

                configurationFileWriter.WriteElementString(EM_XmlHandler.TAGS.EM2CONFIG_LAST_RUN, DefPar.Value.NO);
                configurationFileWriter.WriteEndElement();
                configurationFileWriter.WriteEndDocument();
            }
            // EM3 returns the config-dictionary, therefore (mis)use this structure to just store the file-path as the first entry (to avoid extra variables for EM2)
            return new Dictionary<string, string>() { { configurationFileNameAndPath, null } };
        }

        private void EM2_RunBaselineSystems()
        {
            // get the systems of the checked addon (if one was checked)
            string addon = GetCheckedAddon();
            bool hasAddon = addon != string.Empty;
            List<AddOnSystemInfo> addonSystems = hasAddon ? AddOnInfoHelper.GetAddOnSystemInfo(addon) : null;

            foreach (DataRow row in policyDataTable.Rows)
            {
                if (row.Field<bool>("Check"))
                {
                    string countryShortName = row["Country"].ToString();
                    Country copiedCountry = CountryAdministrator.GetCopyOfCountry(countryShortName);
                    CountryConfigFacade ccf = copiedCountry.GetCountryConfigFacade();
                    DataConfigFacade _dataConfigFacade = copiedCountry.GetDataConfigFacade();
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

                    if (hasAddon)
                    {
                        if (checkRadioData1.Checked || checkRadioDataBoth.Checked) MergeAddOn(addonSystems, copiedCountry, ref sr1);
                        if (checkRadioData2.Checked || checkRadioDataBoth.Checked) MergeAddOn(addonSystems, copiedCountry, ref sr2);
                    }

                    copiedCountry.WriteXML(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles));

                    if (checkBoxAlphaMII.Checked)
                    {
                        SystemBackgroundWorker w1, w2;
                        w1 = RunSystem(countryShortName, sr1.Name, dbr1.Name, EM2_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr1, sr1, true));
                        w2 = RunSystem(countryShortName, sr2.Name, dbr2.Name, EM2_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr2, sr2, true));
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
                            w1 = RunSystem(countryShortName, sr1.Name, dbr1.Name, EM2_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr1, sr1, hasAddon));
                            w1.isBaseline = true;
                        }
                        if (checkRadioData2.Checked || checkRadioDataBoth.Checked)
                        {
                            w2 = RunSystem(countryShortName, sr2.Name, dbr2.Name, EM2_CreateConfig(countryShortName, textBoxOutputPath.Text, dbr2, sr2, hasAddon));
                            w2.isBaseline = true;
                        }

                        if (hasAddon && checkRadioDataBoth.Checked)
                        {
                            w1.secondBaseline = w2;
                            w2.secondBaseline = w1;
                        }
                        if (checkRadioData1.Checked || checkRadioDataBoth.Checked) workers.Add(w1);
                        if (checkRadioData2.Checked || checkRadioDataBoth.Checked) workers.Add(w2);
                        updateInfoLabel();
                    }
                }
            }
        }

        private void EM2_RunDecompCountry(string countryShortName)
        {
            string currentAction = "";
            try
            {
                currentAction = "getting " + countryShortName + " config files";
                DataRow row = policyDataTable.Select("Country='" + countryShortName + "'")[0];
                string sn1 = showFull ? row["System1"].ToString() : countryShortName + "_" + comboBox1.Text;
                string sn2 = showFull ? row["System2"].ToString() : countryShortName + "_" + comboBox2.Text;
                CountryConfig.SystemRow sr1 = CountryAdministrator.GetCountryConfigFacade(countryShortName).GetSystemRowByName(sn1);
                CountryConfig.SystemRow sr2 = CountryAdministrator.GetCountryConfigFacade(countryShortName).GetSystemRowByName(sn2);

                // make a copy of the country, to be later stored in the temp-folder
                currentAction = "copying " + countryShortName + " for decomposition";
                Country copiedCountry = CountryAdministrator.GetCopyOfCountry(countryShortName);
                CountryConfigFacade ccf = copiedCountry.GetCountryConfigFacade();
                DataConfigFacade _dataConfigFacade = copiedCountry.GetDataConfigFacade();

                DataConfig.DataBaseRow dbr1 = null;
                DataConfig.DataBaseRow dbr2 = null;
                foreach (DataConfig.DataBaseRow dataSet in _dataConfigFacade.GetDataBaseRows())
                {
                    if (dataSet.Name == row["Data1"].ToString()) dbr1 = dataSet;
                    if (dataSet.Name == row["Data2"].ToString()) dbr2 = dataSet;
                }

                // then create all required systems, depending on decomposition and Alpha selection
                currentAction = "getting " + countryShortName + "'s uprate factors";
                Dictionary<string, string> upratingFactors1 = GetUpratingFactors(sr1, dbr1, countryShortName);
                Dictionary<string, string> upratingFactors2 = GetUpratingFactors(sr2, dbr2, countryShortName);
                double alpha = 0;

                List<DecompSystem> allSystems = new List<DecompSystem>();
                bool treatAsMarket = chkTreatAsMarket.Checked;

                // get the systems of the checked addon (if one was checked)
                string addon = GetCheckedAddon();
                bool hasAddon = addon != string.Empty;
                List<AddOnSystemInfo> addonSystems = hasAddon ? AddOnInfoHelper.GetAddOnSystemInfo(addon) : null;

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
                        AddToLog($"{RunLogger.PetInfo.LOGTAG_ALPHA_CPI} ({countryShortName})", $"{alpha} ({hicp2}/{hicp1})", LOGMODE.EM2);
                        systemNameExt = "_cpi";
                        alphaValues.Add(countryShortName + "_cpi", alpha);
                    }
                    else if (a == ALPHA_MII)
                    {
                        double mii1 = getAlphaFromBaselineFile(sr1.Name);
                        double mii2 = getAlphaFromBaselineFile(sr2.Name);
                        alpha = mii2 / mii1;
                        AddToLog($"{RunLogger.PetInfo.LOGTAG_ALPHA_MII} ({countryShortName})", $"{alpha} ({mii2}/{mii1})", LOGMODE.EM2);
                        systemNameExt = "_mii";
                        alphaValues.Add(countryShortName + "_mii", alpha);
                    }
                    else
                    {
                        alpha = a;
                        systemNameExt = "_a" + GetAlphaFIXId(a);
                        alphaValues.Add(countryShortName + "_fix" + GetAlphaFIXId(a), alpha);
                    }

                    // Then actually create the required systems
                    if (checkRadioData1.Checked || checkRadioDataBoth.Checked)
                    {
                        DecompSystem ds1 = new DecompSystem();
                        ds1.sr = CountryConfigFacade.CopySystemRow(sr2.Name + "_on_" + dbr1.Name + systemNameExt, ccf.GetSystemRowByID(sr2.ID));
                        copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(ccf.GetSystemRowByID(sr2.ID), ds1.sr);
                        ds1.dbr = dbr1;
                        if (hasAddon) MergeAddOn(addonSystems, sr2.Name, ds1, copiedCountry);
                        EM2_FixUprating(ds1.sr, upratingFactors1, upratingFactors2, countryShortName, alpha, 1, treatAsMarket);
                        allSystems.Add(ds1);
                        if (checkRadioMonetary.Checked)
                        {
                            DecompSystem ds2 = new DecompSystem();
                            ds2.sr = CountryConfigFacade.CopySystemRow(sr2.Name + "ind_on_" + dbr1.Name + systemNameExt, ccf.GetSystemRowByID(sr2.ID));
                            copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(ccf.GetSystemRowByID(sr2.ID), ds2.sr);
                            ds2.dbr = dbr1;
                            if (hasAddon) MergeAddOn(addonSystems, sr2.Name, ds2, copiedCountry);
                            EM2_FixUprating(ds2.sr, upratingFactors1, upratingFactors2, countryShortName, alpha, 2, treatAsMarket);
                            allSystems.Add(ds2);
                            DecompSystem ds3 = new DecompSystem();
                            ds3.sr = CountryConfigFacade.CopySystemRow(sr1.Name + "ind" + systemNameExt, ccf.GetSystemRowByID(sr1.ID));
                            copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(ccf.GetSystemRowByID(sr1.ID), ds3.sr);
                            ds3.dbr = dbr1;
                            if (hasAddon) MergeAddOn(addonSystems, sr1.Name, ds3, copiedCountry);
                            EM2_FixUprating(ds3.sr, upratingFactors1, upratingFactors2, countryShortName, alpha, 3, treatAsMarket);
                            allSystems.Add(ds3);
                        }
                    }
                    if (checkRadioData2.Checked || checkRadioDataBoth.Checked)
                    {
                        DecompSystem ds1 = new DecompSystem();
                        ds1.sr = CountryConfigFacade.CopySystemRow(sr1.Name + "_on_" + dbr2.Name + systemNameExt, ccf.GetSystemRowByID(sr1.ID));
                        copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(ccf.GetSystemRowByID(sr1.ID), ds1.sr);
                        ds1.dbr = dbr2;
                        if (hasAddon) MergeAddOn(addonSystems, sr1.Name, ds1, copiedCountry);
                        EM2_FixUprating(ds1.sr, upratingFactors2, upratingFactors1, countryShortName, 1 / alpha, 1, treatAsMarket);
                        allSystems.Add(ds1);
                        if (checkRadioMonetary.Checked)
                        {
                            DecompSystem ds2 = new DecompSystem();
                            ds2.sr = CountryConfigFacade.CopySystemRow(sr1.Name + "ind_on_" + dbr2.Name + systemNameExt, ccf.GetSystemRowByID(sr2.ID));
                            copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(ccf.GetSystemRowByID(sr1.ID), ds2.sr);
                            ds2.dbr = dbr2;
                            if (hasAddon) MergeAddOn(addonSystems, sr1.Name, ds2, copiedCountry);
                            EM2_FixUprating(ds2.sr, upratingFactors2, upratingFactors1, countryShortName, 1 / alpha, 2, treatAsMarket);
                            allSystems.Add(ds2);
                            DecompSystem ds3 = new DecompSystem();
                            ds3.sr = CountryConfigFacade.CopySystemRow(sr2.Name + "ind" + systemNameExt, ccf.GetSystemRowByID(sr2.ID));
                            copiedCountry.GetDataConfigFacade().CopyDBSystemConfigRows(ccf.GetSystemRowByID(sr2.ID), ds3.sr);
                            ds3.dbr = dbr1;
                            if (hasAddon) MergeAddOn(addonSystems, sr2.Name, ds3, copiedCountry);
                            EM2_FixUprating(ds3.sr, upratingFactors2, upratingFactors1, countryShortName, 1 / alpha, 3, treatAsMarket);
                            allSystems.Add(ds3);
                        }
                    }
                }

                currentAction = "writting decomposed " + countryShortName + " in the temp folder";

                copiedCountry.WriteXML(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles));

                currentAction = "running " + countryShortName + "'s decomposed systems";
                foreach (DecompSystem ds in allSystems)
                {
                    workers.Add(RunSystem(countryShortName, ds.sr.Name, dbr2.Name, EM2_CreateConfig(countryShortName, textBoxOutputPath.Text, ds.dbr, ds.sr)));
                    updateInfoLabel();
                }
            }
            catch (Exception ex)
            {
                em3_petInfo.AddSystemIndependentError($"There was a problem with {currentAction}: {ex.Message}");
            }
        }

        private void EM2_FixUprating(CountryConfig.SystemRow sr1, Dictionary<string, string> upratingFactors, Dictionary<string, string> upratingFactors2, string countryShortName, double alpha, int uprateType, bool treatAsMarket)
        {
            // first get the ils_origy & ils_ben components
            CountryConfig.PolicyRow ilsdef = sr1.GetPolicyRows().FirstOrDefault(p => p.Name.ToLower() == ("ilsdef_" + countryShortName).ToLower());
            if (ilsdef == null) ilsdef = sr1.GetPolicyRows().FirstOrDefault(p => p.Name.ToLower() == ("ildef_" + countryShortName).ToLower());
            if (ilsdef == null) return;

            CountryConfig.FunctionRow[] il_funcs = ilsdef.GetFunctionRows();
            List<string> ils_origy = ExpandIncomeList(DefVarName.ILSORIGY, il_funcs).Keys.ToList();
            List<string> ils_ben = ExpandIncomeList(DefVarName.ILSBEN, il_funcs).Keys.ToList();
            List<string> overrideInclude = ExpandIncomeList("pet_override", il_funcs).Where(x => x.Value).Select(x => x.Key).ToList();
            List<string> overrideExclude = ExpandIncomeList("pet_override", il_funcs).Where(x => !x.Value).Select(x => x.Key).ToList();

            List<string> reservedWords = new List<string> { "dataset", "def_factor", "factor_name", "factor_value", "factor_condition", "aggvar_name", "aggvar_part", "aggvar_tolerance", "warnifnofactor", "run_cond" };

            // Then apply them to the appropriate variables of s1
            foreach (CountryConfig.FunctionRow fr in sr1.GetPolicyRows().First(p => p.Name.ToLower() == ("Uprate_" + countryShortName).ToLower()).GetFunctionRows())
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
                                    pr.Value = FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors2[pr.Value.ToLower()]) * alpha).ToString());
                                else if (EM_Helpers.TryConvertToDouble(pr.Value.ToLower(), out val))
                                    pr.Value = FixDecSep((val * alpha).ToString());
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
                                            pr.Value = FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors[pr.Value.ToLower()]) * alpha).ToString());
                                        else if (EM_Helpers.TryConvertToDouble(pr.Value.ToLower(), out val))
                                            pr.Value = FixDecSep((val * alpha).ToString());
                                    }
                                    else if (uprateType == 2)
                                    {
                                        if (upratingFactors.ContainsKey(pr.Value.ToLower()))
                                            pr.Value = FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors[pr.Value.ToLower()])).ToString());
                                    }
                                }
                                else    // if it is non-market income
                                {
                                    if (uprateType == 2)
                                    {
                                        double val;
                                        if (upratingFactors2.ContainsKey(pr.Value.ToLower()))
                                            pr.Value = FixDecSep((EM_Helpers.SaveConvertToDouble(upratingFactors2[pr.Value.ToLower()]) / alpha).ToString());
                                        else if (EM_Helpers.TryConvertToDouble(pr.Value.ToLower(), out val))
                                            pr.Value = FixDecSep((val / alpha).ToString());
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
                foreach (CountryConfig.FunctionRow fr in sr1.GetPolicyRows().First(p => p.Name.ToLower() == ("Uprate_" + countryShortName).ToLower()).GetFunctionRows())
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
                            {
                                pr.Value = FixDecSep((EM_Helpers.SaveConvertToDouble(val) / alpha).ToString()) + valType;
                            }
                            else if (uprateType == 3)
                            {
                                pr.Value = FixDecSep((EM_Helpers.SaveConvertToDouble(val) * alpha).ToString()) + valType;
                            }
                        }
                    }
                }
            }
            try
            {
                // Then, fix the output filenames
                sr1.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_" + countryShortName).ToLower())
                    .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput")
                    .GetParameterRows().First(p => p.Name.ToLower() == "file")
                    .Value = sr1.Name + "_std";
                sr1.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_hh_" + countryShortName).ToLower())
                    .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput")
                    .GetParameterRows().First(p => p.Name.ToLower() == "file")
                    .Value = sr1.Name + "_std_hh";
                // Finally, if required, do the scaling
                if (checkRadioMarket.Checked)
                {
                    CountryConfig.FunctionRow fr = sr1.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_" + countryShortName).ToLower())
                            .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput");
                    CountryConfig.FunctionRow fr_hh = sr1.GetPolicyRows().First(p => p.Name.ToLower() == ("output_std_hh_" + countryShortName).ToLower())
                            .GetFunctionRows().First(f => f.Name.ToLower() == "defoutput");

                    if (fr.GetParameterRows().Count(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower()) == 0)
                    {
                        CountryConfig.ParameterRow fpr = fr.GetParameterRows().First(p => p.Name.ToLower() == "file");
                        CountryConfig.ParameterRow cpr = CountryConfigFacade.AddParameterRow(fpr, false,
                            DefPar.DefOutput.MultiplyMonetaryBy, DefinitionAdmin.GetParDefinition(DefFun.DefOutput, DefPar.DefOutput.MultiplyMonetaryBy));
                        cpr.Value = FixDecSep((1 / alpha).ToString());
                    }
                    else
                    {
                        CountryConfig.ParameterRow mpr = fr.GetParameterRows().First(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower());
                        double d;
                        if (!EM_Helpers.TryConvertToDouble(mpr.Value, out d)) d = 1;
                        mpr.Value = FixDecSep((d / alpha).ToString());
                    }

                    if (fr_hh.GetParameterRows().Count(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower()) == 0)
                    {
                        CountryConfig.ParameterRow fpr_hh = fr_hh.GetParameterRows().First(p => p.Name.ToLower() == "file");
                        CountryConfig.ParameterRow cpr = CountryConfigFacade.AddParameterRow(fpr_hh, false,
                            DefPar.DefOutput.MultiplyMonetaryBy, DefinitionAdmin.GetParDefinition(DefFun.DefOutput, DefPar.DefOutput.MultiplyMonetaryBy));
                        cpr.Value = FixDecSep((1 / alpha).ToString());
                    }
                    else
                    {
                        CountryConfig.ParameterRow mpr_hh = fr_hh.GetParameterRows().First(p => p.Name.ToLower() == DefPar.DefOutput.MultiplyMonetaryBy.ToLower());
                        double d;
                        if (!EM_Helpers.TryConvertToDouble(mpr_hh.Value, out d)) d = 1;
                        mpr_hh.Value = FixDecSep((d / alpha).ToString());
                    }
                }
            }
            catch
            {
                throw new Exception("Problem in default output functions.");
            }
        }
    }
}
