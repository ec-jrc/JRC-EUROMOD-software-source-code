using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunLoop : FunOutOfSpineBase
    {
        internal FunLoop(InfoStore infoStore) : base(infoStore) { }

        internal string loopID = null;
        internal int indexFirstFun = -1, indexLastFun = -1;

        private ParCond breakCond = null;
        protected double nIterations = -1;
        protected int varIndexLoopCounter = -1;

        private class StartEnd
        {
            internal InfoStore infoStore; internal Description description;
            internal string ident; internal bool isFun, isNeighbour, isStart;

            internal bool IsMatch(List<FunBase> spineFuns, int indexFun, out int index)
            {
                index = -1;
                bool isMatch = (isFun && spineFuns[indexFun].description.funID.ToLower() == ident.ToLower()) ||
                               (!isFun && spineFuns[indexFun].description.pol.name.ToLower() == ident.ToLower());
                if (!isMatch) return false;

                if (isFun) // start/end identified by function-id
                {
                    if (!isNeighbour) { index = indexFun; return true; } // first_func / last_func
                    if (isStart) index = indexFun + 1; // start_after_func
                    else index = indexFun - 1; // stop_before_func
                }
                else // start/end identified by policy-name
                {
                    if (!isNeighbour && isStart) { index = indexFun; return true; } // first_pol
                    if (!isNeighbour && !isStart) { index = GetLastFunOfPol(); return true; } // last_pol
                    if (isNeighbour && isStart) index = GetLastFunOfPol() + 1; // start_after_pol
                    if (isNeighbour && !isStart) index = indexFun - 1; // stop_before_pol
                }
                // for the after/before parameters check whether there is no after/before available
                if (index < 0 || index >= spineFuns.Count) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: no function {(isStart ? "after" : "before")} {ident} available" });
                return true;

                int GetLastFunOfPol()
                {
                    int i;  for (i = indexFun + 1; i < spineFuns.Count; ++i) // search the last function of the policy
                        if (spineFuns[i].description.pol.name.ToLower() != ident.ToLower()) break;
                    return --i;
                }
            }
        }
        private StartEnd loopStart = null, loopEnd = null;

        internal override void ProvideIndexInfo()
        {
            ParBase par = GetUniquePar<ParBase>(DefPar.Loop.Loop_Id); if (par == null) return; // error is issued by standard-check
            loopID = par.xmlValue;
            if (!EM_Helpers.IsValidName(loopID, out string illegal))
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{par.description.Get()}: usage of illegal character(s): {illegal}" });
                return;
            }

            infoStore.operandAdmin.indexLoopIDs.Add(loopID, this); // used by Store-function, for checking if loop exists
            infoStore.operandAdmin.RegisterVar(name: DefVarName.LOOPCOUNT + loopID,
                                               creatorFun: DefFun.Loop,
                                               description: description,
                                               isMonetary: false,
                                               isGlobal: true, // actually it is not global, as this would not work in parallel evironment
                                                               // but by setting is Global to true, usage in RunCond is made possible
                                               isWriteable: false, // cannot be used as output-variable
                                               setInitialised: true);
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            if (loopID != null) varIndexLoopCounter = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.LOOPCOUNT + loopID);
        }

        protected override void PrepareNonCommonPar()
        {
            PrepareIterationPar();
            PrepareStartEndPar();
        }

        private void PrepareIterationPar()
        {
            // assess parameters which define the number of iterations
            breakCond = GetUniquePar<ParCond>(DefPar.Loop.BreakCond);
            if (breakCond == null)
            {
                ParNumber parIt = GetUniquePar<ParNumber>(DefPar.Loop.Num_Iterations);
                if (parIt != null) nIterations = (int)parIt.GetValue();
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: neither parameter {DefPar.Loop.Num_Iterations} nor {DefPar.Loop.BreakCond} defined" });
            }
            else
            {
                if (!breakCond.IsGlobal()) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{breakCond.description.Get()}: condition is supposed to be global but contains personal/household related operands" });
            }
        }

        protected void PrepareStartEndPar()
        {
            // assess parameters which define start and end of loop
            loopStart = GetStartEnd(DefPar.Loop.First_Pol, DefPar.Loop.First_Func, DefPar.Loop.Start_After_Pol, DefPar.Loop.Start_After_Func, true);
            loopEnd = GetStartEnd(DefPar.Loop.Last_Pol, DefPar.Loop.Last_Func, DefPar.Loop.Stop_Before_Pol, DefPar.Loop.Stop_Before_Func, false);

            StartEnd GetStartEnd(string parNamePol, string parNameFun, string parNameNeighbourPol, string parNameNeighbourFun, bool start)
            {
                ParBase parPol = GetUniquePar<ParBase>(parNamePol);
                ParBase parFun = GetUniquePar<ParBase>(parNameFun);
                ParBase parNeighbourPol = GetUniquePar<ParBase>(parNameNeighbourPol);
                ParBase parNeighbourFun = GetUniquePar<ParBase>(parNameNeighbourFun);

                StartEnd se = new StartEnd() { infoStore = infoStore, description = description, isStart = start } ; int defCnt = 0;
                if (parPol != null) { se.ident = parPol.xmlValue; se.isFun = false; se.isNeighbour = false; ++defCnt; }
                if (parFun != null) { se.ident = parFun.xmlValue; se.isFun = true; se.isNeighbour = false; ++defCnt; }
                if (parNeighbourPol != null) { se.ident = parNeighbourPol.xmlValue; se.isFun = false; se.isNeighbour = true; ++defCnt; }
                if (parNeighbourFun != null) { se.ident = parNeighbourFun.xmlValue; se.isFun = true; se.isNeighbour = true; ++defCnt; }
                
                string sse = parNamePol == DefPar.Loop.First_Pol ? "start" : "end";
                if (defCnt > 1) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: over-definition for loop {sse}" });
                if (defCnt == 0) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: missing definition for loop {sse}" });
                return se;
            }
        }

        internal bool GetRange(List<FunBase> spineFuns)
        {
            // find first and last function of the loop
            indexFirstFun = indexLastFun = -1;

            if (loopStart == null) return false; // happens if RunCond is never met (sth like RunCond={0})

            for (int f = 0; f < spineFuns.Count; ++f) if (loopStart.IsMatch(spineFuns, f, out indexFirstFun)) break;
            for (int f = 0; f < spineFuns.Count; ++f) if (loopEnd.IsMatch(spineFuns, f, out indexLastFun)) break;

            if (indexFirstFun == -1 && !HandleInaptRefs(spineFuns, loopStart)) infoStore.communicator.ReportError(new Communicator.ErrorInfo() {
                isWarning = false, message = $"{description.Get()}: could not find a match for {loopStart.ident}" });
            if (indexLastFun == -1 && !HandleInaptRefs(spineFuns, loopEnd)) infoStore.communicator.ReportError(new Communicator.ErrorInfo() {
                isWarning = false, message = $"{description.Get()}: could not find a match for {loopEnd.ident}" });
            if (indexLastFun < indexFirstFun) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                message = $"{description.Get()}: loop ends before it starts" });         

            return indexFirstFun != -1 && indexLastFun != -1 && indexLastFun >= indexFirstFun;
        }

        // give at least a useful error message if users try to use DefTU or DefIL functions or the policies containing them as reference
        // (the implementation tries to not change the existing code too much and therefore is a bit circuitous)
        private bool HandleInaptRefs(List<FunBase> spineFuns, StartEnd startEnd)
        {
            foreach (FunBase f in infoStore.spine.Values)
            {
                bool isDefTuIl = f.description.GetFunName().ToLower() == DefFun.DefIl.ToLower() || f.description.GetFunName().ToLower() == DefFun.DefTu.ToLower();
                bool isRefFun = startEnd.isFun && f.description.funID.ToLower() == startEnd.ident.ToLower();
                bool isRefPol = !startEnd.isFun && f.description.pol.name.ToLower() == startEnd.ident.ToLower();
                if (isDefTuIl && (isRefFun || isRefPol))
                {
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{description.Get()}: functions {DefFun.DefIl} and {DefFun.DefTu} and the policies containing them cannot be used as loop-start or -end" });
                    return true;
                }
            }
            return false; // another problem
        }

        internal virtual bool DoBreak(HH hh = null) // note: hh is only available in parallel runs (see setting counter below)
        {
            double iterationsDone = GetCounter(hh);
            if (IsFinished(hh, iterationsDone, true)) return true;
            SetCounter(hh, iterationsDone + 1); // increase loop-counter
            return false;
        }

        internal virtual bool DoJumpOver(bool init, HH hh = null) // note: hh is only available in parallel runs (see setting counter below)
        {
            if (!init) return false; // do nothing on jump-back, as the work was done at the loop-end: loop-counter was increased and checked
                                     // any (global) condition was checked and cannot have changed meanwhile
                                     // in fact updates are only necessary for UnitLoops, which override this function

            SetCounter(hh, 1); // initialise loop-counter
            return IsFinished(hh, 0, false); // check if the (global) conition is never fulfilled
        }

        private bool IsFinished(HH hh, double iterationsDone, bool checkRunCond)
        {
            if (checkRunCond && !IsRunCondMet(hh)) return true; // mimics the old exe, which does a do-while, i.e. check RunCond at end only

            if (nIterations == -1) return hh == null ? breakCond.GetGlobalValue() // loop is terminated by condition
                                                     : breakCond.GetPersonValue(hh, new Person(0)); // condition is global: any person is ok as 
            return iterationsDone >= nIterations; // loop has a fixed number of iteration
        }

        protected double GetCounter(HH hh)
        {
            if (hh == null) return infoStore.hhAdmin.GlobalGetVar(varIndexLoopCounter); // sequential run: get counter from 1st hh
            return hh.GetPersonValue(varIndexLoopCounter, 0); // parallel run: get counter from 1st person of the currently processed hh
        }

        protected void SetCounter(HH hh, double cnt)
        {
            if (hh == null) infoStore.hhAdmin.GlobalSetVar(varIndexLoopCounter, cnt); // sequential run: set counter for all households
            else // parallel run: set counter only for the currently processed household
                for (int personIndex = 0; personIndex < hh.GetPersonCount(); ++personIndex)
                    hh.SetPersonValue(cnt, varIndexLoopCounter, personIndex);
        }
    }
}
