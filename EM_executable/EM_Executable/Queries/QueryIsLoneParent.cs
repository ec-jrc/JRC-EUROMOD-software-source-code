using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsLoneParent: QueryBase
    {
        internal QueryIsLoneParent(InfoStore infoStore) : base(infoStore)
        {
            isParent = new QueryIsParent(infoStore);
            isWithPartner = new QueryIsWithPartner(infoStore);
        }

        private QueryIsParent isParent;
        private QueryIsWithPartner isWithPartner;

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            isParent.CheckAndPrepare(fun, footnoteNumbers);
            isWithPartner.CheckAndPrepare(fun, footnoteNumbers);
        }

        internal override void PrepareVarIndices()
        {
            base.PrepareVarIndices();
            isParent.PrepareVarIndices();
            isWithPartner.PrepareVarIndices();
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return (isParent.GetValue(hh, tu, person) == 1.0 && isWithPartner.GetValue(hh, tu, person) == 0.0) ? 1.0 : 0.0;
        }
    }
}
