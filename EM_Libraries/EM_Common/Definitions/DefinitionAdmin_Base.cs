using System;
using System.Collections.Generic;

namespace EM_Common
{
    public partial class DefinitionAdmin
    {
        public const int MANY = 1000; // used to define non-unique parameters
        public static double POS_INFINITE { get { return double.MaxValue; } }
        public static double NEG_INFINITE { get { return -double.MaxValue; } }

        // THESE STRUCTURES CONTAIN ALL THE DEFINITIONS
        private static Dictionary<string, Fun> funDefs = null;
        private static Dictionary<string, Query> queryDefs = null;

        /// <summary> creates the (constant) function- and query-definitions </summary>
        public static void Init()
        {
            lock (initLock)
            {
                if (funDefs == null)
                {
                    DefineFun();        // fills funDefs
                    DefineQueries();    // fills queryDefs
                }
            }
        }
        private static object initLock = new object();

        /// <summary>
        /// get default value of an optional parameter (e.g. #_UpLim -> POS_INFINITE)
        /// note that only 'legal' calls are assumed, therefore throw exceptions otherwise (is a programming error)
        /// </summary>
        public static T GetParDefault<T>(string funName, string parName)
        {
            Fun fun = GetFunDefinition(funName); if (fun == null) ReportIllegal();
            Par par = fun.GetParDef(parName);
            if (par == null) par = fun.GetGroupParDef(parName, out string dummy);
            if (par == null) { ReportIllegal(); return default(T); }

            T defaultValue;
            try { defaultValue = (T)par.defaultValue; }  catch(Exception ex) { ReportIllegal(ex.Message); }
            return defaultValue;

            void ReportIllegal(string exMess = null)
            {
                throw new Exception(
                    $"Illegal call of DefinitionAdmin.GetParDefault (funName={funName}, parName={parName}" +
                    exMess == null ? string.Empty : $"{Environment.NewLine}{exMess}");
            }
        }
    }
}
