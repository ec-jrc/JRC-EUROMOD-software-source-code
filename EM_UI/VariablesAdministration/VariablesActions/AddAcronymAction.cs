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
    internal class AddAcronymAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;

        internal AddAcronymAction(VariablesForm variablesForm)
        {
            _variablesForm = variablesForm;
            _treeAcronyms = _variablesForm.treeAcronyms;
            _varConfigFacade = _variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            if (_treeAcronyms.FocusedNode == null || AcronymManager.IsTypeNode(_treeAcronyms.FocusedNode))
                return false;

            TreeListNode parentNode = null;

            //assess at which level acronym should be inserted with respect to which node is focused
            if (AcronymManager.IsLevelNode(_treeAcronyms.FocusedNode))
                parentNode = _treeAcronyms.FocusedNode;
            else if (AcronymManager.IsAcronymNode(_treeAcronyms.FocusedNode))
                parentNode = _treeAcronyms.FocusedNode.ParentNode;

            //append the new node
            TreeListNode node = _treeAcronyms.AppendNode(null, parentNode);
            node.Tag = _varConfigFacade.AddAcronymRow(parentNode.Tag as VarConfig.AcronymLevelRow);

            parentNode.Expanded = true;
            parentNode.ParentNode.Expanded = true;

            return true;
        }
    }
}
