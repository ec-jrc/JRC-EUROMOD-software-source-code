using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class FunStore : FunInSpineBase
    {
        internal const double UNITLOOP_VOID = 0.0000000000001;

        internal FunStore(InfoStore infoStore) : base(infoStore) { }

        internal class StoreVar
        {
            internal VarSpec origVar;
            internal VarSpec storeVar;
            internal double iteration = NOTAP; // if != NOTAP, store only if the loop is in the respective iteration (e.g. storeVar.name = yem_loop7)

            internal string level = null; // only relevant for UnitLoop
            internal ParIL unitLoopILPar = null;
        }
        internal const int NOTAP = -1; // not applicable

        internal string post = string.Empty;
        internal enum STORETYPE { FIX, LOOP, UNITLOOP } internal STORETYPE storeType = STORETYPE.FIX;
        internal List<StoreVar> vars = new List<StoreVar>();
        private int varIndexLoopCounter = NOTAP;
        private int varIndexCurElig = NOTAP; // only relevant for UnitLoop

        private Dictionary<string, string> ilLevels = new Dictionary<string, string>();
        private Dictionary<string, string> varLevels = new Dictionary<string, string>();
        private string generalLevel = null;

        protected override void PrepareNonCommonPar()
        {
            if (!PreparePostPar()) return;

            // this is for any related Restore, that needs to know what it should restore
            infoStore.operandAdmin.indexStoreFuns.Add(post, this);

            // level parameters are only relevant for UnitLoops (they are prepared here and "consumed" in RegisterUnitLoopOperands)
            PrepareLevelPar(out bool hasLevelPar);
            if (hasLevelPar && storeType != STORETYPE.UNITLOOP) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                message = $"{description.Get()}: Level parameters can only be used with UnitLoop (parameters are ignored)" });

            // now we would need to register all the variables that are to be produced by this Store (e.g. yem_backup, yse_loop3)
            // that's relativly easy if POSTFIX is used, and would be also solveable if POSTLOOP is used with a fixed number of iterations
            // but what if POSTLOOP is used with a break-condition?
            // thus do the following:

            // for POSTFIX and POSTLOOP: register the variables/ILs (and the variables in these ILs) with a known name,
            // e.g. yem_fix, yem_loop (that's the "running" var, taking up the current value inside the loop), ils_earns_fix, ils_earns_loop
            if (storeType != STORETYPE.UNITLOOP) RegisterOperands();
            else RegisterUnitLoopOperands(); // this is not completely different from the above, but it is confusing to use lots of ifs

            // for POSTLOOP (even with a fixed number of iterations) create the variables only "on demand",
            // i.e. if they are addressed
            // - via OperandAdmin.CheckRegistration, i.e. a formula uses e.g. yem_loop3:
            //   in this case OperandAdmin.CheckRegistration calls IsStoreOperand (see below and call in OperandAdmin.CheckRegistration)
            // - via Restore, if it addresses a specific iteration:
            //   in this case FunRestore calls RegisterOperands with the respective iteration
            // - by using (new) parameter LoopFromTo (see below):
            //   this makes in fact only sense if one wants to output a variable that is not used
            //   (i.e. already taken into account via OperandAdmin.CheckRegistration)
            //   CURRENTLY DEACTIVATED, because actually the old exe is even more restrictive in outputing store-variables
            //   one needs to use Var/IL and the full name to get even POSTFIX stuff, VarGroup with wildcards does not work at all
            // if (storeType != STORETYPE.FIX)
            //    foreach (ParBase parIteration in GetNonUniquePar<ParBase>(DefPar.Store.LOOP_FROM_TO))
            //    {
            //        if (!GetFromTo(parIteration, out List<int> iterations)) continue;
            //        foreach (int iteration in iterations) RegisterOperands(iteration);
            //    }
        }

        private bool GetFromTo(ParBase parFromTo, out List<int> iterations) // not activated (see above)
        {
            iterations = new List<int>(); string from = string.Empty, to = string.Empty;
            if (!parFromTo.xmlValue.Contains('-')) from = parFromTo.xmlValue;
            else
            {
                string[] ft = parFromTo.xmlValue.Split('-'); if (ft.Count() < 2) return SyntaxError();
                from = ft[0]; to = ft[1];
            }
            if (!int.TryParse(from, out int f)) return SyntaxError();
            int t = f; if (to != string.Empty && !int.TryParse(to, out t)) return SyntaxError();
            if (f > t) return SyntaxError($"from ({f}) > to ({t})");
            if (f <= 0) return SyntaxError($"from ({f}) must not be zero or negative");
            for (int i = f; i <= t; ++i) iterations.Add(i); return true;

            bool SyntaxError(string explanation = "correct syntax: e.g. 1-3")
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{parFromTo.description.Get()}: invalid range of loop-iterations: {explanation}" });
                return false;
            }
        }

        private bool PreparePostPar()
        {
            ParBase parPostLoop = GetUniquePar<ParBase>(DefPar.Store.PostLoop);
            ParBase parPostFix = GetUniquePar<ParBase>(DefPar.Store.PostFix);
            if (parPostLoop == null && parPostFix == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: neither {DefPar.Store.PostFix} nor {DefPar.Store.PostLoop} defined" });
                return false;
            }
            if (parPostLoop != null && parPostFix != null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: unclear specification - both {DefPar.Store.PostFix} and {DefPar.Store.PostLoop} defined" });
                return false;
            }
            if (parPostLoop != null)
            {
                post = parPostLoop.xmlValue;
                if (!infoStore.operandAdmin.indexLoopIDs.ContainsKey(post))
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{parPostLoop.description.Get()}: no loop with {DefPar.Loop.Loop_Id} = {post} found" });
                    return false;
                }
                storeType = infoStore.operandAdmin.indexLoopIDs[post] is FunUnitLoop ? STORETYPE.UNITLOOP : STORETYPE.LOOP;
            }
            else
            {
                post = parPostFix.xmlValue; storeType = STORETYPE.FIX;
                if (!EM_Helpers.IsValidName(post, out string illegal))
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{parPostFix.description.Get()}: usage of illegal character(s): {illegal}" });
                    return false;
                }
            }
            return true;
        }

        private void PrepareLevelPar(out bool hasLevelPar)
        {
            hasLevelPar = false;

            ParTU parLevel = GetUniquePar<ParTU>(DefPar.Store.Level);
            if (parLevel != null) { generalLevel = parLevel.name; hasLevelPar = true; }
            else if (storeType == STORETYPE.UNITLOOP) // default is the EligUnit of the UnitLoop
                generalLevel = (infoStore.operandAdmin.indexLoopIDs[post] as FunUnitLoop).GetEligUnit();

            foreach (ParTU par in GetNonUniquePar<ParTU>(DefPar.Store.IL_Level))
            {
                if (!GetGroup(par, out string group)) continue;
                if (ilLevels.ContainsKey(group)) DoubleDef(par);
                else ilLevels.Add(group, par.name); hasLevelPar = true;
            }
            foreach (ParTU par in GetNonUniquePar<ParTU>(DefPar.Store.Var_Level))
            {
                if (!GetGroup(par, out string group)) continue;
                if (varLevels.ContainsKey(group)) DoubleDef(par);
                else varLevels.Add(group, par.name); hasLevelPar = true;
            }

            void DoubleDef(ParTU par)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                    message = $"{par.description.Get()}: double definition (group {par.description.GetParGroup()}) is ignored" });
            }

            bool GetGroup(ParTU par, out string group)
            {
                group = par.description.GetParGroup();
                if (!string.IsNullOrEmpty(group)) return true;
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                    message = $"{par.description.Get()}: no group defined, parameter is ignored" });
                return false;
            }
        }

        /// <summary>
        /// register variables, ILs and the variables they contain  
        /// if iteration = NOTAP (internal call, see above): for POSTFIX (e.g. yem_backup) or the "running" variable (e.g. yem_loop)
        /// if iteration > 0 (call by FunRestore): for the respective iteration (e.g. yem_loop7)
        /// </summary>
        internal void RegisterOperands(int iteration = NOTAP) // note that the iteration is handled in ComposeStoreName
        {
            foreach (ParVar par in GetNonUniquePar<ParVar>(DefPar.Store.Var))
                RegisterVar(par.name, par.description, iteration);
            foreach (ParIL par in GetNonUniquePar<ParIL>(DefPar.Store.IL))
                RegisterILAndContent(par, iteration);
        }

        // note that (different from RegisterOperands above) there is only the internal call (for the "running" variables)
        // because Restore is not allowed to be used with UnitLoops
        // (the only other possibility for generating variables with a UnitLoop-Store is "on demand" (i.e. with IsStoreOperand)
        // which works exactly as for a Loop-Store)
        private void RegisterUnitLoopOperands()
        {
            List<string> usedGroups = new List<string>();
            foreach (ParVar par in GetNonUniquePar<ParVar>(DefPar.Store.Var))
            {
                string level = generalLevel, group = par.description.GetParGroup();
                if (varLevels.ContainsKey(group)) { level = varLevels[group]; usedGroups.Add(group); }
                RegisterVar(par.name, par.description, NOTAP, level);
            }
            CheckUnused(varLevels);

            usedGroups.Clear();
            foreach (ParIL par in GetNonUniquePar<ParIL>(DefPar.Store.IL))
            {
                string level = generalLevel, group = par.description.GetParGroup();
                if (ilLevels.ContainsKey(group)) { level = ilLevels[group]; usedGroups.Add(group); }
                // for a UnitLoop-IL the "running" item (e.g. ils_earns_unit) is a variable not an incomelist (see Run for details)
                RegisterVar(par.name, par.description, NOTAP, level, par);
            }
            CheckUnused(ilLevels);

            void CheckUnused(Dictionary<string, string> levels)
            {
                if (!infoStore.runConfig.warnAboutUselessGroups) return;    // report unused groups
                foreach (var g in levels.Keys)
                    if (!usedGroups.Contains(g)) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                        message = $"{description.Get()}: unused level parameter (group {g})" });
            }
        }

        private bool RegisterVar(string origVarName, Description description, int iteration, string level = null, ParIL unitLoopILPar = null)
        {
            string storeVarName = ComposeStoreName(origVarName, iteration);
            if ((from v in vars select v.storeVar.name).Contains(storeVarName))
                return true; // this can easily happen by storing ILs with overlapping content

            infoStore.operandAdmin.RegisterVar(
                name: storeVarName,
                creatorFun: DefFun.Store,
                description: description,
                isMonetary: infoStore.operandAdmin.GetIsMonetary(origVarName),
                isGlobal: false,
                isWriteable: false, // cannot be used as output variable
                setInitialised: true);

            if (!infoStore.operandAdmin.Exists(storeVarName)) return false; // registration failed
            vars.Add(new StoreVar()
            {
                origVar = new VarSpec() { name = origVarName },
                storeVar = new VarSpec() { name = storeVarName },
                iteration = iteration,
                level = level,
                unitLoopILPar = unitLoopILPar
            });
            return true;
        }

        // this needs to do what usually DefIL does and, in addition, create all the contained variables
        private void RegisterILAndContent(ParIL parIL, int iteration)
        {
            // first create the content: e.g. for ils_earns_loop3: yem_loop3, yse_loop3 ...
            Dictionary<string, double> content = new Dictionary<string, double>();
            foreach (var entry in parIL.GetFlatContent())
            {
                string origVarName = entry.varSpec.name;
                if (!RegisterVar(origVarName, parIL.description, iteration)) return;
                string storeName = ComposeStoreName(origVarName, iteration);
                // entry of new il: e.g. name: yem_loop3, factor: 1
                if (!content.ContainsKey(storeName)) content.Add(storeName, entry.addFactor); // the usual case
                else content[storeName] += entry.addFactor; // exception: e.g. il contains two ils, which share variables
            }

            // ... then do do what usually DefIL does, i.e. register the incomelist (e.g. ils_earns_loop3)
            // note: this IL differs from its original by being "flat", i.e. ils are resolved into their variables
            infoStore.operandAdmin.RegisterIL(
                name: ComposeStoreName(parIL.name, iteration),
                creatorFun: DefFun.Store,
                description: parIL.description,
                content: content,
                warnIfNonMon: false);
        }

        private string ComposeStoreName(string name, int iteration = NOTAP, string _post = null)
        {
            if (_post == null) _post = post;
            return $"{name}_{_post}{(iteration != NOTAP ? iteration.ToString() : string.Empty)}".ToLower();
        }

        /// <summary>
        /// called by OperandAdmin.CheckRegistration, i.e. on checking if a variable/IL, e.g. used in a formula, is valid
        /// checks if this Store-function could be possibly "responsible" for this variable/IL
        /// (e.g. ask for yem_loop3 - Store is responsible if POSTLOOP-parameter=loop and there is a VAR-parameter=yem)
        /// if responsible, registration takes place as described above
        /// </summary>
        internal bool IsStoreOperand(string testOperandName)
        {
            if (storeType == STORETYPE.FIX) return false; // variables of POSTFIX-Stores are not produced "on demand" (see explanation above)

            int iteration = NOTAP;
            foreach (ParVar par in GetNonUniquePar<ParVar>(DefPar.Store.Var))
            {
                if (CheckIfResponsibleAndGetIteration(par.name)) { RegisterVar(par.name, par.description, iteration); return true; }
            }
            foreach (ParIL par in GetNonUniquePar<ParIL>(DefPar.Store.IL))
            {
                // first check for IL itself, e.g. ils_earns_loop7
                if (CheckIfResponsibleAndGetIteration(par.name)) { RegisterILAndContent(par, iteration); return true; }
                // then check for the content, e.g. yem_loop7, yse_loop7
                foreach (string ilVar in ParIL.GetILComponents(par.name, infoStore.operandAdmin))
                    if (CheckIfResponsibleAndGetIteration(ilVar)) { RegisterVar(ilVar, par.description, iteration); return true; }
            }
            return false;

            bool CheckIfResponsibleAndGetIteration(string origName)
            {
                string storeName = ComposeStoreName(origName, NOTAP);
                return testOperandName.StartsWith(storeName) &&
                    int.TryParse(testOperandName.Substring(storeName.Length), out iteration);
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (StoreVar v in vars)
            {
                if (v.unitLoopILPar == null) v.origVar.index = infoStore.operandAdmin.GetIndexInPersonVarList(v.origVar.name);
                v.storeVar.index = infoStore.operandAdmin.GetIndexInPersonVarList(v.storeVar.name);

                if (storeType == STORETYPE.UNITLOOP && v.iteration == NOTAP)
                    infoStore.hhAdmin.GlobalSetVar(v.storeVar.index, UNITLOOP_VOID);
            }
            if (storeType != STORETYPE.FIX && infoStore.operandAdmin.Exists(DefVarName.LOOPCOUNT + post))
                varIndexLoopCounter = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.LOOPCOUNT + post);
            if (storeType == STORETYPE.UNITLOOP && infoStore.operandAdmin.Exists(DefVarName.UNITLOOP_IS_CUR_ELIG + post))
                varIndexCurElig = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.UNITLOOP_IS_CUR_ELIG + post);
        }

        // note that Store actually does not have a TAX_UNIT, thus this is the HHAdmin.DUMMY_INDIVIDUAL_TU (see FunInSpineBase.GetTUName)
        // that means, amongst others, that the function is called for each person in the household (i.e. tu always contains one person)
        internal override void Run(HH hh, List<Person> tu)
        {
            if (!IsRunCondMet(hh)) return;
            if (storeType != STORETYPE.UNITLOOP) RunLoopAndFix(hh, tu); else RunUnitLoop(hh, tu);
        }

        private void RunLoopAndFix(HH hh, List<Person> tu)
        {
            // remark: gets an "invalid" loop-counter (0/end of loop) if a loop-store is used outside a loop, but that's a developer error/decision
            double curIteration = varIndexLoopCounter == NOTAP ? NOTAP : hh.GetPersonValue(varIndexLoopCounter, 0);

            foreach (StoreVar v in vars)
            {
                if (v.iteration == NOTAP || // always stored (possibly overwritten): yem_loop, yem_backup, i.e. in each iteration or outside any loop
                    v.iteration == curIteration) // stored only if it is the appropriate iteration: yem_loop7
                    hh.SetTUValue(hh.GetTUValue(v.origVar.index, tu), v.storeVar.index, tu);
            }
        }

        private void RunUnitLoop(HH hh, List<Person> tu)
        {
            // store the "running" variables (yem_unit, ils_earns_unit), and take the level into account
            foreach (StoreVar v in vars)
            {
                if (v.iteration != NOTAP) continue; // is handled below
                if (hh.GetPersonValue(varIndexCurElig, tu[0].indexInHH) <= 0) continue; // store only for the (head of the) "currently elig" unit

                List<Person> altTU = hh.GetAlternativeTU(v.level, tu, description); // on may consider performance here, but let's first get it right
                double levVal = v.unitLoopILPar == null ? hh.GetTUValue(v.origVar.index, altTU) : v.unitLoopILPar.GetValue(hh, altTU);
                hh.SetPersonValue(levVal, v.storeVar.index, tu[0].indexInHH);
            }

            // store the "on demand" variables (e.g. yem_unit1) - they are stored the same way as for a normal loops (i.e. no level-stuff)
            foreach (StoreVar v in vars)
            {
                if (v.iteration != NOTAP && v.iteration == hh.GetPersonValue(varIndexLoopCounter, 0))
                    hh.SetTUValue(hh.GetTUValue(v.origVar.index, tu), v.storeVar.index, tu);
            }
        }

        internal bool IsProspectiveLoopOperand(string operand) // see InfoStore.IsProspectiveStoreOperand for description
        { // note that this can be called before (or after) PrepareNonCommonPar, therefore one cannot rely on variables (e.g. post, storeType, ...)
          // also note that we are only looking for the "running" operands, e.g. yem_loop, ils_earns_loop (i.e. without an iteration)
            ParBase parPostLoop = GetUniquePar<ParBase>(DefPar.Store.PostLoop);
            if (parPostLoop == null) return false; // most likely a PostFix-Store, thus not relevant

            foreach (ParVar par in GetNonUniquePar<ParVar>(DefPar.Store.Var))
                if (ComposeStoreName(par.xmlValue, NOTAP, parPostLoop.xmlValue).ToLower() == operand.ToLower()) return true;

            foreach (ParIL par in GetNonUniquePar<ParIL>(DefPar.Store.IL))
            {
                // first check for IL itself, e.g. ils_earns_loop
                string ili = ComposeStoreName(par.xmlValue, NOTAP, parPostLoop.xmlValue);
                if (ComposeStoreName(par.xmlValue, NOTAP, parPostLoop.xmlValue).ToLower() == operand.ToLower()) return true;
                // then check for the content, e.g. yem_loop, yse_loop
                if (!infoStore.operandAdmin.Exists(par.xmlValue)) continue; // only check if il is already registered
                foreach (string ilVar in ParIL.GetILComponents(par.xmlValue, infoStore.operandAdmin))
                    if (ComposeStoreName(ilVar, NOTAP, parPostLoop.xmlValue).ToLower() == operand.ToLower()) return true;
            }
            return false;
        }
    }
}
