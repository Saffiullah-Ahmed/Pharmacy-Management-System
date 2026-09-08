using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class RegisterPharmacyViewModel
    {
        // ============================================================
        // PHARMACY INFORMATION
        // ============================================================

        [Required(
            ErrorMessage = "Pharmacy name is required.")]
        [StringLength(
            150,
            ErrorMessage =
                "Pharmacy name cannot exceed 150 characters.")]
        public string PharmacyName { get; set; } = "";


        [Phone(
            ErrorMessage =
                "Please enter a valid phone number.")]
        [StringLength(30)]
        public string Phone { get; set; } = "";


        [StringLength(500)]
        public string Address { get; set; } = "";


        // ============================================================
        // ADMIN ACCOUNT
        // ============================================================

        [Required(
            ErrorMessage =
                "Admin username is required.")]
        [StringLength(
            50,
            MinimumLength = 3,
            ErrorMessage =
                "Username must be between 3 and 50 characters.")]
        public string AdminUsername { get; set; } = "";


        [Required(
            ErrorMessage =
                "Password is required.")]
        [DataType(DataType.Password)]
        [MinLength(
            8,
            ErrorMessage =
                "Password must contain at least 8 characters.")]
        public string Password { get; set; } = "";


        [Required(
            ErrorMessage =
                "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare(
            nameof(Password),
            ErrorMessage =
                "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = "";
    }
}