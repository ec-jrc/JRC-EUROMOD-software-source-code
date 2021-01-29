using System.Collections.Generic;

namespace EM_Common
{
    public class DefPol
    {
        public const string SPECIAL_POL_UPRATE = "Uprate";
        public const string SPECIAL_POL_CONSTDEF = "ConstDef";
        public const string SPECIAL_POL_ILDEF = "ILDef";
        public const string SPECIAL_POL_ILSDEF = "ILSDef";
        public const string SPECIAL_POL_ILSUDBDEF = "ILSUDBDef";
        public const string SPECIAL_POL_TUDEF = "TUDef";
        public const string SPECIAL_POL_OUTPUT_STD = "Output_Std";
        public const string SPECIAL_POL_OUTPUT_STD_HH = "Output_Std_HH";
        public const string SPECIAL_POL_SETDEFAULT = "SetDefault";

        public static List<string> GetStandardOutputPolicies() { return new List<string> { SPECIAL_POL_OUTPUT_STD, SPECIAL_POL_OUTPUT_STD_HH }; }

        public static List<string> GetCompulsoryPolicies()
        {
            return new List<string>
            {
                SPECIAL_POL_UPRATE,
                SPECIAL_POL_CONSTDEF,
                SPECIAL_POL_ILSDEF,
                SPECIAL_POL_TUDEF,
                SPECIAL_POL_OUTPUT_STD,
                SPECIAL_POL_OUTPUT_STD_HH
            };
        }

        public static List<string> GetSystemPolicies()
        {
            return new List<string>
            {
                SPECIAL_POL_UPRATE,
                SPECIAL_POL_CONSTDEF,
                SPECIAL_POL_ILDEF,
                SPECIAL_POL_ILSDEF,
                SPECIAL_POL_TUDEF,
                SPECIAL_POL_OUTPUT_STD,
                SPECIAL_POL_OUTPUT_STD_HH
            };
        }
    }
}
