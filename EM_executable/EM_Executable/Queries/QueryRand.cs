using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryRand : QueryBase
    {
        internal QueryRand(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return hh.GetNextRandom(description);
        }
    }
}
