using System;
using System.Collections.Generic;
using System.Linq;

namespace EM_Common
{
    public partial class DefinitionAdmin
    {
        public class Query
        {
            public Dictionary<string, Par> par = new Dictionary<string, Par>(StringComparer.OrdinalIgnoreCase);
            public List<string> aliases = new List<string>(); // many queries have not just one name, e.g. IsDepChild/IsDependentChild
            public bool isGlobal = false; // true for e.g. IsUsedDatabase, IsOutputCurrencyEuro, etc., i.e. does not depend on personal data
            public string description = string.Empty;
        }

        public static void GetQueryDefinition(string queryName, out Query queryDef, out string queryMainName, bool doThrow = true)
        {
            if (queryName.EndsWith(DefQuery.HAS_PAR_MARKER)) queryName = queryName.Substring(0, queryName.Length - DefQuery.HAS_PAR_MARKER.Length); // remove potential #x which may have been added for UI
            if (queryDefs.ContainsKey(queryName))
            { queryDef = queryDefs[queryName]; queryMainName = queryDefs.GetOriginalKey(queryName); return; }
            foreach (var q in queryDefs) // could still be an alias
                if (q.Value.aliases.Contains(queryName, StringComparer.CurrentCultureIgnoreCase))
                    { queryDef = q.Value; queryMainName = q.Key; return; }
            if (doThrow) throw new Exception($"Common library does not contain a definition for query {queryName}");
            queryDef = null; queryMainName = queryName;
        }

        public static List<string> GetQueryNamesAndAliases()
        {
            List<string> NandA = new List<string>();
            foreach (var q in queryDefs) { NandA.Add(q.Key); NandA.AddRange(q.Value.aliases); }
            return NandA;
        }

        public static Dictionary<string, string> GetQueryNamesAndDesc(bool addHasParMarker = true)
        {
            Dictionary<string, string> queries = new Dictionary<string, string>(); // key: name, value: description
            foreach (var query in queryDefs)
                queries.Add(query.Key + (addHasParMarker && query.Value.par.Count() > 0 ? DefQuery.HAS_PAR_MARKER : string.Empty), query.Value.description);
            return queries;
        }
    }
}
