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
        /// <summary> reads the relevant content of a add-on file into an ExeXml.AddOn structure (see ReadCountry for IMPORTANT NOTES) </summary>
        /// <param name="path"> full path to the add-on-xml-file </param>
        /// <param name="addOnSysIdentifier"> id or name of add-on-system, only info belonging to this system is read </param>
        public static ExeXml.AddOn ReadAddOn(string path, string addOnSysIdentifier, Communicator communicator)
        {
            try
            {
                ExeXml.AddOn addOn = new ExeXml.AddOn();

                // first read the xlm-file into simple property/value dictionaries ...
                Dictionary<string, Dictionary<string, string>> ctry, syss, pols, refPols, funs, pars, sysPols, sysFuns, sysPars;
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
                }

                // ... then analyse the info:
                GetCountryInfo(addOn.cao, ctry);

                string addOnSysId = GetIdByIdOrName(addOnSysIdentifier, syss, true); // search system by id or name (throws exeception if not found)

                GetPolInfo(addOnSysId, pols, sysPols, refPols, false, addOn.cao, out List<string> polErrors);
                foreach (string polError in polErrors) ReportError(communicator, path, polError);

                GetFunInfo(addOnSysId, funs, sysFuns, refPols, false, addOn.cao, out List<string> funErrors);
                foreach (string funError in funErrors) ReportError(communicator, path, funError);

                GetParInfo(addOnSysId, pars, sysPars, false, addOn.cao, out List<string> parErrors);
                foreach (string parError in parErrors) ReportError(communicator, path, parError);

                // filter the add-on info out (for convenience, could actually also be done in the executable's add-on-handler)
                foreach (var pol in addOn.cao.pols)
                {
                    if (!pol.Value.name.ToLower().StartsWith(ExeXml.AddOn.POL_AO_CONTROL.ToLower())) continue;

                    addOn.polAOControl = pol.Value;
                    foreach(var fun in pol.Value.funs)
                    {
                        if (fun.Value.Name.ToLower() != DefFun.AddOn_Applic.ToLower()) continue;
                        foreach (var par in fun.Value.pars)
                            if (par.Value.Name.ToLower() == ExeXml.AddOn.PAR_APPLIC_SYS && par.Value.val != DefPar.Value.NA)
                                addOn.applicSys.Add(par.Value.val);
                        pol.Value.funs.Remove(fun.Key);
                        break;
                    }

                    addOn.cao.pols.Remove(pol.Key);
                    break;
                }
                if (addOn.polAOControl == null) throw new Exception($"Policy {ExeXml.AddOn.POL_AO_CONTROL}* not found");
                
                return addOn;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failure reading file {path}{Environment.NewLine}{exception.Message}");
            }
        }
    }
}
