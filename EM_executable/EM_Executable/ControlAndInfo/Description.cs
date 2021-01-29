using EM_XmlHandler;

namespace EM_Executable
{
    /// <summary>
    /// holds the info about a function or parameter as read from XML
    /// main purpose: provide a unified description for usage in error and progress reports
    /// but also serves not loosing the xml-info (e.g. names of parameter/function/policy, group, etc.)
    /// </summary>
    internal class Description
    {
        internal ExeXml.Pol pol = null;
        internal ExeXml.Fun fun = null; internal string funID = null;
        internal ExeXml.Par par = null; internal string parID = null;
        internal string queryName = string.Empty;

        internal bool isPlaceholder = false; // is relevant for e.g. FunDefVar and FunDefUprate (see FunBase.GetPlaceholderGroupPar)

        internal Description(ExeXml.Pol _pol, ExeXml.Fun _fun, string _funID = null)
        { pol = _pol; fun = _fun; funID = _funID; }
        internal Description(Description funDesc, ExeXml.Par _par, string _parID = null)
        { pol = funDesc.pol; fun = funDesc.fun; funID = funDesc.funID; par = _par; parID = _parID; }
        internal Description(Description parDesc, string _queryName)
        { pol = parDesc.pol; fun = parDesc.fun; funID = parDesc.funID; par = parDesc.par; parID = parDesc.parID; queryName = _queryName; }

        internal string Get(bool skipID = false) // to be improved and/or extended by other descriptive functions
        {
            string order = string.Empty; // order makes only sense if we have pol+fun or pol+fun+par
            if (pol != null && fun != null) order = $"{pol.order}.{fun.order}";
            if (par != null && order != string.Empty) order += $".{par.order}";
            if (order != string.Empty) order += " ";

            string name = string.Empty;
            if (pol != null) name = $"{pol.name}/";
            if (fun != null) name += $"{fun.Name}";
            if (par != null) { if (name != string.Empty) name += "/"; name += par.Name; }
            if (queryName != string.Empty) name += "/" + queryName;

            string id = string.Empty;
            if (!skipID && (parID != null || funID != null)) id = " (" + (parID == null ? funID : parID) + ")";

            return order + name + id;
        }

        internal string GetPolName() { return pol == null ? string.Empty : pol.name; }
        internal string GetFunName() { return fun == null ? string.Empty : fun.Name; }
        internal string GetParName() { return par == null ? string.Empty : par.Name; }
        internal string GetParGroup() { return par == null ? string.Empty : par.Group; }
        internal string GetQueryName() { return queryName; }
    }
}
