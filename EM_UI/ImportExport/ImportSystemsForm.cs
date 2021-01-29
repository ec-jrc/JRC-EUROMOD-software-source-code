using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;

namespace EM_UI.ImportExport
{
    internal partial class ImportSystemsForm : Form
    {
        internal Country _importCountry = null;
        internal List<string> _selectedSystems = null;
        internal bool _machtByID = false;
        internal bool _fromAddOn = false;

        internal ImportSystemsForm(bool fromAddOn)
        {
            InitializeComponent();
            _fromAddOn = fromAddOn;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (lstSystems.CheckedItems.Count == 0)
            {
                Tools.UserInfoHandler.ShowError("Please select at least one system.");
                return;
            }

            _selectedSystems = new List<string>();
            foreach (object selectedSystem in lstSystems.CheckedItems)
                _selectedSystems.Add(selectedSystem.ToString());

            _machtByID = chkMatchByUniqueIdentifier.Enabled == true && chkMatchByUniqueIdentifier.Checked == true;

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnSelectImportFolder_Click(object sender, EventArgs e)
        {
            string path = string.Empty;

            if (CountryAdministrator.ConsiderOldAddOnFileStructure(_fromAddOn))
            {
                _importCountry = ImportExportAdministrator.GetImportAddOn_OldStyle(out path);
            }
            else
            { 
                _importCountry = ImportExportAdministrator.GetImportCountry(out path,_fromAddOn);    
            }
            if (_importCountry == null)
                return;

            txtImportFolder.Text = path;
            displayCountrySystems();
        }

        private void btnCheckPath_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            bool hasInsertedPath = true;
            string selectedPath = String.Empty;
            string insertedPath = txtImportFolder.Text;

            if(insertedPath == null || insertedPath == "") {
                Tools.UserInfoHandler.ShowError("Please insert or select a folder.");
                this.Cursor = Cursors.Default;
                return;
            }

            if (CountryAdministrator.ConsiderOldAddOnFileStructure(_fromAddOn))
                _importCountry = ImportExportAdministrator.GetImportAddOn_OldStyle(out selectedPath, hasInsertedPath, insertedPath);
            else
                _importCountry = ImportExportAdministrator.GetImportCountry(out selectedPath, _fromAddOn, hasInsertedPath, insertedPath);
            if (_importCountry == null)
            {
                this.Cursor = Cursors.Default;
                return;
            }
                

            displayCountrySystems();
            this.Cursor = Cursors.Default;
        }

        void displayCountrySystems()
        {
            //list systems
            lstSystems.Items.Clear();
            List<CountryConfig.SystemRow> importSystemRows = _importCountry.GetCountryConfigFacade().GetSystemRows();
            foreach (CountryConfig.SystemRow systemRow in importSystemRows)
                lstSystems.Items.Add(systemRow.Name);

            //if the country to import contains at least one system with the same unique ID as a system in the active country a match by unique ID is principally possible
            CountryConfigFacade activeCountryConfigFacade = CountryAdministrator.GetCountryConfigFacade(EM_AppContext.Instance.GetActiveCountryMainForm().GetCountryShortName());
            chkMatchByUniqueIdentifier.Enabled = ImportByIDAssistant.DoesMatchSystemExist(activeCountryConfigFacade.GetSystemRows(), importSystemRows);
        }

        private void btnAllSystems_Click(object sender, EventArgs e) { btnAllNoSystems_Click(true); }
        private void btnNoSystem_Click(object sender, EventArgs e) { btnAllNoSystems_Click(false); }
        private void btnAllNoSystems_Click(bool sel) { for (int i = 0; i < lstSystems.Items.Count; ++i) lstSystems.SetItemChecked(i, sel); }

        internal string GetImportFolder() { return txtImportFolder.Text; }
    }
}
