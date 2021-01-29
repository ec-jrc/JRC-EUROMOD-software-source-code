using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetParentsIncome : QueryBase
    {
        internal QueryGetParentsIncome(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareIncomeInfoPar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double motherId = hh.GetPersonValue(indexMotherIdVar, person.indexInHH);
            double fatherId = hh.GetPersonValue(indexFatherIdVar, person.indexInHH);
            double income = 0.0;
            for (byte i = 0; i < hh.GetPersonCount(); i++)
                if (motherId == hh.GetPersonValue(indexPersonIdVar, i) || fatherId == hh.GetPersonValue(indexPersonIdVar, i))
                    income += parIncome.GetValue(hh, tu, new Person(i));
            return income;
        }
    }
}
