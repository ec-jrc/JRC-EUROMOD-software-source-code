using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EM_UI.CountryAdministration;

namespace EM_UI.Actions
{
    internal class SaveAction : BaseAction
    {
        string _countryShortName = "";
        string _alternativePath = "";
        string _alternativeName = "";        
        
        internal SaveAction(string countryShortName, string alternativePath = "", string alternativeName = "")
        {
            _countryShortName = countryShortName;
            _alternativePath = alternativePath;
            _alternativeName = alternativeName;
        }

        internal override void PerformAction()
        {
            CountryAdministrator.WriteXML(_countryShortName, _alternativePath, _alternativeName);
        }

        internal override bool ClearMultiSelector()
        {
            return false;
        }
    }
}
