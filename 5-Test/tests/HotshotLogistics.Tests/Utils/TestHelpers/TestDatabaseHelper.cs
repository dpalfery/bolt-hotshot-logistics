using HotshotLogistics.Core.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace HotshotLogistics.Tests.Utils.TestHelpers
{
    /// <summary>
    ///     Resolves the test database connection string from Azure App Configuration when configured,
    ///     otherwise from .NET user secrets and environment variables.
    ///     Secrets are stored outside the repository via <c>dotnet user-secrets</c>.
    /// </summary>
    public static class TestDatabaseHelper
    {
        private static readonly object s_syncLock = new();
        private static IConfiguration? s_configuration;
        private static string? s_cachedConnectionString;

        /// <summary>
        ///     Gets a value indicating whether a test database connection is configured.
        /// </summary>
        public static bool IsConfigured => !string.IsNullOrWhiteSpace(TryGetConnectionString());

        /// <summary>
        ///     Gets the SQL Server connection string from user secrets or environment variables.
        /// </summary>
        /// <returns>A valid SQL Server connection string.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no connection string is configured.</exception>
        public static string GetConnectionString()
        {
            string? connectionString = TryGetConnectionString();
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

                IConfiguration config = GetConfiguration();
                string? connectionString = config.GetConnectionString("DefaultConnection");
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    connectionString = BuildConnectionStringFromParts(config);
                }

                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    s_cachedConnectionString = connectionString;
                }

                return s_cachedConnectionString;
            }
        }

        private static IConfiguration GetConfiguration()
        {
            if (s_configuration is not null)
            {
                return s_configuration;
            }

            IConfigurationBuilder configBuilder = new ConfigurationBuilder()
                .AddUserSecrets(typeof(Program).Assembly, true)
                .AddEnvironmentVariables();
            configBuilder.AddAzureAppConfigurationIfConfigured(false);
            s_configuration = configBuilder.Build();

            return s_configuration;
        }

        private static string? BuildConnectionStringFromParts(IConfiguration config)
        {
            string? server = config["HOTSHOT_DB_SERVER"];
            string? database = config["HOTSHOT_DB_NAME"];
            string? user = config["HOTSHOT_DB_APP_USER"];
            string? password = config["HOTSHOT_DB_PASSWORD"];

            if (string.IsNullOrWhiteSpace(server)
                || string.IsNullOrWhiteSpace(database)
                || string.IsNullOrWhiteSpace(user)
                || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            SqlConnectionStringBuilder builder = new()
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
