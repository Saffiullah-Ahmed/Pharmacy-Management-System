namespace PharmacyManagementSystem.Web.Models
{
    public class PharmacySettings
    {
        // ==========================================================
        // PHARMACY ID
        // ==========================================================

        public int PharmacyId { get; set; }


        // ==========================================================
        // PHARMACY INFORMATION
        // ==========================================================

        public string PharmacyName { get; set; } = "";

        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";


        // ==========================================================
        // SYSTEM PREFERENCES
        // ==========================================================

        public string Currency { get; set; } = "Rs.";

        public int LowStockThreshold { get; set; } = 10;

        public string DateFormat { get; set; } =
            "dd/MM/yyyy";
    }
}