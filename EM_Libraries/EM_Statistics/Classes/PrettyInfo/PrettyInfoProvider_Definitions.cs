using System.Collections.Generic;

namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private const string PRETTY_INFO_BASE_COUNTRY = "[baseCountry]"; // alias of [country] for downwards-compatibility

        private const string PRETTY_INFO_BASE_SYS = "[baseSys]";
        private const string PRETTY_INFO_BASE_SYS_PRETTY = "[baseSysPretty]";
        private const string PRETTY_INFO_BASE_FILENAME = "[baseFileName]";
        private const string PRETTY_INFO_BASE_DATASET = "[baseDataset]";
        private const string PRETTY_INFO_BASE_RUNLOG = "[baseRunLog]";
        private const string PRETTY_INFO_BASE_RUNLOG_EXTENDED = "[baseRunLogExtended]";
        private const string PRETTY_INFO_BASE_RUN_WARNINGS = "[baseRunWarnings]";

        private const string PRETTY_INFO_REF_SYS = "[refSys]";
        private const string PRETTY_INFO_REF_SYS_PRETTY = "[refSysPretty]";
        private const string PRETTY_INFO_REF_FILENAME = "[refFileName]";
        private const string PRETTY_INFO_REF_DATASET = "[refDataset]";
        private const string PRETTY_INFO_REF_RUN_LOG = "[refRunLog]";
        private const string PRETTY_INFO_REF_RUN_LOG_EXTENDED = "[refRunLogExtended]";
        private const string PRETTY_INFO_REF_RUN_WARNINGS = "[refRunWarnings]";

        private const string PRETTY_INFO_REF_SYS_LIST = "[refSysList]";

        private static List<PrettyInfo> allPrettyInfos = new List<PrettyInfo>()
        {
            new PrettyInfo_BaseCountry(),
            new PrettyInfo_BaseSys(),
            new PrettyInfo_BaseSysPretty(),
            new PrettyInfo_BaseFileName(),
            new PrettyInfo_BaseDataset(),
            new PrettyInfo_BaseRunLog(),
            new PrettyInfo_BaseRunLogExtended(),
            new PrettyInfo_BaseRunWarnings(),

            new PrettyInfo_RefSys(),
            new PrettyInfo_RefSysPretty(),
            new PrettyInfo_RefFileName(),
            new PrettyInfo_RefDataset(),
            new PrettyInfo_RefRunLog(),
            new PrettyInfo_RefRunLogExtended(),
            new PrettyInfo_RefRunWarnings(),

            new PrettyInfo_RefSysList(),

            new PrettyInfo_UserVar()
        };
    }
}
