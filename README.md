# Circle to Search for Windows

ฟีเจอร์ **Circle to Search** แบบมือถือ Samsung/Android มาไว้บน Windows 10/11 — วงรอบอะไรก็ได้บนจอ แล้วค้นหาด้วย Google Lens ทันที

เป็นตัว**เสริม**ของเครื่องมือแคปจอ Windows ไม่ได้ไปแทนที่: Win+Shift+S ยังทำงานเหมือนเดิมทุกอย่าง

*Bring Android-style Circle to Search to Windows: circle anything on screen and search it with Google Lens. It's an add-on — Windows' own Win+Shift+S keeps working as usual.*

## ดาวน์โหลด

ไปที่หน้า **[Releases](../../releases/latest)** แล้วโหลด `CircleToSearch.exe` → ดับเบิลคลิกใช้ได้เลย ไม่ต้องติดตั้ง

- ใช้ได้บน Windows 10 / 11 (ใช้ .NET Framework 4 ที่มีอยู่แล้วในเครื่อง)
- ถ้า SmartScreen เตือน (โปรแกรมไม่มีลายเซ็นดิจิทัล) ให้กด **More info → Run anyway**

## วิธีใช้

| คีย์ | ทำอะไร |
|---|---|
| **Win+Shift+C** | เปิดโหมดวง: จอค้างไว้ → วาดวงรอบสิ่งที่อยากรู้ → **ค้นหาด้วย Google** |
| **Win+Shift+S** | แคปจอของ Windows ตามปกติ — แคปเสร็จจะมีปุ่ม **"ค้นหาภาพนี้ด้วย Google Lens"** ลอยขึ้นมา (ไม่กดก็หายเอง) |

ในโหมดวง:

| คีย์ / เมาส์ | ทำอะไร |
|---|---|
| ลากเป็นวง | เลือกสิ่งที่จะค้นหา (คลิกจุดเดียวจะเลือกพื้นที่รอบๆ ให้) |
| Enter, G, L | ค้นหาด้วย Google Lens |
| Ctrl+C / Ctrl+S | คัดลอก / บันทึกภาพ |
| Tab | สลับโหมด วง ↔ สี่เหลี่ยม |
| F | เลือกทั้งจอ |
| คลิกขวา / Esc | ยกเลิก / ปิด |

คลิกขวาที่ไอคอนในถาดระบบเพื่อตั้งค่า: เปิดพร้อม Windows, วงเสร็จแล้วค้นหาทันที (แบบ Samsung), บันทึกภาพอัตโนมัติ, ปิดปุ่มลอยหลังแคป ฯลฯ

## ทำงานยังไง

- จับภาพทุกจอ (รองรับหลายจอและจอ DPI สูง) แล้ววาดทับด้วยหน้าต่างเต็มจอ
- ส่งภาพไป Google Lens ผ่าน**เบราว์เซอร์ของคุณเอง** (สร้างหน้า HTML ชั่วคราวที่ส่งฟอร์มแบบเดียวกับที่ Chrome ใช้) — ไม่มีเซิร์ฟเวอร์ตัวกลาง โปรแกรมไม่เก็บหรือส่งข้อมูลไปที่อื่น
- ภาพที่ค้นหาจะถูกคัดลอกลงคลิปบอร์ดด้วย ถ้าเบราว์เซอร์ส่งไม่สำเร็จ เปิด lens.google.com แล้ว Ctrl+V ได้

## Build เอง

```
build.bat
```

ใช้คอมไพเลอร์ C# ที่ติดมากับ Windows (`csc.exe` ของ .NET Framework 4) ไม่ต้องลง Visual Studio

โครงสร้างโค้ด (`src/`):

| ไฟล์ | หน้าที่ |
|---|---|
| `Program.cs` | จุดเริ่มต้น, กันเปิดซ้ำ, DPI |
| `TrayContext.cs` | ไอคอนถาดระบบ, เมนู, ต่อคีย์ลัด/คลิปบอร์ดเข้ากับหน้าจอ |
| `HotkeyHook.cs` | Low-level keyboard hook สำหรับ Win+Shift+C (และสังเกต Win+Shift+S) |
| `OverlayForm.cs` | หน้าจอวง / ครอปแบบเต็มจอ |
| `SnipPopup.cs` | ปุ่มลอยหลังแคปด้วย Win+Shift+S |
| `GoogleLens.cs` | ส่งภาพไป Google Lens |
| `ImageActions.cs` | คัดลอก / บันทึกภาพ |
| `Settings.cs` | การตั้งค่า (registry `HKCU\Software\CircleToSearch`) |
| `Strings.cs` | ข้อความทั้งหมดใน UI |
| `UI.cs`, `Icons.cs`, `Native.cs` | ฟอนต์/สี, ไอคอนแบบเวกเตอร์, Win32 interop |

## ถอนการติดตั้ง

คลิกขวาไอคอน → เอาติ๊ก "เปิดอัตโนมัติ" ออก → ออก แล้วลบไฟล์ทิ้ง (ลบ `HKCU\Software\CircleToSearch` ได้ถ้าต้องการ)

## License

[MIT](LICENSE) — ไม่ได้เกี่ยวข้องกับ Google หรือ Samsung. Google Lens เป็นเครื่องหมายการค้าของ Google LLC
