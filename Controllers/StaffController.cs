using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.Services;
using ShoeStore.ViewModels;
using ShoeStore.ViewModels.Staff;
using ShoeStore.ViewModels.Stock;
using System.Globalization;
using System.Security.Claims;

namespace ShoeStore.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : Controller
    {
        private const string StaffManagerRole = "Staff Manager";
        private readonly ShoeStoreContext _context;
        private readonly StaffSalesService _staffSalesService;
        private readonly StaffExpressService _staffExpressService;

        public StaffController(ShoeStoreContext context, StaffSalesService staffSalesService, StaffExpressService staffExpressService)
        {
            _context = context;
            _staffSalesService = staffSalesService;
            _staffExpressService = staffExpressService;
        }

        public IActionResult Index()
        {
            var model = StaffNavigationService.BuildDashboard(User);
            return View(model);
        }

        public async Task<IActionResult> Stock()
        {
            if (!CanAccessSection("Staff Stock"))
            {
                return Forbid();
            }

            var categories = await _context.Categories
                .OrderBy(c => c.CategoryName)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CategoryName
                })
                .ToListAsync();

            var model = new StockPageViewModel
            {
                Categories = categories
            };

            return View(model);
        }

        public IActionResult ManageOrders()
        {
            if (!CanAccessSection(StaffManagerRole))
            {
                return Forbid();
            }

            return View();
        }

        [Authorize(Roles = "Admin,Staff")]
        public async Task<IActionResult> ManageUsers()
        {
            if (!CanAccessSection(StaffManagerRole))
            {
                return Forbid();
            }

            var roles = (await _context.Roles
                .AsNoTracking()
                .OrderBy(r => r.RoleName)
                .ToListAsync())
                .Where(r => !IsAdminRole(r.RoleName))
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RoleName
                })
                .ToList();

            var model = new StaffManagerPageViewModel
            {
                RoleOptions = roles
            };

            return View(model);
        }

        public IActionResult Sales()
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            var now = DateTime.UtcNow;
            var culture = CultureInfo.GetCultureInfo("th-TH");

            var yearOptions = Enumerable.Range(0, 3)
                .Select(offset => now.Year - offset)
                .Select(year => new SelectListItem
                {
                    Value = year.ToString(),
                    Text = year.ToString(),
                    Selected = year == now.Year
                })
                .ToList();

            var monthOptions = Enumerable.Range(1, 12)
                .Select(month => new SelectListItem
                {
                    Value = month.ToString(),
                    Text = culture.DateTimeFormat.GetMonthName(month),
                    Selected = month == now.Month
                })
                .ToList();

            var model = new SalesDashboardViewModel
            {
                YearOptions = yearOptions,
                MonthOptions = monthOptions,
                DefaultScope = "monthly",
                DefaultYear = now.Year,
                DefaultMonth = now.Month
            };

            return View(model);
        }

        public async Task<IActionResult> Express()
        {
            if (!CanAccessSection("Staff Express"))
            {
                return Forbid();
            }

            var dashboard = await _staffExpressService.GetDashboardAsync();
            return View(dashboard);
        }

        [HttpGet]
        public async Task<IActionResult> ListCoupons()
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            var data = await _staffSalesService.GetCouponsAsync();
            return Json(new { success = true, data });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponUpsertRequest request)
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลคูปองให้ครบถ้วน" });
            }

            try
            {
                var data = await _staffSalesService.CreateCouponAsync(request);
                return Json(new { success = true, data });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateCoupon(int id, [FromBody] CouponUpsertRequest request)
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลคูปองให้ครบถ้วน" });
            }

            try
            {
                var data = await _staffSalesService.UpdateCouponAsync(id, request);
                return Json(new { success = true, data });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteCoupon([FromBody] CouponDeleteRequest request)
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "เลือกคูปองที่ต้องการลบก่อน" });
            }

            try
            {
                await _staffSalesService.DeleteCouponAsync(request.CouponId);
                return Json(new { success = true });
            }
            catch (InvalidOperationException ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> SalesSummary([FromQuery] SalesSummaryQuery query)
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            query ??= new SalesSummaryQuery();
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "พารามิเตอร์สรุปยอดขายไม่ถูกต้อง" });
            }

            var data = await _staffSalesService.GetSalesSummaryAsync(query);
            return Json(new { success = true, data });
        }

        [HttpGet]
        public async Task<IActionResult> TopProducts([FromQuery] SalesSummaryQuery query, [FromQuery] int limit = 5)
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            query ??= new SalesSummaryQuery();
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "พารามิเตอร์สรุปยอดขายไม่ถูกต้อง" });
            }

            var data = await _staffSalesService.GetTopProductsAsync(query, limit);
            return Json(new { success = true, data });
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ManageStaff()
        {
            var roles = await GetRoleOptionsAsync();
            var users = await _context.Users
                .Include(u => u.Role)
                .Where(u => !u.Role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new StaffSummaryViewModel
                {
                    Id = u.Id,
                    FullName = u.Fullname,
                    Email = u.Email,
                    RoleName = u.Role.RoleName,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            var viewModel = new StaffManagementViewModel
            {
                RoleOptions = roles,
                ExistingUsers = users,
                StatusMessage = TempData["StaffStatus"] as string,
                ErrorMessage = TempData["StaffError"] as string
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateStaff(CreateStaffViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["StaffError"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return RedirectToAction(nameof(ManageStaff));
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == model.RoleId);
            if (role == null || role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["StaffError"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return RedirectToAction(nameof(ManageStaff));
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                TempData["StaffError"] = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return RedirectToAction(nameof(ManageStaff));
            }

            var newUser = new Models.db.User
            {
                Fullname = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                RoleId = role.Id
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            TempData["StaffStatus"] = $"สร้าง {role.RoleName} สำหรับ {model.FullName} เรียบร้อยแล้ว";
            return RedirectToAction(nameof(ManageStaff));
        }

        private bool CanAccessSection(string roleName)
        {
            if (User.IsInRole("Admin"))
            {
                return true;
            }

            foreach (var candidate in StaffNavigationService.GetRoleNamesForAccess(roleName))
            {
                if (User.IsInRole(candidate))
                {
                    return true;
                }
            }

            return false;
        }

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpGet]
        public async Task<IActionResult> GetProductDetail(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return Json(new { success = false, message = "ไม่พบข้อมูลสินค้าที่ค้นหา" });
            }

            var response = new ProductDetailResponse
            {
                Id = product.Id,
                Name = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                DiscountPercent = product.DiscountPercent ?? 0m,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.CategoryName,
                ImageUrl = product.ImageUrl,
                Variants = product.ProductVariants
                    .OrderBy(v => v.Size)
                    .ThenBy(v => v.Color)
                    .Select(v => new ProductVariantViewModel
                    {
                        Id = v.Id,
                        Size = v.Size,
                        Color = v.Color,
                        StockQuantity = v.StockQuantity ?? 0
                    })
                    .ToList()
            };

            return Json(new { success = true, data = response });
        }

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateProductInfo([FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลสินค้าไม่ถูกต้อง" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
            {
                return Json(new { success = false, message = "ไม่พบข้อมูลสินค้าที่ค้นหา" });
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
            if (category == null)
            {
                return Json(new { success = false, message = "ไม่พบหมวดหมู่ที่เลือก" });
            }

            product.ProductName = request.Name;
            product.Description = request.Description;
            product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            product.Price = request.Price;
            product.DiscountPercent = request.DiscountPercent;
            product.CategoryId = request.CategoryId;
            product.IsLimited = IsLimitedCategory(category.CategoryName);

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateVariantStock([FromBody] UpdateVariantStockRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == request.ProductId &&
                                          v.Size == request.Size &&
                                          v.Color == request.Color);

            if (variant == null)
            {
                return Json(new { success = false, message = "ไม่พบตัวเลือกสินค้าที่เลือก" });
            }

            variant.StockQuantity = request.Quantity;
            await _context.SaveChangesAsync();
            await RefreshStockTotal(request.ProductId);

            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddVariant([FromBody] AddVariantRequest request)
        {
            var normalizedSize = request.Size?.Trim() ?? string.Empty;
            var normalizedColor = request.Color?.Trim() ?? string.Empty;
            if (!string.Equals(request.Size, normalizedSize, StringComparison.Ordinal) ||
                !string.Equals(request.Color, normalizedColor, StringComparison.Ordinal))
            {
                request.Size = normalizedSize;
                request.Color = normalizedColor;
                ModelState.Clear();
                TryValidateModel(request);
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }

            var exists = await _context.ProductVariants.AnyAsync(v =>
                v.ProductId == request.ProductId &&
                v.Size == request.Size &&
                v.Color == request.Color);

            if (exists)
            {
                return Json(new { success = false, message = "ตัวเลือกสินค้าซ้ำกัน" });
            }

            var variant = new ProductVariant
            {
                ProductId = request.ProductId,
                Size = request.Size,
                Color = request.Color,
                StockQuantity = 0
            };

            _context.ProductVariants.Add(variant);
            await _context.SaveChangesAsync();
            await RefreshStockTotal(request.ProductId);

            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpGet]
        public async Task<IActionResult> ListInventory()
        {
            var data = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .OrderBy(p => p.Id)
                .Take(100)
                .Select(p => new InventoryRowViewModel
                {
                    Id = p.Id,
                    Name = p.ProductName,
                    Category = p.Category.CategoryName,
                    Price = p.Price,
                    DiscountPercent = p.DiscountPercent ?? 0m,
                    StockTotal = p.StockTotal,
                    IsLimited = p.IsLimited
                })
                .ToListAsync();

            return Json(new { success = true, data });
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpGet]
        public async Task<IActionResult> ListManagedUsers()
        {
            if (!CanAccessSection(StaffManagerRole))
            {
                return Forbid();
            }

            var users = (await _context.Users
                .AsNoTracking()
                .Include(u => u.Role)
                .OrderBy(u => u.Id)
                .ToListAsync())
                .Where(u => !IsAdminRole(u.Role.RoleName))
                .Select(u => new ManagedUserRowViewModel
                {
                    Id = u.Id,
                    RoleId = u.RoleId,
                    RoleName = u.Role.RoleName,
                    FullName = u.Fullname,
                    Email = u.Email,
                    Password = u.PasswordHash,
                    Phone = u.Phone,
                    Address = u.Address
                })
                .ToList();

            return Json(new { success = true, data = users });
        }

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ถูกต้อง" });
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == request.CategoryId);
            if (category == null)
            {
                return Json(new { success = false, message = "ไม่พบหมวดหมู่ที่เลือก" });
            }

            var product = new Product
            {
                ProductName = request.Name,
                Description = request.Description,
                ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim(),
                Price = request.Price,
                DiscountPercent = request.DiscountPercent,
                CategoryId = request.CategoryId,
                IsLimited = IsLimitedCategory(category.CategoryName),
                StockTotal = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = product.Id });
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateManagedUser([FromBody] UpdateManagedUserRequest request)
        {
            if (!CanAccessSection(StaffManagerRole))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ครบถ้วน" });
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (user == null || IsAdminRole(user.Role.RoleName))
            {
                return Json(new { success = false, message = "ไม่สามารถแก้ไขบัญชีนี้ได้" });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == request.RoleId);
            if (role == null || IsAdminRole(role.RoleName))
            {
                return Json(new { success = false, message = "สิทธิ์ที่เลือกไม่ถูกต้อง" });
            }

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == request.Email && u.Id != request.UserId);
            if (emailExists)
            {
                return Json(new { success = false, message = "อีเมลนี้มีผู้ใช้งานแล้ว" });
            }

            user.RoleId = request.RoleId;
            user.Fullname = request.FullName.Trim();
            user.Email = request.Email.Trim();
            user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
            user.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = request.Password;
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [Authorize(Roles = "Admin,Staff")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> DeleteManagedUser([FromBody] DeleteManagedUserRequest request)
        {
            if (!CanAccessSection(StaffManagerRole))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "กรุณากรอกข้อมูลให้ครบ" });
            }

            var target = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == request.UserId);

            if (target == null || IsAdminRole(target.Role.RoleName))
            {
                return Json(new { success = false, message = "ไม่สามารถลบบัญชีนี้ได้" });
            }

            var managerIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(managerIdClaim) || !int.TryParse(managerIdClaim, out var managerId))
            {
                return Json(new { success = false, message = "กรุณาเข้าสู่ระบบใหม่" });
            }

            var manager = await _context.Users.FirstOrDefaultAsync(u => u.Id == managerId);
            if (manager == null || manager.PasswordHash != request.ManagerPassword)
            {
                return Json(new { success = false, message = "รหัสผ่านผู้จัดการไม่ถูกต้อง" });
            }

            _context.Users.Remove(target);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        private async Task<IEnumerable<SelectListItem>> GetRoleOptionsAsync()
        {
            return await _context.Roles
                .Where(r => !r.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                .OrderBy(r => r.RoleName)
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RoleName
                })
                .ToListAsync();
        }

        private async Task RefreshStockTotal(int productId)
        {
            var total = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .SumAsync(v => v.StockQuantity ?? 0);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.StockTotal = total;
                await _context.SaveChangesAsync();
            }
        }

        private static bool IsAdminRole(string roleName)
        {
            return roleName.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsLimitedCategory(string categoryName)
        {
            return categoryName.Equals("Limited Edition", StringComparison.OrdinalIgnoreCase);
        }

        [HttpGet]
        public async Task<IActionResult> ExpressShipments()
        {
            if (!CanAccessSection("Staff Express"))
            {
                return Forbid();
            }

            var snapshot = await _staffExpressService.GetDashboardAsync();
            return Json(new
            {
                success = true,
                data = new
                {
                    pending = snapshot.ActionableShipments,
                    all = snapshot.AllShipments,
                    metrics = snapshot.Metrics
                }
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateShipmentStatus([FromBody] ExpressStatusUpdateRequest request)
        {
            if (!CanAccessSection("Staff Express"))
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลการอัปเดตไม่ถูกต้อง" });
            }

            var result = await _staffExpressService.UpdateStatusAsync(request.ShipmentId, request.NewStatus);
            if (result == null)
            {
                return Json(new { success = false, message = "ไม่พบการจัดส่งที่เลือก" });
            }

            return Json(new { success = true, data = result });
        }
    }
}
