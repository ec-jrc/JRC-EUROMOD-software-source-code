using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;
using EM_UI.TreeListManagement;

namespace EM_UI.Dialogs
{
    internal partial class UnhideRowsForm : Form
    {
        void UnhideRowsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            if (lstHiddenRows.CheckedIndices.Count == 0)
                DialogResult = DialogResult.Cancel;
            else
                DialogResult = DialogResult.OK;
            Close();
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        internal UnhideRowsForm(List<KeyValuePair<TreeListNode, TreeListNode>> rangesOfHiddenNodes, TreeListManager treeListManager)
        {
            InitializeComponent();
            foreach (KeyValuePair<TreeListNode, TreeListNode> fromNodeToNode in rangesOfHiddenNodes)
            {
                ListViewItem item = lstHiddenRows.Items.Add(TreeListManager.GetNodeRowNumber(fromNodeToNode.Key, fromNodeToNode.Key.ParentNode) + " - " +
                                                            TreeListManager.GetNodeRowNumber(fromNodeToNode.Value, fromNodeToNode.Value.ParentNode));
                item.Tag = fromNodeToNode;
            }
        }

        internal List<KeyValuePair<TreeListNode, TreeListNode>> GetRangesOfNodesToHide()
        {
            List<KeyValuePair<TreeListNode, TreeListNode>> rangesOfNodesToHide = new List<KeyValuePair<TreeListNode, TreeListNode>>();
            foreach (int indexChecked in lstHiddenRows.CheckedIndices)
                rangesOfNodesToHide.Add((KeyValuePair<TreeListNode, TreeListNode>)lstHiddenRows.Items[indexChecked].Tag);
            return rangesOfNodesToHide;
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem chk in lstHiddenRows.Items)
                chk.Checked = chkAll.Checked;
        }
    }
}
