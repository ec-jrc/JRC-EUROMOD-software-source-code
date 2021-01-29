using EM_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace EM_UI.Run
{
    internal partial class Run
    {
        RunManager _runManager = null;
        internal string _em2ConfigurationFile = string.Empty;
        internal int _rowInRunForm = -1;
        internal string _runFormInfoText = string.Empty;

        BackgroundWorker _backgroundWorker = null;
        internal Process _process = null;
        internal string _processStatus = _processStatus_Queued;
        internal string _processStatusAdditionalInfo = string.Empty;

        private object _runLogLock = new object();
        private List<string> _runLog = null;
        internal List<string> RunLog 
        { 
            get 
            {
                lock (_runLogLock)
                {
                    return new List<string>(_runLog);
                }
            } 
        }
        const int MAXENTRIES_RUNLOG = 50;

        private object _errorLogLock = new object();
        internal List<string> _errorLog = null;
        internal List<string> ErrorLog
        {
            get
            {
                lock (_errorLogLock)
                {
                    return new List<string>(_errorLog);
                }
            }
        }
        const int MAXENTRIES_ERRORLOG = 50;

        internal const string _processStatus_Queued = "queued";
        internal const string _processStatus_Running = "running";
        internal const string _processStatus_Finished = "finished";
        internal const string _processStatus_Aborted = "aborted";

        static int _progressCounter = 0;

        internal string _configId = "";
        internal Dictionary<string, string> _contentConfig = new Dictionary<string, string>();

        void RunLogHandler(object sender, DataReceivedEventArgs drea) { RunLogHandler(drea.Data); }
        void RunLogHandler(string outLine, Dictionary<string, string> detailedInfo = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outLine))
                    return;

                lock (_runLogLock)
                {
                    if (_runLog.Count > MAXENTRIES_RUNLOG)
                        _runLog.RemoveAt(0); //if the log is not kept relatively small, outputting takes so long that the end-process signal is missed
                    _runLog.Add(outLine);

                    em3RunInfo.ExtractOutputFiles(detailedInfo); // note: this will not be filled if _runExeViaLib=false (info could be obtained; but seems not worth the effort)
                }

                _backgroundWorker.ReportProgress(_progressCounter++); //invokes BackgroundWorker_ProgressChanged (which informs RunManager about the change)
                                                                      //necessary because, if _runManager.HandleLogChanged (the action of BackgroundWorker_ProgressChanged)
                                                                      //is called directly the programme complains about a not valid cross-thread operation
                                                                      //_progressCounter is a dummy, as the ReportProgress functions needs to be fed with augmenting numbers (produces an error otherwise)
            }
            catch (Exception exception)
            {
                //do nothing, just catch to not jeopardise the run just because of logging
                Tools.UserInfoHandler.RecordIgnoredException("Run.RunLogHandler", exception);
            }
        }

        void ErrorLogHandler(object sender, DataReceivedEventArgs drea) { ErrorLogHandler(drea.Data); }
        void ErrorLogHandler(string outLine, Communicator.ErrorInfo errorInfo = null)
        {
            try
            {
                if (string.IsNullOrEmpty(outLine))
                    return;

                lock (_errorLogLock)
                {
                    _errorLog.Add(outLine);
                    em3RunInfo.errorInfo.Add(errorInfo ?? new Communicator.ErrorInfo() { isWarning = false, message = outLine });
                    if (_runLog.Count > MAXENTRIES_ERRORLOG)
                        _runLog.RemoveAt(0); //if the log is not kept relatively small, outputting takes so long that the end-process signal is missed
                }                            //this may have to be reconsidered if the UI overtakes the whole reporting (instead of providing just a double of the executable's reporting)

                _backgroundWorker.ReportProgress(_progressCounter++); //invokes BackgroundWorker_ProgressChanged - see RunLogHandler
            }
            catch (Exception exception)
            {
                //do nothing - see RunLogHandler
                Tools.UserInfoHandler.RecordIgnoredException("Run.ErrorLogHandler", exception);
            }
        }

        internal Run(RunManager runManager, string configurationFile)
        {
            _runManager = runManager;
            _em2ConfigurationFile = configurationFile;
        }

        internal bool StartRun()
        {
            try
            {
                _backgroundWorker = new BackgroundWorker();
                _backgroundWorker.DoWork += new DoWorkEventHandler(BackgroundWorker_DoWork); //handler for starting activities
                _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(BackgroundWorker_ProgressChanged); //handler for progress report activities (error-log, run-log)
                _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BackgroundWorker_RunWorkerCompleted); //handler for terminating activities
                _backgroundWorker.WorkerReportsProgress = true;
                if (EM_AppContext.Instance._runExeViaLib && !_runManager._runEM2) _backgroundWorker.WorkerSupportsCancellation = true;
                _backgroundWorker.RunWorkerAsync();
                _processStatus = _processStatus_Running;
                return true;
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
                return false;
            }
        }

        void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _runManager.HandleLogChanged(this);
            return;
        }

        void BackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            lock (_runLogLock) { _runLog = new List<string>(); } lock (_errorLogLock) { _errorLog = new List<string>(); }
            try
            {
                if (_runManager._runEM2) RunEM2();
                else RunEM3(_em2ConfigurationFile);
            }
            catch (Exception exception) { lock (_errorLogLock) { _errorLog.Add(exception.Message); } }
        }

        private void RunEM2()
        {
            _process = new Process();
            _process.StartInfo.FileName = EnvironmentInfo.GetEM2ExecutableFile();
            _process.StartInfo.Arguments += EnvironmentInfo.EncloseByQuotes(_em2ConfigurationFile);
            _process.StartInfo.CreateNoWindow = true;

            //redirect the standard-output and standard-error of the process, to be asynchronously read by event handlers
            _process.StartInfo.UseShellExecute = false;
            _process.StartInfo.RedirectStandardOutput = true;
            _process.StartInfo.RedirectStandardError = true;
            _process.OutputDataReceived += new DataReceivedEventHandler(RunLogHandler);
            _process.ErrorDataReceived += new DataReceivedEventHandler(ErrorLogHandler);

            //out-commented the following part and using a BackgroundWorker instead, as the callback function works from the processes thread
            //thus accessing the RunInfo-window fails
            //set event handler to recognise termination
            //_process.EnableRaisingEvents = true;
            //_process.Exited += new EventHandler(ExitedHandler);

            //start the process and start the asynchronous read of the output
            if (!_process.Start())
            {
                lock (_errorLogLock) { _errorLog.Add("Failed to start model run."); } //should not happen
                _runManager.HandleRunExited(this);
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
            _processStatusAdditionalInfo = string.Format("{0:T}", _process.StartTime) + " - ";

            _process.WaitForExit();
        }

        void BackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            try
            {
                bool success = _runManager._runEM2 ? _process.ExitCode == 1 :
                    (EM_AppContext.Instance._runExeViaLib ? em3RunInfo.finishStatus == RunLogger.RunInfo.FINISH_STATUS.finished : _process.ExitCode == 0);

                if (_processStatus == _processStatus_Aborted || !success)
                    _processStatus = _processStatus_Aborted;
                else
                    _processStatus = _processStatus_Finished;

                DateTime startTime = EM_AppContext.Instance._runExeViaLib && !_runManager._runEM2 ? em3RunInfo.duration.GetStartTime_dt() : _process.StartTime;
                DateTime exitTime = EM_AppContext.Instance._runExeViaLib && !_runManager._runEM2 ? em3RunInfo.duration.GetEndTime_dt() : _process.ExitTime;
                _processStatusAdditionalInfo = string.Format("{0:T}", startTime) + " - " + string.Format("{0:T}", exitTime) +
                    " (" + TimeSpanToHHMMSS(exitTime - startTime) + ")";

                _runManager.HandleRunExited(this);
            }
            catch (Exception exception)
            {
                lock (_errorLogLock)
                {
                    _errorLog.Add(exception.Message);
                }
            }

            string TimeSpanToHHMMSS(TimeSpan timeSpan) { return string.Format("{0:D2}h:{1:D2}m:{2:D2}s", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds); }
        }

        internal void StopRun()
        {
            try { if (EM_AppContext.Instance._runExeViaLib && !_runManager._runEM2) EM3_StopRun(); else EM2_StopRun(); }
            catch (Exception exception) { Tools.UserInfoHandler.ShowException(exception); }
        }

        private void EM2_StopRun()
        {
            if (_process == null || _process.HasExited == true) return;
            _process.Kill(); _processStatus = _processStatus_Aborted;
        }

        internal bool HasErrorLog()
        {
            lock (_errorLogLock)
            {
                return _errorLog != null && _errorLog.Count > 0;
            }
        }

        internal bool HasRunLog()
        {
            lock (_runLogLock)
            {
                return _runLog != null && _runLog.Count > 0;
            }
        }

        internal void AddNewExeError(List<string> errors)
        {
            _errorLog.Add(Environment.NewLine + "*** NEW EXECUTABLE ****");
            foreach (string error in errors) _errorLog.Add(error);
        }
    }
}
