using EM_Common;
using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionSetPrivateAction : BaseAction
    {
        string extensionName, cc;
        bool set, actionIsCanceled = false;

        internal ExtensionSetPrivateAction(string _extensionName, string _cc, bool _set)
        {
            extensionName = _extensionName; cc = _cc; set = _set;
        }

        internal override bool ActionIsCanceled()
        {
            return actionIsCanceled;
        }

        internal override void PerformAction()
        {
            string extensionID = (from e in ExtensionAndGroupManager.GetExtensions(cc) where e.Name.ToLower() == extensionName.ToLower() select e.ID).FirstOrDefault();
            if (extensionID == null) { actionIsCanceled = true; return; }

            CountryConfigFacade countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(cc);
            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();

            List<string> polIDs = (from pe in countryConfig.Extension_Policy where pe.ExtensionID == extensionID && pe.BaseOff == false select pe.PolicyID).ToList();
            List<string> funIDs = (from fe in countryConfig.Extension_Function where fe.ExtensionID == extensionID && fe.BaseOff == false select fe.FunctionID).ToList();
            List<string> parIDs = (from pe in countryConfig.Extension_Parameter where pe.ExtensionID == extensionID && pe.BaseOff == false select pe.ParameterID).ToList();
            if (!polIDs.Any() && !funIDs.Any() && !polIDs.Any()) { actionIsCanceled = true; return; }

            string yn = set ? DefPar.Value.YES : DefPar.Value.NO;
            foreach (string polID in polIDs) countryConfigFacade.GetPolicyRowByID(polID).Private = yn;
            foreach (string funID in funIDs) countryConfigFacade.GetFunctionRowByID(funID).Private = yn;
            foreach (string parID in parIDs) countryConfigFacade.GetParameterRowByID(parID).Private = yn;
        }
    }
}
