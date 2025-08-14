using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Windows.Forms; // برای TextRenderer

namespace PersianTrayDate
{
    public class CalendarPopupForm : Form
    {
        private TableLayoutPanel tableDays;
        private TableLayoutPanel tableHeader;
        private Label lblMonthYear;
        private PersianCalendar pc = new PersianCalendar();
        private HashSet<int> holidaysInThisMonth = new HashSet<int>();

        private static readonly string[] WeekdayFull =
            { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه" };
        private void EnsureHeaderFitsWidth()
        {
            // فونت هدر (Bold) را اندازه‌گیری می‌کنیم
            using (var f = new Font(Font, FontStyle.Bold))
            {
                int shift = WeekStartShift();
                int maxCell = 0;

                for (int i = 0; i < 7; i++)
                {
                    string name = WeekdayFull[(i + shift) % 7];
                    // اندازه‌گیری دقیق بدون Padding اضافی
                    var sz = TextRenderer.MeasureText(name, f, Size.Empty, TextFormatFlags.NoPadding);
                    if (sz.Width > maxCell) maxCell = sz.Width;
                }

                int cellPadding = 8; // کمی حاشیه برای هر سلول
                int neededCellsWidth = (maxCell + cellPadding) * 7;
                int chrome = this.Padding.Horizontal; // حاشیه‌های فرم

                int neededClientWidth = neededCellsWidth + chrome;
                if (this.ClientSize.Width < neededClientWidth)
                    this.ClientSize = new Size(neededClientWidth, this.ClientSize.Height);
            }
        }

        public CalendarPopupForm()
        {
            Text = "تقویم";
            StartPosition = FormStartPosition.Manual;
            FormBorderStyle = FormBorderStyle.FixedToolWindow;
            ShowInTaskbar = false;
            TopMost = true;
            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(8);
            MinimumSize = new Size(360, 340);
            DoubleBuffered = true;
            RightToLeft = RightToLeft.Yes;

            // Header: Month/Year
            lblMonthYear = new Label { Dock = DockStyle.Top, Height = 32, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font, FontStyle.Bold) };
            Controls.Add(lblMonthYear);

            // Weekday header (separate fixed row)
            tableHeader = new TableLayoutPanel { Dock = DockStyle.Top, Height = 28, ColumnCount = 7, RowCount = 1, Margin = new Padding(0), Padding = new Padding(0), RightToLeft = RightToLeft.Yes };
            for (int c = 0; c < 7; c++) tableHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f/7f));
            Controls.Add(tableHeader);

            // Days grid (6 rows)
            tableDays = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 7, RowCount = 6, Margin = new Padding(0), Padding = new Padding(0), RightToLeft = RightToLeft.Yes };
            for (int c = 0; c < 7; c++) tableDays.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f/7f));
            for (int r = 0; r < 6; r++) tableDays.RowStyles.Add(new RowStyle(SizeType.Percent, 100f/6f));
            Controls.Add(tableDays);

            BuildWeekdayHeader();
            BuildWeekdayHeader();
            EnsureHeaderFitsWidth();  // ← عرض را متناسب می‌کند
            LoadMonth(DateTime.Now);

            LoadMonth(DateTime.Now);
        }

        private int WeekStartShift() =>
            Config.Current.WeekStart == WeekStartDay.Saturday ? 0 :
            Config.Current.WeekStart == WeekStartDay.Sunday   ? 1 : 2;

        private int BaseSat0(DayOfWeek d) =>
            d == DayOfWeek.Saturday ? 0 :
            d == DayOfWeek.Sunday   ? 1 :
            d == DayOfWeek.Monday   ? 2 :
            d == DayOfWeek.Tuesday  ? 3 :
            d == DayOfWeek.Wednesday? 4 :
            d == DayOfWeek.Thursday ? 5 : 6;

        private int MapToColumn(DayOfWeek d)
        {
            int shift = WeekStartShift();
            return (BaseSat0(d) - shift + 7) % 7;
        }

        private void BuildWeekdayHeader()
        {
            tableHeader.SuspendLayout();
            tableHeader.Controls.Clear();
            int shift = WeekStartShift();
            for (int i = 0; i < 7; i++)
            {
                string name = WeekdayFull[(i + shift) % 7];
                var l = new Label
                {
                    Text = name,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    AutoSize = false,
                    Font = new Font(Font, FontStyle.Bold),
                    ForeColor = Color.Black,
                    Margin = new Padding(0, 0, 0, 2)
                };
                l.UseCompatibleTextRendering = true;
                tableHeader.Controls.Add(l, i, 0);
            }
            tableHeader.ResumeLayout();
        }

        public void LoadMonth(DateTime anyDate)
        {
            tableDays.SuspendLayout();
            tableDays.Controls.Clear();

            int y = pc.GetYear(anyDate);
            int m = pc.GetMonth(anyDate);

            lblMonthYear.Text = PersianDateUtil.MonthNames[m - 1] + " " + PersianDateUtil.ToPersianDigits(y);
            holidaysInThisMonth = HolidayProvider.GetHolidayDays(y, m);

            DateTime first = pc.ToDateTime(y, m, 1, 0, 0, 0, 0);
            int startCol = MapToColumn(first.DayOfWeek);
            int days = pc.GetDaysInMonth(y, m);

            int r = 0, c = startCol;
            for (int d = 1; d <= days; d++)
            {
                var l = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter };
                l.Text = PersianDateUtil.ToPersianDigits(d);
                l.UseCompatibleTextRendering = true;

                bool isFriday = MapToColumn(DayOfWeek.Friday) == c;
                if (isFriday || holidaysInThisMonth.Contains(d))
                    l.ForeColor = Color.Firebrick;

                if (d == pc.GetDayOfMonth(DateTime.Now) && m == pc.GetMonth(DateTime.Now) && y == pc.GetYear(DateTime.Now))
                {
                    l.Font = new Font(Font, FontStyle.Bold);
                    l.BackColor = Color.FromArgb(24, 144, 255);
                    l.ForeColor = Color.White;
                }
                tableDays.Controls.Add(l, c, r);
                c++;
                if (c >= 7) { c = 0; r++; }
            }
            tableDays.ResumeLayout();
        }
    }
}
