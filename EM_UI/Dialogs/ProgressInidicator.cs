using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ProgressIndicator : Form
    {
        string _title = string.Empty;
        object _argument = null;
        internal object Result { get; private set; }

        internal ProgressIndicator(DoWorkEventHandler doWorkEventHandler, string title = "", object argument = null)
        {
            InitializeComponent();
            _title = title;
            _argument = argument;
            this.Text = title;
            this.backgroundWorker.DoWork += doWorkEventHandler; //the action for which progress is reported needs to be provided by the caller
        }

        internal void HideCancelButton()
        {
            btnCancel.Visible = false;
        }

        void ProgressIndicator_Shown(object sender, System.EventArgs e)
        {
            backgroundWorker.RunWorkerAsync(_argument); //start the action
        }

        void btnCancel_Click(object sender, System.EventArgs e)
        {
            backgroundWorker.CancelAsync(); //stop the action
        }

        void backgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            int progress = e.ProgressPercentage > progressBar.Maximum ? progressBar.Maximum : e.ProgressPercentage;
            progress = progress < progressBar.Minimum ? progressBar.Minimum : progress;
            progressBar.Value = progress;
            Text = _title + " " + progressBar.Value.ToString() + " %";
        }

        void backgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            if ((e != null && e.Cancelled == true) || backgroundWorker.CancellationPending) //asking for CancellationPending probably not necessary as DoWork-event-handlers set e.Cancel
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel; //Cancel button was clicked or action unsuccessfully terminated
            else
            {
                this.DialogResult = System.Windows.Forms.DialogResult.OK; //action completed successfully
                Result = e == null ? true : e.Result;
            }
            Close();
        }
    }
}
