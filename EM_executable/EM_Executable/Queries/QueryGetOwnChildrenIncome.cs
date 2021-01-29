using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetOwnChildrenIncome : QueryBase
    {
        internal QueryGetOwnChildrenIncome(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareIncomeInfoPar(fun, footnoteNumbers);
            PrepareAgePar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            GetAgeLimits(out double ageMin, out double ageMax);
            double personId = hh.GetPersonValue(indexPersonIdVar, person.indexInHH);
            double income = 0.0;

            for (byte i = 0; i < hh.GetPersonCount(); i++)
                if (personId == hh.GetPersonValue(indexMotherIdVar, i) || personId == hh.GetPersonValue(indexFatherIdVar, i))
                {
                    double age = hh.GetPersonValue(indexAgeVar, i);
                    if (age >= ageMin && age <= ageMax)
                        income += parIncome.GetValue(hh, tu, new Person(i));
                }
            return income;
        }
    }
}
