using System;
using System.Collections.Generic;
using System.Text;

namespace EM_XmlHandler.VersionControl
{
    public class VCServersInfo
    {
        #region variables
        public string serverName;
        public string serverURL;
        public List<VCOrganizationInfo> organizations;
        public string authorName;
        public string encryptKey;
        public string encryptedUserName;
        public string userName;
        public string encryptedPassword;
        public string password;
        #endregion variables

        public VCServersInfo(string serverName, string serverURL, string authorName, string encryptKey, List<VCOrganizationInfo> organizations, string encryptedUserName, string encryptedPassword)
        {
            this.serverName = serverName;
            this.serverURL = serverURL;
            this.organizations = organizations;
            this.authorName = authorName;
            this.encryptKey = encryptKey;
            this.encryptedUserName = encryptedUserName;
            this.encryptedPassword = encryptedPassword;
        }
    }

    public class VCOrganizationInfo
    {
        #region variables
        public string organizationName;
        public string organizationID;

        #endregion variables
        public VCOrganizationInfo(string organizationName, string organizationID)
        {
            this.organizationName = organizationName;
            this.organizationID = organizationID;
        }
        
    }
}
