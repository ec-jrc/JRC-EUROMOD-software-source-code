using EM_Common;
using EM_Common_Win;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.Tools;
using EM_UI.UndoManager;
using EM_UI.VersionControl.Dialogs;
using EM_UI.VersionControl.Merging;
using EM_XmlHandler.VersionControl;
using Octokit;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using VCUIAPI;

namespace EM_UI.VersionControl
{
    internal class VCAdministrator
    {
        const string OPEN_CONFLICT = "OPEN CONFLICT";
        const string RESOLVED_CONFLICT = "RESOLVED CONFLICT";

        internal const string UI_VERSION = "=>";
        internal const string MOST_RECENT_VERSION = "°";

        internal const string VC_STATUS_UPTODATE = "up-to-date";
        internal const string VC_STATUS_MODIFIED = "modified";
        internal const string VC_STATUS_OLD = "old";
        internal const string VC_STATUS_NEW = "newer";
        internal const string VC_STATUS_CONFLICTED = "conflicted";
        internal const string VC_STATUS_RESOLVED_RECONFLICTED = "conflicted "; //don't show the user the difference between the different conflicted, but internally distinguish by spaces at the end
        internal const string VC_STATUS_RESOLVE_IN_PROG_RECONFLICTED = "conflicted  ";
        internal const string VC_STATUS_CONFLICT_RESOLVED = "conflict resolved";
        internal const string VC_STATUS_RESOLVE_IN_PROG = "resolve in progress";
        internal const string VC_STATUS_NOLOCAL = "no local unit";
        internal const string VC_STATUS_NOTINBUNDLE = "pending";
        internal readonly List<string> VC_UNSUPPORTED_MERGE = new List<string>() 
        {
            Path.GetFileNameWithoutExtension(EMPath.FILE_EMLOG).ToLower(),
        };

        internal VCAPI _vcAPI = null;
        internal string downloaded_bundle = String.Empty;
        internal static bool _isLoggedIn = false;

        internal VCAdministrator()
        {
            ReBoot(); //in fact Boot (i.e. initialise API with project-path)
        }

        ~VCAdministrator()
        {
            if (_vcAPI != null && _vcAPI.isLoggedIn) _vcAPI.Logout();
        }

        internal void InitButtons() //called when UserSettings (i.e. project) are changed
        {
            ReBoot(); //project-path probably has changed, therefore re-initialise
            // commented out to make sure that when opening a project you are always logged out.
            EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
        }

        #region HANDLE...FUNCTIONS

        internal bool HandleButtonLogInOutClicked()
        {
            try
            {
                if (IsLoggedIn())
                {
                    if (IsProjectVersionControlled()) HandleButtonDisConnectClicked(false);  // if connected, disconnect first!
                    _vcAPI.Logout();
                    _isLoggedIn = false;
                    EM_AppContext.Instance.SetState_btnVCLogInOut(false);
                    EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                    return true;
                }

                using (VCLogin loginDialog = new VCLogin(this))
                {
                    if (loginDialog.ShowDialog() == DialogResult.OK)
                    {
                        _isLoggedIn = true;
                        EM_AppContext.Instance.SetState_btnVCLogInOut(true);
                        EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                        return true;
                    }
                    else return false;
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: "+ exceptionMessage);
                return false;
            }

            

        }


        //Users will only be allowed to remove projects if they have admin rights for that projects
        internal void HandleButtonRemoveProjectClicked()
        {
            try
            {
                bool checkAdminRights = true; //Only projects in which the user has admin rights will be displayed
                VCSelectProject selectProject = new VCSelectProject(_vcAPI, "Version Control - Remove Project", checkAdminRights);
                if (selectProject.ShowDialog() == DialogResult.Cancel) return;

                int projectId = Convert.ToInt32(selectProject.GetSelectedProject().Id);
                string projectName = selectProject.GetSelectedProject().Name;
                if (UserInfoHandler.GetInfo(string.Format("Are you sure you want to remove project '{0} {1}' from version-control?", projectId, projectName),
                    MessageBoxButtons.YesNo) == DialogResult.No) return;

                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
                if (!_vcAPI.RemoveProjectFromVersionControl(projectId)) UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                else UserInfoHandler.ShowSuccess(string.Format("Successfull removed project '{0} {1}' from version-control.", projectId, projectName));
                if (IsProjectVersionControlled() && _vcAPI.vc_projectInfo.ProjectId == projectId)   // if you just removed the project you were connected to, disconnect!
                    HandleButtonDisConnectClicked(false);
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        internal void HandleButtonEstablishProjectClicked()
        {
            try
            {
                if (!WarnIsLoggedIn()) return;
                EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm();
                string projectName = string.Empty; long projectId; bool success;
                // if the local project is connected to VC, you need to disconnect it first
                if (IsProjectVersionControlled())
                {
                    if (UserInfoHandler.GetInfo("The project loaded by the interface is already connected to a project on version control." + Environment.NewLine
                        + "In order to proceed you need to first disconnect your local project." + Environment.NewLine + Environment.NewLine
                        + "Do you want to continue?", MessageBoxButtons.YesNo) == DialogResult.No) return;
                    if (!HandleButtonDisConnectClicked(false)) return;
                }
                projectName = EM_AppContext.Instance.GetProjectName(); //suggest the name of the currently loaded project, if this is not already VC-controlled (as it is likely that this project should be added to VC)

                if (UserInput.Get("Name of the new project", out projectName, projectName) == DialogResult.Cancel) return;

                mainForm.Cursor = Cursors.WaitCursor;
                success = _vcAPI.AddProjectToVersionControl(projectName, out projectId);
                if (!success) UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                else UserInfoHandler.ShowSuccess(string.Format("Successfully added project '{0} {1}' to version-control.", projectId, projectName));
                mainForm.Cursor = Cursors.Default;
                if (!success) return;

                mainForm.Cursor = Cursors.WaitCursor;
                success = _vcAPI.LinkLocalToVersionControl(projectId);
                if (!success) UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                else UserInfoHandler.ShowSuccess(string.Format("Successfully connected local project to version control project '{0} {1}'.", projectId, projectName));
                EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                mainForm.Cursor = Cursors.Default;
                if (!success) return;
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        internal bool HandleButtonDisConnectClicked(bool showMessages = true)
        {
            try
            {
                if (!WarnIsLoggedIn()) return false;

                EM_UI_MainForm mainForm = EM_AppContext.Instance.GetActiveCountryMainForm();

                bool disconnect = IsProjectVersionControlled();
                if (disconnect)
                {
                    if (showMessages && UserInfoHandler.GetInfo("Are you sure you want to disconnect the project from version control?", MessageBoxButtons.YesNo) == DialogResult.No) return false;
                    mainForm.Cursor = Cursors.WaitCursor;
                    if (!_vcAPI.UnlinkLocalFromVersionControl())
                    {
                        mainForm.Cursor = Cursors.Default;
                        UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                        return false;
                    }
                    else if (showMessages) UserInfoHandler.ShowSuccess("Successfully disconnected project from version control.");
                    _vcAPI.setExistAnyBundle(false);
                    mainForm.Cursor = Cursors.Default;
                }
                else
                {
                    VCSelectProject selectProject = new VCSelectProject(_vcAPI, "Connect Project to Version Control", false);
                    if (selectProject.ShowDialog() == DialogResult.Cancel) return false;


                    mainForm.Cursor = Cursors.WaitCursor;
                    if (!_vcAPI.LinkLocalToVersionControl(Convert.ToInt32(selectProject.GetSelectedProject().Id))) UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                    else
                    {
                        string mergingInfo = string.Empty;
                        if (_vcAPI.GetMergingUser(out mergingInfo))
                        {
                            if (mergingInfo == _vcAPI.GetCurrentUser().username)
                            {
                                mergingInfo = Environment.NewLine + Environment.NewLine + "Note: You seem to have an unfinished merge!";

                            }
                            else if (mergingInfo != string.Empty)
                            {
                                mergingInfo = Environment.NewLine + Environment.NewLine + "Note: User " + mergingInfo + " is currently merging this project.";
                            }
                        }
                        else
                        {
                            mergingInfo = Environment.NewLine + Environment.NewLine + "Note: Could not receive Merging info for this project.";
                        }
                        _vcAPI.checkExistAnyBundle();

                        UserInfoHandler.ShowSuccess(string.Format("Successfully connected local project to version control (project {0} {1}). {2}",
                                                                    selectProject.GetSelectedProject().Id, selectProject.GetSelectedProject().Name, mergingInfo));


                        if (_vcAPI.GetMergingUser(out mergingInfo))
                        {
                            if (mergingInfo == _vcAPI.GetCurrentUser().username)
                            {

                                string currentPath = _vcAPI.GetPath();
                                string filepath = currentPath + "\\online_bundle_" + selectProject.GetSelectedProject().Id.ToString() + ".txt";
                                if (File.Exists(filepath))
                                {
                                    string localFilePath = File.ReadLines(filepath).First();

                                    DialogResult result = MessageBox.Show("The bundle you are using was downloaded as latest bundle for this project. Would you like to use it as latest online bundle?", "Downloaded online bundle found.", MessageBoxButtons.YesNo);
                                    if (result == DialogResult.Yes)
                                    {
                                        _vcAPI.setLocalBundlePath(localFilePath);
                                        _vcAPI.setIsThisLatestBundle(true);
                                    }
                                    else if (result == DialogResult.No)
                                    {
                                        _vcAPI.setLocalBundlePath(String.Empty);
                                        _vcAPI.setIsThisLatestBundle(false);
                                        File.Delete(filepath);
                                    }
                                }
                                else
                                {
                                    _vcAPI.setLocalBundlePath(String.Empty);
                                    _vcAPI.setIsThisLatestBundle(false);
                                }

                            }
                        }
                    }

                    mainForm.Cursor = Cursors.Default;
                    selectProject.Dispose();
                }

                EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                return true;
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
                return false;
            }
        }

        internal bool GetReleaseInfo(long projectId, string tagName, out List<VersionControlUnitInfo> unitInfos)
        {
            ProgressIndicator progressIndicator = new ProgressIndicator(GetReleaseInfo_BackgroundEventHandler, "Getting online bundle info ...", new Tuple<long, string>(projectId, tagName));
            bool cancelled = progressIndicator.ShowDialog() == DialogResult.Cancel;
            Tuple<List<VersionControlUnitInfo>> t = progressIndicator.Result as Tuple<List<VersionControlUnitInfo>>;
            unitInfos = t.Item1;
            return !cancelled;
        }
        void GetReleaseInfo_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            Object lockingObject = new object();
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;

            List<VersionControlUnitInfo> units = new List<VersionControlUnitInfo>();
            List<long> removedItems = new List<long>();
            List<VersionControlUnitInfo> outUnits = new List<VersionControlUnitInfo>();

            Tuple<long, string> t = e.Argument as Tuple<long, string>;
            long projectId = t.Item1;
            string releaseName = t.Item2;

            List<VersionControlUnitInfo> unitInfos = _vcAPI.GetReleaseUnitsInfo(projectId, releaseName);
            if (unitInfos == null) { e.Cancel = true; return; }
            int done = 0;
            foreach(VersionControlUnitInfo unitInfo in unitInfos)
            {
                outUnits.Add(unitInfo);
                done++;
                double progressDouble = (done + 1.0) / (unitInfos.Count * 1.0) * 100.0;
                int progress = (int)progressDouble;
                backgroundWorker.ReportProgress(progress);
            }

            e.Result = new Tuple<List<VersionControlUnitInfo>>(outUnits);
        }

        // Function for comparing the local project with a given online bundle
        internal bool CompareLocalProjectWithOnlineBundle(List<VersionControlUnitInfo> unitInfos, out List<string> unitStati, bool getNewOld = false)
        {
            ProgressIndicator progressIndicator = new ProgressIndicator(CompareLocalProjectWithOnlineBundle_BackgroundEventHandler, "Comparing with local project ...", new Tuple<List<VersionControlUnitInfo>>(unitInfos));
            bool cancelled = progressIndicator.ShowDialog() == DialogResult.Cancel;
            unitStati = progressIndicator.Result as List<string>;
            return !cancelled;
        }

        void CompareLocalProjectWithOnlineBundle_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            Tuple<List<VersionControlUnitInfo>> t = e.Argument as Tuple<List<VersionControlUnitInfo>>;
            List<VersionControlUnitInfo> unitInfos = t.Item1;
            List<string> unitStati = new List<string>();

            for (int i = 0; i < unitInfos.Count; ++i)
            {
                if (backgroundWorker.CancellationPending) { e.Cancel = true; break; }
                unitStati.Add(GetLocalUnitStatus(unitInfos[i]));
                backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (unitInfos.Count * 1.0) * 100.0));
            }
            e.Result = unitStati;
        }

        internal string GetLocalUnitStatus(VersionControlUnitInfo unitInfo)
        {
            string localVersion = string.Empty;
            bool localUnitExists;
            if (!_vcAPI.DoesLocalUnitExist(out localUnitExists, unitInfo))
            {
                UserInfoHandler.ShowError(unitInfo.Name + ": Checking unit's local existence failed!" + Environment.NewLine + "Error: " + _vcAPI.GetErrorMessage());
                return VC_STATUS_MODIFIED;
            }
            if (!localUnitExists) return VC_STATUS_NOLOCAL;


            if (_vcAPI.getUnitMD5Hash(unitInfo) == unitInfo.UnitHash) return VC_STATUS_UPTODATE;
            else return VC_STATUS_MODIFIED;
        }


        internal bool GetUnitsStati(List<VersionControlUnitInfo> unitInfos, out List<string> unitStati, bool checkBundleExistence = false)
        {
            ProgressIndicator progressIndicator = new ProgressIndicator(GetUnitsStati_BackgroundEventHandler, "Assessing Units' Stati ...", new Tuple<List<VersionControlUnitInfo>,bool>(unitInfos, checkBundleExistence));
            bool cancelled = progressIndicator.ShowDialog() == DialogResult.Cancel;
            unitStati = progressIndicator.Result as List<string>;
            return !cancelled;
        }

        void GetUnitsStati_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            List<string> unitStati = new List<string>();
            Tuple<List<VersionControlUnitInfo>, bool> t = e.Argument as Tuple<List<VersionControlUnitInfo>, bool>;

            List<VersionControlUnitInfo> unitInfos = t.Item1;
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                if (backgroundWorker.CancellationPending) { e.Cancel = true; break; }
                unitStati.Add(GetUnitStatus(unitInfos[i], t.Item2));
                backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (unitInfos.Count * 1.0) * 100.0));
            }
            e.Result = unitStati;
        }

        internal string GetUnitStatus(VersionControlUnitInfo unitInfo, bool checkBundleExistence = false) {string dummy3; return GetUnitStatus(unitInfo, out dummy3, checkBundleExistence); }
        internal string GetUnitStatus(VersionControlUnitInfo unitInfo, out string resolvedCommitVersion, bool checkBundleExistence = false, string lastVersion = "")
        {
            resolvedCommitVersion = string.Empty;
            List<VersionControlUnitInfo> latestReleaseInfo = null;

            bool localUnitExists;
            if (!_vcAPI.DoesLocalUnitExist(out localUnitExists, unitInfo))
            {
                UserInfoHandler.ShowError(unitInfo.Name + ": Checking unit's local existence failed!" + Environment.NewLine + "Error: " + _vcAPI.GetErrorMessage());
                return VC_STATUS_MODIFIED;
            }
            if (!localUnitExists) return VC_STATUS_NOLOCAL;

            ReleaseInfo latestRelease = null;
            
            _vcAPI.GetLatestReleaseUnitInfo(unitInfo.ProjectId, out latestRelease);

            if (latestRelease == null)
            {
                return VC_STATUS_NOTINBUNDLE;
            }
            else
            {
                lastVersion = latestRelease.Name;
                latestReleaseInfo = _vcAPI.GetReleaseUnitsInfo(unitInfo.ProjectId, lastVersion);

                VersionControlUnitInfo unitInfoFound = latestReleaseInfo.Find(x => x.Name == unitInfo.Name);
                if (unitInfoFound != null)
                {
                    bool sameHash;
                    _vcAPI.CompareElements(out sameHash, unitInfoFound);
                    if (sameHash) { return VC_STATUS_UPTODATE; }
                    else { return VC_STATUS_MODIFIED; }
                }
                else
                {
                    return VC_STATUS_NOTINBUNDLE;
                }

            }

        }

        internal void HandleButtonAdministrateContent()
        {
            try
            {
                if (!WarnIsLoggedIn()) return;

                //The user cannot administrate content if another user if merging at that moment
                if (_vcAPI.isCurrentlyMerging) return;

                //The current user needs to have admin rights 
                //This button is only enable if the user had admin rights, so we could suppose that the user has admin rights
                //We are going to check again in case the permissions have changed.
                bool hasAdminRight = _vcAPI.HasCurrentUserCurrentProjectAdminRight();
                if (!hasAdminRight) { UserInfoHandler.ShowError("You need to have administration rights in this project to administrate content."); return; }

                if (!CloseIfAnythingOpen("Administrating content")) return;

                using (VCAdminContent adminContent = new VCAdminContent(this, hasAdminRight))
                {
                    adminContent.ShowDialog();
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }

        }

        private bool WarnIsLoggedIn(bool showWarning = true)
        {
            if (_vcAPI == null || !_vcAPI.isLoggedIn)
            {
                if (showWarning) UserInfoHandler.ShowError("Not Logged in!");
                return false;
            }
            return true;
        }

        internal class UnitExistence
        {
            internal string unitName { get; set; }
            internal VCAPI.VC_FOLDER_TYPE unitType { get; set; }
            internal bool existsPhysically { get; set; }
            internal bool existsInLocalVC { get; set; }
            internal bool existsInRemoteVC { get; set; }

            internal UnitExistence(string name, VCAPI.VC_FOLDER_TYPE type, bool physically = false, bool inLocalVC = false, bool inRemoteVC = false)
            { unitName = name; unitType = type; existsPhysically = physically; existsInLocalVC = inLocalVC; existsInRemoteVC = inRemoteVC; }
        }

        int GetIndexUnitExistence(List<UnitExistence> list, string unitName, VCAPI.VC_FOLDER_TYPE unitType)
        {
            for (int i = 0; i < list.Count; ++i) if (list[i].unitName.ToLower() == unitName.ToLower() && list[i].unitType == unitType) return i;
            return -1;
        }

        bool GetAllPossibleUnits(out List<UnitExistence> allPossibleUnits)
        {
            allPossibleUnits = new List<UnitExistence>();
            try
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;

                //(a) start the list with all physically existing units, i.e. countries in country-folder, add-ons in add-on folder and varconfig
                List<VersionControlUnitInfo> localUnits = GetContent_LocalProject();
                foreach (VersionControlUnitInfo localUnit in localUnits)
                    allPossibleUnits.Add(new UnitExistence(localUnit.Name, localUnit.UnitType, true));

                //(b) extend the list by all units listed in the local VC-file (VersionControl.xml)
                List<VersionControlUnitInfo> units; if (!_vcAPI.GetLocalUnits(out units) || units == null) { UserInfoHandler.ShowError(_vcAPI.GetErrorMessage()); return false; }
                foreach (VersionControlUnitInfo vcUnit in units)
                {
                    int index = GetIndexUnitExistence(allPossibleUnits, vcUnit.Name, vcUnit.UnitType);
                    if (index != -1) allPossibleUnits[index].existsInLocalVC = true;
                    else allPossibleUnits.Add(new UnitExistence(vcUnit.Name, vcUnit.UnitType, false, true));
                }

                //(c) extend the list by all units existing in the (remote) VC-System
                List<VersionControlUnitInfo> vcUnits; _vcAPI.GetRemoteUnits(_vcAPI.vc_projectInfo.ProjectId, out vcUnits);
                foreach (VersionControlUnitInfo vcUnit in vcUnits)
                {
                    int index = GetIndexUnitExistence(allPossibleUnits, vcUnit.Name, vcUnit.UnitType);
                    if (index != -1) allPossibleUnits[index].existsInRemoteVC = true;
                    else allPossibleUnits.Add(new UnitExistence(vcUnit.Name, vcUnit.UnitType, false, false, true));
                }

                return true;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
                return false;
            }
            finally { EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default; }
        }

        void UploadUnits_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            List<object> arguments = e.Argument as List<object>;
            List<UnitExistence> selectedUnits = arguments.ElementAt(0) as List<UnitExistence>;
            List<VersionControlUnitInfo> succesfulUnits = new List<VersionControlUnitInfo>();

            string success = string.Empty;
            Dictionary<string, string> failure = new Dictionary<string, string>();

            for (int i = 0; i < selectedUnits.Count; ++i)
            {
                UnitExistence selUnit = selectedUnits.ElementAt(i);
                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; }

                //(a) add to unit to VC
                long unitId;
                if (_vcAPI.AddUnitToVersionControl(selUnit.unitName, selUnit.unitType, out unitId))
                {
                    VersionControlUnitInfo unitInfo = _vcAPI.vc_projectInfo.GetUnit(selUnit.unitName, selUnit.unitType);
                    if (unitInfo == null)
                    { //should actually not happen - therefore not so sure what to put as error-message
                        failure.Add(selUnit.unitName, string.Format("Failed to upload the user interface's current version as the first version of {0}.", selUnit.unitName));
                        continue;
                    }
                    //(b) push the UI's version as the first commit
                    if(_vcAPI.PUSH(unitInfo, false))
                    {
                        AddUnitToMessage(ref success, selUnit.unitName);
                        succesfulUnits.Add(unitInfo);
                    }
                    else failure.Add(selUnit.unitName, _vcAPI.GetErrorMessage());

                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (selectedUnits.Count * 1.0) * 100.0));
                }
                else failure.Add(selUnit.unitName, _vcAPI.GetErrorMessage());
            }
            e.Result = new object[] { success, failure, succesfulUnits };
        }

        internal bool HandleButtonUploadUnits()
        {
            //gather all units that can be possibly uploaded
            List<UnitExistence> selectedUnits; string nextVersion;
            if (!SelectSpecificUnits(out selectedUnits, out nextVersion,
                ExistenceCriterion.MUST_EXIST, //the project folder contains (country/add-on/vardesc) xml-files
                ExistenceCriterion.MUSTNOT_EXIST, //the unit is not already version-controlled (known in the local (and hopefully remote) VC)
                ExistenceCriterion.MUSTNOT_EXIST)) //the unit is not already version-controlled (known in the remote VC)
                return false;

            //add the units selected in the dialog
            _vcAPI.StartMerge();
            List<object> arguments = new List<object>() {selectedUnits };
            ProgressIndicator progressIndicator = new ProgressIndicator(UploadUnits_BackgroundEventHandler, "Uploading Units ...", arguments);
            DialogResult result = progressIndicator.ShowDialog();

            if (result.Equals(DialogResult.Cancel))
            {
                AbortMerge(false);
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                return false;
            }
            
            object[] results = (object[])progressIndicator.Result;
            List<VersionControlUnitInfo> succesfulUnits =(List <VersionControlUnitInfo>) results[2];

            Dictionary<string, string> failureUnits = (Dictionary<string, string>)results[1];
            if(failureUnits!=null && failureUnits.Count > 0)
            {
                AbortMerge(false);
                ReportSuccessFailure("Adding local units to VC", (string)results[0], (Dictionary<string, string>)results[1]);
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                return false;
            }

            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
            //A new release is created
            //string nextAutoVersion = _vcAPI.NextAutoBundleVersion(_vcAPI.vc_projectInfo.ProjectId);
            string nextAutoVersion = nextVersion;

            bool added = true;
            bool mergeResult = FinishMergeNewProject(nextAutoVersion, succesfulUnits, added);
            _vcAPI.setExistAnyBundle(true);
            ReportSuccessFailure("Adding local units to VC", (string)results[0], (Dictionary<string, string>)results[1]);

            if (mergeResult) { UserInfoHandler.ShowSuccess("Created bundle " + nextAutoVersion + " successfully!"); EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState(); }
            

            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            return true;
        }

        enum ExistenceCriterion { MUST_EXIST, MUSTNOT_EXIST, IRRELEVANT };
        bool SelectSpecificUnits(out List<UnitExistence> selectedUnits, out string nextVersion,
                                 ExistenceCriterion physicalExistence, ExistenceCriterion localExistence, ExistenceCriterion remoteExistence, bool displayVersion = true)
        {
            //gather all units that meet the specifications
            List<UnitExistence> allPossibleUnits; selectedUnits = null; nextVersion = string.Empty;
            if (!GetAllPossibleUnits(out allPossibleUnits)) return false;

            List<UnitExistence> specificUnits = new List<UnitExistence>();
            foreach (UnitExistence unitExistence in allPossibleUnits)
            {
                switch (physicalExistence)
                {
                    case ExistenceCriterion.IRRELEVANT: break;
                    case ExistenceCriterion.MUST_EXIST: if (!unitExistence.existsPhysically) continue; break;
                    case ExistenceCriterion.MUSTNOT_EXIST: if (unitExistence.existsPhysically) continue; break;
                }
                switch (localExistence)
                {
                    case ExistenceCriterion.IRRELEVANT: break;
                    case ExistenceCriterion.MUST_EXIST: if (!unitExistence.existsInLocalVC) continue; break;
                    case ExistenceCriterion.MUSTNOT_EXIST: if (unitExistence.existsInLocalVC) continue; break;
                }
                switch (remoteExistence)
                {
                    case ExistenceCriterion.IRRELEVANT: break;
                    case ExistenceCriterion.MUST_EXIST: if (!unitExistence.existsInRemoteVC) continue; break;
                    case ExistenceCriterion.MUSTNOT_EXIST: if (unitExistence.existsInRemoteVC) continue; break;
                }
                specificUnits.Add(unitExistence);
            }

            //show dialog to select the units
            string nextAutoVersion = _vcAPI.NextAutoBundleVersion(_vcAPI.vc_projectInfo.ProjectId);
            VCAddRemoveUnits addRemoveUnits = new VCAddRemoveUnits(specificUnits, displayVersion, nextAutoVersion);
            if (addRemoveUnits.ShowDialog() == DialogResult.Cancel) return false;
            selectedUnits = addRemoveUnits.GetSelected();
            nextVersion = addRemoveUnits.getNextVersion();

            return true;
        }

        internal bool HandleButtonDownloadUnits()
        {
            //gather all units that can be possibly downloaded from VC
            List<UnitExistence> selectedUnits; string nextVersion = string.Empty;
            if (!SelectSpecificUnits(out selectedUnits, out nextVersion,
                ExistenceCriterion.IRRELEVANT, //it is not relevant whether the project folder contains (country/add-on/vardesc) xml-files (would be overwritten)
                ExistenceCriterion.MUSTNOT_EXIST, //non-existence in the local VersionControl.xml is the reason why one wants to download the unit
                ExistenceCriterion.MUST_EXIST, false)) //the unit must be remotely VC-controlled (of course)
                return false;

            //connect the units selected in the dialog to VC and download them
            string success = string.Empty;
            Dictionary<string, string> failure = new Dictionary<string, string>();

            //The most recent version of the Unit will be downloaded, this is the newest release
           ReleaseInfo latestRelease = null;
            string mostRecentRelease = string.Empty;

            foreach (UnitExistence selUnit in selectedUnits)
            {
                VersionControlUnitInfo vcUnit; string errorMessage; //need to get the unit to have its id for connecting
                if (!GetRemoteUnit(selUnit.unitName, selUnit.unitType, out vcUnit, out errorMessage))
                { failure.Add(selUnit.unitName, errorMessage); continue; }

                //connect the unit (write to VersionControl.xml)
                if (!_vcAPI.LinkUnitToVersionControl(vcUnit))
                { failure.Add(selUnit.unitName, _vcAPI.GetErrorMessage()); continue; }

                if (!_vcAPI.GetLatestReleaseUnitInfo(vcUnit.ProjectId, out latestRelease))
                {
                    failure.Add(selUnit.unitName, errorMessage); continue;
                }

                if(latestRelease != null)
                {
                    mostRecentRelease = latestRelease.Name;
                }

                //create a country/add-on-folder if necessary (for security-reasons check for folder instead of using selUnit.existsPhysically, because the latter would be false if the folder exists, but does not contain the correct xml-files)
                if ((selUnit.unitType == VCAPI.VC_FOLDER_TYPE.COUNTRY && !Directory.Exists(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + selUnit.unitName)) ||
                    (selUnit.unitType == VCAPI.VC_FOLDER_TYPE.ADDON && !Directory.Exists(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + selUnit.unitName)))
                {
                    try
                    {
                        Directory.CreateDirectory((selUnit.unitType == VCAPI.VC_FOLDER_TYPE.COUNTRY ? EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles)) + selUnit.unitName);
                    }
                    catch (Exception exception) { failure.Add(selUnit.unitName, exception.Message); continue; }
                }

                //get most recent version, unless user wants to avoid overwritting her local (not VC-controlled) version
                if (selUnit.existsPhysically &&
                    UserInfoHandler.GetInfo("Do you want to download the unit's most recent version?" + Environment.NewLine +
                        "(Note that by this action your local version would be overwritten and lost.)", MessageBoxButtons.YesNo) == DialogResult.No)
                {
                    AddUnitToMessage(ref success, selUnit.unitName);
                    continue;
                }

                if (!_vcAPI.GET(vcUnit, mostRecentRelease)) failure.Add(selUnit.unitName, _vcAPI.GetErrorMessage());
                else AddUnitToMessage(ref success, selUnit.unitName);
            }

            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //to show any new countries/add-ons created by downloading
            return ReportSuccessFailure("Getting units from VC", success, failure);
        }

        internal bool HandleButtonRemoveUnits()
        {
            //gather all units that can be possibly removed from VC
            List<UnitExistence> selectedUnits; string nextVersion;
            if (!SelectSpecificUnits(out selectedUnits, out nextVersion,
                ExistenceCriterion.IRRELEVANT, //it is not relevant whether the project folder contains (country/add-on/vardesc) xml-files (perhaps deleted per accident)
                ExistenceCriterion.MUST_EXIST, //existence in the local VersionControl.xml is assumed necessary, as it's rather strange to remove a unit from VC, which is actually not even downloaded
                ExistenceCriterion.MUST_EXIST)) //the unit must (currently) be remotely VC-controlled to be removed from VC
                return false;

            //remove the units selected in the dialog
            /*
            DialogResult answer = UserInfoHandler.GetInfo("Do you also want to delete the units physically (i.e. the local XML-files)?", MessageBoxButtons.YesNoCancel);
            if (answer == DialogResult.Cancel) return false;
            bool removePhysically = answer == DialogResult.Yes;
            */

            string success = string.Empty;
            Dictionary<string, string> failure = new Dictionary<string, string>();
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
            List<VersionControlUnitInfo> succesfulUnits = new List<VersionControlUnitInfo>();
            _vcAPI.StartMerge();
            foreach (UnitExistence selUnit in selectedUnits)
            {
                VersionControlUnitInfo unitInfo = _vcAPI.vc_projectInfo.GetUnit(selUnit.unitName, selUnit.unitType);
                if (unitInfo == null) continue; //should not happen(?)
                if (_vcAPI.RemoveUnitFromVersionControl(_vcAPI.vc_projectInfo.ProjectId, unitInfo))
                {
                    /*if (removePhysically)
                    {
                        try
                        {
                            if (unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG) File.Delete(new EMPath(EM_AppContext.FolderEuromodFiles).GetVarFilePath(true));
                            else Directory.Delete((unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.COUNTRY ? EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) : EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles)) + unitInfo.Name, true);
                            CountryAdministrator.RemoveCountryFromList(selUnit.unitName);
                        }
                        catch (Exception exception) { failure.Add(selUnit.unitName, exception.Message); continue; }
                    }*/
                    AddUnitToMessage(ref success, selUnit.unitName);
                    succesfulUnits.Add(unitInfo);
                }
                else failure.Add(selUnit.unitName, _vcAPI.GetErrorMessage());
            }


            //A new release is created
            //string nextAutoVersion = _vcAPI.NextAutoBundleVersion(_vcAPI.vc_projectInfo.ProjectId);
            string nextAutoVersion = nextVersion;
            bool added = false;
            bool mergeResult = FinishMergeNewProject(nextAutoVersion, succesfulUnits, added);
            ReportSuccessFailure("Removing units", success, failure);
            if (mergeResult) { UserInfoHandler.ShowSuccess("Created bundle " + nextAutoVersion + " successfully!"); _vcAPI.setExistAnyBundle(true); }

            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            //if (removePhysically) EM_AppContext.Instance.UpdateAllCountryMainFormGalleries();
            EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
            return true;
            
        }

        internal void HandleButtonDownloadRelease()
        {
            try
            {
                if (!CloseIfAnythingOpen("Downloading Bundle")) return;


                using (VCDownloadRelease vcDownloadRelease = new VCDownloadRelease(this))
                {
                    if (vcDownloadRelease.ShowDialog() == DialogResult.Cancel) return;

                    long projectId; ReleaseInfo releaseInfo; string alternativePath;
                    List<VersionControlUnitInfo> unitInfos; List<VCDownloadRelease.DownloadAction> downloadActions;
                    vcDownloadRelease.GetChoices(out projectId, out releaseInfo, out unitInfos, out downloadActions, out alternativePath);

                    if (downloadActions.Contains(VCDownloadRelease.DownloadAction.getReleaseVersion))
                    {
                        DialogResult result = MessageBox.Show("You have selected 'Get online version' for one or more units. Please, note that your current files will be overwritten for those units with the bundle you selected. If you do not want to overwrite your files, please click on 'Cancel' and download the bundle via the option 'New project'. Do you want to continue?", "EUROMOD - Confirmation", MessageBoxButtons.OKCancel);
                        if (result == DialogResult.Cancel)
                        {
                            return;
                        }
                    }

                    EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
                    if (alternativePath == string.Empty)
                    {
                        Tuple<long, string, List<VersionControlUnitInfo>, List<VCDownloadRelease.DownloadAction>> arguments = new Tuple<long, string, List<VersionControlUnitInfo>, List<VCDownloadRelease.DownloadAction>>(projectId, releaseInfo.Name, unitInfos, downloadActions);
                        ProgressIndicator progressIndicator = new ProgressIndicator(DownloadRelease_BackgroundEventHandler, "Downloading Bundle ...", arguments);
                        bool cancelled = progressIndicator.ShowDialog() == DialogResult.Cancel;
                        object[] results = (object[])progressIndicator.Result;
                        if (cancelled)
                            UserInfoHandler.ShowError("User cancelled the Download.");
                        else
                            ReportSuccessFailure("Downloading Bundle", (string)results[0], (Dictionary<string, string>)results[1]);
                    }
                }
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }


        internal void HandleButtonDownloadLatestRelease()
        {
            try
            {
                if (!WarnIsLoggedIn()) return;

                if (!CloseIfAnythingOpen("Downloading Bundle")) return;
                string currentPath = String.Empty;

                VCAdministrator vcAdministrator = IsLoggedIn() ? EM_AppContext.Instance.GetVCAdministrator() : null;
                using (VCDownloadLatestBundle vcDownloadLatestBundle = new VCDownloadLatestBundle(this))
                {
                    if (vcDownloadLatestBundle.ShowDialog() == DialogResult.Cancel) return;

                    string projectName; string releaseName; string destinationFolder; long projectId; long releaseId; VCNewProject.ProjectContent projectContent;

                    vcDownloadLatestBundle.GetInfo(out destinationFolder, out projectName, out projectId, out releaseName, out releaseId, out projectContent);

                    if (vcAdministrator == null) vcAdministrator = EM_AppContext.Instance.GetVCAdministrator();

                    string euromodFolder = CountryAdministrator.GenerateEuromodFileStructure(destinationFolder, projectName + "_" + releaseName);

                    if (!euromodFolder.Equals(String.Empty))
                    {

                        string pathUserSettings = UserSettingsAdministrator.GenerateProjectSettings(EMPath.AddSlash(euromodFolder).ToLower());

                        currentPath = vcAdministrator._vcAPI.GetPath();

                        vcAdministrator._vcAPI.ChangePath(euromodFolder);   // make sure that the vcAPI has the right path into which to download the new project's units

                        EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
                        projectContent.selectedRelease = releaseName;
                        ProgressIndicator progressIndicator = new ProgressIndicator(NewRelease_BackgroundEventHandler, "Downloading Bundle ...", projectContent);
                        bool cancelled = progressIndicator.ShowDialog() == DialogResult.Cancel;
                        object[] results = (object[])progressIndicator.Result;
                        downloaded_bundle = destinationFolder;
                        if (cancelled)
                        {
                            UserInfoHandler.ShowError("User cancelled the Download.");
                            downloaded_bundle = String.Empty;
                        }
                        else
                        {
                            ReportSuccessFailure("Downloading Bundle", (string)results[0], (Dictionary<string, string>)results[1]);

                            string path = euromodFolder + "\\online_bundle_" + projectId.ToString() + ".txt";
                            using (StreamWriter sw = new StreamWriter(path))
                                sw.WriteLine(currentPath);

                            EM_AppContext.Instance.GetUserSettingsAdministrator().LoadCurrentSettings(pathUserSettings, true, EMPath.AddSlash(euromodFolder).ToLower());
                            CountryAdministrator.ClearCountryList();
                            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //only the empty main form is open and must be updated
                            EM_AppContext.Instance.UpdateMainFormCaption(); //set title (of single open mainform) to "EUROMOD Version (Path)"

                            if (_vcAPI.VCLogInAfterDownloading() && _vcAPI.LinkLocalToVersionControl(Convert.ToInt32(projectId)))
                            {

                                string mergingInfo = string.Empty;
                                _vcAPI.GetMergingUser(out mergingInfo);
                                _vcAPI.setIsThisLatestBundle(true);
                                _vcAPI.setLocalBundlePath(currentPath);

                                EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                            }

                            UserInfoHandler.ShowInfo("The latest version of the bundle has been downloaded and opened.");
                        }
                    }

                }

                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
            }
            catch (Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
                
            }
            
            
        }

        internal void HandleButtonRemoveBundle()
        {
            try
            {
                //The project needs to be connected to the version control
                if (!_vcAPI.IsVersionControlled()) return;

                //The user needs to have permission to write (push)
                if (!_vcAPI.HasCurrentUserCurrentProjectPushRight()) return;


                using (VCRemoveBundle removeBundle = new VCRemoveBundle(_vcAPI, true))
                {
                    removeBundle.ShowDialog();
                }

                _vcAPI.checkExistAnyBundle();
                EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
            }catch(Exception ex){
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }

        }

        internal static void HandleButtonRemovePrivate()
        {
            if (!CloseIfAnythingOpen("Removing private components")) return;
            if (UserInfoHandler.GetInfo("Are you sure you want to remove all private components?" + Environment.NewLine +
                "Please note that no undo is possible!", MessageBoxButtons.YesNo) == DialogResult.No) return;
            RemovePrivate.RemovePrivateComponents();
        }


        internal void HandleButtonAdministrateUsers()
        {
            try
            {
                if (!WarnIsLoggedIn()) return;

                if (!_vcAPI.HasCurrentUserCurrentProjectAdminRight())
                {
                    UserInfoHandler.ShowInfo("Administrating users requires administration rights.");
                    return;
                }

                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
                using (VCUsers vcUsers = new VCUsers(_vcAPI))
                {
                    EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                    if (vcUsers.ShowDialog() == DialogResult.Cancel)
                        return;

                    List<VCUsers.UserRightInfo> usersToAdd, usersToChange; List<UserInfo> usersToRemove;
                    vcUsers.GetChanges(out usersToAdd, out usersToRemove, out usersToChange);

                    string success = string.Empty;
                    Dictionary<string, string> failure = new Dictionary<string, string>();

                    EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;

                    //add users (in principle, adding and changing is the same procedure, i.e. this if-sequence does the same as the next, just the success-message differs)
                    if (usersToAdd.Count > 0)
                    {
                        foreach (VCUsers.UserRightInfo userToAdd in usersToAdd)
                        {
                            if (_vcAPI.addorUpdateUserToRepository(_vcAPI.vc_projectInfo.ProjectId, userToAdd.userInfo.username, userToAdd.hasProjectAdminRight, userToAdd.defaultUnitRight))
                                AddUnitToMessage(ref success, "adding " + userToAdd.userInfo.username);
                            else failure.Add("adding " + userToAdd.userInfo.username, _vcAPI.GetErrorMessage());
                        }
                    }

                    //change users rights-settings
                    if (usersToChange.Count > 0)
                    {
                        foreach (VCUsers.UserRightInfo userToChange in usersToChange)
                        {
                            if (_vcAPI.addorUpdateUserToRepository(_vcAPI.vc_projectInfo.ProjectId, userToChange.userInfo.username, userToChange.hasProjectAdminRight, userToChange.defaultUnitRight))
                                AddUnitToMessage(ref success, "changing " + userToChange.userInfo.username);
                            else failure.Add("changing " + userToChange.userInfo.username, _vcAPI.GetErrorMessage());
                        }
                    }

                    //remove users
                    if (usersToRemove.Count > 0)
                    {
                        foreach (UserInfo userToRemove in usersToRemove)
                        {
                            if (_vcAPI.RemoveUserFromProject(_vcAPI.vc_projectInfo.ProjectId, userToRemove.userId, userToRemove.username))
                                AddUnitToMessage(ref success, "removing " + userToRemove.username);
                            else failure.Add("removing " + userToRemove.username, _vcAPI.GetErrorMessage());
                        }
                    }

                    EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;

                    if (success == string.Empty && failure.Count == 0) UserInfoHandler.ShowSuccess("Nothing to change.");
                    else ReportSuccessFailure("Changing project's user settings", success, failure);
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        internal void StartMerge()
        {
            try
            {
                if (!WarnIsLoggedIn()) return;

                if (MessageBox.Show(null, "Are you sure you want to start a new online bundle?", "New online bundle", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    if (_vcAPI.StartMerge())
                    {
                        MessageBox.Show("New online bundle started");
                    }

                    else
                    {
                        MessageBox.Show("Could not start a new online bundle: " + Environment.NewLine + _vcAPI.GetErrorMessage());
                    }
                    EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        internal void FinishMerge()
        {
            try
            {
                if (!WarnIsLoggedIn()) return;

                if (EM_AppContext.Instance.HasChanges())
                {
                    switch (Tools.UserInfoHandler.GetInfo("Do you want to save changes to all open countries?", MessageBoxButtons.YesNoCancel))
                    {
                        case DialogResult.Cancel: return;
                        case DialogResult.Yes:
                            {
                                List<EM_UI_MainForm> countryMainForms = EM_AppContext.Instance.GetOpenCountriesMainForms();
                                foreach (EM_UI_MainForm country in countryMainForms)
                                {
                                    country.WriteXml();
                                }
                                break;
                            }
                    }
                }

                if (MessageBox.Show(null, "Are you sure you want to upload your files?", "Upload project files", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {

                    string nextAutoVersion = _vcAPI.NextAutoBundleVersion(_vcAPI.vc_projectInfo.ProjectId);
                    VCMultiUpDownload vcMultiUpDownload = new VCMultiUpDownload(this, true, true, nextAutoVersion);

                    if (vcMultiUpDownload.ShowDialog() == DialogResult.OK)
                    {
                        EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;

                        string releaseVersion = vcMultiUpDownload.GetVersion();
                        List<VersionControlUnitInfo> successfulUploads = vcMultiUpDownload.GetSuccessfulUploads();

                        if (!_vcAPI.PushRelease(releaseVersion, successfulUploads, true))
                            UserInfoHandler.ShowError("Failure in uploading release:" + Environment.NewLine + _vcAPI.GetErrorMessage() + Environment.NewLine + Environment.NewLine + "Please try finishing the Merge again!");
                        else
                        {
                            List<string> changes = ChangeLogAdministrator.GetChanges(releaseVersion); // extract changes from EM_Log
                            if (_vcAPI.FinishMerge(releaseVersion, changes))
                            {
                                UserInfoHandler.ShowSuccess("Created bundle " + releaseVersion + " successfully!");

                                string path = EM_AppContext.FolderEuromodFiles + "\\online_bundle_" + _vcAPI.vc_projectInfo.ProjectId.ToString() + ".txt";
                                if (File.Exists(path)) { File.Delete(path); }
                                _vcAPI.setLocalBundlePath(String.Empty);
                                _vcAPI.setIsThisLatestBundle(false);

                                string newFolderName = _vcAPI.vc_projectInfo.ProjectName + "_" + releaseVersion;
                                string newProjectPath = string.Empty;
                                if (MessageBox.Show(null, "Would you like to rename your project's folder to " + newFolderName + "?", "Rename current project folder", MessageBoxButtons.YesNo) == DialogResult.Yes)
                                {
                                    if (CloseIfAnythingOpen("Renaming the project's folder"))
                                    {
                                        EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
                                        if (_vcAPI.RenameProjectFolder(newFolderName, out newProjectPath) && !string.IsNullOrEmpty(newProjectPath))
                                        {
                                            string pathUserSettings = UserSettingsAdministrator.GenerateProjectSettings(EMPath.AddSlash(newProjectPath).ToLower());

                                            EM_AppContext.Instance.GetUserSettingsAdministrator().LoadCurrentSettings(pathUserSettings, true, EMPath.AddSlash(newProjectPath).ToLower());
                                            CountryAdministrator.ClearCountryList();
                                            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //only the empty main form is open and must be updated
                                            EM_AppContext.Instance.UpdateMainFormCaption(); //set title (of single open mainform) to "EUROMOD Version (Path)"

                                            if (_vcAPI.VCLogInAfterDownloading())
                                            {

                                                string mergingInfo = string.Empty;
                                                _vcAPI.GetMergingUser(out mergingInfo);

                                                EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                                            }
                                            UserInfoHandler.ShowSuccess("The project's folder has been renamed.");
                                        }
                                        else
                                        {
                                            UserInfoHandler.ShowError("The project's folder cannot be renamed.");
                                        }
                                    }
                                     
                                }
                            }


                        }

                        EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                    }

                    _vcAPI.checkExistAnyBundle();
                    EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        internal bool FinishMergeNewProject(string nextAutoVersion, List<VersionControlUnitInfo> successfulUploads, bool added)
        {

            if (!_vcAPI.PushRelease(nextAutoVersion, successfulUploads, added))
            {
                UserInfoHandler.ShowError("Failure in uploading release:" + Environment.NewLine + _vcAPI.GetErrorMessage() + Environment.NewLine + Environment.NewLine + "Please try finishing the Merge again!");
                return false;
            }
                
            else
            {
                List<string> changes = ChangeLogAdministrator.GetChanges(nextAutoVersion); // extract changes from EM_Log
                if (_vcAPI.FinishMerge(nextAutoVersion, changes))
                {

                    string path = EM_AppContext.FolderEuromodFiles + "\\online_bundle_" + _vcAPI.vc_projectInfo.ProjectId.ToString() + ".txt";
                    if (File.Exists(path)) { File.Delete(path); }
                    _vcAPI.setLocalBundlePath(String.Empty);
                    _vcAPI.setIsThisLatestBundle(false);
                }
                

            }

            _vcAPI.checkExistAnyBundle();
            EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
            return true;
        }



        internal void AbortMerge(bool askIfAbort = true)
        {
            try
            {
                if (!WarnIsLoggedIn()) return;

                DialogResult result = DialogResult.Yes;
                if (askIfAbort)
                {
                    result = MessageBox.Show(null, "Are you sure you want to abort the creation of a new online bundle?", "Abort new online bundle", MessageBoxButtons.YesNo);
                }

                if (result == DialogResult.Yes)
                {
                    long projectId = _vcAPI.vc_projectInfo.ProjectId;
                    if (_vcAPI.AbortMerge())
                    {
                        if (askIfAbort) { MessageBox.Show("Aborted creation of a new online bundle.", "Abort new online bundle."); }


                        //If the latest bundle was downloaded and we are on it
                        if (_vcAPI.isThisLatestBundle())
                        {
                            string localBundlePath = _vcAPI.getLocalBundlePath();
                            string latestBundlePath = EM_AppContext.FolderEuromodFiles;

                            //The file that identifies the current bundle as latest bundle is deleted
                            string latestBundlefilePath = EM_AppContext.FolderEuromodFiles + "\\online_bundle_" + _vcAPI.vc_projectInfo.ProjectId.ToString() + ".txt";
                            if (File.Exists(latestBundlefilePath)) { File.Delete(latestBundlefilePath); }

                            //The path for the local bundle is cleared.
                            _vcAPI.setLocalBundlePath(String.Empty);
                            //The current bundle is not the latest bundle anymore.
                            _vcAPI.setIsThisLatestBundle(false);

                            //If the local bundle still exist and it still has the right structure, it is opened again
                            if (localBundlePath != string.Empty && Directory.Exists(localBundlePath) && Directory.Exists(EMPath.AddSlash(localBundlePath) + EMPath.Folder_Countries_withoutPath()))
                            {
                                ReloadUserSettings(localBundlePath);
                                EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //only the empty main form is open and must be updated
                                EM_AppContext.Instance.UpdateMainFormCaption(); //set title (of single open mainform) to "EUROMOD Version (Path)"

                                //The latest bundle is removed
                                if (Directory.Exists(latestBundlePath)) { Directory.Delete(latestBundlePath, true); }

                                if (_vcAPI.VCLogInAfterDownloading() && _vcAPI.LinkLocalToVersionControl(projectId))
                                {

                                    string mergingInfo = string.Empty;
                                    _vcAPI.GetMergingUser(out mergingInfo);

                                    EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                                }

                                MessageBox.Show("The bundle from which you initiated the new online bundle has been opened.");
                            }

                        }

                    }

                    else
                    {
                        MessageBox.Show("Could not abort creation of a new online bundle: " + Environment.NewLine + _vcAPI.GetErrorMessage());
                    }
                    _vcAPI.checkExistAnyBundle();

                    EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        static void  ReloadUserSettings(string projectPath)
        {
            List<string> pathsUserSettings = null;
            List<string> projectPaths = UserSettingsAdministrator.GetAvailableProjectPaths(out pathsUserSettings);
            string pathUserSettings = string.Empty;
            if (projectPaths.Contains(projectPath)) //loading a project with existing user-settings
                pathUserSettings = pathsUserSettings.ElementAt(projectPaths.IndexOf(projectPath));

            EM_AppContext.Instance.StoreViewSettings();
            EM_AppContext.Instance.GetUserSettingsAdministrator().LoadCurrentSettings(pathUserSettings);
            EM_AppContext.Instance.InitViewKeeper();
        }

        //actually this function doesn't quite fit in the VCAdministrator - the only reason it is there is that it was developped with VC
        //but I do not know where it really would fit
        internal void HandleMenuNewProject(Form caller)
        {
            try
            {
                if (!CloseIfAnythingOpen("Generating a new project")) return;

                VCAdministrator vcAdministrator = IsLoggedIn() ? EM_AppContext.Instance.GetVCAdministrator() : null;
                VCNewProject newProjectDialog = new VCNewProject(vcAdministrator);
                if (newProjectDialog.ShowDialog() == DialogResult.Cancel) return;

                string projectPath, projectName; bool diskBaseProject, vcBaseProject; VCNewProject.ProjectContent projectContent;
                if (!newProjectDialog.GetInfo(out projectPath, out projectName, out diskBaseProject, out vcBaseProject, out projectContent)) return;

                caller.Cursor = Cursors.WaitCursor;

                //create the project locally
                string euromodFolder = CountryAdministrator.GenerateEuromodFileStructure(projectPath, projectName);
                if (euromodFolder == string.Empty) { caller.Cursor = Cursors.Default; return; }
                string pathUserSettings = UserSettingsAdministrator.GenerateProjectSettings(EMPath.AddSlash(euromodFolder).ToLower());

                string success = string.Empty;
                Dictionary<string, string> failure = new Dictionary<string, string>();
                if (diskBaseProject) //copy selected files from disk-base-project
                {
                    for (int i = 0; i < projectContent.projectUnits.Count; i++)
                    {
                        if (!projectContent.selections[i]) continue; //unit not selected
                        VersionControlUnitInfo unit = projectContent.projectUnits[i];
                        bool copySuccess = true; string errorMessage = string.Empty;
                        switch (unit.UnitType)
                        {
                            case VCAPI.VC_FOLDER_TYPE.COUNTRY: copySuccess = CountryAdministrator.CopyCountry(unit.Name, projectContent.baseProjectPath, euromodFolder, out errorMessage); break;
                            case VCAPI.VC_FOLDER_TYPE.ADDON: copySuccess = CountryAdministrator.CopyAddOn(unit.Name, projectContent.baseProjectPath, euromodFolder, out errorMessage); break;
                            case VCAPI.VC_FOLDER_TYPE.CONFIG: copySuccess = CountryAdministrator.CopyConfig(unit.Name, projectContent.baseProjectPath, euromodFolder, out errorMessage); break;
                            case VCAPI.VC_FOLDER_TYPE.INPUT: copySuccess = CopyInputOrLog(false, out errorMessage); break;
                            case VCAPI.VC_FOLDER_TYPE.LOG: copySuccess = CopyInputOrLog(true, out errorMessage); break;
                            default: break;
                        }
                        if (copySuccess) AddUnitToMessage(ref success, unit.Name); else failure.Add(unit.Name, errorMessage);
                    }
                }
                else if (vcBaseProject) //download selected files from VC-base-project
                {
                    if (vcAdministrator == null) vcAdministrator = EM_AppContext.Instance.GetVCAdministrator();

                    vcAdministrator._vcAPI.ChangePath(euromodFolder);   // make sure that the vcAPI has the right path into which to download the new project's units

                    EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
                    ProgressIndicator progressIndicator = new ProgressIndicator(NewRelease_BackgroundEventHandler, "Downloading Bundle ...", projectContent);
                    bool cancelled = progressIndicator.ShowDialog() == DialogResult.Cancel;
                    object[] results = (object[])progressIndicator.Result;
                    if (cancelled) { UserInfoHandler.ShowError("User cancelled the Download."); caller.Cursor = Cursors.Default; return; }
                    else ReportSuccessFailure("Downloading Bundle", (string)results[0], (Dictionary<string, string>)results[1]);
                }
                //else chkNoBaseProject checked: nothing to do

                //update UI-environement (i.e. load new project)
                //EM_AppContext.Instance.GetUserSettingsAdministrator().LoadCurrentSettings(pathUserSettings);
                EM_AppContext.Instance.GetUserSettingsAdministrator().LoadCurrentSettings(pathUserSettings, true, EMPath.AddSlash(euromodFolder).ToLower());
                CountryAdministrator.ClearCountryList();

                // the plan is, to replace this rather untidy procedure (most importantly: not removing extensions referring the system, but also not taking care of pol/par/fun getting obsolete)
                // once we have a more considered idea about a unique handling of export (maybe also in terms of user interface)
                if (projectContent.selectedYears.Count > 0)
                {
                    foreach (Country country in CountryAdministrator.GetCountries(true))
                    {
                        try
                        {
                            EMPath emPath = new EMPath(euromodFolder);
                            CountryConfigFacade ccf = country.GetCountryConfigFacade();
                            DataConfigFacade dcf = country.GetDataConfigFacade();

                            List<CountryConfig.SystemRow> delSysRows = new List<CountryConfig.SystemRow>();
                            List<DataConfig.DBSystemConfigRow> delDbsRows = new List<DataConfig.DBSystemConfigRow>();
                            foreach (CountryConfig.SystemRow sysRow in ccf.GetSystemRows())
                                if (!projectContent.selectedYears.Contains(sysRow.Year))
                                { delSysRows.Add(sysRow); delDbsRows.AddRange(dcf.GetDBSystemConfigRowsBySystem(sysRow.Name)); }
                            foreach (CountryConfig.SystemRow delSysRow in delSysRows) delSysRow.Delete();
                            foreach (DataConfig.DBSystemConfigRow delDbsRow in delDbsRows) delDbsRow.Delete();
                            ccf.GetCountryConfig().AcceptChanges(); dcf.GetDataConfig().AcceptChanges();
                            country.WriteXML();
                        }
                        catch (Exception exception) { failure.Add($"{country._shortName}: adapting years failed", exception.Message); }
                    }
                }

                //Logout from version control
                if (IsLoggedIn() && _vcAPI != null)
                {
                    if (IsProjectVersionControlled()) HandleButtonDisConnectClicked(false);  // if connected, disconnect first!
                    if (_vcAPI.isLoggedIn) _vcAPI.Logout();
                    EM_AppContext.Instance.SetState_btnVCLogInOut(false);
                    EM_AppContext.Instance.AllCountryMainForm_SetSetVCButtonsGreyState();
                }
                caller.Cursor = Cursors.WaitCursor;

                EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //only the empty main form is open and must be updated
                EM_AppContext.Instance.UpdateMainFormCaption(); //set title (of single open mainform) to "EUROMOD Version (Path)"

                caller.Cursor = Cursors.WaitCursor;

                if (failure.Count == 0) UserInfoHandler.ShowSuccess($"Successfully generate the new project at '{euromodFolder}'.");
                else ReportSuccessFailure("Generating new project", success, failure);

                caller.Cursor = Cursors.Default;

                bool CopyInputOrLog(bool log, out string errorMessage)
                {
                    try
                    {
                        string src = log ? new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderLog() : new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderInput();
                        string dest = log ? new EMPath(euromodFolder).GetFolderLog() : new EMPath(euromodFolder).GetFolderInput();
                        foreach (FileInfo fi in new DirectoryInfo(src).GetFiles()) File.Copy(fi.FullName, Path.Combine(dest, fi.Name));
                        errorMessage = null; return true;
                    }
                    catch (Exception exception) { errorMessage = exception.Message; return false; }
                }
            }catch(Exception ex)
            {
                EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                UserInfoHandler.ShowError("An error has ocurred: " + exceptionMessage);
            }
        }

        void NewRelease_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;

            VCNewProject.ProjectContent projectContent = e.Argument as VCNewProject.ProjectContent;

            int allUnits = projectContent.selections.Count(x => x == true);
            int done = 0;
            //download the release, following the users instructions on 'get', 'keep', 'merge'
            string success = string.Empty;
            Dictionary<string, string> failure = new Dictionary<string, string>();

//            System.Threading.Tasks.Parallel.For(0, units.Count, index =>
            for (int index = 0; index < projectContent.selections.Count; index++)
            {
                if (!projectContent.selections[index]) continue;
                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; }

                if (_vcAPI.GET(projectContent.projectUnits[index], projectContent.selectedRelease, "", "", true))
                {
                    ReleasePossiblyLoadedVersion(projectContent.projectUnits[index]);
                    AddUnitToMessage(ref success, projectContent.projectUnits[index].Name);
                }
                else failure.Add(projectContent.projectUnits[index].Name, _vcAPI.GetErrorMessage());

                done++;
                backgroundWorker.ReportProgress(Convert.ToInt32((done * 1.0) / (allUnits * 1.0) * 100.0));
//            });
            }
            backgroundWorker.ReportProgress(100);

            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //new countries could have been added
            e.Result = new object[] { success, failure };
        }


        internal void HandleButtonVCSettingsClicked()
        {
            VCSettings vcSettingsDialog = new VCSettings();
            vcSettingsDialog.ShowDialog();
        }

        #endregion HANDLE...FUNCTIONS

        #region HELPER FUNCTIONS

        static string GetConflictFileName(string unitName, VCAPI.VC_FOLDER_TYPE unitType) 
        {
            switch (unitType)
            {
                case VCAPI.VC_FOLDER_TYPE.CONFIG: return EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) + "VCConflict.txt";
                case VCAPI.VC_FOLDER_TYPE.COUNTRY: return EMPath.AddSlash(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + unitName) + "VCConflict.txt";
                case VCAPI.VC_FOLDER_TYPE.ADDON: return EMPath.AddSlash(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + unitName) + "VCConflict.txt";
                default: return string.Empty; //just necessary for compiler
            }
        }

        static string GetUnitName(bool variables) { return GetVCUnitName(EM_AppContext.Instance.GetActiveCountryMainForm().GetCountryShortName(), variables); }
        static string GetVCUnitName(string unitName, bool variables)
        {
            if (!variables) return unitName;
            string varConfigName = EMPath.EM2_FILE_VARS;
            if (varConfigName.EndsWith(".xml")) varConfigName = varConfigName.Substring(0, varConfigName.Length - ".xml".Length);
            return varConfigName;
        }


        internal static bool IsVarConfig(string unitName) { return unitName.ToLower() == Path.GetFileNameWithoutExtension(EMPath.EM2_FILE_VARS).ToLower(); }

        static void WriteConflictFile(string unitName, VCAPI.VC_FOLDER_TYPE unitType, string state, string commitId)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(GetConflictFileName(unitName, unitType)))
                {
                    writer.WriteLine(state);
                    writer.WriteLine(commitId);
                    writer.Close();
                }
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Writing conflict-info-file failed.", false); //actually hooe that this doesn't happen
            }
        }

        static void ReadConflictFile(string unitName, VCAPI.VC_FOLDER_TYPE unitType, out string state, out string commitId)
        {
            try
            {
                using (StreamReader reader = new StreamReader(GetConflictFileName(unitName, unitType)))
                {
                    state = reader.ReadLine();
                    commitId = reader.ReadLine();
                }
            }
            catch (Exception exception)
            {
                state = commitId = string.Empty;
                UserInfoHandler.ShowException(exception, "Reading conflict-info-file failed.", false); //actually hope that this doesn't happen
            }
        }

        internal static bool IsConflictOpen(string unitName, VCAPI.VC_FOLDER_TYPE unitType)
        {
            if (!File.Exists(GetConflictFileName(unitName, unitType))) return false;
            string state, commitId; ReadConflictFile(unitName, unitType, out state, out commitId);
            return state == OPEN_CONFLICT;
        }

        internal static void DeleteConflictFile(string unitName, VCAPI.VC_FOLDER_TYPE unitType)
        { //note: called, amongst others, by the MergeAdministrator when the merge-folder is deleted
            try
            {
                if (File.Exists(GetConflictFileName(unitName, unitType)))
                    File.Delete(GetConflictFileName(unitName, unitType));
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Deleting conflict-info-file failed.", false); //actually hope that this doesn't happen
            }
        }

        bool GenerateMergeEnvironement(VersionControlUnitInfo unitInfo, string conflictedCommitId, string commonParentCommitId)
        { string dummy; return GenerateMergeEnvironement(unitInfo, conflictedCommitId, commonParentCommitId, false, out dummy); }
        bool GenerateMergeEnvironement(VersionControlUnitInfo unitInfo, string mergeCommitId, string commonParentCommitId, bool forDownloadRelease, out string errMessage)
        {
            const string tempConflict = "tempConflict"; errMessage = string.Empty;
            if (!_vcAPI.GET(unitInfo, mergeCommitId, tempConflict,"", false, true) || //get version to merge, e.g. as 'tempConflictUU.xml'
                !_vcAPI.GET(unitInfo, commonParentCommitId, tempConflict + "Parent","", false, true)) //get common parent version, e.g. as 'tempConflictParentUU.xml'
            {
                errMessage = string.Format("Preparing merge-environement failed:{0}{1}", forDownloadRelease ? " " : Environment.NewLine, _vcAPI.GetErrorMessage());
                if (!forDownloadRelease) UserInfoHandler.ShowError(errMessage);
                return false;
            }

            string path = unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG ? EMPath.Folder_Config(EM_AppContext.FolderEuromodFiles) :
                          unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.COUNTRY ? EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + unitInfo.Name
                                                                     : EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + unitInfo.Name;
            MergeAdministrator mergeAdministrator = forDownloadRelease //need a different initialisation of the MergeAdmin if conflict-merge-environement (=MainForm open) and downloading release (just empty MainForm open)
                ? new MergeAdministrator(unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG, unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.ADDON, unitInfo.Name, unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG ? unitInfo.Name + ".xml" : "")
                : new MergeAdministrator(EM_AppContext.Instance.GetActiveCountryMainForm(), unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG, unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG ? unitInfo.Name + ".xml" : "");
            if (!mergeAdministrator.GenerateMergeFileStructure(path, tempConflict + (unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG ? ".xml" : string.Empty),
                                                               path, tempConflict + (unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG ? "Parent.xml" : "Parent"), false, false))
                return false;

            foreach (FileInfo f in new DirectoryInfo(path).GetFiles(tempConflict + "*")) f.Delete();

            if (forDownloadRelease) return true;

            WriteConflictFile(unitInfo.Name, unitInfo.UnitType, OPEN_CONFLICT, mergeCommitId);

            if (IsVarConfig(unitInfo.Name)) mergeAdministrator.ShowVariablesMergeDialog(); else mergeAdministrator.ShowMergeDialog();
            return true;
        }

        internal void CheckForUploading(string unitName, VCAPI.VC_FOLDER_TYPE unitType) //called after successful ApplyAcceptedChanges of MergeAdministrator
        {
            if (!File.Exists(GetConflictFileName(unitName, unitType)))
                return; //probably some other merging, not invoked by version control

            string state, commitId; ReadConflictFile(unitName, unitType, out state, out commitId);
            WriteConflictFile(unitName, unitType, RESOLVED_CONFLICT, commitId); //update the conflict-info-file to "conflict is resolved" (actually unnecessary if the version is immediately uploaded and thus the conflict-info deleted)

            if (UserInfoHandler.GetInfo("Should the now consolidated version be uploaded?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (Upload(_vcAPI.vc_projectInfo.GetUnit(unitName, unitType))) DeleteConflictFile(unitName, unitType); //conflict file is not needed anymore
            }
        }

        internal static bool ReportSuccessFailure(string vcAction, string success, Dictionary<string, string> failure)
        {
            string message = string.Empty;
            if (success != string.Empty)
                message += string.Format("{0} succeeded for {1}.", vcAction, success);
            if (failure.Count != 0)
            {
                if (message != string.Empty) message += Environment.NewLine + Environment.NewLine;
                message += string.Format("{0} failed with the following error{1}:", vcAction, failure.Count == 1 ? string.Empty : "s") + Environment.NewLine;
                foreach (string unit in failure.Keys) message += string.Format("{0}: {1}", unit, failure[unit]) + Environment.NewLine;
            }

            if (failure.Count == 0) UserInfoHandler.ShowSuccess(message);
            else if (success != string.Empty) UserInfoHandler.ShowError(message);
            else UserInfoHandler.ShowInfo(message);

            return failure.Count == 0;
        }

        bool GetRemoteUnit(string unitName, VCAPI.VC_FOLDER_TYPE unitType, out VersionControlUnitInfo unit, out string errorMessage)
        {
            List<VersionControlUnitInfo> units; unit = null; errorMessage = string.Empty;
            if (!_vcAPI.GetRemoteUnits(_vcAPI.vc_projectInfo.ProjectId, out units))
            {
                errorMessage = _vcAPI.GetErrorMessage();
                return false;
            }
            foreach (VersionControlUnitInfo u in units)
                if (u.Name.ToLower() == unitName.ToLower() &&
                    u.UnitType == unitType) { unit = u; return true; }
            errorMessage = "Unit not found in remote VC.";
            return false;
        }

        internal static void AddUnitToMessage(ref string message, string unitName)
        {
            if (message != string.Empty) message += ", "; message += "'" + unitName + "'";
        }

        static bool CloseIfAnythingOpen(string action)
        {
            if (!EM_AppContext.Instance.IsAnythingOpen()) return true;
            if (UserInfoHandler.GetInfo(action + " requires all countries, add-ons and the variables-form to be closed."
                + Environment.NewLine + "All open instances will be closed.", MessageBoxButtons.OKCancel) == DialogResult.Cancel) return false;
            EM_AppContext.Instance.CloseAllMainForms(false);
            return true;
        }

        internal static List<VersionControlUnitInfo> GetContent_LocalProject(string projectPath = "")
        {
            EMPath emPath = new EMPath(EM_AppContext.FolderEuromodFiles);
            string folderCountries = EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles);
            string folderAddOns = EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles);
            string fileVarConfig = emPath.GetVarFilePath(true);
            string fileHICPConfig = emPath.GetHICPFilePath(true);
            string fileExchangeRatesConfig = emPath.GetExRatesFilePath(true);
            string fileSwitchablePolicyConfig = emPath.GetExtensionsFilePath(true);
            string fileLog = emPath.GetEmLogFilePath();
            // string folderInput = PathsHelper.Folder_Input;
            string folderInput = new EMPath(EM_AppContext.FolderEuromodFiles).GetFolderInput(); // make sure you get the Input folder within the project and not the one actually used!!
            if (projectPath != string.Empty)
            {
                folderCountries = emPath.Folder_AtAlternativeEMPath(folderCountries, projectPath);
                folderAddOns = emPath.Folder_AtAlternativeEMPath(folderAddOns, projectPath);
                fileVarConfig = emPath.File_AtAlternativeEMPath(fileVarConfig, projectPath);
                fileHICPConfig = emPath.File_AtAlternativeEMPath(fileHICPConfig, projectPath);
                fileExchangeRatesConfig = emPath.File_AtAlternativeEMPath(fileExchangeRatesConfig, projectPath);
                fileSwitchablePolicyConfig = emPath.File_AtAlternativeEMPath(fileSwitchablePolicyConfig, projectPath);
                fileLog = emPath.File_AtAlternativeEMPath(fileLog, projectPath);
                folderInput = emPath.File_AtAlternativeEMPath(folderInput, projectPath);
            }
            List<VersionControlUnitInfo> units = new List<VersionControlUnitInfo>();
            long tempUnitId = -1;

            if (Directory.Exists(folderCountries))
                foreach (DirectoryInfo countryFolder in (new DirectoryInfo(folderCountries)).GetDirectories())
                {
                    if (File.Exists(EMPath.AddSlash(countryFolder.FullName) + Country.GetCountryXMLFileName(countryFolder.Name)) &&
                        File.Exists(EMPath.AddSlash(countryFolder.FullName) + Country.GetDataConfigXMLFileName(countryFolder.Name)))
                        units.Add(new VersionControlUnitInfo
                        {
                            UnitId = tempUnitId++,
                            Name = countryFolder.Name,
                            UnitType = VCAPI.VC_FOLDER_TYPE.COUNTRY
                        });
                }
            if (Directory.Exists(folderAddOns))
                foreach (DirectoryInfo addOnFolder in (new DirectoryInfo(folderAddOns)).GetDirectories())
                {
                    if (File.Exists(EMPath.AddSlash(addOnFolder.FullName) + Country.GetCountryXMLFileName(addOnFolder.Name)))
                        units.Add(new VersionControlUnitInfo
                        {
                            UnitId = tempUnitId++,
                            Name = addOnFolder.Name,
                            UnitType = VCAPI.VC_FOLDER_TYPE.ADDON
                        });
                }
            if (File.Exists(fileVarConfig))
                units.Add(new VersionControlUnitInfo
                {
                    UnitId = tempUnitId++,
                    Name = GetVCUnitName(string.Empty, true),
                    UnitType = VCAPI.VC_FOLDER_TYPE.CONFIG
                });
            if (File.Exists(fileHICPConfig))
                units.Add(new VersionControlUnitInfo
                {
                    UnitId = tempUnitId++,
                    Name = Path.GetFileNameWithoutExtension(EMPath.EM2_FILE_HICP),
                    UnitType = VCAPI.VC_FOLDER_TYPE.CONFIG
                });
            if (File.Exists(fileExchangeRatesConfig))
                units.Add(new VersionControlUnitInfo
                {
                    UnitId = tempUnitId++,
                    Name = Path.GetFileNameWithoutExtension(EMPath.EM2_FILE_EXRATES),
                    UnitType = VCAPI.VC_FOLDER_TYPE.CONFIG
                });
            if (File.Exists(fileSwitchablePolicyConfig))
                units.Add(new VersionControlUnitInfo
                {
                    UnitId = tempUnitId++,
                    Name = Path.GetFileNameWithoutExtension(EMPath.EM2_FILE_EXTENSIONS),
                    UnitType = VCAPI.VC_FOLDER_TYPE.CONFIG
                });
            if (File.Exists(fileLog))
                units.Add(new VersionControlUnitInfo
                {
                    UnitId = tempUnitId++,
                    Name = Path.GetFileNameWithoutExtension(EMPath.FILE_EMLOG),
                    UnitType = VCAPI.VC_FOLDER_TYPE.LOG
                });
            if (Directory.Exists(folderInput))
                units.Add(new VersionControlUnitInfo
                {
                    UnitId = tempUnitId++,
                    Name = EMPath.FOLDER_INPUT,
                    UnitType = VCAPI.VC_FOLDER_TYPE.INPUT

                });
            return units;
        }

        #endregion HELPER FUNCTIONS

        #region API REQUEST FUNCTIONS

        internal bool IsProjectVersionControlled() { bool yn; _vcAPI.IsVersionControlled(out yn); return yn; }

        internal bool IsLoggedIn() { return _vcAPI != null && _vcAPI.isLoggedIn; }

        internal string GetProjectName() { return _vcAPI.vc_projectInfo.ProjectName; }

        #endregion API REQUEST FUNCTIONS

        #region API ACTION FUNCTIONS

        internal void ReBoot()
        {
            if (_vcAPI != null) _vcAPI.Logout();
            _vcAPI = new VCAPI(EM_AppContext.FolderEuromodFiles);
        }

        internal bool LogIn(string insertedUserNameOrEmail, string password, out string userName)
        {
            if (_vcAPI != null) _vcAPI.Logout();

            if(_vcAPI.VCLogIn(insertedUserNameOrEmail, password, out userName)) { return true; }
            else
            {
                UserInfoHandler.ShowError("Establishing connection to VC-server failed with the following error:" + Environment.NewLine + _vcAPI.GetErrorMessage());
                return false;
            }
        }

        #region proxy

        internal Boolean checkProxyCredentials()
        {
           return _vcAPI.checkProxyCredentials();
        }

        internal void setProxyCredentials(string proxyUserName, string proxyPassword)
        {

            _vcAPI.setProxyCredentials(proxyUserName, proxyPassword);


        }
        internal void setProxyConfiguration(string proxyURL, string proxyPort, bool useProxy)
        {

            _vcAPI.setProxyConfiguration(proxyURL, proxyPort, useProxy);


        }
        #endregion proxy

        public void setServerConfiguration(string serverURL, string serverName, string orgName, string encryptKey, string authorName, string encryptedUserName, out string userName, 
            string encryptedPassword, out string password)
        {
            _vcAPI.setServerConfiguration(serverURL, serverName, orgName, encryptKey, authorName, encryptedUserName, out userName, encryptedPassword, out password);
        }

        public bool updateUserNameToXMLFile(string userName, string organizationID)
        {
            return _vcAPI.updateUserNameToXMLFile(userName, organizationID);

        }

        public bool updatePasswordToXMLFile(string password, string organizationID)
        {
            return _vcAPI.updatePasswordToXMLFile(password, organizationID);
        }


        void MultiUpload_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;
            List<object> arguments = e.Argument as List<object>;
            List<VersionControlUnitInfo> unitInfos = arguments.ElementAt(0) as List<VersionControlUnitInfo>;
            List<string> stati = arguments.ElementAt(1) as List<string>;
            string version = arguments.ElementAt(2) as string;
            long projectId = -1;

            List<VersionControlUnitInfo> successfulUploads = new List<VersionControlUnitInfo>();
            string success = string.Empty;
            Dictionary<string, string> failure = new Dictionary<string, string>();
            backgroundWorker.ReportProgress(3);
            for (int i = 0; i < unitInfos.Count; ++i)
            {
                projectId = unitInfos[i].ProjectId;
                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; }

                bool update = false;
                if (stati[i] == VC_STATUS_MODIFIED) { update = true; }
                string comment = _vcAPI.UpdateComment(unitInfos[i], version);
                if(stati[i] == VC_STATUS_NOTINBUNDLE || _vcAPI.PUSH(unitInfos[i], update))
                {
                    if (stati[i] == VC_STATUS_NOTINBUNDLE)
                    {
                        comment = _vcAPI.UpdateComment(unitInfos[i], version);
                        if (_vcAPI.PUSH(unitInfos[i], true))
                        {
                            AddUnitToMessage(ref success, unitInfos[i].Name);
                            successfulUploads.Add(unitInfos[i]);
                        }
                        else
                        {
                            failure.Add(unitInfos[i].Name, _vcAPI.GetErrorMessage());
                        }
                    }
                    else
                    {
                        if (stati[i] == VC_STATUS_CONFLICT_RESOLVED) DeleteConflictFile(unitInfos[i].Name, unitInfos[i].UnitType);
                        AddUnitToMessage(ref success, unitInfos[i].Name);
                        successfulUploads.Add(unitInfos[i]);
                    }
                }
                else failure.Add(unitInfos[i].Name, _vcAPI.GetErrorMessage());

                backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (unitInfos.Count * 1.0) * 100.0));
            }


            backgroundWorker.ReportProgress(100);
            e.Result = new object[] { successfulUploads, success, failure };

        }

        internal List<VersionControlUnitInfo> MultiUpload(List<VersionControlUnitInfo> unitInfos, List<string> stati, string version)
        {
            List<object> arguments = new List<object> { unitInfos, stati, version };
            ProgressIndicator progressIndicator = new ProgressIndicator(MultiUpload_BackgroundEventHandler, "Uploading ...", arguments);
            DialogResult res = progressIndicator.ShowDialog();
            object[] results = (object[])progressIndicator.Result;
            if (results == null)
            {
                UserInfoHandler.ShowError("User cancelled the Upload.");
            }
            else
            {
                ReportSuccessFailure("Upload", (string)results[1], (Dictionary<string, string>)results[2]);
            }
            return (res == DialogResult.Cancel) ? new List<VersionControlUnitInfo>() : results[0] as List<VersionControlUnitInfo>;
        }

        static void ReleasePossiblyLoadedVersion(VersionControlUnitInfo unitInfo)
        {
            if (unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.COUNTRY || unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.ADDON)
                CountryAdministrator.ResetConfigFacades(unitInfo.Name); //to release any possibly loaded countries or add/ons (if they were open and closed before the download)
            if (unitInfo.UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG)
                EM_AppContext.Instance.ReleaseVarConfigFacade(); //release possibly loaded VarConfig (see above)
        }

        internal bool Upload(VersionControlUnitInfo unitInfo)
        {
            string info = string.Empty;
            if (UserInput.Get(new UserInput.Item("id", "Please insert an upload info:") { AllowEmpty = true, LineCount = 3, Width = 400, MaxTextLength = 200 },
                out info) == DialogResult.Cancel) //note: maxLength is restricted to 200, as LogicalDoc documents allow for the title only about 250 characters
                    return false;

            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.WaitCursor;
            if (!_vcAPI.PUSH(unitInfo))
                UserInfoHandler.ShowError("Upload failed with the following error:" + Environment.NewLine + _vcAPI.GetErrorMessage());
            else
                UserInfoHandler.ShowSuccess("Upload succeeded!");
            EM_AppContext.Instance.GetActiveCountryMainForm().Cursor = Cursors.Default;

            return true;
        }



        void DownloadRelease_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker;

            Tuple<long, string, List<VersionControlUnitInfo>, List<VCDownloadRelease.DownloadAction>> arguments = e.Argument as Tuple<long, string, List<VersionControlUnitInfo>, List<VCDownloadRelease.DownloadAction>>;
            long projectId = arguments.Item1;
            string releaseVersion = arguments.Item2;
            List<VersionControlUnitInfo> unitInfos = arguments.Item3;
            List<VCDownloadRelease.DownloadAction> downloadActions = arguments.Item4;

            //download the release, following the users instructions on 'get', 'keep', 'merge'
            string success = string.Empty;
            Dictionary<string, string> failure = new Dictionary<string, string>();

            for (int i = 0; i < downloadActions.Count; ++i)
            {
                if (backgroundWorker.CancellationPending) { e.Cancel = true; break; }
                switch (downloadActions[i])
                {   //keep the UI-version
                    case VCDownloadRelease.DownloadAction.noAction: continue;
                    
                    //download the Release-version
                    case VCDownloadRelease.DownloadAction.getReleaseVersion:
                        
                        (new MergeAdministrator(unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG, unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.ADDON, unitInfos[i].Name, unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG? unitInfos[i].Name+ ".xml" :"")).DeleteMergeFolder(true, true);
                        

                        if (_vcAPI.GET(unitInfos[i], releaseVersion,
                            //if the download is from another project or the project is not VC-controlled, the local VC-info (VersionControl.xml) should/can not be updated ...
                            (!IsProjectVersionControlled() || projectId != _vcAPI.vc_projectInfo.ProjectId) ? unitInfos[i].Name : string.Empty)) //... indicating the name prevents from this
                        {
                            ReleasePossiblyLoadedVersion(unitInfos[i]);
                            AddUnitToMessage(ref success, unitInfos[i].Name);
                        }
                        else failure.Add(unitInfos[i].Name, _vcAPI.GetErrorMessage());
                        break;

                    //download the Relase-version but provide merge-environment to consolidate with the UI-version
                    case VCDownloadRelease.DownloadAction.getMergeSupport: //merging does not make sense, if the unit does not exist locally (what would be merged?) ...
                        (new MergeAdministrator(unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG, unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.ADDON, unitInfos[i].Name, unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG ? unitInfos[i].Name + ".xml" : "")).DeleteMergeFolder(true, true);
                        if ((unitInfos[i].UnitType != VCAPI.VC_FOLDER_TYPE.CONFIG && !CountryAdministrator.DoesCountryExist(unitInfos[i].Name)) ||
                            (unitInfos[i].UnitType == VCAPI.VC_FOLDER_TYPE.CONFIG && !File.Exists(new EMPath(EM_AppContext.FolderEuromodFiles).GetAnyConfigFilePath(unitInfos[i].Name + ".xml", true))))
                        { downloadActions[i] = VCDownloadRelease.DownloadAction.getReleaseVersion; --i; continue; } //... therefore change action to simple download

                        
                        string errMessage;
                        if (GenerateMergeEnvironement(unitInfos[i], releaseVersion, releaseVersion, true, out errMessage))
                            AddUnitToMessage(ref success, unitInfos[i].Name + "+merge-environment");
                        else failure.Add(unitInfos[i].Name, errMessage);
                        
                        break;
                }
                backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (downloadActions.Count * 1.0) * 100.0));
            }
            backgroundWorker.ReportProgress(100);

            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //new countries could have been added
            e.Result = new object[] { success, failure };
        }

        #endregion API ACTION FUNCTIONS
        
        #region NOTIFICATIONS

        #endregion NOTIFICATIONS

    }
}
