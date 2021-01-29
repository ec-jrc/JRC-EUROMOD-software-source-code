using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;

namespace EM_UI.Actions
{
    internal class ChangeParameterValueAction : BaseAction
    {
        TreeListNode _senderNode = null;
        TreeListColumn _senderColumn = null;
        string _newValue = null;
        bool _changeInTree = false;

        internal ChangeParameterValueAction(TreeListNode senderNode, TreeListColumn senderColumn, string newValue, bool changeInTree = false)
        {
            _senderNode = senderNode;
            _senderColumn = senderColumn;
            _newValue = newValue;
            _changeInTree = changeInTree;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }
        
        internal override void PerformAction()
        {
            if (_newValue == string.Empty || _changeInTree)
            {
                if (_newValue == string.Empty)
                    _newValue = DefPar.Value.NA;  //developer's wish: do not leave cell empty, but set to n/a instead
                _senderNode.SetValue(_senderColumn, _newValue);
            }
            (_senderNode.Tag as BaseTreeListTag).StoreChangedValue(_newValue, (_senderColumn.Tag as SystemTreeListTag).GetSystemRow());

            //enforce updating of the info provided by combo-boxes and Intelli-sense if Def-functions are changed
            TreeListManager.UpdateIntelliAndTUBoxInfo((_senderNode.Tag as BaseTreeListTag).GetFunctionName(), _senderColumn);
        }
    }

}
