using EM_UI.Actions;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EM_UI.ExternalStatistics
{
    internal class ExternalStatisticsAction : BaseAction
    {
        ExternalStatisticsForm _externalStatisticForm = null;
        CountryConfigFacade _countryConfigFacade = null;

        internal ExternalStatisticsAction(ExternalStatisticsForm externalStatisticsForm)
        {
            _externalStatisticForm = externalStatisticsForm;
            _countryConfigFacade = _externalStatisticForm._countryConfigFacade;
        }

        internal override void PerformAction()
        {
            _countryConfigFacade.UpdateExternalStatistics(_externalStatisticForm.GetExternalStatisticsInfo());
        }
    }
}
