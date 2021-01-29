namespace EM_UI.ImportExport
{
    partial class ExportAddOnForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportAddOnForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lstAddOnSystems = new System.Windows.Forms.CheckedListBox();
            this.lstBaseSystems = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.chkUseSymbolicIDs = new System.Windows.Forms.CheckBox();
            this.chkUseCC = new System.Windows.Forms.CheckBox();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.radExportAndDelete = new System.Windows.Forms.RadioButton();
            this.radExportOnly = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtShortName = new System.Windows.Forms.TextBox();
            this.txtLongName = new System.Windows.Forms.TextBox();
            this.txtSymbol = new System.Windows.Forms.TextBox();
            this.btnSymbol = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Add-On System";
            // 
            // lstAddOnSystems
            // 
            this.lstAddOnSystems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstAddOnSystems.CheckOnClick = true;
            this.lstAddOnSystems.FormattingEnabled = true;
            this.lstAddOnSystems.Location = new System.Drawing.Point(15, 95);
            this.lstAddOnSystems.Name = "lstAddOnSystems";
            this.lstAddOnSystems.Size = new System.Drawing.Size(150, 199);
            this.lstAddOnSystems.TabIndex = 5;
            this.lstAddOnSystems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstAddOnSystems_ItemCheck);
            // 
            // lstBaseSystems
            // 
            this.lstBaseSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBaseSystems.CheckOnClick = true;
            this.lstBaseSystems.FormattingEnabled = true;
            this.lstBaseSystems.Location = new System.Drawing.Point(187, 95);
            this.lstBaseSystems.Name = "lstBaseSystems";
            this.lstBaseSystems.Size = new System.Drawing.Size(150, 199);
            this.lstBaseSystems.TabIndex = 6;
            this.lstBaseSystems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstBaseSystems_ItemCheck);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(184, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Select Base System";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(75, 380);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(187, 380);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // chkUseSymbolicIDs
            // 
            this.chkUseSymbolicIDs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkUseSymbolicIDs.AutoSize = true;
            this.chkUseSymbolicIDs.Location = new System.Drawing.Point(15, 314);
            this.chkUseSymbolicIDs.Name = "chkUseSymbolicIDs";
            this.chkUseSymbolicIDs.Size = new System.Drawing.Size(135, 17);
            this.chkUseSymbolicIDs.TabIndex = 7;
            this.chkUseSymbolicIDs.Text = "Use symbolic identifiers";
            this.chkUseSymbolicIDs.UseVisualStyleBackColor = true;
            // 
            // chkUseCC
            // 
            this.chkUseCC.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkUseCC.AutoSize = true;
            this.chkUseCC.Location = new System.Drawing.Point(15, 338);
            this.chkUseCC.Name = "chkUseCC";
            this.chkUseCC.Size = new System.Drawing.Size(141, 17);
            this.chkUseCC.TabIndex = 8;
            this.chkUseCC.Text = "Use country placeholder";
            this.chkUseCC.UseVisualStyleBackColor = true;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // radExportAndDelete
            // 
            this.radExportAndDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radExportAndDelete.AutoSize = true;
            this.radExportAndDelete.Checked = true;
            this.radExportAndDelete.Location = new System.Drawing.Point(189, 314);
            this.radExportAndDelete.Name = "radExportAndDelete";
            this.radExportAndDelete.Size = new System.Drawing.Size(108, 17);
            this.radExportAndDelete.TabIndex = 23;
            this.radExportAndDelete.TabStop = true;
            this.radExportAndDelete.Text = "Export and delete";
            this.radExportAndDelete.UseVisualStyleBackColor = true;
            // 
            // radExportOnly
            // 
            this.radExportOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.radExportOnly.AutoSize = true;
            this.radExportOnly.Location = new System.Drawing.Point(189, 338);
            this.radExportOnly.Name = "radExportOnly";
            this.radExportOnly.Size = new System.Drawing.Size(77, 17);
            this.radExportOnly.TabIndex = 24;
            this.radExportOnly.TabStop = true;
            this.radExportOnly.Text = "Export only";
            this.radExportOnly.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(62, 13);
            this.label3.TabIndex = 35;
            this.label3.Text = "Long Name";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(215, 17);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 36;
            this.label4.Text = "Short Name";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(41, 13);
            this.label5.TabIndex = 40;
            this.label5.Text = "Symbol";
            // 
            // txtShortName
            // 
            this.txtShortName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtShortName.Location = new System.Drawing.Point(278, 14);
            this.txtShortName.Name = "txtShortName";
            this.txtShortName.Size = new System.Drawing.Size(59, 20);
            this.txtShortName.TabIndex = 2;
            // 
            // txtLongName
            // 
            this.txtLongName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLongName.Location = new System.Drawing.Point(74, 14);
            this.txtLongName.Name = "txtLongName";
            this.txtLongName.Size = new System.Drawing.Size(138, 20);
            this.txtLongName.TabIndex = 1;
            // 
            // txtSymbol
            // 
            this.txtSymbol.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSymbol.Location = new System.Drawing.Point(74, 40);
            this.txtSymbol.Name = "txtSymbol";
            this.txtSymbol.Size = new System.Drawing.Size(221, 20);
            this.txtSymbol.TabIndex = 3;
            // 
            // btnSymbol
            // 
            this.btnSymbol.AccessibleDescription = "btnModellDataDir";
            this.btnSymbol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSymbol.Image = ((System.Drawing.Image)(resources.GetObject("btnSymbol.Image")));
            this.btnSymbol.Location = new System.Drawing.Point(297, 35);
            this.btnSymbol.Name = "btnSymbol";
            this.btnSymbol.Size = new System.Drawing.Size(40, 40);
            this.btnSymbol.TabIndex = 4;
            this.btnSymbol.UseVisualStyleBackColor = true;
            this.btnSymbol.Click += new System.EventHandler(this.btnSymbol_Click);
            // 
            // ExportAddOnForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(357, 420);
            this.Controls.Add(this.chkUseCC);
            this.Controls.Add(this.chkUseSymbolicIDs);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lstBaseSystems);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstAddOnSystems);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radExportOnly);
            this.Controls.Add(this.radExportAndDelete);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtSymbol);
            this.Controls.Add(this.btnSymbol);
            this.Controls.Add(this.txtShortName);
            this.Controls.Add(this.txtLongName);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ImportingExportingAddOns.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportAddOnForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Export Add-On";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckedListBox lstAddOnSystems;
        private System.Windows.Forms.CheckedListBox lstBaseSystems;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        internal System.Windows.Forms.CheckBox chkUseSymbolicIDs;
        internal System.Windows.Forms.CheckBox chkUseCC;
        private System.Windows.Forms.HelpProvider helpProvider;
        internal System.Windows.Forms.RadioButton radExportAndDelete;
        internal System.Windows.Forms.RadioButton radExportOnly;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        internal System.Windows.Forms.TextBox txtShortName;
        private System.Windows.Forms.Button btnSymbol;
        internal System.Windows.Forms.TextBox txtLongName;
        internal System.Windows.Forms.TextBox txtSymbol;
    }
}