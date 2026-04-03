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
    public class StaffSalesService
    {
        private static readonly CultureInfo ChartCulture = CultureInfo.InvariantCulture;
        private readonly ShoeStoreContext _context;

        public StaffSalesService(ShoeStoreContext context)
        {
            _context = context;
        }

        public async Task<List<CouponRowViewModel>> GetCouponsAsync(CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;

            var coupons = await _context.Coupons
                .AsNoTracking()
                .OrderByDescending(c => c.StartDate ?? c.EndDate ?? DateTime.MinValue)
                .ThenByDescending(c => c.Id)
                .Select(c => new
                {
                    Coupon = c,
                    UsageCount = c.Orders.Count
                })
                .ToListAsync(cancellationToken);

            return coupons.Select(item => MapCoupon(item.Coupon, item.UsageCount, now)).ToList();
        }

        public async Task<CouponRowViewModel> CreateCouponAsync(CouponUpsertRequest request, CancellationToken cancellationToken = default)
        {
            var normalized = NormalizeCouponRequest(request);
            await EnsureCouponCodeUniqueAsync(normalized.CouponCode, null, cancellationToken);
            ValidateCouponWindow(normalized);

            var entity = new Coupon
            {
                CouponCode = normalized.CouponCode,
                DiscountPercent = normalized.DiscountPercent,
                MinPurchase = NormalizeAmount(normalized.MinPurchase),
                StartDate = normalized.StartDate,
                EndDate = normalized.EndDate
            };

            _context.Coupons.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            return await GetCouponRowAsync(entity.Id, cancellationToken);
        }

        public async Task<CouponRowViewModel> UpdateCouponAsync(int id, CouponUpsertRequest request, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Coupons.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
            if (entity == null)
            {
                throw new InvalidOperationException("ไม่พบคูปองที่ต้องการแก้ไข");
            }

            var normalized = NormalizeCouponRequest(request);
            await EnsureCouponCodeUniqueAsync(normalized.CouponCode, id, cancellationToken);
            ValidateCouponWindow(normalized);

            entity.CouponCode = normalized.CouponCode;
            entity.DiscountPercent = normalized.DiscountPercent;
            entity.MinPurchase = NormalizeAmount(normalized.MinPurchase);
            entity.StartDate = normalized.StartDate;
            entity.EndDate = normalized.EndDate;

            await _context.SaveChangesAsync(cancellationToken);

            return await GetCouponRowAsync(entity.Id, cancellationToken);
        }

        public async Task DeleteCouponAsync(int id, CancellationToken cancellationToken = default)
        {
            var entity = await _context.Coupons
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException("ไม่พบคูปองที่ต้องการลบ");
            }

            if (entity.Orders.Any())
            {
                throw new InvalidOperationException("ไม่สามารถลบคูปองที่ถูกใช้งานแล้วได้");
            }

            _context.Coupons.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<SalesSummaryResult> GetSalesSummaryAsync(SalesSummaryQuery query, CancellationToken cancellationToken = default)
        {
            var range = ResolveDateRange(query);

            var orderRows = await _context.Orders
                .AsNoTracking()
                .Where(o => o.CreatedAt >= range.Start && o.CreatedAt < range.End)
                .Where(o => o.Payments.Any(p => p.PaymentStatus == "Paid"))
                .Select(o => new OrderSummaryRow
                {
                    FinalAmount = o.FinalAmount,
                    TotalAmount = o.TotalAmount,
                    DiscountAmount = o.DiscountAmount,
                    CouponId = o.CouponId,
                    CreatedAt = o.CreatedAt
                })
                .ToListAsync(cancellationToken);

            var totalOrders = orderRows.Count;
            var totalRevenue = orderRows.Sum(o => o.FinalAmount ?? o.TotalAmount ?? 0m);
            var discountTotal = orderRows.Sum(o => o.DiscountAmount ?? 0m);
            var couponOrders = orderRows.Count(o => o.CouponId.HasValue);
            var averageOrderValue = totalOrders > 0 ? Math.Round(totalRevenue / totalOrders, 2) : 0m;

            var series = range.Scope == SalesSummaryScope.Monthly
                ? BuildMonthlySeries(orderRows, range.Year, range.Month!.Value)
                : BuildYearlySeries(orderRows, range.Year);

            return new SalesSummaryResult
            {
                Scope = range.Scope,
                Year = range.Year,
                Month = range.Month,
                RangeLabel = range.Scope == SalesSummaryScope.Monthly
                    ? $"{CultureInfo.GetCultureInfo("th-TH").DateTimeFormat.GetMonthName(range.Month!.Value)} {range.Year}"
                    : $"ภาพรวมปี {range.Year}",
                Cards = new SalesSummaryCardsViewModel
                {
                    TotalRevenue = totalRevenue,
                    TotalOrders = totalOrders,
                    AverageOrderValue = averageOrderValue,
                    CouponOrders = couponOrders,
                    DiscountTotal = discountTotal
                },
                Series = series
            };
        }

        public async Task<List<TopProductRowViewModel>> GetTopProductsAsync(SalesSummaryQuery query, int limit = 5, CancellationToken cancellationToken = default)
        {
            var range = ResolveDateRange(query);
            var topLimit = Math.Clamp(limit, 1, 20);

            var grouped = await _context.OrderItems
                .AsNoTracking()
                .Where(oi => oi.Order.CreatedAt >= range.Start && oi.Order.CreatedAt < range.End)
                .Where(oi => oi.Order.Payments.Any(p => p.PaymentStatus == "Paid"))
                .GroupBy(oi => new
                {
                    oi.ProductVariant.ProductId,
                    ProductName = oi.ProductVariant.Product.ProductName,
                    Category = oi.ProductVariant.Product.Category.CategoryName,
                    ImageUrl = oi.ProductVariant.Product.ImageUrl
                })
                .Select(g => new
                {
                    g.Key.ProductId,
                    g.Key.ProductName,
                    g.Key.Category,
                    g.Key.ImageUrl,
                    Quantity = g.Sum(i => i.Quantity),
                    Revenue = g.Sum(i => i.Quantity * i.Price)
                })
                .OrderByDescending(x => x.Quantity)
                .ThenByDescending(x => x.Revenue)
                .Take(topLimit)
                .ToListAsync(cancellationToken);

            var totalQuantity = grouped.Sum(g => g.Quantity);
            var index = 1;

            return grouped.Select(g => new TopProductRowViewModel
            {
                Rank = index++,
                ProductId = g.ProductId,
                ProductName = g.ProductName,
                Category = g.Category,
                ImageUrl = g.ImageUrl,
                QuantitySold = g.Quantity,
                Revenue = Math.Round(g.Revenue, 2),
                SharePercent = totalQuantity == 0 ? 0 : Math.Round((decimal)g.Quantity / totalQuantity * 100, 2)
            }).ToList();
        }

        private async Task<CouponRowViewModel> GetCouponRowAsync(int couponId, CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;

            var row = await _context.Coupons
                .AsNoTracking()
                .Where(c => c.Id == couponId)
                .Select(c => new
                {
                    Coupon = c,
                    UsageCount = c.Orders.Count
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (row == null)
            {
                throw new InvalidOperationException("ไม่สามารถโหลดข้อมูลคูปองได้");
            }

            return MapCoupon(row.Coupon, row.UsageCount, now);
        }

        private static CouponRowViewModel MapCoupon(Coupon coupon, int usageCount, DateTime referenceTime)
        {
            var isUpcoming = coupon.StartDate.HasValue && coupon.StartDate.Value > referenceTime;
            var isExpired = coupon.EndDate.HasValue && coupon.EndDate.Value < referenceTime;
            var isActive = !isUpcoming && !isExpired;
            var status = isExpired
                ? "หมดอายุ"
                : isUpcoming
                    ? "ยังไม่เริ่ม"
                    : "ใช้งานได้";

            return new CouponRowViewModel
            {
                Id = coupon.Id,
                CouponCode = coupon.CouponCode,
                DiscountPercent = coupon.DiscountPercent,
                MinPurchase = coupon.MinPurchase,
                StartDate = coupon.StartDate,
                EndDate = coupon.EndDate,
                UsageCount = usageCount,
                IsActive = isActive,
                StatusLabel = status
            };
        }

        private static void ValidateCouponWindow(CouponUpsertRequest request)
        {
            if (request.StartDate.HasValue && request.EndDate.HasValue &&
                request.StartDate.Value.Date > request.EndDate.Value.Date)
            {
                throw new InvalidOperationException("ช่วงวันที่คูปองไม่ถูกต้อง");
            }
        }

        private static decimal? NormalizeAmount(decimal? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value <= 0 ? null : Math.Round(value.Value, 2);
        }

        private static CouponUpsertRequest NormalizeCouponRequest(CouponUpsertRequest request)
        {
            request.CouponCode = request.CouponCode?.Trim().ToUpperInvariant() ?? string.Empty;
            request.MinPurchase = request.MinPurchase.HasValue ? Math.Round(request.MinPurchase.Value, 2) : null;
            request.StartDate = request.StartDate.HasValue ? DateTime.SpecifyKind(request.StartDate.Value, DateTimeKind.Utc) : null;
            request.EndDate = request.EndDate.HasValue ? DateTime.SpecifyKind(request.EndDate.Value, DateTimeKind.Utc) : null;
            request.DiscountPercent = Math.Round(request.DiscountPercent, 2);
            return request;
        }

        private async Task EnsureCouponCodeUniqueAsync(string couponCode, int? currentId, CancellationToken cancellationToken)
        {
            var exists = await _context.Coupons.AnyAsync(
                c => c.CouponCode == couponCode && (!currentId.HasValue || c.Id != currentId.Value),
                cancellationToken);

            if (exists)
            {
                throw new InvalidOperationException("โค้ดคูปองนี้ถูกใช้งานแล้ว");
            }
        }

        private static SalesSummaryRange ResolveDateRange(SalesSummaryQuery query)
        {
            var now = DateTime.UtcNow;
            var allowedYears = new HashSet<int>(Enumerable.Range(0, 3).Select(offset => now.Year - offset));

            var requestedYear = allowedYears.Contains(query.Year) ? query.Year : now.Year;
            var scope = string.Equals(query.Scope, "yearly", StringComparison.OrdinalIgnoreCase)
                ? SalesSummaryScope.Yearly
                : SalesSummaryScope.Monthly;

            if (scope == SalesSummaryScope.Monthly)
            {
                var requestedMonth = query.Month.HasValue && query.Month.Value is >= 1 and <= 12
                    ? query.Month.Value
                    : now.Month;

                var start = new DateTime(requestedYear, requestedMonth, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddMonths(1);
                return new SalesSummaryRange(start, end, scope, requestedYear, requestedMonth);
            }
            else
            {
                var start = new DateTime(requestedYear, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var end = start.AddYears(1);
                return new SalesSummaryRange(start, end, scope, requestedYear, null);
            }
        }

        private static IEnumerable<SalesTrendPointViewModel> BuildMonthlySeries(IEnumerable<OrderSummaryRow> orders, int year, int month)
        {
            var days = DateTime.DaysInMonth(year, month);
            var lookup = Enumerable.Range(1, days).ToDictionary(
                day => day,
                day => new SalesTrendPointViewModel
                {
                    Label = day.ToString(ChartCulture),
                    OrderCount = 0,
                    Revenue = 0m
                });

            foreach (var o in orders)
            {
                if (o.CreatedAt is DateTime created && created.Month == month && created.Year == year)
                {
                    var key = created.Day;
                    var point = lookup[key];
                    point.OrderCount += 1;
                    point.Revenue += o.FinalAmount ?? o.TotalAmount ?? 0m;
                }
            }

            return lookup
                .OrderBy(pair => pair.Key)
                .Select(pair => pair.Value)
                .ToList();
        }

        private static IEnumerable<SalesTrendPointViewModel> BuildYearlySeries(IEnumerable<OrderSummaryRow> orders, int year)
        {
            var culture = CultureInfo.GetCultureInfo("th-TH");
            var lookup = Enumerable.Range(1, 12).ToDictionary(
                month => month,
                month => new SalesTrendPointViewModel
                {
                    Label = culture.DateTimeFormat.GetAbbreviatedMonthName(month),
                    OrderCount = 0,
                    Revenue = 0m
                });

            foreach (var o in orders)
            {
                if (o.CreatedAt is DateTime created && created.Year == year)
                {
                    var key = created.Month;
                    var point = lookup[key];
                    point.OrderCount += 1;
                    point.Revenue += o.FinalAmount ?? o.TotalAmount ?? 0m;
                }
            }

            return lookup
                .OrderBy(pair => pair.Key)
                .Select(pair => pair.Value)
                .ToList();
        }

        private record SalesSummaryRange(DateTime Start, DateTime End, SalesSummaryScope Scope, int Year, int? Month);

        private class OrderSummaryRow
        {
            public decimal? FinalAmount { get; set; }
            public decimal? TotalAmount { get; set; }
            public decimal? DiscountAmount { get; set; }
            public int? CouponId { get; set; }
            public DateTime? CreatedAt { get; set; }
        }
    }
}
