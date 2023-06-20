namespace EM_UI.UpratingIndices
{
    partial class UpratingIndicesForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tabIndices = new System.Windows.Forms.TabControl();
            this.tabPageIndices = new System.Windows.Forms.TabPage();
            this.updwnYearToAdd = new System.Windows.Forms.NumericUpDown();
            this.cmbYearToDelete = new System.Windows.Forms.ComboBox();
            this.btnDeleteYear = new System.Windows.Forms.Button();
            this.btnAddYear = new System.Windows.Forms.Button();
            this.dgvIndices = new System.Windows.Forms.DataGridView();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colIndexDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colReference = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabPageFactors = new System.Windows.Forms.TabPage();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.txtIncomeYear = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbDatasets = new System.Windows.Forms.ComboBox();
            this.dgvFactors = new System.Windows.Forms.DataGridView();
            this.colIndexName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStripRows = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCheckUsage = new System.Windows.Forms.Button();
            this.addRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tabIndices.SuspendLayout();
            this.tabPageIndices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.updwnYearToAdd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndices)).BeginInit();
            this.tabPageFactors.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFactors)).BeginInit();
            this.contextMenuStripRows.SuspendLayout();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(485, 381);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "    Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(387, 381);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(92, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "    Save && Close";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tabIndices
            // 
            this.tabIndices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabIndices.Controls.Add(this.tabPageIndices);
            this.tabIndices.Controls.Add(this.tabPageFactors);
            this.tabIndices.Location = new System.Drawing.Point(9, 10);
            this.tabIndices.Margin = new System.Windows.Forms.Padding(2);
            this.tabIndices.Name = "tabIndices";
            this.tabIndices.SelectedIndex = 0;
            this.tabIndices.Size = new System.Drawing.Size(550, 366);
            this.tabIndices.TabIndex = 9;
            this.tabIndices.SelectedIndexChanged += new System.EventHandler(this.tabIndices_SelectedIndexChanged);
            // 
            // tabPageIndices
            // 
            this.tabPageIndices.Controls.Add(this.updwnYearToAdd);
            this.tabPageIndices.Controls.Add(this.cmbYearToDelete);
            this.tabPageIndices.Controls.Add(this.btnDeleteYear);
            this.tabPageIndices.Controls.Add(this.btnAddYear);
            this.tabPageIndices.Controls.Add(this.dgvIndices);
            this.tabPageIndices.Location = new System.Drawing.Point(4, 22);
            this.tabPageIndices.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageIndices.Name = "tabPageIndices";
            this.tabPageIndices.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageIndices.Size = new System.Drawing.Size(542, 340);
            this.tabPageIndices.TabIndex = 0;
            this.tabPageIndices.Text = "Raw Indices";
            this.tabPageIndices.UseVisualStyleBackColor = true;
            // 
            // updwnYearToAdd
            // 
            this.updwnYearToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updwnYearToAdd.Location = new System.Drawing.Point(373, 297);
            this.updwnYearToAdd.Margin = new System.Windows.Forms.Padding(2);
            this.updwnYearToAdd.Maximum = new decimal(new int[] {
            2500,
            0,
            0,
            0});
            this.updwnYearToAdd.Minimum = new decimal(new int[] {
            1900,
            0,
            0,
            0});
            this.updwnYearToAdd.Name = "updwnYearToAdd";
            this.updwnYearToAdd.Size = new System.Drawing.Size(81, 20);
            this.updwnYearToAdd.TabIndex = 10;
            this.updwnYearToAdd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.updwnYearToAdd.Value = new decimal(new int[] {
            2005,
            0,
            0,
            0});
            // 
            // cmbYearToDelete
            // 
            this.cmbYearToDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbYearToDelete.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbYearToDelete.FormattingEnabled = true;
            this.cmbYearToDelete.Location = new System.Drawing.Point(458, 297);
            this.cmbYearToDelete.Margin = new System.Windows.Forms.Padding(2);
            this.cmbYearToDelete.Name = "cmbYearToDelete";
            this.cmbYearToDelete.Size = new System.Drawing.Size(82, 21);
            this.cmbYearToDelete.TabIndex = 9;
            // 
            // btnDeleteYear
            // 
            this.btnDeleteYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteYear.Location = new System.Drawing.Point(458, 318);
            this.btnDeleteYear.Margin = new System.Windows.Forms.Padding(2);
            this.btnDeleteYear.Name = "btnDeleteYear";
            this.btnDeleteYear.Size = new System.Drawing.Size(81, 20);
            this.btnDeleteYear.TabIndex = 8;
            this.btnDeleteYear.Text = "Delete Year";
            this.btnDeleteYear.UseVisualStyleBackColor = true;
            this.btnDeleteYear.Click += new System.EventHandler(this.btnDeleteYear_Click);
            // 
            // btnAddYear
            // 
            this.btnAddYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddYear.Location = new System.Drawing.Point(372, 318);
            this.btnAddYear.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddYear.Name = "btnAddYear";
            this.btnAddYear.Size = new System.Drawing.Size(81, 20);
            this.btnAddYear.TabIndex = 5;
            this.btnAddYear.Text = "Add Year";
            this.btnAddYear.UseVisualStyleBackColor = true;
            this.btnAddYear.Click += new System.EventHandler(this.btnAddYear_Click);
            // 
            // dgvIndices
            // 
            this.dgvIndices.AllowUserToDeleteRows = false;
            this.dgvIndices.AllowUserToOrderColumns = true;
            this.dgvIndices.AllowUserToResizeRows = false;
            this.dgvIndices.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvIndices.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvIndices.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvIndices.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvIndices.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colIndexDescription,
            this.colReference,
            this.colComment});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvIndices.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvIndices.Location = new System.Drawing.Point(7, 0);
            this.dgvIndices.Margin = new System.Windows.Forms.Padding(2);
            this.dgvIndices.Name = "dgvIndices";
            this.dgvIndices.RowHeadersWidth = 62;
            this.dgvIndices.RowTemplate.Height = 24;
            this.dgvIndices.ShowCellErrors = false;
            this.dgvIndices.ShowEditingIcon = false;
            this.dgvIndices.ShowRowErrors = false;
            this.dgvIndices.Size = new System.Drawing.Size(535, 286);
            this.dgvIndices.TabIndex = 0;
            this.dgvIndices.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvIndices_CellEndEdit);
            this.dgvIndices.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dgvIndices_RowPostPaint);
            this.dgvIndices.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvIndices_KeyDown);
            this.dgvIndices.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvIndices_MouseClick);
            // 
            // colID
            // 
            this.colID.DataPropertyName = "ID";
            this.colID.Frozen = true;
            this.colID.MinimumWidth = 8;
            this.colID.Name = "colID";
            this.colID.Visible = false;
            this.colID.Width = 150;
            // 
            // colIndexDescription
            // 
            this.colIndexDescription.DataPropertyName = "colIndexDescription";
            this.colIndexDescription.FillWeight = 300F;
            this.colIndexDescription.Frozen = true;
            this.colIndexDescription.HeaderText = "Index";
            this.colIndexDescription.MinimumWidth = 8;
            this.colIndexDescription.Name = "colIndexDescription";
            this.colIndexDescription.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colIndexDescription.Width = 250;
            // 
            // colReference
            // 
            this.colReference.DataPropertyName = "colReference";
            this.colReference.FillWeight = 80F;
            this.colReference.Frozen = true;
            this.colReference.HeaderText = "Reference";
            this.colReference.MinimumWidth = 8;
            this.colReference.Name = "colReference";
            this.colReference.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colReference.Width = 150;
            // 
            // colComment
            // 
            this.colComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colComment.DataPropertyName = "colComment";
            this.colComment.HeaderText = "Comment";
            this.colComment.MinimumWidth = 200;
            this.colComment.Name = "colComment";
            this.colComment.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // tabPageFactors
            // 
            this.tabPageFactors.Controls.Add(this.btnUpdate);
            this.tabPageFactors.Controls.Add(this.txtIncomeYear);
            this.tabPageFactors.Controls.Add(this.label2);
            this.tabPageFactors.Controls.Add(this.label1);
            this.tabPageFactors.Controls.Add(this.cmbDatasets);
            this.tabPageFactors.Controls.Add(this.dgvFactors);
            this.tabPageFactors.Location = new System.Drawing.Point(4, 22);
            this.tabPageFactors.Margin = new System.Windows.Forms.Padding(2);
            this.tabPageFactors.Name = "tabPageFactors";
            this.tabPageFactors.Padding = new System.Windows.Forms.Padding(2);
            this.tabPageFactors.Size = new System.Drawing.Size(542, 340);
            this.tabPageFactors.TabIndex = 1;
            this.tabPageFactors.Text = "Factors per Data and System";
            this.tabPageFactors.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(343, 4);
            this.btnUpdate.Margin = new System.Windows.Forms.Padding(2);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(56, 20);
            this.btnUpdate.TabIndex = 5;
            this.btnUpdate.Text = "Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // txtIncomeYear
            // 
            this.txtIncomeYear.Enabled = false;
            this.txtIncomeYear.Location = new System.Drawing.Point(273, 6);
            this.txtIncomeYear.Margin = new System.Windows.Forms.Padding(2);
            this.txtIncomeYear.Name = "txtIncomeYear";
            this.txtIncomeYear.Size = new System.Drawing.Size(50, 20);
            this.txtIncomeYear.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(206, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Income Year";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Dataset";
            // 
            // cmbDatasets
            // 
            this.cmbDatasets.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbDatasets.FormattingEnabled = true;
            this.cmbDatasets.Location = new System.Drawing.Point(52, 5);
            this.cmbDatasets.Margin = new System.Windows.Forms.Padding(2);
            this.cmbDatasets.Name = "cmbDatasets";
            this.cmbDatasets.Size = new System.Drawing.Size(138, 21);
            this.cmbDatasets.TabIndex = 1;
            this.cmbDatasets.SelectedIndexChanged += new System.EventHandler(this.cmbDatasets_SelectedIndexChanged);
            // 
            // dgvFactors
            // 
            this.dgvFactors.AllowUserToAddRows = false;
            this.dgvFactors.AllowUserToDeleteRows = false;
            this.dgvFactors.AllowUserToResizeRows = false;
            this.dgvFactors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvFactors.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvFactors.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvFactors.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvFactors.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colIndexName});
            this.dgvFactors.Location = new System.Drawing.Point(4, 33);
            this.dgvFactors.Margin = new System.Windows.Forms.Padding(2);
            this.dgvFactors.Name = "dgvFactors";
            this.dgvFactors.RowHeadersVisible = false;
            this.dgvFactors.RowHeadersWidth = 62;
            this.dgvFactors.ShowCellErrors = false;
            this.dgvFactors.ShowCellToolTips = false;
            this.dgvFactors.ShowEditingIcon = false;
            this.dgvFactors.ShowRowErrors = false;
            this.dgvFactors.Size = new System.Drawing.Size(540, 310);
            this.dgvFactors.TabIndex = 0;
            // 
            // colIndexName
            // 
            this.colIndexName.HeaderText = "Index";
            this.colIndexName.MinimumWidth = 8;
            this.colIndexName.Name = "colIndexName";
            this.colIndexName.ReadOnly = true;
            this.colIndexName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colIndexName.Width = 150;
            // 
            // contextMenuStripRows
            // 
            this.contextMenuStripRows.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripRows.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteRowToolStripMenuItem,
            this.addRowToolStripMenuItem});
            this.contextMenuStripRows.Name = "contextMenuStripRows";
            this.contextMenuStripRows.Size = new System.Drawing.Size(241, 101);
            // 
            // deleteRowToolStripMenuItem
            // 
            this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
            this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.deleteRowToolStripMenuItem.Text = "Delete Row(s)";
            this.deleteRowToolStripMenuItem.Click += new System.EventHandler(this.deleteRowToolStripMenuItem_Click);
            // 
            // btnCheckUsage
            // 
            this.btnCheckUsage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCheckUsage.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCheckUsage.Location = new System.Drawing.Point(9, 381);
            this.btnCheckUsage.Name = "btnCheckUsage";
            this.btnCheckUsage.Size = new System.Drawing.Size(81, 23);
            this.btnCheckUsage.TabIndex = 10;
            this.btnCheckUsage.Text = "Check Usage";
            this.btnCheckUsage.UseVisualStyleBackColor = true;
            this.btnCheckUsage.Click += new System.EventHandler(this.btnCheckUsage_Click);
            // 
            // addRowToolStripMenuItem
            // 
            this.addRowToolStripMenuItem.Name = "addRowToolStripMenuItem";
            this.addRowToolStripMenuItem.Size = new System.Drawing.Size(240, 32);
            this.addRowToolStripMenuItem.Text = "Add Row(s)";
            this.addRowToolStripMenuItem.Click += new System.EventHandler(this.addRowToolStripMenuItem_Click);
            // 
            // UpratingIndicesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(568, 414);
            this.Controls.Add(this.btnCheckUsage);
            this.Controls.Add(this.tabIndices);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_DefiningUpratingFactors.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimizeBox = false;
            this.Name = "UpratingIndicesForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Uprating Indices";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.UpratingIndicesForm_FormClosing);
            this.Load += new System.EventHandler(this.UpratingIndicesForm_Load);
            this.tabIndices.ResumeLayout(false);
            this.tabPageIndices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.updwnYearToAdd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvIndices)).EndInit();
            this.tabPageFactors.ResumeLayout(false);
            this.tabPageFactors.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvFactors)).EndInit();
            this.contextMenuStripRows.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TabControl tabIndices;
        private System.Windows.Forms.TabPage tabPageIndices;
        private System.Windows.Forms.TabPage tabPageFactors;
        private System.Windows.Forms.DataGridView dgvIndices;
        private System.Windows.Forms.Button btnDeleteYear;
        private System.Windows.Forms.Button btnAddYear;
        private System.Windows.Forms.ComboBox cmbYearToDelete;
        private System.Windows.Forms.NumericUpDown updwnYearToAdd;
        private System.Windows.Forms.DataGridView dgvFactors;
        private System.Windows.Forms.TextBox txtIncomeYear;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbDatasets;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndexName;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colIndexDescription;
        private System.Windows.Forms.DataGridViewTextBoxColumn colReference;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRows;
        private System.Windows.Forms.ToolStripMenuItem deleteRowToolStripMenuItem;
        private System.Windows.Forms.Button btnCheckUsage;
        private System.Windows.Forms.ToolStripMenuItem addRowToolStripMenuItem;
    }
}