using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Columns;
using EM_UI.TreeListTags;
using EM_UI.DataSets;
using EM_UI.Tools;
using EM_UI.TreeListManagement;

namespace EM_UI.Actions
{
    internal class RenamePolicyAction : BaseAction
    {
        EM_UI_MainForm _mainForm = null;
        TreeListNode _senderNode = null;
        string _countryShortName = string.Empty;
        bool _actionIsCanceled = false;

        internal RenamePolicyAction(EM_UI_MainForm mainForm, TreeListNode senderNode, string countryShortName)
        {
            _mainForm = mainForm; 
            _senderNode = senderNode;
            _countryShortName = countryShortName;
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
            string countryShortName = CountryConfigFacade.GetCountryShortName((_senderNode.Tag as PolicyTreeListTag).GetDefaultPolicyRow()).ToLower();
            string policyName = policyTreeListTag.GetPolicyName();
            string currentName = policyTreeListTag.GetDefaultPolicyRow().Name;
            if (!UserInfoHandler.GetPolicyName(ref policyName, countryShortName, _senderNode.TreeList, currentName))
            {
                _actionIsCanceled = true;
                return;
            }

            foreach (CountryConfig.PolicyRow policyRow in policyTreeListTag.GetPolicyRows()) //loop over systems (actually over neighbour policies within systems)
            {
                policyRow.Name = policyName;
                _senderNode.SetValue(_mainForm.GetTreeListBuilder().GetSystemColumnByID(policyRow.SystemID), policyRow.Switch);
            }
            _senderNode.SetValue(_mainForm.GetTreeListBuilder().GetPolicyColumn(), policyName);
        }       
    }
}
