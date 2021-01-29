using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;

namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    class RVItem_CompulsoryPolicies : RVItem_Base
    {
        internal RVItem_CompulsoryPolicies() { tableName = "Compulsory Policies"; }

        List<string> compulsoryPolicies = new List<string> { DefPol.SPECIAL_POL_SETDEFAULT,
                                                             DefPol.SPECIAL_POL_UPRATE,
                                                             DefPol.SPECIAL_POL_ILSDEF,
                                                             DefPol.SPECIAL_POL_TUDEF,
                                                             DefPol.SPECIAL_POL_OUTPUT_STD,
                                                             DefPol.SPECIAL_POL_OUTPUT_STD_HH };
        List<string> unwantedFunctions = new List<string> { DefFun.Break };


        internal override void PerformValidation(List<string> countries, bool showProblemsOnly)
        {
            base.PerformValidation(countries, showProblemsOnly);
            foreach (string compulsoryPolicy in compulsoryPolicies)
            {
                string faulty = string.Empty;
                foreach (string country in countries) RegisterProblems(country, IsPolicyMissing(country, compulsoryPolicy), ref faulty);
                AddDataGridRow("Policy '" + compulsoryPolicy + "' is missing", faulty);
            }
            foreach (string unwantedFunction in unwantedFunctions)
            {
                string faulty = string.Empty;
                foreach (string country in countries) RegisterProblems(country, HasActiveFunction(country, unwantedFunction), ref faulty);
                AddDataGridRow("Function '" + unwantedFunction + "' should be removed", faulty);
            }
        }

        string IsPolicyMissing(string country, string policyName)
        {
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
            string faulty = string.Empty;
            foreach (CountryConfig.SystemRow system in GetPublicSystems(country))
            {
                CountryConfig.PolicyRow pol = ccf.GetPolicyRowByFullName(system.ID, policyName + "_" + country);
                if (pol != null && pol.Private != DefPar.Value.YES &&
                   (pol.Switch == DefPar.Value.ON ||
                   (policyName == DefPol.SPECIAL_POL_OUTPUT_STD_HH && pol.Switch == DefPar.Value.OFF))) continue;
                faulty += system.Name + ", ";
            }
            return faulty;
        }

        string HasActiveFunction(string country, string functionName)
        {
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(country);
            string faulty = string.Empty;
            foreach (CountryConfig.SystemRow system in GetPublicSystems(country))
            {
                foreach (CountryConfig.PolicyRow pol in system.GetPolicyRows())
                {
                    if (pol.Private != DefPar.Value.YES &&
                        pol.Switch == DefPar.Value.ON)
                    {
                        foreach (CountryConfig.FunctionRow fun in pol.GetFunctionRows())
                            if (fun.Name == functionName &&
                                fun.Private != DefPar.Value.YES &&
                                fun.Switch == DefPar.Value.ON)
                                { faulty += system.Name + ", "; break; }
                    }
                }
            }
            return faulty;
        }
    }
}
