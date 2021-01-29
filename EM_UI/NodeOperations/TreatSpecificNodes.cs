using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Nodes.Operations;

namespace EM_UI.NodeOperations
{
    internal class IsSpecificBase
    {
        internal virtual bool Execute(TreeListNode node) { return true; }
    }

    internal class TreatmentBase
    {
        public virtual void Execute(TreeListNode node) { }
        public virtual void FinalizeOperation() { }
    }

    internal class TreatSpecificNodes : TreeListOperation
    {
        IsSpecificBase _isSpecific = null;
        TreatmentBase _treatment = null;
        bool _visualiseTreated = false;  // expand parent node(s) of treated
        bool _expandTreated = false;     // expand parent node(s) and treated itself
        bool? _setVisibleTreated = null; // if true/false: set node.Visible = true/false, if null: do not change
        List<TreeListNode> _specificNodes = null;

        internal TreatSpecificNodes(IsSpecificBase isSpecific, TreatmentBase treatment, bool visualiseTreated, bool expandTreated = false, bool? setVisibleTreated = null)
            : base()
        {
            _isSpecific = isSpecific == null ? new IsSpecificBase() : isSpecific;
            _treatment = treatment == null ? new TreatmentBase() : treatment;
            _visualiseTreated = visualiseTreated;
            _expandTreated = expandTreated;
            _setVisibleTreated = setVisibleTreated;
            _specificNodes = new List<TreeListNode>();

            EM_AppContext.Instance.GetActiveCountryMainForm().TreeListBeginUnboundLoad(); //for performance reasons
        }

        internal List<TreeListNode> GetSpecificNodes()
        {
            return _specificNodes;
        }

        public override void Execute(TreeListNode senderNode)
        {
            if (!_isSpecific.Execute(senderNode))
                return;
            _treatment.Execute(senderNode);
            _specificNodes.Add(senderNode);
            if (_setVisibleTreated != null)
                senderNode.Visible = _setVisibleTreated == true ? true : false;
            if (!_visualiseTreated)
                return;
            if (_expandTreated)
                senderNode.ExpandAll();
            for (TreeListNode parentNode = senderNode.ParentNode; parentNode != null; parentNode = parentNode.ParentNode)
                parentNode.Expanded = true;
        }

        public override void FinalizeOperation()
        {
            base.FinalizeOperation();
            _treatment.FinalizeOperation();

            EM_AppContext.Instance.GetActiveCountryMainForm().TreeListEndUnboundLoad();
        }
    }

}
