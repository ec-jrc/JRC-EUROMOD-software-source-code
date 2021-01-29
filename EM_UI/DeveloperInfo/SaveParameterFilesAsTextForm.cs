using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo
{
    internal partial class SaveParameterFilesAsTextForm : Form
    {
        const string textFormat = "TextFormat";
        const string withLineBreaks = "WithLineBreaks";

        internal SaveParameterFilesAsTextForm()
        {
            InitializeComponent();

            txtFolder.Text = EM_AppContext.FolderOutput;

            foreach (Country country in CountryAdministrator.GetCountries())
                lvCountries.Items.Add(country._shortName);

            CheckCountries(true);
        }

        internal List<string> GetSelectedCountries()
        {
            List<string> selectedCountries = new List<string>();
            foreach (ListViewItem listViewItem in lvCountries.Items)
            {
                if (listViewItem.Checked == true)
                {
                    selectedCountries.Add(listViewItem.Text);
                }
            }
            return selectedCountries;
        }

        void SaveParameterFilesAsTextForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (GetSelectedCountries().Count == 0)
            {
                UserInfoHandler.ShowError("Please select one or more countries.");
                return;
            }

            try
            {
                string exportPath = EMPath.AddSlash(txtFolder.Text);
                if (!Directory.Exists(exportPath))
                {
                    UserInfoHandler.ShowError("'" + txtFolder.Text + "' is not a valid path.");
                    return;
                }

                List<string> arguments = new List<string>();
                arguments.Add(txtFolder.Text);
                arguments.Add(radTextFormat.Checked ? textFormat : withLineBreaks);
                arguments.AddRange(GetSelectedCountries());
                ProgressIndicator progressIndicator = new ProgressIndicator(SaveAsText_BackgroundEventHandler, "Saving parameter files formatted ...", arguments);
                if (progressIndicator.ShowDialog() == DialogResult.Cancel)
                    return;

                if (EM_Helpers.SaveConvertToBoolean(progressIndicator.Result) == true)
                    UserInfoHandler.ShowSuccess("Successfully stored parameter files formatted at " + txtFolder.Text + ".");
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void SaveAsText_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker backgroundWorker = sender as BackgroundWorker;

                List<string> arguments = e.Argument as List<string>;
                string exportPath = arguments.ElementAt(0);
                string format = arguments.ElementAt(1);
                List<string> selectedCountries = arguments.GetRange(2, arguments.Count - 2);

                int actionCounter = 0;
                foreach (string countryShortName in selectedCountries)
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button
                    backgroundWorker.ReportProgress(Convert.ToInt32((actionCounter++ + 1) / (selectedCountries.Count * 1.0) * 100.0));

                    bool success = (format == textFormat) ?
                        CountryAdministrator.SaveAsTabDelimitedTextFile(exportPath, countryShortName) :
                        CountryAdministrator.SaveWithLineBreaks(exportPath, countryShortName);
                    if (!success)
                    {
                        e.Result = false;
                        e.Cancel = true;
                        return;
                    }
                }
                e.Result = true;
            }
            catch (Exception exception)
            {
                e.Result = false;
                e.Cancel = true;
                UserInfoHandler.ShowException(exception);
            }
        }

        void btnAll_Click(object sender, EventArgs e) { CheckCountries(true); }
        void btnNo_Click(object sender, EventArgs e) { CheckCountries(false); }
        void CheckCountries(bool checkState)
        {
            for (int i = 0; i < lvCountries.Items.Count; ++i )
                lvCountries.Items[i].Checked = checkState;
        }

        void btnSelectExportFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please choose the Export folder";
            folderBrowserDialog.SelectedPath = txtFolder.Text;
            if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                txtFolder.Text = folderBrowserDialog.SelectedPath;
        }
    }
}
