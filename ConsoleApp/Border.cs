using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConsoleApp
{
    public partial class Border : Form
    {
        public Rectangle InnerRectangle;
        public Rectangle OuterRectangle;
        public int BorderWidth;
        public Color FillColor = Color.Blue;
        public Color BorderColor = Color.Blue;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cX, int cY, uint uFlags);

        public Border()
        {
            InitializeComponent();
            BackColor = FillColor;
            ForeColor = BorderColor;
            base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            base.MaximizeBox = false;
            base.MinimizeBox = false;
            base.ShowInTaskbar = false;
            base.Text = "";
            base.StartPosition = FormStartPosition.Manual;

        }
        public void HighLight(Rectangle rectangle, int borderWidth) {
            BorderWidth = borderWidth;
            SetLocation(rectangle);
            SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x43);
            Show();
        }

        private void SetLocation(Rectangle rectangle)
        {
            int width = BorderWidth;
            this.TopMost = false;
            OuterRectangle = new Rectangle(new Point(0, 0), rectangle.Size + new Size(width * 2, width * 2));
            InnerRectangle = new Rectangle(new Point(width, width), rectangle.Size);
            Region region = new Region(OuterRectangle);
            region.Exclude(InnerRectangle);
            base.Location = rectangle.Location - new Size(width, width);
            base.Size = OuterRectangle.Size;
            base.Region = region;
        }
        protected override void OnPaint(PaintEventArgs e)
        {

            Rectangle rect = new Rectangle(OuterRectangle.Left, OuterRectangle.Top, OuterRectangle.Width, OuterRectangle.Height);
            Rectangle rectangle2 = new Rectangle(InnerRectangle.Left, InnerRectangle.Top, InnerRectangle.Width, InnerRectangle.Height);
            e.Graphics.DrawRectangle(new Pen(ForeColor), rectangle2);
            e.Graphics.DrawRectangle(new Pen(ForeColor), rect);
        }
        public void Show()
        {
            SetWindowPos(base.Handle, IntPtr.Zero, 0, 0, 0, 0, 0x53);
        }
    }
}
