using EM_Common;
using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace EM_UI.Tools
{
    internal class BrandHandler
    {
        private const string BRAND_ELEMENT_PROJECT_NAME = "ProjectName";
        private const string BRAND_BACKGROUND_IMAGE_PREFIX = "brand_background_";
        private const string BRAND_ICON_PREFIX = "brand_icon_";

        internal static void PrepareBrand(EM_UI_MainForm emptyForm)
        {
            try
            {
                string brandName = GetProjectNameOrDefault();
                string brandTitle = Properties.Resources.ResourceManager.GetString("brand_title_" + brandName);

                if (String.IsNullOrEmpty(brandTitle))
                {
                    brandTitle = brandName = "EUROMOD";
                    // For now, completely go silent if the brand is wrong. To warn, uncomment the line below:
                    //UserInfoHandler.ShowError($"File '{projectSettingsFullPath}' does not contain a valid Project Name.{Environment.NewLine}Alternative brand is ignored.");
                }

                DefGeneral.BRAND_TITLE = brandTitle;
                DefGeneral.BRAND_NAME = brandName;
                SetBackgroundImage(emptyForm);
                SetIcon(emptyForm);
                emptyForm.btnRun.Caption = $"Run{Environment.NewLine}{DefGeneral.BRAND_TITLE}";
                emptyForm.barText_PoweredBy.Caption = DefGeneral.IsAlternativeBrand() ? GetPoweredByText() : string.Empty;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Adapting to alternative Brand failed!", false);
            }
        }

        // this reads the ProjectSettings.xml if it exists and returns the ProjectName field if it exists, or null otherwise
        private static string GetProjectNameOrDefault()
        {
            string brandName = null;
            string projectSettingsFullPath = new EMPath(EM_AppContext.FolderEuromodFiles).GetProjectSettingsPath(true);
            if (File.Exists(projectSettingsFullPath))
            {
                try
                {
                    using (XmlReader xmlReader = XmlReader.Create(projectSettingsFullPath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                    {
                        xmlReader.Read();
                        while (xmlReader.NodeType != XmlNodeType.None && (xmlReader.NodeType != XmlNodeType.Element || xmlReader.Name != "ProjectSettings")) xmlReader.Read();
                        if (xmlReader.NodeType == XmlNodeType.None)
                            UserInfoHandler.ShowError($"File '{projectSettingsFullPath}' does not contain a valid XML structure." + Environment.NewLine + "Alternative brand is ignored.");
                        XElement settingsElement = XElement.ReadFrom(xmlReader) as XElement;
                        foreach (XElement xe in settingsElement.Elements())
                        {
                            if (xe.Value == null) continue;
                            switch (GetXEleName(xe))
                            {
                                case BRAND_ELEMENT_PROJECT_NAME: brandName = xe.Value; break;
                                default: break; // unknown tags are simply ignored for now
                            }
                        };
                    }
                }
                catch { }
            }
            return String.IsNullOrEmpty(brandName) ? "EUROMOD" : brandName;  // return the brand or 
        }

        private static string GetXEleName(XElement xe) { return xe.Name == null ? string.Empty : xe.Name.ToString(); }

        internal static string GetPoweredByText() { return "POWERED BY EUROMOD      "; }

        private static void SetBackgroundImage(EM_UI_MainForm emptyForm)
        {
            // if a brand logo exists, use it
            object obj = Properties.Resources.ResourceManager.GetObject(BRAND_BACKGROUND_IMAGE_PREFIX + DefGeneral.BRAND_NAME);
            if (obj != null && obj is Image)
            {
                emptyForm.treeList.BackgroundImage = (Image)obj;
            }
        }

        private static void SetIcon(EM_UI_MainForm emptyForm)
        {
            // if a brand icon exists, use it
            object obj = Properties.Resources.ResourceManager.GetObject(BRAND_ICON_PREFIX + DefGeneral.BRAND_NAME);
            if (obj != null && obj is Icon)
            {
                emptyForm.Icon = (Icon)obj;
            }
        }
    }
}
