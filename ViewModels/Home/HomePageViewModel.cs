using System.Collections.Generic;
using System.Linq;

namespace ShoeStore.ViewModels.Home
{
    public class HomePageViewModel
    {
        public string? Username { get; set; }
        public List<ProductCardViewModel> FeaturedProducts { get; set; } = new();
        public List<ProductCardViewModel> LimitedProducts { get; set; } = new();
        public List<HomeSectionViewModel> CategorySections { get; set; } = new();
    }

    public class HomeSectionViewModel
    {
        public string Title { get; set; } = string.Empty;
        public List<ProductCardViewModel> Products { get; set; } = new();
    }

    public class ProductCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public bool IsLimited { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public List<ProductVariantSummaryViewModel> Variants { get; set; } = new();

        public decimal FinalPrice => DiscountPercent > 0 ? Price * (1 - DiscountPercent / 100m) : Price;
        public string PriceDisplay => $"{Price:N0}.-";
        public string FinalPriceDisplay => $"{FinalPrice:N0}.-";
        public string Sku => $"TS-{Id:000}";
        public string PrimaryColor => Variants.FirstOrDefault()?.Color ?? "-";
        public int TotalStock => Variants.Sum(v => v.StockQuantity);
        public IEnumerable<string> SizeOptions => Variants
            .Select(v => v.Size ?? string.Empty)
            .Where(size => !string.IsNullOrWhiteSpace(size))
            .Distinct();
        public IEnumerable<string> ColorOptions => Variants
            .Select(v => v.Color ?? string.Empty)
            .Where(color => !string.IsNullOrWhiteSpace(color))
            .Distinct();
    }

    public class ProductVariantSummaryViewModel
    {
        public int VariantId { get; set; }

        public string? Size { get; set; }
        public string? Color { get; set; }
        public int StockQuantity { get; set; }
    }
}


