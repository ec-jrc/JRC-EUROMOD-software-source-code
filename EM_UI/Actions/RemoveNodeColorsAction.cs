using EM_UI.DataSets;

namespace EM_UI.Actions
{
    internal class RemoveNodeColorsAction : BaseAction
    {
        CountryConfigFacade _countryConfigFacade = null;

        internal RemoveNodeColorsAction(CountryConfigFacade countryConfigFacade)
        {
            _countryConfigFacade = countryConfigFacade;
        }

        internal override void PerformAction()
        {
            _countryConfigFacade.RemoveAllNodeColors();
        }
    }
}
