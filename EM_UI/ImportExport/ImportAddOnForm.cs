using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.ImportExport
{
    internal partial class ImportAddOnForm : Form
    {
        int _lastCheckedBaseSystem = -1; //for single check mode
        int _lastCheckedAddOnSystem = -1;

        List<string> _baseSystemNames = null; //input

        internal string _addOnShortName = string.Empty; //output
        internal string _addOnSystemName = string.Empty;
        internal string _baseSystemName = string.Empty;

        internal ImportAddOnForm(List<string> BaseSystemNames)
        {
            InitializeComponent();
            _baseSystemNames = BaseSystemNames;

            foreach (Country addOn in CountryAdministrator.GetAddOns())
                cmbAddOns.Items.Add(addOn._shortName);
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (lstAddOnSystems.CheckedItems.Count == 0 || lstBaseSystems.CheckedItems.Count == 0)
            {
                Tools.UserInfoHandler.ShowError("Please select an add-on system and a base system.");
                return;
            }
            _addOnShortName = cmbAddOns.Text;
            _addOnSystemName = lstAddOnSystems.SelectedItem.ToString();
            _baseSystemName = lstBaseSystems.SelectedItem.ToString();

            DialogResult = DialogResult.OK;
            Close();
        }

        void cmbAddOns_SelectedIndexChanged(object sender, EventArgs e)
        {//list only the systems of the add-on which support at least one base system
            lstAddOnSystems.Items.Clear();
            lstBaseSystems.Items.Clear();
            _lastCheckedBaseSystem = -1;
            _lastCheckedAddOnSystem = -1;

            if (cmbAddOns.Text == string.Empty)
                return;
            foreach (AddOnSystemInfo addOnSystemInfo in AddOnInfoHelper.GetAddOnSystemInfo(cmbAddOns.Text))
            {
                bool supportsAnySystems = false;
                foreach (string supportedSystemName in addOnSystemInfo._supportedSystems)
                {
                    foreach (string baseSystemName in _baseSystemNames)
                    {
                        if (EM_Helpers.DoesValueMatchPattern(supportedSystemName, baseSystemName))
                        {
                            supportsAnySystems = true;
                            break;
                        }
                    }
                    if (supportsAnySystems == true)
                        break;
                }
                if (supportsAnySystems == false)
                    continue;
                lstAddOnSystems.Items.Add(addOnSystemInfo._addOnSystemName);
            }
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

            //select the base systems which are supported by the checked add-on system
            lstBaseSystems.Items.Clear();
            if (e.CurrentValue == CheckState.Checked)
                return; //currently checked add-on system is unchecked

            foreach (AddOnSystemInfo addOnSystemInfo in AddOnInfoHelper.GetAddOnSystemInfo(cmbAddOns.Text))
            {
                if (addOnSystemInfo._addOnSystemName != lstAddOnSystems.SelectedItem.ToString())
                    continue;
                foreach (string supportedSystemName in addOnSystemInfo._supportedSystems)
                {
                    foreach (string baseSystemName in _baseSystemNames)
                    {
                        if (EM_Helpers.DoesValueMatchPattern(supportedSystemName, baseSystemName))
                            lstBaseSystems.Items.Add(baseSystemName);
                    }
                }
            }
        }
    }
}
