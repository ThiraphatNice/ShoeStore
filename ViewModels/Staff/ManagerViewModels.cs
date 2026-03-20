using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels.Staff
{
    public class StaffManagerPageViewModel
    {
        public IEnumerable<SelectListItem> RoleOptions { get; set; } = new List<SelectListItem>();
    }

    public class ManagedUserRowViewModel
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class UpdateManagedUserRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Password { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class DeleteManagedUserRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public string ManagerPassword { get; set; } = string.Empty;
    }
}
