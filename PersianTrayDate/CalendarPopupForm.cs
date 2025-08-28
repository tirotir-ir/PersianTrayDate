using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using System.Collections.Generic;

namespace PersianTrayDate
{
    public class CalendarPopupForm : Form
    {
        // ریشه (۳ ردیف: ماه/سال، نام روزها، روزهای ماه)
        private TableLayoutPanel root;
        private TableLayoutPanel tableHeader; // هدر نام روزها
        private TableLayoutPanel tableDays;   // جدول روزها (۶ ردیف)
        private Label lblMonthYear;

        private readonly PersianCalendar pc = new PersianCalendar();
        private HashSet<int> holidaysInThisMonth = new HashSet<int>();

        private static readonly string[] WeekdayFull =
            { "شنبه", "یکشنبه", "دوشنبه", "سه‌شنبه", "چهارشنبه", "پنجشنبه", "جمعه" };

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
            RightToLeft = RightToLeft.Yes;
            DoubleBuffered = true;

            // --- Root layout: ۳ ردیف
            root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0),
                RightToLeft = RightToLeft.Yes
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 32)); // ماه/سال
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 28)); // نام روزها
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100f)); // روزهای ماه

            // ماه/سال
            lblMonthYear = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font(Font, FontStyle.Bold)
            };
            root.Controls.Add(lblMonthYear, 0, 0);

            // هدر نام روزها
            tableHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                RightToLeft = RightToLeft.Yes
            };
            for (int c = 0; c < 7; c++)
                tableHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7f));
            root.Controls.Add(tableHeader, 0, 1);

            // جدول روزها (۶ ردیف)
            tableDays = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 7,
                RowCount = 6,
                Margin = new Padding(0),
                Padding = new Padding(0),
                RightToLeft = RightToLeft.Yes,
                GrowStyle = TableLayoutPanelGrowStyle.FixedSize
            };
            for (int c = 0; c < 7; c++)
                tableDays.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7f));
            for (int r = 0; r < 6; r++)
                tableDays.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6f));
            root.Controls.Add(tableDays, 0, 2);

            Controls.Add(root);

            BuildWeekdayHeader();
            EnsureHeaderFitsWidth();   // عرض کافی برای «چهارشنبه»
            LoadMonth(DateTime.Now);
        }

        private int WeekStartShift() =>
            Config.Current.WeekStart == WeekStartDay.Saturday ? 0 :
            Config.Current.WeekStart == WeekStartDay.Sunday ? 1 : 2;

        private int BaseSat0(DayOfWeek d) =>
            d == DayOfWeek.Saturday ? 0 :
            d == DayOfWeek.Sunday ? 1 :
            d == DayOfWeek.Monday ? 2 :
            d == DayOfWeek.Tuesday ? 3 :
            d == DayOfWeek.Wednesday ? 4 :
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
                    Margin = new Padding(0, 0, 0, 2),
                    UseCompatibleTextRendering = true
                };
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
                var l = new Label
                {
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = PersianDateUtil.ToPersianDigits(d),
                    UseCompatibleTextRendering = true
                };

                bool isFriday = MapToColumn(DayOfWeek.Friday) == c;
                if (isFriday || holidaysInThisMonth.Contains(d))
                    l.ForeColor = Color.Firebrick;

                // هایلایت امروز
                DateTime now = DateTime.Now;
                if (d == pc.GetDayOfMonth(now) && m == pc.GetMonth(now) && y == pc.GetYear(now))
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

        /// <summary>
        /// اندازهٔ عرض فرم را طوری تنظیم می‌کند که بلندترین نام روزِ هفته (مثل «چهارشنبه») در یک خط جا شود.
        /// </summary>
        private void EnsureHeaderFitsWidth()
        {
            using var f = new Font(Font, FontStyle.Bold);
            int shift = WeekStartShift();
            int maxCell = 0;

            for (int i = 0; i < 7; i++)
            {
                string name = WeekdayFull[(i + shift) % 7];
                var sz = TextRenderer.MeasureText(name, f, Size.Empty, TextFormatFlags.NoPadding);
                if (sz.Width > maxCell) maxCell = sz.Width;
            }

            int cellPadding = 8; // حاشیهٔ هر سلول
            int neededCellsWidth = (maxCell + cellPadding) * 7;
            int neededClientWidth = neededCellsWidth + this.Padding.Horizontal;

            if (this.ClientSize.Width < neededClientWidth)
                this.ClientSize = new Size(neededClientWidth, this.ClientSize.Height);
        }
    }
}
