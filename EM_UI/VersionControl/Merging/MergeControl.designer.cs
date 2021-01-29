namespace EM_UI.VersionControl.Merging
{
    partial class MergeControl
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
            this.components = new System.ComponentModel.Container();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.treeLocal = new DevExpress.XtraTreeList.TreeList();
            this.localToolTipController = new DevExpress.Utils.ToolTipController(this.components);
            this.labLocal = new System.Windows.Forms.Label();
            this.treeRemote = new DevExpress.XtraTreeList.TreeList();
            this.labRemote = new System.Windows.Forms.Label();
            this.grpRemoteLocal = new System.Windows.Forms.GroupBox();
            this.radRemoteOnly = new System.Windows.Forms.RadioButton();
            this.radLocalOnly = new System.Windows.Forms.RadioButton();
            this.btnLocalAndRemote = new System.Windows.Forms.RadioButton();
            this.grpFilter = new System.Windows.Forms.GroupBox();
            this.btnApplyFilter = new System.Windows.Forms.Button();
            this.labLevel = new System.Windows.Forms.Label();
            this.cmbLevel = new System.Windows.Forms.ComboBox();
            this.btnReleaseFilter = new System.Windows.Forms.Button();
            this.grpType = new System.Windows.Forms.GroupBox();
            this.btnShowOrder = new System.Windows.Forms.Button();
            this.chkReordered = new System.Windows.Forms.CheckBox();
            this.chkAllValues = new System.Windows.Forms.CheckBox();
            this.chkAllSettings = new System.Windows.Forms.CheckBox();
            this.chkChanged = new System.Windows.Forms.CheckBox();
            this.chkRemoved = new System.Windows.Forms.CheckBox();
            this.lstValues = new System.Windows.Forms.CheckedListBox();
            this.chkAdded = new System.Windows.Forms.CheckBox();
            this.lstSettings = new System.Windows.Forms.CheckedListBox();
            this.btnAcceptAll = new System.Windows.Forms.Button();
            this.btnRejectAll = new System.Windows.Forms.Button();
            this.chkVisibleOnly = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radAccRejAllRemote = new System.Windows.Forms.RadioButton();
            this.radAccRejAllLocal = new System.Windows.Forms.RadioButton();
            this.grpConflicts = new System.Windows.Forms.GroupBox();
            this.radConflictWarn = new System.Windows.Forms.RadioButton();
            this.radConflictPreferRemote = new System.Windows.Forms.RadioButton();
            this.radConflictPreferLocal = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.treeLocal)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeRemote)).BeginInit();
            this.grpRemoteLocal.SuspendLayout();
            this.grpFilter.SuspendLayout();
            this.grpType.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.grpConflicts.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer
            // 
            this.splitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer.Location = new System.Drawing.Point(7, 2);
            this.splitContainer.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.treeLocal);
            this.splitContainer.Panel1.Controls.Add(this.labLocal);
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.treeRemote);
            this.splitContainer.Panel2.Controls.Add(this.labRemote);
            this.splitContainer.Size = new System.Drawing.Size(748, 45);
            this.splitContainer.SplitterDistance = 335;
            this.splitContainer.SplitterWidth = 1;
            this.splitContainer.TabIndex = 3;
            // 
            // treeLocal
            // 
            this.treeLocal.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeLocal.Location = new System.Drawing.Point(2, 16);
            this.treeLocal.Margin = new System.Windows.Forms.Padding(2);
            this.treeLocal.Name = "treeLocal";
            this.treeLocal.OptionsBehavior.Editable = false;
            this.treeLocal.OptionsCustomization.AllowQuickHideColumns = false;
            this.treeLocal.OptionsView.AutoWidth = false;
            this.treeLocal.Size = new System.Drawing.Size(331, 26);
            this.treeLocal.TabIndex = 0;
            this.treeLocal.ToolTipController = this.localToolTipController;
            this.treeLocal.ColumnWidthChanged += new DevExpress.XtraTreeList.ColumnWidthChangedEventHandler(this.treeLocal_ColumnWidthChanged);
            this.treeLocal.NodeCellStyle += new DevExpress.XtraTreeList.GetCustomNodeCellStyleEventHandler(this.treeLocal_NodeCellStyle);
            this.treeLocal.AfterExpand += new DevExpress.XtraTreeList.NodeEventHandler(this.tree_AfterCollapseExpand);
            this.treeLocal.AfterCollapse += new DevExpress.XtraTreeList.NodeEventHandler(this.tree_AfterCollapseExpand);
            this.treeLocal.CustomDrawNodeIndicator += new DevExpress.XtraTreeList.CustomDrawNodeIndicatorEventHandler(this.treeLocal_CustomDrawNodeIndicator);
            this.treeLocal.CustomDrawColumnHeader += new DevExpress.XtraTreeList.CustomDrawColumnHeaderEventHandler(this.treeLocal_CustomDrawColumnHeader);
            this.treeLocal.PopupMenuShowing += new DevExpress.XtraTreeList.PopupMenuShowingEventHandler(this.tree_PopupMenuShowing);
            this.treeLocal.LeftCoordChanged += new System.EventHandler(this.treeLocal_LeftCoordChanged);
            this.treeLocal.TopVisibleNodeIndexChanged += new System.EventHandler(this.tree_TopVisibleNodeIndexChanged);
            this.treeLocal.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tree_MouseUp);
            // 
            // localToolTipController
            // 
            this.localToolTipController.AutoPopDelay = 15000;
            this.localToolTipController.InitialDelay = 100;
            this.localToolTipController.GetActiveObjectInfo += new DevExpress.Utils.ToolTipControllerGetActiveObjectInfoEventHandler(this.localToolTipController_GetActiveObjectInfo);
            // 
            // labLocal
            // 
            this.labLocal.AutoSize = true;
            this.labLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labLocal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labLocal.Location = new System.Drawing.Point(2, 0);
            this.labLocal.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labLocal.Name = "labLocal";
            this.labLocal.Size = new System.Drawing.Size(38, 13);
            this.labLocal.TabIndex = 4;
            this.labLocal.Text = "Local";
            // 
            // treeRemote
            // 
            this.treeRemote.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeRemote.Location = new System.Drawing.Point(2, 16);
            this.treeRemote.Margin = new System.Windows.Forms.Padding(2);
            this.treeRemote.Name = "treeRemote";
            this.treeRemote.OptionsBehavior.Editable = false;
            this.treeRemote.OptionsCustomization.AllowQuickHideColumns = false;
            this.treeRemote.OptionsSelection.MultiSelect = true;
            this.treeRemote.OptionsView.AutoWidth = false;
            this.treeRemote.Size = new System.Drawing.Size(412, 26);
            this.treeRemote.TabIndex = 5;
            this.treeRemote.ColumnWidthChanged += new DevExpress.XtraTreeList.ColumnWidthChangedEventHandler(this.treeRemote_ColumnWidthChanged);
            this.treeRemote.NodeCellStyle += new DevExpress.XtraTreeList.GetCustomNodeCellStyleEventHandler(this.treeLocal_NodeCellStyle);
            this.treeRemote.AfterExpand += new DevExpress.XtraTreeList.NodeEventHandler(this.tree_AfterCollapseExpand);
            this.treeRemote.AfterCollapse += new DevExpress.XtraTreeList.NodeEventHandler(this.tree_AfterCollapseExpand);
            this.treeRemote.PopupMenuShowing += new DevExpress.XtraTreeList.PopupMenuShowingEventHandler(this.tree_PopupMenuShowing);
            this.treeRemote.LeftCoordChanged += new System.EventHandler(this.treeRemote_LeftCoordChanged);
            this.treeRemote.TopVisibleNodeIndexChanged += new System.EventHandler(this.tree_TopVisibleNodeIndexChanged);
            this.treeRemote.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tree_MouseUp);
            // 
            // labRemote
            // 
            this.labRemote.AutoSize = true;
            this.labRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labRemote.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labRemote.Location = new System.Drawing.Point(2, 0);
            this.labRemote.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labRemote.Name = "labRemote";
            this.labRemote.Size = new System.Drawing.Size(50, 13);
            this.labRemote.TabIndex = 5;
            this.labRemote.Text = "Remote";
            // 
            // grpRemoteLocal
            // 
            this.grpRemoteLocal.Controls.Add(this.radRemoteOnly);
            this.grpRemoteLocal.Controls.Add(this.radLocalOnly);
            this.grpRemoteLocal.Controls.Add(this.btnLocalAndRemote);
            this.grpRemoteLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpRemoteLocal.ForeColor = System.Drawing.SystemColors.ControlText;
            this.grpRemoteLocal.Location = new System.Drawing.Point(4, 17);
            this.grpRemoteLocal.Margin = new System.Windows.Forms.Padding(2);
            this.grpRemoteLocal.Name = "grpRemoteLocal";
            this.grpRemoteLocal.Padding = new System.Windows.Forms.Padding(2);
            this.grpRemoteLocal.Size = new System.Drawing.Size(114, 128);
            this.grpRemoteLocal.TabIndex = 6;
            this.grpRemoteLocal.TabStop = false;
            this.grpRemoteLocal.Text = "Remote / Local";
            // 
            // radRemoteOnly
            // 
            this.radRemoteOnly.AutoSize = true;
            this.radRemoteOnly.Location = new System.Drawing.Point(4, 76);
            this.radRemoteOnly.Margin = new System.Windows.Forms.Padding(2);
            this.radRemoteOnly.Name = "radRemoteOnly";
            this.radRemoteOnly.Size = new System.Drawing.Size(84, 17);
            this.radRemoteOnly.TabIndex = 9;
            this.radRemoteOnly.Text = "Remote only";
            this.radRemoteOnly.UseVisualStyleBackColor = true;
            // 
            // radLocalOnly
            // 
            this.radLocalOnly.AutoSize = true;
            this.radLocalOnly.Location = new System.Drawing.Point(4, 50);
            this.radLocalOnly.Margin = new System.Windows.Forms.Padding(2);
            this.radLocalOnly.Name = "radLocalOnly";
            this.radLocalOnly.Size = new System.Drawing.Size(73, 17);
            this.radLocalOnly.TabIndex = 8;
            this.radLocalOnly.Text = "Local only";
            this.radLocalOnly.UseVisualStyleBackColor = true;
            // 
            // btnLocalAndRemote
            // 
            this.btnLocalAndRemote.AutoSize = true;
            this.btnLocalAndRemote.Checked = true;
            this.btnLocalAndRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLocalAndRemote.Location = new System.Drawing.Point(4, 25);
            this.btnLocalAndRemote.Margin = new System.Windows.Forms.Padding(2);
            this.btnLocalAndRemote.Name = "btnLocalAndRemote";
            this.btnLocalAndRemote.Size = new System.Drawing.Size(112, 17);
            this.btnLocalAndRemote.TabIndex = 7;
            this.btnLocalAndRemote.TabStop = true;
            this.btnLocalAndRemote.Text = "Local and Remote";
            this.btnLocalAndRemote.UseVisualStyleBackColor = true;
            // 
            // grpFilter
            // 
            this.grpFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.grpFilter.Controls.Add(this.btnApplyFilter);
            this.grpFilter.Controls.Add(this.labLevel);
            this.grpFilter.Controls.Add(this.cmbLevel);
            this.grpFilter.Controls.Add(this.btnReleaseFilter);
            this.grpFilter.Controls.Add(this.grpType);
            this.grpFilter.Controls.Add(this.grpRemoteLocal);
            this.grpFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpFilter.ForeColor = System.Drawing.SystemColors.ControlText;
            this.grpFilter.Location = new System.Drawing.Point(7, 46);
            this.grpFilter.Margin = new System.Windows.Forms.Padding(2);
            this.grpFilter.Name = "grpFilter";
            this.grpFilter.Padding = new System.Windows.Forms.Padding(2);
            this.grpFilter.Size = new System.Drawing.Size(526, 150);
            this.grpFilter.TabIndex = 7;
            this.grpFilter.TabStop = false;
            this.grpFilter.Text = "Filter Changes";
            // 
            // btnApplyFilter
            // 
            this.btnApplyFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnApplyFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnApplyFilter.Location = new System.Drawing.Point(429, 68);
            this.btnApplyFilter.Margin = new System.Windows.Forms.Padding(2);
            this.btnApplyFilter.Name = "btnApplyFilter";
            this.btnApplyFilter.Size = new System.Drawing.Size(85, 37);
            this.btnApplyFilter.TabIndex = 11;
            this.btnApplyFilter.Text = "Apply / Update Filter";
            this.btnApplyFilter.UseVisualStyleBackColor = true;
            this.btnApplyFilter.Click += new System.EventHandler(this.btnApplyFilter_Click);
            // 
            // labLevel
            // 
            this.labLevel.AutoSize = true;
            this.labLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labLevel.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labLevel.Location = new System.Drawing.Point(429, 18);
            this.labLevel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labLevel.Name = "labLevel";
            this.labLevel.Size = new System.Drawing.Size(33, 13);
            this.labLevel.TabIndex = 10;
            this.labLevel.Text = "Level";
            // 
            // cmbLevel
            // 
            this.cmbLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbLevel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cmbLevel.FormattingEnabled = true;
            this.cmbLevel.Location = new System.Drawing.Point(429, 35);
            this.cmbLevel.Margin = new System.Windows.Forms.Padding(2);
            this.cmbLevel.Name = "cmbLevel";
            this.cmbLevel.Size = new System.Drawing.Size(86, 21);
            this.cmbLevel.TabIndex = 9;
            // 
            // btnReleaseFilter
            // 
            this.btnReleaseFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReleaseFilter.Enabled = false;
            this.btnReleaseFilter.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnReleaseFilter.Location = new System.Drawing.Point(429, 108);
            this.btnReleaseFilter.Margin = new System.Windows.Forms.Padding(2);
            this.btnReleaseFilter.Name = "btnReleaseFilter";
            this.btnReleaseFilter.Size = new System.Drawing.Size(85, 37);
            this.btnReleaseFilter.TabIndex = 9;
            this.btnReleaseFilter.Text = "Release Filter";
            this.btnReleaseFilter.UseVisualStyleBackColor = true;
            this.btnReleaseFilter.Click += new System.EventHandler(this.btnReleaseFilter_Click);
            // 
            // grpType
            // 
            this.grpType.Controls.Add(this.btnShowOrder);
            this.grpType.Controls.Add(this.chkReordered);
            this.grpType.Controls.Add(this.chkAllValues);
            this.grpType.Controls.Add(this.chkAllSettings);
            this.grpType.Controls.Add(this.chkChanged);
            this.grpType.Controls.Add(this.chkRemoved);
            this.grpType.Controls.Add(this.lstValues);
            this.grpType.Controls.Add(this.chkAdded);
            this.grpType.Controls.Add(this.lstSettings);
            this.grpType.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpType.ForeColor = System.Drawing.SystemColors.ControlText;
            this.grpType.Location = new System.Drawing.Point(123, 17);
            this.grpType.Margin = new System.Windows.Forms.Padding(2);
            this.grpType.Name = "grpType";
            this.grpType.Padding = new System.Windows.Forms.Padding(2);
            this.grpType.Size = new System.Drawing.Size(299, 128);
            this.grpType.TabIndex = 7;
            this.grpType.TabStop = false;
            this.grpType.Text = "Type";
            // 
            // btnShowOrder
            // 
            this.btnShowOrder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnShowOrder.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnShowOrder.Location = new System.Drawing.Point(4, 99);
            this.btnShowOrder.Margin = new System.Windows.Forms.Padding(2);
            this.btnShowOrder.Name = "btnShowOrder";
            this.btnShowOrder.Size = new System.Drawing.Size(74, 21);
            this.btnShowOrder.TabIndex = 12;
            this.btnShowOrder.Text = "Show Order";
            this.btnShowOrder.UseVisualStyleBackColor = true;
            this.btnShowOrder.Visible = false;
            this.btnShowOrder.Click += new System.EventHandler(this.btnShowOrder_Click);
            // 
            // chkReordered
            // 
            this.chkReordered.AutoSize = true;
            this.chkReordered.Checked = true;
            this.chkReordered.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReordered.Location = new System.Drawing.Point(4, 76);
            this.chkReordered.Margin = new System.Windows.Forms.Padding(2);
            this.chkReordered.Name = "chkReordered";
            this.chkReordered.Size = new System.Drawing.Size(76, 17);
            this.chkReordered.TabIndex = 13;
            this.chkReordered.Text = "Reordered";
            this.chkReordered.UseVisualStyleBackColor = true;
            this.chkReordered.Visible = false;
            // 
            // chkAllValues
            // 
            this.chkAllValues.AutoSize = true;
            this.chkAllValues.Checked = true;
            this.chkAllValues.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllValues.Location = new System.Drawing.Point(196, 25);
            this.chkAllValues.Margin = new System.Windows.Forms.Padding(2);
            this.chkAllValues.Name = "chkAllValues";
            this.chkAllValues.Size = new System.Drawing.Size(72, 17);
            this.chkAllValues.TabIndex = 12;
            this.chkAllValues.Text = "All Values";
            this.chkAllValues.UseVisualStyleBackColor = true;
            this.chkAllValues.CheckedChanged += new System.EventHandler(this.chkAllValues_CheckedChanged);
            // 
            // chkAllSettings
            // 
            this.chkAllSettings.AutoSize = true;
            this.chkAllSettings.Checked = true;
            this.chkAllSettings.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllSettings.Location = new System.Drawing.Point(96, 25);
            this.chkAllSettings.Margin = new System.Windows.Forms.Padding(2);
            this.chkAllSettings.Name = "chkAllSettings";
            this.chkAllSettings.Size = new System.Drawing.Size(78, 17);
            this.chkAllSettings.TabIndex = 11;
            this.chkAllSettings.Text = "All Settings";
            this.chkAllSettings.UseVisualStyleBackColor = true;
            this.chkAllSettings.CheckedChanged += new System.EventHandler(this.chkAllSettings_CheckedChanged);
            // 
            // chkChanged
            // 
            this.chkChanged.AutoSize = true;
            this.chkChanged.Checked = true;
            this.chkChanged.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkChanged.Location = new System.Drawing.Point(96, 9);
            this.chkChanged.Margin = new System.Windows.Forms.Padding(2);
            this.chkChanged.Name = "chkChanged";
            this.chkChanged.Size = new System.Drawing.Size(72, 17);
            this.chkChanged.TabIndex = 10;
            this.chkChanged.Text = "Changed:";
            this.chkChanged.UseVisualStyleBackColor = true;
            // 
            // chkRemoved
            // 
            this.chkRemoved.AutoSize = true;
            this.chkRemoved.Checked = true;
            this.chkRemoved.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemoved.Location = new System.Drawing.Point(4, 51);
            this.chkRemoved.Margin = new System.Windows.Forms.Padding(2);
            this.chkRemoved.Name = "chkRemoved";
            this.chkRemoved.Size = new System.Drawing.Size(72, 17);
            this.chkRemoved.TabIndex = 9;
            this.chkRemoved.Text = "Removed";
            this.chkRemoved.UseVisualStyleBackColor = true;
            // 
            // lstValues
            // 
            this.lstValues.BackColor = System.Drawing.SystemColors.Control;
            this.lstValues.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstValues.CheckOnClick = true;
            this.lstValues.Enabled = false;
            this.lstValues.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstValues.FormattingEnabled = true;
            this.lstValues.Location = new System.Drawing.Point(195, 43);
            this.lstValues.Margin = new System.Windows.Forms.Padding(2);
            this.lstValues.Name = "lstValues";
            this.lstValues.Size = new System.Drawing.Size(96, 70);
            this.lstValues.TabIndex = 8;
            // 
            // chkAdded
            // 
            this.chkAdded.AutoSize = true;
            this.chkAdded.Checked = true;
            this.chkAdded.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAdded.Location = new System.Drawing.Point(4, 26);
            this.chkAdded.Margin = new System.Windows.Forms.Padding(2);
            this.chkAdded.Name = "chkAdded";
            this.chkAdded.Size = new System.Drawing.Size(57, 17);
            this.chkAdded.TabIndex = 8;
            this.chkAdded.Text = "Added";
            this.chkAdded.UseVisualStyleBackColor = true;
            // 
            // lstSettings
            // 
            this.lstSettings.BackColor = System.Drawing.SystemColors.Control;
            this.lstSettings.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstSettings.CheckOnClick = true;
            this.lstSettings.Enabled = false;
            this.lstSettings.FormattingEnabled = true;
            this.lstSettings.Location = new System.Drawing.Point(95, 43);
            this.lstSettings.Margin = new System.Windows.Forms.Padding(2);
            this.lstSettings.Name = "lstSettings";
            this.lstSettings.Size = new System.Drawing.Size(96, 70);
            this.lstSettings.TabIndex = 0;
            // 
            // btnAcceptAll
            // 
            this.btnAcceptAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnAcceptAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAcceptAll.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnAcceptAll.Location = new System.Drawing.Point(7, 17);
            this.btnAcceptAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnAcceptAll.Name = "btnAcceptAll";
            this.btnAcceptAll.Size = new System.Drawing.Size(75, 37);
            this.btnAcceptAll.TabIndex = 10;
            this.btnAcceptAll.Text = "Accept All";
            this.btnAcceptAll.UseVisualStyleBackColor = true;
            this.btnAcceptAll.Click += new System.EventHandler(this.btnAcceptAll_Click);
            // 
            // btnRejectAll
            // 
            this.btnRejectAll.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnRejectAll.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRejectAll.ForeColor = System.Drawing.SystemColors.ControlText;
            this.btnRejectAll.Location = new System.Drawing.Point(86, 17);
            this.btnRejectAll.Margin = new System.Windows.Forms.Padding(2);
            this.btnRejectAll.Name = "btnRejectAll";
            this.btnRejectAll.Size = new System.Drawing.Size(75, 37);
            this.btnRejectAll.TabIndex = 11;
            this.btnRejectAll.Text = "Reject All";
            this.btnRejectAll.UseVisualStyleBackColor = true;
            this.btnRejectAll.Click += new System.EventHandler(this.btnRejectAll_Click);
            // 
            // chkVisibleOnly
            // 
            this.chkVisibleOnly.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.chkVisibleOnly.AutoSize = true;
            this.chkVisibleOnly.Checked = true;
            this.chkVisibleOnly.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkVisibleOnly.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkVisibleOnly.ForeColor = System.Drawing.SystemColors.ControlText;
            this.chkVisibleOnly.Location = new System.Drawing.Point(7, 59);
            this.chkVisibleOnly.Margin = new System.Windows.Forms.Padding(2);
            this.chkVisibleOnly.Name = "chkVisibleOnly";
            this.chkVisibleOnly.Size = new System.Drawing.Size(80, 17);
            this.chkVisibleOnly.TabIndex = 12;
            this.chkVisibleOnly.Text = "Visible Only";
            this.chkVisibleOnly.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.radAccRejAllRemote);
            this.groupBox1.Controls.Add(this.radAccRejAllLocal);
            this.groupBox1.Controls.Add(this.chkVisibleOnly);
            this.groupBox1.Controls.Add(this.btnAcceptAll);
            this.groupBox1.Controls.Add(this.btnRejectAll);
            this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(582, 46);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(170, 103);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Accept/Reject";
            // 
            // radAccRejAllRemote
            // 
            this.radAccRejAllRemote.AutoSize = true;
            this.radAccRejAllRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radAccRejAllRemote.Location = new System.Drawing.Point(100, 82);
            this.radAccRejAllRemote.Margin = new System.Windows.Forms.Padding(2);
            this.radAccRejAllRemote.Name = "radAccRejAllRemote";
            this.radAccRejAllRemote.Size = new System.Drawing.Size(62, 17);
            this.radAccRejAllRemote.TabIndex = 14;
            this.radAccRejAllRemote.Text = "Remote";
            this.radAccRejAllRemote.UseVisualStyleBackColor = true;
            // 
            // radAccRejAllLocal
            // 
            this.radAccRejAllLocal.AutoSize = true;
            this.radAccRejAllLocal.Checked = true;
            this.radAccRejAllLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radAccRejAllLocal.Location = new System.Drawing.Point(100, 60);
            this.radAccRejAllLocal.Margin = new System.Windows.Forms.Padding(2);
            this.radAccRejAllLocal.Name = "radAccRejAllLocal";
            this.radAccRejAllLocal.Size = new System.Drawing.Size(51, 17);
            this.radAccRejAllLocal.TabIndex = 13;
            this.radAccRejAllLocal.TabStop = true;
            this.radAccRejAllLocal.Text = "Local";
            this.radAccRejAllLocal.UseVisualStyleBackColor = true;
            // 
            // grpConflicts
            // 
            this.grpConflicts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConflicts.Controls.Add(this.radConflictWarn);
            this.grpConflicts.Controls.Add(this.radConflictPreferRemote);
            this.grpConflicts.Controls.Add(this.radConflictPreferLocal);
            this.grpConflicts.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.grpConflicts.Location = new System.Drawing.Point(582, 154);
            this.grpConflicts.Margin = new System.Windows.Forms.Padding(2);
            this.grpConflicts.Name = "grpConflicts";
            this.grpConflicts.Padding = new System.Windows.Forms.Padding(2);
            this.grpConflicts.Size = new System.Drawing.Size(170, 42);
            this.grpConflicts.TabIndex = 15;
            this.grpConflicts.TabStop = false;
            this.grpConflicts.Text = "Conflicts - Prefer ...";
            // 
            // radConflictWarn
            // 
            this.radConflictWarn.AutoSize = true;
            this.radConflictWarn.Checked = true;
            this.radConflictWarn.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radConflictWarn.Location = new System.Drawing.Point(122, 17);
            this.radConflictWarn.Margin = new System.Windows.Forms.Padding(2);
            this.radConflictWarn.Name = "radConflictWarn";
            this.radConflictWarn.Size = new System.Drawing.Size(51, 17);
            this.radConflictWarn.TabIndex = 16;
            this.radConflictWarn.TabStop = true;
            this.radConflictWarn.Text = "Warn";
            this.radConflictWarn.UseVisualStyleBackColor = true;
            // 
            // radConflictPreferRemote
            // 
            this.radConflictPreferRemote.AutoSize = true;
            this.radConflictPreferRemote.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radConflictPreferRemote.Location = new System.Drawing.Point(58, 17);
            this.radConflictPreferRemote.Margin = new System.Windows.Forms.Padding(2);
            this.radConflictPreferRemote.Name = "radConflictPreferRemote";
            this.radConflictPreferRemote.Size = new System.Drawing.Size(62, 17);
            this.radConflictPreferRemote.TabIndex = 15;
            this.radConflictPreferRemote.Text = "Remote";
            this.radConflictPreferRemote.UseVisualStyleBackColor = true;
            // 
            // radConflictPreferLocal
            // 
            this.radConflictPreferLocal.AutoSize = true;
            this.radConflictPreferLocal.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.radConflictPreferLocal.Location = new System.Drawing.Point(7, 17);
            this.radConflictPreferLocal.Margin = new System.Windows.Forms.Padding(2);
            this.radConflictPreferLocal.Name = "radConflictPreferLocal";
            this.radConflictPreferLocal.Size = new System.Drawing.Size(51, 17);
            this.radConflictPreferLocal.TabIndex = 14;
            this.radConflictPreferLocal.Text = "Local";
            this.radConflictPreferLocal.UseVisualStyleBackColor = true;
            // 
            // MergeControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.grpConflicts);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.grpFilter);
            this.Controls.Add(this.splitContainer);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "MergeControl";
            this.Size = new System.Drawing.Size(755, 199);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel1.PerformLayout();
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.treeLocal)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.treeRemote)).EndInit();
            this.grpRemoteLocal.ResumeLayout(false);
            this.grpRemoteLocal.PerformLayout();
            this.grpFilter.ResumeLayout(false);
            this.grpFilter.PerformLayout();
            this.grpType.ResumeLayout(false);
            this.grpType.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpConflicts.ResumeLayout(false);
            this.grpConflicts.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private DevExpress.XtraTreeList.TreeList treeLocal;
        private System.Windows.Forms.Label labLocal;
        private System.Windows.Forms.Label labRemote;
        private System.Windows.Forms.GroupBox grpRemoteLocal;
        private System.Windows.Forms.RadioButton radRemoteOnly;
        private System.Windows.Forms.RadioButton radLocalOnly;
        private System.Windows.Forms.RadioButton btnLocalAndRemote;
        private System.Windows.Forms.GroupBox grpFilter;
        private System.Windows.Forms.GroupBox grpType;
        private System.Windows.Forms.CheckedListBox lstSettings;
        private System.Windows.Forms.Label labLevel;
        private System.Windows.Forms.ComboBox cmbLevel;
        private System.Windows.Forms.CheckedListBox lstValues;
        private System.Windows.Forms.CheckBox chkChanged;
        private System.Windows.Forms.CheckBox chkRemoved;
        private System.Windows.Forms.CheckBox chkAdded;
        private System.Windows.Forms.Button btnReleaseFilter;
        private System.Windows.Forms.Button btnAcceptAll;
        private System.Windows.Forms.Button btnRejectAll;
        private System.Windows.Forms.CheckBox chkVisibleOnly;
        private DevExpress.XtraTreeList.TreeList treeRemote;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkAllSettings;
        private System.Windows.Forms.CheckBox chkAllValues;
        private System.Windows.Forms.Button btnApplyFilter;
        private System.Windows.Forms.RadioButton radAccRejAllRemote;
        private System.Windows.Forms.RadioButton radAccRejAllLocal;
        private System.Windows.Forms.GroupBox grpConflicts;
        private System.Windows.Forms.RadioButton radConflictWarn;
        private System.Windows.Forms.RadioButton radConflictPreferRemote;
        private System.Windows.Forms.RadioButton radConflictPreferLocal;
        private System.Windows.Forms.CheckBox chkReordered;
        private System.Windows.Forms.Button btnShowOrder;
        private DevExpress.Utils.ToolTipController localToolTipController;
    }
}
