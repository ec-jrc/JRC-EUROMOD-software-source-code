namespace EM_UI.Dialogs
{
    partial class ConditionalFormattingForm
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConditionalFormattingForm));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.lblCountry = new System.Windows.Forms.Label();
            this.dgvConditionalFormatting = new System.Windows.Forms.DataGridView();
            this.colCondition = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colSystemsToApply = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBackColorConditionalFormatting = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colForeColorConditionalFormatting = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.dgvBaseSystemFormatting = new System.Windows.Forms.DataGridView();
            this.colSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBaseSystem = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colBackColorBaseSystemFormatting = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colForeColorBaseSystemFormatting = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colExpandDifferences = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.label2 = new System.Windows.Forms.Label();
            this.btnDeleteConditionalFormatting = new System.Windows.Forms.Button();
            this.btnAddConditionalFormatting = new System.Windows.Forms.Button();
            this.dlgColor = new System.Windows.Forms.ColorDialog();
            this.tipConditionalFormatting = new System.Windows.Forms.ToolTip(this.components);
            this.btnBaseSystemFormattingSetDefault = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.menuClearColor = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.itemClearColor = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.dgvConditionalFormatting)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBaseSystemFormatting)).BeginInit();
            this.menuClearColor.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnCancel.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnCancel.Location = new System.Drawing.Point(570, 520);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(90, 23);
            this.btnCancel.TabIndex = 13;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Image = global::EM_UI.Properties.Resources.apply16;
            this.btnOK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOK.Location = new System.Drawing.Point(474, 520);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(90, 23);
            this.btnOK.TabIndex = 12;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lblCountry
            // 
            this.lblCountry.AutoSize = true;
            this.lblCountry.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCountry.ForeColor = System.Drawing.Color.DarkSlateBlue;
            this.lblCountry.Location = new System.Drawing.Point(12, 9);
            this.lblCountry.Name = "lblCountry";
            this.lblCountry.Size = new System.Drawing.Size(71, 20);
            this.lblCountry.TabIndex = 29;
            this.lblCountry.Text = "Country";
            // 
            // dgvConditionalFormatting
            // 
            this.dgvConditionalFormatting.AllowUserToAddRows = false;
            this.dgvConditionalFormatting.AllowUserToDeleteRows = false;
            this.dgvConditionalFormatting.AllowUserToResizeRows = false;
            this.dgvConditionalFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvConditionalFormatting.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dgvConditionalFormatting.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvConditionalFormatting.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvConditionalFormatting.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvConditionalFormatting.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCondition,
            this.colSystemsToApply,
            this.colBackColorConditionalFormatting,
            this.colForeColorConditionalFormatting});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.Thistle;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvConditionalFormatting.DefaultCellStyle = dataGridViewCellStyle2;
            this.dgvConditionalFormatting.GridColor = System.Drawing.SystemColors.Control;
            this.dgvConditionalFormatting.Location = new System.Drawing.Point(16, 52);
            this.dgvConditionalFormatting.MultiSelect = false;
            this.dgvConditionalFormatting.Name = "dgvConditionalFormatting";
            this.dgvConditionalFormatting.RowHeadersVisible = false;
            this.dgvConditionalFormatting.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvConditionalFormatting.ShowCellErrors = false;
            this.dgvConditionalFormatting.ShowCellToolTips = false;
            this.dgvConditionalFormatting.ShowEditingIcon = false;
            this.helpProvider.SetShowHelp(this.dgvConditionalFormatting, false);
            this.dgvConditionalFormatting.ShowRowErrors = false;
            this.dgvConditionalFormatting.Size = new System.Drawing.Size(644, 210);
            this.dgvConditionalFormatting.TabIndex = 32;
            this.dgvConditionalFormatting.TabStop = false;
            this.tipConditionalFormatting.SetToolTip(this.dgvConditionalFormatting, "Cell values which correspond to Condition are highlighted by the indicated Format" +
        "");
            this.dgvConditionalFormatting.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvConditionalFormatting_CellClick);
            this.dgvConditionalFormatting.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvConditionalFormatting_MouseClick);
            // 
            // colCondition
            // 
            this.colCondition.Frozen = true;
            this.colCondition.HeaderText = "Condition";
            this.colCondition.Name = "colCondition";
            this.colCondition.ToolTipText = resources.GetString("colCondition.ToolTipText");
            this.colCondition.Width = 150;
            // 
            // colSystemsToApply
            // 
            this.colSystemsToApply.HeaderText = "Systems to Apply";
            this.colSystemsToApply.Name = "colSystemsToApply";
            this.colSystemsToApply.ReadOnly = true;
            this.colSystemsToApply.ToolTipText = "Click in respective cell to select the systems the formatting is to be applied on" +
    "";
            this.colSystemsToApply.Width = 150;
            // 
            // colBackColorConditionalFormatting
            // 
            this.colBackColorConditionalFormatting.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colBackColorConditionalFormatting.HeaderText = "Back Color";
            this.colBackColorConditionalFormatting.Name = "colBackColorConditionalFormatting";
            this.colBackColorConditionalFormatting.ReadOnly = true;
            this.colBackColorConditionalFormatting.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colBackColorConditionalFormatting.ToolTipText = "Left click in respective cell to select the back color of the formatting; right c" +
    "lick to restore default coloring";
            this.colBackColorConditionalFormatting.Width = 84;
            // 
            // colForeColorConditionalFormatting
            // 
            this.colForeColorConditionalFormatting.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.colForeColorConditionalFormatting.HeaderText = "Text Color";
            this.colForeColorConditionalFormatting.Name = "colForeColorConditionalFormatting";
            this.colForeColorConditionalFormatting.ReadOnly = true;
            this.colForeColorConditionalFormatting.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colForeColorConditionalFormatting.ToolTipText = "Click in respective cell to select the text color of the formatting; right click " +
    "to restore default coloring";
            this.colForeColorConditionalFormatting.Width = 80;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 36);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(111, 13);
            this.label1.TabIndex = 33;
            this.label1.Text = "Conditional Formatting";
            // 
            // dgvBaseSystemFormatting
            // 
            this.dgvBaseSystemFormatting.AllowUserToAddRows = false;
            this.dgvBaseSystemFormatting.AllowUserToDeleteRows = false;
            this.dgvBaseSystemFormatting.AllowUserToResizeRows = false;
            this.dgvBaseSystemFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvBaseSystemFormatting.BackgroundColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBaseSystemFormatting.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.dgvBaseSystemFormatting.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvBaseSystemFormatting.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colSystem,
            this.colBaseSystem,
            this.colBackColorBaseSystemFormatting,
            this.colForeColorBaseSystemFormatting,
            this.colExpandDifferences});
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.Color.Thistle;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dgvBaseSystemFormatting.DefaultCellStyle = dataGridViewCellStyle4;
            this.dgvBaseSystemFormatting.GridColor = System.Drawing.SystemColors.Control;
            this.dgvBaseSystemFormatting.Location = new System.Drawing.Point(16, 292);
            this.dgvBaseSystemFormatting.MultiSelect = false;
            this.dgvBaseSystemFormatting.Name = "dgvBaseSystemFormatting";
            this.dgvBaseSystemFormatting.RowHeadersVisible = false;
            this.dgvBaseSystemFormatting.ShowCellErrors = false;
            this.dgvBaseSystemFormatting.ShowCellToolTips = false;
            this.dgvBaseSystemFormatting.ShowEditingIcon = false;
            this.dgvBaseSystemFormatting.ShowRowErrors = false;
            this.dgvBaseSystemFormatting.Size = new System.Drawing.Size(644, 210);
            this.dgvBaseSystemFormatting.TabIndex = 34;
            this.dgvBaseSystemFormatting.TabStop = false;
            this.tipConditionalFormatting.SetToolTip(this.dgvBaseSystemFormatting, "System\'s differences to Base System are highlighted by the indicated Formatting");
            this.dgvBaseSystemFormatting.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBaseSystemFormatting_CellClick);
            this.dgvBaseSystemFormatting.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvBaseSystemFormatting_CellContentClick);
            this.dgvBaseSystemFormatting.MouseClick += new System.Windows.Forms.MouseEventHandler(this.dgvBaseSystemFormatting_MouseClick);
            // 
            // colSystem
            // 
            this.colSystem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colSystem.Frozen = true;
            this.colSystem.HeaderText = "System";
            this.colSystem.Name = "colSystem";
            this.colSystem.ReadOnly = true;
            this.colSystem.Width = 66;
            // 
            // colBaseSystem
            // 
            this.colBaseSystem.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colBaseSystem.Frozen = true;
            this.colBaseSystem.HeaderText = "Base System";
            this.colBaseSystem.Name = "colBaseSystem";
            this.colBaseSystem.ReadOnly = true;
            this.colBaseSystem.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colBaseSystem.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colBaseSystem.ToolTipText = "Click in respective cell to select the base system";
            this.colBaseSystem.Width = 74;
            // 
            // colBackColorBaseSystemFormatting
            // 
            this.colBackColorBaseSystemFormatting.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colBackColorBaseSystemFormatting.Frozen = true;
            this.colBackColorBaseSystemFormatting.HeaderText = "Back Color";
            this.colBackColorBaseSystemFormatting.Name = "colBackColorBaseSystemFormatting";
            this.colBackColorBaseSystemFormatting.ReadOnly = true;
            this.colBackColorBaseSystemFormatting.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colBackColorBaseSystemFormatting.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colBackColorBaseSystemFormatting.ToolTipText = "Click in respective cell to select the back color of the formatting; right click " +
    "to restore default coloring";
            this.colBackColorBaseSystemFormatting.Width = 65;
            // 
            // colForeColorBaseSystemFormatting
            // 
            this.colForeColorBaseSystemFormatting.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colForeColorBaseSystemFormatting.Frozen = true;
            this.colForeColorBaseSystemFormatting.HeaderText = "Text Color";
            this.colForeColorBaseSystemFormatting.Name = "colForeColorBaseSystemFormatting";
            this.colForeColorBaseSystemFormatting.ReadOnly = true;
            this.colForeColorBaseSystemFormatting.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.colForeColorBaseSystemFormatting.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colForeColorBaseSystemFormatting.ToolTipText = "Click in respective cell to select the text color of the formatting; right click " +
    "to restore default coloring";
            this.colForeColorBaseSystemFormatting.Width = 61;
            // 
            // colExpandDifferences
            // 
            this.colExpandDifferences.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colExpandDifferences.Frozen = true;
            this.colExpandDifferences.HeaderText = "Expand Differences";
            this.colExpandDifferences.Name = "colExpandDifferences";
            this.colExpandDifferences.Width = 106;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 276);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(189, 13);
            this.label2.TabIndex = 35;
            this.label2.Text = "Differences to Base System Formatting";
            // 
            // btnDeleteConditionalFormatting
            // 
            this.btnDeleteConditionalFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteConditionalFormatting.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnDeleteConditionalFormatting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnDeleteConditionalFormatting.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnDeleteConditionalFormatting.Image = global::EM_UI.Properties.Resources.delete16;
            this.btnDeleteConditionalFormatting.Location = new System.Drawing.Point(666, 88);
            this.btnDeleteConditionalFormatting.Name = "btnDeleteConditionalFormatting";
            this.btnDeleteConditionalFormatting.Size = new System.Drawing.Size(30, 30);
            this.btnDeleteConditionalFormatting.TabIndex = 9;
            this.tipConditionalFormatting.SetToolTip(this.btnDeleteConditionalFormatting, "Delete the selected Conditional Formatting");
            this.btnDeleteConditionalFormatting.UseVisualStyleBackColor = false;
            this.btnDeleteConditionalFormatting.Click += new System.EventHandler(this.btnDeleteCondForm_Click);
            // 
            // btnAddConditionalFormatting
            // 
            this.btnAddConditionalFormatting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddConditionalFormatting.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAddConditionalFormatting.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddConditionalFormatting.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAddConditionalFormatting.Image = ((System.Drawing.Image)(resources.GetObject("btnAddConditionalFormatting.Image")));
            this.btnAddConditionalFormatting.Location = new System.Drawing.Point(666, 52);
            this.btnAddConditionalFormatting.Name = "btnAddConditionalFormatting";
            this.btnAddConditionalFormatting.Size = new System.Drawing.Size(30, 30);
            this.btnAddConditionalFormatting.TabIndex = 8;
            this.tipConditionalFormatting.SetToolTip(this.btnAddConditionalFormatting, "Add a new Conditional Formatting");
            this.btnAddConditionalFormatting.UseVisualStyleBackColor = false;
            this.btnAddConditionalFormatting.Click += new System.EventHandler(this.btnAddCondForm_Click);
            // 
            // dlgColor
            // 
            this.dlgColor.AnyColor = true;
            this.dlgColor.Color = System.Drawing.Color.Transparent;
            this.dlgColor.FullOpen = true;
            this.dlgColor.SolidColorOnly = true;
            // 
            // btnBaseSystemFormattingSetDefault
            // 
            this.btnBaseSystemFormattingSetDefault.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnBaseSystemFormattingSetDefault.Location = new System.Drawing.Point(16, 520);
            this.btnBaseSystemFormattingSetDefault.Name = "btnBaseSystemFormattingSetDefault";
            this.btnBaseSystemFormattingSetDefault.Size = new System.Drawing.Size(152, 23);
            this.btnBaseSystemFormattingSetDefault.TabIndex = 39;
            this.btnBaseSystemFormattingSetDefault.Text = "Restore &Default Back Color";
            this.tipConditionalFormatting.SetToolTip(this.btnBaseSystemFormattingSetDefault, "Restore default formatting for all system/base system pairs");
            this.btnBaseSystemFormattingSetDefault.UseVisualStyleBackColor = true;
            this.btnBaseSystemFormattingSetDefault.Click += new System.EventHandler(this.btnSetDefaultSysDif_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // menuClearColor
            // 
            this.menuClearColor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.menuClearColor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.itemClearColor});
            this.menuClearColor.Name = "contextMenuStrip1";
            this.menuClearColor.Size = new System.Drawing.Size(134, 26);
            this.menuClearColor.Text = "Clear Color";
            this.menuClearColor.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.menuClearColor_ItemClicked);
            // 
            // itemClearColor
            // 
            this.itemClearColor.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.itemClearColor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.itemClearColor.Name = "itemClearColor";
            this.itemClearColor.Size = new System.Drawing.Size(133, 22);
            this.itemClearColor.Text = "Clear Color";
            // 
            // ConditionalFormattingForm
            // 
            this.AcceptButton = this.btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(708, 575);
            this.Controls.Add(this.btnDeleteConditionalFormatting);
            this.Controls.Add(this.btnBaseSystemFormattingSetDefault);
            this.Controls.Add(this.btnAddConditionalFormatting);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dgvBaseSystemFormatting);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgvConditionalFormatting);
            this.Controls.Add(this.lblCountry);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_ConditionalFormatting.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConditionalFormattingForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Conditional Formatting";
            this.Load += new System.EventHandler(this.ConditionalFormattingForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvConditionalFormatting)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvBaseSystemFormatting)).EndInit();
            this.menuClearColor.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lblCountry;
        private System.Windows.Forms.DataGridView dgvConditionalFormatting;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView dgvBaseSystemFormatting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnDeleteConditionalFormatting;
        private System.Windows.Forms.Button btnAddConditionalFormatting;
        private System.Windows.Forms.ColorDialog dlgColor;
        private System.Windows.Forms.ToolTip tipConditionalFormatting;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.Button btnBaseSystemFormattingSetDefault;
        private System.Windows.Forms.ContextMenuStrip menuClearColor;
        private System.Windows.Forms.ToolStripMenuItem itemClearColor;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCondition;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSystemsToApply;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBackColorConditionalFormatting;
        private System.Windows.Forms.DataGridViewTextBoxColumn colForeColorConditionalFormatting;
        private System.Windows.Forms.DataGridViewTextBoxColumn colSystem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBaseSystem;
        private System.Windows.Forms.DataGridViewTextBoxColumn colBackColorBaseSystemFormatting;
        private System.Windows.Forms.DataGridViewTextBoxColumn colForeColorBaseSystemFormatting;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colExpandDifferences;
    }
}