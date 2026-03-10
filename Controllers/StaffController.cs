using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.ViewModels;
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
                Description = "ตรวจเช็กจำนวนสินค้า รับสินค้าเข้า และอัปเดตคลังแบบ real-time",
                ActionName = nameof(Stock)
            },
            ["Staff Manag"] = new StaffSectionOption
            {
                RoleName = "Staff Manag",
                DisplayName = "Operations Hub",
                Description = "ดูแลระบบหลังบ้าน กำกับออเดอร์และการดูแลระบบ",
                ActionName = nameof(ManageOrders)
            },
            ["Staff Sell"] = new StaffSectionOption
            {
                RoleName = "Staff Sell",
                DisplayName = "Sales & Promotions",
                Description = "วางแผนโปรโมชันและแคมเปญการขาย",
                ActionName = nameof(Sales)
            },
            ["Staff Express"] = new StaffSectionOption
            {
                RoleName = "Staff Express",
                DisplayName = "Express Logistics",
                Description = "เตรียมแพ็คสินค้าและดูแลการจัดส่ง",
                ActionName = nameof(Express)
            }
        };

        private static readonly StaffSectionOption AdminSection = new StaffSectionOption
        {
            RoleName = "Admin",
            DisplayName = "Admin Control Center",
            Description = "สร้าง/จัดการบัญชีพนักงานและผู้ใช้งาน",
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

        public IActionResult Stock()
        {
            if (!CanAccessSection("Staff Stock"))
            {
                return Forbid();
            }

            return View();
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
                TempData["StaffError"] = "โปรดกรอกข้อมูลให้ครบถ้วน";
                return RedirectToAction(nameof(ManageStaff));
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == model.RoleId);
            if (role == null || role.RoleName.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                TempData["StaffError"] = "ไม่สามารถเพิ่มบัญชีด้วยสิทธิ์ที่เลือกได้";
                return RedirectToAction(nameof(ManageStaff));
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                TempData["StaffError"] = "อีเมลนี้ถูกใช้งานอยู่แล้ว";
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

            TempData["StaffStatus"] = $"สร้างบัญชี {role.RoleName} สำหรับ {model.FullName} สำเร็จ";
            return RedirectToAction(nameof(ManageStaff));
        }

        private bool CanAccessSection(string roleName)
        {
            return User.IsInRole("Admin") || User.IsInRole(roleName);
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
    }
}
