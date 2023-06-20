using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
        // the point of this addition here is to change the reading process below to make it much faster if only the external statistics or the incomelists are required.
        public enum ReadCountryOptions { ALL, EXSTAT, ILS };

        /// <summary>
        /// reads the relevant content of a country file into an ExeXml.Country structure (see parameters and ExeXml.Country wrt 'relevant')
        /// IMPORTANT NOTES:
        /// - error-reporting:
        ///   - everything that does not at least allow analysing the info throws an exception (e.g. file not found, system not found, ...)
        ///   - other errors are reported via the communicator - if they still allow running the programme via isWarning=true
        ///   - "relaxed" error handling: unlikely errors (which are actually only possible by manual manipulation of the xml-file)
        ///      are ignored without comment, if they do not prohibit the run (if things still work - so what?)
        /// - polices and functions switched to n/a are not read
        /// </summary>
        /// <param name="path"> full path to the country-xml-file </param>
        /// <param name="sysIdentifier"> id or name of system, only info belonging to this system is read, e.g. only relevant uprating indices </param>
        /// <param name="dataIdentifier"> id or name, only info belonging to this dataset is read, e.g. only relevant uprating indices </param>
        /// <param name="ignorePrivate"> if true, private elements are ignored by the reader </param>
        /// <param name="readComment"> if false (default), comments are not read (as the executable does not need them) </param>
        public static ExeXml.Country ReadCountry(string path, string sysIdentifier, string dataIdentifier,
                                                 bool ignorePrivate, Communicator communicator, bool readComment = false, ReadCountryOptions rco = ReadCountryOptions.ALL)
        {
            try
            {
                ExeXml.Country country = new ExeXml.Country();

                // first read the xlm-file into simple property/value dictionaries ...
                Dictionary<string, Dictionary<string, string>> ctry = null, syss = null, pols = null, refPols = null, funs = null, pars = null, sysPols = null, sysFuns = null, sysPars = null,
                                                               upInds = null, upIndYears = null, datas = null, sysDatas = null, extSwitch = null, localExt = null, extPol = null, extFun = null, extPar = null, indTaxes = null, indTaxYears = null, exStats = null, exStatYears = null;
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                // Read the Root Element first, and then the ones within it.
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    while (reader.NodeType != XmlNodeType.Element || reader.Name != TAGS.ROOT_ELEMENT)
                        if (!reader.Read()) return null;
                    XElement el = XElement.ReadFrom(reader) as XElement;
                    if (el == null || el.Name != TAGS.ROOT_ELEMENT) return null;

                    foreach (XElement xe in el.Elements())
                    {
                        if (xe.Value == null) continue;
                        switch (GetXEleName(xe))
                        {
                            case TAGS.COUNTRY: ctry = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.COUNTRY, hasId: false, singleItem: true); break;
                            case TAGS.SYS + "s": syss = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.SYS); break;
                            case TAGS.POL + "s": pols = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.POL); break;
                            case TAGS.REFPOL + "s": refPols = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.REFPOL); break;
                            case TAGS.FUN + "s": funs = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.FUN); break;
                            case TAGS.PAR + "s": pars = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.PAR, hasId: false); break;
                            case TAGS.SYS_POL + "s": sysPols = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.SYS_POL, hasId: false); break;
                            case TAGS.SYS_FUN + "s": sysFuns = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.SYS_FUN, hasId: false); break;
                            case TAGS.SYS_PAR + "s": sysPars = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.SYS_PAR, hasId: false); break;
                            case TAGS.UPIND + "s": upInds = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.UPIND); break;
                            case TAGS.UPIND_YEAR + "s": upIndYears = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.UPIND_YEAR, hasId: false); break;
                            case TAGS.EXSTAT + "s": exStats = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.EXSTAT); break;
                            case TAGS.EXSTAT_YEAR + "s": exStatYears = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.EXSTAT_YEAR, hasId: false); break;
                            case TAGS.DATA + "s": datas = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.DATA); break;
                            case TAGS.SYS_DATA + "s": sysDatas = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.SYS_DATA, hasId: false); break;
                            case TAGS.LOCAL_EXTENSION + "s": localExt = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.LOCAL_EXTENSION); break;
                            case TAGS.EXTENSION_POL + "s": extPol = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.EXTENSION_POL, hasId: false); break;
                            case TAGS.EXTENSION_FUN + "s": extFun = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.EXTENSION_FUN, hasId: false); break;
                            case TAGS.EXTENSION_PAR + "s": extPar = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.EXTENSION_PAR, hasId: false); break;
                            case TAGS.EXTENSION_SWITCH + "s": extSwitch = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.EXTENSION_SWITCH, hasId: false); break;
                            case TAGS.INDTAX + "s": indTaxes = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.INDTAX); break;
                            case TAGS.INDTAX_YEAR + "s": indTaxYears = XmlHelpers.GetXmlGroupItems(xe: xe, tag: TAGS.INDTAX_YEAR, hasId: false); break;
                            default: continue;  // simply ignore unknown tags
                        }
                    }
                }

                // ... then analyse the info:
                GetCountryInfo(country.cao, ctry);

                // note that sys-id can be actually the system's id (guid) or the system's name ...
                string sysId = GetSysInfo(sysIdentifier, syss, country, out string sysWarning);
                if (sysWarning != null) ReportError(communicator, path, sysWarning, false);

                // if no data was defined, try to get the best match
                if (string.IsNullOrEmpty(dataIdentifier))
                {
                    foreach (Dictionary<string, string> sysdata in sysDatas.Values)
                    {
                        if (sysdata[TAGS.SYS_ID] == sysId && sysdata[TAGS.BEST_MATCH].Equals("yes",StringComparison.InvariantCultureIgnoreCase))
                        {
                            dataIdentifier = sysdata[TAGS.DATA_ID];
                            break;
                        }
                    }

                    // if still no data, then issue error
                    if (string.IsNullOrEmpty(dataIdentifier))
                        ReportError(communicator, path, "No dataset provided and no best match was found for this system.", true);
                }

                // ... note that dataIdentifier can be actually the data's id (guid) or the data's name ...
                string dataId = GetDataInfo(dataIdentifier, datas, country, out string dataWarning);

                if (dataWarning != null) ReportError(communicator, path, dataWarning, false);

                GetPolInfo(sysId, pols, sysPols, refPols, ignorePrivate, country.cao, out List<string> polErrors, readComment);
                foreach (string polError in polErrors) ReportError(communicator, path, polError);

                GetFunInfo(sysId, funs, sysFuns, refPols, ignorePrivate, country.cao, out List<string> funErrors, readComment);
                foreach (string funError in funErrors) ReportError(communicator, path, funError);

                GetParInfo(sysId, pars, sysPars, ignorePrivate, country.cao, out List<string> parErrors, readComment);
                foreach (string parError in parErrors) ReportError(communicator, path, parError);

                GetUpIndInfo(upInds, upIndYears, country);

                GetExtensionInfo(sysId, dataId, extSwitch, localExt, extPol, extFun, extPar, country);

                if (!GetIndTaxInfo(indTaxes, indTaxYears, country))
                    ReportError(communicator, path, $"No values for year {country.data.indirectTaxTableYear} found in Indirect Taxes Table");

                GetExtStatInfo(exStats, exStatYears, country);

                return country;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failure reading file {path}{Environment.NewLine}{exception.Message}");
            }
        }
        private static string GetXEleName(XElement xe) { return xe.Name == null ? string.Empty : xe.Name.ToString(); }
    }
}
