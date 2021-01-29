using EM_UI.Actions;

namespace EM_UI.IndirectTaxes
{
    internal class IndirectTaxesAction : BaseAction
    {
        IndirectTaxesForm upratingIndicesForm = null;

        internal IndirectTaxesAction(IndirectTaxesForm _upratingIndicesForm)
        {
            upratingIndicesForm = _upratingIndicesForm;
        }

        internal override void PerformAction()
        {
            upratingIndicesForm.SaveTable();
        }
    }
}
