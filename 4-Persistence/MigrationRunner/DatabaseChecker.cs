using Microsoft.Data.SqlClient;

namespace MigrationRunner
{
    internal static class DatabaseChecker
    {
        public static void CheckCustomers()
        {
            string? connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");
            if (string.IsNullOrEmpty(connectionString))
            {
                Console.WriteLine("DB_CONNECTION_STRING environment variable not found.");
                return;
            }

            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                // Check customers with cust- prefix
                using SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = @"
                SELECT TOP 10 Id, CompanyName
                FROM [dbo].[Customers]
                WHERE Id LIKE 'cust-%'
                ORDER BY Id";

                Console.WriteLine("Customers with 'cust-' prefix:");
                using SqlDataReader reader = cmd.ExecuteReader();
                bool hasCustomers = false;
                while (reader.Read())
                {
                    hasCustomers = true;
                    Console.WriteLine($"  {reader["Id"]} - {reader["CompanyName"]}");
                }

                if (!hasCustomers)
                {
                    Console.WriteLine("  No customers found with 'cust-' prefix");
                }

                reader.Close();

                // Check total customer count
                cmd.CommandText = "SELECT COUNT(*) FROM [dbo].[Customers]";
                int totalCount = (int)cmd.ExecuteScalar();
                Console.WriteLine($"Total customers in database: {totalCount}");

                // Check for any customers at all
                if (totalCount > 0)
                {
                    cmd.CommandText = "SELECT TOP 5 Id, CompanyName FROM [dbo].[Customers] ORDER BY Id";
                    using SqlDataReader reader2 = cmd.ExecuteReader();
                    Console.WriteLine("Sample customers (any pattern):");
                    while (reader2.Read())
                    {
                        Console.WriteLine($"  {reader2["Id"]} - {reader2["CompanyName"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking database: {ex.Message}");
            }
        }
    }
}
