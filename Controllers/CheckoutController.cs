using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoeStore.Models.db;
using ShoeStore.Services;
using ShoeStore.ViewModels.Cart;

namespace ShoeStore.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ShoeStoreContext _context;
        private readonly CheckoutService _checkoutService;

        public CheckoutController(ShoeStoreContext context, CheckoutService checkoutService)
        {
            _context = context;
            _checkoutService = checkoutService;
        }

        [HttpGet]
        public async Task<IActionResult> ValidateCoupon(string? code)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _checkoutService.PreviewTotalsAsync(userId.Value, code);
            return Json(result);
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SubmitPayment([FromBody] CheckoutRequest request)
        {
            if (IsInternalPurchaseRestricted())
            {
                return Json(new { success = false, message = "บัญชีแอดมินและพนักงานไม่สามารถทำรายการสั่งซื้อได้" });
            }

            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return Unauthorized();
            }

            var profileStatus = BuildProfileStatus(user);
            if (!profileStatus.IsComplete)
            {
                return Json(new
                {
                    success = false,
                    reason = "profile",
                    profileStatus.MissingFields,
                    profileUrl = Url.Action("Profile", "Account")
                });
            }

            var paymentMethod = request.PaymentMethod?.Trim();
            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                return Json(new { success = false, message = "กรุณาเลือกวิธีชำระเงิน" });
            }

            if (paymentMethod.Equals("Credit Card", StringComparison.OrdinalIgnoreCase))
            {
                if (request.CreditCard == null)
                {
                    return Json(new { success = false, message = "กรอกข้อมูลบัตรเครดิตให้ครบ" });
                }

                if (!TryValidateModel(request.CreditCard, nameof(request.CreditCard)))
                {
                    return Json(new { success = false, message = "ข้อมูลบัตรเครดิตไม่ถูกต้อง" });
                }
            }
            else if (paymentMethod.Equals("PromptPay", StringComparison.OrdinalIgnoreCase))
            {
                if (!request.PromptPayConfirmed)
                {
                    return Json(new { success = false, message = "กรุณายืนยันการชำระเงิน PromptPay" });
                }
            }
            else
            {
                return Json(new { success = false, message = "วิธีชำระเงินไม่รองรับ" });
            }

            var checkoutResult = await _checkoutService.ProcessCheckoutAsync(userId.Value, paymentMethod, request.CouponCode);
            if (!checkoutResult.Success)
            {
                return Json(new { success = false, message = checkoutResult.Message });
            }

            return Json(new
            {
                success = true,
                orderId = checkoutResult.OrderId,
                paymentMethod = checkoutResult.PaymentMethod,
                finalAmount = checkoutResult.FinalAmount,
                finalAmountDisplay = checkoutResult.FinalAmountDisplay,
                historyUrl = Url.Action("Index", "Order")
            });
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

        private bool IsInternalPurchaseRestricted()
        {
            if (User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            if (User.IsInRole("Admin") || User.IsInRole("Staff"))
            {
                return true;
            }

            return User.Claims.Any(c =>
                c.Type == ClaimTypes.Role &&
                c.Value.StartsWith("Staff", StringComparison.OrdinalIgnoreCase));
        }
    }
}
