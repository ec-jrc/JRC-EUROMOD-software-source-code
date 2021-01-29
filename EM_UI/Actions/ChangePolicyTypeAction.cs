using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.TreeListTags;

namespace EM_UI.Actions
{
    internal class ChangePolicyTypeAction : BaseAction // outdated - the menu item is not visible
    {
        TreeListNode _senderNode = null;
        string _newType = string.Empty;

        internal ChangePolicyTypeAction(TreeListNode senderNode, string newType)
        {
            _senderNode = senderNode;
            //_newType = newType.Substring(0, PolicyDefinitions._strLen_policyType).ToLower();
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return true;
        }

        internal override void PerformAction()
        {
            foreach (CountryConfig.PolicyRow policyRow in (_senderNode.Tag as PolicyTreeListTag).GetPolicyRows())
                policyRow.Type = _newType;
        }
    }

}
