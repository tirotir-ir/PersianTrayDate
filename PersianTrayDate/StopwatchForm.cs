using System;
using System.Drawing;
using System.Windows.Forms;
using System.Diagnostics;

namespace PersianTrayDate
{
    public class StopwatchForm : Form
    {
        private Label lblElapsed;
        private Button btnStartStop, btnLap, btnReset, btnClose;
        private ListBox lstLaps;
        private System.Windows.Forms.Timer t;
        private Stopwatch sw = new Stopwatch();
        private int lapCount = 0;

        public StopwatchForm()
        {
            Text = "کرنومتر";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(380, 260);
            Font = new Font("Segoe UI", 10f);
            Padding = new Padding(10);

            lblElapsed = new Label { Text = "00:00:00.000", Dock = DockStyle.Top, Height = 40, TextAlign = ContentAlignment.MiddleCenter, Font = new Font(Font.FontFamily, 18f, FontStyle.Bold) };

            var panelTop = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0),
                Margin = new Padding(0),
                WrapContents = false
            };

            btnStartStop = new Button { Text = "شروع", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(12, 6, 12, 6), Margin = new Padding(4) };
            btnLap       = new Button { Text = "Lap",   AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(12, 6, 12, 6), Margin = new Padding(4), Enabled = false };
            btnReset     = new Button { Text = "Reset", AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(12, 6, 12, 6), Margin = new Padding(4), Enabled = false };
            btnClose     = new Button { Text = "بستن",  AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, Padding = new Padding(12, 6, 12, 6), Margin = new Padding(4) };
            btnStartStop.UseCompatibleTextRendering = true;
            btnClose.UseCompatibleTextRendering     = true;

            panelTop.Controls.AddRange(new Control[]{ btnStartStop, btnLap, btnReset, btnClose });

            lstLaps = new ListBox { Dock = DockStyle.Fill };

            Controls.Add(lstLaps);
            Controls.Add(panelTop);
            Controls.Add(lblElapsed);

            btnStartStop.Click += (s,e)=> {
                if (sw.IsRunning)
                {
                    sw.Stop();
                    btnStartStop.Text = "ادامه";
                    btnLap.Enabled = false;
                }
                else
                {
                    sw.Start();
                    btnStartStop.Text = "توقف";
                    btnLap.Enabled = true;
                    btnReset.Enabled = true;
                }
            };
            btnLap.Click += (s,e)=> {
                lapCount++;
                lstLaps.Items.Insert(0, $"Lap {lapCount}: {lblElapsed.Text}");
            };
            btnReset.Click += (s,e)=> {
                sw.Reset();
                lapCount = 0;
                lstLaps.Items.Clear();
                lblElapsed.Text = "00:00:00.000";
                btnStartStop.Text = "شروع";
                btnLap.Enabled = false;
                btnReset.Enabled = false;
            };
            btnClose.Click += (s,e)=> this.Close();

            t = new System.Windows.Forms.Timer { Interval = 50 };
            t.Tick += (s,e)=> {
                if (sw.IsRunning)
                {
                    var ts = sw.Elapsed;
                    lblElapsed.Text = string.Format("{0:00}:{1:00}:{2:00}.{3:000}", (int)ts.TotalHours, ts.Minutes, ts.Seconds, ts.Milliseconds);
                }
            };
            t.Start();
        }
    }
}
