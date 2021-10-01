using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class QueryIsDataVariable: QueryBase
    {
        internal QueryIsDataVariable(InfoStore infoStore) : base(infoStore) { }

        double isDataVar = 0.0;

        internal override void CheckAndPrepare(FunBase fun, List<string> footnoteNumbers)
        {
            ParBase parVariableName = fun.GetFootnotePar<ParBase>(DefQuery.Par.VariableName, footnoteNumbers);
            if (parVariableName != null) isDataVar = infoStore.allDataVariables.Contains(parVariableName.xmlValue.ToLower()) ? 1.0 : 0.0;
            else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: compulsory query-parameter {DefQuery.Par.VariableName} missing" });
        }

        internal override double GetValue(HH hh, List<Person> tu, Person person)
        {
            return isDataVar;
        }
    }
}
