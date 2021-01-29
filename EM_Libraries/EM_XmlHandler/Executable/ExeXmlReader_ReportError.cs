using EM_Common;
using System.IO;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        // unified error/warning reporting
        private static void ReportError(Communicator communicator, string filePath, string error, bool forbidsRun = true)
        {
            communicator.ReportError(new Communicator.ErrorInfo()
            {
                isWarning = !forbidsRun,
                message = $"Problem reading {Path.GetFileName(filePath)}: {error}"
            });
        }
    }
}
