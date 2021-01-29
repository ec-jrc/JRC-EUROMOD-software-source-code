using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class ImportRefTablesForm : Form
    {
        private Program plugin;
        private string importProjectPath;
        ReferenceTablesDataSet importRefTabData = new ReferenceTablesDataSet();

        public ImportRefTablesForm()
        {
            InitializeComponent();
        }

        internal bool Init(Program plugin, string importProjectPath)
        {
            try
            {
                this.plugin = plugin;
                this.importProjectPath = importProjectPath;

                importRefTabData.ReadXml(plugin.getReferenceTablesFile(importProjectPath));
                foreach (ReferenceTablesDataSet.ReferenceTablesRow refTabInfo in importRefTabData.ReferenceTables)
                {
                    int r = gridRefTabs.Rows.Add(refTabInfo.TableDescription, true,
                        ImportVariablesForm.GetFieldStringValue(refTabInfo, "Comments"));
                    gridRefTabs.Rows[r].Tag = refTabInfo;
                }
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(string.Format("Failed to read reference table info of HHOT project {0}:",
                    Path.GetFileName(importProjectPath)) + Environment.NewLine + exception.Message);
                return false;
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                // adapt the reference tables of the import project to the loaded project (decimal sep., years and countries)
                plugin.fixRefTablesDecimalSeparator(importRefTabData);
                string dummy; plugin.adaptYearsAndCountriesOfReferenceTables(out dummy, importRefTabData);
                importRefTabData.AcceptChanges();

                // take care of renaming where a table with an equally description exists, e.g. EU-SILC average wage (1)
                List<string> existingDescriptions = new List<string>();
                foreach (var rt in plugin.referenceTablesData.ReferenceTables) existingDescriptions.Add(rt.TableDescription.ToLower().Trim());

                foreach (DataGridViewRow dgvRow in gridRefTabs.Rows)
                {
                    if (Convert.ToBoolean(dgvRow.Cells[colGet.Name].Value) == false) continue; // user does not want to overtake the table

                    // first copy the table from the import-dataset and add to plugin-dataset ...
                    ReferenceTablesDataSet.ReferenceTablesRow refTabInfo = dgvRow.Tag as ReferenceTablesDataSet.ReferenceTablesRow;
                    DataTable refTable = importRefTabData.Tables[refTabInfo.TableName].Copy();
                    refTable.TableName = SettingsManagement.ManageReferenceTables.getUniqueRefTableName(plugin);
                    plugin.referenceTablesData.Tables.Add(refTable);
                    // ... then add a respective row to the info-table
                    plugin.referenceTablesData.ReferenceTables.AddReferenceTablesRow(refTable.TableName,
                        ImportHHForm.GetUniqueName(refTabInfo.TableDescription, existingDescriptions),
                        ImportVariablesForm.GetFieldStringValue(refTabInfo, "IsEditable"), // true.ToString() /* I think this is not used */,
                        ImportVariablesForm.GetFieldStringValue(refTabInfo, "Comments"));
                }
                plugin.saveReferenceTables();

                Cursor = Cursors.Default;
                MessageBox.Show("Import succeeded - extended reference tables saved.");
                DialogResult = DialogResult.OK;
            }
            catch (Exception exception)
            {
                plugin.referenceTablesData.RejectChanges();
                Cursor = Cursors.Default;
                MessageBox.Show(string.Format("Failed to import reference tables from HHOT project {0}:",
                    Path.GetFileName(importProjectPath)) + Environment.NewLine + exception.Message);
                DialogResult = DialogResult.Cancel;
            }
            Close();
        }

        private void btnGetAll_Click(object sender, EventArgs e) { btnGet_Click(true); }
        private void btnGetNone_Click(object sender, EventArgs e) { btnGet_Click(false); }
        private void btnGet_Click(bool get) { foreach (DataGridViewRow row in gridRefTabs.Rows) row.Cells[colGet.Name].Value = get; }
    }
}
