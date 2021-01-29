using System.Collections.Generic;

namespace EM_Transformer
{
    /// <summary> class for reading all EM2 XML-files of a version (countries, variables file, global files, add-ons) </summary>
    public partial class EM2All
    {
        /// <summary> structure to combine EM2 country- and data-config-XML-files </summary>
        public class CountryContent
        {
            public EM2Country.Content country = null;
            public EM2Data.Content data = null;
        }

        /// <summary> structure to store all elements of a EM2 version, i.e. list of countries, variables, global files, add-ons </summary>
        public class Content
        {
            public List<CountryContent> countries = new List<CountryContent>();
            public List<EM2Country.Content> addOns = new List<EM2Country.Content>(); // (i.e. CountryContent without EM2Data.Content)
            public List<List<MultiProp>> exRates = null;   // i.e. List<rate> where rate is a list<Property>
            public List<List<MultiProp>> hicp = null;      // where Property is e.g. Country=BE or June30=40.5
            public List<List<MultiProp>> switchPol = null; // same format for hicp and switchPol
            public EM2Variables.Content varConfig = null;  // variables and acros
        }
    }
}
