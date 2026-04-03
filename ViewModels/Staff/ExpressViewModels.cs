using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels.Staff
{
    public class ExpressDashboardViewModel
    {
        public IReadOnlyList<ExpressShipmentRow> ActionableShipments { get; set; } = Array.Empty<ExpressShipmentRow>();

        public IReadOnlyList<ExpressShipmentRow> AllShipments { get; set; } = Array.Empty<ExpressShipmentRow>();

        public IEnumerable<SelectListItem> StatusOptions { get; set; } = Array.Empty<SelectListItem>();

        public ExpressSummaryMetrics Metrics { get; set; } = new();
    }

    public class ExpressSummaryMetrics
    {
        public int PreparingCount { get; set; }
        public int InTransitCount { get; set; }
        public int DeliveredCount { get; set; }
    }

    public class ExpressShipmentRow
    {
        public int ShipmentId { get; set; }
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string ShippingStatus { get; set; } = string.Empty;
        public string StatusValue { get; set; } = string.Empty;
        public string StatusBadgeTheme { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string ItemsSummary { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal FinalAmount { get; set; }
        public string AmountLabel { get; set; } = string.Empty;
        public string CreatedAtLabel { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }

    public class ExpressStatusUpdateRequest
    {
        [Required]
        public int ShipmentId { get; set; }

        [Required]
        [StringLength(100)]
        public string NewStatus { get; set; } = string.Empty;
    }
}
