using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Home;
using ShoeStore.Services;
using System.Security.Claims;
using System.Linq;

namespace ShoeStore.Controllers;

public class HomeController : Controller
{
    private readonly ShoeStoreContext _context;

    public HomeController(ShoeStoreContext context)
    {
        _context = context;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var featuredProducts = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .OrderByDescending(p => p.DiscountPercent ?? 0m)
            .ThenByDescending(p => p.CreatedAt ?? DateTime.UtcNow)
            .Take(6)
            .ToListAsync();

        var limitedProducts = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .Where(p => p.IsLimited == true)
            .OrderByDescending(p => p.CreatedAt ?? DateTime.UtcNow)
            .Take(6)
            .ToListAsync();

        var categorySeeds = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.CategoryName)
            .Take(3)
            .Select(c => new { c.Id, c.CategoryName })
            .ToListAsync();

        var categoryIds = categorySeeds.Select(c => c.Id).ToList();

        var categoryProductsPool = await _context.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.ProductVariants)
            .Where(p => categoryIds.Contains(p.CategoryId))
            .OrderByDescending(p => p.CreatedAt ?? DateTime.UtcNow)
            .ToListAsync();

        var categorySections = categorySeeds
            .Select(seed => new HomeSectionViewModel
            {
                Title = seed.CategoryName,
                Products = categoryProductsPool
                    .Where(p => p.CategoryId == seed.Id)
                    .OrderByDescending(p => p.CreatedAt ?? DateTime.UtcNow)
                    .Take(4)
                    .Select(ProductDisplayMapper.CreateProductCardModel)
                    .ToList()
            })
            .Where(section => section.Products.Any())
            .ToList();

        var model = new HomePageViewModel
        {
            Username = User.Identity?.Name,
            FeaturedProducts = featuredProducts.Select(ProductDisplayMapper.CreateProductCardModel).ToList(),
            LimitedProducts = limitedProducts.Select(ProductDisplayMapper.CreateProductCardModel).ToList(),
            CategorySections = categorySections
        };

        ViewBag.Role = User.FindFirst(ClaimTypes.Role)?.Value;
        return View(model);
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
