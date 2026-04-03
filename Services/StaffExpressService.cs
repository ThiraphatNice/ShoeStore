using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Staff;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShoeStore.Services
{
    public class StaffExpressService
    {
        private static readonly CultureInfo ThaiCulture = new("th-TH");

        private static readonly StatusDefinition[] StatusDefinitions = new[]
        {
            new StatusDefinition("packing", "กำลังเตรียมสินค้า", "warning"),
            new StatusDefinition("delivering", "กำลังส่งสินค้า", "info"),
            new StatusDefinition("done", "ส่งสินค้าแล้ว", "success")
        };

        private readonly ShoeStoreContext _context;

        public StaffExpressService(ShoeStoreContext context)
        {
            _context = context;
        }

        public async Task<ExpressDashboardViewModel> GetDashboardAsync(CancellationToken cancellationToken = default)
        {
            var actionable = await GetShipmentsAsync(includeDelivered: false, limit: 20, cancellationToken);
            var all = await GetShipmentsAsync(includeDelivered: true, limit: 50, cancellationToken);

            return new ExpressDashboardViewModel
            {
                ActionableShipments = actionable,
                AllShipments = all,
                StatusOptions = BuildStatusOptions(),
                Metrics = new ExpressSummaryMetrics
                {
                    PreparingCount = all.Count(s => s.StatusValue == StatusDefinitions[0].Value),
                    InTransitCount = all.Count(s => s.StatusValue == StatusDefinitions[1].Value),
                    DeliveredCount = all.Count(s => s.StatusValue == StatusDefinitions[2].Value)
                }
            };
        }

        public async Task<IReadOnlyList<ExpressShipmentRow>> GetShipmentsSnapshotAsync(bool includeDelivered, CancellationToken cancellationToken = default)
        {
            return await GetShipmentsAsync(includeDelivered, includeDelivered ? 50 : 20, cancellationToken);
        }

        public async Task<ExpressShipmentRow?> UpdateStatusAsync(int shipmentId, string desiredStatus, CancellationToken cancellationToken = default)
        {
            var normalizedStatus = NormalizeStatus(desiredStatus);
            var shipment = await _context.Shipments
                .Include(s => s.Order)
                    .ThenInclude(o => o.User)
                .Include(s => s.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                .FirstOrDefaultAsync(s => s.Id == shipmentId, cancellationToken);

            if (shipment == null)
            {
                return null;
            }

            shipment.ShippingStatus = normalizedStatus;
            if (shipment.Order != null)
            {
                shipment.Order.OrderStatus = normalizedStatus;
            }
            await _context.SaveChangesAsync(cancellationToken);
            return MapShipment(shipment);
        }

        private async Task<IReadOnlyList<ExpressShipmentRow>> GetShipmentsAsync(bool includeDelivered, int limit, CancellationToken cancellationToken)
        {
            var query = _context.Shipments
                .AsNoTracking()
                .Include(s => s.Order)
                    .ThenInclude(o => o.User)
                .Include(s => s.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.ProductVariant)
                            .ThenInclude(pv => pv.Product)
                .OrderByDescending(s => s.Order.CreatedAt ?? DateTime.MinValue);

            var shipments = await query.ToListAsync(cancellationToken);
            shipments = shipments
                .Where(s => includeDelivered || NormalizeStatus(s.ShippingStatus) != StatusDefinitions[2].Value)
                .Take(limit)
                .ToList();

            return shipments.Select(MapShipment).ToList();
        }

        private static ExpressShipmentRow MapShipment(Shipment shipment)
        {
            var order = shipment.Order;
            var user = order.User;
            var items = order.OrderItems;
            var normalizedStatus = NormalizeStatus(shipment.ShippingStatus);
            var definition = GetStatusDefinition(normalizedStatus);

            var summary = string.Join(", ", items
                .Select(i =>
                {
                    var name = i.ProductVariant?.Product?.ProductName ?? "สินค้า";
                    return $"{name} x{i.Quantity}";
                })
                .Take(3));

            var amount = order.FinalAmount ?? order.TotalAmount ?? 0m;

            return new ExpressShipmentRow
            {
                ShipmentId = shipment.Id,
                OrderId = order.Id,
                CustomerName = user?.Fullname ?? "ไม่ทราบชื่อ",
                CustomerEmail = user?.Email ?? "-",
                CustomerPhone = user?.Phone,
                ShippingStatus = definition.Label,
                StatusValue = definition.Value,
                StatusBadgeTheme = definition.Theme,
                TrackingNumber = shipment.TrackingNumber ?? "-",
                ItemsSummary = summary,
                ItemCount = items.Sum(i => i.Quantity),
                FinalAmount = amount,
                AmountLabel = amount.ToString("C2", ThaiCulture),
                CreatedAt = order.CreatedAt,
                CreatedAtLabel = (order.CreatedAt ?? DateTime.UtcNow).ToLocalTime().ToString("dd MMM yyyy, HH:mm", ThaiCulture)
            };
        }

        private static StatusDefinition GetStatusDefinition(string value) =>
            StatusDefinitions.FirstOrDefault(def => def.Value.Equals(value, StringComparison.OrdinalIgnoreCase),
                new StatusDefinition(value, value, "secondary"));

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return StatusDefinitions[0].Value;
            }

            var trimmed = status.Trim();

            var lowered = trimmed.ToLowerInvariant();

            if (lowered is "packing" or "กำลังเตรียมสินค้า")
            {
                return StatusDefinitions[0].Value;
            }

            if (lowered is "intransit" or "กำลังส่งสินค้า" or "delivering")
            {
                return StatusDefinitions[1].Value;
            }

            if (lowered is "delivered" or "done" or "ส่งสินค้าแล้ว")
            {
                return StatusDefinitions[2].Value;
            }

            return StatusDefinitions[0].Value;
        }

        private static IEnumerable<SelectListItem> BuildStatusOptions()
        {
            return StatusDefinitions.Select(status => new SelectListItem
            {
                Text = status.Label,
                Value = status.Value
            }).ToList();
        }

        private record StatusDefinition(string Value, string Label, string Theme);
    }
}
