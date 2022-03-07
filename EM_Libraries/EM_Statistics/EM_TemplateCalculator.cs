using EM_Common;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Text;

namespace EM_Statistics
{
    public partial class EM_TemplateCalculator
    {
        private readonly List<SystemInfo> baselineSystemInfos = new List<SystemInfo>(); // base-only, base-reform: info about base-system (i.e. List-count = 1)
                                                                                        // multi: info about each system (i.e. List-Count = number of systems)
        private readonly List<SystemInfo> reformSystemInfos = new List<SystemInfo>();   // base-reform: info about reform-systems, empty else

        private readonly List<Dictionary<string, DataStatsHolder>> dataAndStats = // base-only, base-reform: 'the' data and statistics (i.e. List-count = 1)
                     new List<Dictionary<string, DataStatsHolder>>();             // multi: data and statistics for each system (i.e. List-Count = number of systems)
                                                                                  // dictionary: d&s per calculation-level (ind, hh, ...), key: level
        private readonly DisplayResults displayResults = new DisplayResults();

        private readonly Template template;
        private readonly string packageKey;
        private ErrorCollector errorCollector = new ErrorCollector();

        // replicate the default calculation levels here, just for readability's sake! 
        private const string IND = HardDefinitions.DefaultCalculationLevels.INDIVIDUAL;
        private const string HH = HardDefinitions.DefaultCalculationLevels.HOUSEHOLD;

        private const string constMakeDouble = "1.0 * ";

        /// <summary>
        /// This function runs a dummy dynamic expression to "warm up" the dynamic expression engine. 
        /// </summary>
        public static void WarmUp()
        {
            new System.Threading.Thread(() =>
            {
                System.Threading.Thread.CurrentThread.IsBackground = true;
                double result = WarmUpThread();
            }).Start();
        }

        private static double WarmUpThread()
        {
            ParameterExpression[] DATA_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") };
            Func<List<double>, double> func = (Func<List<double>, double>)DynamicExpressionParser.ParseLambda(DATA_VAR, typeof(double), "3.0 / 2.0", Array.Empty<object>()).Compile();
            return func(null);
        }

        public EM_TemplateCalculator(Template _template = null, string _packageKey = null)
        {
            // If template exists, clone it to avoid affecting the original (e.g. due to saving compiled filters), 
            // else make a new one (this is for programmatic calls to statistical function directly from other plug-ins, e.g. OC_Runner.cs)

            template = _template == null ? new Template(true) : _template.Clone();
            packageKey = _packageKey;
        }

        /// <summary>
        /// Prepares all statistics using output in files or memory
        /// </summary>
        /// <param name="allFilenames">The output filenames</param>
        /// <param name="_errorCollector">A collector of all errors</param>
        /// <returns>True if the statistics were build successfuly, false if there was an exception</returns>
        public bool Prepare(List<string> allFilenames, out ErrorCollector _errorCollector)
        {
            errorCollector = new ErrorCollector(); _errorCollector = errorCollector;

            if (template == null) { errorCollector.AddError("No template available."); return false; }
            template.CheckConsistency(errorCollector); template.ReplaceNamedSdcMinObsAlternatives(errorCollector);

            if (allFilenames == null || allFilenames.Count < 1 || (template.info.templateType == HardDefinitions.TemplateType.BaselineReform && allFilenames.Count < 2))
            { errorCollector.AddError("Insufficient number of files."); return false; };

            switch (template.info.templateType)
            {
                case HardDefinitions.TemplateType.Default:
                    baselineSystemInfos.Add(new SystemInfo(allFilenames[0]));
                    break;

                case HardDefinitions.TemplateType.Multi:
                    for (int dasNo = 0; dasNo < allFilenames.Count; dasNo++) baselineSystemInfos.Add(new SystemInfo(allFilenames[dasNo]));
                    break;
                case HardDefinitions.TemplateType.BaselineReform:
                    baselineSystemInfos.Add(new SystemInfo(allFilenames[0]));
                    foreach (string reformFilename in allFilenames.Skip(1)) reformSystemInfos.Add(new SystemInfo(reformFilename));
                    break;
            }
            // prepare the display info
            displayResults.info.title = PrettyInfoProvider.GetPrettyText(template.info, template.info.title, baselineSystemInfos[0], reformSystemInfos, packageKey);
            displayResults.info.subtitle = PrettyInfoProvider.GetPrettyText(template.info, template.info.subtitle, baselineSystemInfos[0], reformSystemInfos, packageKey);
            displayResults.info.button = PrettyInfoProvider.GetPrettyText(template.info, template.info.button, baselineSystemInfos[0], reformSystemInfos, packageKey);
            displayResults.info.description = template.info.generalDescription;
            displayResults.info.exportDescriptionMode = template.info.exportDescriptionMode;

            displayResults.prepared = true;

            return true;
        }

        /// <summary>
        /// Calculates all statistics using output in files or memory
        /// </summary>
        /// <param name="allMemoryData">The microdata in memory</param>
        /// <returns>True if the statistics were built successfuly, false if there was an exception</returns>
        public bool CalculateStatistics(List<StringBuilder> allMemoryData = null)
        {
            try { return Instantiate(allMemoryData) && CompileTemplate(); }
            catch (Exception exception) { errorCollector.AddError(exception.Message); return false; }
        }

        private bool Instantiate(List<StringBuilder> allMemoryData)
        {
            // make sure the template has the default calculation levels, to maintain support of older templates!
            if (!template.info.calculationLevels.Exists(x => x.name == HH))
                template.info.calculationLevels.Insert(0, new Template.TemplateInfo.CalculationLevel() { name = HH, groupingVar = HardDefinitions.idHH });
            if (!template.info.calculationLevels.Exists(x => x.name == IND))
                template.info.calculationLevels.Insert(0, new Template.TemplateInfo.CalculationLevel() { name = IND, groupingVar = HardDefinitions.idPerson });

            switch (template.info.templateType)
            {
                case HardDefinitions.TemplateType.Default:
                    if (allMemoryData != null && allMemoryData.Any()) baselineSystemInfos[0].SetMemoryData(allMemoryData[0]);
                    if (!ReadData(out Dictionary<string, DataStatsHolder> defData, errorCollector,
                        template.info, packageKey, baselineSystemInfos.First())) return false;
                    dataAndStats.Add(defData);
                    break;

                case HardDefinitions.TemplateType.Multi:
                    for (int dasNo = 0; dasNo < baselineSystemInfos.Count; dasNo++)
                    {
                        if (allMemoryData != null && allMemoryData.Count() > dasNo) baselineSystemInfos[dasNo].SetMemoryData(allMemoryData[dasNo]);
                        if (!ReadData(out Dictionary<string, DataStatsHolder> mulData, errorCollector,
                            template.info, packageKey, baselineSystemInfos[dasNo], null, dasNo)) return false;
                        dataAndStats.Add(mulData);
                    }
                    break;

                case HardDefinitions.TemplateType.BaselineReform:
                    if (allMemoryData != null)
                    {
                        if (allMemoryData.Any()) baselineSystemInfos[0].SetMemoryData(allMemoryData[0]);
                        for (int refNo = 0; refNo < reformSystemInfos.Count; ++refNo)
                            if (allMemoryData.Count > refNo + 1) // note: allMemoryData[0] is reserved for baseline-data
                                reformSystemInfos[refNo].SetMemoryData(allMemoryData[refNo + 1]);
                    }
                    if (!ReadData(out Dictionary<string, DataStatsHolder> baseRefData, errorCollector,
                        template.info, packageKey, baselineSystemInfos[0], reformSystemInfos)) return false;
                    dataAndStats.Add(baseRefData);
                    break;
            }

            // Finally prepare & handle all global filters and actions
            PrepareActionsAndFilters(template.globalActions, template.globalFilters);

            return true;
        }

        private void PrepareActionsAndFilters(List<Template.Action> actions, List<Template.Filter> filters)
        {
            if (template.info.templateType == HardDefinitions.TemplateType.BaselineReform)
            {
                // Multiply template filters & actions for each reform (unless marked otherwise in XML)
                List<Template.Action> reformActions = new List<Template.Action>();
                foreach (Template.Action action in actions)
                    if (action.Reform)
                        for (int refNo = 0; refNo < reformSystemInfos.Count; refNo++)
                            reformActions.Add(new Template.Action(action, refNo));
                actions.AddRange(reformActions);

                List<Template.Filter> reformFilters = new List<Template.Filter>();
                foreach (Template.Filter filter in filters)
                    if (filter.reform)
                        for (int refNo = 0; refNo < reformSystemInfos.Count; refNo++)
                            reformFilters.Add(new Template.Filter(filter, refNo));
                filters.AddRange(reformFilters);
            }

            // Prepare & handle actions
            foreach (Template.Action action in actions)
                for (int dasNo = 0; dasNo < dataAndStats.Count; dasNo++)
                    HandleAction(dataAndStats[dasNo], action, new Template.Page.Table.SDCDefinition(), out _);
        }

        private bool ReadData(out Dictionary<string, DataStatsHolder> _dataAndStats, ErrorCollector errorCollector,
                              Template.TemplateInfo templateInfo, string packageKey,
                              SystemInfo baseSystemInfo, List<SystemInfo> reformSystemInfos = null,
                              int dasNo = 0)
        {
            _dataAndStats = new Dictionary<string, DataStatsHolder>();
            if (templateInfo == null) return false;
            foreach (Template.TemplateInfo.CalculationLevel cl in template.info.calculationLevels)
            {
                _dataAndStats.Add(cl.name, new DataStatsHolder(cl.groupingVar, dasNo, cl.name, packageKey));
                if (!templateInfo.requiredVariables.Any(x => x.name == cl.groupingVar))
                {
                    if (!string.IsNullOrEmpty(cl.groupingVar)) // if groupingVar is not even defined, the consistency-check will report
                        errorCollector.AddError($"<AdditionalCalculationLevel><GroupingVar> '{cl.groupingVar}' should be defined as a required variable.");
                    return false;
                }
            }

            var dataRows = baseSystemInfo.GetDataRows();
            double baselineRowsCount = dataRows.Count();

            // always include idperson, idhh, dwt and dag in the required variables
            foreach (var arv in HardDefinitions.alwaysRequiredVariables)
                if (!(from r in templateInfo.requiredVariables select r.name).Contains(arv.Key))
                    templateInfo.requiredVariables.Add(new Template.TemplateInfo.RequiredVariable() { name = arv.Key, readVar = arv.Value });

            // first read header line to get the column-indices of the variables to read
            List<string> headers = dataRows.First().ToLower().Split('\t').ToList();
            Dictionary<string, int> varColumnIndices = new Dictionary<string, int>();
            List<string> missingVars = new List<string>();
            foreach (Template.TemplateInfo.RequiredVariable v in templateInfo.requiredVariables)
            {
                if (_dataAndStats[IND].HasVariable(v.name, null)) continue;
                _dataAndStats[IND].AddVar(v.name, v.monetary, null);
                int index = headers.IndexOf(v.readVar.ToLower());
                if (index < 0) missingVars.Add($"'{v.readVar}'");
                varColumnIndices.Add(v.name, index);
            }
            if (missingVars.Any())
            {
                errorCollector.AddError($"File {baseSystemInfo.GetFileName()} does not contain required variable(s) {string.Join(", ", missingVars)}.");
                return false;
            }

            if (templateInfo.optionalVariables != null)
                foreach (Template.TemplateInfo.OptionalVariable v in templateInfo.optionalVariables)
                {
                    if (_dataAndStats[IND].HasVariable(v.name, null)) continue;
                    _dataAndStats[IND].AddVar(v.name, v.monetary, null); varColumnIndices.Add(v.name, headers.IndexOf(v.readVar.ToLower()));
                }
            if (templateInfo.userVariables != null)
                foreach (Template.TemplateInfo.UserVariable v in templateInfo.userVariables.Where(x => x.inputType == HardDefinitions.UserInputType.VariableName))
                {
                    if (_dataAndStats[IND].HasVariable(v.value, null)) continue;
                    _dataAndStats[IND].AddVar(v.value, v.monetary, null); varColumnIndices.Add(v.value, headers.IndexOf(v.value));
                }
            _dataAndStats[IND].ConfirmKey();

            // now read the data-lines, but only add the necessary variables
            foreach (string inputLine in dataRows.Skip(1))
            {
                string[] splitInputLine = inputLine.Replace(',', '.').Split('\t');
                if (splitInputLine.Length != headers.Count)
                {
                    errorCollector.AddError($"Mismatch between headers and data columns {baseSystemInfo.GetFileName()}."); 
                    return false;
                }

                List<double> onePersonsVariablesDouble = new List<double>();
                foreach (var varColumnIndex in varColumnIndices)
                {
                    if (varColumnIndex.Value < 0)
                    {
                        // if it is a missing Optional Variable, then use its default value
                        if (templateInfo.optionalVariables.Exists(x => x.name == varColumnIndex.Key))
                            onePersonsVariablesDouble.Add(templateInfo.optionalVariables.Find(x => x.name == varColumnIndex.Key).defaultValue);
                        // else the variable should be there! 
                        else
                        {
                            errorCollector.AddError($"Variable {varColumnIndex.Key} not found in base file {baseSystemInfo.GetFileName()}."); return false;
                        }
                    }
                    else onePersonsVariablesDouble.Add(double.Parse(splitInputLine.ElementAt(varColumnIndex.Value), CultureInfo.InvariantCulture));
                }
                _dataAndStats[IND].AddObs(onePersonsVariablesDouble);
            }

            // also create the HH level and other grouping tables with only their key, weight & number of individuals to start with
            foreach (Template.TemplateInfo.CalculationLevel cl in template.info.calculationLevels)
            {
                if (cl.name == IND) continue;

                _dataAndStats[cl.name].AddVar(cl.groupingVar, false, null);
                _dataAndStats[cl.name].AddVar(HardDefinitions.weight, false, null);
                _dataAndStats[cl.name].AddVar(HardDefinitions.NumberOfIndividuals, false, null);
                _dataAndStats[IND].AddVar(cl.groupingVar + HardDefinitions.NumberOfIndividuals, false, null);
                int keyIndex = _dataAndStats[IND].GetVarIndex(cl.groupingVar, null);
                int weightIndex = _dataAndStats[IND].GetVarIndex(HardDefinitions.weight, null);

                _dataAndStats[cl.name].ConfirmKey();
                var tmpGroup = from person in _dataAndStats[IND].GetData()
                               group person by person[keyIndex] into HH
                               select HH;
                foreach (var gp in tmpGroup)
                {
                    double key = gp.ElementAt(0)[keyIndex];
                    double weightGroup = gp.ElementAt(0)[weightIndex];
                    double indNo = gp.Count();
                    _dataAndStats[cl.name].AddObs(new List<double>() { key, weightGroup, indNo });
                    foreach (List<double> i in gp)
                        i.Add(indNo);
                }
            }

            if (reformSystemInfos == null) return true;

            // redo the process omitting/matching base vars, for the reforms
            int refNo = -1;
            foreach (SystemInfo reformSystemInfo in reformSystemInfos)
            {
                dataRows = reformSystemInfo.GetDataRows(); ++refNo;
                if (dataRows.Count() != baselineRowsCount) { errorCollector.AddError(
                    $"Reform file {reformSystemInfo.GetFileName()} has a different number of observation than base file {baseSystemInfo.GetFileName()}."); return false; }

                // first read header line to get the column-indices of the variables to read
                headers = dataRows.First().ToLower().Split('\t').ToList();
                varColumnIndices = new Dictionary<string, int>();
                foreach (Template.TemplateInfo.RequiredVariable v in templateInfo.requiredVariables)
                {
                    string refVar = v.name + HardDefinitions.Reform + refNo;
                    if (_dataAndStats[IND].HasVariable(refVar, null)) continue;
                    _dataAndStats[IND].AddVar(refVar, v.monetary, null);
                    int index = headers.IndexOf(v.readVar.ToLower());
                    if (index < 0)
                    {
                        errorCollector.AddError($"File {reformSystemInfo.GetFileName()} does not contain required variable '{v.readVar}'.");
                        if (HardDefinitions.alwaysRequiredVariables.ContainsKey(v.name)) return false;
                    }
                    varColumnIndices.Add(refVar, index);
                }
                foreach (Template.TemplateInfo.OptionalVariable v in templateInfo.optionalVariables)
                {
                    string refVar = v.name + HardDefinitions.Reform + refNo;
                    if (_dataAndStats[IND].HasVariable(refVar, null)) continue;
                    _dataAndStats[IND].AddVar(refVar, v.monetary, null); varColumnIndices.Add(refVar, headers.IndexOf(v.readVar.ToLower()));
                }
                foreach (Template.TemplateInfo.UserVariable v in templateInfo.userVariables.Where(x => x.inputType == HardDefinitions.UserInputType.VariableName))
                {
                    string refVar = v.value + HardDefinitions.Reform + refNo;
                    if (_dataAndStats[IND].HasVariable(refVar, null)) continue;
                    _dataAndStats[IND].AddVar(refVar, v.monetary, null); varColumnIndices.Add(refVar, headers.IndexOf(v.value));
                }
                // now read the data-lines, but only add the necessary variables
                int idPersonIndex = _dataAndStats[IND].GetVarIndex(HardDefinitions.idPerson, null);
                foreach (string inputLine in dataRows.Skip(1))
                {
                    string[] splitInputLine = inputLine.Replace(',', '.').Split('\t');
                    List<double> onePersonsVariablesDouble = new List<double>();
                    foreach (var varColumnIndex in varColumnIndices)
                    {
                        if (varColumnIndex.Value < 0)
                        {
                            // if it is a missing Optional Variable, then use its default value
                            if (templateInfo.optionalVariables.Exists(x => x.name == GetBaseVarName(varColumnIndex.Key, refNo)))
                                onePersonsVariablesDouble.Add(templateInfo.optionalVariables.Find(x => x.name == GetBaseVarName(varColumnIndex.Key, refNo)).defaultValue);
                            // else the variable should be there! 
                            else
                            {
                                errorCollector.AddError($"Variable {GetBaseVarName(varColumnIndex.Key, refNo)} not found in reform file {reformSystemInfo.GetFileName()}."); return false;
                            }
                        }
                        else onePersonsVariablesDouble.Add(double.Parse(splitInputLine.ElementAt(varColumnIndex.Value), CultureInfo.InvariantCulture));
                    }
                    if (!_dataAndStats[IND].HasObs(onePersonsVariablesDouble[idPersonIndex]))
                    {
                        errorCollector.AddError($"Mismatch in {HardDefinitions.idPerson} between reform file {reformSystemInfo.GetFileName()} and base file {baseSystemInfo.GetFileName()}." + Environment.NewLine +
                            $"Base file does not contain {HardDefinitions.idPerson} {onePersonsVariablesDouble[idPersonIndex]}.");
                        return false;
                    }
                    _dataAndStats[IND].GetObs(onePersonsVariablesDouble[idPersonIndex]).AddRange(onePersonsVariablesDouble);
                }
            }

            return true;
        }

        public string GetErrorMessage()
        {
            return errorCollector.GetErrorMessage();
        }

        private double HandleAction(Dictionary<string, DataStatsHolder> data, Template.Action action,
                                    Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo,
                                    List<CellUnderConstruction> rowUnderConstruction = null, int refNo = -1)
        {
            sdcObsNo = int.MaxValue;
            try
            {
                if (action.calculationType == HardDefinitions.CalculationType.Empty) return double.NaN;
                if (!data.ContainsKey(action.CalculationLevel)) return ActionError(action, $"unknown CalculationLevel {action.CalculationLevel}");

                action = FixActionUserVariables(action, template);
                PrepareFilter(data, action.CalculationLevel, action.localMap, action.filter, template);
                if (action.filter != null && action.filter.func == null)
                    ActionError(action, $"Preparing Filter {(string.IsNullOrEmpty(action.filter.name) ? string.Empty : $" '{action.filter.name}'")} failed");

                if (action.calculationType == HardDefinitions.CalculationType.Message) IssueMessage(data, action);
                else if (action.calculationType == HardDefinitions.CalculationType.CreateArithmetic)
                {
                    // createArithmetic requires a formula and will use it at the "inner" level
                    if (string.IsNullOrEmpty(action.formulaString)) return ActionError(action, $"{action.calculationType} requires a <FormulaString>");
                    if (!PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, action.formulaString, data, action.CalculationLevel, action.localMap, action.parameters, out string error)) return ActionError(action, error);
                    ParameterExpression[] DATA_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") };
                    action.func = ParseFormula(DATA_VAR, formula);
                    action.result = CreateArithmeticColumn(data, action) ? 0 : double.NaN;
                }
                else if (HardDefinitions.InEnumList(action.calculationType, new HardDefinitions.CalculationType[] { HardDefinitions.CalculationType.CreateEquivalized, HardDefinitions.CalculationType.CreateOECDScale, HardDefinitions.CalculationType.CreateEquivalenceScale, HardDefinitions.CalculationType.CalculateGini, HardDefinitions.CalculationType.CalculateS8020, HardDefinitions.CalculationType.CalculateMeanLogDeviation, HardDefinitions.CalculationType.CalculateAtkinson, HardDefinitions.CalculationType.CreateDeciles, HardDefinitions.CalculationType.CalculateMedian, HardDefinitions.CalculationType.CreateGroupValue, HardDefinitions.CalculationType.CreateHHValue, HardDefinitions.CalculationType.CalculatePovertyGap, HardDefinitions.CalculationType.CreateFlag }))
                {
                    // these actions ignore the formula attribute
                    if (!string.IsNullOrEmpty(action.formulaString)) // do not break - just ignore formula
                        ActionError(action, $"{action.calculationType} does not allow for a <FormulaString> (<FormulaString> is ignored)");
                    switch (action.calculationType)
                    {
                        case HardDefinitions.CalculationType.CalculateGini:
                            action.result = CalculateGini(data, action, sdcDefinition, out sdcObsNo);
                            break;
                        case HardDefinitions.CalculationType.CalculateS8020:
                            action.result = CalculateS8020(data, action, sdcDefinition, out sdcObsNo);
                            break;
                        case HardDefinitions.CalculationType.CalculatePovertyGap:
                            action.result = CalculatePovertyGap(data, action, sdcDefinition, out sdcObsNo);
                            break;
                        case HardDefinitions.CalculationType.CalculateMedian:
                            action.result = CalculateMedian(data, action, sdcDefinition, out sdcObsNo);
                            break;
                        case HardDefinitions.CalculationType.CalculateMeanLogDeviation:
                            action.result = CalculateMeanLogDeviation(data, action, sdcDefinition, out sdcObsNo);
                            break;
                        case HardDefinitions.CalculationType.CalculateAtkinson:
                            action.result = CalculateAtkinson(data, action, sdcDefinition, out sdcObsNo);
                            break;
                        case HardDefinitions.CalculationType.CreateDeciles:
                            action.result = CreateDeciles(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateEquivalized:
                            action.result = CreateEquivalized(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateGroupValue:
                            action.result = CreateGroupValue(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateOECDScale:
                            action.result = CreateOECDScale(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateEquivalenceScale:
                            action.result = CreateEquivalenceScale(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateFlag:
                            action.result = CreateFlag(data, action) ? 0 : double.NaN;
                            break;
                        default: return ActionError(action, "Unknown <CalculationType>"); // is actually already found by the xml-handler
                    }
                }
                else if (HardDefinitions.InEnumList(action.calculationType, new HardDefinitions.CalculationType[] { HardDefinitions.CalculationType.CalculateArithmetic, HardDefinitions.CalculationType.CalculateSumWeighted, HardDefinitions.CalculationType.CalculateWeightedAverage, HardDefinitions.CalculationType.CalculatePopulationCount }))
                {
                    // these actions treat the formula attribute at the "outer" level
                    if (string.IsNullOrEmpty(action.formulaString)) return ActionError(action, $"{action.calculationType} requires a <FormulaString>");
                    if (!PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, action.formulaString, data, action.CalculationLevel, action.localMap, action.parameters, out string error)) return ActionError(action, error);

                    if (action.calculationType == HardDefinitions.CalculationType.CalculateArithmetic)
                    {
                        // The CalculateArithmetic is the only type allowed to use column values
                        if (!PrepareFormulaColumns(ref formula, out error, rowUnderConstruction, refNo)) return ActionError(action, error);
                        //if (formula.Contains("DATA_VAR")) return ActionError(action, $"{action.calculationType} does not allow for DATA_VAR[]");
                        ParameterExpression[] DATA_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") };
                        action.func = ParseFormula(DATA_VAR, constMakeDouble + formula);
                        action.result = CalculateArithmeticAction(data, action, sdcDefinition, out sdcObsNo);
                        sdcObsNo = Math.Min(sdcObsNo, sdcObsNo_SavedNumbers);
                    }
                    else
                    {
                        // Handle all Operation variables
                        int pos = formula.IndexOf(HardDefinitions.FormulaParameter.DATA_VAR);
                        List<double> opvars = new List<double>();
                        sdcObsNo = sdcObsNo_SavedNumbers;
                        while (pos > -1)
                        {
                            int startpos = pos + HardDefinitions.FormulaParameter.DATA_VAR.Length;
                            int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                            if (startpos > formula.Length || endpos < startpos) return ActionError(action, "Invalid FormulaString");
                            if (!int.TryParse(formula.Substring(startpos, endpos - startpos), out int varIndex)) return ActionError(action, "Invalid FormulaString");
                            double varResult = double.NaN; int sdcObsNoTemp = int.MaxValue;
                            switch (action.calculationType)
                            {
                                case HardDefinitions.CalculationType.CalculateSumWeighted:
                                    varResult = CalculateSumWeighted(data, action, varIndex, sdcDefinition, out sdcObsNoTemp);
                                    break;
                                case HardDefinitions.CalculationType.CalculateWeightedAverage:
                                    varResult = CalculateWeightedAverage(data, action, varIndex, sdcDefinition, out sdcObsNoTemp);
                                    break;
                                case HardDefinitions.CalculationType.CalculatePopulationCount:
                                    varResult = CalculatePopulationCount(data, action, varIndex, sdcDefinition, out sdcObsNoTemp);
                                    break;
                            }
                            sdcObsNo = Math.Min(sdcObsNo, sdcObsNoTemp);
                            string cnt = opvars.Count.ToString();
                            opvars.Add(varResult);
                            formula = formula.Substring(0, startpos) + cnt + formula.Substring(endpos);
                            pos = formula.IndexOf(HardDefinitions.FormulaParameter.DATA_VAR, startpos + cnt.Length);
                        }

                        ParameterExpression[] DATA_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") };
                        action.func = ParseFormula(DATA_VAR, constMakeDouble + formula);
                        action.result = action.func(opvars);

                        if (!string.IsNullOrEmpty(action.outputVar))
                            data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, action.result, sdcObsNo));
                    }
                }

                Func<List<double>, double> ParseFormula(ParameterExpression[] XX_VAR, string formula)
                {
                    if (formula.Contains(double.NaN.ToString())) return funcHandingNaN; // if formula already contains NaN, just go ahead with NaN, but do not "complain"
                    return (Func<List<double>, double>)DynamicExpressionParser.ParseLambda(XX_VAR, null, formula, Array.Empty<object>()).Compile();

                    double funcHandingNaN(List<double> d) { return double.NaN; }
                }
            }
            catch (Exception exception) { ActionError(action, $"Unexpected error: {exception.Message}"); }

            return action.result;
        }

        /**
         * This function parses a filter and its parameters from string, and compiles it so that it can be used as a function
         */
        private void PrepareFilter(Dictionary<string, DataStatsHolder> dataAllLevels, string level, LocalMap localMap, Template.Filter filter, Template template)
        {
            DataStatsHolder data = dataAllLevels[level];

            foreach (Template.TemplateInfo.CalculationLevel cl in template.info.calculationLevels)
            if (filter == null || filter.HasFunc(data)) return;

            // if named filters on any higher level exist, combine missing properties
            foreach (Template.Filter namedFilter in LocalMap.GetNamedFilters(template, filter.name, localMap))
            {
                if (string.IsNullOrEmpty(filter.formulaString) && !string.IsNullOrEmpty(namedFilter.formulaString))
                    filter.formulaString = namedFilter.formulaString; // i.e. take lowest local level of Table/Page/Global 
                foreach (Template.Parameter par in namedFilter.parameters)
                    if (!(from p in filter.parameters where p.name.ToLower() == par.name.ToLower() select p).Any()) // i.e. do not overwrite (or duplicate) lower level parameters
                        filter.parameters.Add(par);
            }
            if (string.IsNullOrEmpty(filter.formulaString)) return;

            if (filter.func == null) filter.func = new Dictionary<string, Func<List<double>, bool>>();
            Func<List<double>, bool> func = funcHandingNaN;
            try
            {
                if (PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, filter.formulaString, dataAllLevels, level, localMap, filter.parameters, out string error))
                {
                    ParameterExpression[] DATA_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "DATA_VAR") };
                    if (!formula.Contains(double.NaN.ToString()))
                        func = (Func<List<double>, bool>)DynamicExpressionParser.ParseLambda(DATA_VAR, null, formula, Array.Empty<object>()).Compile();
                }
                else FilterError(error);
            }
            catch (Exception exception) { FilterError($"Unexpected error: {exception.Message}"); }
            finally // try to go ahead, even in case of error, without producing further troubles
            {
                filter.func.Add(Template.Filter.FuncID(data), func);
                if (!string.IsNullOrEmpty(filter.name) && !data.filters.ContainsKey(filter.name)) data.filters.Add(filter.name, func);
            }

            void FilterError(string msg)
            {
                string ident = string.IsNullOrEmpty(filter.name) ? string.Empty : $"'{filter.name}'";
                if (string.IsNullOrEmpty(ident) && !string.IsNullOrEmpty(filter.formulaString))
                {
                    string fs = filter.formulaString.Length < 20 ? filter.formulaString : filter.formulaString.Substring(0, 20);
                    ident += $"FormulaString={fs}{(fs.Length == filter.formulaString.Length ? string.Empty : " ...")}";
                }
                errorCollector.AddError($"Error in Filter {ident.Trim()}.{Environment.NewLine}{msg}.");
            }

            bool funcHandingNaN(List<double> d) { return false; }
        }

        private bool PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, string origFormula,
                                    Dictionary<string, DataStatsHolder> dataAllLevels,
                                    string calculationLevel, LocalMap localMap,
                                    List<Template.Parameter> parameters, out string error)
        {
            // the library can generate formulas with direct indexes - see Gini
            // for user generated, parameter names need to start with "@"

            // in all our replacements, we add (" + doubleMultiplier + " ... ) to make sure that all our parameters are considered doubles! otherwise, you may get an integer division or even integer result which would cause a crash!

            DataStatsHolder data = dataAllLevels[calculationLevel];
            formula = origFormula; error = null; sdcObsNo_SavedNumbers = int.MaxValue;
            // Handle all Saved Variables
            int pos = -1; if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.SAVED_VAR)) return false;
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.SAVED_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid saved variable in formula", ref error);
                string svParName = formula.Substring(startpos, endpos - startpos);
                string svName = null;
                Template.Parameter svPar = parameters.FirstOrDefault(x => x.name == svParName);
                if (svPar != null)
                {
                    if (string.IsNullOrEmpty(svPar.variableName)) return PrepareFormulaError($"Parameter '{svParName}': <VarName> not defined", ref error);
                    svName = svPar.variableName;
                }
                else // a "short-cut-reference", e.g. USR_VAR[@MySavedVar] without providing a parameter { Name="MySavedVar", VarName="MySavedVar" }
                    svName = svParName;
                if (!data.HasSavedNumber(svName, localMap))
                    return PrepareFormulaError($"SAVED_VAR '{svName}' not found{(svName == svParName ? string.Empty : $" (parameter '{svParName}')")}", ref error);
                DataStatsHolder.SavedNumber savedNumber = data.GetSavedNumber(svName, localMap);
                sdcObsNo_SavedNumbers = Math.Min(savedNumber.sdcObsNo, sdcObsNo_SavedNumbers);
                string snStr = "(" + constMakeDouble + savedNumber.number.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")";
                formula = formula.Substring(0, pos) + snStr + formula.Substring(endpos + 1);
                if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.SAVED_VAR, pos + snStr.Length)) return false;
            }

            // Handle all User Variables
            if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.USR_VAR)) return false;
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.USR_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid user variable in formula", ref error);
                string uvParName = formula.Substring(startpos, endpos - startpos);
                string uvName = null;
                Template.Parameter uvPar = parameters.FirstOrDefault(x => x.name == uvParName);
                if (uvPar != null)
                {
                    if (string.IsNullOrEmpty(uvPar.variableName)) return PrepareFormulaError($"Parameter '{uvParName}': <VarName> not defined", ref error);
                    uvName = uvPar.variableName;
                }
                else // a "short-cut-reference", e.g. USR_VAR[@MyUserVar] without providing a parameter { Name="MyUserVar", VarName="MyUserVar" }
                    uvName = uvParName;
                int uvRefNo = ExtractRefFromVarName(uvName, out uvName); // todo: consider whether refNo can be made useable for this (currently it is not reliable, as for non-CellActions (i.e. Global/Page/Table-Actions) it is always -1 (should it?)
                Template.TemplateInfo.UserVariable uservar = template.info.GetUserVariable(uvName, packageKey, uvRefNo);
                if (uservar == null || string.IsNullOrEmpty(uservar.value))
                    return PrepareFormulaError($"USR_VAR '{uvName}' not found{(uvName == uvParName ? string.Empty : $" (parameter '{uvParName}')")}", ref error);
                string value = "";
                if (uservar.inputType == HardDefinitions.UserInputType.Numeric || uservar.inputType == HardDefinitions.UserInputType.Categorical_Numeric)
                    value = "(" + constMakeDouble + uservar.value + ")";
                else if (uservar.inputType == HardDefinitions.UserInputType.VariableName || uservar.inputType == HardDefinitions.UserInputType.Categorical_VariableName)
                {
                    int varIndex = GetVarIndex_AutoGrossUp(uservar.value);
                    if (varIndex < 0) return PrepareFormulaError($"user variable {uservar.name} (variable '{uservar.value}') not found", ref error);
                    value = "(" + constMakeDouble + HardDefinitions.FormulaParameter.DATA_VAR + varIndex.ToString() + HardDefinitions.FormulaParameter.CLOSING_TOKEN + ")";
                }
                formula = formula.Substring(0, pos) + value + formula.Substring(endpos + 1);
                if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.USR_VAR, pos + value.Length)) return false;
            }

            // Handle all Operation variables
            if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.DATA_VAR)) return false;
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.DATA_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid variable in formula", ref error);
                string dvParName = formula.Substring(startpos, endpos - startpos);
                string dvName = null;
                Template.Parameter dvPar = parameters.FirstOrDefault(x => x.name == dvParName);
                if (dvPar != null)
                {
                    if (string.IsNullOrEmpty(dvPar.variableName)) return PrepareFormulaError($"Parameter '{dvParName}': <VarName> not defined", ref error);
                    dvName = dvPar.variableName;
                }
                else // a "short-cut-reference", e.g. DATA_VAR[@yem] without providing a parameter { Name="yem", VarName="yem" }
                    dvName = dvParName;
                string varIndex = GetVarIndex_AutoGrossUp(dvName).ToString();
                if (varIndex == "-1")
                    return PrepareFormulaError($"variable '{dvName}' not found{(dvName == dvParName ? string.Empty : $" (parameter '{dvParName}')")}", ref error);
                varIndex = "(" + constMakeDouble + HardDefinitions.FormulaParameter.DATA_VAR + varIndex + HardDefinitions.FormulaParameter.CLOSING_TOKEN + ")";
                formula = formula.Substring(0, pos) + varIndex + formula.Substring(endpos + 1);
                if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.DATA_VAR, pos + varIndex.Length)) return false;
            }

            if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.TEMP_VAR)) return false;
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.TEMP_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid template variable in formula", ref error);
                string tvParName = formula.Substring(startpos, endpos - startpos);
                Template.Parameter tvPar = parameters.FirstOrDefault(x => x.name == tvParName);
                if (tvPar == null) return PrepareFormulaError($"Parameter '{tvParName}' not found", ref error);
                else if (double.IsNaN(tvPar.numericValue)) return PrepareFormulaError($"Parameter '{tvParName}': <NumericValue> not defined", ref error);
                string varValue = "(" + constMakeDouble + tvPar.numericValue + ")";
                formula = formula.Substring(0, pos) + varValue + formula.Substring(endpos + 1);
                if (!NextPos(ref pos, ref error, formula, HardDefinitions.FormulaParameter.TEMP_VAR, pos + varValue.Length)) return false;
            }
            return true;

            // returns the index of a variable on the respective level of calculation (individual, household, family, ...)
            // if the index does not exist on a higher than individual level, it tries to find the variable on individual level and, if available, "grosses it up"
            int GetVarIndex_AutoGrossUp(string varName)
            {
                int varIndex = dataAllLevels[calculationLevel].GetVarIndex(varName, localMap);
                if (varIndex != -1) return varIndex;

                int indVarIndex = calculationLevel == IND ? -1 : dataAllLevels[IND].GetVarIndex(varName, localMap);
                if (indVarIndex == -1) return -1; // does not exist on individual level either (or request for index was on individual level)

                CreateGroupValue(dataAllLevels, calculationLevel, varName);
                return dataAllLevels[calculationLevel].GetVarIndex(varName, localMap);
            }
            
            bool PrepareFormulaError(string msg, ref string err) { err = $"Faulty <FormulaString>: {msg}"; return false; }

            bool NextPos(ref int np, ref string err, string f, string marker, int startIndex = 0)
            {
                for (np = f.IndexOf(marker, startIndex);
                     np != -1 && np + marker.Length < f.Length && EM_Helpers.IsDigit(f[np + marker.Length]);
                     np = f.IndexOf(marker, np + marker.Length)) ;
                return (np == -1 || (np + marker.Length < f.Length && f[np + marker.Length] == '@')) ? true
                   : PrepareFormulaError($"'{marker}' should be followed by '@'", ref err);
            }
        }

        private static int ExtractRefFromVarName(string origVarName, out string cleanedVarName)
        {
            int refNo = -1; cleanedVarName = origVarName;
            if (cleanedVarName != null && cleanedVarName.Contains(HardDefinitions.Reform))
            {
                string rn = cleanedVarName.Substring(cleanedVarName.IndexOf(HardDefinitions.Reform) + HardDefinitions.Reform.Length); // extract e.g. "2" from "Const1~ref~2"
                if (int.TryParse(rn, out refNo)) cleanedVarName = cleanedVarName.Substring(0, cleanedVarName.IndexOf(HardDefinitions.Reform));
            }
            return refNo;
        }

        private static bool PrepareFormulaColumns(ref string formula, out string error, List<CellUnderConstruction> rowUnderConstruction, int refNo)
        {
            // Handle all Base Columns
            error = null;

            int pos = formula.IndexOf(HardDefinitions.FormulaParameter.BASE_COL);
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.BASE_COL.Length;
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) { error = $"Invalid use of {HardDefinitions.FormulaParameter.BASE_COL} in formula"; return false; }

                string colName = formula.Substring(startpos, endpos - startpos);
                DisplayResults.DisplayPage.DisplayTable.DisplayCell referredCell = (from ruc in rowUnderConstruction
                                                                                    where ruc.refNo == -1 && ruc.colName.ToLower() == colName.ToLower()
                                                                                    select ruc.displayCell).FirstOrDefault();
                if (referredCell == null) { error = $"{HardDefinitions.FormulaParameter.BASE_COL}{colName}]: Column not found"; return false; }
                string colValue = "(" + constMakeDouble + referredCell.value.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")"; // TODO - what kind of string to preserve best accuracy?
                formula = formula.Substring(0, pos) + colValue + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.BASE_COL, pos + colValue.Length);
            }

            // Handle all Reform Columns
            pos = formula.IndexOf(HardDefinitions.FormulaParameter.REF_COL);
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.REF_COL.Length;
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) { error = $"Invalid use of {HardDefinitions.FormulaParameter.REF_COL} in formula"; return false; }
                string colName = formula.Substring(startpos, endpos - startpos);
                DisplayResults.DisplayPage.DisplayTable.DisplayCell referredCell = (from ruc in rowUnderConstruction
                                                                                    where ruc.refNo == refNo && ruc.colName.ToLower() == colName.ToLower()
                                                                                    select ruc.displayCell).FirstOrDefault();
                if (referredCell == null) { error = $"{HardDefinitions.FormulaParameter.REF_COL}{colName}]: ReformColumn not found"; return false; }
                string colValue = "(" + constMakeDouble + referredCell.value.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")"; // TODO - what kind of string to preserve best accuracy?
                formula = formula.Substring(0, pos) + colValue + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.REF_COL, pos + colValue.Length);
            }

            pos = formula.IndexOf(HardDefinitions.FormulaParameter.REF_COL_PRE);
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.REF_COL_PRE.Length;
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) { error = $"Invalid use of {HardDefinitions.FormulaParameter.REF_COL_PRE} in formula"; return false; }
                string []baseRefNames = formula.Substring(startpos, endpos - startpos).Split(HardDefinitions.Separator[0]);
                if (baseRefNames.Length < 1) { error = $"Invalid use of {HardDefinitions.FormulaParameter.REF_COL_PRE} in formula"; return false; }
                string refColName = baseRefNames[0], baseColName = baseRefNames.Length < 2 ? null : baseRefNames[1];

                DisplayResults.DisplayPage.DisplayTable.DisplayCell referredCell = null;
                if (refNo == 0) // first reform
                {
                    referredCell = (from ruc in rowUnderConstruction
                                    where ruc.refNo == -1 && // if no name for base is provided, try if there is a base-col-definition with the same name as ref-col-definition
                                          ruc.colName.ToLower() == (baseColName  == null ? refColName.ToLower() : baseColName.ToLower())
                                    select ruc.displayCell).FirstOrDefault();
                    if (referredCell == null && baseColName != null) { error = $"Column {baseColName} not found"; return false; }
                }
                if (referredCell == null) // second+ reform or first reform if finding base-col failed (as well with explicitly defined name as with name equal to ref-col-name)
                {
                    referredCell = (from ruc in rowUnderConstruction // for first reform just use first reform instead of (non existent) previous (very likely you get sth. like col[0] - col[0], i.e. result 0)
                                    where ruc.refNo == Math.Max(refNo - 1, 0) &&
                                          ruc.colName.ToLower() == refColName.ToLower()
                                    select ruc.displayCell).FirstOrDefault();
                    if (referredCell == null) { error = $"ReformColumn {refColName} not found"; return false; }
                }

                string colValue = "(" + constMakeDouble + referredCell.value.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")"; // TODO - what kind of string to preserve best accuracy?
                formula = formula.Substring(0, pos) + colValue + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.REF_COL_PRE, pos + colValue.Length);
            }

            return true;
        }

        // this structure is only necessary for formula-components BASE_COL[] and REF_COL[] (see PrepareFormulaColumns)
        private class CellUnderConstruction
        {
            internal DisplayResults.DisplayPage.DisplayTable.DisplayCell displayCell = null;
            internal int refNo = -1; internal string colName = null;
        }

        /**
         * This function is used to create a display table to be displayed in the Statistics Presenter
         */
        internal bool CreateDisplayTable(out DisplayResults.DisplayPage.DisplayTable displayTable, Template.Page.Table table, int refNo_PerReform = -1)
        {
            displayTable = null;

            // Handle the case that a row does not describe a single row in the display table, but leads to generation of several rows
            // keep a copy of the original rows to set back at the end of function, in order to not destroy the original template, in case of further packages
            if (!HandleForEachRow(table.cellAction.CalculationLevel, table, out List<Template.Page.Table.Row> origRows)) return false;

            // First prepare a display table that will be passed back to the Presenter, based on the template table
            displayTable = PrepareDisplayTable(table, refNo_PerReform); // Prepare Table itself
            displayTable.rows.AddRange(PrepareDisplayRows(table));   // Prepare Table Rows
            // Preparing Table Columns is done together with preparing cells, to avoid redundant code for columns-sorting

            // The "cells" is a table of each actual cell to be displayed in the end
            displayTable.cells = new List<List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>>();

            // Build the actual display table
            for (int rowNo = 0; rowNo < table.rows.Count; rowNo++)
            {
                // Create a new row of cells (the other components of CellUnderConstruction are only necessary for formula-components BASE_COL[] and REF_COL[] - see PrepareFormulaColumns)
                List<CellUnderConstruction> rowUnderConstruction = new List<CellUnderConstruction>();

                switch (template.info.templateType)
                {
                    case HardDefinitions.TemplateType.Default:
                        for (int colNo = 0; colNo < table.columns.Count; colNo++)
                            CreateBaseColAndCell(displayTable, colNo);
                        break;

                    case HardDefinitions.TemplateType.Multi:
                        if (table.columnGrouping == HardDefinitions.ColumnGrouping.SystemFirst) // Col1/Sys1 .. ColX/Sys1 .... Col1/SysY .. ColX/SysY
                        {
                            for (int dasNo = 0; dasNo < dataAndStats.Count; dasNo++)
                                for (int colNo = 0; colNo < table.columns.Count; colNo++)
                                    CreateBaseColAndCell(displayTable, colNo, dasNo);
                        }
                        else if (table.columnGrouping == HardDefinitions.ColumnGrouping.ColumnFirst) // Col1/Sys1 .. Col1/SysY .... ColX/Sys1 .. ColX/SysY
                        {
                            for (int c = 0; c < table.columns.Count; c++)
                                for (int dasNo = 0; dasNo < dataAndStats.Count; dasNo++)
                                    CreateBaseColAndCell(displayTable, c, dasNo);
                        }
                        break;

                    case HardDefinitions.TemplateType.BaselineReform:
                        if (table.columnGrouping == HardDefinitions.ColumnGrouping.SystemFirst)
                        {
                            for (int baseColNo = 0; baseColNo < table.columns.Count; baseColNo++)
                            {
                                // create the column and cell of each baseline ...
                                CreateBaseColAndCell(displayTable, baseColNo);

                                // ... followed by the columns and cells of each reform column that ties with this baseline column ...
                                for (int refNo = (refNo_PerReform == -1 ? 0 : refNo_PerReform); refNo < (refNo_PerReform == -1 ? reformSystemInfos.Count : refNo_PerReform + 1); refNo++)
                                    for (int refColNo = 0; refColNo < table.reformColumns.Count; refColNo++)
                                    {
                                        if (string.IsNullOrEmpty(table.reformColumns[refColNo].tiesWith) ||
                                            table.reformColumns[refColNo].tiesWith.ToLower() != table.columns[baseColNo].name.ToLower()) continue;
                                        CreateReformColAndCell(displayTable, refColNo, refNo);
                                    }
                            }
                            // ... followed by do the "loose" (i.e. not tied) reform columns
                            for (int refNo = (refNo_PerReform == -1 ? 0 : refNo_PerReform); refNo < (refNo_PerReform == -1 ? reformSystemInfos.Count : refNo_PerReform + 1); refNo++)
                                for (int refColNo = 0; refColNo < table.reformColumns.Count; refColNo++)
                                {
                                    if (!string.IsNullOrEmpty(table.reformColumns[refColNo].tiesWith)) continue; // already done above
                                    CreateReformColAndCell(displayTable, refColNo, refNo);
                                }
                        }

                        else if (table.columnGrouping == HardDefinitions.ColumnGrouping.ColumnFirst)
                        {
                            for (int baseColNo = 0; baseColNo < table.columns.Count; baseColNo++)
                            {
                                // create the column and cell of each baseline ...
                                CreateBaseColAndCell(displayTable, baseColNo);

                                // ... followed by the columns and cells of each reform column that ties with this baseline column ...
                                for (int refColNo = 0; refColNo < table.reformColumns.Count; refColNo++)
                                {
                                    if (string.IsNullOrEmpty(table.reformColumns[refColNo].tiesWith) ||
                                        table.reformColumns[refColNo].tiesWith.ToLower() != table.columns[baseColNo].name.ToLower()) continue;
                                    for (int refNo = (refNo_PerReform == -1 ? 0 : refNo_PerReform); refNo < (refNo_PerReform == -1 ? reformSystemInfos.Count : refNo_PerReform + 1); refNo++)
                                        CreateReformColAndCell(displayTable, refColNo, refNo);
                                }
                            }
                            // ... followed by do the "loose" (i.e. not tied) reform columns
                            for (int refColNo = 0; refColNo < table.reformColumns.Count; refColNo++)
                            {
                                if (!string.IsNullOrEmpty(table.reformColumns[refColNo].tiesWith)) continue; // already done above
                                for (int refNo = (refNo_PerReform == -1 ? 0 : refNo_PerReform); refNo < (refNo_PerReform == -1 ? reformSystemInfos.Count : refNo_PerReform + 1); refNo++)
                                    CreateReformColAndCell(displayTable, refColNo, refNo);
                            }
                        }
                        break;
                }

                void CreateBaseColAndCell(DisplayResults.DisplayPage.DisplayTable _displayTable, int baseColNo, int dasNo = 0)
                {
                    // Create the display column, if it not already exists
                    if (rowNo == 0) _displayTable.columns.Add(PrepareDisplayColumn(table.columns[baseColNo], -1, dasNo, table.columnGrouping, false));
                    // Create the diplay cell for this row and column
                    DisplayResults.DisplayPage.DisplayTable.DisplayCell baseDisplayCell =
                        CalculateCellValue(dataAndStats[dasNo], table, baseColNo, rowNo, rowUnderConstruction, -1, refNo_PerReform); // refNo_PerReform is provided for SDC only
                    rowUnderConstruction.Add(new CellUnderConstruction() { displayCell = baseDisplayCell, colName = table.columns[baseColNo].name });
                }

                void CreateReformColAndCell(DisplayResults.DisplayPage.DisplayTable _displayTable, int refColNo, int refNo)
                {
                    // Create the display column, if it not already exists
                    if (rowNo == 0) _displayTable.columns.Add(PrepareDisplayColumn(table.reformColumns[refColNo], refNo, 0, table.columnGrouping, refNo_PerReform != -1));
                    // Create the diplay cell for this row and column
                    DisplayResults.DisplayPage.DisplayTable.DisplayCell reformDisplayCell =
                        CalculateCellValue(dataAndStats[0], // note: for baseline-reform there is only one dataAndStats
                                           table, refColNo, rowNo, rowUnderConstruction, refNo);
                    rowUnderConstruction.Add(new CellUnderConstruction() { displayCell = reformDisplayCell, refNo = refNo, colName = table.reformColumns[refColNo].name });
                }

                // add the new row to the display table
                displayTable.cells.Add((from ruc in rowUnderConstruction select ruc.displayCell).ToList());
            }

            // make sure that there are no double-borders in the beginning or end of the table
            if (displayTable.columns.Any())
            {
                displayTable.columns[0].hasSeparatorBefore = false;
                displayTable.columns[displayTable.columns.Count - 1].hasSeparatorAfter = false;
            }

            table.rows = origRows; // undo any changes to original template-rows, see above
            return true;
        }

        private bool HandleForEachRow(string calculationLevel, Template.Page.Table table,
                                      out List<Template.Page.Table.Row> origRows)
        {
            // make a copy of the original rows, to not modify the template (in case of further packages)
            origRows = table.rows;
            List<Template.Page.Table.Row> adaptedRows = new List<Template.Page.Table.Row>();

            foreach (Template.Page.Table.Row origRow in origRows)
            {
                if (!origRow.forEachDataRow && string.IsNullOrEmpty(origRow.forEachValueOf)) { adaptedRows.Add(origRow); continue; }

                if (!HandleForEachRow_ErrorCheck(calculationLevel, table.localMap, origRow)) return false;

                // row will be replaced by as many rows as necessary, each with an appropriate filter:
                // ForEachDataRow: as many rows as data-rows (taking original row-filter into account) with filter: pid=x (or hhid=x)
                // ForEachValueOf: as many rows as there are distinct values of this variable with filter: var=oneval (e.g. decile=1 ... decile=10)
                foreach (Template.Filter filter in origRow.forEachDataRow
                                         ? HandleForEachRow_GetFiltersDataRow(calculationLevel, table.localMap, origRow)
                                         : HandleForEachRow_GetFiltersValueOf(calculationLevel, table.localMap, table, origRow))
                {
                    // note that HandleForEachRow_GetFiltersValueOf stores the new name for the rows temporarily in filter.name
                    string rowTitle = !string.IsNullOrEmpty(filter.name) ? filter.name : origRow.title; filter.name = string.Empty;
                    Template.Action cellAction = origRow.cellAction == null ? new Template.Action(table.localMap) : origRow.cellAction.Clone();
                    cellAction.filter = filter;
                    adaptedRows.Add(new Template.Page.Table.Row()
                    {
                        name = origRow.name, title = rowTitle, cellAction = cellAction,
                        isVisible = origRow.isVisible, stringFormat = origRow.stringFormat, strong = origRow.strong, tooltip = origRow.tooltip,
                        foregroundColour = origRow.foregroundColour, backgroundColour = origRow.backgroundColour, textAlign = origRow.textAlign
                    });
                }
            }
            table.rows = adaptedRows;
            return true;
        }

        private List<Template.Filter> HandleForEachRow_GetFiltersDataRow(string calculationLevel, LocalMap localMap, Template.Page.Table.Row origRow)
        {
            DataStatsHolder data = dataAndStats[0][calculationLevel]; // data 0 is ok as the filter refers to pid or hhid, which need to be equal in base/reform (and multi is not allowed for this option)
            Func<List<double>, bool> func = null; // first apply the original filter on the data
            if (origRow.cellAction != null && origRow.cellAction.filter != null)
            {
                PrepareFilter(dataAndStats[0], calculationLevel, localMap, origRow.cellAction.filter, template);
                func = origRow.cellAction.filter.GetFunc(data);
            }

            List<Template.Filter> listFilters = new List<Template.Filter>();
            data.GetData(func).ForEach(x => listFilters.Add(new Template.Filter(localMap)
            {
                formulaString = string.Format("DATA_VAR[@key_id] == {0}", x[data.GetKeyIndex()]),
                parameters = new List<Template.Parameter>() { new Template.Parameter() { name = "key_id", variableName = data.GetKeyName() } }
            }));
            return listFilters;
        }

        private List<Template.Filter> HandleForEachRow_GetFiltersValueOf(string calculationLevel, LocalMap localMap, Template.Page.Table table, Template.Page.Table.Row origRow)
        {
            Template.Filter origFilter = origRow.cellAction?.filter ?? table.cellAction?.filter;
            string valofVar = origRow.forEachValueOf;

            List<double> valofValues = new List<double>();
            for (int dasNo = 0; dasNo < dataAndStats.Count; dasNo++) // for multi, to be precise, one needs to assess the values from all datasets
            {
                DataStatsHolder data = dataAndStats[dasNo][calculationLevel];
                Func<List<double>, bool> func = null; // only take the values which are covered when applying the original filter
                if (origFilter != null)
                {
                    PrepareFilter(dataAndStats[dasNo], calculationLevel, localMap, origFilter, template);
                    func = origFilter.GetFunc(data);
                }
                foreach (double v in data.GetData(func).Select(x => x[data.GetVarIndex(valofVar, localMap)]).Distinct())
                    if (!valofValues.Contains(v)) valofValues.Add(v);
            }

            if (valofValues.Count() > origRow.forEachValueMaxCount)
            {
                errorCollector.AddError($"Break down variable has too many distinct values ({valofValues.Count}). Only {origRow.forEachValueMaxCount} will be displayed, the rest are ignored.");
                for (int i = valofValues.Count - 1; i >= origRow.forEachValueMaxCount; --i) valofValues.RemoveAt(i);
            }

            valofValues.Sort();
            List<Template.Filter> listFilters = new List<Template.Filter>();
            // the row-name in template could be e.g. "Decile [value]" or "[value]. Decile" or "Category [value~MyDescriptions]"
            string rowTitle = origRow.title.Contains("[value") ? origRow.title : $" {origRow.title} [value]";
            foreach (double valofValue in valofValues)
            {
                listFilters.Add(new Template.Filter(localMap)
                {
                    formulaString = $"{(origFilter == null ? string.Empty : $"({origFilter.formulaString}) && ")}DATA_VAR[@valof_var] == {valofValue}",
                    name = ReplaceValuePlaceholder(rowTitle, valofValue), // (mis)use the name to transfer the new title of the row
                    parameters = new List<Template.Parameter>()
                    {
                        new Template.Parameter() { name = "valof_var", variableName = valofVar, _source = Template.Parameter.Source.BASELINE }
                    }
                });
            }
            return listFilters;

            // the placeholder [value] is at run-time replaced by the respective value or description if provided
            string ReplaceValuePlaceholder(string name, double val)
            {
                int iOpening = name.IndexOf("[value"); if (iOpening < 0) return name;
                int iClosing = name.IndexOf(']', iOpening); if (iClosing < 0) return name;
                string placeholder = name.Substring(iOpening, iClosing - iOpening + 1);

                if (origRow.forEachValueDescriptions != null && origRow.forEachValueDescriptions.ContainsKey(val))
                    return name.Replace(placeholder, origRow.forEachValueDescriptions[val]);

                // the following code is still used by HHot, but should be removed and replaced by using the template-api (to modify row.forEachValueDescriptions)
                // if a UserVariable with UserInputType = ForEachValueDescription exists in the template,
                // and the actual user variable is provided by a programme as Dictionary(value, description), these descriptions are used instead (see HHot)
                string[] pHuV = placeholder.Split(HardDefinitions.Separator[0]);
                string userVarName = pHuV.Length <= 1 ? null : pHuV[1].TrimEnd(new char[] { ']' });
                string replacement = val.ToString(CultureInfo.InvariantCulture);
                var desc = from uv in template.info.userVariables
                           where uv.inputType == HardDefinitions.UserInputType.ForEachValueDescription &&
                                 uv.forEachValueDescription != null &&
                                 (string.IsNullOrEmpty(userVarName) || uv.name == userVarName) &&
                                 uv.forEachValueDescription.ContainsKey(val.ToString(CultureInfo.InvariantCulture))
                           select uv;
                if (desc.Any() && (string.IsNullOrEmpty(userVarName) || desc.First().name == userVarName))
                    replacement = desc.First().forEachValueDescription[val.ToString(CultureInfo.InvariantCulture)];
                return name.Replace(placeholder, replacement);
            }
        }

        private bool HandleForEachRow_ErrorCheck(string calculationLevel, LocalMap localMap, Template.Page.Table.Row origRow)
        {
            string error = string.Empty;
            if (origRow.forEachDataRow && !string.IsNullOrEmpty(origRow.forEachValueOf))
                error += "ForEachDataRow=true cannot be combined with ForEachValueOf(variable)";
            if (origRow.forEachDataRow && template.info.templateType == HardDefinitions.TemplateType.Multi)
                error += "ForEachDataRow=true is not possible with TemplateType=Multi (as number of rows is likely to be different for scenarios)";
            if (!string.IsNullOrEmpty(origRow.forEachValueOf))
                for (int dasNo = 0; dasNo < dataAndStats.Count; dasNo++)
                    if (dataAndStats[dasNo][calculationLevel].GetVarIndex(origRow.forEachValueOf, localMap) < 0)
                        error += "ForEachValueOf: variable " + origRow.forEachValueOf + " not found";
            if (origRow.cellAction != null)
            {
                if (!string.IsNullOrEmpty(origRow.cellAction.formulaString) || origRow.cellAction.calculationType != HardDefinitions.CalculationType.NA)
                    error += "ForEachDataRow=true does not allow for content definition on Row level (use Column or Table definition instead)";
                // for completeness one would need to warn about filters on table-, column- or cell-level, as inhertience is broken
            }
            if (string.IsNullOrEmpty(error)) return true;
            errorCollector.AddError(error); return false;
        }

        /**
         * This function is used to create a single display cell for a display table, by merging the attributes of the corresponding table, column, row & custom cell
         */
        private DisplayResults.DisplayPage.DisplayTable.DisplayCell CalculateCellValue(
                                           Dictionary<string, DataStatsHolder> data, Template.Page.Table table, int colNo, int rowNo,
                                           List<CellUnderConstruction> rowUnderConstruction = null, int refNo = -1,
                                           int refNo_SdcPerReform = -1) // this is only required for SDC (for the secondary SDC groups, see CalculateSDCProperties)
        {
            // Get the corresponding row & column, and create a new cell
            Template.Page.Table.Row row = table.rows[rowNo];
            Template.Page.Table.Column column = refNo != -1 ? table.reformColumns[colNo] : table.columns[colNo];

            // Start the new cell based on the corresponding custom cell, if one exists
            List<Template.Page.Table.Cell> customCells = refNo != -1 ? table.reformCells : table.cells;
            Template.Page.Table.Cell cell = customCells.FirstOrDefault(
                x => x.rowName != null && x.rowName.ToLower() == row.name.ToLower() && x.colName != null && x.colName.ToLower() == column.name.ToLower());
            DisplayResults.DisplayPage.DisplayTable.DisplayCell displayCell =
                cell == null ? new DisplayResults.DisplayPage.DisplayTable.DisplayCell()
                             : new DisplayResults.DisplayPage.DisplayTable.DisplayCell()
                               {
                                   stringFormat = cell.stringFormat,
                                   tooltip = cell.tooltip,
                                   strong = cell.strong ?? false,
                                   foregroundColour = cell.foregroundColour,
                                   backgroundColour = cell.backgroundColour,
                                   textAlign = cell.textAlign
                               };

            // Merge the properties of the corresponding table, column, row & custom cell into a single cell, considering reforms
            if (cell == null || cell.cellAction == null || cell.cellAction.calculationType != HardDefinitions.CalculationType.Empty)
                cell = CalculateProperties(cell, column, row, table, refNo, refNo_SdcPerReform);

            if (cell.cellAction.calculationType == HardDefinitions.CalculationType.Empty)
            {
                displayCell.displayValue = string.Empty;
                displayCell.isStringValue = true;
            }
            else if (cell.cellAction.calculationType == HardDefinitions.CalculationType.Info)
            {
                int dasNo = template.info.templateType == HardDefinitions.TemplateType.Multi ? dataAndStats.IndexOf(data) : 0;
                displayCell.displayValue = PrettyInfoProvider.GetPrettyText(template.info, cell.cellAction.formulaString,
                                           baselineSystemInfos[dasNo], reformSystemInfos, packageKey, refNo);
                displayCell.isStringValue = true;
            }
            else
            {
                // Given the actual double value and the merged String Format, calculate the display value
                if (double.IsNaN(displayCell.value)) displayCell.displayValue = "NaN";
                else
                {
                    displayCell.value = HandleAction(data, cell.cellAction, cell.sdcDefinition, out displayCell.sdcObsNo, rowUnderConstruction, refNo);

                    // some info-transfers from cell to displayCell, because still required outside this function (when cell is lost) ...
                    displayCell.stringFormat = cell.stringFormat; // ... though applied here, required for exporting to Excel
                    displayCell.secondarySdcGroups = cell.secondarySdcGroups; // ... required for secondary SDC-check

                    displayCell.displayValue = displayCell.value.ToString(cell.stringFormat, CultureInfo.InvariantCulture);

                    PrimarySDCCheck(displayCell, cell.sdcDefinition); // check for cells which need to be hidden because they are based on too few observations
                }
            }

            // don't forget to add the tooltip (if there is one)
            displayCell.tooltip = cell.tooltip;

            return displayCell;
        }

        private void PrimarySDCCheck(DisplayResults.DisplayPage.DisplayTable.DisplayCell displayCell, Template.Page.Table.SDCDefinition sdcDefinition)
        {
            if (sdcDefinition.suspendSdc != true && displayCell.sdcObsNo < (sdcDefinition.minObsAlternative ?? template.info.sdcMinObsDefault) &&
               (template.info.sdcHideZeroObs == true || displayCell.sdcObsNo != 0))
            {
                displayCell.sdcStatus = DisplayResults.DisplayPage.DisplayTable.DisplayCell.SDC_STATUS.PRIMARY;
                displayCell.value = double.NaN; displayCell.displayValue = string.Empty;
            }

            // (de-)activate the following rows to display test information showing observation number
            //if (displayCell.sdcStatus == DisplayResults.DisplayPage.DisplayTable.DisplayCell.SDC_STATUS.PRIMARY)
            //{
            //    displayCell.backgroundColour = "#CAFF70";
            //    displayCell.isStringValue = true;
            //    displayCell.displayValue += "PrimSdc";
            //}
            //displayCell.displayValue += $" (obs: {displayCell.sdcObsNo})";
        }

        private DisplayResults.DisplayPage PrepareDisplayPage(Template.Page page, int refNo_PerReform = -1)
        {
            // Create a new DisplayTemplate.DisplayPage and copy the main attributes
            DisplayResults.DisplayPage displayPage = new DisplayResults.DisplayPage()
            {
                name = page.name,
                title = PrettyInfoProvider.GetPrettyText(template.info, page.title, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo_PerReform),
                subtitle = PrettyInfoProvider.GetPrettyText(template.info, page.subtitle, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo_PerReform),
                button = new DisplayResults.DisplayPage.VisualDisplayElement()
                {
                    backgroundColour = page.button.backgroundColour,
                    foregroundColour = page.button.foregroundColour,
                    textAlign = page.button.textAlign,
                    strong = page.button.strong ?? false,
                    tooltip = page.button.tooltip,
                    title = PrettyInfoProvider.GetPrettyText(template.info, page.button.title, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo_PerReform)
                },
                html = page.html,
                description = page.description,
                displayTables = new List<DisplayResults.DisplayPage.DisplayTable>()
            };
            return displayPage;
        }

        private DisplayResults.DisplayPage.DisplayTable PrepareDisplayTable(Template.Page.Table table, int refNo_PerReform = -1)
        {
            // Create a new DisplayTable, copy the title, create new column and row lists
            return new DisplayResults.DisplayPage.DisplayTable()
            {
                name = table.name,
                title = PrettyInfoProvider.GetPrettyText(template.info, table.title, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo_PerReform),
                subtitle = PrettyInfoProvider.GetPrettyText(template.info, table.subtitle, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo_PerReform),
                //button = SystemInfo.GetCaptionText(template.info, table.button, baselineSystem[0], reformSystems), // table does not have a button
                description = table.description,
                stringFormat = table.stringFormat,
                columns = new List<DisplayResults.DisplayPage.DisplayTable.DisplayColumn>(),
                rows = new List<DisplayResults.DisplayPage.DisplayTable.DisplayRow>(),
                graph = table.graph
            };
        }

        private List<DisplayResults.DisplayPage.DisplayTable.DisplayRow> PrepareDisplayRows(Template.Page.Table table)
        {
            List<DisplayResults.DisplayPage.DisplayTable.DisplayRow> displayRows = new List<DisplayResults.DisplayPage.DisplayTable.DisplayRow>();
            for (int r = 0; r < table.rows.Count; r++)
            {
                Template.Page.Table.Row row = table.rows[r];
                DisplayResults.DisplayPage.DisplayTable.DisplayRow displayRow = new DisplayResults.DisplayPage.DisplayTable.DisplayRow()
                {
                    title = row.title,
                    stringFormat = row.stringFormat,
                    tooltip = row.tooltip,
                    hasSeparatorAfter = row.hasSeparatorAfter,
                    hasSeparatorBefore = row.hasSeparatorBefore,
                    strong = row.strong ?? false,
                    foregroundColour = row.foregroundColour,
                    backgroundColour = row.backgroundColour,
                    isVisible = row.isVisible,
                    textAlign = row.textAlign
                };
                displayRows.Add(displayRow);
            }
            return displayRows;
        }

        private DisplayResults.DisplayPage.DisplayTable.DisplayColumn PrepareDisplayColumn(Template.Page.Table.Column column, int refNo, int dasNo,
                                                                                           HardDefinitions.ColumnGrouping columnGrouping, bool perReform)
        {
            bool isLastSys = true, isFirstSys = true;
            if (template.info.templateType == HardDefinitions.TemplateType.Multi)
            {
                if (dasNo > 0) isFirstSys = false;
                if (dasNo < baselineSystemInfos.Count - 1) isLastSys = false;
            }
            if (template.info.templateType == HardDefinitions.TemplateType.BaselineReform && refNo != -1 && !perReform)
            {
                if (refNo > 0) isFirstSys = false;
                if (refNo < reformSystemInfos.Count - 1) isLastSys = false;
            }
            return new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
            {
                title = PrettyInfoProvider.GetPrettyText(template.info, column.title, baselineSystemInfos[dasNo], reformSystemInfos, packageKey, refNo),
                stringFormat = column.stringFormat,
                tooltip = column.tooltip,
                hasSeparatorAfter = column.hasSeparatorAfter && (columnGrouping != HardDefinitions.ColumnGrouping.ColumnFirst || isLastSys),
                hasSeparatorBefore = column.hasSeparatorBefore && (columnGrouping != HardDefinitions.ColumnGrouping.ColumnFirst || isFirstSys),
                strong = column.strong ?? false,
                foregroundColour = column.foregroundColour,
                backgroundColour = column.backgroundColour,
                isVisible = column.isVisible,
                textAlign = column.textAlign
            };
        }

        /**
         * This function will accept a table, row, column and custom cell, and merge their properties into a single template cell
         */
        private Template.Page.Table.Cell CalculateProperties(Template.Page.Table.Cell cell, Template.Page.Table.Column column,
                                                             Template.Page.Table.Row row, Template.Page.Table table,
                                                             int refNo, int refNo_SdcPerReform)
        {
            // Create the new cell
            Template.Page.Table.Cell newCell = new Template.Page.Table.Cell() { cellAction = new Template.Action(table.localMap) };

            // Decide on whether this cell should be printed as strong (this should be moved into an inner "display class" eventually, that will hold the format and other related values)
            if (cell != null && cell.strong != null)
                newCell.strong = cell.strong;
            else if (row.strong != null)
                newCell.strong = row.strong;
            else if (column.strong != null)
                newCell.strong = column.strong;

            // Decide on the foregroundColour to be used
            if (cell != null && cell.foregroundColour != null)
                newCell.foregroundColour = cell.foregroundColour;
            else if (row.foregroundColour != null)
                newCell.foregroundColour = row.foregroundColour;
            else if (column.foregroundColour != null)
                newCell.foregroundColour = column.foregroundColour;

            // Decide on the backgroundColour to be used
            if (cell != null && cell.backgroundColour != null)
                newCell.backgroundColour = cell.backgroundColour;
            else if (row.backgroundColour != null)
                newCell.backgroundColour = row.backgroundColour;
            else if (column.backgroundColour != null)
                newCell.backgroundColour = column.backgroundColour;

            // Decide on the textAlign to be used
            if (cell != null && cell.textAlign != null)
                newCell.textAlign = cell.textAlign;
            else if (row.textAlign != null)
                newCell.textAlign = row.textAlign;
            else if (column.textAlign != null)
                newCell.textAlign = column.textAlign;

            // Decide on the string format to be used
            if (cell != null && cell.stringFormat != null)
                newCell.stringFormat = cell.stringFormat;
            else if (row.stringFormat != null)
                newCell.stringFormat = row.stringFormat;
            else if (column.stringFormat != null)
                newCell.stringFormat = column.stringFormat;
            else if (table.stringFormat != null)
                newCell.stringFormat = table.stringFormat;

            if (cell != null && cell.cellAction != null) BlendAction(newCell.cellAction, cell.cellAction);
            if (newCell.cellAction.BlendParameters && newCell.cellAction.calculationType != HardDefinitions.CalculationType.Empty && row.cellAction != null) BlendAction(newCell.cellAction, row.cellAction);
            if (newCell.cellAction.BlendParameters && newCell.cellAction.calculationType != HardDefinitions.CalculationType.Empty && column.cellAction != null) BlendAction(newCell.cellAction, column.cellAction);
            if (newCell.cellAction.BlendParameters && newCell.cellAction.calculationType != HardDefinitions.CalculationType.Empty && table.cellAction != null) BlendAction(newCell.cellAction, table.cellAction);
            
            // Merge the properties for SDC
            CalculateSDCProperties(newCell, cell, column, row, table, refNo, refNo_SdcPerReform);

            // Reform the variable names where appropriate
            if (refNo != -1)
            {
                // if for DATA_VAR[@yem] no parameter { Name=yem, VarName=yem } is provided, PrepareFormula is able to handle that,
                // but it would not work properly for reform-variables, therefore add a respective parameter here (to finally get yem~ref~refno)
                newCell.cellAction.parameters.AddRange(Template.GetDirectRefParameters(newCell.cellAction.formulaString, newCell.cellAction.parameters));
                foreach (Template.Parameter par in newCell.cellAction.parameters)
                    par.SourceAdaptVariableName(refNo);
                if (newCell.cellAction.filter != null)
                {
                    newCell.cellAction.filter.parameters.AddRange(Template.GetDirectRefParameters(newCell.cellAction.filter.formulaString, newCell.cellAction.filter.parameters)); // see comment above
                    foreach (Template.Parameter par in newCell.cellAction.filter.parameters)
                        par.SourceAdaptVariableName(refNo);
                }
            }
            return newCell;
        }

        private void CalculateSDCProperties(Template.Page.Table.Cell newCell, Template.Page.Table.Cell cell,
                                            Template.Page.Table.Column column, Template.Page.Table.Row row, Template.Page.Table table,
                                            int refNo, int refNo_SdcPerReform)
        {
            newCell.sdcDefinition = new Template.Page.Table.SDCDefinition();

            List<string> secGroups = new List<string>();
            if (cell != null) Merge(cell.sdcDefinition); Merge(row.sdcDefinition); Merge(column.sdcDefinition); Merge(table.sdcDefinition);

            void Merge(Template.Page.Table.SDCDefinition parentSDCDefinition)
            {
                if (newCell.sdcDefinition.minObsAlternative == null && parentSDCDefinition.minObsAlternative != null)
                    newCell.sdcDefinition.minObsAlternative = parentSDCDefinition.minObsAlternative;
                if (newCell.sdcDefinition.countNonZeroObsOnly == null && parentSDCDefinition.countNonZeroObsOnly != null)
                    newCell.sdcDefinition.countNonZeroObsOnly = parentSDCDefinition.countNonZeroObsOnly;
                if (newCell.sdcDefinition.ignoreActionFilter == null && parentSDCDefinition.ignoreActionFilter != null)
                    newCell.sdcDefinition.ignoreActionFilter = parentSDCDefinition.ignoreActionFilter;
                secGroups.AddRange(parentSDCDefinition.secondaryGroups); // a cell belongs to its own groups and those of its parents
                if (newCell.sdcDefinition.suspendSecondaryGroups == null && parentSDCDefinition.suspendSecondaryGroups != null)
                    newCell.sdcDefinition.suspendSecondaryGroups = parentSDCDefinition.suspendSecondaryGroups;
                if (newCell.sdcDefinition.suspendSdc == null && parentSDCDefinition.suspendSdc != null)
                    newCell.sdcDefinition.suspendSdc = parentSDCDefinition.suspendSdc;
            }

            if (newCell.sdcDefinition.suspendSecondaryGroups != true)
            {
                foreach (string sdcG in secGroups.Distinct())
                {
                    string secondarySdcGroupName = sdcG;
                    // secondary SDC-check must be per reform: thus add ~ref~X to distinguish (analog) cells of different reforms
                    if (refNo != -1) secondarySdcGroupName += HardDefinitions.Reform + refNo;
                    // base cells within a per-reform-page must also be distinguished from (analog) base cells of another reform: thus add ~base~X
                    // note: this approach is a problem if the group contains cells outside the page, but as this is not overly likely ...
                    if (refNo_SdcPerReform != -1) secondarySdcGroupName += HardDefinitions.Base + refNo_SdcPerReform;
                    newCell.secondarySdcGroups.Add(secondarySdcGroupName);
                }
            }
        }

        private void BlendAction(Template.Action newAction, Template.Action oldAction)
        {
            if (string.IsNullOrEmpty(newAction.formulaString) && !string.IsNullOrEmpty(oldAction.formulaString)) newAction.formulaString = oldAction.formulaString;
            if (string.IsNullOrEmpty(newAction._calculationLevel) && !string.IsNullOrEmpty(oldAction._calculationLevel)) newAction._calculationLevel = oldAction._calculationLevel;
            if (newAction.calculationType == HardDefinitions.CalculationType.NA && oldAction.calculationType != HardDefinitions.CalculationType.NA) newAction.calculationType = oldAction.calculationType;
            if (string.IsNullOrEmpty(newAction.outputVar) && !string.IsNullOrEmpty(oldAction.outputVar)) newAction.outputVar = oldAction.outputVar;
            if (newAction._reform == null && oldAction._reform != null) newAction._reform = oldAction._reform;
            if (newAction._saveResult == null && oldAction._saveResult != null) newAction._saveResult = oldAction._saveResult;
            if (newAction._blendParameters == null && oldAction._blendParameters != null) newAction._blendParameters = oldAction._blendParameters;

            if (oldAction.filter != null)
            {
                if (newAction.filter == null) newAction.filter = new Template.Filter(newAction.localMap);
                BlendFilter(newAction.filter, oldAction.filter);
            }

            BlendParameters(newAction.parameters, oldAction.parameters);
        }

        private void BlendFilter(Template.Filter newFilter, Template.Filter oldFilter)
        {
            if (oldFilter != null && newFilter != null)
            {
                // if a filter existed, try to blend the two filters
                if (string.IsNullOrEmpty(newFilter.formulaString)
                    && !string.IsNullOrEmpty(oldFilter.formulaString))
                    newFilter.formulaString = oldFilter.formulaString;
                if (string.IsNullOrEmpty(newFilter.name) && !string.IsNullOrEmpty(oldFilter.name))
                    newFilter.name = oldFilter.name;
                if (oldFilter.parameters != null)
                    BlendParameters(newFilter.parameters, oldFilter.parameters);
            }
        }

        private void BlendParameters(List<Template.Parameter> newPars, List<Template.Parameter> oldPars)
        {
            if (oldPars == null || newPars == null) return;
            foreach (Template.Parameter oldPar in oldPars)
            {
                if (!string.IsNullOrEmpty(oldPar.name))
                {
                    if (newPars.Any(x => x.name == oldPar.name))
                    {
                        // if a named parameter exists, try to blend it
                        Template.Parameter par = newPars.First(x => x.name == oldPar.name);
                        // blend the parameter type
                        if (par._source == null) par._source = oldPar._source;
                    }
                    else
                    {
                        // add all non-existing parameters
                        newPars.Add(new Template.Parameter(oldPar));
                    }
                }
                else
                {
                    // add all unamed parameters
                    newPars.Add(new Template.Parameter(oldPar));
                }
            }
        }

        private bool CompileTemplate()
        {
            // Prepare the display pages
            foreach (Template.Page page in template.pages)
            {
                if (!page.active) continue;
                PrepareActionsAndFilters(page.actions, page.filters);

                if (page.perReform) // special case for template-type baseline-reform: create a page for each reform, instead of one page with a column for each reform
                    for (int refNo = 0; refNo < reformSystemInfos.Count; refNo++) _CreateDisplayPage(refNo);
                else _CreateDisplayPage(); // the usual case

                void _CreateDisplayPage(int refNo_PagePerReform = -1)
                {
                    DisplayResults.DisplayPage displayPage = PrepareDisplayPage(page, refNo_PagePerReform);
                    foreach (Template.Page.Table table in page.tables)
                    {
                        if (!table.active) continue;
                        PrepareActionsAndFilters(table.actions, table.filters);

                        if (table.perReform && !page.perReform) // special case for template-type baseline-reform: create a table for each reform, instead of one table with a column for each reform (ignore table per Reform, if already the page is per Reform)
                            for (int refNo = 0; refNo < reformSystemInfos.Count; refNo++) _CreateDisplayTable(refNo);
                        else _CreateDisplayTable(refNo_PagePerReform); // the usual case

                        void _CreateDisplayTable(int refNo_PerReform)
                        {
                            if (CreateDisplayTable(out DisplayResults.DisplayPage.DisplayTable displayTable, table, refNo_PerReform))
                                displayPage.displayTables.Add(displayTable);
                        }
                    }
                    displayResults.displayPages.Add(displayPage);
                }
            }

            // Check for secondary SDC-cells, i.e. cells that need to be hidden to prevent deducting primary SDC-cells (cells which are hidden because they are based on too few observations)
            SecondarySDCCheck();

            displayResults.calculated = true;
            return true;
        }

        private void SecondarySDCCheck() // check for cells that need to be hidden to prevent deducting primary SDC-cells
        {
            // first collect the cells into the SDC-groups
            Dictionary<string, List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>> allGroups = new Dictionary<string, List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>>();
            foreach (DisplayResults.DisplayPage displayPage in displayResults.displayPages)
            {
                foreach (DisplayResults.DisplayPage.DisplayTable displayTable in displayPage.displayTables)
                {
                    for (int c = 0; c < displayTable.columns.Count; c++)
                    {
                        for (int r = 0; r < displayTable.rows.Count; r++)
                        {
                            foreach (string sdcGroup in displayTable.cells[r][c].secondarySdcGroups) // note: a cell can belong to more than one SDC-group
                            {
                                if (!allGroups.ContainsKey(sdcGroup)) allGroups.Add(sdcGroup, new List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>());
                                allGroups[sdcGroup].Add(displayTable.cells[r][c]);
                            }
                        }
                    }
                }
            }

            // then check all groups and hide more cells as required
            foreach (List<DisplayResults.DisplayPage.DisplayTable.DisplayCell> sdcGroup in allGroups.Values)
            {
                // if the group contains exactly one hidden cell (i.e. a single primary SDC-cell) the cell with the next least observarions needs to be hidden too
                if (sdcGroup.Count > 1 &&
                    // if the group contains exactly one primary SDC-cell ...
                    sdcGroup.Count(x => x.sdcStatus == DisplayResults.DisplayPage.DisplayTable.DisplayCell.SDC_STATUS.PRIMARY) == 1 &&
                    // ... and there is not already a second hidden cell (secondarily hidden due to another group)
                    sdcGroup.Count(x => x.sdcStatus == DisplayResults.DisplayPage.DisplayTable.DisplayCell.SDC_STATUS.SECONDARY) == 0)
                {
                    // then hide the cell with the smallest number of observations
                    var orderedCells = sdcGroup.Where(x => x.sdcStatus == DisplayResults.DisplayPage.DisplayTable.DisplayCell.SDC_STATUS.NONE)
                                               .OrderBy(x => x.sdcObsNo);
                    DisplayResults.DisplayPage.DisplayTable.DisplayCell cell;
                    if (template.info.sdcHideZeroObs) cell = orderedCells.First();
                    else // take option of not hidinge 0-obs-cells into account, but if there is only one cell left, hide it anyway, even if it has 0 obs
                        cell = orderedCells.Where(x => x.sdcObsNo != 0).FirstOrDefault() ?? orderedCells.First();
                    cell.sdcStatus = DisplayResults.DisplayPage.DisplayTable.DisplayCell.SDC_STATUS.SECONDARY;
                    cell.displayValue = string.Empty; cell.value = double.NaN;

                    // (de-)activate the following row to display test information showing observation number
                    //cell.displayValue += $"SecSdc (obs: {cell.sdcObsNo})"; cell.backgroundColour = "#C1FFC1"; cell.isStringValue = true;
                }
            }
        }

        /// <summary>
        /// This function returns the full compiled display template object that will be shown in the StatisticsPresenter.
        /// </summary>
        /// <returns>Returns a list of DisplayTemplate.DisplayPage elements.</returns>
        public DisplayResults GetDisplayResults()
        {
            foreach (DisplayResults.DisplayPage page in displayResults.displayPages)
                foreach (DisplayResults.DisplayPage.DisplayTable table in page.displayTables)
                {
                    for (int r = table.rows.Count - 1; r >= 0; --r)
                        if (!table.rows[r].isVisible)
                        {
                            table.rows.RemoveAt(r);
                            if (table.cells.Count > r) // just for safety
                                table.cells.RemoveAt(r);
                        }
                    for (int c = table.columns.Count - 1; c >= 0; --c)
                        if (!table.columns[c].isVisible)
                        {
                            table.columns.RemoveAt(c);
                            for (int r = 0; r < table.rows.Count; ++r)
                                if (table.cells.Count > r && table.cells[r].Count > c) // just for safety
                                    table.cells[r].RemoveAt(c);
                        }
                }
            return displayResults;
        }
    }
}
