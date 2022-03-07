using EM_BackEnd;
using EM_Common;
using EM_Common_Win;
using EM_Statistics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace StatisticsPresenter
{
    public partial class PresenterForm : Form
    {
        private BackEnd backEnd = new BackEnd();
        private string responderKey = null;
        private bool requiresWarmUp = true;

        public PresenterForm(Template template, List<FilePackageContent> filePackages, List<Template.TemplateInfo.UserVariable> userInput)
        {
            InitializeComponent();
            if (!string.IsNullOrEmpty(template?.info?.name)) Text = template.info.name;

            try
            {
                try
                {
                    if (requiresWarmUp)
                    {
                        requiresWarmUp = false;
                        string dummyHtml = Path.Combine(new EMPath(UISessionInfo.GetEuromodFilesFolder()).GetFolderTemp(), "dummy.html");
                        if (!File.Exists(dummyHtml)) File.WriteAllText(dummyHtml, Resources.dummy_html);
                        webBrowser.Url = new Uri(dummyHtml);
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error: " + e.Message);
                }

                EM_StatisticsBackEndResponder backEndResponder = new EM_StatisticsBackEndResponder(backEnd, template, filePackages, userInput);
                responderKey = backEndResponder.responderKey;
                backEnd.Start(new Dictionary<string, EM_BackEndResponder>() { { responderKey, backEndResponder } });
                webBrowser.Url = new Uri($"http://localhost:{backEnd.Port}/{responderKey}_{EM_StatisticsBackEndResponder.LOAD_STATISTICS_HTML}?packageKey={filePackages[0].Key}");
            }
            catch (Exception exception) { Error(exception.Message); }

            void Error(string message) { MessageBox.Show("Error starting Statistics Back-End:" + Environment.NewLine + message); }
        }

        private void PresenterForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (backEnd != null && backEnd.IsRunning) backEnd.Stop(new List<string>() { responderKey });
        }
    }
}
