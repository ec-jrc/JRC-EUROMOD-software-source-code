using System;
using System.Collections.Generic;
using EM_Common;

namespace EM_Executable
{
    internal class FunElig  : FunInSpineBase
    {
        internal FunElig(InfoStore infoStore) : base(infoStore) { }

        private ParCond eligCond = null;
        private int indexOutVar = -1;

        protected override void PrepareNonCommonPar()
        {
            eligCond = GetUniquePar<ParCond>(DefPar.Elig.Elig_Cond);
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            indexOutVar = infoStore.operandAdmin.GetIndexInPersonVarList(coParOutVar != null ? coParOutVar.Item1.name
                                                                                             : DefVarName.DEFAULT_ELIGVAR);
        }

        internal override void Run(HH hh, List<Person> tu)
        {
            if (!IsRunCondMet(hh)) return;

            if (IsEligCondMet(hh, tu))
            {
                Dictionary<byte, bool> persElig = eligCond.GetTUValues(hh, tu);

                foreach (var elig in persElig)
                    hh.SetPersonValue(value: elig.Value ? 1 : 0, varIndex: indexOutVar, personIndex: elig.Key);

                if (coParResultVar != null)
                    foreach (var elig in persElig)
                        hh.SetPersonValue(value: elig.Value ? 1 : 0, varIndex: coParResultVar.index, personIndex: elig.Key);
            }
            else
            {
                hh.SetTUValue(value: 0, varIndex: indexOutVar, tu: tu, addTo: false);
                if (coParResultVar != null) hh.SetTUValue(value: 0, varIndex: coParResultVar.index, tu: tu);
            }
        }
    }
}
