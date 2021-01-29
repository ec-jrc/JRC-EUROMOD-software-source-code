using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class QueryNChildrenOfCouple : QueryBase
    {
        internal QueryNChildrenOfCouple(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareAgePar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            GetAgeLimits(out double ageMin, out double ageMax);
            double headID = hh.GetPersonValue(indexPersonIdVar, person.indexInHH), partnerID = hh.GetPersonValue(indexPartnerIdVar, person.indexInHH);
            double n = 0.0;
            for (int i = 0; i < hh.GetPersonCount(); i++)
                if (hh.GetPersonValue(indexPersonIdVar, i) != headID && hh.GetPersonValue(indexPersonIdVar, i) !=partnerID)
                    if (hh.GetPersonValue(indexFatherIdVar, i) == headID || (partnerID>0 && hh.GetPersonValue(indexFatherIdVar, i) == partnerID)
                        || hh.GetPersonValue(indexMotherIdVar, i) == headID || (partnerID>0 && hh.GetPersonValue(indexMotherIdVar, i) == partnerID))
                        if (hh.GetPersonValue(indexAgeVar, i) >= ageMin && hh.GetPersonValue(indexAgeVar, i) <= ageMax)
                            ++n;

            return n;
        }
    }
}
