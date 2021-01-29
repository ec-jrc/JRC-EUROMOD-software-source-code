namespace HypotheticalHousehold
{
    partial class ImportRefTablesForm
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
            this.gridRefTabs = new System.Windows.Forms.DataGridView();
            this.btnGetNone = new DevExpress.XtraEditors.SimpleButton();
            this.btnGetAll = new DevExpress.XtraEditors.SimpleButton();
            this.btnImport = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.colTable = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGet = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colInfo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.gridRefTabs)).BeginInit();
            this.SuspendLayout();
            // 
            // gridRefTabs
            // 
            this.gridRefTabs.AllowUserToAddRows = false;
            this.gridRefTabs.AllowUserToDeleteRows = false;
            this.gridRefTabs.AllowUserToResizeRows = false;
            this.gridRefTabs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridRefTabs.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridRefTabs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridRefTabs.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colTable,
            this.colGet,
            this.colInfo});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridRefTabs.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridRefTabs.Location = new System.Drawing.Point(11, 10);
            this.gridRefTabs.Name = "gridRefTabs";
            this.gridRefTabs.RowHeadersVisible = false;
            this.gridRefTabs.RowTemplate.Height = 24;
            this.gridRefTabs.Size = new System.Drawing.Size(535, 347);
            this.gridRefTabs.TabIndex = 16;
            // 
            // btnGetNone
            // 
            this.btnGetNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetNone.Location = new System.Drawing.Point(77, 368);
            this.btnGetNone.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetNone.Name = "btnGetNone";
            this.btnGetNone.Size = new System.Drawing.Size(62, 28);
            this.btnGetNone.TabIndex = 15;
            this.btnGetNone.Text = "Get None";
            this.btnGetNone.Click += new System.EventHandler(this.btnGetNone_Click);
            // 
            // btnGetAll
            // 
            this.btnGetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetAll.Location = new System.Drawing.Point(10, 368);
            this.btnGetAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetAll.Name = "btnGetAll";
            this.btnGetAll.Size = new System.Drawing.Size(62, 28);
            this.btnGetAll.TabIndex = 14;
            this.btnGetAll.Text = "Get All";
            this.btnGetAll.Click += new System.EventHandler(this.btnGetAll_Click);
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImport.Location = new System.Drawing.Point(394, 364);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(73, 36);
            this.btnImport.TabIndex = 13;
            this.btnImport.Text = "Import";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(475, 364);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(73, 36);
            this.btnCancel.TabIndex = 12;
            this.btnCancel.Text = "Cancel";
            // 
            // colTable
            // 
            this.colTable.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colTable.HeaderText = "Table";
            this.colTable.Name = "colTable";
            this.colTable.ReadOnly = true;
            this.colTable.Width = 73;
            // 
            // colGet
            // 
            this.colGet.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colGet.HeaderText = "Get";
            this.colGet.Name = "colGet";
            this.colGet.Width = 37;
            // 
            // colInfo
            // 
            this.colInfo.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colInfo.HeaderText = "Info";
            this.colInfo.Name = "colInfo";
            this.colInfo.ReadOnly = true;
            // 
            // ImportRefTablesForm
            // 
            this.AcceptButton = this.btnImport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(559, 411);
            this.Controls.Add(this.gridRefTabs);
            this.Controls.Add(this.btnGetNone);
            this.Controls.Add(this.btnGetAll);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnCancel);
            this.Name = "ImportRefTablesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Reference Tables";
            ((System.ComponentModel.ISupportInitialize)(this.gridRefTabs)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView gridRefTabs;
        private DevExpress.XtraEditors.SimpleButton btnGetNone;
        private DevExpress.XtraEditors.SimpleButton btnGetAll;
        private DevExpress.XtraEditors.SimpleButton btnImport;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colTable;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGet;
        private System.Windows.Forms.DataGridViewTextBoxColumn colInfo;
    }
}