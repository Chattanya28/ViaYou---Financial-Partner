using System;
using System.ComponentModel.DataAnnotations;

namespace ViaYou.ViewModels
{
    public class ProfileViewModel
    {
        // Display Info
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public DateTime MemberSince { get; set; }
        public int SecurityScore { get; set; }
        public string? ProfilePictureBase64 { get; set; }
        public string Initial => !string.IsNullOrEmpty(FullName) ? FullName[0].ToString().ToUpper() : (Email?[0].ToString().ToUpper() ?? "U");
        
        public bool EmailConfirmed { get; set; }
        public string KycStatus { get; set; } = "Pending";
        public string LanguagePreference { get; set; } = "English";
        public bool LoginAlertsEnabled { get; set; }
        public string? PhoneNumber { get; set; }

        // Forms
        public EditProfileForm EditProfile { get; set; } = new EditProfileForm();
        public ChangePasswordForm ChangePassword { get; set; } = new ChangePasswordForm();
    }

    public class EditProfileForm
    {
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid Phone Number")]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        public string LanguagePreference { get; set; } = "English";
        public bool LoginAlertsEnabled { get; set; } = true;
    }

    public class ChangePasswordForm
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; } = string.Empty;

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
