using Microsoft.AspNetCore.Identity;
using MySql.Data.MySqlClient;
using PharmacyManagementSystem.Web.Database;
using PharmacyManagementSystem.Web.Models;

namespace PharmacyManagementSystem.Web.DataStorage
{
    public static class UserStorage
    {
        private static readonly PasswordHasher<User> passwordHasher =
            new PasswordHasher<User>();


        // ============================================================
        // ADD USER
        // ============================================================

        public static bool Add(User user, string password)
        {
            if (user == null)
                return false;

            if (user.PharmacyId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(user.Username))
                return false;

            if (string.IsNullOrWhiteSpace(password))
                return false;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    // Username is globally UNIQUE.
                    const string duplicateSql = @"
                        SELECT COUNT(*)
                        FROM users
                        WHERE username = @username";


                    using (MySqlCommand duplicateCommand =
                        new MySqlCommand(
                            duplicateSql,
                            connection))
                    {
                        duplicateCommand.Parameters.AddWithValue(
                            "@username",
                            user.Username.Trim());

                        int count =
                            Convert.ToInt32(
                                duplicateCommand.ExecuteScalar());

                        if (count > 0)
                            return false;
                    }


                    user.Username =
                        user.Username.Trim();

                    user.PasswordHash =
                        passwordHasher.HashPassword(
                            user,
                            password);

                    user.Role =
                        string.IsNullOrWhiteSpace(user.Role)
                            ? "Staff"
                            : user.Role.Trim();

                    user.IsActive = true;


                    const string sql = @"
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
                            @pharmacyId,
                            @username,
                            @passwordHash,
                            @role,
                            @isActive
                        )";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            user.PharmacyId);

                        command.Parameters.AddWithValue(
                            "@username",
                            user.Username);

                        command.Parameters.AddWithValue(
                            "@passwordHash",
                            user.PasswordHash);

                        command.Parameters.AddWithValue(
                            "@role",
                            user.Role);

                        command.Parameters.AddWithValue(
                            "@isActive",
                            user.IsActive);


                        int affectedRows =
                            command.ExecuteNonQuery();

                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "ADD USER ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // CREATE ADMIN
        // ============================================================

        public static bool CreateAdmin(
            User user,
            string password)
        {
            if (user == null)
                return false;

            if (user.PharmacyId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(user.Username))
                return false;

            if (string.IsNullOrWhiteSpace(password))
                return false;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string duplicateSql = @"
                        SELECT COUNT(*)
                        FROM users
                        WHERE username = @username";


                    using (MySqlCommand duplicateCommand =
                        new MySqlCommand(
                            duplicateSql,
                            connection))
                    {
                        duplicateCommand.Parameters.AddWithValue(
                            "@username",
                            user.Username.Trim());

                        int count =
                            Convert.ToInt32(
                                duplicateCommand.ExecuteScalar());

                        if (count > 0)
                            return false;
                    }


                    user.Username =
                        user.Username.Trim();

                    user.Role = "Admin";

                    user.IsActive = true;

                    user.PasswordHash =
                        passwordHasher.HashPassword(
                            user,
                            password);


                    const string sql = @"
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
                            @pharmacyId,
                            @username,
                            @passwordHash,
                            @role,
                            @isActive
                        )";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            user.PharmacyId);

                        command.Parameters.AddWithValue(
                            "@username",
                            user.Username);

                        command.Parameters.AddWithValue(
                            "@passwordHash",
                            user.PasswordHash);

                        command.Parameters.AddWithValue(
                            "@role",
                            user.Role);

                        command.Parameters.AddWithValue(
                            "@isActive",
                            user.IsActive);


                        int affectedRows =
                            command.ExecuteNonQuery();

                        return affectedRows > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "CREATE ADMIN ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // GET USER BY USERNAME
        // ============================================================

        public static User? GetByUsername(
            string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT
                            u.id,
                            u.pharmacy_id,
                            u.username,
                            u.password_hash,
                            u.role,
                            u.is_active,
                            u.created_at,
                            p.name AS pharmacy_name
                        FROM users u
                        LEFT JOIN pharmacies p
                            ON p.id = u.pharmacy_id
                        WHERE u.username = @username
                        LIMIT 1";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@username",
                            username.Trim());


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                                return null;


                            return MapUser(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET USER BY USERNAME ERROR: " +
                    ex.Message);

                return null;
            }
        }


        // ============================================================
        // GET USER BY USERNAME FOR PHARMACY
        // ============================================================

        public static User? GetByUsernameForPharmacy(
            string username,
            int pharmacyId)
        {
            if (string.IsNullOrWhiteSpace(username))
                return null;

            if (pharmacyId <= 0)
                return null;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT
                            u.id,
                            u.pharmacy_id,
                            u.username,
                            u.password_hash,
                            u.role,
                            u.is_active,
                            u.created_at,
                            p.name AS pharmacy_name
                        FROM users u
                        LEFT JOIN pharmacies p
                            ON p.id = u.pharmacy_id
                        WHERE
                            u.username = @username
                            AND u.pharmacy_id = @pharmacyId
                        LIMIT 1";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@username",
                            username.Trim());

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                                return null;


                            return MapUser(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET USER BY PHARMACY ERROR: " +
                    ex.Message);

                return null;
            }
        }


        // ============================================================
        // GET USER BY ID
        // ============================================================

        // Kept for compatibility with existing application code.

        public static User? GetById(int id)
        {
            if (id <= 0)
                return null;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT
                            u.id,
                            u.pharmacy_id,
                            u.username,
                            u.password_hash,
                            u.role,
                            u.is_active,
                            u.created_at,
                            p.name AS pharmacy_name
                        FROM users u
                        LEFT JOIN pharmacies p
                            ON p.id = u.pharmacy_id
                        WHERE u.id = @id
                        LIMIT 1";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id",
                            id);


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                                return null;


                            return MapUser(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET USER BY ID ERROR: " +
                    ex.Message);

                return null;
            }
        }


        // ============================================================
        // GET USER BY ID FOR PHARMACY
        // ============================================================

        public static User? GetByIdForPharmacy(
            int userId,
            int pharmacyId)
        {
            if (userId <= 0)
                return null;

            if (pharmacyId <= 0)
                return null;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT
                            u.id,
                            u.pharmacy_id,
                            u.username,
                            u.password_hash,
                            u.role,
                            u.is_active,
                            u.created_at,
                            p.name AS pharmacy_name
                        FROM users u
                        LEFT JOIN pharmacies p
                            ON p.id = u.pharmacy_id
                        WHERE
                            u.id = @id
                            AND u.pharmacy_id = @pharmacyId
                        LIMIT 1";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@id",
                            userId);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            if (!reader.Read())
                                return null;


                            return MapUser(reader);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET USER BY ID FOR PHARMACY ERROR: " +
                    ex.Message);

                return null;
            }
        }


        // ============================================================
        // GET ALL USERS FOR PHARMACY
        // ============================================================

        public static List<User> GetByPharmacyId(
            int pharmacyId)
        {
            List<User> users =
                new List<User>();


            if (pharmacyId <= 0)
                return users;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT
                            u.id,
                            u.pharmacy_id,
                            u.username,
                            u.password_hash,
                            u.role,
                            u.is_active,
                            u.created_at,
                            p.name AS pharmacy_name
                        FROM users u
                        LEFT JOIN pharmacies p
                            ON p.id = u.pharmacy_id
                        WHERE u.pharmacy_id = @pharmacyId
                        ORDER BY
                            CASE
                                WHEN LOWER(u.role) = 'admin'
                                THEN 0
                                ELSE 1
                            END,
                            u.username ASC";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(
                                    MapUser(reader));
                            }
                        }
                    }
                }


                foreach (User user in users)
                {
                    user.Permissions =
                        GetPermissionsForPharmacy(
                            user.Id,
                            pharmacyId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET USERS ERROR: " +
                    ex.Message);
            }


            return users;
        }


        // ============================================================
        // VERIFY PASSWORD
        // ============================================================

        public static bool VerifyPassword(
            User user,
            string password)
        {
            if (user == null)
                return false;

            if (string.IsNullOrEmpty(password))
                return false;


            PasswordVerificationResult result =
                passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    password);


            return result ==
                       PasswordVerificationResult.Success
                   ||
                   result ==
                       PasswordVerificationResult.SuccessRehashNeeded;
        }


        // ============================================================
        // CHANGE PASSWORD
        // ============================================================

        // Compatibility method.

        public static bool ChangePassword(
            int userId,
            string newPassword)
        {
            if (userId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(newPassword))
                return false;


            User? user =
                GetById(userId);

            if (user == null)
                return false;


            return ChangePassword(
                userId,
                user.PharmacyId,
                newPassword);
        }


        // ============================================================
        // CHANGE PASSWORD - PHARMACY SCOPED
        // ============================================================

        public static bool ChangePassword(
            int userId,
            int pharmacyId,
            string newPassword)
        {
            if (userId <= 0)
                return false;

            if (pharmacyId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(newPassword))
                return false;


            User? user =
                GetByIdForPharmacy(
                    userId,
                    pharmacyId);

            if (user == null)
                return false;


            string passwordHash =
                passwordHasher.HashPassword(
                    user,
                    newPassword);


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        UPDATE users
                        SET password_hash = @passwordHash
                        WHERE
                            id = @id
                            AND pharmacy_id = @pharmacyId";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@passwordHash",
                            passwordHash);

                        command.Parameters.AddWithValue(
                            "@id",
                            userId);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);


                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "CHANGE PASSWORD ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // DELETE USER
        // ============================================================

        public static bool Delete(
            int userId,
            int pharmacyId)
        {
            if (userId <= 0 ||
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


                    using (MySqlTransaction transaction =
                        connection.BeginTransaction())
                    {
                        try
                        {
                            // Only remove permissions belonging
                            // to a user in this pharmacy.

                            const string permissionSql = @"
                                DELETE FROM user_permissions
                                WHERE user_id = @userId
                                AND EXISTS
                                (
                                    SELECT 1
                                    FROM users
                                    WHERE
                                        users.id =
                                            user_permissions.user_id
                                        AND users.pharmacy_id =
                                            @pharmacyId
                                )";


                            using (MySqlCommand permissionCommand =
                                new MySqlCommand(
                                    permissionSql,
                                    connection,
                                    transaction))
                            {
                                permissionCommand.Parameters.AddWithValue(
                                    "@userId",
                                    userId);

                                permissionCommand.Parameters.AddWithValue(
                                    "@pharmacyId",
                                    pharmacyId);

                                permissionCommand.ExecuteNonQuery();
                            }


                            const string userSql = @"
                                DELETE FROM users
                                WHERE
                                    id = @id
                                    AND pharmacy_id = @pharmacyId";


                            using (MySqlCommand userCommand =
                                new MySqlCommand(
                                    userSql,
                                    connection,
                                    transaction))
                            {
                                userCommand.Parameters.AddWithValue(
                                    "@id",
                                    userId);

                                userCommand.Parameters.AddWithValue(
                                    "@pharmacyId",
                                    pharmacyId);


                                int affectedRows =
                                    userCommand.ExecuteNonQuery();


                                if (affectedRows == 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }


                            transaction.Commit();

                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "DELETE USER ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // SET ACTIVE STATUS
        // ============================================================

        public static bool SetActiveStatus(
            int userId,
            int pharmacyId,
            bool isActive)
        {
            if (userId <= 0 ||
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


                    const string sql = @"
                        UPDATE users
                        SET is_active = @isActive
                        WHERE
                            id = @id
                            AND pharmacy_id = @pharmacyId";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@isActive",
                            isActive);

                        command.Parameters.AddWithValue(
                            "@id",
                            userId);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);


                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "SET USER STATUS ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // GET PERMISSIONS
        // ============================================================

        // Compatibility method.

        public static List<string> GetPermissions(
            int userId)
        {
            List<string> permissions =
                new List<string>();


            if (userId <= 0)
                return permissions;


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT permission_name
                        FROM user_permissions
                        WHERE user_id = @userId
                        ORDER BY permission_name";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@userId",
                            userId);


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                permissions.Add(
                                    reader["permission_name"]
                                        ?.ToString()
                                    ?? "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET PERMISSIONS ERROR: " +
                    ex.Message);
            }


            return permissions;
        }


        // ============================================================
        // GET PERMISSIONS FOR PHARMACY
        // ============================================================

        public static List<string>
            GetPermissionsForPharmacy(
                int userId,
                int pharmacyId)
        {
            List<string> permissions =
                new List<string>();


            if (userId <= 0 ||
                pharmacyId <= 0)
            {
                return permissions;
            }


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT
                            up.permission_name
                        FROM user_permissions up
                        INNER JOIN users u
                            ON u.id = up.user_id
                        WHERE
                            up.user_id = @userId
                            AND u.pharmacy_id = @pharmacyId
                        ORDER BY up.permission_name";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@userId",
                            userId);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            pharmacyId);


                        using (MySqlDataReader reader =
                            command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                permissions.Add(
                                    reader["permission_name"]
                                        ?.ToString()
                                    ?? "");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "GET PHARMACY PERMISSIONS ERROR: " +
                    ex.Message);
            }


            return permissions;
        }


        // ============================================================
        // SET PERMISSIONS
        // ============================================================

        public static bool SetPermissions(
            int userId,
            int pharmacyId,
            List<string> permissions)
        {
            if (userId <= 0 ||
                pharmacyId <= 0)
            {
                return false;
            }


            permissions ??=
                new List<string>();


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    using (MySqlTransaction transaction =
                        connection.BeginTransaction())
                    {
                        try
                        {
                            // Verify that the user belongs
                            // to this pharmacy.

                            const string verifySql = @"
                                SELECT COUNT(*)
                                FROM users
                                WHERE
                                    id = @userId
                                    AND pharmacy_id = @pharmacyId";


                            using (MySqlCommand verifyCommand =
                                new MySqlCommand(
                                    verifySql,
                                    connection,
                                    transaction))
                            {
                                verifyCommand.Parameters.AddWithValue(
                                    "@userId",
                                    userId);

                                verifyCommand.Parameters.AddWithValue(
                                    "@pharmacyId",
                                    pharmacyId);


                                int exists =
                                    Convert.ToInt32(
                                        verifyCommand.ExecuteScalar());


                                if (exists == 0)
                                {
                                    transaction.Rollback();
                                    return false;
                                }
                            }


                            // Delete old permissions.

                            const string deleteSql = @"
                                DELETE FROM user_permissions
                                WHERE user_id = @userId
                                AND EXISTS
                                (
                                    SELECT 1
                                    FROM users
                                    WHERE
                                        users.id =
                                            user_permissions.user_id
                                        AND users.pharmacy_id =
                                            @pharmacyId
                                )";


                            using (MySqlCommand deleteCommand =
                                new MySqlCommand(
                                    deleteSql,
                                    connection,
                                    transaction))
                            {
                                deleteCommand.Parameters.AddWithValue(
                                    "@userId",
                                    userId);

                                deleteCommand.Parameters.AddWithValue(
                                    "@pharmacyId",
                                    pharmacyId);


                                deleteCommand.ExecuteNonQuery();
                            }


                            // Add new permissions.

                            foreach (string permission
                                in permissions)
                            {
                                if (string.IsNullOrWhiteSpace(permission))
                                    continue;


                                const string insertSql = @"
                                    INSERT INTO user_permissions
                                    (
                                        user_id,
                                        permission_name
                                    )
                                    VALUES
                                    (
                                        @userId,
                                        @permissionName
                                    )";


                                using (MySqlCommand insertCommand =
                                    new MySqlCommand(
                                        insertSql,
                                        connection,
                                        transaction))
                                {
                                    insertCommand.Parameters.AddWithValue(
                                        "@userId",
                                        userId);

                                    insertCommand.Parameters.AddWithValue(
                                        "@permissionName",
                                        permission.Trim());


                                    insertCommand.ExecuteNonQuery();
                                }
                            }


                            transaction.Commit();

                            return true;
                        }
                        catch
                        {
                            transaction.Rollback();
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "SET PERMISSIONS ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // CHECK PERMISSION
        // ============================================================

        public static bool HasPermission(
            int userId,
            string permission)
        {
            if (userId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(permission))
                return false;


            User? user =
                GetById(userId);

            if (user == null)
                return false;


            // Admin has full access.

            if (string.Equals(
                    user.Role,
                    "Admin",
                    StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }


            try
            {
                using (MySqlConnection connection =
                    DatabaseConnection.GetConnection())
                {
                    connection.Open();


                    const string sql = @"
                        SELECT COUNT(*)
                        FROM user_permissions up
                        INNER JOIN users u
                            ON u.id = up.user_id
                        WHERE
                            u.id = @userId
                            AND u.pharmacy_id = @pharmacyId
                            AND up.permission_name = @permission";


                    using (MySqlCommand command =
                        new MySqlCommand(
                            sql,
                            connection))
                    {
                        command.Parameters.AddWithValue(
                            "@userId",
                            userId);

                        command.Parameters.AddWithValue(
                            "@pharmacyId",
                            user.PharmacyId);

                        command.Parameters.AddWithValue(
                            "@permission",
                            permission.Trim());


                        int count =
                            Convert.ToInt32(
                                command.ExecuteScalar());


                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(
                    "CHECK PERMISSION ERROR: " +
                    ex.Message);

                return false;
            }
        }


        // ============================================================
        // MAP USER
        // ============================================================

        private static User MapUser(
            MySqlDataReader reader)
        {
            User user =
                new User
                {
                    Id =
                        Convert.ToInt32(
                            reader["id"]),

                    PharmacyId =
                        Convert.ToInt32(
                            reader["pharmacy_id"]),

                    Username =
                        reader["username"]?.ToString()
                        ?? "",

                    PasswordHash =
                        reader["password_hash"]?.ToString()
                        ?? "",

                    Role =
                        reader["role"]?.ToString()
                        ?? "Staff",

                    IsActive =
                        Convert.ToBoolean(
                            reader["is_active"]),

                    CreatedAt =
                        Convert.ToDateTime(
                            reader["created_at"])
                };


            if (HasColumn(
                    reader,
                    "pharmacy_name"))
            {
                user.PharmacyName =
                    reader["pharmacy_name"]?.ToString()
                    ?? "";
            }


            return user;
        }


        // ============================================================
        // CHECK COLUMN
        // ============================================================

        private static bool HasColumn(
            MySqlDataReader reader,
            string columnName)
        {
            for (int i = 0;
                 i < reader.FieldCount;
                 i++)
            {
                if (string.Equals(
                        reader.GetName(i),
                        columnName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }


            return false;
        }
    }
}