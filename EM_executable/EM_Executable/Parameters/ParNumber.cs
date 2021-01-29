using EM_Common;

namespace EM_Executable
{
    internal class ParNumber : ParBase
    {
        internal ParNumber(InfoStore infoStore) : base(infoStore) { }

        private double value = 0.0;
        private string constName = null; // only relevant if value is a constant, e.g. #_amount = $Const_A, Factor_Value = $Const_B

        internal override void CheckAndPrepare(FunBase fun)
        {
            double periodFactor = HandlePeriodFactor(out string xmlCleaned);

            // if TryParse works: a simple number ...
            if (double.TryParse(EM_Helpers.AdaptDecimalSign(xmlCleaned), out value)) { value *= periodFactor; return; }
            
            // ... if not: possibly a constant
            if (infoStore.operandAdmin.Exists(xmlValue) && infoStore.operandAdmin.GetIsGlobal(xmlValue))
                constName = xmlValue; // note: not necessary to us xmlCleaned, as a constant cannot have a period
            else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: {xmlValue} is not a valid number" });
        }

        internal double GetValue()
        {
            if (constName == null) return value; // simple number

            if (infoStore.hhAdmin != null) // constant
                return infoStore.hhAdmin.GlobalGetVar(infoStore.operandAdmin.GetIndexInPersonVarList(constName));

            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = true, message = $"{description.Get()}: not allowed usage of constant ({xmlValue})" });
            return 0; // would happen if a function asks for this at read-time, i.e. expects a number and not a constant
        }

        private double HandlePeriodFactor(out string xmlCleaned)
        {
            xmlCleaned = xmlValue;
            foreach (var p in DefPeriod.GetPeriods())
            {
                if (!xmlCleaned.EndsWith(p.Key)) continue; // needs to end with period, as a number cannot have footnotes
                xmlCleaned = xmlCleaned.Substring(0, xmlCleaned.Length - p.Key.Length);
                return p.Value;
            }
            return 1.0;
        }
    }
}
