using System;
using System.Collections.Generic;

namespace ShoeStore.Models.db;

public partial class Payment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? PaymentMethod { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? PaidAt { get; set; }

    public virtual Order Order { get; set; } = null!;
}
