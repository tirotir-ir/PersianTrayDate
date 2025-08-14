using System;
using System.Drawing;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public class SettingsForm : Form
    {
        private CheckBox chkStartup, chkTwoDigit, chkShowWeek, chkAutoTheme, chkLogging;
        private ComboBox cmbTheme, cmbNumerals, cmbWeekStart;
        private Button btnSave, btnCancel;

        public SettingsForm()
        {
            Text = "تنظیمات PersianTrayDate";
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(420, 260);
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(12);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7, AutoSize = true };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            chkStartup = new CheckBox(){ Text="اجرا همراه ویندوز", AutoSize=true, Checked=true };
            chkTwoDigit = new CheckBox(){ Text="نمایش روز با دو رقم", AutoSize=true };
            chkShowWeek= new CheckBox(){ Text="نمایش شمارهٔ هفته", AutoSize=true, Checked=true };
            chkAutoTheme= new CheckBox(){ Text="تم خودکار بر اساس ویندوز", AutoSize=true };
            chkLogging = new CheckBox(){ Text="فعال‌سازی لاگ", AutoSize=true };

            var lblTheme = new Label(){ Text="تم آیکن:", AutoSize=true, Anchor=AnchorStyles.Left };
            cmbTheme = new ComboBox(){ DropDownStyle=ComboBoxStyle.DropDownList, Width=160 };
            cmbTheme.Items.AddRange(new object[]{ "Blue","Green","Purple","Orange" });

            var lblNumerals = new Label(){ Text="ارقام:", AutoSize=true, Anchor=AnchorStyles.Left };
            cmbNumerals = new ComboBox(){ DropDownStyle=ComboBoxStyle.DropDownList, Width=160 };
            cmbNumerals.Items.AddRange(new object[]{ "Persian","Latin" });

            var lblWeekStart = new Label(){ Text="شروع هفته:", AutoSize=true, Anchor=AnchorStyles.Left };
            cmbWeekStart = new ComboBox(){ DropDownStyle=ComboBoxStyle.DropDownList, Width=160 };
            cmbWeekStart.Items.AddRange(new object[]{ "Saturday","Sunday","Monday" });

            btnSave = new Button(){ Text="ذخیره", Width=90, DialogResult=DialogResult.OK };
            btnCancel= new Button(){ Text="انصراف", Width=90, DialogResult=DialogResult.Cancel };

            table.Controls.Add(chkStartup, 0, 0); table.SetColumnSpan(chkStartup, 2);
            table.Controls.Add(chkTwoDigit, 0, 1); table.SetColumnSpan(chkTwoDigit, 2);
            table.Controls.Add(chkShowWeek,0, 2); table.SetColumnSpan(chkShowWeek,2);
            table.Controls.Add(chkAutoTheme,0,3); table.SetColumnSpan(chkAutoTheme,2);
            table.Controls.Add(chkLogging,0,4); table.SetColumnSpan(chkLogging,2);

            table.Controls.Add(lblTheme, 0, 5); table.Controls.Add(cmbTheme, 1, 5);
            table.Controls.Add(lblNumerals, 0, 6); table.Controls.Add(cmbNumerals, 1, 6);
            table.Controls.Add(lblWeekStart,0, 7); table.Controls.Add(cmbWeekStart, 1, 7);

            var panelButtons = new FlowLayoutPanel{ Dock=DockStyle.Bottom, FlowDirection=FlowDirection.RightToLeft, AutoSize=true };
            panelButtons.Controls.Add(btnCancel); panelButtons.Controls.Add(btnSave);

            Controls.Add(panelButtons);
            Controls.Add(table);

            Load += SettingsForm_Load;
            btnSave.Click += BtnSave_Click;
        }

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            chkStartup.Checked   = Config.Current.StartWithWindows || StartupManager.IsEnabled();
            chkTwoDigit.Checked  = Config.Current.TwoDigitDay;
            chkShowWeek.Checked  = Config.Current.ShowWeekNumber;
            chkAutoTheme.Checked = Config.Current.AutoTheme;
            chkLogging.Checked   = Config.Current.Logging;
            cmbTheme.SelectedItem = Config.Current.Theme.ToString();
            cmbNumerals.SelectedItem = Config.Current.Numerals.ToString();
            cmbWeekStart.SelectedItem = Config.Current.WeekStart.ToString();
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            Config.Current.StartWithWindows = chkStartup.Checked;
            Config.Current.TwoDigitDay = chkTwoDigit.Checked;
            Config.Current.ShowWeekNumber = chkShowWeek.Checked;
            Config.Current.AutoTheme = chkAutoTheme.Checked;
            Config.Current.Logging = chkLogging.Checked;

            if (Enum.TryParse<IconTheme>(cmbTheme.SelectedItem.ToString(), out var t)) Config.Current.Theme = t;
            if (Enum.TryParse<NumeralMode>(cmbNumerals.SelectedItem.ToString(), out var nm)) Config.Current.Numerals = nm;
            if (Enum.TryParse<WeekStartDay>(cmbWeekStart.SelectedItem.ToString(), out var ws)) Config.Current.WeekStart = ws;

            Config.Save();
            try {
                string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                if (chkStartup.Checked) StartupManager.Enable(exe); else StartupManager.Disable();
            } catch {}

            DialogResult = DialogResult.OK; Close();
        }
    }
}
