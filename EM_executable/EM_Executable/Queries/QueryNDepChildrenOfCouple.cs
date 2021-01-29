using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class QueryNDepChildrenOfCouple : QueryBase
    {
        internal QueryNDepChildrenOfCouple(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareAgePar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            GetAgeLimits(out double ageMin, out double ageMax);
            double headID = hh.GetPersonValue(indexPersonIdVar, person.indexInHH), partnerID = hh.GetPersonValue(indexPartnerIdVar, person.indexInHH);
            double n = 0.0;
            foreach (Person p in tu)
                if (p.isDepChild || p.isLooseDepChild)
                    if (hh.GetPersonValue(indexPersonIdVar, p.indexInHH) != headID && hh.GetPersonValue(indexPersonIdVar, p.indexInHH) != partnerID)
                        if (hh.GetPersonValue(indexFatherIdVar, p.indexInHH) == headID || (partnerID > 0 && hh.GetPersonValue(indexFatherIdVar, p.indexInHH) == partnerID)
                            || hh.GetPersonValue(indexMotherIdVar, p.indexInHH) == headID || (partnerID > 0 && hh.GetPersonValue(indexMotherIdVar, p.indexInHH) == partnerID)
                            || (hh.GetPersonValue(indexFatherIdVar, p.indexInHH) == 0 && hh.GetPersonValue(indexMotherIdVar, p.indexInHH) == 0))
                            if (hh.GetPersonValue(indexAgeVar, p.indexInHH) >= ageMin && hh.GetPersonValue(indexAgeVar, p.indexInHH) <= ageMax)
                                ++n;

            return n;
        }
    }
}
