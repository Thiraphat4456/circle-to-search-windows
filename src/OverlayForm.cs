using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace CircleToSearch
{
    enum OverlayAction { Search, Copy, Save }
    enum SelMode { Lasso, Rect }
    enum SelState { Idle, Drawing, Selected }

    class Btn
    {
        public string Id, Text;
        public Icons.Kind Icon;
        public RectangleF Rect;
        public bool Primary, Toggled;
    }

    class OverlayForm : Form
    {
        public event Action<OverlayAction, Bitmap> ActionRequested;

        readonly Bitmap shot;   // original capture
        readonly Bitmap dimmed; // darkened copy (background)
        readonly Rectangle virt;
        readonly Settings settings;

        SelMode mode = SelMode.Lasso;
        SelState state = SelState.Idle;
        readonly List<PointF> pts = new List<PointF>();
        Point dragStart;
        Rectangle sel;          // client coords
        Rectangle lastPaintSel; // for invalidation while dragging
        readonly List<Btn> toolbar = new List<Btn>();
        readonly List<Btn> actionBar = new List<Btn>();
        RectangleF toolbarRect, actionRect, hintRect;
        Btn hover;
        readonly Font fontBtn, fontBold, fontHint, fontSize;
        float scale = 1f;
        bool closing;

        public OverlayForm(Bitmap shot, Rectangle virt, Settings settings)
        {
            this.shot = shot;
            this.virt = virt;
            this.settings = settings;

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;
            TopMost = true;
            KeyPreview = true;
            Cursor = Cursors.Cross;
            Text = "Circle to Search";
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.Opaque, true);
            Bounds = virt;

            using (var g = CreateGraphics()) scale = Math.Max(1f, g.DpiX / 96f);
            fontBtn = UI.Font(9.5f, FontStyle.Regular);
            fontBold = UI.Font(9.5f, FontStyle.Bold);
            fontHint = UI.Font(10.5f, FontStyle.Regular);
            fontSize = UI.Font(8f, FontStyle.Regular);

            dimmed = new Bitmap(shot.Width, shot.Height, PixelFormat.Format32bppPArgb);
            using (var g = Graphics.FromImage(dimmed))
            {
                g.DrawImageUnscaled(shot, 0, 0);
                using (var b = new SolidBrush(Color.FromArgb(120, 8, 10, 20))) g.FillRectangle(b, 0, 0, shot.Width, shot.Height);
            }

            BuildToolbar();
        }

        // Make sure the overlay really becomes the active window so Esc/Enter work.
        public void ForceForeground()
        {
            Native.SendMaskKey();
            Native.SetForegroundWindow(Handle);
            Activate();
            Focus();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x80; // WS_EX_TOOLWINDOW: no Alt+Tab entry
                return cp;
            }
        }

        float S(float v) { return v * scale; }

        Rectangle PrimaryClient()
        {
            var p = Screen.PrimaryScreen.Bounds;
            p.Offset(-virt.X, -virt.Y);
            return p;
        }

        Rectangle ScreenClientAt(Point clientPt)
        {
            var sp = new Point(clientPt.X + virt.X, clientPt.Y + virt.Y);
            var b = Screen.FromPoint(sp).Bounds;
            b.Offset(-virt.X, -virt.Y);
            return b;
        }

        // ---------------- toolbar / action bar layout
        void BuildToolbar()
        {
            toolbar.Clear();
            toolbar.Add(new Btn { Id = "lasso", Text = Strings.ToolLasso, Icon = Icons.Kind.Lasso });
            toolbar.Add(new Btn { Id = "rect", Text = Strings.ToolRect, Icon = Icons.Kind.Rect });
            toolbar.Add(new Btn { Id = "full", Text = Strings.ToolFull, Icon = Icons.Kind.Full });
            toolbar.Add(new Btn { Id = "close", Text = "", Icon = Icons.Kind.Close });
            LayoutBar(toolbar, out toolbarRect, PrimaryClient(), true, RectangleF.Empty);
            UpdateToggles();
            var pc = PrimaryClient();
            var hs = TextRenderer.MeasureText(HintText(), fontHint);
            hintRect = new RectangleF(pc.X + (pc.Width - hs.Width - S(28)) / 2f, toolbarRect.Bottom + S(10), hs.Width + S(28), hs.Height + S(12));
        }

        string HintText()
        {
            if (mode == SelMode.Lasso) return Strings.HintLasso;
            return Strings.HintRect;
        }

        void UpdateToggles()
        {
            foreach (var b in toolbar)
                b.Toggled = (b.Id == "lasso" && mode == SelMode.Lasso) || (b.Id == "rect" && mode == SelMode.Rect);
        }

        void LayoutBar(List<Btn> bar, out RectangleF barRect, Rectangle area, bool top, RectangleF anchor)
        {
            float pad = S(6), gap = S(4), h = S(36), icon = S(16);
            float x = 0;
            var widths = new List<float>();
            foreach (var b in bar)
            {
                float w;
                if (string.IsNullOrEmpty(b.Text)) w = h;
                else
                {
                    var sz = TextRenderer.MeasureText(b.Text, b.Primary ? fontBold : fontBtn);
                    w = S(12) + icon + S(8) + sz.Width + S(14);
                }
                widths.Add(w);
                x += w + gap;
            }
            float totalW = x - gap + pad * 2, totalH = h + pad * 2;
            float bx, by;
            if (top)
            {
                bx = area.X + (area.Width - totalW) / 2f;
                by = area.Y + S(18);
            }
            else
            {
                // place under the selection, else above, else inside bottom
                bx = anchor.X + (anchor.Width - totalW) / 2f;
                by = anchor.Bottom + S(14);
                if (by + totalH > area.Bottom - S(8)) by = anchor.Y - S(14) - totalH;
                if (by < area.Y + S(8)) by = anchor.Bottom - totalH - S(14);
                bx = Math.Max(area.X + S(8), Math.Min(bx, area.Right - totalW - S(8)));
                by = Math.Max(area.Y + S(8), Math.Min(by, area.Bottom - totalH - S(8)));
            }
            barRect = new RectangleF(bx, by, totalW, totalH);
            float cx = bx + pad;
            for (int i = 0; i < bar.Count; i++)
            {
                bar[i].Rect = new RectangleF(cx, by + pad, widths[i], h);
                cx += widths[i] + gap;
            }
        }

        void BuildActionBar()
        {
            actionBar.Clear();
            actionBar.Add(new Btn { Id = "search", Text = Strings.ActionSearch, Icon = Icons.Kind.Search, Primary = true });
            actionBar.Add(new Btn { Id = "copy", Text = Strings.ActionCopy, Icon = Icons.Kind.Copy });
            actionBar.Add(new Btn { Id = "save", Text = Strings.ActionSave, Icon = Icons.Kind.Save });
            actionBar.Add(new Btn { Id = "cancel", Text = "", Icon = Icons.Kind.Close });
            var area = ScreenClientAt(new Point(sel.X + sel.Width / 2, sel.Y + sel.Height / 2));
            LayoutBar(actionBar, out actionRect, area, false, sel);
        }

        // ---------------- painting
        protected override void OnPaintBackground(PaintEventArgs e) { }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            var clip = e.ClipRectangle;
            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.DrawImage(dimmed, clip, clip, GraphicsUnit.Pixel);
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (state == SelState.Drawing && mode == SelMode.Lasso)
            {
                DrawLassoStroke(g);
            }
            else if (state == SelState.Drawing && mode == SelMode.Rect)
            {
                if (sel.Width > 0 && sel.Height > 0)
                {
                    g.DrawImage(shot, sel, sel, GraphicsUnit.Pixel);
                    using (var p = new Pen(Color.White, S(1.5f))) g.DrawRectangle(p, sel);
                    DrawSizeLabel(g, sel);
                }
            }
            else if (state == SelState.Selected)
            {
                DrawSelection(g);
                DrawBar(g, actionBar, actionRect);
            }

            if (state != SelState.Drawing)
            {
                DrawBar(g, toolbar, toolbarRect);
                if (state == SelState.Idle) DrawHint(g);
            }
        }

        void DrawLassoStroke(Graphics g)
        {
            if (pts.Count < 2) return;
            var arr = pts.ToArray();
            var b = PtsBounds(pts);
            b.Inflate(S(10), S(10));
            using (var glow = new Pen(Color.FromArgb(70, 255, 255, 255), S(14)))
            {
                glow.LineJoin = LineJoin.Round; glow.StartCap = LineCap.Round; glow.EndCap = LineCap.Round;
                g.DrawLines(glow, arr);
            }
            using (var br = UI.Gradient(b))
            using (var pen = new Pen(br, S(5.5f)))
            {
                pen.LineJoin = LineJoin.Round; pen.StartCap = LineCap.Round; pen.EndCap = LineCap.Round;
                g.DrawLines(pen, arr);
            }
        }

        void DrawSelection(Graphics g)
        {
            float r = S(18);
            using (var path = UI.RoundRect(sel, r))
            {
                var st = g.Save();
                g.SetClip(path, CombineMode.Intersect);
                g.DrawImage(shot, sel, sel, GraphicsUnit.Pixel);
                g.Restore(st);

                // soft glow + gradient border, Samsung-style
                using (var glow = new Pen(Color.FromArgb(60, 255, 255, 255), S(9))) g.DrawPath(glow, path);
                RectangleF gb = sel; gb.Inflate(S(4), S(4));
                using (var br = UI.Gradient(gb))
                using (var pen = new Pen(br, S(3.5f))) g.DrawPath(pen, path);
            }
            // keep the lasso faintly visible inside the box
            if (mode == SelMode.Lasso && pts.Count > 2)
            {
                using (var pen = new Pen(Color.FromArgb(110, 255, 255, 255), S(2)))
                {
                    pen.DashStyle = DashStyle.Dot; pen.LineJoin = LineJoin.Round;
                    g.DrawLines(pen, pts.ToArray());
                }
            }
            DrawSizeLabel(g, sel);
        }

        void DrawSizeLabel(Graphics g, Rectangle r)
        {
            string t = r.Width + " × " + r.Height;
            var sz = TextRenderer.MeasureText(t, fontSize);
            var lr = new RectangleF(r.X, r.Y - sz.Height - S(10), sz.Width + S(12), sz.Height + S(4));
            if (lr.Y < 0) lr.Y = r.Y + S(6);
            using (var path = UI.RoundRect(lr, S(6)))
            using (var b = new SolidBrush(Color.FromArgb(200, 20, 22, 28))) g.FillPath(b, path);
            TextRenderer.DrawText(g, t, fontSize, Rectangle.Round(lr), Color.FromArgb(230, 230, 235),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        void DrawBar(Graphics g, List<Btn> bar, RectangleF rect)
        {
            using (var shadowPath = UI.RoundRect(new RectangleF(rect.X, rect.Y + S(3), rect.Width, rect.Height), rect.Height / 2))
            using (var sb = new SolidBrush(Color.FromArgb(70, 0, 0, 0))) g.FillPath(sb, shadowPath);
            using (var path = UI.RoundRect(rect, rect.Height / 2))
            {
                using (var b = new SolidBrush(Color.FromArgb(245, 32, 33, 38))) g.FillPath(b, path);
                using (var p = new Pen(Color.FromArgb(60, 255, 255, 255), 1f)) g.DrawPath(p, path);
            }
            foreach (var btn in bar)
            {
                bool hot = btn == hover;
                using (var bp = UI.RoundRect(btn.Rect, btn.Rect.Height / 2))
                {
                    if (btn.Primary)
                    {
                        using (var br = UI.Gradient(btn.Rect)) g.FillPath(br, bp);
                        if (hot) using (var hb = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) g.FillPath(hb, bp);
                    }
                    else if (btn.Toggled)
                    {
                        using (var b = new SolidBrush(Color.FromArgb(hot ? 90 : 70, 138, 180, 248))) g.FillPath(b, bp);
                    }
                    else if (hot)
                    {
                        using (var b = new SolidBrush(Color.FromArgb(40, 255, 255, 255))) g.FillPath(b, bp);
                    }
                }
                Color fg = btn.Primary ? Color.White : (btn.Toggled ? Color.FromArgb(174, 203, 250) : Color.FromArgb(232, 234, 237));
                float icon = S(16);
                if (string.IsNullOrEmpty(btn.Text))
                {
                    var ir = new RectangleF(btn.Rect.X + (btn.Rect.Width - icon) / 2, btn.Rect.Y + (btn.Rect.Height - icon) / 2, icon, icon);
                    Icons.Draw(g, btn.Icon, ir, fg);
                }
                else
                {
                    var ir = new RectangleF(btn.Rect.X + S(12), btn.Rect.Y + (btn.Rect.Height - icon) / 2, icon, icon);
                    Icons.Draw(g, btn.Icon, ir, fg);
                    var tr = Rectangle.Round(new RectangleF(ir.Right + S(8), btn.Rect.Y, btn.Rect.Right - ir.Right - S(8), btn.Rect.Height));
                    TextRenderer.DrawText(g, btn.Text, btn.Primary ? fontBold : fontBtn, tr, fg,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
                }
            }
        }

        void DrawHint(Graphics g)
        {
            using (var path = UI.RoundRect(hintRect, hintRect.Height / 2))
            using (var b = new SolidBrush(Color.FromArgb(170, 20, 22, 28))) g.FillPath(b, path);
            TextRenderer.DrawText(g, HintText(), fontHint, Rectangle.Round(hintRect), Color.FromArgb(225, 228, 235),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding | TextFormatFlags.SingleLine);
        }

        static RectangleF PtsBounds(List<PointF> p)
        {
            float x1 = float.MaxValue, y1 = float.MaxValue, x2 = float.MinValue, y2 = float.MinValue;
            foreach (var q in p)
            {
                if (q.X < x1) x1 = q.X; if (q.Y < y1) y1 = q.Y;
                if (q.X > x2) x2 = q.X; if (q.Y > y2) y2 = q.Y;
            }
            return RectangleF.FromLTRB(x1, y1, x2, y2);
        }

        void InvalidateF(RectangleF r, float pad)
        {
            r.Inflate(pad, pad);
            Invalidate(Rectangle.Round(r));
        }

        // ---------------- input
        Btn HitTest(Point p)
        {
            if (state == SelState.Selected)
                foreach (var b in actionBar) if (b.Rect.Contains(p)) return b;
            if (state != SelState.Drawing)
                foreach (var b in toolbar) if (b.Rect.Contains(p)) return b;
            return null;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Right)
            {
                if (state == SelState.Selected) ResetSelection(); else CloseOverlay();
                return;
            }
            if (e.Button != MouseButtons.Left) return;

            var hit = HitTest(e.Location);
            if (hit != null) { OnButton(hit.Id); return; }

            // start a new selection
            state = SelState.Drawing;
            pts.Clear();
            pts.Add(e.Location);
            dragStart = e.Location;
            sel = new Rectangle(e.Location, Size.Empty);
            lastPaintSel = sel;
            hover = null;
            Cursor = Cursors.Cross;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (state == SelState.Drawing)
            {
                if (mode == SelMode.Lasso)
                {
                    var last = pts[pts.Count - 1];
                    float dx = e.X - last.X, dy = e.Y - last.Y;
                    if (dx * dx + dy * dy < 4) return;
                    pts.Add(e.Location);
                    // The gradient spans the whole stroke, so repaint its bounds (redraw is clipped, so it's cheap).
                    InvalidateF(PtsBounds(pts), S(16));
                }
                else
                {
                    sel = Rectangle.FromLTRB(Math.Min(dragStart.X, e.X), Math.Min(dragStart.Y, e.Y), Math.Max(dragStart.X, e.X), Math.Max(dragStart.Y, e.Y));
                    var u = Rectangle.Union(sel, lastPaintSel);
                    u.Inflate((int)S(8), (int)S(40));
                    Invalidate(u);
                    lastPaintSel = sel;
                }
                return;
            }
            var h = HitTest(e.Location);
            if (h != hover)
            {
                if (hover != null) InvalidateF(hover.Rect, 2);
                hover = h;
                if (hover != null) InvalidateF(hover.Rect, 2);
                Cursor = h != null ? Cursors.Hand : Cursors.Cross;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (state != SelState.Drawing || e.Button != MouseButtons.Left) return;

            Rectangle r;
            if (mode == SelMode.Lasso)
            {
                var b = PtsBounds(pts);
                if (b.Width < S(14) && b.Height < S(14))
                {
                    // a tap: grab a sensible area around the point (like tapping an object on Samsung)
                    float half = S(140);
                    b = new RectangleF(e.X - half, e.Y - half, half * 2, half * 2);
                }
                b.Inflate(S(8), S(8));
                r = Rectangle.Round(b);
            }
            else r = sel;

            r.Intersect(new Rectangle(Point.Empty, ClientSize));
            if (r.Width < 4 || r.Height < 4) { ResetSelection(); return; }

            sel = r;
            state = SelState.Selected;
            BuildActionBar();
            Invalidate();

            if (mode == SelMode.Lasso && settings.SearchImmediately)
                Finish(OverlayAction.Search);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.Escape) { CloseOverlay(); return; }
            if (e.KeyCode == Keys.F) { OnButton("full"); return; }
            if (state == SelState.Selected)
            {
                if (e.KeyCode == Keys.Enter) Finish(mode == SelMode.Lasso ? OverlayAction.Search : OverlayAction.Copy);
                else if (e.Control && e.KeyCode == Keys.C) Finish(OverlayAction.Copy);
                else if (e.Control && e.KeyCode == Keys.S) Finish(OverlayAction.Save);
                else if (e.KeyCode == Keys.G || e.KeyCode == Keys.L) Finish(OverlayAction.Search);
            }
        }

        protected override bool ProcessDialogKey(Keys keyData)
        {
            if (keyData == Keys.Tab) { SetMode(mode == SelMode.Lasso ? SelMode.Rect : SelMode.Lasso); return true; }
            return base.ProcessDialogKey(keyData);
        }

        void OnButton(string id)
        {
            switch (id)
            {
                case "lasso": SetMode(SelMode.Lasso); break;
                case "rect": SetMode(SelMode.Rect); break;
                case "full":
                    {
                        var p = PointToClient(Cursor.Position);
                        sel = ScreenClientAt(p);
                        pts.Clear();
                        state = SelState.Selected;
                        BuildActionBar();
                        // full screen: bar would land inside; that's fine
                        Invalidate();
                        break;
                    }
                case "close": CloseOverlay(); break;
                case "cancel": ResetSelection(); break;
                case "search": Finish(OverlayAction.Search); break;
                case "copy": Finish(OverlayAction.Copy); break;
                case "save": Finish(OverlayAction.Save); break;
            }
        }

        void SetMode(SelMode m)
        {
            mode = m;
            BuildToolbar();
            if (state == SelState.Drawing) state = SelState.Idle;
            Invalidate();
        }

        void ResetSelection()
        {
            state = SelState.Idle;
            pts.Clear();
            sel = Rectangle.Empty;
            hover = null;
            Invalidate();
        }

        void Finish(OverlayAction action)
        {
            if (closing || sel.Width < 1 || sel.Height < 1) return;
            closing = true;
            var crop = new Bitmap(sel.Width, sel.Height, PixelFormat.Format24bppRgb);
            using (var g = Graphics.FromImage(crop))
                g.DrawImage(shot, new Rectangle(0, 0, sel.Width, sel.Height), sel, GraphicsUnit.Pixel);
            var handler = ActionRequested;
            Hide();
            Close();
            if (handler != null) handler(action, crop); else crop.Dispose();
        }

        void CloseOverlay()
        {
            if (closing) return;
            closing = true;
            Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                shot.Dispose(); dimmed.Dispose();
                fontBtn.Dispose(); fontBold.Dispose(); fontHint.Dispose(); fontSize.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
