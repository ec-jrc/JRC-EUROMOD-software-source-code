using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.TreeListTags;
using System.Collections.Generic;

namespace EM_UI.Actions
{
    internal class AddFunctionAction : BaseAction
    {
        EM_UI_MainForm _mainForm = null;
        TreeListNode _senderNode = null;
        string _functionName = string.Empty;
        List<string> _addedFunctionsIDs = new List<string>();

        internal AddFunctionAction(EM_UI_MainForm mainForm, TreeListNode senderNode, string functionName, List<string> hiddenSystemstems)
        {
            _mainForm = mainForm;
            _senderNode = senderNode;
            _functionName = functionName;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal List<string> GetAddedFunctionsIDs()
        {
            return _addedFunctionsIDs;
        }

        internal override void PerformAction()
        {
            TreeListNode policyNode = _senderNode;
            if (_senderNode.ParentNode != null)
                policyNode = _senderNode.ParentNode;
            
            int newFunctionNodeIndex = -1;
            List<CountryConfig.FunctionRow> newFunctionRows = new List<CountryConfig.FunctionRow>();

            foreach (CountryConfig.PolicyRow policyRow in (policyNode.Tag as PolicyTreeListTag).GetPolicyRows()) //loop over systems
            {
                CountryConfig.FunctionRow newFunctionRow = null;

                //(1) add function
                //if the sender node is a policy node: insert as last function ('Add Function' was called from policy node or 'Add Function After' was called from last function node in policy)
                if (_senderNode == policyNode)
                {
                    newFunctionRow = CountryConfigFacade.AddFunctionRowAtTail(_functionName, policyRow,
                        // the switch of a function inside an extension-policy cannot be changed, therefore add as on (otherwise one would need to remove the policy from the extension to be able to change the switch to on)
                        ExtensionAndGroupManager.ShowExtensionSwitchEditor(policyRow) ? DefPar.Value.ON : DefPar.Value.NA);
                }

                //if the sender node is a function node: insert before ('Add Function Before' was called from function node)
                else
                {
                    CountryConfig.FunctionRow neighbourFunction = (_senderNode.Tag as FunctionTreeListTag).GetFunctionRowOfSystem(policyRow.SystemRow.ID);
                    newFunctionRow = CountryConfigFacade.AddFunctionRow(_functionName, neighbourFunction, true,
                        ExtensionAndGroupManager.ShowExtensionSwitchEditor(policyRow) ? DefPar.Value.ON : DefPar.Value.NA); // see comment above
                    newFunctionNodeIndex = _mainForm.treeList.GetNodeIndex(_senderNode);
                }
                _addedFunctionsIDs.Add(newFunctionRow.ID);

                //(2) add compulsory parameters
                DefinitionAdmin.Fun fun = DefinitionAdmin.GetFunDefinition(_functionName, false);
                if (fun != null)
                {
                    for (short doCommon = 0; doCommon < 2; ++doCommon) // add function-specific parameters before common
                    {
                        foreach (var p in fun.GetParList()) AddPar(p, false);
                        foreach (var pg in fun.GetGroupParList())
                            if (pg.minCount > 0) foreach (var p in pg.par) AddPar(p, true);

                        void AddPar(KeyValuePair<string, DefinitionAdmin.Par> p, bool group)
                        {
                            string parName = p.Key.ToUpper() == DefPar.PAR_TYPE.PLACEHOLDER.ToString().ToUpper() ? DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.PLACEHOLDER) : p.Key;
                            DefinitionAdmin.Par parDef = p.Value;
                            if (parDef.minCount > 0 && ((doCommon == 0 && !parDef.isCommon) || (doCommon == 1 && parDef.isCommon)))
                                CountryConfigFacade.AddParameterRowAtTail(newFunctionRow, parName, parDef, group ? "1" : string.Empty);
                        }
                    }
                }
                newFunctionRows.Add(newFunctionRow);
            }

            _mainForm.GetTreeListBuilder().InsertFunctionNode(newFunctionRows, policyNode, newFunctionNodeIndex);
        }
    }
}
