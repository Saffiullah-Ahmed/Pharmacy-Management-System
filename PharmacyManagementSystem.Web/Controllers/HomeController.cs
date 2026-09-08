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
        public IActionResult Index()
        {
            DashboardViewModel dashboard = new DashboardViewModel();

            // ==========================================
            // GET CURRENT PHARMACY
            // ==========================================

            string? pharmacyIdClaim =
                User.FindFirstValue("PharmacyId");

            if (!int.TryParse(pharmacyIdClaim, out int pharmacyId) ||
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


                    // ==========================================
                    // TOTAL MEDICINES
                    // ==========================================

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


                    // ==========================================
                    // LOW STOCK MEDICINES
                    // ==========================================

                    string lowStockQuery =
                        @"
                        SELECT COUNT(*)
                        FROM medicines
                        WHERE pharmacy_id = @pharmacy_id
                        AND quantity <= 10";

                    using (MySqlCommand command =
                           new MySqlCommand(
                               lowStockQuery,
                               connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId
                        );

                        dashboard.LowStockMedicines =
                            Convert.ToInt32(
                                command.ExecuteScalar()
                            );
                    }


                    // ==========================================
                    // EXPIRING SOON MEDICINES
                    // ==========================================

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


                    // ==========================================
                    // TODAY'S SALES
                    //
                    // Sales are already directly linked
                    // to pharmacy_id.
                    //
                    // Refunds do NOT have pharmacy_id,
                    // so they are connected through sales.
                    // ==========================================

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


                    // ==========================================
                    // TOTAL SALES
                    // ==========================================

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
            }


            return View(dashboard);
        }


        // ==========================================
        // PRIVACY
        // ==========================================

        public IActionResult Privacy()
        {
            return View();
        }
    }
}