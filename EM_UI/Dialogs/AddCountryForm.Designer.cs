namespace EM_UI.Dialogs
{
    partial class AddCountryForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtShortName = new System.Windows.Forms.TextBox();
            this.txtLongName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFlag = new System.Windows.Forms.TextBox();
            this.labelFlag = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnFlag = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.chkAdvancedAdaptations = new System.Windows.Forms.CheckBox();
            this.btnConfigureAdvancedAdaptations = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // txtShortName
            // 
            this.txtShortName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtShortName.Location = new System.Drawing.Point(78, 12);
            this.txtShortName.Name = "txtShortName";
            this.txtShortName.Size = new System.Drawing.Size(59, 20);
            this.txtShortName.TabIndex = 0;
            // 
            // txtLongName
            // 
            this.txtLongName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLongName.Location = new System.Drawing.Point(78, 38);
            this.txtLongName.Name = "txtLongName";
            this.txtLongName.Size = new System.Drawing.Size(309, 20);
            this.txtLongName.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 13);
            this.label2.TabIndex = 36;
            this.label2.Text = "Short Name";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 41);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 35;
            this.label1.Text = "Long Name";
            // 
            // txtFlag
            // 
            this.txtFlag.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFlag.Location = new System.Drawing.Point(78, 64);
            this.txtFlag.Name = "txtFlag";
            this.txtFlag.Size = new System.Drawing.Size(309, 20);
            this.txtFlag.TabIndex = 2;
            // 
            // labelFlag
            // 
            this.labelFlag.AutoSize = true;
            this.labelFlag.Location = new System.Drawing.Point(14, 67);
            this.labelFlag.Name = "labelFlag";
            this.labelFlag.Size = new System.Drawing.Size(54, 13);
            this.labelFlag.TabIndex = 40;
            this.labelFlag.Text = "Flag (png)";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(283, 106);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(64, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(353, 106);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnFlag
            // 
            this.btnFlag.AccessibleDescription = "btnModellDataDir";
            this.btnFlag.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFlag.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnFlag.Location = new System.Drawing.Point(393, 64);
            this.btnFlag.Name = "btnFlag";
            this.btnFlag.Size = new System.Drawing.Size(24, 20);
            this.btnFlag.TabIndex = 3;
            this.btnFlag.Text = "···";
            this.btnFlag.UseVisualStyleBackColor = true;
            this.btnFlag.Click += new System.EventHandler(this.btnFlag_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // chkAdvancedAptations
            // 
            this.chkAdvancedAdaptations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkAdvancedAdaptations.AutoSize = true;
            this.helpProvider.SetHelpString(this.chkAdvancedAdaptations, "");
            this.chkAdvancedAdaptations.Location = new System.Drawing.Point(271, 14);
            this.chkAdvancedAdaptations.Name = "chkdvancedAptations";
            this.helpProvider.SetShowHelp(this.chkAdvancedAdaptations, true);
            this.chkAdvancedAdaptations.Size = new System.Drawing.Size(122, 17);
            this.chkAdvancedAdaptations.TabIndex = 4;
            this.chkAdvancedAdaptations.Text = "Advanced Adaptations";
            this.chkAdvancedAdaptations.UseVisualStyleBackColor = true;
            // 
            // btnConfigureAdvancedAdaptations
            // 
            this.btnConfigureAdvancedAdaptations.AccessibleDescription = "btnModellDataDir";
            this.btnConfigureAdvancedAdaptations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConfigureAdvancedAdaptations.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnConfigureAdvancedAdaptations.Location = new System.Drawing.Point(393, 12);
            this.btnConfigureAdvancedAdaptations.Name = "btnConfigureContentAdapt";
            this.btnConfigureAdvancedAdaptations.Size = new System.Drawing.Size(24, 20);
            this.btnConfigureAdvancedAdaptations.TabIndex = 5;
            this.btnConfigureAdvancedAdaptations.Text = "···";
            this.btnConfigureAdvancedAdaptations.UseVisualStyleBackColor = true;
            this.btnConfigureAdvancedAdaptations.Click += new System.EventHandler(this.btnConfigureAdvancedAdaptations_Click);
            // 
            // AddCountryForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(433, 149);
            this.Controls.Add(this.btnConfigureAdvancedAdaptations);
            this.Controls.Add(this.chkAdvancedAdaptations);
            this.Controls.Add(this.txtFlag);
            this.Controls.Add(this.labelFlag);
            this.Controls.Add(this.btnFlag);
            this.Controls.Add(this.txtShortName);
            this.Controls.Add(this.txtLongName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_AddingCountries.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AddCountryForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Country";
            this.Load += new System.EventHandler(this.AddCountryForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtShortName;
        private System.Windows.Forms.TextBox txtLongName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnFlag;
        private System.Windows.Forms.TextBox txtFlag;
        private System.Windows.Forms.Label labelFlag;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.CheckBox chkAdvancedAdaptations;
        private System.Windows.Forms.Button btnConfigureAdvancedAdaptations;
    }
}