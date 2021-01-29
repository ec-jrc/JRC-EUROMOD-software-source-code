using EM_UI.CountryAdministration;
using EM_UI.Dialogs;

namespace EM_UI.Actions
{
    internal class SaveAsAction: BaseAction
    {
        string origShortName, copyShortName, copyLongName, flagPathAndFileName;
        SaveAsAdaptOptions saveAsAdaptOptions;

        internal SaveAsAction(string _origShortName, string _copyShortName, string _copyLongName, string _flagPathAndFileName,
                              SaveAsAdaptOptions _saveAsAdaptOptions)
        {
            origShortName = _origShortName;
            copyShortName = _copyShortName;
            copyLongName = _copyLongName;
            flagPathAndFileName = _flagPathAndFileName;
            saveAsAdaptOptions = _saveAsAdaptOptions;
        }

        internal override void PerformAction()
        {
            CountryAdministrator.SaveAs(origShortName, copyShortName, copyLongName, flagPathAndFileName, saveAsAdaptOptions);
        }
    }
}
