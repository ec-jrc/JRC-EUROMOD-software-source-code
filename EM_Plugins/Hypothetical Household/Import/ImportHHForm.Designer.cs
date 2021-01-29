namespace HypotheticalHousehold
{
    partial class ImportHHForm
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
            this.btnGetNone = new DevExpress.XtraEditors.SimpleButton();
            this.btnGetAll = new DevExpress.XtraEditors.SimpleButton();
            this.btnImport = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.gridHH = new System.Windows.Forms.DataGridView();
            this.colHHName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGet = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridHH)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGetNone
            // 
            this.btnGetNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetNone.Location = new System.Drawing.Point(78, 370);
            this.btnGetNone.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetNone.Name = "btnGetNone";
            this.btnGetNone.Size = new System.Drawing.Size(62, 28);
            this.btnGetNone.TabIndex = 10;
            this.btnGetNone.Text = "Get None";
            this.btnGetNone.Click += new System.EventHandler(this.btnGetNone_Click);
            // 
            // btnGetAll
            // 
            this.btnGetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetAll.Location = new System.Drawing.Point(11, 370);
            this.btnGetAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetAll.Name = "btnGetAll";
            this.btnGetAll.Size = new System.Drawing.Size(62, 28);
            this.btnGetAll.TabIndex = 9;
            this.btnGetAll.Text = "Get All";
            this.btnGetAll.Click += new System.EventHandler(this.btnGetAll_Click);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImport.Location = new System.Drawing.Point(395, 366);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(73, 36);
            this.btnImport.TabIndex = 8;
            this.btnImport.Text = "Import";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(476, 366);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(73, 36);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            // 
            // gridHH
            // 
            this.gridHH.AllowUserToAddRows = false;
            this.gridHH.AllowUserToDeleteRows = false;
            this.gridHH.AllowUserToResizeRows = false;
            this.gridHH.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridHH.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridHH.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridHH.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colHHName,
            this.colGet,
            this.colDescription});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridHH.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridHH.Location = new System.Drawing.Point(12, 12);
            this.gridHH.Name = "gridHH";
            this.gridHH.RowHeadersVisible = false;
            this.gridHH.RowTemplate.Height = 24;
            this.gridHH.Size = new System.Drawing.Size(535, 347);
            this.gridHH.TabIndex = 11;
            // 
            // colHHName
            // 
            this.colHHName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colHHName.HeaderText = "Name";
            this.colHHName.Name = "colHHName";
            this.colHHName.ReadOnly = true;
            this.colHHName.Width = 74;
            // 
            // colGet
            // 
            this.colGet.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colGet.HeaderText = "Get";
            this.colGet.Name = "colGet";
            this.colGet.Width = 37;
            // 
            // colDescription
            // 
            this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDescription.HeaderText = "Description";
            this.colDescription.Name = "colDescription";
            this.colDescription.ReadOnly = true;
            // 
            // ImportHHForm
            // 
            this.AcceptButton = this.btnImport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(559, 411);
            this.Controls.Add(this.gridHH);
            this.Controls.Add(this.btnGetNone);
            this.Controls.Add(this.btnGetAll);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnCancel);
            this.Name = "ImportHHForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Households";
            ((System.ComponentModel.ISupportInitialize)(this.gridHH)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.SimpleButton btnGetNone;
        private DevExpress.XtraEditors.SimpleButton btnGetAll;
        private DevExpress.XtraEditors.SimpleButton btnImport;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private System.Windows.Forms.DataGridView gridHH;
        private System.Windows.Forms.DataGridViewTextBoxColumn colHHName;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGet;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDescription;
    }
}