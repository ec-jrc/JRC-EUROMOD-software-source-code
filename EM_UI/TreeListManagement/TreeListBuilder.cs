using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.Actions;
using EM_UI.DataSets;
using EM_UI.NodeOperations;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace EM_UI.TreeListManagement
{
    internal class TreeListBuilder
    {
        CountryConfigFacade _countryConfigFacade = null;
        EM_UI_MainForm _mainForm = null;

        static List<string> _fixedColumnsLeft = null;
        static string _fixedColumnRight = string.Empty;

        Dictionary<string, int> _storedColumnWidths = null;
        string _storedColumnFocus = string.Empty;

        bool _singlePolicyView = false;
        internal bool SinglePolicyView { get { return _singlePolicyView; } }
        List<string> _idsDisplayedSinglePolicy = new List<string>();
        List<CountryConfig.PolicyRow> _availablePolicies = new List<CountryConfig.PolicyRow>();

        const int _columnWidthMax = 200;
        const int _columnWidhtMin = 300;
        const int _columnWidthCorrection = 100;
        const int _textSizeChangeLevel = 3;

        TreeListNode BuildFunctionNode(TreeListNode policyNode, CountryConfig.FunctionRow functionRow)
        {
            TreeListNode functionNode = _mainForm.treeList.AppendNode(null, policyNode);
            functionNode.SetValue(_policyColumnName, functionRow.Name);
            functionNode.SetValue(_commentColumnName, functionRow.Comment);
            functionNode.StateImageIndex = (functionRow.Private == DefPar.Value.YES) ?
                DefGeneral.IMAGE_IND_PRIV_FUN : DefGeneral.IMAGE_IND_FUN;
            functionNode.ImageIndex = functionNode.SelectImageIndex = -1; //not used
            functionNode.Tag = new FunctionTreeListTag(_mainForm);
            return functionNode;
        }

        TreeListNode BuildParameterNode(TreeListNode functionNode, CountryConfig.ParameterRow parameterRow)
        {
            TreeListNode parameterNode = _mainForm.treeList.AppendNode(null, functionNode);
            parameterNode.SetValue(_policyColumnName, parameterRow.Name);
            parameterNode.SetValue(_commentColumnName, parameterRow.Comment);
            parameterNode.SetValue(_groupColumnName, parameterRow.Group);
            parameterNode.Tag = new ParameterTreeListTag(_mainForm);
            parameterNode.StateImageIndex = (parameterRow.Private == DefPar.Value.YES) ?
                DefGeneral.IMAGE_IND_PRIV_PAR : -1;
            parameterNode.ImageIndex = parameterNode.SelectImageIndex = -1; //not used
            return parameterNode;
        }

        internal const string _groupColumnName = "Group";
        internal const string _policyColumnName = "Policy";
        internal const string _commentColumnName = "Comment";

        internal TreeListBuilder(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade)
        {
            _mainForm = mainForm;
            _countryConfigFacade = countryConfigFacade;

            _fixedColumnsLeft = new List<string>();
            _fixedColumnsLeft.Add(_policyColumnName);
            _fixedColumnsLeft.Add(_groupColumnName);
            _fixedColumnRight = _commentColumnName;
        }

        internal static bool IsFixedColumn(TreeListColumn column)
        {
            if (column.Name == _fixedColumnRight)
                return true;
            return _fixedColumnsLeft.Contains(column.Name);
        }

        internal static bool IsFixedColumnLeft(TreeListColumn column) { return _fixedColumnsLeft.Contains(column.Name); }

        internal static bool IsFixedColumnRight(TreeListColumn column) { return (column.Name == _fixedColumnRight); }

        internal static bool IsPolicyColumn(TreeListColumn column) { return (column.Name.ToLower() == _policyColumnName.ToLower()); }

        internal static bool IsGroupColumn(TreeListColumn column) { return (column.Name.ToLower() == _groupColumnName.ToLower()); }

        internal static bool IsCommentColumn(TreeListColumn column) { return (column.Name.ToLower() == _commentColumnName.ToLower()); }

        internal static bool IsSystemColumn(TreeListColumn column) { return !IsFixedColumnLeft(column) && !IsFixedColumnRight(column); }

        internal static int GetFixedColumnsLeftCount() { return _fixedColumnsLeft.Count; }

        internal TreeListColumn GetPolicyColumn() { return _mainForm.treeList.Columns.ColumnByName(_policyColumnName); }

        internal TreeListColumn GetGroupColumn() { return _mainForm.treeList.Columns.ColumnByName(_groupColumnName); }

        internal TreeListColumn GetCommentColumn() { return _mainForm.treeList.Columns.ColumnByName(_commentColumnName); }

        internal List<TreeListColumn> GetSystemColums()
        {
            List<TreeListColumn> systemColumns = new List<TreeListColumn>();
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
                if (IsSystemColumn(column))
                    systemColumns.Add(column);
            return systemColumns;
        }

        internal TreeListColumn GetSystemColumnByOrder(int order)
        {
            foreach (TreeListColumn column in GetSystemColums())
                if ((column.Tag as SystemTreeListTag).GetSystemOrder() == order)
                    return column;
            return null;
        }

        internal TreeListColumn GetSystemColumnByID(string ID)
        {
            foreach (TreeListColumn column in GetSystemColums())
                if ((column.Tag as SystemTreeListTag).GetSystemRow().ID == ID)
                    return column;
            return null;
        }

        internal TreeListColumn GetSystemColumnByName(string name)
        {
            foreach (TreeListColumn column in GetSystemColums())
                if (column.Name.ToLower() == name.ToLower())
                    return column;
            return null;
        }

        internal List<string> GetSystemIDs()
        {
            List<string> systemIDs = new List<string>();
            foreach (TreeListColumn column in GetSystemColums())
                systemIDs.Add((column.Tag as SystemTreeListTag).GetSystemRow().ID);
            return systemIDs;
        }

        internal void BuildTreeList()
        {
            //remove all columns but policy- and group-column (i.e. all system columns and the comment column)
            //(the former are defined in the mainform-designer, i.e. even exist if the treelist is drawn for the first time)
            for (int index = _mainForm.treeList.Columns.Count - 1; index >= _fixedColumnsLeft.Count; --index)
                _mainForm.treeList.Columns.RemoveAt(index);

            //build the system columns
            foreach (CountryConfig.SystemRow systemRow in _countryConfigFacade.GetSystemRowsOrdered())
            {
                TreeListColumn sysColumn = _mainForm.treeList.Columns.Add();
                sysColumn.Name = systemRow.Name;
                sysColumn.FieldName = systemRow.Name;
                sysColumn.Caption = systemRow.Name;
                sysColumn.Visible = true;
                sysColumn.Tag = new SystemTreeListTag(systemRow);
                sysColumn.OptionsColumn.AllowSort = false;
            }

            //build the comment-column
            TreeListColumn commentColumn = _mainForm.treeList.Columns.Add();
            commentColumn.Name = _fixedColumnRight;
            commentColumn.FieldName = _fixedColumnRight;
            commentColumn.Caption = _fixedColumnRight;
            commentColumn.Visible = true;
            commentColumn.OptionsColumn.AllowSort = false;
            commentColumn.OptionsColumn.AllowMove = false;
            commentColumn.Fixed = FixedStyle.Right;

            _mainForm.treeList.BackgroundImage = null;
        }

        internal void FillTreeList()
        {
            _availablePolicies.Clear(); //structure to gather the available policies for displaying in the select-policy-menu

            foreach (CountryConfig.PolicyRow distinctPolicyRow in _countryConfigFacade.GetPolicyRowsOrderedAndDistinct())
            {//to generate the policy node an arbitrary policy (i.e. located in any system) is selected, the "correct" policy is assessed in the system loop
                //do not show the policy which contains the definitions of uprating-factors
                if (distinctPolicyRow.Name.StartsWith(EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name))
                    continue;

                _availablePolicies.Add(distinctPolicyRow); //gather the available policies for displaying in the select-policy-menu

                if (_singlePolicyView && _idsDisplayedSinglePolicy != null && !_idsDisplayedSinglePolicy.Contains(distinctPolicyRow.ID))
                    continue; //only display the selected policy in single-policy-view

                InsertPolicyNode(distinctPolicyRow);
            } //end foreach distinctPolicyRow

            CheckPrivatePublicInconsistencies();

            //try to fit in columns in the tree list in a way that allows - if possible - to see all systems without scrolling
            AdaptTreeListColumnWidth();

            //set backcolor and foreColor as defined by the user
            SetSystemFormats();
        }

        internal void InsertPolicyNode(CountryConfig.PolicyRow distinctPolicyRow, int nodeIndex = -1)
        {
            TreeListNode policyNode = _mainForm.treeList.AppendNode(null, null);
            if (nodeIndex != -1)
                _mainForm.treeList.SetNodeIndex(policyNode, nodeIndex);

            //a reference policy is a policy that does not really exist, but marks (in the spine)
            //the re-calculation of a policy that was already calculated further up in the spine
            CountryConfig.PolicyRow referencePolicy = null;

            if (distinctPolicyRow.ReferencePolID != null && distinctPolicyRow.ReferencePolID != string.Empty)
                referencePolicy = _countryConfigFacade.GetPolicyRowByID(distinctPolicyRow.ReferencePolID); //get the "real" policy to assess the necessary information (in essence the name)

            //equip the node with the non system specific information about the policy
            policyNode.SetValue(_policyColumnName, referencePolicy == null ? distinctPolicyRow.Name : referencePolicy.Name);
            policyNode.SetValue(_commentColumnName, distinctPolicyRow.Comment);
            policyNode.Tag = new PolicyTreeListTag(_mainForm); //here create the tag - (system specific) information will be filled in below
            policyNode.StateImageIndex = policyNode.SelectImageIndex = PolicyTreeListTag.GetStateImageIndex(distinctPolicyRow);
            policyNode.ImageIndex = -1; //not used

            bool isFirstSystem = true;
            foreach (CountryConfig.SystemRow systemRow in _countryConfigFacade.GetSystemRowsOrdered())
            {
                //equip the policy node with the system specific information about the policy
                CountryConfig.PolicyRow policyRow = _countryConfigFacade.GetPolicyRowByNameOrderAndSystemID(distinctPolicyRow.Name,
                                                                                            distinctPolicyRow.Order, systemRow.ID);
                (policyNode.Tag as PolicyTreeListTag).AddPolicyRowOfSystem(systemRow.ID, policyRow);
                policyNode.SetValue(systemRow.Name, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(policyRow));

                FillPolicyNodeWithFunctions(systemRow, policyRow, policyNode, isFirstSystem);
                isFirstSystem = false;
            }
        }

        internal void InsertFunctionNode(List<CountryConfig.FunctionRow> functionRowsOfAllSystems, TreeListNode policyNode, int nodeIndex = -1)
        {
            TreeListNode functionNode = null;
            bool isFirstSystem = true;
            foreach (CountryConfig.FunctionRow functionRow in functionRowsOfAllSystems)
            {
                CountryConfig.SystemRow systemRow = functionRow.PolicyRow.SystemRow;
                functionNode = isFirstSystem ? BuildFunctionNode(policyNode, functionRow) : policyNode.Nodes.LastNode;
                (functionNode.Tag as FunctionTreeListTag).AddFunctionRowOfSystem(systemRow.ID, functionRow);
                functionNode.SetValue(systemRow.Name, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(functionRow));

                FillFunctionNodeWithParameters(systemRow, functionRow, functionNode, isFirstSystem);
                isFirstSystem = false;
            }
            if (nodeIndex != -1)
                _mainForm.treeList.SetNodeIndex(functionNode, nodeIndex);
        }

        void FillPolicyNodeWithFunctions(CountryConfig.SystemRow systemRow, CountryConfig.PolicyRow policyRow, TreeListNode policyNode, bool isFirstSystem)
        {
            //generate the functions of the policy
            int functionIndex = 0;
            var functionQuery = (from function in policyRow.GetFunctionRows() select function).OrderBy(function => long.Parse(function.Order));
            foreach (CountryConfig.FunctionRow functionRow in functionQuery.ToList<CountryConfig.FunctionRow>())
            {
                TreeListNode functionNode = isFirstSystem ? BuildFunctionNode(policyNode, functionRow) : policyNode.Nodes[functionIndex++];
                (functionNode.Tag as FunctionTreeListTag).AddFunctionRowOfSystem(systemRow.ID, functionRow);
                functionNode.SetValue(systemRow.Name, ExtensionAndGroupManager.GetExtensionAdaptedSwitch(functionRow));

                //generate the parameters of the function
                FillFunctionNodeWithParameters(systemRow, functionRow, functionNode, isFirstSystem);
            }
        }

        void FillFunctionNodeWithParameters(CountryConfig.SystemRow systemRow, CountryConfig.FunctionRow functionRow, TreeListNode functionNode, bool isFirstSystem)
        {
            //generate the parameters of the function
            int parameterIndex = 0;
            var parameterQuery = (from parameter in functionRow.GetParameterRows() select parameter).OrderBy(parameter => long.Parse(parameter.Order));
            foreach (CountryConfig.ParameterRow parameterRow in parameterQuery.ToList())
            {
                TreeListNode parameterNode = isFirstSystem ? BuildParameterNode(functionNode, parameterRow) : functionNode.Nodes[parameterIndex++];
                (parameterNode.Tag as ParameterTreeListTag).AddParameterRowOfSystem(systemRow.ID, parameterRow);
                parameterNode.SetValue(systemRow.Name, parameterRow.Value);
            }
        }

        //performance optimisation Aug 13: for updating just one function instead of the whole tree
        internal void RefillFunctionNode(TreeListNode functionNode)
        {
            functionNode.Nodes.Clear();

            bool isFirstSystem = true;
            foreach (CountryConfig.SystemRow systemRow in _countryConfigFacade.GetSystemRowsOrdered())
            {
                CountryConfig.FunctionRow functionRow = (functionNode.Tag as FunctionTreeListTag).GetFunctionDictionary()[systemRow.ID];
                FillFunctionNodeWithParameters(systemRow, functionRow, functionNode, isFirstSystem);
                isFirstSystem = false;
            }
        }

        internal void AdaptTreeListColumnWidth()
        {
            int sumWidth = 0;

            //best-fit policy and group-column and assess how much space that takes
            foreach (string columnName in _fixedColumnsLeft)
            {
                TreeListColumn column = _mainForm.treeList.Columns.ColumnByName(columnName);
                column.BestFit();
                sumWidth += column.Width;
            }

            //calculate a column width for system-columns that allows - if possible - to see all systems without scrolling
            int columnWidth = 0;
            if (_mainForm.treeList.Columns.Count > _fixedColumnsLeft.Count + 1)
                columnWidth = (_mainForm.treeList.Width - sumWidth - _columnWidhtMin) / //subtract width of policy- and group-column from treelist-width and reserve minimum width for comment-column
                                (_mainForm.treeList.Columns.Count - (_fixedColumnsLeft.Count + 1)); //divide by number of system-columns

            //assess comment column's best fit (without line breaks)
            TreeListColumn commentColumn = _mainForm.treeList.Columns.ColumnByName(_fixedColumnRight);
            commentColumn.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            commentColumn.BestFit();
            int bestFitCommentColumn = commentColumn.Width;
            commentColumn.AppearanceCell.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;

            foreach (TreeListColumn systemColumn in GetSystemColums())
            {
                systemColumn.Width = System.Math.Min(columnWidth, _columnWidthMax);
                sumWidth += systemColumn.Width;
            }

            //set comment-column-width to the maximum possible without horizontal scrolling or to it's best fit if this is smaller (the latter mainly concerns Simpleland and prevents the comment column from using more space than necessary)
            commentColumn.Width = System.Math.Min(bestFitCommentColumn, _mainForm.treeList.Width - _columnWidthCorrection - sumWidth);
        }

        internal void ExpandAllPolicies()
        {
            _mainForm.treeList.ExpandAll();
        }

        internal void CollapseAllPolicies()
        {
            _mainForm.treeList.CollapseAll();
        }

        //in the first version the node is specified by the "defaultID", which is the ID deliverd by ...TreeListTag.GetDefaultID (i.e. the system is arbitrary)
        //in the second version the specification is done by the IDs of all policies/functions/parameters belonging to the node (i.e. of all systems)
        internal void ExpandSpecificNode(string defaultID) { GetNodeByID getter = new GetNodeByID(defaultID); ExpandSpecificNode(getter); }
        internal void ExpandSpecificNode(List<string> IDs) { GetNodeByID getter = new GetNodeByID(IDs); ExpandSpecificNode(getter); }
        void ExpandSpecificNode(GetNodeByID getter)
        {
            TreatSpecificNodes treater = new TreatSpecificNodes(getter, null, true, true);
            _mainForm.treeList.NodesIterator.DoOperation(treater);
        }

        internal void SetSystemFormats()
        {//set backcolor and foreColor of systems' cells according to the conditional formatting settings defined by the user

            //first clear all current formats
            foreach (TreeListColumn formatColumn in _mainForm._specialFormatCells.Keys)
                _mainForm._specialFormatCells[formatColumn].Clear();
            _mainForm._specialFormatCells.Clear();

            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (IsFixedColumnLeft(column) || IsFixedColumnRight(column))
                    continue; //no conditional formatting for policy-, group- and comment-column

                SystemTreeListTag systemTag = column.Tag as SystemTreeListTag;
                foreach (CountryConfig.ConditionalFormatRow conditionalFormatRow in _countryConfigFacade.GetConditionalFormatRowsOfSystem(systemTag.GetSystemRow()))
                {
                    //first define condition ...
                    IsSpecificBase condition = null;

                    //'standard' conditional formatting: does cell value correspond to specific patterns
                    if (conditionalFormatRow.BaseSystemName == null || conditionalFormatRow.BaseSystemName == string.Empty)
                        condition = new DoesNodeMatchPatterns(conditionalFormatRow.Condition, column);
                    //special conditional formatting: show differences between the system and its base-system
                    else
                    {
                        TreeListColumn columnBaseSystem = _mainForm.treeList.Columns.ColumnByName(conditionalFormatRow.BaseSystemName);
                        if (columnBaseSystem == null)
                            continue; //base system does for whatever reason not exist (was e.g. deleted)
                        condition = new IsNodeValueDifferent(column, columnBaseSystem);
                    }

                    //... then find all cells which match condition ...
                    TreatSpecificNodes treater = new TreatSpecificNodes(condition, null, false);
                    _mainForm.treeList.NodesIterator.DoOperation(treater);

                    //... finally add to list of cells to be formatted: formatting is accomplished in MainForm's treeList_NodeCellStyle-callback (based on the list)
                    Color backColor = conditionalFormatRow.BackColor == ConditionalFormattingHelper._noSpecialColor ? Color.Empty : ConditionalFormattingHelper.GetColorFromDisplayText(conditionalFormatRow.BackColor);
                    Color foreColor = conditionalFormatRow.ForeColor == ConditionalFormattingHelper._noSpecialColor ? Color.Empty : ConditionalFormattingHelper.GetColorFromDisplayText(conditionalFormatRow.ForeColor);

                    AddToSpecialFormatCells(column, treater.GetSpecificNodes(), backColor, foreColor);
                }
            }
        }

        internal void AddToSpecialFormatCells(TreeListColumn column, List<TreeListNode> nodes, Color backColor, Color foreColor)
        {
            if (!_mainForm._specialFormatCells.Keys.Contains(column))
            {
                Dictionary<TreeListNode, KeyValuePair<Color, Color>> nodesList = new Dictionary<TreeListNode, KeyValuePair<Color, Color>>();
                _mainForm._specialFormatCells.Add(column, nodesList);
            }
            foreach (TreeListNode node in nodes)
            {
                if (_mainForm._specialFormatCells[column].Keys.Contains(node))
                {
                    Color newBackColor = _mainForm._specialFormatCells[column][node].Key;
                    Color newForeColor = _mainForm._specialFormatCells[column][node].Value;
                    if (backColor != Color.Empty) //do not overwrite an existing format with "no color"
                        newBackColor = backColor;
                    if (foreColor != Color.Empty)
                        newForeColor = foreColor;
                    _mainForm._specialFormatCells[column][node] = new KeyValuePair<Color, Color>(newBackColor, newForeColor);
                }
                else
                {
                    _mainForm._specialFormatCells[column].Add(node, new KeyValuePair<Color, Color>(backColor, foreColor));
                }
            }
        }

        internal void IncreaseTextSize() { ChangeTextSize(_textSizeChangeLevel); }
        internal void DecreaseTextSize() { ChangeTextSize(_textSizeChangeLevel * (-1)); }
        void ChangeTextSize(float delta)
        {
            Tuple<float, float> currentSize = GetTextSize();
            float newRowSize = currentSize.Item1 + delta;
            if (newRowSize <= 0) return;
            float newHeaderSize = currentSize.Item2 + delta;
            SetTextSize(new Tuple<float, float>(newRowSize, newHeaderSize));
        }

        internal Tuple<float, float> GetTextSize()
        {
            return new Tuple<float, float>(_mainForm.treeList.Appearance.Row.Font.Size, _mainForm.treeList.Appearance.HeaderPanel.Font.Size);
        }

        internal void SetTextSize(Tuple<float, float> newSize)
        {
            Font cellFont = _mainForm.treeList.Appearance.Row.Font;
            _mainForm.treeList.Appearance.Row.Font = new Font(cellFont.Name, newSize.Item1, cellFont.Style, cellFont.Unit);
            Font columnFont = _mainForm.treeList.Appearance.HeaderPanel.Font;
            _mainForm.treeList.Appearance.HeaderPanel.Font = new Font(columnFont.Name, newSize.Item2, columnFont.Style, columnFont.Unit);
        }

        internal void StoreColumnStates()
        {
            if (_storedColumnWidths == null)
                _storedColumnWidths = new Dictionary<string, int>();
            else
                _storedColumnWidths.Clear();
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                _storedColumnWidths.Add(column.Name, column.Width);
                if (column.Equals(_mainForm.treeList.FocusedColumn))
                    _storedColumnFocus = column.Name;
            }
        }

        internal void RestoreColumnStates()
        {
            if (_storedColumnWidths == null)
                return; //should not happen

            int previousColumnWidth = -1;
            bool newlyAddedColumn = false;
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (!_storedColumnWidths.ContainsKey(column.Name)) //new column
                {
                    if (previousColumnWidth != -1)
                        column.Width = previousColumnWidth; //for new columns take the width of the column left to it

                    //newly added column gets the focus
                    _mainForm.treeList.FocusedColumn = column;
                    newlyAddedColumn = true;
                }
                else
                {
                    column.Width = _storedColumnWidths[column.Name];
                    previousColumnWidth = column.Width;
                }

                if (!newlyAddedColumn && column.Name == _storedColumnFocus)
                    _mainForm.treeList.FocusedColumn = column;
            }
        }

        internal void PolicyViewChanged(bool toSinglePolicyView)
        {
            if (toSinglePolicyView == _singlePolicyView)
                return; //nothing to do, just a click on the already selected option

            //"manual" implementation of check-boxes, as that da... ribbon does not offer applicable check-boxes
            _mainForm.chkFullSpine.Glyph = toSinglePolicyView ? Properties.Resources.checkbox : Properties.Resources.checkbox_checked;
            _mainForm.chkSinglePolicy.Glyph = toSinglePolicyView ? Properties.Resources.checkbox_checked : Properties.Resources.checkbox;

            _singlePolicyView = toSinglePolicyView;

            _idsDisplayedSinglePolicy.Clear();
            if (toSinglePolicyView) //change to single policy view with the focused policy as the single one
            {
                TreeListNode node = _mainForm.treeList.FocusedNode;
                while (node != null && node.ParentNode != null)
                    node = node.ParentNode; //get the policy node of focused functions/parameters
                if (node != null && node.Tag != null)
                {
                    PolicyTreeListTag policyTreeListTag = node.Tag as PolicyTreeListTag;
                    if (policyTreeListTag != null) //need to gather policy-ids of all systems as the FillTreeList function has just one of them
                        foreach (CountryConfig.PolicyRow policyRow in policyTreeListTag.GetPolicyRows())
                            _idsDisplayedSinglePolicy.Add(policyRow.ID);
                }
            }

            RedrawTreeForPolicyView(); //update the tree to realise the selected view
        }

        internal void RedrawTreeForPolicyView()
        {
            BaseAction action = new BaseAction();
            action.SetNoCommitAction();
            _mainForm.PerformAction(action); //update tree without performing commit

            if (_mainForm.treeList.Nodes.Count == 1)
            {
                _mainForm.treeList.Nodes[0].ExpandAll(); //expand the single policy in single-policy-view
                _mainForm.treeList.TopVisibleNodeIndex = 0; //to avoid that, due to a previous scrolling position, some middle- or end-part of the policy is shown
            }
        }

        internal void ShowPolicySelectMenu(Point mainFormMousePosition)
        {   //create a selection-list that displays all available policies
            (new SelectPolicyMenu(this, _mainForm)).Show(mainFormMousePosition, _availablePolicies, _idsDisplayedSinglePolicy, _singlePolicyView);
        }

        internal void PolicySelectMenu_ItemSelected(CountryConfig.PolicyRow policyRow)
        {
            try
            {
                if (policyRow == null) //user selected menu item "Full Spine"
                {
                    PolicyViewChanged(false);
                    return;
                }

                CountryConfigFacade countryConfigFacade = CountryAdministration.CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName());
                if (_singlePolicyView) //in single-poliy-view: display the selected policy as "the" policy
                {
                    _idsDisplayedSinglePolicy.Clear();

                    //need to gather policy-ids of all systems as the FillTreeList function has just one of them
                    foreach (string systemID in GetSystemIDs())
                    {
                        System.Data.DataRow twinRow = CountryConfigFacade.GetTwinRow(policyRow, systemID);
                        if (twinRow != null && (twinRow as CountryConfig.PolicyRow) != null)
                            _idsDisplayedSinglePolicy.Add((twinRow as CountryConfig.PolicyRow).ID);
                    }

                    RedrawTreeForPolicyView(); //update the tree to realise the selected view
                }
                else //in full-spine-view: jump to the selected policy
                {
                    foreach (TreeListNode policyNode in _mainForm.treeList.Nodes)
                    {
                        PolicyTreeListTag policyTag = policyNode.Tag as PolicyTreeListTag;
                        //need to check policy-ids of all systems as the FillTreeList function (where the policies for the menu were gathered) provided just one of them
                        foreach (CountryConfig.PolicyRow nodePolicyRow in policyTag.GetPolicyRows())
                            if (nodePolicyRow.ID == policyRow.ID)
                            {
                                _mainForm.treeList.FocusedNode = policyNode;
                                return; //done
                            }
                    }
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.RecordIgnoredException("TreeListBuilder.selectPolicyMenu_ItemClicked", exception);
            }
        }

        internal int GetSystemEvenlyDistributionWidht()
        {
            int distColumnCount = 0;
            int usedSpace = 0;
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (IsFixedColumn(column))
                    usedSpace += column.Width;
                else if (column.Visible == true)
                    ++distColumnCount;
            }
            return distColumnCount == 0 ? 0 : (_mainForm.treeList.Width         //width of treelist
                - _mainForm.treeList.IndicatorWidth                             //minus list of column with row numbers
                - System.Windows.Forms.SystemInformation.VerticalScrollBarWidth //minus width of scrollbar
                - 5                                                             //minus width of rule-of-thumb rounding error zone
                - usedSpace)                                                    //minus width used by policy-, group- and comment-column
                / distColumnCount; //count of visible system columns (which should share the calculated space)
        }

        internal KeyValuePair<TreeListNode, TreeListColumn> GetCellByDataRow(string rowID, string systemID, bool fun)
        {
            foreach (TreeListNode polNode in _mainForm.treeList.Nodes)
            {
                if (IsDataRowNode(polNode, rowID)) return new KeyValuePair<TreeListNode, TreeListColumn>(polNode, GetSystemColumnByID(systemID));
                if (fun)
                    foreach (TreeListNode funNode in polNode.Nodes)
                        if (IsDataRowNode(funNode, rowID)) return new KeyValuePair<TreeListNode, TreeListColumn>(funNode, GetSystemColumnByID(systemID));
            }
            return new KeyValuePair<TreeListNode, TreeListColumn>(null, null);
        }
        private bool IsDataRowNode(TreeListNode node, string rowID)
        {
            BaseTreeListTag tag = node.Tag as BaseTreeListTag;
            return tag != null && tag.GetIDsWithinAllSystems().Contains(rowID);
        }

        internal void AddToAvailablePolicies(CountryConfig.PolicyRow policyRow) { _availablePolicies.Add(policyRow); }
        internal void RemoveFromAvailablePolicies(CountryConfig.PolicyRow policyRow)
        {
            if (_availablePolicies.Contains(policyRow))
                _availablePolicies.Remove(policyRow);
        }

        internal void CheckPrivatePublicInconsistencies()
        {

            List<TreeListNode> allNodes = _mainForm.treeList.GetNodeList();
            List<TreeListNode> inconsistencies = new List<TreeListNode>();
            inconsistencies = new List<TreeListNode>();
            foreach (TreeListNode node in allNodes)
            {
                PolicyTreeListTag polTag = node.Tag as PolicyTreeListTag;
                FunctionTreeListTag funTag = node.Tag as FunctionTreeListTag;
                ParameterTreeListTag parTag = node.Tag as ParameterTreeListTag;

                string firstIsPrivate = String.Empty;

                if (polTag != null)
                {
                    foreach (CountryConfig.PolicyRow policyRow in polTag.GetPolicyRows())
                    {
                        string currentIsPrivate = policyRow.Private;

                        if (String.IsNullOrEmpty(currentIsPrivate)) currentIsPrivate = DefPar.Value.NO;
                        if (String.IsNullOrEmpty(firstIsPrivate)) firstIsPrivate = currentIsPrivate;
                        else
                        { 
                            if (firstIsPrivate != currentIsPrivate && !inconsistencies.Contains(node)) inconsistencies.Add(node);
                        }
                    }
                }

                else if (funTag != null)
                {
                    foreach (CountryConfig.FunctionRow funRow in funTag.GetFunctionRows())
                    {
                        string currentIsPrivate = funRow.Private;

                        if (String.IsNullOrEmpty(currentIsPrivate)) currentIsPrivate = DefPar.Value.NO;
                        if (String.IsNullOrEmpty(firstIsPrivate)) firstIsPrivate = currentIsPrivate;
                        else
                        {
                            if (firstIsPrivate != currentIsPrivate && !inconsistencies.Contains(node)) inconsistencies.Add(node);
                        }
                    }

                }
                else if (parTag != null)
                {
                    foreach (CountryConfig.ParameterRow parRow in parTag.GetParameterRows())
                    {
                        string currentIsPrivate = parRow.Private;

                        if (String.IsNullOrEmpty(currentIsPrivate)) currentIsPrivate = DefPar.Value.NO;
                        if (String.IsNullOrEmpty(firstIsPrivate)) firstIsPrivate = currentIsPrivate;
                        else
                        {
                            if (firstIsPrivate != currentIsPrivate)
                            {
                                if (!inconsistencies.Contains(node) && !inconsistencies.Contains(node)) inconsistencies.Add(node);
                            }
                        }
                    }
                }
            }

            if(inconsistencies != null && inconsistencies.Any())
            {
                foreach (TreeListNode node in inconsistencies)
                {
                    PolicyTreeListTag polTag = node.Tag as PolicyTreeListTag;
                    FunctionTreeListTag funTag = node.Tag as FunctionTreeListTag;
                    ParameterTreeListTag parTag = node.Tag as ParameterTreeListTag;


                    if (parTag != null)
                    {
                        node.StateImageIndex = DefGeneral.IMAGE_IND_INCONS;

                        //Function and Policy also needs to be signaled as inconsistent
                        if (node.ParentNode != null) node.ParentNode.StateImageIndex = node.ParentNode.StateImageIndex == DefGeneral.IMAGE_IND_FUN ? DefGeneral.IMAGE_IND_FUN_INCONS : DefGeneral.IMAGE_IND_PRIV_FUN_INCONS;
                        if (node.ParentNode.ParentNode != null) node.ParentNode.ParentNode.StateImageIndex = node.ParentNode.ParentNode.StateImageIndex == DefGeneral.IMAGE_IND_POL ? DefGeneral.IMAGE_IND_POL_INCONS : DefGeneral.IMAGE_IND_PRIV_POL_INCONS;

                    }
                    else if (funTag != null)
                    {
                        node.StateImageIndex = DefGeneral.IMAGE_IND_INCONS;

                        //Policy also needs to be signaled as inconsistent
                        if (node.ParentNode != null) node.ParentNode.StateImageIndex = node.ParentNode.StateImageIndex == DefGeneral.IMAGE_IND_POL ? DefGeneral.IMAGE_IND_POL_INCONS : DefGeneral.IMAGE_IND_PRIV_POL_INCONS;
                    }
                    else if (polTag != null)
                    {
                        node.StateImageIndex = DefGeneral.IMAGE_IND_INCONS;

                    }
                    
                }
            }

        }
    }

}
