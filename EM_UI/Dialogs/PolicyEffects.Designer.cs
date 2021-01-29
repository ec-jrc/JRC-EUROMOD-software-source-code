using System.Drawing;
using System.Windows.Forms;
namespace EM_UI.Dialogs
{
    partial class PolicyEffects
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolicyEffects));
            this.btnRunPolicyEffects = new System.Windows.Forms.Button();
            this.labelYear1 = new System.Windows.Forms.Label();
            this.labelYear2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.comboBox2 = new System.Windows.Forms.ComboBox();
            this.textBoxAlpha = new System.Windows.Forms.TextBox();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.gridColumnCheck = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemCheckEditCountry = new DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit();
            this.gridColumnCountry = new DevExpress.XtraGrid.Columns.GridColumn();
            this.gridColumnSys1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBoxSys1 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumnData1 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBoxData = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumnSys2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.repositoryItemComboBoxSys2 = new DevExpress.XtraEditors.Repository.RepositoryItemComboBox();
            this.gridColumnData2 = new DevExpress.XtraGrid.Columns.GridColumn();
            this.lblInfo = new System.Windows.Forms.Label();
            this.labelDecomposition = new DevExpress.XtraEditors.LabelControl();
            this.buttonClose = new System.Windows.Forms.Button();
            this.textBoxOutputPath = new System.Windows.Forms.TextBox();
            this.buttonChangePath = new DevExpress.XtraEditors.SimpleButton();
            this.panelResults = new System.Windows.Forms.Panel();
            this.fileTabSelector = new System.Windows.Forms.TabControl();
            this.btnRunOnly = new System.Windows.Forms.Button();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.chkEM2 = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelIndex = new DevExpress.XtraEditors.LabelControl();
            this.groupBoxData = new DevExpress.XtraEditors.PanelControl();
            this.checkRadioDataBoth = new DevExpress.XtraEditors.CheckEdit();
            this.checkRadioData2 = new DevExpress.XtraEditors.CheckEdit();
            this.checkRadioData1 = new DevExpress.XtraEditors.CheckEdit();
            this.groupBoxAlpha = new DevExpress.XtraEditors.PanelControl();
            this.checkBoxAlphaFIX = new DevExpress.XtraEditors.CheckEdit();
            this.checkBoxAlphaCPI = new DevExpress.XtraEditors.CheckEdit();
            this.checkBoxAlphaMII = new DevExpress.XtraEditors.CheckEdit();
            this.btnAlphaRange = new System.Windows.Forms.Button();
            this.labelAlpha = new DevExpress.XtraEditors.LabelControl();
            this.groupBoxIndex = new DevExpress.XtraEditors.PanelControl();
            this.checkRadioMarketUnscaled = new DevExpress.XtraEditors.CheckEdit();
            this.checkRadioMarket = new DevExpress.XtraEditors.CheckEdit();
            this.checkRadioMonetary = new DevExpress.XtraEditors.CheckEdit();
            this.checkAllCountries = new DevExpress.XtraEditors.CheckEdit();
            this.chkTreatAsMarket = new System.Windows.Forms.CheckBox();
            this.lblAddons = new System.Windows.Forms.Label();
            this.chkListAddons = new System.Windows.Forms.CheckedListBox();
            this.labelRunFirstNHH = new System.Windows.Forms.Label();
            this.textRunFirstNHH = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEditCountry)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxSys1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxSys2)).BeginInit();
            this.panelResults.SuspendLayout();
            this.panelBottom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxData)).BeginInit();
            this.groupBoxData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioDataBoth.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioData2.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioData1.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxAlpha)).BeginInit();
            this.groupBoxAlpha.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAlphaFIX.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAlphaCPI.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAlphaMII.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxIndex)).BeginInit();
            this.groupBoxIndex.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioMarketUnscaled.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioMarket.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioMonetary.Properties)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkAllCountries.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // btnRunPolicyEffects
            // 
            this.btnRunPolicyEffects.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunPolicyEffects.Location = new System.Drawing.Point(443, 12);
            this.btnRunPolicyEffects.Name = "btnRunPolicyEffects";
            this.btnRunPolicyEffects.Size = new System.Drawing.Size(159, 40);
            this.btnRunPolicyEffects.TabIndex = 0;
            this.btnRunPolicyEffects.Text = "Run && Show Results";
            this.btnRunPolicyEffects.UseVisualStyleBackColor = true;
            this.btnRunPolicyEffects.Click += new System.EventHandler(this.btnRunPolicyEffects_Click);
            // 
            // labelYear1
            // 
            this.labelYear1.AutoSize = true;
            this.labelYear1.Location = new System.Drawing.Point(16, 15);
            this.labelYear1.Name = "labelYear1";
            this.labelYear1.Size = new System.Drawing.Size(102, 13);
            this.labelYear1.TabIndex = 1;
            this.labelYear1.Text = "Start period (year 1):";
            // 
            // labelYear2
            // 
            this.labelYear2.AutoSize = true;
            this.labelYear2.Location = new System.Drawing.Point(280, 15);
            this.labelYear2.Name = "labelYear2";
            this.labelYear2.Size = new System.Drawing.Size(99, 13);
            this.labelYear2.TabIndex = 2;
            this.labelYear2.Text = "End period (year 2):";
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(157, 12);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(94, 21);
            this.comboBox1.TabIndex = 4;
            this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // comboBox2
            // 
            this.comboBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox2.FormattingEnabled = true;
            this.comboBox2.Location = new System.Drawing.Point(416, 12);
            this.comboBox2.Name = "comboBox2";
            this.comboBox2.Size = new System.Drawing.Size(94, 21);
            this.comboBox2.TabIndex = 5;
            this.comboBox2.SelectedIndexChanged += new System.EventHandler(this.comboBox2_SelectedIndexChanged);
            // 
            // textBoxAlpha
            // 
            this.textBoxAlpha.Location = new System.Drawing.Point(26, 60);
            this.textBoxAlpha.Name = "textBoxAlpha";
            this.textBoxAlpha.Size = new System.Drawing.Size(73, 21);
            this.textBoxAlpha.TabIndex = 10;
            this.textBoxAlpha.Text = "1";
            // 
            // gridControl1
            // 
            this.gridControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.gridControl1.Location = new System.Drawing.Point(20, 52);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.RepositoryItems.AddRange(new DevExpress.XtraEditors.Repository.RepositoryItem[] {
            this.repositoryItemComboBoxData,
            this.repositoryItemComboBoxSys1,
            this.repositoryItemComboBoxSys2,
            this.repositoryItemCheckEditCountry});
            this.gridControl1.Size = new System.Drawing.Size(422, 273);
            this.gridControl1.TabIndex = 12;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            // 
            // gridView1
            // 
            this.gridView1.Columns.AddRange(new DevExpress.XtraGrid.Columns.GridColumn[] {
            this.gridColumnCheck,
            this.gridColumnCountry,
            this.gridColumnSys1,
            this.gridColumnData1,
            this.gridColumnSys2,
            this.gridColumnData2});
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.IndicatorWidth = 20;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsCustomization.AllowColumnMoving = false;
            this.gridView1.OptionsCustomization.AllowColumnResizing = false;
            this.gridView1.OptionsCustomization.AllowFilter = false;
            this.gridView1.OptionsCustomization.AllowGroup = false;
            this.gridView1.OptionsCustomization.AllowSort = false;
            this.gridView1.OptionsMenu.EnableColumnMenu = false;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            this.gridView1.CustomDrawColumnHeader += new DevExpress.XtraGrid.Views.Grid.ColumnHeaderCustomDrawEventHandler(this.gridView1_CustomDrawColumnHeader);
            // 
            // gridColumnCheck
            // 
            this.gridColumnCheck.Caption = " ";
            this.gridColumnCheck.ColumnEdit = this.repositoryItemCheckEditCountry;
            this.gridColumnCheck.FieldName = "Check";
            this.gridColumnCheck.MaxWidth = 65;
            this.gridColumnCheck.MinWidth = 65;
            this.gridColumnCheck.Name = "gridColumnCheck";
            this.gridColumnCheck.Visible = true;
            this.gridColumnCheck.VisibleIndex = 0;
            this.gridColumnCheck.Width = 65;
            // 
            // repositoryItemCheckEditCountry
            // 
            this.repositoryItemCheckEditCountry.AllowHtmlDraw = DevExpress.Utils.DefaultBoolean.True;
            this.repositoryItemCheckEditCountry.AutoHeight = false;
            this.repositoryItemCheckEditCountry.Caption = "Check";
            this.repositoryItemCheckEditCountry.Name = "repositoryItemCheckEditCountry";
            this.repositoryItemCheckEditCountry.CheckedChanged += new System.EventHandler(this.repositoryItemCheckEditCountry_CheckedChanged);
            // 
            // gridColumnCountry
            // 
            this.gridColumnCountry.Caption = "Country";
            this.gridColumnCountry.FieldName = "Country";
            this.gridColumnCountry.Name = "gridColumnCountry";
            this.gridColumnCountry.OptionsColumn.AllowEdit = false;
            this.gridColumnCountry.OptionsColumn.ReadOnly = true;
            this.gridColumnCountry.Visible = true;
            this.gridColumnCountry.VisibleIndex = 1;
            this.gridColumnCountry.Width = 78;
            // 
            // gridColumnSys1
            // 
            this.gridColumnSys1.Caption = "System year 1";
            this.gridColumnSys1.ColumnEdit = this.repositoryItemComboBoxSys1;
            this.gridColumnSys1.FieldName = "System1";
            this.gridColumnSys1.Name = "gridColumnSys1";
            this.gridColumnSys1.Visible = true;
            this.gridColumnSys1.VisibleIndex = 2;
            this.gridColumnSys1.Width = 80;
            // 
            // repositoryItemComboBoxSys1
            // 
            this.repositoryItemComboBoxSys1.AutoHeight = false;
            this.repositoryItemComboBoxSys1.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBoxSys1.Name = "repositoryItemComboBoxSys1";
            this.repositoryItemComboBoxSys1.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // gridColumnData1
            // 
            this.gridColumnData1.Caption = "Population year 1";
            this.gridColumnData1.ColumnEdit = this.repositoryItemComboBoxData;
            this.gridColumnData1.FieldName = "Data1";
            this.gridColumnData1.Name = "gridColumnData1";
            this.gridColumnData1.Visible = true;
            this.gridColumnData1.VisibleIndex = 4;
            this.gridColumnData1.Width = 110;
            // 
            // repositoryItemComboBoxData
            // 
            this.repositoryItemComboBoxData.AutoHeight = false;
            this.repositoryItemComboBoxData.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBoxData.Name = "repositoryItemComboBoxData";
            this.repositoryItemComboBoxData.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // gridColumnSys2
            // 
            this.gridColumnSys2.Caption = "System year 2";
            this.gridColumnSys2.ColumnEdit = this.repositoryItemComboBoxSys2;
            this.gridColumnSys2.FieldName = "System2";
            this.gridColumnSys2.Name = "gridColumnSys2";
            this.gridColumnSys2.Visible = true;
            this.gridColumnSys2.VisibleIndex = 3;
            this.gridColumnSys2.Width = 80;
            // 
            // repositoryItemComboBoxSys2
            // 
            this.repositoryItemComboBoxSys2.AutoHeight = false;
            this.repositoryItemComboBoxSys2.Buttons.AddRange(new DevExpress.XtraEditors.Controls.EditorButton[] {
            new DevExpress.XtraEditors.Controls.EditorButton(DevExpress.XtraEditors.Controls.ButtonPredefines.Combo)});
            this.repositoryItemComboBoxSys2.Name = "repositoryItemComboBoxSys2";
            this.repositoryItemComboBoxSys2.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            // 
            // gridColumnData2
            // 
            this.gridColumnData2.Caption = "Population year 2";
            this.gridColumnData2.ColumnEdit = this.repositoryItemComboBoxData;
            this.gridColumnData2.FieldName = "Data2";
            this.gridColumnData2.Name = "gridColumnData2";
            this.gridColumnData2.Visible = true;
            this.gridColumnData2.VisibleIndex = 5;
            this.gridColumnData2.Width = 110;
            // 
            // lblInfo
            // 
            this.lblInfo.Location = new System.Drawing.Point(557, 15);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(46, 17);
            this.lblInfo.TabIndex = 15;
            this.lblInfo.Text = "label3";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblInfo.Visible = false;
            // 
            // labelDecomposition
            // 
            this.labelDecomposition.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDecomposition.Location = new System.Drawing.Point(462, 159);
            this.labelDecomposition.Name = "labelDecomposition";
            this.labelDecomposition.Size = new System.Drawing.Size(87, 13);
            this.labelDecomposition.TabIndex = 18;
            this.labelDecomposition.Text = "Decomposition on ";
            // 
            // buttonClose
            // 
            this.buttonClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonClose.Location = new System.Drawing.Point(12, 12);
            this.buttonClose.Name = "buttonClose";
            this.buttonClose.Size = new System.Drawing.Size(111, 40);
            this.buttonClose.TabIndex = 19;
            this.buttonClose.Text = "Close";
            this.buttonClose.UseVisualStyleBackColor = true;
            this.buttonClose.Click += new System.EventHandler(this.buttonClose_Click);
            // 
            // textBoxOutputPath
            // 
            this.textBoxOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxOutputPath.Location = new System.Drawing.Point(20, 404);
            this.textBoxOutputPath.Name = "textBoxOutputPath";
            this.textBoxOutputPath.Size = new System.Drawing.Size(557, 20);
            this.textBoxOutputPath.TabIndex = 24;
            // 
            // textRunFirstNHH
            // 
            this.textRunFirstNHH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));
            this.textRunFirstNHH.Location = new System.Drawing.Point(315, 21);
            this.textRunFirstNHH.Name = "textRunFirstNHH";
            this.textRunFirstNHH.Size = new System.Drawing.Size(50, 20);
            // 
            // labelRunFirstNHH
            // 
            this.labelRunFirstNHH.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left))));
            this.labelRunFirstNHH.Location = new System.Drawing.Point(217, 23);
            this.labelRunFirstNHH.AutoSize = true;
            this.labelRunFirstNHH.Name = "labelRunFirstNHH";
            this.labelRunFirstNHH.Size = new System.Drawing.Size(55, 13);
            this.labelRunFirstNHH.Text = "Run first N HH only";
            // 
            // buttonChangePath
            // 
            this.buttonChangePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonChangePath.Image = ((System.Drawing.Image)(resources.GetObject("buttonChangePath.Image")));
            this.buttonChangePath.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.buttonChangePath.Location = new System.Drawing.Point(586, 401);
            this.buttonChangePath.Name = "buttonChangePath";
            this.buttonChangePath.Size = new System.Drawing.Size(27, 27);
            this.buttonChangePath.TabIndex = 25;
            this.buttonChangePath.Click += new System.EventHandler(this.buttonChangePath_Click);
            // 
            // panelResults
            // 
            this.panelResults.Controls.Add(this.fileTabSelector);
            this.panelResults.Location = new System.Drawing.Point(624, 0);
            this.panelResults.Name = "panelResults";
            this.panelResults.Padding = new System.Windows.Forms.Padding(5);
            this.panelResults.Size = new System.Drawing.Size(624, 377);
            this.panelResults.TabIndex = 26;
            // 
            // fileTabSelector
            // 
            this.fileTabSelector.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.fileTabSelector.Dock = System.Windows.Forms.DockStyle.Fill;
            this.fileTabSelector.Location = new System.Drawing.Point(5, 5);
            this.fileTabSelector.Margin = new System.Windows.Forms.Padding(5);
            this.fileTabSelector.Name = "fileTabSelector";
            this.fileTabSelector.SelectedIndex = 0;
            this.fileTabSelector.Size = new System.Drawing.Size(614, 367);
            this.fileTabSelector.TabIndex = 7;
            // 
            // btnRunOnly
            // 
            this.btnRunOnly.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRunOnly.Location = new System.Drawing.Point(322, 12);
            this.btnRunOnly.Name = "btnRunOnly";
            this.btnRunOnly.Size = new System.Drawing.Size(111, 40);
            this.btnRunOnly.TabIndex = 29;
            this.btnRunOnly.Text = "Run Only";
            this.btnRunOnly.UseVisualStyleBackColor = true;
            this.btnRunOnly.Click += new System.EventHandler(this.btnRunOnly_Click);
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.chkEM2);
            this.panelBottom.Controls.Add(this.btnRunOnly);
            this.panelBottom.Controls.Add(this.buttonClose);
            this.panelBottom.Controls.Add(this.btnRunPolicyEffects);
            this.panelBottom.Controls.Add(this.labelRunFirstNHH);
            this.panelBottom.Controls.Add(this.textRunFirstNHH);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 436);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(626, 64);
            this.panelBottom.TabIndex = 30;
            // 
            // chkEM2
            // 
            this.chkEM2.AutoSize = true;
            this.chkEM2.Location = new System.Drawing.Point(139, 22);
            this.chkEM2.Name = "chkEM2";
            this.chkEM2.Size = new System.Drawing.Size(70, 17);
            this.chkEM2.TabIndex = 30;
            this.chkEM2.Text = "Use EM2";
            this.chkEM2.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 381);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 13);
            this.label1.TabIndex = 32;
            this.label1.Text = "Output path:";
            // 
            // labelIndex
            // 
            this.labelIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelIndex.Location = new System.Drawing.Point(462, 268);
            this.labelIndex.Name = "labelIndex";
            this.labelIndex.Size = new System.Drawing.Size(31, 13);
            this.labelIndex.TabIndex = 34;
            this.labelIndex.Text = "Index ";
            // 
            // groupBoxData
            // 
            this.groupBoxData.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxData.Controls.Add(this.checkRadioDataBoth);
            this.groupBoxData.Controls.Add(this.checkRadioData2);
            this.groupBoxData.Controls.Add(this.checkRadioData1);
            this.groupBoxData.Location = new System.Drawing.Point(455, 168);
            this.groupBoxData.Name = "groupBoxData";
            this.groupBoxData.Size = new System.Drawing.Size(157, 88);
            this.groupBoxData.TabIndex = 35;
            // 
            // checkRadioDataBoth
            // 
            this.checkRadioDataBoth.Location = new System.Drawing.Point(9, 62);
            this.checkRadioDataBoth.Name = "checkRadioDataBoth";
            this.checkRadioDataBoth.Properties.Caption = "Both datasets";
            this.checkRadioDataBoth.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkRadioDataBoth.Properties.RadioGroupIndex = 1;
            this.checkRadioDataBoth.Size = new System.Drawing.Size(106, 19);
            this.checkRadioDataBoth.TabIndex = 2;
            this.checkRadioDataBoth.TabStop = false;
            this.checkRadioDataBoth.CheckedChanged += new System.EventHandler(this.checkRadioDataBoth_CheckedChanged);
            // 
            // checkRadioData2
            // 
            this.checkRadioData2.Location = new System.Drawing.Point(9, 36);
            this.checkRadioData2.Name = "checkRadioData2";
            this.checkRadioData2.Properties.Caption = "Data2";
            this.checkRadioData2.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkRadioData2.Properties.RadioGroupIndex = 1;
            this.checkRadioData2.Size = new System.Drawing.Size(60, 19);
            this.checkRadioData2.TabIndex = 1;
            this.checkRadioData2.TabStop = false;
            this.checkRadioData2.CheckedChanged += new System.EventHandler(this.checkRadioData2_CheckedChanged);
            // 
            // checkRadioData1
            // 
            this.checkRadioData1.Location = new System.Drawing.Point(9, 10);
            this.checkRadioData1.Name = "checkRadioData1";
            this.checkRadioData1.Properties.Caption = "Data1";
            this.checkRadioData1.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkRadioData1.Properties.RadioGroupIndex = 1;
            this.checkRadioData1.Size = new System.Drawing.Size(60, 19);
            this.checkRadioData1.TabIndex = 0;
            this.checkRadioData1.TabStop = false;
            this.checkRadioData1.CheckedChanged += new System.EventHandler(this.checkRadioData1_CheckedChanged);
            // 
            // groupBoxAlpha
            // 
            this.groupBoxAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxAlpha.Controls.Add(this.textBoxAlpha);
            this.groupBoxAlpha.Controls.Add(this.checkBoxAlphaFIX);
            this.groupBoxAlpha.Controls.Add(this.checkBoxAlphaCPI);
            this.groupBoxAlpha.Controls.Add(this.checkBoxAlphaMII);
            this.groupBoxAlpha.Controls.Add(this.btnAlphaRange);
            this.groupBoxAlpha.Location = new System.Drawing.Point(455, 52);
            this.groupBoxAlpha.Name = "groupBoxAlpha";
            this.groupBoxAlpha.Size = new System.Drawing.Size(157, 94);
            this.groupBoxAlpha.TabIndex = 36;
            // 
            // checkBoxAlphaFIX
            // 
            this.checkBoxAlphaFIX.Location = new System.Drawing.Point(9, 62);
            this.checkBoxAlphaFIX.Name = "checkBoxAlphaFIX";
            this.checkBoxAlphaFIX.Properties.Caption = "";
            this.checkBoxAlphaFIX.Size = new System.Drawing.Size(75, 19);
            this.checkBoxAlphaFIX.TabIndex = 2;
            // 
            // checkBoxAlphaCPI
            // 
            this.checkBoxAlphaCPI.Location = new System.Drawing.Point(9, 36);
            this.checkBoxAlphaCPI.Name = "checkBoxAlphaCPI";
            this.checkBoxAlphaCPI.Properties.Caption = "CPI";
            this.checkBoxAlphaCPI.Size = new System.Drawing.Size(46, 19);
            this.checkBoxAlphaCPI.TabIndex = 1;
            // 
            // checkBoxAlphaMII
            // 
            this.checkBoxAlphaMII.Location = new System.Drawing.Point(9, 10);
            this.checkBoxAlphaMII.Name = "checkBoxAlphaMII";
            this.checkBoxAlphaMII.Properties.Caption = "MII";
            this.checkBoxAlphaMII.Size = new System.Drawing.Size(46, 19);
            this.checkBoxAlphaMII.TabIndex = 0;
            // 
            // btnAlphaRange
            // 
            this.btnAlphaRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAlphaRange.Location = new System.Drawing.Point(101, 60);
            this.btnAlphaRange.Name = "btnAlphaRange";
            this.btnAlphaRange.Size = new System.Drawing.Size(53, 23);
            this.btnAlphaRange.TabIndex = 11;
            this.btnAlphaRange.Text = "Range";
            this.btnAlphaRange.UseVisualStyleBackColor = true;
            this.btnAlphaRange.Click += new System.EventHandler(this.btnAlphaRange_Click);
            // 
            // labelAlpha
            // 
            this.labelAlpha.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAlpha.Location = new System.Drawing.Point(462, 43);
            this.labelAlpha.Name = "labelAlpha";
            this.labelAlpha.Size = new System.Drawing.Size(92, 13);
            this.labelAlpha.TabIndex = 37;
            this.labelAlpha.Text = "Indexation (alpha) ";
            // 
            // groupBoxIndex
            // 
            this.groupBoxIndex.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxIndex.Controls.Add(this.checkRadioMarketUnscaled);
            this.groupBoxIndex.Controls.Add(this.checkRadioMarket);
            this.groupBoxIndex.Controls.Add(this.checkRadioMonetary);
            this.groupBoxIndex.Location = new System.Drawing.Point(455, 277);
            this.groupBoxIndex.Name = "groupBoxIndex";
            this.groupBoxIndex.Size = new System.Drawing.Size(157, 113);
            this.groupBoxIndex.TabIndex = 36;
            // 
            // checkRadioMarketUnscaled
            // 
            this.checkRadioMarketUnscaled.Location = new System.Drawing.Point(9, 33);
            this.checkRadioMarketUnscaled.Name = "checkRadioMarketUnscaled";
            this.checkRadioMarketUnscaled.Properties.Appearance.Options.UseTextOptions = true;
            this.checkRadioMarketUnscaled.Properties.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.checkRadioMarketUnscaled.Properties.Caption = "Market Incomes (unscaled output)";
            this.checkRadioMarketUnscaled.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkRadioMarketUnscaled.Properties.RadioGroupIndex = 2;
            this.checkRadioMarketUnscaled.Size = new System.Drawing.Size(122, 30);
            this.checkRadioMarketUnscaled.TabIndex = 3;
            this.checkRadioMarketUnscaled.TabStop = false;
            // 
            // checkRadioMarket
            // 
            this.checkRadioMarket.Location = new System.Drawing.Point(9, 9);
            this.checkRadioMarket.Name = "checkRadioMarket";
            this.checkRadioMarket.Properties.Caption = "Market Incomes";
            this.checkRadioMarket.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkRadioMarket.Properties.RadioGroupIndex = 2;
            this.checkRadioMarket.Size = new System.Drawing.Size(122, 19);
            this.checkRadioMarket.TabIndex = 1;
            this.checkRadioMarket.TabStop = false;
            // 
            // checkRadioMonetary
            // 
            this.checkRadioMonetary.Location = new System.Drawing.Point(9, 73);
            this.checkRadioMonetary.Name = "checkRadioMonetary";
            this.checkRadioMonetary.Properties.Appearance.Options.UseTextOptions = true;
            this.checkRadioMonetary.Properties.Appearance.TextOptions.WordWrap = DevExpress.Utils.WordWrap.Wrap;
            this.checkRadioMonetary.Properties.Caption = "Policy Parameters                 && Market Incomes";
            this.checkRadioMonetary.Properties.CheckStyle = DevExpress.XtraEditors.Controls.CheckStyles.Radio;
            this.checkRadioMonetary.Properties.RadioGroupIndex = 2;
            this.checkRadioMonetary.Size = new System.Drawing.Size(139, 30);
            this.checkRadioMonetary.TabIndex = 0;
            this.checkRadioMonetary.TabStop = false;
            // 
            // checkAllCountries
            // 
            this.checkAllCountries.Location = new System.Drawing.Point(23, 56);
            this.checkAllCountries.Name = "checkAllCountries";
            this.checkAllCountries.Properties.Caption = "";
            this.checkAllCountries.Size = new System.Drawing.Size(17, 19);
            this.checkAllCountries.TabIndex = 11;
            this.checkAllCountries.CheckedChanged += new System.EventHandler(this.checkAllCountries_CheckedChanged);
            // 
            // chkTreatAsMarket
            // 
            this.chkTreatAsMarket.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkTreatAsMarket.AutoSize = true;
            this.chkTreatAsMarket.Location = new System.Drawing.Point(20, 331);
            this.chkTreatAsMarket.Name = "chkTreatAsMarket";
            this.chkTreatAsMarket.Size = new System.Drawing.Size(331, 17);
            this.chkTreatAsMarket.TabIndex = 38;
            this.chkTreatAsMarket.Text = "Treat all *non-simulated* monetary variables as ‘market incomes’ ";
            this.chkTreatAsMarket.UseVisualStyleBackColor = true;
            // 
            // lblAddons
            // 
            this.lblAddons.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblAddons.AutoSize = true;
            this.lblAddons.Location = new System.Drawing.Point(17, 357);
            this.lblAddons.Name = "lblAddons";
            this.lblAddons.Size = new System.Drawing.Size(49, 13);
            this.lblAddons.TabIndex = 40;
            this.lblAddons.Text = "Add-ons:";
            // 
            // chkListAddons
            // 
            this.chkListAddons.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.chkListAddons.CheckOnClick = true;
            this.chkListAddons.ColumnWidth = 100;
            this.chkListAddons.Location = new System.Drawing.Point(72, 354);
            this.chkListAddons.MultiColumn = true;
            this.chkListAddons.Name = "chkListAddons";
            this.chkListAddons.Size = new System.Drawing.Size(370, 19);
            this.chkListAddons.TabIndex = 41;
            this.chkListAddons.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.chkListAddons_ItemCheck);
            this.chkListAddons.SelectedIndexChanged += new System.EventHandler(this.chkListAddons_SelectedIndexChanged);
            this.chkListAddons.ClientSizeChanged += new System.EventHandler(this.chkListAddons_ClientSizeChanged);
            // 
            // PolicyEffects
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CancelButton = this.buttonClose;
            this.ClientSize = new System.Drawing.Size(626, 500);
            this.Controls.Add(this.chkListAddons);
            this.Controls.Add(this.lblAddons);
            this.Controls.Add(this.chkTreatAsMarket);
            this.Controls.Add(this.checkAllCountries);
            this.Controls.Add(this.labelAlpha);
            this.Controls.Add(this.labelIndex);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panelBottom);
            this.Controls.Add(this.panelResults);
            this.Controls.Add(this.buttonChangePath);
            this.Controls.Add(this.textBoxOutputPath);
            this.Controls.Add(this.labelDecomposition);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.gridControl1);
            this.Controls.Add(this.comboBox2);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.labelYear2);
            this.Controls.Add(this.labelYear1);
            this.Controls.Add(this.groupBoxAlpha);
            this.Controls.Add(this.groupBoxData);
            this.Controls.Add(this.groupBoxIndex);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.MinimumSize = new System.Drawing.Size(642, 534);
            this.Name = "PolicyEffects";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PolicyEffects_FormClosing);
            this.Load += new System.EventHandler(this.PolicyEffects_Load);
            this.Shown += new System.EventHandler(this.PolicyEffects_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemCheckEditCountry)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxSys1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxData)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.repositoryItemComboBoxSys2)).EndInit();
            this.panelResults.ResumeLayout(false);
            this.panelBottom.ResumeLayout(false);
            this.panelBottom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxData)).EndInit();
            this.groupBoxData.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioDataBoth.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioData2.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioData1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxAlpha)).EndInit();
            this.groupBoxAlpha.ResumeLayout(false);
            this.groupBoxAlpha.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAlphaFIX.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAlphaCPI.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkBoxAlphaMII.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.groupBoxIndex)).EndInit();
            this.groupBoxIndex.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioMarketUnscaled.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioMarket.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkRadioMonetary.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.checkAllCountries.Properties)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnRunPolicyEffects;
        private System.Windows.Forms.Label labelYear1;
        private System.Windows.Forms.Label labelYear2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.TextBox textBoxAlpha;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCountry;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnData1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnData2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnSys1;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnSys2;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBoxData;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBoxSys1;
        private DevExpress.XtraEditors.Repository.RepositoryItemComboBox repositoryItemComboBoxSys2;
        private DevExpress.XtraGrid.Columns.GridColumn gridColumnCheck;
        private DevExpress.XtraEditors.Repository.RepositoryItemCheckEdit repositoryItemCheckEditCountry;
        private System.Windows.Forms.Label lblInfo;
        private DevExpress.XtraEditors.LabelControl labelDecomposition;
        private System.Windows.Forms.Button buttonClose;
        private System.Windows.Forms.TextBox textBoxOutputPath;
        private DevExpress.XtraEditors.SimpleButton buttonChangePath;
        private System.Windows.Forms.Panel panelResults;
        private System.Windows.Forms.TabControl fileTabSelector;
        private System.Windows.Forms.Button btnRunOnly;
        private System.Windows.Forms.Panel panelBottom;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.LabelControl labelIndex;
        private DevExpress.XtraEditors.PanelControl groupBoxData;
        private DevExpress.XtraEditors.CheckEdit checkRadioDataBoth;
        private DevExpress.XtraEditors.CheckEdit checkRadioData2;
        private DevExpress.XtraEditors.CheckEdit checkRadioData1;
        private DevExpress.XtraEditors.PanelControl groupBoxAlpha;
        private DevExpress.XtraEditors.CheckEdit checkBoxAlphaFIX;
        private DevExpress.XtraEditors.CheckEdit checkBoxAlphaCPI;
        private DevExpress.XtraEditors.CheckEdit checkBoxAlphaMII;
        private DevExpress.XtraEditors.LabelControl labelAlpha;
        private DevExpress.XtraEditors.PanelControl groupBoxIndex;
        private DevExpress.XtraEditors.CheckEdit checkRadioMarket;
        private DevExpress.XtraEditors.CheckEdit checkRadioMonetary;
        private DevExpress.XtraEditors.CheckEdit checkRadioMarketUnscaled;
        private DevExpress.XtraEditors.CheckEdit checkAllCountries;
        private System.Windows.Forms.CheckBox chkTreatAsMarket;
        private System.Windows.Forms.Label lblAddons;
        private CheckedListBox chkListAddons;
        private CheckBox chkEM2;
        private System.Windows.Forms.Button btnAlphaRange;
        private System.Windows.Forms.Label labelRunFirstNHH;
        private System.Windows.Forms.TextBox textRunFirstNHH;
    }
}