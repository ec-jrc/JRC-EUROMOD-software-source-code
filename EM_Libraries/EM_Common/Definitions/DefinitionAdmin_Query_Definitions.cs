using System;
using System.Collections.Generic;

namespace EM_Common
{
    public partial class DefinitionAdmin
    {
        private static void DefineQueries()
        {
            queryDefs = new Dictionary<string, Query>(StringComparer.OrdinalIgnoreCase);

            queryDefs.Add(DefQuery.IsHeadOfTu, new Query() { aliases = new List<string>() { "IsHead" } });
            queryDefs.Add(DefQuery.IsPartner, new Query() { aliases = new List<string>() { "IsPartnerOfHeadOfTU" } });
            queryDefs.Add(DefQuery.IsDepChild, new Query() { aliases = new List<string>() { "IsDependentChild" } });
            queryDefs.Add(DefQuery.IsOwnChild, new Query());
            queryDefs.Add(DefQuery.IsOwnDepChild, new Query() { aliases = new List<string>() { "IsOwnDependentChild" } });
            queryDefs.Add(DefQuery.IsLooseDepChild, new Query() { aliases = new List<string>() { "IsLooseDependentChild" } });
            queryDefs.Add(DefQuery.IsDepParent, new Query() { aliases = new List<string>() { "IsDependentParent", "IsDepPar" } });
            queryDefs.Add(DefQuery.IsDepRelative, new Query() { aliases = new List<string>() { "IsDependentRelative", "IsDepRel" } });
            queryDefs.Add(DefQuery.IsLoneParentOfDepChild, new Query() { aliases = new List<string>() { "IsSingleParentOfDepChild" } });
            queryDefs.Add(DefQuery.IsMarried, new Query());
            queryDefs.Add(DefQuery.IsCohabiting, new Query());
            queryDefs.Add(DefQuery.IsWithPartner, new Query());
            queryDefs.Add(DefQuery.IsInEducation, new Query());
            queryDefs.Add(DefQuery.IsDisabled, new Query());
            queryDefs.Add(DefQuery.IsCivilServant, new Query());
            queryDefs.Add(DefQuery.IsBlueColl, new Query());
            queryDefs.Add(DefQuery.IsParent, new Query());
            queryDefs.Add(DefQuery.IsParentOfDepChild, new Query());
            queryDefs.Add(DefQuery.IsLoneParent, new Query() { aliases = new List<string>() { "IsLonePar", "IsSingleParent", "IsSinglePar" } } );
            queryDefs.Add(DefQuery.nLooseDepChildrenInTu, new Query() { aliases = new List<string>() { "nLooseDepChInTu", "nLooseDepChildrenInTaxunit", "nLooseDepchInTaxunit" } });
            queryDefs.Add(DefQuery.nDepParentsInTu, new Query());
            queryDefs.Add(DefQuery.nDepRelativesInTu, new Query());
            queryDefs.Add(DefQuery.nDepParentsAndRelativesInTu, new Query());
            queryDefs.Add(DefQuery.GetSystemYear, new Query() { isGlobal = true });
            queryDefs.Add(DefQuery.GetDataIncomeYear, new Query() { isGlobal = true });
            queryDefs.Add(DefQuery.IsOutputCurrencyEuro, new Query() { isGlobal = true });
            queryDefs.Add(DefQuery.IsParamCurrencyEuro, new Query() { isGlobal = true });
            queryDefs.Add(DefQuery.GetExchangeRate, new Query() { isGlobal = true });
            queryDefs.Add(DefQuery.IsNtoMchild, new Query() { par = DefQuery.GetNMPar(optional: false) });
            queryDefs.Add(DefQuery.HasMaxValInTu, new Query() { par = DefQuery.GetHasMinMaxPar(optional: false), aliases = new List<string>() { "IsRichestInTu" } });
            queryDefs.Add(DefQuery.IsUsedDatabase, new Query() { par = DefQuery.GetDBNamePar(optional: false), isGlobal = true });
            queryDefs.Add(DefQuery.HasMinValInTu, new Query() { par = DefQuery.GetHasMinMaxPar(optional: false) });
            queryDefs.Add(DefQuery.rand, new Query());

            queryDefs.Add(DefQuery.nChildrenOfCouple, new Query()
            {
                aliases = new List<string>() { "NChOfCouple" },
                par = DefQuery.GetAgePar()
            });
            queryDefs.Add(DefQuery.nDepChildrenOfCouple, new Query()
            {
                aliases = new List<string>() { "NDepChOfCouple" },
                par = DefQuery.GetAgePar()
            });
            queryDefs.Add(DefQuery.nPersInUnit, new Query()
            {
                aliases = new List<string>() { "NPersonsInTU", "NPersonsInTaxunit", "NPersInTU", "NPersTaxunit" },
                par = DefQuery.GetAgePar()
            });
            queryDefs.Add(DefQuery.nAdultsInTu, new Query()
            {
                aliases = new List<string>() { "nAdultsInTaxunit" },
                par = DefQuery.GetAgePar()
            });
            queryDefs.Add(DefQuery.nDepChildrenInTu, new Query()
            {
                aliases = new List<string>() { "nDepChInTU", "nDepChildrenInTaxunit", "nDepChInTaxunit" },
                par = DefQuery.GetAgePar()
            });

            queryDefs.Add(DefQuery.GetPartnerIncome, new Query()
            {
                aliases = new List<string>() { "GetPartnerInfo" },
                par = DefQuery.GetIncomePar(optional: false)
            });
            queryDefs.Add(DefQuery.GetCoupleIncome, new Query()
            {
                aliases = new List<string>() { "GetCoupleInfo" }, // this is only kept for security, actually it may suggest that one could ask for non-monetary variables (e.g. age) which does not make sense for a "group" of people
                par = DefQuery.GetIncomePar(optional: false)
            });
            queryDefs.Add(DefQuery.GetParentsIncome, new Query()
            {
                aliases = new List<string>() { "GetParentsInfo" }, // see above
                par = DefQuery.GetIncomePar(optional: false)
            });
            queryDefs.Add(DefQuery.GetMotherIncome, new Query()
            {
                aliases = new List<string>() { "GetMotherInfo" },
                par = DefQuery.GetIncomePar(optional: false)
            });
            queryDefs.Add(DefQuery.GetFatherIncome, new Query()
            {
                aliases = new List<string>() { "GetFatherInfo" },
                par = DefQuery.GetIncomePar(optional: false)
            });
            queryDefs.Add(DefQuery.GetOwnChildrenIncome, new Query()
            {
                par = DefQuery.GetIncomePar(optional: false)
            });
            queryDefs[DefQuery.GetOwnChildrenIncome].par.AddRange(DefQuery.GetAgePar());

            foreach (var q in queryDefs)
            {
                q.Value.description = q.Value.description == string.Empty ? GetQueryDescription(q.Key) : q.Value.description;
                foreach (var qp in q.Value.par)
                    qp.Value.description = qp.Value.description == string.Empty ? GetQueryParDescription(qp.Key) : qp.Value.description;
            }
        }
    }
}
