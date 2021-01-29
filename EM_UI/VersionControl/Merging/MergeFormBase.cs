using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace EM_UI.VersionControl.Merging
{
    internal abstract class MergeFormBase : Form
    {
        internal abstract bool HasDifferences();
        internal abstract MergeControl GetMergeControlByName(string mcName);
        internal abstract void LoadMergeControl(string mcName, List<string> levelInfo = null);
        protected internal string StoreMergeControl(string mcName) { return GetMergeControlByName(mcName).Store(mcName); }
        internal void RestoreMergeControl(string mcName, string content) { GetMergeControlByName(mcName).Restore(content); }

        protected void SetPositionMergeControl(MergeControl mergeControl, TabPage tabPage,
            //optional parameters are in fact not used (is a relict of starting with more than one MergeControl in a tab
                                    int top = -1, int height = -1,
                                    bool topAnchor = true, bool bottomAnchor = true)
        {
            mergeControl.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            if (topAnchor) mergeControl.Anchor = mergeControl.Anchor | AnchorStyles.Top;
            if (bottomAnchor) mergeControl.Anchor = mergeControl.Anchor | AnchorStyles.Bottom;
            mergeControl.Location = new Point(0, top);
            mergeControl.Size = new Size(tabPage.Width, height == -1 ? tabPage.Height : height);
            tabPage.Controls.Add(mergeControl);
        }

        internal void SetInfoMergeControl(string mcName, List<MergeControl.ColumnInfo> columInfo,
                        List<MergeControl.NodeInfo> nodeInfoLocal, List<MergeControl.NodeInfo> nodeInfoRemote,
                        bool provideSequenceInfo)
        {
            GetMergeControlByName(mcName).SetInfo(columInfo, nodeInfoLocal, nodeInfoRemote, provideSequenceInfo);
        }
    }
}
