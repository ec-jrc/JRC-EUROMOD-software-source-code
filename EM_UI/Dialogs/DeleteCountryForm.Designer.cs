namespace EM_UI.Dialogs
{
    partial class DeleteCountryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DeleteCountryForm));
            this.lvCountries = new System.Windows.Forms.ListView();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblWarning = new System.Windows.Forms.TextBox();
            this.picWarning = new System.Windows.Forms.PictureBox();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.chkAdaptGlobal = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.picWarning)).BeginInit();
            this.SuspendLayout();
            // 
            // lvCountries
            // 
            this.lvCountries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvCountries.CheckBoxes = true;
            this.lvCountries.HideSelection = false;
            this.lvCountries.Location = new System.Drawing.Point(12, 12);
            this.lvCountries.Name = "lvCountries";
            this.lvCountries.Size = new System.Drawing.Size(230, 292);
            this.lvCountries.TabIndex = 0;
            this.lvCountries.UseCompatibleStateImageBehavior = false;
            this.lvCountries.View = System.Windows.Forms.View.List;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(132, 376);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDelete.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnDelete.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDelete.Location = new System.Drawing.Point(36, 376);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(90, 23);
            this.btnDelete.TabIndex = 1;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblWarning
            // 
            this.lblWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblWarning.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblWarning.Location = new System.Drawing.Point(50, 334);
            this.lblWarning.Multiline = true;
            this.lblWarning.Name = "lblWarning";
            this.lblWarning.ReadOnly = true;
            this.lblWarning.Size = new System.Drawing.Size(201, 33);
            this.lblWarning.TabIndex = 9;
            this.lblWarning.TabStop = false;
            this.lblWarning.Text = "Please note that deleting is final. No undo is possible!";
            // 
            // picWarning
            // 
            this.picWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.picWarning.Enabled = false;
            this.picWarning.Image = ((System.Drawing.Image)(resources.GetObject("picWarning.Image")));
            this.picWarning.ImageLocation = "";
            this.picWarning.Location = new System.Drawing.Point(12, 334);
            this.picWarning.Name = "picWarning";
            this.picWarning.Size = new System.Drawing.Size(32, 33);
            this.picWarning.TabIndex = 10;
            this.picWarning.TabStop = false;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // chkAdaptGlobal
            // 
            this.chkAdaptGlobal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkAdaptGlobal.AutoSize = true;
            this.chkAdaptGlobal.Location = new System.Drawing.Point(12, 310);
            this.chkAdaptGlobal.Name = "chkAdaptGlobal";
            this.chkAdaptGlobal.Size = new System.Drawing.Size(230, 17);
            this.chkAdaptGlobal.TabIndex = 53;
            this.chkAdaptGlobal.Text = "Adapt Global Files (HICP, Exchange Rates)";
            this.chkAdaptGlobal.UseVisualStyleBackColor = true;
            // 
            // DeleteCountryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(257, 411);
            this.Controls.Add(this.chkAdaptGlobal);
            this.Controls.Add(this.picWarning);
            this.Controls.Add(this.lblWarning);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.lvCountries);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_DeletingCountries.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeleteCountryForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Delete Countries";
            this.Load += new System.EventHandler(this.DeleteCountryForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picWarning)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lvCountries;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.TextBox lblWarning;
        private System.Windows.Forms.PictureBox picWarning;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.CheckBox chkAdaptGlobal;
    }
}