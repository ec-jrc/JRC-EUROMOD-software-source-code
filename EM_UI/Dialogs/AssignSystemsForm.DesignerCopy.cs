namespace EM_UI.Dialogs
{
    partial class AssignSystemsForm
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
            this.components = new System.ComponentModel.Container();
            this.txPasteCountry = new System.Windows.Forms.TextBox();
            this.txCopyCountry = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnAssign = new System.Windows.Forms.Button();
            this.lvSystemsPasteCountry = new System.Windows.Forms.ListView();
            this.colHDest = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colHOrig = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.lvSystemsCopyCountry = new System.Windows.Forms.ListView();
            this.colHSys = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tipAssignSystems = new System.Windows.Forms.ToolTip(this.components);
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.SuspendLayout();
            // 
            // txPasteCountry
            // 
            this.txPasteCountry.BackColor = System.Drawing.SystemColors.Control;
            this.txPasteCountry.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txPasteCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txPasteCountry.Location = new System.Drawing.Point(12, 12);
            this.txPasteCountry.Name = "txPasteCountry";
            this.txPasteCountry.ReadOnly = true;
            this.txPasteCountry.Size = new System.Drawing.Size(260, 13);
            this.txPasteCountry.TabIndex = 1;
            this.txPasteCountry.TabStop = false;
            // 
            // txCopyCountry
            // 
            this.txCopyCountry.BackColor = System.Drawing.SystemColors.Control;
            this.txCopyCountry.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txCopyCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txCopyCountry.Location = new System.Drawing.Point(334, 12);
            this.txCopyCountry.Name = "txCopyCountry";
            this.txCopyCountry.ReadOnly = true;
            this.txCopyCountry.Size = new System.Drawing.Size(130, 13);
            this.txCopyCountry.TabIndex = 3;
            this.txCopyCountry.TabStop = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(375, 215);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(279, 215);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnAssign
            // 
            this.btnAssign.Location = new System.Drawing.Point(278, 100);
            this.btnAssign.Name = "btnAssign";
            this.btnAssign.Size = new System.Drawing.Size(50, 23);
            this.btnAssign.TabIndex = 6;
            this.btnAssign.Text = "Assign";
            this.tipAssignSystems.SetToolTip(this.btnAssign, "Select system in the right box to assign it to selected system in the left box");
            this.btnAssign.UseVisualStyleBackColor = true;
            this.btnAssign.Click += new System.EventHandler(this.btnAssign_Click);
            // 
            // lvSystemsPasteCountry
            // 
            this.lvSystemsPasteCountry.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lvSystemsPasteCountry.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHDest,
            this.colHOrig});
            this.lvSystemsPasteCountry.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSystemsPasteCountry.HideSelection = false;
            this.lvSystemsPasteCountry.Location = new System.Drawing.Point(12, 31);
            this.lvSystemsPasteCountry.MultiSelect = false;
            this.lvSystemsPasteCountry.Name = "lvSystemsPasteCountry";
            this.lvSystemsPasteCountry.Size = new System.Drawing.Size(260, 167);
            this.lvSystemsPasteCountry.TabIndex = 7;
            this.lvSystemsPasteCountry.UseCompatibleStateImageBehavior = false;
            this.lvSystemsPasteCountry.View = System.Windows.Forms.View.Details;
            // 
            // colHDest
            // 
            this.colHDest.Text = "System ...";
            this.colHDest.Width = 127;
            // 
            // colHOrig
            // 
            this.colHOrig.Text = "... Assigned to";
            this.colHOrig.Width = 127;
            // 
            // lvSystemsCopyCountry
            // 
            this.lvSystemsCopyCountry.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colHSys});
            this.lvSystemsCopyCountry.FullRowSelect = true;
            this.lvSystemsCopyCountry.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvSystemsCopyCountry.HideSelection = false;
            this.lvSystemsCopyCountry.Location = new System.Drawing.Point(334, 31);
            this.lvSystemsCopyCountry.MultiSelect = false;
            this.lvSystemsCopyCountry.Name = "lvSystemsCopyCountry";
            this.lvSystemsCopyCountry.Size = new System.Drawing.Size(130, 167);
            this.lvSystemsCopyCountry.TabIndex = 8;
            this.lvSystemsCopyCountry.UseCompatibleStateImageBehavior = false;
            this.lvSystemsCopyCountry.View = System.Windows.Forms.View.Details;
            // 
            // colHSys
            // 
            this.colHSys.Text = "Systems";
            this.colHSys.Width = 125;
            // 
            // tipAssignSystems
            // 
            this.tipAssignSystems.AutomaticDelay = 0;
            this.tipAssignSystems.AutoPopDelay = 500000;
            this.tipAssignSystems.InitialDelay = 0;
            this.tipAssignSystems.ReshowDelay = 0;
            this.tipAssignSystems.ShowAlways = true;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // AssignSystemsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(477, 250);
            this.Controls.Add(this.lvSystemsCopyCountry);
            this.Controls.Add(this.lvSystemsPasteCountry);
            this.Controls.Add(this.btnAssign);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txCopyCountry);
            this.Controls.Add(this.txPasteCountry);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.TableOfContents);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssignSystemsForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Assign Systems";
            this.Load += new System.EventHandler(this.AssignSystemsForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txPasteCountry;
        private System.Windows.Forms.TextBox txCopyCountry;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnAssign;
        private System.Windows.Forms.ListView lvSystemsPasteCountry;
        private System.Windows.Forms.ColumnHeader colHDest;
        private System.Windows.Forms.ColumnHeader colHOrig;
        private System.Windows.Forms.ListView lvSystemsCopyCountry;
        private System.Windows.Forms.ColumnHeader colHSys;
        private System.Windows.Forms.ToolTip tipAssignSystems;
        private System.Windows.Forms.HelpProvider helpProvider;
    }
}