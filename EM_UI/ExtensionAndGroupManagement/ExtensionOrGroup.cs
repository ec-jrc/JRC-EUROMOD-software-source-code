using EM_UI.DataSets;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionOrGroup
    {
        internal string id = string.Empty;
        internal string name = string.Empty;
        internal string shortName = string.Empty;
        internal LookDef look = new LookDef();
        internal LookDef.STYLE style = LookDef.STYLE.GROUP;

        internal ExtensionOrGroup(CountryConfig.LookGroupRow lookGroupRow)
        {
            id = lookGroupRow.ID;
            name = lookGroupRow.Name;
            shortName = lookGroupRow.ShortName;
            look = new LookDef(lookGroupRow.Look);
        }

        internal ExtensionOrGroup(DataConfig.ExtensionRow extensionRow, LookDef.STYLE _style = LookDef.STYLE.EXTENSION_ON)
        {
            id = extensionRow.ID;
            name = extensionRow.Name;
            shortName = extensionRow.ShortName;
            look = new LookDef(extensionRow.Look);
            style = _style;
        }

        internal ExtensionOrGroup(SwitchablePolicyConfig.SwitchablePolicyRow extensionRow, LookDef.STYLE _style = LookDef.STYLE.EXTENSION_ON)
        {
            id = extensionRow.ID;
            name = extensionRow.LongName;
            shortName = extensionRow.NamePattern;
            look = new LookDef(extensionRow.Look);
            style = _style;
        }

        internal ExtensionOrGroup(GlobLocExtensionRow globLocExtensionRow, LookDef.STYLE _style = LookDef.STYLE.EXTENSION_ON)
        {
            id = globLocExtensionRow.ID;
            name = globLocExtensionRow.Name;
            shortName = globLocExtensionRow.ShortName;
            look = new LookDef(globLocExtensionRow.Look);
            style = _style;
        }

        internal ExtensionOrGroup() { }
    }
}
