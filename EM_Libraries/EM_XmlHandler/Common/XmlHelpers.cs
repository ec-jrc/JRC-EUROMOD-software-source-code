using EM_Common;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace EM_XmlHandler
{
    public static class XmlHelpers
    {
        public static string RemoveCData(string s)
        {
            string pre = "<![CDATA[", post = "]]>";
            return s.StartsWith(pre) && s.EndsWith(post) ? s.Substring(pre.Length, s.Length - pre.Length - post.Length) : s;
        }

        public static string CDATA(string content = "") { return $"<![CDATA[{content}]]>"; }

        /// <summary>
        /// returns for example all systems, if tag==SYS, or all policies, if tag==POL, ...
        /// Key of outer Dictionary: id, e.g. system-id, if hasId=false: pseudo-id
        /// Key of inner Dictionary: name of property (e.g. Name, Year, ...),
        ///     if uniqueProperties=false a counter is added (e.g. SysID_1, SysID_2 for PRIVATE)
        /// Value of inner Dictionary: content of property (e.g. SL_demo, 2014, ...)
        /// <param name="singleItem"> true for e.g. COUNTRY, meaning that there is no COUNTRYs (like for SYS -> SYSs) </param>
        /// <returns></returns>
        public static Dictionary<string, Dictionary<string, string>> GetXmlGroupItems(XElement xe, string tag,
                                           bool hasId = true, bool uniqueProperties = true, bool singleItem = false)
        {
            using (XmlReader reader = xe.CreateReader())
            {
                return GetXmlGroupItems(reader, tag, hasId, uniqueProperties, singleItem);
            }
        }
        public static Dictionary<string, Dictionary<string, string>> GetXmlGroupItems(XmlReader reader, string tag,
                                           bool hasId = true, bool uniqueProperties = true, bool singleItem = false)
        {
            string mainTag = singleItem ? tag : TAGS.Enclosure(tag);

            Dictionary<string, Dictionary<string, string>> groupItems = new Dictionary<string, Dictionary<string, string>>();
            Dictionary<string, string> groupItem = null;
            string id = null;

            int pseudoId = 0, nonUniquePropId = 0;

            do
            {
                // e.g. </SYS> (but not </COUNTRY>)
                if (reader.Name == mainTag && !singleItem && (reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement))
                    return groupItems;

                if (reader.Name == tag)
                {
                    if (reader.NodeType == XmlNodeType.Element) // e.g. <SYS> or <COUNTRY>
                    {
                        groupItem = new Dictionary<string, string>();
                        if (hasId == false) id = (pseudoId++).ToString();
                    }
                    if (reader.NodeType == XmlNodeType.EndElement) // e.g. </SYS> or </COUNTRY>
                    {
                        if (id != null) groupItems.TryAdd(id, groupItem);
                        groupItem = null; id = null;
                        if (singleItem) return groupItems; // e.g. </COUNTRY>
                    }
                }
                else if (groupItem != null && reader.NodeType == XmlNodeType.Element)
                {
                    string propName = reader.Name, propVal = reader.ReadInnerXml();
                    if (propName == TAGS.ID) id = propVal;
                    if (groupItem.ContainsKey(propName) && !uniqueProperties)
                        propName += "_" + (nonUniquePropId++).ToString();
                    groupItem.TryAdd(propName, propVal);
                }
            }
            while (reader.Read());

            return groupItems; // getting here means the end-element was not found (i.e. <tag>...</tag> does not exist in the xml-file)
        }
    }
}
