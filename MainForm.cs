using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using Newtonsoft.Json;

namespace AudioMixerApp
{
    public partial class MainForm : Form
    {
        private List<AppPreset> presets = new List<AppPreset>();
        private List<string> monitoredApps = new List<string>();
        private List<AudioSessionInfo> cachedSessions = new List<AudioSessionInfo>();
        private string presetsPath = "presets.json";

        private Panel presetsPanel;
        private ListBox modernPresetList;
        private ModernButton switchToMixerBtn;
        private Button switchToPresetsBtn;
        private ModernButton createPresetBtn;
        private ModernButton addAppBtnCustom;
        private ModernButton themeToggleBtnCustom;
        private Label pTitle;

        private Panel batchActionsPanel;
        private ModernCheckBox selectAllCb;
        private ModernButton batchDeleteBtn;
        private Label batchVolLabel;
        private CustomTrackBar batchVolTrack;
        private Label batchBalLabel;
        private CustomTrackBar batchBalTrack;

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private bool isReallyClosing = false;

        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();
        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public MainForm()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;

            InitializeCustomTabs();
            InitializeBatchPanel();
            InitializeTrayIcon();
            SetupWindowDragging();
        }

        private void SetupWindowDragging()
        {
            sidebarPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    ReleaseCapture();
                    SendMessage(this.Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
                }
            };
        }

        private void InitializeTrayIcon()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Открыть", (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; });
            trayMenu.MenuItems.Add("Выход", (s, e) => { isReallyClosing = true; System.Windows.Forms.Application.Exit(); });

            trayIcon = new NotifyIcon
            {
                Text = "Audio Studio Pro",
                Icon = SystemIcons.Application,
                ContextMenu = trayMenu,
                Visible = true
            };
            trayIcon.DoubleClick += (s, e) => { this.Show(); this.WindowState = FormWindowState.Normal; };

            this.FormClosing += MainForm_FormClosing;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isReallyClosing)
            {
                e.Cancel = true;
                this.Hide();
            }
            else
            {
                trayIcon.Visible = false;
            }
        }

        private void InitializeBatchPanel()
        {
            batchActionsPanel = new Panel
            {
                Size = new Size(540, 50),
                Location = new Point(240, 0),
                BackColor = Color.Transparent,
                Visible = false
            };
            this.Controls.Add(batchActionsPanel);
            batchActionsPanel.BringToFront();

            selectAllCb = new ModernCheckBox { Text = "Выделить все", Location = new Point(10, 13), Size = new Size(110, 24), Font = new Font("Segoe UI Semibold", 9F) };
            selectAllCb.CheckedChanged += SelectAllCb_CheckedChanged;

            batchDeleteBtn = new ModernButton { Text = "Удалить", Location = new Point(125, 10), Size = new Size(80, 30), Font = new Font("Segoe UI Semibold", 8.5F) };
            batchDeleteBtn.Click += BatchDeleteBtn_Click;

            batchVolLabel = new Label { Text = "Громк.", Location = new Point(215, 17), Size = new Size(45, 15), Font = new Font("Segoe UI Semibold", 8F), Tag = "Sub" };
            batchVolTrack = new CustomTrackBar { Minimum = 0, Maximum = 100, Value = 100, Location = new Point(265, 15), Size = new Size(85, 24) };
            batchVolTrack.Scroll += (s, e) => ApplyBatchVolume(batchVolTrack.Value / 100f);

            batchBalLabel = new Label { Text = "Баланс", Location = new Point(360, 17), Size = new Size(45, 15), Font = new Font("Segoe UI Semibold", 8F), Tag = "Sub" };
            batchBalTrack = new CustomTrackBar { Minimum = -50, Maximum = 50, Value = 0, Location = new Point(410, 15), Size = new Size(85, 24) };
            batchBalTrack.Scroll += (s, e) => ApplyBatchBalance(batchBalTrack.Value / 50f);

            batchActionsPanel.Controls.Add(selectAllCb);
            batchActionsPanel.Controls.Add(batchDeleteBtn);
            batchActionsPanel.Controls.Add(batchVolLabel);
            batchActionsPanel.Controls.Add(batchVolTrack);
            batchActionsPanel.Controls.Add(batchBalLabel);
            batchActionsPanel.Controls.Add(batchBalTrack);

            appsFlowPanel.Location = new Point(240, 0);
            appsFlowPanel.Height = this.ClientSize.Height;
            appsFlowPanel.Padding = new Padding(20, 5, 20, 20);
        }

        private void InitializeCustomTabs()
        {
            sidebarPanel.Controls.Clear();

            // Кастомная минималистичная кнопка закрытия всей программы (в угол сайдбара)
            Button formCloseBtn = new Button
            {
                Location = new Point(15, 15),
                Size = new Size(24, 24),
                FlatStyle = FlatStyle.Flat,
                Name = "FormCloseBtn",
                Cursor = Cursors.Hand
            };
            formCloseBtn.FlatAppearance.BorderSize = 0;
            formCloseBtn.BackColor = ThemeManager.IsDarkTheme ? Color.FromArgb(45, 45, 45) : Color.FromArgb(210, 215, 222);
            formCloseBtn.Paint += PaintCustomCross;
            formCloseBtn.Click += (s, e) => this.Hide();
            sidebarPanel.Controls.Add(formCloseBtn);

            Label sideTitle = new Label
            {
                Text = "Audio Studio",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                Location = new Point(20, 50),
                Size = new Size(200, 35)
            };
            sidebarPanel.Controls.Add(sideTitle);

            addAppBtnCustom = new ModernButton { Text = "＋ Выбрать программу", Location = new Point(20, 105), Size = new Size(200, 42) };
            addAppBtnCustom.Click += AddAppBtn_Click;
            sidebarPanel.Controls.Add(addAppBtnCustom);

            switchToMixerBtn = new ModernButton { Text = "🔊 Микшер звука", Location = new Point(20, 175), Size = new Size(200, 42) };
            switchToPresetsBtn = new ModernButton { Text = "💾 Конфигурации", Location = new Point(20, 230), Size = new Size(200, 42) };

            switchToMixerBtn.Click += (s, e) => ShowTab(true);
            switchToPresetsBtn.Click += (s, e) => ShowTab(false);

            sidebarPanel.Controls.Add(switchToMixerBtn);
            sidebarPanel.Controls.Add(switchToPresetsBtn);

            themeToggleBtnCustom = new ModernButton { Text = "Сменить тему", Location = new Point(20, 530), Size = new Size(200, 38) };
            themeToggleBtnCustom.Click += ThemeToggleBtn_Click;
            sidebarPanel.Controls.Add(themeToggleBtnCustom);

            presetsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Visible = false,
                Padding = new Padding(30)
            };
            this.Controls.Add(presetsPanel);
            presetsPanel.BringToFront();

            pTitle = new Label { Text = "Сохраненные конфигурации", Font = new Font("Segoe UI", 16F, FontStyle.Bold), Location = new Point(30, 25), Size = new Size(400, 35) };

            modernPresetList = new ListBox
            {
                Location = new Point(30, 80),
                Size = new Size(350, 420),
                Font = new Font("Segoe UI Semibold", 10.5F),
                BorderStyle = BorderStyle.FixedSingle,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 38
            };

            modernPresetList.DrawItem += (s, e) => {
                if (e.Index < 0) return;
                bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                e.Graphics.FillRectangle(new SolidBrush(isSelected ? ThemeManager.AccentColor : ThemeManager.CardColor), e.Bounds);
                e.Graphics.DrawString(modernPresetList.Items[e.Index].ToString(), e.Font, new SolidBrush(isSelected ? Color.White : ThemeManager.TextColor), e.Bounds.X + 12, e.Bounds.Y + 9);
            };

            createPresetBtn = new ModernButton { Text = "💾 Сохранить текущий", Location = new Point(400, 80), Size = new Size(170, 42) };
            createPresetBtn.Click += SavePresetBtn_Click;

            ModernButton applyPBtn = new ModernButton { Text = "▶ Применить пресет", Location = new Point(400, 135), Size = new Size(170, 42) };
            applyPBtn.Click += LoadPresetBtn_Click;

            ModernButton deletePBtn = new ModernButton { Text = "🗑 Удалить пресет", Location = new Point(400, 190), Size = new Size(170, 42) };
            deletePBtn.Click += (s, e) => {
                if (modernPresetList.SelectedItem == null) return;
                presets.RemoveAll(p => p.PresetName == modernPresetList.SelectedItem.ToString());
                SavePresetsToFile();
                PopulatePresetCombo();
            };

            presetsPanel.Controls.Add(pTitle);
            presetsPanel.Controls.Add(modernPresetList);
            presetsPanel.Controls.Add(createPresetBtn);
            presetsPanel.Controls.Add(applyPBtn);
            presetsPanel.Controls.Add(deletePBtn);
        }

        // Общий метод прорисовки минималистичного крестика ✕
        private void PaintCustomCross(object sender, PaintEventArgs ev)
        {
            Graphics g = ev.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            Color crossColor = ThemeManager.IsDarkTheme ? Color.White : Color.FromArgb(50, 50, 50);

            int size = (sender is Button btn && btn.Name == "FormCloseBtn") ? 24 : 32;
            int offsetStart = size == 24 ? 7 : 10;
            int offsetEnd = size == 24 ? 17 : 22;

            using (Pen pen = new Pen(crossColor, 2f))
            {
                pen.StartCap = LineCap.Round;
                pen.EndCap = LineCap.Round;
                g.DrawLine(pen, offsetStart, offsetStart, offsetEnd, offsetEnd);
                g.DrawLine(pen, offsetEnd, offsetStart, offsetStart, offsetEnd);
            }
        }

        private void ShowTab(bool showMixer)
        {
            appsFlowPanel.Visible = showMixer;
            UpdateBatchPanelVisibility();
            presetsPanel.Visible = !showMixer;
            addAppBtnCustom.Visible = showMixer;

            switchToMixerBtn.Tag = showMixer ? "TabActive" : "TabInactive";
            switchToPresetsBtn.Tag = !showMixer ? "TabActive" : "TabInactive";

            ThemeManager.ApplyTheme(this);
            sidebarPanel.BackColor = ThemeManager.IsDarkTheme ? ThemeManager.SidebarColor : Color.FromArgb(225, 228, 232);
            presetsPanel.BackColor = ThemeManager.BackgroundColor;

            switchToMixerBtn.Invalidate();
            switchToPresetsBtn.Invalidate();
        }

        private void UpdateBatchPanelVisibility()
        {
            bool shouldBeVisible = appsFlowPanel.Visible && appsFlowPanel.Controls.Count >= 2;
            batchActionsPanel.Visible = shouldBeVisible;

            if (shouldBeVisible)
            {
                appsFlowPanel.Padding = new Padding(20, 55, 20, 20);
            }
            else
            {
                appsFlowPanel.Padding = new Padding(20, 5, 20, 20);
                selectAllCb.Checked = false;
            }

            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    Control[] foundCheckboxes = card.Controls.Find("itemSelectCb", true);
                    if (foundCheckboxes.Length > 0)
                    {
                        foundCheckboxes[0].Visible = shouldBeVisible;
                    }

                    Control[] foundLabels = card.Controls.Find("lblProcess", true);
                    Control[] foundSubLabels = card.Controls.Find("lblSubProcess", true);

                    int textX = shouldBeVisible ? 42 : 15;
                    if (foundLabels.Length > 0) foundLabels[0].Left = textX;
                    if (foundSubLabels.Length > 0) foundSubLabels[0].Left = textX;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadPresetsFromFile();
            cachedSessions = AudioController.GetActiveSessions();
            UpdateTheme();
            ShowTab(true);
            uiTimer.Start();
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            cachedSessions = AudioController.GetActiveSessions();
            SynchronizeTracks();
        }

        private void ThemeToggleBtn_Click(object sender, EventArgs e)
        {
            ThemeManager.IsDarkTheme = !ThemeManager.IsDarkTheme;
            UpdateTheme();
            ShowTab(appsFlowPanel.Visible);
            modernPresetList.Invalidate();
            batchVolTrack.Invalidate();
            batchBalTrack.Invalidate();
            addAppBtnCustom.Invalidate();
            themeToggleBtnCustom.Invalidate();
            createPresetBtn.Invalidate();
            selectAllCb.Invalidate();

            Control[] closeButtons = sidebarPanel.Controls.Find("FormCloseBtn", true);
            if (closeButtons.Length > 0)
            {
                closeButtons[0].BackColor = ThemeManager.IsDarkTheme ? Color.FromArgb(45, 45, 45) : Color.FromArgb(210, 215, 222);
                closeButtons[0].Invalidate();
            }

            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    card.Invalidate();
                    foreach (Control c in card.Controls)
                    {
                        if (c is ModernCheckBox mcb) mcb.Invalidate();
                    }
                }
            }
        }

        private void UpdateTheme()
        {
            ThemeManager.ApplyTheme(this);
            sidebarPanel.BackColor = ThemeManager.IsDarkTheme ? ThemeManager.SidebarColor : Color.FromArgb(225, 228, 232);
            presetsPanel.BackColor = ThemeManager.BackgroundColor;
            modernPresetList.BackColor = ThemeManager.CardColor;
            modernPresetList.ForeColor = ThemeManager.TextColor;

            if (batchVolTrack != null) batchVolTrack.BackColor = ThemeManager.BackgroundColor;
            if (batchBalTrack != null) batchBalTrack.BackColor = ThemeManager.BackgroundColor;
            if (batchActionsPanel != null) batchActionsPanel.BackColor = ThemeManager.BackgroundColor;

            if (batchVolLabel != null) batchVolLabel.ForeColor = ThemeManager.SubTextColor;
            if (batchBalLabel != null) batchBalLabel.ForeColor = ThemeManager.SubTextColor;
        }

        private void SelectAllCb_CheckedChanged(object sender, EventArgs e)
        {
            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    ModernCheckBox cb = card.Controls["itemSelectCb"] as ModernCheckBox;
                    if (cb != null) cb.Checked = selectAllCb.Checked;
                }
            }
        }

        private void BatchDeleteBtn_Click(object sender, EventArgs e)
        {
            List<Panel> toRemove = new List<Panel>();
            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    ModernCheckBox cb = card.Controls["itemSelectCb"] as ModernCheckBox;
                    if (cb != null && cb.Checked) toRemove.Add(card);
                }
            }

            foreach (Panel card in toRemove)
            {
                AudioController.SetVolume(card.Name, 1.0f);
                AudioController.SetBalance(card.Name, 0.0f);
                monitoredApps.Remove(card.Name);
                appsFlowPanel.Controls.Remove(card);
            }
            UpdateBatchPanelVisibility();
        }

        private void ApplyBatchVolume(float vol)
        {
            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    ModernCheckBox cb = card.Controls["itemSelectCb"] as ModernCheckBox;
                    if (cb != null && cb.Checked)
                    {
                        CustomTrackBar tb = card.Controls["tbVolume"] as CustomTrackBar;
                        if (tb != null) tb.Value = (int)(vol * 100);
                    }
                }
            }
        }

        private void ApplyBatchBalance(float bal)
        {
            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    ModernCheckBox cb = card.Controls["itemSelectCb"] as ModernCheckBox;
                    if (cb != null && cb.Checked)
                    {
                        CustomTrackBar tb = card.Controls["tbBalance"] as CustomTrackBar;
                        if (tb != null) tb.Value = (int)(bal * 50);
                    }
                }
            }
        }

        private void AddAppBtn_Click(object sender, EventArgs e)
        {
            using (AppSelectorForm selector = new AppSelectorForm(cachedSessions, monitoredApps))
            {
                if (selector.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(selector.SelectedProcess))
                {
                    string selectedProcess = selector.SelectedProcess;
                    monitoredApps.Add(selectedProcess);
                    CreateAppCard(cachedSessions.Find(x => x.ProcessName == selectedProcess));
                }
            }
        }

        private void CreateAppCard(AudioSessionInfo session)
        {
            if (session == null) return;

            Panel card = new Panel
            {
                Size = new Size(540, 95),
                Tag = "Card",
                Margin = new Padding(0, 0, 0, 15),
                Name = session.ProcessName,
                BorderStyle = BorderStyle.None,
                BackColor = Color.Transparent
            };

            card.Paint += (s, e) =>
            {
                Graphics g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                int r = 12;
                Rectangle rect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
                GraphicsPath p = new GraphicsPath();
                p.AddArc(rect.X, rect.Y, r, r, 180, 90);
                p.AddArc(rect.X + rect.Width - r, rect.Y, r, r, 270, 90);
                p.AddArc(rect.X + rect.Width - r, rect.Y + rect.Height - r, r, r, 0, 90);
                p.AddArc(rect.X, rect.Y + rect.Height - r, r, r, 90, 90);
                p.CloseAllFigures();

                using (SolidBrush b = new SolidBrush(ThemeManager.CardColor)) g.FillPath(b, p);
                using (Pen pen = new Pen(ThemeManager.IsDarkTheme ? Color.FromArgb(60, 60, 60) : Color.FromArgb(210, 215, 222))) g.DrawPath(pen, p);
            };

            ModernCheckBox selectCb = new ModernCheckBox { Name = "itemSelectCb", Location = new Point(12, 36), Size = new Size(24, 24), Text = "", Visible = (monitoredApps.Count >= 2) };

            int textX = (monitoredApps.Count >= 2) ? 42 : 15;
            Label nameLabel = new Label { Text = session.DisplayName.Length > 18 ? session.DisplayName.Substring(0, 15) + "..." : session.DisplayName, Location = new Point(textX, 18), Size = new Size(160, 22), Font = new Font("Segoe UI Semibold", 11F), Name = "lblProcess" };
            Label processSubLabel = new Label { Text = session.ProcessName + ".exe", Location = new Point(textX, 43), Size = new Size(160, 15), Font = new Font("Segoe UI", 8F), Tag = "Sub", Name = "lblSubProcess" };

            Label volLabel = new Label { Text = "Громкость: 100%", Location = new Point(210, 15), Size = new Size(125, 15), Font = new Font("Segoe UI Semibold", 8F), Tag = "Sub", Name = "lblVolText" };
            CustomTrackBar volTrack = new CustomTrackBar { Minimum = 0, Maximum = 100, Value = 100, Location = new Point(205, 38), Size = new Size(130, 24), Name = "tbVolume" };
            volTrack.Scroll += (s, ev) => {
                AudioController.SetVolume(session.ProcessName, volTrack.Value / 100f);
                volLabel.Text = "Громкость: " + volTrack.Value + "%";
            };

            Label balLabel = new Label { Text = "Баланс: Центр", Location = new Point(355, 15), Size = new Size(125, 15), Font = new Font("Segoe UI Semibold", 8F), Tag = "Sub", Name = "lblBalText" };
            CustomTrackBar balTrack = new CustomTrackBar { Minimum = -50, Maximum = 50, Value = 0, Location = new Point(345, 38), Size = new Size(130, 24), Name = "tbBalance" };
            balTrack.Scroll += (s, ev) => {
                AudioController.SetBalance(session.ProcessName, balTrack.Value / 50f);
                if (balTrack.Value == 0) balLabel.Text = "Баланс: Центр";
                else if (balTrack.Value < 0) balLabel.Text = "Баланс: Левее " + Math.Abs(balTrack.Value * 2) + "%";
                else balLabel.Text = "Баланс: Правее " + (balTrack.Value * 2) + "%";
            };

            Button removeBtn = new Button
            {
                Location = new Point(495, 32),
                Size = new Size(32, 32),
                FlatStyle = FlatStyle.Flat,
                Name = "RemoveButton",
                Cursor = Cursors.Hand
            };
            removeBtn.FlatAppearance.BorderSize = 0;
            removeBtn.BackColor = ThemeManager.IsDarkTheme ? Color.FromArgb(45, 45, 45) : Color.FromArgb(210, 215, 222);
            removeBtn.Paint += PaintCustomCross;

            removeBtn.Click += (s, ev) => {
                AudioController.SetVolume(session.ProcessName, 1.0f);
                AudioController.SetBalance(session.ProcessName, 0.0f);
                monitoredApps.Remove(session.ProcessName);
                appsFlowPanel.Controls.Remove(card);
                UpdateBatchPanelVisibility();
            };

            card.Controls.Add(selectCb);
            card.Controls.Add(nameLabel);
            card.Controls.Add(processSubLabel);
            card.Controls.Add(volLabel);
            card.Controls.Add(volTrack);
            card.Controls.Add(balLabel);
            card.Controls.Add(balTrack);
            card.Controls.Add(removeBtn);

            appsFlowPanel.Controls.Add(card);

            nameLabel.ForeColor = ThemeManager.IsDarkTheme ? Color.White : Color.FromArgb(25, 25, 25);
            processSubLabel.ForeColor = ThemeManager.IsDarkTheme ? Color.FromArgb(190, 190, 190) : Color.FromArgb(115, 115, 115);
            volLabel.ForeColor = ThemeManager.IsDarkTheme ? Color.FromArgb(190, 190, 190) : Color.FromArgb(115, 115, 115);
            balLabel.ForeColor = ThemeManager.IsDarkTheme ? Color.FromArgb(190, 190, 190) : Color.FromArgb(115, 115, 115);

            ThemeManager.ApplyTheme(this);
            UpdateBatchPanelVisibility();
        }

        private void SynchronizeTracks()
        {
            foreach (Control control in appsFlowPanel.Controls)
            {
                if (control is Panel card)
                {
                    bool isAlive = cachedSessions.Exists(x => x.ProcessName.Equals(card.Name, StringComparison.OrdinalIgnoreCase));
                    card.Enabled = isAlive;
                }
            }
        }

        private void SavePresetBtn_Click(object sender, EventArgs e)
        {
            using (Form dialog = new Form())
            {
                dialog.Size = new Size(320, 160);
                dialog.Text = "Сохранение пресета";
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.MaximizeBox = false;
                dialog.MinimizeBox = false;

                Label lbl = new Label { Text = "Введите название пресета:", Location = Point.Empty, Size = new Size(260, 20), Font = new Font("Segoe UI", 9.5F) };
                TextBox text = new TextBox { Location = new Point(20, 40), Width = 260, Font = new Font("Segoe UI", 10F) };
                ModernButton confirm = new ModernButton { Text = "Сохранить", Location = new Point(180, 80), Size = new Size(100, 32), DialogResult = DialogResult.OK };

                dialog.Controls.Add(lbl);
                dialog.Controls.Add(text);
                dialog.Controls.Add(confirm);
                ThemeManager.ApplyTheme(dialog);

                if (dialog.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(text.Text))
                {
                    presets.RemoveAll(p => p.PresetName == text.Text);

                    foreach (Control control in appsFlowPanel.Controls)
                    {
                        if (control is Panel card)
                        {
                            string procName = card.Name;
                            CustomTrackBar volTrack = card.Controls["tbVolume"] as CustomTrackBar;
                            CustomTrackBar balTrack = card.Controls["tbBalance"] as CustomTrackBar;

                            if (volTrack != null && balTrack != null)
                            {
                                presets.Add(new AppPreset { PresetName = text.Text, ProcessName = procName, Volume = volTrack.Value / 100f, Balance = balTrack.Value / 50f });
                            }
                        }
                    }
                    SavePresetsToFile();
                    PopulatePresetCombo();
                }
            }
        }

        private void LoadPresetBtn_Click(object sender, EventArgs e)
        {
            if (modernPresetList.SelectedItem == null) return;
            string selectedPreset = modernPresetList.SelectedItem.ToString();
            List<AppPreset> activePresets = presets.FindAll(p => p.PresetName == selectedPreset);

            foreach (AppPreset preset in activePresets)
            {
                if (!monitoredApps.Contains(preset.ProcessName))
                {
                    AudioSessionInfo session = cachedSessions.Find(x => x.ProcessName.Equals(preset.ProcessName, StringComparison.OrdinalIgnoreCase));
                    if (session != null)
                    {
                        monitoredApps.Add(preset.ProcessName);
                        CreateAppCard(session);
                    }
                }

                Control[] foundCards = appsFlowPanel.Controls.Find(preset.ProcessName, true);
                if (foundCards != null && foundCards.Length > 0)
                {
                    Panel card = foundCards[0] as Panel;
                    if (card != null)
                    {
                        CustomTrackBar volTrack = card.Controls["tbVolume"] as CustomTrackBar;
                        CustomTrackBar balTrack = card.Controls["tbBalance"] as CustomTrackBar;
                        Label volLabel = card.Controls["lblVolText"] as Label;
                        Label balLabel = card.Controls["lblBalText"] as Label;

                        if (volTrack != null && balTrack != null)
                        {
                            volTrack.Value = (int)(preset.Volume * 100);
                            balTrack.Value = (int)(preset.Balance * 50);

                            if (volLabel != null) volLabel.Text = "Громкость: " + volTrack.Value + "%";
                            if (balLabel != null)
                            {
                                if (balTrack.Value == 0) balLabel.Text = "Баланс: Центр";
                                else if (balTrack.Value < 0) balLabel.Text = "Баланс: Левее " + Math.Abs(balTrack.Value * 2) + "%";
                                else balLabel.Text = "Баланс: Правее " + (balTrack.Value * 2) + "%";
                            }

                            AudioController.SetVolume(preset.ProcessName, preset.Volume);
                            AudioController.SetBalance(preset.ProcessName, preset.Balance);
                        }
                    }
                }
            }
        }

        private void SavePresetsToFile()
        {
            string json = JsonConvert.SerializeObject(presets, Formatting.Indented);
            File.WriteAllText(presetsPath, json);
        }

        private void LoadPresetsFromFile()
        {
            if (File.Exists(presetsPath))
            {
                string json = File.ReadAllText(presetsPath);
                presets = JsonConvert.DeserializeObject<List<AppPreset>>(json) ?? new List<AppPreset>();
                PopulatePresetCombo();
            }
        }

        private void PopulatePresetCombo()
        {
            modernPresetList.Items.Clear();
            HashSet<string> uniqueNames = new HashSet<string>();
            foreach (AppPreset p in presets)
            {
                if (uniqueNames.Add(p.PresetName))
                {
                    modernPresetList.Items.Add(p.PresetName);
                }
            }
            if (modernPresetList.Items.Count > 0) modernPresetList.SelectedIndex = 0;
        }
    }
}
