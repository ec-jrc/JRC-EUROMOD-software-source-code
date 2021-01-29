using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace EM_Transformer
{
    public partial class EM2Variables
    {
        /// <summary> reads an EM2 varconfig-xml-file into an EM2Variables.Content structure </summary>
        /// <param name="configPath"> path to varconfig-xml-file </param>
        /// <param name="errors"> stores critical and non-critical erros during the read-process, empty structure for no errors </param>
        /// <returns> Content structure on success, null on failure </returns>
        public static Content Read(string configPath, out List<string> errors)
        {
            errors = new List<string>();
            string fileName = Path.Combine(configPath, FILE_VARCONFIG);
            try
            {
                Content content = new Content();
                using (StreamReader sr = new StreamReader(fileName, Encoding.UTF8))
                using (XmlReader mainReader = XmlReader.Create(sr))
                {
                    mainReader.ReadToDescendant(EM2TAGS.ACRO_TYPE);
                    do
                    {
                        if (mainReader.NodeType != XmlNodeType.Element) continue;
                        // *** READ ACRONYMS ***
                        if (mainReader.Name == EM2TAGS.ACRO_TYPE)
                        {
                            // *** READ ACRONYM TYPES ***
                            XmlReader acroTypeReader = mainReader.ReadSubtree();
                            acroTypeReader.ReadToDescendant(EM2TAGS.ID);
                            Dictionary<string, string> acroTypeProp = new Dictionary<string, string>();
                            do
                            {
                                if (acroTypeReader.NodeType != XmlNodeType.Element) continue;
                                if (acroTypeReader.Name == EM2TAGS.ACRO_LEVEL)
                                {
                                    // *** READ ACRONYM LEVELS ***
                                    XmlReader acroLevelReader = acroTypeReader.ReadSubtree();
                                    acroLevelReader.ReadToDescendant(EM2TAGS.ID);
                                    Dictionary<string, string> acroLevelProp = new Dictionary<string, string>();
                                    do
                                    {
                                        if (acroLevelReader.NodeType != XmlNodeType.Element) continue;
                                        if (acroLevelReader.Name == EM2TAGS.ACRO)
                                        {
                                            // *** READ ACRONYMS ***
                                            XmlReader acroReader = acroLevelReader.ReadSubtree();
                                            acroReader.ReadToDescendant(EM2TAGS.ID);
                                            Dictionary<string, string> acroProp = new Dictionary<string, string>();
                                            do
                                            {
                                                if (acroReader.NodeType != XmlNodeType.Element) continue;
                                                if (acroReader.Name == EM2TAGS.CATEGORY)
                                                {
                                                    // *** READ ACRONYM CATEGORIES ***
                                                    XmlReader acroCatReader = acroReader.ReadSubtree();
                                                    acroCatReader.ReadToDescendant(EM2TAGS.ACRO_ID);
                                                    Dictionary<string, string> acroCatProp = new Dictionary<string, string>();
                                                    do
                                                    {
                                                        if (acroCatReader.NodeType != XmlNodeType.Element) continue;
                                                        acroCatProp.Add(acroCatReader.Name, acroCatReader.ReadInnerXml());
                                                    } while (acroCatReader.Read());
                                                    content.acroCategories.Add(acroCatProp); acroCatReader.Close();
                                                }
                                                else acroProp.Add(acroReader.Name, acroReader.ReadInnerXml());

                                            } while (acroReader.Read());
                                            content.acros.Add(acroProp); acroReader.Close();
                                        }
                                        else acroLevelProp.Add(acroLevelReader.Name, acroLevelReader.ReadInnerXml());
                                    } while (acroLevelReader.Read());
                                    content.acroLevels.Add(acroLevelProp); acroLevelReader.Close();
                                }
                                else acroTypeProp.Add(acroTypeReader.Name, acroTypeReader.ReadInnerXml());
                            } while (acroTypeReader.Read());
                            content.acroTypes.Add(acroTypeProp); acroTypeReader.Close();
                        }
                        // *** READ VARIABLES ***
                        else if (mainReader.Name == EM2TAGS.VARIABLE)
                        {
                            XmlReader varReader = mainReader.ReadSubtree();
                            varReader.ReadToDescendant(EM2TAGS.ID);
                            Dictionary<string, string> varProp = new Dictionary<string, string>();
                            do
                            {
                                if (varReader.NodeType != XmlNodeType.Element) continue;
                                if (varReader.Name == EM2TAGS.COUNTRY_LABEL)
                                {
                                    XmlReader labReader = varReader.ReadSubtree(); labReader.ReadToDescendant(EM2TAGS.ID);
                                    Dictionary<string, string> labProp = new Dictionary<string, string>();
                                    do
                                    {
                                        if (labReader.NodeType != XmlNodeType.Element) continue;
                                        labProp.Add(labReader.Name, labReader.ReadInnerXml());
                                    } while (labReader.Read());
                                    content.countryLabels.Add(labProp); labReader.Close();
                                }
                                else varProp.Add(varReader.Name, varReader.ReadInnerXml());

                            } while (varReader.Read());
                            content.variables.Add(varProp); varReader.Close();
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
