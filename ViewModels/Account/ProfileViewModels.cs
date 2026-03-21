using System.ComponentModel.DataAnnotations;

namespace ShoeStore.ViewModels.Account
{
    public class ProfilePageViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public ProfileEditViewModel EditForm { get; set; } = new();
    }

    public class ProfileEditViewModel
    {
        [Required]
        [Display(Name = "Full name")]
        [StringLength(100, MinimumLength = 2)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "Email address")]
        public string Email { get; set; } = string.Empty;

        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Password confirmation does not match.")]
        public string? ConfirmPassword { get; set; }

        [Display(Name = "Phone number")]
        [RegularExpression(@"^\d{7,15}$", ErrorMessage = "Phone number must contain 7-15 digits.")]
        public string? Phone { get; set; }

        [StringLength(200)]
        [Display(Name = "Address")]
        public string? Address { get; set; }
    }
}
