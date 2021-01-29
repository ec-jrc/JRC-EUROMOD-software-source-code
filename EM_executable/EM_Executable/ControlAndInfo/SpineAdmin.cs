using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary> organises the order in which functions are called </summary>
    internal class SpineAdmin
    {
        private InfoStore infoStore = null;
        internal SpineAdmin(InfoStore _infoStore) { infoStore = _infoStore; }

        internal List<FunOutOfSpineBase> runBeforeSpine = new List<FunOutOfSpineBase>();
        internal List<FunOutOfSpineBase> runAfterSpine = new List<FunOutOfSpineBase>();
        private List<FunBase> runInSpine = new List<FunBase>();

        // these Dictionaries have as keys the index of the first respectively last function of a loop
        // this allows the SpineAdmin to check in LoopyGetNextFun if it has to communicate with the loop because it hit a loop's start or end
        // usually the value of the Dictionary would be just a FunLoop, but with nested loops there could be several loops with the same start/end
        private Dictionary<int, SortedList<double, FunLoop>> loopStarts = new Dictionary<int, SortedList<double, FunLoop>>();
        private Dictionary<int, SortedList<double, FunLoop>> loopEnds = new Dictionary<int, SortedList<double, FunLoop>>();

        internal void PrepareSpine(ref bool runSequential)
        {
            string BEFORE = "BEFORE", AFTER = "AFTER", IN = "IN", spineState = BEFORE;
            FunBase causeForSequential = null, prevFun = null;
            if (!runSequential)
            {
                foreach (FunBase fun in infoStore.spine.Values)
                {
                    switch (DefinitionAdmin.GetFunDefinition(fun.description.GetFunName()).runMode)
                    {
                        case DefFun.RUN_MODE.NOT_APPLICABLE: continue;
                        case DefFun.RUN_MODE.IN_SPINE:
                            if (spineState == AFTER) { runSequential = true; causeForSequential = prevFun; }
                            else { spineState = IN; runInSpine.Add(fun); }
                            break;
                        case DefFun.RUN_MODE.OUTSIDE_SPINE:
                            if (spineState == IN) spineState = AFTER;
                            if (spineState == AFTER) runAfterSpine.Add(fun as FunOutOfSpineBase);
                            else runBeforeSpine.Add(fun as FunOutOfSpineBase);
                            break;
                    }
                    if (runSequential) break; prevFun = fun;
                }
            }
            if (runSequential)
            {
                runBeforeSpine.Clear(); runAfterSpine.Clear(); runInSpine.Clear();
                foreach (FunBase fun in infoStore.spine.Values)
                    if (DefinitionAdmin.GetFunDefinition(fun.description.GetFunName()).runMode != DefFun.RUN_MODE.NOT_APPLICABLE)
                        runInSpine.Add(fun);
                string cause = causeForSequential == null ? "Forcing sequential" : $"{causeForSequential.description.Get()}: usage inside spine";
                infoStore.communicator.ReportProgress(new Communicator.ProgressInfo() { message = cause + " causes (slightly) slower run" });
            }

            // organise loop-handling (if there is/are any)
            foreach (FunBase fun in infoStore.spine.Values)
            {
                if (fun is FunLoop)
                {
                    FunLoop funLoop = fun as FunLoop;
                    if (!funLoop.GetRange(runInSpine) || IsLoopIntersection(funLoop)) continue;

                    // put loops into Dictionaries that allow LoopyGetNextFun to recognise that it needs to "talk" to the loops (when it hits start or end)
                    // if there are several loops with the same last function (nested loops) they are sorted reversly
                    // by their first function, in order to "ask" the inner loop first, if there should be a jump back
                    if (!loopEnds.ContainsKey(funLoop.indexLastFun)) loopEnds.Add(funLoop.indexLastFun, new SortedList<double, FunLoop>());
                    double iSort = funLoop.indexFirstFun; // if loops have the same start and end, make the first declared the outer
                    while (loopEnds[funLoop.indexLastFun].ContainsKey(iSort * (-1))) iSort += 0.001;
                    loopEnds[funLoop.indexLastFun].Add(iSort * (-1), funLoop);

                    // similar holds for loops with the same start, they need to be sorted reversly by their last function
                    // in order to ask the outer loop first, if there should be a jump over
                    if (!loopStarts.ContainsKey(funLoop.indexFirstFun)) loopStarts.Add(funLoop.indexFirstFun, new SortedList<double, FunLoop>());
                    iSort = funLoop.indexLastFun; // see above, wrt to loops with same start and end
                    while (loopStarts[funLoop.indexFirstFun].ContainsKey(iSort * (-1))) iSort -= 0.001;
                    loopStarts[funLoop.indexFirstFun].Add(iSort * (-1), funLoop);
                }
            }
        }

        private bool IsLoopIntersection(FunLoop newLoop)
        {
            foreach (var ol in loopStarts.Values) // note: newFirst<=newLast always holds, as this is checked by FunLoop.Init
                foreach (FunLoop oldLoop in ol.Values)
                {
                    if (newLoop.indexLastFun < oldLoop.indexFirstFun || newLoop.indexFirstFun > oldLoop.indexLastFun)
                        continue; // completely outside (no touch)
                    if (newLoop.indexFirstFun == oldLoop.indexFirstFun && newLoop.indexLastFun == oldLoop.indexLastFun)
                    {
                        //infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        //    message = $"{oldLoop.description.Get()} and {newLoop.description.Get()}: cannot determine the outer/inner loop" });
                        //return true;
                        continue; // this happens, thus needs to be possible - just take the first declared as the outer loop
                    }
                    if (newLoop.indexFirstFun >= oldLoop.indexFirstFun && newLoop.indexLastFun <= oldLoop.indexLastFun)
                        continue; // completely contained (inner loop)
                    if (newLoop.indexFirstFun <= oldLoop.indexFirstFun && newLoop.indexLastFun >= oldLoop.indexLastFun)
                        continue; // completely containing (outer loop)
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                        message = $"{oldLoop.description.Get()}: intersection with {newLoop.description.Get()}" });
                    return true;
                }
            return false;
        }

        internal FunBase GetNextFun(ref int curFunIndex, HH hh = null)
        {
            return loopStarts.Count == 0 ? SimpleGetNextFun(ref curFunIndex) : LoopyGetNextFun(ref curFunIndex, hh);
        }

        private FunBase SimpleGetNextFun(ref int curFunIndex)
        {
            if (curFunIndex < runInSpine.Count) return runInSpine[curFunIndex++];
            curFunIndex = 0; return null;
        }

        private FunBase LoopyGetNextFun(ref int curFunIndex, HH hh = null) // hh is only available in parallel-runs (for adapting the loopcounter)
        {
            FunLoop backSender = null;
            // the just run function was the last of a loop: increase loop-counter and check if a jump back to loop-start is required
            if (loopEnds.ContainsKey(curFunIndex - 1))
            {
                foreach (FunLoop loop in loopEnds[curFunIndex - 1].Values) // nested loops may have the same last function:
                {                                                          // inner loop is first asked for jump-back
                    if (!loop.DoBreak(hh)) // loop is not finished, i.e. jump back
                    {
                        curFunIndex = loop.indexFirstFun;
                        backSender = loop; // is required for loop-starts (see below)
                        break; // if inner loop requires jump-back - do not ask outer loop
                    }
                }
            }
            // the function to run next is the first of a loop: if no jump-back: init (e.g. loop-counter), if jump-back: update UnitLoop-vars
            // or (very rarly) jump-over to loop-end if required: this is actually only necessary if the loop-condition is never fulfilled
            if (loopStarts.ContainsKey(curFunIndex))
            {
                foreach (FunLoop loop in loopStarts[curFunIndex].Values) // nested loops may have the same first function
                {
                    bool init = backSender == null ||                 // do not init if this is a back-sending ...
                         backSender.indexLastFun > loop.indexLastFun; // ... unless the back-sender was an outer loop with the same start
                    if (loop.DoJumpOver(init, hh))
                    {
                        curFunIndex = loop.indexLastFun + 1;
                        break; // possible outer loop requires jump-over - no point to ask inner loop
                    }
                }
            }

            return SimpleGetNextFun(ref curFunIndex);
        }

        // returns the functions enclosed by a UnitLoop (key) and the respective UnitLoop(s) (value)
        // see Control_RunSequential.cs for usage
        internal Dictionary<FunBase, List<FunUnitLoop>> GetUnitLoopRangesForSequentialRun()
        {
            Dictionary<FunBase, List<FunUnitLoop>> ulRanges = new Dictionary<FunBase, List<FunUnitLoop>>();
            foreach (var l in loopStarts.Values)
                foreach (FunLoop funLoop in l.Values)
                {
                    if (!(funLoop is FunUnitLoop)) continue;
                    for (int i = funLoop.indexFirstFun; i <= funLoop.indexLastFun; ++i)
                    {
                        if (!ulRanges.ContainsKey(runInSpine[i])) ulRanges.Add(runInSpine[i], new List<FunUnitLoop>());
                        ulRanges[runInSpine[i]].Add(funLoop as FunUnitLoop);
                    }
                }
            return ulRanges;
        }
    }
}
