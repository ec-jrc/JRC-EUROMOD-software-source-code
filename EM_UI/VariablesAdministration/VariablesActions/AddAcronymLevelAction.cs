using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.VariablesAdministration.VariablesManagement;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class AddAcronymLevelAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;

        internal AddAcronymLevelAction(VariablesForm variablesForm)
        {
            _variablesForm = variablesForm;
            _treeAcronyms = _variablesForm.treeAcronyms;
            _varConfigFacade = _variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            if (_treeAcronyms.FocusedNode == null)
                return false;

            TreeListNode addAfterNode = null;
            TreeListNode parentNode = null;
            
            //assess where level should be inserted with respect to which node is focused ...
            if (AcronymManager.IsTypeNode(_treeAcronyms.FocusedNode))
                parentNode = _treeAcronyms.FocusedNode; //... type node: insert as first level

            else if (AcronymManager.IsLevelNode(_treeAcronyms.FocusedNode))
            {
                addAfterNode = _treeAcronyms.FocusedNode; //... (other) level node: insert after this level
                parentNode = addAfterNode.ParentNode;
            }

            else if (AcronymManager.IsAcronymNode(_treeAcronyms.FocusedNode))
            {
                addAfterNode = _treeAcronyms.FocusedNode.ParentNode; //... acronym node: insert after the level of the acronym
                parentNode = addAfterNode.ParentNode;
            }

            //first append the new node ...
            TreeListNode node = _treeAcronyms.AppendNode(null, parentNode);
            VarConfig.AcronymLevelRow addAfterRow = null;
            if (addAfterNode != null)
                addAfterRow = addAfterNode.Tag as VarConfig.AcronymLevelRow;
            node.Tag = _varConfigFacade.AddAcronymLevelRow(parentNode.Tag as VarConfig.AcronymTypeRow, addAfterRow);
            
            //... then move ...
            int pos = -1;
            if (addAfterNode == null)
                pos = 0; //... at front, if type node was selected
            else if (addAfterNode.NextNode != null) //... after the focused node, if level- or acronym-node selected
                pos = addAfterNode.ParentNode.Nodes.IndexOf(addAfterNode.NextNode);

            if (pos >= 0)
                _treeAcronyms.SetNodeIndex(node, pos);

            parentNode.Expanded = true;

            return true;
        }
    }
}
