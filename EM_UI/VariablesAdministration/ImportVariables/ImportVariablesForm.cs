using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.ImportVariables
{
    internal partial class ImportVariablesForm : Form
    {
        internal VarConfigFacade _internalVarConfigFacade = null;
        internal VarConfigFacade _externalVarConfigFacade = null;
        internal VariablesForm _variablesForm = null;

        const bool _actionDefaultPerform = true;
        internal const string _actionDelete = "delete";
        internal const string _actionAdd = "add";
        internal const string _actionChange = "change";
        internal const string _infoChangeCountryLabel = "different country description(s)";
        internal const string _infoNewDescription = "new description: ";
        internal const string _infoChangeCategory = "different category(ies)";

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void ImportVariablesForm_Shown(object sender, EventArgs e)
        {
            btnSelectImportFile_Click(null, null);
        }

        void btnImport_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            (new ImportVariablesManager()).PerformImport(this);
            _variablesForm._hasChangedSinceLastSave = true;
            _variablesForm.UpdateListsAndFilters(true, true, true);
            _variablesForm._acronymManager.UpdateAutomaticLabel();
            _variablesForm.UpdateListsAndFilters(true, false, false);
            Cursor = Cursors.Default;

            DialogResult = DialogResult.OK;
            Close();
        }

        void txtImportFile_Validating(object sender, CancelEventArgs e)
        {
            if (txtImportFile.Text == string.Empty)
                return;

            txtImportFile.Text = txtImportFile.Text.Trim();
            if (!txtImportFile.Text.ToLower().EndsWith(".xml"))
                txtImportFile.Text += ".xml";

            _externalVarConfigFacade = new VarConfigFacade(txtImportFile.Text);
            if (!_externalVarConfigFacade.LoadVarConfig())
                return;

            dgvVariables.Rows.Clear();
            dgvDescriptions.Rows.Clear();
            treeAcronyms.Nodes.Clear();
            dgvCategories.Rows.Clear();

            FillVariablesList();
            FillAcronymsList();
        }

        TreeListNode AcronymTree_AddNode(TreeListNode parentNode, object nodeTag, string action = "")
        {
            TreeListNode node = treeAcronyms.AppendNode(null, parentNode);
            if (action == string.Empty)
                node.SetValue(colPerformAcronyms, string.Empty); //do not show check box if just "header", e.g. type containing added/deleted/changed acronyms/levels, but not changed itself
            else
                node.SetValue(colPerformAcronyms, _actionDefaultPerform);
            node.SetValue(colActionAcronyms, action);            
            node.SetValue(colInfoAcronyms, string.Empty);
            node.Tag = nodeTag;

            //somewhat unelegant procedure, which serves only to determin the content of colAcronym
            string acronymText = string.Empty;
            VarConfig.AcronymTypeRow typeRow = null;
            VarConfig.AcronymLevelRow levelRow = null;
            VarConfig.AcronymRow acronymRow = null;

            bool tagIsDictionary = action == _actionChange || action == string.Empty;
            if (parentNode == null)
                typeRow = !tagIsDictionary ? nodeTag as VarConfig.AcronymTypeRow
                                          : (nodeTag as Dictionary<VarConfig.AcronymTypeRow, VarConfig.AcronymTypeRow>).Keys.ElementAt(0);
            else if (parentNode != null && parentNode.ParentNode == null)
                levelRow = !tagIsDictionary ? nodeTag as VarConfig.AcronymLevelRow
                                           : (nodeTag as Dictionary<VarConfig.AcronymLevelRow, VarConfig.AcronymLevelRow>).Keys.ElementAt(0);
            else
                acronymRow = !tagIsDictionary ? nodeTag as VarConfig.AcronymRow
                                             : (nodeTag as Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow>).Keys.ElementAt(0);

            if (typeRow != null)
                acronymText = typeRow.ShortName.ToUpper() + " (" + typeRow.LongName + ")";
            if (levelRow != null)
                acronymText = levelRow.Name;
            if (acronymRow != null)
                acronymText = acronymRow.Name + " (" + acronymRow.Description + ")";

            node.SetValue(colAcronym, acronymText);

            return node;
        }

        void FillAcronymsList()
        {
            //(1) ACRONYM TYPES: search for elements existing only in one version as well as elements with different descriptions and/or content
            List<VarConfig.AcronymTypeRow> internalTypes = _internalVarConfigFacade.GetAcronymTypesSortedByShortName();
            List<VarConfig.AcronymTypeRow> externalTypes = _externalVarConfigFacade.GetAcronymTypesSortedByShortName();

            List<string> internalIDs = (from internalType in internalTypes select internalType.ShortName).ToList();
            List<string> externalIDs = (from externalType in externalTypes select externalType.ShortName).ToList();

            Dictionary<int, int> compareTypeIndexList = new Dictionary<int, int>();
            List<int> addIndexList = new List<int>();
            List<int> deleteIndexList = new List<int>();

            AnalyseLists(internalIDs, externalIDs, ref compareTypeIndexList, ref addIndexList, ref deleteIndexList);

            //fill the found differences into the tree-control
            foreach (int addIndex in addIndexList) //elements only existent in external version
                AcronymTree_AddNode(null, externalTypes.ElementAt(addIndex), _actionAdd);

            foreach (int deleteIndex in deleteIndexList) //elements only existent in internal version
                AcronymTree_AddNode(null, internalTypes.ElementAt(deleteIndex), _actionDelete);

            foreach (int internalTypeIndex in compareTypeIndexList.Keys) //elements existent in both version: still have to check for different description and/or content
            {
                int externalTypeIndex = compareTypeIndexList[internalTypeIndex];
                VarConfig.AcronymTypeRow internalType = internalTypes.ElementAt(internalTypeIndex);
                VarConfig.AcronymTypeRow externalType = externalTypes.ElementAt(externalTypeIndex);
                Dictionary<VarConfig.AcronymTypeRow, VarConfig.AcronymTypeRow> typeTag = new Dictionary<VarConfig.AcronymTypeRow, VarConfig.AcronymTypeRow>();
                typeTag.Add(internalType, externalType); //put a Dictionary with one element into Tag, as KeyValuePair cannot be casted back, because it does not allow for NULL values

                TreeListNode typeNode = null;

                //check for different description (LongName)
                if (internalType.LongName != externalType.LongName)
                {
                    typeNode = AcronymTree_AddNode(null, typeTag, _actionChange);
                    typeNode.SetValue(colInfoAcronyms, _infoNewDescription + externalType.LongName);
                }

                //check for differnet content, i.e.:
                //(2) ACRONYM LEVELS: search for elements existing only in one version as well as elements with different content
                List<VarConfig.AcronymLevelRow> internalLevels = _internalVarConfigFacade.GetAcronymLevelsSortedByName(internalType);
                List<VarConfig.AcronymLevelRow> externalLevels = _externalVarConfigFacade.GetAcronymLevelsSortedByName(externalType);

                internalIDs = (from internalLevel in internalLevels select internalLevel.Name).ToList();
                externalIDs = (from externalLevel in externalLevels select externalLevel.Name).ToList();

                Dictionary<int, int> compareLevelIndexList = new Dictionary<int, int>();
                addIndexList.Clear();
                deleteIndexList.Clear();

                AnalyseLists(internalIDs, externalIDs, ref compareLevelIndexList, ref addIndexList, ref deleteIndexList);

                if (typeNode == null && (addIndexList.Count != 0 || deleteIndexList.Count != 0))
                    typeNode = AcronymTree_AddNode(null, typeTag); //generate parent node if necessary

                //fill the found differences into the tree-control
                foreach (int addIndex in addIndexList) //elements only existent in external version
                    AcronymTree_AddNode(typeNode, externalLevels.ElementAt(addIndex), _actionAdd);

                foreach (int deleteIndex in deleteIndexList) //elements only existent in internal version
                    AcronymTree_AddNode(typeNode, internalLevels.ElementAt(deleteIndex), _actionDelete);

                foreach (int internalLevelIndex in compareLevelIndexList.Keys) //elements existent in both version: still have to check for different content
                {
                    int externalLevelIndex = compareLevelIndexList[internalLevelIndex];
                    VarConfig.AcronymLevelRow internalLevel = internalLevels.ElementAt(internalLevelIndex);
                    VarConfig.AcronymLevelRow externalLevel = externalLevels.ElementAt(externalLevelIndex);
                    Dictionary<VarConfig.AcronymLevelRow, VarConfig.AcronymLevelRow> levelTag = new Dictionary<VarConfig.AcronymLevelRow, VarConfig.AcronymLevelRow>();
                    levelTag.Add(internalLevel, externalLevel); //put a Dictionary with one element into Tag, as KeyValuePair cannot be casted back, because it does not allow for NULL values

                    TreeListNode levelNode = null;

                    //check for differnet content, i.e.:
                    //(3) ACRONYMS: search for elements existing only in one version as well as elements with different description
                    List<VarConfig.AcronymRow> internalAcronyms = _internalVarConfigFacade.GetAcronymsOfLevelSortedByName(internalLevel);
                    List<VarConfig.AcronymRow> externalAcronyms = _externalVarConfigFacade.GetAcronymsOfLevelSortedByName(externalLevel);

                    internalIDs = (from internalAcronym in internalAcronyms select internalAcronym.Name).ToList();
                    externalIDs = (from externalAcronym in externalAcronyms select externalAcronym.Name).ToList();

                    Dictionary<int, int> compareAcronymIndexList = new Dictionary<int, int>();
                    addIndexList.Clear();
                    deleteIndexList.Clear();

                    AnalyseLists(internalIDs, externalIDs, ref compareAcronymIndexList, ref addIndexList, ref deleteIndexList);

                    if (addIndexList.Count != 0 || deleteIndexList.Count != 0)
                    {
                        if (typeNode == null) //generate parent nodes if necessary
                            typeNode = AcronymTree_AddNode(null, typeTag);
                        if (levelNode == null)
                            levelNode = AcronymTree_AddNode(typeNode, levelTag);
                    }

                    //fill the found differences into the tree-control
                    foreach (int addIndex in addIndexList) //elements only existent in external version
                        AcronymTree_AddNode(levelNode, externalAcronyms.ElementAt(addIndex), _actionAdd);

                    foreach (int deleteIndex in deleteIndexList) //elements only existent in internal version
                        AcronymTree_AddNode(levelNode, internalAcronyms.ElementAt(deleteIndex), _actionDelete);

                    foreach (int internalAcronymIndex in compareAcronymIndexList.Keys) //elements existent in both version: still have to check for different description
                    {
                        int externalAcronymIndex = compareAcronymIndexList[internalAcronymIndex];
                        VarConfig.AcronymRow internalAcronym = internalAcronyms.ElementAt(internalAcronymIndex);
                        VarConfig.AcronymRow externalAcronym = externalAcronyms.ElementAt(externalAcronymIndex);
                        Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow> acronymTag = new Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow>();
                        acronymTag.Add(internalAcronym, externalAcronym); //put a Dictionary with one element into Tag, as KeyValuePair cannot be casted back, because it does not allow for NULL values

                        TreeListNode acronymNode = null;

                        //check for different description 
                        if (internalAcronym.Description != externalAcronym.Description)
                        {
                            if (typeNode == null) //generate parent nodes if necessary
                                typeNode = AcronymTree_AddNode(null, typeTag);
                            if (levelNode == null)
                                levelNode = AcronymTree_AddNode(typeNode, levelTag);
                            acronymNode = AcronymTree_AddNode(levelNode, acronymTag, _actionChange);
                            acronymNode.SetValue(colInfoAcronyms, _infoNewDescription + externalAcronym.Description);
                        }

                        //check for different categories
                        bool equal = internalAcronym.GetCategoryRows().Count() == externalAcronym.GetCategoryRows().Count();
                        if (equal)
                        {
                            foreach (VarConfig.CategoryRow internalCategory in internalAcronym.GetCategoryRows())
                            {
                                bool found = false;
                                foreach (VarConfig.CategoryRow externalCategory in externalAcronym.GetCategoryRows())
                                    if (internalCategory.Value == externalCategory.Value && internalCategory.Description == externalCategory.Description)
                                    {
                                        found = true;
                                        break;
                                    }
                                if (!found)
                                {
                                    equal = false;
                                    break; //if one category different, acronym needs to be marked as changed - no need to check all categories here
                                }
                            }
                        }
                        if (!equal)
                        {
                            if (typeNode == null) //generate parent nodes if necessary
                                typeNode = AcronymTree_AddNode(null, typeTag);
                            if (levelNode == null)
                                levelNode = AcronymTree_AddNode(typeNode, levelTag);

                            string info = _infoChangeCategory;
                            if (acronymNode == null)
                                acronymNode = AcronymTree_AddNode(levelNode, acronymTag, _actionChange);
                            else
                                info = acronymNode.GetDisplayText(colInfoAcronyms) + " + " + _infoChangeCategory;
                            acronymNode.SetValue(colInfoAcronyms, info);
                        }
                    }
                }
            }
            
            colPerformAcronyms.BestFit();
            colAcronym.BestFit();
            colActionAcronyms.BestFit();
            colInfoAcronyms.BestFit();

            treeAcronyms.ExpandAll();
        }

        void treeAcronyms_CustomNodeCellEdit(object sender, GetCustomNodeCellEditEventArgs e)
        {
            if (e.Column != colPerformAcronyms)
                return;
            //do not show check box if just "header", e.g. type containing added/deleted/changed acronyms/levels, but not changed itself
            if (e.Node.GetDisplayText(colActionAcronyms) != string.Empty)
                e.RepositoryItem = rpiCheckEdit;
        }

        void FillVariablesList()
        {
            //analyse the two lists of variables for variables existing only in one of them as well as variables with different properties (monetary-status, descriptions)
            List<VarConfig.VariableRow> internalVariables = _internalVarConfigFacade.GetVariablesSortedByName();
            List<VarConfig.VariableRow> externalVariables = _externalVarConfigFacade.GetVariablesSortedByName();

            List<string> internalVariablesNames = (from internalVariable in internalVariables select internalVariable.Name).ToList();
            List<string> externalVariablesNames = (from externalVariable in externalVariables select externalVariable.Name).ToList();

            Dictionary<int, int> compareIndexList = new Dictionary<int, int>();
            List<int> addIndexList = new List<int>();
            List<int> deleteIndexList = new List<int>();

            AnalyseLists(internalVariablesNames, externalVariablesNames, ref compareIndexList, ref addIndexList, ref deleteIndexList);

            //add a row for each potential change to the variables list
            foreach (int addIndex in addIndexList)
            {
                int index = dgvVariables.Rows.Add(externalVariablesNames.ElementAt(addIndex), _actionAdd, _actionDefaultPerform, string.Empty);
                dgvVariables.Rows[index].Tag = externalVariables.ElementAt(addIndex);
            }

            foreach (int deleteIndex in deleteIndexList)
            {
                int index = dgvVariables.Rows.Add(internalVariablesNames.ElementAt(deleteIndex), _actionDelete, _actionDefaultPerform, string.Empty);
                dgvVariables.Rows[index].Tag = internalVariables.ElementAt(deleteIndex);
            }

            bool warningShown = false;
            foreach (int internalIndex in compareIndexList.Keys)
            {
                int externalIndex = compareIndexList[internalIndex];
                VarConfig.VariableRow internalVariable = internalVariables.ElementAt(internalIndex);
                VarConfig.VariableRow externalVariable = externalVariables.ElementAt(externalIndex);
                Dictionary<VarConfig.VariableRow, VarConfig.VariableRow> tag = new Dictionary<VarConfig.VariableRow, VarConfig.VariableRow>();
                tag.Add(internalVariable, externalVariable); //put a Dictionary with one element into Tag, as KeyValuePair cannot be casted back, because it does not allow for NULL values

                //check for different monetary status
                if (internalVariable.Monetary != externalVariable.Monetary)
                {
                    int index = dgvVariables.Rows.Add(internalVariablesNames.ElementAt(internalIndex), _actionChange, _actionDefaultPerform,
                        (externalVariable.Monetary == "1") ? "to monetary" : "to non-monetary");
                    dgvVariables.Rows[index].Tag = tag;
                }

                //check for different country specific descriptions
                foreach (VarConfig.CountryLabelRow internalLabel in internalVariable.GetCountryLabelRows())
                {
                    VarConfig.CountryLabelRow externalLabel = null;
                    foreach (VarConfig.CountryLabelRow searchExternalLabel in externalVariable.GetCountryLabelRows())
                        if (searchExternalLabel.Country.ToLower() == internalLabel.Country.ToLower())
                        {
                            externalLabel = searchExternalLabel;
                            break;
                        }

                    if (externalLabel == null)
                    {
                        if (!warningShown)
                        {
                            Tools.UserInfoHandler.ShowError("The external variables file seems to refer to another set of countries, thus country descriptions may not be updated completely.");
                            warningShown = true;
                        }
                        continue;
                    }

                    if (internalLabel.Label != externalLabel.Label)
                    {
                        int index = dgvVariables.Rows.Add(internalVariablesNames.ElementAt(internalIndex), _actionChange, _actionDefaultPerform, _infoChangeCountryLabel);
                        dgvVariables.Rows[index].Tag = tag;
                        break; //if one description different, variables needs to be marked as changed - no need to check all countries' descriptions here
                    }
                }
            }

            //finally some formatting
            colPerformVariables.Width = colPerformVariables.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colVariableName.Width = colVariableName.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colAction.Width = colAction.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colInfo.Width = colInfo.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);

            dgvVariables.Sort(colVariableName, System.ComponentModel.ListSortDirection.Ascending);

            FillCountrySpecificDescriptionList();
        }

        internal void FillCountrySpecificDescriptionList()
        {
            dgvDescriptions.Rows.Clear();
            if (dgvVariables.SelectedRows.Count != 1 || dgvVariables.SelectedRows[0].Tag == null)
                return; //don't show any definitions if no or more than one variable is selected

            if (dgvVariables.SelectedRows[0].Cells[dgvVariables.Columns.IndexOf(colInfo)].Value.ToString() != _infoChangeCountryLabel)
                return; //only need to fill list when selected row in variables list indicates different country labels

            Dictionary<VarConfig.VariableRow, VarConfig.VariableRow> internalAndExternVariable = dgvVariables.SelectedRows[0].Tag as Dictionary<VarConfig.VariableRow, VarConfig.VariableRow>; //Tag is a Dictionary with one element as KeyValuePair cannot be casted back, because it does not allow for NULL values
            VarConfig.VariableRow internalVariable = internalAndExternVariable.Keys.ElementAt(0);
            VarConfig.VariableRow externalVariable = internalAndExternVariable.Values.ElementAt(0);

            foreach (VarConfig.CountryLabelRow internalLabel in internalVariable.GetCountryLabelRows())
            {
                VarConfig.CountryLabelRow externalLabel = null;
                foreach (VarConfig.CountryLabelRow searchExternalLabel in externalVariable.GetCountryLabelRows())
                    if (searchExternalLabel.Country.ToLower() == internalLabel.Country.ToLower())
                    {
                        externalLabel = searchExternalLabel;
                        break; //just break if no corresponding country-label found (warning was issued when variables list was generated)
                    }

                if (externalLabel == null || internalLabel.Label == externalLabel.Label)
                    continue;

                dgvDescriptions.Rows.Add(internalLabel.Country, internalLabel.Label, externalLabel.Label);
            }

            colCountry.Width = colCountry.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colOldCountryDescription.Width = colOldCountryDescription.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colNewCountryDescription.Width = colNewCountryDescription.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
        }

        void FillCategoriesList()
        {
            dgvCategories.Rows.Clear();

            if (treeAcronyms.FocusedNode == null || treeAcronyms.FocusedNode.ParentNode == null || treeAcronyms.FocusedNode.ParentNode.ParentNode == null)
                return; //no acronym node

            if (!treeAcronyms.FocusedNode.GetDisplayText(colInfoAcronyms).Contains(_infoChangeCategory))
                return; //no node indicating a category difference

            VarConfig.AcronymRow internalAcronym = (treeAcronyms.FocusedNode.Tag as Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow>).Keys.ElementAt(0);
            VarConfig.AcronymRow externalAcronym = (treeAcronyms.FocusedNode.Tag as Dictionary<VarConfig.AcronymRow, VarConfig.AcronymRow>).Values.ElementAt(0);

            //search for categories existing in internal list only and for categories with different description or value
            foreach (VarConfig.CategoryRow internalCategory in internalAcronym.GetCategoryRows())
            {
                bool found = false;
                foreach (VarConfig.CategoryRow externalCategory in externalAcronym.GetCategoryRows())
                {
                    if (internalCategory.Value != externalCategory.Value && internalCategory.Description != externalCategory.Description)
                        continue;
                    
                    found = true;
                    if (internalCategory.Value != externalCategory.Value || internalCategory.Description != externalCategory.Description)
                        dgvCategories.Rows.Add(internalCategory.Value, internalCategory.Description, externalCategory.Value, externalCategory.Description);
                    break;
                }
                if (!found)
                    dgvCategories.Rows.Add(internalCategory.Value, internalCategory.Description, "-", "-");
            }

            //search for categories existing in external list only
            foreach (VarConfig.CategoryRow externalCategory in externalAcronym.GetCategoryRows())
            {
                bool found = false;
                foreach (VarConfig.CategoryRow internalCategory in internalAcronym.GetCategoryRows())
                {
                    if (internalCategory.Value == externalCategory.Value || internalCategory.Description == externalCategory.Description)
                    {//it is enough that value OR description are equal, as only categories existing in the external list only are searched
                        found = true;
                        break;
                    }
                }
                if (!found)
                    dgvCategories.Rows.Add("-", "-", externalCategory.Value, externalCategory.Description);
            }
        }

        void AnalyseLists(List<string> internalList, List<string> externalList, ref Dictionary<int, int> compareIndexList, ref List<int> addIndexList, ref List<int> deleteIndexList)
        {
            //find elements which exist only in internal list: potentially delete, thus add to deleteIndexList
            //find elements which exist only in external list: potentially add, thus add to addIndexList
            //find elements which exist in both lists: potentially different, thus add to compareIndexList
            for (int i = 0, e = 0; i < internalList.Count || e < externalList.Count; ++i)
            {
                string internalElement = string.Empty;
                if (i < internalList.Count)
                    internalElement = internalList.ElementAt(i);
                
                string externalElement = string.Empty;
                if (e < externalList.Count)
                    externalElement = externalList.ElementAt(e);

                //finished with internal list, but there are still elements in external list -> potentially add
                if (internalElement == string.Empty)
                {
                    addIndexList.Add(e);
                    ++e;
                    continue;
                }

                //finished with external list, but there are still elements in internal list -> potentially delete
                if (externalElement == string.Empty)
                {
                    deleteIndexList.Add(i);
                    continue;
                }

                //element exists in both lists -> potentially different
                if (internalElement == externalElement)
                {
                    compareIndexList.Add(i, e);
                    ++e;
                    continue;
                }

                //current element in internal list is smaller than current element in external list -> element exists only in internal list -> potentially delete
                if (string.Compare(internalElement, externalElement) < 0)
                {
                    deleteIndexList.Add(i);
                    continue;
                }

                //current element in external list is smaller than current element in internal list -> element exists only in external list -> potentially add
                addIndexList.Add(e);
                ++e; --i;
            }
        }

        void dgvVariables_SelectionChanged(object sender, EventArgs e)
        {
            FillCountrySpecificDescriptionList();
        }

        void treeAcronyms_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
        {
            FillCategoriesList();
        }

        void btnSelectImportFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "xml files (*.xml)|*.xml";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            openFileDialog.Title = "Select variables configuration file to import ...";

            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            txtImportFile.Text = openFileDialog.FileName;
            txtImportFile_Validating(null, null);
        }

        internal ImportVariablesForm(VarConfigFacade varConfigFacade, VariablesForm variablesForm)
        {
            _internalVarConfigFacade = varConfigFacade;
            _variablesForm = variablesForm;

            InitializeComponent();
        }

        void btnTickSelectedVariables_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvVariables.SelectedRows)
                row.Cells[colPerformVariables.Index].Value = true;
        }

        void btnUntickSelectedVariables_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dgvVariables.SelectedRows)
                row.Cells[colPerformVariables.Index].Value = false;
        }

        void btnAddOnly_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            foreach (DataGridViewRow row in dgvVariables.Rows)
                if (row.Cells[colAction.Index].Value.ToString() == _actionDelete)
                    row.Cells[colPerformVariables.Index].Value = false;

            foreach (TreeListNode typeNode in treeAcronyms.Nodes)
            {
                if (typeNode.GetValue(colActionAcronyms).ToString() == _actionDelete)
                    typeNode.SetValue(colPerformAcronyms, false);
                foreach (TreeListNode levelNode in typeNode.Nodes)
                {
                    if (levelNode.GetValue(colActionAcronyms).ToString() == _actionDelete)
                        levelNode.SetValue(colPerformAcronyms, false);
                    foreach (TreeListNode acronymNode in levelNode.Nodes)
                    {
                        if (acronymNode.GetValue(colActionAcronyms).ToString() == _actionDelete)
                            acronymNode.SetValue(colPerformAcronyms, false);
                    }
                }
            }

            Cursor = Cursors.Default;
        }
    }
}
