using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsInEducation : QueryBase
    {
        internal QueryIsInEducation(InfoStore infoStore) : base(infoStore) { }

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers) {}

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return (hh.GetPersonValue(indexEducationVar, person.indexInHH) > 0.0 && hh.GetPersonValue(indexEconomicStatusVar, person.indexInHH) == 6.0) ? 1.0 : 0.0;
        }
    }
}
