using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    static class OptionalWarningsManager
    {
        internal static int _hiddenSystemstemWarning = 0;
        internal static int _changeOrderOfPolicyFunctionWarning = 1;
        internal static int _changeCountryShortNameWarning = 2;
        internal static int _deleteFunctionWarning = 3;
        internal static int _deleteParameterWarning = 4;
        internal static int _invalidPasteFunctionsWarning = 5;
        internal static int _administrateSwitchablePoliciesWarning = 6;
        internal static int _systemWithoutDatasetsWarning = 7;
        internal static int _renameSystemWarning = 8;
        internal static int _policySwitchWarning = 9;

        static Dictionary<string, string> _warningsLabelsAndTexts = new Dictionary<string, string>
        {
            { "Show 'Hidden Systems' Warning", "Please note that the action will have effect on the hidden systems as well!" },
            { "Show 'Change Policy/Function Order' Warning", "Are you sure you want to change the order of calculations?" },
            { "Show 'Change Country Shortname' Warning", "Please note that the shortname should correspond with file names." + Environment.NewLine + "Are you sure you want to change the shortname?" },
            { "Show 'Delete Function' Warning", "Are you sure you want to delete the function(s)?" },
            { "Show 'Delete Parameter' Warning", "Are you sure you want to delete the parameter(s)?" },
            { "Show 'Invalid Paste Function' Warning", "Please note that the selection is not a valid paste function region." + Environment.NewLine + "(Hint: only select functions of one policy)." + Environment.NewLine + "The right-clicked function is pasted instead." },
            { "Show 'Administrate Switchable Policies' Warning", "Please note that changes made in this dialog are saved in the file storing variables" + Environment.NewLine + "and that no undo is possible once the dialog is closed." },
            { "Show 'System Without Datasets' Warning", "Please note that the following systems have no dataset assigned" + Environment.NewLine + "and are therefore not displayed (as they are not runable):" },
            { "Show 'Rename System / no Add-On-Adaptation' Warning", "Please note that Add-Ons are not adapted to reflect the new system name." + Environment.NewLine + "Please change policy 'AddOn_Applic' manually where necessary." },
            { "Show 'Add-on switches off Switchable Policy' Warning", "Please note that switchable policies are permanently switched off via an add-on!" + Environment.NewLine +
                                                                      "This means that on-switching via the run-tool is no longer possible (i.e. displayed switch-values are not valid)." + Environment.NewLine + Environment.NewLine}
        };

        internal static bool Show(int indexWarning, bool showCancelButton = true, string headText = "", string bottomText = "")
        {
            char reshow = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OptionalWarnings.ElementAt(indexWarning);
            if (reshow == '0')
                return true;

            OptionalWarningsForm optionalWarningsForm = new OptionalWarningsForm(headText + _warningsLabelsAndTexts.Values.ElementAt(indexWarning) + bottomText, showCancelButton);
            if (optionalWarningsForm.ShowDialog() == DialogResult.Cancel)
                return false;

            if (!optionalWarningsForm.Reshow())
            {
                string reshowSettings = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OptionalWarnings;
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OptionalWarnings = reshowSettings.Substring(0, indexWarning) + '0' + reshowSettings.Substring(indexWarning + 1);
                EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(true);
            }

            return true;
        }

        internal static Dictionary<string, bool> GetOptionalWarnings()
        {
            Dictionary<string, bool> optionalWarnings = new Dictionary<string, bool>();
            for (int index = 0; index < _warningsLabelsAndTexts.Count; ++index)
            {
                bool reshow = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OptionalWarnings.ElementAt(index) == '1';
                optionalWarnings.Add(_warningsLabelsAndTexts.Keys.ElementAt(index), reshow);
            }
            return optionalWarnings;
        }

        internal static void SetOptionalWarnings(string optionalWarnings)
        {
            EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OptionalWarnings = optionalWarnings + EM_AppContext.Instance.GetUserSettingsAdministrator().Get().OptionalWarnings.Substring(optionalWarnings.Length);
        }

        //not very sophisticated attempt to 'centralise' warnings, which hint at (for what ever reason) outdated elements
        //allows at least to easily look up which outdated warnings exist and check whether they can be eliminited meanwhile
        internal static void ShowOutdatedWarning(string warning)
        {
            Tools.UserInfoHandler.ShowError(warning);
        }
    }
}
