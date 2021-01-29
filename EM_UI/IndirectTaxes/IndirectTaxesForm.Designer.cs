namespace EM_UI.IndirectTaxes
{
    partial class IndirectTaxesForm
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
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.updwnYearToAdd = new System.Windows.Forms.NumericUpDown();
            this.cmbYearToDelete = new System.Windows.Forms.ComboBox();
            this.btnDeleteYear = new System.Windows.Forms.Button();
            this.btnAddYear = new System.Windows.Forms.Button();
            this.table = new System.Windows.Forms.DataGridView();
            this.contextMenuStripRows = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteRowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.updwnYearToAdd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.table)).BeginInit();
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
            this.btnCancel.Location = new System.Drawing.Point(819, 516);
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
            this.btnOK.Location = new System.Drawing.Point(721, 516);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(92, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "    Save && Close";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // updwnYearToAdd
            // 
            this.updwnYearToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.updwnYearToAdd.Location = new System.Drawing.Point(9, 498);
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
            this.cmbYearToDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cmbYearToDelete.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbYearToDelete.FormattingEnabled = true;
            this.cmbYearToDelete.Location = new System.Drawing.Point(94, 498);
            this.cmbYearToDelete.Margin = new System.Windows.Forms.Padding(2);
            this.cmbYearToDelete.Name = "cmbYearToDelete";
            this.cmbYearToDelete.Size = new System.Drawing.Size(82, 21);
            this.cmbYearToDelete.TabIndex = 9;
            // 
            // btnDeleteYear
            // 
            this.btnDeleteYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteYear.Location = new System.Drawing.Point(94, 519);
            this.btnDeleteYear.Margin = new System.Windows.Forms.Padding(2);
            this.btnDeleteYear.Name = "btnDeleteYear";
            this.btnDeleteYear.Size = new System.Drawing.Size(81, 22);
            this.btnDeleteYear.TabIndex = 8;
            this.btnDeleteYear.Text = "Delete Year";
            this.btnDeleteYear.UseVisualStyleBackColor = true;
            this.btnDeleteYear.Click += new System.EventHandler(this.btnDeleteYear_Click);
            // 
            // btnAddYear
            // 
            this.btnAddYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddYear.Location = new System.Drawing.Point(8, 519);
            this.btnAddYear.Margin = new System.Windows.Forms.Padding(2);
            this.btnAddYear.Name = "btnAddYear";
            this.btnAddYear.Size = new System.Drawing.Size(81, 22);
            this.btnAddYear.TabIndex = 5;
            this.btnAddYear.Text = "Add Year";
            this.btnAddYear.UseVisualStyleBackColor = true;
            this.btnAddYear.Click += new System.EventHandler(this.btnAddYear_Click);
            // 
            // table
            // 
            this.table.AllowUserToDeleteRows = false;
            this.table.AllowUserToOrderColumns = true;
            this.table.AllowUserToResizeRows = false;
            this.table.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.table.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.table.BackgroundColor = System.Drawing.SystemColors.Control;
            this.table.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.table.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colID,
            this.colName,
            this.colComment});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.table.DefaultCellStyle = dataGridViewCellStyle1;
            this.table.Location = new System.Drawing.Point(9, 11);
            this.table.Margin = new System.Windows.Forms.Padding(2);
            this.table.Name = "table";
            this.table.RowTemplate.Height = 24;
            this.table.ShowCellErrors = false;
            this.table.ShowEditingIcon = false;
            this.table.ShowRowErrors = false;
            this.table.Size = new System.Drawing.Size(876, 483);
            this.table.TabIndex = 0;
            this.table.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.table_CellEndEdit);
            this.table.KeyDown += new System.Windows.Forms.KeyEventHandler(this.table_KeyDown);
            this.table.MouseClick += new System.Windows.Forms.MouseEventHandler(this.table_MouseClick);
            // 
            // contextMenuStripRows
            // 
            this.contextMenuStripRows.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStripRows.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteRowToolStripMenuItem});
            this.contextMenuStripRows.Name = "contextMenuStripRows";
            this.contextMenuStripRows.Size = new System.Drawing.Size(147, 26);
            // 
            // deleteRowToolStripMenuItem
            // 
            this.deleteRowToolStripMenuItem.Name = "deleteRowToolStripMenuItem";
            this.deleteRowToolStripMenuItem.Size = new System.Drawing.Size(146, 22);
            this.deleteRowToolStripMenuItem.Text = "Delete Row(s)";
            this.deleteRowToolStripMenuItem.Click += new System.EventHandler(this.deleteRowToolStripMenuItem_Click);
            // 
            // colID
            // 
            this.colID.DataPropertyName = "ID";
            this.colID.HeaderText = "";
            this.colID.Name = "colID";
            this.colID.Visible = false;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "colName";
            this.colName.FillWeight = 300F;
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colName.Width = 250;
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
            // IndirectTaxesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(895, 547);
            this.Controls.Add(this.updwnYearToAdd);
            this.Controls.Add(this.cmbYearToDelete);
            this.Controls.Add(this.btnDeleteYear);
            this.Controls.Add(this.btnAddYear);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.table);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_DefiningIndirectTaxes.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimizeBox = false;
            this.Name = "IndirectTaxesForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Indirect Taxes - Tax Rates and Excises Prices";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IndirectTaxesForm_FormClosing);
            this.Load += new System.EventHandler(this.IndirectTaxesForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.updwnYearToAdd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.table)).EndInit();
            this.contextMenuStripRows.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView table;
        private System.Windows.Forms.Button btnDeleteYear;
        private System.Windows.Forms.Button btnAddYear;
        private System.Windows.Forms.ComboBox cmbYearToDelete;
        private System.Windows.Forms.NumericUpDown updwnYearToAdd;
        private System.Windows.Forms.ContextMenuStrip contextMenuStripRows;
        private System.Windows.Forms.ToolStripMenuItem deleteRowToolStripMenuItem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}