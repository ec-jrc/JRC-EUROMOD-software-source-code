using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.IO;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class AddCountryForm : Form
    {
        private string saveAsOriginalCountry = null;
        private SaveAsAdaptOptions saveAsAdaptOptions = new SaveAsAdaptOptions();

        void AddCountryForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            string potCountryFolder = EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + txtShortName.Text;
            string potAddOnFolder = EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + txtShortName.Text;
            if (Directory.Exists(potCountryFolder) || Directory.Exists(potAddOnFolder))
            {
                UserInfoHandler.ShowError("Country or Add-on '" + txtShortName.Text + "' already exists.");
                return;
            }

            if (txtFlag.Text != string.Empty && !CountryAdministrator.IsValidFlagFilePath(txtFlag.Text))
                return;

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void btnFlag_Click(object sender, EventArgs e)
        {
            txtFlag.Text = CountryAdministrator.ShowFlagSelectDialog(txtFlag.Text);
        }

        internal AddCountryForm(bool isAddOn, string _originalCountry = null)
        {
            InitializeComponent();

            Text = isAddOn ? "Add Add-On" : "Add Country";
            labelFlag.Text = isAddOn ? "Symbol (png)" : "Flag (png)";

            saveAsOriginalCountry = _originalCountry;
            chkAdvancedAdaptations.Visible = btnConfigureAdvancedAdaptations.Visible = !isAddOn && saveAsOriginalCountry != null;
        }

        internal string GetCountryLongName()
        {
            return txtLongName.Text;
        }

        internal string GetCountryShortName()
        {
            return txtShortName.Text;
        }
        
        internal string GetFlagPathAndFileName()
        {
            return txtFlag.Text;
        }

        internal SaveAsAdaptOptions GetSaveAsAdaptOptions()
        {
            return chkAdvancedAdaptations.Checked ? saveAsAdaptOptions : null;
        }

        private void btnConfigureAdvancedAdaptations_Click(object sender, EventArgs e)
        {
            AddCountry_SaveAs_AdaptForm form = new AddCountry_SaveAs_AdaptForm(saveAsOriginalCountry, saveAsAdaptOptions);
            if (form.ShowDialog() == DialogResult.OK) saveAsAdaptOptions = form.options;
        }
    }   
}
