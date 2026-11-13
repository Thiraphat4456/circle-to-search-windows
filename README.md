# Circle to Search for Windows

**English** · [ภาษาไทย](#ภาษาไทย)

Android-style **Circle to Search** on Windows 10/11: circle anything on your screen and search it with Google Lens.

It's an **add-on**, not a replacement: Windows' own Win+Shift+S keeps working exactly as before.

## Download

Get `CircleToSearch.exe` from **[Releases](../../releases/latest)** and double-click it. No install needed.

- Runs on Windows 10 / 11 (uses the .NET Framework 4 that's already on your PC)
- If SmartScreen warns you (the app isn't code-signed), click **More info → Run anyway**

## How to use

| Key | What it does |
|---|---|
| **Win+Shift+C** | Circle mode: the screen freezes → circle what you want → **Search with Google** |
| **Win+Shift+S** | Windows' normal snipping tool. After you snip, a **"Search this image with Google Lens"** button pops up (it disappears on its own if you ignore it) |

In circle mode:

| Key / mouse | What it does |
|---|---|
| Draw a circle | Select what to search (a single click selects the area around it) |
| Enter, G, L | Search with Google Lens |
| Ctrl+C / Ctrl+S | Copy / save the image |
| Tab | Switch between circle and rectangle |
| F | Select the whole screen |
| Right-click / Esc | Cancel / close |

Right-click the tray icon for settings: language (Thai / English / match Windows), start with Windows, search as soon as you finish circling (like Samsung), auto-save captures, turn off the pop-up after a snip, and more.

## How it works

- Captures every monitor (multi-monitor and high-DPI aware) and draws a full-screen overlay on top
- Sends the image to Google Lens **through your own browser**: it writes a temporary HTML page that submits the same form Chrome uses. There's no middle server, and the app doesn't store or send your data anywhere else
- The searched image is also copied to the clipboard. If the browser can't send it, open lens.google.com and press Ctrl+V

## Build it yourself

```
build.bat
```

Uses the C# compiler that ships with Windows (`csc.exe` from .NET Framework 4). No Visual Studio needed.

Code layout (`src/`):

| File | Purpose |
|---|---|
| `Program.cs` | Entry point, single instance, DPI |
| `TrayContext.cs` | Tray icon and menu; wires the hotkey and clipboard to the screens |
| `HotkeyHook.cs` | Low-level keyboard hook for Win+Shift+C (and watching Win+Shift+S) |
| `OverlayForm.cs` | Full-screen circle / rectangle selection |
| `SnipPopup.cs` | Pop-up button after a Win+Shift+S snip |
| `GoogleLens.cs` | Sends the image to Google Lens |
| `ImageActions.cs` | Copy / save images |
| `Settings.cs` | Settings (registry `HKCU\Software\CircleToSearch`) |
| `Strings.cs` | All UI text, Thai and English |
| `UI.cs`, `Icons.cs`, `Native.cs` | Fonts/colors, vector icons, Win32 interop |

## Uninstall

Right-click the tray icon → untick "Start with Windows" → Exit, then delete the file. You can also delete `HKCU\Software\CircleToSearch` if you want.

---

## ภาษาไทย

ฟีเจอร์ **Circle to Search** แบบมือถือ Samsung/Android มาไว้บน Windows 10/11 วงรอบอะไรก็ได้บนจอ แล้วค้นหาด้วย Google Lens ได้ทันที

เป็นตัว**เสริม**ของเครื่องมือแคปจอ Windows ไม่ได้ไปแทนที่ Win+Shift+S ยังทำงานเหมือนเดิมทุกอย่าง

### ดาวน์โหลด

ไปที่หน้า **[Releases](../../releases/latest)** แล้วโหลด `CircleToSearch.exe` ดับเบิลคลิกใช้ได้เลย ไม่ต้องติดตั้ง

- ใช้ได้บน Windows 10 / 11 (ใช้ .NET Framework 4 ที่มีอยู่แล้วในเครื่อง)
- ถ้า SmartScreen เตือน (โปรแกรมไม่มีลายเซ็นดิจิทัล) ให้กด **More info → Run anyway**

### วิธีใช้

| คีย์ | ทำอะไร |
|---|---|
| **Win+Shift+C** | เปิดโหมดวง: จอค้างไว้ → วาดวงรอบสิ่งที่อยากรู้ → **ค้นหาด้วย Google** |
| **Win+Shift+S** | แคปจอของ Windows ตามปกติ แคปเสร็จจะมีปุ่ม **"ค้นหาภาพนี้ด้วย Google Lens"** ลอยขึ้นมา (ไม่กดก็หายเอง) |

ในโหมดวง:

| คีย์ / เมาส์ | ทำอะไร |
|---|---|
| ลากเป็นวง | เลือกสิ่งที่จะค้นหา (คลิกจุดเดียวจะเลือกพื้นที่รอบๆ ให้) |
| Enter, G, L | ค้นหาด้วย Google Lens |
| Ctrl+C / Ctrl+S | คัดลอก / บันทึกภาพ |
| Tab | สลับโหมด วง ↔ สี่เหลี่ยม |
| F | เลือกทั้งจอ |
| คลิกขวา / Esc | ยกเลิก / ปิด |

คลิกขวาที่ไอคอนในถาดระบบเพื่อตั้งค่า เช่น ภาษา (ไทย / English / ตาม Windows), เปิดพร้อม Windows, วงเสร็จแล้วค้นหาทันที (แบบ Samsung), บันทึกภาพอัตโนมัติ, ปิดปุ่มลอยหลังแคป

### ทำงานยังไง

- จับภาพทุกจอ (รองรับหลายจอและจอ DPI สูง) แล้ววาดทับด้วยหน้าต่างเต็มจอ
- ส่งภาพไป Google Lens ผ่าน**เบราว์เซอร์ของคุณเอง** โดยสร้างหน้า HTML ชั่วคราวที่ส่งฟอร์มแบบเดียวกับที่ Chrome ใช้ ไม่มีเซิร์ฟเวอร์ตัวกลาง และโปรแกรมไม่เก็บหรือส่งข้อมูลไปที่อื่น
- ภาพที่ค้นหาจะถูกคัดลอกลงคลิปบอร์ดด้วย ถ้าเบราว์เซอร์ส่งไม่สำเร็จ ให้เปิด lens.google.com แล้วกด Ctrl+V

### Build เอง

รัน `build.bat` ซึ่งใช้คอมไพเลอร์ C# ที่ติดมากับ Windows ไม่ต้องลง Visual Studio (โครงสร้างโค้ดดูในตารางภาษาอังกฤษด้านบน)

### ถอนการติดตั้ง

คลิกขวาไอคอน → เอาติ๊ก "เปิดอัตโนมัติ" ออก → ออก แล้วลบไฟล์ทิ้ง (ลบ `HKCU\Software\CircleToSearch` ได้ถ้าต้องการ)

---

## License

[MIT](LICENSE). Not affiliated with Google or Samsung. Google Lens is a trademark of Google LLC.
