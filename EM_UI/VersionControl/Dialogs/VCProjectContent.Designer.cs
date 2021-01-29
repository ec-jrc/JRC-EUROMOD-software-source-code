namespace EM_UI.VersionControl.Dialogs
{
    partial class VCProjectContent
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
            this.btnSel = new System.Windows.Forms.Button();
            this.btnUnsel = new System.Windows.Forms.Button();
            this.groupYears = new System.Windows.Forms.GroupBox();
            this.listYears = new System.Windows.Forms.ListBox();
            this.numYear = new System.Windows.Forms.NumericUpDown();
            this.btnAddYear = new System.Windows.Forms.Button();
            this.checkAllYears = new System.Windows.Forms.CheckBox();
            this.groupYears.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numYear)).BeginInit();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(332, 464);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "    Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(268, 464);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(58, 23);
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
            this.listUnits.HideSelection = false;
            this.listUnits.Location = new System.Drawing.Point(9, 7);
            this.listUnits.Margin = new System.Windows.Forms.Padding(2);
            this.listUnits.Name = "listUnits";
            this.listUnits.Size = new System.Drawing.Size(392, 331);
            this.listUnits.TabIndex = 2;
            this.listUnits.UseCompatibleStateImageBehavior = false;
            this.listUnits.View = System.Windows.Forms.View.List;
            // 
            // btnSel
            // 
            this.btnSel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSel.Location = new System.Drawing.Point(9, 340);
            this.btnSel.Name = "btnSel";
            this.btnSel.Size = new System.Drawing.Size(66, 20);
            this.btnSel.TabIndex = 7;
            this.btnSel.Text = "Select All";
            this.btnSel.UseVisualStyleBackColor = true;
            this.btnSel.Click += new System.EventHandler(this.btnSel_Click);
            // 
            // btnUnsel
            // 
            this.btnUnsel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUnsel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUnsel.Location = new System.Drawing.Point(77, 340);
            this.btnUnsel.Name = "btnUnsel";
            this.btnUnsel.Size = new System.Drawing.Size(71, 20);
            this.btnUnsel.TabIndex = 8;
            this.btnUnsel.Text = "Unselect All";
            this.btnUnsel.UseVisualStyleBackColor = true;
            this.btnUnsel.Click += new System.EventHandler(this.btnUnsel_Click);
            // 
            // groupYears
            // 
            this.groupYears.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupYears.Controls.Add(this.listYears);
            this.groupYears.Controls.Add(this.numYear);
            this.groupYears.Controls.Add(this.btnAddYear);
            this.groupYears.Controls.Add(this.checkAllYears);
            this.groupYears.Location = new System.Drawing.Point(9, 367);
            this.groupYears.Name = "groupYears";
            this.groupYears.Size = new System.Drawing.Size(392, 87);
            this.groupYears.TabIndex = 9;
            this.groupYears.TabStop = false;
            this.groupYears.Text = "Years";
            // 
            // listYears
            // 
            this.listYears.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listYears.FormattingEnabled = true;
            this.listYears.Location = new System.Drawing.Point(6, 14);
            this.listYears.MultiColumn = true;
            this.listYears.Name = "listYears";
            this.listYears.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listYears.Size = new System.Drawing.Size(283, 69);
            this.listYears.TabIndex = 3;
            this.listYears.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listYears_KeyUp);
            // 
            // numYear
            // 
            this.numYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.numYear.Location = new System.Drawing.Point(295, 53);
            this.numYear.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.numYear.Minimum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.numYear.Name = "numYear";
            this.numYear.Size = new System.Drawing.Size(50, 20);
            this.numYear.TabIndex = 2;
            this.numYear.Value = new decimal(new int[] {
            2019,
            0,
            0,
            0});
            // 
            // btnAddYear
            // 
            this.btnAddYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddYear.Location = new System.Drawing.Point(347, 53);
            this.btnAddYear.Name = "btnAddYear";
            this.btnAddYear.Size = new System.Drawing.Size(39, 20);
            this.btnAddYear.TabIndex = 1;
            this.btnAddYear.Text = "Add";
            this.btnAddYear.UseVisualStyleBackColor = true;
            this.btnAddYear.Click += new System.EventHandler(this.btnAddYear_Click);
            // 
            // checkAllYears
            // 
            this.checkAllYears.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkAllYears.AutoSize = true;
            this.checkAllYears.Location = new System.Drawing.Point(295, 33);
            this.checkAllYears.Name = "checkAllYears";
            this.checkAllYears.Size = new System.Drawing.Size(67, 17);
            this.checkAllYears.TabIndex = 0;
            this.checkAllYears.Text = "All Years";
            this.checkAllYears.UseVisualStyleBackColor = true;
            this.checkAllYears.CheckedChanged += new System.EventHandler(this.checkAllYears_CheckedChanged);
            // 
            // VCProjectContent
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(407, 493);
            this.Controls.Add(this.groupYears);
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
            this.MinimumSize = new System.Drawing.Size(410, 288);
            this.Name = "VCProjectContent";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Define Project Content";
            this.Load += new System.EventHandler(this.VCProjectContent_Load);
            this.groupYears.ResumeLayout(false);
            this.groupYears.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numYear)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListView listUnits;
        private System.Windows.Forms.Button btnSel;
        private System.Windows.Forms.Button btnUnsel;
        private System.Windows.Forms.GroupBox groupYears;
        private System.Windows.Forms.ListBox listYears;
        private System.Windows.Forms.NumericUpDown numYear;
        private System.Windows.Forms.Button btnAddYear;
        private System.Windows.Forms.CheckBox checkAllYears;
    }
}