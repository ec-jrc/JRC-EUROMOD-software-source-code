using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HypotheticalHousehold.SettingsManagement
{
    public partial class ManageCountries: Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        internal bool changed = false;

        public ManageCountries(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;
            gridControlCountries.DataSource = Plugin.settingsData.Cur_Countries;
            gridView1.ValidatingEditor += gridView1_ValidatingEditor;
        }

        void gridView1_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e)
        {
            if (e.Value.ToString() == "")
            {
                gridView1.DeleteSelectedRows();
            }
        }

        private void ManageCountries_FormClosing(object sender, FormClosingEventArgs e)
        {
            btnClose.Focus();
            if (Plugin.settingsData.HasChanges())
            {
                DialogResult res = MessageBox.Show(this, "Do you want to save your changes?", "Save", MessageBoxButtons.YesNoCancel);
                if (res == System.Windows.Forms.DialogResult.Yes)
                {
                    Plugin.automaticlyUpdateReferenceTables();  // if the Countries changed then you probably also need to update all the Reference Tables!
                    Plugin.saveSettings();
                    changed = true;
                }
                else if (res == System.Windows.Forms.DialogResult.Cancel)
                    e.Cancel = true;
                else if (res == System.Windows.Forms.DialogResult.No)
                    Plugin.settingsData.RejectChanges();
            }
        }

        private void gridControlCountries_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                DeleteRow(null, null);
            }
        }

        private void DeleteRow(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Do you want to delete this record?", "Delete", MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
            {
                (gridControlCountries.FocusedView as DevExpress.XtraGrid.Views.Grid.GridView).DeleteSelectedRows();
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // remove all references of the deleted countries from variables
            List<string> removedCountries = new List<string>();
            foreach (DataRow r in Plugin.settingsData.Cur_Countries.Rows)
                if (r.RowState == DataRowState.Deleted) removedCountries.Add(r["Country", DataRowVersion.Original].ToString());
            RemoveCountryReferences(Plugin.settingsData, removedCountries);

            // if the Countries changed then you probably also need to update all the Reference Tables!
            Plugin.automaticlyUpdateReferenceTables();

            Plugin.saveSettings();
            changed = true;
            MessageBox.Show("Settings saved.");
        }

        private void gridView1_PopupMenuShowing(object sender, DevExpress.XtraGrid.Views.Grid.PopupMenuShowingEventArgs e)
        {
            e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete", DeleteRow));
        }

        internal static void RemoveCountryReferences(VariableDataSet dataSet, List<string> countries)
        {
            // for the advanced and derived variables and for the country-specific names of basic variables
            // countries are stored in a column as string (AT,XX,CY,UK)
            // in this case the string is either changed (AT,CY,UK) or the row removed if only relevant for the country/ies to remove
            RemoveCountriesFromColumn(dataSet.Cur_BasicCountrySpecificDetail, countries);
            RemoveCountriesFromColumn(dataSet.Cur_AdvancedVariables, countries);
            RemoveCountriesFromColumn(dataSet.Cur_DerivedVariables, countries);

            // for advanced country specific variables there is one row for each country
            List<VariableDataSet.Cur_AdvancedCountrySpecificDetailRow> delRows = new List<VariableDataSet.Cur_AdvancedCountrySpecificDetailRow>();
            foreach (VariableDataSet.Cur_AdvancedCountrySpecificDetailRow row in dataSet.Cur_AdvancedCountrySpecificDetail)
                if (countries.Contains(row.Country.ToUpper().Trim())) delRows.Add(row);
            foreach (VariableDataSet.Cur_AdvancedCountrySpecificDetailRow delRow in delRows) delRow.Delete();
        }

        private static void RemoveCountriesFromColumn(DataTable table, List<string> countries)
        {
            const string COL_COUNTRIES = "Countries";
            List<DataRow> delRows = new List<DataRow>();
            foreach (DataRow row in table.Rows)
            {
                string origCC = ImportVariablesForm.GetFieldStringValue(row, COL_COUNTRIES).ToUpper().Trim(), cleanedCC = origCC;
                foreach (string country in countries) // for the examples below: countries = { XX }
                {
                    if (cleanedCC == country) { cleanedCC = string.Empty; break; } // XX -> empty
                    cleanedCC = cleanedCC.Replace(", " + country + ",", ","); // ... C1, XX, C2 ... -> ... C1, C2 ...
                    if (cleanedCC.EndsWith(", " + country)) // ... CC, XX -> ... CC
                        cleanedCC = cleanedCC.Substring(0, cleanedCC.Length - (", " + country).Length);
                    if (cleanedCC.StartsWith(country + ", ")) // XX, CC ... -> CC ...
                        cleanedCC = cleanedCC.Substring((country + ", ").Length);
                }
                if (cleanedCC == origCC) continue;
                if (cleanedCC == string.Empty) delRows.Add(row);
                else row.SetField<string>(COL_COUNTRIES, cleanedCC);
            }
            foreach (DataRow delRow in delRows)
            {
                foreach (DataRelation rel in delRow.Table.ChildRelations) // for Cur_DerivedVariables the conditions must be deleted
                    foreach (DataRow subRow in delRow.GetChildRows(rel)) subRow.Delete();
                delRow.Delete();
            }
        }
    }
}
