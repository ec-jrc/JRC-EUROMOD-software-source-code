using System;
using System.Collections.Generic;
using System.Text;

namespace EM_Statistics
{
    public class FilePackageContent
    {
        public string PathBase { get { return pathBase; } set { if (value != null) pathBase = value; } }
        private string pathBase = string.Empty;

        public List<string> PathsAlt { get { return pathsAlt; } set { if (value != null) pathsAlt = value; } }
        private List<string> pathsAlt = new List<string>();

        public StringBuilder MicroDataBase = null;
        public List<StringBuilder> MicroDataAlt { get { return microDataAlt; } set { if (value != null) microDataAlt = value; } }
        private List<StringBuilder> microDataAlt = new List<StringBuilder>();

        /// <summary> Note that there is no need to set Key manually. </summary>
        public string Key { get { return key; } }
        private readonly string key = Guid.NewGuid().ToString(); // automatic generation of key!
    }

}
