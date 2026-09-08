using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class SaleReturnItem
    {
        [Key]
        public int Id { get; set; }

        public int ReturnId { get; set; }

        public int SaleItemId { get; set; }

        public int MedicineId { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalRefund { get; set; }

        public string MedicineName { get; set; } = string.Empty;
    }
}