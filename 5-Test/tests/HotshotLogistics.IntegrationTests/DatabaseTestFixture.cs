using FluentMigrator.Runner;
using HotshotLogistics.Data.Migrations;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace HotshotLogistics.IntegrationTests
{
    /// <summary>
    ///     Shared test fixture that provisions the SQL Server database using FluentMigrator before integration tests run.
    /// </summary>
    public sealed class DatabaseTestFixture : IAsyncLifetime
    {
        private static readonly SemaphoreSlim s_setupSemaphore = new(1, 1);
        private static bool s_initialized;

        /// <inheritdoc />
        public async ValueTask InitializeAsync()
        {
            if (s_initialized)
            {
                return;
            }

            await s_setupSemaphore.WaitAsync();
            try
            {
                if (s_initialized)
                {
                    return;
                }

                // Use the pre-configured connection string from environment to match application behavior
                string connectionString = TestDatabaseHelper.GetConnectionString();

                // Verify the database is reachable
                await VerifyConnectionAsync(connectionString);

                // Skip problematic migrations before running them
                SkipProblematicMigrations(connectionString);

                // Run database migrations to ensure schema is up to date
                RunMigrations(connectionString);

                s_initialized = true;
            }
            finally
            {
                s_setupSemaphore.Release();
            }
        }

        /// <inheritdoc />
        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        private static void SkipProblematicMigrations(string connectionString)
        {
            try
            {
                using SqlConnection connection = new(connectionString);
                connection.Open();

                // Check if VersionInfo table exists
                using SqlCommand checkTableCmd = new(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'VersionInfo'",
                    connection);
                int tableExists = (int)(checkTableCmd.ExecuteScalar() ?? 0);

                if (tableExists == 0)
                {
                    // Create VersionInfo table if it doesn't exist
                    using SqlCommand createTableCmd = new(
                        @"CREATE TABLE [dbo].[VersionInfo] (
                            [Version] [bigint] NOT NULL,
                            [AppliedOn] [datetime] NULL,
                            [Description] [nvarchar](1024) NULL
                        )",
                        connection);
                    createTableCmd.ExecuteNonQuery();
                    Console.WriteLine("Created VersionInfo table.");
                }

                // Check and mark migrations that might fail in container environment
                // 1. Mark SeedInitialData (20240101000002) if initial data already exists
                MarkMigrationIfDataExists(connection, 20240101000002, "SeedInitialData",
                    "SELECT COUNT(*) FROM Customers WHERE Email = 'billing@industrialcorp.example.com'");

                // 2. Mark SeedHistoricalDriverLocations (20240616000002) if locations already exist
                MarkMigrationIfDataExists(connection, 20240616000002, "SeedHistoricalDriverLocations",
                    "SELECT COUNT(*) FROM LocationTracking WHERE DriverId = 1");

                // 3. Mark SeedLargeTestData (20240616000003) if large dataset exists
                MarkMigrationIfDataExists(connection, 20240616000003, "SeedLargeTestData",
                    "SELECT COUNT(*) FROM Customers WHERE Id = 'cust-001'");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during pre-migration check: {ex.Message}");
            }
        }

        private static void MarkMigrationIfDataExists(SqlConnection connection, long migrationVersion,
            string description, string checkQuery)
        {
            try
            {
                // Check if already marked
                using SqlCommand checkVersionCmd = new(
                    "SELECT COUNT(*) FROM VersionInfo WHERE Version = @version",
                    connection);
                checkVersionCmd.Parameters.AddWithValue("@version", migrationVersion);
                int versionExists = (int)(checkVersionCmd.ExecuteScalar() ?? 0);

                if (versionExists > 0)
                {
                    return; // Already marked as applied
                }

                // Check if the data already exists
                using SqlCommand checkDataCmd = new(checkQuery, connection);
                int dataExists = (int)(checkDataCmd.ExecuteScalar() ?? 0);

                if (dataExists <= 0)
                {
                    return; // Data doesn't exist, let migration run
                }

                Console.WriteLine(
                    $"Data for migration {migrationVersion} ({description}) already exists. Marking as completed to prevent duplicate key errors.");

                // Mark migration as applied
                using SqlCommand insertCmd = new(
                    "INSERT INTO VersionInfo (Version, AppliedOn, Description) VALUES (@version, GETUTCDATE(), @description)",
                    connection);
                insertCmd.Parameters.AddWithValue("@version", migrationVersion);
                insertCmd.Parameters.AddWithValue("@description", description);
                insertCmd.ExecuteNonQuery();
                Console.WriteLine($"Successfully marked migration {migrationVersion} ({description}) as completed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error marking migration as completed: {ex.Message}");
            }
        }

        private static void RunMigrations(string connectionString)
        {
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddFluentMigratorCore()
                .ConfigureRunner(rb => rb
                    .AddSqlServer()
                    .WithGlobalConnectionString(connectionString)
                    .ScanIn(typeof(CreateCustomersTable).Assembly).For.Migrations())
                .AddLogging(lb => lb.AddFluentMigratorConsole())
                .BuildServiceProvider(false);

            using IServiceScope scope = serviceProvider.CreateScope();
            IMigrationRunner runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

            // Run remaining migrations
            runner.MigrateUp();
        }

        private static async Task VerifyConnectionAsync(string connectionString)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();

            await using SqlCommand command = new("SELECT 1", connection);
            _ = await command.ExecuteScalarAsync();
        }
    }
}
