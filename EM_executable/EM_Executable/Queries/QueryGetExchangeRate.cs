using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetExchangeRate : QueryBase
    {
        internal QueryGetExchangeRate(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return infoStore.exRate;
        }
    }
}
