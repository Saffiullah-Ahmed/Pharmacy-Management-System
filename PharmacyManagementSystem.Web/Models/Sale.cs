using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        public int PharmacyId { get; set; }

        [Required]
        public string InvoiceNumber { get; set; } = string.Empty;

        public DateTime SaleDate { get; set; } = DateTime.Now;

        // ============================================================
        // CUSTOMER
        // ============================================================

        public int? CustomerId { get; set; }

        // ============================================================
        // PAYMENT
        // ============================================================

        public decimal TotalAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal NetAmount { get; set; }

        public decimal AmountReceived { get; set; }

        public decimal ChangeAmount { get; set; }

        public decimal BalanceAmount { get; set; }

        public bool GenerateBill { get; set; } = true;

        // ============================================================
        // SALE ITEMS
        // ============================================================

        public List<SaleItem> Items { get; set; }
            = new List<SaleItem>();
    }
}