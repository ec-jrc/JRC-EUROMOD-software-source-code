using EM_UI.Actions;
using EM_UI.DataSets;

namespace EM_UI.UpratingIndices
{
    internal class UpratingIndicesAction : BaseAction
    {
        UpratingIndicesForm _upratingIndicesForm = null;
        CountryConfigFacade _countryConfigFacade = null;

        internal UpratingIndicesAction(UpratingIndicesForm upratingIndicesForm)
        {
            _upratingIndicesForm = upratingIndicesForm;
            _countryConfigFacade = _upratingIndicesForm._countryConfigFacade;
        }

        internal override void PerformAction()
        {
            _countryConfigFacade.UpdateUpratingIndices(_upratingIndicesForm.GetRawIndicesInfo());
        }
    }
}
