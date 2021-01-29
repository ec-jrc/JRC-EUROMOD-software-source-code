namespace EM_UI.ImportExport
{
    partial class ExportSystemsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportSystemsForm));
            this.label2 = new System.Windows.Forms.Label();
            this.lstSystems = new System.Windows.Forms.CheckedListBox();
            this.btnSelectExportFolder = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.txtExportFolder = new System.Windows.Forms.TextBox();
            this.radExportAndDelete = new System.Windows.Forms.RadioButton();
            this.radExportOnly = new System.Windows.Forms.RadioButton();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 20;
            this.label2.Text = "Systems";
            // 
            // lstSystems
            // 
            this.lstSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSystems.CheckOnClick = true;
            this.lstSystems.FormattingEnabled = true;
            this.lstSystems.Location = new System.Drawing.Point(12, 69);
            this.lstSystems.Name = "lstSystems";
            this.lstSystems.Size = new System.Drawing.Size(261, 199);
            this.lstSystems.TabIndex = 19;
            // 
            // btnSelectExportFolder
            // 
            this.btnSelectExportFolder.AccessibleDescription = "btnSelectExportFolder";
            this.btnSelectExportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectExportFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectExportFolder.Image")));
            this.btnSelectExportFolder.Location = new System.Drawing.Point(346, 7);
            this.btnSelectExportFolder.Name = "btnSelectExportFolder";
            this.btnSelectExportFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectExportFolder.TabIndex = 16;
            this.btnSelectExportFolder.UseVisualStyleBackColor = true;
            this.btnSelectExportFolder.Click += new System.EventHandler(this.btnSelectExportFolder_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(297, 241);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 15;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(297, 212);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 14;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 7);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(66, 13);
            this.label3.TabIndex = 21;
            this.label3.Text = "Export folder";
            // 
            // txtExportFolder
            // 
            this.txtExportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExportFolder.Location = new System.Drawing.Point(12, 23);
            this.txtExportFolder.Name = "txtExportFolder";
            this.txtExportFolder.Size = new System.Drawing.Size(329, 20);
            this.txtExportFolder.TabIndex = 22;
            // 
            // radExportAndDelete
            // 
            this.radExportAndDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radExportAndDelete.AutoSize = true;
            this.radExportAndDelete.Checked = true;
            this.radExportAndDelete.Location = new System.Drawing.Point(279, 70);
            this.radExportAndDelete.Name = "radExportAndDelete";
            this.radExportAndDelete.Size = new System.Drawing.Size(108, 17);
            this.radExportAndDelete.TabIndex = 23;
            this.radExportAndDelete.TabStop = true;
            this.radExportAndDelete.Text = "Export and delete";
            this.radExportAndDelete.UseVisualStyleBackColor = true;
            // 
            // radExportOnly
            // 
            this.radExportOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.radExportOnly.AutoSize = true;
            this.radExportOnly.Location = new System.Drawing.Point(279, 93);
            this.radExportOnly.Name = "radExportOnly";
            this.radExportOnly.Size = new System.Drawing.Size(77, 17);
            this.radExportOnly.TabIndex = 24;
            this.radExportOnly.TabStop = true;
            this.radExportOnly.Text = "Export only";
            this.radExportOnly.UseVisualStyleBackColor = true;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // ExportSystemsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(399, 276);
            this.Controls.Add(this.radExportOnly);
            this.Controls.Add(this.radExportAndDelete);
            this.Controls.Add(this.txtExportFolder);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstSystems);
            this.Controls.Add(this.btnSelectExportFolder);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ImportingExportingSystems.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportSystemsForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Export System(s)";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckedListBox lstSystems;
        private System.Windows.Forms.Button btnSelectExportFolder;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtExportFolder;
        private System.Windows.Forms.RadioButton radExportAndDelete;
        private System.Windows.Forms.RadioButton radExportOnly;
        private System.Windows.Forms.HelpProvider helpProvider;
    }
}