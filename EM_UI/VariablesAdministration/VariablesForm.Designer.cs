namespace EM_UI.VariablesAdministration
{
    partial class VariablesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VariablesForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.ribbon = new DevExpress.XtraBars.Ribbon.RibbonControl();
            this.applicationMenu = new DevExpress.XtraBars.Ribbon.ApplicationMenu(this.components);
            this.btnSave = new DevExpress.XtraBars.BarButtonItem();
            this.btnImportVariables = new DevExpress.XtraBars.BarButtonItem();
            this.btnCleanVariables = new DevExpress.XtraBars.BarButtonItem();
            this.btnClose = new DevExpress.XtraBars.BarButtonItem();
            this.barAndDockingController = new DevExpress.XtraBars.BarAndDockingController(this.components);
            this.btnUndo = new DevExpress.XtraBars.BarButtonItem();
            this.btnRedo = new DevExpress.XtraBars.BarButtonItem();
            this.btnAddVariable = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteVariable = new DevExpress.XtraBars.BarButtonItem();
            this.chkMonetary = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.chkNonMonetary = new DevExpress.XtraBars.BarEditItem();
            this.chkSimulated = new DevExpress.XtraBars.BarEditItem();
            this.chkData = new DevExpress.XtraBars.BarEditItem();
            this.chkIndLevel = new DevExpress.XtraBars.BarEditItem();
            this.chkHHLevel = new DevExpress.XtraBars.BarEditItem();
            this.chkNonCategorical = new DevExpress.XtraBars.BarEditItem();
            this.chkCategorical = new DevExpress.XtraBars.BarEditItem();
            this.btnAddType = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteType = new DevExpress.XtraBars.BarButtonItem();
            this.btnAddLevel = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteLevel = new DevExpress.XtraBars.BarButtonItem();
            this.btnAddAcronym = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteAcronym = new DevExpress.XtraBars.BarButtonItem();
            this.btnAddCategory = new DevExpress.XtraBars.BarButtonItem();
            this.btnDeleteCategories = new DevExpress.XtraBars.BarButtonItem();
            this.btnApplyFilters = new DevExpress.XtraBars.BarButtonItem();
            this.btnUpdateAutomaticLabel = new DevExpress.XtraBars.BarButtonItem();
            this.btnExpandAcronyms = new DevExpress.XtraBars.BarButtonItem();
            this.btnCollapseAcronyms = new DevExpress.XtraBars.BarButtonItem();
            this.btnSelectAllFilters = new DevExpress.XtraBars.BarButtonItem();
            this.btnUnselectAllFilters = new DevExpress.XtraBars.BarButtonItem();
            this.cmbCountry = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemComoBoxCountries = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.chkHasSpecificDescription = new DevExpress.XtraBars.BarEditItem();
            this.txtSearchVariable = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEditSearchVariable = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.btnSearchVariable = new DevExpress.XtraBars.BarButtonItem();
            this.txtSearchAcronym = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemTextEditSearchAcronym = new DevExpress.XtraEditors.Repository.RepositoryItemTextEdit();
            this.cmbAcronymType = new DevExpress.XtraBars.BarEditItem();
            this.repositoryItemComboBoxTypes = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.btnSearchAcronym = new DevExpress.XtraBars.BarButtonItem();
            this.btnSwitchablePolicies = new DevExpress.XtraBars.BarButtonItem();
            this.txtSearchAcroByDescription = new DevExpress.XtraBars.BarStaticItem();
            this.btnSearchAcroByDescription = new DevExpress.XtraBars.BarButtonItem();
            this.btnSearchAcronymDescription = new DevExpress.XtraBars.BarButtonItem();
            this.rpVariables = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rpgEdit = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgFilter = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgAutomaticLabel = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgSearchVariable = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpAcronyms = new DevExpress.XtraBars.Ribbon.RibbonPage();
            this.rpgType = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgLevel = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgAcronym = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgCategory = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgExpand = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.rpgSearchAcronym = new DevExpress.XtraBars.Ribbon.RibbonPageGroup();
            this.ribbonStatusBar = new DevExpress.XtraBars.Ribbon.RibbonStatusBar();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.lblVariables = new System.Windows.Forms.Label();
            this.lblDescriptions = new System.Windows.Forms.Label();
            this.lblCategories = new System.Windows.Forms.Label();
            this.lblAcronyms = new System.Windows.Forms.Label();
            this.treeAcronyms = new DevExpress.XtraTreeList.TreeList();
            this.colAcronym = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colAcronymDescription = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.RunEditor = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.dgvDescriptions = new System.Windows.Forms.DataGridView();
            this.colCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCountryDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvCategories = new System.Windows.Forms.DataGridView();
            this.colCategoryValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategoryDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.dgvVariables = new System.Windows.Forms.DataGridView();
            this.colVariableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colMonetary = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colHHLevel = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colCategorical = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colAutomaticLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.tipVariablesForm = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.applicationMenu)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComoBoxCountries)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditSearchVariable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditSearchAcronym)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxTypes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeAcronyms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.RunEditor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDescriptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // ribbon
            // 
            this.ribbon.ApplicationButtonDropDownControl = this.applicationMenu;
            this.ribbon.ApplicationButtonText = null;
            this.ribbon.Controller = this.barAndDockingController;
            this.ribbon.ExpandCollapseItem.Id = 0;
            this.ribbon.Items.AddRange(new DevExpress.XtraBars.BarItem[] {
            this.ribbon.ExpandCollapseItem,
            this.btnUndo,
            this.btnRedo,
            this.btnSave,
            this.btnClose,
            this.btnAddVariable,
            this.btnDeleteVariable,
            this.chkMonetary,
            this.chkNonMonetary,
            this.chkSimulated,
            this.chkData,
            this.chkIndLevel,
            this.chkHHLevel,
            this.chkNonCategorical,
            this.chkCategorical,
            this.btnImportVariables,
            this.btnAddType,
            this.btnDeleteType,
            this.btnAddLevel,
            this.btnDeleteLevel,
            this.btnAddAcronym,
            this.btnDeleteAcronym,
            this.btnCleanVariables,
            this.btnAddCategory,
            this.btnDeleteCategories,
            this.btnApplyFilters,
            this.btnUpdateAutomaticLabel,
            this.btnExpandAcronyms,
            this.btnCollapseAcronyms,
            this.btnSelectAllFilters,
            this.btnUnselectAllFilters,
            this.cmbCountry,
            this.chkHasSpecificDescription,
            this.txtSearchVariable,
            this.btnSearchVariable,
            this.txtSearchAcronym,
            this.cmbAcronymType,
            this.btnSearchAcronym,
            this.btnSwitchablePolicies,
            this.txtSearchAcroByDescription,
            this.btnSearchAcroByDescription,
            this.btnSearchAcronymDescription});
            this.ribbon.Location = new System.Drawing.Point(0, 0);
            this.ribbon.MaxItemId = 100;
            this.ribbon.Name = "ribbon";
            this.ribbon.Pages.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPage[] {
            this.rpVariables,
            this.rpAcronyms});
            this.ribbon.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemCheckEdit,
            this.repositoryItemComoBoxCountries,
            this.repositoryItemTextEditSearchAcronym,
            this.repositoryItemComboBoxTypes,
            this.repositoryItemTextEditSearchVariable});
            this.ribbon.RibbonStyle = DevExpress.XtraBars.Ribbon.RibbonControlStyle.Office2010;
            this.ribbon.Size = new System.Drawing.Size(1074, 144);
            this.ribbon.StatusBar = this.ribbonStatusBar;
            this.ribbon.Toolbar.ItemLinks.Add(this.btnUndo);
            this.ribbon.Toolbar.ItemLinks.Add(this.btnRedo);
            // 
            // applicationMenu
            // 
            this.applicationMenu.ItemLinks.Add(this.btnSave);
            this.applicationMenu.ItemLinks.Add(this.btnImportVariables);
            this.applicationMenu.ItemLinks.Add(this.btnCleanVariables);
            this.applicationMenu.ItemLinks.Add(this.btnClose);
            this.applicationMenu.Name = "applicationMenu";
            this.applicationMenu.Ribbon = this.ribbon;
            // 
            // btnSave
            // 
            this.btnSave.Caption = "Save (Ctrl-S)";
            this.btnSave.Glyph = global::EM_UI.Properties.Resources.save_32;
            this.btnSave.Id = 4;
            this.btnSave.Name = "btnSave";
            this.btnSave.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSave_ItemClick);
            // 
            // btnImportVariables
            // 
            this.btnImportVariables.Caption = "Import Variables";
            this.btnImportVariables.Glyph = global::EM_UI.Properties.Resources.import;
            this.btnImportVariables.Id = 36;
            this.btnImportVariables.Name = "btnImportVariables";
            this.btnImportVariables.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnImportVariables_ItemClick);
            // 
            // btnCleanVariables
            // 
            this.btnCleanVariables.Caption = "Clean Variables";
            this.btnCleanVariables.Id = 45;
            this.btnCleanVariables.LargeGlyph = global::EM_UI.Properties.Resources.CleanVariables;
            this.btnCleanVariables.Name = "btnCleanVariables";
            this.btnCleanVariables.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnCleanVariables_ItemClick);
            // 
            // btnClose
            // 
            this.btnClose.Caption = "Close";
            this.btnClose.Glyph = global::EM_UI.Properties.Resources.Turn_off;
            this.btnClose.Id = 5;
            this.btnClose.Name = "btnClose";
            this.btnClose.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnClose_ItemClick);
            // 
            // barAndDockingController
            // 
            this.barAndDockingController.LookAndFeel.SkinName = "Black";
            this.barAndDockingController.LookAndFeel.UseDefaultLookAndFeel = false;
            this.barAndDockingController.PropertiesBar.AllowLinkLighting = false;
            this.barAndDockingController.PropertiesBar.DefaultGlyphSize = new System.Drawing.Size(16, 16);
            this.barAndDockingController.PropertiesBar.DefaultLargeGlyphSize = new System.Drawing.Size(32, 32);
            // 
            // btnUndo
            // 
            this.btnUndo.Caption = "Undo";
            this.btnUndo.Glyph = global::EM_UI.Properties.Resources.undo;
            this.btnUndo.Id = 2;
            this.btnUndo.Name = "btnUndo";
            this.btnUndo.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnUndo_ItemClick);
            // 
            // btnRedo
            // 
            this.btnRedo.Caption = "Redo";
            this.btnRedo.Glyph = global::EM_UI.Properties.Resources.redo;
            this.btnRedo.Id = 3;
            this.btnRedo.Name = "btnRedo";
            this.btnRedo.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnRedo_ItemClick);
            // 
            // btnAddVariable
            // 
            this.btnAddVariable.Caption = "Add Variable";
            this.btnAddVariable.Glyph = ((System.Drawing.Image)(resources.GetObject("btnAddVariable.Glyph")));
            this.btnAddVariable.Hint = "Add new variable below the selected";
            this.btnAddVariable.Id = 6;
            this.btnAddVariable.ItemAppearance.Normal.Options.UseTextOptions = true;
            this.btnAddVariable.ItemAppearance.Normal.TextOptions.WordWrap = DevExpress.Utils.WordWrap.NoWrap;
            this.btnAddVariable.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.V));
            this.btnAddVariable.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnAddVariable.LargeGlyph")));
            this.btnAddVariable.Name = "btnAddVariable";
            this.btnAddVariable.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddVariable_ItemClick);
            // 
            // btnDeleteVariable
            // 
            this.btnDeleteVariable.Caption = "Delete Variable";
            this.btnDeleteVariable.Glyph = global::EM_UI.Properties.Resources.delete32;
            this.btnDeleteVariable.Hint = "Delete selected variable";
            this.btnDeleteVariable.Id = 7;
            this.btnDeleteVariable.ItemAppearance.Normal.Options.UseTextOptions = true;
            this.btnDeleteVariable.ItemAppearance.Normal.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.btnDeleteVariable.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.V));
            this.btnDeleteVariable.LargeGlyph = global::EM_UI.Properties.Resources.delete32;
            this.btnDeleteVariable.Name = "btnDeleteVariable";
            this.btnDeleteVariable.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteVariable_ItemClick);
            // 
            // chkMonetary
            // 
            this.chkMonetary.Caption = "Monetary";
            this.chkMonetary.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkMonetary.Edit = this.repositoryItemCheckEdit;
            this.chkMonetary.EditValue = true;
            this.chkMonetary.Id = 27;
            this.chkMonetary.Name = "chkMonetary";
            // 
            // repositoryItemCheckEdit
            // 
            this.repositoryItemCheckEdit.AccessibleName = "";
            this.repositoryItemCheckEdit.Appearance.BackColor = System.Drawing.Color.Transparent;
            this.repositoryItemCheckEdit.Appearance.BackColor2 = System.Drawing.Color.Transparent;
            this.repositoryItemCheckEdit.Appearance.Options.UseBackColor = true;
            this.repositoryItemCheckEdit.Appearance.Options.UseTextOptions = true;
            this.repositoryItemCheckEdit.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.repositoryItemCheckEdit.AppearanceFocused.BackColor = System.Drawing.Color.Transparent;
            this.repositoryItemCheckEdit.AppearanceFocused.BackColor2 = System.Drawing.Color.Transparent;
            this.repositoryItemCheckEdit.AppearanceFocused.Options.UseBackColor = true;
            this.repositoryItemCheckEdit.AutoHeight = false;
            this.repositoryItemCheckEdit.Caption = "";
            this.repositoryItemCheckEdit.Name = "repositoryItemCheckEdit";
            // 
            // chkNonMonetary
            // 
            this.chkNonMonetary.Caption = "Non-monetary";
            this.chkNonMonetary.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkNonMonetary.Edit = this.repositoryItemCheckEdit;
            this.chkNonMonetary.EditValue = true;
            this.chkNonMonetary.Id = 28;
            this.chkNonMonetary.Name = "chkNonMonetary";
            // 
            // chkSimulated
            // 
            this.chkSimulated.Caption = "Simulated";
            this.chkSimulated.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkSimulated.Edit = this.repositoryItemCheckEdit;
            this.chkSimulated.EditValue = true;
            this.chkSimulated.Id = 29;
            this.chkSimulated.Name = "chkSimulated";
            // 
            // chkData
            // 
            this.chkData.Caption = "Data";
            this.chkData.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkData.Edit = this.repositoryItemCheckEdit;
            this.chkData.EditValue = true;
            this.chkData.Id = 30;
            this.chkData.Name = "chkData";
            // 
            // chkIndLevel
            // 
            this.chkIndLevel.Caption = "Individual-level";
            this.chkIndLevel.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkIndLevel.Edit = this.repositoryItemCheckEdit;
            this.chkIndLevel.EditValue = true;
            this.chkIndLevel.Id = 999;
            this.chkIndLevel.Name = "chkIndLevel";
            // 
            // chkHHLevel
            // 
            this.chkHHLevel.Caption = "HH-level";
            this.chkHHLevel.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkHHLevel.Edit = this.repositoryItemCheckEdit;
            this.chkHHLevel.EditValue = true;
            this.chkHHLevel.Id = 998;
            this.chkHHLevel.Name = "chkHHLevel";
            // 
            // chkNonCategorical
            // 
            this.chkNonCategorical.Caption = "Non-categorical";
            this.chkNonCategorical.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkNonCategorical.Edit = this.repositoryItemCheckEdit;
            this.chkNonCategorical.EditValue = true;
            this.chkNonCategorical.Id = 998;
            this.chkNonCategorical.Name = "chkNonCategorical";
            // 
            // chkCategorical
            // 
            this.chkCategorical.Caption = "Categorical";
            this.chkCategorical.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkCategorical.Edit = this.repositoryItemCheckEdit;
            this.chkCategorical.EditValue = true;
            this.chkCategorical.Id = 996;
            this.chkCategorical.Name = "chkCategorical";
            // 
            // btnAddType
            // 
            this.btnAddType.Caption = "Add Type";
            this.btnAddType.Hint = "Add acronym type (equivalent to e.g. BENEFIT) at the end of the list";
            this.btnAddType.Id = 37;
            this.btnAddType.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.T));
            this.btnAddType.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnAddType.LargeGlyph")));
            this.btnAddType.Name = "btnAddType";
            this.btnAddType.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddType_ItemClick);
            // 
            // btnDeleteType
            // 
            this.btnDeleteType.Caption = "Delete Type";
            this.btnDeleteType.Hint = "Delete selected acronym type";
            this.btnDeleteType.Id = 38;
            this.btnDeleteType.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.T));
            this.btnDeleteType.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnDeleteType.LargeGlyph")));
            this.btnDeleteType.Name = "btnDeleteType";
            this.btnDeleteType.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteType_ItemClick);
            // 
            // btnAddLevel
            // 
            this.btnAddLevel.Caption = "Add Level";
            this.btnAddLevel.Hint = "Add acronym level (equivalent to \'benefit type\') below selected row";
            this.btnAddLevel.Id = 39;
            this.btnAddLevel.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.L));
            this.btnAddLevel.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnAddLevel.LargeGlyph")));
            this.btnAddLevel.Name = "btnAddLevel";
            this.btnAddLevel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddLevel_ItemClick);
            // 
            // btnDeleteLevel
            // 
            this.btnDeleteLevel.Caption = "Delete Level";
            this.btnDeleteLevel.Hint = "Delete selected acronym level";
            this.btnDeleteLevel.Id = 40;
            this.btnDeleteLevel.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.L));
            this.btnDeleteLevel.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnDeleteLevel.LargeGlyph")));
            this.btnDeleteLevel.Name = "btnDeleteLevel";
            this.btnDeleteLevel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteLevel_ItemClick);
            // 
            // btnAddAcronym
            // 
            this.btnAddAcronym.Caption = "Add Acronym";
            this.btnAddAcronym.Hint = "Add acronym at the end of the list";
            this.btnAddAcronym.Id = 41;
            this.btnAddAcronym.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.A));
            this.btnAddAcronym.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnAddAcronym.LargeGlyph")));
            this.btnAddAcronym.Name = "btnAddAcronym";
            this.btnAddAcronym.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddAcronym_ItemClick);
            // 
            // btnDeleteAcronym
            // 
            this.btnDeleteAcronym.Caption = "Delete Acronym";
            this.btnDeleteAcronym.Hint = "Delete selected acronym";
            this.btnDeleteAcronym.Id = 42;
            this.btnDeleteAcronym.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.A));
            this.btnDeleteAcronym.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnDeleteAcronym.LargeGlyph")));
            this.btnDeleteAcronym.Name = "btnDeleteAcronym";
            this.btnDeleteAcronym.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteAcronym_ItemClick);
            // 
            // btnAddCategory
            // 
            this.btnAddCategory.Caption = "Add Category";
            this.btnAddCategory.Hint = "Add category to the selected acronym";
            this.btnAddCategory.Id = 46;
            this.btnAddCategory.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.C));
            this.btnAddCategory.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnAddCategory.LargeGlyph")));
            this.btnAddCategory.Name = "btnAddCategory";
            this.btnAddCategory.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnAddCategory_ItemClick);
            // 
            // btnDeleteCategories
            // 
            this.btnDeleteCategories.Caption = "Delete Categories";
            this.btnDeleteCategories.Hint = "Delete selected category";
            this.btnDeleteCategories.Id = 47;
            this.btnDeleteCategories.ItemShortcut = new DevExpress.XtraBars.BarShortcut(((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Shift) 
                | System.Windows.Forms.Keys.C));
            this.btnDeleteCategories.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnDeleteCategories.LargeGlyph")));
            this.btnDeleteCategories.Name = "btnDeleteCategories";
            this.btnDeleteCategories.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnDeleteCategories_ItemClick);
            // 
            // btnApplyFilters
            // 
            this.btnApplyFilters.Caption = "Apply Filters";
            this.btnApplyFilters.Hint = "Display variables with ticked properties only";
            this.btnApplyFilters.Id = 51;
            this.btnApplyFilters.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F));
            this.btnApplyFilters.LargeGlyph = global::EM_UI.Properties.Resources.Filter;
            this.btnApplyFilters.Name = "btnApplyFilters";
            this.btnApplyFilters.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnApplyFilters_ItemClick);
            // 
            // btnUpdateAutomaticLabel
            // 
            this.btnUpdateAutomaticLabel.Caption = "Update Automatic Label";
            this.btnUpdateAutomaticLabel.Hint = "Update column Automatic Label in Variables list";
            this.btnUpdateAutomaticLabel.Id = 52;
            this.btnUpdateAutomaticLabel.ItemShortcut = new DevExpress.XtraBars.BarShortcut((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.U));
            this.btnUpdateAutomaticLabel.LargeGlyph = ((System.Drawing.Image)(resources.GetObject("btnUpdateAutomaticLabel.LargeGlyph")));
            this.btnUpdateAutomaticLabel.Name = "btnUpdateAutomaticLabel";
            this.btnUpdateAutomaticLabel.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnUpdateAutomaticLabel_ItemClick);
            // 
            // btnExpandAcronyms
            // 
            this.btnExpandAcronyms.Caption = "Expand";
            this.btnExpandAcronyms.Glyph = ((System.Drawing.Image)(resources.GetObject("btnExpandAcronyms.Glyph")));
            this.btnExpandAcronyms.Id = 53;
            this.btnExpandAcronyms.Name = "btnExpandAcronyms";
            this.btnExpandAcronyms.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnExpandAcronyms_ItemClick);
            // 
            // btnCollapseAcronyms
            // 
            this.btnCollapseAcronyms.Caption = "Collapse";
            this.btnCollapseAcronyms.Glyph = ((System.Drawing.Image)(resources.GetObject("btnCollapseAcronyms.Glyph")));
            this.btnCollapseAcronyms.Id = 54;
            this.btnCollapseAcronyms.Name = "btnCollapseAcronyms";
            this.btnCollapseAcronyms.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnCollapseAcronyms_ItemClick);
            // 
            // btnSelectAllFilters
            // 
            this.btnSelectAllFilters.Caption = "Select All Filters";
            this.btnSelectAllFilters.Glyph = global::EM_UI.Properties.Resources.AllFilters;
            this.btnSelectAllFilters.Id = 59;
            this.btnSelectAllFilters.Name = "btnSelectAllFilters";
            this.btnSelectAllFilters.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSelectAllFilters_ItemClick);
            // 
            // btnUnselectAllFilters
            // 
            this.btnUnselectAllFilters.Caption = "Unselect All Filters";
            this.btnUnselectAllFilters.Glyph = global::EM_UI.Properties.Resources.NoFilter;
            this.btnUnselectAllFilters.Id = 60;
            this.btnUnselectAllFilters.Name = "btnUnselectAllFilters";
            this.btnUnselectAllFilters.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnUnselectAllFilters_ItemClick);
            // 
            // cmbCountry
            // 
            this.cmbCountry.Caption = "        Country";
            this.cmbCountry.Edit = this.repositoryItemComoBoxCountries;
            this.cmbCountry.Id = 69;
            this.cmbCountry.Name = "cmbCountry";
            this.cmbCountry.Width = 100;
            // 
            // repositoryItemComoBoxCountries
            // 
            this.repositoryItemComoBoxCountries.AutoHeight = false;
            this.repositoryItemComoBoxCountries.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComoBoxCountries.Items.AddRange(new object[] {
            "Any Country",
            "Austria",
            "Belgium",
            "Cyprus",
            "Denmark",
            "Sweden",
            "UK"});
            this.repositoryItemComoBoxCountries.Name = "repositoryItemComoBoxCountries";
            this.repositoryItemComoBoxCountries.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // chkHasSpecificDescription
            // 
            this.chkHasSpecificDescription.Caption = "Has Specific Description";
            this.chkHasSpecificDescription.CaptionAlignment = DevExpress.Utils.HorzAlignment.Far;
            this.chkHasSpecificDescription.Edit = this.repositoryItemCheckEdit;
            this.chkHasSpecificDescription.EditValue = false;
            this.chkHasSpecificDescription.Id = 71;
            this.chkHasSpecificDescription.Name = "chkHasSpecificDescription";
            // 
            // txtSearchVariable
            // 
            this.txtSearchVariable.Edit = this.repositoryItemTextEditSearchVariable;
            this.txtSearchVariable.Id = 72;
            this.txtSearchVariable.Name = "txtSearchVariable";
            // 
            // repositoryItemTextEditSearchVariable
            // 
            this.repositoryItemTextEditSearchVariable.AutoHeight = false;
            this.repositoryItemTextEditSearchVariable.Name = "repositoryItemTextEditSearchVariable";
            this.repositoryItemTextEditSearchVariable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.repositoryItemTextEditSearchVariable_KeyDown);
            // 
            // btnSearchVariable
            // 
            this.btnSearchVariable.Caption = "Search";
            this.btnSearchVariable.Glyph = global::EM_UI.Properties.Resources.find;
            this.btnSearchVariable.Id = 73;
            this.btnSearchVariable.Name = "btnSearchVariable";
            this.btnSearchVariable.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSearchVariable_ItemClick);
            // 
            // txtSearchAcronym
            // 
            this.txtSearchAcronym.Edit = this.repositoryItemTextEditSearchAcronym;
            this.txtSearchAcronym.Id = 74;
            this.txtSearchAcronym.Name = "txtSearchAcronym";
            this.txtSearchAcronym.Width = 100;
            // 
            // repositoryItemTextEditSearchAcronym
            // 
            this.repositoryItemTextEditSearchAcronym.AutoHeight = false;
            this.repositoryItemTextEditSearchAcronym.Name = "repositoryItemTextEditSearchAcronym";
            this.repositoryItemTextEditSearchAcronym.KeyDown += new System.Windows.Forms.KeyEventHandler(this.repositoryItemTextEditSearchAcronym_KeyDown);
            // 
            // cmbAcronymType
            // 
            this.cmbAcronymType.Edit = this.repositoryItemComboBoxTypes;
            this.cmbAcronymType.Id = 75;
            this.cmbAcronymType.Name = "cmbAcronymType";
            this.cmbAcronymType.Width = 100;
            // 
            // repositoryItemComboBoxTypes
            // 
            this.repositoryItemComboBoxTypes.AutoHeight = false;
            this.repositoryItemComboBoxTypes.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBoxTypes.Name = "repositoryItemComboBoxTypes";
            // 
            // btnSearchAcronym
            // 
            this.btnSearchAcronym.Alignment = DevExpress.XtraBars.BarItemLinkAlignment.Left;
            this.btnSearchAcronym.Caption = "Acronym";
            this.btnSearchAcronym.Glyph = global::EM_UI.Properties.Resources.find;
            this.btnSearchAcronym.Id = 76;
            this.btnSearchAcronym.Name = "btnSearchAcronym";
            this.btnSearchAcronym.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSearchAcronym_ItemClick);
            // 
            // btnSwitchablePolicies
            // 
            this.btnSwitchablePolicies.Caption = "Administrate";
            this.btnSwitchablePolicies.Id = 77;
            this.btnSwitchablePolicies.LargeGlyph = global::EM_UI.Properties.Resources.Switch;
            this.btnSwitchablePolicies.Name = "btnSwitchablePolicies";
            // 
            // txtSearchAcroByDescription
            // 
            this.txtSearchAcroByDescription.Caption = "Description";
            this.txtSearchAcroByDescription.Id = 84;
            this.txtSearchAcroByDescription.Name = "txtSearchAcroByDescription";
            this.txtSearchAcroByDescription.TextAlignment = System.Drawing.StringAlignment.Near;
            // 
            // btnSearchAcroByDescription
            // 
            this.btnSearchAcroByDescription.Caption = "Sollte Text sein";
            this.btnSearchAcroByDescription.Id = 85;
            this.btnSearchAcroByDescription.Name = "btnSearchAcroByDescription";
            // 
            // btnSearchAcronymDescription
            // 
            this.btnSearchAcronymDescription.Caption = "Description";
            this.btnSearchAcronymDescription.Glyph = ((System.Drawing.Image)(resources.GetObject("btnSearchAcronymDescription.Glyph")));
            this.btnSearchAcronymDescription.Id = 86;
            this.btnSearchAcronymDescription.Name = "btnSearchAcronymDescription";
            this.btnSearchAcronymDescription.ItemClick += new DevExpress.XtraBars.ItemClickEventHandler(this.btnSearchAcronymDescription_ItemClick);
            // 
            // rpVariables
            // 
            this.rpVariables.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.rpgEdit,
            this.rpgFilter,
            this.rpgAutomaticLabel,
            this.rpgSearchVariable});
            this.rpVariables.Name = "rpVariables";
            this.rpVariables.Text = "Variables";
            // 
            // rpgEdit
            // 
            this.rpgEdit.ItemLinks.Add(this.btnAddVariable, "ADD VARIABLE BELOW THE SELECTED");
            this.rpgEdit.ItemLinks.Add(this.btnDeleteVariable);
            this.rpgEdit.Name = "rpgEdit";
            this.rpgEdit.ShowCaptionButton = false;
            this.rpgEdit.Text = "Edit";
            // 
            // rpgFilter
            // 
            this.rpgFilter.Glyph = ((System.Drawing.Image)(resources.GetObject("rpgFilter.Glyph")));
            this.rpgFilter.ItemLinks.Add(this.btnApplyFilters);
            this.rpgFilter.ItemLinks.Add(this.btnSelectAllFilters, true);
            this.rpgFilter.ItemLinks.Add(this.btnUnselectAllFilters);
            this.rpgFilter.ItemLinks.Add(this.chkMonetary, true);
            this.rpgFilter.ItemLinks.Add(this.chkNonMonetary);
            this.rpgFilter.ItemLinks.Add(this.chkData, true);
            this.rpgFilter.ItemLinks.Add(this.chkSimulated);
            this.rpgFilter.ItemLinks.Add(this.chkIndLevel, true);
            this.rpgFilter.ItemLinks.Add(this.chkHHLevel);
            this.rpgFilter.ItemLinks.Add(this.chkNonCategorical, true);
            this.rpgFilter.ItemLinks.Add(this.chkCategorical);
            this.rpgFilter.ItemLinks.Add(this.chkHasSpecificDescription, true);
            this.rpgFilter.ItemLinks.Add(this.cmbCountry);
            this.rpgFilter.Name = "rpgFilter";
            this.rpgFilter.ShowCaptionButton = false;
            this.rpgFilter.Text = "Show variables ...";
            // 
            // rpgAutomaticLabel
            // 
            this.rpgAutomaticLabel.Glyph = ((System.Drawing.Image)(resources.GetObject("rpgAutomaticLabel.Glyph")));
            this.rpgAutomaticLabel.ItemLinks.Add(this.btnUpdateAutomaticLabel);
            this.rpgAutomaticLabel.Name = "rpgAutomaticLabel";
            this.rpgAutomaticLabel.ShowCaptionButton = false;
            this.rpgAutomaticLabel.Text = "Automatic label";
            this.rpgAutomaticLabel.Visible = false;
            // 
            // rpgSearchVariable
            // 
            this.rpgSearchVariable.AllowMinimize = false;
            this.rpgSearchVariable.ItemLinks.Add(this.txtSearchVariable);
            this.rpgSearchVariable.ItemLinks.Add(this.btnSearchVariable);
            this.rpgSearchVariable.Name = "rpgSearchVariable";
            this.rpgSearchVariable.ShowCaptionButton = false;
            this.rpgSearchVariable.Text = "Search Variable";
            // 
            // rpAcronyms
            // 
            this.rpAcronyms.Groups.AddRange(new DevExpress.XtraBars.Ribbon.RibbonPageGroup[] {
            this.rpgType,
            this.rpgLevel,
            this.rpgAcronym,
            this.rpgCategory,
            this.rpgExpand,
            this.rpgSearchAcronym});
            this.rpAcronyms.Name = "rpAcronyms";
            this.rpAcronyms.Text = "Acronyms";
            // 
            // rpgType
            // 
            this.rpgType.ItemLinks.Add(this.btnAddType);
            this.rpgType.ItemLinks.Add(this.btnDeleteType);
            this.rpgType.Name = "rpgType";
            this.rpgType.ShowCaptionButton = false;
            this.rpgType.Text = "Type";
            // 
            // rpgLevel
            // 
            this.rpgLevel.ItemLinks.Add(this.btnAddLevel);
            this.rpgLevel.ItemLinks.Add(this.btnDeleteLevel);
            this.rpgLevel.Name = "rpgLevel";
            this.rpgLevel.ShowCaptionButton = false;
            this.rpgLevel.Text = "Level";
            // 
            // rpgAcronym
            // 
            this.rpgAcronym.ItemLinks.Add(this.btnAddAcronym);
            this.rpgAcronym.ItemLinks.Add(this.btnDeleteAcronym);
            this.rpgAcronym.Name = "rpgAcronym";
            this.rpgAcronym.ShowCaptionButton = false;
            this.rpgAcronym.Text = "Acronym";
            // 
            // rpgCategory
            // 
            this.rpgCategory.ItemLinks.Add(this.btnAddCategory);
            this.rpgCategory.ItemLinks.Add(this.btnDeleteCategories);
            this.rpgCategory.Name = "rpgCategory";
            this.rpgCategory.ShowCaptionButton = false;
            this.rpgCategory.Text = "Category";
            // 
            // rpgExpand
            // 
            this.rpgExpand.ItemLinks.Add(this.btnExpandAcronyms);
            this.rpgExpand.ItemLinks.Add(this.btnCollapseAcronyms);
            this.rpgExpand.Name = "rpgExpand";
            this.rpgExpand.ShowCaptionButton = false;
            this.rpgExpand.Text = "Expand/Collapse";
            // 
            // rpgSearchAcronym
            // 
            this.rpgSearchAcronym.AllowMinimize = false;
            this.rpgSearchAcronym.ItemLinks.Add(this.cmbAcronymType);
            this.rpgSearchAcronym.ItemLinks.Add(this.txtSearchAcronym);
            this.rpgSearchAcronym.ItemLinks.Add(this.btnSearchAcronym, true);
            this.rpgSearchAcronym.ItemLinks.Add(this.btnSearchAcronymDescription);
            this.rpgSearchAcronym.Name = "rpgSearchAcronym";
            this.rpgSearchAcronym.ShowCaptionButton = false;
            this.rpgSearchAcronym.Text = "Search Acronym";
            // 
            // ribbonStatusBar
            // 
            this.ribbonStatusBar.Location = new System.Drawing.Point(0, 478);
            this.ribbonStatusBar.Name = "ribbonStatusBar";
            this.ribbonStatusBar.Ribbon = this.ribbon;
            this.ribbonStatusBar.Size = new System.Drawing.Size(1074, 31);
            this.ribbonStatusBar.Visible = false;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // lblVariables
            // 
            this.lblVariables.AutoSize = true;
            this.lblVariables.BackColor = System.Drawing.Color.Gray;
            this.lblVariables.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblVariables.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVariables.ForeColor = System.Drawing.Color.White;
            this.lblVariables.Location = new System.Drawing.Point(0, 0);
            this.lblVariables.Name = "lblVariables";
            this.lblVariables.Size = new System.Drawing.Size(68, 16);
            this.lblVariables.TabIndex = 7;
            this.lblVariables.Text = "Variables";
            // 
            // lblDescriptions
            // 
            this.lblDescriptions.AutoSize = true;
            this.lblDescriptions.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblDescriptions.Location = new System.Drawing.Point(0, 0);
            this.lblDescriptions.Name = "lblDescriptions";
            this.lblDescriptions.Size = new System.Drawing.Size(65, 13);
            this.lblDescriptions.TabIndex = 8;
            this.lblDescriptions.Text = "Descriptions";
            // 
            // lblCategories
            // 
            this.lblCategories.AutoSize = true;
            this.lblCategories.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblCategories.Location = new System.Drawing.Point(43, 0);
            this.lblCategories.Name = "lblCategories";
            this.lblCategories.Size = new System.Drawing.Size(59, 13);
            this.lblCategories.TabIndex = 9;
            this.lblCategories.Text = "Categories";
            // 
            // lblAcronyms
            // 
            this.lblAcronyms.AutoSize = true;
            this.lblAcronyms.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblAcronyms.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAcronyms.Location = new System.Drawing.Point(43, 0);
            this.lblAcronyms.Name = "lblAcronyms";
            this.lblAcronyms.Size = new System.Drawing.Size(73, 16);
            this.lblAcronyms.TabIndex = 10;
            this.lblAcronyms.Text = "Acronyms";
            // 
            // treeAcronyms
            // 
            this.treeAcronyms.Appearance.EvenRow.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.EvenRow.BackColor2 = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.EvenRow.Options.UseBackColor = true;
            this.treeAcronyms.Appearance.FocusedCell.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.treeAcronyms.Appearance.FocusedCell.BackColor2 = System.Drawing.SystemColors.ActiveCaption;
            this.treeAcronyms.Appearance.FocusedCell.Options.UseBackColor = true;
            this.treeAcronyms.Appearance.FocusedRow.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.FocusedRow.BackColor2 = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.FocusedRow.Options.UseBackColor = true;
            this.treeAcronyms.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.HideSelectionRow.BackColor2 = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.treeAcronyms.Appearance.OddRow.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.OddRow.BackColor2 = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.OddRow.Options.UseBackColor = true;
            this.treeAcronyms.Appearance.SelectedRow.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.SelectedRow.Options.UseBackColor = true;
            this.treeAcronyms.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.treeAcronyms.Columns.AddRange(new DevExpress.XtraTreeList.Columns.TreeListColumn[] {
            this.colAcronym,
            this.colAcronymDescription});
            this.treeAcronyms.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeAcronyms.ImeMode = System.Windows.Forms.ImeMode.Katakana;
            this.treeAcronyms.Location = new System.Drawing.Point(43, 16);
            this.treeAcronyms.LookAndFeel.SkinName = "Black";
            this.treeAcronyms.LookAndFeel.UseDefaultLookAndFeel = false;
            this.treeAcronyms.MinWidth = 50;
            this.treeAcronyms.Name = "treeAcronyms";
            this.treeAcronyms.OptionsMenu.EnableColumnMenu = false;
            this.treeAcronyms.OptionsMenu.EnableFooterMenu = false;
            this.treeAcronyms.OptionsMenu.ShowAutoFilterRowItem = false;
            this.treeAcronyms.OptionsView.ShowIndicator = false;
            this.treeAcronyms.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.RunEditor});
            this.treeAcronyms.ShowButtonMode = DevExpress.XtraTreeList.ShowButtonModeEnum.ShowOnlyInEditor;
            this.treeAcronyms.Size = new System.Drawing.Size(386, 210);
            this.treeAcronyms.TabIndex = 2;
            this.treeAcronyms.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeAcronyms_FocusedNodeChanged);
            this.treeAcronyms.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeAcronyms_CellValueChanged);
            this.treeAcronyms.ShowingEditor += new System.ComponentModel.CancelEventHandler(this.treeAcronyms_ShowingEditor);
            this.treeAcronyms.Enter += new System.EventHandler(this.Control_Enter);
            this.treeAcronyms.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VariablesForm_KeyDown);
            this.treeAcronyms.Leave += new System.EventHandler(this.Control_Leave);
            // 
            // colAcronym
            // 
            this.colAcronym.Caption = "Acronym";
            this.colAcronym.FieldName = "colAcronym";
            this.colAcronym.MinWidth = 50;
            this.colAcronym.Name = "colAcronym";
            this.colAcronym.OptionsColumn.AllowSort = false;
            this.colAcronym.Visible = true;
            this.colAcronym.VisibleIndex = 1;
            this.colAcronym.Width = 148;
            // 
            // colAcronymDescription
            // 
            this.colAcronymDescription.Caption = "Description";
            this.colAcronymDescription.FieldName = "colAcronymDescription";
            this.colAcronymDescription.MinWidth = 50;
            this.colAcronymDescription.Name = "colAcronymDescription";
            this.colAcronymDescription.OptionsColumn.AllowSort = false;
            this.colAcronymDescription.Visible = true;
            this.colAcronymDescription.VisibleIndex = 0;
            this.colAcronymDescription.Width = 342;
            // 
            // RunEditor
            // 
            this.RunEditor.AutoHeight = false;
            this.RunEditor.Caption = "Check";
            this.RunEditor.Name = "RunEditor";
            // 
            // dgvDescriptions
            // 
            this.dgvDescriptions.AllowUserToAddRows = false;
            this.dgvDescriptions.AllowUserToDeleteRows = false;
            this.dgvDescriptions.AllowUserToResizeRows = false;
            this.dgvDescriptions.BackgroundColor = System.Drawing.Color.White;
            this.dgvDescriptions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDescriptions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCountry,
            this.colCountryDescription});
            this.dgvDescriptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDescriptions.GridColor = System.Drawing.SystemColors.Control;
            this.dgvDescriptions.Location = new System.Drawing.Point(0, 13);
            this.dgvDescriptions.MultiSelect = false;
            this.dgvDescriptions.Name = "dgvDescriptions";
            this.dgvDescriptions.RowHeadersVisible = false;
            this.dgvDescriptions.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDescriptions.Size = new System.Drawing.Size(621, 66);
            this.dgvDescriptions.TabIndex = 3;
            this.dgvDescriptions.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDescriptions_CellEndEdit);
            this.dgvDescriptions.SelectionChanged += new System.EventHandler(this.dgvDescriptions_SelectionChanged);
            this.dgvDescriptions.SortCompare += new System.Windows.Forms.DataGridViewSortCompareEventHandler(this.dgvDescriptions_SortCompare);
            this.dgvDescriptions.Enter += new System.EventHandler(this.Control_Enter);
            this.dgvDescriptions.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VariablesForm_KeyDown);
            this.dgvDescriptions.Leave += new System.EventHandler(this.Control_Leave);
            // 
            // colCountry
            // 
            this.colCountry.Frozen = true;
            this.colCountry.HeaderText = "Country";
            this.colCountry.Name = "colCountry";
            this.colCountry.ReadOnly = true;
            this.colCountry.Width = 71;
            // 
            // colCountryDescription
            // 
            this.colCountryDescription.Frozen = true;
            this.colCountryDescription.HeaderText = "Description";
            this.colCountryDescription.Name = "colCountryDescription";
            this.colCountryDescription.Width = 85;
            // 
            // dgvCategories
            // 
            this.dgvCategories.AllowUserToAddRows = false;
            this.dgvCategories.AllowUserToDeleteRows = false;
            this.dgvCategories.AllowUserToResizeRows = false;
            this.dgvCategories.BackgroundColor = System.Drawing.Color.White;
            this.dgvCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCategoryValue,
            this.colCategoryDescription});
            this.dgvCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvCategories.GridColor = System.Drawing.SystemColors.Control;
            this.dgvCategories.Location = new System.Drawing.Point(43, 13);
            this.dgvCategories.Name = "dgvCategories";
            this.dgvCategories.RowHeadersVisible = false;
            this.dgvCategories.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvCategories.Size = new System.Drawing.Size(386, 66);
            this.dgvCategories.TabIndex = 4;
            this.dgvCategories.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvCategories_CellEndEdit);
            this.dgvCategories.SelectionChanged += new System.EventHandler(this.dgvCategories_SelectionChanged);
            this.dgvCategories.Enter += new System.EventHandler(this.Control_Enter);
            this.dgvCategories.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VariablesForm_KeyDown);
            this.dgvCategories.Leave += new System.EventHandler(this.Control_Leave);
            // 
            // colCategoryValue
            // 
            this.colCategoryValue.Frozen = true;
            this.colCategoryValue.HeaderText = "Value";
            this.colCategoryValue.Name = "colCategoryValue";
            this.colCategoryValue.Width = 58;
            // 
            // colCategoryDescription
            // 
            this.colCategoryDescription.Frozen = true;
            this.colCategoryDescription.HeaderText = "Description";
            this.colCategoryDescription.Name = "colCategoryDescription";
            this.colCategoryDescription.Width = 85;
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(12, 155);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.panel1);
            this.splitContainer.Panel1.Controls.Add(this.panel2);
            this.splitContainer.Panel1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.panel4);
            this.splitContainer.Panel2.Controls.Add(this.panel3);
            this.splitContainer.Panel2.Padding = new System.Windows.Forms.Padding(0, 16, 0, 0);
            this.splitContainer.Panel2.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.splitContainer.Size = new System.Drawing.Size(1050, 325);
            this.splitContainer.SplitterDistance = 226;
            this.splitContainer.TabIndex = 21;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.dgvVariables);
            this.panel1.Controls.Add(this.lblVariables);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(621, 226);
            this.panel1.TabIndex = 11;
            // 
            // dgvVariables
            // 
            this.dgvVariables.AllowUserToAddRows = false;
            this.dgvVariables.AllowUserToDeleteRows = false;
            this.dgvVariables.AllowUserToResizeRows = false;
            this.dgvVariables.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Tahoma", 8.25F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvVariables.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvVariables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVariables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVariableName,
            this.colMonetary,
            this.colHHLevel,
            this.colCategorical,
            this.colAutomaticLabel});
            this.dgvVariables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvVariables.GridColor = System.Drawing.SystemColors.Control;
            this.dgvVariables.Location = new System.Drawing.Point(0, 16);
            this.dgvVariables.MultiSelect = false;
            this.dgvVariables.Name = "dgvVariables";
            this.dgvVariables.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVariables.Size = new System.Drawing.Size(621, 210);
            this.dgvVariables.TabIndex = 1;
            this.dgvVariables.CellBeginEdit += new System.Windows.Forms.DataGridViewCellCancelEventHandler(this.dgvVariables_CellBeginEdit);
            this.dgvVariables.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVariables_CellEndEdit);
            this.dgvVariables.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgvVariables_RowPostPaint);
            this.dgvVariables.SelectionChanged += new System.EventHandler(this.dgvVariables_SelectionChanged);
            this.dgvVariables.Enter += new System.EventHandler(this.Control_Enter);
            this.dgvVariables.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VariablesForm_KeyDown);
            this.dgvVariables.Leave += new System.EventHandler(this.Control_Leave);
            // 
            // colVariableName
            // 
            this.colVariableName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVariableName.HeaderText = "Name";
            this.colVariableName.Name = "colVariableName";
            this.colVariableName.Width = 59;
            // 
            // colMonetary
            // 
            this.colMonetary.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colMonetary.HeaderText = "Monetary";
            this.colMonetary.Name = "colMonetary";
            this.colMonetary.Width = 59;
            // 
            // colHHLevel
            // 
            this.colHHLevel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colHHLevel.HeaderText = "HH Level";
            this.colHHLevel.Name = "colHHLevel";
            this.colHHLevel.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colHHLevel.Width = 74;
            // 
            // colCategorical
            // 
            this.colCategorical.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colCategorical.HeaderText = "Categorical";
            this.colCategorical.Name = "colCategorical";
            this.colCategorical.ReadOnly = true;
            this.colCategorical.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colCategorical.Width = 86;
            // 
            // colAutomaticLabel
            // 
            this.colAutomaticLabel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colAutomaticLabel.HeaderText = "Automatic Label";
            this.colAutomaticLabel.Name = "colAutomaticLabel";
            this.colAutomaticLabel.ReadOnly = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.treeAcronyms);
            this.panel2.Controls.Add(this.lblAcronyms);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(621, 0);
            this.panel2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel2.Name = "panel2";
            this.panel2.Padding = new System.Windows.Forms.Padding(43, 0, 0, 0);
            this.panel2.Size = new System.Drawing.Size(429, 226);
            this.panel2.TabIndex = 12;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.dgvDescriptions);
            this.panel4.Controls.Add(this.lblDescriptions);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 16);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(621, 79);
            this.panel4.TabIndex = 11;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.dgvCategories);
            this.panel3.Controls.Add(this.lblCategories);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(621, 16);
            this.panel3.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panel3.Name = "panel3";
            this.panel3.Padding = new System.Windows.Forms.Padding(43, 0, 0, 0);
            this.panel3.Size = new System.Drawing.Size(429, 79);
            this.panel3.TabIndex = 10;
            // 
            // VariablesForm
            // 
            this.Appearance.ForeColor = System.Drawing.Color.Black;
            this.Appearance.Options.UseForeColor = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1074, 509);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.ribbonStatusBar);
            this.Controls.Add(this.ribbon);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_Variables.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "VariablesForm";
            this.Ribbon = this.ribbon;
            this.helpProvider.SetShowHelp(this, true);
            this.StatusBar = this.ribbonStatusBar;
            this.Text = "Administration of Variables and Acronyms";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.VariablesForm_FormClosing);
            this.Load += new System.EventHandler(this.VariablesForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.VariablesForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.ribbon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.applicationMenu)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.barAndDockingController)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEdit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComoBoxCountries)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditSearchVariable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemTextEditSearchAcronym)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxTypes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeAcronyms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.RunEditor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDescriptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        internal DevExpress.XtraBars.Ribbon.RibbonControl ribbon;
        internal DevExpress.XtraBars.Ribbon.RibbonPage rpVariables;
        internal DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgEdit;
        internal DevExpress.XtraBars.Ribbon.RibbonStatusBar ribbonStatusBar;
        internal System.Windows.Forms.HelpProvider helpProvider;
        internal DevExpress.XtraBars.BarAndDockingController barAndDockingController;
        internal DevExpress.XtraBars.BarButtonItem btnUndo;
        internal DevExpress.XtraBars.BarButtonItem btnRedo;
        internal DevExpress.XtraBars.Ribbon.RibbonPage rpAcronyms;
        internal DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgFilter;
        internal DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgType;
        internal DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgLevel;
        internal DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgAcronym;
        internal DevExpress.XtraBars.BarButtonItem btnSave;
        internal DevExpress.XtraBars.Ribbon.ApplicationMenu applicationMenu;
        internal DevExpress.XtraBars.BarButtonItem btnClose;
        internal DevExpress.XtraBars.BarButtonItem btnAddVariable;
        internal DevExpress.XtraBars.BarButtonItem btnDeleteVariable;
        internal DevExpress.XtraBars.BarEditItem chkMonetary;
        internal DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEdit;
        internal DevExpress.XtraBars.BarEditItem chkNonMonetary;
        internal DevExpress.XtraBars.BarEditItem chkSimulated;
        internal DevExpress.XtraBars.BarEditItem chkData;
        internal DevExpress.XtraBars.BarEditItem chkIndLevel;
        internal DevExpress.XtraBars.BarEditItem chkHHLevel;
        internal DevExpress.XtraBars.BarEditItem chkNonCategorical;
        internal DevExpress.XtraBars.BarEditItem chkCategorical;
        internal DevExpress.XtraBars.BarButtonItem btnImportVariables;
        internal DevExpress.XtraBars.BarButtonItem btnAddType;
        internal DevExpress.XtraBars.BarButtonItem btnDeleteType;
        internal DevExpress.XtraBars.BarButtonItem btnAddLevel;
        internal DevExpress.XtraBars.BarButtonItem btnDeleteLevel;
        internal DevExpress.XtraBars.BarButtonItem btnAddAcronym;
        internal DevExpress.XtraBars.BarButtonItem btnDeleteAcronym;
        internal System.Windows.Forms.Label lblVariables;
        internal System.Windows.Forms.Label lblDescriptions;
        internal System.Windows.Forms.Label lblCategories;
        internal System.Windows.Forms.Label lblAcronyms;
        internal DevExpress.XtraTreeList.TreeList treeAcronyms;
        internal DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit RunEditor;
        internal DevExpress.XtraBars.BarButtonItem btnCleanVariables;
        internal System.Windows.Forms.DataGridView dgvDescriptions;
        internal System.Windows.Forms.DataGridView dgvCategories;
        internal System.Windows.Forms.SplitContainer splitContainer;
        internal System.Windows.Forms.DataGridView dgvVariables;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colAcronym;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colAcronymDescription;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCategoryValue;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCategoryDescription;
        internal DevExpress.XtraBars.BarButtonItem btnAddCategory;
        internal DevExpress.XtraBars.BarButtonItem btnDeleteCategories;
        internal DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgCategory;
        private System.ComponentModel.IContainer components;
        private DevExpress.XtraBars.BarButtonItem btnApplyFilters;
        private System.Windows.Forms.ToolTip tipVariablesForm;
        private DevExpress.XtraBars.BarButtonItem btnUpdateAutomaticLabel;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgAutomaticLabel;
        private DevExpress.XtraBars.BarButtonItem btnExpandAcronyms;
        private DevExpress.XtraBars.BarButtonItem btnCollapseAcronyms;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgExpand;
        private DevExpress.XtraBars.BarButtonItem btnSelectAllFilters;
        private DevExpress.XtraBars.BarButtonItem btnUnselectAllFilters;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgSearchVariable;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComoBoxCountries;
        internal DevExpress.XtraBars.BarEditItem cmbCountry;
        internal DevExpress.XtraBars.BarEditItem chkHasSpecificDescription;
        internal DevExpress.XtraBars.BarEditItem txtSearchVariable;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEditSearchAcronym;
        private DevExpress.XtraBars.BarButtonItem btnSearchVariable;
        internal DevExpress.XtraBars.BarEditItem txtSearchAcronym;
        internal DevExpress.XtraBars.BarEditItem cmbAcronymType;
        private DevExpress.XtraBars.BarButtonItem btnSearchAcronym;
        private DevExpress.XtraBars.Ribbon.RibbonPageGroup rpgSearchAcronym;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBoxTypes;
        private DevExpress.XtraEditors.Repository.RepositoryItemTextEdit repositoryItemTextEditSearchVariable;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCountry;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCountryDescription;
        private DevExpress.XtraBars.BarButtonItem btnSwitchablePolicies;
        private DevExpress.XtraBars.BarStaticItem txtSearchAcroByDescription;
        private DevExpress.XtraBars.BarButtonItem btnSearchAcroByDescription;
        private DevExpress.XtraBars.BarButtonItem btnSearchAcronymDescription;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel3;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colVariableName;
        internal System.Windows.Forms.DataGridViewCheckBoxColumn colMonetary;
        internal System.Windows.Forms.DataGridViewCheckBoxColumn colHHLevel;
        internal System.Windows.Forms.DataGridViewCheckBoxColumn colCategorical;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colAutomaticLabel;
    }
}