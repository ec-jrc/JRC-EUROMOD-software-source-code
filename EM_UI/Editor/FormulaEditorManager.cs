using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraTreeList;
using DevExpress.XtraTreeList.Columns;
using DevExpress.XtraTreeList.Nodes;
using EM_Common;
using EM_UI.DataSets;
using EM_UI.TreeListManagement;
using EM_UI.TreeListTags;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace EM_UI.Editor
{
    // Used for IntelliSense
    internal class IntelliItem
    {
        internal IntelliItem(string text,  string description, int imageIndex = -1) { Text = text; Description = description; ImageIndex = imageIndex; }
        internal string Text { get; set; }
        internal string Description { get; set; }
        internal int ImageIndex { get; set; }
        public override string ToString() { return Text; }
    }

    internal class FormulaEditorManager
    {
        ListBox _lstIntelli = null;
        Label _lblComboBoxEditorToolTip = null;
        ImageList _imlIntelliIcons = null;
        TreeList _treeList = null;
        int _activeSelectionStart = 0;
        CellReferenz _activeCell = null;
        // the next two variables are used for triple-click testing
        System.Windows.Forms.Timer triple_click_timer = new System.Windows.Forms.Timer();
        int click_count = 0;

        
        const int _editorMinHeight = 70;
        const int _editorMaxHeight = 200;
        const int _editorMinWidth = 50;
        const int _editorMaxWidth = 700;
        const int _editorSpareWidth = 50;

        const int _intelliImageVariable = 0;
        const int _intelliImageIL = 1;
        const int _intelliImageTU = 1; //use the same symbol for IL and TU
        const int _intelliImageConfiguration = 2;
        const int _intelliImageFootnote = 3;
        const int _intelliImageConstant = 4;

        const string _queryPrefix = "Q ";

        static char[] textSplitCharacters = new char[] { ' ', '{', '}', '(', ')', ',', '.', '\n', '\r', '\t', '!', ':', ';', '?', '[', ']', '/', '\\', '~', '<', '>', '%', '&', '=', '^', '+', '-', '*' };

        static string GetLastWord(string text)
        {
            string[] parts = text.Split(textSplitCharacters);
            return (parts.Last<string>());
        }

        bool EndsWithFootnote(string lastWord)
        {
            if (lastWord.EndsWith("#"))
                return true;

            string[] parts = lastWord.Split('#');
            int result;
            if (parts.Count() >= 2 && int.TryParse(parts.Last<string>(), out result))   // behind the # only digits!
                return true;

            return false;
        }

        // Show/Close IntelliList
        void Handle_lstIntelli(KeyPressEventArgs e, MemoEdit activeEditor, string lastWord)
        {
            if (_lstIntelli.Items.Count > 0 &&
                (e.KeyChar == '<' || lastWord.Length >= 1 || EndsWithFootnote(lastWord)))   // Should display Intellisense List ...
            {
                _activeSelectionStart = activeEditor.SelectionStart;
                
                if (_lstIntelli.Visible == false)    // ... but only if not already visible
                    _lstIntelli.Visible = true;

                Graphics graphics = Graphics.FromHwnd(_lstIntelli.Handle);
                int height = 0;
                int width = _editorMinWidth;
                foreach (IntelliItem intelliItem in _lstIntelli.Items)
                {
                    SizeF itemSize = graphics.MeasureString(intelliItem.Text, _lstIntelli.Font);
                    height += (int)itemSize.Height;
                    if (itemSize.Width > width)
                        width = (int)itemSize.Width;
                }

                if (height < _editorMinHeight)
                    height = _editorMinHeight;
                if (height > _editorMaxHeight)
                    height = _editorMaxHeight;
                if (width > _editorMaxWidth)
                    width = _editorMaxWidth;
                _lstIntelli.Height = height;
                _lstIntelli.Width = width + _editorSpareWidth;

                int locationTop = activeEditor.Bottom + _activeCell.Node.TreeList.Location.Y + 5;
                if (activeEditor.Bottom + height > _activeCell.Node.TreeList.Height)
                    locationTop = activeEditor.Top + _activeCell.Node.TreeList.Location.Y - height;
                _lstIntelli.Location = new Point(activeEditor.Location.X, locationTop);

                if (_lstIntelli.Items.Count >= 1)
                    _lstIntelli.SelectedIndex = 0;
            }
            else
            {
                _lstIntelli.Visible = false; // no hit, close List again
                _lblComboBoxEditorToolTip.Visible = false;
            }
        }

        List<IntelliItem> GetIntelliItems()
        {
           List<IntelliItem> intelliItems = new List<IntelliItem>();
            intelliItems.Clear();

            try
            {
                if (_treeList.FocusedNode.Tag == null)
                    return intelliItems; //should not happen

                BaseTreeListTag nodeTag = _treeList.FocusedNode.Tag as BaseTreeListTag;
                SystemTreeListTag systemTag = null;

                //first assess which intelli-items should be shown in dependece of the edited cell ...
                List<int> specficIntelliItems = null;
                if (TreeListBuilder.IsPolicyColumn(_treeList.FocusedColumn)) //editited cell is an exceptionally editable policy-column-cell (e.g. functions DefIL, DefConst, SetDefault, etc.)
                {
                    //take system specific intelli-items (e.g. incomelists) from any existing system, as one cannot know for which system the user wants to define the parameter
                    TreeListColumn anySystemColumn = _treeList.Columns.ColumnByName(nodeTag.GetDefaultPolicyRow().SystemRow.Name);
                    if (anySystemColumn != null)
                        systemTag = anySystemColumn.Tag as SystemTreeListTag;
                    //in dependence of the function define the available intelli-items (e.g. only variables or variables and incomelists, etc.)
                    specficIntelliItems = GetIntelliItemsForEditablePolicyColumn(nodeTag.GetDefaultFunctionRow().Name);
                }
                else if (TreeListBuilder.IsSystemColumn(_treeList.FocusedColumn))
                {
                    //in dependence of the parameter-value-type define the available intelli-items (usually everything, but e.g. for output_variables only variables)
                    systemTag = _treeList.FocusedColumn.Tag as SystemTreeListTag;
                    specficIntelliItems = nodeTag.GetTypeSpecificIntelliItems(systemTag);
                }
                else
                    return intelliItems; //should not happen

                //... then gather the respective intelli-items
                //constants defined by function DefConst
                if ((specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsDefConst)) && systemTag != null)
                {
                    foreach (DataSets.CountryConfig.ParameterRow parameterRow in systemTag.GetParameterRowsConstants())
                    {
                        if (parameterRow.Name.ToLower() == DefPar.DefVar.EM2_Var_Name.ToLower())
                            intelliItems.Add(new IntelliItem(parameterRow.Value, parameterRow.Comment, _intelliImageConstant));
                        else if (parameterRow.Name.ToLower() == DefPar.DefConst.Condition.ToLower())
                            continue;
                        else
                            intelliItems.Add(new IntelliItem(parameterRow.Name, GetConstDescription(parameterRow) + " " + parameterRow.Comment, _intelliImageConstant));
                    }
                }

                //variables defined by function DefVar
                if ((specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsDefVar)) && systemTag != null)
                {
                    foreach (DataSets.CountryConfig.ParameterRow parameterRow in systemTag.GetParameterRowsDefVariables())
                    {
                        if (parameterRow.Name.ToLower() == DefPar.DefVar.EM2_Var_Name.ToLower())
                            intelliItems.Add(new IntelliItem(parameterRow.Value, parameterRow.Comment, _intelliImageVariable));
                        else
                            intelliItems.Add(new IntelliItem(parameterRow.Name, parameterRow.Comment, _intelliImageVariable));
                    }
                }

                // uprating factors
                if (specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsUpRateFactor))
                {
                    string cc = EM_AppContext.Instance.GetActiveCountryMainForm().GetCountryShortName();
                    CountryConfigFacade ccf = CountryAdministration.CountryAdministrator.GetCountryConfigFacade(cc);
                    foreach (CountryConfig.UpratingIndexRow ur in ccf.GetUpratingIndices())
                        intelliItems.Add(new IntelliItem(ur.Reference, ur.Description, _intelliImageConstant));
                }

                //standard variables defined in VarConfig (idhh, yem, poa, ...)
                if (specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsStandardVar))
                {
                    string countryShortName = systemTag == null ? string.Empty : systemTag.GetSystemRow().CountryRow.ShortName;
                    Dictionary<string, string> standardVariables = EM_AppContext.Instance.GetVarConfigFacade().GetVariables_NamesAndDescriptions(countryShortName);
                    foreach (string standardVariable in standardVariables.Keys)
                        intelliItems.Add(new IntelliItem(standardVariable, standardVariables[standardVariable], _intelliImageVariable));
                }

                //incomelists defined by function DefIL
                if ((specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsDefIL)) && systemTag != null)
                {
                    foreach (DataSets.CountryConfig.ParameterRow parameterRow in systemTag.GetParameterRowsILs())
                        intelliItems.Add(new IntelliItem(parameterRow.Value, systemTag.GetILTUComment(parameterRow), _intelliImageIL));
                }

                //queries (IsDepChild, ...)
                if (specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsQueries))
                {
                    foreach (var q in DefinitionAdmin.GetQueryNamesAndDesc())
                    {
                        string queryName = q.Key, queryDesc = q.Value; 
                        intelliItems.Add(new IntelliItem(queryName, queryDesc, _intelliImageConfiguration)); //first add normally ...
                        intelliItems.Add(new IntelliItem(_queryPrefix + queryName, //.. then add with query prefix to allow users to see all available queries (is removed before query is inserted)
                                            queryDesc, _intelliImageConfiguration));
                    }
                }

                //footnotes
                if (specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsFootnotes))
                {
                    //placeholders for new footnote parameters (#x1[_Level], #x2[_UpLim], etc.)
                    Dictionary<string, string> footnotes = GetFootnoteParametersForIntelli();
                    foreach (string footnote in footnotes.Keys)
                        intelliItems.Add(new IntelliItem(footnote, footnotes[footnote], _intelliImageFootnote));

                    //existing footnote amount parameters (#_Amount reverserd to Amount#x)
                    footnotes = TreeListManager.GetFunctionsExistingAmountParameters(_treeList.FocusedNode.ParentNode, _treeList.FocusedColumn.GetCaption());
                    foreach (string footnote in footnotes.Keys)
                        intelliItems.Add(new IntelliItem(footnote, footnotes[footnote], _intelliImageFootnote));

                    //other existing footnote parameters (#_Level, #_UpLim, etc.)
                    footnotes = TreeListManager.GetFunctionsExistingFootnoteParameters(_treeList.FocusedNode.ParentNode, _treeList.FocusedColumn.GetCaption());
                    foreach (string footnote in footnotes.Keys)
                        intelliItems.Add(new IntelliItem(footnote, footnotes[footnote], _intelliImageFootnote));
                }

                if (specficIntelliItems == null || specficIntelliItems.Contains(_intelliContainsRandAbsMinMax))
                {
                    intelliItems.Add(new IntelliItem("rand", "Random number", _intelliImageConfiguration));
                    intelliItems.Add(new IntelliItem("<abs>()", "Absolute value operator", _intelliImageConfiguration));
                    intelliItems.Add(new IntelliItem("<min> ", "Minimum operator", _intelliImageConfiguration));
                    intelliItems.Add(new IntelliItem("<max> ", "Maximum operator", _intelliImageConfiguration));
                }

                // special case for parameter DefTU/Members: show the options, e.g. Partner, OwnDepChild, ...
                if (specficIntelliItems != null && specficIntelliItems.Contains(_intelliContainsDefTUMembers))
                {
                    ParameterTreeListTag parTag = nodeTag as ParameterTreeListTag;
                    if (parTag != null)
                        foreach (string tuMember in new List<string>() { DefPar.DefTu.MEMBER_TYPE.PARTNER_CAMEL_CASE, DefPar.DefTu.MEMBER_TYPE.OWNCHILD_CAMEL_CASE,
                                    DefPar.DefTu.MEMBER_TYPE.DEPCHILD_CAMEL_CASE, DefPar.DefTu.MEMBER_TYPE.OWNDEPCHILD_CAMEL_CASE, DefPar.DefTu.MEMBER_TYPE.LOOSEDEPCHILD_CAMEL_CASE,
                                    DefPar.DefTu.MEMBER_TYPE.DEPPARENT_CAMEL_CASE, DefPar.DefTu.MEMBER_TYPE.DEPRELATIVE_CAMEL_CASE })
                            intelliItems.Add(new IntelliItem(tuMember, string.Empty, _intelliImageConfiguration));
                }

                //taxunits defined by function DefTU: only for add-ons, which use formulas for all parameters (i.e. also for taxunit parameters)
                if ((_treeList.Parent as EM_UI_MainForm)._isAddOn && systemTag != null)
                {
                    foreach (DataSets.CountryConfig.ParameterRow parameterRow in systemTag.GetParameterRowsTUs())
                        intelliItems.Add(new IntelliItem(parameterRow.Value, systemTag.GetILTUComment(parameterRow), _intelliImageTU));
                }
                return intelliItems;
            }
            catch
            {
                //do not jeopardise the UI-stability because IntelliItems cannot be gathered but try if problem can be fixed by updating the info
                UpdateInfo();
                return intelliItems;
            }

            List<int> GetIntelliItemsForEditablePolicyColumn(string functionName)
            {
                functionName = functionName.ToLower();
                List<int> items = new List<int>();
                if (functionName == DefFun.SetDefault.ToLower() || functionName == DefFun.Uprate.ToLower())
                {
                    items.Add(_intelliContainsStandardVar);
                    items.Add(_intelliContainsDefVar); //not sure whether variables defined by DefVar should be displayed (?)
                }
                if (functionName == DefFun.DefIl.ToLower())
                {
                    items.Add(_intelliContainsStandardVar);
                    items.Add(_intelliContainsDefVar);
                    items.Add(_intelliContainsDefConst);
                    items.Add(_intelliContainsDefIL);
                }
                //other functions with editable policy column (DefConst, DefVar): no intelliItems (i.e. no suggestions for e.g. constant names)
                return items;
            }
        }

        private static string GetConstDescription(CountryConfig.ParameterRow parameterRow)
        {
            var condition = from p in parameterRow.FunctionRow.GetParameterRows()
                            where p.Name.ToLower() == DefPar.DefConst.Condition.ToLower() &
                                  p.Group == parameterRow.Group
                            select p;
            return condition.Count() == 0 ? parameterRow.Value
                                          : condition.First().Value + ": " + parameterRow.Value;
        }

        // IntelliItems are drawn individually together with PopupInfo
        void lstIntelli_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            
            Rectangle bounds = e.Bounds;
            Size imageSize = _imlIntelliIcons.ImageSize;
            try
            {
                IntelliItem intelliItem = _lstIntelli.Items[e.Index] as IntelliItem;
                if (intelliItem.ImageIndex >= 0)
                {
                    _imlIntelliIcons.Draw(e.Graphics, bounds.Left, bounds.Top, intelliItem.ImageIndex);
                }
                e.Graphics.DrawString(intelliItem.Text, e.Font, new SolidBrush(e.ForeColor), bounds.Left + imageSize.Width+5, bounds.Top);
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected && intelliItem.Description != string.Empty)
                {
                    _lblComboBoxEditorToolTip.Text = intelliItem.Description;
                    _lblComboBoxEditorToolTip.Visible = true;

                    int y = _lstIntelli.Location.Y + e.Bounds.Top;
                    int x = _lstIntelli.Location.X + e.Bounds.Right + 5;

                    _lblComboBoxEditorToolTip.Location = new Point(x, y);
                    _lblComboBoxEditorToolTip.BringToFront();
                }
                else
                    _lblComboBoxEditorToolTip.Visible = false;
            }
            catch
            {
                //do not jeopardise the UI-stability because IntelliItems cannot be gathered but try if problem can be fixed by updating the info
                UpdateInfo();
            }
        }

        // when the IntellisenseListbox is closed, the chosen item must be inserted in the editor
        void lstIntelli_Leave(object sender, EventArgs e)
        {
            if (sender == null)  // if sender is null, this was called programmatically to fix the "[placeholder] bug"!
            {
                _lstIntelli.Visible = false;
                _lblComboBoxEditorToolTip.Visible = false;

                _treeList.ShowEditor();  // switch Editor on again
                return;
            }
            _lblComboBoxEditorToolTip.Visible = false;

            if (_lstIntelli.Visible == false)
                return;

            string selectedIntelliItem = string.Empty;
            bool isFootnote = false;

            if (_lstIntelli.SelectedItem != null)
            {
                selectedIntelliItem = _lstIntelli.SelectedItem.ToString().Split(':')[0];
            }
            else
            {
                if (_lstIntelli.Items.Count >= 1)    // nothing selected, but focus on first row, take it.
                    selectedIntelliItem = _lstIntelli.Items[0].ToString().Split(':')[0];
            }

            if (selectedIntelliItem.StartsWith(_queryPrefix)) //remove query prefix (which was added to allow users to see all available queries)
                selectedIntelliItem = selectedIntelliItem.Substring(1);

            if (selectedIntelliItem.StartsWith("#"))   // footnote
            {
                if (!selectedIntelliItem.ToLower().StartsWith("#x")) //for newly added footnotes keep complete text (#x1[_Level]), as needed when respective parameters are generated
                {
                    string temp = "#";
                    for (int index = 1; index < selectedIntelliItem.Length; ++index)
                        if (Char.IsDigit(selectedIntelliItem[index]))
                            temp += selectedIntelliItem[index];
                    selectedIntelliItem = temp;
                }
                isFootnote = true;
            }

            _lstIntelli.Visible = false;
            _lblComboBoxEditorToolTip.Visible = false;

            _treeList.ShowEditor();  // switch Editor on again

            string lastWord = string.Empty;

            if (_treeList.ActiveEditor != null)
            {
                int countReplaceChars = 0;
                if (selectedIntelliItem != string.Empty)
                {
                    lastWord = GetLastWord(_treeList.ActiveEditor.Text.Substring(0, _activeSelectionStart));
                    if (isFootnote && !lastWord.StartsWith("#"))
                    {
                        countReplaceChars = 0;
                        if (EndsWithFootnote(lastWord)) // replace only the footnode, not the whole word
                            countReplaceChars = lastWord.Length - lastWord.LastIndexOf('#');
                    }
                    else
                        countReplaceChars = lastWord.Length;

                    // _treeList.ActiveEditor.Text = _treeList.ActiveEditor.Text.Substring(0, _activeSelectionStart - countReplaceChars) + selectedIntelliItem + _treeList.ActiveEditor.Text.Substring(_activeSelectionStart + 1);
                    // there is a weird bug where, if the existing text is the same as the inteli selection, the cell is not considered updated and the UI does not ask you to save!
                    // hence a really dirty work-around where we always apply an improbable value and then the real one..
                    string x = _treeList.ActiveEditor.Text.Substring(0, _activeSelectionStart - countReplaceChars) + selectedIntelliItem + _treeList.ActiveEditor.Text.Substring(_activeSelectionStart + 1);
                    _treeList.ActiveEditor.Text = "%£$&@#";     // improbable value
                    _treeList.ActiveEditor.Text = x;            // real value
                    SetEditorHeight(_treeList.ActiveEditor as MemoEdit);
                }

                (_treeList.ActiveEditor as MemoEdit).SelectionStart = _activeSelectionStart - countReplaceChars + selectedIntelliItem.Length;
                (_treeList.ActiveEditor as MemoEdit).ScrollToCaret();
            }
        }

        // Doubleclick counts as selected
        void lstIntelli_DoubleClick(object sender, EventArgs e)
        {
            (sender as ListBox).Hide();
        }

        // MouseCLick counts as selected
        void lstIntelli_MouseClick(object sender, MouseEventArgs e)
        {
            (sender as ListBox).Hide();
        }

        void SetEditorHeight(MemoEdit activeEditor)
        {
            int height = _editorMinHeight;
            if (activeEditor.Bounds.Height > height)
                height = activeEditor.Bounds.Height;

            activeEditor.Bounds = new Rectangle(activeEditor.Bounds.Location, new Size(activeEditor.Bounds.Width, height));
        }

        void editor_Enter(object sender, EventArgs e)
        {
            SetEditorHeight(sender as MemoEdit);
        }

        void editor_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // this event is fired when editor is loosing focus. 
            // But also when users clicks on intelliList, in this case, don't close the list but let the List handle the click
            Point mainFormMousePosition = (_treeList.Parent as EM_UI_MainForm).GetMousePosition();
            Point intelliMousePosition = _lstIntelli.PointToClient(mainFormMousePosition);
            if (_lstIntelli.Visible && !_lstIntelli.ClientRectangle.Contains(intelliMousePosition))
            {
                _lstIntelli.Hide();
                _lblComboBoxEditorToolTip.Visible = false;
            }
        }

        void editor_KeyDown(object sender, KeyEventArgs e)
        {
            // Switch to _lstIntelli
            if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && _lstIntelli.Visible)
            {
                if (e.KeyCode == Keys.Up && _lstIntelli.SelectedIndex >= 1)
                    _lstIntelli.SelectedIndex--;

                if (e.KeyCode == Keys.Down && _lstIntelli.SelectedIndex < _lstIntelli.Items.Count - 1)
                    _lstIntelli.SelectedIndex++;

                e.SuppressKeyPress = true;
            }

            // TAB to take selected Item from _lstItelli to Editor
            if ((e.KeyCode == Keys.Tab)
                && _lstIntelli.Visible && _lstIntelli.SelectedIndex != -1)
            {
                lstIntelli_Leave(_lstIntelli, null);
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Right)
            {
                lstIntelli_Leave(null, null);   // this solves the amazing "[placeholder] bug"!!! (by simulating a mouse click after the enter is pressed)
            }
        }

       // A key is pressed for the FormulaEditor, check if IntellisenseList should be displayed     
        void  editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            MemoEdit activeEditor = sender as MemoEdit;

            List<IntelliItem> intelliList = GetIntelliItems();
            
            string lastWord = GetLastWord(activeEditor.Text.Substring(0, activeEditor.SelectionStart)) + e.KeyChar; ;

            string selectorCharacter = lastWord.ToLower();
            if (EndsWithFootnote(lastWord)) // e.g. "isMyQuery#1"
                selectorCharacter = "#";
            
            var sugQuery = from s in intelliList where s.Text.ToLower().StartsWith(selectorCharacter) select s;
            _lstIntelli.Items.Clear();
            _lstIntelli.Items.AddRange(sugQuery.ToArray<Object>());

            Handle_lstIntelli(e, activeEditor, lastWord);
        }

        // Hide IntelliList as well
        void editor_Leave(object sender, EventArgs e)
        {
            if (_lstIntelli.Visible)
            {
                lstIntelli_Leave(null, null);
                //(sender as MemoEdit).Focus();
            }
        }

        internal static int _intelliContainsDefConst = 1;
        internal static int _intelliContainsDefVar = 2;
        internal static int _intelliContainsStandardVar = 3;
        internal static int _intelliContainsDefIL = 4;
        internal static int _intelliContainsQueries = 5;
        internal static int _intelliContainsFootnotes = 6;
        internal static int _intelliContainsRandAbsMinMax = 7;
        internal static int _intelliContainsDefTUMembers = 8;
        internal static int _intelliContainsUpRateFactor = 9;

        internal FormulaEditorManager(ListBox lstIntelli, Label lblComboBoxEditorToolTip, ImageList imlIntelliIcons, TreeList treeList)
        {
            _lstIntelli = lstIntelli;
            _lstIntelli.DrawMode = DrawMode.OwnerDrawVariable;
            _lstIntelli.DrawItem += new DrawItemEventHandler(lstIntelli_DrawItem);

            lstIntelli.Leave += new EventHandler(lstIntelli_Leave);
            lstIntelli.DoubleClick += new EventHandler(lstIntelli_DoubleClick);
            lstIntelli.MouseClick += new MouseEventHandler(lstIntelli_MouseClick);
            
            _lblComboBoxEditorToolTip = lblComboBoxEditorToolTip;
            _imlIntelliIcons = imlIntelliIcons;
            _treeList = treeList;
        }

        internal RepositoryItemMemoEdit CreateEditor(TreeListNode node, TreeListColumn column)
        {
            RepositoryItemMemoEdit editor = new RepositoryItemMemoEdit();
			
            // initialize the triple_click_timer to tick every "DoubleClickTime" (default system variable for the maximum time-span of a double-click)
            triple_click_timer.Interval = System.Windows.Forms.SystemInformation.DoubleClickTime;
            triple_click_timer.Tick += triple_click_timer_Tick;

            _activeCell = new CellReferenz(node, column);

            editor.AcceptsReturn = false;
            editor.MouseUp += new MouseEventHandler(editor_MouseUp);
            editor.KeyPress += new KeyPressEventHandler(editor_KeyPress);
            editor.KeyDown += new KeyEventHandler(editor_KeyDown);
            editor.Leave += new EventHandler(editor_Leave);
            editor.Validating += new System.ComponentModel.CancelEventHandler(editor_Validating);
            editor.Enter += new EventHandler(editor_Enter);

            return editor;
        }
		
        // when the triple_click_timer ticks, reset click_count and stop the timer
        void triple_click_timer_Tick(object sender, EventArgs e)
        {
            click_count = 0;
            triple_click_timer.Enabled = false;
        }
		
        // with every new click, start the triple_click_timer and start counting clicks. On the third, select the whole cell 
        void editor_MouseUp(object sender, MouseEventArgs e)
        {
            if (!triple_click_timer.Enabled) triple_click_timer.Enabled = true;
            if (++click_count == 3) _treeList.ActiveEditor.SelectAll();
        }

        // Editor is closed, but IntelliList still open
        internal bool isStillEditing() { return _lstIntelli.Visible; }

        // Called eg when scrolling
        internal void StopIntelliList()
        {
            _lstIntelli.SelectedItem = null;
            _lstIntelli.Items.Clear();
            _lstIntelli.Visible = false;
            _lblComboBoxEditorToolTip.Visible = false;
        }

        // called when the mouse rests over a formula: if the formula contains contants, returns their value (e.g. $Const1=123, $Const2=456, etc.)
        static internal string GetDisplayText_ValueOfConstants(CountryConfigFacade ccf, SystemTreeListTag systemTag, string cellContent)
        {
            try
            {
                if (systemTag == null || cellContent == string.Empty) return string.Empty;

                string[] parts = cellContent.ToLower().Split(textSplitCharacters);

                string displayText = string.Empty;
                foreach (CountryConfig.ParameterRow constant in systemTag.GetParameterRowsConstants())
                {
                    if (parts.Contains(constant.Name.ToLower()))
                        displayText += constant.Name + " = " + GetConstDescription(constant) + Environment.NewLine;
                }
                foreach (CountryConfig.UpratingIndexRow ui in ccf.GetUpratingIndices())
                {
                    if (parts.Contains(ui.Reference.ToLower()))
                    {
                        if (string.IsNullOrEmpty(systemTag.GetSystemRow().Year) || !int.TryParse(systemTag.GetSystemRow().Year, out int year)) break;
                        Dictionary<int, double> yearValues = ccf.GetUpratingIndexYearValues(ui);
                        if (yearValues.ContainsKey(year)) displayText += $"{ui.Reference} = {yearValues[year]}";
                    }
                }
                return displayText;
            }
            catch
            {
                //do not jeopardise the UI-stability because IntelliItems cannot be gathered but try if problem can be fixed by updating the info
                UpdateInfo();
                return string.Empty;
            }
        }

        //this function is called in the (likely) case that an exception is produced because information on deleted ils, tus, consts or vars is requested
        static void UpdateInfo()
        {
            try { EM_AppContext.Instance.GetActiveCountryMainForm().GetTreeListManager().UpdateIntelliAndTUBoxInfo(); }
            catch { }
        }

        Dictionary<string, string> GetFootnoteParametersForIntelli()
        {
            Dictionary<string, string> footnoteParameters = new Dictionary<string, string>();

            DefinitionAdmin.Fun dummyFun = new DefinitionAdmin.Fun();
            DefPar.Footnote.Add(dummyFun); DefQuery.AddAllPar(dummyFun);

            foreach (string footnoteParameterName in dummyFun.GetParList().Keys)
            {
                string description = "";

                string name = footnoteParameterName;
                if (footnoteParameterName.ToLower() == DefPar.Footnote.Amount.ToLower())
                    name = DefPar.Value.AMOUNT + DefQuery.HAS_PAR_MARKER; //#_Amount -> Amount#xi
                else
                {
                    name = name.Replace("#_", DefQuery.HAS_PAR_MARKER + "[_"); //e.g. #_LowLim -> #xi[_LowLim]
                    name += "]";
                }
                footnoteParameters.Add(name, "(new) " + description); //"(new)" in contrast to "(value)" of an existing footnote-parameter

            }
            return footnoteParameters;
        }
    }
}
