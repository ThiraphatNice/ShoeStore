# Present Final Guide

เอกสารนี้ใช้เป็นคู่มือพรีเซนต์ระบบ โดยจัดตามหน้าใช้งานจริงและตาม role ที่เกี่ยวข้อง เน้นอธิบายโค้ดว่าแต่ละหน้าพึ่งพา `View`, `ViewModel`, `Controller` และ logic ส่วนไหนบ้าง รวมถึง flow การเชื่อมกันของแต่ละส่วน

## 1. Login

- **หน้าที่ของหน้า**: ใช้ตรวจสอบบัญชีผู้ใช้จากอีเมลและรหัสผ่าน แล้วสร้าง cookie/claims เพื่อให้ระบบรู้ว่า user เป็น `Users`, `Staff`, หรือ `Admin`
- **View ที่ใช้**
  - `Views/Account/Login.cshtml:1-118` กำหนด model และ CSS ของหน้า login
  - `Views/Account/Login.cshtml:121-155` สร้าง UI จริงของฟอร์ม login, พื้นที่แสดง `ViewBag.Error` และ `ViewBag.Success`, และลิงก์ไปหน้า Register / Forget Password
- **ViewModel ที่ใช้**
  - `ViewModels/LoginViewModel.cs:5-16` มี 2 field หลักคือ `Email` และ `Password` พร้อม DataAnnotations สำหรับ validate input
- **Controller ที่ใช้**
  - `Controllers/AccountController.cs:22-25` `Login()` แบบ GET ใช้เปิดหน้า login
  - `Controllers/AccountController.cs:28-55` `Login(LoginViewModel model)` แบบ POST ตรวจ `ModelState`, query user จาก `users`, include `roles`, เช็กรหัสผ่าน, สร้าง claims และ `SignInAsync`
  - `Controllers/AccountController.cs:200-208` `PasswordMatches` ใช้เช็กรหัสผ่านที่กรอกกับค่าที่เก็บในฐานข้อมูล
  - `Controllers/AccountController.cs:210-231` `BuildClaims` สร้าง role claims เพิ่มเติม เช่น `Admin` และ `Staff`
- **หลักการทำงานและการเชื่อมกัน**
  - View ส่งฟอร์มไปที่ `AccountController.Login (POST)`
  - Controller bind ข้อมูลเข้า `LoginViewModel`
  - ถ้าข้อมูลถูกต้อง Controller จะอ่าน `users.role_id` และ `roles.role_name` เพื่อสร้าง claim
  - หลัง login สำเร็จ ระบบ redirect ไป `HomeController.Index`

## 2. Register

- **หน้าที่ของหน้า**: ใช้สมัครสมาชิกใหม่ โดยสร้างบัญชีใหม่ในตาราง `users` และผูก role ลูกค้าปกติให้โดยอัตโนมัติ
- **View ที่ใช้**
  - `Views/Account/Register.cshtml:1-118` กำหนด model และ style ของหน้าสมัครสมาชิก
  - `Views/Account/Register.cshtml:121-156` สร้างฟอร์มกรอกชื่อ อีเมล รหัสผ่าน และ confirm password
- **ViewModel ที่ใช้**
  - `ViewModels/RegisterViewModel.cs:5-26` มี `FullName`, `Email`, `Password`, `ConfirmPassword` และ `[Compare]` เพื่อบังคับให้รหัสผ่านตรงกัน
- **Controller ที่ใช้**
  - `Controllers/AccountController.cs:57-60` `Register()` แบบ GET เปิดหน้าสมัคร
  - `Controllers/AccountController.cs:63-100` `Register(RegisterViewModel model)` แบบ POST ตรวจข้อมูล, เช็กอีเมลซ้ำ, หา role เริ่มต้น, สร้าง `User`, บันทึกลงฐานข้อมูล แล้วส่งกลับหน้า login
- **หลักการทำงานและการเชื่อมกัน**
  - View ส่งข้อมูลเข้า `RegisterViewModel`
  - Controller เช็กว่ามีอีเมลซ้ำใน `users` หรือไม่
  - Controller หา role ลูกค้าจาก `roles`
  - ถ้าผ่านทั้งหมดจะ insert ลง `users` แล้ว render หน้า login พร้อมข้อความสำเร็จ

## 3. ForgetPassword

- **หน้าที่ของหน้า**: ให้ผู้ใช้เปลี่ยนรหัสผ่านใหม่ด้วยอีเมลที่มีอยู่ในระบบ
- **View ที่ใช้**
  - `Views/Account/ForgetPassword.cshtml:1-118` กำหนด model และ style
  - `Views/Account/ForgetPassword.cshtml:121-151` สร้างฟอร์ม `Email`, `NewPassword`, `ConfirmPassword`
- **ViewModel ที่ใช้**
  - `ViewModels/ForgetPasswordViewModel.cs:5-22` ใช้รับอีเมลและรหัสผ่านใหม่ พร้อม `[Compare]`
- **Controller ที่ใช้**
  - `Controllers/AccountController.cs:102-105` `ForgetPassword()` แบบ GET เปิดหน้า
  - `Controllers/AccountController.cs:108-127` `ForgetPassword(ForgetPasswordViewModel model)` แบบ POST ค้นหา user ตามอีเมล แล้วอัปเดต `PasswordHash`
- **หลักการทำงานและการเชื่อมกัน**
  - View ส่งฟอร์มไป action POST
  - Controller bind เข้า `ForgetPasswordViewModel`
  - ถ้าไม่พบอีเมลจะเพิ่ม error ให้ `ModelState`
  - ถ้าพบจะอัปเดตรหัสผ่านใน `users.password_hash` แล้วส่งกลับหน้า login

## 4. Home

- **หน้าที่ของหน้า**: เป็นหน้า dashboard หลักหลัง login สำหรับโชว์สินค้าเด่น, limited edition และสินค้าแยกตามหมวด
- **View ที่ใช้**
  - `Views/Home/Index.cshtml:1-16` ประกาศ model และเริ่ม style section
  - `Views/Home/Index.cshtml:16-57` CSS ของหน้า home
  - `Views/Home/Index.cshtml:58-67` ประกาศ `window.cartApi` และ `window.cartRestrictions` ให้ปุ่ม add-to-cart ใช้ต่อได้
  - ส่วน layout หลักของหน้าจริงอยู่ใน `Views/Home/Index.cshtml` ถัดจาก style/scripts โดยใช้ model ที่ controller ส่งมาเพื่อ render product sections
- **ViewModel ที่ใช้**
  - `ViewModels/Home/HomePageViewModel.cs:6-12` เก็บข้อมูลรวมของหน้า home
  - `ViewModels/Home/HomePageViewModel.cs:14-18` `HomeSectionViewModel` ใช้แทน section ตามหมวดสินค้า
  - `ViewModels/Home/HomePageViewModel.cs:20-46` `ProductCardViewModel` เป็นรูปแบบข้อมูลที่แต่ละการ์ดสินค้าใช้ render
  - `ViewModels/Home/HomePageViewModel.cs:48-55` `ProductVariantSummaryViewModel` ใช้เก็บสี/ไซซ์/stock ของสินค้า
- **Controller/Service ที่ใช้**
  - `Controllers/HomeController.cs:23-85` `Index()` โหลดสินค้าเด่น, limited, และ category sections แล้วประกอบเป็น `HomePageViewModel`
  - `Services/ProductDisplayMapper.cs:27-57` แปลง `Product` จากฐานข้อมูลให้เป็น `ProductCardViewModel`
  - `Controllers/HomeController.cs:87-95` มี `Privacy()` และ `Error()` เป็นหน้าเสริมของ home area
- **หลักการทำงานและการเชื่อมกัน**
  - `HomeController.Index` query `products`, `categories`, `product_variants`
  - ข้อมูลแต่ละสินค้าถูกแปลงโดย `ProductDisplayMapper`
  - View ใช้ model เดียวกัน render หลาย section
  - `window.cartApi` ถูกส่งให้ปุ่มซื้อเชื่อมต่อกับ `CartController.AddItem`

## 5. Search

- **หน้าที่ของหน้า**: ใช้ค้นหาสินค้าด้วยชื่อ หมวด หรือรหัสสินค้า และถ้าไม่พบจะแสดงสินค้าที่แนะนำ
- **View ที่ใช้**
  - `Views/Product/Index.cshtml:17-103` CSS และโครงสร้าง visual ของหน้าค้นหา
  - `Views/Product/Index.cshtml:104-149` ฟอร์มค้นหา, พื้นที่แสดงผลลัพธ์, และ suggestion state
  - `Views/Product/Index.cshtml:150-159` สร้าง `window.cartApi` และ `window.cartRestrictions`
  - `Views/Product/Details.cshtml:1-9` หน้า details ปัจจุบันยังเป็น placeholder
- **ViewModel ที่ใช้**
  - `ViewModels/Product/ProductSearchViewModel.cs:6-14` เก็บ query, result list, suggestion list และ flag นับผลลัพธ์
  - `ViewModels/Home/HomePageViewModel.cs:20-46` ใช้ `ProductCardViewModel` ซ้ำสำหรับแสดงสินค้าในผลค้นหา
- **Controller/Service ที่ใช้**
  - `Controllers/ProductController.cs:25-90` `Index(string? q)` รับ query string, ประมวลผลการค้นหา, และเติม suggestion
  - `Controllers/ProductController.cs:93-95` `Details(int id)` ยังเป็น placeholder
  - `Services/ProductDisplayMapper.cs:27-57` ใช้ map สินค้าเป็น card model
- **หลักการทำงานและการเชื่อมกัน**
  - หน้า search ส่ง query string เข้า `ProductController.Index`
  - Controller query `products` พร้อม `category` และ `product_variants`
  - ถ้าค้นไม่เจอจะใช้ suggestion แทน
  - View render ผลลัพธ์ด้วย card model ชุดเดียวกับหน้า Home

## 6. Cart

- **หน้าที่ของหน้า**: ใช้จัดการตะกร้าสินค้า ปรับจำนวน ลบสินค้า ตรวจโปรไฟล์ และเข้าสู่ขั้นตอน checkout
- **View ที่ใช้**
  - `Views/Cart/Index.cshtml:7-355` CSS ทั้งหน้าตะกร้าและ modal
  - `Views/Cart/Index.cshtml:356-442` รายการสินค้าในตะกร้าและกล่องสรุปยอด
  - `Views/Cart/Index.cshtml:443-465` modal ยืนยันลบและแจ้งเตือน stock
  - `Views/Cart/Index.cshtml:466-586` modal checkout เช่น coupon, payment method, summary
  - `Views/Cart/Index.cshtml:587-604` modal แสดงสถานะชำระเงิน
  - `Views/Cart/Index.cshtml:605-612` empty state เมื่อไม่มีสินค้า
  - `Views/Cart/Index.cshtml:613-832` script ฝั่งหน้าเว็บสำหรับเรียก API ตะกร้าและ checkout
- **ViewModel ที่ใช้**
  - `ViewModels/Cart/CartViewModels.cs:9-24` `CartPageViewModel` เป็น model หลักของหน้า
  - `ViewModels/Cart/CartViewModels.cs:26-65` `CartItemViewModel` ใช้ render แต่ละรายการ
  - `ViewModels/Cart/CartViewModels.cs:67-103` `CartTotalsViewModel` ใช้สรุป subtotal, pair discount, coupon discount, shipping, final amount
  - `ViewModels/Cart/CartViewModels.cs:105-127` request model สำหรับ add/update/remove
  - `ViewModels/Cart/CheckoutViewModels.cs:6-113` ใช้ตอน validate coupon และ submit payment
- **Controller/Service ที่ใช้**
  - `Controllers/CartController.cs:25-51` `Index()` โหลด cart items แล้วสร้างหน้า cart
  - `Controllers/CartController.cs:54-65` `CheckProfileStatus()` เช็กว่าข้อมูลโปรไฟล์ครบก่อน checkout หรือไม่
  - `Controllers/CartController.cs:69-144` `AddItem()` เพิ่มสินค้าลงตะกร้า
  - `Controllers/CartController.cs:148-230` `UpdateItem()` ปรับจำนวนสินค้า
  - `Controllers/CartController.cs:234-276` `RemoveItem()` ลบสินค้าและคืน stock
  - `Controllers/CartController.cs:279-420` helper สำหรับ map item, คำนวณ totals, ตรวจ role, อ่าน current user
  - `Services/CartPricingCalculator.cs:7-76` รวมกติกาคิด pair discount 10% และค่าส่ง 300 บาทถ้ายอดสุทธิต่ำกว่า 3000
  - `Controllers/CheckoutController.cs:26-116` รับช่วง validate coupon และจ่ายเงินจริงจาก modal checkout
  - `Services/CheckoutService.cs:19-266` เป็น business logic หลักของ checkout
- **หลักการทำงานและการเชื่อมกัน**
  - View โหลด `CartPageViewModel` จาก `CartController.Index`
  - JavaScript ในหน้าเรียก `AddItem`, `UpdateItem`, `RemoveItem` แบบ AJAX
  - ก่อน checkout หน้า cart จะเช็ก `CheckProfileStatus`
  - ตอนคิดราคารวมจะใช้ `CartPricingCalculator`
  - ตอนกดยืนยันจ่ายจะส่งต่อไป `CheckoutController.SubmitPayment`

## 7. History (Orders)

- **หน้าที่ของหน้า**: ใช้ดูประวัติการสั่งซื้อทั้งหมดของ user และดูรายละเอียด order ทีละรายการ
- **View ที่ใช้**
  - `Views/Order/Index.cshtml:7-137` CSS ของหน้า order history และ modal detail
  - `Views/Order/Index.cshtml:138-200` แสดงรายการ order summary
  - `Views/Order/Index.cshtml:201-242` modal แสดงรายละเอียด order แบบ AJAX
  - `Views/Order/Index.cshtml:243-249` ผูก endpoint `DetailsData`
  - `Views/Order/Details.cshtml:12-64` CSS ของหน้า detail แบบเต็ม
  - `Views/Order/Details.cshtml:65-145` แสดง timeline, ข้อมูลลูกค้า, สินค้า และยอดรวม
- **ViewModel ที่ใช้**
  - `ViewModels/Order/OrderHistoryViewModels.cs:8-17` `OrderHistoryPageViewModel` เป็น model ของหน้ารายการ
  - `ViewModels/Order/OrderHistoryViewModels.cs:19-50` `OrderSummaryViewModel` ใช้สรุปแต่ละ order
  - `ViewModels/Order/OrderHistoryViewModels.cs:52-69` `OrderProductSummaryViewModel` ใช้สรุปรายการสินค้า
  - `ViewModels/Order/OrderHistoryViewModels.cs:71-82` `OrderDetailViewModel` ขยายจาก summary เพื่อใส่ข้อมูลลูกค้าและ timeline
  - `ViewModels/Order/OrderHistoryViewModels.cs:84-97` `ShipmentTimelineItem` ใช้สร้าง progress timeline
- **Controller ที่ใช้**
  - `Controllers/OrderController.cs:31-47` `Index()` โหลด order ทั้งหมดของ user
  - `Controllers/OrderController.cs:49-65` `Details(int id)` เปิดหน้ารายละเอียด
  - `Controllers/OrderController.cs:67-83` `DetailsData(int id)` ส่ง JSON ให้ modal ในหน้า history
  - `Controllers/OrderController.cs:85-205` helper สำหรับ query order, map summary, แก้ shipping status และสร้าง timeline
- **หลักการทำงานและการเชื่อมกัน**
  - `OrderController.Index` query `orders`, `order_items`, `payments`, `coupons`, `shipments`
  - View `Order/Index` แสดง summary ก่อน
  - ถ้ากดดูรายละเอียด จะใช้ endpoint `DetailsData` หรือเปิดหน้า `Order/Details`
  - timeline ใช้ shipping status และวันที่สร้าง order มาสร้างลำดับขั้นของการจัดส่ง

## 8. Profile

- **หน้าที่ของหน้า**: ใช้แสดงข้อมูลบัญชีปัจจุบันและให้ user แก้ไข profile ของตัวเอง
- **View ที่ใช้**
  - `Views/Account/Profile.cshtml:1-185` CSS และโครงสร้างการ์ด profile
  - `Views/Account/Profile.cshtml:186-225` ส่วนแสดงข้อมูล profile และปุ่ม edit/logout
  - `Views/Account/Profile.cshtml:226-271` modal แก้ไขข้อมูล profile
  - `Views/Account/Profile.cshtml:272-297` script เปิด/ปิด modal และคุม UI หลัง validation error
- **ViewModel ที่ใช้**
  - `ViewModels/Account/ProfileViewModels.cs:5-13` `ProfilePageViewModel` เก็บข้อมูล profile ที่โชว์หน้า view
  - `ViewModels/Account/ProfileViewModels.cs:15-44` `ProfileEditViewModel` ใช้ validate ตอนแก้ไข
- **Controller ที่ใช้**
  - `Controllers/AccountController.cs:139-150` `Profile()` โหลด user ปัจจุบันและสร้าง page model
  - `Controllers/AccountController.cs:153-192` `UpdateProfile()` รับข้อมูลจาก modal แล้วอัปเดต user
  - `Controllers/AccountController.cs:233-275` helper สำหรับอ่าน current user, ประกอบ model และ refresh sign-in cookie
  - `Controllers/AccountController.cs:130-136` `Logout()` ใช้กับปุ่ม logout บนหน้า profile
- **หลักการทำงานและการเชื่อมกัน**
  - หน้า profile โหลดผ่าน `AccountController.Profile`
  - modal แก้ไขส่งข้อมูลกลับ `UpdateProfile`
  - ถ้ามี validation error controller จะตั้ง `ViewData["OpenEditModal"]` ให้ modal เปิดค้าง
  - ถ้าแก้ไขสำเร็จ controller refresh cookie เพื่อให้ claims และชื่อผู้ใช้บนระบบตรงกับข้อมูลใหม่

## 9. Staff Dashboard

- **หน้าที่ของหน้า**: เป็นจุดรวมเมนูของ staff โดยเลือก section ที่เข้าได้ตาม role ของผู้ใช้ และถ้าเป็น admin จะเห็นทุก section
- **View ที่ใช้**
  - `Views/Staff/Index.cshtml:6-77` CSS ของ dashboard
  - `Views/Staff/Index.cshtml:78-122` card กลาง, dropdown/select และรายการเมนู section
  - `Views/Staff/Index.cshtml:123-144` script สำหรับเปลี่ยนหน้าเมื่อเลือก section
- **ViewModel ที่ใช้**
  - `ViewModels/StaffDashboardViewModel.cs:5-17` มี `IsAdmin` และ list ของ `StaffSectionOption`
- **Controller/Service ที่ใช้**
  - `Controllers/StaffController.cs:30-34` `Index()` ใช้เปิด staff dashboard
  - `Controllers/AdminController.cs:10-13` `Index()` ของ admin ก็ใช้ view เดียวกัน
  - `Services/StaffNavigationService.cs:11-40` กำหนด section ที่มีในระบบ
  - `Services/StaffNavigationService.cs:55-81` `BuildDashboard` และ `GetSectionsFor` ใช้ role claims ตัดสินว่าเห็นเมนูไหน
  - `Services/StaffNavigationService.cs:83-102` helper จัดการ alias ของ role name
- **หลักการทำงานและการเชื่อมกัน**
  - Controller ไม่ได้สร้างเมนูเอง แต่เรียก `StaffNavigationService.BuildDashboard(User)`
  - Service ดู `ClaimTypes.Role` ของ user แล้วเลือก menu ที่สัมพันธ์กับ role
  - View แสดง section เป็นลิงก์/selector ไปยัง Stock, Manager, Sales, Express

## 10. Stock Staff

- **หน้าที่ของหน้า**: ใช้จัดการข้อมูลสินค้าและ stock จริง เช่น ดูรายละเอียดสินค้า แก้ข้อมูลสินค้า เพิ่ม variant ปรับ stock และเพิ่มสินค้าใหม่
- **View ที่ใช้**
  - `Views/Staff/Stock.cshtml:6-157` CSS ของหน้าคลังสินค้า
  - `Views/Staff/Stock.cshtml:158-192` hero และ quick actions
  - `Views/Staff/Stock.cshtml:193-298` modal รายละเอียดสินค้าและแก้ไข variant/stock
  - `Views/Staff/Stock.cshtml:299-322` modal ตาราง inventory ทั้งหมด
  - `Views/Staff/Stock.cshtml:323-377` modal เพิ่มสินค้าใหม่
  - `Views/Staff/Stock.cshtml:378-385` alert modal
  - `Views/Staff/Stock.cshtml:386-800` script ที่เรียก API แก้ข้อมูลสินค้าแบบ AJAX
- **ViewModel ที่ใช้**
  - `ViewModels/Stock/StockModels.cs:7-10` `StockPageViewModel` ใช้ส่งหมวดหมู่ไปหน้า view
  - `ViewModels/Stock/StockModels.cs:12-32` `UpdateProductRequest`
  - `ViewModels/Stock/StockModels.cs:34-47` `UpdateVariantStockRequest`
  - `ViewModels/Stock/StockModels.cs:49-60` `AddVariantRequest`
  - `ViewModels/Stock/StockModels.cs:62-80` `CreateProductRequest`
  - `ViewModels/Stock/StockModels.cs:82-103` `ProductDetailResponse` และ `ProductVariantViewModel`
  - `ViewModels/Stock/StockModels.cs:104-113` `InventoryRowViewModel`
- **Controller ที่ใช้**
  - `Controllers/StaffController.cs:36-58` `Stock()` โหลดหมวดหมู่มาใส่ dropdown
  - `Controllers/StaffController.cs:362-398` `GetProductDetail()` คืน JSON รายละเอียดสินค้า
  - `Controllers/StaffController.cs:403-432` `UpdateProductInfo()` อัปเดตข้อมูลหลักของสินค้า
  - `Controllers/StaffController.cs:437-459` `UpdateVariantStock()` อัปเดต stock ของ variant
  - `Controllers/StaffController.cs:464-505` `AddVariant()` เพิ่ม option size/color ใหม่
  - `Controllers/StaffController.cs:509-529` `ListInventory()` คืนตาราง inventory
  - `Controllers/StaffController.cs:565-595` `CreateProduct()` เพิ่มสินค้าใหม่
  - `Controllers/StaffController.cs:702-724` helper รีเฟรช `stock_total` และตรวจหมวด limited
- **หลักการทำงานและการเชื่อมกัน**
  - หน้า stock โหลดแค่หมวดหมู่ก่อน
  - รายละเอียดสินค้าจริงถูกดึงแบบ AJAX ผ่าน `GetProductDetail`
  - เมื่อแก้ข้อมูลหรือ stock หน้า view เรียก endpoint JSON แล้ว refresh ตาราง
  - controller จะคำนวณ `products.stock_total` ใหม่จาก `product_variants.stock_quantity`

## 11. Manager Staff

- **หน้าที่ของหน้า**: ใช้ให้ staff manager จัดการบัญชีผู้ใช้/พนักงาน เช่น ดูรายชื่อ แก้ role แก้ข้อมูล และลบบัญชี
- **View ที่ใช้**
  - `Views/Staff/ManageUsers.cshtml:10-115` CSS ของหน้า manager
  - `Views/Staff/ManageUsers.cshtml:116-140` hero และปุ่ม action หลัก
  - `Views/Staff/ManageUsers.cshtml:141-166` modal ตารางรายชื่อผู้ใช้
  - `Views/Staff/ManageUsers.cshtml:167-210` modal แก้ไขข้อมูลผู้ใช้
  - `Views/Staff/ManageUsers.cshtml:211-227` modal ลบผู้ใช้
  - `Views/Staff/ManageUsers.cshtml:228-235` modal แจ้งเตือน
  - `Views/Staff/ManageUsers.cshtml:236-465` script เรียก API list/update/delete
  - `Views/Staff/ManageStaff.cshtml:6-63` ฟอร์มเพิ่ม staff ใหม่
  - `Views/Staff/ManageStaff.cshtml:64-88` ตาราง staff เดิม
- **ViewModel ที่ใช้**
  - `ViewModels/Staff/ManagerViewModels.cs:7-10` `StaffManagerPageViewModel` ใช้ส่ง role options ไปหน้า manage users
  - `ViewModels/Staff/ManagerViewModels.cs:12-22` `ManagedUserRowViewModel`
  - `ViewModels/Staff/ManagerViewModels.cs:24-43` `UpdateManagedUserRequest`
  - `ViewModels/Staff/ManagerViewModels.cs:45-52` `DeleteManagedUserRequest`
  - `ViewModels/StaffManagementViewModel.cs:7-24` ใช้กับหน้า `ManageStaff`
  - `ViewModels/CreateStaffViewModel.cs:5-30` ใช้รับข้อมูลตอนเพิ่ม staff ใหม่
- **Controller ที่ใช้**
  - `Controllers/StaffController.cs:71-96` `ManageUsers()` เตรียม role options สำหรับ staff manager
  - `Controllers/StaffController.cs:274-301` `ManageStaff()` เปิดหน้าสร้าง/ดู staff
  - `Controllers/StaffController.cs:305-340` `CreateStaff()` เพิ่มพนักงานใหม่
  - `Controllers/StaffController.cs:533-560` `ListManagedUsers()` ดึงรายชื่อผู้ใช้
  - `Controllers/StaffController.cs:600-646` `UpdateManagedUser()` แก้ role และข้อมูลผู้ใช้
  - `Controllers/StaffController.cs:651-687` `DeleteManagedUser()` ลบผู้ใช้หลังยืนยันรหัสผ่าน manager
  - `Controllers/StaffController.cs:342-359` `CanAccessSection()` ใช้คุมสิทธิ์ของ role manager
  - `Controllers/StaffController.cs:689-699` `GetRoleOptionsAsync()` ใช้ดึง role ที่อนุญาตให้เลือก
- **หลักการทำงานและการเชื่อมกัน**
  - หน้า `ManageUsers` เป็น UI แบบ modal + AJAX
  - หน้า `ManageStaff` เป็น form submit ปกติสำหรับสร้าง staff
  - controller จะไม่ให้แก้ไขหรือลบ `Admin`
  - ตอนลบ user ต้องตรวจ `manager.PasswordHash` เทียบกับรหัสที่ผู้จัดการกรอก

## 12. Sell (Sales) Staff

- **หน้าที่ของหน้า**: ใช้ดูยอดขาย สรุปรายเดือน/รายปี ดูสินค้ายอดนิยม และจัดการคูปองโปรโมชั่น
- **View ที่ใช้**
  - `Views/Staff/Sales.cshtml:32-194` CSS ของหน้า sales
  - `Views/Staff/Sales.cshtml:195-238` hero และ filter bar สำหรับเลือกรายเดือน/รายปี
  - `Views/Staff/Sales.cshtml:239-259` summary cards
  - `Views/Staff/Sales.cshtml:260-325` ตารางสรุปยอดขายและตารางสินค้ายอดนิยม
  - `Views/Staff/Sales.cshtml:326-354` ตารางจัดการคูปอง
  - `Views/Staff/Sales.cshtml:355-402` modal เพิ่ม/แก้/ลบคูปอง
  - `Views/Staff/Sales.cshtml:404-902` script ฝั่ง client สำหรับเรียก API coupon, summary และ top products
- **ViewModel ที่ใช้**
  - `ViewModels/Staff/SalesViewModels.cs:8-15` `SalesDashboardViewModel` ใช้กำหนด dropdown เดือน/ปีและค่า default
  - `ViewModels/Staff/SalesViewModels.cs:17-28` `CouponRowViewModel`
  - `ViewModels/Staff/SalesViewModels.cs:30-45` `CouponUpsertRequest`
  - `ViewModels/Staff/SalesViewModels.cs:47-50` `CouponDeleteRequest`
  - `ViewModels/Staff/SalesViewModels.cs:53-68` `SalesSummaryScope` และ `SalesSummaryQuery`
  - `ViewModels/Staff/SalesViewModels.cs:70-94` model ของผลสรุปยอดขาย
  - `ViewModels/Staff/SalesViewModels.cs:96-106` `TopProductRowViewModel`
- **Controller/Service ที่ใช้**
  - `Controllers/StaffController.cs:98-137` `Sales()` เปิดหน้า sales พร้อมค่า default ของเดือน/ปี
  - `Controllers/StaffController.cs:151-235` ชุด action จัดการคูปอง `ListCoupons`, `CreateCoupon`, `UpdateCoupon`, `DeleteCoupon`
  - `Controllers/StaffController.cs:238-271` `SalesSummary()` และ `TopProducts()`
  - `Services/StaffSalesService.cs:23-85` จัดการ CRUD คูปอง
  - `Services/StaffSalesService.cs:105-197` สรุปยอดขายและสินค้ายอดนิยมจาก `orders` และ `order_items`
  - `Services/StaffSalesService.cs:221-313` helper สำหรับแปลง coupon, validate ช่วงเวลา, normalize และกำหนดช่วงวันที่รายงาน
  - `Services/StaffSalesService.cs:315-371` สร้าง series แบบรายวัน/รายเดือนเพื่อใช้เป็นข้อมูลสรุป
- **หลักการทำงานและการเชื่อมกัน**
  - หน้า sales เปิดมาพร้อม config เดือน/ปีจาก `SalesDashboardViewModel`
  - JavaScript ใน view เรียก `SalesSummary` และ `TopProducts` ตาม filter ที่เลือก
  - CRUD คูปองทั้งหมดวิ่งผ่าน `StaffSalesService`
  - ข้อมูลวัดยอดนิยมใช้ `quantity` จาก `order_items` เป็นหลัก ไม่ได้จัดอันดับด้วยราคา

## 13. Express Staff

- **หน้าที่ของหน้า**: ใช้จัดการสถานะการจัดส่งของ orders และดูภาพรวมงานจัดส่งทั้งหมด
- **View ที่ใช้**
  - `Views/Staff/Express.cshtml:15-125` CSS ของหน้า express
  - `Views/Staff/Express.cshtml:126-156` hero และ metric cards
  - `Views/Staff/Express.cshtml:158-210` ตารางงานจัดส่งที่ต้องจัดการ และตารางประวัติทั้งหมด
  - `Views/Staff/Express.cshtml:215-419` script สำหรับโหลด snapshot และอัปเดตสถานะผ่าน dropdown
- **ViewModel ที่ใช้**
  - `ViewModels/Staff/ExpressViewModels.cs:8-17` `ExpressDashboardViewModel`
  - `ViewModels/Staff/ExpressViewModels.cs:19-24` `ExpressSummaryMetrics`
  - `ViewModels/Staff/ExpressViewModels.cs:26-43` `ExpressShipmentRow`
  - `ViewModels/Staff/ExpressViewModels.cs:45-53` `ExpressStatusUpdateRequest`
- **Controller/Service ที่ใช้**
  - `Controllers/StaffController.cs:139-148` `Express()` เปิดหน้า express และส่ง dashboard model
  - `Controllers/StaffController.cs:727-745` `ExpressShipments()` ส่ง snapshot ล่าสุดเป็น JSON
  - `Controllers/StaffController.cs:749-768` `UpdateShipmentStatus()` รับสถานะใหม่จาก dropdown
  - `Services/StaffExpressService.cs:18-23` กำหนดสถานะมาตรฐาน 3 ค่า: `packing`, `delivering`, `done`
  - `Services/StaffExpressService.cs:32-49` `GetDashboardAsync()` สร้าง dashboard model
  - `Services/StaffExpressService.cs:56-80` `UpdateStatusAsync()` อัปเดตทั้ง `shipments.shipping_status` และ `orders.order_status`
  - `Services/StaffExpressService.cs:82-139` query shipment และ map เป็น row สำหรับหน้า view
  - `Services/StaffExpressService.cs:141-180` normalize สถานะหลายรูปแบบให้เป็นค่ากลางของระบบ
- **หลักการทำงานและการเชื่อมกัน**
  - หน้า express โหลด `ExpressDashboardViewModel` ครั้งแรกจาก controller
  - script ในหน้าใช้ `ExpressShipments()` เพื่อ refresh แบบ AJAX
  - เมื่อเปลี่ยน dropdown ระบบเรียก `UpdateShipmentStatus()`
  - service จะ normalize สถานะ แล้ว sync ไปทั้ง `shipments` และ `orders` เพื่อให้ฝั่ง staff กับฝั่งลูกค้าเห็นสถานะตรงกัน

## 14. Admin Dashboard

- **หน้าที่ของหน้า**: เป็นจุดเข้าของ admin ซึ่งใช้โครงสร้าง dashboard เดียวกับ staff แต่ admin เห็นทุกเมนู
- **View ที่ใช้**
  - `Views/Staff/Index.cshtml:6-144` เป็น dashboard หลักที่ admin ใช้งานจริง
  - `Views/Admin/Index.cshtml:1-12` เป็นหน้า placeholder เดิมของ admin area
- **ViewModel ที่ใช้**
  - `ViewModels/StaffDashboardViewModel.cs:5-17` ใช้กับหน้า dashboard ของ admin เช่นเดียวกับ staff
- **Controller/Service ที่ใช้**
  - `Controllers/AdminController.cs:10-13` `Index()` เรียก `StaffNavigationService.BuildDashboard(User)` แล้วส่งไป `Views/Staff/Index.cshtml`
  - `Controllers/AdminController.cs:16-29` action redirect ไปหน้าจัดการของ staff area
  - `Services/StaffNavigationService.cs:55-81` ถ้า user เป็น admin จะคืนทุก section
- **หลักการทำงานและการเชื่อมกัน**
  - admin ไม่ได้มี dashboard logic แยกเอง
  - ใช้ dashboard ของ staff เพื่อ reuse UI และ behavior
  - ความต่างหลักคือ `IsAdmin = true` และ service จะส่งทุก section กลับมา

## 15. Admin

- **หน้าที่ของหน้า**: คือการทำงานของผู้ดูแลระบบในภาพรวม ซึ่งในโปรเจกต์นี้ใช้หน้าของ staff ทั้ง 4 ส่วนเป็นแกนหลัก แล้วมี controller admin คอย redirect ไปยังหน้างานนั้น
- **View ที่ใช้**
  - `Views/Staff/Stock.cshtml:158-800` ใช้สำหรับจัดการสินค้าและ stock
  - `Views/Staff/ManageUsers.cshtml:116-465` และ `Views/Staff/ManageStaff.cshtml:6-88` ใช้จัดการผู้ใช้และพนักงาน
  - `Views/Staff/Sales.cshtml:195-902` ใช้ดูยอดขายและจัดการคูปอง
  - `Views/Staff/Express.cshtml:126-419` ใช้จัดการสถานะการส่งสินค้า
  - `Views/Admin/ManageProducts.cshtml:1-9` และ `Views/Admin/ManageUsers.cshtml:1-9` เป็น placeholder เดิม ไม่ใช่หน้าทำงานหลักปัจจุบัน
- **ViewModel ที่ใช้**
  - `ViewModels/StaffDashboardViewModel.cs:5-17`
  - `ViewModels/Stock/StockModels.cs:7-113`
  - `ViewModels/Staff/ManagerViewModels.cs:7-52`
  - `ViewModels/StaffManagementViewModel.cs:7-24`
  - `ViewModels/CreateStaffViewModel.cs:5-30`
  - `ViewModels/Staff/SalesViewModels.cs:8-106`
  - `ViewModels/Staff/ExpressViewModels.cs:8-53`
- **Controller/Service ที่ใช้**
  - `Controllers/AdminController.cs:10-29` เป็นตัว gateway ของ admin
  - `Controllers/StaffController.cs:36-768` คือ controller หลักที่ admin ใช้งานจริงผ่านการ redirect
  - `Services/StaffNavigationService.cs:55-81` กำหนดสิทธิ์เมนูทั้งหมดให้ admin
  - `Services/StaffSalesService.cs:23-371` ใช้ในงานฝั่ง sales
  - `Services/StaffExpressService.cs:32-180` ใช้ในงานฝั่ง express
- **หลักการทำงานและการเชื่อมกัน**
  - ผู้ใช้ role admin เข้า `AdminController.Index`
  - controller ส่งไป dashboard กลาง
  - เมื่อกดเมนูต่าง ๆ ระบบจะวิ่งไปหน้า staff ที่ตรงหน้าที่จริง
  - สรุปคือ admin เป็นผู้ใช้ที่มีสิทธิ์ครอบทุก workflow ของ staff

## สรุป flow การเชื่อมทั้งระบบแบบใช้พรีเซนต์

- `AccountController` เป็นจุดเริ่มของระบบ: login, register, forgot password, profile
- หลัง login สำเร็จ user จะเข้า `HomeController.Index`
- ถ้าค้นหาสินค้าจะวิ่งไป `ProductController.Index`
- ถ้าเลือกซื้อสินค้าจะใช้ `CartController` และ `CheckoutController`
- หลังชำระเงิน ระบบสร้าง `orders`, `order_items`, `payments`, `shipments`
- ผู้ใช้ดูประวัติผ่าน `OrderController`
- ฝั่ง staff และ admin ใช้ `StaffController` เป็นแกนหลักของการจัดการหลังบ้าน
- `Services` เป็นชั้น business logic ที่ช่วยไม่ให้ controller รับงานหนักเกินไป เช่น
  - `ProductDisplayMapper` ช่วยแปลงสินค้าเป็น card model
  - `CartPricingCalculator` รวมกฎคิดราคาของตะกร้า
  - `CheckoutService` รวม logic การ checkout และบันทึก order
  - `StaffNavigationService` สร้างเมนู dashboard ตาม role
  - `StaffSalesService` รวม logic รายงานขายและคูปอง
  - `StaffExpressService` รวม logic งานจัดส่งและ sync สถานะ order/shipment

## อธิบายโฟลเดอร์ Services เพิ่มเติม

- **`Services/ProductDisplayMapper.cs`**
  - ไฟล์นี้ทำหน้าที่แปลง entity `Product` จากฐานข้อมูลให้เป็น `ProductCardViewModel` ที่หน้า Home และ Search ใช้แสดงผล
  - โค้ดหลักอยู่ที่ `Services/ProductDisplayMapper.cs:27-57` ส่วน `Services/ProductDisplayMapper.cs:11-25` และ `Services/ProductDisplayMapper.cs:59-62` ใช้จัดการรูป fallback หรือ map รูปตามชื่อสินค้า
  - ใช้ใน `HomeController.Index` (`Controllers/HomeController.cs:23-85`) และ `ProductController.Index` (`Controllers/ProductController.cs:25-90`)

- **`Services/CartPricingCalculator.cs`**
  - ไฟล์นี้รวมกติกาคิดราคาตะกร้า เช่น ซื้อ 2 คู่ขึ้นไปลด 10% และถ้ายอดสุทธิต่ำกว่า 3000 บาทคิดค่าส่ง 300 บาท
  - โค้ดหลักอยู่ที่ `Services/CartPricingCalculator.cs:14-52` และ model ภายใน service อยู่ที่ `Services/CartPricingCalculator.cs:54-76`
  - ใช้ใน `CartController` (`Controllers/CartController.cs:299-420`) และ `CheckoutService` (`Services/CheckoutService.cs:136-265`)

- **`Services/CheckoutService.cs`**
  - ไฟล์นี้เป็น business logic ของการ checkout จริง เช่น โหลด snapshot ของตะกร้า, ตรวจ coupon, คิดยอดสุดท้าย, สร้าง `orders`, `order_items`, `payments`, `shipments`
  - `Services/CheckoutService.cs:19-23` ใช้ preview coupon/totals
  - `Services/CheckoutService.cs:25-99` ใช้บันทึกคำสั่งซื้อจริง
  - `Services/CheckoutService.cs:101-128` โหลดข้อมูลตะกร้าจากฐานข้อมูล
  - `Services/CheckoutService.cs:130-224` ตรวจ coupon และคำนวณส่วนลด
  - ใช้ผ่าน `CheckoutController` (`Controllers/CheckoutController.cs:26-116`)

- **`Services/StaffNavigationService.cs`**
  - ไฟล์นี้ใช้กำหนดว่า role ไหนเห็นเมนูอะไรใน Staff/Admin Dashboard
  - `Services/StaffNavigationService.cs:11-40` กำหนดรายการ section ของ staff
  - `Services/StaffNavigationService.cs:43-53` จัดการ alias ของ role
  - `Services/StaffNavigationService.cs:55-81` สร้าง dashboard ตามสิทธิ์ของ user
  - ใช้ใน `StaffController.Index` (`Controllers/StaffController.cs:30-34`) และ `AdminController.Index` (`Controllers/AdminController.cs:10-13`)

- **`Services/StaffSalesService.cs`**
  - ไฟล์นี้รวม logic ฝั่ง Staff Sell ทั้งหมด เช่น CRUD คูปอง, สรุปยอดขายรายเดือน/รายปี, และสรุปสินค้ายอดนิยม
  - `Services/StaffSalesService.cs:23-85` จัดการคูปอง
  - `Services/StaffSalesService.cs:105-152` สรุปยอดขาย
  - `Services/StaffSalesService.cs:153-197` สรุปสินค้ายอดนิยม
  - `Services/StaffSalesService.cs:221-371` เป็น helper เช่น validate ช่วงเวลา coupon, normalize ค่าตัวเลข, สร้างช่วงวันที่รายงาน
  - ใช้ผ่าน `StaffController.Sales`, `ListCoupons`, `CreateCoupon`, `UpdateCoupon`, `DeleteCoupon`, `SalesSummary`, `TopProducts` (`Controllers/StaffController.cs:98-271`)

- **`Services/StaffExpressService.cs`**
  - ไฟล์นี้รวม logic ฝั่ง Staff Express เช่น โหลด dashboard ขนส่ง, สร้างรายการ shipment สำหรับหน้า view, normalize สถานะภาษาไทย/อังกฤษ, และอัปเดตสถานะส่งของให้ sync กับ `orders`
  - `Services/StaffExpressService.cs:18-23` นิยามสถานะกลางของระบบ
  - `Services/StaffExpressService.cs:32-54` โหลด snapshot/dashboard
  - `Services/StaffExpressService.cs:56-80` อัปเดตสถานะ shipment และ order
  - `Services/StaffExpressService.cs:82-139` query และ map row สำหรับหน้า express
  - `Services/StaffExpressService.cs:141-180` normalize และสร้าง dropdown option
  - ใช้ผ่าน `StaffController.Express`, `ExpressShipments`, `UpdateShipmentStatus` (`Controllers/StaffController.cs:139-148`, `Controllers/StaffController.cs:727-768`)

## ทำไมต้องแยก Services ออกจาก Controller

- **Controller ควรรับผิดชอบแค่ flow ของ request/response**
  - เช่น รับค่าจาก form หรือ AJAX, ตรวจ `ModelState`, ส่ง view หรือ JSON กลับ
  - ถ้าเอา logic คำนวณราคา, สรุปยอดขาย, sync สถานะ shipment ไปเขียนใน controller ทั้งหมด ไฟล์ controller จะยาวมากและอ่านยาก

- **Service เหมาะกับ business logic ที่ใช้ซ้ำหลายจุด**
  - ตัวอย่างชัดคือ `CartPricingCalculator` ถูกใช้ทั้งใน `CartController` และ `CheckoutService`
  - `StaffNavigationService` ถูกใช้ทั้งฝั่ง `StaffController` และ `AdminController`

- **แยกแล้วดูแลง่ายกว่า**
  - เวลาแก้กติกาคิดราคา จะไปแก้ที่ service เดียว ไม่ต้องไล่แก้หลาย controller
  - เวลา debug งานขายหรือ shipment จะหาจุดรวม logic ได้เร็ว

- **แยกแล้วทดสอบง่ายกว่า**
  - service มักเป็นโค้ดที่มีเงื่อนไขธุรกิจเยอะ เช่น คูปอง, shipping status, รายงานยอดขาย
  - ถ้าแยกไว้ชัด จะเขียน unit test หรือทดสอบเฉพาะส่วนได้ง่ายกว่า controller

- **สรุปสั้น ๆ**
  - Controller = รับ request / ส่ง response
  - Service = ประมวลผลกฎธุรกิจของระบบ
  - การแยกแบบนี้ทำให้โครงสร้างโปรเจกต์ชัดตามหลัก MVC มากกว่าเขียนทุกอย่างกองอยู่ใน controller

## ภาพรวมของระบบโดยสรุป

- **ภาษาและเทคโนโลยีที่ใช้**
  - Backend ใช้ `C#` บน `ASP.NET Core MVC`
  - Frontend ใช้ `Razor (.cshtml)`, `HTML`, `CSS`, `JavaScript`
  - ใช้ `Entity Framework Core` สำหรับ query และบันทึกข้อมูลกับฐานข้อมูล
  - ใช้ `Bootstrap` สำหรับ layout, modal, ตาราง, ปุ่ม และ responsive UI
  - ใช้ `AJAX/fetch` ในหลายหน้า เช่น Cart, Orders modal, Stock, Sales, Express, Manager เพื่ออัปเดตข้อมูลแบบไม่ต้อง reload ทั้งหน้า

- **ภาพรวมตามหลัก MVC**
  - `Model` คือ entity จากฐานข้อมูลและ ViewModel ที่ใช้รับ-ส่งข้อมูลกับหน้า view
  - `View` คือไฟล์ `.cshtml` ที่แสดงผล UI
  - `Controller` คือจุดรับ request จากผู้ใช้ แล้วส่งต่อไปยัง service หรือฐานข้อมูล ก่อนตอบกลับเป็น view หรือ JSON
  - `Services` เป็นชั้นช่วยประมวลผล business logic ระหว่าง controller กับ model/database

- **การใช้หลัก CRUD ในโปรเจกต์**
  - `Create` เช่น สมัครสมาชิก, เพิ่มสินค้า, เพิ่ม variant, เพิ่ม staff, สร้าง coupon, สร้าง order ตอน checkout
  - `Read` เช่น หน้า Home, Search, Cart, History, Staff Dashboard, รายงานยอดขาย, รายการ shipment
  - `Update` เช่น แก้ profile, เปลี่ยนจำนวนสินค้าใน cart, แก้ข้อมูลสินค้า, แก้ role ผู้ใช้, เปลี่ยนสถานะ shipment, แก้ coupon
  - `Delete` เช่น ลบสินค้าออกจาก cart, ลบ coupon, ลบผู้ใช้ที่ manager จัดการ

- **สรุปแนวคิดของระบบ**
  - ระบบนี้เป็นเว็บขายรองเท้าที่มีทั้งฝั่งลูกค้าและฝั่งหลังบ้าน
  - ฝั่งลูกค้าเน้น flow ตั้งแต่ login -> เลือกสินค้า -> cart -> checkout -> history
  - ฝั่ง staff/admin เน้น flow จัดการสินค้า, ผู้ใช้, ยอดขาย, และการจัดส่ง
  - โครงสร้างโดยรวมพยายามแยกหน้าที่ชัดเจนระหว่าง View, Controller, Service และ Database เพื่อให้ดูแลง่ายและขยายระบบต่อได้
