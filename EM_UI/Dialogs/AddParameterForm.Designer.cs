namespace EM_UI.Dialogs
{
    partial class AddParameterForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddParameterForm));
            this.lblFunction = new System.Windows.Forms.TextBox();
            this.dgvParameter = new System.Windows.Forms.DataGridView();
            this.colAdd = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colParameter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReplaces = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGroupNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDefault = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.chkShowCommonParameters = new System.Windows.Forms.CheckBox();
            this.chkShowFootnoteParameters = new System.Windows.Forms.CheckBox();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnHelpDescription = new System.Windows.Forms.Button();
            this.btnHelpSummary = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameter)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFunction
            // 
            this.lblFunction.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblFunction.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblFunction.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFunction.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.lblFunction.Location = new System.Drawing.Point(12, 12);
            this.lblFunction.Name = "lblFunction";
            this.lblFunction.Size = new System.Drawing.Size(450, 19);
            this.lblFunction.TabIndex = 6;
            this.lblFunction.TabStop = false;
            this.lblFunction.Text = "Function in Policy";
            // 
            // dgvParameter
            // 
            this.dgvParameter.AllowUserToAddRows = false;
            this.dgvParameter.AllowUserToDeleteRows = false;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.Khaki;
            this.dgvParameter.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvParameter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvParameter.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
            this.dgvParameter.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvParameter.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvParameter.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colAdd,
            this.colParameter,
            this.colReplaces,
            this.colGroupNo,
            this.colCount,
            this.colDefault,
            this.colDescription});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Khaki;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvParameter.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvParameter.Location = new System.Drawing.Point(12, 37);
            this.dgvParameter.MultiSelect = false;
            this.dgvParameter.Name = "dgvParameter";
            this.dgvParameter.RowHeadersVisible = false;
            this.dgvParameter.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvParameter.ShowEditingIcon = false;
            this.dgvParameter.Size = new System.Drawing.Size(802, 461);
            this.dgvParameter.TabIndex = 8;
            this.dgvParameter.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.gridParam_CellValidating);
            this.dgvParameter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridParam_KeyDown);
            // 
            // colAdd
            // 
            this.colAdd.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colAdd.HeaderText = "Add";
            this.colAdd.Name = "colAdd";
            this.colAdd.Width = 32;
            // 
            // colParameter
            // 
            this.colParameter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colParameter.HeaderText = "Parameter";
            this.colParameter.Name = "colParameter";
            this.colParameter.ReadOnly = true;
            this.colParameter.Width = 80;
            // 
            // colReplaces
            // 
            this.colReplaces.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colReplaces.HeaderText = "Replaces";
            this.colReplaces.Name = "colReplaces";
            this.colReplaces.ReadOnly = true;
            this.colReplaces.ToolTipText = "Parameter is a substitude for existing parameter as indicated in this column";
            this.colReplaces.Width = 77;
            // 
            // colGroupNo
            // 
            this.colGroupNo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colGroupNo.HeaderText = "Grp/No";
            this.colGroupNo.Name = "colGroupNo";
            this.colGroupNo.ToolTipText = "Group of a group parameter respectively (first) number of a footnote parameter";
            this.colGroupNo.Width = 68;
            // 
            // colCount
            // 
            this.colCount.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colCount.HeaderText = "Count";
            this.colCount.Name = "colCount";
            this.colCount.ToolTipText = "Number of parameters to add";
            this.colCount.Width = 60;
            // 
            // colDefault
            // 
            this.colDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colDefault.HeaderText = "Default";
            this.colDefault.Name = "colDefault";
            this.colDefault.ReadOnly = true;
            this.colDefault.ToolTipText = "The default value if parameter is not used";
            this.colDefault.Width = 66;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // chkShowCommonParameters
            // 
            this.chkShowCommonParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowCommonParameters.AutoSize = true;
            this.chkShowCommonParameters.Checked = true;
            this.chkShowCommonParameters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowCommonParameters.Location = new System.Drawing.Point(12, 510);
            this.chkShowCommonParameters.Name = "chkShowCommonParameters";
            this.chkShowCommonParameters.Size = new System.Drawing.Size(153, 17);
            this.chkShowCommonParameters.TabIndex = 11;
            this.chkShowCommonParameters.Text = "Show Common Parameters";
            this.chkShowCommonParameters.UseVisualStyleBackColor = true;
            this.chkShowCommonParameters.CheckedChanged += new System.EventHandler(this.checkShowCommon_CheckedChanged);
            // 
            // chkShowFootnoteParameters
            // 
            this.chkShowFootnoteParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkShowFootnoteParameters.AutoSize = true;
            this.chkShowFootnoteParameters.Checked = true;
            this.chkShowFootnoteParameters.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowFootnoteParameters.Location = new System.Drawing.Point(12, 533);
            this.chkShowFootnoteParameters.Name = "chkShowFootnoteParameters";
            this.chkShowFootnoteParameters.Size = new System.Drawing.Size(154, 17);
            this.chkShowFootnoteParameters.TabIndex = 12;
            this.chkShowFootnoteParameters.Text = "Show Footnote Parameters";
            this.chkShowFootnoteParameters.UseVisualStyleBackColor = true;
            this.chkShowFootnoteParameters.CheckedChanged += new System.EventHandler(this.checkShowFootnote_CheckedChanged);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAdd.Image = ((System.Drawing.Image)(resources.GetObject("btnAdd.Image")));
            this.btnAdd.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAdd.Location = new System.Drawing.Point(626, 515);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(91, 35);
            this.btnAdd.TabIndex = 7;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(723, 515);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(91, 35);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnHelpDescription
            // 
            this.btnHelpDescription.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHelpDescription.Image = ((System.Drawing.Image)(resources.GetObject("btnHelpDescription.Image")));
            this.btnHelpDescription.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHelpDescription.Location = new System.Drawing.Point(293, 515);
            this.btnHelpDescription.Name = "btnHelpDescription";
            this.btnHelpDescription.Size = new System.Drawing.Size(103, 35);
            this.btnHelpDescription.TabIndex = 13;
            this.btnHelpDescription.Text = "Description (F5)";
            this.btnHelpDescription.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnHelpDescription.UseVisualStyleBackColor = true;
            this.btnHelpDescription.Click += new System.EventHandler(this.btnHelpDescription_Click);
            // 
            // btnHelpSummary
            // 
            this.btnHelpSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnHelpSummary.Image = ((System.Drawing.Image)(resources.GetObject("btnHelpSummary.Image")));
            this.btnHelpSummary.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHelpSummary.Location = new System.Drawing.Point(402, 515);
            this.btnHelpSummary.Name = "btnHelpSummary";
            this.btnHelpSummary.Size = new System.Drawing.Size(107, 35);
            this.btnHelpSummary.TabIndex = 14;
            this.btnHelpSummary.Text = "Summary (F6)";
            this.btnHelpSummary.UseVisualStyleBackColor = true;
            this.btnHelpSummary.Click += new System.EventHandler(this.btnHelpSummary_Click);
            // 
            // AddParameterForm
            // 
            this.AcceptButton = this.btnAdd;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(826, 568);
            this.Controls.Add(this.btnHelpSummary);
            this.Controls.Add(this.btnHelpDescription);
            this.Controls.Add(this.chkShowFootnoteParameters);
            this.Controls.Add(this.chkShowCommonParameters);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgvParameter);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lblFunction);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_AddingParameters.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MinimizeBox = false;
            this.Name = "AddParameterForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Add Parameters";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AddParamForm_FormClosing);
            this.Load += new System.EventHandler(this.AddParameterForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvParameter)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox lblFunction;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.DataGridView dgvParameter;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckBox chkShowCommonParameters;
        private System.Windows.Forms.CheckBox chkShowFootnoteParameters;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnHelpDescription;
        private System.Windows.Forms.Button btnHelpSummary;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn colParameter;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReplaces;
        private System.Windows.Forms.DataGridViewTextBoxColumn colGroupNo;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
    }
}