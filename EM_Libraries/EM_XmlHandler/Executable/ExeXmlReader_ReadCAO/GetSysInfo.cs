using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static string GetSysInfo(string sysIdentifier, Dictionary<string, Dictionary<string, string>> syss,
                                         ExeXml.Country country, out string warning)
        {
            warning = null;
            string sysId = GetIdByIdOrName(sysIdentifier, syss, true); // search system by id or name (throws exeception if not found)
            Dictionary<string, string> sys = syss[sysId];

            country.sys.id = sysId;
            country.sys.name = sys.GetOrEmpty(TAGS.NAME);
            country.sys.headDefInc = sys.GetOrEmpty(TAGS.HEAD_DEF_INC);

            country.sys.year = sys.GetOrEmpty(TAGS.YEAR); // is important for uprating factors, therefore warn
            if (country.sys.year == string.Empty) warning = $"system year (<{TAGS.YEAR}>) not found";

            if (sys.TryGetValue(TAGS.CURRENCY_OUTPUT, out string curOut)) country.sys.isOutputEuro = DefPar.Value.IsEuro(curOut);
            if (sys.TryGetValue(TAGS.CURRENCY_PARAM, out string curPar)) country.sys.areParamEuro = DefPar.Value.IsEuro(curPar);

            return sysId;
        }
    }
}
