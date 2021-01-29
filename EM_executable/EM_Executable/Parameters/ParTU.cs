using EM_Common;

namespace EM_Executable
{
    internal class ParTU : ParBase
    {
        internal ParTU(InfoStore infoStore) : base(infoStore) { }

        internal string name = string.Empty;

        internal override void CheckAndPrepare(FunBase fun)
        {
            if (infoStore.indexTUs.ContainsKey(xmlValue))
            {
                name = xmlValue;

                // in contrast to other functions FunDefTU's CheckAndPrepare must be called on "first usage" instead of once it is defined
                // otherwise one would get "not-yet-calculated"-waringings for variables, which are not
                // available at the point of TU-definition, but are available at the point of first usage
                if (!infoStore.indexTUs[xmlValue].isPrepared) infoStore.indexTUs[xmlValue].CheckAndPrepare();
                infoStore.indexTUs[xmlValue].isPrepared = true; // to avoid that CheckAndPrepare is called for further usages
            }
            else infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                { isWarning = false, message = $"{description.Get()}: unknown tax unit {xmlValue}" }); 
        }
    }
}
