namespace HypotheticalHousehold
{
    partial class InputForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
                if (householdBindingSource != null)
                {
                    householdBindingSource.Dispose();
                    householdBindingSource = null;
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InputForm));
            this.panelBottom = new System.Windows.Forms.Panel();
            this.generateButton = new DevExpress.XtraEditors.SimpleButton();
            this.toolTipButtons = new System.Windows.Forms.ToolTip(this.components);
            this.btnHouseholdAdd = new System.Windows.Forms.Button();
            this.btnHouseholdRemove = new System.Windows.Forms.Button();
            this.btnPersonAdd = new System.Windows.Forms.Button();
            this.btnPersonRemove = new System.Windows.Forms.Button();
            this.panelLeft = new System.Windows.Forms.Panel();
            this.panelLeftBottom = new System.Windows.Forms.Panel();
            this.treeHouseholds = new DevExpress.XtraTreeList.TreeList();
            this.toolTipControllerGrid = new DevExpress.Utils.ToolTipController(this.components);
            this.labelControl2 = new DevExpress.XtraEditors.LabelControl();
            this.panelLeftTop = new System.Windows.Forms.Panel();
            this.resetAllAdvButton = new System.Windows.Forms.Button();
            this.chkSelectAll = new System.Windows.Forms.CheckBox();
            this.mainMenuStrip = new System.Windows.Forms.MenuStrip();
            this.projectStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsProjectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.SaveAsPublicToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showAdvancedVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.showDerivedVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.showOnlyChangedValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.highlightChangedValuesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.wizardToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.advancedOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importHHToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importRefTabToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageCategoriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageCountriesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageYearsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.manageBasicVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageAdvancedVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageAdvancedCountryspecificVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageDerivedVariablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.manageReferenceTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveCurrentSettingsAsDefaultToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statisticsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.breakDownTypesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.breakDownCountryYearToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.budgetConstraintsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panelRightTop = new System.Windows.Forms.Panel();
            this.separatorLine = new System.Windows.Forms.Label();
            this.panelYears = new System.Windows.Forms.Panel();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelSelectedYears = new DevExpress.XtraEditors.LabelControl();
            this.yearsCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.panelCountries = new System.Windows.Forms.Panel();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.countriesCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.labelSelectedCountries = new DevExpress.XtraEditors.LabelControl();
            this.labelControl3 = new DevExpress.XtraEditors.LabelControl();
            this.txtQuickFilter = new DevExpress.XtraEditors.TextEdit();
            this.panelRightBottom = new System.Windows.Forms.Panel();
            this.gridHousehold = new DevExpress.XtraVerticalGrid.VGridControl();
            this.editorInteger = new DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit();
            this.editorFloat = new DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit();
            this.repositoryItemPopupContainerEditNumeric = new DevExpress.XtraEditors.Repository.RepositoryItemPopupContainerEdit();
            this.panelFilter = new System.Windows.Forms.Panel();
            this.panelTop = new System.Windows.Forms.Panel();
            this.splitMainInterface = new DevExpress.XtraEditors.SplitContainerControl();
            this.panelBottom.SuspendLayout();
            this.panelLeft.SuspendLayout();
            this.panelLeftBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeHouseholds)).BeginInit();
            this.panelLeftTop.SuspendLayout();
            this.mainMenuStrip.SuspendLayout();
            this.panelRightTop.SuspendLayout();
            this.panelYears.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yearsCheckedComboBoxEdit.Properties)).BeginInit();
            this.panelCountries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countriesCheckedComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuickFilter.Properties)).BeginInit();
            this.panelRightBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridHousehold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.editorInteger)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.editorFloat)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPopupContainerEditNumeric)).BeginInit();
            this.panelFilter.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMainInterface)).BeginInit();
            this.splitMainInterface.SuspendLayout();
            this.SuspendLayout();
            this.barManager = new DevExpress.XtraBars.BarManager(); //Added to solve the issue of the "shaking" of the cursor when right clicking.
            this.barManager.Form = this;
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.generateButton);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 668);
            this.panelBottom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(1299, 89);
            this.panelBottom.TabIndex = 4;
            // 
            // generateButton
            // 
            this.generateButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.generateButton.Appearance.Font = new System.Drawing.Font("Tahoma", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.generateButton.Appearance.Options.UseFont = true;
            this.generateButton.Location = new System.Drawing.Point(563, 11);
            this.generateButton.Margin = new System.Windows.Forms.Padding(3, 15, 160, 2);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(212, 65);
            this.generateButton.TabIndex = 6;
            this.generateButton.Text = "Generate";
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // toolTipButtons
            // 
            this.toolTipButtons.AutomaticDelay = 100;
            this.toolTipButtons.AutoPopDelay = 10000;
            this.toolTipButtons.InitialDelay = 100;
            this.toolTipButtons.ReshowDelay = 20;
            // 
            // btnHouseholdAdd
            // 
            this.btnHouseholdAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnHouseholdAdd.BackgroundImage")));
            this.btnHouseholdAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnHouseholdAdd.Location = new System.Drawing.Point(15, 14);
            this.btnHouseholdAdd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnHouseholdAdd.Name = "btnHouseholdAdd";
            this.btnHouseholdAdd.Size = new System.Drawing.Size(49, 50);
            this.btnHouseholdAdd.TabIndex = 0;
            this.toolTipButtons.SetToolTip(this.btnHouseholdAdd, "Add Household");
            this.btnHouseholdAdd.UseVisualStyleBackColor = true;
            this.btnHouseholdAdd.Click += new System.EventHandler(this.btnHouseholdAdd_Click);
            // 
            // btnHouseholdRemove
            // 
            this.btnHouseholdRemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnHouseholdRemove.BackgroundImage")));
            this.btnHouseholdRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnHouseholdRemove.Location = new System.Drawing.Point(65, 14);
            this.btnHouseholdRemove.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnHouseholdRemove.Name = "btnHouseholdRemove";
            this.btnHouseholdRemove.Size = new System.Drawing.Size(49, 50);
            this.btnHouseholdRemove.TabIndex = 1;
            this.toolTipButtons.SetToolTip(this.btnHouseholdRemove, "Delete Household");
            this.btnHouseholdRemove.UseVisualStyleBackColor = true;
            this.btnHouseholdRemove.Click += new System.EventHandler(this.btnHouseholdRemove_Click);
            // 
            // btnPersonAdd
            // 
            this.btnPersonAdd.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPersonAdd.BackgroundImage")));
            this.btnPersonAdd.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPersonAdd.Location = new System.Drawing.Point(117, 14);
            this.btnPersonAdd.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPersonAdd.Name = "btnPersonAdd";
            this.btnPersonAdd.Size = new System.Drawing.Size(49, 50);
            this.btnPersonAdd.TabIndex = 2;
            this.toolTipButtons.SetToolTip(this.btnPersonAdd, "Add Person to Household");
            this.btnPersonAdd.UseVisualStyleBackColor = true;
            this.btnPersonAdd.Click += new System.EventHandler(this.btnPersonAdd_Click);
            // 
            // btnPersonRemove
            // 
            this.btnPersonRemove.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnPersonRemove.BackgroundImage")));
            this.btnPersonRemove.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnPersonRemove.Location = new System.Drawing.Point(168, 14);
            this.btnPersonRemove.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.btnPersonRemove.Name = "btnPersonRemove";
            this.btnPersonRemove.Size = new System.Drawing.Size(49, 50);
            this.btnPersonRemove.TabIndex = 3;
            this.toolTipButtons.SetToolTip(this.btnPersonRemove, "Delete Person from Household");
            this.btnPersonRemove.UseVisualStyleBackColor = true;
            this.btnPersonRemove.Click += new System.EventHandler(this.btnPersonRemove_Click);
            // 
            // panelLeft
            // 
            this.panelLeft.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLeft.Controls.Add(this.panelLeftBottom);
            this.panelLeft.Controls.Add(this.panelLeftTop);
            this.panelLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeft.Location = new System.Drawing.Point(0, 0);
            this.panelLeft.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelLeft.MinimumSize = new System.Drawing.Size(210, 2);
            this.panelLeft.Name = "panelLeft";
            this.panelLeft.Size = new System.Drawing.Size(240, 630);
            this.panelLeft.TabIndex = 0;
            // 
            // panelLeftBottom
            // 
            this.panelLeftBottom.Controls.Add(this.treeHouseholds);
            this.panelLeftBottom.Controls.Add(this.labelControl2);
            this.panelLeftBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelLeftBottom.Location = new System.Drawing.Point(0, 162);
            this.panelLeftBottom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelLeftBottom.Name = "panelLeftBottom";
            this.panelLeftBottom.Size = new System.Drawing.Size(238, 466);
            this.panelLeftBottom.TabIndex = 2;
            // 
            // treeHouseholds
            // 
            this.treeHouseholds.Appearance.FocusedCell.FontStyleDelta = System.Drawing.FontStyle.Bold;
            this.treeHouseholds.Appearance.FocusedCell.Options.UseFont = true;
            this.treeHouseholds.Appearance.FocusedRow.FontStyleDelta = System.Drawing.FontStyle.Bold;
            this.treeHouseholds.Appearance.FocusedRow.Options.UseFont = true;
            this.treeHouseholds.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeHouseholds.Location = new System.Drawing.Point(0, 0);
            this.treeHouseholds.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.treeHouseholds.Name = "treeHouseholds";
            this.treeHouseholds.OptionsBehavior.AllowCopyToClipboard = false;
            this.treeHouseholds.OptionsBehavior.DragNodes = true;
            this.treeHouseholds.OptionsBehavior.ImmediateEditor = false;
            this.treeHouseholds.OptionsBehavior.KeepSelectedOnClick = false;
            this.treeHouseholds.OptionsBehavior.ResizeNodes = false;
            this.treeHouseholds.OptionsNavigation.MoveOnEdit = false;
            this.treeHouseholds.OptionsView.ShowCheckBoxes = true;
            this.treeHouseholds.OptionsView.ShowColumns = false;
            this.treeHouseholds.OptionsView.ShowHorzLines = false;
            this.treeHouseholds.OptionsView.ShowIndicator = false;
            this.treeHouseholds.OptionsView.ShowVertLines = false;
            this.treeHouseholds.Size = new System.Drawing.Size(238, 406);
            this.treeHouseholds.TabIndex = 2;
            this.treeHouseholds.ToolTipController = this.toolTipControllerGrid;
            this.treeHouseholds.BeforeExpand += new DevExpress.XtraTreeList.BeforeExpandEventHandler(this.treeHouseholds_BeforeExpand);
            this.treeHouseholds.BeforeDragNode += new DevExpress.XtraTreeList.BeforeDragNodeEventHandler(this.treeHouseholds_BeforeDragNode);
            this.treeHouseholds.AfterExpand += new DevExpress.XtraTreeList.NodeEventHandler(this.treeHouseholds_AfterExpand);
            this.treeHouseholds.AfterCollapse += new DevExpress.XtraTreeList.NodeEventHandler(this.treeHouseholds_AfterCollapse);
            this.treeHouseholds.AfterCheckNode += new DevExpress.XtraTreeList.NodeEventHandler(this.treeHouseholds_AfterCheckNode);
            this.treeHouseholds.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeHouseholds_FocusedNodeChanged);
            this.treeHouseholds.CalcNodeDragImageIndex += new DevExpress.XtraTreeList.CalcNodeDragImageIndexEventHandler(this.treeHouseholds_CalcNodeDragImageIndex);
            this.treeHouseholds.ShownEditor += new System.EventHandler(this.treeHouseholds_ShownEditor);
            this.treeHouseholds.PopupMenuShowing += new DevExpress.XtraTreeList.PopupMenuShowingEventHandler(this.treeHouseholds_PopupMenuShowing);
            this.treeHouseholds.CellValueChanged += new DevExpress.XtraTreeList.CellValueChangedEventHandler(this.treeHouseholds_CellValueChanged);
            this.treeHouseholds.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeHouseholds_DragDrop);
            this.treeHouseholds.DragOver += new System.Windows.Forms.DragEventHandler(this.treeHouseholds_DragOver);
            // 
            // toolTipControllerGrid
            // 
            this.toolTipControllerGrid.AutoPopDelay = 50000;
            this.toolTipControllerGrid.InitialDelay = 100;
            // 
            // labelControl2
            // 
            this.labelControl2.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.Vertical;
            this.labelControl2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.labelControl2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.labelControl2.Location = new System.Drawing.Point(0, 406);
            this.labelControl2.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.labelControl2.Name = "labelControl2";
            this.labelControl2.Padding = new System.Windows.Forms.Padding(5);
            this.labelControl2.Size = new System.Drawing.Size(238, 60);
            this.labelControl2.TabIndex = 1;
            this.labelControl2.Text = "Please check the families and people that you wish to generate the Household Data" +
    " for.";
            // 
            // panelLeftTop
            // 
            this.panelLeftTop.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.panelLeftTop.Controls.Add(this.resetAllAdvButton);
            this.panelLeftTop.Controls.Add(this.chkSelectAll);
            this.panelLeftTop.Controls.Add(this.btnPersonRemove);
            this.panelLeftTop.Controls.Add(this.btnPersonAdd);
            this.panelLeftTop.Controls.Add(this.btnHouseholdRemove);
            this.panelLeftTop.Controls.Add(this.btnHouseholdAdd);
            this.panelLeftTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelLeftTop.Location = new System.Drawing.Point(0, 0);
            this.panelLeftTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelLeftTop.Name = "panelLeftTop";
            this.panelLeftTop.Size = new System.Drawing.Size(238, 162);
            this.panelLeftTop.TabIndex = 1;
            // 
            // resetAllAdvButton
            // 
            this.resetAllAdvButton.Font = new System.Drawing.Font("Tahoma", 7F);
            this.resetAllAdvButton.Location = new System.Drawing.Point(15, 98);
            this.resetAllAdvButton.Name = "resetAllAdvButton";
            this.resetAllAdvButton.Size = new System.Drawing.Size(202, 47);
            this.resetAllAdvButton.TabIndex = 5;
            this.resetAllAdvButton.Text = "Reset selected Household\'s Advanced Variables";
            this.resetAllAdvButton.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.resetAllAdvButton.UseVisualStyleBackColor = true;
            this.resetAllAdvButton.Click += new System.EventHandler(this.resetAllAdvButton_Click);
            // 
            // chkSelectAll
            // 
            this.chkSelectAll.AutoSize = true;
            this.chkSelectAll.Location = new System.Drawing.Point(15, 70);
            this.chkSelectAll.Margin = new System.Windows.Forms.Padding(4);
            this.chkSelectAll.Name = "chkSelectAll";
            this.chkSelectAll.Size = new System.Drawing.Size(210, 21);
            this.chkSelectAll.TabIndex = 4;
            this.chkSelectAll.Text = "Select/Unselect all households";
            this.chkSelectAll.UseVisualStyleBackColor = true;
            this.chkSelectAll.CheckedChanged += new System.EventHandler(this.chkSelectAll_CheckedChanged);
            // 
            // mainMenuStrip
            // 
            this.mainMenuStrip.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.mainMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectStripMenuItem,
            this.viewToolStripMenuItem,
            this.wizardToolStripMenuItem,
            this.advancedOptionsToolStripMenuItem,
            this.statisticsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.mainMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.mainMenuStrip.Name = "mainMenuStrip";
            this.mainMenuStrip.Padding = new System.Windows.Forms.Padding(5, 2, 0, 2);
            this.mainMenuStrip.Size = new System.Drawing.Size(1299, 38);
            this.mainMenuStrip.TabIndex = 5;
            this.mainMenuStrip.Text = "menuStrip1";
            // 
            // projectStripMenuItem
            // 
            this.projectStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.openProjectToolStripMenuItem,
            this.saveProjectToolStripMenuItem,
            this.saveAsProjectToolStripMenuItem,
            this.SaveAsPublicToolStripMenuItem,
            this.toolStripSeparator7,
            this.ExitToolStripMenuItem});
            this.projectStripMenuItem.Name = "projectStripMenuItem";
            this.projectStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.projectStripMenuItem.Size = new System.Drawing.Size(69, 34);
            this.projectStripMenuItem.Text = "Project";
            this.projectStripMenuItem.Click += new System.EventHandler(this.projectStripMenuItem_Click);
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.newToolStripMenuItem.Text = "New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // openProjectToolStripMenuItem
            // 
            this.openProjectToolStripMenuItem.Name = "openProjectToolStripMenuItem";
            this.openProjectToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.openProjectToolStripMenuItem.Text = "Open";
            this.openProjectToolStripMenuItem.Click += new System.EventHandler(this.openProjectToolStripMenuItem_Click);
            // 
            // saveProjectToolStripMenuItem
            // 
            this.saveProjectToolStripMenuItem.Name = "saveProjectToolStripMenuItem";
            this.saveProjectToolStripMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.saveProjectToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.saveProjectToolStripMenuItem.Text = "Save";
            this.saveProjectToolStripMenuItem.Click += new System.EventHandler(this.saveProjectToolStripMenuItem_Click);
            // 
            // saveAsProjectToolStripMenuItem
            // 
            this.saveAsProjectToolStripMenuItem.Name = "saveAsProjectToolStripMenuItem";
            this.saveAsProjectToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.saveAsProjectToolStripMenuItem.Text = "Save As";
            this.saveAsProjectToolStripMenuItem.Click += new System.EventHandler(this.saveAsProjectToolStripMenuItem_Click);
            // 
            // SaveAsPublicToolStripMenuItem
            // 
            this.SaveAsPublicToolStripMenuItem.Name = "SaveAsPublicToolStripMenuItem";
            this.SaveAsPublicToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.SaveAsPublicToolStripMenuItem.Text = "Save As Public";
            this.SaveAsPublicToolStripMenuItem.Click += new System.EventHandler(this.SaveAsPublicToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(221, 6);
            // 
            // ExitToolStripMenuItem
            // 
            this.ExitToolStripMenuItem.Name = "ExitToolStripMenuItem";
            this.ExitToolStripMenuItem.Size = new System.Drawing.Size(224, 26);
            this.ExitToolStripMenuItem.Text = "Exit";
            this.ExitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showAdvancedVariablesToolStripMenuItem,
            this.showDerivedVariablesToolStripMenuItem,
            this.toolStripSeparator3,
            this.showOnlyChangedValuesToolStripMenuItem,
            this.highlightChangedValuesToolStripMenuItem});
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(55, 34);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // showAdvancedVariablesToolStripMenuItem
            // 
            this.showAdvancedVariablesToolStripMenuItem.Name = "showAdvancedVariablesToolStripMenuItem";
            this.showAdvancedVariablesToolStripMenuItem.Size = new System.Drawing.Size(266, 26);
            this.showAdvancedVariablesToolStripMenuItem.Text = "Show Advanced Variables";
            this.showAdvancedVariablesToolStripMenuItem.Click += new System.EventHandler(this.showAdvancedVariablesToolStripMenuItem_Click);
            // 
            // showDerivedVariablesToolStripMenuItem
            // 
            this.showDerivedVariablesToolStripMenuItem.Name = "showDerivedVariablesToolStripMenuItem";
            this.showDerivedVariablesToolStripMenuItem.Size = new System.Drawing.Size(266, 26);
            this.showDerivedVariablesToolStripMenuItem.Text = "Show Derived Variables";
            this.showDerivedVariablesToolStripMenuItem.Click += new System.EventHandler(this.showDerivedVariablesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(263, 6);
            // 
            // showOnlyChangedValuesToolStripMenuItem
            // 
            this.showOnlyChangedValuesToolStripMenuItem.Name = "showOnlyChangedValuesToolStripMenuItem";
            this.showOnlyChangedValuesToolStripMenuItem.Size = new System.Drawing.Size(266, 26);
            this.showOnlyChangedValuesToolStripMenuItem.Text = "Show only changed values";
            this.showOnlyChangedValuesToolStripMenuItem.Click += new System.EventHandler(this.showOnlyChangedValuesToolStripMenuItem_Click);
            // 
            // highlightChangedValuesToolStripMenuItem
            // 
            this.highlightChangedValuesToolStripMenuItem.Name = "highlightChangedValuesToolStripMenuItem";
            this.highlightChangedValuesToolStripMenuItem.Size = new System.Drawing.Size(266, 26);
            this.highlightChangedValuesToolStripMenuItem.Text = "Highlight changed values";
            this.highlightChangedValuesToolStripMenuItem.Click += new System.EventHandler(this.highlightChangedValuesToolStripMenuItem_Click);
            // 
            // wizardToolStripMenuItem
            // 
            this.wizardToolStripMenuItem.Name = "wizardToolStripMenuItem";
            this.wizardToolStripMenuItem.Size = new System.Drawing.Size(70, 34);
            this.wizardToolStripMenuItem.Text = "Wizard";
            this.wizardToolStripMenuItem.Click += new System.EventHandler(this.wizardToolStripMenuItem_Click);
            // 
            // advancedOptionsToolStripMenuItem
            // 
            this.advancedOptionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importToolStripMenuItem,
            this.manageVariablesToolStripMenuItem});
            this.advancedOptionsToolStripMenuItem.Name = "advancedOptionsToolStripMenuItem";
            this.advancedOptionsToolStripMenuItem.Size = new System.Drawing.Size(145, 34);
            this.advancedOptionsToolStripMenuItem.Text = "Advanced Options";
            // 
            // importToolStripMenuItem
            // 
            this.importToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.importHHToolStripMenuItem,
            this.importVariablesToolStripMenuItem,
            this.importRefTabToolStripMenuItem});
            this.importToolStripMenuItem.Name = "importToolStripMenuItem";
            this.importToolStripMenuItem.Size = new System.Drawing.Size(212, 26);
            this.importToolStripMenuItem.Text = "Import";
            // 
            // importHHToolStripMenuItem
            // 
            this.importHHToolStripMenuItem.Name = "importHHToolStripMenuItem";
            this.importHHToolStripMenuItem.Size = new System.Drawing.Size(252, 26);
            this.importHHToolStripMenuItem.Text = "Import Households";
            this.importHHToolStripMenuItem.Click += new System.EventHandler(this.importHHToolStripMenuItem_Click);
            // 
            // importVariablesToolStripMenuItem
            // 
            this.importVariablesToolStripMenuItem.Name = "importVariablesToolStripMenuItem";
            this.importVariablesToolStripMenuItem.Size = new System.Drawing.Size(252, 26);
            this.importVariablesToolStripMenuItem.Text = "Import Variables";
            this.importVariablesToolStripMenuItem.Click += new System.EventHandler(this.importVariablesToolStripMenuItem_Click);
            // 
            // importRefTabToolStripMenuItem
            // 
            this.importRefTabToolStripMenuItem.Name = "importRefTabToolStripMenuItem";
            this.importRefTabToolStripMenuItem.Size = new System.Drawing.Size(252, 26);
            this.importRefTabToolStripMenuItem.Text = "Import Reference Tables";
            this.importRefTabToolStripMenuItem.Click += new System.EventHandler(this.importRefTabToolStripMenuItem_Click);
            // 
            // manageVariablesToolStripMenuItem
            // 
            this.manageVariablesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageCategoriesToolStripMenuItem,
            this.manageCountriesToolStripMenuItem,
            this.manageYearsToolStripMenuItem,
            this.toolStripSeparator2,
            this.manageBasicVariablesToolStripMenuItem,
            this.manageAdvancedVariablesToolStripMenuItem,
            this.manageAdvancedCountryspecificVariablesToolStripMenuItem,
            this.manageDerivedVariablesToolStripMenuItem,
            this.toolStripSeparator4,
            this.manageReferenceTablesToolStripMenuItem,
            this.saveCurrentSettingsAsDefaultToolStripMenuItem});
            this.manageVariablesToolStripMenuItem.Name = "manageVariablesToolStripMenuItem";
            this.manageVariablesToolStripMenuItem.Size = new System.Drawing.Size(212, 26);
            this.manageVariablesToolStripMenuItem.Text = "Manage Settings...";
            // 
            // manageCategoriesToolStripMenuItem
            // 
            this.manageCategoriesToolStripMenuItem.Name = "manageCategoriesToolStripMenuItem";
            this.manageCategoriesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageCategoriesToolStripMenuItem.Text = "Manage Categories";
            this.manageCategoriesToolStripMenuItem.Click += new System.EventHandler(this.manageCategoriesToolStripMenuItem_Click);
            // 
            // manageCountriesToolStripMenuItem
            // 
            this.manageCountriesToolStripMenuItem.Name = "manageCountriesToolStripMenuItem";
            this.manageCountriesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageCountriesToolStripMenuItem.Text = "Manage Countries";
            this.manageCountriesToolStripMenuItem.Click += new System.EventHandler(this.manageCountriesToolStripMenuItem_Click);
            // 
            // manageYearsToolStripMenuItem
            // 
            this.manageYearsToolStripMenuItem.Name = "manageYearsToolStripMenuItem";
            this.manageYearsToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageYearsToolStripMenuItem.Text = "Manage Years";
            this.manageYearsToolStripMenuItem.Click += new System.EventHandler(this.manageYearsToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(388, 6);
            // 
            // manageBasicVariablesToolStripMenuItem
            // 
            this.manageBasicVariablesToolStripMenuItem.Name = "manageBasicVariablesToolStripMenuItem";
            this.manageBasicVariablesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageBasicVariablesToolStripMenuItem.Text = "Manage Basic Variables";
            this.manageBasicVariablesToolStripMenuItem.Click += new System.EventHandler(this.manageBasicVariablesToolStripMenuItem_Click);
            // 
            // manageAdvancedVariablesToolStripMenuItem
            // 
            this.manageAdvancedVariablesToolStripMenuItem.Name = "manageAdvancedVariablesToolStripMenuItem";
            this.manageAdvancedVariablesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageAdvancedVariablesToolStripMenuItem.Text = "Manage Advanced Variables";
            this.manageAdvancedVariablesToolStripMenuItem.Click += new System.EventHandler(this.manageAdvancedVariablesToolStripMenuItem_Click);
            // 
            // manageAdvancedCountryspecificVariablesToolStripMenuItem
            // 
            this.manageAdvancedCountryspecificVariablesToolStripMenuItem.Name = "manageAdvancedCountryspecificVariablesToolStripMenuItem";
            this.manageAdvancedCountryspecificVariablesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageAdvancedCountryspecificVariablesToolStripMenuItem.Text = "Manage Advanced Country-specific Variables";
            this.manageAdvancedCountryspecificVariablesToolStripMenuItem.Click += new System.EventHandler(this.manageAdvancedCountryspecificVariablesToolStripMenuItem_Click);
            // 
            // manageDerivedVariablesToolStripMenuItem
            // 
            this.manageDerivedVariablesToolStripMenuItem.Name = "manageDerivedVariablesToolStripMenuItem";
            this.manageDerivedVariablesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageDerivedVariablesToolStripMenuItem.Text = "Manage Derived Variables";
            this.manageDerivedVariablesToolStripMenuItem.Click += new System.EventHandler(this.manageDerivedVariablesToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(388, 6);
            // 
            // manageReferenceTablesToolStripMenuItem
            // 
            this.manageReferenceTablesToolStripMenuItem.Name = "manageReferenceTablesToolStripMenuItem";
            this.manageReferenceTablesToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.manageReferenceTablesToolStripMenuItem.Text = "Manage Reference Tables";
            this.manageReferenceTablesToolStripMenuItem.Click += new System.EventHandler(this.manageReferenceTablesToolStripMenuItem_Click);
            // 
            // saveCurrentSettingsAsDefaultToolStripMenuItem
            // 
            this.saveCurrentSettingsAsDefaultToolStripMenuItem.Name = "saveCurrentSettingsAsDefaultToolStripMenuItem";
            this.saveCurrentSettingsAsDefaultToolStripMenuItem.Size = new System.Drawing.Size(391, 26);
            this.saveCurrentSettingsAsDefaultToolStripMenuItem.Text = "Save Settings as Default";
            this.saveCurrentSettingsAsDefaultToolStripMenuItem.Click += new System.EventHandler(this.saveCurrentSettingsAsDefaultToolStripMenuItem_Click);
            // 
            // statisticsToolStripMenuItem
            // 
            this.statisticsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.breakDownTypesToolStripMenuItem,
            this.breakDownCountryYearToolStripMenuItem,
            this.budgetConstraintsToolStripMenuItem});
            this.statisticsToolStripMenuItem.Name = "statisticsToolStripMenuItem";
            this.statisticsToolStripMenuItem.Size = new System.Drawing.Size(81, 34);
            this.statisticsToolStripMenuItem.Text = "Statistics";
            // 
            // breakDownTypesToolStripMenuItem
            // 
            this.breakDownTypesToolStripMenuItem.Name = "breakDownTypesToolStripMenuItem";
            this.breakDownTypesToolStripMenuItem.Size = new System.Drawing.Size(309, 26);
            this.breakDownTypesToolStripMenuItem.Text = "Break Down per Household Type";
            this.breakDownTypesToolStripMenuItem.Click += new System.EventHandler(this.breakDownTypesToolStripMenuItem_Click);
            // 
            // breakDownCountryYearToolStripMenuItem
            // 
            this.breakDownCountryYearToolStripMenuItem.Name = "breakDownCountryYearToolStripMenuItem";
            this.breakDownCountryYearToolStripMenuItem.Size = new System.Drawing.Size(309, 26);
            this.breakDownCountryYearToolStripMenuItem.Text = "Break Down per Country/Year";
            this.breakDownCountryYearToolStripMenuItem.Click += new System.EventHandler(this.breakDownCountryYearToolStripMenuItem_Click);
            // 
            // budgetConstraintsToolStripMenuItem
            // 
            this.budgetConstraintsToolStripMenuItem.Name = "budgetConstraintsToolStripMenuItem";
            this.budgetConstraintsToolStripMenuItem.Size = new System.Drawing.Size(309, 26);
            this.budgetConstraintsToolStripMenuItem.Text = "Budget Constraints";
            this.budgetConstraintsToolStripMenuItem.Click += new System.EventHandler(this.budgetConstraintsToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(55, 34);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(133, 26);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // panelRightTop
            // 
            this.panelRightTop.BackColor = System.Drawing.SystemColors.Control;
            this.panelRightTop.Controls.Add(this.separatorLine);
            this.panelRightTop.Controls.Add(this.panelYears);
            this.panelRightTop.Controls.Add(this.panelCountries);
            this.panelRightTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelRightTop.Location = new System.Drawing.Point(0, 0);
            this.panelRightTop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelRightTop.Name = "panelRightTop";
            this.panelRightTop.Size = new System.Drawing.Size(1044, 103);
            this.panelRightTop.TabIndex = 11;
            // 
            // separatorLine
            // 
            this.separatorLine.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.separatorLine.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.separatorLine.Location = new System.Drawing.Point(0, 102);
            this.separatorLine.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.separatorLine.Name = "separatorLine";
            this.separatorLine.Size = new System.Drawing.Size(1044, 1);
            this.separatorLine.TabIndex = 9;
            // 
            // panelYears
            // 
            this.panelYears.Controls.Add(this.labelControl5);
            this.panelYears.Controls.Add(this.labelSelectedYears);
            this.panelYears.Controls.Add(this.yearsCheckedComboBoxEdit);
            this.panelYears.Location = new System.Drawing.Point(476, 0);
            this.panelYears.Margin = new System.Windows.Forms.Padding(4);
            this.panelYears.Name = "panelYears";
            this.panelYears.Size = new System.Drawing.Size(475, 103);
            this.panelYears.TabIndex = 8;
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelControl5.Location = new System.Drawing.Point(16, 16);
            this.labelControl5.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(43, 17);
            this.labelControl5.TabIndex = 5;
            this.labelControl5.Text = "Years:";
            // 
            // labelSelectedYears
            // 
            this.labelSelectedYears.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelSelectedYears.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelSelectedYears.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelSelectedYears.Location = new System.Drawing.Point(16, 44);
            this.labelSelectedYears.Margin = new System.Windows.Forms.Padding(4);
            this.labelSelectedYears.Name = "labelSelectedYears";
            this.labelSelectedYears.Size = new System.Drawing.Size(448, 55);
            this.labelSelectedYears.TabIndex = 6;
            this.labelSelectedYears.Text = "No years selected!";
            // 
            // yearsCheckedComboBoxEdit
            // 
            this.yearsCheckedComboBoxEdit.Location = new System.Drawing.Point(71, 12);
            this.yearsCheckedComboBoxEdit.Margin = new System.Windows.Forms.Padding(4);
            this.yearsCheckedComboBoxEdit.Name = "yearsCheckedComboBoxEdit";
            this.yearsCheckedComboBoxEdit.Properties.AllowMultiSelect = true;
            this.yearsCheckedComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.yearsCheckedComboBoxEdit.Properties.DisplayMember = "Year";
            this.yearsCheckedComboBoxEdit.Properties.ValueMember = "Year";
            this.yearsCheckedComboBoxEdit.Size = new System.Drawing.Size(151, 22);
            this.yearsCheckedComboBoxEdit.TabIndex = 4;
            this.yearsCheckedComboBoxEdit.EditValueChanged += new System.EventHandler(this.yearsCheckedComboBoxEdit_EditValueChanged);
            // 
            // panelCountries
            // 
            this.panelCountries.Controls.Add(this.labelControl1);
            this.panelCountries.Controls.Add(this.countriesCheckedComboBoxEdit);
            this.panelCountries.Controls.Add(this.labelSelectedCountries);
            this.panelCountries.Location = new System.Drawing.Point(0, 0);
            this.panelCountries.Margin = new System.Windows.Forms.Padding(4);
            this.panelCountries.Name = "panelCountries";
            this.panelCountries.Size = new System.Drawing.Size(475, 103);
            this.panelCountries.TabIndex = 7;
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelControl1.Location = new System.Drawing.Point(19, 16);
            this.labelControl1.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(72, 17);
            this.labelControl1.TabIndex = 2;
            this.labelControl1.Text = "Countries:";
            // 
            // countriesCheckedComboBoxEdit
            // 
            this.countriesCheckedComboBoxEdit.Location = new System.Drawing.Point(103, 12);
            this.countriesCheckedComboBoxEdit.Margin = new System.Windows.Forms.Padding(4);
            this.countriesCheckedComboBoxEdit.Name = "countriesCheckedComboBoxEdit";
            this.countriesCheckedComboBoxEdit.Properties.AllowMultiSelect = true;
            this.countriesCheckedComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.countriesCheckedComboBoxEdit.Properties.DisplayMember = "Country";
            this.countriesCheckedComboBoxEdit.Properties.ValueMember = "Country";
            this.countriesCheckedComboBoxEdit.Size = new System.Drawing.Size(151, 22);
            this.countriesCheckedComboBoxEdit.TabIndex = 1;
            this.countriesCheckedComboBoxEdit.EditValueChanged += new System.EventHandler(this.countriesCheckedComboBoxEdit_EditValueChanged);
            // 
            // labelSelectedCountries
            // 
            this.labelSelectedCountries.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelSelectedCountries.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelSelectedCountries.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelSelectedCountries.Location = new System.Drawing.Point(19, 44);
            this.labelSelectedCountries.Margin = new System.Windows.Forms.Padding(4);
            this.labelSelectedCountries.Name = "labelSelectedCountries";
            this.labelSelectedCountries.Size = new System.Drawing.Size(439, 55);
            this.labelSelectedCountries.TabIndex = 3;
            this.labelSelectedCountries.Text = "No countries selected!";
            // 
            // labelControl3
            // 
            this.labelControl3.Location = new System.Drawing.Point(15, 12);
            this.labelControl3.Margin = new System.Windows.Forms.Padding(4);
            this.labelControl3.Name = "labelControl3";
            this.labelControl3.Size = new System.Drawing.Size(85, 16);
            this.labelControl3.TabIndex = 2;
            this.labelControl3.Text = "Variable Filter:";
            // 
            // txtQuickFilter
            // 
            this.txtQuickFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtQuickFilter.Location = new System.Drawing.Point(115, 9);
            this.txtQuickFilter.Margin = new System.Windows.Forms.Padding(4);
            this.txtQuickFilter.Name = "txtQuickFilter";
            this.txtQuickFilter.Size = new System.Drawing.Size(915, 22);
            this.txtQuickFilter.TabIndex = 1;
            this.txtQuickFilter.EditValueChanged += new System.EventHandler(this.txtQuickFilter_EditValueChanged);
            // 
            // panelRightBottom
            // 
            this.panelRightBottom.Controls.Add(this.gridHousehold);
            this.panelRightBottom.Controls.Add(this.panelFilter);
            this.panelRightBottom.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelRightBottom.Location = new System.Drawing.Point(0, 103);
            this.panelRightBottom.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.panelRightBottom.Name = "panelRightBottom";
            this.panelRightBottom.Size = new System.Drawing.Size(1044, 523);
            this.panelRightBottom.TabIndex = 10;
            // 
            // gridHousehold
            // 
            this.gridHousehold.Appearance.Category.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(226)))), ((int)(((byte)(236)))), ((int)(((byte)(230)))));
            this.gridHousehold.Appearance.Category.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(197)))), ((int)(((byte)(180)))));
            this.gridHousehold.Appearance.Category.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
            this.gridHousehold.Appearance.Category.ForeColor = System.Drawing.Color.Black;
            this.gridHousehold.Appearance.Category.Options.UseBackColor = true;
            this.gridHousehold.Appearance.Category.Options.UseBorderColor = true;
            this.gridHousehold.Appearance.Category.Options.UseFont = true;
            this.gridHousehold.Appearance.Category.Options.UseForeColor = true;
            this.gridHousehold.Appearance.CategoryExpandButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(209)))), ((int)(((byte)(188)))));
            this.gridHousehold.Appearance.CategoryExpandButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(178)))), ((int)(((byte)(209)))), ((int)(((byte)(188)))));
            this.gridHousehold.Appearance.CategoryExpandButton.Options.UseBackColor = true;
            this.gridHousehold.Appearance.CategoryExpandButton.Options.UseBorderColor = true;
            this.gridHousehold.Appearance.Empty.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(244)))), ((int)(((byte)(236)))));
            this.gridHousehold.Appearance.Empty.BackColor2 = System.Drawing.Color.White;
            this.gridHousehold.Appearance.Empty.Options.UseBackColor = true;
            this.gridHousehold.Appearance.ExpandButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(220)))), ((int)(((byte)(204)))));
            this.gridHousehold.Appearance.ExpandButton.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(197)))), ((int)(((byte)(220)))), ((int)(((byte)(204)))));
            this.gridHousehold.Appearance.ExpandButton.Options.UseBackColor = true;
            this.gridHousehold.Appearance.ExpandButton.Options.UseBorderColor = true;
            this.gridHousehold.Appearance.FocusedCell.BackColor = System.Drawing.Color.White;
            this.gridHousehold.Appearance.FocusedCell.ForeColor = System.Drawing.Color.Black;
            this.gridHousehold.Appearance.FocusedCell.Options.UseBackColor = true;
            this.gridHousehold.Appearance.FocusedCell.Options.UseForeColor = true;
            this.gridHousehold.Appearance.FocusedRecord.BackColor = System.Drawing.Color.White;
            this.gridHousehold.Appearance.FocusedRecord.Options.UseBackColor = true;
            this.gridHousehold.Appearance.FocusedRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(129)))), ((int)(((byte)(171)))), ((int)(((byte)(177)))));
            this.gridHousehold.Appearance.FocusedRow.ForeColor = System.Drawing.Color.White;
            this.gridHousehold.Appearance.FocusedRow.Options.UseBackColor = true;
            this.gridHousehold.Appearance.FocusedRow.Options.UseForeColor = true;
            this.gridHousehold.Appearance.HideSelectionRow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(186)))), ((int)(((byte)(211)))), ((int)(((byte)(215)))));
            this.gridHousehold.Appearance.HideSelectionRow.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(104)))), ((int)(((byte)(130)))), ((int)(((byte)(134)))));
            this.gridHousehold.Appearance.HideSelectionRow.Options.UseBackColor = true;
            this.gridHousehold.Appearance.HideSelectionRow.Options.UseForeColor = true;
            this.gridHousehold.Appearance.HorzLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(197)))), ((int)(((byte)(180)))));
            this.gridHousehold.Appearance.HorzLine.Options.UseBackColor = true;
            this.gridHousehold.Appearance.RecordValue.BackColor = System.Drawing.Color.White;
            this.gridHousehold.Appearance.RecordValue.ForeColor = System.Drawing.Color.Black;
            this.gridHousehold.Appearance.RecordValue.Options.UseBackColor = true;
            this.gridHousehold.Appearance.RecordValue.Options.UseForeColor = true;
            this.gridHousehold.Appearance.RowHeaderPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(244)))), ((int)(((byte)(242)))));
            this.gridHousehold.Appearance.RowHeaderPanel.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(215)))), ((int)(((byte)(234)))), ((int)(((byte)(221)))));
            this.gridHousehold.Appearance.RowHeaderPanel.ForeColor = System.Drawing.Color.Black;
            this.gridHousehold.Appearance.RowHeaderPanel.Options.UseBackColor = true;
            this.gridHousehold.Appearance.RowHeaderPanel.Options.UseBorderColor = true;
            this.gridHousehold.Appearance.RowHeaderPanel.Options.UseForeColor = true;
            this.gridHousehold.Appearance.VertLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(172)))), ((int)(((byte)(197)))), ((int)(((byte)(180)))));
            this.gridHousehold.Appearance.VertLine.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(117)))), ((int)(((byte)(136)))), ((int)(((byte)(122)))));
            this.gridHousehold.Appearance.VertLine.Options.UseBackColor = true;
            this.gridHousehold.Appearance.VertLine.Options.UseBorderColor = true;
            this.gridHousehold.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridHousehold.Location = new System.Drawing.Point(0, 46);
            this.gridHousehold.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.gridHousehold.Name = "gridHousehold";
            this.gridHousehold.OptionsBehavior.ResizeRowHeaders = false;
            this.gridHousehold.OptionsBehavior.ShowEditorOnMouseUp = true;
            this.gridHousehold.OptionsView.LevelIndent = 0;
            this.gridHousehold.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.editorInteger,
            this.editorFloat,
            this.repositoryItemPopupContainerEditNumeric});
            this.gridHousehold.RowHeaderWidth = 150;
            this.gridHousehold.Size = new System.Drawing.Size(1044, 477);
            this.gridHousehold.TabIndex = 0;
            this.gridHousehold.ToolTipController = this.toolTipControllerGrid;
            this.gridHousehold.CellValueChanged += new DevExpress.XtraVerticalGrid.Events.CellValueChangedEventHandler(this.gridHousehold_CellValueChanged);
            // 
            // editorInteger
            // 
            this.editorInteger.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.editorInteger.IsFloatValue = false;
            this.editorInteger.Mask.EditMask = "N00";
            this.editorInteger.Name = "editorInteger";
            // 
            // editorFloat
            // 
            this.editorFloat.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.editorFloat.DisplayFormat.FormatString = "F2";
            this.editorFloat.DisplayFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.editorFloat.EditFormat.FormatString = "F2";
            this.editorFloat.EditFormat.FormatType = DevExpress.Utils.FormatType.Numeric;
            this.editorFloat.Mask.EditMask = "#0.00";
            this.editorFloat.Name = "editorFloat";
            // 
            // repositoryItemPopupContainerEditNumeric
            // 
            this.repositoryItemPopupContainerEditNumeric.AutoHeight = false;
            this.repositoryItemPopupContainerEditNumeric.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemPopupContainerEditNumeric.Name = "repositoryItemPopupContainerEditNumeric";
            this.repositoryItemPopupContainerEditNumeric.PopupFormSize = new System.Drawing.Size(330, 175);
            this.repositoryItemPopupContainerEditNumeric.PopupSizeable = false;
            // 
            // panelFilter
            // 
            this.panelFilter.Controls.Add(this.txtQuickFilter);
            this.panelFilter.Controls.Add(this.labelControl3);
            this.panelFilter.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFilter.Location = new System.Drawing.Point(0, 0);
            this.panelFilter.Margin = new System.Windows.Forms.Padding(4);
            this.panelFilter.Name = "panelFilter";
            this.panelFilter.Size = new System.Drawing.Size(1044, 46);
            this.panelFilter.TabIndex = 8;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.mainMenuStrip);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Margin = new System.Windows.Forms.Padding(4);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(1299, 38);
            this.panelTop.TabIndex = 5;
            // 
            // splitMainInterface
            // 
            this.splitMainInterface.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitMainInterface.Location = new System.Drawing.Point(0, 38);
            this.splitMainInterface.Margin = new System.Windows.Forms.Padding(4);
            this.splitMainInterface.Name = "splitMainInterface";
            this.splitMainInterface.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.splitMainInterface.Panel1.Controls.Add(this.panelLeft);
            this.splitMainInterface.Panel1.MinSize = 180;
            this.splitMainInterface.Panel1.Text = "Panel1";
            this.splitMainInterface.Panel2.BorderStyle = DevExpress.XtraEditors.Controls.BorderStyles.Simple;
            this.splitMainInterface.Panel2.Controls.Add(this.panelRightBottom);
            this.splitMainInterface.Panel2.Controls.Add(this.panelRightTop);
            this.splitMainInterface.Panel2.MinSize = 200;
            this.splitMainInterface.Panel2.Text = "Panel2";
            this.splitMainInterface.Size = new System.Drawing.Size(1299, 630);
            this.splitMainInterface.SplitterPosition = 240;
            this.splitMainInterface.TabIndex = 6;
            this.splitMainInterface.SplitterMoved += new System.EventHandler(this.splitMainInterface_SplitterMoved);
            // 
            // InputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1299, 757);
            this.Controls.Add(this.splitMainInterface);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelTop);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.MinimumSize = new System.Drawing.Size(1314, 415);
            this.Name = "InputForm";
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Hypothetical Household Tool";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InputForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InputForm_FormClosed);
            this.Load += new System.EventHandler(this.InputForm_Load);
            this.Shown += new System.EventHandler(this.InputForm_Shown);
            this.panelBottom.ResumeLayout(false);
            this.panelLeft.ResumeLayout(false);
            this.panelLeftBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeHouseholds)).EndInit();
            this.panelLeftTop.ResumeLayout(false);
            this.panelLeftTop.PerformLayout();
            this.mainMenuStrip.ResumeLayout(false);
            this.mainMenuStrip.PerformLayout();
            this.panelRightTop.ResumeLayout(false);
            this.panelYears.ResumeLayout(false);
            this.panelYears.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.yearsCheckedComboBoxEdit.Properties)).EndInit();
            this.panelCountries.ResumeLayout(false);
            this.panelCountries.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countriesCheckedComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtQuickFilter.Properties)).EndInit();
            this.panelRightBottom.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridHousehold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.editorInteger)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.editorFloat)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemPopupContainerEditNumeric)).EndInit();
            this.panelFilter.ResumeLayout(false);
            this.panelFilter.PerformLayout();
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitMainInterface)).EndInit();
            this.splitMainInterface.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panelBottom;
        private DevExpress.XtraEditors.SimpleButton generateButton;
        private System.Windows.Forms.ToolTip toolTipButtons;
        private System.Windows.Forms.Panel panelLeft;
        private System.Windows.Forms.Panel panelLeftBottom;
        private System.Windows.Forms.Panel panelLeftTop;
        private System.Windows.Forms.Button btnPersonRemove;
        private System.Windows.Forms.Button btnPersonAdd;
        private System.Windows.Forms.Button btnHouseholdRemove;
        private System.Windows.Forms.Button btnHouseholdAdd;
        private System.Windows.Forms.Panel panelRightTop;
        private System.Windows.Forms.Panel panelRightBottom;
        private DevExpress.XtraVerticalGrid.VGridControl gridHousehold;
        private DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit editorInteger;
        private DevExpress.XtraEditors.Repository.RepositoryItemSpinEdit editorFloat;
        private DevExpress.XtraEditors.LabelControl labelControl2;
        private System.Windows.Forms.MenuStrip mainMenuStrip;
        private DevExpress.XtraEditors.LabelControl labelControl3;
        private DevExpress.XtraEditors.TextEdit txtQuickFilter;
        private System.Windows.Forms.ToolStripMenuItem advancedOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showAdvancedVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageVariablesToolStripMenuItem;
        private DevExpress.XtraEditors.LabelControl labelSelectedCountries;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        private DevExpress.XtraEditors.LabelControl labelSelectedYears;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.ToolStripMenuItem manageCategoriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageCountriesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageYearsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem manageBasicVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageAdvancedVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageAdvancedCountryspecificVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem showOnlyChangedValuesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem highlightChangedValuesToolStripMenuItem;
        private DevExpress.XtraEditors.Repository.RepositoryItemPopupContainerEdit repositoryItemPopupContainerEditNumeric;
        private DevExpress.Utils.ToolTipController toolTipControllerGrid;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem manageReferenceTablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageDerivedVariablesToolStripMenuItem;
        internal DevExpress.XtraTreeList.TreeList treeHouseholds;
        internal DevExpress.XtraEditors.CheckedComboBoxEdit countriesCheckedComboBoxEdit;
        internal DevExpress.XtraEditors.CheckedComboBoxEdit yearsCheckedComboBoxEdit;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private DevExpress.XtraEditors.SplitContainerControl splitMainInterface;
        private System.Windows.Forms.Panel panelFilter;
        private System.Windows.Forms.Panel panelCountries;
        private System.Windows.Forms.Panel panelYears;
        private System.Windows.Forms.Label separatorLine;
        private System.Windows.Forms.CheckBox chkSelectAll;
        private System.Windows.Forms.ToolStripMenuItem projectStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsProjectToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem ExitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importHHToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importRefTabToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem SaveAsPublicToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem showDerivedVariablesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem statisticsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem breakDownTypesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem breakDownCountryYearToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem budgetConstraintsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveCurrentSettingsAsDefaultToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem wizardToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importToolStripMenuItem;
        private System.Windows.Forms.Button resetAllAdvButton;
        protected DevExpress.XtraBars.BarManager barManager;
    }
}