using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using EM_XmlHandler;
using EM_Common;

namespace EM_Transformer
{
    public static partial class EM3Global
    {
        public static bool WriteExRates(List<List<MultiProp>> content, string emPath, out List<string> errors)
        {
            (new EM23Adapt()).AdaptExRates(ref content);
            return Write(content, new EMPath(emPath).GetExRatesFilePath(), TAGS.EXRATE, out errors);
        }

        public static bool WriteHICP(List<List<MultiProp>> content, string emPath, out List<string> errors)
        {   // HICP does not have a EM2->EM3 transform-necessity (yet)
            return Write(content, new EMPath(emPath).GetHICPFilePath(), TAGS.HICP, out errors);
        }

        public static bool WriteExtensions(List<List<MultiProp>> content, string emPath, out List<string> errors)
        {
            (new EM23Adapt()).AdaptExtensions(ref content);
            return Write(content, new EMPath(emPath).GetExtensionsFilePath(), TAGS.EXTENSION, out errors);
        }

        /// <summary>
        /// transfers a global file (for ex-rates, hicp, glo-switches) into EM3 style and creates the EM3 file
        /// note: this class is only responsible for writing
        ///            EM2->EM3 adaptations are accomplished by a call to the EM23Adapt class (see AdaptX in the concrete functions above)
        /// also note: the function (respectively the concrete versions, see above)
        ///            is not intended to be used "stand-alone", but by EM3Global.Transform and EM3All.Write
        /// </summary>
        /// <param name="content"> content of global file as read by EM2Global.Read </param>
        /// <param name="fileName"> full path of EM3 global-file (i.e. the file to write) </param>
        /// <param name="errors"> critical and non-critical erros during the write-process, empty structure for no errors </param>
        private static bool Write(List<List<MultiProp>> content, string fileName, string tag, out List<string> errors)
        {
            errors = new List<string>();
            XmlWriter writer;
            try
            {
                using (writer = XmlWriter.Create(fileName, TransformerCommon.GetXmlWriterSettings()))
                {
                    writer.WriteStartElement(TransformerCommon.ROOT_ELEMENT);
                    writer.WriteStartElement(TAGS.Enclosure(tag)); // e.g. <HICP>
                    foreach (List<MultiProp> item in content)
                    {
                        writer.WriteStartElement(tag);
                        foreach (var prop in item) TransformerCommon.TranslateAndWriteElement(writer, prop.tag, prop.content, tag);
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement(); // e.g. </HICPs>
                    writer.WriteEndElement(); // ROOT_ELEMENT
                }
                return true;
            }
            catch (Exception exception)
            {
                errors.Add($"{fileName}: {exception.Message}");
                return false;
            }
        }
    }
}
