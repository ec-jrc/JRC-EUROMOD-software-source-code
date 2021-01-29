using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsUsedDatabase : QueryBase
    {
        internal QueryIsUsedDatabase(InfoStore infoStore) : base(infoStore) { }

        double isUsedDB = 0.0;

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            ParBase parDatabase = fun.GetFootnotePar<ParBase>(DefQuery.Par.DataBasename, footnoteNumbers);
            if (parDatabase != null) isUsedDB = infoStore.IsUsedDatabase(parDatabase.xmlValue) == true ? 1.0 : 0.0;
            else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: compulsory query-parameter {DefQuery.Par.DataBasename} missing" });
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return isUsedDB;
        }
    }
}
