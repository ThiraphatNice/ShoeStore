using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Cart;

namespace ShoeStore.Services
{
    public class CheckoutService
    {
        private readonly ShoeStoreContext _context;

        public CheckoutService(ShoeStoreContext context)
        {
            _context = context;
        }

        public async Task<CouponValidationResult> PreviewTotalsAsync(int userId, string? couponCode)
        {
            var snapshot = await LoadCartSnapshotAsync(userId);
            return await BuildCouponResultAsync(snapshot, couponCode);
        }

        public async Task<CheckoutResponseViewModel> ProcessCheckoutAsync(int userId, string paymentMethod, string? couponCode)
        {
            var snapshot = await LoadCartSnapshotAsync(userId);
            if (!snapshot.Items.Any())
            {
                return CheckoutResponseViewModel.Failure("ยังไม่มีสินค้าในตะกร้า");
            }

            var totals = await BuildCouponResultAsync(snapshot, couponCode);
            if (!totals.IsValid || (totals.HasCoupon && totals.CouponId == null))
            {
                return CheckoutResponseViewModel.Failure(string.IsNullOrWhiteSpace(totals.Message)
                    ? "คูปองไม่ถูกต้อง"
                    : totals.Message);
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            var order = new Order
            {
                UserId = userId,
                TotalAmount = totals.Subtotal,
                DiscountAmount = totals.DiscountAmount,
                FinalAmount = totals.FinalAmount,
                CouponId = totals.CouponId,
                OrderStatus = "packing",
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            foreach (var item in snapshot.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    ProductVariantId = item.VariantId,
                    Quantity = item.Quantity,
                    Price = item.FinalUnitPrice
                };
                _context.OrderItems.Add(orderItem);
            }

            var payment = new Payment
            {
                OrderId = order.Id,
                PaymentMethod = paymentMethod,
                PaymentStatus = "Paid",
                PaidAt = DateTime.UtcNow
            };
            _context.Payments.Add(payment);

            var shipment = new Shipment
            {
                OrderId = order.Id,
                ShippingStatus = "packing",
                TrackingNumber = $"TH{DateTime.UtcNow:yyyyMMddHHmmss}"
            };
            _context.Shipments.Add(shipment);

            _context.CartItems.RemoveRange(snapshot.EntityItems);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return new CheckoutResponseViewModel
            {
                Success = true,
                OrderId = order.Id,
                FinalAmount = totals.FinalAmount,
                PaymentMethod = paymentMethod,
                Message = "ชำระเงินสำเร็จ"
            };
        }

        private async Task<CheckoutCartSnapshot> LoadCartSnapshotAsync(int userId)
        {
            var items = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
                .ToListAsync();

            var snapshotItems = items.Select(ci => new CheckoutCartItem
            {
                CartItemId = ci.Id,
                VariantId = ci.ProductVariantId,
                Quantity = ci.Quantity,
                ProductName = ci.ProductVariant.Product.ProductName,
                Color = ci.ProductVariant.Color,
                Size = ci.ProductVariant.Size,
                ImageUrl = ci.ProductVariant.Product.ImageUrl,
                UnitPrice = ci.ProductVariant.Product.Price,
                DiscountPercent = ci.ProductVariant.Product.DiscountPercent ?? 0m
            }).ToList();

            return new CheckoutCartSnapshot
            {
                UserId = userId,
                Items = snapshotItems,
                EntityItems = items
            };
        }

        private async Task<CouponValidationResult> BuildCouponResultAsync(CheckoutCartSnapshot snapshot, string? couponCode)
        {
            var normalizedCode = string.IsNullOrWhiteSpace(couponCode)
                ? null
                : couponCode.Trim();

            var pricingItems = snapshot.Items.Select(item => new CartPricingCalculator.CartPricingItem
            {
                UnitPrice = item.UnitPrice,
                DiscountPercent = item.DiscountPercent,
                Quantity = item.Quantity
            });

            var baseTotals = CartPricingCalculator.CalculateBaseTotals(pricingItems);
            var subtotal = baseTotals.Subtotal;
            var pairDiscount = baseTotals.PairDiscountAmount;
            var netTotal = baseTotals.NetTotal;

            var result = new CouponValidationResult
            {
                Subtotal = subtotal,
                PairDiscountAmount = pairDiscount,
                CouponDiscountAmount = 0m,
                NetTotal = netTotal,
                FinalAmount = netTotal,
                ShippingFee = 0m,
                DiscountAmount = pairDiscount,
                DiscountPercent = 0m,
                CouponCode = normalizedCode,
                IsValid = true,
                Message = "ยังไม่ได้ใช้คูปอง",
                CouponId = null
            };            if (subtotal <= 0)
            {
                result.Message = "ยังไม่มีสินค้าในตะกร้า";
                RefreshFinalAmounts(result);
                return result;
            }

            if (string.IsNullOrWhiteSpace(normalizedCode))
            {
                RefreshFinalAmounts(result);
                return result;
            }

            var coupon = await _context.Coupons
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.CouponCode == normalizedCode);

            if (coupon == null)
            {
                result.IsValid = false;
                result.Message = "ไม่พบคูปองนี้";
                RefreshFinalAmounts(result);
                return result;
            }

            var now = DateTime.UtcNow;
            if (coupon.StartDate.HasValue && coupon.StartDate.Value > now)
            {
                result.IsValid = false;
                result.Message = "คูปองยังไม่เริ่มใช้งาน";
                RefreshFinalAmounts(result);
                return result;
            }

            if (coupon.EndDate.HasValue && coupon.EndDate.Value < now)
            {
                result.IsValid = false;
                result.Message = "คูปองหมดอายุแล้ว";
                RefreshFinalAmounts(result);
                return result;
            }

            var minPurchase = coupon.MinPurchase ?? 0m;
            if (subtotal < minPurchase)
            {
                result.IsValid = false;
                result.Message = $"ยอดขั้นต่ำ {minPurchase:N0} บาท";
                RefreshFinalAmounts(result);
                return result;
            }

            var discountPercent = coupon.DiscountPercent ?? 0m;
            var discountAmount = decimal.Round(subtotal * discountPercent / 100m, 2, MidpointRounding.AwayFromZero);

            result.IsValid = true;
            result.CouponId = coupon.Id;
            result.DiscountPercent = discountPercent;
            result.CouponDiscountAmount = discountAmount;
            RefreshFinalAmounts(result);
            result.Message = $"ใช้คูปองลด {discountPercent:N0}%";

            return result;
        }

        private sealed class CheckoutCartSnapshot
        {
            public int UserId { get; set; }

            public List<CheckoutCartItem> Items { get; set; } = new();

            public List<CartItem> EntityItems { get; set; } = new();
        }

        private sealed class CheckoutCartItem
        {
            public int CartItemId { get; set; }

            public int VariantId { get; set; }

            public string ProductName { get; set; } = string.Empty;

            public string? Color { get; set; }

            public string? Size { get; set; }

            public string? ImageUrl { get; set; }

            public decimal UnitPrice { get; set; }

            public decimal DiscountPercent { get; set; }

            public int Quantity { get; set; }

            public decimal FinalUnitPrice => decimal.Round(UnitPrice * (1 - DiscountPercent / 100m), 2, MidpointRounding.AwayFromZero);

            public decimal LineTotal => decimal.Round(FinalUnitPrice * Quantity, 2, MidpointRounding.AwayFromZero);
        }

        private static void RefreshFinalAmounts(CouponValidationResult result)
        {
            result.DiscountAmount = decimal.Round(result.PairDiscountAmount + result.CouponDiscountAmount, 2, MidpointRounding.AwayFromZero);
            result.NetTotal = Math.Max(0m, decimal.Round(result.Subtotal - result.DiscountAmount, 2, MidpointRounding.AwayFromZero));
            result.ShippingFee = CartPricingCalculator.CalculateShippingFee(result.NetTotal);
            result.FinalAmount = decimal.Round(result.NetTotal + result.ShippingFee, 2, MidpointRounding.AwayFromZero);
        }
    }
}






