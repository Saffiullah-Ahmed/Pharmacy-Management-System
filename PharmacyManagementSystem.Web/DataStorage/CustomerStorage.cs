using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class CustomerStorage
    {
        // ============================================================
        // LOAD CUSTOMERS FOR CURRENT PHARMACY
        // ============================================================

        public static List<Customer> Load(int pharmacyId)
        {
            List<Customer> customers =
                new List<Customer>();

            if (pharmacyId <= 0)
            {
                Console.WriteLine(
                    "LOAD CUSTOMERS ERROR: Invalid pharmacy ID.");

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
                            id,
                            name,
                            phone,
                            cnic,
                            address,
                            created_at
                        FROM customers
                        WHERE pharmacy_id = @pharmacyId
                        ORDER BY id DESC";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Customer customer =
                                    new Customer
                                    {
                                        Id =
                                            reader.GetInt32("id"),

                                        Name =
                                            reader.GetString("name"),

                                        Phone =
                                            reader.IsDBNull(
                                                reader.GetOrdinal("phone"))
                                                ? ""
                                                : reader.GetString("phone"),

                                        CNIC =
                                            reader.IsDBNull(
                                                reader.GetOrdinal("cnic"))
                                                ? ""
                                                : reader.GetString("cnic"),

                                        Address =
                                            reader.IsDBNull(
                                                reader.GetOrdinal("address"))
                                                ? ""
                                                : reader.GetString("address"),

                                        CreatedAt =
                                            reader.GetDateTime("created_at")
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
                    "LOAD CUSTOMERS ERROR: " +
                    ex.Message);
            }

            return customers;
        }


        // ============================================================
        // GET CUSTOMER BY ID
        // ============================================================

        public static Customer? GetById(
            int id,
            int pharmacyId)
        {
            if (id <= 0 || pharmacyId <= 0)
            {
                return null;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        SELECT
                            id,
                            name,
                            phone,
                            cnic,
                            address,
                            created_at
                        FROM customers
                        WHERE id = @id
                          AND pharmacy_id = @pharmacyId
                        LIMIT 1";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id",
                            id);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

                            return new Customer
                            {
                                Id =
                                    reader.GetInt32("id"),

                                Name =
                                    reader.GetString("name"),

                                Phone =
                                    reader.IsDBNull(
                                        reader.GetOrdinal("phone"))
                                        ? ""
                                        : reader.GetString("phone"),

                                CNIC =
                                    reader.IsDBNull(
                                        reader.GetOrdinal("cnic"))
                                        ? ""
                                        : reader.GetString("cnic"),

                                Address =
                                    reader.IsDBNull(
                                        reader.GetOrdinal("address"))
                                        ? ""
                                        : reader.GetString("address"),

                                CreatedAt =
                                    reader.GetDateTime("created_at")
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET CUSTOMER ERROR: " +
                    ex.Message);

                return null;
            }
        }


        // ============================================================
        // ADD CUSTOMER
        // ============================================================

        public static bool Add(
            Customer customer,
            int pharmacyId)
        {
            if (customer == null ||
                pharmacyId <= 0)
            {
                return false;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        INSERT INTO customers
                        (
                            pharmacy_id,
                            name,
                            phone,
                            cnic,
                            address
                        )
                        VALUES
                        (
                            @pharmacyId,
                            @name,
                            @phone,
                            @cnic,
                            @address
                        )";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        command.Parameters.AddWithValue(
                            "@name",
                            customer.Name.Trim());

                        command.Parameters.AddWithValue(
                            "@phone",
                            string.IsNullOrWhiteSpace(customer.Phone)
                                ? DBNull.Value
                                : customer.Phone.Trim());

                        command.Parameters.AddWithValue(
                            "@cnic",
                            string.IsNullOrWhiteSpace(customer.CNIC)
                                ? DBNull.Value
                                : customer.CNIC.Trim());

                        command.Parameters.AddWithValue(
                            "@address",
                            string.IsNullOrWhiteSpace(customer.Address)
                                ? DBNull.Value
                                : customer.Address.Trim());

                        int affectedRows =
                            command.ExecuteNonQuery();

                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "ADD CUSTOMER ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // UPDATE CUSTOMER
        // ============================================================

        public static bool Update(
            Customer customer,
            int pharmacyId)
        {
            if (customer == null ||
                customer.Id <= 0 ||
                pharmacyId <= 0)
            {
                return false;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();

                    string query = @"
                        UPDATE customers
                        SET
                            name = @name,
                            phone = @phone,
                            cnic = @cnic,
                            address = @address
                        WHERE id = @id
                          AND pharmacy_id = @pharmacyId";

                    using (MySqlCommand command =
                        new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id",
                            customer.Id);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        command.Parameters.AddWithValue(
                            "@name",
                            customer.Name.Trim());

                        command.Parameters.AddWithValue(
                            "@phone",
                            string.IsNullOrWhiteSpace(customer.Phone)
                                ? DBNull.Value
                                : customer.Phone.Trim());

                        command.Parameters.AddWithValue(
                            "@cnic",
                            string.IsNullOrWhiteSpace(customer.CNIC)
                                ? DBNull.Value
                                : customer.CNIC.Trim());

                        command.Parameters.AddWithValue(
                            "@address",
                            string.IsNullOrWhiteSpace(customer.Address)
                                ? DBNull.Value
                                : customer.Address.Trim());

                        int affectedRows =
                            command.ExecuteNonQuery();

                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "UPDATE CUSTOMER ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // DELETE CUSTOMER
        // ============================================================

        public static bool Delete(
            int id,
            int pharmacyId)
        {
            if (id <= 0 ||
                pharmacyId <= 0)
            {
                return false;
            }

            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    // ====================================================
                    // CHECK SALES
                    // ====================================================

                    string salesQuery = @"
                        SELECT COUNT(*)
                        FROM sales
                        WHERE customer_id = @id
                          AND pharmacy_id = @pharmacyId";

                    using (MySqlCommand salesCommand =
                        new MySqlCommand(
                            salesQuery,
                            connection))
                    {
                        salesCommand.Parameters.AddWithValue(
                            "@id",
                            id);

                        salesCommand.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        int salesCount =
                            Convert.ToInt32(
                                salesCommand.ExecuteScalar());

                        if (salesCount > 0)
                        {
                            Console.WriteLine(
                                "CUSTOMER DELETE BLOCKED: " +
                                "Customer has sales history.");

                            return false;
                        }
                    }


                    // ====================================================
                    // CHECK CUSTOMER PAYMENTS
                    //
                    // customer_payments does not contain pharmacy_id.
                    // We therefore check through the related sale.
                    // ====================================================

                    string paymentsQuery = @"
                        SELECT COUNT(*)
                        FROM customer_payments cp
                        INNER JOIN sales s
                            ON cp.sale_id = s.id
                        WHERE cp.customer_id = @id
                          AND s.pharmacy_id = @pharmacyId";

                    using (MySqlCommand paymentsCommand =
                        new MySqlCommand(
                            paymentsQuery,
                            connection))
                    {
                        paymentsCommand.Parameters.AddWithValue(
                            "@id",
                            id);

                        paymentsCommand.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        int paymentCount =
                            Convert.ToInt32(
                                paymentsCommand.ExecuteScalar());

                        if (paymentCount > 0)
                        {
                            Console.WriteLine(
                                "CUSTOMER DELETE BLOCKED: " +
                                "Customer has payment history.");

                            return false;
                        }
                    }


                    // ====================================================
                    // DELETE CUSTOMER
                    // ====================================================

                    string deleteQuery = @"
                        DELETE FROM customers
                        WHERE id = @id
                          AND pharmacy_id = @pharmacyId";

                    using (MySqlCommand deleteCommand =
                        new MySqlCommand(
                            deleteQuery,
                            connection))
                    {
                        deleteCommand.Parameters.AddWithValue(
                            "@id",
                            id);

                        deleteCommand.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);

                        int affectedRows =
                            deleteCommand.ExecuteNonQuery();

                        Console.WriteLine(
                            "CUSTOMER DELETE AFFECTED ROWS: " +
                            affectedRows);

                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "DELETE CUSTOMER ERROR: " +
                    ex.Message);

                return false;
            }
        }
    }
}