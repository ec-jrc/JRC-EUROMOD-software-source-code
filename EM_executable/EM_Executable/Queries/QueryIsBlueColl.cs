using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsBlueColl : QueryBase
    {
        internal QueryIsBlueColl(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double occ = hh.GetPersonValue(indexOccupationVar, person.indexInHH);
            return (occ == 0 || occ == 6 || occ == 7 || occ == 8 || occ == 9) ? 1.0 : 0.0;
        }
    }
}
