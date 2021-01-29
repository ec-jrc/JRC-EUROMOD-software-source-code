using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraTreeList.Columns;
using EM_UI.TreeListTags;
using EM_UI.DataSets;

namespace EM_UI.Actions
{
    internal class ChangeParameterNameAction : BaseAction
    {
        TreeListNode _senderNode = null;
        string _newName = string.Empty;
        bool _changeInTree = false;
        TreeListColumn _policyColumn = null;

        internal ChangeParameterNameAction(TreeListNode senderNode, string newName, bool changeInTree = false, TreeListColumn policyColumn = null)
        {
            _senderNode = senderNode;
            _newName = newName;
            _changeInTree = changeInTree;
            _policyColumn = policyColumn;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return true;
        }

        internal override void PerformAction()
        {
            foreach (CountryConfig.ParameterRow parameterRow in (_senderNode.Tag as ParameterTreeListTag).GetParameterRows())
                parameterRow.Name = _newName;
            if (_changeInTree)
                _senderNode.SetValue(_policyColumn, _newName);
        }       
    }
}
