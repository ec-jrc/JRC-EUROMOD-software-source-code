using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunArithOp : FunInSpineBase
    {
        internal FunArithOp(InfoStore infoStore) : base(infoStore) { }

        private ParFormula parFormula = null;

        protected override void PrepareNonCommonPar()
        {
            parFormula = GetUniquePar<ParFormula>(DefPar.ArithOp.Formula);
        }

        protected override double DoFunWork(HH hh, List<Person> tu)
        {
            return parFormula.GetValue(hh, tu);
        }
    }
}
