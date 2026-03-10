using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ShoeStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;

namespace ShoeStore.Controllers
{
    public class AccountController : Controller
    {
        private readonly ShoeStoreContext _context;

        public AccountController(ShoeStoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null || !PasswordMatches(user.PasswordHash, model.Password))
            {
                ViewBag.Error = "Invalid email or password";
                return View(model);
            }

            var claims = BuildClaims(user);
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                new AuthenticationProperties());

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == model.Email);
            if (emailExists)
            {
                ModelState.AddModelError(nameof(model.Email), "Email already exists");
                return View(model);
            }

            const int defaultUserRoleId = 2;
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Id == defaultUserRoleId) 
                ?? await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Users");
            if (customerRole == null)
            {
                ModelState.AddModelError(string.Empty, "Default role 'Users' was not found. Please seed roles first.");
                return View(model);
            }

            var newUser = new Models.db.User
            {
                Fullname = model.FullName,
                Email = model.Email,
                PasswordHash = model.Password,
                RoleId = customerRole.Id
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            ViewBag.Success = "Registration successful. Please login.";
            return View("Login", new LoginViewModel { Email = model.Email });
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email not found");
                return View(model);
            }

            user.PasswordHash = model.NewPassword;
            await _context.SaveChangesAsync();

            ViewBag.Success = "Password changed successfully. Please login.";
            return View("Login", new LoginViewModel { Email = model.Email });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdValue) || !int.TryParse(userIdValue, out var userId))
            {
                return RedirectToAction("Login");
            }

            var profile = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (profile == null)
            {
                return RedirectToAction("Login");
            }

            return View(profile);
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private static bool PasswordMatches(string storedPassword, string providedPassword)
        {
            if (string.IsNullOrEmpty(storedPassword))
            {
                return false;
            }

            return storedPassword == providedPassword;
        }

        private static List<Claim> BuildClaims(Models.db.User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Fullname),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            if (user.Role.RoleName.Contains("Admin", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            if (user.Role.RoleName.Contains("Staff", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Role, "Staff"));
            }

            return claims;
        }
    }
}
