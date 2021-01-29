using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class QueryHasMinValInTu : QueryBase
    {
        internal QueryHasMinValInTu(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            PrepareMinMaxPar(fun, footnoteNumbers);
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            double personVal = minMaxVal.GetValue(hh, tu, person);
            List<Person> temp_tu = tu;
            // first filter for adults if you have to
            if (minMaxAdultsOnly && temp_tu.Count(p => !p.isDepChild) > 0)
                temp_tu = temp_tu.Where(p => !p.isDepChild).ToList();
            // then order the remaining by val & id
            temp_tu = temp_tu.OrderBy(p => minMaxVal.GetValue(hh, tu, p)).
                ThenBy(p => hh.GetPersonValue(indexPersonIdVar, p.indexInHH)).ToList();
            // then give value depending on unique
            return ((personVal == minMaxVal.GetValue(hh, tu, temp_tu[0])) && (!minMaxUnique || person == temp_tu[0])) ? 1.0 : 0.0;
        }
    }
}
