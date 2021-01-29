using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class QueryIsParent : QueryBase
    {
        internal QueryIsParent(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double perId = hh.GetPersonValue(indexPersonIdVar, person.indexInHH);
            double parId = hh.GetPersonValue(indexPartnerIdVar, person.indexInHH);  // also get the partnerID
            if (parId == 0) parId = -1; // don't match 0!
            for (int i = 0; i < hh.GetPersonCount(); i++)
                if (hh.GetPersonValue(indexFatherIdVar, i) == perId || hh.GetPersonValue(indexMotherIdVar, i) == perId
                     || hh.GetPersonValue(indexFatherIdVar, i) == parId || hh.GetPersonValue(indexMotherIdVar, i) == parId)
                    return 1.0;
            return 0.0;
        }
    }
}
