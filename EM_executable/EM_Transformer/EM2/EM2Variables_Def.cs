using System.Collections.Generic;

namespace EM_Transformer
{
    public partial class EM2Variables
    {
        public const string FILE_VARCONFIG = "VARCONFIG.xml";

        /// <summary> the complete content of a EM2 varconfig-XML-file </summary>
        public class Content
        {
            // dic-key: property name, dic-value: property value (e.g. name=afc, monetary=1)
            public List<Dictionary<string, string>> variables = new List<Dictionary<string, string>>();
            public List<Dictionary<string, string>> countryLabels = new List<Dictionary<string, string>>();
            public List<Dictionary<string, string>> acroTypes = new List<Dictionary<string, string>>();
            public List<Dictionary<string, string>> acroLevels = new List<Dictionary<string, string>>();
            public List<Dictionary<string, string>> acros = new List<Dictionary<string, string>>();
            public List<Dictionary<string, string>> acroCategories = new List<Dictionary<string, string>>();
        }
    }
}
