using System;
using System.Drawing;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public class AlarmForm : Form
    {
        private DateTimePicker timePicker;
        private CheckBox chkDaily, chkSound;
        private Button btnSet, btnCancel;

        public DateTime AlarmTime => timePicker.Value;
        public bool RepeatDaily => chkDaily.Checked;
        public bool PlaySound => chkSound.Checked;

        public AlarmForm()
        {
            Text = "آلارم";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(340, 170);
            Font = new Font("Segoe UI", 9f);
            Padding = new Padding(10);

            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 4 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 8));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            var lbl = new Label(){ Text="ساعت/دقیقه:", AutoSize = true, Anchor = AnchorStyles.Left };
            timePicker = new DateTimePicker(){ Format = DateTimePickerFormat.Time, ShowUpDown = true, Width = 120, Anchor = AnchorStyles.Left, RightToLeft = RightToLeft.No, Value = DateTime.Now.AddMinutes(10) };

            chkDaily = new CheckBox(){ Text="هر روز تکرار شود", AutoSize=true, Anchor=AnchorStyles.Left };
            chkSound = new CheckBox(){ Text="پخش صدا هنگام زنگ", AutoSize=true, Anchor=AnchorStyles.Left, Checked=Config.Current.AlarmPlaySound };

            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
            btnSet = new Button(){ Text="تنظیم", Width=80, DialogResult=DialogResult.OK };
            btnCancel = new Button(){ Text="انصراف", Width=80, DialogResult=DialogResult.Cancel };
            buttons.Controls.Add(btnCancel); buttons.Controls.Add(btnSet);

            table.Controls.Add(lbl, 0, 0);
            table.Controls.Add(new Panel { Width=1, Height=1 }, 1, 0);
            table.Controls.Add(timePicker, 2, 0);
            table.Controls.Add(chkDaily, 2, 1);
            table.Controls.Add(chkSound, 2, 2);
            table.Controls.Add(buttons,  2, 3);

            Controls.Add(table);

            AcceptButton = btnSet;
            CancelButton = btnCancel;
        }
    }
}
