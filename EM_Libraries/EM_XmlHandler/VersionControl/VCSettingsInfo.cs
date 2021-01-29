using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using EM_Common;
using System.Xml;
using System.Linq;

namespace EM_XmlHandler.VersionControl
{
    public class VCSettingsInfo
    {
        #region constants
        const string SERVER_SETTINGS_NAME = "VCSettings.xml";
        #endregion constants

        #region variables
        public List<VCServersInfo> servers;
        string encryptedUserName = string.Empty;

        #endregion variables

        public static VCSettingsInfo GetVCSettingsInfo()
        {

            VCSettingsInfo vcSettingsInfo = new VCSettingsInfo();
            List<VCServersInfo> serversInfo = new List<VCServersInfo>();
            string encryptedUserName = string.Empty;
            try
            {

                string path = GetCurrentSettingsFullName();
                using (StreamReader sr = new StreamReader(path, Encoding.UTF8))
                using (XmlReader reader = XmlReader.Create(sr))
                {
                    serversInfo = readVCSettingsXML(reader);
                }

            }
            catch (Exception ex)
            {
                string ms = ex.Message;
            }

            vcSettingsInfo.servers = serversInfo;
            vcSettingsInfo.encryptedUserName = encryptedUserName;
            return vcSettingsInfo;
        }

        internal static string GetCurrentSettingsFullName() { return Path.Combine(EnvironmentInfo.GetUserSettingsFolder(), SERVER_SETTINGS_NAME); }

        internal static List<VCServersInfo> readVCSettingsXML(XmlReader reader)

        {
            List<VCServersInfo> vcServersList = new List<VCServersInfo>();

            bool insideServer = false;
            bool insideOrganization = false;

            string name = string.Empty; string URL = string.Empty; string authorName = string.Empty; string encryptKey = string.Empty; string encryptedUserName = string.Empty; string encryptedPassword = string.Empty;
            string orgName = string.Empty; string orgID = string.Empty;
            List<VCOrganizationInfo> organizationsList = new List<VCOrganizationInfo>();

            do
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == TAGS_VC.SERVER && !insideServer) // e.g. <SYS> or <COUNTRY>
                {
                    insideServer = true;
                }

                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == TAGS_VC.SERVER && insideServer) // e.g. </SYS> or </COUNTRY>
                {
                    insideServer = false;
                    VCServersInfo vcServerSingle = new VCServersInfo(name, URL, authorName, encryptKey, organizationsList, encryptedUserName, encryptedPassword);
                    vcServersList.Add(vcServerSingle);

                    name = string.Empty; URL = string.Empty; authorName = string.Empty; encryptKey = string.Empty; encryptedUserName = string.Empty;
                    orgName = string.Empty; orgID = string.Empty; ;
                    organizationsList = new List<VCOrganizationInfo>();
                }

                if (reader.NodeType == XmlNodeType.Element && reader.Name == TAGS_VC.ORGANIZATION && insideServer && !insideOrganization)
                {
                    insideOrganization = true;
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == TAGS_VC.ORGANIZATION && insideServer && insideOrganization)
                {
                    insideOrganization = false;
                    VCOrganizationInfo organization = new VCOrganizationInfo(orgName, orgID);
                    organizationsList.Add(organization);
                    orgName = string.Empty; orgID = string.Empty; ;
                }

                if (insideServer && !insideOrganization)
                {
                    switch (reader.Name)
                    {
                        case TAGS_VC.SERVER_NAME:
                            name = reader.ReadInnerXml();
                            break;
                        case TAGS_VC.SERVER_URL:
                            URL = reader.ReadInnerXml();
                            break;
                        case TAGS_VC.SERVER_AUTHOR_NAME:
                            authorName = reader.ReadInnerXml();
                            break;
                        case TAGS_VC.SERVER_ENCRYPT_KEY:
                            encryptKey = reader.ReadInnerXml();
                            break;
                        case TAGS_VC.SERVER_USER_NAME:
                            encryptedUserName = reader.ReadInnerXml();
                            break;
                        case TAGS_VC.SERVER_PASSWORD:
                            encryptedPassword = reader.ReadInnerXml();
                            break;
                        default:
                            //Console.WriteLine("Default case");
                            break;
                    }
                }
                else if (insideServer && insideOrganization)
                {
                    switch (reader.Name)
                    {
                        case TAGS_VC.ORGANIZATION_NAME:
                            orgName = reader.ReadInnerXml();
                            break;
                        case TAGS_VC.ORGANIZATION_ID:
                            orgID = reader.ReadInnerXml();
                            break;
                        default:
                            //Console.WriteLine("Default case");
                            break;
                    }
                }

            } while (reader.Read());

            return vcServersList;
        }

        public static VCServersInfo getVCServerInfoByOrganizationID(string organizationID)
        {
            VCSettingsInfo settingsInfo = GetVCSettingsInfo();
            List<VCServersInfo> vcServers = null;
            VCServersInfo server = null;

            if (settingsInfo != null)
            {
                vcServers = settingsInfo.servers;
            }

            if (vcServers != null)
            {
                server = vcServers.FirstOrDefault(vcServer => vcServer.organizations.Any(organizations => organizations.organizationID == organizationID));
            }
            return server;
        }

        public static bool updateUserNameToXMLFile(string userName, string organizationID)
        {
            try
            {

                string path = GetCurrentSettingsFullName();
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                bool foundOrganization = false;
                bool foundUserName = false;
                bool found = false;

                XmlNodeList xmlNodelist = doc.DocumentElement.ChildNodes;
                foreach (XmlNode node in xmlNodelist)
                {
                    if (node.Name.Equals(TAGS_VC.SERVERS))
                    {
                        XmlNodeList servers = node.ChildNodes;
                        foreach (XmlNode server in servers)
                        {
                            if (!found)
                            {
                                XmlNode userNameNode = null;
                                foundUserName = false;
                                foundOrganization = false;

                                XmlNodeList serverElements = server.ChildNodes;
                                foreach (XmlNode serverElement in serverElements)
                                {
                                    if (!foundUserName || !foundOrganization)
                                    {

                                        if (serverElement.Name.Equals(TAGS_VC.SERVER_USER_NAME))
                                        {
                                            userNameNode = serverElement;
                                            foundUserName = true;
                                        }
                                        if (serverElement.Name.Equals(TAGS_VC.ORGANIZATIONS) && !foundOrganization)
                                        {
                                            XmlNodeList serverOrganisations = serverElement.ChildNodes;
                                            foreach (XmlNode serverOrganisation in serverOrganisations)
                                            {
                                                if (serverOrganisation.FirstChild.InnerText.Equals(organizationID))
                                                {
                                                    foundOrganization = true;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (foundUserName && foundOrganization)
                                {
                                    found = true;
                                    userNameNode.InnerText = userName;
                                }
                            }

                        }
                    }
                }
                if (found)
                {
                    doc.Save(path);
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                string ms = ex.Message;
                return false;
            }
        }

        public static bool updatePasswordToXMLFile(string password, string organizationID)
        {
            try
            {

                string path = GetCurrentSettingsFullName();
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                bool foundOrganization = false;
                bool foundPassword = false;
                bool found = false;

                XmlNodeList xmlNodelist = doc.DocumentElement.ChildNodes;
                foreach (XmlNode node in xmlNodelist)
                {
                    if (node.Name.Equals(TAGS_VC.SERVERS))
                    {
                        XmlNodeList servers = node.ChildNodes;
                        foreach (XmlNode server in servers)
                        {
                            if (!found)
                            {
                                XmlNode passwordNode = null;
                                foundPassword = false;
                                foundOrganization = false;

                                XmlNodeList serverElements = server.ChildNodes;
                                foreach (XmlNode serverElement in serverElements)
                                {
                                    if (!foundPassword || !foundOrganization)
                                    {

                                        if (serverElement.Name.Equals(TAGS_VC.SERVER_PASSWORD))
                                        {
                                            passwordNode = serverElement;
                                            foundPassword = true;
                                        }
                                        if (serverElement.Name.Equals(TAGS_VC.ORGANIZATIONS) && !foundOrganization)
                                        {
                                            XmlNodeList serverOrganisations = serverElement.ChildNodes;
                                            foreach (XmlNode serverOrganisation in serverOrganisations)
                                            {
                                                if (serverOrganisation.FirstChild.InnerText.Equals(organizationID))
                                                {
                                                    foundOrganization = true;
                                                }
                                            }
                                        }
                                    }

                                }

                                if (foundPassword && foundOrganization)
                                {
                                    found = true;
                                    passwordNode.InnerText = password;
                                }
                            }

                        }
                    }
                }
                if (found)
                {
                    doc.Save(path);
                    return true;
                }

                return false;

            }
            catch (Exception ex)
            {
                string ms = ex.Message;
                return false;
            }
        }
    }

}

    
