using EM_XmlHandler.VersionControl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Dialogs
{
    public partial class VCSettings : Form
    {
        VCAdministrator _vcAdministrator = null;
        List<VCServersInfo> _servers = null;

        public VCSettings()
        {
            _vcAdministrator = EM_AppContext.Instance.GetVCAdministrator();
            InitializeComponent();
        }

        private void VCSettings_Load(object sender, EventArgs e)
        {
            //Proxy
            string proxyURLText = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCProxyURL;
            string proxyPortText = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCProxyPort;
            bool useProxy = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCUseProxy;
            proxyURL.Text = proxyURLText;
            proxyPort.Text = proxyPortText;

            if (useProxy)
            {
                proxyURL.ReadOnly = false;
                proxyPort.ReadOnly = false;
                radioBtnYesProxy.Checked = true;
            }
            else
            {
                proxyURL.ReadOnly = true;
                proxyPort.ReadOnly = true;
                radioBtnNoProxy.Checked = true;
            }

            //Server
            string organisationID = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCOrganizationID;

            VCSettingsInfo settingsInfo = VCSettingsInfo.GetVCSettingsInfo();
            if (settingsInfo != null)
            {
                _servers = settingsInfo.servers;

                int totalOrgs = 0;
                foreach (VCServersInfo server in _servers)
                {
                    totalOrgs = totalOrgs + server.organizations.Count;
                }

                System.Windows.Forms.RadioButton[] radioButtons = new System.Windows.Forms.RadioButton[totalOrgs];
                int i = 0;
                int verticalPosition = 19;
                int horizontalPosition = 6;
                foreach (VCServersInfo server in _servers)
                {
                    Label txt = new Label();
                    txt.Text = server.serverName;
                    txt.Location = new Point(horizontalPosition, verticalPosition);
                    txt.Font = new Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline);
                    txt.AutoSize = true;
                    groupBox2.Controls.Add(txt);
                    verticalPosition = verticalPosition + 20;

                    List<VCOrganizationInfo> organizations = server.organizations;
                    foreach (VCOrganizationInfo organization in organizations)
                    {
                        radioButtons[i] = new RadioButton();
                        radioButtons[i].Text = organization.organizationName;
                        radioButtons[i].Tag = organization.organizationID;
                        radioButtons[i].Location = new Point(horizontalPosition, verticalPosition);

                        if ((organization.organizationID.Equals(organisationID) || totalOrgs == 1))
                        {
                            radioButtons[i].Checked = true;
                        }

                        groupBox2.Controls.Add(radioButtons[i]);
                        i++;
                        verticalPosition = verticalPosition + 23;
                    }
                    verticalPosition = verticalPosition + 10;
                }
            }
            else //If the user settings cannot be taken, the user will receive a message here
            {
                int verticalPosition = 19;
                int horizontalPosition = 6;
                RichTextBox txt = new RichTextBox();
                txt.Text = "The VC settings file cannot be loaded or it is not correct, please contact an administrator user.";
                txt.Location = new Point(horizontalPosition, verticalPosition);
                txt.Width = 450;
                txt.ReadOnly = true;
                txt.BorderStyle = System.Windows.Forms.BorderStyle.None;
                groupBox2.Controls.Add(txt);
            }
        }

        private void btnOK_Click_1(object sender, EventArgs e)
        {
            //Proxy
            String proxyURLText = proxyURL.Text.Trim();
            String proxyPortText = proxyPort.Text.Trim();
            bool useProxy = radioBtnYesProxy.Checked;
            bool saveProxyConfigurationAndClose = false;
            bool saveServerConfigurationAndClose = false;

            if (!useProxy)
            {
                saveProxyConfigurationAndClose = true;
            }
            else
            {
                if (String.IsNullOrEmpty(proxyURLText) && String.IsNullOrEmpty(proxyPortText))
                {
                    MessageBox.Show("You haven't provided any values for proxy configuration. Please, provide proxy information or select 'Do not use proxy configuration'.");
                    saveProxyConfigurationAndClose = false;
                }
                else if ((String.IsNullOrEmpty(proxyURLText) && !String.IsNullOrEmpty(proxyPortText)) || (!String.IsNullOrEmpty(proxyURLText) && String.IsNullOrEmpty(proxyPortText)))
                {
                    MessageBox.Show("Both fields (proxy URL and proxy port) are needed to configure the proxy.");
                }
                else if (!Uri.IsWellFormedUriString(proxyURLText, UriKind.Absolute))
                {
                    MessageBox.Show("You need to insert a valid proxy URL (starting with http://) in the proxy URL field.");
                }
                else if (!int.TryParse(proxyPortText, out int n))
                {
                    MessageBox.Show("You need to insert a numberic value in proxy port.");
                }
                else
                {
                    saveProxyConfigurationAndClose = true;
                }
            }

            //Server
            var selectedRadioButton = groupBox2.Controls.OfType<RadioButton>().Where(rb => rb.Checked).FirstOrDefault();
            VCServersInfo selectedServer = null;
            string orgID = string.Empty;
            string orgName = string.Empty;


            if (selectedRadioButton == null)
            {
                MessageBox.Show("You have to select and organization to connect to.");
                saveServerConfigurationAndClose = false;
            }
            else
            {
                orgName = selectedRadioButton.Text;
                orgID = selectedRadioButton.Tag.ToString();
                selectedServer = VCSettingsInfo.getVCServerInfoByOrganizationID(orgID);
                saveServerConfigurationAndClose = true;

            }
            
            if (saveProxyConfigurationAndClose && saveServerConfigurationAndClose)
            { 
                //Proxy
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCProxyURL = proxyURL.Text;
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCProxyPort = proxyPort.Text;
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCUseProxy = radioBtnYesProxy.Checked;

                //Server
                EM_AppContext.Instance.GetUserSettingsAdministrator().Get().VCOrganizationID = orgID;

                EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(true);

                _vcAdministrator.setProxyConfiguration(proxyURL.Text, proxyPort.Text, radioBtnYesProxy.Checked);

                string userName = string.Empty; string password = string.Empty;
                _vcAdministrator.setServerConfiguration(selectedServer.serverURL, selectedServer.serverName, orgName, selectedServer.encryptKey, selectedServer.authorName, selectedServer.encryptedUserName, out userName, selectedServer.encryptedPassword, out password);

                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();

            }

        }

        private void radioBtnNoProxy_CheckedChanged(object sender, EventArgs e)
        {
            bool useProxy = radioBtnYesProxy.Checked;
            if (useProxy)
            {
                proxyURL.ReadOnly = false;
                proxyPort.ReadOnly = false;
            }
            else
            {
                proxyURL.ReadOnly = true;
                proxyPort.ReadOnly = true;
            }
            
        }

        private void radioBtnYesProxy_CheckedChanged(object sender, EventArgs e)
        {
            bool useProxy = radioBtnYesProxy.Checked;
            if (useProxy)
            {
                proxyURL.ReadOnly = false;
                proxyPort.ReadOnly = false;
            }
            else
            {
                proxyURL.ReadOnly = true;
                proxyPort.ReadOnly = true;
            }
        }
    }
}
