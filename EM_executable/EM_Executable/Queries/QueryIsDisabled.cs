using System.Collections.Generic;
using EM_Common;

namespace EM_Executable
{
    internal class QueryIsDisabled : QueryBase
    {
        private int indexDisabledVar = -1;
        private bool autoReg = false; // this variables is required for the case that actually DDI is not found in data,
                                      // but the user provides a default (i.e. the CheckAndPrepare below is not the one doing the registration)

        internal QueryIsDisabled(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            foreach (string dVar in new List<string>() { DefVarName.DDI, DefVarName.DDILV })
            {
                if (infoStore.operandAdmin.Exists(dVar)) continue;
                if (dVar == DefVarName.DDI) autoReg = true;
                infoStore.operandAdmin.CheckRegistration(name: dVar,
                        isOutVar: false,
                        warnIfNotInit: true,
                        description: description);
                infoStore.operandAdmin.SetProvidedBySetDefault(dVar);
            }
        }

        internal override void PrepareVarIndices() 
        {
            if (!infoStore.operandAdmin.GetProvidedBySetDefault(DefVarName.DDI)) // providedByDefault is set to false, if variable is found in data (see HHAdmin.GenerateData)
                indexDisabledVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DDI);
            else if (!infoStore.operandAdmin.GetProvidedBySetDefault(DefVarName.DDILV))
            {
                indexDisabledVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DDILV);
                if (autoReg) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    message = $"{description.Get()}: variable {DefVarName.DDI} missing - using {DefVarName.DDILV} for the {DefQuery.IsDisabled} query",
                });
            }
            else
            {
                indexDisabledVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.DDI); // variable ddi exists and is 0 (due to using SetProvidedBySetDefault in CheckAndPrepare)
                if (autoReg) infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    message = $"{description.Get()}: variables {DefVarName.DDI} & {DefVarName.DDILV} both missing - {DefQuery.IsDisabled} query will always return 0"
                });
            }
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return (hh.GetPersonValue(indexDisabledVar, person.indexInHH) > 0.0) ? 1.0 : 0.0;
        }
    }
}
