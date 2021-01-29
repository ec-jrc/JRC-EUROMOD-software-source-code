using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;

namespace EM_UI.VariablesAdministration
{
    internal partial class SwitchablePoliciesForm : Form
    {
        VarConfigFacade _varConfigFacade = null;
        const string _noUpdateInCountriesWarning = "Please note that switches in the country files are not automatically updated!\n\nTo update them please open the 'Set Policy Switches' dialog (button 'Policy Switches' in ribbon 'Country Tools') and close it with 'OK'.";

        void SwitchablePoliciesForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;

            if (EM_AppContext.Instance.IsVariablesFormOpen())
            {
                UserInfoHandler.ShowInfo("Please close the administration tool for variables before administrating switchable policies.");
                Close();
            }

            if (!OptionalWarningsManager.Show(OptionalWarningsManager._administrateSwitchablePoliciesWarning))
                Close();
        }

        internal SwitchablePoliciesForm(VarConfigFacade varConfigFacade)
        {
            InitializeComponent();

            try
            {
                _varConfigFacade = varConfigFacade;

                foreach (VarConfig.SwitchablePolicyRow switchablePolicy in _varConfigFacade.GetSwitchablePolicies())
                {
                    int index = dgvSwitchablePolicies.Rows.Add(switchablePolicy.LongName, switchablePolicy.NamePattern);
                    dgvSwitchablePolicies.Rows[index].Tag = switchablePolicy;
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                int index = dgvSwitchablePolicies.Rows.Add(string.Empty, string.Empty);
                dgvSwitchablePolicies.Rows[index].Tag = _varConfigFacade.AddSwitchablePolicy();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvSwitchablePolicies.SelectedRows.Count != 1)
                return;

            try
            {
                //deleting an existing switchable policy may need to require an update of country files (which is too complex here)
                //example: delete bun*_??: all switches defined for the matching policies of the country would have to be changed from 'switch' to 'toggle'
                //removal in the country files however only takes place on closing the SetPolicySwitchesForm with 'OK' - thus this warning
                VarConfig.SwitchablePolicyRow switchablePolicyRow = (dgvSwitchablePolicies.SelectedRows[0].Tag as VarConfig.SwitchablePolicyRow);
                if (switchablePolicyRow.RowState != DataRowState.Added && UserInfoHandler.GetInfo(_noUpdateInCountriesWarning, MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                    return;

                switchablePolicyRow.Delete();
                dgvSwitchablePolicies.Rows.Remove(dgvSwitchablePolicies.SelectedRows[0]);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }

            dgvSwitchablePolicies.Update(); //if gridview is not updated it looks weired (text of two rows are displayed in one)
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow row in dgvSwitchablePolicies.Rows)
                {
                    object valueLongName = row.Cells[colLongName.Name].Value;
                    object valueNamePattern = row.Cells[colNamePattern.Name].Value;
                    if (valueLongName == null || valueLongName.ToString().Trim() == string.Empty ||
                        valueNamePattern == null || valueNamePattern.ToString().Trim() == string.Empty)
                    {
                        UserInfoHandler.ShowError("Row " + row.Index.ToString() + " is not completely defined. Please complete or delete the row.");
                        return;
                    }
                }

                _varConfigFacade._varConfig.AcceptChanges();
                _varConfigFacade.WriteXML();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void dgvSwitchablePolicies_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0 || dgvSwitchablePolicies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value == null)
                return;

            try
            {
                string newValue = dgvSwitchablePolicies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
                VarConfig.SwitchablePolicyRow switchablePolicy = dgvSwitchablePolicies.Rows[e.RowIndex].Tag as VarConfig.SwitchablePolicyRow;

                //changing the name pattern (e.g. yem_??) of an existing switchable policy may need to require an update of country files (which is too complex here)
                //example: change yem_?? to yem*_??: the switches already set for this country would apply to more policies (the long name is not visible, therefore no problem changing it)
                //updating in the country files however only takes place on closing the SetPolicySwitchesForm with 'OK' - thus this warning
                if (e.ColumnIndex == colNamePattern.Index &&
                    switchablePolicy.RowState != DataRowState.Added && 
                    newValue != switchablePolicy.NamePattern &&
                    UserInfoHandler.GetInfo(_noUpdateInCountriesWarning, MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.Cancel)
                {
                    dgvSwitchablePolicies.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = switchablePolicy.NamePattern;
                    return;
                }

                if (e.ColumnIndex == colLongName.Index)
                    switchablePolicy.LongName = newValue;
                else if (e.ColumnIndex == colNamePattern.Index)
                    switchablePolicy.NamePattern = newValue;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                _varConfigFacade.RejectSwitchablePolicyChanges();
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }

            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
