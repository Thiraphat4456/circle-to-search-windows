using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace CircleToSearch
{
    static class GoogleLens
    {
        // Remove upload pages left over from earlier sessions.
        public static void CleanOldFiles()
        {
            try
            {
                foreach (var f in Directory.GetFiles(TempDir(), "lens_*.html"))
                    if (File.GetCreationTime(f) < DateTime.Now.AddHours(-1)) File.Delete(f);
            }
            catch { }
        }

        static string TempDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), "CircleToSearch");
            Directory.CreateDirectory(dir);
            return dir;
        }

        // Opens Google Lens in the default browser with the image already uploaded.
        // Works by writing a tiny local HTML page that builds a File from base64 and auto-submits
        // the same multipart form Chrome itself uses (field "encoded_image" -> lens.google.com/v3/upload).
        // The upload happens inside the user's browser, so their Google sign-in / consent / language apply.
        public static void Search(Bitmap img)
        {
            byte[] bytes;
            string mime;
            Encode(img, out bytes, out mime);
            string b64 = Convert.ToBase64String(bytes);
            long st = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
            string action = "https://lens.google.com/v3/upload?hl=th&st=" + st;
            string ext = mime == "image/png" ? "png" : "jpg";

            var sb = new StringBuilder();
            sb.Append("<!DOCTYPE html><html><head><meta charset=\"utf-8\"><title>Circle to Search</title>");
            sb.Append("<style>html,body{height:100%;margin:0}body{display:flex;align-items:center;justify-content:center;");
            sb.Append("background:#0f1115;color:#e8eaed;font:16px 'Segoe UI','Leelawadee UI',Tahoma,sans-serif}");
            sb.Append(".c{text-align:center}.r{width:44px;height:44px;margin:0 auto 18px;border-radius:50%;");
            sb.Append("border:4px solid #333;border-top-color:#8ab4f8;border-right-color:#c58af9;animation:s .8s linear infinite}");
            sb.Append("@keyframes s{to{transform:rotate(360deg)}}img{max-width:240px;max-height:160px;border-radius:12px;margin-top:18px;opacity:.85}");
            sb.Append("button{margin-top:16px;padding:8px 18px;border-radius:20px;border:0;background:#8ab4f8;color:#000;font:inherit;cursor:pointer}</style></head><body>");
            sb.Append("<div class=\"c\"><div class=\"r\"></div><div>" + Strings.LensUploading + "</div>");
            sb.Append("<img id=\"p\" alt=\"\"><br><button id=\"b\" style=\"display:none\">" + Strings.LensRetry + "</button></div>");
            sb.Append("<form id=\"f\" method=\"POST\" enctype=\"multipart/form-data\" action=\"").Append(action).Append("\">");
            sb.Append("<input type=\"file\" name=\"encoded_image\" id=\"i\" style=\"display:none\">");
            sb.Append("<input type=\"hidden\" name=\"processed_image_dimensions\" value=\"").Append(img.Width).Append(",").Append(img.Height).Append("\">");
            sb.Append("</form><script>\n");
            sb.Append("var d=\"").Append(b64).Append("\";\n");
            sb.Append("var t=\"").Append(mime).Append("\";\n");
            sb.Append("document.getElementById('p').src='data:'+t+';base64,'+d;\n");
            sb.Append("function go(){try{var s=atob(d),u=new Uint8Array(s.length);for(var k=0;k<s.length;k++)u[k]=s.charCodeAt(k);");
            sb.Append("var dt=new DataTransfer();dt.items.add(new File([u],'circle.").Append(ext).Append("',{type:t}));");
            sb.Append("document.getElementById('i').files=dt.files;document.getElementById('f').submit();}");
            sb.Append("catch(e){document.querySelector('.c div:nth-child(2)').textContent='").Append(Strings.LensFallback).Append("';}}\n");
            sb.Append("var b=document.getElementById('b');b.onclick=go;setTimeout(function(){b.style.display='inline-block'},6000);go();\n");
            sb.Append("</script></body></html>");

            string path = Path.Combine(TempDir(), "lens_" + DateTime.Now.Ticks + ".html");
            File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));

            // Clipboard copy as a fallback (paste into Lens manually if needed).
            try { ImageActions.CopyToClipboard(img); } catch { }

            var psi = new ProcessStartInfo(path);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        static void Encode(Bitmap src, out byte[] bytes, out string mime)
        {
            // Lens doesn't need huge images; keep uploads quick.
            const int maxSide = 2000;
            Bitmap img = src;
            bool owned = false;
            int longest = Math.Max(src.Width, src.Height);
            if (longest > maxSide)
            {
                double sc = (double)maxSide / longest;
                img = new Bitmap(Math.Max(1, (int)(src.Width * sc)), Math.Max(1, (int)(src.Height * sc)), PixelFormat.Format24bppRgb);
                using (var g = Graphics.FromImage(img))
                {
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.DrawImage(src, 0, 0, img.Width, img.Height);
                }
                owned = true;
            }
            try
            {
                using (var ms = new MemoryStream())
                {
                    img.Save(ms, ImageFormat.Png);
                    if (ms.Length <= 1500 * 1024) { bytes = ms.ToArray(); mime = "image/png"; return; }
                }
                using (var ms = new MemoryStream())
                {
                    ImageCodecInfo jpg = null;
                    foreach (var c in ImageCodecInfo.GetImageEncoders()) if (c.MimeType == "image/jpeg") jpg = c;
                    var ep = new EncoderParameters(1);
                    ep.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 90L);
                    img.Save(ms, jpg, ep);
                    bytes = ms.ToArray(); mime = "image/jpeg";
                }
            }
            finally { if (owned) img.Dispose(); }
        }
    }
}
