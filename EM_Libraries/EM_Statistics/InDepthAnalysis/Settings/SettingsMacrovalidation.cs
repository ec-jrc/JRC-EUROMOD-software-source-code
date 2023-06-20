using EM_Common;
using EM_Statistics;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EM_Statistics.ExternalStatistics;

namespace EM_Statistics.InDepthAnalysis
{
    
    public class ExternalStatisticsGlobal
    {
        public Dictionary<string, ExternalStatisticsCountry> AllCountries = new Dictionary<string, ExternalStatisticsCountry>();
    }

    public class ExternalStatisticsCountry
    {
        public Dictionary<string, ExternalStatisticsIncomeVariable> AllIncomeVariables = new Dictionary<string, ExternalStatisticsIncomeVariable>();
    }

    public class ExternalStatisticsIncomeVariable
    {
        // The key of each dictionary will be: incomelist_varname_year, and the value will be the value
        internal string incomelist = string.Empty;
        internal string varName = string.Empty;
        internal string description = string.Empty;
        internal Dictionary<string, string> amounts;
        internal Dictionary<string, string> beneficiaries;
        internal Dictionary<string, string> level;
        internal string source;
        internal string comments;
        internal string destination = string.Empty;

        // Country XML
        internal bool? baselineSubstract = false;
        internal List<bool?> reformsSubstract = new List<bool?>();
        internal Dictionary<string, bool> exists = new Dictionary<string, bool>();  // this should supposedly read from Country XML 

        public string GetAmount(string key)
        {
            if (amounts.ContainsKey(key)) return amounts[key];
            else return null;
        }
        
    }




    public class SettingsMacrovalidation
    {
        //Page 1
        string page1Name = "Table A3.1";
        string tableName_OriginalIncomeTaxpayersBeneficiaries = "OriginalIncomeTaxpayersBeneficiaries";
        string tableName_OriginalIncomeSourcesAndComments = "OriginalIncomeSourcesAndComments";

        //Page 2
        string page2Name = "Table A3.2";
        string tableName_OriginalIncomeAmounts = "OriginalIncomeAmounts";

        //Page 3
        string page3Name = "Table A3.3";
        string tableName_TaxesSICPayers = "TaxesSICPayers";
        string tableName_TaxesSICPayersSourcesAndComments = "TaxesSICPayersSourcesAndComment";

        //Page 4
        string page4Name = "Table A3.4";
        string tableName_TaxesSICAmounts = "TaxesSICAmounts";

        //Page 5
        string page5Name = "Table A3.5";
        string tableName_BenefitsRecipients = "BenefitsRecipients";
        string tableName_BenefitsRecipientsSourcesAndComments = "BenefitsRecipientsSourcesAndComment";

        //Page 6
        string page6Name = "Table A3.6";
        string tableName_BenefitsAmounts = "BenefitsAmounts";

        //Page 7
        string page7Name = "Table A3.7";
        string tableName_DistributionalInequality = "DistributionalInequality";

        //Page 8
        string page8Name = "Table A3.8";
        string tableName_DistributionalPoverty = "DistributionalPoverty";

        //Page 9
        string page9Name = "Metadata";
        string tableName_Metadata = "Metadata";


        //Common constants for all tables
        private const string colName_Baseline = "Baseline";
        private const string colName_External = "External";
        private const string colName_Comments = "Comments";
        private const string colName_Source = "Source";
        private const string colName_PercentageDifference = "PercentageDifference";
        private const string colName_SILCValue = "SILCValue";
        private const string colName_SILCRatio = "SILCRatio";
        private const string colName_Simulated = "Simulated";
        private const string simulated_Ending = "_s";

        // template columns that will be duplicated for each file
        readonly string[] baseCols = { colName_Baseline, colName_External, colName_PercentageDifference, colName_SILCValue, colName_SILCRatio };

        public Settings settings = null;


        public SettingsMacrovalidation() { }

        public void getSystemNamesAndYears(out List<string> systemNames, out List<int> systemYears)
        {
            systemNames = new List<string>();
            systemYears = new List<int>();


            //First, let's get a list of all systems names
            foreach (BaselineReformPackage baselineReformPackage in settings.baselineReformPackages)
            {
                if(baselineReformPackage.baseline != null && baselineReformPackage.baseline.systemInfo != null && !string.IsNullOrEmpty(baselineReformPackage.baseline.systemInfo.systemName))
                {
                    if(!systemNames.Contains(baselineReformPackage.baseline.systemInfo.systemName)) systemNames.Add(baselineReformPackage.baseline.systemInfo.systemName);
                }

                if (baselineReformPackage.reforms != null && baselineReformPackage.reforms.Count > 0)
                {
                    List<BaselineReformPackage.BaselineOrReform> reforms = baselineReformPackage.reforms;

                    foreach (BaselineReformPackage.BaselineOrReform reform in reforms)
                    {
                        if(reform.systemInfo != null && reform.systemInfo.systemName != null && !string.IsNullOrEmpty(reform.systemInfo.systemName))
                        {
                            systemNames.Add(reform.systemInfo.systemName);
                        }
                    }
                }
            }

            //Once we have all the system names, we need the system years
            foreach (string systemName in systemNames)
            {
                string [] nameYear = systemName.Split('_');

                if(nameYear.Length == 2)
                {
                    int year = -1;
                    try { year = int.Parse(nameYear[1]); } catch(Exception){ }
                    if (year!= -1)
                    {
                        systemYears.Add(year);
                    }
                }
            }

            systemYears.Sort();
        }

        public void ModifyTemplate(TemplateApi templateApi, out List<Template.TemplateInfo.UserVariable> systemSpecificVars, Dictionary<string, List<string>> yearsAndFiles, Dictionary<string, Dictionary<string, SettingsMacrovalidation.ILVarSummary>> allIncomelists,
            Dictionary<string, string> amounts, Dictionary<string, string> beneficiaries, Dictionary<string, string> level, Dictionary<string, string> source, Dictionary<string, string> comments, Dictionary<string, string> destination, Dictionary<string, Dictionary<string, string>> inequality, Dictionary<string, Dictionary<string, string>> poverty)
        {
            systemSpecificVars = new List<Template.TemplateInfo.UserVariable>();

            // read system info
            List<int> systemYears = new List<int>();
            List<string> systemNames = new List<string>();

            foreach (string year in yearsAndFiles.Keys.ToList())
            {
                int y = int.Parse(year);
                if (!systemYears.Contains(y)) systemYears.Add(y);
                foreach (string fileName in yearsAndFiles[year])
                {
                    // FIND A WAY TO CONSISTENTLY READ THE SYSTEM NAME FROM A COMMON FUNCTION THAT UNDERSTANDS THE LOG! 
                    string systemName = Path.GetFileNameWithoutExtension(fileName);
                    if (systemName.EndsWith("_std")) systemName = systemName.Substring(0, systemName.Length - 4);
                    if (!systemNames.Contains(systemName)) systemNames.Add(systemName);
                }
            }



            //GetAllSysILComposition();

            //Page 1 - Table 1
            foreach (string il in new List<string> { InDepthDefinitions.ILS_EARNS, InDepthDefinitions.OtherOriginalIncome })
            {
                ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page1Name, tableName_OriginalIncomeTaxpayersBeneficiaries, systemYears, systemNames, il, beneficiaries, level, destination, out List <Template.TemplateInfo.UserVariable> ssv, false, false, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page1Name, tableName_OriginalIncomeTaxpayersBeneficiaries, systemYears, systemNames, InDepthDefinitions.ILS_EXTSTAT_OTHER, beneficiaries, level, destination, out List<Template.TemplateInfo.UserVariable> ssv1, false, false, allIncomelists);
            systemSpecificVars.AddRange(ssv1);

            //Page 1 - Table 2: Sources and acomments
            foreach (string il in new List<string> { InDepthDefinitions.ILS_EARNS, InDepthDefinitions.OtherOriginalIncome })
            {
                ModifyTemplate_SourcesAndComments(templateApi, page1Name, tableName_OriginalIncomeSourcesAndComments, systemYears, il, source, comments, destination, out List <Template.TemplateInfo.UserVariable> ssv, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_SourcesAndComments(templateApi, page1Name, tableName_OriginalIncomeSourcesAndComments, systemYears, InDepthDefinitions.ILS_EXTSTAT_OTHER, source, comments, destination, out List<Template.TemplateInfo.UserVariable> ssv2, allIncomelists);
            systemSpecificVars.AddRange(ssv2);
            //Page 2: Original income in EUROMOD - Annual amounts (millions)
            //Page 2 - Table 1
            foreach (string il in new List<string> { InDepthDefinitions.ILS_EARNS, InDepthDefinitions.OtherOriginalIncome })
            {
                ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page2Name, tableName_OriginalIncomeAmounts, systemYears, systemNames, il, amounts, level, destination, out List<Template.TemplateInfo.UserVariable> ssv, true, false, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page2Name, tableName_OriginalIncomeAmounts, systemYears, systemNames, InDepthDefinitions.ILS_EXTSTAT_OTHER, amounts, level, destination, out List<Template.TemplateInfo.UserVariable> ssv3, true, false, allIncomelists);
            systemSpecificVars.AddRange(ssv3);

            
            //Page 3: Taxes and SIC - Number of payers (thousands)
            foreach (string il in new List<string> { InDepthDefinitions.ILS_TAX, InDepthDefinitions.ILS_SICEE, InDepthDefinitions.ILS_SICSE, InDepthDefinitions.ILS_SICER, InDepthDefinitions.ILS_SICCT, InDepthDefinitions.ILS_SICOT })
            {
                ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page3Name, tableName_TaxesSICPayers, systemYears, systemNames, il, beneficiaries, level, destination, out List <Template.TemplateInfo.UserVariable> ssv, false, true, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page3Name, tableName_TaxesSICPayers, systemYears, systemNames, InDepthDefinitions.ILS_EXTSTAT_OTHER, beneficiaries, level, destination, out List<Template.TemplateInfo.UserVariable> ssv4, false, true, allIncomelists);
            systemSpecificVars.AddRange(ssv4);

            //Page 3 - Table 2: Sources and acomments
            foreach (string il in new List<string> { InDepthDefinitions.ILS_TAX, InDepthDefinitions.ILS_SICEE, InDepthDefinitions.ILS_SICSE, InDepthDefinitions.ILS_SICER, InDepthDefinitions.ILS_SICCT, InDepthDefinitions.ILS_SICOT })
            {
                ModifyTemplate_SourcesAndComments(templateApi, page3Name, tableName_TaxesSICPayersSourcesAndComments, systemYears, il, source, comments, destination, out List <Template.TemplateInfo.UserVariable> ssv, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_SourcesAndComments(templateApi, page3Name, tableName_TaxesSICPayersSourcesAndComments, systemYears, InDepthDefinitions.ILS_EXTSTAT_OTHER, source, comments, destination, out List<Template.TemplateInfo.UserVariable> ssv5, allIncomelists);
            systemSpecificVars.AddRange(ssv5);

            //Page 4: Table A3.4. Taxes and SIC - Annual amounts (million EUR)
            foreach (string il in new List<string> { InDepthDefinitions.ILS_TAX, InDepthDefinitions.ILS_SICEE, InDepthDefinitions.ILS_SICSE, InDepthDefinitions.ILS_SICER, InDepthDefinitions.ILS_SICCT, InDepthDefinitions.ILS_SICOT })
            {
                //ModifyTemplate_TaxesSICAmounts(templateApi, il, taxpayersCondition, taxpayersConditionParameters, out List<Template.TemplateInfo.UserVariable> ssv, systemYears, amounts);
                ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page4Name, tableName_TaxesSICAmounts, systemYears, systemNames, il, amounts, level, destination, out List <Template.TemplateInfo.UserVariable> ssv, true, true, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page4Name, tableName_TaxesSICAmounts, systemYears, systemNames, InDepthDefinitions.ILS_EXTSTAT_OTHER, amounts, level, destination, out List<Template.TemplateInfo.UserVariable> ssv6, true, true, allIncomelists);
            systemSpecificVars.AddRange(ssv6);

            //Page 5: Table A3.5. Benefits - Number of recipients (thousands)
            foreach (string il in new List<string> { InDepthDefinitions.ILS_PEN, InDepthDefinitions.ILS_BENMT, InDepthDefinitions.ILS_BENNT })
            {
                ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page5Name, tableName_BenefitsRecipients, systemYears, systemNames, il, beneficiaries, level, destination, out List <Template.TemplateInfo.UserVariable> ssv, false, true, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page5Name, tableName_BenefitsRecipients, systemYears, systemNames, InDepthDefinitions.ILS_EXTSTAT_OTHER, beneficiaries, level, destination, out List<Template.TemplateInfo.UserVariable> ssv7, false, true, allIncomelists);
            systemSpecificVars.AddRange(ssv7);

            //Page 5 - Table 2: Sources and acomments
            foreach (string il in new List<string> { InDepthDefinitions.ILS_PEN, InDepthDefinitions.ILS_BENMT, InDepthDefinitions.ILS_BENNT })
            {
                ModifyTemplate_SourcesAndComments(templateApi, page5Name, tableName_BenefitsRecipientsSourcesAndComments, systemYears, il, source, comments, destination, out List <Template.TemplateInfo.UserVariable> ssv, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_SourcesAndComments(templateApi, page5Name, tableName_BenefitsRecipientsSourcesAndComments, systemYears, InDepthDefinitions.ILS_EXTSTAT_OTHER, source, comments, destination, out List<Template.TemplateInfo.UserVariable> ssv8, allIncomelists);
            systemSpecificVars.AddRange(ssv8);

            //Page 6: Table A3.6. Benefits - Annual amounts (million EUR)
            foreach (string il in new List<string> { InDepthDefinitions.ILS_PEN, InDepthDefinitions.ILS_BENMT, InDepthDefinitions.ILS_BENNT })
            {
                ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page6Name, tableName_BenefitsAmounts, systemYears, systemNames, il, amounts, level, destination, out List <Template.TemplateInfo.UserVariable> ssv, true, true, allIncomelists);
                systemSpecificVars.AddRange(ssv);
            }
            // add other
            ModifyTemplate_BeneficiariesTaxpayersAmounts(templateApi, page6Name, tableName_BenefitsAmounts, systemYears, systemNames, InDepthDefinitions.ILS_EXTSTAT_OTHER, amounts, level, destination, out List<Template.TemplateInfo.UserVariable> ssv9, true, true, allIncomelists);
            systemSpecificVars.AddRange(ssv9);

            //Page 7: Table A3.7. Distribution of equivalised disposable income
            {
                ModifyTemplate_Distributional(templateApi, page7Name, tableName_DistributionalInequality, systemYears, systemNames, inequality, out List<Template.TemplateInfo.UserVariable> ssv);
                systemSpecificVars.AddRange(ssv);
            }

            //Page 8: Table A3.8. At-risk-of-poverty rates (%) by gender and age
            {
                ModifyTemplate_Distributional(templateApi, page8Name, tableName_DistributionalPoverty, systemYears, systemNames, poverty, out List<Template.TemplateInfo.UserVariable> ssv);
                systemSpecificVars.AddRange(ssv);
            }

            //Page 9: Metadata
            {
                ModifyTemplate_Metadata(templateApi, page9Name, tableName_Metadata, systemYears);
            }/**/
        }


        private void ModifyTemplate_BeneficiariesTaxpayersAmounts(TemplateApi templateApi, string pageName, string tableName,
                                                                  List<int> systemYears, List<string> systemNames, string incomelistName, Dictionary<string, string> amounts, Dictionary<string, string> levels,
                                                                  Dictionary<string, string> destination, out List<Template.TemplateInfo.UserVariable> sysSpecILFactors, bool isAmounts, bool doSILC, 
                                                                  Dictionary<string, Dictionary<string, SettingsMacrovalidation.ILVarSummary>> allIncomelists)
        {
            string Magnitude = isAmounts ? " * 12 / 1000000" : " / 1000";    // in millions annually, or in thousands, for amounts and recievers respectively 
            sysSpecILFactors = new List<Template.TemplateInfo.UserVariable>();
            // First of all, read the variables for which we need to generate rows
            List<ILVarSummary> incomeListComponents = null;

            /*
            foreach (string key in destination.Keys)
            {
                ILVarSummary ivs = new ILVarSummary();
                incomeListComponents.Add(ivs);
            }/**/

            if (incomelistName.Equals(InDepthDefinitions.OtherOriginalIncome)) //variable which are in ils_origy and not in ils_earns, for tables 1 and two
            {
                //List<ILVarSummary> incomeListComponentsIlsOrigy = GetAllSysILCompositionMacrovalidation(InDepthDefinitions.ILS_ORIGY, destination);
                //List<ILVarSummary> incomeListComponentsIlsEarns = GetAllSysILCompositionMacrovalidation(InDepthDefinitions.ILS_EARNS, destination);
                List<ILVarSummary> incomeListComponentsIlsOrigy = allIncomelists[InDepthDefinitions.ILS_ORIGY].Values.ToList();
                List<ILVarSummary> incomeListComponentsIlsEarns = allIncomelists[InDepthDefinitions.ILS_EARNS].Values.ToList();
                incomeListComponents = new List<ILVarSummary>();

                foreach (var ivs in incomeListComponentsIlsOrigy)
                {
                    ILVarSummary summ = null;
                    summ = incomeListComponentsIlsEarns.Find(v => v.varName.Equals(ivs.varName));
                    if (summ == null)
                    {
                        incomeListComponents.Add(ivs);
                    }
                }
            }
            else
            {
                //incomeListComponents = GetAllSysILCompositionMacrovalidation(incomelistName, destination);
                incomeListComponents = allIncomelists[incomelistName].Values.ToList();
                // add the filter with the user defined condition to the row referring the income-list (e.g. ils_tax)
                //                templateApi.ModifyCellFilter_Row(new Template.Filter() { formulaString = $"{Settings.DATA_VAR(incomelistName)}{condition}", parameters = conditionParameters },
                //                    pageName, mainTable, $"{mainTable}_{incomelistName}");
            }

            bool hasHidden = false;
            for (int i = incomeListComponents.Count - 1; i >= 0; i--)
            {
                string x = incomeListComponents[i].destination;
                if (x.Equals(InDepthDefinitions.DESTINATION_NONE) || !tableName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))
                {
                    hasHidden = true;
                    incomeListComponents.RemoveAt(i);
                }
            }
            if (hasHidden)
            {
                Template.Page.Table.Row row = templateApi.GetRow(pageName, tableName, tableName + "_" + incomelistName);
                if (row != null) row.title += " (NOTE: some components are hidden!)";
            }
            // if this is ext_other, and there are no matches, delete the row and return
            if (incomelistName.Equals(InDepthDefinitions.ILS_EXTSTAT_OTHER) && incomeListComponents.Count == 0)
            {
                templateApi.ModifyRows(new Template.Page.Table.Row() { name = $"{tableName}_{incomelistName}" }, pageName, tableName, TemplateApi.ModifyMode.Delete);
                return;
            }

            // Fix the incomelist name for external vars
            string incomeListExt = incomelistName;
            if (incomelistName.Equals(InDepthDefinitions.OtherOriginalIncome) || incomelistName.Equals(InDepthDefinitions.ILS_EARNS)) //variable which are in ils_origy and not in ils_earns
            {
                incomeListExt = InDepthDefinitions.ILS_ORIGY;
            }

            // Then prepare the columns 
            foreach (string colName in baseCols)
            {
                int colNum = (colName == colName_External || colName == colName_SILCValue) ? systemYears.Count : systemNames.Count;
                // if there is a known base column, copy it for all files, and make sure that only the first has a separator
                Template.Page.Table.Column column = templateApi.GetColumn(pageName, tableName, colName);
                if (column != null)
                {
                    for (int i = 0; i < colNum; i++)
                    {
                        string ys = (colName == colName_External || colName == colName_SILCValue) ? systemYears[i].ToString() : systemNames[i];
                        if (!templateApi.CopyColumn(pageName, tableName, colName, colName + ys, false, TemplateApi.AddWhere.Before))
                            System.Windows.Forms.MessageBox.Show("Cannot copy column '" + (colName + ys) + "' in template");
                        else
                        {
                            Template.Page.Table.Column col = templateApi.GetColumn(pageName, tableName, colName + ys);
                            col.hasSeparatorBefore = i == 0;
                            if (col.cellAction != null && col.cellAction.formulaString != null)
                                col.cellAction.formulaString = fixFormulaString(col.cellAction.formulaString, ys);
                        }
                    }
                    // finally, delete the template column
                    templateApi.ModifyColumns(column, pageName, tableName, TemplateApi.ModifyMode.Delete);
                }

                // if there are base custom cells for this column, also multiply those and delete the base ones
                List<Template.Page.Table.Cell> customCells = templateApi.GetCells(pageName, tableName, colName);
                if (customCells.Count > 0)
                {
                    foreach (Template.Page.Table.Cell cell in customCells)
                    {
                        for (int i = 0; i < colNum; i++)
                        {
                            string ys = (colName == colName_External || colName == colName_SILCValue) ? systemYears[i].ToString() : systemNames[i];
                            string newCellName = Settings.MakeId();
                            templateApi.CopyCell(pageName, tableName, cell, newCellName);
                            Template.Page.Table.Cell newCell = templateApi.GetCell(pageName, tableName, newCellName);
                            newCell.cellAction.formulaString = fixFormulaString(newCell.cellAction.formulaString, ys);
                        }
                        templateApi.ModifyCells(cell, pageName, tableName, TemplateApi.ModifyMode.Delete);
                    }
                }
            }

            // then fix the titles of known header columns
            for (int i = 0; i < systemNames.Count; i++)
            {
                // we give baseline a multiNo number, so that it takes its data from that file
                templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_Baseline + systemNames[i], title = "Baseline values<br>" + systemNames[i], multiNo = i.ToString() }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
                // we don't need multiNo for the others, because we generate them
                templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_PercentageDifference + systemNames[i], title = "Base/Ext ratio<br>" + systemNames[i] }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);

                if (doSILC)
                    templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_SILCRatio + systemNames[i], title = "Base/SILC ratio<br>" + systemNames[i] }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
            }
            for (int i = 0; i < systemYears.Count; i++)
            {
                // we don't need multiNo for the others, because we generate them
                templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_External + systemYears[i], title = "External values<br>" + systemYears[i] }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);

                if (doSILC)
                    templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_SILCValue + systemYears[i], title = "SILC values<br>" + systemYears[i] }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
            }

            // Then the cells
            // First of all, check if we should add the aggregate incomelist values (but not for the origy tables)
            if (!incomeListExt.Equals(InDepthDefinitions.ILS_ORIGY))
            {
                // clear the sim cell 
                string cellNameSim = Settings.MakeId();
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameSim, rowName = $"{tableName}_{incomeListExt}", colName = colName_Simulated },
                    pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error31");
                if (!templateApi.ModifyCellAction_Cell(new Template.Action { calculationType = HardDefinitions.CalculationType.Empty },
                    pageName, tableName, cellNameSim, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error32");
                for (int i = 0; i < systemNames.Count; i++)
                {
                    if (doSILC)
                    {
                        string cellSILCdiff = Settings.MakeId();
                        // Clear the SILC cells
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellSILCdiff, rowName = $"{tableName}_{incomeListExt}", colName = colName_SILCRatio + systemNames[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error43");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { calculationType = HardDefinitions.CalculationType.Empty },
                            pageName, tableName, cellSILCdiff, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error44");
                    }
                    string cellNameBaseline = Settings.MakeId(), cellNameExternal = Settings.MakeId(), cellNameDiff = Settings.MakeId();
                    if (!(amounts.TryGetValue(incomeListExt + "_" + systemNames[i].Split('_')[1], out string dStr) && double.TryParse(dStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double d)) || !destination.ContainsKey(incomeListExt) || destination[incomeListExt].Equals(InDepthDefinitions.DESTINATION_NONE))
                    {
                        // Clear the remaining cells
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameBaseline, rowName = $"{tableName}_{incomeListExt}", colName = colName_Baseline + systemNames[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error51");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { calculationType = HardDefinitions.CalculationType.Empty },
                            pageName, tableName, cellNameBaseline, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error52");
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameDiff, rowName = $"{tableName}_{incomeListExt}", colName = colName_PercentageDifference + systemNames[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error55");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { calculationType = HardDefinitions.CalculationType.Empty },
                            pageName, tableName, cellNameDiff, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error56");
                        continue;
                    }
                    if (destination.ContainsKey(incomeListExt) && !destination[incomeListExt].Equals(InDepthDefinitions.DESTINATION_NONE))
                    {
                        // Add the Baseline cells
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameBaseline, rowName = $"{tableName}_{incomeListExt}", colName = colName_Baseline + systemNames[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error61");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { formulaString = $"{Settings.DATA_VAR(incomeListExt)}" + Magnitude, _calculationLevel = GetCalculationLevel(levels, incomeListExt + "__" + systemYears[i]) },
                            pageName, tableName, cellNameBaseline, TemplateApi.ModifyMode.MergeReplace)) System.Windows.Forms.MessageBox.Show("Error62");
                    }
                }
                for (int i = 0; i < systemYears.Count; i++)
                {
                    string cellNameExternal = Settings.MakeId();
                    if (doSILC)
                    {
                        string cellSILC = Settings.MakeId();
                        // Clear the SILC cells
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellSILC, rowName = $"{tableName}_{incomeListExt}", colName = colName_SILCValue + systemYears[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error41");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { calculationType = HardDefinitions.CalculationType.Empty },
                            pageName, tableName, cellSILC, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error42");
                    }

                    if (!(amounts.TryGetValue(incomeListExt + "_" + systemYears[i], out string dStr) && double.TryParse(dStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double d)) || !destination.ContainsKey(incomeListExt) || destination[incomeListExt].Equals(InDepthDefinitions.DESTINATION_NONE))
                    {
                        // Clear the remaining cells
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameExternal, rowName = $"{tableName}_{incomeListExt}", colName = colName_External + systemYears[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error53");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { calculationType = HardDefinitions.CalculationType.Empty },
                            pageName, tableName, cellNameExternal, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error54");
                        continue;
                    }

                    if (destination.ContainsKey(incomeListExt) && !destination[incomeListExt].Equals(InDepthDefinitions.DESTINATION_NONE))
                    {
                        // Add the External values
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameExternal, rowName = $"{tableName}_{incomeListExt}", colName = colName_External + systemYears[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error63");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { formulaString = $"{Settings.USR_VAR(incomeListExt + "_" + systemYears[i] + "_" + (isAmounts ? "amount" : "count"))}" },
                            pageName, tableName, cellNameExternal, TemplateApi.ModifyMode.MergeReplace)) System.Windows.Forms.MessageBox.Show("Error64");
                        // Create the User Variables for the External values
                        if (!templateApi.ModifyUserVariables(new Template.TemplateInfo.UserVariable() { name = incomeListExt + "_" + systemYears[i] + "_" + (isAmounts ? "amount" : "count"), inputType = HardDefinitions.UserInputType.Numeric }))
                            System.Windows.Forms.MessageBox.Show("Error65");
                        // Make sure the external value exists, otherwise cancel and ask user to open and save the External Statistics form and save
                        // (to auto-update any missing values e.g. in case someone added more variables to an incomelist and never updated the external statistics)
                        sysSpecILFactors.Add(new Template.TemplateInfo.UserVariable()
                        {
                            name = incomeListExt + "_" + systemYears[i] + "_" + (isAmounts ? "amount" : "count"),
                            value = d.ToString(CultureInfo.InvariantCulture)    // if value does not exist, fill with NaN
                        });
                    }
                }
            }

            incomeListComponents.Reverse();
            // Then add the rows for each component-variable
            foreach (var ivs in incomeListComponents)
            {
                if (string.IsNullOrEmpty(ivs.varName)) continue;
                // Create the row and cell IDs, so that we can reference them 
                string rowName = Settings.MakeId(), cellNameSimulated = Settings.MakeId();
                string[] cellNameBaseline = new string[systemNames.Count], cellNameSILC = new string[systemYears.Count], cellNameExternal = new string[systemYears.Count];
                for (int i = 0; i < systemNames.Count; i++)
                    cellNameBaseline[i] = Settings.MakeId();
                for (int j = 0; j < systemYears.Count; j++)
                {
                    cellNameSILC[j] = Settings.MakeId();
                    cellNameExternal[j] = Settings.MakeId();
                }
                
                // Create the optional variable (default NaN) that reads the baseline value from file
                if (!templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable()
                    { name = ivs.varName, readVar = ivs.varName, defaultValue = double.NaN })) System.Windows.Forms.MessageBox.Show("Error1");
                // Create the optional variable (default NaN) for the SILC value if required
                if (doSILC && ivs.varName.EndsWith("_s"))
                    if (!templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable() 
                        { name = ivs.varName.Remove(ivs.varName.Length - 2), readVar = ivs.varName.Remove(ivs.varName.Length - 2), defaultValue = double.NaN }))
                            System.Windows.Forms.MessageBox.Show("Error11");

                // Add the actual row
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowName, title = $"{SubstractToText(ivs)} {ivs.description} ({ivs.varName})" },
                    pageName, tableName,
                    TemplateApi.ModifyMode.AddNew, TemplateApi.AddWhere.After, $"{tableName}_{incomelistName}")) System.Windows.Forms.MessageBox.Show("Error2");

                // Add the Simulated cell
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameSimulated, rowName = rowName, colName = colName_Simulated },
                    pageName, tableName, TemplateApi.ModifyMode.AddOrReplace, false)) System.Windows.Forms.MessageBox.Show("Error3");
                if (!templateApi.ModifyCellAction_Cell(new Template.Action()
                {
                    calculationType = HardDefinitions.CalculationType.Info,
                    formulaString = ivs.varName.EndsWith(simulated_Ending) ? "Y" : "N"
                },
                    pageName, tableName, cellNameSimulated, TemplateApi.ModifyMode.AddNew, false)) System.Windows.Forms.MessageBox.Show("Error4");

                string varKey = incomeListExt + "_" + ivs.varName;

                for (int i = 0; i < systemNames.Count; i++)
                {
                    // Add the Baseline cells
                    if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameBaseline[i], rowName = rowName, colName = colName_Baseline + systemNames[i] },
                        pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error5");
                    if (!templateApi.ModifyCellAction_Cell(new Template.Action { formulaString = ivs.exists[i] ? $"{Settings.DATA_VAR(ivs.varName)}" + Magnitude : double.NaN.ToString(), _calculationLevel = GetCalculationLevel(levels, incomeListExt + "_" + ivs.varName + "_" + systemNames[i].Split('_')[1]) },
                        pageName, tableName, cellNameBaseline[i], TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error6");
                }
                for (int i = 0; i < systemYears.Count; i++)
                {
                    // Add the SILC values
                    if (doSILC)
                    {
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameSILC[i], rowName = rowName, colName = colName_SILCValue + systemYears[i] },
                            pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error7");
                        if (!templateApi.ModifyCellAction_Cell(new Template.Action { formulaString = ivs.exists[i] ? $"{Settings.DATA_VAR((ivs.varName.EndsWith("_s") ? ivs.varName.Remove(ivs.varName.Length - 2) : ivs.varName))}" + Magnitude : double.NaN.ToString() },
                            pageName, tableName, cellNameSILC[i], TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error8");
                    }
                    // Add the External values
                    if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameExternal[i], rowName = rowName, colName = colName_External + systemYears[i] },
                        pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error9");
                    if (!templateApi.ModifyCellAction_Cell(new Template.Action { formulaString = $"{Settings.USR_VAR(varKey + "_" + systemYears[i] + "_" + (isAmounts ? "amount" : "count"))}" },
                        pageName, tableName, cellNameExternal[i], TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error10");
                    // Create the User Variables for the External values
                    if (!templateApi.ModifyUserVariables(new Template.TemplateInfo.UserVariable() { name = varKey + "_" + systemYears[i] + "_" + (isAmounts ? "amount" : "count"), inputType = HardDefinitions.UserInputType.Numeric }))
                        System.Windows.Forms.MessageBox.Show("Error11");
                    // Make sure the external value exists, otherwise cancel and ask user to open and save the External Statistics form and save
                    // (to auto-update any missing values e.g. in case someone added more variables to an incomelist and never updated the external statistics)
                    sysSpecILFactors.Add(new Template.TemplateInfo.UserVariable()
                    {
                        name = varKey + "_" + systemYears[i] + "_" + (isAmounts ? "amount" : "count"),
                        value = ((amounts.TryGetValue(varKey + "_" + systemYears[i], out string dStr) && double.TryParse(dStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double d)) ? d : double.NaN).ToString(CultureInfo.InvariantCulture)    // if value does not exist, fill with NaN
                    });
                }
            // finally delete the empty placeholder row
            }
//            if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = $"{tableName}_{incomelistName}" },
//                pageName, tableName,
//                TemplateApi.ModifyMode.Delete)) System.Windows.Forms.MessageBox.Show("Error12");
        }

        string fixFormulaString(string formula, string colName)
        {
            string y = colName;
            if (colName.Contains("_")) y = colName.Split('_')[1];   // calculate year from system
            foreach (string baseCol in baseCols)
                formula = formula.Replace("[" + baseCol + "]", "[" + baseCol + ((baseCol == colName_External || baseCol == colName_SILCValue) ? y : colName) + "]");
            return formula;
        }

        private string GetCalculationLevel(Dictionary<string, string> levels, string index)
        {
            if (levels == null || string.IsNullOrEmpty(index) || !levels.ContainsKey(index)) return HardDefinitions.DefaultCalculationLevels.INDIVIDUAL;
            return levels[index] == HardDefinitions.DefaultCalculationLevels.HOUSEHOLD ? HardDefinitions.DefaultCalculationLevels.HOUSEHOLD : HardDefinitions.DefaultCalculationLevels.INDIVIDUAL;
        }

        private void ModifyTemplate_SourcesAndComments(TemplateApi templateApi, string pageName, string tableName,
                                                       List<int> systemYears, string incomelistName, Dictionary<string, string> source, Dictionary<string, string> comments,
                                                       Dictionary<string, string> destination, out List<Template.TemplateInfo.UserVariable> sysSpecILFactors,
                                                       Dictionary<string, Dictionary<string, SettingsMacrovalidation.ILVarSummary>> allIncomelists)
        {
            sysSpecILFactors = new List<Template.TemplateInfo.UserVariable>();

            List<ILVarSummary> incomeListComponents = null;
            if (incomelistName.Equals(InDepthDefinitions.OtherOriginalIncome)) //variable which are in ils_origy and not in ils_earns
            {
                //List<ILVarSummary> incomeListComponentsIlsOrigy = GetAllSysILCompositionMacrovalidation(InDepthDefinitions.ILS_ORIGY, destination);
                //List<ILVarSummary> incomeListComponentsIlsEarns = GetAllSysILCompositionMacrovalidation(InDepthDefinitions.ILS_EARNS, destination);
                List<ILVarSummary> incomeListComponentsIlsOrigy = allIncomelists[InDepthDefinitions.ILS_ORIGY].Values.ToList();
                List<ILVarSummary> incomeListComponentsIlsEarns = allIncomelists[InDepthDefinitions.ILS_EARNS].Values.ToList();
                incomeListComponents = new List<ILVarSummary>();

                foreach (var ivs in incomeListComponentsIlsOrigy)
                {
                    ILVarSummary summ = null;
                    summ = incomeListComponentsIlsEarns.Find(v => v.varName.Equals(ivs.varName));
                    if (summ == null)
                    {
                        incomeListComponents.Add(ivs);
                    }
                }
            }
            else
            {
                //incomeListComponents = GetAllSysILCompositionMacrovalidation(incomelistName, destination);
                incomeListComponents = allIncomelists[incomelistName].Values.ToList();
            }

            bool hasHidden = false;
            for (int i = incomeListComponents.Count - 1; i >= 0; i--)
            {
                string x = incomeListComponents[i].destination;
                if (x.Equals(InDepthDefinitions.DESTINATION_NONE) || !tableName.StartsWith(x, StringComparison.InvariantCultureIgnoreCase))
                {
                    hasHidden = true;
                    incomeListComponents.RemoveAt(i);
                }
            }
            if (hasHidden)
            {
                Template.Page.Table.Row row = templateApi.GetRow(pageName, tableName, tableName + "_" + incomelistName);
                if (row != null) row.title += " (NOTE: some components are hidden!)";
            }
            // if this is ext_other, and there are no matches, delete the row and return
            if (incomelistName.Equals(InDepthDefinitions.ILS_EXTSTAT_OTHER) && incomeListComponents.Count == 0)
            {
                templateApi.ModifyRows(new Template.Page.Table.Row() { name = $"{tableName}_{incomelistName}" }, pageName, tableName, TemplateApi.ModifyMode.Delete);
                return;
            }


            incomeListComponents.Reverse();
            // add rows for each component-variable
            foreach (var ivs in incomeListComponents)
            {
                if (string.IsNullOrEmpty(ivs.varName)) continue;
                string userVarName = Settings.MakeId(), rowName = Settings.MakeId(), cellVarName = Settings.MakeId(), cellSourceName = Settings.MakeId(),
                cellCommentsName = Settings.MakeId();

                // create optional variable (default NaN) that reads the component from file
                if (!templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable()
                { name = ivs.varName, readVar = ivs.varName, defaultValue = double.NaN })) continue;

                // create a user variable that takes the incomelist-factor into account, in fact only if it is n/a
                // the user variable allows having different values for the reforms, as user variables can be set per reform,
                // in our case we need 1 and NaN to be able to (system-specificly) "ignore" components which are set to n/a
                // implementing a user variable requires two steps:
                // 1. step: (usually done in the template): defining the user variable (in our case name and type)
                // 2. step: (usually done by the user or a programme): assigning values, if the latter and appropriate, per package and reform (in our case per reform)
                if (!templateApi.ModifyUserVariables(new Template.TemplateInfo.UserVariable() // 1. step: defining the user variable
                { name = userVarName, inputType = HardDefinitions.UserInputType.Numeric })) continue;

                // Add the actual row
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowName, title = $"{SubstractToText(ivs)} {ivs.description} ({ivs.varName})" },
                    pageName, tableName,
                    TemplateApi.ModifyMode.AddNew, TemplateApi.AddWhere.After, $"{tableName}_{incomelistName}")) System.Windows.Forms.MessageBox.Show("Error2");

                string formulaStringValue = string.Empty;

                string incomeListExt = incomelistName;
                if (incomelistName.Equals(InDepthDefinitions.OtherOriginalIncome) || incomelistName.Equals(InDepthDefinitions.ILS_EARNS)) //variable which are in ils_origy and not in ils_earns
                {
                    incomeListExt = InDepthDefinitions.ILS_ORIGY;
                }
                //Source column
                if (source.ContainsKey(incomeListExt + "_" + ivs.varName)) { formulaStringValue = source[incomeListExt + "_" + ivs.varName]; }
                if (String.IsNullOrEmpty(formulaStringValue)) { formulaStringValue = "-"; }
                // add the cell for the reform-columns, as we require a row- and column-specific formula
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellSourceName, rowName = rowName, colName = colName_Source },
                    pageName, tableName, TemplateApi.ModifyMode.AddNew, false)) continue;
                if (!templateApi.ModifyCellAction_Cell(new Template.Action()
                {
                    calculationType = HardDefinitions.CalculationType.Info,
                    formulaString = formulaStringValue
                },
                    pageName, tableName, cellSourceName, TemplateApi.ModifyMode.AddNew, false)) continue;

                //Comments column
                formulaStringValue = string.Empty;

                if (comments.ContainsKey(incomeListExt + "_" + ivs.varName)) { formulaStringValue = comments[incomeListExt + "_" + ivs.varName]; }
                if (String.IsNullOrEmpty(formulaStringValue)) { formulaStringValue = "-"; }
                // add the cell for the reform-columns, as we require a row- and column-specific formula
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellCommentsName, rowName = rowName, colName = colName_Comments },
                    pageName, tableName, TemplateApi.ModifyMode.AddNew, false)) continue;
                if (!templateApi.ModifyCellAction_Cell(new Template.Action()
                { // add the formula for the reforms
                    calculationType = HardDefinitions.CalculationType.Info,
                    formulaString = formulaStringValue
                },
                    pageName, tableName, cellCommentsName, TemplateApi.ModifyMode.AddNew, false)) continue;
            }
        }

        private void ModifyTemplate_Distributional(TemplateApi templateApi, string pageName, string tableName,
                                                       List<int> systemYears, List<string> systemNames, Dictionary<string, Dictionary<string, string>> distValues, 
                                                       out List<Template.TemplateInfo.UserVariable> sysSpecILFactors)
        {
            sysSpecILFactors = new List<Template.TemplateInfo.UserVariable>();

            // Then prepare the columns 
            foreach (string colName in baseCols)
            {
                int colNum = (colName == colName_External || colName == colName_SILCValue) ? systemYears.Count : systemNames.Count;
                // if there is a known base column, copy it for all files, and make sure that only the first has a separator
                Template.Page.Table.Column column = templateApi.GetColumn(pageName, tableName, colName);
                if (column != null)
                {
                    for (int i = 0; i < colNum; i++)
                    {
                        string ys = (colName == colName_External || colName == colName_SILCValue) ? systemYears[i].ToString() : systemNames[i];
                        if (!templateApi.CopyColumn(pageName, tableName, colName, colName + ys, false, TemplateApi.AddWhere.Before))
                            System.Windows.Forms.MessageBox.Show("Cannot copy column '" + (colName + ys) + "' in template");
                        else
                        {
                            Template.Page.Table.Column col = templateApi.GetColumn(pageName, tableName, colName + ys);
                            col.hasSeparatorBefore = i == 0;
                            if (col.cellAction != null && col.cellAction.formulaString != null)
                                col.cellAction.formulaString = fixFormulaString(col.cellAction.formulaString, ys);
                        }
                    }
                    // finally, delete the template column
                    templateApi.ModifyColumns(column, pageName, tableName, TemplateApi.ModifyMode.Delete);
                }

                // if there are base custom cells for this column, also multiply those and delete the base ones
                List<Template.Page.Table.Cell> customCells = templateApi.GetCells(pageName, tableName, colName);
                if (customCells.Count > 0)
                {
                    foreach (Template.Page.Table.Cell cell in customCells)
                    {
                        if (string.IsNullOrEmpty(cell.name))
                            cell.name = Settings.MakeId();

                        for (int i = 0; i < colNum; i++)
                        {
                            string ys = (colName == colName_External || colName == colName_SILCValue) ? systemYears[i].ToString() : systemNames[i];
                            string newCellName = Settings.MakeId();
                            templateApi.CopyCell(pageName, tableName, cell, newCellName);
                            Template.Page.Table.Cell newCell = templateApi.GetCell(pageName, tableName, newCellName);
                            newCell.colName = newCell.colName + ys;
                            newCell.cellAction.formulaString = fixFormulaString(newCell.cellAction.formulaString, ys);
                        }
                        templateApi.ModifyCells(cell, pageName, tableName, TemplateApi.ModifyMode.Delete);
                    }
                }
            }


            // Then fix the column names
            for (int i = 0; i < systemNames.Count; i++)
            {
                templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_Baseline + systemNames[i], title = "Baseline values<br>" + systemNames[i], multiNo = i.ToString() }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
                templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_PercentageDifference + systemNames[i], title = "Base/Ext ratio<br>" + systemNames[i] }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
            }
            for (int j = 0; j < systemYears.Count; j++)
            {
                templateApi.ModifyColumns(new Template.Page.Table.Column() { name = colName_External + systemYears[j], title = "External values<br>" + systemYears[j] }, pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
            }

            // Then add the rows for each component-variable
            foreach (string rowName in distValues.Keys)
            {
                Dictionary<string, string> rowVal = distValues[rowName];
                // Create the row and cell IDs, so that we can reference them 
                string[] cellNameBaseline = new string[systemYears.Count], cellNameExternal = new string[systemYears.Count];
                for (int i = 0; i < systemYears.Count; i++)
                {
                        cellNameBaseline[i] = Settings.MakeId();
                        cellNameExternal[i] = Settings.MakeId();
                }

                for (int i = 0; i < systemYears.Count; i++)
                {
                    // Add the External values
                    if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameExternal[i], rowName = rowName, colName = colName_External + systemYears[i] },
                        pageName, tableName, TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error3");
                    if (!templateApi.ModifyCellAction_Cell(new Template.Action { formulaString = $"{Settings.USR_VAR(rowName + "_" + systemYears[i])}" },
                        pageName, tableName, cellNameExternal[i], TemplateApi.ModifyMode.AddOrReplace)) System.Windows.Forms.MessageBox.Show("Error4");
                    // Create the User Variables for the External values
                    if (!templateApi.ModifyUserVariables(new Template.TemplateInfo.UserVariable() { name = rowName + "_" + systemYears[i], inputType = HardDefinitions.UserInputType.Numeric }))
                        System.Windows.Forms.MessageBox.Show("Error5");
                    // Make sure the system exists, otherwise cancel and ask user to open and save the External Statistics form and save
                    // (to auto-update any missing systems e.g. in case someone added more systems to the country and never updated the external statistics)
                    sysSpecILFactors.Add(new Template.TemplateInfo.UserVariable()
                    {
                        name = rowName + "_" + systemYears[i],
                        value = ((rowVal.TryGetValue(systemYears[i].ToString(), out string dStr) && double.TryParse(dStr, NumberStyles.Number, CultureInfo.InvariantCulture, out double d)) ? d : double.NaN).ToString(CultureInfo.InvariantCulture)    // if value does not exist, fill with NaN
                    });
                }
            }
        }

        private void ModifyTemplate_Metadata(TemplateApi templateApi, string pageName, string tableName, List<int> systemYears)
        {
            for (int i = 0; i < systemYears.Count; i++)
            {
                if (!templateApi.CopyColumn(pageName, tableName, "BaseSys", "BaseSys" + i, false, TemplateApi.AddWhere.Before))
                    System.Windows.Forms.MessageBox.Show("Cannot copy column '" + ("BaseSys" + i) + "' in template");
                else
                {
                    Template.Page.Table.Column col = templateApi.GetColumn(pageName, tableName, "BaseSys" + i);
                    col.multiNo = i.ToString();

                }
            }
            templateApi.ModifyColumns(new Template.Page.Table.Column() { name = "BaseSys" }, pageName, tableName, TemplateApi.ModifyMode.Delete);
        }

        string SubstractToNum(bool? sub, bool naToZero) { return sub == null ? (naToZero ? "0.0" : double.NaN.ToString()) : (sub == true ? "-1.0" : "1.0"); }

        private string SubstractToText(ILVarSummary ivs)
        {
            bool? joint = ivs.baselineSubstract;
            foreach (bool? r in ivs.reformsSubstract)
            {
                if (r == null || r == joint) continue;
                if (joint == null) joint = r; else return "~"; // a variable is partly added, partly substracted - that is actually non-sense
            }
            return joint == true ? "-" : "+";
        }

        public class ILVarSummary
        {
            public string varName = string.Empty;
            public string description = string.Empty;
            public string destination = string.Empty;
            public bool? baselineSubstract = null;
            public List<bool?> reformsSubstract = new List<bool?>();
            public List<bool> exists = new List<bool>();
        }

        private List<ILVarSummary> GetAllSysILCompositionMacrovalidation(string ilName, Dictionary<string, string> destination) //Dictionary<string, ExternalStatisticAggregate> allVarInfo
        {
            BaselineReformPackage brp = settings.baselineReformPackages[0]; // this function is only called if there is exactly one baseline-reform-package
            List<ILVarSummary> allSysILComposition = new List<ILVarSummary>();
            // we only check the reform packages, because baseline is also passed as the first reform
            int refNo = 0;
            foreach (BaselineReformPackage.BaselineOrReform reform in brp.reforms)
            {
                if (reform.systemInfo != null && reform.systemInfo.fiscalIls.ContainsKey(ilName))
                {
                    foreach (var vi in reform.systemInfo.GetIncomelistContent(ilName))
                    {
                        ILVarSummary ivs = (from a in allSysILComposition where a.varName.ToLower() == vi.Key.ToLower() select a).FirstOrDefault();
                        if (ivs == null)
                        {
                            string d = destination.FirstOrDefault(x => x.Key.StartsWith((ilName.Equals(InDepthDefinitions.ILS_EARNS) ? InDepthDefinitions.ILS_ORIGY : ilName) + "_" + vi.Key)).Value?? InDepthDefinitions.IncomelistDefaultDestination[ilName];
                            ivs = new ILVarSummary() { varName = vi.Key, description = vi.Value.description, baselineSubstract = null, destination = d };
                            for (int r = 0; r < brp.reforms.Count; ++r) ivs.reformsSubstract.Add(null);
                            allSysILComposition.Add(ivs);
                        }
                        
                        ivs.reformsSubstract[brp.reforms.IndexOf(reform)] = vi.Value.substract;
                        ivs.exists[refNo] = true;
                    }
                }
                refNo++;
            }
            return allSysILComposition;
        }

    }

}