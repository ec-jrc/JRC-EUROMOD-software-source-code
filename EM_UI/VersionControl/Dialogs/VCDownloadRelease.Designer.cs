namespace EM_UI.VersionControl.Dialogs
{
    partial class VCDownloadRelease
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCDownloadRelease));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.cmbReleases = new System.Windows.Forms.ComboBox();
            this.btnFullInfo = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbProjects = new System.Windows.Forms.ComboBox();
            this.btnSelectFolder = new System.Windows.Forms.Button();
            this.txtAlternativeFolder = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.dgvContent = new System.Windows.Forms.DataGridView();
            this.colUnit = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colUIReleaseCompare = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colGetReleaseVersion = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colKeepUIVersion = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colGetMergeSupport = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnMergeAll = new System.Windows.Forms.Button();
            this.btnKeepAll = new System.Windows.Forms.Button();
            this.btnGetAll = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvContent)).BeginInit();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(369, 445);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 35);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "  Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnDownload.Image = ((System.Drawing.Image)(resources.GetObject("btnDownload.Image")));
            this.btnDownload.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDownload.Location = new System.Drawing.Point(284, 445);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(80, 35);
            this.btnDownload.TabIndex = 3;
            this.btnDownload.Text = "Download";
            this.btnDownload.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 43);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(40, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Bundle";
            // 
            // cmbReleases
            // 
            this.cmbReleases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbReleases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbReleases.FormattingEnabled = true;
            this.cmbReleases.Location = new System.Drawing.Point(53, 41);
            this.cmbReleases.Margin = new System.Windows.Forms.Padding(2);
            this.cmbReleases.Name = "cmbReleases";
            this.cmbReleases.Size = new System.Drawing.Size(610, 21);
            this.cmbReleases.TabIndex = 15;
            this.cmbReleases.SelectedIndexChanged += new System.EventHandler(this.cmbReleases_SelectedIndexChanged);
            // 
            // btnFullInfo
            // 
            this.btnFullInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFullInfo.Location = new System.Drawing.Point(666, 38);
            this.btnFullInfo.Margin = new System.Windows.Forms.Padding(2);
            this.btnFullInfo.Name = "btnFullInfo";
            this.btnFullInfo.Size = new System.Drawing.Size(56, 23);
            this.btnFullInfo.TabIndex = 16;
            this.btnFullInfo.Text = "Full Info";
            this.btnFullInfo.UseVisualStyleBackColor = true;
            this.btnFullInfo.Click += new System.EventHandler(this.btnFullInfo_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Project";
            // 
            // cmbProjects
            // 
            this.cmbProjects.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbProjects.BackColor = System.Drawing.SystemColors.Window;
            this.cmbProjects.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbProjects.ForeColor = System.Drawing.SystemColors.WindowText;
            this.cmbProjects.FormattingEnabled = true;
            this.cmbProjects.Location = new System.Drawing.Point(53, 9);
            this.cmbProjects.Margin = new System.Windows.Forms.Padding(2);
            this.cmbProjects.Name = "cmbProjects";
            this.cmbProjects.Size = new System.Drawing.Size(610, 21);
            this.cmbProjects.TabIndex = 19;
            this.cmbProjects.SelectedIndexChanged += new System.EventHandler(this.cmbProjects_SelectedIndexChanged);
            // 
            // btnSelectFolder
            // 
            this.btnSelectFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFolder.Image = global::EM_UI.Properties.Resources.Folder;
            this.btnSelectFolder.Location = new System.Drawing.Point(681, 404);
            this.btnSelectFolder.Name = "btnSelectFolder";
            this.btnSelectFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectFolder.TabIndex = 13;
            this.btnSelectFolder.UseVisualStyleBackColor = true;
            this.btnSelectFolder.Visible = false;
            this.btnSelectFolder.Click += new System.EventHandler(this.btnSelectFolder_Click);
            // 
            // txtAlternativeFolder
            // 
            this.txtAlternativeFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtAlternativeFolder.Location = new System.Drawing.Point(8, 415);
            this.txtAlternativeFolder.Margin = new System.Windows.Forms.Padding(2);
            this.txtAlternativeFolder.Name = "txtAlternativeFolder";
            this.txtAlternativeFolder.Size = new System.Drawing.Size(668, 20);
            this.txtAlternativeFolder.TabIndex = 1;
            this.txtAlternativeFolder.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 399);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(113, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Alternative Destination";
            this.label2.Visible = false;
            // 
            // dgvContent
            // 
            this.dgvContent.AllowUserToAddRows = false;
            this.dgvContent.AllowUserToDeleteRows = false;
            this.dgvContent.AllowUserToResizeRows = false;
            this.dgvContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvContent.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvContent.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvContent.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colUnit,
            this.colType,
            this.colVersion,
            this.colUIReleaseCompare,
            this.colGetReleaseVersion,
            this.colKeepUIVersion,
            this.colGetMergeSupport});
            this.dgvContent.Location = new System.Drawing.Point(8, 75);
            this.dgvContent.Margin = new System.Windows.Forms.Padding(2);
            this.dgvContent.Name = "dgvContent";
            this.dgvContent.RowHeadersVisible = false;
            this.dgvContent.RowTemplate.Height = 24;
            this.dgvContent.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvContent.ShowCellErrors = false;
            this.dgvContent.ShowEditingIcon = false;
            this.dgvContent.ShowRowErrors = false;
            this.dgvContent.Size = new System.Drawing.Size(712, 323);
            this.dgvContent.TabIndex = 21;
            this.dgvContent.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvContent_CellContentClick);
            // 
            // colUnit
            // 
            this.colUnit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colUnit.Frozen = true;
            this.colUnit.HeaderText = "Unit";
            this.colUnit.Name = "colUnit";
            this.colUnit.ReadOnly = true;
            this.colUnit.Width = 51;
            // 
            // colType
            // 
            this.colType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colType.Frozen = true;
            this.colType.HeaderText = "Type";
            this.colType.Name = "colType";
            this.colType.ReadOnly = true;
            this.colType.Width = 56;
            // 
            // colVersion
            // 
            this.colVersion.HeaderText = "Version";
            this.colVersion.Name = "colVersion";
            this.colVersion.ReadOnly = true;
            this.colVersion.Width = 250;
            // 
            // colUIReleaseCompare
            // 
            this.colUIReleaseCompare.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colUIReleaseCompare.HeaderText = "Local Status";
            this.colUIReleaseCompare.Name = "colUIReleaseCompare";
            this.colUIReleaseCompare.ReadOnly = true;
            this.colUIReleaseCompare.Width = 84;
            // 
            // colGetReleaseVersion
            // 
            this.colGetReleaseVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colGetReleaseVersion.HeaderText = "Get Online Version";
            this.colGetReleaseVersion.Name = "colGetReleaseVersion";
            this.colGetReleaseVersion.Width = 70;
            // 
            // colKeepUIVersion
            // 
            this.colKeepUIVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colKeepUIVersion.HeaderText = "Keep Local Version";
            this.colKeepUIVersion.Name = "colKeepUIVersion";
            this.colKeepUIVersion.Width = 70;
            // 
            // colGetMergeSupport
            // 
            this.colGetMergeSupport.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.colGetMergeSupport.HeaderText = "Get Merge Support";
            this.colGetMergeSupport.Name = "colGetMergeSupport";
            this.colGetMergeSupport.Width = 70;
            // 
            // btnMergeAll
            // 
            this.btnMergeAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnMergeAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMergeAll.Location = new System.Drawing.Point(664, 403);
            this.btnMergeAll.Name = "btnMergeAll";
            this.btnMergeAll.Size = new System.Drawing.Size(57, 23);
            this.btnMergeAll.TabIndex = 24;
            this.btnMergeAll.Text = "Merge All";
            this.btnMergeAll.UseVisualStyleBackColor = true;
            this.btnMergeAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnKeepAll
            // 
            this.btnKeepAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnKeepAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnKeepAll.Location = new System.Drawing.Point(601, 403);
            this.btnKeepAll.Name = "btnKeepAll";
            this.btnKeepAll.Size = new System.Drawing.Size(57, 23);
            this.btnKeepAll.TabIndex = 23;
            this.btnKeepAll.Text = "Keep All";
            this.btnKeepAll.UseVisualStyleBackColor = true;
            this.btnKeepAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // btnGetAll
            // 
            this.btnGetAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGetAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnGetAll.Location = new System.Drawing.Point(538, 403);
            this.btnGetAll.Name = "btnGetAll";
            this.btnGetAll.Size = new System.Drawing.Size(57, 23);
            this.btnGetAll.TabIndex = 22;
            this.btnGetAll.Text = "Get All";
            this.btnGetAll.UseVisualStyleBackColor = true;
            this.btnGetAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // VCDownloadRelease
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 492);
            this.Controls.Add(this.btnMergeAll);
            this.Controls.Add(this.btnKeepAll);
            this.Controls.Add(this.btnGetAll);
            this.Controls.Add(this.dgvContent);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnSelectFolder);
            this.Controls.Add(this.txtAlternativeFolder);
            this.Controls.Add(this.cmbProjects);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnFullInfo);
            this.Controls.Add(this.cmbReleases);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDownload);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_DownloadRelease.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(396, 382);
            this.Name = "VCDownloadRelease";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Version Control - Download Bundle";
            this.Load += new System.EventHandler(this.VCDownloadRelease_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvContent)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox cmbReleases;
        private System.Windows.Forms.Button btnFullInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbProjects;
        private System.Windows.Forms.Button btnSelectFolder;
        private System.Windows.Forms.TextBox txtAlternativeFolder;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView dgvContent;
        private System.Windows.Forms.Button btnMergeAll;
        private System.Windows.Forms.Button btnKeepAll;
        private System.Windows.Forms.Button btnGetAll;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUnit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colType;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn colUIReleaseCompare;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetReleaseVersion;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colKeepUIVersion;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colGetMergeSupport;
    }
}