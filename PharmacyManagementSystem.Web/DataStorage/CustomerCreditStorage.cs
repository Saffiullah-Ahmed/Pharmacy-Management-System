using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class CustomerCreditStorage
    {
        // ============================================================
        // GET CUSTOMERS WITH PENDING PAYMENTS
        // SEARCH BY CUSTOMER NAME OR INVOICE NUMBER
        // ============================================================

        public static List<Customer>
            GetCustomersWithPendingPayments(
                int pharmacyId,
                string? search = null)
        {
            List<Customer> customers =
                new List<Customer>();

            if (pharmacyId <= 0)
            {
                return customers;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            c.id,
                            c.pharmacy_id,
                            c.name,
                            c.phone,
                            c.cnic,
                            c.address,
                            c.created_at
                        FROM customers c
                        WHERE c.pharmacy_id = @pharmacy_id
                          AND EXISTS
                          (
                              SELECT 1
                              FROM sales s
                              WHERE s.customer_id = c.id
                                AND s.pharmacy_id = @pharmacy_id
                                AND s.balance_amount > 0
                                AND
                                (
                                    @search = ''
                                    OR c.name LIKE CONCAT('%', @search, '%')
                                    OR s.invoice_no LIKE CONCAT('%', @search, '%')
                                )
                          )
                        ORDER BY c.name";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);

                        command.Parameters.AddWithValue(
                            "@search",
                            search?.Trim() ?? "");

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Customer customer =
                                    new Customer
                                    {
                                        Id =
                                            reader.GetInt32(
                                                "id"),

                                        PharmacyId =
                                            reader.GetInt32(
                                                "pharmacy_id"),

                                        Name =
                                            reader.GetString(
                                                "name"),

                                        Phone =
                                            reader.IsDBNull(
                                                reader.GetOrdinal(
                                                    "phone"))
                                                ? ""
                                                : reader.GetString(
                                                    "phone"),

                                        CNIC =
                                            reader.IsDBNull(
                                                reader.GetOrdinal(
                                                    "cnic"))
                                                ? ""
                                                : reader.GetString(
                                                    "cnic"),

                                        Address =
                                            reader.IsDBNull(
                                                reader.GetOrdinal(
                                                    "address"))
                                                ? ""
                                                : reader.GetString(
                                                    "address"),

                                        CreatedAt =
                                            reader.GetDateTime(
                                                "created_at")
                                    };

                                customers.Add(customer);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET PENDING CUSTOMERS ERROR: " +
                    ex.Message);
            }

            return customers;
        }


        // ============================================================
        // GET CUSTOMER OUTSTANDING BALANCE
        // ============================================================

        public static decimal GetCustomerBalance(
            int customerId,
            int pharmacyId)
        {
            if (customerId <= 0 ||
                pharmacyId <= 0)
            {
                return 0;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT COALESCE(
                            SUM(s.balance_amount),
                            0
                        )
                        FROM sales s
                        INNER JOIN customers c
                            ON c.id = s.customer_id
                           AND c.pharmacy_id = s.pharmacy_id
                        WHERE s.customer_id = @customer_id
                          AND s.pharmacy_id = @pharmacy_id
                          AND c.pharmacy_id = @pharmacy_id
                          AND s.balance_amount > 0";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@customer_id",
                            customerId);

                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);

                        return Convert.ToDecimal(
                            command.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET CUSTOMER BALANCE ERROR: " +
                    ex.Message);

                return 0;
            }
        }


        // ============================================================
        // GET CUSTOMER SALES WITH BALANCE
        // ============================================================

        public static List<Sale>
            GetPendingSales(
                int customerId,
                int pharmacyId)
        {
            List<Sale> sales =
                new List<Sale>();

            if (customerId <= 0 ||
                pharmacyId <= 0)
            {
                return sales;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            s.id,
                            s.invoice_no,
                            s.sale_date,
                            s.customer_id,
                            s.total_amount,
                            s.discount_amount,
                            s.net_amount,
                            s.amount_received,
                            s.change_amount,
                            s.balance_amount
                        FROM sales s
                        INNER JOIN customers c
                            ON c.id = s.customer_id
                           AND c.pharmacy_id = s.pharmacy_id
                        WHERE s.customer_id = @customer_id
                          AND s.pharmacy_id = @pharmacy_id
                          AND c.pharmacy_id = @pharmacy_id
                          AND s.balance_amount > 0
                        ORDER BY
                            s.sale_date ASC,
                            s.id ASC";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@customer_id",
                            customerId);

                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Sale sale =
                                    new Sale
                                    {
                                        Id =
                                            reader.GetInt32(
                                                "id"),

                                        InvoiceNumber =
                                            reader.GetString(
                                                "invoice_no"),

                                        SaleDate =
                                            reader.GetDateTime(
                                                "sale_date"),

                                        CustomerId =
                                            reader.IsDBNull(
                                                reader.GetOrdinal(
                                                    "customer_id"))
                                                ? null
                                                : reader.GetInt32(
                                                    reader.GetOrdinal(
                                                        "customer_id")),

                                        TotalAmount =
                                            reader.GetDecimal(
                                                "total_amount"),

                                        DiscountAmount =
                                            reader.GetDecimal(
                                                "discount_amount"),

                                        NetAmount =
                                            reader.GetDecimal(
                                                "net_amount"),

                                        AmountReceived =
                                            reader.GetDecimal(
                                                "amount_received"),

                                        ChangeAmount =
                                            reader.GetDecimal(
                                                "change_amount"),

                                        BalanceAmount =
                                            reader.GetDecimal(
                                                "balance_amount")
                                    };

                                sales.Add(sale);
                            }
                        }
                    }
                }


                // ====================================================
                // LOAD SALE ITEMS
                // ====================================================

                foreach (Sale sale in sales)
                {
                    sale.Items =
                        SaleStorage.GetItemsForCredit(
                            sale.Id,
                            pharmacyId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET PENDING SALES ERROR: " +
                    ex.Message);
            }

            return sales;
        }


        // ============================================================
        // RECEIVE PAYMENT
        // ============================================================

        public static bool ReceivePayment(
            int customerId,
            int saleId,
            decimal amount,
            string notes,
            int pharmacyId)
        {
            if (customerId <= 0 ||
                saleId <= 0 ||
                amount <= 0 ||
                pharmacyId <= 0)
            {
                return false;
            }

            using (MySqlConnection connection =
                DatabaseConnection.GetConnection())
            {
                connection.Open();

                using (MySqlTransaction transaction =
                    connection.BeginTransaction())
                {
                    try
                    {
                        // ==================================================
                        // VERIFY CUSTOMER BELONGS TO CURRENT PHARMACY
                        // ==================================================

                        string customerQuery = @"
                            SELECT id
                            FROM customers
                            WHERE id = @customer_id
                              AND pharmacy_id = @pharmacy_id
                            LIMIT 1";

                        using (MySqlCommand customerCommand =
                            new MySqlCommand(
                                customerQuery,
                                connection,
                                transaction))
                        {
                            customerCommand.Parameters.AddWithValue(
                                "@customer_id",
                                customerId);

                            customerCommand.Parameters.AddWithValue(
                                "@pharmacy_id",
                                pharmacyId);

                            object? customerResult =
                                customerCommand.ExecuteScalar();

                            if (customerResult == null)
                            {
                                throw new Exception(
                                    "Customer was not found.");
                            }
                        }


                        // ==================================================
                        // LOCK SALE
                        // ==================================================

                        string saleQuery = @"
                            SELECT
                                customer_id,
                                balance_amount
                            FROM sales
                            WHERE id = @sale_id
                              AND pharmacy_id = @pharmacy_id
                            FOR UPDATE";

                        int? saleCustomerId = null;
                        decimal currentBalance = 0;

                        using (MySqlCommand command =
                            new MySqlCommand(
                                saleQuery,
                                connection,
                                transaction))
                        {
                            command.Parameters.AddWithValue(
                                "@sale_id",
                                saleId);

                            command.Parameters.AddWithValue(
                                "@pharmacy_id",
                                pharmacyId);

                            using (MySqlDataReader reader =
                                command.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    throw new Exception(
                                        "Sale was not found.");
                                }

                                saleCustomerId =
                                    reader.IsDBNull(
                                        reader.GetOrdinal(
                                            "customer_id"))
                                        ? null
                                        : reader.GetInt32(
                                            reader.GetOrdinal(
                                                "customer_id"));

                                currentBalance =
                                    reader.GetDecimal(
                                        reader.GetOrdinal(
                                            "balance_amount"));
                            }
                        }


                        // ==================================================
                        // VERIFY SALE BELONGS TO CUSTOMER
                        // ==================================================

                        if (!saleCustomerId.HasValue ||
                            saleCustomerId.Value != customerId)
                        {
                            throw new Exception(
                                "This sale does not belong to the selected customer.");
                        }


                        // ==================================================
                        // CHECK BALANCE
                        // ==================================================

                        if (currentBalance <= 0)
                        {
                            throw new Exception(
                                "This sale has no pending balance.");
                        }


                        if (amount > currentBalance)
                        {
                            throw new Exception(
                                "Payment cannot be greater than the pending balance.");
                        }


                        // ==================================================
                        // SAVE PAYMENT HISTORY
                        // ==================================================

                        string paymentQuery = @"
                            INSERT INTO customer_payments
                            (
                                customer_id,
                                sale_id,
                                amount,
                                payment_date,
                                notes
                            )
                            VALUES
                            (
                                @customer_id,
                                @sale_id,
                                @amount,
                                @payment_date,
                                @notes
                            )";

                        using (MySqlCommand paymentCommand =
                            new MySqlCommand(
                                paymentQuery,
                                connection,
                                transaction))
                        {
                            paymentCommand.Parameters.AddWithValue(
                                "@customer_id",
                                customerId);

                            paymentCommand.Parameters.AddWithValue(
                                "@sale_id",
                                saleId);

                            paymentCommand.Parameters.AddWithValue(
                                "@amount",
                                amount);

                            paymentCommand.Parameters.AddWithValue(
                                "@payment_date",
                                DateTime.Now);

                            paymentCommand.Parameters.AddWithValue(
                                "@notes",
                                string.IsNullOrWhiteSpace(
                                    notes)
                                    ? DBNull.Value
                                    : notes.Trim());

                            paymentCommand.ExecuteNonQuery();
                        }


                        // ==================================================
                        // UPDATE SALE BALANCE
                        // ==================================================

                        string updateSaleQuery = @"
                            UPDATE sales
                            SET
                                amount_received =
                                    amount_received + @amount,

                                balance_amount =
                                    balance_amount - @amount
                            WHERE id = @sale_id
                              AND pharmacy_id = @pharmacy_id";

                        using (MySqlCommand updateCommand =
                            new MySqlCommand(
                                updateSaleQuery,
                                connection,
                                transaction))
                        {
                            updateCommand.Parameters.AddWithValue(
                                "@amount",
                                amount);

                            updateCommand.Parameters.AddWithValue(
                                "@sale_id",
                                saleId);

                            updateCommand.Parameters.AddWithValue(
                                "@pharmacy_id",
                                pharmacyId);

                            int affectedRows =
                                updateCommand.ExecuteNonQuery();

                            if (affectedRows == 0)
                            {
                                throw new Exception(
                                    "Sale balance could not be updated.");
                            }
                        }


                        // ==================================================
                        // COMMIT
                        // ==================================================

                        transaction.Commit();

                        return true;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            transaction.Rollback();
                        }
                        catch
                        {
                        }

                        Console.WriteLine(
                            "RECEIVE PAYMENT ERROR: " +
                            ex.Message);

                        return false;
                    }
                }
            }
        }


        // ============================================================
        // PAYMENT HISTORY
        // ============================================================

        public static List<CustomerPayment>
            GetPaymentHistory(
                int customerId,
                int pharmacyId)
        {
            List<CustomerPayment> payments =
                new List<CustomerPayment>();

            if (customerId <= 0 ||
                pharmacyId <= 0)
            {
                return payments;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            cp.id,
                            cp.customer_id,
                            cp.sale_id,
                            cp.amount,
                            cp.payment_date,
                            cp.notes,
                            c.name AS customer_name,
                            s.invoice_no
                        FROM customer_payments cp
                        INNER JOIN customers c
                            ON cp.customer_id = c.id
                        INNER JOIN sales s
                            ON cp.sale_id = s.id
                           AND s.customer_id = c.id
                        WHERE cp.customer_id = @customer_id
                          AND c.pharmacy_id = @pharmacy_id
                          AND s.pharmacy_id = @pharmacy_id
                        ORDER BY
                            cp.payment_date DESC,
                            cp.id DESC";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@customer_id",
                            customerId);

                        command.Parameters.AddWithValue(
                            "@pharmacy_id",
                            pharmacyId);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CustomerPayment payment =
                                    new CustomerPayment
                                    {
                                        Id =
                                            reader.GetInt32(
                                                "id"),

                                        CustomerId =
                                            reader.GetInt32(
                                                "customer_id"),

                                        SaleId =
                                            reader.GetInt32(
                                                "sale_id"),

                                        Amount =
                                            reader.GetDecimal(
                                                "amount"),

                                        PaymentDate =
                                            reader.GetDateTime(
                                                "payment_date"),

                                        Notes =
                                            reader.IsDBNull(
                                                reader.GetOrdinal(
                                                    "notes"))
                                                ? ""
                                                : reader.GetString(
                                                    "notes"),

                                        CustomerName =
                                            reader.GetString(
                                                "customer_name"),

                                        InvoiceNumber =
                                            reader.GetString(
                                                "invoice_no")
                                    };

                                payments.Add(payment);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET PAYMENT HISTORY ERROR: " +
                    ex.Message);
            }

            return payments;
        }
    }
}