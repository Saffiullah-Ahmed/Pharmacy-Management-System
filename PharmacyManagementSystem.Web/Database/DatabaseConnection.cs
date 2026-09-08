using MySql.Data.MySqlClient;

namespace PharmacyManagementSystem.Web.Database
{
    public static class DatabaseConnection
    {
        private static string connectionString = string.Empty;

        public static void Initialize(string connection)
        {
            connectionString = connection;
        }

        public static MySqlConnection GetConnection()
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Database connection has not been initialized."
                );
            }

            return new MySqlConnection(connectionString);
        }
    }
}