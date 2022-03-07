using System.Collections.Generic;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        public class PackageLabels
        {
            public string packageKey = null;
            public string baseSystemLabel = string.Empty;
            public List<string> reformSystemLabels = new List<string>();
        }
        public static List<PackageLabels> labels = null; // can be set by api (SetLabels) to replace system-names

        public const string PRETTY_INFO_BASE_COUNTRY = "[baseCountry]"; // alias of [country] for downwards-compatibility
        public const string PRETTY_INFO_BASE_TAX_SYS_PRETTY = "[baseTaxSysPretty]";
        public const string PRETTY_INFO_REFORM_TAX_SYS_PRETTY = "[reformTaxSysPretty]";

        public const string PRETTY_INFO_BASE_SYS = "[baseSys]";
        public const string PRETTY_INFO_BASE_SYS_PRETTY = "[baseSysPretty]";
        public const string PRETTY_INFO_BASE_SYS_LABEL = "[baseSysLabel]";
        public const string PRETTY_INFO_BASE_FILENAME = "[baseFileName]";
        public const string PRETTY_INFO_BASE_DATASET = "[baseDataset]";
        public const string PRETTY_INFO_BASE_RUNLOG = "[baseRunLog]";
        public const string PRETTY_INFO_BASE_RUNLOG_EXTENDED = "[baseRunLogExtended]";
        public const string PRETTY_INFO_BASE_RUN_WARNINGS = "[baseRunWarnings]";
        public const string PRETTY_INFO_BASE_RUN_EXTENSION_SWITCHES = "[baseRunExtensionSwitches]";
        public const string PRETTY_INFO_BASE_RUN_ADDONS = "[baseRunAddons]";
        public const string PRETTY_INFO_BASE_RUN_DURATION = "[baseRunDuration]";
        public const string PRETTY_INFO_BASE_RUN_START_TIME = "[baseRunStartTime]";
        public const string PRETTY_INFO_BASE_RUN_END_TIME = "[baseRunEndTime]";

        public const string PRETTY_INFO_REF_SYS = "[refSys]";
        public const string PRETTY_INFO_REF_SYS_PRETTY = "[refSysPretty]";
        public const string PRETTY_INFO_REF_SYS_LABEL = "[refSysLabel]";
        public const string PRETTY_INFO_REF_FILENAME = "[refFileName]";
        public const string PRETTY_INFO_REF_DATASET = "[refDataset]";
        public const string PRETTY_INFO_REF_RUN_LOG = "[refRunLog]";
        public const string PRETTY_INFO_REF_RUN_LOG_EXTENDED = "[refRunLogExtended]";
        public const string PRETTY_INFO_REF_RUN_WARNINGS = "[refRunWarnings]";
        public const string PRETTY_INFO_REF_RUN_EXTENSION_SWITCHES = "[refRunExtensionSwitches]";
        public const string PRETTY_INFO_REF_RUN_ADDONS = "[refRunAddons]";
        public const string PRETTY_INFO_REF_RUN_DURATION = "[refRunDuration]";
        public const string PRETTY_INFO_REF_RUN_START_TIME = "[refRunStartTime]";
        public const string PRETTY_INFO_REF_RUN_END_TIME = "[refRunEndTime]";

        public const string PRETTY_INFO_REF_SYS_LIST = "[refSysList]";
        public const string PRETTY_INFO_REF_FILE_LIST = "[refFileList]";

        private static List<PrettyInfo> allPrettyInfos = new List<PrettyInfo>()
        {
            new PrettyInfo_BaseCountry(),
            new PrettyInfo_BaseSys(),
            new PrettyInfo_BaseSysPretty(),
            new PrettyInfo_BaseSysLabel(),
            new PrettyInfo_BaseTaxSysPretty(),
            new PrettyInfo_RefTaxSysPretty(),
            new PrettyInfo_BaseFileName(),
            new PrettyInfo_BaseDataset(),
            new PrettyInfo_BaseRunLog(),
            new PrettyInfo_BaseRunLogExtended(),
            new PrettyInfo_BaseRunWarnings(),
            new PrettyInfo_BaseRunExtensionSwitches(),
            new PrettyInfo_BaseRunAddons(),
            new PrettyInfo_BaseRunDuration(),
            new PrettyInfo_BaseRunStartTime(),
            new PrettyInfo_BaseRunEndTime(),

            new PrettyInfo_RefSys(),
            new PrettyInfo_RefSysPretty(),
            new PrettyInfo_RefSysLabel(),
            new PrettyInfo_RefFileName(),
            new PrettyInfo_RefDataset(),
            new PrettyInfo_RefRunLog(),
            new PrettyInfo_RefRunLogExtended(),
            new PrettyInfo_RefRunWarnings(),
            new PrettyInfo_RefRunExtensionSwitches(),
            new PrettyInfo_RefRunAddons(),
            new PrettyInfo_RefRunDuration(),
            new PrettyInfo_RefRunStartTime(),
            new PrettyInfo_RefRunEndTime(),

            new PrettyInfo_RefSysList(),
            new PrettyInfo_RefFileList(),

            new PrettyInfo_UserVar()
        };
    }
}
