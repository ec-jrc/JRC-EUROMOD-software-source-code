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
        public static Dictionary<string, Tuple<string, string>> ReadExtensions(string path, Communicator communicator)
        {
            Dictionary<string, Tuple<string, string>> extensions = new Dictionary<string, Tuple<string, string>>();
            try
            {
                if (!File.Exists(path)) return extensions;

                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    foreach (var ext in XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.EXTENSION))
                    {
                        ext.Value.TryGetValue(TAGS.NAME, out string longName);
                        ext.Value.TryGetValue(TAGS.SHORT_NAME, out string shortName);
                        extensions.Add(ext.Key, new Tuple<string, string>(longName, shortName));
                    }
                }
            } 
            catch (Exception exception)
            {
                communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true, // do not throw as this not essential enough to jeopardise the run
                    message = $"Failure reading file {path}{Environment.NewLine}{exception.Message}" });
            }
            return extensions;
        }
    }
}
