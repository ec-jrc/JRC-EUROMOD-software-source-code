using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryGetDataIncomeYear : QueryBase
    {
        internal QueryGetDataIncomeYear(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double year = 0; double.TryParse(infoStore.country.data.year, out year);
            return year;
        }
    }
}
