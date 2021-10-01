using DevExpress.XtraBars;
using DevExpress.XtraBars.Ribbon;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_Crypt;
using EM_UI.Actions;
using EM_UI.ContextMenu;
using EM_UI.CountryAdministration;
using EM_UI.DataSets;
using EM_UI.DeveloperInfo;
using EM_UI.DeveloperInfo.ReleaseValidation;
using EM_UI.Dialogs;
using EM_UI.Editor;
using EM_UI.ExtensionAndGroupManagement;
using EM_UI.GlobalAdministration;
using EM_UI.ImportExport;
using EM_UI.NodeOperations;
using EM_UI.Run;
using EM_UI.Tools;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using EM_UI.UndoManager;
using EM_UI.UpratingIndices;
using EM_UI.VersionControl;
using EM_UI.VersionControl.Merging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI
{
    internal partial class EM_UI_MainForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        #region member_variables

        string _countryShortName = string.Empty;

        const string _autoSaveFilePrefix = "astmp_";
        const string _titleHiddenSystemsBox = "Hidden Systems Box";

        TreeListBuilder _treeListBuilder = null;
        TreeListManager _treeListManager = null;
        ComponentUseForm _componentUseForm = null;

        ADOUndoManager _undoManager = null;
        FormulaEditorManager _formulaEditorManager = null;
        MultiCellSelector _multiCellSelector = null;

        PolicyContextMenu _policyContextMenu = null;
        FunctionContextMenu _functionContextMenu = null;
        RowContextMenu _rowContextMenu = null;
        ColumnContextMenuHelper _columnContextMenu = null;
        CellContextMenu _cellContextMenu = null;

        internal ImportByIDDiscrepancies _importByIDDiscrepancies = null;

        internal RunMainForm _runMainForm = null;

        bool isClosing = false;
        int _hasChanges = 0;

        internal bool _isAddOn = false;

        const string _statusBarText_NotAllCountriesVisible = "Note that not all countries are visible in the countries' gallery. Use scrolling to view all countries.";

        internal Dictionary<TreeListColumn, Dictionary<TreeListNode, KeyValuePair<Color, Color>>> _specialFormatCells = new Dictionary<TreeListColumn, Dictionary<TreeListNode, KeyValuePair<Color, Color>>>();

        // These four variables are used to help with the cell selection process
        TreeListNode _startNode = null;
        TreeListNode _endNode = null;
        TreeListColumn _startColumn = null;
        TreeListColumn _endColumn = null;

        internal KeyEventArgs _currentKeyState = null; //current state of the keyboard (set and released in key-down and key-up events)

        internal bool _isReadOnly = false; //set to true if country is in use by another instance and therefore only available in read-only mode

        #endregion member_variables

        #region get_functions

        internal string GetCountryShortName() { return _countryShortName; }

        internal string GetCountryLongName() { return _countryConfigFacade == null ? string.Empty : _countryConfigFacade.GetCountryLongName(); }

        internal ADOUndoManager GetUndoManager() { return _undoManager; }

        internal EM_UI_MainForm() { InitializeComponent(); }

        internal EM_UI_MainForm(string countryShortName)
        {
            _countryShortName = countryShortName;
            InitializeComponent();
        }

        internal FunctionContextMenu GetFunctionContextMenu() { return _functionContextMenu; }
        internal PolicyContextMenu GetPolicyContextMenu() { return _policyContextMenu; }
        internal RowContextMenu GetRowContextMenu() { return _rowContextMenu; }
        internal ColumnContextMenuHelper GetColumnContextMenu() { return _columnContextMenu; }
        internal CellContextMenu GetCellContextMenu() { return _cellContextMenu; }

        internal TreeListBuilder GetTreeListBuilder() { return _treeListBuilder; }
        internal TreeListManager GetTreeListManager() { return _treeListManager; }
        internal FormulaEditorManager GetFormulaEditorManager() { return _formulaEditorManager; }
        internal MultiCellSelector GetMultiCellSelector() { return _multiCellSelector; }

        CountryConfigFacade _countryConfigFacade = null;
        DataConfigFacade _dataConfigFacade = null;

        internal Point GetMousePosition() { return MousePosition; }

        #endregion get_functions

        #region mainform_events

        protected override void OnLoad(EventArgs e)
        {
            ShowTreelistColumnHeaders(_countryShortName != string.Empty); //only show column headers once a country is loaded

            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath);
            helpProvider.HelpNamespace = helpPath;

            FillCountryGallery();
            FillAddOnGallery();
            FillPlugInGallery();

            _multiCellSelector = new MultiCellSelector(this);
            _formulaEditorManager = new FormulaEditorManager(lstFormulaEditorIntelli, lblComboboxEditorToolTip, intelliImages, treeList);

            _policyContextMenu = new PolicyContextMenu(this);
            _policyContextMenu.Visible = false;
            Controls.Add(_policyContextMenu);

            _functionContextMenu = new FunctionContextMenu(this);
            _functionContextMenu.Visible = false;
            Controls.Add(_functionContextMenu);

            _columnContextMenu = new ColumnContextMenuHelper(this);

            _rowContextMenu = new RowContextMenu(this);
            _rowContextMenu.Visible = false;
            Controls.Add(_rowContextMenu);

            _cellContextMenu = new CellContextMenu(this);
            _cellContextMenu.Visible = false;
            Controls.Add(_cellContextMenu);

            if (_countryShortName != string.Empty)
                LoadCountry();

            treeList.IndicatorWidth = 70; //todo: find out how much space row numbers actually take (using smth like TextRenderer.MeasureText in treeList_CustomDrawNodeIndicator)

            EM_MainRibbon.Toolbar.ItemLinks.CollectionChanged += new CollectionChangeEventHandler(ItemLinks_CollectionChanged);

            SetButtonGreyState();
        }

        void UpdateMainFormCaption()
        {
            this.Text = EM_AppContext.Instance.ComposeMainFormCaption(GetCountryLongName(), _isReadOnly); //set caption to "EUROMOD Version (Path)" if no country is loaded
        }

        void ShowTreelistColumnHeaders(bool show)
        {
            treeList.OptionsView.ShowIndicator = show;
            treeList.OptionsView.ShowColumns = show;
            treeList.OptionsView.ShowHorzLines = show;
            treeList.OptionsView.ShowVertLines = show;
            treeList.TreeLineStyle = show ? LineStyle.Percent50 : LineStyle.None;
        }

        void EM_UI_MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!EM_AppContext.Instance.CheckForActiveRunsOnLastMainFormClosing() || EM_AppContext.Instance.WriteXml(_countryShortName, true, false) == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
            if (_componentUseForm != null) _componentUseForm.Close();
            EM_AppContext.Instance.RemoveCountryMainForm(this);
            isClosing = true;

            ViewKeeper.StoreSettings(this); // store systems' width and visible-state, to store if country is opened next time

            if (!_isReadOnly && _countryShortName != string.Empty) //second condition: the empty form is closing
                InUseFileHandler.ReleaseFile(CountryAdministrator.GetCountryPath(_countryShortName), _countryShortName); //files can now be used by other users again
        }

        void EM_UI_MainForm_Activated(object sender, EventArgs e)
        {
            EM_AppContext.Instance.MainForm = this; //to allow for assessing which country is active from the EM_AppContext (see GetActiveCountryMainForm)

            //this is to adapt the content of the add-parameter dialog if the user switches to another country, we switched it off before delivering prototype 3 - but I forgot why
            //if (EM_AppContext.Instance.AddParameterForm.Visible)
            //    EM_AppContext.Instance.AddParameterForm.UpdateContent((treeList != null) ? treeList.FocusedNode : null);
        }

        void autoSaveTimer_Tick(object sender, EventArgs e)
        {
            if (_isReadOnly)
                return;
            try
            {
                if (EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval <= 0)
                {
                    autoSaveTimer.Stop();
                    return;
                }

                if (treeList.ActiveEditor != null) // to avoid that users lose their changes when they are currently typing (and autosave closes their editor)
                {
                    autoSaveTimer.Interval = 30000; // try again in half a minute
                    return;
                }

                autoSaveTimer.Interval = EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval;

                txtInStatusBar.Caption = "Autosaving..."; txtInStatusBar.Refresh();

                PerformAction(new SaveAction(_countryShortName, EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles), _autoSaveFilePrefix + _countryShortName), false, false);
                ViewKeeper.StoreSettings(this); EM_AppContext.Instance.StoreViewSettings();
            }
            catch (Exception exception)
            {//do nothing
                autoSaveTimer.Stop();
                UserInfoHandler.RecordIgnoredException("EM_UI_MainForm.autoSaveTimer_Tick", exception);
            }
            finally
            {
                txtInStatusBar.Caption = _countryShortName;
            }
        }

        void OnOffToggleEditor_MouseWheel(object sender, MouseEventArgs e)
        {
            treeList.Focus(); //set focus to treeList, to avoid changing value of switch
        }

        internal bool HasChanges()
        {
            //return _undoManager != null && _undoManager.HasChanges(); //outcommented, as saving does not clear undo-manager anymore
            return _hasChanges > 0;
        }

        internal int GetChangesStatus() { return _hasChanges; }

        internal void WriteXml()
        {
            //if (_undoManager != null)
            //    _undoManager.Reset(); //outcommented, because undo should also be available after saving

            _treeListManager.CheckForUnhandledChange(); //make sure that really all changes are changed (see description in function)

            PerformAction(new SaveAction(_countryShortName), false, false);
            _hasChanges = 0;
        }

        #endregion mainform_events

        #region display_gallery_and_countries

        void galAddOns_GalleryItemClick(object sender, GalleryItemClickEventArgs galleryItemClickEventArgs)
        {
            GalleryItemClick(galleryItemClickEventArgs);
        }

        void galCountries_GalleryItemClick(object sender, GalleryItemClickEventArgs galleryItemClickEventArgs)
        {
            GalleryItemClick(galleryItemClickEventArgs);
        }

        void GalleryItemClick(GalleryItemClickEventArgs galleryItemClickEventArgs)
        {
            GalleryItem countryItem = galleryItemClickEventArgs.Item;

            if (countryItem.Caption == _countryShortName) //don't load same country second time
                return;

            EM_UI_MainForm countryForm = EM_AppContext.Instance.GetCountryMainForm(countryItem.Caption);
            if (countryForm != null) //country is already open (but not on top)
            {
                countryForm.Focus();
            }
            else //country is not yet open
            {
                //capture parameter files, to avoid editing by another user
                bool openedReadOnly = false;
                if (!InUseFileHandler.CaptureFile(CountryAdministrator.GetCountryPath(countryItem.Caption), countryItem.Caption,
                                                              ref openedReadOnly)) //set to true, if another user already captured the files
                    return; //files are captured by another user and the user decided to not open in read-only mode

                countryForm = new EM_UI_MainForm(countryItem.Caption); //loading is done in OnLoad
                countryForm._isReadOnly = openedReadOnly;
                EM_AppContext.Instance.AddAndShowCountryMainForm(countryForm);

                //reset to actually loaded country of this form
                galleryItemClickEventArgs.Gallery.SetItemCheck(galleryItemClickEventArgs.Gallery.GetItemByCaption(_countryShortName), true);
            }
        }

        internal void LoadCountry(GalleryItem galleryItem = null)
        {
            if (galleryItem == null) //in OnLoad _countryShortName is set, i.e. need to find respective gallery item
            {
                List<GalleryItem> galleryItems = galCountries.Gallery.GetAllItems(); //first check country gallery
                var itemQuery = from item in galleryItems where item.Caption.ToLower() == _countryShortName.ToLower() select item;
                if (itemQuery.ToList<GalleryItem>().Count == 1)
                    galleryItem = itemQuery.ToList<GalleryItem>().First<GalleryItem>();
                else //if not found, check add-on gallery
                {
                    galleryItems = galAddOns.Gallery.GetAllItems();
                    itemQuery = from item in galleryItems where item.Caption.ToLower() == _countryShortName.ToLower() select item;
                    if (itemQuery.ToList<GalleryItem>().Count == 1)
                        galleryItem = itemQuery.ToList<GalleryItem>().First<GalleryItem>();
                    else
                    {
                        Tools.UserInfoHandler.ShowError(_countryShortName + " could not be loaded.");
                        return;
                    }
                }
            }

            try
            {
                Cursor = Cursors.WaitCursor;

                _countryShortName = galleryItem.Caption;
                _isAddOn = CountryAdministrator.IsAddOn(_countryShortName);

                TreeListBeginUnboundLoad(); //essential for performance: prevents screen updates
                treeList.ClearNodes();

                _undoManager = new ADOUndoManager();

                //read country's xml-files
                _countryConfigFacade = CountryAdministrator.GetCountryConfigFacade(_countryShortName, true);
                _dataConfigFacade = CountryAdministrator.GetDataConfigFacade(_countryShortName, true);

                Extension_Checks(); // check for "old style" extension handling, i.e. identification via pattern (e.g. BTA_??)
                                    // and for adaptations needs upon removal of global extensions
                _countryConfigFacade.RegisterWithUndoManager(_undoManager);
                if (_dataConfigFacade != null) //is null for add-ons
                    _dataConfigFacade.RegisterWithUndoManager(_undoManager);

                string countryLongName = _countryConfigFacade.GetCountryLongName();

                _treeListManager = new TreeListManager(this, _countryConfigFacade);
                _treeListBuilder = new TreeListBuilder(this, _countryConfigFacade);

                //load country into treelist
                _treeListBuilder.BuildTreeList();
                _treeListBuilder.FillTreeList();

                VisualiseWhichCountryIsLoaded();
                barText_PoweredBy.Caption = DefGeneral.IsAlternativeBrand() ? BrandHandler.GetPoweredByText() : string.Empty;

                BookmarkAndColorManager.LoadAndDrawBookmarks(this);

                ViewKeeper.RestoreSettings(this); // restore systems' width and visible-state, if country was open before

                SetButtonGreyState();
            }
            catch (Exception exception)
            {
                TryReconstruction(exception);
            }
            finally
            {
                TreeListEndUnboundLoad();
                this.Cursor = Cursors.Default;
                if (EM_AppContext.Instance.GetUserSettingsAdministrator().Get().AutoSaveInterval <= 0)
                    autoSaveTimer.Stop();
                else
                {
                    autoSaveTimer.Interval = 6000; //to do the first autosave after a second, real interval is set in autoSaveTimer_Tick
                    autoSaveTimer.Start();
                }
                EM_AppContext.Instance.ShowEmptyForm(false); //hide the empty form (showing EUROMOD logo) if it was visible (i.e. first country loaded)
            }
        }

        private void Extension_Checks()
        {
            if (_isAddOn) return;
            
            Cursor = Cursors.WaitCursor;
            bool isOldStyle = ExtensionAndGroupManager.IsOldStyleExtensions(_countryShortName, out bool openReadOnly);
            if (!isOldStyle) ExtensionAndGroupManager.CheckForRemovedGlobalExtensions(_countryShortName, out openReadOnly);
            Cursor = Cursors.Default;

            if (openReadOnly) // if user refuses adaptation: open country read-only
            {
                InUseFileHandler.ReleaseFile(CountryAdministrator.GetCountryPath(_countryShortName), _countryShortName);
                _isReadOnly = true;
            }
        }

        void VisualiseWhichCountryIsLoaded()
        {
            //adapt status bar
            Image flag = CountryAdministrator.GetFlag(_countryShortName);
            btnFlagInStatusBar.Glyph = flag;
            txtInStatusBar.Caption = _countryShortName;

            //adapt window text and icon
            string countryLongName = _countryConfigFacade.GetCountryLongName();
            UpdateMainFormCaption();
            this.Icon = EM_UI.Tools.IconConverter.MakeIcon(flag);

            //for especial visibility which country/add-on is loaded: put the flag/icon in an extra button on the left side of the gallery
            ribbonPageGroupLoadedCountry.Visible = true;
            btnLoadedCountry.LargeGlyph = flag;
            btnLoadedCountry.Caption = countryLongName;
        }

        void TryReconstruction(Exception exception)
        {
            autoSaveTimer.Stop();

            if (Directory.GetFiles(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles), _autoSaveFilePrefix + _countryShortName + "*.xml").ToList().Count == 0)
            {
                Tools.UserInfoHandler.ShowException(exception);
                return; //if no autosaved version available, reconstruction not possible
            }

            if (Tools.UserInfoHandler.GetInfo("Failed to load country. Try reconstruction with last auto-save version?", MessageBoxButtons.YesNo) == DialogResult.No)
                return;

            //copy autosaved country and data file from temp-folder to country folder
            try
            {
                foreach (string tmpFile in Directory.GetFiles(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles), _autoSaveFilePrefix + _countryShortName + "*.xml"))
                {
                    string countryFile = tmpFile.Replace(_autoSaveFilePrefix, "");
                    countryFile = countryFile.Replace(EMPath.Folder_Temp(EM_AppContext.FolderEuromodFiles), EMPath.AddSlash(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + _countryShortName));
                    File.Copy(tmpFile, countryFile, true);
                }
            }
            catch (Exception reloadException)
            {
                Tools.UserInfoHandler.ShowException(reloadException);
            }

            LoadCountry();
        }

        internal void FillCountryGallery(bool refresh = false)
        {
            FillGallery(galCountries.Gallery, CountryAdministrator.GetCountries(refresh), false);
        }

        internal void FillAddOnGallery(bool refresh = false)
        {
            FillGallery(galAddOns.Gallery, CountryAdministrator.GetAddOns(refresh), true);
        }

        void FillGallery(DevExpress.XtraBars.Ribbon.Gallery.InRibbonGallery gallery, List<Country> items, bool addOns)
        {
            GalleryItemGroup galleryItems = new GalleryItemGroup();
            foreach (Country item in items)
            {
                GalleryItem galleryItem = new GalleryItem(item._flag, item._shortName, string.Empty);
                galleryItem.Caption = item._shortName;
                galleryItem.Hint = item._shortName;
                galleryItems.Items.Add(galleryItem);
            }
            gallery.Groups.Clear();
            gallery.Groups.Add(galleryItems);
            gallery.ImageSize = new Size(CountryAdministrator.GetImageWidth(addOns), CountryAdministrator.GetImageHeight(addOns));
        }

        internal void SetButtonGreyState()
        {
            btnUndo.Enabled = _undoManager != null && _undoManager.HasChanges();
            btnRedo.Enabled = _undoManager != null && _undoManager.CanRedo();
            btnAddSystem.Enabled = _countryConfigFacade != null;
            btnDeleteSystems.Enabled = _countryConfigFacade != null;
            btnCleanUpSystems.Enabled = _countryConfigFacade != null;
            btnImportSystems.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnExportSystems.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnImportAddOn.Enabled = _countryConfigFacade != null && !_isAddOn && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnExportAddOn.Enabled = _countryConfigFacade != null && !_isAddOn && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnCompareVersions.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnStoreInfoMarkers.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView) && _importByIDDiscrepancies != null;
            btnLoadInfoMarkers.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnRemoveInfoMarkers.Enabled = _importByIDDiscrepancies != null && _importByIDDiscrepancies.Count() > 0;
            btnSearchByID.Enabled = _countryConfigFacade != null;
            btnConditionalFormatting.Enabled = _countryConfigFacade != null;
            btnAutomaticFormatting.Enabled = _countryConfigFacade != null;
            btnSearchReplace.Enabled = _countryConfigFacade != null;
            btnComponentUse.Enabled = _countryConfigFacade != null && !_isAddOn && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnExpandPrivate.Enabled = _countryConfigFacade != null;
            btnConfigCountry.Enabled = _countryConfigFacade != null;
            btnConfigSystems.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnConfigData.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnAdminLookGroups.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnAddToLookGroup.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnRemoveFromLookGroup.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetVisibleLookGroup.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetNotVisibleLookGroup.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnAdminLExtensions.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetExtensionsSwitches.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnAddToOnExtension.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnAddToOffExtension.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnRemoveFromExtension.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetVisibleExtension.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetNotVisibleExtension.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnExpandExtension.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetPrivateExtensionL.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnSetNotPrivateExtensionL.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnExpandLookGroup.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnUpratingIndices.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnIndirectTaxes.Enabled = _countryConfigFacade != null && !_isAddOn;
            btnRestore.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnMergeCountry.Enabled = _countryConfigFacade != null && !(_treeListBuilder != null && _treeListBuilder.SinglePolicyView);
            btnSave.Enabled = _countryConfigFacade != null && !_isReadOnly;
            btnSave_As.Enabled = _countryConfigFacade != null;
            btnCloseAllCountries.Enabled = _countryConfigFacade != null;
            btnCloseCountry.Enabled = _countryConfigFacade != null;
            btnSave.Caption = _isAddOn ? "Save Add-On  (Ctrl+S)" : "Save Country  (Ctrl+S)";
            btnSave_As.Caption = _isAddOn ? "Save Add-On As" : "Save Country As";
            btnCloseCountry.Caption = _isAddOn ? "Close Add-On" : "Close Country";
            btnClearNodeColor.Enabled = _countryConfigFacade != null;
            btnRemoveAllNodeColors.Enabled = _countryConfigFacade != null;
            colorEditChooseNodeColor.Enabled = _countryConfigFacade != null;
            btnSetBookmark.Enabled = _countryConfigFacade != null;
            chkSuspendSystemFormatting.Enabled = _countryConfigFacade != null;
            chkShowFunctionSpecifiers.Enabled = _countryConfigFacade != null;
            UpdateChkShowFunctionSpecifiers();
            chkFullSpine.Enabled = _countryConfigFacade != null;
            chkSinglePolicy.Enabled = _countryConfigFacade != null;
            txtFullSpine.Enabled = _countryConfigFacade != null;
            txtSinglePolicy.Enabled = _countryConfigFacade != null;
            btnShowMatrixViewOfIncomelists.Enabled = _countryConfigFacade != null;
            btnShowHiddenSystems.Enabled = _countryConfigFacade != null;

            ribbonPageLookGroup.Visible = !_isAddOn;
            ribbonPageGroupExtensions.Visible = !_isAddOn;

            bool showVC = EnvironmentInfo.ShowComponent(EnvironmentInfo.Display.VC_show);
            if (showVC) SetVCButtonsGreyState();
            btnMergeCountry.Visibility = showVC ? BarItemVisibility.Never : BarItemVisibility.Always;
            btnVariablesMerge.Visibility = showVC ? BarItemVisibility.Never : BarItemVisibility.Always;
            ribbonVersionControl.Visible = showVC;
            ribbonPageGroupPublicVersion.Visible = EnvironmentInfo.ShowComponent(EnvironmentInfo.Display.PR_producer);
            btnIndirectTaxes.Visibility = EnvironmentInfo.ShowComponent(EnvironmentInfo.Display.IndirectTaxes_debug) ? BarItemVisibility.Always : BarItemVisibility.Never;
            UpdateMainFormCaption();

            // This should always go last to overwrite everything else! 
            SetSecureButtonsGreyState();
        }

        private void SetSecureButtonsGreyState()
        {
            if (!SecureInfo.IsSecure) return;

            // if required, lock to the current project only
            if (SecureInfo.LockProject)
            {
                // Main menu
                btnOpen.Visibility = BarItemVisibility.Never;
                btnNew.Visibility = BarItemVisibility.Never;
                btnSave_As.Visibility = BarItemVisibility.Never;
                btnNewProject.Visibility = BarItemVisibility.Never;
                // Country tools
                btnImportSystems.Visibility = BarItemVisibility.Never;
                btnExportSystems.Visibility = BarItemVisibility.Never;
                btnImportAddOn.Visibility = BarItemVisibility.Never;
                btnExportAddOn.Visibility = BarItemVisibility.Never;
                btnMergeCountry.Visibility = BarItemVisibility.Never;
                btnRestore.Visibility = BarItemVisibility.Never;
                ribbonPageGroupCompareVersions.Visible = false;
                // Admin
                ribbonPageGroupCountry.Visible = false;
                btnVariablesMerge.Visibility = BarItemVisibility.Never;
                ribbonPageGroupDeveloperInfo.Visible = false;
                ribbonPageGroupPublicVersion.Visible = false;
                ribbonPageGroupExtract.Visible = false;
                // rest
                ribbonVersionControl.Visible = false;
                ribbonPageGroupOpenOutputFile.Visible = false;
            }
        }

        internal void SetVCButtonsGreyState()
        {
            VCUIAPI.VCAPI vcAPI = EM_AppContext.Instance.GetVCAdministrator()._vcAPI;

            bool loggedIn = vcAPI.isLoggedIn;
            bool vcControlled = EM_AppContext.Instance.GetVCAdministrator().IsProjectVersionControlled();
            bool isAdminUser = vcAPI.getIsCurrentUserCurrentProjectAdmin();
            bool hasWritingPermissions = vcAPI.hasWritingPermissions();
            bool isCurrentUserMerging = vcAPI.isCurrentUserMerging;
            bool isSomeoneCurrentlyMerging = vcAPI.isCurrentlyMerging;
            bool isThereABundle = vcAPI.existAnyBundle(); 
            bool isThisLatestBundle = vcAPI.isThisLatestBundle();

            SetState_btnVCLogInOut(loggedIn);
            btnVCCountryMerge.Enabled = _treeListManager != null;
            string what = _isAddOn ? "add-On" : "country";
            btnVCCountryMerge.Caption = "Compare && merge " + what;
            btnVCVariablesMerge.Enabled = true;
            btnVCEstablishProject.Enabled = loggedIn;
            btnVCRemoveProject.Enabled = loggedIn; 
            btnVCDisConnect.Enabled = loggedIn;
            btnVCSettings.Enabled = !loggedIn;
            SetState_btnVCDisConnect(vcControlled);
            btnVCAdministrateContent.Enabled = loggedIn && vcControlled && !isSomeoneCurrentlyMerging && isAdminUser;
            btnVCAdministrateUsers.Enabled = loggedIn && vcControlled && isAdminUser;
            btnVCNewProject.Enabled = true;
            btnVCRemoveBundle.Enabled = loggedIn && vcControlled && hasWritingPermissions;

            btnVCDownloadBundle.Enabled = loggedIn;
            btnVCStartMerging.Enabled = loggedIn && vcControlled && hasWritingPermissions && !isSomeoneCurrentlyMerging && isThereABundle;
            btnVCDownloadLatestBundle.Enabled = loggedIn && vcControlled && isCurrentUserMerging && isThereABundle && !isThisLatestBundle;
            btnVCMergeCountryOnlineBundle.Caption = "Merge " + what;
            btnVCMergeCountryOnlineBundle.Enabled = _treeListManager != null && loggedIn && vcControlled && isCurrentUserMerging && isThisLatestBundle;
            btnVCMergeVariablesOnlineBundle.Enabled = loggedIn && vcControlled && isCurrentUserMerging && isThisLatestBundle;
            btnVCFinishMerging.Enabled = loggedIn && vcControlled && isCurrentUserMerging && isThisLatestBundle;
            btnVCAbortMerging.Enabled = loggedIn && vcControlled && isCurrentUserMerging;
            btnVCNewBundleLocalVersion.Enabled = loggedIn && vcControlled && isCurrentUserMerging;
            UpdateMainFormCaption();
        }

        internal void SetState_btnVCDisConnect(bool linked)
        {
            btnVCDisConnect.Caption = linked ? "Disconnect" : "Connect";
            btnVCDisConnect.LargeGlyph = linked ? Properties.Resources.vc_unlink : Properties.Resources.vc_connect;
        }

        internal void SetState_btnVCLogInOut(bool setDoorOpen)
        {
            btnVCLogInOut.LargeGlyph = setDoorOpen ? Properties.Resources.log_out_vc : Properties.Resources.log_in_vc;
            btnVCLogInOut.Caption = setDoorOpen ? "Log Out" : "Log In";
        }

        internal bool IsDoorOpen_btnVCLogInOut() { return btnVCLogInOut.Caption == "Log Out"; }

        internal void SetCountryGalleryText(string text)
        {
            ribbonPageGroupCountries.Text = text; //shows a hint that not all countries are visible without scrolling
        }

        internal void ReloadCountry()
        {
            CountryAdministrator.ResetConfigFacades(_countryShortName);
            LoadCountry();
        }

        internal void UpdateTree()
        {
            BaseAction emptyAction = new BaseAction();
            emptyAction.SetNoCommitAction();
            PerformAction(emptyAction, true, true); //"empty-action" to enforce update of tree
        }

        internal void UpdateChkShowFunctionSpecifiers()
        {
            chkShowFunctionSpecifiers.Caption = EM_AppContext.Instance._showFunctionSpecifiers ? "Hide Key Parameters" : "Show Key Parameters";
            treeList.OptionsView.ShowPreview = EM_AppContext.Instance._showFunctionSpecifiers;
        }

        void EM_MainRibbon_ShowCustomizationMenu(object sender, RibbonCustomizationMenuEventArgs e)
        {//prevent users from being able to delete the fixed toolbar-buttons (i.e. the undo- and redo-buttons) by hiding the respective menu-item
            e.CustomizationMenu.ItemLinks[1].Visible = !(e.HitInfo.HitTest == DevExpress.XtraBars.Ribbon.ViewInfo.RibbonHitTest.Item &&
                (e.HitInfo.Item.Caption == btnUndo.Name || e.HitInfo.Item.Caption == btnRedo.Caption));
        }

        #endregion display_gallery_and_countries

        #region treelist_events

        void treeList_KeyDown(object sender, KeyEventArgs e) { if (_treeListManager != null) _treeListManager.HandleKeyDown(e, _countryConfigFacade, _undoManager); }
        void treeList_KeyUp(object sender, KeyEventArgs e) { if (_treeListManager != null) _treeListManager.HandleKeyUp(e); }
        void treeList_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e) { if (_treeListManager != null) _treeListManager.HandleKeyPress(e); }
        void treeList_MouseDown(object sender, MouseEventArgs e) { if (_treeListManager != null) _treeListManager.HandleMouseDown(e, MousePosition, ModifierKeys); }
        void treeList_MouseUp(object sender, MouseEventArgs e) { if (_treeListManager != null) _treeListManager.HandleMouseUp(e, MousePosition, ModifierKeys); }
        void treeList_PopupMenuShowing(object sender, PopupMenuShowingEventArgs e) { if (_treeListManager != null) _treeListManager.HandleColumnMenuShowing(e); }
        void treeList_CustomNodeCellEdit(object sender, GetCustomNodeCellEditEventArgs e) { if (_treeListManager != null) _treeListManager.CreateRespectiveEditor(e); }
        void treeList_ShowingEditor(object sender, CancelEventArgs e) { if (_treeListManager != null) _treeListManager.HandleShowingEditor(e); }
        void treeList_ValidatingEditor(object sender, DevExpress.XtraEditors.Controls.BaseContainerValidateEditorEventArgs e) { if (_treeListManager != null) _treeListManager.ValidateEditorInput(e); }
        void treeList_TopVisibleNodeIndexChanged(object sender, EventArgs e) { if (_treeListManager != null) _treeListManager.StopFormulaEditing(); }
        void treeList_LeftCoordChanged(object sender, EventArgs e) { if (_treeListManager != null) _treeListManager.StopFormulaEditing(); }
        void treeList_DragDrop(object sender, DragEventArgs e) { if (_treeListManager != null) _treeListManager.HandleDragDrop(e); }
        void treeList_CustomDrawNodeIndicator(object sender, CustomDrawNodeIndicatorEventArgs e) { if (_treeListManager != null) _treeListManager.DrawNodeRowNumber(e); }
        void treeList_CellValueChanged(object sender, CellValueChangedEventArgs e) { if (_treeListManager != null) _treeListManager.HandleCellValueChanged(e.Value.ToString(), e.Column, e.Node); }
        void OnOffToggleEditor_EditValueChanged(object sender, EventArgs e) { if (_treeListManager != null) _treeListManager.HandleSwitchToNAallComponents(); }
        void treeList_ShowCustomizationForm(object sender, System.EventArgs e) { treeList.CustomizationForm.Text = _titleHiddenSystemsBox; }
        void treeList_GetPreviewText(object sender, GetPreviewTextEventArgs e) { if (e.Node != null && e.Node.Tag != null) { if (!treeList.OptionsView.ShowPreview) return; BaseTreeListTag treeListTag = (e.Node.Tag as BaseTreeListTag); e.PreviewText = treeListTag.GetFunctionSpecifier(e); } }


        //to avoid "broken selection" if e.g. a not selected column is dragged within a selection or nodes are collapsed/expanded
        void treeList_DragObjectDrop(object sender, DragObjectDropEventArgs e) { ClearCellSelection(); }
        void treeList_BeforeCollapse(object sender, BeforeCollapseEventArgs e) { ClearCellSelection(); }
        void treeList_BeforeExpand(object sender, BeforeExpandEventArgs e) { ClearCellSelection(); }

        //show private comment or info produced by compare versions, if existing, as a tool tip
        void toolTipController_GetActiveObjectInfo(object sender, DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e)
        {
            if (e.SelectedControl == null || !e.SelectedControl.Equals(this.treeList))
                return;

            TreeListHitInfo hit = this.treeList.CalcHitInfo(e.ControlMousePosition);
            if (HandledByShowingSystemComment(e, hit)) return;

            if (EM_AppContext.Instance.IsPublicVersion() && _importByIDDiscrepancies == null)
                return; //avoid doing anything ... for performance reasons (update: I think there are no performance issues, but it does no harm)

            string toolTipToShow = string.Empty;
            if (hit.HitInfoType == HitInfoType.Cell && hit.Node.Tag != null)
            {
                BaseTreeListTag nodeTag = (hit.Node.Tag as BaseTreeListTag);

                if (TreeListBuilder.IsCommentColumn(hit.Column)) //get and show private comment if exists
                    toolTipToShow = (nodeTag.GetPrivateComment() != "\"\"") ? nodeTag.GetPrivateComment() : string.Empty;
                else if (TreeListBuilder.IsGroupColumn(hit.Column)) //get and show info about comparison-discrepancy if exists
                    toolTipToShow = (_importByIDDiscrepancies == null) ? string.Empty
                                    : _importByIDDiscrepancies.GetDiscrepancy(nodeTag.GetIDsWithinAllSystems(), (from sys in _countryConfigFacade.GetSystemRows() select sys.ID).ToList());
                else if (TreeListBuilder.IsSystemColumn(hit.Column))
                {
                    string parValue = hit.Node.GetDisplayText(hit.Column);
                    //show a "user-friendly" representation of a unique-id which refers to a function or parameter
                    Guid dummy;
                    if (Guid.TryParse(parValue, out dummy))
                    {
                        TreeListNode refNode = _treeListManager.GetSpecifiedNode(parValue);
                        if (refNode == null) return;
                        string rowNumber = TreeListManager.GetNodeRowNumber(refNode, refNode.ParentNode);
                        CountryConfig.FunctionRow functionRow = _countryConfigFacade.GetFunctionRowByID(parValue);
                        string systemID = (hit.Column.Tag as SystemTreeListTag).GetSystemRow().ID;
                        if (functionRow != null)
                        {
                            toolTipToShow = string.Format("{0} - {1} ({2})", functionRow.PolicyRow.Name, functionRow.Name, rowNumber);
                            if (functionRow.PolicyRow.SystemRow.ID != systemID)
                                toolTipToShow += Environment.NewLine + string.Format("!!! INVALID !!! (belongs to system {0})", functionRow.PolicyRow.SystemRow.Name);
                        }
                        else
                        {
                            CountryConfig.ParameterRow parameterRow = _countryConfigFacade.GetParameterRowByID(parValue);
                            if (parameterRow != null)
                            {
                                toolTipToShow = string.Format("{0} - {1} - {2} ({3})", parameterRow.FunctionRow.PolicyRow.Name, parameterRow.FunctionRow.Name, parameterRow.Name, rowNumber);
                                if (parameterRow.FunctionRow.PolicyRow.SystemRow.ID != systemID)
                                    toolTipToShow += Environment.NewLine + string.Format("!!! INVALID !!! (belongs to system {0})", parameterRow.FunctionRow.PolicyRow.SystemRow.Name);
                            }
                        }
                    }
                    //show value of constants if cell contains any
                    else
                    {
                        if (e.Info != null) toolTipToShow = e.Info.Text; //default shows complete cell-value if it does not fit into cell (add to that)
                        if (toolTipToShow != string.Empty) toolTipToShow += Environment.NewLine + Environment.NewLine;
                        toolTipToShow += FormulaEditorManager.GetDisplayText_ValueOfConstants(_countryConfigFacade, hit.Column.Tag as SystemTreeListTag, parValue);
                    }
                }
            }

            if (ExtensionAndGroupManager.GetExtensionToolTip(_countryShortName, hit, out string tt))
                toolTipToShow += (string.IsNullOrEmpty(toolTipToShow) ? string.Empty : Environment.NewLine) + tt;

            if (toolTipToShow != string.Empty)
                e.Info = new DevExpress.Utils.ToolTipControlInfo(new DevExpress.XtraTreeList.ViewInfo.TreeListCellToolTipInfo(hit.Node, hit.Column, null), toolTipToShow);
        }

        private bool HandledByShowingSystemComment(DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventArgs e, TreeListHitInfo hit)
        {
            if (hit.HitInfoType != HitInfoType.Column || !TreeListBuilder.IsSystemColumn(hit.Column)) return false;
            string sysComment = (hit.Column.Tag as SystemTreeListTag).GetSystemRow().Comment;
            if (!string.IsNullOrEmpty(sysComment))
                e.Info = new DevExpress.Utils.ToolTipControlInfo(new DevExpress.XtraTreeList.ViewInfo.TreeListCellToolTipInfo(
                    hit.Node, hit.Column, null), sysComment);
            return true;
        }

        void treeList_FocusedNodeChanged(object sender, FocusedNodeChangedEventArgs e)
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift) //prepare selection if shift-key pressed
            {
                if (_currentKeyState != null && _currentKeyState.KeyCode == Keys.Insert)
                    return; //to avoid generating a selection when Shift-Insert is pressed for pasting
                if (_startNode == null)     //selection may have started with a previous focus-change, if selecting with arrow-keys
                    _startNode = e.OldNode; //otherwise selection starts with the node, which just lost focus
            }
            _endNode = e.Node; //always record the currently focused node (amongst others to be checked in FocusedColumnChanged)
            if (_endColumn == treeList.FocusedColumn) //call FocusedCellChanged only once if a pair FocusedNodeChanged/FocusedColumnChanged causes it
                treeList_FocusedCellChanged();        //if the recorded _endColumn does not correspond with the currently focused column, FocusedColumnChanged has yet to be called-back

            if (EM_AppContext.Instance.GetAddParameterForm().Visible)
                EM_AppContext.Instance.GetAddParameterForm().UpdateContent(e.Node);

            txtInfoInStatusBar.Caption = _treeListManager.WhatIsWorkedOn(e.Node);
        }

        void treeList_FocusedColumnChanged(object sender, FocusedColumnChangedEventArgs e)
        { //see comments treeList_FocusedNodeChanged (approach is the same)
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                if (_currentKeyState != null && _currentKeyState.KeyCode == Keys.Insert)
                    return; //to avoid generating a selection when Shift-Insert is pressed for pasting
                if (_startColumn == null)
                    _startColumn = e.OldColumn;
            }
            _endColumn = e.Column;
            if (_endNode == treeList.FocusedNode)
                treeList_FocusedCellChanged();
            return;
        }

        void treeList_FocusedCellChanged() //function is called by either treeList_FocusedNodeChanged or treeList_FocusedColumnChanged
        {                                  //if both function were called-back, care is taken, that treeList_FocusedCellChanged is only called once
            //select the respective cells if shift-key is pressed
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                _startNode = _startNode == null ? _endNode : _startNode;
                _startColumn = _startColumn == null ? _endColumn : _startColumn;
                if (_startNode != null && _endNode != null && _startColumn != null && _endColumn != null)
                    _multiCellSelector.SelectCells(_startNode, _endNode, _startColumn, _endColumn);
            }
            //unselect cells if focus is changed without pressing shift ...
            else if (_multiCellSelector != null && _multiCellSelector.HasSelection())
            {
                //... but ony if the now focused cell is outside the selection (thus to allow context menus to be opened for the selection)
                if ((_endNode != null && !_multiCellSelector.GetSelectedNodes().Contains(_endNode)) ||
                    (_endColumn != null && !_multiCellSelector.GetSelectedColumns().Contains(_endColumn)))
                {
                    ClearCellSelection();
                    treeList.Refresh();
                }
            }
        }

        void treeList_NodeCellStyle(object sender, GetCustomNodeCellStyleEventArgs cellStyleEventArgs)
        {
            if (_countUnboundLoadCalls > 0 || _countUpdateCalls > 0 || chkSuspendSystemFormatting.Checked)
                return; //to enhance performance - no need to care about node colors while treeview-drawing is in progress

            //do not destroy display of selection
            if (treeList.FocusedColumn != null && treeList.FocusedColumn != null &&
                treeList.FocusedNode == cellStyleEventArgs.Node && treeList.FocusedColumn == cellStyleEventArgs.Column)
                return;

            bool customStyle = false;

            //lowest priority: display if the user set a custom node color
            int argbColor = (cellStyleEventArgs.Node.Tag as BaseTreeListTag).GetSpecialNodeColor();
            if (argbColor != DefPar.Value.NO_COLOR)
            {
                cellStyleEventArgs.Appearance.BackColor = Color.FromArgb(argbColor);
                customStyle = true;
            }

            //next priority: conditional-formatting and base/derived-formatting
            if (_specialFormatCells.Keys.Contains(cellStyleEventArgs.Column) && _specialFormatCells[cellStyleEventArgs.Column].Keys.Contains(cellStyleEventArgs.Node))
            {
                if (_specialFormatCells[cellStyleEventArgs.Column][cellStyleEventArgs.Node].Key != Color.Empty)
                    cellStyleEventArgs.Appearance.BackColor = _specialFormatCells[cellStyleEventArgs.Column][cellStyleEventArgs.Node].Key;
                if (_specialFormatCells[cellStyleEventArgs.Column][cellStyleEventArgs.Node].Value != Color.Empty)
                    cellStyleEventArgs.Appearance.ForeColor = _specialFormatCells[cellStyleEventArgs.Column][cellStyleEventArgs.Node].Value;
                customStyle = true;
            }

            //highest priority: display if a node is selected (with the multi-cell-selector)
            if (_multiCellSelector != null && _multiCellSelector.IsCellSelected(new CellReferenz(cellStyleEventArgs.Node, cellStyleEventArgs.Column)))
            {
                cellStyleEventArgs.Appearance.BackColor = Color.FromArgb(183, 219, 255);
                customStyle = true;
            }

            //the 'usual' node color i.e. white for policies/parameters, lavendar for functions
            if (!customStyle && cellStyleEventArgs.Node.Tag != null)
            {
                cellStyleEventArgs.Appearance.BackColor = (cellStyleEventArgs.Node.Tag as BaseTreeListTag).GetBackColor();
                cellStyleEventArgs.Appearance.Font = (cellStyleEventArgs.Node.Tag as BaseTreeListTag).GetFont(cellStyleEventArgs.Appearance.Font);
            }
        }

        void treeList_CustomDrawNodeCell(object sender, DevExpress.XtraTreeList.CustomDrawNodeCellEventArgs drawNodeCellEventArgs)
        {
            //draw little red square in comments column to indicate private comment, or in group column to indicate info produced by compare versions
            if (drawNodeCellEventArgs.Node.Tag != null &&
                (!EM_AppContext.Instance.IsPublicVersion() || _importByIDDiscrepancies != null)) //for performance reasons avoid doing anything if there is neither need to show private comment (public version) nor any info
            {
                bool significantDiscrepancy = true;
                if (!EM_AppContext.Instance.IsPublicVersion() &&
                        (TreeListBuilder.IsCommentColumn(drawNodeCellEventArgs.Column) && //check if there is a private comment for the policy/function/parameter
                        (drawNodeCellEventArgs.Node.Tag as BaseTreeListTag).GetPrivateComment() != string.Empty &&
                        (drawNodeCellEventArgs.Node.Tag as BaseTreeListTag).GetPrivateComment() != "\"\"") ||
                    (_importByIDDiscrepancies != null && //check if there is any info as produced by compare versions for the policy/function/parameter
                        TreeListBuilder.IsGroupColumn(drawNodeCellEventArgs.Column) &&
                        _importByIDDiscrepancies.HasDiscrepancy((drawNodeCellEventArgs.Node.Tag as BaseTreeListTag).GetIDsWithinAllSystems(), out significantDiscrepancy)))
                {
                    drawNodeCellEventArgs.Graphics.FillRectangle(new SolidBrush(drawNodeCellEventArgs.Appearance.BackColor), drawNodeCellEventArgs.Bounds); //first draw background
                    drawNodeCellEventArgs.Graphics.FillRectangle(new SolidBrush(significantDiscrepancy ? Color.Red : Color.Green),
                            new Rectangle(drawNodeCellEventArgs.Bounds.Location, new Size(5, 5))); //then, if necessary, produce small rectangle for private comment
                    drawNodeCellEventArgs.Graphics.DrawString(drawNodeCellEventArgs.CellText, drawNodeCellEventArgs.Appearance.Font, //redraw text
                        new SolidBrush(drawNodeCellEventArgs.Appearance.ForeColor), drawNodeCellEventArgs.Bounds, drawNodeCellEventArgs.Appearance.GetStringFormat());
                    drawNodeCellEventArgs.Handled = true; // prohibiting default painting
                }
            }

            //make border of focused cell more pronounced, in particular to not overlook the focus, if cells are selected (with the multi-cell-selector)
            if (drawNodeCellEventArgs.Node.Focused && drawNodeCellEventArgs.Column == treeList.FocusedColumn)
            {
                Pen pen = new Pen(Color.DarkSalmon, 2F);
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                int penWidth = Convert.ToInt32(pen.Width);
                Point pointTopLeft = new Point(drawNodeCellEventArgs.Bounds.Left + penWidth,
                                                drawNodeCellEventArgs.Bounds.Top + penWidth);
                Point pointTopRight = new Point(pointTopLeft.X + drawNodeCellEventArgs.Bounds.Width - 2 * penWidth,
                                                pointTopLeft.Y);
                Point pointBotLeft = new Point(pointTopLeft.X,
                                                pointTopLeft.Y + drawNodeCellEventArgs.Bounds.Height - 2 * penWidth);
                Point pointBotRight = new Point(pointTopRight.X, pointBotLeft.Y);
                drawNodeCellEventArgs.Graphics.DrawLine(pen, pointTopLeft, pointTopRight);
                drawNodeCellEventArgs.Graphics.DrawLine(pen, pointTopRight, pointBotRight);
                drawNodeCellEventArgs.Graphics.DrawLine(pen, pointBotRight, pointBotLeft);
                drawNodeCellEventArgs.Graphics.DrawLine(pen, pointBotLeft, pointTopLeft);
            }
        }

        //make sure that unbound load is not called "nested", i.e. treelist is already in unbound load, while begin/end is called again (i.e. ended to soon)
        int _countUnboundLoadCalls = 0;
        int _countUpdateCalls = 0;

        internal void TreeListBeginUpdate()
        {
            if (_countUpdateCalls == 0)
                treeList.BeginUpdate();
            ++_countUpdateCalls;
        }

        internal void TreeListEndUpdate()
        {
            _countUpdateCalls = Math.Max(_countUpdateCalls - 1, 0);
            if (_countUpdateCalls == 0)
                treeList.EndUpdate();
        }

        internal void TreeListBeginUnboundLoad()
        {
            if (_countUnboundLoadCalls == 0)
                treeList.BeginUnboundLoad();
            ++_countUnboundLoadCalls;
        }

        internal void TreeListEndUnboundLoad()
        {
            _countUnboundLoadCalls = Math.Max(_countUnboundLoadCalls - 1, 0);
            if (_countUnboundLoadCalls == 0 && treeList.IsUnboundMode)
                treeList.EndUnboundLoad();
        }

        internal void ClearCellSelection()
        {
            _startColumn = null;
            _startNode = null;
            if (_multiCellSelector != null)
                _multiCellSelector.Clear();
        }

        #endregion treelist_events

        #region perform_action

        internal static object _performActionLock = new object();

        internal bool PerformAction(BaseAction action, bool updateNodes = true, bool updateColumns = false,
                                    TreeListNode updateOnlyThisFunctionNode = null) //performance optimisation Aug 13: parameter added
        {
            lock (_performActionLock)
            {
                Cursor bkupCursor = Cursors.Default;
                try
                {
                    List<string> idsOfHiddenSystems = _treeListManager.GetIDsOfHiddenSystems();
                    if (action.ShowHiddenSystemsWarning() && idsOfHiddenSystems.Count > 0)
                    {
                        if (!OptionalWarningsManager.Show(OptionalWarningsManager._hiddenSystemstemWarning))
                            return true;
                    }

                    bkupCursor = Cursor;
                    Cursor = Cursors.WaitCursor;

                    //store which nodes are expanded or hidden, and which node is focused - to restore these states after redrawing
                    StoreRestoreNodeStates storeNodeStates = new StoreRestoreNodeStates();
                    if (updateNodes)
                    {
                        treeList.NodesIterator.DoOperation(storeNodeStates);
                        _treeListBuilder.StoreColumnStates();
                    }
                    //store position to scroll back to it after redrawing
                    int topVisibleNodeIndex = treeList.TopVisibleNodeIndex;
                    int LeftCoord = treeList.LeftCoord;

                    TreeListBeginUpdate();  //essential for performance (suppress update operations during load)

                    action.PerformAction();

                    if (action.ActionIsCanceled() == true)
                    {
                        Cursor = Cursors.Default;
                        TreeListEndUpdate();
                        return true;
                    }

                    if (!action.IsNoCommitAction())
                        _undoManager.Commit();

                    action.DoAfterCommitWork(); // do work (on tree) after committing (to do updates in tree without having to take not yet committed data-changes into account)

                    if (!action.ToString().Contains("SaveAction")) //otherwise the flag is set by auto-saving
                        ++_hasChanges;

                    SetButtonGreyState();

                    if (updateColumns)
                    {//complete redrawing of the treelist
                        treeList.Nodes.Clear();
                        _treeListBuilder.BuildTreeList();
                    }
                    else
                    {
                        foreach (TreeListColumn col in _treeListBuilder.GetSystemColums())
                            (col.Tag as SystemTreeListTag).UpdateEditors(); //editors for TUs, ILs, etc. need to be updated to reflect eventually added items
                    }

                    if (updateNodes)
                    {
                        if (updateOnlyThisFunctionNode != null) //performance optimisation Aug 13: if-branch added
                        {
                            ClearCellSelection();
                            _treeListBuilder.RefillFunctionNode(updateOnlyThisFunctionNode);
                            treeList.NodesIterator.DoOperation(storeNodeStates);
                        }
                        else
                        {
                            treeList.Nodes.Clear();
                            ClearCellSelection();
                            _treeListBuilder.FillTreeList();
                            if (treeList.Columns.Count > 0 && treeList.Nodes.Count > 0) //this condition is a not optimal solution for avoiding a crash if all systems respectively all policies are deleted
                                treeList.Refresh(); //Refresh searches for not existing data-rows (?)

                            treeList.NodesIterator.DoOperation(storeNodeStates);
                            _treeListBuilder.GetCommentColumn().Width = 5; //unless doing this the comments column does not adapt its height to view the full comment
                            _treeListBuilder.RestoreColumnStates();

                            treeList.LeftCoord = LeftCoord;
                        }
                        treeList.TopVisibleNodeIndex = topVisibleNodeIndex;
                    }

                    if (!updateNodes || updateOnlyThisFunctionNode != null) //set backcolor and foreColor as defined by the user (in the else-case done within FillTreeList)
                    {
                        if (!chkSuspendSystemFormatting.Checked)
                            _treeListBuilder.SetSystemFormats();
                    }

                    if (updateColumns)
                    {
                        //put columns back to column chooser, if they were there before the update
                        foreach (string idOfHiddenSystem in idsOfHiddenSystems)
                        {
                            TreeListColumn hiddenColumn = _treeListBuilder.GetSystemColumnByID(idOfHiddenSystem);
                            if (hiddenColumn != null)
                            {
                                hiddenColumn.OptionsColumn.ShowInCustomizationForm = true;
                                hiddenColumn.VisibleIndex = -1;
                            }
                        }

                        //for whatever reason, if columns are updated, the row height changes to quite high (without reference to the cell content)
                        treeList.BestFitColumns();  //this solves the problem (but actually don't know why it happens)
                        _treeListBuilder.RestoreColumnStates();
                    }
                }
                catch (Exception exception)
                {
                    Cursor = Cursors.Default;
                    autoSaveTimer.Stop(); //avoid autosaving of a damaged version
                    Tools.UserInfoHandler.ShowException(exception);
                    return false;
                }
                finally
                {
                    TreeListEndUpdate();
                    Cursor = bkupCursor; //set cursor back to what it was before
                }

                //maybe there is a smarter way, but this scrolls the treeview such that the focused column is visible
                TreeListColumn focusedColumn = treeList.FocusedColumn;
                if (treeList.FocusedColumn == null)
                    return true;
                treeList.FocusedColumn = treeList.Columns[0];
                treeList.FocusedColumn = focusedColumn;

                return true;
            }
        }

        internal void SuspendSystemFormatting(bool suspend) { chkSuspendSystemFormatting.Checked = suspend; }

        #endregion perform_action

        #region ribbon_events

        void btnRun_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { if (_runMainForm == null || _runMainForm.IsDisposed) _runMainForm = new RunMainForm(); _runMainForm.Show(_countryShortName); }
        void btnUpratingIndices_ItemClick(object sender, ItemClickEventArgs e) { (new UpratingIndices.UpratingIndicesForm(this)).ShowDialog(); }
        void btnIndirectTaxes_ItemClick(object sender, ItemClickEventArgs e) { new IndirectTaxes.IndirectTaxesForm(this).ShowDialog(); }
        void btnHICP_ItemClick(object sender, ItemClickEventArgs e) { GlobalAdministrator.ShowHICPDialog(); }
        void btnExchangeRates_ItemClick(object sender, ItemClickEventArgs e) { GlobalAdministrator.ShowExchangeRatesDialog(); }
        void btnCloseCountry_ItemClick(object sender, ItemClickEventArgs e) { Close(); }
        void btnCloseAllCountries_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.CloseAllMainForms(false); }
        void btnExit_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { EM_AppContext.Instance.CloseAllMainForms(true); }
        void btnOpen_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { (new ConfigurePathsForm()).ShowDialog(); }
        void btnConfig_ItemClick(object sender, ItemClickEventArgs e) { ConfigurationForm configurationForm = new ConfigurationForm(); configurationForm.ShowDialog(); }
        void btnSave_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { treeList.CloseEditor(); WriteXml(); ViewKeeper.StoreSettings(this); EM_AppContext.Instance.StoreViewSettings(); SetButtonGreyState(); }
        void btnSearchPolFuncPar_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { _treeListManager.ShowComponentSearchForm(); }
        void btnUndo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { PerformAction(new UndoAction(this._undoManager), true, true); }
        void btnRedo_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { PerformAction(new RedoAction(this._undoManager), true, true); }
        void btnConfigSystems_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { PerformAction(new ConfigSystemsAction(_countryShortName, _countryConfigFacade.GetSystemDataTable()), false, false); }
        void btnConfigCountry_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { PerformAction(new ConfigCountryAction(_countryShortName, _countryConfigFacade.GetCountryRow()), false, false); }
        void btnSearchReplace_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { EM_AppContext.Instance.GetFindReplaceForm().Show(false); }
        void btnComponentUse_ItemClick(object sender, ItemClickEventArgs e) { ShowComponentUseForm(); }
        void btnExpandPrivate_ItemClick(object sender, ItemClickEventArgs e) { _treeListManager.HandleExpandPrivate(); ; }
        void btnConditionalFormatting_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { _treeListManager.HandleConditionalFormatting(); }
        void btnAddSystem_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { _treeListManager.AddSystem(); }
        void btnDeleteSystems_ItemClick(object sender, ItemClickEventArgs e) { _treeListManager.DeleteSystems(); }
        void btnVariables_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { EM_AppContext.Instance.ShowVariablesForm(); }
        void btnPublicVersion_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { PublicVersion.Generate(); }
        void btnExtractProject_ItemClick(object sender, ItemClickEventArgs e) { (new ExtractProjectForm()).ShowDialog(); }
        void btnAddCountry_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { CountryAdministrator.AddCountry(); }
        void btnAddAddOn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { CountryAdministrator.AddCountry(true); }
        void btnDeleteCountry_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { CountryAdministrator.DeleteCountry(); }
        void btnDeleteAddOn_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { CountryAdministrator.DeleteCountry(true); }
        void btnImportCountry_ItemClick(object sender, ItemClickEventArgs e) { CountryAdministrator.ImportCountry(); }
        void btnImportSystems_ItemClick(object sender, ItemClickEventArgs e) { ImportExportAdministrator.ImportSystems(this, _countryConfigFacade, _dataConfigFacade); }
        void btnExportSystems_ItemClick(object sender, ItemClickEventArgs e) { ImportExportAdministrator.ExportSystems(this, _countryConfigFacade, _dataConfigFacade); }
        void btnCleanUpSystems_ItemClick(object sender, ItemClickEventArgs e) { ImportExportAdministrator.CleanUpCountryFromNA(this, _countryConfigFacade, _dataConfigFacade); }
        void btnImportAddOn_ItemClick(object sender, ItemClickEventArgs e) { ImportExportAdministrator.ImportAddOn(this, _countryConfigFacade, _dataConfigFacade); }
        void btnExportAddOn_ItemClick(object sender, ItemClickEventArgs e) { ImportExportAdministrator.ExportAddOn(this, _countryConfigFacade, _dataConfigFacade); }
        void btnCompareVersions_ItemClick(object sender, ItemClickEventArgs e) { ImportExportAdministrator.CompareVersions(this, _countryConfigFacade); }
        void btnStoreInfoMarkers_ItemClick(object sender, ItemClickEventArgs e) { if (_importByIDDiscrepancies != null) _importByIDDiscrepancies.WriteToFile(); }
        void btnLoadInfoMarkers_ItemClick(object sender, ItemClickEventArgs e) { _importByIDDiscrepancies = ImportExportAdministrator.LoadImportByIDDiscrepancies(this); SetButtonGreyState(); }
        void btnRemoveInfoMarkers_ItemClick(object sender, ItemClickEventArgs e) { if (_importByIDDiscrepancies == null) return; _importByIDDiscrepancies.Clear(); _importByIDDiscrepancies = null; SetButtonGreyState(); treeList.Refresh(); }
        //internal void NotYetImplemented() { (new NotYetImplementedForm()).ShowDialog(); }
        void btnReleaseValidation_ItemClick(object sender, ItemClickEventArgs e) { DeveloperInfo.ReleaseValidation.RVAdministrator.ShowDialog(); }
        void chkSuspendSystemFormatting_CheckedChanged(object sender, ItemClickEventArgs e) { chkSuspendSystemFormatting.Caption = (chkSuspendSystemFormatting.Checked == true) ? "Restore System Formatting" : "Suspend System Formatting"; }
        void chkShowFunctionSpecifiers_CheckedChanged(object sender, ItemClickEventArgs e) { EM_AppContext.Instance._showFunctionSpecifiers = !EM_AppContext.Instance._showFunctionSpecifiers; EM_AppContext.Instance.UpdateAllCountryMainFormChkShowFunctionSpecifiers(); }
        void btnUpdatingProgress_ItemClick(object sender, ItemClickEventArgs e) { UpdatingProgressForm updatingProgressForm = new UpdatingProgressForm(); updatingProgressForm.ShowDialog(); }
        void btnSaveAsText_ItemClick(object sender, ItemClickEventArgs e) { SaveParameterFilesAsTextForm saveParameterFilesAsTextForm = new SaveParameterFilesAsTextForm(); saveParameterFilesAsTextForm.ShowDialog(); }
        void PolicyViewButton_ItemClick(object sender, ItemClickEventArgs e) { _treeListBuilder.PolicyViewChanged(e.Item.Name == txtSinglePolicy.Name || e.Item.Name == chkSinglePolicy.Name); }
        void btnRestore_ItemClick(object sender, ItemClickEventArgs e) { RestoreManager.RestoreCountry(this); }

        void btnHelp_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); Help.ShowHelp(this, helpPath); }
        void btnVersion_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { Tools.UserInfoHandler.ShowInfo(DefGeneral.UI_VERSION); }
        private void btnLicence_ItemClick(object sender, ItemClickEventArgs e){ try { ShowLicenceForm licenceForm = new ShowLicenceForm(); licenceForm.Show(); } catch(Exception){ MessageBox.Show("The licence file cannot be displayed."); }
        
        }

        void btnConfigData_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            ConfigDataAction configDataAction = new ConfigDataAction(_countryShortName, _dataConfigFacade);
            PerformAction(configDataAction, false);
            if (!configDataAction.ActionIsCanceled())
            {
                UpdateUpratingIndices("Please note that uprating-indices are now updated in accordance with your changes to datasets.");
                if (configDataAction.DatasetAdded() &&
                    _dataConfigFacade.GetDataConfig().PolicySwitch.Count > 0 &&
                    UserInfoHandler.GetInfo("Do you want to define Policy Switches for the new dataset(s) now?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    PerformAction(new ExtensionSetSwitchesAction(_countryShortName), false);
            }
        }
        internal void UpdateUpratingIndices(string request)
        {
            if (_countryConfigFacade.GetSystemRows().Count > 0 &&
                _countryConfigFacade.GetPolicyRowByName(_countryConfigFacade.GetSystemRows().First().ID,
                                                        EM_UI.UpratingIndices.UpratingIndicesForm._policyUprateFactors_Name) != null &&
                UserInfoHandler.GetInfo(request, MessageBoxButtons.OKCancel) == System.Windows.Forms.DialogResult.OK)
                PerformAction(new UpratingIndicesAction(new UpratingIndicesForm(this)), false);
        }

        //Version-Control
        void btnVCCountryMerge_ItemClick(object sender, ItemClickEventArgs e) { (new MergeAdministrator(this, false)).ShowChoices(false); }
        void btnVCVariablesMerge_ItemClick(object sender, ItemClickEventArgs e) { (new MergeAdministrator(this, true)).ShowChoices(false); }
        void btnVCLogInOut_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonLogInOutClicked(); }
        void btnVCEstablishProject_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonEstablishProjectClicked(); }
        void btnVCRemoveProject_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonRemoveProjectClicked(); }
        void btnVCRemoveBundle_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonRemoveBundle(); }
        void btnVCLinkUnLink_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonDisConnectClicked(); }
        void btnVCAdministrateContent_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonAdministrateContent(); }
        void btnVCDownloadBundle_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonDownloadRelease(); }
        void btnVCRemovePrivate_ItemClick(object sender, ItemClickEventArgs e) { VCAdministrator.HandleButtonRemovePrivate(); }
        void btnVCCheckPrivate_ItemClick(object sender, ItemClickEventArgs e) { RVAdministrator.ShowDialog(); }
        void btnAdaptLogFile_ItemClick(object sender, ItemClickEventArgs e) { PublicVersion.CleanLogFile(); }
        void btnVCAdministrateUsers_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonAdministrateUsers(); }
        void btnNewProject_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleMenuNewProject(this); }
        void btnVCStartMerging_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().StartMerge(); }
        void btnVCFinishMerging_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().FinishMerge(); }
        void btnVCAbortMerging_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().AbortMerge(); }
        void btnVCSettings_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonVCSettingsClicked(); }
        private void btnVCDownloadLatestBundle_ItemClick(object sender, ItemClickEventArgs e) { EM_AppContext.Instance.GetVCAdministrator().HandleButtonDownloadLatestRelease(); }
        void btnOpenOutputFile_ItemClick(object sender, ItemClickEventArgs e) { OpenOutputFile(); }
        private void btnVCMergeCountryOnlineBundle_ItemClick(object sender, ItemClickEventArgs e) { (new MergeAdministrator(this, false)).ShowChoices(true); }
        private void btnVCMergeVariablesOnlineBundle_ItemClick(object sender, ItemClickEventArgs e) { (new MergeAdministrator(this, true)).ShowChoices(true); }
        private void btnVCNewBundleLocalVersion_ItemClick(object sender, ItemClickEventArgs e){
            DialogResult result = MessageBox.Show("A new online bundle will be created directly from your local version. In order to create a new bundle it is recommend to follow the steps under 'New Online Bundle'. Are you sure you want to proceed?", "Warning", MessageBoxButtons.YesNo);
            if(result == DialogResult.Yes) { EM_AppContext.Instance.GetVCAdministrator().FinishMerge(); }
            else { return; }

        }

        // Extensions
        void btnAdminGroups_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new GroupAdminAction(_countryShortName), false); }
        void btnGroupAddTo_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_GROUP_ADD); }
        void btnGroupRemoveFrom_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_GROUP_REMOVE); }
        void btnGroupVisible_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_GROUP_VISIBLE); }
        void btnGroupNotVisible_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_GROUP_NOT_VISIBLE); }
        void btnGroupExpand_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_GROUP_EXPAND); }
        void btnAdminLExtensions_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new ExtensionAdminCtryAction(_countryShortName, this), false); }
        void btnAddToOnExtension_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_ADDON); }
        void btnAddToOffExtension_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_ADDOFF); }
        void btnRemoveFromExtension_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_REMOVE); }
        void btnSetVisibleExtension_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_VISIBLE); }
        void btnSetNotVisibleExtension_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_NOT_VISIBLE); }
        void btnExpandExtension_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_EXPAND); }
        void btnSetPrivateExtensionL_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_PRIVATE_COUNTRY); }
        void btnSetNotPrivateExtensionL_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_NOT_PRIVATE_COUNTRY); }
        void btnAdminGExtensions_ItemClick(object sender, ItemClickEventArgs e) { ExtensionAdminGlobal.DoAdmin(); }
        void btnSetPrivateExtensionG_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_PRIVATE_ALL); }
        void btnSetNotPrivateExtensionG_ItemClick(object sender, ItemClickEventArgs e) { ExtensionOrGroup_DrawMenu(ExtensionAndGroupMenuManager.MENU_EXTENSION_NOT_PRIVATE_ALL); }
        void btnSetExtensionsSwitches_ItemClick(object sender, ItemClickEventArgs e) { PerformAction(new ExtensionSetSwitchesAction(_countryShortName), false); }

        // called by buttons in the CountryTools/Extension-menu respectivly the Display/Groups-menu to draw the menu listing the relevant groups/extensions
        private void ExtensionOrGroup_DrawMenu(string selectedMenu)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            foreach (KeyValuePair<string, Image> ge in ExtensionAndGroupMenuManager.GetRelevantMenuItems(_countryShortName, selectedMenu))
                menu.Items.Add(ge.Key, ge.Value, ExtensionOrGroup_MenuItemClicked);
            menu.Show(MousePosition);
            void ExtensionOrGroup_MenuItemClicked(object sender, EventArgs e) { ExtensionAndGroupMenuManager.MenuItemClicked(this, sender.ToString()); }
        }

        void StartEMTool(string fullPathTool)
        {
            try
            {
                Process Excel = new Process();
                Excel.StartInfo.FileName = "Excel.exe";
                Excel.StartInfo.Arguments = fullPathTool;
                Excel.Start();
            }
            catch (Exception exception)
            {
                Tools.UserInfoHandler.ShowException(exception);
            }
        }

        //this informs the user if not all countries are visible in the gallery without scrolling
        void Gallery_CustomDrawItemImage(object sender, DevExpress.XtraBars.Ribbon.GalleryItemCustomDrawEventArgs e)
        {
            try
            {
                List<GalleryItem> allItems = galCountries.Gallery.GetAllItems();
                //as a default assume that all countries are visible without scrolling
                if (allItems[0].Caption != e.Item.Caption) //just do for first item, as function is called quite often
                {
                    txtInStatusBar.Caption = txtInStatusBar.Caption.Replace(_statusBarText_NotAllCountriesVisible, string.Empty);
                    this.btnImageInStatusBar.Glyph = null;
                }

                //check whether an additional item would find place in the same row
                //note that function would not be called for the next item if not, as it wouldn't be visible and therefore doesn't need drawing
                DevExpress.XtraBars.Ribbon.ViewInfo.GalleryItemViewInfo info = e.ItemInfo as DevExpress.XtraBars.Ribbon.ViewInfo.GalleryItemViewInfo;
                if (info.Bounds.Left + info.Bounds.Width * 2 > //times 2 to approximate the distance between items
                    info.GalleryInfo.GalleryContentBounds.Left + info.GalleryInfo.GalleryContentBounds.Width)
                {
                    if (allItems[allItems.Count - 1].Caption != e.Item.Caption) //this condition concerns the very last item: it would be fine (all items visible), if no further item finds place
                    {
                        txtInStatusBar.Caption = _statusBarText_NotAllCountriesVisible;
                        this.btnImageInStatusBar.Glyph = global::EM_UI.Properties.Resources.TipNotAllCountriesVisible;
                    }
                }
            }
            catch (Exception exception)
            {
                //do nothing: not important enough to risk an error
                UserInfoHandler.RecordIgnoredException("EM_UI_MainForm.Gallery_CustomDrawItemImage", exception);
            }
        }

        internal void ShowComponentUseForm()
        {
            if (_componentUseForm == null)
                _componentUseForm = new ComponentUseForm(this);
            if (!_componentUseForm.Visible) _componentUseForm.Show();
        }

        void OpenOutputFile()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";
            openFileDialog.CheckPathExists = true;
            openFileDialog.CheckFileExists = true;
            openFileDialog.AddExtension = true;
            openFileDialog.Multiselect = true;
            openFileDialog.InitialDirectory = EM_AppContext.FolderOutput;

            if (openFileDialog.ShowDialog() == DialogResult.Cancel || openFileDialog.FileNames.Count() == 0)
                return;

            string arguments = "/p " + "\"" + EM_AppContext.FolderOutput + "\""; //set Excels working directory to Euromod's output folder
            foreach (string fileName in openFileDialog.FileNames)
                arguments += " \"" + fileName + "\"";
            StartEMTool(arguments);
        }

        void btnSave_As_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            AddCountryForm saveAsDialog = new AddCountryForm(_isAddOn, _countryShortName) { Text = "Save As ..." };
            for (; ; )
            {
                if (saveAsDialog.ShowDialog() == DialogResult.Cancel) return;
                string newName = saveAsDialog.GetCountryShortName();
                if (File.Exists(EMPath.Folder_AddOns(EM_AppContext.FolderEuromodFiles) + newName + ".xml")
                    || Directory.Exists(EMPath.Folder_Countries(EM_AppContext.FolderEuromodFiles) + newName))
                    UserInfoHandler.ShowError("'" + newName + "' already exists.");
                else break;
            }

            SaveAsAdaptOptions saveAsAdaptOptions = saveAsDialog.GetSaveAsAdaptOptions();
            PerformAction(new SaveAsAction(_countryShortName, saveAsDialog.GetCountryShortName(), saveAsDialog.GetCountryLongName(),
                saveAsDialog.GetFlagPathAndFileName(), saveAsAdaptOptions), saveAsAdaptOptions != null, saveAsAdaptOptions != null);

            //release the in-use-file of the original country (not in use anymore)
            if (!_isReadOnly)
                InUseFileHandler.ReleaseFile(CountryAdministrator.GetCountryPath(_countryShortName), _countryShortName);
            
            _countryShortName = saveAsDialog.GetCountryShortName();
            _isReadOnly = false; _undoManager.Reset(); _hasChanges = 0;

            //capture parameter files of the now loaded country
            bool dummy = false;
            InUseFileHandler.CaptureFile(CountryAdministrator.GetCountryPath(_countryShortName), _countryShortName, ref dummy);

            VisualiseWhichCountryIsLoaded();
            _runMainForm = null; //to force updating of the run-dialog (otherwise new country would not be displayed)
            SetButtonGreyState();

            EM_AppContext.Instance.UpdateAllCountryMainFormGalleries(); //update galleries of all loaded countries            
        }

        void colorEditChooseNodeColor_HiddenEditor(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {//is called when the color-dialog to set a custom node color closes
            Color color = ((Color)(sender as BarEditItem).EditValue);
            PerformAction(new ChangeNodeColorAction(color.ToArgb(), _multiCellSelector), false);
            ClearCellSelection();
            treeList.Refresh();
        }

        void btnClearNodeColor_ItemClick(object sender, ItemClickEventArgs e)
        {
            List<string> nodeIDs = new List<string>();
            PerformAction(new ChangeNodeColorAction(DefPar.Value.NO_COLOR, _multiCellSelector), false);
            ClearCellSelection();
            treeList.Refresh();
        }

        void btnRemoveAllNodeColors_ItemClick(object sender, ItemClickEventArgs e)
        {
            PerformAction(new RemoveNodeColorsAction(_countryConfigFacade), false);
            ClearCellSelection();
            treeList.Refresh();
        }

        void btnAutomaticFormatting_ItemClick(object sender, ItemClickEventArgs e)
        {
            PerformAction(new AutomaticConditionalFormattingAction(_countryConfigFacade), false);
            ClearCellSelection();
            treeList.Refresh();
        }

        void btnSetBookmark_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (treeList.FocusedNode == null || treeList.FocusedNode.Tag == null)
                return;

            AddBookmarkForm addBookmarkForm = new AddBookmarkForm();
            if (addBookmarkForm.ShowDialog() == DialogResult.OK)
            {
                BookmarkAndColorManager.SaveNewBookmark(_countryShortName, (treeList.FocusedNode.Tag as BaseTreeListTag).GetDefaultID(), addBookmarkForm.txtBMName.Text);
                DrawBookmark(addBookmarkForm.txtBMName.Text, (treeList.FocusedNode.Tag as BaseTreeListTag).GetDefaultID(), treeList.FocusedNode);
            }
        }

        void btnShowMatrixViewOfIncomelists_ItemClick(object sender, ItemClickEventArgs e)
        {
            SelectSystemsForm selectSystemsForm = new SelectSystemsForm(_countryShortName);
            selectSystemsForm.SetSingleSelectionMode();
            if (selectSystemsForm.ShowDialog() == DialogResult.Cancel)
                return;
            CountryConfig.SystemRow systemRow = selectSystemsForm.GetSelectedSystemRows().First();
            SystemTreeListTag systemTreeListTag = _treeListBuilder.GetSystemColumnByID(systemRow.ID).Tag as SystemTreeListTag;
            EM_AppContext.Instance.GetMatrixViewOfIncomelistsForm().UpdateView(systemTreeListTag);
        }

        private void btnPolicyEffects_ItemClick(object sender, ItemClickEventArgs e)
        {
            PolicyEffects pc = new PolicyEffects(this);
            pc.Show(this);
        }

        private void btnShowHiddenSystems_ItemClick(object sender, ItemClickEventArgs e)
        {
            showHiddenSystemsBox();
        }

        internal void showHiddenSystemsBox()
        {
            if (treeList.CustomizationForm == null)
                treeList.ColumnsCustomization();
            treeList.CustomizationForm.Visible = true;
        }

        #endregion ribbon_events

        #region statusbar_and_bookmark_events

        void btnTextSizePlus_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { if (_treeListBuilder != null) _treeListBuilder.IncreaseTextSize(); }
        void btnTextSizeMinus_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) { if (_treeListBuilder != null) _treeListBuilder.DecreaseTextSize(); }

        internal void DrawBookmark(string bookmarkName, string referenceID, TreeListNode node)
        {
            foreach (object itemLink in EM_MainRibbon.Toolbar.ItemLinks)
            {
                DevExpress.XtraBars.BarButtonItemLink barButtonItemLink = itemLink as DevExpress.XtraBars.BarButtonItemLink;
                if (barButtonItemLink != null && barButtonItemLink.Item != null &&
                    barButtonItemLink.Item.Tag != null && barButtonItemLink.Item.Tag.ToString() == referenceID)
                    return; //to avoid double-drawing (e.g. when country is reloaded)
            }

            BarButtonItem btnBookMark = EM_MainRibbon.Items.CreateButton(""); //do not set a caption, because this is used for accessing the button with Alt-FirstChar and thus prevents Alt-S for spreading from working
            btnBookMark.Hint = bookmarkName; //if no caption, hint still shows the bookmark's name
            btnBookMark.Tag = referenceID;
            if (node.StateImageIndex >= 0)
                btnBookMark.Glyph = mainFormStateImageList.Images[node.StateImageIndex];
            else
                btnBookMark.Glyph = mainFormStateImageList.Images[node.StateImageIndex];
            btnBookMark.ItemClick += new ItemClickEventHandler(btnBookMark_ItemClick);
            EM_MainRibbon.Toolbar.ItemLinks.Add(btnBookMark);
        }

        void btnBookMark_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                _treeListManager.FocusSpecifiedNode((e.Item as BarButtonItem).Tag as string);
            }
            catch (Exception exception)
            {
                UserInfoHandler.ShowException(exception);
            }
        }

        void ItemLinks_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            if (e.Action == CollectionChangeAction.Remove && !isClosing)
            {
                BookmarkAndColorManager.DeleteBookmarks(_countryShortName);
                BookmarkAndColorManager.SaveBookmarks(_countryShortName, sender as RibbonQuickToolbarItemLinkCollection);
            }
        }

        #endregion statusbar_and_bookmark_events

        #region plugins

        internal void FillPlugInGallery()
        {
            // load and init plugins
            List<PiInterface> plugIns; string warnings;
            if (!PiLoader.GetPlugInList(out plugIns, out warnings))
                UserInfoHandler.ShowError("Loading plug-ins caused problems:" + Environment.NewLine + warnings);
            if (plugIns.Count > 0)  // If there are plugins available
            {
                // Create a new ribbon group
                RibbonPageGroup plugin_group = new RibbonPageGroup { Text = $"{DefGeneral.BRAND_TITLE} plugins",
                                                                     AllowTextClipping = false, ShowCaptionButton = false };
                foreach (PiInterface plugIn in plugIns)
                {
                    // Initialize the plugins and add the plugin buttons
                    BarLargeButtonItem btn = new BarLargeButtonItem(ribbonApplications.Ribbon.Manager, plugIn.GetTitle());
                    btn.Hint = plugIn.GetDescription();
                    btn.LargeGlyph = Icon.ExtractAssociatedIcon(plugIn.GetFullFileName()).ToBitmap();
                    plugin_group.ItemLinks.Add(btn);
                    try
                    {
                        btn.ItemClick += (o, k) => { plugIn.Run(); };
                    }
                    catch (Exception exception)
                    {
                        UserInfoHandler.ShowException(exception, plugIn.GetTitle(), false);
                    }
                }
                ribbonApplications.Groups.Add(plugin_group);
            }
        }

        #endregion plugins

    }  
        
 }
