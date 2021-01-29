using EM_Common;

namespace EM_UI.Actions
{
    internal class RedoAction : BaseAction
    {
        ADOUndoManager _undoManager = null;

        public RedoAction(ADOUndoManager undoManager)
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
                _undoManager.Redo();
        }
    }
}
