using EM_Common;
using System;
using System.Linq;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunTotals : FunOutOfSpineBase // this pseudo-code has a lot of room for improvment, it's just to show it could work
    {
        internal FunTotals(InfoStore infoStore) : base(infoStore) { }

        Dictionary<string, VarSpec> totals = new Dictionary<string, VarSpec>(); // key: sort of Total, e.g. Sum, Min (actually Varname_Sum)
        List<string> actions = new List<string>();      // the actions to be executed
        private List<ParVarIL> aggs = null;             // the list of variables to execute actions on - note: two params in EM2 (Agg_IL and Agg_Var), but Transformer merges them
        private Tuple<ParCond, string> include = null;
        private bool warnIfDuplicateDefinition = true;
        private int indexDwt = -1;
        private string dwtName;
        private bool useWeights = true;
        bool hasMedian = false;
        bool hasQuint = false;
        bool hasDec = false;
        bool hasSum = false;
        bool hasMean = false;

        protected override void PrepareNonCommonPar()
        {
            aggs = GetNonUniquePar<ParVarIL>(DefPar.Totals.Agg);
            if (aggs.Count == 0)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: missing required parameter AGG" });
                return;
            }

            // make sure the optional parameter warnIfDuplicateDefinition is parsed before registering operands! 
            warnIfDuplicateDefinition = GetParBoolValueOrDefault(DefFun.Totals, DefPar.Totals.WarnIfDuplicateDefinition);

            foreach (string t in new List<string> { DefPar.Totals.Varname_Sum, DefPar.Totals.Varname_Mean, DefPar.Totals.Varname_Median,
            DefPar.Totals.Varname_Decile, DefPar.Totals.Varname_Quintile, DefPar.Totals.Varname_Count, DefPar.Totals.Varname_PosCount,
            DefPar.Totals.Varname_NegCount, DefPar.Totals.Varname_Min, DefPar.Totals.Varname_Max })
            {
                ParBase par = GetUniquePar<ParBase>(t); if (par == null) continue;

                actions.Add(t);

                foreach (ParVarIL agg in aggs)
                {
                    string aggName = agg.GetName();
                    agg.CheckAndPrepare(this);
                    int n = 1; if (t == DefPar.Totals.Varname_Decile) n = 9; if (t == DefPar.Totals.Varname_Quintile) n = 4;
                    for (int i = 1; i <= n; ++i)
                    {
                        string counter = t == DefPar.Totals.Varname_Decile || t == DefPar.Totals.Varname_Quintile ? i.ToString() : string.Empty;
                        string varName = $"{par.xmlValue}{counter}_{aggName}";
                        infoStore.operandAdmin.RegisterVar( // generate the variable that will take the total
                            name: varName,
                            creatorFun: DefFun.Totals,
                            description: par.description,
                            isMonetary: false,  // not really clear, but adding over TU does not make sense
                            isGlobal: true,     // equal for each person
                            isWriteable: false, // cannot be use as output-variable
                            setInitialised: true,
                            warnForDuplicates: warnIfDuplicateDefinition);
                        if (infoStore.operandAdmin.Exists(varName))
                            totals.Add(t + (n > 1 ? i.ToString() : "") + aggName, new VarSpec() { name = varName });
                    }
                }
            }

            // setup booleans
            hasMedian = actions.Contains(DefPar.Totals.Varname_Median);
            hasQuint = actions.Contains(DefPar.Totals.Varname_Quintile);
            hasDec = actions.Contains(DefPar.Totals.Varname_Decile);
            hasSum = actions.Contains(DefPar.Totals.Varname_Sum);
            hasMean = actions.Contains(DefPar.Totals.Varname_Mean);

            // check if there is a condition for inclusion
            ParCond inclCond = GetUniquePar<ParCond>(DefPar.Totals.Incl_Cond);
            if (inclCond != null)
            {
                ParCateg inclWho = GetUniquePar<ParCateg>(DefPar.Totals.Incl_Cond_Who);
                string inclWhoVal = (inclWho == null) ? DefPar.Value.WHO_ALL : inclWho.GetCateg();
                include = new Tuple<ParCond, string>(inclCond, inclWhoVal);
            }

            // check if results should be weighted 
            useWeights = GetParBoolValueOrDefault(DefFun.Totals, DefPar.Totals.Use_Weights);
            if (useWeights) // get the weight var, if null use dwt
            {
                ParVar dwtPar = GetUniquePar<ParVar>(DefPar.Totals.Weight_Var);
                dwtName = dwtPar != null ? dwtPar.name : DefVarName.DWT;
                infoStore.operandAdmin.CheckRegistration(dwtName, false, false, description);
            }

            // 
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (var t in totals)
                t.Value.index = infoStore.operandAdmin.GetIndexInPersonVarList(t.Value.name);
            if (useWeights) indexDwt = infoStore.operandAdmin.GetIndexInPersonVarList(dwtName);
        }

        protected override void DoFunWork()
        {
            // prepare the results dictionary - note that some cases overlap and should only be calculated once
            Dictionary<string, double> results = new Dictionary<string, double>();
            Dictionary<string,List<List<double>>> valueList = new Dictionary<string, List<List<double>>>();
            double countObservations = 0;

            foreach (ParVarIL agg in aggs)
            {
                foreach (string key in actions)
                {
                    switch (key)
                    {
                        case DefPar.Totals.Varname_Sum:
                        case DefPar.Totals.Varname_Mean:
                        case DefPar.Totals.Varname_Count:
                        case DefPar.Totals.Varname_PosCount:
                        case DefPar.Totals.Varname_NegCount:
                        case DefPar.Totals.Varname_Median:
                            results.Add(key + agg.GetName(), 0.0);
                            break;
                        case DefPar.Totals.Varname_Decile:
                            for (int i = 1; i < 10; i++)
                                results.Add(key + i + agg.GetName(), 0.0);
                            break;
                        case DefPar.Totals.Varname_Quintile:
                            for (int i = 1; i < 5; i++)
                                results.Add(key + i + agg.GetName(), 0.0);
                            break;
                        case DefPar.Totals.Varname_Min:
                            results.Add(key + agg.GetName(), double.MaxValue);
                            break;
                        case DefPar.Totals.Varname_Max:
                            results.Add(key + agg.GetName(), double.MinValue);
                            break;
                    }
                }
                // mean also needs SUM, so add it if it does not already exist
                if (hasMean && !hasSum) results.Add(DefPar.Totals.Varname_Sum + agg.GetName(), 0.0);

                if (hasMedian || hasQuint || hasDec)
                    valueList.Add(agg.GetName(), new List<List<double>>());
            }

            // then go through each household and do the calculations
            foreach (HH hh in infoStore.hhAdmin.hhs)
            {
                foreach (List<Person> tu in hh.GetTUs(coParTU.name))
                {
                    if (include == null || FunInSpineBase.IsCondMetByTU(hh, tu, include.Item1, include.Item2))
                    {
                        double weight = (useWeights ? hh.GetPersonValue(indexDwt, tu[0].indexInHH) : 1);
                        countObservations += weight;
                        foreach (ParVarIL agg in aggs)
                        {
                            double tuValue = agg.GetValue(hh, tu);
                            double tuValueWeighted = tuValue * weight;
                            foreach (string key in actions)
                            {
                                string res = key + agg.GetName();
                                switch (key)
                                {
                                    case DefPar.Totals.Varname_Count:
                                        if (tuValue != 0) results[res] += weight;
                                        break;
                                    case DefPar.Totals.Varname_PosCount:
                                        if (tuValue > 0) results[res] += weight;
                                        break;
                                    case DefPar.Totals.Varname_NegCount:
                                        if (tuValue < 0) results[res] += weight;
                                        break;
                                    case DefPar.Totals.Varname_Min:
                                        if (tuValue < results[res]) results[res] = tuValue;
                                        break;
                                    case DefPar.Totals.Varname_Max:
                                        if (tuValue > results[res]) results[res] = tuValue;
                                        break;
                                }
                            }
                            if (hasSum || hasMean)
                                results[DefPar.Totals.Varname_Sum + agg.GetName()] += tuValueWeighted;
                            if (hasMedian || hasQuint || hasDec)
                                valueList[agg.GetName()].Add(new List<double>() { tuValue, weight });
                        }
                    }
                }
            }

            // do any final calculations required and get the final results
            foreach (ParVarIL agg in aggs)
            {
                // calculate the mean
                if (hasMean)
                    results[DefPar.Totals.Varname_Mean + agg.GetName()] = results[DefPar.Totals.Varname_Sum + agg.GetName()] / countObservations;
                
                // calculate the median and deciles
                if (hasMedian || hasQuint || hasDec)
                {
                    bool findMedian = hasMedian, findQuint = hasQuint, findDec = hasDec;

                    valueList[agg.GetName()] = valueList[agg.GetName()].OrderBy(p => p[0]).ToList();
                    double allObs = valueList[agg.GetName()].Sum(p => p[1]);
                    double halfObs = allObs / 2;
                    List<double> quints = new List<double>() { allObs / 5.0, 2.0 * allObs / 5.0, 3.0 * allObs / 5.0, 4.0 * allObs / 5.0 };
                    List<double> decs = new List<double>() { allObs / 10.0, 2.0 * allObs / 10.0, 3.0 * allObs / 10.0, 4.0 * allObs / 10.0, 5.0 * allObs / 10.0, 6.0 * allObs / 10.0, 7.0 * allObs / 10.0, 8.0 * allObs / 10.0, 9.0 * allObs / 10.0 };
                    int quintCoutner = 0, decCounter = 0;
                    double obsCounter = 0;
                    for (int i = 0; i < valueList[agg.GetName()].Count - 1; i++)        // Count-1 limit is to prevent overflow if there are too few observations...
                    {
                        obsCounter += valueList[agg.GetName()][i][1];

                        if (findMedian && obsCounter >= halfObs)
                        {
                            results[DefPar.Totals.Varname_Median + agg.GetName()] = valueList[agg.GetName()][i][0];
                            findMedian = false;
                            if (!findQuint && !findDec) break;
                        }
                        if (findQuint && obsCounter >= quints[quintCoutner])
                        {
                            results[DefPar.Totals.Varname_Quintile + (quintCoutner + 1) + agg.GetName()] = obsCounter > quints[quintCoutner]? valueList[agg.GetName()][i][0] : (valueList[agg.GetName()][i][0] + valueList[agg.GetName()][i + 1][0]) / 2;
                            quintCoutner++;
                            if (quintCoutner >= quints.Count) findQuint = false;
                            if (!findDec) break;
                        }
                        if (findDec && obsCounter >= decs[decCounter])
                        {
                            results[DefPar.Totals.Varname_Decile + (decCounter + 1) + agg.GetName()] = obsCounter > decs[decCounter] ? valueList[agg.GetName()][i][0] : (valueList[agg.GetName()][i][0] + valueList[agg.GetName()][i + 1][0]) / 2;
                            decCounter++;
                            if (decCounter >= decs.Count) break;
                        }
                    }
                }
            }

            // finally put the results in all individuals 
            foreach (HH hh in infoStore.hhAdmin.hhs)
                for (int i = 0; i < hh.GetPersonCount(); i++)
                    foreach (ParVarIL agg in aggs)
                        foreach (string key in actions)
                        {
                            if (key != DefPar.Totals.Varname_Decile && key != DefPar.Totals.Varname_Quintile)
                                hh.SetPersonValue(results[key + agg.GetName()], totals[key + agg.GetName()].index, i);
                            else
                            {
                                for (int q = 1; q <= (key == DefPar.Totals.Varname_Decile ? 9 : 4); ++q)
                                {
                                    string keyQ = key + q.ToString() + agg.GetName();
                                    hh.SetPersonValue(results[keyQ], totals[keyQ].index, i);
                                }
                            }
                        }
        }
    }
}
