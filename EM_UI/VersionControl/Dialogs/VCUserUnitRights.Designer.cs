namespace EM_UI.VersionControl.Dialogs
{
    partial class VCUserUnitRights
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
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.dgvUnits = new System.Windows.Forms.DataGridView();
            this.colUnitName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUnitType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colWrite = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colRead = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colNone = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnAllWrite = new System.Windows.Forms.Button();
            this.btnAllRead = new System.Windows.Forms.Button();
            this.btnAllNone = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvUnits)).BeginInit();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(338, 449);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(62, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "   Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(338, 420);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(62, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // dgvUnits
            // 
            this.dgvUnits.AllowUserToAddRows = false;
            this.dgvUnits.AllowUserToDeleteRows = false;
            this.dgvUnits.AllowUserToResizeRows = false;
            this.dgvUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvUnits.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvUnits.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvUnits.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUnitName,
            this.colUnitType,
            this.colWrite,
            this.colRead,
            this.colNone});
            this.dgvUnits.Location = new System.Drawing.Point(9, 10);
            this.dgvUnits.Margin = new System.Windows.Forms.Padding(2);
            this.dgvUnits.Name = "dgvUnits";
            this.dgvUnits.RowHeadersVisible = false;
            this.dgvUnits.RowTemplate.Height = 24;
            this.dgvUnits.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvUnits.ShowCellErrors = false;
            this.dgvUnits.ShowEditingIcon = false;
            this.dgvUnits.ShowRowErrors = false;
            this.dgvUnits.Size = new System.Drawing.Size(324, 463);
            this.dgvUnits.TabIndex = 22;
            this.dgvUnits.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvUnits_CellContentClick);
            // 
            // colUnitName
            // 
            this.colUnitName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colUnitName.Frozen = true;
            this.colUnitName.HeaderText = "Unit";
            this.colUnitName.Name = "colUnitName";
            this.colUnitName.ReadOnly = true;
            this.colUnitName.Width = 51;
            // 
            // colUnitType
            // 
            this.colUnitType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colUnitType.Frozen = true;
            this.colUnitType.HeaderText = "Type";
            this.colUnitType.Name = "colUnitType";
            this.colUnitType.ReadOnly = true;
            this.colUnitType.Width = 56;
            // 
            // colWrite
            // 
            this.colWrite.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colWrite.HeaderText = "Upload";
            this.colWrite.Name = "colWrite";
            this.colWrite.Width = 70;
            // 
            // colRead
            // 
            this.colRead.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colRead.HeaderText = "Download";
            this.colRead.Name = "colRead";
            this.colRead.Width = 70;
            // 
            // colNone
            // 
            this.colNone.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colNone.HeaderText = "None";
            this.colNone.Name = "colNone";
            this.colNone.Width = 70;
            // 
            // btnAllWrite
            // 
            this.btnAllWrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllWrite.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAllWrite.Location = new System.Drawing.Point(338, 10);
            this.btnAllWrite.Name = "btnAllWrite";
            this.btnAllWrite.Size = new System.Drawing.Size(62, 23);
            this.btnAllWrite.TabIndex = 24;
            this.btnAllWrite.Text = "All Write";
            this.btnAllWrite.UseVisualStyleBackColor = true;
            this.btnAllWrite.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnAllRead
            // 
            this.btnAllRead.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllRead.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAllRead.Location = new System.Drawing.Point(338, 39);
            this.btnAllRead.Name = "btnAllRead";
            this.btnAllRead.Size = new System.Drawing.Size(62, 23);
            this.btnAllRead.TabIndex = 25;
            this.btnAllRead.Text = "All Read";
            this.btnAllRead.UseVisualStyleBackColor = true;
            this.btnAllRead.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnAllNone
            // 
            this.btnAllNone.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllNone.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAllNone.Location = new System.Drawing.Point(338, 68);
            this.btnAllNone.Name = "btnAllNone";
            this.btnAllNone.Size = new System.Drawing.Size(62, 23);
            this.btnAllNone.TabIndex = 26;
            this.btnAllNone.Text = "All None";
            this.btnAllNone.UseVisualStyleBackColor = true;
            this.btnAllNone.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // VCUserUnitRights
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(410, 483);
            this.Controls.Add(this.btnAllNone);
            this.Controls.Add(this.btnAllRead);
            this.Controls.Add(this.btnAllWrite);
            this.Controls.Add(this.dgvUnits);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_AdminUsers.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(334, 197);
            this.Name = "VCUserUnitRights";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Version Control - User Rights for ";
            this.Load += new System.EventHandler(this.VCUserUnitRights_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvUnits)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.DataGridView dgvUnits;
        private System.Windows.Forms.Button btnAllWrite;
        private System.Windows.Forms.Button btnAllRead;
        private System.Windows.Forms.Button btnAllNone;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnitType;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colWrite;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colRead;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colNone;
     }
}