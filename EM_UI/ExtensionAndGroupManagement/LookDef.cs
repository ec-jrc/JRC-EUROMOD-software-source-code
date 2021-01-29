using DevExpress.XtraTreeList;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace EM_UI.ExtensionAndGroupManagement
{
    /// <summary> class for representation of the "optics" of LookGroups and Extensions </summary>
    internal class LookDef
    {
        // varialbes for defining look
        private Color buttonColor = defaultColor; // color of the shape (square, ...) shown in spine-row-number-column
        private int buttonColorArgb { get { return buttonColor.ToArgb(); } } // color of shape as in (for storage)
        // to come:
        // internal Shape shape; // Shape-class needs to be "invented" to represent square, circle, triangle, ...
        // internal Color backgroundColor; // node-background-color in spine (actual implementation depends on possibility of solving performance problems) 
        //
        private readonly static Color defaultColor = Color.DarkSalmon;

        // constants for defining look - used in XML
        private const string BUTTON_COLOR = "BUTTON_COLOR";
        // to come:
        // private const string BACKGROUND_COLOR = "BACKGROUND_COLOR";
        // private const string SHAPE = "SHAPE";

        // for this actually a bool would be enough (as EXTENSION_ON is the same as GROUP -> draw filled shape), but the enum may make things clearer ...
        internal enum STYLE { EXTENSION_ON, EXTENSION_OFF, GROUP }

        internal LookDef(string xmlString = null)
        {
            if (xmlString == null) return;
            buttonColor = GetColorFromXml(BUTTON_COLOR, xmlString);
            // to come: backgroundColor = ...; shape = ...;
        }

        public override string ToString() { return ToXml(); }

        internal string ToXml()
        {
            string xmlString = MakeAttribute(BUTTON_COLOR, buttonColorArgb);
            // to come:
            // xmlString += MakeAttribute(BACKGROUND_COLOR, ...);
            // xmlString += MakeAttribute(SHAPE, ... square/circle/triangle ...);
            return xmlString;
        }

        // used for painting symbol in Admin-dialog
        internal void PaintGridSymbol(DataGridViewCellPaintingEventArgs e)
        {
            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
            e.Graphics.FillRectangle(new SolidBrush(buttonColor),
                                     e.CellBounds.Left + 3, e.CellBounds.Top + 3, e.CellBounds.Width - 6, e.CellBounds.Height - 6);
            // to come: paint other shapes (circle, triangle, ...)
        }

        // used for display in spine
        internal void DoTreePainting(CustomDrawNodeIndicatorEventArgs e, ref int rightPos, STYLE style)
        {
            const int sz = 9;
            if (style == STYLE.GROUP)   // fill box
            {
                Rectangle rect = new Rectangle(e.Bounds.Right - rightPos - sz, e.Bounds.Top, sz, sz);
                e.Graphics.FillRectangle(new SolidBrush(buttonColor), rect);
                // to come: paint other shapes (circle, triangle, ...)
            }
            else if (style == STYLE.EXTENSION_ON)   // check
            {
                e.Graphics.DrawLine(new Pen(buttonColor, 2), new Point(e.Bounds.Right - rightPos - sz, e.Bounds.Top + 5), new Point(e.Bounds.Right - rightPos - sz + 4, e.Bounds.Top + sz));
                e.Graphics.DrawLine(new Pen(buttonColor, 2), new Point(e.Bounds.Right - rightPos - sz + 4, e.Bounds.Top + sz), new Point(e.Bounds.Right - rightPos -1, e.Bounds.Top));
            }
            else if (style == STYLE.EXTENSION_OFF)  // x
            {
                e.Graphics.DrawLine(new Pen(buttonColor, 2), new Point(e.Bounds.Right - rightPos - sz + 2, e.Bounds.Top), new Point(e.Bounds.Right - rightPos - 1, e.Bounds.Top + sz - 2));
                e.Graphics.DrawLine(new Pen(buttonColor, 2), new Point(e.Bounds.Right - rightPos - sz + 2, e.Bounds.Top + sz - 2), new Point(e.Bounds.Right - rightPos -1, e.Bounds.Top));
            }
            rightPos += sz + 1;
        }

        // used for painting symbol in context-menus
        internal Bitmap GetMenuImage()
        {
            const int sz = 16;
            Bitmap bitmap = new Bitmap(sz, sz);
            Graphics.FromImage(bitmap).FillRectangle(new SolidBrush(buttonColor), 0, 0, sz, sz);
            return bitmap;
        }

        internal static LookDef DefineLook()
        {
            // to come: own dialog that allows defining colors, shapes, etc.
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == DialogResult.Cancel) return null;
            return new LookDef() { buttonColor = colorDialog.Color };
        }

        private static T GetAttribute<T>(string name, string xmlString)
        {
            int sPos = xmlString.IndexOf(string.Format("|{0}=", name));
            if (sPos < 0) return default(T); // assuming attribute does not exist in XML (i.e. old file)
            sPos = xmlString.IndexOf('=', sPos + 1) + 1;
            int ePos = xmlString.IndexOf('|', sPos + 1);
            string sAtt = xmlString.Substring(sPos, ePos - sPos);
            try { return (T)Convert.ChangeType(sAtt, typeof(T)); }
            catch { } // should not happen
            return default(T);
        }

        private static string MakeAttribute(string name, object content)
        {
            return string.Format("|{0}={1}|", name, content);
        }

        private static Color GetColorFromXml(string attributeName, string xmlString)
        {
            return Color.FromArgb(GetAttribute<int>(attributeName, xmlString));
        }
    }
}
