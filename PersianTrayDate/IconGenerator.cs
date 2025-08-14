using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PersianTrayDate
{
    public static class IconGenerator
    {
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        public static Icon GenerateDayIcon(string dayText, IconTheme theme, bool twoDigits, Font font = null)
        {
            string day = dayText;
            if (twoDigits && day.Length == 1) day = "۰" + day;

            Size size = SystemInformation.SmallIconSize;
            int W = Math.Clamp(size.Width, 16, 32);
            int H = W;

            using (var bmp = new Bitmap(W, H))
            {
                bmp.SetResolution(96, 96);

                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

                    int radius = (W >= 24) ? 5 : 3;
                    using (var path = RoundedRect(new Rectangle(0, 0, W - 1, H - 1), radius))
                    using (var brush = new LinearGradientBrush(new Rectangle(0, 0, W, H),
                        ThemeStart(theme), ThemeEnd(theme), LinearGradientMode.ForwardDiagonal))
                    {
                        g.FillPath(brush, path);
                        using (var borderPen = new Pen(Color.FromArgb(230, 255, 255, 255), 1f))
                            g.DrawPath(borderPen, path);
                    }

                    float padding = (W <= 18) ? 0f : 1f;
                    var content = new RectangleF(padding, padding, W - padding * 2, H - padding * 2);

                    using (var ff = new FontFamily("Segoe UI"))
                    using (var sf = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.NoClip)
                    { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    {
                        if (day.Length == 2)
                        {
                            // Split into two equal cells with tiny gap
                            float gap = (W <= 18) ? 0f : 0.5f;
                            var left  = new RectangleF(content.X, content.Y, (content.Width - gap) / 2f, content.Height);
                            var right = new RectangleF(left.Right + gap, content.Y, left.Width, content.Height);

                            DrawDigit(g, ff, sf, day[0].ToString(), left,  W);
                            DrawDigit(g, ff, sf, day[1].ToString(), right, W);
                        }
                        else
                        {
                            DrawDigit(g, ff, sf, day, content, W);
                        }
                    }
                }

                IntPtr hIcon = bmp.GetHicon();
                try
                {
                    using (var tmp = Icon.FromHandle(hIcon))
                        return (Icon)tmp.Clone();
                }
                finally { DestroyIcon(hIcon); }
            }
        }

        private static void DrawDigit(Graphics g, FontFamily ff, StringFormat sf, string ch, RectangleF rect, int W)
        {
            using (var gp = new GraphicsPath())
            {
                float em = (W >= 24) ? 16f : 12.4f;
                gp.AddString(ch, ff, (int)FontStyle.Bold, em, rect, sf);
                var b = gp.GetBounds();

                float sx = rect.Width  / Math.Max(1, b.Width);
                float sy = rect.Height / Math.Max(1, b.Height);
                float s  = Math.Min(sx, sy) * 0.92f;

                using (var m = new Matrix())
                {
                    m.Translate(-b.X, -b.Y);
                    m.Scale(s, s, MatrixOrder.Append);
                    var bw = b.Width * s; var bh = b.Height * s;
                    float tx = rect.X + (rect.Width  - bw) / 2f;
                    float ty = rect.Y + (rect.Height - bh) / 2f;
                    m.Translate(tx, ty, MatrixOrder.Append);
                    gp.Transform(m);
                }

                // Shadow
                using (var shadow = (GraphicsPath)gp.Clone())
                using (var m2 = new Matrix())
                using (var shadowBrush = new SolidBrush(Color.FromArgb(90, 0, 0, 0)))
                {
                    m2.Translate(1f, 1f); shadow.Transform(m2);
                    g.FillPath(shadowBrush, shadow);
                }

                float outlineW = Math.Max(1f, W / 18f);
                using (var outline = new Pen(Color.FromArgb(255, 20, 20, 20), outlineW)
                { LineJoin = LineJoin.Round, StartCap = LineCap.Round, EndCap = LineCap.Round })
                    g.DrawPath(outline, gp);
                using (var fill = new SolidBrush(Color.White))
                    g.FillPath(fill, gp);
            }
        }

        private static Color ThemeStart(IconTheme t) =>
            t==IconTheme.Green ? Color.FromArgb(255,46,204,113) :
            t==IconTheme.Purple? Color.FromArgb(255,155,89,182) :
            t==IconTheme.Orange? Color.FromArgb(255,243,156,18) :
                                  Color.FromArgb(255,52,152,219);

        private static Color ThemeEnd(IconTheme t) =>
            t==IconTheme.Green ? Color.FromArgb(255,39,174,96) :
            t==IconTheme.Purple? Color.FromArgb(255,142,68,173) :
            t==IconTheme.Orange? Color.FromArgb(255,211,84,0) :
                                  Color.FromArgb(255,41,128,185);

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
