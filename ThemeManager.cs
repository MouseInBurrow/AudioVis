using System;
using System.Drawing;
using System.Windows.Forms;

namespace AudioMixerApp
{
    public static class ThemeManager
    {
        public static bool IsDarkTheme { get; set; } = true;

        public static Color BackgroundColor => IsDarkTheme ? Color.FromArgb(20, 20, 20) : Color.FromArgb(242, 244, 247);
        public static Color SidebarColor => IsDarkTheme ? Color.FromArgb(32, 32, 32) : Color.FromArgb(255, 255, 255);
        public static Color CardColor => IsDarkTheme ? Color.FromArgb(44, 44, 44) : Color.FromArgb(255, 255, 255);

        public static Color TextColor => IsDarkTheme ? Color.FromArgb(255, 255, 255) : Color.FromArgb(25, 25, 25);
        public static Color SidebarTextColor => IsDarkTheme ? Color.FromArgb(255, 255, 255) : Color.FromArgb(25, 25, 25);
        public static Color SubTextColor => IsDarkTheme ? Color.FromArgb(190, 190, 190) : Color.FromArgb(115, 115, 115);
        public static Color AccentColor => Color.FromArgb(0, 120, 212);

        public static void ApplyTheme(Form form)
        {
            form.BackColor = BackgroundColor;
            ApplyToControls(form.Controls);
        }

        public static void ApplyToControls(Control.ControlCollection controls)
        {
            foreach (Control control in controls)
            {
                if (control is Panel panel && panel.Tag?.ToString() == "Card")
                {
                    panel.BackColor = CardColor;
                }
                else if (control is Label lbl)
                {
                    if (lbl.Parent != null && lbl.Parent.Name == "sidebarPanel")
                    {
                        lbl.ForeColor = SidebarTextColor;
                    }
                    else
                    {
                        lbl.ForeColor = TextColor;
                    }
                }
                else if (control is ListBox lb)
                {
                    lb.BackColor = CardColor;
                    lb.ForeColor = TextColor;
                }
                else if (control is CheckBox cb)
                {
                    cb.ForeColor = TextColor;
                }
                else if (control is CustomTrackBar ctb)
                {
                    ctb.Invalidate();
                }
                else if (control is ModernButton mbtn)
                {
                    mbtn.Invalidate();
                }

                if (control.Controls.Count > 0 && control.Tag?.ToString() != "Card")
                {
                    ApplyToControls(control.Controls);
                }
            }
        }
    }
}
