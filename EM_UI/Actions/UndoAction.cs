using EM_Common;

namespace EM_UI.Actions
{
    internal class UndoAction : BaseAction
    {
        ADOUndoManager _undoManager = null;

        public UndoAction(ADOUndoManager undoManager)
        {
            _undoManager = undoManager;
        }

        internal override bool ShowHiddenSystemsWarning()
        {
            return false;
        }
        
        internal override void PerformAction()
        {
            if (_undoManager != null) 
                _undoManager.Undo();
        }
    }
}
