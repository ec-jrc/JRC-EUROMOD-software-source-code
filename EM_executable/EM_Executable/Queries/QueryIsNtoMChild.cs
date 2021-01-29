using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class QueryIsNtoMChild : QueryBase
    {
        internal QueryIsNtoMChild(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareNMPar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            int n = Convert.ToInt32(parN.GetValue()), m = Convert.ToInt32(parM.GetValue());
            List<Person> chs = (from p in tu where p.isDepChild select p).
                OrderByDescending(p => hh.GetPersonValue(indexAgeVar, p.indexInHH)).
                ThenBy(p => hh.GetPersonValue(indexPersonIdVar, p.indexInHH)).ToList();
            for (int i = Math.Max(n - 1, 0); i <= m - 1 && i < chs.Count; ++i)
                if (person.indexInHH == chs[i].indexInHH) return 1;
            return 0.0;
        }

        // help extract:
        // Returns 1 if a person belongs to the n to m oldest dependent children of the assessment unit, 0 otherwise.
        // For being counted as 'dependent child' query IsDepChild must apply.
        // If children are equally aged, they are sorted by idPerson,
        // i.e.children with lower idPerson are considered to be younger than their equally aged siblings.
    }
}
