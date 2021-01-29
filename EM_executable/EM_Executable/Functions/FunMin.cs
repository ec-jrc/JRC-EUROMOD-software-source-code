using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunMin  : FunInSpineBase
    {
        private bool posOnly = false;
        List<ParFormula> vals = new List<ParFormula>();

        internal FunMin(InfoStore infoStore) : base(infoStore) { }

        protected override void PrepareNonCommonPar()
        {
            foreach(ParFormula val in GetNonUniquePar<ParFormula>(DefPar.Min.Val))
                vals.Add(val);
            posOnly = GetParBoolValueOrDefault(DefFun.Min, DefPar.Min.Positive_Only);
        }

        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            double min = double.MaxValue;
            foreach (ParFormula val in vals)
            {
                double v = val.GetValue(hh, tu);
                if (posOnly && v <= 0) continue;
                min = Math.Min(min, v);
            }
            return min == double.MaxValue ? 0 : min;
        }
    }
}
