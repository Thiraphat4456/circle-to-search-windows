using System;
using System.Windows.Forms;
using Microsoft.Win32;

namespace CircleToSearch
{
    class Settings
    {
        const string Key = "Software\\CircleToSearch";
        const string RunKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";

        public bool ReplaceWinShiftS = false;  // false (default): Windows keeps Win+Shift+S, Circle to Search = Win+Shift+C
        public bool SnipPopup = true;          // after a normal Win+Shift+S snip, offer "Search with Google Lens"
        public bool SearchImmediately = false; // circle -> search right away (like Samsung)
        public bool AutoSaveScreenshots = false;

        public static Settings Load()
        {
            var s = new Settings();
            try
            {
                using (var k = Registry.CurrentUser.OpenSubKey(Key))
                {
                    if (k != null)
                    {
                        s.ReplaceWinShiftS = ToBool(k.GetValue("OverrideWinShiftS"), false);
                        s.SnipPopup = ToBool(k.GetValue("SnipPopup"), true);
                        s.SearchImmediately = ToBool(k.GetValue("SearchImmediately"), false);
                        s.AutoSaveScreenshots = ToBool(k.GetValue("AutoSaveScreenshots"), false);
                    }
                }
            }
            catch { }
            return s;
        }

        public void Save()
        {
            try
            {
                using (var k = Registry.CurrentUser.CreateSubKey(Key))
                {
                    k.SetValue("OverrideWinShiftS", ReplaceWinShiftS ? 1 : 0, RegistryValueKind.DWord);
                    k.SetValue("SnipPopup", SnipPopup ? 1 : 0, RegistryValueKind.DWord);
                    try { k.DeleteValue("ReplaceWinShiftS", false); } catch { }
                    k.SetValue("SearchImmediately", SearchImmediately ? 1 : 0, RegistryValueKind.DWord);
                    k.SetValue("AutoSaveScreenshots", AutoSaveScreenshots ? 1 : 0, RegistryValueKind.DWord);
                }
            }
            catch { }
        }

        static bool ToBool(object o, bool def)
        {
            if (o == null) return def;
            try { return Convert.ToInt32(o) != 0; } catch { return def; }
        }

        public static bool StartWithWindows
        {
            get
            {
                try
                {
                    using (var k = Registry.CurrentUser.OpenSubKey(RunKey))
                        return k != null && k.GetValue(Program.AppName) != null;
                }
                catch { return false; }
            }
            set
            {
                try
                {
                    using (var k = Registry.CurrentUser.CreateSubKey(RunKey))
                    {
                        if (value) k.SetValue(Program.AppName, "\"" + Application.ExecutablePath + "\"");
                        else k.DeleteValue(Program.AppName, false);
                    }
                }
                catch { }
            }
        }
    }
}
