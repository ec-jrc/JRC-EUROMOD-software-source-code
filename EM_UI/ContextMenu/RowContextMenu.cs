using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.Dialogs;
using EM_UI.TreeListManagement;
using DevExpress.XtraTreeList.Columns;
using EM_UI.TreeListTags;

namespace EM_UI.ContextMenu
{
    internal partial class RowContextMenu : UserControl
    {
        EM_UI_MainForm _mainForm = null;
        TreeListNode _senderNode = null;

        internal RowContextMenu(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
            InitializeComponent();
        }

        internal ContextMenuStrip GetContextMenu(TreeListNode senderNode)
        {
            _senderNode = senderNode;
            List<TreeListNode> nodeList = EM_AppContext.Instance.GetActiveCountryMainForm().GetMultiCellSelector().GetSelectedNodes();
            mniHideSelectedRows.Enabled = (nodeList.Count>0);
            //fill drop-down menu for 'Hide Rows unto ...' with the rownumber of the node itself (e.g. 1.2) and all its successors on the same level (e.g. 1.3, 1.4, ...)
            cboHideRowsUnto.Items.Clear();
            cboHideRowsUnto.Items.Add(TreeListManager.GetNodeRowNumber(_senderNode, _senderNode.ParentNode));
            foreach (TreeListNode siblingNodeAfter in TreeListManager.GetSiblingNodesAfter(_senderNode))
                cboHideRowsUnto.Items.Add(TreeListManager.GetNodeRowNumber(siblingNodeAfter, siblingNodeAfter.ParentNode));
            cboHideRowsUnto.SelectedIndex = 0; //put starting node into textfield of drop-down

            return mnuRow;
        }

        #region EventHandlers

        internal static void HideRow(TreeListNode node)
        {
            if (node != null)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().ClearCellSelection();  //clear any existing selection of cells (to avoid that actions on selected cells change hidden rows)
                node.Visible = false;
            }
        }

        void mniHideRow_Click(object sender, EventArgs e)
        {
            HideRow(_senderNode);
        }

        internal static void HideAllOtherRows(TreeListNode node)
        {
            if (node == null)
                return;
            node.TreeList.BeginUpdate();
            EM_AppContext.Instance.GetActiveCountryMainForm().ClearCellSelection();  //clear any existing selection of cells (to avoid that actions on selected cells change hidden rows)
            foreach (TreeListNode siblingNode in TreeListManager.GetSiblingNodes(node))
                siblingNode.Visible = false;
            node.TreeList.EndUpdate();
        }

        private void mniHideSelectedRows_Click(object sender, EventArgs e)
        {
            List<TreeListNode> nodeList = EM_AppContext.Instance.GetActiveCountryMainForm().GetMultiCellSelector().GetSelectedNodes();
            if (nodeList != null)
            {
                _mainForm.treeList.BeginUpdate();
                foreach (TreeListNode n in nodeList) n.Visible = false;
                _mainForm.treeList.EndUpdate();
            }
            EM_AppContext.Instance.GetActiveCountryMainForm().ClearCellSelection();  //clear any existing selection of cells (to avoid that actions on selected cells change hidden rows)
        }

        void mniHideAllOtherRows_Click(object sender, EventArgs e)
        {
            HideAllOtherRows(_senderNode);
        }

        void mniHideSelectedNARows_Click(object sender, EventArgs e)
        {
            List<TreeListNode> nodeList = EM_AppContext.Instance.GetActiveCountryMainForm().GetMultiCellSelector().GetSelectedNodes(true, true);
            _mainForm.treeList.BeginUpdate();
            HideNARows(nodeList);
            _mainForm.treeList.EndUpdate();
        }

        void mniHideNARows_Click(object sender, EventArgs e)
        {
            List<TreeListNode> nodeList = EM_AppContext.Instance.GetActiveCountryMainForm().treeList.Nodes.ToList();
            _mainForm.treeList.BeginUpdate();
            HideNARows(nodeList);
            _mainForm.treeList.EndUpdate();
        }

        void HideNARows(List<TreeListNode> nodeList)
        {
            if (nodeList != null)
            {
                foreach (TreeListNode n in nodeList)
                {
                    if (n.Level == 0 || n.Level == 1)
                    {
                        if (isNodeNA(n)) n.Visible = false;
                        else if (n.Level == 0)
                        {
                            HideNARows(n.Nodes.ToList());
                        }
                    }

                }
            }
            EM_AppContext.Instance.GetActiveCountryMainForm().ClearCellSelection();  //clear any existing selection of cells (to avoid that actions on selected cells change hidden rows)
        }

        bool isNodeNA(TreeListNode node)
        {
            bool isNA = true;
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (column.Tag != null &&
                    !_mainForm.GetTreeListManager().GetIDsOfHiddenSystems().Contains((column.Tag as SystemTreeListTag).GetSystemRow().ID))
                {
                    string value = node.GetValue((column.Tag as SystemTreeListTag).GetSystemRow().Name).ToString();
                    if (value != "n/a")
                    {
                        isNA = false;
                        break;
                    }
                }
            }
            return isNA;
        }

        void mniUnhideRows_Click(object sender, EventArgs e)
        {
            List<KeyValuePair<TreeListNode, TreeListNode>> rangesOfHiddenNodes = new List<KeyValuePair<TreeListNode, TreeListNode>>();
            TreeListManager.GetRangesOfHiddenNodes(_mainForm.treeList.Nodes, ref rangesOfHiddenNodes);

            UnhideRowsForm unhideRowForm = new UnhideRowsForm(rangesOfHiddenNodes, _mainForm.GetTreeListManager());
            if (unhideRowForm.ShowDialog() == DialogResult.Cancel)
                return;

            _mainForm.ClearCellSelection();
            _mainForm.treeList.BeginUpdate();
            foreach (KeyValuePair<TreeListNode, TreeListNode> fromNodeToNode in unhideRowForm.GetRangesOfNodesToHide())
                _mainForm.GetTreeListManager().UnhideNodeRange(fromNodeToNode.Key, fromNodeToNode.Value);
            _mainForm.treeList.EndUpdate();
        }

        void cboHideRowsUnto_DropDownClosed(object sender, EventArgs e)
        {   
            mnuRow.Hide();

            EM_AppContext.Instance.GetActiveCountryMainForm().ClearCellSelection();  //clear any existing selection of cells (to avoid that actions on selected cells change hidden rows)

            //hide all nodes from the one the context menu was started from to the one selected from the combo-box with nodes after
            int index = 0;
            _senderNode.Visible = false;
            foreach (TreeListNode siblingNodeAfter in TreeListManager.GetSiblingNodesAfter(_senderNode))
            {
                if (index >= cboHideRowsUnto.SelectedIndex)
                    break;
                siblingNodeAfter.Visible = false;
                ++index;
            }
        }

        #endregion EventHandlers
    }
}
