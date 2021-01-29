namespace EM_UI.GlobalAdministration
{
    partial class ExchangeRatesForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnAddRow = new System.Windows.Forms.Button();
            this.cmbCountryToAdd = new System.Windows.Forms.ComboBox();
            this.dgvRates = new EM_Common_Win.CustomDataGrid();
            this.colCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colJune30 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colYearAverage = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colFirstSemester = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSecondSemester = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDefault = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colValidFor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colID = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnDeleteRow = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRates)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.AutoSize = true;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(978, 48);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(118, 27);
            this.btnCancel.TabIndex = 10;
            this.btnCancel.Text = "    Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.AutoSize = true;
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(978, 12);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(118, 28);
            this.btnOK.TabIndex = 9;
            this.btnOK.Text = "    Save && Close";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnAddRow
            // 
            this.btnAddRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddRow.Location = new System.Drawing.Point(625, 672);
            this.btnAddRow.Name = "btnAddRow";
            this.btnAddRow.Size = new System.Drawing.Size(170, 24);
            this.btnAddRow.TabIndex = 11;
            this.btnAddRow.Text = "Add Row for Country ...";
            this.btnAddRow.UseVisualStyleBackColor = true;
            this.btnAddRow.Click += new System.EventHandler(this.btnAddRow_Click);
            // 
            // cmbCountryToAdd
            // 
            this.cmbCountryToAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbCountryToAdd.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCountryToAdd.FormattingEnabled = true;
            this.cmbCountryToAdd.Location = new System.Drawing.Point(625, 702);
            this.cmbCountryToAdd.Name = "cmbCountryToAdd";
            this.cmbCountryToAdd.Size = new System.Drawing.Size(170, 24);
            this.cmbCountryToAdd.Sorted = true;
            this.cmbCountryToAdd.TabIndex = 15;
            // 
            // dgvRates
            // 
            this.dgvRates.AllowUserToAddRows = false;
            this.dgvRates.AllowUserToDeleteRows = false;
            this.dgvRates.AllowUserToResizeRows = false;
            this.dgvRates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRates.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvRates.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle10.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle10.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle10.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle10.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle10.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle10.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            this.dgvRates.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle10;
            this.dgvRates.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRates.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCountry,
            this.colJune30,
            this.colYearAverage,
            this.colFirstSemester,
            this.colSecondSemester,
            this.colDefault,
            this.colValidFor,
            this.colID});
            dataGridViewCellStyle18.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle18.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle18.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle18.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle18.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle18.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle18.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvRates.DefaultCellStyle = dataGridViewCellStyle18;
            this.dgvRates.Location = new System.Drawing.Point(12, 12);
            this.dgvRates.Name = "dgvRates";
            this.dgvRates.RowTemplate.Height = 24;
            this.dgvRates.ShowCellErrors = false;
            this.dgvRates.ShowEditingIcon = false;
            this.dgvRates.ShowRowErrors = false;
            this.dgvRates.Size = new System.Drawing.Size(959, 654);
            this.dgvRates.TabIndex = 13;
            this.dgvRates.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvRates_CellClick);
            this.dgvRates.MouseDown += EM_UI.Dialogs.SingleClickForDataGridCombo.HandleDataGridViewMouseDown;
            // 
            // colCountry
            // 
            this.colCountry.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colCountry.DataPropertyName = "colCountry";
            dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colCountry.DefaultCellStyle = dataGridViewCellStyle11;
            this.colCountry.HeaderText = "Country";
            this.colCountry.Name = "colCountry";
            this.colCountry.ReadOnly = true;
            this.colCountry.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colCountry.Width = 63;
            // 
            // colJune30
            // 
            this.colJune30.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colJune30.DataPropertyName = "colJune30";
            dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colJune30.DefaultCellStyle = dataGridViewCellStyle12;
            this.colJune30.HeaderText = "June 30";
            this.colJune30.Name = "colJune30";
            this.colJune30.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colJune30.Width = 59;
            // 
            // colYearAverage
            // 
            this.colYearAverage.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colYearAverage.DataPropertyName = "colYearAverage";
            dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colYearAverage.DefaultCellStyle = dataGridViewCellStyle13;
            this.colYearAverage.HeaderText = "Year Average";
            this.colYearAverage.Name = "colYearAverage";
            this.colYearAverage.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colYearAverage.Width = 91;
            // 
            // colFirstSemester
            // 
            this.colFirstSemester.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colFirstSemester.DataPropertyName = "colFirstSemester";
            dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colFirstSemester.DefaultCellStyle = dataGridViewCellStyle14;
            this.colFirstSemester.HeaderText = "First Semester";
            this.colFirstSemester.Name = "colFirstSemester";
            this.colFirstSemester.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colFirstSemester.Width = 95;
            this.colFirstSemester.Visible = false;
            // 
            // colSecondSemester
            // 
            this.colSecondSemester.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colSecondSemester.DataPropertyName = "colSecondSemester";
            dataGridViewCellStyle15.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colSecondSemester.DefaultCellStyle = dataGridViewCellStyle15;
            this.colSecondSemester.HeaderText = "Second Semester";
            this.colSecondSemester.Name = "colSecondSemester";
            this.colSecondSemester.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colSecondSemester.Width = 113;
            this.colSecondSemester.Visible = false;
            // 
            // colDefault
            // 
            this.colDefault.DataPropertyName = "colDefault";
            dataGridViewCellStyle16.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            this.colDefault.DefaultCellStyle = dataGridViewCellStyle16;
            this.colDefault.HeaderText = "Default";
            this.colDefault.Items.AddRange(new object[] {
            "June 30",
            "Year Average"/*,
            "First Semester",
            "Second Semester"*/});
            this.colDefault.Name = "colDefault";
            this.colDefault.Width = 150;
            // 
            // colValidFor
            // 
            this.colValidFor.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colValidFor.DataPropertyName = "colValidFor";
            dataGridViewCellStyle17.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle17.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.colValidFor.DefaultCellStyle = dataGridViewCellStyle17;
            this.colValidFor.HeaderText = "Valid for";
            this.colValidFor.Name = "colValidFor";
            this.colValidFor.ReadOnly = true;
            this.colValidFor.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // colID
            // 
            this.colID.DataPropertyName = "colID";
            this.colID.HeaderText = "ID";
            this.colID.Name = "colID";
            this.colID.ReadOnly = true;
            this.colID.Visible = false;
            // 
            // btnDeleteRow
            // 
            this.btnDeleteRow.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteRow.Location = new System.Drawing.Point(801, 672);
            this.btnDeleteRow.Name = "btnDeleteRow";
            this.btnDeleteRow.Size = new System.Drawing.Size(170, 24);
            this.btnDeleteRow.TabIndex = 16;
            this.btnDeleteRow.Text = "Delete Selected Row(s)";
            this.btnDeleteRow.UseVisualStyleBackColor = true;
            this.btnDeleteRow.Click += new System.EventHandler(this.btnDeleteRow_Click);
            // 
            // ExchangeRatesForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1109, 738);
            this.Controls.Add(this.btnDeleteRow);
            this.Controls.Add(this.cmbCountryToAdd);
            this.Controls.Add(this.dgvRates);
            this.Controls.Add(this.btnAddRow);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Name = "ExchangeRatesForm";
            this.ShowIcon = false;
            this.Text = "EURO Exchange Rates";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExchangeRatesForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRates)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnAddRow;
        private EM_Common_Win.CustomDataGrid dgvRates;
        private System.Windows.Forms.ComboBox cmbCountryToAdd;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCountry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colJune30;
        private System.Windows.Forms.DataGridViewTextBoxColumn colYearAverage;
        private System.Windows.Forms.DataGridViewTextBoxColumn colFirstSemester;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSecondSemester;
        private System.Windows.Forms.DataGridViewComboBoxColumn colDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn colValidFor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colID;
        private System.Windows.Forms.Button btnDeleteRow;
    }
}