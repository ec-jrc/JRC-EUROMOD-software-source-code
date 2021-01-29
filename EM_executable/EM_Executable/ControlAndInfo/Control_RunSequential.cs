using EM_Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EM_Executable
{
    public partial class Control
    {
        private bool RunSequential(SpineAdmin spineManager)
        {
            int curFunIndex = 0;

            Dictionary<FunBase, List<FunUnitLoop>> unitLoopRanges = spineManager.GetUnitLoopRangesForSequentialRun();

            while (true) // loop over functions
            {
                FunBase fun = spineManager.GetNextFun(ref curFunIndex); if (fun == null) break;

                DefFun.RUN_MODE runMode = // find out if the function expects to be run within HH-loop or not
                    DefinitionAdmin.GetFunDefinition(fun.description.GetFunName()).runMode;
                if (runMode != DefFun.RUN_MODE.IN_SPINE)
                    (fun as FunOutOfSpineBase).Run(); // concerns functions usually running pre- or post-spine (DefOutput, Totals, etc.)
                else
                {
                    // concerns functions usually running in spine (BenCalc, Elig, etc.)
                    FunInSpineBase spineFun = fun as FunInSpineBase;
                    if (infoStore.runConfig.forceSequentialRun)
                    {
                        // run in a single thread
                        foreach (HH hh in infoStore.hhAdmin.hhs) 
                            foreach (List<Person> tu in hh.GetTUs(spineFun.GetTUName()))
                                SpineFunRun(spineFun, hh, tu);
                    }
                    else
                    {
                        // run in multiple threads
                        Parallel.ForEach<HH>(infoStore.hhAdmin.hhs, hh => {
                            foreach (List<Person> tu in hh.GetTUs(spineFun.GetTUName()))
                                SpineFunRun(spineFun, hh, tu);
                        });
                    }
                }
                if (!infoStore.communicator.ReportProgress(
                    new Communicator.ProgressInfo() { message = $"Done with function {fun.description.Get(true)}" })) return false;
            }
            return true;

            void SpineFunRun(FunInSpineBase spineFun, HH hh, List<Person> tu)
            {
                bool run = false;
                if (unitLoopRanges.ContainsKey(spineFun)) // there are UnitLoops in the spine
                {                                         // note that a function may be enclosed by more than one UnitLoop (see NRR add-on)
                    foreach (FunUnitLoop ul in unitLoopRanges[spineFun]) // if any of the UnitLoops allows running - run
                        if (!ul.IsFinished(hh)) run = true;
                }
                else run = true; // the usual case - no UnitLoops

                if (run) spineFun.Run(hh, tu);
            }
        }
    }
}
