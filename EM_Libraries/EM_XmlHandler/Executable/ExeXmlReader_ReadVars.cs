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
        public static Dictionary<string, bool> ReadVars(string path, Communicator communicator)
        {
            try
            {
                Dictionary<string, bool> content = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    foreach (var v in XmlHelpers.GetXmlGroupItems(reader: reader, tag: TAGS.VAR).Values)
                    {
                        if (!content.TryAdd(XmlHelpers.RemoveCData(v.GetOrEmpty(TAGS.NAME)),
                                            v.GetOrEmpty(TAGS.MONETARY) != "0"))
                            communicator.ReportError(new Communicator.ErrorInfo() { isWarning = true,
                                message = $"{Path.GetFileName(path)}: double definition of variable {XmlHelpers.RemoveCData(v.GetOrEmpty(TAGS.NAME))} - second definition is ignored" });
                    }
                }
                return content;
            }
            catch (Exception exception)
            {
                throw new Exception($"Failure reading file {path}{Environment.NewLine}{exception.Message}");
            }
        }
    }
}
