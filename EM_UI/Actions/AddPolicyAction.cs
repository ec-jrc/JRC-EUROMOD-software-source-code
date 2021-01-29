using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using System.Collections.Generic;

namespace EM_UI.Actions
{
    internal class AddPolicyAction : BaseAction
    {
        EM_UI_MainForm _mainForm = null;
        CountryConfigFacade _countryConfigFacade = null;
        TreeListNode _senderNode = null;
        string _policyType = string.Empty;
        bool _addBeforeSender = true;
        bool _actionIsCanceled = false;

        //add very first policy
        internal AddPolicyAction(EM_UI_MainForm mainForm, CountryConfigFacade countryConfigFacade)
        {
            _mainForm = mainForm;
            _countryConfigFacade = countryConfigFacade;
            _policyType = "DEF";
        }

        //add policy after/before an existing policy
        internal AddPolicyAction(EM_UI_MainForm mainForm, TreeListNode senderNode, string polType, bool addBeforeSender, List<string> hiddenSystems)
        {
            _mainForm = mainForm;
            _senderNode = senderNode;
            _policyType = polType.Substring(0, 3).ToLower();
            _addBeforeSender = addBeforeSender;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal override void PerformAction()
        {
            string countryShortName = string.Empty;
            string policyName = string.Empty;

            CountryConfig.PolicyRow newPolicyRow = null;
            int newPolicyNodeIndex = -1;

            //add very first policy
            if (_senderNode == null)
            {
                countryShortName = _countryConfigFacade.GetCountryShortName().ToLower();
                if (!UserInfoHandler.GetPolicyName(ref policyName, countryShortName, null))
                {
                    _actionIsCanceled = true;
                    return;
                }

                foreach (CountryConfig.SystemRow systemRow in _countryConfigFacade.GetSystemRows()) //loop over systems
                {
                    newPolicyRow = _countryConfigFacade.AddFirstPolicyRow(policyName, _policyType, systemRow,
                                                            DefPar.Value.NA); //policy is initially set to n/a
                }
            }

            //add policy after/before an existing policy
            else
            {
                countryShortName = CountryConfigFacade.GetCountryShortName((_senderNode.Tag as PolicyTreeListTag).GetDefaultPolicyRow()).ToLower();
                if (!UserInfoHandler.GetPolicyName(ref policyName, countryShortName, _senderNode.TreeList))
                    return;

                foreach (CountryConfig.PolicyRow policyRow in (_senderNode.Tag as PolicyTreeListTag).GetPolicyRows()) //loop over systems (actually over neighbour policies within systems)
                {
                    newPolicyRow = CountryConfigFacade.AddPolicyRow(policyName, _policyType, policyRow, _addBeforeSender,
                                                    DefPar.Value.NA); //policy is initially switched off
                }

                newPolicyNodeIndex = _addBeforeSender ? _mainForm.treeList.GetNodeIndex(_senderNode) : _mainForm.treeList.GetNodeIndex(_senderNode) + 1;
            }

            if (newPolicyRow != null)
            {
                _mainForm.GetTreeListBuilder().InsertPolicyNode(newPolicyRow, newPolicyNodeIndex);
                _mainForm.GetTreeListBuilder().AddToAvailablePolicies(newPolicyRow);
            }
        }

    }
}
