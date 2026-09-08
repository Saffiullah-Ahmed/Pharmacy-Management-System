using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmacyManagementSystem.Web.Authorization;
using PharmacyManagementSystem.Web.DataStorage;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class ReportsController : PharmacyControllerBase
    {
        // ==========================================================
        // REPORT DASHBOARD
        // ==========================================================

        [HttpGet]
        public IActionResult Index(
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!PermissionChecker.HasPermission(User, "Reports"))
                return Forbid();

            IActionResult? pharmacyCheck =
                RequirePharmacy();

            if (pharmacyCheck != null)
                return pharmacyCheck;

            int pharmacyId =
                CurrentPharmacyId;

            DateTime selectedFromDate;
            DateTime selectedToDate;

            // Default:
            // Current month from the 1st until today.
            if (!fromDate.HasValue &&
                !toDate.HasValue)
            {
                selectedFromDate =
                    new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1);

                selectedToDate =
                    DateTime.Today;
            }
            else
            {
                selectedFromDate =
                    fromDate?.Date
                    ?? new DateTime(
                        DateTime.Today.Year,
                        DateTime.Today.Month,
                        1);

                selectedToDate =
                    toDate?.Date
                    ?? DateTime.Today;
            }

            // Prevent invalid date ranges.
            if (selectedFromDate > selectedToDate)
            {
                DateTime temp =
                    selectedFromDate;

                selectedFromDate =
                    selectedToDate;

                selectedToDate =
                    temp;
            }

            // Do not allow future report dates.
            if (selectedFromDate > DateTime.Today)
            {
                selectedFromDate =
                    DateTime.Today;
            }

            if (selectedToDate > DateTime.Today)
            {
                selectedToDate =
                    DateTime.Today;
            }

            // ======================================================
            // DATE-RANGE SUMMARY
            // ======================================================

            decimal totalSales =
                ReportStorage.GetSales(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            int saleCount =
                ReportStorage.GetSaleCount(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            decimal purchases =
                ReportStorage.GetPurchases(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            decimal refunds =
                ReportStorage.GetRefunds(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            decimal estimatedProfit =
                ReportStorage.GetEstimatedProfit(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            // ======================================================
            // CURRENT PHARMACY STATUS
            // ======================================================

            int totalCustomers =
                ReportStorage.GetTotalCustomers(
                    pharmacyId);

            int totalMedicines =
                ReportStorage.GetTotalMedicines(
                    pharmacyId);

            decimal pendingCredit =
                ReportStorage.GetPendingCredit(
                    pharmacyId);

            int customersWithCredit =
                ReportStorage.GetCustomersWithCredit(
                    pharmacyId);

            // ======================================================
            // CURRENT STOCK STATUS
            // ======================================================

            var lowStockMedicines =
                ReportStorage.GetLowStockMedicines(
                    pharmacyId);

            var expiringMedicines =
                ReportStorage.GetExpiringMedicines(
                    pharmacyId);

            // ======================================================
            // DATE-RANGE SALES
            // ======================================================

            var sales =
                ReportStorage.GetSales(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate,
                    10000);

            // ======================================================
            // ADVANCED REPORTS
            // ======================================================

            var medicineSales =
                ReportStorage.GetMedicineWiseSales(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            var supplierPurchases =
                ReportStorage.GetSupplierPurchases(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            var stockMovement =
                ReportStorage.GetStockMovement(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate);

            // ======================================================
            // VIEWBAGS
            // ======================================================

            ViewBag.FromDate =
                selectedFromDate;

            ViewBag.ToDate =
                selectedToDate;

            ViewBag.TotalSales =
                totalSales;

            ViewBag.SaleCount =
                saleCount;

            ViewBag.Purchases =
                purchases;

            ViewBag.Refunds =
                refunds;

            ViewBag.EstimatedProfit =
                estimatedProfit;

            ViewBag.TotalCustomers =
                totalCustomers;

            ViewBag.TotalMedicines =
                totalMedicines;

            ViewBag.PendingCredit =
                pendingCredit;

            ViewBag.CustomersWithCredit =
                customersWithCredit;

            ViewBag.LowStockMedicines =
                lowStockMedicines;

            ViewBag.ExpiringMedicines =
                expiringMedicines;

            ViewBag.Sales =
                sales;

            ViewBag.MedicineSales =
                medicineSales;

            ViewBag.SupplierPurchases =
                supplierPurchases;

            ViewBag.StockMovement =
                stockMovement;

            return View();
        }


        // ==========================================================
        // CSV EXPORT
        // ==========================================================

        [HttpGet]
        public IActionResult ExportCsv(
            DateTime? fromDate,
            DateTime? toDate)
        {
            if (!PermissionChecker.HasPermission(User, "Reports"))
                return Forbid();

            IActionResult? pharmacyCheck =
                RequirePharmacy();

            if (pharmacyCheck != null)
                return pharmacyCheck;

            int pharmacyId =
                CurrentPharmacyId;

            DateTime selectedFromDate =
                fromDate?.Date
                ?? new DateTime(
                    DateTime.Today.Year,
                    DateTime.Today.Month,
                    1);

            DateTime selectedToDate =
                toDate?.Date
                ?? DateTime.Today;

            if (selectedFromDate > selectedToDate)
            {
                DateTime temp =
                    selectedFromDate;

                selectedFromDate =
                    selectedToDate;

                selectedToDate =
                    temp;
            }

            if (selectedFromDate > DateTime.Today)
                selectedFromDate = DateTime.Today;

            if (selectedToDate > DateTime.Today)
                selectedToDate = DateTime.Today;

            var sales =
                ReportStorage.GetSales(
                    pharmacyId,
                    selectedFromDate,
                    selectedToDate,
                    10000);

            string csv =
                ReportStorage.BuildSalesCsv(
                    sales,
                    selectedFromDate,
                    selectedToDate);

            byte[] bytes =
                System.Text.Encoding.UTF8.GetBytes(
                    csv);

            string fileName =
                $"Sales-Report-{selectedFromDate:yyyy-MM-dd}-to-{selectedToDate:yyyy-MM-dd}.csv";

            return File(
                bytes,
                "text/csv",
                fileName);
        }
    }
}