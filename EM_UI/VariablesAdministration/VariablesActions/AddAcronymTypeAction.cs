using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class AddAcronymTypeAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        TreeList _treeAcronyms = null;
        VarConfigFacade _varConfigFacade = null;

        internal AddAcronymTypeAction(VariablesForm variablesForm)
        {
            _variablesForm = variablesForm;
            _treeAcronyms = _variablesForm.treeAcronyms;
            _varConfigFacade = _variablesForm._varConfigFacade;
        }

        internal override bool Perform()
        {
            TreeListNode node = _treeAcronyms.AppendNode(null, null);
            node.Tag = _varConfigFacade.AddAcronymTypeRow();

            return true;
        }

        internal override bool UpdateFilterCheckboxes()
        {
            return true;
        }
    }
}
