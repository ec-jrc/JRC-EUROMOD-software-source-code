using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static string GetDataInfo(string dataIdentifier, Dictionary<string, Dictionary<string, string>> datas,
                                          ExeXml.Country country, out string warning)
        {
            warning = null;

            string dataId = GetIdByIdOrName(dataIdentifier, datas, false);
            if (dataId == null) // note: this only happens if a search by name fails (failed search by id throws exception)
            {                   //       and allows a command-line call with a "not-registered" dataset
                country.data.Name = dataIdentifier;
                warning = $"no description (year, currency, ...) for dataset {dataId} found";
                return string.Empty;
            }

            Dictionary<string, string> data = datas[dataId];
            country.data.Name = data.GetOrEmpty(TAGS.NAME);           
            country.data.year = data.GetOrEmpty(TAGS.YEAR_INC); // do not warn if empty, because e.g. training-data has no income-year

            if (data.TryGetValue(TAGS.CURRENCY_DATA, out string curOut)) country.data.isEuro = DefPar.Value.IsEuro(curOut);
            if (data.TryGetValue(TAGS.USE_COMMON_DEFAULT, out string ucd)) country.data.useCommonDefault = DefPar.Value.IsYes(ucd);
            if (data.TryGetValue(TAGS.READ_X_VARIABLES, out string xvar)) country.data.readXVar = DefPar.Value.IsYes(xvar);
            if (data.TryGetValue(TAGS.FILEPATH_DATA, out string specialPath)) country.data.specialPath = specialPath;
            if (data.TryGetValue(TAGS.LIST_STRING_OUTVAR, out string listStringOutVar)) country.data.listStringOutVar = listStringOutVar;
            if (data.TryGetValue(TAGS.INDIRECT_TAX_TABLE_YEAR, out string indirectTaxTableYear)) country.data.indirectTaxTableYear = indirectTaxTableYear;

            return dataId;
        }
    }
}
