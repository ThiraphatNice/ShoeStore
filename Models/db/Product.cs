using System;
using System.Collections.Generic;

namespace ShoeStore.Models.db;

public partial class Product
{
    public int Id { get; set; }

    public int CategoryId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public decimal? DiscountPercent { get; set; }

    public bool? IsLimited { get; set; }

    public int? StockTotal { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
