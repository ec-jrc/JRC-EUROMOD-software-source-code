using System.Collections.Generic;

namespace EM_Executable
{
    /// <summary>
    /// each "regular tax-ben" function inherits from this class
    /// more concrete: each function with DefinitionAdmin.Fun.defaultRunOption = IN_SPINE 
    /// </summary>
    internal partial class FunInSpineBase : FunBase
    {
        internal FunInSpineBase(InfoStore infoStore) : base(infoStore) { }

        // make sure that there is always a taxunit, as it is required in the hh-loop
        // this is essential for functions which actually do not have a TAX_UNIT parameter (ILVarOp, Store, ...)
        // see HHAdmin.CreateDummyTU for more comments
        internal virtual string GetTUName()
        {
            return coParTU == null ? HHAdmin.DUMMY_INDIVIDUAL_TU : coParTU.name;
        }

        internal override void PrepareCommonPar()
        {
            base.PrepareCommonPar();
            PrepareEligPar(); PrepareOutPar(); PrepareLimPar(); PrepareRoundPar();
        }

        /// <summary>
        /// base class implements the usual tasks of a regular tax-ben-function:
        /// - evaluate run condition
        /// - evaluate eligiblity condition
        /// - do whatever the function does
        /// - apply limits and rounding
        /// - set output variables
        /// each of these steps could be individually adapted by the derived functions by implementing an override
        /// moreover, for some functions (e.g. Store, Restore, ...) it is easier to override the whole function (therefore virtual)
        /// </summary>
        internal virtual void Run(HH hh, List<Person> tu)
        {    
            if (!IsRunCondMet(hh)) return;

            double val = 0.0;
            if (IsEligCondMet(hh, tu))
            {
                val = DoFunWork(hh, tu);
                val = ApplyOutVarLimits(val, hh, tu);
                val = ApplyRounding(val, hh, tu);
            }
            SetOutVars(val, hh, tu);
        }

        /// <summary> each derived function overrides this to do its individual work </summary>
        protected virtual double DoFunWork(HH hh, List<Person> tu) { return 0; }
    }
}
