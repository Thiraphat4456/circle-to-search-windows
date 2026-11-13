using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CircleToSearch
{
    static class UI
    {
        static string family;
        public static Font Font(float size, FontStyle style)
        {
            if (family == null)
            {
                family = "Tahoma";
                foreach (var f in new[] { "Leelawadee UI", "Segoe UI" })
                {
                    try
                    {
                        using (var ff = new FontFamily(f)) { family = f; break; }
                    }
                    catch { }
                }
            }
            return new Font(family, size, style, GraphicsUnit.Point);
        }

        public static GraphicsPath RoundRect(RectangleF r, float rad)
        {
            var p = new GraphicsPath();
            float d = Math.Min(rad * 2, Math.Min(r.Width, r.Height));
            if (d <= 1) { p.AddRectangle(r); return p; }
            p.AddArc(r.X, r.Y, d, d, 180, 90);
            p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            p.CloseFigure();
            return p;
        }

        public static readonly Color Blue = Color.FromArgb(66, 133, 244);
        public static readonly Color Purple = Color.FromArgb(171, 99, 245);
        public static readonly Color Pink = Color.FromArgb(234, 67, 160);
        public static readonly Color Cyan = Color.FromArgb(52, 199, 230);

        public static Brush Gradient(RectangleF r)
        {
            if (r.Width < 1) r.Width = 1;
            if (r.Height < 1) r.Height = 1;
            var b = new LinearGradientBrush(r, Blue, Pink, 35f);
            var cb = new ColorBlend(4);
            cb.Colors = new[] { Blue, Purple, Pink, Cyan };
            cb.Positions = new[] { 0f, 0.4f, 0.75f, 1f };
            b.InterpolationColors = cb;
            return b;
        }
    }
}
