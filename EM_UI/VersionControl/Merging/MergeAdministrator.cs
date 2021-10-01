using EM_Common;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.Tools;
using EM_UI.UndoManager;
using EM_UI.VersionControl.Dialogs;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl.Merging
{
    internal class MergeAdministrator
    {
        const string APPENDIX_LOCAL = "_local";
        const string APPENDIX_REMOTE = "_remote";
        const string APPENDIX_PARENT = "_parent";
        const string PREFIX_MERGE_INFO_FILE = "MergeInfo_";
        const string MERGEINFO_COUNTRY = "Country";
        const string PARENT_UNAVAILABLE = "no parent";
        internal const string IDENTIFIER_ADD_DEL_COUNTRY_LABELS = "IDENTIFIER_ADD_DEL_COUNTRY_LABELS#";

        string FOLDER_MERGE
        {
            get
            {
                if (_mergeConfigFile)
                {
                    if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_HICP.ToLower())){ return EMPath.AddSlash(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + "MERGE_HICPCONFIG"); }
                    else if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_EXRATES.ToLower())){ return EMPath.AddSlash(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + "MERGE_EXCHANGERATESCONFIG"); }
                    else if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_VARS.ToLower())){ return EMPath.AddSlash(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + "MERGE_VARCONFIG"); }
                    else if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_EXTENSIONS.ToLower())) { return EMPath.AddSlash(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + "MERGE_SWITCHABLEPOLICYCONFIG"); }
                    else { return EMPath.AddSlash(EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles)); }
                    
                }
                //merging country- or add/on
                if (CountryAdministrator.ConsiderOldAddOnFileStructure(_isAddOn))
                    return EMPath.AddSlash(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + "MERGE_" + _shortName);
                return EMPath.AddSlash(EMPath.AddSlash((_isAddOn ? EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)) + _shortName) + "MERGE");
            }
            
        }
        string ComposeMergeFolderName(string appendix) {
            if (_mergeConfigFile)
            {
                if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_HICP.ToLower())) { return EMPath.AddSlash(FOLDER_MERGE + "HICPCONFIG" + appendix); }
                else if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_EXRATES.ToLower())) { return EMPath.AddSlash(FOLDER_MERGE + "EXCHANGERATESCONFIG" + appendix); }
                else if (_configFileName.ToLower().Equals(EMPath.EM2_FILE_EXTENSIONS.ToLower())) { return EMPath.AddSlash(FOLDER_MERGE + "SWITCHABLEPOLICYCONFIG" + appendix); }
                else { return EMPath.AddSlash(FOLDER_MERGE + "VARCONFIG" + appendix); }

            }
            else
            {
                return EMPath.AddSlash(FOLDER_MERGE + _shortName + appendix);
            }
        }

        public string FOLDER_LOCAL  { get { return ComposeMergeFolderName(APPENDIX_LOCAL);  } }
        public string FOLDER_REMOTE { get { return ComposeMergeFolderName(APPENDIX_REMOTE); } }
        public string FOLDER_PARENT { get { return ComposeMergeFolderName(APPENDIX_PARENT); } }

        public string _pathLocalVersion = string.Empty;
        public string _pathRemoteVersion = string.Empty;

        string _shortName = string.Empty;
        string _configFileName = string.Empty;
        bool _isAddOn = false;
        bool _mergeConfigFile = false;
        int _checkCountryLabels = -1;

        EM_UI_MainForm _mainForm = null;
        MergeForm _mergeForm = null;
        VariablesMergeForm _variablesMergeForm = null;
        HICPConfigMergeForm _hicpConfigMergeForm = null;
        ExchangeRatesConfigMergeForm _exchangeRatesConfigMergeForm = null;
        SwitchablePolicyConfigMergeForm _switchablePolicyConfigMergeForm = null;

        CountryConfigFacade _ccFacLocal = new CountryConfigFacade();
        CountryConfigFacade _ccFacRemote = new CountryConfigFacade();
        CountryConfigFacade _ccFacParent = new CountryConfigFacade();
        CountryConfig _ccLocal = null;
        CountryConfig _ccRemote = null;
        CountryConfig _ccParent = null;

        DataConfigFacade _dcFacLocal = new DataConfigFacade();
        DataConfigFacade _dcFacRemote = new DataConfigFacade();
        DataConfigFacade _dcFacParent = new DataConfigFacade();
        DataConfig _dcLocal = null;
        DataConfig _dcRemote = null;
        DataConfig _dcParent = null;

        VarConfigFacade _vcFacLocal = null;
        VarConfigFacade _vcFacRemote = null;
        VarConfigFacade _vcFacParent = null;
        VarConfig _vcLocal = null;
        VarConfig _vcRemote = null;
        VarConfig _vcParent = null;

        HICPConfigFacade _vcFacHICPLocal = null;
        HICPConfigFacade _vcFacHICPRemote = null;
        HICPConfigFacade _vcFacHICPParent = null;
        HICPConfig _vcHICPLocal = null;
        HICPConfig _vcHICPRemote = null;
        HICPConfig _vcHICPParent = null;


        ExchangeRatesConfigFacade _vcFacExchangeRatesLocal = null;
        ExchangeRatesConfigFacade _vcFacExchangeRatesRemote = null;
        ExchangeRatesConfigFacade _vcFacExchangeRatesParent = null;
        ExchangeRatesConfig _vcExchangeRatesLocal = null;
        ExchangeRatesConfig _vcExchangeRatesRemote = null;
        ExchangeRatesConfig _vcExchangeRatesParent = null;

        SwitchablePolicyConfigFacade _vcFacSwitchablePolicyLocal = null;
        SwitchablePolicyConfigFacade _vcFacSwitchablePolicyRemote = null;
        SwitchablePolicyConfigFacade _vcFacSwitchablePolicyParent = null;
        SwitchablePolicyConfig _vcSwitchablePolicyLocal = null;
        SwitchablePolicyConfig _vcSwitchablePolicyRemote = null;
        SwitchablePolicyConfig _vcSwitchablePolicyParent = null;


        internal MergeAdministrator(bool mergeVariables, bool isAddOn, string shortName, string configFileName = "")
        {
            _shortName = shortName;
            _isAddOn = isAddOn;
            _mergeConfigFile = mergeVariables;
            _configFileName = configFileName;
            _mainForm = EM_AppContext.Instance.GetActiveCountryMainForm(); //just to avoid crash just because _mainForm.WaitCursor is activated
        }

        internal MergeAdministrator(EM_UI_MainForm mainForm, bool mergeVariables, string configFileName = "")
        {
            _mainForm = mainForm;
            _shortName = _mainForm.GetCountryShortName();
            _isAddOn = _mainForm._isAddOn;
            _mergeConfigFile = mergeVariables;
            _configFileName = configFileName;
        }

        bool AreInfoFilesAvailable() { return Directory.GetFiles(FOLDER_MERGE, PREFIX_MERGE_INFO_FILE + "*", SearchOption.TopDirectoryOnly).Count() > 0; }

        void GetInfoFromInfoFile(string mergeControlName, out string contentInfoFile)
        {
            contentInfoFile = ReadFromInfoFile(mergeControlName);
        }

        string ReadFromInfoFile(string infoName)
        {
            using (StreamReader reader = new StreamReader(FOLDER_MERGE + PREFIX_MERGE_INFO_FILE + infoName))
            {
                string content = reader.ReadToEnd();
                return content;
            }
        }

        void WriteInfoToInfoFile(string infoName, string content)
        {
            using (StreamWriter writer = new StreamWriter(FOLDER_MERGE + PREFIX_MERGE_INFO_FILE + infoName))
                writer.Write(content);
        }

        internal void WriteInfoToInfoFiles()
        {
            try
            {
                
                if (_mergeConfigFile)
                {
                    if (_configFileName.Equals(EMPath.EM2_FILE_HICP))
                    {
                        _hicpConfigMergeForm.Cursor = Cursors.WaitCursor;
                        WriteInfoToInfoFile(HICPConfigMergeForm.HICPCONFIG, _hicpConfigMergeForm.StoreMergeControl(HICPConfigMergeForm.HICPCONFIG));
                    }
                    else if (_configFileName.Equals(EMPath.EM2_FILE_EXRATES))
                    {
                        _exchangeRatesConfigMergeForm.Cursor = Cursors.WaitCursor;
                        WriteInfoToInfoFile(ExchangeRatesConfigMergeForm.EXCHANGERATESCONFIG, _exchangeRatesConfigMergeForm.StoreMergeControl(ExchangeRatesConfigMergeForm.EXCHANGERATESCONFIG));
                    }
                    else if (_configFileName.Equals(EMPath.EM2_FILE_EXTENSIONS))
                    {
                        _switchablePolicyConfigMergeForm.Cursor = Cursors.WaitCursor;
                        WriteInfoToInfoFile(SwitchablePolicyConfigMergeForm.SWITCHABLEPOLICYCONFIG, _switchablePolicyConfigMergeForm.StoreMergeControl(SwitchablePolicyConfigMergeForm.SWITCHABLEPOLICYCONFIG));

                    }
                    else
                    {
                        _variablesMergeForm.Cursor = Cursors.WaitCursor;
                        WriteInfoToInfoFile(VariablesMergeForm.VARIABLES, _variablesMergeForm.StoreMergeControl(VariablesMergeForm.VARIABLES));
                        WriteInfoToInfoFile(VariablesMergeForm.ACRONYMS, _variablesMergeForm.StoreMergeControl(VariablesMergeForm.ACRONYMS));
                        WriteInfoToInfoFile(VariablesMergeForm.COUNTRY_LABELS, _variablesMergeForm.StoreMergeControl(VariablesMergeForm.COUNTRY_LABELS));
                        
                    }
                    
                    //WriteInfoToInfoFile(VariablesMergeForm.SWITCHABLE_POLICIES, _variablesMergeForm.StoreMergeControl(VariablesMergeForm.SWITCHABLE_POLICIES));
                }
                else
                {
                    _mergeForm.Cursor = Cursors.WaitCursor;
                    WriteInfoToInfoFile(MERGEINFO_COUNTRY, _mergeForm.StoreCountryControls());
                    WriteInfoToInfoFile(MergeForm.SYSTEMS, _mergeForm.StoreMergeControl(MergeForm.SYSTEMS));
                    if (!_isAddOn) WriteInfoToInfoFile(MergeForm.DATASETS, _mergeForm.StoreMergeControl(MergeForm.DATASETS));
                    WriteInfoToInfoFile(MergeForm.SPINE, _mergeForm.StoreMergeControl(MergeForm.SPINE));
                    WriteInfoToInfoFile(MergeForm.CONDITIONAL_FORMATTING, _mergeForm.StoreMergeControl(MergeForm.CONDITIONAL_FORMATTING));
                    WriteInfoToInfoFile(MergeForm.UPRATING_INDICES, _mergeForm.StoreMergeControl(MergeForm.UPRATING_INDICES));
                    if (!_isAddOn)
                    {
                        WriteInfoToInfoFile(MergeForm.EXTENSIONS, _mergeForm.StoreMergeControl(MergeForm.EXTENSIONS));
                        WriteInfoToInfoFile(MergeForm.EXT_SWITCHES, _mergeForm.StoreMergeControl(MergeForm.EXT_SWITCHES));
                        WriteInfoToInfoFile(MergeForm.LOOK_GROUPS, _mergeForm.StoreMergeControl(MergeForm.LOOK_GROUPS));
                    }
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);

                foreach (FileInfo file in new DirectoryInfo(FOLDER_MERGE).GetFiles(PREFIX_MERGE_INFO_FILE + "*"))
                    file.Delete(); //delete already generated info, to avoid that difficulties in loading these not complete info-files
            }
            finally { if (_mergeConfigFile)
                {
                    if (_configFileName.Equals(EMPath.EM2_FILE_HICP))
                    {
                        _hicpConfigMergeForm.Cursor = Cursors.Default;
                    }
                    else if (_configFileName.Equals(EMPath.EM2_FILE_EXRATES))
                    {
                        _exchangeRatesConfigMergeForm.Cursor = Cursors.Default;
                    }
                    else if (_configFileName.Equals(EMPath.EM2_FILE_EXTENSIONS))
                    {
                        _switchablePolicyConfigMergeForm.Cursor = Cursors.Default;
                    }
                    else
                    {
                        _variablesMergeForm.Cursor = Cursors.Default;

                    }

                }
                else _mergeForm.Cursor = Cursors.Default; }
        }

        void GetInfoFromXml(string mergeControlName, out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            switch (mergeControlName)
            {
                case MergeForm.SYSTEMS: GetInfoSystemsFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.DATASETS: GetInfoDatasetsFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.SPINE: GetInfoSpineFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.CONDITIONAL_FORMATTING: GetInfoCondFormatFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.UPRATING_INDICES: GetInfoUpratingIndicesFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.EXTENSIONS: GetInfoExtensionsFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.EXT_SWITCHES: GetInfoExtSwitchesFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case MergeForm.LOOK_GROUPS: GetInfoGroupsFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case VariablesMergeForm.VARIABLES: GetInfoVariablesFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case VariablesMergeForm.ACRONYMS: GetInfoAcronymsFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case VariablesMergeForm.COUNTRY_LABELS: GetInfoCountryLabelsFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case SwitchablePolicyConfigMergeForm.SWITCHABLEPOLICYCONFIG: GetInfoSwitchablePoliciesFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case HICPConfigMergeForm.HICPCONFIG: GetInfoHicpConfigFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                case ExchangeRatesConfigMergeForm.EXCHANGERATESCONFIG: GetInfoExchangeRatesConfigFromXml(out columInfo, out nodeInfoLocal, out nodeInfoRemote); break;
                default: columInfo = null; nodeInfoLocal = null; nodeInfoRemote = null; break;
            }
        }

        internal void ShowChoices(bool createNewBundleMenu)
        {
            //if merging variables check if variables-form is open (and perhaps editing in-progress)
            if (_mergeConfigFile && EM_AppContext.Instance.IsVariablesFormOpen()) 
            {
                UserInfoHandler.ShowInfo("Please finalise editing variables (close dialog) before merging.");
                return;
            }
            //if merging (necessarily open!) country or add-on check for not saved changes
            if (!_mergeConfigFile && _mainForm.HasChanges())
            {
                switch (Tools.UserInfoHandler.GetInfo("Do you want to save changes?", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel: return;
                    case DialogResult.Yes: _mainForm.WriteXml(); break;
                }
            }

            bool showSelectVersionDialog = true;

            //In the case of the config files, first we need to check if there is one or more in-progress merging
            List<string> foundMergeConfigFiles = new List<string>();
            if (_mergeConfigFile)
            {
                DirectoryInfo configFolder = new DirectoryInfo(FOLDER_MERGE);
                FileSystemInfo[] filesAndDirs = configFolder.GetFileSystemInfos("MERGE_" + "*");
                
                if((filesAndDirs != null && filesAndDirs.Length > 0))
                {

                    foreach (FileSystemInfo foundFile in filesAndDirs)
                    {
                        string fileName = Path.GetFileName(foundFile.FullName.Replace("MERGE_", "") + ".xml");
                        foundMergeConfigFiles.Add(fileName);
                    }

                    
                }
            }

            try
            {
                if (!_mergeConfigFile && Directory.Exists(FOLDER_MERGE)) //check if folder ...\cc\Merge\ exists 
                {
                    bool outdated = false;
                    if (!AreInfoFilesAvailable()) //if the user has not started any consolidating of the two versions (not pressed button Save)
                    {                             //refresh the files in the folder ...\cc\Merge\cc_Local\, to be sure they correspond with the UI's current version
                        if (_mergeConfigFile) CopyVarFiles(); else CopyCCAOFiles();
                    }
                    else outdated = !AreLocalFilesUpToDate();
                    
                    switch (UserInfoHandler.GetInfo("Info on an in-progress merging found." +
                           (outdated ? Environment.NewLine + "Note that this info is possibly not up-to-date!" : string.Empty) +
                           Environment.NewLine + Environment.NewLine + "Do you want to use this info?" + Environment.NewLine + Environment.NewLine +
                           "Please note that choosing 'No' irrevocably deletes the merge-environment.", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Cancel: return;
                        case DialogResult.Yes: showSelectVersionDialog = false; break;
                        case DialogResult.No: DeleteMergeFolder(true, true); break;
                        
                    }
                }
                else if (_mergeConfigFile)
                {
                    bool continueLoop = true ;
                    foreach(string foundMergeConfigFile in foundMergeConfigFiles)
                    {
                        if (!continueLoop) break;
                        _configFileName = new EMPath(EM_AppContext.FolderEuromodFiles).FormatAnyConfigFileName(foundMergeConfigFile);
                        if (Directory.Exists(FOLDER_MERGE)) //check if folder ...\cc\Merge\ exists
                        {
                            bool outdated = false;
                            if (!AreInfoFilesAvailable()) //if the user has not started any consolidating of the two versions (not pressed button Save)
                            {                             //refresh the files in the folder ...\cc\Merge\cc_Local\, to be sure they correspond with the UI's current version
                                if (_mergeConfigFile) CopyVarFiles(); else CopyCCAOFiles();
                            }
                            else outdated = !AreLocalFilesUpToDate();

                            switch (UserInfoHandler.GetInfo("Info on an in-progress merging found: "+ _configFileName +
                                   (outdated ? Environment.NewLine + "Note that this info is possibly not up-to-date!" : string.Empty) +
                                   Environment.NewLine + Environment.NewLine + "Do you want to use this info?" + Environment.NewLine + Environment.NewLine +
                                   "Please note that choosing 'No' irrevocably deletes the merge-environment.", MessageBoxButtons.YesNoCancel))
                            {
                                case DialogResult.Cancel: return;
                                case DialogResult.No: DeleteMergeFolder(true, true); break;
                                case DialogResult.Yes:
                                    {
                                        if (_configFileName == EMPath.EM2_FILE_EXTENSIONS)
                                        {
                                            if (EM_AppContext.Instance.IsAnythingOpen(false))
                                            {
                                                if (!EM_AppContext.Instance.CloseAnythingOpen()) { return; }

                                            }
                                        }
                                        showSelectVersionDialog = false; continueLoop = false; break;
                                    }
                            }
                        }
                    }

                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }

            string localBundlePath = EM_AppContext.Instance.GetVCAdministrator()._vcAPI.getLocalBundlePath();
            if (createNewBundleMenu && !localBundlePath.Equals(String.Empty) && showSelectVersionDialog)
            {
                {
                    switch (UserInfoHandler.GetInfo("Would you like to compare this online bundle with the following local bundle: " + localBundlePath + "?", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Cancel: return;
                        case DialogResult.No: createNewBundleMenu = false; break;
                        case DialogResult.Yes: break;
                    }
                }
            }

            if (showSelectVersionDialog)
                if (!ShowSelectVersionDialog(createNewBundleMenu))
                    return; //user canceled the process

            if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_VARS) { ShowVariablesMergeDialog(); }
            else if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_HICP) { ShowHICPConfigMergeDialog(); }
            else if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_EXRATES) { ShowExchangeRatesConfigDialog(); }
            else if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_EXTENSIONS) {
                if (EM_AppContext.Instance.IsAnythingOpen(false))
                {
                    //UserInfoHandler.ShowInfo("Merging SWITCHABLEPOLICYCONFIG.xml needs all countries to be closed.");
                    if (!EM_AppContext.Instance.CloseAnythingOpen()) { return; }
                }
                ShowSwitchablePolicyConfigMergeDialog();
            }
            else {
                ShowMergeDialog();

            }
        }

        bool AreLocalFilesUpToDate()

        {
            if (_mergeConfigFile)
            {

                string[] files = Directory.GetFiles(FOLDER_LOCAL);
                if (files != null && files.Length == 1)
                {

                    string fileNameRemoteVersion = Path.GetFileName(files[0]);
                    _configFileName = fileNameRemoteVersion;
                }

                if (_configFileName.Equals(EMPath.EM2_FILE_HICP))
                {
                    return EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_HICP, new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));
                }
                else if(_configFileName.Equals(EMPath.EM2_FILE_VARS))
                {
                    return EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_VARS, new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true));
                }
                else if (_configFileName.Equals(EMPath.EM2_FILE_EXRATES))
                {
                    return EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_EXRATES, new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));
                }
                else if (_configFileName.Equals(EMPath.EM2_FILE_EXTENSIONS))
                {
                    return EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_EXTENSIONS, new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));

                }
                else //The type of config file is not known 
               {
                    bool areHasesEqual = false;
                    try { areHasesEqual = EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_HICP, new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true)); _configFileName = EMPath.EM2_FILE_HICP; return areHasesEqual; } catch (Exception e) { string exceMsg = e.ToString(); }
                    try { areHasesEqual = EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_VARS, new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true)); _configFileName = EMPath.EM2_FILE_VARS; return areHasesEqual; } catch (Exception e) { string exceMsg = e.ToString(); }
                    try { areHasesEqual = EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_EXRATES, new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true)); _configFileName = EMPath.EM2_FILE_EXRATES; return areHasesEqual; } catch (Exception e) { string exceMsg = e.ToString(); }
                    try { areHasesEqual = EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_EXTENSIONS, new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true)); _configFileName = EMPath.EM2_FILE_EXTENSIONS; return areHasesEqual; } catch (Exception e) { string exceMsg = e.ToString(); }
                }
            }

            //merging country or add-on


            //if it is country, the switchablepolicyconfig file also needs to be cheched
            if (!EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + CountryAdministrator.GetCountryFileName(_shortName),
                          CountryAdministrator.GetCountryPath(_shortName) + CountryAdministrator.GetCountryFileName(_shortName)))
                return false;

            //if it is addon
            if (!_isAddOn)
            {
                if(!EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + CountryAdministrator.GetDataFileName(_shortName),
                                        CountryAdministrator.GetCountryPath(_shortName) + CountryAdministrator.GetDataFileName(_shortName)))
                    return false;
                try
                {
                    if (!EM_Helpers.AreFileHashesEqual(FOLDER_LOCAL + EMPath.EM2_FILE_EXTENSIONS,
                        new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(EMPath.EM2_FILE_EXTENSIONS, true)))
                        return false;
                }
                catch (Exception e) {
                    string msg = e.Message; //If the switchable policy has not been copied (because the country is not within a EUROMOD bundle 
                }

            } 

            return true;

        }

        bool ShowSelectVersionDialog(bool createNewBundle)
        {
            string pathRemoteVersion = string.Empty;
            string nameRemoteVersion = string.Empty;
            string pathParentVersion = string.Empty;
            string nameParentVersion= string.Empty;
            bool useLocalAsParent = false;
            bool useRemoteAsParent = false;

            if (!createNewBundle)
            {

                MergeChoicesForm mergeChoicesForm = new MergeChoicesForm(_mergeConfigFile, _isAddOn);
                if (_mergeConfigFile) mergeChoicesForm.ShowSkipCheckCountryLabels(); //allows for skipping the time-consuming check for changes in country-labels

                if (mergeChoicesForm.ShowDialog() == DialogResult.Cancel)
                    return false;


                mergeChoicesForm.GetInfo(out pathRemoteVersion, out nameRemoteVersion, out pathParentVersion, out nameParentVersion,
                                         out useLocalAsParent, out useRemoteAsParent);
                _checkCountryLabels = mergeChoicesForm.CheckCountryLabels() ? 1 : 0;

                if (_mergeConfigFile)
                {
                    _configFileName = new EMPath(EM_AppContext.FolderEuromodFiles).FormatAnyConfigFileName(nameRemoteVersion);
                }
            }
            else
            {
                bool oldStyleAddon = false;
                if (_isAddOn)
                {
                    oldStyleAddon = CountryAdministrator.ConsiderOldAddOnFileStructure(_isAddOn);
                }

                if (_mergeConfigFile)
                {
                    VCSelectConfigFile vcSelectConfigFile = new VCSelectConfigFile();
                    if (vcSelectConfigFile.ShowDialog() == DialogResult.Cancel) return false;

                    string configFileChosen = string.Empty;
                    vcSelectConfigFile.GetChosenConfigFile(out configFileChosen);
                    //pathRemoteVersion = EM_AppContext.Instance.GetVCAdministrator()._vcAPI.getVariablePathLocalBundle();
                    pathRemoteVersion = EMPath.Folder_Config(EM_AppContext.Instance.GetVCAdministrator()._vcAPI.getLocalBundlePath());



                    _configFileName = new EMPath(EM_AppContext.FolderEuromodFiles).FormatAnyConfigFileName(configFileChosen);
                    nameRemoteVersion = _configFileName;

                    if (pathRemoteVersion != "" && !File.Exists(pathRemoteVersion+ "\\" + configFileChosen))
                    {
                        Tools.UserInfoHandler.ShowError("Failed to load add-on XML-files from '" + pathRemoteVersion + "'. The remote bundle may not contain the selected variable.");
                        return false;
                    }
                }
                else if(_isAddOn && oldStyleAddon)
                {
                    pathRemoteVersion = EM_AppContext.Instance.GetVCAdministrator()._vcAPI.getOldAddonPathLocalBundle(_shortName);

                    if (pathRemoteVersion != "" && !File.Exists(pathRemoteVersion))
                    {
                        Tools.UserInfoHandler.ShowError("Failed to load add-on XML-files from '" + pathRemoteVersion + "'. The remote bundle may not contain the selected Addon.");
                        return false;
                    }
                }
                else if (_isAddOn && !oldStyleAddon)
                {
                    pathRemoteVersion = EMPath.Folder_AddOns(EM_AppContext.Instance.GetVCAdministrator()._vcAPI.getLocalBundlePath()) + _shortName;
                    nameRemoteVersion = _shortName;

                    if (pathRemoteVersion != "" && !Directory.Exists(pathRemoteVersion))
                    {
                        Tools.UserInfoHandler.ShowError("Failed to load add-on XML-files from '" + pathRemoteVersion + "'. The remote bundle may not contain the selected Addon.");
                        return false;
                    }
                }
                else{
                   
                   
                    pathRemoteVersion = EMPath.Folder_Countries(EM_AppContext.Instance.GetVCAdministrator()._vcAPI.getLocalBundlePath()) + _shortName;

                    nameRemoteVersion = _shortName;

                    if (pathRemoteVersion != "" && !Directory.Exists(pathRemoteVersion))
                    {
                        Tools.UserInfoHandler.ShowError("Failed to load add-on XML-files from '" + pathRemoteVersion + "'. The remote bundle may not contain the selected country.");
                        return false;
                    }
                }
                useRemoteAsParent = false;
                useLocalAsParent = true;

               
                _checkCountryLabels = 1;
            }

            _pathRemoteVersion = pathRemoteVersion;
            return GenerateMergeFileStructure(pathRemoteVersion, nameRemoteVersion, pathParentVersion, nameParentVersion,
                                              useLocalAsParent, useRemoteAsParent);
        }

        internal bool GenerateMergeFileStructure(string pathRemoteVersion, string nameRemoteVersion,
                                                 string pathParentVersion, string nameParentVersion,
                                                 bool useLocalAsParent, bool useRemoteAsParent)
        {
            string errMessage = "REPORT";
            return GenerateMergeFileStructure(pathRemoteVersion, nameRemoteVersion, pathParentVersion, nameParentVersion, useLocalAsParent, useRemoteAsParent,
                                              ref errMessage);
        }

        internal bool GenerateMergeFileStructure(string pathRemoteVersion, string nameRemoteVersion,
                                        string pathParentVersion, string nameParentVersion,
                                        bool useLocalAsParent, bool useRemoteAsParent,
                                        ref string errMessage)
        {
            bool silent = errMessage != "REPORT"; errMessage = string.Empty;
            try
            {
                //generate the folder structure, if necessary: ...\cc\merge\ containing cc_Local\ + cc_Remote\ + cc_Parent\
                if (!Directory.Exists(FOLDER_MERGE)) Directory.CreateDirectory(FOLDER_MERGE);
                if (!Directory.Exists(FOLDER_LOCAL)) Directory.CreateDirectory(FOLDER_LOCAL);
                if (!Directory.Exists(FOLDER_REMOTE)) Directory.CreateDirectory(FOLDER_REMOTE);
                if (!Directory.Exists(FOLDER_PARENT)) Directory.CreateDirectory(FOLDER_PARENT);

                //copy the xml-files into the structure
                if (_mergeConfigFile) CopyVarFiles(pathRemoteVersion, nameRemoteVersion, pathParentVersion, nameParentVersion, useLocalAsParent, useRemoteAsParent);
                else CopyCCAOFiles(pathRemoteVersion, nameRemoteVersion, pathParentVersion, nameParentVersion, useLocalAsParent, useRemoteAsParent);
            }
            catch (Exception exception)
            {
                if (silent) errMessage = exception.Message; else UserInfoHandler.ShowException(exception);
                DeleteMergeFolder(true, true); //try to remove any started generation of the structure
                return false;
            }

            return true;
        }

        string GetVarFileFullName(string path, string appendix)
        {
            FileInfo fileInfo = new FileInfo(new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true));
            string fileNameWoExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            return path + fileNameWoExtension + appendix + fileInfo.Extension;
        }

        string GetAnyConfigFileFullName(string path, string fileName, string appendix)
        {
            FileInfo fileInfo = new FileInfo(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(fileName, true));
            string fileNameWoExtension = fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length);
            return path + fileNameWoExtension + appendix + fileInfo.Extension;
        }

        void CopyVarFiles(string pathRemoteVersion = "", string fileNameRemoteVersion = "",
                          string pathParentVersion = "", string fileNameParentVersion = "",
                          bool useLocalAsParent = false, bool useRemoteAsParent = false)
        {
            //Just in case the user has not written the name of the file with proper capital letters
            if(fileNameRemoteVersion != ""){fileNameRemoteVersion = new EMPath(EM_AppContext.FolderEuromodFiles).FormatAnyConfigFileName(fileNameRemoteVersion);}

            bool updateLocalFiles = pathRemoteVersion == string.Empty;
            if (updateLocalFiles) //if yes: function is used to up-date local files (in folder ...\cc\merge\cc_local\) with current version of UI
            { //check whether local version corresponds with parent version, in which case it (i.e. files in folder ...\cc\merge\cc_parent\) must be updated too
                /*string[] files = Directory.GetFiles(FOLDER_LOCAL);
                if(files != null && files.Length == 1)
                {
                    fileNameRemoteVersion = Path.GetFileName(files[0]);
                    _configFileName = fileNameRemoteVersion;
                }*/
                if(!_configFileName.Equals("") && fileNameRemoteVersion.Equals("")){
                    fileNameRemoteVersion = _configFileName;
                }
                FileInfo fileInfoLocal = new FileInfo(FOLDER_LOCAL + fileNameRemoteVersion);
                FileInfo fileInfoParent = new FileInfo(GetVarFileFullName(FOLDER_PARENT, APPENDIX_PARENT));
                if (fileInfoLocal.LastAccessTime == fileInfoParent.LastAccessTime) useLocalAsParent = true;
            }

            string pathLocalVersion = EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles);

            pathRemoteVersion = EMPath.AddSlash(pathRemoteVersion);

            if (useLocalAsParent) { pathParentVersion = pathLocalVersion; fileNameParentVersion = _configFileName; }
            else if (useRemoteAsParent) { pathParentVersion = pathRemoteVersion; fileNameParentVersion = fileNameRemoteVersion; }
            else if (!updateLocalFiles) pathParentVersion = EMPath.AddSlash(pathParentVersion);

            File.Copy(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true), FOLDER_LOCAL + _configFileName, true);
            FileInfo fileInfo = new FileInfo(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));
            if (pathRemoteVersion != string.Empty) File.Copy(pathRemoteVersion + fileNameRemoteVersion, GetAnyConfigFileFullName(FOLDER_REMOTE, _configFileName, APPENDIX_REMOTE), true);
            if (pathParentVersion != string.Empty) File.Copy(pathParentVersion + fileNameParentVersion, GetAnyConfigFileFullName(FOLDER_PARENT, _configFileName, APPENDIX_PARENT), true);
            _pathLocalVersion = new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true);
        }

        void CopyCCAOFiles(string pathRemoteVersion = "", string shortNameRemoteVersion = "",
                                    string pathParentVersion = "", string shortNameParentVersion = "",
                                    bool useLocalAsParent = false, bool useRemoteAsParent = false)
        {
            bool updateLocalFiles = pathRemoteVersion == string.Empty;
            if (updateLocalFiles) //if yes: function is used to up-date local files (in folder ...\cc\merge\cc_local\) with current version of UI
            { //check whether local version corresponds with parent version, in which case it (i.e. files in folder ...\cc\merge\cc_parent\) must be updated too
                FileInfo fileInfoLocal = new FileInfo(FOLDER_LOCAL + Country.GetCountryXMLFileName(_shortName));
                FileInfo fileInfoParent = new FileInfo(FOLDER_PARENT + Country.GetCountryXMLFileName(_shortName + APPENDIX_PARENT));
                if (fileInfoLocal.LastWriteTime == fileInfoParent.LastWriteTime) useLocalAsParent = true;
            }

            string pathLocalVersion = CountryAdministrator.GetCountryPath(_shortName);
            _pathLocalVersion = pathLocalVersion;
            pathRemoteVersion = EMPath.AddSlash(pathRemoteVersion);

            if (useLocalAsParent) { pathParentVersion = pathLocalVersion; shortNameParentVersion = _shortName; }
            else if (useRemoteAsParent) { pathParentVersion = pathRemoteVersion; shortNameParentVersion = shortNameRemoteVersion; }
            else if (!updateLocalFiles) pathParentVersion = EMPath.AddSlash(pathParentVersion);

            File.Copy(pathLocalVersion + Country.GetCountryXMLFileName(_shortName),
                          FOLDER_LOCAL + Country.GetCountryXMLFileName(_shortName), true);
            if (pathRemoteVersion != string.Empty)
                File.Copy(pathRemoteVersion + Country.GetCountryXMLFileName(shortNameRemoteVersion),
                          FOLDER_REMOTE + Country.GetCountryXMLFileName(_shortName + APPENDIX_REMOTE), true);
            if (pathParentVersion != string.Empty)
                File.Copy(pathParentVersion + Country.GetCountryXMLFileName(shortNameParentVersion),
                          FOLDER_PARENT + Country.GetCountryXMLFileName(_shortName + APPENDIX_PARENT), true);
            if (_isAddOn) return;
            
            File.Copy(pathLocalVersion + Country.GetDataConfigXMLFileName(_shortName),
                            FOLDER_LOCAL + Country.GetDataConfigXMLFileName(_shortName), true);
            if (pathRemoteVersion != string.Empty)
                File.Copy(pathRemoteVersion + Country.GetDataConfigXMLFileName(shortNameRemoteVersion),
                            FOLDER_REMOTE + Country.GetDataConfigXMLFileName(_shortName + APPENDIX_REMOTE), true);
            if (pathParentVersion != string.Empty)
                File.Copy(pathParentVersion + Country.GetDataConfigXMLFileName(shortNameParentVersion),
                            FOLDER_PARENT + Country.GetDataConfigXMLFileName(_shortName + APPENDIX_PARENT), true);

            //SwitchablePolicy files also need to be copied
            bool copied = false;

            //Remote
            if (pathRemoteVersion != string.Empty)
            {
                string switchableConfigPathRemote = getSwitchableConfigPathFromCountryPath(pathRemoteVersion);
                copied = CopySwitchablePolicyConfigFile(switchableConfigPathRemote, FOLDER_REMOTE, APPENDIX_REMOTE);
            }

            
            if (copied)
            {
                //Local
                string switchableConfigPathLocal = getSwitchableConfigPathFromCountryPath(pathLocalVersion);
                CopySwitchablePolicyConfigFile(switchableConfigPathLocal, FOLDER_LOCAL, "");

                //Parent
                if (pathParentVersion != string.Empty)
                {
                    string switchableConfigPathParent = getSwitchableConfigPathFromCountryPath(pathParentVersion);
                    CopySwitchablePolicyConfigFile(switchableConfigPathParent, FOLDER_PARENT, APPENDIX_PARENT);
                }

            }
            
        }

        internal static bool CopySwitchablePolicyConfigFile(string switchableConfigPath, string pathCountryVersion, string appendix)
        {
            bool copied = false;
            if(switchableConfigPath != string.Empty)
            {
                try
                {
                    File.Copy(switchableConfigPath + ".xml", pathCountryVersion + SwitchablePolicyConfigMergeForm.SWITCHABLEPOLICYCONFIG + appendix + ".xml", true);
                    copied = true;
                }
                catch (Exception e)
                {
                    string msg = e.Message;
                    UserInfoHandler.ShowInfo("SwitchablePolicyConfig.xml file cannot be found. Global extensions will not be merged.");
                }
            }
            else
            {
                UserInfoHandler.ShowInfo("SwitchablePolicyConfig.xml file cannot be found. Global extensions will not be merged.");
            }
            return copied;
            
        }

        public static string getSwitchableConfigPathFromCountryPath(string countryPath)
        {
            string switchableConfigPath = string.Empty;
            try
            {
                int countryIndex = countryPath.ToLower().LastIndexOf(EMPath.GetCountriesFolderName().ToLower());
                switchableConfigPath = countryPath.Substring(0, countryIndex) + EMPath.GetConfigFolderName() + "\\" + SwitchablePolicyConfigMergeForm.SWITCHABLEPOLICYCONFIG;
                
            }
            catch (Exception)
            {
                return switchableConfigPath = string.Empty;
            }
            return switchableConfigPath;
        }

        internal void ShowVariablesMergeDialog()
        {
            try
            {
                _mainForm.Cursor = Cursors.WaitCursor;

                bool mergeInfoAvailabe = AreInfoFilesAvailable();
                if (!mergeInfoAvailabe) ReadXmlVariables();

                _variablesMergeForm = new VariablesMergeForm(this);

                //fill merge-controls
                FillMergeControl(!mergeInfoAvailabe, VariablesMergeForm.VARIABLES);

                FillMergeControl(!mergeInfoAvailabe, VariablesMergeForm.ACRONYMS, new List<string>() { "Type", "Level", "Acronym", "Category" });

                FillMergeControl(!mergeInfoAvailabe, VariablesMergeForm.COUNTRY_LABELS);


                if (_variablesMergeForm.HasDifferences())
                    _variablesMergeForm.ShowDialog();
                else
                {
                    UserInfoHandler.ShowSuccess("No differences found between local and remote version.");
                    DeleteMergeFolder(false, false);

                    //request if this solved a VC-conflict and therefore is ready for uploading
                    EM_AppContext.Instance.GetVCAdministrator().CheckForUploading(GetNameforVC(),
                        _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY));
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            finally { _mainForm.Cursor = Cursors.Default; }
        }


        internal void ShowHICPConfigMergeDialog()
        {
            try
            {
                _mainForm.Cursor = Cursors.WaitCursor;

                bool mergeInfoAvailabe = AreInfoFilesAvailable();
                if (!mergeInfoAvailabe) ReadXmlVariables();

                _hicpConfigMergeForm = new HICPConfigMergeForm(this);

                //fill merge-controls
                FillMergeControl(!mergeInfoAvailabe, HICPConfigMergeForm.HICPCONFIG);

                //progressBar.Stop();

                if (_hicpConfigMergeForm.HasDifferences())
                    _hicpConfigMergeForm.ShowDialog();
                else
                {
                    UserInfoHandler.ShowSuccess("No differences found between local and remote version.");
                    DeleteMergeFolder(false, false);

                    //request if this solved a VC-conflict and therefore is ready for uploading
                    EM_AppContext.Instance.GetVCAdministrator().CheckForUploading(GetNameforVC(),
                        _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY));
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            finally { _mainForm.Cursor = Cursors.Default; }
        }

        internal void ShowExchangeRatesConfigDialog()
        {
            try
            {
                _mainForm.Cursor = Cursors.WaitCursor;

                bool mergeInfoAvailabe = AreInfoFilesAvailable();
                if (!mergeInfoAvailabe) ReadXmlVariables();

                _exchangeRatesConfigMergeForm = new ExchangeRatesConfigMergeForm(this);

                //fill merge-controls
                FillMergeControl(!mergeInfoAvailabe, ExchangeRatesConfigMergeForm.EXCHANGERATESCONFIG);

                //progressBar.Stop();

                if (_exchangeRatesConfigMergeForm.HasDifferences())
                    _exchangeRatesConfigMergeForm.ShowDialog();
                else
                {
                    UserInfoHandler.ShowSuccess("No differences found between local and remote version.");
                    DeleteMergeFolder(false, false);

                    
                    //request if this solved a VC-conflict and therefore is ready for uploading
                    EM_AppContext.Instance.GetVCAdministrator().CheckForUploading(GetNameforVC(),
                        _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY));
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            finally { _mainForm.Cursor = Cursors.Default; }
        }

        internal void ShowSwitchablePolicyConfigMergeDialog()
        {
            try
            {
                _mainForm.Cursor = Cursors.WaitCursor;

                bool mergeInfoAvailabe = AreInfoFilesAvailable();
                if (!mergeInfoAvailabe) ReadXmlVariables();

                _switchablePolicyConfigMergeForm = new SwitchablePolicyConfigMergeForm(this);

                //fill merge-controls
                FillMergeControl(!mergeInfoAvailabe, SwitchablePolicyConfigMergeForm.SWITCHABLEPOLICYCONFIG);

                if (_switchablePolicyConfigMergeForm.HasDifferences())
                    _switchablePolicyConfigMergeForm.ShowDialog();
                else
                {
                    UserInfoHandler.ShowSuccess("No differences found between local and remote version.");
                    DeleteMergeFolder(false, false);


                    //request if this solved a VC-conflict and therefore is ready for uploading
                    EM_AppContext.Instance.GetVCAdministrator().CheckForUploading(GetNameforVC(),
                        _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY));
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            finally { _mainForm.Cursor = Cursors.Default; }
        }

        internal void ShowMergeDialog()
        {
            try
            {
                _mainForm.Cursor = Cursors.WaitCursor;

                bool mergeInfoAvailabe = AreInfoFilesAvailable();
                if (!mergeInfoAvailabe)
                {
                    ReadXmlCountry();
                    if (!_isAddOn) ReadXmlData();
                }

                _mergeForm = new MergeForm(this);

                //fill controls indicating local and remote country-name and whether country is private
                if (mergeInfoAvailabe)
                    GetInfoCountryFromInfoFile();
                else
                    GetInfoCountryFromXml();


                //fill merge-controls
                FillMergeControl(!mergeInfoAvailabe, MergeForm.SYSTEMS);

                if (!_isAddOn) FillMergeControl(!mergeInfoAvailabe, MergeForm.DATASETS, new List<string>() { "Dataset", "Data-System-Combination", "Policy Switch" });

                FillMergeControl(!mergeInfoAvailabe, MergeForm.SPINE, new List<string>() { "Policy", "Function", "Parameter" }, true);

                FillMergeControl(!mergeInfoAvailabe, MergeForm.CONDITIONAL_FORMATTING);

                FillMergeControl(!mergeInfoAvailabe, MergeForm.UPRATING_INDICES);

                if (!_isAddOn)
                {
                    FillMergeControl(!mergeInfoAvailabe, MergeForm.EXTENSIONS);

                    FillMergeControl(!mergeInfoAvailabe, MergeForm.EXT_SWITCHES);

                    FillMergeControl(!mergeInfoAvailabe, MergeForm.LOOK_GROUPS);

                }

                if (_mergeForm.HasDifferences())
                    _mergeForm.ShowDialog();
                else
                {
                    UserInfoHandler.ShowSuccess("No differences found between local and remote version.");
                    DeleteMergeFolder(true, false);

                    //request if this solved a VC-conflict and therefore is ready for uploading
                    EM_AppContext.Instance.GetVCAdministrator().CheckForUploading(GetNameforVC(),
                        _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY));
                }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); }
            finally { _mainForm.Cursor = Cursors.Default; }
        }

        void FillMergeControl(bool getInfoFromXml, string mergeControlName, List<string> levelInfo = null, bool provideSequenceInfo = false)
        {
            List<MergeControl.ColumnInfo> columInfo; List<MergeControl.NodeInfo> nodeInfoLocal; List<MergeControl.NodeInfo> nodeInfoRemote;
            string contentInfoFile;
            if (getInfoFromXml)
            {
                GetInfoFromXml(mergeControlName, out columInfo, out nodeInfoLocal, out nodeInfoRemote);

                if (_mergeConfigFile && _configFileName.Equals(EMPath.EM2_FILE_VARS)) _variablesMergeForm.SetInfoMergeControl(mergeControlName, columInfo, nodeInfoLocal, nodeInfoRemote, provideSequenceInfo);
                else if (_mergeConfigFile && _configFileName.Equals(EMPath.EM2_FILE_HICP)) { _hicpConfigMergeForm.SetInfoMergeControl(mergeControlName, columInfo, nodeInfoLocal, nodeInfoRemote, provideSequenceInfo); }
                else if (_mergeConfigFile && _configFileName.Equals(EMPath.EM2_FILE_EXRATES)) { _exchangeRatesConfigMergeForm.SetInfoMergeControl(mergeControlName, columInfo, nodeInfoLocal, nodeInfoRemote, provideSequenceInfo); }
                else if(_mergeConfigFile && _configFileName.Equals(EMPath.EM2_FILE_EXTENSIONS)) { _switchablePolicyConfigMergeForm.SetInfoMergeControl(mergeControlName, columInfo, nodeInfoLocal, nodeInfoRemote, provideSequenceInfo); }
                else _mergeForm.SetInfoMergeControl(mergeControlName, columInfo, nodeInfoLocal, nodeInfoRemote, provideSequenceInfo);
            }
            else 
            {
                GetInfoFromInfoFile(mergeControlName, out contentInfoFile);
                if (_mergeConfigFile)
                {
                    if (_configFileName == EMPath.EM2_FILE_VARS) { _variablesMergeForm.RestoreMergeControl(mergeControlName, contentInfoFile); }
                    else if (_configFileName == EMPath.EM2_FILE_HICP) { _hicpConfigMergeForm.RestoreMergeControl(mergeControlName, contentInfoFile); }
                    else if (_configFileName == EMPath.EM2_FILE_EXRATES) { _exchangeRatesConfigMergeForm.RestoreMergeControl(mergeControlName, contentInfoFile); }
                    else if (_configFileName == EMPath.EM2_FILE_EXTENSIONS) { _switchablePolicyConfigMergeForm.RestoreMergeControl(mergeControlName, contentInfoFile); }
                }
                else _mergeForm.RestoreMergeControl(mergeControlName, contentInfoFile);
            }
            if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_VARS) _variablesMergeForm.LoadMergeControl(mergeControlName, levelInfo);
            else if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_HICP) { _hicpConfigMergeForm.LoadMergeControl(mergeControlName, levelInfo); }
            else if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_EXRATES) { _exchangeRatesConfigMergeForm.LoadMergeControl(mergeControlName, levelInfo); }
            else if (_mergeConfigFile && _configFileName == EMPath.EM2_FILE_EXTENSIONS) { _switchablePolicyConfigMergeForm.LoadMergeControl(mergeControlName, levelInfo); }
            else _mergeForm.LoadMergeControl(mergeControlName, levelInfo);
        }

        void ReadXmlVariables()
        {
            if(_configFileName == EMPath.EM2_FILE_HICP)
            {
                _vcFacHICPRemote = new HICPConfigFacade(GetAnyConfigFileFullName(FOLDER_REMOTE, _configFileName, APPENDIX_REMOTE));
                _vcFacHICPRemote.LoadHICPConfig();
                _vcHICPRemote = _vcFacHICPRemote.GetHICPConfig();

                _vcFacHICPLocal = new HICPConfigFacade(GetAnyConfigFileFullName(FOLDER_LOCAL, _configFileName, string.Empty));
                _vcFacHICPLocal.LoadHICPConfig();
                _vcHICPLocal = _vcFacHICPLocal.GetHICPConfig();

                _vcFacHICPParent = new HICPConfigFacade(GetAnyConfigFileFullName(FOLDER_PARENT, _configFileName, APPENDIX_PARENT));
                _vcFacHICPParent.LoadHICPConfig();
                _vcHICPParent = _vcFacHICPParent.GetHICPConfig();
            }
            else if(_configFileName == EMPath.EM2_FILE_VARS)
            {
                _vcFacRemote = new VarConfigFacade(GetAnyConfigFileFullName(FOLDER_REMOTE, _configFileName, APPENDIX_REMOTE));
                _vcFacRemote.LoadVarConfig();
                _vcRemote = _vcFacRemote.GetVarConfig();

                _vcFacLocal = new VarConfigFacade(GetAnyConfigFileFullName(FOLDER_LOCAL, _configFileName, string.Empty));
                _vcFacLocal.LoadVarConfig();
                _vcLocal = _vcFacLocal.GetVarConfig();

                _vcFacParent = new VarConfigFacade(GetAnyConfigFileFullName(FOLDER_PARENT, _configFileName, APPENDIX_PARENT));
                _vcFacParent.LoadVarConfig();
                _vcParent = _vcFacParent.GetVarConfig();
            }else if(_configFileName == EMPath.EM2_FILE_EXRATES)
            {
                _vcFacExchangeRatesRemote = new ExchangeRatesConfigFacade(GetAnyConfigFileFullName(FOLDER_REMOTE, _configFileName, APPENDIX_REMOTE));
                _vcFacExchangeRatesRemote.LoadExchangeRatesConfig();
                _vcExchangeRatesRemote = _vcFacExchangeRatesRemote.GetExchangeRatesConfig();

                _vcFacExchangeRatesLocal = new ExchangeRatesConfigFacade(GetAnyConfigFileFullName(FOLDER_LOCAL, _configFileName, string.Empty));
                _vcFacExchangeRatesLocal.LoadExchangeRatesConfig();
                _vcExchangeRatesLocal = _vcFacExchangeRatesLocal.GetExchangeRatesConfig();

                _vcFacExchangeRatesParent = new ExchangeRatesConfigFacade(GetAnyConfigFileFullName(FOLDER_PARENT, _configFileName, APPENDIX_PARENT));
                _vcFacExchangeRatesParent.LoadExchangeRatesConfig();
                _vcExchangeRatesParent = _vcFacExchangeRatesParent.GetExchangeRatesConfig();
            }else if(_configFileName == EMPath.EM2_FILE_EXTENSIONS)
            {
                _vcFacSwitchablePolicyRemote = new SwitchablePolicyConfigFacade(GetAnyConfigFileFullName(FOLDER_REMOTE, _configFileName, APPENDIX_REMOTE));
                _vcSwitchablePolicyRemote = _vcFacSwitchablePolicyRemote.GetSwitchablePolicyConfig();

                _vcFacSwitchablePolicyLocal = new SwitchablePolicyConfigFacade(GetAnyConfigFileFullName(FOLDER_LOCAL, _configFileName, string.Empty));
                _vcSwitchablePolicyLocal = _vcFacSwitchablePolicyLocal.GetSwitchablePolicyConfig();

                _vcFacSwitchablePolicyParent = new SwitchablePolicyConfigFacade(GetAnyConfigFileFullName(FOLDER_PARENT, _configFileName, APPENDIX_PARENT));
                _vcSwitchablePolicyParent = _vcFacSwitchablePolicyParent.GetSwitchablePolicyConfig();
            }
            
        }

        void ReadXmlCountry()
        {
            _ccFacRemote.ReadXml(FOLDER_REMOTE, Country.GetCountryXMLFileName(_shortName + APPENDIX_REMOTE));
            _ccRemote = _ccFacRemote.GetCountryConfig();

            _ccFacLocal.ReadXml(FOLDER_LOCAL, Country.GetCountryXMLFileName(_shortName));
            _ccLocal = _ccFacLocal.GetCountryConfig();            

            _ccFacParent.ReadXml(FOLDER_PARENT, Country.GetCountryXMLFileName(_shortName + APPENDIX_PARENT));
            _ccParent = _ccFacParent.GetCountryConfig();


            if (File.Exists(FOLDER_REMOTE + "/SWITCHABLEPOLICYCONFIG" + APPENDIX_REMOTE + ".xml")) { 
                _vcFacSwitchablePolicyLocal = new SwitchablePolicyConfigFacade(FOLDER_LOCAL + "/SWITCHABLEPOLICYCONFIG" + ".xml");
                _vcFacSwitchablePolicyRemote = new SwitchablePolicyConfigFacade(FOLDER_REMOTE + "/SWITCHABLEPOLICYCONFIG" + APPENDIX_REMOTE + ".xml");
                _vcFacSwitchablePolicyParent = new SwitchablePolicyConfigFacade(FOLDER_PARENT + "/SWITCHABLEPOLICYCONFIG" + APPENDIX_PARENT + ".xml");
            }
        }

        void ReadXmlData(bool remoteOnly = false)
        {
            _dcFacRemote.ReadXml(FOLDER_REMOTE, Country.GetDataConfigXMLFileName(_shortName + APPENDIX_REMOTE));
            _dcRemote = _dcFacRemote.GetDataConfig();

            if (remoteOnly) return;

            _dcFacLocal.ReadXml(FOLDER_LOCAL, Country.GetDataConfigXMLFileName(_shortName));
            _dcLocal = _dcFacLocal.GetDataConfig();

            _dcFacParent.ReadXml(FOLDER_PARENT, Country.GetDataConfigXMLFileName(_shortName + APPENDIX_PARENT));
            _dcParent = _dcFacParent.GetDataConfig();
        }

        void GetOrReadXml()
        {
            if (_ccLocal != null) return; //already read
            ReadXmlCountry();
            if (!_isAddOn) ReadXmlData();
        }

        void GetOrReadXmlVariables() { if (_vcLocal == null) ReadXmlVariables(); }

        void GetInfoCountryFromInfoFile()
        {
            string content = ReadFromInfoFile(MERGEINFO_COUNTRY); ;
            _mergeForm.RestoreCountryControls(content);
        }

        void GetInfoCountryFromXml()
        {
            string nameLocal = _ccLocal.Country.First().Name;
            string nameRemote = _ccRemote.Country.First().Name;
            string nameParent = _ccParent.Country.First().Name;
            string privLocal = _ccLocal.Country.First().Private;
            string privRemote = _ccRemote.Country.First().Private;
            string privParent = _ccParent.Country.First().Private;

            bool nameChangedLocal = nameLocal != nameRemote && nameLocal != nameParent;
            bool nameChangedRemote = nameLocal != nameRemote && nameRemote != nameParent &&
                                     !nameChangedLocal; //this avoids a conflict (i.e. changed in local and remote) - just assume local prevails
            bool privateChangedLocal = privLocal != privRemote && privLocal != privParent;
            bool privateChangedRemote = privLocal != privRemote && privRemote != privParent &&
                                     !privateChangedLocal; //see remark above

            _mergeForm.SetInfoCountryControls(nameLocal, nameRemote,
                privLocal == DefPar.Value.YES, privRemote == DefPar.Value.YES,
                nameChangedLocal, nameChangedRemote, privateChangedLocal, privateChangedRemote);
        }

        void GetInfoVariablesFromXml(out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_vcLocal.Variable.NameColumn);
            settingColumns.Add(_vcLocal.Variable.MonetaryColumn);
            settingColumns.Add(_vcLocal.Variable.AutoLabelColumn);
            settingColumns.Add(_vcLocal.Variable.HHLevelColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //add a node for each variable
            List<MergeControl.ChangeType> ignoredChangeTypes = new List<MergeControl.ChangeType>();
            ignoredChangeTypes.Add(MergeControl.ChangeType.none);
            List<string> localAndRemoteIDs = BuildNodeInfo_List(_vcLocal.Variable, _vcRemote.Variable, _vcParent.Variable,
                          _vcLocal.Variable.IDColumn.ColumnName, _vcLocal.Variable.NameColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns, false, ignoredChangeTypes);
        }

        void GetInfoCountryLabelsFromXml(out List<MergeControl.ColumnInfo> columInfo,
                                         out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            switch (_checkCountryLabels)
            { 
                case 0: return; //do not check
                case 1: break; //check
                default: if (MessageBox.Show(EM_AppContext.Instance.GetTopMostWindow(), "Should the (time-consuming) check for differences in country specific descriptions be skipped?",
                    $"{DefGeneral.BRAND_TITLE} - Request", MessageBoxButtons.YesNo, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly) == DialogResult.Yes) return; break;
            }

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_vcLocal.CountryLabel.VariableIDColumn);
            settingColumns.Add(_vcLocal.CountryLabel.CountryColumn);
            settingColumns.Add(_vcLocal.CountryLabel.LabelColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //add a node for each country-label
            List<MergeControl.ChangeType> ignoredChangeTypes = new List<MergeControl.ChangeType>();
            ignoredChangeTypes.Add(MergeControl.ChangeType.none); ignoredChangeTypes.Add(MergeControl.ChangeType.added); ignoredChangeTypes.Add(MergeControl.ChangeType.removed);
            List<string> localAndRemoteIDs = BuildNodeInfo_List(_vcLocal.CountryLabel, _vcRemote.CountryLabel, _vcParent.CountryLabel,
                          _vcLocal.CountryLabel.IDColumn.ColumnName, _vcLocal.CountryLabel.LabelColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns, false, ignoredChangeTypes);

            //replace variable-id by variable-name
            for (int index = 0; index < nodeInfoLocal.Count; ++index)
            {
                MergeControl.CellInfo varCellLocal = nodeInfoLocal.ElementAt(index).cellInfo.ElementAt(0);
                MergeControl.CellInfo varCellRemote = nodeInfoRemote.ElementAt(index).cellInfo.ElementAt(0);

                VarConfig.VariableRow variable = _vcFacLocal.GetVariableByID(varCellLocal.text);
                if (variable == null) continue; //should not happen

                varCellLocal.SetText(variable.Name);
                varCellRemote.SetText(variable.Name);
            }

            //ADD A NODE FOR EACH ADDED / DELETED COUNTRY (telling that labels for this country were added/removed)
            List<string> countriesLocal = _vcFacLocal.GetCountriesShortNames();
            List<string> countriesRemote = _vcFacRemote.GetCountriesShortNames();
            List<string> countriesParent = _vcFacRemote.GetCountriesShortNames();

            List<KeyValuePair<string, short>> diffCountries = new List<KeyValuePair<string, short>> ();
            const short CC_ADD_LOCAL = 0, CC_DEL_LOCAL = 1, CC_ADD_REMOTE = 2, CC_DEL_REMOTE = 3;
            List<string> addLocal = new List<string>(); List<string> addRemote = new List<string>();
            List<string> delLocal = new List<string>(); List<string> delRemote = new List<string>();
            foreach (string country in countriesLocal)
            {
                if (countriesRemote.Contains(country)) continue;
                if (countriesParent.Contains(country)) diffCountries.Add(new KeyValuePair<string, short>(country, CC_DEL_REMOTE));
                else diffCountries.Add(new KeyValuePair<string, short>(country, CC_ADD_LOCAL));
            }
            foreach (string country in countriesRemote)
            {
                if (countriesLocal.Contains(country)) continue;
                if (countriesParent.Contains(country)) diffCountries.Add(new KeyValuePair<string, short>(country, CC_DEL_LOCAL));
                else diffCountries.Add(new KeyValuePair<string, short>(country, CC_ADD_REMOTE));
            }

            foreach(KeyValuePair<string, short> diffCountry in diffCountries)
            {
                MergeControl.ChangeType changeTypeLocal = MergeControl.ChangeType.none, changeTypeRemote = MergeControl.ChangeType.none;
                MergeControl.ChangeHandling changeHandlingLocal = MergeControl.ChangeHandling.none, changeHandlingRemote = MergeControl.ChangeHandling.none;
                string infoLocal = string.Empty, infoRemote = string.Empty, infoPre = "Labels for country '" + diffCountry.Key + "' "; 
                switch (diffCountry.Value)
                {
                    case CC_ADD_LOCAL: infoLocal = infoPre + "added";
                        changeTypeLocal = MergeControl.ChangeType.added; changeHandlingLocal = MergeControl.ChangeHandling.accept; break;
                    case CC_DEL_LOCAL: infoLocal = infoPre + "removed";
                        changeTypeLocal = MergeControl.ChangeType.removed; changeHandlingLocal = MergeControl.ChangeHandling.accept; break;
                    case CC_ADD_REMOTE: infoRemote = infoPre + "added";
                        changeTypeRemote = MergeControl.ChangeType.added; changeHandlingRemote = MergeControl.ChangeHandling.accept; break;
                    case CC_DEL_REMOTE: infoRemote = infoPre + "removed";
                        changeTypeRemote = MergeControl.ChangeType.removed; changeHandlingRemote = MergeControl.ChangeHandling.accept; break;
                }

                MergeControl.NodeInfo iNodeLocal = new MergeControl.NodeInfo(IDENTIFIER_ADD_DEL_COUNTRY_LABELS + diffCountry.Key,
                                                   string.Empty, null,changeTypeLocal, changeHandlingLocal);
                iNodeLocal.cellInfo.Add(new MergeControl.CellInfo(_vcLocal.CountryLabel.VariableIDColumn.ColumnName, infoLocal));
                MergeControl.NodeInfo iNodeRemote = new MergeControl.NodeInfo(IDENTIFIER_ADD_DEL_COUNTRY_LABELS + diffCountry.Key,
                                                    string.Empty, null, changeTypeRemote, changeHandlingRemote);
                iNodeRemote.cellInfo.Add(new MergeControl.CellInfo(_vcLocal.CountryLabel.VariableIDColumn.ColumnName, infoRemote));
                nodeInfoLocal.Add(iNodeLocal);
                nodeInfoRemote.Add(iNodeRemote);
            }
        }

        void GetInfoAcronymsFromXml(out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_vcLocal.Acronym.NameColumn);
            settingColumns.Add(_vcLocal.Acronym.DescriptionColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //adapt column-names of tables AcronymType, AcronymLevel and Category to correspond to columns of table Acronym
            //this way the whole information can be displayed in two columns: Name and Description
            Dictionary<string, string> typeColumnAliases = new Dictionary<string, string>();
            typeColumnAliases.Add("ShortName", "Name"); typeColumnAliases.Add("LongName", "Description");
            Dictionary<string, string> levelColumnAliases = new Dictionary<string, string>();
            levelColumnAliases.Add("Name", "Description"); //table AcronymLevel has in fact a Name-column, but his is in fact the description (main, status, other, etc.)
                                                           //new Name-column is then built of Index-column (see below)
            Dictionary<string, string> catColumnAliases = new Dictionary<string, string>();
            catColumnAliases.Add("Value", "Name"); //table Category has already a Description-column

            //top-level: add a node for each acro-type
            List<string> localAndRemoteIDs = BuildNodeInfo_List(GetSubTable(_vcLocal.AcronymType, string.Empty, typeColumnAliases),
                                                                GetSubTable(_vcRemote.AcronymType, string.Empty, typeColumnAliases),
                                                                GetSubTable(_vcParent.AcronymType, string.Empty, typeColumnAliases),
                                                                _vcLocal.AcronymType.IDColumn.ColumnName, "Name",
                                                                string.Empty, nodeInfoLocal, nodeInfoRemote, settingColumns);

            //1st sub-level: add nodes for each acro-level
            foreach (string typeID in localAndRemoteIDs)
            {
                string whereClause = "TypeID = '" + typeID + "'"; //ConvertLevelIDToString: the numeric Index-column is transferred to a (new) Name-column (of type string)
                                                                  //the conversion is necessary as BuildNodeInfo uses Field<type>, which requires a concrete type (for simplicity string is assumed for everything)
                List<string> localAndRemoteIDs1 = BuildNodeInfo_List(ConvertLevelIDToString(GetSubTable(_vcLocal.AcronymLevel, whereClause, levelColumnAliases)),
                                    ConvertLevelIDToString(GetSubTable(_vcRemote.AcronymLevel, whereClause, levelColumnAliases)),
                                    ConvertLevelIDToString(GetSubTable(_vcParent.AcronymLevel, whereClause, levelColumnAliases)),
                                    _vcLocal.AcronymLevel.IDColumn.ColumnName, "Name", typeID,
                                    nodeInfoLocal, nodeInfoRemote, settingColumns);

                //2nd sub-level: add nodes for each acronym
                foreach (string levelID in localAndRemoteIDs1)
                {
                    whereClause = "LevelID = '" + levelID + "'";
                    List<string> localAndRemoteIDs2 = BuildNodeInfo_List(GetSubTable(_vcLocal.Acronym, whereClause),
                                       GetSubTable(_vcRemote.Acronym, whereClause),
                                       GetSubTable(_vcParent.Acronym, whereClause),
                                       _vcLocal.Acronym.IDColumn.ColumnName, "Name", levelID,
                                       nodeInfoLocal, nodeInfoRemote, settingColumns);

                    //3rd sub-level: add nodes for each category
                    foreach (string acroID in localAndRemoteIDs2)
                    {
                        whereClause = "AcronymID = '" + acroID + "'";
                        BuildNodeInfo_List(GetSubTable(_vcLocal.Category, whereClause, catColumnAliases),
                                       GetSubTable(_vcRemote.Category, whereClause, catColumnAliases),
                                       GetSubTable(_vcParent.Category, whereClause, catColumnAliases),
                                       "Name", "Name", acroID, nodeInfoLocal, nodeInfoRemote, settingColumns,
                                       true); //this true effects that the nodes' IDs are built as Value#acroID, to be uniquely identifiable
                    }                         //in fact unique identifying is not possible, because Category.Value is not necessarily unique within an Acronym (can be "0-no", "0-nothing" or even "0-no", "0-no")
                }
            }
        }

        DataTable ConvertLevelIDToString(DataTable table)
        {
            table.Columns.Add("Name", typeof(string));
            foreach (DataRow row in table.Rows)
                row.SetField<string>("Name", row.Field<System.Int32>("Index").ToString());
            return table;
        }

        void GetInfoSwitchablePoliciesFromXml(out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_vcSwitchablePolicyLocal.SwitchablePolicy.NamePatternColumn);
            settingColumns.Add(_vcSwitchablePolicyLocal.SwitchablePolicy.LongNameColumn);
            settingColumns.Add(_vcSwitchablePolicyLocal.SwitchablePolicy.LookColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES 
            //add a node for each variable
            List<string> localAndRemoteIDs = BuildNodeInfo_List(_vcSwitchablePolicyLocal.SwitchablePolicy, _vcSwitchablePolicyRemote.SwitchablePolicy, _vcSwitchablePolicyParent.SwitchablePolicy,
                          _vcSwitchablePolicyLocal.SwitchablePolicy.IDColumn.ColumnName, _vcSwitchablePolicyLocal.SwitchablePolicy.NamePatternColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns);
        }

        void GetInfoHicpConfigFromXml(out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();

            settingColumns.Add(_vcHICPLocal.HICP.CountryColumn);
            settingColumns.Add(_vcHICPLocal.HICP.YearColumn);
            settingColumns.Add(_vcHICPLocal.HICP.ValueColumn);
            settingColumns.Add(_vcHICPLocal.HICP.CommentColumn);


            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //add a node for each variable
            List<string> localAndRemoteIDs = BuildNodeInfo_List_HICPConfig(_vcHICPLocal.HICP, _vcHICPRemote.HICP, _vcHICPParent.HICP,
                          _vcHICPLocal.HICP.CountryColumn.ColumnName, _vcHICPLocal.HICP.YearColumn.ColumnName, string.Empty, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns);

        }

        void GetInfoExchangeRatesConfigFromXml(out List<MergeControl.ColumnInfo> columInfo,
                              out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_vcExchangeRatesLocal.ExchangeRates.CountryColumn);
            settingColumns.Add(_vcExchangeRatesLocal.ExchangeRates.June30Column);
            settingColumns.Add(_vcExchangeRatesLocal.ExchangeRates.YearAverageColumn);
            settingColumns.Add(_vcExchangeRatesLocal.ExchangeRates.DefaultColumn);
            settingColumns.Add(_vcExchangeRatesLocal.ExchangeRates.ValidForColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //add a node for each variable
            List<MergeControl.ChangeType> ignoredChangeTypes = new List<MergeControl.ChangeType>();
            ignoredChangeTypes.Add(MergeControl.ChangeType.none);
            List<string> localAndRemoteIDs = BuildNodeInfo_List_HICPConfig(_vcExchangeRatesLocal.ExchangeRates, _vcExchangeRatesRemote.ExchangeRates, _vcExchangeRatesParent.ExchangeRates,
                          _vcExchangeRatesLocal.ExchangeRates.CountryColumn.ColumnName, _vcExchangeRatesLocal.ExchangeRates.ValidForColumn.ColumnName, string.Empty, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns, false, ignoredChangeTypes);
        }



        void GetInfoSystemsFromXml(out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            List<DataColumn> cfSettingColumns = new List<DataColumn>();
            settingColumns.Add(_ccLocal.System.NameColumn);
            settingColumns.Add(_ccLocal.System.YearColumn);
            settingColumns.Add(_ccLocal.System.CurrencyOutputColumn);
            settingColumns.Add(_ccLocal.System.CurrencyParamColumn);
            settingColumns.Add(_ccLocal.System.ExchangeRateEuroColumn);
            settingColumns.Add(_ccLocal.System.HeadDefIncColumn);
            settingColumns.Add(_ccLocal.System.PrivateColumn);
            settingColumns.Add(_ccLocal.System.OrderColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //add a node for each system
            List<string> localAndRemoteIDs = BuildNodeInfo_List(_ccLocal.System, _ccRemote.System, _ccParent.System,
                          _ccLocal.System.IDColumn.ColumnName, _ccLocal.System.NameColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns);

        }

        void GetInfoSpineFromXml(out List<MergeControl.ColumnInfo> columInfo,
                             out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //find the matching systems
            List<string> matchSysIDs = new List<string>();
            foreach (string id in (from loc in _ccLocal.System select loc.ID))
                if ((from rem in _ccRemote.System where rem.ID == id select rem).Count() == 1)
                    matchSysIDs.Add(id);
            if (matchSysIDs.Count() == 0)
                return; //nothing to fill in if there are no matching systems

            // D I S P L A Y   S E T T I N G S   (pol/func/par-name, comment, etc.)

            //BUILD TREE-COLUMNS FOR SETTINGS
            //define the settings which are to be compared
            List<DataColumn> parSettingColumns = new List<DataColumn>(); List<DataColumn> polfuncSettingColumns = new List<DataColumn>();
            parSettingColumns.Add(_ccLocal.Parameter.NameColumn); polfuncSettingColumns.Add(_ccLocal.Policy.NameColumn);
            parSettingColumns.Add(_ccLocal.Parameter.GroupColumn);
            parSettingColumns.Add(_ccLocal.Parameter.CommentColumn); polfuncSettingColumns.Add(_ccLocal.Policy.CommentColumn);
            parSettingColumns.Add(_ccLocal.Parameter.PrivateCommentColumn); polfuncSettingColumns.Add(_ccLocal.Policy.PrivateCommentColumn);
            parSettingColumns.Add(_ccLocal.Parameter.PrivateColumn); polfuncSettingColumns.Add(_ccLocal.Policy.PrivateColumn);
            //parSettingColumns.Add(_ccLocal.Parameter.OrderColumn); polfuncSettingColumns.Add(_ccLocal.Policy.OrderColumn);
            //ColorColumn is not a string-column, therefore Field<string>() does not work - from simplicity just ignore node-colour
            //parSettingColumns.Add(_ccLocal.Parameter.ColorColumn); polfuncSettingColumns.Add(_ccLocal.Policy.ColorColumn);
            parSettingColumns.Add(_ccLocal.Parameter.ValueTypeColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in parSettingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES AND FILL WITH SETTINGS
            //top-level: add a node for each policy
            string whereClause = "SystemID = '" + matchSysIDs.First() + "'";
            string orderClause = "order ASC";
            //string orderClause = "";
            Dictionary<string, string> columnAliases = null;
            List<string> localAndRemoteIDs = BuildNodeInfo_List(GetSubTable(_ccLocal.Policy, whereClause, columnAliases, orderClause),
                          GetSubTable(_ccRemote.Policy, whereClause, columnAliases, orderClause),
                          GetSubTable(_ccParent.Policy, whereClause, columnAliases, orderClause),
                          _ccLocal.Policy.IDColumn.ColumnName, _ccLocal.Policy.NameColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, polfuncSettingColumns);

            //1st sub-level: add nodes for each function
            foreach (string polID in localAndRemoteIDs)
            {
                whereClause = "PolicyID = '" + polID + "'";
                localAndRemoteIDs = BuildNodeInfo_List(GetSubTable(_ccLocal.Function, whereClause, columnAliases, orderClause),
                                    GetSubTable(_ccRemote.Function, whereClause, columnAliases, orderClause),
                                    GetSubTable(_ccParent.Function, whereClause, columnAliases, orderClause),
                                    _ccLocal.Function.IDColumn.ColumnName, _ccLocal.Function.NameColumn.Caption, polID,
                                    nodeInfoLocal, nodeInfoRemote, polfuncSettingColumns);
                TakeCareOfReferencePolicies(nodeInfoLocal, _ccLocal); TakeCareOfReferencePolicies(nodeInfoRemote, _ccRemote);

                //2nd sub-level: add nodes for each parameter
                foreach (string funcID in localAndRemoteIDs)
                {
                    whereClause = "FunctionID = '" + funcID + "'";
                    BuildNodeInfo_List(GetSubTable(_ccLocal.Parameter, whereClause, columnAliases, orderClause),
                                       GetSubTable(_ccRemote.Parameter, whereClause, columnAliases, orderClause),
                                       GetSubTable(_ccParent.Parameter, whereClause, columnAliases, orderClause),
                                       _ccLocal.Parameter.IDColumn.ColumnName, _ccLocal.Parameter.NameColumn.Caption,
                                       funcID, nodeInfoLocal, nodeInfoRemote, parSettingColumns);
                }
            }

            // D I S P L A Y   V A L U E S   P E R   S Y S T E M

            //BUILD TREE-COLUMNS FOR SYSTEMS
            //add a column for each system
            foreach (string sysID in matchSysIDs)
                columInfo.Add(new MergeControl.ColumnInfo((from sys in _ccLocal.System where sys.ID == sysID select sys.Name).First(),
                              sysID, MergeControl.CellType.value));

            //FILL TREE-NODES WITH SYSTEM-VALUES
            //first use a specially structured table to gather and order all policy- and function-switches and parameter-values
            ComponentValueTable componentValueTable = new ComponentValueTable();

            //loop over datasets (local, remote, parent)
            List<CountryConfig> dataLocalRemoteParent = new List<CountryConfig>() { _ccLocal, _ccRemote, _ccParent };
            for (int dataIndex = 0; dataIndex < dataLocalRemoteParent.Count; ++dataIndex)
            {
                CountryConfig dataSet = dataLocalRemoteParent[dataIndex];

                //loop over systems
                foreach (string sysID in matchSysIDs)
                {
                    
                    var query = from sys in dataSet.System where sys.ID == sysID select sys;
                    if (query.Count() == 0)
                        continue; //possible for _ccParent
                    CountryConfig.SystemRow system = query.First(); //first and only
                    bool isMatchSys = sysID == matchSysIDs.First();

                    //loop over policies
                    foreach (CountryConfig.PolicyRow policy in system.GetPolicyRows())
                    {
                        string order = ComposeOrder(dataIndex, policy);
                        PutInComponentValueTable(componentValueTable, policy, dataIndex, sysID, isMatchSys, order, false);

                        //loop over functions
                        foreach (CountryConfig.FunctionRow function in policy.GetFunctionRows())
                        {
                            order = ComposeOrder(dataIndex, policy, function);
                            PutInComponentValueTable(componentValueTable, function, dataIndex, sysID, isMatchSys, order, false);

                            //loop over parameters
                            foreach (CountryConfig.ParameterRow parameter in function.GetParameterRows())
                            {
                                order = ComposeOrder(dataIndex, policy, function, parameter);
                                PutInComponentValueTable(componentValueTable, parameter, dataIndex, sysID, isMatchSys, order, true);
                            }
                        }
                    }
                }
                
            }

            //then use the table to fill the MergeControl with respective info
            List<List<MergeControl.NodeInfo>> infoLocalRemote = new List<List<MergeControl.NodeInfo>>() { nodeInfoLocal, nodeInfoRemote };
            for (int indexInfo = 0; indexInfo < infoLocalRemote.Count; ++indexInfo)
            {
                List<MergeControl.NodeInfo> nodeInfos = infoLocalRemote[indexInfo];
                foreach (MergeControl.NodeInfo nodeInfo in nodeInfos)
                {
                    foreach (ComponentValueTable.Item item in componentValueTable.Get(nodeInfo.ID))
                    {
                        string value = indexInfo == 0 ? item.valueLocal : item.valueRemote;
                        MergeControl.CellInfo cellInfo = new MergeControl.CellInfo(item.systemID, value, item.id);
                        if (item.valueLocal != string.Empty && item.valueRemote != string.Empty)
                        {
                            if (indexInfo == 0)
                                CompareLocalRemote_FillLocal(nodeInfo, cellInfo, item.valueRemote, item.valueParent);
                            else
                                CompareLocalRemote_FillRemote(nodeInfo, cellInfo, item.valueLocal, item.valueParent);
                        }
                        nodeInfo.cellInfo.Add(cellInfo);
                        nodeInfo.nodeSequenceInfo = indexInfo == 0 ? item.originalOrderLocal : item.originalOrderRemote;
                    }
                    //get node change-handling based on cell change-handling
                    nodeInfo.changeHandling = MergeControl.GetNodeChangeHandling(nodeInfo, MergeControl.ChangeHandling.accept);
                }
            }
        }

        void PutInComponentValueTable(ComponentValueTable componentValueTable, DataRow pfp,
                                      int dataIndex, string sysID, bool isMatchSys, string order, bool isParam)
        {
            short iLocal = 0, iRemote = 1, iParent = 2;

            string colIDValue = isParam ? "Value" : "Switch";

            if (dataIndex == iLocal)
            {
                componentValueTable.Add(new ComponentValueTable.Item(
                                            pfp.Field<string>("ID"), //id
                                            pfp.Field<string>("ID"), //idInMatchSys
                                            sysID, order, //obvious
                                            pfp.Field<string>("Order"), //original order local
                                            string.Empty, //original order remote
                                            pfp.Field<string>(colIDValue)), //valueLocal
                                        isMatchSys ? string.Empty : order); //siblingOrder
            }
            else {
                componentValueTable.Insert(new ComponentValueTable.Item(
                                                pfp.Field<string>("ID"), //id
                                                pfp.Field<string>("ID"), //idInMatchSys
                                                sysID, order, //obvious
                                                string.Empty, //original order local
                                                pfp.Field<string>("Order"), //original order remote
                                                string.Empty, //valueLocal
                                                dataIndex == iRemote ? pfp.Field<string>(colIDValue) : string.Empty,  //valueRemote
                                                dataIndex == iParent ? pfp.Field<string>(colIDValue) : string.Empty), //valueParent
                                            pfp.Field<string>("ID"), //twinID
                                            isMatchSys ? string.Empty : order); //siblingOrder
            }
        }

        void TakeCareOfReferencePolicies(List<MergeControl.NodeInfo> nodeInfoList, CountryConfig countryConfig)
        {//rather inelegant and tedious way to display "=> reference-policy-name" instead of an empty name-column
            foreach (MergeControl.NodeInfo nodeInfo in nodeInfoList)
            {
                var query = from pol in countryConfig.Policy where pol.ID == nodeInfo.ID select pol;
                if (query.Count() == 0)
                    return;
                CountryConfig.PolicyRow policy = query.First();
                if (!policy.IsReferencePolIDNull() && policy.ReferencePolID != string.Empty)
                {
                    string refPolicyName = (from pol in countryConfig.Policy where pol.ID == policy.ReferencePolID select pol.Name).First();
                    foreach (MergeControl.CellInfo cellInfo in nodeInfo.cellInfo)
                        if (cellInfo.columnID == countryConfig.Policy.NameColumn.ColumnName)
                            cellInfo.SetText("=> " + refPolicyName);
                }
            }
        }

        internal class ComponentValueTable
        {
            internal class Item
            {
                internal string id = string.Empty;
                internal string idInMatchSys = string.Empty;
                internal string systemID = string.Empty;
                internal string order = string.Empty;
                internal string originalOrderLocal = string.Empty;
                internal string originalOrderRemote = string.Empty; 
                internal string valueLocal = string.Empty;
                internal string valueRemote = string.Empty;
                internal string valueParent = PARENT_UNAVAILABLE;

                internal Item(string id, string idInMatchSys, string systemID, string order, string originalOrderLocal, string originalOrderRemote,
                              string valueLocal = "", string valueRemote = "", string valueParent = PARENT_UNAVAILABLE)
                {
                    this.id = id; this.idInMatchSys = idInMatchSys; this.systemID = systemID; this.order = order;
                    this.originalOrderLocal = originalOrderLocal; this.originalOrderRemote = originalOrderRemote;
                    this.valueLocal = valueLocal; this.valueRemote = valueRemote; this.valueParent = valueParent;
                }
            };

            List<Item> items = new List<Item>();
            ILookup<string, Item> itemsGroupedById = null;
            //ILookup<string, Item> itemsGroupedByOrder = null;
            ILookup<string, Item> itemsGroupedByIdInMatchSys = null;
            SortedDictionary<string, Item> itemsGroupedByOrderDict = null;

            //possibilitie of adding or inserting items:
            //(1) item from local match-system: generate new row, fill: id, idInMatchSys, systemID, order (e.g.1.3.5), valueLocal
            //(2) item from other local system: generate new row, search for row with same order, overtake: idInMatchSys, fill: id, systemID, order, valueLocal
            //(3) item from remote match-system: search for row with same id, if not found: (1), else fill: valueRemote in found row
            //(4) item from other remote system: search for row with same id, if not found: (2), else fill: valueRemote in found row
            //(5) item from parent match-system: search for row with same id, if not found: skip, else fill: valueParent in found row
            //(6) item from other parent system: search for row with same id, if not found: skip, else fill: valueParent in found row

            internal void Add(Item item, string siblingOrder = "") // (1: siblingOrder = ""), (2: siblingOrder != "")
            {                                                      // note: 'twin' stands for same item in other country (e.g. c1: polX in sysA, c2: polX in sysA)
                if (siblingOrder != string.Empty)                  //       'sibling' stands for same item in other system but same country (e.g. c1: polX in sysA, c1: polX in sysB)
                {
                    Item siblingItem = GetSiblingItemDict(siblingOrder);

                    item.idInMatchSys = siblingItem == null ? "" : siblingItem.idInMatchSys;
                }
                items.Add(item);

                //The element is added to a sorted dictionary
                if (itemsGroupedByOrderDict!= null && !itemsGroupedByOrderDict.ContainsKey(item.order))
                {
                    itemsGroupedByOrderDict.Add(item.order, item);
                }
            }

            internal void Insert(Item item, string twinId,  // (3,5: siblingOrder = ""; 3: item.valueRemote != "", 5: item.valueParent != "")
                                 string siblingOrder = "")  // (4,6: siblingOrder != "", 4: item.valueRemote != "", 6: item.valueParent != "")
            {                                               // note: siblingOrder is only applied (for 5) if item is not found by twinId

                Item twinItem = GetTwinItem(twinId);

                if (twinItem == null)
                {
                    if (item.valueRemote != string.Empty)
                    {
                        Add(item, siblingOrder);
                        //itemsGroupedByOrder = null; //the "dictionary" must be renewed, as an item is added
                          
                    }
                }
                else {                
                    if (item.valueRemote != string.Empty) { twinItem.valueRemote = item.valueRemote; twinItem.originalOrderRemote = item.originalOrderRemote; }
                    else twinItem.valueParent = item.valueParent;
                }
            }

            internal List<Item> Get(string idInMatchSys)
            {
                if (itemsGroupedByIdInMatchSys == null)
                    itemsGroupedByIdInMatchSys = items.ToLookup(item => item.idInMatchSys);
                return itemsGroupedByIdInMatchSys[idInMatchSys].ToList<Item>();
            }
            
            /*Item GetSiblingItem(string order)
            {
                if (itemsGroupedByOrder == null)
                {
                    itemsGroupedByOrder = items.ToLookup(item => item.order);
                }

                return itemsGroupedByOrder[order].Count() == 0 ? null : itemsGroupedByOrder[order].First();
            }*/

              
            //We are going to use a sorted dictionary instead of sorting the list every time.
            Item GetSiblingItemDict(string order)
            {
                Item sibling = null;
                if (itemsGroupedByOrderDict == null)
                {
                    Dictionary<string, Item> itemsGroupedByOrderDictNoSorted = items.ToDictionary(x => x.order, x => x);
                    itemsGroupedByOrderDict =  new SortedDictionary<string, Item>(itemsGroupedByOrderDictNoSorted);
                }

                if (itemsGroupedByOrderDict.ContainsKey(order))
                {
                    sibling = itemsGroupedByOrderDict[order];

                }

                return sibling;
            }

            Item GetTwinItem(string id)
            {
                if (itemsGroupedById == null)
                    itemsGroupedById = items.ToLookup(item => item.id);
                return itemsGroupedById[id].Count() == 0 ? null : itemsGroupedById[id].First();
            }
        }

        void GetInfoDatasetsFromXml(out List<MergeControl.ColumnInfo> columInfo,
                               out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>(); //this list is a combination of the ones below, with the order in which columns should be displayed
            List<DataColumn> dataSettingColumns = new List<DataColumn>(); //dataset settings
            List<DataColumn> dbcSettingColumns = new List<DataColumn>();  //database-system-combination settings
            List<DataColumn> polSettingColumns = new List<DataColumn>();  //policy-switch settings
            settingColumns.Add(_dcLocal.DataBase.NameColumn); dataSettingColumns.Add(_dcLocal.DataBase.NameColumn);
            settingColumns.Add(_dcLocal.DBSystemConfig.SystemNameColumn); dbcSettingColumns.Add(_dcLocal.DBSystemConfig.SystemNameColumn);
            settingColumns.Add(_dcLocal.DBSystemConfig.BestMatchColumn); dbcSettingColumns.Add(_dcLocal.DBSystemConfig.BestMatchColumn);
            settingColumns.Add(_dcLocal.DBSystemConfig.UseCommonDefaultColumn); dataSettingColumns.Add(_dcLocal.DBSystemConfig.UseCommonDefaultColumn);
            settingColumns.Add(_dcLocal.DataBase.YearCollectionColumn); dataSettingColumns.Add(_dcLocal.DataBase.YearCollectionColumn);
            settingColumns.Add(_dcLocal.DataBase.YearIncColumn); dataSettingColumns.Add(_dcLocal.DataBase.YearIncColumn);
            settingColumns.Add(_dcLocal.DataBase.CurrencyColumn); dataSettingColumns.Add(_dcLocal.DataBase.CurrencyColumn);
            settingColumns.Add(_dcLocal.DataBase.FilePathColumn); dataSettingColumns.Add(_dcLocal.DataBase.FilePathColumn);
            settingColumns.Add(_dcLocal.DataBase.DecimalSignColumn); dataSettingColumns.Add(_dcLocal.DataBase.DecimalSignColumn);
            settingColumns.Add(_dcLocal.DataBase.PrivateColumn); dataSettingColumns.Add(_dcLocal.DataBase.PrivateColumn);
            settingColumns.Add(_dcLocal.DataBase.ReadXVariablesColumn); dataSettingColumns.Add(_dcLocal.DataBase.ReadXVariablesColumn);
            settingColumns.Add(_dcLocal.DataBase.ListStringOutVarColumn); dataSettingColumns.Add(_dcLocal.DataBase.ListStringOutVarColumn);
            settingColumns.Add(_dcLocal.DataBase.IndirectTaxTableYearColumn); dataSettingColumns.Add(_dcLocal.DataBase.IndirectTaxTableYearColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //top-level: add a node for each dataset
            List<string> localAndRemoteIDs = BuildNodeInfo_List(_dcLocal.DataBase, _dcRemote.DataBase, _dcParent.DataBase,
                                                                _dcLocal.DataBase.IDColumn.ColumnName, _dcLocal.DataBase.NameColumn.Caption, string.Empty,
                                                                nodeInfoLocal, nodeInfoRemote, dataSettingColumns);

            //1st sub-level: add nodes for each database-system-combination
            foreach (string dataID in localAndRemoteIDs)
            {
                string whereClause = "DataBaseID = '" + dataID + "'";
                localAndRemoteIDs = BuildNodeInfo_List(GetSubTable(_dcLocal.DBSystemConfig, whereClause),
                                    GetSubTable(_dcRemote.DBSystemConfig, whereClause),
                                    GetSubTable(_dcParent.DBSystemConfig, whereClause),
                                    _dcLocal.DBSystemConfig.SystemIDColumn.ColumnName, _dcLocal.DBSystemConfig.SystemNameColumn.Caption, dataID,
                                    nodeInfoLocal, nodeInfoRemote, dbcSettingColumns,
                                    true); //this true effects that the nodes' IDs are built as sysID#dataID, to be uniquely identifiable 
            }
        }

        void GetInfoCondFormatFromXml(out List<MergeControl.ColumnInfo> columInfo,
                          out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            //BUILD TREE-COLUMNS
            //define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_ccLocal.ConditionalFormat.ConditionColumn);
            settingColumns.Add(_ccLocal.ConditionalFormat.BaseSystemNameColumn);
            settingColumns.Add(_ccLocal.ConditionalFormat.BackColorColumn);
            settingColumns.Add(_ccLocal.ConditionalFormat.ForeColorColumn);

            //add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            //BUILD TREE-NODES
            //add a node for each conditional-formatting
            List<string> localAndRemoteIDs = BuildNodeInfo_List(AdaptCondFormatTable(_ccLocal.ConditionalFormat),
                          AdaptCondFormatTable(_ccRemote.ConditionalFormat), AdaptCondFormatTable(_ccParent.ConditionalFormat),
                          _ccLocal.ConditionalFormat.IDColumn.ColumnName, _ccLocal.ConditionalFormat.ConditionColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns);
        }

        DataTable AdaptCondFormatTable(CountryConfig.ConditionalFormatDataTable originalCondFormatTable)
        {//rather inelegant and adhoc way to display create a reasonably graspable view of conditional-formatting changes
            CountryConfig.ConditionalFormatDataTable condFormatTable = originalCondFormatTable.Copy() as CountryConfig.ConditionalFormatDataTable; //important to copy the table, otherwise changes would affect the orginal talbe (though no return parameter(?))
            foreach (CountryConfig.ConditionalFormatRow condFormat in condFormatTable.Rows)
            {
                bool isValueFormatting = condFormat.IsBaseSystemNameNull() || condFormat.Field<string>("BaseSystemName") == string.Empty;

                //gather the systems on which the CF is to be applied (i.e. THE system in base-changes-formatting and usually all systems in value-formatting)
                string applySystems = string.Empty;
                CountryConfig.ConditionalFormatRow originalCondFormat = null; //have to use the orginial CF, as copying the table (see Copy() above) does not copy the sub-tables
                foreach (CountryConfig.ConditionalFormatRow cfr in originalCondFormatTable.Rows)
                    if (cfr.ID == condFormat.ID) { originalCondFormat = cfr; break; }
                if (originalCondFormat != null)
                    foreach (CountryConfig.ConditionalFormat_SystemsRow applySystem in originalCondFormat.GetConditionalFormat_SystemsRows())
                        applySystems += (applySystems == string.Empty) ? applySystem.SystemName : "#" + applySystem.SystemName;
                
                if (isValueFormatting)
                    condFormat.SetField<string>("BaseSystemName", applySystems); //(mis)use base-system-column to show the systems upon which the value-formatting is applied (not too wring column-header)
                else //base-changes-formatting
                    condFormat.SetField<string>("Condition", applySystems); //(mis)use condition-column to show THE system (wrong column-header but intuitively ok, because it is the first column)
            }
            return condFormatTable;
        }

        internal const string MERGECOL_EXT_ID = "ID";
        internal const string MERGECOL_EXT_NAME = "Name";
        internal const string MERGECOL_EXT_SHORT_BASEOFF = "Short_BaseOff";
        internal const string MERGECOL_EXT_LOOK = "Look";
        internal const string MERGEINFO_POLID = "POL|";
        internal const string MERGEINFO_FUNID = "FUN|";
        internal const string MERGEINFO_PARID = "PAR|";
        internal const string MERGEINFO_EXTID = "EXT|";
        void GetInfoExtensionsFromXml(out List<MergeControl.ColumnInfo> columInfo,
                                      out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            // put all extensions (local and global) into a "hand-made" DataTable
            DataTable localExt = new DataTable(), remoteExt = new DataTable(), parentExt = new DataTable();
            foreach (string colName in new List<string>() { MERGECOL_EXT_ID, MERGECOL_EXT_NAME, MERGECOL_EXT_SHORT_BASEOFF, MERGECOL_EXT_LOOK })
                { localExt.Columns.Add(colName); remoteExt.Columns.Add(colName); parentExt.Columns.Add(colName); }

            // global extensions are equally put in local, remote and parent table
            //Only the global extensions which are used in the current country will appear
            if (_vcFacSwitchablePolicyLocal != null && _vcFacSwitchablePolicyRemote != null && _vcFacSwitchablePolicyParent != null)
            {
                Dictionary<string, bool> localInfo = new Dictionary<string, bool>(), remoteInfo = new Dictionary<string, bool>(), parentInfo = new Dictionary<string, bool>();
                foreach (var dt in new Dictionary<DataTable, SwitchablePolicyConfigFacade>() { { localExt, _vcFacSwitchablePolicyLocal }, { remoteExt, _vcFacSwitchablePolicyRemote }, { parentExt, _vcFacSwitchablePolicyParent } })
                    foreach (var gExt in dt.Value.GetSwitchablePolicyConfig().SwitchablePolicy)
                    {

                        //Only global extensions which have relation with that country will appear
                        Dictionary<string, bool> polInfos = new Dictionary<string, bool>(), funInfos = new Dictionary<string, bool>(), parInfos = new Dictionary<string, bool>();

                        bool existInThisCountry = false;
                        foreach (var dt2 in new List<CountryConfigFacade>() { _ccFacLocal, _ccFacRemote, _ccFacParent })
                        {
                            ExtensionAndGroupMergeHelper.GetExtensionContent(dt2, gExt.ID, out polInfos, out funInfos, out parInfos);
                            if (polInfos.Count > 0 || funInfos.Count > 0 || parInfos.Count > 0)
                            {
                                existInThisCountry = true;
                                break;
                            }
                        }

                        if (existInThisCountry)
                        {
                            DataRow row = dt.Key.NewRow();
                            row[0] = gExt.ID; row[1] = gExt.LongName; row[2] = gExt.NamePattern; row[3] = gExt.Look;
                            dt.Key.Rows.Add(row);
                        }

                    }
            }

            // local extensions are taken from the respective DataConfig
            foreach (var dt in new Dictionary<DataTable, DataConfigFacade>() { { localExt, _dcFacLocal }, { remoteExt, _dcFacRemote }, { parentExt, _dcFacParent } })
                foreach (var cExt in ExtensionAndGroupMergeHelper.GetLocalExtensions(dt.Value))
                {
                    DataRow row = dt.Key.NewRow();
                    row[0] = cExt.ID; row[1] = cExt.Name; row[2] = cExt.ShortName; row[3] = cExt.Look;
                    dt.Key.Rows.Add(row);
                }

            // BUILD TREE-COLUMNS
            List<DataColumn> settingColumns = new List<DataColumn>();
            foreach (DataColumn c in localExt.Columns) if (c.ColumnName != MERGECOL_EXT_ID) settingColumns.Add(c);

            // add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            // BUILD TREE-NODES
            // top-level: extensions: global (equal for local/remote/parent) and local (possibly different)
            List<string> localAndRemoteIDs = BuildNodeInfo_List(localExt, remoteExt, parentExt, MERGECOL_EXT_ID,MERGECOL_EXT_NAME, string.Empty,
                                                                nodeInfoLocal, nodeInfoRemote, settingColumns);
            
            // sub-level: add a node for each content-element of the extension, i.e. policy, function or parameter
            foreach (string extID in localAndRemoteIDs)
            {
                // again, use "hand-made" DataTables to be able to unify columns
                DataTable localContent = new DataTable(), remoteContent = new DataTable(), parentContent = new DataTable();
                foreach (DataColumn col in localExt.Columns) { localContent.Columns.Add(col.ColumnName); remoteContent.Columns.Add(col.ColumnName); parentContent.Columns.Add(col.ColumnName); }
                foreach (var dt in new Dictionary<DataTable, CountryConfigFacade>() { { localContent, _ccFacLocal }, { remoteContent, _ccFacRemote }, { parentContent, _ccFacParent } })
                {
                    Dictionary<string, bool> polInfos = new Dictionary<string, bool>(), funInfos = new Dictionary<string, bool>(), parInfos = new Dictionary<string, bool>();
                    ExtensionAndGroupMergeHelper.GetExtensionContent(dt.Value, extID, out polInfos, out funInfos, out parInfos);
                    List<string> orders = new List<string>(); // to show only once and not for each system
                    foreach (var polInfo in polInfos)
                    {
                        CountryConfig.PolicyRow polRow = dt.Value.GetPolicyRowByID(polInfo.Key); if (orders.Contains(polRow.Order)) continue;
                        DataRow row = dt.Key.NewRow();
                        row[0] =  MERGEINFO_POLID + polRow.ID + MERGEINFO_EXTID + extID; row[1] = polRow.Name; row[2] = polInfo.Value; row[3] = string.Empty;
                        dt.Key.Rows.Add(row); orders.Add(polRow.Order);
                    }
                    foreach (var funInfo in funInfos)
                    {
                        CountryConfig.FunctionRow funRow = dt.Value.GetFunctionRowByID(funInfo.Key); if (orders.Contains(funRow.PolicyRow.Order + "/" + funRow.Order)) continue;
                        DataRow row = dt.Key.NewRow();
                        row[0] = MERGEINFO_FUNID + funRow.ID + MERGEINFO_EXTID + extID; row[1] = funRow.PolicyRow.Name + "/" + funRow.Name; row[2] = funInfo.Value; row[3] = string.Empty;
                        dt.Key.Rows.Add(row); orders.Add(funRow.PolicyRow.Order + "/" + funRow.Order);
                    }
                    foreach (var parInfo in parInfos)
                    {
                        CountryConfig.ParameterRow parRow = dt.Value.GetParameterRowByID(parInfo.Key); if (orders.Contains(parRow.FunctionRow.PolicyRow.Order + "/" + parRow.FunctionRow.Order + "/" + parRow.Order)) continue;
                        DataRow row = dt.Key.NewRow();
                        row[0] =  MERGEINFO_PARID + parRow.ID + MERGEINFO_EXTID + extID; row[1] = parRow.FunctionRow.PolicyRow.Name + "/" + parRow.FunctionRow.Name + "/" + parRow.Name; row[2] = parInfo.Value; row[3] = string.Empty;
                        dt.Key.Rows.Add(row); orders.Add(parRow.FunctionRow.PolicyRow.Order + "/" + parRow.FunctionRow.Order + "/" + parRow.Order);
                    }
                }

                bool jointNodeID = false;
                List<MergeControl.ChangeType> ignoredChangeTypes = null;
                List<String> ignoreColumns = new List<String>();
                ignoreColumns.Add(MERGECOL_EXT_NAME);
                localAndRemoteIDs = BuildNodeInfo_List(localContent, remoteContent, parentContent, MERGECOL_EXT_ID, MERGECOL_EXT_NAME, extID,
                                                       nodeInfoLocal, nodeInfoRemote, settingColumns, jointNodeID, ignoredChangeTypes, ignoreColumns);
            }
        }

        internal const string MERGECOL_GROUP_ID = "ID";
        internal const string MERGECOL_GROUP_NAME = "Name";
        internal const string MERGECOL_GROUP_SHORT_NAME = "ShortName";
        internal const string MERGECOL_GROUP_LOOK = "Look";
        internal const string MERGEINFO_GROUP_POLID = "POL|";
        internal const string MERGEINFO_GROUP_FUNID = "FUN|";
        internal const string MERGEINFO_GROUP_PARID = "PAR|";
        internal const string MERGEINFO_GROUPID = "GRP|";
        void GetInfoGroupsFromXml(out List<MergeControl.ColumnInfo> columInfo,
                              out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            // put all groups into a "hand-made" DataTable
            DataTable localGroup = new DataTable(), remoteGroup = new DataTable(), parentGroup = new DataTable();
            foreach (string colName in new List<string>() { MERGECOL_GROUP_ID, MERGECOL_GROUP_NAME, MERGECOL_GROUP_SHORT_NAME, MERGECOL_GROUP_LOOK })
            { localGroup.Columns.Add(colName); remoteGroup.Columns.Add(colName); parentGroup.Columns.Add(colName); }

            //groups are taken from the country file
            foreach (var dt in new Dictionary<DataTable, CountryConfigFacade>() { { localGroup, _ccFacLocal }, { remoteGroup, _ccFacRemote }, { parentGroup, _ccFacParent } })
                foreach (var cGroup in from lg in dt.Value.GetCountryConfig().LookGroup select lg)
                {
                    DataRow row = dt.Key.NewRow();
                    row[0] = cGroup.ID; row[1] = cGroup.Name; row[2] = cGroup.ShortName; row[3] = cGroup.Look;
                    dt.Key.Rows.Add(row);
                }

            // BUILD TREE-COLUMNS
            List<DataColumn> settingColumns = new List<DataColumn>();
            foreach (DataColumn c in localGroup.Columns) if (c.ColumnName != MERGECOL_GROUP_ID) settingColumns.Add(c);

            // add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            // BUILD TREE-NODES
            // top-level: extensions: global (equal for local/remote/parent) and local (possibly different)
            List<string> localAndRemoteIDs = BuildNodeInfo_List(localGroup, remoteGroup, parentGroup, MERGECOL_GROUP_ID, MERGECOL_GROUP_NAME, string.Empty,
                                                                nodeInfoLocal, nodeInfoRemote, settingColumns);

            // sub-level: add a node for each content-element of the group, i.e. policy, function or parameter
            foreach (string groupID in localAndRemoteIDs)
            {
                // again, use "hand-made" DataTables to be able to unify columns
                DataTable localContent = new DataTable(), remoteContent = new DataTable(), parentContent = new DataTable();
                foreach (DataColumn col in localGroup.Columns) { localContent.Columns.Add(col.ColumnName); remoteContent.Columns.Add(col.ColumnName); parentContent.Columns.Add(col.ColumnName); }
                foreach (var dt in new Dictionary<DataTable, CountryConfigFacade>() { { localContent, _ccFacLocal }, { remoteContent, _ccFacRemote }, { parentContent, _ccFacParent } })
                {
                    List<string> polInfos = new List<string>(), funInfos = new List<string>(), parInfos = new List<string>();
                    ExtensionAndGroupMergeHelper.GetLookGroupContent(dt.Value, groupID, out polInfos, out funInfos, out parInfos);
                   
                    List<string> orders = new List<string>(); // to show only once and not for each system
                    foreach (var polInfoId in polInfos)
                    {
                        CountryConfig.PolicyRow polRow = dt.Value.GetPolicyRowByID(polInfoId); if (orders.Contains(polRow.Order)) continue;
                        DataRow row = dt.Key.NewRow();
                        row[0] = MERGEINFO_POLID + polRow.ID + MERGEINFO_GROUPID + groupID; row[1] = polRow.Name; row[2] = String.Empty;
                        dt.Key.Rows.Add(row); orders.Add(polRow.Order);
                    }
                    foreach (var funInfoId in funInfos)
                    {
                        CountryConfig.FunctionRow funRow = dt.Value.GetFunctionRowByID(funInfoId); if (orders.Contains(funRow.PolicyRow.Order + "/" + funRow.Order)) continue;
                        DataRow row = dt.Key.NewRow();
                        row[0] = MERGEINFO_FUNID + funRow.ID + MERGEINFO_GROUPID + groupID; row[1] = funRow.PolicyRow.Name + "/" + funRow.Name; row[2] = string.Empty;
                        dt.Key.Rows.Add(row); orders.Add(funRow.PolicyRow.Order + "/" + funRow.Order);
                    }
                    foreach (var parInfoId in parInfos)
                    {
                        CountryConfig.ParameterRow parRow = dt.Value.GetParameterRowByID(parInfoId); if (orders.Contains(parRow.FunctionRow.PolicyRow.Order + "/" + parRow.FunctionRow.Order + "/" + parRow.Order)) continue;
                        DataRow row = dt.Key.NewRow();
                        row[0] = MERGEINFO_PARID + parRow.ID + MERGEINFO_GROUPID + groupID; row[1] = parRow.FunctionRow.PolicyRow.Name + "/" + parRow.FunctionRow.Name + "/" + parRow.Name; row[2] = string.Empty; 
                        dt.Key.Rows.Add(row); orders.Add(parRow.FunctionRow.PolicyRow.Order + "/" + parRow.FunctionRow.Order + "/" + parRow.Order);
                    }
                }
                localAndRemoteIDs = BuildNodeInfo_List(localContent, remoteContent, parentContent, MERGECOL_EXT_ID, MERGECOL_EXT_NAME, groupID,
                                                       nodeInfoLocal, nodeInfoRemote, settingColumns);
            }
        }

        const string MERGECOL_EXTSWITCH_ID = "ID";
        const string MERGECOL_EXTSWITCH_EXT = "Extension";
        const string MERGECOL_EXTSWITCH_SYS = "System";
        const string MERGECOL_EXTSWITCH_DATA = "Data";
        const string MERGECOL_EXTSWITCH_ONOFF = "On/Off";
        void GetInfoExtSwitchesFromXml(out List<MergeControl.ColumnInfo> columInfo,
                                       out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            // put all extension-switches into a "hand-made" DataTable
            DataTable localESw = new DataTable(), remoteESw = new DataTable(), parentESw = new DataTable();
            foreach (string colName in new List<string>() { MERGECOL_EXTSWITCH_ID, MERGECOL_EXTSWITCH_EXT, MERGECOL_EXTSWITCH_SYS, MERGECOL_EXTSWITCH_DATA, MERGECOL_EXTSWITCH_ONOFF })
                { localESw.Columns.Add(colName); remoteESw.Columns.Add(colName); parentESw.Columns.Add(colName); }

            // gather all extension-switches for where extension and data-sys-combination actually exists
            foreach (var d in new Dictionary<DataTable, DataConfig>() { { localESw, _dcLocal }, { remoteESw, _dcRemote }, { parentESw, _dcParent } })
            {
                DataTable extSwitchTable = d.Key; DataConfig dataConfig = d.Value; List<string> duplicates = new List<string>();
                foreach (var extSwitch in dataConfig.PolicySwitch)
                {
                    if (extSwitch.Value == DefPar.Value.NA) continue; // n/a is like not existent (should actually be removed)

                    string extName = null;
                    var globExt = ExtensionAndGroupManager.GetGlobalExtension(extSwitch.SwitchablePolicyID);
                    if (globExt != null) extName = globExt.Name;
                    else
                    {
                        var locExt = (from e in dataConfig.Extension where e.ID == extSwitch.SwitchablePolicyID select e).FirstOrDefault();
                        if (locExt != null) extName = locExt.Name;
                    }
                    if (extName == null) continue; // neither local nor global extension - must be rubbish (should actually be removed)

                    var dbSysComb = (from dsc in dataConfig.DBSystemConfig
                                     where dsc.DataBaseID == extSwitch.DataBaseID & dsc.SystemID == extSwitch.SystemID
                                     select dsc).FirstOrDefault();
                    if (dbSysComb == null) continue; // no such data-system-combination exists - must be rubbish (should actually be removed)
                    
                    DataRow row = extSwitchTable.NewRow();
                    string id = extSwitch.SwitchablePolicyID + "|" + extSwitch.SystemID + "|" + extSwitch.DataBaseID;

                    row[0] = id; row[1] = extName; row[2] = dbSysComb.SystemName; row[3] = dbSysComb.DataBaseRow.Name; row[4] = extSwitch.Value;
                    extSwitchTable.Rows.Add(row);

                    if (duplicates.Contains(id)) // this happens by deleting a system and undoing the delete (it's quite a mess ...)
                    {
                        string where = (d.Value == _dcParent) ? "parent" : (d.Value == _dcRemote ? "remote" : "local");
                        UserInfoHandler.ShowInfo("Extension Switches cannot be compared, as they are in an invalid state in the " + where + " version." + Environment.NewLine +
                            "Opening the Set Switches dialog in the respective version and closing with OK solves the problem.");
                        return;
                    }
                    duplicates.Add(id);
                }
            }

            // BUILD TREE-COLUMNS
            List<DataColumn> settingColumns = new List<DataColumn>();
            foreach (DataColumn c in localESw.Columns) if (c.ColumnName != MERGECOL_EXTSWITCH_ID) settingColumns.Add(c);

            // add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                  columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));

            List<String> ignoreColumns = new List<String>();
            //ignoreColumns.Add(MERGECOL_EXTSWITCH_EXT);
            ignoreColumns.Add(MERGECOL_EXTSWITCH_SYS);
            ignoreColumns.Add(MERGECOL_EXTSWITCH_DATA);


            // BUILD TREE-NODES
            BuildNodeInfo_List(localESw, remoteESw, parentESw, MERGECOL_EXTSWITCH_ID, MERGECOL_EXTSWITCH_EXT, string.Empty,
                                                               nodeInfoLocal, nodeInfoRemote, settingColumns, false, null, ignoreColumns);
        }

        void GetInfoUpratingIndicesFromXml(out List<MergeControl.ColumnInfo> columInfo,
                          out List<MergeControl.NodeInfo> nodeInfoLocal, out List<MergeControl.NodeInfo> nodeInfoRemote)
        {
            columInfo = new List<MergeControl.ColumnInfo>();
            nodeInfoLocal = new List<MergeControl.NodeInfo>();
            nodeInfoRemote = new List<MergeControl.NodeInfo>();

            // BUILD TREE-COLUMNS
            // define the settings which are to be compared
            List<DataColumn> settingColumns = new List<DataColumn>();
            settingColumns.Add(_ccLocal.UpratingIndex.DescriptionColumn);
            settingColumns.Add(_ccLocal.UpratingIndex.ReferenceColumn);
            settingColumns.Add(_ccLocal.UpratingIndex.YearValuesColumn);
            settingColumns.Add(_ccLocal.UpratingIndex.CommentColumn);

            // add setting-columns to tree
            foreach (DataColumn settingColumn in settingColumns)
                columInfo.Add(new MergeControl.ColumnInfo(settingColumn.ColumnName));
                

            // BUILD TREE-NODES
            // add a node for each uprating index (with simingly large part to ignore 
            CountryConfig.UpratingIndexRow locHICP = _ccFacLocal.GetUpratingIndex("$HICP");  // $HICP should actually be ignored because it is administrated in global table)
            CountryConfig.UpratingIndexRow remHICP = _ccFacRemote.GetUpratingIndex("$HICP"); // but appears to be differnt, because in global table has do description
            if (locHICP != null && remHICP != null) remHICP.Description = locHICP.Description;
            List<string> localAndRemoteIDs = BuildNodeInfo_List(_ccLocal.UpratingIndex, _ccRemote.UpratingIndex, _ccParent.UpratingIndex,
                          _ccLocal.UpratingIndex.IDColumn.ColumnName, _ccLocal.UpratingIndex.ReferenceColumn.Caption, string.Empty,
                          nodeInfoLocal, nodeInfoRemote, settingColumns);

            //To display uprating indices nodes properly
            UpdateNodesUpratingIndices(nodeInfoLocal, nodeInfoRemote, columInfo, _ccParent.UpratingIndex);
 

        }

        DataTable GetSubTable(DataTable table, string whereClause = "", Dictionary<string, string> columnAliases = null, string orderClause = "")
        {
            DataTable subTable = null;
            if (whereClause != string.Empty && orderClause == string.Empty)
            {
                DataRow[] selection = table.Select(whereClause);
                subTable = selection.Count() == 0 ? table.Clone() : selection.CopyToDataTable();
            }
            else if(whereClause != string.Empty && orderClause != string.Empty)
            {
                DataRow[] selection = table.Select(whereClause, orderClause);
                subTable = selection.Count() == 0 ? table.Clone() : selection.CopyToDataTable();

                //The table is sorted. If, for any reasons, it cannot be sorted
                //It will continue without sorting the table
                try
                {
                    string orderColumn = orderClause.ToLower().Replace(" asc", "").Replace(" desc", "");
                    string orderDirection = orderClause.ToLower().Replace(orderColumn + " ", "");
                    subTable.Columns.Add("orderInt", typeof(int), orderColumn);
                    subTable.DefaultView.Sort = "orderInt " + orderDirection;
                    subTable = subTable.DefaultView.ToTable();
                }catch(Exception e) { 
                    string msg = e.Message; 
                }
                
            }
            else
                subTable = table.Copy(); //use the whole table, but make a copy to not destroy the original one (function is just called for renaming columns)

            if (columnAliases != null) //rename e.g. table AcronyType's 'ShortName' to 'Name' to view it in the same column as Acronym's 'Name'
                foreach (string column in columnAliases.Keys)
                    subTable.Columns[column].ColumnName = columnAliases[column];

            
            return subTable;
        }

        List<string> BuildNodeInfo_List(DataTable localTable, DataTable remoteTable, DataTable parentTable,
            string idColumnName, string labelColumnCaption, string parentID,
            List<MergeControl.NodeInfo> nodeInfoLocal, List<MergeControl.NodeInfo> nodeInfoRemote, List<DataColumn> settingColumns,
            bool jointNodeID = false, List<MergeControl.ChangeType> ignoredChangeTypes = null, List<String> ignoreColumns = null)
        {
            //gather component-IDs of both countries (to take into account that some may only exist in one country)
            List<string> tempLocalAndRemoteIDs = new List<string>();
            foreach (DataRow dataRow in localTable.Rows)
                tempLocalAndRemoteIDs.Add(dataRow.Field<string>(idColumnName));
            foreach (DataRow dataRow in remoteTable.Rows)
                if (!tempLocalAndRemoteIDs.Contains(dataRow.Field<string>(idColumnName)))
                    tempLocalAndRemoteIDs.Add(dataRow.Field<string>(idColumnName));

            //foreach component: add a node to each of the two tree-lists, with respective info (added, removed, changed, ...)
            List<string> localAndRemoteIDs = new List<string>();
            foreach (string id in tempLocalAndRemoteIDs)
            {
                //the existence of the component in local/remote/parent country reflects whether it is added (loc/rem), changed (loc/rem), ...
                string whereClause = idColumnName + " = '" + id + "'";
                DataRow[] localRows = localTable.Select(whereClause); DataRow localRow = localRows.Count() >= 1 ? localRows.First() : null;
                DataRow[] remoteRows = remoteTable.Select(whereClause); DataRow remoteRow = remoteRows.Count() >= 1 ? remoteRows.First() : null;
                DataRow[] parentRows = parentTable.Select(whereClause); DataRow parentRow = parentRows.Count() >= 1 ? parentRows.First() : null;

                //fill the cell-values of the node
                MergeControl.NodeInfo mcNodeLocal; MergeControl.NodeInfo mcNodeRemote;
                BuildNodeInfo_Single(BuildNodeInfoID(id, parentID, jointNodeID), //usually just the id is used (joint=false), but provide the possibility to use id#parentID as unique identifier (e.g. for system-database-comb.-table)
                    parentID, settingColumns, labelColumnCaption, localRow, remoteRow, parentRow, out mcNodeLocal, out mcNodeRemote, ignoreColumns);

                if (ignoredChangeTypes != null &&
                    ignoredChangeTypes.Contains(mcNodeLocal.changeType) && ignoredChangeTypes.Contains(mcNodeRemote.changeType))
                        continue;

                nodeInfoLocal.Add(mcNodeLocal);
                nodeInfoRemote.Add(mcNodeRemote);
                localAndRemoteIDs.Add(id);
            }

            return localAndRemoteIDs;
        }

        List<string> BuildNodeInfo_List_HICPConfig(DataTable localTable, DataTable remoteTable, DataTable parentTable,
            string idColumnFirstID, string idColumnSecondID, string labelColumnCaption, string parentID,
            List<MergeControl.NodeInfo> nodeInfoLocal, List<MergeControl.NodeInfo> nodeInfoRemote, List<DataColumn> settingColumns,
            bool jointNodeID = false, List<MergeControl.ChangeType> ignoredChangeTypes = null)
        {
            //gather component-IDs of both countries (to take into account that some may only exist in one country)
            List<string> tempLocalAndRemoteIDs = new List<string>();
            foreach (DataRow dataRow in localTable.Rows)
                tempLocalAndRemoteIDs.Add(dataRow[idColumnFirstID].ToString() + "_"+ dataRow[idColumnSecondID].ToString());

            foreach (DataRow dataRow in remoteTable.Rows)
                if (!tempLocalAndRemoteIDs.Contains(dataRow[idColumnFirstID].ToString() + "_" + dataRow[idColumnSecondID].ToString()))
                    tempLocalAndRemoteIDs.Add(dataRow[idColumnFirstID].ToString() + "_" + dataRow[idColumnSecondID].ToString());

            //foreach component: add a node to each of the two tree-lists, with respective info (added, removed, changed, ...)
            List<string> localAndRemoteIDs = new List<string>();
            foreach (string id in tempLocalAndRemoteIDs)
            {
                string firstId = id.Substring(0, id.IndexOf("_"));
                string secondId = id.Substring(id.IndexOf("_") + 1);

                //the existence of the component in local/remote/parent country reflects whether it is added (loc/rem), changed (loc/rem), ...
                string whereClause = idColumnFirstID + " = '" + firstId + "' AND "+ idColumnSecondID + " = '"+ secondId + "'";
                DataRow[] localRows = localTable.Select(whereClause); DataRow localRow = localRows.Count() >= 1 ? localRows.First() : null;
                DataRow[] remoteRows = remoteTable.Select(whereClause); DataRow remoteRow = remoteRows.Count() >= 1 ? remoteRows.First() : null;
                DataRow[] parentRows = parentTable.Select(whereClause); DataRow parentRow = parentRows.Count() >= 1 ? parentRows.First() : null;

                //fill the cell-values of the node
                MergeControl.NodeInfo mcNodeLocal; MergeControl.NodeInfo mcNodeRemote;
                BuildNodeInfo_Single(BuildNodeInfoID(id, parentID, jointNodeID), //usually just the id is used (joint=false), but provide the possibility to use id#parentID as unique identifier (e.g. for system-database-comb.-table)
                    parentID, settingColumns, labelColumnCaption, localRow, remoteRow, parentRow, out mcNodeLocal, out mcNodeRemote);

                if (ignoredChangeTypes != null &&
                    ignoredChangeTypes.Contains(mcNodeLocal.changeType) && ignoredChangeTypes.Contains(mcNodeRemote.changeType))
                    continue;

                nodeInfoLocal.Add(mcNodeLocal);
                nodeInfoRemote.Add(mcNodeRemote);
                localAndRemoteIDs.Add(id);
            }

            return localAndRemoteIDs;
        }

        string ComposeOrder(int dataIndex, CountryConfig.PolicyRow policy, CountryConfig.FunctionRow function = null, CountryConfig.ParameterRow parameter = null)
        {
            string compOrder = dataIndex.ToString() + "." + policy.Order;
            if (function != null) compOrder += "." + function.Order;
            if (parameter != null) compOrder += "." + parameter.Order;
            return compOrder;
        }

        string BuildNodeInfoID(string ID, string parentID = "", bool joint = false) { return joint ? ID + "#" + parentID : ID; }
        static internal List<string> DisassembleNodeInfoID(string nodeID) { return nodeID.Split(new string[]{"#"}, StringSplitOptions.None).ToList(); }

        //function generates the info to add a row (node) to each of the two trees for a component (e.g. for a policy), by providing info about 
        //- the single 'settings' of each column (e.g. private=no, comment=blablabla) and info whether this specific setting was changed
        //- the components state, i.e. was it added (loc/rem), changed (loc/rem), ...
        void BuildNodeInfo_Single(string nodeID, string nodeParentID,
                                  List<DataColumn> settingColumns, string labelColumnCaption,
                                  DataRow localComp, DataRow remoteComp, DataRow parentComp,
                                  out MergeControl.NodeInfo mcNodeLocal, out MergeControl.NodeInfo mcNodeRemote, List<string> ignoreColumns = null)
        {
            mcNodeLocal = new MergeControl.NodeInfo(nodeID, nodeParentID);
            mcNodeRemote = new MergeControl.NodeInfo(nodeID, nodeParentID);

            //find out whether the component (e.g. policy) was added/removed local/remote
            if (localComp == null) //component does not exist in local (but does exist in remote, as it needs to exist in one of them to be considered anyway)
            {
                if (parentComp != null) //exists in remote and parent, but not in local -> was deleted in local
                    { mcNodeLocal.changeType = MergeControl.ChangeType.removed; mcNodeLocal.changeHandling = MergeControl.ChangeHandling.accept; }
                else //exists in remote only (not in local nor in parent) -> was added in remote
                    { mcNodeRemote.changeType = MergeControl.ChangeType.added; mcNodeRemote.changeHandling = MergeControl.ChangeHandling.accept; }
            }
            if (remoteComp == null) //component does not exist in remote (but does exist in local, as ... see above)
            {
                if (parentComp != null) //exists in local and parent, but not in remote -> was deleted in remote
                    { mcNodeRemote.changeType = MergeControl.ChangeType.removed; mcNodeRemote.changeHandling = MergeControl.ChangeHandling.accept; }
                else //exists in local only (not in remote nor in parent) -> was added in local
                    { mcNodeLocal.changeType = MergeControl.ChangeType.added; mcNodeLocal.changeHandling = MergeControl.ChangeHandling.accept; }
            }

            //assess the settings to be filled into the columns (e.g. private = 'no', comment = 'bla bla bla')
            foreach (DataColumn settingColumn in settingColumns)
            {
                MergeControl.CellInfo localCellInfo = new MergeControl.CellInfo(settingColumn.ColumnName);
                MergeControl.CellInfo remoteCellInfo = new MergeControl.CellInfo(settingColumn.ColumnName);

                //assess the setting-value (e.g. 'yes') - if the component (e.g. policy) does not exist for local or remote, just do not provide a value (just leave empty)
            
                if (localComp != null) localCellInfo.SetText(localComp[settingColumn.ColumnName].ToString());
                if (remoteComp != null) remoteCellInfo.SetText(remoteComp[settingColumn.ColumnName].ToString());
                string parentValue = parentComp != null ? parentComp[settingColumn.ColumnName].ToString() : PARENT_UNAVAILABLE;

                //if exists in both, local and remote, compare values
                if (localComp != null && remoteComp != null)
                {
                    bool ignoreColumnsSingle = false;
                    if (ignoreColumns!= null && ignoreColumns.Contains(settingColumn.ColumnName))
                    {
                        ignoreColumnsSingle = true;
                    }
                        
                    CompareLocalRemote(mcNodeLocal, mcNodeRemote, localCellInfo, remoteCellInfo, parentValue, ignoreColumnsSingle);
                }
                    

                //this is for users convenience: removed components would be shown as emtpy nodes (i.e. no column filled, not even the name-column), just with the symbol for removed
                //though the info of what was removed is shown in the other tree, that looks odd, therefore fill at least the "label"-column (usually name-column)
                if (mcNodeLocal.changeType == MergeControl.ChangeType.removed && settingColumn.Caption == labelColumnCaption)
                    localCellInfo.SetText(remoteCellInfo.text);
                if (mcNodeRemote.changeType == MergeControl.ChangeType.removed && settingColumn.Caption == labelColumnCaption)
                    remoteCellInfo.SetText(localCellInfo.text);

                mcNodeLocal.cellInfo.Add(localCellInfo);
                mcNodeRemote.cellInfo.Add(remoteCellInfo);
            }
        }

        void CompareLocalRemote_FillLocal(MergeControl.NodeInfo mcNodeLocal, MergeControl.CellInfo localCellInfo, string remoteValue, string parentValue)
        {
            //generate fake remote node- and cell-info to be able to use CompareLocalRemote
            MergeControl.NodeInfo mcNodeRemote = new MergeControl.NodeInfo(string.Empty);
            MergeControl.CellInfo remoteCellInfo = new MergeControl.CellInfo(string.Empty);
            remoteCellInfo.SetText(remoteValue); //only this is relevant for the comparison
            CompareLocalRemote(mcNodeLocal, mcNodeRemote, localCellInfo, remoteCellInfo, parentValue);
        }
        void CompareLocalRemote_FillRemote(MergeControl.NodeInfo mcNodeRemote, MergeControl.CellInfo remoteCellInfo, string localValue, string parentValue)
        {
            //generate fake local node- and cell-info to be able to use CompareLocalRemote
            MergeControl.NodeInfo mcNodeLocal = new MergeControl.NodeInfo(string.Empty);
            MergeControl.CellInfo localCellInfo = new MergeControl.CellInfo(string.Empty);
            localCellInfo.SetText(localValue); //only this is relevant for the comparison
            CompareLocalRemote(mcNodeLocal, mcNodeRemote, localCellInfo, remoteCellInfo, parentValue);
        }
        void CompareLocalRemote(MergeControl.NodeInfo mcNodeLocal, MergeControl.NodeInfo mcNodeRemote, MergeControl.CellInfo localCellInfo, MergeControl.CellInfo remoteCellInfo, string parentText, bool ignoreColumns = false)
        {
            localCellInfo.SetText((localCellInfo.text == null || localCellInfo.text == "\"\"") ? string.Empty : localCellInfo.text); //for whatever reason comments are sometimes set to "\"\"", if they should be in fact empty
            remoteCellInfo.SetText((remoteCellInfo.text == null || remoteCellInfo.text == "\"\"") ? string.Empty : remoteCellInfo.text);
            parentText = (parentText == null || parentText == "\"\"") ? string.Empty : parentText;

            if ((localCellInfo.text != remoteCellInfo.text) && !ignoreColumns)
            {
                if (parentText != PARENT_UNAVAILABLE)
                {
                    if (remoteCellInfo.text != parentText) //remote change
                    {
                        remoteCellInfo.isChanged = remoteCellInfo.acceptChange = true; //initally 'accept' all changes
                        mcNodeRemote.changeType = MergeControl.ChangeType.changed; //also set the component to changed (as at least one setting is changed)
                        mcNodeRemote.changeHandling = MergeControl.ChangeHandling.accept;
                    }
                    if (localCellInfo.text != parentText) //local change
                    {
                        localCellInfo.isChanged = localCellInfo.acceptChange = true;
                        mcNodeLocal.changeType = MergeControl.ChangeType.changed; //also set ... see above
                        mcNodeLocal.changeHandling = MergeControl.ChangeHandling.accept;
                    }
                    if (localCellInfo.isChanged && remoteCellInfo.isChanged) //remote and local change = conflict
                    {
                        localCellInfo.isConflicted = remoteCellInfo.isConflicted = true;
                        remoteCellInfo.acceptChange = false; //initially only accept the local change (and subsequently prevent user from accepting both)
                    }
                }
                else //the change is assumed local if parent does not exist (though it should, as component is available in both derived countries)
                {
                    localCellInfo.isChanged = localCellInfo.acceptChange = true;
                    mcNodeLocal.changeType = MergeControl.ChangeType.changed;
                    mcNodeLocal.changeHandling = MergeControl.ChangeHandling.accept;
                }
            }

            //get node change-handling based on cell change-handling
            mcNodeLocal.changeHandling = MergeControl.GetNodeChangeHandling(mcNodeLocal, MergeControl.ChangeHandling.accept);
            mcNodeRemote.changeHandling = MergeControl.GetNodeChangeHandling(mcNodeRemote, MergeControl.ChangeHandling.accept);
        }

        internal bool ApplyAcceptedChanges()
        {
            if (!AreLocalFilesUpToDate() &&
                UserInfoHandler.GetInfo("Please note that the local XML-files used for merging are possibly not up-to-date.", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                    return false;
            bool success = _mergeConfigFile ? ApplyAcceptChanges_Variables() : ApplyAcceptChanges_CCAO();

            //request if this solved a VC-conflict and therefore is ready for uploading
            EM_AppContext.Instance.GetVCAdministrator().CheckForUploading(GetNameforVC(),
                _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY));

            return success;
        }

        bool ApplyAcceptChanges_Variables()
        {          
            bool success = true;

            string backUpFolder = RestoreManager.StoreFile(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));
            if (backUpFolder == string.Empty) return false;

            try
            {

                if(_configFileName == EMPath.EM2_FILE_VARS)
                {
                    _variablesMergeForm.Cursor = Cursors.WaitCursor;

                    GetOrReadXmlVariables(); //apply needs the data as stored in .../Merge/ (which is already read, unless a stored merge-info was used)
                                             //that means it works on the files in the Merge-folder, which do not necessarily correspond to the current variables-file, i.e. .../Config/VarConfig.xml

                    (new VariablesApplyAdministrator(this, _variablesMergeForm, _vcFacLocal, _vcFacRemote)).Apply();

                }
                else if(_configFileName == EMPath.EM2_FILE_HICP)
                {
                    _hicpConfigMergeForm.Cursor = Cursors.WaitCursor;

                    GetOrReadXmlVariables(); //apply needs the data as stored in .../Merge/ (which is already read, unless a stored merge-info was used)
                                             //that means it works on the files in the Merge-folder, which do not necessarily correspond to the current variables-file, i.e. .../Config/VarConfig.xml

                    (new HICPConfigApplyAdministrator(this, _hicpConfigMergeForm, _vcFacHICPLocal, _vcFacHICPRemote)).Apply();

                }
                else if(_configFileName == EMPath.EM2_FILE_EXRATES)
                {
                    _exchangeRatesConfigMergeForm.Cursor = Cursors.WaitCursor;

                    GetOrReadXmlVariables(); //apply needs the data as stored in .../Merge/ (which is already read, unless a stored merge-info was used)
                                             //that means it works on the files in the Merge-folder, which do not necessarily correspond to the current variables-file, i.e. .../Config/VarConfig.xml

                    (new ExchangeRatesConfigApplyAdministrator(this, _exchangeRatesConfigMergeForm, _vcFacExchangeRatesLocal, _vcFacExchangeRatesRemote)).Apply();


                }
                else if (_configFileName == EMPath.EM2_FILE_EXTENSIONS)
                {
                    _switchablePolicyConfigMergeForm.Cursor = Cursors.WaitCursor;

                    GetOrReadXmlVariables(); //apply needs the data as stored in .../Merge/ (which is already read, unless a stored merge-info was used)
                                             //that means it works on the files in the Merge-folder, which do not necessarily correspond to the current variables-file, i.e. .../Config/VarConfig.xml

                    (new SwitchablePolicyConfigApplyAdministrator(this, _switchablePolicyConfigMergeForm, _vcFacSwitchablePolicyLocal, _vcFacSwitchablePolicyRemote)).Apply();

                }

                DeleteMergeFolder(false, false);

                //store the result in the localc config-folder (and not in the merge-folder, were it is produced)
                if(_configFileName == EMPath.EM2_FILE_VARS)
                {
                    _vcFacLocal.WriteXML(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));
                    EM_AppContext.Instance.UnloadVarConfigFacade(); //force re-reading on next access
                    RestoreManager.ReportSuccessAndInfo("Merging variables", backUpFolder, false);
                }
                else if(_configFileName == EMPath.EM2_FILE_HICP)
                {
                    _vcFacHICPLocal.WriteXML();
                    EM_AppContext.Instance.UnloadHICPConfigFacade(); //force re-reading on next access
                    RestoreManager.ReportSuccessAndInfo("Merging variables", backUpFolder, false);
                }
                else if(_configFileName == EMPath.EM2_FILE_EXRATES)
                {
                    _vcFacExchangeRatesLocal.WriteXML();
                    EM_AppContext.Instance.UnloadExchangeRatesConfigFacade(); //force re-reading on next access
                    RestoreManager.ReportSuccessAndInfo("Merging variables", backUpFolder, false);
                }
                else if(_configFileName == EMPath.EM2_FILE_EXTENSIONS)
                {
                    _vcFacSwitchablePolicyLocal.WriteXML(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true));
                    EM_AppContext.Instance.UnloadSwitchablePolicyConfigFacade(); //force re-reading on next access
                    RestoreManager.ReportSuccessAndInfo("Merging variables", backUpFolder, false);
                    UserInfoHandler.GetInfo("Please note that the necessary changes for countries using this/these extension(s) are accomplished " +
                    "once the country is next opened (automatically, but with a user request).", MessageBoxButtons.OK);
                }

                success = true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                RestoreManager.RestoreFile(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true), backUpFolder);
                success = false;
            }
            finally
            {
                if (_configFileName.Equals(EMPath.EM2_FILE_VARS))
                {
                    _variablesMergeForm.Cursor = Cursors.Default;
                }
                else if (_configFileName.Equals(EMPath.EM2_FILE_HICP))
                {
                    _hicpConfigMergeForm.Cursor = Cursors.Default;
                }
                else if (_configFileName.Equals(EMPath.EM2_FILE_EXRATES))
                {
                    _exchangeRatesConfigMergeForm.Cursor = Cursors.Default;
                }
                else if(_configFileName.Equals(EMPath.EM2_FILE_EXTENSIONS))
                {
                    _switchablePolicyConfigMergeForm.Cursor = Cursors.Default;
                }
                
            }

            return success;
        }

        bool ApplyAcceptChanges_CCAO()
        {
            //as the action directly operates the xml-files it does not fit in the usual undo-process
            //instead save the xml-files, reset the undo-list and create a backup - to be used for a possible restore by the user and in error-cases
            _mainForm.WriteXml();
            string backUpFolder = RestoreManager.StoreCountry(_mainForm);
            string backUpFolder2 = string.Empty;
            if (File.Exists(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(EMPath.EM2_FILE_EXTENSIONS, true)))
            {
                backUpFolder2 = RestoreManager.StoreFile(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(EMPath.EM2_FILE_EXTENSIONS, true));
                if (backUpFolder2 == string.Empty) return false;
                backUpFolder = backUpFolder + " and " + backUpFolder2;
            }

            if (backUpFolder == string.Empty) return false;

            bool success = true;
            try
            {
                _mergeForm.Cursor = Cursors.WaitCursor;

                GetOrReadXml(); //apply needs the data as stored in .../cc/Merge/ (which is already read, unless a stored merge-info was used)
                                //that means it works on the files in the Merge-folder, including cc_Local, which do not necessarily correspond to the current country-files in .../cc/

                (new ApplyAdministrator(this, _mergeForm, _ccFacLocal, _isAddOn ? null : _dcFacLocal, _ccFacRemote, _isAddOn ? null : _dcFacRemote, _vcFacSwitchablePolicyLocal, _vcFacSwitchablePolicyRemote)).Apply();

                //store the result in the country folder (and not in the merge-folder, were it is produced)
                _ccFacLocal.WriteXml(CountryAdministrator.GetCountryPath(_shortName), CountryAdministrator.GetCountryFileName(_shortName));

                if (!_isAddOn)
                {
                    _dcFacLocal.WriteXml(CountryAdministrator.GetCountryPath(_shortName), CountryAdministrator.GetDataFileName(_shortName));
                    
                    if (_vcFacSwitchablePolicyLocal != null) { 
                        _vcFacSwitchablePolicyLocal.WriteXML(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(EMPath.EM2_FILE_EXTENSIONS, true));
                    }
                }

                _mainForm.ReloadCountry();
                RestoreManager.ReportSuccessAndInfo("Merging", backUpFolder);

                DeleteMergeFolder(false, false); //delete current merging-info, i.e. content of ...\cc\merge\cc_local\, ...\cc\merge\cc_remote\ and ...\cc\merge\cc_parent\

                success = true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                RestoreManager.RestoreCountry(_mainForm, backUpFolder);
                if (File.Exists(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(EMPath.EM2_FILE_EXTENSIONS, true)))
                {
                    RestoreManager.RestoreFile(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(_configFileName, true), backUpFolder);
                }
                
                success = false;
            }
            finally
            {
                _mergeForm.Cursor = Cursors.Default;
            }

            return success;
        }

        string GetNameforVC()
        {
            if (!_mergeConfigFile) return _shortName;
            string name = EMPath.EM2_FILE_VARS;
            if (name.ToLower().EndsWith(".xml")) name = name.Substring(0, name.Length - ".xml".Length);
            return name;
        }

        internal void DeleteMergeFolder(bool deleteOpenConflictInfo, bool deleteAnyConflictInfo)
        {
            try
            {
                if (Directory.Exists(FOLDER_MERGE)) Directory.Delete(FOLDER_MERGE, true);
                
                
                VCAPI.VC_FOLDER_TYPE unitType = _mergeConfigFile ? VCAPI.VC_FOLDER_TYPE.CONFIG : (_isAddOn ? VCAPI.VC_FOLDER_TYPE.ADDON : VCAPI.VC_FOLDER_TYPE.COUNTRY);
                if (deleteAnyConflictInfo || (deleteOpenConflictInfo && EM_UI.VersionControl.VCAdministrator.IsConflictOpen(GetNameforVC(), unitType))) EM_UI.VersionControl.VCAdministrator.DeleteConflictFile(GetNameforVC(), unitType);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Failed to remove folder '" + "'. You may consider manual removal.", false);
            }
        }

        /*****Methods used to display the uprating indices properly *****/
        /// <summary>
        /// Calls all other methods in charge of updating the different components so that the uprating factors are displayed properly in the merge tool
        /// </summary>
        /// <param name="nodeInfoLocal"></param>
        /// <param name="nodeInfoRemote"></param>
        /// <param name="columInfo"></param>
        /// <param name="parentTable"></param>
        void UpdateNodesUpratingIndices(List<MergeControl.NodeInfo> nodeInfoLocal, List<MergeControl.NodeInfo> nodeInfoRemote, List<MergeControl.ColumnInfo> columInfo, DataTable parentTable)
        {
            //First we need to get all the years
            List<string> years = new List<string>();
            years = getUpratingIndicesYears(years, nodeInfoLocal);
            years = getUpratingIndicesYears(years, nodeInfoRemote);
            years.Sort();

            //Then, we get a dictionary with nodes, years and values for local and remote
            //We will also get a list of all nodes
            List<string> nodes = new List<String>();
            Dictionary<string, Dictionary<string, double>> yearValuesDictLocal = GetAllUpratingIndexNodeYearsValuesDictionary(nodeInfoLocal, nodes);
            Dictionary<string, Dictionary<string, double>> yearValuesDictRemote = GetAllUpratingIndexNodeYearsValuesDictionary(nodeInfoRemote, nodes);

            //Then, we also get a dictionary for the parent
            Dictionary<string, Dictionary<string, double>> yearValuesDictParent = GetAllUpratingIndexNodeYearsValuesDictionaryForParent(nodes, parentTable);

            //We iterate through the local and remote and create the new nodes (one per year)
            UpdateNodesInfo(nodeInfoLocal, nodeInfoRemote, yearValuesDictLocal, yearValuesDictRemote, yearValuesDictParent, years);


            //Finally, we update columnInfo
            UpdateColumnInfo(columInfo, years);


        }

        /// <summary>
        /// Updates the columnInfo list so that it contains each year separately
        /// </summary>
        /// <param name="columInfo">Initial list of columInfo</param>
        /// <param name="years">All years that exist in the UpratingIndices YearValues parameter</param>
        void UpdateColumnInfo(List<MergeControl.ColumnInfo> columInfo, List<string> years)
        {
            MergeControl.ColumnInfo yearValuesColumn = null;
            foreach (MergeControl.ColumnInfo singleColumn in columInfo)
            {
                if (singleColumn.ID.Equals(MergeForm.YEAR_VALUES))
                {
                    yearValuesColumn = singleColumn;
                }
            }

            columInfo.Remove(yearValuesColumn);

            foreach (string id in years)
            {
                MergeControl.ColumnInfo columnInfoNew = new MergeControl.ColumnInfo(id, "", MergeControl.CellType.value);
                columInfo.Add(columnInfoNew);
            }
        }


        /// <summary>
        /// Updates the NodeInfo (local and remote) so that they contain each year separately
        /// </summary>
        /// <param name="nodeInfoLocal"></param>
        /// <param name="nodeInfoRemote"></param>
        /// <param name="yearValuesDictLocal"></param>
        /// <param name="yearValuesDictRemote"></param>
        /// <param name="yearValuesDictParent"></param>
        /// <param name="ids"></param>
        void UpdateNodesInfo(List<MergeControl.NodeInfo> nodeInfoLocal, List<MergeControl.NodeInfo> nodeInfoRemote, Dictionary<string, Dictionary<string, double>> yearValuesDictLocal, Dictionary<string, Dictionary<string, double>> yearValuesDictRemote, Dictionary<string, Dictionary<string, double>> yearValuesDictParent, List<string> ids)
        {
            //We need to get the HICP node in order to remove it

            List<MergeControl.NodeInfo> hicpNodeLocal = new List<MergeControl.NodeInfo>();
            List<MergeControl.NodeInfo> hicpNodeRemote = new List<MergeControl.NodeInfo>();
            foreach (MergeControl.NodeInfo node in nodeInfoLocal)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (cell.columnID.ToLower().Equals("reference") && cell.text.ToLower().Equals("$hicp"))
                    {
                        hicpNodeLocal.Add(node);
                        
                        break;
                    }
                }
            }

            

            foreach (MergeControl.NodeInfo node in nodeInfoRemote)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (cell.columnID.ToLower().Equals("reference") && cell.text.ToLower().Equals("$hicp"))
                    {
                        hicpNodeRemote.Add(node);

                        break;
                    }
                }
            }


            //HICP nodes are removed
            //There may be more than one if they have different ids in remote and local

            //We search first the ones in local
            foreach (MergeControl.NodeInfo nodeRemote in hicpNodeRemote)
            {
                string id = "";
                bool foundLocal = false;
                MergeControl.NodeInfo foundNodeLocal = null;
                id = nodeRemote.ID;

                foreach (MergeControl.NodeInfo nodeLocal in hicpNodeLocal)
                {
                    if (nodeLocal.ID.Equals(id))
                    {
                        foundLocal = true;
                        break;
                    }
                }

                if (!foundLocal)
                {
                    foreach (MergeControl.NodeInfo node in nodeInfoLocal)
                    {
                        if (node.ID.Equals(id))
                        {
                            foundLocal = true;
                            foundNodeLocal = node;
                            break;
                        }
                    }
                }

                if (foundLocal && foundNodeLocal != null)
                {
                    nodeInfoLocal.Remove(foundNodeLocal);
                }
            }

            //Now we search in the remote
            foreach (MergeControl.NodeInfo nodeLocal in hicpNodeLocal)
            {
                string id = nodeLocal.ID;
                bool foundRemote = false;
                MergeControl.NodeInfo foundNodeRemote = null;

                foreach (MergeControl.NodeInfo nodeRemote in hicpNodeRemote)
                {
                    if (nodeRemote.ID.Equals(id)){
                        foundRemote = true;
                        break;
                    }
                }

                if (!foundRemote)
                {
                    foreach (MergeControl.NodeInfo node in nodeInfoRemote)
                    {
                        if (node.ID.Equals(id)){
                            foundRemote = true;
                            foundNodeRemote = node;
                            break;
                        }
                    }
                }

                if (foundRemote && foundNodeRemote != null)
                {
                    nodeInfoRemote.Remove(foundNodeRemote);
                }
            }

            //Now, the already found ones are removed
            foreach (MergeControl.NodeInfo nodeRemote in hicpNodeRemote)
            {
                nodeInfoRemote.Remove(nodeRemote);
            }
            foreach (MergeControl.NodeInfo nodeLocal in hicpNodeLocal)
            {
                nodeInfoLocal.Remove(nodeLocal);
            }



            //First, we fill in the nodeInfoLocal
            foreach (MergeControl.NodeInfo node in nodeInfoLocal)
            {
                Dictionary<string, double> yearValuesDictLocalNode = yearValuesDictLocal[node.ID];

                //Now we check if the component didn't exist before
                bool existRemote = true;
                bool existParent = true;
                bool existLocal = true;

                try { if (!(yearValuesDictRemote[node.ID].Values.LongCount() > 0)) existRemote = false; } catch (Exception e) { existRemote = false; string msg = e.Message; }
                try { if (!(yearValuesDictParent[node.ID].Values.LongCount() > 0)) existParent = false; } catch (Exception e) { existParent = false; string msg = e.Message; }
                try { if (!(yearValuesDictLocal[node.ID].Values.LongCount() > 0)) existLocal = false; } catch (Exception e) { existLocal = false; string msg = e.Message; }

                if (!existLocal && existParent) //It does not exist in local
                {
                    node.changeType = MergeControl.ChangeType.removed;
                    node.changeHandling = MergeControl.ChangeHandling.accept;
                }
                else if (!existRemote && !existParent)
                {
                    node.changeType = MergeControl.ChangeType.added;
                    node.changeHandling = MergeControl.ChangeHandling.accept;
                }

                foreach (string entryYear in ids)
                {
                    MergeControl.CellInfo localCellInfo = new MergeControl.CellInfo(entryYear);
                    //local value
                    string localValue = "";
                    try { localValue = yearValuesDictLocalNode[entryYear].ToString(); } catch (Exception e) { string msg = e.Message; }
                    localCellInfo.SetText(localValue);

                    //remote value
                    string remoteValue = "";
                    try { remoteValue = yearValuesDictRemote[node.ID][entryYear].ToString(); } catch (Exception e) { string msg = e.Message; }

                    if(node.changeType != MergeControl.ChangeType.added && node.changeType != MergeControl.ChangeType.removed) {
                        //If the values are the same, nothing else needs to be done
                        if (!localValue.Equals(remoteValue))
                        {
                            string parentValue = "";
                            try { parentValue = yearValuesDictParent[node.ID][entryYear].ToString(); } catch (Exception e) { string msg = e.Message; }

                            Boolean remoteChange = !remoteValue.Equals(parentValue);
                            Boolean localChange = !localValue.Equals(parentValue);

                            if (localChange)
                            {
                                localCellInfo.isChanged = localCellInfo.acceptChange = true;
                                node.changeType = MergeControl.ChangeType.changed;
                                node.changeHandling = MergeControl.ChangeHandling.accept;
                            }

                            if (localChange && remoteChange)
                            {
                                localCellInfo.isConflicted = true;
                            }
                    
                        }
                    }
                    else{
                        localCellInfo.isChanged = false;
                    }

                    node.cellInfo.Add(localCellInfo);
                }

              

            }

            foreach (MergeControl.NodeInfo node in nodeInfoLocal)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        node.cellInfo.Remove(cell);
                        break;
                    }
                }
            }

            //Now, similar changes for the remote
            foreach (MergeControl.NodeInfo node in nodeInfoRemote)
            {
                //Now we check if the component didn't exist before
                bool existLocal = true;
                bool existParent = true;
                bool existRemote = true;
                try { if (!(yearValuesDictRemote[node.ID].Values.LongCount() > 0)) existRemote = false; } catch (Exception e) { existRemote = false; string msg = e.Message; }
                try { if (!(yearValuesDictLocal[node.ID].Values.LongCount() > 0)) existLocal = false; } catch (Exception e) { existLocal = false; string msg = e.Message; }
                try { if (!(yearValuesDictParent[node.ID].Values.LongCount() > 0)) existParent = false; } catch (Exception e) { existParent = false; string msg = e.Message; }


                if (!existLocal && !existParent)
                {
                    node.changeType = MergeControl.ChangeType.added; node.changeHandling = MergeControl.ChangeHandling.accept;
                }
                else if (!existRemote && existParent)
                {
                    node.changeType = MergeControl.ChangeType.removed; node.changeHandling = MergeControl.ChangeHandling.accept;
                }

                
               Dictionary<string, double> yearValuesDictRemoteNode = yearValuesDictRemote[node.ID];
               foreach (string entryYear in ids)
                {
                    MergeControl.CellInfo remoteCellInfo = new MergeControl.CellInfo(entryYear);
                    string remoteValue = "";
                    try { remoteValue = yearValuesDictRemoteNode[entryYear].ToString(); } catch (Exception e) {
                        string msg = e.Message;
                    }
                    remoteCellInfo.SetText(remoteValue);

                    //Local value
                    string localValue = "";
                    try { localValue = yearValuesDictLocal[node.ID][entryYear].ToString(); } catch (Exception e) { string msg = e.Message; }

                    if(node.changeType != MergeControl.ChangeType.removed && node.changeType != MergeControl.ChangeType.added)
                    {
                        if (!localValue.Equals(remoteValue))
                        {
                            string parentValue = "";
                            try { parentValue = yearValuesDictParent[node.ID][entryYear].ToString(); } catch (Exception e) { string msg = e.Message; }

                            Boolean remoteChange = !remoteValue.Equals(parentValue);
                            Boolean localChange = !localValue.Equals(parentValue);

                            if (remoteChange) {
                                remoteCellInfo.isChanged = remoteCellInfo.acceptChange = true;
                                node.changeType = MergeControl.ChangeType.changed;
                                node.changeHandling = MergeControl.ChangeHandling.accept;
                            }
                            if (remoteChange && localChange)
                            {
                                remoteCellInfo.isConflicted = true;
                                remoteCellInfo.acceptChange = false;
                                node.changeHandling = MergeControl.ChangeHandling.reject;
                            }              

                        }
                    }
                    else
                    {
                        remoteCellInfo.isChanged = false;
                       
                    }
                    
                    node.cellInfo.Add(remoteCellInfo);
                }
            }

            foreach (MergeControl.NodeInfo node in nodeInfoRemote)
            {
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        node.cellInfo.Remove(cell);
                        break;
                    }
                }
            }
        }


        /// <summary>
        /// Populates a dictionary which key is the nodeId and which value is another dictionary that contains the years (key) and values for the nodesIfo
        /// </summary>
        /// <param name="nodes">List of nodes that exist in the local and remote</param>
        /// <param name="nodeInfo">List of NodeInfo</param>
        /// <returns>Dictionary which key is the nodeId and which value is another dictionary that contains the years (key) and values for the nodesInfo</returns>
        public static Dictionary<string, Dictionary<string, double>> GetAllUpratingIndexNodeYearsValuesDictionary(List<MergeControl.NodeInfo> nodeInfo, List<string> nodes)
        {
            Dictionary<string, Dictionary<string, double>> nodeDictionary = new Dictionary<string, Dictionary<string, double>>();

            foreach (MergeControl.NodeInfo node in nodeInfo)
            {
                Dictionary<string, double> yearValuesDict = new Dictionary<string, double>();
                string yearValuesString = "";
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        yearValuesString = cell.text;
                    }

                }

                yearValuesDict = GetDictionaryUpratingIndexYearsValues(yearValuesString);
                nodeDictionary.Add(node.ID, yearValuesDict);
                if (!nodes.Contains(node.ID)) { nodes.Add(node.ID); }

            }

            return nodeDictionary;
        }

        /// <summary>
        /// Populates a dictionary which key is the nodeId and which value is another dictionary that contains the years (key) and values for the parent table
        /// </summary>
        /// <param name="nodes">List of nodes that exist in the local and remote</param>
        /// <param name="parentTable">Datatable of parent values</param>
        /// <returns>Dictionary which key is the nodeId and which value is another dictionary that contains the years (key) and values for the parent table</returns>
        public static Dictionary<string, Dictionary<string, double>> GetAllUpratingIndexNodeYearsValuesDictionaryForParent(List<string> nodes, DataTable parentTable)
        {
            Dictionary<string, Dictionary<string, double>> parentDictionary = new Dictionary<string, Dictionary<string, double>>();

            foreach (string node in nodes)
            {
                string whereClause = "ID" + " = '" + node + "'";
                DataRow[] parentRows = parentTable.Select(whereClause); DataRow parentRow = parentRows.Count() >= 1 ? parentRows.First() : null;
                string parentValue = parentRow != null ? parentRow.Field<string>(MergeForm.YEAR_VALUES) : "";
                if (!parentValue.Equals(""))
                {
                    Dictionary<string, double> yearValuesDict = GetDictionaryUpratingIndexYearsValues(parentValue);
                    parentDictionary.Add(node, yearValuesDict);
                }
            }
            return parentDictionary;
        }

        /// <summary>
        /// Creates a list of the years that appear in the uprating indices YearValues string
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="nodeInfo"></param>
        /// <returns>List of the years that appear in the uprating indices YearValues string</returns>
        List<String> getUpratingIndicesYears(List<String> ids, List<MergeControl.NodeInfo> nodeInfo)
        {

            foreach (MergeControl.NodeInfo node in nodeInfo)
            {
                string yearValuesString = "";
                foreach (MergeControl.CellInfo cell in node.cellInfo)
                {
                    if (cell.columnID.Equals(MergeForm.YEAR_VALUES))
                    {
                        yearValuesString = cell.text;
                        Dictionary<String, double> yearValuesDict = GetDictionaryUpratingIndexYearsValues(yearValuesString);

                        foreach (KeyValuePair<string, double> entry in yearValuesDict)
                        {
                            string year = entry.Key.ToString();
                            if (!ids.Contains(year))
                            {
                                ids.Add(year);
                            }
                        }

                    }

                }
            }
            return ids;
        }

        /// <summary>
        /// Creates a dictionary with the year and the values for the uprating indices
        /// </summary>
        /// <param name="yearValuesString">YearValues string with the uprating indices values</param>
        /// <returns>Dictionary with the year and the values for the uprating indices</returns>
        internal static Dictionary<string, double> GetDictionaryUpratingIndexYearsValues(string yearValuesString)
        {
            Dictionary<string, double> yearValues = new Dictionary<string, double>();
            foreach (string yv in yearValuesString.Split(UpratingIndices.UpratingIndicesForm._separator))
            {
                if (string.IsNullOrEmpty(yv)) continue;
                int year; double value;
                bool valEmpty = !EM_Helpers.TryConvertToDouble(yv.Substring(5), out value);
                if (int.TryParse(yv.Substring(0, 4), out year) && (!valEmpty))
                    yearValues.Add(year.ToString(), valEmpty ? -1 : value);
            }
            return yearValues;
        }
    }
}
