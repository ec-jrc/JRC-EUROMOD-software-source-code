using EM_Common;
using EM_Common_Win;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_UI.PlugInService
{
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    // DO NOT DELETE THESE FUNCTIONS, THOUGH THEY SEEM TO BE NOT USED
    // SEE EM_Common.UISessionInfo FOR UNDERSTANDING THEIR APPLICATION
    // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!

    public class SessionInfo
    {
        public static Form GetActiveMainForm()
        {
            try { return EM_AppContext.Instance.GetActiveCountryMainForm(); }
            catch { return null; }
        }

        public static string GetEuromodFilesPath()
        {
            return EM_AppContext.FolderEuromodFiles;
        }

        public static string GetOutputPath()
        {
            return EM_AppContext.FolderOutput;
        }

        public static string GetInputPath()
        {
            return EM_AppContext.FolderInput;
        }

        public static List<string> GetCountries()
        {
            List<string> countries = new List<string>();
            foreach (Country cc in EM_UI.CountryAdministration.CountryAdministrator.GetCountries())
                countries.Add(cc._shortName.ToLower());
            return countries;
        }

        public static List<string> GetCountrySystems(string cn)
        {
            List<string> systems = new List<string>();
            CountryConfigFacade ccf = EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(cn);
            if (ccf == null) return systems;
            foreach (CountryConfig.SystemRow sr in ccf.GetSystemRows())
                systems.Add(sr.Name.ToLower());
            return systems;
        }

        public static string GetSystemYear(string cn, string sn)
        {
            CountryConfigFacade ccf = EM_UI.CountryAdministration.CountryAdministrator.GetCountryConfigFacade(cn);
            if (ccf == null) return "";
            CountryConfig.SystemRow sr = ccf.GetSystemRowByName(sn);
            if (sr == null) return "";
            return sr.Year;
        }

        public static bool GetCAOInfo(out TSDictionary info, bool includeCountries = true, bool includeAddOns = true, bool openOnly = false)
        {
            info = new TSDictionary();

            try
            {
                List<Country> caos = new List<Country>();
                if (includeCountries) caos.AddRange(CountryAdministrator.GetCountries());
                if (includeAddOns) caos.AddRange(CountryAdministrator.GetAddOns());

                Dictionary<string, EM_UI_MainForm> openMainForms = new Dictionary<string,EM_UI_MainForm>();
                foreach (EM_UI_MainForm mf in EM_AppContext.Instance.GetOpenCountriesMainForms())
                    openMainForms.Add(mf.GetCountryShortName().ToLower(), mf);

                List<Form> mainForms = new List<Form>();
                List<string> shortNames = new List<string>();
                List<string> longNames = new List<string>();
                List<bool> isAddOn = new List<bool>();
                List<bool> isOpen = new List<bool>();
                List<string> mainXMLFileNames = new List<string>();
                List<string> dataXMLFileNames = new List<string>();
                List<string> filePaths = new List<string>();
                foreach (Country cao in caos)
                {
                    bool open = openMainForms.ContainsKey(cao._shortName.ToLower());
                    if (openOnly && !open) continue;
                    mainForms.Add(open ? openMainForms[cao._shortName.ToLower()] : null);
                    shortNames.Add(cao._shortName);
                    // long name is only availabel through CountryConfig (it doesn't seem appropriate to open the country just to get the long-name)
                    longNames.Add(open ? openMainForms[cao._shortName.ToLower()].GetCountryLongName() : cao._shortName);
                    isAddOn.Add(cao._isAddOn);
                    isOpen.Add(open);
                    mainXMLFileNames.Add(CountryAdministrator.GetCountryFileName(cao._shortName));
                    dataXMLFileNames.Add(cao._isAddOn ? string.Empty : CountryAdministrator.GetDataFileName(cao._shortName));
                    filePaths.Add(CountryAdministrator.GetCountryPath(cao._shortName));
                }
                info.SetItem(UISessionInfo.CAO_INFO_MainForms, mainForms);
                info.SetItem(UISessionInfo.CAO_INFO_ShortNames, shortNames);
                info.SetItem(UISessionInfo.CAO_INFO_LongNames, longNames);
                info.SetItem(UISessionInfo.CAO_INFO_IsAddOn, isAddOn);
                info.SetItem(UISessionInfo.CAO_INFO_IsOpen, isOpen);
                info.SetItem(UISessionInfo.CAO_INFO_MainXMLFileNames, mainXMLFileNames);
                info.SetItem(UISessionInfo.CAO_INFO_DataXMLFileNames, dataXMLFileNames);
                info.SetItem(UISessionInfo.CAO_INFO_FilePaths, filePaths);
                return true;
            }
            catch { return false; }
        }

        // the next two functions allow for storing user-choices of plug-ins during the livetime of the UI
        // example:
        // ON OPENING THE PLUG-IN:
        // EM_Common.TSDictionary pluginSessionInfo = UISessionInfo.GetSessionUserSettings(StatisticsPresenter.STATISTICS_PRESENTER_SESSION_INFO);
        // if (pluginSessionInfo != null)
        // {
        //     // Get the base path
        //     if (pluginSessionInfo.ContainsKey(StatisticsPresenter.BASE_OUTPUT_FOLDER)) 
        //         textBasePath.Text = pluginSessionInfo.GetItem<string>(StatisticsPresenter.BASE_OUTPUT_FOLDER);
        //     // get the reform path
        //     if (pluginSessionInfo.ContainsKey(StatisticsPresenter.REFORM_OUTPUT_FOLDER))
        //         textAltPath.Text = pluginSessionInfo.GetItem<string>(StatisticsPresenter.REFORM_OUTPUT_FOLDER);
        //     // or use the base path for this too
        //     else if (pluginSessionInfo.ContainsKey(StatisticsPresenter.BASE_OUTPUT_FOLDER))
        //         textAltPath.Text = pluginSessionInfo.GetItem<string>(StatisticsPresenter.BASE_OUTPUT_FOLDER);
        // }
        // ON CLOSING THE PLUG-IN:
        // // save selected path in session info
        // EM_Common.TSDictionary pluginSessionInfo = new EM_Common.TSDictionary();
        // pluginSessionInfo.SetItem(StatisticsPresenter.BASE_OUTPUT_FOLDER, textBasePath.Text);
        //     pluginSessionInfo.SetItem(StatisticsPresenter.REFORM_OUTPUT_FOLDER, textAltPath.Text);
        //     UISessionInfo.SetSessionUserSettings(StatisticsPresenter.STATISTICS_PRESENTER_SESSION_INFO, pluginSessionInfo);
        public static TSDictionary GetSessionUserSettings(string plugInUserSettingsId)
        {
            return EM_AppContext.Instance._sessionUserSettings.TryGetValue(plugInUserSettingsId, out TSDictionary sus) ? sus : null;
        }
        public static void SetSessionUserSettings(string plugInUserSettingsId, TSDictionary settings)
        {
            if (EM_AppContext.Instance._sessionUserSettings.ContainsKey(plugInUserSettingsId))
                EM_AppContext.Instance._sessionUserSettings.Remove(plugInUserSettingsId);
            EM_AppContext.Instance._sessionUserSettings.Add(plugInUserSettingsId, settings);
        }

        // similar to the two functions above, which store session-info for a plug-in, the next three functions store "beyond"-session-info for a plug-in,
        //    i.e. info that can be retained even when closing the UI and thus must be saved in the user settings
        // the reason (decision) for using the UI's user-settings and not plug-in-owened user settings are, in the case that caused implementing the functions
        //    (pathes of the Statistics Presenter), that the information is project-specific and thus uses the UI's project handling instead of inventing an own
        // using the UI's settings however restricts to string-settings and does not allow for own types
        // just for avoid confusion: the session-functions above Get and Set the whole (plug-inadministrated) session-info,
        //    while the retained-functions below Get and Set single settings
        public static string GetRetainedUserSetting(string plugInUserSettingsId, string settingId)
        {
            return EM_AppContext.Instance.GetUserSettingsAdministrator().GetPlugInSetting(plugInUserSettingsId, settingId);
        }
        public static void SetRetainedUserSetting(string plugInUserSettingsId, string settingId, string settingValue)
        {
            EM_AppContext.Instance.GetUserSettingsAdministrator().AddSetPlugInSetting(plugInUserSettingsId, settingId, settingValue);
        }
        public static void RemoveRetainedUserSetting(string plugInUserSettingsId, string settingId)
        {
            EM_AppContext.Instance.GetUserSettingsAdministrator().RemovePlugInSetting(plugInUserSettingsId, settingId);
        }
    }
}
