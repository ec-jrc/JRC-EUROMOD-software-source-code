using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunRestore  : FunInSpineBase
    {
        internal FunRestore(InfoStore infoStore) : base(infoStore) { }

        private FunStore relatedStore = null;
        private double loopIteration = -1;

        protected override void PrepareNonCommonPar()
        {
            ParBase parPostLoop = GetUniquePar<ParBase>(DefPar.Restore.PostLoop);
            ParBase parPostFix = GetUniquePar<ParBase>(DefPar.Restore.PostFix);
            if (parPostLoop == null && parPostFix == null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: neither {DefPar.Restore.PostFix} nor {DefPar.Restore.PostLoop} defined" });
                return;
            }
            if (parPostLoop != null && parPostFix != null)
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: unclear specification - both {DefPar.Restore.PostFix} and {DefPar.Restore.PostLoop} defined" });
                return;
            }

            string post = parPostLoop == null ? parPostFix.xmlValue : parPostLoop.xmlValue;
            if (!infoStore.operandAdmin.indexStoreFuns.ContainsKey(post))
            {
                infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: no related Store-function ({post}) found" });
                return;
            }

            relatedStore = infoStore.operandAdmin.indexStoreFuns[post];
            switch (relatedStore.storeType)
            {
                case FunStore.STORETYPE.UNITLOOP: infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{description.Get()}: restoring UnitLoop values is not possible" }); return;
                case FunStore.STORETYPE.FIX: if (parPostFix == null) { infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{parPostLoop.description.Get()}: related Store-function concerns {DefPar.Store.PostFix}" }); return; }
                    break;
                case FunStore.STORETYPE.LOOP: if (parPostLoop == null) { infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false,
                    message = $"{parPostFix.description.Get()}: related Store-function concerns {DefPar.Store.PostLoop}" }); return; }
                    break;
            }

            if (parPostLoop != null)
            {
                ParNumber parIteration = GetUniquePar<ParNumber>(DefPar.Restore.Iteration);
                if (parIteration != null)
                {
                    loopIteration = parIteration.GetValue();
                    if (!EM_Helpers.IsNonNegInteger(loopIteration, false)) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $"{parIteration.description.Get()}: must be a non-negative integer" });
                    else relatedStore.RegisterOperands((int)loopIteration); // see description in FunStore
                }
                else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                     { isWarning = false, message = $"{description.Get()}: parameter {DefPar.Restore.Iteration} missing" });
            }
        }

        // note that Restore actually does not have a TAX_UNIT, thus this is the HHAdmin.DUMMY_INDIVIDUAL_TU (see FunInSpineBase.GetTUName)
        // that means, amongst others, that the function is called for each person in the household (i.e. tu always contains one person)
        internal override void Run(HH hh, List<Person> tu)
        {
            if (!IsRunCondMet(hh)) return;

            foreach (FunStore.StoreVar v in relatedStore.vars)
            {
                if (loopIteration == -1 || v.iteration == loopIteration)
                    hh.SetTUValue(hh.GetTUValue(v.storeVar.index, tu), v.origVar.index, tu);
            }
        }
    }
}
