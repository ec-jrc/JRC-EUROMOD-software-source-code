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
        private readonly List<SystemInfo> baselineSystemInfos = new List<SystemInfo>(); // base-only, base-reform: info about base-system, i.e. count = 1)
                                                                                        // multi: info about all systems
        private readonly List<SystemInfo> reformSystemInfos = new List<SystemInfo>();   // base-reform: info about reform-systems, empty else

        private readonly List<Dictionary<string, DataStatsHolder>> allData = // base-only, base-reform: 'the' data and statistics, i.e. count = 1
                     new List<Dictionary<string, DataStatsHolder>>();        // multi: data and statistics of all datasets
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
            ParameterExpression[] OP_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "OP_VAR") };
            Func<List<double>, double> func = (Func<List<double>, double>)DynamicExpressionParser.ParseLambda(OP_VAR, typeof(double), "3.0 / 2.0", Array.Empty<object>()).Compile();
            return func(null);
        }

        public EM_TemplateCalculator(Template _template = null, string _packageKey = null) {
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
        /// <param name="allMicroData">The microdata in memory</param>
        /// <returns>True if the statistics were build successfuly, false if there was an exception</returns>
        public bool Prepare(List<string> allFilenames, out ErrorCollector _errorCollector, List<StringBuilder> allMicroData = null, bool calculateResults = false)
        {
            errorCollector = new ErrorCollector(); _errorCollector = errorCollector;

            if (template == null)
            { errorCollector.AddError("No template available."); return false; }
            if (allFilenames == null || allFilenames.Count < 1 || (template.info.templateType == HardDefinitions.TemplateType.BaselineReform && allFilenames.Count < 2))
            { errorCollector.AddError("Insufficient number of files."); return false; };

            switch (template.info.templateType)
            {
                case HardDefinitions.TemplateType.Default:
                    baselineSystemInfos.Add(new SystemInfo(allFilenames[0]));
                    break;

                case HardDefinitions.TemplateType.Multi:
                    for (int dataNo = 0; dataNo < allFilenames.Count; dataNo++) baselineSystemInfos.Add(new SystemInfo(allFilenames[dataNo]));
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

            return calculateResults ? CalculateStatistics(allMicroData) : true;
        }

        /// <summary>
        /// Calculates all statistics using output in files or memory
        /// </summary>
        /// <param name="allFilenames">The output filenames</param>
        /// <param name="_errorCollector">A collector of all errors</param>
        /// <param name="allMemoryData">The microdata in memory</param>
        /// <returns>True if the statistics were build successfuly, false if there was an exception</returns>
        public bool CalculateStatistics(List<StringBuilder> allMemoryData = null)
        {
            try
            {
                if (!Instantiate(allMemoryData)) return false;
                return CompileTemplate();
            }
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
                    allData.Add(defData);
                    break;

                case HardDefinitions.TemplateType.Multi:
                    for (int dataNo = 0; dataNo < baselineSystemInfos.Count; dataNo++)
                    {
                        if (allMemoryData != null && allMemoryData.Count() > dataNo) baselineSystemInfos[dataNo].SetMemoryData(allMemoryData[dataNo]);
                        if (!ReadData(out Dictionary<string, DataStatsHolder> mulData, errorCollector,
                            template.info, packageKey, baselineSystemInfos[dataNo], null, dataNo)) return false;
                        allData.Add(mulData);
                    }
                    break;

                case HardDefinitions.TemplateType.BaselineReform:
                    if (allMemoryData != null)
                    {
                        if (allMemoryData.Any()) baselineSystemInfos[0].SetMemoryData(allMemoryData[0]);
                        for (int r = 0; r < reformSystemInfos.Count; ++r) if (allMemoryData.Count > r + 1) reformSystemInfos[r].SetMemoryData(allMemoryData[r + 1]);
                    }
                    if (!ReadData(out Dictionary<string, DataStatsHolder> basData, errorCollector,
                        template.info, packageKey, baselineSystemInfos[0], reformSystemInfos)) return false;
                    allData.Add(basData);

                    // Multiply template filters & actions for each reform (unless marked otherwise in XML)
                    List<Template.Filter> newFilters = new List<Template.Filter>();
                    foreach (Template.Filter filter in template.globalFilters)
                    {
                        if (!filter.reform) continue;
                        for (int c = 1; c <= ReformNumber(); c++)
                        {
                            Template.Filter newFilter = ReformFilter(filter, c);
                            newFilters.Add(newFilter);
                        }
                    }
                    template.globalFilters.AddRange(newFilters);

                    List<Template.Action> reformActions = new List<Template.Action>();
                    foreach (Template.Action action in template.globalActions)
                    {
                        if (!action.Reform) continue;
                        for (int c = 1; c <= ReformNumber(); c++)
                        {
                            Template.Action newAction = ReformAction(action, c);
                            reformActions.Add(newAction);
                        }
                    }
                    template.globalActions.AddRange(reformActions);

                    break;
            }

            // Finally prepare each filter for each data
            foreach (Dictionary<string, DataStatsHolder> data in allData)
                foreach (Template.Filter filter in template.globalFilters)
                    PrepareFilter(data[IND], filter, template); // maybe not necessary, just prepare on usage
            // and prepare & handle all actions for each dataset
            foreach (Template.Action action in template.globalActions)
                HandleGlobalAction(action);

            return true;
        }

        private bool ReadData(out Dictionary<string, DataStatsHolder> allData, ErrorCollector errorCollector,
                              Template.TemplateInfo templateInfo, string packageKey,
                              SystemInfo baseSystemInfo, List<SystemInfo> reformSystemInfos = null,
                              int dataNo = 0)
        {
            allData = new Dictionary<string, DataStatsHolder>();
            if (templateInfo == null) return false;
            foreach (Template.TemplateInfo.CalculationLevel cl in template.info.calculationLevels)
            {
                allData.Add(cl.name, new DataStatsHolder(cl.groupingVar, dataNo, cl.name, packageKey));
                if (!templateInfo.requiredVariables.Any(x => x.name == cl.groupingVar))
                {
                    errorCollector.AddError($"Template Error: Calculation level '{cl.groupingVar}' should be defined as a required variable.");
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
            foreach (Template.TemplateInfo.RequiredVariable v in templateInfo.requiredVariables)
            {
                if (allData[IND].HasVariable(v.name)) continue;
                allData[IND].AddVar(v.name);
                int index = headers.IndexOf(v.readVar.ToLower());
                if (index < 0) 
                    errorCollector.AddError($"File {baseSystemInfo.GetFileName()} does not contain required variable '{v.readVar}'.");
                varColumnIndices.Add(v.name, index);
            }
            if (errorCollector.HasErrors()) return false;   // if CalculationLevel or required variables are missing, it is too dangerous to continue!

            if (templateInfo.optionalVariables != null)
                foreach (Template.TemplateInfo.OptionalVariable v in templateInfo.optionalVariables)
                {
                    if (allData[IND].HasVariable(v.name)) continue;
                    allData[IND].AddVar(v.name); varColumnIndices.Add(v.name, headers.IndexOf(v.readVar.ToLower()));
                }
            if (templateInfo.userVariables != null)
                foreach (Template.TemplateInfo.UserVariable v in templateInfo.userVariables.Where(x => x.inputType == HardDefinitions.UserInputType.VariableName))
                {
                    if (allData[IND].HasVariable(v.value)) continue;
                    allData[IND].AddVar(v.value); varColumnIndices.Add(v.value, headers.IndexOf(v.value));
                }
            allData[IND].ConfirmKey();

            // now read the data-lines, but only add the necessary variables
            foreach (string inputLine in dataRows.Skip(1))
            {
                string[] splitInputLine = inputLine.Replace(',', '.').Split('\t');
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
                allData[IND].AddObs(onePersonsVariablesDouble);
            }

            // also create the HH level and other grouping tables with only their key, weight & number of individuals to start with
            foreach (Template.TemplateInfo.CalculationLevel cl in template.info.calculationLevels)
            {
                if (cl.name == IND) continue;

                allData[cl.name].AddVar(cl.groupingVar);
                allData[cl.name].AddVar(HardDefinitions.weight);
                allData[cl.name].AddVar(HardDefinitions.NumberOfIndividuals);
                allData[IND].AddVar(cl.groupingVar + HardDefinitions.NumberOfIndividuals);
                int keyIndex = allData[IND].GetVarIndex(cl.groupingVar);
                int weightIndex = allData[IND].GetVarIndex(HardDefinitions.weight);

                allData[cl.name].ConfirmKey();
                var tmpGroup = from person in allData[IND].GetData()
                            group person by person[keyIndex] into HH
                            select HH;
                foreach (var gp in tmpGroup)
                {
                    double key = gp.ElementAt(0)[keyIndex];
                    double weightGroup = gp.ElementAt(0)[weightIndex];
                    double indNo = gp.Count();
                    allData[cl.name].AddObs(new List<double>() { key, weightGroup, indNo });
                    foreach (List<double> i in gp)
                        i.Add(indNo);
                }
            }

            if (reformSystemInfos == null) return true;

            // redo the process omiting/matching base vars, for the reforms
            int reformNumber = 0;
            foreach (SystemInfo reformSystemInfo in reformSystemInfos)
            {
                dataRows = reformSystemInfo.GetDataRows(); ++reformNumber;
                if (dataRows.Count() != baselineRowsCount) { errorCollector.AddError(
                    $"Reform file {reformSystemInfo.GetFileName()} has a different number of observation than base file {baseSystemInfo.GetFileName()}."); return false; }

                // first read header line to get the column-indices of the variables to read
                headers = dataRows.First().ToLower().Split('\t').ToList();
                varColumnIndices = new Dictionary<string, int>();
                foreach (Template.TemplateInfo.RequiredVariable v in templateInfo.requiredVariables)
                {
                    string refVar = v.name + HardDefinitions.Reform + reformNumber;
                    if (allData[IND].HasVariable(refVar)) continue;
                    allData[IND].AddVar(refVar);
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
                    string refVar = v.name + HardDefinitions.Reform + reformNumber;
                    if (allData[IND].HasVariable(refVar)) continue;
                    allData[IND].AddVar(refVar); varColumnIndices.Add(refVar, headers.IndexOf(v.readVar.ToLower()));
                }
                foreach (Template.TemplateInfo.UserVariable v in templateInfo.userVariables.Where(x => x.inputType == HardDefinitions.UserInputType.VariableName))
                {
                    string refVar = v.value + HardDefinitions.Reform + reformNumber;
                    if (allData[IND].HasVariable(refVar)) continue;
                    allData[IND].AddVar(refVar); varColumnIndices.Add(refVar, headers.IndexOf(v.value));
                }
                // now read the data-lines, but only add the necessary variables
                int idPersonIndex = allData[IND].GetVarIndex(HardDefinitions.idPerson);
                foreach (string inputLine in dataRows.Skip(1))
                {
                    string[] splitInputLine = inputLine.Replace(',', '.').Split('\t');
                    List<double> onePersonsVariablesDouble = new List<double>();
                    foreach (var varColumnIndex in varColumnIndices)
                    {
                        if (varColumnIndex.Value < 0)
                        {
                            // if it is a missing Optional Variable, then use its default value
                            if (templateInfo.optionalVariables.Exists(x => x.name == GetBaseVarName(varColumnIndex.Key, reformNumber)))
                                onePersonsVariablesDouble.Add(templateInfo.optionalVariables.Find(x => x.name == GetBaseVarName(varColumnIndex.Key, reformNumber)).defaultValue);
                            // else the variable should be there! 
                            else
                            {
                                errorCollector.AddError($"Variable {GetBaseVarName(varColumnIndex.Key, reformNumber)} not found in reform file {reformSystemInfo.GetFileName()}."); return false;
                            }
                        }
                        else onePersonsVariablesDouble.Add(double.Parse(splitInputLine.ElementAt(varColumnIndex.Value), CultureInfo.InvariantCulture));
                    }
                    if (!allData[IND].HasObs(onePersonsVariablesDouble[idPersonIndex]))
                    {
                        errorCollector.AddError($"Mismatch in {HardDefinitions.idPerson} between reform file {reformSystemInfo.GetFileName()} and base file {baseSystemInfo.GetFileName()}." + Environment.NewLine +
                            $"Base file does not contain {HardDefinitions.idPerson} {onePersonsVariablesDouble[idPersonIndex]}.");
                        return false;
                    }
                    allData[IND].GetObs(onePersonsVariablesDouble[idPersonIndex]).AddRange(onePersonsVariablesDouble);
                }
            }

            return true;
        }

        public string GetErrorMessage()
        {
            return errorCollector.GetErrorMessage();
        }

        private void HandleGlobalAction(Template.Action action)
        {
            for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                HandleAction(allData[dataNo], action, new Template.Page.Table.SDCDefinition(), out int dummy);
        }

        private double HandleAction(Dictionary<string, DataStatsHolder> data, Template.Action action,
                                    Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo,
                                    List<DisplayResults.DisplayPage.DisplayTable.DisplayCell> baseRowCells = null,
                                    Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell> reformRowCells = null)
        {
            sdcObsNo = int.MaxValue;
            try
            {
                if (action.calculationType == HardDefinitions.CalculationType.NA) return ActionError(action, "CalculationType not defined.");
                if (action.calculationType == HardDefinitions.CalculationType.Empty) return double.NaN;

                action = FixActionUserVariables(action, template);
                PrepareFilter(data[action.CalculationLevel], action.filter, template);

                if (action.calculationType == HardDefinitions.CalculationType.CreateArithmetic)
                {
                    // createArithmetic requires a formula and will use it at the "inner" level
                    if (string.IsNullOrEmpty(action.formulaString)) return ActionError(action, $"{action.calculationType} requires a <FormulaString>");
                    if (!PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, action.formulaString, data[action.CalculationLevel], action.parameters, out string error, true)) return ActionError(action, error);
                    ParameterExpression[] OBS_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "OBS_VAR") };
                    action.func = ParseFormula(OBS_VAR, formula);
                    action.result = CreateArithmeticColumn(data, action) ? 0 : double.NaN;
                }
                else if (HardDefinitions.InEnumList(action.calculationType, new HardDefinitions.CalculationType[] { HardDefinitions.CalculationType.CreateEquivalized, HardDefinitions.CalculationType.CreateOECDScale, HardDefinitions.CalculationType.CreateEquivalenceScale, HardDefinitions.CalculationType.CalculateGini, HardDefinitions.CalculationType.CalculateS8020, HardDefinitions.CalculationType.CreateDeciles, HardDefinitions.CalculationType.CalculateMedian, HardDefinitions.CalculationType.CreateGroupValue, HardDefinitions.CalculationType.CreateHHValue, HardDefinitions.CalculationType.CalculatePovertyGap, HardDefinitions.CalculationType.CreateFlag }))
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
                        case HardDefinitions.CalculationType.CreateDeciles:
                            action.result = CreateDeciles(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateEquivalized:
                            action.result = CreateEquivalized(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateGroupValue:
                            action.result = CreateGroupValue(data, action) ? 0 : double.NaN;
                            break;
                        case HardDefinitions.CalculationType.CreateHHValue:  // this is here for support of older templates
                            action._calculationLevel = HH;
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
                    if (!PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, action.formulaString, data[action.CalculationLevel], action.parameters, out string error)) return ActionError(action, error);

                    if (action.calculationType == HardDefinitions.CalculationType.CalculateArithmetic)
                    {
                        // The CalculateArithmetic is the only type allowed to use column values
                        if (!PrepareFormulaColumns(ref formula, out error, baseRowCells, reformRowCells)) return ActionError(action, error);
                        if (formula.Contains("OP_VAR") || formula.Contains("OBS_VAR")) return ActionError(action, $"{action.calculationType} does not allow for OP_VAR[] or OBS_VAR[]");
                        ParameterExpression[] NO_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "NO_VAR") };
                        action.func = ParseFormula(NO_VAR, constMakeDouble + formula);
                        action.result = CalculateArithmeticAction(data, action, sdcDefinition, out sdcObsNo);
                        sdcObsNo = Math.Min(sdcObsNo, sdcObsNo_SavedNumbers);
                    }
                    else
                    {
                        // Handle all Operation variables
                        int pos = formula.IndexOf(HardDefinitions.FormulaParameter.OP_VAR);
                        List<double> opvars = new List<double>();
                        sdcObsNo = sdcObsNo_SavedNumbers;
                        while (pos > -1)
                        {
                            int startpos = pos + HardDefinitions.FormulaParameter.OP_VAR.Length;
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
                            pos = formula.IndexOf(HardDefinitions.FormulaParameter.OP_VAR, startpos + cnt.Length);
                        }

                        ParameterExpression[] OP_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "OP_VAR") };
                        action.func = ParseFormula(OP_VAR, constMakeDouble + formula);
                        action.result = action.func(opvars);

                        if (!string.IsNullOrEmpty(action.outputVar))
                            data[action.CalculationLevel].SetSavedNumber(action.outputVar, new DataStatsHolder.SavedNumber(action.result, sdcObsNo));
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
        private void PrepareFilter(DataStatsHolder data, Template.Filter filter, Template template)
        {
            foreach(Template.TemplateInfo.CalculationLevel cl in template.info.calculationLevels)
            if (filter == null || filter.HasFunc(data)) return; // if filter is null, or it already exists (check for IND should be enough), return
            if (!string.IsNullOrEmpty(filter.name) && template.globalFilters.Count(y => y.name == filter.name) == 1)
            {
                // If a named filter already excists in the globals, combine missing properties
                Template.Filter namedFilter = template.globalFilters.First(y => y.name == filter.name);
                if (string.IsNullOrEmpty(filter.formulaString) && namedFilter.formulaString != null) filter.formulaString = namedFilter.formulaString;
                if (filter.parameters.Count == 0 && namedFilter.parameters.Count > 0)
                    foreach (Template.Parameter par in namedFilter.parameters)
                        filter.parameters.Add(par);
            }
            if (string.IsNullOrEmpty(filter.formulaString)) return;

            if (filter.func == null) filter.func = new Dictionary<string, Func<List<double>, bool>>();
            Func<List<double>, bool> func = funcHandingNaN;
            try
            {
                if (PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, filter.formulaString, data, filter.parameters, out string error, true))
                {
                    ParameterExpression[] OBS_VAR = new ParameterExpression[] { Expression.Parameter(typeof(List<double>), "OBS_VAR") };
                    if (!formula.Contains(double.NaN.ToString()))
                        func = (Func<List<double>, bool>)DynamicExpressionParser.ParseLambda(OBS_VAR, null, formula, Array.Empty<object>()).Compile();
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
                string ident = string.IsNullOrEmpty(filter.name) ? string.Empty : filter.name;
                if (string.IsNullOrEmpty(ident))
                {
                    string fs = filter.formulaString.Length < 20 ? filter.formulaString : filter.formulaString.Substring(0, 20);
                    ident += $"FormulaString={fs}{(fs.Length == filter.formulaString.Length ? string.Empty : " ...")}";
                }
                errorCollector.AddError($"Error in Filter {ident.Trim()}.{Environment.NewLine}{msg}.");
            }

            bool funcHandingNaN(List<double> d) { return false; }
        }

        private bool PrepareFormula(out string formula, out int sdcObsNo_SavedNumbers, string origFormula, DataStatsHolder data, List<Template.Parameter> parameters, 
                                    out string error, bool isOBS_VAR = false)
        {
            // the library can generate formulas with direct indexes - see Gini
            // for user generated, parameter names need to start with "@"

            // in all our replacements, we add (" + doubleMultiplier + " ... ) to make sure that all our parameters are considered doubles! otherwise, you may get an integer division or even integer result which would cause a crash!

            formula = origFormula; error = null; sdcObsNo_SavedNumbers = int.MaxValue;
            // Handle all Saved Variables
            int pos = formula.IndexOf(HardDefinitions.FormulaParameter.SAVED_VAR + "@");
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.SAVED_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid saved variable in formula", ref error);
                string par_name = formula.Substring(startpos, endpos - startpos);
                Template.Parameter saved_name = parameters.FirstOrDefault(x => x.name == par_name);
                if (saved_name == null) return PrepareFormulaError($"Parameter {par_name} not found", ref error);
                if (string.IsNullOrEmpty(saved_name.variableName)) return PrepareFormulaError($"Parameter {par_name}: <VarName> not defined", ref error);
                if (!data.HasSavedNumber(saved_name.variableName)) return PrepareFormulaError($"saved variable {saved_name.name} (variable {saved_name.variableName}) not found", ref error);
                DataStatsHolder.SavedNumber savedNumber = data.GetSavedNumber(saved_name.variableName);
                sdcObsNo_SavedNumbers = Math.Min(savedNumber.sdcObsNo, sdcObsNo_SavedNumbers);
                string snStr = "(" + constMakeDouble + savedNumber.number.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")";
                formula = formula.Substring(0, pos) + snStr + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.SAVED_VAR, pos + snStr.Length);
            }

            // Handle all User Variables
            pos = formula.IndexOf(HardDefinitions.FormulaParameter.USR_VAR + "@");
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.USR_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid user variable in formula", ref error);
                string par_name = formula.Substring(startpos, endpos - startpos);
                Template.Parameter user_var_name = parameters.FirstOrDefault(x => x.name == par_name);
                if (user_var_name == null) return PrepareFormulaError($"Parameter {par_name} not found", ref error);
                if (string.IsNullOrEmpty(user_var_name.variableName)) return PrepareFormulaError($"Parameter {par_name}: <VarName> not defined", ref error);

                int refNo = -1; string uvName = user_var_name.variableName;
                if (user_var_name.variableName.Contains(HardDefinitions.Reform)) // one could pass the reform-number through per parameter
                {                                                                // but it is less change-effort to extract it from the variable-name
                    string rn = uvName.Substring(uvName.IndexOf(HardDefinitions.Reform) + HardDefinitions.Reform.Length); // extract e.g. "2" from "Const1~ref~2"
                    if (int.TryParse(rn, out refNo)) uvName = uvName.Substring(0, uvName.IndexOf(HardDefinitions.Reform));
                }
                Template.TemplateInfo.UserVariable uservar = template.info.GetUserVariable(uvName, packageKey, refNo);
                if (uservar == null || string.IsNullOrEmpty(uservar.value)) return PrepareFormulaError($"user variable {user_var_name.name} not found", ref error);

                string value = "";
                if (uservar.inputType == HardDefinitions.UserInputType.Numeric || uservar.inputType == HardDefinitions.UserInputType.Categorical_Numeric)
                {
                    value = "(" + constMakeDouble + uservar.value + ")";
                }
                else if (uservar.inputType == HardDefinitions.UserInputType.VariableName || uservar.inputType == HardDefinitions.UserInputType.Categorical_VariableName)
                {
                    int varIndex = data.GetVarIndex(uservar.value);
                    if (varIndex < 0) return PrepareFormulaError($"user variable {uservar.name} (variable {uservar.value}) not found", ref error);
                    value = "(" + constMakeDouble + (isOBS_VAR ? HardDefinitions.FormulaParameter.OBS_VAR : HardDefinitions.FormulaParameter.OP_VAR) + varIndex.ToString() + HardDefinitions.FormulaParameter.CLOSING_TOKEN + ")";
                }
                formula = formula.Substring(0, pos) + value + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.USR_VAR, pos + value.Length);
            }

            // Handle all Operation variables
            pos = formula.IndexOf(HardDefinitions.FormulaParameter.OP_VAR + "@");
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.OP_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid variable in formula", ref error);
                string par_name = formula.Substring(startpos, endpos - startpos);
                Template.Parameter var_name = parameters.FirstOrDefault(x => x.name == par_name);
                if (var_name == null) return PrepareFormulaError($"Parameter {par_name} not found", ref error);
                if (string.IsNullOrEmpty(var_name.variableName)) return PrepareFormulaError($"Parameter {par_name}: <VarName> not defined", ref error);
                string varIndex = data.GetVarIndex(var_name.variableName).ToString();
                if (varIndex == "-1") return PrepareFormulaError($"variable {var_name.variableName} not found", ref error);
                varIndex = "(" + constMakeDouble + HardDefinitions.FormulaParameter.OP_VAR + varIndex + HardDefinitions.FormulaParameter.CLOSING_TOKEN + ")";
                formula = formula.Substring(0, pos) + varIndex + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.OP_VAR, pos + varIndex.Length);
            }

            // Handle all Observation variables
            pos = formula.IndexOf(HardDefinitions.FormulaParameter.OBS_VAR + "@");
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.OBS_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid variable in formula", ref error);
                string par_name = formula.Substring(startpos, endpos - startpos);
                Template.Parameter var_name = parameters.FirstOrDefault(x => x.name == par_name);
                if (var_name == null) return PrepareFormulaError($"Parameter {par_name} not found", ref error);
                if (string.IsNullOrEmpty(var_name.variableName)) return PrepareFormulaError($"Parameter {par_name}: <VarName> not defined", ref error);
                string varIndex = data.GetVarIndex(var_name.variableName).ToString();
                if (varIndex == "-1") return PrepareFormulaError($"variable {var_name.variableName} not found", ref error);
                varIndex = "(" + constMakeDouble + HardDefinitions.FormulaParameter.OBS_VAR + varIndex + HardDefinitions.FormulaParameter.CLOSING_TOKEN + ")";
                formula = formula.Substring(0, pos) + varIndex + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.OBS_VAR, pos + varIndex.Length);
            }
            pos = formula.IndexOf(HardDefinitions.FormulaParameter.TEMP_VAR + "@");
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.TEMP_VAR.Length + 1; // +1 to take "@" into account
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) return PrepareFormulaError("invalid template variable in formula", ref error);
                string par_name = formula.Substring(startpos, endpos - startpos);
                Template.Parameter var_name = parameters.FirstOrDefault(x => x.name == par_name);
                if (var_name == null) return PrepareFormulaError($"Parameter {par_name} not found", ref error);
                string varValue = "(" + constMakeDouble + var_name.numericValue.ToString() + ")";
                formula = formula.Substring(0, pos) + varValue + formula.Substring(endpos + 1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.TEMP_VAR, pos + varValue.Length);
            }
            return true;

            bool PrepareFormulaError(string msg, ref string err) { err = $"Faulty <FormulaString>: {msg}"; return false; }
        }

        private static bool PrepareFormulaColumns(ref string formula, out string error, List<DisplayResults.DisplayPage.DisplayTable.DisplayCell> baseRowCells, Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell> reformRowCells)
        {
            // Handle all Base Columns
            error = null;

            int pos = formula.IndexOf(HardDefinitions.FormulaParameter.BASE_COL);
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.BASE_COL.Length;
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) { error = "Invalid base column in formula"; return false; }

                string colNumber = formula.Substring(startpos, endpos - startpos);
                if (!int.TryParse(colNumber, out int colNo) || baseRowCells.Count <= colNo) { error = "Invalid base column number in formula"; return false; }
                string colValue = "(" + constMakeDouble + baseRowCells[colNo].value.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")"; // TODO - what kind of string to preserve best accuracy?
                formula = formula.Substring(0, pos) + colValue + formula.Substring(endpos+1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.BASE_COL, pos + colValue.Length);
            }

            // Handle all Reform Columns
            pos = formula.IndexOf(HardDefinitions.FormulaParameter.REF_COL);
            while (pos > -1)
            {
                int startpos = pos + HardDefinitions.FormulaParameter.REF_COL.Length;
                int endpos = formula.IndexOf(HardDefinitions.FormulaParameter.CLOSING_TOKEN, pos);
                if (startpos > formula.Length || endpos < startpos) { error = "Invalid reformAction column in formula"; return false; }
                string colNumber = formula.Substring(startpos, endpos - startpos);
                if (!int.TryParse(colNumber, out int colNo) || reformRowCells.Count <= colNo) { error = "Invalid reform column number in formula"; return false; }
                string colValue = "(" + constMakeDouble + reformRowCells[colNo].value.ToString(HardDefinitions.FormulaParameter.NumberFormat, CultureInfo.InvariantCulture) + ")"; // TODO - what kind of string to preserve best accuracy?
                formula = formula.Substring(0, pos) + colValue + formula.Substring(endpos+1);
                pos = formula.IndexOf(HardDefinitions.FormulaParameter.REF_COL, pos + colValue.Length);
            }

            return true;
        }

        /**
         * This function is used to create a display table to be displayed in the Statistics Presenter
         */
        internal bool CreateDisplayTable(out DisplayResults.DisplayPage.DisplayTable displayTable, Template.Page.Table table, int perReformNumber = -1)
        {
            displayTable = null;

            // Handle the case that a row does not describe a single row in the display table, but leads to generation of several rows
            // keep a copy of the original rows to set back at the end of function, in order to not destroy the original template, in case of further packages
            if (!HandleForEachRow(table.action.CalculationLevel, table, out List<Template.Page.Table.Row> origRows)) return false;

            // First prepare a display table that will be passed back to the Presenter, based on the template table
            // Copy all rows, columns, cells, and prepare filters and formulas
            displayTable = PrepareDisplayTable(table, perReformNumber);

            // The "cells" is a table of each actual cell to be displayed in the end
            displayTable.cells = new List<List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>>();

            // Build the actual display table
            for (int r = 0; r < table.rows.Count; r++)
            {
                // Create a new row of cells 
                List<DisplayResults.DisplayPage.DisplayTable.DisplayCell> rowCells = new List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>();
                List<DisplayResults.DisplayPage.DisplayTable.DisplayCell> baseRowCells = new List<DisplayResults.DisplayPage.DisplayTable.DisplayCell>();
                Dictionary<int, Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell>> reformRowCells = new Dictionary<int, Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell>>();

                if (table.columnGrouping == HardDefinitions.ColumnGrouping.SystemFirst)
                {
                    // Grouping Columns based on System

                    // For each baseline column
                    for (int c = 0; c < table.columns.Count; c++)
                    {
                        // create the cell of each baseline
                        for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                        {
                            // Get the right dataset for this baseline-reform set
                            DisplayResults.DisplayPage.DisplayTable.DisplayCell displayCell = CalculateCellValue(allData[dataNo], table, c, r, baseRowCells, null, 0, perReformNumber);
                            rowCells.Add(displayCell);
                            baseRowCells.Add(displayCell);

                            // followed by the cells of each reform column for each reform system // that ties with this baseline column
                            for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                            {
                                for (int rc = 0; rc < table.reformColumns.Count; rc++)
                                {
                                    if (table.reformColumns[rc].tiesWith != c) continue;
                                    if (!reformRowCells.ContainsKey(refNo)) reformRowCells.Add(refNo, new Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell>());
                                    DisplayResults.DisplayPage.DisplayTable.DisplayCell reformDisplayCell = CalculateCellValue(allData[dataNo], table, rc, r, baseRowCells, reformRowCells[refNo], refNo);
                                    rowCells.Add(reformDisplayCell);
                                    reformRowCells[refNo].Add(rc, reformDisplayCell);
                                }
                            }
                        }
                    }
                    for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                    {
                        for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                        {
                            // then do the extra reform columns 
                            for (int rc = 0; rc < table.reformColumns.Count; rc++)
                            {
                                if (!double.IsNaN(table.reformColumns[rc].tiesWith) && table.reformColumns[rc].tiesWith >= 0 && table.reformColumns[rc].tiesWith < table.columns.Count) continue;
                                if (!reformRowCells.ContainsKey(refNo)) reformRowCells.Add(refNo, new Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell>());
                                DisplayResults.DisplayPage.DisplayTable.DisplayCell reformDisplayCell = CalculateCellValue(allData[dataNo], table, rc, r, baseRowCells, reformRowCells[refNo], refNo);
                                rowCells.Add(reformDisplayCell);
                                reformRowCells[refNo].Add(rc, reformDisplayCell);
                            }
                        }
                    }
                }
                else if (table.columnGrouping == HardDefinitions.ColumnGrouping.ColumnFirst)
                {
                    // Grouping Columns based on Column

                    // For each baseline column
                    for (int c = 0; c < table.columns.Count; c++)
                    {
                        // create the cell of each baseline
                        for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                        {
                            DisplayResults.DisplayPage.DisplayTable.DisplayCell displayCell = CalculateCellValue(allData[dataNo], table, c, r, baseRowCells, null, 0, perReformNumber);
                            rowCells.Add(displayCell);
                            baseRowCells.Add(displayCell);

                            // followed by the cells of each reform column that ties with this baseline column
                            for (int rc = 0; rc < table.reformColumns.Count; rc++)
                            {
                                if (table.reformColumns[rc].tiesWith != c) continue; //if (table.reformColumns[rc].tiesWith != (c+1)) continue;
                                // for each reform system
                                for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                                {
                                    if (!reformRowCells.ContainsKey(refNo)) reformRowCells.Add(refNo, new Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell>());
                                    DisplayResults.DisplayPage.DisplayTable.DisplayCell reformDisplayCell = CalculateCellValue(allData[dataNo], table, rc, r, baseRowCells, reformRowCells[refNo], refNo);
                                    rowCells.Add(reformDisplayCell);
                                    reformRowCells[refNo].Add(rc, reformDisplayCell);
                                }
                            }
                        }
                    }
                    // then do the extra reform columns 
                    for (int rc = 0; rc < table.reformColumns.Count; rc++)
                    {
                        if (!double.IsNaN(table.reformColumns[rc].tiesWith) && table.reformColumns[rc].tiesWith >= 0) continue; //if (!double.IsNaN(table.reformColumns[rc].tiesWith) && table.reformColumns[rc].tiesWith > 0) continue; // handled above
                        for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                        {
                            for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                            {
                                if (!reformRowCells.ContainsKey(refNo)) reformRowCells.Add(refNo, new Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell>());
                                DisplayResults.DisplayPage.DisplayTable.DisplayCell reformDisplayCell = CalculateCellValue(allData[dataNo], table, rc, r, baseRowCells, reformRowCells[refNo], refNo);
                                rowCells.Add(reformDisplayCell);
                                reformRowCells[refNo].Add(rc, reformDisplayCell);
                            }
                        }
                    }
                }

                // add the new row to the display table
                displayTable.cells.Add(rowCells);
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

                if (!HandleForEachRow_ErrorCheck(calculationLevel, origRow)) return false;

                // row will be replaced by as many rows as necessary, each with an appropriate filter:
                // ForEachDataRow: as many rows as data-rows (taking original row-filter into account) with filter: pid=x (or hhid=x)
                // ForEachValueOf: as many rows as there are distinct values of this variable with filter: var=oneval (e.g. decile=1 ... decile=10)
                foreach (Template.Filter filter in origRow.forEachDataRow
                                         ? HandleForEachRow_GetFiltersDataRow(calculationLevel, origRow)
                                         : HandleForEachRow_GetFiltersValueOf(calculationLevel, origRow, origRow.forEachValueOf))
                {
                    // note that HandleForEachRow_GetFiltersValueOf stores the new name for the rows temporarily in filter.name
                    string rowName = !string.IsNullOrEmpty(filter.name) ? filter.name : origRow.name; filter.name = string.Empty;
                    adaptedRows.Add(new Template.Page.Table.Row()
                    {
                        name = rowName,
                        action = new Template.Action() { filter = filter, outputVar = "", formulaString = "", parameters = new List<Template.Parameter>() },
                        isVisible = origRow.isVisible, stringFormat = origRow.stringFormat, strong = origRow.strong, tooltip = origRow.tooltip,
                        foregroundColour = origRow.foregroundColour, backgroundColour = origRow.backgroundColour
                    });
                }
            }
            table.rows = adaptedRows;
            return true;
        }

        private List<Template.Filter> HandleForEachRow_GetFiltersDataRow(string calculationLevel, Template.Page.Table.Row origRow)
        {
            DataStatsHolder data = allData[0][calculationLevel]; // data 0 is ok as the filter refers to pid or hhid, which need to be equal in base/reform (and multi is not allowed for this option)
            Func<List<double>, bool> func = null; // first apply the original filter on the data
            if (origRow.action != null && origRow.action.filter != null)
            {
                PrepareFilter(data, origRow.action.filter, template);
                func = origRow.action.filter.GetFunc(data);
            }

            List<Template.Filter> listFilters = new List<Template.Filter>();
            data.GetData(func).ForEach(x => listFilters.Add(new Template.Filter()
            {
                formulaString = string.Format("OBS_VAR[@key_id] == {0}", x[data.GetKeyIndex()]),
                parameters = new List<Template.Parameter>() { new Template.Parameter() { name = "key_id", variableName = data.GetKeyName() } }
            }));
            return listFilters;
        }

        private List<Template.Filter> HandleForEachRow_GetFiltersValueOf(string calculationLevel, Template.Page.Table.Row origRow, string valofVar)
        {
            List<double> valofValues = new List<double>();
            for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++) // for multi, to be precise, one needs to assess the values from all datasets
            {
                DataStatsHolder data = allData[dataNo][calculationLevel];
                Func<List<double>, bool> func = null; // only take the values which are covered when applying the original filter
                //List<double> vals = data.GetData(null).Select(x => x[36]).ToList();
                if (origRow.action != null && origRow.action.filter != null)
                {
                    PrepareFilter(data, origRow.action.filter, template);
                    func = origRow.action.filter.GetFunc(data);
                }
                foreach (double v in data.GetData(func).Select(x => x[data.GetVarIndex(valofVar)]).Distinct())
                    if (!valofValues.Contains(v)) valofValues.Add(v);
            }

            valofValues.Sort();
            List<Template.Filter> listFilters = new List<Template.Filter>();
            // the row-name in template could be e.g. "Decile [value]" or "[value]. Decile"
            string rowName = origRow.name.Contains("[value]") ? origRow.name : origRow.name + " [value]";
            foreach (double valofValue in valofValues)
            {
                listFilters.Add(new Template.Filter()
                {
                    formulaString = string.Format("OBS_VAR[@valof_var] == {0}", valofValue),
                    name = ReplaceValuePlaceholder(rowName, valofValue), // (mis)use the name to transfer the new name of the row
                    parameters = new List<Template.Parameter>()
                    { // one could consider making _reform a user-option
                        new Template.Parameter() { name = "valof_var", variableName = valofVar, _reform = false }
                    }
                });   
            }
            return listFilters;

            // the placeholder [value] is usually replaced by the respective value, but the template may contain a UserVariable with
            // type ForEachValueDescription, containing a Dictionary(value, description)
            // this can obviously only be used by a programme (see HHot)
            string ReplaceValuePlaceholder(string name, double val)
            {
                string replacement = val.ToString(CultureInfo.InvariantCulture);
                var desc = from uv in template.info.userVariables where uv.inputType == HardDefinitions.UserInputType.ForEachValueDescription select uv;
                if (desc.Any() && desc.First().forEachValueDescription != null && desc.First().forEachValueDescription.ContainsKey(val.ToString(CultureInfo.InvariantCulture)))
                    replacement = desc.First().forEachValueDescription[val.ToString(CultureInfo.InvariantCulture)];
                return name.Replace("[value]", replacement);
            }
        }

        private bool HandleForEachRow_ErrorCheck(string calculationLevel, Template.Page.Table.Row origRow)
        {
            string error = string.Empty;
            if (origRow.forEachDataRow && !string.IsNullOrEmpty(origRow.forEachValueOf))
                error += "ForEachDataRow=true cannot be combined with ForEachValueOf(variable)";
            if (origRow.forEachDataRow && template.info.templateType == HardDefinitions.TemplateType.Multi)
                error += "ForEachDataRow=true is not possible with TemplateType=Multi (as number of rows is likely to be different for scenarios)";
            if (!string.IsNullOrEmpty(origRow.forEachValueOf))
                for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                    if (allData[dataNo][calculationLevel].GetVarIndex(origRow.forEachValueOf) < 0)
                        error += "ForEachValueOf: variable " + origRow.forEachValueOf + " not found";
            if (origRow.action != null)
            {
                if (!string.IsNullOrEmpty(origRow.action.formulaString) || origRow.action.calculationType != HardDefinitions.CalculationType.NA)
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
                                           Dictionary<string, DataStatsHolder> data, Template.Page.Table table, int c, int r,
                                           List<DisplayResults.DisplayPage.DisplayTable.DisplayCell> baseRowCells,
                                           Dictionary<int, DisplayResults.DisplayPage.DisplayTable.DisplayCell> reformRowCells, int refNo,
                                           int sdcRefNoForBaseCellOfPagePerReform = -1) // this is only required for SDC (for the secondary SDC groups, see CalculateSDCProperties)
        {
            // Create a new display cell
            DisplayResults.DisplayPage.DisplayTable.DisplayCell displayCell = new DisplayResults.DisplayPage.DisplayTable.DisplayCell();

            // Get the corresponding row & column, and create a new cell
            Template.Page.Table.Row row = table.rows[r];
            Template.Page.Table.Column column = refNo > 0 ? table.reformColumns[c] : table.columns[c];
            Template.Page.Table.Cell cell = null;

            // Start the new cell based on the corresponding custom cell, if one exists
            List<Template.Page.Table.Cell> customCells = refNo > 0 ? table.reformCells : table.cells;
            if (customCells.Any(x => x.rowNum == r && x.colNum == c))
            {
                cell = customCells.FirstOrDefault(x => x.rowNum == r && x.colNum == c);
                displayCell = new DisplayResults.DisplayPage.DisplayTable.DisplayCell()
                {
                    stringFormat = cell.stringFormat,
                    tooltip = cell.tooltip,
                    strong = cell.strong ?? false,
                    foregroundColour = cell.foregroundColour,
                    backgroundColour = cell.backgroundColour
                };
            }

            // Merge the properties of the corresponding table, column, row & custom cell into a single cell, considering reforms
            if (cell == null || cell.action == null || cell.action.calculationType != HardDefinitions.CalculationType.Empty)
                cell = CalculateProperties(cell, column, row, table, refNo, sdcRefNoForBaseCellOfPagePerReform);

            if (cell.action.calculationType == HardDefinitions.CalculationType.Empty)
            {
                displayCell.displayValue = string.Empty;
                displayCell.isStringValue = true;
            }
            else if (cell.action.calculationType == HardDefinitions.CalculationType.Info)
            {
                int dataNo = template.info.templateType == HardDefinitions.TemplateType.Multi ? allData.IndexOf(data) : 0;
                int captionRefNo = refNo - 1; // GetCaptionText requires 0-based reference-number
                displayCell.displayValue = PrettyInfoProvider.GetPrettyText(template.info, cell.action.formulaString,
                                           baselineSystemInfos[dataNo], reformSystemInfos, packageKey, captionRefNo);
                displayCell.isStringValue = true;
            }
            else
            {
                // Given the actual double value and the merged String Format, calculate the display value
                if (double.IsNaN(displayCell.value)) displayCell.displayValue = "NaN";
                else
                {
                    displayCell.value = HandleAction(data, cell.action, cell.sdcDefinition, out displayCell.sdcObsNo, baseRowCells, reformRowCells);

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

        private DisplayResults.DisplayPage PrepareDisplayPage(Template.Page page, int refNo = -1)
        {
            // Create a new DisplayTemplate.DisplayPage and copy the main attributes
            if (refNo != -1) --refNo; // GetCaptionText requires 0-based reference-number
            DisplayResults.DisplayPage displayPage = new DisplayResults.DisplayPage()
            {
                name = page.name,
                title = PrettyInfoProvider.GetPrettyText(template.info, page.title, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo),
                subtitle = PrettyInfoProvider.GetPrettyText(template.info, page.subtitle, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo),
                button = PrettyInfoProvider.GetPrettyText(template.info, page.button, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo),
                description = page.description,
                visible = page.visible,
                displayTables = new List<DisplayResults.DisplayPage.DisplayTable>()
            };
            return displayPage;
        }

        /**
         * This function will create a base DisplayTable based on a template table
         */
        private DisplayResults.DisplayPage.DisplayTable PrepareDisplayTable(Template.Page.Table table, int perReformNumber = -1)
        {
            // Create a new DisplayTable, copy the title, create new column and row lists
            int captionRefNo = perReformNumber == -1 ? -1 : perReformNumber - 1; // GetCaptionText requires 0-based reference-number
            DisplayResults.DisplayPage.DisplayTable displayTable = new DisplayResults.DisplayPage.DisplayTable()
            {
                name = table.name,
                title = PrettyInfoProvider.GetPrettyText(template.info, table.title, baselineSystemInfos[0], reformSystemInfos, packageKey, captionRefNo),
                subtitle = PrettyInfoProvider.GetPrettyText(template.info, table.subtitle, baselineSystemInfos[0], reformSystemInfos, packageKey, captionRefNo),
                //button = SystemInfo.GetCaptionText(template.info, table.button, baselineSystem[0], reformSystems), // table does not have a button
                description = table.description,
                visible = table.visible,
                stringFormat = table.stringFormat,
                columns = new List<DisplayResults.DisplayPage.DisplayTable.DisplayColumn>(),
                rows = new List<DisplayResults.DisplayPage.DisplayTable.DisplayRow>(),
                graph = table.graph
            };

            Template.Page.Table.Column column;
            DisplayResults.DisplayPage.DisplayTable.DisplayColumn displayColumn;

            // Read & prepare all Columns
            if (table.columnGrouping == HardDefinitions.ColumnGrouping.SystemFirst)
            {
                // Grouping Columns based on System


                // For each baseline column
                for (int c = 0; c < table.columns.Count; c++)
                {
                    // create the cell of each baseline
                    for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                    {
                        column = table.columns[c];
                        displayColumn = new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
                        {
                            title = PrettyInfoProvider.GetPrettyText(template.info, column.name, baselineSystemInfos[dataNo], null, packageKey, captionRefNo),
                            stringFormat = column.stringFormat,
                            tooltip = column.tooltip,
                            hasSeparatorAfter = column.hasSeparatorAfter,
                            hasSeparatorBefore = column.hasSeparatorBefore,
                            strong = column.strong ?? false,
                            foregroundColour = column.foregroundColour,
                            backgroundColour = column.backgroundColour
                        };
                        displayTable.columns.Add(displayColumn);

                        for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                        {
                            // followed by the cells of each reform column that ties with this baseline column
                            for (int rc = 0; rc < table.reformColumns.Count; rc++)
                            {
                                if (table.reformColumns[rc].tiesWith != c) continue; //if (table.reformColumns[rc].tiesWith != (c + 1)) continue;
                                // for each reform system
                                column = table.reformColumns[rc];
                                displayColumn = new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
                                {
                                    title = PrettyInfoProvider.GetPrettyText(template.info, column.name, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo - 1),
                                    stringFormat = column.stringFormat,
                                    tooltip = column.tooltip,
                                    hasSeparatorAfter = column.hasSeparatorAfter,
                                    hasSeparatorBefore = column.hasSeparatorBefore,
                                    strong = column.strong ?? false,
                                    foregroundColour = column.foregroundColour,
                                    backgroundColour = column.backgroundColour
                                };
                                displayTable.columns.Add(displayColumn);
                            }
                        }
                    }
                }
                // then do the extra reform columns 
                for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                {
                    // Get the right dataset for this baseline-reform set
                    for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                    {
                        for (int rc = 0; rc < table.reformColumns.Count; rc++)
                        {
                            if (!double.IsNaN(table.reformColumns[rc].tiesWith) && table.reformColumns[rc].tiesWith >= 0 && table.reformColumns[rc].tiesWith < table.columns.Count) continue;
                            column = table.reformColumns[rc];
                            displayColumn = new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
                            {
                                title = PrettyInfoProvider.GetPrettyText(template.info, column.name, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo - 1),
                                stringFormat = column.stringFormat,
                                tooltip = column.tooltip,
                                hasSeparatorAfter = column.hasSeparatorAfter,
                                hasSeparatorBefore = column.hasSeparatorBefore,
                                strong = column.strong ?? false,
                                foregroundColour = column.foregroundColour,
                                backgroundColour = column.backgroundColour
                            };
                            displayTable.columns.Add(displayColumn);
                        }
                    }
                }
            }
            else if (table.columnGrouping == HardDefinitions.ColumnGrouping.ColumnFirst)
            {
                // Grouping Columns based on Column

                // For each baseline column
                for (int c = 0; c < table.columns.Count; c++)
                {
                    // create the cell of each baseline
                    for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                    {
                        column = table.columns[c];
                        displayColumn = new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
                        {
                            title = PrettyInfoProvider.GetPrettyText(template.info, column.name, baselineSystemInfos[dataNo], null, packageKey, captionRefNo),
                            stringFormat = column.stringFormat,
                            tooltip = column.tooltip,
                            hasSeparatorAfter = column.hasSeparatorAfter,
                            hasSeparatorBefore = column.hasSeparatorBefore,
                            strong = column.strong ?? false,
                            foregroundColour = column.foregroundColour,
                            backgroundColour = column.backgroundColour
                        };
                        displayTable.columns.Add(displayColumn);

                        // followed by the cells of each reform column that ties with this baseline column
                        for (int rc = 0; rc < table.reformColumns.Count; rc++)
                        {
                            if (table.reformColumns[rc].tiesWith != c) continue; //if (table.reformColumns[rc].tiesWith != (c + 1)) continue;
                            // for each reform system
                            for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                            {
                                column = table.reformColumns[rc];
                                displayColumn = new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
                                {
                                    title = PrettyInfoProvider.GetPrettyText(template.info, column.name, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo - 1),
                                    stringFormat = column.stringFormat,
                                    tooltip = column.tooltip,
                                    hasSeparatorAfter = column.hasSeparatorAfter,
                                    hasSeparatorBefore = column.hasSeparatorBefore,
                                    strong = column.strong ?? false,
                                    foregroundColour = column.foregroundColour,
                                    backgroundColour = column.backgroundColour
                                };
                                displayTable.columns.Add(displayColumn);
                            }
                        }
                    }
                }
                // then do the extra reform columns 
                for (int rc = 0; rc < table.reformColumns.Count; rc++)
                {
                    if (!double.IsNaN(table.reformColumns[rc].tiesWith) && table.reformColumns[rc].tiesWith >= 0) continue; //if (!double.IsNaN(table.reformColumns[rc].tiesWith) && table.reformColumns[rc].tiesWith > 0) continue; // handled above
                    for (int dataNo = 0; dataNo < DatasetNumber(); dataNo++)
                    {
                        // Get the right dataset for this baseline-reform set
                        for (int refNo = perReformNumber == -1 ? 1 : perReformNumber; refNo <= (perReformNumber == -1 ? ReformNumber() : perReformNumber); refNo++)
                        {
                            column = table.reformColumns[rc];
                            displayColumn = new DisplayResults.DisplayPage.DisplayTable.DisplayColumn()
                            {
                                title = PrettyInfoProvider.GetPrettyText(template.info, column.name, baselineSystemInfos[0], reformSystemInfos, packageKey, refNo - 1),
                                stringFormat = column.stringFormat,
                                tooltip = column.tooltip,
                                hasSeparatorAfter = column.hasSeparatorAfter,
                                hasSeparatorBefore = column.hasSeparatorBefore,
                                strong = column.strong ?? false,
                                foregroundColour = column.foregroundColour,
                                backgroundColour = column.backgroundColour
                            };
                            displayTable.columns.Add(displayColumn);
                        }
                    }
                }
            }

            // Read & prepare all Rows
            for (int r = 0; r < table.rows.Count; r++)
            {
                Template.Page.Table.Row row = table.rows[r];
                DisplayResults.DisplayPage.DisplayTable.DisplayRow displayRow = new DisplayResults.DisplayPage.DisplayTable.DisplayRow()
                { 
                    title = row.name, 
                    stringFormat = row.stringFormat,
                    tooltip = row.tooltip,
                    hasSeparatorAfter = row.hasSeparatorAfter,
                    hasSeparatorBefore = row.hasSeparatorBefore,
                    strong = row.strong ?? false,
                    foregroundColour = row.foregroundColour,
                    backgroundColour = row.backgroundColour
                };
                displayTable.rows.Add(displayRow);
            }

            // Custom cell display properties need to be handled at the point of cell calculation!

            // make sure that there are no double-borders in the beginning or end of the table
            displayTable.columns[0].hasSeparatorBefore = false;
            displayTable.columns[displayTable.columns.Count - 1].hasSeparatorAfter = false;

            return displayTable;
        }

        /**
         * This function will accept a table, row, column and custom cell, and merge their properties into a single template cell
         */
        private Template.Page.Table.Cell CalculateProperties(Template.Page.Table.Cell cell,
                                                             Template.Page.Table.Column column, Template.Page.Table.Row row, Template.Page.Table table,
                                                             int refNo, int sdcRefNoForBaseCellOfPagePerReform)
        {
            // Create the new cell
            Template.Page.Table.Cell newCell = new Template.Page.Table.Cell() { action = new Template.Action() { parameters = new List<Template.Parameter>() } };

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

            // Decide on the string format to be used
            if (cell != null && cell.stringFormat != null)
                newCell.stringFormat = cell.stringFormat;
            else if (row.stringFormat != null)
                newCell.stringFormat = row.stringFormat;
            else if (column.stringFormat != null)
                newCell.stringFormat = column.stringFormat;
            else if (table.stringFormat != null)
                newCell.stringFormat = table.stringFormat;

            if (cell != null && cell.action != null) BlendAction(newCell.action, cell.action);
            if (newCell.action.BlendParameters && newCell.action.calculationType != HardDefinitions.CalculationType.Empty && row.action != null) BlendAction(newCell.action, row.action);
            if (newCell.action.BlendParameters && newCell.action.calculationType != HardDefinitions.CalculationType.Empty && column.action != null) BlendAction(newCell.action, column.action);
            if (newCell.action.BlendParameters && newCell.action.calculationType != HardDefinitions.CalculationType.Empty && table.action != null) BlendAction(newCell.action, table.action);
            
            // Merge the properties for SDC
            CalculateSDCProperties(newCell, cell, column, row, table, refNo, sdcRefNoForBaseCellOfPagePerReform);

            // Reform the variable names where appropriate
            if (refNo > 0)
            {
                foreach (Template.Parameter par in newCell.action.parameters)
                    if (par.Reform) par.variableName += HardDefinitions.Reform + refNo;
                if (newCell.action.filter != null)
                    foreach (Template.Parameter par in newCell.action.filter.parameters)
                        if (par.Reform) par.variableName += HardDefinitions.Reform + refNo;
            }
            return newCell;
        }

        private void CalculateSDCProperties(Template.Page.Table.Cell newCell, Template.Page.Table.Cell cell,
                                            Template.Page.Table.Column column, Template.Page.Table.Row row, Template.Page.Table table,
                                            int refNo, int sdcRefNoForBaseCellOfPagePerReform)
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
                    if (refNo > 0) secondarySdcGroupName += HardDefinitions.Reform + refNo;
                    // base cells within a per-reform-page must also be distinguished from (analog) base cells of another reform: thus add ~base~X
                    // note: this approach is a problem if the group contains cells outside the page, but as this is not overly likely ...
                    if (sdcRefNoForBaseCellOfPagePerReform != -1) secondarySdcGroupName += HardDefinitions.Base + sdcRefNoForBaseCellOfPagePerReform;
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
                if (newAction.filter == null) newAction.filter = new Template.Filter();
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
                        if (par._reform == null) par._reform = oldPar._reform;
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

        /**
         * This function creates a duplicate of a Filter for given reform
         */
        private static Template.Filter ReformFilter(Template.Filter filter, int refNo)
        {
            // not implemented yet
            Template.Filter newFilter = new Template.Filter(filter, refNo);
            return newFilter;

        }

        /**
         * This function creates a duplicate of an Action for given reform
         */
        private static Template.Action ReformAction(Template.Action action, int refNo)
        {
            Template.Action newAction = new Template.Action(action, refNo);
            return newAction;
        }

        private bool CompileTemplate()
        {
            // then prepare the display pages
            foreach (Template.Page page in template.pages)
            {
                if (template.info.templateType == HardDefinitions.TemplateType.BaselineReform)
                {
                    List<Template.Action> newActions = new List<Template.Action>();
                    foreach (Template.Action action in page.actions)
                    {
                        if (!action.Reform) continue;
                        for (int c = 1; c <= ReformNumber(); c++)
                        {
                            Template.Action newAction = ReformAction(action, c);
                            newActions.Add(newAction);
                        }
                    }
                    page.actions.AddRange(newActions);
                }

                foreach (Template.Action action in page.actions)
                    HandleGlobalAction(action);

                for (int refNo = page.perReform ? 1 : -1; refNo <= (page.perReform ? ReformNumber() : -1); refNo++)
                {
                    DisplayResults.DisplayPage displayPage = PrepareDisplayPage(page, refNo);
                    foreach (Template.Page.Table table in page.tables)
                    {
                        if (CreateDisplayTable(out DisplayResults.DisplayPage.DisplayTable displayTable, table, refNo))
                            displayPage.displayTables.Add(displayTable);
                    }
                    displayResults.displayPages.Add(displayPage);
                }
            }

            // check for secondary SDC-cells, i.e. cells that need to be hidden to prevent deducting primary SDC-cells (cells which are hidden because they are based on too few observations)
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
            return displayResults;
        }

        /// <summary>
        /// This function returns all the compiled display pages.
        /// </summary>
        /// <returns>Returns a list of DisplayTemplate.DisplayPage elements.</returns>
        public List<DisplayResults.DisplayPage> GetDisplayPages()
        {
            return displayResults.displayPages;
        }

        /// <summary>
        /// This function returns a specific compiled display page based on its key.
        /// </summary>
        /// <returns>Returns a list of DisplayTemplate.DisplayPage elements.</returns>
        public DisplayResults.DisplayPage GetDisplayPage(string key)
        {
            return displayResults.displayPages.FirstOrDefault(x => x.key == key);
        }

        private int ReformNumber()
        {
            return reformSystemInfos.Count;
        }

        private int DatasetNumber()
        {
            return allData.Count;
        }
    }
}
