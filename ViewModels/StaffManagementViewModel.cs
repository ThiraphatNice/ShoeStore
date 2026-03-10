using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace ShoeStore.ViewModels
{
    public class StaffManagementViewModel
    {
        public CreateStaffViewModel NewStaff { get; set; } = new();
        public IEnumerable<SelectListItem> RoleOptions { get; set; } = Array.Empty<SelectListItem>();
        public IEnumerable<StaffSummaryViewModel> ExistingUsers { get; set; } = Array.Empty<StaffSummaryViewModel>();
        public string? StatusMessage { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class StaffSummaryViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
