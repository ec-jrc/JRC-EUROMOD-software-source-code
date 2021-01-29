namespace EM_UI.ImportExport
{
    partial class ImportAddOnForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cmbAddOns = new System.Windows.Forms.ComboBox();
            this.lstAddOnSystems = new System.Windows.Forms.CheckedListBox();
            this.lstBaseSystems = new System.Windows.Forms.CheckedListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(74, 209);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(184, 209);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Select add-on system";
            // 
            // cmbAddOns
            // 
            this.cmbAddOns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbAddOns.FormattingEnabled = true;
            this.cmbAddOns.Location = new System.Drawing.Point(12, 25);
            this.cmbAddOns.Name = "cmbAddOns";
            this.cmbAddOns.Size = new System.Drawing.Size(150, 21);
            this.cmbAddOns.TabIndex = 1;
            this.cmbAddOns.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbAddOns.SelectedIndexChanged += new System.EventHandler(this.cmbAddOns_SelectedIndexChanged);
            // 
            // lstAddOnSystems
            // 
            this.lstAddOnSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAddOnSystems.CheckOnClick = true;
            this.lstAddOnSystems.FormattingEnabled = true;
            this.lstAddOnSystems.Location = new System.Drawing.Point(12, 55);
            this.lstAddOnSystems.Name = "lstAddOnSystems";
            this.lstAddOnSystems.Size = new System.Drawing.Size(150, 139);
            this.lstAddOnSystems.TabIndex = 2;
            this.lstAddOnSystems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstAddOnSystems_ItemCheck);
            // 
            // lstBaseSystems
            // 
            this.lstBaseSystems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBaseSystems.CheckOnClick = true;
            this.lstBaseSystems.FormattingEnabled = true;
            this.lstBaseSystems.Location = new System.Drawing.Point(184, 25);
            this.lstBaseSystems.Name = "lstBaseSystems";
            this.lstBaseSystems.Size = new System.Drawing.Size(150, 169);
            this.lstBaseSystems.TabIndex = 3;
            this.lstBaseSystems.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lstBaseSystems_ItemCheck);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(181, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(98, 13);
            this.label2.TabIndex = 7;
            this.label2.Text = "Select base system";
            // 
            // ImportAddOnForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(346, 246);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lstBaseSystems);
            this.Controls.Add(this.lstAddOnSystems);
            this.Controls.Add(this.cmbAddOns);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ImportingExportingAddOns.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ImportAddOnForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Import Add-On";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbAddOns;
        private System.Windows.Forms.CheckedListBox lstAddOnSystems;
        private System.Windows.Forms.CheckedListBox lstBaseSystems;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.HelpProvider helpProvider;
    }
}