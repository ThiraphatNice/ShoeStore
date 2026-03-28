using System;
using System.Collections.Generic;
using System.Linq;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Home;

namespace ShoeStore.Services
{
    public static class ProductDisplayMapper
    {
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

        public static ProductCardViewModel CreateProductCardModel(Product product)
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
}
