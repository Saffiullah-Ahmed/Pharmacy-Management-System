namespace PharmacyManagementSystem.Web.Models
{
    public class ReturnItemViewModel
    {
        public int SaleItemId { get; set; }

        public int MedicineId { get; set; }

        public string MedicineName { get; set; } = string.Empty;

        public int SoldQuantity { get; set; }

        public int AlreadyReturnedQuantity { get; set; }

        public int RemainingQuantity { get; set; }

        public decimal UnitPrice { get; set; }

        public int ReturnQuantity { get; set; }
    }
}