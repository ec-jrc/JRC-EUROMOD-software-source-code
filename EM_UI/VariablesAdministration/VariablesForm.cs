using DevExpress.XtraBars;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using EM_UI.VariablesAdministration.CleanVariables;
using EM_UI.VariablesAdministration.ImportVariables;
using EM_UI.VariablesAdministration.VariablesActions;
using EM_UI.VariablesAdministration.VariablesManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration
{
    internal partial class VariablesForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        internal bool _hasChangedSinceLastSave = false;
        internal ADOUndoManager _undoManager = null;
        internal VarConfigFacade _varConfigFacade = null;
        internal AcronymManager _acronymManager = null;
        internal VariablesManager _variablesManager = null;
        internal List<BarEditItem> _typeFilterCheckboxes = null;

        internal const string _anyCountry = "Any Country";
        internal const string _allAcronymTypes = "ALL ACRONYMS";
        internal const string OtherCountryDescription = "other";

        internal bool _isReadOnly = false;

        void VariablesForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            btnSwitchablePolicies.Enabled = !EM_AppContext.Instance.IsPublicVersion();

            try
            {
                _undoManager = new ADOUndoManager();
                _undoManager.AddDataSet(_varConfigFacade._varConfig);
                _acronymManager = new AcronymManager(this);
                _variablesManager = new VariablesManager(this);

                _acronymManager.FillAcronymsList();
                _variablesManager.FillVariablesList();

                //fill the combo-box containing all countries to filter for country specific descriptions
                RepositoryItemComboBox ricmbCountry = cmbCountry.Edit as RepositoryItemComboBox;
                ricmbCountry.Items.Clear();
                ricmbCountry.Items.Add(_anyCountry);
                foreach (CountryAdministration.Country country in CountryAdministration.CountryAdministrator.GetCountries())
                    ricmbCountry.Items.Add(country._shortName);
                cmbCountry.EditValue = _anyCountry;

                //fill the combo-box containing all acronym types for searching an acronym
                //(should actually be updated if an acronym type is added/removed, but considering this is a very uncommon event and a rather unimportant function ...)
                RepositoryItemComboBox ricmbAcronymType = cmbAcronymType.Edit as RepositoryItemComboBox;
                ricmbAcronymType.Items.Clear();
                ricmbAcronymType.Items.Add(_allAcronymTypes);
                foreach (VarConfig.AcronymTypeRow acroTypeRow in _varConfigFacade.GetAllAcronyms())
                    ricmbAcronymType.Items.Add(acroTypeRow.LongName.ToUpper());
                cmbAcronymType.EditValue = ricmbAcronymType.Items[0].ToString();

                //add data-dependant filter-checkboxes (show benefit variables, show demographic variables, ...)
                DrawFilterCheckboxes();

                //_varConfigFacade contains a list of variables and acronyms created in the current session
                _varConfigFacade.ClearNewRowsList();

                SetButtonGreyState();
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        void SetButtonGreyState()
        {
            btnUndo.Enabled = _undoManager != null && _undoManager.HasChanges();
            btnRedo.Enabled = _undoManager != null && _undoManager.CanRedo();

            btnAddVariable.Enabled = true;
            btnDeleteVariable.Enabled = dgvVariables.SelectedRows.Count > 0;

            btnAddAcronym.Enabled = treeAcronyms.FocusedNode != null && !AcronymManager.IsTypeNode(treeAcronyms.FocusedNode);
            btnDeleteAcronym.Enabled = AcronymManager.IsAcronymNode(treeAcronyms.FocusedNode);

            btnAddLevel.Enabled = treeAcronyms.FocusedNode != null;
            btnDeleteLevel.Enabled = AcronymManager.IsLevelNode(treeAcronyms.FocusedNode);

            btnAddType.Enabled = true;
            btnDeleteType.Enabled = AcronymManager.IsTypeNode(treeAcronyms.FocusedNode);

            btnAddCategory.Enabled = AcronymManager.IsAcronymNode(treeAcronyms.FocusedNode);
            btnDeleteCategories.Enabled = dgvCategories.SelectedRows.Count > 0;

            btnApplyFilters.Enabled = true;

            btnImportVariables.Enabled = !_isReadOnly && _varConfigFacade != null && _undoManager != null && !_undoManager.HasChanges() && _hasChangedSinceLastSave == false;
            btnCleanVariables.Enabled = !_isReadOnly && _varConfigFacade != null && _undoManager != null && !_undoManager.HasChanges() && _hasChangedSinceLastSave == false;

            btnSave.Enabled = !_isReadOnly;
        }

        void dgvVariables_SelectionChanged(object sender, EventArgs e) { _variablesManager.FillCountrySpecificDescriptionList(); SetButtonGreyState(); }
        void treeAcronyms_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e) { _acronymManager.FillCategoriesList(e.Node); SetButtonGreyState(); }
        void dgvDescriptions_SelectionChanged(object sender, EventArgs e) { SetButtonGreyState(); }
        void dgvCategories_SelectionChanged(object sender, EventArgs e) { SetButtonGreyState(); }
        void dgvVariables_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e) { if (_acronymManager == null || e.ColumnIndex != dgvVariables.Columns.IndexOf(colVariableName)) return; _acronymManager.HandleBeginVariableEdit(e); }
        void dgvVariables_CellEndEdit(object sender, DataGridViewCellEventArgs e) { _acronymManager.HandleEndVariableEdit(e); _variablesManager.HandleEndVariableEdit(e); }
        void dgvVariables_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) { _variablesManager.HandleValidateVariable(e); }
        void dgvDescriptions_CellEndEdit(object sender, DataGridViewCellEventArgs e) { if (colCountryDescription.Index == e.ColumnIndex) PerformAction(new ChangeDescriptionAction(this, e)); }
        void dgvCategories_CellEndEdit(object sender, DataGridViewCellEventArgs e) { PerformAction(new ChangeCategoryAction(this, e)); }
        void btnApplyFilters_ItemClick(object sender, ItemClickEventArgs e) { _variablesManager.SetFilter(); }
        void btnUndo_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new VariablesUndoAction(_undoManager)); }
        void btnRedo_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new VariablesRedoAction(_undoManager)); }
        void btnAddVariable_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new AddVariableAction(dgvVariables, _varConfigFacade)); }
        void btnDeleteVariable_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new DeleteVariableAction(dgvVariables, _varConfigFacade)); }
        void btnClose_ItemClick(object sender, ItemClickEventArgs e) { this.Close(); }
        void btnSave_ItemClick(object sender, ItemClickEventArgs e) { SaveChanges(); }
        void btnUpdateAutomaticLabel_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new UpdateAutomaticLabelAction(this)); } //this button is actually not visible, as it is not necessary for the user to update the labels (labels are updated automatically at each change), however keep if gets necessary for some reason
        void btnAddType_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new AddAcronymTypeAction(this)); }
        void btnAddLevel_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new AddAcronymLevelAction(this)); }
        void btnAddAcronym_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new AddAcronymAction(this)); }
        void btnAddCategory_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new AddCategoryAction(this)); }
        void treeAcronyms_CellValueChanged(object sender, CellValueChangedEventArgs e) { _acronymManager.HandleChangeAcronymAction(e); }
        void btnDeleteType_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new DeleteAcronymTypeAction(this)); }
        void btnDeleteLevel_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new DeleteAcronymLevelAction(this)); }
        void btnDeleteAcronym_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new DeleteAcronymAction(this)); }
        void btnDeleteCategories_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new DeleteCategoryAction(this)); }
        void treeAcronyms_ShowingEditor(object sender, CancelEventArgs e) { e.Cancel = _acronymManager.IsCellReadOnly(e); }
        void btnImportVariables_ItemClick(object sender, ItemClickEventArgs e) { ImportVariablesForm form = new ImportVariablesForm(_varConfigFacade, this); form.ShowDialog(); }
        void btnCleanVariables_ItemClick(object sender, ItemClickEventArgs e) { CleanVariablesForm form = new CleanVariablesForm(_varConfigFacade, this); form.ShowDialog(); }
        void btnSelectAllFilters_ItemClick(object sender, ItemClickEventArgs e) { _variablesManager.SelectAllFilters(true); }
        void btnUnselectAllFilters_ItemClick(object sender, ItemClickEventArgs e) { _variablesManager.SelectAllFilters(false); }
        void btnSearchVariable_ItemClick(object sender, ItemClickEventArgs e) { _variablesManager.SearchVariable(); }
        void btnSearchAcronym_ItemClick(object sender, ItemClickEventArgs e) { _acronymManager.SearchAcronymsByNameOrDescription(true); }
        void btnSearchAcronymDescription_ItemClick(object sender, ItemClickEventArgs e) { _acronymManager.SearchAcronymsByNameOrDescription(false); }

        private void VariablesForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                if (_isReadOnly)
                    UserInfoHandler.ShowInfo("File is in read-only mode.");
                else
                    SaveChanges();
            }
            if (e.Control && e.KeyCode == Keys.Z)
                PerformAction(new VariablesUndoAction(_undoManager));
            if (e.Control && e.KeyCode == Keys.Y)
                PerformAction(new VariablesRedoAction(_undoManager));
            if (treeAcronyms.Focused == true)
                _acronymManager.HandleEnterKey(e);
            if (e.Control && e.KeyCode == Keys.V && dgvVariables.Focused == true)
                CopyVariablesFromClipboard();
        }

        void CopyVariablesFromClipboard()
        {
			// TODO
        }

        void VariablesForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_hasChangedSinceLastSave && !_isReadOnly)
            {
                DialogResult result = Tools.UserInfoHandler.GetInfo("Do you want to save the changes?", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }

                if (result == DialogResult.Yes && !SaveChanges())
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (!_isReadOnly)
                InUseFileHandler.ReleaseFile(new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true)); //files can now be used by other users again
            EM_AppContext.Instance.ReleaseVarConfigFacade();
        }

        internal bool SaveChanges()
        {
            Cursor = Cursors.WaitCursor;
            string errList = _variablesManager.CheckForEmptyRows();
            errList += _acronymManager.CheckForEmptyRows();

            if (errList != string.Empty)
            {
                Cursor = Cursors.Default;
                errList = "The following variables/acronyms are invalid:\n\n" + errList + "\nPlease correct!";
                Tools.UserInfoHandler.ShowError(errList);
                return false;
            }

            _varConfigFacade.WriteXML();
            _varConfigFacade.RefreshVariables_NamesAndDescriptions(); // update for intellisense
            _hasChangedSinceLastSave = false;

            SetButtonGreyState();
            Cursor = Cursors.Default;

            return true;
        }

        void dgvVariables_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {//draw row numbers in variables list
            string rowNumber = (e.RowIndex + 1).ToString();
            float height = e.Graphics.MeasureString(rowNumber, dgvVariables.Font).Height;
            e.Graphics.DrawString(rowNumber, dgvVariables.Font, System.Drawing.SystemBrushes.ControlText,
                        e.RowBounds.Location.X + 15, e.RowBounds.Location.Y + ((e.RowBounds.Height - height) / 2));
        }

        internal VariablesForm(VarConfigFacade varConfigFacade)
        {
            _varConfigFacade = varConfigFacade;
            InitializeComponent();
        }

        internal void PerformAction(VariablesBaseAction action)
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                if (action.Perform() == false)
                    return;

                _undoManager.Commit();
                _hasChangedSinceLastSave = true;

                UpdateListsAndFilters(action.UpdateVariables(), action.UpdateAcronyms(), action.UpdateFilterCheckboxes());
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        internal void UpdateListsAndFilters(bool updateVariables, bool updateAcronyms, bool updateFilterCheckboxes)
        {
            if (updateFilterCheckboxes)
                DrawFilterCheckboxes();
            if (updateAcronyms)
                _acronymManager.FillAcronymsList();
            if (updateVariables)
            {
                _variablesManager.FillVariablesList();
                _variablesManager.SetFilter();
            }

            SetButtonGreyState();
        }

        void DrawFilterCheckboxes()
        {
            List<string> uncheckedFilters = new List<string>();
            if (_typeFilterCheckboxes != null)
            {
                foreach (BarEditItem typeFilterCheckbox in _typeFilterCheckboxes)
                {
                    if ((bool)typeFilterCheckbox.EditValue == false && typeFilterCheckbox.Tag != null)
                        uncheckedFilters.Add(typeFilterCheckbox.Tag.ToString().ToLower());
                    rpgFilter.ItemLinks.Remove(typeFilterCheckbox);
                }
            }

            bool beginGroup = true;
            _typeFilterCheckboxes = new List<BarEditItem>();
            BarEditItem filterCheckbox = null;
            foreach (VarConfig.AcronymTypeRow acroTypeRow in _varConfigFacade.GetAllAcronyms())
            {
                if (acroTypeRow.ShortName == string.Empty)
                    continue; //probably just added type, which does not yet have a name
                filterCheckbox = new BarEditItem();
                filterCheckbox.Edit = repositoryItemCheckEdit;
                filterCheckbox.Caption = acroTypeRow.LongName.ToUpper();
                filterCheckbox.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far; //to show checkbox left of text (default is right)
                filterCheckbox.EditValue = !uncheckedFilters.Contains(acroTypeRow.ShortName);
                filterCheckbox.Tag = acroTypeRow.ShortName;
                rpgFilter.ItemLinks.Add(filterCheckbox, beginGroup);
                _typeFilterCheckboxes.Add(filterCheckbox);
                beginGroup = false;
            }

            //add checkbox 'IDENTIFIER': this is not a real type, but otherwise the variables disappear when filters are applied
            filterCheckbox = new BarEditItem();
            filterCheckbox.Edit = repositoryItemCheckEdit;
            filterCheckbox.Caption = "IDENTIFIER";
            filterCheckbox.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            filterCheckbox.EditValue = !uncheckedFilters.Contains(AcronymManager.IDENTIFIER_TYPE.ToLower());
            filterCheckbox.Tag = AcronymManager.IDENTIFIER_TYPE;
            rpgFilter.ItemLinks.Add(filterCheckbox);
            _typeFilterCheckboxes.Add(filterCheckbox);

            //add checkbox 'UNKNOWN': for not "losing" the variables with unknown type
            filterCheckbox = new BarEditItem();
            filterCheckbox.Edit = repositoryItemCheckEdit;
            filterCheckbox.Caption = "UNKNOWN";
            filterCheckbox.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            filterCheckbox.EditValue = !uncheckedFilters.Contains(VariablesManager.DESCRIPTION_UNKNOWN);
            filterCheckbox.Tag = VariablesManager.DESCRIPTION_UNKNOWN;
            rpgFilter.ItemLinks.Add(filterCheckbox);
            _typeFilterCheckboxes.Add(filterCheckbox);
        }

        void Control_Enter(object sender, EventArgs e)
        {//use this 'hand-knittet' approach to indicate which list is active, as it's nearly impossible to find out which of the x appearance settings has to be changed
            Control lblLeftList = null;
            if (sender.Equals(dgvVariables))
                lblLeftList = lblVariables;
            else if (sender.Equals(treeAcronyms))
                lblLeftList = lblAcronyms;
            else if (sender.Equals(dgvDescriptions))
                lblLeftList = lblDescriptions;
            else if (sender.Equals(dgvCategories))
                lblLeftList = lblCategories;
            lblLeftList.BackColor = System.Drawing.Color.Gray;
            lblLeftList.ForeColor = System.Drawing.Color.White;
        }

        void Control_Leave(object sender, EventArgs e)
        {
            Control lblLeftList = null;
            if (sender.Equals(dgvVariables))
                lblLeftList = lblVariables;
            else if (sender.Equals(treeAcronyms))
                lblLeftList = lblAcronyms;
            else if (sender.Equals(dgvDescriptions))
                lblLeftList = lblDescriptions;
            else if (sender.Equals(dgvCategories))
                lblLeftList = lblCategories;
            lblLeftList.BackColor = System.Drawing.Color.Transparent;
            lblLeftList.ForeColor = System.Drawing.Color.Black;
        }

        void btnExpandAcronyms_ItemClick(object sender, ItemClickEventArgs e)
        {
            treeAcronyms.ExpandAll();
        }

        void btnCollapseAcronyms_ItemClick(object sender, ItemClickEventArgs e)
        {
            treeAcronyms.CollapseAll();
        }

        void repositoryItemTextEditSearchVariable_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                txtSearchVariable.EditValue = (sender as DevExpress.XtraEditors.TextEdit).Text;   // make sure the text is updated
                _variablesManager.SearchVariable();
            }
        }

        void repositoryItemTextEditSearchAcronym_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.Handled = true;
                txtSearchAcronym.EditValue = (sender as DevExpress.XtraEditors.TextEdit).Text;   // make sure the text is updated
                _acronymManager.SearchAcronymsByNameOrDescription(true);
            }
        }

        private void dgvDescriptions_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            if (e.CellValue1.ToString().Equals(OtherCountryDescription))
            {
                e.SortResult = 1;
                e.Handled = true;
            }
            else if (e.CellValue2.ToString().Equals(OtherCountryDescription))
            {
                e.SortResult = -1;
                e.Handled = true;
            }
        }
    }
}