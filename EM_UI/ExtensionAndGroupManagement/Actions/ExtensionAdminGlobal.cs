using EM_UI.DataSets;
using EM_UI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionAdminGlobal // this is actually not an Action, but it seems to fit here
    {
        internal static void DoAdmin()
        {
            SwitchablePolicyConfigFacade globalExtensionConfigFacade = EM_AppContext.Instance.GetSwitchablePolicyConfigFacade();
            SwitchablePolicyConfig globalExtensionConfig = globalExtensionConfigFacade.GetSwitchablePolicyConfig();
            try
            {
                List<ExtensionOrGroup> extensions = new List<ExtensionOrGroup>();
                foreach (var sp in globalExtensionConfig.SwitchablePolicy) extensions.Add(new ExtensionOrGroup(sp));

                AdminExtensionsOrGroupsForm adminDialog = new AdminExtensionsOrGroupsForm("Administrate Global Extensions", extensions, true);

                if (adminDialog.ShowDialog() == DialogResult.Cancel) return;

                foreach (ExtensionOrGroup add in adminDialog.added)
                    globalExtensionConfig.SwitchablePolicy.AddSwitchablePolicyRow(Guid.NewGuid().ToString(), add.name, add.shortName, add.look.ToXml());

                foreach (ExtensionOrGroup change in adminDialog.changed)
                {
                    SwitchablePolicyConfig.SwitchablePolicyRow changeRow = (from ext in globalExtensionConfig.SwitchablePolicy where ext.ID == change.id select ext).First();
                    changeRow.LongName = change.name; changeRow.NamePattern = change.shortName; changeRow.Look = change.look.ToXml();
                }

                List<SwitchablePolicyConfig.SwitchablePolicyRow> delRow = (from ext in globalExtensionConfig.SwitchablePolicy select ext).ToList();
                for (int i = delRow.Count - 1; i >= 0; --i)
                    if (adminDialog.deletedIds.Contains(delRow[i].ID)) delRow[i].Delete();

                globalExtensionConfigFacade.AcceptChanges();
                globalExtensionConfigFacade.WriteXML();

                foreach (EM_UI_MainForm mainForm in EM_AppContext.Instance.GetOpenCountriesMainForms()) mainForm.treeList.Refresh(); // update look if necessary
            }
            catch (Exception exception)
            {
                globalExtensionConfigFacade.RejectChanges();
                UserInfoHandler.ShowException(exception);
            }
        }
    }
}
