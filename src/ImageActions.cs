using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CircleToSearch
{
    static class ImageActions
    {
        public static void CopyToClipboard(Bitmap img)
        {
            var data = new DataObject();
            data.SetImage(img);
            // Also put a PNG stream on the clipboard — keeps quality in apps that understand it (Discord, Chrome, Office).
            var ms = new MemoryStream();
            img.Save(ms, ImageFormat.Png);
            data.SetData("PNG", false, ms);
            for (int i = 0; i < 5; i++)
            {
                try { Clipboard.SetDataObject(data, true); return; }
                catch (ExternalException) { Thread.Sleep(60); }
            }
        }

        public static void SaveAs(Bitmap img)
        {
            using (var dlg = new SaveFileDialog())
            {
                dlg.Filter = "PNG|*.png|JPEG|*.jpg";
                dlg.FileName = "Screenshot " + DateTime.Now.ToString("yyyy-MM-dd HHmmss");
                dlg.InitialDirectory = ScreenshotsDir();
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    var fmt = dlg.FilterIndex == 2 ? ImageFormat.Jpeg : ImageFormat.Png;
                    img.Save(dlg.FileName, fmt);
                }
            }
        }

        public static void AutoSave(Bitmap img)
        {
            string path = Path.Combine(ScreenshotsDir(), "Screenshot " + DateTime.Now.ToString("yyyy-MM-dd HHmmss") + ".png");
            img.Save(path, ImageFormat.Png);
        }

        static string ScreenshotsDir()
        {
            string dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Screenshots");
            try { Directory.CreateDirectory(dir); } catch { }
            return dir;
        }
    }
}
