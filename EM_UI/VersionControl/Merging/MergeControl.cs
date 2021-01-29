using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.Utils.Drawing;
using System.Reflection;

namespace EM_UI.VersionControl.Merging
{

    internal partial class MergeControl : UserControl
    {
        internal enum ChangeType { none = -1, added = 0, removed = 1, changed = 2 };
            static internal ChangeType ChangeTypeRestore(string s) { switch (s) { case "added": return ChangeType.added; case "removed": return ChangeType.removed; case "changed": return ChangeType.changed; default: return ChangeType.none; } }
        internal enum ChangeHandling { none = -1, accept = 0, reject = 1, mixed = 2 };
            static internal ChangeHandling ChangeHandlingFromString(string s) { switch (s) { case "accept": return ChangeHandling.accept; case "reject": return ChangeHandling.reject; case "mixed": return ChangeHandling.mixed; default: return ChangeHandling.none; } }
        internal enum CellType { info = 1, value = 2, setting = 3 };
        static internal CellType CellTypeFromString(string s) { switch (s) { case "info": return CellType.info; case "value": return CellType.value; default: return CellType.setting; } }

        internal class NodeInfo //information on a component (e.g. parameter) which is displayed as a node of the trees
        {
            internal NodeInfo(string ID, string parentID = "", List<CellInfo> cellInfo = null, ChangeType changeType = ChangeType.none, ChangeHandling changeHandling = ChangeHandling.none)
            {
                this.ID = ID;
                this.parentID = parentID;
                if (cellInfo != null) this.cellInfo = cellInfo;
                this.changeType = changeType;
                this.changeHandling = changeHandling;
            }

            internal string Store()
            {
                string content = ID + SEPARATOR + parentID + SEPARATOR + changeType.ToString() + SEPARATOR + changeHandling.ToString() + SEPARATOR + nodeSequenceInfo + SEPARATOR;
                content += subNodesSequenceInfo != null ? subNodesSequenceInfo.Store() + SEPARATOR : SubNodesSequenceInfo.NO_SEQUENCE_INFO + SEPARATOR;               
                foreach (CellInfo info in cellInfo)
                    if (info.columnID != string.Empty)
                        content += info.Store() + SEPARATOR;
                return content;
            }
            
            internal void Restore(string content)
            {
                string[] elements = content.Split(new string[] { SEPARATOR }, StringSplitOptions.None);
                if (elements.Count() < 6)
                    return;
                ID = elements[0];
                parentID = elements[1];
                changeType = ChangeTypeRestore(elements[2]);
                changeHandling = ChangeHandlingFromString(elements[3]);
                nodeSequenceInfo = elements[4];
                if (elements[5] != SubNodesSequenceInfo.NO_SEQUENCE_INFO) { subNodesSequenceInfo = new SubNodesSequenceInfo(); subNodesSequenceInfo.Restore(elements[5]); }
                for (int index = 6; index < elements.Count(); ++index)
                {
                    CellInfo info = new CellInfo(string.Empty);
                    info.Restore(elements[index]);
                    cellInfo.Add(info);
                }
            }

            internal string ID = string.Empty; //e.g. parameter-id (in first or default system)
            internal string parentID = string.Empty; //e.g. id of the function containing the parameter
            internal string nodeSequenceInfo = string.Empty;
            internal SubNodesSequenceInfo subNodesSequenceInfo = null; //e.g. info on how the functions of a policy or parameters of a functions are ordered
            internal List<CellInfo> cellInfo = new List<CellInfo>(); //e.g. info on parameter values in each system, plus other information (e.g. private)
            internal ChangeType changeType = ChangeType.none; //e.g. parameter was added (removed, changed)
            internal ChangeHandling changeHandling = ChangeHandling.none; //does the user want to accept or reject a possible change
            internal TreeListNode node = null;
            const string SEPARATOR = "<SEP_NOD>";
        }

        internal class SubNodesSequenceInfo //information on the order of a component's sub-components, e.g. policy-order, function-order, parameter-order
        {
            internal string Store() { return isChanged.ToString() + SEPARATOR + acceptChange.ToString(); }
 
            internal void Restore(string content)
            {
                string[] elements = content.Split(new string[] { SEPARATOR }, StringSplitOptions.None);
                if (elements.Count() != 2)
                    return;
                isChanged = elements[0].ToLower() == "true";
                acceptChange = elements[1].ToLower() == "true";
            }

            internal bool isChanged = false; //is the order changed
            internal bool acceptChange = true; //does the user want to accept a possible change
            const string SEPARATOR = "<SEP_SEQ>";
            internal const string NO_SEQUENCE_INFO = "---";
        }

        internal class CellInfo //information on the cells (column-values) of a component
        {
            internal CellInfo(string columnID, string text = "", string ID = "", bool isChanged = false, bool acceptChange = true)
            {
                this.ID = ID;
                this.columnID = columnID;
                SetText(text);
                this.isChanged = isChanged;
                this.acceptChange = acceptChange;
            }

            internal string Store() { return ID + SEPARATOR + columnID + SEPARATOR + text + SEPARATOR + isChanged.ToString() + SEPARATOR + isConflicted.ToString() + SEPARATOR + acceptChange.ToString(); }

            internal void Restore(string content)
            {
                string[] elements = content.Split(new string[] { SEPARATOR }, StringSplitOptions.None);
                if (elements.Count() != 6)
                    return;
                ID = elements[0];
                columnID = elements[1];
                text = elements[2];
                isChanged = elements[3].ToLower() == "true";
                isConflicted = elements[4].ToLower() == "true";
                acceptChange = elements[5].ToLower() == "true";
            }

            internal string ID = string.Empty; //e.g. id of the parameter in this special system; null in case of other information (e.g. private)
            internal string columnID = string.Empty; //id of column where to display the value (e.g. system-id or id of name-column)
            internal string text { get; private set; } //e.g value of the parameter in the respective system; or name of parameter; or is parameter private
            internal void SetText(string text) { this.text = text == null ? string.Empty : text; }
            internal bool isChanged = false; //is this special value changed
            internal bool isConflicted = false; //is the value changed in both, local and remote
            internal bool acceptChange = true; //does the user want to accept a possible change
            internal TreeListNode node = null;
            internal TreeListColumn column = null;
            const string SEPARATOR = "<SEP_CELL>";
        }

        internal class ColumnInfo
        {
            internal ColumnInfo(string name, string ID = "", CellType cellType = CellType.setting)
            {
                this.ID = (ID == string.Empty) ? name : ID;
                this.name = name;
                this.cellType = cellType;
            }

            internal string Store() { return ID + SEPARATOR + name + SEPARATOR + cellType.ToString(); }

            internal void Restore(string content)
            {
                string[] elements = content.Split(new string[] { SEPARATOR }, StringSplitOptions.None);
                if (elements.Count() != 3)
                    return;
                ID = elements[0];
                name = elements[1];
                cellType = CellTypeFromString(elements[2]);
            }

            internal string ID = string.Empty; //column-identifier, e.g. system-id or (invented) id of name-column
            internal string name = string.Empty; //e.g. system-name or 'Name' for name-column
            internal CellType cellType = CellType.setting; //are the cells of the column values of components (e.g. parameter-values) or sth special (name of parameter, is parameter private)
            internal TreeListColumn column = null;
            const string SEPARATOR = "<SEP_COL>";
        }

        internal class UpratingYearsComparison
        {
            internal string valueLocal = string.Empty;
            internal string valueRemote = string.Empty;
            internal bool isChangedLocal = false;
            internal bool isChangedRemote = false;
            internal bool acceptChangeLocal = false;
            internal bool acceptChangeRemote = false;
            internal ChangeType changeTypeLocal;
            internal ChangeType changeTypeRemote;
            internal ChangeHandling changeHandlingLocal;
            internal ChangeHandling changeHandlingRemote;

        }

        Color COLOUR_ACCEPT = Color.PaleGreen;
        Color COLOUR_REJECT = Color.Pink;
        Color COLOUR_CELLCHANGED = Color.Blue;
        Color COLOUR_CONFLICTED = Color.DarkMagenta;

        List<ColumnInfo> _columInfo = new List<ColumnInfo>(); //columns are identically for both trees
        List<NodeInfo> _nodeInfoLocal = null; //what the left tree needs to display: user interface's version
        List<NodeInfo> _nodeInfoRemote = null; //what the right tree needs to display: version to be merged
        List<string> _levelInfo = null; //e.g. 'Policy', 'Function', 'Parameter' (also identically for both trees)
        internal SubNodesSequenceInfo _sequenceInfo = null; //e.g. policy-order (only visualised in left tree)
        internal bool _needUpdate = true; //_nodeInfoLoca, _nodeInfoRemote and _nodeInfo need to be modified to be back to the YearValues parameter (only once)

        short _isInUnboundLoad = 0;

        internal MergeControl()
        {
            InitializeComponent();

            //provides that trees have the same width, irrelevant how large the control is drawn in the dialog
            splitContainer.SplitterDistance = splitContainer.Width / 2;

            //for images which show whether a component was added/removed/changed and whether the user accepts/rejects the changes
            BuildNodeImageLists(treeLocal);
            BuildNodeImageLists(treeRemote);
        }

        internal void SetInfo(List<ColumnInfo> columnInfo, List<NodeInfo> nodeInfoLocal, List<NodeInfo> nodeInfoRemote, bool provideSequenceInfo)
        {
            _columInfo = columnInfo;
            _nodeInfoLocal = nodeInfoLocal;
            _nodeInfoRemote = nodeInfoRemote;
            if (provideSequenceInfo)
                BuildSequenceInfo();
        }

        internal bool IsEmpty() { return treeLocal.VisibleNodesCount == 0; }

        void BuildSequenceInfo()
        {
            _sequenceInfo = new SubNodesSequenceInfo();
            
            //transform original order into subsequent order (e.g. 5-3-8-6 instead of 2-1-4-3) to have the same order in local and remote
            SerialiseSequence(_nodeInfoLocal);
            SerialiseSequence(_nodeInfoRemote);

            //search for parents with reordered children
            List<ChangeType> relevantChangeTypes = new List<ChangeType>() { ChangeType.none, ChangeType.changed }; //i.e. exclude added and removed
            List<string> parentIDsOfReordered = new List<string>();
            foreach (NodeInfo nodeInfoLocal in _nodeInfoLocal)
            {
                NodeInfo nodeInfoRemote = GetNodeInfoByID(nodeInfoLocal.ID, false);
                if (!relevantChangeTypes.Contains(nodeInfoLocal.changeType) || !relevantChangeTypes.Contains(nodeInfoRemote.changeType) ||
                    nodeInfoLocal.nodeSequenceInfo == string.Empty || nodeInfoRemote.nodeSequenceInfo == string.Empty)
                    continue;
                if (nodeInfoLocal.nodeSequenceInfo != nodeInfoRemote.nodeSequenceInfo)
                    if (!parentIDsOfReordered.Contains(nodeInfoLocal.parentID))
                        parentIDsOfReordered.Add(nodeInfoLocal.parentID);
            }

            //mark parent with reordered children by generating the subNodesSequenceInfo and updating changeType
            foreach (string parentOfReordered in parentIDsOfReordered)
            {
                if (parentOfReordered == string.Empty) //policies reordered (visualised in top-left-corner-button)
                    _sequenceInfo.isChanged = _sequenceInfo.acceptChange = true;
                else //functions or parameters reordered (visualised in row-button of respective policy/function)
                {
                    NodeInfo parentInfo = GetNodeInfoByID(parentOfReordered, true);
                    parentInfo.subNodesSequenceInfo = new SubNodesSequenceInfo();
                    parentInfo.subNodesSequenceInfo.isChanged = true;
                    parentInfo.changeType = ChangeType.changed;
                    parentInfo.changeHandling = ChangeHandling.accept;
                }

                //equip each child-element (e.g. function) of a (partly) reordered parent-elemet (e.g. policy) with order-info (to be visualised in local tree's order-column)
                foreach (NodeInfo nodeInfoLocal in GetNodeInfoChildren(parentOfReordered, _nodeInfoLocal))
                {
                    NodeInfo nodeInfoRemote = GetNodeInfoByID(nodeInfoLocal.ID, false);
                    if (nodeInfoLocal.nodeSequenceInfo != nodeInfoRemote.nodeSequenceInfo) //local and remote order different
                        nodeInfoLocal.nodeSequenceInfo = "L:" + nodeInfoLocal.nodeSequenceInfo + "/R:" + nodeInfoRemote.nodeSequenceInfo;
                    else //local and remote order equal
                        nodeInfoLocal.nodeSequenceInfo = "LR:" + nodeInfoLocal.nodeSequenceInfo;
                }
            }

            //delete all the unnecessary information in nodeSequenceInfo, i.e. of children of not reordered parents (to result in an empty cell in order-column)
            foreach (NodeInfo nodeInfoLocal in _nodeInfoLocal)
            {
                if (!parentIDsOfReordered.Contains(nodeInfoLocal.parentID))
                    nodeInfoLocal.nodeSequenceInfo = string.Empty;
            }
        }

        void SerialiseSequence(List<NodeInfo> listNodeInfo)
        {
            List<string> potParents = new List<string>() { string.Empty };
            foreach (NodeInfo nodeInfo in listNodeInfo)
                potParents.Add(nodeInfo.ID);

            foreach (string potParent in potParents)
            {
                SortedList<int, NodeInfo> sortedSiblings = new SortedList<int, NodeInfo>();
                List<ChangeType> relevantChangeTypes = new List<ChangeType>() { ChangeType.none, ChangeType.changed };
                foreach (NodeInfo siblingInfo in GetNodeInfoChildren(potParent, listNodeInfo))
                {
                    NodeInfo twinInfo = GetNodeInfoByID(siblingInfo.ID, listNodeInfo != _nodeInfoLocal);
                    if (relevantChangeTypes.Contains(siblingInfo.changeType) && siblingInfo.nodeSequenceInfo != string.Empty &&
                        relevantChangeTypes.Contains(twinInfo.changeType) && twinInfo.nodeSequenceInfo != string.Empty)
                        sortedSiblings.Add(Convert.ToInt32(siblingInfo.nodeSequenceInfo), siblingInfo);
                }
                foreach (int key in sortedSiblings.Keys)
                    sortedSiblings[key].nodeSequenceInfo = sortedSiblings.IndexOfKey(key).ToString();
            }
        }

        List<NodeInfo> GetNodeInfoChildren(string parentID, List<NodeInfo> listNodeInfo)
        {
            List<NodeInfo> nodeInfoChildren = new List<NodeInfo>();
            foreach (NodeInfo nodeInfo in listNodeInfo)
                if (nodeInfo.parentID == parentID)
                    nodeInfoChildren.Add(nodeInfo);
            return nodeInfoChildren;
        }

        internal void LoadInfo(string treeCaption = "", List<string> levelInfo = null, string localPath = "", string remotePath = "")
        {
            _levelInfo = levelInfo;

            if (treeCaption != string.Empty)
            {
                labLocal.Text = treeCaption + " - LOCAL " + (!String.IsNullOrEmpty(localPath) ? " - " + localPath.ToLower() : "");
                labRemote.Text = treeCaption + " - REMOTE " + (!String.IsNullOrEmpty(remotePath) ? " - " + remotePath.ToLower() : "");
            }

            BeginUnboundLoad();

            BuildColumns(treeLocal, _sequenceInfo != null);
            FillTree(treeLocal, _nodeInfoLocal);

            BuildColumns(treeRemote);
            FillTree(treeRemote, _nodeInfoRemote);

            EndUnboundLoad();

            FillFlexibleFilterControls(); //value-filter (e.g. systems), special-column-filter (name, comment, private), level-filter

            ApplyFilter();
        }

        void BeginUnboundLoad()
        {
            ++_isInUnboundLoad;
            if (_isInUnboundLoad > 1)
                return;
            treeLocal.BeginUnboundLoad();
            treeRemote.BeginUnboundLoad();
        }

        void EndUnboundLoad()
        {
            --_isInUnboundLoad;
            if (_isInUnboundLoad > 0)
                return;
            treeLocal.EndUnboundLoad();
            treeRemote.EndUnboundLoad();
        }

        void ApplyFilter()
        {
            BeginUnboundLoad();

            //hide/unhide columns
            foreach (ColumnInfo columnInfo in _columInfo)
            {
                bool visible = false;

                

                TreeListColumn column = GetColumn(columnInfo.ID, treeLocal);
                if (columnInfo != null)
                {
                    switch (columnInfo.cellType)
                    {
                        case CellType.setting:
                            if (chkAllSettings.Checked)
                                visible = true;
                            else
                                if (lstSettings.CheckedItems.Contains(column.Name))
                                    visible = true;
                            break;
                        case CellType.value:
                            if (chkAllValues.Checked)
                                visible = true;
                            else
                                if (lstValues.CheckedItems.Contains(column.Name))
                                    visible = true;
                            break;
                        default: visible = true; break;
                    }

                }
                column.Visible = GetTwinColumn(treeLocal, column).Visible = visible;
            }

            //hide/unhide nodes
            List<ChangeType> changeTypes = new List<ChangeType>();
            if (chkAdded.Checked) changeTypes.Add(ChangeType.added);
            if (chkRemoved.Checked) changeTypes.Add(ChangeType.removed);
            if (chkChanged.Checked) changeTypes.Add(ChangeType.changed);

            List<TreeListNode> visibleNodes = new List<TreeListNode>();
            foreach (TreeListNode nodeLocal in GetSubNodes(false, treeLocal))
            {
                TreeListNode nodeRemote = GetTwinNode(nodeLocal);
                nodeLocal.Visible = nodeRemote.Visible = false;

                NodeInfo nodeInfoLocal = GetNodeInfo(nodeLocal);
                NodeInfo nodeInfoRemote = GetNodeInfo(nodeRemote);
                if (nodeInfoLocal == null || nodeInfoRemote == null)
                    continue; //should not happen

                //apply level restrictions
                if (_levelInfo != null && nodeLocal.Level > cmbLevel.SelectedIndex) //if level is e.g. function - don't show any parameters
                    continue;

                //apply local/remote restrictions
                if (radLocalOnly.Checked)
                    nodeInfoRemote = nodeInfoLocal;
                else if (radRemoteOnly.Checked)
                    nodeInfoLocal = nodeInfoRemote; //thus checking for the same twice, which saves code and is probably no performace issue
                
                //apply type-restrictions
                if (!changeTypes.Contains(nodeInfoLocal.changeType) && !changeTypes.Contains(nodeInfoRemote.changeType))
                    continue;

                //apply column-restrictions
                if (!IsNodeVisiblyChanged(nodeInfoLocal, treeLocal) && !IsNodeVisiblyChanged(nodeInfoRemote, treeRemote))
                    continue;
                
                nodeLocal.Visible = nodeRemote.Visible = true;

                visibleNodes.Add(nodeLocal);
            }

            //reshow parent-nodes
            foreach (TreeListNode node in visibleNodes)
                ShowAncestors(node);

            EndUnboundLoad();

            treeLocal.MoveFirst();
            treeRemote.MoveFirst();

            btnReleaseFilter.Enabled = true;
        }

        bool IsNodeVisiblyChanged(NodeInfo nodeInfo, TreeList tree)
        {
            if (nodeInfo.changeType == ChangeType.none)
                return false; //no criterium to be visible (happens if local is changed and remote not or vice-versa)
            if (nodeInfo.changeType != ChangeType.changed)
                return true; //added or removed, i.e. irrelevant what columns are displayed
            if (chkReordered.Checked && nodeInfo.subNodesSequenceInfo != null && nodeInfo.subNodesSequenceInfo.isChanged)
                return true; //sequence of sub-nodes changed
            foreach (CellInfo cellInfo in nodeInfo.cellInfo)
            {
                if (cellInfo.isChanged && GetColumn(cellInfo.columnID, tree).Visible)
                    return true; //node is visible if at least one cell (value or setting) is changed, however the value must be visible (column not hidden)
            }
            return false;
        }

        void ShowAncestors(TreeListNode node)
        {
            if (node.ParentNode == null)
                return;
            node.ParentNode.Visible = GetTwinNode(node.ParentNode).Visible = true;
            ShowAncestors(node.ParentNode);
        }

        void ReleaseFilter()
        {
            BeginUnboundLoad();

            foreach (TreeListColumn column in treeLocal.Columns)
                if (column.Name != SEQUENCE_COLUMN_NAME)
                    column.Visible = GetTwinColumn(treeLocal, column).Visible = true;

            foreach (TreeListNode node in GetSubNodes(false, treeLocal))
                node.Visible = GetTwinNode(node).Visible = true;

            btnReleaseFilter.Enabled = false;

            EndUnboundLoad();
        }

        void ResizeAcceptButton(Button button, int width, int height, int top)
        {
            button.Size = new Size(width, height);
            button.Top = top;
        }

        void FillFlexibleFilterControls()
        {
            lstValues.Items.Clear();
            lstSettings.Items.Clear();

            //fill the two checked lists containing column-headers (split into settings (e.g. name, comment, private, ...) and values (e.g. Systems))
            foreach (ColumnInfo columnInfo in _columInfo)
            {
                if (columnInfo.cellType == CellType.value) //e.g. is a system-column
                    lstValues.Items.Add(columnInfo.name, true);
                else if (columnInfo.cellType == CellType.setting) //e.g. is a special (editable) column, like name, comment, private
                    lstSettings.Items.Add(columnInfo.name, true);
            }

            //fill combo for (e.g.) 'Policy', 'Function', 'Parameter'
            if (_levelInfo != null)
            {
                foreach (string level in _levelInfo)
                    cmbLevel.Items.Add(level);
                if (cmbLevel.Items.Count > 0)
                    cmbLevel.SelectedIndex = cmbLevel.Items.Count - 1;
            }

            //hide lists and related controls if there are no entries
            lstSettings.Visible = chkAllSettings.Visible = lstSettings.Items.Count > 0;
            lstValues.Visible = chkAllValues.Visible = lstValues.Items.Count > 0;
            cmbLevel.Visible = labLevel.Visible = cmbLevel.Items.Count > 0;

            //show reordered-checkbox and show/hide-order-button if order needs to be taken into account
            chkReordered.Visible = chkReordered.Checked = _sequenceInfo != null;
            btnShowOrder.Visible = _sequenceInfo != null;
        }

        TreeListNode GetParentNode(string parentID, TreeListNodes potentialParentNodes)
        {
            foreach (TreeListNode node in potentialParentNodes)
            {
                if (GetNodeInfo(node).ID == parentID)
                    return node;
                if (node.HasChildren)
                {
                    TreeListNode potParent = GetParentNode(parentID, node.Nodes);
                    if (potParent != null)
                        return potParent;
                }
            }
            return null;
        }

        TreeListColumn GetColumn(string ID, TreeList tree)
        {
            foreach (TreeListColumn column in tree.Columns)
                if (GetColumnInfo(column).ID == ID)
                    return column;
            return null;
        }

        void BuildNodeImageLists(TreeList tree)
        {
            ImageList _imageList = new ImageList();
            _imageList.Images.Add(EM_UI.Properties.Resources.add_20);
            _imageList.Images.Add(EM_UI.Properties.Resources.ChangeTypeRemoved);
            _imageList.Images.Add(EM_UI.Properties.Resources.ChangeTypeChanged);
            tree.SelectImageList = _imageList;

            _imageList = new ImageList();
            _imageList.Images.Add(EM_UI.Properties.Resources.merge_accept);
            _imageList.Images.Add(EM_UI.Properties.Resources.merge_reject);
            _imageList.Images.Add(EM_UI.Properties.Resources.merge_mixed);
            tree.StateImageList = _imageList;
        }

        void BuildColumns(TreeList tree, bool addSequenceColumn = false)
        {
            List<ColumnInfo> ci = new List<ColumnInfo>();
            ci.AddRange(_columInfo);
            if (addSequenceColumn) ci.Add(new ColumnInfo(SEQUENCE_COLUMN_NAME, SEQUENCE_COLUMN_ID, CellType.info));
            foreach (ColumnInfo columnInfo in ci)
            {
                columnInfo.column = tree.Columns.Add(); //handle columnInfo.column with care (see GetColumnInfo)!!!
                columnInfo.column.Name = columnInfo.name;
                columnInfo.column.Caption = columnInfo.name;
                columnInfo.column.FieldName = columnInfo.name;
                columnInfo.column.Visible = columnInfo.ID != SEQUENCE_COLUMN_ID;
                columnInfo.column.OptionsColumn.AllowSort = false;
                columnInfo.column.OptionsColumn.AllowSize = true;
                columnInfo.column.Tag = columnInfo;
                columnInfo.column.MinWidth = 60;

            }
        }



        void FillTree(TreeList tree, List<NodeInfo> nodeInfos)
        {
            foreach (NodeInfo nodeInfo in nodeInfos)
            {
                nodeInfo.node = tree.AppendNode(null, GetParentNode(nodeInfo.parentID, tree.Nodes));
                nodeInfo.node.Tag = nodeInfo;

                foreach (CellInfo cellInfo in nodeInfo.cellInfo)
                {
                    cellInfo.column = GetColumn(cellInfo.columnID, tree);
                    cellInfo.node = nodeInfo.node;
                    nodeInfo.node.SetValue(cellInfo.column, cellInfo.text);
                }

                nodeInfo.node.SetValue(GetColumn(SEQUENCE_COLUMN_ID, tree), nodeInfo.nodeSequenceInfo);

                //state-image shows whether a change is accepted or declined; this image is not changed automatically by the tree-control
                nodeInfo.node.StateImageIndex = Convert.ToInt32(nodeInfo.changeHandling);
                //standard-image shows what the change is about (added, removed, changed)
                //this image is changed by the tree-control when the node is selected, namely to the select-image
                //to avoid this unwanted behaviour, always set ImageIndex = SelectImageIndex
                nodeInfo.node.ImageIndex = nodeInfo.node.SelectImageIndex = Convert.ToInt32(nodeInfo.changeType);
            }
            tree.OptionsCustomization.AllowColumnMoving = false;
            setBestColumnWidths(tree);
        }

        void treeLocal_NodeCellStyle(object sender, GetCustomNodeCellStyleEventArgs e)
        {
            NodeInfo nodeInfo = GetNodeInfo(e.Node);
            ColumnInfo columnInfo = GetColumnInfo(e.Column);

            if (nodeInfo == null || columnInfo == null || nodeInfo.changeType == ChangeType.none)
                return;

            foreach (CellInfo cellInfo in nodeInfo.cellInfo)
            {
                if (cellInfo.columnID == columnInfo.ID && cellInfo.isChanged)
                {
                    e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
                    e.Appearance.ForeColor = cellInfo.isConflicted ? COLOUR_CONFLICTED : COLOUR_CELLCHANGED;
                    e.Appearance.BackColor = !cellInfo.acceptChange ? COLOUR_REJECT : COLOUR_ACCEPT;
                }
            }
        }

        TreeList GetTwinTree(object senderTree)
        {
            return (senderTree as TreeList) == treeLocal ? treeRemote : treeLocal;
        }

        TreeListNode GetTwinNode(TreeListNode node)
        {
            NodeInfo nodeInfo = GetNodeInfo(node);
            return GetTwinTree(node.TreeList).FindNode((twinNode) =>
            {
                NodeInfo twinNodeInfo = GetNodeInfo(twinNode);
                return twinNodeInfo.ID == nodeInfo.ID;
            });
        }

        TreeListColumn GetTwinColumn(TreeList tree, TreeListColumn column)
        {
            ColumnInfo columnInfo = GetColumnInfo(column);
            if (columnInfo == null) return null;
            TreeList twinTree = GetTwinTree(tree);
            foreach (TreeListColumn twinColumn in twinTree.Columns)
            {
                ColumnInfo twinInfo = GetColumnInfo(twinColumn);
                if (twinInfo == null) continue;
                if (twinInfo.ID == columnInfo.ID)
                    return twinColumn;
            }
            return null;
        }

        internal NodeInfo GetNodeInfoByID(string ID, bool local)
        {
            List<NodeInfo> twinList = local ? _nodeInfoLocal : _nodeInfoRemote;
            foreach (NodeInfo twinNodeInfo in twinList)
                if (twinNodeInfo.ID == ID)
                    return twinNodeInfo;
            return null;
        }

        internal CellInfo GetTwinCellInfo(CellInfo cellInfo)
        {
            TreeListNode twinNode = GetTwinNode(cellInfo.node);
            foreach (CellInfo twinCellInfo in GetNodeInfo(twinNode).cellInfo)
            {
                if (twinCellInfo.columnID == cellInfo.columnID)
                    return twinCellInfo;
            }
            return null;
        }

        void tree_AfterCollapseExpand(object sender, NodeEventArgs e)
        {
            GetTwinNode(e.Node).Expanded = e.Node.Expanded;
        }

        void tree_TopVisibleNodeIndexChanged(object sender, EventArgs e)
        {
            GetTwinTree(sender).TopVisibleNodeIndex = (sender as TreeList).TopVisibleNodeIndex;
        }

        NodeInfo GetNodeInfo(TreeListNode node) { return (node == null || node.Tag == null) ? null : node.Tag as NodeInfo; }
        ColumnInfo GetColumnInfo(TreeListColumn column)
        {
            if (column == null || column.Tag == null)
                return null;
            ColumnInfo columnInfo = column.Tag as ColumnInfo;
            columnInfo.column = column; //found no other way to distinguish between local-tree-column and remote-tree-column
            return columnInfo;          //as even having two separate _columnInfo (_columnInfoLocal/_columnInfoRemote) always delivered the remote tree
        }
        CellInfo GetCellInfo(NodeInfo nodeInfo, ColumnInfo columnInfo)
        {
            if (nodeInfo == null || columnInfo == null)
                return null;
            foreach (CellInfo cellInfo in nodeInfo.cellInfo)
                if (cellInfo.columnID == columnInfo.ID)
                    return cellInfo;
            return null;
        }

        string GetNodeReference(NodeInfo nodeInfo)
        {
            if (nodeInfo != null && nodeInfo.cellInfo.Count > 0)
                return nodeInfo.cellInfo.First().text;
            return string.Empty;
        }

        string GetCellReference(NodeInfo nodeInfo, string columnName)
        {
            return GetNodeReference(nodeInfo) + " - " + columnName;
        }

        void tree_MouseUp(object sender, MouseEventArgs e)
        {
            //show accept/reject context-menu if node/cell contains change, respectively always (without checking for changes) if column is clicked
            TreeList tree = sender as TreeList;

            if (e.Button == MouseButtons.Right && ModifierKeys == Keys.None)
            {
                Point pt = tree.PointToClient(MousePosition);
                TreeListHitInfo hitInfo = tree.CalcHitInfo(pt);

                NodeInfo nodeInfo = GetNodeInfo(hitInfo.Node);
                ColumnInfo columnInfo = GetColumnInfo(hitInfo.Column);
                CellInfo cellInfo = GetCellInfo(nodeInfo, columnInfo);

                //find out which view of the accept/reject context-menu must be shown
                AcceptRejectMenu.ViewType viewType = AcceptRejectMenu.ViewType.cell;
                string reference = string.Empty;
                
                if (cellInfo != null) //right-click on ... single cell
                {
                    if (!cellInfo.isChanged)
                        return;
                    reference = GetCellReference(nodeInfo, columnInfo.name);
                }
                else if (nodeInfo != null) //... node header-area
                {
                    if (hitInfo.HitInfoType == HitInfoType.Cell)
                        return; //possible, if click is on cell but cellInfo is null
                    reference = GetNodeReference(nodeInfo);
                    viewType = (nodeInfo.changeType == ChangeType.none) ? AcceptRejectMenu.ViewType.node_OnlyExCol : AcceptRejectMenu.ViewType.node_full;
                }
                else if (hitInfo.HitInfoType != HitInfoType.Cell && columnInfo != null) //... column
                {
                    if (hitInfo.HitInfoType == HitInfoType.Cell) //possible, if click is on cell but cellInfo is null
                        return;
                    reference = columnInfo.name;
                    viewType = AcceptRejectMenu.ViewType.column;
                }
                else if (hitInfo.HitInfoType == HitInfoType.ColumnButton) //... the top-left-area (top of rows, top of columns)
                {
                    reference = "All nodes";
                    viewType = AcceptRejectMenu.ViewType.node_OnlyExCol; //allow for expanding/collapsing all nodes
                }
                else
                    return;

                AcceptRejectMenu menu = new AcceptRejectMenu(this, reference, MousePosition, viewType, InvokeSequenceMenu(tree, hitInfo, nodeInfo));
                menu.SetReferenceInfo(tree, nodeInfo, columnInfo, cellInfo);
                menu.Show();
            }
        }

        bool InvokeSequenceMenu(TreeList tree, TreeListHitInfo hitInfo, NodeInfo nodeInfo)
        { //assess if the merge-control (in principle) shows order-differences (e.g. policy-order, function-order, parameter-order)
          //and if yes, find out whether a right-mouse-click asks for showing a respective (-ly adapted) context-menu

            if (_sequenceInfo == null || tree == treeRemote)
                return false; //no need to care about order-issues

            if (hitInfo.HitInfoType == HitInfoType.RowIndicator && //click on grey "buttons" on the very left (possibly showing function- or parameter-order-differences)
                nodeInfo != null && nodeInfo.subNodesSequenceInfo != null && nodeInfo.subNodesSequenceInfo.isChanged)
                return true;

            if (hitInfo.HitInfoType == HitInfoType.ColumnButton && //click in the top-left-area, i.e. top of rows, top of columns (possibly showing policy-order-differences)
                _sequenceInfo.isChanged)
                return true;
            
            return false; //context-menu does not need to take order-differences into account
        }

        void tree_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) { e.Menu.Items.Clear(); e.Allow = false; /* do not show standard column-context-menu */ }

        void GetSubNodes(ref List<TreeListNode> nodes,
                         bool mustBeVisible, TreeList tree, TreeListNode parentNode = null)
        {
            TreeListNodes potNodes = (parentNode == null) ? tree.Nodes : parentNode.Nodes;
            foreach (TreeListNode node in potNodes)
            {
                if (mustBeVisible == false || node.Visible == true)
                    nodes.Add(node);
                GetSubNodes(ref nodes, mustBeVisible, tree, node);
            }
        }

        List<TreeListNode> GetSubNodes(bool mustBeVisible, TreeList tree, TreeListNode topNode = null, bool includeTop = true)
        {
            List<TreeListNode> nodes = new List<TreeListNode>();
            if (topNode != null && includeTop)
                nodes.Add(topNode);
            GetSubNodes(ref nodes, mustBeVisible, tree, topNode);
            return nodes;
        }

        void RefreshNode(TreeListNode node, NodeInfo nodeInfo = null)
        {
            nodeInfo = nodeInfo == null ? GetNodeInfo(node) : nodeInfo;
            if (nodeInfo == null)
                return;
            node.StateImageIndex = Convert.ToInt32(nodeInfo.changeHandling);
            node.TreeList.RefreshNode(node); //for updating the colouring (green=accept, red=reject)
        }

        internal void ExpandCollapseAllNodes(bool expand, TreeList tree, bool includeSubNodes)
        {
            NodeInfo nodeInfo = new NodeInfo(string.Empty);
            foreach (TreeListNode node in tree.Nodes)
            {
                nodeInfo.node = node;
                if (expand) ExpandNode(nodeInfo, includeSubNodes);
                else CollapseNode(nodeInfo, includeSubNodes);
            }
        }

        internal void ExpandNode(NodeInfo nodeInfo, bool includeSubNodes)
        {
            if (nodeInfo.node == null) return;
            if (includeSubNodes) nodeInfo.node.ExpandAll(); else nodeInfo.node.Expanded = true;
        }

        internal void CollapseNode(NodeInfo nodeInfo, bool includeSubNodes)
        {
            if (nodeInfo.node == null) return;
            if (includeSubNodes) CollapseAll(nodeInfo.node); else nodeInfo.node.Expanded = false;
        }

        void CollapseAll(TreeListNode node)
        {
            node.Expanded = false;
            if (!node.HasChildren) return;
            foreach (TreeListNode subNode in node.Nodes)
                CollapseAll(subNode);
        }

        internal void AcceptReject(bool accept, bool mustBeVisible, NodeInfo nodeInfo, ColumnInfo columnInfo, CellInfo cellInfo)
        {
            if (cellInfo != null)
            {
                if (!cellInfo.isChanged || (mustBeVisible && cellInfo.column != null && cellInfo.column.Visible == false))
                    return;
                if (accept && cellInfo.isConflicted) //user tries to accept a conflicted change
                {
                    CellInfo twinCellInfo = GetTwinCellInfo(cellInfo);
                    if (twinCellInfo.acceptChange) //care in conflict case must only be taken if user tries to accept and the "competitive" change is currently accepted
                    {                              //i.e. nor rejecting doesn't any harm nor accepting, if the competitive change is rejected
                        if (radConflictWarn.Checked)
                        {
                            if (Tools.UserInfoHandler.GetInfo("'" + GetCellReference(nodeInfo, cellInfo.column.Name) +
                                "' changed in local and remote." + Environment.NewLine +
                                "Change can be only accepted in one of them and will be automatically rejected in the other.",
                                MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                                return;
                        }
                        else //there is a preference (set via radio-buttons) to prefer local or remote changes in conflict-case
                        {
                            bool tryLocalAccept = cellInfo.node.TreeList == treeLocal;
                            if ((tryLocalAccept && radConflictPreferRemote.Checked) || (!tryLocalAccept && radConflictPreferLocal.Checked))
                                return; //prevent local change if remote is preferred in conflict case and vice-versa
                        }

                        //competitive accept must be turned to reject
                        twinCellInfo.acceptChange = false;
                        NodeInfo twinNodeInfo = GetNodeInfo(twinCellInfo.node);
                        twinNodeInfo.changeHandling = GetNodeChangeHandling(twinNodeInfo, ChangeHandling.reject);
                        RefreshNode(twinCellInfo.node); //for updating the accept-symbols and the colouring                        
                    }
                }
                cellInfo.acceptChange = accept;
                nodeInfo.changeHandling = GetNodeChangeHandling(nodeInfo, accept ? ChangeHandling.accept : ChangeHandling.reject);
                RefreshNode(cellInfo.node); //for updating the accept-symbols and the colouring
            }
            else if (nodeInfo != null)
                AcceptRejectRange(accept, mustBeVisible, nodeInfo.node.TreeList, nodeInfo.node);
            else if (columnInfo != null)
                AcceptRejectRange(accept, mustBeVisible, columnInfo.column.TreeList, null, columnInfo.ID);
        }

        void AcceptRejectRange(bool accept, bool mustBeVisible, TreeList tree, TreeListNode topNode = null, string columnID = "")
        {
            Cursor = Cursors.WaitCursor;
            foreach (TreeListNode node in GetSubNodes(mustBeVisible, tree, topNode))
            {
                NodeInfo nodeInfo = GetNodeInfo(node);
                if (nodeInfo == null || nodeInfo.changeType == ChangeType.none)
                    continue;
                
                if (nodeInfo.changeType == ChangeType.changed)
                {
                    foreach (CellInfo cellInfo in nodeInfo.cellInfo)
                        if (columnID == string.Empty || cellInfo.columnID == columnID)
                            AcceptReject(accept, mustBeVisible, nodeInfo, null, cellInfo);
                }
                else if (nodeInfo.changeType == ChangeType.added || nodeInfo.changeType == ChangeType.removed)
                {
                    nodeInfo.changeHandling = accept ? ChangeHandling.accept : ChangeHandling.reject;
                    RefreshNode(nodeInfo.node); //for updating the accept-symbols and the colouring
                }
            }
            Cursor = Cursors.Default;
        }

        static internal ChangeHandling GetNodeChangeHandling(NodeInfo nodeInfo, ChangeHandling defaultHandling = ChangeHandling.accept)
        {
            switch (nodeInfo.changeType)
            {
                case ChangeType.none: return ChangeHandling.none;
                case ChangeType.added: return defaultHandling;
                case ChangeType.removed: return defaultHandling;
                default: //ChangeType.changed: change handling of node depends on change handling of single (changed) cells
                    if (nodeInfo.cellInfo.Count() == 0)
                        return nodeInfo.changeHandling; //do not change change handling if there is no cell-info

                    ChangeHandling handling = defaultHandling;
                    List<CellInfo> ci = new List<CellInfo>(); ci.AddRange(nodeInfo.cellInfo);
                    if (nodeInfo.subNodesSequenceInfo != null) ci.Add(new CellInfo(string.Empty, string.Empty, string.Empty,
                        nodeInfo.subNodesSequenceInfo.isChanged, nodeInfo.subNodesSequenceInfo.acceptChange)); //add a fake cell-info, to take account of possible order-changes
                    foreach (CellInfo cellInfo in ci)
                    {
                        if (!cellInfo.isChanged) continue;
                        if (handling == ChangeHandling.none)
                            handling = cellInfo.acceptChange ? ChangeHandling.accept : ChangeHandling.reject;
                        else if ((handling == ChangeHandling.accept && !cellInfo.acceptChange) ||
                                 (handling == ChangeHandling.reject && cellInfo.acceptChange))
                        {
                            handling = ChangeHandling.mixed;
                            break;
                        }
                    }

                    return handling;
            }
        }

        void chkAllSettings_CheckedChanged(object sender, EventArgs e) { lstSettings.Enabled = !chkAllSettings.Checked; }
        void chkAllValues_CheckedChanged(object sender, EventArgs e) { lstValues.Enabled = !chkAllValues.Checked; }

        void btnApplyFilter_Click(object sender, EventArgs e) { ApplyFilter(); }
        void btnReleaseFilter_Click(object sender, EventArgs e) { ReleaseFilter(); }

        const string SEPARATOR_LIST = "SEP_LIST";
        const string SEPARATOR_ELEMENT = "SEP_ELE";

        const string SEQUENCE_COLUMN_NAME = "Order";
        const string SEQUENCE_COLUMN_ID = "<OrderID>";

        internal string Store(string type)
        {

            string content = string.Empty;
            foreach (ColumnInfo columnInfo in _columInfo)
                content += columnInfo.Store() + SEPARATOR_ELEMENT;

            content += SEPARATOR_LIST;

            foreach (NodeInfo nodeInfo in _nodeInfoLocal)
                content += nodeInfo.Store() + SEPARATOR_ELEMENT;
            content += SEPARATOR_LIST;
            foreach (NodeInfo nodeInfo in _nodeInfoRemote)
                content += nodeInfo.Store() + SEPARATOR_ELEMENT;
            content += SEPARATOR_LIST;
            content += _sequenceInfo != null ? _sequenceInfo.Store() : SubNodesSequenceInfo.NO_SEQUENCE_INFO;
            return content;
        }


        internal void Restore(string content)
        {
            _nodeInfoLocal = new List<NodeInfo>();
            _nodeInfoRemote = new List<NodeInfo>();

            string[] lists = content.Split(new string[] { SEPARATOR_LIST }, StringSplitOptions.None);
            if (lists.Count() != 4)
                return;
            
            foreach (string sColumnInfo in lists[0].Split(new string[] { SEPARATOR_ELEMENT }, StringSplitOptions.None))
            {
                ColumnInfo columnInfo = new ColumnInfo(string.Empty);
                columnInfo.Restore(sColumnInfo);
                if (columnInfo.ID != string.Empty)
                    _columInfo.Add(columnInfo);
            }
            foreach (string sNodeInfo in lists[1].Split(new string[] { SEPARATOR_ELEMENT }, StringSplitOptions.None))
            {
                NodeInfo nodeInfo = new NodeInfo(string.Empty);
                nodeInfo.Restore(sNodeInfo);
                if (nodeInfo.ID != string.Empty)
                    _nodeInfoLocal.Add(nodeInfo);
            }
            foreach (string sNodeInfo in lists[2].Split(new string[] { SEPARATOR_ELEMENT }, StringSplitOptions.None))
            {
                NodeInfo nodeInfo = new NodeInfo(string.Empty);
                nodeInfo.Restore(sNodeInfo);
                if (nodeInfo.ID != string.Empty)
                    _nodeInfoRemote.Add(nodeInfo);
            }
            if (lists[3] != SubNodesSequenceInfo.NO_SEQUENCE_INFO)
            {
                _sequenceInfo = new SubNodesSequenceInfo();
                _sequenceInfo.Restore(lists[3]);
            }
        }

        void btnAcceptAll_Click(object sender, EventArgs e)
        {
            AcceptRejectAll(true);
        }

        void btnRejectAll_Click(object sender, EventArgs e)
        {
            AcceptRejectAll(false);
        }

        void AcceptRejectAll(bool accept)
        {
            AcceptRejectRange(accept, chkVisibleOnly.Checked, radAccRejAllLocal.Checked ? treeLocal : treeRemote);
            if (!radAccRejAllLocal.Checked)
                return; //order-changes are only relevant for local tree
            if (!chkVisibleOnly.Checked)
                AcceptRejectOrder(accept, true, null);
            if (chkVisibleOnly.Checked)
            {
                AcceptRejectOrder(accept, false, null);
                foreach (TreeListNode node in GetSubNodes(true, treeLocal))
                    AcceptRejectOrder(accept, false, GetNodeInfo(node));
            }
        }

        void treeLocal_CustomDrawNodeIndicator(object sender, CustomDrawNodeIndicatorEventArgs e)
        {
            NodeInfo nodeInfo = e.Node == null ? null : GetNodeInfo(e.Node);

            if (nodeInfo == null || nodeInfo.subNodesSequenceInfo == null || !nodeInfo.subNodesSequenceInfo.isChanged)
                return;
            
            //indicate differences in order of sub-nodes
            e.Graphics.FillRectangle(new SolidBrush(nodeInfo.subNodesSequenceInfo.acceptChange ? COLOUR_ACCEPT : COLOUR_REJECT),
                new Rectangle(e.Bounds.Left + 1, e.Bounds.Top + 1, e.Bounds.Width - 3, e.Bounds.Height - 3));
            //dilettante attempt to point user to order changes
            e.Appearance.DrawString(e.Cache, " !", e.Bounds, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.DarkSlateBlue), StringFormat.GenericDefault);
            e.Handled = true;
        }

        void treeLocal_CustomDrawColumnHeader(object sender, CustomDrawColumnHeaderEventArgs e)
        {
            if (e.ColumnType != HitInfoType.ColumnButton)
                return;

            if (_sequenceInfo == null || !_sequenceInfo.isChanged)
                return;
            
            //indicate differences in order of top-level-nodes
            e.Graphics.FillRectangle(new SolidBrush(_sequenceInfo.acceptChange ? COLOUR_ACCEPT : COLOUR_REJECT),
                new Rectangle(e.Bounds.Left + 1, e.Bounds.Top + 1, e.Bounds.Width - 3, e.Bounds.Height - 3));
            //dilettante attempt to point user to order changes
            e.Appearance.DrawString(e.Cache, " !", e.Bounds, new Font("Arial", 10, FontStyle.Bold), new SolidBrush(Color.DarkSlateBlue), StringFormat.GenericDefault);
            e.Handled = true;
        }

        void btnShowOrder_Click(object sender, EventArgs e)
        {
            TreeListColumn orderColumn = GetColumn(SEQUENCE_COLUMN_ID, treeLocal);
            if (orderColumn == null)
                return;
            bool show = orderColumn.Visible;
            btnShowOrder.Text = show ? "Show Order" : "Hide Order";
            orderColumn.Visible = !show;
        }

        internal void AcceptRejectOrder(bool accept, bool includeSubNodes, NodeInfo clickedNodeInfo)
        {
            if (_sequenceInfo == null)
                return;

            Cursor = Cursors.WaitCursor;
            List<TreeListNode> nodes = new List<TreeListNode>();
            if (clickedNodeInfo == null) //accept policy-order (click in top-right-field of tree)
            {
                _sequenceInfo.acceptChange = accept;
                foreach (TreeListColumn col in treeLocal.Columns)
                    treeLocal.InvalidateColumnHeader(col);
                treeLocal.Refresh();

                if (includeSubNodes)
                    nodes = GetSubNodes(false, treeLocal);
            }
            else
            {
                if (!includeSubNodes)
                    nodes.Add(clickedNodeInfo.node);
                else
                    nodes = GetSubNodes(false, treeLocal, clickedNodeInfo.node);
            }
            foreach (TreeListNode node in nodes)
            {
                NodeInfo nodeInfo = GetNodeInfo(node);
                if (nodeInfo == null || nodeInfo.changeType != ChangeType.changed || nodeInfo.subNodesSequenceInfo == null)
                    continue;
                nodeInfo.subNodesSequenceInfo.acceptChange = accept;
                nodeInfo.changeHandling = GetNodeChangeHandling(nodeInfo);
                RefreshNode(nodeInfo.node);
            }
            Cursor = Cursors.Default;            
        }

        internal List<NodeInfo> GetNodeInfoLocal(string type = "none") {
            //_nodeInfoLocal needs to be updated to removed additional columns if uprating indices
            if (type.Equals(MergeForm.UPRATING_INDICES))
            {
                getNodeInfoUprating();

            }
            return _nodeInfoLocal;
        }



        internal List<NodeInfo> GetNodeInfoRemote(string type = "none")
        {
            //_nodeInfoRemote needs to be updated to removed additional columns if uprating indices
            if (type.Equals(MergeForm.UPRATING_INDICES))
            {
                getNodeInfoUprating();

             }

            return _nodeInfoRemote;
        }

        /// <summary>
        /// Creates the new NodeIfo with the initial YearValues parameter according to the options selected by the user
        /// </summary>
        internal void getNodeInfoUprating()
        {
            if (_needUpdate)
            {
                List<NodeInfo> _nodeInfoLocalTemp = _nodeInfoLocal;
                List<NodeInfo> _nodeInfoRemoteTemp = _nodeInfoRemote;
                _columInfo = changeColumnInfo(_columInfo);
                _nodeInfoLocal = ChangeNodeInfoForUpratingFactors(_nodeInfoLocalTemp, _nodeInfoRemoteTemp);
                _nodeInfoRemote = _nodeInfoLocal;
                _needUpdate = false;
            }
        }

        private void setBestColumnWidths(TreeList t)
        {
            t.BestFitColumns();
            foreach (TreeListColumn col in t.Columns)
            {
                col.OptionsColumn.FixedWidth = true;
                col.Width = Math.Min(col.Width, col.Name.Length > 2 && col.Name[2] == '_' ? 60 : 300); 
            }
        }

        private void treeLocal_ColumnWidthChanged(object sender, ColumnChangedEventArgs e)
        {
            MethodInfo mi = treeRemote.GetType().GetMethod("ResizeColumn", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(treeRemote, new object[] { e.Column.AbsoluteIndex, e.Column.VisibleWidth - treeRemote.Columns[e.Column.AbsoluteIndex].VisibleWidth, int.MaxValue });
        }

        private void treeRemote_ColumnWidthChanged(object sender, ColumnChangedEventArgs e)
        {
            MethodInfo mi = treeLocal.GetType().GetMethod("ResizeColumn", BindingFlags.NonPublic | BindingFlags.Instance);
            mi.Invoke(treeLocal, new object[] { e.Column.AbsoluteIndex, e.Column.VisibleWidth - treeLocal.Columns[e.Column.AbsoluteIndex].VisibleWidth, int.MaxValue });
        }

        private void treeLocal_LeftCoordChanged(object sender, EventArgs e)
        {
            treeRemote.LeftCoord = treeLocal.LeftCoord;
        }

        private void treeRemote_LeftCoordChanged(object sender, EventArgs e)
        {
            treeLocal.LeftCoord = treeRemote.LeftCoord;
        }

        private void localToolTipController_GetActiveObjectInfo(object sender, DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            // show tooltip with order-differences if mouse is over the lefthand button of an item where the order is changed
            try
            {
                TreeList tree = e.SelectedControl as TreeList; if (tree == null) return;
                TreeListHitInfo hitInfo = tree.CalcHitInfo(e.ControlMousePosition); if (hitInfo == null) return;
                TreeListNodes subNodes;
                if (hitInfo.HitInfoType == HitInfoType.ColumnButton)
                {
                    if (_sequenceInfo == null || !_sequenceInfo.isChanged) return;
                    subNodes = tree.Nodes;
                }
                else if (hitInfo.HitInfoType == HitInfoType.RowIndicator)
                {
                    NodeInfo nodeInfo = hitInfo.Node == null ? null : GetNodeInfo(hitInfo.Node);
                    if (nodeInfo == null || nodeInfo.subNodesSequenceInfo == null || !nodeInfo.subNodesSequenceInfo.isChanged) return;
                    subNodes = hitInfo.Node.Nodes;
                }
                else return;

                string toolTip = "Differences in order of elements:" + Environment.NewLine;
                object cellInfo = new DevExpress.XtraTreeList.ViewInfo.TreeListCellToolTipInfo(hitInfo.Node, hitInfo.Column, null);                
                foreach (TreeListNode subNode in subNodes)
                {
                    NodeInfo subNodeInfo = GetNodeInfo(subNode);
                    toolTip += subNode.GetDisplayText(tree.Columns.First()) + " " + subNodeInfo.nodeSequenceInfo + Environment.NewLine;
                }
                e.Info = new DevExpress.Utils.ToolTipControlInfo(cellInfo, toolTip);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Calls the methods needed to update the locaNodes to the initial structure with YearValues
        /// </summary>
        /// <param name="localNodeInfoList"></param>
        /// <param name="remoteNodeInfoList"></param>
        /// <returns></returns>
        internal List<NodeInfo> ChangeNodeInfoForUpratingFactors(List<NodeInfo> localNodeInfoList, List<NodeInfo> remoteNodeInfoList)
        {
            List<string> years = new List<string>();
            Dictionary<String, UpratingYearsComparison> dictUpratingYearsComparison = PopulateUpratingYearsComparison(localNodeInfoList, remoteNodeInfoList, years);
            List<NodeInfo> newNodesList = PopulateNewNodes(localNodeInfoList, dictUpratingYearsComparison, years);
            Dictionary<String, UpratingYearsComparison> dictOtherFieldsComparison = PopulateOtherFieldsDictionary(newNodesList, remoteNodeInfoList);
            newNodesList = PopulateNewNodesOtherValues(newNodesList, dictOtherFieldsComparison);
            return newNodesList;
        }

        /// <summary>
        /// Populates a dictionary for the additional fields in the uprating factors (comments and description)
        /// </summary>
        /// <param name="localNodeInfoList"></param>
        /// <param name="remoteNodeInfoList"></param>
        /// <returns></returns>
        internal Dictionary<String, UpratingYearsComparison> PopulateOtherFieldsDictionary(List<NodeInfo> localNodeInfoList, List<NodeInfo> remoteNodeInfoList)
        {
            
            Dictionary<string, UpratingYearsComparison> otherFieldsDict = new Dictionary<String, UpratingYearsComparison>();
            foreach (MergeControl.NodeInfo node in localNodeInfoList)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (MergeForm.UPRATING_INDICES_COLUMNS.Contains(cell.columnID) && !cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        string nodeColumnId = node.ID + "_" + cell.columnID;
                        string nodeYearId = node.ID + "_" + cell.columnID;
                        UpratingYearsComparison upratingYearsComparison = new UpratingYearsComparison();
                        upratingYearsComparison.valueLocal = cell.text;
                        upratingYearsComparison.isChangedLocal = cell.isChanged;
                        upratingYearsComparison.acceptChangeLocal = cell.acceptChange;
                        upratingYearsComparison.changeTypeLocal = node.changeType;
                        upratingYearsComparison.changeHandlingLocal = node.changeHandling;
                        otherFieldsDict.Add(nodeYearId, upratingYearsComparison);
                    }
                }
            }

            foreach (MergeControl.NodeInfo node in remoteNodeInfoList)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (MergeForm.UPRATING_INDICES_COLUMNS.Contains(cell.columnID) &&  !cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        string nodeYearId = node.ID + "_" + cell.columnID;
                        UpratingYearsComparison upratingYearsComparison = otherFieldsDict[nodeYearId];
                        upratingYearsComparison.valueRemote = cell.text;
                        upratingYearsComparison.isChangedRemote = cell.isChanged;
                        upratingYearsComparison.acceptChangeRemote = cell.acceptChange;
                        upratingYearsComparison.changeTypeRemote = node.changeType;
                        upratingYearsComparison.changeHandlingRemote = node.changeHandling;
                        otherFieldsDict[nodeYearId] = upratingYearsComparison;
                    }


                }
            }
            return otherFieldsDict;
        }

        /// <summary>
        /// Modify the new nodes to include the changes in comments and description
        /// </summary>
        /// <param name="localNodeInfoList"></param>
        /// <param name="dictUpratingYearsComparison"></param>
        /// <param name="years"></param>
        /// <returns></returns>
        internal List<NodeInfo> PopulateNewNodesOtherValues(List<NodeInfo> newNodeList, Dictionary<String, UpratingYearsComparison> dictOtherFieldsComparison)
        {
            List<NodeInfo> finalNodeInfoList = new List<MergeControl.NodeInfo>();
            foreach (MergeControl.NodeInfo node in newNodeList)
            {
                MergeControl.NodeInfo nodemod = new MergeControl.NodeInfo(node.ID);
                nodemod.ID = node.ID;
                nodemod.changeHandling = node.changeHandling;
                nodemod.changeType = node.changeType;

                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    //This one does not need to be modified
                    if (cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        nodemod.cellInfo.Add(cell);
                    }
                    else
                    {
                        UpratingYearsComparison singleCellInfo = dictOtherFieldsComparison[node.ID + "_" + cell.columnID];
                        MergeControl.CellInfo otherFieldCell = new MergeControl.CellInfo(cell.columnID);
                        bool areThereChanges = false;

                        //If there are changes
                        if(singleCellInfo.changeTypeLocal == ChangeType.changed || singleCellInfo.changeTypeRemote == ChangeType.changed)
                        {
                            if (singleCellInfo.isChangedLocal && singleCellInfo.acceptChangeLocal)
                            { 
                                //No changes: Since it is local, we keep it as it is
                            }
                            else if (singleCellInfo.isChangedRemote && singleCellInfo.acceptChangeRemote)
                            {
                                otherFieldCell.SetText(singleCellInfo.valueRemote);
                                otherFieldCell.isChanged = true;
                                otherFieldCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.changed;
                                areThereChanges = true;
                            }
                            else if ((singleCellInfo.isChangedLocal && !singleCellInfo.acceptChangeLocal) || (singleCellInfo.isChangedRemote && !singleCellInfo.acceptChangeRemote))
                            {
                                if (singleCellInfo.isChangedLocal && !singleCellInfo.acceptChangeLocal)
                                {
                                    otherFieldCell.SetText(singleCellInfo.valueRemote);
                                    otherFieldCell.isChanged = true;
                                    otherFieldCell.acceptChange = true;
                                    nodemod.changeHandling = ChangeHandling.accept;
                                    nodemod.changeType = ChangeType.changed;
                                    areThereChanges = true;
                                }
                                else if (singleCellInfo.isChangedRemote && !singleCellInfo.acceptChangeRemote && (!singleCellInfo.isChangedLocal || !singleCellInfo.acceptChangeLocal))
                                {
                                    //We keep local, no changes are needed.
                                }
                                
                            }

                        }

                        if (!areThereChanges)
                        {
                            nodemod.cellInfo.Add(cell);
                        }
                        else
                        {
                            nodemod.cellInfo.Add(otherFieldCell);
                        }

                    }
                }

                finalNodeInfoList.Add(nodemod);
            }
            return finalNodeInfoList;
        }

        /// <summary>
        /// Builds a dictionary that contains the values selected by the user in the merge tool by node and year
        /// </summary>
        /// <param name="localNodeInfoList"></param>
        /// <param name="remoteNodeInfoList"></param>
        /// <param name="years"></param>
        /// <returns>Dictionary that contains the values selected by the user in the merge tool by node and year</returns>
        internal Dictionary<String, UpratingYearsComparison> PopulateUpratingYearsComparison(List<NodeInfo> localNodeInfoList, List<NodeInfo> remoteNodeInfoList, List<String> years)
        {

            Dictionary<String, UpratingYearsComparison> dictUpratingYearsComparison = new Dictionary<String, UpratingYearsComparison>();
            foreach (MergeControl.NodeInfo node in localNodeInfoList)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (!MergeForm.UPRATING_INDICES_COLUMNS.Contains(cell.columnID))
                    {
                        string nodeYearId = node.ID + "_" + cell.columnID;
                        UpratingYearsComparison upratingYearsComparison = new UpratingYearsComparison();
                        upratingYearsComparison.valueLocal = cell.text;
                        upratingYearsComparison.isChangedLocal = cell.isChanged;
                        upratingYearsComparison.acceptChangeLocal = cell.acceptChange;
                        upratingYearsComparison.changeTypeLocal = node.changeType;
                        upratingYearsComparison.changeHandlingLocal = node.changeHandling;
                        dictUpratingYearsComparison.Add(nodeYearId, upratingYearsComparison);

                        if (!years.Contains(cell.columnID))
                        {
                            years.Add(cell.columnID);
                        }
                    }
                }
            }

            foreach (MergeControl.NodeInfo node in remoteNodeInfoList)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (!MergeForm.UPRATING_INDICES_COLUMNS.Contains(cell.columnID))
                    {
                        string nodeYearId = node.ID + "_" + cell.columnID;
                        UpratingYearsComparison upratingYearsComparison = dictUpratingYearsComparison[nodeYearId];
                        upratingYearsComparison.valueRemote = cell.text;
                        upratingYearsComparison.isChangedRemote = cell.isChanged;
                        upratingYearsComparison.acceptChangeRemote = cell.acceptChange;
                        upratingYearsComparison.changeTypeRemote = node.changeType;
                        upratingYearsComparison.changeHandlingRemote = node.changeHandling;
                        dictUpratingYearsComparison[nodeYearId] = upratingYearsComparison;

                        if (!years.Contains(cell.columnID))
                        {
                            years.Add(cell.columnID);
                        }
                    }


                }
            }

            return dictUpratingYearsComparison;

        }

        /// <summary>
        /// Populates the new localNodeInfo with the structure YearValues according to the values selected by the user in the merge tool
        /// </summary>
        /// <param name="localNodeInfoList">Initial localNodeInfo</param>
        /// <param name="dictUpratingYearsComparison">A dictionary that contains the values selected by the user per node and year</param>
        /// <param name="years">List of years</param>
        /// <returns>New list of nodes with the initial YearValues structure</returns>
        internal List<NodeInfo> PopulateNewNodes(List<NodeInfo> localNodeInfoList, Dictionary<String, UpratingYearsComparison> dictUpratingYearsComparison, List<String> years)
        {
            List<NodeInfo> modNodeInfoList = new List<MergeControl.NodeInfo>();
            foreach (MergeControl.NodeInfo node in localNodeInfoList)
            {
                MergeControl.NodeInfo nodemod = new MergeControl.NodeInfo(node.ID);
                nodemod.ID = node.ID;
                nodemod.changeHandling = ChangeHandling.none;
                nodemod.changeType = ChangeType.none;

                Dictionary<string, string> finalYearsValues = new Dictionary<string, string>();
                MergeControl.CellInfo yearValuesCell = new MergeControl.CellInfo(MergeForm.YEAR_VALUES);
                yearValuesCell.isChanged = false;
                yearValuesCell.acceptChange = false;

                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (!years.Contains(cell.columnID))
                    {
                        nodemod.cellInfo.Add(cell);
                    }
                    else
                    {
                        UpratingYearsComparison singleCellInfo = dictUpratingYearsComparison[node.ID + "_" + cell.columnID];

                        if (singleCellInfo.changeTypeLocal == ChangeType.added || singleCellInfo.changeTypeLocal == ChangeType.removed || singleCellInfo.changeTypeRemote == ChangeType.added || singleCellInfo.changeTypeRemote == ChangeType.removed)
                        {
                            ChangeType localChange = ChangeType.none;
                            //If a new row is added in local, and local is the parent
                            if (singleCellInfo.changeTypeRemote == ChangeType.removed)
                            {
                                if (singleCellInfo.changeHandlingRemote == ChangeHandling.accept) //It is removed from local
                                {
                                    localChange = ChangeType.removed;
                                }
                                else if (singleCellInfo.changeHandlingRemote == ChangeHandling.reject)//No changes in local
                                {
                                    //It is none by default
                                }

                            }
                            //If a new row is added in local and remote is the parent
                            else if (singleCellInfo.changeTypeLocal == ChangeType.added)
                            {
                                if (singleCellInfo.changeHandlingLocal == ChangeHandling.accept) //No changes in local
                                {
                                    //It is none by default;
                                }
                                else if (singleCellInfo.changeHandlingLocal == ChangeHandling.reject)// It is removed from local
                                {
                                    localChange = ChangeType.removed;
                                }
                            }
                            // A row is removed from local, and local is the parent
                            else if (singleCellInfo.changeTypeRemote == ChangeType.added)
                            {
                                if (singleCellInfo.changeHandlingRemote == ChangeHandling.accept) //It is added in local
                                {
                                    localChange = ChangeType.added;
                                }
                                else if (singleCellInfo.changeHandlingRemote == ChangeHandling.reject)//No changes in local
                                {
                                    nodemod = null;
                                    break;
                                }

                            }
                            // A row is removed from local, and remote is the parent
                            else if (singleCellInfo.changeTypeLocal == ChangeType.removed)
                            {
                                if (singleCellInfo.changeHandlingLocal == ChangeHandling.accept) //No changes in local
                                {
                                    nodemod = null;
                                    break;
                                }
                                else if (singleCellInfo.changeHandlingLocal == ChangeHandling.reject)//It is accepted from remote
                                {

                                    localChange = ChangeType.added;
                                }
                            }

                            if (localChange == ChangeType.none)
                            {
                                finalYearsValues.Add(cell.columnID, singleCellInfo.valueLocal);
                                yearValuesCell.isChanged = true;
                                yearValuesCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.changed;
                            }
                            else if (localChange == ChangeType.removed)
                            {
                                finalYearsValues.Add(cell.columnID, singleCellInfo.valueLocal);
                                yearValuesCell.isChanged = true;
                                yearValuesCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.removed;
                            }
                            else
                            {
                                finalYearsValues.Add(cell.columnID, singleCellInfo.valueRemote);
                                yearValuesCell.isChanged = true;
                                yearValuesCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.added;
                            }
                        }
                        else
                        {

                            if (singleCellInfo.isChangedLocal && singleCellInfo.acceptChangeLocal)
                            {
                                finalYearsValues.Add(cell.columnID, singleCellInfo.valueLocal);
                                yearValuesCell.isChanged = true;
                                yearValuesCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.changed;
                            }
                            else if (singleCellInfo.isChangedRemote && singleCellInfo.acceptChangeRemote)
                            {
                                finalYearsValues.Add(cell.columnID, singleCellInfo.valueRemote);
                                yearValuesCell.isChanged = true;
                                yearValuesCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.changed;
                            }
                            else if ((singleCellInfo.isChangedLocal && !singleCellInfo.acceptChangeLocal) || (singleCellInfo.isChangedRemote && !singleCellInfo.acceptChangeRemote))
                            {
                                if (singleCellInfo.isChangedLocal && !singleCellInfo.acceptChangeLocal)
                                {
                                    finalYearsValues.Add(cell.columnID, singleCellInfo.valueRemote);

                                }
                                else if (singleCellInfo.isChangedRemote && !singleCellInfo.acceptChangeRemote && (!singleCellInfo.isChangedLocal || !singleCellInfo.acceptChangeLocal))
                                {
                                    finalYearsValues.Add(cell.columnID, singleCellInfo.valueLocal);
                                }
                                yearValuesCell.isChanged = true;
                                yearValuesCell.acceptChange = true;
                                nodemod.changeHandling = ChangeHandling.accept;
                                nodemod.changeType = ChangeType.changed;
                            }
                            else
                            {
                                finalYearsValues.Add(cell.columnID, singleCellInfo.valueLocal);
                            }
                        }
                    }
                }

                if (nodemod != null)
                {
                    String yearValues = "";
                    foreach (string year in years)
                    {

                        if (yearValues != "")
                        {
                            yearValues = yearValues + UpratingIndices.UpratingIndicesForm._separator;
                        }

                        yearValues = yearValues + year + "|" + finalYearsValues[year];
                    }

                    yearValuesCell.SetText(yearValues);
                    nodemod.cellInfo.Add(yearValuesCell);

                    modNodeInfoList.Add(nodemod);
                }
            }

            return modNodeInfoList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_columInfo">Columninfo which contains all years in Uprating factors</param>
        /// <returns>ColumnInfo with the initial structure (with the YearValues parameters)</returns>

        internal List<ColumnInfo> changeColumnInfo(List<ColumnInfo> _columInfo)
        {
            List<ColumnInfo> columnInfoNew = new List<ColumnInfo>();
            bool yearValuesExist = false;
            foreach (ColumnInfo columnInfo in _columInfo)
            {
                string columnName = columnInfo.name;
                if (MergeForm.UPRATING_INDICES_COLUMNS.Contains(columnName))
                {
                    columnInfoNew.Add(columnInfo);
                }

                if (columnName.Equals(MergeForm.YEAR_VALUES))
                {
                    yearValuesExist = true;
                }
            }

            if (!yearValuesExist)
            {
                ColumnInfo yearValues = new ColumnInfo(MergeForm.YEAR_VALUES);
                columnInfoNew.Add(yearValues);

            }
            _columInfo = columnInfoNew;

            return _columInfo;
        }
    }
}
