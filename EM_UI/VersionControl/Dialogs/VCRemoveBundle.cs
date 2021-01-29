using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using EM_UI.Tools;
using VCUIAPI;

namespace EM_UI.VersionControl.Dialogs
{
    internal partial class VCRemoveBundle : Form
    {
        VCAPI _vcAPI = null;
        bool _removeReleases = true;

        internal VCRemoveBundle(VCAPI vcAPI, bool removeRelease)
        {
            InitializeComponent();
            
            _vcAPI = vcAPI;
            _removeReleases = removeRelease;

            Cursor = Cursors.WaitCursor;
            List<ReleaseInfo> bundles = new List<ReleaseInfo>();
            if (_removeReleases && !_vcAPI.GetReleases(out bundles))
            {
                Cursor = Cursors.Default;
                UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
                return;
            }

            lvBundles.Columns.Add("Generated");
            if (_removeReleases) lvBundles.Columns.Add("Name");
            lvBundles.Columns.Add("Info");
            lvBundles.Columns.Add("Author");
            foreach (ReleaseInfo bundle in bundles)
            {
                ListViewItem bundleDescription = new ListViewItem();
                bundleDescription.Text = bundle.Date.ToString();
                if (_removeReleases) bundleDescription.SubItems.Add(bundle.Name);
                bundleDescription.SubItems.Add(bundle.Message);
                bundleDescription.SubItems.Add(bundle.Author);
                bundleDescription.Tag = bundle;
                lvBundles.Items.Add(bundleDescription);
            }
            lvBundles.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            int indexInfoCol = _removeReleases ? 2 : 1;
            lvBundles.AutoResizeColumn(indexInfoCol, ColumnHeaderAutoResizeStyle.HeaderSize);
            lvBundles.Columns[indexInfoCol].Width = 250;
            if(lvBundles.Items.Count > 0)
            {
                lvBundles.Items[0].Selected = true;
                lvBundles.Items[0].Focused = true;
                lvBundles.Select();
                lvBundles.Focus();
            }

            Cursor = Cursors.Default;
        }

        void VCRemoveBundle_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnClose_Click(object sender, EventArgs e) { Close(); }

        void btnRemoveBundles_Click(object sender, EventArgs e)
        {
            if (lvBundles.CheckedItems.Count == 0) { UserInfoHandler.ShowInfo("Please select the bundles you want to remove."); return; }

            string warningContent = (_removeReleases) ? Environment.NewLine + Environment.NewLine + "This will remove an already commited merge!" : string.Empty;
            if (UserInfoHandler.GetInfo("Are you sure you want to irrevocably delete " +
               (lvBundles.CheckedItems.Count == 1 && warningContent == string.Empty ? "this item?" : "these items?" +
               warningContent), MessageBoxButtons.YesNo) == DialogResult.No) return;

            List<ReleaseInfo> bundles = new List<ReleaseInfo>();
            foreach (ListViewItem item in lvBundles.CheckedItems) bundles.Add(item.Tag as ReleaseInfo);

            Cursor = Cursors.WaitCursor;
            bool success = _vcAPI.RemoveReleases(bundles, _vcAPI.vc_projectInfo.ProjectId);

            if (success) UserInfoHandler.ShowSuccess("Removal accomplished."); else UserInfoHandler.ShowError(_vcAPI.GetErrorMessage());
            Cursor = Cursors.Default;

            Close();
        }
    }
}

