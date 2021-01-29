using EM_Common;
using System.Collections.Generic;
using System.Windows.Forms;

namespace EM_Common_Win
{
    public static class UISessionInfo
    {
        // the functions below replicate the functions provided by EM_UI.PlugInService.SessionInfo for more convenient calling by plug-ins
        // (also see EM_Common.UICommunicator for further comments)

        public const string CLASS_NAME_UISessionInfo = "SessionInfo";

        public static Form GetActiveMainForm()
        {
            if (UICommunicator.CallStaticFunction(CLASS_NAME_UISessionInfo, "GetActiveMainForm", out TSObject theForm, out string errMsg))
                return theForm.GetValue<Form>();
            MessageBox.Show(errMsg); return null;
        }

        public static string GetEuromodFilesFolder() { return GetUISessionFolder("GetEuromodFilesPath"); }
        public static string GetOutputFolder() { return GetUISessionFolder("GetOutputPath"); }
        public static string GetInputFolder() { return GetUISessionFolder("GetInputPath"); }
        public static List<string> GetCountries() { return CallUIFunction<List<string>>("GetCountries"); }
        public static List<string> GetCountrySystems(string cn) { return CallUIFunction<List<string>>("GetCountrySystems", new Dictionary<string, object>() { { "cn", cn } }); }
        public static string GetSystemYear(string cn, string sn) { return CallUIFunction<string>("GetSystemYear", new Dictionary<string, object>() { { "cn", cn }, { "sn", sn } }); }

        private static string GetUISessionFolder(string uiGetFunctionName)
        {
            if (UICommunicator.CallStaticFunction(CLASS_NAME_UISessionInfo, uiGetFunctionName, out TSObject thePath, out string errMsg))
                return thePath.GetValue<string>();
            MessageBox.Show(errMsg); return string.Empty;
        }

        private static T CallUIFunction<T>(string uiGetFunctionName, Dictionary<string, object> parameters = null)
        {
            if (UICommunicator.CallStaticFunction(CLASS_NAME_UISessionInfo, uiGetFunctionName, out TSObject theList, out string errMsg,
                                                  UICommunicator.ComposeParameters(parameters)))
                return theList.GetValue<T>();
            MessageBox.Show(errMsg); return default(T);
        }

        public const string CAO_INFO_MainForms = "MainForms";
        public const string CAO_INFO_ShortNames = "ShortNames";
        public const string CAO_INFO_LongNames = "LongNames";
        public const string CAO_INFO_IsAddOn = "IsAddOn";
        public const string CAO_INFO_IsOpen = "IsOpen";
        public const string CAO_INFO_MainXMLFileNames = "MainXMLFileNames";
        public const string CAO_INFO_DataXMLFileNames = "DataXMLFileNames";
        public const string CAO_INFO_FilePaths = "FilePaths";
        public static bool GetCAOInfo(out TSDictionary info,
                                      bool includeCountries = true, bool includeAddOns = true, bool openOnly = false)
        {
            info = new TSDictionary();
            TSDictionary parameters = UICommunicator.ComposeParameters("includeCountries", includeCountries,
                                                                       "includeAddOns", includeAddOns,
                                                                       "openOnly", openOnly);
            if (UICommunicator.CallStaticFunction(CLASS_NAME_UISessionInfo, "GetCAOInfo", out TSObject retVal, out string errMsg, parameters))
            {
                if (retVal.GetValue<bool>() == true) info = parameters.GetItem<TSDictionary>("info");
                return retVal.GetValue<bool>();
            }
            MessageBox.Show(errMsg); return false;
        }

        // the following 5 functions allow for storing plug-in-user-settings (the former two on session level, the latter two beyond closing the UI)
        // for description see EM_UI.PlugInService.SessionInfo.cs
        public static TSDictionary GetSessionUserSettings(string plugInUserSettingsId)
        {
            return CallUIFunction<TSDictionary>("GetSessionUserSettings", new Dictionary<string, object>()
            {
                { "plugInUserSettingsId", plugInUserSettingsId }
            });
        }
        public static void SetSessionUserSettings(string plugInUserSettingsId, TSDictionary settings)
        {
            CallUIFunction<string>("SetSessionUserSettings", new Dictionary<string, object>()
            {
                { "plugInUserSettingsId", plugInUserSettingsId },
                { "settings", settings }
            });
        }
        public static string GetRetainedUserSetting(string plugInUserSettingsId, string settingId)
        {
            return CallUIFunction<string>("GetRetainedUserSetting", new Dictionary<string, object>()
            {
                { "plugInUserSettingsId", plugInUserSettingsId },
                { "settingId", settingId }
            });
        }
        public static void SetRetainedUserSetting(string plugInUserSettingsId, string settingId, string settingValue)
        {
            CallUIFunction<string>("SetRetainedUserSetting", new Dictionary<string, object>()
            {
                { "plugInUserSettingsId", plugInUserSettingsId },
                { "settingId", settingId },
                { "settingValue", settingValue }
            });
        }
        public static void RemoveRetainedUserSetting(string plugInUserSettingsId, string settingId)
        {
            CallUIFunction<string>("RemoveRetainedUserSetting", new Dictionary<string, object>()
            {
                { "plugInUserSettingsId", plugInUserSettingsId },
                { "settingId", settingId }
            });
        }
    }
}
