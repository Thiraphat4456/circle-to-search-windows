using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace CircleToSearch
{
    static class Icons
    {
        public static Icon MakeTrayIcon()
        {
            int s = 32;
            using (var bmp = new Bitmap(s, s, PixelFormat.Format32bppArgb))
            {
                using (var g = Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(Color.Transparent);
                    var r = new RectangleF(3, 3, s - 6, s - 6);
                    using (var br = UI.Gradient(r))
                    using (var pen = new Pen(br, 4.5f))
                    {
                        pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                        g.DrawArc(pen, r, -60, 300);
                    }
                    using (var dot = new SolidBrush(Color.White))
                        g.FillEllipse(dot, s / 2f - 4, s / 2f - 4, 8, 8);
                }
                IntPtr h = bmp.GetHicon();
                var ic = (Icon)Icon.FromHandle(h).Clone();
                Native.DestroyIcon(h);
                return ic;
            }
        }

        public enum Kind { Lasso, Rect, Full, Close, Search, Copy, Save }

        public static void Draw(Graphics g, Kind k, RectangleF r, Color c)
        {
            using (var pen = new Pen(c, 1.8f))
            {
                pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round; pen.LineJoin = LineJoin.Round;
                float cx = r.X + r.Width / 2, cy = r.Y + r.Height / 2, h = r.Width / 2;
                switch (k)
                {
                    case Kind.Lasso:
                        g.DrawArc(pen, cx - h, cy - h * 0.8f, h * 2, h * 1.6f, -40, 320);
                        g.DrawLine(pen, cx + h * 0.55f, cy - h * 0.55f, cx + h * 0.95f, cy - h * 0.1f);
                        break;
                    case Kind.Rect:
                        pen.DashStyle = DashStyle.Dash;
                        g.DrawRectangle(pen, cx - h, cy - h * 0.75f, h * 2, h * 1.5f);
                        break;
                    case Kind.Full:
                        g.DrawRectangle(pen, cx - h, cy - h * 0.7f, h * 2, h * 1.4f);
                        g.DrawLine(pen, cx - h * 0.4f, cy + h * 0.95f, cx + h * 0.4f, cy + h * 0.95f);
                        break;
                    case Kind.Close:
                        g.DrawLine(pen, cx - h * 0.7f, cy - h * 0.7f, cx + h * 0.7f, cy + h * 0.7f);
                        g.DrawLine(pen, cx + h * 0.7f, cy - h * 0.7f, cx - h * 0.7f, cy + h * 0.7f);
                        break;
                    case Kind.Search:
                        g.DrawEllipse(pen, cx - h * 0.85f, cy - h * 0.85f, h * 1.3f, h * 1.3f);
                        pen.Width = 2.4f;
                        g.DrawLine(pen, cx + h * 0.3f, cy + h * 0.3f, cx + h * 0.9f, cy + h * 0.9f);
                        break;
                    case Kind.Copy:
                        g.DrawRectangle(pen, cx - h * 0.45f, cy - h * 0.45f, h * 1.3f, h * 1.3f);
                        g.DrawLines(pen, new[] { new PointF(cx - h * 0.8f, cy + h * 0.5f), new PointF(cx - h * 0.8f, cy - h * 0.85f), new PointF(cx + h * 0.5f, cy - h * 0.85f) });
                        break;
                    case Kind.Save:
                        g.DrawLine(pen, cx, cy - h * 0.9f, cx, cy + h * 0.35f);
                        g.DrawLines(pen, new[] { new PointF(cx - h * 0.5f, cy - h * 0.1f), new PointF(cx, cy + h * 0.4f), new PointF(cx + h * 0.5f, cy - h * 0.1f) });
                        g.DrawLine(pen, cx - h * 0.85f, cy + h * 0.9f, cx + h * 0.85f, cy + h * 0.9f);
                        break;
                }
            }
        }
    }
}
