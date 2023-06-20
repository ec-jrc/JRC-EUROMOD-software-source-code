using EM_Common;
using System.Collections.Generic;
using System;
using System.Linq;

namespace EM_Executable
{
    internal class FunUprate : FunInSpineBase
    {
        internal FunUprate(InfoStore infoStore) : base(infoStore) { }

        private class UpVar
        {
            internal Description description;
            internal const double ALLYEARS = double.MaxValue;
            internal VarSpec varSpec = null;
            internal ParCond parCondition = null;
            // If DBYearVar is used, then we need to store all possible factors for each variable
            // otherwise, we store a single factor, with key ALLYEARS
            internal Dictionary<double, double> factors = new Dictionary<double, double>();
            internal double GetFactor(double year)
            {
                if (factors.ContainsKey(year)) return factors[year];
                if (factors.ContainsKey(ALLYEARS)) return factors[ALLYEARS];
                return 1;
            }
        }

        private class AggUpVar
        {
            internal VarSpec varSpec = null;
            internal List<VarSpec> parts = new List<VarSpec>();
            internal double tolerance = 0.1;
        }

        private List<UpVar> upVars = new List<UpVar>();
        private List<AggUpVar> aggUpVars = new List<AggUpVar>();
        private List<UpVar> upRegExp = new List<UpVar>(); // key: pattern, value: factor(name)
        private Dictionary<double, double> defaultFactor = new Dictionary<double, double>() { { UpVar.ALLYEARS, 1.0 } };
        private bool warnIfNoFactor = true;
        private bool warnIfNonMonetary = true;
        private bool isDBYearVarUsed = false;
        private int indDBYearVar = -1;
        private string DBYearVar;

        private List<string> zeroFactorSysCollector = new List<string>();
        private List<string> zeroFactorDataCollector = new List<string>();

        protected override void PrepareNonCommonPar()
        {
            if (!IsRunCondMet()) return;

            infoStore.applicableUprateFunctions.Add(description); // to allow for checking if there is no/more than one applicable Uprate

            // get the "function-internal" (rather outdated) factor definitions, i.e. those defined by parameters Factor_Name/Factor_Value
            Dictionary<string, double> internalFactorDefs = GetInternalFactorDefs();

            // check if DBYearVar exists and handle accordingly
            ParVar parDBYearVarPar = GetUniquePar<ParVar>(DefPar.Uprate.DBYearVar);
            isDBYearVarUsed = parDBYearVarPar != null && parDBYearVarPar.name != DefPar.Value.NA;
            if (isDBYearVarUsed)
            {
                if (parDBYearVarPar.name.Trim() != string.Empty)
                {
                    infoStore.operandAdmin.CheckRegistration(name: parDBYearVarPar.name.Trim(), isOutVar: false,
                                         warnIfNotInit: false, description: parDBYearVarPar.description);
                    if (!infoStore.operandAdmin.GetVarExistsInPersonVarList(parDBYearVarPar.name))
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{parDBYearVarPar.description.Get()}: Parameter {DefPar.Uprate.DBYearVar} was used with value '{parDBYearVarPar.name}', but this variable is not defined" });
                    else
                        DBYearVar = parDBYearVarPar.name;
                }
                else
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{this.description.Get()}: Parameter {DefPar.Uprate.DBYearVar} was used and left empty" });
            }

            // (1) "NORMAL" VARIABLES:
            // an uprating-group consists of the variable to uprate (e.g. yem=$f_cpi) and optionally of a condition (e.g. Factor_Cond={dgn=0})
            foreach (var group in GetParGroups(DefPar.Uprate.GROUP_MAIN).Values)
            {
                // optional condition parameter
                ParCond parCond = GetUniqueGroupPar<ParCond>(DefPar.Uprate.Factor_Condition, group);

                // compulsory var-name/factor-parameter (e.g. yem (in policy-column) / $f_cpi (in system-column))
                // usually this will be one placeholder-parameter but it is possible that one condition applies to several variables
                List<ParBase> listParMain = GetPlaceholderGroupPar<ParBase>(group);
                if (listParMain.Count == 0)
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{parCond.description.Get()}: loose condition {parCond.xmlValue}" });
                    continue;
                }
                foreach (ParBase parMain in listParMain)
                {
                    string upVarName = parMain.description.GetParName(); // get the name of the variable to uprate (e.g. yem)
                                                                         // check for double uprating, but take care of conditions, e.g. twice yem-uprating without condition -> warning, yem-uprating different for men and women -> ok
                    string condition = parCond == null ? "" : parCond.xmlValue;
                    bool exists = ExistsUpVar(upVarName, condition, out string reason);
                    if (exists) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = true, message = $"{parMain.description.Get()}: {reason}" });
                    Dictionary<double, double> upFactors = GetFactors(internalFactorDefs, parMain);

                    upVars.Add(new UpVar()
                    {
                        varSpec = new VarSpec() { name = upVarName },
                        factors = upFactors,
                        parCondition = parCond,
                        description = parMain.description
                    });

                    // the main purpose of this registration is to ensure the variable exists (an error is issued if not)
                    // in fact uprated variables are likely to be registered by usage (why uprate a variable that is not used?)
                    // (it could still be that an uprated but otherwise unused variable should be in the output and would not if VarGroup is used)
                    infoStore.operandAdmin.CheckRegistration(name: upVarName, isOutVar: false,
                                                             warnIfNotInit: false, description: parMain.description);
                }
            }

            // check for double conditioned uprating
            foreach (UpVar condUpVar in from cuv in upVars where cuv.parCondition != null select cuv)
                if ((from cuv in upVars 
                     where upVars.IndexOf(cuv) > upVars.IndexOf(condUpVar) && cuv.varSpec.name.ToLower() == condUpVar.varSpec.name.ToLower() && 
                           cuv.parCondition != null && cuv.parCondition.description.GetParGroup() == condUpVar.parCondition.description.GetParGroup()
                     select cuv).Count() > 0)
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = true, message = $"{condUpVar.parCondition.description.Get()}: double uprating of variable {condUpVar.varSpec.name}" });

            // (2) AGGREGATE VARIABLES:
            // the group consists of AggVar_Name (the agg-var to uprate) and a list of AggVar_Part (the vars summing up to the agg-var)
            // and optionally of AggVar_Tolerance (a tolerance for the check if part-vars actually sum up to agg-var)
            foreach (var group in GetParGroups(DefPar.Uprate.GROUP_AGG).Values)
            {
                ParVar parName = GetUniqueGroupPar<ParVar>(DefPar.Uprate.AggVar_Name, group);
                if (parName == null) continue; // error is issued by general checking

                bool exists = ExistsUpVar(parName.name, "", out string _);
                if (exists) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = true, message = $"{parName.description.Get()}: double uprating of variable {parName.name} (aggregate and normally)" });

                ParNumber parTol = GetUniqueGroupPar<ParNumber>(DefPar.Uprate.AggVar_Tolerance, group);

                AggUpVar aggUpVar = new AggUpVar() { varSpec = new VarSpec() { name = parName.name } };
                if (parTol != null) aggUpVar.tolerance = parTol.GetValue();

                foreach (ParVar parPart in GetNonUniqueGroupPar<ParVar>(DefPar.Uprate.AggVar_Part, group))
                    aggUpVar.parts.Add(new VarSpec() { name = parPart.name });

                if (aggUpVar.parts.Count > 0) aggUpVars.Add(aggUpVar);
            }

            // (3) VARIABLES DEFINED BY REGULAR EXPRESSION (e.g. for updating expenditure variables)
            foreach (var group in GetParGroups(DefPar.Uprate.GROUP_REGEXP).Values)
            {
                ParCond parCond = GetUniqueGroupPar<ParCond>(DefPar.Uprate.RegExp_Condition, group);
                ParBase parDef = GetUniqueGroupPar<ParBase>(DefPar.Uprate.RegExp_Def, group);
                ParBase parFactor = GetUniqueGroupPar<ParBase>(DefPar.Uprate.RegExp_Factor, group);
                if (parDef == null || parFactor == null) continue;
                upRegExp.Add(new UpVar() { varSpec = new VarSpec() { name = parDef.xmlValue }, factors = GetFactors(internalFactorDefs, parFactor), parCondition = parCond, description = parDef.description });
            }

            // get default factor ...
            ParBase parDefFac = GetUniquePar<ParBase>(DefPar.Uprate.Def_Factor);
            if (parDefFac != null) defaultFactor = GetFactors(internalFactorDefs, parDefFac);

            // ... and the optional parameter WarnIfNoFactor
            warnIfNoFactor = GetParBoolValueOrDefault(DefFun.Uprate, DefPar.Uprate.WarnIfNoFactor);

            // ... and the optional parameter WarnIfNonMonetary
            warnIfNonMonetary = GetParBoolValueOrDefault(DefFun.Uprate, DefPar.Uprate.WarnIfNonMonetary);

            if (zeroFactorSysCollector.Count > 0) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                message = $"{description.Get()}: uprating-index for system year is 0 for {string.Join(", ", zeroFactorSysCollector)}, resulting in uprating concerned variables by 0." });
            if (zeroFactorDataCollector.Count > 0) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                message = $"{description.Get()}: uprating-index for data year is 0 for {string.Join(", ", zeroFactorDataCollector)}, resulting in setting concerned variables to NaN." });
        }

        internal override void ReplaceVarNameByIndex()
        {
            // add the variables defined by regular expression (e.g. expenditure variables)
            foreach (UpVar regExp in upRegExp)
            {
                foreach (string matchVar in infoStore.operandAdmin.GetMatchingVar(pattern: regExp.varSpec.name, regExpr: true))
                {
                    // if it exists: as non-conditioned, or this RegExp is non-conditioned, or this specific condition already exists
                    string condition = regExp.parCondition == null ? "" : regExp.parCondition.xmlValue;
                    bool exists = ExistsUpVar(matchVar, condition, out string reason);
                    if (exists)
                    { 
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() {
                            isWarning = true,
                            message = $"{regExp.description.Get()}:'{reason} This factor will be ignored.'"
                        });
                        continue; // do not add if already otherwise uprated and this is not a conditioned uprate
                    }
                    upVars.Add(new UpVar() { varSpec = new VarSpec() { name = matchVar }, factors = regExp.factors, parCondition = regExp.parCondition });
                }
            }

            // remove and/or add DBYearVar as appropriate to avoid warnings
            if (isDBYearVarUsed)
            {
                indDBYearVar = infoStore.operandAdmin.GetIndexInPersonVarList(DBYearVar);

                // make sure DBYearVar was not added by accident in the uprating 
                int pos = upVars.FindIndex(x => x.varSpec.name == DBYearVar);
                if (pos > -1) upVars.RemoveAt(pos);

                // if the DBYearVar is incorrectly set as monetary, then add this variable with uprating 1 to avoid warnings and default uprating!
                if (infoStore.operandAdmin.GetIsMonetary(DBYearVar))
                {
                    upVars.Add(new UpVar()
                    {
                        varSpec = new VarSpec() { name = DBYearVar },
                        factors = new Dictionary<double, double>() { { UpVar.ALLYEARS, 1.0 } },
                        parCondition = null
                    });
                }
            }

            // uprating by default-factor:
            // this happens for variables which do not have an explicit factor (within this function!)
            // the concerned variables are those which are read from file
            // a warning is issued if such a default-uprating occurs (unless switched off by setting WarnIfNoFacor=no)
            string noFac = string.Empty;
            foreach (string v in infoStore.operandAdmin.GetVarsToUprate())
            {
                if (ExistsUpVar(v, "", out string _)) continue;
                if ((from auv in aggUpVars where auv.varSpec.name.ToLower() == v.ToLower() select auv).Count() > 0) continue;
                upVars.Add(new UpVar() { varSpec = new VarSpec() { name = v }, factors = defaultFactor });
                if (warnIfNoFactor) noFac += v + ", ";
            }
            if (noFac != string.Empty) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                message = $"{description.Get()}: variable(s) " + noFac.TrimEnd(new char[] { ',', ' ' }) + $" is/are uprated with default factor ({defaultFactor.First().Value})" });

            // the usual ReplaceVarNameByIndex actions
            base.ReplaceVarNameByIndex();
            foreach (UpVar uv in upVars) uv.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(uv.varSpec.name);
            foreach (AggUpVar auv in aggUpVars)
            {
                auv.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(auv.varSpec.name);
                foreach (VarSpec pv in auv.parts) pv.index = infoStore.operandAdmin.GetIndexInPersonVarList(pv.name);
            }

            if (warnIfNonMonetary) CheckForNonMon();
        }

        private void CheckForNonMon()
        {
            List<string> varNames = (from v in upVars select v.varSpec.name).ToList();
            varNames.AddRange((from v in aggUpVars select v.varSpec.name).ToList());
            foreach (string varName in varNames)
                if (!infoStore.operandAdmin.GetIsMonetary(varName))
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: uprating of non-monetary variable {varName}" });
        }

        internal override void Run(HH hh, List<Person> tu) // is called for each person in HH, i.e. with individual TU
        {
            if (!IsRunCondMet(hh)) return;

            // 1st step of uprating aggregates: store original values
            Dictionary<int, List<double>> orParts = new Dictionary<int, List<double>>(); // store the original parts ...
            Dictionary<int, double> sumParts = new Dictionary<int, double>(); // ... and the actual sum of the parts (which may differ from the aggregate, hopefully not by much)
            foreach (AggUpVar auv in aggUpVars)
            {
                double agg = hh.GetPersonValue(auv.varSpec.index, tu[0].indexInHH);
                double sumP = 0.0;
                foreach (VarSpec pv in auv.parts)
                {
                    double p = hh.GetPersonValue(pv.index, tu[0].indexInHH); sumP += p;
                    if (!orParts.ContainsKey(auv.varSpec.index)) orParts.Add(auv.varSpec.index, new List<double>());
                    orParts[auv.varSpec.index].Add(p);
                }
                //sumParts.Add(auv.varSpec.index, sumP); // first thought that this approach is more accurate ...
                sumParts.Add(auv.varSpec.index, agg);    // ... but the old exe uses this
                if (Math.Abs(agg - sumP) > auv.tolerance) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    runTimeErrorId = description.funID,
                    message = $"{description.Get()}: idperson {infoStore.GetIDPerson(hh, tu[0].indexInHH)}: parts of {auv.varSpec.name} do not sum up {agg} <> {sumP}"
                });
            }

            List<string> failed = new List<string>();
            List<string> dble = new List<string>();
            List<string> done = new List<string>();
            // uprating "normal" variables
            foreach (UpVar uv in upVars)
            {
                if (uv.parCondition == null || // either there is no condition or the condition is fulfilled
                    uv.parCondition.GetPersonValue(hh, tu[0]))
                {
                    hh.SetPersonValue(hh.GetPersonValue(uv.varSpec.index, tu[0].indexInHH) * uv.GetFactor(isDBYearVarUsed ? hh.GetPersonValue(indDBYearVar, tu[0].indexInHH) : UpVar.ALLYEARS),
                                                        uv.varSpec.index, tu[0].indexInHH);
                    if (!done.Contains(uv.varSpec.name)) done.Add(uv.varSpec.name);
                    else dble.Add(uv.varSpec.name);
                    if (failed.Contains(uv.varSpec.name)) failed.Remove(uv.varSpec.name);
                }
                else
                {
                    if (!failed.Contains(uv.varSpec.name) && !done.Contains(uv.varSpec.name)) failed.Add(uv.varSpec.name);
                }
            }
/*            if (failed.Count > 0)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true, 
                    runTimeErrorId = description.funID,
                    message = $"{description.Get()}: idperson {infoStore.GetIDPerson(hh, tu[0].indexInHH)}: the following var(s) were not uprated because they didn't match any of the conditions: {string.Join(", ", failed)}"
                });
            }
            if (dble.Count > 0)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    runTimeErrorId = description.funID,
                    message = $"{description.Get()}: idperson {infoStore.GetIDPerson(hh, tu[0].indexInHH)}: the following var(s) were uprated more than once because they matched multiple conditions: {string.Join(", ", dble)}"
                });
            }
*/            // 2nd step of uprating aggregates: build the factor of each aggregate variable as
            // factor_part1 * share_part1 + ... + factor_partN * share_partN =
            // = (part1_new / part1_old) * (part1_old / sum_parts_old) + ... + (partN_new / partN_old) * (partN_old / sum_parts_old)
            foreach (AggUpVar auv in aggUpVars)
            {
                if (sumParts[auv.varSpec.index] == 0) continue; // avoid division by zero
                double agg = hh.GetPersonValue(auv.varSpec.index, tu[0].indexInHH); double factor = 0;
                for (int i = 0; i < auv.parts.Count; ++i)
                {
                    if (orParts[auv.varSpec.index][i] == 0) continue; // avoid division by zero
                    double newPart = hh.GetPersonValue(auv.parts[i].index, tu[0].indexInHH);
                    factor += (newPart / orParts[auv.varSpec.index][i]) * (orParts[auv.varSpec.index][i] / sumParts[auv.varSpec.index]);
                }
                hh.SetPersonValue(agg * factor, auv.varSpec.index, tu[0].indexInHH);
            }
        }

        private Dictionary<double, double> GetFactors(Dictionary<string, double> internalFactorDefs, ParBase parFac)
        {
            Dictionary<double, double> allFactors = new Dictionary<double, double>();
            string upFactorS = parFac.xmlValue; // xmlValue contains the uprating instruction,
                                                // which may be a number (e.g. 1.4) or the name of a factor (e.g. $f_cpi)
            if (!double.TryParse(EM_Helpers.AdaptDecimalSign(upFactorS), out double upFactor)) // first try if a number ...
            {
                if (internalFactorDefs.ContainsKey(upFactorS)) // ... then try if a factor defined by Factor_Name/Factor_Value
                    allFactors.Add(UpVar.ALLYEARS, internalFactorDefs[upFactorS]);
                else if (infoStore.country.upFacs.ContainsKey(upFactorS)) // ... then try if a factor-name defined in dialog
                {
                    if (infoStore.country.upFacs[upFactorS].Get(infoStore.country.sys.year, out double sysInd))
                    {
                        if (sysInd == 0 && !zeroFactorSysCollector.Contains(upFactorS)) zeroFactorSysCollector.Add(upFactorS);
                        if (isDBYearVarUsed)
                        {
                            // Calculate the uprating factors
                            foreach (KeyValuePair<string, double> facs in infoStore.country.upFacs[upFactorS].GetAll())
                                if (double.TryParse(facs.Key, out double yr))
                                {
                                    if (facs.Value == 0 && !zeroFactorDataCollector.Contains(facs.Key)) zeroFactorDataCollector.Add(facs.Key);
                                    allFactors.Add(yr, sysInd / facs.Value);
                                }
                                else
                                {
                                    // This should only happen if a year cannot be parsed - This should NEVER happen!
                                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                                    {
                                        isWarning = false,
                                        message = $"{parFac.description.Get()}: invalid year '{facs.Key}' found in uprating factors!"
                                    });
                                    return allFactors;
                                }
                        }
                        else
                        {
                            if (infoStore.country.upFacs[upFactorS].Get(infoStore.country.data.year, out double dbInd))
                            {
                                if (dbInd == 0 && !zeroFactorDataCollector.Contains(upFactorS)) zeroFactorDataCollector.Add(upFactorS);
                                allFactors.Add(UpVar.ALLYEARS, sysInd / dbInd);
                            }
                            else
                            {
                                // Or keep 1 and return warning if any indices are missing
                                string dataInfo = string.IsNullOrEmpty(infoStore.country.data.year)
                                    ? $": income-year is missing for '{infoStore.country.data.Name}'"
                                    : $" for data year '{infoStore.country.data.year}'";
                                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                                message = $"{parFac.description.Get()}: insufficient definition of uprating-factor '{parFac.xmlValue}'{dataInfo} (1 is used as default)" });
                            }
                        }
                    }
                    else
                    {
                        // Or keep 1 and return warning if any indices are missing
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{parFac.description.Get()}: insufficient definition of uprating-factor '{parFac.xmlValue}' for system year '{infoStore.country.sys.year}' (1 is used as default)" });
                    }
                }
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = false,
                    message = $"{parFac.description.Get()}: unknown factor {parFac.xmlValue}"
                });
            }
            else
            {
                allFactors.Add(UpVar.ALLYEARS, upFactor);
            }
            return allFactors;
        }

        private Dictionary<string, double> GetInternalFactorDefs()
        {   // this is a somewhat simplified implementation, as I think it's an outdated concept and not used (and as such not worth the effort):
            // in the old executable the value of the factor can be a constant (and therewith also an uprating factor defined in the dialog)
            Dictionary<string, double> facDefs = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in GetParGroups(DefPar.Uprate.GROUP_FACTOR_DEF).Values)
            {
                ParBase pName = GetUniqueGroupPar<ParBase>(DefPar.Uprate.Factor_Name, group);
                ParNumber pNum = GetUniqueGroupPar<ParNumber>(DefPar.Uprate.Factor_Value, group);
                if (pName == null || pNum == null) continue; // error (compulsory param missing ...) is issued in the general check
                if (facDefs.ContainsKey(pName.xmlValue))
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    {
                        isWarning = true,
                        message = $"{description.Get()}: double definition of {DefPar.Uprate.Factor_Name} {pName.xmlValue}"
                    });
                else facDefs.Add(pName.xmlValue, pNum.GetValue()); // as said above, ignore the possibility of a constant
            }
            return facDefs;
        }

        private bool ExistsUpVar(string varName, string varCondition, out string description)
        {
            description = string.Empty;
            foreach (var uv in upVars)
            {
                if (uv.varSpec.name.ToLower() == varName.ToLower())
                {
                    string condition = (uv.parCondition != null) ? uv.parCondition.xmlValue : "";
                    string position = uv.description.pol.order + "." + uv.description.fun.order + "." + uv.description.par.order;

                    if (string.IsNullOrEmpty(condition))
                    {
                        description = $"Variable '{varName}' is already uprated in {position} without a condition.";
                        return true;
                    }
                    if (string.IsNullOrEmpty(varCondition))
                    {
                        description = $"Variable '{varName}' is already uprated in {position} with conditions. You cannot uprate wihout a condition.";
                        return true;
                    }
                    if (condition == varCondition)
                    {
                        description = $"Variable '{varName}' is already uprated in {position} with the same condition.";
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
