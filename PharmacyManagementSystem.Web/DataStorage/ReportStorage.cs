using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;
using System.Text;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class ReportStorage
    {
        // ==========================================================
        // SALES
        //
        // Selected dates are inclusive.
        // Example:
        // 02/08/2026 -> 02/09/2026
        //
        // Internally:
        // >= 02/08/2026 00:00:00
        // < 03/09/2026 00:00:00
        // ==========================================================

        public static decimal GetSales(
            int pharmacyId,
            DateTime fromDate,
            DateTime toDate)
        {
            if (pharmacyId <= 0)
                return 0;

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    COALESCE(
                        SUM(s.net_amount),
                        0
                    )
                    -
                    COALESCE(
                        (
                            SELECT SUM(sr.total_refund)
                            FROM sale_returns sr
                            INNER JOIN sales s2
                                ON s2.id = sr.sale_id
                            WHERE s2.pharmacy_id = @pharmacy_id
                              AND sr.return_date >= @start_date
                              AND sr.return_date < @end_date
                        ),
                        0
                    )
                FROM sales s
                WHERE s.pharmacy_id = @pharmacy_id
                  AND s.sale_date >= @start_date
                  AND s.sale_date < @end_date;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            return Convert.ToDecimal(
                command.ExecuteScalar());
        }


        // ==========================================================
        // SALE COUNT
        // ==========================================================

        public static int GetSaleCount(
            int pharmacyId,
            DateTime fromDate,
            DateTime toDate)
        {
            if (pharmacyId <= 0)
                return 0;

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COUNT(*)
                FROM sales
                WHERE pharmacy_id = @pharmacy_id
                  AND sale_date >= @start_date
                  AND sale_date < @end_date;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            return Convert.ToInt32(
                command.ExecuteScalar());
        }


        // ==========================================================
        // PURCHASES
        // ==========================================================

        public static decimal GetPurchases(
            int pharmacyId,
            DateTime fromDate,
            DateTime toDate)
        {
            if (pharmacyId <= 0)
                return 0;

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COALESCE(
                    SUM(total_amount),
                    0
                )
                FROM purchases
                WHERE pharmacy_id = @pharmacy_id
                  AND purchase_date >= @start_date
                  AND purchase_date < @end_date;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            return Convert.ToDecimal(
                command.ExecuteScalar());
        }


        // ==========================================================
        // REFUNDS
        // ==========================================================

        public static decimal GetRefunds(
            int pharmacyId,
            DateTime fromDate,
            DateTime toDate)
        {
            if (pharmacyId <= 0)
                return 0;

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COALESCE(
                    SUM(sr.total_refund),
                    0
                )
                FROM sale_returns sr
                INNER JOIN sales s
                    ON s.id = sr.sale_id
                WHERE s.pharmacy_id = @pharmacy_id
                  AND sr.return_date >= @start_date
                  AND sr.return_date < @end_date;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            return Convert.ToDecimal(
                command.ExecuteScalar());
        }


        // ==========================================================
        // ESTIMATED PROFIT
        //
        // Uses weighted average purchase cost.
        // ==========================================================

        public static decimal GetEstimatedProfit(
            int pharmacyId,
            DateTime fromDate,
            DateTime toDate)
        {
            if (pharmacyId <= 0)
                return 0;

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    COALESCE(
                        SUM(
                            si.quantity *
                            COALESCE(
                                (
                                    SELECT
                                        SUM(
                                            pi2.quantity *
                                            pi2.cost_price
                                        )
                                        /
                                        NULLIF(
                                            SUM(pi2.quantity),
                                            0
                                        )
                                    FROM purchase_items pi2
                                    INNER JOIN purchases p2
                                        ON p2.id = pi2.purchase_id
                                    WHERE pi2.medicine_id =
                                          si.medicine_id
                                      AND p2.pharmacy_id =
                                          @pharmacy_id
                                      AND p2.purchase_date <
                                          @end_date
                                ),
                                0
                            )
                        ),
                        0
                    )
                FROM sale_items si
                INNER JOIN sales s
                    ON s.id = si.sale_id
                INNER JOIN medicines m
                    ON m.id = si.medicine_id
                WHERE s.pharmacy_id = @pharmacy_id
                  AND m.pharmacy_id = @pharmacy_id
                  AND s.sale_date >= @start_date
                  AND s.sale_date < @end_date;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            decimal costOfGoods =
                Convert.ToDecimal(
                    command.ExecuteScalar());

            decimal sales =
                GetSales(
                    pharmacyId,
                    fromDate,
                    toDate);

            return sales - costOfGoods;
        }


        // ==========================================================
        // TOTAL CUSTOMERS
        // ==========================================================

        public static int GetTotalCustomers(
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COUNT(*)
                FROM customers
                WHERE pharmacy_id = @pharmacy_id;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            return Convert.ToInt32(
                command.ExecuteScalar());
        }


        // ==========================================================
        // TOTAL MEDICINES
        // ==========================================================

        public static int GetTotalMedicines(
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COUNT(*)
                FROM medicines
                WHERE pharmacy_id = @pharmacy_id;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            return Convert.ToInt32(
                command.ExecuteScalar());
        }


        // ==========================================================
        // PENDING CREDIT
        // ==========================================================

        public static decimal GetPendingCredit(
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COALESCE(
                    SUM(balance_amount),
                    0
                )
                FROM sales
                WHERE pharmacy_id = @pharmacy_id
                  AND balance_amount > 0;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            return Convert.ToDecimal(
                command.ExecuteScalar());
        }


        // ==========================================================
        // CUSTOMERS WITH CREDIT
        // ==========================================================

        public static int GetCustomersWithCredit(
            int pharmacyId)
        {
            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT COUNT(DISTINCT customer_id)
                FROM sales
                WHERE pharmacy_id = @pharmacy_id
                  AND customer_id IS NOT NULL
                  AND balance_amount > 0;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            return Convert.ToInt32(
                command.ExecuteScalar());
        }


        // ==========================================================
        // LOW STOCK
        // ==========================================================

        public static List<Medicine> GetLowStockMedicines(
            int pharmacyId,
            int threshold = 10)
        {
            var medicines =
                new List<Medicine>();

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    id,
                    name,
                    price,
                    quantity,
                    expiry_date
                FROM medicines
                WHERE pharmacy_id = @pharmacy_id
                  AND quantity <= @threshold
                ORDER BY quantity ASC, name ASC;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@threshold",
                threshold);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                medicines.Add(
                    MapMedicine(reader));
            }

            return medicines;
        }


        // ==========================================================
        // EXPIRING MEDICINES
        // ==========================================================

        public static List<Medicine> GetExpiringMedicines(
            int pharmacyId,
            int days = 30)
        {
            var medicines =
                new List<Medicine>();

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    id,
                    name,
                    price,
                    quantity,
                    expiry_date
                FROM medicines
                WHERE pharmacy_id = @pharmacy_id
                  AND expiry_date IS NOT NULL
                  AND expiry_date >= CURDATE()
                  AND expiry_date <= DATE_ADD(
                        CURDATE(),
                        INTERVAL @days DAY
                  )
                ORDER BY expiry_date ASC;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@days",
                days);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                medicines.Add(
                    MapMedicine(reader));
            }

            return medicines;
        }


        // ==========================================================
        // SALES
        // ==========================================================

        public static List<Sale> GetSales(
            int pharmacyId,
            DateTime fromDate,
            DateTime toDate,
            int limit = 10000)
        {
            var sales =
                new List<Sale>();

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    s.id,
                    s.pharmacy_id,
                    s.invoice_no,
                    s.customer_id,
                    s.sale_date,
                    s.total_amount,
                    s.discount_amount,
                    s.net_amount,
                    s.amount_received,
                    s.change_amount,
                    s.balance_amount,
                    s.generate_bill
                FROM sales s
                WHERE s.pharmacy_id = @pharmacy_id
                  AND s.sale_date >= @start_date
                  AND s.sale_date < @end_date
                ORDER BY
                    s.sale_date DESC,
                    s.id DESC
                LIMIT @limit;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            command.Parameters.AddWithValue(
                "@limit",
                limit);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                sales.Add(
                    MapSale(reader));
            }

            reader.Close();

            foreach (var sale in sales)
            {
                sale.Items =
                    GetSaleItems(
                        sale.Id,
                        pharmacyId);
            }

            return sales;
        }


        // ==========================================================
        // MEDICINE-WISE SALES
        // ==========================================================

        public static List<MedicineSalesReport>
            GetMedicineWiseSales(
                int pharmacyId,
                DateTime fromDate,
                DateTime toDate)
        {
            var result =
                new List<MedicineSalesReport>();

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    si.medicine_id,
                    m.name AS medicine_name,
                    SUM(si.quantity) AS quantity_sold,
                    SUM(si.total) AS sales_amount,
                    COALESCE(
                        (
                            SELECT
                                SUM(
                                    pi.quantity *
                                    pi.cost_price
                                )
                                /
                                NULLIF(
                                    SUM(pi.quantity),
                                    0
                                )
                            FROM purchase_items pi
                            INNER JOIN purchases p
                                ON p.id = pi.purchase_id
                            WHERE pi.medicine_id =
                                  si.medicine_id
                              AND p.pharmacy_id =
                                  @pharmacy_id
                              AND p.purchase_date <
                                  @end_date
                        ),
                        0
                    ) AS average_cost
                FROM sale_items si
                INNER JOIN sales s
                    ON s.id = si.sale_id
                INNER JOIN medicines m
                    ON m.id = si.medicine_id
                WHERE s.pharmacy_id = @pharmacy_id
                  AND m.pharmacy_id = @pharmacy_id
                  AND s.sale_date >= @start_date
                  AND s.sale_date < @end_date
                GROUP BY
                    si.medicine_id,
                    m.name
                ORDER BY sales_amount DESC;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                int quantity =
                    Convert.ToInt32(
                        reader["quantity_sold"]);

                decimal salesAmount =
                    Convert.ToDecimal(
                        reader["sales_amount"]);

                decimal averageCost =
                    Convert.ToDecimal(
                        reader["average_cost"]);

                decimal estimatedCost =
                    quantity * averageCost;

                result.Add(
                    new MedicineSalesReport
                    {
                        MedicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]),

                        MedicineName =
                            reader["medicine_name"]
                                ?.ToString()
                            ?? "",

                        QuantitySold =
                            quantity,

                        SalesAmount =
                            salesAmount,

                        EstimatedCost =
                            estimatedCost,

                        EstimatedProfit =
                            salesAmount -
                            estimatedCost
                    });
            }

            return result;
        }


        // ==========================================================
        // SUPPLIER PURCHASES
        // ==========================================================

        public static List<SupplierPurchaseReport>
            GetSupplierPurchases(
                int pharmacyId,
                DateTime fromDate,
                DateTime toDate)
        {
            var result =
                new List<SupplierPurchaseReport>();

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    supplier_name,
                    COUNT(*) AS purchase_count,
                    COALESCE(
                        SUM(total_amount),
                        0
                    ) AS total_amount
                FROM purchases
                WHERE pharmacy_id = @pharmacy_id
                  AND purchase_date >= @start_date
                  AND purchase_date < @end_date
                GROUP BY supplier_name
                ORDER BY total_amount DESC;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                result.Add(
                    new SupplierPurchaseReport
                    {
                        SupplierName =
                            reader["supplier_name"]
                                ?.ToString()
                            ?? "Unknown",

                        PurchaseCount =
                            Convert.ToInt32(
                                reader["purchase_count"]),

                        TotalAmount =
                            Convert.ToDecimal(
                                reader["total_amount"])
                    });
            }

            return result;
        }


        // ==========================================================
        // STOCK MOVEMENT
        // ==========================================================

        public static List<StockMovementReport>
            GetStockMovement(
                int pharmacyId,
                DateTime fromDate,
                DateTime toDate)
        {
            var result =
                new List<StockMovementReport>();

            DateTime startDate =
                fromDate.Date;

            DateTime endDate =
                toDate.Date.AddDays(1);

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    m.id AS medicine_id,
                    m.name AS medicine_name,

                    COALESCE(
                        (
                            SELECT SUM(pi.quantity)
                            FROM purchase_items pi
                            INNER JOIN purchases p
                                ON p.id = pi.purchase_id
                            WHERE pi.medicine_id = m.id
                              AND p.pharmacy_id = @pharmacy_id
                              AND p.purchase_date >= @start_date
                              AND p.purchase_date < @end_date
                        ),
                        0
                    ) AS purchased_quantity,

                    COALESCE(
                        (
                            SELECT SUM(si.quantity)
                            FROM sale_items si
                            INNER JOIN sales s
                                ON s.id = si.sale_id
                            WHERE si.medicine_id = m.id
                              AND s.pharmacy_id = @pharmacy_id
                              AND s.sale_date >= @start_date
                              AND s.sale_date < @end_date
                        ),
                        0
                    ) AS sold_quantity,

                    m.quantity AS current_stock

                FROM medicines m

                WHERE m.pharmacy_id = @pharmacy_id

                ORDER BY m.name;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            command.Parameters.AddWithValue(
                "@start_date",
                startDate);

            command.Parameters.AddWithValue(
                "@end_date",
                endDate);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                int purchased =
                    Convert.ToInt32(
                        reader["purchased_quantity"]);

                int sold =
                    Convert.ToInt32(
                        reader["sold_quantity"]);

                result.Add(
                    new StockMovementReport
                    {
                        MedicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]),

                        MedicineName =
                            reader["medicine_name"]
                                ?.ToString()
                            ?? "",

                        PurchasedQuantity =
                            purchased,

                        SoldQuantity =
                            sold,

                        NetMovement =
                            purchased - sold,

                        CurrentStock =
                            Convert.ToInt32(
                                reader["current_stock"])
                    });
            }

            return result;
        }


        // ==========================================================
        // SALE ITEMS
        // ==========================================================

        private static List<SaleItem> GetSaleItems(
            int saleId,
            int pharmacyId)
        {
            var items =
                new List<SaleItem>();

            using var connection =
                DatabaseConnection.GetConnection();

            connection.Open();

            const string sql = @"
                SELECT
                    si.id,
                    si.sale_id,
                    si.medicine_id,
                    m.name AS medicine_name,
                    si.quantity,
                    si.price AS unit_price,
                    si.total AS total_price

                FROM sale_items si

                INNER JOIN sales s
                    ON s.id = si.sale_id

                INNER JOIN medicines m
                    ON m.id = si.medicine_id

                WHERE si.sale_id = @sale_id
                  AND s.pharmacy_id = @pharmacy_id
                  AND m.pharmacy_id = @pharmacy_id

                ORDER BY si.id;";

            using var command =
                new MySqlCommand(
                    sql,
                    connection);

            command.Parameters.AddWithValue(
                "@sale_id",
                saleId);

            command.Parameters.AddWithValue(
                "@pharmacy_id",
                pharmacyId);

            using var reader =
                command.ExecuteReader();

            while (reader.Read())
            {
                items.Add(
                    new SaleItem
                    {
                        Id =
                            Convert.ToInt32(
                                reader["id"]),

                        SaleId =
                            Convert.ToInt32(
                                reader["sale_id"]),

                        MedicineId =
                            Convert.ToInt32(
                                reader["medicine_id"]),

                        MedicineName =
                            reader["medicine_name"]
                                ?.ToString()
                            ?? "",

                        Quantity =
                            Convert.ToInt32(
                                reader["quantity"]),

                        UnitPrice =
                            Convert.ToDecimal(
                                reader["unit_price"]),

                        TotalPrice =
                            Convert.ToDecimal(
                                reader["total_price"])
                    });
            }

            return items;
        }


        // ==========================================================
        // MAP SALE
        // ==========================================================

        private static Sale MapSale(
            MySqlDataReader reader)
        {
            return new Sale
            {
                Id =
                    Convert.ToInt32(
                        reader["id"]),

                PharmacyId =
                    Convert.ToInt32(
                        reader["pharmacy_id"]),

                InvoiceNumber =
                    reader["invoice_no"]
                        ?.ToString()
                    ?? "",

                CustomerId =
                    reader["customer_id"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(
                            reader["customer_id"]),

                SaleDate =
                    Convert.ToDateTime(
                        reader["sale_date"]),

                TotalAmount =
                    Convert.ToDecimal(
                        reader["total_amount"]),

                DiscountAmount =
                    Convert.ToDecimal(
                        reader["discount_amount"]),

                NetAmount =
                    Convert.ToDecimal(
                        reader["net_amount"]),

                AmountReceived =
                    Convert.ToDecimal(
                        reader["amount_received"]),

                ChangeAmount =
                    Convert.ToDecimal(
                        reader["change_amount"]),

                BalanceAmount =
                    Convert.ToDecimal(
                        reader["balance_amount"]),

                GenerateBill =
                    Convert.ToBoolean(
                        reader["generate_bill"])
            };
        }


        // ==========================================================
        // MAP MEDICINE
        // ==========================================================

        private static Medicine MapMedicine(
            MySqlDataReader reader)
        {
            DateTime? expiryDate =
                reader["expiry_date"] == DBNull.Value
                    ? null
                    : Convert.ToDateTime(
                        reader["expiry_date"]);

            return new Medicine
            {
                Id =
                    Convert.ToInt32(
                        reader["id"]),

                Name =
                    reader["name"]
                        ?.ToString()
                    ?? "",

                Price =
                    Convert.ToDecimal(
                        reader["price"]),

                Quantity =
                    Convert.ToInt32(
                        reader["quantity"]),

                ExpiryDate =
                    expiryDate
            };
        }


        // ==========================================================
        // CSV EXPORT
        // ==========================================================

        public static string BuildSalesCsv(
            List<Sale> sales,
            DateTime fromDate,
            DateTime toDate)
        {
            var builder =
                new StringBuilder();

            builder.AppendLine(
                "Pharmacy Sales Report");

            builder.AppendLine(
                $"From,{fromDate:dd/MM/yyyy}");

            builder.AppendLine(
                $"To,{toDate:dd/MM/yyyy}");

            builder.AppendLine();

            builder.AppendLine(
                "Invoice Number,Sale Date,Total Amount,Discount,Net Amount,Amount Received,Change,Balance");

            foreach (var sale in sales)
            {
                builder.AppendLine(
                    string.Join(
                        ",",
                        Csv(sale.InvoiceNumber),

                        Csv(
                            sale.SaleDate.ToString(
                                "dd/MM/yyyy hh:mm tt")),

                        sale.TotalAmount.ToString(
                            "N2"),

                        sale.DiscountAmount.ToString(
                            "N2"),

                        sale.NetAmount.ToString(
                            "N2"),

                        sale.AmountReceived.ToString(
                            "N2"),

                        sale.ChangeAmount.ToString(
                            "N2"),

                        sale.BalanceAmount.ToString(
                            "N2")));
            }

            return builder.ToString();
        }


        // ==========================================================
        // CSV ESCAPE
        // ==========================================================

        private static string Csv(
            string? value)
        {
            if (string.IsNullOrEmpty(value))
                return "\"\"";

            return "\"" +
                   value.Replace(
                       "\"",
                       "\"\"") +
                   "\"";
        }


        // ==========================================================
        // REPORT CLASSES
        // ==========================================================

        public class MedicineSalesReport
        {
            public int MedicineId { get; set; }

            public string MedicineName { get; set; } = "";

            public int QuantitySold { get; set; }

            public decimal SalesAmount { get; set; }

            public decimal EstimatedCost { get; set; }

            public decimal EstimatedProfit { get; set; }
        }


        public class SupplierPurchaseReport
        {
            public string SupplierName { get; set; } = "";

            public int PurchaseCount { get; set; }

            public decimal TotalAmount { get; set; }
        }


        public class StockMovementReport
        {
            public int MedicineId { get; set; }

            public string MedicineName { get; set; } = "";

            public int PurchasedQuantity { get; set; }

            public int SoldQuantity { get; set; }

            public int NetMovement { get; set; }

            public int CurrentStock { get; set; }
        }
    }
}