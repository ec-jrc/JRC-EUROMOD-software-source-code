using System.Collections.Generic;
using EM_Common;

namespace EM_Executable
{
    internal class FunInitVars : FunInSpineBase
    {
        internal FunInitVars(InfoStore infoStore) : base(infoStore) { }

        private class Default
        {
            internal VarSpec varSpec;
            internal ParFormula parFormula;
        }

        private List<Default> defaults = new List<Default>();
        private bool initOnce = false;

        protected override void PrepareNonCommonPar()
        {
            if (!IsRunCondMet()) return;

            foreach (var p in GetPlaceholderPar<ParFormula>())
            {
                // the 'content' of the parameter (i.e. the init-value or -formula) was handled in FunBase.CheckAndPrepare
                // but the variable for which the default is set must still be registered and set to initialised
                infoStore.operandAdmin.CheckRegistration(name: p.Key, isOutVar: false, warnIfNotInit: false, description: p.Value.description);
                if (infoStore.operandAdmin.Exists(p.Key)) // registration could fail, therefore check
                {
                    infoStore.operandAdmin.SetInitialised(p.Key);
                    defaults.Add(new Default() { varSpec = new VarSpec() { name = p.Key }, parFormula = p.Value });
                }
            }
            if (GetUniquePar<ParBool>(DefPar.InitVars.InitOnce) != null) initOnce = GetUniquePar<ParBool>(DefPar.InitVars.InitOnce).GetBoolValue();
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (Default d in defaults) d.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(d.varSpec.name);

            if (initOnce)
            {
                foreach (HH hh in infoStore.hhAdmin.hhs)
                    foreach (List<double> hhMem in hh.personVarList)
                        foreach (Default d in defaults)
                            hhMem[d.varSpec.index] = double.NaN;
            }
        }

        internal override void Run(HH hh, List<Person> tu) // is called for each person in HH, i.e. with individual TU
        {
            if (!IsRunCondMet(hh)) return;

            foreach (Default d in defaults)
            {
                if (initOnce && !double.IsNaN(hh.GetPersonValue(d.varSpec.index, tu[0].indexInHH))) continue;
                double val = d.parFormula.GetValue(hh, tu, tu[0]);
                hh.SetPersonValue(val, d.varSpec.index, tu[0].indexInHH);
            }
        }
    }
}
