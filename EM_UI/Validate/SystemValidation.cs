using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using EM_Common;
using EM_Common_Win;
using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.Validate
{
    class SystemValidation
    {
        internal static bool IsExistingSystemName(string systemName, TreeList countryTreeList = null, CountryConfigFacade countryConfigFacade = null)
        {
            if (countryTreeList != null)
            {
                foreach (TreeListColumn column in countryTreeList.Columns)
                    if (column.GetCaption().ToLower() == systemName.ToLower())
                        return true;
            }

            if (countryConfigFacade != null)
            {
                foreach (CountryConfig.SystemRow systemRow in countryConfigFacade.GetSystemRows())
                    if (systemRow.Name.ToLower() == systemName.ToLower())
                        return true;
            }

            return false;
        }

        internal static bool GetSystemNameAndYear(bool askForYear, ref string systemName, ref string systemYear, TreeList countryTreeList = null, CountryConfigFacade countryConfigFacade = null)
        {
            int year = -1;
            List<UserInput.Item> items = new List<UserInput.Item> { new UserInput.Item("name", "System Name") { InitialValue = systemName } };
            if (askForYear) items.Add(new UserInput.Item("year", "System Year") { MinMax = new Tuple<double, double>(1900, 2100), InitialValue = systemYear });
            while (true)
            {
                UserInput ui = new UserInput(items); if (ui.ShowDialog() == DialogResult.Cancel) return false;
                systemName = ui.GetValue<string>("name");
                if (askForYear) year = ui.GetValue<int>("year");
                if (countryTreeList == null || 
                    (IsValidSystemName(systemName, countryTreeList, countryConfigFacade) && CheckSysNameYearMatch(systemName, year))) break;
                items[0].InitialValue = systemName; if (askForYear && year > 0) items[1].InitialValue = year;
            }
            if (year > 0) systemYear = year.ToString();
            return true;
        }

        private static bool CheckSysNameYearMatch(string systemName, int year)
        {
            if (year < 0) return true;
            string yYear = year.ToString(), sYear = EM_Helpers.ExtractSystemYear(systemName);
            if (string.IsNullOrEmpty(sYear) || yYear == sYear) return true;
            return UserInfoHandler.GetInfo($"Year does not match with year suggested by system name ({yYear} vs. {sYear})" +
                Environment.NewLine + "Do you want to correct?", MessageBoxButtons.YesNo) == DialogResult.No;
        }

        static bool IsValidSystemName(string systemName, TreeList countryTreeList, CountryConfigFacade countryConfigFacade = null)
        {
            string errorText = string.Empty;
            if (EM_Helpers.ContainsIllegalChar(systemName, ref errorText))
            {
                Tools.UserInfoHandler.ShowError(errorText);
                return false;
            }

            if (IsExistingSystemName(systemName, countryTreeList, countryConfigFacade) == true)
            {
                Tools.UserInfoHandler.ShowError("System '" + systemName + "' already exists. Please choose another name.");
                return false;
            }

            return true;
        }

    }
}
