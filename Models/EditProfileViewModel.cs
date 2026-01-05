using System.ComponentModel.DataAnnotations;

namespace FinanceBudgetApp.Models;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email address")]
    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? FullName { get; set; }

    [Display(Name = "Phone Number")]
    [Phone(ErrorMessage = "Invalid phone number")]
    [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Address")]
    [StringLength(500, ErrorMessage = "Address cannot exceed 500 characters")]
    public string? Address { get; set; }
}

