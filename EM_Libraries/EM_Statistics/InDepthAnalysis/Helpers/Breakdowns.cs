using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace EM_Statistics.InDepthAnalysis
{
    internal class Breakdowns
    {
        internal class BreakDownItem
        {
            internal string title;
            internal string filter;
            internal double value = double.NaN;
        }

        internal const string VALUE_PLACEHOLDER = "[value~Numeric]";
        internal const string rowName_breakDown = "BreakDownRow";

        internal const string STD_DISPY_DECILES = "Disposable_Income_Deciles";
        internal static List<BreakDownItem> STD_DISPY_DECILES_DEFINITIONS = new List<BreakDownItem>()
        {
            new BreakDownItem() { title = "Decile 1", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 1", value = 1 },
            new BreakDownItem() { title = "Decile 2", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 2", value = 2 },
            new BreakDownItem() { title = "Decile 3", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 3", value = 3 },
            new BreakDownItem() { title = "Decile 4", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 4", value = 4 },
            new BreakDownItem() { title = "Decile 5", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 5", value = 5 },
            new BreakDownItem() { title = "Decile 6", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 6", value = 6 },
            new BreakDownItem() { title = "Decile 7", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 7", value = 7 },
            new BreakDownItem() { title = "Decile 8", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 8", value = 8 },
            new BreakDownItem() { title = "Decile 9", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 9", value = 9 },
            new BreakDownItem() { title = "Decile 10", filter = $"{Settings.DATA_VAR("deciles_eqDispy")} == 10", value = 10 }
        };

        internal const string STD_HH_CAT = "Standard_HH_Categories";
        internal static List<BreakDownItem> STD_HH_CAT_DEFINITIONS = new List<BreakDownItem>()
        {
            new BreakDownItem() { title = "One adult < 65, no children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 1 && {Settings.DATA_VAR("NumberOfElderly")} == 0", value = 0},
            new BreakDownItem() { title = "  - Female adult",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 1 && {Settings.DATA_VAR("NumberOfAdultFemales")} == 1 && {Settings.DATA_VAR("NumberOfElderly")} == 0" },
            new BreakDownItem() { title = "  - Male adult",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 1 && {Settings.DATA_VAR("NumberOfAdultMales")} == 1 && {Settings.DATA_VAR("NumberOfElderly")} == 0" },
            new BreakDownItem() { title = "One adult >= 65, no children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 0 && {Settings.DATA_VAR("NumberOfElderly")} == 1", value = 1 },
            new BreakDownItem() { title = "  - Female adult",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 0 && {Settings.DATA_VAR("NumberOfElderlyFemales")} == 1 && {Settings.DATA_VAR("NumberOfElderly")} == 1" },
            new BreakDownItem() { title = "  - Male adult",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 0 && {Settings.DATA_VAR("NumberOfElderlyMales")} == 1 && {Settings.DATA_VAR("NumberOfElderly")} == 1" },
            new BreakDownItem() { title = "One adult with children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} > 0 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 1", value = 2 },
            new BreakDownItem() { title = "  - Female adult",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} > 0 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 1 && {Settings.DATA_VAR("NumberOfAdultFemales")} + {Settings.DATA_VAR("NumberOfElderlyFemales")} == 1" },
            new BreakDownItem() { title = "  - Male adult",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} > 0 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 1 && {Settings.DATA_VAR("NumberOfAdultMales")} + {Settings.DATA_VAR("NumberOfElderlyMales")} == 1" },
            new BreakDownItem() { title = "Two adults < 65, no children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} == 2 && {Settings.DATA_VAR("NumberOfElderly")} == 0", value = 3 },
            new BreakDownItem() { title = "Two adults, at least one >= 65, no children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 2 && {Settings.DATA_VAR("NumberOfElderly")} > 0", value = 4 },
            new BreakDownItem() { title = "Two adults with one child",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 1 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 2", value = 5 },
            new BreakDownItem() { title = "Two adults with two children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 2 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 2", value = 6 },
            new BreakDownItem() { title = "Two adults with three or more children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} >= 3 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} == 2", value = 7 },
            new BreakDownItem() { title = "Three or more adults, no children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} == 0 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} >= 3", value = 8 },
            new BreakDownItem() { title = "Three or more adults with children",
                filter = $"{Settings.DATA_VAR("NumberOfChildren")} > 0 && {Settings.DATA_VAR("NumberOfAdults")} + {Settings.DATA_VAR("NumberOfElderly")} >= 3", value = 9 }
        };

        internal const string STD_LABOUR_CAT = "Standard_Labour_Categories";
        internal static List<BreakDownItem> STD_LABOUR_CAT_DEFINITIONS = new List<BreakDownItem>()
        {
            new BreakDownItem() { title = "Pre-school", filter = $"{Settings.DATA_VAR("FilterStatusPreschool")} > 0", value = 0 },
            new BreakDownItem() { title = "Farmer", filter = $"{Settings.DATA_VAR("FilterStatusFarmer")} > 0", value = 1 },
            new BreakDownItem() { title = "Employer or self-employed", filter = $"{Settings.DATA_VAR("FilterStatusSelfEmployed")} > 0", value = 2 },
            new BreakDownItem() { title = "Employee", filter = $"{Settings.DATA_VAR("FilterStatusEmployee")} > 0", value = 3 },
            new BreakDownItem() { title = "Pensioner", filter = $"{Settings.DATA_VAR("FilterStatusPensioner")} > 0", value = 4 },
            new BreakDownItem() { title = "Unemployed", filter = $"{Settings.DATA_VAR("FilterStatusUnemployed")} > 0", value = 5 },
            new BreakDownItem() { title = "Student", filter = $"{Settings.DATA_VAR("FilterStatusStudent")} > 0", value = 6 },
            new BreakDownItem() { title = "Inactive", filter = $"{Settings.DATA_VAR("FilterStatusInactive")} > 0", value = 7 },
            new BreakDownItem() { title = "Sick or Disabled", filter = $"{Settings.DATA_VAR("FilterStatusDisabled")} > 0", value = 8 },
            new BreakDownItem() { title = "Other", filter = $"{Settings.DATA_VAR("FilterStatusOther")} > 0", value = 9 }
        };

        internal const string STD_AGE_CAT = "Standard_Age_Categories";
        internal static List<BreakDownItem> STD_AGE_CAT_DEFINITIONS = new List<BreakDownItem>()
        {
            new BreakDownItem() { title = "0 - 14", filter = $"{Settings.DATA_VAR("Filter0_14")} > 0", value = 0 },
            new BreakDownItem() { title = "15 - 24", filter = $"{Settings.DATA_VAR("Filter15_24")} > 0", value = 1 },
            new BreakDownItem() { title = "25 - 49", filter = $"{Settings.DATA_VAR("Filter25_49")} > 0", value = 2 },
            new BreakDownItem() { title = "50 - 64", filter = $"{Settings.DATA_VAR("Filter50_64")} > 0", value = 3 },
            new BreakDownItem() { title = "65 - 79", filter = $"{Settings.DATA_VAR("Filter65_79")} > 0", value = 4 },
            new BreakDownItem() { title = "80+", filter = $"{Settings.DATA_VAR("Filter80plus")} > 0", value = 5 }
        };

        internal const string STD_GENDER_CAT = "Standard_Gender_Categories";
        internal static List<BreakDownItem> STD_GENDER_CAT_DEFINITIONS = new List<BreakDownItem>()
        {
            new BreakDownItem() { title = "Female", filter = $"{Settings.DATA_VAR("FilterFemale")} > 0", value = 0 },
            new BreakDownItem() { title = "Male", filter = $"{Settings.DATA_VAR("FilterMale")} > 0", value = 1 }
        };

        internal static readonly Dictionary<string, TypeBreakDown> STD_CATS = new Dictionary<string, TypeBreakDown>() { 
            // this is only in distributional
            { STD_DISPY_DECILES, new TypeBreakDown(STD_DISPY_DECILES, "ils_dispy" , STD_DISPY_DECILES_DEFINITIONS, true, "Deciles", true, 10) }, 
            // these are for both distributional and inequality & poverty
            { STD_HH_CAT, new TypeBreakDown(STD_HH_CAT, "hh_type", STD_HH_CAT_DEFINITIONS, false, "HH Type") },
            { STD_LABOUR_CAT, new TypeBreakDown(STD_LABOUR_CAT, "les", STD_LABOUR_CAT_DEFINITIONS, true, "Labour Status") },
            { STD_GENDER_CAT, new TypeBreakDown(STD_GENDER_CAT, "dgn", STD_GENDER_CAT_DEFINITIONS, true, "Gender") },
            { STD_AGE_CAT, new TypeBreakDown(STD_AGE_CAT, "age_group", STD_AGE_CAT_DEFINITIONS, false, "Age group") }
        };

        internal class TypeBreakDown
        {
            private const string XMLTAG_VALUE_DESCRIPTION = "ValueDescription"; // this one is only for backwards compatibility
            internal const string XMLTAG_ROW_TITLE = "RowTitle";
            internal const string XMLTAG_QUANTILES = "Quantiles";
            private const string XMLTAG_NAME = "Name";
            private const string XMLTAG_VARIABLE = "Variable";
            internal const string XMLTAG_EQUIVALISED = "Equivalised";
            private const string XMLTAG_ISVALUED = "IsValued";
            private const string XMLTAG_ITEM = "Item";
            private const string XMLTAG_VALUE = "Value";
            private const string XMLTAG_TEXT = "Text";
            private const string XMLTAG_FILTER = "Filter";
            internal List<BreakDownItem> items = new List<BreakDownItem>();
            internal bool isValued = true;
            internal string name = string.Empty;
            internal string variable = string.Empty;
            internal string rowTitle = string.Empty;
            internal int quantiles = -1;
            internal bool equivalised = true;

            internal TypeBreakDown(string _name = "", string _variable = "", List<BreakDownItem> _items = null,  bool _isValued = true, string _title = "", bool _equivalised = false, int _quantiles = -1) 
            {
                name = _name;
                if (_items == null && Breakdowns.STD_CATS.ContainsKey(_name))
                {
                    variable = Breakdowns.STD_CATS[_name].variable;
                    isValued = Breakdowns.STD_CATS[_name].isValued;
                    items = Breakdowns.STD_CATS[_name].items;
                    quantiles = Breakdowns.STD_CATS[_name].quantiles;
                    equivalised = Breakdowns.STD_CATS[_name].equivalised;
                    rowTitle = Breakdowns.STD_CATS[_name].rowTitle;
                }
                else
                {
                    variable = string.IsNullOrEmpty(_variable) ? _name : _variable;
                    isValued = _isValued;
                    items = _items ?? new List<BreakDownItem>();
                    rowTitle = _title;
                    equivalised = _equivalised;
                    quantiles = _quantiles;
                }
            }

            internal static TypeBreakDown FromXml(XElement xElement, out string warnings)
            {
                warnings = string.Empty; TypeBreakDown breakDown = new TypeBreakDown();
                foreach (XElement xe in xElement.Elements())
                {
                    if (xe.Value == null) continue;
                    switch (Settings.GetXEleName(xe))
                    {
                        case XMLTAG_ROW_TITLE: breakDown.rowTitle = xe.Value; break;
                        case XMLTAG_NAME: breakDown.name = xe.Value; break;
                        case XMLTAG_VARIABLE: breakDown.variable = xe.Value; break;
                        case XMLTAG_QUANTILES: if (int.TryParse(xe.Value, out int q)) breakDown.quantiles = q; break;
                        case XMLTAG_EQUIVALISED: if (bool.TryParse(xe.Value, out bool e)) breakDown.equivalised = e; break;
                        case XMLTAG_ISVALUED: if (bool.TryParse(xe.Value, out bool isV)) breakDown.isValued = isV; break;
                        case XMLTAG_VALUE_DESCRIPTION + "s":    // backwards compatibility
                        case XMLTAG_ITEM + "s":
                            foreach (XElement xeVd in xe.Elements())
                            {
                                if (xeVd.Value == null) continue;
                                double _value = double.NaN; string _title = string.Empty; string _filter = string.Empty;
                                foreach (XElement xeValDesc in xeVd.Elements())
                                {
                                    switch (Settings.GetXEleName(xeValDesc))
                                    {
                                        case XMLTAG_VALUE: if (double.TryParse(xeValDesc.Value, out double d)) _value = d; break;
                                        case XMLTAG_TEXT: _title = xeValDesc.Value; break;
                                        case XMLTAG_FILTER: _filter = xeValDesc.Value; break;
                                        default: warnings += $"Unknown setting {Settings.GetXEleName(xeValDesc)} is ignored." + Environment.NewLine; break;
                                    }
                                }
                                breakDown.items.Add(new BreakDownItem() { title = _title, value = _value, filter = _filter });
                            }
                            break;
                        default: warnings += $"Unknown setting {Settings.GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                    }
                }
                return breakDown;
            }

            internal void ToXml(XmlWriter xmlWriter, string xmlTag)
            {
                xmlWriter.WriteStartElement(xmlTag);
                Settings.WriteElement(xmlWriter, XMLTAG_ROW_TITLE, rowTitle);
                Settings.WriteElement(xmlWriter, XMLTAG_NAME, name);
                Settings.WriteElement(xmlWriter, XMLTAG_VARIABLE, variable);
                Settings.WriteElement(xmlWriter, XMLTAG_EQUIVALISED, equivalised.ToString());
                Settings.WriteElement(xmlWriter, XMLTAG_QUANTILES, quantiles.ToString());
                Settings.WriteElement(xmlWriter, XMLTAG_ISVALUED, isValued.ToString());
                xmlWriter.WriteStartElement(XMLTAG_ITEM + "s");
                foreach (BreakDownItem bi in items)
                {
                    xmlWriter.WriteStartElement(XMLTAG_ITEM);
                    Settings.WriteElement(xmlWriter, XMLTAG_VALUE, bi.value.ToString());
                    Settings.WriteElement(xmlWriter, XMLTAG_TEXT, bi.title);
                    Settings.WriteElement(xmlWriter, XMLTAG_FILTER, bi.filter);
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            internal List<Tuple<double, string>> GetValueListFromDefinitions()
            {
                return items.Select(x => new Tuple<double, string>(x.value, x.title)).ToList();
            }

            internal string GetValueStringFromDefinitions()
            {
                return string.Join(";", items.Select(x => (isValued ? x.value.ToString() + "=" : "") + x.title));
            }

            internal Dictionary<double, string> GetValueDictionaryFromDefinitions()
            {
                Dictionary<double, string> dic = null;
                if (isValued)
                {
                    dic = new Dictionary<double, string>();
                    items.ForEach(x => { if (!dic.ContainsKey(x.value)) dic.Add(x.value, x.title); });
                }
                return dic;
            }

            internal string GetByText(bool forButton = false)
            {
                if (forButton)
                {
                    string byTextShort = rowTitle.Replace(VALUE_PLACEHOLDER, string.Empty).Trim();
                    if (!string.IsNullOrEmpty(byTextShort)) return $"by {byTextShort}";
                }
                string byTxt = quantiles <= 0 ? "values" : (quantiles == 10 ? "deciles" : (quantiles == 5 ? "quintiles" : $"{quantiles} quantiles"));
                return $"by {byTxt} of {(equivalised? "equivalised ": "")}{variable}";
            }

            internal static void CreateStdHHCatVar(TemplateApi templateApi, string pageName)
            {
                List<string> vars = new List<string>();
                foreach (var hhCatDefs in STD_HH_CAT_DEFINITIONS)
                {
                    string var = $"{STD_HH_CAT}{hhCatDefs.value}"; string actionId = Settings.MakeId();
                    if (!templateApi.ModifyPageActions(new Template.Action()
                    {
                        name = actionId,
                        calculationType = HardDefinitions.CalculationType.CreateFlag,
                        outputVar = var
                    },
                        pageName)) continue;
                    if (templateApi.ModifyFilter_PageAction(new Template.Filter() { formulaString = hhCatDefs.filter },
                        pageName, actionId)) vars.Add($"{Settings.DATA_VAR(var)} * {hhCatDefs.value}");
                }
                CreateVar(string.Join("+", vars));

                void CreateVar(string formula)
                {
                    templateApi.ModifyPageActions(new Template.Action()
                    {
                        calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                        formulaString = formula,
                        outputVar = STD_HH_CAT
                    }, pageName);
                }
            }




        }
    } 
}

