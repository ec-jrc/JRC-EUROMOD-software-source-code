using EM_Common;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace EM_Statistics.InDepthAnalysis
{
    internal partial class SettingsDistributional : Form, ISettings
    {
        internal const string pageName_Distributional = "Distributional";
        internal const string tableName_DistributionalTaxpayersBeneficiaries = "DistributionalTaxpayersBeneficiaries";
        internal const string tableName_TotalTaxOrBenefit = "TotalTaxOrBenefit";
        internal const string tableName_MeanTaxOrBenefit = "MeanTaxOrBenefit";
        internal const string tableName_AverageTaxBurden = "AverageTaxBurden";
        internal const string tableName_MeanDisposableIncome = "MeanDisposableIncome";
        internal const string tableName_MeanEqDispIncome = "MeanEqDispIncome";
        internal const string tableName_Winners = "Winners";
        internal const string tableName_Losers = "Losers";

        private const string varName_HHMembersCount = "HHMembersCount";
        private const string varName_eq_ils_dispy = "eq_ils_dispy";
        private const string varName_OECDScale = "OECDScale";
        private const string actionName_table21_AllUnits = "AllUnits";
        private const string actionName_table21_ConcernedUnits = "ConcernedUnits";
        private const string actionName_table21_AnalysisObject = "AnalysisObject";
        private const string actionName_table22_AnalysisObject = "AnalysisObject";
        private const string actionName_table23_AnalysisObject = "AnalysisObject";
        private const string actionName_table24_Tax = "Tax";
        private const string actionName_table24_TaxBase = "TaxBase";
        private const string actionName_table278_ConcernedUnits = "ConcernedUnits";
        private const string actionName_table278_Sensitivity = "Sensitivity";
        private const string parName_table278_eq_ils_dispy_base = "eq_ils_dispy_base";

        internal class UserSettings
        {
            internal const string XMLTAG_PAGE_TITLE_DISTRIBUTIONAL = "PageTitle_Distributional";
            internal const string XMLTAG_TABLE_TITLE_DISTRIBUTIONAL_TAX_PAYERS_BENEFICIARIES = "TableTitle_DistributionalTaxpayersBeneficiaries";
            internal const string XMLTAG_TABLE_TITLE_TOTAL_TAX_OR_BENEFIT = "TableTitle_TotalTaxOrBenefit";
            internal const string XMLTAG_TABLE_TITLE_MEAN_TAX_OR_BENEFIT = "TableTitle_MeanTaxOrBenefit";
            internal const string XMLTAG_TABLE_TITLE_AVERAGE_TAX_BURDEN = "TableTitle_AverageTaxBurden";
            internal const string XMLTAG_TABLE_TITLE_MEAN_DISPOSABLE_INCOME = "TableTitle_MeanDisposableIncome";
            internal const string XMLTAG_TABLE_TITLE_MEAN_EQ_DISP_INCOME = "TableTitle_MeanEqDispIncome";
            internal const string XMLTAG_TABLE_TITLE_WINNERS = "TableTitle_Winners";
            internal const string XMLTAG_TABLE_TITLE_LOSERS = "TableTitle_Losers";

            internal const string XMLTAG_BREAK_DOWN = "BreakDown";
            internal const string XMLTAG_ANALYSIS_OBJECT = "AnalysisObject";
            internal const string XMLTAG_TABLE21_ANALYSIS_OBJECTS = "Table21_AnalysisObjects";
            internal const string XMLTAG_TABLE21_CALCULATION_LEVEL = "Table21_CalculationLevel";
            internal const string XMLTAG_TABLE21_TARGET_POPULATION = "Table21_TargetPopulation";
            internal const string XMLTAG_TABLE21_DISTRIBUTIONAL_TAXPAYERS_BENEFICIARIES = "Table21_DistributionalTaxpayersBeneficiaries";
            internal const string XMLTAG_TABLE22_ANALYSIS_OBJECTS = "Table22_AnalysisObjects";
            internal const string XMLTAG_TABLE22_CALCULATION_LEVEL = "Table22_CalculationLevel";
            internal const string XMLTAG_TABLE22_TARGET_POPULATION = "Table22_TargetPopulation";
            internal const string XMLTAG_TABLE23_ANALYSIS_OBJECTS = "Table23_AnalysisObjects";
            internal const string XMLTAG_TABLE23_CALCULATION_LEVEL = "Table23_CalculationLevel";
            internal const string XMLTAG_TABLE23_TARGET_POPULATION = "Table23_TargetPopulation";
            internal const string XMLTAG_TABLE24_TAX = "Table24_Tax";
            internal const string XMLTAG_TABLE24_TAX_BASE = "Table24_TaxBase";
            internal const string XMLTAG_TABLE24_CALCULATION_LEVEL = "Table24_CalculationLevel";
            internal const string XMLTAG_TABLE24_TARGET_POPULATION = "Table24_TargetPopulation";
            internal const string XMLTAG_TABLE25_CALCULATION_LEVEL = "Table25_CalculationLevel";
            internal const string XMLTAG_TABLE25_TARGET_POPULATION = "Table25_TargetPopulation";
            internal const string XMLTAG_TABLE26_TARGET_POPULATION = "Table26_TargetPopulation";
            internal const string XMLTAG_TABLE278_SENSITIVITY = "Table278_Sensitivity";
            internal const string XMLTAG_TABLE278_CALCULATION_LEVEL = "Table278_CalculationLevel";
            internal const string XMLTAG_TABLE278_TARGET_POPULATION = "Table278_TargetPopulation";

            internal string pageTitle_Distributional = "2. Distributional";
            internal string tableTitle_DistributionalTaxpayersBeneficiaries = "2.1. Total number and share of taxpayers/beneficiaries";
            internal string tableTitle_TotalTaxOrBenefit = "2.2. Total tax/benefit (annual)";
            internal string tableTitle_MeanTaxOrBenefit = "2.3. Mean tax/benefit (annual)";
            internal string tableTitle_AverageTaxBurden = "2.4. Average tax burden (%)";
            internal string tableTitle_MeanDisposableIncome = "2.5. Mean disposable income (annual)";
            internal string tableTitle_MeanEqDispIncome = "2.6. Mean eq. disp. income (annual)";
            internal string tableTitle_Winners = "2.7. Winners";
            internal string tableTitle_Losers = "2.8. Losers";


            internal List<Breakdowns.TypeBreakDown> breakDowns = new List<Breakdowns.TypeBreakDown>() {
                new Breakdowns.TypeBreakDown(Breakdowns.STD_DISPY_DECILES),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_HH_CAT),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_LABOUR_CAT),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_GENDER_CAT),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_AGE_CAT)
            };

            internal List<string> table21_AnalysisObjects = new List<string>() { InDepthDefinitions.ILS_TAX };
            internal string table21_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table21_TargetPopulation = string.Empty;
            internal string table21_DistributionalTaxpayersBeneficiaries = "!=0";
            internal List<string> table22_AnalysisObjects = new List<string>() { InDepthDefinitions.ILS_TAX };
            internal string table22_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table22_TargetPopulation = string.Empty;
            internal List<string> table23_AnalysisObjects = new List<string>() { InDepthDefinitions.ILS_TAX };
            internal string table23_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table23_TargetPopulation = string.Empty;
            internal string table24_Tax = $"{Settings.DATA_VAR(InDepthDefinitions.ILS_TAX)} + {Settings.DATA_VAR(InDepthDefinitions.ILS_SICDY)}";
            internal string table24_TaxBase = $"{Settings.DATA_VAR(InDepthDefinitions.ILS_ORIGY)} + {Settings.DATA_VAR(InDepthDefinitions.ILS_BEN)}";
            internal string table24_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table24_TargetPopulation = string.Empty;
            internal string table25_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table25_TargetPopulation = string.Empty;
            internal string table26_TargetPopulation = string.Empty;
            internal string table278_Sensitivity = "0.0";
            internal string table278_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table278_TargetPopulation = string.Empty;

            internal void FromXml(XElement xElement, out string warnings)
            {
                warnings = string.Empty;
                foreach (XElement xe in xElement.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (Settings.GetXEleName(xe))
                    {
                        case XMLTAG_PAGE_TITLE_DISTRIBUTIONAL: pageTitle_Distributional = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_DISTRIBUTIONAL_TAX_PAYERS_BENEFICIARIES: tableTitle_DistributionalTaxpayersBeneficiaries = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_TOTAL_TAX_OR_BENEFIT: tableTitle_TotalTaxOrBenefit = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_MEAN_TAX_OR_BENEFIT: tableTitle_MeanTaxOrBenefit = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_AVERAGE_TAX_BURDEN: tableTitle_AverageTaxBurden = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_MEAN_DISPOSABLE_INCOME: tableTitle_MeanDisposableIncome = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_MEAN_EQ_DISP_INCOME: tableTitle_MeanEqDispIncome = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_WINNERS: tableTitle_Winners = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_LOSERS: tableTitle_Losers = xe.Value; break;
                        case XMLTAG_BREAK_DOWN + "s":
                            breakDowns.Clear();
                            foreach (XElement xebd in xe.Elements()) { breakDowns.Add(Breakdowns.TypeBreakDown.FromXml(xebd, out string wbd)); warnings += wbd; }
                            break;
                        case XMLTAG_TABLE21_ANALYSIS_OBJECTS: table21_AnalysisObjects = AnalysisObjectsFromXml(); break;
                        case XMLTAG_TABLE21_CALCULATION_LEVEL: table21_CalculationLevel = xe.Value; break;
                        case XMLTAG_TABLE21_TARGET_POPULATION: table21_TargetPopulation = xe.Value; break;
                        case XMLTAG_TABLE21_DISTRIBUTIONAL_TAXPAYERS_BENEFICIARIES: table21_DistributionalTaxpayersBeneficiaries = xe.Value; break;
                        case XMLTAG_TABLE22_ANALYSIS_OBJECTS: table22_AnalysisObjects = AnalysisObjectsFromXml(); break;
                        case XMLTAG_TABLE22_CALCULATION_LEVEL: table22_CalculationLevel = xe.Value; break;
                        case XMLTAG_TABLE22_TARGET_POPULATION: table22_TargetPopulation = xe.Value; break;
                        case XMLTAG_TABLE23_ANALYSIS_OBJECTS: table23_AnalysisObjects = AnalysisObjectsFromXml(); break;
                        case XMLTAG_TABLE23_CALCULATION_LEVEL: table23_CalculationLevel = xe.Value; break;
                        case XMLTAG_TABLE23_TARGET_POPULATION: table23_TargetPopulation = xe.Value; break;
                        case XMLTAG_TABLE24_TAX: table24_Tax = xe.Value; break;
                        case XMLTAG_TABLE24_TAX_BASE: table24_TaxBase = xe.Value; break;
                        case XMLTAG_TABLE24_CALCULATION_LEVEL: table24_CalculationLevel = xe.Value; break;
                        case XMLTAG_TABLE24_TARGET_POPULATION: table24_TargetPopulation = xe.Value; break;
                        case XMLTAG_TABLE25_CALCULATION_LEVEL: table25_CalculationLevel = xe.Value; break;
                        case XMLTAG_TABLE25_TARGET_POPULATION: table25_TargetPopulation = xe.Value; break;
                        case XMLTAG_TABLE26_TARGET_POPULATION: table26_TargetPopulation = xe.Value; break;
                        case XMLTAG_TABLE278_SENSITIVITY: table278_Sensitivity = xe.Value; break;
                        case XMLTAG_TABLE278_CALCULATION_LEVEL: table278_CalculationLevel = xe.Value; break;
                        case XMLTAG_TABLE278_TARGET_POPULATION: table278_TargetPopulation = xe.Value; break;

                        default: warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;

                        List<string> AnalysisObjectsFromXml()
                        {
                            List<string> analysisObjects = new List<string>();
                            foreach (XElement xeSub in xe.Elements())
                                if (xeSub.Value != null) analysisObjects.Add(xeSub.Value);
                            return analysisObjects;
                        }
                    }
                }
            }

            internal void ToXml(XmlWriter xmlWriter)
            {
                Settings.WriteElement(xmlWriter, XMLTAG_PAGE_TITLE_DISTRIBUTIONAL, pageTitle_Distributional);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_DISTRIBUTIONAL_TAX_PAYERS_BENEFICIARIES, tableTitle_DistributionalTaxpayersBeneficiaries);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_TOTAL_TAX_OR_BENEFIT, tableTitle_TotalTaxOrBenefit);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_MEAN_TAX_OR_BENEFIT, tableTitle_MeanTaxOrBenefit);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_AVERAGE_TAX_BURDEN, tableTitle_AverageTaxBurden);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_MEAN_DISPOSABLE_INCOME, tableTitle_MeanDisposableIncome);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_MEAN_EQ_DISP_INCOME, tableTitle_MeanEqDispIncome);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_WINNERS, tableTitle_Winners);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_LOSERS, tableTitle_Losers);
                xmlWriter.WriteStartElement(XMLTAG_BREAK_DOWN + "s"); foreach (Breakdowns.TypeBreakDown bd in breakDowns) bd.ToXml(xmlWriter, XMLTAG_BREAK_DOWN); xmlWriter.WriteEndElement();
                AnalysisObjectsToXml(XMLTAG_TABLE21_ANALYSIS_OBJECTS, table21_AnalysisObjects);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE21_CALCULATION_LEVEL, table21_CalculationLevel);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE21_TARGET_POPULATION, table21_TargetPopulation);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE21_DISTRIBUTIONAL_TAXPAYERS_BENEFICIARIES, table21_DistributionalTaxpayersBeneficiaries);
                AnalysisObjectsToXml(XMLTAG_TABLE22_ANALYSIS_OBJECTS, table22_AnalysisObjects);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE22_CALCULATION_LEVEL, table22_CalculationLevel);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE22_TARGET_POPULATION, table22_TargetPopulation);
                AnalysisObjectsToXml(XMLTAG_TABLE23_ANALYSIS_OBJECTS, table23_AnalysisObjects);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE23_CALCULATION_LEVEL, table23_CalculationLevel);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE23_TARGET_POPULATION, table23_TargetPopulation);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE24_TAX, table24_Tax);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE24_TAX_BASE, table24_TaxBase);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE24_CALCULATION_LEVEL, table24_CalculationLevel);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE24_TARGET_POPULATION, table24_TargetPopulation);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE25_CALCULATION_LEVEL, table25_CalculationLevel);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE25_TARGET_POPULATION, table25_TargetPopulation);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE26_TARGET_POPULATION, table26_TargetPopulation);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE278_SENSITIVITY, table278_Sensitivity);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE278_CALCULATION_LEVEL, table278_CalculationLevel);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE278_TARGET_POPULATION, table278_TargetPopulation);

                void AnalysisObjectsToXml(string mainTag, List<string> analysisObjects)
                {
                    xmlWriter.WriteStartElement(mainTag);
                    foreach (string ao in analysisObjects) Settings.WriteElement(xmlWriter, XMLTAG_ANALYSIS_OBJECT, ao);
                    xmlWriter.WriteEndElement();
                }
            }

            internal void WriteMetadata(TemplateApi templateApi, List<string> inactiveTablesAndPages)
            {
                Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, "Breakdown");
                foreach (Breakdowns.TypeBreakDown bd in breakDowns) Settings.AddMetaDataRow(templateApi, pageName_Distributional, bd.GetByText(), string.Empty);

                if (!inactiveTablesAndPages.Contains(tableName_DistributionalTaxpayersBeneficiaries))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_DistributionalTaxpayersBeneficiaries);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Variables/Formulas", string.Join("<br>", table21_AnalysisObjects));
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", table21_CalculationLevel);
                    if (!string.IsNullOrEmpty(table21_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table21_TargetPopulation);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Taxpayers/Beneficiaries", table21_DistributionalTaxpayersBeneficiaries);
                }
                if (!inactiveTablesAndPages.Contains(tableName_TotalTaxOrBenefit))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_TotalTaxOrBenefit);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Variables/Formulas", string.Join("<br>", table22_AnalysisObjects));
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", table22_CalculationLevel);
                    if (!string.IsNullOrEmpty(table22_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table22_TargetPopulation);
                }
                if (!inactiveTablesAndPages.Contains(tableName_MeanTaxOrBenefit))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_MeanTaxOrBenefit);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Variables/Formulas", string.Join("<br>", table23_AnalysisObjects));
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", table23_CalculationLevel);
                    if (!string.IsNullOrEmpty(table23_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table23_TargetPopulation);
                }
                if (!inactiveTablesAndPages.Contains(tableName_AverageTaxBurden))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_AverageTaxBurden);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Tax liability", table24_Tax);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Tax base", table24_TaxBase);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", table24_CalculationLevel);
                    if (!string.IsNullOrEmpty(table24_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table24_TargetPopulation);
                }
                if (!inactiveTablesAndPages.Contains(tableName_MeanDisposableIncome))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_MeanDisposableIncome);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", table25_CalculationLevel);
                    if (!string.IsNullOrEmpty(table25_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table25_TargetPopulation);
                }
                if (!inactiveTablesAndPages.Contains(tableName_MeanEqDispIncome))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_MeanEqDispIncome);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", "Individual");
                    if (!string.IsNullOrEmpty(table26_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table26_TargetPopulation);
                }
                if (!inactiveTablesAndPages.Contains(tableName_Winners) || !inactiveTablesAndPages.Contains(tableName_Losers))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Distributional, tableTitle_Winners + "<br>" + tableTitle_Losers);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Sensitivity", table278_Sensitivity);
                    Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Level of analysis", table278_CalculationLevel);
                    if (!string.IsNullOrEmpty(table278_TargetPopulation)) Settings.AddMetaDataRow(templateApi, pageName_Distributional, "Target population", table278_TargetPopulation);
                }
            }
        }

        private Settings settings = null;
        internal Dictionary<string, Settings.TextBoxInfo> checkFields = new Dictionary<string, Settings.TextBoxInfo>();
        internal Dictionary<string, Settings.DataGridViewInfo> checkGridViews = new Dictionary<string, Settings.DataGridViewInfo>();


        internal SettingsDistributional()
        {
            InitializeComponent();
            InDepthAnalysis.SetShowHelp(this, helpProvider);
        }

        void ISettings.ModifyTemplate(TemplateApi templateApi, out List<Template.TemplateInfo.UserVariable> systemSpecificVars)
        {
            systemSpecificVars = null;
            if (settings == null || settings.inactiveTablesAndPages.Contains(pageName_Distributional)) return;
            ModifyTemplate_Titles(templateApi);

            List<Breakdowns.TypeBreakDown> breakDowns = settings.userSettingsDistributional.breakDowns;

            List<Template.Page> pages = new List<Template.Page>(); // first make all necessary copies, i.e. before the original page is modified
            for (int bdNo = 0; bdNo < breakDowns.Count; ++bdNo)
                if (bdNo > 0) templateApi.CopyPage(pageName_Distributional, GetPageName(bdNo),
                    TemplateApi.AddWhere.After, bdNo == 1 ? pageName_Distributional : GetPageName(bdNo-1));
            for (int bdNo = 0; bdNo < breakDowns.Count; ++bdNo)
            {
                ModifyTemplate_BreakDown(templateApi, GetPageName(bdNo), breakDowns[bdNo]);
                ModifyTemplate_Table21(templateApi, GetPageName(bdNo));
                ModifyTemplate_Table22(templateApi, GetPageName(bdNo));
                ModifyTemplate_Table23(templateApi, GetPageName(bdNo));
                ModifyTemplate_Table24(templateApi, GetPageName(bdNo));
                ModifyTemplate_Table25(templateApi, GetPageName(bdNo));
                ModifyTemplate_Table26(templateApi, GetPageName(bdNo));
                ModifyTemplate_Table278(templateApi, GetPageName(bdNo));
            }

            string GetPageName(int bdNo) { return $"{pageName_Distributional}{(bdNo == 0 ? string.Empty : $"{bdNo}")}"; }
        }

        private void ModifyTemplate_Titles(TemplateApi templateApi)
        {
            templateApi.ModifyPage(new Template.Page() { name = pageName_Distributional, title = settings.userSettingsDistributional.pageTitle_Distributional });
            if (!settings.inactiveTablesAndPages.Contains(tableName_DistributionalTaxpayersBeneficiaries)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_DistributionalTaxpayersBeneficiaries, title = settings.userSettingsDistributional.tableTitle_DistributionalTaxpayersBeneficiaries }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_TotalTaxOrBenefit)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_TotalTaxOrBenefit, title = settings.userSettingsDistributional.tableTitle_TotalTaxOrBenefit }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_MeanTaxOrBenefit)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_MeanTaxOrBenefit, title = settings.userSettingsDistributional.tableTitle_MeanTaxOrBenefit }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_AverageTaxBurden)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_AverageTaxBurden, title = settings.userSettingsDistributional.tableTitle_AverageTaxBurden }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_MeanDisposableIncome)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_MeanDisposableIncome, title = settings.userSettingsDistributional.tableTitle_MeanDisposableIncome }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_MeanEqDispIncome)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_MeanEqDispIncome, title = settings.userSettingsDistributional.tableTitle_MeanEqDispIncome }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_Winners)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_Winners, title = settings.userSettingsDistributional.tableTitle_Winners }, pageName_Distributional);
            if (!settings.inactiveTablesAndPages.Contains(tableName_Losers)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_Losers, title = settings.userSettingsDistributional.tableTitle_Losers }, pageName_Distributional);
        }

        private void ModifyTemplate_BreakDown(TemplateApi templateApi, string pageName, Breakdowns.TypeBreakDown breakDown)
        {
            string breakDownVar = Settings.MakeOutVarName($"BreakDown_{breakDown.variable}");
            if (breakDown.quantiles <= 0)
            {
                if (Settings.HandleFormulaString(templateApi, breakDown.variable,
                                                 out string formulaString, out List<Template.Parameter> parameters))
                {
                    foreach (string calculationLevel in new List<string>() { HardDefinitions.DefaultCalculationLevels.INDIVIDUAL,
                                                                             HardDefinitions.DefaultCalculationLevels.HOUSEHOLD })
                        templateApi.ModifyPageActions(new Template.Action() {
                            outputVar = breakDownVar, _calculationLevel = calculationLevel, calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                            formulaString = formulaString, parameters = parameters }, pageName);
                }
            }
            else
            {
                string varQuantBase = "";
                string varQuantBaseNotEq = Settings.MakeReadVarName(templateApi, breakDown.variable);
                templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable() { name = varQuantBaseNotEq, readVar = breakDown.variable, defaultValue = double.NaN });
                if (!breakDown.equivalised) varQuantBase = varQuantBaseNotEq;
                else
                {
                    varQuantBase = $"eq{varQuantBaseNotEq}";
                    templateApi.ModifyPageActions(new Template.Action() { calculationType = HardDefinitions.CalculationType.CreateEquivalized,
                            outputVar = varQuantBase,
                            parameters = new List<Template.Parameter>() {
                            new Template.Parameter() { variableName = varQuantBaseNotEq },
                            new Template.Parameter() { name = EM_TemplateCalculator.PAR_EQUIVALENCESCALE, variableName = varName_OECDScale, _source = Template.Parameter.Source.BASELINE } } },
                        pageName);
                }
                templateApi.ModifyPageActions(new Template.Action() {
                    outputVar = breakDownVar, calculationType = HardDefinitions.CalculationType.CreateDeciles,
                    parameters = new List<Template.Parameter>() {
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_DECNO, numericValue = breakDown.quantiles },
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_INCOME, variableName = varQuantBase },
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_GROUPING, variableName = DefVarName.IDHH } } },
                    pageName);
            }

            foreach (string tableName in new List<string>() { tableName_DistributionalTaxpayersBeneficiaries,
                tableName_TotalTaxOrBenefit, tableName_MeanTaxOrBenefit, tableName_AverageTaxBurden,
                tableName_MeanDisposableIncome, tableName_MeanEqDispIncome,
                tableName_Winners, tableName_Losers })
            {
                if (settings.inactiveTablesAndPages.Contains(tableName)) continue;

                if (Breakdowns.STD_CATS.ContainsKey(breakDown.name) && !Breakdowns.STD_CATS[breakDown.name].isValued)
                {
                    List<string> rowIdByType = new List<string>();
                    foreach (Breakdowns.BreakDownItem br in Breakdowns.STD_CATS[breakDown.name].items)
                    {
                        rowIdByType.Add(Settings.MakeId());
                        if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowIdByType.Last(), title = br.title,
                            cellAction = new Template.Action() { filter = new Template.Filter() { formulaString = br.filter }}},
                            pageName, tableName, TemplateApi.ModifyMode.AddNew, TemplateApi.AddWhere.Before, Breakdowns.rowName_breakDown)) return;
                    }
                    templateApi.ModifyRows(new Template.Page.Table.Row()
                    {
                        name = Breakdowns.rowName_breakDown
                    },
                    pageName, tableName, TemplateApi.ModifyMode.Delete);
                }
                else
                {
                    templateApi.ModifyRows(new Template.Page.Table.Row()
                    {
                        name = Breakdowns.rowName_breakDown,
                        title = breakDown.rowTitle,
                        forEachValueOf = breakDownVar,
                        forEachValueMaxCount = Settings.FOREACH_VALUE_OF_MAX_COUNT,
                        forEachValueDescriptions = breakDown.GetValueDictionaryFromDefinitions()
                    },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
                }
            }

            templateApi.ModifyPage(new Template.Page() { name = pageName,
                button = new Template.Page.VisualElement()
                {
                    title = settings.userSettingsDistributional.breakDowns.Count <= 1 ? string.Empty :
                         $"{settings.userSettingsDistributional.pageTitle_Distributional} {breakDown.GetByText(true)}"
                },
                subtitle = breakDown.GetByText() });
        }

        private void ModifyTemplate_Table21(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_DistributionalTaxpayersBeneficiaries)) return;

            List<string> analysisObjects = (from ao in settings.userSettingsDistributional.table21_AnalysisObjects select ao).ToList();
            if (!analysisObjects.Any()) analysisObjects.Add(InDepthDefinitions.ILS_TAX);

            string calculationLevel = settings.userSettingsDistributional.table21_CalculationLevel == HardDefinitions.DefaultCalculationLevels.INDIVIDUAL
                    ? HardDefinitions.DefaultCalculationLevels.INDIVIDUAL : HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;

            string unitsFormulaString =
                settings.userSettingsDistributional.table21_CalculationLevel == HardDefinitions.DefaultCalculationLevels.INDIVIDUAL ||
                settings.userSettingsDistributional.table21_CalculationLevel == HardDefinitions.DefaultCalculationLevels.HOUSEHOLD
                ? "1.0" : $"{Settings.DATA_VAR(varName_HHMembersCount)}";

            if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table21_DistributionalTaxpayersBeneficiaries,
                out string formulaTBFilter, out List<Template.Parameter> parTBFilter)) return;
            string taxpayersBeneficiariesFilter = $"{Settings.DATA_VAR(actionName_table21_AnalysisObject)}{formulaTBFilter}";

            string formulaTargetPop = null; List<Template.Parameter> parTargetPop = null;
            if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table21_TargetPopulation))
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table21_TargetPopulation,
                    out formulaTargetPop, out parTargetPop)) return;

            for (int tabNo = 0; tabNo < analysisObjects.Count; ++tabNo)
            {
                string tableName = $"{tableName_DistributionalTaxpayersBeneficiaries}{(tabNo == 0 ? string.Empty : $"{tabNo}")}";
                if (tabNo > 0) templateApi.CopyTable(pageName, tableName_DistributionalTaxpayersBeneficiaries, tableName,
                    TemplateApi.AddWhere.After, $"{tableName_DistributionalTaxpayersBeneficiaries}{(tabNo == 1 ? string.Empty : $"{tabNo-1}")}");
                ModifyTable(tableName, analysisObjects[tabNo]);
            }

            void ModifyTable(string tableName, string analysisObject)
            {
                if (!Settings.HandleFormulaString(templateApi, analysisObject, out string formulaAnaObj, out List<Template.Parameter> parAnaObj)) return;
                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table21_AnalysisObject,
                    formulaString = formulaAnaObj, parameters = parAnaObj, _calculationLevel = calculationLevel },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;

                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table21_AllUnits,
                    formulaString = unitsFormulaString, _calculationLevel = calculationLevel },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;
                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table21_ConcernedUnits,
                    formulaString = unitsFormulaString, _calculationLevel = calculationLevel },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;

                if (!templateApi.ModifyFilter_TableAction(new Template.Filter() {
                    formulaString = taxpayersBeneficiariesFilter, parameters = parTBFilter },
                    pageName, tableName, actionName_table21_ConcernedUnits)) return;

                if (!templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = calculationLevel },
                    pageName, tableName)) return;

                if (formulaTargetPop != null)
                    if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPop,
                        parameters = parTargetPop }, pageName, tableName)) return;

                templateApi.ModifyTable(new Template.Page.Table() { name = tableName, subtitle = FormulaEditor.Remove_DATA_VAR(analysisObject) }, pageName);
            }
        }

        private void ModifyTemplate_Table22(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_TotalTaxOrBenefit)) return;

            List<string> analysisObjects = (from ao in settings.userSettingsDistributional.table22_AnalysisObjects select ao).ToList();
            if (!analysisObjects.Any()) analysisObjects.Add(InDepthDefinitions.ILS_TAX);

            string formulaTargetPop = null; List<Template.Parameter> parTargetPop = null;
            if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table22_TargetPopulation))
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table22_TargetPopulation,
                    out formulaTargetPop, out parTargetPop)) return;

            for (int tabNo = 0; tabNo < analysisObjects.Count; ++tabNo)
            {
                string tableName = $"{tableName_TotalTaxOrBenefit}{(tabNo == 0 ? string.Empty : $"{tabNo}")}";
                if (tabNo > 0) templateApi.CopyTable(pageName, tableName_TotalTaxOrBenefit, tableName,
                    TemplateApi.AddWhere.After, $"{tableName_TotalTaxOrBenefit}{(tabNo == 1 ? string.Empty : $"{tabNo - 1}")}");
                ModifyTable(tableName, analysisObjects[tabNo]);
            }

            void ModifyTable(string tableName, string analysisObject)
            {
                if (!Settings.HandleFormulaString(templateApi, analysisObject, out string formulaAnaObj, out List<Template.Parameter> parAnaObj)) return;
                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table22_AnalysisObject,
                    formulaString = formulaAnaObj, parameters = parAnaObj, _calculationLevel = settings.userSettingsDistributional.table22_CalculationLevel },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;

                if (!templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = settings.userSettingsDistributional.table22_CalculationLevel },
                    pageName, tableName)) return;

                if (formulaTargetPop != null)
                    if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPop,
                        parameters = parTargetPop }, pageName, tableName)) return;

                templateApi.ModifyTable(new Template.Page.Table() { name = tableName, subtitle = FormulaEditor.Remove_DATA_VAR(analysisObject) }, pageName);
            }
        }

        private void ModifyTemplate_Table23(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_MeanTaxOrBenefit)) return;

            List<string> analysisObjects = (from ao in settings.userSettingsDistributional.table23_AnalysisObjects select ao).ToList();
            if (!analysisObjects.Any()) analysisObjects.Add(InDepthDefinitions.ILS_TAX);

            string formulaTargetPop = null; List<Template.Parameter> parTargetPop = null;
            if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table23_TargetPopulation))
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table23_TargetPopulation,
                    out formulaTargetPop, out parTargetPop)) return;

            for (int tabNo = 0; tabNo < analysisObjects.Count; ++tabNo)
            {
                string tableName = $"{tableName_MeanTaxOrBenefit}{(tabNo == 0 ? string.Empty : $"{tabNo}")}";
                if (tabNo > 0) templateApi.CopyTable(pageName, tableName_MeanTaxOrBenefit, tableName,
                    TemplateApi.AddWhere.After, $"{tableName_MeanTaxOrBenefit}{(tabNo == 1 ? string.Empty : $"{tabNo - 1}")}");
                ModifyTable(tableName, analysisObjects[tabNo]);
            }

            void ModifyTable(string tableName, string analysisObject)
            {
                if (!Settings.HandleFormulaString(templateApi, analysisObject, out string formulaAnaObj, out List<Template.Parameter> parAnaObj)) return;
                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table23_AnalysisObject,
                    formulaString = formulaAnaObj, parameters = parAnaObj, _calculationLevel = settings.userSettingsDistributional.table23_CalculationLevel },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;

                if (!templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = settings.userSettingsDistributional.table23_CalculationLevel },
                    pageName, tableName)) return;

                if (formulaTargetPop != null)
                    if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPop,
                        parameters = parTargetPop }, pageName, tableName)) return;

                templateApi.ModifyTable(new Template.Page.Table() { name = tableName, subtitle = FormulaEditor.Remove_DATA_VAR(analysisObject) }, pageName);
            }
        }

        private void ModifyTemplate_Table24(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_AverageTaxBurden)) return;

            if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table24_Tax,
                out string formulaTax, out List<Template.Parameter> parTax)) return;
            if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table24_Tax,
                formulaString = formulaTax, parameters = parTax, _calculationLevel = settings.userSettingsDistributional.table24_CalculationLevel },
                pageName, tableName_AverageTaxBurden, TemplateApi.ModifyMode.MergeReplace)) return;

            if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table24_TaxBase,
                out string formulaTaxBase, out List<Template.Parameter> parTaxBase)) return;
            if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table24_TaxBase,
                formulaString = formulaTaxBase, parameters = parTaxBase, _calculationLevel = settings.userSettingsDistributional.table24_CalculationLevel },
                pageName, tableName_AverageTaxBurden, TemplateApi.ModifyMode.MergeReplace)) return;

            if (!templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = settings.userSettingsDistributional.table24_CalculationLevel },
                pageName, tableName_AverageTaxBurden)) return;

            if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table24_TargetPopulation))
            {
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table24_TargetPopulation,
                    out string formulaTargetPopulation, out List<Template.Parameter> parameterTargetPopulation)) return;
                if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPopulation,
                    parameters = parameterTargetPopulation }, pageName, tableName_AverageTaxBurden)) return;
            }
            templateApi.ModifyTable(new Template.Page.Table() { name = tableName_AverageTaxBurden,
                subtitle = $"({FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table24_Tax)}) / " +
                           $"({FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table24_TaxBase)})" }, pageName);
        }

        private void ModifyTemplate_Table25(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_MeanDisposableIncome)) return;

            if (!templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = settings.userSettingsDistributional.table25_CalculationLevel },
                pageName, tableName_MeanDisposableIncome)) return;

            if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table25_TargetPopulation))
            {
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table25_TargetPopulation,
                    out string formulaTargetPopulation, out List<Template.Parameter> parameterTargetPopulation)) return;
                if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPopulation,
                    parameters = parameterTargetPopulation }, pageName, tableName_MeanDisposableIncome)) return;
            }
        }

        private void ModifyTemplate_Table26(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_MeanEqDispIncome)) return;

            if (!templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = HardDefinitions.DefaultCalculationLevels.INDIVIDUAL },
                pageName, tableName_MeanEqDispIncome)) return;

            if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table26_TargetPopulation))
            {
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table26_TargetPopulation,
                    out string formulaTargetPopulation, out List<Template.Parameter> parameterTargetPopulation)) return;
                if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPopulation,
                    parameters = parameterTargetPopulation }, pageName, tableName_MeanEqDispIncome)) return;
            }
        }

        private void ModifyTemplate_Table278(TemplateApi templateApi, string pageName)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_Winners) && settings.inactiveTablesAndPages.Contains(tableName_Losers)) return;

            if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table278_Sensitivity,
                out string formulaSensitivity, out List<Template.Parameter> parSensitivity)) return;

            for (int wl = 1; wl <= 2; ++wl)
            {
                string tableName = wl == 1 ? tableName_Winners : tableName_Losers;

                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table278_ConcernedUnits,
                    _calculationLevel = settings.userSettingsDistributional.table278_CalculationLevel },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;

                if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_table278_Sensitivity,
                    _calculationLevel = settings.userSettingsDistributional.table278_CalculationLevel,
                    formulaString = formulaSensitivity, parameters = parSensitivity },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace)) return;

                if (!templateApi.ModifyCellAction_Table(new Template.Action() {
                    _calculationLevel = settings.userSettingsDistributional.table278_CalculationLevel },
                    pageName, tableName)) return;

                if (!string.IsNullOrEmpty(settings.userSettingsDistributional.table278_TargetPopulation))
                {
                    if (!Settings.HandleFormulaString(templateApi, settings.userSettingsDistributional.table278_TargetPopulation,
                        out string formulaTargetPopulation, out List<Template.Parameter> parameterTargetPopulation)) return;
                    if (!templateApi.ModifyCellFilter_Table(new Template.Filter() { formulaString = formulaTargetPopulation,
                        parameters = parameterTargetPopulation }, pageName, tableName)) return;
                }

                if (!settings.compareWithBaseline) templateApi.ModifyFilter_TableAction(new Template.Filter() {
                        parameters = new List<Template.Parameter>() { new Template.Parameter() { name = parName_table278_eq_ils_dispy_base,
                            variableName = varName_eq_ils_dispy, _source = Template.Parameter.Source.PREVIOUS_REFORM } } },
                        pageName, tableName, actionName_table278_ConcernedUnits, TemplateApi.ModifyMode.MergeReplace);
            }
        }

        void ISettings.UpdateSettings(Settings _settings)
        {
            settings = _settings;
        }

        void ISettings.ShowDialog()
        {
            UpdateControls();
            ShowDialog();
        }

        private void UpdateControls()
        {
            txtPageTitleDistributional.Text = settings.userSettingsDistributional.pageTitle_Distributional;
            txtTableTitleDistributionalTaxpayersBeneficiaries.Text = settings.userSettingsDistributional.tableTitle_DistributionalTaxpayersBeneficiaries;
            txtTableTitleTotalTaxOrBenefit.Text = settings.userSettingsDistributional.tableTitle_TotalTaxOrBenefit;
            txtTableTitleMeanTaxOrBenefit.Text = settings.userSettingsDistributional.tableTitle_MeanTaxOrBenefit;
            txtTableTitleAverageTaxBurden.Text = settings.userSettingsDistributional.tableTitle_AverageTaxBurden;
            txtTableTitleMeanDisposableIncome.Text = settings.userSettingsDistributional.tableTitle_MeanDisposableIncome;
            txtTableTitleMeanEqDispIncome.Text = settings.userSettingsDistributional.tableTitle_MeanEqDispIncome;
            txtTableTitleWinners.Text = settings.userSettingsDistributional.tableTitle_Winners;
            txtTableTitleLosers.Text = settings.userSettingsDistributional.tableTitle_Losers;
            gridBreakDown.Rows.Clear();
            foreach (Breakdowns.TypeBreakDown tbd in settings.userSettingsDistributional.breakDowns)
            {
                int r = gridBreakDown.Rows.Add(tbd.rowTitle.EndsWith(Breakdowns.VALUE_PLACEHOLDER) ? tbd.rowTitle.Substring(0, tbd.rowTitle.Length - Breakdowns.VALUE_PLACEHOLDER.Length).TrimEnd() : tbd.rowTitle, 
                    tbd.name, tbd.equivalised, tbd.quantiles <= 0 ? DefPar.Value.NA : tbd.quantiles.ToString(), tbd.GetValueStringFromDefinitions());
                gridBreakDown.Rows[r].Tag = tbd.GetValueListFromDefinitions();
            }
            grid21AnalysisObjects.Rows.Clear(); foreach (string ao in settings.userSettingsDistributional.table21_AnalysisObjects) grid21AnalysisObjects.Rows.Add(FormulaEditor.Remove_DATA_VAR(ao));
            combo21Level.Text = settings.userSettingsDistributional.table21_CalculationLevel;
            txt21TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table21_TargetPopulation);
            txt21DistributionalTaxpayersBeneficiaries.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table21_DistributionalTaxpayersBeneficiaries);
            grid22AnalysisObjects.Rows.Clear(); foreach (string ao in settings.userSettingsDistributional.table22_AnalysisObjects) grid22AnalysisObjects.Rows.Add(FormulaEditor.Remove_DATA_VAR(ao));
            combo22Level.Text = settings.userSettingsDistributional.table22_CalculationLevel;
            txt22TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table22_TargetPopulation);
            grid23AnalysisObjects.Rows.Clear(); foreach (string ao in settings.userSettingsDistributional.table23_AnalysisObjects) grid23AnalysisObjects.Rows.Add(FormulaEditor.Remove_DATA_VAR(ao));
            combo23Level.Text = settings.userSettingsDistributional.table23_CalculationLevel;
            txt23TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table23_TargetPopulation);
            txt24Tax.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table24_Tax);
            txt24TaxBase.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table24_TaxBase);
            combo24Level.Text = settings.userSettingsDistributional.table24_CalculationLevel;
            txt24TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table24_TargetPopulation);
            combo25Level.Text = settings.userSettingsDistributional.table25_CalculationLevel;
            txt25TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table25_TargetPopulation);
            txt26TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table26_TargetPopulation);
            txt278Sensitivity.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table278_Sensitivity);
            combo278Level.Text = settings.userSettingsDistributional.table278_CalculationLevel;
            txt278TargetPopulation.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsDistributional.table278_TargetPopulation);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            settings.userSettingsDistributional.pageTitle_Distributional = txtPageTitleDistributional.Text;
            settings.userSettingsDistributional.tableTitle_DistributionalTaxpayersBeneficiaries = txtTableTitleDistributionalTaxpayersBeneficiaries.Text;
            settings.userSettingsDistributional.tableTitle_TotalTaxOrBenefit = txtTableTitleTotalTaxOrBenefit.Text;
            settings.userSettingsDistributional.tableTitle_MeanTaxOrBenefit = txtTableTitleMeanTaxOrBenefit.Text;
            settings.userSettingsDistributional.tableTitle_AverageTaxBurden = txtTableTitleAverageTaxBurden.Text;
            settings.userSettingsDistributional.tableTitle_MeanDisposableIncome = txtTableTitleMeanDisposableIncome.Text;
            settings.userSettingsDistributional.tableTitle_MeanEqDispIncome = txtTableTitleMeanEqDispIncome.Text;
            settings.userSettingsDistributional.tableTitle_Winners = txtTableTitleWinners.Text;
            settings.userSettingsDistributional.tableTitle_Losers = txtTableTitleLosers.Text;

            List<Breakdowns.TypeBreakDown> bds = new List<Breakdowns.TypeBreakDown>(); 
            string errors = string.Empty;
            foreach (DataGridViewRow row in gridBreakDown.Rows)
            {
                DataGridViewCell cell = row.Cells[colBreakDownVariable.Index];
                bool failed = false;
                if (string.IsNullOrEmpty(cell.FormattedValue?.ToString()))
                {
                    MessageBox.Show("Breakdown variable must not be empty.");
                    failed = true;
                }
                else if (FormulaEditor.ContainsSeparator(cell.FormattedValue.ToString()))
                {
                    MessageBox.Show($"{cell.FormattedValue}: breakdown variable does not allow for formulas.");
                    failed = true;
                }
                else if (cell.FormattedValue.ToString().Trim().EndsWith(Settings.BASELINE_MARKER))
                {
                    MessageBox.Show($"{cell.FormattedValue}: breakdown is always based on baseline variables, thus '{Settings.BASELINE_MARKER}' does not make sense.");
                    failed = true;
                }
                if (failed)
                {
                    gridBreakDown.CurrentCell = cell;
                    gridBreakDown.BeginEdit(true);
                    return;
                }

                int iRow = gridBreakDown.Rows.IndexOf(row);
                string error = string.Empty;
                string var = row.Cells[colBreakDownVariable.Index].Value?.ToString() ?? string.Empty;
                string squant = row.Cells[colBreakDownQuantiles.Index].Value?.ToString() ?? DefPar.Value.NA; int quant = -1;

                if (squant != DefPar.Value.NA && (!int.TryParse(squant, out quant) || quant <= 0))
                { errors += $"Breakdown (row {iRow + 1}): {squant} is not a valid number of quantiles" + Environment.NewLine; }
                
                Breakdowns.TypeBreakDown tb = new Breakdowns.TypeBreakDown(var);
                if (!Breakdowns.STD_CATS.ContainsKey(var))    // if this is not a default category
                {
                    List<Tuple<double, string>> values = row.Tag as List<Tuple<double, string>>;
                    foreach (var val in values) tb.items.Add(new Breakdowns.BreakDownItem() { value = val.Item1, title = val.Item2 });
                    tb.rowTitle = row.Cells[colBreakDownTitle.Index].Value?.ToString() ?? string.Empty;
                    tb.quantiles = quant;
                    tb.equivalised = Convert.ToBoolean(row.Cells[colBreakDownEquiv.Index].Value);
                }
                bds.Add(tb);
            }
            if (!bds.Any()) errors = "At least one valid breakdown definition is required.";
            if (!string.IsNullOrEmpty(errors.Trim())) { MessageBox.Show(errors.Trim()); return; }

            foreach (Settings.TextBoxInfo tbi in checkFields.Values)
            {
                bool failed = false;
                if (tbi.textbox == txt21DistributionalTaxpayersBeneficiaries && string.IsNullOrEmpty(tbi.textbox.Text)) 
                {
                    MessageBox.Show("Taxpayers/Beneficiaries must not be empty.");
                    failed = true;
                }
                else if ((tbi.textbox == txt24Tax || tbi.textbox == txt24TaxBase) && string.IsNullOrEmpty(tbi.textbox.Text))
                {
                    MessageBox.Show("Taxliability and -base must not be empty.");
                    failed = true;
                }
                else if ((tbi.textbox == txt278Sensitivity) && string.IsNullOrEmpty(tbi.textbox.Text))
                { 
                    MessageBox.Show("Sensitivity must not be empty.");
                    failed = true;
                }
                else if (tbi.startsWithComparer.HasValue ? 
                    !Settings.IsValidFormula(tbi.textbox.Text, tbi.isFilter, out errors, tbi.startsWithComparer.Value) :
                    !Settings.IsValidFormula(tbi.textbox.Text, tbi.isFilter, out errors))
                {
                    MessageBox.Show($"Invalid formula '{tbi.textbox.Text}' at TextBox '{tbi.textbox.Name}'" + Environment.NewLine + errors + Environment.NewLine + "Please fix it before saving the settings.");
                    failed = true;
                }
                if (failed)
                {
                    tbi.textbox.Focus();
                    tbi.textbox.SelectAll();
                    return;
                }
            }
            foreach (Settings.DataGridViewInfo dgvi in checkGridViews.Values)
            {
                foreach (DataGridViewRow row in dgvi.dgv.Rows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        bool failed = false;
                        if (dgvi.startsWithComparer.HasValue ?
                            !Settings.IsValidFormula(cell.Value.ToString(), dgvi.isFilter, out errors, dgvi.startsWithComparer.Value) :
                            !Settings.IsValidFormula(cell.Value.ToString(), dgvi.isFilter, out errors))
                        {
                            MessageBox.Show($"Invalid formula '{cell.Value.ToString()}' at GridView '{dgvi.dgv.Name}'" + Environment.NewLine + errors + Environment.NewLine + "Please fix it before saving the settings.");
                            failed = true;
                        }
                        if (failed)
                        {
                            dgvi.dgv.CurrentCell = cell;
                            gridBreakDown.BeginEdit(true);
                            return;
                        }
                    }
                }
            }
            settings.userSettingsDistributional.breakDowns = bds;
            settings.userSettingsDistributional.table21_AnalysisObjects = AnalysisObjectsToSettings(grid21AnalysisObjects);
            settings.userSettingsDistributional.table21_CalculationLevel = combo21Level.Text;
            settings.userSettingsDistributional.table21_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt21TargetPopulation?.Text);
            settings.userSettingsDistributional.table21_DistributionalTaxpayersBeneficiaries = FormulaEditor.Add_DATA_VAR(txt21DistributionalTaxpayersBeneficiaries?.Text);
            settings.userSettingsDistributional.table22_AnalysisObjects = AnalysisObjectsToSettings(grid22AnalysisObjects);
            settings.userSettingsDistributional.table22_CalculationLevel = combo22Level.Text;
            settings.userSettingsDistributional.table22_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt22TargetPopulation?.Text);
            settings.userSettingsDistributional.table23_AnalysisObjects = AnalysisObjectsToSettings(grid23AnalysisObjects);
            settings.userSettingsDistributional.table23_CalculationLevel = combo23Level.Text;
            settings.userSettingsDistributional.table23_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt23TargetPopulation?.Text);
            settings.userSettingsDistributional.table24_Tax = FormulaEditor.Add_DATA_VAR(txt24Tax?.Text);
            settings.userSettingsDistributional.table24_TaxBase = FormulaEditor.Add_DATA_VAR(txt24TaxBase?.Text);
            settings.userSettingsDistributional.table24_CalculationLevel = combo24Level.Text;
            settings.userSettingsDistributional.table24_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt24TargetPopulation?.Text);
            settings.userSettingsDistributional.table25_CalculationLevel = combo25Level.Text;
            settings.userSettingsDistributional.table25_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt25TargetPopulation?.Text);
            settings.userSettingsDistributional.table26_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt26TargetPopulation?.Text);
            settings.userSettingsDistributional.table278_Sensitivity = FormulaEditor.Add_DATA_VAR(txt278Sensitivity?.Text);
            settings.userSettingsDistributional.table278_CalculationLevel = combo278Level.Text;
            settings.userSettingsDistributional.table278_TargetPopulation = FormulaEditor.Add_DATA_VAR(txt278TargetPopulation?.Text);

            DialogResult = DialogResult.OK; Close();

            List<string> AnalysisObjectsToSettings(DataGridView grid)
            {
                List<string> analysisObjects = new List<string>();
                foreach (DataGridViewRow row in grid.Rows)
                    if (!string.IsNullOrEmpty(row.Cells[col21VariableFormula.Index].Value?.ToString()))
                        analysisObjects.Add(FormulaEditor.Add_DATA_VAR(row.Cells[col21VariableFormula.Index].Value.ToString()));
                return analysisObjects;
            }
        }

        DataGridView GetGrid(object senderButton)
        {
            if ((senderButton as Button).Name.Contains("21")) return grid21AnalysisObjects;
            if ((senderButton as Button).Name.Contains("22")) return grid22AnalysisObjects;
            if ((senderButton as Button).Name.Contains("23")) return grid23AnalysisObjects;
            if ((senderButton as Button).Name.Contains("BreakDown")) return gridBreakDown;
            return null;
        }

        private void btnAnaAddRow_Click(object sender, EventArgs e)
        {
            int i = GetGrid(sender).Rows.Add(string.Empty);
            GetGrid(sender).CurrentCell = GetGrid(sender).Rows[i].Cells[0]; GetGrid(sender).BeginEdit(true);
        }

        private void btnBreakDownAddRow_Click(object sender, EventArgs e)
        {
            int r = gridBreakDown.Rows.Add(string.Empty, string.Empty, false, "n/a", "");
            gridBreakDown.Rows[r].Tag = new List<Tuple<double, string>>();
            GetGrid(sender).CurrentCell = gridBreakDown.Rows[gridBreakDown.Rows.Count-1].Cells[0]; gridBreakDown.BeginEdit(true);
        }

        private void btnDelRow_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetGrid(sender);
            if (dgv.SelectedRows.Count == 0 && dgv.SelectedCells.Count == 0) return;
            DataGridViewRow selectedRow = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0] : dgv.SelectedCells[0].OwningRow;
            if (!(sender as Button).Name.Contains("BreakDown") && // for breakdown there is a check in btnOk_Click
                !string.IsNullOrEmpty(selectedRow.Cells[0].Value?.ToString())) // empty rows can be always deleted
            {
                int notEmpty = 0; foreach (DataGridViewRow row in dgv.Rows) if (!string.IsNullOrEmpty(row.Cells[0].Value?.ToString())) ++notEmpty;
                if (notEmpty <= 1) { MessageBox.Show("At least one non-empty variable/formula definition is required."); return; }
            }
            dgv.Rows.Remove(selectedRow);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetGrid(sender);
            if (dgv.SelectedRows.Count == 0 && dgv.SelectedCells.Count == 0) return;
            DataGridViewRow row = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0] : dgv.SelectedCells[0].OwningRow;
            int iSelRow = dgv.Rows.IndexOf(row); if (iSelRow == 0) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in row.Cells) rowContent.Add(cell.Value);
            dgv.Rows.Insert(iSelRow - 1, rowContent.ToArray()); dgv.Rows.RemoveAt(iSelRow + 1);
            dgv.ClearSelection(); dgv.Rows[iSelRow - 1].Cells[0].Selected = true;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetGrid(sender);
            if (dgv.SelectedRows.Count == 0 && dgv.SelectedCells.Count == 0) return;
            DataGridViewRow row = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0] : dgv.SelectedCells[0].OwningRow;
            int iSelRow = dgv.Rows.IndexOf(row); if (iSelRow == dgv.Rows.Count - 1) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in row.Cells) rowContent.Add(cell.Value);
            dgv.Rows.RemoveAt(iSelRow); dgv.Rows.Insert(iSelRow + 1, rowContent.ToArray());
            dgv.ClearSelection(); dgv.Rows[iSelRow + 1].Cells[0].Selected = true;
        }

        private void gridBreakDown_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != colBreakDownValueDescEdit.Index || e.RowIndex < 0) return;
            string v = gridBreakDown.Rows[e.RowIndex].Cells[colBreakDownVariable.Name].Value.ToString();
            CategoryDescriptionsForm form = Breakdowns.STD_CATS.ContainsKey(v) ?
                new CategoryDescriptionsForm(gridBreakDown.Rows[e.RowIndex].Tag as List<Tuple<double, string>>, true, Breakdowns.STD_CATS[v].isValued) :
                new CategoryDescriptionsForm(gridBreakDown.Rows[e.RowIndex].Tag as List<Tuple<double, string>>);
            if (form.ShowDialog() != DialogResult.OK) return;
            gridBreakDown.Rows[e.RowIndex].Tag = form.GetDescriptionsList();
            gridBreakDown.Rows[e.RowIndex].Cells[colBreakDownValueDesc.Index].Value = CategoryDescriptionsForm.DicToString(form.GetDescriptionsDictionary());
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            settings.userSettingsDistributional = new UserSettings(); // note: this "really" resets it, i.e. even if the dialog is closed with Cancel
            UpdateControls(); // if this behaviour turns out to be unwanted (i.e. users complain), one needs to only set the controls back instead
        }                     // (this is more effort and mess, as one needs to define the defaults separately and not only as initial values of the variables)

        private void txtTargetPopulation_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, true));

        }

        private void txt21DistributionalTaxpayersBeneficiaries_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, true, true));
        }

        private void txtTaxAndBase_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, false));
        }

        private void txt278Sensitivity_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, false));
        }

        private void gridAnalysisObjects_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 && // grids have only one column
                !checkGridViews.ContainsKey(dgv.Name)) checkGridViews.Add(dgv.Name, new Settings.DataGridViewInfo(dgv, false));
        }

        private void SettingsDistributional_Load(object sender, EventArgs e)
        {
            //preselect the only choice in the disabled combo
            combo26Level.SelectedIndex = 0;
            if (settings != null) checkFields.Clear();
        }

        private void gridBreakDown_CellValidated(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewCell cell = (sender as DataGridView).CurrentCell;
            if (cell.OwningColumn == colBreakDownVariable)
            {
                string v = cell.Value.ToString();
                if (Breakdowns.STD_CATS.ContainsKey(v))
                {
                    cell.OwningRow.Cells[colBreakDownValueDesc.Name].Value = Breakdowns.STD_CATS[v].GetValueStringFromDefinitions();
                    cell.OwningRow.Cells[colBreakDownEquiv.Name].Value = Breakdowns.STD_CATS[v].equivalised;
                    cell.OwningRow.Cells[colBreakDownQuantiles.Name].Value = Breakdowns.STD_CATS[v].quantiles < 0 ? DefPar.Value.NA : Breakdowns.STD_CATS[v].quantiles.ToString();
                    cell.OwningRow.Cells[colBreakDownTitle.Name].Value = Breakdowns.STD_CATS[v].rowTitle;
                    cell.OwningRow.Tag = Breakdowns.STD_CATS[v].GetValueListFromDefinitions();
                }
            }

        }
    }
}
