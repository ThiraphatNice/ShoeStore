using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.ViewModels.Cart;
using System.Security.Claims;

namespace ShoeStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ShoeStoreContext _context;

        public CartController(ShoeStoreContext context)
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

            var items = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            var model = new CartPageViewModel
            {
                Items = items.Select(MapCartItem).ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CheckProfileStatus()
        {
            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return Unauthorized();
            }

            var status = BuildProfileStatus(user);
            status.ProfileUrl = Url.Action("Profile", "Account");
            return Json(status);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลสินค้าไม่ถูกต้อง" });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var variant = await _context.ProductVariants
                .Include(v => v.Product)
                .FirstOrDefaultAsync(v => v.Id == request.VariantId);

            if (variant == null)
            {
                return Json(new { success = false, message = "ไม่พบสินค้าที่เลือก" });
            }

            var available = variant.StockQuantity ?? 0;
            if (request.Quantity > available)
            {
                return Json(new
                {
                    success = false,
                    message = available <= 0
                        ? "สินค้าหมดสต็อก"
                        : $"คงเหลือ {available} ชิ้น"
                });
            }

            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductVariantId == request.VariantId);

            if (existingItem == null)
            {
                existingItem = new CartItem
                {
                    UserId = userId.Value,
                    ProductVariantId = request.VariantId,
                    Quantity = request.Quantity
                };
                _context.CartItems.Add(existingItem);
            }
            else
            {
                existingItem.Quantity += request.Quantity;
            }

            variant.StockQuantity = available - request.Quantity;

            await _context.SaveChangesAsync();
            await RefreshProductStockTotalAsync(variant.ProductId);

            var totals = await CalculateCartTotalsAsync(userId.Value);

            return Json(new
            {
                success = true,
                item = new
                {
                    cartItemId = existingItem.Id,
                    quantity = existingItem.Quantity,
                    variantStock = variant.StockQuantity ?? 0
                },
                totals
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ครบถ้วน" });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductVariant)
                    .ThenInclude(v => v.Product)
                .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "ไม่พบสินค้าในตะกร้า" });
            }

            var desiredQuantity = Math.Max(1, request.Quantity);
            var delta = desiredQuantity - cartItem.Quantity;
            if (delta == 0)
            {
                var noChangeTotals = await CalculateCartTotalsAsync(userId.Value);
                return Json(new
                {
                    success = true,
                    item = new
                    {
                        cartItemId = cartItem.Id,
                        quantity = cartItem.Quantity,
                        variantStock = cartItem.ProductVariant.StockQuantity ?? 0,
                        lineTotal = CalculateLineTotal(cartItem),
                    },
                    totals = noChangeTotals
                });
            }

            var variant = cartItem.ProductVariant;
            var available = variant.StockQuantity ?? 0;

            if (delta > 0 && available < delta)
            {
                return Json(new
                {
                    success = false,
                    message = available <= 0
                        ? "สินค้าหมดสต็อก"
                        : $"เพิ่มได้สูงสุดอีก {available} ชิ้น"
                });
            }

            variant.StockQuantity = available - delta;
            cartItem.Quantity = desiredQuantity;

            await _context.SaveChangesAsync();
            await RefreshProductStockTotalAsync(variant.ProductId);

            var totals = await CalculateCartTotalsAsync(userId.Value);
            var lineTotal = CalculateLineTotal(cartItem);

            return Json(new
            {
                success = true,
                item = new
                {
                    cartItemId = cartItem.Id,
                    quantity = cartItem.Quantity,
                    variantStock = variant.StockQuantity ?? 0,
                    lineTotal
                },
                totals
            });
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> RemoveItem([FromBody] RemoveCartItemRequest request)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "ข้อมูลไม่ครบถ้วน" });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var cartItem = await _context.CartItems
                .Include(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(ci => ci.Id == request.CartItemId && ci.UserId == userId);

            if (cartItem == null)
            {
                return Json(new { success = false, message = "ไม่พบสินค้าในตะกร้า" });
            }

            var variant = cartItem.ProductVariant;
            variant.StockQuantity = (variant.StockQuantity ?? 0) + cartItem.Quantity;

            _context.CartItems.Remove(cartItem);

            await _context.SaveChangesAsync();
            await RefreshProductStockTotalAsync(variant.ProductId);

            var totals = await CalculateCartTotalsAsync(userId.Value);

            return Json(new
            {
                success = true,
                variantStock = variant.StockQuantity ?? 0,
                totals
            });
        }

        private static CartItemViewModel MapCartItem(CartItem entity)
        {
            var product = entity.ProductVariant.Product;
            return new CartItemViewModel
            {
                CartItemId = entity.Id,
                VariantId = entity.ProductVariantId,
                ProductName = product.ProductName,
                CategoryName = product.Category?.CategoryName ?? string.Empty,
                ImageUrl = product.ImageUrl,
                Size = entity.ProductVariant.Size,
                Color = entity.ProductVariant.Color,
                UnitPrice = product.Price,
                DiscountPercent = product.DiscountPercent ?? 0m,
                IsLimited = product.IsLimited ?? false,
                Quantity = entity.Quantity,
                StockAvailable = entity.ProductVariant.StockQuantity ?? 0
            };
        }

        private async Task<CartTotalsViewModel> CalculateCartTotalsAsync(int userId)
        {
            var aggregated = await _context.CartItems
                .Where(ci => ci.UserId == userId)
                .Select(ci => new
                {
                    ci.Quantity,
                    Price = ci.ProductVariant.Product.Price,
                    Discount = ci.ProductVariant.Product.DiscountPercent ?? 0m
                })
                .ToListAsync();

            var totalItems = aggregated.Sum(item => item.Quantity);
            var totalAmount = aggregated.Sum(item =>
            {
                var finalUnit = item.Price * (1 - item.Discount / 100m);
                return finalUnit * item.Quantity;
            });

            return new CartTotalsViewModel
            {
                TotalItems = totalItems,
                TotalAmount = decimal.Round(totalAmount, 2, MidpointRounding.AwayFromZero)
            };
        }

        private static decimal CalculateLineTotal(CartItem cartItem)
        {
            var product = cartItem.ProductVariant.Product;
            var discount = product.DiscountPercent ?? 0m;
            var finalUnit = product.Price * (1 - discount / 100m);
            return decimal.Round(finalUnit * cartItem.Quantity, 2, MidpointRounding.AwayFromZero);
        }

        private async Task RefreshProductStockTotalAsync(int productId)
        {
            var total = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .SumAsync(v => v.StockQuantity ?? 0);

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.StockTotal = total;
                await _context.SaveChangesAsync();
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return null;
            }

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return null;
            }

            return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        }

        private static ProfileStatusViewModel BuildProfileStatus(User user)
        {
            var missing = new List<string>();
            if (string.IsNullOrWhiteSpace(user.Fullname))
            {
                missing.Add("ชื่อ-นามสกุล");
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                missing.Add("อีเมล");
            }
            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                missing.Add("เบอร์โทร");
            }
            if (string.IsNullOrWhiteSpace(user.Address))
            {
                missing.Add("ที่อยู่");
            }

            return new ProfileStatusViewModel
            {
                IsComplete = missing.Count == 0,
                MissingFields = missing
            };
        }
    }
}
