using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CircleToSearch
{
    // The app lives in the system tray: owns the hotkey hook, the clipboard listener and the overlay.
    class TrayContext : ApplicationContext
    {
        public const string MsgWindowTitle = "CircleToSearch_MsgWindow";
        public const int WM_APP_SHOW = 0x8000 + 77;  // open the circle overlay
        const int WM_APP_SNIP = 0x8000 + 78;         // Windows' own Win+Shift+S was pressed
        const int SnipWindowSeconds = 120;           // how long after Win+Shift+S a clipboard image counts as "the snip"

        readonly Settings settings;
        readonly NotifyIcon tray;
        readonly HotkeyHook hook;
        readonly MessageWindow msgWin;
        ToolStripMenuItem miOpen;
        OverlayForm overlay;
        SnipPopup popup;
        DateTime snipPressedAt = DateTime.MinValue;

        // Hidden window: receives our posted messages and WM_CLIPBOARDUPDATE.
        class MessageWindow : NativeWindow
        {
            readonly Action<int> handler;
            public MessageWindow(Action<int> handler)
            {
                this.handler = handler;
                var cp = new CreateParams();
                cp.Caption = MsgWindowTitle;
                CreateHandle(cp);
            }
            protected override void WndProc(ref Message m)
            {
                if (m.Msg == WM_APP_SHOW || m.Msg == WM_APP_SNIP || m.Msg == Native.WM_CLIPBOARDUPDATE)
                {
                    handler(m.Msg);
                    return;
                }
                base.WndProc(ref m);
            }
        }

        public TrayContext(bool showNow)
        {
            settings = Settings.Load();
            msgWin = new MessageWindow(OnMessage);

            tray = new NotifyIcon();
            tray.Icon = Icons.MakeTrayIcon();
            tray.Text = Program.AppName;
            tray.ContextMenuStrip = BuildMenu();
            tray.MouseClick += delegate(object s, MouseEventArgs e) { if (e.Button == MouseButtons.Left) ShowOverlay(); };
            tray.Visible = true;

            // The hook callback must return fast (Windows silently drops slow hooks), so just post to ourselves.
            hook = new HotkeyHook();
            hook.TriggerKey = delegate { return settings.ReplaceWinShiftS ? 0x53 /*S*/ : 0x43 /*C*/; };
            hook.Triggered = delegate { Post(WM_APP_SHOW); };
            hook.SnipObserved = delegate { Post(WM_APP_SNIP); };

            Native.AddClipboardFormatListener(msgWin.Handle);
            GoogleLens.CleanOldFiles();

            if (!hook.Installed)
                Notify(Strings.HookFailed, ToolTipIcon.Warning);
            else if (!showNow)
                Notify(string.Format(Strings.ReadyFormat, HotkeyText()) +
                       (settings.ReplaceWinShiftS ? "" : "\n" + Strings.ReadyWindowsSnipKept), ToolTipIcon.Info);

            if (showNow) ShowOverlay();
        }

        void Post(int msg) { Native.PostMessage(msgWin.Handle, msg, IntPtr.Zero, IntPtr.Zero); }

        void Notify(string text, ToolTipIcon icon) { tray.ShowBalloonTip(3000, Program.AppName, text, icon); }

        string HotkeyText() { return settings.ReplaceWinShiftS ? "Win+Shift+S" : "Win+Shift+C"; }

        void OnMessage(int msg)
        {
            if (msg == WM_APP_SHOW) ShowOverlay();
            else if (msg == WM_APP_SNIP) snipPressedAt = DateTime.Now;
            else if (msg == Native.WM_CLIPBOARDUPDATE) OnClipboardChanged();
        }

        // ---------------- menu
        ContextMenuStrip BuildMenu()
        {
            var m = new ContextMenuStrip();
            m.Font = UI.Font(9f, FontStyle.Regular);

            miOpen = new ToolStripMenuItem(string.Format(Strings.MenuOpenFormat, HotkeyText()), null, delegate { ShowOverlay(); });
            miOpen.Font = UI.Font(9f, FontStyle.Bold);
            m.Items.Add(miOpen);
            m.Items.Add(new ToolStripSeparator());

            AddToggle(m, Strings.MenuSnipPopup,
                () => settings.SnipPopup, v => settings.SnipPopup = v);
            AddToggle(m, Strings.MenuOverrideWinShiftS,
                () => settings.ReplaceWinShiftS, v =>
                {
                    settings.ReplaceWinShiftS = v;
                    miOpen.Text = string.Format(Strings.MenuOpenFormat, HotkeyText());
                    Notify(string.Format(Strings.HotkeyNowFormat, HotkeyText()), ToolTipIcon.Info);
                });
            AddToggle(m, Strings.MenuSearchImmediately,
                () => settings.SearchImmediately, v => settings.SearchImmediately = v);
            AddToggle(m, Strings.MenuAutoSave,
                () => settings.AutoSaveScreenshots, v => settings.AutoSaveScreenshots = v);
            AddToggle(m, Strings.MenuStartup,
                () => Settings.StartWithWindows, v => Settings.StartWithWindows = v);

            m.Items.Add(new ToolStripSeparator());
            m.Items.Add(new ToolStripMenuItem(Strings.MenuExit, null, delegate { ExitThread(); }));
            return m;
        }

        void AddToggle(ContextMenuStrip menu, string text, Func<bool> get, Action<bool> set)
        {
            var item = new ToolStripMenuItem(text);
            item.Checked = get();
            item.Click += delegate
            {
                set(!get());
                settings.Save();
                item.Checked = get();
            };
            menu.Items.Add(item);
        }

        // ---------------- circle overlay
        void ShowOverlay()
        {
            if (overlay != null && !overlay.IsDisposed) { overlay.Activate(); return; }

            Rectangle virt = SystemInformation.VirtualScreen;
            Bitmap shot;
            try
            {
                shot = new Bitmap(virt.Width, virt.Height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(shot))
                    g.CopyFromScreen(virt.Left, virt.Top, 0, 0, virt.Size, CopyPixelOperation.SourceCopy);
            }
            catch (Exception ex)
            {
                Program.Log(ex);
                Notify(Strings.CaptureFailed, ToolTipIcon.Error);
                return;
            }

            overlay = new OverlayForm(shot, virt, settings);
            overlay.ActionRequested += OnOverlayAction;
            overlay.FormClosed += delegate { overlay = null; };
            overlay.Show();
            overlay.ForceForeground();
        }

        // Called after the overlay has closed; we own (and dispose) the cropped image.
        void OnOverlayAction(OverlayAction action, Bitmap img)
        {
            try
            {
                if (settings.AutoSaveScreenshots) ImageActions.AutoSave(img);
                switch (action)
                {
                    case OverlayAction.Search:
                        GoogleLens.Search(img);
                        break;
                    case OverlayAction.Copy:
                        ImageActions.CopyToClipboard(img);
                        Notify(Strings.Copied, ToolTipIcon.None);
                        break;
                    case OverlayAction.Save:
                        ImageActions.CopyToClipboard(img);
                        ImageActions.SaveAs(img);
                        break;
                }
            }
            catch (Exception ex)
            {
                Program.Log(ex);
                Notify(string.Format(Strings.ErrorFormat, ex.Message), ToolTipIcon.Error);
            }
            finally { img.Dispose(); }
        }

        // ---------------- add-on for Windows' own Win+Shift+S
        // A normal Windows snip just landed on the clipboard -> offer to search it.
        void OnClipboardChanged()
        {
            if (!settings.SnipPopup || settings.ReplaceWinShiftS) return;
            if ((DateTime.Now - snipPressedAt).TotalSeconds > SnipWindowSeconds) return;

            Bitmap img = ReadClipboardImage();
            if (img == null) return;
            snipPressedAt = DateTime.MinValue; // one popup per snip

            if (popup != null && !popup.IsDisposed) popup.Close();
            popup = new SnipPopup(img);
            popup.SearchClicked += delegate(Bitmap b)
            {
                try { GoogleLens.Search(b); }
                catch (Exception ex) { Program.Log(ex); }
            };
            popup.FormClosed += delegate { popup = null; };
            popup.Show();
        }

        static Bitmap ReadClipboardImage()
        {
            for (int i = 0; i < 4; i++)
            {
                try
                {
                    if (!Clipboard.ContainsImage()) return null;
                    using (var im = Clipboard.GetImage())
                        if (im != null) return new Bitmap(im);
                }
                catch (ExternalException) { Thread.Sleep(50); } // clipboard briefly locked by the writer
            }
            return null;
        }

        protected override void ExitThreadCore()
        {
            hook.Dispose();
            Native.RemoveClipboardFormatListener(msgWin.Handle);
            if (popup != null) popup.Close();
            if (overlay != null) overlay.Close();
            tray.Visible = false;
            tray.Dispose();
            msgWin.DestroyHandle();
            base.ExitThreadCore();
        }
    }
}
