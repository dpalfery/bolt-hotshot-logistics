using Microsoft.Data.SqlClient;

namespace HotshotLogistics.Tests.Utils.TestHelpers
{
    /// <summary>
    ///     Shared test fixture that provisions the SQL Server database using the DbSetup CLI before integration tests run.
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

                if (!TestDatabaseHelper.IsConfigured)
                {
                    return;
                }

                // Use the pre-configured connection string from environment to match application behavior
                string connectionString = TestDatabaseHelper.GetConnectionString();

                // Verify the database is reachable
                await VerifyConnectionAsync(connectionString);

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

        private static async Task VerifyConnectionAsync(string connectionString)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();

            await using SqlCommand command = new("SELECT 1", connection);
            _ = await command.ExecuteScalarAsync();
        }
    }
}
