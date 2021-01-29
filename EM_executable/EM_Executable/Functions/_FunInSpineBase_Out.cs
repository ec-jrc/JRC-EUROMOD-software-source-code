using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class FunInSpineBase : FunBase
    {
        protected Tuple<ParOutVar, bool> coParOutVar = null; // bool: addto
        protected ParOutVar coParResultVar = null;

        protected void PrepareOutPar()
        {
            ParOutVar parOutVar = GetUniquePar<ParOutVar>(DefPar.Common.Output_Var);
            if (parOutVar != null) coParOutVar = new Tuple<ParOutVar, bool>(parOutVar, false);
            else
            {
                parOutVar = GetUniquePar<ParOutVar>(DefPar.Common.Output_Add_Var);
                if (parOutVar != null) coParOutVar = new Tuple<ParOutVar, bool>(parOutVar, true);
            }
            coParResultVar = GetUniquePar<ParOutVar>(DefPar.Common.Result_Var);
        }

        protected virtual void SetOutVars(double val, HH hh, List<Person> tu)
        {
            hh.SetTUValue(value: val, varIndex: coParOutVar.Item1.index, tu: tu, addTo: coParOutVar.Item2);
            if (coParResultVar != null) hh.SetTUValue(value: val, varIndex: coParResultVar.index, tu: tu);
        }
    }
}
