using EM_Common;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace EM_Crypt
{
    public static class SecureInfo
    {
        public static bool IsSecure { get; private set; } = false;
        public static bool LockProject { get; private set; } = false;
        public static string DataPassword { get; private set; } = string.Empty;
        private static string _encryptKeySecureSettings = "sfhjlk2350gj34itrsd02r34fd6s3hi0";

        // This function reads the secure settings from SecureSettings.xml in the user settings 
        public static void ReadSecureInfo()
        {
            string path = Path.Combine(EnvironmentInfo.GetUserSettingsFolder(), "SecureSettings.xml");
            if (File.Exists(path))
            {
                try
                {
                    using (XmlReader xmlReader = XmlReader.Create(path, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                    {
                        // first element must always be the SecureSettings! so read until you find it
                        while (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "SecureSettings")
                            if (!xmlReader.Read()) { return; }
                        // try to read the SecureSettings element
                        XElement xeInfo = XElement.ReadFrom(xmlReader) as XElement;
                        if (xeInfo == null || xeInfo.Name != "SecureSettings") return;
                        // read the secure settings
                        foreach (XElement xe in xeInfo.Elements())
                        {
                            if (xe.Value == null) continue;
                            switch (xe.Name == null ? string.Empty : xe.Name.ToString())
                            {
                                case "LockProject": if (bool.TryParse(xe.Value, out bool b1)) LockProject = b1; else return; break;
                                case "DataPassword": DataPassword = xe.Value; break;
                            }
                        }
                        // if there is a password, decrypt it
                        if (!string.IsNullOrEmpty(DataPassword)) DataPassword = DecryptDataPassword(DataPassword);
                        // if all settings were read sucessfully, turn the security features 
                        IsSecure = true;
                    }
                }
                catch { }
            }
        }

        public static string DecryptDataPassword(string password)
        {
            return SimpleCrypt.SimpleDecryptWithPassword(password, _encryptKeySecureSettings);
        }

        public static string EncryptDataPassword(string password)
        {
            return SimpleCrypt.SimpleEncryptWithPassword(password, _encryptKeySecureSettings);
        }
    }
}
