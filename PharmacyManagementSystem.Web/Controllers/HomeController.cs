using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        // =========================================================
        // DASHBOARD
        // =========================================================

        [HttpGet]
        public IActionResult Index()
        {
            DashboardViewModel dashboard = new DashboardViewModel();

            // =====================================================
            // GET CURRENT PHARMACY ID
            // =====================================================

            string? pharmacyIdClaim =
                User.FindFirstValue("PharmacyId");

            if (!int.TryParse(
                    pharmacyIdClaim,
                    out int pharmacyId) ||
                pharmacyId <= 0)
            {
                return RedirectToAction(
                    "Logout",
                    "Account"
                );
            }


            try
            {
                using (MySqlConnection connection =
                       DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    // =================================================
                    // GET PHARMACY LOW STOCK THRESHOLD
                    // =================================================

                    int lowStockThreshold = 10;

                    string settingsQuery =
                        @"
                        SELECT
                            COALESCE(low_stock_threshold, 10)
                        FROM pharmacies
                        WHERE id = @pharmacy_id
                        LIMIT 1";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               settingsQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        object? result =
                            command.ExecuteScalar();

                        if (result != null &&
                            result != DBNull.Value)
                        {
                            lowStockThreshold =
                                Convert.ToInt32(result);
                        }
                    }

                    // Safety limit
                    if (lowStockThreshold < 1)
                    {
                        lowStockThreshold = 1;
                    }


                    // =================================================
                    // TOTAL MEDICINES
                    // =================================================

                    string medicineQuery =
                        @"
                        SELECT COUNT(*)
                        FROM medicines
                        WHERE pharmacy_id = @pharmacy_id";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               medicineQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        dashboard.TotalMedicines =
                            Convert.ToInt32(
                                command.ExecuteScalar()
                            );
                    }


                    // =================================================
                    // LOW STOCK MEDICINES
                    // =================================================

                    string lowStockQuery =
                        @"
                        SELECT COUNT(*)
                        FROM medicines
                        WHERE pharmacy_id = @pharmacy_id
                        AND quantity <= @low_stock_threshold";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               lowStockQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        command.Parameters.AddWithValue(
                            "@low_stock_threshold",
                            lowStockThreshold
                        );

                        dashboard.LowStockMedicines =
                            Convert.ToInt32(
                                command.ExecuteScalar()
                            );
                    }


                    // =================================================
                    // EXPIRING SOON MEDICINES
                    // =================================================
                    //
                    // Medicines expiring from today through
                    // the next 30 days.
                    //
                    // Expired medicines are NOT included here.
                    // =================================================

                    string expiryQuery =
                        @"
                        SELECT COUNT(*)
                        FROM medicines
                        WHERE pharmacy_id = @pharmacy_id
                        AND expiry_date >= CURDATE()
                        AND expiry_date <=
                            DATE_ADD(CURDATE(), INTERVAL 30 DAY)";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               expiryQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        dashboard.ExpiringSoonMedicines =
                            Convert.ToInt32(
                                command.ExecuteScalar()
                            );
                    }


                    // =================================================
                    // TODAY'S SALES
                    // =================================================
                    //
                    // Net sales today
                    // MINUS
                    // refunds processed today.
                    //
                    // Refunds are linked to sales through sale_id,
                    // so pharmacy isolation is still enforced through
                    // the parent sales record.
                    // =================================================

                    string todaySalesQuery =
                        @"
                        SELECT
                            COALESCE(
                                (
                                    SELECT SUM(s.net_amount)
                                    FROM sales s
                                    WHERE s.pharmacy_id = @pharmacy_id
                                    AND DATE(s.sale_date) = CURDATE()
                                ),
                                0
                            )
                            -
                            COALESCE(
                                (
                                    SELECT SUM(sr.total_refund)
                                    FROM sale_returns sr
                                    INNER JOIN sales s
                                        ON sr.sale_id = s.id
                                    WHERE s.pharmacy_id = @pharmacy_id
                                    AND DATE(sr.return_date) = CURDATE()
                                ),
                                0
                            )";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               todaySalesQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        dashboard.TodaySales =
                            Convert.ToDecimal(
                                command.ExecuteScalar()
                            );
                    }


                    // =================================================
                    // TOTAL SALES
                    // =================================================

                    string totalSalesQuery =
                        @"
                        SELECT COUNT(*)
                        FROM sales
                        WHERE pharmacy_id = @pharmacy_id";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               totalSalesQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        dashboard.TotalSales =
                            Convert.ToInt32(
                                command.ExecuteScalar()
                            );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"Dashboard error: {ex.Message}"
                );

                TempData["ErrorMessage"] =
                    "Some dashboard information could not be loaded.";
            }


            return View(dashboard);
        }


        // =========================================================
        // PRIVACY
        // =========================================================

        [HttpGet]
        public IActionResult Privacy()
        {
            return View();
        }
    }
}