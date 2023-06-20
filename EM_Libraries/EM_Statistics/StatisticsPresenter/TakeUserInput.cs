using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using EM_Statistics;

namespace EM_Statistics.StatisticsPresenter
{
    public partial class TakeUserInput : Form
    {
        
        //private Dictionary<EMS_UserInputDefinitionItem, Control> uiDefAndControl = new Dictionary<EMS_UserInputDefinitionItem, Control>();
        internal List<Template.TemplateInfo.UserVariable> userVariables = null;

        class ComboItem 
        {
            public string Text;
            public string Value;
            public override string ToString() { return Text; }
        }

        public TakeUserInput(Template.TemplateInfo templateInfo)
        {

            InitializeComponent();

            labCaption.Text = templateInfo.name;

            int x = 12, y = 20, space = 12, dialogWidth = Math.Max(labCaption.Width, labSubCaption.Width), tabIndex = 0; Control selControl = null;
            foreach (Template.TemplateInfo.UserVariable uv in templateInfo.GetUserVariables())
            {
                // draw control-label (i.e. the instruction for the user)
                Label label = new Label() { Text = uv.title, Left = x, Top = y, Anchor = AnchorStyles.Top | AnchorStyles.Left, AutoSize = true };
                panContent.Controls.Add(label);
                y += label.Size.Height;
                dialogWidth = Math.Max(dialogWidth, label.Size.Width);

                // draw control either as TextBox or ComboBox
                Control control;
                if (uv.inputType == HardDefinitions.UserInputType.Categorical_Numeric ||
                    uv.inputType == HardDefinitions.UserInputType.Categorical_VariableName) // user is expected to select a value of a list (with optionaly being able to type)
                {
                    control = new ComboBox() { DropDownStyle = false ? ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList,
                                               Left = x, Top = y, Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };
                    foreach (Template.TemplateInfo.ComboItem t in uv.comboItems)
                        (control as ComboBox).Items.Add(new ComboItem { Text = t.name, Value = t.value });
                    (control as ComboBox).SelectedIndex = 0;
                }
                else // user is expected to type a value
                    control = new TextBox() { Text = uv.defaultValue, Left = x, Top = y,
                                              Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right };                    
                y += control.Height + space;
                control.Tag = uv; control.TabIndex = tabIndex++;
                control.Validating += inputBox_Validating;
                panContent.Controls.Add(control); if (selControl == null) selControl = control;

                // if there is an additional description for the user, draw an info-button
                if (uv.description != string.Empty)
                {
                    Button infoButton = new Button()
                    {
                        Font = new System.Drawing.Font("Times New Roman", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0))),
                        Location = new Point(ClientSize.Width - x - 30, control.Top), ForeColor = Color.CornflowerBlue, TabIndex = tabIndex++,
                        Size = new System.Drawing.Size(30, 30), // tried to use Control.Height instead, but got different values for Text and Combo
                        Anchor = AnchorStyles.Top | AnchorStyles.Right, Text = "i", TextAlign = ContentAlignment.MiddleCenter, Tag = uv.description
                    };
                    infoButton.Click += infoButton_Click;
                    panContent.Controls.Add(infoButton);
                    control.Width = ClientSize.Width - infoButton.Width - space - 2 * x;
                }
                else control.Width = ClientSize.Width - 2 * x;
            }

            // adapt width and height of the dialog and position OK- and Cancel-buttons
            SetClientSizeCore(dialogWidth + 50, panCaptions.Height + y + btnOK.Height + 4 * space);

            btnOK.Location = new Point(ClientSize.Width / 2 - btnOK.Width - space, ClientSize.Height - btnOK.Height - space);
            btnCancel.Location = new Point(ClientSize.Width / 2 + space, btnOK.Top);

            panContent.Select(); if (selControl != null) selControl.Select();
        }

        void infoButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show((sender as Button).Tag.ToString());
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            userVariables = new List<Template.TemplateInfo.UserVariable>();
            foreach (Control control in panContent.Controls)
            {
                if (control.Tag == null || control.Tag is Template.TemplateInfo.UserVariable == false) continue;
                
                Template.TemplateInfo.UserVariable uv = control.Tag as Template.TemplateInfo.UserVariable;
                object input = GetUserInput(control, uv);
                if (input == null)
                {
                    //if (!uv.Compulsory) continue;
                    MessageBox.Show("This input is compulsory. Please indicate a value.");
                    control.Focus(); return;
                }
                (control.Tag as Template.TemplateInfo.UserVariable).value = input.ToString();
                if (control is ComboBox)
                    (control.Tag as Template.TemplateInfo.UserVariable).description = (control as ComboBox).SelectedItem.ToString();
                
                userVariables.Add(control.Tag as Template.TemplateInfo.UserVariable);
            }
            if (userVariables.Count == 0) userVariables = null;
            DialogResult = DialogResult.OK; Close();
        }

        private static object GetUserInput(Control control, Template.TemplateInfo.UserVariable uv)
        {
            if (control is TextBox /* || uiDef.AllowFreeTyping*/) return control.Text == string.Empty ? null : control.Text;
            ComboBox comboBox = control as ComboBox;
            return comboBox.SelectedItem != null && uv.comboItems.Count(x => x.name == comboBox.SelectedItem.ToString()) > 0
                ? uv.comboItems.Where(x => x.name == comboBox.SelectedItem.ToString()).Select(x => x.value).First() : null;
        }

        private void inputBox_Validating(object sender, CancelEventArgs e)
        {
            try
            {
                Control control = sender as Control;
                Template.TemplateInfo.UserVariable uv = control.Tag as Template.TemplateInfo.UserVariable;
                e.Cancel = !ValidateUserInput(control, uv);   
            }
            catch { }
        }

        private static bool ValidateUserInput(Control control, Template.TemplateInfo.UserVariable uv)
        {
            string error = string.Empty;
            /*            object input = GetUserInput(control, uv);
                        if (input == null) return true;

                        try { Convert.ChangeType(input, uv.ParameterType); } catch (Exception exception) { error = exception.Message; }

                        if (error == string.Empty && (uv.MinMax.Item1 != double.MinValue || uv.MinMax.Item2 != double.MaxValue))
                        {
                            try
                            {
                                double dInput = Convert.ToDouble(input);
                                if (dInput < uv.MinMax.Item1 || dInput > uv.MinMax.Item2)
                                    error = uv.MinMax.Item1 == double.MinValue ? "Value must not be greater than " + uv.MinMax.Item2.ToString()
                                            : (uv.MinMax.Item2 == double.MaxValue ? "Value must not be smaller than " + uv.MinMax.Item1.ToString()
                                                                    : string.Format("Value must be between {0} and {1}", uv.MinMax.Item1, uv.MinMax.Item2));
                            }
                            catch { }
                        }
                        */
            if (error != string.Empty) { MessageBox.Show(uv.name + ":" + Environment.NewLine + error); return false; } return true;
        }
         
    }
}
