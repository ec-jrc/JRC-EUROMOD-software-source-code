using System;
using System.Linq;
using System.Collections.Generic;

namespace EM_Common
{
    public partial class DefinitionAdmin
    {
        public class Fun
        {
            public DefFun.RUN_MODE runMode = DefFun.RUN_MODE.IN_SPINE;

            // kind and description are only used by the user interface, i.e. not required by the executable
            public enum KIND { POLICY, SYSTEM, SPECIAL, ADDON, ALL }; 
            public KIND kind = KIND.POLICY;
            public string description = string.Empty;

            internal Dictionary<string, Par> par = new Dictionary<string, Par>(StringComparer.OrdinalIgnoreCase);
            internal List<ParGroup> parGroups = new List<ParGroup>();

            // lists are not made public available, to prevent forgetting about substitues
            public Dictionary<string, Par> GetParList() { return par; }
            public List<ParGroup> GetGroupParList() { return parGroups; }

            public Par GetParDef(string name) { return par.ContainsKey(name) ? par[name] : null; }

            // search in all groups, whether this could be a valid parameter
            public Par GetGroupParDef(string parName, out string groupName)
            {
                groupName = null;
                foreach (var parGroup in parGroups)
                {
                    if (parGroup.par.ContainsKey(parName)) { groupName = parGroup.groupName; return parGroup.par[parName]; }
                }
                return null;
            }

            public string GetOfficalParName(string nonCaseSensitiveName)
            {
                // e.g. nonCaseSensitiveKey=comp_cond -> Comp_Cond, i.e. the name as defined by the constant COMP_COND
                if (par.ContainsKey(nonCaseSensitiveName)) return par.GetOriginalKey(nonCaseSensitiveName);
                // is supposed to be called only when we know that parameter exists, thus throw
                throw new Exception($"Invalid call of function GetOfficalParName({nonCaseSensitiveName})");
            }

            // returns true for e.g. SetDefault, ILDef, DefVar, etc., which allow placeholders
            public bool AllowsForPlaceholders() { return GetPlaceholderDef(out string dummy) != null; }
            public Par GetPlaceholderDef(out string groupName)
            {
                groupName = null;
                foreach (var p in par) if (p.Key == DefPar.PAR_TYPE.PLACEHOLDER.ToString()) return p.Value;
                foreach (ParGroup pg in parGroups)
                    foreach (var p in pg.par)
                        if (p.Key == DefPar.PAR_TYPE.PLACEHOLDER.ToString())
                        {
                            groupName = pg.groupName;
                            return p.Value;
                        }
                return null;
            }
        }

        public class Par
        {
            public DefPar.PAR_TYPE valueType;
            public int minCount = 0; // Min=0/Max=1 -> optional, Min=1/Max=1 -> compulsory/unique
            public int maxCount = 1; // Min=0/Max=MANY -> optional/not unique, Min=1/Max=MANY -> at least one, ...
            public object defaultValue = null;
            public bool isFootnote = false;
            public List<string> categValues = new List<string>();
            public List<string> substitutes = new List<string>();

            // the following info is only used by the user interface, i.e. not required by the executable
            public string description = string.Empty;
            public DefPar.PAR_TYPE placeholderType = DefPar.PAR_TYPE.TEXT; // only required if name (i.e. key of Fun.par) is PAR_TYPE.PLACEHOLDER
            public bool isCommon = false;
        }

        public class ParGroup
        {
            public Dictionary<string, Par> par = new Dictionary<string, Par>(StringComparer.OrdinalIgnoreCase);
            public string groupName = string.Empty; // an arbitrary name (e.g. GROUP_COMP for BenCalc's Comp_XXX-group)
                                                    // should be descriptive, as it is used in error-messages
            public int minCount = 0; // see correspondent description for class Par above
            public int maxCount = 1; // (defines optional and unique within the group)
        }
        public static Dictionary<string, string> GetFunNamesAndDesc(Fun.KIND kind)
        {
            Dictionary<string, string> funs = new Dictionary<string, string>(); // key: name, value: description
            foreach (var funDef in from fd in funDefs
                                   where kind == Fun.KIND.ALL || fd.Value.kind == kind
                                   select fd) funs.Add(funDef.Key, funDef.Value.description);
            return funs;
        }

        public static Fun GetFunDefinition(string funName, bool doThrow = true)
        {
            if (funDefs.ContainsKey(funName)) return funDefs[funName];
            if (!doThrow) return null;
            throw new Exception($"Common library does not contain a definition for function {funName}");
        }

        public static Par GetParDefinition(string funName, string parName, bool save = true)
        {
            Fun fun = GetFunDefinition(funName, false);
            if (fun != null)
            {
                // first search in non-group-parameters ...
                if (fun.par.ContainsKey(parName)) return fun.par[parName];
                // ... then search in group-parameters
                foreach (ParGroup pg in fun.parGroups)
                {
                    if (pg.par.ContainsKey(parName)) return pg.par[parName];
                }
            }
            return save ? new Par() : null;
        }
    }
}
