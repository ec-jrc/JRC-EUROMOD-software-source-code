using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsParamCurrencyEuro : QueryBase
    {
        internal QueryIsParamCurrencyEuro(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) { }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return infoStore.country.sys.areParamEuro ? 1.0 : 0.0;
        }
    }
}
