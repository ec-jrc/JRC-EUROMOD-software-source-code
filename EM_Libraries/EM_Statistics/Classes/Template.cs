using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    /// <summary>
    /// The Template class holds the entire template including all actions, pages and tables.
    /// </summary>
    public class Template
    {
        // The template info is used in the template selection process, so it needs to be public.
        public TemplateInfo info = new TemplateInfo();
        // The global actions/filters and the template pages/tables should not be directly accessible from outside.
        internal List<Filter> globalFilters = new List<Filter>();
        internal List<Action> globalActions = new List<Action>();
        internal List<Page> pages = new List<Page>();

        public Template(bool addDefaultLevels = false)
        {
            if (addDefaultLevels)
            {
                info.calculationLevels.Add(new TemplateInfo.CalculationLevel() { name = HardDefinitions.DefaultCalculationLevels.INDIVIDUAL, groupingVar = HardDefinitions.idPerson });
                info.calculationLevels.Add(new TemplateInfo.CalculationLevel() { name = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD, groupingVar = HardDefinitions.idHH });
            }
        }

        /// <summary>
        /// The TemplateInfo class holds all the public information about the template, including its name, description and list of required/optional/user variables.
        /// </summary>
        public class TemplateInfo
        {
            public string name = "No name found";
            public string description = "No description found";
            public string generalDescription = string.Empty;
            public string title = "No title found";
            public string subtitle = string.Empty;
            private string _button = null; // make sure button is never empty, as it is used else excel-file-name for export
            public string button { get { return _button ?? "NoName"; } set { _button = value; } }
            public int minFiles = 1;
            public int maxFiles = 50;
            public bool HideMainSelectorForSingleFilePackage = true;
            public int sdcMinObsDefault = int.MinValue;
            public Dictionary<string, int> sdcMinObsAlternatives = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            public bool sdcHideZeroObs = true;
            public HardDefinitions.TemplateType templateType = HardDefinitions.TemplateType.Default;
            public List<CalculationLevel> calculationLevels = new List<CalculationLevel>();
            public List<RequiredVariable> requiredVariables = new List<RequiredVariable>();
            public List<OptionalVariable> optionalVariables = new List<OptionalVariable>();
            public List<UserVariable> userVariables = new List<UserVariable>();
            public HardDefinitions.ExportDescriptionMode exportDescriptionMode = HardDefinitions.ExportDescriptionMode.No;

            public class CalculationLevel
            {
                public string name = string.Empty;
                public string groupingVar = string.Empty;
                internal CalculationLevel Clone()
                {
                    return new CalculationLevel() { name = this.name, groupingVar = this.groupingVar };
                }
            }

            public class RequiredVariable
            {
                public string name = string.Empty;
                public string readVar = string.Empty;
                internal RequiredVariable Clone()
                {
                    return new RequiredVariable() { name = this.name, readVar = this.readVar };
                }
            }

            public class OptionalVariable
            {
                public string name = string.Empty;
                public string readVar = string.Empty;
                public double defaultValue = 0.0;
                internal OptionalVariable Clone()
                {
                    return new OptionalVariable() { name = this.name, readVar = this.readVar, defaultValue = this.defaultValue };
                }
            }

            public class UserVariable
            {
                public HardDefinitions.UserInputType inputType = HardDefinitions.UserInputType.Null;
                public string name = string.Empty;
                public string description = string.Empty;
                public string title = string.Empty;
                public List<ComboItem> comboItems = new List<ComboItem>();
                public string value = string.Empty;
                public string defaultValue = string.Empty;
                public bool displayDescription = false;
                public string packageKey = null;
                public int reformNumber = -1;
                public Dictionary<string, string> forEachValueDescription = new Dictionary<string, string>();

                public UserVariable() {}
                public UserVariable(UserVariable uv)
                {
                    if (uv != null)
                    {
                        inputType = uv.inputType; name = uv.name; description = uv.description; title = uv.title;
                        comboItems = uv.comboItems; value = uv.value; defaultValue = uv.defaultValue;
                        displayDescription = uv.displayDescription; reformNumber = uv.reformNumber; packageKey = uv.packageKey;
                    }
                }
                internal UserVariable Clone()
                {
                    UserVariable uv = new UserVariable
                    {
                        inputType = this.inputType,
                        name = this.name,
                        description = this.description,
                        title = this.title,
                        value = this.value,
                        defaultValue = this.defaultValue,
                        displayDescription = this.displayDescription,
                        packageKey = this.packageKey,
                        reformNumber = this.reformNumber
                    };
                    foreach (ComboItem c in this.comboItems) uv.comboItems.Add(c.Clone());
                    foreach (KeyValuePair<string, string> d in this.forEachValueDescription) uv.forEachValueDescription.Add(d.Key, d.Value);
                    return uv;
                }
            }

            public class ComboItem
            {
                public string name = string.Empty;
                public string value = string.Empty;
                internal ComboItem Clone()
                {
                    return new ComboItem() { name = this.name, value = this.value };
                }
            }

            /// <summary>
            /// GetRequiredFields returns all the required variables
            /// </summary>
            /// <returns>A list of strings, containing the names of all the required variables.</returns>
            public List<string> GetRequiredVariables()
            {
                return requiredVariables.Select(x => x.readVar).ToList();
            }

            public void TakeUserInput(List<Template.TemplateInfo.UserVariable> userInputs)
            {
                if (userInputs != null)
                {
                    List<Template.TemplateInfo.UserVariable> duplicates = new List<Template.TemplateInfo.UserVariable>();
                    foreach (Template.TemplateInfo.UserVariable userVar in userVariables)
                    {
                        var uvs = userInputs.Where(x => x.name == userVar.name); // usually there should be one userInput for each UserVariable in the template
                        if (uvs.Any())                                     // but for PET we require user-input per package and reform
                        {
                            userVar.value = uvs.First().value;
                            userVar.description = uvs.First().description;
                            userVar.forEachValueDescription = uvs.First().forEachValueDescription;
                            userVar.reformNumber = uvs.First().reformNumber;
                            userVar.packageKey = uvs.First().packageKey;
                            foreach (var uv in uvs.Skip(1)) // no need for this if there isn't user-input per package and/or reform
                            {
                                Template.TemplateInfo.UserVariable duv = new Template.TemplateInfo.UserVariable(userVar)
                                {
                                    value = uv.value,
                                    description = uv.description,
                                    reformNumber = uv.reformNumber,
                                    packageKey = uv.packageKey
                                };
                                userVar.forEachValueDescription = uvs.First().forEachValueDescription;
                                duplicates.Add(duv);
                            }
                        }
                    }
                    userVariables.AddRange(duplicates);
                }
            }

            /// <summary>
            /// GetUserVariables returns all the user variables.
            /// </summary>
            /// <returns>A list of UserVariable, containing all teh user variables.</returns>
            public List<UserVariable> GetUserVariables()
            {
                return userVariables;
            }

            internal UserVariable GetUserVariable(string name, string packageKey = null, int reformNumber = -1)
            {
                foreach (UserVariable uv in userVariables)
                {
                    if (uv.name != name) continue; // usually only this condition is relevant, but for PET we require user-variables ...
                    if (uv.reformNumber >= 0 && uv.reformNumber != reformNumber) continue; // ... per reform, i.e. per alpha (cpi, custom, MII)
                    if (uv.packageKey != null && uv.packageKey != packageKey) continue;    // ... per package, i.e. per country and/or data
                    return uv;
                }
                return null;
            }

            internal string GetCalculationLevelVar(string level)
            {
                foreach (CalculationLevel l in calculationLevels)
                    if (l.name == level) return l.groupingVar;
                return string.Empty;
            }

            internal string GetCalculationLevelFromVar(string var)
            {
                foreach (CalculationLevel l in calculationLevels)
                    if (l.groupingVar == var) return l.name;
                return string.Empty;
            }

            internal TemplateInfo Clone()
            {
                TemplateInfo inf = new TemplateInfo
                {
                    name = this.name,
                    description = this.description,
                    generalDescription = this.generalDescription,
                    title = this.title,
                    subtitle = this.subtitle,
                    button = this.button,
                    minFiles = this.minFiles,
                    maxFiles = this.maxFiles,
                    HideMainSelectorForSingleFilePackage = this.HideMainSelectorForSingleFilePackage,
                    templateType = this.templateType,
                    sdcMinObsDefault = this.sdcMinObsDefault,
                    sdcHideZeroObs = this.sdcHideZeroObs,
                    exportDescriptionMode = this.exportDescriptionMode
                };
                foreach (CalculationLevel x in this.calculationLevels) inf.calculationLevels.Add(x.Clone());
                foreach (RequiredVariable x in this.requiredVariables) inf.requiredVariables.Add(x.Clone());
                foreach (OptionalVariable x in this.optionalVariables) inf.optionalVariables.Add(x.Clone());
                foreach (UserVariable x in this.userVariables) inf.userVariables.Add(x.Clone());
                inf.sdcMinObsAlternatives = new Dictionary<string, int>();
                foreach(var x in this.sdcMinObsAlternatives) inf.sdcMinObsAlternatives.Add(x.Key, x.Value);
                return inf;
            }
        }

        internal class Filter
        {
            internal string name = string.Empty;
            internal string formulaString = string.Empty;
            internal bool reform = false;
            internal List<Parameter> parameters = new List<Parameter>();
            internal Dictionary<string, Func<List<double>, bool>> func;

            public Filter() { }

            /// <summary>
            /// Used to make anonymous hardcoded filters (see S8020 etc.)
            /// </summary>
            /// <param name="data"></param>
            /// <param name="_func"></param>
            public Filter(DataStatsHolder data, Func<List<double>, bool> _func)
            {
                func = new Dictionary<string, Func<List<double>, bool>> { { FuncID(data), _func } };
            }

            /// <summary>
            /// User to copy a filter or make a reform filter.
            /// </summary>
            /// <param name="copyFilter"></param>
            /// <param name="refNo"></param>
            public Filter(Filter copyFilter, int refNo = 0)
            {
                if (copyFilter == null) return;
                if (!string.IsNullOrEmpty(copyFilter.name))
                    name = copyFilter.name + (refNo > 0 ? HardDefinitions.Reform + refNo : string.Empty);
                formulaString = copyFilter.formulaString;
                parameters = new List<Parameter>();
                foreach (Parameter p in copyFilter.parameters)
                    parameters.Add(new Parameter(p, refNo));
            }

            internal static string FuncID(DataStatsHolder data)
            {
                return data.dataNo + data.level + data.packageKey;
            }

            internal Func<List<double>, bool> GetFunc(DataStatsHolder data)
            {
                if (func == null || func.Count == 0)
                    return x => true;
                return func.ContainsKey(FuncID(data)) ? func[FuncID(data)] : func.First().Value;
            }

            internal bool HasFunc(DataStatsHolder data)
            {
                return func != null && func.ContainsKey(FuncID(data));
            }

            internal Filter Clone()
            {
                return new Filter(this);
            }
        }

        internal class Action
        {
            internal string _calculationLevel = string.Empty;
            internal HardDefinitions.CalculationType calculationType = HardDefinitions.CalculationType.NA;
            internal string outputVar = string.Empty;
            internal string formulaString = string.Empty;
            internal List<Parameter> parameters = new List<Parameter>();
            internal Filter filter = null;
            internal bool? _reform = null;
            internal bool? _saveResult = null;
            internal bool? _blendParameters = null; // this could be enum (overwrite, blend, add) instead of bool - or is this a parameter attribute?
            internal Func<List<double>, double> func;
            internal double result = double.NaN;

            internal string CalculationLevel { get { return string.IsNullOrEmpty(_calculationLevel) ? HardDefinitions.DefaultCalculationLevels.INDIVIDUAL : _calculationLevel; } }
            internal bool Reform { get { return !_reform.HasValue || _reform.Value; } }
            internal bool SaveResult { get { return !_saveResult.HasValue || _saveResult.Value; } }
            internal bool BlendParameters { get { return !_blendParameters.HasValue || _blendParameters.Value; } }


            public Action() { }
            public Action(Action copyAction, int refNo = 0)
            {
                _calculationLevel = copyAction._calculationLevel;
                calculationType = copyAction.calculationType;
                outputVar = copyAction.outputVar + (refNo > 0 && copyAction.Reform ? HardDefinitions.Reform + refNo : string.Empty);
                formulaString = copyAction.formulaString;
                parameters = new List<Parameter>();
                foreach (Parameter p in copyAction.parameters)
                    parameters.Add(new Parameter(p, refNo));
                if (copyAction.filter != null)
                    filter = new Filter(copyAction.filter, refNo);
                _reform = refNo == 0 ? copyAction._reform : false; // is this already a reform? if so, it should not be re-reformed! ;)
                _blendParameters = copyAction._blendParameters;
                _saveResult = copyAction._saveResult;
            }

            internal bool GetVariableParameter(string parName, out string varName)
            {
                varName = string.Empty;
                if (string.IsNullOrEmpty(parName)) return false;
                if (parameters.FirstOrDefault(x => x.name == parName) == null) return false;
                varName = parameters.First(x => x.name == parName).variableName;
                return !string.IsNullOrEmpty(varName);
            }

            internal bool GetUnnamedVariableParameter(out string varName)
            {
                varName = string.Empty;
                if (parameters.FirstOrDefault(x => string.IsNullOrEmpty(x.name)) == null) return false;
                varName = parameters.First(x => string.IsNullOrEmpty(x.name)).variableName;
                return !string.IsNullOrEmpty(varName);
            }

            internal bool GetDoubleParameter(string parName, out double dVal)
            {
                dVal = double.NaN;
                if (parameters.FirstOrDefault(x => x.name == parName) == null) return false;
                dVal = parameters.First(x => x.name == parName).numericValue;
                return !double.IsNaN(dVal);
            }

            internal bool GetBoolParameter(string parName, out bool bVal)
            {
                bVal = false;
                if (parameters.FirstOrDefault(x => x.name == parName) == null) return false;
                bool? bv = parameters.First(x => x.name == parName).boolValue;
                if (bv == true) bVal = true;
                return bv != null;
            }

            internal bool GetEquivBandsParameters(string parName, out Dictionary<string, double> sdVal)
            {
                sdVal = new Dictionary<string, double>();
                if (parameters.FirstOrDefault(x => x.name == parName) == null) return false;
                bool allOK = true; 
                foreach(Parameter p in parameters.Where(x => x.name == parName))
                {
                    if (double.IsNaN(p.numericValue) || string.IsNullOrEmpty(p.variableName))
                    {
                        allOK = false;
                        break;
                    }
                    sdVal.Add(p.variableName, p.numericValue);
                }
                return allOK;
            }

            internal bool HasParameter(string parName)
            {
                return parameters.Any(x => x.name == parName);
            }

            public override string ToString()
            {
                return string.Format("{0} -> {1}", Enum.GetName(typeof(HardDefinitions.CalculationType), calculationType), outputVar);
            }

            internal Action Clone()
            {
                return new Action(this);
            }
        }

        internal class Parameter
        {
            internal string name = string.Empty;
            internal string variableName = string.Empty;
            internal double numericValue = double.NaN;
            internal bool? boolValue = null;
            internal bool? _reform = null;
            internal bool Reform { get { return !_reform.HasValue || _reform.Value; } }

            public Parameter() { }
            public Parameter(Parameter copyParameter, int refNo = 0)
            {
                name = copyParameter.name;
                variableName = copyParameter.variableName;
                numericValue = copyParameter.numericValue;
                boolValue = copyParameter.boolValue;
                _reform = copyParameter._reform;
                if (!string.IsNullOrEmpty(copyParameter.variableName) && refNo > 0 && copyParameter.Reform && (copyParameter.variableName[0] < '0' || copyParameter.variableName[0] > '9'))  // if reform, and parameter not numeric 
                    variableName += HardDefinitions.Reform + refNo;
            }

            internal Parameter Clone()
            {
                return new Parameter()
                {
                    name = this.name,
                    variableName = this.variableName,
                    numericValue = this.numericValue,
                    boolValue = this.boolValue,
                    _reform = this._reform
                };
            }
        }

        internal class Page
        {
            internal string name = string.Empty;
            internal string title = string.Empty;
            internal string subtitle = string.Empty;
            internal string button = string.Empty;
            internal string description = string.Empty;
            internal bool visible = true;
            internal bool perReform = false;
            internal List<Table> tables = new List<Table>();
            internal List<Action> actions = new List<Action>();

            internal class Table
            {
                internal string name = string.Empty;
                internal string title = string.Empty;
                internal string subtitle = string.Empty;
                internal string description = string.Empty;
                internal bool visible = true;
                internal HardDefinitions.ColumnGrouping columnGrouping = HardDefinitions.ColumnGrouping.SystemFirst;
                internal List<Column> columns = new List<Column>();
                internal List<Column> reformColumns = new List<Column>();
                internal List<Row> rows = new List<Row>();
                internal List<Cell> cells = new List<Cell>();
                internal List<Cell> reformCells = new List<Cell>();
                internal string stringFormat = string.Empty;
                internal Action action = new Action();
                internal DisplayResults.DisplayPage.DisplayTable.DisplayGraph graph;
                internal SDCDefinition sdcDefinition = new SDCDefinition();

                internal class TableElement
                {
                    internal bool? strong = null;
                    internal string foregroundColour = string.Empty;
                    internal string backgroundColour = string.Empty;
                    internal string stringFormat = null;
                    internal Action action = null;
                    internal string tooltip = null;
                    internal SDCDefinition sdcDefinition = new SDCDefinition();
                }

                internal class SDCDefinition
                {
                    internal string minObsAlternativeName = null;
                    internal int? minObsAlternative = null;
                    internal bool? countNonZeroObsOnly = null;
                    internal bool? ignoreActionFilter = null;
                    internal List<string> secondaryGroups = new List<string>();
                    internal bool? suspendSecondaryGroups = false;
                    internal bool? suspendSdc = null;

                    internal SDCDefinition Clone()
                    {
                        SDCDefinition sdcDefinition = new SDCDefinition()
                        {
                            minObsAlternativeName = this.minObsAlternativeName,
                            minObsAlternative = this.minObsAlternative,
                            countNonZeroObsOnly = this.countNonZeroObsOnly,
                            ignoreActionFilter = this.ignoreActionFilter,
                            suspendSdc = this.suspendSdc,
                            suspendSecondaryGroups = this.suspendSecondaryGroups
                        };
                        foreach (string x in this.secondaryGroups) sdcDefinition.secondaryGroups.Add(x);
                        return sdcDefinition;
                    }
                }

                internal class Column : TableElement
                {
                    internal string name = string.Empty;
                    internal bool isVisible = true;
                    internal bool hasSeparatorBefore = false;
                    internal bool hasSeparatorAfter = false;
                    internal double tiesWith = double.NaN;  // this is used to visually tie a reform column to a base column

                    internal Column Clone()
                    {
                        return new Column()
                        {
                            strong = this.strong,
                            stringFormat = this.stringFormat,
                            foregroundColour = this.foregroundColour,
                            backgroundColour = this.backgroundColour,
                            action = this.action?.Clone(),
                            tooltip = this.tooltip,
                            name = this.name,
                            isVisible = this.isVisible,
                            hasSeparatorAfter = this.hasSeparatorAfter,
                            hasSeparatorBefore = this.hasSeparatorBefore,
                            tiesWith = this.tiesWith,
                            sdcDefinition = this.sdcDefinition.Clone()
                        };
                    }
                }

                internal class Row : TableElement
                {
                    internal string name = string.Empty;
                    internal bool isVisible = true;
                    internal bool hasSeparatorBefore = false;
                    internal bool hasSeparatorAfter = false;
                    internal bool forEachDataRow = false;
                    internal string forEachValueOf = null;

                    internal Row Clone()
                    {
                        return new Row()
                        {
                            strong = this.strong,
                            stringFormat = this.stringFormat,
                            action = this.action?.Clone(),
                            foregroundColour = this.foregroundColour,
                            backgroundColour = this.backgroundColour,
                            tooltip = this.tooltip,
                            name = this.name,
                            isVisible = this.isVisible,
                            hasSeparatorAfter = this.hasSeparatorAfter,
                            hasSeparatorBefore = this.hasSeparatorBefore,
                            forEachDataRow = this.forEachDataRow,
                            forEachValueOf = this.forEachValueOf,
                            sdcDefinition = this.sdcDefinition.Clone()
                        };
                    }
                }

                internal class Cell : TableElement
                {
                    internal int rowNum = 0;
                    internal int colNum = 0;
                    internal List<string> secondarySdcGroups = new List<string>();

                    internal Cell Clone()
                    {
                        return new Cell()
                        {
                            strong = this.strong,
                            stringFormat = this.stringFormat,
                            foregroundColour = this.foregroundColour,
                            backgroundColour = this.backgroundColour,
                            action = this.action?.Clone(),
                            tooltip = this.tooltip,
                            rowNum = this.rowNum,
                            colNum = this.colNum,
                            sdcDefinition = this.sdcDefinition.Clone()
                        };
                    }
                }

                internal Cell GetCell(int c, int r)
                {
                    return cells.FirstOrDefault(x => x.rowNum == r && x.colNum == c);
                }

                internal Cell GetReformCell(int c, int r)
                {
                    return reformCells.FirstOrDefault(x => x.rowNum == r && x.colNum == c);
                }

                internal Table Clone()
                {
                    Table t = new Table
                    {
                        name = this.name,
                        title = this.title,
                        subtitle = this.subtitle,
                        description = this.description,
                        visible = this.visible,
                        columnGrouping = this.columnGrouping,
                        stringFormat = this.stringFormat,
                        action = this.action.Clone(),
                        graph = this.graph, // todo: check this comment: Important! The graph should never be changed by the TemplateCalculator once read! (hence not cloned)
                        sdcDefinition = this.sdcDefinition.Clone()
                    };
                    foreach (Column x in this.columns) t.columns.Add(x.Clone());
                    foreach (Column x in this.reformColumns) t.reformColumns.Add(x.Clone());
                    foreach (Row x in this.rows) t.rows.Add(x.Clone());
                    foreach (Cell x in this.cells) t.cells.Add(x.Clone());
                    foreach (Cell x in this.reformCells) t.reformCells.Add(x.Clone());
                    return t;
                }
            }

            internal Page Clone()
            {
                Page p = new Page
                {
                    name = this.name,
                    title = this.title,
                    subtitle = this.subtitle,
                    button = this.button,
                    description = this.description,
                    visible = this.visible,
                    perReform = this.perReform
                };
                foreach (Table t in this.tables) p.tables.Add(t.Clone());
                foreach (Action a in this.actions) p.actions.Add(a.Clone());
                return p;
            }
        }

        internal Template Clone()
        {
            Template t = new Template { info = this.info.Clone() };
            foreach (Filter f in this.globalFilters) t.globalFilters.Add(f.Clone());
            foreach (Action a in this.globalActions) t.globalActions.Add(a.Clone());
            foreach (Page p in this.pages) t.pages.Add(p.Clone());
            return t;
        }
    }
}
