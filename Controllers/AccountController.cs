using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using ShoeStore.Models;
using System.Collections.Generic;
using ShoeStore.ViewModels;

namespace ShoeStore.Controllers
{
    public class AccountController : Controller
    {
        // Mock data - in real app, this would be from database
        private static List<User> users = new List<User>
        {
            new User { Username = "customer1", Password = "123", Email = "customer1@example.com", Role = "Customer" },
            new User { Username = "staff1", Password = "123", Email = "staff1@example.com", Role = "Staff" },
            new User { Username = "admin", Password = "123", Email = "admin@example.com", Role = "Admin" }
        };

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

            var user = users.FirstOrDefault(u => u.Username == model.Username && u.Password == model.Password);
            if (user != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties();

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Invalid username or password";
                return View();
            }
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match");
                return View(model);
            }

            if (users.Any(u => u.Username == model.Username))
            {
                ModelState.AddModelError("", "Username already exists");
                return View(model);
            }

            users.Add(new User { Username = model.Username, Password = model.Password, Email = model.Email, Role = "Customer" });
            ViewBag.Success = "Registration successful. Please login.";
            return View("Login");
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ForgetPassword(ForgetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = users.FirstOrDefault(u => u.Username == model.Username && u.Email == model.Email);
            if (user != null)
            {
                user.Password = model.NewPassword;
                ViewBag.Success = "Password changed successfully. Please login.";
                return View("Login");
            }
            else
            {
                ModelState.AddModelError("", "Username and email do not match");
                return View(model);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}