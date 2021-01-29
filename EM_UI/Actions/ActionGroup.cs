using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EM_UI.Actions
{
    internal class ActionGroup : BaseAction
    {
        List<BaseAction> baseActions = null;

        internal ActionGroup()
        {
            baseActions = new List<BaseAction>();
            baseActions.Clear();
        }

        internal override void PerformAction()
        {
            foreach (BaseAction baseAction in baseActions)
                baseAction.PerformAction();
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            foreach (BaseAction baseAction in baseActions)
                if (baseAction.ShowHiddenSystemsWarning() == true)
                    return true;
            return false;
        }

        internal override bool ClearMultiSelector()
        {
            foreach (BaseAction baseAction in baseActions)
                if (baseAction.ClearMultiSelector() == true)
                    return true;
            return false;
        }

        internal void AddAction(BaseAction baseAction)
        {
            baseActions.Add(baseAction);
        }

        internal void InsertAction(int index, BaseAction baseAction)
        {
            if (index >= 0 && index < baseActions.Count)
                baseActions.Insert(index, baseAction);
            else if (index < 0)
                baseActions.Insert(0, baseAction);
            else //index >= baseActions.Count
                baseActions.Add(baseAction);
        }

        internal int GetActionsCount()
        {
            return baseActions.Count;
        }
    }

}
