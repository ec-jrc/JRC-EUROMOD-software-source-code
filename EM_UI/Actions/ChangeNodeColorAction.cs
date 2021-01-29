using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;

namespace EM_UI.Actions
{
    internal class ChangeNodeColorAction : BaseAction
    {
        int _argbColor = DefPar.Value.NO_COLOR;
        MultiCellSelector _multiCellSelector = null;

        internal ChangeNodeColorAction(int argbColor, MultiCellSelector multiCellSelector)
        {
            _argbColor = argbColor;
            _multiCellSelector = multiCellSelector;
        }

        internal override void PerformAction()
        {
            foreach (TreeListNode node in _multiCellSelector.GetSelectedNodes(true))
                (node.Tag as BaseTreeListTag).SetSpecialNodeColor(_argbColor);
        }
    }
}
