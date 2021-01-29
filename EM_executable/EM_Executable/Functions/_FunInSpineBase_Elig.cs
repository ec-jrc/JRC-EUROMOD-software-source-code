using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class FunInSpineBase : FunBase
    {
        protected ParCateg coParWho = null;
        private ParVar coParEligVar = null; protected int indexEligVar = -1;

        protected void PrepareEligPar()
        {
            coParWho = GetUniquePar<ParCateg>(DefPar.Common.Who_Must_Be_Elig);
            coParEligVar = GetUniquePar<ParVar>(DefPar.Common.Elig_Var);
        }

        internal override void ReplaceVarNameByIndex()
        {
            indexEligVar = infoStore.operandAdmin.GetIndexInPersonVarList(coParEligVar != null ? coParEligVar.name
                                                                                               : DefVarName.DEFAULT_ELIGVAR);
            base.ReplaceVarNameByIndex();
        }

        protected virtual bool IsEligCondMet(HH hh, List<Person> tu)
        {
            return coParWho == null ? true : IsCondMetByTU(hh, tu, indexEligVar, coParWho.GetCateg());
        }

        // this function and its overloads is called from above and by functions FunDropKeepUnit, FunTotals, FunDefOutput ...
        internal static bool IsCondMetByTU(HH hh, List<Person> tu, int iEligVar, string who) { return IsCondMetByTU(hh, tu, who, null, iEligVar); }
        internal static bool IsCondMetByTU(HH hh, List<Person> tu, ParCond parCond, string who) { return IsCondMetByTU(hh, tu, who, parCond); }
        private static bool IsCondMetByTU(HH hh, List<Person> tu, string who, ParCond parCond = null, int iEligVar = -1)
        {
            int i = 0; if (who == DefPar.Value.WHO_NOBODY) return true;
            if (parCond != null)
            {
                foreach (var pe in parCond.GetTUValues(hh, tu))
                {
                    bool elig = pe.Value; Person p = tu[i++];
                    bool? ew = EvalWho(elig, p); if (ew != null) return ew == true ? true : false;
                }
            }
            else
            {
                foreach (Person p in tu)
                {
                    bool elig = hh.GetPersonValue(iEligVar, p.indexInHH) != 0;
                    bool? ew = EvalWho(elig, p); if (ew != null) return ew == true ? true : false;
                }
            }
            return who == DefPar.Value.WHO_ALL || who == DefPar.Value.WHO_ALL_ADULTS;

            bool? EvalWho(bool elig, Person p)
            {
                if (who == DefPar.Value.WHO_ONE && elig) return true;
                if (who == DefPar.Value.WHO_ALL && !elig) return false;
                if (who == DefPar.Value.WHO_ONE_ADULT && !p.isDepChild && elig) return true;
                if ((who == DefPar.Value.WHO_ALL_ADULTS) && !p.isDepChild && !elig) return false;
                if (who == DefPar.Value.WHO_NOBODY) return true;
                return null;
            }
        }
    }
}
