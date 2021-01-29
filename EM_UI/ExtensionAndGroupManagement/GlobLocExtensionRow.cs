using EM_UI.DataSets;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class GlobLocExtensionRow
    {
        internal string ID;
        internal string Name;
        internal string ShortName;
        internal string Look;

        internal GlobLocExtensionRow(SwitchablePolicyConfig.SwitchablePolicyRow swpr)
        {
            ID = swpr.ID; Name = swpr.LongName; ShortName = swpr.NamePattern; Look = swpr.Look;
        }

        internal GlobLocExtensionRow(DataConfig.ExtensionRow er)
        {
            ID = er.ID; Name = er.Name; ShortName = er.ShortName; Look = er.Look;
        }
    }
}
