using System;
using System.Collections.Generic;

namespace EM_Common
{
    public partial class Communicator
    {
        /// <summary> information handed to ReportAction (to be interpreted by the caller) </summary>
        public class ErrorInfo
        {
            public bool isWarning = false;
            public string message = string.Empty;
            public string runTimeErrorId = string.Empty; // code for limiting run-time errors (see below)
            // perhaps other info
        }

        /// <summary> information handed to ReportAction (to be interpreted by the caller) </summary>
        public class ProgressInfo
        {
            public string id = string.Empty;
            public string message = string.Empty;
            public Dictionary<string, string> detailedInfo = null;
        }

        /// <summary> action provided by the calling programme, to be called on progress </summary>
        public Func<ProgressInfo, bool> progressAction = null;

        /// <summary> action provided by the calling programme, to be called on error </summary>
        public Action<ErrorInfo> errorAction = null;

        public int errorCount = 0;
        public int warningCount = 0;

        // allow for limiting run-time errors, if they would be issued for each person (e.g. upper limit always lower than lower limit)
        public int maxRunTimeErrors = 5; // 5 is only a default, in fact this is set via the configuration file
        private Dictionary<string, int> runTimeErrorCounts = new Dictionary<string, int>(); // key: id of error-issuer, value: count (up to max)

        object progressLock = new object();
        public bool ReportProgress(ProgressInfo info) { lock (progressLock) { return progressAction == null ? true : progressAction.Invoke(info); } }

        object errorLock = new object();

        public void ReportError(ErrorInfo info)
        {
            lock (errorLock)
            {
                if (info.runTimeErrorId != string.Empty)
                {
                    if (!runTimeErrorCounts.ContainsKey(info.runTimeErrorId)) runTimeErrorCounts.Add(info.runTimeErrorId, 0);
                    if (runTimeErrorCounts[info.runTimeErrorId] >= maxRunTimeErrors) return;
                    runTimeErrorCounts[info.runTimeErrorId]++;
                }
                if (errorAction != null) errorAction(info);
                else Console.WriteLine(info.message);

                if (info.isWarning) ++warningCount; else ++errorCount;
            }
        }
    }
}
