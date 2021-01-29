using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetCoupleIncome : QueryBase
    {
        internal QueryGetCoupleIncome(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareIncomeInfoPar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double partnerId = hh.GetPersonValue(indexPartnerIdVar, person.indexInHH);
            double income = parIncome.GetValue(hh, tu, person);
            for (byte i = 0; i < hh.GetPersonCount(); i++)
                if (partnerId == hh.GetPersonValue(indexPersonIdVar, i))
                    return income + parIncome.GetValue(hh, tu, new Person(i)); 
            return income;
        }
    }
}
