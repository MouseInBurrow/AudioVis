using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AudioMixerApp
{
    public class CustomTrackBar : Control
    {
        private int minimum = 0;
        private int maximum = 100;
        private int value = 50;
        private bool isHovered = false;
        private bool isPressed = false;

        public event EventHandler Scroll;

        public int Minimum { get => minimum; set { minimum = value; Invalidate(); } }
        public int Maximum { get => maximum; set { maximum = value; Invalidate(); } }
        public int Value
        {
            get => value;
            set
            {
                int val = value;
                if (val < minimum) val = minimum;
                if (val > maximum) val = maximum;
                if (this.value != val)
                {
                    this.value = val;
                    Invalidate();
                    Scroll?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public CustomTrackBar()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Size = new Size(130, 24);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Color parentBg = this.Parent != null ? this.Parent.BackColor : ThemeManager.BackgroundColor;
            using (SolidBrush bgBrush = new SolidBrush(parentBg))
            {
                g.FillRectangle(bgBrush, this.ClientRectangle);
            }

            float percentage = (float)(value - minimum) / (maximum - minimum);
            int trackY = Height / 2;
            int thumbSize = 12;
            int thumbX = (int)(percentage * (Width - thumbSize - 4)) + 2;

            Color trackBg = ThemeManager.IsDarkTheme ? Color.FromArgb(70, 70, 70) : Color.FromArgb(200, 200, 200);
            using (Pen pen = new Pen(trackBg, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, 6, trackY, Width - 6, trackY);
            }

            using (Pen pen = new Pen(ThemeManager.AccentColor, 4))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, 6, trackY, thumbX + (thumbSize / 2), trackY);
            }

            Color thumbColor = isPressed ? ThemeManager.AccentColor : (isHovered ? Color.FromArgb(220, 220, 220) : Color.White);
            using (SolidBrush brush = new SolidBrush(thumbColor))
            {
                g.FillEllipse(brush, thumbX, trackY - (thumbSize / 2), thumbSize, thumbSize);
            }

            using (Pen pen = new Pen(ThemeManager.AccentColor, 1.5f))
            {
                g.DrawEllipse(pen, thumbX, trackY - (thumbSize / 2), thumbSize, thumbSize);
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isPressed = true;
                UpdateValueFromMouse(e.X);
            }
            base.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isPressed) UpdateValueFromMouse(e.X);
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e) { isPressed = false; Invalidate(); base.OnMouseUp(e); }
        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; isPressed = false; Invalidate(); base.OnMouseLeave(e); }

        private void UpdateValueFromMouse(int mouseX)
        {
            int thumbSize = 12;
            float percentage = (float)(mouseX - 6) / (Width - thumbSize - 4);
            if (percentage < 0) percentage = 0;
            if (percentage > 1) percentage = 1;
            Value = minimum + (int)(percentage * (maximum - minimum));
        }
    }
}
