using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class SaleItem
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }

        // Nullable so an empty medicine selection does not produce:
        // "The value '' is invalid."
        public int? MedicineId { get; set; }

        public string MedicineName { get; set; } = string.Empty;

        // Nullable for safe model binding.
        public int? Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}