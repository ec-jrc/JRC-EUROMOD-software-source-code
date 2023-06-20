namespace EM_Statistics.InDepthAnalysis
{
    partial class InDepthAnalysisForm
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
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InDepthAnalysisForm));
            this.btnRun = new System.Windows.Forms.Button();
            this.gridBaselines = new System.Windows.Forms.DataGridView();
            this.colBaselineExists = new System.Windows.Forms.DataGridViewImageColumn();
            this.colBaselines = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBaselineLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gridReforms = new System.Windows.Forms.DataGridView();
            this.colReformsExists = new System.Windows.Forms.DataGridViewImageColumn();
            this.colReforms = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReformsLabel = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.treeCategories = new EM_Common_Win.CustomControls.NoDoubleClickTreeView();
            this.mainMenu = new System.Windows.Forms.MenuStrip();
            this.miFile = new System.Windows.Forms.ToolStripMenuItem();
            this.newToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miLoadSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miSaveSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.miFileSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.miExit = new System.Windows.Forms.ToolStripMenuItem();
            this.miSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miFiscalSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miDistributionalSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miInequalityPovertySettings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.miResetSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpMain = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.miHelpAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSelectBaselinesAndReforms = new System.Windows.Forms.Button();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.btnSelPathEuromod = new System.Windows.Forms.Button();
            this.txtPathEuromodFiles = new System.Windows.Forms.TextBox();
            this.labPathEuromodFiles = new System.Windows.Forms.Label();
            this.btnSelPathMergedDataset = new System.Windows.Forms.Button();
            this.txtPathMergedDataset = new System.Windows.Forms.TextBox();
            this.chkSaveMergedDataset = new System.Windows.Forms.CheckBox();
            this.groupCompareWith = new System.Windows.Forms.GroupBox();
            this.radioCompareWithPrevious = new System.Windows.Forms.RadioButton();
            this.radioCompareWithBaseline = new System.Windows.Forms.RadioButton();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnRstPathEuromod = new System.Windows.Forms.Button();
            this.btnRstPathMergedDataset = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.gridBaselines)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridReforms)).BeginInit();
            this.mainMenu.SuspendLayout();
            this.groupCompareWith.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnRun
            // 
            this.btnRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRun.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRun.Location = new System.Drawing.Point(620, 493);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(91, 42);
            this.btnRun.TabIndex = 36;
            this.btnRun.Text = "Run";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // gridBaselines
            // 
            this.gridBaselines.AllowUserToAddRows = false;
            this.gridBaselines.AllowUserToDeleteRows = false;
            this.gridBaselines.AllowUserToResizeRows = false;
            this.gridBaselines.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridBaselines.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridBaselines.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridBaselines.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colBaselineExists,
            this.colBaselines,
            this.colBaselineLabel});
            this.gridBaselines.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.gridBaselines.Location = new System.Drawing.Point(12, 67);
            this.gridBaselines.MultiSelect = false;
            this.gridBaselines.Name = "gridBaselines";
            this.gridBaselines.RowHeadersVisible = false;
            this.gridBaselines.RowHeadersWidth = 20;
            this.gridBaselines.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.gridBaselines.RowsDefaultCellStyle = dataGridViewCellStyle1;
            this.gridBaselines.RowTemplate.DefaultCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
            this.gridBaselines.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.ControlLightLight;
            this.gridBaselines.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.gridBaselines.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridBaselines.Size = new System.Drawing.Size(333, 116);
            this.gridBaselines.TabIndex = 1;
            this.gridBaselines.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridBaselines_CellValueChanged);
            // 
            // colBaselineExists
            // 
            this.colBaselineExists.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colBaselineExists.HeaderText = "";
            this.colBaselineExists.Name = "colBaselineExists";
            this.colBaselineExists.ReadOnly = true;
            this.colBaselineExists.ToolTipText = "Check: file exists; cross: file does not exist (anymore)";
            this.colBaselineExists.Width = 5;
            // 
            // colBaselines
            // 
            this.colBaselines.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colBaselines.HeaderText = "Baselines";
            this.colBaselines.Name = "colBaselines";
            this.colBaselines.ReadOnly = true;
            this.colBaselines.Width = 88;
            // 
            // colBaselineLabel
            // 
            this.colBaselineLabel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colBaselineLabel.HeaderText = "Label";
            this.colBaselineLabel.Name = "colBaselineLabel";
            this.colBaselineLabel.ToolTipText = "Label used in Tables as name for baseline, if left empty system name is used";
            // 
            // gridReforms
            // 
            this.gridReforms.AllowUserToAddRows = false;
            this.gridReforms.AllowUserToDeleteRows = false;
            this.gridReforms.AllowUserToResizeRows = false;
            this.gridReforms.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridReforms.BackgroundColor = System.Drawing.SystemColors.Control;
            this.gridReforms.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridReforms.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colReformsExists,
            this.colReforms,
            this.colReformsLabel});
            this.gridReforms.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.gridReforms.Location = new System.Drawing.Point(12, 189);
            this.gridReforms.MultiSelect = false;
            this.gridReforms.Name = "gridReforms";
            this.gridReforms.RowHeadersVisible = false;
            this.gridReforms.RowHeadersWidth = 20;
            this.gridReforms.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.DisableResizing;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLightLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            this.gridReforms.RowsDefaultCellStyle = dataGridViewCellStyle2;
            this.gridReforms.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridReforms.Size = new System.Drawing.Size(333, 198);
            this.gridReforms.TabIndex = 2;
            this.gridReforms.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridReforms_CellValueChanged);
            // 
            // colReformsExists
            // 
            this.colReformsExists.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colReformsExists.HeaderText = "";
            this.colReformsExists.Name = "colReformsExists";
            this.colReformsExists.ReadOnly = true;
            this.colReformsExists.ToolTipText = "Check: file exists; cross: file does not exist (anymore)";
            this.colReformsExists.Width = 5;
            // 
            // colReforms
            // 
            this.colReforms.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colReforms.HeaderText = "Reforms";
            this.colReforms.Name = "colReforms";
            this.colReforms.ReadOnly = true;
            this.colReforms.Width = 80;
            // 
            // colReformsLabel
            // 
            this.colReformsLabel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colReformsLabel.HeaderText = "Label";
            this.colReformsLabel.Name = "colReformsLabel";
            this.colReformsLabel.ToolTipText = "Label used in Tables as name for Reform, if left empty system name is used";
            // 
            // treeCategories
            // 
            this.treeCategories.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeCategories.BackColor = System.Drawing.SystemColors.Control;
            this.treeCategories.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeCategories.CheckBoxes = true;
            this.treeCategories.Location = new System.Drawing.Point(347, 67);
            this.treeCategories.Name = "treeCategories";
            this.treeCategories.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.treeCategories.ShowPlusMinus = false;
            this.treeCategories.Size = new System.Drawing.Size(347, 405);
            this.treeCategories.TabIndex = 37;
            this.treeCategories.DrawNode += new System.Windows.Forms.DrawTreeNodeEventHandler(this.treeCategories_DrawNode);
            this.treeCategories.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeCategories.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeCategories_NodeMouseClick);
            // 
            // mainMenu
            // 
            this.mainMenu.BackColor = System.Drawing.SystemColors.ControlLight;
            this.mainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFile,
            this.miSettings,
            this.miHelpMain});
            this.mainMenu.Location = new System.Drawing.Point(0, 0);
            this.mainMenu.Name = "mainMenu";
            this.mainMenu.Size = new System.Drawing.Size(723, 24);
            this.mainMenu.TabIndex = 52;
            this.mainMenu.Text = "menuStrip1";
            // 
            // miFile
            // 
            this.miFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newToolStripMenuItem,
            this.miLoadSettings,
            this.miSaveSettings,
            this.saveAsToolStripMenuItem,
            this.miFileSeparator,
            this.miExit});
            this.miFile.Name = "miFile";
            this.miFile.Size = new System.Drawing.Size(37, 20);
            this.miFile.Text = "&File";
            // 
            // newToolStripMenuItem
            // 
            this.newToolStripMenuItem.Name = "newToolStripMenuItem";
            this.newToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.newToolStripMenuItem.Text = "&New";
            this.newToolStripMenuItem.Click += new System.EventHandler(this.newToolStripMenuItem_Click);
            // 
            // miLoadSettings
            // 
            this.miLoadSettings.Name = "miLoadSettings";
            this.miLoadSettings.Size = new System.Drawing.Size(123, 22);
            this.miLoadSettings.Text = "&Load";
            this.miLoadSettings.Click += new System.EventHandler(this.miLoadSettings_Click);
            // 
            // miSaveSettings
            // 
            this.miSaveSettings.Name = "miSaveSettings";
            this.miSaveSettings.Size = new System.Drawing.Size(123, 22);
            this.miSaveSettings.Text = "&Save";
            this.miSaveSettings.Click += new System.EventHandler(this.miSaveSettings_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(123, 22);
            this.saveAsToolStripMenuItem.Text = "Save &As...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // miFileSeparator
            // 
            this.miFileSeparator.Name = "miFileSeparator";
            this.miFileSeparator.Size = new System.Drawing.Size(120, 6);
            // 
            // miExit
            // 
            this.miExit.Name = "miExit";
            this.miExit.Size = new System.Drawing.Size(123, 22);
            this.miExit.Text = "E&xit";
            this.miExit.Click += new System.EventHandler(this.miExit_Click);
            // 
            // miSettings
            // 
            this.miSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miFiscalSettings,
            this.miDistributionalSettings,
            this.miInequalityPovertySettings,
            this.toolStripMenuItem1,
            this.miResetSettings});
            this.miSettings.Name = "miSettings";
            this.miSettings.Size = new System.Drawing.Size(116, 20);
            this.miSettings.Text = "Advanced &settings";
            // 
            // miFiscalSettings
            // 
            this.miFiscalSettings.Name = "miFiscalSettings";
            this.miFiscalSettings.Size = new System.Drawing.Size(238, 22);
            this.miFiscalSettings.Text = "1. &Fiscal settings";
            this.miFiscalSettings.Click += new System.EventHandler(this.miFiscalSettings_Click);
            // 
            // miDistributionalSettings
            // 
            this.miDistributionalSettings.Name = "miDistributionalSettings";
            this.miDistributionalSettings.Size = new System.Drawing.Size(238, 22);
            this.miDistributionalSettings.Text = "2. &Distributional settings";
            this.miDistributionalSettings.Click += new System.EventHandler(this.miDistributionalSettings_Click);
            // 
            // miInequalityPovertySettings
            // 
            this.miInequalityPovertySettings.Name = "miInequalityPovertySettings";
            this.miInequalityPovertySettings.Size = new System.Drawing.Size(238, 22);
            this.miInequalityPovertySettings.Text = "3. &Inequality && poverty settings";
            this.miInequalityPovertySettings.Click += new System.EventHandler(this.miInequalityPovertySettings_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(235, 6);
            // 
            // miResetSettings
            // 
            this.miResetSettings.Name = "miResetSettings";
            this.miResetSettings.Size = new System.Drawing.Size(238, 22);
            this.miResetSettings.Text = "&Reset settings";
            this.miResetSettings.Click += new System.EventHandler(this.miResetSettings_Click);
            // 
            // miHelpMain
            // 
            this.miHelpMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.miHelp,
            this.miHelpAbout});
            this.miHelpMain.Name = "miHelpMain";
            this.miHelpMain.Size = new System.Drawing.Size(44, 20);
            this.miHelpMain.Text = "&Help";
            // 
            // miHelp
            // 
            this.miHelp.Name = "miHelp";
            this.miHelp.Size = new System.Drawing.Size(107, 22);
            this.miHelp.Text = "Help";
            this.miHelp.Click += new System.EventHandler(this.miHelp_Click);
            // 
            // miHelpAbout
            // 
            this.miHelpAbout.Name = "miHelpAbout";
            this.miHelpAbout.Size = new System.Drawing.Size(107, 22);
            this.miHelpAbout.Text = "About";
            this.miHelpAbout.Click += new System.EventHandler(this.miHelpAbout_Click);
            // 
            // btnSelectBaselinesAndReforms
            // 
            this.btnSelectBaselinesAndReforms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBaselinesAndReforms.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSelectBaselinesAndReforms.Location = new System.Drawing.Point(12, 388);
            this.btnSelectBaselinesAndReforms.Name = "btnSelectBaselinesAndReforms";
            this.btnSelectBaselinesAndReforms.Size = new System.Drawing.Size(333, 32);
            this.btnSelectBaselinesAndReforms.TabIndex = 53;
            this.btnSelectBaselinesAndReforms.Text = "Select baselines && reforms";
            this.btnSelectBaselinesAndReforms.UseVisualStyleBackColor = true;
            this.btnSelectBaselinesAndReforms.Click += new System.EventHandler(this.btnSelectBaselinesAndReforms_Click);
            // 
            // btnSelPathEuromod
            // 
            this.btnSelPathEuromod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelPathEuromod.Font = new System.Drawing.Font("Wingdings", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnSelPathEuromod.Location = new System.Drawing.Point(659, 33);
            this.btnSelPathEuromod.Name = "btnSelPathEuromod";
            this.btnSelPathEuromod.Size = new System.Drawing.Size(24, 24);
            this.btnSelPathEuromod.TabIndex = 55;
            this.btnSelPathEuromod.Text = "1";
            this.btnSelPathEuromod.UseVisualStyleBackColor = true;
            this.btnSelPathEuromod.Click += new System.EventHandler(this.btnSelPathEuromod_Click);
            // 
            // txtPathEuromodFiles
            // 
            this.txtPathEuromodFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathEuromodFiles.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPathEuromodFiles.Location = new System.Drawing.Point(155, 36);
            this.txtPathEuromodFiles.Name = "txtPathEuromodFiles";
            this.txtPathEuromodFiles.Size = new System.Drawing.Size(498, 20);
            this.txtPathEuromodFiles.TabIndex = 54;
            this.txtPathEuromodFiles.Validated += new System.EventHandler(this.txtPathEuromodFiles_Validated);
            // 
            // labPathEuromodFiles
            // 
            this.labPathEuromodFiles.AutoSize = true;
            this.labPathEuromodFiles.Location = new System.Drawing.Point(13, 37);
            this.labPathEuromodFiles.Name = "labPathEuromodFiles";
            this.labPathEuromodFiles.Size = new System.Drawing.Size(143, 15);
            this.labPathEuromodFiles.TabIndex = 56;
            this.labPathEuromodFiles.Text = "EUROMOD project folder";
            // 
            // btnSelPathMergedDataset
            // 
            this.btnSelPathMergedDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelPathMergedDataset.Font = new System.Drawing.Font("Wingdings", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnSelPathMergedDataset.Location = new System.Drawing.Point(560, 502);
            this.btnSelPathMergedDataset.Name = "btnSelPathMergedDataset";
            this.btnSelPathMergedDataset.Size = new System.Drawing.Size(24, 24);
            this.btnSelPathMergedDataset.TabIndex = 59;
            this.btnSelPathMergedDataset.Text = "1";
            this.btnSelPathMergedDataset.UseVisualStyleBackColor = true;
            this.btnSelPathMergedDataset.Click += new System.EventHandler(this.btnSelPathMergedDataset_Click);
            // 
            // txtPathMergedDataset
            // 
            this.txtPathMergedDataset.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPathMergedDataset.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtPathMergedDataset.Location = new System.Drawing.Point(12, 504);
            this.txtPathMergedDataset.Name = "txtPathMergedDataset";
            this.txtPathMergedDataset.Size = new System.Drawing.Size(542, 20);
            this.txtPathMergedDataset.TabIndex = 58;
            this.txtPathMergedDataset.Validated += new System.EventHandler(this.txtPathMergedDataset_Validated);
            // 
            // chkSaveMergedDataset
            // 
            this.chkSaveMergedDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkSaveMergedDataset.AutoSize = true;
            this.chkSaveMergedDataset.Location = new System.Drawing.Point(13, 484);
            this.chkSaveMergedDataset.Name = "chkSaveMergedDataset";
            this.chkSaveMergedDataset.Size = new System.Drawing.Size(168, 19);
            this.chkSaveMergedDataset.TabIndex = 57;
            this.chkSaveMergedDataset.Text = "Save merged dataset at ...";
            this.chkSaveMergedDataset.UseVisualStyleBackColor = true;
            this.chkSaveMergedDataset.CheckedChanged += new System.EventHandler(this.chkSaveMergedDataset_CheckedChanged);
            // 
            // groupCompareWith
            // 
            this.groupCompareWith.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupCompareWith.Controls.Add(this.radioCompareWithPrevious);
            this.groupCompareWith.Controls.Add(this.radioCompareWithBaseline);
            this.groupCompareWith.Location = new System.Drawing.Point(12, 432);
            this.groupCompareWith.Name = "groupCompareWith";
            this.groupCompareWith.Size = new System.Drawing.Size(333, 40);
            this.groupCompareWith.TabIndex = 60;
            this.groupCompareWith.TabStop = false;
            this.groupCompareWith.Text = "Compare with ...";
            // 
            // radioCompareWithPrevious
            // 
            this.radioCompareWithPrevious.AutoSize = true;
            this.radioCompareWithPrevious.Location = new System.Drawing.Point(103, 17);
            this.radioCompareWithPrevious.Name = "radioCompareWithPrevious";
            this.radioCompareWithPrevious.Size = new System.Drawing.Size(123, 19);
            this.radioCompareWithPrevious.TabIndex = 1;
            this.radioCompareWithPrevious.Text = "previous scenario";
            this.radioCompareWithPrevious.UseVisualStyleBackColor = true;
            this.radioCompareWithPrevious.CheckedChanged += new System.EventHandler(this.radioCompareWith_CheckedChanged);
            // 
            // radioCompareWithBaseline
            // 
            this.radioCompareWithBaseline.AutoSize = true;
            this.radioCompareWithBaseline.Checked = true;
            this.radioCompareWithBaseline.Location = new System.Drawing.Point(8, 17);
            this.radioCompareWithBaseline.Name = "radioCompareWithBaseline";
            this.radioCompareWithBaseline.Size = new System.Drawing.Size(73, 19);
            this.radioCompareWithBaseline.TabIndex = 0;
            this.radioCompareWithBaseline.TabStop = true;
            this.radioCompareWithBaseline.Text = "baseline";
            this.radioCompareWithBaseline.UseVisualStyleBackColor = true;
            this.radioCompareWithBaseline.CheckedChanged += new System.EventHandler(this.radioCompareWith_CheckedChanged);
            // 
            // btnRstPathEuromod
            // 
            this.btnRstPathEuromod.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRstPathEuromod.Font = new System.Drawing.Font("Wingdings 3", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnRstPathEuromod.Location = new System.Drawing.Point(687, 33);
            this.btnRstPathEuromod.Name = "btnRstPathEuromod";
            this.btnRstPathEuromod.Size = new System.Drawing.Size(24, 24);
            this.btnRstPathEuromod.TabIndex = 61;
            this.btnRstPathEuromod.Text = "Q";
            this.toolTips.SetToolTip(this.btnRstPathEuromod, "Reset to current project folder");
            this.btnRstPathEuromod.UseVisualStyleBackColor = true;
            this.btnRstPathEuromod.Click += new System.EventHandler(this.btnRstPathEuromod_Click);
            // 
            // btnRstPathMergedDataset
            // 
            this.btnRstPathMergedDataset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRstPathMergedDataset.Font = new System.Drawing.Font("Wingdings 3", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.btnRstPathMergedDataset.Location = new System.Drawing.Point(590, 502);
            this.btnRstPathMergedDataset.Name = "btnRstPathMergedDataset";
            this.btnRstPathMergedDataset.Size = new System.Drawing.Size(24, 24);
            this.btnRstPathMergedDataset.TabIndex = 62;
            this.btnRstPathMergedDataset.Text = "Q";
            this.toolTips.SetToolTip(this.btnRstPathMergedDataset, "Reset to current project output folder");
            this.btnRstPathMergedDataset.UseVisualStyleBackColor = true;
            this.btnRstPathMergedDataset.Click += new System.EventHandler(this.btnRstPathMergedDataset_Click);
            // 
            // InDepthAnalysisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(723, 547);
            this.Controls.Add(this.btnRstPathMergedDataset);
            this.Controls.Add(this.btnRstPathEuromod);
            this.Controls.Add(this.groupCompareWith);
            this.Controls.Add(this.btnSelPathMergedDataset);
            this.Controls.Add(this.txtPathMergedDataset);
            this.Controls.Add(this.chkSaveMergedDataset);
            this.Controls.Add(this.btnSelPathEuromod);
            this.Controls.Add(this.txtPathEuromodFiles);
            this.Controls.Add(this.labPathEuromodFiles);
            this.Controls.Add(this.btnSelectBaselinesAndReforms);
            this.Controls.Add(this.treeCategories);
            this.Controls.Add(this.gridReforms);
            this.Controls.Add(this.gridBaselines);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.mainMenu);
            this.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider.SetHelpKeyword(this, "Settings_Main.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu;
            this.Name = "InDepthAnalysisForm";
            this.helpProvider.SetShowHelp(this, true);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "In-depth Analysis";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InDepthAnalysisForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.gridBaselines)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridReforms)).EndInit();
            this.mainMenu.ResumeLayout(false);
            this.mainMenu.PerformLayout();
            this.groupCompareWith.ResumeLayout(false);
            this.groupCompareWith.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.DataGridView gridBaselines;
        private System.Windows.Forms.DataGridView gridReforms;
        private EM_Common_Win.CustomControls.NoDoubleClickTreeView  treeCategories;
        private System.Windows.Forms.MenuStrip mainMenu;
        private System.Windows.Forms.ToolStripMenuItem miFile;
        private System.Windows.Forms.ToolStripMenuItem miLoadSettings;
        private System.Windows.Forms.ToolStripMenuItem miSaveSettings;
        private System.Windows.Forms.ToolStripMenuItem miExit;
        private System.Windows.Forms.ToolStripMenuItem miSettings;
        private System.Windows.Forms.ToolStripMenuItem miHelpMain;
        private System.Windows.Forms.ToolStripMenuItem miFiscalSettings;
        private System.Windows.Forms.ToolStripMenuItem miDistributionalSettings;
        private System.Windows.Forms.ToolStripMenuItem miInequalityPovertySettings;
        private System.Windows.Forms.ToolStripMenuItem miHelpAbout;
        private System.Windows.Forms.ToolStripSeparator miFileSeparator;
        private System.Windows.Forms.Button btnSelectBaselinesAndReforms;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnSelPathEuromod;
        private System.Windows.Forms.TextBox txtPathEuromodFiles;
        private System.Windows.Forms.Label labPathEuromodFiles;
        private System.Windows.Forms.Button btnSelPathMergedDataset;
        private System.Windows.Forms.TextBox txtPathMergedDataset;
        private System.Windows.Forms.CheckBox chkSaveMergedDataset;
        private System.Windows.Forms.ToolStripMenuItem miHelp;
        private System.Windows.Forms.GroupBox groupCompareWith;
        private System.Windows.Forms.RadioButton radioCompareWithPrevious;
        private System.Windows.Forms.RadioButton radioCompareWithBaseline;
        private System.Windows.Forms.DataGridViewImageColumn colBaselineExists;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBaselines;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBaselineLabel;
        private System.Windows.Forms.DataGridViewImageColumn colReformsExists;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReforms;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReformsLabel;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem miResetSettings;
        private System.Windows.Forms.ToolStripMenuItem newToolStripMenuItem;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnRstPathEuromod;
        private System.Windows.Forms.Button btnRstPathMergedDataset;
    }
}