using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.XtraEditors.Popup;
using System.Windows.Forms;
using System.Reflection;
using DevExpress.XtraBars;
using DevExpress.Utils.Drawing;
using DevExpress.XtraEditors.Controls;

namespace EM_UI.CustomControls
{
    [UserRepositoryItem("RegisterCustomEdit")]
    public class RepositoryItemEM_RadioValueEditor : RepositoryItem
    {
        public event EventHandler ControlTypeChanged;
        UserControl _drawControl;
        internal UserControl DrawControl
        {
            get { return _drawControl; }
        }
        UserControl _editorControl;
        internal UserControl EditorControl
        {
            get { return _editorControl; }
        }
        internal Dictionary<string, RepositoryItem> ControlRepositories;

        Type _controlType;
        public Type ControlType
        {
            get { return _controlType; }
            set
            {
                if (_controlType == value)
                    return;
                _controlType = value;
                ConstructorInfo cConstructor = ControlType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.NonPublic, null, new Type[] { }, null);
                _drawControl = cConstructor.Invoke(null) as UserControl;
                _editorControl = cConstructor.Invoke(null) as UserControl;
                _editorControl.Dock = DockStyle.Fill;
                UpdateControlRepositoreies();
                this.OnControlTypeChanged();
                this.OnPropertiesChanged();
            }
        }
        void OnControlTypeChanged()
        {
            if (ControlTypeChanged != null)
                ControlTypeChanged(this, EventArgs.Empty);
        }
        void UpdateControlRepositoreies()
        {
            ControlRepositories = new Dictionary<string, RepositoryItem>();
            List<BaseEdit> editorList = EditorFinder.FindEditors(_drawControl);
            foreach (Control control in editorList)
            {
                BaseEdit editor = control as BaseEdit;
                if (editor == null)
                    continue;
                ConstructorInfo cConstructor = editor.Properties.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance | BindingFlags.NonPublic, null, new Type[] { }, null);
                RepositoryItem ri = cConstructor.Invoke(null) as RepositoryItem;
                if (!ControlRepositories.ContainsKey(ri.EditorTypeName))
                    ControlRepositories.Add(ri.EditorTypeName, ri);
            }
        }
        // here
        static RepositoryItemEM_RadioValueEditor()
        {
            RegisterEM_RadioValueEditor();
        }

        public RepositoryItemEM_RadioValueEditor() : base()
        {
        }

        public const string CustomEditName = "EM_RadioValueEditor";

        public static void RegisterEM_RadioValueEditor()
        {
            EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(CustomEditName,
                typeof(EM_RadioValueEditor), typeof(RepositoryItemEM_RadioValueEditor),
                typeof(EM_RadioValueEditorViewInfo), new EM_RadioValueEditorPainter(), true, null));
        }

        public override string EditorTypeName
        {
            get
            {
                return CustomEditName;
            }
        }


        public override void Assign(RepositoryItem item)
        {
            BeginUpdate();
            try
            {
                base.Assign(item);
                ControlType = (item as RepositoryItemEM_RadioValueEditor).ControlType;
                RepositoryItemEM_RadioValueEditor source = item as RepositoryItemEM_RadioValueEditor;
                if (source == null) return;
                //
            }
            finally
            {
                EndUpdate();
            }
        }
    }

    static public class EditorFinder
    {
        static public List<BaseEdit> FindEditors(Control drawControl)
        {
            List<BaseEdit> editorList = new List<BaseEdit>();
            foreach (Control control in drawControl.Controls)
            {
                BaseEdit editor = control as BaseEdit;
                if (editor != null)
                    editorList.Add(editor);
                else
                    editorList.AddRange(FindEditors(control));
            }
            return editorList;
        }
    }

    [ToolboxItem(true)]
    public class EM_RadioValueEditor : BaseEdit, IAutoHeightControl
    {
        #region IAutoHeightControl implement
        bool IAutoHeightControl.SupportsAutoHeight { get { return true; } }

        public event EventHandler heightChanged;
        event EventHandler IAutoHeightControl.HeightChanged
        {
            add { heightChanged += value; }
            remove { heightChanged -= value; }
        }
        protected void RaiseHeightChanged()
        {
            if (heightChanged != null)
                heightChanged(this, EventArgs.Empty);
        }

        int IAutoHeightControl.CalcHeight(GraphicsCache cache)
        {
            if (ViewInfo.IsReady)
            {
                IHeightAdaptable ih = ViewInfo as IHeightAdaptable;
                if (ih != null) return ih.CalcHeight(cache, Width);
            }
            return Height;

        }
        #endregion
        static EM_RadioValueEditor()
        {
            RepositoryItemEM_RadioValueEditor.RegisterEM_RadioValueEditor();
        }

        public EM_RadioValueEditor()
            : base()
        {
            Properties.ControlTypeChanged += new EventHandler(Properties_ControlTypeChanged);
            UpdateControls();
        }

        void Properties_ControlTypeChanged(object sender, EventArgs e)
        {
            UpdateControls();
        }

        void UpdateControls()
        {
            Controls.Clear();
            if (Properties.ControlType == null)
                return;
            Controls.Add(Properties.EditorControl);
            (Properties.EditorControl as IEditValue).EditValueChanged += new EventHandler(this.editor_EditValueChanged);
            (Properties.EditorControl as IEditValue).EditValue = EditValue;
        }

        void editor_EditValueChanged(object sender, EventArgs e)
        {

            EditValue = (Properties.EditorControl as IEditValue).EditValue;
            IsModified = true;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new RepositoryItemEM_RadioValueEditor Properties
        {
            get
            {
                return base.Properties as RepositoryItemEM_RadioValueEditor;
            }
        }

        public override string EditorTypeName
        {
            get
            {
                return RepositoryItemEM_RadioValueEditor.CustomEditName;
            }
        }

        protected override void OnPropertiesChanged()
        {
            base.OnPropertiesChanged();
            this.RaiseHeightChanged();
        }

        public override object EditValue
        {
            get
            {
                return base.EditValue;
            }
            set
            {
                base.EditValue = value;
                (Properties.EditorControl as IEditValue).EditValue = EditValue;
            }
        }
    }

    public class EM_RadioValueBarEditItem : BarEditItem
    {
        static EM_RadioValueBarEditItem() { RepositoryItemEM_RadioValueEditor.RegisterEM_RadioValueEditor(); }

        public EM_RadioValueBarEditItem()
        {

        }


        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RepositoryItemEM_RadioValueEditor Properties
        {
            get { return base.Edit as RepositoryItemEM_RadioValueEditor; }
        }

    }

    public class EM_RadioValueEditorViewInfo : BaseEditViewInfo, IHeightAdaptable
    {
        public EM_RadioValueEditorViewInfo(RepositoryItem item)
            : base(item)
        {
        }

        int IHeightAdaptable.CalcHeight(GraphicsCache cache, int width)
        {
            RepositoryItemEM_RadioValueEditor cri = Item as RepositoryItemEM_RadioValueEditor;
            if (cri.ControlType == null)
                return this.CalcMinHeight(cache.Graphics);
            return cri.DrawControl.Height;
        }
        public override object EditValue
        {
            get
            {
                return base.EditValue;
            }
            set
            {
                base.EditValue = value;
            }
        }
    }

    public class EM_RadioValueEditorPainter : BaseEditPainter
    {
        public EM_RadioValueEditorPainter() : base() { }

        public override void Draw(ControlGraphicsInfoArgs info)
        {
            base.Draw(info);

            EM_RadioValueEditorViewInfo vi = info.ViewInfo as EM_RadioValueEditorViewInfo;
            RepositoryItemEM_RadioValueEditor cri = vi.Item as RepositoryItemEM_RadioValueEditor;
            if (cri.ControlType == null)
                return;
            (cri.DrawControl as IEditValue).EditValue = vi.EditValue;
            cri.DrawControl.Bounds = info.Bounds;
            Bitmap bm = new Bitmap(info.Bounds.Width, info.Bounds.Height);
			cri.DrawControl.DrawToBitmap(bm, new Rectangle(0, 0, bm.Width, bm.Height));
            info.Graphics.DrawImage(bm, info.Bounds.Location);

            List<BaseEdit> editors = new List<BaseEdit>();
            editors = EditorFinder.FindEditors(cri.DrawControl);
            DrawEditors(editors, info, cri);
        }

        void DrawEditors(List<BaseEdit> editors, ControlGraphicsInfoArgs info, RepositoryItemEM_RadioValueEditor cri)
        {
            foreach (BaseEdit editor in editors)
            {
                RepositoryItem ri = cri.ControlRepositories[editor.EditorTypeName];
                ri.Assign(editor.Properties);
                BaseEditViewInfo bevi = ri.CreateViewInfo();
                bevi.EditValue = editor.EditValue;
                Rectangle rec = editor.Bounds;

                rec.X += info.ViewInfo.Bounds.X;
                rec.Y += info.ViewInfo.Bounds.Y;

                bevi.CalcViewInfo(info.Graphics, MouseButtons.Left, new Point(0, 0), rec);
                ControlGraphicsInfoArgs cinfo = new ControlGraphicsInfoArgs(bevi, info.Cache, info.ViewInfo.Bounds);
                BaseEditPainter bp = ri.CreatePainter();
                try
                {
                    bp.Draw(cinfo);
                }
                catch { }
            }
        }

    }

    public interface IEditValue
    {
        object EditValue { get; set; }
        event EventHandler EditValueChanged;
    }

}
