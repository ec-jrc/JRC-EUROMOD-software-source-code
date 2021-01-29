using System.Collections.Generic;
using EM_Common;

namespace EM_Executable
{
    internal class FunScale : FunInSpineBase
    {
        internal FunScale(InfoStore infoStore) : base(infoStore) { }

        double factorVar;

        internal void SetParScaleFactor()
        {
            // it is important to set infoStore.parScaleFactor already here, to be ready for the actual scaling, which is done in ParBase.GetPeriods:
            // this function is called in FunBase.TakePar, immediately after FunBase.ClassifyParameters
            // ClassifyParameters generates e.g. formulas (ParFormula), which call ParBase.GetPeriods in the constructor
            // that means each function after the very Scale-function will be affected by the scaling

            ParNumber parFactorPar = GetUniquePar<ParNumber>(DefPar.Scale.FactorParameter);
            if (parFactorPar == null) return;

            // as all the preparing is not yet done, we cannot rely on the "normal automatic" handling, but need to pre-handle here, e.g. the run-condition
            coParRunCond = GetUniquePar<ParCond>(DefPar.Common.Run_Cond);
            if (coParRunCond != null)
            {
                coParRunCond.CheckAndPrepare(this);
                if (!IsRunCondMet()) return;
            }

            parFactorPar.CheckAndPrepare(this);
            double factorPar = parFactorPar.GetValue();
            infoStore.parScaleFactor = factorPar;
        }

        protected override void PrepareNonCommonPar()
        {
            ParNumber parFactorVar = GetUniquePar<ParNumber>(DefPar.Scale.FactorVariables);           
            factorVar = parFactorVar == null ? 1.0 : parFactorVar.GetValue();
        }

        internal override void Run(HH hh, List<Person> tu) // note: Scale does not have a TAX_UNIT parameter
        {                                                  // therefore tu is DUMMY_INDIVIDUAL_TU, i.e. this is called for each person
            if (factorVar == 1 || !IsRunCondMet(hh)) return;

            foreach (int varIndex in infoStore.operandAdmin.GetMonetaryVarIndices())
                hh.SetTUValue(hh.GetTUValue(varIndex, tu) * factorVar, varIndex, tu);
        }
    }
}
