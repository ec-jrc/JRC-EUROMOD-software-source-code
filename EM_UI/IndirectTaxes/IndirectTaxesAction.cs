using EM_UI.Actions;
using EM_UI.DataSets;

namespace EM_UI.IndirectTaxes
{
    internal class IndirectTaxesAction : BaseAction
    {
        IndirectTaxesForm _indirectTaxesForm = null;
        CountryConfigFacade _countryConfigFacade = null;

        internal IndirectTaxesAction(IndirectTaxesForm indirectTaxesForm)
        {
            _indirectTaxesForm = indirectTaxesForm;
            _countryConfigFacade = _indirectTaxesForm._countryConfigFacade;
        }

        internal override void PerformAction()
        {
            _indirectTaxesForm.SaveTable();
        }
    }
}
