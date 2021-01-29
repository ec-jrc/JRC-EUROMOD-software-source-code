using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListTags;

namespace EM_UI.Actions
{
    internal class AddParameterAction : BaseAction
    {
        DefinitionAdmin.Par _parDef = null;
        string _parName = string.Empty;
        TreeListNode _senderNode = null;
        string _parameterGroup = string.Empty;

        internal AddParameterAction(TreeListNode senderNode, string parName, DefinitionAdmin.Par parDef, string parameterGroup)
        {
            _senderNode = senderNode;
            _parName = parName;
            _parDef = parDef;
            _parameterGroup = parameterGroup;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override void PerformAction()
        {
            //reference is function (add parameter as last parameter of this function)
            if ((_senderNode.Tag as BaseTreeListTag).GetDefaultParameterRow() == null)
            {
                foreach (CountryConfig.FunctionRow functionRow in (_senderNode.Tag as FunctionTreeListTag).GetFunctionRows())
                    CountryConfigFacade.AddParameterRowAtTail(functionRow, _parName, _parDef, _parameterGroup);
            }

            //reference is the parameter after which the new parameter should be added
            else
	        {
                foreach (CountryConfig.ParameterRow parameterRow in (_senderNode.Tag as ParameterTreeListTag).GetParameterRows())
                    CountryConfigFacade.AddParameterRow(parameterRow,
                                                        false, //addBeforeNeighbour
                                                        _parName, _parDef, _parameterGroup); 
	        }
        }       
    }
}
