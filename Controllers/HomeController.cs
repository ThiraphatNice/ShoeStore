using System.Diagnostics;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Home;
using System.Security.Claims;
using System.Linq;

namespace ShoeStore.Controllers;

public class HomeController : Controller
{
    private readonly ShoeStoreContext _context;
    private static readonly Dictionary<string, string> ProductImageMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Nike Air Force 1"] = "https://images.unsplash.com/photo-1549298916-f52d724204b4?auto=format&fit=crop&w=800&q=80",
        ["Adidas Superstar"] = "https://images.unsplash.com/photo-1519741497674-611481863552?auto=format&fit=crop&w=800&q=80",
        ["Nike Air Zoom Pegasus 40"] = "https://images.unsplash.com/photo-1528701800489-20be3c9f8728?auto=format&fit=crop&w=800&q=80",
        ["Adidas Adilette Comfort"] = "https://images.unsplash.com/photo-1504198453319-5ce911bafcde?auto=format&fit=crop&w=800&q=80",
        ["Nike Victori One Slide"] = "https://images.unsplash.com/photo-1520338471901-0f4d3f3302fb?auto=format&fit=crop&w=800&q=80",
        ["Nike Air Max Excee Women"] = "https://images.unsplash.com/photo-1514986888952-8cd320577b68?auto=format&fit=crop&w=800&q=80",
        ["Adidas Grand Court Women"] = "https://images.unsplash.com/photo-1509042239860-f550ce710b93?auto=format&fit=crop&w=800&q=80",
        ["Clarks Tilden Cap"] = "https://images.unsplash.com/photo-1460353581641-37baddab0fa2?auto=format&fit=crop&w=800&q=80",
        ["Dr. Martens 1460"] = "https://images.unsplash.com/photo-1475180098004-ca77a66827be?auto=format&fit=crop&w=800&q=80",
        ["Nike Dunk Low Panda"] = "https://images.unsplash.com/photo-1483985988355-763728e1935b?auto=format&fit=crop&w=800&q=80"
    };
    private const string DefaultProductImage = "/img/placeholder-shoe.svg";

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
                    .Select(CreateProductCardModel)
                    .ToList()
            })
            .Where(section => section.Products.Any())
            .ToList();

        var model = new HomePageViewModel
        {
            Username = User.Identity?.Name,
            FeaturedProducts = featuredProducts.Select(CreateProductCardModel).ToList(),
            LimitedProducts = limitedProducts.Select(CreateProductCardModel).ToList(),
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

    private static ProductCardViewModel CreateProductCardModel(Product product)
    {
        var variants = product.ProductVariants?
            .OrderBy(v => v.Size)
            .ThenBy(v => v.Color)
            .Select(v => new ProductVariantSummaryViewModel
            {
                VariantId = v.Id,
                Size = v.Size,
                Color = v.Color,
                StockQuantity = v.StockQuantity ?? 0
            })
            .ToList() ?? new List<ProductVariantSummaryViewModel>();

        var resolvedImage = string.IsNullOrWhiteSpace(product.ImageUrl)
            ? ResolveProductImage(product.ProductName)
            : product.ImageUrl!;

        return new ProductCardViewModel
        {
            Id = product.Id,
            Name = product.ProductName,
            Category = product.Category?.CategoryName ?? string.Empty,
            Description = product.Description,
            Price = product.Price,
            DiscountPercent = product.DiscountPercent ?? 0m,
            IsLimited = product.IsLimited ?? false,
            ImageUrl = resolvedImage,
            Variants = variants
        };
    }

    private static string ResolveProductImage(string productName)
    {
        return ProductImageMap.TryGetValue(productName, out var url) ? url : DefaultProductImage;
    }
}
