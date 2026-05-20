using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AudioMixerApp
{
    public class ModernCheckBox : Control
    {
        private bool isChecked = false;
        private bool isHovered = false;

        public event EventHandler CheckedChanged;

        public bool Checked
        {
            get => isChecked;
            set
            {
                if (isChecked != value)
                {
                    isChecked = value;
                    Invalidate();
                    CheckedChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public override string Text
        {
            get => base.Text;
            set { base.Text = value; Invalidate(); }
        }

        public ModernCheckBox()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.BackColor = Color.Transparent;
            this.Size = new Size(120, 24);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (SolidBrush bgBrush = new SolidBrush(this.Parent?.BackColor ?? ThemeManager.BackgroundColor))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            int boxSize = 16;
            int boxY = (Height - boxSize) / 2;
            Rectangle boxRect = new Rectangle(2, boxY, boxSize, boxSize);

            int radius = 4;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(boxRect.X, boxRect.Y, radius, radius, 180, 90);
            path.AddArc(boxRect.X + boxRect.Width - radius, boxRect.Y, radius, radius, 270, 90);
            path.AddArc(boxRect.X + boxRect.Width - radius, boxRect.Y + boxRect.Height - radius, radius, radius, 0, 90);
            path.AddArc(boxRect.X, boxRect.Y + boxRect.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();

            Color bg = ThemeManager.IsDarkTheme ? Color.FromArgb(50, 50, 50) : Color.FromArgb(240, 240, 240);
            Color border = isHovered ? ThemeManager.AccentColor : (ThemeManager.IsDarkTheme ? Color.FromArgb(80, 80, 80) : Color.FromArgb(180, 180, 180));

            if (isChecked)
            {
                bg = ThemeManager.AccentColor;
                border = ThemeManager.AccentColor;
            }

            using (SolidBrush brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }

            using (Pen pen = new Pen(border, 1.5f))
            {
                g.DrawPath(pen, path);
            }

            if (isChecked)
            {
                using (Pen checkPen = new Pen(Color.White, 2f))
                {
                    checkPen.StartCap = LineCap.Round;
                    checkPen.EndCap = LineCap.Round;
                    g.DrawLine(checkPen, boxRect.X + 4, boxRect.Y + 8, boxRect.X + 7, boxRect.Y + 11);
                    g.DrawLine(checkPen, boxRect.X + 7, boxRect.Y + 11, boxRect.X + 12, boxRect.Y + 5);
                }
            }

            if (!string.IsNullOrEmpty(Text))
            {
                Color textCol = ThemeManager.IsDarkTheme ? Color.White : Color.FromArgb(30, 30, 30);
                Rectangle textRect = new Rectangle(boxSize + 10, 0, Width - boxSize - 10, Height);
                TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
                TextRenderer.DrawText(g, Text, Font, textRect, textCol, flags);
            }
        }

        protected override void OnClick(EventArgs e)
        {
            Checked = !Checked;
            base.OnClick(e);
        }

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; Invalidate(); base.OnMouseLeave(e); }
    }
}
