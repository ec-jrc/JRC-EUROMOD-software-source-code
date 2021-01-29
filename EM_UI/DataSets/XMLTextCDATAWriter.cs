using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace EM_UI.DataSets
{
    internal class XmlTextCDATAWriter : XmlTextWriter
    {
        List<string> _elementsToWriteAsCDATA = new List<string>();

        bool _writeAsCDATA = false;

        internal XmlTextCDATAWriter(Stream stream, Encoding encoding) : base(stream, encoding) { }

        //all elements with names contained in elementsToWriteAsCDATA are written as CDATA
        internal XmlTextCDATAWriter(Stream stream, Encoding encoding, string[] elementsToWriteAsCDATA, bool saveWithLineBreaks = true)
            : base(stream, encoding) 
        {
            if (saveWithLineBreaks)
            {
                this.Formatting = Formatting.Indented;
                this.Indentation = 2;
            }
            foreach (String elem in elementsToWriteAsCDATA)
                _elementsToWriteAsCDATA.Add(elem);
        }

        public override void WriteStartElement(string prefix, string localName, string nameSpace)
        {
            if (_elementsToWriteAsCDATA.Contains(localName))
                _writeAsCDATA = true;
            else
                _writeAsCDATA = false;
            
            base.WriteStartElement(prefix, localName, nameSpace);
        }

        public override void WriteEndElement()
        {
                _writeAsCDATA = false;

                base.WriteEndElement();
        }

        public override void WriteString(string text)
        {
            if (_writeAsCDATA && WriteState == System.Xml.WriteState.Element)
                base.WriteCData(text);
            else
                base.WriteString(text);
        }
    }
}
