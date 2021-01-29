using System;
using System.Collections.Generic;
using System.Linq;
using EM_Common;

namespace EM_Statistics
{
    public class Old_EMS_Classes
    {
        /// <summary>
        /// Class holds a file package that consists of pathes to EUROMOD output files of base and alternative scenarios.
        /// Such a file package is selected by the user and handed by the viewer together with the template (see EMS_InterfaceViewer.GetViewerTablesContent).
        /// </summary>
/*        public class EMS_FilePackageContent
        {
            public string PathBase { get { return pathBase; } set { if (value != null) pathBase = value; } }
            private string pathBase = string.Empty;

            public List<string> PathsAlt { get { return pathsAlt; } set { if (value != null) pathsAlt = value; } }
            private List<string> pathsAlt = new List<string>();

            /// <summary> Note that there is no need to set Key manually. </summary>
            public string Key { get { return key; } }
            private readonly string key = Guid.NewGuid().ToString(); // automatic generation of key!

            public List<EMS_UserInput> UserInputs { get { return userInputs; } set { if (value != null) userInputs = value; } }
            private List<EMS_UserInput> userInputs = new List<EMS_UserInput>();

        }
        */
        /// <summary> Class takes a user choice for a parameter of an EMS_SubJobs at runtime. </summary>
        public class EMS_UserInput
        {
            /// <summary> Key of EMS_UserInputDefinitionItem. Needs to be transferred when taking the user input. </summary>
            //public string Key { get { return key == null ? string.Empty : key; } set { if (value != null) key = value; } }
            //private string key = string.Empty;

            /// <summary> List of parameters upon which the user input should be applied (defined as subjob-name/parameter-name). </summary>
            //public List<Tuple<string, string>> SubJobParameterList { get { return subJobParameterList == null ? new List<Tuple<string, string>>() : subJobParameterList; } set { if (value != null) subJobParameterList = value; } }
            //private List<Tuple<string, string>> subJobParameterList = new List<Tuple<string, string>>();

            /// <summary> The actual user input. </summary>
            //public object Input { get { return input == null ? string.Empty : input; } set { if (value != null) input = value; } }
            //private object input = string.Empty;

            /// <summary> User Input (which needs to be numeric) is multiplied by this factor. This allows e.g. to indicate %-values as e.g. 50 instead of 0.5. </summary>
            //public double ScalingFactor { get { return scalingFactor; } set { scalingFactor = value; } }
            //private double scalingFactor = 1;

            /// <summary>
            /// If EMS_UserInputDefinitionItem defines a SelectList, this parameter contains which item the user selects at run-time (SelectList.Key).
            /// This can for example be used for some Caption, to make clear what the user selected.
            /// </summary>
            //public string SelectionText { get { return selectionText == null ? string.Empty : selectionText; } set { if (value != null) selectionText = value; } }
            //private string selectionText = string.Empty;

            /// <summary>
            /// Optional, in fact only useful if caller is a programme (e.g. PET).
            /// Allows for different "user input" for different file packages.
            /// </summary>
            //public string FilePackageKey { get { return filePackageKey == null ? string.Empty : filePackageKey; } set { if (value != null) filePackageKey = value; } }
            //private string filePackageKey = string.Empty;

            /// <summary>
            /// Used for validation.
            /// Key: job-name + parameter-name for any parameter of SubJobParameterList that could be assigned.
            /// Value: optimally empty, if not: contains error-message if assigning caused problems.
            /// </summary>
            //internal Dictionary<string, string> Taken = new Dictionary<string, string>();

            /// <summary> Input is set to this value for validation purposes. </summary>
            //internal static string VALIDATION_INPUT = "VALIDATION_INPUT";
        }

        /// <summary>
        /// Class allows for holding the numbers of a specific statistic table (e.g. Means of Incomes table)
        /// A list of instances of this class is passed to the viewer.
        /// </summary>
        public class EMS_ViewerTableContent
        {
            public string LastError { get { return lastError ?? string.Empty; } private set { lastError = value; } }
            private string lastError = string.Empty;

            internal Dictionary<string, Dictionary<string, double>> Numbers = new Dictionary<string, Dictionary<string, double>>();
            private readonly string tableName = string.Empty; // only used for error messages

            public string SelectorKey { get { return selectorKey ?? string.Empty; } internal set { if (value != null) selectorKey = value; } }
            private string selectorKey = string.Empty;

            public EMS_ViewerTableContent() { }
            internal EMS_ViewerTableContent(string tableName) { if (tableName != null) this.tableName = tableName; }

            public List<string> GetColNames() { return Numbers.Keys.ToList<string>(); }
            public List<string> GetRowNames() { return (Numbers.Count == 0) ? new List<string>() : Numbers.ElementAt(0).Value.Keys.ToList<string>(); }

            public bool GetNumber(out double number, string col, string row)
            {
                number = 0.0; col = col.ToLower(); row = row.ToLower();
                if (!Numbers.ContainsKey(col))
                {
                    LastError = string.Format("Table '{0}' does not contain a colum '{1}'.", tableName, col);
                    return false;
                }
                if (!Numbers[col].ContainsKey(row))
                {
                    LastError = string.Format("Colum '{0}' of table '{1}' does not contain a row '{2}'.", col, tableName, row);
                    return false;
                }
                number = Numbers[col][row];
                return true;
            }
        }

        /// <summary>
        /// Class defines a button-group where each button allows to select a viewer-table.
        /// Buttons are arranged as drop-down.
        /// </summary>
        [Serializable]
        public class EMS_SelectorButtonGroup : EMS_SelectorBase
        {
            public List<EMS_SelectorButton> Buttons { get { return buttons ?? new List<EMS_SelectorButton>(); } set { if (value != null) buttons = value; } }
            private List<EMS_SelectorButton> buttons = new List<EMS_SelectorButton>();

            /// <summary> If not set the button group shows the caption of the currently selected button. </summary>
            public EMS_Caption GroupCaption { get { return groupCaption; } set { groupCaption = value; } }
            private EMS_Caption groupCaption = null;

            public EMS_SelectorButtonGroup() { SelectorType = EMS_SELECTOR_TYPE.BUTTONGROUP; }
        }

        
        /// <summary>
        /// Selector to be used for iterated tables.
        /// Each viewer-table emerging from the iteration is equipped with SelectorKey = EMS_SelectorButtonIterated.Key + italtX,
        /// which are also the Keys of the buttons emerging from this selector-type.
        /// </summary>
        [Serializable]
        public class EMS_SelectorButtonIterated : EMS_SelectorButton
        {
            /// <summary>
            /// True (default): the buttons emerging due to the iteration are arranged as a EMS_SelectorButtonGroup (i.e. a drop down).
            /// False: the buttons emerging due to the iteration are arranged as EMS_SelectorButton (i.e. alongside each other).
            /// </summary>
            public bool ArrangeAsGroup { get { return arrangeAsGroup; } set { arrangeAsGroup = value; } }
            private bool arrangeAsGroup = true;

            public EMS_SelectorButtonIterated() { SelectorType = EMS_SELECTOR_TYPE.BUTTONITERATED; }

            public List<EMS_SelectorButton> GetButtons(int numAlt)
            {
                List<EMS_SelectorButton> buttons = new List<EMS_SelectorButton>();
                /*
                foreach (string iteratorValue in EMS_Template.GetIteratorRuntimeValues(numAlt))
                {
                    EMS_SelectorButton button = new EMS_SelectorButton() { Caption = this.Caption, Tooltip = this.Tooltip };
                    button.SetKey(this.Key + iteratorValue);
                    buttons.Add(button);
                }*/

                return buttons;
            }
        }
        
        /// <summary> Class defines a button allowing to select a viewer-table. </summary>
        [Serializable]
        public class EMS_SelectorButton : EMS_SelectorBase
        {
            /// <summary> Note that there is no need (in fact it is impossible) to set Key manually. The setter is only public for serialisation. </summary>
            public string Key { get { return key; } set { if ((key == null || key == string.Empty) && value != null && value != string.Empty) key = value; } }
            private string key = null; // automatic generation of key!

            public EMS_Caption Caption { get { return caption ?? new EMS_Caption(); } set { if (value != null) caption = value; } }
            private EMS_Caption caption = new EMS_Caption();

            public string Tooltip { get { return tooltip ?? string.Empty; } set { if (value != null) tooltip = value; } }
            private string tooltip = string.Empty;

            public EMS_SelectorButton() { SelectorType = EMS_SELECTOR_TYPE.BUTTON; }

            internal void SetKey(string key) { this.key = key; }
        }

        /// <summary>
        /// Class defines a button or button-group (i.e. collection of buttons) to select a viewer-table.
        /// Main purpose of Base: allowing to bundle all selectors in a template-list (EMS_Template.Selectors).
        /// Definitions need to be "consumed" by SubJob_MakeTable.SelectorKey
        /// and are transferred to EMS_ViewerTableContent.SelectorKey during the process of generating viewer tables.
        /// </summary>
        [Serializable]
        public abstract class EMS_SelectorBase
        {
            public EMS_SELECTOR_TYPE SelectorType;
        }

        [Serializable]
        public enum EMS_SELECTOR_TYPE { BUTTON, BUTTONGROUP, BUTTONITERATED }
        
        /// <summary>
        /// Multipurpose caption for tables, columns, rows, selector-buttons, etc.
        /// Allows for using placeholders which are to be replaced at runtime by using file-package-info (e.g. system-name, file-name, ...).
        /// </summary>
        [Serializable]
        public class EMS_Caption
        {
            // placeholder, to be replaced by ...
            public const string PLACEHOLDER_BASE_FILE = "BASE_FILE"; // ... filename of base-file
            public const string PLACEHOLDER_BASE_SYSTEM = "BASE_SYSTEM"; // ... system, which is (tried to be) extracted from filename of base-file
            public const string PLACEHOLDER_BASE_COUNTRYSHORT = "BASE_COUNTRYSHORT"; // ... short name of country, which is (tried to be) extracted from filename of base-file
            public const string PLACEHOLDER_BASE_COUNTRYLONG = "BASE_COUNTRYLONG"; // ... long name of country, which is (tried to be) extracted from filename of base-file
            //     note this is using CountryShortToLong and CountryShortToLongPredef
            public const string PLACEHOLDER_BASE_SYSTEMYEAR = "BASE_YEAR"; // ... system-year, which is (tried to be) extracted from filename of base-file

            // note that the following placeholders require parameter 'key' of function 'GetText' to contain EMS_Template.ITERATOR_ALT
            public const string PLACEHOLDER_ALT_FILE = "ALT_FILE"; // ... detto for alt-files
            public const string PLACEHOLDER_ALT_SYSTEM = "ALT_SYSTEM";
            public const string PLACEHOLDER_ALT_COUNTRYSHORT = "ALT_COUNTRYSHORT";
            public const string PLACEHOLDER_ALT_COUNTRYLONG = "ALT_COUNTRYLONG";
            public const string PLACEHOLDER_ALT_SYSTEMYEAR = "ALT_YEAR";
            public const string PLACEHOLDER_ALT_NUMBER = "ALT_NUMBER"; // ... number of alternative scenario

            // note that the following placeholder requires parameter 'key' of function 'GetText' to contain EMS_Template.ITERATOR_COLUMNGROUP
            public const string PLACEHOLDER_GROUP_NUMBER = "GROUP_NUMBER"; // ... number of group

            // this is used as new EMS_Caption("whatsoever" + PLACEHOLDER_USER_INPUT + userInputDefinitionItem.Key + "whatsoever")
            // note that these placeholders are (somewhat inconsistent) not resolved here but in the StatisticPresenter
            public const string PLACEHOLDER_USER_INPUT = "USER_INPUT";
            public const string PLACEHOLDER_USER_SELECTION = "USER_SELECTION";
            public const string PLACEHOLDER_RUNTIME_CAPTION = "PLACEHOLDER_RUNTIME_CAPTION";

            // === T O D O === add more placehoders, if necessary (and improve implementation(!))

            public EMS_Caption() { }
            public EMS_Caption(string text) { Template = text; }

            public string Template { get { return template ?? string.Empty; } set { if (value != null) template = value; } }
            private string template = string.Empty;

            public bool ReplacementToUpper { get { return replacementToUpper; } set { replacementToUpper = value; } }
            private bool replacementToUpper = false;

            /// <summary>
            /// Replaces placeholders in Caption with run-time values.
            /// Parameter 'key': 'Name' of table/column/row respectively 'Key' of selector-button.
            /// Parameter is optional for placeholders referring to base-scenario, but compulsory for placeholders referring to
            /// alt-scenarios (key must contain EMS_Template.ITERATOR_ALT) or groups (key must contain EMS_Template.ITERATOR_COLUMNGROUP).
            /// </summary>
            public string GetText(string baseFileName, List<string> altFileNames, string key = "")
            {
                if (baseFileName == string.Empty && altFileNames.Count > 0) baseFileName = altFileNames[0]; // this allows the usage of any "base-placeholder" for a multi-alt template with reference scenario

                string text = Template;
                FileNameReplacements(ref text, false, baseFileName);

                string altFileName = GetAltFileName(key, altFileNames);
                if (altFileName != string.Empty) FileNameReplacements(ref text, true, altFileName);

                NumberReplacements(ref text, key);

                return text;
            }

            private static void NumberReplacements(ref string text, string key)
            {
                if (text.Contains(PLACEHOLDER_GROUP_NUMBER))
                    text = text.Replace(PLACEHOLDER_GROUP_NUMBER, (GetIteratorIndex(key, false) + 1).ToString());
                if (text.Contains(PLACEHOLDER_ALT_NUMBER))
                    text = text.Replace(PLACEHOLDER_ALT_NUMBER, (GetIteratorIndex(key, true) + 1).ToString());
            }

            private void FileNameReplacements(ref string text, bool alt, string fileName)
            {
                string phFile = alt ? PLACEHOLDER_ALT_FILE : PLACEHOLDER_BASE_FILE;
                if (text.Contains(phFile))
                    text = text.Replace(phFile, ReplacementToUpper ? fileName.ToUpper() : fileName);

                string phSystem = alt ? PLACEHOLDER_ALT_SYSTEM : PLACEHOLDER_BASE_SYSTEM;
                string phYear = alt ? PLACEHOLDER_ALT_SYSTEMYEAR : PLACEHOLDER_BASE_SYSTEMYEAR;
                if (text.Contains(phSystem) || text.Contains(phYear))
                {
                    string systemName = GetSystemName(fileName);
                    if (text.Contains(phSystem)) text = text.Replace(phSystem, systemName);
                    if (text.Contains(phYear)) text = text.Replace(phYear, GetSystemYear(systemName));
                }

                string phCountryShort = alt ? PLACEHOLDER_ALT_COUNTRYSHORT : PLACEHOLDER_BASE_COUNTRYSHORT;
                string phCountryLong = alt ? PLACEHOLDER_ALT_COUNTRYLONG : PLACEHOLDER_BASE_COUNTRYLONG;
                if (text.Contains(phCountryShort) || text.Contains(phCountryLong))
                {
                    string countryShortName = GetCountryShortName(fileName);
                    if (text.Contains(phCountryShort)) text = text.Replace(phCountryShort, countryShortName);
                    if (text.Contains(phCountryLong)) text = text.Replace(phCountryLong, EM_Helpers.ShortCountryToFullCountry(countryShortName));
                }
            }

            private string GetSystemName(string fileName)
            {
                string name = fileName.ToLower();
                name = name.Replace(".txt", string.Empty);
                name = name.Replace("_std", string.Empty);
                return ReplacementToUpper ? name.ToUpper() : name;
            }

            private string GetSystemYear(string systemName)
            {
                int i = systemName.IndexOf('_');
                return i < 0 ? systemName : systemName.Substring(i + 1);
            }

            private string GetCountryShortName(string fileName)
            {
                int i = fileName.IndexOf('_');
                if (i > 0) fileName = fileName.Substring(0, i);
                return ReplacementToUpper ? fileName.ToUpper() : fileName;
            }

            private string GetAltFileName(string key, List<string> altFileNames)
            {
                int iAlt = key != string.Empty ? GetIteratorIndex(key, true) : 0; // the latter may happen for main-caption or main-selector-caption (for MULTI_ALT)
                return iAlt >= 0 && altFileNames.Count > iAlt ? altFileNames[iAlt] : string.Empty;
            }

            public static int GetIteratorIndex(string key, bool alt)
            {
                string iterator = string.Empty;//alt ? EMS_Template.ITERATOR_ALT : EMS_Template.ITERATOR_COLUMNGROUP;
                if (!key.Contains(iterator)) return -1;
                string altNum = string.Empty;
                int i = key.IndexOf(iterator) + iterator.Length;
                while (i < key.Length && "0123456789".Contains(key[i])) altNum += key[i++];
                int iAlt = Convert.ToInt32(altNum) - 1;
                return iAlt;
            }
        }
        /*
        /// <summary> Class is responsible for communication with the Viewer. </summary>
        public class EMS_InterfaceViewer
        {
            /// <summary>
            /// If any function returns false, this variable is filled with the respective error.
            /// </summary>
            public string LastError { get { return lastError == null ? string.Empty : lastError; } private set { lastError = value; } }
            private string lastError = string.Empty;

            /// <summary>
            /// If GetStatisticsData produces an error and the respective template's or subjob's TryContinueOnError-variable is set to true
            /// (and was successful in providing substitute action) GetStatistics still returns true and LastError is not set.
            /// Instead this list of warnings is extended by the respective error message.
            /// </summary>
            public List<string> Warnings { get { return warnings == null ? new List<string>() : warnings; } private set { warnings = value; } }
            private List<string> warnings = new List<string>();

            /// <summary>
            /// Provides the data tables for a certain statistic set.
            /// For classic summary statistics these would comprise poverty table, inequality table, means/share of incomes table,
            ///                                                     average/share of hh-members table, cut-offs table.
            /// Note that these are pure data tables (for corresponding layouts see EMS_DecoStatistic).
            /// Also note that this comprises tables for one "file package" (in case of summary statistics for one output-file/system).
            ///           Obtaining data for all "file packages" (all output-files/systems) requires calling the function several times.
            /// </summary>
            public bool GetViewerTablesContent(out Dictionary<string, EMS_ViewerTableContent> viewerTables,
                                               string pathTemplate, EMS_FilePackageContent filePackage,
                                               List<EMS_UserInput> userInput = null) // if the template provides for user-input (see EMS_Template.UserInputDefinitions)
            {                                                                                                   // respective run-time user-inputs can be handed over via this parameter
                Template template; viewerTables = new Dictionary<string, EMS_ViewerTableContent>();
                try { template = XML_handling.ParseTemplate(pathTemplate); }
                catch (Exception exception) { LastError = exception.Message; return false; }
                return GetViewerTablesContent(out viewerTables, template, filePackage, userInput);
            }
            public bool GetViewerTablesContent(out Dictionary<string, EMS_ViewerTableContent> viewerTables,
                                               Template pTemplate, EMS_FilePackageContent filePackage,
                                               List<EMS_UserInput> userInput = null)
            {
                viewerTables = new Dictionary<string, EMS_ViewerTableContent>();

                string error;
                if (pTemplate == null) { LastError = "Template not defined."; return false; }
                Template template = pTemplate.Copy(); // note that this is a not very elegant way to not "pollute" the template, as otherwise (the rather rare cases)
                // of multi-package EMS_TEMPLATE_TYPE.BASE_ALT and EMS_TEMPLATE_TYPE.MULTI_ALT do not work
                if (!template.Validate(out lastError)) return false;

                if (!CheckFilePackage(ref filePackage, template.TemplateType, out error)) { LastError = error; return false; }

                EMS_SubJob_Processor subJob_Processor = new EMS_SubJob_Processor(); DataTable dummy;
                if (!subJob_Processor.GetViewerTables(out viewerTables, out dummy,
                                                      template.TemplatePartBase, filePackage.PathBase,
                                                      template.TemplatePartAlt, filePackage.PathsAlt,
                                                      template.TemplatePartJoint, template.VariableJoin,
                                                      template.TemplateType, userInput))
                {
                    LastError = subJob_Processor.LastError;
                    return false;
                }

                Warnings = subJob_Processor.Warnings;
                return true;
            }

            /// <summary>
            /// Function allows access to the data produces instead of delivering results in tables (to be displayed by the presenter),
            /// i.e. dataTable is the one-row DataTable that contains all the calculated variables (as columns).
            /// </summary>
            public bool GetDataTable(out DataTable dataTable, Template template, EMS_FilePackageContent filePackage)
            {
                dataTable = null;

                string error;
                if (!template.Validate(out lastError)) return false;
                if (!CheckFilePackage(ref filePackage, template.TemplateType, out error)) { LastError = error; return false; }

                EMS_SubJob_Processor subJob_Processor = new EMS_SubJob_Processor(); Dictionary<string, EMS_ViewerTableContent> dummy;
                if (!subJob_Processor.GetViewerTables(out dummy, out dataTable,
                                                      template.TemplatePartBase, filePackage.PathBase,
                                                      template.TemplatePartAlt, filePackage.PathsAlt,
                                                      template.TemplatePartJoint, template.VariableJoin,
                                                      template.TemplateType))
                { LastError = subJob_Processor.LastError; return false; }
                Warnings = subJob_Processor.Warnings;
                return true;
            }

            /// <summary>
            /// Function is called internally by GetStatisticsData to protect EMS_Processor from faulty input and Viewer from strange error messages.
            /// However, function should be called upfront by the external caller, i.e. user interface, ELight
            /// (or file package consistence with template type checked for in another appropriate manner).
            /// </summary>
            public static bool CheckFilePackage(ref EMS_FilePackageContent filePackage, Template template, out string error)
            {
                error = string.Empty;
                if (filePackage == null) { error = "File package not defined."; return false; }
                switch (template.templateType)
                {
                    case HardDefinitions.TemplateType.Default:
                        if (filePackage.PathBase == string.Empty)
                        { error = "File package does not contain the path of the base scenario file."; return false; }
                        filePackage.PathsAlt = null; // make sure this is not set (to avoid confusing SubJob_Processor)
                        return true;
                    case HardDefinitions.TemplateType.BaselineReform:
                        if (filePackage.PathBase == string.Empty) error = "File package does not contain the path of the base scenario file." + Environment.NewLine;
                        if (filePackage.PathsAlt.Count == 0) error += "File package does not contain the path(s) of the alternative scenario(s) file(s).";
                        return error == string.Empty;
                    case HardDefinitions.TemplateType.Multi:
                        if (filePackage.PathsAlt.Count < 2) { error = "File package does not contain sufficient paths info."; return false; }
                        filePackage.PathBase = null; // make sure this is not set (to avoid confusing SubJob_Processor)
                        return true;
                    default: error = string.Format("Unknown template type '{0}'.", template.templateType); return false;
                }
                // note checking for valid paths is not included here as it anyway has to be done by the reading sub jog (for security reasons)
            }
        }
         */
    }
}
