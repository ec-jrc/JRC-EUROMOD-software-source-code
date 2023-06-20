using EM_Common;
using EM_Statistics;
using System;
using System.Globalization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace EM_Statistics.InDepthAnalysis
{
    internal partial class SettingsInequalityAndPoverty : Form, ISettings
    {
        internal const string pageName_InequalityAndPoverty = "InequalityAndPoverty";
        internal const string tableName_Inequality = "Inequality";
        internal const string tableName_Progressivity = "Progressivity";
        internal const string tableName_AropRates = "AROPRates";
        internal const string tableName_AropGap = "AROPGap";
        internal const string tableName_AropRatesByType = "AROPRatesByType";
        internal const string tableName_AropGapByType = "AROPGapByType";

        private const string varName_Median_eq_DispY_Anchored = "Median_eq_DispY_Anchored";
        private const string varName_Median_eq_DispY_Floating = "Median_eq_DispY_Floating";
        private const string varName_OECDScale = "OECDScale";
        private const string varName_eq_ils_dispy = "eq_ils_dispy";
        private const string varName_Deciles_Floating_CutOff = "Deciles_Floating~dec~";
        private const string rowName_RedistributionIndex = "RedistributionIndex";
        private const string rowName_SocialWelfareIndex = "SocialWelfareIndex";
        private const string actionAndRowName_QuantileShareRatio = "QuantileShareRatio";
        private const string actionAndRowName_InterDecileRatio = "InterDecileRatio";
        private const string actionAndRowName_Atkinson = "Atkinson";
        private const string actionName_Progressivity_GrossIncome = "GrossIncome";
        private const string actionName_Progressivity_NetIncome = "NetIncome";
        private const string parName_Poor = "Poor";
        private const string colName_Difference = "Difference";

        internal class UserSettings
        {
            internal const string XMLTAG_PAGE_INEQUALITY_AND_POVERTY = "PageTitle_InequalityAndPoverty";
            internal const string XMLTAG_TABLE_TITLE_INEQUALITY = "TableTitle_Inequality";
            internal const string XMLTAG_TABLE_TITLE_PROGRESSIVITY = "TableTitle_Progressivity";
            internal const string XMLTAG_TABLE_TITLE_AROP_RATES = "TableTitle_AROPRates";
            internal const string XMLTAG_TABLE_TITLE_AROP_GAP = "TableTitle_AROPGap";
            internal const string XMLTAG_TABLE_TITLE_AROP_RATES_BY_TYPE = "TableTitle_AROPRatesByType";
            internal const string XMLTAG_TABLE_TITLE_AROP_GAP_BY_TYPE = "TableTitle_AROPGapByType";

            internal const string XMLTAG_TABLE31_QUANTILE_SHARE_RATIO_TOP = "Table31_QuantileShareRatioTop";
            internal const string XMLTAG_TABLE31_QUANTILE_SHARE_RATIO_BOTTOM = "Table31_QuantileShareRatioBottom";
            internal const string XMLTAG_TABLE31_INTER_DEC_RATIO_TOP = "Table31_InterDecRatioTop";
            internal const string XMLTAG_TABLE31_INTER_DEC_RATIO_BOTTOM = "Table31_InterDecRatioBottom";
            internal const string XMLTAG_TABLE31_ATKINSON_INEQUALITY_AVERSION = "Table31_AtkinsonInequalityAversion";
            internal const string XMLTAG_TABLE32_GROSS_INCOME = "Table32_GrossIncome";
            internal const string XMLTAG_TABLE32_NET_INCOME = "Table32_NetIncome";
            internal const string XMLTAG_TABLE334_POVERTY_LINE = "Table334_PovertyLine";
            internal const string XMLTAG_TABLE356_VARIABLE_BREAK_DOWN = "Table356_VariableBreakDown";
            internal const string XMLTAG_TABLE356_POVERTY_LINE = "Table356_PovertyLine";
            internal const string XMLTAG_TABLE346_AROP_CALCTYPE_FGT = "Table346_AROPCalcTypeFGT";
            internal const string XMLTAG_TABLE134_ORIGY = "Table3134_OrigY";
            internal const string XMLTAG_TABLE3134_ORIGY_ROW_TITLE = "Table3134_OrigY_RowTitle";
            internal const string XMLTAG_TABLE3134_INCOME_DEFINITION = "Table3134_IncomeDefinition";
            internal const string XMLTAG_TABLE356_TYPE_BREAK_DOWN = "Table356_HH_BreakDown";

            internal class PovertyLine
            {
                private const string XMLTAG_VALUE = "Value";
                private const string XMLTAG_IS_RELATIVE = "IsRelative";
                private const string XMLTAG_IS_ANCHORED = "IsAnchored";

                private const string COMBO_ITEM_RELATIVE_ANCHORED = "Relative anchored";
                private const string COMBO_ITEM_RELATIVE_FLOATING = "Relative floating";
                private const string COMBO_ITEM_ABSOLUTE = "Absolute (monthly)";

                internal readonly string id;
                internal readonly double value;
                internal readonly bool isRelative = true, isAnchored = true;
                internal PovertyLine(double _value, bool _isRelative, bool _isAnchored)
                {
                    value = _value; isRelative = _isRelative; isAnchored = _isAnchored;
                    id = Settings.MakeOutVarName($"PovertyLine_{value}");
                }
                internal static PovertyLine FromXml(XElement xElement, out string warnings)
                {
                    warnings = string.Empty; double _value = 60.0; bool _isRelative = true, _isAnchored = true;
                    foreach (XElement xe in xElement.Elements())
                    {
                        if (xe.Value == null) continue;
                        switch (Settings.GetXEleName(xe))
                        {
                            case XMLTAG_VALUE: if (double.TryParse(xe.Value, out double d)) _value = d; break;
                            case XMLTAG_IS_RELATIVE: if (bool.TryParse(xe.Value, out bool bp)) _isRelative = bp; break;
                            case XMLTAG_IS_ANCHORED: if (bool.TryParse(xe.Value, out bool ba)) _isAnchored = ba; break;
                            default: warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                        }
                    }
                    return new PovertyLine(_value, _isRelative, _isAnchored);
                }

                internal void ToXml(XmlWriter xmlWriter, bool byType)
                {
                    xmlWriter.WriteStartElement(byType ? XMLTAG_TABLE356_POVERTY_LINE : XMLTAG_TABLE334_POVERTY_LINE);
                    Settings.WriteElement(xmlWriter, XMLTAG_VALUE, value.ToString());
                    Settings.WriteElement(xmlWriter, XMLTAG_IS_RELATIVE, isRelative.ToString());
                    Settings.WriteElement(xmlWriter, XMLTAG_IS_ANCHORED, isAnchored.ToString());
                    xmlWriter.WriteEndElement();
                }

                internal static bool FromGrid(DataGridViewRow row, out PovertyLine povertyLine)
                {
                    povertyLine = null; if (row.Cells[0].Value == null || string.IsNullOrEmpty(row.Cells[0].Value.ToString())) return true;
                    string pl = row.Cells[0].Value.ToString(); pl.Trim(); if (pl.EndsWith("%")) pl = pl.Substring(0, pl.Length - 1); pl.TrimEnd();
                    if (!double.TryParse(pl, out double v) || v < 0) { MessageBox.Show($"{row.Cells[0].Value.ToString()} is not a valid poverty line."); row.DataGridView.CurrentCell = row.Cells[0]; row.DataGridView.BeginEdit(true); return false; }
                    povertyLine = new PovertyLine(v, row.Cells[1].Value.ToString() != COMBO_ITEM_ABSOLUTE,
                                                     row.Cells[1].Value.ToString() != COMBO_ITEM_RELATIVE_FLOATING);
                    return true;
                }

                internal void ToGrid(DataGridView grid)
                {
                    grid.Rows.Add(value, isRelative ? (isAnchored ? COMBO_ITEM_RELATIVE_ANCHORED : COMBO_ITEM_RELATIVE_FLOATING) : COMBO_ITEM_ABSOLUTE);
                }
            }

            internal class IncomeDefinition
            {
                private const string XMLTAG_ADD_TO_PREVIOUS = "addToPrevious";
                private const string XMLTAG_ROW_TITLE = "rowTitle";

                internal readonly string id;
                internal readonly string addToPrevious, rowTitle;
                internal IncomeDefinition(string _addToPrevious, string _rowTitle)
                {
                    addToPrevious = _addToPrevious; rowTitle = _rowTitle;
                    id = Settings.MakeOutVarName(rowTitle);
                }
                internal static IncomeDefinition FromXml(XElement xElement, out string warnings)
                {
                    warnings = string.Empty; string _addToPrevious = string.Empty, _rowTitle = string.Empty;
                    foreach (XElement xe in xElement.Elements())
                    {
                        if (xe.Value == null) continue;
                        switch (Settings.GetXEleName(xe))
                        {
                            case XMLTAG_ADD_TO_PREVIOUS: _addToPrevious = xe.Value; break;
                            case XMLTAG_ROW_TITLE: _rowTitle = xe.Value; break;
                            default: warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                        }
                    }
                    return new IncomeDefinition(_addToPrevious, _rowTitle);
                }

                internal void ToXml(XmlWriter xmlWriter)
                {
                    xmlWriter.WriteStartElement(XMLTAG_TABLE3134_INCOME_DEFINITION);
                    Settings.WriteElement(xmlWriter, XMLTAG_ADD_TO_PREVIOUS, addToPrevious);
                    Settings.WriteElement(xmlWriter, XMLTAG_ROW_TITLE, rowTitle);
                    xmlWriter.WriteEndElement();
                }
            }

            internal string pageTitle_InequalityAndPoverty = "3. Inequality and Poverty";
            internal string tableTitle_Inequality = "3.1. Inequality and redistributive effect of the tax-benefit system";
            internal string tableTitle_Progressivity = "3.2. Progressivity and redistributive effect";
            internal string tableTitle_AropRates = "3.3. At-risk-of-poverty rates for different poverty lines and definitions of income";
            internal string tableTitle_AropGap = "3.4. At-risk-of-poverty gaps for different poverty lines and definitions of income";
            internal string tableTitle_AropRatesByType = "3.5. At-risk-of-poverty rates";
            internal string tableTitle_AropGapByType = "3.6. At-risk-of-poverty gaps";

            internal int table31_QuantileShareRatioTop = 80;
            internal int table31_QuantileShareRatioBottom = 20;
            internal int table31_InterDecRatioTop = 5;
            internal int table31_InterDecRatioBottom = 1;
            internal double table31_AtkinsonInequalityAversion = 0.25;

            internal string table32_GrossIncome = $"{Settings.DATA_VAR(InDepthDefinitions.ILS_ORIGY)} + {Settings.DATA_VAR(InDepthDefinitions.ILS_BEN)}";
            internal string table32_NetIncome = $"{Settings.DATA_VAR(InDepthDefinitions.ILS_DISPY)}";

            internal List<PovertyLine> table334_PovertyLines = new List<PovertyLine>()
            {
                new PovertyLine(40, true, true),
                new PovertyLine(50, true, true),
                new PovertyLine(60, true, true),
            };
            
            internal List<Breakdowns.TypeBreakDown> table356_TypeBreakDowns = new List<Breakdowns.TypeBreakDown>() {
                new Breakdowns.TypeBreakDown(Breakdowns.STD_LABOUR_CAT),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_HH_CAT),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_GENDER_CAT),
                new Breakdowns.TypeBreakDown(Breakdowns.STD_AGE_CAT)
            };

            internal PovertyLine table356_PovertyLine = new PovertyLine(60.0, true, true);
            internal bool table346_AropCalcTypeFGT = true;

            internal string table3134_OrigY = InDepthDefinitions.ILS_ORIGY;
            internal string table3134_OrigY_RowTitle = "A = original income";
            internal List<IncomeDefinition> table3134_IncomeDefinitions = new List<IncomeDefinition>()
            {
                new IncomeDefinition($"- {Settings.DATA_VAR(InDepthDefinitions.ILS_TAX)} - {Settings.DATA_VAR(InDepthDefinitions.ILS_SICDY)}", "B = A - taxes and social insurance contributions (EQ_INC23)"),
                new IncomeDefinition($"+ {Settings.DATA_VAR(InDepthDefinitions.ILS_PEN)}", "C = B + pensions (EQ_INC22)"),
                new IncomeDefinition($"+ {Settings.DATA_VAR(InDepthDefinitions.ILS_BENMT)} + {Settings.DATA_VAR(InDepthDefinitions.ILS_BENNT)}", "D = C + other benefits (disposable income, EQ_INC20)")
            };

            internal void FromXml(XElement xElement, out string warnings)
            {
                warnings = string.Empty;
                foreach (XElement xe in xElement.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (Settings.GetXEleName(xe))
                    {
                        case XMLTAG_PAGE_INEQUALITY_AND_POVERTY: pageTitle_InequalityAndPoverty = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_INEQUALITY: tableTitle_Inequality = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_PROGRESSIVITY: tableTitle_Progressivity = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_AROP_RATES: tableTitle_AropRates = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_AROP_GAP: tableTitle_AropGap = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_AROP_RATES_BY_TYPE: tableTitle_AropRatesByType = xe.Value; break;
                        case XMLTAG_TABLE_TITLE_AROP_GAP_BY_TYPE: tableTitle_AropGapByType = xe.Value; break;

                        case XMLTAG_TABLE31_QUANTILE_SHARE_RATIO_TOP: if (int.TryParse(xe.Value, out int i)) table31_InterDecRatioTop = i; break;
                        case XMLTAG_TABLE31_QUANTILE_SHARE_RATIO_BOTTOM: if (int.TryParse(xe.Value, out i)) table31_QuantileShareRatioBottom = i; break;
                        case XMLTAG_TABLE31_INTER_DEC_RATIO_TOP: if (int.TryParse(xe.Value, out i)) table31_InterDecRatioTop = i; break;
                        case XMLTAG_TABLE31_INTER_DEC_RATIO_BOTTOM: if (int.TryParse(xe.Value, out i)) table31_InterDecRatioBottom = i; break;
                        case XMLTAG_TABLE31_ATKINSON_INEQUALITY_AVERSION: if (double.TryParse(xe.Value, out double d)) table31_AtkinsonInequalityAversion= d; break;

                        case XMLTAG_TABLE32_GROSS_INCOME: table32_GrossIncome = xe.Value; break;
                        case XMLTAG_TABLE32_NET_INCOME: table32_NetIncome = xe.Value; break;
                        case XMLTAG_TABLE334_POVERTY_LINE + "s":
                            table334_PovertyLines.Clear();
                            foreach (XElement xepl in xe.Elements())
                            {
                                table334_PovertyLines.Add(PovertyLine.FromXml(xepl, out string wpl));
                                warnings += wpl;
                            }
                            break;
                        case XMLTAG_TABLE356_POVERTY_LINE: table356_PovertyLine = PovertyLine.FromXml(xe, out string w); warnings += w; break;
                        case XMLTAG_TABLE346_AROP_CALCTYPE_FGT: if (bool.TryParse(xe.Value, out bool b)) table346_AropCalcTypeFGT = b; break;

                        case XMLTAG_TABLE134_ORIGY: table3134_OrigY = xe.Value; break;
                        case XMLTAG_TABLE3134_ORIGY_ROW_TITLE: table3134_OrigY_RowTitle = xe.Value; break;
                        case XMLTAG_TABLE3134_INCOME_DEFINITION + "s":
                            table3134_IncomeDefinitions.Clear();
                            foreach (XElement xeid in xe.Elements())
                            {
                                table3134_IncomeDefinitions.Add(IncomeDefinition.FromXml(xeid, out string wid));
                                warnings += wid;
                            }
                            break;
                        case XMLTAG_TABLE356_TYPE_BREAK_DOWN + "s":
                            table356_TypeBreakDowns.Clear();
                            foreach (XElement xetbd in xe.Elements())
                            {
                                table356_TypeBreakDowns.Add(Breakdowns.TypeBreakDown.FromXml(xetbd, out string wtbd));
                                warnings += wtbd;
                            }
                            break;
                        default: warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                    }
                }
            }

            internal void ToXml(XmlWriter xmlWriter)
            {
                Settings.WriteElement(xmlWriter, XMLTAG_PAGE_INEQUALITY_AND_POVERTY, pageTitle_InequalityAndPoverty);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_INEQUALITY, tableTitle_Inequality);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_PROGRESSIVITY, tableTitle_Progressivity);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_AROP_RATES, tableTitle_AropRates);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_AROP_GAP, tableTitle_AropGap);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_AROP_RATES_BY_TYPE, tableTitle_AropRatesByType);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE_TITLE_AROP_GAP_BY_TYPE, tableTitle_AropGapByType);

                Settings.WriteElement(xmlWriter, XMLTAG_TABLE31_QUANTILE_SHARE_RATIO_TOP, table31_QuantileShareRatioTop.ToString());
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE31_QUANTILE_SHARE_RATIO_BOTTOM, table31_QuantileShareRatioBottom.ToString());
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE31_INTER_DEC_RATIO_TOP, table31_InterDecRatioTop.ToString());
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE31_INTER_DEC_RATIO_BOTTOM, table31_InterDecRatioBottom.ToString());
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE31_ATKINSON_INEQUALITY_AVERSION, table31_AtkinsonInequalityAversion.ToString());

                Settings.WriteElement(xmlWriter, XMLTAG_TABLE32_GROSS_INCOME, table32_GrossIncome);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE32_NET_INCOME, table32_NetIncome);
                xmlWriter.WriteStartElement(XMLTAG_TABLE334_POVERTY_LINE + "s");
                foreach (PovertyLine pl in table334_PovertyLines) pl.ToXml(xmlWriter, false);
                xmlWriter.WriteEndElement();
                table356_PovertyLine.ToXml(xmlWriter, true);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE346_AROP_CALCTYPE_FGT, table346_AropCalcTypeFGT.ToString());

                Settings.WriteElement(xmlWriter, XMLTAG_TABLE134_ORIGY, table3134_OrigY);
                Settings.WriteElement(xmlWriter, XMLTAG_TABLE3134_ORIGY_ROW_TITLE, table3134_OrigY_RowTitle);
                xmlWriter.WriteStartElement(XMLTAG_TABLE3134_INCOME_DEFINITION + "s");
                foreach (IncomeDefinition id in table3134_IncomeDefinitions) id.ToXml(xmlWriter);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(XMLTAG_TABLE356_TYPE_BREAK_DOWN + "s");
                foreach (Breakdowns.TypeBreakDown tbd in table356_TypeBreakDowns) tbd.ToXml(xmlWriter, XMLTAG_TABLE356_TYPE_BREAK_DOWN);
                xmlWriter.WriteEndElement();
            }

            internal void WriteMetadata(TemplateApi templateApi, List<string> inactiveTablesAndPages)
            {
                if (!inactiveTablesAndPages.Contains(tableName_Inequality))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_InequalityAndPoverty, tableTitle_Inequality);
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Income quantile share ratio", $"S{table31_QuantileShareRatioTop}/S{table31_QuantileShareRatioBottom}");
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Inter-decile ratio", $"D{table31_InterDecRatioTop}/D{table31_InterDecRatioBottom}");
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Atkinson inequality aversion", table31_AtkinsonInequalityAversion.ToString());
                }
                if (!inactiveTablesAndPages.Contains(tableName_Progressivity))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_InequalityAndPoverty, tableTitle_Progressivity);
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Gross income", table32_GrossIncome);
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Net income", table32_NetIncome);
                }
                if (!inactiveTablesAndPages.Contains(tableName_AropRates) || !inactiveTablesAndPages.Contains(tableName_AropGap))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_InequalityAndPoverty, tableTitle_AropRates + "<br>" + tableTitle_AropGap);
                    foreach (PovertyLine pl in table334_PovertyLines) AddPovertyLineRow(pl);
                }
                if (!inactiveTablesAndPages.Contains(tableName_AropRatesByType) || !inactiveTablesAndPages.Contains(tableName_AropGapByType))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_InequalityAndPoverty, tableTitle_AropRatesByType + "<br>" + tableTitle_AropGapByType);
                    AddPovertyLineRow(table356_PovertyLine);
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Type breakdown(s)",
                                            string.Join("<br>", from tbd in table356_TypeBreakDowns select tbd.variable));
                }
                if (!inactiveTablesAndPages.Contains(tableName_Inequality) || !inactiveTablesAndPages.Contains(tableName_AropRates) || !inactiveTablesAndPages.Contains(tableName_AropGap))
                {
                    Settings.AddMetaDataHeaderRow(templateApi, pageName_InequalityAndPoverty, tableTitle_Inequality + "<br>" + tableTitle_AropRates + "<br>" + tableTitle_AropGap);
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, table3134_OrigY_RowTitle, table3134_OrigY);
                    foreach (IncomeDefinition idf in table3134_IncomeDefinitions) Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, idf.rowTitle, idf.addToPrevious);
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty, "Poverty gap calculation mode", table346_AropCalcTypeFGT ? "FGT(1)" : "Median");
                }

                void AddPovertyLineRow(PovertyLine pl)
                {
                    Settings.AddMetaDataRow(templateApi, pageName_InequalityAndPoverty,
                                            $"{(pl.isAnchored || !pl.isRelative ? "Anchored" : "Floating")} poverty line",
                                            pl.isRelative ? $"{pl.value.ToString(CultureInfo.InvariantCulture)} %" : pl.value.ToString("N"));
                }
            }
        }

        private Settings settings = null;
        internal Dictionary<string, Settings.TextBoxInfo> checkFields = new Dictionary<string, Settings.TextBoxInfo>();
        internal Dictionary<string, Settings.DataGridViewInfo> checkGridViews = new Dictionary<string, Settings.DataGridViewInfo>();


        internal SettingsInequalityAndPoverty()
        {
            InitializeComponent();
            InDepthAnalysis.SetShowHelp(this, helpProvider);
        }

        void ISettings.ModifyTemplate(TemplateApi templateApi, out List<Template.TemplateInfo.UserVariable> systemSpecificVars)
        {
            systemSpecificVars = null;
            if (settings == null || settings.inactiveTablesAndPages.Contains(pageName_InequalityAndPoverty)) return;
            ModifyTemplate_Titles(templateApi);
            ModifyTemplate_GenerateIncomeDefVars(templateApi, // generate variables (via actions) for the income definitions defined by the user
                out List<UserSettings.IncomeDefinition> incomeDefinitions); // definition for original income + the list of other income definitions
            ModifyTemplate_Table31(templateApi, incomeDefinitions);
            ModifyTemplate_Table32(templateApi);
            ModifyTemplate_Table33456(templateApi, incomeDefinitions);
        }

        private void ModifyTemplate_Titles(TemplateApi templateApi)
        {
            templateApi.ModifyPage(new Template.Page() { name = pageName_InequalityAndPoverty, title = settings.userSettingsInequalityAndPoverty.pageTitle_InequalityAndPoverty });
            if (!settings.inactiveTablesAndPages.Contains(tableName_Inequality)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_Inequality, title = settings.userSettingsInequalityAndPoverty.tableTitle_Inequality }, pageName_InequalityAndPoverty);
            if (!settings.inactiveTablesAndPages.Contains(tableName_Progressivity)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_Progressivity, title = settings.userSettingsInequalityAndPoverty.tableTitle_Progressivity }, pageName_InequalityAndPoverty);
            if (!settings.inactiveTablesAndPages.Contains(tableName_AropRates)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_AropRates, title = settings.userSettingsInequalityAndPoverty.tableTitle_AropRates }, pageName_InequalityAndPoverty);
            if (!settings.inactiveTablesAndPages.Contains(tableName_AropGap)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_AropGap, title = settings.userSettingsInequalityAndPoverty.tableTitle_AropGap }, pageName_InequalityAndPoverty);
            if (!settings.inactiveTablesAndPages.Contains(tableName_AropRatesByType)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_AropRatesByType, title = settings.userSettingsInequalityAndPoverty.tableTitle_AropRatesByType }, pageName_InequalityAndPoverty);
            if (!settings.inactiveTablesAndPages.Contains(tableName_AropGapByType)) templateApi.ModifyTable(new Template.Page.Table() { name = tableName_AropGapByType, title = settings.userSettingsInequalityAndPoverty.tableTitle_AropGapByType }, pageName_InequalityAndPoverty);
        }

        private void ModifyTemplate_GenerateIncomeDefVars(TemplateApi templateApi, out List<UserSettings.IncomeDefinition> incomeDefinitions)
        {
            incomeDefinitions = new List<UserSettings.IncomeDefinition>();
            if (settings.inactiveTablesAndPages.Contains(tableName_AropRates) && settings.inactiveTablesAndPages.Contains(tableName_AropGap) &&
                settings.inactiveTablesAndPages.Contains(tableName_Inequality)) return;

            string previousIncome = string.Empty;
            incomeDefinitions = new List<UserSettings.IncomeDefinition>() {
                new UserSettings.IncomeDefinition(settings.userSettingsInequalityAndPoverty.table3134_OrigY,
                                                  settings.userSettingsInequalityAndPoverty.table3134_OrigY_RowTitle) };
            incomeDefinitions.AddRange(settings.userSettingsInequalityAndPoverty.table3134_IncomeDefinitions);

            foreach (UserSettings.IncomeDefinition incomeDefinition in incomeDefinitions)
            {
                if (!Settings.HandleFormulaString(templateApi, incomeDefinition.addToPrevious,
                    out string formulaAddToPrevious, out List<Template.Parameter> parIncome)) continue;
                string formulaIncome = $"{previousIncome}{formulaAddToPrevious}";

                // generate the income variable ...
                string idIncome = Settings.MakeOutVarName(incomeDefinition.addToPrevious);
                if (!templateApi.ModifyPageActions(new Template.Action() {
                    outputVar = idIncome, calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                    formulaString = formulaIncome, parameters = parIncome },
                    pageName_InequalityAndPoverty, TemplateApi.ModifyMode.AddNew)) continue;
                previousIncome = Settings.DATA_VAR(idIncome);

                // ... equivalise it ...
                templateApi.ModifyPageActions(new Template.Action() {
                    outputVar = incomeDefinition.id, calculationType = HardDefinitions.CalculationType.CreateEquivalized,
                    parameters = new List<Template.Parameter>() {
                        new Template.Parameter() { variableName = idIncome },
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_EQUIVALENCESCALE, variableName = varName_OECDScale, _source = Template.Parameter.Source.BASELINE } } },
                    pageName_InequalityAndPoverty, TemplateApi.ModifyMode.AddNew);
            }
        }

        private void ModifyTemplate_Table31(TemplateApi templateApi, List<UserSettings.IncomeDefinition> incomeDefinitions)
        {
            string varNameGiniA = null, varNameGiniX = null;
            foreach (UserSettings.IncomeDefinition incomeDefinition in incomeDefinitions)
            {
                if (!GenerateGini(incomeDefinition, out string varNameGini)) continue;
                if (incomeDefinitions.IndexOf(incomeDefinition) == 0) varNameGiniA = varNameGini;
                if (incomeDefinitions.IndexOf(incomeDefinition) == incomeDefinitions.Count() - 1) varNameGiniX = varNameGini;
                if (!GenerateIncomeDefRow(incomeDefinition, varNameGini)) continue;
            }

            if (varNameGiniA != null && varNameGiniX != null)
                templateApi.ModifyCellAction_Row(new Template.Action() { formulaString = $"{Settings.SAVED_VAR(varNameGiniA)} - {Settings.SAVED_VAR(varNameGiniX)}" },
                    pageName_InequalityAndPoverty, tableName_Inequality, rowName_RedistributionIndex);
            if (varNameGiniX != null && incomeDefinitions.Count > 0)
            {
                string varNameMeanEqDispy = $"Mean_{incomeDefinitions.Last().id}";
                templateApi.ModifyPageActions(new Template.Action() { outputVar = varNameMeanEqDispy,
                    calculationType = HardDefinitions.CalculationType.CalculateWeightedAverage, formulaString = Settings.DATA_VAR(incomeDefinitions.Last().id) },
                    pageName_InequalityAndPoverty, TemplateApi.ModifyMode.AddNew);
                templateApi.ModifyCellAction_Row(new Template.Action() { formulaString = $"{Settings.SAVED_VAR(varNameMeanEqDispy)} * (1 - {Settings.SAVED_VAR(varNameGiniX)})" },
                    pageName_InequalityAndPoverty, tableName_Inequality, rowName_SocialWelfareIndex);
            }

            templateApi.ModifyTableActions(new Template.Action() { name = actionAndRowName_QuantileShareRatio, parameters = new List<Template.Parameter>() {
                    new Template.Parameter() { name = EM_TemplateCalculator.PAR_S8020_STOP, numericValue = settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioTop },
                    new Template.Parameter() { name = EM_TemplateCalculator.PAR_S8020_SBOTTOM, numericValue = settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioBottom }, } },
                pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.MergeReplace);
            templateApi.ModifyTableActions(new Template.Action() { name = actionAndRowName_InterDecileRatio, parameters = new List<Template.Parameter>() {
                    new Template.Parameter() { name = "DTop", variableName = $"{varName_Deciles_Floating_CutOff}{settings.userSettingsInequalityAndPoverty.table31_InterDecRatioTop}" },
                    new Template.Parameter() { name = "DBottom", variableName = $"{varName_Deciles_Floating_CutOff}{settings.userSettingsInequalityAndPoverty.table31_InterDecRatioBottom}" }, } },
                pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.MergeReplace);
            templateApi.ModifyTableActions(new Template.Action() { name = actionAndRowName_Atkinson, parameters = new List<Template.Parameter>() {
                    new Template.Parameter() { name = EM_TemplateCalculator.PAR_ATKINSON_INEQUALITY_AVERSION, numericValue = settings.userSettingsInequalityAndPoverty.table31_AtkinsonInequalityAversion }, } },
                pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.MergeReplace);
            templateApi.ModifyRows(new Template.Page.Table.Row() { name = actionAndRowName_QuantileShareRatio,
                title = $"Income quantile share ratio (S{settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioTop}/S{settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioBottom})" },
                pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.MergeReplace);
            templateApi.ModifyRows(new Template.Page.Table.Row() { name = actionAndRowName_InterDecileRatio,
                title = $"Inter-decile ratio (D{settings.userSettingsInequalityAndPoverty.table31_InterDecRatioTop}/D{settings.userSettingsInequalityAndPoverty.table31_InterDecRatioBottom})" },
                pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.MergeReplace);
            templateApi.ModifyRows(new Template.Page.Table.Row() { name = actionAndRowName_Atkinson,
                title = $"Atkinson inequality index (inequality aversion parameter = {settings.userSettingsInequalityAndPoverty.table31_AtkinsonInequalityAversion})" },
                pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.MergeReplace);

            bool GenerateGini(UserSettings.IncomeDefinition incomeDefinition, out string varNameGini)
            {
                varNameGini = $"Gini_{incomeDefinition.id}";
                return templateApi.ModifyPageActions(new Template.Action() { outputVar = varNameGini, calculationType = HardDefinitions.CalculationType.CalculateGini,
                    parameters = new List<Template.Parameter>()
                    {
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_GINI, variableName = incomeDefinition.id },
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_RECODENEGATIVES, boolValue = true },
                        new Template.Parameter() { name = EM_TemplateCalculator.PAR_GROUPING, variableName = DefVarName.IDHH }
                    } },
                pageName_InequalityAndPoverty, TemplateApi.ModifyMode.AddNew);
            }

            bool GenerateIncomeDefRow(UserSettings.IncomeDefinition incomeDefinition, string giniVarName)
            {
                string rowId = Settings.MakeId();
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowId, title = "Gini: " + incomeDefinition.rowTitle },
                    pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.AddNew,
                    TemplateApi.AddWhere.Before, rowName_RedistributionIndex)) return false;
                templateApi.ModifyCellAction_Row(new Template.Action() {
                    calculationType = HardDefinitions.CalculationType.CalculateArithmetic, formulaString = $"{Settings.SAVED_VAR(giniVarName)}" },
                    pageName_InequalityAndPoverty, tableName_Inequality, rowId);

                string cellId = Settings.MakeId();
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellId, colName = colName_Difference, rowName = rowId },
                    pageName_InequalityAndPoverty, tableName_Inequality, TemplateApi.ModifyMode.AddNew, true)) return false;
                return templateApi.ModifyCellAction_Cell(new Template.Action() { calculationType = HardDefinitions.CalculationType.CalculateArithmetic,
                    formulaString = $"{HardDefinitions.FormulaParameter.REF_COL}Reform{HardDefinitions.FormulaParameter.CLOSING_TOKEN} - " + 
                                    $"{HardDefinitions.FormulaParameter.BASE_COL}Baseline{HardDefinitions.FormulaParameter.CLOSING_TOKEN}" },
                                    pageName_InequalityAndPoverty, tableName_Inequality, cellId, TemplateApi.ModifyMode.AddNew, true);
            }
        }

        private void ModifyTemplate_Table32(TemplateApi templateApi)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_Progressivity)) return;

            if (!Settings.HandleFormulaString(templateApi, settings.userSettingsInequalityAndPoverty.table32_GrossIncome,
                out string formulaGross, out List<Template.Parameter> parGross)) return;
            if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_Progressivity_GrossIncome,
                formulaString = formulaGross, parameters = parGross },
                pageName_InequalityAndPoverty, tableName_Progressivity, TemplateApi.ModifyMode.MergeReplace)) return;

            if (!Settings.HandleFormulaString(templateApi, settings.userSettingsInequalityAndPoverty.table32_NetIncome,
                out string formulaNet, out List<Template.Parameter> parNet)) return;
            if (!templateApi.ModifyTableActions(new Template.Action() { name = actionName_Progressivity_NetIncome,
                formulaString = formulaNet, parameters = parNet },
                pageName_InequalityAndPoverty, tableName_Progressivity, TemplateApi.ModifyMode.MergeReplace)) return;

            templateApi.ModifyTable(new Template.Page.Table() { name = tableName_Progressivity,
                subtitle = $"({FormulaEditor.Remove_DATA_VAR(settings.userSettingsInequalityAndPoverty.table32_GrossIncome)}) &rArr; " +
                           $"({FormulaEditor.Remove_DATA_VAR(settings.userSettingsInequalityAndPoverty.table32_NetIncome)})" }, pageName_InequalityAndPoverty);
        }

        private void ModifyTemplate_Table33456(TemplateApi templateApi, List<UserSettings.IncomeDefinition> incomeDefinitions)
        {
            if (settings.inactiveTablesAndPages.Contains(tableName_AropRates) && settings.inactiveTablesAndPages.Contains(tableName_AropGap) &&
                settings.inactiveTablesAndPages.Contains(tableName_AropRatesByType) && settings.inactiveTablesAndPages.Contains(tableName_AropGapByType)) return;

            // tables 3.3. and 3.4. (rates, gap)
            if (!settings.inactiveTablesAndPages.Contains(tableName_AropRates) || !settings.inactiveTablesAndPages.Contains(tableName_AropGap))
            {
                foreach (UserSettings.PovertyLine povertyLine in settings.userSettingsInequalityAndPoverty.table334_PovertyLines)
                {
                    GeneratePovertyLineVar(povertyLine); // generate (saved) variables (via actions) for the poverty lines
                    foreach (UserSettings.IncomeDefinition incomeDefinition in incomeDefinitions) // generate (flag) variables for being poor for each income definition and poverty line
                        GeneratePoorFlag(povertyLine, incomeDefinition.id);

                    foreach (string tableName in new List<string>() { tableName_AropRates, tableName_AropGap }) 
                    {
                        GeneratePovertyLineRow(povertyLine, tableName); // add the row for the povert line
                        foreach (UserSettings.IncomeDefinition incomeDefinition in incomeDefinitions) // add the rows for the income definitions
                            GenerateIncomeDefRow(povertyLine, tableName, incomeDefinition);
                    }
                }
            }

            // tables 3.5. and 3.6. (rates, gap by hh-type)
            if (!settings.inactiveTablesAndPages.Contains(tableName_AropRatesByType) || !settings.inactiveTablesAndPages.Contains(tableName_AropGapByType))
            {
                UserSettings.PovertyLine povertyLine = settings.userSettingsInequalityAndPoverty.table356_PovertyLine;
                GeneratePovertyLineVar(povertyLine); // generate (saved) variable (via action) for the poverty line
                GeneratePoorFlag(povertyLine, varName_eq_ils_dispy); // generate (flag) variable for being poor for equivalised disposable income and the poverty line

                foreach (string tableTypeName in new List<string>() { tableName_AropRatesByType, tableName_AropGapByType }) // first copy (not yet changed) original table ...
                    for (int breakDownNo = 0; breakDownNo < settings.userSettingsInequalityAndPoverty.table356_TypeBreakDowns.Count; ++breakDownNo)
                        if (breakDownNo > 0) templateApi.CopyTable(pageName_InequalityAndPoverty, tableTypeName, GetTableName(tableTypeName, breakDownNo),
                            TemplateApi.AddWhere.After, $"{tableTypeName}{(breakDownNo == 1 ? string.Empty : $"{breakDownNo - 1}")}");
                
                foreach (string tableTypeName in new List<string>() { tableName_AropRatesByType, tableName_AropGapByType }) // then modify original and copied tables ...
                {
                    for (int breakDownNo = 0; breakDownNo < settings.userSettingsInequalityAndPoverty.table356_TypeBreakDowns.Count; ++breakDownNo)
                    {
                        Breakdowns.TypeBreakDown breakDown = settings.userSettingsInequalityAndPoverty.table356_TypeBreakDowns[breakDownNo];
                        if (!Breakdowns.STD_CATS.ContainsKey(breakDown.name) || Breakdowns.STD_CATS[breakDown.name].isValued)    // Do not do this for default catgories with custom rows
                        { 
                            templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable()
                            {
                                name = breakDown.variable,
                                readVar = breakDown.variable,
                                defaultValue = double.NaN
                            },
                                TemplateApi.ModifyMode.AddOrReplace);
                        }

                        GeneratePovertyLineRow(povertyLine, GetTableName(tableTypeName, breakDownNo), true); // add the row for the povert line
                        GenerateIncomeDefRow(povertyLine, GetTableName(tableTypeName, breakDownNo), null, breakDown); // add the row for the equivalised disposable income

                        templateApi.ModifyTable(new Template.Page.Table() { name = GetTableName(tableTypeName, breakDownNo),
                            subtitle = breakDown.variable == Breakdowns.STD_HH_CAT ? "by different types of households" : $"by values of {breakDown.variable}" },
                            pageName_InequalityAndPoverty);
                    }
                }

                string GetTableName(string tableTypeName, int breakDownNo) { return $"{tableTypeName}{(breakDownNo == 0 ? string.Empty : $"{breakDownNo}")}"; }
            }

            void GeneratePovertyLineVar(UserSettings.PovertyLine povertyLine)
            {
                templateApi.ModifyPageActions(new Template.Action() {
                    calculationType = HardDefinitions.CalculationType.CalculateArithmetic, outputVar = povertyLine.id,
                    formulaString = povertyLine.isRelative ? $"{Settings.SAVED_VAR("Median")} * {Settings.TEMP_VAR("Percentage")} / 100"
                                                               : povertyLine.value.ToString(CultureInfo.InvariantCulture),
                    parameters = povertyLine.isRelative
                        ? new List<Template.Parameter>() {
                            new Template.Parameter() { name = "Median", variableName = povertyLine.isAnchored ? varName_Median_eq_DispY_Anchored : varName_Median_eq_DispY_Floating },
                            new Template.Parameter() { name = "Percentage", numericValue = povertyLine.value } }
                        : new List<Template.Parameter>() },
                    pageName_InequalityAndPoverty, TemplateApi.ModifyMode.AddNew);
            }

            void GeneratePovertyLineRow(UserSettings.PovertyLine povertyLine, string tableName, bool sepAfter = false)
            {
                string rowId = Settings.MakeId();
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowId, hasSeparatorBefore = true, hasSeparatorAfter = sepAfter, stringFormat = "#,0.00",
                    title = $"Poverty line{(povertyLine.isRelative ? $"<br>{povertyLine.value}% of median {(povertyLine.isAnchored ? "baseline " : string.Empty)} eq.disp.income" : string.Empty)}" },
                    pageName_InequalityAndPoverty, tableName, TemplateApi.ModifyMode.AddNew)) return;
                templateApi.ModifyCellAction_Row(new Template.Action() {
                    calculationType = HardDefinitions.CalculationType.CalculateArithmetic, formulaString = $"{Settings.SAVED_VAR(povertyLine.id)}*12" },
                    pageName_InequalityAndPoverty, tableName, rowId);
                string cellId = Settings.MakeId();
                if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellId, rowName = rowId, colName = colName_Difference },
                    pageName_InequalityAndPoverty, tableName, TemplateApi.ModifyMode.AddNew, true)) return;
                templateApi.ModifyCellAction_Cell(new Template.Action() { calculationType = HardDefinitions.CalculationType.CalculateArithmetic,
                    formulaString = $"{HardDefinitions.FormulaParameter.REF_COL}Reform{HardDefinitions.FormulaParameter.CLOSING_TOKEN} - " +
                                    $"{HardDefinitions.FormulaParameter.BASE_COL}Baseline{HardDefinitions.FormulaParameter.CLOSING_TOKEN}" },
                    pageName_InequalityAndPoverty, tableName, cellId, TemplateApi.ModifyMode.AddNew, true);
            }

            void GeneratePoorFlag(UserSettings.PovertyLine povertyLine, string incomeVar)
            {
                if (!templateApi.ModifyPageActions(new Template.Action() { name = GetPoorFlagVar(povertyLine, incomeVar),
                    outputVar = GetPoorFlagVar(povertyLine, incomeVar), calculationType = HardDefinitions.CalculationType.CreateFlag },
                    pageName_InequalityAndPoverty, TemplateApi.ModifyMode.AddNew)) return;
                templateApi.ModifyFilter_PageAction(new Template.Filter() {
                    formulaString = $"{Settings.DATA_VAR(incomeVar)} <= {Settings.SAVED_VAR(povertyLine.id)}" },
                    pageName_InequalityAndPoverty, GetPoorFlagVar(povertyLine, incomeVar));
            }

            void GenerateIncomeDefRow(UserSettings.PovertyLine povertyLine, string tableName,
                                      UserSettings.IncomeDefinition incomeDefinition, Breakdowns.TypeBreakDown breakDown = null)
            {
                bool calcGap = tableName.ToLower().Contains("gap"), byType = breakDown != null;

                List<string> rowIdByType = new List<string>();
                if (byType && breakDown != null && Breakdowns.STD_CATS.ContainsKey(breakDown.name) && !Breakdowns.STD_CATS[breakDown.name].isValued)
                {
                    foreach (Breakdowns.BreakDownItem br in Breakdowns.STD_CATS[breakDown.name].items)
                    {
                        rowIdByType.Add(Settings.MakeId());
                        if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowIdByType.Last(), title = br.title,
                            cellAction = new Template.Action() { filter = new Template.Filter() { formulaString = br.filter }}},
                            pageName_InequalityAndPoverty, tableName, TemplateApi.ModifyMode.AddNew)) return;
                    }
                }
                else
                {
                    rowIdByType.Add(Settings.MakeId());
                    if (byType && !templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowIdByType.Last(), title = string.Empty,
                        forEachValueOf = byType ? breakDown.variable : null, forEachValueMaxCount = Settings.FOREACH_VALUE_OF_MAX_COUNT,
                        forEachValueDescriptions = byType ? breakDown.GetValueDictionaryFromDefinitions() : null },
                        pageName_InequalityAndPoverty, tableName, TemplateApi.ModifyMode.AddNew)) return;
                }
                string rowId = Settings.MakeId();
                if (!templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowId,
                    title = incomeDefinition == null ? "All" : incomeDefinition.rowTitle, hasSeparatorBefore = byType },
                    pageName_InequalityAndPoverty, tableName, TemplateApi.ModifyMode.AddNew,
                    byType ? TemplateApi.AddWhere.After : TemplateApi.AddWhere.Appropriate, byType ? rowIdByType.Last() : null)) return;

                List<Template.Parameter> parameters = new List<Template.Parameter>();
                if (calcGap)
                {
                    parameters.Add(new Template.Parameter() { name = EM_TemplateCalculator.PAR_INCOME,
                        variableName = incomeDefinition == null ? varName_eq_ils_dispy : incomeDefinition.id });
                    parameters.Add(new Template.Parameter() { name = EM_TemplateCalculator.PAR_POVERTYLINE, variableName = povertyLine.id });
                    parameters.Add(new Template.Parameter() { name = EM_TemplateCalculator.PAR_USE_SWITCH_APPROACH,
                        boolValue = settings.userSettingsInequalityAndPoverty.table346_AropCalcTypeFGT });
                }
                else parameters.Add(new Template.Parameter() { name = parName_Poor,
                    variableName = GetPoorFlagVar(povertyLine, incomeDefinition == null ? varName_eq_ils_dispy : incomeDefinition.id) });

                List<string> allIDs = new List<string>() { rowId };
                if (byType) allIDs.AddRange(rowIdByType.ToList());
                foreach (string rId in allIDs)
                { 
                    templateApi.ModifyCellAction_Row(new Template.Action() { parameters = parameters }, pageName_InequalityAndPoverty, tableName, rId);

                    string cellId = Settings.MakeId();
                    if (!templateApi.ModifyCells(new Template.Page.Table.Cell() { name = cellId,
                        colName = colName_Difference, rowName = rId, stringFormat = "0.0pp" },
                        pageName_InequalityAndPoverty, tableName, TemplateApi.ModifyMode.AddNew, true)) continue;
                    templateApi.ModifyCellAction_Cell(new Template.Action() { calculationType = HardDefinitions.CalculationType.CalculateArithmetic,
                        formulaString = $"({HardDefinitions.FormulaParameter.REF_COL}Reform{HardDefinitions.FormulaParameter.CLOSING_TOKEN} - " + 
                                        $"{HardDefinitions.FormulaParameter.BASE_COL}Baseline{HardDefinitions.FormulaParameter.CLOSING_TOKEN}) * 100" },
                                        pageName_InequalityAndPoverty, tableName, cellId, TemplateApi.ModifyMode.AddNew, true);
                }
            }

            string GetPoorFlagVar(UserSettings.PovertyLine povertyLine, string incomeVar) { return povertyLine.id + incomeVar; }
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
            txtPageTitleInequalityAndPoverty.Text = settings.userSettingsInequalityAndPoverty.pageTitle_InequalityAndPoverty;
            txtTableTitle31.Text = settings.userSettingsInequalityAndPoverty.tableTitle_Inequality;
            txtTableTitle32.Text = settings.userSettingsInequalityAndPoverty.tableTitle_Progressivity;
            txtTableTitle33.Text = settings.userSettingsInequalityAndPoverty.tableTitle_AropRates;
            txtTableTitle34.Text = settings.userSettingsInequalityAndPoverty.tableTitle_AropGap;
            txtTableTitle35.Text = settings.userSettingsInequalityAndPoverty.tableTitle_AropRatesByType;
            txtTableTitle36.Text = settings.userSettingsInequalityAndPoverty.tableTitle_AropGapByType;

            num31QuantileShareRatioTop.Value = settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioTop;
            num31QuantileShareRatioBottom.Value = settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioBottom;
            num31InterDecRatioTop.Value = settings.userSettingsInequalityAndPoverty.table31_InterDecRatioTop;
            num31InterDecRatioBottom.Value = settings.userSettingsInequalityAndPoverty.table31_InterDecRatioBottom;
            txt31AtkinsonInequalityAversion.Text = settings.userSettingsInequalityAndPoverty.table31_AtkinsonInequalityAversion.ToString();

            txt32GrossIncome.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsInequalityAndPoverty.table32_GrossIncome);
            txt32NetIncome.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsInequalityAndPoverty.table32_NetIncome);

            grid334PovLines.Rows.Clear();
            foreach (UserSettings.PovertyLine pl in settings.userSettingsInequalityAndPoverty.table334_PovertyLines) pl.ToGrid(grid334PovLines);
            gridBreakDown.Rows.Clear();
            foreach (Breakdowns.TypeBreakDown tbd in settings.userSettingsInequalityAndPoverty.table356_TypeBreakDowns)
            {
                bool isDef = Breakdowns.STD_CATS.ContainsKey(tbd.name);
                int r = gridBreakDown.Rows.Add(isDef ? tbd.name : tbd.variable, (isDef ? Breakdowns.STD_CATS[tbd.name] : tbd).GetValueStringFromDefinitions());
                gridBreakDown.Rows[r].Tag = (isDef ? Breakdowns.STD_CATS[tbd.name] : tbd).GetValueListFromDefinitions();
            }
            grid356PovLine.Rows.Clear(); settings.userSettingsInequalityAndPoverty.table356_PovertyLine.ToGrid(grid356PovLine);
            txt3134OrigY.Text = FormulaEditor.Remove_DATA_VAR(settings.userSettingsInequalityAndPoverty.table3134_OrigY);
            txt3134_OrigY_RowTitle.Text = settings.userSettingsInequalityAndPoverty.table3134_OrigY_RowTitle;
            grid3134IncDef.Rows.Clear();
            foreach (UserSettings.IncomeDefinition incomeDefinition in settings.userSettingsInequalityAndPoverty.table3134_IncomeDefinitions)
                grid3134IncDef.Rows.Add(FormulaEditor.Remove_DATA_VAR(incomeDefinition.addToPrevious), incomeDefinition.rowTitle);
            radio356AropCalcTypeFGT.Checked = settings.userSettingsInequalityAndPoverty.table346_AropCalcTypeFGT;
            radio356AropCalcTypeMedian.Checked = !settings.userSettingsInequalityAndPoverty.table346_AropCalcTypeFGT;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            foreach (Settings.TextBoxInfo tbi in checkFields.Values)
            {
                bool failed = false;
                if (tbi.textbox == txt32GrossIncome || tbi.textbox == txt32NetIncome || tbi.textbox == txt3134OrigY)
                {
                    if (string.IsNullOrEmpty(tbi.textbox.Text?.ToString()))
                    {
                        MessageBox.Show("Field must not be empty.");
                        failed = true;
                    } 
                    else if (!Settings.IsValidFormula(tbi.textbox.Text, false))
                    {
                        MessageBox.Show("Value must be a valid formula.");
                        failed = true;
                    }
                }
                else if (tbi.textbox == txt31AtkinsonInequalityAversion)
                {
                    if (string.IsNullOrEmpty(tbi.textbox.Text?.ToString()))
                    {
                        MessageBox.Show("Field must not be empty.");
                        failed = true;
                    }
                    else if (!double.TryParse(tbi.textbox.Text, out _))
                    {
                        MessageBox.Show("Value must be a double.");
                        failed = true;
                    }
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
                        if ((dgvi.dgv == grid356PovLine || dgvi.dgv == grid334PovLines) && cell.ColumnIndex == 0)
                        {
                            if (string.IsNullOrEmpty(cell.FormattedValue?.ToString()) ||
                               !double.TryParse(cell.FormattedValue.ToString(), out double pl) || pl < 0)
                            {
                                MessageBox.Show("This is not a valid poverty line.");
                                failed = true;
                            }
                        }
                        else if (dgvi.dgv == grid3134IncDef && cell.ColumnIndex == colAddToPrevious.Index)
                        {
                            if (string.IsNullOrEmpty(cell.FormattedValue?.ToString())) 
                            { 
                                MessageBox.Show("Add to previous must not be empty.");
                                failed = true;
                            }
                            else if (!Settings.IsValidFormula(cell.FormattedValue?.ToString().TrimStart(new char[] { '+' }), false))
                            {
                                MessageBox.Show("Invalid formula.");
                                failed = true;
                            }
                        }
                        if (failed)
                        {
                            dgvi.dgv.CurrentCell = cell;
                            dgvi.dgv.BeginEdit(true);
                            return;
                        }
                    }
                }
            }

            settings.userSettingsInequalityAndPoverty.pageTitle_InequalityAndPoverty = txtPageTitleInequalityAndPoverty?.Text;
            settings.userSettingsInequalityAndPoverty.tableTitle_Inequality = txtTableTitle31?.Text;
            settings.userSettingsInequalityAndPoverty.tableTitle_Progressivity = txtTableTitle32?.Text;
            settings.userSettingsInequalityAndPoverty.tableTitle_AropRates = txtTableTitle33?.Text;
            settings.userSettingsInequalityAndPoverty.tableTitle_AropGap = txtTableTitle34?.Text;
            settings.userSettingsInequalityAndPoverty.tableTitle_AropRatesByType = txtTableTitle35?.Text;
            settings.userSettingsInequalityAndPoverty.tableTitle_AropGapByType = txtTableTitle36?.Text;

            settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioTop = Convert.ToInt32(num31QuantileShareRatioTop.Value);
            settings.userSettingsInequalityAndPoverty.table31_QuantileShareRatioBottom = Convert.ToInt32(num31QuantileShareRatioBottom.Value);
            settings.userSettingsInequalityAndPoverty.table31_InterDecRatioTop = Convert.ToInt32(num31InterDecRatioTop.Value);
            settings.userSettingsInequalityAndPoverty.table31_InterDecRatioBottom = Convert.ToInt32(num31InterDecRatioBottom.Value);
            if (!string.IsNullOrEmpty(txt31AtkinsonInequalityAversion.Text) && double.TryParse(txt31AtkinsonInequalityAversion.Text, out double d))
                settings.userSettingsInequalityAndPoverty.table31_AtkinsonInequalityAversion = d;

            settings.userSettingsInequalityAndPoverty.table32_GrossIncome = FormulaEditor.Add_DATA_VAR(txt32GrossIncome?.Text);
            settings.userSettingsInequalityAndPoverty.table32_NetIncome = FormulaEditor.Add_DATA_VAR(txt32NetIncome?.Text);

            settings.userSettingsInequalityAndPoverty.table334_PovertyLines.Clear();
            foreach (DataGridViewRow row in grid334PovLines.Rows)
                if (!UserSettings.PovertyLine.FromGrid(row, out UserSettings.PovertyLine pl334)) return;
                else if (pl334 != null) settings.userSettingsInequalityAndPoverty.table334_PovertyLines.Add(pl334);

            settings.userSettingsInequalityAndPoverty.table356_TypeBreakDowns.Clear();
            foreach (DataGridViewRow row in gridBreakDown.Rows)
            {
                if (string.IsNullOrEmpty(row.Cells[colBreakDownVariable.Index].Value?.ToString())) continue;
                string v = row.Cells[colBreakDownVariable.Index].Value.ToString();
                Breakdowns.TypeBreakDown tb = new Breakdowns.TypeBreakDown(row.Cells[colBreakDownVariable.Index].Value.ToString());
                if (!Breakdowns.STD_CATS.ContainsKey(v))    // if this is not a default category
                {
                    List<Tuple<double, string>> values = row.Tag as List<Tuple<double, string>>;
                    foreach (var val in values) tb.items.Add(new Breakdowns.BreakDownItem() { value = val.Item1, title = val.Item2 });
                }
                settings.userSettingsInequalityAndPoverty.table356_TypeBreakDowns.Add(tb);
            }

            if (!UserSettings.PovertyLine.FromGrid(grid356PovLine.Rows[0], out UserSettings.PovertyLine pl356)) return;
            else settings.userSettingsInequalityAndPoverty.table356_PovertyLine = pl356 ?? new UserSettings.PovertyLine(60, true, true);
            settings.userSettingsInequalityAndPoverty.table3134_OrigY = FormulaEditor.Add_DATA_VAR(txt3134OrigY?.Text);
            settings.userSettingsInequalityAndPoverty.table3134_OrigY_RowTitle = txt3134_OrigY_RowTitle?.Text;
            settings.userSettingsInequalityAndPoverty.table3134_IncomeDefinitions.Clear();
            foreach (DataGridViewRow row in grid3134IncDef.Rows)
                settings.userSettingsInequalityAndPoverty.table3134_IncomeDefinitions.Add(new UserSettings.IncomeDefinition(
                    FormulaEditor.Add_DATA_VAR(row.Cells[colAddToPrevious.Index].Value?.ToString()), row.Cells[colRowTitle.Index].Value?.ToString()));

            settings.userSettingsInequalityAndPoverty.table346_AropCalcTypeFGT = radio356AropCalcTypeFGT.Checked;

            DialogResult = DialogResult.OK; Close();
        }

        private void btnGridIncAddRow_Click(object sender, EventArgs e)
        {
            int i = grid3134IncDef.Rows.Add(string.Empty, string.Empty);
            grid3134IncDef.CurrentCell = grid3134IncDef.Rows[i].Cells[colAddToPrevious.Index]; grid3134IncDef.BeginEdit(true);
        }

        private void btnGridPovAddRow_Click(object sender, EventArgs e)
        {
            new UserSettings.PovertyLine(0, true, true).ToGrid(grid334PovLines);
            grid334PovLines.CurrentCell = grid334PovLines.Rows[grid334PovLines.Rows.Count-1].Cells[0]; grid334PovLines.BeginEdit(true);
        }

        private void btnBreakDownAddRow_Click(object sender, EventArgs e)
        {
            int i = gridBreakDown.Rows.Add(string.Empty, string.Empty);
            gridBreakDown.Rows[i].Tag = new List<Tuple<double, string>>();
            gridBreakDown.CurrentCell = gridBreakDown.Rows[i].Cells[0]; gridBreakDown.BeginEdit(true);
        }

        private DataGridView GetGrid(object senderButton)
        {
            if ((senderButton as Button).Name.Contains("Inc")) return grid3134IncDef;
            if ((senderButton as Button).Name.Contains("Pov")) return grid334PovLines;
            if ((senderButton as Button).Name.Contains("BreakDown")) return gridBreakDown;
            return null;
        }

        private void btnGridDelRow_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetGrid(sender);
            if (dgv.SelectedRows.Count == 0 && dgv.SelectedCells.Count == 0) return;
            DataGridViewRow selectedRow = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0] : dgv.SelectedCells[0].OwningRow;
            if (!(sender as Button).Name.Contains("Inc") && // for breakdown there is a check in btnOk_Click
                !string.IsNullOrEmpty(selectedRow.Cells[0].Value?.ToString())) // empty rows can be always deleted
            {
                int notEmpty = 0; foreach (DataGridViewRow row in dgv.Rows) if (!string.IsNullOrEmpty(row.Cells[0].Value?.ToString())) ++notEmpty;
                if (notEmpty <= 1) { MessageBox.Show("At least one non-empty definition is required."); return; }
            }
            dgv.Rows.Remove(selectedRow);

        }

        private void btnGridMoveUp_Click(object sender, EventArgs e)
        {
            DataGridView dgv = GetGrid(sender);
            if (dgv.SelectedRows.Count == 0 && dgv.SelectedCells.Count == 0) return;
            DataGridViewRow row = dgv.SelectedRows.Count > 0 ? dgv.SelectedRows[0] : dgv.SelectedCells[0].OwningRow;
            int iSelRow = dgv.Rows.IndexOf(row); if (iSelRow == 0) return;
            List<object> rowContent = new List<object>(); foreach (DataGridViewCell cell in row.Cells) rowContent.Add(cell.Value);
            dgv.Rows.Insert(iSelRow - 1, rowContent.ToArray()); dgv.Rows.RemoveAt(iSelRow + 1);
            dgv.ClearSelection(); dgv.Rows[iSelRow - 1].Cells[0].Selected = true;
        }

        private void btnGridMoveDown_Click(object sender, EventArgs e)
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
            CategoryDescriptionsForm form = Breakdowns.STD_CATS.ContainsKey(v)?
                new CategoryDescriptionsForm(gridBreakDown.Rows[e.RowIndex].Tag as List<Tuple<double, string>>, true, Breakdowns.STD_CATS[v].isValued) :
                new CategoryDescriptionsForm(gridBreakDown.Rows[e.RowIndex].Tag as List<Tuple<double, string>>);
            if (form.ShowDialog() != DialogResult.OK) return;
            gridBreakDown.Rows[e.RowIndex].Tag = form.GetDescriptionsList();
            gridBreakDown.Rows[e.RowIndex].Cells[colBreakDownValueDesc.Index].Value = CategoryDescriptionsForm.DicToString(form.GetDescriptionsDictionary());
        }

        private void gridPovLine_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 && // grids have only one column
                !checkGridViews.ContainsKey(dgv.Name)) checkGridViews.Add(dgv.Name, new Settings.DataGridViewInfo(dgv, false));
        }

        private void btnReset_Click(object sender, EventArgs e) // see comment SettingsDistributional.btnReset_Click
        {
            settings.userSettingsInequalityAndPoverty = new UserSettings();
            UpdateControls();
        }

        private void txt31AtkinsonInequalityAversion_Validating(object sender, CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, true, true));
        }

        private void textBox_FormulaValidating(object sender, CancelEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (!checkFields.ContainsKey(textBox.Name)) checkFields.Add(textBox.Name, new Settings.TextBoxInfo(textBox, true, true));
        }

        private void gridCell_FormulaValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            DataGridView dgv = sender as DataGridView;
            if (e.RowIndex >= 0 && e.ColumnIndex == 0 && // grids have only one column
                !checkGridViews.ContainsKey(dgv.Name)) checkGridViews.Add(dgv.Name, new Settings.DataGridViewInfo(dgv, false));
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
                    cell.OwningRow.Tag = Breakdowns.STD_CATS[v].GetValueListFromDefinitions();
                }
            }

        }
    }
}
