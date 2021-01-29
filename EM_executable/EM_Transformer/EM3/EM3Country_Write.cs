using System;
using System.Collections.Generic;
using System.Xml;
using EM_XmlHandler;

namespace EM_Transformer
{
    public partial class EM3Country
    {
        /// <summary>
        /// transfers a country's EM2 country- and data-config-content into EM3 style and creates the EM3 country-file
        /// note: this class is only responsible for writing, EM2->EM3 adaptations are accomplished by a call to the EM23Adapt class
        /// also note: does not create the country folder
        ///            i.e. the function is not intended to be used "stand-alone", but by EM3.Transform/AddOn and EM3All.Write
        /// </summary>
        /// <param name="ctryContent"> content of country-XML-file as read by EM2Country.Read </param>
        /// <param name="dataContent"> content of country-dataconfig-XML-file as read by EM2Data.Read </param>
        /// <param name="extensionInfo"> content of global extension-XML-file (necessary for EM2->EM3 adaptation) </param>
        /// <param name="fileName"> full path of EM3 country-XML-file (i.e. the file to write) </param>
        /// <param name="errors"> critical and non-critical erros during the write-process, empty structure for no errors </param>
        public static bool Write(EM2Country.Content ctryContent, EM2Data.Content dataContent,
                                   List<List<MultiProp>> extensionInfo,
                                   string fileName, out List<string> errors)
        {
            errors = new List<string>();
            XmlWriter writer; // declare here to be able to use it in sub-functions
            try
            {
                // "summarised" EM2->EM3 adaptation process
                (new EM23Adapt()).AdaptCountry(ctryContent, dataContent, extensionInfo, out errors);

                using (writer = XmlWriter.Create(fileName, TransformerCommon.GetXmlWriterSettings()))
                {
                    writer.WriteStartElement(TransformerCommon.ROOT_ELEMENT);

                    // COUNTRY
                    WriteItem(TAGS.COUNTRY, ctryContent.general);

                    // SYSTEMS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.SYS));
                    foreach (var sys in ctryContent.systems.Values) WriteItem(TAGS.SYS, sys);
                    writer.WriteEndElement();

                    // POLICIES
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.POL));
                    foreach (var pol in ctryContent.policies.Values) WriteItem(TAGS.POL, pol);
                    writer.WriteEndElement();

                    // REFERENCE POLICIES
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.REFPOL));
                    foreach (var refPol in ctryContent.referencePolicies)
                    {
                        writer.WriteStartElement(TAGS.REFPOL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.ID, refPol.Key, TAGS.REFPOL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.REFPOL_ID, refPol.Value, TAGS.REFPOL);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // FUNCTIONS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.FUN));
                    foreach (var fun in ctryContent.functions.Values) WriteItem(TAGS.FUN, fun);
                    writer.WriteEndElement();

                    // PARAMETERS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.PAR));
                    foreach (var par in ctryContent.parameters.Values) WriteItem(TAGS.PAR, par);
                    writer.WriteEndElement();

                    // SYS-POL-ITEMS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.SYS_POL));
                    foreach (var syspol in ctryContent.sysPol) WriteSysItem(TAGS.SYS_POL, TAGS.POL_ID, TAGS.SWITCH, syspol);
                    writer.WriteEndElement();

                    // SYS-FUNC-ITEMS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.SYS_FUN));
                    foreach (var sysfun in ctryContent.sysFun) WriteSysItem(TAGS.SYS_FUN, TAGS.FUN_ID, TAGS.SWITCH, sysfun);
                    writer.WriteEndElement();

                    // SYS-PAR-ITEMS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.SYS_PAR));
                    foreach (var syspar in ctryContent.sysPar) WriteSysItem(TAGS.SYS_PAR, TAGS.PAR_ID, TAGS.VALUE, syspar);
                    writer.WriteEndElement();

                    // BREAK HERE FOR ADD-ONS !!! the rest only concerns countries
                    if (dataContent == null) return true;
                    // -------------------------------------------------------------------

                    // UPRATING INDICES - PROPERTIES (name, description, ...)
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.UPIND));
                    foreach (var upInd in ctryContent.upInd.Values) WriteItem(TAGS.UPIND, upInd);
                    writer.WriteEndElement();

                    // UPRATING INDICES - VALUES PER YEAR
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.UPIND_YEAR));
                    foreach (var upIndVal in ctryContent.upIndVal)
                    {
                        writer.WriteStartElement(TAGS.UPIND_YEAR);
                        WriteProperties(upIndVal, TAGS.UPIND_YEAR);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // DATASETS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.DATA));
                    foreach (var data in dataContent.dataSets.Values)
                    {
                        writer.WriteStartElement(TAGS.DATA);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.ID, data.id, TAGS.DATA);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.NAME, data.name, TAGS.DATA);
                        WriteProperties(data.properties, TAGS.DATA);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // SYS-DATA-ITEMS
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.SYS_DATA));
                    foreach (var ds in dataContent.sysData)
                    {
                        writer.WriteStartElement(TAGS.SYS_DATA);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.DATA_ID, ds.dataID, TAGS.SYS_DATA);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.SYS_ID, ds.sysID, TAGS.SYS_DATA);
                        WriteProperties(ds.properties, TAGS.SYS_DATA);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // EXTENSIONS
                    // extensions themself
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.LOCAL_EXTENSION));
                    foreach (var ext in dataContent.localExtensions) WriteItem(TAGS.LOCAL_EXTENSION, ext);
                    writer.WriteEndElement();
                    // extensions' policies
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.EXTENSION_POL));
                    foreach (var ep in ctryContent.extensionPol)
                    {
                        writer.WriteStartElement(TAGS.EXTENSION_POL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXTENSION_ID, ep.Item1, TAGS.EXTENSION_POL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.POL_ID, ep.Item2, TAGS.EXTENSION_POL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXENTSION_BASEOFF, ep.Item3, TAGS.EXTENSION_POL);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    // extensions' functions
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.EXTENSION_FUN));
                    foreach (var fp in ctryContent.extensionFun)
                    {
                        writer.WriteStartElement(TAGS.EXTENSION_FUN);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXTENSION_ID, fp.Item1, TAGS.EXTENSION_FUN);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.FUN_ID, fp.Item2, TAGS.EXTENSION_FUN);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXENTSION_BASEOFF, fp.Item3, TAGS.EXTENSION_FUN);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    // extensions' parameters
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.EXTENSION_PAR));
                    foreach (var ep in ctryContent.extensionPar)
                    {
                        writer.WriteStartElement(TAGS.EXTENSION_PAR);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXTENSION_ID, ep.Item1, TAGS.EXTENSION_PAR);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.PAR_ID, ep.Item2, TAGS.EXTENSION_PAR);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXENTSION_BASEOFF, ep.Item3, TAGS.EXTENSION_PAR);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    // extensions' switches
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.EXTENSION_SWITCH));
                    foreach (var sw in dataContent.policySwitches)
                    {
                        writer.WriteStartElement(TAGS.EXTENSION_SWITCH);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.EXTENSION_ID, sw.switchPolID, TAGS.EXTENSION_SWITCH);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.SYS_ID, sw.sysID, TAGS.EXTENSION_SWITCH);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.DATA_ID, sw.dataID, TAGS.EXTENSION_SWITCH);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.VALUE, sw.value, TAGS.EXTENSION_SWITCH);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // LOOKGROUPS
                    // groups themself
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.LOOKGROUP));
                    foreach (var lg in ctryContent.lookGroup.Values) WriteItem(TAGS.LOOKGROUP, lg);
                    writer.WriteEndElement();
                    // groups' policies
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.LOOKGROUP_POL));
                    foreach (var lgp in ctryContent.lookGroupPol)
                    {
                        writer.WriteStartElement(TAGS.LOOKGROUP_POL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.LOOKGROUP_ID, lgp.Item1, TAGS.LOOKGROUP_POL);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.POL_ID, lgp.Item2, TAGS.LOOKGROUP_POL);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    // groups' functions
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.LOOKGROUP_FUN));
                    foreach (var lgf in ctryContent.lookGroupFun)
                    {
                        writer.WriteStartElement(TAGS.LOOKGROUP_FUN);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.LOOKGROUP_ID, lgf.Item1, TAGS.LOOKGROUP_FUN);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.FUN_ID, lgf.Item2, TAGS.LOOKGROUP_FUN);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                    // groups' parameters
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.LOOKGROUP_PAR));
                    foreach (var lgp in ctryContent.lookGroupPar)
                    {
                        writer.WriteStartElement(TAGS.LOOKGROUP_PAR);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.LOOKGROUP_ID, lgp.Item1, TAGS.LOOKGROUP_PAR);
                        TransformerCommon.TranslateAndWriteElement(writer, TAGS.PAR_ID, lgp.Item2, TAGS.LOOKGROUP_PAR);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    // INDIRECT TAX - PROPERTIES (name, comment, ...)
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.INDTAX));
                    foreach (var indTax in ctryContent.indTax.Values) WriteItem(TAGS.INDTAX, indTax);
                    writer.WriteEndElement();

                    // INDIRECT TAX - VALUES PER YEAR
                    writer.WriteStartElement(TAGS.Enclosure(TAGS.INDTAX_YEAR));
                    foreach (var indTaxVal in ctryContent.indTaxVal)
                    {
                        writer.WriteStartElement(TAGS.INDTAX_YEAR);
                        WriteProperties(indTaxVal, TAGS.INDTAX_YEAR);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // ROOT_ELEMENT
                    return true;
                }
            }
            catch (Exception exception)
            {
                errors.Add($"{ctryContent.general.name}: {exception.Message}");
                return false;
            }

            void WriteItem(string tag, EM2Item item)
            {
                writer.WriteStartElement(tag);
                if (item.id != null) TransformerCommon.TranslateAndWriteElement(writer, TAGS.ID, item.id, tag);
                if (item.name != null) TransformerCommon.TranslateAndWriteElement(writer, TAGS.NAME, item.name, tag);
                WriteProperties(item.properties, tag);
                writer.WriteEndElement();
            }

            void WriteProperties(Dictionary<string, string> properties, string tag)
            {
                foreach (var p in properties)
                    if (!string.IsNullOrEmpty(p.Value))
                        TransformerCommon.TranslateAndWriteElement(writer, p.Key, p.Value, tag);
            }

            void WriteSysItem(string mainTag, string itemTag, string valTag, EM2Country.SysItem sysItem)
            {
                writer.WriteStartElement(mainTag);
                TransformerCommon.TranslateAndWriteElement(writer, TAGS.SYS_ID, sysItem.sysID, mainTag);
                TransformerCommon.TranslateAndWriteElement(writer, itemTag, sysItem.itemID, mainTag);
                TransformerCommon.TranslateAndWriteElement(writer, TAGS.ORDER, sysItem.order, mainTag);
                TransformerCommon.TranslateAndWriteElement(writer, valTag, sysItem.value, mainTag);
                writer.WriteEndElement();
            }
        }
    }
}
