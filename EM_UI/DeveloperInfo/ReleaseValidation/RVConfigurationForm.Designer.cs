namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    partial class RVConfigurationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RVConfigurationForm));
            this.btnClose = new System.Windows.Forms.Button();
            this.lstValidationItems = new System.Windows.Forms.CheckedListBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkShowProblemsOnly = new System.Windows.Forms.CheckBox();
            this.btnNoItems = new System.Windows.Forms.Button();
            this.btnAllItems = new System.Windows.Forms.Button();
            this.btnNoCountries = new System.Windows.Forms.Button();
            this.btnAllCountries = new System.Windows.Forms.Button();
            this.btnValidate = new System.Windows.Forms.Button();
            this.lstCountries = new System.Windows.Forms.CheckedListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCreateInfoFile = new System.Windows.Forms.Button();
            this.txtInfoOutputFile = new System.Windows.Forms.TextBox();
            this.txtInfoOutputFolder = new System.Windows.Forms.TextBox();
            this.btnSelectInfoOutputFolder = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.toolTips = new System.Windows.Forms.ToolTip(this.components);
            this.txtFolderVersion = new System.Windows.Forms.TextBox();
            this.btnSelectFolderVersion = new System.Windows.Forms.Button();
            this.btnCompareVersions = new System.Windows.Forms.Button();
            this.txtDataFolder = new System.Windows.Forms.TextBox();
            this.btnSelectInputFolder = new System.Windows.Forms.Button();
            this.txtDataNamePattern = new System.Windows.Forms.TextBox();
            this.btnAddCustomData = new System.Windows.Forms.Button();
            this.btnDeleteCustomData = new System.Windows.Forms.Button();
            this.btnCheckHHLevel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.listCustomData = new System.Windows.Forms.ListBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(568, 564);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(59, 25);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // lstValidationItems
            // 
            this.lstValidationItems.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstValidationItems.CheckOnClick = true;
            this.lstValidationItems.FormattingEnabled = true;
            this.lstValidationItems.Location = new System.Drawing.Point(439, 17);
            this.lstValidationItems.Margin = new System.Windows.Forms.Padding(2);
            this.lstValidationItems.Name = "lstValidationItems";
            this.lstValidationItems.Size = new System.Drawing.Size(169, 184);
            this.lstValidationItems.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.groupBox1.Controls.Add(this.chkShowProblemsOnly);
            this.groupBox1.Controls.Add(this.btnNoItems);
            this.groupBox1.Controls.Add(this.btnAllItems);
            this.groupBox1.Controls.Add(this.btnNoCountries);
            this.groupBox1.Controls.Add(this.btnAllCountries);
            this.groupBox1.Controls.Add(this.btnValidate);
            this.groupBox1.Controls.Add(this.lstCountries);
            this.groupBox1.Controls.Add(this.lstValidationItems);
            this.groupBox1.Location = new System.Drawing.Point(9, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(619, 251);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Validate ...";
            // 
            // chkShowProblemsOnly
            // 
            this.chkShowProblemsOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkShowProblemsOnly.AutoSize = true;
            this.chkShowProblemsOnly.Location = new System.Drawing.Point(435, 231);
            this.chkShowProblemsOnly.Margin = new System.Windows.Forms.Padding(2);
            this.chkShowProblemsOnly.Name = "chkShowProblemsOnly";
            this.chkShowProblemsOnly.Size = new System.Drawing.Size(123, 17);
            this.chkShowProblemsOnly.TabIndex = 14;
            this.chkShowProblemsOnly.Text = "Show Problems Only";
            this.toolTips.SetToolTip(this.chkShowProblemsOnly, "If ticked, results do not contain items which passed the check (if not ticked suc" +
        "h items are shown with an ok-symbol)");
            this.chkShowProblemsOnly.UseVisualStyleBackColor = true;
            // 
            // btnNoItems
            // 
            this.btnNoItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNoItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNoItems.Location = new System.Drawing.Point(493, 207);
            this.btnNoItems.Name = "btnNoItems";
            this.btnNoItems.Size = new System.Drawing.Size(51, 20);
            this.btnNoItems.TabIndex = 13;
            this.btnNoItems.Text = "No Item";
            this.toolTips.SetToolTip(this.btnNoItems, "Unselect all validation items");
            this.btnNoItems.UseVisualStyleBackColor = true;
            this.btnNoItems.Click += new System.EventHandler(this.btnAllNo_Click);
            // 
            // btnAllItems
            // 
            this.btnAllItems.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAllItems.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAllItems.Location = new System.Drawing.Point(439, 207);
            this.btnAllItems.Name = "btnAllItems";
            this.btnAllItems.Size = new System.Drawing.Size(51, 20);
            this.btnAllItems.TabIndex = 12;
            this.btnAllItems.Text = "All Items";
            this.toolTips.SetToolTip(this.btnAllItems, "Select all validation items");
            this.btnAllItems.UseVisualStyleBackColor = true;
            this.btnAllItems.Click += new System.EventHandler(this.btnAllNo_Click);
            // 
            // btnNoCountries
            // 
            this.btnNoCountries.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNoCountries.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnNoCountries.Location = new System.Drawing.Point(85, 207);
            this.btnNoCountries.Name = "btnNoCountries";
            this.btnNoCountries.Size = new System.Drawing.Size(75, 20);
            this.btnNoCountries.TabIndex = 11;
            this.btnNoCountries.Text = "No Country";
            this.toolTips.SetToolTip(this.btnNoCountries, "Unselect all countries");
            this.btnNoCountries.UseVisualStyleBackColor = true;
            this.btnNoCountries.Click += new System.EventHandler(this.btnAllNo_Click);
            // 
            // btnAllCountries
            // 
            this.btnAllCountries.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAllCountries.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAllCountries.Location = new System.Drawing.Point(7, 207);
            this.btnAllCountries.Name = "btnAllCountries";
            this.btnAllCountries.Size = new System.Drawing.Size(75, 20);
            this.btnAllCountries.TabIndex = 10;
            this.btnAllCountries.Text = "All Countries";
            this.toolTips.SetToolTip(this.btnAllCountries, "Select all countries");
            this.btnAllCountries.UseVisualStyleBackColor = true;
            this.btnAllCountries.Click += new System.EventHandler(this.btnAllNo_Click);
            // 
            // btnValidate
            // 
            this.btnValidate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnValidate.Image = ((System.Drawing.Image)(resources.GetObject("btnValidate.Image")));
            this.btnValidate.Location = new System.Drawing.Point(574, 211);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(33, 36);
            this.btnValidate.TabIndex = 9;
            this.btnValidate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTips.SetToolTip(this.btnValidate, "Validate whether the selected countries fulfill Public Release requirements for t" +
        "he selected validation items");
            this.btnValidate.UseVisualStyleBackColor = true;
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // lstCountries
            // 
            this.lstCountries.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstCountries.CheckOnClick = true;
            this.lstCountries.FormattingEnabled = true;
            this.lstCountries.Location = new System.Drawing.Point(7, 17);
            this.lstCountries.Margin = new System.Windows.Forms.Padding(2);
            this.lstCountries.MultiColumn = true;
            this.lstCountries.Name = "lstCountries";
            this.lstCountries.Size = new System.Drawing.Size(428, 184);
            this.lstCountries.TabIndex = 7;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.btnCreateInfoFile);
            this.groupBox2.Controls.Add(this.txtInfoOutputFile);
            this.groupBox2.Controls.Add(this.txtInfoOutputFolder);
            this.groupBox2.Controls.Add(this.btnSelectInfoOutputFolder);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Location = new System.Drawing.Point(9, 266);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(619, 86);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Create Info File";
            // 
            // btnCreateInfoFile
            // 
            this.btnCreateInfoFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCreateInfoFile.Image = global::EM_UI.Properties.Resources.ValidationInfo;
            this.btnCreateInfoFile.Location = new System.Drawing.Point(574, 43);
            this.btnCreateInfoFile.Name = "btnCreateInfoFile";
            this.btnCreateInfoFile.Size = new System.Drawing.Size(30, 38);
            this.btnCreateInfoFile.TabIndex = 10;
            this.btnCreateInfoFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTips.SetToolTip(this.btnCreateInfoFile, "Generate Excel file with country information (available systems, datasets, add-on" +
        "s, private parts, ...)");
            this.btnCreateInfoFile.UseVisualStyleBackColor = true;
            this.btnCreateInfoFile.Click += new System.EventHandler(this.btnCreateInfoFile_Click);
            // 
            // txtInfoOutputFile
            // 
            this.txtInfoOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtInfoOutputFile.Location = new System.Drawing.Point(80, 45);
            this.txtInfoOutputFile.Margin = new System.Windows.Forms.Padding(2);
            this.txtInfoOutputFile.Name = "txtInfoOutputFile";
            this.txtInfoOutputFile.Size = new System.Drawing.Size(120, 20);
            this.txtInfoOutputFile.TabIndex = 12;
            this.toolTips.SetToolTip(this.txtInfoOutputFile, "Insert name of info file (without path)");
            // 
            // txtInfoOutputFolder
            // 
            this.txtInfoOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfoOutputFolder.Location = new System.Drawing.Point(80, 20);
            this.txtInfoOutputFolder.Margin = new System.Windows.Forms.Padding(2);
            this.txtInfoOutputFolder.Name = "txtInfoOutputFolder";
            this.txtInfoOutputFolder.Size = new System.Drawing.Size(501, 20);
            this.txtInfoOutputFolder.TabIndex = 11;
            this.toolTips.SetToolTip(this.txtInfoOutputFolder, "Insert storage folder for info file");
            // 
            // btnSelectInfoOutputFolder
            // 
            this.btnSelectInfoOutputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInfoOutputFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectInfoOutputFolder.Location = new System.Drawing.Point(586, 18);
            this.btnSelectInfoOutputFolder.Name = "btnSelectInfoOutputFolder";
            this.btnSelectInfoOutputFolder.Size = new System.Drawing.Size(21, 20);
            this.btnSelectInfoOutputFolder.TabIndex = 10;
            this.btnSelectInfoOutputFolder.Text = "°°°";
            this.btnSelectInfoOutputFolder.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTips.SetToolTip(this.btnSelectInfoOutputFolder, "Select storage folder for info file");
            this.btnSelectInfoOutputFolder.UseVisualStyleBackColor = true;
            this.btnSelectInfoOutputFolder.Click += new System.EventHandler(this.btnSelectInfoOutputFolder_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 46);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Output File";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 21);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output Folder";
            // 
            // txtFolderVersion
            // 
            this.txtFolderVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolderVersion.Location = new System.Drawing.Point(80, 20);
            this.txtFolderVersion.Margin = new System.Windows.Forms.Padding(2);
            this.txtFolderVersion.Name = "txtFolderVersion";
            this.txtFolderVersion.Size = new System.Drawing.Size(501, 20);
            this.txtFolderVersion.TabIndex = 14;
            this.toolTips.SetToolTip(this.txtFolderVersion, "Insert storage folder of the version to compare with");
            // 
            // btnSelectFolderVersion
            // 
            this.btnSelectFolderVersion.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectFolderVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectFolderVersion.Location = new System.Drawing.Point(586, 18);
            this.btnSelectFolderVersion.Name = "btnSelectFolderVersion";
            this.btnSelectFolderVersion.Size = new System.Drawing.Size(21, 20);
            this.btnSelectFolderVersion.TabIndex = 13;
            this.btnSelectFolderVersion.Text = "°°°";
            this.btnSelectFolderVersion.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTips.SetToolTip(this.btnSelectFolderVersion, "Select storage folder of version to compare with");
            this.btnSelectFolderVersion.UseVisualStyleBackColor = true;
            this.btnSelectFolderVersion.Click += new System.EventHandler(this.btnSelectFolderVersion_Click);
            // 
            // btnCompareVersions
            // 
            this.btnCompareVersions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCompareVersions.Image = global::EM_UI.Properties.Resources.Compare;
            this.btnCompareVersions.Location = new System.Drawing.Point(574, 41);
            this.btnCompareVersions.Name = "btnCompareVersions";
            this.btnCompareVersions.Size = new System.Drawing.Size(30, 34);
            this.btnCompareVersions.TabIndex = 13;
            this.btnCompareVersions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTips.SetToolTip(this.btnCompareVersions, "Assess differences of the loaded version and the version stored at \'Folder Versio" +
        "n\' (added/deleted datasets/systems/policies)");
            this.btnCompareVersions.UseVisualStyleBackColor = true;
            this.btnCompareVersions.Click += new System.EventHandler(this.btnCompareVersions_Click);
            // 
            // txtDataFolder
            // 
            this.txtDataFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDataFolder.Location = new System.Drawing.Point(80, 18);
            this.txtDataFolder.Margin = new System.Windows.Forms.Padding(2);
            this.txtDataFolder.Name = "txtDataFolder";
            this.txtDataFolder.Size = new System.Drawing.Size(344, 20);
            this.txtDataFolder.TabIndex = 1;
            this.toolTips.SetToolTip(this.txtDataFolder, "Insert folder containing EUROMOD input data");
            // 
            // btnSelectInputFolder
            // 
            this.btnSelectInputFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectInputFolder.Font = new System.Drawing.Font("Microsoft Sans Serif", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSelectInputFolder.Location = new System.Drawing.Point(429, 18);
            this.btnSelectInputFolder.Name = "btnSelectInputFolder";
            this.btnSelectInputFolder.Size = new System.Drawing.Size(21, 20);
            this.btnSelectInputFolder.TabIndex = 2;
            this.btnSelectInputFolder.Text = "°°°";
            this.btnSelectInputFolder.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.toolTips.SetToolTip(this.btnSelectInputFolder, "Select storage folder for info file");
            this.btnSelectInputFolder.UseVisualStyleBackColor = true;
            this.btnSelectInputFolder.Click += new System.EventHandler(this.btnSelectInputFolder_Click);
            // 
            // txtDataNamePattern
            // 
            this.txtDataNamePattern.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDataNamePattern.Location = new System.Drawing.Point(487, 18);
            this.txtDataNamePattern.Margin = new System.Windows.Forms.Padding(2);
            this.txtDataNamePattern.Name = "txtDataNamePattern";
            this.txtDataNamePattern.Size = new System.Drawing.Size(120, 20);
            this.txtDataNamePattern.TabIndex = 3;
            this.txtDataNamePattern.Text = "??_????_a?.txt";
            this.toolTips.SetToolTip(this.txtDataNamePattern, "Insert name pattern for files to test");
            // 
            // btnAddCustomData
            // 
            this.btnAddCustomData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddCustomData.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddCustomData.Location = new System.Drawing.Point(429, 47);
            this.btnAddCustomData.Name = "btnAddCustomData";
            this.btnAddCustomData.Size = new System.Drawing.Size(47, 20);
            this.btnAddCustomData.TabIndex = 4;
            this.btnAddCustomData.Text = "Add";
            this.toolTips.SetToolTip(this.btnAddCustomData, "Select all validation items");
            this.btnAddCustomData.UseVisualStyleBackColor = true;
            this.btnAddCustomData.Click += new System.EventHandler(this.btnAddCustomData_Click);
            // 
            // btnDeleteCustomData
            // 
            this.btnDeleteCustomData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteCustomData.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteCustomData.Location = new System.Drawing.Point(429, 68);
            this.btnDeleteCustomData.Name = "btnDeleteCustomData";
            this.btnDeleteCustomData.Size = new System.Drawing.Size(47, 20);
            this.btnDeleteCustomData.TabIndex = 5;
            this.btnDeleteCustomData.Text = "Delete";
            this.toolTips.SetToolTip(this.btnDeleteCustomData, "Select all validation items");
            this.btnDeleteCustomData.UseVisualStyleBackColor = true;
            this.btnDeleteCustomData.Click += new System.EventHandler(this.btnDeleteCustomData_Click);
            // 
            // btnCheckHHLevel
            // 
            this.btnCheckHHLevel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCheckHHLevel.Image = global::EM_UI.Properties.Resources.merge_mixed;
            this.btnCheckHHLevel.Location = new System.Drawing.Point(574, 71);
            this.btnCheckHHLevel.Name = "btnCheckHHLevel";
            this.btnCheckHHLevel.Size = new System.Drawing.Size(33, 32);
            this.btnCheckHHLevel.TabIndex = 6;
            this.btnCheckHHLevel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTips.SetToolTip(this.btnCheckHHLevel, "Assess differences of the loaded version and the version stored at \'Folder Versio" +
        "n\' (added/deleted datasets/systems/policies)");
            this.btnCheckHHLevel.UseVisualStyleBackColor = true;
            this.btnCheckHHLevel.Click += new System.EventHandler(this.btnCheckHHLevel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btnCompareVersions);
            this.groupBox3.Controls.Add(this.txtFolderVersion);
            this.groupBox3.Controls.Add(this.btnSelectFolderVersion);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(9, 357);
            this.groupBox3.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox3.Size = new System.Drawing.Size(619, 80);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Compare with another Version";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 21);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(74, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Folder Version";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.btnCheckHHLevel);
            this.groupBox4.Controls.Add(this.btnDeleteCustomData);
            this.groupBox4.Controls.Add(this.btnAddCustomData);
            this.groupBox4.Controls.Add(this.label7);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Controls.Add(this.listCustomData);
            this.groupBox4.Controls.Add(this.txtDataNamePattern);
            this.groupBox4.Controls.Add(this.label5);
            this.groupBox4.Controls.Add(this.txtDataFolder);
            this.groupBox4.Controls.Add(this.btnSelectInputFolder);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Location = new System.Drawing.Point(9, 442);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(619, 116);
            this.groupBox4.TabIndex = 10;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Check Data for correct HH-level Variables";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(5, 62);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 20;
            this.label7.Text = "(additional)";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 47);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(66, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Custom Files";
            // 
            // listCustomData
            // 
            this.listCustomData.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listCustomData.FormattingEnabled = true;
            this.listCustomData.Location = new System.Drawing.Point(80, 47);
            this.listCustomData.Name = "listCustomData";
            this.listCustomData.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listCustomData.Size = new System.Drawing.Size(344, 56);
            this.listCustomData.TabIndex = 18;
            this.listCustomData.KeyUp += new System.Windows.Forms.KeyEventHandler(this.listCustomData_KeyUp);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(456, 21);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(28, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Files";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(4, 19);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(62, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Data Folder";
            // 
            // RVConfigurationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(638, 596);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnClose);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RVConfigurationForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.CheckedListBox lstValidationItems;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectInfoOutputFolder;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.CheckedListBox lstCountries;
        private System.Windows.Forms.ToolTip toolTips;
        private System.Windows.Forms.Button btnCreateInfoFile;
        private System.Windows.Forms.TextBox txtInfoOutputFile;
        private System.Windows.Forms.TextBox txtInfoOutputFolder;
        private System.Windows.Forms.Button btnNoItems;
        private System.Windows.Forms.Button btnAllItems;
        private System.Windows.Forms.Button btnNoCountries;
        private System.Windows.Forms.Button btnAllCountries;
        private System.Windows.Forms.CheckBox chkShowProblemsOnly;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnCompareVersions;
        private System.Windows.Forms.TextBox txtFolderVersion;
        private System.Windows.Forms.Button btnSelectFolderVersion;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtDataFolder;
        private System.Windows.Forms.Button btnSelectInputFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtDataNamePattern;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnCheckHHLevel;
        private System.Windows.Forms.Button btnDeleteCustomData;
        private System.Windows.Forms.Button btnAddCustomData;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ListBox listCustomData;
    }
}