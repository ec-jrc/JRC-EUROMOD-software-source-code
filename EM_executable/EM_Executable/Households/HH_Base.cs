using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class HH
    {
        private InfoStore infoStore = null;

        internal HH(InfoStore _infoStore) { infoStore = _infoStore; }

        // the household-data, i.e. a list of 'persons', where 'persons' is a list of the person's variables
        internal List<List<double>> personVarList = new List<List<double>>();

        // an additional list of non-numeric variables (mainly to allow transfer from input- to output-data)
        internal List<List<string>> personStringVarList = new List<List<string>>();
    }
}
