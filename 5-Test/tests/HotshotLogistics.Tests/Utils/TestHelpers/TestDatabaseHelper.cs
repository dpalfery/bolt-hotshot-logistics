using Microsoft.Extensions.Configuration;
using HotshotLogistics.Api;
using HotshotLogistics.Core.Extensions;

namespace HotshotLogistics.Tests
{
    /// <summary>
    /// Resolves the test database connection string from Azure App Configuration when configured,
    /// otherwise from .NET user secrets and environment variables.
    /// Secrets are stored outside the repository via <c>dotnet user-secrets</c>.
    /// </summary>
    public static class TestDatabaseHelper
    {
        private static readonly object SyncLock = new();
        private static IConfiguration? configuration;
        private static string? cachedConnectionString;

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
                    "(dotnet user-secrets set --project 1-Presentation/HotshotLogistics.Api/HotshotLogistics.Api.csproj " +
                    "\"ConnectionStrings:DefaultConnection\" \"<connection-string>\"), " +
                    "or set CONNECTIONSTRINGS__DEFAULTCONNECTION.");
            }

            return connectionString;
        }

        private static string? TryGetConnectionString()
        {
            if (cachedConnectionString is not null)
            {
                return cachedConnectionString;
            }

            lock (SyncLock)
            {
                if (cachedConnectionString is not null)
                {
                    return cachedConnectionString;
                }

                var config = GetConfiguration();
                var connectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = BuildConnectionStringFromParts(config);
                }

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    cachedConnectionString = connectionString;
                }

                return cachedConnectionString;
            }
        }

        private static IConfiguration GetConfiguration()
        {
            if (configuration is not null)
            {
                return configuration;
            }

            var configBuilder = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, optional: true)
                .AddEnvironmentVariables();
            configBuilder.AddAzureAppConfigurationIfConfigured(useDefaultAzureCredential: false);
            configuration = configBuilder.Build();

            return configuration;
        }

        private static string? BuildConnectionStringFromParts(IConfiguration config)
        {
            var server = config["HOTSHOT_DB_SERVER"];
            var database = config["HOTSHOT_DB_NAME"];
            var user = config["HOTSHOT_DB_APP_USER"];
            var password = config["HOTSHOT_DB_PASSWORD"];

            if (string.IsNullOrWhiteSpace(server)
                || string.IsNullOrWhiteSpace(database)
                || string.IsNullOrWhiteSpace(user)
                || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder
            {
                DataSource = server,
                InitialCatalog = database,
                UserID = user,
                Password = password,
                TrustServerCertificate = true,
                MultipleActiveResultSets = true
            };

            return builder.ConnectionString;
        }
    }
}
