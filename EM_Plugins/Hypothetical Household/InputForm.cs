using DevExpress.Spreadsheet;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList.Menu;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraVerticalGrid.Rows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace HypotheticalHousehold
{
    public partial class InputForm : Form
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        private bool blockUpdating = false;
        private bool isTreeInitialized = false;
        private System.Windows.Forms.BindingSource householdBindingSource;
        
        public InputForm(Program _plugin)
        {
            InitializeComponent();
            Plugin = _plugin;

            householdBindingSource = new System.Windows.Forms.BindingSource() { Filter = "Checked = true" };

            buildDataGrid();

            countriesCheckedComboBoxEdit.Properties.DataSource = Plugin.settingsData.Cur_Countries;
            yearsCheckedComboBoxEdit.Properties.DataSource = Plugin.settingsData.Cur_Years;

            countriesCheckedComboBoxEdit.SetEditValue(Plugin.userSettings.settings.selectedCounries);
            countriesCheckedComboBoxEdit_EditValueChanged(null, null);
            yearsCheckedComboBoxEdit.SetEditValue(Plugin.userSettings.settings.selectedYears);
            yearsCheckedComboBoxEdit_EditValueChanged(null, null);

            treeHouseholds.DataSource = Plugin.householdData.Tables[Plugin.HOUSEHOLD_STRUCTURE_TABLE];

            treeHouseholds.ParentFieldName = "ParentID";
            treeHouseholds.KeyFieldName = "ID";
            treeHouseholds.RootValue = -1;
            treeHouseholds.Columns["isChecked"].Visible = false;
            treeHouseholds.Columns["isExpanded"].Visible = false;
            treeHouseholds.Columns["orderId"].Visible = false;
            treeHouseholds.Columns["dataId"].Visible = false;
            treeHouseholds.Columns["orderId"].SortOrder = SortOrder.Ascending;

            treeHouseholds.OptionsBehavior.EnableFiltering = true;

            ShowHideSaveAsPuplicMenu();
        }

        private void InputForm_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            gridHousehold.RecordCellStyle += gridHousehold_RecordCellStyle;
            gridHousehold.CustomDrawRowHeaderCell += gridHousehold_CustomDrawRowHeaderCell;
            gridHousehold.CustomDrawRowValueCell += gridHousehold_CustomDrawRowValueCell;
            gridHousehold.KeyDown += gridHousehold_KeyDown;
            if (treeHouseholds.Nodes.Count > 0) changeNode(treeHouseholds.Nodes[0]);    // set focus to the first node, and cause the grid to update
            else changeNode(null);
            isTreeInitialized = true;   // workaround for incredible bug that took me 3 days to resolve (and still didn't find the cause!) - (tree nodes appearing expanded when they shouldn't!)
            updateTreeState();
        }

        void gridHousehold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.F)
            {
                txtQuickFilter.Focus();
                e.Handled = true;
            }
        }

        void updateTreeState()
        {
            blockUpdating = true;
            foreach (TreeListNode n in treeHouseholds.Nodes)
            {
                n.Checked = (bool)n.GetValue("isChecked");
                if (n.HasChildren) foreach (TreeListNode n1 in n.Nodes) n1.Checked = (bool)n1.GetValue("isChecked");
                n.Expanded = (bool)n.GetValue("isExpanded");
            }
            blockUpdating = false;
        }

        void gridHousehold_CustomDrawRowValueCell(object sender, DevExpress.XtraVerticalGrid.Events.CustomDrawRowValueCellEventArgs e)
        {
            if (blockUpdating) return;
            MyEditorRow field = e.Row as MyEditorRow;
            if (field == null || !field.isDerived) return;
            e.CellText = field.defaultValue;
        }

        void gridHousehold_CustomDrawRowHeaderCell(object sender, DevExpress.XtraVerticalGrid.Events.CustomDrawRowHeaderCellEventArgs e)
        {
            if (blockUpdating) return;
            MyEditorRow field = e.Row as MyEditorRow;
            if (field == null) return;
            if (field.isDerived || field.isAdvanced)
            {
                if (gridHousehold.FocusedRow == e.Row)
                    e.Appearance.BackColor = Color.FromArgb(129, 171, 179);
                else
                    e.Appearance.BackColor = Color.FromArgb(230, 230, 230);
            }
        }

        void gridHousehold_UpdateVariableCaptions()
        {
            gridHousehold.BeginUpdate();
            foreach (BaseRow category in gridHousehold.Rows)
            {
                foreach (MyEditorRow field in category.ChildRows)
                {
                    if (field.varType == MyEditorRow.BASIC_VARIABLE)
                    {

                        string cntr = "";
                        string ttp = "Variable used:";

                        DataRow[] rows = Plugin.settingsData.Cur_BasicCountrySpecificDetail.Select("VariableName = '" + field.Properties.FieldName + "'");
                        if (rows.Length == 0)
                        {
                            ttp += "\n" + field.Properties.FieldName + " in all countries";
                        }
                        else
                        {
                            foreach (DataRow c in Plugin.settingsData.Cur_Countries.Rows)
                                if (rows.Count(x => x.Field<string>("Countries").Contains(c.Field<string>("Country"))) == 0) cntr += ", " + c.Field<string>("Country");
                            if (cntr != "") ttp += "\n" + field.Properties.FieldName + " in " + cntr.Substring(2);
                            foreach (DataRow fld1 in rows)
                                ttp += "\n" + fld1.Field<string>("CountrySpecificVariableName") + " in " + fld1.Field<string>("Countries");
                        }
                        field.Properties.ToolTip = ttp;
                    }
                    else if (field.varType == MyEditorRow.ADVANCED_VARIABLE || field.varType == MyEditorRow.DERIVED_VARIABLE)
                    {
                        DataRow fld;
                        if (field.varType == MyEditorRow.ADVANCED_VARIABLE) 
                            fld = Plugin.settingsData.Cur_AdvancedVariables.First(x => x.Field<string>("VariableName") == field.Properties.FieldName);
                        else
                            fld = Plugin.settingsData.Cur_DerivedVariables.First(x => x.Field<string>("VariableName") == field.Properties.FieldName);
                        if (countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count < 2)
                        {
                            field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ")";
                        }
                        else
                        {
                            string usedIn = "";
                            bool usedInAll = true;
                            foreach (string v in countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
                            {
                                if (fld.Field<string>("Countries").Contains(v))
                                    usedIn += ", " + v;
                                else
                                    usedInAll = false;
                            }
                            if (usedInAll)
                                field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ") [used in all selected countries]";
                            else if (usedIn != "")
                            {
                                field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ") [used in " + usedIn.Substring(2) + "]";
                            }
                        }
                    }
                    else if (field.varType == MyEditorRow.ADVANCEDCOUNTRYSPECIFIC_VARIABLE)
                    {
                        string var_name = field.Properties.FieldName.Substring(0, field.Properties.FieldName.LastIndexOf("_"));
                        string var_country = field.Properties.FieldName.Substring(field.Properties.FieldName.LastIndexOf("_") + 1);
                        DataRow fld = Plugin.settingsData.Cur_AdvancedCountrySpecificVariables.First(x => x.Field<string>("VariableName") == var_name);
                        if (countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues().Count < 2)
                        {
                            field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ")";
                        }
                        else
                        {
                            field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ") [used in " + var_country + "]";
                        }
                    }
                    else
                        continue;

                }
            }
            gridHousehold.EndUpdate();
        }

        void gridHousehold_RecordCellStyle(object sender, DevExpress.XtraVerticalGrid.Events.GetCustomRowCellStyleEventArgs e)
        {
            if (blockUpdating) return;
            MyEditorRow field = e.Row as MyEditorRow;
            if (field.isDerived) return;
            if (e.Row.Name == "fieldPersonName")
            {
                e.Appearance.BackColor = Color.FromArgb(236, 244, 242);
                e.Appearance.ForeColor = Color.Black;
            }
            else
            {
                if (field.isAdvanced)
                    e.Appearance.BackColor = Color.FromArgb(250, 250, 255);
                // This causes a crash if previously viewing an empty household
                // I cannot find a solution to properly check and avoid the crash! (hence just containing the Exception)
                try
                {
                    if (highlightChangedValuesToolStripMenuItem.Checked && gridHousehold.GetCellValue(e.Row, e.RecordIndex) != null)
                    {
                        string curVal = gridHousehold.GetCellValue(e.Row, e.RecordIndex).ToString();
                        if (field.defaultValue != curVal)
                        {
                            e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
                            e.Appearance.ForeColor = Color.FromArgb(200, 50, 50);
                        }
                    }
                }
                finally
                { }
            }
        }

        private void treeHouseholds_PopupMenuShowing(object sender, DevExpress.XtraTreeList.PopupMenuShowingEventArgs e)
        {
            if (e.Menu.MenuType == DevExpress.XtraTreeList.Menu.TreeListMenuType.Node)
            {
                TreeListNode node = ((TreeListNodeMenu)e.Menu).Node;
                treeHouseholds.FocusedNode = node;

                // if the node is a household
                if (node.Level == 0)
                {
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Rename Household", btnHouseholdRename_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Add Household", btnHouseholdAdd_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Duplicate Household", btnHouseholdDuplicate_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete Household", btnHouseholdRemove_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Reset Household's Advanced Variables", resetHouseholdAdvancedVariables_Click));
                }
                else    // if the node is a person
                {
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Rename Person", btnPersonRename_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Add Person to Household", btnPersonAdd_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Duplicate Person in Household", btnPersonDuplicate_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Delete Person from Household", btnPersonRemove_Click));
                    e.Menu.Items.Add(new DevExpress.Utils.Menu.DXMenuItem("Reset Person's Advanced Variables", resetPersonAdvancedVariables_Click));
                }
            }
        }

        private void changeNode(TreeListNode node, bool forceRefresh = false)
        {
            if (blockUpdating) return;
            if (node == null)
            {
                gridHousehold.Hide();
                btnPersonRemove.Enabled = false;
                btnPersonAdd.Enabled = false;
                btnHouseholdRemove.Enabled = false;
                return;
            }
            gridHousehold.Show();
            btnPersonAdd.Enabled = true;
            btnHouseholdRemove.Enabled = true;
            btnPersonRemove.Enabled = node.Level > 0;
            treeHouseholds.FocusedNode = node;
            node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
            string selTable = node.GetValue("HouseholdName").ToString();
            string famTypeId = node.GetValue("dataId").ToString();
            if (forceRefresh) gridHousehold.DataSource = null;
            if (householdBindingSource.DataMember == null || householdBindingSource.DataMember != selTable || forceRefresh)
            {
                gridHousehold.Rows[0].Properties.Caption = $"Currently viewing: {selTable} ({DataGenerator.HHTYPE_ID}: {famTypeId})";
                ((System.ComponentModel.ISupportInitialize)(householdBindingSource)).BeginInit();
                householdBindingSource.DataMember = selTable;
                householdBindingSource.DataSource = Plugin.householdData;
                householdBindingSource.Sort = "orderId asc";
                ((System.ComponentModel.ISupportInitialize)(householdBindingSource)).EndInit();
                gridHousehold.DataSource = householdBindingSource;
                updateConnectionGridView(); // required to properly show "connection" cells
                gridHousehold.Refresh();
            }
            else if (forceRefresh) gridHousehold.Refresh();
        }

        private void treeHouseholds_AfterCollapse(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (blockUpdating) return;
            e.Node.SetValue("isExpanded", e.Node.Expanded);

        }

        private void treeHouseholds_AfterExpand(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (blockUpdating) return;
            e.Node.SetValue("isExpanded", e.Node.Expanded);
        }

        void treeHouseholds_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            if (blockUpdating) return;
            e.Node.SetValue("isChecked", e.Node.Checked);
            if (e.Node.Level == 1)  // also update the table (for grid column filtering)
            {
                int nodeId = (int)e.Node.GetValue("dataId");
                DataTable curTable = Plugin.householdData.Tables[e.Node.ParentNode.GetValue("HouseholdName").ToString()];
                curTable.Rows.Find(nodeId).SetField<bool>("Checked", e.Node.Checked);
                e.Node.ParentNode.Checked = e.Node.ParentNode.Nodes.Any(n => n.Checked);
                e.Node.ParentNode.SetValue("isChecked", e.Node.ParentNode.Checked);
            }
            else  // if a household is checked, check/uncheck all members
            {
                List<TreeListNode> childNodes = e.Node.Nodes.ToList();
                foreach (TreeListNode n in childNodes)
                {
                    n.Checked = e.Node.Checked;
                    n.SetValue("isChecked", e.Node.Checked);
                    int nodeId = (int)n.GetValue("dataId");
                    DataTable curTable = Plugin.householdData.Tables[e.Node.GetValue("HouseholdName").ToString()];
                    curTable.Rows.Find(nodeId).SetField<bool>("Checked", e.Node.Checked);
                }
            }
            changeNode(e.Node, true);
        }

        // this is called when renaming a node (i.e. a Household or member)
        void treeHouseholds_CellValueChanged(object sender, DevExpress.XtraTreeList.CellValueChangedEventArgs e)
        {
            if (blockUpdating) return;
            if (gridHousehold.DataSource == null) return;

            if (e.Node.Level == 0)
            {
                gridHousehold.BeginDataUpdate();
                string old_name = treeHouseholds.ActiveEditor.OldEditValue.ToString();
                if (Plugin.householdData.Tables[e.Value.ToString()] == null)
                {
                    blockUpdating = true;
                    Plugin.householdData.Tables[old_name].TableName = e.Value.ToString();
                    householdBindingSource.DataMember = e.Value.ToString();
                    blockUpdating = false;
                }
                else
                {
                    e.Node.SetValue("HouseholdName", treeHouseholds.ActiveEditor.OldEditValue.ToString());
                    MessageBox.Show("This Household Name already exists!");
                }
                gridHousehold.EndDataUpdate();
            }
            else
            {
                int nodeId = (int)e.Node.GetValue("dataId"); 
                DataTable curTable = Plugin.householdData.Tables[householdBindingSource.DataMember];
                if (curTable.Select("PersonName='" + e.Value.ToString() + "'").Length > 0)
                {
                    treeHouseholds.BeginUpdate();
                    e.Node.SetValue("HouseholdName", curTable.Rows.Find(nodeId).Field<string>("PersonName"));
                    treeHouseholds.EndUpdate();
                    MessageBox.Show("This Person Name already exists!");
                }
                else
                {
                    gridHousehold.BeginUpdate();
                    curTable.Rows.Find(nodeId).SetField<string>("PersonName", e.Value.ToString());
                    updateConnectionGridView();
                    gridHousehold.EndUpdate();
                }
            }
        }


        private void generateButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (!checkAllOK(true)) return;

                using (OutputForm outputForm = new OutputForm(Plugin, this))
                {
                    outputForm.ShowDialog(this);
                }

            }
            catch
            {
                MessageBox.Show("The generation process was interrupted by an error.");
            }
        }

        private bool checkAllOK(bool checkCountriesYears = false)
        {
            string errorString = "";
            if (checkCountriesYears)
            {
                if (countriesCheckedComboBoxEdit.Properties.GetCheckedItems().ToString() == "") errorString += "You need to select at least one Country.\n";
                if (yearsCheckedComboBoxEdit.Properties.GetCheckedItems().ToString() == "") errorString += "You need to select at least one Year.\n";
            }
            if (treeHouseholds.GetAllCheckedNodes().Count < 1) errorString += "You need to select at least one Household.\n";
            List<string> missingVars = new List<string>();

            foreach (TreeListNode fam in treeHouseholds.Nodes)
            {
                if (fam.Checked)
                {
                    DataTable tbl = Plugin.householdData.Tables[fam.GetValue("HouseholdName").ToString()];
                    foreach (TreeListNode p in fam.Nodes)
                    {
                        if (p.Checked)
                        {
                            DataRow row = tbl.Rows.Find(p.GetValue("dataId"));
                            foreach (VariableDataSet.Cur_BasicVariablesRow var in Plugin.settingsData.Cur_BasicVariables.Rows)
                            {
                                if (var.VariableType != Program.EDITOR_TYPE_CONNECTION)
                                {
                                    if (row[var.VariableName] == DBNull.Value || row[var.VariableName].ToString() == "") 
                                    {
                                        if (!missingVars.Contains(var.Description)) missingVars.Add(var.Description);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (missingVars.Count > 0)
            {
                string s = missingVars.Count == 1 ? "" : "s";
                errorString += $"Missing value{s} in {missingVars.Count} Basic Variable{s} ({String.Join("", missingVars)}).\n";
            }
            if (errorString != "")
            {
                MessageBox.Show(errorString, "Errors found");
                return false;
            }

            return true;
        }

        #region household & person buttons 
        private void btnHouseholdRename_Click(object sender, EventArgs e)
        {
            if (treeHouseholds.FocusedNode == null) return;
            TreeListNode node = treeHouseholds.FocusedNode;
            node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
            treeHouseholds.FocusedNode = node;
            treeHouseholds.ShowEditor();
        }

        private void btnHouseholdAdd_Click(object sender, EventArgs e)
        {
            string name = "";
            DialogResult res = DialogResult.OK;
            bool done = false;
            do
            {
                res = InputBox.Show("New Household", "Please give the name for the new Household:", ref name, true);
                if (res == DialogResult.OK)
                {
                    done = Plugin.addNewHousehold(name);
                    updateTreeState();
                }
            }
            while (!done && res != DialogResult.Cancel);
        }

        private void btnHouseholdDuplicate_Click(object sender, EventArgs e)
        {
            if (treeHouseholds.FocusedNode == null) return;
            string newname = "";
            DialogResult res;
            bool done = false;
            do
            {
                res = InputBox.Show("New Household", "Please give the name for the new Household:", ref newname, true);
                if (res == DialogResult.OK)
                {
                    TreeListNode node = treeHouseholds.FocusedNode;
                    node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
                    done = Plugin.duplicateHousehold(node.GetValue("HouseholdName").ToString(), newname);
                }
            }
            while (!done && res != DialogResult.Cancel);
            // there is some weird update bug in the tree, so I have to call this twice!!!
            updateTreeState();
        }

        private void btnHouseholdRemove_Click(object sender, EventArgs e)
        {
            if (treeHouseholds.FocusedNode == null) return;
            TreeListNode node = treeHouseholds.FocusedNode;
            node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
            if (MessageBox.Show("Are you sure you want to delete the Household '" + node.GetValue("HouseholdName") + "'?", "Remove Household", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                Plugin.householdData.Tables.Remove(node.GetValue("HouseholdName").ToString());
                DataTable householdStructureTable = Plugin.householdData.Tables[Plugin.HOUSEHOLD_STRUCTURE_TABLE];
                DataRow[] rows = householdStructureTable.Select("ParentID='" + node.GetValue("ID") + "'");
                foreach (DataRow row in rows) householdStructureTable.Rows.Remove(row);
                householdStructureTable.Rows.Remove(householdStructureTable.Rows.Find(node.GetValue("ID")));
                if (householdStructureTable.Rows.Count == 0) changeNode(null);
            }
        }

        private void btnPersonRename_Click(object sender, EventArgs e)
        {
            if (treeHouseholds.FocusedNode == null) return;
            treeHouseholds.ShowEditor();
        }

        private void btnPersonAdd_Click(object sender, EventArgs e)
        {
            TreeListNode node = treeHouseholds.FocusedNode;
            node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
            string name = "";
            DialogResult res = DialogResult.OK;
            bool done = false;
            do
            {
                res = InputBox.Show("New Person", "Please give the name for the new Person:", ref name, true);
                if (res == DialogResult.OK)
                {
                    done = Plugin.addDefaultPerson(Plugin.householdData.Tables[(string)node.GetValue("HouseholdName")], name);
                    updateTreeState();
                    treeHouseholds.FocusedNode = node.LastNode;
                    gridHousehold.Refresh();
                }
            }
            while (!done && res != DialogResult.Cancel);
        }

        private void btnPersonDuplicate_Click(object sender, EventArgs e)
        {
            TreeListNode node = treeHouseholds.FocusedNode;
            string oldName = node.GetValue("HouseholdName").ToString();
            node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
            string newName = "";
            DialogResult res = DialogResult.OK;
            bool done = false;
            do
            {
                res = InputBox.Show("Duplicate Person", "Please give the name for the new Person:", ref newName, true);
                if (res == DialogResult.OK)
                {
                    done = Plugin.duplicatePerson(Plugin.householdData.Tables[(string)node.GetValue("HouseholdName")], oldName, newName);
                    updateTreeState();
                    treeHouseholds.FocusedNode = node.LastNode;
                    gridHousehold.Refresh();
                }
            }
            while (!done && res != DialogResult.Cancel);
        }

        private void btnPersonRemove_Click(object sender, EventArgs e)
        {
            TreeListNode node = treeHouseholds.FocusedNode;
            if (node.ParentNode.Nodes.Count < 2)
            {
                MessageBox.Show("You cannot remove the last Person in a Household!");
            }
            else if (MessageBox.Show("Are you sure you want to delete the Person '" + node.GetValue("HouseholdName") + "'?", "Remove Person", MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes)
            {
                // remove the data row
                DataTable grid = Plugin.householdData.Tables[node.ParentNode.GetValue("HouseholdName").ToString()];
                DataRow row = grid.Rows.Find(node.GetValue("dataId").ToString());
                grid.Rows.Remove(row);
                // remove the tree entry
                DataTable householdStructureTable = Plugin.householdData.Tables[Plugin.HOUSEHOLD_STRUCTURE_TABLE];
                TreeListNode prev = node.PrevNode;
                householdStructureTable.Rows.Remove(householdStructureTable.Rows.Find(node.GetValue("ID")));
                treeHouseholds.FocusedNode = prev;
                updateConnectionGridView();
            }
        }
        #endregion household & person buttons

        #region Mouse Handling (drag & drop)

        // This decides what can be dragged
        private void treeHouseholds_BeforeDragNode(object sender, DevExpress.XtraTreeList.BeforeDragNodeEventArgs e)
        {
            // a node can be moved if it is a household or if it is not the only person in the household
            e.CanDrag = ((e.Node.Level == 0) || (e.Node.ParentNode.Nodes.Count > 1));
        }

        // This handles the drag & drop left side (position) icons
        private void treeHouseholds_CalcNodeDragImageIndex(object sender, DevExpress.XtraTreeList.CalcNodeDragImageIndexEventArgs e)
        {
            if (e.Node == null)
            {
                e.ImageIndex = -1;  // no target - no icon
            }
            else
            {
                TreeListNode draggedNode = treeHouseholds.FocusedNode;
                if (draggedNode == e.Node || draggedNode.ParentNode == e.Node)
                {
                    e.ImageIndex = -1;  // cannot drag onto self or onto parent
                }
                else if (draggedNode.Level != e.Node.Level)
                {
                    // households can be moved only relative to households, people can be moved only relative to people
                    e.ImageIndex = -1;
                }
                else
                {
                    if (draggedNode.ParentNode == e.Node.ParentNode)
                    {
                        // if the node is moved upwards in the household, move above alse below target node
                        e.ImageIndex = (treeHouseholds.GetNodeIndex(draggedNode) < treeHouseholds.GetNodeIndex(e.Node)) ? 2 : 1;
                    }
                    else
                    {
                        // do not allow moving into different households
                        e.ImageIndex = -1;
                    }
                }
            }
        }

        // This handles what actually happens when a drag & drop occurs
        private void treeHouseholds_DragDrop(object sender, DragEventArgs e)
        {
            if (blockUpdating) return;
            // If the desired effect as determined by the DragOver is to move the node
            if (e.Effect == DragDropEffects.Move)
            {
                blockUpdating = true;
                TreeListNode draggedNode = treeHouseholds.FocusedNode;
                TreeListNode targetNode = treeHouseholds.CalcHitInfo(treeHouseholds.PointToClient(new Point(e.X, e.Y))).Node;
                // if moving a household, or a person within its household
                if (draggedNode.Level == 0 || draggedNode.ParentNode == targetNode.ParentNode)
                {
                    // just change the index. note that if the index is bigger than the starting the node will move below it, and if it is smaller the node will move above it.
                    treeHouseholds.BeginSort();
                    treeHouseholds.SetNodeIndex(draggedNode, treeHouseholds.GetNodeIndex(targetNode));
                    updateOrderInTable(draggedNode);
                    treeHouseholds.EndSort();
                }
                else    // if moving a person into a different household
                {
                    DataTable sourceTable = Plugin.householdData.Tables[draggedNode.ParentNode.GetValue("HouseholdName").ToString()];
                    DataTable targetTable = Plugin.householdData.Tables[(targetNode.Level == 0?targetNode:targetNode.ParentNode).GetValue("HouseholdName").ToString()];
                    // if the target is the household, put as first child
                    treeHouseholds.BeginSort();
                    if (targetNode.Level == 0)
                    {
                        treeHouseholds.MoveNode(draggedNode, targetNode, true);
                        // make node the first child
                        treeHouseholds.SetNodeIndex(draggedNode, 0);
                    }
                    else    // if target is another person, move below
                    {
                        treeHouseholds.MoveNode(draggedNode, targetNode.ParentNode, true);
                        treeHouseholds.SetNodeIndex(draggedNode, 0);    // first move node to top, so that it will then move under the target node
                        treeHouseholds.SetNodeIndex(draggedNode, treeHouseholds.GetNodeIndex(targetNode));
                    }

                    DataRow row = sourceTable.Rows.Find(draggedNode.GetValue("dataId"));
                    gridHousehold.BeginDataUpdate();
                    targetTable.Rows.Add(row.ItemArray);
                    sourceTable.Rows.Remove(row);
                    gridHousehold.EndDataUpdate();

                    updateOrderInTable(draggedNode);
                    treeHouseholds.EndSort();
                }
                blockUpdating = false;
                // focus again on the dragged node
                changeNode(draggedNode);
            }

            e.Effect = DragDropEffects.None; //suppress default behaviour
        }

        private void updateOrderInTable(TreeListNode draggedNode)
        {
            // first get a list of the all the node IDs to reorder
            List<int> IDs = new List<int>();
            TreeListNodes nodes = (draggedNode.Level == 0 ? treeHouseholds.Nodes : draggedNode.ParentNode.Nodes);
            foreach (TreeListNode node in nodes) IDs.Add(int.Parse(node.GetValue("dataId").ToString()));
            // and use these to properly update the orderId 
            DataTable tbl = Plugin.householdData.Tables[Plugin.HOUSEHOLD_STRUCTURE_TABLE];
            DataTable tbl1 = (draggedNode.Level > 0 ? Plugin.householdData.Tables[draggedNode.ParentNode.GetValue("HouseholdName").ToString()] : null);
            for (int i = 0; i < IDs.Count; i++)
            {
                DataRow row = tbl.Select("dataId=" + IDs[i])[0];
                row.SetField<int>("orderId", i);
                if (draggedNode.Level > 0)
                {
                    row = tbl1.Select("dataId=" + IDs[i])[0];
                    row.SetField<int>("orderId", i);
                }
            }
        }

        // This handles the drag & drop right side (action) icons
        private void treeHouseholds_DragOver(object sender, DragEventArgs e)
        {
            TreeListNode draggedNode = treeHouseholds.FocusedNode;
            TreeListNode targetNode = treeHouseholds.CalcHitInfo(treeHouseholds.PointToClient(new Point(e.X, e.Y))).Node;
            // No action should be taken if:
            if (draggedNode == null ||                      // no node is dragged
                targetNode == null ||                       // there is no target
                draggedNode == targetNode ||                // the node is dragged onto itself
                draggedNode.ParentNode == targetNode ||     // the node is dragged onto its parent
                draggedNode.Level < targetNode.Level ||     // a household is dragged onto a person
                (targetNode.Level < draggedNode.Level && targetNode.ParentNode != draggedNode) || // a person is dragged onto a different household
                (targetNode.Level == draggedNode.Level && targetNode.ParentNode != draggedNode.ParentNode)) // a person is dragged onto a person of a different household
            {
                e.Effect = DragDropEffects.None;
            }
            else    // else the node should be moved
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        #endregion Mouse Handling (drag & drop)

        private void InputForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            gridHousehold.DataSource = null;
            Plugin.Close();
        }

        private void treeHouseholds_FocusedNodeChanged(object sender, DevExpress.XtraTreeList.FocusedNodeChangedEventArgs e)
        {
            if (blockUpdating) return;
            if (e.Node == null) return;     // no node selected
            changeNode(e.Node);
        }

        #region Menu Item Evens
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog() { DefaultExt = ".xml", Filter = "XML files|*.xml" };
            sfd.ShowDialog();
            if (sfd.FileName != String.Empty)
            {
                blockUpdating = true;
                GridEditorCloseAndSubmit();
                Plugin.saveHouseholdDataAs(sfd.FileName);
                blockUpdating = false;
                Text = "Hypothetical Household Tool - " + sfd.FileName;
            }
        }

        private void showAdvancedVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            showAdvancedVariablesToolStripMenuItem.Checked = !showAdvancedVariablesToolStripMenuItem.Checked;
            changeVisibleRows();
        }

        private void showDerivedVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            showDerivedVariablesToolStripMenuItem.Checked = !showDerivedVariablesToolStripMenuItem.Checked;
            changeVisibleRows();
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //loadHouseholdFile();
        }

        private void GridEditorCloseAndSubmit()
        {
            treeHouseholds.CloseEditor();
            gridHousehold.CloseEditor();
            btnHouseholdAdd.Focus(); // to make sure that possible changes do not get lost
        }

        internal bool RequestSavingHouseholdData(bool force = false)
        {
            blockUpdating = true;
            GridEditorCloseAndSubmit();
            if (Plugin.householdData.HasChanges())
            {
                DialogResult res = MessageBox.Show(this, force ? "Changes need to be saved." : "Do you want to save your changes?", "Save",
                    force ? MessageBoxButtons.OKCancel : MessageBoxButtons.YesNoCancel);
                
                if (res == DialogResult.Yes || res == DialogResult.OK) Plugin.saveHouseholdData();
                else if (res == DialogResult.Cancel) return false;
                else if (res == DialogResult.No) Plugin.householdData.RejectChanges();
            }
            blockUpdating = false;
            return true;
        }


        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!RequestSavingHouseholdData()) return;

            string projectPath;
            if (Plugin.SelectPathOpenProject(out projectPath))
            {
                Cursor = Cursors.WaitCursor;
                Plugin.openProject(projectPath);
                Cursor = Cursors.Default;
            }
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            blockUpdating = true;
            Plugin.saveHouseholdData();
            blockUpdating = false;
        }

        private void saveAsProjectToolStripMenuItem_Click(object sender, EventArgs e) { SaveProjectAs(false); }
        private void SaveAsPublicToolStripMenuItem_Click(object sender, EventArgs e) { SaveProjectAs(true); }
        private void SaveProjectAs(bool saveAsPublic)
        {
            PrepareProjectForm prepareProjectForm = new PrepareProjectForm("Save HHOT project as ...");
            if (prepareProjectForm.ShowDialog() == DialogResult.Cancel) return;

            GridEditorCloseAndSubmit();
            blockUpdating = true;
            Plugin.saveProjectAs(prepareProjectForm.GetProjectPath(), saveAsPublic);
            blockUpdating = false;
            if (saveAsPublic) updateGrid(); // adapt for possibly removed private variables
        }

        internal void ShowHideSaveAsPuplicMenu()
        {
            SaveAsPublicToolStripMenuItem.Visible = !Plugin.isPublicVersion();
        }

        private void importHHToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!RequestSavingHouseholdData(true)) return;

            string importFolder = SelectImportFolder(); if (importFolder == null) return;
            ImportHHForm form = new ImportHHForm();
            if (form.Init(Plugin, importFolder) && form.ShowDialog() == DialogResult.OK) updateTreeState();
        }

        private void importVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            string importFolder = SelectImportFolder(); if (importFolder == null) return;
            ImportVariablesForm form = new ImportVariablesForm();
            Cursor = Cursors.WaitCursor;
            if (form.Init(Plugin, importFolder) && form.ShowDialog() == DialogResult.OK) updateGrid();
            Cursor = Cursors.Default;
        }

        private void importRefTabToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            string importFolder = SelectImportFolder(); if (importFolder == null) return;
            ImportRefTablesForm form = new ImportRefTablesForm();
            if (form.Init(Plugin, importFolder)) form.ShowDialog();
        }

        private string SelectImportFolder()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
            {
                Description = "Please choose the folder of the HHOT project which serves as import source ...",
                SelectedPath = Plugin.currentProjectPath
            };
            if (folderBrowserDialog.ShowDialog() == DialogResult.Cancel) return null;

            string error;
            if (Plugin.IsHHotProjectPath(folderBrowserDialog.SelectedPath, out error)) return folderBrowserDialog.SelectedPath;
            MessageBox.Show("Folder " + folderBrowserDialog.SelectedPath + " does not contain a valid HHOT project." + Environment.NewLine + error);
            return null;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void budgetConstraintsToolStripMenuItem_Click(object sender, EventArgs e) { SimpleStatistics.BudgetConstraints(); }
        private void breakDownCountryYearToolStripMenuItem_Click(object sender, EventArgs e) { SimpleStatistics.BreakDownCountryYear(); }
        private void breakDownTypesToolStripMenuItem_Click(object sender, EventArgs e) { SimpleStatistics.BreakDownHHTypes(); }
        
        #endregion Menu Item Evens

        internal void readHouseholds(string fileName)
        {
            treeHouseholds.BeginUpdate();
            gridHousehold.BeginDataUpdate();
            householdBindingSource = new System.Windows.Forms.BindingSource() { Filter = "Checked = true" };
            Plugin.readHouseholdDataFrom(fileName);
            treeHouseholds.EndUpdate();
            buildDataGrid();
            gridHousehold.EndDataUpdate();
            treeHouseholds.DataSource = Plugin.householdData.Tables[Plugin.HOUSEHOLD_STRUCTURE_TABLE];
            changeNode(treeHouseholds.Nodes[0]);  // set focus to the first node, and cause the grid to update
            updateTreeState();
        }

        internal void loadHouseholdFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "XML files|*.xml" };
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) return;
            treeHouseholds.BeginUpdate();
            gridHousehold.BeginDataUpdate();
            householdBindingSource = new System.Windows.Forms.BindingSource() { Filter = "Checked = true" };
            Plugin.readHouseholdDataFrom(ofd.FileName);

            treeHouseholds.EndUpdate();
//            changeNode(null);    // first set focus to none
            gridHousehold.EndDataUpdate();
            treeHouseholds.DataSource = Plugin.householdData.Tables[Plugin.HOUSEHOLD_STRUCTURE_TABLE];
            changeNode(treeHouseholds.Nodes[0]);    // set focus to the first node, and cause the grid to update
            updateTreeState();
            Text = "Hypothetical Household Tool - " + ofd.FileName;
        }

        private void treeHouseholds_BeforeExpand(object sender, DevExpress.XtraTreeList.BeforeExpandEventArgs e)
        {
            e.CanExpand = isTreeInitialized;
        }

        private void updateGrid()
        {
            Cursor = Cursors.WaitCursor;
            buildDataGrid();
            Plugin.addMissingVariables();
            Plugin.removeExtraVariables();
            changeNode(treeHouseholds.FocusedNode, true);
            Cursor = Cursors.Default;
        }

        private void buildDataGrid()
        {
            gridHousehold.BeginUpdate();
            gridHousehold.Rows.Clear();
            gridHousehold.Rows.Add(makeColumnHeaderRow());
            string categoryString = "";
            CategoryRow categoryObject = null;
            int fieldCounter = 0;
            try
            {
                foreach (DataRow cat in Plugin.settingsData.Cur_Categories.Rows)
                {
                    // create a new category
                    categoryString = cat.Field<string>("Category");
                    categoryObject = new CategoryRow(categoryString);
                    categoryObject.Name = "category" + categoryString.Replace("/", "");
                    categoryObject.Properties.Caption = categoryString;
                    foreach (DataTable tbl in new DataTable[] { Plugin.settingsData.Cur_BasicVariables, Plugin.settingsData.Cur_AdvancedCountrySpecificVariables, Plugin.settingsData.Cur_AdvancedVariables, Plugin.settingsData.Cur_DerivedVariables })
                    {
                        foreach (DataRow fld in tbl.Select("Category = '" + categoryString + "'"))
                        {
                            if (tbl == Plugin.settingsData.Cur_BasicVariables)
                            {
                                MyEditorRow field = new MyEditorRow()
                                {
                                    varType = MyEditorRow.BASIC_VARIABLE,
                                    isAdvanced = false,
                                    isDerived = false,
                                    isHouseholdVar = fld["IsHouseholdVar"] == DBNull.Value ? false : fld.Field<bool>("IsHouseholdVar"),
                                    Name = "field" + fieldCounter
                                };
                                field.Properties.Caption = fld.Field<string>("Description");
                                field.countries.Add("ALL");
                                field.Properties.FieldName = fld.Field<string>("VariableName");
                                field.defaultValue = fld.Table.Columns.Contains("DefaultValue") ? fld.Field<string>("DefaultValue") : null;
                                switch (fld.Field<string>("VariableType"))
                                {
                                    case Program.EDITOR_TYPE_CONNECTION:
                                        RepositoryConnectionLookUpEdit concombo = new RepositoryConnectionLookUpEdit(Plugin, fieldCounter, gridHousehold);
                                        field.Properties.RowEdit = concombo;
                                        break;
                                    case Program.EDITOR_TYPE_COMBO:
                                        RepositoryItemLookUpEdit combo = new RepositoryItemLookUpEdit() { 
                                            ShowHeader = false, 
                                            ShowFooter = false, 
                                            ShowPopupShadow = true 
                                        };
                                        BindingList<GridComboItem> bl = new BindingList<GridComboItem>();
                                        string[] dv = fld.Field<string>("TextRange").Split('#');
                                        string[] v = fld.Field<string>("ValueRange").Split('#');
                                        for (int i = 0; i < dv.Length; i++)
                                            bl.Add(new GridComboItem { Text = dv[i].Trim(), Value = v[i].Trim() });
                                        combo.DataSource = bl;
                                        combo.DisplayMember = "Text";
                                        combo.ValueMember = "Value";
                                        combo.Name = "combo" + fieldCounter;
                                        combo.ForceInitialize();
                                        combo.PopulateColumns();
                                        combo.Columns["Value"].Visible = false;
                                        combo.DropDownRows = dv.Length > 7 ? 7 : dv.Length;
                                        combo.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                                        combo.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
                                        field.Properties.RowEdit = combo;
                                        break;
                                    case Program.EDITOR_TYPE_NUMERIC:
                                        RepositoryItemPopupContainerEditNumericRange pcemr = new RepositoryItemPopupContainerEditNumericRange(Plugin);
                                        field.Properties.RowEdit = pcemr;
                                        break;
                                    default:
                                        throw new Exception("Unknown editor type for field: " + field.Name);
                                }
                                field.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                                field.Appearance.Options.UseTextOptions = true;
                                field.Properties.ReadOnly = false;
                                categoryObject.ChildRows.Add(field);
                            }
                            else if (tbl == Plugin.settingsData.Cur_AdvancedVariables)
                            {
                                MyEditorRow field = new MyEditorRow() { 
                                    varType = MyEditorRow.ADVANCED_VARIABLE, 
                                    isAdvanced = true, 
                                    isDerived = false, 
                                    isHouseholdVar = fld["IsHouseholdVar"] == DBNull.Value ? false : fld.Field<bool>("IsHouseholdVar"), 
                                    Name = "field" + fieldCounter 
                                };
                                field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ")";
                                field.Properties.FieldName = fld.Field<string>("VariableName");
                                if (fld.Field<string>("Countries") != null) foreach (string c in fld.Field<string>("Countries").Split(',')) field.countries.Add(c.Trim().ToUpper());
                                field.defaultValue = fld.Table.Columns.Contains("DefaultValue") ? fld.Field<string>("DefaultValue") : null;
                                switch (fld.Field<string>("VariableType"))
                                {
                                    case Program.EDITOR_TYPE_CONNECTION:
                                        RepositoryConnectionLookUpEdit concombo = new RepositoryConnectionLookUpEdit(Plugin, fieldCounter, gridHousehold);
                                        field.Properties.RowEdit = concombo;
                                        break;
                                    case Program.EDITOR_TYPE_COMBO:
                                        RepositoryItemLookUpEdit combo = new RepositoryItemLookUpEdit() { 
                                            ShowHeader = false, 
                                            ShowFooter = false, 
                                            ShowPopupShadow = true 
                                        };
                                        BindingList<GridComboItem> bl = new BindingList<GridComboItem>();
                                        string[] dv = fld.Field<string>("TextRange").Split('#');
                                        string[] v = fld.Field<string>("ValueRange").Split('#');
                                        for (int i = 0; i < dv.Length; i++)
                                            bl.Add(new GridComboItem { Text = dv[i].Trim(), Value = v[i].Trim() });
                                        combo.DataSource = bl;
                                        combo.DisplayMember = "Text";
                                        combo.ValueMember = "Value";
                                        combo.Name = "combo" + fieldCounter;
                                        combo.ForceInitialize();
                                        combo.PopulateColumns();
                                        combo.Columns["Value"].Visible = false;
                                        combo.DropDownRows = dv.Length > 7 ? 7 : dv.Length;
                                        combo.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                                        combo.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
                                        field.Properties.RowEdit = combo;
                                        break;
                                    case Program.EDITOR_TYPE_NUMERIC:
                                        RepositoryItemPopupContainerEditNumericRange pcemr = new RepositoryItemPopupContainerEditNumericRange(Plugin);
                                        field.Properties.RowEdit = pcemr;
                                        break;
                                    default:
                                        throw new Exception("Unknown editor type for field: " + field.Name);
                                }
                                field.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                                field.Appearance.Options.UseTextOptions = true;
                                field.Properties.ReadOnly = false;
                                categoryObject.ChildRows.Add(field);
                            }
                            else if (tbl == Plugin.settingsData.Cur_AdvancedCountrySpecificVariables)
                            {
                                foreach (DataRow fld1 in Plugin.settingsData.Cur_AdvancedCountrySpecificDetail.Select("VariableName = '" + fld.Field<string>("VariableName") + "'"))
                                {
                                    MyEditorRow field = new MyEditorRow() { 
                                        varType = MyEditorRow.ADVANCEDCOUNTRYSPECIFIC_VARIABLE, 
                                        isAdvanced = true, 
                                        isDerived = false, 
                                        isHouseholdVar = fld["IsHouseholdVar"] == DBNull.Value ? false : fld.Field<bool>("IsHouseholdVar"), 
                                        Name = "field" + fieldCounter 
                                    };
                                    field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ")";
                                    field.Properties.FieldName = fld.Field<string>("VariableName") + "_" + fld1.Field<string>("Country");
                                    field.countries.Add(fld1.Field<string>("Country"));
                                    field.defaultValue = fld1.Field<string>("DefaultValue");
                                    switch (fld.Field<string>("VariableType"))
                                    {
                                        case Program.EDITOR_TYPE_CONNECTION:
                                            RepositoryConnectionLookUpEdit concombo = new RepositoryConnectionLookUpEdit(Plugin, fieldCounter, gridHousehold);
                                            field.Properties.RowEdit = concombo;
                                            break;
                                        case Program.EDITOR_TYPE_COMBO:
                                            RepositoryItemLookUpEdit combo = new RepositoryItemLookUpEdit() { 
                                                ShowHeader = false, 
                                                ShowFooter = false, 
                                                ShowPopupShadow = true 
                                            };
                                            BindingList<GridComboItem> bl = new BindingList<GridComboItem>();
                                            string[] dv = fld1.Field<string>("TextRange").Split('#');
                                            string[] v = fld1.Field<string>("ValueRange").Split('#');
                                            for (int i = 0; i < dv.Length; i++)
                                                bl.Add(new GridComboItem { Text = dv[i].Trim(), Value = v[i].Trim() });
                                            combo.DataSource = bl;
                                            combo.DisplayMember = "Text";
                                            combo.ValueMember = "Value";
                                            combo.Name = "combo" + fieldCounter;
                                            combo.ForceInitialize();
                                            combo.PopulateColumns();
                                            combo.Columns["Value"].Visible = false;
                                            combo.DropDownRows = dv.Length > 7 ? 7 : dv.Length;
                                            combo.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
                                            combo.BestFitMode = DevExpress.XtraEditors.Controls.BestFitMode.BestFitResizePopup;
                                            field.Properties.RowEdit = combo;
                                            break;
                                        case Program.EDITOR_TYPE_NUMERIC:
                                            RepositoryItemPopupContainerEditNumericRange pcemr = new RepositoryItemPopupContainerEditNumericRange(Plugin);
                                            field.Properties.RowEdit = pcemr;
                                            break;
                                        default:
                                            throw new Exception("Unknown editor type for field: " + field.Name);
                                    }
                                    field.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Far;
                                    field.Appearance.Options.UseTextOptions = true;
                                    field.Properties.ReadOnly = false;
                                    categoryObject.ChildRows.Add(field);
                                }
                            }
                            else if (tbl == Plugin.settingsData.Cur_DerivedVariables)
                            {
                                MyEditorRow field = new MyEditorRow() { 
                                    varType = MyEditorRow.DERIVED_VARIABLE, 
                                    isAdvanced = false, 
                                    isDerived = true, 
                                    isHouseholdVar = false, 
                                    Name = "field" + fieldCounter,
                                    Visible = true
                                };
                                field.Properties.Caption = fld.Field<string>("Description") + " (" + fld.Field<string>("VariableName") + ")";
                                field.Properties.FieldName = fld.Field<string>("VariableName");
                                if (fld.Field<string>("Countries") != null) foreach (string c in fld.Field<string>("Countries").Split(',')) field.countries.Add(c.Trim().ToUpper());
                                string condValues = "";
                                foreach (DataRow fld1 in Plugin.settingsData.Cur_DerivedVariablesDetail.Select("VariableName = '" + fld.Field<string>("VariableName") + "'"))
                                {
                                    condValues += Environment.NewLine + "[" + (fld1.Field<string>("Condition") ?? "") + "] --> " + (fld1.Field<string>("DerivedValue") ?? "");
                                }
                                if (condValues == "")
                                    field.defaultValue = fld.Field<string>("DefaultValue") ?? "";
                                else
                                    field.defaultValue = "[default] --> " + (fld.Field<string>("DefaultValue") ?? "") + condValues;
                                field.Enabled = false;
                                field.Properties.ReadOnly = true;
                                categoryObject.ChildRows.Add(field);
                            }
                            fieldCounter++;
                        }
                    }
                    gridHousehold.Rows.Add(categoryObject);
                }
            }
            finally
            {
                changeVisibleRows();
                gridHousehold_UpdateVariableCaptions();
                gridHousehold.EndUpdate();
                gridHousehold.Refresh();
            }
        }

        private void updateConnectionGridView()
        {
            BindingList<GridComboItem> conblfull = new BindingList<GridComboItem>();
            DataTable curTable = Plugin.householdData.Tables[householdBindingSource.DataMember];
            conblfull.Add(new GridComboItem { Text = "", Value = "0" });
            foreach (DataRow row in curTable.Rows)
                conblfull.Add(new GridComboItem { Text = row.Field<string>("PersonName"), Value = row.Field<int>("dataId").ToString() });

            foreach (BaseRow r in gridHousehold.Rows)
                foreach (BaseRow rc in r.ChildRows)
                    if (rc.Properties.RowEdit is RepositoryConnectionLookUpEdit)
                        (rc.Properties.RowEdit as RepositoryConnectionLookUpEdit).DataSource = conblfull;
        }

        private void changeVisibleRows()
        {
            gridHousehold.BeginUpdate();
            foreach (BaseRow row in gridHousehold.Rows)
            {
                if (row.Name == "fieldPersonName") continue;
                row.Visible = false;
                foreach (MyEditorRow row1 in row.ChildRows)
                {
                    bool showField = true;
                    if (row1.isDerived && !showDerivedVariablesToolStripMenuItem.Checked) showField = false;
                    else if (row1.isAdvanced && !showAdvancedVariablesToolStripMenuItem.Checked) showField = false;
                    else if (txtQuickFilter.Text != "" && !row1.Properties.Caption.ToLower().Contains(txtQuickFilter.Text.ToLower()) && !row1.Properties.ToolTip.ToLower().Contains(txtQuickFilter.Text.ToLower())) showField = false;
                    else if (!row1.countries.Contains("ALL"))
                    {
                        bool hasCountry = false;
                        foreach (string c in countriesCheckedComboBoxEdit.Properties.Items.GetCheckedValues())
                            if (row1.countries.Contains(c.ToUpper().Trim()))
                            {
                                hasCountry = true;
                                break;
                            }
                        showField = showField && hasCountry;
                    }
                    if (showField && showOnlyChangedValuesToolStripMenuItem.Checked)
                    {
                        bool changed = false;
                        for (int i = 0; i<gridHousehold.RecordCount; i++)
                        {
                            if (gridHousehold.GetCellValue(row1, i) == null) break;
                            if (gridHousehold.GetCellValue(row1, i).ToString() != (row1 as MyEditorRow).defaultValue)
                            {
                                changed = true;
                                break;
                            }
                        }
                        showField = changed;
                    }

                    row1.Visible = showField;
                }
            }
            gridHousehold.EndUpdate();
            // Because of some stupid update bug, I need to run the loop twice to properly hide categories
            gridHousehold.BeginUpdate();
            foreach (BaseRow row in gridHousehold.Rows)
            {
                if (row.Name == "fieldPersonName") continue;
                row.Visible = row.ChildRows.HasVisibleItems;
            }
            gridHousehold.EndDataUpdate();
            gridHousehold.TopVisibleRowIndex = 0;
        }

        private void txtQuickFilter_EditValueChanged(object sender, EventArgs e)
        {
            changeVisibleRows();
        }

        private MyEditorRow makeColumnHeaderRow()
        {
            MyEditorRow hrow = new MyEditorRow();
            hrow.Appearance.BackColor = System.Drawing.SystemColors.Control;
            hrow.Appearance.Font = new System.Drawing.Font("Tahoma", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            hrow.Appearance.ForeColor = System.Drawing.Color.Black;
            hrow.Appearance.Options.UseBackColor = true;
            hrow.Appearance.Options.UseFont = true;
            hrow.Appearance.Options.UseForeColor = true;
            hrow.Fixed = DevExpress.XtraVerticalGrid.Rows.FixedStyle.Top;
            hrow.InternalFixed = DevExpress.XtraVerticalGrid.Rows.FixedStyle.Top;
            hrow.Name = "fieldPersonName";
            hrow.Properties.Caption = "No Data";
            hrow.OptionsRow.AllowFocus = false;
            hrow.Properties.FieldName = "PersonName";
            hrow.Properties.ReadOnly = true;
            hrow.isAdvanced = false;
            hrow.isDerived = false;
            hrow.Visible = true;
            return hrow;
        }

        internal void countriesCheckedComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            string selection = countriesCheckedComboBoxEdit.Properties.GetCheckedItems().ToString();
            labelSelectedCountries.Text = (selection == "") ? "No countries selected!" : "Selected countries:\n" + selection;
            changeVisibleRows();
            gridHousehold_UpdateVariableCaptions();
            Plugin.userSettings.settings.selectedCounries = countriesCheckedComboBoxEdit.EditValue.ToString();
        }

        internal void yearsCheckedComboBoxEdit_EditValueChanged(object sender, EventArgs e)
        {
            string selection = yearsCheckedComboBoxEdit.Properties.GetCheckedItems().ToString();
            labelSelectedYears.Text = (selection == "") ? "No years selected!" : "Selected years:\n" + selection;
            Plugin.userSettings.settings.selectedYears = yearsCheckedComboBoxEdit.EditValue.ToString();
        }

        private void manageCategoriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageCategories mc = new SettingsManagement.ManageCategories(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void manageCountriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageCountries mc = new SettingsManagement.ManageCountries(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void manageYearsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageYears mc = new SettingsManagement.ManageYears(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void manageBasicVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageBasicVariables mc = new SettingsManagement.ManageBasicVariables(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void manageAdvancedVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageAdvancedVariables mc = new SettingsManagement.ManageAdvancedVariables(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void manageAdvancedCountryspecificVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageAdvancedCountrySpecificVariables mc = new SettingsManagement.ManageAdvancedCountrySpecificVariables(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void manageDerivedVariablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageDerivedVariables mc = new SettingsManagement.ManageDerivedVariables(Plugin))
            {
                mc.ShowDialog(this);
                if (mc.changed) updateGrid();
            }
        }

        private void showOnlyChangedValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            showOnlyChangedValuesToolStripMenuItem.Checked = !showOnlyChangedValuesToolStripMenuItem.Checked;
            if (showOnlyChangedValuesToolStripMenuItem.Checked) highlightChangedValuesToolStripMenuItem.Checked = false;
            changeVisibleRows();
        }

        private void highlightChangedValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            highlightChangedValuesToolStripMenuItem.Checked = !highlightChangedValuesToolStripMenuItem.Checked;
            if (highlightChangedValuesToolStripMenuItem.Checked) showOnlyChangedValuesToolStripMenuItem.Checked = false;
            changeVisibleRows();
            gridHousehold.Refresh();
        }

        private void manageReferenceTablesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            using (SettingsManagement.ManageReferenceTables mrt = new SettingsManagement.ManageReferenceTables(Plugin))
            {
                mrt.ShowDialog(this);
            }
        }

        private void InputForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!RequestSavingHouseholdData()) e.Cancel = true;
        }

        private void resetHouseholdAdvancedVariables_Click(object sender, EventArgs e)
        {
            resetHouseholdAdvancedVariables();
        }

        private void resetPersonAdvancedVariables_Click(object sender, EventArgs e)
        {
            resetPersonAdvancedVariables();
        }

        private void resetHouseholdAdvancedVariables()
        {
            DialogResult res = MessageBox.Show(this, "Do you wish to reset the Advanced Variables for all members of this Household?", "Reset Advanced Values", MessageBoxButtons.YesNo);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                TreeListNode node = treeHouseholds.FocusedNode;
                node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
                Plugin.resetHouseholdAdvancedVariables(Plugin.householdData.Tables[node.GetValue("HouseholdName").ToString()]);
                gridHousehold.Refresh();
            }
        }

        private void resetAllHouseholdsAdvancedVariables()
        {
            DialogResult res = MessageBox.Show(this, "Do you wish to reset the Advanced Variables for all members of all selected Households?", "Reset Advanced Values", MessageBoxButtons.YesNo);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                List<TreeListNode> hhNodes = treeHouseholds.Nodes.ToList();
                foreach (TreeListNode hn in hhNodes)
                {
                    if (hn.Checked)
                    {
                        if(hn.Level == 0)
                        {
                            Plugin.resetHouseholdAdvancedVariables(Plugin.householdData.Tables[hn.GetValue("HouseholdName").ToString()]);
                        }
                    }
                }
                gridHousehold.Refresh();
            }
        }

        private void resetPersonAdvancedVariables()
        {
            DialogResult res = MessageBox.Show(this, "Do you wish to reset the Advanced Variables for this Person?", "Reset Advanced Values", MessageBoxButtons.YesNo);
            if (res == System.Windows.Forms.DialogResult.Yes)
            {
                TreeListNode node = treeHouseholds.FocusedNode;
                int dataId = int.Parse(node.GetValue("dataId").ToString());
                node = (node.Level == 0) ? node : node.ParentNode;  // find the Household node
                Plugin.resetPersonAdvancedVariables(Plugin.householdData.Tables[node.GetValue("HouseholdName").ToString()], dataId);
                gridHousehold.Refresh();
            }
        }

        // this is a hidden option for importing variables from a specially formulated Excel file
        private void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool hide = true; if (hide) return;

            DevExpress.XtraSpreadsheet.SpreadsheetControl spreadsheetControl1 = new DevExpress.XtraSpreadsheet.SpreadsheetControl();
            spreadsheetControl1.LoadDocument();
            if (!spreadsheetControl1.Document.Worksheets.Contains("Other_vars")) return;
            Worksheet ws = spreadsheetControl1.Document.Worksheets["Other_vars"];
            int i = 2;
            // we will use the editors to parse the default values
            NumericEditor numericEditor = new NumericEditor(Plugin);
            while (i < ws.Rows.LastUsedIndex)
            {
                string vname = ws.Cells[i, 0].Value.TextValue;
                if (ws.Cells[i, 9].Value.TextValue == "A")
                {
                    if (Plugin.settingsData.Cur_AdvancedVariables.Select("VariableName = '" + vname + "'").Length == 0)
                    {
                        // only import values that have a type - if they are combo, only import those that have a default value
                        if (editorTranslate(ws.Cells[i, 4].Value.TextValue) != "" && (ws.Cells[i, 4].Value.TextValue != "combo" || ws.Cells[i, 5].Value.TextValue != "?"))
                        {
                            VariableDataSet.Cur_AdvancedVariablesRow row = Plugin.settingsData.Cur_AdvancedVariables.NewCur_AdvancedVariablesRow();
                            row.VariableName = vname;
                            row.Category = ws.Cells[i, 1].Value.TextValue.ToUpper();
                            row.Description = ws.Cells[i, 2].Value.TextValue;
                            row.Countries = ws.Cells[i, 3].Value.TextValue;
                            row.VariableType = editorTranslate(ws.Cells[i, 4].Value.TextValue);
                            if (row.VariableType == Program.EDITOR_TYPE_NUMERIC)
                            {
                                numericEditor.setSingleValue(ws.Cells[i, 5].Value.TextValue == "?" ? "0" : ws.Cells[i, 5].Value.ToString());
                                row.DefaultValue = numericEditor.EditValue;
                            }
                            else
                                row.DefaultValue = ws.Cells[i, 5].Value.TextValue == "?" ? "0" : ws.Cells[i, 5].Value.ToString();
                            row.ValueRange = ws.Cells[i, 7].Value.TextValue == "?" ? "" : ws.Cells[i, 7].Value.ToString();
                            row.TextRange = ws.Cells[i, 8].Value.TextValue == "?" ? "" : ws.Cells[i, 8].Value.ToString();
                            Plugin.settingsData.Cur_AdvancedVariables.Rows.Add(row);
                        }
                    }
                }
                else if (ws.Cells[i, 9].Value.TextValue == "D")
                {
                    VariableDataSet.Cur_DerivedVariablesRow row;
                    if (Plugin.settingsData.Cur_DerivedVariables.Select("VariableName = '" + vname + "'").Length == 0)
                    {
                        row = Plugin.settingsData.Cur_DerivedVariables.NewCur_DerivedVariablesRow();
                        row.VariableName = vname;
                        row.Category = ws.Cells[i, 1].Value.TextValue.ToUpper();
                        row.Description = ws.Cells[i, 2].Value.TextValue;
                        row.Countries = ws.Cells[i, 3].Value.TextValue;
                        Plugin.settingsData.Cur_DerivedVariables.Rows.Add(row);
                    }
                    else
                    {
                        row = (VariableDataSet.Cur_DerivedVariablesRow)Plugin.settingsData.Cur_DerivedVariables.Select("VariableName = '" + vname + "'").First();
                    }

                    string cond = ws.Cells[i, 6].Value.TextValue == null ? "" : ws.Cells[i, 6].Value.TextValue;
                    if (cond != "")   // if this was not a default condition, then add it as an extra condition
                    {
                        VariableDataSet.Cur_DerivedVariablesDetailRow drow = Plugin.settingsData.Cur_DerivedVariablesDetail.NewCur_DerivedVariablesDetailRow();
                        drow.VariableName = vname;
                        drow.Condition = cond;
                        drow.DerivedValue = ws.Cells[i, 5].Value.ToString();
                        Plugin.settingsData.Cur_DerivedVariablesDetail.Rows.Add(drow);
                    }
                    else
                    {
                        row.DefaultValue = ws.Cells[i, 5].Value.ToString();
                    }
                }
                else if (ws.Cells[i, 9].Value.TextValue == "M")
                {
                    if (Plugin.settingsData.Cur_BasicVariables.Select("VariableName = '" + vname + "'").Length == 0)
                    {
                        VariableDataSet.Cur_BasicVariablesRow row = Plugin.settingsData.Cur_BasicVariables.NewCur_BasicVariablesRow();
                        row.VariableName = vname;
                        row.Category = ws.Cells[i, 1].Value.TextValue.ToUpper();
                        row.Description = ws.Cells[i, 2].Value.TextValue;
                        row.VariableType = editorTranslate(ws.Cells[i, 4].Value.TextValue);
                        row.ValueRange = ws.Cells[i, 7].Value.ToString();
                        row.TextRange = ws.Cells[i, 8].Value.ToString();
                        Plugin.settingsData.Cur_BasicVariables.Rows.Add(row);
                    }
                }
                i++;
            }

            Plugin.addMissingVariables();
            Plugin.removeExtraVariables();
        }

        private string editorTranslate(string x)
        {
            switch (x)
            {
                case "numeric": return Program.EDITOR_TYPE_NUMERIC;
                case "combo": return Program.EDITOR_TYPE_COMBO;
                case "connection": return Program.EDITOR_TYPE_CONNECTION;
                default: return "";
            }
        }

        private void gridHousehold_CellValueChanged(object sender, DevExpress.XtraVerticalGrid.Events.CellValueChangedEventArgs e)
        {
            MyEditorRow field = e.Row as MyEditorRow;
            if (field.isHouseholdVar)
            {
                string vn = field.Properties.FieldName;
                DataTable curTable = Plugin.householdData.Tables[householdBindingSource.DataMember];
                foreach (DataRow row in curTable.Rows)
                    row.SetField<string>(vn, e.Value.ToString());
            }
        }

        private void InputForm_Load(object sender, EventArgs e)
        {
            saveCurrentSettingsAsDefaultToolStripMenuItem.Visible = Plugin.isDeveloper;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GridEditorCloseAndSubmit();
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            string info = "HHoT v" + v.Major + "." + v.Minor + "." + v.Build;
            MessageBox.Show(this, info, "About HHoT", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void splitMainInterface_SplitterMoved(object sender, EventArgs e)
        {
            labelControl2.Refresh();
        }

        private void treeHouseholds_ShownEditor(object sender, EventArgs e)
        {
            treeHouseholds.ActiveEditor.BackColor = Color.Azure;
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            List<TreeListNode> hhNodes = treeHouseholds.Nodes.ToList();
            foreach (TreeListNode hn in hhNodes)
            {
                hn.Checked = chkSelectAll.Checked;
                hn.SetValue("isChecked", chkSelectAll.Checked);
                List<TreeListNode> childNodes = hn.Nodes.ToList();
                foreach (TreeListNode n in childNodes)
                {
                    n.Checked = chkSelectAll.Checked;
                    n.SetValue("isChecked", chkSelectAll.Checked);
                    int nodeId = (int)n.GetValue("dataId");
                    DataTable curTable = Plugin.householdData.Tables[hn.GetValue("HouseholdName").ToString()];
                    curTable.Rows.Find(nodeId).SetField<bool>("Checked", chkSelectAll.Checked);
                }
            }

            
        }

        private void saveCurrentSettingsAsDefaultToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plugin.saveDefaultSettings();
        }

        private void resetAllSettingsToDefaultSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Plugin.restoreDefaultSettings();
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!RequestSavingHouseholdData()) return;

            string projectPath;
            if (Plugin.CreateNewProject(out projectPath))
            {
                Cursor = Cursors.WaitCursor;
                Plugin.openProject(projectPath);
                Cursor = Cursors.Default;
            }
        }

        private void wizardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!checkAllOK()) return;
            StatisticsWizardForm wizard = new StatisticsWizardForm(Plugin, this);
            wizard.ShowDialog();
            wizard.StartPresenter(); // if the Presenter is started from the wizard it opens behind the HHot-window, thus start from here
        }

        private void resetAllAdvButton_Click(object sender, EventArgs e)
        {
            resetAllHouseholdsAdvancedVariables();
        }

        private void projectStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }

    class MyEditorRow : EditorRow
    {
        internal bool isAdvanced { get; set; }
        internal bool isDerived { get; set; }
        internal bool isHouseholdVar { get; set; }
        internal byte varType { get; set; }
        internal string defaultValue { get; set; }
        internal List<string> countries { get; set; }
        internal const byte BASIC_VARIABLE = 0;
        internal const byte ADVANCED_VARIABLE = 1;
        internal const byte ADVANCEDCOUNTRYSPECIFIC_VARIABLE = 2;
        internal const byte DERIVED_VARIABLE = 3;

        public MyEditorRow()
        {
            countries = new List<string>();
        }
    }

    class GridComboItem
    {
        public string Value { get; set; }
        public string Text { get; set; }
    }

    class RepositoryConnectionLookUpEdit : RepositoryItemLookUpEdit 
    { 
        private Program Plugin;                    // variable that links to the actual plugin
        private DevExpress.XtraVerticalGrid.VGridControl grid;

        public RepositoryConnectionLookUpEdit(Program _plugin, int fieldCounter, DevExpress.XtraVerticalGrid.VGridControl _grid)
        {
            grid = _grid;
            Plugin = _plugin;
            ShowHeader = false;
            ShowFooter = false;
            ShowPopupShadow = true;
            TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            BindingList<GridComboItem> conbl = new BindingList<GridComboItem>();
//            conbl.Add(new GridComboItem { Text = "empty", Value = "0" });
            DataSource = conbl;
            DisplayMember = "Text";
            ValueMember = "Value";
            Name = "combo" + fieldCounter;
            ForceInitialize();
            PopulateColumns();
            Columns["Value"].Visible = false;
            DropDownRows = 1;
            Enter += concombo_Enter;
            CustomDisplayText += RepositoryConnectionLookUpEdit_CustomDisplayText;
        }

        void RepositoryConnectionLookUpEdit_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
        {
            if (e.Value == null || e.Value.ToString() == "") e.DisplayText = "";
            else
            {
                DataTable tbl = Plugin.householdData.Tables[(grid.DataSource as System.Windows.Forms.BindingSource).DataMember];
                if (tbl == null) e.DisplayText = "";
                else
                {
                    DataRow[] rows = tbl.Select("dataId='" + e.Value + "'");
                    e.DisplayText = rows.Length == 0 || !rows[0].Field<bool>("checked") ? "" : rows[0]["PersonName"].ToString();
                }
            }
        }

        // this function will change in real-time the available values in a "connection" combo, e.g. idparent, idmother, idpartner
        void concombo_Enter(object sender, EventArgs e)
        {
            LookUpEdit concombo = sender as LookUpEdit;
            BindingList<GridComboItem> conbl = new BindingList<GridComboItem>();
            DataRowView drv = grid.GetRecordObject(grid.FocusedRecord) as DataRowView;
            int curPersonId = int.Parse(drv.Row.ItemArray[0].ToString());
            DataTable curTable = Plugin.householdData.Tables[(grid.DataSource as System.Windows.Forms.BindingSource).DataMember];
            int cnt = 1;
            conbl.Add(new GridComboItem { Text = "", Value = "0" });
            foreach (DataRow row in curTable.Rows)
            {
                if (row.Field<int>("dataId") != curPersonId && row.Field<bool>("checked"))
                {
                    conbl.Add(new GridComboItem { Text = row.Field<string>("PersonName"), Value = row.Field<int>("dataId").ToString() });
                    if (cnt < 7) cnt++;
                }
            }
            concombo.Properties.DataSource = conbl;
            concombo.Properties.DropDownRows = cnt;
        }
    }

    class RepositoryItemPopupContainerEditNumericRange : RepositoryItemPopupContainerEdit
    {
        internal Program Plugin;                    // variable that links to the actual plugin
        readonly bool isRange;
        readonly NumericEditor me;

        public RepositoryItemPopupContainerEditNumericRange(Program _plugin, bool _isRange = true)
        {
            Plugin = _plugin;
            isRange = _isRange;
            PopupSizeable = false;
            ShowPopupCloseButton = false;
            PopupFormSize = new Size(400, isRange ? 170 : 120);
            PopupContainerControl pcc = new PopupContainerControl();
            me = new NumericEditor(Plugin, isRange) { 
                Location = new Point(0, 0), 
                Size = new Size(400, isRange ? 170 : 120) 
            };
            QueryResultValue += pcemr_QueryResultValue;
            ParseEditValue += pcemr_ParseEditValue;
            QueryDisplayText += pcemr_QueryDisplayText;
            pcc.Controls.Add(me);
            CloseUpKey = new DevExpress.Utils.KeyShortcut(Keys.Enter);
            PopupControl = pcc;
        }

        void pcemr_QueryDisplayText(object sender, DevExpress.XtraEditors.Controls.QueryDisplayTextEventArgs e)
        {
            e.DisplayText = NumericEditor.GetDisplayText(e.EditValue, isRange, Plugin);
        }

        void pcemr_QueryResultValue(object sender, DevExpress.XtraEditors.Controls.QueryResultValueEventArgs e)
        {
            e.Value = me.EditValue;
        }

        void pcemr_ParseEditValue(object sender, DevExpress.XtraEditors.Controls.ConvertEditValueEventArgs e)
        {
            me.EditValue = e.Value==null?"":e.Value.ToString();
        }
    }
}

