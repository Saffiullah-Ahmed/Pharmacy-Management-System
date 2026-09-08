using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class CustomerPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int SaleId { get; set; }

        [Required(ErrorMessage = "Payment amount is required.")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; }
            = DateTime.Now;

        // Optional
        [StringLength(
            500,
            ErrorMessage = "Notes cannot exceed 500 characters.")]
        public string? Notes { get; set; }

        // ==========================================
        // DISPLAY INFORMATION
        // ==========================================

        public string CustomerName { get; set; }
            = string.Empty;

        public string InvoiceNumber { get; set; }
            = string.Empty;
    }
}