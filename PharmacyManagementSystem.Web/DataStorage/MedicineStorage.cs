using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class MedicineStorage
    {
        // =========================================================
        // LOAD
        // =========================================================

        public static List<Medicine> Load(int pharmacyId)
        {
            var medicines = new List<Medicine>();

            const string sql = @"
                SELECT
                    id,
                    company_medicine_id,
                    name,
                    category,
                    company,
                    formula,
                    mg,
                    price,
                    quantity,
                    expiry_date,
                    created_at,
                    edited_at,
                    pharmacy_id
                FROM medicines
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
                    medicines.Add(MapMedicine(reader));
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MedicineStorage.Load Error: {ex.Message}"
                );
            }

            return medicines;
        }

        // =========================================================
        // SEARCH FOR PURCHASE
        // =========================================================

        public static List<Medicine> Search(
            int pharmacyId,
            string searchTerm,
            int limit = 30)
        {
            var medicines = new List<Medicine>();

            if (string.IsNullOrWhiteSpace(searchTerm))
                return medicines;

            searchTerm = searchTerm.Trim();

            if (searchTerm.Length < 1)
                return medicines;

            limit = Math.Clamp(limit, 1, 50);

            const string sql = @"
                SELECT
                    id,
                    company_medicine_id,
                    name,
                    category,
                    company,
                    formula,
                    mg,
                    price,
                    quantity,
                    expiry_date,
                    created_at,
                    edited_at,
                    pharmacy_id
                FROM medicines
                WHERE pharmacy_id = @pharmacy_id
                  AND
                  (
                      name LIKE @search
                      OR company LIKE @search
                      OR company_medicine_id LIKE @search
                      OR category LIKE @search
                  )
                ORDER BY name ASC
                LIMIT @limit;
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
                    "@search",
                    $"%{searchTerm}%"
                );

                command.Parameters.AddWithValue(
                    "@limit",
                    limit
                );

                using var reader =
                    command.ExecuteReader();

                while (reader.Read())
                {
                    medicines.Add(
                        MapMedicine(reader)
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MedicineStorage.Search Error: {ex.Message}"
                );
            }

            return medicines;
        }

        // =========================================================
        // GET BY ID
        // =========================================================

        public static Medicine? GetById(
            int id,
            int pharmacyId)
        {
            const string sql = @"
                SELECT
                    id,
                    company_medicine_id,
                    name,
                    category,
                    company,
                    formula,
                    mg,
                    price,
                    quantity,
                    expiry_date,
                    created_at,
                    edited_at,
                    pharmacy_id
                FROM medicines
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

                command.Parameters.AddWithValue(
                    "@id",
                    id
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                using var reader =
                    command.ExecuteReader();

                if (reader.Read())
                    return MapMedicine(reader);
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MedicineStorage.GetById Error: {ex.Message}"
                );
            }

            return null;
        }

        // =========================================================
        // ADD
        // =========================================================

        public static bool Add(
            Medicine medicine,
            int pharmacyId)
        {
            const string sql = @"
                INSERT INTO medicines
                (
                    company_medicine_id,
                    name,
                    category,
                    company,
                    formula,
                    mg,
                    price,
                    quantity,
                    expiry_date,
                    created_at,
                    pharmacy_id
                )
                VALUES
                (
                    @company_medicine_id,
                    @name,
                    @category,
                    @company,
                    @formula,
                    @mg,
                    @price,
                    @quantity,
                    @expiry_date,
                    @created_at,
                    @pharmacy_id
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
                    "@company_medicine_id",
                    string.IsNullOrWhiteSpace(
                        medicine.CompanyMedicineId)
                        ? DBNull.Value
                        : medicine.CompanyMedicineId.Trim()
                );

                command.Parameters.AddWithValue(
                    "@name",
                    medicine.Name.Trim()
                );

                command.Parameters.AddWithValue(
                    "@category",
                    string.IsNullOrWhiteSpace(
                        medicine.Category)
                        ? DBNull.Value
                        : medicine.Category.Trim()
                );

                command.Parameters.AddWithValue(
                    "@company",
                    string.IsNullOrWhiteSpace(
                        medicine.Company)
                        ? DBNull.Value
                        : medicine.Company.Trim()
                );

                command.Parameters.AddWithValue(
                    "@formula",
                    string.IsNullOrWhiteSpace(
                        medicine.Formula)
                        ? DBNull.Value
                        : medicine.Formula.Trim()
                );

                command.Parameters.AddWithValue(
                    "@mg",
                    string.IsNullOrWhiteSpace(
                        medicine.Mg)
                        ? DBNull.Value
                        : medicine.Mg.Trim()
                );

                command.Parameters.AddWithValue(
                    "@price",
                    medicine.Price
                );

                command.Parameters.AddWithValue(
                    "@quantity",
                    medicine.Quantity
                );

                command.Parameters.AddWithValue(
                    "@expiry_date",
                    medicine.ExpiryDate.HasValue
                        ? medicine.ExpiryDate.Value
                        : DBNull.Value
                );

                command.Parameters.AddWithValue(
                    "@created_at",
                    DateTime.Now
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MedicineStorage.Add Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // UPDATE
        // =========================================================

        public static bool Update(
            Medicine medicine,
            int pharmacyId)
        {
            const string sql = @"
                UPDATE medicines
                SET
                    company_medicine_id = @company_medicine_id,
                    name = @name,
                    category = @category,
                    company = @company,
                    formula = @formula,
                    mg = @mg,
                    price = @price,
                    quantity = @quantity,
                    expiry_date = @expiry_date,
                    edited_at = @edited_at
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
                    medicine.Id
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                command.Parameters.AddWithValue(
                    "@company_medicine_id",
                    string.IsNullOrWhiteSpace(
                        medicine.CompanyMedicineId)
                        ? DBNull.Value
                        : medicine.CompanyMedicineId.Trim()
                );

                command.Parameters.AddWithValue(
                    "@name",
                    medicine.Name.Trim()
                );

                command.Parameters.AddWithValue(
                    "@category",
                    string.IsNullOrWhiteSpace(
                        medicine.Category)
                        ? DBNull.Value
                        : medicine.Category.Trim()
                );

                command.Parameters.AddWithValue(
                    "@company",
                    string.IsNullOrWhiteSpace(
                        medicine.Company)
                        ? DBNull.Value
                        : medicine.Company.Trim()
                );

                command.Parameters.AddWithValue(
                    "@formula",
                    string.IsNullOrWhiteSpace(
                        medicine.Formula)
                        ? DBNull.Value
                        : medicine.Formula.Trim()
                );

                command.Parameters.AddWithValue(
                    "@mg",
                    string.IsNullOrWhiteSpace(
                        medicine.Mg)
                        ? DBNull.Value
                        : medicine.Mg.Trim()
                );

                command.Parameters.AddWithValue(
                    "@price",
                    medicine.Price
                );

                command.Parameters.AddWithValue(
                    "@quantity",
                    medicine.Quantity
                );

                command.Parameters.AddWithValue(
                    "@expiry_date",
                    medicine.ExpiryDate.HasValue
                        ? medicine.ExpiryDate.Value
                        : DBNull.Value
                );

                command.Parameters.AddWithValue(
                    "@edited_at",
                    DateTime.Now
                );

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MedicineStorage.Update Error: {ex.Message}"
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
                DELETE FROM medicines
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
                    id
                );

                command.Parameters.AddWithValue(
                    "@pharmacy_id",
                    pharmacyId
                );

                return command.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    $"MedicineStorage.Delete Error: {ex.Message}"
                );

                return false;
            }
        }

        // =========================================================
        // MAP
        // =========================================================

        private static Medicine MapMedicine(
            MySqlDataReader reader)
        {
            return new Medicine
            {
                Id = Convert.ToInt32(
                    reader["id"]
                ),

                CompanyMedicineId =
                    reader["company_medicine_id"] == DBNull.Value
                        ? null
                        : reader["company_medicine_id"]?.ToString(),

                Name =
                    reader["name"]?.ToString()
                    ?? string.Empty,

                Category =
                    reader["category"] == DBNull.Value
                        ? string.Empty
                        : reader["category"]?.ToString()
                            ?? string.Empty,

                Company =
                    reader["company"] == DBNull.Value
                        ? string.Empty
                        : reader["company"]?.ToString()
                            ?? string.Empty,

                Formula =
                    reader["formula"] == DBNull.Value
                        ? string.Empty
                        : reader["formula"]?.ToString()
                            ?? string.Empty,

                Mg =
                    reader["mg"] == DBNull.Value
                        ? string.Empty
                        : reader["mg"]?.ToString()
                            ?? string.Empty,

                Price = Convert.ToDecimal(
                    reader["price"]
                ),

                Quantity = Convert.ToInt32(
                    reader["quantity"]
                ),

                ExpiryDate =
                    reader["expiry_date"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["expiry_date"]
                        ),

                CreatedAt =
                    Convert.ToDateTime(
                        reader["created_at"]
                    ),

                EditedAt =
                    reader["edited_at"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(
                            reader["edited_at"]
                        )
            };
        }
    }
}