namespace EM_UI.VariablesAdministration.CleanVariables
{
    partial class CleanVariablesForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnClean = new System.Windows.Forms.Button();
            this.lblAcronyms = new System.Windows.Forms.Label();
            this.lblVariables = new System.Windows.Forms.Label();
            this.dgvVariables = new System.Windows.Forms.DataGridView();
            this.colVariableName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVariableDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeleteVariables = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.ctmCheckOptions = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniCheckAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mniUncheckAll = new System.Windows.Forms.ToolStripMenuItem();
            this.treeAcronyms = new DevExpress.XtraTreeList.TreeList();
            this.colAcronym = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.colDeleteAcronyms = new DevExpress.XtraTreeList.Columns.TreeListColumn();
            this.rpiCheckEdit = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.pgbProgress = new System.Windows.Forms.ProgressBar();
            this.lblLoading = new System.Windows.Forms.Label();
            this.btnLoad = new System.Windows.Forms.Button();
            this.checkAll = new System.Windows.Forms.CheckBox();
            this.uncheckInput = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).BeginInit();
            this.ctmCheckOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeAcronyms)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rpiCheckEdit)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(952, 431);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnClean
            // 
            this.btnClean.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClean.Enabled = false;
            this.btnClean.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnClean.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClean.Location = new System.Drawing.Point(856, 431);
            this.btnClean.Name = "btnClean";
            this.btnClean.Size = new System.Drawing.Size(90, 23);
            this.btnClean.TabIndex = 16;
            this.btnClean.Text = "Clean";
            this.btnClean.UseVisualStyleBackColor = true;
            this.btnClean.Click += new System.EventHandler(this.btnClean_Click);
            // 
            // lblAcronyms
            // 
            this.lblAcronyms.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAcronyms.AutoSize = true;
            this.lblAcronyms.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAcronyms.Location = new System.Drawing.Point(539, 8);
            this.lblAcronyms.Name = "lblAcronyms";
            this.lblAcronyms.Size = new System.Drawing.Size(132, 16);
            this.lblAcronyms.TabIndex = 19;
            this.lblAcronyms.Text = "Not used acronyms";
            // 
            // lblVariables
            // 
            this.lblVariables.AutoSize = true;
            this.lblVariables.BackColor = System.Drawing.SystemColors.Control;
            this.lblVariables.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVariables.ForeColor = System.Drawing.Color.Black;
            this.lblVariables.Location = new System.Drawing.Point(12, 8);
            this.lblVariables.Name = "lblVariables";
            this.lblVariables.Size = new System.Drawing.Size(128, 16);
            this.lblVariables.TabIndex = 18;
            this.lblVariables.Text = "Not used variables";
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
            this.colVariableDescription,
            this.colDeleteVariables});
            this.dgvVariables.ContextMenuStrip = this.ctmCheckOptions;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvVariables.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvVariables.GridColor = System.Drawing.SystemColors.Control;
            this.dgvVariables.Location = new System.Drawing.Point(12, 27);
            this.dgvVariables.MultiSelect = false;
            this.dgvVariables.Name = "dgvVariables";
            this.dgvVariables.RowHeadersVisible = false;
            this.dgvVariables.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvVariables.Size = new System.Drawing.Size(500, 398);
            this.dgvVariables.TabIndex = 14;
            this.dgvVariables.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVariables_CellContentClick);
            this.dgvVariables.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvVariables_CellValueChanged);
            // 
            // colVariableName
            // 
            this.colVariableName.HeaderText = "Name";
            this.colVariableName.Name = "colVariableName";
            this.colVariableName.ReadOnly = true;
            this.colVariableName.Width = 59;
            // 
            // colVariableDescription
            // 
            this.colVariableDescription.HeaderText = "Description";
            this.colVariableDescription.Name = "colVariableDescription";
            this.colVariableDescription.ReadOnly = true;
            // 
            // colDeleteVariables
            // 
            this.colDeleteVariables.HeaderText = "Delete";
            this.colDeleteVariables.Name = "colDeleteVariables";
            this.colDeleteVariables.Width = 59;
            // 
            // ctmCheckOptions
            // 
            this.ctmCheckOptions.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniCheckAll,
            this.mniUncheckAll});
            this.ctmCheckOptions.Name = "ctmCheckOptions";
            this.ctmCheckOptions.Size = new System.Drawing.Size(138, 48);
            this.ctmCheckOptions.Opening += new System.ComponentModel.CancelEventHandler(this.ctmCheckOptions_Opening);
            // 
            // mniCheckAll
            // 
            this.mniCheckAll.Name = "mniCheckAll";
            this.mniCheckAll.Size = new System.Drawing.Size(137, 22);
            this.mniCheckAll.Text = "Check All";
            this.mniCheckAll.Click += new System.EventHandler(this.mniCheckAll_Click);
            // 
            // mniUncheckAll
            // 
            this.mniUncheckAll.Name = "mniUncheckAll";
            this.mniUncheckAll.Size = new System.Drawing.Size(137, 22);
            this.mniUncheckAll.Text = "Uncheck All";
            this.mniUncheckAll.Click += new System.EventHandler(this.mniUncheckAll_Click);
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
            this.colDeleteAcronyms});
            this.treeAcronyms.ContextMenuStrip = this.ctmCheckOptions;
            this.treeAcronyms.CustomizationFormBounds = new System.Drawing.Rectangle(892, 439, 208, 170);
            this.treeAcronyms.ImeMode = System.Windows.Forms.ImeMode.Katakana;
            this.treeAcronyms.Location = new System.Drawing.Point(542, 27);
            this.treeAcronyms.LookAndFeel.SkinName = "Black";
            this.treeAcronyms.LookAndFeel.UseDefaultLookAndFeel = false;
            this.treeAcronyms.MinWidth = 50;
            this.treeAcronyms.Name = "treeAcronyms";
            this.treeAcronyms.OptionsView.AutoWidth = false;
            this.treeAcronyms.OptionsView.ShowIndicator = false;
            this.treeAcronyms.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.rpiCheckEdit});
            this.treeAcronyms.ShowButtonMode = DevExpress.XtraTreeList.ShowButtonModeEnum.ShowOnlyInEditor;
            this.treeAcronyms.Size = new System.Drawing.Size(500, 398);
            this.treeAcronyms.TabIndex = 15;
            this.treeAcronyms.CustomNodeCellEdit += new DevExpress.XtraTreeList.GetCustomNodeCellEditEventHandler(this.treeAcronyms_CustomNodeCellEdit);
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
            // colDeleteAcronyms
            // 
            this.colDeleteAcronyms.Caption = "Delete";
            this.colDeleteAcronyms.FieldName = "Delete";
            this.colDeleteAcronyms.MinWidth = 50;
            this.colDeleteAcronyms.Name = "colDeleteAcronyms";
            this.colDeleteAcronyms.OptionsColumn.AllowMove = false;
            this.colDeleteAcronyms.OptionsColumn.AllowSort = false;
            this.colDeleteAcronyms.Visible = true;
            this.colDeleteAcronyms.VisibleIndex = 1;
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
            // pgbProgress
            // 
            this.pgbProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pgbProgress.Location = new System.Drawing.Point(183, 431);
            this.pgbProgress.Name = "pgbProgress";
            this.pgbProgress.Size = new System.Drawing.Size(329, 23);
            this.pgbProgress.TabIndex = 20;
            this.pgbProgress.Visible = false;
            // 
            // lblLoading
            // 
            this.lblLoading.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblLoading.AutoSize = true;
            this.lblLoading.Location = new System.Drawing.Point(12, 436);
            this.lblLoading.Name = "lblLoading";
            this.lblLoading.Size = new System.Drawing.Size(165, 13);
            this.lblLoading.TabIndex = 21;
            this.lblLoading.Text = "Searching for unused variables ...";
            this.lblLoading.Visible = false;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(760, 431);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(90, 23);
            this.btnLoad.TabIndex = 22;
            this.btnLoad.Text = "Load";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.btnLoad_Click);
            // 
            // checkAll
            // 
            this.checkAll.AutoSize = true;
            this.checkAll.Location = new System.Drawing.Point(262, 6);
            this.checkAll.Name = "checkAll";
            this.checkAll.Size = new System.Drawing.Size(71, 17);
            this.checkAll.TabIndex = 23;
            this.checkAll.Text = "Check All";
            this.checkAll.UseVisualStyleBackColor = true;
            this.checkAll.CheckedChanged += new System.EventHandler(this.checkAll_CheckedChanged);
            // 
            // uncheckInput
            // 
            this.uncheckInput.Location = new System.Drawing.Point(361, 2);
            this.uncheckInput.Name = "uncheckInput";
            this.uncheckInput.Size = new System.Drawing.Size(151, 23);
            this.uncheckInput.TabIndex = 24;
            this.uncheckInput.Text = "Uncheck Input Variables";
            this.uncheckInput.UseVisualStyleBackColor = true;
            this.uncheckInput.Click += new System.EventHandler(this.uncheckInput_Click);
            // 
            // CleanVariablesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1054, 466);
            this.ContextMenuStrip = this.ctmCheckOptions;
            this.Controls.Add(this.uncheckInput);
            this.Controls.Add(this.checkAll);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.lblLoading);
            this.Controls.Add(this.pgbProgress);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnClean);
            this.Controls.Add(this.lblAcronyms);
            this.Controls.Add(this.lblVariables);
            this.Controls.Add(this.dgvVariables);
            this.Controls.Add(this.treeAcronyms);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_CleaningVariables.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CleanVariablesForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Clean Variables";
            ((System.ComponentModel.ISupportInitialize)(this.dgvVariables)).EndInit();
            this.ctmCheckOptions.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeAcronyms)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rpiCheckEdit)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnClean;
        internal System.Windows.Forms.Label lblAcronyms;
        internal System.Windows.Forms.Label lblVariables;
        internal System.Windows.Forms.DataGridView dgvVariables;
        internal DevExpress.XtraTreeList.TreeList treeAcronyms;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colAcronym;
        internal DevExpress.XtraTreeList.Columns.TreeListColumn colDeleteAcronyms;
        internal DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit rpiCheckEdit;
        internal System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVariableName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVariableDescription;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDeleteVariables;
        private System.Windows.Forms.ProgressBar pgbProgress;
        private System.Windows.Forms.Label lblLoading;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.ContextMenuStrip ctmCheckOptions;
        private System.Windows.Forms.ToolStripMenuItem mniCheckAll;
        private System.Windows.Forms.ToolStripMenuItem mniUncheckAll;
        private System.Windows.Forms.CheckBox checkAll;
        private System.Windows.Forms.Button uncheckInput;
    }
}