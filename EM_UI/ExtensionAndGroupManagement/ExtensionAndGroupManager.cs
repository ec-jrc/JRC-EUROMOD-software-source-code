using EM_UI.DataSets;
using EM_UI.CountryAdministration;
using System.Linq;
using System.Collections.Generic;
using DevExpress.XtraTreeList.Nodes;
using EM_UI.TreeListTags;
using DevExpress.XtraTreeList;
using EM_UI.TreeListManagement;
using System;
using EM_Common;
using EM_UI.Tools;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    internal class ExtensionAndGroupManager
    {
        internal static bool ShowExtensionSwitchEditor(CountryConfig.PolicyRow polRow)
        {
            return (from ep in GetCountryConfig(GetCC(polRow)).Extension_Policy 
                    where ep.PolicyID == polRow.ID && ep.BaseOff == false select ep).Any();
        }

        internal static string GetExtensionAdaptedSwitch(CountryConfig.PolicyRow polRow)
        {
            return ShowExtensionSwitchEditor(polRow) ? DefPar.Value.SWITCH : polRow.Switch;
        }

        internal static bool ShowExtensionSwitchEditor(CountryConfig.FunctionRow funRow)
        {
            return ShowExtensionSwitchEditor(funRow.PolicyRow) || (from ef in GetCountryConfig(GetCC(funRow)).Extension_Function
                                                                   where ef.FunctionID == funRow.ID && ef.BaseOff == false
                                                                   select ef).Any();
        }

        internal static string GetExtensionAdaptedSwitch(CountryConfig.FunctionRow funRow)
        { 
            if (!ShowExtensionSwitchEditor(funRow)) return funRow.Switch;
            // function-rows should not show 'switch' if they are only part of an extension-policy (i.e. not themselves extension-functions) and if they are not on
            // because in that case they are in fact not switchable, but permanently off (or n/a)
            return !(from ef in GetCountryConfig(GetCC(funRow)).Extension_Function where ef.FunctionID == funRow.ID && ef.BaseOff == false select ef).Any() && 
                     funRow.Switch != DefPar.Value.ON ? funRow.Switch : DefPar.Value.SWITCH;
        }

        internal static List<ExtensionOrGroup> GetLookGroupDrawInfo(string cc, TreeListNode node)
        {
            if (CountryAdministrator.IsAddOn(cc) || node.Tag == null || node.Tag as BaseTreeListTag == null) return new List<ExtensionOrGroup>();
            BaseTreeListTag nodeTag = node.Tag as BaseTreeListTag;
            string polID = nodeTag.GetDefaultPolicyRow().ID;
            List<string> lookGroupIds = (from lgPol in GetCountryConfig(cc).LookGroup_Policy where lgPol.PolicyID == polID select lgPol.LookGroupID).ToList();
            if (nodeTag.GetDefaultFunctionRow() != null)
            {
                string funID = nodeTag.GetDefaultFunctionRow().ID;
                lookGroupIds.AddRange((from lgFun in GetCountryConfig(cc).LookGroup_Function where lgFun.FunctionID == funID select lgFun.LookGroupID));
            }
            if (nodeTag.GetDefaultParameterRow() != null)
            {
                string parID = nodeTag.GetDefaultParameterRow().ID;
                lookGroupIds.AddRange((from lgPar in GetCountryConfig(cc).LookGroup_Parameter where lgPar.ParameterID == parID select lgPar.LookGroupID));
            }

            List<ExtensionOrGroup> lookGroups = new List<ExtensionOrGroup>();
            foreach (CountryConfig.LookGroupRow lg in GetCountryConfig(cc).LookGroup)
                if (lookGroupIds.Contains(lg.ID)) lookGroups.Add(new ExtensionOrGroup(lg));
            return lookGroups;
        }

        internal static List<ExtensionOrGroup> GetExtensionDrawInfo(string cc, TreeListNode node)
        {
            if (CountryAdministrator.IsAddOn(cc) || node.Tag == null || node.Tag as BaseTreeListTag == null) return new List<ExtensionOrGroup>();
            BaseTreeListTag nodeTag = node.Tag as BaseTreeListTag;

            Dictionary<string, LookDef.STYLE> extIdsAndStyles = new Dictionary<string, LookDef.STYLE>();
            string polID = nodeTag.GetDefaultPolicyRow().ID;
            foreach (CountryConfig.Extension_PolicyRow extPolRow in from ePol in GetCountryConfig(cc).Extension_Policy where ePol.PolicyID == polID select ePol)
                extIdsAndStyles.Add(extPolRow.ExtensionID, extPolRow.BaseOff ? LookDef.STYLE.EXTENSION_OFF : LookDef.STYLE.EXTENSION_ON);
            if (nodeTag.GetDefaultFunctionRow() != null)
            {
                string funID = nodeTag.GetDefaultFunctionRow().ID;
                foreach (CountryConfig.Extension_FunctionRow extFunRow in from eFun in GetCountryConfig(cc).Extension_Function where eFun.FunctionID == funID select eFun)
                { // function-setting with regard to BaseOff overwrites policy-setting (in fact only BaseOff=true may overwrite BaseOff=false, but this is not the point to check for observing this rule)
                    if (extIdsAndStyles.ContainsKey(extFunRow.ExtensionID)) extIdsAndStyles[extFunRow.ExtensionID] = extFunRow.BaseOff ? LookDef.STYLE.EXTENSION_OFF : LookDef.STYLE.EXTENSION_ON;
                    else extIdsAndStyles.Add(extFunRow.ExtensionID, extFunRow.BaseOff ? LookDef.STYLE.EXTENSION_OFF : LookDef.STYLE.EXTENSION_ON);
                }
            }
            if (nodeTag.GetDefaultParameterRow() != null)
            {
                string parID = nodeTag.GetDefaultParameterRow().ID;
                foreach (CountryConfig.Extension_ParameterRow extParRow in from ePar in GetCountryConfig(cc).Extension_Parameter where ePar.ParameterID == parID select ePar)
                { // see overwriting rule above
                    if (extIdsAndStyles.ContainsKey(extParRow.ExtensionID)) extIdsAndStyles[extParRow.ExtensionID] = extParRow.BaseOff ? LookDef.STYLE.EXTENSION_OFF : LookDef.STYLE.EXTENSION_ON;
                    else extIdsAndStyles.Add(extParRow.ExtensionID, extParRow.BaseOff ? LookDef.STYLE.EXTENSION_OFF : LookDef.STYLE.EXTENSION_ON);
                }
            }
            List<ExtensionOrGroup> extensions = new List<ExtensionOrGroup>();
            foreach (GlobLocExtensionRow e in GetExtensions(cc))
                if (extIdsAndStyles.ContainsKey(e.ID)) extensions.Add(new ExtensionOrGroup(e, extIdsAndStyles[e.ID]));
            return extensions;
        }

        internal static bool GetExtensionToolTip(string cc, TreeListHitInfo hit, out string toolTip)
        {
            toolTip = string.Empty;
            if (hit.Node == null || hit.Node.Tag == null || // show if mouse is over row-number-column or over policy-column
              !(hit.HitInfoType == HitInfoType.RowIndicator || (hit.Column != null && TreeListBuilder.IsPolicyColumn(hit.Column)))) return false;

            List<ExtensionOrGroup> egs = GetLookGroupDrawInfo(cc, hit.Node);
            egs.AddRange(GetExtensionDrawInfo(cc, hit.Node));
            if (!egs.Any()) return false;

            foreach (ExtensionOrGroup eg in egs)
                toolTip += eg.name + (eg.style == LookDef.STYLE.GROUP ? string.Empty :
                    $" (element will be {(eg.style == LookDef.STYLE.EXTENSION_ON ? "ON" : "OFF")} if extension is ON)") + Environment.NewLine;
            return true;
        }

        internal static List<GlobLocExtensionRow> GetExtensions(string cc)
        {
            List<GlobLocExtensionRow> extensionRows = GetGlobalExtensions();
            extensionRows.AddRange(GetLocalExtensions(cc));
            return extensionRows;
        }

        internal static List<GlobLocExtensionRow> GetGlobalExtensions()
        {
            List<GlobLocExtensionRow> extensionRows = new List<GlobLocExtensionRow>();
            foreach (SwitchablePolicyConfig.SwitchablePolicyRow s in GetGlobalExtensionConfig().SwitchablePolicy)
                extensionRows.Add(new GlobLocExtensionRow(s));
            return extensionRows;
        }

        internal static List<GlobLocExtensionRow> GetLocalExtensions(string cc)
        {
            List<GlobLocExtensionRow> extensionRows = new List<GlobLocExtensionRow>();
            foreach (DataConfig.ExtensionRow e in GetDataConfig(cc).Extension) extensionRows.Add(new GlobLocExtensionRow(e));
            return extensionRows;
        }

        internal static GlobLocExtensionRow GetGlobalExtension(string extensionID)
        {
            return (from ge in GetGlobalExtensions() where ge.ID == extensionID select ge).FirstOrDefault();
        }

        internal static string GetExtensionDefaultSwitch(DataConfig.DBSystemConfigRow dbSystemConfigRow, string switchablePolicyID)
        {
            var policySwitchRow = (from ps in dbSystemConfigRow.GetPolicySwitchRows() where ps.SwitchablePolicyID == switchablePolicyID select ps).FirstOrDefault();
            return policySwitchRow == null ? DefPar.Value.NA : policySwitchRow.Value;
        }

        internal static void SetExtensionDefaultSwitch(DataConfig dataConfig, DataConfig.DBSystemConfigRow dbSystemConfigRow, string extensionID, string extensionSwitch)
        {
            // instead of changing a possibly existing value for this extension/db/sys-combination delete "all" existing and add new
            // actually "all" should always be one or zero, but obviously undo produces duplicates (e.g. by deleting system and undoing it)
            // this corrects for these wrong duplicates (in principle there should be a unique key, but I'm reluctant adding one ...)
            List<DataConfig.PolicySwitchRow> existing = (from es in dataConfig.PolicySwitch
                                                         where es.RowState != System.Data.DataRowState.Deleted &&
                                                               es.SwitchablePolicyID == extensionID &&
                                                               es.SystemID == dbSystemConfigRow.SystemID &&
                                                               es.DataBaseID == dbSystemConfigRow.DataBaseID
                                                         select es).ToList();
            foreach (DataConfig.PolicySwitchRow e in existing) e.Delete();
            dataConfig.PolicySwitch.AddPolicySwitchRow(extensionID, dbSystemConfigRow.SystemID, dbSystemConfigRow.DataBaseID, extensionSwitch);
        }

        internal static void ExtensionDefaultSwitches_RemoveRelics(string cc) // remove any "relics", i.e. default switches of extensions, which were deleted meanwhile
        {
            for (int index = GetDataConfig(cc).PolicySwitch.Count - 1; index >= 0; --index)
            {
                DataConfig.PolicySwitchRow policySwitchRow = GetDataConfig(cc).PolicySwitch[index];
                if (policySwitchRow.RowState == System.Data.DataRowState.Deleted) continue;
                if (!(from e in GetExtensions(cc) where e.ID == policySwitchRow.SwitchablePolicyID select e).Any())
                    policySwitchRow.Delete();
            }
        }

        internal static void CheckForRemovedGlobalExtensions(string cc, out bool openReadOnly)
        {
            openReadOnly = false;
            List<string> existingExt = (from e in GetExtensions(cc) select e.ID).ToList();
            
            List<CountryConfig.Extension_PolicyRow> delExtPol = new List<CountryConfig.Extension_PolicyRow>();
            List<CountryConfig.Extension_FunctionRow> delExtFun = new List<CountryConfig.Extension_FunctionRow>();
            List<CountryConfig.Extension_ParameterRow> delExtPar = new List<CountryConfig.Extension_ParameterRow>();
            foreach (CountryConfig.Extension_PolicyRow ep in GetCountryConfig(cc).Extension_Policy)
                if (!existingExt.Contains(ep.ExtensionID)) delExtPol.Add(ep);
            foreach (CountryConfig.Extension_FunctionRow ef in GetCountryConfig(cc).Extension_Function)
                if (!existingExt.Contains(ef.ExtensionID)) delExtFun.Add(ef);
            foreach (CountryConfig.Extension_ParameterRow ep in GetCountryConfig(cc).Extension_Parameter)
                if (!existingExt.Contains(ep.ExtensionID)) delExtPar.Add(ep);
            
            if (!delExtPol.Any() && !delExtFun.Any() && !delExtPar.Any()) return;
            if (UserInfoHandler.GetInfo("The country needs to be updated for removed Global Extensions." + Environment.NewLine +
                "If you click Cancel the country will be opened in read-only mode.", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                { openReadOnly = true; return; }

            try
            {
                foreach (CountryConfig.Extension_PolicyRow dep in delExtPol)
                {
                    if (dep.PolicyRow.Switch == DefPar.Value.SWITCH) dep.PolicyRow.Switch = DefPar.Value.TOGGLE;
                    dep.Delete();
                }
                foreach (CountryConfig.Extension_FunctionRow def in delExtFun)
                {
                    if (def.FunctionRow.Switch == DefPar.Value.SWITCH) def.FunctionRow.Switch = DefPar.Value.TOGGLE;
                    def.Delete();
                }
                foreach (CountryConfig.Extension_ParameterRow dep in delExtPar) dep.Delete();
                ExtensionDefaultSwitches_RemoveRelics(cc);

                GetCountryConfig(cc).AcceptChanges();
                CountryAdministrator.WriteXML(cc);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Adapting Extensions failed.", false);
                GetCountryConfig(cc).RejectChanges();
            }
        }

        internal static bool IsOldStyleExtensions(string cc, out bool openReadOnly)
        {
            openReadOnly = false; bool isOldStyle = false;
            // check for all policies with switch='switch' whether they are part of any extension ...
            foreach (CountryConfig.PolicyRow pol in from p in GetCountryConfig(cc).Policy where p.Switch == DefPar.Value.SWITCH select p)
                if (!(from ep in GetCountryConfig(cc).Extension_Policy where ep.PolicyID == pol.ID && !ep.BaseOff select ep).Any())
                    { isOldStyle = true; break; }
            if (!isOldStyle) return false;

            // ... if not - suggest transformation to new style
            if (UserInfoHandler.GetInfo("The country needs to be updated to the new handling for Extensions (aka Switchable Policies)." + Environment.NewLine +
                "If you click Cancel the country will be opened in read-only mode.", MessageBoxButtons.OKCancel) == DialogResult.Cancel)
                { openReadOnly = true; return true; }

            // transform to new style: this assumes that the 'switch' settings in the spine are ok and just adds policies to extensions, which fulfill the name-pattern requirements
            CountryConfigFacade ccf = CountryAdministrator.GetCountryConfigFacade(cc);
            try
            {
                foreach (GlobLocExtensionRow ext in GetGlobalExtensions())
                {
                    foreach (CountryConfig.PolicyRow polRow in ccf.GetPolicyRowsOrderedAndDistinct())
                    {
                        string polName = string.IsNullOrEmpty(polRow.ReferencePolID) ? polRow.Name : ccf.GetPolicyRowByID(polRow.ReferencePolID).Name;
                        if (!EM_Helpers.DoesValueMatchPattern(ext.ShortName, polName)) continue;

                        foreach (CountryConfig.SystemRow sysRow in ccf.GetSystemRows())
                        {
                            CountryConfig.PolicyRow polSysRow = ccf.GetPolicyRowByOrder(polRow.Order, sysRow.ID);
                            if (!(from e in ccf.GetCountryConfig().Extension_Policy
                                  where e.ExtensionID == ext.ID & e.PolicyID == polSysRow.ID
                                  select e).Any()) // avoid double adding, which could actually only be caused by "playing"
                                ccf.GetCountryConfig().Extension_Policy.AddExtension_PolicyRow(ext.ID, polSysRow, false);
                        }
                    }
                }
                GetCountryConfig(cc).AcceptChanges();
                CountryAdministrator.WriteXML(cc);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception, "Adapting Extensions failed.", false);
                ccf.GetCountryConfig().RejectChanges();
            }

            return true;
        }

        internal static void CopyExtensionAndGroupMemberships(CountryConfig cc, string newSysID, string oldSysID)
        {
            // all policies belonging to any extension
            List<Tuple<string, CountryConfig.PolicyRow, bool>> newExtPols = new List<Tuple<string, CountryConfig.PolicyRow, bool>>();
            foreach (var ePol in from ep in cc.Extension_Policy where ep.PolicyRow.SystemID == oldSysID select ep)
                newExtPols.Add(new Tuple<string, CountryConfig.PolicyRow, bool>(ePol.ExtensionID,
                    CountryConfigFacade.GetTwinRow(ePol.PolicyRow, newSysID) as CountryConfig.PolicyRow, ePol.BaseOff));
            foreach (var newExtPol in newExtPols) cc.Extension_Policy.AddExtension_PolicyRow(newExtPol.Item1, newExtPol.Item2, newExtPol.Item3);

            // all functions belonging to any extension
            List<Tuple<string, CountryConfig.FunctionRow, bool>> newExtFuns = new List<Tuple<string, CountryConfig.FunctionRow, bool>>();
            foreach (var eFun in from ef in cc.Extension_Function where ef.FunctionRow.PolicyRow.SystemID == oldSysID select ef)
                newExtFuns.Add(new Tuple<string, CountryConfig.FunctionRow, bool>(eFun.ExtensionID,
                    CountryConfigFacade.GetTwinRow(eFun.FunctionRow, newSysID) as CountryConfig.FunctionRow, eFun.BaseOff));
            foreach (var newExtFun in newExtFuns) cc.Extension_Function.AddExtension_FunctionRow(newExtFun.Item1, newExtFun.Item2, newExtFun.Item3);

            // all parameters belonging to any extension
            List<Tuple<string, CountryConfig.ParameterRow, bool>> newExtPars = new List<Tuple<string, CountryConfig.ParameterRow, bool>>();
            foreach (var ePar in from ep in cc.Extension_Parameter where ep.ParameterRow.FunctionRow.PolicyRow.SystemID == oldSysID select ep)
                newExtPars.Add(new Tuple<string, CountryConfig.ParameterRow, bool>(ePar.ExtensionID,
                    CountryConfigFacade.GetTwinRow(ePar.ParameterRow, newSysID) as CountryConfig.ParameterRow, ePar.BaseOff));
            foreach (var newExtPar in newExtPars) cc.Extension_Parameter.AddExtension_ParameterRow(newExtPar.Item1, newExtPar.Item2, newExtPar.Item3);

            // all policies belonging to any group
            List<Tuple<CountryConfig.LookGroupRow, CountryConfig.PolicyRow>> newGroupPols = new List<Tuple<CountryConfig.LookGroupRow, CountryConfig.PolicyRow>>();
            foreach (var gPol in from gp in cc.LookGroup_Policy where gp.PolicyRow.SystemID == oldSysID select gp)
                newGroupPols.Add(new Tuple<CountryConfig.LookGroupRow, CountryConfig.PolicyRow>(gPol.LookGroupRow,
                    CountryConfigFacade.GetTwinRow(gPol.PolicyRow, newSysID) as CountryConfig.PolicyRow));
            foreach (var newGroupPol in newGroupPols) cc.LookGroup_Policy.AddLookGroup_PolicyRow(newGroupPol.Item1, newGroupPol.Item2);

            // all functions belonging to any group
            List<Tuple<CountryConfig.LookGroupRow, CountryConfig.FunctionRow>> newGroupFuns = new List<Tuple<CountryConfig.LookGroupRow, CountryConfig.FunctionRow>>();
            foreach (var gFun in from gf in cc.LookGroup_Function where gf.FunctionRow.PolicyRow.SystemID == oldSysID select gf)
                newGroupFuns.Add(new Tuple<CountryConfig.LookGroupRow, CountryConfig.FunctionRow>(gFun.LookGroupRow,
                    CountryConfigFacade.GetTwinRow(gFun.FunctionRow, newSysID) as CountryConfig.FunctionRow));
            foreach (var newGroupFun in newGroupFuns) cc.LookGroup_Function.AddLookGroup_FunctionRow(newGroupFun.Item1, newGroupFun.Item2);

            // all parameters belonging to any group
            List<Tuple<CountryConfig.LookGroupRow, CountryConfig.ParameterRow>> newGroupPars = new List<Tuple<CountryConfig.LookGroupRow, CountryConfig.ParameterRow>>();
            foreach (var gPar in from gp in cc.LookGroup_Parameter where gp.ParameterRow.FunctionRow.PolicyRow.SystemID == oldSysID select gp)
                newGroupPars.Add(new Tuple<CountryConfig.LookGroupRow, CountryConfig.ParameterRow>(gPar.LookGroupRow,
                    CountryConfigFacade.GetTwinRow(gPar.ParameterRow, newSysID) as CountryConfig.ParameterRow));
            foreach (var newGroupPar in newGroupPars) cc.LookGroup_Parameter.AddLookGroup_ParameterRow(newGroupPar.Item1, newGroupPar.Item2);
        }

        private static CountryConfig GetCountryConfig(string cc) { return CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig(); }
        private static DataConfig GetDataConfig(string cc) { return CountryAdministrator.GetDataConfigFacade(cc).GetDataConfig(); }
        private static SwitchablePolicyConfig GetGlobalExtensionConfig() { return EM_AppContext.Instance.GetSwitchablePolicyConfigFacade().GetSwitchablePolicyConfig(); }

        private static string GetCC(CountryConfig.PolicyRow polRow) { return polRow.SystemRow.CountryRow.ShortName; }
        private static string GetCC(CountryConfig.FunctionRow funRow) { return funRow.PolicyRow.SystemRow.CountryRow.ShortName; }
    }
}
