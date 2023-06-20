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

                if (node.StateImageIndex != DefGeneral.IMAGE_IND_INCONS) //It is not the node affected by an inconsistency
                {
                    if (polTag != null)
                    {
                        foreach (CountryConfig.PolicyRow policyRow in polTag.GetPolicyRows())
                            policyRow.Private = policyRow.Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                        if(node.StateImageIndex == DefGeneral.IMAGE_IND_POL_INCONS || node.StateImageIndex == DefGeneral.IMAGE_IND_PRIV_POL_INCONS)
                            node.StateImageIndex = polTag.GetDefaultPolicyRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_POL_INCONS : DefGeneral.IMAGE_IND_POL_INCONS;
                        else
                            node.StateImageIndex = polTag.GetDefaultPolicyRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_POL : DefGeneral.IMAGE_IND_POL;
                    }
                    else if (funTag != null)
                    {
                        foreach (CountryConfig.FunctionRow funRow in funTag.GetFunctionRows())
                            funRow.Private = funRow.Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;

                        if (node.StateImageIndex == DefGeneral.IMAGE_IND_FUN_INCONS || node.StateImageIndex == DefGeneral.IMAGE_IND_PRIV_FUN_INCONS)
                            node.StateImageIndex = polTag.GetDefaultPolicyRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_FUN_INCONS : DefGeneral.IMAGE_IND_FUN_INCONS;
                        else
                            node.StateImageIndex = funTag.GetDefaultFunctionRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_FUN : DefGeneral.IMAGE_IND_FUN;
                    }
                    else if (parTag != null)
                    {
                        foreach (CountryConfig.ParameterRow parRow in parTag.GetParameterRows())
                            parRow.Private = parRow.Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                        node.StateImageIndex = parTag.GetDefaultParameterRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_PAR : -1;
                    }
                }
                else //The node is affected by an inconsistency
                {
                    if (polTag != null)
                    {
                        string oppositeDefaultValue = polTag.GetDefaultPolicyRow().Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                        foreach (CountryConfig.PolicyRow policyRow in polTag.GetPolicyRows())
                            policyRow.Private = oppositeDefaultValue;
                        node.StateImageIndex = polTag.GetDefaultPolicyRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_POL : DefGeneral.IMAGE_IND_POL;
                    }
                    else if (funTag != null)
                    {
                        string oppositeDefaultValue = funTag.GetDefaultFunctionRow().Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                        foreach (CountryConfig.FunctionRow funRow in funTag.GetFunctionRows())
                            funRow.Private = oppositeDefaultValue;
                        node.StateImageIndex = funTag.GetDefaultFunctionRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_FUN : DefGeneral.IMAGE_IND_FUN;
                        //Policy also needs to be signaled as consistent now
                        if (node.ParentNode != null) node.ParentNode.StateImageIndex = node.ParentNode.StateImageIndex == DefGeneral.IMAGE_IND_POL_INCONS ? DefGeneral.IMAGE_IND_POL : DefGeneral.IMAGE_IND_PRIV_POL;
                    }
                    else if (parTag != null)
                    {
                        string oppositeDefaultValue = parTag.GetDefaultParameterRow().Private == DefPar.Value.YES ? DefPar.Value.NO : DefPar.Value.YES;
                        foreach (CountryConfig.ParameterRow parRow in parTag.GetParameterRows())
                            parRow.Private = oppositeDefaultValue;
                        node.StateImageIndex = parTag.GetDefaultParameterRow().Private == DefPar.Value.YES ? DefGeneral.IMAGE_IND_PRIV_PAR : -1;
                        //Function and Policy also needs to be signaled as consistent now
                        if (node.ParentNode != null) node.ParentNode.StateImageIndex = node.ParentNode.StateImageIndex == DefGeneral.IMAGE_IND_FUN_INCONS ? DefGeneral.IMAGE_IND_FUN : DefGeneral.IMAGE_IND_PRIV_FUN;
                        if (node.ParentNode.ParentNode != null) node.ParentNode.ParentNode.StateImageIndex = node.ParentNode.ParentNode.StateImageIndex == DefGeneral.IMAGE_IND_POL_INCONS ? DefGeneral.IMAGE_IND_POL : DefGeneral.IMAGE_IND_PRIV_POL;

                    }
                }
            }
        }       
    }
}
