using System;
using System.Collections.Generic;
using System.Drawing;
using System.Media;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public class TrayAppContext : ApplicationContext
    {
        private NotifyIcon _tray;
        private System.Windows.Forms.Timer _clockTimer;
        private System.Windows.Forms.Timer _schedulerTimer;
        private Icon _currentIcon;
        private Dictionary<string, Icon> _iconCache = new Dictionary<string, Icon>();

        private DateTime? _timerEnd = null;
        private bool _timerSound = true;

        private DateTime? _alarmAt = null;
        private bool _alarmDaily = false;
        private bool _alarmSound = true;

        private bool _pomodoroActive = false;
        private bool _pomodoroIsFocus = true;
        private DateTime _pomodoroEnd;
        private int _pomodoroCompleted = 0;
        private TimeSpan _focusLen = TimeSpan.FromMinutes(25);
        private TimeSpan _breakLen = TimeSpan.FromMinutes(5);

        private IcsEvent _nextEvent = null;

        private HotkeyWindow _hotkeyWin;

        public TrayAppContext()
        {
            Config.Load();
            Logger.Log("App started");

            try
            {
                string exe = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                if (Config.Current.StartWithWindows) StartupManager.Enable(exe);
            } catch { }

            _tray = new NotifyIcon();
            _tray.Visible = true;
            _tray.ContextMenuStrip = BuildMenu();
            _tray.MouseClick += Tray_MouseClick;

            _hotkeyWin = new HotkeyWindow();
            _hotkeyWin.OnCopyDate += () => { CopyDateToClipboard(); };
            _hotkeyWin.ShowInTaskbar = false;
            _hotkeyWin.Opacity = 0;
            _hotkeyWin.Show(); _hotkeyWin.Hide();

            UpdateTray();

            _clockTimer = new System.Windows.Forms.Timer();
            _clockTimer.Interval = 30000;
            _clockTimer.Tick += (s,e)=> UpdateTray();
            _clockTimer.Start();

            _schedulerTimer = new System.Windows.Forms.Timer();
            _schedulerTimer.Interval = 1000;
            _schedulerTimer.Tick += Scheduler_Tick;
            _schedulerTimer.Start();

            Application.ApplicationExit += (s,e)=> Cleanup();
        }

        private ContextMenuStrip BuildMenu()
        {
            var menu = new ContextMenuStrip();
            menu.Items.Add(new ToolStripMenuItem("نمایش امروز", null, Today_Click));
            menu.Items.Add(new ToolStripMenuItem("کپی تاریخ", null, Copy_Click));
            menu.Items.Add(new ToolStripMenuItem("چسباندن تاریخ (SendKeys)", null, PasteDate_Click));
            menu.Items.Add(new ToolStripMenuItem("تقویم…", null, Calendar_Click));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("تایمر…", null, Timer_Click) { Name="TimerSet" });
            menu.Items.Add(new ToolStripMenuItem("لغو تایمر", null, TimerCancel_Click) { Name="TimerCancel", Enabled=false });
            menu.Items.Add(new ToolStripMenuItem("آلارم…", null, Alarm_Click) { Name="AlarmSet" });
            menu.Items.Add(new ToolStripMenuItem("لغو آلارم", null, AlarmCancel_Click) { Name="AlarmCancel", Enabled=false });
            menu.Items.Add(new ToolStripMenuItem("سنووز ۵ دقیقه", null, (s,e)=> SnoozeAlarm(TimeSpan.FromMinutes(5))) { Name="Snooze5", Enabled=false });
            menu.Items.Add(new ToolStripMenuItem("سنووز ۱۰ دقیقه", null, (s,e)=> SnoozeAlarm(TimeSpan.FromMinutes(10))) { Name="Snooze10", Enabled=false });
            menu.Items.Add(new ToolStripSeparator());
            var pomodoroMenu = new ToolStripMenuItem("پومودورو");
            pomodoroMenu.DropDownItems.Add("Start 25/5", null, PomodoroStart_Click);
            pomodoroMenu.DropDownItems.Add("Stop", null, PomodoroStop_Click);
            menu.Items.Add(pomodoroMenu);
            menu.Items.Add(new ToolStripMenuItem("کرنومتر…", null, Stopwatch_Click));
            menu.Items.Add(new ToolStripMenuItem("Import ICS…", null, ImportIcs_Click));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("تنظیمات…", null, Settings_Click));
            menu.Items.Add(new ToolStripMenuItem("درباره", null, About_Click));
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(new ToolStripMenuItem("خروج", null, Exit_Click));
            return menu;
        }

        private void Scheduler_Tick(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            if (_timerEnd.HasValue && now >= _timerEnd.Value)
            {
                _timerEnd = null;
                ShowBalloon("پایان تایمر", "زمان تایمر به پایان رسید.");
                if (_timerSound) SystemSounds.Exclamation.Play();
                UpdateMenuEnabled();
            }

            if (_alarmAt.HasValue && now >= _alarmAt.Value)
            {
                if (_alarmSound) SystemSounds.Beep.Play();
                ShowBalloon("زنگ آلارم", "زمان تعیین‌شده فرا رسید.");
                if (_alarmDaily)
                {
                    var t = _alarmAt.Value.TimeOfDay;
                    _alarmAt = now.Date.AddDays(1).Add(t);
                }
                else
                {
                    _alarmAt = null;
                }
                UpdateMenuEnabled();
            }

            if (_pomodoroActive && now >= _pomodoroEnd)
            {
                _pomodoroIsFocus = !_pomodoroIsFocus;
                if (_pomodoroIsFocus)
                {
                    _pomodoroEnd = now.Add(_focusLen);
                    ShowBalloon("Pomodoro", "شروع تمرکز بعدی (25 دقیقه).");
                }
                else
                {
                    _pomodoroCompleted++;
                    _pomodoroEnd = now.Add(_breakLen);
                    ShowBalloon("Pomodoro", "استراحت 5 دقیقه‌ای شروع شد.");
                }
            }

            UpdateTrayToolTipOnly();
        }

        private void SnoozeAlarm(TimeSpan span)
        {
            if (_alarmAt.HasValue)
            {
                _alarmAt = DateTime.Now.Add(span);
                UpdateMenuEnabled();
                ShowBalloon("سنووز", $"آلارم {span.TotalMinutes} دقیقه به تعویق افتاد.");
            }
        }

        private void UpdateMenuEnabled()
        {
            var menu = _tray.ContextMenuStrip;
            if (menu == null) return;
            (menu.Items["TimerCancel"] as ToolStripMenuItem).Enabled = _timerEnd.HasValue;
            (menu.Items["AlarmCancel"] as ToolStripMenuItem).Enabled = _alarmAt.HasValue;
            (menu.Items["Snooze5"] as ToolStripMenuItem).Enabled = _alarmAt.HasValue;
            (menu.Items["Snooze10"] as ToolStripMenuItem).Enabled = _alarmAt.HasValue;
        }

        private void ShowBalloon(string title, string text)
        {
            _tray.BalloonTipTitle = title;
            _tray.BalloonTipText = text;
            _tray.ShowBalloonTip(4000);
        }

        private void Tray_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Calendar_Click(sender, e);
        }

        private void Calendar_Click(object s, EventArgs e)
        {
            var f = new CalendarPopupForm();
            f.TopMost = true;
            f.StartPosition = FormStartPosition.Manual;

            var cursor = Cursor.Position;
            var desired = new Point(cursor.X - f.Width / 2, cursor.Y - f.Height - 12);

            f.Location = PopupPlacement.FitInScreen(desired, f.Size, 8);
            f.Show();
        }

        private void Today_Click(object s, EventArgs e)
        {
            var info = PersianDateUtil.GetNowInfo(Config.Current.Numerals);
            string text = info.DayOfWeekName + "\n" + info.Day + " " + info.MonthName + " " + info.Year;
            if (Config.Current.ShowWeekNumber) text += "\n" + "هفتهٔ " + info.WeekNumber;
            MessageBox.Show(text, "تاریخ امروز (هجری شمسی)", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void CopyDateToClipboard()
        {
            var info = PersianDateUtil.GetNowInfo(Config.Current.Numerals);
            string text = info.DayOfWeekName + "، " + info.Day + " " + info.MonthName + " " + info.Year;
            if (Config.Current.ShowWeekNumber) text += " — هفتهٔ " + info.WeekNumber;
            Clipboard.SetText(text);
        }

        private void Copy_Click(object s, EventArgs e) => CopyDateToClipboard();

        private void PasteDate_Click(object s, EventArgs e)
        {
            CopyDateToClipboard();
            try { SendKeys.SendWait("^v"); } catch {}
        }

        private void Exit_Click(object s, EventArgs e)
        {
            try { _tray.Icon = null; _tray.Visible = false; } catch { }
            Application.ExitThread();
        }

        private void Timer_Click(object s, EventArgs e)
        {
            using (var f = new TimerForm())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var span = new TimeSpan(0, f.Minutes, f.Seconds);
                    if (span.TotalSeconds < 1) { MessageBox.Show("زمان تایمر باید حداقل ۱ ثانیه باشد."); return; }
                    _timerEnd = DateTime.Now.Add(span);
                    _timerSound = f.PlaySound;
                    Config.Current.TimerPlaySound = f.PlaySound;
                    Config.Save();
                    UpdateMenuEnabled();
                    ShowBalloon("تایمر", "تایمر آغاز شد.");
                }
            }
        }

        private void TimerCancel_Click(object s, EventArgs e)
        {
            _timerEnd = null;
            UpdateMenuEnabled();
            ShowBalloon("تایمر", "تایمر لغو شد.");
        }

        private void Alarm_Click(object s, EventArgs e)
        {
            using (var f = new AlarmForm())
            {
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var selected = DateTime.Today.Add(f.AlarmTime.TimeOfDay);
                    if (selected <= DateTime.Now) selected = selected.AddDays(1);
                    _alarmAt = selected;
                    _alarmDaily = f.RepeatDaily;
                    _alarmSound = f.PlaySound;
                    Config.Current.AlarmPlaySound = f.PlaySound;
                    Config.Save();
                    UpdateMenuEnabled();
                    ShowBalloon("آلارم", "آلارم تنظیم شد.");
                }
            }
        }

        private void AlarmCancel_Click(object s, EventArgs e)
        {
            _alarmAt = null;
            _alarmDaily = false;
            UpdateMenuEnabled();
            ShowBalloon("آلارم", "آلارم لغو شد.");
        }

        private void PomodoroStart_Click(object s, EventArgs e)
        {
            _pomodoroActive = true;
            _pomodoroIsFocus = true;
            _pomodoroCompleted = 0;
            _pomodoroEnd = DateTime.Now.Add(_focusLen);
            ShowBalloon("Pomodoro", "تمرکز 25 دقیقه‌ای شروع شد.");
        }

        private void PomodoroStop_Click(object s, EventArgs e)
        {
            _pomodoroActive = false;
            ShowBalloon("Pomodoro", "پومودورو متوقف شد.");
        }

        private void Stopwatch_Click(object s, EventArgs e)
        {
            using (var f = new StopwatchForm())
                f.ShowDialog();
        }

        private void ImportIcs_Click(object s, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Calendar files (*.ics)|*.ics";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var eventsList = IcsImport.Parse(ofd.FileName);
                    _nextEvent = IcsImport.NextUpcoming(eventsList);
                    if (_nextEvent != null)
                        ShowBalloon("رویداد بعدی", _nextEvent.Summary + " — " + _nextEvent.Start.ToString("yyyy/MM/dd HH:mm"));
                    else
                        ShowBalloon("رویداد", "رویداد آینده‌ای یافت نشد.");
                }
            }
        }

        private Icon GetCachedIcon(string dayText)
        {
            var theme = Config.Current.AutoTheme ? ThemeManager.AutoPickTheme() : Config.Current.Theme;
            string key = dayText + "|" + theme.ToString() + "|" + Config.Current.TwoDigitDay;
            if (_iconCache.TryGetValue(key, out var ic)) return ic;
            var newIcon = IconGenerator.GenerateDayIcon(dayText, theme, Config.Current.TwoDigitDay);
            _iconCache[key] = newIcon;
            return newIcon;
        }

        private void UpdateTray()
        {
            var info = PersianDateUtil.GetNowInfo(Config.Current.Numerals);
            UpdateTooltipText(info);
            var newIcon = GetCachedIcon(info.Day);
            var old = _currentIcon;
            _tray.Icon = newIcon; _currentIcon = newIcon;
            if (old != null && old != newIcon) old.Dispose();
        }

        private void UpdateTrayToolTipOnly()
        {
            var info = PersianDateUtil.GetNowInfo(Config.Current.Numerals);
            UpdateTooltipText(info);
        }

        private void UpdateTooltipText(PersianDateUtil.PersianInfo info)
        {
            string extra = "";
            if (_timerEnd.HasValue)
            {
                var left = _timerEnd.Value - DateTime.Now;
                if (left.TotalSeconds < 0) left = TimeSpan.Zero;
                extra += " ⏱ " + ((int)left.TotalMinutes).ToString() + ":" + left.Seconds.ToString("00");
            }
            if (_alarmAt.HasValue) extra += " ⏰ " + _alarmAt.Value.ToString("HH:mm");
            if (_pomodoroActive)
                extra += _pomodoroIsFocus ? " 🔥 Focus" : " 💤 Break";

            string nextEvt = (_nextEvent != null) ? "\nرویداد بعدی: " + _nextEvent.Summary + " (" + _nextEvent.Start.ToString("MM/dd HH:mm") + ")" : "";

            _tray.Text = (info.DayOfWeekName + " " + info.Day + " " + info.MonthName + " " + info.Year + extra + nextEvt).TruncateNotifyIconText();
        }

        private void Settings_Click(object s, EventArgs e)
        {
            using (var f = new SettingsForm())
            {
                if (f.ShowDialog() == DialogResult.OK)
                    UpdateTray();
            }
        }

        private void About_Click(object s, EventArgs e)
        {
            MessageBox.Show("PersianTrayDate\nنمایش تاریخ شمسی در System Tray\ntirotir.ir", "About",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Cleanup()
        {
            try { _clockTimer?.Stop(); } catch { }
            try { _schedulerTimer?.Stop(); } catch { }

            // جدا کردن ایونت‌ها برای اطمینان
            try { if (_tray != null) _tray.MouseClick -= Tray_MouseClick; } catch { }

            // ترتیب صحیح: اول آیکن نوتیفای را خنثی کن، بعد پنهان و Dispose
            if (_tray != null)
            {
                try { _tray.Icon = null; } catch { }
                try { _tray.Visible = false; } catch { }
                try { _tray.Dispose(); } catch { }
                _tray = null;
            }

            // سپس آیکن‌های کش را آزاد کن
            if (_iconCache != null)
            {
                foreach (var kv in _iconCache)
                {
                    try { kv.Value?.Dispose(); } catch { }
                }
                _iconCache.Clear();
            }

            try { _currentIcon?.Dispose(); } catch { }  // اگر جدا از کش نگه داشته شده
            _currentIcon = null;

            try { _hotkeyWin?.Close(); _hotkeyWin?.Dispose(); } catch { }
            _hotkeyWin = null;
        }

    }

    internal static class NotifyIconTextHelper
    {
        public static string TruncateNotifyIconText(this string s)
        {
            if (s == null) return string.Empty;
            return s.Length <= 63 ? s : s.Substring(0, 63) + "…";
        }
    }
}
