namespace PharmacyManagementSystem.Web.Models
{
    public class PurchaseItem
    {
        public int Id { get; set; }

        public int PurchaseId { get; set; }

        public int MedicineId { get; set; }

        public string MedicineName { get; set; } = "";

        public int Quantity { get; set; }

        public decimal CostPrice { get; set; }

        public decimal TotalPrice { get; set; }
    }
}