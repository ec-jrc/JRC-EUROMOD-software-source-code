using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.ImportExport
{
    public partial class CompareVersionsForm : Form
    {
        internal bool _fromAddOn = false;
        internal Country _importCountry = null;

        public CompareVersionsForm(bool fromAddOn)
        {
            InitializeComponent();
            _fromAddOn = fromAddOn;
        }

        private void CompareVersionsForm_Load(object sender, EventArgs e)
        {

        }

        private void btnSelectImportFolder_Click(object sender, EventArgs e)
        {

            string path = String.Empty;
            if (CountryAdministrator.ConsiderOldAddOnFileStructure(_fromAddOn))
            {
                ImportExportAdministrator.GetImportAddOnPath_OldStyle(out string addOnPath, out string addOnShortName, out string fileName);
                path = fileName;

            }
            else
            {
                ImportExportAdministrator.GetImportCountryPath(_fromAddOn, out string importPath, out string countryShortName);
                path = importPath;
            }

            txtImportFolder.Text = path;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (txtImportFolder.Text == string.Empty) { UserInfoHandler.ShowError("Please select a version to compare with."); return; }

            Cursor = Cursors.WaitCursor;
            bool hasInsertedPath = true;
            string insertedPath = txtImportFolder.Text;
            string selectedPath = String.Empty;
            if (CountryAdministrator.ConsiderOldAddOnFileStructure(_fromAddOn))
                _importCountry = ImportExportAdministrator.GetImportAddOn_OldStyle(out selectedPath, hasInsertedPath, insertedPath);
            else
                _importCountry = ImportExportAdministrator.GetImportCountry(out selectedPath, _fromAddOn, hasInsertedPath, insertedPath);
            if (_importCountry == null)
            { 
                Cursor = Cursors.Default;
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
