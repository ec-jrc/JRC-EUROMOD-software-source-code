namespace EM_UI.Dialogs
{
    partial class AssignSystemsForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.txPasteCountry = new System.Windows.Forms.TextBox();
            this.txCopyCountry = new System.Windows.Forms.TextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.tipAssignSystems = new System.Windows.Forms.ToolTip();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.dgvAssign = new System.Windows.Forms.DataGridView();
            this.colSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAssign = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.btnClear = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvAssign)).BeginInit();
            this.SuspendLayout();
            // 
            // txPasteCountry
            // 
            this.txPasteCountry.BackColor = System.Drawing.SystemColors.Control;
            this.txPasteCountry.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txPasteCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txPasteCountry.Location = new System.Drawing.Point(9, 9);
            this.txPasteCountry.Margin = new System.Windows.Forms.Padding(2);
            this.txPasteCountry.Name = "txPasteCountry";
            this.txPasteCountry.ReadOnly = true;
            this.txPasteCountry.Size = new System.Drawing.Size(195, 13);
            this.txPasteCountry.TabIndex = 1;
            this.txPasteCountry.TabStop = false;
            // 
            // txCopyCountry
            // 
            this.txCopyCountry.BackColor = System.Drawing.SystemColors.Control;
            this.txCopyCountry.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txCopyCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txCopyCountry.Location = new System.Drawing.Point(250, 9);
            this.txCopyCountry.Margin = new System.Windows.Forms.Padding(2);
            this.txCopyCountry.Name = "txCopyCountry";
            this.txCopyCountry.ReadOnly = true;
            this.txCopyCountry.Size = new System.Drawing.Size(98, 13);
            this.txCopyCountry.TabIndex = 3;
            this.txCopyCountry.TabStop = false;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(174, 164);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(2);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(66, 22);
            this.btnCancel.TabIndex = 5;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(102, 164);
            this.btnOK.Margin = new System.Windows.Forms.Padding(2);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(66, 22);
            this.btnOK.TabIndex = 4;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // tipAssignSystems
            // 
            this.tipAssignSystems.AutomaticDelay = 0;
            this.tipAssignSystems.AutoPopDelay = 500000;
            this.tipAssignSystems.InitialDelay = 0;
            this.tipAssignSystems.ReshowDelay = 0;
            this.tipAssignSystems.ShowAlways = true;
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // dgvAssign
            // 
            this.dgvAssign.AllowUserToAddRows = false;
            this.dgvAssign.AllowUserToDeleteRows = false;
            this.dgvAssign.AllowUserToResizeRows = false;
            this.dgvAssign.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvAssign.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dgvAssign.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAssign.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvAssign.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvAssign.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSystem,
            this.colAssign});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Thistle;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvAssign.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvAssign.GridColor = System.Drawing.SystemColors.Control;
            this.dgvAssign.Location = new System.Drawing.Point(9, 24);
            this.dgvAssign.Margin = new System.Windows.Forms.Padding(2);
            this.dgvAssign.MultiSelect = false;
            this.dgvAssign.Name = "dgvAssign";
            this.dgvAssign.RowHeadersVisible = false;
            this.dgvAssign.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvAssign.Size = new System.Drawing.Size(232, 127);
            this.dgvAssign.TabIndex = 33;
            this.dgvAssign.TabStop = false;
            this.dgvAssign.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvAssign_CellClick);
            // 
            // colSystem
            // 
            this.colSystem.Frozen = true;
            this.colSystem.HeaderText = "Destination";
            this.colSystem.Name = "colSystem";
            this.colSystem.Width = 150;
            // 
            // colAssign
            // 
            this.colAssign.HeaderText = "Origin";
            this.colAssign.Name = "colAssign";
            this.colAssign.ReadOnly = true;
            this.colAssign.ToolTipText = "Click in respective cell to select the system";
            this.colAssign.Width = 150;
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnClear.Location = new System.Drawing.Point(9, 164);
            this.btnClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(77, 22);
            this.btnClear.TabIndex = 34;
            this.btnClear.Text = "&Clear Assignement";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // AssignSystemsForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(250, 191);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.dgvAssign);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txCopyCountry);
            this.Controls.Add(this.txPasteCountry);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.TableOfContents);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AssignSystemsForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Assign Systems";
            this.Load += new System.EventHandler(this.AssignSystemsForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvAssign)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txPasteCountry;
        private System.Windows.Forms.TextBox txCopyCountry;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ToolTip tipAssignSystems;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.DataGridView dgvAssign;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSystem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAssign;
    }
}