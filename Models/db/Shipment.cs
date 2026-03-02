using System;
using System.Collections.Generic;

namespace ShoeStore.Models.db;

public partial class Shipment
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string? TrackingNumber { get; set; }

    public string? ShippingStatus { get; set; }

    public virtual Order Order { get; set; } = null!;
}
