using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetPartnerIncome: QueryBase
    {
        internal QueryGetPartnerIncome(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareIncomeInfoPar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double partnerId = hh.GetPersonValue(indexPartnerIdVar, person.indexInHH);
            for (byte i = 0; i < hh.GetPersonCount(); i++)
                if (partnerId == hh.GetPersonValue(indexPersonIdVar, i))
                    return parIncome.GetValue(hh, tu, new Person(i));
            return 0.0;
        }
    }
}
