using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Nodes.Operations;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System.Collections.Generic;

namespace EM_UI.NodeOperations
{
    internal class UsedComponent
    {
        internal string _policyName = string.Empty;
        internal string _functionName = string.Empty;
        internal string _parameterName = string.Empty;
        internal string _row = string.Empty;
        internal TreeListNode _node = null;
        internal List<string> _systemNames = new List<string>();
        internal object _tag = null;
    }

    internal class CheckComponentUse : TreeListOperation
    {
        string _componentName = string.Empty;
        List<UsedComponent> _usedComponents = null;
        bool _ignoreIfSwitchedOff = true;
        List<string> _systemIDs = null;
        bool _doPatternSearch = false;

        internal CheckComponentUse(string componentName, bool ignoreIfSwitchedOff, List<string> systemIDs, bool doPatternSearch = false)
            : base()
        {
            _componentName = componentName;
            _usedComponents = new List<UsedComponent>();
            _ignoreIfSwitchedOff = ignoreIfSwitchedOff;
            _systemIDs = systemIDs;
            _doPatternSearch = doPatternSearch;
        }

        internal List<UsedComponent> GetUsedComponents()
        {
            return _usedComponents;
        }

        public override void Execute(TreeListNode senderNode)
        {
            if (senderNode.Tag == null || (senderNode.Tag as BaseTreeListTag).GetParameterName() == null)
                return; //policy- or function-node

            ParameterTreeListTag nodeTag = senderNode.Tag as ParameterTreeListTag;

            UsedComponent usedComponent = new UsedComponent();
            foreach (CountryConfig.ParameterRow parameterRow in nodeTag.GetParameterRows())
            {
                if (!_systemIDs.Contains(parameterRow.FunctionRow.PolicyRow.SystemID))
                    continue;

                if (_ignoreIfSwitchedOff && (parameterRow.FunctionRow.Switch.ToLower() == DefPar.Value.OFF ||
                    parameterRow.FunctionRow.PolicyRow.Switch.ToLower() == DefPar.Value.OFF))
                    continue;
                
                //formulas and conditions need a special comparison operation ...
                if (parameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.CONDITION).ToLower() ||
                    parameterRow.ValueType.ToLower() == DefinitionAdmin.ParTypeToString(DefPar.PAR_TYPE.FORMULA).ToLower())
                {
                    if (!_doPatternSearch) { if (!EM_Helpers.DoesFormulaContainComponent(parameterRow.Value, _componentName)) continue; }
                    else { if (!EM_Helpers.DoesValueMatchPattern(_componentName, parameterRow.Value, false, true)) continue; }
                }
                //... for other parameter types it's enough to compare the value
                else
                {
                    if (!_doPatternSearch) { if (parameterRow.Value.ToLower() != _componentName.ToLower()) continue; }
                    else { if (!EM_Helpers.DoesValueMatchPattern(_componentName, parameterRow.Value, false, true)) continue; }
                }

                usedComponent._systemNames.Add(parameterRow.FunctionRow.PolicyRow.SystemRow.Name);
            }

            //take account of the fact that the policy column also may contain components (i.e. variables in DefIL, SetDefault, Uprate, etc.)
            if (nodeTag.IsPolicyColumnEditable())
            {
                string contentPolicyColumn = senderNode.GetDisplayText(senderNode.TreeList.Columns.ColumnByName(TreeListBuilder._policyColumnName));
                if (EM_Helpers.DoesValueMatchPattern(_componentName, contentPolicyColumn, false, false))
                    usedComponent._row = TreeListManager.GetNodeRowNumber(senderNode, senderNode.ParentNode);
            }

            if (usedComponent._systemNames.Count == 0 && usedComponent._row == string.Empty)
                return;

            usedComponent._policyName = nodeTag.GetPolicyName();
            usedComponent._functionName = nodeTag.GetFunctionName();
            if (usedComponent._row == string.Empty)
                usedComponent._parameterName = nodeTag.GetParameterName();
            usedComponent._row = TreeListManager.GetNodeRowNumber(senderNode, senderNode.ParentNode);
            usedComponent._node = senderNode;
            _usedComponents.Add(usedComponent);
        }
    }
}
