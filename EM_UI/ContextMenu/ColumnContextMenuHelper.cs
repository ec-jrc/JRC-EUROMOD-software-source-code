using DevExpress.Utils.Menu;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Menu;
using EM_Common_Win;
using EM_UI.Actions;
using EM_UI.CountryAdministration;
using EM_UI.Dialogs;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.ContextMenu
{
    internal class ColumnContextMenuHelper
    {
        EM_UI_MainForm _mainForm = null;
        TreeListColumn _senderColumn = null;
        Dictionary<string, EventHandler> _menuItemsSystemColumn = new Dictionary<string, EventHandler>();
        Dictionary<string, EventHandler> _menuItemsPolicyColumn = new Dictionary<string, EventHandler>();

        const string _subMenuHeadMarker = ">";
        const string _subMenuMarker = "<";
        const string _menuSeparator1 = "-";
        const string _menuSeparator3 = "---";
        const string _menuSeparator4 = "----";
        const string _menuItemCaption_CopySystem = "Copy/Paste System";
        const string _menuItemCaption_RenameSystem = "Rename System";
        const string _menuItemCaption_DeleteSystem = "Delete System";
        const string _menuItemCaption_RestoreSystemOrder = "Restore System Order";
        const string _menuItemCaption_SaveSystemOrder = "Save System Order";
        const string _menuItemCapiton_MoveToHiddenSystems = _subMenuHeadMarker + "Move To Hidden Systems Box ...";
        const string _subMenuItemCapiton_SelectedSystem = _subMenuMarker + "Selected System";
        const string _subMenuItemCapiton_AllSystemsBut = _subMenuMarker + "All Systems But Selected";
        const string _subMenuItemCapiton_SelectSystems = _subMenuMarker + "Select Systems ...";
        const string _subMenuItemCapiton_AllSystemsToTheLeft = _subMenuMarker + "All Systems To The Left";
        const string _subMenuItemCapiton_AllSystemsToTheRight = _subMenuMarker + "All Systems To The Right";
        const string _subMenuItemCapiton_menuSeparator5 = _subMenuMarker + "-----";
        const string _subMenuItemCapiton_UnhideAllSystems = _subMenuMarker + "Unhide All Systems";
        const string _subMenuItemCapiton_menuSeparator6 = _subMenuMarker + "------";
        const string _subMenuItemCapiton_ShowHiddenSystemsBox = _subMenuMarker + "Show Hidden Systems Box";
        const string _menuItemCaption_ShowMatrixView = "Show Matrix View of Incomelists";
        const string _menuItemCaption_BestFitAllSystemColumns = "Best Fit (all system columns)";
        const string _menuItemCaption_SetWidthColumn = "Set Width Column";
        const string _menuItemCaption_SetWidthAllSystemColumns = "Set Width All System Columns";
        const string _menuItemCaption_InsertFirstPolicy = "Insert First Policy";
        const string _menuItemCaption_ExpandAllPolicies = "Expand All Policies";
        const string _menuItemCaption_CollapseAllPolicies = "Collapse all Policies";
        const string _menuItemCaption_InsertFirstSystem = "Insert First System";

        void InsertMenuItems(TreeListMenu menu, Dictionary<string, EventHandler> menuItemsCaptionsAndHandlers)
        {
            int mainIndex = 0;
            int subIndex = 0;
            bool beginGroup = false;
            DXSubMenuItem actDropDown = null;

            foreach (string menuItemCaption_wm in menuItemsCaptionsAndHandlers.Keys) //wm = with marker
            {
                bool isSubMenuHeader = menuItemCaption_wm.StartsWith(_subMenuHeadMarker);
                bool isSubMenu = menuItemCaption_wm.StartsWith(_subMenuMarker);
                string menuItemCaption = (isSubMenuHeader || isSubMenu) ? menuItemCaption_wm.Substring(1) : menuItemCaption_wm;

                if (menuItemCaption.StartsWith(_menuSeparator1))
                {
                    beginGroup = true;
                    continue;
                }

                DXMenuItem menuItem = null;
                if (isSubMenuHeader)
                {
                    actDropDown = new DXSubMenuItem(menuItemCaption);
                    menuItem = actDropDown;
                    subIndex = 0;
                }
                else
                    menuItem = new DXMenuItem(menuItemCaption, new EventHandler(menuItemsCaptionsAndHandlers[menuItemCaption_wm]));

                menuItem.BeginGroup = beginGroup;
                beginGroup = false;
                GreyState(menuItem);

                if (isSubMenu)
                    actDropDown.Items.Insert(subIndex++, menuItem);
                else
                    menu.Items.Insert(mainIndex++, menuItem);
            }
        }

        void GreyState(DXMenuItem menuItem)
        {
            menuItem.Enabled = true;
            switch (menuItem.Caption)
            {
                case _menuItemCaption_InsertFirstPolicy:
                    if (_mainForm.treeList.Nodes.Count > 0)
                        menuItem.Enabled = false;
                    break;
                case _menuItemCaption_InsertFirstSystem:
                    if (_mainForm.treeList.Columns.Count > TreeListBuilder.GetFixedColumnsLeftCount() + 1) //only columns 'Policy', 'Group' and 'Comments' exist = no systems yet
                        menuItem.Enabled = false;
                    break;
                case _menuItemCaption_ShowMatrixView:
                    menuItem.Enabled = !_mainForm._isAddOn;
                    break;
            }
            if (_subMenuItemCapiton_ShowHiddenSystemsBox.Contains(menuItem.Caption))
                menuItem.Enabled = (_mainForm.treeList.CustomizationForm == null || !_mainForm.treeList.CustomizationForm.Visible);
        }

        internal ColumnContextMenuHelper(EM_UI_MainForm mainForm)
        {
            _mainForm = mainForm;

            //additional menu items to menu for system column
            _menuItemsSystemColumn.Add(_menuItemCaption_CopySystem, CopySystem_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuItemCaption_RenameSystem, RenameSystem_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuItemCaption_DeleteSystem, DeleteSystem_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuSeparator1, null);
            _menuItemsSystemColumn.Add(_menuItemCaption_RestoreSystemOrder, RestoreSystemOrder_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuItemCaption_SaveSystemOrder, SaveSystemOrder_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuItemCapiton_MoveToHiddenSystems, null);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_SelectedSystem, MoveToHiddenSystems_SelectedColumn_MenuItemClick);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_AllSystemsBut, MoveToHiddenSystems_AllSystemsBut_MenuItemClick);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_SelectSystems, MoveToHiddenSystems_SelectSystems_MenuItemClick);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_AllSystemsToTheLeft, MoveToHiddenSystems_AllSystemsToTheLeft_MenuItemClick);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_AllSystemsToTheRight, MoveToHiddenSystems_AllSystemsToTheRight_MenuItemClick);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_menuSeparator5, null);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_UnhideAllSystems, MoveToHiddenSystems_UnhideAllSystems_MenuItemClick);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_menuSeparator6, null);
            _menuItemsSystemColumn.Add(_subMenuItemCapiton_ShowHiddenSystemsBox, MoveToHiddenSystems_ShowHiddenSystemBox_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuSeparator3, null);
            _menuItemsSystemColumn.Add(_menuItemCaption_ShowMatrixView, ShowMatrixView_MenuItemClick);
            _menuItemsSystemColumn.Add(_menuSeparator4, null);
            _menuItemsSystemColumn.Add(_menuItemCaption_InsertFirstPolicy, InsertFirstPolicy_MenuItemClick);

            //additional menu items to menu for policy column
            _menuItemsPolicyColumn.Add(_menuItemCaption_ExpandAllPolicies, ExpandAllPolicies_MenuItemClick);
            _menuItemsPolicyColumn.Add(_menuItemCaption_CollapseAllPolicies, CollapseAllPolicies_MenuItemClick);
            _menuItemsPolicyColumn.Add(_menuSeparator1, null);
            _menuItemsPolicyColumn.Add(_menuItemCaption_InsertFirstSystem, InsertFirstSystem_MenuItemClick);
        }

        internal void ShowContextMenu(PopupMenuShowingEventArgs e) //add own menue items to the automatically provided columns menue
        {
            TreeListHitInfo hitInfo = _mainForm.treeList.CalcHitInfo(e.Point);

            if (hitInfo.HitInfoType != HitInfoType.Column)
                return;

            //remove automatically provided but disabled menue items and item 'Column Chooser'
            for (int i = e.Menu.Items.Count - 1; i >= 0; --i)
                if (e.Menu.Items[i].Enabled == false || e.Menu.Items[i].Caption.ToLower().Contains("chooser"))
                    e.Menu.Items.RemoveAt(i); //column chooser is replaced by menu item 'Move To Hidden Columns ...' (see above)

            //no menue for columns 'Group' and 'Comment'
            if (TreeListBuilder.IsFixedColumn(hitInfo.Column) && !TreeListBuilder.IsPolicyColumn(hitInfo.Column))
            {
                e.Menu.Items[0].BeginGroup = false; //remove separator
                return;
            }
                
            //menue for column 'Policy'
            if (TreeListBuilder.IsPolicyColumn(hitInfo.Column))
                InsertMenuItems(e.Menu, _menuItemsPolicyColumn);

            //menu for system columns
            else
            {
                InsertMenuItems(e.Menu, _menuItemsSystemColumn);

                //add at the end of the menu, below 'Best Fit (all columns)' an option to only best fit the system columns, as with the former usually
                //the comments column uses lots of space and some systems may even get 'invisible', i.e. one needs to scroll (which may easily be overlooked)
                DXMenuItem menuItem = new DXMenuItem(_menuItemCaption_BestFitAllSystemColumns, new EventHandler(BestFitAllSystemColumns_MenuItemClick)); ;
                e.Menu.Items.Add(menuItem);
                //also add options to set a specific column width
                menuItem = new DXMenuItem(_menuItemCaption_SetWidthColumn, new EventHandler(SetWidthColumn_MenuItemClick));
                e.Menu.Items.Add(menuItem);
                menuItem = new DXMenuItem(_menuItemCaption_SetWidthAllSystemColumns, new EventHandler(SetWidthAllSystemColumns_MenuItemClick));
                e.Menu.Items.Add(menuItem);
            }

            //store clicked column to be used in the menue event handlers
            _senderColumn = hitInfo.Column;
        }

        #region EventHandlers

        void InsertFirstSystem_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new CopySystemAction(
                CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName()), _mainForm._isAddOn), true, true);
        }

        void CopySystem_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new CopySystemAction(_senderColumn, CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName()),
                                                                        CountryAdministrator.GetDataConfigFacade(_mainForm.GetCountryShortName())), true, true);
        }

        void RenameSystem_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new RenameSystemAction(_senderColumn, CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName()),
                                                                          CountryAdministrator.GetDataConfigFacade(_mainForm.GetCountryShortName())), true, true);
        }

        void DeleteSystem_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new DeleteSystemAction(_senderColumn, CountryAdministrator.GetDataConfigFacade(_mainForm.GetCountryShortName())), true, true);
        }

        void SaveSystemOrder_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new SaveSystemOrderAction(_mainForm), false);
        }

        void MoveToHiddenSystems_SelectedColumn_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            if (_mainForm.treeList.CustomizationForm == null)
                _mainForm.treeList.ColumnsCustomization(); //invoke column chooser if not already available

            _mainForm.ClearCellSelection(); //clear any existing selection of cells (to avoid that actions on selected cell change hidden systems)

            //move selected column to column chooser
            _senderColumn.VisibleIndex = -1;
            _senderColumn.OptionsColumn.ShowInCustomizationForm = true;
            _mainForm.treeList.EndUpdate();
        }

        void MoveToHiddenSystems_AllSystemsBut_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            if (_mainForm.treeList.CustomizationForm == null)
                _mainForm.treeList.ColumnsCustomization(); //invoke column chooser if not already available

            _mainForm.ClearCellSelection(); //clear any existing selection of cells (to avoid that actions on selected cell change hidden systems)

            //move all columns to column chooser except selected and fixed columns (policy, group, comments)
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (column.Caption == _senderColumn.Caption || column.Fixed != FixedStyle.None)
                    continue;
                column.VisibleIndex = -1;
                column.OptionsColumn.ShowInCustomizationForm = true;
            }
            _mainForm.treeList.EndUpdate();
        }

        private void MoveToHiddenSystems_AllSystemsToTheRight_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            if (_mainForm.treeList.CustomizationForm == null)
                _mainForm.treeList.ColumnsCustomization(); //invoke column chooser if not already available

            _mainForm.ClearCellSelection(); //clear any existing selection of cells (to avoid that actions on selected cell change hidden systems)

            //move all columns to column chooser except selected and fixed columns (policy, group, comments)
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (column.VisibleIndex <= _senderColumn.VisibleIndex || column.Fixed != FixedStyle.None)
                    continue;
                column.VisibleIndex = -1;
                column.OptionsColumn.ShowInCustomizationForm = true;
            }
            _mainForm.treeList.EndUpdate();
        }

        private void MoveToHiddenSystems_AllSystemsToTheLeft_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            if (_mainForm.treeList.CustomizationForm == null)
                _mainForm.treeList.ColumnsCustomization(); //invoke column chooser if not already available

            _mainForm.ClearCellSelection(); //clear any existing selection of cells (to avoid that actions on selected cell change hidden systems)

            //move all columns to column chooser except selected and fixed columns (policy, group, comments)
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (column.VisibleIndex >= _senderColumn.VisibleIndex || column.Fixed != FixedStyle.None)
                    continue;
                column.VisibleIndex = -1;
                column.OptionsColumn.ShowInCustomizationForm = true;
            }
            _mainForm.treeList.EndUpdate();
        }
        
        void MoveToHiddenSystems_SelectSystems_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            //assess which systems are already hidden (and therefore listed in the column chooser)
            List<string> noShowSystemIDs = new List<string>();
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
                if (column.VisibleIndex == -1 && column.OptionsColumn.ShowInCustomizationForm == true && column.Tag != null)
                    noShowSystemIDs.Add((column.Tag as SystemTreeListTag).GetSystemRow().ID);

            //allow user to select systems (which are not already hidden)
            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(_mainForm.GetCountryShortName(), noShowSystemIDs);
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;

            //invoke column chooser if not already available
            //if no systems are selected for hiding, the column chooser gets visible, if it was closed with the close button (with its current content)
            if (_mainForm.treeList.CustomizationForm == null)
                _mainForm.treeList.ColumnsCustomization();

            _mainForm.ClearCellSelection(); //clear any existing selection of cells (to avoid that actions on selected cell change hidden systems)

            //hide the systems selected by the user
            List<string> selectedSystemIDs = (from sR in selectSystemsForm.GetSelectedSystemRows() select sR.ID).ToList();
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (column.Tag != null && selectedSystemIDs.Contains((column.Tag as SystemTreeListTag).GetSystemRow().ID))
                {
                    column.VisibleIndex = -1;
                    column.OptionsColumn.ShowInCustomizationForm = true;
                }
            }
            _mainForm.treeList.EndUpdate();
        }

        void MoveToHiddenSystems_UnhideAllSystems_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            //unhide all columns (listed in the column chooser)
            foreach (TreeListColumn column in _mainForm.treeList.Columns)
            {
                if (column.VisibleIndex == -1 && column.OptionsColumn.ShowInCustomizationForm == true)
                {
                    column.Visible = true;
                    column.OptionsColumn.ShowInCustomizationForm = false;
                }
            }
            _mainForm.treeList.EndUpdate();
        }

        void MoveToHiddenSystems_ShowHiddenSystemBox_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.showHiddenSystemsBox();
        }

        void RestoreSystemOrder_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new BaseAction(), true, true); //BaseAction does nothing at PerformAction, i.e. this only effects that treelist is redrawn
        }

        void InsertFirstPolicy_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.PerformAction(new AddPolicyAction(_mainForm, CountryAdministrator.GetCountryConfigFacade(_mainForm.GetCountryShortName())), false);
        }

        void ExpandAllPolicies_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.GetTreeListBuilder().ExpandAllPolicies();
        }

        void CollapseAllPolicies_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.GetTreeListBuilder().CollapseAllPolicies();
        }

        void ShowMatrixView_MenuItemClick(object sender, EventArgs e)
        {
            EM_AppContext.Instance.GetMatrixViewOfIncomelistsForm().UpdateView(_senderColumn.Tag as SystemTreeListTag);
        }

        void BestFitAllSystemColumns_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            foreach (TreeListColumn systemColumn in _mainForm.GetTreeListBuilder().GetSystemColums())
                systemColumn.BestFit();
            _mainForm.treeList.EndUpdate();
        }

        int AskForColumnWidth(int defaultWidth = -1)
        {
            string width = (defaultWidth == -1) ? string.Empty : defaultWidth.ToString();
            UserInput ui = new UserInput(new UserInput.Item("id", "Column Width:") { InitialValue = width, ValueType = typeof(int), 
                                                                                     MinMax = new Tuple<double,double>(0, double.MaxValue) });
            if (ui.ShowDialog() == DialogResult.Cancel) return -1;
            return ui.GetValue<int>("id");
        }

        void SetWidthColumn_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            int width = AskForColumnWidth();
            if (width >= 0 && _senderColumn != null)
                _senderColumn.Width = width;
            _mainForm.treeList.EndUpdate();
        }

        void SetWidthAllSystemColumns_MenuItemClick(object sender, EventArgs e)
        {
            _mainForm.treeList.BeginUpdate();
            int width = AskForColumnWidth(_mainForm.GetTreeListBuilder().GetSystemEvenlyDistributionWidht());
            if (width >= 0)
            {
                foreach (TreeListColumn systemColumn in _mainForm.GetTreeListBuilder().GetSystemColums())
                    systemColumn.Width = width;
            }
            _mainForm.treeList.EndUpdate();
        }

        #endregion EventHandlers
    }
}
