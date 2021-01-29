namespace EM_XmlHandler
{
    public static partial class TAGS
    {
        // CONFIGURATION FILE TAGS - BASICS
        public const string CONFIG_PATH_EUROMODFILES = "PATH_EUROMODFILES";
        public const string CONFIG_PATH_DATA = "PATH_DATA";
        public const string CONFIG_PATH_OUTPUT = "PATH_OUTPUT";
        public const string CONFIG_PATH_GLOBAL = "PATH_GLOBAL";
        public const string CONFIG_COUNTRY = "COUNTRY";
        public const string CONFIG_ID_DATA = "ID_DATASET";
        public const string CONFIG_ID_SYSTEM = "ID_SYSTEM";
        public const string CONFIG_DATE_EXCHANGE_RATE = "DATEEXCHANGE_RATE";
        public const string CONFIG_ADDON = "ADDON";
        public const string CONFIG_PATH_PAR_MODIFICATIONS = "PATH_PAR_MODIFICATIONS";
        public const string CONFIG_STRING_PAR_MODIFICATIONS = "STRING_PAR_MODIFICATIONS";
        public const string CONFIG_USE_FINAL_EM3_PATHS = "USE_FINAL_EM3_PATHS";

        // CONFIGURATION FILE TAGS WHICH INFLUENCE BEHAVIOUR
        public const string CONFIG_FORCE_SEQUENTIAL_RUN = "FORCE_SEQUENTIAL_RUN";               // useful when running multiple systems in parallel
        public const string CONFIG_FORCE_SEQUENTIAL_OUTPUT = "FORCE_SEQUENTIAL_OUTPUT";         // useful when available memory is an issue
        public const string CONFIG_FORCE_AUTO_SEQUENTIAL_RUN = "FORCE_AUTO_SEQUENTIAL_RUN";     // for testing purposes only!
        public const string CONFIG_FIRST_HH = "FIRST_HH";
        public const string CONFIG_LAST_HH = "LAST_HH";
        public const string CONFIG_FIRST_N_HH_ONLY = "FIRST_N_HH_ONLY";
        public const string CONFIG_IGNORE_PRIVATE = "IGNORE_PRIVATE";
        public const string CONFIG_EXTENSION_SWITCH = "EXTENSION_SWITCH";
        public const string CONFIG_MAX_RUNTIME_ERRORS = "MAX_RUNTIME_ERRORS";
        public const string CONFIG_WARN_ABOUT_USELESS_GROUPS = "WARN_ABOUT_USELESS_GROUPS";
        public const string CONFIG_INPUT_PASSWORD = "INPUT_PASSWORD";

        // CONFIGURATION FILE TAGS - INFO FOR OUTPUT
        public const string CONFIG_DATE_OUTFILE = "DATE_OUTFILE";
        public const string STDOUTPUT_FILENAME_SUFFIX = "STDOUTPUT_FILENAME_SUFFIX";
        public const string CONFIG_FORCE_OUTPUT_EURO = "FORCE_OUTPUT_EURO";
        public const string CONFIG_RETURN_OUTPUT_IN_MEMORY = "RETURN_OUTPUT_IN_MEMORY";

        // EM2-CONFIGURATION FILE TAGS
        public const string EM2CONFIG_ERRLOG_FILE = "ERRLOG_FILE";
        public const string EM2CONFIG_LOG_WARNINGS = "LOG_WARNINGS";
        public const string EM2CONFIG_EMVERSION = "EMVERSION";
        public const string EM2CONFIG_UIVERSION = "UIVERSION";
        public const string EM2CONFIG_PARAMPATH = "PARAMPATH";
        public const string EM2CONFIG_CONFIGPATH = "CONFIGPATH";
        public const string EM2CONFIG_OUTPUTPATH = "OUTPUTPATH";
        public const string EM2CONFIG_DATAPATH = "DATAPATH";
        public const string EM2CONFIG_HEADER_DATE = "HEADER_DATE";
        public const string EM2CONFIG_OUTFILE_DATE = "OUTFILE_DATE";
        public const string EM2CONFIG_LOG_RUNTIME = "LOG_RUNTIME";
        public const string EM2CONFIG_DECSIGN_PARAM = "DECSIGN_PARAM";
        public const string EM2CONFIG_STARTHH = "STARTHH";
        public const string EM2CONFIG_LASTHH = "LASTHH";
        public const string EM2CONFIG_COUNTRY_FILE = "COUNTRY_FILE";
        public const string EM2CONFIG_DATACONFIG_FILE = "DATACONFIG_FILE";
        public const string EM2CONFIG_LAST_RUN = "LAST_RUN";
        public const string EM2CONFIG_DATASET_ID = "DATASET_ID";
        public const string EM2CONFIG_SYSTEM_ID = "SYSTEM_ID";
        public const string EM2CONFIG_POLICY_SWITCH = "POLICY_SWITCH";
        public const string EM2CONFIG_EMCONTENTPATH = "EMCONTENTPATH";
        public const string EM2CONFIG_ISPUBLICVERSION = "ISPUBLICVERSION";
        public const string EM2CONFIG_IGNORE_PRIVATE = "IGNORE_PRIVATE";
        public const string EM2CONFIG_FIRST_N_HH_ONLY = "FIRST_N_HH_ONLY";
        public const string EM2CONFIG_EXCHANGE_RATE_DATE = "EXCHANGE_RATE_DATE";
        public const string EM2CONFIG_EMCONFIG = "EMConfig";
        public const string EM2CONFIG_ADDON = "ADDON";

        public const string EM2CONFIG_errLogFileName = "_errlog.txt";
        public const string EM2CONFIG_errLogAddOnFileName = "_errlog_addon.txt";
        public const string EM2CONFIG_defaultHHID = "-1";
        public const string EM2CONFIG_labelEMConfig = "EMConfig";
    }
}
