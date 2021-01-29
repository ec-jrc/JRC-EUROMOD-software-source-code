namespace EM_UI.ContextMenu
{
    partial class CellContextMenu
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
            this.mnuCell = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mniCopyValues = new System.Windows.Forms.ToolStripMenuItem();
            this.mniPasteValues = new System.Windows.Forms.ToolStripMenuItem();
            this.separator = new System.Windows.Forms.ToolStripSeparator();
            this.mniPrivateComment = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCell.SuspendLayout();
            this.SuspendLayout();
            // 
            // mnuCell
            // 
            this.mnuCell.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mniCopyValues,
            this.mniPasteValues,
            this.separator,
            this.mniPrivateComment});
            this.mnuCell.Name = "mnuCell";
            this.mnuCell.ShowImageMargin = false;
            this.mnuCell.Size = new System.Drawing.Size(137, 98);
            // 
            // mniCopyValues
            // 
            this.mniCopyValues.Name = "mniCopyValues";
            this.mniCopyValues.Size = new System.Drawing.Size(136, 22);
            this.mniCopyValues.Text = "Copy Value(s)";
            this.mniCopyValues.Click += new System.EventHandler(this.mniCopyValues_Click);
            // 
            // mniPasteValues
            // 
            this.mniPasteValues.Name = "mniPasteValues";
            this.mniPasteValues.Size = new System.Drawing.Size(136, 22);
            this.mniPasteValues.Text = "Paste Value(s)";
            this.mniPasteValues.Click += new System.EventHandler(this.mniPasteValues_Click);
            // 
            // separator
            // 
            this.separator.Name = "separator";
            this.separator.Size = new System.Drawing.Size(133, 6);
            // 
            // mniPrivateComment
            // 
            this.mniPrivateComment.Name = "mniPrivateComment";
            this.mniPrivateComment.Size = new System.Drawing.Size(136, 22);
            this.mniPrivateComment.Text = "Private Comment";
            this.mniPrivateComment.Click += new System.EventHandler(this.mniPrivateComment_Click);
            // 
            // CellContextMenu
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "CellContextMenu";
            this.mnuCell.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ContextMenuStrip mnuCell;
        private System.Windows.Forms.ToolStripMenuItem mniCopyValues;
        private System.Windows.Forms.ToolStripMenuItem mniPasteValues;
        private System.Windows.Forms.ToolStripSeparator separator;
        private System.Windows.Forms.ToolStripMenuItem mniPrivateComment;
    }
}
