using System.Collections.Generic;

namespace EM_Common
{
    public class DefQuery
    {
        public const string IsHeadOfTu = "IsHeadOfTu";
        public const string IsPartner = "IsPartner";
        public const string IsDepChild = "IsDepChild";
        public const string IsOwnChild = "IsOwnChild";
        public const string IsOwnDepChild = "IsOwnDepChild";
        public const string IsLooseDepChild = "IsLooseDepChild";
        public const string IsDepParent = "IsDepParent";
        public const string IsDepRelative = "IsDepRelative";
        public const string IsLoneParentOfDepChild = "IsLoneParentOfDepChild";
        public const string nDepChildrenOfCouple = "nDepChildrenOfCouple";
        public const string nChildrenOfCouple = "nChildrenOfCouple";
        public const string nPersInUnit = "nPersInUnit";
        public const string nAdultsInTu = "nAdultsInTu";
        public const string nDepChildrenInTu = "nDepChildrenInTu";
        public const string IsNtoMchild = "IsNtoMchild";
        public const string HasMaxValInTu = "HasMaxValInTu";
        public const string HasMinValInTu = "HasMinValInTu";
        public const string GetPartnerIncome = "GetPartnerIncome";
        public const string GetCoupleIncome = "GetCoupleIncome";
        public const string GetParentsIncome = "GetParentsIncome";
        public const string GetMotherIncome = "GetMotherIncome";
        public const string GetFatherIncome = "GetFatherIncome";
        public const string GetOwnChildrenIncome = "GetOwnChildrenIncome";
        public const string IsUsedDatabase = "IsUsedDatabase";
        public const string IsMarried = "IsMarried";
        public const string IsCohabiting = "IsCohabiting";
        public const string IsWithPartner = "IsWithPartner";
        public const string IsInEducation = "IsInEducation";
        public const string IsDisabled = "IsDisabled";
        public const string IsCivilServant = "IsCivilServant";
        public const string IsBlueColl = "IsBlueColl";
        public const string IsParent = "IsParent";
        public const string IsParentOfDepChild = "IsParentOfDepChild";
        public const string IsLoneParent = "IsLoneParent";
        public const string nLooseDepChildrenInTu = "nLooseDepChildrenInTu";
        public const string nDepParentsInTu = "nDepParentsInTu";
        public const string nDepRelativesInTu = "nDepRelativesInTu";
        public const string nDepParentsAndRelativesInTu = "nDepParentsAndRelativesInTu";
        public const string GetSystemYear = "GetSystemYear";
        public const string GetDataIncomeYear = "GetDataIncomeYear";
        public const string IsOutputCurrencyEuro = "IsOutputCurrencyEuro";
        public const string IsParamCurrencyEuro = "IsParamCurrencyEuro";
        public const string IsDataVariable = "IsDataVariable";
        public const string GetExchangeRate = "GetExchangeRate";
        public const string rand = "rand";


        public const string HAS_PAR_MARKER = "#x"; // added in formula to link query to its parameters (e.g. in formula: IsNtoMChild#3, paramters with group 3: #_N, #_M)

        public class Par
        {
            public const string AgeMin = "#_AgeMin";
            public const string AgeMax = "#_AgeMax";
            public const string N = "#_n";
            public const string M = "#_m";
            public const string Income = "#_Income";
            public const string Info = "#_Info";
            public const string DataBasename = "#_DataBasename";
            public const string Val = "#_Val";
            public const string Adults_Only = "#_Adults_Only";
            public const string Unique = "#_Unique";
            public const string VariableName = "#_VariableName";
        }

        public static void AddAllPar(DefinitionAdmin.Fun fun)
        {
            fun.par.AddRange(GetAgePar());
            fun.par.AddRange(GetNMPar(optional: true));
            fun.par.AddRange(GetIncomePar(optional: true));
            fun.par.AddRange(GetHasMinMaxPar(optional: true));
            fun.par.AddRange(GetDBNamePar(optional: true));
            fun.par.AddRange(GetVariableNamePar(optional: true));
        }

        internal static Dictionary<string, DefinitionAdmin.Par> GetAgePar()
        {
            return new Dictionary<string, DefinitionAdmin.Par>()
            {
                { Par.AgeMin, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = 0.0,
                    isFootnote = true,
                    description = "Parameter of several queries (e.g. nDepChildrenInTu)."
                } },
                { Par.AgeMax, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.NUMBER,
                    minCount = 0, maxCount = 1, defaultValue = DefinitionAdmin.POS_INFINITE,
                    isFootnote = true,
                    description = "Parameter of several queries (e.g. nDepChildrenInTu)."
                } }
            };
        }

        internal static Dictionary<string, DefinitionAdmin.Par> GetNMPar(bool optional) // optional for fun-def, but compulsory for query-def
        {
            return new Dictionary<string, DefinitionAdmin.Par>()
            {
                { Par.N, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.NUMBER,
                    minCount = optional ? 0 : 1, maxCount = 1,
                    isFootnote = true,
                    description = "Parameter of query IsNtoMchild."
                } },
                { Par.M, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.NUMBER,
                    minCount = optional ? 0 : 1, maxCount = 1,
                    isFootnote = true,
                    description = "Parameter of query IsNtoMchild."
                } }
            };
        }

        internal static Dictionary<string, DefinitionAdmin.Par> GetIncomePar(bool optional)
        {
            return new Dictionary<string, DefinitionAdmin.Par>()
            {
                { Par.Income, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.VARorIL,
                    minCount = optional ? 0 : 1, maxCount = 1,
                    substitutes = new List<string>() { Par.Info }, isFootnote = true,
                    description = "Parameter of several queries (e.g. GetPartnerIncome)." } },
                { Par.Info, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.VARorIL,
                    minCount = 0, maxCount = 1,
                    substitutes = new List<string>() { Par.Income }, isFootnote = true,
                    description = "Parameter of several queries (e.g. GetPartnerInfo)." } }
            };
        }

        internal static Dictionary<string, DefinitionAdmin.Par> GetDBNamePar(bool optional)
        {
            return new Dictionary<string, DefinitionAdmin.Par>()
            {
                { Par.DataBasename, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.TEXT,
                    minCount = optional ? 0 : 1, maxCount = 1,
                    isFootnote = true,
                    description = "Parameter of query IsUsedDatabase."
                } }
            };
        }

        internal static Dictionary<string, DefinitionAdmin.Par> GetVariableNamePar(bool optional)
        {
            return new Dictionary<string, DefinitionAdmin.Par>()
            {
                { Par.VariableName, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.TEXT,
                    minCount = optional ? 0 : 1, maxCount = 1,
                    isFootnote = true,
                    description = "Parameter of query IsDataVariable."
                } }
            };
        }

        // DatabaseName & VariableName are actually the only global query-parameters, and thus if functions only allow for RunCond
        // (and not for other common parameters) they usually will allow only for these query-parameters
        internal static void AddGlobalQueryParams(DefinitionAdmin.Fun fun)
        {
            fun.par.AddRange(GetDBNamePar(optional: true));
            fun.par.AddRange(GetVariableNamePar(optional: true));
        }

        internal static Dictionary<string, DefinitionAdmin.Par> GetHasMinMaxPar(bool optional)
        {
            return new Dictionary<string, DefinitionAdmin.Par>()
            {
                { Par.Val, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.VARorIL,
                    minCount = optional ? 0 : 1, maxCount = 1,
                    isFootnote = true,
                    description = "Parameter of query HasMaxValInTu."
                } },
                { Par.Unique, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false,
                    isFootnote = true,
                    description = "Parameter of query HasMaxValInTu."
                } },
                { Par.Adults_Only, new DefinitionAdmin.Par() {
                    valueType = DefPar.PAR_TYPE.BOOLEAN,
                    minCount = 0, maxCount = 1, defaultValue = false,
                    isFootnote = true, 
                    description = "Parameter of query HasMaxValInTu."
                } }
            };
        }
    }
}
