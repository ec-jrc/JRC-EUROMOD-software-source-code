namespace EM_UI.Dialogs
{
    partial class FindReplaceForm
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
            this.cboFind = new System.Windows.Forms.ComboBox();
            this.cboReplaceBy = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labReplace = new System.Windows.Forms.Label();
            this.chkMatchCase = new System.Windows.Forms.CheckBox();
            this.checkEntireCell = new System.Windows.Forms.CheckBox();
            this.btnFindNext = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnReplace = new System.Windows.Forms.Button();
            this.btnReplaceAll = new System.Windows.Forms.Button();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radSearchByRows = new System.Windows.Forms.RadioButton();
            this.radSearchByColumns = new System.Windows.Forms.RadioButton();
            this.btnFindPrevious = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.radSearchInSelectedCells = new System.Windows.Forms.RadioButton();
            this.radSearchInVisibleCells = new System.Windows.Forms.RadioButton();
            this.radSeachInAllCells = new System.Windows.Forms.RadioButton();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radSearchInCommentColumn = new System.Windows.Forms.RadioButton();
            this.radSearchInPolicyColumn = new System.Windows.Forms.RadioButton();
            this.radSeachInSystemColumns = new System.Windows.Forms.RadioButton();
            this.radSearchInAllColumns = new System.Windows.Forms.RadioButton();
            this.chkIncludePrivateComments = new System.Windows.Forms.CheckBox();
            this.chkMatchExactWord = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // cboFind
            // 
            this.cboFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboFind.FormattingEnabled = true;
            this.cboFind.Location = new System.Drawing.Point(16, 23);
            this.cboFind.Name = "cboFind";
            this.cboFind.Size = new System.Drawing.Size(351, 21);
            this.cboFind.TabIndex = 0;
            // 
            // cboReplaceBy
            // 
            this.cboReplaceBy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboReplaceBy.FormattingEnabled = true;
            this.cboReplaceBy.Location = new System.Drawing.Point(16, 65);
            this.cboReplaceBy.Name = "cboReplaceBy";
            this.cboReplaceBy.Size = new System.Drawing.Size(351, 21);
            this.cboReplaceBy.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Search";
            // 
            // labReplace
            // 
            this.labReplace.AutoSize = true;
            this.labReplace.Location = new System.Drawing.Point(13, 49);
            this.labReplace.Name = "labReplace";
            this.labReplace.Size = new System.Drawing.Size(61, 13);
            this.labReplace.TabIndex = 5;
            this.labReplace.Text = "Replace by";
            // 
            // chkMatchCase
            // 
            this.chkMatchCase.AutoSize = true;
            this.chkMatchCase.Location = new System.Drawing.Point(149, 175);
            this.chkMatchCase.Name = "chkMatchCase";
            this.chkMatchCase.Size = new System.Drawing.Size(83, 17);
            this.chkMatchCase.TabIndex = 5;
            this.chkMatchCase.Text = "Match Case";
            this.chkMatchCase.UseVisualStyleBackColor = true;
            // 
            // checkEntireCell
            // 
            this.checkEntireCell.AutoSize = true;
            this.checkEntireCell.Location = new System.Drawing.Point(149, 198);
            this.checkEntireCell.Name = "checkEntireCell";
            this.checkEntireCell.Size = new System.Drawing.Size(146, 17);
            this.checkEntireCell.TabIndex = 6;
            this.checkEntireCell.Text = "Match Entire Cell Content";
            this.checkEntireCell.UseVisualStyleBackColor = true;
            // 
            // btnFindNext
            // 
            this.btnFindNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFindNext.Location = new System.Drawing.Point(377, 23);
            this.btnFindNext.Name = "btnFindNext";
            this.btnFindNext.Size = new System.Drawing.Size(95, 23);
            this.btnFindNext.TabIndex = 7;
            this.btnFindNext.Text = "Search &Next";
            this.btnFindNext.UseVisualStyleBackColor = true;
            this.btnFindNext.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(377, 233);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(95, 23);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnReplace
            // 
            this.btnReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplace.Location = new System.Drawing.Point(377, 106);
            this.btnReplace.Name = "btnReplace";
            this.btnReplace.Size = new System.Drawing.Size(95, 23);
            this.btnReplace.TabIndex = 9;
            this.btnReplace.Text = "&Replace";
            this.btnReplace.UseVisualStyleBackColor = true;
            this.btnReplace.Click += new System.EventHandler(this.btnReplace_Click);
            // 
            // btnReplaceAll
            // 
            this.btnReplaceAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnReplaceAll.Location = new System.Drawing.Point(377, 135);
            this.btnReplaceAll.Name = "btnReplaceAll";
            this.btnReplaceAll.Size = new System.Drawing.Size(95, 23);
            this.btnReplaceAll.TabIndex = 10;
            this.btnReplaceAll.Text = "Replace &All";
            this.btnReplaceAll.UseVisualStyleBackColor = true;
            this.btnReplaceAll.Click += new System.EventHandler(this.btnReplaceAll_Click);
            // 
            // helpProvider
            // 
            this.helpProvider.HelpNamespace = "C:\\Euromod\\Bauer42\\EM_UI\\EuromodFiles\\Help\\EUROMODHelp.chm";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radSearchByRows);
            this.groupBox1.Controls.Add(this.radSearchByColumns);
            this.groupBox1.Location = new System.Drawing.Point(16, 198);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(121, 41);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Search by";
            // 
            // radSearchByRows
            // 
            this.radSearchByRows.AutoSize = true;
            this.radSearchByRows.Location = new System.Drawing.Point(69, 17);
            this.radSearchByRows.Name = "radSearchByRows";
            this.radSearchByRows.Size = new System.Drawing.Size(52, 17);
            this.radSearchByRows.TabIndex = 1;
            this.radSearchByRows.Text = "Rows";
            this.radSearchByRows.UseVisualStyleBackColor = true;
            // 
            // radSearchByColumns
            // 
            this.radSearchByColumns.AutoSize = true;
            this.radSearchByColumns.Checked = true;
            this.radSearchByColumns.Location = new System.Drawing.Point(4, 17);
            this.radSearchByColumns.Name = "radSearchByColumns";
            this.radSearchByColumns.Size = new System.Drawing.Size(65, 17);
            this.radSearchByColumns.TabIndex = 0;
            this.radSearchByColumns.TabStop = true;
            this.radSearchByColumns.Text = "Columns";
            this.radSearchByColumns.UseVisualStyleBackColor = true;
            // 
            // btnFindPrevious
            // 
            this.btnFindPrevious.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnFindPrevious.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnFindPrevious.Location = new System.Drawing.Point(377, 52);
            this.btnFindPrevious.Name = "btnFindPrevious";
            this.btnFindPrevious.Size = new System.Drawing.Size(95, 23);
            this.btnFindPrevious.TabIndex = 8;
            this.btnFindPrevious.Text = "Search &Previous";
            this.btnFindPrevious.UseVisualStyleBackColor = true;
            this.btnFindPrevious.Click += new System.EventHandler(this.btnFindPrevious_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.radSearchInSelectedCells);
            this.groupBox2.Controls.Add(this.radSearchInVisibleCells);
            this.groupBox2.Controls.Add(this.radSeachInAllCells);
            this.groupBox2.Location = new System.Drawing.Point(16, 99);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(121, 93);
            this.groupBox2.TabIndex = 2;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Search in";
            // 
            // radSearchInSelectedCells
            // 
            this.radSearchInSelectedCells.AutoSize = true;
            this.radSearchInSelectedCells.Location = new System.Drawing.Point(6, 65);
            this.radSearchInSelectedCells.Name = "radSearchInSelectedCells";
            this.radSearchInSelectedCells.Size = new System.Drawing.Size(92, 17);
            this.radSearchInSelectedCells.TabIndex = 2;
            this.radSearchInSelectedCells.TabStop = true;
            this.radSearchInSelectedCells.Text = "Selected Cells";
            this.radSearchInSelectedCells.UseVisualStyleBackColor = true;
            // 
            // radSearchInVisibleCells
            // 
            this.radSearchInVisibleCells.AutoSize = true;
            this.radSearchInVisibleCells.Location = new System.Drawing.Point(6, 42);
            this.radSearchInVisibleCells.Name = "radSearchInVisibleCells";
            this.radSearchInVisibleCells.Size = new System.Drawing.Size(80, 17);
            this.radSearchInVisibleCells.TabIndex = 1;
            this.radSearchInVisibleCells.TabStop = true;
            this.radSearchInVisibleCells.Text = "Visible Cells";
            this.radSearchInVisibleCells.UseVisualStyleBackColor = true;
            // 
            // radSeachInAllCells
            // 
            this.radSeachInAllCells.AutoSize = true;
            this.radSeachInAllCells.Checked = true;
            this.radSeachInAllCells.Location = new System.Drawing.Point(6, 19);
            this.radSeachInAllCells.Name = "radSeachInAllCells";
            this.radSeachInAllCells.Size = new System.Drawing.Size(61, 17);
            this.radSeachInAllCells.TabIndex = 0;
            this.radSeachInAllCells.TabStop = true;
            this.radSeachInAllCells.Text = "All Cells";
            this.radSeachInAllCells.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radSearchInCommentColumn);
            this.groupBox3.Controls.Add(this.radSearchInPolicyColumn);
            this.groupBox3.Controls.Add(this.radSeachInSystemColumns);
            this.groupBox3.Controls.Add(this.radSearchInAllColumns);
            this.groupBox3.Location = new System.Drawing.Point(148, 99);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(222, 66);
            this.groupBox3.TabIndex = 3;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Search in";
            // 
            // radSearchInCommentColumn
            // 
            this.radSearchInCommentColumn.AutoSize = true;
            this.radSearchInCommentColumn.Location = new System.Drawing.Point(106, 42);
            this.radSearchInCommentColumn.Name = "radSearchInCommentColumn";
            this.radSearchInCommentColumn.Size = new System.Drawing.Size(107, 17);
            this.radSearchInCommentColumn.TabIndex = 3;
            this.radSearchInCommentColumn.TabStop = true;
            this.radSearchInCommentColumn.Text = "Comment Column";
            this.radSearchInCommentColumn.UseVisualStyleBackColor = true;
            // 
            // radSearchInPolicyColumn
            // 
            this.radSearchInPolicyColumn.AutoSize = true;
            this.radSearchInPolicyColumn.Location = new System.Drawing.Point(6, 42);
            this.radSearchInPolicyColumn.Name = "radSearchInPolicyColumn";
            this.radSearchInPolicyColumn.Size = new System.Drawing.Size(91, 17);
            this.radSearchInPolicyColumn.TabIndex = 2;
            this.radSearchInPolicyColumn.TabStop = true;
            this.radSearchInPolicyColumn.Text = "Policy Column";
            this.radSearchInPolicyColumn.UseVisualStyleBackColor = true;
            // 
            // radSeachInSystemColumns
            // 
            this.radSeachInSystemColumns.AutoSize = true;
            this.radSeachInSystemColumns.Location = new System.Drawing.Point(106, 19);
            this.radSeachInSystemColumns.Name = "radSeachInSystemColumns";
            this.radSeachInSystemColumns.Size = new System.Drawing.Size(102, 17);
            this.radSeachInSystemColumns.TabIndex = 1;
            this.radSeachInSystemColumns.TabStop = true;
            this.radSeachInSystemColumns.Text = "System Columns";
            this.radSeachInSystemColumns.UseVisualStyleBackColor = true;
            // 
            // radSearchInAllColumns
            // 
            this.radSearchInAllColumns.AutoSize = true;
            this.radSearchInAllColumns.Checked = true;
            this.radSearchInAllColumns.Location = new System.Drawing.Point(6, 19);
            this.radSearchInAllColumns.Name = "radSearchInAllColumns";
            this.radSearchInAllColumns.Size = new System.Drawing.Size(79, 17);
            this.radSearchInAllColumns.TabIndex = 0;
            this.radSearchInAllColumns.TabStop = true;
            this.radSearchInAllColumns.Text = "All Columns";
            this.radSearchInAllColumns.UseVisualStyleBackColor = true;
            // 
            // chkIncludePrivateComments
            // 
            this.chkIncludePrivateComments.AutoSize = true;
            this.chkIncludePrivateComments.Location = new System.Drawing.Point(149, 243);
            this.chkIncludePrivateComments.Name = "chkIncludePrivateComments";
            this.chkIncludePrivateComments.Size = new System.Drawing.Size(149, 17);
            this.chkIncludePrivateComments.TabIndex = 12;
            this.chkIncludePrivateComments.Text = "Include Private Comments";
            this.chkIncludePrivateComments.UseVisualStyleBackColor = true;
            // 
            // chkMatchExactWord
            // 
            this.chkMatchExactWord.AutoSize = true;
            this.chkMatchExactWord.Location = new System.Drawing.Point(149, 220);
            this.chkMatchExactWord.Name = "chkMatchExactWord";
            this.chkMatchExactWord.Size = new System.Drawing.Size(115, 17);
            this.chkMatchExactWord.TabIndex = 13;
            this.chkMatchExactWord.Text = "Match Exact Word";
            this.chkMatchExactWord.UseVisualStyleBackColor = true;
            // 
            // FindReplaceForm
            // 
            this.AcceptButton = this.btnFindNext;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(478, 268);
            this.Controls.Add(this.chkMatchExactWord);
            this.Controls.Add(this.chkIncludePrivateComments);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.btnFindPrevious);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnReplaceAll);
            this.Controls.Add(this.btnReplace);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnFindNext);
            this.Controls.Add(this.checkEntireCell);
            this.Controls.Add(this.chkMatchCase);
            this.Controls.Add(this.labReplace);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cboReplaceBy);
            this.Controls.Add(this.cboFind);
            this.helpProvider.SetHelpKeyword(this, "EM_WW_SearchReplace.htm");
            this.helpProvider.SetHelpNavigator(this, System.Windows.Forms.HelpNavigator.Topic);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FindReplaceForm";
            this.helpProvider.SetShowHelp(this, true);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Search and Replace";
            this.TopMost = true;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FindReplaceForm_FormClosing);
            this.Load += new System.EventHandler(this.FindReplaceForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cboFind;
        private System.Windows.Forms.ComboBox cboReplaceBy;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label labReplace;
        private System.Windows.Forms.CheckBox chkMatchCase;
        private System.Windows.Forms.CheckBox checkEntireCell;
        private System.Windows.Forms.Button btnFindNext;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnReplace;
        private System.Windows.Forms.Button btnReplaceAll;
        private System.Windows.Forms.HelpProvider helpProvider;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radSearchByRows;
        private System.Windows.Forms.RadioButton radSearchByColumns;
        private System.Windows.Forms.Button btnFindPrevious;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radSearchInSelectedCells;
        private System.Windows.Forms.RadioButton radSearchInVisibleCells;
        private System.Windows.Forms.RadioButton radSeachInAllCells;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radSearchInPolicyColumn;
        private System.Windows.Forms.RadioButton radSeachInSystemColumns;
        private System.Windows.Forms.RadioButton radSearchInAllColumns;
        private System.Windows.Forms.RadioButton radSearchInCommentColumn;
        private System.Windows.Forms.CheckBox chkIncludePrivateComments;
        private System.Windows.Forms.CheckBox chkMatchExactWord;
    }
}