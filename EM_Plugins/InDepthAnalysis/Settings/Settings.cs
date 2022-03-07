using EM_Common_Win;
using EM_Statistics;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace InDepthAnalysis
{
    internal class Settings
    {
        internal const int FOREACH_VALUE_OF_MAX_COUNT = 150;
        internal const string VERSION = "0.1.5";
        internal const string VERSION_FOR_ASSEMBLY = VERSION + ".0";

        internal List<BaselineReformPackage> baselineReformPackages = new List<BaselineReformPackage>();
        internal List<string> inactiveTablesAndPages = new List<string>();

        internal SettingsFiscal.UserSettings userSettingsFiscal = new SettingsFiscal.UserSettings();
        internal SettingsDistributional.UserSettings userSettingsDistributional = new SettingsDistributional.UserSettings();
        internal SettingsInequalityAndPoverty.UserSettings userSettingsInequalityAndPoverty = new SettingsInequalityAndPoverty.UserSettings();

        internal string pathEuromodFiles = UISessionInfo.GetEuromodFilesFolder();
        internal string pathBaselineFiles = UISessionInfo.GetOutputFolder();
        internal string pathReformFiles = UISessionInfo.GetOutputFolder();
        internal string pathMergedDataset = UISessionInfo.GetOutputFolder();
        internal bool saveMergedDataset = false;
        internal bool compareWithBaseline = true;

        private const string pageName_Metadata = "Metadata";
        internal const string pageName_ParameterSettings = "ParameterSettings";
        private const string rowName_ParameterSettings_CompareWith = "Compare with";
        private const string rowName_ParameterSettings_DateAndTime = "Date and time";
        private const string rowName_ParameterSettings_User = "User";

        internal bool GetFilePackages(out List<FilePackageContent> filePackages, out List<string> errors)
        {
            filePackages = new List<FilePackageContent>(); errors = new List<string>();
            foreach (BaselineReformPackage baselineReformPackage in baselineReformPackages)
            {
                if (baselineReformPackage.GetFilePackageContent(out FilePackageContent fpc, out List<string> e)) filePackages.Add(fpc);
                errors.AddRange(e);
            }
            if (!filePackages.Any()) errors.Add($"No valid baseline-reform packages available.");
            return filePackages.Any();
        }

        internal void ResetPathEuromodFilesFolder()
        {
            pathEuromodFiles = UISessionInfo.GetEuromodFilesFolder(); ;
        }

        internal void ResetPathMergedDataset()
        {
            pathMergedDataset = UISessionInfo.GetOutputFolder();
        }

        internal void UpdateBaselineReformInfo(out List<string> errors)
        {
            errors = new List<string>();
            foreach (BaselineReformPackage baselineReformPackage in baselineReformPackages)
            {
                baselineReformPackage.UpdateSystemInfo(this, out List<string> e);
                errors.AddRange(e);
            }
        }

        private const string XMLTAG_ROOT = "InDepthAnalysis_Settings";
        private const string XMLTAG_INACTIVE_ELEMENT = "InactiveElement";
        private const string XMLTAG_FISCAL = "Fiscal";
        private const string XMLTAG_DISTRIBUTIONAL = "Distributional";
        private const string XMLTAG_INEQUALITY_AND_POVERTY = "InequalityAndPoverty";
        private const string XMLTAG_COMPARE_WITH_BASELINE = "CompareWithBaseline";

        internal static Settings FromXml(string xmlFilePath, List<BaselineReformPackage> brps)
        {
            Settings settings = new Settings() { baselineReformPackages = brps ?? new List<BaselineReformPackage>() }; string warnings = string.Empty;
            if (!File.Exists(xmlFilePath)) return settings;
            using (XmlReader xmlReader = XmlReader.Create(xmlFilePath))
            {
                while (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != XMLTAG_ROOT)
                    if (!xmlReader.Read()) throw new Exception($"Tag {XMLTAG_ROOT} not found.");
                foreach (XElement xe in (XElement.ReadFrom(xmlReader) as XElement).Elements())
                {
                    if (xe.Value == null) continue;
                    switch (GetXEleName(xe))
                    {
                        case XMLTAG_INACTIVE_ELEMENT + "s":
                            foreach (XElement xeSub in GetSubElements(xe)) settings.inactiveTablesAndPages.Add(xeSub.Value);
                            break;
                        case XMLTAG_FISCAL: settings.userSettingsFiscal.FromXml(xe, out string wf); warnings += wf; break;
                        case XMLTAG_DISTRIBUTIONAL: settings.userSettingsDistributional.FromXml(xe, out string wd); warnings += wd; break;
                        case XMLTAG_INEQUALITY_AND_POVERTY: settings.userSettingsInequalityAndPoverty.FromXml(xe, out string wi); warnings += wi; break;
                        case XMLTAG_COMPARE_WITH_BASELINE: if (bool.TryParse(xe.Value, out bool c)) settings.compareWithBaseline = c; break;
                        default: warnings += $"Unknown setting {GetXEleName(xe)} is ignored." + Environment.NewLine; break;
                    }
                }

                List<XElement> GetSubElements(XElement xe)
                {
                    List<XElement> subElements = new List<XElement>();
                    foreach (XElement xeSub in xe.Elements())
                    {
                        if (xeSub.Value == null) continue;
                        if (GetXEleName(xeSub) != GetXEleName(xe).Substring(0, GetXEleName(xe).Length - 1)) // e.g. xe.Name=ReformFiles, xeSub.Name=ReformFile
                            warnings += $"Unknown setting {GetXEleName(xeSub)} is ignored." + Environment.NewLine;
                        else subElements.Add(xeSub);
                    }
                    return subElements;
                }
            }
            if (!string.IsNullOrEmpty(warnings)) MessageBox.Show($"Warnings wrt reading Settings from {Path.GetFileName(xmlFilePath)}:{Environment.NewLine}{warnings}");
            return settings;
        }

        internal void ToXml(string xmlFilePath)
        {
            using (XmlWriter xmlWriter = XmlWriter.Create(xmlFilePath))
            {
                xmlWriter.WriteStartElement(XMLTAG_ROOT);

                xmlWriter.WriteStartElement(XMLTAG_INACTIVE_ELEMENT + "s");
                foreach (string inactivePage in inactiveTablesAndPages)
                    WriteElement(xmlWriter, XMLTAG_INACTIVE_ELEMENT, inactivePage);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement(XMLTAG_FISCAL); userSettingsFiscal.ToXml(xmlWriter); xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement(XMLTAG_DISTRIBUTIONAL); userSettingsDistributional.ToXml(xmlWriter); xmlWriter.WriteEndElement();
                xmlWriter.WriteStartElement(XMLTAG_INEQUALITY_AND_POVERTY); userSettingsInequalityAndPoverty.ToXml(xmlWriter); xmlWriter.WriteEndElement();

                WriteElement(xmlWriter, XMLTAG_COMPARE_WITH_BASELINE, compareWithBaseline.ToString(), false);

                xmlWriter.WriteEndElement(); // root
            }
        }

        internal void WriteMetadata(TemplateApi templateApi)
        {
            templateApi.ModifyCellAction_Row(new Template.Action() { formulaString = $"{(compareWithBaseline ? "Baseline" : "Previous reform")}" },
                pageName_Metadata, pageName_Metadata, rowName_ParameterSettings_CompareWith, TemplateApi.ModifyMode.AddOrReplace);
            templateApi.ModifyCellAction_Row(new Template.Action() { formulaString = DateTime.Now.ToString() },
                pageName_Metadata, pageName_Metadata, rowName_ParameterSettings_DateAndTime, TemplateApi.ModifyMode.AddOrReplace);
            templateApi.ModifyCellAction_Row(new Template.Action() { formulaString = Environment.UserName },
                pageName_Metadata, pageName_Metadata, rowName_ParameterSettings_User, TemplateApi.ModifyMode.AddOrReplace);

            if (!inactiveTablesAndPages.Contains(SettingsFiscal.pageName_Fiscal)) userSettingsFiscal.WriteMetadata(templateApi, inactiveTablesAndPages);
            else AddMetaDataHeaderRow(templateApi, SettingsFiscal.pageName_Fiscal, "not displayed");
            if (!inactiveTablesAndPages.Contains(SettingsDistributional.pageName_Distributional)) userSettingsDistributional.WriteMetadata(templateApi, inactiveTablesAndPages);
            else AddMetaDataHeaderRow(templateApi, SettingsDistributional.pageName_Distributional, "not displayed");
            if (!inactiveTablesAndPages.Contains(SettingsInequalityAndPoverty.pageName_InequalityAndPoverty)) userSettingsInequalityAndPoverty.WriteMetadata(templateApi, inactiveTablesAndPages);
            else AddMetaDataHeaderRow(templateApi, SettingsInequalityAndPoverty.pageName_InequalityAndPoverty, "not displayed");
        }

        internal static void AddMetaDataHeaderRow(TemplateApi templateApi, string tableName, string title) { AddMetaDataRow(templateApi, tableName, title, string.Empty, true, true); }
        internal static void AddMetaDataRow(TemplateApi templateApi, string tableName, string title, string content, bool separaterAfter = false, bool isheaderRow = false)
        {
            string rowId = MakeId();
            if (templateApi.ModifyRows(new Template.Page.Table.Row() { name = rowId, title = title,
                strong = isheaderRow, backgroundColour = isheaderRow ? "#ccffcc" : string.Empty,
                hasSeparatorAfter = isheaderRow | separaterAfter, hasSeparatorBefore = isheaderRow },
                pageName_ParameterSettings, tableName, TemplateApi.ModifyMode.AddNew))
                templateApi.ModifyCellAction_Row(new Template.Action() { formulaString = FormulaEditor.Remove_DATA_VAR(content) }, pageName_ParameterSettings, tableName, rowId);
        }

        internal static void WriteElement(XmlWriter xmlWriter, string tag, string content, bool cData = true)
        {
            xmlWriter.WriteStartElement(tag);
            xmlWriter.WriteRaw(cData ? XmlHelpers.CDATA(content) : content);
            xmlWriter.WriteEndElement();
        }
        internal static string GetXEleName(XElement xe) { return xe.Name == null ? string.Empty : xe.Name.ToString(); }

        internal const string BASELINE_MARKER = "_base";

        /// <summary>
        /// analysis StringFormula of Global/Page/Table-Action (i.e. not CellAction) or Filter:
        /// identifies DATA_VAR[@xxx]-components and generates optional variables for each of them
        /// in addition, if a variable is to be taken from baseline, it generates parameters (with reform=false) and adapts the formula respectively
        /// </summary>
        /// <param name="origFormula">StringFormula of Action or Filter</param>
        /// <param name="handledFormula">differs from origFormula only, if there are variables to be taken from baseline</param>
        /// <param name="parameters">for variables to be taken from baseline (reform is set to false)</param>
        /// <returns>true for success, false for failure</returns>
        internal static bool HandleFormulaString(TemplateApi templateApi, string origFormula,
                                                 out string handledFormula, out List<Template.Parameter> parameters)
        {
            string DATA_VAR_MARKER = HardDefinitions.FormulaParameter.DATA_VAR + "@";
            handledFormula = origFormula; parameters = new List<Template.Parameter>();

            if (!handledFormula.Contains(HardDefinitions.FormulaParameter.DATA_VAR))
            {
                if (handledFormula.IndexOfAny(new char[] { '<', '>', '=', '&', '|', '+', '-', '*', '/', '!', '(', ')' }) >= 0) return true;
                if (Double.TryParse(handledFormula, out double d)) { handledFormula = d.ToString("F"); return true; }
                handledFormula = DATA_VAR(handledFormula); // probably a variable-name
            }

            List<string> dataVars = new List<string>();
            for (int i = handledFormula.IndexOf(DATA_VAR_MARKER, 0), j; i >= 0; i = handledFormula.IndexOf(DATA_VAR_MARKER, j))
            {
                j = handledFormula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, i);
                if (j < 0) { templateApi.AddError($"Invalid formula:{Environment.NewLine}{handledFormula}"); return false; }
                string varName = handledFormula.Substring(i + DATA_VAR_MARKER.Length, j - i - DATA_VAR_MARKER.Length);
                if (!dataVars.Contains(varName)) dataVars.Add(varName);
            }

            foreach (string dataVar in dataVars)
            {
                string varName = dataVar, varId = MakeReadVarName(templateApi, varName);
                if (dataVar.EndsWith(BASELINE_MARKER))
                {
                    string parId = varName;
                    handledFormula = handledFormula.Replace($"{DATA_VAR(varName)}", $"{DATA_VAR(parId)}");
                    varName = varName.Substring(0, varName.Length - BASELINE_MARKER.Length);
                    parameters.Add(new Template.Parameter() { name = parId, variableName = varId, _source = Template.Parameter.Source.BASELINE });
                }
                else handledFormula = handledFormula.Replace($"{DATA_VAR(varName)}", $"{DATA_VAR(varId)}");
                templateApi.ModifyOptionalVariables(new Template.TemplateInfo.OptionalVariable() { name = varId, readVar = varName, defaultValue = double.NaN },
                                                    TemplateApi.ModifyMode.AddOrKeep);
            }
            return true;
        }

        internal static string MakeReadVarName(TemplateApi templateApi, string readVar)
        {
            // first try to use an already defined optional or required variable ...
            string varName = (from v in templateApi.GetOptinalVariables() where v.readVar.ToLower() == readVar.ToLower() select v.name).FirstOrDefault() ??
                             (from v in templateApi.GetRequiredVariables() where v.readVar.ToLower() == readVar.ToLower() select v.name).FirstOrDefault();
            if (varName == null) // ... if not yet defined, define one ...
            {
                varName = readVar;
                while (((from v in templateApi.GetOptinalVariables() where v.name.ToLower() == varName.ToLower() select v.name).FirstOrDefault() ??
                       (from v in templateApi.GetRequiredVariables() where v.name.ToLower() == varName.ToLower() select v.name).FirstOrDefault()) != null)
                    varName += "_"; // ... unlikely, but avoid overwriting an existing variable
            }
            return varName;
        }

        internal static string MakeOutVarName(string varIdent)
        {
            string valid = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ", varName = string.Empty;
            foreach (char c in FormulaEditor.Remove_DATA_VAR(varIdent ?? string.Empty).Trim()) varName += valid.Contains(c) ? c : '_';
            return $"{(varName.Length > 20 ? varName.Substring(0, 20) : varName)}_{MakeId()}";
        }

        internal static string MakeId() { return Guid.NewGuid().ToString().Replace("-", ""); }

        internal static bool IsValidFormula(string formula, bool isFilter, bool startsWithComparer = false)
        {
            if (string.IsNullOrEmpty(formula)) return true;

            if (startsWithComparer)
            {
                List<string> cmps = new List<string>() { "!=", "==", "<=", ">=", "=", "<", ">" };
                formula = formula.TrimStart(); bool ok = false;
                foreach (string cmp in cmps) if (formula.StartsWith(cmp)) { ok = true; break; }
                if (!ok) { MessageBox.Show($"Invalid condition: must start with one of {string.Join("  ", cmps)}"); return false; }
                formula = $"0.0{formula}"; // from !=whatsoever to 0.0!=whatsoever
            }

            formula = FormulaEditor.Add_DATA_VAR(formula, true);
            if (TemplateApi.IsValidFormula(formula, isFilter, out string error)) return true;
            MessageBox.Show(error); return false;
        }

        internal static string DATA_VAR(string dataVar) { return $"{HardDefinitions.FormulaParameter.DATA_VAR}@{dataVar}{HardDefinitions.FormulaParameter.CLOSING_TOKEN}"; }
        internal static string SAVED_VAR(string savedVar) { return $"{HardDefinitions.FormulaParameter.SAVED_VAR}@{savedVar}{HardDefinitions.FormulaParameter.CLOSING_TOKEN}"; }
        internal static string TEMP_VAR(string tempVar) { return $"{HardDefinitions.FormulaParameter.TEMP_VAR}@{tempVar}{HardDefinitions.FormulaParameter.CLOSING_TOKEN}"; }
        internal static string USR_VAR(string userVar) { return $"{HardDefinitions.FormulaParameter.USR_VAR}@{userVar}{HardDefinitions.FormulaParameter.CLOSING_TOKEN}"; }
    }
}
