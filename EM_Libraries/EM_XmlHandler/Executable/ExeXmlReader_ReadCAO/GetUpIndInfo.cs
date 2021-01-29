using EM_Common;
using System.Collections.Generic;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetUpIndInfo(Dictionary<string, Dictionary<string, string>> upInds,
                                         Dictionary<string, Dictionary<string, string>> upIndYears,
                                         ExeXml.Country country)
        {
            foreach (Dictionary<string, string> ui in upIndYears.Values) // loop over <UPIND_YEARs>
            {
                if (ui.ContainsKey(TAGS.YEAR))
                {
                    string upIndId = ui.GetOrEmpty(TAGS.UPIND_ID);

                    string sVal = ui.GetOrEmpty(TAGS.VALUE);
                    if (upIndId == string.Empty || sVal == string.Empty ||
                        !double.TryParse(EM_Helpers.AdaptDecimalSign(sVal), out double val)) continue;

                    string facName = null; // get the name of the factor from <UPINDs>
                    if (upInds.ContainsKey(upIndId) && upInds[upIndId].ContainsKey(TAGS.NAME))
                        facName = upInds[upIndId][TAGS.NAME];

                    // Store all uprate indices
                    if (!country.upFacs.ContainsKey(facName))   // If uprating factor name does not exist, create a new entry
                        country.upFacs.Add(facName, new ExeXml.UpIndDict());
                    country.upFacs[facName].SetYear(ui[TAGS.YEAR], val);
                }
            }
        }
    }
}
