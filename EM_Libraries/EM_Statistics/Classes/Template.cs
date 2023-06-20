using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
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
            public HardDefinitions.TemplateType templateType = HardDefinitions.TemplateType.Default;
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
            public List<KeyValuePair<string, int>> sdcMinObsAlternatives = new List<KeyValuePair<string, int>>();
            public bool sdcHideZeroObs = true;
            public List<CalculationLevel> calculationLevels = new List<CalculationLevel>();
            public List<RequiredVariable> requiredVariables = new List<RequiredVariable>();
            public List<OptionalVariable> optionalVariables = new List<OptionalVariable>();
            public List<UserVariable> userVariables = new List<UserVariable>();
            public HardDefinitions.ExportDescriptionMode exportDescriptionMode = HardDefinitions.ExportDescriptionMode.InSheets;
            internal bool endUserFriendlyActionErrorMode = false;
            internal bool doMultiColumns = true;

            public class CalculationLevel
            {
                public string name = string.Empty;
                public string groupingVar = string.Empty;

                internal CalculationLevel Clone()
                {
                    return new CalculationLevel() { name = this.name, groupingVar = this.groupingVar };
                }

                internal bool ApiMerge(CalculationLevel apiCL, bool keep, ErrorCollector errorCollector)
                {
                    TemplateApi.Merge(ref groupingVar, apiCL.groupingVar, keep, errorCollector);
                    return true;
                }

                internal void CheckConsistency(ErrorCollector errorCollector)
                {
                    if (string.IsNullOrEmpty(name)) errorCollector.AddXmlMissingPropError("Name", string.Empty, "AdditionalCalculationLevel");
                    if (string.IsNullOrEmpty(groupingVar)) errorCollector.AddXmlMissingPropError("GroupingVar", name, "AdditionalCalculationLevel");
                }
            }

            public class RequiredVariable
            {
                public string name = string.Empty;
                public string readVar = string.Empty;
                public bool monetary = true;

                internal RequiredVariable Clone()
                {
                    return new RequiredVariable() { name = this.name, readVar = this.readVar, monetary = this.monetary };
                }

                internal bool ApiMerge(RequiredVariable apiRV, bool keep, ErrorCollector errorCollector)
                {
                    TemplateApi.Merge(ref readVar, apiRV.readVar, keep, errorCollector);
                    TemplateApi.Merge(ref monetary, apiRV.monetary, true, keep, errorCollector);
                    return true;
                }

                internal void CheckConsistency(ErrorCollector errorCollector)
                {
                    if (string.IsNullOrEmpty(name)) errorCollector.AddXmlMissingPropError("Name", string.Empty, "RequiredVariable");
                    if (string.IsNullOrEmpty(readVar)) errorCollector.AddXmlMissingPropError("ReadVar", name, "RequiredVariable");
                }
            }

            public class OptionalVariable
            {
                public string name = string.Empty;
                public string readVar = string.Empty;
                public double defaultValue = 0.0;
                public bool monetary = true;

                internal OptionalVariable Clone()
                {
                    return new OptionalVariable() { name = this.name, readVar = this.readVar, defaultValue = this.defaultValue, monetary = this.monetary };
                }

                internal bool ApiMerge(OptionalVariable apiOV, bool keep, ErrorCollector errorCollector)
                {
                    TemplateApi.Merge(ref readVar, apiOV.readVar, keep, errorCollector);
                    TemplateApi.Merge(ref defaultValue, apiOV.defaultValue, 0.0, keep, errorCollector);
                    TemplateApi.Merge(ref monetary, apiOV.monetary, true, keep, errorCollector);
                    return true;
                }

                internal void CheckConsistency(ErrorCollector errorCollector)
                {
                    if (string.IsNullOrEmpty(name)) errorCollector.AddXmlMissingPropError("Name", string.Empty, "OptionalVariable");
                    if (string.IsNullOrEmpty(readVar)) errorCollector.AddXmlMissingPropError("ReadVar", name, "OptionalVariable");
                }
            }

            public class UserVariable
            {
                public HardDefinitions.UserInputType inputType = HardDefinitions.UserInputType.Null;
                public string name = string.Empty;
                public bool monetary = true;
                public string description = string.Empty;
                public string title = string.Empty;
                public List<ComboItem> comboItems = new List<ComboItem>();
                public string value = string.Empty;
                public string defaultValue = string.Empty;
                public bool displayDescription = false;
                public string packageKey = null;
                public int refNo = -1;
                public Dictionary<string, string> forEachValueDescription = new Dictionary<string, string>();

                public UserVariable() { }
                public UserVariable(UserVariable uv)
                {
                    if (uv != null)
                    {
                        inputType = uv.inputType; name = uv.name; description = uv.description; title = uv.title;
                        comboItems = uv.comboItems; value = uv.value; defaultValue = uv.defaultValue;
                        displayDescription = uv.displayDescription; refNo = uv.refNo; packageKey = uv.packageKey;
                    }
                }
                internal UserVariable Clone()
                {
                    UserVariable uv = new UserVariable
                    {
                        inputType = this.inputType,
                        name = this.name,
                        monetary = this.monetary,
                        description = this.description,
                        title = this.title,
                        value = this.value,
                        defaultValue = this.defaultValue,
                        displayDescription = this.displayDescription,
                        packageKey = this.packageKey,
                        refNo = this.refNo
                    };
                    foreach (ComboItem c in this.comboItems) uv.comboItems.Add(c.Clone());
                    foreach (KeyValuePair<string, string> d in this.forEachValueDescription) uv.forEachValueDescription.Add(d.Key, d.Value);
                    return uv;
                }

                internal bool ApiMerge(UserVariable apiUV, bool keep, ErrorCollector errorCollector)
                {
                    TemplateApi.Merge(ref inputType, apiUV.inputType, keep, errorCollector);
                    TemplateApi.Merge(ref monetary, apiUV.monetary, true, keep, errorCollector);
                    TemplateApi.Merge(ref description, apiUV.description, keep, errorCollector);
                    TemplateApi.Merge(ref title, apiUV.title, keep, errorCollector);
                    TemplateApi.Merge(ref value, apiUV.value, keep, errorCollector);
                    TemplateApi.Merge(ref defaultValue, apiUV.defaultValue, keep, errorCollector);
                    TemplateApi.Merge(ref displayDescription, apiUV.displayDescription, false, keep, errorCollector);
                    if (apiUV.comboItems != null && apiUV.comboItems.Any() && (comboItems == null || !comboItems.Any() || !keep)) comboItems = apiUV.comboItems;
                    if (apiUV.forEachValueDescription != null && apiUV.forEachValueDescription.Any() && (forEachValueDescription == null || !forEachValueDescription.Any() || !keep)) forEachValueDescription = apiUV.forEachValueDescription;
                    return true;
                }

                internal void CheckConsistency(ErrorCollector errorCollector)
                {
                    if (string.IsNullOrEmpty(name)) errorCollector.AddXmlMissingPropError("Name", string.Empty, "UserVariable");
                    if (inputType == HardDefinitions.UserInputType.Null) errorCollector.AddXmlMissingPropError("UserInputType", name, "UserVariable");
                }
            }

            public class ComboItem
            {
                public string name = string.Empty;
                public string value = string.Empty;
                internal ComboItem Clone() { return new ComboItem() { name = this.name, value = this.value }; }
            }

            /// <summary>
            /// GetRequiredFields returns all the required variables
            /// </summary>
            /// <returns>A list of strings, containing the names of all the required variables.</returns>
            public List<string> GetRequiredVariables()
            {
                return requiredVariables.Select(x => x.readVar).ToList();
            }

            public void TakeUserInput(List<UserVariable> userInputs)
            {
                if (userInputs != null)
                {
                    List<UserVariable> duplicates = new List<UserVariable>();
                    foreach (UserVariable userVar in userVariables)
                    {
                        var uvs = userInputs.Where(x => x.name == userVar.name); // usually there should be one userInput for each UserVariable in the template
                        if (uvs.Any())                                           // but for PET we require user-input per package and reform
                        {
                            userVar.value = uvs.First().value;
                            userVar.description = uvs.First().description;
                            userVar.forEachValueDescription = uvs.First().forEachValueDescription;
                            userVar.refNo = uvs.First().refNo;
                            userVar.packageKey = uvs.First().packageKey;
                            foreach (var uv in uvs.Skip(1)) // no need for this if there isn't user-input per package and/or reform
                            {
                                UserVariable duv = new UserVariable(userVar)
                                {
                                    value = uv.value,
                                    description = uv.description,
                                    refNo = uv.refNo,
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

            internal UserVariable GetUserVariable(string name, string packageKey = null, int refNo = -1)
            {
                UserVariable userVariable = null;
                foreach (UserVariable uv in userVariables)
                {
                    if (uv.name != name) continue; // usually only this condition is relevant, but for PET we require user-variables ...
                    if (uv.refNo != -1 && uv.refNo != refNo) continue; // ... per reform, i.e. per alpha (cpi, custom, MII)
                    if (uv.packageKey != null && uv.packageKey != packageKey) continue;    // ... per package, i.e. per country and/or data
                    if (userVariable == null || // eiter not yet found ...
                       (userVariable.refNo != refNo && uv.refNo == refNo) || // ... or a better match
                       (userVariable.packageKey != packageKey && uv.packageKey == packageKey))
                    userVariable = uv; 
                }
                return userVariable;
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
                    exportDescriptionMode = this.exportDescriptionMode,
                    endUserFriendlyActionErrorMode = this.endUserFriendlyActionErrorMode,
                    doMultiColumns = this.doMultiColumns
                };
                foreach (CalculationLevel x in this.calculationLevels) inf.calculationLevels.Add(x.Clone());
                foreach (RequiredVariable x in this.requiredVariables) inf.requiredVariables.Add(x.Clone());
                foreach (OptionalVariable x in this.optionalVariables) inf.optionalVariables.Add(x.Clone());
                foreach (UserVariable x in this.userVariables) inf.userVariables.Add(x.Clone());
                inf.sdcMinObsAlternatives = new List<KeyValuePair<string, int>>();
                foreach (KeyValuePair<string, int> x in this.sdcMinObsAlternatives) inf.sdcMinObsAlternatives.Add(new KeyValuePair<string, int>(x.Key, x.Value));
                return inf;
            }

            internal bool ApiMerge(TemplateInfo apiTemplateInfo, bool keep, ErrorCollector errorCollector)
            {
                TemplateApi.Merge(ref description, apiTemplateInfo.description, "No description found", keep, errorCollector);
                TemplateApi.Merge(ref generalDescription, apiTemplateInfo.generalDescription, keep, errorCollector);
                TemplateApi.Merge(ref title, apiTemplateInfo.title, "No title found", keep, errorCollector);
                TemplateApi.Merge(ref subtitle, apiTemplateInfo.subtitle, keep, errorCollector);
                TemplateApi.Merge(ref _button, apiTemplateInfo._button, keep, errorCollector);
                TemplateApi.Merge(ref minFiles, apiTemplateInfo.minFiles, 1, keep, errorCollector);
                TemplateApi.Merge(ref maxFiles, apiTemplateInfo.maxFiles, 50, keep, errorCollector);
                TemplateApi.Merge(ref HideMainSelectorForSingleFilePackage, apiTemplateInfo.HideMainSelectorForSingleFilePackage, true, keep, errorCollector);
                TemplateApi.Merge(ref templateType, apiTemplateInfo.templateType, HardDefinitions.TemplateType.Default, keep, errorCollector);
                return true;
            }

            internal void CheckConsistency(ErrorCollector errorCollector)
            {
                foreach (KeyValuePair<string, int> moa in sdcMinObsAlternatives)
                {
                    string moaName = moa.Key; int moaValue = moa.Value;
                    if (moaValue == int.MinValue) errorCollector.AddXmlMissingPropError("MinObs", moaName, new List<string>() { "TemplateInfo", "SDCDefinition", "MinObsAlternative" });
                    if (string.IsNullOrEmpty(moaName)) errorCollector.AddXmlMissingPropError("Name", string.Empty, new List<string>() { "TemplateInfo", "SDCDefinition", "MinObsAlternative" });
                }
                errorCollector.CheckXmlDoubleDefError(from moa in sdcMinObsAlternatives select moa.Key, "Name", string.Empty, new List<string>() { "TemplateInfo", "SDCDefinition", "MinObsAlternatives" });

                foreach (CalculationLevel cl in calculationLevels) cl.CheckConsistency(errorCollector);
                errorCollector.CheckXmlDoubleDefError(from cl in calculationLevels select cl.name, "Name", string.Empty, "AdditionalCalculationLevels");

                foreach (RequiredVariable v in requiredVariables) v.CheckConsistency(errorCollector);
                errorCollector.CheckXmlDoubleDefError(from v in requiredVariables select v.name, "Name", string.Empty, "RequiredVariables");

                foreach (OptionalVariable v in optionalVariables) v.CheckConsistency(errorCollector);
                errorCollector.CheckXmlDoubleDefError(from v in optionalVariables select v.name, "Name", string.Empty, "OptionalVariables");

                foreach (UserVariable v in userVariables) v.CheckConsistency(errorCollector);
                // todo: for the time being out-commented because HHoT sends several instances of the user-variable "hhtype_selected" (should probably be reduced to one)
                //errorCollector.CheckXmlDoubleDefError(from v in userVariables select v.name, "Name", string.Empty, "UserVariables");
            }
        }

        public class Filter
        {
            public string name = string.Empty;
            public string formulaString = string.Empty;
            public bool reform = false;
            public List<Parameter> parameters = new List<Parameter>();
            internal Dictionary<string, Func<List<double>, bool>> func;
            public LocalMap localMap = null;

            public Filter() { }
            internal Filter(LocalMap _localMap = null) { localMap = _localMap; }

            /// <summary>
            /// Used to make anonymous hardcoded filters (see S8020 etc.)
            /// </summary>
            /// <param name="data"></param>
            /// <param name="_func"></param>
            public Filter(DataStatsHolder data, Func<List<double>, bool> _func, LocalMap _localMap)
            {
                func = new Dictionary<string, Func<List<double>, bool>> { { FuncID(data), _func } };
                localMap = _localMap;
            }

            /// <summary>
            /// User to copy a filter or make a reform filter.
            /// </summary>
            /// <param name="copyFilter"></param>
            /// <param name="refNo"></param>
            public Filter(Filter copyFilter, int refNo = -1)
            {
                if (copyFilter == null) return;
                if (!string.IsNullOrEmpty(copyFilter.name))
                    name = copyFilter.name + (refNo >= 0 ? HardDefinitions.Reform + refNo : string.Empty);
                reform = copyFilter.reform;
                formulaString = copyFilter.formulaString;
                parameters = new List<Parameter>();
                foreach (Parameter p in copyFilter.parameters)
                    parameters.Add(new Parameter(p, refNo));
                // see comment in EM_TemplateCalculater.CalculateProperties: here parameters are added for global-, page- and table-Filters
                if (refNo != -1) foreach (Parameter p in Template.GetDirectRefParameters(formulaString, parameters)) parameters.Add(new Parameter(p, refNo));
                localMap = copyFilter.localMap;
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

            internal bool ApiMerge(Filter apiFilter, bool keep, ErrorCollector errorCollector)
            {
                TemplateApi.Merge(ref reform, apiFilter.reform, keep, errorCollector);
                TemplateApi.Merge(ref formulaString, apiFilter.formulaString, keep, errorCollector);
                TemplateApi.MergeParameters(ref parameters, apiFilter.parameters, keep, errorCollector);
                return true;
            }

            internal void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags)
            {
                List<string> parentIdents = new List<string>(_parentIdents) { name }, parentTags = new List<string>(_parentTags) { "Filter" };
                if (parameters != null)
                {
                    foreach (Parameter parameter in parameters) parameter.CheckConsistency(errorCollector, parentIdents, parentTags);
                    parentTags.Add("Parameters");
                    errorCollector.CheckXmlDoubleDefError(from p in parameters select p.name, "Name", parentIdents, parentTags);
                }
            }
        }

        public class Action
        {
            public string name = string.Empty;
            public string _calculationLevel = string.Empty;
            public HardDefinitions.CalculationType calculationType = HardDefinitions.CalculationType.NA;
            public string outputVar = string.Empty;
            public string formulaString = string.Empty;
            public List<Parameter> parameters = new List<Parameter>();
            internal Filter filter = null;
            public bool? _reform = null;
            public bool? _saveResult = null;
            public bool? _blendParameters = null; // this could be enum (overwrite, blend, add) instead of bool - or is this a parameter attribute?
            public bool? _important = null; // this could be enum (overwrite, blend, add) instead of bool - or is this a parameter attribute?
            internal Func<List<double>, double> func;
            internal double result = double.NaN;
            public LocalMap localMap = null;

            public string CalculationLevel { get { return string.IsNullOrEmpty(_calculationLevel) ? HardDefinitions.DefaultCalculationLevels.INDIVIDUAL : _calculationLevel; } }
            public bool Reform { get { return !_reform.HasValue || _reform.Value; } }  // default is true
            public bool SaveResult { get { return !_saveResult.HasValue || _saveResult.Value; } }  // default is true
            public bool BlendParameters { get { return !_blendParameters.HasValue || _blendParameters.Value; } }  // default is true
            public bool Important { get { return _important.HasValue && _important.Value; } }  // default is false

            public Action() { }
            internal Action(LocalMap _localMap) { localMap = _localMap; }
            public Action(Action copyAction, int refNo = -1)
            {
                name = copyAction.name;
                _calculationLevel = copyAction._calculationLevel;
                calculationType = copyAction.calculationType;
                outputVar = copyAction.outputVar + (refNo >= 0 && copyAction.Reform ? HardDefinitions.Reform + refNo : string.Empty);
                formulaString = copyAction.formulaString;
                parameters = new List<Parameter>();

                foreach (Parameter p in copyAction.parameters)
                    parameters.Add(new Parameter(p, refNo));
                // see comment in EM_TemplateCalculater.CalculateProperties: here parameters are added for global-, page- and table-Actions
                if (refNo != -1) foreach (Parameter p in Template.GetDirectRefParameters(formulaString, parameters)) parameters.Add(new Parameter(p, refNo));

                if (copyAction.filter != null)
                    filter = new Filter(copyAction.filter, refNo);
                _reform = refNo == -1 ? copyAction._reform : false; // is this already a reform? if so, it should not be re-reformed! ;)
                _blendParameters = copyAction._blendParameters;
                _important = copyAction._important;
                _saveResult = copyAction._saveResult;
                localMap = copyAction.localMap;
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

            internal bool GetStringParameter(string parName, out string sVal)
            {
                sVal = null;
                if (!parameters.Any(x => x.name == parName)) return false;
                sVal = parameters.First(x => x.name == parName).stringValue; return true;
            }

            internal bool GetEquivBandsParameters(string parName, out Dictionary<string, double> sdVal)
            {
                sdVal = new Dictionary<string, double>();
                if (parameters.FirstOrDefault(x => x.name == parName) == null) return false;
                bool allOK = true;
                foreach (Parameter p in parameters.Where(x => x.name == parName))
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

            internal bool ApiMerge(Action apiAction, bool keep, ErrorCollector errorCollector)
            {
                TemplateApi.Merge(ref _calculationLevel, apiAction._calculationLevel, keep, errorCollector);
                TemplateApi.Merge(ref calculationType, apiAction.calculationType, HardDefinitions.CalculationType.NA, keep, errorCollector);
                TemplateApi.Merge(ref outputVar, apiAction.outputVar, keep, errorCollector);
                TemplateApi.Merge(ref _reform, apiAction._reform, keep, errorCollector);
                TemplateApi.Merge(ref _saveResult, apiAction._saveResult, keep, errorCollector);
                TemplateApi.Merge(ref _blendParameters, apiAction._blendParameters, keep, errorCollector);
                TemplateApi.Merge(ref _important, apiAction._important, keep, errorCollector);
                TemplateApi.Merge(ref formulaString, apiAction.formulaString, keep, errorCollector);
                TemplateApi.MergeParameters(ref parameters, apiAction.parameters, keep, errorCollector);
                return true;
            }

            internal void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags)
            {
                List<string> parentIdents = new List<string>(_parentIdents) { name }, parentTags = new List<string>(_parentTags) { "Action" };

                if (filter != null) filter.CheckConsistency(errorCollector, parentIdents, parentTags);

                if (parameters != null)
                {
                    if (calculationType != HardDefinitions.CalculationType.CreateEquivalized) // todo (optimise): this is an inconsistency: CreateEquivalized allows for a parameter that only has VarName
                        foreach (Parameter parameter in parameters) parameter.CheckConsistency(errorCollector, parentIdents, parentTags);
                    parentTags.Add("Parameters");

                    var parList = from p in parameters
                                  where p.name.ToLower() != EM_TemplateCalculator.PAR_EQUIVSCALEBAND.ToLower() // todo (optimise): again an inconsistency: this parameter-name is not unique
                                  select p.name;
                    errorCollector.CheckXmlDoubleDefError(parList, "Name", parentIdents, parentTags);
                }
            }
        }

        public class Parameter
        {
            public enum Source { DEFAULT, BASELINE, PREVIOUS_REFORM }
            public string name = string.Empty;
            public string variableName = string.Empty;
            public double numericValue = double.NaN;
            public bool? boolValue = null;
            public string stringValue = null;
            public Source? _source = null;
            public Source source { get { return _source ?? Source.DEFAULT; } }

            public Parameter() { }
            public Parameter(Parameter copyParameter, int refNo = -1)
            {
                name = copyParameter.name;
                variableName = copyParameter.variableName;
                numericValue = copyParameter.numericValue;
                boolValue = copyParameter.boolValue;
                stringValue = copyParameter.stringValue;
                _source = copyParameter._source;
                SourceAdaptVariableName(refNo);
            }

            internal void SourceAdaptVariableName(int refNo)
            {
                if (string.IsNullOrEmpty(variableName)) return;
                switch (source)
                {
                    case Source.DEFAULT: if (refNo >= 0) Adapt(refNo); break;
                    case Source.BASELINE: break;
                    case Source.PREVIOUS_REFORM: if (refNo > 0) Adapt(refNo - 1); break; // note: yields in baseline for 1st reform
                }
                void Adapt(int r)
                {
                    int i = variableName.IndexOf(HardDefinitions.Reform);
                    if (i >= 0) variableName = variableName.Substring(0, i); // this should not be necessary, but to be on the save side
                    variableName += HardDefinitions.Reform + r;
                } 
            }

            internal Parameter Clone()
            {
                return new Parameter()
                {
                    name = this.name,
                    variableName = this.variableName,
                    numericValue = this.numericValue,
                    boolValue = this.boolValue,
                    stringValue = this.stringValue,
                    _source = this._source
                };
            }

            internal void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags)
            {
                List<string> parentIdents = new List<string>(_parentIdents) { name }, parentTags = new List<string>(_parentTags) { "Parameter" };

                if (string.IsNullOrEmpty(name)) errorCollector.AddXmlMissingPropError("Name", parentIdents, parentTags);
                if (string.IsNullOrEmpty(variableName) && double.IsNaN(numericValue) && boolValue == null && stringValue == null)
                    errorCollector.AddXmlMissingPropError("VarName|BoolValue|NumericValue|StringValue", parentIdents, parentTags);
            }
        }

        public class Page
        {
            public string name = string.Empty;
            public string title { get { return _title ?? name; } set { _title = value; } } private string _title = null;
            public string subtitle = string.Empty;
            public VisualElement button = new VisualElement();
            public string html = string.Empty;
            public string description = string.Empty;
            public bool active = true;
            public bool visible = true;
            public bool perReform = false;
            internal List<Table> tables = new List<Table>();
            internal List<Filter> filters = new List<Filter>();
            internal List<Action> actions = new List<Action>();
            public LocalMap localMap = null;

            public Page() { }
            internal Page(LocalMap _localMap = null) { localMap = _localMap; }

            public class VisualElement
            {
                public string name = string.Empty;
                public bool? strong = null;
                public string foregroundColour = string.Empty;
                public string backgroundColour = string.Empty;
                public string textAlign = string.Empty;
                public string tooltip = null;
                public string _title = null;
                public string title { get { return _title ?? name; } set { _title = value; } }

                internal void Copy(VisualElement ve)
                {
                    name = ve.name;
                    strong = ve.strong;
                    foregroundColour = ve.foregroundColour;
                    backgroundColour = ve.backgroundColour;
                    textAlign = ve.textAlign;
                    tooltip = ve.tooltip;
                    title = ve.title;
                }

                internal VisualElement Clone()
                {
                    VisualElement v = new VisualElement();
                    v.Copy(this);
                    return v;
                }
                internal bool ApiMerge(VisualElement apiPage, bool keep, ErrorCollector errorCollector)
                {
                    TemplateApi.Merge(ref foregroundColour, apiPage.foregroundColour, keep, errorCollector);
                    TemplateApi.Merge(ref backgroundColour, apiPage.backgroundColour, keep, errorCollector);
                    TemplateApi.Merge(ref textAlign, apiPage.textAlign, keep, errorCollector);
                    TemplateApi.Merge(ref tooltip, apiPage.tooltip, keep, errorCollector);
                    TemplateApi.Merge(ref name, apiPage.name, keep, errorCollector);
                    TemplateApi.Merge(ref strong, apiPage.strong, keep, errorCollector);
                    TemplateApi.Merge(ref _title, apiPage._title, keep, errorCollector);
                    return true;
                }
            }

            public class Table
            {
                public string name = string.Empty;
                public string title { get { return _title ?? name; } set { _title = value; } } private string _title = null;
                public string subtitle = string.Empty;
                public string description = string.Empty;
                public string stringFormat = string.Empty;
                public bool active = true;
                public bool visible = true;
                public bool perReform = false;
                public HardDefinitions.ColumnGrouping columnGrouping = HardDefinitions.ColumnGrouping.SystemFirst;
                internal List<Column> columns = new List<Column>();
                internal List<Column> reformColumns = new List<Column>();
                internal List<Row> rows = new List<Row>();
                internal List<Cell> cells = new List<Cell>();
                internal List<Cell> reformCells = new List<Cell>();
                internal List<Filter> filters = new List<Filter>();
                internal List<Action> actions = new List<Action>();
                internal Action cellAction = null;
                public DisplayResults.DisplayPage.DisplayTable.DisplayGraph graph;
                internal SDCDefinition sdcDefinition = new SDCDefinition();
                public LocalMap localMap = null;

                public Table() { }
                internal Table(LocalMap _localMap = null) { localMap = _localMap; cellAction = new Action(localMap); }

                public class TableElement : VisualElement
                {
                    public string stringFormat = null;
                    public Action cellAction = null;
                    internal SDCDefinition sdcDefinition = new SDCDefinition();

                    internal virtual bool ApiMerge(TableElement apiElement, bool keep, ErrorCollector errorCollector)
                    {
                        TemplateApi.Merge(ref strong, apiElement.strong, keep, errorCollector);
                        TemplateApi.Merge(ref foregroundColour, apiElement.foregroundColour, keep, errorCollector);
                        TemplateApi.Merge(ref backgroundColour, apiElement.backgroundColour, keep, errorCollector);
                        TemplateApi.Merge(ref stringFormat, apiElement.stringFormat, keep, errorCollector);
                        TemplateApi.Merge(ref tooltip, apiElement.tooltip, keep, errorCollector);
                        return true;
                    }

                    internal virtual void CheckConsistency(ErrorCollector errorCollector, List<string> parentIdents, List<string> parentTags, bool reform = false)
                    {
                        if (cellAction != null) cellAction.CheckConsistency(errorCollector, parentIdents, parentTags);
                        sdcDefinition.CheckConsistency(errorCollector, parentIdents, parentTags);
                    }

                    internal void Copy(TableElement te)
                    {
                        base.Copy(te);
                        stringFormat = te.stringFormat;
                        cellAction = te.cellAction?.Clone();
                        sdcDefinition = te.sdcDefinition?.Clone();
                    }

                    public new TableElement Clone()
                    {
                        TableElement t = new TableElement();
                        t.Copy(this);
                        return t;
                    }
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

                    internal void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags)
                    {
                    }
                }

                public class Column : TableElement
                {
                    public bool isVisible = true;
                    public bool hasSeparatorBefore = false;
                    public bool hasSeparatorAfter = false;
                    public string tiesWith = null; // this is used to visually tie a reform column to a base column
                    public string multiNo = null;

                    private void Copy(Column c)
                    {
                        base.Copy(c);
                        isVisible = c.isVisible;
                        hasSeparatorAfter = c.hasSeparatorAfter;
                        hasSeparatorBefore = c.hasSeparatorBefore;
                        tiesWith = c.tiesWith;
                        multiNo = c.multiNo;
                    }
                    public new Column Clone()
                    {
                        Column c = new Column();
                        c.Copy(this);
                        return c;
                    }

                    internal override bool ApiMerge(TableElement apiElement, bool keep, ErrorCollector errorCollector)
                    {
                        Column apiColumn = apiElement as Column;
                        TemplateApi.Merge(ref _title, apiColumn._title, keep, errorCollector);
                        TemplateApi.Merge(ref isVisible, apiColumn.isVisible, true, keep, errorCollector);
                        TemplateApi.Merge(ref hasSeparatorBefore, apiColumn.hasSeparatorBefore, false, keep, errorCollector);
                        TemplateApi.Merge(ref hasSeparatorAfter, apiColumn.hasSeparatorAfter, false, keep, errorCollector);
                        TemplateApi.Merge(ref tiesWith, apiColumn.tiesWith, keep, errorCollector);
                        TemplateApi.Merge(ref multiNo, apiColumn.multiNo, keep, errorCollector);
                        return base.ApiMerge(apiColumn, keep, errorCollector);
                    }

                    internal override void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags, bool reform = false)
                    {
                        string tag = (reform ? "Reform" : string.Empty) + "Column";
                        List<string> parentIdents = new List<string>(_parentIdents) { ThisOrThatIdent(tag, name, title) };
                        List<string> parentTags = new List<string>(_parentTags) { tag };
                        if (!reform && !string.IsNullOrEmpty(tiesWith))
                            errorCollector.AddError($"<Column> '{string.Join("|", parentIdents)}': <TiesWith> is only applicable for <ReformColumn>.");
                        base.CheckConsistency(errorCollector, parentIdents, parentTags);
                    }
                }

                public class Row : TableElement
                {
                    public bool isVisible = true;
                    public bool hasSeparatorBefore = false;
                    public bool hasSeparatorAfter = false;
                    public bool forEachDataRow = false;
                    public string forEachValueOf = null;
                    public int forEachValueMaxCount = int.MaxValue;
                    public Dictionary<double, string> forEachValueDescriptions = new Dictionary<double, string>();

                    internal override bool ApiMerge(TableElement apiElement, bool keep, ErrorCollector errorCollector)
                    {
                        Row apiRow = apiElement as Row;
                        TemplateApi.Merge(ref _title, apiRow._title, keep, errorCollector);
                        TemplateApi.Merge(ref isVisible, apiRow.isVisible, true, keep, errorCollector);
                        TemplateApi.Merge(ref hasSeparatorBefore, apiRow.hasSeparatorBefore, false, keep, errorCollector);
                        TemplateApi.Merge(ref hasSeparatorAfter, apiRow.hasSeparatorAfter, false, keep, errorCollector);
                        TemplateApi.Merge(ref forEachDataRow, apiRow.forEachDataRow, false, keep, errorCollector);
                        TemplateApi.Merge(ref forEachValueOf, apiRow.forEachValueOf, keep, errorCollector);
                        TemplateApi.Merge(ref forEachValueMaxCount, apiRow.forEachValueMaxCount, keep, errorCollector);
                        if (apiRow.forEachValueDescriptions != null && apiRow.forEachValueDescriptions.Any() && (forEachValueDescriptions == null || !forEachValueDescriptions.Any() || !keep)) forEachValueDescriptions = apiRow.forEachValueDescriptions;
                        return base.ApiMerge(apiRow, keep, errorCollector);
                    }

                    internal void Copy(Row r)
                    {
                        base.Copy(r);
                        isVisible = r.isVisible;
                        hasSeparatorAfter = r.hasSeparatorAfter;
                        hasSeparatorBefore = r.hasSeparatorBefore;
                        forEachDataRow = r.forEachDataRow;
                        forEachValueOf = r.forEachValueOf;
                        forEachValueMaxCount = r.forEachValueMaxCount;
                        if (r.forEachValueDescriptions != null) foreach(var d in r.forEachValueDescriptions) forEachValueDescriptions.Add(d.Key, d.Value);
                    }
                    public new Row Clone()
                    {
                        Row r = new Row();
                        r.Copy(this);
                        return r;
                    }

                    internal override void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags, bool reform = false)
                    {
                        List<string> parentIdents = new List<string>(_parentIdents) { ThisOrThatIdent("Row", name, title) };
                        List<string> parentTags = new List<string>(_parentTags) { "Row" };
                        base.CheckConsistency(errorCollector, parentIdents, parentTags);
                    }
                }

                public class Cell : TableElement
                {
                    public string rowName = null;
                    public string colName = null;
                    internal List<string> secondarySdcGroups = new List<string>();

                    internal void Copy(Cell c)
                    {
                        base.Copy(c);
                        rowName = c.rowName;
                        colName = c.colName;
                    }
                    public new Cell Clone()
                    {
                        Cell c = new Cell();
                        c.Copy(this);
                        return c;
                    }

                    internal override bool ApiMerge(TableElement apiElement, bool keep, ErrorCollector errorCollector)
                    {
                        Cell apiCell = apiElement as Cell;
                        TemplateApi.Merge(ref rowName, apiCell.rowName, keep, errorCollector);
                        TemplateApi.Merge(ref colName, apiCell.colName, keep, errorCollector);
                        return base.ApiMerge(apiCell, keep, errorCollector);
                    }
                    internal override void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags, bool reform = false)
                    {
                        List<string> parentIdents = new List<string>(_parentIdents) { name };
                        List<string> parentTags = new List<string>(_parentTags) { (reform ? "Reform" : string.Empty) + "Cell" };

                        if (string.IsNullOrEmpty(rowName)) errorCollector.AddXmlMissingPropError("RowName", parentIdents, parentTags);
                        if (string.IsNullOrEmpty(colName)) errorCollector.AddXmlMissingPropError("ColName", parentIdents, parentTags);
                        base.CheckConsistency(errorCollector, parentIdents, parentTags);
                    }
                }

                public Table Clone()
                {
                    Table t = new Table(this.localMap)
                    {
                        name = this.name,
                        title = this.title,
                        subtitle = this.subtitle,
                        description = this.description,
                        active = this.active,
                        perReform = this.perReform,
                        columnGrouping = this.columnGrouping,
                        stringFormat = this.stringFormat,
                        cellAction = this.cellAction.Clone(),
                        graph = this.graph, // todo: check this comment: Important! The graph should never be changed by the TemplateCalculator once read! (hence not cloned)
                        sdcDefinition = this.sdcDefinition.Clone()
                    };
                    foreach (Column x in this.columns) t.columns.Add(x.Clone());
                    foreach (Column x in this.reformColumns) t.reformColumns.Add(x.Clone());
                    foreach (Row x in this.rows) t.rows.Add(x.Clone());
                    foreach (Cell x in this.cells) t.cells.Add(x.Clone());
                    foreach (Cell x in this.reformCells) t.reformCells.Add(x.Clone());
                    foreach (Action x in this.actions) t.actions.Add(x.Clone());
                    foreach (Filter x in this.filters) t.filters.Add(x.Clone());
                    return t;
                }

                internal bool ApiMerge(Table apiTable, bool keep, ErrorCollector errorCollector)
                {
                    TemplateApi.Merge(ref _title, apiTable._title, keep, errorCollector);
                    TemplateApi.Merge(ref subtitle, apiTable.subtitle, keep, errorCollector);
                    TemplateApi.Merge(ref description, apiTable.description, keep, errorCollector);
                    TemplateApi.Merge(ref active, apiTable.active, true, keep, errorCollector);
                    TemplateApi.Merge(ref perReform, apiTable.perReform, false, keep, errorCollector);
                    TemplateApi.Merge(ref stringFormat, apiTable.stringFormat, keep, errorCollector);
                    TemplateApi.Merge(ref columnGrouping, apiTable.columnGrouping, HardDefinitions.ColumnGrouping.SystemFirst, keep, errorCollector);
                    return true;
                }

                internal void CheckConsistency(ErrorCollector errorCollector, List<string> _parentIdents, List<string> _parentTags)
                {
                    List<string> parentIdents = new List<string>(_parentIdents) { ThisOrThatIdent("Table", name, title) };

                    if (cellAction != null) cellAction.CheckConsistency(errorCollector, parentIdents, new List<string>());

                    foreach (Column column in columns) column.CheckConsistency(errorCollector, parentIdents, new List<string>());
                    errorCollector.CheckXmlDoubleDefError(from c in columns select c.name, "Name", parentIdents, new List<string> { "Table", "Columns" });

                    foreach (Row row in rows) row.CheckConsistency(errorCollector, parentIdents, new List<string>());
                    errorCollector.CheckXmlDoubleDefError(from r in rows select r.name, "Name", parentIdents, new List<string> { "Table", "Rows" });

                    foreach (Cell cell in cells) { cell.CheckConsistency(errorCollector, parentIdents, new List<string>()); CheckCellIds(cell, false); }
                    errorCollector.CheckXmlDoubleDefError(from c in cells select c.name, "Name", parentIdents, new List<string> { "Table", "Cells" });

                    foreach (Column reformColumn in reformColumns)
                    {
                        reformColumn.CheckConsistency(errorCollector, parentIdents, new List<string>(), true);
                        if (!string.IsNullOrEmpty(reformColumn.tiesWith) && !(from c in columns select c.name.ToLower()).Contains(reformColumn.tiesWith.ToLower()))
                            errorCollector.AddError($"<ReformColumn> '{string.Join("|", parentIdents)}': " + $"<TiesWith> '{reformColumn.tiesWith}': no <Column> with this <Name> defined."); ;
                    }
                    errorCollector.CheckXmlDoubleDefError(from c in reformColumns select c.name, "Name", parentIdents, new List<string> { "Table", "ReformColumns" });

                    foreach (Cell cell in reformCells) { cell.CheckConsistency(errorCollector, parentIdents, new List<string>(), true); CheckCellIds(cell, true); }
                    errorCollector.CheckXmlDoubleDefError(from c in reformCells select c.name, "Name", parentIdents, new List<string> { "Table", "ReformCells" });

                    foreach (Action action in actions) action.CheckConsistency(errorCollector, parentIdents, new List<string>());
                    errorCollector.CheckXmlDoubleDefError(from a in actions select a.name, "Name", parentIdents, new List<string> { "Table", "Actions" });

                    foreach (Filter filter in filters)
                    {
                        if (string.IsNullOrEmpty(filter.name)) errorCollector.AddError($"<Table><Filters> '{string.Join("|", parentIdents)}': unnamed <Filter> found.");
                        filter.CheckConsistency(errorCollector, parentIdents, new List<string>());
                    }
                    errorCollector.CheckXmlDoubleDefError(from f in filters select f.name, "Name", parentIdents, new List<string> { "Table", "Filters" });

                    void CheckCellIds(Cell cell, bool reform)
                    {
                        if (!string.IsNullOrEmpty(cell.rowName) && !(from r in rows select r.name.ToLower()).Contains(cell.rowName.ToLower()))
                            errorCollector.AddError($"<{(reform ? "Reform" : string.Empty)}Cell> '{string.Join("|", parentIdents)}': unknown <RowName> '{cell.rowName}'.");
                        if (!string.IsNullOrEmpty(cell.colName) && !(from c in (reform ? reformColumns : columns) select c.name.ToLower()).Contains(cell.colName.ToLower()))
                            errorCollector.AddError($"<{(reform ? "Reform" : string.Empty)}Cell> '{string.Join("|", parentIdents)}': unknown <ColName> '{cell.colName}'.");
                    }
                }
            }

            public Page Clone()
            {
                Page p = new Page(this.localMap)
                {
                    name = this.name,
                    title = this.title,
                    subtitle = this.subtitle,
                    button = this.button?.Clone(),
                    html = this.html,
                    description = this.description,
                    active = this.active,
                    perReform = this.perReform
                };
                foreach (Table t in this.tables) p.tables.Add(t.Clone());
                foreach (Action a in this.actions) p.actions.Add(a.Clone());
                foreach (Filter f in this.filters) p.filters.Add(f.Clone());
                return p;
            }

            internal bool ApiMerge(Page apiPage, bool keep, ErrorCollector errorCollector)
            {
                TemplateApi.Merge(ref _title, apiPage._title, keep, errorCollector);
                TemplateApi.Merge(ref subtitle, apiPage.subtitle, keep, errorCollector);
                button.ApiMerge(apiPage.button, keep, errorCollector);
                TemplateApi.Merge(ref description, apiPage.description, keep, errorCollector);
                TemplateApi.Merge(ref active, apiPage.active, true, keep, errorCollector);
                TemplateApi.Merge(ref perReform, apiPage.perReform, false, keep, errorCollector);
                return true;
            }

            internal void CheckConsistency(ErrorCollector errorCollector)
            {
                List<string> parentIdents = new List<string>() { ThisOrThatIdent("Page", name, title) };

                foreach (Table table in tables) table.CheckConsistency(errorCollector, parentIdents, new List<string>());
                errorCollector.CheckXmlDoubleDefError(from t in tables select t.name, "Name", parentIdents, new List<string> { "Tables" });

                foreach (Action action in actions) action.CheckConsistency(errorCollector, parentIdents, new List<string>());
                errorCollector.CheckXmlDoubleDefError(from a in actions select a.name, "Name", parentIdents, new List<string> { "Page", "Actions" });

                foreach (Filter filter in filters)
                {
                    if (string.IsNullOrEmpty(filter.name)) errorCollector.AddError($"<Page><Filters> '{string.Join("|", parentIdents)}': unnamed <Filter> found.");
                    filter.CheckConsistency(errorCollector, parentIdents, new List<string>());
                }
                errorCollector.CheckXmlDoubleDefError(from f in filters select f.name, "Name", parentIdents, new List<string> { "Page", "Filters" });
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

        internal void CheckConsistency(ErrorCollector errorCollector)
        {
            try
            {
                info.CheckConsistency(errorCollector);

                foreach (Filter globalFilter in globalFilters)
                {
                    if (string.IsNullOrEmpty(globalFilter.name)) errorCollector.AddError($"<Globals><Filters>: unnamed <Filter> found.");
                    globalFilter.CheckConsistency(errorCollector, new List<string>(), new List<string> { "Globals" });
                }
                errorCollector.CheckXmlDoubleDefError(from f in globalFilters select f.name, "Name", new List<string>(), new List<string> { "Globals", "Filters" });

                foreach (Action globalAction in globalActions) globalAction.CheckConsistency(errorCollector, new List<string>(), new List<string> { "Globals" });
                errorCollector.CheckXmlDoubleDefError(from a in globalActions select a.name, "Name", new List<string>(), new List<string> { "Globals", "Actions" });

                foreach (Page page in pages) page.CheckConsistency(errorCollector);
                errorCollector.CheckXmlDoubleDefError(from p in pages select p.name, "Name", new List<string>(), new List<string> { "Globals", "Pages" });
            }
            catch (Exception exception)
            {
                errorCollector.AddError("Unexpected error while checking Template for consistency:" + Environment.NewLine + exception.Message);
            }
        }

        private static string ThisOrThatIdent(string identName, string sThis, string sThat)
        {
            if (!string.IsNullOrEmpty(sThis)) return sThis;
            if (!string.IsNullOrEmpty(sThat)) return sThat;
            return $"unnamed {identName}";
        }

        // TemplateInfo allows for named minObsAlternatives (i.e. general minimums applicable for more than one Row, Column, etc.), e.g. PERCENTAGE=100
        // these are spread here to the applying elements
        internal void ReplaceNamedSdcMinObsAlternatives(ErrorCollector errorCollector)
        {
            foreach (Page page in pages)
                foreach (Page.Table table in page.tables)
                {
                    if (table.sdcDefinition.minObsAlternative == null && table.sdcDefinition.minObsAlternativeName != null)
                    {
                        string pageTableIdent = $"{ThisOrThatIdent("Page", page.title, page.name)}-{ThisOrThatIdent("Table", table.title, table.name)}";
                        Replace(ref table.sdcDefinition, "Table");
                        foreach (Page.Table.Row row in table.rows) Replace(ref row.sdcDefinition, "Row", ThisOrThatIdent("Row", row.title, row.name));
                        foreach (Page.Table.Column column in table.columns) Replace(ref column.sdcDefinition, "Column", ThisOrThatIdent("Column", column.title, column.name));
                        foreach (Page.Table.Column column in table.reformColumns) Replace(ref column.sdcDefinition, "ReformColumn", ThisOrThatIdent("ReformColumn", column.title, column.name));
                        foreach (Page.Table.Cell cell in table.cells) Replace(ref cell.sdcDefinition, "Cell", cell.name);
                        foreach (Page.Table.Cell cell in table.reformCells) Replace(ref cell.sdcDefinition, "ReformCell", cell.name);

                        void Replace(ref Page.Table.SDCDefinition sdcDefinition, string eleTag, string eleIdent = null)
                        {
                            if (string.IsNullOrEmpty(sdcDefinition.minObsAlternativeName)) return;
                            if (int.TryParse(sdcDefinition.minObsAlternativeName, out int i))
                                sdcDefinition.minObsAlternative = i;
                            else
                            {
                                string minObsAlternativeName = sdcDefinition.minObsAlternativeName;
                                var moas = (from moa in info.sdcMinObsAlternatives where moa.Key.ToLower() == minObsAlternativeName.ToLower() select moa.Value);
                                if (moas.Any()) { sdcDefinition.minObsAlternative = moas.First(); return; }
                                
                                if (string.IsNullOrEmpty(eleIdent)) eleIdent = pageTableIdent; else eleIdent = $"{pageTableIdent}|{eleIdent}";
                                errorCollector.AddError($"<{eleTag}><SDCDefinition> '{eleIdent}':" + Environment.NewLine +
                                                        $"Unknown <MinObsAlternative> '{sdcDefinition.minObsAlternativeName}'.");
                            }
                        }
                    }
                }
        }

        // find direct-reference-usages of DATA_VAR/USR_VAR/SAVED_VAR in formula, e.g. DATA_VAR[@yem] without providing a parameter { Name="yem", VarName="yem" }
        internal static List<Parameter> GetDirectRefParameters(string formula, List<Template.Parameter> parameters)
        {
            List<Parameter> shortRef = new List<Parameter>();
            foreach (string XXX_VAR in new List<string>() { HardDefinitions.FormulaParameter.DATA_VAR,
                                                            HardDefinitions.FormulaParameter.USR_VAR,
                                                            HardDefinitions.FormulaParameter.SAVED_VAR })
            {
                int pos = formula.IndexOf(XXX_VAR);
                while (pos > -1)
                {
                    int startpos = pos + XXX_VAR.Length + 1; // +1 to take "@" into account
                    int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                    if (startpos > formula.Length || endpos < startpos) break; // error will be issued by PrepareFormula
                    string parName = formula.Substring(startpos, endpos - startpos);
                    if (!parameters.Any(x => x.name == parName)) shortRef.Add(new Parameter() { name = parName, variableName = parName });
                    pos = formula.IndexOf(XXX_VAR, endpos);
                }
            }
            return shortRef;
        }
    }
}
