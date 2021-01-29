namespace HypotheticalHousehold
{
    partial class ImportVariablesForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabBasic = new System.Windows.Forms.TabPage();
            this.gridBasic = new System.Windows.Forms.DataGridView();
            this.colVarBasic = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGetBasic = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colStatusBasic = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabAdvanced = new System.Windows.Forms.TabPage();
            this.gridAdvanced = new System.Windows.Forms.DataGridView();
            this.colVarAdvanced = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGetAdvanced = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colStatusAdvanced = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabAdCSpec = new System.Windows.Forms.TabPage();
            this.gridAdvCSpec = new System.Windows.Forms.DataGridView();
            this.colVarAdvCSpec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGetAdvCSpec = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colStatusAdvCSpec = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabDerived = new System.Windows.Forms.TabPage();
            this.gridDerived = new System.Windows.Forms.DataGridView();
            this.colVarDerived = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGetDerived = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colStatusDerived = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnImport = new DevExpress.XtraEditors.SimpleButton();
            this.btnCancel = new DevExpress.XtraEditors.SimpleButton();
            this.btnGetAll = new DevExpress.XtraEditors.SimpleButton();
            this.btnGetNone = new DevExpress.XtraEditors.SimpleButton();
            this.tabControl.SuspendLayout();
            this.tabBasic.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridBasic)).BeginInit();
            this.tabAdvanced.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAdvanced)).BeginInit();
            this.tabAdCSpec.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAdvCSpec)).BeginInit();
            this.tabDerived.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDerived)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabBasic);
            this.tabControl.Controls.Add(this.tabAdvanced);
            this.tabControl.Controls.Add(this.tabAdCSpec);
            this.tabControl.Controls.Add(this.tabDerived);
            this.tabControl.Location = new System.Drawing.Point(12, 12);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(319, 351);
            this.tabControl.TabIndex = 0;
            // 
            // tabBasic
            // 
            this.tabBasic.Controls.Add(this.gridBasic);
            this.tabBasic.Location = new System.Drawing.Point(4, 25);
            this.tabBasic.Name = "tabBasic";
            this.tabBasic.Padding = new System.Windows.Forms.Padding(3);
            this.tabBasic.Size = new System.Drawing.Size(311, 322);
            this.tabBasic.TabIndex = 0;
            this.tabBasic.Text = "Basic";
            this.tabBasic.UseVisualStyleBackColor = true;
            // 
            // gridBasic
            // 
            this.gridBasic.AllowUserToAddRows = false;
            this.gridBasic.AllowUserToDeleteRows = false;
            this.gridBasic.AllowUserToResizeRows = false;
            this.gridBasic.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridBasic.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridBasic.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridBasic.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVarBasic,
            this.colGetBasic,
            this.colStatusBasic});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridBasic.DefaultCellStyle = dataGridViewCellStyle1;
            this.gridBasic.Location = new System.Drawing.Point(6, 6);
            this.gridBasic.Name = "gridBasic";
            this.gridBasic.RowHeadersVisible = false;
            this.gridBasic.RowTemplate.Height = 24;
            this.gridBasic.Size = new System.Drawing.Size(299, 313);
            this.gridBasic.TabIndex = 0;
            this.gridBasic.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
            // 
            // colVarBasic
            // 
            this.colVarBasic.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVarBasic.HeaderText = "Variable";
            this.colVarBasic.Name = "colVarBasic";
            this.colVarBasic.ReadOnly = true;
            this.colVarBasic.Width = 89;
            // 
            // colGetBasic
            // 
            this.colGetBasic.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colGetBasic.HeaderText = "Get";
            this.colGetBasic.Name = "colGetBasic";
            this.colGetBasic.Width = 37;
            // 
            // colStatusBasic
            // 
            this.colStatusBasic.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusBasic.HeaderText = "";
            this.colStatusBasic.Name = "colStatusBasic";
            this.colStatusBasic.ReadOnly = true;
            // 
            // tabAdvanced
            // 
            this.tabAdvanced.Controls.Add(this.gridAdvanced);
            this.tabAdvanced.Location = new System.Drawing.Point(4, 25);
            this.tabAdvanced.Name = "tabAdvanced";
            this.tabAdvanced.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdvanced.Size = new System.Drawing.Size(311, 322);
            this.tabAdvanced.TabIndex = 1;
            this.tabAdvanced.Text = "Advanced";
            this.tabAdvanced.UseVisualStyleBackColor = true;
            // 
            // gridAdvanced
            // 
            this.gridAdvanced.AllowUserToAddRows = false;
            this.gridAdvanced.AllowUserToDeleteRows = false;
            this.gridAdvanced.AllowUserToResizeRows = false;
            this.gridAdvanced.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAdvanced.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAdvanced.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAdvanced.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVarAdvanced,
            this.colGetAdvanced,
            this.colStatusAdvanced});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAdvanced.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridAdvanced.Location = new System.Drawing.Point(6, 4);
            this.gridAdvanced.Name = "gridAdvanced";
            this.gridAdvanced.RowHeadersVisible = false;
            this.gridAdvanced.RowTemplate.Height = 24;
            this.gridAdvanced.Size = new System.Drawing.Size(296, 308);
            this.gridAdvanced.TabIndex = 1;
            this.gridAdvanced.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
            // 
            // colVarAdvanced
            // 
            this.colVarAdvanced.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVarAdvanced.HeaderText = "Variable";
            this.colVarAdvanced.Name = "colVarAdvanced";
            this.colVarAdvanced.ReadOnly = true;
            this.colVarAdvanced.Width = 89;
            // 
            // colGetAdvanced
            // 
            this.colGetAdvanced.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colGetAdvanced.HeaderText = "Get";
            this.colGetAdvanced.Name = "colGetAdvanced";
            this.colGetAdvanced.Width = 37;
            // 
            // colStatusAdvanced
            // 
            this.colStatusAdvanced.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusAdvanced.HeaderText = "";
            this.colStatusAdvanced.Name = "colStatusAdvanced";
            this.colStatusAdvanced.ReadOnly = true;
            // 
            // tabAdCSpec
            // 
            this.tabAdCSpec.Controls.Add(this.gridAdvCSpec);
            this.tabAdCSpec.Location = new System.Drawing.Point(4, 25);
            this.tabAdCSpec.Name = "tabAdCSpec";
            this.tabAdCSpec.Padding = new System.Windows.Forms.Padding(3);
            this.tabAdCSpec.Size = new System.Drawing.Size(311, 322);
            this.tabAdCSpec.TabIndex = 2;
            this.tabAdCSpec.Text = "Adv. Country Spec.";
            this.tabAdCSpec.ToolTipText = "Advanced Country Specific";
            this.tabAdCSpec.UseVisualStyleBackColor = true;
            // 
            // gridAdvCSpec
            // 
            this.gridAdvCSpec.AllowUserToAddRows = false;
            this.gridAdvCSpec.AllowUserToDeleteRows = false;
            this.gridAdvCSpec.AllowUserToResizeRows = false;
            this.gridAdvCSpec.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridAdvCSpec.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridAdvCSpec.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridAdvCSpec.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVarAdvCSpec,
            this.colGetAdvCSpec,
            this.colStatusAdvCSpec});
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAdvCSpec.DefaultCellStyle = dataGridViewCellStyle3;
            this.gridAdvCSpec.Location = new System.Drawing.Point(6, 4);
            this.gridAdvCSpec.Name = "gridAdvCSpec";
            this.gridAdvCSpec.RowHeadersVisible = false;
            this.gridAdvCSpec.RowTemplate.Height = 24;
            this.gridAdvCSpec.Size = new System.Drawing.Size(296, 308);
            this.gridAdvCSpec.TabIndex = 1;
            this.gridAdvCSpec.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
            // 
            // colVarAdvCSpec
            // 
            this.colVarAdvCSpec.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVarAdvCSpec.HeaderText = "Variable";
            this.colVarAdvCSpec.Name = "colVarAdvCSpec";
            this.colVarAdvCSpec.ReadOnly = true;
            this.colVarAdvCSpec.Width = 89;
            // 
            // colGetAdvCSpec
            // 
            this.colGetAdvCSpec.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colGetAdvCSpec.HeaderText = "Get";
            this.colGetAdvCSpec.Name = "colGetAdvCSpec";
            this.colGetAdvCSpec.Width = 37;
            // 
            // colStatusAdvCSpec
            // 
            this.colStatusAdvCSpec.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusAdvCSpec.HeaderText = "";
            this.colStatusAdvCSpec.Name = "colStatusAdvCSpec";
            this.colStatusAdvCSpec.ReadOnly = true;
            // 
            // tabDerived
            // 
            this.tabDerived.Controls.Add(this.gridDerived);
            this.tabDerived.Location = new System.Drawing.Point(4, 25);
            this.tabDerived.Name = "tabDerived";
            this.tabDerived.Padding = new System.Windows.Forms.Padding(3);
            this.tabDerived.Size = new System.Drawing.Size(311, 322);
            this.tabDerived.TabIndex = 3;
            this.tabDerived.Text = "Derived";
            this.tabDerived.UseVisualStyleBackColor = true;
            // 
            // gridDerived
            // 
            this.gridDerived.AllowUserToAddRows = false;
            this.gridDerived.AllowUserToDeleteRows = false;
            this.gridDerived.AllowUserToResizeRows = false;
            this.gridDerived.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDerived.BackgroundColor = System.Drawing.SystemColors.Window;
            this.gridDerived.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDerived.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colVarDerived,
            this.colGetDerived,
            this.colStatusDerived});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridDerived.DefaultCellStyle = dataGridViewCellStyle4;
            this.gridDerived.Location = new System.Drawing.Point(6, 4);
            this.gridDerived.Name = "gridDerived";
            this.gridDerived.RowHeadersVisible = false;
            this.gridDerived.RowTemplate.Height = 24;
            this.gridDerived.Size = new System.Drawing.Size(296, 308);
            this.gridDerived.TabIndex = 1;
            this.gridDerived.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grid_CellClick);
            // 
            // colVarDerived
            // 
            this.colVarDerived.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVarDerived.HeaderText = "Variable";
            this.colVarDerived.Name = "colVarDerived";
            this.colVarDerived.ReadOnly = true;
            this.colVarDerived.Width = 89;
            // 
            // colGetDerived
            // 
            this.colGetDerived.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colGetDerived.HeaderText = "Get";
            this.colGetDerived.Name = "colGetDerived";
            this.colGetDerived.Width = 37;
            // 
            // colStatusDerived
            // 
            this.colStatusDerived.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colStatusDerived.HeaderText = "";
            this.colStatusDerived.Name = "colStatusDerived";
            this.colStatusDerived.ReadOnly = true;
            // 
            // btnImport
            // 
            this.btnImport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnImport.Location = new System.Drawing.Point(177, 370);
            this.btnImport.Margin = new System.Windows.Forms.Padding(4);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(73, 36);
            this.btnImport.TabIndex = 4;
            this.btnImport.Text = "Import";
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(258, 370);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(73, 36);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            // 
            // btnGetAll
            // 
            this.btnGetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetAll.Location = new System.Drawing.Point(15, 374);
            this.btnGetAll.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetAll.Name = "btnGetAll";
            this.btnGetAll.Size = new System.Drawing.Size(62, 28);
            this.btnGetAll.TabIndex = 5;
            this.btnGetAll.Text = "Get All";
            this.btnGetAll.Click += new System.EventHandler(this.btnGetAll_Click);
            // 
            // btnGetNone
            // 
            this.btnGetNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnGetNone.Location = new System.Drawing.Point(81, 374);
            this.btnGetNone.Margin = new System.Windows.Forms.Padding(4);
            this.btnGetNone.Name = "btnGetNone";
            this.btnGetNone.Size = new System.Drawing.Size(62, 28);
            this.btnGetNone.TabIndex = 6;
            this.btnGetNone.Text = "Get None";
            this.btnGetNone.Click += new System.EventHandler(this.btnGetNone_Click);
            // 
            // ImportVariablesForm
            // 
            this.AcceptButton = this.btnImport;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(343, 416);
            this.Controls.Add(this.btnGetNone);
            this.Controls.Add(this.btnGetAll);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.tabControl);
            this.Name = "ImportVariablesForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import Variables";
            this.tabControl.ResumeLayout(false);
            this.tabBasic.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridBasic)).EndInit();
            this.tabAdvanced.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridAdvanced)).EndInit();
            this.tabAdCSpec.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridAdvCSpec)).EndInit();
            this.tabDerived.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridDerived)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabBasic;
        private System.Windows.Forms.TabPage tabAdvanced;
        private DevExpress.XtraEditors.SimpleButton btnImport;
        private DevExpress.XtraEditors.SimpleButton btnCancel;
        private System.Windows.Forms.DataGridView gridBasic;
        private DevExpress.XtraEditors.SimpleButton btnGetAll;
        private DevExpress.XtraEditors.SimpleButton btnGetNone;
        private System.Windows.Forms.DataGridView gridAdvanced;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVarBasic;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetBasic;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusBasic;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVarAdvanced;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetAdvanced;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusAdvanced;
        private System.Windows.Forms.TabPage tabAdCSpec;
        private System.Windows.Forms.DataGridView gridAdvCSpec;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVarAdvCSpec;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetAdvCSpec;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusAdvCSpec;
        private System.Windows.Forms.TabPage tabDerived;
        private System.Windows.Forms.DataGridView gridDerived;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVarDerived;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetDerived;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatusDerived;
    }
}