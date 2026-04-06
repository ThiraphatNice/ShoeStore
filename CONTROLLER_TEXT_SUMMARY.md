# สรุปการทำงานของ Controller ที่ต้องแก้ภาษา

## 1. CartController.cs

### Index()
- ทำงาน: แสดงหน้าตะกร้าสินค้าของผู้ใช้ปัจจุบัน
- รายการที่โหลด: `CartItem` ของผู้ใช้, `ProductVariant`, `Product`, `Category`
- ข้อความไม่เพี้ยน: ไม่มี

### CheckProfileStatus()
- ทำงาน: ตรวจสอบสถานะข้อมูลโปรไฟล์ผู้ใช้
- คืนค่า JSON ที่มีฟิลด์ `MissingFields` และ `ProfileUrl`
- ข้อความไม่เพี้ยน: ไม่มี

### AddItem([FromBody] AddCartItemRequest request)
- ทำงาน:
  - ตรวจสอบ `ModelState`
  - ป้องกัน Admin/Staff ซื้อสินค้า
  - หาค่า variant จาก `ProductVariants`
  - ถ้าไม่มี variant คืน error
  - ถ้า stock ไม่พอ คืนข้อความเหมาะสม
  - เพิ่มรายการหรืออัปเดตจำนวนในตะกร้า
  - ปรับ `StockQuantity` และอัปเดต `Product.StockTotal`
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "ข้อมูลไม่ถูกต้อง" หรือ "ข้อมูลการเพิ่มสินค้าผิดพลาด"
  - ไม่พบ variant: "ไม่พบสินค้าที่เลือก"
  - สินค้าหมด: "สินค้าหมด"
  - สินค้าคงเหลือไม่พอ: "เหลือสินค้า {available} ชิ้น"

### UpdateItem([FromBody] UpdateCartItemRequest request)
- ทำงาน:
  - ตรวจสอบ `ModelState`
  - ป้องกัน Admin/Staff ซื้อสินค้า
  - ดึง `CartItem` ของผู้ใช้พร้อม variant
  - ถ้าไม่พบรายการคืน error
  - ถ้าไม่เปลี่ยนจำนวนยังคืน totals เดิม
  - ถ้าเพิ่มจำนวนจะเช็ค stock
  - ปรับ `variant.StockQuantity` และจำนวนในตะกร้า
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "ข้อมูลไม่ถูกต้อง"
  - ไม่พบรายการ: "ไม่พบรายการในตะกร้า"
  - stock ไม่พอ: "เหลือสินค้า {available} ชิ้น" หรือ "สินค้าหมด"

### RemoveItem([FromBody] RemoveCartItemRequest request)
- ทำงาน:
  - ตรวจสอบ `ModelState`
  - ป้องกัน Admin/Staff ซื้อสินค้า
  - หารายการในตะกร้าของผู้ใช้
  - คืนสต็อกให้ variant แล้วลบรายการ
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "ข้อมูลไม่ถูกต้อง"
  - ไม่พบรายการ: "ไม่พบรายการในตะกร้า"

### BuildProfileStatus(User user)
- ทำงาน: สร้าง `ProfileStatusViewModel`
- เช็ค field ที่ขาดหาย:
  - `Fullname`
  - `Email`
  - `Phone`
  - `Address`
- ข้อความที่ควรใช้:
  - `Fullname` missing -> "ชื่อ-นามสกุล"
  - `Email` missing -> "อีเมล"
  - `Phone` missing -> "เบอร์โทร"
  - `Address` missing -> "ที่อยู่"

---

## 2. StaffController.cs

### CreateStaff(CreateStaffViewModel model)
- ทำงาน:
  - ตรวจสอบ `ModelState`
  - ตรวจสอบ role ที่เลือกต้องมีอยู่ และต้องไม่ใช่ Admin
  - ตรวจสอบ email ซ้ำ
  - สร้างผู้ใช้ใหม่ในฐานข้อมูล
  - เก็บผลลัพธ์ที่ `TempData` เพื่อนำไปแสดงใน view
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "กรุณากรอกข้อมูลให้ครบถ้วน"
  - role invalid / admin role: "บทบาทไม่ถูกต้อง" หรือ "ไม่สามารถสร้างผู้ใช้ด้วยบทบาทนี้ได้"
  - email ซ้ำ: "อีเมลนี้ถูกใช้งานแล้ว"
  - success: "สร้าง {role.RoleName} สำหรับ {model.FullName} เรียบร้อยแล้ว"

### GetProductDetail(int id)
- ทำงาน:
  - คืนข้อมูลสินค้าในรูปแบบ JSON
  - รวม `Category` และ `ProductVariants`
- ข้อความที่ควรใช้:
  - สินค้าไม่พบ: "ไม่พบข้อมูลสินค้าที่ค้นหา"

### UpdateProductInfo([FromBody] UpdateProductRequest request)
- ทำงาน:
  - ตรวจสอบ `ModelState`
  - หา `Product` ตาม `ProductId`
  - หา `Category` ตาม `CategoryId`
  - ปรับข้อมูลสินค้า (ชื่อ, คำอธิบาย, รูป, ราคา, ส่วนลด, หมวดหมู่, จำกัดจำนวน)
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "ข้อมูลสินค้าไม่ถูกต้อง"
  - product ไม่พบ: "ไม่พบข้อมูลสินค้าที่ต้องการแก้ไข"
  - category ไม่พบ: "ไม่พบหมวดหมู่สินค้า"

### UpdateVariantStock([FromBody] UpdateVariantStockRequest request)
- ทำงาน:
  - ตรวจสอบ `ModelState`
  - หารายการ `ProductVariant` ตาม `ProductId`, `Size`, `Color`
  - ปรับ `StockQuantity`
  - อัปเดตผลรวมสต็อกของสินค้า
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "ข้อมูลสต็อกสินค้าไม่ถูกต้อง"
  - variant ไม่พบ: "ไม่พบรูปแบบสินค้าที่ระบุ หรือขนาด/สีไม่ถูกต้อง"

### AddVariant([FromBody] AddVariantRequest request)
- ทำงาน:
  - trim `Size` และ `Color`
  - validate ข้อมูลใหม่
  - ตรวจสอบ duplicate variant
  - สร้าง `ProductVariant` ใหม่
- ข้อความที่ควรแก้:
  - `ModelState` invalid: "ข้อมูลรูปแบบสินค้าไม่ถูกต้อง"
  - ขนาด/สีซ้ำ: "รูปแบบสินค้า (ขนาด/สี) นี้มีอยู่แล้ว"

---

## คำแนะนำในการแก้โค้ดเฉพาะ Controller

- แก้เฉพาะข้อความใน `Json(new { success = false, message = ... })`
- แก้เฉพาะข้อความใน `TempData["StaffError"]` และ `TempData["StaffStatus"]`
- อย่าแก้ `View` หรือ `cshtml` ถ้าไม่แน่ใจ เพราะตอนนี้เป้าหมายคือแก้เฉพาะข้อความ response / error ใน Controller
- ถ้าต้องการเพิ่มข้อความ status แบบไทยที่ชัดเจนขึ้น ให้ใช้คำว่า:
  - "ข้อมูลไม่ถูกต้อง"
  - "ไม่พบรายการในตะกร้า"
  - "ไม่พบสินค้าที่เลือก"
  - "เหลือสินค้า {available} ชิ้น"
  - "กรุณากรอกข้อมูลให้ครบถ้วน"
  - "อีเมลนี้ถูกใช้งานแล้ว"
  - "ไม่พบหมวดหมู่สินค้า"
  - "รูปแบบสินค้า (ขนาด/สี) นี้มีอยู่แล้ว"

> หมายเหตุ: ข้อความที่เพี้ยนในไฟล์ตอนนี้น่าจะเกิดจากการ encoding ผิดพลาด จึงควรแก้ด้วยข้อความไทยที่อ่านได้ตามตัวอย่างด้านบน
