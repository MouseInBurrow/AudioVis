namespace AudioMixerApp
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.sidebarPanel = new System.Windows.Forms.Panel();
            this.lblPresetsTitle = new System.Windows.Forms.Label();
            this.lblAddTitle = new System.Windows.Forms.Label();
            this.appsCombo = new System.Windows.Forms.ComboBox();
            this.addAppBtn = new System.Windows.Forms.Button();
            this.lblTitle = new System.Windows.Forms.Label();
            this.themeToggleBtn = new System.Windows.Forms.Button();
            this.presetCombo = new System.Windows.Forms.ComboBox();
            this.savePresetBtn = new System.Windows.Forms.Button();
            this.loadPresetBtn = new System.Windows.Forms.Button();
            this.appsFlowPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.uiTimer = new System.Windows.Forms.Timer(this.components);
            this.sidebarPanel.SuspendLayout();
            this.SuspendLayout();

            this.sidebarPanel.Dock = System.Windows.Forms.DockStyle.Left;
            this.sidebarPanel.Location = new System.Drawing.Point(0, 0);
            this.sidebarPanel.Name = "sidebarPanel";
            this.sidebarPanel.Size = new System.Drawing.Size(240, 600);
            this.sidebarPanel.TabIndex = 0;

            this.lblTitle.Location = new System.Drawing.Point(20, 25);
            this.lblTitle.Size = new System.Drawing.Size(200, 35);
            this.lblTitle.Text = "Audio Studio";
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Display", 18F, System.Drawing.FontStyle.Bold);

            this.themeToggleBtn.Location = new System.Drawing.Point(20, 75);
            this.themeToggleBtn.Size = new System.Drawing.Size(200, 38);
            this.themeToggleBtn.Text = "Сменить тему";
            this.themeToggleBtn.Click += new System.EventHandler(this.ThemeToggleBtn_Click);

            this.lblAddTitle.Location = new System.Drawing.Point(20, 145);
            this.lblAddTitle.Size = new System.Drawing.Size(200, 20);
            this.lblAddTitle.Text = "ДОБАВИТЬ ПРОГРАММУ";
            this.lblAddTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblAddTitle.Tag = "Sub";

            this.appsCombo.Location = new System.Drawing.Point(20, 175);
            this.appsCombo.Size = new System.Drawing.Size(200, 28);
            this.appsCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.appsCombo.Font = new System.Drawing.Font("Segoe UI", 9.5F);

            this.addAppBtn.Location = new System.Drawing.Point(20, 215);
            this.addAppBtn.Size = new System.Drawing.Size(200, 38);
            this.addAppBtn.Text = "Добавить в микшер";
            this.addAppBtn.Click += new System.EventHandler(this.AddAppBtn_Click);

            this.lblPresetsTitle.Location = new System.Drawing.Point(20, 290);
            this.lblPresetsTitle.Size = new System.Drawing.Size(200, 20);
            this.lblPresetsTitle.Text = "УПРАВЛЕНИЕ ПРЕСЕТАМИ";
            this.lblPresetsTitle.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPresetsTitle.Tag = "Sub";

            this.presetCombo.Location = new System.Drawing.Point(20, 320);
            this.presetCombo.Size = new System.Drawing.Size(200, 28);
            this.presetCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.presetCombo.Font = new System.Drawing.Font("Segoe UI", 9.5F);

            this.savePresetBtn.Location = new System.Drawing.Point(20, 360);
            this.savePresetBtn.Size = new System.Drawing.Size(200, 38);
            this.savePresetBtn.Text = "Сохранить текущий";
            this.savePresetBtn.Click += new System.EventHandler(this.SavePresetBtn_Click);

            this.loadPresetBtn.Location = new System.Drawing.Point(20, 410);
            this.loadPresetBtn.Size = new System.Drawing.Size(200, 38);
            this.loadPresetBtn.Text = "Применить выбранный";
            this.loadPresetBtn.Click += new System.EventHandler(this.LoadPresetBtn_Click);

            this.sidebarPanel.Controls.Add(this.lblTitle);
            this.sidebarPanel.Controls.Add(this.themeToggleBtn);
            this.sidebarPanel.Controls.Add(this.lblAddTitle);
            this.sidebarPanel.Controls.Add(this.appsCombo);
            this.sidebarPanel.Controls.Add(this.addAppBtn);
            this.sidebarPanel.Controls.Add(this.lblPresetsTitle);
            this.sidebarPanel.Controls.Add(this.presetCombo);
            this.sidebarPanel.Controls.Add(this.savePresetBtn);
            this.sidebarPanel.Controls.Add(this.loadPresetBtn);

            this.appsFlowPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.appsFlowPanel.Location = new System.Drawing.Point(240, 0);
            this.appsFlowPanel.Name = "appsFlowPanel";
            this.appsFlowPanel.Padding = new System.Windows.Forms.Padding(20);
            this.appsFlowPanel.Size = new System.Drawing.Size(610, 600);
            this.appsFlowPanel.TabIndex = 1;
            this.appsFlowPanel.AutoScroll = true;

            this.uiTimer.Interval = 2000;
            this.uiTimer.Tick += new System.EventHandler(this.UiTimer_Tick);

            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(850, 600);
            this.Controls.Add(this.appsFlowPanel);
            this.Controls.Add(this.sidebarPanel);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.MinimumSize = new System.Drawing.Size(866, 639);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Audio Studio Pro";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.sidebarPanel.ResumeLayout(false);
            this.ResumeLayout(false);
        }

        private System.Windows.Forms.Panel sidebarPanel;
        private System.Windows.Forms.Button themeToggleBtn;
        private System.Windows.Forms.ComboBox presetCombo;
        private System.Windows.Forms.Button savePresetBtn;
        private System.Windows.Forms.Button loadPresetBtn;
        private System.Windows.Forms.ComboBox appsCombo;
        private System.Windows.Forms.Button addAppBtn;
        private System.Windows.Forms.FlowLayoutPanel appsFlowPanel;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblAddTitle;
        private System.Windows.Forms.Label lblPresetsTitle;
        private System.Windows.Forms.Timer uiTimer;
    }
}
