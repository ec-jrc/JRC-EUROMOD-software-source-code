using DevExpress.Office.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class ImportHHForm : Form
    {
        private Program plugin;
        private string importProjectPath;
        private DataSet importHHData = new DataSet();

        public ImportHHForm()
        {
            InitializeComponent();
        }

        private class HHStructure { internal DataRow hhRow; internal List<DataRow> memberRows = new List<DataRow>(); }

        internal bool Init(Program plugin, string importProjectPath)
        {
            try
            {
                this.plugin = plugin;
                this.importProjectPath = importProjectPath;

                importHHData.ReadXml(plugin.getHouseholdFile(importProjectPath));

                Dictionary<int, HHStructure> hhStructures = new Dictionary<int, HHStructure>();
                foreach (DataRow row in importHHData.Tables[plugin.HOUSEHOLD_STRUCTURE_TABLE].Rows)
                {
                    int parentId = row.Field<int>("ParentID");
                    if (parentId == -1) hhStructures.Add(row.Field<int>("ID"), new HHStructure() { hhRow = row });
                    else hhStructures[parentId].memberRows.Add(row);
                }

                foreach (HHStructure hhStructure in hhStructures.Values)
                {
                    string description = hhStructure.memberRows.Count + " members: ";
                    foreach (DataRow memberRow in hhStructure.memberRows) description += memberRow.Field<string>("HouseholdName") + ", ";
                    int r = gridHH.Rows.Add(hhStructure.hhRow.Field<string>("HouseholdName"), true, description.TrimEnd(new char[] { ',', ' ' }));
                    gridHH.Rows[r].Tag = hhStructure;
                }
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Failed to read household data of HHOT project {0}:",
                    Path.GetFileName(importProjectPath)) + Environment.NewLine + exception.Message);
                return false;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // remove/add variables, adapt decimal separator, etc.
                plugin.AdaptHouseholdDataToSettings(importHHData);

                // take care of renaming households where an equally named household exists, e.g. Example_Household (1)
                List<string> existingHHs = new List<string>();
                foreach (DataTable dt in plugin.householdData.Tables) existingHHs.Add(dt.TableName.ToLower().Trim());

                foreach (DataGridViewRow dgvRow in gridHH.Rows)
                {
                    if (Convert.ToBoolean(dgvRow.Cells[colGet.Name].Value) == false) continue; // user does not want to overtake the HH
                    HHStructure hhStructure = dgvRow.Tag as HHStructure;
                    string oldName = dgvRow.Cells[colHHName.Name].Value.ToString();
                    string newName = GetUniqueName(oldName, existingHHs);
                    plugin.copyHousehold(oldName, newName, importHHData);
                }
                plugin.saveHouseholdData();

                Cursor = Cursors.Default;
                MessageBox.Show("Import succeeded - extended household data saved.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                plugin.householdData.RejectChanges();
                Cursor = Cursors.Default;
                MessageBox.Show(string.Format("Failed to import households from HHOT project {0}:",
                    Path.GetFileName(importProjectPath)) + Environment.NewLine + exception.Message);
                DialogResult = DialogResult.Cancel;
            }
            Close();
        }

        internal static string GetUniqueName(string name, List<string> existingNames)
        {
            name = name.Trim();
            while (existingNames.Contains(name.ToLower()))
            {
                if (name.EndsWith(")"))
                {
                    int openB = name.LastIndexOf('(');
                    if (openB >= 0)
                    {
                        int counter;
                        if (int.TryParse(name.Substring(openB + 1, name.Length - openB - 2), out counter))
                        {
                            name = name.Substring(0, openB) + "(" + (counter + 1).ToString() + ")";
                            continue;
                        }
                    }
                }
                name += " (1)";
            }
            existingNames.Add(name.ToLower());
            return name;
        }

        private void btnGetAll_Click(object sender, EventArgs e) { btnGet_Click(true); }
        private void btnGetNone_Click(object sender, EventArgs e) { btnGet_Click(false); }       
        private void btnGet_Click(bool get) { foreach (DataGridViewRow row in gridHH.Rows) row.Cells[colGet.Name].Value = get; }
    }
}
