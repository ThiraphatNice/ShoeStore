using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels.Stock
{
    public class StockPageViewModel
    {
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
    }

    public class UpdateProductRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class UpdateVariantStockRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        public string Size { get; set; } = string.Empty;

        [Required]
        public string Color { get; set; } = string.Empty;

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class AddVariantRequest
    {
        [Required]
        public int ProductId { get; set; }

        [Required]
        [RegularExpression(@"^\d{1,4}$", ErrorMessage = "Size must contain digits only.")]
        public string Size { get; set; } = string.Empty;

        [Required]
        public string Color { get; set; } = string.Empty;
    }

    public class CreateProductRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Required]
        public int CategoryId { get; set; }
    }

    public class ProductDetailResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public List<ProductVariantViewModel> Variants { get; set; } = new();
    }

    public class ProductVariantViewModel
    {
        public int Id { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public int StockQuantity { get; set; }
    }

    public class InventoryRowViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal DiscountPercent { get; set; }
        public int? StockTotal { get; set; }
        public bool? IsLimited { get; set; }
    }
}
