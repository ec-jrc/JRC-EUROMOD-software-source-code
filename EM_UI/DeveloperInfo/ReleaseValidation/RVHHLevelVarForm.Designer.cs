namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    partial class RVHHLevelVarForm
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
            this.btnClose = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.dgvVar = new System.Windows.Forms.DataGridView();
            this.colVar = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvVar)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(729, 409);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(59, 29);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(729, 12);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(59, 29);
            this.btnSave.TabIndex = 1;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // dgvVar
            // 
            this.dgvVar.AllowUserToAddRows = false;
            this.dgvVar.AllowUserToDeleteRows = false;
            this.dgvVar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvVar.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dgvVar.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvVar.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvVar.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVar,
            this.colData});
            this.dgvVar.Location = new System.Drawing.Point(12, 12);
            this.dgvVar.Name = "dgvVar";
            this.dgvVar.RowHeadersVisible = false;
            this.dgvVar.RowTemplate.DefaultCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.TopLeft;
            this.dgvVar.RowTemplate.DefaultCellStyle.SelectionBackColor = System.Drawing.SystemColors.Window;
            this.dgvVar.RowTemplate.DefaultCellStyle.SelectionForeColor = System.Drawing.Color.Black;
            this.dgvVar.RowTemplate.DefaultCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvVar.RowTemplate.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvVar.Size = new System.Drawing.Size(711, 426);
            this.dgvVar.TabIndex = 2;
            // 
            // colVar
            // 
            this.colVar.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colVar.HeaderText = "Variable";
            this.colVar.Name = "colVar";
            this.colVar.ReadOnly = true;
            this.colVar.Width = 70;
            // 
            // colData
            // 
            this.colData.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colData.HeaderText = "Incorrect in Data";
            this.colData.Name = "colData";
            this.colData.ReadOnly = true;
            // 
            // RVHHLevelVarForm
            // 
            this.AcceptButton = this.btnClose;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgvVar);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnClose);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RVHHLevelVarForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Household Level Variables Check";
            this.Shown += new System.EventHandler(this.RVHHLevelVarForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvVar)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.DataGridView dgvVar;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVar;
        private System.Windows.Forms.DataGridViewTextBoxColumn colData;
    }
}