using System;
using System.Collections.Generic;
using System.Windows.Forms;
using EM_UI.Tools;

namespace EM_UI.Dialogs
{
    internal partial class DeleteCountryForm : Form
    {
        void DeleteCountryForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            int nSel = 0;
            foreach (ListViewItem it in lvCountries.Items)
            {
                if (it.Checked == true)
                {
                    ++nSel;
                    if (EM_AppContext.Instance.GetCountryMainForm(it.Text) != null)
                    {
                        EM_UI.Tools.UserInfoHandler.ShowError("A loaded country cannot be deleted. Please close '" + it.Text + "'.");
                        return;
                    }
                }
            }

            if (nSel > 0)
                DialogResult = DialogResult.OK;
            else
                DialogResult = DialogResult.Cancel;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        internal DeleteCountryForm(List<string> countries)
        {
            InitializeComponent();

            foreach (string country in countries)
                lvCountries.Items.Add(country);
        }

        internal List<string> GetSelectedCountries()
        {
            List<string> selectedCountries = new List<string>();
            foreach (ListViewItem listViewItem in lvCountries.Items)
            {
                if (listViewItem.Checked == true)
                    selectedCountries.Add(listViewItem.Text);
            }
            return selectedCountries;
        }

        internal bool GetAdaptGlobal()
        {
            return chkAdaptGlobal.Checked;
        }
    }
}
