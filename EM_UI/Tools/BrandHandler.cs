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
        private const string BRAND_ELEMENT_PROJECT_TITLE = "Title";
        private const string BRAND_ELEMENT_BACKGROUND_IMAGE_PATH = "BackgroundImage";
        private const string BRAND_ELEMENT_BACKGROUND_IMAGE_LAYOUT = "BackgroundImageLayout";
        private const string BRAND_ELEMENT_ICON_PATH = "Icon";

        private static string brandName;
        private static string brandTitle;
        private static string brandBackgroundImagePath;
        private static string brandBackgroundImageLayout;
        private static string brandIconPath;

        internal static void PrepareBrand(EM_UI_MainForm emptyForm)
        {
            try
            {
                GetProjectBrandInfo();

                if (String.IsNullOrEmpty(brandName)) 
                {
                    brandTitle = brandName = DefGeneral.BRAND_NAME_DEFAULT;
                    // For now, completely go silent if the brand is wrong. To warn, uncomment the line below:
                    //UserInfoHandler.ShowError($"File '{projectSettingsFullPath}' does not contain a valid Project Name.{Environment.NewLine}Alternative brand is ignored.");
                }
                else
                {
                    if (String.IsNullOrEmpty(brandTitle)) brandTitle = brandName;
                    DefGeneral.BRAND_NAME = brandName;
                    DefGeneral.BRAND_TITLE = brandTitle;
                    SetBackgroundImage(emptyForm);
                    SetBackgroundImageLayout(emptyForm);
                    SetIcon(emptyForm);
                }
                emptyForm.btnRun.Caption = $"Run{Environment.NewLine}{DefGeneral.BRAND_TITLE}";
                emptyForm.barText_PoweredBy.Caption = DefGeneral.IsAlternativeBrand() ? GetPoweredByText() : string.Empty;
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Adapting to alternative Brand failed!", false);
            }
        }

        // this reads the ProjectSettings.xml if it exists and returns the ProjectName field if it exists, or null otherwise
        private static void GetProjectBrandInfo()
        {
            string projectSettingsFullPath = new EMPath(EM_AppContext.FolderEuromodFiles).GetProjectSettingsFilePath(true);
            brandName = brandTitle = brandBackgroundImagePath = brandBackgroundImageLayout = brandIconPath = string.Empty;   // first remove any existing brand!
            if (File.Exists(projectSettingsFullPath))
            {
                try
                {
                    using (XmlReader xmlReader = XmlReader.Create(projectSettingsFullPath, new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
                    {
                        string brandFilesFullPath = Path.Combine(Path.GetDirectoryName(projectSettingsFullPath), "BrandFiles");
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
                                case BRAND_ELEMENT_BACKGROUND_IMAGE_PATH: brandBackgroundImagePath = Path.Combine(brandFilesFullPath, xe.Value); break;
                                case BRAND_ELEMENT_BACKGROUND_IMAGE_LAYOUT: brandBackgroundImageLayout = xe.Value; break;
                                case BRAND_ELEMENT_ICON_PATH: brandIconPath = Path.Combine(brandFilesFullPath, xe.Value); break;
                                case BRAND_ELEMENT_PROJECT_NAME: brandName = xe.Value; break;
                                case BRAND_ELEMENT_PROJECT_TITLE: brandTitle = xe.Value; break;
                                default: break; // unknown tags are simply ignored for now
                            }
                        };
                    }
                }
                catch { }
            }
            if (String.IsNullOrEmpty(brandName)) brandName = DefGeneral.BRAND_NAME_DEFAULT;  
        }

        private static string GetXEleName(XElement xe) { return xe.Name == null ? string.Empty : xe.Name.ToString(); }

        internal static string GetPoweredByText() { return "POWERED BY EUROMOD      "; }

        private static void SetBackgroundImage(EM_UI_MainForm emptyForm)
        {
            if (!DefGeneral.IsAlternativeBrand()) return;
            // if a brand logo exists, use it
            if (!string.IsNullOrEmpty(brandBackgroundImagePath) && File.Exists(brandBackgroundImagePath))
            {
                try
                {
                    emptyForm.treeList.BackgroundImage = Image.FromFile(brandBackgroundImagePath);
                } catch { }
            }
        }
        private static void SetBackgroundImageLayout(EM_UI_MainForm emptyForm)
        {
            if (!DefGeneral.IsAlternativeBrand()) return;
            if (!string.IsNullOrEmpty(brandBackgroundImageLayout))
            {
                switch (brandBackgroundImageLayout.ToLower())
                {
                    case "stretch": emptyForm.treeList.BackgroundImageLayout = ImageLayout.Stretch; break;
                    case "center": emptyForm.treeList.BackgroundImageLayout = ImageLayout.Center; break;
                    case "tile": emptyForm.treeList.BackgroundImageLayout = ImageLayout.Tile; break;
                    case "zoom": emptyForm.treeList.BackgroundImageLayout = ImageLayout.Zoom; break;
                    case "none": emptyForm.treeList.BackgroundImageLayout = ImageLayout.None; break;
                }
            }
        }

        private static void SetIcon(EM_UI_MainForm emptyForm)
        {
            if (!DefGeneral.IsAlternativeBrand()) return;
            // if a brand icon exists, use it
            if (!string.IsNullOrEmpty(brandBackgroundImagePath) && File.Exists(brandIconPath))
            {
                try
                {
                    if (brandIconPath.EndsWith(".ico"))
                        emptyForm.Icon = Icon.ExtractAssociatedIcon(brandIconPath);
                    else
                        emptyForm.Icon = EM_UI.Tools.IconConverter.MakeIcon(Image.FromFile(brandIconPath));
                }
                catch { }
            }
        }
    }
}
