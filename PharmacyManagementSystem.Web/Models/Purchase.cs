namespace PharmacyManagementSystem.Web.Models
{
    public class Purchase
    {
        public int Id { get; set; }

        public string InvoiceNumber { get; set; } = "";

        public string SupplierName { get; set; } = "";

        public DateTime PurchaseDate { get; set; }

        public decimal TotalAmount { get; set; }

        public List<PurchaseItem> Items { get; set; }
            = new List<PurchaseItem>();
    }
}
