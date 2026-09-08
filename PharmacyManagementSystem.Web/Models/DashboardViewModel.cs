namespace PharmacyManagementSystem.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalMedicines { get; set; }

        public int LowStockMedicines { get; set; }

        public int ExpiringSoonMedicines { get; set; }

        public decimal TodaySales { get; set; }

        public int TotalSales { get; set; }
    }
}
