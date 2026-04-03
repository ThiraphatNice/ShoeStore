using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ShoeStore.ViewModels.Cart
{
    public class CartPageViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();

        public int TotalItems => Items.Sum(item => item.Quantity);

        public decimal TotalAmount => Items.Sum(item => item.LineTotal);

        public string TotalAmountDisplay => $"{TotalAmount:N0} บาท";

        public IReadOnlyList<string> PaymentOptions { get; } = new[] { "Credit Card", "PromptPay" };

        public bool HasItems => Items.Any();
    }

    public class CartItemViewModel
    {
        public int CartItemId { get; set; }

        public int VariantId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public string CategoryName { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }

        public string? Size { get; set; }

        public string? Color { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal DiscountPercent { get; set; }

        public bool IsLimited { get; set; }

        public int Quantity { get; set; }

        public int StockAvailable { get; set; }

        public decimal FinalUnitPrice => DiscountPercent > 0
            ? UnitPrice * (1 - DiscountPercent / 100m)
            : UnitPrice;

        public decimal LineTotal => FinalUnitPrice * Quantity;

        public string PriceDisplay => $"{FinalUnitPrice:N0}.-";

        public string OriginalPriceDisplay => DiscountPercent > 0 ? $"{UnitPrice:N0}.-" : string.Empty;

        public string ImageOrPlaceholder => string.IsNullOrWhiteSpace(ImageUrl)
            ? "/img/placeholder-shoe.svg"
            : ImageUrl!;
    }

    public class CartTotalsViewModel
    {
        public int TotalItems { get; set; }

        public decimal TotalAmount { get; set; }

        public string TotalAmountDisplay => $"{TotalAmount:N0} บาท";
    }

    public class AddCartItemRequest
    {
        [Required]
        public int VariantId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class UpdateCartItemRequest
    {
        [Required]
        public int CartItemId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
    }

    public class RemoveCartItemRequest
    {
        [Required]
        public int CartItemId { get; set; }
    }
}

