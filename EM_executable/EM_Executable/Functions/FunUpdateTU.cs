using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    // ---------------------------------------------------------------------------------------------------------------------------------------
    // Some notes on TU-building and rebuilding:
    // The initial idea was to replace the function UpdateTU by a DefTU-parameter 'IsStatic':
    // - true: TUs are built on first use of the specific definition (e.g. tu_family_cc) and never changed afterwards
    // - false: TUs are built on each use and thus e.g. allowing for an "automatic" update of e.g. a child-condition
    //
    // The default-value for this parameter set by the transformer would have been true, as this is the current behaviour
    // (developers could then adapt if they want, which means a.o. that we would need to provide a technical possibility)
    //
    // The default for any future UI is however disputable:
    // False might be the intuitive behaviour but, if set without clear consideration, a considerable speed killer.
    // Moreover, even if a TU-definition should be "dynamic", it may not be necessary to rebuild on each use
    // For illustration consider a tu_family_cc with a child-defintion dependent on il_IsChild
    // 1) tu_fam_cc is used x times in policy bch_cc, where il_IsChild does not change
    // 2) policy bwhatsoever_cc changes il_IsChild
    // 3) tu_fam_cc is used x times in policy tin_cc, where again il_IsChild does not change, but changes of (2) should be taken into account
    // Setting IsStatic=false for tu_fam_cc would yield correct results, but is likely to cause considerable performance losses
    //
    // Thus the currently adopted solution is the following:
    // - Keep function UpdateTU: in the above example it can be used between (2) and (3) and thus solve the performance problem
    // - Still introduce DefTU-parameter IsStatic, with default=true (i.e. the old behaviour), as the danger of unconsidered use seems likely
    // ---------------------------------------------------------------------------------------------------------------------------------------

    internal class FunUpdateTU : FunInSpineBase
    {
        internal FunUpdateTU(InfoStore infoStore) : base(infoStore) { }

        private string tuName = null;
        private bool updateAll;

        protected override void PrepareNonCommonPar()
        {
            updateAll = GetParBoolValueOrDefault(DefFun.UpdateTu, DefPar.UpdateTu.Update_All);
            if (!updateAll)
            {
                ParBase parName = GetUniquePar<ParBase>(DefPar.UpdateTu.Name);
                if (parName != null) tuName = parName.xmlValue;
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { isWarning = false, message = $"{description.Get()}: paramter {DefPar.UpdateTu.Name} not defined" });
            }
        }

        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            if (tuName == null) infoStore.hhAdmin.UpdateAllTUs(hh); else hh.UpdateTU(tuName);
            return 0.0;
        }

        protected override void SetOutVars(double val, HH hh, List<Person> tu) { } // nothing to do
        
    }
}
