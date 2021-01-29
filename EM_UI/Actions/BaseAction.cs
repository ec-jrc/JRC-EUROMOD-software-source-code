namespace EM_UI.Actions
{
    internal class BaseAction
    {
        virtual internal void PerformAction() { }
        virtual internal bool ShowHiddenSystemsWarning() { return false; }
        virtual internal bool ClearMultiSelector() { return true; }
        virtual internal bool ActionIsCanceled() { return false; }
        virtual internal void DoAfterCommitWork() { }

        //allows for dummy-action to just update the treeview
        bool _isNoCommitAction = false;
        virtual internal void SetNoCommitAction() { _isNoCommitAction = true; }
        virtual internal bool IsNoCommitAction() { return _isNoCommitAction; } 
    }
    
}
