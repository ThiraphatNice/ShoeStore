using System;
using System.Collections.Generic;

namespace ShoeStore.Models.db;

public partial class Coupon
{
    public int Id { get; set; }

    public string CouponCode { get; set; } = null!;

    public decimal? DiscountPercent { get; set; }

    public decimal? MinPurchase { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
