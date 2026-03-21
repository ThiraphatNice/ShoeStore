## ShoeStore Web Application 👟

**ShoeStore** คือ ASP.NET Core MVC application สำหรับร้านขายรองเท้าออนไลน์ ครบจากการเลือกสินค้า, ตะกร้าสินค้า, ไปจนถึงการจัดการสต็อก 🛍️

### 🏗️ โครงสร้างโปรเจกต์

| Layer | ทำหน้าที่อะไร |
| --- | --- |
| **Controllers** | รับ HTTP requests → ค้นหาข้อมูลจาก database ผ่าน `ShoeStoreContext` → เตรียมข้อมูล send ให้ View ใช้ |
| **Models / EF Core** | `Models/db/*` คือ entity classes (เหมือนตัวแทนของ database tables) เช่น Users, Products, Categories ฯลฯ |
| **ViewModels** | ไฟล์สำหรับ "ขนส่งข้อมูล" จากตัวระบบไปให้ View ใช้ (เช่น LoginViewModel สำหรับหน้า Login) |
| **Views** | Razor pages (ไฟล์ `.cshtml`) คือหน้าเว็บ มี HTML + CSS + JavaScript ไว้แสดงและจัดการ UI |

### 📋 Controllers ทำอะไรบ้าง

#### 1. **AccountController** (`Controllers/AccountController.cs`)
ควบคุมเรื่องการ login/register/logout/เปลี่ยน password 🔐
- **ทำอะไร:**
  - ตรวจสอบ email + password ที่ user ป้อน
  - สร้างข้อมูล user ใหม่ในฐานข้อมูล
  - จัดการเรื่อง session (login/logout)
  - แสดงข้อมูล profile ของ user
- **ใช้ Models:** `Users` table + `Roles` table
- **ใช้ ViewModels:** `LoginViewModel`, `RegisterViewModel`, `ForgetPasswordViewModel`
- **View ที่ใช้:** `Views/Account/` เก็บหน้า Login, Register, เปลี่ยน password

#### 2. **HomeController** (`Controllers/HomeController.cs`)
แสดงหน้าแรก + สินค้าโดดเด่น 🏠
- **ทำอะไร:**
  - ดึงข้อมูลสินค้าจาก database
  - จัดกำหนด: สินค้าโดดเด่น, limited edition, จัดแยกตามหมวดหมู่
  - จัดเรียงข้อมูลให้พร้อม display (HomePageViewModel)
- **ใช้ Models:** `Products`, `Categories`, `ProductVariants` tables
- **View ที่ใช้:** `Views/Home/Index.cshtml` + `_ProductSection.cshtml`

#### 3. **StaffController** (`Controllers/StaffController.cs`)
ของเจ้าหน้าที่ขาย พนักงาน กับผู้จัดการ 👨‍💼
ใหญ่ที่สุด มีหลายเรื่อง:

**A. Dashboard** - หน้าหลักมีเมนู
- Views: `Views/Staff/Index.cshtml`

**B. Stock Control** - จัดการคลังสินค้า 📦
- ดูสินค้าที่มี
- อัปเดต จำนวนสต็อก
- เพิ่ม/แก้ไข สินค้า product variants (ไซส์, สี, ฯลฯ)
- พื้นที่ทำงาน: `Views/Staff/Stock.cshtml`
- เก็บ endpoints: GetProductDetail, UpdateProductInfo, UpdateVariantStock, AddVariant

**C. Staff Manager** - จัดการพนักงาน 👥
- ดูรายชื่อพนักงาน
- เพิ่ม/แก้ไข/ลบ พนักงาน
- ต้องยืนยัน password เมื่อแก้ไข
- Views: `Views/Staff/ManageUsers.cshtml` + `Views/Staff/ManageStaff.cshtml`

#### 4. **Other Controllers** (AdminController, ProductController, etc.)
ตอนนี้ยังเป็น placeholder แต่พร้อมสำหรับขยายในอนาคต ⏳

### 📦 ViewModels คืออะไร? (ตัวขนส่งข้อมูล)

ViewModels คือ class ที่ "บรรจุ" ข้อมูลจาก Controller ไปให้ View ใช้ เหมือนกล่องส่งของ 📮

| ไฟล์ ViewModel | ใช้ที่ไหน | ทำหน้าที่อะไร |
| --- | --- | --- |
| `LoginViewModel`, `RegisterViewModel`, `ForgetPasswordViewModel` | หน้า Login/Register | บรรจุข้อมูล form เช่น email, password |
| `Home/HomePageViewModel` | หน้า Home | ส่งข้อมูล สินค้าโดดเด่น, limited, และตามหมวดหมู่ |
| `Stock/StockModels` | หน้า Stock Control | ส่ง/รับข้อมูล เวลา update stock |
| `Staff/ManagerViewModels` | หน้า Staff Manager | ส่งข้อมูล รายการพนักงาน และ roles |
| `StaffDashboardViewModel` | Dashboard | ส่งข้อมูล menu/sections สำหรับพนักงาน |
| `CreateStaffViewModel` | สร้างพนักงานใหม่ | ส่ง email, password, role ฯลฯ |

### 🎨 Views & Assets (หน้าเว็บและไฟล์ CSS/JS)

**`_Layout.cshtml`** - เหมือนเป็น "กรอบ" ของทุกหน้า
- ไว้ navbar ด้านบน
- ไว้ shared CSS และ JavaScript
- อื่นๆ ใช้ `@RenderBody()` ให้เนื้อหาของหน้านั้นๆ แสดงตรงกลาง

**หน้า Views แต่ละหน้า** เก็บ CSS + JS ไว้ด้านใน
- ตัวอย่าง: `Views/Home/Index.cshtml` มี
  - `<style>` สำหรับ สไตล์ (เช่น `.home-hero`, `.product-card`)
  - `<script>` สำหรับ ฟังก์ชัน เช่น modal, AJAX calls

**wwwroot/ folder** - เก็บไฟล์ที่ public เช่น
- Bootstrap, jQuery (ไฟล์ vendor)
- ไฟล์ CSS/JS ร่วม

### 🗄️ ฐานข้อมูล - Tables ที่ใช้

| ฟีเจอร์ | Tables ที่ใช้ | อธิบาย |
| --- | --- | --- |
| **ล็อกอิน & สมัครสมาชิก** 🔐 | `users`, `roles` | เก็บข้อมูล user (email, password hash) และ roles (Admin, Staff, User) |
| **หน้า Home & แคตตาล็อก** 🏠 | `products`, `categories`, `product_variants` | เก็บสินค้า, หมวดหมู่, และ variants (ไซส์, สี) |
| **จัดการ Stock** 📦 | `products`, `product_variants`, `categories` | อัปเดต จำนวนสต็อก, ข้อมูล product |
| **จัดการ Staff/Users** 👥 | `users`, `roles` | พนักงาน/ผู้จัดการ สามารถ edit/delete users |

### 🔍 วิธีอ่านโค้ดอย่างไรให้เข้าใจ?

ถ้า คุณอยากเข้าใจ feature ใดๆ ให้ทำตามนี้:

**Step 1:** เปิด **Controller** ที่เกี่ยวข้อง
- ตัวอย่าง: อยากรู้เรื่อง "Stock Control" → เปิด `StaffController.cs`
- ดูว่า method ไหนต่อกับ feature นั้น
- ดูว่า return ค่าอะไร (ViewModel ไหน)

**Step 2:** เปิด **View** ที่ตรงกัน
- ตัวอย่าง: ดูว่า Stock Control View อยู่ที่ `Views/Staff/Stock.cshtml`
- ดู HTML structure
- ดู CSS ที่อยู่ใน `<style>` block
- ดู JavaScript ที่อยู่ใน `<script>` block

**Step 3:** เปิด **ViewModel** ที่ Controller ใช้
- ดูว่า model ของ feature นั้น ส่งข้อมูลอะไร
- ข้อมูลไหนบ้างที่ Controller ส่งให้ View

**Step 4:** ถ้าต้องรู้ database
- เปิด `Models/db/` ดู entity class
- เช่น `Models/db/Product.cs` ดูว่า table มี columns อะไรบ้าง

**📝 สรุป:**
```
Feature ที่อยากรู้
    ↓
ไปที่ Controller
    ↓
ไปที่ View (HTML + CSS + JS)
    ↓
ไปที่ ViewModel (ข้อมูล)
    ↓
ถ้าต้องรู้ database → ไปที่ Models/db/
```
