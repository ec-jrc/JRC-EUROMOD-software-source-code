using EM_Common;
using System.Collections.Generic;
using System.Linq;

namespace EM_Executable
{
    internal class ParQuery : ParBase_FormulaComponent
    {
        internal ParQuery(InfoStore infoStore) : base(infoStore) { }

        private QueryBase query = null;

        internal override void CheckAndPrepare(FunBase fun)
        {
            ExtractStandardFootnotes(xmlValue, out string queryName, fun); // it is not impossible that #_Low/UpLim are applied on queries
                                                                           // and #_Level may make good sense
            DefinitionAdmin.GetQueryDefinition(queryName, out DefinitionAdmin.Query queryDef, out string queryMainName);
            switch (queryMainName) // a query may have aliases, therefore GetQueryDefinition returns the "main"-name
            {
                case DefQuery.GetSystemYear: query = new QueryGetSystemYear(infoStore); break;
                case DefQuery.GetDataIncomeYear: query = new QueryGetDataIncomeYear(infoStore); break;
                case DefQuery.GetExchangeRate: query = new QueryGetExchangeRate(infoStore); break;
                case DefQuery.GetPartnerIncome: query = new QueryGetPartnerIncome(infoStore); break;
                case DefQuery.GetMotherIncome: query = new QueryGetMotherIncome(infoStore); break;
                case DefQuery.GetFatherIncome: query = new QueryGetFatherIncome(infoStore); break;
                case DefQuery.GetCoupleIncome: query = new QueryGetCoupleIncome(infoStore); break;
                case DefQuery.GetParentsIncome: query = new QueryGetParentsIncome(infoStore); break;
                case DefQuery.GetOwnChildrenIncome: query = new QueryGetOwnChildrenIncome(infoStore); break;
                case DefQuery.HasMaxValInTu: query = new QueryHasMaxValInTu(infoStore); break;
                case DefQuery.HasMinValInTu: query = new QueryHasMinValInTu(infoStore); break;
                case DefQuery.IsHeadOfTu: query = new QueryIsHead(infoStore); break;
                case DefQuery.IsMarried: query = new QueryIsMarried(infoStore); break;
                case DefQuery.IsCohabiting: query = new QueryIsCohabiting(infoStore); break;
                case DefQuery.IsWithPartner: query = new QueryIsWithPartner(infoStore); break;
                case DefQuery.IsUsedDatabase: query = new QueryIsUsedDatabase(infoStore); break;
                case DefQuery.IsDataVariable: query = new QueryIsDataVariable(infoStore); break;
                case DefQuery.IsNtoMchild: query = new QueryIsNtoMChild(infoStore); break;
                case DefQuery.IsParent: query = new QueryIsParent(infoStore); break;
                case DefQuery.IsParentOfDepChild: query = new QueryIsParentOfDepChild(infoStore); break;
                case DefQuery.IsInEducation: query = new QueryIsInEducation(infoStore); break;
                case DefQuery.IsDisabled: query = new QueryIsDisabled(infoStore); break;
                case DefQuery.IsPartner: query = new QueryIsPartner(infoStore); break;
                case DefQuery.IsDepChild: query = new QueryIsDepChild(infoStore); break;
                case DefQuery.IsOwnChild: query = new QueryIsOwnChild(infoStore); break;
                case DefQuery.IsOwnDepChild: query = new QueryIsOwnDepChild(infoStore); break;
                case DefQuery.IsLooseDepChild: query = new QueryIsLooseDepChild(infoStore); break;
                case DefQuery.IsDepParent: query = new QueryIsDepParent(infoStore); break;
                case DefQuery.IsDepRelative: query = new QueryIsDepRelative(infoStore); break;
                case DefQuery.IsLoneParentOfDepChild: query = new QueryIsLoneParentOfDepChild(infoStore); break;
                case DefQuery.IsCivilServant: query = new QueryIsCivilServant(infoStore); break;
                case DefQuery.IsBlueColl: query = new QueryIsBlueColl(infoStore); break;
                case DefQuery.IsOutputCurrencyEuro: query = new QueryIsOutputCurrencyEuro(infoStore); break;
                case DefQuery.IsParamCurrencyEuro: query = new QueryIsParamCurrencyEuro(infoStore); break;
                case DefQuery.IsLoneParent: query = new QueryIsLoneParent(infoStore); break;
                case DefQuery.nChildrenOfCouple: query = new QueryNChildrenOfCouple(infoStore); break;
                case DefQuery.nDepChildrenOfCouple: query = new QueryNDepChildrenOfCouple(infoStore); break;
                case DefQuery.nAdultsInTu: query = new QueryNAdultsInTu(infoStore); break;
                case DefQuery.nDepChildrenInTu: query = new QueryNDepChildrenInTu(infoStore); break;
                case DefQuery.nLooseDepChildrenInTu: query = new QueryNLooseDepChildrenInTu(infoStore); break;
                case DefQuery.nDepParentsInTu: query = new QueryNDepParentsInTu(infoStore); break;
                case DefQuery.nDepRelativesInTu: query = new QueryNDepRelativesInTu(infoStore); break;
                case DefQuery.nDepParentsAndRelativesInTu: query = new QueryNDepParentsAndRelativesInTu(infoStore); break;
                case DefQuery.nPersInUnit: query = new QueryNPersInUnit(infoStore); break;
                case DefQuery.rand: query = new QueryRand(infoStore); break;
                default: // the query must be defined in DefQuery, otherwise there wouldn't be a ParQuery-parameter, but it seems to be not implemented)
                    infoStore.communicator.ReportError(new Communicator.ErrorInfo()
                        { isWarning = false, message = $"{description.Get()}: usage of not implemented query {queryName}" });
                    return;
            }

            isGlobal = queryDef.isGlobal;
            query.description = new Description(description, queryName);
            query.CheckAndPrepare(fun, GetFootnoteNumbers(xmlValue, out string dummy));
        }

        internal override void ReplaceVarNameByIndex()
        {
            query.PrepareVarIndices(); // queries require variables like age, gender, ...
        }

        // note: person is only relevant for some queries, e.g. IsMarried, IsHead, IsNtoMChild ...
        // but ignored by e.g. nPersInTu, IsUsedDatabase, ...
        // for the former applies:
        // person != null: value is requested for a specific person
        // person == null: value is taken from head
        internal override double GetValue(HH hh, List<Person> tu, Person person = null)
        {
            if (alternativeTU != null)
            {
                tu = hh.GetAlternativeTU(alternativeTU, tu, description);
                if (person != null) person = (from p in tu where p.indexInHH == person.indexInHH select p).FirstOrDefault();
            }
            double val = query.GetValue(hh, tu, person == null ? tu[0] : person);
            return ApplyLimits(val, hh, tu, person);
        }
    }
}
