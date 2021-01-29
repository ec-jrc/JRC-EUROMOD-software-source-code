using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using EM_UI.TreeListManagement;

namespace EM_UI.Actions
{
    class SpreadParameterValueAction :BaseAction
    {
        EM_UI_MainForm _mainForm = null;
        bool _actionIsCanceled = false;

        internal SpreadParameterValueAction(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }

        internal override bool ActionIsCanceled()
        {
            return _actionIsCanceled;
        }

        internal override void PerformAction()
        {
            TreeListColumn columnToCopy = _mainForm.GetMultiCellSelector().GetColumnIfExactlyOneSelected();
            if (columnToCopy == null)
            {
                _actionIsCanceled = true;
                return;
            }

            foreach (TreeListNode selectedNode in _mainForm.GetMultiCellSelector().GetSelectedNodes(true, true))
            {
                foreach (TreeListColumn column in _mainForm.treeList.Columns)
                {
                    if (column.Tag != null &&
                        !_mainForm.GetTreeListManager().GetIDsOfHiddenSystems().Contains((column.Tag as SystemTreeListTag).GetSystemRow().ID)) 
                    {
                        string value = selectedNode.GetValue((columnToCopy.Tag as SystemTreeListTag).GetSystemRow().Name).ToString();
                        (selectedNode.Tag as BaseTreeListTag).StoreChangedValue(value, (column.Tag as SystemTreeListTag).GetSystemRow());
                        selectedNode.SetValue(column, value);
                        
                        //enforce updating of the info provided by combo-boxes and Intelli-sense if Def-functions are changed
                        TreeListManager.UpdateIntelliAndTUBoxInfo((selectedNode.Tag as BaseTreeListTag).GetFunctionName(), column);
                    }
                }
            }
        }
    }
}
