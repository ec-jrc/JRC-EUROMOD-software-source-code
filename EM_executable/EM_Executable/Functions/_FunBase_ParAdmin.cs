using EM_Common;
using System;
using System.Collections.Generic;

namespace EM_Executable
{
    internal partial class FunBase
    {
        // ===================================================================================================================================
        // the following parameter-lists are produced by 'TakePar', based on the general function/parameter definitions in the Common-library
        // 'CheckAndPrepare' and partly 'ProvideIndexInfo' then assess them via the Get-functions below
        // -----------------------------------------------------------------------------------------------------------------------------------
        private Dictionary<string, ParBase> uniquePar = // key: name of parameter (e.g. Formula)
            new Dictionary<string, ParBase>();

        private Dictionary<string, List<ParBase>> nonUniquePar = // key: name of parameter (e.g. VarGroup, ILGroup, Dataset, ...)
            new Dictionary<string, List<ParBase>>();

        private Dictionary<string, // group-name (e.g. Group_Comp), that's just an identifier, and usually there will be one (or no) group
                                   // however there are functions with several groups, as e.g. Uprate (Group_Main, Group_Agg, ...)
                SortedList<int,    // list of groups (e.g. 1 to many components of BenCalc), sorted by group-number
                                   // (sorting is not relevant for BenCalc, but e.g. for SchedCalc)
                List<ParBase>>>    // list of the parameters of the group (e.g. Comp_Cond, Comp_PerElig, Comp_UpLim)
                                   // note: this cannot be a dictionary, because the group could contain non-unique parameters
                groupPar = new Dictionary<string, SortedList<int, List<ParBase>>>();

        private Dictionary<string,   // footnote-number (e.g. 7)
                  Dictionary<string, // name of parameter (e.g. #_Level, #_UpLim, #_AgeMin, ...)
                  ParBase>>          // the footnote-parameter
                  footnotePar = new Dictionary<string, Dictionary<string, ParBase>>();
        private Dictionary<string,       // footnote-number
                List<string>>            // names of parameters
                footnoteUsage = new Dictionary<string, List<string>>(); // any used parameter is added (see GetFootnotePar) for later check

        private Dictionary<string, // for DefIL e.g. 'bch_s', for SetDefault e.g. 'yem'
                ParBase>           // for DefIL e.g. a ParBase with xmlValue='+', for SetDefault e.g. a ParFormula with xmlValue='yemiv'
                                   // note: the type of the parameter refers to the content, i.e. '+' and 'yemiv' (and not to 'bch_s' resp. 'yem')
                                   // also note: this list only contains non-group placeholder-parameters (as in e.g. DefIL, SetDefault)
                placeholderPar = new Dictionary<string, ParBase>();
        // ==============================================================================================================================================

        /// <summary> gather all parameters in a plain list (i.e. extract from the complex structures above) </summary>
        private List<ParBase> GetPlainParList(bool includeFootnotes = false)
        {
            List<ParBase> plain = new List<ParBase>();
            plain.AddRange(uniquePar.Values);
            foreach (var nup in nonUniquePar.Values) plain.AddRange(nup);
            foreach (var _gp in groupPar.Values) foreach (var gp in _gp.Values) plain.AddRange(gp);
            plain.AddRange(placeholderPar.Values);
            if (includeFootnotes) foreach (var fn in footnotePar.Values) plain.AddRange(fn.Values);
            return plain;
        }

        // ===================================================================================================================================
        // get-functions, which provide (as handy as possible) access to the parameter lists above
        // -----------------------------------------------------------------------------------------------------------------------------------
        protected T GetUniquePar<T>(string name)
        {
            if (uniquePar.ContainsKey(name)) { object p = uniquePar[name]; return (T)p; }
            else return default(T);
        }

        protected List<T> GetNonUniquePar<T>(string name)
        {
            List<T> list = new List<T>();
            if (nonUniquePar.ContainsKey(name))
                foreach (object p in nonUniquePar[name]) list.Add((T)p);
            return list;
        }

        // get value for optional parameter, i.e. either user-choice or default from Common-lib
        protected bool GetParBoolValueOrDefault(string funName, string parName) { return GetParValueOrDefault<bool, ParBool>(funName, parName, BNT.BOOL); }
        protected double GetParNumberValueOrDefault(string funName, string parName) { return GetParValueOrDefault<double, ParNumber>(funName, parName, BNT.NUM); }
        protected string GetParBaseValueOrDefault(string funName, string parName) { return GetParValueOrDefault<string, ParBase>(funName, parName, BNT.TEXT); }
        private enum BNT { BOOL, NUM, TEXT };
        private T GetParValueOrDefault<T, parT>(string funName, string parName, BNT boolNumText)
        {
            parT par = GetUniquePar<parT>(parName);
            if (par == null) return DefinitionAdmin.GetParDefault<T>(funName, parName);
            switch (boolNumText)
            {
                case BNT.BOOL: return (T)((par as ParBool).GetBoolValue() as object);
                case BNT.NUM: return (T)((par as ParNumber).GetValue() as object);
                case BNT.TEXT: return (T)((par as ParBase).xmlValue as object);
            }
            throw new Exception("GetParValueOrDefault failed");
        }

        // get all non-group placeholder parameters of the function
        protected Dictionary<string, T> GetPlaceholderPar<T>()
        {
            Dictionary<string, T> dic = new Dictionary<string, T>();
            foreach (var p in placeholderPar) dic.Add(p.Key, (T)(object)p.Value);
            return dic;
        }

        // usage is best illustrated by example: i.e. components of BenCalc
        // outer (sorted) List: 1 to many components of the BenCalc, sorting is irrelevant here, but relevant e.g. for SchedCalc
        // inner List: parameters of one component (Comp_Cond/Comp_perTU/...)
        protected SortedList<int, List<ParBase>> GetParGroups(string group)
        {
            if (groupPar.ContainsKey(group)) return groupPar[group];
            else return new SortedList<int, List<ParBase>>();
        }

        // again best explained by example: example of GetParGroups continued:
        // to filter out e.g. Comp_Cond of an element of the inner List retrieved by GetParGroups,
        // one needs to consult the fun.description for assessing the parameter-name 
        // (optimally one would not have a List, but a Dictionary<name/par>, but due to groups with non-unique
        // parameters this is not possible (e.g. Uprate agg-group has several agg-parts)
        protected static T GetUniqueGroupPar<T>(string name, List<ParBase> parGroup)
        {
            foreach (ParBase p in parGroup)
                if (p.description.GetParName().ToLower() == name.ToLower())
                    return (T)(object)p;
            return default(T);
        }

        // used for e.g. AggVarPart of DefUprate
        protected static List<T> GetNonUniqueGroupPar<T>(string name, List<ParBase> parGroup)
        {
            List<T> list = new List<T>();
            foreach (ParBase p in parGroup) // see description above for selection
                if (p.description.GetParName().ToLower() == name.ToLower())
                    list.Add((T)(object)p);
            return list;
        }

        // this extracts the placeholder parameters of the group (see below for example)       
        // note: for DefVar one will use GetUniquePlaceholderGroupPar, but DefUprate may have more than one placeholder in a group
        // (though not too likely), example: cond(grp1)={dgn=1}, var(grp1)=yem, var(grp1)=yse, factor(grp1)=1.2, cond(grp2)={dgn=0} ...
        protected static List<T> GetPlaceholderGroupPar<T>(List<ParBase> parGroup)
        {
            List<T> phs = new List<T>();
            foreach (ParBase p in parGroup)
                if (p.description.isPlaceholder)
                    phs.Add((T)(object)p);
            return phs;
        }

        // this extracts the single placeholder parameter of the group (see example above)
        // example: a DefVar-group can consist of an IsGlobal-, an IsMonetary- and a placeholder-parameter (i.e. the name of the variable)
        //          this function delivers the placeholder-parameter (of parGroup)
        // note: isUnique allows function to complain if there is more than one placeholder-param (e.g. FunDefVar)
        protected static T GetUniquePlaceholderGroupPar<T>(List<ParBase> parGroup, out bool isUnique)
        {
            List<T> phs = GetPlaceholderGroupPar<T>(parGroup);
            isUnique = phs.Count == 1;
            return phs.Count == 0 ? default(T) : phs[0];
        }

        // examples: name="LowLim", numbers={3}; name="AgeMax", numbers={1};
        // unlikely but possible: name="Level", numbers={2,7} (could come from yem#2#7, where e.g. 7 is shared with another operator, but 2 not)
        internal T GetFootnotePar<T>(string name, List<string> numbers)
        {
            foreach (string n in numbers)
            {
                if (footnotePar.ContainsKey(n) && footnotePar[n].ContainsKey(name))
                {
                    if (!footnoteUsage.ContainsKey(n)) footnoteUsage.Add(n, new List<string>());
                    footnoteUsage[n].Add(name);
                    return (T)(object)footnotePar[n][name];
                }
            }
            return default(T);
        }

        internal int GetFootnoteParCount(string number)
        {
            return footnotePar.ContainsKey(number) ? footnotePar[number].Count : 0;
        }

        // ===================================================================================================================================
        // get-functions, for test purposes only (because the lists are private)
        // -----------------------------------------------------------------------------------------------------------------------------------
        internal Dictionary<string, ParBase> TestGetUniquePar() { return uniquePar; }
        internal Dictionary<string, List<ParBase>> TestGetNonUniquePar() { return nonUniquePar; }
        internal Dictionary<string, SortedList<int, List<ParBase>>> TestGetGroupPar() { return groupPar; }
        internal Dictionary<string, Dictionary<string, ParBase>> TestGetFootnotePar() { return footnotePar; }
        internal Dictionary<string, ParBase> TestGetPlaceholderPar() { return placeholderPar; }
        internal List<ParBase> TestGetAllPar(bool includeFootnotes) { return GetPlainParList(includeFootnotes); }
    }
}
