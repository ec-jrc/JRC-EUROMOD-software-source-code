using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunMax  : FunInSpineBase
    {
        List<ParFormula> vals = new List<ParFormula>();

        internal FunMax(InfoStore infoStore) : base(infoStore) { }

        protected override void PrepareNonCommonPar()
        {
            foreach(ParFormula val in GetNonUniquePar<ParFormula>(DefPar.Max.Val))
                vals.Add(val);
        }

        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            double max = double.MinValue;
            foreach (ParFormula val in vals)
                max = Math.Max(max, val.GetValue(hh, tu));
            return max;
        }
    }
}
