using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace ShoeStore.ViewModels.Order
{
    public class OrderHistoryPageViewModel
    {
        public List<OrderSummaryViewModel> Orders { get; set; } = new();

        public int ItemsShipping => Orders.Count(o => !string.Equals(o.ShippingStatus, "done", StringComparison.OrdinalIgnoreCase));

        public int ItemsCompleted => Orders.Count(o => string.Equals(o.ShippingStatus, "done", StringComparison.OrdinalIgnoreCase));

        public bool HasOrders => Orders.Any();
    }

    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }

        public DateTime? CreatedAt { get; set; }

        public string PaymentMethod { get; set; } = string.Empty;

        public string ShippingStatus { get; set; } = "packing";

        public decimal FinalAmount { get; set; }

        public string? CouponCode { get; set; }

        public decimal? DiscountPercent { get; set; }

        public List<OrderProductSummaryViewModel> Items { get; set; } = new();

        public string OrderNumber => $"ORD-{OrderId:000000}";

        public string CreatedAtDisplay => CreatedAt?.ToString("dd MMM yyyy HH:mm", new CultureInfo("th-TH")) ?? "-";

        public string FinalAmountDisplay => $"{FinalAmount:N0} บาท";

        public string ShippingBadgeClass => ShippingStatus?.ToLowerInvariant() switch
        {
            "packing" => "bg-warning-subtle text-warning-emphasis",
            "delivering" => "bg-info-subtle text-info-emphasis",
            "done" => "bg-success-subtle text-success-emphasis",
            _ => "bg-secondary-subtle text-secondary-emphasis"
        };
    }

    public class OrderProductSummaryViewModel
    {
        public string ProductName { get; set; } = string.Empty;

        public string? Color { get; set; }

        public string? Size { get; set; }

        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }

        public string? ImageUrl { get; set; }

        public string DisplayLine => $"{ProductName} x {Quantity}";

        public string LineTotalDisplay => $"{LineTotal:N0} บาท";
    }

    public class OrderDetailViewModel : OrderSummaryViewModel
    {
        public string? CustomerName { get; set; }

        public string? CustomerEmail { get; set; }

        public string? CustomerPhone { get; set; }

        public string? CustomerAddress { get; set; }

        public List<ShipmentTimelineItem> Timeline { get; set; } = new();
    }

    public class ShipmentTimelineItem
    {
        public string Status { get; set; } = string.Empty;

        public string Label { get; set; } = string.Empty;

        public string Icon { get; set; } = "bi-box-seam";

        public DateTime? Timestamp { get; set; }

        public bool IsActive { get; set; }

        public string TimestampDisplay => Timestamp?.ToString("dd MMM yyyy HH:mm", new CultureInfo("th-TH")) ?? "รอดำเนินการ";
    }
}
