using EM_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EM_UI.Run
{
    internal partial class RunInfoForm : Form
    {
        RunManager _runManager;
        
        internal const int _colConfiguration = 5;  //the TableLayoutPanel's RightToLeft property is set to yes
        internal const int _colStatus = 4;         //because this allows autosizing the leftmost column (i.e. the configuration column)
        internal const int _colInfo = 3;           //which is the only column where the width is content dependent
        internal const int _colShowRunLog = 2;     //this however means that everything is right-left reversed (e.g. _colConfiguration is actually the first column)
        internal const int _colShowErrorLog = 1;   
        internal const int _colStop = 0;

        Control GetControlByIndex(int columnIndex, int rowIndex)
        {
            foreach (Control control in tableRunInfo.Controls)
            {
                int ri = tableRunInfo.GetRow(control);
                int ci = tableRunInfo.GetColumn(control);
                if (tableRunInfo.GetRow(control) == rowIndex && tableRunInfo.GetColumn(control) == columnIndex)
                    return control;
            }
            return null;
        }

        Label GetLabelByIndex(int columnIndex, int rowIndex)
        {
            return GetControlByIndex(columnIndex, rowIndex) as Label;
        }

        Button GetButtonByIndex(int columnIndex, int rowIndex)
        {
            return GetControlByIndex(columnIndex, rowIndex) as Button;
        }

        Label GetLayoutedLabel(string text)
        {
            Label label = new Label();
            label.Text = text;
            label.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.RightToLeft = System.Windows.Forms.RightToLeft.No;
            return label;
        }

        Button GetLayoutedButton(string text, int column, int row)
        {
            Button button = new Button();
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            button.Text = text;
            button.Click += new EventHandler(_runManager.HandleInfoForm_ButtonClick);
            Dictionary<int, int> columnRowPair = new Dictionary<int, int>();
            columnRowPair.Add(column, row);
            button.Tag = columnRowPair;
            button.RightToLeft = System.Windows.Forms.RightToLeft.No;
            return button;
        }

        void RunInfoForm_Load(object sender, EventArgs e)
        {
            //ClientSize = new Size(ClientSize.Width, tableRunInfo.Height + 25);
            Text = DefGeneral.BRAND_TITLE + " Run started " + DateTime.Now.ToString();
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void RunInfoForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_runManager.Close(true))
                e.Cancel = true;
        }

        internal RunInfoForm(RunManager runManager)
        {
            _runManager = runManager;
            InitializeComponent();
        }

        internal int AddInfoRow(string infoText)
        {
            ++tableRunInfo.RowCount;
            int rowIndex = tableRunInfo.RowCount - 1;

            tableRunInfo.Controls.Add(GetLayoutedLabel(infoText), _colConfiguration, rowIndex);
            tableRunInfo.Controls.Add(GetLayoutedLabel(string.Empty), _colStatus, rowIndex);
            tableRunInfo.Controls.Add(GetLayoutedLabel(string.Empty), _colInfo, rowIndex);

            tableRunInfo.Controls.Add(GetLayoutedButton("Run Log", _colShowRunLog, rowIndex), _colShowRunLog, rowIndex);
            tableRunInfo.Controls.Add(GetLayoutedButton("Error Log", _colShowErrorLog, rowIndex), _colShowErrorLog, rowIndex);
            tableRunInfo.Controls.Add(GetLayoutedButton("Stop", _colStop, rowIndex), _colStop, rowIndex);

            return rowIndex;
        }

        delegate void SetStatusCallback(int row, string status, string info);

        internal void SetStatus(int row, string status, string info)
        {
            if (GetLabelByIndex(_colStatus, row).InvokeRequired || GetLabelByIndex(_colInfo, row).InvokeRequired)  // if the call is coming from a different thread, use Invoke to make this thread-safe
            {
                SetStatusCallback d = new SetStatusCallback(SetStatus);
                this.Invoke(d, new object[] { row, status, info });
            }
            else
            {
                GetLabelByIndex(_colStatus, row).Text = status;
                GetLabelByIndex(_colInfo, row).Text = info;
            }
        }

        delegate void SetButtonStatusCallback(int column, int row, bool enabled, string text = "");

        internal void SetButtonStatus(int column, int row, bool enabled, string text = "")
        {
            Button button = GetButtonByIndex(column, row);
            if (button.InvokeRequired)  // if the call is coming from a different thread, use Invoke to make this thread-safe
            {
                SetButtonStatusCallback d = new SetButtonStatusCallback(SetButtonStatus);
                this.Invoke(d, new object[] { column, row, enabled, text });
            }
            else
            {
                button.Enabled = enabled;
                if (text != string.Empty)
                    button.Text = text;
            }
        }

        internal void SetLog(string header, string log)
        {
            labLogHeader.Text = header;
            txtLog.Text = log;
            txtLog.SelectionStart = txtLog.Text.Length;
            txtLog.ScrollToCaret();
        }
    }
}
