namespace HypotheticalHousehold
{
    partial class StatisticsWizardForm
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
            this.btnCancel = new System.Windows.Forms.Button();
            this.radioBudget = new System.Windows.Forms.RadioButton();
            this.radioTypes = new System.Windows.Forms.RadioButton();
            this.radioCountryYear = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBoxBudget = new System.Windows.Forms.GroupBox();
            this.txtFixedValue = new System.Windows.Forms.TextBox();
            this.txtRangeStep = new System.Windows.Forms.TextBox();
            this.txtRangeFrom = new System.Windows.Forms.TextBox();
            this.txtRangeTo = new System.Windows.Forms.TextBox();
            this.labFixedVal = new System.Windows.Forms.Label();
            this.radioRangeIncomeFixedWage = new System.Windows.Forms.RadioButton();
            this.radioRangeWageFixedHours = new System.Windows.Forms.RadioButton();
            this.radioRangeIncomeFixedHours = new System.Windows.Forms.RadioButton();
            this.radioRangeHoursFixedWage = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            this.comboPerson = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.labFrom = new System.Windows.Forms.Label();
            this.labProgress = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnNotVisibleAcceptButton = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.chkOutputInEuro = new System.Windows.Forms.CheckBox();
            this.labelControl1 = new DevExpress.XtraEditors.LabelControl();
            this.countriesCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.labelSelectedCountries = new DevExpress.XtraEditors.LabelControl();
            this.labelControl5 = new DevExpress.XtraEditors.LabelControl();
            this.labelSelectedYears = new DevExpress.XtraEditors.LabelControl();
            this.yearsCheckedComboBoxEdit = new DevExpress.XtraEditors.CheckedComboBoxEdit();
            this.listHHType_MultiSelect = new System.Windows.Forms.CheckedListBox();
            this.listHHType_SingleSelect = new System.Windows.Forms.CheckedListBox();
            this.groupBox1.SuspendLayout();
            this.groupBoxBudget.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countriesCheckedComboBoxEdit.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.yearsCheckedComboBoxEdit.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(368, 472);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(59, 25);
            this.btnCancel.TabIndex = 19;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // radioBudget
            // 
            this.radioBudget.AutoSize = true;
            this.radioBudget.Checked = true;
            this.radioBudget.Location = new System.Drawing.Point(7, 17);
            this.radioBudget.Margin = new System.Windows.Forms.Padding(2);
            this.radioBudget.Name = "radioBudget";
            this.radioBudget.Size = new System.Drawing.Size(114, 17);
            this.radioBudget.TabIndex = 3;
            this.radioBudget.TabStop = true;
            this.radioBudget.Text = "Budget Constraints";
            this.radioBudget.UseVisualStyleBackColor = true;
            this.radioBudget.CheckedChanged += new System.EventHandler(this.statisticRadioChanged);
            // 
            // radioTypes
            // 
            this.radioTypes.AutoSize = true;
            this.radioTypes.Location = new System.Drawing.Point(124, 17);
            this.radioTypes.Margin = new System.Windows.Forms.Padding(2);
            this.radioTypes.Name = "radioTypes";
            this.radioTypes.Size = new System.Drawing.Size(135, 17);
            this.radioTypes.TabIndex = 4;
            this.radioTypes.TabStop = true;
            this.radioTypes.Text = "Break Down HH Types";
            this.radioTypes.UseVisualStyleBackColor = true;
            this.radioTypes.CheckedChanged += new System.EventHandler(this.statisticRadioChanged);
            // 
            // radioCountryYear
            // 
            this.radioCountryYear.AutoSize = true;
            this.radioCountryYear.Location = new System.Drawing.Point(261, 17);
            this.radioCountryYear.Margin = new System.Windows.Forms.Padding(2);
            this.radioCountryYear.Name = "radioCountryYear";
            this.radioCountryYear.Size = new System.Drawing.Size(150, 17);
            this.radioCountryYear.TabIndex = 5;
            this.radioCountryYear.TabStop = true;
            this.radioCountryYear.Text = "Break Down Country/Year";
            this.radioCountryYear.UseVisualStyleBackColor = true;
            this.radioCountryYear.CheckedChanged += new System.EventHandler(this.statisticRadioChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radioBudget);
            this.groupBox1.Controls.Add(this.radioCountryYear);
            this.groupBox1.Controls.Add(this.radioTypes);
            this.groupBox1.Location = new System.Drawing.Point(11, 66);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(417, 42);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Statistic";
            // 
            // groupBoxBudget
            // 
            this.groupBoxBudget.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxBudget.Controls.Add(this.txtFixedValue);
            this.groupBoxBudget.Controls.Add(this.txtRangeStep);
            this.groupBoxBudget.Controls.Add(this.txtRangeFrom);
            this.groupBoxBudget.Controls.Add(this.txtRangeTo);
            this.groupBoxBudget.Controls.Add(this.labFixedVal);
            this.groupBoxBudget.Controls.Add(this.radioRangeIncomeFixedWage);
            this.groupBoxBudget.Controls.Add(this.radioRangeWageFixedHours);
            this.groupBoxBudget.Controls.Add(this.radioRangeIncomeFixedHours);
            this.groupBoxBudget.Controls.Add(this.radioRangeHoursFixedWage);
            this.groupBoxBudget.Controls.Add(this.label7);
            this.groupBoxBudget.Controls.Add(this.comboPerson);
            this.groupBoxBudget.Controls.Add(this.label5);
            this.groupBoxBudget.Controls.Add(this.label4);
            this.groupBoxBudget.Controls.Add(this.labFrom);
            this.groupBoxBudget.Location = new System.Drawing.Point(10, 271);
            this.groupBoxBudget.Margin = new System.Windows.Forms.Padding(2);
            this.groupBoxBudget.Name = "groupBoxBudget";
            this.groupBoxBudget.Padding = new System.Windows.Forms.Padding(2);
            this.groupBoxBudget.Size = new System.Drawing.Size(419, 171);
            this.groupBoxBudget.TabIndex = 7;
            this.groupBoxBudget.TabStop = false;
            this.groupBoxBudget.Text = "Specifications Budget Constraints";
            // 
            // txtFixedValue
            // 
            this.txtFixedValue.Location = new System.Drawing.Point(67, 138);
            this.txtFixedValue.Margin = new System.Windows.Forms.Padding(2);
            this.txtFixedValue.Name = "txtFixedValue";
            this.txtFixedValue.Size = new System.Drawing.Size(76, 20);
            this.txtFixedValue.TabIndex = 16;
            this.txtFixedValue.TextChanged += new System.EventHandler(this.txtFixedValue_TextChanged);
            // 
            // txtRangeStep
            // 
            this.txtRangeStep.Location = new System.Drawing.Point(266, 109);
            this.txtRangeStep.Margin = new System.Windows.Forms.Padding(2);
            this.txtRangeStep.Name = "txtRangeStep";
            this.txtRangeStep.Size = new System.Drawing.Size(76, 20);
            this.txtRangeStep.TabIndex = 15;
            this.txtRangeStep.TextChanged += new System.EventHandler(this.txtRangeStep_TextChanged);
            // 
            // txtRangeFrom
            // 
            this.txtRangeFrom.Location = new System.Drawing.Point(67, 109);
            this.txtRangeFrom.Margin = new System.Windows.Forms.Padding(2);
            this.txtRangeFrom.Name = "txtRangeFrom";
            this.txtRangeFrom.Size = new System.Drawing.Size(76, 20);
            this.txtRangeFrom.TabIndex = 13;
            this.txtRangeFrom.TextChanged += new System.EventHandler(this.txtRangeFrom_TextChanged);
            // 
            // txtRangeTo
            // 
            this.txtRangeTo.Location = new System.Drawing.Point(160, 109);
            this.txtRangeTo.Margin = new System.Windows.Forms.Padding(2);
            this.txtRangeTo.Name = "txtRangeTo";
            this.txtRangeTo.Size = new System.Drawing.Size(76, 20);
            this.txtRangeTo.TabIndex = 14;
            this.txtRangeTo.TextChanged += new System.EventHandler(this.txtRangeTo_TextChanged);
            // 
            // labFixedVal
            // 
            this.labFixedVal.AutoSize = true;
            this.labFixedVal.Location = new System.Drawing.Point(5, 141);
            this.labFixedVal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labFixedVal.Name = "labFixedVal";
            this.labFixedVal.Size = new System.Drawing.Size(36, 13);
            this.labFixedVal.TabIndex = 27;
            this.labFixedVal.Text = "Wage";
            // 
            // radioRangeIncomeFixedWage
            // 
            this.radioRangeIncomeFixedWage.AutoSize = true;
            this.radioRangeIncomeFixedWage.Location = new System.Drawing.Point(180, 55);
            this.radioRangeIncomeFixedWage.Name = "radioRangeIncomeFixedWage";
            this.radioRangeIncomeFixedWage.Size = new System.Drawing.Size(158, 17);
            this.radioRangeIncomeFixedWage.TabIndex = 10;
            this.radioRangeIncomeFixedWage.TabStop = true;
            this.radioRangeIncomeFixedWage.Text = "Range Income, Fixed Wage";
            this.radioRangeIncomeFixedWage.UseVisualStyleBackColor = true;
            this.radioRangeIncomeFixedWage.CheckedChanged += new System.EventHandler(this.budgetConstraintsRangeType_CheckedChanged);
            // 
            // radioRangeWageFixedHours
            // 
            this.radioRangeWageFixedHours.AutoSize = true;
            this.radioRangeWageFixedHours.Location = new System.Drawing.Point(8, 78);
            this.radioRangeWageFixedHours.Name = "radioRangeWageFixedHours";
            this.radioRangeWageFixedHours.Size = new System.Drawing.Size(151, 17);
            this.radioRangeWageFixedHours.TabIndex = 11;
            this.radioRangeWageFixedHours.TabStop = true;
            this.radioRangeWageFixedHours.Text = "Range Wage, Fixed Hours";
            this.radioRangeWageFixedHours.UseVisualStyleBackColor = true;
            this.radioRangeWageFixedHours.CheckedChanged += new System.EventHandler(this.budgetConstraintsRangeType_CheckedChanged);
            // 
            // radioRangeIncomeFixedHours
            // 
            this.radioRangeIncomeFixedHours.AutoSize = true;
            this.radioRangeIncomeFixedHours.Location = new System.Drawing.Point(180, 78);
            this.radioRangeIncomeFixedHours.Name = "radioRangeIncomeFixedHours";
            this.radioRangeIncomeFixedHours.Size = new System.Drawing.Size(157, 17);
            this.radioRangeIncomeFixedHours.TabIndex = 12;
            this.radioRangeIncomeFixedHours.TabStop = true;
            this.radioRangeIncomeFixedHours.Text = "Range Income, Fixed Hours";
            this.radioRangeIncomeFixedHours.UseVisualStyleBackColor = true;
            this.radioRangeIncomeFixedHours.CheckedChanged += new System.EventHandler(this.budgetConstraintsRangeType_CheckedChanged);
            // 
            // radioRangeHoursFixedWage
            // 
            this.radioRangeHoursFixedWage.AutoSize = true;
            this.radioRangeHoursFixedWage.Checked = true;
            this.radioRangeHoursFixedWage.Location = new System.Drawing.Point(8, 55);
            this.radioRangeHoursFixedWage.Name = "radioRangeHoursFixedWage";
            this.radioRangeHoursFixedWage.Size = new System.Drawing.Size(151, 17);
            this.radioRangeHoursFixedWage.TabIndex = 9;
            this.radioRangeHoursFixedWage.TabStop = true;
            this.radioRangeHoursFixedWage.Text = "Range Hours, Fixed Wage";
            this.radioRangeHoursFixedWage.UseVisualStyleBackColor = true;
            this.radioRangeHoursFixedWage.CheckedChanged += new System.EventHandler(this.budgetConstraintsRangeType_CheckedChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(4, 24);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(73, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Person in HH:";
            // 
            // comboPerson
            // 
            this.comboPerson.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboPerson.FormattingEnabled = true;
            this.comboPerson.Location = new System.Drawing.Point(91, 21);
            this.comboPerson.Margin = new System.Windows.Forms.Padding(2);
            this.comboPerson.Name = "comboPerson";
            this.comboPerson.Size = new System.Drawing.Size(251, 21);
            this.comboPerson.TabIndex = 8;
            this.comboPerson.SelectedIndexChanged += new System.EventHandler(this.comboPerson_SelectedIndexChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(236, 112);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 13);
            this.label5.TabIndex = 20;
            this.label5.Text = "Step";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(146, 112);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(16, 13);
            this.label4.TabIndex = 19;
            this.label4.Text = "to";
            // 
            // labFrom
            // 
            this.labFrom.AutoSize = true;
            this.labFrom.Location = new System.Drawing.Point(4, 112);
            this.labFrom.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labFrom.Name = "labFrom";
            this.labFrom.Size = new System.Drawing.Size(58, 13);
            this.labFrom.TabIndex = 17;
            this.labFrom.Text = "Hours from";
            // 
            // labProgress
            // 
            this.labProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labProgress.AutoSize = true;
            this.labProgress.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labProgress.ForeColor = System.Drawing.Color.Purple;
            this.labProgress.Location = new System.Drawing.Point(8, 475);
            this.labProgress.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labProgress.Name = "labProgress";
            this.labProgress.Size = new System.Drawing.Size(0, 17);
            this.labProgress.TabIndex = 12;
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(304, 472);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(59, 25);
            this.btnStart.TabIndex = 18;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnNotVisibleAcceptButton
            // 
            this.btnNotVisibleAcceptButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNotVisibleAcceptButton.Location = new System.Drawing.Point(220, 467);
            this.btnNotVisibleAcceptButton.Margin = new System.Windows.Forms.Padding(2);
            this.btnNotVisibleAcceptButton.Name = "btnNotVisibleAcceptButton";
            this.btnNotVisibleAcceptButton.Size = new System.Drawing.Size(80, 34);
            this.btnNotVisibleAcceptButton.TabIndex = 20;
            this.btnNotVisibleAcceptButton.TabStop = false;
            this.btnNotVisibleAcceptButton.Text = "Not visible Accept button";
            this.btnNotVisibleAcceptButton.UseVisualStyleBackColor = true;
            this.btnNotVisibleAcceptButton.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 122);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(88, 13);
            this.label6.TabIndex = 16;
            this.label6.Text = "Household Type:";
            // 
            // chkOutputInEuro
            // 
            this.chkOutputInEuro.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkOutputInEuro.AutoSize = true;
            this.chkOutputInEuro.Location = new System.Drawing.Point(18, 447);
            this.chkOutputInEuro.Name = "chkOutputInEuro";
            this.chkOutputInEuro.Size = new System.Drawing.Size(148, 17);
            this.chkOutputInEuro.TabIndex = 17;
            this.chkOutputInEuro.Text = "Produce all output in Euro";
            this.chkOutputInEuro.UseVisualStyleBackColor = true;
            this.chkOutputInEuro.CheckedChanged += new System.EventHandler(this.chkOutputInEuro_CheckedChanged);
            // 
            // labelControl1
            // 
            this.labelControl1.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelControl1.Location = new System.Drawing.Point(14, 11);
            this.labelControl1.Name = "labelControl1";
            this.labelControl1.Size = new System.Drawing.Size(57, 13);
            this.labelControl1.TabIndex = 19;
            this.labelControl1.Text = "Countries:";
            // 
            // countriesCheckedComboBoxEdit
            // 
            this.countriesCheckedComboBoxEdit.Location = new System.Drawing.Point(77, 8);
            this.countriesCheckedComboBoxEdit.Name = "countriesCheckedComboBoxEdit";
            this.countriesCheckedComboBoxEdit.Properties.AllowMultiSelect = true;
            this.countriesCheckedComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.countriesCheckedComboBoxEdit.Properties.DisplayMember = "Country";
            this.countriesCheckedComboBoxEdit.Properties.ValueMember = "Country";
            this.countriesCheckedComboBoxEdit.Size = new System.Drawing.Size(113, 20);
            this.countriesCheckedComboBoxEdit.TabIndex = 0;
            this.countriesCheckedComboBoxEdit.EditValueChanged += new System.EventHandler(this.countriesCheckedComboBoxEdit_EditValueChanged);
            // 
            // labelSelectedCountries
            // 
            this.labelSelectedCountries.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelSelectedCountries.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelSelectedCountries.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelSelectedCountries.Location = new System.Drawing.Point(14, 34);
            this.labelSelectedCountries.Name = "labelSelectedCountries";
            this.labelSelectedCountries.Size = new System.Drawing.Size(202, 29);
            this.labelSelectedCountries.TabIndex = 20;
            this.labelSelectedCountries.Text = "No countries selected!";
            // 
            // labelControl5
            // 
            this.labelControl5.Appearance.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(161)));
            this.labelControl5.Location = new System.Drawing.Point(231, 11);
            this.labelControl5.Name = "labelControl5";
            this.labelControl5.Size = new System.Drawing.Size(35, 13);
            this.labelControl5.TabIndex = 22;
            this.labelControl5.Text = "Years:";
            // 
            // labelSelectedYears
            // 
            this.labelSelectedYears.Appearance.TextOptions.VAlignment = DevExpress.Utils.VertAlignment.Top;
            this.labelSelectedYears.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.labelSelectedYears.AutoSizeMode = DevExpress.XtraEditors.LabelAutoSizeMode.None;
            this.labelSelectedYears.Location = new System.Drawing.Point(231, 34);
            this.labelSelectedYears.Name = "labelSelectedYears";
            this.labelSelectedYears.Size = new System.Drawing.Size(184, 29);
            this.labelSelectedYears.TabIndex = 23;
            this.labelSelectedYears.Text = "No years selected!";
            // 
            // yearsCheckedComboBoxEdit
            // 
            this.yearsCheckedComboBoxEdit.Location = new System.Drawing.Point(272, 8);
            this.yearsCheckedComboBoxEdit.Name = "yearsCheckedComboBoxEdit";
            this.yearsCheckedComboBoxEdit.Properties.AllowMultiSelect = true;
            this.yearsCheckedComboBoxEdit.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.yearsCheckedComboBoxEdit.Properties.DisplayMember = "Year";
            this.yearsCheckedComboBoxEdit.Properties.ValueMember = "Year";
            this.yearsCheckedComboBoxEdit.Size = new System.Drawing.Size(113, 20);
            this.yearsCheckedComboBoxEdit.TabIndex = 1;
            this.yearsCheckedComboBoxEdit.EditValueChanged += new System.EventHandler(this.yearsCheckedComboBoxEdit_EditValueChanged);
            // 
            // listHHType_MultiSelect
            // 
            this.listHHType_MultiSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listHHType_MultiSelect.CheckOnClick = true;
            this.listHHType_MultiSelect.FormattingEnabled = true;
            this.listHHType_MultiSelect.Location = new System.Drawing.Point(12, 139);
            this.listHHType_MultiSelect.Name = "listHHType_MultiSelect";
            this.listHHType_MultiSelect.Size = new System.Drawing.Size(419, 109);
            this.listHHType_MultiSelect.TabIndex = 24;
            this.listHHType_MultiSelect.SelectedIndexChanged += new System.EventHandler(this.listHHType_SelectedIndexChanged);
            // 
            // listHHType_SingleSelect
            // 
            this.listHHType_SingleSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listHHType_SingleSelect.CheckOnClick = true;
            this.listHHType_SingleSelect.FormattingEnabled = true;
            this.listHHType_SingleSelect.Location = new System.Drawing.Point(12, 139);
            this.listHHType_SingleSelect.Name = "listHHType_SingleSelect";
            this.listHHType_SingleSelect.Size = new System.Drawing.Size(419, 109);
            this.listHHType_SingleSelect.TabIndex = 25;
            this.listHHType_SingleSelect.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.listHHType_SingleSelect_ItemCheck);
            this.listHHType_SingleSelect.SelectedIndexChanged += new System.EventHandler(this.listHHType_SelectedIndexChanged);
            // 
            // StatisticsWizardForm
            // 
            this.AcceptButton = this.btnNotVisibleAcceptButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(443, 506);
            this.Controls.Add(this.listHHType_SingleSelect);
            this.Controls.Add(this.listHHType_MultiSelect);
            this.Controls.Add(this.labelControl5);
            this.Controls.Add(this.labelSelectedYears);
            this.Controls.Add(this.yearsCheckedComboBoxEdit);
            this.Controls.Add(this.labelControl1);
            this.Controls.Add(this.countriesCheckedComboBoxEdit);
            this.Controls.Add(this.labelSelectedCountries);
            this.Controls.Add(this.chkOutputInEuro);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.btnNotVisibleAcceptButton);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.labProgress);
            this.Controls.Add(this.groupBoxBudget);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnCancel);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "StatisticsWizardForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Statistics Wizard";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.StatisticsWizardForm_FormClosing);
            this.Shown += new System.EventHandler(this.StatisticsWizardForm_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBoxBudget.ResumeLayout(false);
            this.groupBoxBudget.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.countriesCheckedComboBoxEdit.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.yearsCheckedComboBoxEdit.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.RadioButton radioBudget;
        private System.Windows.Forms.RadioButton radioTypes;
        private System.Windows.Forms.RadioButton radioCountryYear;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBoxBudget;
        private System.Windows.Forms.TextBox txtRangeFrom;
        private System.Windows.Forms.Label labFrom;
        private System.Windows.Forms.TextBox txtRangeStep;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtRangeTo;
        private System.Windows.Forms.Label labProgress;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnNotVisibleAcceptButton;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboPerson;
        private System.Windows.Forms.RadioButton radioRangeIncomeFixedWage;
        private System.Windows.Forms.RadioButton radioRangeWageFixedHours;
        private System.Windows.Forms.RadioButton radioRangeIncomeFixedHours;
        private System.Windows.Forms.RadioButton radioRangeHoursFixedWage;
        private System.Windows.Forms.Label labFixedVal;
        private System.Windows.Forms.TextBox txtFixedValue;
        private System.Windows.Forms.CheckBox chkOutputInEuro;
        private DevExpress.XtraEditors.LabelControl labelControl1;
        internal DevExpress.XtraEditors.CheckedComboBoxEdit countriesCheckedComboBoxEdit;
        private DevExpress.XtraEditors.LabelControl labelSelectedCountries;
        private DevExpress.XtraEditors.LabelControl labelControl5;
        private DevExpress.XtraEditors.LabelControl labelSelectedYears;
        internal DevExpress.XtraEditors.CheckedComboBoxEdit yearsCheckedComboBoxEdit;
        private System.Windows.Forms.CheckedListBox listHHType_MultiSelect;
        private System.Windows.Forms.CheckedListBox listHHType_SingleSelect;
    }
}