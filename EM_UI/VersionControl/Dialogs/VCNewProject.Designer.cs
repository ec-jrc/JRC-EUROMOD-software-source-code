namespace EM_UI.VersionControl.Dialogs
{
    partial class VCNewProject
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(VCNewProject));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.cmbBaseProjects = new System.Windows.Forms.ComboBox();
            this.cmbSelectVersion = new System.Windows.Forms.ComboBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnSelectProjectPath = new System.Windows.Forms.Button();
            this.txtProjectPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btnDefineContent = new System.Windows.Forms.Button();
            this.btnSelectBasePath = new System.Windows.Forms.Button();
            this.txtBasePath = new System.Windows.Forms.TextBox();
            this.chkNoBaseProject = new System.Windows.Forms.CheckBox();
            this.chkProjectOnVC = new System.Windows.Forms.CheckBox();
            this.chkProjectOnDisk = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtProjectName = new System.Windows.Forms.TextBox();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // cmbBaseProjects
            // 
            this.cmbBaseProjects.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbBaseProjects.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBaseProjects.Enabled = false;
            this.cmbBaseProjects.FormattingEnabled = true;
            this.helpProvider.SetHelpString(this.cmbBaseProjects, "Select the Project on Version Control that shall serve as the Base Project");
            this.cmbBaseProjects.Location = new System.Drawing.Point(104, 65);
            this.cmbBaseProjects.Margin = new System.Windows.Forms.Padding(2);
            this.cmbBaseProjects.Name = "cmbBaseProjects";
            this.helpProvider.SetShowHelp(this.cmbBaseProjects, true);
            this.cmbBaseProjects.Size = new System.Drawing.Size(308, 21);
            this.cmbBaseProjects.TabIndex = 4;
            this.cmbBaseProjects.SelectedIndexChanged += new System.EventHandler(this.cmbBaseProjects_SelectedIndexChanged);
            // 
            // cmbSelectVersion
            // 
            this.cmbSelectVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbSelectVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSelectVersion.Enabled = false;
            this.cmbSelectVersion.FormattingEnabled = true;
            this.helpProvider.SetHelpString(this.cmbSelectVersion, "Select the Version of the Project that shall serve as the Base Project");
            this.cmbSelectVersion.Location = new System.Drawing.Point(416, 65);
            this.cmbSelectVersion.Margin = new System.Windows.Forms.Padding(2);
            this.cmbSelectVersion.Name = "cmbSelectVersion";
            this.helpProvider.SetShowHelp(this.cmbSelectVersion, true);
            this.cmbSelectVersion.Size = new System.Drawing.Size(96, 21);
            this.cmbSelectVersion.TabIndex = 8;
            this.cmbSelectVersion.SelectedIndexChanged += new System.EventHandler(this.cmbSelectVersion_SelectedIndexChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(291, 197);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(70, 24);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "  Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(215, 197);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(70, 24);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnSelectProjectPath
            // 
            this.btnSelectProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectProjectPath.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectProjectPath.Image")));
            this.btnSelectProjectPath.Location = new System.Drawing.Point(524, 3);
            this.btnSelectProjectPath.Name = "btnSelectProjectPath";
            this.btnSelectProjectPath.Size = new System.Drawing.Size(40, 40);
            this.btnSelectProjectPath.TabIndex = 4;
            this.toolTips.SetToolTip(this.btnSelectProjectPath, "Select storage place for the New Project\'s Folder");
            this.btnSelectProjectPath.UseVisualStyleBackColor = true;
            this.btnSelectProjectPath.Click += new System.EventHandler(this.btnSelectProjectPath_Click);
            // 
            // txtProjectPath
            // 
            this.txtProjectPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectPath.Location = new System.Drawing.Point(81, 14);
            this.txtProjectPath.Margin = new System.Windows.Forms.Padding(2);
            this.txtProjectPath.Name = "txtProjectPath";
            this.txtProjectPath.Size = new System.Drawing.Size(440, 20);
            this.txtProjectPath.TabIndex = 1;
            this.toolTips.SetToolTip(this.txtProjectPath, "Store place for the New Project\'s Folder (see Project Name for name of this folde" +
        "r)");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Project Path";
            this.toolTips.SetToolTip(this.label1, "Store place for the New Project\'s Folder (see Project Name for name of this folde" +
        "r)");
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.cmbSelectVersion);
            this.groupBox1.Controls.Add(this.btnDefineContent);
            this.groupBox1.Controls.Add(this.btnSelectBasePath);
            this.groupBox1.Controls.Add(this.txtBasePath);
            this.groupBox1.Controls.Add(this.chkNoBaseProject);
            this.groupBox1.Controls.Add(this.chkProjectOnVC);
            this.groupBox1.Controls.Add(this.chkProjectOnDisk);
            this.groupBox1.Controls.Add(this.cmbBaseProjects);
            this.groupBox1.Location = new System.Drawing.Point(9, 68);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(554, 121);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Base Project";
            this.toolTips.SetToolTip(this.groupBox1, "Define initial content of the New Project");
            // 
            // btnDefineContent
            // 
            this.btnDefineContent.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnDefineContent.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnDefineContent.Location = new System.Drawing.Point(235, 89);
            this.btnDefineContent.Name = "btnDefineContent";
            this.btnDefineContent.Size = new System.Drawing.Size(87, 24);
            this.btnDefineContent.TabIndex = 6;
            this.btnDefineContent.Text = "Define Content";
            this.toolTips.SetToolTip(this.btnDefineContent, "Define the \"differences\" between the Base Project and the New Project, i.e. in/ex" +
        "clude specific countries and add-ons");
            this.btnDefineContent.UseVisualStyleBackColor = true;
            this.btnDefineContent.Click += new System.EventHandler(this.btnDefineContent_Click);
            // 
            // btnSelectBasePath
            // 
            this.btnSelectBasePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBasePath.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectBasePath.Image")));
            this.btnSelectBasePath.Location = new System.Drawing.Point(514, 31);
            this.btnSelectBasePath.Name = "btnSelectBasePath";
            this.btnSelectBasePath.Size = new System.Drawing.Size(34, 40);
            this.btnSelectBasePath.TabIndex = 7;
            this.toolTips.SetToolTip(this.btnSelectBasePath, "Select location of the Base Project\'s Folder");
            this.btnSelectBasePath.UseVisualStyleBackColor = true;
            this.btnSelectBasePath.Click += new System.EventHandler(this.btnSelectBasePath_Click);
            // 
            // txtBasePath
            // 
            this.txtBasePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBasePath.Location = new System.Drawing.Point(104, 41);
            this.txtBasePath.Margin = new System.Windows.Forms.Padding(2);
            this.txtBasePath.Name = "txtBasePath";
            this.txtBasePath.Size = new System.Drawing.Size(408, 20);
            this.txtBasePath.TabIndex = 2;
            this.toolTips.SetToolTip(this.txtBasePath, "Location of the Base Project\'s Folder on your hard-disk (or a network-drive)");
            // 
            // chkNoBaseProject
            // 
            this.chkNoBaseProject.AutoSize = true;
            this.chkNoBaseProject.Location = new System.Drawing.Point(4, 20);
            this.chkNoBaseProject.Margin = new System.Windows.Forms.Padding(2);
            this.chkNoBaseProject.Name = "chkNoBaseProject";
            this.chkNoBaseProject.Size = new System.Drawing.Size(103, 17);
            this.chkNoBaseProject.TabIndex = 5;
            this.chkNoBaseProject.Text = "No Base Project";
            this.toolTips.SetToolTip(this.chkNoBaseProject, "New Project is to be initially \"empty\", i.e. does not contain any countries or ad" +
        "d-ons, nor a variables file");
            this.chkNoBaseProject.UseVisualStyleBackColor = true;
            this.chkNoBaseProject.Click += new System.EventHandler(this.chkNoBaseProject_Click);
            // 
            // chkProjectOnVC
            // 
            this.chkProjectOnVC.AutoSize = true;
            this.chkProjectOnVC.Location = new System.Drawing.Point(4, 67);
            this.chkProjectOnVC.Margin = new System.Windows.Forms.Padding(2);
            this.chkProjectOnVC.Name = "chkProjectOnVC";
            this.chkProjectOnVC.Size = new System.Drawing.Size(91, 17);
            this.chkProjectOnVC.TabIndex = 3;
            this.chkProjectOnVC.Text = "Project on VC";
            this.toolTips.SetToolTip(this.chkProjectOnVC, "New Project is to be downloaded from Version Control (i.e. is a copy of a Project" +
        " stored on VC)");
            this.chkProjectOnVC.UseVisualStyleBackColor = true;
            this.chkProjectOnVC.Click += new System.EventHandler(this.chkProjectOnVC_Click);
            // 
            // chkProjectOnDisk
            // 
            this.chkProjectOnDisk.AutoSize = true;
            this.chkProjectOnDisk.Checked = true;
            this.chkProjectOnDisk.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkProjectOnDisk.Location = new System.Drawing.Point(4, 43);
            this.chkProjectOnDisk.Margin = new System.Windows.Forms.Padding(2);
            this.chkProjectOnDisk.Name = "chkProjectOnDisk";
            this.chkProjectOnDisk.Size = new System.Drawing.Size(98, 17);
            this.chkProjectOnDisk.TabIndex = 1;
            this.chkProjectOnDisk.Text = "Project on Disk";
            this.toolTips.SetToolTip(this.chkProjectOnDisk, "New Project is to be a copy of an existing Project stored on your hard-disk (or a" +
        " network-drive)");
            this.chkProjectOnDisk.UseVisualStyleBackColor = true;
            this.chkProjectOnDisk.Click += new System.EventHandler(this.chkProjectOnDisk_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 13);
            this.label2.TabIndex = 25;
            this.label2.Text = "Project Name";
            this.toolTips.SetToolTip(this.label2, "Name of the New Project = Folder used to store the New Project");
            // 
            // txtProjectName
            // 
            this.txtProjectName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtProjectName.Location = new System.Drawing.Point(81, 37);
            this.txtProjectName.Margin = new System.Windows.Forms.Padding(2);
            this.txtProjectName.Name = "txtProjectName";
            this.txtProjectName.Size = new System.Drawing.Size(282, 20);
            this.txtProjectName.TabIndex = 2;
            this.toolTips.SetToolTip(this.txtProjectName, "Name of the New Project = Folder used to store the New Project");
            // 
            // VCNewProject
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(574, 237);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtProjectName);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSelectProjectPath);
            this.Controls.Add(this.txtProjectPath);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_NewProject.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1590, 275);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(590, 275);
            this.Name = "VCNewProject";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "New Project";
            this.Load += new System.EventHandler(this.VCNewProject_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ComboBox cmbBaseProjects;
        private System.Windows.Forms.Button btnSelectProjectPath;
        private System.Windows.Forms.TextBox txtProjectPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnSelectBasePath;
        private System.Windows.Forms.TextBox txtBasePath;
        private System.Windows.Forms.CheckBox chkNoBaseProject;
        private System.Windows.Forms.CheckBox chkProjectOnVC;
        private System.Windows.Forms.CheckBox chkProjectOnDisk;
        private System.Windows.Forms.Button btnDefineContent;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtProjectName;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.ComboBox cmbSelectVersion;
    }
}