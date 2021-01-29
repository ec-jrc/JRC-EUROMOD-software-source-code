using EM_Common;
using System.Collections.Generic;

namespace EM_Executable
{
    internal class ParCateg : ParBase
    {
        private string categ = string.Empty;
        private List<string> validCateg = new List<string>();

        internal ParCateg(InfoStore infoStore, List<string> _validCateg) : base(infoStore) { validCateg = _validCateg; }

        internal override void CheckAndPrepare(FunBase fun)
        {
            foreach (string valid in validCateg)
                if (xmlValue.ToLower() == valid.ToLower())
                {
                    categ = valid; return;
                }
            infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $" {description.Get()}: invalid category {xmlValue}" });
        }

        internal string GetCateg() { return categ; }
    }
}
