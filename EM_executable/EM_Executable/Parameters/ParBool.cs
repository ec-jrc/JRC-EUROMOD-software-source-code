using EM_Common;

namespace EM_Executable
{
    internal class ParBool : ParBase
    {
        internal ParBool(InfoStore infoStore) : base(infoStore) { }

        private bool value = false;

        internal static bool IsTrue(string val) // note that this is static and internal, because the parameter may be used before 
        {                                       // CheckAndPrepare, which would allow using GetBoolValue (see e.g. in FunDefIL.ProvideIndexInfo)
            return EM_Helpers.GetBool(val) == true;
        }

        internal static bool IsValid(string val) { return EM_Helpers.IsValidBool(val); }

        internal override void CheckAndPrepare(FunBase fun)
        {
            if (IsValid(xmlValue)) value = IsTrue(xmlValue);
            else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: {xmlValue} is not a boolean value" });
        }

        internal bool GetBoolValue() { return value; }
    }
}
