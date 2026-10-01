using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace RBX_Alt_Manager.Classes
{
    internal class NBTabControl : TabControl
    {
        // https://dotnetrix.co.uk/tabcontrol.htm

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private Container components = null;

        public NBTabControl()
        {
            InitializeComponent();

            // UserPaint used to be set here (alongside DoubleBuffer/ResizeRedraw) so OnPaint
            // could redraw the selected tab's content-area border below. Verified via a
            // minimal repro that this was the actual bug: UserPaint suppresses WM_DRAWITEM
            // entirely on a .NET TabControl, so OnDrawItem (which paints the tab captions)
            // never fired - the header row rendered as a blank strip with no text, no matter
            // what DrawMode was set to. DoubleBuffer/ResizeRedraw alone are enough for the
            // OnPaint override below and don't block WM_DRAWITEM, so UserPaint is dropped.
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            DrawMode = TabDrawMode.OwnerDrawFixed;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                if (components != null)
                    components.Dispose();

            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() =>
            components = new System.ComponentModel.Container();
        #endregion

        #region Interop

        [StructLayout(LayoutKind.Sequential)]
        private struct NMHDR
        {
            public IntPtr HWND;
            public uint idFrom;
            public int code;
            public override String ToString()
            {
                return String.Format("Hwnd: {0}, ControlID: {1}, Code: {2}", HWND, idFrom, code);
            }
        }

        private const int TCN_FIRST = 0 - 550;
        private const int TCN_SELCHANGING = (TCN_FIRST - 2);

        private const int WM_USER = 0x400;
        private const int WM_NOTIFY = 0x4E;
        private const int WM_REFLECT = WM_USER + 0x1C00;

        #endregion

        #region BackColor Manipulation

        private Color m_Backcolor = Color.Empty;
        [Browsable(true), Description("The background color used to display text and graphics in a control.")]
        public override Color BackColor
        {
            get
            {
                if (m_Backcolor.Equals(Color.Empty))
                {
                    if (Parent == null)
                        return Control.DefaultBackColor;
                    else
                        return Parent.BackColor;
                }
                return m_Backcolor;
            }
            set
            {
                if (m_Backcolor.Equals(value)) return;
                m_Backcolor = value;
                Invalidate();

                base.OnBackColorChanged(EventArgs.Empty);
            }
        }

        public bool ShouldSerializeBackColor() => !m_Backcolor.Equals(Color.Empty);

        public override void ResetBackColor()
        {
            m_Backcolor = Color.Empty;
            Invalidate();
        }

        #endregion

        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            Invalidate();
        }

        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            base.OnSelectedIndexChanged(e);
            Invalidate();
        }

        // Only fills the selected tab's content area here - the header row (tab captions) is
        // handled by OnDrawItem below, since UserPaint blocks the native WM_PAINT header
        // drawing but WM_DRAWITEM (which OwnerDrawFixed triggers) still gets through and is
        // the correct place to draw owner-drawn tab captions.
        //
        // This used to also draw a 1px bevel border around the content area via
        // ControlPaint.Light/Dark(tp.BackColor, 0.7f) - meant to visually separate the tab
        // body from the native tab header next to it. On a dark theme, Light(..., 0.7f)
        // produces a distinctly light/gray line, which used to be masked by the native header
        // sitting right next to it. Once the header was replaced by TabButtonsPanel's buttons
        // (see AccountControl.cs), that border became a visible light line along the control's
        // right/bottom edge with nothing next to it to explain its color - removed since
        // HeaderPanel's own themed background already provides the surrounding fill.
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(BackColor);

            if (TabCount <= 0) return;

            Rectangle r = SelectedTab.Bounds;
            r.Inflate(3, 3);

            TabPage tp = TabPages[SelectedIndex];
            using SolidBrush PaintBrush = new SolidBrush(tp.BackColor);

            e.Graphics.FillRectangle(PaintBrush, r);
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= TabCount) return;

            TabPage tp = TabPages[e.Index];
            Rectangle r = GetTabRect(e.Index);
            bool isSelected = e.Index == SelectedIndex;

            using SolidBrush PaintBrush = new SolidBrush(tp.BackColor);
            e.Graphics.FillRectangle(PaintBrush, r);

            Color br = PaintBrush.Color.GetBrightness() < 0.4 ? ControlPaint.Light(PaintBrush.Color, isSelected ? 1f : 0.4f) : ControlPaint.Dark(PaintBrush.Color, isSelected ? 1f : 0.4f);
            ControlPaint.DrawBorder(e.Graphics, r,
                br, 1, ButtonBorderStyle.Solid,
                br, 1, ButtonBorderStyle.Solid,
                br, 1, ButtonBorderStyle.Solid,
                br, 1, isSelected ? ButtonBorderStyle.None : ButtonBorderStyle.Solid);

            PaintBrush.Color = tp.ForeColor;

            using StringFormat sf = new StringFormat
            {
                Alignment = StringAlignment.Center,
                LineAlignment = StringAlignment.Center
            };

            if (Alignment == TabAlignment.Left || Alignment == TabAlignment.Right)
            {
                float RotateAngle = 90;
                if (Alignment == TabAlignment.Left) RotateAngle = 270;
                PointF cp = new PointF(r.Left + (r.Width >> 1), r.Top + (r.Height >> 1));
                e.Graphics.TranslateTransform(cp.X, cp.Y);
                e.Graphics.RotateTransform(RotateAngle);
                r = new Rectangle(-(r.Height >> 1), -(r.Width >> 1), r.Height, r.Width);
            }

            if (tp.Enabled)
                e.Graphics.DrawString(tp.Text, Font, PaintBrush, (RectangleF)r, sf);
            else
                ControlPaint.DrawStringDisabled(e.Graphics, tp.Text, Font, tp.BackColor, (RectangleF)r, sf);

            e.Graphics.ResetTransform();
        }

        [Description("Occurs as a tab is being changed.")]
        public event SelectedTabPageChangeEventHandler SelectedIndexChanging;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (WM_REFLECT + WM_NOTIFY))
            {
                NMHDR hdr = (NMHDR)(Marshal.PtrToStructure(m.LParam, typeof(NMHDR)));
                if (hdr.code == TCN_SELCHANGING)
                {
                    TabPage tp = TestTab(PointToClient(Cursor.Position));
                    if (tp != null)
                    {
                        TabPageChangeEventArgs e = new TabPageChangeEventArgs(SelectedTab, tp);
                        if (SelectedIndexChanging != null)
                            SelectedIndexChanging(this, e);
                        if (e.Cancel || tp.Enabled == false)
                        {
                            m.Result = new IntPtr(1);
                            return;
                        }
                    }
                }
            }
            base.WndProc(ref m);
        }

        private TabPage TestTab(Point pt)
        {
            for (int index = 0; index <= TabCount - 1; index++)
                if (GetTabRect(index).Contains(pt.X, pt.Y))
                    return TabPages[index];

            return null;
        }
    }

    public class TabPageChangeEventArgs : EventArgs
    {
        private TabPage _Selected = null;
        private TabPage _PreSelected = null;
        public bool Cancel = false;

        public TabPage CurrentTab => _Selected;
        public TabPage NextTab => _PreSelected;

        public TabPageChangeEventArgs(TabPage CurrentTab, TabPage NextTab)
        {
            _Selected = CurrentTab;
            _PreSelected = NextTab;
        }
    }

    public delegate void SelectedTabPageChangeEventHandler(Object sender, TabPageChangeEventArgs e);
}