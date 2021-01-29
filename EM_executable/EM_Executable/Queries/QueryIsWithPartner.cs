using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsWithPartner : QueryBase
    {
        internal QueryIsWithPartner(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return (hh.GetPersonValue(indexPartnerIdVar, person.indexInHH) > 0.0) ? 1.0 : 0.0;
        }
    }
}
