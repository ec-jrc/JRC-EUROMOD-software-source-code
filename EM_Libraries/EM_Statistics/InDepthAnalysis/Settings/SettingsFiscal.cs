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
    internal partial class SettingsFiscal : Form, ISettings
    {
        internal const string pageName_Fiscal = "Fiscal";
        internal const string tableName_RevenueExpenditure = "RevenueExpenditure";
        internal const string tableName_FiscalTaxpayersBeneficiaries = "FiscalTaxpayersBeneficiaries";
        internal const string tableName_DisaggregatedConcepts = "DisaggregatedConcepts";
        internal const string tableName_DisaggregatedConceptsUnits = "DisaggregatedConceptsUnits";

        private const string actionName_Units = "Units";
        private const string varName_HHMembersCount = "HHMembersCount";
        private const string varName_NumberOne = "Number_One";
        private const string parName_Var = "var";
        private const string colName_Baseline = "Baseline";
        private const string colName_Reform = "Reform";
        private const string rowAndVarName_FiscalTaxpayersBeneficiaries_OtherOriginalIncome = "FiscalTaxpayersBeneficiaries_OtherOriginalIncome";
        private const string cellName_NetEffect_TotalDifference = "NetEffect_TotalDifference";
        private const string cellName_NetEffect_PercentageDifference = "NetEffect_PercentageDifference";
        private const string parName_NetEffect_base = "NetEffect_base";
        private const string varName_NetEffect = "NetEffect";

        private const double TOLERANCE_PCTDIFF_SUM_COMPONENTS = 0.00005;

        internal class UserSettings
        {
            private const string XMLTAG_PAGE_TITLE_FISCAL = "PageTitle_Fiscal";
            private const string XMLTAG_TABLE_TITLE_REVENUE_EXPENDITURE = "TableTitle_RevenueExpenditure";
            private const string XMLTAG_TABLE_TITLE_FISCAL_TAXPAYERS_BENEFICIARIES = "TableTitle_FiscalTaxpayersBeneficiaries";
            private const string XMLTAG_TABLE_TITLE_DISAGGREGATED_CONCEPTS = "TableTitle_DisaggregatedConcepts";
            private const string XMLTAG_TABLE_TITLE_DISAGGREGATED_CONCEPTS_UNITS = "TableTitle_DisaggregatedConceptsUnits";
            private const string XMLTAG_TAXPAYERS = "Taxpayers";
            private const string XMLTAG_BENEFICIARIES = "Beneficiaries";
            private const string XMLTAG_CALCULATION_LEVEL = "CalculationLevel";
            private const string XMLTAG_DISAGGREGATED_CONCEPT = "DisaggregatedConcept";
            private const string XMLTAG_CONCEPT = "Concept";
            private const string XMLTAG_FORMULA = "Formula";
            private const string XMLTAG_FORMULA_FILTER = "FormulaFilter";
            private const string XMLTAG_TAXPAYERS_BENEFICIARIES = "TaxpayersBeneficiaries";
            private const string XMLTAG_BOLD = "Bold";
            private const string XMLTAG_BORDER = "Border";

            internal string pageTitle_Fiscal = "1. Fiscal";
            internal string tableTitle_RevenueExpenditure = "1.1. Aggregate earnings, government revenue and expenditure (annual)";
            internal string tableTitle_FiscalTaxpayersBeneficiaries = "1.2. Number of earners/taxpayers/beneficiaries";
            internal string tableTitle_DisaggregatedConcepts = "1.3. Disaggregated concepts (annual)";
            internal string tableTitle_DisaggregatedConceptsUnits = "1.4. Disaggregated concepts - number of units";

            internal string taxpayers = "!=0";
            internal string beneficiaries = "!=0";
            internal string calculationLevel = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;

            internal class DisaggregatedConcept
            {
                internal readonly string formula, concept, formulaFilter = null, calculationLevel = null, taxpayersBeneficiaries = null;
                internal readonly bool bold = false, border = false;
                internal DisaggregatedConcept(string _concept, string _formula, string _formulaFilter,
                                              string _calculationLevel, string _taxpayersBeneficiaries, bool _bold, bool _border)
                {
                    concept = _concept; formula = _formula; formulaFilter = _formulaFilter;
                    calculationLevel = _calculationLevel; taxpayersBeneficiaries = _taxpayersBeneficiaries;
                    bold = _bold; border = _border;
                }
            }
            internal List<DisaggregatedConcept> disaggregatedConcepts = new List<DisaggregatedConcept>();

            internal void FromXml(XElement xElement, out string warnings)
            {
                warnings = string.Empty;
                foreach (XElement xe in xElement.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (Settings.GetXEleName(xe))
                    {
                        case XMLTAG_PAGE_TITLE_FISCAL:
                            pageTitle_Fiscal = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_REVENUE_EXPENDITURE:
                            tableTitle_RevenueExpenditure = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_FISCAL_TAXPAYERS_BENEFICIARIES:
                            tableTitle_FiscalTaxpayersBeneficiaries = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_DISAGGREGATED_CONCEPTS:
                            tableTitle_DisaggregatedConcepts = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_DISAGGREGATED_CONCEPTS_UNITS:
                            tableTitle_DisaggregatedConceptsUnits = xe.Value; break;
                        case XMLTAG_TAXPAYERS:
                            taxpayers = xe.Value; break;
                        case XMLTAG_BENEFICIARIES:
                            beneficiaries = xe.Value; break;
                        case XMLTAG_CALCULATION_LEVEL:
                            calculationLevel = xe.Value; break;
                        case XMLTAG_DISAGGREGATED_CONCEPT + "s":
                            foreach (XElement xeSub in xe.Elements()) disaggregatedConcepts.Add(ReadDisaggregatedConcept(xeSub)); break;
                    }
                }

                DisaggregatedConcept ReadDisaggregatedConcept(XElement xe)
                {
                    string concept = string.Empty, formula = string.Empty, formulaFilter = null, calculationLevel = null, taxpayersBeneficiaries = null;
                    bool bold = false, border = false;
                    foreach (XElement xeSub in xe.Elements())
                    {
                        if (xeSub.Value == null) continue;
                        switch (Settings.GetXEleName(xeSub))
                        {
                            case XMLTAG_CONCEPT: concept = xeSub.Value; break;
                            case XMLTAG_FORMULA: formula = xeSub.Value; break;
                            case XMLTAG_FORMULA_FILTER: formulaFilter = xeSub.Value; break;
                            case XMLTAG_CALCULATION_LEVEL: calculationLevel = xeSub.Value; break;
                            case XMLTAG_TAXPAYERS_BENEFICIARIES: taxpayersBeneficiaries = xeSub.Value; break;
                            case XMLTAG_BOLD: bool.TryParse(xeSub.Value, out bold); break;
                            case XMLTAG_BORDER: bool.TryParse(xeSub.Value, out border); break;
                        }
                    }
                    return new DisaggregatedConcept(concept, formula, formulaFilter, calculationLevel, taxpayersBeneficiaries, bold, border);
                }
            }

            internal void ToXml(XmlWriter xmlWriter)
            {
                Settings.WriteElement(xmlWriter, XMLTAG_PAGE_TITLE_FISCAL, pageTitle_Fiscal);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_REVENUE_EXPENDITURE, tableTitle_RevenueExpenditure);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_FISCAL_TAXPAYERS_BENEFICIARIES, tableTitle_FiscalTaxpayersBeneficiaries);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_DISAGGREGATED_CONCEPTS, tableTitle_DisaggregatedConcepts);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_DISAGGREGATED_CONCEPTS_UNITS, tableTitle_DisaggregatedConceptsUnits);

                Settings.WriteElement(xmlWriter, XMLTAG_TAXPAYERS, taxpayers);
                Settings.WriteElement(xmlWriter, XMLTAG_BENEFICIARIES, beneficiaries);
                Settings.WriteElement(xmlWriter, XMLTAG_CALCULATION_LEVEL, calculationLevel);

                WriteDisaggregatedConcepts();

                void WriteDisaggregatedConcepts()
                {
                    xmlWriter.WriteStartElement(XMLTAG_DISAGGREGATED_CONCEPT + "s");
                    foreach (DisaggregatedConcept dc in disaggregatedConcepts)
                    {
                        xmlWriter.WriteStartElement(XMLTAG_DISAGGREGATED_CONCEPT);
                        Settings.WriteElement(xmlWriter, XMLTAG_CONCEPT, dc.concept);
                        Settings.WriteElement(xmlWriter, XMLTAG_FORMULA, dc.formula);
                        Settings.WriteElement(xmlWriter, XMLTAG_FORMULA_FILTER, dc.formulaFilter);
                        Settings.WriteElement(xmlWriter, XMLTAG_CALCULATION_LEVEL, dc.calculationLevel);
                        Settings.WriteElement(xmlWriter, XMLTAG_TAXPAYERS_BENEFICIARIES, dc.taxpayersBeneficiaries);
                        Settings.WriteElement(xmlWriter, XMLTAG_BOLD, dc.bold.ToString(), false);
                        Settings.WriteElement(xmlWriter, XMLTAG_BORDER, dc.border.ToString(), false);
                        xmlWriter.WriteEndElement();
                    }
                    xmlWriter.WriteEndElement();
                }
            }

            internal void WriteMetadata(TemplateApi templateApi, List<string> inactiveTablesAndPages)
            {
                if (!inactiveTablesAndPages.Contains(tableName_RevenueExpenditure))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Fiscal, tableTitle_RevenueExpenditure);
                }
                if (!inactiveTablesAndPages.Contains(tableName_FiscalTaxpayersBeneficiaries))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Fiscal, tableTitle_FiscalTaxpayersBeneficiaries);
                    Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Level of analysis", calculationLevel);
                    Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Taxpayers", taxpayers);
                    Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Beneficiaries", beneficiaries);
                }

                if (disaggregatedConcepts.Any() &&
                   (!inactiveTablesAndPages.Contains(tableName_DisaggregatedConcepts) || !inactiveTablesAndPages.Contains(tableName_DisaggregatedConceptsUnits)))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_Fiscal, tableTitle_DisaggregatedConcepts + "<br>" + tableTitle_DisaggregatedConceptsUnits);
                    foreach (DisaggregatedConcept disCon in disaggregatedConcepts)
                    {
                        Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Concept", string.IsNullOrEmpty(disCon.concept) ? "no concept" : disCon.concept);
                        Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Variable/Formula", disCon.formula);
                        if (!string.IsNullOrEmpty(disCon.formulaFilter)) Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Formula filter", disCon.formulaFilter);
                        Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Taxpayers/Beneficiaries", disCon.taxpayersBeneficiaries);
                        Settings.AddMetaDataRow(templateApi, pageName_Fiscal, "Taxp./Ben. level of analysis", disCon.calculationLevel, true);
                    }
                }
            }
        }

        private Settings settings = null;
        internal Dictionary<string, Settings.TextBoxInfo> checkFields = new Dictionary<string, Settings.TextBoxInfo>();
        internal Dictionary<string, Settings.DataGridViewInfo> checkGridViews = new Dictionary<string, Settings.DataGridViewInfo>();


        internal SettingsFiscal()
        {
            InitializeComponent();
            InDepthAnalysis.SetShowHelp(this, helpProvider);
        }

        void ISettings.ModifyTemplate(TemplateApi templateApi, out List<Template.TemplateInfo.UserVariable> systemSpecificVars)
        {
            systemSpecificVars = null;
            if (settings == null || settings.inactiveTablesAndPages.Contains(pageName_Fiscal)) return;

            templateApi.ModifyPage(new Template.Page() { name = pageName_Fiscal, title = settings.userSettingsFiscal.pageTitle_Fiscal });

            if (!settings.inactiveTablesAndPages.Contains(tableName_RevenueExpenditure))
            {
                templateApi.ModifyTable(new Template.Page.Table() { name = tableName_RevenueExpenditure,
                    title = settings.userSettingsFiscal.tableTitle_RevenueExpenditure }, pageName_Fiscal);

                if (settings.baselineReformPackages.Count == 1) // no incomelist-component-break-down if more than one country is analysed
                {
                    systemSpecificVars = new List<Template.TemplateInfo.UserVariable>();
                    foreach (string il in new List<string> { InDepthDefinitions.ILS_EARNS, InDepthDefinitions.ILS_TAX, InDepthDefinitions.ILS_SICCT, InDepthDefinitions.ILS_SICDY, InDepthDefinitions.ILS_SICER,
                                                             InDepthDefinitions.ILS_PEN, InDepthDefinitions.ILS_BENMT, InDepthDefinitions.ILS_BENNT })
                    {
                        ModifyTemplate_RevenueExpenditure(templateApi, il, out List<Template.TemplateInfo.UserVariable> ssv);
                        systemSpecificVars.AddRange(ssv);
                    }
                }

                if (!settings.compareWithBaseline)
                {
                    foreach (string cellName in new List<string>() { cellName_NetEffect_TotalDifference, cellName_NetEffect_PercentageDifference } )
                        templateApi.ModifyCellAction_Cell(new Template.Action() { parameters = new List<Template.Parameter>() { new Template.Parameter() {
                            name = parName_NetEffect_base, variableName = varName_NetEffect, _source = Template.Parameter.Source.PREVIOUS_REFORM } } },
                            pageName_Fiscal, tableName_RevenueExpenditure, cellName, TemplateApi.ModifyMode.MergeReplace, true);
                }
            }

            if (!settings.inactiveTablesAndPages.Contains(tableName_FiscalTaxpayersBeneficiaries))
            {
                string calculationLevel = settings.userSettingsFiscal.calculationLevel == HardDefinitions.DefaultCalculationLevels.INDIVIDUAL
                    ? HardDefinitions.DefaultCalculationLevels.INDIVIDUAL : HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
                string countFormula = (settings.userSettingsFiscal.calculationLevel == HardDefinitions.DefaultCalculationLevels.INDIVIDUAL ||
                                       settings.userSettingsFiscal.calculationLevel == HardDefinitions.DefaultCalculationLevels.HOUSEHOLD)
                    ? "1.0" : $"{Settings.DATA_VAR(varName_HHMembersCount)}";
                templateApi.ModifyTableActions(new Template.Action() {
                    name = actionName_Units, _calculationLevel = calculationLevel, formulaString = countFormula },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, TemplateApi.ModifyMode.MergeReplace);
                templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = calculationLevel },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, TemplateApi.ModifyMode.MergeReplace);
                templateApi.ModifyCellAction_Table(new Template.Action() { _calculationLevel = calculationLevel },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, TemplateApi.ModifyMode.MergeReplace);

                templateApi.ModifyTable(new Template.Page.Table() { name = tableName_FiscalTaxpayersBeneficiaries,
                    title = settings.userSettingsFiscal.tableTitle_FiscalTaxpayersBeneficiaries }, pageName_Fiscal);

                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsFiscal.taxpayers,
                   out string taxpayersCondition, out List<Template.Parameter> taxpayersConditionParameters)) taxpayersCondition = "!=0";
                if (!Settings.HandleFormulaString(templateApi, settings.userSettingsFiscal.beneficiaries,
                    out string beneficiariesCondition, out List<Template.Parameter> beneficiariesConditionParameters)) beneficiariesCondition = "!=0";

                templateApi.ModifyCellFilter_Row(new Template.Filter() { // adapt the filter for the other income row, for the incomelists this is done in ModifyTemplate_TaxpayersBeneficiaries
                    formulaString = $"{Settings.DATA_VAR(rowAndVarName_FiscalTaxpayersBeneficiaries_OtherOriginalIncome)}{taxpayersCondition}",
                    parameters = taxpayersConditionParameters },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, rowAndVarName_FiscalTaxpayersBeneficiaries_OtherOriginalIncome);

                if (systemSpecificVars == null) systemSpecificVars = new List<Template.TemplateInfo.UserVariable>();
                foreach (string il in new List<string> { InDepthDefinitions.ILS_EARNS, InDepthDefinitions.ILS_TAX, InDepthDefinitions.ILS_SICCT, InDepthDefinitions.ILS_SICDY, InDepthDefinitions.ILS_SICER })
                {
                    ModifyTemplate_TaxpayersBeneficiaries(templateApi, il, taxpayersCondition, taxpayersConditionParameters, out List<Template.TemplateInfo.UserVariable> ssv);
                    systemSpecificVars.AddRange(ssv);
                }
                foreach (string il in new List<string> { InDepthDefinitions.ILS_PEN, InDepthDefinitions.ILS_BENMT, InDepthDefinitions.ILS_BENNT })
                {
                    ModifyTemplate_TaxpayersBeneficiaries(templateApi, il, beneficiariesCondition, beneficiariesConditionParameters, out List<Template.TemplateInfo.UserVariable> ssv);
                    systemSpecificVars.AddRange(ssv);
                }
            }

            if (settings.inactiveTablesAndPages.Contains(tableName_DisaggregatedConcepts) &&
                settings.inactiveTablesAndPages.Contains(tableName_DisaggregatedConceptsUnits)) return;
            if (!settings.inactiveTablesAndPages.Contains(tableName_DisaggregatedConcepts)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_DisaggregatedConcepts, title = settings.userSettingsFiscal.tableTitle_DisaggregatedConcepts }, pageName_Fiscal);
            if (!settings.inactiveTablesAndPages.Contains(tableName_DisaggregatedConceptsUnits)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_DisaggregatedConceptsUnits, title = settings.userSettingsFiscal.tableTitle_DisaggregatedConceptsUnits }, pageName_Fiscal);
            ModifyTemplate_DisaggregatedConcepts(templateApi);
        }

        private class ILVarSummary
        {
            internal string varName = string.Empty;
            internal string description = string.Empty;
            internal bool? baselineSubstract = false;
            internal List<bool?> reformsSubstract = new List<bool?>();
        }

        private List<ILVarSummary> GetAllSysILComposition(string ilName)
        {
            BaselineReformPackage brp = settings.baselineReformPackages[0]; // this function is only called if there is exactly one baseline-reform-package
            List<ILVarSummary> allSysILComposition = new List<ILVarSummary>();
            if (brp.baseline.systemInfo != null)
                foreach (var vi in brp.baseline.systemInfo.GetIncomelistContent(ilName))
                {
                    ILVarSummary ivs = new ILVarSummary() { varName = vi.Key, description = vi.Value.description, baselineSubstract = vi.Value.substract };
                    for (int r = 0; r < brp.reforms.Count; ++r) ivs.reformsSubstract.Add(null);
                    allSysILComposition.Add(ivs);
                }
            foreach (BaselineReformPackage.BaselineOrReform reform in brp.reforms)
                if (reform.systemInfo != null)
                    foreach (var vi in reform.systemInfo.GetIncomelistContent(ilName))
                    {
                        ILVarSummary ivs = (from a in allSysILComposition where a.varName.ToLower() == vi.Key.ToLower() select a).FirstOrDefault();
                        if (ivs == null)
                        {
                            ivs = new ILVarSummary() { varName = vi.Key, description = vi.Value.description, baselineSubstract = null };
                            for (int r = 0; r < brp.reforms.Count; ++r) ivs.reformsSubstract.Add(null);
                            allSysILComposition.Add(ivs);
                        }
                        ivs.reformsSubstract[brp.reforms.IndexOf(reform)] = vi.Value.substract;
                    }
            return allSysILComposition;
        }

        private void ModifyTemplate_RevenueExpenditure(TemplateApi templateApi, string incomelistName, out List<Template.TemplateInfo.UserVariable> sysSpecILFactors)
        {
            sysSpecILFactors = new List<Template.TemplateInfo.UserVariable>();
            
            List<string> sumComponents = new List<string>();
            foreach (ILVarSummary ivs in GetAllSysILComposition(incomelistName))
            {
                string userVarNameTab = Settings.MakeId(), userVarNameCheck = Settings.MakeId(), varNameCheck = Settings.MakeId(),
                       rowName = Settings.MakeId(), cellNameBase = Settings.MakeId(), cellNameReform = Settings.MakeId();

                // create optional variable that reads the component from file, with default value NaN ...
                if (!templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable()
                    { name = ivs.varName, readVar = ivs.varName, defaultValue = double.NaN })) continue;
                // ... and again with default value zero for the checking whether ils-value and sum of components correspond
                if (!templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable()
                    { name = varNameCheck, readVar = ivs.varName, defaultValue = 0 })) continue;

                // create a user variable that takes the incomelist-factor, i.e. + or - or n/a, transformed to 1 or -1 or NaN
                // this allows having different values for the reforms, as user variables can be set per reform
                GenerateILFactors(userVarNameTab, ivs, sysSpecILFactors, false); // for the table, with NaN for n/a
                GenerateILFactors(userVarNameCheck, ivs, sysSpecILFactors, true); // ... and the checking, with 0 for n/a

                string formulaTab = $"{Settings.USR_VAR(userVarNameTab) } * { Settings.DATA_VAR(ivs.varName)} * 12";
                // add the row that will show the variable
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowName, title = $"{SubstractToText(ivs)} {ivs.description} ({ivs.varName})" },
                    pageName_Fiscal, tableName_RevenueExpenditure,
                    TemplateApi.ModifyMode.AddNew, TemplateApi.AddWhere.Before, $"{tableName_RevenueExpenditure}_{incomelistName}")) continue;
                // add the cell for the baseline-column, as we require a row- and column-specific formula
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameBase, rowName = rowName, colName = colName_Baseline },
                    pageName_Fiscal, tableName_RevenueExpenditure, TemplateApi.ModifyMode.AddNew)) continue;
                if (!templateApi.ModifyCellAction_Cell(new Template.Action() { formulaString = formulaTab }, // add the formula for the baseline
                    pageName_Fiscal, tableName_RevenueExpenditure, cellNameBase, TemplateApi.ModifyMode.AddNew)) continue;
                // add the cell for the reform-columns, as we require a row- and column-specific formula
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameReform, rowName = rowName, colName = colName_Reform },
                    pageName_Fiscal, tableName_RevenueExpenditure, TemplateApi.ModifyMode.AddNew, true)) continue;
                if (!templateApi.ModifyCellAction_Cell(new Template.Action() { formulaString = formulaTab }, // add the formula for the reforms
                    pageName_Fiscal, tableName_RevenueExpenditure, cellNameReform, TemplateApi.ModifyMode.AddNew, true)) continue;

                // add the variable to the check-sum for comparing with the incomelist-value (below)
                sumComponents.Add($"{Settings.USR_VAR(userVarNameCheck) } * { Settings.DATA_VAR(varNameCheck)}");
            }

            // compare sum of components with incomelist-value
            if (!sumComponents.Any()) return;
            string varSumComponents = Settings.MakeId(), varDiff = Settings.MakeId();
            // add an action creating a variable that takes the sum of components
            templateApi.ModifyTableActions(new Template.Action() { outputVar = varSumComponents, calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                formulaString = string.Join("+", sumComponents) },
                pageName_Fiscal, tableName_RevenueExpenditure);
            // add an action creating a variable that takes relative difference between sum of components and incomelist-value (sum comp - il-val) / il-val
            templateApi.ModifyTableActions(new Template.Action() { outputVar = varDiff, calculationType = HardDefinitions.CalculationType.CalculateSumWeighted,
                formulaString = $"({Settings.DATA_VAR(varSumComponents)} - {Settings.DATA_VAR(incomelistName)}) / {Settings.DATA_VAR(incomelistName)}" },
                pageName_Fiscal, tableName_RevenueExpenditure);

            // add a message that is issued, if the relative difference exceeds a minimum
            // note that the sum of components is quite imprecise as multiplying with weights enlarges the failure due to (usually) 2-decimal-rounded numbers
            // (the diffenrences go against zero, with 1-weights or more decimals outputted)
            string msg = $"{PrettyInfoProvider.PRETTY_INFO_REF_SYS_LABEL}: {incomelistName}: relative difference between sum and components >{TOLERANCE_PCTDIFF_SUM_COMPONENTS * 100}%.";
            List<Template.Parameter> parameters = new List<Template.Parameter>()
                { new Template.Parameter() { name = EM_TemplateCalculator.PAR_MESSAGE_SWITCH_VAR, variableName = varDiff },
                    new Template.Parameter() { name = EM_TemplateCalculator.PAR_MESSAGE_RANGE_MAX_X + "1", numericValue = (-1) * TOLERANCE_PCTDIFF_SUM_COMPONENTS, stringValue = msg  },
                    new Template.Parameter() { name = EM_TemplateCalculator.PAR_MESSAGE_RANGE_MIN_X + "2", numericValue = TOLERANCE_PCTDIFF_SUM_COMPONENTS, stringValue = msg  } };
            templateApi.ModifyTableActions(new Template.Action() { calculationType = HardDefinitions.CalculationType.Message, parameters = parameters },
                pageName_Fiscal, tableName_RevenueExpenditure);

            void GenerateILFactors(string userVarName, ILVarSummary ivs, List<Template.TemplateInfo.UserVariable> _sysSpecILFactors, bool naToZero)
            { 
                // implementing a user variable requires two steps:
                // 1. step: (usually done in the template): defining the user variable (in our case name and type)
                if (!templateApi.ModifyUserVariables(new Template.TemplateInfo.UserVariable()
                    { name = userVarName, inputType = HardDefinitions.UserInputType.Numeric })) return;
                // 2. step: (usually done by the user or a programme): assigning values, if the latter and appropriate, per package and reform (in our case per reform)
                for (int r = 0; r < ivs.reformsSubstract.Count; ++r) _sysSpecILFactors.Add(new Template.TemplateInfo.UserVariable() { name = userVarName,
                    refNo = r, value = SubstractToNum(ivs.reformsSubstract[r], naToZero) }); // assigning a value for each reform ...
                _sysSpecILFactors.Add(new Template.TemplateInfo.UserVariable() { name = userVarName, // ... and for the baseline
                    refNo = -1, value = SubstractToNum(ivs.baselineSubstract, naToZero) });
            }
            string SubstractToNum(bool? sub, bool naToZero) { return sub == null ? (naToZero ? "0.0" : double.NaN.ToString()) : (sub == true ? "-1.0" : "1.0"); }
        }

        private void ModifyTemplate_TaxpayersBeneficiaries(TemplateApi templateApi, string incomelistName,
                                                           string condition, List<Template.Parameter> conditionParameters,
                                                           out List<Template.TemplateInfo.UserVariable> sysSpecILFactors)
        {
            sysSpecILFactors = new List<Template.TemplateInfo.UserVariable>();

            // add the filter with the user defined condition to the row referring the income-list (e.g. ils_tax)
            templateApi.ModifyCellFilter_Row(new Template.Filter() { formulaString = $"{Settings.DATA_VAR(incomelistName)}{condition}", parameters = conditionParameters },
                pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, $"{tableName_FiscalTaxpayersBeneficiaries}_{incomelistName}");

            if (settings.baselineReformPackages.Count > 1) return; // no incomelist-component-break-down if more than one country is analysed

            // add rows for each component-variable
            foreach (var ivs in GetAllSysILComposition(incomelistName))
            {
                string userVarName = Settings.MakeId(), rowName = Settings.MakeId(), cellNameBase = Settings.MakeId(), cellNameReform = Settings.MakeId();

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
                for (int r = 0; r < ivs.reformsSubstract.Count; ++r) sysSpecILFactors.Add(new Template.TemplateInfo.UserVariable() { name = userVarName,
                    refNo = r, value = ivs.reformsSubstract[r] == null ? double.NaN.ToString() : "1.0" }); // 2. step: assigning a value for each reform

                // add the row that will show the variable
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowName, title = $"{SubstractToText(ivs)} {ivs.description} ({ivs.varName})" },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries,
                    TemplateApi.ModifyMode.AddNew, TemplateApi.AddWhere.Before, $"{tableName_FiscalTaxpayersBeneficiaries}_{incomelistName}")) continue;
                // add the cell for the baseline-column, as we require a row- and column-specific formula
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameBase, rowName = rowName, colName = colName_Baseline },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, TemplateApi.ModifyMode.AddNew)) continue;
                if (!templateApi.ModifyCellAction_Cell(new Template.Action() { // add the formula for the baseline
                    formulaString = ivs.baselineSubstract == null ? double.NaN.ToString() : Settings.DATA_VAR(actionName_Units) },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, cellNameBase, TemplateApi.ModifyMode.AddNew)) continue;
                templateApi.ModifyCellFilter_Cell(new Template.Filter() { // add the filter that allows counting taxpayers respectively beneficiaries
                    formulaString = $"{Settings.DATA_VAR(ivs.varName)}{condition}", parameters = conditionParameters },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, cellNameBase);
                // add the cell for the reform-columns, as we require a row- and column-specific formula
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellNameReform, rowName = rowName, colName = colName_Reform },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, TemplateApi.ModifyMode.AddNew, true)) continue;
                if (!templateApi.ModifyCellAction_Cell(new Template.Action() { // add the formula for the reforms
                    formulaString = $"{Settings.USR_VAR(userVarName)} * {Settings.DATA_VAR(actionName_Units)}" },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, cellNameReform, TemplateApi.ModifyMode.AddNew, true)) continue;
                templateApi.ModifyCellFilter_Cell(new Template.Filter() { // add the filter that allows counting taxpayers respectively beneficiaries
                    formulaString = $"{Settings.DATA_VAR(ivs.varName)}{condition}", parameters = conditionParameters },
                    pageName_Fiscal, tableName_FiscalTaxpayersBeneficiaries, cellNameReform, TemplateApi.ModifyMode.AddNew, true);
            }
        }

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

        private void ModifyTemplate_DisaggregatedConcepts(TemplateApi templateApi)
        {
            if (!settings.userSettingsFiscal.disaggregatedConcepts.Any())
            {
                templateApi.ModifyTable(new Template.Page.Table() { name = tableName_DisaggregatedConcepts, active = false }, pageName_Fiscal);
                templateApi.ModifyTable(new Template.Page.Table() { name = tableName_DisaggregatedConceptsUnits, active = false }, pageName_Fiscal);
                return;
            }

            foreach (UserSettings.DisaggregatedConcept dc in settings.userSettingsFiscal.disaggregatedConcepts)
            {
                if (!Settings.HandleFormulaString(templateApi, dc.formula, 
                    out string handledFormula, out List<Template.Parameter> formulaParameters)) continue;

                string actionId = Settings.MakeId(); string varId = Settings.MakeOutVarName(dc.concept);
                if (!templateApi.ModifyGlobalActions(new Template.Action() { calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                    name = actionId, outputVar = varId, formulaString = handledFormula, parameters = formulaParameters })) continue;
                if (!string.IsNullOrEmpty(dc.formulaFilter))
                {
                    if (!Settings.HandleFormulaString(templateApi, dc.formulaFilter, out string handledFilter, out List<Template.Parameter> filterParameters)) continue;
                    if (!templateApi.ModifyFilter_GlobalAction(new Template.Filter() {
                        formulaString = handledFilter, parameters = filterParameters }, actionId)) continue;
                }

                if (!settings.inactiveTablesAndPages.Contains(tableName_DisaggregatedConcepts))
                { 
                    string rowId = Settings.MakeId();
                    if (!templateApi.ModifyRows(new Template.Page.Table.Row() {
                        name = rowId, title = $"{dc.concept} ({FormulaEditor.Remove_DATA_VAR(dc.formula)})", hasSeparatorAfter = dc.border, strong = dc.bold },
                        pageName_Fiscal, tableName_DisaggregatedConcepts, TemplateApi.ModifyMode.AddNew)) continue;
                    templateApi.ModifyCellAction_Row(new Template.Action() {
                        parameters = new List<Template.Parameter>() { new Template.Parameter() { name = parName_Var, variableName = varId } } },
                        pageName_Fiscal, tableName_DisaggregatedConcepts, rowId);
                }

                if (!settings.inactiveTablesAndPages.Contains(tableName_DisaggregatedConceptsUnits))
                {
                    string rowId = Settings.MakeId();
                    if (!templateApi.ModifyRows(new Template.Page.Table.Row() {
                        name = rowId, title = $"{dc.concept} ({FormulaEditor.Remove_DATA_VAR(dc.formula)})", hasSeparatorAfter = dc.border, strong = dc.bold },
                        pageName_Fiscal, tableName_DisaggregatedConceptsUnits, TemplateApi.ModifyMode.AddNew)) continue;

                    string calculationLevel = dc.calculationLevel == HardDefinitions.DefaultCalculationLevels.INDIVIDUAL
                        ? dc.calculationLevel : HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;
                    string countVar = dc.calculationLevel == HardDefinitions.DefaultCalculationLevels.INDIVIDUAL ||
                        dc.calculationLevel == HardDefinitions.DefaultCalculationLevels.HOUSEHOLD ? varName_NumberOne : varName_HHMembersCount;
                    string actionFormula = $"{Settings.DATA_VAR(countVar)}";

                    foreach (string colName in new List<string>() { colName_Baseline, colName_Reform })
                    {
                        string cellName = $"{colName}{rowId}"; bool isReform = colName == colName_Reform;
                        if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellName, rowName = rowId, colName = colName },
                            pageName_Fiscal, tableName_DisaggregatedConceptsUnits, TemplateApi.ModifyMode.AddNew, isReform)) continue;
                        templateApi.ModifyCellAction_Cell(new Template.Action() { calculationType = HardDefinitions.CalculationType.CalculateSumWeighted,
                            formulaString = actionFormula, _calculationLevel = calculationLevel },
                            pageName_Fiscal, tableName_DisaggregatedConceptsUnits, cellName, TemplateApi.ModifyMode.AddNew, isReform);
                    }

                    if (!string.IsNullOrEmpty(dc.taxpayersBeneficiaries))
                    {
                        if (!Settings.HandleFormulaString(templateApi, dc.taxpayersBeneficiaries,
                            out string handledTaxpayersBeneficiaries, out List<Template.Parameter> taxpayersBeneficiariesParameters)) continue;
                        string taxpayersBeneficiariesCondition = $"{Settings.DATA_VAR(varId)}{handledTaxpayersBeneficiaries}";
                        foreach (string colName in new List<string>() { colName_Baseline, colName_Reform })
                            templateApi.ModifyCellFilter_Cell(new Template.Filter() { formulaString = taxpayersBeneficiariesCondition, parameters = taxpayersBeneficiariesParameters },
                                pageName_Fiscal, tableName_DisaggregatedConceptsUnits, $"{colName}{rowId}", TemplateApi.ModifyMode.AddNew, colName == colName_Reform);
                    }
                }
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

        void UpdateControls()
        {
            txtPageTitleFiscal.Text = settings.userSettingsFiscal.pageTitle_Fiscal;
            txtTableTitleRevenueExpenditure.Text = settings.userSettingsFiscal.tableTitle_RevenueExpenditure;
            txtTableTitleFiscalTaxpayersBeneficiaries.Text = settings.userSettingsFiscal.tableTitle_FiscalTaxpayersBeneficiaries;
            txtTableTitleDisaggregatedConcepts.Text = settings.userSettingsFiscal.tableTitle_DisaggregatedConcepts;
            txtTableTitleDisaggregatedConceptsUnits.Text = settings.userSettingsFiscal.tableTitle_DisaggregatedConceptsUnits;

            txtTaxpayers.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsFiscal.taxpayers);
            txtBeneficiaries.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsFiscal.beneficiaries);
            comboCalculationLevel.Text = settings.userSettingsFiscal.calculationLevel;

            FillGridDisaggregatedConcepts(settings.userSettingsFiscal.disaggregatedConcepts);
        }

        private void FillGridDisaggregatedConcepts(List<UserSettings.DisaggregatedConcept> disaggregatedConcepts)
        {
            gridDisCon.Rows.Clear();
            foreach (UserSettings.DisaggregatedConcept dc in disaggregatedConcepts)
                gridDisCon.Rows.Add(dc.concept,
                    FormulaEditor.Remove_DATA_VAR(dc.formula),
                    FormulaEditor.Remove_DATA_VAR(dc.formulaFilter),
                    FormulaEditor.Remove_DATA_VAR(dc.taxpayersBeneficiaries),
                    dc.calculationLevel, dc.bold, dc.border);
        }

        private void SaveGridDisaggregatedConcepts()
        {
            settings.userSettingsFiscal.disaggregatedConcepts.Clear();
            foreach (DataGridViewRow row in gridDisCon.Rows)
            {
                string concept = row.Cells[colDisConConcept.Index].Value?.ToString();
                string formula = FormulaEditor.Add_DATA_VAR(row.Cells[colDisConFormula.Index].Value?.ToString());
                string formulaFilter = FormulaEditor.Add_DATA_VAR(row.Cells[colDisConFormulaFilter.Index].Value?.ToString());
                string taxpayersBeneficiaries = FormulaEditor.Add_DATA_VAR(row.Cells[colDisConTaxpayersBeneficiaries.Index].Value?.ToString());
                string calculationLevel = row.Cells[colDisConLevel.Index].Value?.ToString();
                bool.TryParse(row.Cells[colDisConBold.Index].Value?.ToString() ?? string.Empty, out bool bold);
                bool.TryParse(row.Cells[colDisConBorder.Index].Value?.ToString() ?? string.Empty, out bool border);
                if (!string.IsNullOrEmpty(formula)) settings.userSettingsFiscal.disaggregatedConcepts.Add(
                    new UserSettings.DisaggregatedConcept(concept, formula, formulaFilter, calculationLevel, taxpayersBeneficiaries, bold, border));
            }
        }

        private void btnDisConAddRow_Click(object sender, EventArgs e)
        {
            int i = gridDisCon.Rows.Add(string.Empty, string.Empty, string.Empty, "!=0", HardDefinitions.DefaultCalculationLevels.INDIVIDUAL, false, false);
            gridDisCon.CurrentCell = gridDisCon.Rows[i].Cells[colDisConConcept.Index]; gridDisCon.BeginEdit(true);
        }

        private void btnDisConDelRow_Click(object sender, EventArgs e)
        {
            if (gridDisCon.SelectedRows.Count > 0) gridDisCon.Rows.Remove(gridDisCon.SelectedRows[0]);
            else if (gridDisCon.SelectedCells.Count > 0) gridDisCon.Rows.Remove(gridDisCon.SelectedCells[0].OwningRow);
        }

        private void btnDisConMoveUp_Click(object sender, EventArgs e)
        {
            if (gridDisCon.SelectedRows.Count == 0 && gridDisCon.SelectedCells.Count == 0) return;
            DataGridViewRow row = gridDisCon.SelectedRows.Count > 0 ? gridDisCon.SelectedRows[0] : gridDisCon.SelectedCells[0].OwningRow;
            int iSelRow = gridDisCon.Rows.IndexOf(row); if (iSelRow == 0) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in row.Cells) rowContent.Add(cell.Value);
            gridDisCon.Rows.Insert(iSelRow - 1, rowContent.ToArray()); gridDisCon.Rows.RemoveAt(iSelRow + 1);
            gridDisCon.ClearSelection(); gridDisCon.Rows[iSelRow - 1].Cells[0].Selected = true;
        }

        private void btnDisConMoveDown_Click(object sender, EventArgs e)
        {
            if (gridDisCon.SelectedRows.Count == 0 && gridDisCon.SelectedCells.Count == 0) return;
            DataGridViewRow row = gridDisCon.SelectedRows.Count > 0 ? gridDisCon.SelectedRows[0] : gridDisCon.SelectedCells[0].OwningRow;
            int iSelRow = gridDisCon.Rows.IndexOf(row); if (iSelRow == gridDisCon.Rows.Count - 1) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in row.Cells) rowContent.Add(cell.Value);
            gridDisCon.Rows.RemoveAt(iSelRow); gridDisCon.Rows.Insert(iSelRow + 1, rowContent.ToArray());
            gridDisCon.ClearSelection(); gridDisCon.Rows[iSelRow - 1].Cells[0].Selected = true;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string errors;
            foreach (Settings.TextBoxInfo tbi in checkFields.Values)
            {
                bool failed = false;
                if ((tbi.textbox == txtTaxpayers || tbi.textbox == txtBeneficiaries) && string.IsNullOrEmpty(tbi.textbox?.Text))
                {
                    MessageBox.Show($"Please indicate a condition for {(tbi.textbox == txtTaxpayers ? "Taxpayers" : "Beneficiaries")}");
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
            foreach (DataGridViewRow row in gridDisCon.Rows)
                foreach (DataGridViewCell cell in row.Cells)
                {
                    bool failed = false;
                    bool isFormula = cell.ColumnIndex == colDisConFormula.Index || cell.ColumnIndex == colDisConFormulaFilter.Index || cell.ColumnIndex == colDisConTaxpayersBeneficiaries.Index;
                    bool isFilter = cell.ColumnIndex == colDisConFormulaFilter.Index || cell.ColumnIndex == colDisConTaxpayersBeneficiaries.Index;
                    bool startsWithComparer = cell.ColumnIndex == colDisConTaxpayersBeneficiaries.Index;
                    if ((cell.ColumnIndex == colDisConFormula.Index || cell.ColumnIndex == colDisConTaxpayersBeneficiaries.Index) && string.IsNullOrEmpty(cell.FormattedValue?.ToString()))
                    {
                        MessageBox.Show($"{(cell.ColumnIndex == colDisConFormula.Index ? "Variable/Formula" : "Payers/Receivers")} must not be empty.");
                        failed = true;
                    }
                    else if (isFormula && !Settings.IsValidFormula(cell.FormattedValue?.ToString(), isFilter, out errors, startsWithComparer))
                    {
                        MessageBox.Show($"Invalid formula '{cell.Value.ToString()}' at GridView '{gridDisCon.Name}'" + Environment.NewLine + errors + Environment.NewLine + "Please fix it before saving the settings.");
                        failed = true;
                    }
                    if (failed)
                    {
                        gridDisCon.CurrentCell = cell;
                        gridDisCon.BeginEdit(true);
                        return;
                    }
                }

            settings.userSettingsFiscal.pageTitle_Fiscal = txtPageTitleFiscal.Text;
            settings.userSettingsFiscal.tableTitle_RevenueExpenditure = txtTableTitleRevenueExpenditure.Text;
            settings.userSettingsFiscal.tableTitle_FiscalTaxpayersBeneficiaries = txtTableTitleFiscalTaxpayersBeneficiaries.Text;
            settings.userSettingsFiscal.tableTitle_DisaggregatedConcepts = txtTableTitleDisaggregatedConcepts.Text;
            settings.userSettingsFiscal.tableTitle_DisaggregatedConceptsUnits = txtTableTitleDisaggregatedConceptsUnits.Text;

            settings.userSettingsFiscal.taxpayers = FormulaEditor.Add_DATA_VAR(txtTaxpayers.Text);
            settings.userSettingsFiscal.beneficiaries = FormulaEditor.Add_DATA_VAR(txtBeneficiaries.Text);
            settings.userSettingsFiscal.calculationLevel = comboCalculationLevel.Text;

            SaveGridDisaggregatedConcepts();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnReset_Click(object sender, EventArgs e) // see comment SettingsDistributional.btnReset_Click
        {
            settings.userSettingsFiscal = new UserSettings();
            UpdateControls();
        }

        private void txtTaxpayersBeneficiaries_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, true, true));
        }

        private void gridDisCon_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0) return;
        }

        private void SettingsFiscal_Load(object sender, EventArgs e)
        {
            if (settings != null) checkFields.Clear();
        }
    }
}
