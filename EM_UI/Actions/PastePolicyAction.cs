using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;
using EM_UI.DataSets;
using EM_UI.Tools;

namespace EM_UI.Actions
{
    internal class PastePolicyAction : BaseAction
    {
        //TreeListNode _copyNode = null;
        Dictionary<string, CountryConfig.PolicyRow> _copyPolicyRows = null;
        TreeListNode _pasteNode = null;
        EM_UI_MainForm _copyCountryForm = null;
        EM_UI_MainForm _pasteCountryForm = null;
        bool _pasteBefore = false;
        bool _pasteAsReference = false;
        List<string> _hiddenSystems = null;
        bool _actionIsCanceled = false;

        internal PastePolicyAction(TreeListNode copyNode, EM_UI_MainForm copyCountryForm)
        {
            PolicyTreeListTag tag = (copyNode.Tag != null) ? (copyNode.Tag as PolicyTreeListTag) : null;
            if (tag != null)
            {
                _copyPolicyRows = new Dictionary<string, CountryConfig.PolicyRow>();
                foreach (CountryConfig.PolicyRow policyRow in tag.GetPolicyRows())
                    _copyPolicyRows.Add(policyRow.SystemRow.ID, policyRow);
            }
            _copyCountryForm = copyCountryForm;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal void SetPasteInfo(TreeListNode pasteNode, EM_UI_MainForm pasteCountryForm, bool pasteBefore, List<string> hiddenSystems, bool pasteAsReference)
        {
            _pasteCountryForm = pasteCountryForm;
            _pasteNode = pasteNode;
            _pasteBefore = pasteBefore;
            _hiddenSystems = hiddenSystems;
            _pasteAsReference = pasteAsReference;
        }

        internal string GetCopyCountryShortName()
        {
            return _copyCountryForm.GetCountryShortName();
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal override void PerformAction()
        {
            try
            {
                string policyName = _copyPolicyRows.Values.First().Name; //name of orgin policy

                string pasteCountryShortName = CountryConfigFacade.GetCountryShortName((_pasteNode.Tag as PolicyTreeListTag).GetDefaultPolicyRow()).ToLower(); //name of destination country
                if (!_pasteAsReference)
                {
                    if (!UserInfoHandler.GetPolicyName(ref policyName, pasteCountryShortName, _pasteNode.TreeList)) //note: uses treelist of destination country
                    {
                        _actionIsCanceled = true;
                        return;
                    }
                }

                CountryConfig.PolicyRow pastedPolicyRow = null;
                //copy policy within country
                if (_copyCountryForm == _pasteCountryForm)
                {
                    foreach (CountryConfig.PolicyRow pastePolicyRow in (_pasteNode.Tag as PolicyTreeListTag).GetPolicyRows()) //loop over systems
                    {
                        CountryConfig.PolicyRow policyRow = _copyPolicyRows[pastePolicyRow.SystemID];
                        bool switchNA = _hiddenSystems.Contains(pastePolicyRow.SystemID) ? true : false;
                        if (!_pasteAsReference)
                            pastedPolicyRow = CountryConfigFacade.CopyPolicyRow(policyRow, policyName, pastePolicyRow, _pasteBefore, switchNA);
                        else
                            pastedPolicyRow = CountryConfigFacade.AddReferencePolicyRow(policyRow, pastePolicyRow, _pasteBefore, switchNA);
                    }
                }

                //copy policy from one country to another
                else
                {
                    //link systems of origin country to systems of destination country
                    Dictionary<string, string> systemAssignment = UserInfoHandler.GetSystemAssignement(_copyCountryForm, _pasteCountryForm, _hiddenSystems);
                    if (systemAssignment == null)
                    {
                        _actionIsCanceled = true;
                        return;
                    }

                    foreach (CountryConfig.PolicyRow pastePolicyRow in (_pasteNode.Tag as PolicyTreeListTag).GetPolicyRows()) //loop over systems
                    {
                        //search for 'corresponding' policy, i.e. policy within the system of origin country assigned to current system (of loop)
                        CountryConfig.PolicyRow policyRow = null;
                        bool switchNA = !systemAssignment.Keys.Contains(pastePolicyRow.SystemID);
                        if (!switchNA) //policy found
                            policyRow = _copyPolicyRows[systemAssignment[pastePolicyRow.SystemID]];
                        else //policy not found because (a) system is not assigend to any system in origin country or (b) system is hidden
                            policyRow = _copyPolicyRows.Values.First(); //still copy (default) policy for symmetry but switch off and set paramters to n/a
                        pastedPolicyRow = CountryConfigFacade.CopyPolicyRow(policyRow, policyName, pastePolicyRow, _pasteBefore, switchNA);
                    }
                }

                if (pastedPolicyRow != null)
                {
                    _pasteCountryForm.GetTreeListBuilder().InsertPolicyNode(pastedPolicyRow,
                            _pasteBefore ? _pasteCountryForm.treeList.GetNodeIndex(_pasteNode) : _pasteCountryForm.treeList.GetNodeIndex(_pasteNode) + 1);
                    _pasteCountryForm.GetTreeListBuilder().AddToAvailablePolicies(pastedPolicyRow);
                }
            }
            catch
            {
                Tools.UserInfoHandler.ShowError("Copied policy (or other necessary information) is not available any more." + Environment.NewLine
                                                 + "Please repeat copying.");
                EM_AppContext.Instance.SetPastePolicyAction(null);
                _actionIsCanceled = true;
            }
        }
    }
}
