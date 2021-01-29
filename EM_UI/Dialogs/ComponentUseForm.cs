using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.NodeOperations;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class ComponentUseForm : Form
    {
        EM_UI_MainForm _mainForm = null;
        Dictionary<string, string> _selectedComponents = null;
        List<UsedComponent> _usedComponenets = null;
        List<CountryConfig.SystemRow> _systemRows = new List<CountryConfig.SystemRow>();
        List<string> _selectedSystemIDs = new List<string>();
        const char _separator = '°';

        const string _componentType_Variable = "Variable";
        const string _componentType_Incomelist = "Incomelist";
        const string _componentType_Taxunit = "Assessment Unit";
        const string _componentType_Query = "Query";
        const string _componentType_Unknown = "Unknown";

        void ComponentUseForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void FillSystemsList()
        {
            bool firstFill = !_systemRows.Any(); List<string> selIds = new List<string>();
            try { if (!firstFill) foreach (int index in lstSystems.CheckedIndices) selIds.Add(_systemRows.ElementAt(index).ID); } catch { }

            lstSystems.Items.Clear();
            _systemRows.Clear();
            foreach (TreeListColumn systemColumn in _mainForm.GetTreeListBuilder().GetSystemColums())
            {
                CountryConfig.SystemRow systemRow = (systemColumn.Tag as SystemTreeListTag).GetSystemRow();
                int index = lstSystems.Items.Add(systemRow.Name);
                lstSystems.SetItemChecked(index, firstFill || selIds.Contains(systemRow.ID));
                _systemRows.Add(systemRow);
            }
        }

        void btnRun_Click(object sender, EventArgs e)
        {
            _selectedComponents = GatherComponents();
            if (_selectedComponents.Count == 0 && !IsPatternSearchForSingleComponent())
            {
                UserInfoHandler.ShowInfo("None of the selected components are in use");
                return;
            }

            _selectedSystemIDs.Clear();
            foreach (int index in lstSystems.CheckedIndices)
                _selectedSystemIDs.Add(_systemRows.ElementAt(index).ID);

            if (_usedComponenets == null)
                _usedComponenets = new List<UsedComponent>();
            _usedComponenets.Clear();

            //the handler passed to the progress indicator will do the work (see below)
            ProgressIndicator progressIndicator = new ProgressIndicator(ListUsage_BackgroundEventHandler, "Assessing usage of selected components");
            if (progressIndicator.ShowDialog() == System.Windows.Forms.DialogResult.Cancel)
                return; //user cancelled the procedure

            DoPatternSearchForSingleComponent(); //pattern-search has to be treated differently

            //display the found usages of selected components
            lvComponents.Items.Clear();
            foreach (UsedComponent usedComponent in _usedComponenets)
            {
                string selectedComponent = (usedComponent._tag as string).Split(_separator).ElementAt(0); //the component (e.g. tu_household_sl, ils_dispy, ...)
                string selectedComponentType = (usedComponent._tag as string).Split(_separator).ElementAt(1); //the type of the component (e.g. assessment unit, incomelist, ...)
                ListViewItem listViewItem = lvComponents.Items.Add(selectedComponent);
                listViewItem.SubItems.Add(selectedComponentType);
                listViewItem.SubItems.Add(usedComponent._policyName);       //used in policy
                listViewItem.SubItems.Add(usedComponent._functionName);     //used in function
                listViewItem.SubItems.Add(usedComponent._parameterName);    //used in parameter
                listViewItem.SubItems.Add(usedComponent._row);              //used in system(s)
                string systemNames = string.Empty;
                foreach (string systemName in usedComponent._systemNames)
                    systemNames += systemName + " ";
                listViewItem.SubItems.Add(systemNames);
                listViewItem.Tag = usedComponent._node; //to be able to jumpt to the respective parameter
            }
        }

        void DoPatternSearchForSingleComponent()
        {
            if (!IsPatternSearchForSingleComponent()) return;
            CheckComponentUse componentUseChecker = new CheckComponentUse(txtComponentName.Text, chkIgnoreIfSwitchedOff.Checked, _selectedSystemIDs, true);
            _mainForm.treeList.NodesIterator.DoOperation(componentUseChecker);

            foreach (UsedComponent usedComponent in componentUseChecker.GetUsedComponents())
            {
                usedComponent._tag = txtComponentName.Text + _separator + _componentType_Unknown; //store component and its type for display
                _usedComponenets.Add(usedComponent);
            }
        }

        bool IsPatternSearchForSingleComponent() { return txtComponentName.Text.Contains('*') || txtComponentName.Text.Contains('?'); }

        void ListUsage_BackgroundEventHandler(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button: stop the process and allow progress indicator to set dialog result to Cancel

            for (int i = 0; i < _selectedComponents.Count; ++i) //run over selected components, i.e. all taxunits, all incomelists, ...
            {
                string selectedComponent = _selectedComponents.ElementAt(i).Key; //e.g. tu_household_sl, ils_dispy, ...

                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; } //user pressed Cancel button

                //check wheter component (e.g. tu_household_sl, ils_dispy, ...) is used in any parameters
                CheckComponentUse componentUseChecker = new CheckComponentUse(selectedComponent, chkIgnoreIfSwitchedOff.Checked, _selectedSystemIDs);
                _mainForm.treeList.NodesIterator.DoOperation(componentUseChecker);

                foreach (UsedComponent usedComponent in componentUseChecker.GetUsedComponents())
                {//store usages of component for later display in listview (see btnRun_Click): cannot be added to listview here, because listview does not belong to this thread
                    usedComponent._tag = selectedComponent + _separator + _selectedComponents.ElementAt(i).Value; //store component and its type for display
                    _usedComponenets.Add(usedComponent);
                }

                backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (_selectedComponents.Count * 1.0) * 100.0));
            }
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        void btnGoTo_Click(object sender, EventArgs e)
        {
            if (lvComponents.SelectedItems.Count == 0)
            {
                UserInfoHandler.ShowError("Please select a component in the list.");
                return;
            }

            try
            {
                TreeListNode node = lvComponents.SelectedItems[0].Tag as TreeListNode;
                node.TreeList.FocusedNode = node;

                do
                {
                    node.Expanded = true;
                    node = node.ParentNode;
                } while (node != null);

                Hide();
            }
            catch { UserInfoHandler.ShowError("GoTo failed!"); }
        }

        void chkAllVariables_CheckedChanged(object sender, EventArgs e)
        {
            chkCountrySpecific.Enabled = chkAllVariables.Checked;
            chkSimulated.Enabled = chkAllVariables.Checked;
            chkNonSimulated.Enabled = chkAllVariables.Checked;
            chkMonetary.Enabled = chkAllVariables.Checked;
            chkNonMonetary.Enabled = chkAllVariables.Checked;
        }

        bool IsCheckedVariable(VarConfig.VariableRow variableRow)
        {
            return ((!chkMonetary.Checked || variableRow.Monetary == "1") &&
                (!chkNonMonetary.Checked || variableRow.Monetary != "1") &&
                (!chkSimulated.Checked || variableRow.Name.EndsWith(DefGeneral.POSTFIX_SIMULATED)) &&
                (!chkNonSimulated.Checked || !variableRow.Name.EndsWith(DefGeneral.POSTFIX_SIMULATED)));
        }

        Dictionary<string, string> GatherComponents()
        {
            Dictionary<string, string> selectedComponents = new Dictionary<string, string>();

            if (chkAllVariables.Checked)
            {
                if (chkCountrySpecific.Checked)
                {
                    string countryShortName = _mainForm.GetCountryShortName();
                    foreach (VarConfig.VariableRow variableRow in EM_AppContext.Instance.GetVarConfigFacade().GetVariablesWithCountrySpecificDescription(countryShortName))
                        if (IsCheckedVariable(variableRow))
                            selectedComponents.Add(variableRow.Name, _componentType_Variable);
                }
                else if (chkMonetary.Checked || chkNonMonetary.Checked || chkSimulated.Checked || chkNonSimulated.Checked)
                {
                    foreach (VarConfig.VariableRow variableRow in EM_AppContext.Instance.GetVarConfigFacade().GetVariables())
                        if (IsCheckedVariable(variableRow))
                            selectedComponents.Add(variableRow.Name, _componentType_Variable);
                }
                else
                {
                    foreach (string variableName in EM_AppContext.Instance.GetVarConfigFacade().GetVariables_NamesAndDescriptions().Keys)
                    {
                        if (!selectedComponents.Keys.Contains(variableName.ToLower()))
                            selectedComponents.Add(variableName.ToLower(), _componentType_Variable);
                    }
                }
            }

            if (chkAssessmentUnits.Checked)
            {
                List<CountryConfig.ParameterRow> parameterRowsTUs = null;
                foreach (TreeListColumn systemColumn in _mainForm.GetTreeListBuilder().GetSystemColums())
                {
                    CountryConfig.SystemRow systemRow = (systemColumn.Tag as SystemTreeListTag).GetSystemRow();
                    CountryConfigFacade.GetDefFunctionInformation(systemRow, ref parameterRowsTUs, DefFun.DefTu, DefPar.DefTu.Name);
                    foreach (CountryConfig.ParameterRow parameterRowsTU in parameterRowsTUs)
                        if (!selectedComponents.Keys.Contains(parameterRowsTU.Value.ToLower()))
                            selectedComponents.Add(parameterRowsTU.Value.ToLower(), _componentType_Taxunit);
                }
            }

            if (chkIncomelists.Checked)
            {
                List<CountryConfig.ParameterRow> parameterRowsILs = null;
                foreach (TreeListColumn systemColumn in _mainForm.GetTreeListBuilder().GetSystemColums())
                {
                    CountryConfig.SystemRow systemRow = (systemColumn.Tag as SystemTreeListTag).GetSystemRow();
                    CountryConfigFacade.GetDefFunctionInformation(systemRow, ref parameterRowsILs, DefFun.DefIl, DefPar.DefIl.Name);
                    foreach (CountryConfig.ParameterRow parameterRowsIL in parameterRowsILs)
                        if (!selectedComponents.Keys.Contains(parameterRowsIL.Value.ToLower()))
                            selectedComponents.Add(parameterRowsIL.Value.ToLower(), _componentType_Incomelist);
                }
            }

            if (chkQueries.Checked)
            {
                foreach (string queryName in DefinitionAdmin.GetQueryNamesAndDesc(false).Keys)
                {
                    if (!selectedComponents.Keys.Contains(queryName.ToLower()))
                    {
                        selectedComponents.Add(queryName.ToLower(), _componentType_Query);
                        DefinitionAdmin.GetQueryDefinition(queryName, out DefinitionAdmin.Query queryDef, out string dummy, false);
                        if (queryDef != null)
                            foreach (string queryAlias in queryDef.aliases)
                                selectedComponents.Add(queryAlias.ToLower(), _componentType_Query);
                    }
                }
            }

            if (txtComponentName.Text != string.Empty && !IsPatternSearchForSingleComponent()) //pattern-search has to be treated differently
                if (!selectedComponents.Keys.Contains(txtComponentName.Text.ToLower()))
                    selectedComponents.Add(txtComponentName.Text.ToLower(), _componentType_Unknown);

            return selectedComponents;
        }

        void ComponentUseForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //don't close just hide
            Hide();
        }

        internal ComponentUseForm(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        private void ComponentUseForm_Shown(object sender, EventArgs e)
        {
            FillSystemsList();
        }

        void chkNonSimulated_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNonSimulated.Checked == true)
                chkSimulated.Checked = false;
        }

        void chkSimulated_CheckedChanged(object sender, EventArgs e)
        {
            if (chkSimulated.Checked == true)
                chkNonSimulated.Checked = false;
        }

        void chkNonMonetary_CheckedChanged(object sender, EventArgs e)
        {
            if (chkNonMonetary.Checked == true)
                chkMonetary.Checked = false;
        }

        void chkMonetary_CheckedChanged(object sender, EventArgs e)
        {
            if (chkMonetary.Checked == true)
                chkNonMonetary.Checked = false;
        }

        void btnExport_Click(object sender, EventArgs e)
        {
            if (lvComponents.Items.Count == 0)
            {
                UserInfoHandler.ShowInfo("There are no component-use-search results which can be exported.");
                return;
            }
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = false;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = EM_AppContext.FolderOutput;

            if (openFileDialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            DeveloperInfo.DeveloperInfoTools.ExportListView(lvComponents, Path.GetDirectoryName(openFileDialog.FileName), Path.GetFileName(openFileDialog.FileName));
        }

        void btnAllSystems_Click(object sender, EventArgs e) { for (int i = 0; i < lstSystems.Items.Count; ++i) lstSystems.SetItemChecked(i, true); }
        void btnNoSystem_Click(object sender, EventArgs e) { for (int i = 0; i < lstSystems.Items.Count; ++i) lstSystems.SetItemChecked(i, false); }
    }
}
