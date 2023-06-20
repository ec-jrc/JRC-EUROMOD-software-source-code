using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using Ionic.Zip;
using System.Net;
using Octokit;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Net.Http;
using System.Threading.Tasks;
using Octokit.Internal;
using EM_XmlHandler.VersionControl;
using EM_Crypt;

namespace VCUIAPI
{
    /// <summary>
    /// A class that provides a connection between the EUROMOD UI and the online repository
    /// </summary>
    public class VCAPI : VCAPIInterface
    {
        #region constants
        /***************************************\
        |*          Private Constants           |
        \***************************************/
        private const string FILENAME_VC_INFO = "VersionControl.xml";
        private const string INFO_TYPE_COMMITMD5HASH = "[MD5Hash]";
        private const string INFO_TYPE_PARENTVERSION = "[parentVersion]";
        private const string INFO_TYPE_COMMENT = "[comment]";
        private const string INFO_TYPE_UNITID = "[unitId]";
        private const string FOLDER_TYPE_COUNTRY = "[country]";
        private const string FOLDER_TYPE_ADDON = "[addon]";
        private const string FOLDER_TYPE_CONFIG = "[config]";
        private const string FOLDER_TYPE_PROJECT = "[project]";
        private const string FOLDER_TYPE_LOG = "[log]";
        private const string FOLDER_TYPE_INPUT = "[input]";
        private const string FILE_EXTENTION_MAIN = ".xml";
        private const string FILE_EXTENTION_DATACONFIG = "_DataConfig.xml";
        private const string FILE_EXTENTION_ICON = ".png";
        private const string FILE_EXTENTION_ZIP = ".zip";
        private const string FILE_EXTENTION_LOG = ".xlsx";
        private const string FOLDER_PATH_XMLPARAM = "XMLParam";
        private const string FOLDER_PATH_COUNTRIES = "Countries";
        private const string FOLDER_PATH_ADDONS = "AddOns";
        private const string FOLDER_PATH_CONFIG = "Config";
        private const string FOLDER_PATH_LOG = "Log";
        private const string FOLDER_PATH_INPUT = "Input";
        private const string PROXY_ERROR = "407";
        private const string INITIAL_FILE_NAME = "initial_file.txt";


        private const char MARKER_RELEASE_SPLIT_1 = '#';
        private const char MARKER_RELEASE_SPLIT_2 = '$';
        private readonly string[] ALLOWED_INPUT_FILES = { "PPP.txt", "training_data.txt", "sl_demo_v4.txt", "DRD_sl_demo_v4.xls", "DRD_training_data.xls" };

        private enum VC_ACTIONS { CREATE_REPOSITORY, CREATE_MERGE_FILE, DELETE_MERGE_FILE }
        private const long MASTER_VERSION_ID = 367;


        /***************************************\
        |*          Internal Constants          |
        \***************************************/
        internal const string FOLDER_NAME_CORE_PROJECTS = "EUROMOD_PROJECTS";
        internal const string FOLDER_NAME_USER_PROJECTS = "USER_PROJECTS";
        internal const long DEFAULT_WORKSPACE = 4;

        /***************************************\
        |*           Public Constants           |
        \***************************************/

        /// <summary>
        /// The enumeration that defines access rights.
        /// UNDEFINED: The user is not even a member of this project
        /// NONE: The user has no access rights on this project
        /// DOWNLOAD: The user has Read access rights to this project (view project & download commits)
        /// UPLOAD: The user has both Read & Write acess rights to this project (view project, download commits and add new commits)
        /// </summary>
        public enum VC_ACCESS_RIGHTS { UNDEFINED = -1, NONE = 0, DOWNLOAD = 1, UPLOAD = 2 };
        /// <summary>
        /// The enumeration to distinguish between different folder types on LogicalDoc.
        /// </summary>
        public enum VC_FOLDER_TYPE { UNDEFINED, COUNTRY, ADDON, CONFIG, PROJECT, FOLDER, TYPEFOLDER, INPUT, LOG };
        /// <summary>
        /// The enum to distinguish between notification types.
        /// </summary>
        public enum VC_NOTIFICATION_TYPE { UNDEFINED, START_MERGE, FINISH_MERGE, ABORT_MERGE };

        #endregion constants

        #region variables
        /***************************************\
        |*           Public Variables           |
        \***************************************/
        /// <summary>
        /// The variable that holds the currently loaded project.
        /// </summary>
        public VersionControlProjectInfo vc_projectInfo;
        /// <summary>
        /// This variable will store the path of the local project when creating a new bundle
        /// </summary>
        public string _localBundlePath = String.Empty;
        /// <summary>
        /// This variable will be true if the latest version has just been downloaded
        /// </summary>
        public bool _isLatestBundle;
        ////
        /// <summary>
        /// Get the initialization state of the Version Control system.
        /// </summary>
        public bool isInitialized { get { return _isInitialized; } }
        /// <summary>
        /// Get the login state of the Version Control system.
        /// </summary>
        public bool isLoggedIn { get { return _isLoggedIn; } }
        /// <summary>
        /// Get the merging state of the Version Control system.
        /// </summary>
        public bool isCurrentlyMerging { get { return _userCurrentlyMerging != string.Empty; } }
        /// <summary>
        /// Get the merging state of the Version Control system.
        /// </summary>
        public bool isCurrentUserMerging { get { return _userCurrentlyMerging != string.Empty && _userCurrentlyMerging == currentUser.username; } }
        /// <summary>
        /// Get the username of the person currently merging.
        /// </summary>
        public string userCurrentlyMerging { get { return _userCurrentlyMerging; } }
        /// <summary>
        /// Get is proxy has been detected or not
        /// </summary>
        public bool useProxy { get { return _useProxy; } }
        /// <summary>
        /// Get the proxy username
        /// </summary>
        public string proxyUserName { get { return _proxyUserName; } }
        /// <summary>
        /// Get the proxy password
        /// </summary>
        public string proxyPassword { get { return _proxyPassword; } }
        /// <summary>
        /// Client that will be used to connect to GitHub
        /// </summary>
        private static GitHubClient _client;


        /***************************************\
        |*          Private Variables           |
        \***************************************/
        /// <summary>
        /// Variable that holds the error message of the current error.
        /// </summary>
        private string errorMessage = "";
        /// <summary>
        /// Variable that holds the path of the currently loaded project.
        /// </summary>
        private string projectPath = string.Empty;
        /// <summary>
        /// Variable that specifies whether the VC is initialized.
        /// </summary>
        private readonly bool _isInitialized = false;
        /// <summary>
        /// Variable that specifies whether a user is logged in.
        /// </summary>
        private bool _isLoggedIn = false;
        /// <summary>
        /// Variable that specifies the name of the user currently merging.
        /// </summary>
        private string _userCurrentlyMerging = string.Empty;
        /// <summary>
        /// Variable that stores in there is an existing bundle for the current VC project
        /// </summary>
        private bool _existAnyBundle = false;
        /// <summary>
        /// Variable that holds the current user information.
        /// </summary>
        private UserInfo currentUser;
        /// <summary>
        /// Variable that stores if the current user is admin for the current project
        /// </summary>
        private bool _isCurrentUserCurrentProjectAdmin = false;
        /// <summary>
        /// Variable that stores the rights for the current user in the current project
        /// </summary>
        private VC_ACCESS_RIGHTS _rightsCurrentUserCurrentProject = VC_ACCESS_RIGHTS.NONE;
        /// <summary>
        /// Variable that holds the VC project tree.
        /// </summary>
        private ProjectNode projectTree;
        ///<summary>
        /// Variable that specifies whether the proxy configuration should be used.
        /// </summary>
        private static bool _useProxy;
        /// <summary>
        /// Variable that stores the proxy username.
        /// </summary>
        private static string _proxyUserName;
        /// <summary>
        /// Variable that stores the proxy password.
        /// </summary>
        private static string _proxyPassword;
        /// <summary>
        /// Variable that stores the proxy address.
        /// </summary>
        private static string _proxyURL = "";
        /// <summary>
        /// Variable that stores the proxy port.
        /// </summary>
        private static string _proxyPort = "";
        ///<summary>
        ///Variable that stores the gitHub URL. 
        /// </summary>
        private static string _gitHubURL = string.Empty;
        /// <summary>
        /// Name of the organisation withing GitHub where all the projects will be stored
        /// </summary>
        //private static string _organisationName = "EUROMOD";
        private static string _organisationName = string.Empty;
        ///<summary>
        /// Name of the author that will be sent in the .zip file
        /// </summary>
        private static string _authorName = string.Empty;
        ///<summary>
        /// Key that will encrypt the content
        /// </summary>
        private static string _encryptKey = string.Empty;
        ///<summary>
        /// Key that will encrypt the .zip file
        /// </summary>
        private static string _encryptKey2 = "";
        ///<summary>
        /// <summary>
        /// Password that will encrypt the username
        /// </summary>
        private static string _encryptKeyUserName = "";

        /// <summary>
        /// UserName and password inserted by the user. Needed after a bundle is downloaded.
        /// </summary>
        private static string _vcUserName;
        private static string _vcPassword;
        #endregion variables


        /// <summary>
        /// Create a new Version Control instance, linked to a specific local project.
        /// </summary>
        /// <param name="pathToProject">The path of the local project on your hard drive.</param>
        public VCAPI(string pathToProject = "")
        {
            if (pathToProject == string.Empty) return;
            projectPath = pathToProject;
            _isInitialized = init();
        }

        public bool ChangePath(string newPath)
        {
            if (newPath == string.Empty || !Directory.Exists(newPath)) return false;
            projectPath = newPath;
            return true;
        }

        public string GetPath()
        {
            return projectPath;
        }


        public string getOldAddonPathLocalBundle(string shortName)
        {
            string variablePath = getLocalBundlePath() + "\\XMLParam\\AddOns\\" + shortName + ".xml";
            return variablePath;
        }

        public string getLocalBundlePath()
        {
            return _localBundlePath;
        }

        public void setLocalBundlePath(string localBundlePath)
        {
            _localBundlePath = localBundlePath;
        }

        public bool isThisLatestBundle()
        {
            return _isLatestBundle;
        }

        public void setIsThisLatestBundle(bool isLatestBundle)
        {
            _isLatestBundle = isLatestBundle;
        }

        public bool getIsCurrentUserCurrentProjectAdmin()
        {
            return _isCurrentUserCurrentProjectAdmin;
        }

        public VC_ACCESS_RIGHTS getRightsCurrentUserCurrentProject()
        {
            return _rightsCurrentUserCurrentProject;
        }

        public bool hasWritingPermissions()
        {
            return getRightsCurrentUserCurrentProject().Equals(VCUIAPI.VCAPI.VC_ACCESS_RIGHTS.UPLOAD);
        }

        /// <summary>
        /// Checks the existence of an online bundle for the current VC project
        /// </summary>
        /// <returns></returns>
        public bool checkExistAnyBundle(out String errorExistBundle)
        {
            bool existAnyBundle = false;
            errorExistBundle = string.Empty;

            List<ProjectNode> _projectInfos = new List<ProjectNode>();
            List<ReleaseInfo> _releaseInfos = new List<ReleaseInfo>();
            long projectId;
            string txtDestinationFolder = string.Empty;
            if (GetProjectList(out _projectInfos, false))
            {
                foreach (ProjectNode projectInfo in _projectInfos)
                {
                    if (projectInfo.Id == vc_projectInfo.ProjectId)
                    {
                        projectId = vc_projectInfo.ProjectId;
                        if (GetReleases(projectInfo.Id, out _releaseInfos))
                        {
                            if (_releaseInfos != null && _releaseInfos.Count() > 0)
                            {
                                existAnyBundle = true;

                            }
                        }
                        else
                        {
                            errorExistBundle = GetErrorMessage();
                        }
                    }
                }
            }

            _existAnyBundle = existAnyBundle;
            return existAnyBundle;
        }

        public void setExistAnyBundle(bool existAnyBundle)
        {
            _existAnyBundle = existAnyBundle;
        }

        public bool existAnyBundle()
        {
            return _existAnyBundle;
        }

        /// <summary>
        /// Initializes the VCAPI object.
        /// </summary>
        /// <returns>Return: true if object is initialized, false if call failed (error message can be accessed by GetErrorMessage)</returns>
        bool init()
        {
            try
            {
                vc_projectInfo = new VersionControlProjectInfo { isConnected = false };
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Initialization error: \n" + ex.Message);
                vc_projectInfo = new VersionControlProjectInfo { isConnected = false };
                return false;
            }
        }

        /// <summary>
        /// Retrieves the error provoked by the user’s most recent VC operation.
        /// </summary>
        public string GetErrorMessage()
        {
            return errorMessage;
        }


        /// <summary>
        /// Get a flat list of available online projects.
        /// </summary>
        /// <param name="projectList">Out: The list of available projects online.</param>
        /// <param name="forceRefresh">Specifies whether you want to get a fresh list or use the cached info.</param>
        /// <returns>Return: true if info is delivered, false if call failed (error message can be accessed by GetErrorMessage)</returns>
        public bool GetProjectList(out List<ProjectNode> projectList, bool forceRefresh = false)
        {
            projectList = new List<ProjectNode>();
            if (!_isLoggedIn)
            {
                errorMessage = "Not Logged in yet!";
                return false;
            }
            if (forceRefresh || projectTree == null) projectTree = GetProjectTree();
            if (projectTree == null)
            {
                errorMessage = "No projects available!";
                return false;
            }

            foreach (ProjectNode node in projectTree.GetFlatProjectList()) 
                if (node.Id== MASTER_VERSION_ID) projectList.Insert(0, node);
                else projectList.Add(node);


            return true;
        }

        /// <summary>
        /// Get the project tree. The optional parameters have been added because an error
        /// was happening if a project has just been removed and the project tree was refreshed.
        /// The project was still being obtained although it was empty
        /// </summary>
        /// <param name="justRemoved">optional bool that indicates if a project has just been removed.</param>
        /// <param name="projectIdJustRemoved">projectId of the project that has just been removed.</param>
        /// <returns></returns>
        private ProjectNode GetProjectTree(bool justRemoved = false, long projectIdJustRemoved = -1)
        {
            ProjectNode projectTree = null;

            //First, we create an empty projectTree
            projectTree = new ProjectNode(VC_FOLDER_TYPE.FOLDER, VCAPI.DEFAULT_WORKSPACE, "REPOSITORY_ROOT", -1, "The Repository Tree root", "", "");

            //Then, we add the repositories to the projectTree
            AddRepositoriesToProjectTree(projectTree, justRemoved, projectIdJustRemoved);

            return projectTree;
        }

        /// <summary>
        /// Populates the first level of the tree with the repositories
        /// </summary>
        /// <param name="children">All repositories the user has access to</param>
        /// <param name="proj">Parent project node</param>
        private void AddRepositoriesToProjectTree(ProjectNode parentNode, bool justRemoved, long projectIdJustRemoved)
        {
            //All repositories are obtained (for the current organisation)
            IReadOnlyList<Octokit.Repository> repoList = null;

            try
            {
                repoList = _client.Repository.GetAllForOrg(_organisationName).Result;
            }
            catch (Exception) { repoList = null; return; }

            if (repoList == null || repoList.Count == 0) { return; }

            foreach (Octokit.Repository repository in repoList)
            {
                //We need to make sure that the project has not been just removed
                if (!repository.Fork && !(justRemoved && projectIdJustRemoved == repository.Id)) {
                    string description = "";
                    if (repository.Description != null) { description = repository.Description; }

                    if (description.Contains(FOLDER_TYPE_PROJECT)) //If it is not a project, we ignore it
                    {
                        VC_FOLDER_TYPE t = VC_FOLDER_TYPE.PROJECT;
                        ProjectNode project = new ProjectNode(t, repository.Id, repository.Name, repository.Id, repository.Description, "", "");
                        parentNode.addChild(project);

                        AddProjectFolders(repository, project);

                    }
                }

            }
        }

        /// <summary>
        /// Add the container folders for the different elements(addons, countries, config, log...)
        /// </summary>
        /// <param name="repository">Octokit repository</param>
        /// <param name="proj">Parent project</param>
        private void AddProjectFolders(Octokit.Repository repository, ProjectNode proj)
        {
            //Before adding the folders, we are going to get the units
            //Each unit will be added to its folder
            List<VersionControlUnitInfo> releaseUnitInfos = null;
            GetLatestReleaseUnitInfo(repository.Id, out ReleaseInfo latestReleaseInfo);
            if (latestReleaseInfo != null) { releaseUnitInfos = GetReleaseUnitsInfo(repository.Id, latestReleaseInfo.Name); }

            foreach (string fp in new string[] { FOLDER_PATH_ADDONS, FOLDER_PATH_CONFIG, FOLDER_PATH_COUNTRIES, FOLDER_PATH_LOG, FOLDER_PATH_INPUT })
            {
                VC_FOLDER_TYPE t = new VC_FOLDER_TYPE();
                if (fp.Equals(FOLDER_PATH_COUNTRIES)) t = VC_FOLDER_TYPE.COUNTRY;
                else if (fp.Equals(FOLDER_PATH_ADDONS)) t = VC_FOLDER_TYPE.ADDON;
                else if (fp.Equals(FOLDER_PATH_CONFIG)) t = VC_FOLDER_TYPE.CONFIG;
                else if (fp.Equals(FOLDER_PATH_LOG)) t = VC_FOLDER_TYPE.LOG;
                else if (fp.Equals(FOLDER_PATH_INPUT)) t = VC_FOLDER_TYPE.INPUT;

                int tInt = (int)t;
                string newIdString = repository.Id.ToString() + tInt.ToString();
                long newId = Convert.ToInt64(newIdString);

                ProjectNode child = new ProjectNode(VC_FOLDER_TYPE.TYPEFOLDER, newId, fp, repository.Id, "", "", "");
                proj.addChild(child);
                AddProjectUnits(repository, child, t, newIdString, releaseUnitInfos);
            }
        }

        /// <summary>
        /// Get the information on all units contained in a certain folder
        /// </summary>
        /// <param name="repository">Octokit repository for the current unit</param>
        /// <param name="proj">Parent folder</param>
        /// <param name="t">Type of parent folder</param>
        /// <param name="parentId">ParentId</param>
        private void AddProjectUnits(Octokit.Repository repository, ProjectNode proj, VC_FOLDER_TYPE t, string parentId, List<VersionControlUnitInfo> releaseUnitInfos)
        {

            if (releaseUnitInfos != null && releaseUnitInfos.Count > 0)
            {
                foreach (VersionControlUnitInfo unitInfo in releaseUnitInfos)
                {
                    if (unitInfo.UnitType == t)
                    {
                        string unitName = unitInfo.Name;
                        long unitId = unitInfo.UnitId;
                        string hash = unitInfo.UnitHash;
                        string sha = unitInfo.Version;

                        ProjectNode child = new ProjectNode(t, unitId, unitName, Convert.ToInt64(parentId), "", hash, sha);
                        proj.addChild(child);
                    }

                }
            }

        }



        /// <summary>
        /// See if the current local project is linked to an online project.
        /// </summary>
        /// <param name="yesNo">Out: Whether the current project is linked to an online project.</param>
        /// <param name="unit">Optional: Name of country/add-on, if empty VC-availability is checked on project level.</param>
        /// <returns>Return: true if info is delivered, false if call failed (error message can be accessed by GetErrorMessage)</returns>
        public bool IsVersionControlled(out bool yesNo, string unit = "")
        {
            yesNo = false;
            if (vc_projectInfo == null) return true;    // no leaded project means that it is not version controlled...
            try
            {
                if (unit == "") yesNo = vc_projectInfo.isConnected;
                else
                {
                    yesNo = vc_projectInfo.isConnected && vc_projectInfo.Units.Exists(x => x.Name == unit);
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }

        public bool IsVersionControlled(string unit = "")
        {
            bool yesNo;
            return IsVersionControlled(out yesNo, unit) && yesNo;
        }

        /// <summary>
        /// Check if a local instance of the unit exists.
        /// </summary>
        /// <param name="yesNo">Out: Whether there is local instance.</param>
        /// <param name="unit"> Unit to check.</param>
        /// <returns>Return: true if info is delivered, false if call failed (error message can be accessed by GetErrorMessage)</returns>
        public bool DoesLocalUnitExist(out bool yesNo, VersionControlUnitInfo unit)
        {
            yesNo = false;
            try
            {
                if (unit.UnitType != VC_FOLDER_TYPE.ADDON && unit.UnitType != VC_FOLDER_TYPE.CONFIG && unit.UnitType != VC_FOLDER_TYPE.COUNTRY && unit.UnitType != VC_FOLDER_TYPE.LOG && unit.UnitType != VC_FOLDER_TYPE.INPUT)
                {
                    errorMessage = ("Invalid unit type in DoesLocalUnitExist.");
                    return false;
                }
                string path = getLocalPath(unit);

                if (unit.UnitType == VC_FOLDER_TYPE.LOG)
                {
                    yesNo = File.Exists(Path.Combine(path, unit.Name + FILE_EXTENTION_LOG));
                }
                else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
                {
                    yesNo = Directory.Exists(path);
                }
                else
                {
                    yesNo = File.Exists(Path.Combine(path, unit.Name + FILE_EXTENTION_MAIN));
                }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }


        /// <summary>
        /// Get all units from a release from the comments
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public List<VersionControlUnitInfo> GetReleaseUnitsInfo(long projectId, string tag)
        {
            try
            {
                Release release = _client.Repository.Release.Get(projectId, tag).Result;
                List<VersionControlUnitInfo> releaseUnitInfo = new List<VersionControlUnitInfo>();

                if (release != null)
                {
                    string releaseContent = release.Body;
                    if (releaseContent.EndsWith(MARKER_RELEASE_SPLIT_1.ToString())) releaseContent = releaseContent.Substring(0, releaseContent.Length - 1);
                    foreach (string releaseUnit in releaseContent.Split(MARKER_RELEASE_SPLIT_1))
                    {
                        string[] parts = releaseUnit.Split(MARKER_RELEASE_SPLIT_2);
                        if (parts.Count() != 5) continue;
                        string unitName = parts[0];
                        long unitId = long.Parse(parts[1]);
                        VC_FOLDER_TYPE unitType = getFolderType(parts[2]);
                        string unitHash = parts[3];
                        string unitSha = parts[4];

                        VersionControlUnitInfo unit = new VersionControlUnitInfo
                        {
                            UnitId = unitId,
                            ProjectId = projectId,
                            Name = unitName,
                            UnitHash = unitHash,
                            UnitType = unitType,
                            Version = unitSha
                        };

                        releaseUnitInfo.Add(unit);

                    }

                }

                return releaseUnitInfo;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Check if a local instance of the unit exists.
        /// </summary>
        /// <param name="unit"> Unit to check.</param>
        /// <returns>Return: true if unit exists, false if unit does not exist or call failed (error message can be accessed by GetErrorMessage)</returns>
        public bool DoesLocalUnitExist(VersionControlUnitInfo unit)
        {
            bool yesNo;
            return DoesLocalUnitExist(out yesNo, unit) && yesNo;
        }


        public bool CompareElements(out bool yesNo, VersionControlUnitInfo unit)
        {
            string currentHash = getUnitMD5Hash(unit);
            yesNo = currentHash.Equals(unit.UnitHash);
            return true;

        }

        /// <summary>
        /// LogIn using the username and password already used before
        /// It will be used after downloading the latest bundle
        /// </summary>
        /// <returns></returns>
        public bool VCLogInAfterDownloading()
        {
            string userName = string.Empty;
            if (!String.IsNullOrEmpty(_vcUserName) && !String.IsNullOrEmpty(_vcPassword))
            {
                return VCLogIn(_vcUserName, _vcPassword, out userName);
            }

            return false;
        }

        public bool VCLogIn(string insertedUserNameorEmail, string password, out string userName)
        {
            //Disable_CertificateValidation();
            GitHubClient client = null;

            //UriBuilder gitHubUri = new UriBuilder("https", _gitHubURL, 443);
            //All parameters are prepared for the connection
            ICredentialStore anonymousCredentials = new InMemoryCredentialStore(new Credentials(insertedUserNameorEmail, password));
            IJsonSerializer simpleJsonSerializer = new SimpleJsonSerializer();
            IWebProxy proxy = setProxyConfiguration();

            HttpClientAdapter httpClientAdapter = new Octokit.Internal.HttpClientAdapter(() => new HttpClientHandler
            {
                Proxy = proxy,
                UseProxy = useProxy,
            });

            httpClientAdapter.SetRequestTimeout(TimeSpan.FromSeconds(300));


            //The connection is configured
            var connection = new Connection(
                new ProductHeaderValue("test-app"),
                //gitHubUri.Uri,
                new Uri(_gitHubURL),
                anonymousCredentials,

                httpClientAdapter,
                simpleJsonSerializer);

            //The  client is obtained
            client = new GitHubClient(connection);
            //client = new GitHubClient(new ProductHeaderValue("test-app"), new Uri(_gitHubURL));
            //client.Credentials = new Credentials(insertedUserNameorEmail, password);

            //TODO PENDING To test the problem with the time out
            client.SetRequestTimeout(TimeSpan.FromSeconds(300));
            _client = client;

            //The user is obtained and userinfo is populated
            User user = null;
            UserInfo userInfo = null;
            userName = string.Empty;

            try
            {
                user = client.User.Current().Result;

                if (user != null)
                {
                    //If the user inserted an email, the username needs to be obtaine
                    userName = user.Login;

                    userInfo = new UserInfo();
                    userInfo.userId = user.Id;
                    userInfo.username = userName;

                    currentUser = userInfo;
                    _isLoggedIn = true;

                    _vcUserName = userName;
                    _vcPassword = password;

                    projectTree = GetProjectTree();

                    return projectTree != null;


                }
                return false;
            }
            catch (Exception e)
            {
                errorMessage = e.Message;

                if (e.InnerException != null && e.InnerException.Message != null)
                {
                    errorMessage = errorMessage + " - " + e.InnerException.Message;

                    if (e.InnerException.InnerException != null && e.InnerException.InnerException.Message != null)
                    {
                        errorMessage = errorMessage + " - " + e.InnerException.InnerException.Message;

                        if (e.InnerException.InnerException.Message.Contains("407"))
                        {
                            _proxyUserName = string.Empty; _proxyPassword = string.Empty;
                        }
                    }
                }

                _isLoggedIn = false;
                return false;
            }
        }

        /// <summary>
        /// Close the connection with the online Version Control.
        /// </summary>
        public void Logout()
        {
            // logout from any active online sessions
            _client = null;
            projectTree = null;
            _isLoggedIn = false;
            currentUser = null;
        }

        /// <summary>
        /// Get the name of the user who is merging
        /// </summary>
        /// <param name="mergingUser">Name of the user who is merging</param>
        /// <returns></returns>
        public bool GetMergingUser(out string mergingUser)
        {
            mergingUser = string.Empty;
            if (!_isLoggedIn)
            {
                errorMessage = "You need to be logged in to check if someone is merging!";
                return false;
            }
            if (!_isInitialized)
            {
                errorMessage = "You need to be connected to a project check if someone is merging!";
                return false;
            }
            try
            {
                IReadOnlyList<RepositoryContent> docs = _client.Repository.Content.GetAllContents(vc_projectInfo.ProjectId).Result;

                if (docs != null && docs.Count() > 0)
                {
                    foreach (RepositoryContent tf in docs)
                    {
                        if (tf.Name.StartsWith("merging"))
                        {
                            _userCurrentlyMerging = getUserNameFromMergeFileName(tf.Name);

                            mergingUser = _userCurrentlyMerging;
                        }

                    }
                    return true;
                }
                else
                {
                    _userCurrentlyMerging = string.Empty;
                    return true;
                }

            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");

                errorMessage = "Could not get Merge info:\n" + exceptionMessage;
                return false;
            }
        }

        /// <summary>
        /// Get the name of user who is merging from the txt file
        /// </summary>
        /// <param name="fileName">The name of the txt file</param>
        /// <returns>The username of the user who is merging</returns>
        public string getUserNameFromMergeFileName(string fileName)
        {
            int foundUnder = fileName.IndexOf("_");
            int foundDot = fileName.IndexOf(".");
            int numberOfCharacters = foundDot - foundUnder - 1;

            string userName = fileName.Substring(foundUnder + 1, numberOfCharacters);
            return userName;

        }


        /// <summary>
        /// Set the VC into "merging mode" with the current user as the merger.
        /// </summary>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool StartMerge()
        {
            if (!_isLoggedIn)
            {
                errorMessage = "You need to be logged in to start a merge!";
                return false;
            }
            if (!_isInitialized)
            {
                errorMessage = "You need to be connected to a project to start a merge!";
                return false;
            }
            if (!HasCurrentUserCurrentProjectPushRight())
            {
                errorMessage = "You don't have permissions to start a merge!";
                return false;
            }
            try
            {
                //First, we need to check a merge has already been started
                //For this, we check if there is a file named 
                IReadOnlyList<RepositoryContent> docs = _client.Repository.Content.GetAllContents(vc_projectInfo.ProjectId).Result;
                if (docs != null && docs.Count() > 0)
                {
                    foreach (RepositoryContent file in docs)
                    {
                        string name = file.Name;
                        if (name.StartsWith("merging"))
                        {
                            string mergingUser = getUserNameFromMergeFileName(name);
                            errorMessage = "User '" + mergingUser + "' is already merging.";
                            return false;

                        }

                    }
                    _userCurrentlyMerging = currentUser.username;
                    CreateFileRequest request = new CreateFileRequest("merging", "merging");
                    RepositoryContentChangeSet mergingFile = _client.Repository.Content.CreateFile(vc_projectInfo.ProjectId, "merging_" + _userCurrentlyMerging + ".txt", request).Result;

                }
            }

            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Could not start Merge:\n" + exceptionMessage;
                return false;
            }

            SendProjectNotificationMail(VC_NOTIFICATION_TYPE.START_MERGE, vc_projectInfo.ProjectId, _userCurrentlyMerging);
            return true;
        }


        /// <summary>
        /// Abort the current merge.
        /// </summary>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool AbortMerge()
        {
            if (!_isLoggedIn)
            {
                errorMessage = "You need to be logged in to abort a merge!";
                return false;
            }
            if (!isCurrentlyMerging)
            {
                errorMessage = "You need to be currently merging to abort a merge!";
                return false;
            }
            if (!HasCurrentUserCurrentProjectPushRight())
            {
                errorMessage = "You don't have permissions to start a merge!";
                return false;
            }
            try
            {
                IReadOnlyList<RepositoryContent> docs = _client.Repository.Content.GetAllContents(vc_projectInfo.ProjectId).Result;
                if (docs != null && docs.Count() > 0)
                {
                    foreach (RepositoryContent file in docs)
                    {
                        string name = file.Name;
                        if (name.StartsWith("merging"))
                        {

                            string mergingUser = getUserNameFromMergeFileName(name);

                            if (_userCurrentlyMerging != currentUser.username)
                            {
                                errorMessage = "You need to be the owner of the current merge to abort it!";
                                return false;
                            }
                            DeleteFileRequest requestDelete = new DeleteFileRequest("Deleting file", file.Sha);

                            _client.Repository.Content.DeleteFile(vc_projectInfo.ProjectId, "merging_" + _userCurrentlyMerging + ".txt", new DeleteFileRequest("File deletion", file.Sha)).Wait();

                            _userCurrentlyMerging = string.Empty;

                            SendProjectNotificationMail(VC_NOTIFICATION_TYPE.ABORT_MERGE, vc_projectInfo.ProjectId, currentUser.username);
                            return true;
                        }
                    }
                }
                else
                {
                    errorMessage = "You need to be currently merging to abort a merge!";
                    _userCurrentlyMerging = string.Empty;
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Could not abort Merge:\n" + exceptionMessage;
                return false;
            }
        }

        /// <summary>
        /// Finish the current merge.
        /// </summary>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool FinishMerge(string version, List<string> changes = null)
        {
            if (!_isLoggedIn)
            {
                errorMessage = "You need to be logged in to finish a merge!";
                return false;
            }
            if (!isCurrentlyMerging)
            {
                errorMessage = "You need to be currently merging to finish a merge!";
                return false;
            }

            if (_userCurrentlyMerging != currentUser.username)
            {
                errorMessage = "You need to be the owner of the current merge to abort it!";
                return false;
            }
            else
            {
                try
                {
                    IReadOnlyList<RepositoryContent> docs = _client.Repository.Content.GetAllContents(vc_projectInfo.ProjectId).Result;
                    if (docs != null && docs.Count() > 0)
                    {
                        foreach (RepositoryContent file in docs)
                        {
                            string name = file.Name;
                            if (name.StartsWith("merging"))
                            {

                                string mergingUser = getUserNameFromMergeFileName(name);
                                if (currentUser.username != mergingUser)
                                {
                                    errorMessage = "You need to be the owner of the current merge to abort it!";
                                    return false;
                                }


                                DeleteFileRequest requestDelete = new DeleteFileRequest("Deleting file", file.Sha);
                                _client.Repository.Content.DeleteFile(vc_projectInfo.ProjectId, "merging_" + _userCurrentlyMerging + ".txt", new DeleteFileRequest("File deletion", file.Sha)).Wait();


                                SendProjectNotificationMail(VC_NOTIFICATION_TYPE.FINISH_MERGE, vc_projectInfo.ProjectId, _userCurrentlyMerging, version, changes);
                                _userCurrentlyMerging = string.Empty;


                                return true;
                            }

                        }
                        errorMessage = "Could not finish Merge:\n";
                        return false;
                    }
                    else
                    {
                        errorMessage = "Could not finish Merge:\n";
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                    errorMessage = "Could not finish Merge:\n" + exceptionMessage;
                    return false;
                }
            }
        }


        /// <summary>
        /// Get all available Units on a given online Project.
        /// </summary>
        /// <param name="projectId">The Id of the Project to look for.</param>
        /// <param name="units">Out: The list of available Units.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GetRemoteUnits(long projectId, out List<VersionControlUnitInfo> units, bool getFullInfo = false)
        {
            units = new List<VersionControlUnitInfo>();
            if (projectTree == null) projectTree = GetProjectTree();
            ProjectNode proj = projectTree.getNodeById(projectId);
            if (proj == null)
            {
                errorMessage = "Project not found!";
                return false;
            }
            try
            {
                if (HasCurrentUserProjectDownloadRight(projectId))
                {
                    foreach (ProjectNode tnode in proj.Children)
                    {
                        if (tnode.Type != VC_FOLDER_TYPE.TYPEFOLDER) continue;
                        foreach (ProjectNode node in tnode.Children)
                        {
                            if (node.Type == VC_FOLDER_TYPE.ADDON || node.Type == VC_FOLDER_TYPE.CONFIG || node.Type == VC_FOLDER_TYPE.COUNTRY || node.Type == VC_FOLDER_TYPE.LOG || node.Type == VC_FOLDER_TYPE.INPUT)
                            {
                                VersionControlUnitInfo newUnit = new VersionControlUnitInfo
                                {
                                    UnitId = node.Id,
                                    Name = node.Name,
                                    UnitType = node.Type,
                                    ProjectId = projectId
                                };

                                units.Add(newUnit);
                            }
                        }
                    }
                }
                else
                {
                    errorMessage = "The current user does not have download rights for this project.";
                    return false;
                }


            }
            catch (Exception ex)
            {
                errorMessage = "Could not retrieve remote units:\n" + ex.Message;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Get all Units on the current project which are locally version-controlled.
        /// </summary>
        /// <param name="units">Out: The list of Units.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GetLocalUnits(out List<VersionControlUnitInfo> units)
        {
            units = null;
            if (vc_projectInfo.Units == null) { errorMessage = "Error in assessing local units."; return false; }
            units = new List<VersionControlUnitInfo>();
            foreach (VersionControlUnitInfo unit in vc_projectInfo.Units)
                if (projectTree != null && projectTree.getNodeById(unit.UnitId) != null) // make sure only units are shown which the user has access to (i.e. can be found in the rights-adapted project-tree)
                    if (DoesLocalUnitExist(unit)) // also make sure units exist locally
                        units.Add(unit);
            return true;
        }

        /// <summary>
        /// Download a given version of a specific country/add-on.
        /// </summary>
        /// <param name="unit">The unit to download.</param>
        /// <param name="version">The version of the unit to download.</param>
        /// <param name="alternativeName">Optional: The name to give to the downloaded commit.</param>
        /// <param name="alternativePath">Optional: The path to store the downloaded commit.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GET(VersionControlUnitInfo unit, string version, string alternativeName = "", string alternativePath = "", bool isNewProject = false, bool isMergeEnvironment = false)
        {

            try
            {
                bool updateProjectXML = (alternativeName == "" && alternativePath == "");

                bool hasRight = HasCurrentUserProjectDownloadRight(unit.ProjectId);
                if (!hasRight)
                {
                    errorMessage = "The current user does not have download rights for this unit.";
                    return false;
                }


                if (alternativePath == "") alternativePath = getLocalPath(unit);
                if (alternativeName == "") alternativeName = unit.Name;

                string path = getGitHubPath(unit, version);
                version = System.Net.WebUtility.UrlEncode(version);
                IReadOnlyList<RepositoryContent> response = _client.Repository.Content.GetAllContentsByRef(unit.ProjectId, path, version).Result;

                if (response != null)
                {
                    string tempPath = string.Empty;
                    string tempFilePath = string.Empty;

                    foreach (RepositoryContent content in response)
                    {
                        string contentNameWithoutExtension = Path.GetFileNameWithoutExtension(content.Name);
                        if (contentNameWithoutExtension == unit.Name)
                        {

                            tempPath = alternativePath + "_temp";
                            tempFilePath = Path.Combine(tempPath, content.Name);


                            if (!Directory.Exists(tempPath)) Directory.CreateDirectory(tempPath);

                            byte[] filecontent = null;
                            var contentWait = _client.Git.Blob.Get(unit.ProjectId, content.Sha);
                            contentWait.Wait();
                            Blob downloadedBlob = contentWait.Result;
                            var contentBase64 = downloadedBlob.Content;
                            filecontent = Convert.FromBase64String(contentBase64);

                            // Decrypt the content before using it
                            List<byte> passkey = new List<byte>();
                            foreach (char c in _encryptKey.ToCharArray()) passkey.Add((byte)c);
                            filecontent = SimpleCrypt.SimpleDecrypt(filecontent, passkey.ToArray());

                            File.WriteAllBytes(tempFilePath, filecontent);
                        }
                    }

                    tempFilePath = Path.Combine(tempPath, unit.Name) + FILE_EXTENTION_ZIP;
                    using (ZipFile zip = ZipFile.Read(tempFilePath))
                    {
                        if (!String.IsNullOrEmpty(zip.Comment) && zip.Comment.Equals(_authorName))
                        {
                            zip.Password = _encryptKey2;
                            if (!isMergeEnvironment)
                            {
                                zip.ExtractAll(alternativePath, ExtractExistingFileAction.OverwriteSilently);
                            }
                            else
                            {
                                zip.ExtractAll(tempPath, ExtractExistingFileAction.OverwriteSilently);

                            }

                            zip.Dispose();
                            File.Delete(tempFilePath);

                            if (isMergeEnvironment)
                            {
                                renameZipFiles(alternativeName, unit.Name, tempPath, alternativePath);

                            }

                            var dir = new DirectoryInfo(tempPath);
                            dir.Delete(true);
                        }
                        else
                        {
                            bool success = true;
                            if (zip.Entries.Count > 0)
                            {
                                errorMessage = "Security error found when downloading. The file was not created from EUROMOD. Please, contact an administrator user.";
                                success = false;
                            }

                            zip.Dispose();
                            var dir = new DirectoryInfo(tempPath);
                            dir.Delete(true);

                            return success;
                        }

                    }

                }
                else
                {
                    throw new Exception();
                }

                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
        }


        public void renameZipFiles(String alternativeName, String unitName, String alternativePath, string finalPath)
        {
            DirectoryInfo d = new DirectoryInfo(alternativePath);
            FileInfo[] infos = d.GetFiles();
            foreach (FileInfo f in infos)
            {
                String name = f.Name.Replace(unitName, alternativeName);
                File.Move(f.FullName, finalPath + "\\" + name);
            }

        }


        public string getLocalPath(VersionControlUnitInfo unit)
        {
            string path = "";
            switch (unit.UnitType)
            {
                case VC_FOLDER_TYPE.ADDON: path = Path.Combine(FOLDER_PATH_XMLPARAM, FOLDER_PATH_ADDONS, unit.Name); break;
                case VC_FOLDER_TYPE.COUNTRY: path = Path.Combine(FOLDER_PATH_XMLPARAM, FOLDER_PATH_COUNTRIES, unit.Name); break;
                case VC_FOLDER_TYPE.CONFIG: path = Path.Combine(FOLDER_PATH_XMLPARAM, FOLDER_PATH_CONFIG); break;
                case VC_FOLDER_TYPE.INPUT: path = FOLDER_PATH_INPUT; break;
                case VC_FOLDER_TYPE.LOG: path = FOLDER_PATH_LOG; break;
                default:
                    throw new Exception("Unexpected error: Unknown Unit type.");
            }
            return Path.Combine(projectPath, path);
        }

        public VC_FOLDER_TYPE getFolderType(string folderTypeString)
        {

            if (folderTypeString == VC_FOLDER_TYPE.ADDON.ToString()) { return VC_FOLDER_TYPE.ADDON; }
            else if (folderTypeString == VC_FOLDER_TYPE.COUNTRY.ToString()) { return VC_FOLDER_TYPE.COUNTRY; }
            else if (folderTypeString == VC_FOLDER_TYPE.CONFIG.ToString()) { return VC_FOLDER_TYPE.CONFIG; }
            else if (folderTypeString == VC_FOLDER_TYPE.INPUT.ToString()) { return VC_FOLDER_TYPE.INPUT; }
            else if (folderTypeString == VC_FOLDER_TYPE.LOG.ToString()) { return VC_FOLDER_TYPE.LOG; }

            else
            {
                throw new Exception("Unexpected error: Unknown Unit type.");
            }

        }

        public string getGitHubPath(VersionControlUnitInfo unit, string version)
        {
            string ftype = string.Empty;
            string path = string.Empty;

            if (unit.UnitType == VC_FOLDER_TYPE.COUNTRY)
            {
                ftype = FOLDER_PATH_COUNTRIES;

            }
            else if (unit.UnitType == VC_FOLDER_TYPE.ADDON)
            {
                ftype = FOLDER_PATH_ADDONS;
            }
            else if (unit.UnitType == VC_FOLDER_TYPE.CONFIG)
            {
                ftype = FOLDER_PATH_CONFIG;
            }
            else if (unit.UnitType == VC_FOLDER_TYPE.LOG)
            {
                ftype = FOLDER_PATH_LOG;
            }
            else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
            {
                ftype = FOLDER_PATH_INPUT;
            }

            return path = ftype;
        }

        /// <summary>
        /// Update the comment of a given unit into the online repository as a new revision.
        /// </summary>
        /// <param name="unit">The name of the country/add-on unit to upload.</param>
        /// <param name="message">The message/description for this commit.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public string UpdateComment(VersionControlUnitInfo unit, string comment)
        {
            string fullComment = INFO_TYPE_PARENTVERSION + "[" + unit.Version + "]\n" + INFO_TYPE_COMMITMD5HASH + "[" + getUnitMD5Hash(unit) + "]\n" + INFO_TYPE_UNITID + "[" + unit.UnitId + "]\n" + INFO_TYPE_COMMENT + "[" + comment + "]";

            return fullComment;
        }

        /// <summary>
        /// Upload the current state of a given country/add-on into the online repository as a new commit.
        /// </summary>
        /// <param name="unit">The name of the country/add-on unit to upload.</param>
        /// <param name="message">The message/description for this commit.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool PUSH(VersionControlUnitInfo unit, bool update = false)
        {
            bool hasRight = HasCurrentUserCurrentProjectPushRight();

            string fileName = string.Empty;
            string zipFileName = string.Empty;

            if (!hasRight)
            {
                errorMessage = "The current user does not have upload rights for this unit.";
                return false;
            }
            try
            {

                string ftype = "";

                if (unit.UnitType == VC_FOLDER_TYPE.COUNTRY)
                {
                    ftype = FOLDER_PATH_COUNTRIES;

                }
                else if (unit.UnitType == VC_FOLDER_TYPE.ADDON)
                {
                    ftype = FOLDER_PATH_ADDONS;
                }
                else if (unit.UnitType == VC_FOLDER_TYPE.CONFIG)
                {
                    ftype = FOLDER_PATH_CONFIG;
                }
                else if (unit.UnitType == VC_FOLDER_TYPE.LOG)
                {
                    ftype = FOLDER_PATH_LOG;
                }
                else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
                {
                    ftype = FOLDER_PATH_INPUT;
                }

                string fullpath = projectPath;
                if (unit.UnitType == VC_FOLDER_TYPE.CONFIG)
                    fullpath = Path.Combine(projectPath, FOLDER_PATH_XMLPARAM, ftype, unit.Name);
                else if (unit.UnitType == VC_FOLDER_TYPE.LOG)
                    fullpath = Path.Combine(projectPath, FOLDER_PATH_LOG, unit.Name);
                else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
                    fullpath = Path.Combine(projectPath, unit.Name);
                else if (unit.UnitType == VC_FOLDER_TYPE.ADDON || unit.UnitType == VC_FOLDER_TYPE.COUNTRY)
                    fullpath = Path.Combine(projectPath, FOLDER_PATH_XMLPARAM, ftype, unit.Name, unit.Name);

                fileName = unit.Name + FILE_EXTENTION_ZIP;
                zipFileName = fullpath + FILE_EXTENTION_ZIP;


                for (int i = 0; i < 100; i++)
                {
                    string fileExtension = "";
                    if (i == 0)
                    {
                        fileExtension = FILE_EXTENTION_ZIP;
                    }
                    else if (i < 10)
                    {
                        fileExtension = ".z0" + i;
                    }
                    else
                    {
                        fileExtension = ".z" + i;
                    }

                    if (File.Exists(fullpath + fileExtension))
                    {
                        File.Delete(fullpath + fileExtension);
                    }
                }

                int segmentSize = 500 * 1024;
                //The size of the segments need to be decided
                using (ZipFile zip = new ZipFile(zipFileName))
                {
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                    if (unit.UnitType == VC_FOLDER_TYPE.LOG)
                    {
                        zip.AddFile(fullpath + FILE_EXTENTION_LOG, "");
                    }
                    else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
                    {
                        foreach (string f in ALLOWED_INPUT_FILES)
                        {
                            string filename = Path.Combine(fullpath, f);
                            if (File.Exists(filename))
                                zip.AddFile(filename, "");
                        }
                    }
                    else // it is an XML file (country, addon or config)
                    {
                        // all types have the main file
                        zip.AddFile(fullpath + FILE_EXTENTION_MAIN, "");
                        // if a country, also add the dataconfig file
                        if (unit.UnitType == VC_FOLDER_TYPE.COUNTRY) zip.AddFile(fullpath + FILE_EXTENTION_DATACONFIG, "");
                        // then if an icon file exists, also add that
                        if (File.Exists(fullpath + FILE_EXTENTION_ICON)) zip.AddFile(fullpath + FILE_EXTENTION_ICON, "");
                    }

                    zip.Save();
                    //Console.WriteLine("Saving first zip in order to measure the size: " + zipFileName);
                }

                if (File.Exists(zipFileName))
                {
                    FileInfo info = new FileInfo(zipFileName);
                    long length = info.Length;
                    int segments = (int)(length / segmentSize) + 1;
                    if (segments >= 95)
                    {
                        segmentSize = (int)(length / 95);
                    }

                    //We recalculate the segments again
                    double initialSegments = (double)length / (double)segmentSize;
                    segments = (int)(length / segmentSize) + 1;
                    double tolerance = 0.1;
                    while (Math.Abs(segments - initialSegments) <= tolerance)
                    {
                        segmentSize = segmentSize + 50 * 1024;
                        initialSegments = (double)length / (double)segmentSize;
                        segments = (int)(length / segmentSize) + 1;
                    }


                    while (File.Exists(zipFileName))
                    {
                        //Console.WriteLine("The testing zip is about to be removed: " + zipFileName);
                        try
                        {
                            File.Delete(zipFileName);
                            //Console.WriteLine("The testing zip has just been removed: " + zipFileName);
                        }
                        catch (Exception a)
                        {
                            //Console.WriteLine("The testing zip cannot be removed: " + zipFileName);
                        }

                    }

                }

                int segmentsCreated = -1;
                //Console.WriteLine("Before opening new zip file for final file: " + zipFileName);
                using (ZipFile zip = new ZipFile(zipFileName))
                {
                    Console.WriteLine("Starting to compress: " + zipFileName);
                    zip.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                    zip.MaxOutputSegmentSize = segmentSize;
                    zip.Password = _encryptKey2;
                    zip.Comment = _authorName;

                    if (unit.UnitType == VC_FOLDER_TYPE.LOG)
                    {
                        zip.AddFile(fullpath + FILE_EXTENTION_LOG, "");
                    }
                    else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
                    {
                        foreach (string f in ALLOWED_INPUT_FILES)
                        {
                            string filename = Path.Combine(fullpath, f);
                            if (File.Exists(filename))
                                zip.AddFile(filename, "");
                        }
                    }
                    else // it is an XML file (country, addon or config)
                    {
                        // all types have the main file
                        zip.AddFile(fullpath + FILE_EXTENTION_MAIN, "");
                        // if a country, also add the dataconfig file
                        if (unit.UnitType == VC_FOLDER_TYPE.COUNTRY) zip.AddFile(fullpath + FILE_EXTENTION_DATACONFIG, "");
                        // then if an icon file exists, also add that
                        if (File.Exists(fullpath + FILE_EXTENTION_ICON)) zip.AddFile(fullpath + FILE_EXTENTION_ICON, "");
                    }
                    // finally save the zip file
                    //zip.Comment = String.Format("This zip archive was created by the EUROMOD application on machine '{0}'", System.Net.Dns.GetHostName());
                    //zip.Password = EncryptKey; 

                    //Console.WriteLine("The real zip is about to be created: " + zipFileName);
                    zip.Save();
                    segmentsCreated = zip.NumberOfSegmentsForMostRecentSave;
                    //Console.WriteLine("Number of segments created: " + segmentsCreated);
                    //segmentsCreated = 1;
                }

                List<string> currentVersionFiles = new List<string>();
                string fileSha = "";
                bool getFileSha = false;
                for (int i = 1; i <= segmentsCreated; i++)
                {
                    string extension = "";
                    //Encrypt the content before uploading it
                    if (i == segmentsCreated)
                    {
                        extension = FILE_EXTENTION_ZIP;
                        getFileSha = true;
                    }
                    else if (i < 10)
                    {
                        extension = ".z0" + i;
                    }
                    else
                    {
                        extension = ".z" + i;
                    }

                    zipFileName = fullpath + extension;
                    fileName = unit.Name + extension;
                    Console.WriteLine("Before reading zip. Zip file name: " + zipFileName);
                    byte[] content = File.ReadAllBytes(zipFileName);
                    //Console.WriteLine("After reading zip. Zip file name: " + zipFileName);

                    List<byte> passkey = new List<byte>();
                    foreach (char c in _encryptKey.ToCharArray()) passkey.Add((byte)c);
                    content = SimpleCrypt.SimpleEncrypt(content, passkey.ToArray());

                    //uploading the file
                    RepositoryContentChangeSet file = null;

                    //We are going to check if the file already exists (because it is an update or because it existed in a previous release)
                    bool found = false;
                    IReadOnlyList<RepositoryContent> foldersFiles = _client.Repository.Content.GetAllContents(unit.ProjectId, ftype).Result;
                    if (foldersFiles != null)
                    {
                        foreach (RepositoryContent singleFile in foldersFiles)
                        {
                            //If it exists
                            if (singleFile.Name == unit.Name + extension)
                            {
                                //var newRequest = new UpdateFileRequest("Uploading unit", Convert.ToBase64String(content), singleFile.Sha, false);
                                var newRequest = new UpdateFileRequest("Uploding unit", Convert.ToBase64String(content), singleFile.Sha, false);
                                file = _client.Repository.Content.UpdateFile(unit.ProjectId, ftype + "/" + fileName, newRequest).Result;
                                found = true;
                            }
                        }

                    }

                    if (!found)
                    {
                        //var request = new CreateFileRequest("Uploading unit", Convert.ToBase64String(content), false);
                        var request = new CreateFileRequest("uploading unit", Convert.ToBase64String(content), false);
                        file = _client.Repository.Content.CreateFile(unit.ProjectId, ftype + "/" + fileName, request).Result;

                    }

                    if (getFileSha)
                    {
                        fileSha = file.Content.Sha;
                    }
                    currentVersionFiles.Add(fileName);
                    File.Delete(zipFileName);
                }

                IReadOnlyList<RepositoryContent> foldersFilesAfterUploading = _client.Repository.Content.GetAllContents(unit.ProjectId, ftype).Result;
                if (foldersFilesAfterUploading != null)
                {
                    foreach (RepositoryContent singleFile in foldersFilesAfterUploading)
                    {

                        //If the file exists in the list of files
                        if (!currentVersionFiles.Contains(singleFile.Name) && (Path.GetFileNameWithoutExtension(singleFile.Name).Equals(unit.Name))) {
                            DeleteFileRequest request = new DeleteFileRequest(singleFile.Name + "has been deleted", singleFile.Sha);
                            _client.Repository.Content.DeleteFile(unit.ProjectId, ftype + "/" + singleFile.Name, request).Wait();
                        }

                    }
                }

                unit.Version = fileSha;
                unit.UnitHash = getUnitMD5Hash(unit);
                LinkUnitToVersionControl(unit);

                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;

                if (!string.IsNullOrEmpty(zipFileName) && File.Exists(zipFileName))
                {
                    File.Delete(zipFileName);
                }

                return false;
            }
        }

        // This function will look for a parameter and return its value, or an empty string if the parameter does not exist
        private static string parseCommentInfo(string comment, string name)
        {
            string info = "";
            int pos = comment.IndexOf(name);
            if (pos > -1)
            {
                pos += name.Length + 1;
                int len = comment.IndexOf("]", pos) - pos;
                info = comment.Substring(pos, len);
            }
            return info;
        }


        /// <summary>
        /// Get a list of available releases/upload-bundles for a given project.
        /// </summary>
        /// <param name="projectId">The Id of the project to return the releases for.</param>
        /// <param name="releasesOrBundles">Out: The list of available releases/bundles.</param>
        public bool GetReleases(out List<ReleaseInfo> releases) { return GetReleases(vc_projectInfo.ProjectId, out releases); }
        public bool GetReleases(long projectId, out List<ReleaseInfo> releasesOrBundles)
        {
            releasesOrBundles = new List<ReleaseInfo>();

            try
            {
                IReadOnlyList<Release> releases = _client.Repository.Release.GetAll(projectId).Result;

                if (releases == null || releases.Count == 0) return true;  //release-info does not yet exist (no releases yet)
                else
                {
                    foreach (Release release in releases.OrderByDescending(x => x.PublishedAt))
                    {

                        //If the user is deleted from GitHub, the author login is null
                        string authorLogin = string.Empty;
                        if (release.Author != null && !String.IsNullOrEmpty(release.Author.Login))
                        {
                            authorLogin = release.Author.Login;
                        }
                        else
                        {
                            authorLogin = "Deleted user";
                        }

                        releasesOrBundles.Add(new ReleaseInfo
                        {
                            Id = release.Id,
                            Author = authorLogin,
                            Date = release.CreatedAt.DateTime,
                            Name = release.TagName,
                            Message = release.TagName,
                        });
                    }


                }
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Failure in assessing available releases." + Environment.NewLine + exceptionMessage;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Get the information on the latest release. It is null if there are no releases yet
        /// </summary>
        /// <param name="projectId"></param>
        /// <param name="latestReleaseInfo"></param>
        /// <returns></returns>
        public bool GetLatestReleaseUnitInfo(long projectId, out ReleaseInfo latestReleaseInfo)
        {
            latestReleaseInfo = null;

            try 
            {
                // First, we try to get all releases and check if they are 0
                IReadOnlyList<Release> releases = _client.Repository.Release.GetAll(projectId).Result;
                // If there are not releases, it will return an empty list
                if (releases == null || releases.Count() == 0) { return true; }
                // Else get the latest release
                Octokit.Release latestRelease = releases.OrderByDescending(x => x.PublishedAt).First();

                if (latestRelease != null)
                {
                    //If the user is deleted from GitHub, the author login is null
                    string authorLogin = string.Empty;
                    if (latestRelease.Author != null && !String.IsNullOrEmpty(latestRelease.Author.Login))
                    {
                        authorLogin = latestRelease.Author.Login;
                    }
                    else
                    {
                        authorLogin = "Deleted user";
                    }

                    latestReleaseInfo = new ReleaseInfo
                    {
                        Id = latestRelease.Id,
                        Author = authorLogin,
                        Date = latestRelease.CreatedAt.DateTime,
                        Name = latestRelease.TagName,
                        Message = latestRelease.Body,
                    };
                }
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Failure in assessing latest release/upload-bundle." + Environment.NewLine + exceptionMessage;
                return false;
            }

            return true;

        }

        /// <summary>
        /// Remove a release from the VC server
        /// </summary>
        /// <param name="rbInfos">List of releases to delete</param>
        /// <param name="projectId">Id of the project to be removed</param>
        /// <returns></returns>
        public bool RemoveReleases(List<ReleaseInfo> rbInfos, long projectId = -1)
        {

            try
            {

                if (HasCurrentUserProjectPushRight(projectId))
                {

                    foreach (ReleaseInfo releaseInfo in rbInfos)
                    {
                        //First, we need to check if the release is the latest one
                        IReadOnlyList<Release> releases = _client.Repository.Release.GetAll(projectId).Result;
                        Release latestRelease = releases.OrderByDescending(x => x.PublishedAt).First();
                        long latestReleaseId = latestRelease.Id;
                        bool isLatestRelease = false;

                        if (latestReleaseId == releaseInfo.Id)
                        {
                            isLatestRelease = true;
                            latestRelease = null;
                        }

                        //We remove the release
                        _client.Repository.Release.Delete(projectId, (int)releaseInfo.Id).Wait();

                        //The tag also needs to be deleted
                        Reference tag = _client.Git.Reference.Get(projectId, "tags/" + releaseInfo.Name).Result;

                        if (tag != null)
                        {
                            _client.Git.Reference.Delete(projectId, "tags/" + releaseInfo.Name).Wait();
                        }

                        //If the release we are trying to remove is the last one, we also need to remove the commits from the last release
                        if (isLatestRelease)
                        {
                            
                            //The latest release now is the previous one
                            try
                            {
                                releases = _client.Repository.Release.GetAll(projectId).Result;
                                latestRelease = releases.OrderByDescending(x => x.PublishedAt).First();
                            }catch(Exception e)
                            {
                                latestRelease = null;
                            }

                            //If there are previous releases
                            if(latestRelease != null)
                            {
                                var request = new CommitRequest { Since = latestRelease.PublishedAt };
                                IReadOnlyList<GitHubCommit> commits = _client.Repository.Commit.GetAll(projectId, request).Result;
                                if (commits != null)
                                {
                                    GitHubCommit selectedCommit = commits.Last();
                                    _client.Git.Reference.Update(projectId, "heads/master", new ReferenceUpdate(selectedCommit.Sha, true)).Wait();
                                }
                            }
                            //If there are not other previous releases
                            else
                            {
                                IReadOnlyList<GitHubCommit> allCommits = _client.Repository.Commit.GetAll(projectId).Result;
                                List<GitHubCommit> commits = new List<GitHubCommit>();
                                foreach (GitHubCommit singleCommit in allCommits)
                                {
                                    string message = singleCommit.Commit.Message;
                                    if(message.Equals("Initial upload"))
                                    {
                                        _client.Git.Reference.Update(projectId, "heads/master", new ReferenceUpdate(singleCommit.Sha, true)).Wait();
                                        break;
                                    }
                                }
                            }
                           
                        }

                    }
                }

                projectTree = GetProjectTree(); //refresh project tree
                UnlinkLocalFromVersionControl();
                LinkLocalToVersionControl(projectId);
                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Failed to remove release." + Environment.NewLine + exceptionMessage;
                return false;
            }
        }


        /// <summary>
        /// Add a new unit into the given online project (no commit - just create the unit both online and locally).
        /// </summary>
        /// <param name="projectId">The Id of the project into which to add the unit.</param>
        /// <param name="unit">The name of the unit to add to the project.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        // <summary>
        /// Add a new unit into the given online project (no commit - just create the unit both online and locally).
        /// </summary>
        /// <param name="projectId">The Id of the project into which to add the unit.</param>
        /// <param name="unit">The name of the unit to add to the project.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool AddUnitToVersionControl(string unit, VC_FOLDER_TYPE unitType, out long unitId)
        {
            bool hasRight = HasCurrentUserCurrentProjectAdminRight();
            if (!hasRight)
            {
                errorMessage = "User does not have Admin rights for this Project";
                unitId = -1;
                return false;
            }
            try
            {
                unit = unit.ToUpper();
                long folderId = -1;
                string desc = "";
                if (unitType == VC_FOLDER_TYPE.COUNTRY)
                {
                    desc = FOLDER_TYPE_COUNTRY;
                    folderId = vc_projectInfo.FolderCountriesId;
                }
                else if (unitType == VC_FOLDER_TYPE.ADDON)
                {
                    desc = FOLDER_TYPE_ADDON;
                    folderId = vc_projectInfo.FolderAddOnsId;
                }
                else if (unitType == VC_FOLDER_TYPE.CONFIG)
                {
                    desc = FOLDER_TYPE_CONFIG;
                    folderId = vc_projectInfo.FolderConfigId;
                }
                else if (unitType == VC_FOLDER_TYPE.INPUT)
                {
                    desc = FOLDER_TYPE_INPUT;
                    folderId = vc_projectInfo.FolderInputId;
                }
                else if (unitType == VC_FOLDER_TYPE.LOG)
                {
                    desc = FOLDER_TYPE_LOG;
                    folderId = vc_projectInfo.FolderLogId;
                }

                Random rnd = new Random();
                int id = rnd.Next(1, 130000);

                vc_projectInfo.Units.Add(new VersionControlUnitInfo
                {
                    UnitId = id,
                    ProjectId = vc_projectInfo.ProjectId,
                    Name = unit,
                    UnitType = unitType
                });


                ProjectNode node = projectTree.getNodeById(folderId);
                if (node != null) node.addChild(new ProjectNode(unitType, id, unit, folderId, desc, "", ""));
                unitId = id;

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                unitId = -1;
                return false;
            }
        }

        /// <summary>
        /// Remove a unit from an online project.
        /// </summary>
        /// <param name="projectId">The Id of the project whose unit will be removed.</param>
        /// <param name="unitId">The Id of the unit to remove from the project.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool RemoveUnitFromVersionControl(long projectId, VersionControlUnitInfo unitInfoToRemove)
        {

            try
            {

                string ftype = String.Empty;
                if (unitInfoToRemove.UnitType == VC_FOLDER_TYPE.COUNTRY)
                {
                    ftype = FOLDER_PATH_COUNTRIES;

                }
                else if (unitInfoToRemove.UnitType == VC_FOLDER_TYPE.ADDON)
                {
                    ftype = FOLDER_PATH_ADDONS;
                }
                else if (unitInfoToRemove.UnitType == VC_FOLDER_TYPE.CONFIG)
                {
                    ftype = FOLDER_PATH_CONFIG;
                }
                else if (unitInfoToRemove.UnitType == VC_FOLDER_TYPE.LOG)
                {
                    ftype = FOLDER_PATH_LOG;
                }
                else if (unitInfoToRemove.UnitType == VC_FOLDER_TYPE.INPUT)
                {
                    ftype = FOLDER_PATH_INPUT;
                }

                string fileName = unitInfoToRemove.Name + FILE_EXTENTION_ZIP;

                IReadOnlyList<RepositoryContent> foldersFiles = _client.Repository.Content.GetAllContents(unitInfoToRemove.ProjectId, ftype).Result;
                if (foldersFiles != null)
                {
                    foreach (RepositoryContent singleFile in foldersFiles)
                    {
                        //If the file exists in the list of files
                        if ((Path.GetFileNameWithoutExtension(singleFile.Name).Equals(unitInfoToRemove.Name)))
                        {
                            DeleteFileRequest request = new DeleteFileRequest(singleFile.Name + "has been deleted", singleFile.Sha);
                            try
                            {
                                _client.Repository.Content.DeleteFile(unitInfoToRemove.ProjectId, ftype + "/" + singleFile.Name, request).Wait();
                            } catch (Exception exc)
                            {
                                if (exc.InnerException != null && exc.InnerException.Message != null)
                                {
                                    if (!exc.InnerException.Message.ToLower().Equals("not found")) { throw exc; }
                                }
                            }
                        }

                    }
                }

                VersionControlUnitInfo unitInfo = vc_projectInfo.Units.First(x => x.Name == unitInfoToRemove.Name);
                vc_projectInfo.Units.Remove(unitInfo);
                projectTree.removeChild(unitInfoToRemove.UnitId);

                UnlinkUnitFromVersionControl(unitInfoToRemove.UnitId);

                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
        }

        public bool UnlinkUnitFromVersionControl(long unitId)
        {
            try
            {
                foreach (VersionControlUnitInfo ui in vc_projectInfo.Units)
                    if (ui.UnitId == unitId) { vc_projectInfo.Units.Remove(ui); break; }
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Could not unlink Unit from version control.\n" + ex.Message;
                return false;
            }
        }

        public bool LinkUnitToVersionControl(VersionControlUnitInfo unit)
        {
            try
            {

                ProjectNode node = projectTree.getNodeById(unit.UnitId);
                if (!vc_projectInfo.Units.Exists(x => x.UnitId == unit.UnitId))   // if the country does not already exist in the list, add it

                    vc_projectInfo.Units.Add(new VersionControlUnitInfo
                    {
                        UnitId = Convert.ToInt64(unit.UnitId),
                        Name = unit.Name,
                        UnitType = unit.UnitType,
                        ProjectId = unit.ProjectId,
                        Version = unit.Version,
                        UnitHash = unit.UnitHash,
                    });
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = "Could not link Unit to version control.\n" + ex.Message;
                return false;
            }
        }

        /// <summary>
        /// Add a new project in the default Folder online (no upload - just create the project).
        /// </summary>
        /// <param name="projectName">The name of the Project to add to the default folder.</param>
        /// <param name="projectId">Out: The Id of the newly created Project.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool AddProjectToVersionControl(string projectName, out long projectId)
        {
            projectId = -1;
            try
            {
                //A new repository is created
                var repository = new NewRepository(projectName);
                repository.AutoInit = false;
                repository.Description = FOLDER_TYPE_PROJECT;
                repository.LicenseTemplate = "";
                repository.Private = true;

                Repository repositoryCreated = _client.Repository.Create(_organisationName, repository).Result;
                projectId = repositoryCreated.Id;
                long repositoryId = projectId;

                // Then create the five subfolders
                foreach (string fp in new string[] { FOLDER_PATH_ADDONS, FOLDER_PATH_CONFIG, FOLDER_PATH_COUNTRIES, FOLDER_PATH_LOG, FOLDER_PATH_INPUT })
                {
                    CreateFileRequest request = new CreateFileRequest("Initial upload", "test");
                    //Empty folders cannot be created (I think...)
                    RepositoryContentChangeSet file = _client.Repository.Content.CreateFile(repositoryId, fp + "/" + INITIAL_FILE_NAME, request).Result;
                }

                projectTree = GetProjectTree(); //refresh project tree

                return true;
            }
            catch (Exception ex)
            {

                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");

                errorMessage = exceptionMessage;
                return false;
            }
        }



        /// <summary>
        /// Remove an online project.
        /// </summary>
        /// <param name="projectId">The Id of the project to removed.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool RemoveProjectFromVersionControl(long projectId)
        {

            //The user needs to have admin right for that project
            bool adminRight = false;
            GetUserProjectRights(projectId, GetCurrentUser().username, out adminRight, out VC_ACCESS_RIGHTS defaultRight, false);

            if (!adminRight)
            {
                errorMessage = "User does not have Admin rights for this Project";
                return false;
            }
            try
            {

                _client.Repository.Delete(projectId).Wait();
                projectTree = GetProjectTree(true, projectId); //refresh project tree
                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
        }

        /// <summary>
        /// Links the local project to an online project.
        /// </summary>
        /// <param name="projectId">The Id of the online project the local project will be linked to.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool LinkLocalToVersionControl(long projectId)
        {
            try
            {
                Repository repository = _client.Repository.Get(projectId).Result;

                vc_projectInfo = new VersionControlProjectInfo()
                {
                    isConnected = true,
                    ProjectId = repository.Id,
                    ProjectName = repository.Name,
                    Description = repository.Description,
                    Units = new List<VersionControlUnitInfo>()
                };

                //We get the info from the latest release
                GetLatestReleaseUnitInfo(repository.Id, out ReleaseInfo latestReleaseInfo);
                List<VersionControlUnitInfo> releaseUnitInfos = null;
                if (latestReleaseInfo != null) { releaseUnitInfos = GetReleaseUnitsInfo(repository.Id, latestReleaseInfo.Name); }

                IReadOnlyList<RepositoryContent> repoFolders = _client.Repository.Content.GetAllContents(repository.Id).Result;
                foreach (RepositoryContent tf in repoFolders)
                {
                    VCAPI.VC_FOLDER_TYPE ft_type = 0;
                    int typeInt = 0;
                    string idString = "";

                    switch (tf.Name)
                    {
                        case FOLDER_PATH_ADDONS: typeInt = (int)VC_FOLDER_TYPE.ADDON; idString = repository.Id.ToString() + typeInt.ToString(); vc_projectInfo.FolderAddOnsId = Convert.ToInt64(idString); ft_type = VC_FOLDER_TYPE.ADDON; break;
                        case FOLDER_PATH_CONFIG: typeInt = (int)VC_FOLDER_TYPE.CONFIG; idString = repository.Id.ToString() + typeInt.ToString(); vc_projectInfo.FolderConfigId = Convert.ToInt64(idString); ft_type = VC_FOLDER_TYPE.CONFIG; break;
                        case FOLDER_PATH_COUNTRIES: typeInt = (int)VC_FOLDER_TYPE.COUNTRY; idString = repository.Id.ToString() + typeInt.ToString(); vc_projectInfo.FolderCountriesId = Convert.ToInt64(idString); ft_type = VC_FOLDER_TYPE.COUNTRY; break;
                        case FOLDER_PATH_LOG: typeInt = (int)VC_FOLDER_TYPE.LOG; idString = repository.Id.ToString() + typeInt.ToString(); vc_projectInfo.FolderLogId = Convert.ToInt64(idString); ft_type = VC_FOLDER_TYPE.LOG; break;
                        case FOLDER_PATH_INPUT: typeInt = (int)VC_FOLDER_TYPE.INPUT; idString = repository.Id.ToString() + typeInt.ToString(); vc_projectInfo.FolderInputId = Convert.ToInt64(idString); ft_type = VC_FOLDER_TYPE.INPUT; break;
                        default: continue;
                    }

                    if (releaseUnitInfos == null || releaseUnitInfos.Count() == 0) continue; //No content

                    foreach (VersionControlUnitInfo unit in releaseUnitInfos)
                    {
                        if (unit.UnitType == ft_type)
                        {
                            vc_projectInfo.Units.Add(new VersionControlUnitInfo
                            {
                                UnitId = unit.UnitId,
                                Name = unit.Name,
                                UnitType = ft_type,
                                ProjectId = repository.Id,
                                Version = unit.Version,
                                UnitHash = unit.UnitHash,
                            });
                        }
                    }
                }

                //The permissions for the current user are set
                GetUserProjectRights(projectId, GetCurrentUser().username, out bool adminRight, out VC_ACCESS_RIGHTS defaultRight, true);

                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
        }

        public bool UnlinkLocalFromVersionControl()
        {
            try
            {
                string projectInfoPath = Path.Combine(projectPath, FOLDER_PATH_CONFIG, FILENAME_VC_INFO);
                if (File.Exists(projectInfoPath)) File.Delete(projectInfoPath);
                _userCurrentlyMerging = string.Empty;
                _isCurrentUserCurrentProjectAdmin = false;
                _rightsCurrentUserCurrentProject = VC_ACCESS_RIGHTS.NONE;
                vc_projectInfo = new VersionControlProjectInfo { isConnected = false };
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }


        public string NextAutoBundleVersion(long projectId)
        {
            string releaseVersion;
            return GetNextAutoVersion(projectId, out releaseVersion) ? releaseVersion : "";
        }

        public bool GetNextAutoVersion(long projectId, out string releaseVersion)
        {
            releaseVersion = "A0.0";
            try
            {
                IReadOnlyList<Release> releases = _client.Repository.Release.GetAll(projectId).Result;
                if (releases != null && releases.Count > 0)
                {
                    string oldVer = releases.OrderByDescending(x => x.PublishedAt).First().TagName;
                    if (oldVer.LastIndexOf('.') > 0)
                    {
                        string oldMajorVer = oldVer.Substring(0, oldVer.LastIndexOf('.') + 1);
                        string oldMinorVer = oldVer.Substring(oldVer.LastIndexOf('.') + 1);
                        if (oldMinorVer.EndsWith("+")) oldMinorVer = oldMinorVer.Substring(0, oldMinorVer.Length - 1);
                        int minor;
                        if (int.TryParse(oldMinorVer, out minor))
                            releaseVersion = oldMajorVer + (int.Parse(oldMinorVer) + 1);
                        else
                            releaseVersion = oldVer + ".1";
                    }
                    else
                    {
                        releaseVersion = oldVer + ".1";
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Push a release to the version control
        /// </summary>
        /// <param name="versionName">Name of the release</param>
        /// <param name="units">Units to be uploaded</param>
        /// <param name="added">Added = true if new units have been added, false if units have been removed</param>
        /// <returns></returns>
        public bool PushRelease(string versionName, List<VersionControlUnitInfo> units, bool added)
        {
            try
            {
                //First, we need to check if there was already an existing release for this project
                long projectId = vc_projectInfo.ProjectId;

                ReleaseInfo previousReleaseInfo = null;
                GetLatestReleaseUnitInfo(projectId, out previousReleaseInfo);

                List<VersionControlUnitInfo> previousExistingReleaseUnits = null;
                List<VersionControlUnitInfo> currentReleaseUnits = null;

                if (previousReleaseInfo != null)
                {
                    string previousVersion = previousReleaseInfo.Name;
                    previousExistingReleaseUnits = GetReleaseUnitsInfo(projectId, previousVersion);

                    //If new units have been added
                    if (added)
                    {
                        currentReleaseUnits = new List<VersionControlUnitInfo>(units);

                        foreach (VersionControlUnitInfo singleUnit in previousExistingReleaseUnits)
                        {
                            VersionControlUnitInfo foundElement = currentReleaseUnits.Find(x => x.Name == singleUnit.Name);
                            if (foundElement == null)
                            {
                                currentReleaseUnits.Add(singleUnit);
                            }
                        }
                    }
                    else //If units have been removed
                    {
                        currentReleaseUnits = new List<VersionControlUnitInfo>();
                        foreach (VersionControlUnitInfo singleUnit in previousExistingReleaseUnits)
                        {
                            VersionControlUnitInfo foundElement = units.Find(x => x.Name == singleUnit.Name);
                            if (foundElement == null) //If the element is not found among the removed units
                            {
                                currentReleaseUnits.Add(singleUnit);
                            }
                        }

                    }
                }
                else //It is the first release, all units have been added
                {
                    currentReleaseUnits = new List<VersionControlUnitInfo>(units);
                }




                string body = string.Empty;
                foreach (VersionControlUnitInfo unit in currentReleaseUnits)
                    body += string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}", unit.Name, MARKER_RELEASE_SPLIT_2,
                                                                   unit.UnitId, MARKER_RELEASE_SPLIT_2,
                                                                   unit.UnitType, MARKER_RELEASE_SPLIT_2,
                                                                   unit.UnitHash, MARKER_RELEASE_SPLIT_2,
                                                                   unit.Version, MARKER_RELEASE_SPLIT_1);

                var newRelease = new NewRelease(versionName);
                newRelease.Name = versionName;
                newRelease.Body = body;
                newRelease.Draft = false;
                newRelease.Prerelease = false;
                var result = _client.Repository.Release.Create(projectId, newRelease).Result;

                return true;

            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
        }



        #region User Related


        #region Request User Rights

        /// <summary>
        /// Get the access rights a user has for a given project.
        /// </summary>
        /// <param name="projectId">The Id of the project to check.</param>
        /// <param name="userName">The username of the user to check</param>
        /// <param name="adminRight">Out: Whether (yes/no) this user has admin right for this project.</param>
        /// <param name="defaultRight">Out: The default access right this user has for this project.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GetUserProjectRights(long projectId, string userName, out bool adminRight, out VC_ACCESS_RIGHTS defaultRight, bool setPermissions = false)
        {
            //By default the user doesn't have any permissions and he/she is not admin
            defaultRight = VC_ACCESS_RIGHTS.NONE;
            adminRight = false;

            if (_client != null && projectId > 0 && userName != string.Empty)
            {
                try
                {
                    Octokit.Repository repository = _client.Repository.Get(projectId).Result;

                    if (repository != null)
                    {
                        if (repository.Permissions.Admin) { adminRight = true; defaultRight = VC_ACCESS_RIGHTS.UPLOAD; }
                        else if (repository.Permissions.Push) { defaultRight = VC_ACCESS_RIGHTS.UPLOAD; }
                        else if (repository.Permissions.Pull) { defaultRight = VC_ACCESS_RIGHTS.DOWNLOAD; }

                    }
                    else
                    {
                        return false;
                    }

                    if (setPermissions)
                    {
                        _isCurrentUserCurrentProjectAdmin = adminRight;
                        _rightsCurrentUserCurrentProject = defaultRight;
                    }
                }
                catch (Exception)
                {
                    errorMessage = "You don't have permission to access the current project.";
                    return false;
                }

            }
            return true;
        }


        /// <summary>
        /// Check if an user has Admin access rights for a given Project or Unit.
        /// </summary>
        /// <param name="projectId">The Id of the project to check.</param>
        /// <param name="userName">The username of the user</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool HasUserProjectAdminRight(long projectId, string userName, bool setPermissions = false)
        {
            bool adminRight = false;
            GetUserProjectRights(projectId, userName, out adminRight, out VC_ACCESS_RIGHTS defaultRight, setPermissions);
            return adminRight;
        }
        /// <summary>
        /// Check if the current user has Admin access rights for a given Project
        /// </summary>
        /// <param name="projectId">The Id of the project to check.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool HasCurrentUserProjectAdminRight(long projectId)
        {
            if (currentUser.username == String.Empty) { return false; }
            return HasUserProjectAdminRight(projectId, currentUser.username);
        }

        /// <summary>
        /// Check if the current user has Admin access rights for the current project
        /// </summary>
        /// <returns></returns>
        public bool HasCurrentUserCurrentProjectAdminRight()
        {
            if (currentUser.username == String.Empty || !(vc_projectInfo.ProjectId > 0)) { return false; }
            return HasUserProjectAdminRight(vc_projectInfo.ProjectId, currentUser.username, true);
        }

        /// <summary>
        /// Check if a given user has Push Rights for a given project
        /// </summary>
        /// <param name="projectId">The Id of the project to check.</param>
        /// <param name="userName">The username of the user</param>
        /// <returns></returns>
        public bool HasUserProjectPushRight(long projectId, string userName, bool setPermissions = false)
        {
            GetUserProjectRights(projectId, userName, out bool adminRight, out VC_ACCESS_RIGHTS defaultRight, setPermissions);

            bool pushRight = false;
            if (defaultRight.Equals(VC_ACCESS_RIGHTS.UPLOAD))
            {
                pushRight = true;
            }
            return pushRight;
        }

        /// <summary>
        /// Check if the current user has Push rights for a given project
        /// </summary>
        /// <param name="projectId">The id of the project to check</param>
        /// <returns></returns>
        public bool HasCurrentUserProjectPushRight(long projectId)
        {
            if (currentUser.username == String.Empty) { return false; }
            return HasUserProjectPushRight(projectId, currentUser.username);
        }

        /// <summary>
        /// Check if the current user has Push rights for the current project
        /// </summary>
        /// <returns></returns>
        public bool HasCurrentUserCurrentProjectPushRight()
        {
            if (currentUser.username == String.Empty || !(vc_projectInfo.ProjectId > 0)) { return false; }
            return HasUserProjectPushRight(vc_projectInfo.ProjectId, currentUser.username, true);
        }


        /// <summary>
        /// Check if a given user has download Rights for a given project
        /// </summary>
        /// <param name="projectId">The Id of the project to check.</param>
        /// <param name="userName">The username of the user</param>
        /// <returns></returns>
        public bool HasUserProjectDownloadRight(long projectId, string userName, bool setPermissions = false)
        {
            GetUserProjectRights(projectId, userName, out bool adminRight, out VC_ACCESS_RIGHTS defaultRight, setPermissions);

            bool downloadRight = false;
            if (defaultRight.Equals(VC_ACCESS_RIGHTS.DOWNLOAD) || defaultRight.Equals(VC_ACCESS_RIGHTS.UPLOAD))
            {
                downloadRight = true;
            }
            return downloadRight;
        }

        /// <summary>
        /// Check if the current user has download rights for a given project
        /// </summary>
        /// <param name="projectId">The id of the project to check</param>
        /// <returns></returns>
        public bool HasCurrentUserProjectDownloadRight(long projectId)
        {
            if (currentUser.username == String.Empty) { return false; }
            return HasUserProjectDownloadRight(projectId, currentUser.username);
        }

        /// <summary>
        /// Check if the current user has download rights for the current project
        /// </summary>
        /// <returns></returns>
        public bool HasCurrentUserCurrentProjectDownloadRight()
        {
            if (currentUser.username == String.Empty || !(vc_projectInfo.ProjectId > 0)) { return false; }
            return HasUserProjectDownloadRight(vc_projectInfo.ProjectId, currentUser.username, true);
        }


        #endregion Request User Rights


        /// <summary>
        /// Get info about user by username and password
        /// </summary>
        /// <param name="username">The user's username.</param>
        /// <param name="password">The user's password.</param>
        /// <param name="client">GitHubClient</param>
        /// <param name="userInfo">Out: Info about the user.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GetUserInfo(string userName, string password, GitHubClient client, out UserInfo userInfo)
        {
            client.Credentials = new Credentials(userName, password);
            User user = null;
            userInfo = null;

            try
            {
                user = client.User.Current().Result;
            }
            catch (Exception e)
            {
                if (e.InnerException != null && e.InnerException.Message != null)
                {
                    errorMessage = e.InnerException.Message;
                }
                else
                {
                    errorMessage = e.Message;
                }

                return false;
            }

            if (user != null)
            {
                userInfo = new UserInfo();
                userInfo.userId = user.Id;
                userInfo.username = userName;

                return true;
            }

            return false;
        }

        public UserInfo GetCurrentUser() { return currentUser; }


        /// <summary>
        /// Get a list of all the existing Version Control users.
        /// </summary>
        /// <param name="users">Out: The list of users.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GetAllUsers(out List<UserInfo> users)
        {
            users = new List<UserInfo>();

            try
            {
                IReadOnlyList<User> orgUsers = _client.Organization.Member.GetAll(_organisationName).Result;
                if (orgUsers != null && orgUsers.Count > 0)
                {
                    List<long> suspendedUserIds = GetSuspendedUsersIdList();
                    foreach (User orgUser in orgUsers)
                    {
                        if (!suspendedUserIds.Contains(orgUser.Id)) { 
                            UserInfo user = new UserInfo
                            {
                                userId = orgUser.Id,
                                username = orgUser.Login,
                                firstName = orgUser.Name,
                                lastName = orgUser.Name,
                                email = orgUser.Email,

                            };
                            users.Add(user);
                        }
                    }
                }
            }
            catch (System.Net.WebException ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                if (ex.Message.Contains(PROXY_ERROR))
                {
                    errorMessage = "Proxy Authentication Required.";
                }
                else
                {
                    errorMessage = "Error in retrieving users.\n" + exceptionMessage;
                }

                return false;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Error in retrieving users.\n" + exceptionMessage;
                return false;
            }
            return true;
        }


        /// <summary>
        /// Gets the entire user-settings of the project.
        /// </summary>
        /// <param name="projectId">The Id of the project to check.</param>
        /// <param name="userInfos">Out: The list of priject users.</param>
        /// <param name="adminRights">Out: A boolean list whether users have admin-rights or not, in the same order as userInfos.</param>
        /// <param name="defaultRights">Out: A list with users' default-rights for the project (none/down-/upload), in the same order as userInfos.</param>
        /// <param name="unitRights">Out: A list with users' rights for specific units, if these differ from default-project-right (in the form [unit-id,right]), in the same order as userInfos.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool GetProjectUserRights(long projectId, out List<UserInfo> userInfos, out List<bool> adminRights, out List<VC_ACCESS_RIGHTS> defaultRights)
        { return GetProjectUserRightsAndNotificationSettings(projectId, out userInfos, out adminRights, out defaultRights); }
        private bool GetProjectUserRightsAndNotificationSettings(long projectId, out List<UserInfo> userInfos,
                                         out List<bool> adminRights, out List<VC_ACCESS_RIGHTS> defaultRights)

        {
            userInfos = new List<UserInfo>();
            adminRights = new List<bool>();
            defaultRights = new List<VC_ACCESS_RIGHTS>();

            try
            {

                IReadOnlyList<Octokit.User> collaborators = _client.Repository.Collaborator.GetAll(projectId).Result;

                if (collaborators == null || collaborators.Count() == 0) { return true; }//presumably there are no users yet (should not happen actually)

                //Only non-suspended users have to be displayed
                List<long> suspendedUsersId = GetSuspendedUsersIdList();

                foreach (Octokit.User collaborator in collaborators)
                {
                    long userId = collaborator.Id;

                    if (!suspendedUsersId.Contains(userId)) { 
                        UserInfo userInfo = new UserInfo();
                        userInfo.userId = collaborator.Id;
                        userInfo.firstName = collaborator.Name;
                        userInfo.lastName = string.Empty;
                        userInfo.username = collaborator.Login;
                        userInfos.Add(userInfo);

                        RepositoryPermissions repositoryPermissions = collaborator.Permissions;

                        bool isAdmin = repositoryPermissions.Admin;
                        bool isPush = repositoryPermissions.Push;
                        bool isRead = repositoryPermissions.Pull;
                        VC_ACCESS_RIGHTS accessRights = VC_ACCESS_RIGHTS.NONE;

                        if (isRead) { accessRights = VC_ACCESS_RIGHTS.DOWNLOAD; }
                        if (isPush) { accessRights = VC_ACCESS_RIGHTS.UPLOAD; }

                        defaultRights.Add(accessRights);
                        adminRights.Add(isAdmin);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = "Error in retrieving project users.\n" + exceptionMessage;
                return false;
            }

        }

        /// <summary>
        /// Gets list of ids of the suspended users in an organisation
        /// </summary>
        /// <returns>List<long> which contains the id of the suspended users</long></returns>
        private List<long> GetSuspendedUsersIdList()
        {
            List<long> suspendedUsersIds = new List<long>();

            IReadOnlyList<User> orgUsers = _client.Organization.Member.GetAll(_organisationName).Result;
            if (orgUsers != null || orgUsers.Count() > 0)
            {
                var users = orgUsers.Select(user => _client.User.Get(user.Login));
                var suspendedUsers = Task.WhenAll<User>(users).Result.Where<User>(user => user.Suspended).ToList();

                foreach (Octokit.User suspendedUser in suspendedUsers)
                {
                    suspendedUsersIds.Add(suspendedUser.Id);
                }
            }

            return suspendedUsersIds;
        }

        /// <summary>
        /// Add a user to a repository (with certain rights) or update the user's rights
        /// </summary>
        /// <param name="projectId">The id of the project the user is going to be addede to</param>
        /// <param name="userName">The username of the user</param>
        /// <param name="adminRight">Whether the user will have admin rights</param>
        /// <param name="defaultRight">The default right for the user</param>
        /// <returns></returns>
        public bool addorUpdateUserToRepository(long projectId, string userName, bool adminRight, VC_ACCESS_RIGHTS defaultRight)
        {
            try
            {
                //Admin users should not be able to remove their own admin rights

                //The current you are trying to modify is the current user
                if (currentUser != null && currentUser.username == userName && _isCurrentUserCurrentProjectAdmin && !adminRight)
                {
                    errorMessage = "User cannot remove administrator rights for his/her own user.";
                    return false;

                }

                Permission permission = Permission.Pull;
                if (adminRight == true)
                {
                    permission = Permission.Admin;
                }
                else
                {
                    if (defaultRight.Equals(VC_ACCESS_RIGHTS.UPLOAD))
                    {
                        permission = Permission.Push;
                    }
                }
                CollaboratorRequest permissionRequest = new CollaboratorRequest(permission);
                _client.Repository.Collaborator.Add(projectId, userName, permissionRequest);
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
            return true;
        }


        /// <summary>
        /// Remove a user from a project.
        /// </summary>
        /// <param name="projectId">The Id of the project from which to remove the users.</param>
        /// <param name="userId">The user to remove from the project.</param>
        /// <returns>True if call is successful, false if call failed (error message can be accessed by GetErrorMessage).</returns>
        public bool RemoveUserFromProject(long projectId, long userId, string userName) { List<long> userIds = new List<long> { userId }; List<string> userNames = new List<string> { userName };
            return RemoveUsersFromProject(projectId, userIds, userNames); }
        bool RemoveUsersFromProject(long projectId, List<long> userIds, List<string> userNames)
        {
            if (currentUser != null) //current user is null if called from user-admin-plug-in
            {
                bool hasRight = HasCurrentUserProjectAdminRight(projectId);
                if (!hasRight)
                {
                    errorMessage = "Current user does not have admin rights for this project.";
                    return false;
                }
            }
            try
            {
                if (userIds.Count > 0)
                {
                    Repository repository = _client.Repository.Get(projectId).Result;
                    foreach (string userName in userNames)
                    {
                        if (currentUser != null && currentUser.username == userName)
                        {
                            errorMessage = "User cannot remove her/himself from project.";
                            return false;

                        }

                        _client.Repository.Collaborator.Delete(projectId, userName).Wait();

                        bool isCollaborator = _client.Repository.Collaborator.IsCollaborator(projectId, userName).Result;
                        if (isCollaborator)
                        {
                            errorMessage = "The  user " + userName + " could not be removed from this project.";
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");
                errorMessage = exceptionMessage;
                return false;
            }
        }

        #endregion User Related

        #region helper functions

        /// <summary>
        /// Create an MD5 hash for a given file. To be used for byte-to-byte comparisons with online versions.
        /// </summary>
        /// <param name="filename">The full path filename of the local file on your hard drive.</param>
        private static string getFileMD5Hash(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        /// <summary>
        /// Create an MD5 hash for a given unit. To be used for byte-to-byte comparisons with online versions.
        /// </summary>
        /// <param name="name">The name of the unit.</param>
        /// <param name="type">The type of the unit.</param>
        public string getUnitMD5Hash(VersionControlUnitInfo unit)
        {
            try
            {
                if (unit.UnitType != VC_FOLDER_TYPE.ADDON && unit.UnitType != VC_FOLDER_TYPE.CONFIG && unit.UnitType != VC_FOLDER_TYPE.COUNTRY && unit.UnitType != VC_FOLDER_TYPE.LOG && unit.UnitType != VC_FOLDER_TYPE.INPUT)
                    throw new Exception("Invalid unit type in getUnitMD5Hash.");
                string path = getLocalPath(unit);

                string fullMD5 = string.Empty;
                // if this local unit does not exist then return empty hash
                if (unit.UnitType == VC_FOLDER_TYPE.ADDON || unit.UnitType == VC_FOLDER_TYPE.COUNTRY || unit.UnitType == VC_FOLDER_TYPE.CONFIG)
                {
                    if (!File.Exists(Path.Combine(path, unit.Name + FILE_EXTENTION_MAIN))) return string.Empty;

                    fullMD5 = getFileMD5Hash(Path.Combine(path, unit.Name + FILE_EXTENTION_MAIN));

                    if (unit.UnitType == VC_FOLDER_TYPE.COUNTRY)
                        fullMD5 += "|" + getFileMD5Hash(Path.Combine(path, unit.Name + FILE_EXTENTION_DATACONFIG));

                    if (File.Exists(Path.Combine(path, unit.Name + FILE_EXTENTION_ICON)))
                        fullMD5 += "|" + getFileMD5Hash(Path.Combine(path, unit.Name + FILE_EXTENTION_ICON));
                }
                else
                {
                    if (unit.UnitType == VC_FOLDER_TYPE.LOG)
                    {
                        fullMD5 = getFileMD5Hash(Path.Combine(path, unit.Name + FILE_EXTENTION_LOG));
                    }
                    else if (unit.UnitType == VC_FOLDER_TYPE.INPUT)
                    {
                        foreach (string f in ALLOWED_INPUT_FILES)
                        {
                            string filename = Path.Combine(path, f);
                            if (File.Exists(filename))
                                fullMD5 += "|" + getFileMD5Hash(Path.Combine(path, f));
                        }

                        if (fullMD5 != string.Empty) fullMD5 = fullMD5.Substring(1);
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                return fullMD5;
            }
            catch
            {
                // on error, return empty hash
                return string.Empty;
            }
        }

        /// <summary>
        /// This methods rename the current's project folder to another name
        /// </summary>
        /// <param name="newFolderName">The new name for the current project's folder</param>
        /// <param name="newProjectPath">out: The complete path is stored in this variable</param>
        /// <returns>returns true if the folder has been renamed, false if it hasn't</returns>
        public bool RenameProjectFolder(string newFolderName, out string newProjectPath)
        {
            newProjectPath = string.Empty;
            if (Directory.Exists(projectPath)) {
                try
                {
                    DirectoryInfo currentDirectory = new DirectoryInfo(projectPath);
                    string parentPath = currentDirectory.Parent.FullName;
                    newProjectPath = (parentPath.EndsWith("\\") ? parentPath : string.Concat(parentPath, "\\")) + newFolderName;
                    Directory.Move(projectPath, newProjectPath);
                    projectPath = newProjectPath;
                }
                catch (Exception ex)
                {
                    newProjectPath = string.Empty;
                    return false;
                }

            }
            return true;
        }

        /// <summary>
        /// Builds the path to rename the current project's folder.
        /// If it exists, it adds the date
        /// </summary>
        /// <param name="newFolderInitialName">Suggested name for the project's folder</param>
        /// <param name="newFolderName">Final name for the project's folder</param>
        /// <param name="dateString"></param>
        /// <returns>True: if the suggested folder or suggestions folder and date can be used. False: No folder can be used.</returns>
        public bool buildPath(string newFolderInitialName, out string newFolderName)
        {
            bool emptyFolder = false;
            string newProjectPath = string.Empty;
            string dateString = string.Empty;
            newFolderName = string.Empty;
            try {
                if (Directory.Exists(projectPath))
                {

                    DirectoryInfo currentDirectory = new DirectoryInfo(projectPath);
                    string parentPath = currentDirectory.Parent.FullName;
                    newProjectPath = (parentPath.EndsWith("\\") ? parentPath : string.Concat(parentPath, "\\")) + newFolderInitialName;


                    if (!Directory.Exists(newProjectPath))
                    {
                        newFolderName = newFolderInitialName;
                        emptyFolder = true;
                    }

                    else
                    {
                        string date = DateTime.Now.ToString("ddMMMMyyyy_HHmmss");
                        newProjectPath = (parentPath.EndsWith("\\") ? parentPath : string.Concat(parentPath, "\\")) +
                            newFolderInitialName + "_" + date;
                        if (!Directory.Exists(newProjectPath))
                        {
                            newFolderName = newFolderInitialName + "_" + date;
                            emptyFolder = true;
                        }
                        else
                        {
                            newProjectPath = string.Empty;
                            newFolderName = string.Empty;
                            emptyFolder = false;
                        }
                    }

                }

                
            }catch(Exception e){
                newProjectPath = string.Empty;
                newFolderName = string.Empty;
                emptyFolder = false;

                return emptyFolder;
            }

            return emptyFolder;
        }


        #endregion helper functions

        #region notifications

        public const short NOTIFICATION_EMAIL = 1; //denotes that user wants to receive notification (on a specific event-type) by e-mail
        public const short NOTIFICATION_LOGIN = 2; //denotes that user wants to receive notification (on a specific event-type) at VC-log-in 
        public const short NOTIFICATION_NONE = 0;  //denotes that user does not want to receive notification (on a specific event-type)

        public const short NOTIFICATION_EVENTTYPE_UNIT = 0;
        public const short NOTIFICATION_EVENTTYPE_RELEASE = 1;
        public const short NOTIFICATION_EVENTTYPE_ACCESSRIGHTS = 2;
        public const short NOTIFICATION_SHOWINFO = 3;

        /// <summary>
        /// Get the user name to be displayed
        /// </summary>
        /// <param name="userInfo">The information about the user</param>
        /// <returns></returns>
        public static string GetPrettyUserName(UserInfo userInfo)
        {
            string description = "";
            if (userInfo.firstName != string.Empty) description += userInfo.firstName + " ";
            if (userInfo.lastName != string.Empty) description += userInfo.lastName + " ";
            if (userInfo.firstName + userInfo.lastName != string.Empty) description += " (";
            description += userInfo.username;
            if (userInfo.firstName + userInfo.lastName != string.Empty) description += ")";
            return description;
        }


        public bool SendProjectNotificationMail(VC_NOTIFICATION_TYPE nt, long projectId, string author, string releaseVersion = "", List<string> changes = null)
        {
            string subject = String.Empty;
            string content = String.Empty;
            bool notificationSent = false;

            switch (nt)
            {
                case VC_NOTIFICATION_TYPE.START_MERGE:
                    subject = author + " started a new online bundle.";
                    content = author + " started a new online bundle on project " + vc_projectInfo.ProjectName;
                    break;
                case VC_NOTIFICATION_TYPE.ABORT_MERGE:
                    subject = author + " aborted the creation of a new online bundle.";
                    content = author + " aborted the creation of a new online bundle on project " + vc_projectInfo.ProjectName;
                    break;
                case VC_NOTIFICATION_TYPE.FINISH_MERGE:
                    subject = author + " finished creating version " + releaseVersion + ".";
                    content = author + " finished creating version " + releaseVersion + " on project " + vc_projectInfo.ProjectName;
                    if (changes != null && changes.Count > 0)
                    {
                        content += Environment.NewLine + Environment.NewLine + "The new bundle contains the following changes:" + Environment.NewLine;
                        foreach (string change in changes) content += change + Environment.NewLine;
                    }
                    break;
                default:
                    throw new Exception("Unexpected error 958 in VCAPI.");  // this should never happen!
            }

            notificationSent = SendNotification(subject, content, projectId);
            return notificationSent;
        }



        private bool SendNotification(string subject, string content, long projectId)
        {
            Octokit.NewIssue newIssue = new NewIssue(subject) { Body = content };
            try
            {
                Issue issue = _client.Issue.Create(projectId, newIssue).Result;
                return true;
            }
            catch (Exception ex)
            {
                string exceptionMessage = ex.Message + ((ex.InnerException != null && ex.InnerException.Message != null) ? (" - " + ex.InnerException.Message) : "");

                errorMessage = "Could not get Merge info:\n" + exceptionMessage;
                return false;
            }

        }



        #endregion notifications

        #region proxy 

        /// <summary>
        /// Fills in the variables used for the proxy credentials.
        /// </summary>
        /// <param name="proxyUserName">Proxy username</param>
        /// <param name="proxyPassword">Proxy password</param>
        public void setProxyCredentials(string proxyUserName, string proxyPassword)
        {
            _proxyUserName = proxyUserName;
            _proxyPassword = proxyPassword;
        }

        /// <summary>
        /// Fills in the variables used for the proxy configuration.
        /// </summary>
        /// <param name="proxyURL">Proxy URL</param>
        /// <param name="proxyPort">Proxy Port</param>
        public void setProxyConfiguration(string proxyURL, string proxyPort, bool useProxy)
        {
            _proxyURL = proxyURL;
            _proxyPort = proxyPort;
            _useProxy = useProxy;
        }

        /// <summary>
        /// Checks if the proxy credentials have been provided
        /// </summary>
        /// <returns>Returns true if they have been provided</returns>
        public Boolean checkProxyCredentials()
        {
            if (String.IsNullOrEmpty(proxyUserName) || String.IsNullOrEmpty(proxyPassword))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates the IWebProxy object using the proxy username, proxy password, proxy URL and proxy port.
        /// </summary>
        /// <returns>Returns the IWebProxy object</returns>
        private static IWebProxy setProxyConfiguration()
        {
            IWebProxy defaultProxy = WebRequest.GetSystemWebProxy();
            ICredentials credentials = new NetworkCredential(_proxyUserName, _proxyPassword);
            Uri proxyURI = null;

            if (String.IsNullOrEmpty(_proxyURL) || String.IsNullOrEmpty(_proxyURL))
            {
                proxyURI = new Uri("http://empty:80");
            }
            else
            {
                proxyURI = new Uri(string.Format("{0}:{1}", _proxyURL, _proxyPort));
            }

            IWebProxy proxy = new WebProxy(proxyURI);
            proxy.Credentials = credentials;
            return proxy;

        }



        #endregion proxy

        #region server
        public void setServerConfiguration(string serverURL, string serverName, string orgName, string encryptKey, string authorName, 
            string encryptedUsername, out string userName,
            string encryptedPassword, out string password)
        {
            _gitHubURL = serverURL;
            _organisationName = orgName;
            _authorName = authorName;
            _encryptKey = encryptKey;
            userName = CryptVCIdentification(encryptedUsername, true);
            password = CryptVCIdentification(encryptedPassword, true);

        }

        public bool updateUserNameToXMLFile(string userName, string organizationID)
        {
            string encryptedUsername = CryptVCIdentification(userName, false);
            return VCSettingsInfo.updateUserNameToXMLFile(encryptedUsername, organizationID);
        }

        public bool updatePasswordToXMLFile(string password, string organizationID)
        {
            string encryptedPassword = CryptVCIdentification(password, false);
            return VCSettingsInfo.updatePasswordToXMLFile(encryptedPassword, organizationID);
        }

        public string CryptVCIdentification(string userName, bool de)
        {
            string encDecUserName = string.Empty;

            if (!String.IsNullOrEmpty(userName))
            {
                try
                {
                    List<byte> passkey = new List<byte>();
                    foreach (char c in _encryptKeyUserName.ToCharArray()) passkey.Add((byte)c);

                    if (de)
                    {
                        encDecUserName = SimpleCrypt.SimpleDecrypt(userName, passkey.ToArray());

                    }
                    else
                    {
                        encDecUserName = SimpleCrypt.SimpleEncrypt(userName, passkey.ToArray());
                    }
                }
                catch (Exception exception) { errorMessage = exception.Message; }
            }

            return encDecUserName;
        }
        #endregion server

    }

}
