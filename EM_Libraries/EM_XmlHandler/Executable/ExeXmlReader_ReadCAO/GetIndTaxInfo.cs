using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static bool GetIndTaxInfo(Dictionary<string, Dictionary<string, string>> indTaxes,
                                          Dictionary<string, Dictionary<string, string>> indTaxYears,
                                          ExeXml.Country country)
        {
            if (string.IsNullOrEmpty(country.data.indirectTaxTableYear)) return true;
            string relevantYear = country.data.indirectTaxTableYear.Trim().ToLower();

            foreach (Dictionary<string, string> it in indTaxYears.Values) // loop over <INDTAX_YEARs>
            {
                if (it.GetOrEmpty(TAGS.YEAR) != relevantYear) continue;

                string indTaxId = it.GetOrEmpty(TAGS.INDTAX_ID);
                string sVal = it.GetOrEmpty(TAGS.VALUE).Trim();
                if (indTaxId == string.Empty || sVal == string.Empty || sVal == DefPar.Value.NA) continue;

                double pct = sVal.EndsWith("%") ? 0.01 : 1;
                if (!double.TryParse(EM_Helpers.AdaptDecimalSign(sVal.Replace("%", "")), out double val)) continue;

                string indTaxName = null; // get the name of the factor from <INDTAXs>
                if (indTaxes.ContainsKey(indTaxId) && indTaxes[indTaxId].ContainsKey(TAGS.NAME))
                    indTaxName = indTaxes[indTaxId][TAGS.NAME];

                country.indTaxes.AddOrReplace(indTaxName, val * pct);
            }
            return country.indTaxes.Count > 0;
        }
    }
}
