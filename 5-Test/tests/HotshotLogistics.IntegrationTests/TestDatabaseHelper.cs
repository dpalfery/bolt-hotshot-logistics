using Microsoft.Extensions.Configuration;
using HotshotLogistics.Core.Extensions;

namespace HotshotLogistics.IntegrationTests
{
    /// <summary>
    /// Resolves the test database connection string from Azure App Configuration when configured,
    /// otherwise from .NET user secrets and environment variables.
    /// </summary>
    public static class TestDatabaseHelper
    {
        private static readonly object s_syncLock = new();
        private static string? s_cachedConnectionString;

        /// <summary>
        /// Gets a value indicating whether a test database connection is configured.
        /// </summary>
        public static bool IsConfigured => !string.IsNullOrWhiteSpace(TryGetConnectionString());

        /// <summary>
        /// Gets the SQL Server connection string from user secrets or environment variables.
        /// </summary>
        /// <returns>A valid SQL Server connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no connection string is configured.</exception>
        public static string GetConnectionString()
        {
            var connectionString = TryGetConnectionString();
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "A database connection string is required for tests. Set user secret 'ConnectionStrings:DefaultConnection' " +
                    "or CONNECTIONSTRINGS__DEFAULTCONNECTION / DB_CONNECTION_STRING.");
            }

            return connectionString;
        }

        private static string? TryGetConnectionString()
        {
            if (s_cachedConnectionString is not null)
            {
                return s_cachedConnectionString;
            }

            lock (s_syncLock)
            {
                if (s_cachedConnectionString is not null)
                {
                    return s_cachedConnectionString;
                }

                var configBuilder = new ConfigurationBuilder()
                    .AddUserSecrets(typeof(Program).Assembly, optional: true)
                    .AddEnvironmentVariables();
                configBuilder.AddAzureAppConfigurationIfConfigured(useDefaultAzureCredential: false);
                var config = configBuilder.Build();

                var connectionString = config.GetConnectionString("DefaultConnection")
                    ?? config["DB_CONNECTION_STRING"];

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    s_cachedConnectionString = connectionString;
                }

                return s_cachedConnectionString;
            }
        }
    }
}
