using System.ComponentModel.DataAnnotations;

namespace PharmacyManagementSystem.Web.Models
{
    public class SaleReturn
    {
        [Key]
        public int Id { get; set; }

        public int SaleId { get; set; }

        public DateTime ReturnDate { get; set; } = DateTime.Now;

        public decimal TotalRefund { get; set; }

        public string Reason { get; set; } = string.Empty;

        public List<SaleReturnItem> Items { get; set; }
            = new List<SaleReturnItem>();
    }
}