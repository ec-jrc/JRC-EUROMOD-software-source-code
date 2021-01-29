namespace EM_UI.VersionControl.Merging
{
    partial class MergeChoicesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MergeChoicesForm));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.btnSelectRemoteVersion = new System.Windows.Forms.Button();
            this.txtRemoteVersion = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.txtParentVersion = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSelectParentVersion = new System.Windows.Forms.Button();
            this.chkUseLocal = new System.Windows.Forms.CheckBox();
            this.chkUseRemote = new System.Windows.Forms.CheckBox();
            this.chkSkipCountryLabelCheck = new System.Windows.Forms.CheckBox();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.groupAdvancedOptions = new System.Windows.Forms.GroupBox();
            this.linkAdvancedOptions = new System.Windows.Forms.Label();
            this.groupAdvancedOptions.SuspendLayout();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnSelectRemoteVersion
            // 
            this.btnSelectRemoteVersion.AccessibleDescription = "";
            this.btnSelectRemoteVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectRemoteVersion.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectRemoteVersion.Image")));
            this.btnSelectRemoteVersion.Location = new System.Drawing.Point(471, 11);
            this.btnSelectRemoteVersion.Name = "btnSelectRemoteVersion";
            this.btnSelectRemoteVersion.Size = new System.Drawing.Size(40, 40);
            this.btnSelectRemoteVersion.TabIndex = 2;
            this.btnSelectRemoteVersion.UseVisualStyleBackColor = true;
            this.btnSelectRemoteVersion.Click += new System.EventHandler(this.btnSelectVersion_Click);
            // 
            // txtRemoteVersion
            // 
            this.txtRemoteVersion.AllowDrop = true;
            this.txtRemoteVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRemoteVersion.Location = new System.Drawing.Point(98, 22);
            this.txtRemoteVersion.Name = "txtRemoteVersion";
            this.txtRemoteVersion.Size = new System.Drawing.Size(369, 20);
            this.txtRemoteVersion.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(82, 13);
            this.label1.TabIndex = 47;
            this.label1.Text = "Remote Version";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(259, 76);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 8;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(178, 76);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 7;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // txtParentVersion
            // 
            this.txtParentVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParentVersion.Location = new System.Drawing.Point(91, 26);
            this.txtParentVersion.Name = "txtParentVersion";
            this.txtParentVersion.Size = new System.Drawing.Size(363, 20);
            this.txtParentVersion.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 26);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 13);
            this.label4.TabIndex = 51;
            this.label4.Text = "Parent Version";
            // 
            // btnSelectParentVersion
            // 
            this.btnSelectParentVersion.AccessibleDescription = "";
            this.btnSelectParentVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectParentVersion.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectParentVersion.Image")));
            this.btnSelectParentVersion.Location = new System.Drawing.Point(458, 12);
            this.btnSelectParentVersion.Name = "btnSelectParentVersion";
            this.btnSelectParentVersion.Size = new System.Drawing.Size(40, 40);
            this.btnSelectParentVersion.TabIndex = 4;
            this.btnSelectParentVersion.UseVisualStyleBackColor = true;
            this.btnSelectParentVersion.Click += new System.EventHandler(this.btnSelectVersion_Click);
            // 
            // chkUseLocal
            // 
            this.chkUseLocal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseLocal.AutoSize = true;
            this.chkUseLocal.Checked = true;
            this.chkUseLocal.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseLocal.Location = new System.Drawing.Point(24, 59);
            this.chkUseLocal.Margin = new System.Windows.Forms.Padding(2);
            this.chkUseLocal.Name = "chkUseLocal";
            this.chkUseLocal.Size = new System.Drawing.Size(74, 17);
            this.chkUseLocal.TabIndex = 5;
            this.chkUseLocal.Text = "Use Local";
            this.chkUseLocal.UseVisualStyleBackColor = true;
            this.chkUseLocal.CheckedChanged += new System.EventHandler(this.chkUse_CheckedChanged);
            // 
            // chkUseRemote
            // 
            this.chkUseRemote.AutoSize = true;
            this.chkUseRemote.Location = new System.Drawing.Point(102, 59);
            this.chkUseRemote.Margin = new System.Windows.Forms.Padding(2);
            this.chkUseRemote.Name = "chkUseRemote";
            this.chkUseRemote.Size = new System.Drawing.Size(85, 17);
            this.chkUseRemote.TabIndex = 6;
            this.chkUseRemote.Text = "Use Remote";
            this.chkUseRemote.UseVisualStyleBackColor = true;
            this.chkUseRemote.CheckedChanged += new System.EventHandler(this.chkUse_CheckedChanged);
            // 
            // chkSkipCountryLabelCheck
            // 
            this.chkSkipCountryLabelCheck.AutoSize = true;
            this.chkSkipCountryLabelCheck.Checked = true;
            this.chkSkipCountryLabelCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSkipCountryLabelCheck.Location = new System.Drawing.Point(349, 59);
            this.chkSkipCountryLabelCheck.Margin = new System.Windows.Forms.Padding(2);
            this.chkSkipCountryLabelCheck.Name = "chkSkipCountryLabelCheck";
            this.chkSkipCountryLabelCheck.Size = new System.Drawing.Size(149, 17);
            this.chkSkipCountryLabelCheck.TabIndex = 52;
            this.chkSkipCountryLabelCheck.Text = "Skip Country-Label Check";
            this.chkSkipCountryLabelCheck.UseVisualStyleBackColor = true;
            this.chkSkipCountryLabelCheck.Visible = false;
            // 
            // groupAdvancedOptions
            // 
            this.groupAdvancedOptions.Controls.Add(this.label4);
            this.groupAdvancedOptions.Controls.Add(this.txtParentVersion);
            this.groupAdvancedOptions.Controls.Add(this.btnSelectParentVersion);
            this.groupAdvancedOptions.Controls.Add(this.chkSkipCountryLabelCheck);
            this.groupAdvancedOptions.Controls.Add(this.chkUseLocal);
            this.groupAdvancedOptions.Controls.Add(this.chkUseRemote);
            this.groupAdvancedOptions.Location = new System.Drawing.Point(13, 82);
            this.groupAdvancedOptions.Name = "groupAdvancedOptions";
            this.groupAdvancedOptions.Size = new System.Drawing.Size(508, 100);
            this.groupAdvancedOptions.TabIndex = 54;
            this.groupAdvancedOptions.TabStop = false;
            this.groupAdvancedOptions.Text = "Advanced Options";
            this.groupAdvancedOptions.Visible = false;
            // 
            // linkAdvancedOptions
            // 
            this.linkAdvancedOptions.AutoSize = true;
            this.linkAdvancedOptions.Cursor = System.Windows.Forms.Cursors.Hand;
            this.linkAdvancedOptions.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.linkAdvancedOptions.ForeColor = System.Drawing.Color.Blue;
            this.linkAdvancedOptions.Location = new System.Drawing.Point(10, 64);
            this.linkAdvancedOptions.Name = "linkAdvancedOptions";
            this.linkAdvancedOptions.Size = new System.Drawing.Size(129, 13);
            this.linkAdvancedOptions.TabIndex = 53;
            this.linkAdvancedOptions.Text = "Display advanced options";
            this.linkAdvancedOptions.Click += new System.EventHandler(this.LinkAdvanced_Click);
            // 
            // MergeChoicesForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(523, 109);
            this.Controls.Add(this.groupAdvancedOptions);
            this.Controls.Add(this.linkAdvancedOptions);
            this.Controls.Add(this.btnSelectRemoteVersion);
            this.Controls.Add(this.txtRemoteVersion);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_MergeTool.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1700, 300);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(533, 100);
            this.Name = "MergeChoicesForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Merge Choices";
            this.Load += new System.EventHandler(this.MergeChoicesForm_Load);
            this.groupAdvancedOptions.ResumeLayout(false);
            this.groupAdvancedOptions.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnSelectRemoteVersion;
        private System.Windows.Forms.TextBox txtRemoteVersion;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.TextBox txtParentVersion;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSelectParentVersion;
        private System.Windows.Forms.CheckBox chkUseLocal;
        private System.Windows.Forms.CheckBox chkUseRemote;
        private System.Windows.Forms.CheckBox chkSkipCountryLabelCheck;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private System.Windows.Forms.GroupBox groupAdvancedOptions;
        private System.Windows.Forms.Label linkAdvancedOptions;
    }
}