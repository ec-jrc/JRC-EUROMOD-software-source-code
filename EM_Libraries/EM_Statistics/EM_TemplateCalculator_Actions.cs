using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    public partial class EM_TemplateCalculator
    {
        public const string PAR_WEIGHT = "WeightVar";
        public const string PAR_GROUPING = "GroupingVar";
        public const string PAR_DECILE = "DecileVar";
        public const string PAR_GINI = "GiniVar";
        public const string PAR_CONCENTRATION_INDEX_SORT_VAR = "ConcentrationIndexSortVar";
        public const string PAR_S8020 = "S8020Var";
        public const string PAR_S8020_STOP = "STop";
        public const string PAR_S8020_SBOTTOM = "SBottom";
        public const string PAR_INCOME = "IncomeVar";
        public const string PAR_POVERTYLINE = "PovertyLine";
        public const string PAR_USE_SWITCH_APPROACH = "UseSwitchApproach";
        public const string PAR_DECNO = "DecNo";
        public const string PAR_EQUALDECILES = "EqualDeciles";
        public const string PAR_OXFORD = "Oxford";
        public const string PAR_RECODENEGATIVES = "RecodeNegatives";
        public const string PAR_EQUIVALENCESCALE = "EquivalenceScale";
        public const string PAR_SUMVAR = "SumVar";
        public const string PAR_COPY_TO_INDIVIDUALS = "CopyToIndividuals";
        public const string PAR_COPY_TO_GROUP = "CopyToGroup";
        public const string PAR_GIVETOTALS = "GiveTotals";
        public const string PAR_THRESHOLD = "Threshold";
        public const string PAR_EQUIVSCALEBAND = "EquivalenceScaleBand";
        public const string PAR_EQUIVFIRSTPERSON = "EquivalenceFirstPerson";
        public const string PAR_FORCE_RECALCULATION = "ForceRecalculation";
        public const string PAR_MONETARY = "Monetary";
        public const string PAR_MESSAGE_SWITCH_VAR = "MessageSwitchVar";
        public const string PAR_MESSAGE_IF_NON_ZERO = "MessageIfNonZero";
        public const string PAR_MESSAGE_IF_ZERO = "MessageIfZero";
        public const string PAR_MESSAGE_IF_VAL = "MessageIfVal";
        public const string PAR_MESSAGE_RANGE_MIN_X = "RangeMin_";
        public const string PAR_MESSAGE_RANGE_MAX_X = "RangeMax_";
        public const string PAR_MESSAGE_RANGE_X = "RangeMessage";
        public const string PAR_MESSAGE_DEFAULT = "DefaultMessage";
        public const string PAR_ATKINSON_INEQUALITY_AVERSION = "InequalityAversion";

        private bool CreateArithmeticColumn(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");
            if (data[action.CalculationLevel].HasVariable(action.outputVar, action.localMap)) return true;

            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);
            bool monetary = true; if (action.HasParameter(PAR_MONETARY)) GetBoolParameterValue(PAR_MONETARY, action, out monetary);

            data[action.CalculationLevel].AddVar(action.outputVar, monetary, action.localMap);
            if (action.filter == null)
                data[action.CalculationLevel].GetData().ForEach(x => x.Add(recodeNegatives ? Math.Max(action.func(x), 0) : action.func(x)));
            else
                data[action.CalculationLevel].GetData().ForEach(x => x.Add(recodeNegatives ? Math.Max((action.filter.GetFunc(data[action.CalculationLevel])(x) ? action.func(x) : 0), 0) : (action.filter.GetFunc(data[action.CalculationLevel])(x) ? action.func(x) : 0)));
            return true;
        }

        private double CalculateArithmeticAction(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            if (action.SaveResult && data[action.CalculationLevel].HasVariable(action.outputVar, action.localMap))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar, action.localMap);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }
            List<double> array = data[action.CalculationLevel].GetData().FirstOrDefault();  // for CreateArithmeticAction, if DATA_VAR is used, the values of the first individual of the sample are used. 
            double result = action.func(array);
            sdcObsNo = data[action.CalculationLevel].GetData().Count();
            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, result, sdcObsNo));
            return result;
        }

        private bool CreateFlag(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");

            if (data[action.CalculationLevel].HasVariable(action.outputVar, action.localMap)) return true;

            bool monetary = true; if (action.HasParameter(PAR_MONETARY)) GetBoolParameterValue(PAR_MONETARY, action, out monetary);
            data[action.CalculationLevel].AddVar(action.outputVar, monetary, action.localMap);
            data[action.CalculationLevel].GetData().ForEach(x => x.Add(action.filter.GetFunc(data[action.CalculationLevel])(x) ? 1 : 0));
            return true;
        }

        // this is only supposed to be called as a helping function from within the library
        private static double Sum(Dictionary<string, DataStatsHolder> data, string level, string var, LocalMap localMap,
                                  Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo,
                                  Func<List<double>, bool> filter = null)
        {
            int varIndex = data[level].GetVarIndex(var, localMap);
            var filteredData = data[level].GetData(filter);
            double sum = filteredData.Sum(x => x[varIndex]);
            sdcObsNo = CalculateSdcObsNo(sdcDefinition, varIndex, data[level].GetData(), filteredData);
            return sum;
        }

        private static int CalculateSdcObsNo(Template.Page.Table.SDCDefinition sdcDefinition, int varIndex,
                                             List<List<double>> allData, List<List<double>> filteredData, bool recodeNegatives = false)
        {
            List<List<double>> obsData = sdcDefinition.ignoreActionFilter == true ? allData : filteredData;
            return sdcDefinition.countNonZeroObsOnly == true
                 ? obsData.Where(x => recodeNegatives ? x[varIndex] > 0 : x[varIndex] != 0).Count()
                 : obsData.Count();
        }

        private double CalculateSumWeighted(Dictionary<string, DataStatsHolder> data, Template.Action action, int varIndex, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);

            List<List<double>> rawdata = data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel]));

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, varIndex, data[action.CalculationLevel].GetData(), rawdata, recodeNegatives);
            return rawdata.Sum(x => (recodeNegatives ? Math.Max(x[varIndex], 0) : x[varIndex]) * x[wgtVarIndex]);
        }

        private double CalculateWeightedAverage(Dictionary<string, DataStatsHolder> data, Template.Action action, int varIndex, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);

            List<List<double>> rawdata = data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel]));

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, varIndex, data[action.CalculationLevel].GetData(), rawdata, recodeNegatives);

            double totalCount = rawdata.Sum(x => (recodeNegatives ? Math.Max(x[varIndex], 0) : x[varIndex]) * x[wgtVarIndex]);
            double totalWeight = rawdata.Sum(x => x[wgtVarIndex]);
            return totalCount / totalWeight;
        }

        private double CalculateGini(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;

            if (data[action.CalculationLevel].HasSavedNumber(action.outputVar, action.localMap))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar, action.localMap);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }

            double gini = 0;
            if (!GetVarParameterInfo(PAR_GINI, data, action, out int giniVarIndex, out string giniVarName) |
                !GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;

            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);
            GetVarParameterInfo(PAR_CONCENTRATION_INDEX_SORT_VAR, data, action, out int sortVarIndex, out string sortVarName, giniVarName);

            string wtIncomeVar = giniVarName + HardDefinitions.Separator + wgtVarName;

            Template.Action makeWtIncomeAction = new Template.Action {
                calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                formulaString = string.Format("1.0 * DATA_VAR[{0}] * DATA_VAR[{1}]", giniVarIndex, wgtVarIndex),
                parameters = recodeNegatives?new List<Template.Parameter>() { new Template.Parameter() { name = "RecodeNegatives", boolValue = true } }: new List<Template.Parameter>(),
                outputVar = wtIncomeVar
            };

            if (double.IsNaN(HandleAction(data, makeWtIncomeAction, sdcDefinition, out int obsno))) return double.NaN;

            double totalGini = Sum(data, action.CalculationLevel, wtIncomeVar, action.localMap, sdcDefinition, out sdcObsNo);
            if (totalGini == 0) return ActionError(action, $"Gini cannot be calculated, because sum of (weighted) {giniVarName} is zero");
            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, action.localMap, sdcDefinition, out _);
            if (totalPeople == 0) return ActionError(action, $"Gini cannot be calculated, because population is zero");

            int wtGiniVarIndex = data[action.CalculationLevel].GetVarIndex(wtIncomeVar, action.localMap);
            double cumShareVariableRankWeighted = 0, cumSharePersonsWeighted = 0;

            if (NoGrouping(action))
            {
                var all = data[action.CalculationLevel].GetData().OrderBy(x => x[sortVarIndex]);
                foreach (List<double> person in all)
                {
                    cumShareVariableRankWeighted += (recodeNegatives ? Math.Max(person[wtGiniVarIndex], 0) : person[wtGiniVarIndex]) / totalGini;
                    cumSharePersonsWeighted += person[wgtVarIndex] / totalPeople;
                    gini += (person[wgtVarIndex] / totalPeople) * (cumSharePersonsWeighted - cumShareVariableRankWeighted) * 2;
                }
            }
            else
            {
                if (!GetGroupingVar(out int grpVarIndex, out string grpVarName, data, action)) return double.NaN;

                var all = data[action.CalculationLevel].GetData().OrderBy(x => x[sortVarIndex]);
                var allGroups = from person in all
                                group person by person[grpVarIndex] into Group
                                select Group;
                foreach (var group in allGroups)
                {
                    double hhVariableRankWeighted = 0, hhPersonsWeighted = 0;
                    foreach (List<double> member in group)
                    {
                        hhVariableRankWeighted += member[wtGiniVarIndex];
                        hhPersonsWeighted += member[wgtVarIndex];
                    }
                    if (recodeNegatives && hhVariableRankWeighted <= 0) hhVariableRankWeighted = 0;
                    cumShareVariableRankWeighted += hhVariableRankWeighted / totalGini;
                    cumSharePersonsWeighted += hhPersonsWeighted / totalPeople;
                    gini += (hhPersonsWeighted / totalPeople) *
                                    (cumSharePersonsWeighted - cumShareVariableRankWeighted) * 2;
                }
            }

            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, gini, sdcObsNo));
            return gini;
        }

        private double CalculateS8020(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;

            if (data[action.CalculationLevel].HasSavedNumber(action.outputVar, action.localMap))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar, action.localMap);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }

            if (!GetVarParameterInfo(PAR_DECILE, data, action, out int decileIndex, out _) |
                !GetVarParameterInfo(PAR_S8020, data, action, out int incomeVarIndex, out string incomeVarName)) return double.NaN;

            GetDoubleParameterValue(PAR_S8020_STOP, action, out double quantTop); if (quantTop == 0) quantTop = 80;
            if (!QuantToDec(quantTop, out double decTop, out string tError)) return ActionError(action, tError);
            GetDoubleParameterValue(PAR_S8020_SBOTTOM, action, out double quantBottom); if (quantBottom == 0) quantBottom = 20;
            if (!QuantToDec(quantBottom, out double decBottom, out string bError)) return ActionError(action, bError);

            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);

            Template.Action actionDBottom = new Template.Action(action.localMap)
            {
                calculationType = HardDefinitions.CalculationType.CalculateSumWeighted,
                _calculationLevel = action._calculationLevel,
                filter = new Template.Filter(data[action.CalculationLevel], x => x[decileIndex] <= decBottom, action.localMap),
                formulaString = string.Format("1.0 * DATA_VAR[{0}]", incomeVarIndex),
                parameters = recodeNegatives ? new List<Template.Parameter>() { new Template.Parameter() { name = "RecodeNegatives", boolValue = true } } : new List<Template.Parameter>()
            };
            Template.Action actionDTop = new Template.Action(action.localMap)
            {
                calculationType = HardDefinitions.CalculationType.CalculateSumWeighted,
                _calculationLevel = action._calculationLevel,
                filter = new Template.Filter(data[action.CalculationLevel], x => x[decileIndex] > decTop, action.localMap),
                formulaString = string.Format("1.0 * DATA_VAR[{0}]", incomeVarIndex),
                parameters = recodeNegatives ? new List<Template.Parameter>() { new Template.Parameter() { name = "RecodeNegatives", boolValue = true } } : new List<Template.Parameter>()
            };

            double sTop = HandleAction(data, actionDTop, sdcDefinition, out int _); if (double.IsNaN(sTop)) return double.NaN;
            double sBottom = HandleAction(data, actionDBottom, sdcDefinition, out int _); if (double.IsNaN(sBottom)) return double.NaN;

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, incomeVarIndex,
                data[action.CalculationLevel].GetData(), data[action.CalculationLevel].GetData()); // S80/20 does not yet(!) allow for a filter, thus this is always the same data

            if (sBottom == 0) return ActionError(action, $"Index cannot be calculated, because sum of (weighted) {incomeVarName} in lowest quintile is zero");

            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, sTop / sBottom, sdcObsNo));
            return sTop / sBottom;

            bool QuantToDec(double quant, out double dec, out string error)
            {
                dec = -1; error = string.Empty; List<double> list = new List<double>() { 10, 20, 30, 40, 50, 60, 70, 80, 90 };
                if (list.Contains(quant)) { dec = quant / 10; return true; }
                error = $"Invalid quantile {quant}. Must be one of {string.Join(",", list)}."; return false;
            }
        }

        private double CalculatePopulationCount(Dictionary<string, DataStatsHolder> data, Template.Action action, int varIndex, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
            GetBoolParameterValue(PAR_GIVETOTALS, action, out bool giveTotals);
            GetDoubleParameterValue(PAR_THRESHOLD, action, out double threshold);

            List<List<double>> filterData = data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel]));

            var matchingData = filterData.Where(x => x[varIndex] > threshold || x[varIndex] < -threshold);
            double matching = matchingData.Sum(x => x[wgtVarIndex]);
            double result = matching == 0 ? matching : giveTotals ? matching : (double)matching / (double)filterData.Sum(x => x[wgtVarIndex]);

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, varIndex, data[action.CalculationLevel].GetData(), filterData);
            return result;
        }

        private double CalculatePovertyGap(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;
            if (!GetVarParameterInfo(PAR_INCOME, data, action, out int incIndex, out string incVar)) return double.NaN;

            if (!action.GetVariableParameter(PAR_POVERTYLINE, out string povLineVar)) return ActionError(action, $"Parameter {PAR_POVERTYLINE} not defined");
            if (!data[action.CalculationLevel].HasSavedNumber(povLineVar, action.localMap)) return ActionError(action, $"Saved variable {povLineVar} not found");
            DataStatsHolder.SavedNumber povLine = data[action.CalculationLevel].GetSavedNumber(povLineVar, action.localMap); if (double.IsNaN(povLine.number)) return double.NaN;
            if (povLine.number == 0) return ActionError(action, $"Index cannot be calculated, because poverty line is zero");

            GetBoolParameterValue(PAR_USE_SWITCH_APPROACH, action, out bool useSwitchApproach);

            int subGroupFilterIndex = -1;
            if (action.filter != null)
            {
                Template.Action filterAction = new Template.Action(action.localMap)
                {
                    _calculationLevel = action.CalculationLevel,
                    calculationType = HardDefinitions.CalculationType.CreateFlag,
                    filter = action.filter,
                    outputVar = "flag_" + Guid.NewGuid().ToString()
                };
                CreateFlag(data, filterAction);
                subGroupFilterIndex = data[action.CalculationLevel].GetVarIndex(filterAction.outputVar, filterAction.localMap);
                if (subGroupFilterIndex == -1) return ActionError(action, "Couldn't calculate filter for Poverty Gap");
            }
            Template.Filter poorFilter = new Template.Filter(data[action.CalculationLevel], x => x[incIndex] < povLine.number && (subGroupFilterIndex == -1 || x[subGroupFilterIndex] == 1), action.localMap);

            double povGap;
            if (useSwitchApproach)
            {
                if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
                double counter = data[action.CalculationLevel].GetData(poorFilter.GetFunc(data[action.CalculationLevel])).
                    Sum(x => x[wgtVarIndex] * (povLine.number - x[incIndex]) / povLine.number); // = weighted Sum { (povLine - income poor unit) / povLine }
                double denominator = data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel])).Sum(x => x[wgtVarIndex]); // weighted number of units (in possible sub-group)
                povGap = denominator == 0 ? double.NaN : counter / denominator;
            }
            else
            {
                Template.Action medAction = new Template.Action(action.localMap)
                {
                    _calculationLevel = action.CalculationLevel,
                    calculationType = HardDefinitions.CalculationType.CalculateMedian,
                    filter = poorFilter,
                    outputVar = "median_" + Guid.NewGuid().ToString(),
                    parameters = new List<Template.Parameter>() { new Template.Parameter() { name = PAR_INCOME, variableName = incVar } }
                };

                if (double.IsNaN(CalculateMedian(data, medAction, sdcDefinition, out int dummy)))
                    return ActionError(action, "Couldn't calculate the poor median for Poverty Gap");
                if (!data[medAction.CalculationLevel].HasSavedNumber(medAction.outputVar, medAction.localMap)) return ActionError(action, $"Index cannot be calculated, because assessing median failed (based on variable {incVar})");
                DataStatsHolder.SavedNumber median = data[medAction.CalculationLevel].GetSavedNumber(medAction.outputVar, medAction.localMap); // note that in a baseline-reform scenario this is the reform's median

                povGap = (povLine.number - median.number) / povLine.number;
            }

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, incIndex, data[action.CalculationLevel].GetData(), data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel])));

            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, povGap, sdcObsNo));
            return povGap;
        }

        private double CalculateMeanLogDeviation(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;

            if (data[action.CalculationLevel].HasSavedNumber(action.outputVar, action.localMap))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar, action.localMap);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }

            if (!GetVarParameterInfo(PAR_INCOME, data, action, out int incomeVarIndex, out string incomeVarName)) return double.NaN;
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);

            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, action.localMap, sdcDefinition, out _);
            if (totalPeople == 0) return ActionError(action, $"Mean Log Deviation cannot be calculated, because population is zero");

            Template.Action avAction = new Template.Action(action.localMap)
            {
                calculationType = HardDefinitions.CalculationType.CalculateWeightedAverage,
                _calculationLevel = action.CalculationLevel, parameters = action.parameters
            };
            double averageIncome = CalculateWeightedAverage(data, avAction, incomeVarIndex, sdcDefinition, out _);
            if (double.IsNaN(averageIncome)) return ActionError(action, "Couldn't calculate the average income for the Mean Log Deviation");

            double mld = 1 / totalPeople * data[action.CalculationLevel].GetData().
                Sum(x => x[wgtVarIndex] * RecNeg(x[incomeVarIndex]) == 0 ? 0 : Math.Log(averageIncome / RecNeg(x[incomeVarIndex])));

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, incomeVarIndex,
                data[action.CalculationLevel].GetData(), data[action.CalculationLevel].GetData()); // does not yet(!) allow for a filter, thus this is always the same data

            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, mld, sdcObsNo));
            return mld;

            double RecNeg(double inc) { return recodeNegatives ? Math.Max(inc, 0) : inc; }
        }

        private double CalculateAtkinson(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;

            if (data[action.CalculationLevel].HasSavedNumber(action.outputVar, action.localMap))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar, action.localMap);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }

            if (!GetVarParameterInfo(PAR_INCOME, data, action, out int incomeVarIndex, out string incomeVarName)) return double.NaN;
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);
            double epsilon = 0.25; if(action.HasParameter(PAR_ATKINSON_INEQUALITY_AVERSION)) GetDoubleParameterValue(PAR_ATKINSON_INEQUALITY_AVERSION, action, out epsilon);
            if (epsilon < 0) return ActionError(action, $"Atkinson index: {PAR_ATKINSON_INEQUALITY_AVERSION} parameter must be >= 0");

            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, action.localMap, sdcDefinition, out _);
            if (totalPeople == 0) return ActionError(action, $"Atkinson index cannot be calculated, because population is zero");

            Template.Action avAction = new Template.Action(action.localMap)
            {
                calculationType = HardDefinitions.CalculationType.CalculateWeightedAverage,
                _calculationLevel = action.CalculationLevel,
                parameters = action.parameters
            };
            double averageIncome = CalculateWeightedAverage(data, avAction, incomeVarIndex, sdcDefinition, out _);
            if (double.IsNaN(averageIncome)) return ActionError(action, "Couldn't calculate the average income for the Atkinson index");

            double atkinson = 0;
            if (epsilon != 1) atkinson = 1 - 1 / averageIncome * Math.Pow(1 / totalPeople * data[action.CalculationLevel].GetData()
                                        .Sum(x => x[wgtVarIndex] * Math.Pow(RecNeg(x[incomeVarIndex]), 1 - epsilon)), 1 / (1 - epsilon));
            else atkinson = 1 - 1 / averageIncome * Math.Exp(1 / totalPeople * data[action.CalculationLevel].GetData()
                                   .Sum(x => x[wgtVarIndex] * Math.Log(RecNeg(x[incomeVarIndex]))));

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, incomeVarIndex,
                data[action.CalculationLevel].GetData(), data[action.CalculationLevel].GetData()); // does not yet(!) allow for a filter, thus this is always the same data

            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, atkinson, sdcObsNo));
            return atkinson;

            double RecNeg(double inc) { return recodeNegatives ? Math.Max(inc, 0) : inc; }
        }

        // Scales are always automatically added to the individual and the action level datasets
        internal bool CreateOECDScale(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");
            // To support older templates, if there was no calculation level, assume HH
            string lvl = string.IsNullOrEmpty(action._calculationLevel) ? HH : action.CalculationLevel;
            if (data[IND].HasVariable(action.outputVar, action.localMap)) return true;   // if Individual data has this, all should have this
            GetBoolParameterValue(PAR_OXFORD, action, out bool Oxford);
            int ageIndex = data[IND].GetVarIndex(HardDefinitions.age, action.localMap);
            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(lvl), action.localMap);

            foreach (DataStatsHolder d in data.Values) d.AddVar(action.outputVar, false, action.localMap);

            var allGroups = from person in data[IND].GetData()
                        group person by person[groupIndex] into GRP
                        select GRP;
            foreach (var group in allGroups)
            {
                double vAdult = Oxford ? 0.7 : 0.5, vChild = Oxford ? 0.5 : 0.3, vHasAdult = Oxford ? 0.3 : 0.5, vNoAdult = Oxford ? 0.5 : 0.7;
                double scale = 0.0; bool hasAdult = false;
                foreach (List<double> member in group)
                {
                    if (member[ageIndex] < 14) scale += vChild;
                    else { scale += vAdult; hasAdult = true; }
                }
                scale += hasAdult ? vHasAdult : vNoAdult;

                foreach (List<double> member in group) member.Add(scale);
                data[lvl].GetObs(group.ElementAt(0)[groupIndex]).Add(scale);
            }
            return true;
        }

        // Scales are always automatically added to the individual and the action level datasets
        internal bool CreateEquivalenceScale(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");
            if (!GetEquivScaleBands(PAR_EQUIVSCALEBAND, action, out Dictionary<string, double> raw_bands) || raw_bands.Count == 0)
                return ActionErrorBool(action, "No band parameters defined");

            if (data[IND].HasVariable(action.outputVar, action.localMap)) return true;   // if Individual data has this, all should have this
            
            Dictionary<Func<List<double>, bool>, double> bands = new Dictionary<Func<List<double>, bool>, double>();
            
            foreach (KeyValuePair<string, double> raw_band in raw_bands)
            {
                Template.Filter filter = LocalMap.GetNamedFilter(template, raw_band.Key);
                if (filter == null) return ActionErrorBool(action, $"Named Filter {raw_band.Key} not defined");
                PrepareFilter(data, IND, action.localMap, filter, template);
                bands.Add(filter.GetFunc(data[IND]), raw_band.Value);
            }

            bool replaceFirst = action.GetDoubleParameter(PAR_EQUIVFIRSTPERSON, out double firstPersonValue);

            // To support older templates, if there was no calculation level, assume HH
            string lvl = string.IsNullOrEmpty(action._calculationLevel) ? HH : action.CalculationLevel;

            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(lvl), action.localMap);

            data[IND].AddVar(action.outputVar, false, action.localMap);
            data[lvl].AddVar(action.outputVar, false, action.localMap);
            var allGroups = from person in data[IND].GetData()
                        group person by person[groupIndex] into GRP
                        select GRP;
            foreach (var group in allGroups)
            {
                double scale = 0.0; double replaceValue = 0.0;
                foreach (List<double> member in group)
                {
                    foreach (Func<List<double>, bool> matchBand in bands.Keys)
                    {
                        if (matchBand(member))
                        {
                            scale += bands[matchBand];
                            if (bands[matchBand] > replaceValue) replaceValue = bands[matchBand];   // keep the biggest value, to be replaced by "FirstPerson" value
                            break;  // allow only one match in the conditions
                        }
                    }
                }
                if (replaceValue > 0 && replaceFirst) scale = scale + firstPersonValue - replaceValue;  // replace the biggest value (e.g. the adult) with the firstPersonValue
                foreach (List<double> member in group) member.Add(scale);
                data[lvl].GetObs(group.ElementAt(0)[groupIndex]).Add(scale);
            }

            return true;
        }

        // Equivalized values are always automatically added to both ind & action level datasets
        internal bool CreateEquivalized(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");
            if (!GetVarParameterInfo(PAR_EQUIVALENCESCALE, data, action, out int scaleIndex, out string scaleVar, null, true)) return ActionErrorBool(action, "No equivalence scale defined");

            if (data[IND].HasVariable(action.outputVar, action.localMap)) return true;

            // TODO - consider how this could allow multiple variables (e.g. with prefix instead of outputvar)
            if (!action.GetUnnamedVariableParameter(out string varName))
                return ActionErrorBool(action, $"Variable to equivalise not defined (i.e. unnamed variable-Parameter missing)");
            int varIndex = data[IND].GetVarIndex(varName, action.localMap);
            if (varIndex < 0) return ActionErrorBool(action, $"{varName} not found");

            //  to support older templates, if calculation level is missing, use HH 
            string lvl = string.IsNullOrEmpty(action._calculationLevel) ? HH : action.CalculationLevel;

            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(lvl), action.localMap);

            data[IND].AddVar(action.outputVar, false, action.localMap);
            if (lvl != IND) data[lvl].AddVar(action.outputVar, false, action.localMap);
            var allGroups = from person in data[IND].GetData()
                        group person by person[groupIndex] into GRP
                        select GRP;
            int cntZeroScale = 0;
            foreach (var group in allGroups)
            {
                double totVar = group.Sum(x => x[varIndex]);
                double scale = group.ElementAt(0)[scaleIndex];
                if (scale == 0) { ++cntZeroScale; scale = 1; }
                double eqVar = totVar / scale;
                if (lvl != IND) data[lvl].GetObs(group.ElementAt(0)[groupIndex]).Add(eqVar);
                foreach (List<double> member in group) member.Add(eqVar);
            }
            if (cntZeroScale != 0) ActionErrorBool(action, $"Equivalence scale ({scaleVar}) was zero in {cntZeroScale} cases ({varName} was not scaled)");

            return true;
        }

        /**
         * This function brings a variable from the individual dataset to the HH dataset, as an aggregate of the household
         * It accepts a SumVar parameter that denotes the Individual variable to be summed. If this is missing, the OutputVar is used.
         * Is accepts a CopyToIndividuals parameter that allows copying the summed variable back to the Individuals.
         * If the OutputVar exists in the HH dataset the values are replaced (same for the Individuals if CopyToIndividuals is true).
         */
        internal bool CreateGroupValue(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");

            string var = action.outputVar; // if no SumVar is defined, use OutputVar
            if (action.HasParameter(PAR_SUMVAR) && !GetVarParameterInfo(PAR_SUMVAR, data, action, out _, out var, null, true)) return ActionErrorBool(action, "No summing variable defined");
            GetBoolParameterValue(PAR_COPY_TO_INDIVIDUALS, action, out bool copyToIndDataset);
            bool? takeFromHead = null; if(action.HasParameter(PAR_COPY_TO_GROUP) && GetBoolParameterValue(PAR_COPY_TO_GROUP, action, out bool b)) takeFromHead = b;

            CreateGroupValue(data, action.CalculationLevel, var, action.outputVar, action.localMap, takeFromHead, copyToIndDataset);
            return true;
        }

        private void CreateGroupValue(Dictionary<string, DataStatsHolder> data, string calculationLevel, string var,
                                      string outputVar = null, LocalMap localMap = null, bool? takeFromHead = null, bool copyToIndDataset = false)
        {
            if (outputVar == null) outputVar = var;
            bool outVarExists_group = data[calculationLevel].HasVariable(outputVar, localMap);
            bool outVarExists_ind = data[IND].HasVariable(outputVar, localMap);

            if (!data[IND].HasVariable(var, localMap)) { errorCollector.AddError($"CreateGroupValue: variable '{var}' not available on individual level."); return; }
            int indexVar_ind = data[IND].GetVarIndex(var, localMap); bool monetary = data[IND].IsVarMonetary(var, localMap);
            if (takeFromHead == null) takeFromHead = !monetary; // do not sum up over group-members, if parameter requires this
                                                                // otherwise if variable is non-monetary
            if (!outVarExists_group) data[calculationLevel].AddVar(outputVar, monetary, localMap);
            if (copyToIndDataset && !outVarExists_ind) data[IND].AddVar(outputVar, monetary, localMap);

            int indexOutVar_ind = copyToIndDataset ? data[IND].GetVarIndex(outputVar, localMap) : -1;
            int indexOutVar_group = data[calculationLevel].GetVarIndex(outputVar, localMap);
            int indexGroupingVar = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(calculationLevel), localMap);
            int indexIdPerson = data[IND].GetKeyIndex();

            foreach (var grp in from person in data[IND].GetData()
                                group person by person[indexGroupingVar] into GRP
                                select GRP)
            {
                double groupValue;
                if (takeFromHead == true)
                {   // search for the person, where idPerson is equal to the grouping var of this level
                    var headVars = from g in grp where g[indexIdPerson] == grp.Key select g;
                    groupValue = headVars.Any() ? headVars.First()[indexVar_ind] : grp.First()[indexVar_ind];
                }
                else groupValue = grp.Sum(x => x[indexVar_ind]);

                // add group-value to group-dataset
                if (outVarExists_group) data[calculationLevel].GetObs(grp.ElementAt(0)[indexGroupingVar])[indexOutVar_group] = groupValue; 
                else data[calculationLevel].GetObs(grp.ElementAt(0)[indexGroupingVar]).Add(groupValue);
                // add group-value to ind-dataset
                if (copyToIndDataset)
                {
                    if (outVarExists_ind) grp.ToList().ForEach(x => x[indexOutVar_ind] = groupValue);
                    else grp.ToList().ForEach(x => x.Add(groupValue));
                }
            }
        }

        /**
         * This function calculates the median for a given variable, at a given dataset. One can specify multiple facets...
         */
        private double CalculateMedian(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;

            if (data[action.CalculationLevel].HasSavedNumber(action.outputVar, action.localMap))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar, action.localMap);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }
                
            Func<List<double>, bool> filter = action.filter?.GetFunc(data[action.CalculationLevel]);
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName, true) |
                !GetVarParameterInfo(PAR_INCOME, data, action, out int incIndex, out string incVar, null, true)) return ActionError(action, "Both Income and Weight parameters are required for calculating the Median.");

            double aggPeople = 0;
            double prevVar = Double.NegativeInfinity;
            double prevWeight = Double.NegativeInfinity;
            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, action.localMap, sdcDefinition, out sdcObsNo, filter);
            if (totalPeople == 0) return ActionError(action, $"Index cannot be calculated, because population-count is zero (based on weight variable {wgtVarName})");

            foreach (List<double> person in data[action.CalculationLevel].GetData().OrderBy(x => x[incIndex]))
            {
                if (filter == null || filter(person))
                {
                    prevWeight = aggPeople;
                    aggPeople += person[wgtVarIndex];
                    int weightComp = (prevWeight / totalPeople).CompareTo(0.5);
                    int valueComp = prevVar.CompareTo(person[incIndex]);
                    if (weightComp > -1 && valueComp == -1)
                    {
                        if (weightComp == 0)
                            prevVar = (prevVar + person[incIndex]) / 2;
                        break;
                    }
                    prevVar = person[incIndex];
                }
            }
            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(action.outputVar, action.localMap, prevVar, sdcObsNo));

            return prevVar;
        }

        /**
         * This function generates deciles for a given variable, at a given dataset. One can specify multiple facets...
         */
        private bool CreateDeciles(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");

            if (data[action.CalculationLevel].HasVariable(action.outputVar, action.localMap)) return true;
            Func<List<double>, bool> filter = action.filter?.GetFunc(data[action.CalculationLevel]);
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName) |
                !GetVarParameterInfo(PAR_INCOME, data, action, out int incIndex, out string incVar)) return false;

            double decNo = 10;
            if (action.HasParameter(PAR_DECNO) && !action.GetDoubleParameter(PAR_DECNO, out decNo))
                return ActionErrorBool(action, $"Parameter {PAR_DECNO}: numericValue not defined");
            if (decNo == 0)
                return ActionErrorBool(action, $"Parameter {PAR_DECNO}: number of quantiles cannot be zero");
            GetBoolParameterValue(PAR_EQUALDECILES, action, out bool equalDeciles);

            // initialise SavedNumbers for cut-offs for the case that observations do not allow for generating all of them, or an error occurs
            for (double co = 1; co < decNo; ++co)
                data[IND].SetSavedNumber(new DataStatsHolder.SavedNumber(GetVarNameCutOff(action.outputVar, co), action.localMap, double.NaN, int.MaxValue));

            double dec = 1; double aggPeople = 0;
            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, action.localMap, new Template.Page.Table.SDCDefinition(), out int obsNoTotal, filter);
            if (totalPeople == 0) return ActionErrorBool(action, $"Index cannot be calculated, because population-count is zero (based on weight variable {wgtVarName})");
            double prevVar = Double.NegativeInfinity;
            double prevWeight = Double.NegativeInfinity;

            if (NoGrouping(action))
            {
                data[action.CalculationLevel].AddVar(action.outputVar, false, action.localMap); int obsNoQuant = 0;
                foreach (List<double> person in data[action.CalculationLevel].GetData().OrderBy(x => x[incIndex]))
                {
                    if (filter == null || filter(person))
                    {
                        prevWeight = aggPeople;
                        aggPeople += person[wgtVarIndex];
                        int weightComp = (prevWeight / totalPeople).CompareTo(dec / decNo);
                        int valueComp = prevVar.CompareTo(person[incIndex]);
                        if (weightComp > -1 && (equalDeciles || valueComp == -1))
                        {
                            dec++;
                            if (weightComp == 0)
                                prevVar = (prevVar + person[incIndex]) / 2;
                            data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(GetVarNameCutOff(action.outputVar, dec - 1), 
                                                                                                         action.localMap, prevVar, obsNoTotal));
                            obsNoQuant = 0;
                        }
                        prevVar = person[incIndex];
                        person.Add(dec); ++obsNoQuant;
                    }
                    else // if there is a filter then make sure that everyone not included gets decile 0!
                    {
                        person.Add(0);
                    }
                }
            }
            // IMPORTANT NOTE (assuming individual data and decile-building for simplicity of explanation):
            // Using a GroupingVar does not - as one may assume - split data in 10 deciles, where each decile contains an equal number of GROUPS (simplified).
            // Instead it still splits data in 10 deciles, where each decile contains an equal number of PERSONS (simplified).
            // The actual effects of using a GroupingVar are the following:
            // - it makes sure that persons within a group (i.e. usually HH-members) stay in the same decile
            // - if idhh is the GroupingVar, it writes the deciles not only to individual data, but also to household data
            else
            {
                if (!GetGroupingVar(out int grpVarIndex, out string grpVarName, data, action)) return false;
                // to support old templates, if the group is an existing calculation level and is not the same as the action calculation level, write the deciles to both
                string grpLvl = template.info.GetCalculationLevelFromVar(grpVarName);
                bool writeToGroup = !string.IsNullOrEmpty(grpLvl) && action.CalculationLevel != grpLvl;
                data[action.CalculationLevel].AddVar(action.outputVar, false, action.localMap);
                if (writeToGroup) data[grpLvl].AddVar(action.outputVar, false, action.localMap);
                var allGroups = from person in data[action.CalculationLevel].GetData().OrderBy(x => x[incIndex])
                                group person by person[grpVarIndex] into GRP
                                select GRP;
                int obsNoQuant = 0;
                foreach (var group in allGroups)
                {
                    if (filter == null || filter(group.ToArray()[0]))
                    {
                        double hhVariableRankWeighted = 0, hhPersonsWeighted = 0;
                        foreach (List<double> member in group)
                        {
                            hhVariableRankWeighted = member[incIndex];
                            hhPersonsWeighted += member[wgtVarIndex];
                        }
                        prevWeight = aggPeople;
                        aggPeople += hhPersonsWeighted;
                        int weightComp = (prevWeight / totalPeople).CompareTo(dec / decNo);
                        int valueComp = prevVar.CompareTo(hhVariableRankWeighted);
                        if (weightComp > -1 && (equalDeciles || valueComp == -1))
                        {
                            dec++;
                            if (weightComp == 0)
                                prevVar = (prevVar + hhVariableRankWeighted) / 2;
                            data[action.CalculationLevel].SetSavedNumber(new DataStatsHolder.SavedNumber(GetVarNameCutOff(action.outputVar, dec - 1), 
                                                                                                         action.localMap, prevVar, obsNoTotal));
                            if (writeToGroup) data[grpLvl].SetSavedNumber(new DataStatsHolder.SavedNumber(GetVarNameCutOff(action.outputVar, dec - 1), 
                                                                                                          action.localMap, prevVar, obsNoTotal));
                            obsNoQuant = 0;
                        }
                        ++obsNoQuant;
                        prevVar = hhVariableRankWeighted;
                        foreach (List<double> member in group)
                            member.Add(dec);
                        if (writeToGroup) data[grpLvl].GetObs(group.First()[grpVarIndex]).Add(dec);
                    }
                    else    // if there is a filter then make sure that everyone not included gets decile 0!
                    {
                        foreach (List<double> member in group)
                            member.Add(0);
                        if (writeToGroup) data[grpLvl].GetObs(group.First()[grpVarIndex]).Add(0);
                    }
                }
            }

            if (dec < decNo) ActionErrorBool(action, $"Data did only allow for building {dec} quantiles (instead of {decNo})"); // still return true
            if (dec > decNo) return ActionErrorBool(action, $"Unexpected error in decile calculations! Built {dec} quantiles (instead of {decNo})"); // still return true
            return true;
        }

        private void IssueMessage(Dictionary<string, DataStatsHolder> data, Template.Action action) //, int refNo) // todo: consider whether refNo can be made useable for this (currently it is not reliable, as for non-CellActions (i.e. Global/Page/Table-Actions) it is always -1 (should it?)
        {
            string msgSwitchVar = null, defaultMessage = null, zeroMessage = null, nonZeroMessage = null;
            Dictionary<double, string> ifValMessages = new Dictionary<double, string>();
            Dictionary<int, string> rangeMessages = new Dictionary<int, string>();
            Dictionary<int, double> rangeMinVals = new Dictionary<int, double>(), rangeMaxVals = new Dictionary<int, double>();
            foreach (Template.Parameter parameter in action.parameters)
            {
                switch (parameter.name)
                {
                    case PAR_MESSAGE_SWITCH_VAR:
                        if (string.IsNullOrEmpty(parameter.variableName)) ActionError(action, $"Parameter {parameter.name}: no 'VarName' defined.");
                        else msgSwitchVar = parameter.variableName; break;
                    case PAR_MESSAGE_DEFAULT:
                        if (string.IsNullOrEmpty(parameter.stringValue)) ActionError(action, $"Parameter {parameter.name}: no 'StringValue' (i.e. message) defined.");
                        else defaultMessage = parameter.stringValue; break;
                    case PAR_MESSAGE_IF_ZERO:
                        if (string.IsNullOrEmpty(parameter.stringValue)) ActionError(action, $"Parameter {parameter.name}: no 'StringValue' (i.e. message) defined.");
                        else zeroMessage = parameter.stringValue; break;
                    case PAR_MESSAGE_IF_NON_ZERO:
                        if (string.IsNullOrEmpty(parameter.stringValue)) ActionError(action, $"Parameter {parameter.name}: no 'StringValue' (i.e. message) defined.");
                        else nonZeroMessage = parameter.stringValue; break;
                    case PAR_MESSAGE_IF_VAL:
                        if (string.IsNullOrEmpty(parameter.stringValue)) ActionError(action, $"Parameter {parameter.name}: no 'StringValue' (i.e. message) defined.");
                        else if (double.IsNaN(parameter.numericValue)) ActionError(action, $"Parameter {parameter.name}: no 'NumericValue' defined.");
                        else if (ifValMessages.ContainsKey(parameter.numericValue)) ActionError(action, $"Parameter {parameter.name}: message for {parameter.numericValue} already defined.");
                        else ifValMessages.Add(parameter.numericValue, parameter.stringValue);
                        break;
                    default:
                        if (parameter.name.StartsWith(PAR_MESSAGE_RANGE_X))
                        {
                            if (string.IsNullOrEmpty(parameter.stringValue)) ActionError(action, $"Parameter {parameter.name}: no 'StringValue' (i.e. message) defined.");
                            else if (GetRangeNum(out int rangeNum))
                            {
                                if (rangeMessages.ContainsKey(rangeNum)) ActionError(action, $"Parameter {parameter.name}: multiple definition of message for range {rangeNum}.");
                                else rangeMessages.Add(rangeNum, parameter.stringValue);
                            }
                        }
                        else if (parameter.name.StartsWith(PAR_MESSAGE_RANGE_MIN_X))
                        {
                            if (double.IsNaN(parameter.numericValue)) ActionError(action, $"Parameter {parameter.name}: no 'NumericValue' (i.e. minimum) defined.");
                            else if (GetRangeNum(out int rangeNum))
                            {
                                if (rangeMinVals.ContainsKey(rangeNum)) ActionError(action, $"Parameter {parameter.name}: multiple definition of minimum for range {rangeNum}.");
                                else rangeMinVals.Add(rangeNum, parameter.numericValue);
                                if (!string.IsNullOrEmpty(parameter.stringValue)) // allows for defining message without necessity to define a second parameter
                                {
                                    if (rangeMessages.ContainsKey(rangeNum)) ActionError(action, $"Parameter {parameter.name}: multiple definition of message for range {rangeNum}.");
                                    else rangeMessages.Add(rangeNum, parameter.stringValue);
                                }
                            }
                        }
                        else if (parameter.name.StartsWith(PAR_MESSAGE_RANGE_MAX_X))
                        {
                            if (double.IsNaN(parameter.numericValue)) ActionError(action, $"Parameter {parameter.name}: no 'NumericValue' (i.e. maximum) defined.");
                            else if (GetRangeNum(out int rangeNum))
                            {
                                if (rangeMaxVals.ContainsKey(rangeNum)) ActionError(action, $"Parameter {parameter.name}: multiple definition of maximum for range {rangeNum}.");
                                else rangeMaxVals.Add(rangeNum, parameter.numericValue);
                                if (!string.IsNullOrEmpty(parameter.stringValue)) // allows for defining message without necessity to define a second parameter
                                {
                                    if (rangeMessages.ContainsKey(rangeNum)) ActionError(action, $"Parameter {parameter.name}: multiple definition of message for range {rangeNum}.");
                                    else rangeMessages.Add(rangeNum, parameter.stringValue);
                                }
                            }
                        }
                        break;

                        bool GetRangeNum(out int rn)
                        {
                            rn = -1; int i = parameter.name.LastIndexOf('_');
                            return i < 0 || i == parameter.name.Length - 1 || !int.TryParse(parameter.name.Substring(i + 1), out rn)
                                ? ActionErrorBool(action, $"Parameter {parameter.name}: no or invalid range number.") : true;
                        }
                }
            }

            string message = null;
            if (!string.IsNullOrEmpty(zeroMessage) || !string.IsNullOrEmpty(nonZeroMessage) || ifValMessages.Any() || rangeMessages.Any())
            {
                if (msgSwitchVar == null)
                    ActionError(action, $"Parameter '{PAR_MESSAGE_SWITCH_VAR}' not defined.");
                else if (!data[action.CalculationLevel].HasSavedNumber(msgSwitchVar, action.localMap))
                    ActionError(action, $"Parameter {PAR_MESSAGE_SWITCH_VAR}: saved number {msgSwitchVar} not found.");
                else
                {
                    double val = data[action.CalculationLevel].GetSavedNumber(msgSwitchVar, action.localMap).number;

                    // 1st priority exact match
                    if (ifValMessages.ContainsKey(val)) message = ifValMessages[val];
                    // 2nd priority range
                    if (message == null && rangeMessages.Any())
                    {
                        SortedDictionary<double, Tuple<double, string>> ranges = new SortedDictionary<double, Tuple<double, string>>();
                        foreach (var msg in rangeMessages)
                        {
                            double min = rangeMinVals.TryGetValue(msg.Key, out double m) ? m : double.MinValue;
                            double max = rangeMaxVals.TryGetValue(msg.Key, out m) ? m : double.MaxValue;
                            if (min == double.MinValue && max == double.MaxValue) { ActionError(action, $"{PAR_MESSAGE_RANGE_X}{msg.Key}: neither minimum nor maximum defined."); continue; }
                            if (min > max) ActionError(action, $"{PAR_MESSAGE_RANGE_X}{msg.Key}: minimum ({min}) > maximum ({max})");
                            else ranges.Add(min, new Tuple<double, string>(max, msg.Value));
                        }
                        foreach (var m in rangeMinVals) if (!rangeMessages.ContainsKey(m.Key)) ActionError(action, $"{PAR_MESSAGE_RANGE_MIN_X}{m.Key}: no message defined.");
                        foreach (var m in rangeMaxVals) if (!rangeMessages.ContainsKey(m.Key)) ActionError(action, $"{PAR_MESSAGE_RANGE_MAX_X}{m.Key}: no message defined.");
                        double? prvMax = null; foreach (var range in ranges) if (prvMax != null && range.Key <= prvMax)
                                ActionError(action, $"Range {range.Key} intersects with previous range, no distinct map possible.");
                        foreach (var range in ranges) if (range.Key <= val && range.Value.Item1 >= val) { message = range.Value.Item2; break; }
                    }
                    // 3rd priority: zero / non-zero
                    if (message == null && val == 0 && !string.IsNullOrEmpty(zeroMessage)) message = zeroMessage;
                    if (message == null && val != 0 && !string.IsNullOrEmpty(nonZeroMessage)) message = nonZeroMessage;
                }
            }
            // last priority default message
            if (message == null && !string.IsNullOrEmpty(defaultMessage)) message = defaultMessage;
            if (string.IsNullOrEmpty(message)) return;

            int refNo = ExtractRefFromVarName(msgSwitchVar, out _);
            errorCollector.AddError(PrettyInfoProvider.GetPrettyText(template.info,
                refNo == -1 ? message.Replace("[ref", "[base") : message, // this allows for using e.g. [refSys] in Global/Page/Table-Actions to address both, base- and reform-systems (also see comment in function-header, all not very clean and could be improved)
                baselineSystemInfos[0], reformSystemInfos, packageKey, refNo));
        }

        private string GetVarNameCutOff(string outputVarName, double cutOffNumber)
        {
            if (!outputVarName.Contains(HardDefinitions.Reform))
                return $"{outputVarName}{HardDefinitions.DecileSeparator}{cutOffNumber}"; // e.g. DecilesEqDispy~dec~5
            // the cut-offs for reforms need to be saved as decname~dec~Y~ref~X (i.e. put ~dec~Y before ref~X)
            int r = outputVarName.IndexOf(HardDefinitions.Reform); // e.g. DecilesEqDispy~dec~5~ref~1
            return $"{outputVarName.Substring(0, r)}{HardDefinitions.DecileSeparator}{cutOffNumber}{outputVarName.Substring(r)}";
        }

        internal static string GetRealVariableName(string varName, Template.TemplateInfo templateInfo, bool forCaption = false, string packageKey = null, int refNo = -1)
        {
            if (varName == null) return null;
            if (varName.StartsWith("[@"))
            {
                Template.TemplateInfo.UserVariable uv = templateInfo.GetUserVariable(varName.Substring(2, varName.Length - 3), packageKey, refNo);
                return uv == null ? null : forCaption && uv.displayDescription ? uv.description : uv.value;
            }
            else
                return varName;
        }

        internal static Template.Action FixActionUserVariables(Template.Action action, Template template)
        {
            action.outputVar = GetRealVariableName(action.outputVar, template.info);
            foreach (Template.Parameter par in action.parameters)
                if (par.name == "VarName")
                    par.variableName = GetRealVariableName(par.variableName, template.info);
            return action;
        }

        #region HELPERS

        double ActionError(Template.Action action, string msg) { ActionErrorX(action, msg); return double.NaN; }
        private bool ActionErrorBool(Template.Action action, string msg) { ActionErrorX(action, msg); return false; }
        void ActionErrorX(Template.Action action, string msg)
        {
            string ident = string.Empty;
            if (!template.info.endUserFriendlyActionErrorMode)
            {
                ident = string.IsNullOrEmpty(action.name) ? string.Empty : $"Name='{action.name}'; ";
                // the longwinded ident-construction is just for finding an "identifier" that allows users finding the cause of the error
                if (action.calculationType != HardDefinitions.CalculationType.NA) ident += $"CalculationType='{action.calculationType}'; ";
                if (!string.IsNullOrEmpty(action.outputVar)) ident += $"OutputVar='{action.outputVar}'; ";
                if (!string.IsNullOrEmpty(action.formulaString))
                {
                    string fs = action.formulaString.Length < 50 ? action.formulaString : action.formulaString.Substring(0, 50);
                    ident += $"FormulaString='{fs}{(fs.Length == action.formulaString.Length ? string.Empty : " ...")}'";
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(action.outputVar)) ident = $"OutputVar='{action.outputVar}'; ";
                if (!string.IsNullOrEmpty(action.formulaString))
                    ident += "FormulaString='" + action.formulaString.Replace(HardDefinitions.FormulaParameter.SAVED_VAR, string.Empty)
                                                                    .Replace(HardDefinitions.FormulaParameter.DATA_VAR, string.Empty)
                                                                    .Replace(HardDefinitions.FormulaParameter.OPENING_TOKEN, string.Empty)
                                                                    .Replace(HardDefinitions.FormulaParameter.CLOSING_TOKEN, string.Empty)
                                                                    .Replace("@", string.Empty) + "'";
            }
            errorCollector.AddError($"Error in Action: {ident.TrimEnd(new char[] { ' ', ';' })}:{Environment.NewLine}{msg}");

            action.result = double.NaN;
        }

        private bool GetVarParameterInfo(string parName, Dictionary<string, DataStatsHolder> data, Template.Action action, out int varIndex, out string varName, string defaultVarName = null, bool getINDindex = false)
        {
            varIndex = -1;
            if (!action.GetVariableParameter(parName, out varName))
            {
                if (string.IsNullOrEmpty(defaultVarName)) { ActionError(action, $"Parameter {parName} not defined or Tag VarName missing"); return false; }
                varName = defaultVarName; // optional parameter with default value (e.g. idhh for GroupingVar)
            }
            varIndex = data[getINDindex ? IND : action.CalculationLevel].GetVarIndex(varName, action.localMap);
            if (varIndex >= 0) return true;
            ActionError(action, $"{parName} {varName} not found"); return false;
        }
        private bool GetWeightParameterInfo(Dictionary<string, DataStatsHolder> data, Template.Action action, out int wgtVarIndex, out string wgtVarName, bool getINDindex = false)
        { return GetVarParameterInfo(PAR_WEIGHT, data, action, out wgtVarIndex, out wgtVarName, HardDefinitions.weight, getINDindex); }
        private bool GetGroupingVar(out int grpIndex, out string grpName, Dictionary<string, DataStatsHolder> data, Template.Action action, bool getINDindex = false)
        { return GetVarParameterInfo(PAR_GROUPING, data, action, out grpIndex, out grpName, template.info.GetCalculationLevelVar(action.CalculationLevel), getINDindex); }
        private bool NoGrouping(Template.Action action) { return !action.GetVariableParameter(PAR_GROUPING, out string dummy); }

        private bool GetBoolParameterValue(string parName, Template.Action action, out bool boolValue, bool optional = true)
        {
            boolValue = false;
            if (!action.HasParameter(parName)) return optional ? true : ActionErrorBool(action, $"Parameter {parName} not defined");
            if (action.GetBoolParameter(parName, out boolValue)) return true;
            return ActionErrorBool(action, $"Parameter {parName}: BoolValue not defined");
        }

        private bool GetDoubleParameterValue(string parName, Template.Action action, out double doubleValue, bool optional = true)
        {
            doubleValue = 0;
            if (!action.HasParameter(parName)) return optional ? true : ActionErrorBool(action, $"Parameter {parName} not defined");
            if (action.GetDoubleParameter(parName, out doubleValue)) return true;
            return ActionErrorBool(action, $"Parameter {parName}: NumericValue not defined");
        }

        private bool GetEquivScaleBands(string parName, Template.Action action, out Dictionary<string, double> bands, bool optional = true)
        {
            bands = new Dictionary<string, double>();
            if (!action.HasParameter(parName)) return optional ? true : ActionErrorBool(action, $"Parameter {parName} not defined");
            if (action.GetEquivBandsParameters(parName, out bands)) return true;
            return ActionErrorBool(action, $"Parameter {parName}: VariableName or DoubleValue not defined");
        }

        private bool ForceRecalculation(Template.Action action)
        {
            return action.HasParameter(PAR_FORCE_RECALCULATION) && action.GetBoolParameter(PAR_FORCE_RECALCULATION, out bool recalc) && recalc;
        }

        private string GetBaseVarName(string refVarName, int refNo)
        {
            return refVarName.Replace(HardDefinitions.Reform + refNo, "");
        }

        #endregion HELPERS
    }
}
