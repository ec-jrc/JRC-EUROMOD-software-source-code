using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class FunAllocate : FunInSpineBase
    {
        internal FunAllocate(InfoStore infoStore) : base(infoStore) { }

        private ParFormula parShare = null;
        private ParCond parShareBetween = null;
        private ParVarIL parShareProp = null;
        private bool shareAllIfNoElig, shareEquIfZero, ignoreNegProp;

        protected override void PrepareNonCommonPar()
        {
            parShare = GetUniquePar<ParFormula>(DefPar.Allocate.Share);
            parShareBetween = GetUniquePar<ParCond>(DefPar.Allocate.Share_Between);
            parShareProp = GetUniquePar<ParVarIL>(DefPar.Allocate.Share_Prop);
            shareAllIfNoElig = GetParBoolValueOrDefault(DefFun.Allocate, DefPar.Allocate.Share_All_IfNoElig);
            shareEquIfZero = GetParBoolValueOrDefault(DefFun.Allocate, DefPar.Allocate.Share_Equ_IfZero);
            ignoreNegProp = GetParBoolValueOrDefault(DefFun.Allocate, DefPar.Allocate.Ignore_Neg_Prop);
        }

        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            double amountToShare = parShare.GetValue(hh, tu);

            List<byte> shareBetween = new List<byte>();
            if (parShareBetween != null)
            {
                shareBetween = (from p in parShareBetween.GetTUValues(hh, tu) where p.Value select p.Key).ToList();
                if (shareBetween.Count == 0 && !shareAllIfNoElig)
                {
                    base.SetOutVars(0, hh, tu);
                    return 0;
                }
            }
            if (shareBetween.Count == 0) shareBetween = (from p in tu select p.indexInHH).ToList();

            Dictionary<byte, double> pShares = new Dictionary<byte, double>();
            double sumShares = 0;
            if (parShareProp != null)
            {
                foreach (var pers in tu)
                {
                    double share = 0;
                    if (shareBetween.Contains(pers.indexInHH))
                    {
                        share = parShareProp.GetValue(hh, tu, pers);
                        if (ignoreNegProp && share < 0) share = 0;
                        sumShares += share;
                    }
                    pShares.Add(pers.indexInHH, share);
                }
                if (sumShares == 0)
                {
                    if (!shareEquIfZero && amountToShare != 0) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                    { // only report if there is anything to share
                        isWarning = true, runTimeErrorId = description.funID,
                        message = $"{description.Get()}: HH {infoStore.GetIDHH(hh)}: {parShareProp.xmlValue} is 0 for all eligible persons - equal sharing is applied"
                    });
                    pShares.Clear();
                }
            }
            if (sumShares == 0)
            {
                sumShares = shareBetween.Count;
                foreach (var pers in tu) pShares.Add(pers.indexInHH, shareBetween.Contains(pers.indexInHH) ? 1 : 0);
            }

            double oneShare = amountToShare / sumShares;
            foreach (var ps in pShares)
            {
                double v = oneShare * ps.Value;
                hh.SetPersonValue(value: v, varIndex: coParOutVar.Item1.index, personIndex: ps.Key, addTo: coParOutVar.Item2);
                if (coParResultVar != null) hh.SetPersonValue(value: v, varIndex: coParResultVar.index, personIndex: ps.Key);
            }
            return 0;
        }

        protected override void SetOutVars(double val, HH hh, List<Person> tu)
        {
            // do nothing, output was already set in DoCalculations
        }
    }
}
