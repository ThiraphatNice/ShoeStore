using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.ViewModels;
using ShoeStore.ViewModels.Stock;
using System.Security.Claims;

namespace ShoeStore.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class StaffController : Controller
    {
        private readonly ShoeStoreContext _context;

        private static readonly Dictionary<string, StaffSectionOption> StaffSections = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Staff Stock"] = new StaffSectionOption
            {
                RoleName = "Staff Stock",
                DisplayName = "Stock Control",
                Description = "เธ•เธฃเธงเธเน€เธเนเธเธเธณเธเธงเธเธชเธดเธเธเนเธฒ เธฃเธฑเธเธชเธดเธเธเนเธฒเน€เธเนเธฒ เนเธฅเธฐเธญเธฑเธเน€เธ”เธ•เธเธฅเธฑเธเนเธเธ real-time",
                ActionName = nameof(Stock)
            },
            ["Staff Manag"] = new StaffSectionOption
            {
                RoleName = "Staff Manag",
                DisplayName = "Operations Hub",
                Description = "เธ”เธนเนเธฅเธฃเธฐเธเธเธซเธฅเธฑเธเธเนเธฒเธ เธเธณเธเธฑเธเธญเธญเน€เธ”เธญเธฃเนเนเธฅเธฐเธเธฒเธฃเธ”เธนเนเธฅเธฃเธฐเธเธ",
                ActionName = nameof(ManageOrders)
            },
            ["Staff Sell"] = new StaffSectionOption
            {
                RoleName = "Staff Sell",
                DisplayName = "Sales & Promotions",
                Description = "เธงเธฒเธเนเธเธเนเธเธฃเนเธกเธเธฑเธเนเธฅเธฐเนเธเธกเน€เธเธเธเธฒเธฃเธเธฒเธข",
                ActionName = nameof(Sales)
            },
            ["Staff Express"] = new StaffSectionOption
            {
                RoleName = "Staff Express",
                DisplayName = "Express Logistics",
                Description = "เน€เธ•เธฃเธตเธขเธกเนเธเนเธเธชเธดเธเธเนเธฒเนเธฅเธฐเธ”เธนเนเธฅเธเธฒเธฃเธเธฑเธ”เธชเนเธ",
                ActionName = nameof(Express)
            }
        };

        private static readonly StaffSectionOption AdminSection = new StaffSectionOption
        {
            RoleName = "Admin",
            DisplayName = "Admin Control Center",
            Description = "เธชเธฃเนเธฒเธ/เธเธฑเธ”เธเธฒเธฃเธเธฑเธเธเธตเธเธเธฑเธเธเธฒเธเนเธฅเธฐเธเธนเนเนเธเนเธเธฒเธ",
            ActionName = nameof(ManageStaff)
        };

        public StaffController(ShoeStoreContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new StaffDashboardViewModel
            {
                IsAdmin = User.IsInRole("Admin"),
                Sections = GetSectionsForCurrentUser().ToList()
            };

            if (model.IsAdmin)
            {
                model.Sections.Add(AdminSection);
            }

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
            if (!CanAccessSection("Staff Manag"))
            {
                return Forbid();
            }

            return View();
        }

        public IActionResult Sales()
        {
            if (!CanAccessSection("Staff Sell"))
            {
                return Forbid();
            }

            return View();
        }

        public IActionResult Express()
        {
            if (!CanAccessSection("Staff Express"))
            {
                return Forbid();
            }

            return View();
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
                TempData["StaffError"] = "เนเธเธฃเธ”เธเธฃเธญเธเธเนเธญเธกเธนเธฅเนเธซเนเธเธฃเธเธ–เนเธงเธ";
                return RedirectToAction(nameof(ManageStaff));
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == model.RoleId);
            if (role == null || role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["StaffError"] = "เนเธกเนเธชเธฒเธกเธฒเธฃเธ–เน€เธเธดเนเธกเธเธฑเธเธเธตเธ”เนเธงเธขเธชเธดเธ—เธเธดเนเธ—เธตเนเน€เธฅเธทเธญเธเนเธ”เน";
                return RedirectToAction(nameof(ManageStaff));
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                TempData["StaffError"] = "เธญเธตเน€เธกเธฅเธเธตเนเธ–เธนเธเนเธเนเธเธฒเธเธญเธขเธนเนเนเธฅเนเธง";
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

            TempData["StaffStatus"] = $"เธชเธฃเนเธฒเธเธเธฑเธเธเธต {role.RoleName} เธชเธณเธซเธฃเธฑเธ {model.FullName} เธชเธณเน€เธฃเนเธ";
            return RedirectToAction(nameof(ManageStaff));
        }

        private bool CanAccessSection(string roleName)
        {
            return User.IsInRole("Admin") || User.IsInRole(roleName);
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
                return Json(new { success = false, message = "เธเนเธญเธกเธนเธฅเนเธกเนเธ–เธนเธเธ•เนเธญเธ" });
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId);
            if (product == null)
            {
                return Json(new { success = false, message = "เนเธกเนเธเธเธชเธดเธเธเนเธฒ" });
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return Json(new { success = false, message = "เธซเธกเธงเธ”เธซเธกเธนเนเนเธกเนเธ–เธนเธเธ•เนเธญเธ" });
            }

            product.ProductName = request.Name;
            product.Description = request.Description;
            product.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
            product.Price = request.Price;
            product.DiscountPercent = request.DiscountPercent;
            product.CategoryId = request.CategoryId;

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
                return Json(new { success = false, message = "เธเนเธญเธกเธนเธฅเนเธกเนเธเธฃเธเธ–เนเธงเธ" });
            }

            var variant = await _context.ProductVariants
                .FirstOrDefaultAsync(v => v.ProductId == request.ProductId &&
                                          v.Size == request.Size &&
                                          v.Color == request.Color);

            if (variant == null)
            {
                return Json(new { success = false, message = "เนเธกเนเธเธเธฃเธฒเธขเธเธฒเธฃเนเธเธชเน/เธชเธตเธเธตเน" });
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
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "เธเนเธญเธกเธนเธฅเนเธกเนเธเธฃเธเธ–เนเธงเธ" });
            }

            var exists = await _context.ProductVariants.AnyAsync(v =>
                v.ProductId == request.ProductId &&
                v.Size == request.Size &&
                v.Color == request.Color);

            if (exists)
            {
                return Json(new { success = false, message = "เธฃเธฒเธขเธเธฒเธฃเนเธเธชเน/เธชเธตเธเธตเนเธกเธตเธญเธขเธนเนเนเธฅเนเธง" });
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

        [Authorize(Roles = "Admin,Staff Stock")]
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "เธเนเธญเธกเธนเธฅเนเธกเนเธเธฃเธเธ–เนเธงเธ" });
            }

            var categoryExists = await _context.Categories.AnyAsync(c => c.Id == request.CategoryId);
            if (!categoryExists)
            {
                return Json(new { success = false, message = "เธซเธกเธงเธ”เธซเธกเธนเนเนเธกเนเธ–เธนเธเธ•เนเธญเธ" });
            }

            var product = new Product
            {
                ProductName = request.Name,
                Description = request.Description,
                Price = request.Price,
                DiscountPercent = request.DiscountPercent,
                CategoryId = request.CategoryId,
                IsLimited = false,
                StockTotal = 0,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = product.Id });
        }

        private IEnumerable<StaffSectionOption> GetSectionsForCurrentUser()
        {
            if (User.IsInRole("Admin"))
            {
                return StaffSections.Values;
            }

            var roleNames = User.Claims
                .Where(c => c.Type == ClaimTypes.Role && c.Value.StartsWith("Staff", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return roleNames
                .Where(StaffSections.ContainsKey)
                .Select(role => StaffSections[role]);
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
    }
}
