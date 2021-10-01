using EM_Common;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace InDepthAnalysis
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

            internal class BreakDown
            {
                internal const string XMLTAG_ROW_TITLE = "RowTitle";
                internal const string XMLTAG_QUANTILES = "Quantiles";
                internal const string XMLTAG_VARIABLE = "Variable";
                internal const string XMLTAG_EQUIVALISED = "Equivalised";
                private const string XMLTAG_VALUE_DESCRIPTION = "ValueDescription";
                private const string XMLTAG_VALUE = "Value";
                private const string XMLTAG_TEXT = "Text";

                private const string VALUE_PLACEHOLDER = "[value~Numeric]";
                internal const string rowName_breakDown = "BreakDownRow";

                internal string rowTitle = string.Empty;
                internal int quantiles = -1;
                internal string variable = string.Empty;
                internal bool equivalised = true;
                internal Dictionary<double, string> valueDescriptions = new Dictionary<double, string>();

                internal static BreakDown FromXml(XElement xElement, out string warnings)
                {
                    warnings = string.Empty; BreakDown breakDown = new BreakDown();
                    foreach (XElement xe in xElement.Elements())
                    {
                        if (xe.Value == null) continue;
                        switch (Settings.GetXEleName(xe))
                        {
                            case XMLTAG_ROW_TITLE: breakDown.rowTitle = xe.Value; break;
                            case XMLTAG_VARIABLE: breakDown.variable = xe.Value; break;
                            case XMLTAG_QUANTILES: if (int.TryParse(xe.Value, out int q)) breakDown.quantiles = q; break;
                            case XMLTAG_EQUIVALISED: if (bool.TryParse(xe.Value, out bool e)) breakDown.equivalised = e; break;
                            case XMLTAG_VALUE_DESCRIPTION + "s":
                                foreach (XElement xeVd in xe.Elements())
                                {
                                    if (xeVd.Value == null) continue; double value = 0; string text = string.Empty;
                                    foreach (XElement xeValDesc in xeVd.Elements())
                                    {
                                        switch (Settings.GetXEleName(xeValDesc))
                                        {
                                            case XMLTAG_VALUE: if (double.TryParse(xeValDesc.Value, out double d)) value = d; break;
                                            case XMLTAG_TEXT: text = xeValDesc.Value; break;
                                            default: warnings += $"Unknown setting {Settings.GetXEleName(xeValDesc)} is ignored." + Environment.NewLine; break;
                                        }
                                    }
                                    if (!breakDown.valueDescriptions.ContainsKey(value)) breakDown.valueDescriptions.Add(value, text);
                                }
                                break;
                            default: warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                        }
                    }
                    return breakDown;
                }

                internal void ToXml(XmlWriter xmlWriter)
                {
                    xmlWriter.WriteStartElement(XMLTAG_BREAK_DOWN);
                    Settings.WriteElement(xmlWriter, XMLTAG_ROW_TITLE, rowTitle);
                    Settings.WriteElement(xmlWriter, XMLTAG_QUANTILES, quantiles.ToString());
                    Settings.WriteElement(xmlWriter, XMLTAG_VARIABLE, variable);
                    Settings.WriteElement(xmlWriter, XMLTAG_EQUIVALISED, equivalised.ToString());
                    xmlWriter.WriteStartElement(XMLTAG_VALUE_DESCRIPTION + "s");
                    foreach (var vd in valueDescriptions)
                    {
                        xmlWriter.WriteStartElement(XMLTAG_VALUE_DESCRIPTION);
                        Settings.WriteElement(xmlWriter, XMLTAG_VALUE, vd.Key.ToString());
                        Settings.WriteElement(xmlWriter, XMLTAG_TEXT, vd.Value);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }

                internal void ToGrid(DataGridView grid)
                {
                    int i = grid.Rows.Add(rowTitle.EndsWith(VALUE_PLACEHOLDER) ? rowTitle.Substring(0, rowTitle.Length - VALUE_PLACEHOLDER.Length).TrimEnd() : rowTitle,
                        variable, equivalised, quantiles <= 0 ? DefPar.Value.NA : quantiles.ToString(), CategoryDescriptionsForm.DicToString(valueDescriptions));
                    grid.Rows[i].Tag = valueDescriptions;
                }

                internal static bool FromGrid(SettingsDistributional dlg, DataGridViewRow row, int iRow, out BreakDown bd, out string error)
                {
                    bd = null; error = "";
                    string var = row.Cells[dlg.colBreakDownVariable.Index].Value?.ToString() ?? string.Empty;
                    string squant = row.Cells[dlg.colBreakDownQuantiles.Index].Value?.ToString() ?? DefPar.Value.NA; int quant = -1;

                    if (string.IsNullOrEmpty(var) && squant == DefPar.Value.NA) return false;

                    if (string.IsNullOrEmpty(var)) { error = $"Breakdown (row {iRow + 1}): missing variable"; return false; }
                    if (squant != DefPar.Value.NA && (!int.TryParse(squant, out quant) || quant <= 0))
                    { error = $"Breakdown (row {iRow + 1}): {squant} is not a valid number of quantiles"; return false; }

                    string titel = row.Cells[dlg.colBreakDownTitle.Index].Value?.ToString() ?? string.Empty;
                    bd = new BreakDown() { rowTitle = titel.TrimEnd().EndsWith(VALUE_PLACEHOLDER) ? titel : titel.TrimEnd() + " " + VALUE_PLACEHOLDER,
                        variable = var,
                        quantiles = quant,
                        equivalised = Convert.ToBoolean(row.Cells[dlg.colBreakDownEquiv.Index].Value),
                        valueDescriptions = row.Tag as Dictionary<double, string> };
                    return true;
                }

                internal string GetByText(bool forButton = false)
                {
                    if (forButton)
                    {
                        string byTextShort = rowTitle.Replace(VALUE_PLACEHOLDER, string.Empty).Trim();
                        if (!string.IsNullOrEmpty(byTextShort)) return $"by {byTextShort}";
                    }
                    return IsDefault() ? "by deciles of equivalised disposable income"
                                       : $"by {(quantiles <= 0 ? "values" : $"{quantiles} quantiles")} of {variable}";
                }

                internal static BreakDown GetDefault() { return new BreakDown() { rowTitle = "Decile ", quantiles = 10, equivalised = true,
                                                                                  variable = SystemInfo.ILS_DISPY }; }
                internal bool IsDefault(bool strict = true) { return variable == GetDefault().variable && equivalised == GetDefault().equivalised && 
                                                                                 (!strict || quantiles == GetDefault().quantiles); }
            }

            internal List<BreakDown> breakDowns = new List<BreakDown> { BreakDown.GetDefault() };
            internal List<string> table21_AnalysisObjects = new List<string>() { SystemInfo.ILS_TAX };
            internal string table21_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table21_TargetPopulation = string.Empty;
            internal string table21_DistributionalTaxpayersBeneficiaries = "!=0";
            internal List<string> table22_AnalysisObjects = new List<string>() { SystemInfo.ILS_TAX };
            internal string table22_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table22_TargetPopulation = string.Empty;
            internal List<string> table23_AnalysisObjects = new List<string>() { SystemInfo.ILS_TAX };
            internal string table23_CalculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
            internal string table23_TargetPopulation = string.Empty;
            internal string table24_Tax = $"{Settings.DATA_VAR(SystemInfo.ILS_TAX)} + {Settings.DATA_VAR(SystemInfo.ILS_SICDY)}";
            internal string table24_TaxBase = $"{Settings.DATA_VAR(SystemInfo.ILS_ORIGY)} + {Settings.DATA_VAR(SystemInfo.ILS_BEN)}";
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
                            foreach (XElement xebd in xe.Elements()) { breakDowns.Add(BreakDown.FromXml(xebd, out string wbd)); warnings += wbd; }
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
                xmlWriter.WriteStartElement(XMLTAG_BREAK_DOWN + "s"); foreach (BreakDown bd in breakDowns) bd.ToXml(xmlWriter); xmlWriter.WriteEndElement();
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
                foreach (BreakDown bd in breakDowns) Settings.AddMetaDataRow(templateApi, pageName_Distributional, bd.GetByText(), string.Empty);

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

            List<UserSettings.BreakDown> breakDowns = settings.userSettingsDistributional.breakDowns;
            if (!breakDowns.Any()) breakDowns.Add(UserSettings.BreakDown.GetDefault());

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

        private void ModifyTemplate_BreakDown(TemplateApi templateApi, string pageName, UserSettings.BreakDown breakDown)
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
                string varQuantBase = varName_eq_ils_dispy;
                if (!breakDown.IsDefault(false))
                {
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
                if (!settings.inactiveTablesAndPages.Contains(tableName))
                    templateApi.ModifyRows(new Template.Page.Table.Row() {
                        name = UserSettings.BreakDown.rowName_breakDown, title = breakDown.rowTitle,
                        forEachValueOf = breakDownVar, forEachValueMaxCount = Settings.FOREACH_VALUE_OF_MAX_COUNT,
                        forEachValueDescriptions = breakDown.valueDescriptions },
                    pageName, tableName, TemplateApi.ModifyMode.MergeReplace);
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
            if (!analysisObjects.Any()) analysisObjects.Add(SystemInfo.ILS_TAX);

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
            if (!analysisObjects.Any()) analysisObjects.Add(SystemInfo.ILS_TAX);

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
            if (!analysisObjects.Any()) analysisObjects.Add(SystemInfo.ILS_TAX);

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
            gridBreakDown.Rows.Clear(); foreach (UserSettings.BreakDown bd in settings.userSettingsDistributional.breakDowns) bd.ToGrid(gridBreakDown);
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

            List<UserSettings.BreakDown> bds = new List<UserSettings.BreakDown>(); string errors = string.Empty;
            foreach (DataGridViewRow row in gridBreakDown.Rows)
            {
                if (UserSettings.BreakDown.FromGrid(this, row, gridBreakDown.Rows.IndexOf(row), out UserSettings.BreakDown bd, out string error)) bds.Add(bd);
                errors += error + Environment.NewLine;
            }
            if (!bds.Any()) errors = "At least one valid breakdown definition is required.";
            if (!string.IsNullOrEmpty(errors.Trim())) { MessageBox.Show(errors.Trim()); return; }
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
            new UserSettings.BreakDown().ToGrid(gridBreakDown);
            GetGrid(sender).CurrentCell = gridBreakDown.Rows[gridBreakDown.Rows.Count-1].Cells[0]; gridBreakDown.BeginEdit(true);
        }

        private void btnDelRow_Click(object sender, EventArgs e)
        {
            if (GetGrid(sender).SelectedRows.Count <= 0) return;
            if (!(sender as Button).Name.Contains("BreakDown") && // for breakdown there is a check in btnOk_Click
                !string.IsNullOrEmpty(GetGrid(sender).SelectedRows[0].Cells[0].Value?.ToString())) // empty rows can be always deleted
            {
                int notEmpty = 0; foreach (DataGridViewRow row in GetGrid(sender).Rows) if (!string.IsNullOrEmpty(row.Cells[0].Value?.ToString())) ++notEmpty;
                if (notEmpty <= 1) { MessageBox.Show("At least one non-empty variable/formula definition is required."); return; }
            }
            GetGrid(sender).Rows.Remove(GetGrid(sender).SelectedRows[0]);
        }

        private void btnMoveUp_Click(object sender, EventArgs e)
        {
            if (GetGrid(sender).SelectedRows.Count == 0) return;
            int iSelRow = GetGrid(sender).Rows.IndexOf(GetGrid(sender).SelectedRows[0]); if (iSelRow == 0) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in GetGrid(sender).SelectedRows[0].Cells) rowContent.Add(cell.Value);
            GetGrid(sender).Rows.Insert(iSelRow - 1, rowContent.ToArray()); GetGrid(sender).Rows.RemoveAt(iSelRow + 1);
            GetGrid(sender).ClearSelection(); GetGrid(sender).Rows[iSelRow - 1].Selected = true;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            if (GetGrid(sender).SelectedRows.Count == 0) return;
            int iSelRow = GetGrid(sender).Rows.IndexOf(GetGrid(sender).SelectedRows[0]); if (iSelRow == GetGrid(sender).Rows.Count - 1) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in GetGrid(sender).SelectedRows[0].Cells) rowContent.Add(cell.Value);
            GetGrid(sender).Rows.RemoveAt(iSelRow); GetGrid(sender).Rows.Insert(iSelRow + 1, rowContent.ToArray());
            GetGrid(sender).ClearSelection(); GetGrid(sender).Rows[iSelRow + 1].Selected = true;
        }

        private void gridBreakDown_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != colBreakDownValueDescEdit.Index || e.RowIndex < 0) return;
            CategoryDescriptionsForm form = new CategoryDescriptionsForm(gridBreakDown.Rows[e.RowIndex].Tag as Dictionary<double, string>);
            if (form.ShowDialog() != DialogResult.OK) return;
            gridBreakDown.Rows[e.RowIndex].Tag = form.GetDescriptions();
            gridBreakDown.Rows[e.RowIndex].Cells[colBreakDownValueDesc.Index].Value = CategoryDescriptionsForm.DicToString(form.GetDescriptions());
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            settings.userSettingsDistributional = new UserSettings(); // note: this "really" resets it, i.e. even if the dialog is closed with Cancel
            UpdateControls(); // if this behaviour turns out to be unwanted (i.e. users complain), one needs to only set the controls back instead
        }                     // (this is more effort and mess, as one needs to define the defaults separately and not only as initial values of the variables)

        private void txtTargetPopulation_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (sender as TextBox != null && !Settings.IsValidFormula((sender as TextBox).Text, true)) e.Cancel = true;
        }

        private void txt21DistributionalTaxpayersBeneficiaries_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty((sender as TextBox)?.Text)) { MessageBox.Show("Taxpayers/Beneficiaries must not be empty."); e.Cancel = true; return; }
            if (!Settings.IsValidFormula((sender as TextBox).Text, true, true)) e.Cancel = true;
        }

        private void txtTaxAndBase_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty((sender as TextBox)?.Text)) { MessageBox.Show("Taxliability and -base must not be empty."); e.Cancel = true; return; }
            if (!Settings.IsValidFormula((sender as TextBox).Text, false)) e.Cancel = true;
        }

        private void txt278Sensitivity_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrEmpty(txt278Sensitivity.Text)) { MessageBox.Show("Sensitivity must not be empty."); e.Cancel = true; return; }
            if (!Settings.IsValidFormula(txt278Sensitivity.Text, false)) e.Cancel = true;
        }

        private void gridAnalysisObjects_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 && // grids have only one column
                !Settings.IsValidFormula(e.FormattedValue?.ToString(), false)) e.Cancel = true;
        }

        private void gridBreakDown_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colBreakDownVariable.Index) return;
            if (string.IsNullOrEmpty(e.FormattedValue?.ToString()))
            {
                MessageBox.Show("Breakdown variable must not be empty.");
                e.Cancel = true; return;
            }
            if (FormulaEditor.ContainsSeparator(e.FormattedValue.ToString()))
            {
                MessageBox.Show($"{e.FormattedValue}: breakdown variable does not allow for formulas.");
                e.Cancel = true; return;
            }
            if (e.FormattedValue.ToString().Trim().EndsWith(Settings.BASELINE_MARKER))
            {
                MessageBox.Show($"{e.FormattedValue}: breakdown is always based on baseline variables, thus '{Settings.BASELINE_MARKER}' does not make sense.");
                e.Cancel = true; return;
            }
        }

        private void SettingsDistributional_Load(object sender, EventArgs e)
        {
            //preselect the only choice in the disabled combo
            combo26Level.SelectedIndex = 0;
        }
    }
}
