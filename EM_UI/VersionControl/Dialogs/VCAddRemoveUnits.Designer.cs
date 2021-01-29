namespace EM_UI.VersionControl.Dialogs
{
    partial class VCAddRemoveUnits
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
            this.listUnits = new System.Windows.Forms.ListView();
            this.colUnit = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.btnSel = new System.Windows.Forms.Button();
            this.btnUnsel = new System.Windows.Forms.Button();
            this.chkVersion = new System.Windows.Forms.CheckBox();
            this.textVersion = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
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
            this.btnCancel.Location = new System.Drawing.Point(279, 239);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(76, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(200, 239);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(76, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // listUnits
            // 
            this.listUnits.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listUnits.CheckBoxes = true;
            this.listUnits.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colUnit,
            this.colType});
            this.listUnits.HideSelection = false;
            this.listUnits.Location = new System.Drawing.Point(9, 10);
            this.listUnits.Margin = new System.Windows.Forms.Padding(2);
            this.listUnits.Name = "listUnits";
            this.listUnits.Size = new System.Drawing.Size(345, 198);
            this.listUnits.TabIndex = 5;
            this.listUnits.UseCompatibleStateImageBehavior = false;
            this.listUnits.View = System.Windows.Forms.View.Details;
            // 
            // colUnit
            // 
            this.colUnit.Text = "Unit";
            this.colUnit.Width = 100;
            // 
            // colType
            // 
            this.colType.Text = "Type";
            this.colType.Width = 100;
            // 
            // btnSel
            // 
            this.btnSel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSel.Location = new System.Drawing.Point(9, 239);
            this.btnSel.Name = "btnSel";
            this.btnSel.Size = new System.Drawing.Size(67, 23);
            this.btnSel.TabIndex = 8;
            this.btnSel.Text = "Select All";
            this.btnSel.UseVisualStyleBackColor = true;
            this.btnSel.Click += new System.EventHandler(this.btnSel_Click);
            // 
            // btnUnsel
            // 
            this.btnUnsel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUnsel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUnsel.Location = new System.Drawing.Point(82, 239);
            this.btnUnsel.Name = "btnUnsel";
            this.btnUnsel.Size = new System.Drawing.Size(72, 23);
            this.btnUnsel.TabIndex = 9;
            this.btnUnsel.Text = "Unselect All";
            this.btnUnsel.UseVisualStyleBackColor = true;
            this.btnUnsel.Click += new System.EventHandler(this.btnUnsel_Click);
            // 
            // chkVersion
            // 
            this.chkVersion.AutoSize = true;
            this.chkVersion.Location = new System.Drawing.Point(9, 214);
            this.chkVersion.Name = "chkVersion";
            this.chkVersion.Size = new System.Drawing.Size(99, 17);
            this.chkVersion.TabIndex = 10;
            this.chkVersion.Text = "Manual Version";
            this.chkVersion.UseVisualStyleBackColor = true;
            this.chkVersion.CheckedChanged += new System.EventHandler(this.chkVersion_CheckedChanged);
            // 
            // textVersion
            // 
            this.textVersion.Enabled = false;
            this.textVersion.Location = new System.Drawing.Point(105, 214);
            this.textVersion.Name = "textVersion";
            this.textVersion.Size = new System.Drawing.Size(52, 20);
            this.textVersion.TabIndex = 11;
            // 
            // VCAddRemoveUnits
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(361, 272);
            this.Controls.Add(this.textVersion);
            this.Controls.Add(this.chkVersion);
            this.Controls.Add(this.btnUnsel);
            this.Controls.Add(this.btnSel);
            this.Controls.Add(this.listUnits);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_VC_AdminContent.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(278, 290);
            this.Name = "VCAddRemoveUnits";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Version Control - Select Units";
            this.Load += new System.EventHandler(this.VCAddRemoveUnits_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListView listUnits;
        private System.Windows.Forms.ColumnHeader colUnit;
        private System.Windows.Forms.ColumnHeader colType;
        private System.Windows.Forms.Button btnSel;
        private System.Windows.Forms.Button btnUnsel;
        private System.Windows.Forms.CheckBox chkVersion;
        private System.Windows.Forms.TextBox textVersion;
    }
}