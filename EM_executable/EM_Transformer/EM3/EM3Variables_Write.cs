using EM_Common;
using EM_XmlHandler;
using System;
using System.Collections.Generic;
using System.Xml;

namespace EM_Transformer
{
    public partial class EM3Variables
    {
        /// <summary>
        /// transfers the EM2 variables file into EM3 style and creates the respective EM3 file
        /// note: this class is only responsible for writing, EM2->EM3 adaptations are accomplished by a call to the EM23Adapt class
        /// also note: the function is not intended to be used "stand-alone", but by EM3Variables.Transform and EM3All.Write
        /// </summary>
        /// <param name="content"> content of variables-file as read by EM2Variables.Read </param>
        /// <param name="emPath"> EuromodFiles folder (containing EM2-files in XMLParam and (will contain) EM3-files in EM3Translation\XMLParam) </param>
        /// <param name="errors"> critical and non-critical erros during the write-process, empty structure for no errors </param>
        public static bool Write(EM2Variables.Content content, string emPath, out List<string> errors)
        {
            errors = new List<string>();
            XmlWriter writer;
            try
            {
                (new EM23Adapt()).AdaptVar(content); // adapt from EM2 style to EM3 style

                using (writer = XmlWriter.Create(new EMPath(emPath).GetVarFilePath(), TransformerCommon.GetXmlWriterSettings()))
                {
                    writer.WriteStartElement(TransformerCommon.ROOT_ELEMENT);
                    WriteList(TAGS.VAR, content.variables);
                    WriteList(TAGS.ACTYPE, content.acroTypes);
                    WriteList(TAGS.ACLEVEL, content.acroLevels);
                    WriteList(TAGS.ACRO, content.acros);
                    WriteList(TAGS.CAT, content.acroCategories);
                    writer.WriteEndElement();
                    return true;
                }
            }
            catch (Exception exception)
            {
                errors.Add($"Variables: {exception.Message}");
                return false;
            }

            void WriteList(string tag, List<Dictionary<string, string>> items)
            {
                writer.WriteStartElement(TAGS.Enclosure(tag));
                foreach (var item in items)
                {
                    writer.WriteStartElement(tag);
                    foreach (var p in item) TransformerCommon.TranslateAndWriteElement(writer, p.Key, p.Value, tag);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }           
        }
    }
}
