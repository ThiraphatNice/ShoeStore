using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ShoeStore.Models;
using Microsoft.AspNetCore.Authorization;

namespace ShoeStore.Controllers;

public class HomeController : Controller
{
    [Authorize]
    public IActionResult Index()
    {
        ViewBag.Username = User.Identity.Name;
        ViewBag.Role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
