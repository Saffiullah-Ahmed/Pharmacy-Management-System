using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class SupplierStorage
    {
        // =========================================================
        // LOAD ALL SUPPLIERS FOR CURRENT PHARMACY
        // =========================================================

        public static List<Supplier> Load(int pharmacyId)
        {
            var suppliers = new List<Supplier>();

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    name,
                    phone,
                    email,
                    address,
                    company
                FROM suppliers
                WHERE pharmacy_id = @pharmacy_id
                ORDER BY name ASC;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    suppliers.Add(MapSupplier(reader));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SupplierStorage.Load Error: {ex.Message}"
                );
            }

            return suppliers;
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public static Supplier? GetById(
            int id,
            int pharmacyId)
        {
            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    name,
                    phone,
                    email,
                    address,
                    company
                FROM suppliers
                WHERE id = @id
                  AND pharmacy_id = @pharmacy_id
                LIMIT 1;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader = command.ExecuteReader();

                if (reader.Read())
                    return MapSupplier(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SupplierStorage.GetById Error: {ex.Message}"
                );
            }

            return null;
        }

        // =========================================================
        // GET BY NAME
        // =========================================================

        public static Supplier? GetByName(
            string name,
            int pharmacyId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            const string sql = @"
                SELECT
                    id,
                    pharmacy_id,
                    name,
                    phone,
                    email,
                    address,
                    company
                FROM suppliers
                WHERE pharmacy_id = @pharmacy_id
                  AND LOWER(TRIM(name)) = LOWER(TRIM(@name))
                LIMIT 1;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                command.Parameters.AddWithValue(
                    "@name",
                    name.Trim()
                );

                using var reader = command.ExecuteReader();

                if (reader.Read())
                    return MapSupplier(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SupplierStorage.GetByName Error: {ex.Message}"
                );
            }

            return null;
        }

        // =========================================================
        // ADD
        // =========================================================

        public static bool Add(
            Supplier supplier,
            int pharmacyId)
        {
            const string sql = @"
                INSERT INTO suppliers
                (
                    pharmacy_id,
                    name,
                    phone,
                    email,
                    address,
                    company
                )
                VALUES
                (
                    @pharmacy_id,
                    @name,
                    @phone,
                    @email,
                    @address,
                    @company
                );
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                command.Parameters.AddWithValue(
                    "@name",
                    supplier.Name.Trim()
                );

                command.Parameters.AddWithValue(
                    "@phone",
                    supplier.Phone.Trim()
                );

                command.Parameters.AddWithValue(
                    "@email",
                    string.IsNullOrWhiteSpace(supplier.Email)
                        ? DBNull.Value
                        : supplier.Email.Trim()
                );

                command.Parameters.AddWithValue(
                    "@address",
                    string.IsNullOrWhiteSpace(supplier.Address)
                        ? DBNull.Value
                        : supplier.Address.Trim()
                );

                command.Parameters.AddWithValue(
                    "@company",
                    string.IsNullOrWhiteSpace(supplier.Company)
                        ? DBNull.Value
                        : supplier.Company.Trim()
                );

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SupplierStorage.Add Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // ADD FROM PURCHASE
        // =========================================================

        public static Supplier? AddFromPurchase(
            string supplierName,
            int pharmacyId)
        {
            if (string.IsNullOrWhiteSpace(supplierName))
                return null;

            supplierName = supplierName.Trim();

            var existing = GetByName(
                supplierName,
                pharmacyId
            );

            if (existing != null)
                return existing;

            var supplier = new Supplier
            {
                Name = supplierName,
                Phone = "N/A",
                Email = string.Empty,
                Address = string.Empty,
                Company = string.Empty
            };

            if (!Add(
                    supplier,
                    pharmacyId))
            {
                return null;
            }

            return GetByName(
                supplierName,
                pharmacyId
            );
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public static bool Update(
            Supplier supplier,
            int pharmacyId)
        {
            const string sql = @"
                UPDATE suppliers
                SET
                    name = @name,
                    phone = @phone,
                    email = @email,
                    address = @address,
                    company = @company
                WHERE id = @id
                  AND pharmacy_id = @pharmacy_id;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@id",
                    supplier.Id
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                command.Parameters.AddWithValue(
                    "@name",
                    supplier.Name.Trim()
                );

                command.Parameters.AddWithValue(
                    "@phone",
                    supplier.Phone.Trim()
                );

                command.Parameters.AddWithValue(
                    "@email",
                    string.IsNullOrWhiteSpace(supplier.Email)
                        ? DBNull.Value
                        : supplier.Email.Trim()
                );

                command.Parameters.AddWithValue(
                    "@address",
                    string.IsNullOrWhiteSpace(supplier.Address)
                        ? DBNull.Value
                        : supplier.Address.Trim()
                );

                command.Parameters.AddWithValue(
                    "@company",
                    string.IsNullOrWhiteSpace(supplier.Company)
                        ? DBNull.Value
                        : supplier.Company.Trim()
                );

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SupplierStorage.Update Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // DELETE
        // =========================================================

        public static bool Delete(
            int id,
            int pharmacyId)
        {
            const string sql = @"
                DELETE FROM suppliers
                WHERE id = @id
                  AND pharmacy_id = @pharmacy_id;
            ";

            try
            {
                using var connection =
                    DatabaseConnection.GetConnection();

                connection.Open();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue("@id", id);
                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"SupplierStorage.Delete Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // MAP
        // =========================================================

        private static Supplier MapSupplier(
            MySqlDataReader reader)
        {
            return new Supplier
            {
                Id = Convert.ToInt32(
                    reader["id"]
                ),

                Name = reader["name"]?.ToString()
                    ?? string.Empty,

                Phone = reader["phone"]?.ToString()
                    ?? string.Empty,

                Email = reader["email"] == DBNull.Value
                    ? string.Empty
                    : reader["email"]?.ToString()
                        ?? string.Empty,

                Address = reader["address"] == DBNull.Value
                    ? string.Empty
                    : reader["address"]?.ToString()
                        ?? string.Empty,

                Company = reader["company"] == DBNull.Value
                    ? string.Empty
                    : reader["company"]?.ToString()
                        ?? string.Empty
            };
        }
    }
}