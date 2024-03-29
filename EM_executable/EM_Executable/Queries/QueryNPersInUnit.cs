﻿using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryNPersInUnit : QueryBase
    {
        internal QueryNPersInUnit(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareAgePar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double n = 0.0;
            GetAgeLimits(out double ageMin, out double ageMax);
            foreach (Person p in tu)
            {
                double age = hh.GetPersonValue(indexAgeVar, p.indexInHH);
                if (age >= ageMin && age <= ageMax) ++n;
            }
            return n;
        }
    }
}
