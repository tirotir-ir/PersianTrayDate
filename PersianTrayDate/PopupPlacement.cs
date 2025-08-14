using System.Drawing;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public static class PopupPlacement
    {
        public static Point FitInScreen(Point desired, Size size, int margin = 8)
        {
            var wa = Screen.FromPoint(desired).WorkingArea;
            int x = desired.X, y = desired.Y;

            if (x + size.Width > wa.Right - margin)  x = wa.Right  - size.Width - margin;
            if (x < wa.Left + margin)                 x = wa.Left   + margin;
            if (y + size.Height > wa.Bottom - margin) y = wa.Bottom - size.Height - margin;
            if (y < wa.Top + margin)                  y = wa.Top    + margin;

            return new Point(x, y);
        }
    }
}
