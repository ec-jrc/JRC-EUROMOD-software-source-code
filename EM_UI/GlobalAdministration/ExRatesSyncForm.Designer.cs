namespace EM_UI.GlobalAdministration
{
    partial class ExRatesSyncForm
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
            this.dgvDiff = new System.Windows.Forms.DataGridView();
            this.colCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRateCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRateGlobal = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTakeCountry = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colTakeGlobal = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDefault = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colHint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDiff)).BeginInit();
            this.SuspendLayout();
            // 
            // dgvDiff
            // 
            this.dgvDiff.AllowUserToAddRows = false;
            this.dgvDiff.AllowUserToDeleteRows = false;
            this.dgvDiff.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDiff.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDiff.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDiff.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCountry,
            this.colSystem,
            this.colRateCountry,
            this.colRateGlobal,
            this.colTakeCountry,
            this.colTakeGlobal,
            this.colDefault,
            this.colHint});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvDiff.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDiff.Location = new System.Drawing.Point(12, 12);
            this.dgvDiff.Name = "dgvDiff";
            this.dgvDiff.RowHeadersVisible = false;
            this.dgvDiff.RowTemplate.Height = 24;
            this.dgvDiff.Size = new System.Drawing.Size(838, 424);
            this.dgvDiff.TabIndex = 0;
            this.dgvDiff.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDiff_CellContentClick);
            this.dgvDiff.EditingControlShowing += new System.Windows.Forms.DataGridViewEditingControlShowingEventHandler(this.dgvDiff_EditingControlShowing);
            // 
            // colCountry
            // 
            this.colCountry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colCountry.HeaderText = "Country";
            this.colCountry.Name = "colCountry";
            this.colCountry.ReadOnly = true;
            this.colCountry.Width = 86;
            // 
            // colSystem
            // 
            this.colSystem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colSystem.HeaderText = "System";
            this.colSystem.Name = "colSystem";
            this.colSystem.ReadOnly = true;
            this.colSystem.Width = 83;
            // 
            // colRateCountry
            // 
            this.colRateCountry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colRateCountry.HeaderText = "Rate Country";
            this.colRateCountry.Name = "colRateCountry";
            this.colRateCountry.ReadOnly = true;
            this.colRateCountry.Width = 110;
            // 
            // colRateGlobal
            // 
            this.colRateGlobal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colRateGlobal.HeaderText = "Rate Global";
            this.colRateGlobal.Name = "colRateGlobal";
            this.colRateGlobal.ReadOnly = true;
            this.colRateGlobal.Width = 103;
            // 
            // colTakeCountry
            // 
            this.colTakeCountry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colTakeCountry.HeaderText = "Take Country";
            this.colTakeCountry.Name = "colTakeCountry";
            this.colTakeCountry.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colTakeCountry.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.colTakeCountry.Width = 112;
            // 
            // colTakeGlobal
            // 
            this.colTakeGlobal.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colTakeGlobal.HeaderText = "Take Global";
            this.colTakeGlobal.Name = "colTakeGlobal";
            this.colTakeGlobal.Width = 82;
            // 
            // colDefault
            // 
            this.colDefault.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDefault.HeaderText = "Rate Type";
            this.colDefault.Items.AddRange(new object[] {
            "June 30",
            "Year Average"/*,
            "First Semester",
            "Second Semester"*/});
            this.colDefault.Name = "colDefault";
            this.colDefault.Width = 72;
            // 
            // colHint
            // 
            this.colHint.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colHint.HeaderText = "Hint";
            this.colHint.Name = "colHint";
            this.colHint.ReadOnly = true;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(435, 443);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(88, 27);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "    Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.AutoSize = true;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(339, 443);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(88, 28);
            this.btnOK.TabIndex = 11;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // ExRatesSyncForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(862, 483);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.dgvDiff);
            this.Name = "ExRatesSyncForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Synchronise Exchange Rates";
            ((System.ComponentModel.ISupportInitialize)(this.dgvDiff)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDiff;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCountry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSystem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRateCountry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRateGlobal;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colTakeCountry;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colTakeGlobal;
        private System.Windows.Forms.DataGridViewComboBoxColumn colDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHint;
    }
}