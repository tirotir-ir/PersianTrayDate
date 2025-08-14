using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public class HotkeyWindow : Form
    {
        [DllImport("user32.dll")] static extern bool RegisterHotKey(IntPtr hWnd,int id,uint fsModifiers,uint vk);
        [DllImport("user32.dll")] static extern bool UnregisterHotKey(IntPtr hWnd,int id);

        public event Action OnCopyDate;

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            RegisterHotKey(this.Handle, 1, 0x0002 | 0x0001, 0x44); // Ctrl+Alt+D
        }
        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            UnregisterHotKey(this.Handle, 1);
            base.OnFormClosed(e);
        }
        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == 1)
            {
                try { OnCopyDate?.Invoke(); } catch {}
            }
            base.WndProc(ref m);
        }
    }
}
