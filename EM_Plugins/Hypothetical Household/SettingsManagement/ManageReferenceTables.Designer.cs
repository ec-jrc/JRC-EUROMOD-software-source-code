namespace HypotheticalHousehold.SettingsManagement
{
    partial class ManageReferenceTables
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManageReferenceTables));
            this.listBoxControlTables = new DevExpress.XtraEditors.ListBoxControl();
            this.toolTipController1 = new DevExpress.Utils.ToolTipController(this.components);
            this.panelTop = new System.Windows.Forms.Panel();
            this.buttonRemoveTable = new DevExpress.XtraEditors.SimpleButton();
            this.buttonAddTable = new DevExpress.XtraEditors.SimpleButton();
            this.panelMiddle = new System.Windows.Forms.Panel();
            this.splitContainerControl1 = new DevExpress.XtraEditors.SplitContainerControl();
            this.gridControl1 = new DevExpress.XtraGrid.GridControl();
            this.gridView1 = new DevExpress.XtraGrid.Views.Grid.GridView();
            this.panelInputBox = new System.Windows.Forms.Panel();
            this.textEdit1 = new DevExpress.XtraEditors.TextEdit();
            this.label1 = new System.Windows.Forms.Label();
            this.panelBottom = new System.Windows.Forms.Panel();
            this.btnSave = new DevExpress.XtraEditors.SimpleButton();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.listBoxControlTables)).BeginInit();
            this.panelTop.SuspendLayout();
            this.panelMiddle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).BeginInit();
            this.splitContainerControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).BeginInit();
            this.panelInputBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).BeginInit();
            this.panelBottom.SuspendLayout();
            this.SuspendLayout();
            // 
            // listBoxControlTables
            // 
            this.listBoxControlTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBoxControlTables.Location = new System.Drawing.Point(0, 0);
            this.listBoxControlTables.Name = "listBoxControlTables";
            this.listBoxControlTables.Size = new System.Drawing.Size(184, 401);
            this.listBoxControlTables.TabIndex = 0;
            this.listBoxControlTables.ToolTipController = this.toolTipController1;
            this.listBoxControlTables.SelectedIndexChanged += new System.EventHandler(this.listBoxControlTables_SelectedIndexChanged);
            this.listBoxControlTables.MouseLeave += new System.EventHandler(this.listBoxControlTables_MouseLeave);
            this.listBoxControlTables.MouseMove += new System.Windows.Forms.MouseEventHandler(this.listBoxControlTables_MouseMove);
            // 
            // toolTipController1
            // 
            this.toolTipController1.AutoPopDelay = 60000;
            // 
            // panelTop
            // 
            this.panelTop.Controls.Add(this.buttonRemoveTable);
            this.panelTop.Controls.Add(this.buttonAddTable);
            this.panelTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelTop.Location = new System.Drawing.Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new System.Drawing.Size(785, 61);
            this.panelTop.TabIndex = 1;
            // 
            // buttonRemoveTable
            // 
            this.buttonRemoveTable.Image = ((System.Drawing.Image)(resources.GetObject("buttonRemoveTable.Image")));
            this.buttonRemoveTable.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleRight;
            this.buttonRemoveTable.Location = new System.Drawing.Point(185, 10);
            this.buttonRemoveTable.Name = "buttonRemoveTable";
            this.buttonRemoveTable.Size = new System.Drawing.Size(177, 42);
            this.buttonRemoveTable.TabIndex = 1;
            this.buttonRemoveTable.Text = "Remove Reference Table";
            this.buttonRemoveTable.Click += new System.EventHandler(this.buttonRemoveTable_Click);
            // 
            // buttonAddTable
            // 
            this.buttonAddTable.Image = ((System.Drawing.Image)(resources.GetObject("buttonAddTable.Image")));
            this.buttonAddTable.ImageLocation = DevExpress.XtraEditors.ImageLocation.MiddleCenter;
            this.buttonAddTable.ImageToTextAlignment = DevExpress.XtraEditors.ImageAlignToText.RightCenter;
            this.buttonAddTable.Location = new System.Drawing.Point(12, 10);
            this.buttonAddTable.Name = "buttonAddTable";
            this.buttonAddTable.Size = new System.Drawing.Size(160, 42);
            this.buttonAddTable.TabIndex = 0;
            this.buttonAddTable.Text = "Add Reference Table";
            this.buttonAddTable.Click += new System.EventHandler(this.buttonAddTable_Click);
            // 
            // panelMiddle
            // 
            this.panelMiddle.Controls.Add(this.splitContainerControl1);
            this.panelMiddle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelMiddle.Location = new System.Drawing.Point(0, 61);
            this.panelMiddle.Name = "panelMiddle";
            this.panelMiddle.Size = new System.Drawing.Size(785, 401);
            this.panelMiddle.TabIndex = 2;
            // 
            // splitContainerControl1
            // 
            this.splitContainerControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerControl1.Location = new System.Drawing.Point(0, 0);
            this.splitContainerControl1.Name = "splitContainerControl1";
            this.splitContainerControl1.Panel1.Controls.Add(this.listBoxControlTables);
            this.splitContainerControl1.Panel1.MinSize = 150;
            this.splitContainerControl1.Panel2.Controls.Add(this.gridControl1);
            this.splitContainerControl1.Panel2.Controls.Add(this.panelInputBox);
            this.splitContainerControl1.Panel2.MinSize = 200;
            this.splitContainerControl1.Size = new System.Drawing.Size(785, 401);
            this.splitContainerControl1.SplitterPosition = 184;
            this.splitContainerControl1.TabIndex = 4;
            this.splitContainerControl1.Text = "splitContainerControl1";
            // 
            // gridControl1
            // 
            this.gridControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridControl1.Location = new System.Drawing.Point(0, 31);
            this.gridControl1.MainView = this.gridView1;
            this.gridControl1.Name = "gridControl1";
            this.gridControl1.Size = new System.Drawing.Size(596, 370);
            this.gridControl1.TabIndex = 1;
            this.gridControl1.ViewCollection.AddRange(new DevExpress.XtraGrid.Views.Base.BaseView[] {
            this.gridView1});
            this.gridControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.gridControl1_KeyDown);
            // 
            // gridView1
            // 
            this.gridView1.GridControl = this.gridControl1;
            this.gridView1.Name = "gridView1";
            this.gridView1.OptionsCustomization.AllowGroup = false;
            this.gridView1.OptionsSelection.MultiSelect = true;
            this.gridView1.OptionsSelection.MultiSelectMode = DevExpress.XtraGrid.Views.Grid.GridMultiSelectMode.CellSelect;
            this.gridView1.OptionsView.ShowGroupPanel = false;
            // 
            // panelInputBox
            // 
            this.panelInputBox.Controls.Add(this.textEdit1);
            this.panelInputBox.Controls.Add(this.label1);
            this.panelInputBox.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelInputBox.Location = new System.Drawing.Point(0, 0);
            this.panelInputBox.Name = "panelInputBox";
            this.panelInputBox.Padding = new System.Windows.Forms.Padding(5);
            this.panelInputBox.Size = new System.Drawing.Size(596, 31);
            this.panelInputBox.TabIndex = 3;
            // 
            // textEdit1
            // 
            this.textEdit1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textEdit1.Location = new System.Drawing.Point(68, 5);
            this.textEdit1.Name = "textEdit1";
            this.textEdit1.Size = new System.Drawing.Size(523, 20);
            this.textEdit1.TabIndex = 5;
            this.textEdit1.Leave += new System.EventHandler(this.textEdit1_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Left;
            this.label1.Location = new System.Drawing.Point(5, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(5);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label1.Size = new System.Drawing.Size(63, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "Table Info: ";
            // 
            // panelBottom
            // 
            this.panelBottom.Controls.Add(this.btnSave);
            this.panelBottom.Controls.Add(this.button1);
            this.panelBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelBottom.Location = new System.Drawing.Point(0, 462);
            this.panelBottom.Name = "panelBottom";
            this.panelBottom.Size = new System.Drawing.Size(785, 61);
            this.panelBottom.TabIndex = 3;
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSave.Location = new System.Drawing.Point(573, 13);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(81, 35);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(668, 13);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(102, 35);
            this.button1.TabIndex = 0;
            this.button1.Text = "Close";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ManageReferenceTables
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(785, 523);
            this.Controls.Add(this.panelMiddle);
            this.Controls.Add(this.panelTop);
            this.Controls.Add(this.panelBottom);
            this.Name = "ManageReferenceTables";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Manage Reference Tables";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ManageReferenceTables_FormClosing);
            this.Load += new System.EventHandler(this.ManageReferenceTables_Load);
            ((System.ComponentModel.ISupportInitialize)(this.listBoxControlTables)).EndInit();
            this.panelTop.ResumeLayout(false);
            this.panelMiddle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerControl1)).EndInit();
            this.splitContainerControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridControl1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.gridView1)).EndInit();
            this.panelInputBox.ResumeLayout(false);
            this.panelInputBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.textEdit1.Properties)).EndInit();
            this.panelBottom.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DevExpress.XtraEditors.ListBoxControl listBoxControlTables;
        private System.Windows.Forms.Panel panelTop;
        private System.Windows.Forms.Panel panelMiddle;
        private DevExpress.XtraGrid.GridControl gridControl1;
        private DevExpress.XtraGrid.Views.Grid.GridView gridView1;
        private System.Windows.Forms.Panel panelBottom;
        private DevExpress.XtraEditors.SimpleButton buttonRemoveTable;
        private DevExpress.XtraEditors.SimpleButton buttonAddTable;
        private System.Windows.Forms.Button button1;
        private DevExpress.XtraEditors.SimpleButton btnSave;
        private System.Windows.Forms.Panel panelInputBox;
        private System.Windows.Forms.Label label1;
        private DevExpress.XtraEditors.TextEdit textEdit1;
        private DevExpress.XtraEditors.SplitContainerControl splitContainerControl1;
        private DevExpress.Utils.ToolTipController toolTipController1;
    }
}