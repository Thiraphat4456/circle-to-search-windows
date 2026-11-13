using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("Circle to Search")]
[assembly: System.Reflection.AssemblyProduct("Circle to Search for Windows")]
[assembly: System.Reflection.AssemblyVersion("1.1.0.0")]

namespace CircleToSearch
{
    static class Program
    {
        public const string AppName = "Circle to Search";

        [STAThread]
        static void Main(string[] args)
        {
            bool created;
            using (var mutex = new Mutex(true, "Local\\CircleToSearch_SingleInstance_v1", out created))
            {
                if (!created)
                {
                    // Already running: ask the running instance to open the overlay.
                    IntPtr h = Native.FindWindow(null, TrayContext.MsgWindowTitle);
                    if (h != IntPtr.Zero) Native.PostMessage(h, TrayContext.WM_APP_SHOW, IntPtr.Zero, IntPtr.Zero);
                    return;
                }
                Native.EnableDpiAwareness();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.ThreadException += delegate(object s, ThreadExceptionEventArgs e) { Log(e.Exception); };
                AppDomain.CurrentDomain.UnhandledException += delegate(object s, UnhandledExceptionEventArgs e) { Log(e.ExceptionObject as Exception); };
                bool showNow = Array.IndexOf(args, "--snip") >= 0;
                Application.Run(new TrayContext(showNow));
            }
        }

        public static void Log(Exception ex)
        {
            try
            {
                string dir = Path.Combine(Path.GetTempPath(), "CircleToSearch");
                Directory.CreateDirectory(dir);
                File.AppendAllText(Path.Combine(dir, "error.log"), DateTime.Now.ToString("s") + "  " + ex + Environment.NewLine);
            }
            catch { }
        }
    }
}
