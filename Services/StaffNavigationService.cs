using ShoeStore.Controllers;
using ShoeStore.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ShoeStore.Services
{
    public static class StaffNavigationService
    {
        private static readonly Dictionary<string, StaffSectionOption> StaffSections = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Staff Stock"] = new StaffSectionOption
            {
                RoleName = "Staff Stock",
                DisplayName = "Stock Control",
                Description = "เธ•เธฃเธงเธเน€เธเนเธเธเธณเธเธงเธเธชเธดเธเธเนเธฒ เธฃเธฑเธเธชเธดเธเธเนเธฒเน€เธเนเธฒ เนเธฅเธฐเธญเธฑเธเน€เธ”เธ•เธเธฅเธฑเธเนเธเธ real-time",
                ActionName = nameof(StaffController.Stock)
            },
            ["Staff Manag"] = new StaffSectionOption
            {
                RoleName = "Staff Manag",
                DisplayName = "Operations Hub",
                Description = "เธ”เธนเนเธฅเธเน€เธเธทเธเธ?เธเธญเธฅเธฐเธกเธนเธขเธเนเธเธขเธญเธเนเธฅเธฐเธเนเธญเธเธเนเธฒเธเธฑเธเธเธต",
                ActionName = nameof(StaffController.ManageUsers)
            },
            ["Staff Sell"] = new StaffSectionOption
            {
                RoleName = "Staff Sell",
                DisplayName = "Sales & Promotions",
                Description = "เธงเธฒเธเนเธเธเนเธเธฃเนเธกเธเธฑเธเนเธฅเธฐเนเธเธกเน€เธเธเธเธฒเธฃเธเธฒเธข",
                ActionName = nameof(StaffController.Sales)
            },
            ["Staff Express"] = new StaffSectionOption
            {
                RoleName = "Staff Express",
                DisplayName = "Express Logistics",
                Description = "เน€เธ•เธฃเธตเธขเธกเนเธเนเธเธชเธดเธเธเนเธฒเนเธฅเธฐเธ”เธนเนเธฅเธเธฒเธฃเธเธฑเธ”เธชเนเธ",
                ActionName = nameof(StaffController.Express)
            }
        };

        public static StaffDashboardViewModel BuildDashboard(ClaimsPrincipal user)
        {
            var viewModel = new StaffDashboardViewModel
            {
                IsAdmin = user.IsInRole("Admin"),
                Sections = GetSectionsFor(user).ToList()
            };

            return viewModel;
        }

        public static IEnumerable<StaffSectionOption> GetSectionsFor(ClaimsPrincipal user)
        {
            if (user.IsInRole("Admin") || user.IsInRole("Staff"))
            {
                return StaffSections.Values;
            }

            var roleNames = user.Claims
                .Where(c => c.Type == ClaimTypes.Role && c.Value.StartsWith("Staff", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Value)
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return roleNames
                .Where(StaffSections.ContainsKey)
                .Select(role => StaffSections[role]);
        }
    }
}


