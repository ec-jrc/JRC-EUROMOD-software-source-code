using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.VariablesAdministration.VariablesManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.CleanVariables
{
    internal partial class CleanVariablesForm : Form
    {
        VarConfigFacade _varConfigFacade = null;
        VariablesForm _variablesForm = null;
        List<string> _allCountriesParameters = null;
        Point _mousePositionWhenMenuOpened;
        int hitGrid;

        List<VarConfig.VariableRow> _variablesToDisplay = null;
        List<string> _variablesToDelete = null;
        List<VarConfig.AcronymRow> _acronymsToDisplay = null;

        void btnClean_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;

            PerformClean();
            _variablesForm._hasChangedSinceLastSave = true;
            _variablesForm.UpdateListsAndFilters(true, true, true);
            _variablesForm._acronymManager.UpdateAutomaticLabel();
            _variablesForm.UpdateListsAndFilters(true, false, false);

            Cursor = Cursors.Default;

            DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        void PerformClean()
        {
            try
            {
                //delete checked variables
                foreach (DataGridViewRow variableRow in dgvVariables.Rows)
                {
                    if (EM_Helpers.SaveConvertToBoolean(variableRow.Cells[colDeleteVariables.Name].Value) == true)
                        (variableRow.Tag as VarConfig.VariableRow).Delete();
                }
                _varConfigFacade.Commit();

                //delete checked acronyms
                foreach (TreeListNode typeNode in treeAcronyms.Nodes)
                {
                    foreach (TreeListNode levelNode in typeNode.Nodes)
                    {
                        foreach (TreeListNode acroNode in levelNode.Nodes)
                        {
                            if (EM_Helpers.SaveConvertToBoolean(acroNode.GetValue(colDeleteAcronyms)))
                                (acroNode.Tag as VarConfig.AcronymRow).Delete();
                        }
                        _varConfigFacade.Commit();

                        VarConfig.AcronymLevelRow levelRow = levelNode.Tag as VarConfig.AcronymLevelRow;
                        if (levelRow.GetAcronymRows().Count() == 0)
                            levelRow.Delete(); //delete level if all contained acronyms were deleted
                    }
                    _varConfigFacade.Commit();

                    VarConfig.AcronymTypeRow typeRow = typeNode.Tag as VarConfig.AcronymTypeRow;
                    if (typeRow.GetAcronymLevelRows().Count() == 0)
                        typeRow.Delete(); //delete type if all contained levels (and acronyms) were deleted (not very likely to happen)
                    _varConfigFacade.Commit();
                }
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        void btnLoad_Click(object sender, EventArgs e)
        {
            if (EM_AppContext.Instance.HasChanges() &&
                Tools.UserInfoHandler.GetInfo("There are unsaved changes for one or more countries. These will not be taken into account.", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
            {
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            //the handler passed to the progress indicator will do the work (see below)
            ProgressIndicator progressIndicator = new ProgressIndicator(Load_BackgroundEventHandler, "Checking for unused variables");
            if (progressIndicator.ShowDialog() == System.Windows.Forms.DialogResult.Cancel) //user cancelled process
                return;

            //display results, i.e. unused variables and acronyms
            foreach (VarConfig.VariableRow variableToDisplay in _variablesToDisplay)
            {
                int index = dgvVariables.Rows.Add(variableToDisplay.Name, variableToDisplay.AutoLabel, true);
                dgvVariables.Rows[index].Tag = variableToDisplay;
            }
            colDeleteVariables.Width = colDeleteVariables.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colVariableName.Width = colVariableName.GetPreferredWidth(DataGridViewAutoSizeColumnMode.AllCells, true);
            colVariableDescription.Width = colVariableDescription.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, true);
            dgvVariables.Sort(colVariableName, System.ComponentModel.ListSortDirection.Ascending);

            DisplayUnusedAcronyms(true);
            
            btnClean.Enabled = true;
            btnLoad.Enabled = false;
        }

        void Load_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel    

            //initialisations
            const double progressPart_LoadingCountries = 0.3;
            const double progressPart_CheckingVariables = 0.6;
            const double progressPart_CheckingAcronyms = 0.1;

            _allCountriesParameters = new List<string>();
            _variablesToDelete = new List<string>();

            //load all countries' parameter files
            List<string> countryShortNames = _varConfigFacade.GetCountriesShortNames();
            for (int i = 0; i < countryShortNames.Count; ++i) //not the very best solution to get the countries from the descriptions in VarConfig.xml (would be cleaner to have a country administration)
            {
                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button

                string countryShortName = countryShortNames.ElementAt(i);
                CountryConfigFacade countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(countryShortName);
                if (countryConfigFacade == null)
                    continue; //VarConfig.xml may contain country specific descriptions for countries which are not (yet) implemented
                _allCountriesParameters.AddRange(countryConfigFacade.GetParameterValuesPossiblyContainingVariables()); //get all parameters of the country which may contain a variable

                double progressPart = (i + 1.0) / (countryShortNames.Count) * progressPart_LoadingCountries;
                backgroundWorker.ReportProgress(Convert.ToInt32(progressPart * 100.0));
            }

            //check for unused variables and acronyms to be displayed in btnLoad_Click, as displaying them here isn't possible, as the list and tree belong to another thread
            if (!CheckForUnusedVariables(progressPart_LoadingCountries, progressPart_CheckingVariables, backgroundWorker) ||
                !CheckForUnusedAcronyms(progressPart_LoadingCountries + progressPart_CheckingVariables, progressPart_CheckingAcronyms, backgroundWorker))
                e.Cancel = true; //user pressed Cancel button
        }

        bool CheckForUnusedAcronyms(double progressSoFar = 0.0, double progressToAchieve = 0.0, BackgroundWorker backgroundWorker = null)
        {
            if (_acronymsToDisplay == null)
                _acronymsToDisplay = new List<VarConfig.AcronymRow>();
            else
                _acronymsToDisplay.Clear();

            int acronymsCount = _varConfigFacade.GetAcronymsCount(); //for displaying progress, see below
            int i = 0;

            //get all variables to check whether they use an acronym ...
            List<string> variablesNames = _varConfigFacade.GetVariables_Names();
            foreach (VarConfig.AcronymTypeRow typeRow in _varConfigFacade.GetAllAcronyms())
            {
                if (backgroundWorker != null && backgroundWorker.CancellationPending) return false;
                foreach (VarConfig.AcronymLevelRow levelRow in typeRow.GetAcronymLevelRows())
                {
                    if (backgroundWorker != null && backgroundWorker.CancellationPending) return false;
                    foreach (VarConfig.AcronymRow acroRow in levelRow.GetAcronymRows())
                    {
                        if (backgroundWorker != null && backgroundWorker.CancellationPending) return false;
                        bool used = false;
                        foreach (string variableName in variablesNames)
                        {
                            if (backgroundWorker != null && backgroundWorker.CancellationPending) return false;
                            if (_variablesToDelete.Contains(variableName.ToLower()))
                                continue; //... but exempt variables which are to be deleted from the check

                            if (AcronymManager.IsAcronymOfVariable(variableName, acroRow.Name, typeRow.ShortName))
                            {
                                used = true;
                                break;
                            }
                        }

                        if (!used) //store unused acronyms to be displayed outside (via DisplayAcronyms), as this function is (amongst others) used by the background worker,
                            _acronymsToDisplay.Add(acroRow);  //thus displaying here isn't possible, as the acronym-tree-list belongs to another thread

                        if (backgroundWorker != null)
                        {
                            double progressPart = (i++ + 1.0) / acronymsCount * progressToAchieve;
                            backgroundWorker.ReportProgress(Convert.ToInt32((progressSoFar + progressPart) * 100.0));
                        }
                    }
                }
            }

            return true;
        }

        void DisplayUnusedAcronyms(bool expand) //... as assessed in CheckForUnusedAcronyms
        {
            treeAcronyms.BeginUnboundLoad();

            //store which nodes are expanded, the focused node and the first visible node
            List<string> expandedNodesIDs = new List<string>();
            string focusedNodeID = string.Empty;
            string topVisibleNodeID = string.Empty;
            AcronymManager.StoreNodeStates(treeAcronyms, ref expandedNodesIDs, ref focusedNodeID, ref topVisibleNodeID);

            treeAcronyms.Nodes.Clear();

            TreeListNode typeNode = null;
            TreeListNode levelNode = null;
            foreach (VarConfig.AcronymRow acroRow in _acronymsToDisplay)
            {
                VarConfig.AcronymTypeRow typeRow = acroRow.AcronymLevelRow.AcronymTypeRow;
                if (typeNode == null || !(typeNode.Tag as VarConfig.AcronymTypeRow).Equals(typeRow))
                {
                    typeNode = treeAcronyms.AppendNode(null, null);
                    typeNode.SetValue(colAcronym, typeRow.LongName.ToUpper() + " (" + typeRow.ShortName.ToUpper() + ")");
                    typeNode.Tag = typeRow;
                }
                
                VarConfig.AcronymLevelRow levelRow = acroRow.AcronymLevelRow;
                if (levelNode == null || !(levelNode.Tag as VarConfig.AcronymLevelRow).Equals(levelRow))
                {
                    levelNode = treeAcronyms.AppendNode(null, typeNode);
                    levelNode.SetValue(colAcronym, levelRow.Name);
                    levelNode.Tag = levelRow;
                }

                TreeListNode acroNode = treeAcronyms.AppendNode(null, levelNode);
                acroNode.SetValue(colAcronym, acroRow.Description + " (" + acroRow.Name.ToUpper() + ")");
                acroNode.SetValue(colDeleteAcronyms, true);
                acroNode.Tag = acroRow;
            }

            //restore collapse/expanded, focused and first visible node states
            AcronymManager.RestoreNodeStates(treeAcronyms, expandedNodesIDs, focusedNodeID, topVisibleNodeID);

            treeAcronyms.EndUnboundLoad();

            colAcronym.BestFit();
            colDeleteAcronyms.BestFit();
            if (expand)
                treeAcronyms.ExpandAll(); //initially, i.e. if called from btnLoad_Click, expand all nodes
        }

        bool CheckForUnusedVariables(double progressSoFar, double progressToAchieve, BackgroundWorker backgroundWorker)
        {
            if (_variablesToDisplay == null)
                _variablesToDisplay = new List<VarConfig.VariableRow>();
            else
                _variablesToDisplay.Clear();

            List<VarConfig.VariableRow> variableRows = _varConfigFacade.GetVariables();
            for (int i = 0; i < variableRows.Count; ++i)
            {
                if (backgroundWorker.CancellationPending)
                    return false; //user pressed Cancel button

                VarConfig.VariableRow variableRow = variableRows.ElementAt(i);
                if (IsUnusedVariable(variableRow.Name))
                    _variablesToDisplay.Add(variableRow);

                double progressPart = (i + 1.0) / (variableRows.Count) * progressToAchieve;
                backgroundWorker.ReportProgress(Convert.ToInt32((progressSoFar + progressPart) * 100.0));
            }

            //initialise list of variables which are marked to be deleted, to be used in function CheckForUnusedAcronyms
            _variablesToDelete = (from vR in _variablesToDisplay select vR.Name.ToLower()).ToList<string>();

            return true;
        }

        bool IsUnusedVariable(string variableName)
        {
            foreach (string parameterValue in _allCountriesParameters)
                if (EM_Helpers.DoesFormulaContainComponent(parameterValue, variableName)) //use 'whole-word-search', i.e. avoid finding bun_s if bun is searched
                    return false;
            return true;
        }

        void treeAcronyms_CustomNodeCellEdit(object sender, GetCustomNodeCellEditEventArgs e)
        {
            if (e.Column != colDeleteAcronyms)
                return;
            //do not show check box if just "header", i.e. type- or level-node
            if (e.Node.ParentNode != null && e.Node.ParentNode.ParentNode != null)
                e.RepositoryItem = rpiCheckEdit;
        }

        void dgvVariables_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvVariables.IsCurrentCellDirty)
                dgvVariables.CommitEdit(DataGridViewDataErrorContexts.Commit); //this activates the CellValueChanged event when the checkbox is clicked
        }

        void dgvVariables_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            return;
            /*
            if (treeAcronyms.Nodes.Count == 0)
                return; //CellValueChanged-event not activated by a user-change (but by system)

            //adapt list of variables which are marked to be deleted, to be used in function CheckForUnusedAcronyms
            string variableName = dgvVariables.Rows[e.RowIndex].Cells[dgvVariables.Columns.IndexOf(colVariableName)].Value.ToString().ToLower();
            if (EM_Helpers.SaveConvertToBoolean(dgvVariables.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) == true)
                _variablesToDelete.Add(variableName);
            else
                _variablesToDelete.Remove(variableName);

            this.Cursor = Cursors.WaitCursor;
            CheckForUnusedAcronyms();
            DisplayUnusedAcronyms(false);
            this.Cursor = Cursors.Default;
             */
        }

        internal CleanVariablesForm(VarConfigFacade varConfigFacade, VariablesForm variablesForm)
        {
            _varConfigFacade = varConfigFacade;
            _variablesForm = variablesForm;
            InitializeComponent();
        }

        private void ctmCheckOptions_Opening(object sender, CancelEventArgs e)
        {
            _mousePositionWhenMenuOpened = MousePosition;
            hitGrid = GetHitInfo();
            if (hitGrid == 0) 
            {
                e.Cancel = true;
                return;
            }

            mniCheckAll.Text = "Check All " + ((hitGrid == 1) ? "Variables" : "Acronyms");
            mniUncheckAll.Text = "Uncheck All " + ((hitGrid == 1) ? "Variables" : "Acronyms");
        }

        int GetHitInfo()
        {
            Point hit = dgvVariables.PointToClient(_mousePositionWhenMenuOpened);
            Point hit1 = treeAcronyms.PointToClient(_mousePositionWhenMenuOpened);
            if (hit.X > 0 && hit.X < dgvVariables.Width && hit.Y > 0 && hit.Y < dgvVariables.Height) return 1;
            if (hit1.X > 0 && hit1.X < treeAcronyms.Width && hit1.Y > 0 && hit1.Y < treeAcronyms.Height) return 2;
            return 0;
        }

        private void mniCheckAll_Click(object sender, EventArgs e)
        {
            if (hitGrid == 1)
                foreach (DataGridViewRow variableRow in dgvVariables.Rows)
                    variableRow.Cells[colDeleteVariables.Name].Value = true;
            else if (hitGrid == 2)
                foreach (TreeListNode typeNode in treeAcronyms.Nodes)
                    foreach (TreeListNode levelNode in typeNode.Nodes)
                        foreach (TreeListNode acroNode in levelNode.Nodes)
                            acroNode.SetValue(colDeleteAcronyms, true);
            
        }

        private void mniUncheckAll_Click(object sender, EventArgs e)
        {
            if (hitGrid == 1)
                foreach (DataGridViewRow variableRow in dgvVariables.Rows)
                    variableRow.Cells[colDeleteVariables.Name].Value = false;
            else if (hitGrid == 2)
                foreach (TreeListNode typeNode in treeAcronyms.Nodes)
                    foreach (TreeListNode levelNode in typeNode.Nodes)
                        foreach (TreeListNode acroNode in levelNode.Nodes)
                            acroNode.SetValue(colDeleteAcronyms, false);

        }
    }
}
