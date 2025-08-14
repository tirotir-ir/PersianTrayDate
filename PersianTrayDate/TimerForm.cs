using System;
using System.Drawing;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public class TimerForm : Form
    {
        private NumericUpDown numMin, numSec;
        private CheckBox chkSound;
        private Button btnStart, btnCancel;

        public int Minutes => (int)numMin.Value;
        public int Seconds => (int)numSec.Value;
        public bool PlaySound => chkSound.Checked;

        public TimerForm()
        {
            Text = "تایمر";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;

            AutoScaleMode = AutoScaleMode.Dpi;
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(10);
            MinimumSize = new Size(320, 170);
            ClientSize  = new Size(330, 170);

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 4,
                AutoSize = false,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));

            var lbl1 = new Label { Text = "دقیقه:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0,0,0,2) };
            var lbl2 = new Label { Text = "ثانیه:", AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0,0,0,2) };

            numMin = new NumericUpDown { Minimum = 0, Maximum = 1440, Value = 5, Width = 110, Anchor = AnchorStyles.Left, RightToLeft = RightToLeft.No, TextAlign = HorizontalAlignment.Center, UpDownAlign = LeftRightAlignment.Right, Margin = new Padding(0,0,0,2) };
            numSec = new NumericUpDown { Minimum = 0, Maximum = 59, Value = 0, Width = 110, Anchor = AnchorStyles.Left, RightToLeft = RightToLeft.No, TextAlign = HorizontalAlignment.Center, UpDownAlign = LeftRightAlignment.Right, Margin = new Padding(0,0,0,2) };

            chkSound = new CheckBox { Text = "پخش صدا در پایان", Checked = Config.Current.TimerPlaySound, AutoSize = true, Anchor = AnchorStyles.Left, Margin = new Padding(0,4,0,0) };

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill, AutoSize = false, Margin = new Padding(0), Padding = new Padding(0) };
            btnStart  = new Button { Text = "شروع",   Width = 78, DialogResult = DialogResult.OK, Margin = new Padding(4,2,0,0) };
            btnCancel = new Button { Text = "انصراف", Width = 78, DialogResult = DialogResult.Cancel, Margin = new Padding(4,2,0,0) };
            buttons.Controls.Add(btnCancel); buttons.Controls.Add(btnStart);

            table.Controls.Add(lbl1, 0, 0); table.Controls.Add(new Panel { Width = 1, Height = 1 }, 1, 0); table.Controls.Add(numMin, 2, 0);
            table.Controls.Add(lbl2, 0, 1); table.Controls.Add(new Panel { Width = 1, Height = 1 }, 1, 1); table.Controls.Add(numSec, 2, 1);
            table.Controls.Add(chkSound, 2, 2);
            table.Controls.Add(buttons,  2, 3);

            Controls.Add(table);

            AcceptButton = btnStart;
            CancelButton = btnCancel;
        }
    }
}
