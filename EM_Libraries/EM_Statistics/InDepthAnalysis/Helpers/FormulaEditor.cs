using EM_Statistics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_Statistics.InDepthAnalysis
{
    internal class FormulaEditor
    {
        private readonly Form owningDialog;
        private TextBox owningTextBox = null;
        private DataGridViewCell owningGridCell = null;

        private TextBox editor = null;

        private const int editorLines = 3;

        internal FormulaEditor(Form _parentDialog)
        {
            owningDialog = _parentDialog;
            CreateEditor();
        }

        private void CreateEditor()
        {
            try
            {
                editor = new TextBox()
                {
                    Multiline = true, ScrollBars = ScrollBars.Vertical,
                    Location = new Point(0, 0), Size = new Size(0, 0),
                    Visible = false
                };
                owningDialog.Controls.Add(editor);
                editor.Validating += new CancelEventHandler(Editor_Validating);
            }
            catch (Exception exception)
            {
                editor = null;
                MessageBox.Show($"Failed to create formula editor: {exception.Message}");
            }
        }

        internal void Show(TextBox textBox)
        {
            if (editor == null) return;
            try
            {
                // note: our controls (text boxes, grid views) are located in group boxes therefore the parent left and top has to be added
                editor.Location = new Point(textBox.Parent.Left + textBox.Left, textBox.Parent.Top + textBox.Top);
                editor.Size = new Size(textBox.Width, textBox.Height * editorLines);
                editor.Show(); textBox.Hide();

                editor.Text = textBox.Text;
                editor.Focus(); editor.Select(textBox.SelectionStart, textBox.SelectionLength);

                owningTextBox = textBox; owningGridCell = null;
            }
            catch (Exception exception) { MessageBox.Show($"Failed to show formula editor: {exception.Message}"); }
        }

        internal void Show(DataGridViewCell gridCell)
        {
            if (editor == null) return;
            try
            {
                // note: our controls (text boxes, grid views) are located in group boxes therefore the parent left and top has to be added
                Rectangle cellLocation = gridCell.DataGridView.GetCellDisplayRectangle(gridCell.ColumnIndex, gridCell.RowIndex, true);
                editor.Location = new Point(gridCell.DataGridView.Parent.Location.X + gridCell.DataGridView.Location.X + cellLocation.Left,
                                            gridCell.DataGridView.Parent.Location.Y + gridCell.DataGridView.Location.Y + cellLocation.Top);
                editor.Size = new Size(gridCell.OwningColumn.Width, gridCell.OwningRow.Height * editorLines);
                editor.Show();

                editor.Text = gridCell.Value?.ToString();
                editor.Focus(); editor.Select(editor.Text.Length, 0);
                gridCell.ReadOnly = true;

                owningGridCell = gridCell; owningTextBox = null;
            }
            catch (Exception exception) { MessageBox.Show($"Failed to show formula editor: {exception.Message}"); }
        }

        private void Editor_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                if (!IsValidFormula(editor.Text, out List<string> errors))
                {
                    if (errors.Any()) MessageBox.Show(string.Join(Environment.NewLine, errors));
                    e.Cancel = true; return;
                }
                editor.Hide();

                if (owningTextBox != null)
                {
                    owningTextBox.Text = editor.Text;
                    owningTextBox.Show();
                }
                if (owningGridCell != null)
                {
                    owningGridCell.Value = editor.Text;
                    owningGridCell.ReadOnly = false; // todo: by pressing tab currently the cell gets the focus, instead of the next cell
                }
            }
            catch { }
        }

        internal bool IsValidFormula(string text, out List<string> errors)
        {
            // not yet sure what this includes (or whether it should be part of TextToFormula)
            // there could be a check for valid characters (but probably better to not allow inserting them in first place)
            // other issues: matching brackets, valid comparison (= vs. ==), specific checks (starts with comparison operator), etc.
            errors = new List<string>();
            try { return true; }
            catch (Exception exception) { return false; }
        }

        private static List<string> SEPARATORS = new List<string>() { "==", "!=", ">=", "<=", "&&", "||",
                                                                      ">", "<", "!", "(", ")",
                                                                      "+", "-", "*", "/",
                                                                      "=", "&", "|"};
        private static List<string> SINGLE_CHAR_COMPARERS = new List<string>() { "=", "&", "|" };

        internal static string Add_DATA_VAR(string text, bool forValidation = false)
        {
            try
            {
                if (string.IsNullOrEmpty(text)) return text;

                string _text = string.Empty, operand = string.Empty; text.Replace("–", "-");
                for (int i = 0; i < text.Length; ++i)
                {
                    string separator = null;
                    foreach (string s in SEPARATORS) // note: SEPARATORS must be sorted by lenght, in order to find e.g. >= before >
                        if (i + s.Length <= text.Length && text.Substring(i, s.Length) == s) { separator = s; break; }
                    if (separator == null) operand += text[i];
                    else
                    {
                        _text += HandleOperand() +
                            (SINGLE_CHAR_COMPARERS.Contains(separator) ? separator + separator : separator); // change &|= to &&||==
                        if (separator.Length == 2) ++i;
                    }
                }
                _text += HandleOperand();
                return _text;

                string HandleOperand()
                {
                    string o = operand.Trim();
                    if (!string.IsNullOrEmpty(o) && !double.TryParse(o, out _)) o = Settings.DATA_VAR(forValidation ? "0" : o);
                    operand = string.Empty; return o;
                }
            }
            catch { return text; }
        }

        internal static string Remove_DATA_VAR(string text)
        {
            return string.IsNullOrEmpty(text) ? text : text.Replace(HardDefinitions.FormulaParameter.DATA_VAR + "@", string.Empty).
                                                            Replace(HardDefinitions.FormulaParameter.CLOSING_TOKEN, string.Empty);
        }

        internal static bool ContainsSeparator(string s, bool allowInnerSpace = false)
        {
            if (string.IsNullOrEmpty(s)) return false;
            s = s.Trim(); if (s.Contains(" ")) return true;
            foreach (string cmp in SEPARATORS) if (s.Contains(cmp)) return true;
            return false;
        }
    }
}
