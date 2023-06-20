using System;

namespace EM_XmlHandler
{
    public static partial class TAGS
    {
        // GENERAL TAGS
        public const string ROOT_ELEMENT = "RootElement";
        public const string ID = "ID";
        public const string NAME = "Name";
        public const string SHORT_NAME = "ShortName";       // used e.g in <COUNTRY>, <EXTENSION>
        public const string SYS_NAME = "SysName";           // used e.g. in <EXTENSION>, <EXRATE>
        public const string PRIVATE = "Private";            // used e.g. <COUNTRY>

        // COUNTRY FILE - MAIN TAGS
        public const string COUNTRY = "COUNTRY";
        public const string SYS = "SYS";
        public const string POL = "POL";
        public const string REFPOL = "REFPOL";
        public const string FUN = "FUN";
        public const string PAR = "PAR";
        public const string SYS_POL = "SYS_POL";
        public const string SYS_FUN = "SYS_FUN";
        public const string SYS_PAR = "SYS_PAR";
        public const string UPIND = "UPIND";
        public const string UPIND_YEAR = "UPIND_YEAR"; // uprating index year values
        public const string EXSTAT = "EXSTAT";
        public const string EXSTAT_YEAR = "EXSTAT_YEAR"; // uprating index year values
        public const string DATA = "DATA";
        public const string SYS_DATA = "SYS_DATA";
        public const string LOOKGROUP = "LOOKGROUP";
        public const string LOOKGROUP_POL = "LOOKGROUP_POL";
        public const string LOOKGROUP_FUN = "LOOKGROUP_FUN";
        public const string LOOKGROUP_PAR = "LOOKGROUP_PAR";
        public const string LOCAL_EXTENSION = "EXTENSION";
        public const string EXTENSION_POL = "EXTENSION_POL";
        public const string EXTENSION_FUN = "EXTENSION_FUN";
        public const string EXTENSION_PAR = "EXTENSION_PAR";
        public const string EXTENSION_SWITCH = "EXTENSION_SWITCH";
        public const string INDTAX = "INDTAX";
        public const string INDTAX_YEAR = "INDTAX_YEAR"; // inderct tax year values

        // COUNTRY FILE - SUB TAGS - IDs
        public const string SYS_ID = "SysID";
        public const string POL_ID = "PolID";
        public const string REFPOL_ID = "RefPolID";
        public const string FUN_ID = "FunID";
        public const string PAR_ID = "ParID";
        public const string UPIND_ID = "UpIndID";
        public const string EXSTAT_ID = "ExStatID";
        public const string DATA_ID = "DataID";
        public const string LOOKGROUP_ID = "LookGroupID";
        public const string EXTENSION_ID = "ExtensionID";
        public const string EXENTSION_BASEOFF = "BaseOff";
        public const string INDTAX_ID = "IndTaxID";

        // COUNTRY FILE - SUB TAGS - OTHER
        public const string ORDER = "Order";
        public const string SWITCH = "Switch";
        public const string GROUP = "Group";
        public const string VALUE = "Value";
        public const string NUMBER = "Number";
        public const string LEVEL = "Level";
        public const string RUN_OPTION = "RunOption";
        public const string YEAR = "Year"; // used in <UPIND_YEAR>, <EXSTAT_YEAR>
        public const string PRIVATE_MAIN = "PRIVATE_MAIN"; // actually no tag, but the <Name> of the currently only private-group
        public const string HEAD_DEF_INC = "HeadDefInc";
        public const string CURRENCY_PARAM = "CurrencyParam";
        public const string CURRENCY_OUTPUT = "CurrencyOutput";
        public const string CURRENCY_DATA = "Currency";
        public const string FILEPATH_DATA = "FilePath";
        public const string YEAR_INC = "YearInc";
        public const string USE_COMMON_DEFAULT = "UseCommonDefault";
        public const string READ_X_VARIABLES = "ReadXVariables";
        public const string LIST_STRING_OUTVAR = "ListStringOutVar";
        public const string INDIRECT_TAX_TABLE_YEAR = "IndirectTaxTableYear";
        public const string BEST_MATCH = "BestMatch";
        public const string COMMENT = "Comment";
        public const string TABLENAME = "TableName";    // used in <EXSTAT> 

        // VARIABLES FILE - MAIN TAGS
        public const string VAR = "VAR";
        public const string ACTYPE = "ACTYPE";
        public const string ACLEVEL = "ACLEVEL";
        public const string ACRO = "ACRO";
        public const string CAT = "CAT";
        // VARIABLES FILE - SUB TAGS
        public const string ACRO_ID = "AcroID";
        public const string MONETARY = "Monetary";
        public const string LABELS = "Labels";

        // GLOBAL FILE TAGS - MAIN TAGS
        public const string INFO = "INFO";
        public const string EXRATE = "EXRATE";
        public const string HICP = "HICP";
        public const string EXTENSION = "EXTENSION";
        // GLOBAL FILE - SUB TAGS
        public const string EXRATE_JUNE30 = "June30";
        public const string EXRATE_AVERAGE = "YearAverage";
        public const string EXRATE_1stSEMESTER = "FirstSemester";
        public const string EXRATE_2ndSEMESTER = "SecondSemester";
        public const string EXRATE_DEFAULT = "Default";
        public const string EXRATE_COUNTRY = "Country";

        // get the name of the outer tag, e.g. SYSs for SYS ...
        public static string Enclosure(string tag) { return tag + "s"; }

        // ... and the other way round
        public static string Enclosed(string tag)
        {
            if (!tag.EndsWith("s")) throw new Exception($"TAGS.Enclosed: Failed to assess tag. Enclosing tag {tag} does not end with 's'");
            return tag.Substring(0, tag.Length - 1);
        }
    }
}
