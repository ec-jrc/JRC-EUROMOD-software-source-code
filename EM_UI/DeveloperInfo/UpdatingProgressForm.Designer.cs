namespace EM_UI.DeveloperInfo
{
    partial class UpdatingProgressForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdatingProgressForm));
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.tbcUpdatingProgress = new System.Windows.Forms.TabControl();
            this.tabSettings = new System.Windows.Forms.TabPage();
            this.chkGenerateCombinations = new System.Windows.Forms.CheckBox();
            this.chkGenerateDatasetProgress = new System.Windows.Forms.CheckBox();
            this.chkGenerateSystemProgress = new System.Windows.Forms.CheckBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.grpExport = new System.Windows.Forms.GroupBox();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnSelectExportFolder = new System.Windows.Forms.Button();
            this.chkExportCombinations = new System.Windows.Forms.CheckBox();
            this.chkExportDatasetProgress = new System.Windows.Forms.CheckBox();
            this.chkExportSystemProgress = new System.Windows.Forms.CheckBox();
            this.txtCombinations = new System.Windows.Forms.TextBox();
            this.txtDatasetProgress = new System.Windows.Forms.TextBox();
            this.txtSystemProgress = new System.Windows.Forms.TextBox();
            this.txtFolder = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnNo = new System.Windows.Forms.Button();
            this.btnAll = new System.Windows.Forms.Button();
            this.lstCountries = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabSystems = new System.Windows.Forms.TabPage();
            this.dgvSystems = new System.Windows.Forms.DataGridView();
            this.tabDatasets = new System.Windows.Forms.TabPage();
            this.dgvDatasets = new System.Windows.Forms.DataGridView();
            this.colYearCollection = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colYearIncome = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colPrivate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tabCombinations = new System.Windows.Forms.TabPage();
            this.dgvCombinations = new System.Windows.Forms.DataGridView();
            this.btnClose = new System.Windows.Forms.Button();
            this.tbcUpdatingProgress.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.grpExport.SuspendLayout();
            this.tabSystems.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystems)).BeginInit();
            this.tabDatasets.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatasets)).BeginInit();
            this.tabCombinations.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvCombinations)).BeginInit();
            this.SuspendLayout();
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // tbcUpdatingProgress
            // 
            this.tbcUpdatingProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbcUpdatingProgress.Controls.Add(this.tabSettings);
            this.tbcUpdatingProgress.Controls.Add(this.tabSystems);
            this.tbcUpdatingProgress.Controls.Add(this.tabDatasets);
            this.tbcUpdatingProgress.Controls.Add(this.tabCombinations);
            this.tbcUpdatingProgress.Location = new System.Drawing.Point(12, 12);
            this.tbcUpdatingProgress.Name = "tbcUpdatingProgress";
            this.tbcUpdatingProgress.SelectedIndex = 0;
            this.tbcUpdatingProgress.Size = new System.Drawing.Size(751, 377);
            this.tbcUpdatingProgress.TabIndex = 0;
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.chkGenerateCombinations);
            this.tabSettings.Controls.Add(this.chkGenerateDatasetProgress);
            this.tabSettings.Controls.Add(this.chkGenerateSystemProgress);
            this.tabSettings.Controls.Add(this.btnGenerate);
            this.tabSettings.Controls.Add(this.grpExport);
            this.tabSettings.Controls.Add(this.btnNo);
            this.tabSettings.Controls.Add(this.btnAll);
            this.tabSettings.Controls.Add(this.lstCountries);
            this.tabSettings.Controls.Add(this.label1);
            this.tabSettings.Location = new System.Drawing.Point(4, 22);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Padding = new System.Windows.Forms.Padding(3);
            this.tabSettings.Size = new System.Drawing.Size(743, 351);
            this.tabSettings.TabIndex = 3;
            this.tabSettings.Text = "Settings";
            this.tabSettings.UseVisualStyleBackColor = true;
            // 
            // chkGenerateCombinations
            // 
            this.chkGenerateCombinations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGenerateCombinations.AutoSize = true;
            this.chkGenerateCombinations.Checked = true;
            this.chkGenerateCombinations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateCombinations.Location = new System.Drawing.Point(332, 264);
            this.chkGenerateCombinations.Name = "chkGenerateCombinations";
            this.chkGenerateCombinations.Size = new System.Drawing.Size(134, 17);
            this.chkGenerateCombinations.TabIndex = 14;
            this.chkGenerateCombinations.Text = "Available combinations";
            this.chkGenerateCombinations.UseVisualStyleBackColor = true;
            // 
            // chkGenerateDatasetProgress
            // 
            this.chkGenerateDatasetProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGenerateDatasetProgress.AutoSize = true;
            this.chkGenerateDatasetProgress.Checked = true;
            this.chkGenerateDatasetProgress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateDatasetProgress.Location = new System.Drawing.Point(332, 240);
            this.chkGenerateDatasetProgress.Name = "chkGenerateDatasetProgress";
            this.chkGenerateDatasetProgress.Size = new System.Drawing.Size(106, 17);
            this.chkGenerateDatasetProgress.TabIndex = 13;
            this.chkGenerateDatasetProgress.Text = "Dataset progress";
            this.chkGenerateDatasetProgress.UseVisualStyleBackColor = true;
            // 
            // chkGenerateSystemProgress
            // 
            this.chkGenerateSystemProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkGenerateSystemProgress.AutoSize = true;
            this.chkGenerateSystemProgress.Checked = true;
            this.chkGenerateSystemProgress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkGenerateSystemProgress.Location = new System.Drawing.Point(332, 215);
            this.chkGenerateSystemProgress.Name = "chkGenerateSystemProgress";
            this.chkGenerateSystemProgress.Size = new System.Drawing.Size(103, 17);
            this.chkGenerateSystemProgress.TabIndex = 12;
            this.chkGenerateSystemProgress.Text = "System progress";
            this.chkGenerateSystemProgress.UseVisualStyleBackColor = true;
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Image = ((System.Drawing.Image)(resources.GetObject("btnGenerate.Image")));
            this.btnGenerate.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnGenerate.Location = new System.Drawing.Point(472, 215);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(66, 68);
            this.btnGenerate.TabIndex = 5;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // grpExport
            // 
            this.grpExport.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpExport.Controls.Add(this.btnExport);
            this.grpExport.Controls.Add(this.btnSelectExportFolder);
            this.grpExport.Controls.Add(this.chkExportCombinations);
            this.grpExport.Controls.Add(this.chkExportDatasetProgress);
            this.grpExport.Controls.Add(this.chkExportSystemProgress);
            this.grpExport.Controls.Add(this.txtCombinations);
            this.grpExport.Controls.Add(this.txtDatasetProgress);
            this.grpExport.Controls.Add(this.txtSystemProgress);
            this.grpExport.Controls.Add(this.txtFolder);
            this.grpExport.Controls.Add(this.label5);
            this.grpExport.Controls.Add(this.label4);
            this.grpExport.Controls.Add(this.label3);
            this.grpExport.Controls.Add(this.label2);
            this.grpExport.Location = new System.Drawing.Point(144, 19);
            this.grpExport.Name = "grpExport";
            this.grpExport.Size = new System.Drawing.Size(593, 159);
            this.grpExport.TabIndex = 4;
            this.grpExport.TabStop = false;
            this.grpExport.Text = "Export to textfiles";
            // 
            // btnExport
            // 
            this.btnExport.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnExport.Location = new System.Drawing.Point(233, 124);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(75, 23);
            this.btnExport.TabIndex = 11;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnSelectExportFolder
            // 
            this.btnSelectExportFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectExportFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectExportFolder.Image")));
            this.btnSelectExportFolder.Location = new System.Drawing.Point(543, 32);
            this.btnSelectExportFolder.Name = "btnSelectExportFolder";
            this.btnSelectExportFolder.Size = new System.Drawing.Size(40, 40);
            this.btnSelectExportFolder.TabIndex = 2;
            this.btnSelectExportFolder.UseVisualStyleBackColor = true;
            this.btnSelectExportFolder.Click += new System.EventHandler(this.btnSelectExportFolder_Click);
            // 
            // chkExportCombinations
            // 
            this.chkExportCombinations.AutoSize = true;
            this.chkExportCombinations.Checked = true;
            this.chkExportCombinations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportCombinations.Location = new System.Drawing.Point(390, 92);
            this.chkExportCombinations.Name = "chkExportCombinations";
            this.chkExportCombinations.Size = new System.Drawing.Size(15, 14);
            this.chkExportCombinations.TabIndex = 10;
            this.chkExportCombinations.UseVisualStyleBackColor = true;
            this.chkExportCombinations.CheckedChanged += new System.EventHandler(this.chkExportCombinations_CheckedChanged);
            // 
            // chkExportDatasetProgress
            // 
            this.chkExportDatasetProgress.AutoSize = true;
            this.chkExportDatasetProgress.Checked = true;
            this.chkExportDatasetProgress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportDatasetProgress.Location = new System.Drawing.Point(198, 92);
            this.chkExportDatasetProgress.Name = "chkExportDatasetProgress";
            this.chkExportDatasetProgress.Size = new System.Drawing.Size(15, 14);
            this.chkExportDatasetProgress.TabIndex = 9;
            this.chkExportDatasetProgress.UseVisualStyleBackColor = true;
            this.chkExportDatasetProgress.CheckedChanged += new System.EventHandler(this.chkExportDatasetProgress_CheckedChanged);
            // 
            // chkExportSystemProgress
            // 
            this.chkExportSystemProgress.AutoSize = true;
            this.chkExportSystemProgress.Checked = true;
            this.chkExportSystemProgress.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkExportSystemProgress.Location = new System.Drawing.Point(6, 92);
            this.chkExportSystemProgress.Name = "chkExportSystemProgress";
            this.chkExportSystemProgress.Size = new System.Drawing.Size(15, 14);
            this.chkExportSystemProgress.TabIndex = 8;
            this.chkExportSystemProgress.UseVisualStyleBackColor = true;
            this.chkExportSystemProgress.CheckedChanged += new System.EventHandler(this.chkExportSystemProgress_CheckedChanged);
            // 
            // txtCombinations
            // 
            this.txtCombinations.Location = new System.Drawing.Point(408, 92);
            this.txtCombinations.Name = "txtCombinations";
            this.txtCombinations.Size = new System.Drawing.Size(175, 20);
            this.txtCombinations.TabIndex = 7;
            this.txtCombinations.Text = "UpdatingProgress_Combinations.txt";
            // 
            // txtDatasetProgress
            // 
            this.txtDatasetProgress.Location = new System.Drawing.Point(216, 92);
            this.txtDatasetProgress.Name = "txtDatasetProgress";
            this.txtDatasetProgress.Size = new System.Drawing.Size(155, 20);
            this.txtDatasetProgress.TabIndex = 6;
            this.txtDatasetProgress.Text = "UpdatingProgress_Datasets.txt";
            // 
            // txtSystemProgress
            // 
            this.txtSystemProgress.Location = new System.Drawing.Point(24, 92);
            this.txtSystemProgress.Name = "txtSystemProgress";
            this.txtSystemProgress.Size = new System.Drawing.Size(155, 20);
            this.txtSystemProgress.TabIndex = 5;
            this.txtSystemProgress.Text = "UpdatingProgress_Systems.txt";
            // 
            // txtFolder
            // 
            this.txtFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFolder.Location = new System.Drawing.Point(6, 43);
            this.txtFolder.Name = "txtFolder";
            this.txtFolder.Size = new System.Drawing.Size(531, 20);
            this.txtFolder.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(387, 76);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(148, 13);
            this.label5.TabIndex = 3;
            this.label5.Text = "File for available combinations";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(195, 76);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(119, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "File for dataset progress";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "File for system progress";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(36, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Folder";
            // 
            // btnNo
            // 
            this.btnNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnNo.Location = new System.Drawing.Point(74, 319);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(30, 23);
            this.btnNo.TabIndex = 3;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // btnAll
            // 
            this.btnAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAll.Location = new System.Drawing.Point(34, 319);
            this.btnAll.Name = "btnAll";
            this.btnAll.Size = new System.Drawing.Size(30, 23);
            this.btnAll.TabIndex = 2;
            this.btnAll.Text = "All";
            this.btnAll.UseVisualStyleBackColor = true;
            this.btnAll.Click += new System.EventHandler(this.btnAll_Click);
            // 
            // lstCountries
            // 
            this.lstCountries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstCountries.CheckOnClick = true;
            this.lstCountries.FormattingEnabled = true;
            this.lstCountries.Location = new System.Drawing.Point(9, 19);
            this.lstCountries.Name = "lstCountries";
            this.lstCountries.Size = new System.Drawing.Size(120, 289);
            this.lstCountries.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Included countries";
            // 
            // tabSystems
            // 
            this.tabSystems.Controls.Add(this.dgvSystems);
            this.tabSystems.Location = new System.Drawing.Point(4, 22);
            this.tabSystems.Name = "tabSystems";
            this.tabSystems.Padding = new System.Windows.Forms.Padding(3);
            this.tabSystems.Size = new System.Drawing.Size(743, 351);
            this.tabSystems.TabIndex = 0;
            this.tabSystems.Text = "Systems";
            this.tabSystems.UseVisualStyleBackColor = true;
            // 
            // dgvSystems
            // 
            this.dgvSystems.AllowUserToAddRows = false;
            this.dgvSystems.AllowUserToDeleteRows = false;
            this.dgvSystems.AllowUserToResizeRows = false;
            this.dgvSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSystems.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvSystems.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvSystems.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvSystems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSystems.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            this.dgvSystems.Location = new System.Drawing.Point(6, 6);
            this.dgvSystems.MultiSelect = false;
            this.dgvSystems.Name = "dgvSystems";
            this.dgvSystems.ReadOnly = true;
            this.dgvSystems.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvSystems.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvSystems.ShowCellErrors = false;
            this.dgvSystems.ShowCellToolTips = false;
            this.dgvSystems.ShowEditingIcon = false;
            this.dgvSystems.ShowRowErrors = false;
            this.dgvSystems.Size = new System.Drawing.Size(731, 339);
            this.dgvSystems.TabIndex = 0;
            // 
            // tabDatasets
            // 
            this.tabDatasets.Controls.Add(this.dgvDatasets);
            this.tabDatasets.Location = new System.Drawing.Point(4, 22);
            this.tabDatasets.Name = "tabDatasets";
            this.tabDatasets.Padding = new System.Windows.Forms.Padding(3);
            this.tabDatasets.Size = new System.Drawing.Size(743, 351);
            this.tabDatasets.TabIndex = 1;
            this.tabDatasets.Text = "Datasets";
            this.tabDatasets.UseVisualStyleBackColor = true;
            // 
            // dgvDatasets
            // 
            this.dgvDatasets.AllowUserToAddRows = false;
            this.dgvDatasets.AllowUserToDeleteRows = false;
            this.dgvDatasets.AllowUserToResizeRows = false;
            this.dgvDatasets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDatasets.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvDatasets.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvDatasets.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvDatasets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDatasets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colYearCollection,
            this.colYearIncome,
            this.colPrivate});
            this.dgvDatasets.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            this.dgvDatasets.Location = new System.Drawing.Point(6, 6);
            this.dgvDatasets.MultiSelect = false;
            this.dgvDatasets.Name = "dgvDatasets";
            this.dgvDatasets.ReadOnly = true;
            this.dgvDatasets.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvDatasets.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvDatasets.ShowCellErrors = false;
            this.dgvDatasets.ShowCellToolTips = false;
            this.dgvDatasets.ShowEditingIcon = false;
            this.dgvDatasets.ShowRowErrors = false;
            this.dgvDatasets.Size = new System.Drawing.Size(731, 339);
            this.dgvDatasets.TabIndex = 1;
            // 
            // colYearCollection
            // 
            this.colYearCollection.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colYearCollection.HeaderText = "Collected";
            this.colYearCollection.Name = "colYearCollection";
            this.colYearCollection.ReadOnly = true;
            this.colYearCollection.Width = 76;
            // 
            // colYearIncome
            // 
            this.colYearIncome.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colYearIncome.HeaderText = "Income";
            this.colYearIncome.Name = "colYearIncome";
            this.colYearIncome.ReadOnly = true;
            this.colYearIncome.Width = 67;
            // 
            // colPrivate
            // 
            this.colPrivate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colPrivate.HeaderText = "Private";
            this.colPrivate.Name = "colPrivate";
            this.colPrivate.ReadOnly = true;
            this.colPrivate.Width = 65;
            // 
            // tabCombinations
            // 
            this.tabCombinations.Controls.Add(this.dgvCombinations);
            this.tabCombinations.Location = new System.Drawing.Point(4, 22);
            this.tabCombinations.Name = "tabCombinations";
            this.tabCombinations.Padding = new System.Windows.Forms.Padding(3);
            this.tabCombinations.Size = new System.Drawing.Size(743, 351);
            this.tabCombinations.TabIndex = 2;
            this.tabCombinations.Text = "Combinations";
            this.tabCombinations.UseVisualStyleBackColor = true;
            // 
            // dgvCombinations
            // 
            this.dgvCombinations.AllowUserToAddRows = false;
            this.dgvCombinations.AllowUserToDeleteRows = false;
            this.dgvCombinations.AllowUserToResizeRows = false;
            this.dgvCombinations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvCombinations.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dgvCombinations.BackgroundColor = System.Drawing.SystemColors.Window;
            this.dgvCombinations.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            this.dgvCombinations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvCombinations.ColumnHeadersVisible = false;
            this.dgvCombinations.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnF2;
            this.dgvCombinations.Location = new System.Drawing.Point(6, 6);
            this.dgvCombinations.MultiSelect = false;
            this.dgvCombinations.Name = "dgvCombinations";
            this.dgvCombinations.ReadOnly = true;
            this.dgvCombinations.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvCombinations.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvCombinations.ShowCellErrors = false;
            this.dgvCombinations.ShowCellToolTips = false;
            this.dgvCombinations.ShowEditingIcon = false;
            this.dgvCombinations.ShowRowErrors = false;
            this.dgvCombinations.Size = new System.Drawing.Size(731, 339);
            this.dgvCombinations.TabIndex = 2;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(688, 398);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            // 
            // UpdatingProgressForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(775, 433);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.tbcUpdatingProgress);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_UpdatingProgress.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdatingProgressForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Updating Progress";
            this.Load += new System.EventHandler(this.UpdatingProgressForm_Load);
            this.tbcUpdatingProgress.ResumeLayout(false);
            this.tabSettings.ResumeLayout(false);
            this.tabSettings.PerformLayout();
            this.grpExport.ResumeLayout(false);
            this.grpExport.PerformLayout();
            this.tabSystems.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystems)).EndInit();
            this.tabDatasets.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDatasets)).EndInit();
            this.tabCombinations.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvCombinations)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.TabControl tbcUpdatingProgress;
        private System.Windows.Forms.TabPage tabSystems;
        private System.Windows.Forms.TabPage tabDatasets;
        private System.Windows.Forms.TabPage tabCombinations;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TabPage tabSettings;
        private System.Windows.Forms.GroupBox grpExport;
        private System.Windows.Forms.CheckBox chkExportCombinations;
        private System.Windows.Forms.CheckBox chkExportDatasetProgress;
        private System.Windows.Forms.CheckBox chkExportSystemProgress;
        private System.Windows.Forms.TextBox txtCombinations;
        private System.Windows.Forms.TextBox txtDatasetProgress;
        private System.Windows.Forms.TextBox txtSystemProgress;
        private System.Windows.Forms.TextBox txtFolder;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Button btnAll;
        private System.Windows.Forms.CheckedListBox lstCountries;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSelectExportFolder;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.CheckBox chkGenerateCombinations;
        private System.Windows.Forms.CheckBox chkGenerateDatasetProgress;
        private System.Windows.Forms.CheckBox chkGenerateSystemProgress;
        private System.Windows.Forms.DataGridView dgvSystems;
        private System.Windows.Forms.DataGridView dgvDatasets;
        private System.Windows.Forms.DataGridView dgvCombinations;
        private System.Windows.Forms.DataGridViewTextBoxColumn colYearCollection;
        private System.Windows.Forms.DataGridViewTextBoxColumn colYearIncome;
        private System.Windows.Forms.DataGridViewTextBoxColumn colPrivate;
    }
}