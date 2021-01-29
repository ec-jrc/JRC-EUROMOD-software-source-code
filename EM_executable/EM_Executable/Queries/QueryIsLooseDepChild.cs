using System.Collections.Generic;
using EM_Common;

namespace EM_Executable
{
    internal class QueryIsLooseDepChild : QueryBase
    {
        internal QueryIsLooseDepChild(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return person.isLooseDepChild ? 1.0 : 0.0;
        }
    }
}
