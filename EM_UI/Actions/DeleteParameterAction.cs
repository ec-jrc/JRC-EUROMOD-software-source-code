using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using EM_UI.TreeListManagement;
using EM_UI.Dialogs;

namespace EM_UI.Actions
{
    class DeleteParameterAction :BaseAction
    {
        TreeListNode _senderNode = null;

        internal DeleteParameterAction(TreeListNode senderNode)
        {
            _senderNode = senderNode;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return true;
        }

        internal override void PerformAction()
        {
            foreach (CountryConfig.ParameterRow parameterRow in (_senderNode.Tag as ParameterTreeListTag).GetParameterRows())
                parameterRow.Delete();
            _senderNode.TreeList.DeleteNode(_senderNode);
        }
    }
}
