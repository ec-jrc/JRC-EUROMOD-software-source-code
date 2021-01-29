using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using EM_UI.Tools;
using EM_UI.CountryAdministration;

namespace EM_UI.ImportExport
{
    internal partial class ExportAddOnForm : Form
    {
        int _lastCheckedBaseSystem = -1; //for single check mode
        int _lastCheckedAddOnSystem = -1;

        List<string> _systemNames = null; //input

        internal string _addOnSystemName = string.Empty; //output
        internal string _baseSystemName = string.Empty;

        internal ExportAddOnForm(List<string> systemNames)
        {
            InitializeComponent();

            _systemNames = systemNames;
            foreach (string systemName in _systemNames)
            {
                lstBaseSystems.Items.Add(systemName);
                lstAddOnSystems.Items.Add(systemName);
            }
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (txtShortName.Text == string.Empty || txtLongName.Text == string.Empty)
            {
                Tools.UserInfoHandler.ShowError("Please indicate a short name and a long name for the add-on.");
                return;
            }

            if (CountryAdministrator.DoesCountryExist(txtShortName.Text))
            {
                Tools.UserInfoHandler.ShowError("Add-on named '" + txtShortName.Text + "' already exists.");
                return;
            }

            if (txtSymbol.Text != string.Empty  && !CountryAdministrator.IsValidFlagFilePath(txtSymbol.Text))
                return;

            if (lstAddOnSystems.CheckedItems.Count == 0 || lstBaseSystems.CheckedItems.Count == 0)
            {
                Tools.UserInfoHandler.ShowError("Please select an add-on system and a base system.");
                return;
            }
            _addOnSystemName = lstAddOnSystems.SelectedItem.ToString();
            _baseSystemName = lstBaseSystems.SelectedItem.ToString();

            if (_addOnSystemName == _baseSystemName)
            {
                Tools.UserInfoHandler.ShowError("Add-on system must be different from base system.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        void lstBaseSystems_ItemCheck(object sender, ItemCheckEventArgs e)
        {//for single selection mode
            if (e.CurrentValue != CheckState.Checked)
                if (_lastCheckedBaseSystem != -1)
                    lstBaseSystems.SetItemChecked(_lastCheckedBaseSystem, false);
            _lastCheckedBaseSystem = e.Index;
        }

        void lstAddOnSystems_ItemCheck(object sender, ItemCheckEventArgs e)
        {   //for single selection mode
            if (e.CurrentValue != CheckState.Checked)
                if (_lastCheckedAddOnSystem != -1)
                    lstAddOnSystems.SetItemChecked(_lastCheckedAddOnSystem, false);
            _lastCheckedAddOnSystem = e.Index;
        }

        void btnSymbol_Click(object sender, EventArgs e)
        {
            txtSymbol.Text = CountryAdministrator.ShowFlagSelectDialog(txtSymbol.Text);
        }
    }
}
