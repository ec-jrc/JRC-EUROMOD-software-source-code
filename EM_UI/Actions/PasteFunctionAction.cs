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
    internal class PasteFunctionAction : BaseAction
    {
        List<Dictionary<string, CountryConfig.FunctionRow>> _copyFunctions = null;
        TreeListNode _pasteNode = null;
        EM_UI_MainForm _copyCountryForm = null;
        EM_UI_MainForm _pasteCountryForm = null;
        bool _pasteBefore = false;
        List<string> _hiddenSystems = null;
        bool _actionIsCanceled = false;

        internal PasteFunctionAction(List<TreeListNode> copyNodes, EM_UI_MainForm copyCountryForm)
        {
            _copyFunctions = new List<Dictionary<string, CountryConfig.FunctionRow>>();
            foreach (TreeListNode copyNode in copyNodes)
                _copyFunctions.Add((copyNode.Tag as FunctionTreeListTag).GetFunctionDictionary());
            _copyCountryForm = copyCountryForm;
        }

        internal void SetPasteInfo(TreeListNode pasteNode, EM_UI_MainForm pasteCountryForm, bool pasteBefore, List<string> hiddenSystems)
        {
            _pasteCountryForm = pasteCountryForm;
            _pasteNode = pasteNode;
            _pasteBefore = pasteBefore;
            _hiddenSystems = hiddenSystems;
        }

        internal string GetCopyCountryShortName()
        {
            return _copyCountryForm.GetCountryShortName();
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
            try
            {
                TreeListNode pastePolicyNode = _pasteNode.ParentNode;
                if (pastePolicyNode == null)
                    pastePolicyNode = _pasteNode; //'Paste Function' called from policy node

                List<Dictionary<string, CountryConfig.FunctionRow>> copyFunctions = new List<Dictionary<string, CountryConfig.FunctionRow>>();
                foreach (Dictionary<string, CountryConfig.FunctionRow> copyFunction in _copyFunctions)
                    copyFunctions.Add(copyFunction); //make a copy as possible reversing should not be permanent (there may be only one copy action, but several paste actions)
                if (_pasteBefore == false &&
                    pastePolicyNode != _pasteNode)  //do not reverse if functions are inserted as last (i.e. from the policy-context-menu)
                    copyFunctions.Reverse(); //by reversing the order one spares changing the paste node

                //link systems of origin country to systems of destination country
                Dictionary<string, string> systemAssignment = null;
                if (_copyCountryForm != _pasteCountryForm)
                {
                    systemAssignment = UserInfoHandler.GetSystemAssignement(_copyCountryForm, _pasteCountryForm, _hiddenSystems);
                    if (systemAssignment == null)
                    {
                        _actionIsCanceled = true;
                        return;
                    }
                }

                foreach (Dictionary<string, CountryConfig.FunctionRow> copyFunction in copyFunctions)
                {
                    List<CountryConfig.FunctionRow> pastedFunctionRows = null;
                    int pastedFunctionNodeIndex = -1;

                    //copy function within country
                    if (_copyCountryForm == _pasteCountryForm)
                    {
                        List<string> systemIDs = new List<string>();
                        foreach (CountryConfig.PolicyRow policyRow in (pastePolicyNode.Tag as PolicyTreeListTag).GetPolicyRows()) //gather systems
                            systemIDs.Add(policyRow.SystemID);

                        pastedFunctionRows = PasteFunction(copyFunction, systemIDs, pastePolicyNode, out pastedFunctionNodeIndex);
                    }

                    //copy function from one country to another
                    else
                    {
                        List<string> copySystemIDs = new List<string>();
                        List<string> pasteSystemIDs = new List<string>();
                        foreach (CountryConfig.PolicyRow policyRow in (pastePolicyNode.Tag as PolicyTreeListTag).GetPolicyRows()) //gather systems
                        {
                            copySystemIDs.Add(systemAssignment.ContainsKey(policyRow.SystemID) ? systemAssignment[policyRow.SystemID] : null);
                            pasteSystemIDs.Add(policyRow.SystemID);
                        }

                        pastedFunctionRows = PasteFunction(copyFunction, copySystemIDs, pastePolicyNode, out pastedFunctionNodeIndex, pasteSystemIDs);
                    }

                    if (pastedFunctionRows != null)
                        _pasteCountryForm.GetTreeListBuilder().InsertFunctionNode(pastedFunctionRows, pastePolicyNode, pastedFunctionNodeIndex);
                }
            }
            catch (System.Collections.Generic.KeyNotFoundException)
            {
                Tools.UserInfoHandler.ShowError("Copied function(s) not available any more (due to add/remove systems).");
                EM_AppContext.Instance.SetPasteFunctionAction(null);
                _actionIsCanceled = true;
            }
            catch (System.Data.RowNotInTableException)
            {
                Tools.UserInfoHandler.ShowError("Copied function(s) not available any more (due to removal).");
                EM_AppContext.Instance.SetPasteFunctionAction(null);
                _actionIsCanceled = true;
            }
        }

        List<CountryConfig.FunctionRow> PasteFunction(Dictionary<string, CountryConfig.FunctionRow> copyFunction,
                    List<string> copySystemIDs, TreeListNode pastePolicyNode, out int pastedFunctionNodeIndex, List<string> pasteSystemIDs = null)
        {
            List<CountryConfig.FunctionRow> pastedFunctionRows = new List<CountryConfig.FunctionRow>();
            pastedFunctionNodeIndex = -1;

            if (pasteSystemIDs == null)
                pasteSystemIDs = copySystemIDs; //copy within country/add-on (i.e. not to another country/add-on)

            for (int index = 0; index < pasteSystemIDs.Count; ++index) //loop over systems
            {
                string pasteSystemID = pasteSystemIDs.ElementAt(index);
                string copySystemID = copySystemIDs.ElementAt(index);

                bool switchNA = copySystemID == null || //set to n/a if no "twin"-system was assigned in copying the function from one country to another
                                _hiddenSystems.Contains(pasteSystemID); //or destination system is hidden

                CountryConfig.FunctionRow copyFunctionInSystem = null;
                if (copySystemID != null)
                    copyFunctionInSystem = copyFunction[copySystemID];
                else //no "twin"-system was assigned in copying the function from one country to another: still need to copy (default) function for symmetry
                    copyFunctionInSystem = copyFunction.Values.ElementAt(0);

                //'Paste Function Before/After' called from function node
                if (pastePolicyNode != _pasteNode)
                {
                    CountryConfig.FunctionRow pasteFunctionRow = (_pasteNode.Tag as FunctionTreeListTag).GetFunctionRowOfSystem(pasteSystemID);
                    pastedFunctionRows.Add(CountryConfigFacade.CopyFunctionRow(copyFunctionInSystem, pasteFunctionRow, _pasteBefore, switchNA));
                    pastedFunctionNodeIndex = _pasteBefore ? _pasteCountryForm.treeList.GetNodeIndex(_pasteNode) : _pasteCountryForm.treeList.GetNodeIndex(_pasteNode) + 1;
                }

                //'Paste Function' called from policy node (paste as last function)
                else
                {
                    pastedFunctionRows.Add(CountryConfigFacade.CopyFunctionRowAtTailOrUseOriginalOrder(copyFunctionInSystem,
                                                        (pastePolicyNode.Tag as PolicyTreeListTag).GetPolicyRowOfSystem(pasteSystemID),
                                                        true, //copyToEnd
                                                        switchNA));
                }
            }
            return pastedFunctionRows;
        }
    }
}
