using EM_Common;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class VariablesRedoAction : VariablesBaseAction
    {
        ADOUndoManager _undoManager = null;

        internal VariablesRedoAction(ADOUndoManager undoManager)
        {
            _undoManager = undoManager;
        }

        internal override bool Perform()
        {
            if (_undoManager == null)
                return false;
            
            _undoManager.Redo();
            return true;
        }

        internal override bool UpdateVariables()
        {
            return true;
        }

        internal override bool UpdateAcronyms()
        {
            return true;
        }
    }
}
