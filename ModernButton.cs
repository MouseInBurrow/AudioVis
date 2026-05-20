using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AudioMixerApp
{
    public class ModernButton : Button
    {
        private bool isHovered = false;
        private bool isPressed = false;
        public bool IsActiveTab { get; set; } = false;

        public ModernButton()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int borderRadius = 8;
            Rectangle rect = new Rectangle(0, 0, Width - 1, Height - 1);

            GraphicsPath path = new GraphicsPath();
            path.AddArc(rect.X, rect.Y, borderRadius, borderRadius, 180, 90);
            path.AddArc(rect.X + rect.Width - borderRadius, rect.Y, borderRadius, borderRadius, 270, 90);
            path.AddArc(rect.X + rect.Width - borderRadius, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 0, 90);
            path.AddArc(rect.X, rect.Y + rect.Height - borderRadius, borderRadius, borderRadius, 90, 90);
            path.CloseAllFigures();

            this.Region = new Region(path);

            Color bg = ThemeManager.IsDarkTheme ? Color.FromArgb(48, 48, 48) : Color.FromArgb(224, 230, 238);
            Color border = ThemeManager.IsDarkTheme ? Color.FromArgb(65, 65, 65) : Color.FromArgb(180, 195, 210);
            Color textCol = ThemeManager.IsDarkTheme ? Color.White : Color.FromArgb(25, 25, 25);

            if (IsActiveTab)
            {
                bg = ThemeManager.AccentColor;
                border = ThemeManager.AccentColor;
                textCol = Color.White;
            }
            else if (isPressed)
            {
                bg = ThemeManager.IsDarkTheme ? Color.FromArgb(35, 35, 35) : Color.FromArgb(190, 200, 215);
            }
            else if (isHovered)
            {
                bg = ThemeManager.IsDarkTheme ? Color.FromArgb(64, 64, 64) : Color.FromArgb(205, 218, 232);
                border = ThemeManager.IsDarkTheme ? Color.FromArgb(90, 90, 90) : ThemeManager.AccentColor;
            }

            if (this.Tag?.ToString() == "Remove")
            {
                bg = isHovered ? Color.FromArgb(232, 17, 35) : (ThemeManager.IsDarkTheme ? Color.FromArgb(48, 48, 48) : Color.FromArgb(240, 240, 240));
                border = isHovered ? Color.FromArgb(232, 17, 35) : Color.FromArgb(232, 17, 35);
                textCol = isHovered ? Color.White : Color.FromArgb(232, 17, 35);
            }

            using (SolidBrush brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }

            using (Pen pen = new Pen(border, 1.0f))
            {
                g.DrawPath(pen, path);
            }

            TextFormatFlags flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis;
            TextRenderer.DrawText(g, this.Text, this.Font, rect, textCol, flags);
        }

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; isPressed = false; Invalidate(); base.OnMouseLeave(e); }
        protected override void OnMouseDown(MouseEventArgs e) { if (e.Button == MouseButtons.Left) isPressed = true; Invalidate(); base.OnMouseDown(e); }
        protected override void OnMouseUp(MouseEventArgs e) { isPressed = false; Invalidate(); base.OnMouseUp(e); }
    }
}
