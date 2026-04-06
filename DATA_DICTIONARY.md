# Data Dictionary - ShoeStore Project

## ภาพรวมของระบบฐานข้อมูล
ระบบ ShoeStore เป็นแอปพลิเคชันประเภท E-commerce สำหรับจำหน่ายรองเท้า โดยมีฟังก์ชันหลักดังนี้:
- การจัดการสินค้า (Product Management)
- การจัดการหมวดหมู่ (Category Management)
- การจัดการตะกร้าสินค้า (Shopping Cart)
- การจัดการคำสั่งซื้อ (Order Management)
- ระบบการชำระเงิน (Payment Processing)
- ระบบส่งสินค้า (Shipment Tracking)
- ระบบการใช้คูปองส่วนลด (Coupon System)

---

## ตารางฐานข้อมูล (Database Tables)

### 1. Role (บทบาท)
**วัตถุประสงค์:** เก็บข้อมูลบทบาทของผู้ใช้ในระบบ

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของบทบาท |
| RoleName | NVARCHAR(MAX) | NOT NULL | ชื่อบทบาท (เช่น Admin, Staff, Customer) |

**ความสัมพันธ์:**
- Relationship: ONE-TO-MANY กับ User table

**ตัวอย่าง Record:**
```
Id = 1, RoleName = 'Admin'
Id = 2, RoleName = 'Customer'
Id = 3, RoleName = 'Staff'
```

---

### 2. User (ผู้ใช้)
**วัตถุประสงค์:** เก็บข้อมูลผู้ใช้ของระบบ (ลูกค้า และ Staff)

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของผู้ใช้ |
| RoleId | INT | FK → Role.Id, NOT NULL | บทบาทของผู้ใช้ |
| Fullname | NVARCHAR(MAX) | NOT NULL | ชื่อเต็มของผู้ใช้ |
| Email | NVARCHAR(MAX) | NOT NULL, UNIQUE | อีเมลของผู้ใช้ |
| PasswordHash | NVARCHAR(MAX) | NOT NULL | รหัสผ่านที่เข้ารหัส |
| Phone | NVARCHAR(MAX) | Allows NULL | เบอร์โทรศัพท์ |
| Address | NVARCHAR(MAX) | Allows NULL | ที่อยู่ |
| CreatedAt | DATETIME2 | Allows NULL | วันเวลาที่สร้างบัญชี |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ Role table (RoleId)
- Relationship: ONE-TO-MANY กับ CartItem table
- Relationship: ONE-TO-MANY กับ Order table

**ตัวอย่าง Record:**
```
Id = 1, RoleId = 2, Fullname = 'สมชาย ใจดี', Email = 'somchai@email.com', Phone = '0812345678'
```

---

### 3. Category (หมวดหมู่)
**วัตถุประสงค์:** เก็บข้อมูลหมวดหมู่ของสินค้า

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของหมวดหมู่ |
| CategoryName | NVARCHAR(MAX) | NOT NULL | ชื่อหมวดหมู่ (เช่น รองเท้าสปอร์ต, รองเท้าเดินทาง) |

**ความสัมพันธ์:**
- Relationship: ONE-TO-MANY กับ Product table

**ตัวอย่าง Record:**
```
Id = 1, CategoryName = 'รองเท้าสปอร์ต'
Id = 2, CategoryName = 'รองเท้าเดินทาง'
```

---

### 4. Product (สินค้า)
**วัตถุประสงค์:** เก็บข้อมูลสินค้าหลักในระบบ

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของสินค้า |
| CategoryId | INT | FK → Category.Id, NOT NULL | หมวดหมู่ของสินค้า |
| ProductName | NVARCHAR(MAX) | NOT NULL | ชื่อสินค้า |
| Description | NVARCHAR(MAX) | Allows NULL | คำอธิบายสินค้า |
| ImageUrl | NVARCHAR(MAX) | Allows NULL | URL ของรูปภาพสินค้า |
| Price | DECIMAL(18,2) | NOT NULL | ราคาสินค้า |
| DiscountPercent | DECIMAL(18,2) | Allows NULL | ลดราคาเป็นเปอร์เซ็นต์ (0-100) |
| IsLimited | BIT | Allows NULL | เป็นสินค้าที่มีจำกัดหรือไม่ |
| StockTotal | INT | Allows NULL | จำนวนสต็อกรวมทั้งหมด |
| CreatedAt | DATETIME2 | Allows NULL | วันเวลาที่สร้างข้อมูลสินค้า |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ Category table (CategoryId)
- Relationship: ONE-TO-MANY กับ ProductVariant table

**ตัวอย่าง Record:**
```
Id = 1, CategoryId = 1, ProductName = 'Nike Air Max Pro', Price = 2500.00, DiscountPercent = 10
```

---

### 5. ProductVariant (รูปแบบสินค้า)
**วัตถุประสงค์:** เก็บข้อมูลรูปแบบต่างๆ ของสินค้า (ไซส์, สี)

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของรูปแบบสินค้า |
| ProductId | INT | FK → Product.Id, NOT NULL | รหัสของสินค้า |
| Size | NVARCHAR(MAX) | Allows NULL | ไซส์รองเท้า (เช่น 40, 41, 42) |
| Color | NVARCHAR(MAX) | Allows NULL | สีของรองเท้า (เช่น Black, White, Red) |
| StockQuantity | INT | Allows NULL | จำนวนสต็อกของรูปแบบนี้ |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ Product table (ProductId)
- Relationship: ONE-TO-MANY กับ CartItem table
- Relationship: ONE-TO-MANY กับ OrderItem table

**ตัวอย่าง Record:**
```
Id = 1, ProductId = 1, Size = '42', Color = 'Black', StockQuantity = 15
Id = 2, ProductId = 1, Size = '42', Color = 'White', StockQuantity = 10
```

---

### 6. CartItem (รายการในตะกร้า)
**วัตถุประสงค์:** เก็บข้อมูลสินค้าที่ผู้ใช้เพิ่มลงในตะกร้าซื้อของตัวเอง

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของรายการตะกร้า |
| UserId | INT | FK → User.Id, NOT NULL | รหัสของผู้ใช้ |
| ProductVariantId | INT | FK → ProductVariant.Id, NOT NULL | รหัสของรูปแบบสินค้า |
| Quantity | INT | NOT NULL | จำนวนสินค้าที่เพิ่มในตะกร้า |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ User table (UserId)
- Relationship: MANY-TO-ONE กับ ProductVariant table (ProductVariantId)

**ตัวอย่าง Record:**
```
Id = 1, UserId = 1, ProductVariantId = 1, Quantity = 2
```

---

### 7. Coupon (คูปองส่วนลด)
**วัตถุประสงค์:** เก็บข้อมูลคูปองส่วนลดที่ใช้ได้ในระบบ

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของคูปอง |
| CouponCode | NVARCHAR(MAX) | NOT NULL, UNIQUE | รหัสคูปอง (เช่น SUMMER2024, NEW50) |
| DiscountPercent | DECIMAL(18,2) | Allows NULL | ลดราคาเป็นเปอร์เซ็นต์ |
| MinPurchase | DECIMAL(18,2) | Allows NULL | จำนวนซื้อขั้นต่ำเพื่อใช้คูปอง |
| StartDate | DATETIME2 | Allows NULL | วันเริ่มใช้งานคูปอง |
| EndDate | DATETIME2 | Allows NULL | วันสิ้นสุดการใช้งานคูปอง |

**ความสัมพันธ์:**
- Relationship: ONE-TO-MANY กับ Order table

**ตัวอย่าง Record:**
```
Id = 1, CouponCode = 'SUMMER2024', DiscountPercent = 15, MinPurchase = 1000.00, StartDate = '2024-05-01', EndDate = '2024-08-31'
```

---

### 8. Order (คำสั่งซื้อ)
**วัตถุประสงค์:** เก็บข้อมูลคำสั่งซื้อของผู้ใช้

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของคำสั่งซื้อ |
| UserId | INT | FK → User.Id, NOT NULL | รหัสของผู้ใช้ที่ทำการสั่งซื้อ |
| TotalAmount | DECIMAL(18,2) | Allows NULL | ราคารวมทั้งหมด (ก่อนลด) |
| DiscountAmount | DECIMAL(18,2) | Allows NULL | จำนวนส่วนลด |
| FinalAmount | DECIMAL(18,2) | Allows NULL | ราคาสุดท้าย (หลังลด) |
| CouponId | INT | FK → Coupon.Id, Allows NULL | รหัสคูปองที่ใช้ (ถ้ามี) |
| OrderStatus | NVARCHAR(MAX) | Allows NULL | สถานะของคำสั่งซื้อ (Pending, Processing, Shipped, Delivered, Cancelled) |
| CreatedAt | DATETIME2 | Allows NULL | วันเวลาที่สร้างคำสั่งซื้อ |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ User table (UserId)
- Relationship: MANY-TO-ONE กับ Coupon table (CouponId)
- Relationship: ONE-TO-MANY กับ OrderItem table
- Relationship: ONE-TO-MANY กับ Payment table
- Relationship: ONE-TO-MANY กับ Shipment table

**ตัวอย่าง Record:**
```
Id = 1, UserId = 1, TotalAmount = 5000.00, DiscountAmount = 750.00, FinalAmount = 4250.00, CouponId = 1, OrderStatus = 'Shipped'
```

---

### 9. OrderItem (รายการในคำสั่งซื้อ)
**วัตถุประสงค์:** เก็บข้อมูลรายการสินค้าที่อยู่ในแต่ละคำสั่งซื้อ

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของรายการคำสั่งซื้อ |
| OrderId | INT | FK → Order.Id, NOT NULL | รหัสของคำสั่งซื้อ |
| ProductVariantId | INT | FK → ProductVariant.Id, NOT NULL | รหัสของรูปแบบสินค้า |
| Price | DECIMAL(18,2) | NOT NULL | ราคาสินค้าในขณะที่สั่งซื้อ |
| Quantity | INT | NOT NULL | จำนวนสินค้า |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ Order table (OrderId)
- Relationship: MANY-TO-ONE กับ ProductVariant table (ProductVariantId)

**ตัวอย่าง Record:**
```
Id = 1, OrderId = 1, ProductVariantId = 1, Price = 2500.00, Quantity = 2
```

---

### 10. Payment (การชำระเงิน)
**วัตถุประสงค์:** เก็บข้อมูลการชำระเงินของแต่ละคำสั่งซื้อ

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของการชำระเงิน |
| OrderId | INT | FK → Order.Id, NOT NULL | รหัสของคำสั่งซื้อ |
| PaymentMethod | NVARCHAR(MAX) | Allows NULL | วิธีการชำระเงิน (Credit Card, Debit Card, Bank Transfer, Cash On Delivery) |
| PaymentStatus | NVARCHAR(MAX) | Allows NULL | สถานะของการชำระเงิน (Pending, Completed, Failed, Refunded) |
| PaidAt | DATETIME2 | Allows NULL | วันเวลาที่ชำระเงิน |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ Order table (OrderId)

**ตัวอย่าง Record:**
```
Id = 1, OrderId = 1, PaymentMethod = 'Credit Card', PaymentStatus = 'Completed', PaidAt = '2024-05-15 14:30:00'
```

---

### 11. Shipment (การจัดส่ง)
**วัตถุประสงค์:** เก็บข้อมูลการจัดส่งสินค้าของแต่ละคำสั่งซื้อ

| Column Name | Data Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, Auto-increment | รหัสประจำตัวของการจัดส่ง |
| OrderId | INT | FK → Order.Id, NOT NULL | รหัสของคำสั่งซื้อ |
| TrackingNumber | NVARCHAR(MAX) | Allows NULL | เลขติดตามพัสดุ |
| ShippingStatus | NVARCHAR(MAX) | Allows NULL | สถานะการจัดส่ง (Prepared, Shipped, In Transit, Delivered) |

**ความสัมพันธ์:**
- Relationship: MANY-TO-ONE กับ Order table (OrderId)

**ตัวอย่าง Record:**
```
Id = 1, OrderId = 1, TrackingNumber = 'TH123456789', ShippingStatus = 'In Transit'
```

---

## แผนภาพความสัมพันธ์ (Entity Relationship Diagram)

```
┌─────────┐
│  Role   │
└────┬────┘
     │ (1:N)
     │
┌────▼──────┐          ┌──────────┐
│   User    │◄────────►│ CartItem │
└────┬──────┘          └──────┬───┘
     │ (1:N)                  │ (N:1)
     │                        │
     │              ┌─────────▼────────┐
     │              │ ProductVariant   │
     │              └────────┬─────────┘
     │                       │ (N:1)
     │              ┌────────▼────────┐
     │              │    Product      │
     │              └────────┬────────┘
     │                       │ (N:1)
     │              ┌────────▼──────┐
     │              │   Category    │
     │              └───────────────┘
     │
     │
┌────▼──────┐       ┌──────────┐
│   Order   │──────►│ OrderItem│
└────┬──────┘       └──────┬───┘
     │                     │ (N:1)
     │          ┌──────────▼────────┐
     │          │ ProductVariant    │
     │          └───────────────────┘
     │
     ├─────────►│ Coupon│ (0:1)
     │
     ├─────────►│ Payment│ (1:N)
     │
     └─────────►│ Shipment│ (1:N)
```

---

## Constraints และ Business Rules

### Primary Key Constraints
- ทุก Table ต้องมี Id คอลัมน์เป็น Primary Key ที่ Auto-increment

### Foreign Key Constraints
- `User.RoleId` → `Role.Id`
- `Product.CategoryId` → `Category.Id`
- `ProductVariant.ProductId` → `Product.Id`
- `CartItem.UserId` → `User.Id`
- `CartItem.ProductVariantId` → `ProductVariant.Id`
- `Order.UserId` → `User.Id`
- `Order.CouponId` → `Coupon.Id` (Allows NULL)
- `OrderItem.OrderId` → `Order.Id`
- `OrderItem.ProductVariantId` → `ProductVariant.Id`
- `Payment.OrderId` → `Order.Id`
- `Shipment.OrderId` → `Order.Id`

### Unique Constraints
- `User.Email` - ต้องไม่ซ้ำ
- `Coupon.CouponCode` - ต้องไม่ซ้ำ
- `Role.RoleName` - ควรไม่ซ้ำ
- `Category.CategoryName` - ควรไม่ซ้ำ

### NOT NULL Constraints
ดูรายละเอียดในแต่ละตารางข้างต้น

### Data Validation Rules
1. **Price Fields:** ต้องมีค่ามากกว่าหรือเท่ากับ 0
2. **DiscountPercent:** ต้องอยู่ระหว่าง 0-100
3. **Quantity Fields:** ต้องมีค่ามากกว่าหรือเท่ากับ 0
4. **Email:** ต้องเป็นรูปแบบอีเมลที่ถูกต้อง
5. **OrderStatus:** ต้องเป็นค่าที่กำหนดไว้เท่านั้น (Pending, Processing, Shipped, Delivered, Cancelled)
6. **PaymentStatus:** ต้องเป็นค่าที่กำหนดไว้เท่านั้น (Pending, Completed, Failed, Refunded)
7. **ShippingStatus:** ต้องเป็นค่าที่กำหนดไว้เท่านั้น (Prepared, Shipped, In Transit, Delivered)
8. **PaymentMethod:** ต้องเป็นค่าที่กำหนดไว้เท่านั้น

---

## สถานะและค่าที่เป็นไปได้

| Field | Possible Values | Description |
|---|---|---|
| **RoleName** | Admin, Staff, Customer | บทบาทของผู้ใช้ |
| **OrderStatus** | Pending, Processing, Shipped, Delivered, Cancelled | สถานะของคำสั่งซื้อ |
| **PaymentStatus** | Pending, Completed, Failed, Refunded | สถานะของการชำระเงิน |
| **ShippingStatus** | Prepared, Shipped, In Transit, Delivered | สถานะของการจัดส่ง |
| **PaymentMethod** | Credit Card, Debit Card, Bank Transfer, Cash On Delivery | วิธีการชำระเงิน |

---

## Database Design Considerations

### Normalization
- ฐานข้อมูลนี้ได้รับการออกแบบให้เป็นไปตาม 3NF (Third Normal Form)
- ลดการซ้ำซ้อนของข้อมูล
- ทำให้ง่ายต่อการ maintain

### Performance Considerations
- ProductVariant table ถูกสร้างแยกเพื่อจัดการขนาด สี ที่แตกต่างกัน
- CartItem และ OrderItem ถูกแยกเพื่อให้การสืบค้นเร็วขึ้น
- พิจารณาการสร้าง Indexes บน FK columns เพื่อปรับปรุงประสิทธิภาพ

### Future Considerations
- อาจต้องเพิ่ม Timestamp columns (UpdatedAt, DeletedAt) สำหรับการ Audit
- พิจารณาการ Soft Delete สำหรับ Data Archival
- การจัดการ Inventory ที่ซับซ้อนมากขึ้น

---

## Document Information
- **Document Version:** 1.0
- **Last Updated:** April 6, 2026
- **Project Name:** ShoeStore
- **Database Type:** SQL Server / Entity Framework Core .NET
- **Language:** Thai/English

