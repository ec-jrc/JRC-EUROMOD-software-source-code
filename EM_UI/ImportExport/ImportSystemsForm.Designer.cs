namespace EM_UI.ImportExport
{
    partial class ImportSystemsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportSystemsForm));
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnSelectImportFolder = new System.Windows.Forms.Button();
            this.lstSystems = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.chkMatchByUniqueIdentifier = new System.Windows.Forms.CheckBox();
            this.btnNoSystem = new System.Windows.Forms.Button();
            this.btnAllSystems = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtImportFolder = new System.Windows.Forms.TextBox();
            this.btnCheckPath = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(230, 292);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(134, 292);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnSelectImportFolder
            // 
            this.btnSelectImportFolder.AccessibleDescription = "btnSelectImportFolder";
            this.btnSelectImportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectImportFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectImportFolder.Image")));
            this.btnSelectImportFolder.Location = new System.Drawing.Point(395, 16);
            this.btnSelectImportFolder.Name = "btnSelectImportFolder";
            this.btnSelectImportFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectImportFolder.TabIndex = 9;
            this.btnSelectImportFolder.UseVisualStyleBackColor = true;
            this.btnSelectImportFolder.Click += new System.EventHandler(this.btnSelectImportFolder_Click);
            // 
            // lstSystems
            // 
            this.lstSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSystems.CheckOnClick = true;
            this.lstSystems.FormattingEnabled = true;
            this.lstSystems.Location = new System.Drawing.Point(12, 64);
            this.lstSystems.MultiColumn = true;
            this.lstSystems.Name = "lstSystems";
            this.lstSystems.Size = new System.Drawing.Size(380, 199);
            this.lstSystems.TabIndex = 12;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 13;
            this.label2.Text = "Systems";
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // chkMatchByUniqueIdentifier
            // 
            this.chkMatchByUniqueIdentifier.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkMatchByUniqueIdentifier.AutoSize = true;
            this.chkMatchByUniqueIdentifier.Enabled = false;
            this.chkMatchByUniqueIdentifier.Location = new System.Drawing.Point(12, 265);
            this.chkMatchByUniqueIdentifier.Name = "chkMatchByUniqueIdentifier";
            this.chkMatchByUniqueIdentifier.Size = new System.Drawing.Size(150, 17);
            this.chkMatchByUniqueIdentifier.TabIndex = 14;
            this.chkMatchByUniqueIdentifier.Text = "Match by Unique Identifier";
            this.chkMatchByUniqueIdentifier.UseVisualStyleBackColor = true;
            // 
            // btnNoSystem
            // 
            this.btnNoSystem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNoSystem.Location = new System.Drawing.Point(4, 41);
            this.btnNoSystem.Margin = new System.Windows.Forms.Padding(2);
            this.btnNoSystem.Name = "btnNoSystem";
            this.btnNoSystem.Size = new System.Drawing.Size(37, 19);
            this.btnNoSystem.TabIndex = 17;
            this.btnNoSystem.Text = "No";
            this.btnNoSystem.UseVisualStyleBackColor = true;
            this.btnNoSystem.Click += new System.EventHandler(this.btnNoSystem_Click);
            // 
            // btnAllSystems
            // 
            this.btnAllSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllSystems.Location = new System.Drawing.Point(4, 18);
            this.btnAllSystems.Margin = new System.Windows.Forms.Padding(2);
            this.btnAllSystems.Name = "btnAllSystems";
            this.btnAllSystems.Size = new System.Drawing.Size(37, 19);
            this.btnAllSystems.TabIndex = 16;
            this.btnAllSystems.Text = "All";
            this.btnAllSystems.UseVisualStyleBackColor = true;
            this.btnAllSystems.Click += new System.EventHandler(this.btnAllSystems_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.btnAllSystems);
            this.groupBox1.Controls.Add(this.btnNoSystem);
            this.groupBox1.Location = new System.Drawing.Point(395, 196);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(52, 67);
            this.groupBox1.TabIndex = 18;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Select";
            // 
            // txtImportFolder
            // 
            this.txtImportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtImportFolder.Location = new System.Drawing.Point(12, 25);
            this.txtImportFolder.Name = "txtImportFolder";
            this.txtImportFolder.Size = new System.Drawing.Size(286, 20);
            this.txtImportFolder.TabIndex = 23;
            // 
            // btnCheckPath
            // 
            this.btnCheckPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckPath.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnCheckPath.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCheckPath.Location = new System.Drawing.Point(304, 18);
            this.btnCheckPath.Name = "btnCheckPath";
            this.btnCheckPath.Size = new System.Drawing.Size(86, 36);
            this.btnCheckPath.TabIndex = 24;
            this.btnCheckPath.Text = "    Validate folder";
            this.btnCheckPath.UseVisualStyleBackColor = true;
            this.btnCheckPath.Click += new System.EventHandler(this.btnCheckPath_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Import folder";
            // 
            // ImportSystemsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(450, 327);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnCheckPath);
            this.Controls.Add(this.txtImportFolder);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.chkMatchByUniqueIdentifier);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstSystems);
            this.Controls.Add(this.btnSelectImportFolder);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ImportingExportingSystems.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportSystemsForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Import System(s)";
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnSelectImportFolder;
        private System.Windows.Forms.CheckedListBox lstSystems;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.CheckBox chkMatchByUniqueIdentifier;
        private System.Windows.Forms.Button btnNoSystem;
        private System.Windows.Forms.Button btnAllSystems;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtImportFolder;
        private System.Windows.Forms.Button btnCheckPath;
        private System.Windows.Forms.Label label3;
    }
}