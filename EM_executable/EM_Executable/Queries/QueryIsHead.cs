using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsHead : QueryBase
    {
        internal QueryIsHead(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return person.isHead ? 1.0 : 0.0;
        }
    }
}
