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

        public static double ReadExRate(string path, string country,
                                        string sysName,            // in future perhaps 'year'
                                        string exRateDate,         // supposed to be TAGS.EXRATE_AVERAGE or TAGS.EXRATE_1stSEMESTER or
                                        Communicator communicator) //                TAGS.EXRATE_2ndSEMESTER or TAGS.EXRATE_DEFAULT
        {
            try
            {
                if (!File.Exists(path)) return -1;

                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    // find the appropriate exchangerate for the system to run
                    foreach (var rate in XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.EXRATE,
                                                                     hasId: false, uniqueProperties: false).Values)
                    {
                        if (rate.GetOrEmpty(TAGS.EXRATE_COUNTRY).ToLower() != country.ToLower()) continue;

                        foreach (var prop in rate) // in future this could perhaps be 'if (rate.GetOrEmpty(TAGS.YEAR) == year)'
                        {   
                            if (prop.Key.StartsWith(TAGS.SYS_NAME) && // (non-unique tags are stored as SysName1,SysName2,... in the Dictionary)
                                prop.Value.ToLower() == sysName.ToLower())
                            {
                                if (exRateDate == TAGS.EXRATE_DEFAULT) // if the user didn't define a concrete date (e.g. June30) use <Default>xxx</Default>
                                    exRateDate = rate.GetOrEmpty(TAGS.EXRATE_DEFAULT).Replace(" ", ""); // e.g. June 30 -> June30 (the former is the content of <Default>xxx</Default>, the latter the tag-name)
                                string sRate = EM_Helpers.AdaptDecimalSign(rate.GetOrEmpty(exRateDate));
                                if (double.TryParse(sRate, out double exRate)) return exRate;

                                ReportError(communicator, path, $"Exchange rate for {sysName} invalid" +
                                                                (sRate != string.Empty ? $": {sRate}" : string.Empty));
                                return -1;
                            }
                        }
                    }
                }
                return -1; // note: -1 is also what's used in the file to indicate n/a (thus easier for the caller to just check for -1)
                           // also note: not finding must not be reported here, as the rate may not be required (actually quite likely that data/param/output are in same currency)
            } 
            catch (Exception exception)
            {
                throw new Exception($"Failure reading file {path}{Environment.NewLine}{exception.Message}");
            }
        }
    }
}
