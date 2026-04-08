## 1. Login
- **หน้าที่หลัก**: หน้าเข้าสู่ระบบสำหรับลูกค้า/พนักงาน เพื่อตรวจสอบอีเมล-รหัสผ่านและสร้าง Claims เพื่อกำหนด Dashboard ตามบทบาท (ไฟล์ `Views/Account/Login.cshtml`).
- **Views ที่เกี่ยวข้อง**: `Views/Account/Login.cshtml` แสดงฟอร์มและข้อผิดพลาดจาก `ModelState`; หลังสำเร็จ redirect ไป `Views/Home/Index.cshtml`.
- **ViewModels**: `LoginViewModel` (`ViewModels/LoginViewModel.cs:5-16`) กำหนดฟิลด์ Email/Password พร้อม DataAnnotation ตรวจรูปแบบ; ถูก bind ใน action `AccountController.Login (POST)`.
- **Controllers/Services**: `AccountController.Login` (`Controllers/AccountController.cs:22-55`) รับ GET/POST, ตรวจ `ModelState`, คิวรี `Users.Include(Role)` เพื่อสร้าง Claims, ใช้ `PasswordMatches` และ `BuildClaims` ก่อน `SignInAsync`.
- **Database**: ตาราง `users` (คอลัมน์ `email`, `password_hash`, `role_id`) และ `roles` (`id`, `role_name`) ใช้ตรวจสอบผู้ใช้และ role เพื่อเพิ่ม Claim เสริม (เช่น Staff); ข้อมูลถูกดึงผ่าน EF query ใน action.

## 2. Register
- **หน้าที่หลัก**: หน้า `Views/Account/Register.cshtml` สำหรับผู้ใช้ใหม่ ลงทะเบียนและรับ role ลูกค้าเริ่มต้น.
- **Views**: `Views/Account/Register.cshtml` (ฟอร์มสมัคร + error), สำเร็จแล้ว render `Views/Account/Login.cshtml` พร้อม `ViewBag.Success`.
- **ViewModels**: `RegisterViewModel` (`ViewModels/RegisterViewModel.cs:5-26`) มี FullName/Email/Password/ConfirmPassword และ `[Compare]` เพื่อให้แน่ใจว่ารหัสผ่านตรงกัน.
- **Controller**: `AccountController.Register` (`Controllers/AccountController.cs:57-100`) ตรวจอีเมลซ้ำด้วย `_context.Users.AnyAsync`, หา Role ด้วย `_context.Roles.FirstOrDefaultAsync`, เพิ่ม `User` ใหม่และเซ็ต role ลูกค้า, แล้ว redirect.
- **Database**: ตาราง `users` (Fullname, Email, PasswordHash, RoleId) และ `roles` ใช้เพื่อบันทึกผู้ใช้ทั่วไป (RoleId = 2 หรือ role_name = "Users").

## 3. ForgetPassword
- **หน้าที่หลัก**: ให้ผู้ใช้ตั้งรหัสผ่านใหม่ผ่านหน้า `Views/Account/ForgetPassword.cshtml`.
- **Views**: `Views/Account/ForgetPassword.cshtml` แสดงฟอร์มเปลี่ยนรหัสผ่าน.
- **ViewModels**: `ForgetPasswordViewModel` (`ViewModels/ForgetPasswordViewModel.cs:5-22`) รับ Email + รหัสใหม่+ยืนยัน; ใช้ DataAnnotation ตรวจ.
- **Controller**: `AccountController.ForgetPassword` (`Controllers/AccountController.cs:102-128`) โหลด user ตามอีเมล, หากไม่พบเพิ่ม `ModelState` error, หากพบจะเปลี่ยน `PasswordHash` และส่งกลับ `Login`.
- **Database**: ตาราง `users` ใช้คอลัมน์ `email`, `password_hash` เพื่อค้นหาและอัปเดตค่ารหัสผ่านใหม่.

## 4. Home (Dashboard ลูกค้า)
- **หน้าที่หลัก**: หน้า Landing หลัก (`Views/Home/Index.cshtml`) สำหรับผู้ใช้ล็อกอิน แสดงสินค้าเด่น/ลิมิเต็ด/แยกหมวด.
- **Views**: `Views/Home/Index.cshtml` (Dashboard), `Views/Home/Privacy.cshtml` (นโยบาย), `Views/Home/Error.cshtml` (แสดงข้อผิดพลาด).
- **ViewModels**: `HomePageViewModel`, `HomeSectionViewModel`, `ProductCardViewModel` (ดู `ViewModels/Home/*`); ใช้ `ProductDisplayMapper` เพื่อสร้าง card.
- **Controller**: `HomeController.Index` (`Controllers/HomeController.cs:23-85`) โหลดสินค้าจาก `_context.Products` (include Category, Variants), จัด 3 หมวดหมู่แรก, build section list, ตั้ง `ViewBag.Role`.
- **Database**: ตาราง `products`, `categories`, `product_variants` (คอลัมน์ price, discount_percent, created_at, is_limited) ใช้สร้างรายการ, รวมทั้งใช้ role claim จาก `users/roles` เพื่อกำหนดสิทธิ์ (ผ่าน `ViewBag.Role`).

## 5. Search (Product Search)
- **หน้าที่หลัก**: หน้าค้นหาสินค้า (`Views/Product/Index.cshtml`) ให้กรองด้วยคำค้นหรือรหัสสินค้า.
- **Views**: `Views/Product/Index.cshtml` (ผลค้นหา/ข้อเสนอแนะ), `Views/Product/Details.cshtml` (placeholder รายละเอียดสินค้า).
- **ViewModels**: `ProductSearchViewModel` (ผลลัพธ์/คำค้น) และ `ProductCardViewModel` (รายละเอียดการ์ด) จาก `ViewModels/Product`.
- **Controller**: `ProductController.Index` (`Controllers/ProductController.cs:25-91`) ทำ normalization, วิเคราะห์ว่ามีตัวเลขหรือไม่, คิวรี `Products.Include(Category, Variants)`, จำกัด 24 รายการ, ถ้าผลลัพธ์ว่างใช้ suggestion ล่าสุด.
- **Database**: ตาราง `products` และ `categories` (คอลัมน์ `product_name`, `category_name`, `id`, `created_at`) เพื่อค้นและแสดงผล; `product_variants` ช่วยให้การ์ดบอกไซซ์/สีได้.

## 6. Cart
- **หน้าที่หลัก**: หน้า `Views/Cart/Index.cshtml` ให้ผู้ใช้จัดการตะกร้า, ตรวจยอด, เปิด modal Checkout.
- **Views**: `Views/Cart/Index.cshtml` (แสดงรายการ + modal Checkout).
- **ViewModels**: `CartPageViewModel`, `CartItemViewModel`, `CartTotalsViewModel`, `Add/Update/RemoveCartItemRequest` (`ViewModels/Cart/CartViewModels.cs:9-127`) คำนวณ subtotal/ส่วนลด/ค่าจัดส่ง.
- **Controllers/Services**: `CartController` (`Controllers/CartController.cs:24-420`) มี action `Index`, `AddItem`, `UpdateItem`, `RemoveItem`, `CheckProfileStatus`; ใช้ helper `CartPricingCalculator` (`Services/CartPricingCalculator.cs`) เพื่อคิดส่วนลด 10% สำหรับ ≥2 คู่และค่าส่ง 300 บาทใต้ 3,000. Checkout modal ใช้ `CheckoutController` + `CheckoutService`.
- **Database**: ตาราง `cart_items`, `product_variants`, `products` (คอลัมน์ `stock_quantity`, `price`, `discount_percent`), และ `users` (profile info) ใช้ในการตรวจข้อมูล/สต็อก. เมื่อ checkout จะย้ายข้อมูลไป `orders`, `order_items`, `payments`, `shipments`.

## 7. History (Orders)
- **หน้าที่หลัก**: หน้า `Views/Order/Index.cshtml` และ `Views/Order/Details.cshtml` แสดงประวัติและรายละเอียดคำสั่งซื้อ.
- **Views**: `Views/Order/Index.cshtml` (สรุปประวัติ), `Views/Order/Details.cshtml` (รายละเอียด + ไทม์ไลน์), `Views/Order/_OrderTimeline.cshtml` (partial timeline ถ้ามี).
- **ViewModels**: `OrderHistoryPageViewModel`, `OrderSummaryViewModel`, `OrderDetailViewModel`, `ShipmentTimelineItem` (`ViewModels/Order/OrderHistoryViewModels.cs:8-96`).
- **Controller**: `OrderController` (`Controllers/OrderController.cs:31-215`) มี `Index` (โหลด summary ทั้งหมด), `Details` (แสดงหน้า), `DetailsData` (JSON). ใช้ helper `LoadOrderSummariesAsync`, `BuildOrderDetailAsync`, `BuildTimeline`.
- **Database**: ตาราง `orders`, `order_items`, `payments`, `coupons`, `shipments`, `users` เพื่อประกอบคำสั่งซื้อ, รวมคอลัมน์ `final_amount`, `shipping_status`, `payment_method`, `coupon_id`.

## 8. Profile
- **หน้าที่หลัก**: หน้า `Views/Account/Profile.cshtml` ให้ผู้ใช้เห็นข้อมูลและแก้ไข profile ผ่าน modal.
- **Views**: `Views/Account/Profile.cshtml` (โปรไฟล์ + modal Edit), มี partial ฟอร์ม `Views/Account/_ProfileEditForm.cshtml` หากแยก (ขึ้นกับ UI).
- **ViewModels**: `ProfilePageViewModel`, `ProfileEditViewModel` (`ViewModels/Account/ProfileViewModels.cs:5-44`).
- **Controller**: `AccountController.Profile` + `UpdateProfile` (`Controllers/AccountController.cs:139-192`) โหลด user, สร้าง ViewModel, ตรวจอีเมลซ้ำ, แก้ `User` entity, รีเซ็นอิน (`RefreshUserSignInAsync`), ใช้ TempData แจ้งสถานะ.
- **Database**: ตาราง `users` (คอลัมน์ `fullname`, `email`, `phone`, `address`, `password_hash`, `role_id`) ใช้ดึง/อัปเดตข้อมูลส่วนตัว; role ใช้เพื่อแสดง label ในหน้า.

## 9. Staff Dashboard (แยกตามโรล)
- **หน้าที่หลัก**: `Views/Staff/Index.cshtml` แสดงการ์ดเมนูจาก `StaffNavigationService.BuildDashboard(User)` ให้ตาม role.
- **Views**: `Views/Staff/Index.cshtml` (Dashboard กลาง).
- **Roles**
  - **Staff Stock**: เข้าถึง `Stock` (`StaffController.Stock`) เพื่อจัดการสินค้าและสต็อก; ใช้ ViewModels `StockPageViewModel`, `InventoryRowViewModel`.
  - **Staff Sell**: เข้าถึง `Sales` dashboard, coupon management, summary APIs (`ListCoupons`, `SalesSummary`, `TopProducts`).
  - **Staff Express**: ใช้ `Express` dashboard (โหลดจาก `_staffExpressService.GetDashboardAsync()`).
  - **Staff Manager**: จัดการผู้ใช้ (`ManageUsers`, `ListManagedUsers`, `UpdateManagedUser`).
- **Services**: `StaffNavigationService` สร้างเมนู; `StaffSalesService` สำหรับ Sell (คูปอง, summary); `StaffExpressService` สำหรับ Express; `ProductDisplayMapper` สำหรับ stock view.
- **Database**: ตาราง `roles`, `users` (ค้นสิทธิ์), `products`, `product_variants`, `orders`, `order_items`, `coupons`, `shipments` แล้วแต่เมนูที่ใช้งาน.

## 10. Stock Staff
- **หน้าที่หลัก**: จัดการสินค้าสำหรับคลังสินค้าใน `Views/Staff/Stock.cshtml`.
- **Views**: `Views/Staff/Stock.cshtml` (หน้า stock) รวม partials เช่น `Views/Staff/_VariantTable.cshtml` ถ้าแยก.
- **ViewModels**: `StockPageViewModel`, `ProductDetailResponse`, `UpdateProductRequest`, `AddVariantRequest`, `UpdateVariantStockRequest`, `InventoryRowViewModel` (จาก `ViewModels/Stock/*`).
- **Controller/Service**: `StaffController.Stock` (`Controllers/StaffController.cs:36-58`) + action JSON (`GetProductDetail`, `UpdateProductInfo`, `AddVariant`, `UpdateVariantStock`, `ListInventory`, `CreateProduct`). อาศัย `_context` และ helper `RefreshStockTotal`.
- **Database**: `products`, `categories`, `product_variants` (คอลัมน์ `stock_quantity`, `stock_total`, `price`, `discount_percent`, `image_url`, `is_limited`) ใช้เพิ่ม/แก้ข้อมูล; `products.stock_total` จะอัปเดตหลังแก้ variant.

## 11. Manager Staff
- **หน้าที่หลัก**: จัดการบัญชีพนักงานผ่าน `Views/Staff/ManageUsers.cshtml`.
- **Views**: `Views/Staff/ManageUsers.cshtml` (ตารางผู้ใช้/ฟอร์ม), `Views/Staff/ManageStaff.cshtml` สำหรับ admin.
- **ViewModels**: `StaffManagerPageViewModel`, `ManagedUserRowViewModel`, `UpdateManagedUserRequest`, `DeleteManagedUserRequest`, `CreateStaffViewModel`.
- **Controllers**: Action `ManageUsers`, `ListManagedUsers`, `UpdateManagedUser`, `DeleteManagedUser`, `CreateStaff`, `ManageStaff` (`Controllers/StaffController.cs:70-687`) ใช้ TempData แจ้งผล, ตรวจ role ด้วย `CanAccessSection`, บังคับยืนยันรหัสผ่านผู้จัดการก่อนลบ.
- **Database**: ตาราง `users` และ `roles` (คอลัมน์ `role_id`, `fullname`, `email`, `password_hash`, `phone`, `address`, `created_at`) ใช้เพิ่ม/แก้/ลบพนักงาน; ตรวจ unique email.

## 12. Sell (Sales) Staff
- **หน้าที่หลัก**: Dashboard สรุปยอดขาย+คูปอง (`Views/Staff/Sales.cshtml`) ให้ทีมขาย.
- **Views**: `Views/Staff/Sales.cshtml` (แดชบอร์ด), partial เช่น `Views/Staff/_CouponTable.cshtml` (รายการคูปอง) ถ้ามี.
- **ViewModels**: `SalesDashboardViewModel`, `SalesSummaryQuery`, `CouponUpsertRequest`, `CouponDeleteRequest`.
- **Controllers/Services**: `StaffController.Sales` (`Controllers/StaffController.cs:98-137`) โหลดตัวเลือกเดือน/ปี; action `ListCoupons`, `CreateCoupon`, `UpdateCoupon`, `DeleteCoupon`, `SalesSummary`, `TopProducts` ใช้ `_staffSalesService` (`Services/StaffSalesService.cs`) เพื่อดึงยอดขายและผลิตภัณฑ์ยอดนิยม.
- **Database**: `orders`, `order_items`, `coupons`, `products` (คอลัมน์ `discount_percent`, `final_amount`, `created_at`) ใช้สำหรับรายงาน; คูปองบันทึกที่ `coupons` (รหัส, เปอร์เซ็นต์, min_purchase, start/end date).

## 13. Express Staff
- **หน้าที่หลัก**: จัดการการจัดส่งใน `Views/Staff/Express.cshtml` ทั้งรายการงานปัจจุบัน + ประวัติ.
- **Views**: `Views/Staff/Express.cshtml` (แดชบอร์ด Express), partial สำหรับ shipment cards (ถ้ามี).
- **ViewModels**: `ExpressDashboardViewModel`, `ExpressShipmentViewModel`, `ExpressMetrics` (ดู `ViewModels/Staff/SalesViewModels.cs` และไฟล์ Express).
- **Controllers/Services**: `StaffController.Express` (`Controllers/StaffController.cs:139-148`) โหลดข้อมูลผ่าน `_staffExpressService.GetDashboardAsync()`; action `ExpressShipments` (JSON) และ `UpdateShipmentStatus` ใช้ `_staffExpressService.UpdateStatusAsync`.
- **Database**: ตาราง `shipments` (คอลัมน์ `shipping_status`, `tracking_number`), `orders` (สถานะ), `order_items`, `users` (ข้อมูลลูกค้า). เปลี่ยนสถานะจะเขียน `shipments.shipping_status` และ sync กับ `orders.order_status`.

## 14. Admin Dashboard
- **หน้าที่หลัก**: `AdminController.Index` ใช้ view เดียวกับ staff แต่ role Admin สามารถเข้าถึงเมนูทั้งหมด (Stock, Sell, Express, Manager).
- **Views**: `Views/Staff/Index.cshtml` (ใช้ซ้ำสำหรับ Admin), พร้อมลิงก์ไป View เฉพาะของแต่ละแผนก (`Views/Staff/Stock.cshtml`, `Sales.cshtml`, `Express.cshtml`, `ManageUsers.cshtml`).
- **การใช้งานหน้า Staff**: `AdminController.ManageUsers/ManageProducts/ManageStaff` redirect ไป action ใน `StaffController`, ทำให้ Admin ใช้ UI เดิมทุกส่วน.
- **ViewModels/Services/Database**: เหมือนหัวข้อ 9–13 เพราะ Admin ใช้หน้าของ Staff ทั้งหมด; เพิ่มเติมคือ role `Admin` ใน `roles` และ claim ที่สร้างใน `AccountController` เพื่อปลดล็อกทุกเมนู.

## Services Folder คืออะไร?
- โฟลเดอร์ `Services/` รวม business logic ที่ต้อง reuse หรือซับซ้อนเกินกว่าจะอยู่ใน Controller เช่น `CheckoutService`, `StaffSalesService`, `StaffExpressService`, `StaffNavigationService`, `CartPricingCalculator`. จุดประสงค์เพื่อแยก concerns: Controller ดูแล routing/view binding ส่วน Service รับผิดชอบการประมวลผล/เข้าถึงข้อมูล.
- **ความเกี่ยวข้องกับหัวข้อ**:
  - `CheckoutService` ใช้ในหัวข้อ Cart/Checkout (6, 7) เพื่อประมวลผลคำสั่งซื้อ.
  - `StaffSalesService` ครอบคลุมหัวข้อ Sell Staff/Staff Dashboard/Admin Dashboard (9, 12, 14).
  - `StaffExpressService` ครอบคลุมหัวข้อ Express/Staff Dashboard/Admin Dashboard (9, 13, 14).
  - `StaffNavigationService` ใช้สร้างเมนูในหัวข้อ Staff & Admin Dashboard (9, 14).
  - `CartPricingCalculator` สนับสนุน Cart/Checkout (6).
- การแยก Service ช่วยให้ทดสอบง่าย, reuse ได้หลาย Controller, และลดโค้ดซ้ำ/ความหนาแน่นใน Controller.
