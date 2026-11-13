using System.Globalization;

namespace CircleToSearch
{
    enum UiLanguage { Auto = 0, Thai = 1, English = 2 }

    // All user-facing text in one place. Thai or English, picked from the setting
    // (Auto = follow the Windows display language: Thai if Windows is Thai, otherwise English).
    static class Strings
    {
        static bool th;

        public static void Apply(UiLanguage lang)
        {
            th = lang == UiLanguage.Thai ||
                 (lang == UiLanguage.Auto && CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "th");
        }

        static string T(string thai, string english) { return th ? thai : english; }

        // Google Lens result language
        public static string LensLanguage { get { return th ? "th" : "en"; } }

        // Tray
        public static string HookFailed { get { return T("ตั้งคีย์ลัดไม่สำเร็จ — คลิกไอคอนนี้ที่ถาดระบบเพื่อใช้งานแทนได้", "Couldn't register the hotkey — click this tray icon to use it instead"); } }
        public static string ReadyFormat { get { return T("พร้อมใช้งาน — กด {0} แล้ววงรอบสิ่งที่อยากค้นหา", "Ready — press {0} and circle anything to search it"); } }
        public static string ReadyWindowsSnipKept { get { return T("Win+Shift+S ยังเป็นของ Windows ตามเดิม", "Win+Shift+S still works as Windows' own snipping tool"); } }
        public static string HotkeyNowFormat { get { return T("คีย์ลัดตอนนี้คือ {0}", "Hotkey is now {0}"); } }
        public static string CaptureFailed { get { return T("จับภาพหน้าจอไม่สำเร็จ", "Couldn't capture the screen"); } }
        public static string Copied { get { return T("คัดลอกภาพแล้ว — วาง (Ctrl+V) ได้เลย", "Image copied — paste it with Ctrl+V"); } }
        public static string ErrorFormat { get { return T("เกิดข้อผิดพลาด: {0}", "Something went wrong: {0}"); } }

        // Tray menu
        public static string MenuOpenFormat { get { return T("วงเพื่อค้นหา  ({0})", "Circle to Search  ({0})"); } }
        public static string MenuSnipPopup { get { return T("หลังแคปด้วย Win+Shift+S ให้ขึ้นปุ่มค้นหาด้วย Google", "After a Win+Shift+S snip, offer a Google search"); } }
        public static string MenuOverrideWinShiftS { get { return T("ให้ Win+Shift+S เปิดโหมดวงแทนเครื่องมือแคปของ Windows", "Use Win+Shift+S for Circle to Search (replaces Windows' snipping)"); } }
        public static string MenuSearchImmediately { get { return T("วงเสร็จแล้วค้นหาทันที (แบบ Samsung)", "Search as soon as I finish circling (like Samsung)"); } }
        public static string MenuAutoSave { get { return T("บันทึกภาพครอปอัตโนมัติไปที่ Pictures\\Screenshots", "Auto-save captures to Pictures\\Screenshots"); } }
        public static string MenuStartup { get { return T("เปิดอัตโนมัติเมื่อเข้า Windows", "Start with Windows"); } }
        public static string MenuLanguage { get { return T("ภาษา / Language", "Language / ภาษา"); } }
        public static string MenuLanguageAuto { get { return T("อัตโนมัติ (ตาม Windows)", "Automatic (match Windows)"); } }
        public const string MenuLanguageThai = "ไทย";
        public const string MenuLanguageEnglish = "English";
        public static string MenuExit { get { return T("ออก", "Exit"); } }

        // Overlay
        public static string ToolLasso { get { return T("วงเพื่อค้นหา", "Circle"); } }
        public static string ToolRect { get { return T("สี่เหลี่ยม", "Rectangle"); } }
        public static string ToolFull { get { return T("ทั้งจอ", "Full screen"); } }
        public static string HintLasso { get { return T("วาดวงรอบสิ่งที่อยากรู้ แล้วค้นหาด้วย Google Lens   •   Esc เพื่อปิด", "Circle anything you want to know about and search it with Google Lens   •   Esc to close"); } }
        public static string HintRect { get { return T("ลากเพื่อครอปภาพ   •   Enter = คัดลอก   •   Esc เพื่อปิด", "Drag to capture   •   Enter = copy   •   Esc to close"); } }
        public static string ActionSearch { get { return T("ค้นหาด้วย Google", "Search with Google"); } }
        public static string ActionCopy { get { return T("คัดลอก", "Copy"); } }
        public static string ActionSave { get { return T("บันทึก", "Save"); } }

        // Snip popup
        public static string SearchThisImage { get { return T("ค้นหาภาพนี้ด้วย Google Lens", "Search this image with Google Lens"); } }

        // Lens upload page (HTML)
        public static string LensUploading { get { return T("กำลังส่งภาพไปค้นหาด้วย Google Lens…", "Sending your image to Google Lens…"); } }
        public static string LensRetry { get { return T("ค้นหาอีกครั้ง", "Search again"); } }
        public static string LensFallback { get { return T("เบราว์เซอร์นี้ส่งภาพอัตโนมัติไม่ได้ — ภาพถูกคัดลอกไว้แล้ว เปิด lens.google.com แล้วกด Ctrl+V", "This browser couldn't send the image automatically — it's on your clipboard: open lens.google.com and press Ctrl+V"); } }
    }
}
