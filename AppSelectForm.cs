using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AudioMixerApp
{
    public class AppTileControl : Control
    {
        public string DisplayName { get; set; }
        public string ProcessName { get; set; }
        private bool isHovered = false;

        public AppTileControl()
        {
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            this.Size = new Size(390, 65);
            this.Cursor = Cursors.Hand;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int radius = 10;
            Rectangle bounds = new Rectangle(0, 0, Width - 1, Height - 1);

            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, radius, radius, 180, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y, radius, radius, 270, 90);
            path.AddArc(bounds.X + bounds.Width - radius, bounds.Y + bounds.Height - radius, radius, radius, 0, 90);
            path.AddArc(bounds.X, bounds.Y + bounds.Height - radius, radius, radius, 90, 90);
            path.CloseAllFigures();

            Color bg = ThemeManager.IsDarkTheme ? Color.FromArgb(38, 38, 38) : Color.FromArgb(255, 255, 255);
            Color border = ThemeManager.IsDarkTheme ? Color.FromArgb(60, 60, 60) : Color.FromArgb(210, 215, 222);

            if (isHovered)
            {
                bg = ThemeManager.IsDarkTheme ? Color.FromArgb(50, 50, 50) : Color.FromArgb(230, 240, 252);
                border = ThemeManager.AccentColor;
            }

            using (SolidBrush brush = new SolidBrush(bg))
            {
                g.FillPath(brush, path);
            }

            using (Pen pen = new Pen(border, isHovered ? 1.5f : 1.0f))
            {
                g.DrawPath(pen, path);
            }

            Color textCol = ThemeManager.IsDarkTheme ? Color.White : Color.FromArgb(25, 25, 25);
            Color subTextCol = ThemeManager.IsDarkTheme ? Color.FromArgb(180, 180, 180) : Color.FromArgb(115, 115, 115);

            using (SolidBrush textBrush = new SolidBrush(textCol))
            {
                string txt = DisplayName.Length > 35 ? DisplayName.Substring(0, 32) + "..." : DisplayName;
                g.DrawString(txt, new Font("Segoe UI Semibold", 10.5F), textBrush, 15, 12);
            }

            using (SolidBrush subBrush = new SolidBrush(subTextCol))
            {
                g.DrawString(ProcessName + ".exe", new Font("Segoe UI", 8.5F), subBrush, 15, 34);
            }
        }

        protected override void OnMouseEnter(EventArgs e) { isHovered = true; Invalidate(); base.OnMouseEnter(e); }
        protected override void OnMouseLeave(EventArgs e) { isHovered = false; Invalidate(); base.OnMouseLeave(e); }
    }

    public class AppSelectorForm : Form
    {
        private FlowLayoutPanel flowPanel;
        private Label titleLabel;
        private Button closeBtn;
        public string SelectedProcess { get; private set; }
        private List<AudioSessionInfo> sessions;

        public AppSelectorForm(List<AudioSessionInfo> activeSessions, List<string> alreadyMonitored)
        {
            this.sessions = activeSessions;
            this.Size = new Size(460, 520);
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Opacity = 0;

            this.BackColor = ThemeManager.IsDarkTheme ? Color.FromArgb(24, 24, 24) : Color.FromArgb(230, 234, 238);

            Panel borderPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(15)
            };
            this.Controls.Add(borderPanel);

            titleLabel = new Label
            {
                Text = "Выберите приложение",
                Font = new Font("Segoe UI Semibold", 13F),
                ForeColor = ThemeManager.TextColor,
                Location = new Point(20, 20),
                Size = new Size(250, 30)
            };

            closeBtn = new Button
            {
                Text = "✕",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = ThemeManager.TextColor,
                BackColor = ThemeManager.IsDarkTheme ? Color.FromArgb(45, 45, 45) : Color.FromArgb(210, 215, 222),
                FlatStyle = FlatStyle.Flat,
                Location = new Point(400, 15),
                Size = new Size(32, 32)
            };
            closeBtn.FlatAppearance.BorderSize = 0;
            closeBtn.Click += (s, e) => this.Close();

            flowPanel = new FlowLayoutPanel
            {
                Location = new Point(20, 75),
                Size = new Size(415, 415),
                AutoScroll = true
            };

            borderPanel.Controls.Add(titleLabel);
            borderPanel.Controls.Add(closeBtn);
            borderPanel.Controls.Add(flowPanel);

            PopulateApps(alreadyMonitored);

            Timer animTimer = new Timer { Interval = 10 };
            animTimer.Tick += (s, e) =>
            {
                if (this.Opacity < 0.98f) this.Opacity += 0.08f;
                else animTimer.Stop();
            };
            animTimer.Start();
        }

        private void PopulateApps(List<string> alreadyMonitored)
        {
            flowPanel.Controls.Clear();
            bool hasItems = false;

            foreach (var session in sessions)
            {
                if (alreadyMonitored.Contains(session.ProcessName)) continue;
                hasItems = true;

                AppTileControl tile = new AppTileControl
                {
                    DisplayName = session.DisplayName,
                    ProcessName = session.ProcessName,
                    Margin = new Padding(0, 0, 0, 10)
                };

                tile.Click += (s, e) =>
                {
                    this.SelectedProcess = session.ProcessName;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                };

                flowPanel.Controls.Add(tile);
            }

            if (!hasItems)
            {
                Label emptyLbl = new Label
                {
                    Text = "Нет доступных аудио-сессий.\nЗапустите музыку или программу.",
                    Font = new Font("Segoe UI", 10F),
                    ForeColor = ThemeManager.SubTextColor,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(390, 100),
                    Location = new Point(0, 50)
                };
                flowPanel.Controls.Add(emptyLbl);
            }
        }
    }
}
