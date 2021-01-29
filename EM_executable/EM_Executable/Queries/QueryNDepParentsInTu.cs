using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryNDepParentsInTu : QueryBase
    {
        internal QueryNDepParentsInTu(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double n = 0.0;
            foreach (Person p in tu)
                if (p.isDepParent) ++n;
            return n;
        }
    }
}
