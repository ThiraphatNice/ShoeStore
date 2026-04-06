using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Services;

namespace ShoeStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            var model = StaffNavigationService.BuildDashboard(User);
            return View("~/Views/Staff/Index.cshtml", model);
        }

        public IActionResult ManageUsers()
        {
            return RedirectToAction(nameof(StaffController.ManageUsers), "Staff");
        }

        public IActionResult ManageProducts()
        {
            return RedirectToAction(nameof(StaffController.Stock), "Staff");
        }

        public IActionResult ManageStaff()
        {
            return RedirectToAction(nameof(StaffController.ManageStaff), "Staff");
        }
    }
}
