using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetExtStatInfo(Dictionary<string, Dictionary<string, string>> extStats, 
                                         Dictionary<string, Dictionary<string, string>> extStatYears,
                                         ExeXml.Country country)
        {
            if (extStats == null || extStatYears == null) return;
            foreach (Dictionary<string, string> es in extStatYears.Values) // loop over <UPIND_YEARs>
            {
                if (es.ContainsKey(TAGS.YEAR))
                {
                    string exStatId = es.GetOrEmpty(TAGS.EXSTAT_ID);

                    if (exStatId == string.Empty) continue;

                    string sAm = es.GetOrEmpty(TAGS.VALUE);
                    string sNum = es.GetOrEmpty(TAGS.NUMBER);

                    // If there is no amount nor number, then skip (or should we add NaN anyway?)
                    if (string.IsNullOrEmpty(sAm) && string.IsNullOrEmpty(sNum)) continue;

                    string statName = null; // get the name of the factor from <UPINDs>
                    if (extStats.ContainsKey(exStatId))
                    {
                        if (extStats[exStatId].ContainsKey(TAGS.NAME))
                            statName = extStats[exStatId][TAGS.NAME];
                        if (extStats[exStatId].ContainsKey(TAGS.TABLENAME))
                        {
                            if (extStats[exStatId][TAGS.TABLENAME].Equals("Aggregates"))
                            {
                                if (!country.extStats.ContainsKey(statName))   // If uprating factor name does not exist, create a new entry
                                    country.extStats.Add(statName, new ExeXml.ExStatDict(true));
                                if (double.TryParse(EM_Helpers.AdaptDecimalSign(sAm), out double am)) country.extStats[statName].SetYearAmount(es[TAGS.YEAR], am);
                                if (double.TryParse(EM_Helpers.AdaptDecimalSign(sNum), out double num)) country.extStats[statName].SetYearNumber(es[TAGS.YEAR], num);
                            }
                            else
                            {
                                if (!country.extStats.ContainsKey(statName))   // If uprating factor name does not exist, create a new entry
                                    country.extStats.Add(statName, new ExeXml.ExStatDict());
                                if (double.TryParse(EM_Helpers.AdaptDecimalSign(sAm), out double am)) country.extStats[statName].SetYearAmount(es[TAGS.YEAR], am);
                            }
                        }
                    }
                }
            }
        }
    }
}

