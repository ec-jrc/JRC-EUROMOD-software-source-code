using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class QueryIsParentOfDepChild : QueryBase
    {
        internal QueryIsParentOfDepChild(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double perId = hh.GetPersonValue(indexPersonIdVar, person.indexInHH);
            double parId = hh.GetPersonValue(indexPartnerIdVar, person.indexInHH);  // also get the partnerID
            if (parId == 0) parId = -1; // don't match 0!
            for (byte i = 0; i < tu.Count; i++)
                if (tu[i].isDepChild &&  (hh.GetPersonValue(indexFatherIdVar, tu[i].indexInHH) == perId || hh.GetPersonValue(indexMotherIdVar, tu[i].indexInHH) == perId
                     || hh.GetPersonValue(indexFatherIdVar, tu[i].indexInHH) == parId || hh.GetPersonValue(indexMotherIdVar, tu[i].indexInHH) == parId))
                    return 1.0;
            if (person.isHead || person.isPartner)
                return tu.Count(p => p.isLooseDepChild && p.indexInHH != person.indexInHH) > 0 ? 1 : 0;
            return 0.0;
        }
    }
}
