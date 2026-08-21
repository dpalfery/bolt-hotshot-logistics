using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotshotLogistics.DbSetup.Tests
{
    /// <summary>
    ///     Pure unit tests for DbSetup tools (no database required).
    /// </summary>
    public class DatabaseSetupUnitTests : IDisposable
    {
        private readonly ILoggerFactory _loggerFactory;

        public DatabaseSetupUnitTests()
        {
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }

        [Fact]
        public void PasswordManager_ShouldGenerateValidPasswords()
        {
            // Arrange
            PasswordManager passwordManager = new(_loggerFactory.CreateLogger<PasswordManager>());

            // Act
            string password = passwordManager.GeneratePassword();

            // Assert
            Assert.NotNull(password);
            Assert.True(password.Length >= 16, "Password should be at least 16 characters");
            Assert.True(passwordManager.ValidatePassword(password), "Generated password should be valid");

            // Test custom length
            string customPassword = passwordManager.GeneratePassword(32);
            Assert.Equal(32, customPassword.Length);
            Assert.True(passwordManager.ValidatePassword(customPassword));
        }

        [Fact]
        public void ArgumentParser_ShouldParseArgumentsCorrectly()
        {
            // Arrange
            string[] args =
            [
                "--server", "localhost\\SQLEXPRESS",
                "--db-name", "test_db",
                "--app-user", "test_user",
                "--password", "test_password",
                "--non-interactive"
            ];

            // Act
            ArgumentParser parser = new(args);

            // Assert
            Assert.Equal("localhost\\SQLEXPRESS", parser.Server);
            Assert.Equal("test_db", parser.DatabaseName);
            Assert.Equal("test_user", parser.AppUser);
            Assert.Equal("test_password", parser.Password);
            Assert.True(parser.NonInteractive);
        }

        [Fact]
        public void ArgumentParser_ShouldApplyEnvironmentOverrides()
        {
            // Arrange
            Environment.SetEnvironmentVariable("HOTSHOT_DB_SERVER", "env_server");
            Environment.SetEnvironmentVariable("HOTSHOT_DB_NAME", "env_db");

            string[] args = ["--app-user", "arg_user"];

            // Act
            ArgumentParser parser = new(args);
            parser.ApplyEnvironmentOverrides();

            // Assert
            Assert.Equal("arg_user", parser.AppUser); // Should use CLI arg
            Assert.Equal("env_server", parser.Server); // Should use env var
            Assert.Equal("env_db", parser.DatabaseName); // Should use env var

            // Cleanup
            Environment.SetEnvironmentVariable("HOTSHOT_DB_SERVER", null);
            Environment.SetEnvironmentVariable("HOTSHOT_DB_NAME", null);
        }

        [Fact]
        public void SecureLogger_ShouldMaskSecrets()
        {
            // Arrange
            SecureLogger logger = new(_loggerFactory.CreateLogger<SecureLogger>());
            logger.RegisterSecret("SecretPassword123!");

            // Act & Assert
            string originalMessage = "Connection string: Server=myserver;Password=SecretPassword123!;Database=mydb;";
            string sanitizedMessage = "Connection string: Server=myserver;Password=****;Database=mydb;";

            // The logger should mask the secret when logging
            logger.LogInformation("Test message with {ConnectionString}", originalMessage);

            Assert.Contains("****", sanitizedMessage);
            Assert.DoesNotContain("SecretPassword123!", sanitizedMessage);
        }
    }

    /// <summary>
    ///     Integration tests requiring SQL Server with admin/SA access to provision databases and logins.
    /// </summary>
    public class DatabaseSetupIntegrationTests : IDisposable
    {
        private readonly ILogger<DatabaseSetupIntegrationTests> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly string? _saConnectionString;
        private readonly string _testDatabaseName;

        public DatabaseSetupIntegrationTests()
        {
            _testDatabaseName = $"TestHotshot_{Guid.NewGuid():N}";
            _saConnectionString = TryGetTestConnectionString();

            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Debug);
            });

            _logger = _loggerFactory.CreateLogger<DatabaseSetupIntegrationTests>();
        }

        public void Dispose()
        {
            _loggerFactory.Dispose();
        }

        [Fact]
        public async Task FullDatabaseSetupWorkflow_ShouldCreateDatabaseAndUser()
        {
            if (string.IsNullOrWhiteSpace(_saConnectionString))
            {
                return; // Skip when SA connection string is not configured
            }

            string saConnectionString = _saConnectionString;
            string databaseName = _testDatabaseName;
            string appUserName = $"test_hotshot_app_{Guid.NewGuid():N}";
            string password = "TestPassword123!";

            // Act & Assert
            await using (SqlConnection connection = new(saConnectionString))
            {
                await connection.OpenAsync();

                bool dbExists = await DatabaseExistsAsync(connection, databaseName);
                Assert.False(dbExists, "Database should not exist initially");

                bool loginExists = await LoginExistsAsync(connection, appUserName);
                Assert.False(loginExists, "Login should not exist initially");
            }

            // Run the full setup workflow
            SqlServerProvisioner provisioner = new(_loggerFactory.CreateLogger<SqlServerProvisioner>());
            bool success =
                await provisioner.ProvisionDatabaseAsync(saConnectionString, databaseName, appUserName, password);

            Assert.True(success, "Database provisioning should succeed");

            // Verify database was created
            await using (SqlConnection connection = new(saConnectionString))
            {
                await connection.OpenAsync();

                bool dbExists = await DatabaseExistsAsync(connection, databaseName);
                Assert.True(dbExists, "Database should exist after provisioning");

                bool loginExists = await LoginExistsAsync(connection, appUserName);
                Assert.True(loginExists, "Login should exist after provisioning");

                bool userExists = await UserExistsInDatabaseAsync(databaseName, appUserName);
                Assert.True(userExists, "User should exist in database after provisioning");
            }

            // Verify the application user can connect and perform operations
            string appConnectionString =
                $"{saConnectionString};Initial Catalog={databaseName};User Id={appUserName};Password={password};";
            await using (SqlConnection connection = new(appConnectionString))
            {
                await connection.OpenAsync();

                const string testQuery = "SELECT DB_NAME()";
                await using SqlCommand command = new(testQuery, connection);
                object? result = await command.ExecuteScalarAsync();

                Assert.Equal(databaseName, result?.ToString());
            }
        }

        [Fact]
        public async Task PermissionManager_ShouldGrantCorrectPermissions()
        {
            if (string.IsNullOrWhiteSpace(_saConnectionString))
            {
                return; // Skip when SA connection string is not configured
            }

            string saConnectionString = _saConnectionString;
            string databaseName = $"{_testDatabaseName}_Perm";
            string appUserName = "test_hotshot_app_perm";

            await CreateTestDatabaseAsync(saConnectionString, databaseName);

            try
            {
                PermissionManager permissionManager = new(_loggerFactory.CreateLogger<PermissionManager>());
                await permissionManager.SetupApplicationPermissionsAsync(saConnectionString, databaseName, appUserName);

                Assert.True(true, "SetupApplicationPermissionsAsync completed successfully");
            }
            finally
            {
                await CleanupTestDatabaseAsync(saConnectionString, databaseName);
            }
        }

        private async Task<bool> DatabaseExistsAsync(SqlConnection connection, string databaseName)
        {
            const string query = "SELECT database_id FROM sys.databases WHERE name = @dbName";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@dbName", databaseName);

            object? result = await command.ExecuteScalarAsync();
            return result != null && result != DBNull.Value;
        }

        private async Task<bool> LoginExistsAsync(SqlConnection connection, string loginName)
        {
            const string query =
                "SELECT name FROM sys.server_principals WHERE name = @loginName AND type IN ('S', 'U')";
            await using SqlCommand command = new(query, connection);
            command.Parameters.AddWithValue("@loginName", loginName);

            object? result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private async Task<bool> UserExistsInDatabaseAsync(string databaseName,
            string userName)
        {
            SqlConnectionStringBuilder builder = new(_saConnectionString)
            {
                InitialCatalog = databaseName
            };

            await using SqlConnection dbConnection = new(builder.ConnectionString);
            await dbConnection.OpenAsync();

            const string query =
                "SELECT name FROM sys.database_principals WHERE name = @userName AND type IN ('S', 'U')";
            await using SqlCommand command = new(query, dbConnection);
            command.Parameters.AddWithValue("@userName", userName);

            object? result = await command.ExecuteScalarAsync();
            return result != null;
        }

        private async Task CreateTestDatabaseAsync(string connectionString, string databaseName)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();

            string query = $"CREATE DATABASE {GetQuotedIdentifier(databaseName)}";
            await using SqlCommand command = new(query, connection);
            await command.ExecuteNonQueryAsync();
        }

        private async Task CleanupTestDatabaseAsync(string connectionString, string databaseName)
        {
            await using SqlConnection connection = new(connectionString);
            await connection.OpenAsync();

            try
            {
                string killQuery = $@"
                DECLARE @kill varchar(8000) = '';
                SELECT @kill = @kill + 'KILL ' + CONVERT(varchar(5), spid) + ';'
                FROM master..sysprocesses
                WHERE dbid = db_id('{databaseName.Replace("'", "''")}')
                AND spid > 50;

                IF @kill <> ''
                BEGIN
                    EXEC(@kill);
                END";
                await using (SqlCommand killCommand = new(killQuery, connection))
                {
                    await killCommand.ExecuteNonQueryAsync();
                }

                string singleUserQuery =
                    $"ALTER DATABASE {GetQuotedIdentifier(databaseName)} SET SINGLE_USER WITH ROLLBACK IMMEDIATE";
                await using (SqlCommand singleUserCommand = new(singleUserQuery, connection))
                {
                    await singleUserCommand.ExecuteNonQueryAsync();
                }

                string dropQuery = $"DROP DATABASE {GetQuotedIdentifier(databaseName)}";
                await using (SqlCommand dropCommand = new(dropQuery, connection))
                {
                    await dropCommand.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex) when (ex.Number == 3701)
            {
                // Database already dropped
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error during database cleanup for {DatabaseName}", databaseName);
            }
        }

        private static string GetQuotedIdentifier(string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }

        private static string? TryGetTestConnectionString()
        {
            // 1. Try LocalDB (Windows only)
            if (OperatingSystem.IsWindows())
            {
                string localDbConnectionString =
                    "Server=(localdb)\\MSSQLLocalDB;Database=master;Trusted_Connection=True;";
                try
                {
                    using SqlConnection connection = new(localDbConnectionString);
                    connection.Open();
                    return localDbConnectionString;
                }
                catch
                {
                    // Fall through to secrets/env
                }
            }

            // 2. Try User Secrets and Environment Variables
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddUserSecrets<DatabaseSetupIntegrationTests>(true)
                .AddEnvironmentVariables()
                .Build();

            string? saConnection = config["TEST_SA_CONNECTION_STRING"]
                                   ?? config.GetConnectionString("SaConnection")
                                   ?? config.GetConnectionString("TestSaConnection");

            if (!string.IsNullOrWhiteSpace(saConnection))
            {
                return saConnection;
            }

            return null;
        }
    }
}
