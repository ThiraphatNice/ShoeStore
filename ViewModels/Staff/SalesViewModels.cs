using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels.Staff
{
    public class SalesDashboardViewModel
    {
        public IEnumerable<SelectListItem> YearOptions { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> MonthOptions { get; set; } = new List<SelectListItem>();
        public string DefaultScope { get; set; } = "monthly";
        public int DefaultYear { get; set; } = DateTime.UtcNow.Year;
        public int DefaultMonth { get; set; } = DateTime.UtcNow.Month;
    }

    public class CouponRowViewModel
    {
        public int Id { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public decimal? DiscountPercent { get; set; }
        public decimal? MinPurchase { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int UsageCount { get; set; }
        public bool IsActive { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
    }

    public class CouponUpsertRequest
    {
        [Required]
        [MaxLength(40)]
        [RegularExpression("^[A-Za-z0-9_-]+$", ErrorMessage = "Coupon code must be alphanumeric.")]
        public string CouponCode { get; set; } = string.Empty;

        [Range(0, 100)]
        public decimal DiscountPercent { get; set; }

        [Range(0, 1000000)]
        public decimal? MinPurchase { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CouponDeleteRequest
    {
        [Required]
        public int CouponId { get; set; }
    }

    public enum SalesSummaryScope
    {
        Monthly,
        Yearly
    }

    public class SalesSummaryQuery
    {
        public string Scope { get; set; } = "monthly";

        [Range(2000, 2100)]
        public int Year { get; set; } = DateTime.UtcNow.Year;

        [Range(1, 12)]
        public int? Month { get; set; } = DateTime.UtcNow.Month;
    }

    public class SalesSummaryCardsViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public decimal AverageOrderValue { get; set; }
        public int CouponOrders { get; set; }
        public decimal DiscountTotal { get; set; }
    }

    public class SalesTrendPointViewModel
    {
        public string Label { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
    }

    public class SalesSummaryResult
    {
        public SalesSummaryScope Scope { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
        public string RangeLabel { get; set; } = string.Empty;
        public SalesSummaryCardsViewModel Cards { get; set; } = new();
        public IEnumerable<SalesTrendPointViewModel> Series { get; set; } = new List<SalesTrendPointViewModel>();
    }

    public class TopProductRowViewModel
    {
        public int Rank { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? ImageUrl { get; set; }
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal SharePercent { get; set; }
    }
}
