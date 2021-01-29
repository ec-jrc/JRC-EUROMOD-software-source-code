using EM_Common;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        public static ExeXml.UpIndDict ReadHICP(string path, string country, string sysYear, string dataYear, Communicator communicator)
        {
            try
            {
                ExeXml.UpIndDict HICP = new ExeXml.UpIndDict();
                if (!File.Exists(path)) return HICP;

                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    // find the appropriate HICP for the respective data- and system-year
                    foreach (var rate in XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.HICP, hasId: false).Values)
                    {
                        if (rate.GetOrEmpty(TAGS.EXRATE_COUNTRY).ToLower() != country.ToLower()) continue;

                        string year = rate.GetOrEmpty(TAGS.YEAR);

                        string shicp = EM_Helpers.AdaptDecimalSign(rate.GetOrEmpty(TAGS.VALUE));
                        if (double.TryParse(shicp, out double hicp))
                            HICP.SetYear(year, hicp);
                    }
                }
                return HICP; // note: not finding must not be reported here, as the HICP may not be required
            } 
            catch (Exception exception)
            {
                throw new Exception($"Failure reading file {path}{Environment.NewLine}{exception.Message}");
            }
        }
    }
}
