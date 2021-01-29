namespace HypotheticalHousehold
{
    partial class NumericEditor
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.checkEndingValue = new System.Windows.Forms.CheckBox();
            this.labelStep = new System.Windows.Forms.Label();
            this.labelStart = new System.Windows.Forms.Label();
            this.radioButtonReference = new System.Windows.Forms.RadioButton();
            this.radioButtonValue = new System.Windows.Forms.RadioButton();
            this.lookUpEditReference = new DevExpress.XtraEditors.LookUpEdit();
            this.variableDataSet = new HypotheticalHousehold.VariableDataSet();
            this.panelPercent = new System.Windows.Forms.Panel();
            this.labelPercStep = new System.Windows.Forms.Label();
            this.labelPercEnd = new System.Windows.Forms.Label();
            this.labelPercStart = new System.Windows.Forms.Label();
            this.numericUpDownStartingValue = new HypotheticalHousehold.NumericEditor.CustomNumericUpDown();
            this.numericUpDownEndingValue = new HypotheticalHousehold.NumericEditor.CustomNumericUpDown();
            this.numericUpDownStep = new HypotheticalHousehold.NumericEditor.CustomNumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEditReference.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.variableDataSet)).BeginInit();
            this.panelPercent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartingValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEndingValue)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStep)).BeginInit();
            this.SuspendLayout();
            // 
            // checkEndingValue
            // 
            this.checkEndingValue.Location = new System.Drawing.Point(13, 79);
            this.checkEndingValue.Name = "checkEndingValue";
            this.checkEndingValue.Size = new System.Drawing.Size(95, 17);
            this.checkEndingValue.TabIndex = 1;
            this.checkEndingValue.Text = "Ending Value:";
            this.checkEndingValue.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkEndingValue.UseVisualStyleBackColor = true;
            this.checkEndingValue.CheckedChanged += new System.EventHandler(this.checkEndingValue_CheckedChanged);
            // 
            // labelStep
            // 
            this.labelStep.Location = new System.Drawing.Point(13, 106);
            this.labelStep.Name = "labelStep";
            this.labelStep.Size = new System.Drawing.Size(92, 13);
            this.labelStep.TabIndex = 23;
            this.labelStep.Text = "Step:";
            this.labelStep.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // labelStart
            // 
            this.labelStart.Location = new System.Drawing.Point(13, 54);
            this.labelStart.Name = "labelStart";
            this.labelStart.Size = new System.Drawing.Size(92, 13);
            this.labelStart.TabIndex = 22;
            this.labelStart.Text = "Starting Value:";
            this.labelStart.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // radioButtonReference
            // 
            this.radioButtonReference.AutoSize = true;
            this.radioButtonReference.Location = new System.Drawing.Point(76, 13);
            this.radioButtonReference.Name = "radioButtonReference";
            this.radioButtonReference.Size = new System.Drawing.Size(75, 17);
            this.radioButtonReference.TabIndex = 5;
            this.radioButtonReference.TabStop = true;
            this.radioButtonReference.Text = "Reference";
            this.radioButtonReference.UseVisualStyleBackColor = true;
            this.radioButtonReference.CheckedChanged += new System.EventHandler(this.radioButtonReference_CheckedChanged);
            // 
            // radioButtonValue
            // 
            this.radioButtonValue.AutoSize = true;
            this.radioButtonValue.Location = new System.Drawing.Point(10, 13);
            this.radioButtonValue.Name = "radioButtonValue";
            this.radioButtonValue.Size = new System.Drawing.Size(52, 17);
            this.radioButtonValue.TabIndex = 4;
            this.radioButtonValue.TabStop = true;
            this.radioButtonValue.Text = "Value";
            this.radioButtonValue.UseVisualStyleBackColor = true;
            this.radioButtonValue.CheckedChanged += new System.EventHandler(this.radioButtonValue_CheckedChanged);
            // 
            // lookUpEditReference
            // 
            this.lookUpEditReference.Location = new System.Drawing.Point(157, 12);
            this.lookUpEditReference.Name = "lookUpEditReference";
            this.lookUpEditReference.Properties.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.lookUpEditReference.Properties.Columns.AddRange(new DevExpress.XtraEditors.Controls.LookUpColumnInfo[] {
            new DevExpress.XtraEditors.Controls.LookUpColumnInfo("TableDescription", "TableDescription")});
            this.lookUpEditReference.Properties.DisplayMember = "TableDescription";
            this.lookUpEditReference.Properties.NullText = "";
            this.lookUpEditReference.Properties.ShowFooter = false;
            this.lookUpEditReference.Properties.ShowHeader = false;
            this.lookUpEditReference.Properties.ValueMember = "TableName";
            this.lookUpEditReference.Size = new System.Drawing.Size(138, 20);
            this.lookUpEditReference.TabIndex = 6;
            // 
            // variableDataSet
            // 
            this.variableDataSet.DataSetName = "VariableDataSet";
            this.variableDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // panelPercent
            // 
            this.panelPercent.Controls.Add(this.labelPercStep);
            this.panelPercent.Controls.Add(this.labelPercEnd);
            this.panelPercent.Controls.Add(this.labelPercStart);
            this.panelPercent.Location = new System.Drawing.Point(257, 49);
            this.panelPercent.Name = "panelPercent";
            this.panelPercent.Size = new System.Drawing.Size(19, 74);
            this.panelPercent.TabIndex = 29;
            // 
            // labelPercStep
            // 
            this.labelPercStep.AutoSize = true;
            this.labelPercStep.Location = new System.Drawing.Point(-1, 57);
            this.labelPercStep.Name = "labelPercStep";
            this.labelPercStep.Size = new System.Drawing.Size(15, 13);
            this.labelPercStep.TabIndex = 26;
            this.labelPercStep.Text = "%";
            // 
            // labelPercEnd
            // 
            this.labelPercEnd.AutoSize = true;
            this.labelPercEnd.Location = new System.Drawing.Point(-1, 31);
            this.labelPercEnd.Name = "labelPercEnd";
            this.labelPercEnd.Size = new System.Drawing.Size(15, 13);
            this.labelPercEnd.TabIndex = 25;
            this.labelPercEnd.Text = "%";
            // 
            // labelPercStart
            // 
            this.labelPercStart.AutoSize = true;
            this.labelPercStart.Location = new System.Drawing.Point(-1, 5);
            this.labelPercStart.Name = "labelPercStart";
            this.labelPercStart.Size = new System.Drawing.Size(15, 13);
            this.labelPercStart.TabIndex = 24;
            this.labelPercStart.Text = "%";
            // 
            // numericUpDownStartingValue
            // 
            this.numericUpDownStartingValue.DecimalPlaces = 6;
            this.numericUpDownStartingValue.Location = new System.Drawing.Point(161, 52);
            this.numericUpDownStartingValue.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDownStartingValue.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numericUpDownStartingValue.Minimum = new decimal(new int[] {
            9999999,
            0,
            0,
            -2147483648});
            this.numericUpDownStartingValue.Name = "numericUpDownStartingValue";
            this.numericUpDownStartingValue.Size = new System.Drawing.Size(90, 20);
            this.numericUpDownStartingValue.TabIndex = 0;
            // 
            // numericUpDownEndingValue
            // 
            this.numericUpDownEndingValue.DecimalPlaces = 6;
            this.numericUpDownEndingValue.Location = new System.Drawing.Point(162, 78);
            this.numericUpDownEndingValue.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDownEndingValue.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numericUpDownEndingValue.Minimum = new decimal(new int[] {
            9999999,
            0,
            0,
            -2147483648});
            this.numericUpDownEndingValue.Name = "numericUpDownEndingValue";
            this.numericUpDownEndingValue.Size = new System.Drawing.Size(90, 20);
            this.numericUpDownEndingValue.TabIndex = 2;
            // 
            // numericUpDownStep
            // 
            this.numericUpDownStep.DecimalPlaces = 6;
            this.numericUpDownStep.Location = new System.Drawing.Point(161, 104);
            this.numericUpDownStep.Margin = new System.Windows.Forms.Padding(2);
            this.numericUpDownStep.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numericUpDownStep.Minimum = new decimal(new int[] {
            9999999,
            0,
            0,
            -2147483648});
            this.numericUpDownStep.Name = "numericUpDownStep";
            this.numericUpDownStep.Size = new System.Drawing.Size(90, 20);
            this.numericUpDownStep.TabIndex = 3;
            // 
            // NumericEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.Controls.Add(this.numericUpDownStep);
            this.Controls.Add(this.numericUpDownEndingValue);
            this.Controls.Add(this.numericUpDownStartingValue);
            this.Controls.Add(this.panelPercent);
            this.Controls.Add(this.lookUpEditReference);
            this.Controls.Add(this.checkEndingValue);
            this.Controls.Add(this.labelStep);
            this.Controls.Add(this.labelStart);
            this.Controls.Add(this.radioButtonReference);
            this.Controls.Add(this.radioButtonValue);
            this.Name = "NumericEditor";
            this.Size = new System.Drawing.Size(311, 144);
            ((System.ComponentModel.ISupportInitialize)(this.lookUpEditReference.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.variableDataSet)).EndInit();
            this.panelPercent.ResumeLayout(false);
            this.panelPercent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStartingValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownEndingValue)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownStep)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkEndingValue;
        private System.Windows.Forms.Label labelStep;
        private System.Windows.Forms.Label labelStart;
        private System.Windows.Forms.RadioButton radioButtonReference;
        private System.Windows.Forms.RadioButton radioButtonValue;
        private DevExpress.XtraEditors.LookUpEdit lookUpEditReference;
        private VariableDataSet variableDataSet;
        private System.Windows.Forms.Panel panelPercent;
        private System.Windows.Forms.Label labelPercStep;
        private System.Windows.Forms.Label labelPercEnd;
        private System.Windows.Forms.Label labelPercStart;
        private CustomNumericUpDown numericUpDownStartingValue;
        private CustomNumericUpDown numericUpDownEndingValue;
        private CustomNumericUpDown numericUpDownStep;
    }
}
