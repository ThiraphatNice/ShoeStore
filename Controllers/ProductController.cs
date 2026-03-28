using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.Services;
using ShoeStore.ViewModels.Home;
using ShoeStore.ViewModels.Product;

namespace ShoeStore.Controllers;

[Authorize]
public class ProductController : Controller
{
    private readonly ShoeStoreContext _context;

    public ProductController(ShoeStoreContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? q)
    {
        var normalizedQuery = q?.Trim();
        var hasQuery = !string.IsNullOrWhiteSpace(normalizedQuery);
        var resultCards = new List<ProductCardViewModel>();

        if (hasQuery)
        {
            var criteria = normalizedQuery!;
            var numericCandidate = new string(criteria.Where(char.IsDigit).ToArray());

            var queryable = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .AsQueryable();

            if (int.TryParse(numericCandidate, out var parsedId))
            {
                queryable = queryable.Where(p =>
                    p.Id == parsedId ||
                    EF.Functions.Like(p.ProductName, $"%{criteria}%") ||
                    (p.Category != null && EF.Functions.Like(p.Category.CategoryName, $"%{criteria}%")));
            }
            else
            {
                queryable = queryable.Where(p =>
                    EF.Functions.Like(p.ProductName, $"%{criteria}%") ||
                    (p.Category != null && EF.Functions.Like(p.Category.CategoryName, $"%{criteria}%")) ||
                    EF.Functions.Like(p.Id.ToString(), $"%{criteria}%"));
            }

            var matches = await queryable
                .OrderByDescending(p => p.CreatedAt ?? DateTime.UtcNow)
                .Take(24)
                .ToListAsync();

            resultCards = matches
                .Select(ProductDisplayMapper.CreateProductCardModel)
                .ToList();
        }

        var suggestions = new List<ProductCardViewModel>();
        if (!hasQuery || resultCards.Count == 0)
        {
            var suggestionPool = await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.ProductVariants)
                .OrderByDescending(p => p.CreatedAt ?? DateTime.UtcNow)
                .Take(12)
                .ToListAsync();

            suggestions = suggestionPool
                .Select(ProductDisplayMapper.CreateProductCardModel)
                .ToList();
        }

        var model = new ProductSearchViewModel
        {
            Query = normalizedQuery,
            Results = resultCards,
            SuggestedProducts = suggestions
        };

        return View(model);
    }

    public IActionResult Details(int id)
    {
        return View();
    }
}
