using EM_Common;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class VariablesUndoAction : VariablesBaseAction
    {
        ADOUndoManager _undoManager = null;

        internal VariablesUndoAction(ADOUndoManager undoManager)
        {
            _undoManager = undoManager;
        }

        internal override bool Perform()
        {
            if (_undoManager == null)
                return false;

            _undoManager.Undo();
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
