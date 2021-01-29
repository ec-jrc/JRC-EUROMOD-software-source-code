namespace EM_UI.Dialogs
{
    partial class ExtractProjectForm
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
            this.radShowYears = new System.Windows.Forms.RadioButton();
            this.radShowSystems = new System.Windows.Forms.RadioButton();
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtProjectPath = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lstCountries = new System.Windows.Forms.ListBox();
            this.lstSystems = new System.Windows.Forms.ListBox();
            this.btnSelectPath = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radShowYears
            // 
            this.radShowYears.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radShowYears.AutoSize = true;
            this.radShowYears.Checked = true;
            this.radShowYears.Location = new System.Drawing.Point(284, 240);
            this.radShowYears.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radShowYears.Name = "radShowYears";
            this.radShowYears.Size = new System.Drawing.Size(82, 17);
            this.radShowYears.TabIndex = 10;
            this.radShowYears.TabStop = true;
            this.radShowYears.Text = "Show Years";
            this.radShowYears.UseVisualStyleBackColor = true;
            this.radShowYears.CheckedChanged += new System.EventHandler(this.radShow_CheckedChanged);
            // 
            // radShowSystems
            // 
            this.radShowSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radShowSystems.AutoSize = true;
            this.radShowSystems.Location = new System.Drawing.Point(374, 240);
            this.radShowSystems.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.radShowSystems.Name = "radShowSystems";
            this.radShowSystems.Size = new System.Drawing.Size(94, 17);
            this.radShowSystems.TabIndex = 11;
            this.radShowSystems.Text = "Show Systems";
            this.radShowSystems.UseVisualStyleBackColor = true;
            this.radShowSystems.CheckedChanged += new System.EventHandler(this.radShow_CheckedChanged);
            // 
            // txtProjectName
            // 
            this.txtProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectName.Location = new System.Drawing.Point(83, 272);
            this.txtProjectName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.Size = new System.Drawing.Size(197, 20);
            this.txtProjectName.TabIndex = 12;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 275);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 13;
            this.label1.Text = "Project Name";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 301);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Project Path";
            // 
            // txtProjectPath
            // 
            this.txtProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectPath.Location = new System.Drawing.Point(83, 298);
            this.txtProjectPath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.txtProjectPath.Name = "txtProjectPath";
            this.txtProjectPath.Size = new System.Drawing.Size(562, 20);
            this.txtProjectPath.TabIndex = 14;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(599, 333);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(503, 333);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 16;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lstCountries
            // 
            this.lstCountries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstCountries.FormattingEnabled = true;
            this.lstCountries.Location = new System.Drawing.Point(9, 10);
            this.lstCountries.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lstCountries.MultiColumn = true;
            this.lstCountries.Name = "lstCountries";
            this.lstCountries.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstCountries.Size = new System.Drawing.Size(271, 251);
            this.lstCountries.TabIndex = 18;
            this.lstCountries.SelectedIndexChanged += new System.EventHandler(this.lstCountries_SelectedIndexChanged);
            // 
            // lstSystems
            // 
            this.lstSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstSystems.FormattingEnabled = true;
            this.lstSystems.Location = new System.Drawing.Point(284, 10);
            this.lstSystems.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.lstSystems.MultiColumn = true;
            this.lstSystems.Name = "lstSystems";
            this.lstSystems.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.lstSystems.Size = new System.Drawing.Size(407, 225);
            this.lstSystems.TabIndex = 19;
            // 
            // btnSelectPath
            // 
            this.btnSelectPath.AccessibleDescription = "btnModellDataDir";
            this.btnSelectPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectPath.Image = global::EM_UI.Properties.Resources.Folder1;
            this.btnSelectPath.Location = new System.Drawing.Point(650, 288);
            this.btnSelectPath.Name = "btnSelectPath";
            this.btnSelectPath.Size = new System.Drawing.Size(40, 40);
            this.btnSelectPath.TabIndex = 20;
            this.btnSelectPath.UseVisualStyleBackColor = true;
            this.btnSelectPath.Click += new System.EventHandler(this.btnSelectPath_Click);
            // 
            // ExtractProjectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 371);
            this.Controls.Add(this.btnSelectPath);
            this.Controls.Add(this.lstSystems);
            this.Controls.Add(this.lstCountries);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtProjectPath);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtProjectName);
            this.Controls.Add(this.radShowSystems);
            this.Controls.Add(this.radShowYears);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExtractProjectForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Extract Project";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radShowYears;
        private System.Windows.Forms.RadioButton radShowSystems;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtProjectPath;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ListBox lstCountries;
        private System.Windows.Forms.ListBox lstSystems;
        private System.Windows.Forms.Button btnSelectPath;
    }
}