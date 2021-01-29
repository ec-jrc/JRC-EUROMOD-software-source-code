namespace EM_UI.Dialogs
{
    partial class ConfigureSystemsForm
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblCountry = new System.Windows.Forms.Label();
            this.tipSystemConfiguration = new System.Windows.Forms.ToolTip(this.components);
            this.dgvSystems = new System.Windows.Forms.DataGridView();
            this.colSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExchangeRate = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colYear = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colCurrencyParameters = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colCurrencyOutput = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colPrivate = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.colHeadDefInc = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.colComment = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystems)).BeginInit();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(1064, 80);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(120, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(1064, 44);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(120, 28);
            this.btnOK.TabIndex = 2;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCountry.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.lblCountry.Location = new System.Drawing.Point(11, 11);
            this.lblCountry.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(88, 25);
            this.lblCountry.TabIndex = 28;
            this.lblCountry.Text = "Country";
            // 
            // dgvSystems
            // 
            this.dgvSystems.AllowUserToAddRows = false;
            this.dgvSystems.AllowUserToDeleteRows = false;
            this.dgvSystems.AllowUserToResizeRows = false;
            this.dgvSystems.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvSystems.BackgroundColor = System.Drawing.SystemColors.Control;
            this.dgvSystems.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvSystems.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSystem,
            this.colExchangeRate,
            this.colYear,
            this.colCurrencyParameters,
            this.colCurrencyOutput,
            this.colPrivate,
            this.colHeadDefInc,
            this.colComment});
            this.dgvSystems.GridColor = System.Drawing.SystemColors.Control;
            this.dgvSystems.Location = new System.Drawing.Point(28, 44);
            this.dgvSystems.Margin = new System.Windows.Forms.Padding(4);
            this.dgvSystems.Name = "dgvSystems";
            this.dgvSystems.Size = new System.Drawing.Size(1012, 316);
            this.dgvSystems.TabIndex = 1;
            // 
            // colSystem
            // 
            this.colSystem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colSystem.HeaderText = "System";
            this.colSystem.Name = "colSystem";
            this.colSystem.ReadOnly = true;
            this.colSystem.Width = 83;
            // 
            // colExchangeRate
            // 
            this.colExchangeRate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.Gray;
            this.colExchangeRate.DefaultCellStyle = dataGridViewCellStyle1;
            this.colExchangeRate.HeaderText = "Exchange Rate";
            this.colExchangeRate.Name = "colExchangeRate";
            this.colExchangeRate.ReadOnly = true;
            this.colExchangeRate.ToolTipText = "Exchange Rate Euro to National Currency";
            this.colExchangeRate.Width = 122;
            // 
            // colYear
            // 
            this.colYear.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colYear.HeaderText = "Year";
            this.colYear.Name = "colYear";
            this.colYear.Width = 67;
            // 
            // colCurrencyParameters
            // 
            this.colCurrencyParameters.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colCurrencyParameters.HeaderText = "Currency Parameters";
            this.colCurrencyParameters.Name = "colCurrencyParameters";
            this.colCurrencyParameters.Width = 133;
            // 
            // colCurrencyOutput
            // 
            this.colCurrencyOutput.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colCurrencyOutput.HeaderText = "Currency Output";
            this.colCurrencyOutput.Name = "colCurrencyOutput";
            this.colCurrencyOutput.Width = 106;
            // 
            // colPrivate
            // 
            this.colPrivate.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colPrivate.HeaderText = "Private";
            this.colPrivate.Name = "colPrivate";
            this.colPrivate.Width = 58;
            // 
            // colHeadDefInc
            // 
            this.colHeadDefInc.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colHeadDefInc.HeaderText = "Income for Unit Head Definition";
            this.colHeadDefInc.Name = "colHeadDefInc";
            this.colHeadDefInc.Width = 102;
            // 
            // colComment
            // 
            this.colComment.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colComment.HeaderText = "Comment";
            this.colComment.Name = "colComment";
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // ConfigureSystemsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(1199, 375);
            this.Controls.Add(this.dgvSystems);
            this.Controls.Add(this.lblCountry);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_SystemSettings.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigureSystemsForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "System Configuration";
            this.Load += new System.EventHandler(this.ConfigureSystemsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvSystems)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.ToolTip tipSystemConfiguration;
        private System.Windows.Forms.DataGridView dgvSystems;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSystem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colExchangeRate;
        private System.Windows.Forms.DataGridViewTextBoxColumn colYear;
        private System.Windows.Forms.DataGridViewComboBoxColumn colCurrencyParameters;
        private System.Windows.Forms.DataGridViewComboBoxColumn colCurrencyOutput;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colPrivate;
        private System.Windows.Forms.DataGridViewComboBoxColumn colHeadDefInc;
        private System.Windows.Forms.DataGridViewTextBoxColumn colComment;
    }
}