using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class AssignSystemsForm : Form
    {
        const string _labelNotAssigned = "Not Assigned";
        Dictionary<string, string> _systemAssignments = new Dictionary<string, string>();

        void AssignSystemsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            _systemAssignments.Clear();
            foreach (DataGridViewRow row in dgvAssign.Rows)
                if (row.Cells[1].Tag.ToString() != _labelNotAssigned)
                    _systemAssignments.Add(row.Cells[0].Tag.ToString(), row.Cells[1].Tag.ToString());

            DialogResult = DialogResult.OK;
            Close();
        }
       
        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        internal AssignSystemsForm(string pasteCountryShortName, Dictionary<string, string> pasteCountrySystems_NamesAndIDs,
                                   string copyCountryShortName, Dictionary<string, string> copyCountrySystems_NamesAndIDs)
        {
            InitializeComponent();
            txCopyCountry.Text = copyCountryShortName;
            txPasteCountry.Text = pasteCountryShortName + " <- " + copyCountryShortName;

            foreach (string pasteCountrySystemName in pasteCountrySystems_NamesAndIDs.Keys)
            {
                int index = dgvAssign.Rows.Add(pasteCountrySystemName, _labelNotAssigned);
                dgvAssign.Rows[index].Cells[0].Tag = pasteCountrySystems_NamesAndIDs[pasteCountrySystemName];
                dgvAssign.Rows[index].Cells[1].Tag = _labelNotAssigned;
            }

            TryAssignement(copyCountrySystems_NamesAndIDs);
        }

        internal Dictionary<string, string> GetSystemAssignment()
        {
            return _systemAssignments;
        }

        void dgvAssign_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != colAssign.Index)
                return;

            List<string> checkedSystemNames = new List<string>();
            checkedSystemNames.Add(dgvAssign.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());

            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(txCopyCountry.Text);
            selectSystemsForm.CheckSystems(checkedSystemNames);
            selectSystemsForm.SetSingleSelectionMode(false);
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;

            if (selectSystemsForm.GetSelectedSystemRows().Count > 0)
            {
                dgvAssign.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = selectSystemsForm.GetSelectedSystemRows().First().Name;
                dgvAssign.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag = selectSystemsForm.GetSelectedSystemRows().First().ID;
            }
            else
            {
                dgvAssign.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = _labelNotAssigned;
                dgvAssign.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag = _labelNotAssigned;
            }
        }

        void btnClear_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvAssign.Rows)
                row.Cells[1].Value = row.Cells[1].Tag = _labelNotAssigned;
        }

        void TryAssignement(Dictionary<string, string> copyCountrySystems_NamesAndIDs) //try to match systems with the same policy year for the convenience of the user
        {
            //generate a list of year-system name-pairs (e.g. 2001 - uk_2001; 2002 - uk_2002, etc.) for the systems of the origin country
            Dictionary<string, string> copyCountrySystem_YearsAndNames = new Dictionary<string, string>();
            foreach (string systemName in copyCountrySystems_NamesAndIDs.Keys)
            {
                string year = GetSystemYear(systemName);
                if (year != string.Empty && !copyCountrySystem_YearsAndNames.Keys.Contains(year)) //add only the first system with this year (considering uk_2001a, uk_2001b) as the dictionary cannot contain equal values ...
                    copyCountrySystem_YearsAndNames.Add(year, systemName); //... and it's anyway arbitrary which system to assign
            }

            //compare system years of the destination country and use the list to accomplish possible assignements
            foreach (DataGridViewRow row in dgvAssign.Rows)
            {
                string year = GetSystemYear(row.Cells[0].Value.ToString());
                if (year != string.Empty && copyCountrySystem_YearsAndNames.Keys.Contains(year))
                {
                    row.Cells[1].Value = copyCountrySystem_YearsAndNames[year]; //system name
                    row.Cells[1].Tag = copyCountrySystems_NamesAndIDs[copyCountrySystem_YearsAndNames[year]]; //system ID
                }   
            }
        }

        string GetSystemYear(string systemName)
        {
            if (!systemName.Contains("20"))
                return string.Empty;
            int index = systemName.IndexOf("20");
            if (systemName.Length < index + 4 || !EM_Helpers.IsNonNegInteger(systemName.Substring(index, 4)))
                return string.Empty;
            return systemName.Substring(index, 4);
        }
    }
}
