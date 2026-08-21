using Microsoft.Data.SqlClient;

namespace MigrationRunner
{
    internal static class SkipMigration
    {
        public static void MarkAsCompleted(long migrationVersion, string description)
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

                // Check if migration already exists
                using SqlCommand checkCmd = connection.CreateCommand();
                checkCmd.CommandText = "SELECT COUNT(*) FROM [dbo].[VersionInfo] WHERE Version = @Version";
                checkCmd.Parameters.AddWithValue("@Version", migrationVersion);

                bool exists = (int)checkCmd.ExecuteScalar() > 0;
                if (exists)
                {
                    Console.WriteLine($"Migration {migrationVersion} already marked as completed.");
                    return;
                }

                // Insert migration record to mark it as completed
                using SqlCommand insertCmd = connection.CreateCommand();
                insertCmd.CommandText = @"
                INSERT INTO [dbo].[VersionInfo] (Version, AppliedOn, Description)
                VALUES (@Version, @AppliedOn, @Description)";

                insertCmd.Parameters.AddWithValue("@Version", migrationVersion);
                insertCmd.Parameters.AddWithValue("@AppliedOn", DateTime.UtcNow);
                insertCmd.Parameters.AddWithValue("@Description", description);

                insertCmd.ExecuteNonQuery();
                Console.WriteLine($"Successfully marked migration {migrationVersion} ({description}) as completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking migration as completed: {ex.Message}");
            }
        }
    }
}
