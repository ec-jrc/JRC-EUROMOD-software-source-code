using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsCivilServant : QueryBase
    {
        private int indexCivilServantVar = -1;

        internal QueryIsCivilServant(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            infoStore.operandAdmin.CheckRegistration(name: DefVarName.LCS,
                    isOutVar: false,
                    warnIfNotInit: true,
                    description: description);
            infoStore.operandAdmin.SetProvidedBySetDefault(DefVarName.LCS);
        }

        internal override void PrepareVarIndices()
        {
            if (infoStore.operandAdmin.GetProvidedBySetDefault(DefVarName.LCS) && // providedByDefault is set to false if variable is found in data (see HHAdmin.GenerateData)
                !infoStore.country.data.useCommonDefault) // do not issue a warning if common-default is set (mainly to avoid zig warnings with tranining-data)
                infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                {
                    isWarning = true,
                    message = $"{description.Get()}: variable {DefVarName.LCS} missing {DefQuery.IsCivilServant} query will always return 0"
                });
            indexCivilServantVar = infoStore.operandAdmin.GetIndexInPersonVarList(DefVarName.LCS);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return (hh.GetPersonValue(indexCivilServantVar, person.indexInHH) == 1.0) ? 1.0 : 0.0;
        }
    }
}
