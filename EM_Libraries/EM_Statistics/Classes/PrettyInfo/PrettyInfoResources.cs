using System.Collections.Generic;

namespace EM_Statistics
{
    internal static partial class PrettyInfoProvider
    {
        private class PrettyInfoResources
        {
            internal PrettyInfoResources(Template.TemplateInfo _templateInfo,
                                         SystemInfo _baseSystem, List<SystemInfo> _reformSystems,
                                         string _packageKey, int _refNo)
            {
                templateInfo = _templateInfo;
                baseSystem = _baseSystem; reformSystems = _reformSystems;
                packageKey = _packageKey; refNo = _refNo;
            }
            internal readonly Template.TemplateInfo templateInfo = null;
            internal readonly SystemInfo baseSystem = null;
            internal readonly List<SystemInfo> reformSystems = null;
            internal readonly string packageKey = null;
            internal readonly int refNo = -1;
        }
    }
}
