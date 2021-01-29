using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using EM_UI.VersionControl.Merging;

namespace EM_UI.VersionControl.Merging
{
    internal partial class AcceptRejectMenu : Form
    {
        MergeControl _mergeControl = null;
        TreeList _tree = null;
        bool _viewSequenceElements = false;
        MergeControl.NodeInfo _nodeInfo = null;
        MergeControl.ColumnInfo _columnInfo = null;
        MergeControl.CellInfo _cellInfo = null;

        internal enum ViewType { cell, column, node_full, node_OnlyExCol };

        internal AcceptRejectMenu(MergeControl mergeControl, string reference, Point startPosition, ViewType viewType, bool viewSequenceElements)
        {
            InitializeComponent();

            _mergeControl = mergeControl;
            _viewSequenceElements = viewSequenceElements;

            int changeReferenceWidth = labReference.Width;
            labReference.Text = reference;
            changeReferenceWidth = labReference.Width - changeReferenceWidth;

            AdaptView(mergeControl, ref startPosition, viewType, changeReferenceWidth);

            this.StartPosition = FormStartPosition.Manual;
            this.Location = startPosition;
        }

        internal void SetReferenceInfo(TreeList tree, MergeControl.NodeInfo nodeInfo, MergeControl.ColumnInfo columnInfo, MergeControl.CellInfo cellInfo)
        {
            _tree = tree;  _nodeInfo = nodeInfo; _columnInfo = columnInfo; _cellInfo = cellInfo;
        }

        void AdaptView(MergeControl container, ref Point startPosition, ViewType viewType, int changeReferenceWidth)
        {
            //adapt height with respect to being invoked from node, cell or column
            switch (viewType)
            {
                case ViewType.cell: this.Height = chkVisible.Top; break; //cell-menu: accept/reject-buttons
                
                case ViewType.node_full: break; //"big" node-menu: all buttons and checkboxes
                
                case ViewType.node_OnlyExCol: //"small" node-menu: expand/collapse + include-sub-nodes-checkbox
                    if (_viewSequenceElements) break; //show "big" menu if order-changes need to be cared for
                    //exchange the upper and the lower part of the menu (to hide accept/reject and show expand/collapse)
                    chkIncludeSubNodes.Top = chkVisible.Top; chkVisible.Visible = false;
                    btnExpand.Top = btnAccept.Top; btnAccept.Visible = false;
                    btnCollapse.Top = btnReject.Top; btnReject.Visible = false;
                    this.Height = labSeparator.Top; break;

                case ViewType.column: this.Height = labSeparator.Top; break; //accept/reject-buttons + visible-only-checkbox
            }

            if (_viewSequenceElements)
            {
                btnAccept.Text = "Accept Order";
                btnReject.Text = "Reject Order";
                btnAccept.Image = EM_UI.Properties.Resources.merge_accept_order;
                btnReject.Image = EM_UI.Properties.Resources.merge_reject_order;
                chkVisible.Text = "Include Sub-Nodes";
            }

            //adapt width with respect to width of reference (i.e. name of component (e.g. "tin_sl"), column (e.g. "sl_demo") or cell-identifier)
            if (changeReferenceWidth > 0 && this.Width + changeReferenceWidth < this.Width * 2) //max-width = double of original width
                this.Width += changeReferenceWidth;

            if (startPosition.X + this.Width > container.Width)
                startPosition.X -= this.Width;
            if (startPosition.Y + this.Height > container.Height)
                startPosition.Y -= this.Height;
        }

        internal void btnAccept_Click(object sender, EventArgs e)
        {
            if (btnAccept.Text.ToLower().Contains("order"))
                _mergeControl.AcceptRejectOrder(true, chkVisible.Checked, _nodeInfo);
            else
                _mergeControl.AcceptReject(true, chkVisible.Checked, _nodeInfo, _columnInfo, _cellInfo);
            GetRidOff();
        }

        internal void btnReject_Click(object sender, EventArgs e)
        {
            if (btnReject.Text.ToLower().Contains("order"))
                _mergeControl.AcceptRejectOrder(false, chkVisible.Checked, _nodeInfo);
            else
                _mergeControl.AcceptReject(false, chkVisible.Checked, _nodeInfo, _columnInfo, _cellInfo);
            GetRidOff();
        }

        //various attempts to get rid off the quite penedrant context menu once it's not needed any more
        void AcceptRejectMenu_KeyUp(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Escape) GetRidOff(); }
        void AcceptRejectMenu_Deactivate(object sender, EventArgs e) { GetRidOff(); }
        void AcceptRejectMenu_Leave(object sender, EventArgs e) { GetRidOff(); } //probably that's not necessary
        internal void GetRidOff() { try { this.Hide(); this.Dispose(); this.Close(); } catch { } }

        void btnExpand_Click(object sender, EventArgs e)
        {
            if (_nodeInfo == null)
                _mergeControl.ExpandCollapseAllNodes(true, _tree, chkIncludeSubNodes.Checked);
            else
                _mergeControl.ExpandNode(_nodeInfo, chkIncludeSubNodes.Checked);
            GetRidOff();
        }

        void btnCollapse_Click(object sender, EventArgs e)
        {
            if (_nodeInfo == null)
                _mergeControl.ExpandCollapseAllNodes(false, _tree, chkIncludeSubNodes.Checked);
            else
                _mergeControl.CollapseNode(_nodeInfo, chkIncludeSubNodes.Checked);
            GetRidOff();
        }
    }
}
