using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsMarried : QueryBase
    {
        internal QueryIsMarried(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return (hh.GetPersonValue(indexMaritalStatus, person.indexInHH) == 2.0) ? 1.0 : 0.0;
        }
    }
}
