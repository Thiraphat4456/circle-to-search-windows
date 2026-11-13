namespace CircleToSearch
{
    // All user-facing text in one place (Thai UI).
    static class Strings
    {
        // Tray
        public const string HookFailed = "ตั้งคีย์ลัดไม่สำเร็จ — คลิกไอคอนนี้ที่ถาดระบบเพื่อใช้งานแทนได้";
        public const string ReadyFormat = "พร้อมใช้งาน — กด {0} แล้ววงรอบสิ่งที่อยากค้นหา";
        public const string ReadyWindowsSnipKept = "Win+Shift+S ยังเป็นของ Windows ตามเดิม";
        public const string HotkeyNowFormat = "คีย์ลัดตอนนี้คือ {0}";
        public const string CaptureFailed = "จับภาพหน้าจอไม่สำเร็จ";
        public const string Copied = "คัดลอกภาพแล้ว — วาง (Ctrl+V) ได้เลย";
        public const string ErrorFormat = "เกิดข้อผิดพลาด: {0}";

        // Tray menu
        public const string MenuOpenFormat = "วงเพื่อค้นหา  ({0})";
        public const string MenuSnipPopup = "หลังแคปด้วย Win+Shift+S ให้ขึ้นปุ่มค้นหาด้วย Google";
        public const string MenuOverrideWinShiftS = "ให้ Win+Shift+S เปิดโหมดวงแทนเครื่องมือแคปของ Windows";
        public const string MenuSearchImmediately = "วงเสร็จแล้วค้นหาทันที (แบบ Samsung)";
        public const string MenuAutoSave = "บันทึกภาพครอปอัตโนมัติไปที่ Pictures\\Screenshots";
        public const string MenuStartup = "เปิดอัตโนมัติเมื่อเข้า Windows";
        public const string MenuExit = "ออก";

        // Overlay
        public const string ToolLasso = "วงเพื่อค้นหา";
        public const string ToolRect = "สี่เหลี่ยม";
        public const string ToolFull = "ทั้งจอ";
        public const string HintLasso = "วาดวงรอบสิ่งที่อยากรู้ แล้วค้นหาด้วย Google Lens   •   Esc เพื่อปิด";
        public const string HintRect = "ลากเพื่อครอปภาพ   •   Enter = คัดลอก   •   Esc เพื่อปิด";
        public const string ActionSearch = "ค้นหาด้วย Google";
        public const string ActionCopy = "คัดลอก";
        public const string ActionSave = "บันทึก";

        // Snip popup
        public const string SearchThisImage = "ค้นหาภาพนี้ด้วย Google Lens";

        // Lens upload page (HTML)
        public const string LensUploading = "กำลังส่งภาพไปค้นหาด้วย Google Lens…";
        public const string LensRetry = "ค้นหาอีกครั้ง";
        public const string LensFallback = "เบราว์เซอร์นี้ส่งภาพอัตโนมัติไม่ได้ — ภาพถูกคัดลอกไว้แล้ว เปิด lens.google.com แล้วกด Ctrl+V";
    }
}
