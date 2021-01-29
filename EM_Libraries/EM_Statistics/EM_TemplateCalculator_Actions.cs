using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Statistics
{
    public partial class EM_TemplateCalculator
    {
        private const string PAR_WEIGHT = "WeightVar";
        private const string PAR_GROUPING = "GroupingVar";
        private const string PAR_DECILE = "DecileVar";
        private const string PAR_GINI = "GiniVar";
        private const string PAR_S8020 = "S8020Var";
        private const string PAR_INCOME = "IncomeVar";
        private const string PAR_POVERTYLINE = "PovertyLine";
        private const string PAR_USE_SWITCH_APPROACH = "UseSwitchApproach";
        private const string PAR_DECNO = "DecNo";
        private const string PAR_EQUALDECILES = "EqualDeciles";
        private const string PAR_OXFORD = "Oxford";
        private const string PAR_RECODENEGATIVES = "RecodeNegatives";
        private const string PAR_EQUIVALENCESCALE = "EquivalenceScale";
        private const string PAR_SUMVAR = "SumVar";
        private const string PAR_COPY_TO_INDIVIDUALS = "CopyToIndividuals";
        private const string PAR_COPY_TO_GROUP = "CopyToGroup";
        private const string PAR_GIVETOTALS = "GiveTotals";
        private const string PAR_THRESHOLD = "Threshold";
        private const string PAR_EQUIVSCALEBAND = "EquivalenceScaleBand";
        private const string PAR_EQUIVFIRSTPERSON = "EquivalenceFirstPerson";
        private const string PAR_FORCE_RECALCULATION = "ForceRecalculation";

        private bool CreateArithmeticColumn(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");

            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);
            if (data[action.CalculationLevel].HasVariable(action.outputVar)) return true;
            data[action.CalculationLevel].AddVar(action.outputVar);
            if (action.filter == null)
                data[action.CalculationLevel].GetData().ForEach(x => x.Add(recodeNegatives ? Math.Max(action.func(x), 0): action.func(x)));
            else
                data[action.CalculationLevel].GetData().ForEach(x => x.Add(recodeNegatives?Math.Max((action.filter.GetFunc(data[action.CalculationLevel])(x) ? action.func(x) : 0), 0): (action.filter.GetFunc(data[action.CalculationLevel])(x) ? action.func(x) : 0)));
            return true;
        }

        private double CalculateArithmeticAction(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            if (action.SaveResult && data[action.CalculationLevel].HasVariable(action.outputVar))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }
            List<double> array = new List<double>();
            double result = action.func(array);
            sdcObsNo = data[action.CalculationLevel].GetData().Count();
            if (action.SaveResult && !string.IsNullOrEmpty(action.outputVar))
                data[action.CalculationLevel].SetSavedNumber(action.outputVar, new DataStatsHolder.SavedNumber(result, sdcObsNo));
            return result;
        }

        private bool CreateFlag(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");

            if (data[action.CalculationLevel].HasVariable(action.outputVar)) return true;

            data[action.CalculationLevel].AddVar(action.outputVar);
            data[action.CalculationLevel].GetData().ForEach(x => x.Add(action.filter.GetFunc(data[action.CalculationLevel])(x) ? 1 : 0));
            return true;
        }

        // this is only supposed to be called as a helping function from within the library
        private static double Sum(Dictionary<string, DataStatsHolder> data, string level, string var,
                                  Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo,
                                  string outputVar = null, Func<List<double>, bool> filter = null)
        {
            if (outputVar == null && filter == null) outputVar = HardDefinitions.SavedTotal + var;
            if (outputVar != null && data[level].HasSavedNumber(outputVar))
            {
                DataStatsHolder.SavedNumber savedNumber = data[level].GetSavedNumber(outputVar);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }
                
            int varIndex = data[level].GetVarIndex(var);
            var filteredData = data[level].GetData(filter);
            double sum = filteredData.Sum(x => x[varIndex]);
            
            sdcObsNo = CalculateSdcObsNo(sdcDefinition, varIndex, data[level].GetData(), filteredData);

            if (outputVar != null) data[level].SetSavedNumber(outputVar, new DataStatsHolder.SavedNumber(sum, sdcObsNo));
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

            double gini = 0;
            if (!GetVarParameterInfo(PAR_GINI, data, action, out int giniVarIndex, out string giniVarName) |
                !GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;

            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);
            string wtIncomeVar = giniVarName + HardDefinitions.Separator + wgtVarName;

            Template.Action makeWtIncomeAction = new Template.Action() {
                calculationType = HardDefinitions.CalculationType.CreateArithmetic,
                formulaString = string.Format("1.0 * OBS_VAR[{0}] * OBS_VAR[{1}]", giniVarIndex, wgtVarIndex),
                parameters = recodeNegatives?new List<Template.Parameter>() { new Template.Parameter() { name = "RecodeNegatives", boolValue = true } }: new List<Template.Parameter>(),
                outputVar = wtIncomeVar
            };

            if (double.IsNaN(HandleAction(data, makeWtIncomeAction, sdcDefinition, out int obsno))) return double.NaN;

            double totalGini = Sum(data, action.CalculationLevel, wtIncomeVar, sdcDefinition, out sdcObsNo);
            if (totalGini == 0) return ActionError(action, $"Gini cannot be calculated, because sum of (weighted) {giniVarName} is zero");
            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, sdcDefinition, out int dummy);
            if (totalPeople == 0) return ActionError(action, $"Gini cannot be calculated, because population is zero");

            int wtGiniVarIndex = data[action.CalculationLevel].GetVarIndex(wtIncomeVar);
            double cumShareVariableRankWeighted = 0, cumSharePersonsWeighted = 0;

            if (NoGrouping(action))
            {
                var all = data[action.CalculationLevel].GetData().OrderBy(x => x[giniVarIndex]);
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

                var all = data[action.CalculationLevel].GetData().OrderBy(x => x[giniVarIndex]);
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
            return gini;
        }

        private double CalculateS8020(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;
            if (!GetVarParameterInfo(PAR_DECILE, data, action, out int decileIndex, out string dummy) |
                !GetVarParameterInfo(PAR_S8020, data, action, out int incomeVarIndex, out string incomeVarName)) return double.NaN;

            GetBoolParameterValue(PAR_RECODENEGATIVES, action, out bool recodeNegatives);

            Template.Action actionD1_2 = new Template.Action()
            {
                calculationType = HardDefinitions.CalculationType.CalculateSumWeighted,
                _calculationLevel = action._calculationLevel,
                filter = new Template.Filter(data[action.CalculationLevel], x => x[decileIndex] == 1 || x[decileIndex] == 2),
                formulaString = string.Format("1.0 * OP_VAR[{0}]", incomeVarIndex),
                parameters = recodeNegatives ? new List<Template.Parameter>() { new Template.Parameter() { name = "RecodeNegatives", boolValue = true } } : new List<Template.Parameter>()
            };
            Template.Action actionD9_10 = new Template.Action()
            {
                calculationType = HardDefinitions.CalculationType.CalculateSumWeighted,
                _calculationLevel = action._calculationLevel,
                filter = new Template.Filter(data[action.CalculationLevel], x => x[decileIndex] == 9 || x[decileIndex] == 10),
                formulaString = string.Format("1.0 * OP_VAR[{0}]", incomeVarIndex),
                parameters = recodeNegatives ? new List<Template.Parameter>() { new Template.Parameter() { name = "RecodeNegatives", boolValue = true } } : new List<Template.Parameter>()
            };

            double s80 = HandleAction(data, actionD9_10, sdcDefinition, out int dummy1); if (double.IsNaN(s80)) return double.NaN;
            double s20 = HandleAction(data, actionD1_2, sdcDefinition, out int dummy2); if (double.IsNaN(s20)) return double.NaN;

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, incomeVarIndex,
                data[action.CalculationLevel].GetData(), data[action.CalculationLevel].GetData()); // S80/20 does not yet(!) allow for a filter, thus this is always the same data

            if (s20 == 0) return ActionError(action, $"Index cannot be calculated, because sum of (weighted) {incomeVarName} in lowest quintile is zero");
            return s80 / s20;
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
            if (!data[action.CalculationLevel].HasSavedNumber(povLineVar)) return ActionError(action, $"Saved variable {povLineVar} not found");
            DataStatsHolder.SavedNumber povLine = data[action.CalculationLevel].GetSavedNumber(povLineVar); if (double.IsNaN(povLine.number)) return double.NaN;
            if (povLine.number == 0) return ActionError(action, $"Index cannot be calculated, because poverty line is zero");

            GetBoolParameterValue(PAR_USE_SWITCH_APPROACH, action, out bool useSwitchApproach);

            int subGroupFilterIndex = -1;
            if (action.filter != null)
            {
                Template.Action filterAction = new Template.Action()
                {
                    _calculationLevel = action.CalculationLevel,
                    calculationType = HardDefinitions.CalculationType.CreateFlag,
                    filter = action.filter,
                    outputVar = "flag_" + Guid.NewGuid().ToString()
                };
                CreateFlag(data, filterAction);
                subGroupFilterIndex = data[action.CalculationLevel].GetVarIndex(filterAction.outputVar);
                if (subGroupFilterIndex == -1) return ActionError(action, "Couldn't calculate filter for Poverty Gap");
            }
            Template.Filter poorFilter = new Template.Filter(data[action.CalculationLevel], x => x[incIndex] < povLine.number && (subGroupFilterIndex == -1 || x[subGroupFilterIndex] == 1));

            double povGap;
            if (useSwitchApproach)
            {
                if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName)) return double.NaN;
                double denominator = data[action.CalculationLevel].GetData(poorFilter.GetFunc(data[action.CalculationLevel])).
                    Sum(x => x[wgtVarIndex] * (povLine.number - x[incIndex]) / povLine.number); // = weighted Sum { (povLine - income poor unit) / povLine }
                double counter = data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel])).Sum(x => x[wgtVarIndex]); // weighted number of units (in possible sub-group)
                povGap = counter == 0 ? double.NaN : denominator / counter;
            }
            else
            {
                Template.Action medAction = new Template.Action()
                {
                    _calculationLevel = action.CalculationLevel,
                    calculationType = HardDefinitions.CalculationType.CalculateMedian,
                    filter = poorFilter,
                    outputVar = "median_" + Guid.NewGuid().ToString(),
                    parameters = new List<Template.Parameter>() { new Template.Parameter() { name = PAR_INCOME, variableName = incVar } }
                };

                if (double.IsNaN(CalculateMedian(data, medAction, sdcDefinition, out int dummy)))
                    return ActionError(action, "Couldn't calculate the poor median for Poverty Gap");
                if (!data[medAction.CalculationLevel].HasSavedNumber(medAction.outputVar)) return ActionError(action, $"Index cannot be calculated, because assessing median failed (based on variable {incVar})");
                DataStatsHolder.SavedNumber median = data[medAction.CalculationLevel].GetSavedNumber(medAction.outputVar); // note that in a baseline-reform scenario this is the reform's median

                povGap = (povLine.number - median.number) / povLine.number;
            }

            sdcObsNo = CalculateSdcObsNo(sdcDefinition, incIndex, data[action.CalculationLevel].GetData(), data[action.CalculationLevel].GetData(action.filter?.GetFunc(data[action.CalculationLevel])));

            data[action.CalculationLevel].SetSavedNumber(action.outputVar, new DataStatsHolder.SavedNumber(povGap, sdcObsNo));
            return povGap;
        }

        // Scales are always automatically added to the individual and the action level datasets
        internal bool CreateOECDScale(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");
            // To support older templates, if there was no calculation level, assume HH
            string lvl = string.IsNullOrEmpty(action._calculationLevel) ? HH : action.CalculationLevel;
            if (data[IND].HasVariable(action.outputVar)) return true;   // if Individual data has this, all should have this
            GetBoolParameterValue(PAR_OXFORD, action, out bool Oxford);
            int ageIndex = data[IND].GetVarIndex(HardDefinitions.age);
            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(lvl));

            foreach (DataStatsHolder d in data.Values) d.AddVar(action.outputVar);

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

            if (data[IND].HasVariable(action.outputVar)) return true;   // if Individual data has this, all should have this
            
            Dictionary<Func<List<double>, bool>, double> bands = new Dictionary<Func<List<double>, bool>, double>();
            
            foreach (KeyValuePair<string, double> raw_band in raw_bands)
            {
                
                if (!template.globalFilters.Exists(x => x.name == raw_band.Key)) return false;
                Template.Filter filter = template.globalFilters.Find(x => x.name == raw_band.Key);
                if (!filter.HasFunc(data[IND])) return false;
                Func<List<double>, bool> f = filter.GetFunc(data[IND]);
                bands.Add(f, raw_band.Value);
            }

            bool replaceFirst = action.GetDoubleParameter(PAR_EQUIVFIRSTPERSON, out double firstPersonValue);

            // To support older templates, if there was no calculation level, assume HH
            string lvl = string.IsNullOrEmpty(action._calculationLevel) ? HH : action.CalculationLevel;

            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(lvl));

            data[IND].AddVar(action.outputVar);
            data[lvl].AddVar(action.outputVar);
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

            if (data[IND].HasVariable(action.outputVar)) return true;

            // TODO - consider how this could allow multiple variables (e.g. with prefix instead of outputvar)
            if (!action.GetUnnamedVariableParameter(out string varName))
                return ActionErrorBool(action, $"Variable to equivalise not defined (i.e. unnamed variable-Parameter missing)");
            int varIndex = data[IND].GetVarIndex(varName);
            if (varIndex < 0) return ActionErrorBool(action, $"{varName} not found");

            //  to support older templates, if calculation level is missing, use HH 
            string lvl = string.IsNullOrEmpty(action._calculationLevel) ? HH : action.CalculationLevel;

            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(lvl));

            data[IND].AddVar(action.outputVar);
            if (lvl != IND) data[lvl].AddVar(action.outputVar);
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

            // if no SumVar is defined, use OutputVar
            string sumVar = action.outputVar;
            int sumVarIndex = data[IND].GetVarIndex(sumVar);

            if (action.HasParameter(PAR_SUMVAR) && !GetVarParameterInfo(PAR_SUMVAR, data, action, out sumVarIndex, out sumVar, sumVar, true)) return ActionErrorBool(action, "No summing variable defined");

            GetBoolParameterValue(PAR_COPY_TO_INDIVIDUALS, action, out bool copyToInd);
            GetBoolParameterValue(PAR_COPY_TO_GROUP, action, out bool copyToGrp);

            bool addGroupVar = !data[action.CalculationLevel].HasVariable(action.outputVar);
            bool addIndVar = !data[IND].HasVariable(action.outputVar);

            if (addGroupVar) data[action.CalculationLevel].AddVar(action.outputVar);
            if (copyToInd && addIndVar) data[IND].AddVar(action.outputVar);
            int outIndIndex = copyToInd ? data[IND].GetVarIndex(action.outputVar) : 0;
            int outHHIndex = data[action.CalculationLevel].GetVarIndex(action.outputVar);
            int groupIndex = data[IND].GetVarIndex(template.info.GetCalculationLevelVar(action.CalculationLevel));

            var allGroups = from person in data[IND].GetData()
                        group person by person[groupIndex] into GRP
                        select GRP;
            foreach (var group in allGroups)
            {
                double totVar = copyToGrp ? group.First()[sumVarIndex] : group.Sum(x => x[sumVarIndex]);
                // add total to HH dataset
                if (addGroupVar) data[action.CalculationLevel].GetObs(group.ElementAt(0)[groupIndex]).Add(totVar);
                else data[action.CalculationLevel].GetObs(group.ElementAt(0)[groupIndex])[outHHIndex] = totVar;
                // add total to Ind dataset
                if (copyToInd)
                {
                    if (addIndVar) group.ToList().ForEach(x => x.Add(totVar));
                    else group.ToList().ForEach(x => x[outIndIndex] = totVar);
                }
            }
            return true;
        }

        /**
         * This function calculates the median for a given variable, at a given dataset. One can specify multiple facets...
         */
        private double CalculateMedian(Dictionary<string, DataStatsHolder> data, Template.Action action, Template.Page.Table.SDCDefinition sdcDefinition, out int sdcObsNo)
        {
            sdcObsNo = int.MaxValue;

            if (data[action.CalculationLevel].HasSavedNumber(action.outputVar))
            {
                DataStatsHolder.SavedNumber savedNumber = data[action.CalculationLevel].GetSavedNumber(action.outputVar);
                sdcObsNo = savedNumber.sdcObsNo; return savedNumber.number;
            }
                
            Func<List<double>, bool> filter = action.filter?.GetFunc(data[action.CalculationLevel]);
            if (!GetWeightParameterInfo(data, action, out int wgtVarIndex, out string wgtVarName, true) |
                !GetVarParameterInfo(PAR_INCOME, data, action, out int incIndex, out string incVar, null, true)) return ActionError(action, "Both Income and Weight parameters are required for calculating the Median.");

            double aggPeople = 0;
            double prevVar = Double.NegativeInfinity;
            double prevWeight = Double.NegativeInfinity;
            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, sdcDefinition, out sdcObsNo, null, filter);
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
            data[action.CalculationLevel].SetSavedNumber(action.outputVar, new DataStatsHolder.SavedNumber(prevVar, sdcObsNo));

            return prevVar;
        }

        /**
         * This function generates deciles for a given variable, at a given dataset. One can specify multiple facets...
         */
        private bool CreateDeciles(Dictionary<string, DataStatsHolder> data, Template.Action action)
        {
            if (string.IsNullOrEmpty(action.outputVar)) return ActionErrorBool(action, "No output variable defined");

            if (data[action.CalculationLevel].HasVariable(action.outputVar)) return true;
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
                data[IND].SetSavedNumber(GetVarNameCutOff(action.outputVar, co), new DataStatsHolder.SavedNumber(double.NaN, int.MaxValue));

            double dec = 1; double aggPeople = 0;
            double totalPeople = Sum(data, action.CalculationLevel, wgtVarName, new Template.Page.Table.SDCDefinition(), out int obsNoTotal, null, filter);
            if (totalPeople == 0) return ActionErrorBool(action, $"Index cannot be calculated, because population-count is zero (based on weight variable {wgtVarName})");
            double prevVar = Double.NegativeInfinity;
            double prevWeight = Double.NegativeInfinity;

            if (NoGrouping(action))
            {
                data[action.CalculationLevel].AddVar(action.outputVar); int obsNoQuant = 0;
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
                            data[action.CalculationLevel].SetSavedNumber(GetVarNameCutOff(action.outputVar, dec - 1),
                                                          new DataStatsHolder.SavedNumber(prevVar, obsNoTotal));
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
                data[action.CalculationLevel].AddVar(action.outputVar);
                if (writeToGroup) data[grpLvl].AddVar(action.outputVar);
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
                            data[action.CalculationLevel].SetSavedNumber(GetVarNameCutOff(action.outputVar, dec - 1),
                                                          new DataStatsHolder.SavedNumber(prevVar, obsNoTotal));
                            if (writeToGroup) data[grpLvl].SetSavedNumber(GetVarNameCutOff(action.outputVar, dec - 1),
                                                           new DataStatsHolder.SavedNumber(prevVar, obsNoTotal));
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
            string ident = string.Empty; // the longwinded ident-construction is just for finding an "identifier" that allows users finding the cause of the error
            if (action.calculationType != HardDefinitions.CalculationType.NA) ident += $"CalculationType={action.calculationType} ";
            if (!string.IsNullOrEmpty(action.outputVar)) ident += $"OutputVar={action.outputVar} ";
            if (!string.IsNullOrEmpty(action.formulaString))
            {
                string fs = action.formulaString.Length < 20 ? action.formulaString : action.formulaString.Substring(0, 20);
                ident += $"FormulaString={fs}{(fs.Length == action.formulaString.Length ? string.Empty : " ...")}";
            }
            errorCollector.AddError($"Error in Action {ident.Trim()}:{Environment.NewLine}{msg}.");

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
            varIndex = data[getINDindex ? IND : action.CalculationLevel].GetVarIndex(varName);
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
