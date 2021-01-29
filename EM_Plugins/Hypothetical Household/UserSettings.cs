using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace HypotheticalHousehold
{
    internal class UserSettings 
    {
        private static string HOT_USER_SETTINGS = "HHotUserSettings.xml";

        internal HHoTUserSettings settings = new HHoTUserSettings();
        private bool notYetRead = true;

        private void Read()
        {
            notYetRead = false;
            try
            {
                string fileName = Path.Combine(GetUserSettingsFolder(), HOT_USER_SETTINGS);
                if (!File.Exists(fileName)) return; // no user settings yet

                using (XmlReader xmlReader = XmlReader.Create(fileName, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(HHoTUserSettings));
                    try
                    {
                        settings = (HHoTUserSettings)xs.Deserialize(xmlReader);
                    }
                    catch   // most likely, it's old format, so just ignore and start new settings...
                    {
                        settings = new HHoTUserSettings();
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to read HHOT user settings." + Environment.NewLine + exception.Message);
            }
        }

        internal void Write()
        {
            try
            {
                if (!Directory.Exists(GetUserSettingsFolder())) Directory.CreateDirectory(GetUserSettingsFolder());

                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings() { Indent = true };
                using (XmlWriter writer = XmlWriter.Create(Path.Combine(GetUserSettingsFolder(), HOT_USER_SETTINGS), xmlWriterSettings))
                {
                    XmlSerializer xs = new XmlSerializer(typeof(HHoTUserSettings));
                    xs.Serialize(writer, settings);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show("Failed to write HHOT user settings." + Environment.NewLine + exception.Message);
            }
        }

        private string GetUserSettingsFolder()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EUROMOD");
        }

        internal string GetLastOpenProjectPath()
        {
            if (notYetRead) Read();
            return settings.lastOpenProject;
        }
    }

    public class HHoTUserSettings
    {
        public string lastOpenProject = string.Empty;
        public string selectedYears = string.Empty;
        public string selectedCounries = string.Empty;
        public WizardSettings wizardSettings = new WizardSettings();
        public class WizardSettings
        {
            public Program.HHoT_TEMPLATE_TYPE templateType = Program.HHoT_TEMPLATE_TYPE.BUDGET_CONSTRAINTS;
            public List<string> householdTypes = new List<string>();
            public string householdMember = string.Empty;
            public RangeSettings rangeSettings = new RangeSettings();
            public bool outputInEuro = true;

            public class RangeSettings
            {
                public Program.HHoT_INCOME_RANGE rangeType = Program.HHoT_INCOME_RANGE.RANGE_INCOME_WAGE;
                public string rangeValueFrom = string.Empty;
                public string rangeValueTo = string.Empty;
                public string rangeValueStep = string.Empty;
                public string fixedValue = string.Empty;
            }
        }
    }
}
