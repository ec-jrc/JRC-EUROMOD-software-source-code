using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Drawing;
using DevExpress.XtraEditors.Registrator;
using DevExpress.XtraEditors.Repository;
using DevExpress.XtraEditors.ViewInfo;
using DevExpress.Utils.Drawing;
using DevExpress.Skins;
using DevExpress.LookAndFeel;
using DevExpress.XtraBars;

namespace EM_UI.CustomControls
{
    [UserRepositoryItem("RegisterCustomEdit")]
    public class RepositoryItemEM_CustomToggle : RepositoryItemToggleSwitch
    {
        static RepositoryItemEM_CustomToggle() { RegisterEM_CustomToggle(); }

        public RepositoryItemEM_CustomToggle()
        {
            BrushOn = Brushes.Green;
            BrushOff = Brushes.LightGray;
        }

        public const string EM_CustomToggleName = "EM_CustomToggle";

        public override string EditorTypeName { get { return EM_CustomToggleName; } }

        public override DevExpress.Utils.HorzAlignment GlyphAlignment
        {
            get
            {
                return DevExpress.Utils.HorzAlignment.Center;
            }

        }


        private Brush _BrushOn, _BrushOff;

        public virtual Brush BrushOn
        {
            get { return _BrushOn; }
            set
            {
                if (_BrushOn != value)
                {
                    _BrushOn = value;
                    OnPropertiesChanged();
                }
            }
        }
        public virtual Brush BrushOff
        {
            get { return _BrushOff; }
            set
            {
                if (_BrushOff != value)
                {
                    _BrushOff = value;
                    OnPropertiesChanged();
                }
            }
        }

        public static void RegisterEM_CustomToggle()
        {
            EditorRegistrationInfo.Default.Editors.Add(new EditorClassInfo(EM_CustomToggleName,
                typeof(EM_CustomToggle), typeof(RepositoryItemEM_CustomToggle),
                typeof(EM_CustomToggleViewInfo), new EM_CustomToggleSwitchPainter(), true));
        }

        public override void Assign(RepositoryItem item)
        {
            BeginUpdate();
            try
            {
                base.Assign(item);
                RepositoryItemEM_CustomToggle source = item as RepositoryItemEM_CustomToggle;
                if (source == null) return;
                BrushOn = source.BrushOn;
                BrushOff = source.BrushOff;
            }
            finally
            {
                EndUpdate();
            }
        }


    }

    [ToolboxItem(true)]
    public class EM_CustomToggle : ToggleSwitch
    {

        static EM_CustomToggle() { RepositoryItemEM_CustomToggle.RegisterEM_CustomToggle(); }

        public EM_CustomToggle()
        {

        }


        public override string EditorTypeName
        {
            get
            {
                return
                    RepositoryItemEM_CustomToggle.EM_CustomToggleName;
            }
        }


        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public new RepositoryItemEM_CustomToggle Properties
        {
            get { return base.Properties as RepositoryItemEM_CustomToggle; }
        }

    }

    public class EM_CustomToggleBarEditItem : BarEditItem
    {
        static EM_CustomToggleBarEditItem() { RepositoryItemEM_CustomToggle.RegisterEM_CustomToggle(); }

        public EM_CustomToggleBarEditItem()
        {
            EditValue = false;
        }


        [System.ComponentModel.DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RepositoryItemEM_CustomToggle Properties
        {
            get { return base.Edit as RepositoryItemEM_CustomToggle; }
        }

    }

    class EM_CustomToggleSwitchPainter : ToggleSwitchPainter
    {
        public EM_CustomToggleSwitchPainter()
            : base() { }

        protected override void DrawContent(ControlGraphicsInfoArgs info)
        {
            BaseCheckEditViewInfo vi = info.ViewInfo as BaseCheckEditViewInfo;

            vi.CheckInfo.Appearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;

            ToggleObjectInfoArgs args = (vi.CheckInfo) as ToggleObjectInfoArgs;

            if ((bool)vi.EditValue == false)
            {
//                args.GlyphRect = new Rectangle(0, 0, args.GlyphRect.Width, args.GlyphRect.Height);
                vi.CheckInfo.CaptionRect = new Rectangle(args.SwitchRect.Right, args.SwitchRect.Y, args.SwitchRect.Width, args.SwitchRect.Height);
            }
            else // if (args.GlyphRect.Right == args.SwitchRect.Right)
            {
//                args.GlyphRect = new Rectangle(0, 0, args.GlyphRect.Width, args.GlyphRect.Height);
                vi.CheckInfo.CaptionRect = new Rectangle(args.GlyphRect.X, args.GlyphRect.Y, args.SwitchRect.Width, args.SwitchRect.Height);
            }

            base.DrawContent(info);
        }
    }


    public class SkinEM_CustomToggleObjectPainter : SkinToggleObjectPainter
    {

        public virtual Brush BrushOn { get; set; }
        public virtual Brush BrushOff { get; set; }

        public SkinEM_CustomToggleObjectPainter(ISkinProvider provider)
            : base(provider)
        {
        }

        protected override void DrawToggleSwitch(ToggleObjectInfoArgs args)
        {
            if (args.IsOn)
            {
                args.Graphics.FillRectangle(BrushOn, 0, 0, args.Bounds.Width, args.Bounds.Height);
            }
            else
            {
                args.Graphics.FillRectangle(BrushOff, 0, 0, args.Bounds.Width, args.Bounds.Height);
            }
        }
    }
    
    public partial class EM_ToggleButton : UserControl
    {
        public EM_ToggleButton()
        {
            InitializeComponent();
        }
    }

    public class EM_CustomToggleViewInfo : ToggleSwitchViewInfo
    {
        public EM_CustomToggleViewInfo(RepositoryItem item)
            : base(item)
        {
        }

        protected override BaseCheckObjectPainter CreateCheckPainter()
        {
            if (IsPrinting) return TogglePainterHelper.GetPainter(ActiveLookAndFeelStyle.Flat);
            SkinEM_CustomToggleObjectPainter painter = new SkinEM_CustomToggleObjectPainter(LookAndFeel);
            painter.BrushOn = ((RepositoryItemEM_CustomToggle)Item).BrushOn;
            painter.BrushOff = ((RepositoryItemEM_CustomToggle)Item).BrushOff;
            return painter;
        }

    }
}
