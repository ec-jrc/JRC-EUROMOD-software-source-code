using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EM_UI.TreeListManagement
{
    internal class SelectPolicyMenu : ListBox
    {
        private Dictionary<int, CountryConfig.PolicyRow> Content = new Dictionary<int, CountryConfig.PolicyRow>();
        private TreeListBuilder treeListBuilder = null;
        private EM_UI_MainForm mainForm = null;

        internal SelectPolicyMenu(TreeListBuilder treeListBuilder, EM_UI_MainForm mainForm)
        {
            this.treeListBuilder = treeListBuilder; this.mainForm = mainForm;
        }

        internal void Show(Point mainFormMousePosition, List<CountryConfig.PolicyRow> policyRows, List<string> idsDisplayedSinglePolicy, bool singlePolicyView)
        {
            try
            {
                Name = "EMUI_SelectPolicyMenu";
                Location = mainForm.treeList.PointToClient(mainForm.PointToScreen(mainFormMousePosition));
                HorizontalScrollbar = true;
                SelectionMode = SelectionMode.One;
                AutoSize = true;
                MaximumSize = new Size(mainForm.treeList.Width, mainForm.treeList.Height);

                Leave += Disappear;
                Click += ItemSelected;
                KeyUp += HandleKeyUp;

                Items.Clear(); Content.Clear();
                foreach (CountryConfig.PolicyRow policyRow in policyRows)
                {
                    int index = Items.Add(policyRow.Name + " (" + policyRow.Comment + ")");
                    if (idsDisplayedSinglePolicy.Contains(policyRow.ID))
                        SelectedIndex = index;
                    Content.Add(index, policyRow);
                }
                if (singlePolicyView) Items.Add("Full Spine");
                
                mainForm.treeList.Controls.Add(this);
                Focus();
            }
            catch {}
        }

        void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape) Disappear();
            else if (e.KeyCode == Keys.Enter) ItemSelected();
        }

        internal void Disappear(object sender = null, EventArgs e = null) { try { mainForm.treeList.Controls.Remove(this); } catch { } }

        void ItemSelected(object sender = null, EventArgs e = null)
        {
            treeListBuilder.PolicySelectMenu_ItemSelected(Content.ContainsKey(SelectedIndex) ? Content[SelectedIndex] : null);
            Disappear();
        }
    }

}
