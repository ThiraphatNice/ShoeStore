using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Order;

namespace ShoeStore.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ShoeStoreContext _context;

        private static readonly (string Status, string Label, string Icon)[] TimelineStages =
        {
            ("packing", "เตรียมห่อสินค้า", "bi-box-seam"),
            ("delivering", "กำลังจัดส่ง", "bi-truck"),
            ("done", "จัดส่งสำเร็จ", "bi-check-circle")
        };

        public OrderController(ShoeStoreContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await LoadOrderSummariesAsync(userId.Value);
            var model = new OrderHistoryPageViewModel
            {
                Orders = orders
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var detail = await BuildOrderDetailAsync(userId.Value, id);
            if (detail == null)
            {
                return NotFound();
            }

            return View(detail);
        }

        [HttpGet]
        public async Task<IActionResult> DetailsData(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var detail = await BuildOrderDetailAsync(userId.Value, id);
            if (detail == null)
            {
                return NotFound();
            }

            return Json(detail);
        }

        private async Task<List<OrderSummaryViewModel>> LoadOrderSummariesAsync(int userId)
        {
            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.Payments)
                .Include(o => o.Coupon)
                .Include(o => o.Shipments)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders.Select(MapOrderSummary).ToList();
        }

        private async Task<OrderDetailViewModel?> BuildOrderDetailAsync(int userId, int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(v => v.Product)
                .Include(o => o.Payments)
                .Include(o => o.Coupon)
                .Include(o => o.Shipments)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return null;
            }

            var summary = MapOrderSummary(order);
            return new OrderDetailViewModel
            {
                OrderId = summary.OrderId,
                CreatedAt = summary.CreatedAt,
                PaymentMethod = summary.PaymentMethod,
                ShippingStatus = summary.ShippingStatus,
                FinalAmount = summary.FinalAmount,
                CouponCode = summary.CouponCode,
                DiscountPercent = summary.DiscountPercent,
                Items = summary.Items,
                CustomerName = order.User.Fullname,
                CustomerEmail = order.User.Email,
                CustomerPhone = order.User.Phone,
                CustomerAddress = order.User.Address,
                Timeline = BuildTimeline(summary.ShippingStatus, order.CreatedAt)
            };
        }

        private OrderSummaryViewModel MapOrderSummary(Models.db.Order order)
        {
            var paymentMethod = order.Payments
                .OrderByDescending(p => p.PaidAt)
                .FirstOrDefault()
                ?.PaymentMethod ?? "Credit Card";

            var shippingStatus = ResolveShippingStatus(order);

            var items = order.OrderItems.Select(oi => new OrderProductSummaryViewModel
            {
                ProductName = oi.ProductVariant.Product.ProductName,
                Color = oi.ProductVariant.Color,
                Size = oi.ProductVariant.Size,
                Quantity = oi.Quantity,
                LineTotal = decimal.Round(oi.Price * oi.Quantity, 2, MidpointRounding.AwayFromZero),
                ImageUrl = oi.ProductVariant.Product.ImageUrl
            }).ToList();

            return new OrderSummaryViewModel
            {
                OrderId = order.Id,
                CreatedAt = order.CreatedAt,
                PaymentMethod = paymentMethod,
                ShippingStatus = shippingStatus,
                FinalAmount = order.FinalAmount.GetValueOrDefault(),
                CouponCode = order.Coupon?.CouponCode,
                DiscountPercent = order.Coupon?.DiscountPercent,
                Items = items
            };
        }

        private string ResolveShippingStatus(Models.db.Order order)
        {
            var latestShipment = order.Shipments
                .OrderByDescending(s => s.Id)
                .FirstOrDefault();

            var status = latestShipment?.ShippingStatus ?? order.OrderStatus ?? "packing";
            return status.ToLowerInvariant();
        }

        private List<ShipmentTimelineItem> BuildTimeline(string currentStatus, DateTime? createdAt)
        {
            var currentIndex = Array.FindIndex(TimelineStages, stage =>
                string.Equals(stage.Status, currentStatus, StringComparison.OrdinalIgnoreCase));

            if (currentIndex < 0)
            {
                currentIndex = 0;
            }

            var result = new List<ShipmentTimelineItem>();
            for (var i = 0; i < TimelineStages.Length; i++)
            {
                var stage = TimelineStages[i];
                result.Add(new ShipmentTimelineItem
                {
                    Status = stage.Status,
                    Label = stage.Label,
                    Icon = stage.Icon,
                    IsActive = i <= currentIndex,
                    Timestamp = i == 0 ? createdAt : null
                });
            }

            return result;
        }

        private int? GetCurrentUserId()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdValue))
            {
                return null;
            }

            return int.TryParse(userIdValue, out var userId) ? userId : null;
        }
    }
}
