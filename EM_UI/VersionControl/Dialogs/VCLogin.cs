using EM_Common_Win;
using EM_UI.Tools;
using EM_XmlHandler.VersionControl;
using System;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCLogin : Form
    {
        VCAdministrator _vcAdministrator = null;
        string _previousUserName = string.Empty;
        string _previousPassword = string.Empty;
        string _organizationID = string.Empty;

        internal VCLogin(VCAdministrator vcAdministrator)
        {
            _vcAdministrator = vcAdministrator;
            InitializeComponent();
        }

        void VCLogin_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
            string userName = string.Empty; string password = string.Empty;  string organizationID = string.Empty;
            bool proccedLoginServer = handleServerConfiguration(out userName, out password, out organizationID);
            
            if (!proccedLoginServer){Close();}

            _previousUserName = userName;
            _previousPassword = password;
            _organizationID = organizationID;

            txtUserName.Text = userName;
            txtPassword.Text = password;

            this.ActiveControl = txtUserName;

            if (txtUserName.Text != string.Empty)
                this.ActiveControl = txtPassword;

            if (txtPassword.Text != string.Empty)
            {
                this.rememberPasswordCheckbox.Checked = true;    
                this.ActiveControl = btnOK;
            }
                
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            
            string insertedUserNameorEmail = txtUserName.Text.Trim().ToLower();
            string insertedPassword = txtPassword.Text;
            bool savePassword = rememberPasswordCheckbox.Checked;

            if (insertedUserNameorEmail.Equals(String.Empty) && insertedPassword.Equals(String.Empty))
            {
                UserInfoHandler.ShowError("You have to insert a username and a password to connect.");
                return;
            }
            else if (insertedUserNameorEmail.Equals(String.Empty))
            {
                UserInfoHandler.ShowError("You have to insert a username to connect.");
                return;
            }
            else if (insertedPassword.Equals(String.Empty))
            {
                UserInfoHandler.ShowError("You have to insert a password to connect.");
                return;
            }
            else
            {
                Cursor = Cursors.WaitCursor;

                bool proceedLoginProxy = handleProxy();

                //This was already checked before opening the dialog
                //bool proccedLoginServer = handleServerConfiguration(out previousUserName, out organizationID);
               
                if (proceedLoginProxy)
                {
                    string userName = string.Empty;
                    bool success = _vcAdministrator.LogIn(insertedUserNameorEmail, insertedPassword, out userName);

                    if (!success)
                    {
                        Cursor = Cursors.Default;
                        return;
                    }
                    DialogResult = System.Windows.Forms.DialogResult.OK;

                    if (!_previousUserName.Equals(userName))
                    {
                        DialogResult result = MessageBox.Show("Would you like to save/update the username for the current server?", "Update username", MessageBoxButtons.YesNo);
                        if(result == DialogResult.Yes)
                        {
                            //The new username is written to the XML file
                            bool updatedUsername = _vcAdministrator.updateUserNameToXMLFile(userName, _organizationID);
                            if (!updatedUsername)
                            {
                                UserInfoHandler.ShowError("Your username could not be saved.");
                            }
                        }
                    }

                    //If the password is different from the one stored and the user ticks the checkbox
                    if (!_previousPassword.Equals(insertedPassword) && savePassword)
                    {
                        bool updatedPassword = _vcAdministrator.updatePasswordToXMLFile(insertedPassword, _organizationID);
                        if (!updatedPassword){
                            UserInfoHandler.ShowError("Your password could not be saved.");
                        }

                    }
                    else if (!string.IsNullOrEmpty(_previousPassword) && !savePassword) //To forget the password
                    {
                        bool updatedPassword = _vcAdministrator.updatePasswordToXMLFile(string.Empty, _organizationID);
                        if(!updatedPassword){
                            UserInfoHandler.ShowError("Your password could not be saved.");
                        }
                    }
                }
                
                else { 
                    DialogResult = System.Windows.Forms.DialogResult.Cancel;
                };

                //This script uploads the projects to the GitHub VC, the path needs to be  configured in VCAPI
                //_vcAdministrator.launchScript();
                Cursor = Cursors.Default;
                Close();
            }

        }

        bool handleProxy()
        {
            bool proceedLogin = true;
            bool useProxy = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCUseProxy;

            if (useProxy)
            {
                String proxyURL = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCProxyURL;
                String proxyPort = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCProxyPort;

                //This should never happen because the user cannot select "useproxy" without inserting the URL and port
                if(proxyURL == null || proxyURL.Equals("") || proxyPort == null || proxyPort.Equals(""))
                {
                    proceedLogin = false;
                    UserInfoHandler.ShowError("You have to insert a proxy URL and port to connect.");
                }

                _vcAdministrator.setProxyConfiguration(proxyURL, proxyPort, useProxy);

                DialogResult proxyCredentiasResult = DialogResult.OK;
                //While the user clicks OK without inserting proxy credentials
                while (proxyCredentiasResult.Equals(DialogResult.OK) & !_vcAdministrator.checkProxyCredentials())
                {
                    VCProxy loginDialog = new VCProxy();
                    proxyCredentiasResult = loginDialog.ShowDialog();
                }

                //if the user clicks on Cancel without inserting the proxy credentials, the Log in is cancelled
                if (proxyCredentiasResult.Equals(DialogResult.Cancel))
                {
                    proceedLogin = false;
                }
            }

            return proceedLogin;
        }

        bool handleServerConfiguration(out string  userName, out string password, out string organizationID)
        {
            bool proceedLogin = true;
            userName = string.Empty; password = string.Empty;  organizationID = string.Empty;
            string serverURL = string.Empty; string serverName = string.Empty; string organisation = string.Empty; string author = string.Empty; 
            string encryptKey = string.Empty; string encryptedUserName = string.Empty; string encryptedPassword = string.Empty;
            getServerConfiguration(out serverURL, out serverName, out organisation, out author, out encryptKey, out encryptedUserName, out encryptedPassword, out organizationID);

            if(string.IsNullOrEmpty(serverURL) || string.IsNullOrEmpty(organisation) || string.IsNullOrEmpty(author) || string.IsNullOrEmpty(encryptKey)) {

                proceedLogin = false;
                UserInfoHandler.ShowInfo("You need to select a server from the 'VC settings' menu before proceeding.");

                VCSettings vcSettingsDialog = new VCSettings();
                DialogResult result = vcSettingsDialog.ShowDialog();

                if (result == DialogResult.OK)
                {
                    proceedLogin = true;
                    getServerConfiguration(out serverURL, out serverName, out organisation, out author, out encryptKey, out encryptedUserName, out encryptedPassword, out organizationID);
                }

            }
            
            if(proceedLogin)
            {
                _vcAdministrator.setServerConfiguration(serverURL, serverName, organisation, encryptKey, author, encryptedUserName, out userName, encryptedPassword, out password);
            }

            return proceedLogin;

        }

        /// <summary>
        /// Gets the server configuration using the organizationID stored in the UserSettings file
        /// </summary>
        /// <param name="serverURL">out: URL of the gitHub server to which the organisation belongs to</param>
        /// <param name="serverName">out: Name of the gitHub server to which the organisation belongs to</param>
        /// <param name="organizationName">out: Name of the organisation</param>
        /// <param name="author">out: author configured for that sever</param>
        /// <param name="encryptKey">out: encrypted key configured for that server</param>
        /// <param name="encryptedUserName">out: encrypted username</param>
        void getServerConfiguration(out string serverURL, out string serverName, out string organizationName, out string author, out string encryptKey, out string encryptedUserName, out string encryptedPassword, out string organizationID)
        {

            //The organizationID is obtained from the UserSetting file
            organizationID = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCOrganizationID;
            serverURL = string.Empty; serverName = string.Empty; organizationName = string.Empty; author = string.Empty; encryptKey = string.Empty; encryptedUserName = string.Empty; encryptedPassword = string.Empty;

            if (!String.IsNullOrEmpty(organizationID))
            {
                //The file is read
                VCServersInfo serverInfo = VCSettingsInfo.getVCServerInfoByOrganizationID(organizationID);
                VCOrganizationInfo organization = null;
                string selectedOrganizationID = organizationID;

                if (serverInfo != null)
                {
                    serverURL = serverInfo.serverURL;
                    serverName = serverInfo.serverName;
                    organization = serverInfo.organizations.FirstOrDefault(o => o.organizationID == selectedOrganizationID);
                    author = serverInfo.authorName;
                    encryptKey = serverInfo.encryptKey;
                    encryptedUserName = serverInfo.encryptedUserName;
                    encryptedPassword = serverInfo.encryptedPassword;
                }

                if (organization != null)
                {
                    organizationName = organization.organizationName;
                }
                else
                {
                    organizationName = string.Empty;
                }

            }
           
        }

    }
}
