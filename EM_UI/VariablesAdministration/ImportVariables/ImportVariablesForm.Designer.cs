namespace EM_UI.VariablesAdministration.ImportVariables
{
    partial class ImportVariablesForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportVariablesForm));
            this.dgvCategories = new System.Windows.Forms.DataGridView();
            this.colCategoryOldValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCategoryOldDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNewCategoryValue = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNewCategoryDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvVariables = new System.Windows.Forms.DataGridView();
            this.colVariableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAction = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPerformVariables = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvDescriptions = new System.Windows.Forms.DataGridView();
            this.colCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colOldCountryDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colNewCountryDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.treeAcronyms = new DevExpress.XtraTreeList.TreeList();
            this.colAcronym = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colActionAcronyms = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colPerformAcronyms = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colInfoAcronyms = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.rpiCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.label1 = new System.Windows.Forms.Label();
            this.txtImportFile = new System.Windows.Forms.TextBox();
            this.btnSelectImportFile = new System.Windows.Forms.Button();
            this.lblVariables = new System.Windows.Forms.Label();
            this.lblAcronyms = new System.Windows.Forms.Label();
            this.lblDescriptions = new System.Windows.Forms.Label();
            this.lblCategories = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnTickSelectedVariables = new System.Windows.Forms.Button();
            this.btnUntickSelectedVariables = new System.Windows.Forms.Button();
            this.btnAddOnly = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDescriptions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeAcronyms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rpiCheckEdit)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvCategories
            // 
            this.dgvCategories.AllowUserToAddRows = false;
            this.dgvCategories.AllowUserToDeleteRows = false;
            this.dgvCategories.AllowUserToResizeRows = false;
            this.dgvCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCategories.BackgroundColor = System.Drawing.Color.White;
            this.dgvCategories.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCategories.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCategoryOldValue,
            this.colCategoryOldDescription,
            this.colNewCategoryValue,
            this.colNewCategoryDescription});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvCategories.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvCategories.GridColor = System.Drawing.SystemColors.Control;
            this.dgvCategories.Location = new System.Drawing.Point(542, 294);
            this.dgvCategories.Name = "dgvCategories";
            this.dgvCategories.RowHeadersVisible = false;
            this.dgvCategories.Size = new System.Drawing.Size(500, 127);
            this.dgvCategories.TabIndex = 5;
            // 
            // colCategoryOldValue
            // 
            this.colCategoryOldValue.Frozen = true;
            this.colCategoryOldValue.HeaderText = "Old Value";
            this.colCategoryOldValue.Name = "colCategoryOldValue";
            this.colCategoryOldValue.ReadOnly = true;
            this.colCategoryOldValue.Width = 58;
            // 
            // colCategoryOldDescription
            // 
            this.colCategoryOldDescription.Frozen = true;
            this.colCategoryOldDescription.HeaderText = "Old Description";
            this.colCategoryOldDescription.Name = "colCategoryOldDescription";
            this.colCategoryOldDescription.ReadOnly = true;
            this.colCategoryOldDescription.Width = 85;
            // 
            // colNewCategoryValue
            // 
            this.colNewCategoryValue.Frozen = true;
            this.colNewCategoryValue.HeaderText = "New Value";
            this.colNewCategoryValue.Name = "colNewCategoryValue";
            this.colNewCategoryValue.ReadOnly = true;
            // 
            // colNewCategoryDescription
            // 
            this.colNewCategoryDescription.Frozen = true;
            this.colNewCategoryDescription.HeaderText = "New Description";
            this.colNewCategoryDescription.Name = "colNewCategoryDescription";
            this.colNewCategoryDescription.ReadOnly = true;
            // 
            // dgvVariables
            // 
            this.dgvVariables.AllowUserToAddRows = false;
            this.dgvVariables.AllowUserToDeleteRows = false;
            this.dgvVariables.AllowUserToResizeRows = false;
            this.dgvVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvVariables.BackgroundColor = System.Drawing.Color.White;
            this.dgvVariables.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVariables.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVariableName,
            this.colAction,
            this.colPerformVariables,
            this.colInfo});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvVariables.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvVariables.GridColor = System.Drawing.SystemColors.Control;
            this.dgvVariables.Location = new System.Drawing.Point(12, 69);
            this.dgvVariables.Name = "dgvVariables";
            this.dgvVariables.RowHeadersVisible = false;
            this.dgvVariables.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVariables.Size = new System.Drawing.Size(500, 206);
            this.dgvVariables.TabIndex = 2;
            this.dgvVariables.SelectionChanged += new System.EventHandler(this.dgvVariables_SelectionChanged);
            // 
            // colVariableName
            // 
            this.colVariableName.HeaderText = "Name";
            this.colVariableName.Name = "colVariableName";
            this.colVariableName.ReadOnly = true;
            this.colVariableName.Width = 59;
            // 
            // colAction
            // 
            this.colAction.HeaderText = "Action";
            this.colAction.Name = "colAction";
            this.colAction.ReadOnly = true;
            this.colAction.Width = 99;
            // 
            // colPerformVariables
            // 
            this.colPerformVariables.HeaderText = "Perform";
            this.colPerformVariables.Name = "colPerformVariables";
            this.colPerformVariables.Width = 59;
            // 
            // colInfo
            // 
            this.colInfo.HeaderText = "Info";
            this.colInfo.Name = "colInfo";
            this.colInfo.ReadOnly = true;
            // 
            // dgvDescriptions
            // 
            this.dgvDescriptions.AllowUserToAddRows = false;
            this.dgvDescriptions.AllowUserToDeleteRows = false;
            this.dgvDescriptions.AllowUserToResizeRows = false;
            this.dgvDescriptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDescriptions.BackgroundColor = System.Drawing.Color.White;
            this.dgvDescriptions.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDescriptions.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCountry,
            this.colOldCountryDescription,
            this.colNewCountryDescription});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDescriptions.DefaultCellStyle = dataGridViewCellStyle3;
            this.dgvDescriptions.GridColor = System.Drawing.SystemColors.Control;
            this.dgvDescriptions.Location = new System.Drawing.Point(12, 294);
            this.dgvDescriptions.MultiSelect = false;
            this.dgvDescriptions.Name = "dgvDescriptions";
            this.dgvDescriptions.RowHeadersVisible = false;
            this.dgvDescriptions.Size = new System.Drawing.Size(500, 127);
            this.dgvDescriptions.TabIndex = 4;
            // 
            // colCountry
            // 
            this.colCountry.HeaderText = "Country";
            this.colCountry.Name = "colCountry";
            this.colCountry.ReadOnly = true;
            this.colCountry.Width = 71;
            // 
            // colOldCountryDescription
            // 
            this.colOldCountryDescription.HeaderText = "Old Description";
            this.colOldCountryDescription.Name = "colOldCountryDescription";
            this.colOldCountryDescription.ReadOnly = true;
            this.colOldCountryDescription.Width = 85;
            // 
            // colNewCountryDescription
            // 
            this.colNewCountryDescription.HeaderText = "New Description";
            this.colNewCountryDescription.Name = "colNewCountryDescription";
            this.colNewCountryDescription.ReadOnly = true;
            // 
            // treeAcronyms
            // 
            this.treeAcronyms.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeAcronyms.Appearance.EvenRow.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.EvenRow.BackColor2 = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.EvenRow.Options.UseBackColor = true;
            this.treeAcronyms.Appearance.FocusedCell.BackColor = System.Drawing.Color.Transparent;
            this.treeAcronyms.Appearance.FocusedCell.BackColor2 = System.Drawing.Color.Transparent;
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
            this.colActionAcronyms,
            this.colPerformAcronyms,
            this.colInfoAcronyms});
            this.treeAcronyms.CustomizationFormBounds = new System.Drawing.Rectangle(892, 439, 208, 170);
            this.treeAcronyms.ImeMode = System.Windows.Forms.ImeMode.Katakana;
            this.treeAcronyms.Location = new System.Drawing.Point(542, 69);
            this.treeAcronyms.LookAndFeel.SkinName = "Black";
            this.treeAcronyms.LookAndFeel.UseDefaultLookAndFeel = false;
            this.treeAcronyms.MinWidth = 50;
            this.treeAcronyms.Name = "treeAcronyms";
            this.treeAcronyms.OptionsView.AutoWidth = false;
            this.treeAcronyms.OptionsView.ShowIndicator = false;
            this.treeAcronyms.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.rpiCheckEdit});
            this.treeAcronyms.ShowButtonMode = DevExpress.XtraTreeList.ShowButtonModeEnum.ShowOnlyInEditor;
            this.treeAcronyms.Size = new System.Drawing.Size(500, 206);
            this.treeAcronyms.TabIndex = 3;
            this.treeAcronyms.CustomNodeCellEdit += new DevExpress.XtraTreeList.GetCustomNodeCellEditEventHandler(this.treeAcronyms_CustomNodeCellEdit);
            this.treeAcronyms.FocusedNodeChanged += new DevExpress.XtraTreeList.FocusedNodeChangedEventHandler(this.treeAcronyms_FocusedNodeChanged);
            // 
            // colAcronym
            // 
            this.colAcronym.Caption = "Acronym";
            this.colAcronym.FieldName = "Acronym";
            this.colAcronym.MinWidth = 50;
            this.colAcronym.Name = "colAcronym";
            this.colAcronym.OptionsColumn.AllowEdit = false;
            this.colAcronym.OptionsColumn.AllowMove = false;
            this.colAcronym.OptionsColumn.AllowSort = false;
            this.colAcronym.Visible = true;
            this.colAcronym.VisibleIndex = 0;
            // 
            // colActionAcronyms
            // 
            this.colActionAcronyms.Caption = "Action";
            this.colActionAcronyms.FieldName = "Action";
            this.colActionAcronyms.MinWidth = 50;
            this.colActionAcronyms.Name = "colActionAcronyms";
            this.colActionAcronyms.OptionsColumn.AllowEdit = false;
            this.colActionAcronyms.OptionsColumn.AllowMove = false;
            this.colActionAcronyms.OptionsColumn.AllowSort = false;
            this.colActionAcronyms.Visible = true;
            this.colActionAcronyms.VisibleIndex = 1;
            // 
            // colPerformAcronyms
            // 
            this.colPerformAcronyms.Caption = "Perform";
            this.colPerformAcronyms.FieldName = "Perform";
            this.colPerformAcronyms.MinWidth = 50;
            this.colPerformAcronyms.Name = "colPerformAcronyms";
            this.colPerformAcronyms.OptionsColumn.AllowMove = false;
            this.colPerformAcronyms.OptionsColumn.AllowSort = false;
            this.colPerformAcronyms.Visible = true;
            this.colPerformAcronyms.VisibleIndex = 2;
            // 
            // colInfoAcronyms
            // 
            this.colInfoAcronyms.Caption = "Info";
            this.colInfoAcronyms.FieldName = "Info";
            this.colInfoAcronyms.MinWidth = 50;
            this.colInfoAcronyms.Name = "colInfoAcronyms";
            this.colInfoAcronyms.OptionsColumn.AllowEdit = false;
            this.colInfoAcronyms.OptionsColumn.AllowMove = false;
            this.colInfoAcronyms.OptionsColumn.AllowSort = false;
            this.colInfoAcronyms.Visible = true;
            this.colInfoAcronyms.VisibleIndex = 3;
            // 
            // rpiCheckEdit
            // 
            this.rpiCheckEdit.AutoHeight = false;
            this.rpiCheckEdit.Name = "rpiCheckEdit";
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 9;
            this.label1.Text = "Import from";
            // 
            // txtImportFile
            // 
            this.txtImportFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImportFile.Location = new System.Drawing.Point(12, 25);
            this.txtImportFile.Name = "txtImportFile";
            this.txtImportFile.Size = new System.Drawing.Size(454, 20);
            this.txtImportFile.TabIndex = 1;
            this.txtImportFile.Validating += new System.ComponentModel.CancelEventHandler(this.txtImportFile_Validating);
            // 
            // btnSelectImportFile
            // 
            this.btnSelectImportFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectImportFile.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectImportFile.Image")));
            this.btnSelectImportFile.Location = new System.Drawing.Point(472, 14);
            this.btnSelectImportFile.Name = "btnSelectImportFile";
            this.btnSelectImportFile.Size = new System.Drawing.Size(40, 40);
            this.btnSelectImportFile.TabIndex = 11;
            this.btnSelectImportFile.UseVisualStyleBackColor = true;
            this.btnSelectImportFile.Click += new System.EventHandler(this.btnSelectImportFile_Click);
            // 
            // lblVariables
            // 
            this.lblVariables.AutoSize = true;
            this.lblVariables.BackColor = System.Drawing.SystemColors.Control;
            this.lblVariables.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVariables.ForeColor = System.Drawing.Color.Black;
            this.lblVariables.Location = new System.Drawing.Point(12, 50);
            this.lblVariables.Name = "lblVariables";
            this.lblVariables.Size = new System.Drawing.Size(68, 16);
            this.lblVariables.TabIndex = 12;
            this.lblVariables.Text = "Variables";
            // 
            // lblAcronyms
            // 
            this.lblAcronyms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAcronyms.AutoSize = true;
            this.lblAcronyms.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAcronyms.Location = new System.Drawing.Point(539, 50);
            this.lblAcronyms.Name = "lblAcronyms";
            this.lblAcronyms.Size = new System.Drawing.Size(73, 16);
            this.lblAcronyms.TabIndex = 13;
            this.lblAcronyms.Text = "Acronyms";
            // 
            // lblDescriptions
            // 
            this.lblDescriptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblDescriptions.AutoSize = true;
            this.lblDescriptions.Location = new System.Drawing.Point(12, 278);
            this.lblDescriptions.Name = "lblDescriptions";
            this.lblDescriptions.Size = new System.Drawing.Size(65, 13);
            this.lblDescriptions.TabIndex = 14;
            this.lblDescriptions.Text = "Descriptions";
            // 
            // lblCategories
            // 
            this.lblCategories.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCategories.AutoSize = true;
            this.lblCategories.Location = new System.Drawing.Point(539, 278);
            this.lblCategories.Name = "lblCategories";
            this.lblCategories.Size = new System.Drawing.Size(57, 13);
            this.lblCategories.TabIndex = 15;
            this.lblCategories.Text = "Categories";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(542, 444);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnImport.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnImport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnImport.Location = new System.Drawing.Point(422, 444);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(90, 23);
            this.btnImport.TabIndex = 6;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnTickSelectedVariables
            // 
            this.btnTickSelectedVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnTickSelectedVariables.Location = new System.Drawing.Point(15, 427);
            this.btnTickSelectedVariables.Name = "btnTickSelectedVariables";
            this.btnTickSelectedVariables.Size = new System.Drawing.Size(147, 20);
            this.btnTickSelectedVariables.TabIndex = 16;
            this.btnTickSelectedVariables.Text = "Tick selected variables";
            this.btnTickSelectedVariables.UseVisualStyleBackColor = true;
            this.btnTickSelectedVariables.Click += new System.EventHandler(this.btnTickSelectedVariables_Click);
            // 
            // btnUntickSelectedVariables
            // 
            this.btnUntickSelectedVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUntickSelectedVariables.Location = new System.Drawing.Point(15, 453);
            this.btnUntickSelectedVariables.Name = "btnUntickSelectedVariables";
            this.btnUntickSelectedVariables.Size = new System.Drawing.Size(147, 20);
            this.btnUntickSelectedVariables.TabIndex = 17;
            this.btnUntickSelectedVariables.Text = "Untick selected variables";
            this.btnUntickSelectedVariables.UseVisualStyleBackColor = true;
            this.btnUntickSelectedVariables.Click += new System.EventHandler(this.btnUntickSelectedVariables_Click);
            // 
            // btnAddOnly
            // 
            this.btnAddOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddOnly.Location = new System.Drawing.Point(967, 444);
            this.btnAddOnly.Name = "btnAddOnly";
            this.btnAddOnly.Size = new System.Drawing.Size(75, 23);
            this.btnAddOnly.TabIndex = 18;
            this.btnAddOnly.Text = "Add only";
            this.btnAddOnly.UseVisualStyleBackColor = true;
            this.btnAddOnly.Click += new System.EventHandler(this.btnAddOnly_Click);
            // 
            // ImportVariablesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1054, 479);
            this.Controls.Add(this.btnAddOnly);
            this.Controls.Add(this.btnUntickSelectedVariables);
            this.Controls.Add(this.btnTickSelectedVariables);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.lblCategories);
            this.Controls.Add(this.lblDescriptions);
            this.Controls.Add(this.lblAcronyms);
            this.Controls.Add(this.lblVariables);
            this.Controls.Add(this.btnSelectImportFile);
            this.Controls.Add(this.txtImportFile);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvCategories);
            this.Controls.Add(this.dgvVariables);
            this.Controls.Add(this.dgvDescriptions);
            this.Controls.Add(this.treeAcronyms);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ImportingVariables.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportVariablesForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Import Variables";
            this.Shown += new System.EventHandler(this.ImportVariablesForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCategories)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDescriptions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeAcronyms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rpiCheckEdit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal System.Windows.Forms.DataGridView dgvCategories;
        internal System.Windows.Forms.DataGridView dgvVariables;
        internal System.Windows.Forms.DataGridView dgvDescriptions;
        internal DevExpress.XtraTreeList.TreeList treeAcronyms;
        internal System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtImportFile;
        private System.Windows.Forms.Button btnSelectImportFile;
        internal System.Windows.Forms.Label lblVariables;
        internal System.Windows.Forms.Label lblAcronyms;
        internal System.Windows.Forms.Label lblDescriptions;
        internal System.Windows.Forms.Label lblCategories;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnImport;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colPerformAcronyms;
        internal DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit rpiCheckEdit;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colAcronym;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colActionAcronyms;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colInfoAcronyms;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCategoryOldValue;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCategoryOldDescription;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colNewCategoryValue;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colNewCategoryDescription;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colCountry;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colOldCountryDescription;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colNewCountryDescription;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colVariableName;
        internal System.Windows.Forms.DataGridViewCheckBoxColumn colPerformVariables;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colAction;
        internal System.Windows.Forms.DataGridViewTextBoxColumn colInfo;
        private System.Windows.Forms.Button btnTickSelectedVariables;
        private System.Windows.Forms.Button btnUntickSelectedVariables;
        private System.Windows.Forms.Button btnAddOnly;
    }
}