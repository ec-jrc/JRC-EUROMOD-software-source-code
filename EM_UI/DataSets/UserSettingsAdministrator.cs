using EM_Common;
using EM_UI.Run;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.DataSets
{
    class UserSettingsAdministrator
    {
        UserSettings _currentSettings = null;
        UserSettings _bkupCurrentSettings = null;
        const string _currentSettingsName = "UserSettings.txt";
        const char _settingListSeparator = '°';
        const char _settingSubSeparator = '^';

        internal static string GetCurrentSettingsFullName() { return Path.Combine(EnvironmentInfo.GetUserSettingsFolder(), _currentSettingsName); }
        static string GetAnySettingsFullName() { return Path.Combine(EnvironmentInfo.GetUserSettingsFolder(), Guid.NewGuid() + _currentSettingsName); }
        private string oldLocalSettingsFolder() { return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EUROMOD"); }

        private void copyLocalSettings()
        {
            Directory.CreateDirectory(EnvironmentInfo.GetUserSettingsFolder());
            foreach (FileInfo usFileInfo in new DirectoryInfo(oldLocalSettingsFolder()).GetFiles("*.txt"))
            {
                usFileInfo.CopyTo(Path.Combine(EnvironmentInfo.GetUserSettingsFolder(), usFileInfo.Name));
            }

        }

        internal void LoadCurrentSettings(string settingsPath = "", bool checkAndUpdatePath = false, string projectPath = "")
        {
            // if there is no AppData/Roaming folder but there is an AppData/Local folder from a previous version, then copy over all the settings files
            if (!Directory.Exists(EnvironmentInfo.GetUserSettingsFolder()) && Directory.Exists(oldLocalSettingsFolder()))
                copyLocalSettings();

            if (_currentSettings != null && File.Exists(GetCurrentSettingsFullName())) //UI's current UserSettings are changed (i.e. a new project is loaded)
            {
                string anySettingsFullName = GetAnySettingsFullName();
                File.Move(GetCurrentSettingsFullName(), anySettingsFullName); //move UserSettings.txt to GuidUserSettings.txt ...
                SaveSettings(_currentSettings, anySettingsFullName); //... and save any changes

                CheckForGeneralChanges(anySettingsFullName); //check if any of the (changed) settings should be transfered to all existing (project's) user settings
            }

            if (settingsPath != string.Empty && settingsPath != GetCurrentSettingsFullName())//should correspond to _currentUserSettings!=null (i.e. a new project is loaded)
            {
                if (checkAndUpdatePath && !File.Exists(settingsPath))
                {

                    List<string> pathsUserSettings = null;
                    List<string> projectPaths = UserSettingsAdministrator.GetAvailableProjectPaths(out pathsUserSettings);
                    if (projectPaths.Contains(projectPath))
                        //loading a project with existing user-settings
                        settingsPath = pathsUserSettings.ElementAt(projectPaths.IndexOf(projectPath));
                }

                File.Move(settingsPath, GetCurrentSettingsFullName()); //rename the corresponding settings-file from GuidUserSetting.txt to UserSetting.txt, to make it current
            }


            _currentSettings = ReadSettings(GetCurrentSettingsFullName());
            _bkupCurrentSettings = _currentSettings.Copy() as UserSettings;

            EM_AppContext.Instance.GetVCAdministrator().InitButtons(); //request VCAdministrator to initialise
        }

        static UserSettings ReadSettings(string settingsPath)
        {
            UserSettings userSettings = new UserSettings();
            try
            {
                using (StreamReader streamReader = new StreamReader(settingsPath, DefGeneral.DEFAULT_ENCODING))
                    userSettings.ReadXml(streamReader);
            }
            catch (DirectoryNotFoundException)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(settingsPath));
            }
            catch { } //do not report failure, as UserSettings may not yet exist (real failure is reported on saving)

            if (userSettings.Settings.Rows.Count == 0)
                userSettings.Settings.AddSettingsRow(
                    string.Empty, //OutputFolder
                    string.Empty, //InputFolder
                    string.Empty, //EuromodFolder
                    "111111111111111111111111111111111111111111111111111111111111111111111", //OptionalWarnings
                    string.Empty, //Bookmarks
                    "EUROMOD", //ApplicationName
                               //RunFormSettings (is actually called RunFromSettings, but correcting this typo would "destroy" the current settings of existing user-settings)
                    RunMainForm._doNotStopOnNonCriticalErrors + _settingListSeparator + RunMainForm._closeAfterRun + _settingListSeparator,
                    600000, //AutoSaveInterval: 10 min
                    true, //ParallelRunsAuto
                    1, //ParallelRuns
                    false,
                    string.Empty, //PolicySwitchValues
                    string.Empty, //PolicySwitchesDisplayed
                    DefPar.Value.YES, //CloseInterfaceWithLastMainform
                    false, string.Empty, string.Empty, false, // VCLogInAtProjectLoad, VCProxyURL, VCProxyPort, VCUseProxy
                    string.Empty,//VCOrganizationID,
                    string.Empty, //ViewSettings
                    string.Empty); //PlugInSettings
            return userSettings;

            // *!*!*!*   N O T E   *!*!*!*
            // If any new user-setting is added, consider if it is clearly project-specific. If not, add it to CheckForGeneralChanges.
        }

        static internal string GenerateProjectSettings(string projectPath)
        {
            UserSettings settings = ReadSettings(GetCurrentSettingsFullName()); //read UserSetting.txt if available, otherwise get empty settings
            projectPath = EMPath.AddSlash(projectPath);

            //adapt EuromodFolder (= project-path) as well as default input- and output-folder, but the latter only if they were "standard" before, i.e. EuromodFolder\output resp. EuromodFolder\input
            string currentProjectPath = EMPath.AddSlash(settings.Settings.First().EuromodFolder).ToLower();
            bool useDefaultOutputFolder = settings.Settings.First().OutputFolder == string.Empty || settings.Settings.First().OutputFolder.ToLower().Contains(currentProjectPath);
            bool useDefaultInputFolder = settings.Settings.First().InputFolder == string.Empty || settings.Settings.First().InputFolder.ToLower().Contains(currentProjectPath);

            settings.Settings.First().EuromodFolder = projectPath; //change the project path
            if (useDefaultOutputFolder) settings.Settings.First().OutputFolder = projectPath + "output";
            if (useDefaultInputFolder) settings.Settings.First().InputFolder = projectPath + "input";

            string settingsPath = GetAnySettingsFullName();
            //MessageBox.Show(settingsPath);
            SaveSettings(settings, settingsPath); //store as GuidUserSetting.txt
            return settingsPath;
        }

        void UpdateOutputFieldOnRunDialoguesIfDefault()
        {
            // if the output folder changed, check if any loaded Run Dialogues also need to be updated
            if (Get().OutputFolder != bkupGet().OutputFolder)
                EM_AppContext.Instance.UpdateOutputFieldOnRunDialoguesIfDefault(EMPath.AddSlash(bkupGet().OutputFolder), EMPath.AddSlash(Get().OutputFolder));
        }

        public void CheckForGeneralChanges(string changeCausingUserSettingsPath)
        {
            try
            {
                if (_currentSettings == null || _bkupCurrentSettings == null) return;

                List<string> userSettingFullPaths; List<string> userSettingFolders = GetAvailableProjectPaths(out userSettingFullPaths);
                if (userSettingFullPaths.Count == 0 || // check if no other user-settings but the current
                   (userSettingFullPaths.Count == 1 && userSettingFullPaths.First().ToLower() == changeCausingUserSettingsPath.ToLower())) return;

                Dictionary<string, KeyValuePair<string, object>> changedGeneralSettings = new Dictionary<string, KeyValuePair<string, object>>();
                if (Get().OptionalWarnings != bkupGet().OptionalWarnings)
                    changedGeneralSettings.Add("Optional Warnings",
                                               new KeyValuePair<string, object>("OptionalWarnings", Get().OptionalWarnings));
                if (Get().AutoSaveInterval != bkupGet().AutoSaveInterval)
                    changedGeneralSettings.Add(string.Format("Autosave Interval: {0} min", Get().AutoSaveInterval / 60000),
                                               new KeyValuePair<string, object>("AutoSaveInterval", Get().AutoSaveInterval));
                if (Get().ParallelRunsAuto != bkupGet().ParallelRunsAuto)
                    changedGeneralSettings.Add(string.Format("Number of Parallel Executable-Runs: {0}", Get().ParallelRunsAuto ? "Auto" : "Custom"),
                                               new KeyValuePair<string, object>("ParallelRunsAuto", Get().ParallelRunsAuto));
                if (Get().ParallelRuns != bkupGet().ParallelRuns)
                    changedGeneralSettings.Add(string.Format("Number of Parallel Executable-Runs: Custom = {0}", Get().ParallelRuns),
                                               new KeyValuePair<string, object>("ParallelRuns", Get().ParallelRuns));
                if (Get().CloseInterfaceWithLastMainform != bkupGet().CloseInterfaceWithLastMainform)
                    changedGeneralSettings.Add(string.Format("Close Interface With Last Country: {0}", Get().CloseInterfaceWithLastMainform),
                                               new KeyValuePair<string, object>("CloseInterfaceWithLastMainform", Get().CloseInterfaceWithLastMainform));
                if (Get().VCLogInAtProjectLoad != bkupGet().VCLogInAtProjectLoad)
                    changedGeneralSettings.Add(string.Format("Version Control, Log In At ProjectLoad: {0}", Get().VCLogInAtProjectLoad ? "yes" : "no"),
                                               new KeyValuePair<string, object>("VCLogInAtProjectLoad", Get().VCLogInAtProjectLoad));

                if (!bkupGet().VCUseProxy && Get().VCProxyURL != bkupGet().VCProxyURL && Get().VCProxyPort != bkupGet().VCProxyPort)
                    changedGeneralSettings.Add(string.Format("Use Proxy: {0}", Get().VCUseProxy),
                                               new KeyValuePair<string, object>("VCUseProxy", Get().VCUseProxy));
                if (Get().VCProxyURL != bkupGet().VCProxyURL)
                    changedGeneralSettings.Add(string.Format("Version Control Proxy URL: {0}", Get().VCProxyURL),
                                               new KeyValuePair<string, object>("VCProxyURL", Get().VCProxyURL));
                if (Get().VCProxyPort != bkupGet().VCProxyPort)
                    changedGeneralSettings.Add(string.Format("Version Control Proxy Port: {0}", Get().VCProxyPort),
                                               new KeyValuePair<string, object>("VCProxyPort", Get().VCProxyPort));
                if (Get().InputFolder != bkupGet().InputFolder &&
                    !Get().InputFolder.ToLower().StartsWith(Get().EuromodFolder.ToLower())) // only take into account if folder for data is not within current project
                {
                    changedGeneralSettings.Add(string.Format("Input Folder: {0}", Get().InputFolder),
                                               new KeyValuePair<string, object>("InputFolder", Get().InputFolder));
                }

                if (changedGeneralSettings.Count == 0) return;

                string request = string.Format("Should the following change{0} of your (current project's) user settings be transferred to all projects' user settings?", changedGeneralSettings.Count == 1 ? string.Empty : "s") + Environment.NewLine + Environment.NewLine;
                foreach (string descriptionChangedSetting in changedGeneralSettings.Keys) request += "- " + descriptionChangedSetting + Environment.NewLine;
                if (UserInfoHandler.GetInfo(request, MessageBoxButtons.YesNo) == DialogResult.No) return;

                for (int i = 0; i < userSettingFullPaths.Count; ++i)
                {
                    if (userSettingFullPaths.First().ToLower() == changeCausingUserSettingsPath.ToLower()) continue;

                    UserSettings userSettings = ReadSettings(userSettingFullPaths[i]);
                    foreach (KeyValuePair<string, object> changedGeneralSetting in changedGeneralSettings.Values)
                    {
                        string settingName = changedGeneralSetting.Key; object settingValue = changedGeneralSetting.Value;
                        userSettings.Settings.Rows[0][settingName] = settingValue;
                    }
                    userSettings.Settings.AcceptChanges();
                    SaveSettings(userSettings, userSettingFullPaths[i]);
                }
            }
            catch { } // do not jeopardise UI due to not being able to transfer user-setting
        }

        internal void SaveCurrentSettings(bool checkForGeneralChanges)
        {
            SaveSettings(_currentSettings, GetCurrentSettingsFullName());
            if (checkForGeneralChanges) CheckForGeneralChanges(GetCurrentSettingsFullName());
            UpdateOutputFieldOnRunDialoguesIfDefault();     // check if the Run Dialogues need to update their output folder
            _bkupCurrentSettings = _currentSettings.Copy() as UserSettings;
        }
        static void SaveSettings(UserSettings settings, string settingsPath)
        {
            try
            {
                if (!Directory.Exists(EnvironmentInfo.GetUserSettingsFolder()))
                    Directory.CreateDirectory(EnvironmentInfo.GetUserSettingsFolder()); //EUROMOD folder does not yet exist
                Stream fileStream = new FileStream(settingsPath, FileMode.Create);

                UserSettings encSettings = settings.Copy() as UserSettings;
                using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING))
                    encSettings.WriteXml(xmlWriter);
                //MessageBox.Show("File created");
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        //get simple string settings just over the data-row
        internal UserSettings.SettingsRow Get() { return _currentSettings.Settings.First(); }
        internal UserSettings.SettingsRow bkupGet() { return _bkupCurrentSettings.Settings.First(); }

        //get string-list settings with extra functions
        internal List<string> GetBookmarks() { return Get().Bookmarks.Split(_settingListSeparator).ToList<string>(); }
        internal void AddToBookmarks(string setting) { Get().Bookmarks += setting + _settingListSeparator; }
        internal void RemoveFromBookmarks(string setting) { Get().Bookmarks = RemoveFromSettingList(Get().Bookmarks, setting); }

        internal List<string> GetRunFormSettings() { return Get().RunFromSettings.Split(_settingListSeparator).ToList<string>(); }
        internal void AddToRunFormSettings(string setting) { Get().RunFromSettings += setting + _settingListSeparator; }
        internal void RemoveFromRunFormSettings(string setting) { Get().RunFromSettings = RemoveFromSettingList(Get().RunFromSettings, setting); }

        string RemoveFromSettingList(string settingList, string setting) { return settingList.Replace(setting, string.Empty); }

        static internal List<string> GetAvailableProjectPaths(out List<string> correspondingUserSettings)
        {
            correspondingUserSettings = new List<string>();
            List<string> projectPaths = new List<string>();
            SortedList<string, FileInfo> liUserSettings = new SortedList<string, FileInfo>();

            foreach (FileInfo usFileInfo in new DirectoryInfo(EnvironmentInfo.GetUserSettingsFolder()).GetFiles("*" + _currentSettingsName))
            {
                string delUserInfo = string.Empty;
                UserSettings userSettings = ReadSettings(usFileInfo.FullName);
                string projectPath = userSettings.Settings.First().EuromodFolder.ToLower();

                if (projectPath != string.Empty && Directory.Exists(projectPath))
                {
                    if (liUserSettings.ContainsKey(projectPath))
                    {
                        if (usFileInfo.LastWriteTime > liUserSettings[projectPath].LastWriteTime)
                        {
                            liUserSettings[projectPath] = usFileInfo; delUserInfo = liUserSettings[projectPath].FullName;
                        }
                        else delUserInfo = usFileInfo.FullName;
                    }
                    else liUserSettings.Add(projectPath.ToLower(), usFileInfo);
                }
                else delUserInfo = usFileInfo.FullName;

                if (delUserInfo != string.Empty) { try { File.Delete(delUserInfo); } catch { } }
            }

            foreach (string projectPath in liUserSettings.Keys)
            {
                projectPaths.Add(projectPath);
                correspondingUserSettings.Add(liUserSettings[projectPath].FullName);
            }
            return projectPaths;
        }

        internal string GetPolicySwitchValue(string switchablePolicyID, string systemID, string datasetID)
        {
            foreach (string policySwitchValue in Get().PolicySwitchValues.Split(_settingListSeparator).ToList<string>())
            {
                List<string> policySwitchElements = policySwitchValue.Split(_settingSubSeparator).ToList<string>();
                if (policySwitchElements.Count == 4 && policySwitchElements.ElementAt(0) == switchablePolicyID &&
                    policySwitchElements.ElementAt(1) == systemID && policySwitchElements.ElementAt(2) == datasetID)
                    return policySwitchElements.ElementAt(3);
            }
            return string.Empty;
        }
        internal void AddSetPolicySwitchValue(string switchablePolicyID, string systemID, string datasetID, string value)
        {
            //first remove any possible existing setting
            string uniqueIdentifier = switchablePolicyID + _settingSubSeparator + systemID + _settingSubSeparator + datasetID;
            Get().PolicySwitchValues = RemoveFromSettingList(Get().PolicySwitchValues, uniqueIdentifier + _settingSubSeparator + DefPar.Value.ON);
            Get().PolicySwitchValues = RemoveFromSettingList(Get().PolicySwitchValues, uniqueIdentifier + _settingSubSeparator + DefPar.Value.OFF);
            Get().PolicySwitchValues = RemoveFromSettingList(Get().PolicySwitchValues, uniqueIdentifier + _settingSubSeparator);
            //then add with the possibly new value, if not n/a
            if (value != DefPar.Value.NA)
                Get().PolicySwitchValues += uniqueIdentifier + _settingSubSeparator + value + _settingListSeparator;
        }
        internal void RemoveAllPolicySwitchValues() { Get().PolicySwitchValues = string.Empty; }
        internal bool GetPolicySwitchAutoRenameValue()
        {
            return Get().PolicySwitchesAutoRename;
        }
        internal void SetPolicySwitchAutoRenameValue(bool val)
        {
            Get().PolicySwitchesAutoRename = val;
        }

        internal string GetPlugInSetting(string plugInUserSettingsId, string settingId)
        {
            var plugInSettings = Get().PlugInSettings;
            if (!string.IsNullOrEmpty(plugInSettings))
                foreach (string plugInSetting in plugInSettings.Split(_settingListSeparator))
                {
                    List<string> plugInSettingElements = plugInSetting.Split(_settingSubSeparator).ToList();
                    if (plugInSettingElements.Count == 3 && plugInSettingElements.ElementAt(0) == plugInUserSettingsId &&
                        plugInSettingElements.ElementAt(1) == settingId) return plugInSettingElements.ElementAt(2);
                }
            return string.Empty;
        }
        internal void AddSetPlugInSetting(string plugInUserSettingsId, string settingId, string settingValue)
        {
            RemovePlugInSetting(plugInUserSettingsId, settingId);
            Get().PlugInSettings = (Get().PlugInSettings ?? string.Empty) +
                plugInUserSettingsId + _settingSubSeparator + settingId + _settingSubSeparator + settingValue + _settingListSeparator;
        }
        internal void RemovePlugInSetting(string plugInUserSettingsId, string settingId)
        {
            string value = GetPlugInSetting(plugInUserSettingsId, settingId); if (string.IsNullOrEmpty(value)) return;
            Get().PlugInSettings = Get().PlugInSettings.Replace(plugInUserSettingsId + _settingSubSeparator + settingId + _settingSubSeparator + value + _settingListSeparator, string.Empty);
        }
    }
}
