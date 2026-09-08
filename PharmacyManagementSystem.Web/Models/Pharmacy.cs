namespace PharmacyManagementSystem.Web.Models
{
    public class Pharmacy
    {
        public int Id { get; set; }

        public string Name { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";

        public DateTime CreatedAt { get; set; }
    }
}