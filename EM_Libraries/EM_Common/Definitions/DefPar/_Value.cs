namespace EM_Common
{
    public static partial class DefPar
    {
        public static class Value
        {
            public const string NA = "n/a";

            public const string YES = "yes";
            public const string NO = "no";

            public const string ON = "on";
            public const string OFF = "off";
            public const string TOGGLE = "toggle";
            public const string SWITCH = "switch";

            public const string DATA_SYS_X = "x";
            public const string DATA_SYS_BEST = "best";

            public const string WHO_NOBODY = "nobody";
            public const string WHO_ONE = "one";
            public const string WHO_ALL = "all";
            public const string WHO_ONE_ADULT = "one_adult";
            public const string WHO_ALL_ADULTS = "all_adults";

            public const string MULTIPLE_PARTNERS_WARN = "warn";
            public const string MULTIPLE_PARTNERS_IGNORE = "ignore";
            public const string MULTIPLE_PARTNERS_ALLOW = "allow";

            public const string TUTYPE_HH = "HH";
            public const string TUTYPE_IND = "IND";
            public const string TUTYPE_SUBGROUP = "SUBGROUP";

            public const string LIMPRI_UPPER = "upper";
            public const string LIMPRI_LOWER = "lower";

            public const string ILVAROP_MUL = "MUL";
            public const string ILVAROP_ADD = "ADD";
            public const string ILVAROP_NEGTOZERO = "NEGTOZERO";
            public const string ILVAROP_ALL = "ALL";
            public const string ILVAROP_MAX = "MAX";
            public const string ILVAROP_MIN = "MIN";
            public const string ILVAROP_MINPOS = "MINPOS";

            public const string AMOUNT = "Amount";

            public const string BENCALC_BASE = "$base";
            public const string BENCALC_BASE_VAR = "base_var";
            public const string BENCALC_BASE_IL = "base_il";
            public const string BENCALC_BASE_AMOUNT = "base_amount";

            public const string HICP = "$hicp";

            public const string EURO = "euro";
            public const string NATIONAL = "national";

            public const string PLACEHOLDER_CC = "=cc=";
            public const string PLACEHOLDER_SYS = "=sys=";

            public const string UNITINFO_HEADID = "HeadID";
            public const string UNITINFO_ISPARTNER = "IsPartner";
            public const string UNITINFO_ISDEPENDENTCHILD = "IsDependentChild";
            public const string UNITINFO_ISDEPCHILD = "IsDepChild";
            public const string UNITINFO_ISOWNCHILD = "IsOwnChild";
            public const string UNITINFO_ISOWNDEPENDENTCHILD = "IsOwnDependentChild";
            public const string UNITINFO_ISOWNDEPCHILD = "IsOwnDepChild";
            public const string UNITINFO_ISDEPPARENT = "IsDepParent";
            public const string UNITINFO_ISDEPRELATIVE = "IsDepRelative";
            public const string UNITINFO_ISLONEPARENT = "IsLoneParent";
            public const string UNITINFO_ISCOHABITING = "IsCohabiting";
            public const string UNITINFO_ISWITHPARTNER = "IsWithPartner";
            public const string UNITINFO_ISPARENT = "IsParent";
            public const string UNITINFO_ISPARENTOFDEPCHILD = "IsParentOfDepChild";
            public const string UNITINFO_ISINEDUCATION = "IsInEducation";
            public const string UNITINFO_ISLOOSEDEPCHILD = "IsLooseDepChild";
            public const string UNITINFO_ISLONEPARENTOFDEPCHILD = "IsLoneParentOfDepChild";

            public const string UNITINFO_NPERSINUNIT = "nPersInUnit";
            public const string UNITINFO_NDEPCHILDRENINTU = "nDepChildrenInTu";
            public const string UNITINFO_NDEPPARENTSINTU = "nDepParentsInTu";
            public const string UNITINFO_NDEPRELATIVESINTU = "nDepRelativesInTu";

            public const string ADDHHMEMBERS_CHILD = "Child";
            public const string ADDHHMEMBERS_PARTNER = "Partner";
            public const string ADDHHMEMBERS_OTHER = "Other";

            public const int NO_COLOR = -1;

            public const string EXTENSION_ALL = "all";

            /// <summary> returns true if xmlVal is set to yes or 1 otherwise false </summary>
            public static bool IsYes(string xmlVal) { return xmlVal.ToLower() == YES || xmlVal == "1"; }

            /// <summary> returns null if xmlVal=n/a, true if xmlVal=on, false otherwise </summary>
            public static bool? IsOn(string xmlVal)
            {
                switch (xmlVal.ToLower())
                {
                    case ON: return true;
                    case OFF: return false;
                    default: return null; // n/a or invalid
                }
            }

            /// <summary> returns true if xmlVal is set to 'euro' </summary>
            public static bool IsEuro(string xmlVal) { return xmlVal.ToLower() == EURO; }
        }
    }
}
