namespace EM_UI.UpratingIndices
{
    partial class UpratingIndicesUsageForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpratingIndicesUsageForm));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.dgvIndices = new System.Windows.Forms.DataGridView();
            this.colIndexDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUsage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnExtSearch = new System.Windows.Forms.Button();
            this.nudStartYear = new System.Windows.Forms.NumericUpDown();
            this.nudEndYear = new System.Windows.Forms.NumericUpDown();
            this.lable1 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndices)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartYear)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEndYear)).BeginInit();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // dgvIndices
            // 
            this.dgvIndices.AllowUserToAddRows = false;
            this.dgvIndices.AllowUserToDeleteRows = false;
            this.dgvIndices.AllowUserToOrderColumns = true;
            this.dgvIndices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvIndices.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvIndices.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvIndices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIndices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndexDescription,
            this.colReference,
            this.colUsage,
            this.colComment});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndices.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvIndices.Location = new System.Drawing.Point(9, 10);
            this.dgvIndices.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.dgvIndices.Name = "dgvIndices";
            this.dgvIndices.RowHeadersVisible = false;
            this.dgvIndices.ShowCellErrors = false;
            this.dgvIndices.ShowCellToolTips = false;
            this.dgvIndices.ShowEditingIcon = false;
            this.dgvIndices.ShowRowErrors = false;
            this.dgvIndices.Size = new System.Drawing.Size(627, 312);
            this.dgvIndices.TabIndex = 1;
            // 
            // colIndexDescription
            // 
            this.colIndexDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colIndexDescription.FillWeight = 15F;
            this.colIndexDescription.HeaderText = "Index";
            this.colIndexDescription.Name = "colIndexDescription";
            this.colIndexDescription.ReadOnly = true;
            this.colIndexDescription.Width = 250;
            // 
            // colReference
            // 
            this.colReference.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colReference.FillWeight = 40F;
            this.colReference.HeaderText = "Reference";
            this.colReference.Name = "colReference";
            this.colReference.ReadOnly = true;
            this.colReference.Width = 82;
            // 
            // colUsage
            // 
            this.colUsage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colUsage.FillWeight = 40F;
            this.colUsage.HeaderText = "Usage";
            this.colUsage.Name = "colUsage";
            this.colUsage.ReadOnly = true;
            this.colUsage.Width = 500;
            // 
            // colComment
            // 
            this.colComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colComment.FillWeight = 5F;
            this.colComment.HeaderText = "Comment";
            this.colComment.MinimumWidth = 200;
            this.colComment.Name = "colComment";
            this.colComment.ReadOnly = true;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Image = ((System.Drawing.Image)(resources.GetObject("btnClose.Image")));
            this.btnClose.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnClose.Location = new System.Drawing.Point(543, 331);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(93, 29);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnExtSearch
            // 
            this.btnExtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExtSearch.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnExtSearch.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExtSearch.Location = new System.Drawing.Point(380, 331);
            this.btnExtSearch.Name = "btnExtSearch";
            this.btnExtSearch.Size = new System.Drawing.Size(150, 29);
            this.btnExtSearch.TabIndex = 19;
            this.btnExtSearch.Text = "Go to Component Use";
            this.btnExtSearch.UseVisualStyleBackColor = true;
            this.btnExtSearch.Click += new System.EventHandler(this.btnExtSearch_Click);
            // 
            // nudStartYear
            // 
            this.nudStartYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudStartYear.Location = new System.Drawing.Point(39, 334);
            this.nudStartYear.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nudStartYear.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.nudStartYear.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudStartYear.Name = "nudStartYear";
            this.nudStartYear.Size = new System.Drawing.Size(42, 20);
            this.nudStartYear.TabIndex = 20;
            this.nudStartYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudStartYear.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudStartYear.ValueChanged += new System.EventHandler(this.nudStartYear_ValueChanged);
            // 
            // nudEndYear
            // 
            this.nudEndYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.nudEndYear.Location = new System.Drawing.Point(105, 334);
            this.nudEndYear.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nudEndYear.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.nudEndYear.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudEndYear.Name = "nudEndYear";
            this.nudEndYear.Size = new System.Drawing.Size(42, 20);
            this.nudEndYear.TabIndex = 21;
            this.nudEndYear.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.nudEndYear.Value = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.nudEndYear.ValueChanged += new System.EventHandler(this.nudEndYear_ValueChanged);
            // 
            // lable1
            // 
            this.lable1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lable1.AutoSize = true;
            this.lable1.Location = new System.Drawing.Point(9, 336);
            this.lable1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lable1.Name = "lable1";
            this.lable1.Size = new System.Drawing.Size(30, 13);
            this.lable1.TabIndex = 22;
            this.lable1.Text = "From";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(86, 336);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(16, 13);
            this.label1.TabIndex = 23;
            this.label1.Text = "to";
            // 
            // UpratingIndicesUsageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(645, 367);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lable1);
            this.Controls.Add(this.nudEndYear);
            this.Controls.Add(this.nudStartYear);
            this.Controls.Add(this.btnExtSearch);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.dgvIndices);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_DefiningUpratingFactors.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MinimizeBox = false;
            this.Name = "UpratingIndicesUsageForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Usage of Uprating Indices";
            this.Load += new System.EventHandler(this.UpratingIndicesUsageForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndices)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudStartYear)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudEndYear)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.DataGridView dgvIndices;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnExtSearch;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndexDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUsage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
        private System.Windows.Forms.NumericUpDown nudStartYear;
        private System.Windows.Forms.NumericUpDown nudEndYear;
        private System.Windows.Forms.Label lable1;
        private System.Windows.Forms.Label label1;
    }
}