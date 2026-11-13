using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CircleToSearch
{
    class SnipPopup : Form
    {
        public event Action<Bitmap> SearchClicked;
        readonly Bitmap img, thumb;
        readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        readonly Font font, fontBold;
        RectangleF btnSearch, btnClose;
        int hot = -1; // 0 search, 1 close
        float sc = 1f;
        int ticks;

        public SnipPopup(Bitmap img)
        {
            this.img = img;
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
            BackColor = Color.FromArgb(32, 33, 38);
            using (var g = CreateGraphics()) sc = Math.Max(1f, g.DpiX / 96f);
            font = UI.Font(9f, FontStyle.Regular);
            fontBold = UI.Font(9.5f, FontStyle.Bold);

            int th = (int)(56 * sc);
            int tw = Math.Max(1, Math.Min((int)(110 * sc), img.Width * th / Math.Max(1, img.Height)));
            thumb = new Bitmap(tw, th);
            using (var g = Graphics.FromImage(thumb))
            {
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.Clear(Color.FromArgb(50, 52, 58));
                float k = Math.Min((float)tw / img.Width, (float)th / img.Height);
                float w = img.Width * k, h = img.Height * k;
                g.DrawImage(img, (tw - w) / 2, (th - h) / 2, w, h);
            }

            var label = Strings.SearchThisImage;
            var ls = TextRenderer.MeasureText(label, fontBold);
            float pad = 12 * sc;
            float bw = 16 * sc + 8 * sc + ls.Width + 28 * sc, bh = 36 * sc;
            float x = pad + tw + pad;
            btnSearch = new RectangleF(x, pad + (th - bh) / 2, bw, bh);
            btnClose = new RectangleF(btnSearch.Right + 6 * sc, pad + (th - bh) / 2, bh, bh);
            int W = (int)(btnClose.Right + pad), H = (int)(th + pad * 2);
            var wa = Screen.PrimaryScreen.WorkingArea;
            // bottom-center, clear of the Windows notification toast at bottom-right
            Bounds = new Rectangle(wa.X + (wa.Width - W) / 2, wa.Bottom - H - (int)(24 * sc), W, H);
            using (var path = UI.RoundRect(new RectangleF(0, 0, W, H), 16 * sc)) Region = new Region(path);

            timer.Interval = 100;
            timer.Tick += delegate
            {
                if (ClientRectangle.Contains(PointToClient(Cursor.Position))) { ticks = 0; return; }
                if (++ticks > 80) Close(); // ~8 s
            };
            timer.Start();
        }

        protected override bool ShowWithoutActivation { get { return true; } }
        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80 | 0x08000000 | 0x8; // TOOLWINDOW | NOACTIVATE | TOPMOST
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);
            float pad = 12 * sc;
            using (var tp = UI.RoundRect(new RectangleF(pad, pad, thumb.Width, thumb.Height), 8 * sc))
            {
                var st = g.Save(); g.SetClip(tp); g.DrawImage(thumb, pad, pad); g.Restore(st);
                using (var p = new Pen(Color.FromArgb(60, 255, 255, 255))) g.DrawPath(p, tp);
            }
            using (var bp = UI.RoundRect(btnSearch, btnSearch.Height / 2))
            {
                using (var br = UI.Gradient(btnSearch)) g.FillPath(br, bp);
                if (hot == 0) using (var hb = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) g.FillPath(hb, bp);
            }
            float ic = 16 * sc;
            var ir = new RectangleF(btnSearch.X + 12 * sc, btnSearch.Y + (btnSearch.Height - ic) / 2, ic, ic);
            Icons.Draw(g, Icons.Kind.Search, ir, Color.White);
            var tr = Rectangle.Round(new RectangleF(ir.Right + 8 * sc, btnSearch.Y, btnSearch.Right - ir.Right - 8 * sc, btnSearch.Height));
            TextRenderer.DrawText(g, Strings.SearchThisImage, fontBold, tr, Color.White,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
            if (hot == 1) using (var cp = UI.RoundRect(btnClose, btnClose.Height / 2)) using (var hb = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) g.FillPath(hb, cp);
            var cr = new RectangleF(btnClose.X + (btnClose.Width - ic) / 2, btnClose.Y + (btnClose.Height - ic) / 2, ic, ic);
            Icons.Draw(g, Icons.Kind.Close, cr, Color.FromArgb(220, 222, 226));
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            int h = btnSearch.Contains(e.Location) ? 0 : (btnClose.Contains(e.Location) ? 1 : -1);
            if (h != hot) { hot = h; Cursor = h >= 0 ? Cursors.Hand : Cursors.Default; Invalidate(); }
        }

        protected override void OnMouseLeave(EventArgs e) { if (hot != -1) { hot = -1; Invalidate(); } }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            if (btnSearch.Contains(e.Location))
            {
                var h = SearchClicked;
                Hide();
                if (h != null) h(img);
                Close();
            }
            else if (btnClose.Contains(e.Location)) Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Dispose(); img.Dispose(); thumb.Dispose(); font.Dispose(); fontBold.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
