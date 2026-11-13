using System;
using System.Runtime.InteropServices;

namespace CircleToSearch
{
    static class Native
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100, WM_KEYUP = 0x0101, WM_SYSKEYDOWN = 0x0104, WM_SYSKEYUP = 0x0105;
        public const int LLKHF_INJECTED = 0x10;
        public const byte VK_MASK = 0xE8; // unassigned key — used to stop the Start menu from opening

        [StructLayout(LayoutKind.Sequential)]
        public struct KBDLLHOOKSTRUCT { public int vkCode; public int scanCode; public int flags; public int time; public IntPtr dwExtraInfo; }

        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll")] public static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")] public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr GetModuleHandle(string name);
        [DllImport("user32.dll")] public static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);
        [DllImport("user32.dll")] public static extern short GetAsyncKeyState(int vk);
        [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)] public static extern IntPtr FindWindow(string cls, string title);
        [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr w, IntPtr l);
        [DllImport("user32.dll")] static extern bool SetProcessDPIAware();
        [DllImport("user32.dll")] static extern bool SetProcessDpiAwarenessContext(IntPtr value);
        [DllImport("user32.dll")] public static extern bool DestroyIcon(IntPtr h);
        [DllImport("user32.dll")] public static extern bool AddClipboardFormatListener(IntPtr hwnd);
        [DllImport("user32.dll")] public static extern bool RemoveClipboardFormatListener(IntPtr hwnd);
        public const int WM_CLIPBOARDUPDATE = 0x031D;

        public static void EnableDpiAwareness()
        {
            // Per-monitor v2 (Win10 1703+) so captures are 1:1 physical pixels on every monitor.
            try { if (SetProcessDpiAwarenessContext(new IntPtr(-4))) return; } catch { }
            try { SetProcessDPIAware(); } catch { }
        }

        public static bool IsDown(int vk) { return (GetAsyncKeyState(vk) & 0x8000) != 0; }

        public static void SendMaskKey()
        {
            keybd_event(VK_MASK, 0, 0, UIntPtr.Zero);
            keybd_event(VK_MASK, 0, 2 /*KEYUP*/, UIntPtr.Zero);
        }
    }
}
