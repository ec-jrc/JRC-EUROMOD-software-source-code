using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class GroupAdminAction : BaseAction
    {
        string cc;
        bool actionIsCanceled = false;

        internal GroupAdminAction(string _cc)
        {
            cc = _cc;
        }

        internal override bool ActionIsCanceled()
        {
            return actionIsCanceled;
        }

        internal override void PerformAction()
        {
            CountryConfig countryConfig = CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig();

            List<ExtensionOrGroup> groups = new List<ExtensionOrGroup>();
            foreach (CountryConfig.LookGroupRow lgr in from lg in countryConfig.LookGroup select lg) groups.Add(new ExtensionOrGroup(lgr));

            AdminExtensionsOrGroupsForm groupDialog = new AdminExtensionsOrGroupsForm("Administrate Groups", groups);

            if (groupDialog.ShowDialog() == DialogResult.Cancel) { actionIsCanceled = true; return; }

            foreach (ExtensionOrGroup addGroup in groupDialog.added)
                countryConfig.LookGroup.AddLookGroupRow(Guid.NewGuid().ToString(), addGroup.name, addGroup.shortName, addGroup.look.ToXml());
                
            foreach (ExtensionOrGroup chGroup in groupDialog.changed)
            {
                CountryConfig.LookGroupRow lgRow = (from lg in countryConfig.LookGroup where lg.ID == chGroup.id select lg).FirstOrDefault();
                lgRow.Name = chGroup.name; lgRow.ShortName = chGroup.shortName; lgRow.Look = chGroup.look.ToXml();
            }

            List<CountryConfig.LookGroupRow> lgs = (from lg in countryConfig.LookGroup select lg).ToList();
            for (int i = lgs.Count - 1; i >= 0; --i)
                if (groupDialog.deletedIds.Contains(lgs[i].ID)) lgs[i].Delete();
        }
    }
}
