using EM_Common;
using System.IO;
using System.Xml;

namespace EM_Transformer
{
    /// <summary> Common purpose "Library" (can maybe at least partly be moved to XMLHandler) </summary>
    public static class TransformerCommon
    {
        private const string TRANSFORMER_VERSION = "TV1"; // needs to be updated if a new tranformer version requires translating, even if the country content didn't change

        internal const string ROOT_ELEMENT = "RootElement";

        internal static void TranslateAndWriteElement(XmlWriter writer, string tag, string content, string mainTag)
        {
            tag = EM23Adapt.TranslateTag(tag, mainTag); // translate tag if necessary, e.g. PolicyID -> PolID
            WriteElement(writer, tag, content);         // mainTag (e.g. FUN) may be necessary for specification, if items share tag-names
        }

        internal static void WriteElement(XmlWriter writer, string tag, string content)
        {
            writer.WriteStartElement(tag); writer.WriteRaw(content); writer.WriteEndElement();
        }

        internal static XmlWriterSettings GetXmlWriterSettings()
        {
            return new XmlWriterSettings()
            {
                Indent = true,
                NewLineOnAttributes = true,
                ConformanceLevel = ConformanceLevel.Fragment
            };
        }

        public static XmlReaderSettings GetXmlReaderSettings()
        {
            return new XmlReaderSettings()
            {
                ConformanceLevel = ConformanceLevel.Fragment
            };
        }

        // tests whether a file is up-to-date or needs to be transformed
        // examples: IsFileUpToDate("C:\EuromodFiles\XMLParam\Countries\BG\BG.xml", "C:\EuromodFiles\EM3Translation\XMLParam\Countries\BG", out string hc)
        //           IsFileUpToDate("C:\EuromodFiles\XMLParam\Config\VarConfig.xml", "C:\EuromodFiles\EM3Translation\Config", out string hc)
        internal static bool IsFileUpToDate(string em2FullFilePath, string em3Folder, out string hashCode)
        {
            try
            {
                hashCode = EM_Helpers.GetFileMD5Hash(em2FullFilePath) + TRANSFORMER_VERSION;
                string hashFile = GetHashFilePath(em2FullFilePath, em3Folder);
                if (File.Exists(hashFile) && hashCode == File.ReadAllText(hashFile)) return true;
                return false;
            }
            catch { hashCode = string.Empty;  return false; }
        }

        // writes the hash-file, if the transformation was successfull
        internal static void WriteUpToDate(string em2FullFilePath, string em3Folder, string hashCode = null)
        {
            try
            {
                if (hashCode == null) hashCode = EM_Helpers.GetFileMD5Hash(em2FullFilePath);
                string hashFile = GetHashFilePath(em2FullFilePath, em3Folder);
                File.WriteAllText(hashFile, hashCode);
            }
            catch { }
        }

        private static string GetHashFilePath(string em2FullFilePath, string em3Folder)
        {
            return Path.Combine(em3Folder, $"up2Date_{Path.GetFileNameWithoutExtension(em2FullFilePath)}");
        }
    }
}
