namespace EM_UI.GlobalAdministration
{
    partial class HICPForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.updwnYearToAdd = new System.Windows.Forms.NumericUpDown();
            this.btnAddYear = new System.Windows.Forms.Button();
            this.cmbYearToDelete = new System.Windows.Forms.ComboBox();
            this.btnDeleteYear = new System.Windows.Forms.Button();
            this.btnDeleteCountryRow = new System.Windows.Forms.Button();
            this.cmbCountryToDelete = new System.Windows.Forms.ComboBox();
            this.dgvHICP = new EM_Common_Win.CustomDataGrid();
            ((System.ComponentModel.ISupportInitialize)(this.updwnYearToAdd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHICP)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(848, 48);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(118, 27);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "    Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(848, 12);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(118, 28);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "    Save && Close";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // updwnYearToAdd
            // 
            this.updwnYearToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.updwnYearToAdd.Location = new System.Drawing.Point(620, 676);
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
            this.updwnYearToAdd.Size = new System.Drawing.Size(107, 22);
            this.updwnYearToAdd.TabIndex = 12;
            this.updwnYearToAdd.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.updwnYearToAdd.Value = new decimal(new int[] {
            2005,
            0,
            0,
            0});
            // 
            // btnAddYear
            // 
            this.btnAddYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddYear.Location = new System.Drawing.Point(619, 702);
            this.btnAddYear.Name = "btnAddYear";
            this.btnAddYear.Size = new System.Drawing.Size(108, 24);
            this.btnAddYear.TabIndex = 11;
            this.btnAddYear.Text = "Add Year";
            this.btnAddYear.UseVisualStyleBackColor = true;
            this.btnAddYear.Click += new System.EventHandler(this.btnAddYear_Click);
            // 
            // cmbYearToDelete
            // 
            this.cmbYearToDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbYearToDelete.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbYearToDelete.FormattingEnabled = true;
            this.cmbYearToDelete.Location = new System.Drawing.Point(733, 676);
            this.cmbYearToDelete.Name = "cmbYearToDelete";
            this.cmbYearToDelete.Size = new System.Drawing.Size(108, 24);
            this.cmbYearToDelete.Sorted = true;
            this.cmbYearToDelete.TabIndex = 15;
            // 
            // btnDeleteYear
            // 
            this.btnDeleteYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteYear.Location = new System.Drawing.Point(733, 702);
            this.btnDeleteYear.Name = "btnDeleteYear";
            this.btnDeleteYear.Size = new System.Drawing.Size(108, 24);
            this.btnDeleteYear.TabIndex = 14;
            this.btnDeleteYear.Text = "Delete Year";
            this.btnDeleteYear.UseVisualStyleBackColor = true;
            this.btnDeleteYear.Click += new System.EventHandler(this.btnDeleteYear_Click);
            // 
            // btnDeleteCountryRow
            // 
            this.btnDeleteCountryRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteCountryRow.Location = new System.Drawing.Point(457, 702);
            this.btnDeleteCountryRow.Name = "btnDeleteCountryRow";
            this.btnDeleteCountryRow.Size = new System.Drawing.Size(141, 24);
            this.btnDeleteCountryRow.TabIndex = 16;
            this.btnDeleteCountryRow.Text = "Delete Country Row";
            this.btnDeleteCountryRow.UseVisualStyleBackColor = true;
            this.btnDeleteCountryRow.Click += new System.EventHandler(this.btnDeleteCountryRow_Click);
            // 
            // cmbCountryToDelete
            // 
            this.cmbCountryToDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCountryToDelete.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCountryToDelete.FormattingEnabled = true;
            this.cmbCountryToDelete.Location = new System.Drawing.Point(457, 676);
            this.cmbCountryToDelete.Name = "cmbCountryToDelete";
            this.cmbCountryToDelete.Size = new System.Drawing.Size(141, 24);
            this.cmbCountryToDelete.Sorted = true;
            this.cmbCountryToDelete.TabIndex = 17;
            // 
            // dgvHICP
            // 
            this.dgvHICP.AllowUserToAddRows = false;
            this.dgvHICP.AllowUserToDeleteRows = false;
            this.dgvHICP.AllowUserToResizeRows = false;
            this.dgvHICP.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvHICP.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvHICP.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvHICP.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvHICP.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvHICP.Location = new System.Drawing.Point(12, 12);
            this.dgvHICP.Name = "dgvHICP";
            this.dgvHICP.RowTemplate.Height = 24;
            this.dgvHICP.ShowCellErrors = false;
            this.dgvHICP.ShowEditingIcon = false;
            this.dgvHICP.ShowRowErrors = false;
            this.dgvHICP.Size = new System.Drawing.Size(829, 658);
            this.dgvHICP.TabIndex = 13;
            // 
            // HICPForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(979, 738);
            this.Controls.Add(this.cmbCountryToDelete);
            this.Controls.Add(this.btnDeleteCountryRow);
            this.Controls.Add(this.cmbYearToDelete);
            this.Controls.Add(this.btnDeleteYear);
            this.Controls.Add(this.dgvHICP);
            this.Controls.Add(this.updwnYearToAdd);
            this.Controls.Add(this.btnAddYear);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "HICPForm";
            this.ShowIcon = false;
            this.Text = "Harmonised Index of Consumer Prices";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HICPForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.updwnYearToAdd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHICP)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.NumericUpDown updwnYearToAdd;
        private System.Windows.Forms.Button btnAddYear;
        private EM_Common_Win.CustomDataGrid dgvHICP;
        private System.Windows.Forms.ComboBox cmbYearToDelete;
        private System.Windows.Forms.Button btnDeleteYear;
        private System.Windows.Forms.Button btnDeleteCountryRow;
        private System.Windows.Forms.ComboBox cmbCountryToDelete;
    }
}