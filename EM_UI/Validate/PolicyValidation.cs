using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.TreeListManagement;
using System.Windows.Forms;

namespace EM_UI.Validate
{
    class PolicyValidation
    {
        static bool IsExistingPolicyName(TreeList countryTreeList, string policyName)
        {
            if (countryTreeList == null)
                return false;
            TreeListColumn policyColumn = countryTreeList.Columns.ColumnByName(TreeListBuilder._policyColumnName);
            foreach (TreeListNode node in countryTreeList.Nodes)
                if (node.GetDisplayText(policyColumn).ToLower() == policyName.ToLower())
                    return true;
            return false;
        }   

        internal static bool IsValidPolicyName(ref string policyName, string countryShortcut, TreeList countryTreeList, string currentName = "")
        {
            countryShortcut = countryShortcut.ToLower();
            if (countryShortcut.Substring(0, 1) != "_")
                countryShortcut = "_" + countryShortcut;

            string errorText = string.Empty;
            if (EM_Helpers.ContainsIllegalChar(policyName, ref errorText))
            {
                Tools.UserInfoHandler.ShowError(errorText);
                return false;
            }

            if (policyName.Length < countryShortcut.Length || policyName.Substring(policyName.Length - countryShortcut.Length).ToLower() != countryShortcut)
            {
                DialogResult dialogResult = Tools.UserInfoHandler.GetInfo("Policy name should by convention end with the country's shortcut.\n\n" +
                                                            "Should the policy be named '" + policyName + countryShortcut + "'?", MessageBoxButtons.YesNoCancel);
                if (dialogResult == DialogResult.Cancel)
                    return false;
                if (dialogResult == DialogResult.Yes)
                    policyName += countryShortcut;
            }

            if (countryTreeList != null &&
                !(currentName != string.Empty && currentName.ToLower() == policyName.ToLower()) && // allows for lower-upper-case renaming (e.g. tin_sl to TIN_sl)
                IsExistingPolicyName(countryTreeList, policyName) == true)
            {
                Tools.UserInfoHandler.ShowError("Policy '" + policyName + "' already exists. Please choose another name.");
                return false;
            }

            return true;
        }

    }
}
