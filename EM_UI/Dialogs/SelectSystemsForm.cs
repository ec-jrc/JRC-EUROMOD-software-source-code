using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.DataSets;
using EM_UI.CountryAdministration;

namespace EM_UI.Dialogs
{
    internal partial class SelectSystemsForm : Form
    {
        List<CountryConfig.SystemRow> _systemRows = null;
        bool _singleSelectionMode = false;
        bool _enforceSelection = false;
        int _lastCheckedItem = -1;
        const string ALL_SYSTEMS = "All Systems";
        
        void SelectSystemsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (_enforceSelection && lstSystems.CheckedItems.Count == 0)
            {
                EM_UI.Tools.UserInfoHandler.ShowError("Please select a system.");
                return;
            }

            for (int index = _systemRows.Count - 1; index >= 0; --index)
            {
                if (!lstSystems.CheckedIndices.Contains(index))
                    _systemRows.RemoveAt(index);
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void lstSystems_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            if (!_singleSelectionMode)
                return;
            if (e.CurrentValue != CheckState.Checked)
                if (_lastCheckedItem != -1)
                    lstSystems.SetItemChecked(_lastCheckedItem, false);
            _lastCheckedItem = e.Index;
        }

        internal SelectSystemsForm(string countryName, List<string> noShowSystemsIDs = null, bool showAllSystems = false)
        {
            InitializeComponent();

            _systemRows = new List<CountryConfig.SystemRow>();
            foreach (CountryConfig.SystemRow systemRow in CountryAdministrator.GetCountryConfigFacade(countryName).GetSystemRows())
            {
                if (noShowSystemsIDs == null || !noShowSystemsIDs.Contains(systemRow.ID))
                {
                    _systemRows.Add(systemRow);
                    lstSystems.Items.Add(systemRow.Name);
                }
            }
            if (showAllSystems) lstSystems.Items.Add(ALL_SYSTEMS);
        }

        internal void SetSingleSelectionMode(bool enforceSelection = true)
        {
            _singleSelectionMode = true;
            chkSelectAll.Visible = false;
            _enforceSelection = enforceSelection;
        }

        internal void SetCaption(string caption)
        {
            Text = caption;
        }

        internal void CheckSystems(List<string> systemNames)
        {
            List<int> listCheck = new List<int>();
            foreach (object systemName in lstSystems.Items)
                if (systemNames.Contains(systemName))
                    listCheck.Add(lstSystems.Items.IndexOf(systemName));
            foreach (int index in listCheck)
            {
                lstSystems.SetItemChecked(index, true);
                _lastCheckedItem = index;
            }
        }

        internal List<CountryConfig.SystemRow> GetSelectedSystemRows()
        {
            return _systemRows;
        }

        internal bool IsAllSystemsSelected()
        {
            foreach (string item in lstSystems.SelectedItems) if (item == ALL_SYSTEMS) return true;
            return false;
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < lstSystems.Items.Count; i++)
            {
            	lstSystems.SetItemChecked(i, chkSelectAll.Checked);
            }
        }
    }
}
