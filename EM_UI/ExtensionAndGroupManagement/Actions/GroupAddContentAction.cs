using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class GroupAddContentAction : BaseAction
    {
        private string cc, groupName;

        internal GroupAddContentAction(string _cc, string _groupName)
        {
            cc = _cc; groupName = _groupName;
        }

        internal override void PerformAction()
        {
            ExtensionAndGroupMenuManager.GetSelectionPolFunPar(cc, out List<CountryConfig.PolicyRow> polRows, out List<CountryConfig.FunctionRow> funRows, out List<CountryConfig.ParameterRow> parRows);
            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();
            CountryConfig.LookGroupRow groupRow = (from lg in countryConfig.LookGroup where lg.Name.ToLower() == groupName.ToLower() select lg).First();

            foreach (CountryConfig.PolicyRow policyRow in polRows)
                if (!(from pg in countryConfig.LookGroup_Policy where pg.PolicyID == policyRow.ID && pg.LookGroupID == groupRow.ID select pg).Any())
                    countryConfig.LookGroup_Policy.AddLookGroup_PolicyRow(groupRow, policyRow); // make sure to not add twice (and thus crash)
            foreach (CountryConfig.FunctionRow functionRow in funRows)
                if (!(from fg in countryConfig.LookGroup_Function where fg.FunctionID == functionRow.ID && fg.LookGroupID == groupRow.ID select fg).Any())
                    countryConfig.LookGroup_Function.AddLookGroup_FunctionRow(groupRow, functionRow);
            foreach (CountryConfig.ParameterRow parameterRow in parRows)
                if (!(from pg in countryConfig.LookGroup_Parameter where pg.ParameterID == parameterRow.ID && pg.LookGroupID == groupRow.ID select pg).Any())
                    countryConfig.LookGroup_Parameter.AddLookGroup_ParameterRow(groupRow, parameterRow);
        }
    }
}
