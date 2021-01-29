using DevExpress.XtraTreeList.Nodes;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.Dialogs;
using EM_UI.NodeOperations;
using EM_UI.Tools;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    class ExtensionAndGroupMenuManager
    {
        // names of menu-items (used in Policy/FunctionContextMenu.Designer.cs - identivies the selected menu in the callback-functions)
        internal const string MENU_GROUP_REMOVE = "mniGroupRemove";
        internal const string MENU_GROUP_ADD = "mniGroupAdd";
        internal const string MENU_GROUP_VISIBLE = "mniGroupVisible";
        internal const string MENU_GROUP_NOT_VISIBLE = "mniGroupNotVisible";
        internal const string MENU_GROUP_EXPAND = "mniGroupExpand";
        internal const string MENU_EXTENSION_REMOVE = "mniExtensionRemove";
        internal const string MENU_EXTENSION_ADDON = "mniExtensionAddOn";
        internal const string MENU_EXTENSION_ADDOFF = "mniExtensionAddOff";
        internal const string MENU_EXTENSION_VISIBLE = "mniExtensionVisible";
        internal const string MENU_EXTENSION_NOT_VISIBLE = "mniExtensionNotVisible";
        internal const string MENU_EXTENSION_EXPAND = "mniExtensionExpand";
        internal const string MENU_EXTENSION_PRIVATE_COUNTRY = "mniExtensionPrivateCountry";
        internal const string MENU_EXTENSION_NOT_PRIVATE_COUNTRY = "mniExtensionNotPrivateCountry";
        internal const string MENU_EXTENSION_PRIVATE_ALL = "mniExtensionPrivateAll";
        internal const string MENU_EXTENSION_NOT_PRIVATE_ALL = "mniExtensionNotPrivateAll";

        // used as "menu-item" to indicate that no group/extension is concerned or available
        private const string NO_ITEMS_DEFINED = "No items defined yet";

        // stores the selected menu-item (e.g. Set Visible, Add to, ...) on selection, as otherwise it gets lost on sub-menu-selection (i.e. selecting the group/extension)
        internal static string selectedMenu = string.Empty;

        // called by policy/function-context-menus and ribbon-buttons to draw the sub-menu listing the relvant groups/extensions
        internal static Dictionary<string, Image> GetRelevantMenuItems(string cc, string menuItemName)
        {
            selectedMenu = menuItemName;
            return menuItemName.Contains("Extension") ? GetRelevantExtensionMenuItems(cc, menuItemName) : GetRelevantGroupMenuItems(cc, menuItemName);
        }

        private static Dictionary<string, Image> GetRelevantGroupMenuItems(string cc, string menuItemName)
        {
            Dictionary<string, Image> relevantGroups = new Dictionary<string, Image>();
            // (1) add to group: only show groups which still can be added, i.e. where not already all selected elements belong to
            if (menuItemName == MENU_GROUP_ADD)
            {
                foreach (CountryConfig.LookGroupRow groupRow in GetCountryConfig(cc).LookGroup)
                {
                    bool canBeAdded = false;
                    foreach (TreeListNode node in GetSelectedNodes(cc))
                    {
                        if (CountryAdministrator.IsAddOn(cc) || node.Tag == null || node.Tag as BaseTreeListTag == null) continue;
                        BaseTreeListTag nodeTag = node.Tag as BaseTreeListTag;
                        if (nodeTag.GetDefaultParameterRow() != null) // means: if (isParameterRow)
                        {
                            if ((from lgPar in GetCountryConfig(cc).LookGroup_Parameter
                                 where lgPar.LookGroupID == groupRow.ID && lgPar.ParameterID == nodeTag.GetDefaultParameterRow().ID
                                 select lgPar).Any()) continue; // node belongs to group
                        }
                        if (nodeTag.GetDefaultFunctionRow() != null) // means: if (isFunctionRow || isParameterRow)
                        {
                            if ((from lgFun in GetCountryConfig(cc).LookGroup_Function
                                 where lgFun.LookGroupID == groupRow.ID && lgFun.FunctionID == nodeTag.GetDefaultFunctionRow().ID
                                 select lgFun).Any()) continue; // node belongs to group
                        }
                        // means: if (isPolicyRow || isFunctionRow || isParameterRow)
                        if ((from lgPol in GetCountryConfig(cc).LookGroup_Policy
                             where lgPol.LookGroupID == groupRow.ID && lgPol.PolicyID == nodeTag.GetDefaultPolicyRow().ID
                             select lgPol).Any()) continue; // node belongs to group
                        canBeAdded = true; break; // node, nor parent nodes, do belong to group: can be added
                    }
                    if (!canBeAdded) continue;
                    ExtensionOrGroup eg = new ExtensionOrGroup(groupRow);
                    relevantGroups.Add(eg.name, eg.look.GetMenuImage());
                }
            }
            // (2) remove from group: only show groups which can be removed, i.e. where any of the selected elements belongs to
            else if (menuItemName == MENU_GROUP_REMOVE)
            {
                List<string> lookGroupIds = new List<string>();
                foreach (TreeListNode node in GetSelectedNodes(cc))
                {
                    if (CountryAdministrator.IsAddOn(cc) || node.Tag == null || node.Tag as BaseTreeListTag == null) continue;
                    BaseTreeListTag nodeTag = node.Tag as BaseTreeListTag;
                    // "belongs to" means, that the function/parameter itself has to belong to the group, not only the parent
                    if (nodeTag.GetDefaultParameterRow() != null) // means: if (isParameterRow)
                        lookGroupIds.AddRange(from lgPar in GetCountryConfig(cc).LookGroup_Parameter where lgPar.ParameterID == nodeTag.GetDefaultParameterRow().ID select lgPar.LookGroupID);
                    else if (nodeTag.GetDefaultFunctionRow() != null) // means: if (isFunctionRow)
                        lookGroupIds.AddRange(from lgFun in GetCountryConfig(cc).LookGroup_Function where lgFun.FunctionID == nodeTag.GetDefaultFunctionRow().ID select lgFun.LookGroupID);
                    else // means: if (isPolicyRow)
                        lookGroupIds.AddRange(from lgPol in GetCountryConfig(cc).LookGroup_Policy where lgPol.PolicyID == nodeTag.GetDefaultPolicyRow().ID select lgPol.LookGroupID);
                }
                foreach (CountryConfig.LookGroupRow groupRow in from lg in GetCountryConfig(cc).LookGroup where lookGroupIds.Contains(lg.ID) select lg)
                {
                    ExtensionOrGroup eg = new ExtensionOrGroup(groupRow);
                    relevantGroups.Add(eg.name, eg.look.GetMenuImage());
                }
            }
            // (3) all other menu items (Expand,etc.): show all groups
            else
            {
                foreach (CountryConfig.LookGroupRow groupRow in GetCountryConfig(cc).LookGroup)
                {
                    ExtensionOrGroup eg = new ExtensionOrGroup(groupRow);
                    relevantGroups.Add(eg.name, eg.look.GetMenuImage());
                }
            }
            return relevantGroups.Any() ? relevantGroups : new Dictionary<string, Image>() { { NO_ITEMS_DEFINED, null } };
        }

        private static Dictionary<string, Image> GetRelevantExtensionMenuItems(string cc, string menuItemName)
        {
            Dictionary<string, Image> relevantExtensions = new Dictionary<string, Image>();
            bool globalOnly = menuItemName == MENU_EXTENSION_PRIVATE_ALL || menuItemName == MENU_EXTENSION_NOT_PRIVATE_ALL;
            
            // (1) add to extension: only show extensions which still can be added, i.e. where not already all selected elements belong to
            if (menuItemName == MENU_EXTENSION_ADDON || menuItemName == MENU_EXTENSION_ADDOFF)
            {
                foreach (GlobLocExtensionRow extRow in globalOnly ? ExtensionAndGroupManager.GetGlobalExtensions() : ExtensionAndGroupManager.GetExtensions(cc))
                {
                    bool canBeAdded = false;
                    foreach (TreeListNode node in GetSelectedNodes(cc))
                    {
                        if (CountryAdministrator.IsAddOn(cc) || node.Tag == null || node.Tag as BaseTreeListTag == null) continue;
                        BaseTreeListTag nodeTag = node.Tag as BaseTreeListTag;
                        if (nodeTag.GetDefaultParameterRow() != null) // means: if (isParameterRow)
                        {
                            if ((from ePar in GetCountryConfig(cc).Extension_Parameter
                                 where ePar.ExtensionID == extRow.ID && ePar.ParameterID == nodeTag.GetDefaultParameterRow().ID
                                 select ePar).Any()) continue; // node belongs to extension
                        }
                        if (nodeTag.GetDefaultFunctionRow() != null) // means: if (isFunctionRow || isParameterRow)
                        {
                            var ef = from eFun in GetCountryConfig(cc).Extension_Function
                                     where eFun.ExtensionID == extRow.ID && eFun.FunctionID == nodeTag.GetDefaultFunctionRow().ID
                                     select eFun;
                            if (ef.Any())
                            {
                                if (nodeTag.GetDefaultParameterRow() == null) continue; // is actually function-node and belongs to extension
                                // a parameter-node can still be added as switch-off, if function-node is added as switch-on
                                if (ef.First().BaseOff || menuItemName == MENU_EXTENSION_ADDON) continue;
                            }
                        }
                        // means: if (isPolicyRow || isFunctionRow || isParameterRow)
                        var ep = from ePol in GetCountryConfig(cc).Extension_Policy
                                 where ePol.ExtensionID == extRow.ID && ePol.PolicyID == nodeTag.GetDefaultPolicyRow().ID
                                 select ePol;
                        if (ep.Any())
                        {
                            if (nodeTag.GetDefaultParameterRow() == null && nodeTag.GetDefaultFunctionRow() == null) continue; // is actually policy-node and belongs to extension
                            // a parameter- or function-node can still be added as switch-off, if policy-node is added as switch-on
                            if (ep.First().BaseOff || menuItemName == MENU_EXTENSION_ADDON) continue;
                        }                            
                        canBeAdded = true; break; // node, nor parent nodes, do belong to extension: can be added
                    }
                    if (!canBeAdded) continue;
                    ExtensionOrGroup eg = new ExtensionOrGroup(extRow);
                    relevantExtensions.Add(eg.name, eg.look.GetMenuImage());
                }
            }
            // (2) remove from extension: only show extensions which can be removed, i.e. where any of the selected elements belongs to
            else if (menuItemName == MENU_EXTENSION_REMOVE)
            {
                List<string> extIds = new List<string>();
                foreach (TreeListNode node in GetSelectedNodes(cc))
                {
                    if (CountryAdministrator.IsAddOn(cc) || node.Tag == null || node.Tag as BaseTreeListTag == null) continue;
                    BaseTreeListTag nodeTag = node.Tag as BaseTreeListTag;
                    // "belongs to" means, that the function/parameter itself has to belong to the extension, not only the parent
                    if (nodeTag.GetDefaultParameterRow() != null) // means: if (isParameterRow)
                        extIds.AddRange(from ePar in GetCountryConfig(cc).Extension_Parameter where ePar.ParameterID == nodeTag.GetDefaultParameterRow().ID select ePar.ExtensionID);
                    else if (nodeTag.GetDefaultFunctionRow() != null) // means: if (isFunctionRow)
                        extIds.AddRange(from eFun in GetCountryConfig(cc).Extension_Function where eFun.FunctionID == nodeTag.GetDefaultFunctionRow().ID select eFun.ExtensionID);
                    else // means: if (isPolicyRow)
                        extIds.AddRange(from ePol in GetCountryConfig(cc).Extension_Policy where ePol.PolicyID == nodeTag.GetDefaultPolicyRow().ID select ePol.ExtensionID);
                }
                foreach (GlobLocExtensionRow gleRow in from e in globalOnly ? ExtensionAndGroupManager.GetGlobalExtensions() : ExtensionAndGroupManager.GetExtensions(cc)
                                                       where extIds.Contains(e.ID) select e)
                {
                    ExtensionOrGroup eg = new ExtensionOrGroup(gleRow);
                    relevantExtensions.Add(eg.name, eg.look.GetMenuImage());
                }
            }
            // (3) all other menu items (Expand,etc.): show all extensions
            else
            {
                foreach (GlobLocExtensionRow extRow in globalOnly ? ExtensionAndGroupManager.GetGlobalExtensions() : ExtensionAndGroupManager.GetExtensions(cc))
                {
                    ExtensionOrGroup eg = new ExtensionOrGroup(extRow);
                    relevantExtensions.Add(eg.name, eg.look.GetMenuImage());
                }
            }
            return relevantExtensions.Any() ? relevantExtensions : new Dictionary<string, Image>() { { NO_ITEMS_DEFINED, null } };
        }

        // used as call-back-function by the ribbon-buttons and called by call-back-functions of the policy/function-context-menus
        internal static void MenuItemClicked(EM_UI_MainForm mainForm, string groupOrExtensionName)
        {
            if (groupOrExtensionName == NO_ITEMS_DEFINED) return;
            string cc = mainForm.GetCountryShortName();

            switch (selectedMenu) // ... the parent-menu was stored on opening the group-sub-menu in the variable selectedMenu
            {
                case MENU_GROUP_ADD:
                    mainForm.PerformAction(new GroupAddContentAction(cc, groupOrExtensionName), false); break;
                case MENU_EXTENSION_ADDON:
                    mainForm.PerformAction(new ExtensionAddContentAction(cc, mainForm, groupOrExtensionName, true), false); break;
                case MENU_EXTENSION_ADDOFF:
                    mainForm.PerformAction(new ExtensionAddContentAction(cc, mainForm, groupOrExtensionName, false), false); break;
                case MENU_GROUP_REMOVE:
                    mainForm.PerformAction(new GroupRemoveContentAction(cc, groupOrExtensionName), false); break;
                case MENU_EXTENSION_REMOVE:
                    mainForm.PerformAction(new ExtensionRemoveContentAction(cc, mainForm, groupOrExtensionName), false); break;
                case MENU_GROUP_EXPAND:
                    VisualiseMenuOperation(mainForm: mainForm, egName: groupOrExtensionName, extensionOnOnly: null, setVisible: null, expand: true); break;
                case MENU_EXTENSION_EXPAND:
                    VisualiseMenuOperation(mainForm: mainForm, egName: groupOrExtensionName, extensionOnOnly: false, setVisible: null, expand: true); break;
                case MENU_GROUP_VISIBLE:
                    VisualiseMenuOperation(mainForm: mainForm, egName: groupOrExtensionName, extensionOnOnly: null, setVisible: true, expand: false); break;
                case MENU_EXTENSION_VISIBLE:
                    VisualiseMenuOperation(mainForm: mainForm, egName: groupOrExtensionName, extensionOnOnly: true, setVisible: true, expand: false); break;
                case MENU_GROUP_NOT_VISIBLE:
                    VisualiseMenuOperation(mainForm: mainForm, egName: groupOrExtensionName, extensionOnOnly: null, setVisible: false, expand: false); break;
                case MENU_EXTENSION_NOT_VISIBLE:
                    VisualiseMenuOperation(mainForm: mainForm, egName: groupOrExtensionName, extensionOnOnly: true, setVisible: false, expand: false); break;
                case MENU_EXTENSION_PRIVATE_COUNTRY:
                    SetExtensionPrivateCountry(cc: cc, mainForm: mainForm, extName: groupOrExtensionName, set: true); break;
                case MENU_EXTENSION_NOT_PRIVATE_COUNTRY:
                    SetExtensionPrivateCountry(cc: cc, mainForm: mainForm, extName: groupOrExtensionName, set: false); break;
                case MENU_EXTENSION_PRIVATE_ALL:
                    SetExtensionPrivateGlobal(extName: groupOrExtensionName, set: true); break;
                case MENU_EXTENSION_NOT_PRIVATE_ALL:
                    SetExtensionPrivateGlobal(extName: groupOrExtensionName, set: false); break;
                default: break;
            }
        }

        internal static void GetSelectionPolFunPar(string cc, out List<CountryConfig.PolicyRow> policyRows, out List<CountryConfig.FunctionRow> functionRows, out List<CountryConfig.ParameterRow> parameterRows)
        {
            policyRows = new List<CountryConfig.PolicyRow>();
            functionRows = new List<CountryConfig.FunctionRow>();
            parameterRows = new List<CountryConfig.ParameterRow>();

            foreach (var sys in from s in GetCountryConfig(cc).System select s)
            {
                List<TreeListNode> selectedNodes = GetSelectedNodes(cc);
                foreach (TreeListNode node in selectedNodes)
                {
                    // parameter-node ...
                    if ((node.Tag as BaseTreeListTag).GetDefaultParameterRow() != null)
                    {   // ... only add if not anyway the (whole) function is part of the selection
                        if (!selectedNodes.Contains(node.ParentNode)) parameterRows.Add((node.Tag as ParameterTreeListTag).GetParameterRowOfSystem(sys.ID));
                    }
                    // function-node ...
                    else if ((node.Tag as BaseTreeListTag).GetDefaultFunctionRow() != null)
                    {   // ... only add if not anyway the (whole) policy is part of the selection
                        if (!selectedNodes.Contains(node.ParentNode)) functionRows.Add((node.Tag as FunctionTreeListTag).GetFunctionRowOfSystem(sys.ID));
                    }
                    // policy-node
                    else policyRows.Add((node.Tag as PolicyTreeListTag).GetPolicyRowOfSystem(sys.ID));
                }
            }
        }

        private static void VisualiseMenuOperation(EM_UI_MainForm mainForm, string egName, bool? extensionOnOnly, bool? setVisible, bool expand)
        {
            string cc = mainForm.GetCountryShortName();
            IsSpecificBase specificationOperation;
            if (extensionOnOnly == null) specificationOperation = new IsGroupMember(GetCountryConfig(cc), egName);
            else specificationOperation = new IsExtensionMember(GetCountryConfig(cc), GetDataConfig(cc), egName, extensionOnOnly == true ? true : false);

            mainForm.treeList.NodesIterator.DoOperation(new TreatSpecificNodes(
                        isSpecific: specificationOperation, treatment: null,
                        visualiseTreated: expand, expandTreated: expand, setVisibleTreated: setVisible | expand));
        }

        private static void SetExtensionPrivateCountry(string cc, EM_UI_MainForm mainForm, string extName, bool set)
        {
            bool isLocal = (from e in GetDataConfig(cc).Extension where e.Name.ToLower() == extName.ToLower() || e.ID == extName select e).Any();
            if (!isLocal)
            {
                switch (UserInfoHandler.GetInfo("Do you want to perform this private-setting-action for all countries? Click" + Environment.NewLine +
                        "'Yes' to perform the action for all countries" + "," + Environment.NewLine +
                        "'No' to perform the action only for " + cc + ".", MessageBoxButtons.YesNoCancel))
                {
                    case DialogResult.Cancel: return;
                    case DialogResult.Yes: SetExtensionPrivateGlobal(extName, set); return;
                    case DialogResult.No: break;
                }
            }
            mainForm.PerformAction(new ExtensionSetPrivateAction(extName, cc, set));
        }

        private static void SetExtensionPrivateGlobal(string extName, bool set)
        {
            string message = "Setting components of extension '" + extName + "' to " + (set ? "private" : "'not private'");
            if (EM_AppContext.Instance.IsAnythingOpen(false))
            {
                if (UserInfoHandler.GetInfo(message + " requires all countries to be closed." + Environment.NewLine + "All open countries will be closed.",
                    MessageBoxButtons.OKCancel) == DialogResult.Cancel) return;
                EM_AppContext.Instance.CloseAllMainForms(false, false);
            }
            if (EM_AppContext.Instance.IsAnythingOpen(false)) return; // user may have refused to save changes for an open country

            using (ProgressIndicator progressIndicator = new ProgressIndicator(SetPrivate_BackgroundEventHandler, message, new Tuple<string, bool>(extName, set)))
            {
                if (progressIndicator.ShowDialog() == DialogResult.OK)
                    UserInfoHandler.ShowSuccess(message + " successfully accomplished.");
            }
        }

        private static void SetPrivate_BackgroundEventHandler(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker backgroundWorker = sender as BackgroundWorker; if (backgroundWorker.CancellationPending) { e.Cancel = true; return; }
            Tuple<string, bool> settings = e.Argument as Tuple<string, bool>; string extName = settings.Item1; bool set = settings.Item2;
            try
            {
                List<Country> countries = CountryAdministrator.GetCountries();
                for (int i = 0; i < countries.Count; ++i)
                {
                    if (backgroundWorker.CancellationPending) { e.Cancel = true; return; }
                    Country country = countries[i]; CountryConfigFacade ccf = country.GetCountryConfigFacade();
                    ExtensionSetPrivateAction action = new ExtensionSetPrivateAction(extName, countries[i]._shortName, set);
                    action.PerformAction();
                    if (!action.ActionIsCanceled()) // happens if extension has no content
                        { country.WriteXML(); country.SetCountryConfigFacade(null); country.SetDataConfigFacade(null); }
                    backgroundWorker.ReportProgress(Convert.ToInt32((i + 1.0) / (countries.Count * 1.0) * 100.0));
                }
                if (backgroundWorker.CancellationPending) { e.Cancel = true; return; }
            }
            catch (Exception exception) { UserInfoHandler.ShowException(exception); e.Cancel = true; }
        }

        private static CountryConfig GetCountryConfig(string cc) { return CountryAdministrator.GetCountryConfigFacade(cc).GetCountryConfig(); }
        private static DataConfig GetDataConfig(string cc) { return CountryAdministrator.GetDataConfigFacade(cc).GetDataConfig(); }
        private static SwitchablePolicyConfig GetGlobalExtensionConfig() { return EM_AppContext.Instance.GetSwitchablePolicyConfigFacade().GetSwitchablePolicyConfig(); }
        private static List<TreeListNode> GetSelectedNodes(string cc) { return EM_AppContext.Instance.GetCountryMainForm(cc).GetMultiCellSelector().GetSelectedNodes(true); }
    }
}
