using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetSystemYear : QueryBase
    {
        internal QueryGetSystemYear(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return Convert.ToDouble(infoStore.country.sys.year);
        }
    }
}
