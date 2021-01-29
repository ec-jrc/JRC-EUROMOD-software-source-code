using EM_Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        private static void GetCountryInfo(ExeXml.CAO cao, Dictionary<string, Dictionary<string, string>> xmlCountry)
        {
            if (xmlCountry.Count == 0) throw new Exception($"tag {TAGS.COUNTRY} not found"); // must be a faulty xml-file

            Dictionary<string, string> cc = xmlCountry.First().Value;
            cao.shortName = cc.GetOrEmpty(TAGS.SHORT_NAME);
        }
    }
}
