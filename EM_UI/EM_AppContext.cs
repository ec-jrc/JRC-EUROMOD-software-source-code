using EM_Common;
using EM_UI.Actions;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Run;
using EM_UI.Tools;
using EM_UI.TreeListManagement;
using EM_UI.VariablesAdministration;
using EM_UI.VersionControl;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI
{
    internal class EM_AppContext : ApplicationContext
    {
        List<EM_UI_MainForm> _countryMainForms = new List<EM_UI_MainForm>();
        static EM_UI_MainForm _emptyForm = null;
        VarConfigFacade _varConfigFacade = null;
        VariablesForm _variablesForm = null;
        HICPConfigFacade _HICPConfigFacade = null;
        ExchangeRatesConfigFacade _exchangeRatesConfigFacade = null;
        SwitchablePolicyConfigFacade _switchablePolicyConfigFacade = null;
        UserSettingsAdministrator _userSettingsAdministrator = null;
        VCAdministrator _vcAdministrator = null;
        PasteFunctionAction _pasteFunctionAction = null;
        PastePolicyAction _pastePolicyAction = null;
        AddParameterForm _addParameterForm = null;
        MatrixViewOfIncomelistsForm _matrixViewOfIncomelistsForm = null;
        FindReplaceForm _findReplaceForm = null;
        List<Form> _topMostForms = new List<Form>();
        List<RunManager> _registeredRunManagers = new List<RunManager>();
        static object _registeredRunManagersLock = new object();

        // allows for storing user-choices during the livetime of the UI and is mainly intended for plug-ins, but also used by PET
        // see EM_UI.PlugInService for application in plug-ins and EM_UI.Dialogs.PolicyEffects.ReStoreUserSettings for appliction in UI
        internal Dictionary<string, TSDictionary> _sessionUserSettings = new Dictionary<string, TSDictionary>();                      

        internal bool _showFunctionSpecifiers = false;

        internal bool _runExeViaLib = true; // if true, executable runs by using the library-function, if false, by starting the compiled executable

        static EM_AppContext singleton = null;

        internal static EM_AppContext Instance { get { return singleton; } }

        internal static EM_AppContext Create_EM_AppContext(EM_UI_MainForm countryMainForm)
        {
            singleton = new EM_AppContext(countryMainForm);
            _emptyForm = countryMainForm;

            //show the dialog where the user can set the default pathes, if the folder to the country-xml-files does not exist
            if (!Directory.Exists(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles)))
            {
                ConfigurePathsForm configurePathsForm = new ConfigurePathsForm();
                configurePathsForm.ShowDialog();
            }
            singleton.SetBrand(); // allow UI to show another look, i.e. present a brand alternative to EUROMOD

            DefinitionAdmin.Init(); // initialise the common-lib's description of functions and parametes (once for the life-time of the lib)
            return singleton;
        }

        internal void SetBrand()
        {
            BrandHandler.PrepareBrand(_emptyForm); // allow UI to show another look, i.e. present a brand alternative to EUROMOD
        }

        internal EM_AppContext(EM_UI_MainForm countryMainForm) : base(countryMainForm) {}

        internal string GetProjectName()
        {
            if (GetVCAdministrator().IsProjectVersionControlled()) return GetVCAdministrator().GetProjectName();
            try { return (new DirectoryInfo(EM_AppContext.FolderEuromodFiles).Name); }
            catch { return string.Empty; }
        }

        internal string ComposeMainFormCaption(string countryLongName, bool isReadOnly)
        {
            string caption = $"{DefGeneral.BRAND_TITLE} {EM_AppContext.Instance.GetProjectName()} ({EM_AppContext.FolderEuromodFiles})";
            string sIsReadOnly = isReadOnly ? " (read-only)" : string.Empty;
            if (countryLongName != string.Empty)
                caption = countryLongName + sIsReadOnly + " - " + caption;
            if (GetVCAdministrator().IsProjectVersionControlled())
                caption += " => connected to VC: " + GetVCAdministrator().GetProjectName();
            return caption;
        }

        internal void UpdateMainFormCaption()
        {
            foreach (EM_UI_MainForm mainForm in GetAllMainForms_PlusEmptyForm())
                mainForm.Text = ComposeMainFormCaption(mainForm.GetCountryLongName(), mainForm._isReadOnly); //is called by ConfigurePath-Form after changing the content, thus actually only one MainForm is open
        }

        internal void UpdateMainFormVCButtons()
        {
            foreach (EM_UI_MainForm mainForm in GetAllMainForms_PlusEmptyForm())
                mainForm.SetVCButtonsGreyState();
        }

        internal bool IsPublicVersion()
        {
            //from the UI's point of view there is no difference between public and private version (private parts are hidden by the public-version-generating-tool)
            //return false here instead of removing the possibility of hiding buttons/menus, just in case developers may change there mind on this issue
            return false;
        }

        internal VarConfigFacade GetVarConfigFacade()
        {
            if (_varConfigFacade == null)
                _varConfigFacade = new VarConfigFacade();
            return _varConfigFacade;
        }

        internal HICPConfigFacade GetHICPConfigFacade(bool create)
        {
            if (!create && !File.Exists(new EMPath(EM_AppContext.FolderEuromodFiles).GetHICPFilePath(true))) return null;
            return _HICPConfigFacade == null ? new HICPConfigFacade() : _HICPConfigFacade;
        }

        internal ExchangeRatesConfigFacade GetExchangeRatesConfigFacade(bool create)
        {
            if (!create && !File.Exists(new EMPath(FolderEuromodFiles).GetExRatesFilePath(true))) return null;
            return _exchangeRatesConfigFacade == null ? new ExchangeRatesConfigFacade() : _exchangeRatesConfigFacade;
        }

        internal SwitchablePolicyConfigFacade GetSwitchablePolicyConfigFacade()
        {
            return _switchablePolicyConfigFacade == null ? new SwitchablePolicyConfigFacade() : _switchablePolicyConfigFacade;
        }

        internal void UnloadVarConfigFacade() { _varConfigFacade = null; } //force re-reading on next access
        
        internal void UnloadHICPConfigFacade() { _HICPConfigFacade = null; }

        internal void UnloadExchangeRatesConfigFacade() { _exchangeRatesConfigFacade = null; }

        internal void UnloadSwitchablePolicyConfigFacade() { _switchablePolicyConfigFacade = null; }

        internal void ShowVariablesForm()
        {
            if (_variablesForm == null || _variablesForm.IsDisposed)
            {
                //capture parameter file, to avoid editing by another user
                bool openedReadOnly = false;
                if (!InUseFileHandler.CaptureFile(new EMPath(FolderEuromodFiles).GetVarFilePath(true),
                                                  ref openedReadOnly)) //set to true, if another user already captured the file
                    return; //file is captured by another user and the user decided to not open in read-only mode

                _variablesForm = new EM_UI.VariablesAdministration.VariablesForm(GetVarConfigFacade());
                _variablesForm._isReadOnly = openedReadOnly;
            }
            _variablesForm.Show();
            _variablesForm.Activate();
        }
        internal bool IsVariablesFormOpen() { return (_variablesForm != null && _variablesForm.Visible == true); }
        internal void CloseVariablesForm() { if (!IsVariablesFormOpen()) return; _variablesForm.Close(); UnloadVarConfigFacade(); _variablesForm = null; }

        internal UserSettingsAdministrator GetUserSettingsAdministrator()
        {
            if (_userSettingsAdministrator == null)
            {
                _userSettingsAdministrator = new UserSettingsAdministrator();
                _userSettingsAdministrator.LoadCurrentSettings();
                InitViewKeeper();
            }
            return _userSettingsAdministrator;
        }

        internal VCAdministrator GetVCAdministrator()
        {
            if (_vcAdministrator == null)
                _vcAdministrator = new VCAdministrator();
            return _vcAdministrator;
        }

        internal void SetState_btnVCLogInOut(bool setDoorOpen)
        {
            foreach (EM_UI_MainForm mainForm in GetAllMainForms_PlusEmptyForm())
                mainForm.SetState_btnVCLogInOut(setDoorOpen);
        }

        internal bool IsDoorOpen_btnVCLogInOut() { return _emptyForm.IsDoorOpen_btnVCLogInOut(); }

        internal void AllCountryMainForm_SetSetVCButtonsGreyState()
        {
            foreach (EM_UI_MainForm mainForm in GetAllMainForms_PlusEmptyForm())
                mainForm.SetVCButtonsGreyState();
        }

        internal void UpdateAllCountryMainFormChkShowFunctionSpecifiers()
        {
            foreach (EM_UI_MainForm countryMainForm in GetAllMainForms_PlusEmptyForm())
                countryMainForm.UpdateChkShowFunctionSpecifiers();
        }

        internal void ReloadCountry(string countryShortName)
        {
            foreach (EM_UI_MainForm countryMainForm in _countryMainForms)
                if (countryMainForm.GetCountryShortName().ToLower() == countryShortName.ToLower())
                {
                    countryMainForm.ReloadCountry();
                    return;
                }
        }

        internal void ReleaseVarConfigFacade()
        {
            _varConfigFacade = null;
        }

        internal AddParameterForm GetAddParameterForm()
        {
            if (_addParameterForm == null)
            {
                _addParameterForm = new AddParameterForm();
                _topMostForms.Add(_addParameterForm);
            }
            return _addParameterForm;
        }

        internal MatrixViewOfIncomelistsForm GetMatrixViewOfIncomelistsForm()
        {
            if (_matrixViewOfIncomelistsForm == null)
            {
                _matrixViewOfIncomelistsForm = new MatrixViewOfIncomelistsForm();
                _topMostForms.Add(_matrixViewOfIncomelistsForm);
            }
            return _matrixViewOfIncomelistsForm;
        }

        internal FindReplaceForm GetFindReplaceForm()
        {
            if (_findReplaceForm == null)
            {
                _findReplaceForm = new FindReplaceForm();
                _topMostForms.Add(_findReplaceForm);
            }
            return _findReplaceForm;
        }

        //make active topmost-window parent of message box, otherwise the message box is hidden behind the topmost-window and gives the impression that the programme got stuck
        internal IWin32Window GetTopMostWindow()
        {
            foreach (Form topMostForm in _topMostForms)
                if (topMostForm.Visible == true)
                    return topMostForm;
            return null;
        }

        internal void AddTopMostWindow(Form topMostForm) { _topMostForms.Add(topMostForm); }

        internal PasteFunctionAction GetPasteFunctionAction() { return _pasteFunctionAction; }
        internal void SetPasteFunctionAction(PasteFunctionAction pasteFunctionAction) { _pasteFunctionAction = pasteFunctionAction; }

        internal PastePolicyAction GetPastePolicyAction() { return _pastePolicyAction; }
        internal void SetPastePolicyAction(PastePolicyAction pastePolicyAction) { _pastePolicyAction = pastePolicyAction; }

        internal void AddAndShowCountryMainForm(EM_UI_MainForm countryMainForm)
        {
            BrandHandler.PrepareBrand(countryMainForm);
            _countryMainForms.Add(countryMainForm);
            countryMainForm.Show();
        }

        internal EM_UI_MainForm GetActiveCountryMainForm()
        {
            return this.MainForm as EM_UI_MainForm; //EM_AppContext.MainForm is set in EM_UI_MainForm.EM_UI_MainForm_Activated (as I found no other way to assess the active MainForm)
        }

        internal EM_UI_MainForm GetCountryMainForm(string countryName)
        {
            foreach (EM_UI_MainForm countryMainForm in _countryMainForms)
            {
                if (countryMainForm.GetCountryShortName().ToLower() == countryName.ToLower())
                    return countryMainForm;
            }
            return null;
        }

        internal List<EM_UI_MainForm> GetOpenCountriesMainForms() { return _countryMainForms; }

        internal bool IsAnythingOpen(bool checkVariablesForm = true)
        {
            if (_countryMainForms.Count != 0) return true;
            if (checkVariablesForm && IsVariablesFormOpen()) return true;
            return false;
        }

        internal bool CloseAnythingOpen()
        {
            if (IsAnythingOpen() || IsVariablesFormOpen())
            {
                if (UserInfoHandler.GetInfo("All open countries/add-ons/variables need to be closed. Proceed?", MessageBoxButtons.YesNo) == DialogResult.No)
                    return false;
                EM_AppContext.Instance.CloseVariablesForm();
                EM_AppContext.Instance.CloseAllMainForms(false);
                if (IsAnythingOpen() || IsVariablesFormOpen()) // check again in case something was not closed
                {
                    UserInfoHandler.ShowError("Problems with automatic closing - please close all countries/add-ons/variables manually.");
                    return false;
                }
            }
            return true;
        }

        internal List<EM_UI_MainForm> GetAllMainForms_PlusEmptyForm()
        {
            List<EM_UI_MainForm> allForms = new List<EM_UI_MainForm>(_countryMainForms);
            allForms.Add(_emptyForm);
            return allForms;
        }

        internal void UpdateAllCountryMainFormGalleries()
        {
            foreach (EM_UI_MainForm countryMainForm in GetAllMainForms_PlusEmptyForm())
            {
                countryMainForm.FillCountryGallery(true);
                countryMainForm.FillAddOnGallery(true);
                countryMainForm._runMainForm = null;
            }
        }

        internal void UpdateOutputFieldOnRunDialoguesIfDefault(string oldSettingsValue, string newSettingsValue)
        {
            // for each open interface, check if there is a Run Dialogue, and if so, check if its output field should be updated
            foreach (EM_UI_MainForm countryMainForm in GetAllMainForms_PlusEmptyForm())
            {
                if (!(countryMainForm._runMainForm == null || countryMainForm._runMainForm.IsDisposed))
                    countryMainForm._runMainForm.UpdateOutputIfDefault(oldSettingsValue, newSettingsValue);
            }
        }

        internal void InitViewKeeper()
        {
            ViewKeeper.Init(_userSettingsAdministrator.Get().ViewSettings);
        }

        internal void StoreViewSettings()
        {
            try { _userSettingsAdministrator.Get().ViewSettings = ViewKeeper.GetStoreString(); _userSettingsAdministrator.SaveCurrentSettings(false); }
            catch { }
        }

        internal void RemoveCountryMainForm(EM_UI_MainForm countryMainForm)
        {
            if (countryMainForm == _emptyForm)
            {
                StoreViewSettings();
                return; //close button was clicked in the empty form: no need to remove anything, just let the empty form (and therewith the application) close
            }

            if (_pasteFunctionAction != null && _pasteFunctionAction.GetCopyCountryShortName().ToLower() == countryMainForm.GetCountryShortName().ToLower())
                _pasteFunctionAction = null;
            if (_pastePolicyAction != null && _pastePolicyAction.GetCopyCountryShortName().ToLower() == countryMainForm.GetCountryShortName().ToLower())
                _pastePolicyAction = null;
           
            _countryMainForms.Remove(countryMainForm);

            //close application or show empty form if last country is closed
            if (_countryMainForms.Count == 0)
            {
                if (_userSettingsAdministrator.Get().CloseInterfaceWithLastMainform == DefPar.Value.NO)
                    ShowEmptyForm(true);
                else
                {
                    StoreViewSettings();
                    _emptyForm.Close(); //this is the last form open (currently hidden, as a country was open), the means the application will close as well
                }
            }
            //this might not be necessary: I first thought that the application closes if this.MainForm closes (which would make the code necessary to prevent closing)
            //testing proofed that this is not true, however I observed (and could not reproduce) that the application closed with closing "some", but not the last MainForm
            //I guess the code should do no harm and help to be on the save side ...
            else if (countryMainForm == this.MainForm)
                this.MainForm = _emptyForm; //in this context note that this.MainForm is set in EM_UI_MainForm.EM_UI_MainForm_Activated (also see GetActiveCountryMainForm)
        }

        internal void CloseAllMainForms(bool exit, bool closeVariablesForm = true)
        {
            //special case: no country open
            if (_countryMainForms.Count == 0)
            {
                if (exit)
                {
                    Application.Exit();
                }
                else
                {
                    if (closeVariablesForm) CloseVariablesForm();
                    return; //nothing to do if all countries are to be closed and none is open
                }
            }

            //use the user setting "Close Interface with Last Mainform" to realise "Close All Countries/Add-Ons" respectively "Exit"
            //in the former case the setting is set to "no", thus the UI stays open, in the latter the setting set to "yes", thus the UI closes
            string bkupCloseInterfaceSetting = _userSettingsAdministrator.Get().CloseInterfaceWithLastMainform;
            _userSettingsAdministrator.Get().CloseInterfaceWithLastMainform = exit ? DefPar.Value.YES : DefPar.Value.NO;
            EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);

            //calling Close for all countries effects amongst others that the "do you want to save" procedure (for unsaved changes) takes place
            this.MainForm = _emptyForm; //to make sure the application does not close before any of the open countries is checked for changes (see above)
            foreach (Form countryMainForm in _countryMainForms.ToArray<Form>())
                countryMainForm.Close();

            if (closeVariablesForm) CloseVariablesForm();

            //restore the setting as actually choosen by the user
            _userSettingsAdministrator.Get().CloseInterfaceWithLastMainform = bkupCloseInterfaceSetting;
            EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
        }

        internal void ShowEmptyForm(bool show)
        {
            if (_emptyForm == null)
                return; //just to be on the save side, should not happen
            _emptyForm.Visible = show;
            if (show)
                this.MainForm = _emptyForm;
        }

        internal bool CheckForActiveRunsOnLastMainFormClosing()
        {
            //check whether variables form is open to release the lock-file, if necessary
            if (_variablesForm != null && !_variablesForm._isReadOnly)
                InUseFileHandler.ReleaseFile(new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true));

            if (_countryMainForms.Count > 1 || _userSettingsAdministrator.Get().CloseInterfaceWithLastMainform == DefPar.Value.NO)
                return true; //closing MainForm is not the only one open

            //close active runs (after asking user) as well as potentially still open info-windows of terminated runs
            for (int index = _registeredRunManagers.Count - 1; index >= 0; --index)
                if (!_registeredRunManagers.ElementAt(index).Close())    
                    return false; //do not allow closing main form if user does not want to terminate active runs

            return true;
        }

        internal DialogResult WriteXml(string countryName, bool askUser, bool showRunWarning)
        {
            DialogResult dialogResult = DialogResult.No;
            try
            {
                foreach (EM_UI_MainForm countryMainForm in _countryMainForms)
                {
                    if (countryName.ToLower() == countryMainForm.GetCountryShortName().ToLower() && countryMainForm.HasChanges() && !countryMainForm._isReadOnly)
                    {
                        if (askUser)
                        {
                            string request = $"Do you want to save the changes for '{countryMainForm.GetCountryShortName()}'?";
                            if (showRunWarning) request += Environment.NewLine + Environment.NewLine +
                                "Note that, if you choose 'No', the run will be based on the saved version without the changes, but changes are retained in memory.";
                            dialogResult = Tools.UserInfoHandler.GetInfo(request, MessageBoxButtons.YesNoCancel);
                        }
                        if (dialogResult == DialogResult.Yes || !askUser)
                            countryMainForm.WriteXml();
                        break;
                    }
                }
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
            return dialogResult;
        }

        internal bool HasChanges()
        {
            foreach (EM_UI_MainForm countryMainForm in _countryMainForms)
                if (countryMainForm.HasChanges())
                    return true;
            return false;
        }

        internal RunManager RegisterRunManager()
        {
            lock (_registeredRunManagersLock)
            {
                _registeredRunManagers.Add(new RunManager());
                return _registeredRunManagers.ElementAt(_registeredRunManagers.Count - 1);
            }
        }

        internal void DeRegisterRunManager(RunManager runManager)
        {
            lock (_registeredRunManagersLock)
            {
                _registeredRunManagers.Remove(runManager);
            }
        }

        internal bool GetHelpPath(out string helpPath) { return GetInstalledFilePath(out helpPath, "HELP\\EUROMODHelp.chm"); }
        internal bool GetFuncConfigPath(out string funcConfigPath) { return GetInstalledFilePath(out funcConfigPath, "Configuration\\FuncConfig.xml"); }
        internal bool GetInstalledFilePath(out string installedFilePath, string localPath, string oldPath = "")
        {
            // NOTE: This function should only ever be used by the two functions above to retrieve the help & funcConfig paths

            //Application.ExecutablePath should be something like 'C:\Program Files (x86)\EUROMOD\EM_UI\EM_UI.exe'
            //to assess the base installation path we need to extract 'C:\Program Files (x86)\EUROMOD\' and add the local path (e.g. 'executable\\euromod.exe')
            string installationPath = EMPath.AddSlash((new FileInfo(Application.ExecutablePath)).Directory.Parent.FullName);
            if (EnvironmentInfo.isCompilerEnvironment) installationPath = EMPath.AddSlash(Directory.GetParent(Directory.GetParent(installationPath).FullName).FullName);
            installedFilePath = installationPath + localPath;
            if (File.Exists(installedFilePath))
                return true;

            //try if the 'old path' exists, i.e. the path within the EUROMOD bundle (e.g. '...\EuromodFiles\Executable\Euromod.exe')
            if (oldPath != string.Empty && File.Exists(oldPath))
            {
                installedFilePath = oldPath;
                return true;
            }

            EM_UI.Tools.UserInfoHandler.ShowError("File not found: '" + installedFilePath + "'.");
            return false;
        }

        internal static string FolderEuromodFiles { get { return EMPath.AddSlash(Instance.GetUserSettingsAdministrator().Get().EuromodFolder); } }
        internal static string FolderInput { get { return EMPath.AddSlash(Instance.GetUserSettingsAdministrator().Get().InputFolder); } }
        internal static string FolderOutput { get { return EMPath.AddSlash(Instance.GetUserSettingsAdministrator().Get().OutputFolder); } }
    }
}
