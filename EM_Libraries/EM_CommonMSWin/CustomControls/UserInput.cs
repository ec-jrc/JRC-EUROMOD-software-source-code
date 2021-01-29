using EM_Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace EM_Common_Win
{
    public partial class UserInput : Form
    {
        public class Item
        {
            public Item(string id, string label) { Id = id; Label = label; }
            internal string Id = string.Empty;
            internal string Label = string.Empty;
           
            public object InitialValue = string.Empty;
            public string ToolTip = string.Empty;
            public bool IsPassword = false;
            public int LineCount = 1;
            public int Width = -1;
            
            public bool AllowEmpty = false;
            public Type ValueType = typeof(string);
            public Tuple<double, double> MinMax = new Tuple<double, double>(double.MinValue, double.MaxValue);
            public int MaxTextLength = int.MaxValue;

            internal TextBox textBox = null;
        }

        List<Item> Items = null;

        public TSDictionary GetValues()
        {
            TSDictionary values = new TSDictionary();
            foreach (Item item in Items) values.SetItem(item.Id, item.textBox.Text);
            return values;
        }

        public T GetValue<T>(string id)
        {
            TSDictionary values = GetValues();
            if (!values.ContainsKey(id)) throw new Exception("Not existing key: " + id);
            return values.GetItem<T>(id);
        }

        public static DialogResult Get(string label, out string userInput, string intialValue = "", bool allowEmpty = false)
            { return Get(new UserInput.Item("id", label) { InitialValue = intialValue, AllowEmpty = allowEmpty }, out userInput); }
        public static DialogResult Get(Item item, out string userInput)
        {
            UserInput ui = new UserInput(new List<Item>() { item }); DialogResult okCancel = ui.ShowDialog();
            userInput = ui.GetValues().GetItem<string>(item.Id); userInput = userInput ?? string.Empty;
            return okCancel;
        }

        public UserInput(Item item) { UserInputConstructor(new List<Item>() { item }); }
        public UserInput(List<Item> items) { UserInputConstructor(items); }
        private void UserInputConstructor(List<Item> items)
        {
            InitializeComponent();

            Items = items;
            int x = 12, y = 20, space = 12, dialogWidth = Width, tabIndex = 0; Control selControl = null; List<string> ids = new List<string>();
            foreach (Item item in Items)
            {
                if (item.Id == string.Empty) throw new Exception("UserInput.Item: 'Id' must not be empty.");
                if (ids.Contains(item.Id)) throw new Exception("Not unique key: " + item.Id); ids.Add(item.Id);
                if (item.ValueType == typeof(string) && (item.MinMax.Item1 != double.MinValue || item.MinMax.Item2 != double.MaxValue)) item.ValueType = typeof(double);
                Label label = new Label() { Text = item.Label, Left = x, Top = y, Anchor = AnchorStyles.Top | AnchorStyles.Left, AutoSize = true };
                panContent.Controls.Add(label);
                y += label.Size.Height;
                dialogWidth = Math.Max(dialogWidth, label.Size.Width);

                item.textBox = new TextBox() { Text = item.InitialValue.ToString(), Left = x, Top = y };
                if (item.LineCount > 1) { item.textBox.Multiline = true; item.textBox.Height = item.textBox.ClientSize.Height * item.LineCount + 2; }
                if (item.IsPassword) item.textBox.PasswordChar = '*';
                if (item.ToolTip != string.Empty) toolTip.SetToolTip(item.textBox, item.ToolTip);
                if (item.Width > 0) { item.textBox.Width = item.Width; dialogWidth = Math.Max(dialogWidth, item.Width); }
                else { item.textBox.Width = ClientSize.Width - 2 * x; item.textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; }
                y += item.textBox.Height + space;
                item.textBox.TabIndex = tabIndex++;
                item.textBox.Validating += TextBox_Validating;
                item.textBox.Tag = item;

                panContent.Controls.Add(item.textBox); if (selControl == null) selControl = item.textBox;
            }

            // adapt width and height of the dialog and position OK- and Cancel-buttons
            SetClientSizeCore(dialogWidth + 50, y + btnOK.Height + 3 * space);
            foreach (Item item in Items)
                if (item.Width > 0) item.textBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; // set anchor only after resizing dialog to keep user's width-choices

            btnOK.Location = new Point(ClientSize.Width / 2 - btnOK.Width - space, ClientSize.Height - btnOK.Height - space);
            btnCancel.Location = new Point(ClientSize.Width / 2 + space, btnOK.Top);

            panContent.Select(); if (selControl != null) selControl.Select();
        }

        void InfoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show((sender as Button).Tag.ToString());
        }

        private void BtnOK_Click(object sender, System.EventArgs e)
        {
            foreach (Item item in Items)
                if (!item.AllowEmpty && (item.textBox.Text == null || item.textBox.Text.ToString().Trim() == string.Empty))
                    { MessageBox.Show("'" + item.Label.TrimEnd(new char[] { ':'}) + "' must not be empty."); return; }
            DialogResult = DialogResult.OK; Close();
        }

        private void TextBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                TextBox textBox = sender as TextBox; Item item = textBox.Tag as Item; string error = string.Empty;
                if (textBox.Text != null && textBox.Text != string.Empty && item.ValueType != typeof(string))
                {
                    try { Convert.ChangeType(textBox.Text, item.ValueType); }
                    catch { error = "Invalid type. Input type of " + item.ValueType.Name + " required."; }
                    if (error == string.Empty && (item.MinMax.Item1 != double.MinValue || item.MinMax.Item2 != double.MaxValue))
                    {
                        try
                        {
                            double dInput = Convert.ToDouble(textBox.Text);
                            if (dInput < item.MinMax.Item1 || dInput > item.MinMax.Item2)
                                error = item.MinMax.Item1 == double.MinValue ? "Value must not be greater than " + item.MinMax.Item2.ToString() + "."
                                     : (item.MinMax.Item2 == double.MaxValue ? "Value must not be smaller than " + item.MinMax.Item1.ToString() + "."
                                                               : string.Format("Value must be between {0} and {1}.", item.MinMax.Item1, item.MinMax.Item2));
                        }
                        catch { }
                    }
                }
                if (textBox.Text != null && textBox.Text.Length > item.MaxTextLength) error = "Text too long. Maximum number of characters: " + item.MaxTextLength.ToString() + ".";
                if (error != string.Empty) { MessageBox.Show(error); e.Cancel = true; }
            }
            catch { }
        }
    }
}
