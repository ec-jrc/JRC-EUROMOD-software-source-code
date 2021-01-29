using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Dialogs
{
    internal partial class MatrixViewOfIncomelistsForm : Form
    {
        List<CountryConfig.ParameterRow> _ILDefParameterRows_ContainingILNames = null;
        string _defaultCaption = string.Empty; //title of window ("Matrix View of Incomlists for ")
        const int _columnWidth = 50;

        void MatrixViewOfIncomelistsForm_Load(object sender, EventArgs e)
        {
            string helpPath; EM_AppContext.Instance.GetHelpPath(out helpPath); helpProvider.HelpNamespace = helpPath;
        }

        void btnClose_Click(object sender, EventArgs e)
        {
            Hide();
        }

        void MatrixViewOfIncomelistsForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true; //don't close just hide (dialog is non-modal and topmost)
            Hide();
        }

        internal void UpdateView(SystemTreeListTag systemTag)
        {
            this.Text = _defaultCaption + systemTag.GetSystemRow().CountryRow.Name + " " + systemTag.GetSystemRow().Name;

            Show(); //unhide dialog if closed

            dgvMatrix.Rows.Clear();
            dgvMatrix.Columns.Clear();

            _ILDefParameterRows_ContainingILNames = systemTag.GetParameterRowsILs();
            if (_ILDefParameterRows_ContainingILNames.Count == 0)
                return;

            Dictionary<string, Dictionary<string, double>> matrix = new Dictionary<string, Dictionary<string, double>>();
            GenerateMatrix(ref matrix);

            foreach (CountryConfig.ParameterRow _ILDefParameterRows_ContainingILName in _ILDefParameterRows_ContainingILNames) //generate columns with incomelist names as headers
            {
                int columnIndex = dgvMatrix.Columns.Add(_ILDefParameterRows_ContainingILName.Value, _ILDefParameterRows_ContainingILName.Value);
                dgvMatrix.Columns[columnIndex].Width = _columnWidth; //if AutoSizeMode is set to something else than none (e.g. DisplayedCellsExceptHeader) user cannot resize columns anymore
            }

            foreach (string variableName in matrix.Keys) //generate rows with variables as header
            {
                int rowIndex = dgvMatrix.Rows.Add();
                dgvMatrix.Rows[rowIndex].HeaderCell.Value = variableName;
                foreach (string ILName in matrix[variableName].Keys)
                {
                    double factor = matrix[variableName][ILName];
                    if (factor != 0)
                        dgvMatrix[ILName, rowIndex].Value = factor.ToString();
                }
            }
        }

        void GenerateMatrix(ref Dictionary<string, Dictionary<string, double>> matrix)
        {
            //generate helper dictionary with incomelist names as keys and ILDef-function-rows as values
            Dictionary<string, CountryConfig.FunctionRow> ILNamesAndILDefFunctionRows = new Dictionary<string, CountryConfig.FunctionRow>();
            foreach (CountryConfig.ParameterRow ILDefParameterRows_ContainingILName in _ILDefParameterRows_ContainingILNames)
                ILNamesAndILDefFunctionRows.Add(ILDefParameterRows_ContainingILName.Value.ToLower(), ILDefParameterRows_ContainingILName.FunctionRow);

            //fill the matrix recursivly, i.e. incomelist entries have to be broken down into their variables
            foreach (CountryConfig.ParameterRow ILDefParameterRows_ContainingILName in _ILDefParameterRows_ContainingILNames)
                AddILToMatrix(ILDefParameterRows_ContainingILName.Value.ToLower(),
                              ILDefParameterRows_ContainingILName.FunctionRow,
                              ILNamesAndILDefFunctionRows,
                              ref matrix);
        }

        void AddILToMatrix(string ILName, CountryConfig.FunctionRow ILDefFunctionRow,
                            Dictionary<string, CountryConfig.FunctionRow> ILNamesAndILDefFunctionRows,
                            ref Dictionary<string, Dictionary<string, double>> matrix, double inhertitedFactor = 1)
        {
            foreach (CountryConfig.ParameterRow ILDefParameterRow in ILDefFunctionRow.GetParameterRows())
            {
                if (ILDefParameterRow.Name.ToLower() == DefPar.DefIl.Name.ToLower())
                    continue;

                string ILEntry = string.Empty;
                double factor = 0;

                //new style: incomelist entry (i.e. variable or incomelist) stored in parameter.Name, factor stored in parameter.Value
                if (EM_Helpers.IsNumeric(ILDefParameterRow.Value) ||
                    ILDefParameterRow.Value == "+" || ILDefParameterRow.Value == "-")
                {
                    if (ILDefParameterRow.Value == "+")
                        factor = inhertitedFactor;
                    else if (ILDefParameterRow.Value == "-")
                        factor = inhertitedFactor * -1.0;
                    else
                        factor = inhertitedFactor * EM_Helpers.SaveConvertToDouble(ILDefParameterRow.Value);
                    ILEntry = ILDefParameterRow.Name.ToLower();
                }

                //old style: incomelist entry (i.e. variable or incomelist) stored in parameter.Value, +/- stored in parameter.Name, factor stored in parameter.Group
                else
                {
                    if (ILDefParameterRow.Group == string.Empty)
                        factor = inhertitedFactor;
                    else
                        factor = inhertitedFactor * EM_Helpers.SaveConvertToDouble(ILDefParameterRow.Group); //either there is a factor in the group column or factor is 1
                    if (ILDefParameterRow.Name.StartsWith("-"))
                        factor = inhertitedFactor * (-1);
                    else if (!ILDefParameterRow.Name.StartsWith("+"))
                        continue; //should not happen
                    ILEntry = ILDefParameterRow.Value.ToLower();
                }

                if (ILNamesAndILDefFunctionRows.Keys.Contains(ILEntry))
                    AddILToMatrix(ILName, ILNamesAndILDefFunctionRows[ILEntry], ILNamesAndILDefFunctionRows, ref matrix, factor); //recursive call: break incomelist down in its variables
                else
                {
                    if (!matrix.Keys.Contains(ILEntry))
                    {//variable not yet in matrix: add a variable-row with a column for each incomelist (cell is empty if variable is not contained in an incomelist)
                        Dictionary<string, double> variableNamesAndInitialValues = new Dictionary<string, double>();
                        foreach (string variableName in ILNamesAndILDefFunctionRows.Keys)
                            variableNamesAndInitialValues.Add(variableName, 0);
                        matrix.Add(ILEntry, variableNamesAndInitialValues);
                    }
                    matrix[ILEntry][ILName] += factor;
                }
            }
        }

        internal MatrixViewOfIncomelistsForm()
        {
            InitializeComponent();
            _defaultCaption = Text;
        }
    }
}
