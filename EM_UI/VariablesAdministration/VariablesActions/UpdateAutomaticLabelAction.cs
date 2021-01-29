using System.Windows.Forms;

namespace EM_UI.VariablesAdministration.VariablesActions
{
    internal class UpdateAutomaticLabelAction : VariablesBaseAction
    {
        VariablesForm _variablesForm = null;
        DataGridView _dgvVariables = null;

        internal UpdateAutomaticLabelAction(VariablesForm variablesForm)
        {
            _variablesForm = variablesForm;
            _dgvVariables = _variablesForm.dgvVariables;
        }

        internal override bool Perform()
        {
            return _variablesForm._acronymManager.UpdateAutomaticLabel();
        }

        internal override bool UpdateVariables()
        {
            return true;
        }
    }
}
