using System.Collections.Generic;

namespace EM_Transformer
{
    public partial class EM23Adapt
    {
        // Put year-values from string (2006|102.49°2007|104.93°2008|109.83°2009...) into own elements
        // Note: This function supports multiple tables: uprating factors, inderect taxes
        private void AdaptYearValues(EM2Item em2, List<Dictionary<string, string>> em3, string ID)
        {
            foreach (var val in em2.properties)
            {
                if (val.Key != EM2TAGS.YEAR_VALUES) continue;
                foreach (string yearValPair in val.Value.Split('°'))
                {
                    if (string.IsNullOrEmpty(yearValPair)) continue;
                    string[] yearVal = yearValPair.Split('|');
                    em3.Add(new Dictionary<string, string>()
                    {
                        { ID, em2.id },
                        { EM_XmlHandler.TAGS.YEAR, yearVal[0] },
                        { EM_XmlHandler.TAGS.VALUE, yearVal[1] }
                    });
                }
            }
            em2.properties.Remove(EM2TAGS.YEAR_VALUES); // after putting into own elements, remove the string
        }

        // Put year-values from string (2006|102.49°2007|104.93°2008|109.83°2009...) into own elements
        // Note: This function supports external statistics
        private void AdaptYearValuesExStat(EM2Item em2, List<Dictionary<string, string>> em3)
        {
            foreach (var val in em2.properties)
            {
                if (val.Key != EM2TAGS.YEAR_VALUES) continue;
                foreach (string yearValPair in val.Value.Split('°'))
                {
                    string[] yearVal = yearValPair.Split('|');
                    if (yearVal.Length>2)   // if this is one of the ils, we need to add amounts, numbers and levels
                    {
                        em3.Add(new Dictionary<string, string>()
                        {
                            { EM_XmlHandler.TAGS.EXSTAT_ID, em2.id },
                            { EM_XmlHandler.TAGS.YEAR, yearVal[0] },
                            { EM_XmlHandler.TAGS.VALUE, yearVal[1] },
                            { EM_XmlHandler.TAGS.NUMBER, yearVal[2] },
                            { EM_XmlHandler.TAGS.LEVEL, yearVal[3] }
                        });
                    }
                    else    // else if this is distributional just create the amounts entry
                    {
                        em3.Add(new Dictionary<string, string>()
                        {
                            { EM_XmlHandler.TAGS.EXSTAT_ID, em2.id },
                            { EM_XmlHandler.TAGS.YEAR, yearVal[0] },
                            { EM_XmlHandler.TAGS.VALUE, yearVal[1] }
                        });
                    }
                }
            }
            em2.properties.Remove(EM2TAGS.YEAR_VALUES); // after putting into own elements, remove the string
        }
    }
}
