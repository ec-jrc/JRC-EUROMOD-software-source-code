namespace EM_UI.ContextMenu
{
    partial class RowContextMenu
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

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.mnuRow = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniHideRow = new System.Windows.Forms.ToolStripMenuItem();
            this.mniHideSelectedRows = new System.Windows.Forms.ToolStripMenuItem();
            this.mniHideAllOtherRows = new System.Windows.Forms.ToolStripMenuItem();
            this.mniHideRowsUnto = new System.Windows.Forms.ToolStripMenuItem();
            this.cboHideRowsUnto = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mniHideNARows = new System.Windows.Forms.ToolStripMenuItem();
            this.mniHideSelectedNARows = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mniUnhideRows = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRow.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuRow
            // 
            this.mnuRow.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniHideRow,
            this.mniHideSelectedRows,
            this.mniHideAllOtherRows,
            this.mniHideRowsUnto,
            this.toolStripMenuItem1,
            this.mniHideNARows,
            this.mniHideSelectedNARows,
            this.toolStripMenuItem2,
            this.mniUnhideRows});
            this.mnuRow.Name = "indicatorMenu";
            this.mnuRow.ShowImageMargin = false;
            this.mnuRow.Size = new System.Drawing.Size(270, 192);
            // 
            // mniHideRow
            // 
            this.mniHideRow.Name = "mniHideRow";
            this.mniHideRow.ShortcutKeyDisplayString = "Alt+H";
            this.mniHideRow.Size = new System.Drawing.Size(269, 22);
            this.mniHideRow.Text = "Hide Row";
            this.mniHideRow.Click += new System.EventHandler(this.mniHideRow_Click);
            // 
            // mniHideSelectedRows
            // 
            this.mniHideSelectedRows.Name = "mniHideSelectedRows";
            this.mniHideSelectedRows.Size = new System.Drawing.Size(269, 22);
            this.mniHideSelectedRows.Text = "Hide Selected Rows";
            this.mniHideSelectedRows.Click += new System.EventHandler(this.mniHideSelectedRows_Click);
            // 
            // mniHideAllOtherRows
            // 
            this.mniHideAllOtherRows.Name = "mniHideAllOtherRows";
            this.mniHideAllOtherRows.ShortcutKeyDisplayString = "Alt+O";
            this.mniHideAllOtherRows.Size = new System.Drawing.Size(269, 22);
            this.mniHideAllOtherRows.Text = "Hide all other Rows";
            this.mniHideAllOtherRows.Click += new System.EventHandler(this.mniHideAllOtherRows_Click);
            // 
            // mniHideRowsUnto
            // 
            this.mniHideRowsUnto.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mniHideRowsUnto.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cboHideRowsUnto});
            this.mniHideRowsUnto.Name = "mniHideRowsUnto";
            this.mniHideRowsUnto.Size = new System.Drawing.Size(269, 22);
            this.mniHideRowsUnto.Text = "Hide Rows unto ...";
            // 
            // cboHideRowsUnto
            // 
            this.cboHideRowsUnto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHideRowsUnto.Name = "cboHideRowsUnto";
            this.cboHideRowsUnto.Size = new System.Drawing.Size(121, 23);
            this.cboHideRowsUnto.DropDownClosed += new System.EventHandler(this.cboHideRowsUnto_DropDownClosed);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(266, 6);
            // 
            // mniHideNARows
            // 
            this.mniHideNARows.Name = "mniHideNARows";
            this.mniHideNARows.Size = new System.Drawing.Size(269, 22);
            this.mniHideNARows.Text = "Hide all \"n/a\" Policy/Function Rows";
            this.mniHideNARows.ToolTipText = "This will hide all Policies/Functions whose value in all visible systems is \"n/a\"" +
    ".";
            this.mniHideNARows.Click += new System.EventHandler(this.mniHideNARows_Click);
            // 
            // mniHideSelectedNARows
            // 
            this.mniHideSelectedNARows.Name = "mniHideSelectedNARows";
            this.mniHideSelectedNARows.Size = new System.Drawing.Size(269, 22);
            this.mniHideSelectedNARows.Text = "Hide selected \"n/a\" Policy/Function Rows";
            this.mniHideSelectedNARows.ToolTipText = "This will hide the selected Policies/Functions whose value in all visible systems" +
    " is \"n/a\".";
            this.mniHideSelectedNARows.Click += new System.EventHandler(this.mniHideSelectedNARows_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(266, 6);
            // 
            // mniUnhideRows
            // 
            this.mniUnhideRows.Name = "mniUnhideRows";
            this.mniUnhideRows.Size = new System.Drawing.Size(269, 22);
            this.mniUnhideRows.Text = "Unhide Rows";
            this.mniUnhideRows.Click += new System.EventHandler(this.mniUnhideRows_Click);
            // 
            // RowContextMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "RowContextMenu";
            this.mnuRow.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip mnuRow;
        private System.Windows.Forms.ToolStripMenuItem mniHideRow;
        private System.Windows.Forms.ToolStripMenuItem mniHideRowsUnto;
        private System.Windows.Forms.ToolStripMenuItem mniUnhideRows;
        private System.Windows.Forms.ToolStripComboBox cboHideRowsUnto;
        private System.Windows.Forms.ToolStripMenuItem mniHideAllOtherRows;
        private System.Windows.Forms.ToolStripMenuItem mniHideSelectedRows;
        private System.Windows.Forms.ToolStripMenuItem mniHideSelectedNARows;
        private System.Windows.Forms.ToolStripMenuItem mniHideNARows;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
    }
}
