using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    internal class WorktimeRange
    {
        internal double rangeFrom = 0.0, rangeTo = 50.0, rangeStep = 0.5;
        internal double fixValue = 15.0;

        internal List<double> GetIncRange(WorktimeRangeAdmin.RangeType t)
        {
            List<double> rg = new List<double>();
            switch (t)
            {
                case WorktimeRangeAdmin.RangeType.RangeInc_FixHours:
                case WorktimeRangeAdmin.RangeType.RangeInc_FixWage:
                    for (double d = rangeFrom; d <= rangeTo; d += rangeStep) rg.Add(d); break;
                case WorktimeRangeAdmin.RangeType.RangeHours_FixWage:
                case WorktimeRangeAdmin.RangeType.RangeWage_FixHours:
                    for (double d = rangeFrom; d <= rangeTo; d += rangeStep) rg.Add(d * fixValue * 4.35); break;
            }
            return rg;
        }
    }

    internal class WorktimeRangeAdmin
    {
        // this needs consideration: first we may need different variables for some countries
        // a bigger problem (maybe not solvable) would be to include more variables that change in combination
        private const string varInc = "yem";
        private const string varHours = "lhw";
        private const string varWage = "yivwg";

        internal enum RangeType { None, RangeHours_FixWage, RangeWage_FixHours, RangeInc_FixHours, RangeInc_FixWage }

        private readonly RangeType rangeType;
        private readonly List<string> allYears;
        private readonly bool avertOtherRanges; // e.g. for Budget-Constraints-wizard we do not want combined ranges, e.g. by income AND age, ...
        private readonly List<string> reportOnlyHHTypes; // if true, do not report adaptations for any other HH-types (for avoiding unnecessary warnings)

        internal WorktimeRangeAdmin(RangeType _type, bool _avertOtherRanges, List<string> years, List<string> _reportOnlyHHType = null)
        {
            rangeType = _type; allYears = years; avertOtherRanges = _avertOtherRanges; reportOnlyHHTypes = _reportOnlyHHType;
        }

        private Dictionary<string, WorktimeRange> pWorktimeRanges = new Dictionary<string, WorktimeRange>();

        internal void AddPersonWorktimeRange(string hhTypeName, string pName, WorktimeRange worktimeRange) // in fact replace, if already defined
        {
            if (pWorktimeRanges.ContainsKey(Key(hhTypeName, pName))) pWorktimeRanges[Key(hhTypeName, pName)] = worktimeRange;
            else pWorktimeRanges.Add(Key(hhTypeName, pName), worktimeRange);
        }

        // technically we will always range income and keep either wage or hours fixed, while the otherone (i.e. hours or wage) is calculated, i.e. derived
        // that means:
        // - income (yem) stays in numericVars and, for the ranged persons, stays or is moved to rangeVarValues
        // - hours (lhw): if fixed stays in numericVars, if ranging, is moved to derivedVars to be derived from ranging yem and fixed yivwg
        // - wage (yivwg): if ranging stays in derivedVars, if fixed, is moved to numericVars and added to numericVarValues (with a number that keeps hours ok for not ranged persons)
        // note: if we actually also need RangeType.RangeHours_FixInc, RangeWage_FixInc, we need to avoid that yem is the derived variable,
        // as it used by other derived variables, and it seems currently not possible to dervive a variable from other dervived variables
        // (which is obviously also a problem for lhw and yivwg with non-standard HHoT definitions)
        internal void AdaptFileDetails(DataGenerator.FileGenerationDetails fgd)
        {
            try
            {
                reducedRangeVars.Clear(); // prepare the report for possibly set back ranges

                // adapt numericVars and derivedVars (if necessary) ...
                // most likely action: in case of Type.RangeHours_FixWage, move hours to derived and wage to numeric
                if (rangeType != RangeType.None)
                {
                    if (!fgd.numericVars.Contains(varInc)) fgd.numericVars.Add(varInc); // quite unlikely, that action necessary
                    if (fgd.derivedVars.ContainsKey(varInc)) fgd.derivedVars.Remove(varInc);

                    if (rangeType == RangeType.RangeHours_FixWage || rangeType == RangeType.RangeInc_FixWage) // in these cases most likely actions are necessary
                    {
                        if (!fgd.numericVars.Contains(varWage)) fgd.numericVars.Add(varWage);
                        if (fgd.derivedVars.ContainsKey(varWage)) fgd.derivedVars.Remove(varWage);
                        if (fgd.numericVars.Contains(varHours)) fgd.numericVars.Remove(varHours);
                        if (!fgd.derivedVars.ContainsKey(varHours)) fgd.derivedVars.Add(varHours, new DataGenerator.DerivedInfo()
                        {
                            varName = varHours,
                            defaultValue = "0",
                            conditionalValues = new Dictionary<string, string>() { { $"{varWage} > 0", $"yem / ({varWage} * 4.35)" } }
                        });
                    }
                    else // in these cases most likely there is nothing to do
                    {
                        if (!fgd.numericVars.Contains(varHours)) fgd.numericVars.Add(varHours);
                        if (fgd.derivedVars.ContainsKey(varHours)) fgd.derivedVars.Remove(varHours);
                        if (fgd.numericVars.Contains(varWage)) fgd.numericVars.Remove(varWage);
                        if (!fgd.derivedVars.ContainsKey(varWage)) fgd.derivedVars.Add(varWage, new DataGenerator.DerivedInfo()
                        {
                            varName = varWage,
                            defaultValue = "0",
                            conditionalValues = new Dictionary<string, string>() { { $"{varHours} > 0", $"yem / ({varHours} * 4.35)" } }
                        });
                    }
                }

                // ... then adapt rangeVarValues ...
                List<string> hhtNames = fgd.rangeVarValues.Keys.ToList();
                foreach (string hhtName in hhtNames)
                {
                    List<string> pNames = fgd.rangeVarValues[hhtName].Keys.ToList();
                    foreach (string pName in pNames)
                    {
                        List<string> varNames = fgd.rangeVarValues[hhtName][pName].Keys.ToList();

                        // first reduce any ranges to 1 number by moving them to numericVarValues (using the average value)
                        // do this for all variables if range-avert is defined, but anyway for the 3 variables concerned by the the worktime-range                        
                        foreach (string varName in varNames)
                        {
                            if (avertOtherRanges || varName == varInc || varName == varWage || varName == varHours)
                            {
                                fgd.numericVarValues[hhtName][pName].Add(varName, new Dictionary<string, double>());
                                foreach (string year in fgd.rangeVarValues[hhtName][pName][varName].Keys)
                                {
                                    double avgVal = fgd.rangeVarValues[hhtName][pName][varName][year].Average();
                                    fgd.numericVarValues[hhtName][pName][varName].Add(year, avgVal);
                                    string msg = $"'{varName}' for '{hhtName}' / '{pName}' to {avgVal}";
                                    if (!reducedRangeVars.Contains(msg) && (reportOnlyHHTypes == null || reportOnlyHHTypes.Contains(hhtName)))
                                        reducedRangeVars.Add(msg);
                                }
                                fgd.rangeVarValues[hhtName][pName].Remove(varName);
                            }
                        }

                        // if worktime-range is defined for this person: add range for income variable (yem)
                        if (pWorktimeRanges.ContainsKey(Key(hhtName, pName)))
                        {
                            fgd.rangeVarValues[hhtName][pName].Add(varInc, new Dictionary<string, List<double>>());
                            foreach (string year in allYears)
                                fgd.rangeVarValues[hhtName][pName][varInc].Add(year, pWorktimeRanges[Key(hhtName, pName)].GetIncRange(rangeType));
                        }
                    }
                }

                if (rangeType == RangeType.None) return; // type.None only means reducing range-values, thus we are done

                // ... finally adapt numericVarValues
                List<string> nhhtNames = fgd.numericVarValues.Keys.ToList();
                string varFix = rangeType == RangeType.RangeHours_FixWage || rangeType == RangeType.RangeInc_FixWage ? varWage : varHours;
                string varDer = varFix == varHours ? varWage : varHours;
                foreach (string hhtName in nhhtNames)
                {
                    List<string> pNames = fgd.numericVarValues[hhtName].Keys.ToList();
                    foreach (string pName in pNames)
                    {
                        List<string> varNames = fgd.numericVarValues[hhtName][pName].Keys.ToList();

                        if (!varNames.Contains(varFix)) fgd.numericVarValues[hhtName][pName].Add(varFix, new Dictionary<string, double>());

                        // for persons with worktime-range ...
                        if (pWorktimeRanges.ContainsKey(Key(hhtName, pName)))
                        {
                            // ... remove numeric income variable(yem) as it is now a range
                            if (varNames.Contains(varInc)) fgd.numericVarValues[hhtName][pName].Remove(varInc);

                            // ... set value of fix variable
                            foreach (string year in allYears)
                            {
                                if (!fgd.numericVarValues[hhtName][pName][varFix].ContainsKey(year)) fgd.numericVarValues[hhtName][pName][varFix].Add(year, 0);
                                fgd.numericVarValues[hhtName][pName][varFix][year] = pWorktimeRanges[Key(hhtName, pName)].fixValue;
                            }
                                
                        }
                        else // if no worktime-range is defined for this person ...
                        {
                            // ... the fix variable (e.g. wage) needs to take a value allowing for correct derivation of the calculated variable (e.g. hours)
                            foreach (string year in allYears)
                            {
                                double valueInc = varNames.Contains(varInc) && fgd.numericVarValues[hhtName][pName][varInc].ContainsKey(year)
                                                ? fgd.numericVarValues[hhtName][pName][varInc][year] : double.MaxValue;
                                double valueDer = varNames.Contains(varDer) && fgd.numericVarValues[hhtName][pName][varDer].ContainsKey(year)
                                                ? fgd.numericVarValues[hhtName][pName][varDer][year] : double.MaxValue;
                                if (!fgd.numericVarValues[hhtName][pName][varFix].ContainsKey(year)) fgd.numericVarValues[hhtName][pName][varFix].Add(year, 0);
                                if (valueInc != double.MaxValue && valueDer != double.MaxValue && valueDer != 0)
                                    fgd.numericVarValues[hhtName][pName][varFix][year] = valueInc / (valueDer * 4.35);
                            }
                        }

                        // for all persons: if necessary remove calculated var (lhw or yivwg) as it is now derived
                        if (varNames.Contains(varDer)) fgd.numericVarValues[hhtName][pName].Remove(varDer);
                    }
                }
            }
            catch (Exception e) { MessageBox.Show("Error in setting worktime/income ranges:" + Environment.NewLine + e.Message); }
        }

        internal bool HasReducedRanges(out List<string> info) { info = reducedRangeVars; return reducedRangeVars.Count > 0; }

        private List<string> reducedRangeVars = new List<string>();
        
        private static string Key(string hhtName, string pName) { return hhtName + "|" + pName; }
    }
}
