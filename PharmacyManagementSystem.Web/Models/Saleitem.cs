using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class SaleItem
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }


        // ============================================================
        // MEDICINE
        // ============================================================

        // Nullable so an empty medicine selection does not produce:
        // "The value '' is invalid."
        public int? MedicineId { get; set; }

        public string MedicineName { get; set; }
            = string.Empty;


        // ============================================================
        // QUANTITY
        // ============================================================

        // Nullable for safe model binding.
        public int? Quantity { get; set; }


        // ============================================================
        // PRICE
        // ============================================================

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}