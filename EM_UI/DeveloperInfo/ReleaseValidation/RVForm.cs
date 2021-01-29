using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    public partial class RVForm : Form
    {
        // *****************************************************************************************************
        // to add another validation table just add it here and derive the respective class from RVTable
        // all visual parts are created programmatically, thus no need to change sth in the RVForm.Designer
        // *****************************************************************************************************
        List<RVItem_Base> validationTables = new List<RVItem_Base>()
        {
            new RVItem_SystemConfiguration(),
            new RVItem_ConditionalFormatting(),
            new RVItem_DatabaseConfiguration(),
            new RVItem_UpratingFactors(),
            new RVItem_CompulsoryPolicies(),
            new RVItem_IncomelistsCore(true),
            new RVItem_IncomelistsCore(false)
        };
        // *****************************************************************************************************

        List<string> currentValidationItems = null;
        List<string> currentCountries = null;
        bool currentShowProblemsOnly = false;

        public RVForm()
        {
            InitializeComponent();
            SetDataGridLayout(dgvOverview);
        }

        void PerformValidation(string itemToUpdate = "") { PerformValidation(currentValidationItems, currentCountries, currentShowProblemsOnly, true, itemToUpdate); } // private function used by the Update-buttons
        internal void PerformValidation(List<string> validationItems, List<string> countries = null, bool showProblemsOnly = false, bool calledByUpdateButton = false, string itemToUpdate = "")
        {
            if (!calledByUpdateButton) // function was invoked by the configuration dialog, i.e. initialisation is necessary
            {
                currentValidationItems = validationItems;
                currentCountries = countries;
                currentShowProblemsOnly = showProblemsOnly;
                AdaptTables(); // add the TabPages and DataGridViews for the selected validation items (before that remove the current TabPages if necessary)
                               // (note: it is not possible to hide TabPages, therefore, with each new configuration, remove and re-add the necessary tables
            }
            EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm(); if (mainForm != null) mainForm.Cursor = Cursors.WaitCursor;
            dgvOverview.Rows.Clear(); // remove any current information from overview table
            foreach (RVItem_Base valTable in validationTables) // loop over the validation items (System Configuration, Conditional Formatting, etc.)
            {
                if (!validationItems.Contains(valTable.tableName)) { continue; } // item not selected for validation
                try
                {
                    if (itemToUpdate == string.Empty || valTable.tableName == itemToUpdate)
                        valTable.PerformValidation(countries, currentShowProblemsOnly); // update only if complete update or this is the table to update
                    // fill the overview table (also for not updated parts, as info was removed above)
                    if (!currentShowProblemsOnly || valTable.problemCountries != string.Empty)
                        dgvOverview.Rows.Add(valTable.tableName, RVItem_Base.GetResultImage(valTable.problemCountries == string.Empty), RVItem_Base.TrimEnd(valTable.problemCountries));
                }
                catch (Exception exception) // report error, but carry on with other validation items
                {
                    UserInfoHandler.ShowError(string.Format("Validating {0} failed with the following error:{1}{2}", 
                                              valTable.tableName, Environment.NewLine, exception.Message));
                    dgvOverview.Rows.Add(valTable.tableName, Properties.Resources.RVundef, "Validation failed");
                }
            }
            if (dgvOverview.Rows.Count == 0) dgvOverview.Rows.Add(string.Empty, Properties.Resources.RVallgood, "NO PROBLEMS FOUND");
            if (mainForm != null) mainForm.Cursor = Cursors.Default;
            if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal; // if window was minimised it would not be automatically put to normal by calling Show
            Show();
        }

        void RVForm_FormClosing(object sender, FormClosingEventArgs e) { Hide(); e.Cancel = true; } // do not close but hide (the non-modal dialog)

        void btnClose_Click(object sender, EventArgs e) { Hide(); }

        internal List<string> GetValidationItemNames() { return (from vt in validationTables select vt.tableName).ToList<string>(); }

        void AdaptTables()
        {
            for (int i = tabControl.TabPages.Count - 1; i > 0; --i) tabControl.TabPages.Remove(tabControl.TabPages[i]);
            foreach (RVItem_Base validationTable in validationTables)
            {
                if (!currentValidationItems.Contains(validationTable.tableName)) continue;
                tabControl.TabPages.Add(validationTable.tableName, validationTable.tableName);
                validationTable.tabPage = tabControl.TabPages[validationTable.tableName];
                validationTable.dataGrid = new DataGridView();
                validationTable.tabPage.Controls.Add(validationTable.dataGrid);
                SetDataGridLayout(validationTable.dataGrid);
            }
        }

        void SetDataGridLayout(DataGridView dataGrid)
        {
            dataGrid.Location = new Point(6, 6);
            dataGrid.Margin = new Padding(3, 3, 3, 3);
            //dataGrid.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dataGrid.Dock = DockStyle.Fill;
            dataGrid.BackgroundColor = SystemColors.Window;
            int margin = (dataGrid.Location.X + dataGrid.Margin.Left) * 2;
            dataGrid.Size = new Size(tabOverview.ClientSize.Width - margin, tabOverview.Height - margin);
            dataGrid.AllowUserToAddRows = false;
            dataGrid.AllowUserToDeleteRows = false;
            dataGrid.AllowUserToOrderColumns = false;
            dataGrid.RowHeadersVisible = false;
            dataGrid.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGrid.ScrollBars = ScrollBars.Both;

            DataGridViewColumn colValidationItem = new DataGridViewTextBoxColumn();
            colValidationItem.Name = colValidationItem.HeaderText = "Validation Item";
            colValidationItem.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
            colValidationItem.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            dataGrid.Columns.Add(colValidationItem);

            DataGridViewColumn colResult = new DataGridViewImageColumn();
            colResult.Name = colResult.HeaderText = "Result";
            colResult.AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
            dataGrid.Columns.Add(colResult);

            DataGridViewColumn colProblems = new DataGridViewTextBoxColumn();
            colProblems.Name = colProblems.HeaderText = "Problems";
            colProblems.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            colProblems.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dataGrid.Columns.Add(colProblems);

            foreach (DataGridViewColumn column in dataGrid.Columns)
            {
                column.ReadOnly = true;
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
        }

        void btnUpdate_Click(object sender, EventArgs e) { PerformValidation(tabControl.SelectedTab.Text); }
        void btnUpdateAll_Click(object sender, EventArgs e) { PerformValidation(); }

        void dgvOverview_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try // spare checking indices and null values by using try/catch
            {
                RVItem_Base clickedValidationTable = null;
                foreach (RVItem_Base validationTable in validationTables)
                    if (dgvOverview.Rows[e.RowIndex].Cells[0].Value.ToString() == validationTable.tableName)
                        { clickedValidationTable = validationTable; break; }
                tabControl.SelectedTab = clickedValidationTable.tabPage;
            }
            catch { }
        }
    }
}
