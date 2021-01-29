using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class FunDefVar : FunInSpineBase
    {
        internal FunDefVar(InfoStore infoStore, bool _isConst) : base(infoStore) { isConst = _isConst; }

        private class VarDef
        {
            internal int groupNo = int.MinValue;
            internal VarSpec varSpec;
            internal ParFormula initFormula;
            internal ParCond condition = null;
        }

        private readonly bool isConst;

        // a Dictionary is only necessary because DefConst allows for Conditions (key: var-name, value: redundant varSpec + cond-formula-pairs)
        private Dictionary<string, List<VarDef>> varDefs = new Dictionary<string, List<VarDef>>(StringComparer.InvariantCultureIgnoreCase);

        protected override void PrepareNonCommonPar()
        {
            if (!IsRunCondMet()) // note that this works for info that is clearly available at read-time, e.g. {IsUsedDatabase} or {1}
                return;          // but simply returns true for global variables, which may (in theory) change over run-time
                                 // to handle the latter IsRunCond is called again in the Run-function (see below)
                                 // consequently for e.g. {0} the variables are not even generated
                                 // while for {$Const}, with $Const=0 at the point of request, the variables exist but are set to 0
                                 // this also reflects the behaviour of the old executable

            ParBase parSysYear = GetUniquePar<ParBase>(isConst ? DefPar.DefConst.Const_SystemYear : DefPar.DefVar.Var_SystemYear);
            if (parSysYear != null && !EM_Helpers.DoesValueMatchPattern(parSysYear.xmlValue, infoStore.country.sys.year)) return;

            ParBase parDataSet = GetUniquePar<ParBase>(isConst ? DefPar.DefConst.Const_Dataset : DefPar.DefVar.Var_Dataset);
            if (parDataSet != null && !infoStore.IsUsedDatabase(parDataSet.xmlValue)) return;

            foreach (var g in GetParGroups(isConst ? DefPar.DefConst.THE_ONLY_GROUP : DefPar.DefVar.THE_ONLY_GROUP))
            {
                List<ParBase> group = g.Value;

                // global variables (DefConst) allow for parameter Condition (but there is no reason why it wouldn't work for DefVar as well, thus no check)
                ParCond cond = GetUniqueGroupPar<ParCond>(DefPar.DefConst.Condition, group);

                List<ParFormula> initFormulas = GetPlaceholderGroupPar<ParFormula>(group);

                if (initFormulas.Count == 0) // there can be single Monetary parameters if definition is n/a for this system
                    continue;
                if (cond == null && initFormulas.Count > 1)
                { 
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{initFormulas[0].description.Get()}: multiple definition, group {g.Key} is already in use" });
                    continue;
                }

                foreach (ParFormula initFormula in initFormulas)
                {
                    string varName = initFormula.description.GetParName();
                    if (!EM_Helpers.IsValidName(varName, out string illegal))
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{initFormula.description.Get()}: name contains illegal character(s) '{illegal}'" });
                        continue;
                    }

                    if (isConst && cond == null && !initFormula.IsGlobal()) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{initFormula.description.Get()}: trying to initialise global variable with a non-global amount" });

                    bool isMonetary = false;
                    if (!isConst) // variables defined with DefConst cannot be monetary (more precisely, should not be added over TUs)
                    {
                        // if monetary-parameter is defined, use it, otherwise monetary=true (more precisely, get the default value from lib-def)
                        ParBool parMonetary = GetUniqueGroupPar<ParBool>(DefPar.DefVar.Var_Monetary, group);
                        isMonetary = parMonetary != null ? parMonetary.GetBoolValue()
                                   : DefinitionAdmin.GetParDefault<bool>(funName: DefFun.DefVar, parName: DefPar.DefVar.Var_Monetary);
                    }

                    if (!infoStore.operandAdmin.Exists(varName)) // double definition is not illegal (maybe questionable, but handled like this in old exe)
                    {
                        infoStore.operandAdmin.RegisterVar(name: varName, creatorFun: DefFun.DefVar, description: initFormula.description,
                            isMonetary: isMonetary,
                            isGlobal: isConst && cond == null,
                            isWriteable: true,     // DefVar defined variables can be used as output-var (this is a bit lax with DefConst)
                            setInitialised: true); // DefVar defined variables always have an initial value -
                                                   // if user sets it n/a it does not exist, otherwise she needs to assign a value
                                                   // actual initialisation is done in the Run function
                        if (!infoStore.operandAdmin.Exists(varName)) continue; // registration failed
                    }
                    else if (infoStore.operandAdmin.GetCreatorFun(varName) != DefFun.DefVar && infoStore.operandAdmin.GetCreatorFun(varName) != DefFun.DefConst)
                    {
                        infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                            message = $"{description.Get()} {varName}: {DefFun.DefVar}/{DefFun.DefConst} can only overwrite variables created by (another) {DefFun.DefVar}/{DefFun.DefConst}" });
                        continue;
                    }

                    // variable cannot yet be initialised, as at this stage the hh-data does not exist (no formula interpretation possible yet)
                    // note on still setting the variable initialised at this stage, in context with the "not-initialised" warning:
                    // parameters are prepared in the spine-order, thus parameters which are prepared before would get a "not known"
                    // and parameters prepared afterwards are "legal" users
                    if (!varDefs.ContainsKey(varName)) varDefs.Add(varName, new List<VarDef>()); // see definition of varDefs for reason for Dictionary
                    varDefs[varName].Add(new VarDef() { varSpec = new VarSpec() { name = varName }, initFormula = initFormula, condition = cond, groupNo = g.Key });
                }
            }

            // a variable can only be defined more than once within one function, if it is conditioned: 
            // only one definition can exist without a condition (which will be used as the default fallback value if conditions also exist)
            foreach (List<VarDef> condConstDef in varDefs.Values)
                if (condConstDef.Count > 1 && (from c in condConstDef where c.condition == null select c).Count() > 1)
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{description.Get()}: multiple definitions of {condConstDef[0].varSpec.name} without {DefPar.DefConst.Condition} found" });
        }

        private bool CheckValidVarName(string varName, string parDescription)
        {
            if (varName.Contains("#"))
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{parDescription}: character # cannot be used as variable name as it is reserved for footnotes" });
                return false;
            }
            return true;
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (List<VarDef> condGroup in varDefs.Values) foreach (VarDef vd in condGroup) vd.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(vd.varSpec.name);
        }

        internal override void Run(HH hh, List<Person> tu) // note: DefVar does not have a TAX_UNIT parameter
        {                                                  // therefore tu is DUMMY_INDIVIDUAL_TU, i.e. this is called for each person
            if (!IsRunCondMet(hh)) return; // see note in PrepareNonCommonPar

            // initialise the variables (see notes above)
            foreach (List<VarDef> condGroup in varDefs.Values)
            {
                double init = double.MaxValue;
                foreach (VarDef vd in condGroup.OrderBy(x => x.groupNo)) // make sure you preserve group order in applying conditions!
                {
                    if (vd.condition != null && !vd.condition.GetPersonValue(hh, tu[0])) continue;
                    init = vd.initFormula.GetValue(hh, tu); // usually this line would be enough for assessing the init-value, the rest is just for DefVar with Conditions
                }
                if (init == double.MaxValue)
                { 
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, runTimeErrorId = description.funID,
                        message = $"{description.Get()}: is initialised with zero for idperson={infoStore.GetIDPerson(hh, tu[0].indexInHH)}, because no suitable condition was found" });
                    init = 0;
                }
                hh.SetTUValue(init, condGroup[0].varSpec.index, tu);
            }
        }
    }
}
