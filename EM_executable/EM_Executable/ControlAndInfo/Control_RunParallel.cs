using EM_Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EM_Executable
{
    public partial class Control
    {
        int reportingFraction = 10;

        private void RunParallel(SpineAdmin spineManager)
        {
            string frac = reportingFraction == 100 ? "%" : "/" + reportingFraction;
            foreach (FunOutOfSpineBase fun in spineManager.runBeforeSpine) fun.Run(); // run pre-spine functions (e.g. Keep/DropUnit, etc.)
            int totalHHs = infoStore.hhAdmin.hhs.Count;
            int currentHH = 0; int currentPerc = 0;

            Parallel.ForEach<HH>(infoStore.hhAdmin.hhs, hh => // for each household run spine-functions (BenCalc, Elig, etc.)
            {
                int curFunIndex = 0;
                while (true)
                {
                    FunInSpineBase fun = spineManager.GetNextFun(ref curFunIndex, hh) as FunInSpineBase; if (fun == null) break;
                    foreach (List<Person> tu in hh.GetTUs(fun.GetTUName())) // loop over TUs within household
                    fun.Run(hh, tu);
                }

                Interlocked.Increment(ref currentHH);
                if (reportingFraction * currentHH / totalHHs > currentPerc)   // use int division here!
            {
                    currentPerc = reportingFraction * currentHH / totalHHs;
                    if (!infoStore.communicator.ReportProgress(new Communicator.ProgressInfo()
                    {
                        id = "Running spine",
                        message = $"Running spine.. {currentPerc}{frac}"
                    })) throw new OperationCanceledException(); // note: one cannot simply break a Parallel.ForEach
                }
            });

            foreach (FunOutOfSpineBase fun in spineManager.runAfterSpine) fun.Run(); // run post-spine functions (DefOutput, etc.)
        }
    }
}
