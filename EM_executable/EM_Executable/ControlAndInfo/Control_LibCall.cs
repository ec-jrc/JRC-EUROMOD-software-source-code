using EM_Common;
using System;
using System.Collections.Generic;
using System.Text;
using EM_XmlHandler;
using System.Linq;

namespace EM_Executable
{
    public partial class Control
    {
        /// <summary> "LIB-CALL": runs the executable as a library </summary>
        /// <param name="configSettings"> settings defining the programme run </param>
        /// <param name="progressAction"> optional, an action, defined by the caller, which handles progress-reporting </param>
        /// <param name="errorAction"> optional, an action, defined by the caller, which handles error-reporting </param>
        /// <param name="inputData"> optional, the input data, passed directly by the caller </param>
        public bool Run(Dictionary<string, string> configSettings,
                        Func<Communicator.ProgressInfo, bool> progressAction = null,
                        Action<Communicator.ErrorInfo> errorAction = null, IEnumerable<string> inputData = null)
        {
            try
            {
                infoStore.runConfig.TakeSettings(configSettings, infoStore.communicator); // note: throws exception on failure
                infoStore.communicator.progressAction = progressAction; // prepare the progress- and error-reporting
                infoStore.communicator.errorAction = errorAction;
                infoStore.setData(inputData);   // set data (can be null!)
                return Run(); // call the common run-function for "exe"-call and "lib"-call
            }
            catch (Exception exception)
            {
                bool runCanceled = exception.GetType() == typeof(OperationCanceledException) || // is thrown by RunParallel to exit the Parallel.ForEach-loop
                    (exception.InnerException != null && exception.InnerException.GetType() == typeof(OperationCanceledException));
                if (!runCanceled) infoStore.communicator.ReportError(new Communicator.ErrorInfo() { isWarning = false, message = exception.Message });
                return false;
            }
        }

        /// <summary> "LIB-CALL": runs the executable as a library and returns the output(s) in memory. Can be used when there is a need to avoid writting files (e.g. for security purposes).</summary>
        /// <param name="configSettings"> settings defining the programme run </param>
        /// <param name="output"> the output microdata </param>
        /// <param name="progressAction"> optional, an action, defined by the caller, which handles progress-reporting </param>
        /// <param name="errorAction"> optional, an action, defined by the caller, which handles error-reporting </param>
        /// <param name="inputData"> optional, the input data, passed directly by the caller </param>
        public bool RunInMemory(Dictionary<string, string> configSettings, 
                        out Dictionary<string, StringBuilder> output,
                        Func<Communicator.ProgressInfo, bool> progressAction = null,
                        Action<Communicator.ErrorInfo> errorAction = null, IEnumerable<string> inputData = null)
        {
            output = null;
            configSettings.AddOrReplace(TAGS.CONFIG_RETURN_OUTPUT_IN_MEMORY, DefPar.Value.YES);
            bool succeeded = Run(configSettings, progressAction, errorAction, inputData);
            if (succeeded) output = infoStore.output;
            return succeeded;
        }
    }
}
