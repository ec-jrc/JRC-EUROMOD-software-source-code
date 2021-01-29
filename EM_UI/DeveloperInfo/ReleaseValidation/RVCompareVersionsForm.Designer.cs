namespace EM_UI.DeveloperInfo.ReleaseValidation
{
    partial class RVCompareVersionsForm
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
            this.dataGrid = new System.Windows.Forms.DataGridView();
            this.colCountry = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDelData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddSys = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDelSys = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colAddPol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDelPol = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGrid
            // 
            this.dataGrid.AllowUserToAddRows = false;
            this.dataGrid.AllowUserToDeleteRows = false;
            this.dataGrid.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.DisplayedCells;
            this.dataGrid.BackgroundColor = System.Drawing.SystemColors.ButtonFace;
            this.dataGrid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableAlwaysIncludeHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.dataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colCountry,
            this.colAddData,
            this.colDelData,
            this.colAddSys,
            this.colDelSys,
            this.colAddPol,
            this.colDelPol});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.ControlLight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dataGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.dataGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGrid.Location = new System.Drawing.Point(0, 0);
            this.dataGrid.Name = "dataGrid";
            this.dataGrid.ReadOnly = true;
            this.dataGrid.RowHeadersVisible = false;
            this.dataGrid.RowTemplate.Height = 24;
            this.dataGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.ColumnHeaderSelect;
            this.dataGrid.Size = new System.Drawing.Size(982, 494);
            this.dataGrid.TabIndex = 0;
            // 
            // colCountry
            // 
            this.colCountry.HeaderText = "Country";
            this.colCountry.Name = "colCountry";
            this.colCountry.ReadOnly = true;
            this.colCountry.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colCountry.Width = 70;
            // 
            // colAddData
            // 
            this.colAddData.HeaderText = "Added Datasets";
            this.colAddData.Name = "colAddData";
            this.colAddData.ReadOnly = true;
            this.colAddData.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colAddData.Width = 150;
            // 
            // colDelData
            // 
            this.colDelData.HeaderText = "Deleted Datasets";
            this.colDelData.Name = "colDelData";
            this.colDelData.ReadOnly = true;
            this.colDelData.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDelData.Width = 150;
            // 
            // colAddSys
            // 
            this.colAddSys.HeaderText = "Added Systems";
            this.colAddSys.Name = "colAddSys";
            this.colAddSys.ReadOnly = true;
            this.colAddSys.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colAddSys.Width = 150;
            // 
            // colDelSys
            // 
            this.colDelSys.HeaderText = "Deleted Systems";
            this.colDelSys.Name = "colDelSys";
            this.colDelSys.ReadOnly = true;
            this.colDelSys.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDelSys.Width = 150;
            // 
            // colAddPol
            // 
            this.colAddPol.HeaderText = "Added Policies";
            this.colAddPol.Name = "colAddPol";
            this.colAddPol.ReadOnly = true;
            this.colAddPol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colAddPol.Width = 150;
            // 
            // colDelPol
            // 
            this.colDelPol.HeaderText = "Deleted Policies";
            this.colDelPol.Name = "colDelPol";
            this.colDelPol.ReadOnly = true;
            this.colDelPol.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.colDelPol.Width = 150;
            // 
            // RVCompareVersionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(982, 494);
            this.Controls.Add(this.dataGrid);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RVCompareVersionsForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn colCountry;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDelData;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddSys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDelSys;
        private System.Windows.Forms.DataGridViewTextBoxColumn colAddPol;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDelPol;
    }
}