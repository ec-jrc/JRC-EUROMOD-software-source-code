namespace EM_UI.Dialogs
{
    partial class ConfigureDataForm
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
            this.labelCountry = new System.Windows.Forms.Label();
            this.txtYearCollection = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtYearIncome = new System.Windows.Forms.TextBox();
            this.cboCurrency = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.chkPrivate = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.cboDecimalSign = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.btnAddDatabase = new System.Windows.Forms.Button();
            this.btnDeleteDatabase = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.chkUseCommonDefault = new System.Windows.Forms.CheckBox();
            this.tipConfigureDatabases = new System.Windows.Forms.ToolTip();
            this.btnRenameDatabase = new System.Windows.Forms.Button();
            this.chkReadXVariables = new System.Windows.Forms.CheckBox();
            this.txtListStringOutVar = new System.Windows.Forms.TextBox();
            this.labIndirectTaxTableYear = new System.Windows.Forms.Label();
            this.txtIndirectTaxTableYear = new System.Windows.Forms.TextBox();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvSystemDataCombinations = new System.Windows.Forms.DataGridView();
            this.btnPath = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabGeneral = new System.Windows.Forms.TabPage();
            this.tabHHOT = new System.Windows.Forms.TabPage();
            this.dgvHHOT = new System.Windows.Forms.DataGridView();
            this.ctmMultiSelect = new System.Windows.Forms.ContextMenuStrip();
            this.mniAllSystemsX = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllSystemsNa = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniAllDatasetsX = new System.Windows.Forms.ToolStripMenuItem();
            this.mniAllDatasetsNa = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mniCopyFrom = new System.Windows.Forms.ToolStripMenuItem();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystemDataCombinations)).BeginInit();
            this.tabControl.SuspendLayout();
            this.tabGeneral.SuspendLayout();
            this.tabHHOT.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvHHOT)).BeginInit();
            this.ctmMultiSelect.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelCountry
            // 
            this.labelCountry.AutoSize = true;
            this.labelCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelCountry.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.labelCountry.Location = new System.Drawing.Point(12, 9);
            this.labelCountry.Name = "labelCountry";
            this.labelCountry.Size = new System.Drawing.Size(71, 20);
            this.labelCountry.TabIndex = 0;
            this.labelCountry.Text = "Country";
            // 
            // txtYearCollection
            // 
            this.txtYearCollection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtYearCollection.Location = new System.Drawing.Point(12, 471);
            this.txtYearCollection.Name = "txtYearCollection";
            this.txtYearCollection.Size = new System.Drawing.Size(75, 20);
            this.txtYearCollection.TabIndex = 4;
            this.tipConfigureDatabases.SetToolTip(this.txtYearCollection, "Year of data collection");
            this.txtYearCollection.TextChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 455);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Collection Year";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(90, 455);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Income Year";
            // 
            // txtYearIncome
            // 
            this.txtYearIncome.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.txtYearIncome.Location = new System.Drawing.Point(93, 471);
            this.txtYearIncome.Name = "txtYearIncome";
            this.txtYearIncome.Size = new System.Drawing.Size(75, 20);
            this.txtYearIncome.TabIndex = 5;
            this.tipConfigureDatabases.SetToolTip(this.txtYearIncome, "Year of income data");
            this.txtYearIncome.TextChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // cboCurrency
            // 
            this.cboCurrency.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboCurrency.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboCurrency.FormattingEnabled = true;
            this.cboCurrency.Location = new System.Drawing.Point(174, 471);
            this.cboCurrency.Name = "cboCurrency";
            this.cboCurrency.Size = new System.Drawing.Size(75, 21);
            this.cboCurrency.TabIndex = 6;
            this.tipConfigureDatabases.SetToolTip(this.cboCurrency, "Currency of monetary values");
            this.cboCurrency.SelectedIndexChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(171, 455);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Currency";
            // 
            // chkPrivate
            // 
            this.chkPrivate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkPrivate.AutoSize = true;
            this.chkPrivate.Location = new System.Drawing.Point(336, 455);
            this.chkPrivate.Name = "chkPrivate";
            this.chkPrivate.Size = new System.Drawing.Size(59, 17);
            this.chkPrivate.TabIndex = 10;
            this.chkPrivate.Text = "Private";
            this.tipConfigureDatabases.SetToolTip(this.chkPrivate, "Database is removed in public versions");
            this.chkPrivate.UseVisualStyleBackColor = true;
            this.chkPrivate.CheckedChanged += new System.EventHandler(this.PrivateAttributeChanged);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(252, 455);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 11;
            this.label5.Text = "Decimal Sign";
            // 
            // cboDecimalSign
            // 
            this.cboDecimalSign.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cboDecimalSign.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDecimalSign.Font = new System.Drawing.Font("Garamond", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cboDecimalSign.FormattingEnabled = true;
            this.cboDecimalSign.Location = new System.Drawing.Point(255, 471);
            this.cboDecimalSign.Name = "cboDecimalSign";
            this.cboDecimalSign.Size = new System.Drawing.Size(66, 22);
            this.cboDecimalSign.TabIndex = 7;
            this.tipConfigureDatabases.SetToolTip(this.cboDecimalSign, "Decimal separator used for numeric values");
            this.cboDecimalSign.SelectedIndexChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 508);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Path";
            // 
            // txtPath
            // 
            this.txtPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPath.Location = new System.Drawing.Point(48, 505);
            this.txtPath.Name = "txtPath";
            this.txtPath.Size = new System.Drawing.Size(753, 20);
            this.txtPath.TabIndex = 8;
            this.tipConfigureDatabases.SetToolTip(this.txtPath, "Path to database file if not default path");
            this.txtPath.TextChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // btnAddDatabase
            // 
            this.btnAddDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddDatabase.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAddDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddDatabase.Location = new System.Drawing.Point(16, 539);
            this.btnAddDatabase.Name = "btnAddDatabase";
            this.btnAddDatabase.Size = new System.Drawing.Size(60, 34);
            this.btnAddDatabase.TabIndex = 2;
            this.btnAddDatabase.Text = "Add Dataset";
            this.tipConfigureDatabases.SetToolTip(this.btnAddDatabase, "Add Database");
            this.btnAddDatabase.UseVisualStyleBackColor = false;
            this.btnAddDatabase.Click += new System.EventHandler(this.btnAddDataBase_Click);
            // 
            // btnDeleteDatabase
            // 
            this.btnDeleteDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDeleteDatabase.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDeleteDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDeleteDatabase.Location = new System.Drawing.Point(82, 539);
            this.btnDeleteDatabase.Name = "btnDeleteDatabase";
            this.btnDeleteDatabase.Size = new System.Drawing.Size(60, 34);
            this.btnDeleteDatabase.TabIndex = 3;
            this.btnDeleteDatabase.Text = "Delete Dataset";
            this.tipConfigureDatabases.SetToolTip(this.btnDeleteDatabase, "Delete Database");
            this.btnDeleteDatabase.UseVisualStyleBackColor = false;
            this.btnDeleteDatabase.Click += new System.EventHandler(this.btnDeleteDataBase_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(755, 549);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 20;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(659, 549);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 19;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // chkUseCommonDefault
            // 
            this.chkUseCommonDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkUseCommonDefault.AutoSize = true;
            this.chkUseCommonDefault.Location = new System.Drawing.Point(336, 476);
            this.chkUseCommonDefault.Name = "chkUseCommonDefault";
            this.chkUseCommonDefault.Size = new System.Drawing.Size(126, 17);
            this.chkUseCommonDefault.TabIndex = 16;
            this.chkUseCommonDefault.Text = "Use Common Default";
            this.tipConfigureDatabases.SetToolTip(this.chkUseCommonDefault, "Defaults as defined in variable description are used for variables not existent i" +
        "n data");
            this.chkUseCommonDefault.UseVisualStyleBackColor = true;
            this.chkUseCommonDefault.CheckedChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // tipConfigureDatabases
            // 
            this.tipConfigureDatabases.AutoPopDelay = 10000;
            this.tipConfigureDatabases.InitialDelay = 200;
            this.tipConfigureDatabases.ReshowDelay = 100;
            // 
            // btnRenameDatabase
            // 
            this.btnRenameDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRenameDatabase.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnRenameDatabase.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRenameDatabase.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRenameDatabase.Location = new System.Drawing.Point(148, 539);
            this.btnRenameDatabase.Name = "btnRenameDatabase";
            this.btnRenameDatabase.Size = new System.Drawing.Size(60, 34);
            this.btnRenameDatabase.TabIndex = 32;
            this.btnRenameDatabase.Text = "Rename Dataset";
            this.tipConfigureDatabases.SetToolTip(this.btnRenameDatabase, "Delete Database");
            this.btnRenameDatabase.UseVisualStyleBackColor = false;
            this.btnRenameDatabase.Click += new System.EventHandler(this.btnRenameDatabase_Click);
            // 
            // chkReadXVariables
            // 
            this.chkReadXVariables.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkReadXVariables.AutoSize = true;
            this.chkReadXVariables.Location = new System.Drawing.Point(468, 476);
            this.chkReadXVariables.Name = "chkReadXVariables";
            this.chkReadXVariables.Size = new System.Drawing.Size(192, 17);
            this.chkReadXVariables.TabIndex = 34;
            this.chkReadXVariables.Text = "Read Expenditure-related Variables";
            this.tipConfigureDatabases.SetToolTip(this.chkReadXVariables, "Automatically read all expenditure-related variables from the data");
            this.chkReadXVariables.UseVisualStyleBackColor = true;
            this.chkReadXVariables.CheckedChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // txtListStringOutVar
            // 
            this.txtListStringOutVar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProvider.SetHelpString(this.txtListStringOutVar, "");
            this.txtListStringOutVar.Location = new System.Drawing.Point(581, 453);
            this.txtListStringOutVar.Name = "txtListStringOutVar";
            this.helpProvider.SetShowHelp(this.txtListStringOutVar, true);
            this.txtListStringOutVar.Size = new System.Drawing.Size(264, 20);
            this.txtListStringOutVar.TabIndex = 35;
            this.tipConfigureDatabases.SetToolTip(this.txtListStringOutVar, "Variables, separated by space, which exist in data and are transferred to output");
            this.txtListStringOutVar.TextChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // labIndirectTaxTableYear
            // 
            this.labIndirectTaxTableYear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labIndirectTaxTableYear.AutoSize = true;
            this.labIndirectTaxTableYear.Location = new System.Drawing.Point(668, 477);
            this.labIndirectTaxTableYear.Name = "labIndirectTaxTableYear";
            this.labIndirectTaxTableYear.Size = new System.Drawing.Size(98, 13);
            this.labIndirectTaxTableYear.TabIndex = 38;
            this.labIndirectTaxTableYear.Text = "Ind.Tax Table Year";
            this.tipConfigureDatabases.SetToolTip(this.labIndirectTaxTableYear, "Year in Indirect Taxes Table to be used with the dataset");
            // 
            // txtIndirectTaxTableYear
            // 
            this.txtIndirectTaxTableYear.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.helpProvider.SetHelpString(this.txtIndirectTaxTableYear, "");
            this.txtIndirectTaxTableYear.Location = new System.Drawing.Point(769, 474);
            this.txtIndirectTaxTableYear.Name = "txtIndirectTaxTableYear";
            this.helpProvider.SetShowHelp(this.txtIndirectTaxTableYear, true);
            this.txtIndirectTaxTableYear.Size = new System.Drawing.Size(76, 20);
            this.txtIndirectTaxTableYear.TabIndex = 37;
            this.tipConfigureDatabases.SetToolTip(this.txtIndirectTaxTableYear, "Year in Indirect Taxes Table to be used with the dataset");
            this.txtIndirectTaxTableYear.TextChanged += new System.EventHandler(this.DataAttributeChanged);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 39);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 13);
            this.label1.TabIndex = 30;
            this.label1.Text = "Datasets / Systems";
            // 
            // dgvSystemDataCombinations
            // 
            this.dgvSystemDataCombinations.AllowUserToAddRows = false;
            this.dgvSystemDataCombinations.AllowUserToDeleteRows = false;
            this.dgvSystemDataCombinations.AllowUserToResizeRows = false;
            this.dgvSystemDataCombinations.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvSystemDataCombinations.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSystemDataCombinations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvSystemDataCombinations.Location = new System.Drawing.Point(2, 2);
            this.dgvSystemDataCombinations.MultiSelect = false;
            this.dgvSystemDataCombinations.Name = "dgvSystemDataCombinations";
            this.dgvSystemDataCombinations.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvSystemDataCombinations.ShowCellErrors = false;
            this.dgvSystemDataCombinations.ShowCellToolTips = false;
            this.dgvSystemDataCombinations.ShowEditingIcon = false;
            this.dgvSystemDataCombinations.ShowRowErrors = false;
            this.dgvSystemDataCombinations.Size = new System.Drawing.Size(824, 359);
            this.dgvSystemDataCombinations.TabIndex = 31;
            this.dgvSystemDataCombinations.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellValueChanged);
            this.dgvSystemDataCombinations.SelectionChanged += new System.EventHandler(this.dgv_SelectionChanged);
            // 
            // btnPath
            // 
            this.btnPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPath.Image = global::EM_UI.Properties.Resources.open_project_32;
            this.btnPath.Location = new System.Drawing.Point(805, 501);
            this.btnPath.Name = "btnPath";
            this.btnPath.Size = new System.Drawing.Size(40, 40);
            this.btnPath.TabIndex = 9;
            this.btnPath.UseVisualStyleBackColor = true;
            this.btnPath.Click += new System.EventHandler(this.btnPath_Click);
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabGeneral);
            this.tabControl.Controls.Add(this.tabHHOT);
            this.tabControl.Location = new System.Drawing.Point(9, 55);
            this.tabControl.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(836, 389);
            this.tabControl.TabIndex = 33;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.dgvSystemDataCombinations);
            this.tabGeneral.Location = new System.Drawing.Point(4, 22);
            this.tabGeneral.Margin = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Padding = new System.Windows.Forms.Padding(2);
            this.tabGeneral.Size = new System.Drawing.Size(828, 363);
            this.tabGeneral.TabIndex = 0;
            this.tabGeneral.Text = "General";
            this.tabGeneral.UseVisualStyleBackColor = true;
            // 
            // tabHHOT
            // 
            this.tabHHOT.Controls.Add(this.dgvHHOT);
            this.tabHHOT.Location = new System.Drawing.Point(4, 22);
            this.tabHHOT.Margin = new System.Windows.Forms.Padding(2);
            this.tabHHOT.Name = "tabHHOT";
            this.tabHHOT.Padding = new System.Windows.Forms.Padding(2);
            this.tabHHOT.Size = new System.Drawing.Size(828, 363);
            this.tabHHOT.TabIndex = 1;
            this.tabHHOT.Text = "HHOT";
            this.tabHHOT.UseVisualStyleBackColor = true;
            // 
            // dgvHHOT
            // 
            this.dgvHHOT.AllowUserToAddRows = false;
            this.dgvHHOT.AllowUserToDeleteRows = false;
            this.dgvHHOT.AllowUserToResizeRows = false;
            this.dgvHHOT.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvHHOT.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvHHOT.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvHHOT.Location = new System.Drawing.Point(2, 2);
            this.dgvHHOT.MultiSelect = false;
            this.dgvHHOT.Name = "dgvHHOT";
            this.dgvHHOT.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvHHOT.ShowCellErrors = false;
            this.dgvHHOT.ShowCellToolTips = false;
            this.dgvHHOT.ShowEditingIcon = false;
            this.dgvHHOT.ShowRowErrors = false;
            this.dgvHHOT.Size = new System.Drawing.Size(824, 359);
            this.dgvHHOT.TabIndex = 32;
            this.dgvHHOT.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgv_CellValueChanged);
            this.dgvHHOT.SelectionChanged += new System.EventHandler(this.dgv_SelectionChanged);
            // 
            // ctmMultiSelect
            // 
            this.ctmMultiSelect.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniAllSystemsX,
            this.mniAllSystemsNa,
            this.toolStripSeparator1,
            this.mniAllDatasetsX,
            this.mniAllDatasetsNa,
            this.toolStripSeparator2,
            this.mniCopyFrom});
            this.ctmMultiSelect.Name = "ctmMultiSelect";
            this.ctmMultiSelect.Size = new System.Drawing.Size(191, 126);
            this.ctmMultiSelect.Opening += new System.ComponentModel.CancelEventHandler(this.ctmMultiSelect_Opening);
            // 
            // mniAllSystemsX
            // 
            this.mniAllSystemsX.Name = "mniAllSystemsX";
            this.mniAllSystemsX.Size = new System.Drawing.Size(190, 22);
            this.mniAllSystemsX.Text = "Set x for all systems";
            this.mniAllSystemsX.Click += new System.EventHandler(this.mniAllSystemsX_Click);
            // 
            // mniAllSystemsNa
            // 
            this.mniAllSystemsNa.Name = "mniAllSystemsNa";
            this.mniAllSystemsNa.Size = new System.Drawing.Size(190, 22);
            this.mniAllSystemsNa.Text = "Set n/a for all systems";
            this.mniAllSystemsNa.Click += new System.EventHandler(this.mniAllSystemsNa_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(187, 6);
            // 
            // mniAllDatasetsX
            // 
            this.mniAllDatasetsX.Name = "mniAllDatasetsX";
            this.mniAllDatasetsX.Size = new System.Drawing.Size(190, 22);
            this.mniAllDatasetsX.Text = "Set x for all datasets";
            this.mniAllDatasetsX.Click += new System.EventHandler(this.mniAllDatasetsX_Click);
            // 
            // mniAllDatasetsNa
            // 
            this.mniAllDatasetsNa.Name = "mniAllDatasetsNa";
            this.mniAllDatasetsNa.Size = new System.Drawing.Size(190, 22);
            this.mniAllDatasetsNa.Text = "Set n/a for all datasets";
            this.mniAllDatasetsNa.Click += new System.EventHandler(this.mniAllDatasetsNa_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(187, 6);
            // 
            // mniCopyFrom
            // 
            this.mniCopyFrom.Name = "mniCopyFrom";
            this.mniCopyFrom.Size = new System.Drawing.Size(190, 22);
            this.mniCopyFrom.Text = "Copy from ...";
            this.mniCopyFrom.DropDownOpening += new System.EventHandler(this.mniCopyFrom_DropDownOpening);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(465, 456);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(115, 13);
            this.label7.TabIndex = 36;
            this.label7.Text = "String Output Variables";
            // 
            // ConfigureDataForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(864, 583);
            this.ContextMenuStrip = this.ctmMultiSelect;
            this.Controls.Add(this.labIndirectTaxTableYear);
            this.Controls.Add(this.txtIndirectTaxTableYear);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtListStringOutVar);
            this.Controls.Add(this.chkReadXVariables);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.btnRenameDatabase);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.chkUseCommonDefault);
            this.Controls.Add(this.txtYearCollection);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtYearIncome);
            this.Controls.Add(this.cboDecimalSign);
            this.Controls.Add(this.btnDeleteDatabase);
            this.Controls.Add(this.chkPrivate);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnAddDatabase);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cboCurrency);
            this.Controls.Add(this.labelCountry);
            this.Controls.Add(this.btnPath);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ConfiguringDatasets.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureDataForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Configure Databases";
            this.tipConfigureDatabases.SetToolTip(this, "Variables, separated by space, which exist in data and are transferred to output");
            this.Load += new System.EventHandler(this.ConfigureDataForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystemDataCombinations)).EndInit();
            this.tabControl.ResumeLayout(false);
            this.tabGeneral.ResumeLayout(false);
            this.tabHHOT.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvHHOT)).EndInit();
            this.ctmMultiSelect.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelCountry;
        private System.Windows.Forms.TextBox txtYearCollection;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtYearIncome;
        private System.Windows.Forms.ComboBox cboCurrency;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkPrivate;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox cboDecimalSign;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtPath;
        private System.Windows.Forms.Button btnAddDatabase;
        private System.Windows.Forms.Button btnDeleteDatabase;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ToolTip tipConfigureDatabases;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.CheckBox chkUseCommonDefault;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvSystemDataCombinations;
        private System.Windows.Forms.Button btnPath;
        private System.Windows.Forms.Button btnRenameDatabase;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabGeneral;
        private System.Windows.Forms.TabPage tabHHOT;
        private System.Windows.Forms.DataGridView dgvHHOT;
        private System.Windows.Forms.CheckBox chkReadXVariables;
        private System.Windows.Forms.ContextMenuStrip ctmMultiSelect;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsX;
        private System.Windows.Forms.ToolStripMenuItem mniAllSystemsNa;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mniAllDatasetsX;
        private System.Windows.Forms.ToolStripMenuItem mniAllDatasetsNa;
        private System.Windows.Forms.ToolStripMenuItem mniCopyFrom;
        private System.Windows.Forms.TextBox txtListStringOutVar;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label labIndirectTaxTableYear;
        private System.Windows.Forms.TextBox txtIndirectTaxTableYear;
    }
}