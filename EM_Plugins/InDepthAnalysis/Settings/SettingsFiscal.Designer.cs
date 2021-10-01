namespace InDepthAnalysis
{
    partial class SettingsFiscal
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.labelTitle1 = new System.Windows.Forms.Label();
            this.txtPageTitleFiscal = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.group12 = new System.Windows.Forms.GroupBox();
            this.txtTableTitleFiscalTaxpayersBeneficiaries = new System.Windows.Forms.TextBox();
            this.labelTitle12 = new System.Windows.Forms.Label();
            this.txtBeneficiaries = new System.Windows.Forms.TextBox();
            this.labBeneficiaries = new System.Windows.Forms.Label();
            this.txtTaxpayers = new System.Windows.Forms.TextBox();
            this.labTaxpayers = new System.Windows.Forms.Label();
            this.comboCalculationLevel = new System.Windows.Forms.ComboBox();
            this.labCalculationLevel = new System.Windows.Forms.Label();
            this.txtTableTitleRevenueExpenditure = new System.Windows.Forms.TextBox();
            this.labelTitle11 = new System.Windows.Forms.Label();
            this.group11 = new System.Windows.Forms.GroupBox();
            this.group134 = new System.Windows.Forms.GroupBox();
            this.group134_panel = new System.Windows.Forms.Panel();
            this.txtTableTitleDisaggregatedConceptsUnits = new System.Windows.Forms.TextBox();
            this.labelTitle14 = new System.Windows.Forms.Label();
            this.btnDisConMoveDown = new System.Windows.Forms.Button();
            this.btnDisConMoveUp = new System.Windows.Forms.Button();
            this.btnDisConDelRow = new System.Windows.Forms.Button();
            this.btnDisConAddRow = new System.Windows.Forms.Button();
            this.gridDisCon = new System.Windows.Forms.DataGridView();
            this.colDisConConcept = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisConFormula = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisConFormulaFilter = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisConTaxpayersBeneficiaries = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDisConLevel = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colDisConBold = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colDisConBorder = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.txtTableTitleDisaggregatedConcepts = new System.Windows.Forms.TextBox();
            this.labelTitle13 = new System.Windows.Forms.Label();
            this.btnReset = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.mainPanel = new System.Windows.Forms.Panel();
            this.bottomPanel = new System.Windows.Forms.Panel();
            this.group12.SuspendLayout();
            this.group11.SuspendLayout();
            this.group134.SuspendLayout();
            this.group134_panel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDisCon)).BeginInit();
            this.mainPanel.SuspendLayout();
            this.bottomPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // labelTitle1
            // 
            this.labelTitle1.AutoSize = true;
            this.labelTitle1.Location = new System.Drawing.Point(12, 16);
            this.labelTitle1.Name = "labelTitle1";
            this.labelTitle1.Size = new System.Drawing.Size(39, 13);
            this.labelTitle1.TabIndex = 0;
            this.labelTitle1.Text = "Title 1.";
            // 
            // txtPageTitleFiscal
            // 
            this.txtPageTitleFiscal.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPageTitleFiscal.Location = new System.Drawing.Point(63, 13);
            this.txtPageTitleFiscal.Name = "txtPageTitleFiscal";
            this.txtPageTitleFiscal.Size = new System.Drawing.Size(806, 20);
            this.txtPageTitleFiscal.TabIndex = 1;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOK.Location = new System.Drawing.Point(340, 4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(422, 4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // group12
            // 
            this.group12.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.group12.BackColor = System.Drawing.SystemColors.ControlLight;
            this.group12.Controls.Add(this.txtTableTitleFiscalTaxpayersBeneficiaries);
            this.group12.Controls.Add(this.labelTitle12);
            this.group12.Controls.Add(this.txtBeneficiaries);
            this.group12.Controls.Add(this.labBeneficiaries);
            this.group12.Controls.Add(this.txtTaxpayers);
            this.group12.Controls.Add(this.labTaxpayers);
            this.group12.Controls.Add(this.comboCalculationLevel);
            this.group12.Controls.Add(this.labCalculationLevel);
            this.group12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.group12.Location = new System.Drawing.Point(10, 111);
            this.group12.Name = "group12";
            this.group12.Size = new System.Drawing.Size(865, 70);
            this.group12.TabIndex = 75;
            this.group12.TabStop = false;
            this.group12.Text = "Table 1.2.";
            // 
            // txtTableTitleFiscalTaxpayersBeneficiaries
            // 
            this.txtTableTitleFiscalTaxpayersBeneficiaries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTableTitleFiscalTaxpayersBeneficiaries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTableTitleFiscalTaxpayersBeneficiaries.Location = new System.Drawing.Point(53, 14);
            this.txtTableTitleFiscalTaxpayersBeneficiaries.Name = "txtTableTitleFiscalTaxpayersBeneficiaries";
            this.txtTableTitleFiscalTaxpayersBeneficiaries.Size = new System.Drawing.Size(806, 20);
            this.txtTableTitleFiscalTaxpayersBeneficiaries.TabIndex = 84;
            // 
            // labelTitle12
            // 
            this.labelTitle12.AutoSize = true;
            this.labelTitle12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle12.Location = new System.Drawing.Point(3, 17);
            this.labelTitle12.Name = "labelTitle12";
            this.labelTitle12.Size = new System.Drawing.Size(48, 13);
            this.labelTitle12.TabIndex = 83;
            this.labelTitle12.Text = "Title 1.2.";
            // 
            // txtBeneficiaries
            // 
            this.txtBeneficiaries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtBeneficiaries.Location = new System.Drawing.Point(379, 40);
            this.txtBeneficiaries.Name = "txtBeneficiaries";
            this.txtBeneficiaries.Size = new System.Drawing.Size(274, 20);
            this.txtBeneficiaries.TabIndex = 82;
            this.txtBeneficiaries.Validating += new System.ComponentModel.CancelEventHandler(this.txtTaxpayersBeneficiaries_Validating);
            // 
            // labBeneficiaries
            // 
            this.labBeneficiaries.AutoSize = true;
            this.labBeneficiaries.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labBeneficiaries.Location = new System.Drawing.Point(250, 43);
            this.labBeneficiaries.Name = "labBeneficiaries";
            this.labBeneficiaries.Size = new System.Drawing.Size(129, 13);
            this.labBeneficiaries.TabIndex = 81;
            this.labBeneficiaries.Text = "Benefit/Pension receivers";
            // 
            // txtTaxpayers
            // 
            this.txtTaxpayers.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTaxpayers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTaxpayers.Location = new System.Drawing.Point(778, 40);
            this.txtTaxpayers.Name = "txtTaxpayers";
            this.txtTaxpayers.Size = new System.Drawing.Size(81, 20);
            this.txtTaxpayers.TabIndex = 80;
            this.txtTaxpayers.Validating += new System.ComponentModel.CancelEventHandler(this.txtTaxpayersBeneficiaries_Validating);
            // 
            // labTaxpayers
            // 
            this.labTaxpayers.AutoSize = true;
            this.labTaxpayers.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labTaxpayers.Location = new System.Drawing.Point(664, 43);
            this.labTaxpayers.Name = "labTaxpayers";
            this.labTaxpayers.Size = new System.Drawing.Size(115, 13);
            this.labTaxpayers.TabIndex = 79;
            this.labTaxpayers.Text = "Other payers/receivers";
            // 
            // comboCalculationLevel
            // 
            this.comboCalculationLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboCalculationLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboCalculationLevel.FormattingEnabled = true;
            this.comboCalculationLevel.Items.AddRange(new object[] {
            "Household",
            "Individual",
            "Individuals in household"});
            this.comboCalculationLevel.Location = new System.Drawing.Point(88, 40);
            this.comboCalculationLevel.Name = "comboCalculationLevel";
            this.comboCalculationLevel.Size = new System.Drawing.Size(156, 21);
            this.comboCalculationLevel.TabIndex = 78;
            // 
            // labCalculationLevel
            // 
            this.labCalculationLevel.AutoSize = true;
            this.labCalculationLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labCalculationLevel.Location = new System.Drawing.Point(3, 43);
            this.labCalculationLevel.Name = "labCalculationLevel";
            this.labCalculationLevel.Size = new System.Drawing.Size(85, 13);
            this.labCalculationLevel.TabIndex = 77;
            this.labCalculationLevel.Text = "Level of analysis";
            // 
            // txtTableTitleRevenueExpenditure
            // 
            this.txtTableTitleRevenueExpenditure.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTableTitleRevenueExpenditure.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTableTitleRevenueExpenditure.Location = new System.Drawing.Point(53, 19);
            this.txtTableTitleRevenueExpenditure.Name = "txtTableTitleRevenueExpenditure";
            this.txtTableTitleRevenueExpenditure.Size = new System.Drawing.Size(806, 20);
            this.txtTableTitleRevenueExpenditure.TabIndex = 76;
            // 
            // labelTitle11
            // 
            this.labelTitle11.AutoSize = true;
            this.labelTitle11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle11.Location = new System.Drawing.Point(3, 22);
            this.labelTitle11.Name = "labelTitle11";
            this.labelTitle11.Size = new System.Drawing.Size(48, 13);
            this.labelTitle11.TabIndex = 75;
            this.labelTitle11.Text = "Title 1.1.";
            // 
            // group11
            // 
            this.group11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.group11.BackColor = System.Drawing.SystemColors.ControlLight;
            this.group11.Controls.Add(this.txtTableTitleRevenueExpenditure);
            this.group11.Controls.Add(this.labelTitle11);
            this.group11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.group11.Location = new System.Drawing.Point(10, 46);
            this.group11.Name = "group11";
            this.group11.Size = new System.Drawing.Size(865, 47);
            this.group11.TabIndex = 77;
            this.group11.TabStop = false;
            this.group11.Text = "Tables 1.1.";
            // 
            // group134
            // 
            this.group134.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.group134.BackColor = System.Drawing.SystemColors.ControlLight;
            this.group134.Controls.Add(this.group134_panel);
            this.group134.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.group134.Location = new System.Drawing.Point(10, 194);
            this.group134.MinimumSize = new System.Drawing.Size(500, 150);
            this.group134.Name = "group134";
            this.group134.Size = new System.Drawing.Size(865, 260);
            this.group134.TabIndex = 78;
            this.group134.TabStop = false;
            this.group134.Text = "Tables 1.3. && 1.4.";
            // 
            // group134_panel
            // 
            this.group134_panel.Controls.Add(this.txtTableTitleDisaggregatedConceptsUnits);
            this.group134_panel.Controls.Add(this.labelTitle14);
            this.group134_panel.Controls.Add(this.btnDisConMoveDown);
            this.group134_panel.Controls.Add(this.btnDisConMoveUp);
            this.group134_panel.Controls.Add(this.btnDisConDelRow);
            this.group134_panel.Controls.Add(this.btnDisConAddRow);
            this.group134_panel.Controls.Add(this.gridDisCon);
            this.group134_panel.Controls.Add(this.txtTableTitleDisaggregatedConcepts);
            this.group134_panel.Controls.Add(this.labelTitle13);
            this.group134_panel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.group134_panel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.group134_panel.Location = new System.Drawing.Point(3, 16);
            this.group134_panel.Name = "group134_panel";
            this.group134_panel.Size = new System.Drawing.Size(859, 241);
            this.group134_panel.TabIndex = 82;
            // 
            // txtTableTitleDisaggregatedConceptsUnits
            // 
            this.txtTableTitleDisaggregatedConceptsUnits.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTableTitleDisaggregatedConceptsUnits.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTableTitleDisaggregatedConceptsUnits.Location = new System.Drawing.Point(577, 3);
            this.txtTableTitleDisaggregatedConceptsUnits.Name = "txtTableTitleDisaggregatedConceptsUnits";
            this.txtTableTitleDisaggregatedConceptsUnits.Size = new System.Drawing.Size(279, 20);
            this.txtTableTitleDisaggregatedConceptsUnits.TabIndex = 81;
            // 
            // labelTitle14
            // 
            this.labelTitle14.AutoSize = true;
            this.labelTitle14.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle14.Location = new System.Drawing.Point(527, 6);
            this.labelTitle14.Name = "labelTitle14";
            this.labelTitle14.Size = new System.Drawing.Size(48, 13);
            this.labelTitle14.TabIndex = 80;
            this.labelTitle14.Text = "Title 1.4.";
            // 
            // btnDisConMoveDown
            // 
            this.btnDisConMoveDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisConMoveDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisConMoveDown.Location = new System.Drawing.Point(785, 103);
            this.btnDisConMoveDown.Name = "btnDisConMoveDown";
            this.btnDisConMoveDown.Size = new System.Drawing.Size(71, 23);
            this.btnDisConMoveDown.TabIndex = 79;
            this.btnDisConMoveDown.Text = "Move down";
            this.btnDisConMoveDown.UseVisualStyleBackColor = true;
            this.btnDisConMoveDown.Click += new System.EventHandler(this.btnDisConMoveDown_Click);
            // 
            // btnDisConMoveUp
            // 
            this.btnDisConMoveUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisConMoveUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisConMoveUp.Location = new System.Drawing.Point(785, 78);
            this.btnDisConMoveUp.Name = "btnDisConMoveUp";
            this.btnDisConMoveUp.Size = new System.Drawing.Size(71, 23);
            this.btnDisConMoveUp.TabIndex = 78;
            this.btnDisConMoveUp.Text = "Move up";
            this.btnDisConMoveUp.UseVisualStyleBackColor = true;
            this.btnDisConMoveUp.Click += new System.EventHandler(this.btnDisConMoveUp_Click);
            // 
            // btnDisConDelRow
            // 
            this.btnDisConDelRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisConDelRow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisConDelRow.Location = new System.Drawing.Point(785, 53);
            this.btnDisConDelRow.Name = "btnDisConDelRow";
            this.btnDisConDelRow.Size = new System.Drawing.Size(71, 23);
            this.btnDisConDelRow.TabIndex = 77;
            this.btnDisConDelRow.Text = "Delete row";
            this.btnDisConDelRow.UseVisualStyleBackColor = true;
            this.btnDisConDelRow.Click += new System.EventHandler(this.btnDisConDelRow_Click);
            // 
            // btnDisConAddRow
            // 
            this.btnDisConAddRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDisConAddRow.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDisConAddRow.Location = new System.Drawing.Point(785, 28);
            this.btnDisConAddRow.Name = "btnDisConAddRow";
            this.btnDisConAddRow.Size = new System.Drawing.Size(71, 23);
            this.btnDisConAddRow.TabIndex = 76;
            this.btnDisConAddRow.Text = "Add row";
            this.btnDisConAddRow.UseVisualStyleBackColor = true;
            this.btnDisConAddRow.Click += new System.EventHandler(this.btnDisConAddRow_Click);
            // 
            // gridDisCon
            // 
            this.gridDisCon.AllowUserToAddRows = false;
            this.gridDisCon.AllowUserToDeleteRows = false;
            this.gridDisCon.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridDisCon.BackgroundColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridDisCon.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridDisCon.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.gridDisCon.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colDisConConcept,
            this.colDisConFormula,
            this.colDisConFormulaFilter,
            this.colDisConTaxpayersBeneficiaries,
            this.colDisConLevel,
            this.colDisConBold,
            this.colDisConBorder});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.LightGray;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridDisCon.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridDisCon.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.gridDisCon.Location = new System.Drawing.Point(4, 29);
            this.gridDisCon.MultiSelect = false;
            this.gridDisCon.Name = "gridDisCon";
            this.gridDisCon.RowHeadersVisible = false;
            this.gridDisCon.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridDisCon.Size = new System.Drawing.Size(776, 209);
            this.gridDisCon.TabIndex = 75;
            this.gridDisCon.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.gridDisCon_CellValidating);
            // 
            // colDisConConcept
            // 
            this.colDisConConcept.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDisConConcept.HeaderText = "Concept";
            this.colDisConConcept.Name = "colDisConConcept";
            this.colDisConConcept.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDisConConcept.ToolTipText = "Label for table";
            this.colDisConConcept.Width = 53;
            // 
            // colDisConFormula
            // 
            this.colDisConFormula.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDisConFormula.HeaderText = "Variable/Formula";
            this.colDisConFormula.Name = "colDisConFormula";
            this.colDisConFormula.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colDisConFormulaFilter
            // 
            this.colDisConFormulaFilter.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDisConFormulaFilter.HeaderText = "Formula filter";
            this.colDisConFormulaFilter.Name = "colDisConFormulaFilter";
            this.colDisConFormulaFilter.ToolTipText = "Allows \"safeguarding\" formula by e.g. excluding cases which cause a dividsion by " +
    "zero";
            this.colDisConFormulaFilter.Width = 91;
            // 
            // colDisConTaxpayersBeneficiaries
            // 
            this.colDisConTaxpayersBeneficiaries.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDisConTaxpayersBeneficiaries.HeaderText = "Payers/Receivers (1.4. only)";
            this.colDisConTaxpayersBeneficiaries.Name = "colDisConTaxpayersBeneficiaries";
            this.colDisConTaxpayersBeneficiaries.Width = 166;
            // 
            // colDisConLevel
            // 
            this.colDisConLevel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDisConLevel.HeaderText = "Level of analysis (1.4. only)";
            this.colDisConLevel.Items.AddRange(new object[] {
            "Household",
            "Individual",
            "Individuals in household"});
            this.colDisConLevel.Name = "colDisConLevel";
            this.colDisConLevel.Width = 140;
            // 
            // colDisConBold
            // 
            this.colDisConBold.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDisConBold.HeaderText = "Bold";
            this.colDisConBold.Name = "colDisConBold";
            this.colDisConBold.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colDisConBold.Width = 34;
            // 
            // colDisConBorder
            // 
            this.colDisConBorder.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colDisConBorder.HeaderText = "Bottom border";
            this.colDisConBorder.Name = "colDisConBorder";
            this.colDisConBorder.Width = 79;
            // 
            // txtTableTitleDisaggregatedConcepts
            // 
            this.txtTableTitleDisaggregatedConcepts.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtTableTitleDisaggregatedConcepts.Location = new System.Drawing.Point(50, 3);
            this.txtTableTitleDisaggregatedConcepts.Name = "txtTableTitleDisaggregatedConcepts";
            this.txtTableTitleDisaggregatedConcepts.Size = new System.Drawing.Size(475, 20);
            this.txtTableTitleDisaggregatedConcepts.TabIndex = 74;
            // 
            // labelTitle13
            // 
            this.labelTitle13.AutoSize = true;
            this.labelTitle13.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitle13.Location = new System.Drawing.Point(1, 6);
            this.labelTitle13.Name = "labelTitle13";
            this.labelTitle13.Size = new System.Drawing.Size(48, 13);
            this.labelTitle13.TabIndex = 73;
            this.labelTitle13.Text = "Title 1.3.";
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReset.Location = new System.Drawing.Point(797, 4);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(75, 23);
            this.btnReset.TabIndex = 79;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // mainPanel
            // 
            this.mainPanel.AutoScroll = true;
            this.mainPanel.Controls.Add(this.group11);
            this.mainPanel.Controls.Add(this.group12);
            this.mainPanel.Controls.Add(this.txtPageTitleFiscal);
            this.mainPanel.Controls.Add(this.labelTitle1);
            this.mainPanel.Controls.Add(this.group134);
            this.mainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainPanel.Location = new System.Drawing.Point(0, 0);
            this.mainPanel.Name = "mainPanel";
            this.mainPanel.Size = new System.Drawing.Size(885, 466);
            this.mainPanel.TabIndex = 80;
            // 
            // bottomPanel
            // 
            this.bottomPanel.Controls.Add(this.btnReset);
            this.bottomPanel.Controls.Add(this.btnCancel);
            this.bottomPanel.Controls.Add(this.btnOK);
            this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.bottomPanel.Location = new System.Drawing.Point(0, 466);
            this.bottomPanel.Name = "bottomPanel";
            this.bottomPanel.Size = new System.Drawing.Size(885, 38);
            this.bottomPanel.TabIndex = 81;
            // 
            // SettingsFiscal
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(885, 504);
            this.Controls.Add(this.mainPanel);
            this.Controls.Add(this.bottomPanel);
            this.helpProvider.SetHelpKeyword(this, "Settings_Fiscal.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.helpProvider.SetHelpString(this, "");
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsFiscal";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Settings Fiscal";
            this.group12.ResumeLayout(false);
            this.group12.PerformLayout();
            this.group11.ResumeLayout(false);
            this.group11.PerformLayout();
            this.group134.ResumeLayout(false);
            this.group134_panel.ResumeLayout(false);
            this.group134_panel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridDisCon)).EndInit();
            this.mainPanel.ResumeLayout(false);
            this.mainPanel.PerformLayout();
            this.bottomPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label labelTitle1;
        private System.Windows.Forms.TextBox txtPageTitleFiscal;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.GroupBox group12;
        private System.Windows.Forms.TextBox txtTableTitleFiscalTaxpayersBeneficiaries;
        private System.Windows.Forms.Label labelTitle12;
        private System.Windows.Forms.TextBox txtBeneficiaries;
        private System.Windows.Forms.Label labBeneficiaries;
        private System.Windows.Forms.TextBox txtTaxpayers;
        private System.Windows.Forms.Label labTaxpayers;
        private System.Windows.Forms.ComboBox comboCalculationLevel;
        private System.Windows.Forms.Label labCalculationLevel;
        private System.Windows.Forms.TextBox txtTableTitleRevenueExpenditure;
        private System.Windows.Forms.Label labelTitle11;
        private System.Windows.Forms.GroupBox group11;
        private System.Windows.Forms.GroupBox group134;
        private System.Windows.Forms.TextBox txtTableTitleDisaggregatedConceptsUnits;
        private System.Windows.Forms.Label labelTitle14;
        private System.Windows.Forms.Button btnDisConMoveDown;
        private System.Windows.Forms.Button btnDisConMoveUp;
        private System.Windows.Forms.Button btnDisConDelRow;
        private System.Windows.Forms.Button btnDisConAddRow;
        private System.Windows.Forms.DataGridView gridDisCon;
        private System.Windows.Forms.TextBox txtTableTitleDisaggregatedConcepts;
        private System.Windows.Forms.Label labelTitle13;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Panel group134_panel;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisConConcept;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisConFormula;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisConFormulaFilter;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDisConTaxpayersBeneficiaries;
        private System.Windows.Forms.DataGridViewComboBoxColumn colDisConLevel;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDisConBold;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colDisConBorder;
        private System.Windows.Forms.Panel mainPanel;
        private System.Windows.Forms.Panel bottomPanel;
    }
}