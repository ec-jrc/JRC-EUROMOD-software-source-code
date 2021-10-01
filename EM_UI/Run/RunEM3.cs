using EM_Common;
using EM_Crypt;
using EM_Transformer;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EM_UI.Run
{
    internal partial class Run
    {
        internal RunLogger.RunInfo em3RunInfo = null;
        internal bool em3Cancel = false;

        private void RunEM3(string configurationFile)
        {
            if (EM_AppContext.Instance._runExeViaLib) RunEM3_Lib(configurationFile);    // this should always be true, as _runExeViaLib is readonly
            else RunEM3_Exe(configurationFile);
        }

        private void RunEM3_Lib(string configurationFile)
        {
            em3RunInfo = new RunLogger.RunInfo(); DateTime startTime = DateTime.Now;

            if (!TransformEMConfig.Transform(configurationFile, out Dictionary<string, string> em3Config,
                                             new Action<string>(err => { ErrorLogHandler(err); }))) return;

            // If working in a secure environment, make sure the executable knows about this! ;)
            if (SecureInfo.IsSecure && !string.IsNullOrEmpty(SecureInfo.DataPassword))
            {
                em3Config.AddOrReplace(EM_XmlHandler.TAGS.CONFIG_DATA_PASSWORD, SecureInfo.DataPassword);
            }
                
            bool success = new EM_Executable.Control().Run(em3Config,
                progressInfo =>
                {
                    if (em3Cancel) return false;
                    RunLogHandler(progressInfo.message, progressInfo.detailedInfo); return true;
                },
                errorInfo => { ErrorLogHandler((errorInfo.isWarning ? "warning: " : "error: ") + errorInfo.message, errorInfo); });

            em3RunInfo.finishStatus = success ? RunLogger.RunInfo.FINISH_STATUS.finished : RunLogger.RunInfo.FINISH_STATUS.aborted;
            em3RunInfo.duration = new RunLogger.Duration(startTime, DateTime.Now);
        }

        private void RunEM3_Exe(string configurationFile)
        {
            _process = new Process();
            EnvironmentInfo.GetEM3ExecutableCallerInfo(_process.StartInfo);
            _process.StartInfo.Arguments += "UI_RUNTOOL_CALL ";
            _process.StartInfo.Arguments += EnvironmentInfo.EncloseByQuotes(_em2ConfigurationFile);

            _process.StartInfo.CreateNoWindow = true;
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.OutputDataReceived += new DataReceivedEventHandler(RunLogHandler);
            _process.ErrorDataReceived += new DataReceivedEventHandler(ErrorLogHandler);
            if (!_process.Start())
            {
                lock (_errorLogLock) { _errorLog.Add("Failed to start model run."); }
                _runManager.HandleRunExited(this);
            }
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _processStatusAdditionalInfo = string.Format("{0:T}", _process.StartTime) + " - ";
            _process.WaitForExit();
        }

        private void EM3_StopRun()
        {
            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
                _backgroundWorker.CancelAsync();
            if (em3RunInfo != null) em3Cancel = true;
        }
    }
}
