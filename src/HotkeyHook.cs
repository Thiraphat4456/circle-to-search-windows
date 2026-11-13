using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace CircleToSearch
{
    class HotkeyHook : IDisposable
    {
        IntPtr hook = IntPtr.Zero;
        readonly Native.LowLevelKeyboardProc proc; // keep a reference so GC doesn't collect it
        public Func<int> TriggerKey;  // returns the vk that, with Win+Shift, triggers
        public Action Triggered;
        public Action SnipObserved; // Win+Shift+S passed through to Windows
        bool swallowUp;
        bool winDown, shiftDown;

        public HotkeyHook()
        {
            proc = HookProc;
            using (var p = Process.GetCurrentProcess())
            using (var m = p.MainModule)
                hook = Native.SetWindowsHookEx(Native.WH_KEYBOARD_LL, proc, Native.GetModuleHandle(m.ModuleName), 0);
        }

        public bool Installed { get { return hook != IntPtr.Zero; } }

        IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var k = (Native.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(Native.KBDLLHOOKSTRUCT));
                int msg = wParam.ToInt32();
                bool down = msg == Native.WM_KEYDOWN || msg == Native.WM_SYSKEYDOWN;
                bool up = msg == Native.WM_KEYUP || msg == Native.WM_SYSKEYUP;
                int vk = k.vkCode;

                if (vk == 0x5B || vk == 0x5C) winDown = down ? true : (up ? false : winDown);       // LWin / RWin
                if (vk == 0xA0 || vk == 0xA1 || vk == 0x10) shiftDown = down ? true : (up ? false : shiftDown);

                bool injected = (k.flags & Native.LLKHF_INJECTED) != 0;
                int trig = TriggerKey != null ? TriggerKey() : 0x53;

                if (!injected && down && vk == 0x53 && trig != 0x53)
                {
                    bool w = winDown || Native.IsDown(0x5B) || Native.IsDown(0x5C);
                    bool sh = shiftDown || Native.IsDown(0x10);
                    if (w && sh) { var so = SnipObserved; if (so != null) so(); }
                }

                if (!injected && vk == trig)
                {
                    if (down)
                    {
                        bool win = winDown || Native.IsDown(0x5B) || Native.IsDown(0x5C);
                        bool shift = shiftDown || Native.IsDown(0x10);
                        bool ctrl = Native.IsDown(0x11), alt = Native.IsDown(0x12);
                        if (win && shift && !ctrl && !alt)
                        {
                            swallowUp = true;
                            Native.SendMaskKey(); // prevents Start menu on Win release + gives us foreground rights
                            var t = Triggered;
                            if (t != null) t();
                            return new IntPtr(1);
                        }
                    }
                    else if (up && swallowUp)
                    {
                        swallowUp = false;
                        return new IntPtr(1);
                    }
                }
            }
            return Native.CallNextHookEx(hook, nCode, wParam, lParam);
        }

        public void Dispose()
        {
            if (hook != IntPtr.Zero) { Native.UnhookWindowsHookEx(hook); hook = IntPtr.Zero; }
        }
    }
}
