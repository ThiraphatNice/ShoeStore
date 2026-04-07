# Controller Summaries

## AccountController.cs

1. **หน้าที่หลัก / View / ViewModel ที่ใช้** – ควบคุมงานบัญชีผู้ใช้ทั้งหมด (เข้าสู่ระบบ, สมัคร, ลืมรหัส, โปรไฟล์, ออกจากระบบ, AccessDenied) โดยคืน Views ชื่อเดียวกับ action (`Login`, `Register`, `ForgetPassword`, `Profile`, `AccessDenied`) และส่ง `LoginViewModel`, `RegisterViewModel`, `ForgetPasswordViewModel`, `ProfilePageViewModel`, `ProfileEditViewModel` จาก `ShoeStore.ViewModels.Account`. วิธีตรวจสอบคือเปิด action แล้วดู `return View(...)` เพื่อรู้ว่าโยงไป View ไหน และดูพารามิเตอร์ของ action (เช่น `[FromBody]`, `ProfileEditViewModel model`) เพื่อรู้ว่า binding กับ ViewModel ใด
2. **สรุปการทำงานของ action หลัก**
   - `Login (GET)` – `AccountController.cs:22-26` แสดงหน้าเข้าสู่ระบบอย่างเดียว (ไม่มี model) ใช้เมื่อ route `/Account/Login` ถูกเรียก
   - `Login (POST)` – `AccountController.cs:28-55` ตรวจสอบ `LoginViewModel`, โหลดผู้ใช้และ Role จาก `_context.Users.Include(u => u.Role)`, เช็กรหัสผ่าน (`PasswordMatches`), สร้าง Claims ผ่าน `BuildClaims`, เซ็นอินด้วยคุกกี้ (`SignInAsync`) แล้ว Redirect ไป `Home/Index`
   - `Register (GET)` – `AccountController.cs:57-61` ส่งฟอร์มสมัครสมาชิกเปล่า ๆ
   - `Register (POST)` – `AccountController.cs:63-100` ตรวจสอบ `RegisterViewModel`, กันอีเมลซ้ำ, หา Role ลูกค้า (Id=2 หรือ RoleName=`Users`), บันทึก `User`, เซ็ต `ViewBag.Success` และคืน View `Login` พร้อมเติมอีเมลผู้สมัคร
   - `ForgetPassword (GET/POST)` – `AccountController.cs:102-128` แสดงฟอร์มและประมวลผล `ForgetPasswordViewModel`, หากพบอีเมลจะเปลี่ยน `PasswordHash` แล้วส่งกลับหน้า `Login` พร้อมข้อความสำเร็จ
   - `Logout (POST)` – `AccountController.cs:130-137` (Authorize + AntiForgery) เรียก `SignOutAsync` เพื่อลบคุกกี้ auth แล้ว redirect ไป `Login`
   - `Profile (GET)` – `AccountController.cs:139-151` ใช้ `GetCurrentUserAsync` ดึงข้อมูล, ถ้าไม่พบให้ไป `Login`, ถ้าพบจะเตรียม `ProfilePageViewModel` ผ่าน `BuildProfilePageModel` และยก TempData ไป View
   - `UpdateProfile (POST)` – `AccountController.cs:153-192` รับ `[Bind(Prefix="EditForm")] ProfileEditViewModel`, ตรวจอีเมลซ้ำ, หาก ModelState invalid จะตั้ง `ViewData["OpenEditModal"]` เพื่อให้ View รู้ว่าต้องเปิด modal อีกครั้ง, เมื่อ valid จะอัปเดตข้อมูล + password (ถ้ามี), บันทึก, รีเฟรชคุกกี้ (`RefreshUserSignInAsync`) และ Redirect กลับ `Profile`
   - `AccessDenied (GET)` – `AccountController.cs:194-198` คืน View แจ้งสิทธิ์ไม่พอ
   - **Helpers** – `PasswordMatches` (เปรียบเทียบ string), `BuildClaims` (เพิ่ม Claim Admin/Staff เพิ่มเติม), `GetCurrentUserAsync` (อ่าน Claim ชื่อผู้ใช้แล้วโหลดพร้อม Role), `BuildProfilePageModel` (รวมข้อมูลแสดง+ฟอร์ม), `RefreshUserSignInAsync` (ออกคุกกี้ใหม่หลังแก้ข้อมูล)

## AdminController.cs

1. **หน้าที่หลัก / View / ViewModel ที่ใช้** – จำกัดให้ Role `Admin` เท่านั้น (`[Authorize(Roles="Admin")]`) เพื่อเป็นทางลัดไปยัง UI ฝั่ง staff. ใช้ `StaffNavigationService.BuildDashboard(User)` เพื่อสร้าง model ให้กับ View `/Views/Staff/Index.cshtml`. วิธีดูจุดเชื่อมต่อคือดู `return View("~/Views/Staff/Index.cshtml", model)` และ `RedirectToAction` ที่โยงไป action ใน `StaffController`
2. **สรุปการทำงาน**
   - `Index` – `AdminController.cs:10-14` เรียก `StaffNavigationService.BuildDashboard(User)` เพื่อให้ Admin ใช้เมนูเดียวกับ staff
   - `ManageUsers/ManageProducts/ManageStaff` – `AdminController.cs:16-29` เพียง redirect ไปยัง action ใน `StaffController` (เช่น `ManageUsers`, `Stock`, `ManageStaff`) เพื่อไม่ต้องจำ URL อื่น

## CartController.cs

1. **หน้าที่หลัก / View / ViewModel ที่ใช้** – ดูแลตะกร้าสินค้าแบบ AJAX สำหรับผู้ใช้ที่ล็อกอิน (`[Authorize]`). ใช้ `CartPageViewModel`, `CartItemViewModel`, `CartTotalsViewModel` และ request models (`AddCartItemRequest`, `UpdateCartItemRequest`, `RemoveCartItemRequest`) จาก `ShoeStore.ViewModels.Cart`. หน้า View หลักคือ `Views/Cart/Index.cshtml` ส่วน action อื่นส่ง JSON ให้ JavaScript (สังเกต `return Json(...)` ในแต่ละ method)
2. **สรุป action และโค้ดหลัก**
   - `Index` – `CartController.cs:24-51` ตรวจสอบ user id, โหลด `CartItems` + product/variant/category, map เป็น `CartItemViewModel`, คำนวณส่วนลด/ค่าจัดส่งผ่าน `CartPricingCalculator` แล้วคืน `CartPageViewModel` ให้ View
   - `CheckProfileStatus` – `CartController.cs:53-65` ใช้ `GetCurrentUserAsync` + `BuildProfileStatus` เพื่อให้ JS รู้ว่าต้องบังคับกรอกข้อมูลที่หายไปก่อนจ่ายเงิน (ตอบ JSON)
   - `AddItem` – `CartController.cs:67-144` ตรวจ model และ block staff/admin (`IsInternalPurchaseRestricted`), ตรวจสต็อก, เพิ่ม/อัปเดต `CartItem`, ลดจำนวน `ProductVariant.StockQuantity`, บันทึก, รีคำนวณยอดรวม (`CalculateCartTotalsAsync`) แล้วคืน JSON
   - `UpdateItem` – `CartController.cs:146-230` คล้าย `AddItem` แต่แก้จำนวน, กันกรณีเพิ่มเกินสต็อก, อัปเดต stock/line total, ส่งยอดรวมใหม่
   - `RemoveItem` – `CartController.cs:232-276` คืนสต็อก (เพิ่มจำนวนกลับไปที่ variant), ลบ `CartItem`, บันทึก และส่งยอดรวมใหม่
   - **โปรไฟล์/สิทธิ์** – `IsInternalPurchaseRestricted` (`CartController.cs:337-352`) ปิดการซื้อให้ Role staff/admin, `BuildProfileStatus` (`CartController.cs:376-401`) บอกว่าข้อมูล profile ขาดอะไรบ้าง
   - **การคำนวณรวม** – `CartPricingCalculator` (บริการใหม่) ใช้ใน `Index`, `CalculateCartTotalsAsync`, `BuildCartTotalsViewModel` เพื่อสรุปส่วนลด 10% เมื่อซื้อ ≥2 คู่ และค่าจัดส่ง 300 บาทถ้ายอดสุทธิ < 3,000

## CheckoutController.cs

1. **หน้าที่หลัก / ViewModel ที่ใช้** – จัดการ AJAX สำหรับขั้นตอนจ่ายเงิน/ตรวจคูปอง หลังจากผู้ใช้เปิด modal ใน `Cart/Index`. ใช้ `CheckoutRequest`, `CreditCardInputModel`, `CheckoutResponseViewModel`, `ProfileStatusViewModel` (จาก `ShoeStore.ViewModels.Cart`) และบริการ `CheckoutService`. ทุก action ส่ง JSON จึงไม่มี View เฉพาะ
2. **สรุปการทำงาน**
   - `ValidateCoupon` – `CheckoutController.cs:25-35` ตรวจ user id, เรียก `_checkoutService.PreviewTotalsAsync` ส่งผล JSON เพื่อให้ UI อัปเดตยอดแบบเรียลไทม์
   - `SubmitPayment` – `CheckoutController.cs:38-116` เป็น endpoint หลัก: กัน staff/admin ซื้อสินค้า (`IsInternalPurchaseRestricted`), ตรวจผู้ใช้, เช็กความครบถ้วนของโปรไฟล์ (`BuildProfileStatus`), ตรวจวิธีจ่ายเงิน (Credit Card ต้องมีข้อมูลบัตร + `TryValidateModel`, PromptPay ต้องติ๊กยืนยัน), จากนั้นเรียก `_checkoutService.ProcessCheckoutAsync` ถ้าสำเร็จส่งข้อมูล order id / ยอดสุทธิ / URL ประวัติคำสั่งซื้อ
   - **Helpers** – `GetCurrentUserId`, `BuildProfileStatus`, `IsInternalPurchaseRestricted` เหมือนใน Cart เพื่อ reuse logic ตรวจสิทธิ์/ข้อมูลบังคับ

## HomeController.cs

1. **หน้าที่หลัก / ViewModel ที่ใช้** – สร้างหน้า Landing สำหรับผู้ใช้ที่ล็อกอิน (`[Authorize]`). ใช้ `HomePageViewModel` และ `HomeSectionViewModel` จาก `ShoeStore.ViewModels.Home`, Mapper `ProductDisplayMapper` ช่วยสร้าง card model. View หลักคือ `Views/Home/Index.cshtml`
2. **สรุปการทำงาน**
   - `Index` – `HomeController.cs:23-85` โหลดสินค้าเด่น/ลิมิเต็ด (ใช้ `OrderByDescending` ตามส่วนลดหรือ `CreatedAt`), เลือก 3 หมวดเพื่อทำ section, รวมเป็น `HomePageViewModel` พร้อม `ViewBag.Role` เพื่อให้ View ปรับ UI ตามสิทธิ์
   - `Privacy` – `HomeController.cs:87-90` คืน view ธรรมดา
   - `Error` – `HomeController.cs:92-96` คืน view พร้อม `ErrorViewModel` หากเกิด exception (ใช้ `[ResponseCache(NoStore = true)]` ป้องกัน cache)

## OrderController.cs

1. **หน้าที่หลัก / ViewModel** – แสดงประวัติคำสั่งซื้อและรายละเอียดผ่าน `OrderHistoryPageViewModel`, `OrderSummaryViewModel`, `OrderDetailViewModel`, `ShipmentTimelineItem` จาก `ShoeStore.ViewModels.Order`. ใช้ Views `Views/Order/Index.cshtml`, `Views/Order/Details.cshtml`, และ endpoint JSON (`DetailsData`)
2. **สรุปการทำงาน**
   - `Index` – `OrderController.cs:31-47` ตรวจ user id, โหลดรายการคำสั่งซื้อผ่าน `LoadOrderSummariesAsync` (รวม order items, payments, coupon, shipment) แล้ว map เป็น summary list
   - `Details` – `OrderController.cs:49-65` โหลดเฉพาะ order id ที่เป็นของ user ปัจจุบัน (`BuildOrderDetailAsync`), ถ้าไม่พบส่ง 404, ถ้าพบแสดงหน้า detail
   - `DetailsData` – `OrderController.cs:67-83` เวอร์ชัน JSON ของ `Details` สำหรับ page โหลดข้อมูลแบบ AJAX
   - **ธุรกิจเพิ่มเติม** – `LoadOrderSummariesAsync` (รวมข้อมูลที่เกี่ยวข้องทั้งหมด), `BuildOrderDetailAsync` (thumbnail + ข้อมูลลูกค้า + ไทม์ไลน์), `MapOrderSummary` (คำนวณยอด line total), `BuildTimeline` (ใช้ `TimelineStages` เพื่อแสดงความคืบหน้าการส่ง)

## ProductController.cs

1. **หน้าที่หลัก / ViewModel** – จัดการค้นหา/รายการสินค้า (`[Authorize]`). ใช้ `ProductSearchViewModel` และ `ProductCardViewModel` (ผ่าน `ProductDisplayMapper`). View หลักคือ `Views/Product/Index.cshtml`; action `Details` ยังเป็น placeholder
2. **สรุปการทำงาน**
   - `Index` – `ProductController.cs:25-91` รับ query string `q`, ทำ normalization (ตัด space, ดึงตัวเลข), สร้างคำสั่ง EF เพื่อค้นทั้งชื่อสินค้า, หมวดหมู่ หรือ id; ถ้าไม่มีผลลัพธ์หรือไม่ใส่ query จะดึง suggestion ล่าสุดกลับมาเติมให้หน้าค้นหา
   - `Details` – `ProductController.cs:93-96` ปัจจุบันยังส่ง View เปล่า (ใช้เป็น placeholder หากจะเพิ่มรายละเอียดสินค้าในอนาคต)

## StaffController.cs

1. **หน้าที่หลัก / View / ViewModel** – ฮับสำหรับพนักงาน/แอดมิน (`[Authorize(Roles="Admin,Staff")]`). แยกเมนูตาม role (Stock, Sell, Express, Manager). ใช้ ViewModel จำนวนมาก เช่น `StockPageViewModel`, `SalesDashboardViewModel`, `StaffManagerPageViewModel`, `StaffManagementViewModel`, `InventoryRowViewModel`, `SalesSummaryQuery`, `CouponUpsertRequest`, `ExpressStatusUpdateRequest` ฯลฯ (ดู `using ShoeStore.ViewModels.Staff`, `...Stock`). Views หลักอยู่ใน `Views/Staff/*` และ endpoint JSON ถูกใช้โดยสคริปต์ในหน้า staff (สังเกต `return Json(...)`)
2. **สรุป action หลัก (คัดเฉพาะส่วนผู้ใช้บ่อย)**
   - `Index` – `StaffController.cs:30-34` โหลด dashboard ผ่าน `StaffNavigationService.BuildDashboard(User)` แล้วแสดง view หลักของ staff
   - `Stock` – `StaffController.cs:36-58` เช็กสิทธิ์ `Staff Stock`, ดึงรายการหมวดหมู่เพื่อใช้ใน dropdown (`StockPageViewModel.Categories`)
   - `ManageUsers` – `StaffController.cs:70-96` (ต้องมี role ผู้จัดการ) เตรียม options ของ role ที่ไม่ใช่ admin แล้วคืน `StaffManagerPageViewModel`
   - `Sales` – `StaffController.cs:98-137` สำหรับ `Staff Sell` เลือกเดือน/ปี default แล้วส่ง `SalesDashboardViewModel`
   - `Express` – `StaffController.cs:139-148` สำหรับ `Staff Express` โหลดแดชบอร์ดผ่าน `_staffExpressService.GetDashboardAsync()`
   - **จัดการคูปอง** – `ListCoupons`, `CreateCoupon`, `UpdateCoupon`, `DeleteCoupon` (`StaffController.cs:150-235`) ทั้งหมดบังคับสิทธิ์ `Staff Sell`, ตรวจ ModelState แล้วเรียก `_staffSalesService`
   - **รายงานยอดขาย / สินค้ายอดนิยม** – `SalesSummary`, `TopProducts` (`StaffController.cs:237-271`) รับ `SalesSummaryQuery` ผ่าน query string แล้วส่ง JSON
   - **จัดการบุคลากร** – `ManageStaff`, `CreateStaff`, `ListManagedUsers`, `UpdateManagedUser`, `DeleteManagedUser` (`StaffController.cs:273-687`) จำกัด Admin หรือผู้จัดการ, ใช้ TempData แสดงสถานะ, ตรวจ role / อีเมลซ้ำ / รหัสผ่านผู้จัดการก่อนลบ
   - **ดู/ปรับสินค้าและสต็อก** – `GetProductDetail`, `UpdateProductInfo`, `UpdateVariantStock`, `AddVariant`, `ListInventory`, `CreateProduct`, `RefreshStockTotal` (`StaffController.cs:360-595`) ทำงานร่วมกับ `Stock` page เพื่อแก้ไขข้อมูลสินค้าพร้อมตรวจหมวดหมู่พิเศษ (Limited Edition)
   - **ขนส่งด่วน** – `ExpressShipments`, `UpdateShipmentStatus` (`StaffController.cs:726-768`) เรียก `_staffExpressService` เพื่อดึงรายการและอัปเดตสถานะ shipment
   - **Helper ตรวจสิทธิ์** – `CanAccessSection`, `IsAdminRole`, `IsLimitedCategory`, `GetRoleOptionsAsync` ฯลฯ ใช้ร่วมกับทุก action เพื่อบังคับสิทธิ์แบบละเอียด

> เคล็ดลับการตรวจจุดใช้งาน: ค้นคำว่า `return View` เพื่อดูว่า action ใดโชว์หน้าไหน, ส่วนที่ส่ง JSON จะมี `return Json`, และการเรียกบริการ/ใช้ ViewModel ดูได้จาก dependency ที่ injected อยู่ข้างบนของไฟล์ เช่น `_staffSalesService`, `_staffExpressService`.
