using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EM_Transformer
{
    public partial class EM2Data
    {
        /// <summary> reads an EM2 country-dataconfig-XML-file into an EM2Data.Content structure </summary>
        /// <param name="fileName"> full path to the country-dataconfig-XML-file </param>
        /// <param name="errors"> stores critical and non-critical erros during the read-process, empty structure for no errors </param>
        /// <returns> Content structure on success, null on failure </returns>
        public static Content Read(string fileName, out List<string> errors)
        {
            Content content = new Content();
            errors = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                using (XmlReader mainReader = XmlReader.Create(sr))
                {
                    mainReader.ReadToDescendant(EM2TAGS.DATABASE);
                    do
                    {
                        if (mainReader.NodeType != XmlNodeType.Element) continue;
                        if (mainReader.Name == EM2TAGS.DATABASE)
                        {
                            // *** READ DATABASE ELEMENT ***
                            EM2Item dataProperties = new EM2Item(); // takes the data-properties (name, yearinc, currency, ...)
                            XmlReader dataReader = mainReader.ReadSubtree(); dataReader.ReadToDescendant(EM2TAGS.ID);
                            do
                            {
                                if (dataReader.NodeType != XmlNodeType.Element) continue;
                                switch (dataReader.Name)
                                {
                                    case EM2TAGS.ID: dataProperties.id = dataReader.ReadInnerXml(); continue;
                                    case EM2TAGS.NAME: dataProperties.name = dataReader.ReadInnerXml(); continue;
                                    case EM2TAGS.SYSCONFIG: break; // DBSystemConfig-tag (for better readability not coded inside the switch-structure)
                                    default: dataProperties.properties.Add(dataReader.Name, dataReader.ReadInnerXml()); continue;
                                }

                                // *** READ DBSYSTEMCONFIG ELEMENT ***
                                SysDataItem dsProperties = new SysDataItem();
                                XmlReader dsReader = dataReader.ReadSubtree(); dsReader.ReadToDescendant(EM2TAGS.SYSTEM_ID);
                                do
                                {
                                    if (dsReader.NodeType != XmlNodeType.Element) continue;
                                    switch (dsReader.Name)
                                    {
                                        case EM2TAGS.DATAID: dsProperties.dataID = dsReader.ReadInnerXml(); continue;
                                        case EM2TAGS.SYSTEM_ID: dsProperties.sysID = dsReader.ReadInnerXml(); continue;
                                        case EM2TAGS.SYSNAME: dsProperties.sysName = dsReader.ReadInnerXml(); continue;
                                        case EM2TAGS.POLSWITCH: break; // PolicySwitch-tag (for better readability not coded inside the switch-structure)
                                        default: dsProperties.properties.Add(dsReader.Name, dsReader.ReadInnerXml()); continue;
                                    }

                                    // *** READ POLICYSWITCH ELEMENT ***
                                    PolSwitchItem switchProperties = new PolSwitchItem();
                                    XmlReader switchReader = dsReader.ReadSubtree(); switchReader.ReadToDescendant(EM2TAGS.SWITCHPOLID);
                                    do
                                    {
                                        if (switchReader.NodeType != XmlNodeType.Element) continue;
                                        switch (switchReader.Name)
                                        {
                                            case EM2TAGS.SWITCHPOLID: switchProperties.switchPolID = switchReader.ReadInnerXml(); continue;
                                            case EM2TAGS.SYSTEM_ID: switchProperties.sysID = switchReader.ReadInnerXml(); continue;
                                            case EM2TAGS.DATAID: switchProperties.dataID = switchReader.ReadInnerXml(); continue;
                                            case EM2TAGS.VALUE: switchProperties.value = switchReader.ReadInnerXml(); continue;
                                        }
                                    } while (switchReader.Read());

                                    // *** STORE POLICYSWITCH ELEMENT ***
                                    content.policySwitches.Add(switchProperties);
                                    switchReader.Close();

                                } while (dsReader.Read());

                                // *** STORE DBSYSTEMCONFIG ELEMENT ***
                                content.sysData.Add(dsProperties);
                                dsReader.Close();

                            } while (dataReader.Read());

                            // *** STORE DATABASE ELEMENT ***
                            content.dataSets.Add(dataProperties.id, dataProperties);
                            dataReader.Close();
                        }
                        else if (mainReader.Name == EM2TAGS.EXTENSION)
                        {
                            XmlReader extReader = mainReader.ReadSubtree(); extReader.ReadToDescendant(EM2TAGS.ID);
                            EM2Item extension = new EM2Item();
                            do
                            {
                                if (extReader.NodeType != XmlNodeType.Element) continue;
                                switch (extReader.Name)
                                {
                                    case EM2TAGS.ID: extension.id = extReader.ReadInnerXml(); break;
                                    case EM2TAGS.NAME: extension.name = extReader.ReadInnerXml(); break;
                                    case EM2TAGS.SHORTNAME: extension.properties.Add(EM2TAGS.SHORTNAME, extReader.ReadInnerXml()); break;
                                    case EM2TAGS.LOOK: extension.properties.Add(EM2TAGS.LOOK, extReader.ReadInnerXml()); break;
                                }
                            } while (extReader.Read());
                            content.localExtensions.Add(extension);
                            extReader.Close();
                        }
                    } while (mainReader.Read());
                }
                return content;
            }
            catch (Exception exception)
            {
                errors.Add(GenerateError(exception.Message));
                return null;
            }

            string GenerateError(string error)
            {
                FileInfo fi = new FileInfo(fileName);
                return $"{(fi == null ? string.Empty : fi.Name + ": ")}{error}";
            }
        }
    }
}
