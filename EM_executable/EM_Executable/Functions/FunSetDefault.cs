using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class FunSetDefault : FunInSpineBase
    {
        internal FunSetDefault(InfoStore infoStore) : base(infoStore) { }

        private class Default
        {
            internal VarSpec varSpec;
            internal ParFormula parFormula;
        }

        private List<Default> defaults = new List<Default>();

        protected override void PrepareNonCommonPar()
        {
            // note: using a run-condition with SetDefault does in fact not work properly
            // in reality this is (currently) not a problem, because no country/system uses it
            // the problem is that only run-conds available at read-time would prevent the creation of the variables
            // (i.e. conditions using queries like IsUsedDatabase (does not make sense), GetSystemYear, ...)
            // run-conds like { $SomeGlobalVar=1 } are only available at run-time and would only prevent initialisation
            // the old executable also creates such default-variables, but has the VOID-value to issues a warning
            // in the new executable the variables would be zero and no warning is issued
            // a not clean solution could be to check after the before-spine-function-run, but this works only for parallel runs
            // concluding: as this is a fictional problem a think we do not have to do a headstand
            if (!IsRunCondMet()) return;

            foreach (var p in GetPlaceholderPar<ParFormula>())
            {
                // the 'content' of the parameter (i.e. the formula containing the default) was handled in FunBase.CheckAndPrepare
                // but the variable for which the default is set must still be registered
                // more precisely: it must be registered as 'providedBySetDefault',
                // otherwise parameters which use the variable would claim that it is not initialised
                infoStore.operandAdmin.CheckRegistration(name: p.Key, isOutVar: false, warnIfNotInit: false, description: p.Value.description);
                if (infoStore.operandAdmin.Exists(p.Key)) // registration could fail, therefore check
                {
                    infoStore.operandAdmin.SetInitialised(p.Key);
                    infoStore.operandAdmin.SetProvidedBySetDefault(p.Key);
                    defaults.Add(new Default() { varSpec = new VarSpec() { name = p.Key }, parFormula = p.Value });
                }
            }
        }

        internal override void ReplaceVarNameByIndex()
        {
            base.ReplaceVarNameByIndex();
            foreach (Default d in defaults) d.varSpec.index = infoStore.operandAdmin.GetIndexInPersonVarList(d.varSpec.name);

            // remove those variables which were actually found in data (to avoid an if in the hh-loop)
            for (int i = defaults.Count - 1; i >= 0; --i)
                if (!infoStore.operandAdmin.GetProvidedBySetDefault(defaults[i].varSpec.name))
                    defaults.RemoveAt(i);
        }

        internal override void Run(HH hh, List<Person> tu) // is called for each person in HH, i.e. with individual TU
        {
            if (!IsRunCondMet(hh)) return; // see note on run-cond above

            foreach (Default d in defaults)
            {
                double val = d.parFormula.GetValue(hh, tu, tu[0]);
                hh.SetPersonValue(val, d.varSpec.index, tu[0].indexInHH);
            }
        }
    }
}
