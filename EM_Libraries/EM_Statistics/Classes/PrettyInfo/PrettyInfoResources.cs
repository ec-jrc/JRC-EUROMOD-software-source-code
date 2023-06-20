using System.Collections.Generic;

namespace EM_Statistics
{
    public static partial class PrettyInfoProvider
    {
        private class PrettyInfoResources
        {
            internal PrettyInfoResources(Template.TemplateInfo _templateInfo,
                                         List<SystemInfo> _baseSystems, List<SystemInfo> _reformSystems,
                                         string _packageKey, int _refNo)
            {
                templateInfo = _templateInfo;
                baseSystems = _baseSystems; reformSystems = _reformSystems;
                packageKey = _packageKey; refNo = _refNo;
            }
            internal readonly Template.TemplateInfo templateInfo = null;
            internal readonly List<SystemInfo> baseSystems = null;
            internal readonly List<SystemInfo> reformSystems = null;
            internal readonly string packageKey = null;
            internal readonly int refNo = -1;
        }
    }
}
