using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ShoeStore.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult ManageOrders()
        {
            return View();
        }
    }
}