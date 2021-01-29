using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Columns;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System.Windows.Forms;

namespace EM_UI.Actions
{
    internal class SaveSystemOrderAction : BaseAction
    {
        EM_UI_MainForm _mainForm = null;
        bool _actionIsCanceled = false;

        internal SaveSystemOrderAction(EM_UI_MainForm mainForm)
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
            if (Tools.UserInfoHandler.GetInfo("Are you sure you want to use the current system order as default?",  MessageBoxButtons.YesNo) == DialogResult.No)
            {
                _actionIsCanceled = true;
                return;
            }

            foreach (TreeListColumn systemColumn in _mainForm.GetTreeListBuilder().GetSystemColums())
            {
                SystemTreeListTag systemTreeListTag = (systemColumn.Tag as SystemTreeListTag);
                systemTreeListTag.SetSystemOrder(systemColumn.VisibleIndex);
            }
        }
    }
}
