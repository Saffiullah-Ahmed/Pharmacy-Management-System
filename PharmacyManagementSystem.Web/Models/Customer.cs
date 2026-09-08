using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }

        // Set by the system from the logged-in user's pharmacy.
        public int PharmacyId { get; set; }

        // ================================
        // REQUIRED CUSTOMER INFORMATION
        // ================================

        [Required(ErrorMessage = "Customer name is required.")]
        [StringLength(
            100,
            ErrorMessage = "Customer name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(
            20,
            ErrorMessage = "Phone number cannot exceed 20 characters.")]
        public string Phone { get; set; } = string.Empty;

        // ================================
        // OPTIONAL CUSTOMER INFORMATION
        // ================================

        [StringLength(
            15,
            ErrorMessage = "CNIC cannot exceed 15 characters.")]
        public string? CNIC { get; set; }

        [StringLength(
            200,
            ErrorMessage = "Address cannot exceed 200 characters.")]
        public string? Address { get; set; }

        // ================================
        // SYSTEM INFORMATION
        // ================================

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}