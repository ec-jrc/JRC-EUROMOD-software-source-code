using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using System;

namespace EM_UI.Actions
{
    internal class DeletePolicyAction : BaseAction
    {
        TreeListNode _senderNode = null;
        bool _actionIsCanceled = false;

        internal DeletePolicyAction(TreeListNode senderNode)
        {
            _senderNode = senderNode;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return true;
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal override void PerformAction()
        {
            PolicyTreeListTag policyTreeListTag = _senderNode.Tag as PolicyTreeListTag;
            if (CountryConfigFacade.IsReferencePolicy(policyTreeListTag.GetDefaultPolicyRow()))
            {
                Tools.UserInfoHandler.ShowInfo("The policy cannot be deleted as it is used by a(t least one) reference policy." + Environment.NewLine +
                                               "Please delete the reference policies before deleting the policy itself.");
                _actionIsCanceled = true;
                return;
            }
            
            _senderNode.TreeList.DeleteNode(_senderNode);

            foreach (CountryConfig.PolicyRow policyRow in policyTreeListTag.GetPolicyRows())
            {
                policyRow.Delete();
                EM_AppContext.Instance.GetActiveCountryMainForm().GetTreeListBuilder().RemoveFromAvailablePolicies(policyRow);
            }
        }
    }
}
