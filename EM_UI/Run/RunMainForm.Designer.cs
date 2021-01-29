using System.Drawing;
namespace EM_UI.Run
{
    partial class RunMainForm
    {

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        internal void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            DevExpress.Utils.SuperToolTip superToolTip1 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipItem toolTipItem1 = new DevExpress.Utils.ToolTipItem();
            DevExpress.Utils.SuperToolTip superToolTip2 = new DevExpress.Utils.SuperToolTip();
            DevExpress.Utils.ToolTipItem toolTipItem2 = new DevExpress.Utils.ToolTipItem();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RunMainForm));
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.barAndDockingController = new DevExpress.XtraBars.BarAndDockingController(this.components);
            this.gbiCountries = new DevExpress.XtraBars.RibbonGalleryBarItem();
            this.btnRun = new DevExpress.XtraBars.BarButtonItem();
            this.chkShowSelectedHH = new DevExpress.XtraBars.BarEditItem();
            this.chkHideHiddenSystems = new DevExpress.XtraBars.BarEditItem();
            this.chkWarnAboutUselessGroups = new DevExpress.XtraBars.BarEditItem();
            this.checkEditSelectedHH = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.checkEditHideHiddenSystems = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.checkEditWarnAboutUselessGroups = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.btnSelectAll = new DevExpress.XtraBars.BarButtonItem();
            this.txtFilterDatasets = new DevExpress.XtraBars.BarEditItem();
            this.textEditFilterDatasets = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.txtFilterSystems = new DevExpress.XtraBars.BarEditItem();
            this.txtNumberOfHH = new DevExpress.XtraBars.BarEditItem();
            this.textEditNumberOfHH = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.chkDoNotStopOnNonCriticalErrors = new DevExpress.XtraBars.BarEditItem();
            this.checkEditDoNotStop = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.chkAddDateToOuputFilename = new DevExpress.XtraBars.BarEditItem();
            this.checkEditAddDate = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.chkLogRuntimeInDetail = new DevExpress.XtraBars.BarEditItem();
            this.checkEditLogRuntime = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();          
            this.chkRunEM2 = new DevExpress.XtraBars.BarEditItem();
            this.checkEditRunEM2 = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.chkCloseAfterRun = new DevExpress.XtraBars.BarEditItem();
            this.checkEditCloseAfter = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.cmbAddOns = new DevExpress.XtraBars.BarEditItem();
            this.comboBoxAddOns = new DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit();
            this.btnSelectNo = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectAllSystems = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectNoSystem = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectAllCountries = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectNoCountry = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectAllAddOns = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectNoAddOn = new DevExpress.XtraBars.BarButtonItem();
            this.chkBestMatchOnly = new DevExpress.XtraBars.BarEditItem();
            this.checkEditBestMatchOnly = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.chkRegularExpression = new DevExpress.XtraBars.BarEditItem();
            this.checkEditRegularExpression = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.btnRestorePolicySwitchDefaults = new DevExpress.XtraBars.BarButtonItem();
            this.chkDoNotPoolSystemsDatasets = new DevExpress.XtraBars.BarEditItem();
            this.checkEditDoNotPool = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.chkRunPublicOnly = new DevExpress.XtraBars.BarEditItem();
            this.checkEditPublicOnly = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.toggleAutoRename = new EM_UI.CustomControls.EM_CustomToggleBarEditItem();
            this.toggleSwitchAutoRename = new EM_UI.CustomControls.RepositoryItemEM_CustomToggle();
            this.lblAutoRename = new DevExpress.XtraBars.BarStaticItem();
            this.customParallelRunsItem = new EM_UI.CustomControls.EM_RadioValueBarEditItem();
            this.repositoryItemParallelRuns = new EM_UI.CustomControls.RepositoryItemEM_RadioValueEditor();
            this.chkOutputEuro = new DevExpress.XtraBars.BarEditItem();
            this.checkEditOutputEuro = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.cmbExRateDate = new DevExpress.XtraBars.BarEditItem();
            this.comboBoxExRateDate = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.cptExchangeRate = new DevExpress.XtraBars.BarHeaderItem();
            this.rpRunMain = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rpgCountries = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgSelect = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgRun = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpView = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rpgView = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgFilter = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgAddOns = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgExtensions = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.chk = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rpgSettings = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgNumberOfHH = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgOutputEuro = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.tipRunMainForm = new System.Windows.Forms.ToolTip(this.components);
            this.dgvRun = new System.Windows.Forms.DataGridView();
            this.colRun = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDataset = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colFirstHH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastHH = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ctmMultiPolicySwitch = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniAllSystemsOn = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllSystemsOff = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllSystemsDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllSystemsAll = new System.Windows.Forms.ToolStripMenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.txtOutputPath = new System.Windows.Forms.TextBox();
            this.btnSelectOutputPath = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSelectedHH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditHideHiddenSystems)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditWarnAboutUselessGroups)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditFilterDatasets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditNumberOfHH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDoNotStop)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditAddDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditLogRuntime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditRunEM2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditCloseAfter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAddOns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditBestMatchOnly)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditRegularExpression)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDoNotPool)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditPublicOnly)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.toggleSwitchAutoRename)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemParallelRuns)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditOutputEuro)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxExRateDate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRun)).BeginInit();
            this.ctmMultiPolicySwitch.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.AllowMinimizeRibbon = false;
            this.ribbon.ApplicationButtonText = null;
            this.ribbon.Controller = this.barAndDockingController;
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.gbiCountries,
            this.btnRun,
            this.chkShowSelectedHH,
            this.chkHideHiddenSystems,
            this.chkWarnAboutUselessGroups,
            this.btnSelectAll,
            this.txtFilterDatasets,
            this.txtFilterSystems,
            this.txtNumberOfHH,
            this.chkDoNotStopOnNonCriticalErrors,
            this.chkAddDateToOuputFilename,
            this.chkLogRuntimeInDetail,
            this.chkRunEM2,
            this.chkCloseAfterRun,
            this.cmbAddOns,
            this.btnSelectNo,
            this.btnSelectAllSystems,
            this.btnSelectNoSystem,
            this.btnSelectAllCountries,
            this.btnSelectNoCountry,
            this.btnSelectAllAddOns,
            this.btnSelectNoAddOn,
            this.chkBestMatchOnly,
            this.chkRegularExpression,
            this.btnRestorePolicySwitchDefaults,
            this.chkDoNotPoolSystemsDatasets,
            this.chkRunPublicOnly,
            this.toggleAutoRename,
            this.lblAutoRename,
            this.customParallelRunsItem,
            this.chkOutputEuro,
            this.cmbExRateDate,
            this.cptExchangeRate});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ribbon.MaxItemId = 123;
            this.ribbon.Name = "ribbon";
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.rpRunMain,
            this.rpView,
            this.chk});
            this.ribbon.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.checkEditSelectedHH,
            this.checkEditHideHiddenSystems,
            this.checkEditWarnAboutUselessGroups,
            this.textEditFilterDatasets,
            this.textEditNumberOfHH,
            this.checkEditDoNotStop,
            this.checkEditAddDate,
            this.checkEditLogRuntime,
            this.checkEditRunEM2,
            this.checkEditCloseAfter,
            this.comboBoxAddOns,
            this.checkEditBestMatchOnly,
            this.checkEditRegularExpression,
            this.checkEditDoNotPool,
            this.toggleSwitchAutoRename,
            this.checkEditPublicOnly,
            this.repositoryItemParallelRuns,
            this.checkEditOutputEuro,
            this.comboBoxExRateDate});
            this.ribbon.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2010;
            this.ribbon.ShowApplicationButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbon.ShowExpandCollapseButton = DevExpress.Utils.DefaultBoolean.False;
            this.ribbon.Size = new System.Drawing.Size(1896, 157);
            // 
            // barAndDockingController
            // 
            this.barAndDockingController.LookAndFeel.SkinName = "Seven Classic";
            this.barAndDockingController.LookAndFeel.UseDefaultLookAndFeel = false;
            this.barAndDockingController.PropertiesBar.AllowLinkLighting = false;
            this.barAndDockingController.PropertiesBar.DefaultGlyphSize = new System.Drawing.Size(16, 16);
            this.barAndDockingController.PropertiesBar.DefaultLargeGlyphSize = new System.Drawing.Size(32, 32);
            // 
            // gbiCountries
            // 
            this.gbiCountries.Caption = "ribbonGalleryBarItem1";
            // 
            // 
            // 
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Disabled.Options.UseFont = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Disabled.Options.UseTextOptions = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Disabled.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Hovered.Options.UseFont = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Hovered.Options.UseTextOptions = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Hovered.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Normal.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Normal.Options.UseFont = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Normal.Options.UseTextOptions = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Normal.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Pressed.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Pressed.Options.UseFont = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Pressed.Options.UseTextOptions = true;
            this.gbiCountries.Gallery.Appearance.ItemCaptionAppearance.Pressed.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.gbiCountries.Gallery.ColumnCount = 50;
            this.gbiCountries.Gallery.ShowItemText = true;
            this.gbiCountries.Gallery.UseMaxImageSize = true;
            this.gbiCountries.Gallery.ItemClick += new DevExpress.XtraBars.Ribbon.GalleryItemClickEventHandler(this.gbiCountries_Gallery_ItemClick);
            this.gbiCountries.Id = 58;
            this.gbiCountries.Name = "gbiCountries";
            // 
            // btnRun
            // 
            this.btnRun.Caption = "Run";
            this.btnRun.Glyph = global::EM_UI.Properties.Resources.green_right_arrow;
            this.btnRun.Id = 59;
            this.btnRun.LargeGlyph = global::EM_UI.Properties.Resources.green_right_arrow;
            this.btnRun.Name = "btnRun";
            this.btnRun.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnRun_ItemClick);
            // 
            // chkShowSelectedHH
            // 
            this.chkShowSelectedHH.Caption = "Show selected HH options";
            this.chkShowSelectedHH.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkShowSelectedHH.Edit = this.checkEditSelectedHH;
            this.chkShowSelectedHH.EditValue = false;
            this.chkShowSelectedHH.Id = 60;
            this.chkShowSelectedHH.Name = "chkShowSelectedHH";
            // 
            // checkEditSelectedHH
            // 
            this.checkEditSelectedHH.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditSelectedHH.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditSelectedHH.Appearance.Options.UseBackColor = true;
            this.checkEditSelectedHH.Appearance.Options.UseTextOptions = true;
            this.checkEditSelectedHH.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditSelectedHH.AutoHeight = false;
            this.checkEditSelectedHH.Name = "checkEditSelectedHH";
            this.checkEditSelectedHH.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.checkEditSelectedHH_EditValueChanging);
            // 
            // chkHideHiddenSystems
            // 
            this.chkHideHiddenSystems.Caption = "Do not show hidden systems";
            this.chkHideHiddenSystems.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkHideHiddenSystems.Edit = this.checkEditHideHiddenSystems;
            this.chkHideHiddenSystems.EditValue = false;
            this.chkHideHiddenSystems.Id = 601;
            this.chkHideHiddenSystems.Name = "chkHideHiddenSystems";
            // 
            // checkEditHideHiddenSystems
            // 
            this.checkEditHideHiddenSystems.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditHideHiddenSystems.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditHideHiddenSystems.Appearance.Options.UseBackColor = true;
            this.checkEditHideHiddenSystems.Appearance.Options.UseTextOptions = true;
            this.checkEditHideHiddenSystems.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditHideHiddenSystems.AutoHeight = false;
            this.checkEditHideHiddenSystems.Name = "checkEditHideHiddenSystems";
            this.checkEditHideHiddenSystems.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.checkEditHideHiddenSystems_EditValueChanging);
            // 
            // chkWarnAboutUselessGroups
            // 
            this.chkWarnAboutUselessGroups.Caption = "Warn about useless groups";
            this.chkWarnAboutUselessGroups.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkWarnAboutUselessGroups.Edit = this.checkEditWarnAboutUselessGroups;
            this.chkWarnAboutUselessGroups.EditValue = false;
            this.chkWarnAboutUselessGroups.Id = 6012;
            this.chkWarnAboutUselessGroups.Name = "chkWarnAboutUselessGroups";
            // 
            // checkEditWarnAboutUselessGroups
            // 
            this.checkEditWarnAboutUselessGroups.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditWarnAboutUselessGroups.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditWarnAboutUselessGroups.Appearance.Options.UseBackColor = true;
            this.checkEditWarnAboutUselessGroups.Appearance.Options.UseTextOptions = true;
            this.checkEditWarnAboutUselessGroups.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditWarnAboutUselessGroups.AutoHeight = false;
            this.checkEditWarnAboutUselessGroups.Name = "checkEditWarnAboutUselessGroups";
            this.checkEditWarnAboutUselessGroups.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.checkEditWarnAboutUselessGroups_EditValueChanging);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Caption = "Countries";
            this.btnSelectAll.Glyph = global::EM_UI.Properties.Resources.SelectAllCountries;
            this.btnSelectAll.Id = 63;
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectAllOrNo_ItemClick);
            // 
            // txtFilterDatasets
            // 
            this.txtFilterDatasets.Caption = "Filter Datasets";
            this.txtFilterDatasets.Edit = this.textEditFilterDatasets;
            this.txtFilterDatasets.Id = 69;
            this.txtFilterDatasets.Name = "txtFilterDatasets";
            this.txtFilterDatasets.Width = 80;
            // 
            // textEditFilterDatasets
            // 
            this.textEditFilterDatasets.AutoHeight = false;
            this.textEditFilterDatasets.Name = "textEditFilterDatasets";
            this.textEditFilterDatasets.Leave += new System.EventHandler(this.textEditFilterDatasets_Leave);
            // 
            // txtFilterSystems
            // 
            this.txtFilterSystems.Caption = "Filter Systems ";
            this.txtFilterSystems.Edit = this.textEditFilterDatasets;
            this.txtFilterSystems.Id = 71;
            this.txtFilterSystems.Name = "txtFilterSystems";
            this.txtFilterSystems.Width = 80;
            // 
            // txtNumberOfHH
            // 
            this.txtNumberOfHH.Caption = "Run first N Households only  ";
            this.txtNumberOfHH.Edit = this.textEditNumberOfHH;
            this.txtNumberOfHH.Id = 70;
            this.txtNumberOfHH.Name = "txtNumberOfHH";
            // 
            // textEditNumberOfHH
            // 
            this.textEditNumberOfHH.AutoHeight = false;
            this.textEditNumberOfHH.Name = "textEditNumberOfHH";
            // 
            // chkDoNotStopOnNonCriticalErrors
            // 
            this.chkDoNotStopOnNonCriticalErrors.Caption = "Do not stop on non-critical errors";
            this.chkDoNotStopOnNonCriticalErrors.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkDoNotStopOnNonCriticalErrors.Edit = this.checkEditDoNotStop;
            this.chkDoNotStopOnNonCriticalErrors.EditValue = true;
            this.chkDoNotStopOnNonCriticalErrors.Id = 72;
            this.chkDoNotStopOnNonCriticalErrors.Name = "chkDoNotStopOnNonCriticalErrors";
            // 
            // checkEditDoNotStop
            // 
            this.checkEditDoNotStop.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditDoNotStop.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditDoNotStop.Appearance.Options.UseBackColor = true;
            this.checkEditDoNotStop.Appearance.Options.UseTextOptions = true;
            this.checkEditDoNotStop.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditDoNotStop.AutoHeight = false;
            this.checkEditDoNotStop.Name = "checkEditDoNotStop";
            this.checkEditDoNotStop.EditValueChanged += new System.EventHandler(this.checkEditDoNotStop_EditValueChanged);
            // 
            // chkAddDateToOuputFilename
            // 
            this.chkAddDateToOuputFilename.Caption = "Add date to output-filename";
            this.chkAddDateToOuputFilename.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkAddDateToOuputFilename.Edit = this.checkEditAddDate;
            this.chkAddDateToOuputFilename.EditValue = false;
            this.chkAddDateToOuputFilename.Id = 73;
            this.chkAddDateToOuputFilename.Name = "chkAddDateToOuputFilename";
            // 
            // checkEditAddDate
            // 
            this.checkEditAddDate.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditAddDate.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditAddDate.Appearance.Options.UseBackColor = true;
            this.checkEditAddDate.Appearance.Options.UseTextOptions = true;
            this.checkEditAddDate.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditAddDate.AutoHeight = false;
            this.checkEditAddDate.Name = "checkEditAddDate";
            this.checkEditAddDate.EditValueChanged += new System.EventHandler(this.checkEditAddDate_EditValueChanged);
            // 
            // chkLogRuntimeInDetail
            // 
            this.chkLogRuntimeInDetail.Caption = "Log runtime in detail";
            this.chkLogRuntimeInDetail.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkLogRuntimeInDetail.Edit = this.checkEditLogRuntime;
            this.chkLogRuntimeInDetail.EditValue = false;
            this.chkLogRuntimeInDetail.Id = 74;
            this.chkLogRuntimeInDetail.Name = "chkLogRuntimeInDetail";
            // 
            // checkEditLogRuntime
            // 
            this.checkEditLogRuntime.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditLogRuntime.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditLogRuntime.Appearance.Options.UseBackColor = true;
            this.checkEditLogRuntime.Appearance.Options.UseTextOptions = true;
            this.checkEditLogRuntime.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditLogRuntime.AutoHeight = false;
            this.checkEditLogRuntime.Name = "checkEditLogRuntime";
            this.checkEditLogRuntime.EditValueChanged += new System.EventHandler(this.checkEditLogRuntime_EditValueChanged);
            // 
            // chkRunEM2
            // 
            this.chkRunEM2.Caption = "Run Old Executable";
            this.chkRunEM2.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkRunEM2.Edit = this.checkEditRunEM2;
            this.chkRunEM2.EditValue = false;
            this.chkRunEM2.Id = 741;
            this.chkRunEM2.Name = "chkRunEM2";
            // 
            // checkEditRunEM2
            // 
            this.checkEditRunEM2.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditRunEM2.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditRunEM2.Appearance.Options.UseBackColor = true;
            this.checkEditRunEM2.Appearance.Options.UseTextOptions = true;
            this.checkEditRunEM2.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditRunEM2.AutoHeight = false;
            this.checkEditRunEM2.Name = "checkEditRunEM2";
            this.checkEditRunEM2.EditValueChanged += new System.EventHandler(this.checkEditRunEM2_EditValueChanged);
            // 
            // chkCloseAfterRun
            // 
            this.chkCloseAfterRun.Caption = "Close dialog after run";
            this.chkCloseAfterRun.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkCloseAfterRun.Edit = this.checkEditCloseAfter;
            this.chkCloseAfterRun.EditValue = true;
            this.chkCloseAfterRun.Id = 75;
            this.chkCloseAfterRun.Name = "chkCloseAfterRun";
            // 
            // checkEditCloseAfter
            // 
            this.checkEditCloseAfter.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditCloseAfter.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditCloseAfter.Appearance.Options.UseBackColor = true;
            this.checkEditCloseAfter.Appearance.Options.UseTextOptions = true;
            this.checkEditCloseAfter.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditCloseAfter.AutoHeight = false;
            this.checkEditCloseAfter.Name = "checkEditCloseAfter";
            this.checkEditCloseAfter.EditValueChanged += new System.EventHandler(this.checkEditCloseAfter_EditValueChanged);
            // 
            // cmbAddOns
            // 
            this.cmbAddOns.Edit = this.comboBoxAddOns;
            this.cmbAddOns.Id = 78;
            this.cmbAddOns.Name = "cmbAddOns";
            this.cmbAddOns.Width = 120;
            // 
            // comboBoxAddOns
            // 
            this.comboBoxAddOns.AllowMultiSelect = true;
            this.comboBoxAddOns.AutoHeight = false;
            this.comboBoxAddOns.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxAddOns.Name = "comboBoxAddOns";
            this.comboBoxAddOns.Closed += new DevExpress.XtraEditors.Controls.ClosedEventHandler(this.comboBoxAddOns_Closed);
            // 
            // btnSelectNo
            // 
            this.btnSelectNo.Caption = "Countries";
            this.btnSelectNo.Glyph = global::EM_UI.Properties.Resources.SelectNoCountry;
            this.btnSelectNo.Id = 79;
            this.btnSelectNo.Name = "btnSelectNo";
            this.btnSelectNo.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectAllOrNo_ItemClick);
            // 
            // btnSelectAllSystems
            // 
            this.btnSelectAllSystems.Caption = "Systems";
            this.btnSelectAllSystems.Glyph = global::EM_UI.Properties.Resources.SelectAllSystems;
            this.btnSelectAllSystems.Id = 80;
            this.btnSelectAllSystems.Name = "btnSelectAllSystems";
            this.btnSelectAllSystems.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectAllSystems_ItemClick);
            // 
            // btnSelectNoSystem
            // 
            this.btnSelectNoSystem.Caption = "Systems";
            this.btnSelectNoSystem.Glyph = global::EM_UI.Properties.Resources.SelectNoCountry;
            this.btnSelectNoSystem.Id = 81;
            this.btnSelectNoSystem.Name = "btnSelectNoSystem";
            this.btnSelectNoSystem.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectNoSystem_ItemClick);
            // 
            // btnSelectAllCountries
            // 
            this.btnSelectAllCountries.Caption = "Select all countries";
            this.btnSelectAllCountries.Glyph = global::EM_UI.Properties.Resources.Help;
            this.btnSelectAllCountries.Id = 83;
            this.btnSelectAllCountries.Name = "btnSelectAllCountries";
            this.btnSelectAllCountries.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonItemStyles.SmallWithText;
            // 
            // btnSelectNoCountry
            // 
            this.btnSelectNoCountry.Caption = "Select no country";
            this.btnSelectNoCountry.Glyph = global::EM_UI.Properties.Resources.Help;
            this.btnSelectNoCountry.Id = 84;
            this.btnSelectNoCountry.Name = "btnSelectNoCountry";
            // 
            // btnSelectAllAddOns
            // 
            this.btnSelectAllAddOns.Caption = "Add-ons";
            this.btnSelectAllAddOns.Glyph = global::EM_UI.Properties.Resources.SelectAllAddOns;
            this.btnSelectAllAddOns.Id = 89;
            this.btnSelectAllAddOns.Name = "btnSelectAllAddOns";
            this.btnSelectAllAddOns.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectAllAddOns_ItemClick);
            // 
            // btnSelectNoAddOn
            // 
            this.btnSelectNoAddOn.Caption = "Add-ons";
            this.btnSelectNoAddOn.Glyph = global::EM_UI.Properties.Resources.SelectNoCountry;
            this.btnSelectNoAddOn.Id = 90;
            this.btnSelectNoAddOn.Name = "btnSelectNoAddOn";
            this.btnSelectNoAddOn.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectNoAddOn_ItemClick);
            // 
            // chkBestMatchOnly
            // 
            this.chkBestMatchOnly.Caption = "Best Match Only";
            this.chkBestMatchOnly.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkBestMatchOnly.Edit = this.checkEditBestMatchOnly;
            this.chkBestMatchOnly.EditValue = false;
            this.chkBestMatchOnly.Id = 91;
            this.chkBestMatchOnly.Name = "chkBestMatchOnly";
            // 
            // checkEditBestMatchOnly
            // 
            this.checkEditBestMatchOnly.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.Appearance.Options.UseBackColor = true;
            this.checkEditBestMatchOnly.AppearanceDisabled.BackColor = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.AppearanceDisabled.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.AppearanceDisabled.Options.UseBackColor = true;
            this.checkEditBestMatchOnly.AppearanceFocused.BackColor = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.AppearanceFocused.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.AppearanceFocused.Options.UseBackColor = true;
            this.checkEditBestMatchOnly.AppearanceReadOnly.BackColor = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.AppearanceReadOnly.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditBestMatchOnly.AppearanceReadOnly.Options.UseBackColor = true;
            this.checkEditBestMatchOnly.AutoHeight = false;
            this.checkEditBestMatchOnly.Name = "checkEditBestMatchOnly";
            this.checkEditBestMatchOnly.CheckedChanged += new System.EventHandler(this.checkEditBestMatchOnly_CheckedChanged);
            // 
            // chkRegularExpression
            // 
            this.chkRegularExpression.Caption = "Regular Expression";
            this.chkRegularExpression.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkRegularExpression.Edit = this.checkEditRegularExpression;
            this.chkRegularExpression.EditValue = false;
            this.chkRegularExpression.Id = 92;
            this.chkRegularExpression.Name = "chkRegularExpression";
            // 
            // checkEditRegularExpression
            // 
            this.checkEditRegularExpression.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.Appearance.Options.UseBackColor = true;
            this.checkEditRegularExpression.AppearanceDisabled.BackColor = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.AppearanceDisabled.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.AppearanceDisabled.Options.UseBackColor = true;
            this.checkEditRegularExpression.AppearanceFocused.BackColor = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.AppearanceFocused.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.AppearanceFocused.Options.UseBackColor = true;
            this.checkEditRegularExpression.AppearanceReadOnly.BackColor = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.AppearanceReadOnly.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditRegularExpression.AppearanceReadOnly.Options.UseBackColor = true;
            this.checkEditRegularExpression.AutoHeight = false;
            this.checkEditRegularExpression.Name = "checkEditRegularExpression";
            this.checkEditRegularExpression.CheckedChanged += new System.EventHandler(this.checkEditRegularExpression_CheckedChanged);
            // 
            // btnRestorePolicySwitchDefaults
            // 
            this.btnRestorePolicySwitchDefaults.Caption = "Restore Defaults";
            this.btnRestorePolicySwitchDefaults.Glyph = global::EM_UI.Properties.Resources.CleanVariables;
            this.btnRestorePolicySwitchDefaults.Id = 93;
            this.btnRestorePolicySwitchDefaults.LargeGlyph = global::EM_UI.Properties.Resources.Switch;
            this.btnRestorePolicySwitchDefaults.Name = "btnRestorePolicySwitchDefaults";
            this.btnRestorePolicySwitchDefaults.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnRestorePolicySwitchDefaults_ItemClick);
            // 
            // chkDoNotPoolSystemsDatasets
            // 
            this.chkDoNotPoolSystemsDatasets.Caption = "Do not pool system\'s datasets";
            this.chkDoNotPoolSystemsDatasets.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkDoNotPoolSystemsDatasets.Edit = this.checkEditDoNotPool;
            this.chkDoNotPoolSystemsDatasets.EditValue = false;
            this.chkDoNotPoolSystemsDatasets.Id = 95;
            this.chkDoNotPoolSystemsDatasets.Name = "chkDoNotPoolSystemsDatasets";
            // 
            // checkEditDoNotPool
            // 
            this.checkEditDoNotPool.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditDoNotPool.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditDoNotPool.Appearance.Options.UseBackColor = true;
            this.checkEditDoNotPool.Appearance.Options.UseTextOptions = true;
            this.checkEditDoNotPool.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditDoNotPool.AutoHeight = false;
            this.checkEditDoNotPool.Name = "checkEditDoNotPool";
            this.checkEditDoNotPool.CheckedChanged += new System.EventHandler(this.checkEditDoNotPool_CheckedChanged);
            // 
            // chkRunPublicOnly
            // 
            this.chkRunPublicOnly.Caption = "Run public components only";
            this.chkRunPublicOnly.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkRunPublicOnly.Edit = this.checkEditPublicOnly;
            this.chkRunPublicOnly.EditValue = false;
            this.chkRunPublicOnly.Id = 96;
            this.chkRunPublicOnly.Name = "chkRunPublicOnly";
            // 
            // checkEditPublicOnly
            // 
            this.checkEditPublicOnly.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditPublicOnly.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditPublicOnly.Appearance.Options.UseBackColor = true;
            this.checkEditPublicOnly.Appearance.Options.UseTextOptions = true;
            this.checkEditPublicOnly.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditPublicOnly.AutoHeight = false;
            this.checkEditPublicOnly.Name = "checkEditPublicOnly";
            this.checkEditPublicOnly.EditValueChanged += new System.EventHandler(this.checkEditPublicOnly_EditValueChanged);
            // 
            // toggleAutoRename
            // 
            this.toggleAutoRename.Edit = this.toggleSwitchAutoRename;
            this.toggleAutoRename.EditHeight = 24;
            this.toggleAutoRename.EditValue = false;
            this.toggleAutoRename.Hint = "If any of the extensions have non-default values, automatically rename the output f" +
    "ile to include these values.";
            this.toggleAutoRename.Id = 114;
            this.toggleAutoRename.Name = "toggleAutoRename";
            this.toggleAutoRename.PaintStyle = DevExpress.XtraBars.BarItemPaintStyle.Caption;
            toolTipItem1.Text = "If any of the extensions have non-default values, automatically rename the output f" +
    "ile to include these values.";
            superToolTip1.Items.Add(toolTipItem1);
            this.toggleAutoRename.SuperTip = superToolTip1;
            this.toggleAutoRename.Width = 77;
            this.toggleAutoRename.EditValueChanged += new System.EventHandler(this.toggleAutoRename_EditValueChanged);
            // 
            // toggleSwitchAutoRename
            // 
            this.toggleSwitchAutoRename.Name = "toggleSwitchAutoRename";
            this.toggleSwitchAutoRename.OffText = "Off";
            this.toggleSwitchAutoRename.OnText = "On";
            // 
            // lblAutoRename
            // 
            this.lblAutoRename.AutoSize = DevExpress.XtraBars.BarStaticItemSize.None;
            this.lblAutoRename.Caption = "Auto Rename";
            this.lblAutoRename.Id = 115;
            this.lblAutoRename.Name = "lblAutoRename";
            toolTipItem2.Text = "If any of the extensions have non-default values, automatically rename the output f" +
    "ile to include these values.";
            superToolTip2.Items.Add(toolTipItem2);
            this.lblAutoRename.SuperTip = superToolTip2;
            this.lblAutoRename.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // customParallelRunsItem
            // 
            this.customParallelRunsItem.Edit = this.repositoryItemParallelRuns;
            this.customParallelRunsItem.EditHeight = 100;
            this.customParallelRunsItem.EditValue = "D1";
            this.customParallelRunsItem.Id = 116;
            this.customParallelRunsItem.Name = "customParallelRunsItem";
            this.customParallelRunsItem.Width = 200;
            this.customParallelRunsItem.EditValueChanged += new System.EventHandler(this.customParallelRunsItem_EditValueChanged);
            // 
            // repositoryItemParallelRuns
            // 
            this.repositoryItemParallelRuns.Name = "repositoryItemParallelRuns";
            this.repositoryItemParallelRuns.ControlType = typeof(CustomControls.EM_RadioValueControl);
            
            // 
            // chkOutputEuro
            // 
            this.chkOutputEuro.Caption = "All Output in €";
            this.chkOutputEuro.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkOutputEuro.Edit = this.checkEditOutputEuro;
            this.chkOutputEuro.EditValue = false;
            this.chkOutputEuro.Id = 118;
            this.chkOutputEuro.Name = "chkOutputEuro";
            this.chkOutputEuro.Width = 20;
            // 
            // checkEditOutputEuro
            // 
            this.checkEditOutputEuro.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.checkEditOutputEuro.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.checkEditOutputEuro.Appearance.Options.UseBackColor = true;
            this.checkEditOutputEuro.Appearance.Options.UseTextOptions = true;
            this.checkEditOutputEuro.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.checkEditOutputEuro.AutoHeight = false;
            this.checkEditOutputEuro.Name = "checkEditOutputEuro";
            // 
            // cmbExRateDate
            // 
            this.cmbExRateDate.Edit = this.comboBoxExRateDate;
            this.cmbExRateDate.EditValue = "Default";
            this.cmbExRateDate.Id = 121;
            this.cmbExRateDate.Name = "cmbExRateDate";
            this.cmbExRateDate.Width = 110;
            // 
            // comboBoxExRateDate
            // 
            this.comboBoxExRateDate.AutoHeight = false;
            this.comboBoxExRateDate.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.comboBoxExRateDate.Items.AddRange(new object[] {
            "Default",
            "June 30",
            "Year Average"/*,
            "First Semester",
            "Second Semester"*/});
            this.comboBoxExRateDate.Name = "comboBoxExRateDate";
            // 
            // cptExchangeRate
            // 
            this.cptExchangeRate.Caption = "Exchange Rate";
            this.cptExchangeRate.Id = 122;
            this.cptExchangeRate.Name = "cptExchangeRate";
            // 
            // rpRunMain
            // 
            this.rpRunMain.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.rpgCountries,
            this.rpgSelect,
            this.rpgRun});
            this.rpRunMain.Name = "rpRunMain";
            this.rpRunMain.Text = "Main";
            // 
            // rpgCountries
            // 
            this.rpgCountries.AllowMinimize = false;
            this.rpgCountries.ItemLinks.Add(this.gbiCountries);
            this.rpgCountries.Name = "rpgCountries";
            this.rpgCountries.ShowCaptionButton = false;
            this.rpgCountries.Text = "Select countries";
            // 
            // rpgSelect
            // 
            this.rpgSelect.ItemLinks.Add(this.btnSelectAll);
            this.rpgSelect.ItemLinks.Add(this.btnSelectAllSystems);
            this.rpgSelect.ItemLinks.Add(this.btnSelectAllAddOns);
            this.rpgSelect.ItemLinks.Add(this.btnSelectNo);
            this.rpgSelect.ItemLinks.Add(this.btnSelectNoSystem);
            this.rpgSelect.ItemLinks.Add(this.btnSelectNoAddOn);
            this.rpgSelect.Name = "rpgSelect";
            this.rpgSelect.ShowCaptionButton = false;
            this.rpgSelect.Text = "Select all ... / Unselect all ...";
            // 
            // rpgRun
            // 
            this.rpgRun.AllowMinimize = false;
            this.rpgRun.ItemLinks.Add(this.btnRun);
            this.rpgRun.Name = "rpgRun";
            this.rpgRun.ShowCaptionButton = false;
            // 
            // rpView
            // 
            this.rpView.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.rpgView,
            this.rpgFilter,
            this.rpgAddOns,
            this.rpgExtensions});
            this.rpView.Name = "rpView";
            this.rpView.Text = "View / Filter / Add-Ons";
            // 
            // rpgView
            // 
            this.rpgView.AllowMinimize = false;
            this.rpgView.ItemLinks.Add(this.chkShowSelectedHH);
            this.rpgView.Name = "rpgView";
            this.rpgView.ShowCaptionButton = false;
            this.rpgView.Text = "View / Select";
            // 
            // rpgFilter
            // 
            this.rpgFilter.AllowMinimize = false;
            this.rpgFilter.ItemLinks.Add(this.txtFilterDatasets);
            this.rpgFilter.ItemLinks.Add(this.txtFilterSystems);
            this.rpgFilter.ItemLinks.Add(this.chkBestMatchOnly, true);
            this.rpgFilter.ItemLinks.Add(this.chkRegularExpression);
            this.rpgFilter.Name = "rpgFilter";
            this.rpgFilter.ShowCaptionButton = false;
            this.rpgFilter.Text = "Filter";
            // 
            // rpgAddOns
            // 
            this.rpgAddOns.ItemLinks.Add(this.cmbAddOns);
            this.rpgAddOns.Name = "rpgAddOns";
            this.rpgAddOns.ShowCaptionButton = false;
            this.rpgAddOns.Text = "Add-Ons";
            // 
            // rpgExtensions
            // 
            this.rpgExtensions.ItemLinks.Add(this.toggleAutoRename);
            this.rpgExtensions.ItemLinks.Add(this.lblAutoRename);
            this.rpgExtensions.ItemLinks.Add(this.btnRestorePolicySwitchDefaults);
            this.rpgExtensions.Name = "rpgExtensions";
            this.rpgExtensions.ShowCaptionButton = false;
            this.rpgExtensions.Text = "Extensions";
            // 
            // chk
            // 
            this.chk.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.rpgSettings,
            this.rpgNumberOfHH,
            this.rpgOutputEuro});
            this.chk.Name = "chk";
            this.chk.Text = "Advanced Settings";
            // 
            // rpgSettings
            // 
            this.rpgSettings.AllowMinimize = false;
            this.rpgSettings.ItemLinks.Add(this.chkDoNotStopOnNonCriticalErrors);
            this.rpgSettings.ItemLinks.Add(this.chkAddDateToOuputFilename);
            this.rpgSettings.ItemLinks.Add(this.chkLogRuntimeInDetail);
            this.rpgSettings.ItemLinks.Add(this.chkCloseAfterRun);
            this.rpgSettings.ItemLinks.Add(this.chkDoNotPoolSystemsDatasets);
            this.rpgSettings.ItemLinks.Add(this.chkRunPublicOnly);
            this.rpgSettings.ItemLinks.Add(this.chkHideHiddenSystems);
            this.rpgSettings.ItemLinks.Add(this.chkWarnAboutUselessGroups);
            this.rpgSettings.ItemLinks.Add(this.customParallelRunsItem);
            this.rpgSettings.Name = "rpgSettings";
            this.rpgSettings.ShowCaptionButton = false;
            // 
            // rpgNumberOfHH
            // 
            this.rpgNumberOfHH.AllowMinimize = false;
            this.rpgNumberOfHH.ItemLinks.Add(this.txtNumberOfHH);
            this.rpgNumberOfHH.ItemLinks.Add(this.chkRunEM2);
            this.rpgNumberOfHH.Name = "rpgNumberOfHH";
            this.rpgNumberOfHH.ShowCaptionButton = false;
            // 
            // rpgOutputEuro
            // 
            this.rpgOutputEuro.ItemLinks.Add(this.chkOutputEuro);
            this.rpgOutputEuro.ItemLinks.Add(this.cptExchangeRate);
            this.rpgOutputEuro.ItemLinks.Add(this.cmbExRateDate);
            this.rpgOutputEuro.Name = "rpgOutputEuro";
            this.rpgOutputEuro.ShowCaptionButton = false;
            this.rpgOutputEuro.Visible = true;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // dgvRun
            // 
            this.dgvRun.AllowUserToAddRows = false;
            this.dgvRun.AllowUserToDeleteRows = false;
            this.dgvRun.AllowUserToResizeRows = false;
            this.dgvRun.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRun.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvRun.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRun.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colRun,
            this.colCountry,
            this.colSystem,
            this.colDataset,
            this.colFirstHH,
            this.colLastHH});
            this.dgvRun.ContextMenuStrip = this.ctmMultiPolicySwitch;
            this.dgvRun.Location = new System.Drawing.Point(14, 186);
            this.dgvRun.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.dgvRun.Name = "dgvRun";
            this.dgvRun.RowHeadersVisible = false;
            this.dgvRun.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvRun.Size = new System.Drawing.Size(2214, 1089);
            this.dgvRun.TabIndex = 2;
            this.dgvRun.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRun_CellContentClick);
            this.dgvRun.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRun_CellEndEdit);
            this.dgvRun.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvRun_CellFormatting);
            this.dgvRun.MouseDown += EM_UI.Dialogs.SingleClickForDataGridCombo.HandleDataGridViewMouseDown;
            // 
            // colRun
            // 
            this.colRun.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colRun.Frozen = true;
            this.colRun.HeaderText = "Run";
            this.colRun.Name = "colRun";
            this.colRun.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colRun.Width = 62;
            // 
            // colCountry
            // 
            this.colCountry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colCountry.Frozen = true;
            this.colCountry.HeaderText = "Country";
            this.colCountry.Name = "colCountry";
            this.colCountry.ReadOnly = true;
            this.colCountry.Width = 88;
            // 
            // colSystem
            // 
            this.colSystem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colSystem.Frozen = true;
            this.colSystem.HeaderText = "System";
            this.colSystem.Name = "colSystem";
            this.colSystem.ReadOnly = true;
            this.colSystem.Width = 83;
            // 
            // colDataset
            // 
            this.colDataset.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDataset.Frozen = true;
            this.colDataset.HeaderText = "Dataset";
            this.colDataset.Name = "colDataset";
            this.colDataset.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colDataset.Width = 84;
            // 
            // colFirstHH
            // 
            this.colFirstHH.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colFirstHH.HeaderText = "First HH-ID";
            this.colFirstHH.Name = "colFirstHH";
            this.colFirstHH.Visible = false;
            // 
            // colLastHH
            // 
            this.colLastHH.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colLastHH.HeaderText = "Last HH-ID";
            this.colLastHH.Name = "colLastHH";
            this.colLastHH.Visible = false;
            // 
            // ctmMultiPolicySwitch
            // 
            this.ctmMultiPolicySwitch.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.ctmMultiPolicySwitch.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniAllSystemsOn,
            this.mniAllSystemsOff,
            this.mniAllSystemsDefault,
            this.mniAllSystemsAll});
            this.ctmMultiPolicySwitch.Name = "ctmMultiPolicySwitch";
            this.ctmMultiPolicySwitch.Size = new System.Drawing.Size(275, 82);
            this.ctmMultiPolicySwitch.Opening += new System.ComponentModel.CancelEventHandler(this.ctmMultiPolicySwitch_Opening);
            // 
            // mniAllSystemsOn
            // 
            this.mniAllSystemsOn.Name = "mniAllSystemsOn";
            this.mniAllSystemsOn.Size = new System.Drawing.Size(274, 26);
            this.mniAllSystemsOn.Text = "Set to ON for all systems";
            this.mniAllSystemsOn.Click += new System.EventHandler(this.mniAllSystemsOn_Click);
            // 
            // mniAllSystemsOff
            // 
            this.mniAllSystemsOff.Name = "mniAllSystemsOff";
            this.mniAllSystemsOff.Size = new System.Drawing.Size(274, 26);
            this.mniAllSystemsOff.Text = "Set to OFF for all systems";
            this.mniAllSystemsOff.Click += new System.EventHandler(this.mniAllSystemsOff_Click);
            // 
            // mniAllSystemsDefault
            // 
            this.mniAllSystemsDefault.Name = "mniAllSystemsDefault";
            this.mniAllSystemsDefault.Size = new System.Drawing.Size(274, 26);
            this.mniAllSystemsDefault.Text = "Set to Default for all systems";
            this.mniAllSystemsDefault.Click += new System.EventHandler(this.mniAllSystemsDefault_Click);
            // 
            // mniAllSystemsAll
            // 
            this.mniAllSystemsAll.Name = "mniAllSystemsAll";
            this.mniAllSystemsAll.Size = new System.Drawing.Size(274, 26);
            this.mniAllSystemsAll.Text = "Set to ALL for all systems";
            this.mniAllSystemsAll.Click += new System.EventHandler(this.mniAllSystemsAll_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 1298);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 17);
            this.label1.TabIndex = 4;
            this.label1.Text = "Output path";
            // 
            // txtOutputPath
            // 
            this.txtOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOutputPath.Location = new System.Drawing.Point(100, 1295);
            this.txtOutputPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtOutputPath.Name = "txtOutputPath";
            this.txtOutputPath.Size = new System.Drawing.Size(2065, 23);
            this.txtOutputPath.TabIndex = 5;
            // 
            // btnSelectOutputPath
            // 
            this.btnSelectOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectOutputPath.Image = global::EM_UI.Properties.Resources.Folder;
            this.btnSelectOutputPath.Location = new System.Drawing.Point(2183, 1282);
            this.btnSelectOutputPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectOutputPath.Name = "btnSelectOutputPath";
            this.btnSelectOutputPath.Size = new System.Drawing.Size(45, 48);
            this.btnSelectOutputPath.TabIndex = 7;
            this.btnSelectOutputPath.UseVisualStyleBackColor = true;
            this.btnSelectOutputPath.Click += new System.EventHandler(this.btnSelectOutputPath_Click);
            // 
            // RunMainForm
            // 
            this.Appearance.ForeColor = System.Drawing.Color.Black;
            this.Appearance.Options.UseForeColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1896, 1079);
            this.Controls.Add(this.btnSelectOutputPath);
            this.Controls.Add(this.txtOutputPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvRun);
            this.Controls.Add(this.ribbon);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_RunDialog.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "RunMainForm";
            this.Ribbon = this.ribbon;
            this.helpProvider.SetShowHelp(this, true);
            this.Text = "Run " + EM_Common.DefGeneral.BRAND_TITLE;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RunMainForm_FormClosing);
            this.Load += new System.EventHandler(this.RunMainForm_Load);
            this.Shown += new System.EventHandler(this.RunMainForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditSelectedHH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditHideHiddenSystems)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditWarnAboutUselessGroups)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditFilterDatasets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.textEditNumberOfHH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDoNotStop)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditAddDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditLogRuntime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditRunEM2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditCloseAfter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxAddOns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditBestMatchOnly)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditRegularExpression)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditDoNotPool)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditPublicOnly)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.toggleSwitchAutoRename)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemParallelRuns)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkEditOutputEuro)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.comboBoxExRateDate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRun)).EndInit();
            this.ctmMultiPolicySwitch.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        internal DevExpress.XtraBars.Ribbon.RibbonPage rpRunMain;
        internal System.Windows.Forms.HelpProvider helpProvider;
        internal DevExpress.XtraBars.BarAndDockingController barAndDockingController;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ToolTip tipRunMainForm;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgCountries;
        private DevExpress.XtraBars.RibbonGalleryBarItem gbiCountries;
        internal System.Windows.Forms.DataGridView dgvRun;
        private DevExpress.XtraBars.BarButtonItem btnRun;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgRun;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtOutputPath;
        private DevExpress.XtraBars.BarEditItem chkShowSelectedHH;
        private DevExpress.XtraBars.BarEditItem chkHideHiddenSystems;
        private DevExpress.XtraBars.BarEditItem chkWarnAboutUselessGroups;
        private DevExpress.XtraBars.Ribbon.RibbonPage rpView;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgView;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgFilter;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgNumberOfHH;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditSelectedHH;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditHideHiddenSystems;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditWarnAboutUselessGroups;
        private DevExpress.XtraBars.BarButtonItem btnSelectAll;
        private DevExpress.XtraBars.Ribbon.RibbonPage chk;
        private DevExpress.XtraBars.BarEditItem txtFilterDatasets;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit textEditFilterDatasets;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit textEditNumberOfHH;
        private DevExpress.XtraBars.BarEditItem txtFilterSystems;
        private DevExpress.XtraBars.BarEditItem txtNumberOfHH; 
        private DevExpress.XtraBars.BarEditItem chkDoNotStopOnNonCriticalErrors;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditDoNotStop;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgSettings;
        private DevExpress.XtraBars.BarEditItem chkAddDateToOuputFilename;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditAddDate;
        private DevExpress.XtraBars.BarEditItem chkLogRuntimeInDetail;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditLogRuntime;
        private DevExpress.XtraBars.BarEditItem chkCloseAfterRun;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditCloseAfter;
        private System.Windows.Forms.Button btnSelectOutputPath;
        private DevExpress.XtraBars.BarEditItem cmbAddOns;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckedComboBoxEdit comboBoxAddOns;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgAddOns;
        private DevExpress.XtraBars.BarButtonItem btnSelectNo;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgSelect;
        private DevExpress.XtraBars.BarButtonItem btnSelectAllSystems;
        private DevExpress.XtraBars.BarButtonItem btnSelectNoSystem;
        private DevExpress.XtraBars.BarButtonItem btnSelectAllCountries;
        private DevExpress.XtraBars.BarButtonItem btnSelectNoCountry;
        private DevExpress.XtraBars.BarButtonItem btnSelectAllAddOns;
        private DevExpress.XtraBars.BarButtonItem btnSelectNoAddOn;
        internal System.Windows.Forms.DataGridViewCheckBoxColumn colRun;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCountry;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colSystem;
        internal System.Windows.Forms.DataGridViewComboBoxColumn colDataset;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colFirstHH;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colLastHH;
        private DevExpress.XtraBars.BarEditItem chkBestMatchOnly;
        private DevExpress.XtraBars.BarEditItem chkRegularExpression;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditBestMatchOnly;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditRegularExpression;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgExtensions;
        private DevExpress.XtraBars.BarButtonItem btnRestorePolicySwitchDefaults;
        private DevExpress.XtraBars.BarEditItem chkDoNotPoolSystemsDatasets;
        private DevExpress.XtraBars.BarEditItem chkRunPublicOnly;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditDoNotPool;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditPublicOnly;
        private CustomControls.EM_CustomToggleBarEditItem toggleAutoRename;
        private CustomControls.RepositoryItemEM_CustomToggle toggleSwitchAutoRename;
        private DevExpress.XtraBars.BarStaticItem lblAutoRename;
        private System.Windows.Forms.ContextMenuStrip ctmMultiPolicySwitch;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsOn;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsOff;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsDefault;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsAll;
        private CustomControls.EM_RadioValueBarEditItem customParallelRunsItem;
        private CustomControls.RepositoryItemEM_RadioValueEditor repositoryItemParallelRuns;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgOutputEuro;
        internal DevExpress.XtraBars.BarEditItem chkOutputEuro;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditOutputEuro;
        internal DevExpress.XtraBars.BarEditItem cmbExRateDate;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox comboBoxExRateDate;
        private DevExpress.XtraBars.BarHeaderItem cptExchangeRate;
        private DevExpress.XtraBars.BarEditItem chkRunEM2;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit checkEditRunEM2;
    }
}