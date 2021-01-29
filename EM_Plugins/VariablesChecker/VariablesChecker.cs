using EM_Common;
using EM_Common_Win;
using System;
using System.IO;
using System.Windows.Forms;

namespace VariablesChecker
{
    public class VariablesChecker : PiInterface
    {
        internal static VarConfig varData = null;

        public override string GetTitle()
        {
            return "Variables Checker";
        }

        public override string GetDescription()
        {
            return "This Plug-in provides methods for validating the EUROMOD variables file.";
        }

        public override string GetFullFileName()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }

        public override bool IsWebApplicable()
        {
            return false;
        }

        public override void Run(System.Collections.Generic.Dictionary<string, object> arguments = null)
        {
            try
            {
                string varPath = Path.Combine(UISessionInfo.GetEuromodFilesFolder(), @"XMLParam\Config\VarConfig.xml");
                varData = new VarConfig();
                using (StreamReader streamReader = new StreamReader(varPath, DefGeneral.DEFAULT_ENCODING)) varData.ReadXml(streamReader);
                (new VariablesCheckerForm()).ShowDialog();
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }

        internal static void SaveVar()
        {
            varData.AcceptChanges();
            string varPath = Path.Combine(UISessionInfo.GetEuromodFilesFolder(), @"XMLParam\Config\VarConfig.xml");
            Stream fileStream = new FileStream(varPath, FileMode.Create);
            using (XmlTextCDATAWriter xmlWriter = new XmlTextCDATAWriter(fileStream, DefGeneral.DEFAULT_ENCODING,
                   new string[] { "LongName", "ShortName", "Name", "Description", "AutoLabel", "Label", "NamePattern" }))
                varData.WriteXml(xmlWriter);
        }
    }
}
