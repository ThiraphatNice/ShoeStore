using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ShoeStore.Controllers
{
    public class CartController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}