using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevExpress.XtraTreeList.Nodes;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraBars;
using System.Drawing;

namespace EM_UI.TreeListManagement
{
    internal class BookmarkAndColorManager
    {
        internal static void LoadAndDrawBookmarks(EM_UI_MainForm mainForm)
        {
            try
            {
                foreach (string bookmark in EM_AppContext.Instance.GetUserSettingsAdministrator().GetBookmarks())
                {
                    string[] parts = bookmark.Split(';');

                    if (bookmark.StartsWith(mainForm.GetCountryShortName()))
                    {
                        TreeListNode node = mainForm.GetTreeListManager().GetSpecifiedNode(parts[1]);
                        if (node != null)
                            mainForm.DrawBookmark(parts[2], parts[1], node);
                    }
                }
            }
            catch (Exception exception)
            {
                DeleteBookmarks(mainForm.GetCountryShortName()); //try to delete the bookmarks, so that they do not cause further problems in e.g. adding new ones
                //do nothing (does not seem important enough to jeopardise loading of the whole country or any other relevant action)
                Tools.UserInfoHandler.RecordIgnoredException("BookmarkAndColorManager.LoadAndDrawBookmarks", exception);
            }
        }

        internal static void DeleteBookmarks(string countryShortName)
        {
            try
            {
                List<string> bookmarks = new List<string>();
                foreach (string bookmark in EM_AppContext.Instance.GetUserSettingsAdministrator().GetBookmarks())
                {
                    bookmarks.Add(bookmark);
                }

                foreach (string bookmark in bookmarks)
                {
                    if (bookmark.StartsWith(countryShortName))
                        EM_AppContext.Instance.GetUserSettingsAdministrator().RemoveFromBookmarks(bookmark);
                }
                EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.RecordIgnoredException("BookmarkAndColorManager.DeleteBookmarks", exception); //do nothing (see above)
            }
        }

        internal static void SaveBookmarks(string countryShortName, RibbonQuickToolbarItemLinkCollection toolBarLinkCollection)
        {
            try
            {
                foreach (BarItemLink toolBarLink in toolBarLinkCollection.Toolbar.ItemLinks)
                {
                    BarButtonItem toolBarButton = toolBarLink.Item as BarButtonItem;
                    if (toolBarButton.Name != "btnUndo" && toolBarButton.Name != "btnRedo")
                    {
                        string bookmark = countryShortName + ";" + toolBarButton.Tag as string + ";" + toolBarButton.Hint;
                        EM_AppContext.Instance.GetUserSettingsAdministrator().AddToBookmarks(bookmark);
                    }
                }
                EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.RecordIgnoredException("BookmarkAndColorManager.SaveBookmarks", exception); //do nothing (see above)
            }
        }

        internal static void SaveNewBookmark(string countryShortName, string referenceID, string bookmarkName)
        {
            try
            {
                string bookmark = countryShortName + ";" + referenceID + ";" + bookmarkName;
                EM_AppContext.Instance.GetUserSettingsAdministrator().AddToBookmarks(bookmark);
                EM_AppContext.Instance.GetUserSettingsAdministrator().SaveCurrentSettings(false);
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.RecordIgnoredException("BookmarkAndColorManager.SaveNewBookmark", exception); //do nothing (see above)
            }
        }
    }
}
