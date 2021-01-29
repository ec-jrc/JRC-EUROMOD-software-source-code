using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EM_Transformer
{
    public class EM2Global
    {
        public const string FILE_EXRATES = "EXCHANGERATESCONFIG.xml";
        public const string FILE_HICP = "HICPCONFIG.xml";
        public const string FILE_SWITCHPOL = "SWITCHABLEPOLICYCONFIG.xml";

        public static List<List<MultiProp>> ReadExRates(string configPath, out List<string> errors, bool mustExist = false)
            { return Read(Path.Combine(configPath, FILE_EXRATES), EM2TAGS.EXRATES, EM2TAGS.COUNTRY, out errors, mustExist); }

        public static List<List<MultiProp>> ReadHICP(string configPath, out List<string> errors, bool mustExist = false)
            { return Read(Path.Combine(configPath, FILE_HICP), EM2TAGS.HICP, EM2TAGS.COUNTRY, out errors, mustExist); }

        public static List<List<MultiProp>> ReadSwitchPol(string configPath, out List<string> errors, bool mustExist = false)
            { return Read(Path.Combine(configPath, FILE_SWITCHPOL), EM2TAGS.SWITCHPOL, EM2TAGS.ID, out errors, mustExist); }

        private static List<List<MultiProp>> Read(string fileName, string itemTag, string firstPropTag, out List<string> errors, bool mustExist)
        {
            List<List<MultiProp>> content = new List<List<MultiProp>>();
            errors = new List<string>();

            if (!mustExist && !File.Exists(fileName)) return content;

            try
            {
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                using (XmlReader mainReader = XmlReader.Create(sr))
                {
                    mainReader.ReadToDescendant(itemTag);
                    do
                    {
                        if (mainReader.NodeType != XmlNodeType.Element) continue;
                        List<MultiProp> properties = new List<MultiProp>();
                        XmlReader eleReader = mainReader.ReadSubtree(); eleReader.ReadToDescendant(firstPropTag);
                        do
                        {
                            if (eleReader.NodeType != XmlNodeType.Element) continue;
                            properties.Add(new MultiProp() { tag = eleReader.Name, content = eleReader.ReadInnerXml() }); continue;
                        } while (eleReader.Read());
                        content.Add(properties);
                        eleReader.Close(); 
                    } while (mainReader.Read());
                }
                return content;
            }
            catch (Exception exception)
            {
                errors.Add(GenerateError(exception.Message));
                return new List<List<MultiProp>>();
            }

            string GenerateError(string error)
            {
                FileInfo fi = new FileInfo(fileName);
                return $"{(fi == null ? string.Empty : fi.Name + ": ")}{error}";
            }
        }
    }
}
