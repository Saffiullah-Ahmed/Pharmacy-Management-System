using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class PharmacyStorage
    {
        // ==========================================================
        // CREATE PHARMACY
        // ==========================================================

        public static int Create(
            PharmacySettings settings)
        {
            if (settings == null ||
                string.IsNullOrWhiteSpace(
                    settings.PharmacyName))
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
                        INSERT INTO pharmacies
                        (
                            name,
                            phone,
                            address,
                            currency,
                            low_stock_threshold,
                            date_format
                        )
                        VALUES
                        (
                            @name,
                            @phone,
                            @address,
                            @currency,
                            @low_stock_threshold,
                            @date_format
                        );

                        SELECT LAST_INSERT_ID();";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@name",
                            settings.PharmacyName.Trim());

                        command.Parameters.AddWithValue(
                            "@phone",
                            string.IsNullOrWhiteSpace(
                                settings.Phone)
                                ? DBNull.Value
                                : settings.Phone.Trim());

                        command.Parameters.AddWithValue(
                            "@address",
                            string.IsNullOrWhiteSpace(
                                settings.Address)
                                ? DBNull.Value
                                : settings.Address.Trim());

                        command.Parameters.AddWithValue(
                            "@currency",
                            GetSafeCurrency(
                                settings.Currency));

                        command.Parameters.AddWithValue(
                            "@low_stock_threshold",
                            GetSafeLowStockThreshold(
                                settings.LowStockThreshold));

                        command.Parameters.AddWithValue(
                            "@date_format",
                            GetSafeDateFormat(
                                settings.DateFormat));

                        object? result =
                            command.ExecuteScalar();

                        if (result == null ||
                            result == DBNull.Value)
                        {
                            return 0;
                        }

                        return Convert.ToInt32(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "CREATE PHARMACY ERROR: " +
                    ex.Message);

                return 0;
            }
        }


        // ==========================================================
        // GET PHARMACY SETTINGS
        // ==========================================================

        public static PharmacySettings? GetById(
            int pharmacyId)
        {
            if (pharmacyId <= 0)
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
                            address,
                            currency,
                            low_stock_threshold,
                            date_format
                        FROM pharmacies
                        WHERE id = @id
                        LIMIT 1";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id",
                            pharmacyId);

                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                            {
                                return null;
                            }

                            return MapPharmacy(
                                reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET PHARMACY ERROR: " +
                    ex.Message);

                return null;
            }
        }


        // ==========================================================
        // UPDATE PHARMACY SETTINGS
        // ==========================================================

        public static bool Update(
            PharmacySettings settings)
        {
            if (settings == null ||
                settings.PharmacyId <= 0 ||
                string.IsNullOrWhiteSpace(
                    settings.PharmacyName))
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
                        UPDATE pharmacies
                        SET
                            name = @name,
                            phone = @phone,
                            address = @address,
                            currency = @currency,
                            low_stock_threshold =
                                @low_stock_threshold,
                            date_format = @date_format
                        WHERE id = @id";

                    using (MySqlCommand command =
                        new MySqlCommand(
                            query,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id",
                            settings.PharmacyId);

                        command.Parameters.AddWithValue(
                            "@name",
                            settings.PharmacyName.Trim());

                        command.Parameters.AddWithValue(
                            "@phone",
                            string.IsNullOrWhiteSpace(
                                settings.Phone)
                                ? DBNull.Value
                                : settings.Phone.Trim());

                        command.Parameters.AddWithValue(
                            "@address",
                            string.IsNullOrWhiteSpace(
                                settings.Address)
                                ? DBNull.Value
                                : settings.Address.Trim());

                        command.Parameters.AddWithValue(
                            "@currency",
                            GetSafeCurrency(
                                settings.Currency));

                        command.Parameters.AddWithValue(
                            "@low_stock_threshold",
                            GetSafeLowStockThreshold(
                                settings.LowStockThreshold));

                        command.Parameters.AddWithValue(
                            "@date_format",
                            GetSafeDateFormat(
                                settings.DateFormat));

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "UPDATE PHARMACY ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ==========================================================
        // MAP DATABASE RECORD
        // ==========================================================

        private static PharmacySettings MapPharmacy(
            MySqlDataReader reader)
        {
            int phoneOrdinal =
                reader.GetOrdinal("phone");

            int addressOrdinal =
                reader.GetOrdinal("address");

            int currencyOrdinal =
                reader.GetOrdinal("currency");

            int lowStockOrdinal =
                reader.GetOrdinal(
                    "low_stock_threshold");

            int dateFormatOrdinal =
                reader.GetOrdinal(
                    "date_format");


            return new PharmacySettings
            {
                PharmacyId =
                    reader.GetInt32("id"),

                PharmacyName =
                    reader.IsDBNull(
                        reader.GetOrdinal("name"))
                        ? ""
                        : reader.GetString("name"),

                Phone =
                    reader.IsDBNull(phoneOrdinal)
                        ? ""
                        : reader.GetString(phoneOrdinal),

                Address =
                    reader.IsDBNull(addressOrdinal)
                        ? ""
                        : reader.GetString(addressOrdinal),

                Currency =
                    reader.IsDBNull(currencyOrdinal)
                        ? "Rs."
                        : reader.GetString(currencyOrdinal),

                LowStockThreshold =
                    reader.IsDBNull(lowStockOrdinal)
                        ? 10
                        : reader.GetInt32(
                            lowStockOrdinal),

                DateFormat =
                    reader.IsDBNull(dateFormatOrdinal)
                        ? "dd/MM/yyyy"
                        : reader.GetString(
                            dateFormatOrdinal)
            };
        }


        // ==========================================================
        // SAFE CURRENCY
        // ==========================================================

        private static string GetSafeCurrency(
            string? currency)
        {
            string[] allowedCurrencies =
            {
                "Rs.",
                "$",
                "€",
                "£"
            };

            if (string.IsNullOrWhiteSpace(currency))
            {
                return "Rs.";
            }

            if (!allowedCurrencies.Contains(
                    currency.Trim()))
            {
                return "Rs.";
            }

            return currency.Trim();
        }


        // ==========================================================
        // SAFE LOW STOCK THRESHOLD
        // ==========================================================

        private static int GetSafeLowStockThreshold(
            int threshold)
        {
            if (threshold < 1)
            {
                return 10;
            }

            if (threshold > 10000)
            {
                return 10000;
            }

            return threshold;
        }


        // ==========================================================
        // SAFE DATE FORMAT
        // ==========================================================

        private static string GetSafeDateFormat(
            string? dateFormat)
        {
            string[] allowedFormats =
            {
                "dd/MM/yyyy",
                "MM/dd/yyyy",
                "yyyy-MM-dd"
            };

            if (string.IsNullOrWhiteSpace(
                    dateFormat))
            {
                return "dd/MM/yyyy";
            }

            if (!allowedFormats.Contains(
                    dateFormat.Trim()))
            {
                return "dd/MM/yyyy";
            }

            return dateFormat.Trim();
        }
    }
}