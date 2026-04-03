using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels.Cart
{
    public class ProfileStatusViewModel
    {
        public bool IsComplete { get; set; }

        public List<string> MissingFields { get; set; } = new();

        public string? ProfileUrl { get; set; }
    }

    public class CouponValidationResult
    {
        public bool IsValid { get; set; }

        public bool HasCoupon => !string.IsNullOrWhiteSpace(CouponCode);

        public string? CouponCode { get; set; }

        public decimal Subtotal { get; set; }

        public decimal DiscountPercent { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal FinalAmount { get; set; }

        public string FinalAmountDisplay => $"{FinalAmount:N0} บาท";

        public string SubtotalDisplay => $"{Subtotal:N0} บาท";

        public string DiscountDisplay => $"-{DiscountAmount:N0} บาท";

        public string Message { get; set; } = string.Empty;

        public int? CouponId { get; set; }
    }

    public class CheckoutRequest
    {
        [Required]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? CouponCode { get; set; }

        public CreditCardInputModel? CreditCard { get; set; }

        public bool PromptPayConfirmed { get; set; }
    }

    public class CreditCardInputModel
    {
        [Required]
        [StringLength(16, MinimumLength = 16)]
        [RegularExpression("^[0-9]{16}$")]
        public string CardNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        public string CardholderName { get; set; } = string.Empty;

        [Required]
        public string ExpiryMonth { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[0-9]{4}$")]
        public string ExpiryYear { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^[0-9]{3}$")]
        public string Cvv { get; set; } = string.Empty;
    }

    public class CheckoutResponseViewModel
    {
        public bool Success { get; set; }

        public int OrderId { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public decimal FinalAmount { get; set; }

        public string Message { get; set; } = string.Empty;

        public string FinalAmountDisplay => $"{FinalAmount:N0} บาท";

        public static CheckoutResponseViewModel Failure(string message) => new()
        {
            Success = false,
            Message = message
        };
    }
}
