using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using System.Collections.Generic;

namespace EM_UI.Actions
{
    internal class SetSelectionPrivateStateAction : BaseAction
    {
        List<TreeListNode> _senderNodes = null;

        internal SetSelectionPrivateStateAction(List<TreeListNode> senderNodes)
        {
            _senderNodes = senderNodes;
        }

        internal override void PerformAction()
        {
            foreach (TreeListNode node in _senderNodes)
            {
                PolicyTreeListTag polTag = node.Tag as PolicyTreeListTag;
                FunctionTreeListTag funTag = node.Tag as FunctionTreeListTag;
                ParameterTreeListTag parTag = node.Tag as ParameterTreeListTag;

                if (polTag != null)
                {
                    foreach (CountryConfig.PolicyRow policyRow in polTag.GetPolicyRows())
                        policyRow.Private = policyRow.Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                    node.StateImageIndex = polTag.GetDefaultPolicyRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_POL : DefGeneral.IMAGE_IND_POL;
                }
                else if (funTag != null)
                {
                    foreach (CountryConfig.FunctionRow funRow in funTag.GetFunctionRows())
                        funRow.Private = funRow.Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                    node.StateImageIndex = funTag.GetDefaultFunctionRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_FUN : DefGeneral.IMAGE_IND_FUN;
                }
                else if (parTag != null)
                {
                    foreach (CountryConfig.ParameterRow parRow in parTag.GetParameterRows())
                        parRow.Private = parRow.Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                    node.StateImageIndex = parTag.GetDefaultParameterRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_PAR : -1;
                }
            }
        }       
    }
}
