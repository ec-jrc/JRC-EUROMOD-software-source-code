using EM_Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EM_XmlHandler
{
    public partial class ExeXmlReader
    {
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
                                                 bool ignorePrivate, Communicator communicator, bool readComment = false)
        {
            try
            {
                ExeXml.Country country = new ExeXml.Country();

                // first read the xlm-file into simple property/value dictionaries ...
                Dictionary<string, Dictionary<string, string>> ctry, syss, pols, refPols, funs, pars, sysPols, sysFuns, sysPars,
                                                               upInds, upIndYears, datas, sysDatas, extSwitch, localExt, extPol, extFun, extPar, indTaxes, indTaxYears;
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    ctry = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.COUNTRY, hasId: false, singleItem: true);
                    syss = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.SYS);
                    pols = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.POL);
                    refPols = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.REFPOL);
                    funs = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.FUN);
                    pars = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.PAR, hasId: false);
                    sysPols = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.SYS_POL, hasId: false);
                    sysFuns = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.SYS_FUN, hasId: false);
                    sysPars = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.SYS_PAR, hasId: false);
                    upInds = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.UPIND);
                    upIndYears = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.UPIND_YEAR, hasId: false);
                    datas = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.DATA);
                    sysDatas = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.SYS_DATA, hasId: false);
                    localExt = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.LOCAL_EXTENSION);
                    extPol = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.EXTENSION_POL, hasId: false);
                    extFun = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.EXTENSION_FUN, hasId: false);
                    extPar = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.EXTENSION_PAR, hasId: false);
                    extSwitch = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.EXTENSION_SWITCH, hasId: false);
                    indTaxes = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.INDTAX);
                    indTaxYears = XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.INDTAX_YEAR, hasId: false);
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

                return country;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failure reading file {path}{Environment.NewLine}{exception.Message}");
            }
        }
    }
}
