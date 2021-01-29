using System.Collections.Generic;

namespace EM_Common
{
    public partial class DefinitionAdmin
    {
        public static Dictionary<string, string> GetPolRelatedFuns(string pol)
        {
            Dictionary<string, string> polRelFun = new Dictionary<string, string>(); // key: name, value: description
            if (pol.ToLower().StartsWith(DefPol.SPECIAL_POL_ILDEF.ToLower()) || pol.ToLower().StartsWith(DefPol.SPECIAL_POL_ILSDEF.ToLower()))
                polRelFun.Add(DefFun.DefIl, GetFunDefinition(DefFun.DefIl).description);
            if (pol.ToLower().StartsWith(DefPol.SPECIAL_POL_TUDEF.ToLower()))
                polRelFun.Add(DefFun.DefTu, GetFunDefinition(DefFun.DefTu).description);
            if (pol.ToLower().StartsWith(DefPol.SPECIAL_POL_OUTPUT_STD.ToLower()) || pol.ToLower().StartsWith(DefPol.SPECIAL_POL_OUTPUT_STD_HH.ToLower()))
                polRelFun.Add(DefFun.DefOutput, GetFunDefinition(DefFun.DefOutput).description);
            if (pol.ToLower().StartsWith(DefPol.SPECIAL_POL_UPRATE.ToLower()))
            {
                polRelFun.Add(DefFun.Uprate, GetFunDefinition(DefFun.Uprate).description);
                polRelFun.Add(DefFun.SetDefault, GetFunDefinition(DefFun.SetDefault).description);
            }
            return polRelFun;
        }

        public static string ParTypeToString(DefPar.PAR_TYPE parType)
        {
            switch (parType) // reproduce the strings formually used by FunctionConfig and still used in CountryConfig.ParameterRow.ValueType
            {
                case DefPar.PAR_TYPE.FORMULA: return "formula";
                case DefPar.PAR_TYPE.CONDITION: return "condition";
                case DefPar.PAR_TYPE.BOOLEAN: return "yes/no";
                case DefPar.PAR_TYPE.NUMBER: return "amount";
                case DefPar.PAR_TYPE.TEXT: return "string";
                case DefPar.PAR_TYPE.VAR: return "variable";
                case DefPar.PAR_TYPE.OUTVAR: return "variable";
                case DefPar.PAR_TYPE.IL: return "incomelist";
                case DefPar.PAR_TYPE.TU: return "taxunit";
                case DefPar.PAR_TYPE.VARorIL: return "variable/incomelist";
                case DefPar.PAR_TYPE.CATEG: return "categorical";
                case DefPar.PAR_TYPE.PLACEHOLDER: return "[Placeholder]";
                default: return string.Empty;
            }
        }
    }
}
