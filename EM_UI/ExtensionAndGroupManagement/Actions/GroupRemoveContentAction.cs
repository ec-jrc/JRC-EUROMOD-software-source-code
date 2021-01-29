using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Linq;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class GroupRemoveContentAction : BaseAction
    {
        string cc, groupName;

        internal GroupRemoveContentAction(string _cc, string _groupName)
        {
            cc = _cc; groupName = _groupName;
        }

        internal override void PerformAction()
        {
            ExtensionAndGroupMenuManager.GetSelectionPolFunPar(cc, out List<CountryConfig.PolicyRow> polRows, out List<CountryConfig.FunctionRow> funRows, out List<CountryConfig.ParameterRow> parRows);
            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();
            CountryConfig.LookGroupRow groupRow = (from lg in countryConfig.LookGroup where lg.Name.ToLower() == groupName.ToLower() select lg).First();

            List<CountryConfig.LookGroup_PolicyRow> delPolicyRows = new List<CountryConfig.LookGroup_PolicyRow>();
            List<CountryConfig.LookGroup_FunctionRow> delFunctionRows = new List<CountryConfig.LookGroup_FunctionRow>();
            List<CountryConfig.LookGroup_ParameterRow> delParameterRows = new List<CountryConfig.LookGroup_ParameterRow>();

            foreach (CountryConfig.PolicyRow policyRow in polRows)
            {
                funRows.AddRange(policyRow.GetFunctionRows()); // to make sure that all functions of the policy are removed as well - note: the AddContent-function
                                                               // does not remove a single function if its parent-policy is added (later)
                var del = from pg in countryConfig.LookGroup_Policy where pg.LookGroupID == groupRow.ID && pg.PolicyID == policyRow.ID select pg;
                if (del.Any() && !delPolicyRows.Contains(del.First())) delPolicyRows.Add(del.First());
            }
            foreach (CountryConfig.FunctionRow functionRow in funRows)
            {
                parRows.AddRange(functionRow.GetParameterRows()); // see above
                var del = from fg in countryConfig.LookGroup_Function where fg.LookGroupID == groupRow.ID && fg.FunctionID == functionRow.ID select fg;
                if (del.Any() && !delFunctionRows.Contains(del.First())) delFunctionRows.Add(del.First());
            }
            foreach (CountryConfig.ParameterRow parameterRow in parRows)
            {
                var del = from pg in countryConfig.LookGroup_Parameter where pg.LookGroupID == groupRow.ID && pg.ParameterID == parameterRow.ID select pg;
                if (del.Any() && !delParameterRows.Contains(del.First())) delParameterRows.Add(del.First());
            }

            for (int i = delPolicyRows.Count - 1; i >= 0; --i) delPolicyRows[i].Delete();
            for (int i = delFunctionRows.Count - 1; i >= 0; --i) delFunctionRows[i].Delete();
            for (int i = delParameterRows.Count - 1; i >= 0; --i) delParameterRows[i].Delete();
        }
    }
}
