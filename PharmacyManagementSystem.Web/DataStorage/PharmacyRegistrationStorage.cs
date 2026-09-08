using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class PharmacyRegistrationStorage
    {
        private static readonly PasswordHasher<User>
            PasswordHasher =
                new PasswordHasher<User>();


        // ============================================================
        // REGISTER PHARMACY + ADMIN
        // ============================================================

        public static int RegisterPharmacy(
            PharmacySettings pharmacy,
            string adminUsername,
            string password)
        {
            if (pharmacy == null ||
                string.IsNullOrWhiteSpace(
                    pharmacy.PharmacyName) ||
                string.IsNullOrWhiteSpace(
                    adminUsername) ||
                string.IsNullOrWhiteSpace(
                    password))
            {
                return 0;
            }


            using MySqlConnection connection =
                DatabaseConnection.GetConnection();

            connection.Open();


            using MySqlTransaction transaction =
                connection.BeginTransaction();


            try
            {
                // ====================================================
                // CHECK USERNAME
                // ====================================================

                string usernameCheckQuery = @"
                    SELECT COUNT(*)
                    FROM users
                    WHERE username = @username";


                using (
                    MySqlCommand usernameCheck =
                        new MySqlCommand(
                            usernameCheckQuery,
                            connection,
                            transaction))
                {
                    usernameCheck.Parameters.AddWithValue(
                        "@username",
                        adminUsername.Trim());


                    int count =
                        Convert.ToInt32(
                            usernameCheck.ExecuteScalar());


                    if (count > 0)
                    {
                        transaction.Rollback();

                        return 0;
                    }
                }


                // ====================================================
                // INSERT PHARMACY
                // ====================================================

                string pharmacyQuery = @"
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


                int pharmacyId;


                using (
                    MySqlCommand pharmacyCommand =
                        new MySqlCommand(
                            pharmacyQuery,
                            connection,
                            transaction))
                {
                    pharmacyCommand.Parameters.AddWithValue(
                        "@name",
                        pharmacy.PharmacyName.Trim());

                    pharmacyCommand.Parameters.AddWithValue(
                        "@phone",
                        string.IsNullOrWhiteSpace(
                            pharmacy.Phone)
                            ? DBNull.Value
                            : pharmacy.Phone.Trim());

                    pharmacyCommand.Parameters.AddWithValue(
                        "@address",
                        string.IsNullOrWhiteSpace(
                            pharmacy.Address)
                            ? DBNull.Value
                            : pharmacy.Address.Trim());

                    pharmacyCommand.Parameters.AddWithValue(
                        "@currency",
                        string.IsNullOrWhiteSpace(
                            pharmacy.Currency)
                            ? "Rs."
                            : pharmacy.Currency.Trim());

                    pharmacyCommand.Parameters.AddWithValue(
                        "@low_stock_threshold",
                        pharmacy.LowStockThreshold < 1
                            ? 10
                            : pharmacy.LowStockThreshold);

                    pharmacyCommand.Parameters.AddWithValue(
                        "@date_format",
                        string.IsNullOrWhiteSpace(
                            pharmacy.DateFormat)
                            ? "dd/MM/yyyy"
                            : pharmacy.DateFormat);


                    pharmacyId =
                        Convert.ToInt32(
                            pharmacyCommand.ExecuteScalar());
                }


                if (pharmacyId <= 0)
                {
                    transaction.Rollback();

                    return 0;
                }


                // ====================================================
                // CREATE ADMIN USER
                // ====================================================

                User admin =
                    new User
                    {
                        PharmacyId =
                            pharmacyId,

                        Username =
                            adminUsername.Trim(),

                        Role =
                            "Admin",

                        IsActive =
                            true
                    };


                admin.PasswordHash =
                    PasswordHasher.HashPassword(
                        admin,
                        password);


                string userQuery = @"
                    INSERT INTO users
                    (
                        pharmacy_id,
                        username,
                        password_hash,
                        role,
                        is_active
                    )
                    VALUES
                    (
                        @pharmacy_id,
                        @username,
                        @password_hash,
                        @role,
                        @is_active
                    )";


                using (
                    MySqlCommand userCommand =
                        new MySqlCommand(
                            userQuery,
                            connection,
                            transaction))
                {
                    userCommand.Parameters.AddWithValue(
                        "@pharmacy_id",
                        pharmacyId);

                    userCommand.Parameters.AddWithValue(
                        "@username",
                        admin.Username);

                    userCommand.Parameters.AddWithValue(
                        "@password_hash",
                        admin.PasswordHash);

                    userCommand.Parameters.AddWithValue(
                        "@role",
                        "Admin");

                    userCommand.Parameters.AddWithValue(
                        "@is_active",
                        true);


                    int rows =
                        userCommand.ExecuteNonQuery();


                    if (rows <= 0)
                    {
                        transaction.Rollback();

                        return 0;
                    }
                }


                // ====================================================
                // COMPLETE
                // ====================================================

                transaction.Commit();

                return pharmacyId;
            }
            catch (Exception ex)
            {
                try
                {
                    transaction.Rollback();
                }
                catch
                {
                    // Ignore rollback errors.
                }


                Console.WriteLine(
                    "REGISTER PHARMACY ERROR: " +
                    ex.Message);


                return 0;
            }
        }
    }
}